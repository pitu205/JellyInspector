using JellyInspector.Application.Models;

namespace JellyInspector.Application.Comparison;

public sealed class SeriesComparisonDto
{
    public string JellyfinId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public SeriesDetail? Jellyfin { get; init; }

    public TmdbSeries? Tmdb { get; init; }

    public List<Scanning.ScanIssue> Issues { get; init; } = [];
}