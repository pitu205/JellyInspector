namespace JellyInspector.Application.Scanning.Models;

public sealed class DiscoveredEpisode
{
    public int SeasonNumber { get; set; }

    public int EpisodeNumber { get; set; }

    public string FileName { get; set; } = "";

    public string FullPath { get; set; } = "";

    public long Size { get; set; }

    public DateTime LastWriteTime { get; set; }
}