namespace JellyInspector.Application.Models;

public class SeriesDetail
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int ProductionYear { get; set; }

    public string? Overview { get; set; }

    public string? ImageTag { get; set; }

    public string? TmdbId { get; set; }

    public List<SeasonInfo> Seasons { get; set; } = [];
}