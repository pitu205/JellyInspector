namespace JellyInspector.Application.Scanning.Models;

public sealed class DiscoveredSeries
{
    public string Name { get; set; } = "";

    public string Folder { get; set; } = "";

    public List<DiscoveredSeason> Seasons { get; } = [];
}