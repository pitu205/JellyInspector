using JellyInspector.Application.Models;

namespace JellyInspector.Infrastructure.Jellyfin.Clients;

public interface ISeriesClient
{
    Task<IReadOnlyList<SeriesInfo>> GetSeriesAsync(
        IEnumerable<string> libraryIds,
        CancellationToken cancellationToken = default);

    Task<SeriesDetail?> GetSeriesDetailAsync(
        string seriesId,
        CancellationToken cancellationToken = default);
}
