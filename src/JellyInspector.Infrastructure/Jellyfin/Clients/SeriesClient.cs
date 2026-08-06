using JellyInspector.Application.Models;

namespace JellyInspector.Infrastructure.Jellyfin.Clients;

public sealed class SeriesClient : ISeriesClient
{
    private const int PageSize = 200;

    private readonly IJellyfinApiClient _api;
    private readonly IUserClient _userClient;

    public SeriesClient(
        IJellyfinApiClient api,
        IUserClient userClient)
    {
        _api = api;
        _userClient = userClient;
    }

    public async Task<IReadOnlyList<SeriesInfo>> GetSeriesAsync(
        IEnumerable<string> libraryIds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var selectedLibraryIds = libraryIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (selectedLibraryIds.Count == 0)
            {
                return [];
            }

            var userId =
                await _userClient.GetCurrentUserIdAsync();

            if (string.IsNullOrWhiteSpace(userId))
            {
                return [];
            }

            var allSeries = new List<SeriesInfo>();

            foreach (var libraryId in selectedLibraryIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var librarySeries =
                    await GetSeriesFromLibraryAsync(
                        userId,
                        libraryId,
                        cancellationToken);

                allSeries.AddRange(librarySeries);
            }

            return allSeries
                .DistinctBy(
                    item => item.Id,
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(item => item.Name)
                .ThenBy(item => item.ProductionYear)
                .ToList();
        }
        catch (InvalidOperationException)
        {
            return [];
        }
    }

    public async Task<SeriesDetail?> GetSeriesDetailAsync(
        string seriesId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(seriesId))
        {
            return null;
        }

        try
        {
            var userId =
                await _userClient.GetCurrentUserIdAsync();

            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            var escapedSeriesId =
                Uri.EscapeDataString(seriesId);

            var escapedUserId =
                Uri.EscapeDataString(userId);

            var seriesEndpoint =
                $"Users/{escapedUserId}/Items/" +
                $"{escapedSeriesId}" +
                "?Fields=Overview,ProductionYear," +
                "ImageTags,ProviderIds";

            var seriesItem =
                await _api.GetAsync<JellyfinItemResponse>(
                    seriesEndpoint,
                    cancellationToken);

            if (seriesItem is null)
            {
                return null;
            }

            var seasonsEndpoint =
                $"Shows/{escapedSeriesId}/Seasons" +
                $"?UserId={escapedUserId}" +
                "&Fields=Overview,IndexNumber,ImageTags" +
                "&EnableImages=true" +
                "&ImageTypeLimit=1" +
                "&Limit=10000";

            var seasonsResult =
                await _api.GetAsync<JellyfinItemsResponse>(
                    seasonsEndpoint,
                    cancellationToken);

            if (seasonsResult?.Items is null)
            {
                return null;
            }

            var seasons = new List<SeasonInfo>();

            foreach (var season in seasonsResult.Items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(season.Id))
                {
                    continue;
                }

                var episodeCount =
                    await GetSeasonEpisodeCountAsync(
                        userId,
                        seriesId,
                        season.Id,
                        cancellationToken);

                seasons.Add(new SeasonInfo
                {
                    Id = season.Id,
                    Name =
                        season.Name ?? string.Empty,
                    IndexNumber =
                        season.IndexNumber ?? 0,
                    EpisodeCount =
                        episodeCount,
                    ImageTag =
                        GetPrimaryImageTag(season)
                });
            }

            return new SeriesDetail
            {
                Id =
                    seriesItem.Id ?? seriesId,

                Name =
                    seriesItem.Name ?? string.Empty,

                ProductionYear =
                    seriesItem.ProductionYear ?? 0,

                Overview =
                    seriesItem.Overview,

                ImageTag =
                    GetPrimaryImageTag(seriesItem),

                TmdbId =
                    GetProviderId(seriesItem, "Tmdb"),

                Seasons = seasons
                    .OrderBy(season => season.IndexNumber)
                    .ThenBy(season => season.Name)
                    .ToList()
            };
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private async Task<IReadOnlyList<SeriesInfo>>
        GetSeriesFromLibraryAsync(
            string userId,
            string libraryId,
            CancellationToken cancellationToken)
    {
        var escapedUserId =
            Uri.EscapeDataString(userId);

        var escapedLibraryId =
            Uri.EscapeDataString(libraryId);

        var series = new List<SeriesInfo>();
        var startIndex = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var endpoint =
                $"Users/{escapedUserId}/Items" +
                $"?ParentId={escapedLibraryId}" +
                "&IncludeItemTypes=Series" +
                "&Recursive=true" +
                "&Fields=Overview,ProductionYear," +
                "PrimaryImageAspectRatio,ImageTags,ProviderIds" +
                "&EnableImages=true" +
                "&ImageTypeLimit=1" +
                $"&StartIndex={startIndex}" +
                $"&Limit={PageSize}";

            var result =
                await _api.GetAsync<JellyfinItemsResponse>(
                    endpoint,
                    cancellationToken);

            if (result?.Items is null)
            {
                break;
            }

            foreach (var item in result.Items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(item.Id))
                {
                    continue;
                }

                var counts =
                    await GetSeriesCountsAsync(
                        userId,
                        item.Id,
                        cancellationToken);

                series.Add(new SeriesInfo
                {
                    Id = item.Id,
                    Name = item.Name ?? string.Empty,
                    LibraryName = libraryId,
                    ProductionYear =
                        item.ProductionYear ?? 0,
                    SeasonCount =
                        counts.SeasonCount,
                    EpisodeCount =
                        counts.EpisodeCount,
                    Overview =
                        item.Overview,
                    ImageTag =
                        GetPrimaryImageTag(item),
                    TmdbId =
                        GetProviderId(item, "Tmdb"),
                    TvdbId =
                        GetProviderId(item, "Tvdb"),
                    ImdbId =
                        GetProviderId(item, "Imdb")
                });
            }

            startIndex += result.Items.Count;

            if (result.Items.Count == 0 ||
                startIndex >= result.TotalRecordCount)
            {
                break;
            }
        }

        return series;
    }

    private async Task<int> GetSeasonEpisodeCountAsync(
        string userId,
        string seriesId,
        string seasonId,
        CancellationToken cancellationToken)
    {
        var endpoint =
            $"Shows/{Uri.EscapeDataString(seriesId)}/Episodes" +
            $"?UserId={Uri.EscapeDataString(userId)}" +
            $"&SeasonId={Uri.EscapeDataString(seasonId)}" +
            "&Fields=IndexNumber,SeasonId" +
            "&Limit=10000";

        var result =
            await _api.GetAsync<JellyfinItemsResponse>(
                endpoint,
                cancellationToken);

        if (result?.Items is null)
        {
            return 0;
        }

        return result.Items
            .Where(item =>
                !string.IsNullOrWhiteSpace(item.Id))
            .Where(item =>
                item.IndexNumber is > 0)
            .DistinctBy(
                item => item.Id,
                StringComparer.OrdinalIgnoreCase)
            .Count();
    }

    private async Task<(int SeasonCount, int EpisodeCount)>
        GetSeriesCountsAsync(
            string userId,
            string seriesId,
            CancellationToken cancellationToken)
    {
        var escapedUserId =
            Uri.EscapeDataString(userId);

        var escapedSeriesId =
            Uri.EscapeDataString(seriesId);

        var seasonsEndpoint =
            $"Shows/{escapedSeriesId}/Seasons" +
            $"?UserId={escapedUserId}" +
            "&Fields=IndexNumber" +
            "&Limit=10000";

        var seasonsResult =
            await _api.GetAsync<JellyfinItemsResponse>(
                seasonsEndpoint,
                cancellationToken);

        if (seasonsResult?.Items is null)
        {
            return (0, 0);
        }

        var seasons = seasonsResult.Items
            .Where(item =>
                !string.IsNullOrWhiteSpace(item.Id))
            .DistinctBy(
                item => item.Id,
                StringComparer.OrdinalIgnoreCase)
            .ToList();

        var episodeCount = 0;

        foreach (var season in seasons)
        {
            cancellationToken.ThrowIfCancellationRequested();

            episodeCount +=
                await GetSeasonEpisodeCountAsync(
                    userId,
                    seriesId,
                    season.Id!,
                    cancellationToken);
        }

        return (
            seasons.Count,
            episodeCount);
    }

    private static string? GetPrimaryImageTag(
        JellyfinItemResponse item)
    {
        if (item.ImageTags is null)
        {
            return null;
        }

        return item.ImageTags.TryGetValue(
            "Primary",
            out var imageTag)
            ? imageTag
            : null;
    }

    private static string? GetProviderId(
        JellyfinItemResponse item,
        string provider)
    {
        if (item.ProviderIds is null)
        {
            return null;
        }

        return item.ProviderIds.TryGetValue(
            provider,
            out var value)
            ? value
            : null;
    }

    private sealed class JellyfinItemsResponse
    {
        public List<JellyfinItemResponse> Items { get; set; } = [];

        public int TotalRecordCount { get; set; }
    }

    private sealed class JellyfinItemResponse
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? Type { get; set; }

        public string? SeasonId { get; set; }

        public string? CollectionType { get; set; }

        public int? ProductionYear { get; set; }

        public int? IndexNumber { get; set; }

        public string? Overview { get; set; }

        public Dictionary<string, string>? ImageTags { get; set; }

        public Dictionary<string, string>? ProviderIds { get; set; }
    }
}
