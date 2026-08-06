namespace JellyInspector.Application.Models;

public class SeasonInfo
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int IndexNumber { get; set; }

    public int EpisodeCount { get; set; }

    public string? ImageTag { get; set; }
}