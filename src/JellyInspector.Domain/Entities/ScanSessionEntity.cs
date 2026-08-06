namespace JellyInspector.Domain.Entities;

public sealed class ScanSessionEntity
{
    public Guid Id { get; set; }

    public DateTime StartedUtc { get; set; }

    public DateTime FinishedUtc { get; set; }

    public TimeSpan Duration { get; set; }

    public int Series { get; set; }

    public int Seasons { get; set; }

    public int Episodes { get; set; }

    public int IssueCount { get; set; }

    public ICollection<ScanIssueEntity> Issues { get; set; }
        = new List<ScanIssueEntity>();
}