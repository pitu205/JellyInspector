namespace JellyInspector.Application.Models;

public class EpisodeInfo
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int SeasonNumber { get; set; }

    public int EpisodeNumber { get; set; }

    public TimeSpan Runtime { get; set; }

    public string Resolution { get; set; } = string.Empty;

    public string VideoCodec { get; set; } = string.Empty;

    public string AudioCodec { get; set; } = string.Empty;

    public List<string> AudioLanguages { get; set; } = [];

    public List<string> SubtitleLanguages { get; set; } = [];

    public long FileSize { get; set; }

    public long Bitrate { get; set; }

    public bool HasHdr { get; set; }

    public bool HasDolbyVision { get; set; }

    public bool HasSpanishAudio =>
        AudioLanguages.Any(x =>
            x.Equals("spa", StringComparison.OrdinalIgnoreCase) ||
            x.Equals("es", StringComparison.OrdinalIgnoreCase) ||
            x.Equals("Spanish", StringComparison.OrdinalIgnoreCase));

    public bool HasSpanishSubtitles =>
        SubtitleLanguages.Any(x =>
            x.Equals("spa", StringComparison.OrdinalIgnoreCase) ||
            x.Equals("es", StringComparison.OrdinalIgnoreCase) ||
            x.Equals("Spanish", StringComparison.OrdinalIgnoreCase));
}