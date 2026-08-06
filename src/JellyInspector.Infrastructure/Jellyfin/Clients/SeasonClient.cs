using JellyInspector.Application.Models;

namespace JellyInspector.Infrastructure.Jellyfin.Clients;

public sealed class SeasonClient : ISeasonClient
{
    private readonly IJellyfinApiClient _api;
    private readonly IUserClient _userClient;

    public SeasonClient(
        IJellyfinApiClient api,
        IUserClient userClient)
    {
        _api = api;
        _userClient = userClient;
    }

    public async Task<IReadOnlyList<SeasonInfo>> GetSeasonsAsync(
        string seriesId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(seriesId))
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

        var escapedUserId =
            Uri.EscapeDataString(userId);

        var endpoint =
            $"Shows/{escapedSeriesId}/Seasons" +
            $"?UserId={escapedUserId}" +
            "&Fields=Overview,IndexNumber" +
            "&EnableImages=true" +
            "&ImageTypeLimit=1";

        var response =
            await _api.GetAsync<JellyfinItemsResponse>(
                endpoint,
                cancellationToken);

        if (response?.Items is null)
        {
            return [];
        }

        return response.Items
            .Select(item => new SeasonInfo
            {
                Id = item.Id ?? string.Empty,
                Name = item.Name ?? string.Empty,
                IndexNumber = item.IndexNumber ?? 0,
                EpisodeCount = 0,
                ImageTag = GetPrimaryImageTag(item)
            })
            .OrderBy(item => item.IndexNumber)
            .ThenBy(item => item.Name)
            .ToList();
    }

    private static string? GetPrimaryImageTag(
        JellyfinSeasonResponse season)
    {
        if (season.ImageTags is null)
        {
            return null;
        }

        return season.ImageTags.TryGetValue(
            "Primary",
            out var imageTag)
            ? imageTag
            : null;
    }

    private sealed class JellyfinItemsResponse
    {
        public List<JellyfinSeasonResponse> Items { get; set; } = [];
    }

    private sealed class JellyfinSeasonResponse
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public int? IndexNumber { get; set; }

        public Dictionary<string, string>? ImageTags { get; set; }
    }
}