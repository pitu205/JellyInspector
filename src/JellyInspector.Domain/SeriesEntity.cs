namespace JellyInspector.Domain.Entities;

public class SeriesEntity
{
    public Guid Id { get; set; }

    public ICollection<ScanIssueEntity> ScanIssues { get; set; }
    = new List<ScanIssueEntity>();

    public string JellyfinId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int ProductionYear { get; set; }

    public string? Overview { get; set; }

    public string? ImageTag { get; set; }

    public string? TmdbId { get; set; }

    public string? TvdbId { get; set; }

    public string? ImdbId { get; set; }

    public double TmdbVoteAverage { get; set; }

    public int TmdbVoteCount { get; set; }

    public ICollection<SeasonEntity> Seasons { get; set; } = [];
}