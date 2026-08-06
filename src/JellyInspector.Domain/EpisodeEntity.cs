namespace JellyInspector.Domain.Entities;

public class EpisodeEntity
{
    public Guid Id { get; set; }

    public string JellyfinId { get; set; } = string.Empty;

    public Guid SeasonId { get; set; }

    public int EpisodeNumber { get; set; }

    public string Name { get; set; } = string.Empty;

    public TimeSpan Runtime { get; set; }

    public string Resolution { get; set; } = string.Empty;

    public string VideoCodec { get; set; } = string.Empty;

    public string AudioCodec { get; set; } = string.Empty;

    public long Bitrate { get; set; }

    public long FileSize { get; set; }

    public bool HasHdr { get; set; }

    public bool HasDolbyVision { get; set; }

    public SeasonEntity Season { get; set; } = null!;
}