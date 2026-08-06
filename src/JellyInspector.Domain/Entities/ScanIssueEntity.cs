namespace JellyInspector.Domain.Entities;

public sealed class ScanIssueEntity
{
    public Guid Id { get; set; }

    public Guid ScanSessionId { get; set; }

    public ScanSessionEntity ScanSession { get; set; } = null!;

    public Guid SeriesId { get; set; }

    public SeriesEntity Series { get; set; } = null!;

    public string Type { get; set; } = string.Empty;

    public int? SeasonNumber { get; set; }

    public int? EpisodeNumber { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; }
}