namespace JellyInspector.Application.Scanning;

public sealed class ScanIssue
{
    public ScanIssueType Type { get; init; }

    public string SeriesName { get; init; } = string.Empty;

    public int? SeasonNumber { get; init; }

    public int? EpisodeNumber { get; init; }

    public string Message { get; init; } = string.Empty;
}