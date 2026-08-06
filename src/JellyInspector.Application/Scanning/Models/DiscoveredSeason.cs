namespace JellyInspector.Application.Scanning.Models;

public sealed class DiscoveredSeason
{
    public int SeasonNumber { get; set; }

    public List<DiscoveredEpisode> Episodes { get; } = [];
}