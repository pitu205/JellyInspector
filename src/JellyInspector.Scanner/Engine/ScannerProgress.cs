namespace JellyInspector.Scanner.Engine;

/// <summary>
/// Progreso actual del motor de anÃ¡lisis.
/// </summary>
public sealed class ScannerProgress
{
    public int ProcessedSeries { get; init; }

    public string CurrentSeries { get; init; } = string.Empty;

    public int RulesExecuted { get; init; }

    public int IssuesDetected { get; init; }
}
