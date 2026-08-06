using JellyInspector.Application.Comparison;
using JellyInspector.Application.Interfaces;
using JellyInspector.Infrastructure.Tmdb;

namespace JellyInspector.Infrastructure.Comparison;

public sealed class SeriesComparisonService
    : ISeriesComparisonService
{
    private readonly IJellyfinService _jellyfinService;
    private readonly ITmdbClient _tmdbClient;
    private readonly LibraryComparisonService _comparisonService;

    public SeriesComparisonService(
        IJellyfinService jellyfinService,
        ITmdbClient tmdbClient,
        LibraryComparisonService comparisonService)
    {
        _jellyfinService = jellyfinService;
        _tmdbClient = tmdbClient;
        _comparisonService = comparisonService;
    }

    public async Task<SeriesComparisonDto?> GetAsync(
        string jellyfinId,
        CancellationToken cancellationToken = default)
    {
        var jellyfin =
            await _jellyfinService.GetSeriesDetailAsync(jellyfinId);

        if (jellyfin is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(jellyfin.TmdbId))
        {
            return new SeriesComparisonDto
            {
                JellyfinId = jellyfin.Id,
                Name = jellyfin.Name,
                Jellyfin = jellyfin
            };
        }

        var tmdb =
            await _tmdbClient.GetSeriesAsync(
                jellyfin.TmdbId,
                cancellationToken);

        if (tmdb is null)
        {
            return new SeriesComparisonDto
            {
                JellyfinId = jellyfin.Id,
                Name = jellyfin.Name,
                Jellyfin = jellyfin
            };
        }

        // La comparación la conectaremos en el siguiente paso.
        return new SeriesComparisonDto
        {
            JellyfinId = jellyfin.Id,
            Name = jellyfin.Name,
            Jellyfin = jellyfin,
            Tmdb = tmdb
        };
    }
}