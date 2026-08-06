namespace JellyInspector.Application.Scanning.Parsers;

public sealed class EpisodeInfo
{
    public bool Success { get; init; }

    public int Season { get; init; }

    public int Episode { get; init; }

    public string FileName { get; init; } = "";
}