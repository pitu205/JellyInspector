using JellyInspector.Application.Models;

namespace JellyInspector.Infrastructure.Jellyfin.Clients;

public sealed class EpisodeClient : IEpisodeClient
{
    private const long TicksPerSecond = 10_000_000;

    private readonly IJellyfinApiClient _api;
    private readonly IUserClient _userClient;

    public EpisodeClient(
        IJellyfinApiClient api,
        IUserClient userClient)
    {
        _api = api;
        _userClient = userClient;
    }

    public async Task<IReadOnlyList<EpisodeInfo>> GetEpisodesAsync(
        string seriesId,
        string seasonId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(seriesId) ||
            string.IsNullOrWhiteSpace(seasonId))
        {
            return [];
        }

        var userId =
            await _userClient.GetCurrentUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return [];
        }

        var escapedSeriesId =
            Uri.EscapeDataString(seriesId);

        var escapedSeasonId =
            Uri.EscapeDataString(seasonId);

        var escapedUserId =
            Uri.EscapeDataString(userId);

        var endpoint =
            $"Shows/{escapedSeriesId}/Episodes" +
            $"?UserId={escapedUserId}" +
            $"&SeasonId={escapedSeasonId}" +
            "&Fields=MediaSources,MediaStreams,Path,Overview," +
            "RunTimeTicks,IndexNumber,ParentIndexNumber,SeasonId" +
            "&EnableImages=true" +
            "&ImageTypeLimit=1" +
            "&Limit=10000";

        var response =
            await _api.GetAsync<JellyfinItemsResponse>(
                endpoint,
                cancellationToken);

        if (response?.Items is null)
        {
            return [];
        }

        return response.Items
            .Where(item =>
                !string.IsNullOrWhiteSpace(item.Id))
            .Where(item =>
                item.IndexNumber is > 0)
            .DistinctBy(
                item => item.Id,
                StringComparer.OrdinalIgnoreCase)
            .Select(MapEpisode)
            .OrderBy(episode => episode.SeasonNumber)
            .ThenBy(episode => episode.EpisodeNumber)
            .ThenBy(episode => episode.Name)
            .ToList();
    }

    private static EpisodeInfo MapEpisode(
        JellyfinEpisodeResponse episode)
    {
        var mediaSource =
            episode.MediaSources?.FirstOrDefault();

        var streams =
            mediaSource?.MediaStreams
            ?? episode.MediaStreams
            ?? [];

        var videoStream = streams.FirstOrDefault(stream =>
            string.Equals(
                stream.Type,
                "Video",
                StringComparison.OrdinalIgnoreCase));

        var audioStreams = streams
            .Where(stream =>
                string.Equals(
                    stream.Type,
                    "Audio",
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        var subtitleStreams = streams
            .Where(stream =>
                string.Equals(
                    stream.Type,
                    "Subtitle",
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        var audioLanguages = audioStreams
            .Select(stream => stream.Language)
            .Where(language =>
                !string.IsNullOrWhiteSpace(language))
            .Select(language => language!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(language => language)
            .ToList();

        var subtitleLanguages = subtitleStreams
            .Select(stream => stream.Language)
            .Where(language =>
                !string.IsNullOrWhiteSpace(language))
            .Select(language => language!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(language => language)
            .ToList();

        var runtimeTicks =
            episode.RunTimeTicks
            ?? mediaSource?.RunTimeTicks
            ?? 0;

        var videoRange =
            videoStream?.VideoRangeType
            ?? videoStream?.VideoRange
            ?? string.Empty;

        return new EpisodeInfo
        {
            Id = episode.Id ?? string.Empty,
            Name = episode.Name ?? string.Empty,
            SeasonNumber = episode.ParentIndexNumber ?? 0,
            EpisodeNumber = episode.IndexNumber ?? 0,

            Runtime = runtimeTicks > 0
                ? TimeSpan.FromSeconds(
                    runtimeTicks / (double)TicksPerSecond)
                : TimeSpan.Zero,

            Resolution = GetResolution(
                videoStream?.Width,
                videoStream?.Height),

            VideoCodec =
                videoStream?.Codec ?? string.Empty,

            AudioCodec = string.Join(
                ", ",
                audioStreams
                    .Select(stream => stream.Codec)
                    .Where(codec =>
                        !string.IsNullOrWhiteSpace(codec))
                    .Select(codec => codec!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(codec => codec)),

            AudioLanguages = audioLanguages,

            SubtitleLanguages = subtitleLanguages,

            FileSize =
                mediaSource?.Size ?? 0,

            Bitrate =
                mediaSource?.Bitrate
                ?? videoStream?.BitRate
                ?? 0,

            HasHdr =
                IsHdr(videoRange),

            HasDolbyVision =
                IsDolbyVision(
                    videoRange,
                    videoStream?.Profile,
                    videoStream?.DisplayTitle)
        };
    }

    private static string GetResolution(
        int? width,
        int? height)
    {
        if (height is null or <= 0)
        {
            return string.Empty;
        }

        return height.Value switch
        {
            >= 2160 => "2160p",
            >= 1440 => "1440p",
            >= 1080 => "1080p",
            >= 720 => "720p",
            >= 576 => "576p",
            >= 480 => "480p",
            _ => $"{width ?? 0}x{height.Value}"
        };
    }

    private static bool IsHdr(string videoRange)
    {
        if (string.IsNullOrWhiteSpace(videoRange))
        {
            return false;
        }

        return !videoRange.Equals(
                   "SDR",
                   StringComparison.OrdinalIgnoreCase)
               &&
               !videoRange.Equals(
                   "Unknown",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDolbyVision(
        string? videoRange,
        string? profile,
        string? displayTitle)
    {
        return ContainsDolbyVision(videoRange)
               || ContainsDolbyVision(profile)
               || ContainsDolbyVision(displayTitle);
    }

    private static bool ContainsDolbyVision(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Contains(
                   "Dolby Vision",
                   StringComparison.OrdinalIgnoreCase)
               ||
               value.Contains(
                   "DolbyVision",
                   StringComparison.OrdinalIgnoreCase)
               ||
               value.Contains(
                   "DOVI",
                   StringComparison.OrdinalIgnoreCase);
    }

    private sealed class JellyfinItemsResponse
    {
        public List<JellyfinEpisodeResponse> Items { get; set; } = [];
    }

    private sealed class JellyfinEpisodeResponse
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? SeasonId { get; set; }

        public int? IndexNumber { get; set; }

        public int? ParentIndexNumber { get; set; }

        public long? RunTimeTicks { get; set; }

        public string? Path { get; set; }

        public List<JellyfinMediaSourceResponse>? MediaSources { get; set; }

        public List<JellyfinMediaStreamResponse>? MediaStreams { get; set; }
    }

    private sealed class JellyfinMediaSourceResponse
    {
        public string? Path { get; set; }

        public string? Container { get; set; }

        public long? Size { get; set; }

        public long? RunTimeTicks { get; set; }

        public long? Bitrate { get; set; }

        public List<JellyfinMediaStreamResponse>? MediaStreams { get; set; }
    }

    private sealed class JellyfinMediaStreamResponse
    {
        public string? Type { get; set; }

        public string? Codec { get; set; }

        public string? Language { get; set; }

        public string? Profile { get; set; }

        public string? DisplayTitle { get; set; }

        public string? VideoRange { get; set; }

        public string? VideoRangeType { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public long? BitRate { get; set; }

        public bool IsForced { get; set; }

        public bool IsDefault { get; set; }

        public bool IsHearingImpaired { get; set; }
    }
}