namespace JellyInspector.Application.Models;

public class SeriesInfo
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string LibraryName { get; set; } = string.Empty;

    public int ProductionYear { get; set; }

    public int SeasonCount { get; set; }

    public int EpisodeCount { get; set; }

    public string? Overview { get; set; }

    public string? ImageTag { get; set; }

    public string? TmdbId { get; set; }

    public string? TvdbId { get; set; }

    public string? ImdbId { get; set; }
}