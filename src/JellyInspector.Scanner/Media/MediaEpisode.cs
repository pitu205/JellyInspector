namespace JellyInspector.Scanner.Media;

/// <summary>
/// Episodio multimedia normalizado.
/// </summary>
public sealed class MediaEpisode
{
    public string Id { get; init; } = string.Empty;

    public int SeasonNumber { get; init; }

    public int Number { get; init; }

    public string Title { get; init; } = string.Empty;

    public bool HasFile { get; init; }

    public string? FilePath { get; init; }

    public long? FileSize { get; init; }

    public string? Resolution { get; init; }

    public string? VideoCodec { get; init; }

    public string? AudioCodec { get; init; }

    public long? Bitrate { get; init; }

    public bool HasHdr { get; init; }

    public bool HasDolbyVision { get; init; }
}
