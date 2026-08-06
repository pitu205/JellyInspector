using JellyInspector.Scanner.Media;
using JellyInspector.Scanner.Results;

namespace JellyInspector.Scanner.Rules;

/// <summary>
/// Regla independiente ejecutada sobre una serie.
/// </summary>
public interface IScannerRule
{
    string Id { get; }

    string Name { get; }

    string Description { get; }

    string Category { get; }

    bool IsEnabledByDefault { get; }

    Task<IReadOnlyCollection<ScanIssue>> ExecuteAsync(
        MediaSeries series,
        CancellationToken cancellationToken = default);
}
