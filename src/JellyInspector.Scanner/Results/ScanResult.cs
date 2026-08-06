namespace JellyInspector.Scanner.Results;

/// <summary>
/// Resultado completo de una ejecuciÃ³n del motor.
/// </summary>
public sealed class ScanResult
{
    private readonly List<ScanIssue> _issues = [];

    public DateTime StartedUtc { get; init; }

    public DateTime FinishedUtc { get; internal set; }

    public TimeSpan Duration =>
        FinishedUtc > StartedUtc
            ? FinishedUtc - StartedUtc
            : TimeSpan.Zero;

    public ScanStatistics Statistics { get; } = new();

    public IReadOnlyList<ScanIssue> Issues => _issues;

    internal void AddIssues(IEnumerable<ScanIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        foreach (var issue in issues)
        {
            _issues.Add(issue);
            Statistics.IssuesDetected++;

            switch (issue.Severity)
            {
                case ScanSeverity.Info:
                    Statistics.InfoIssues++;
                    break;

                case ScanSeverity.Warning:
                    Statistics.WarningIssues++;
                    break;

                case ScanSeverity.Error:
                    Statistics.ErrorIssues++;
                    break;

                case ScanSeverity.Critical:
                    Statistics.CriticalIssues++;
                    break;
            }
        }
    }
}
