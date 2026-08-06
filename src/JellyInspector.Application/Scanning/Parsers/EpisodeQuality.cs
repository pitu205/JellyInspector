namespace JellyInspector.Application.Scanning.Parsers;

public sealed class EpisodeQuality
{
    public string Resolution { get; set; } = "";

    public string Source { get; set; } = "";

    public string Codec { get; set; } = "";

    public string Audio { get; set; } = "";

    public bool HDR { get; set; }

    public bool DolbyVision { get; set; }

    public string ReleaseGroup { get; set; } = "";
}