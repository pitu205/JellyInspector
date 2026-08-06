using JellyInspector.Application.Models;

namespace JellyInspector.Infrastructure.Jellyfin.Clients;

public sealed class LibraryClient : ILibraryClient
{
    private readonly IJellyfinApiClient _api;
    private readonly IUserClient _userClient;

    public LibraryClient(
        IJellyfinApiClient api,
        IUserClient userClient)
    {
        _api = api;
        _userClient = userClient;
    }

    public async Task<IReadOnlyList<JellyfinLibraryInfo>>
        GetSeriesLibrariesAsync(
            CancellationToken cancellationToken = default)
    {
        var userId =
            await _userClient.GetCurrentUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return [];
        }

        var folders =
            await _api.GetAsync<List<VirtualFolderResponse>>(
                "Library/VirtualFolders",
                cancellationToken);

        if (folders is null)
        {
            return [];
        }

        var result = new List<JellyfinLibraryInfo>();

        foreach (var folder in folders
                     .Where(IsSeriesLibrary)
                     .Where(item =>
                         !string.IsNullOrWhiteSpace(item.ItemId)))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var count = await GetSeriesCountAsync(
                userId,
                folder.ItemId!,
                cancellationToken);

            result.Add(new JellyfinLibraryInfo
            {
                Id = folder.ItemId!,
                Name = folder.Name ?? "Biblioteca sin nombre",
                CollectionType = folder.CollectionType ?? string.Empty,
                SeriesCount = count,
                Locations = folder.Locations ?? []
            });
        }

        return result
            .OrderBy(item => item.Name)
            .ToList();
    }

    private async Task<int> GetSeriesCountAsync(
        string userId,
        string libraryId,
        CancellationToken cancellationToken)
    {
        var endpoint =
            $"Users/{Uri.EscapeDataString(userId)}/Items" +
            $"?ParentId={Uri.EscapeDataString(libraryId)}" +
            "&IncludeItemTypes=Series" +
            "&Recursive=true" +
            "&StartIndex=0" +
            "&Limit=1";

        var response =
            await _api.GetAsync<ItemsResponse>(
                endpoint,
                cancellationToken);

        return response?.TotalRecordCount ?? 0;
    }

    private static bool IsSeriesLibrary(
        VirtualFolderResponse folder)
    {
        return string.Equals(
                   folder.CollectionType,
                   "tvshows",
                   StringComparison.OrdinalIgnoreCase)
               || string.Equals(
                   folder.CollectionType,
                   "shows",
                   StringComparison.OrdinalIgnoreCase);
    }

    private sealed class VirtualFolderResponse
    {
        public string? Name { get; set; }

        public string? CollectionType { get; set; }

        public string? ItemId { get; set; }

        public List<string>? Locations { get; set; }
    }

    private sealed class ItemsResponse
    {
        public int TotalRecordCount { get; set; }
    }
}
