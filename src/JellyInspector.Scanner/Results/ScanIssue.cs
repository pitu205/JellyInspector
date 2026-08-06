namespace JellyInspector.Scanner.Results;

/// <summary>
/// Incidencia detectada por una regla.
/// </summary>
public sealed class ScanIssue
{
    public string RuleId { get; init; } = string.Empty;

    public string RuleName { get; init; } = string.Empty;

    public ScanSeverity Severity { get; init; }

    public string SeriesId { get; init; } = string.Empty;

    public string SeriesName { get; init; } = string.Empty;

    public int? SeasonNumber { get; init; }

    public int? EpisodeNumber { get; init; }

    public string Message { get; init; } = string.Empty;

    public string? RecommendedAction { get; init; }
}
