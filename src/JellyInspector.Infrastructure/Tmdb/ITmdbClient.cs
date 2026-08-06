using JellyInspector.Application.Models;

namespace JellyInspector.Infrastructure.Tmdb;

public interface ITmdbClient
{
    Task<TmdbSeries?> GetSeriesAsync(
        string tmdbId,
        CancellationToken cancellationToken = default);
}