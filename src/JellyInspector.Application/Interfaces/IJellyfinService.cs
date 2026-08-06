using JellyInspector.Application.Models;

namespace JellyInspector.Application.Interfaces;

public interface IJellyfinService
{
    Task<bool> TestConnectionAsync();

    Task<SeriesDetail?> GetSeriesDetailAsync(string seriesId);

    Task<ServerInfo?> GetServerInfoAsync();

    Task<IReadOnlyList<LibraryInfo>> GetLibrariesAsync();

    Task<IReadOnlyList<SeriesInfo>> GetSeriesAsync();

    Task<string?> GetItemWebUrlAsync(string itemId);
}
