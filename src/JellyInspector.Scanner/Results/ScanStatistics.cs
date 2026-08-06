namespace JellyInspector.Scanner.Results;

/// <summary>
/// EstadÃ­sticas acumuladas de un escaneo.
/// </summary>
public sealed class ScanStatistics
{
    public int SeriesProcessed { get; internal set; }

    public int RulesExecuted { get; internal set; }

    public int IssuesDetected { get; internal set; }

    public int InfoIssues { get; internal set; }

    public int WarningIssues { get; internal set; }

    public int ErrorIssues { get; internal set; }

    public int CriticalIssues { get; internal set; }
}
