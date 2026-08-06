using JellyInspector.Scanner.Results;
using JellyInspector.Scanner.Rules;
using JellyInspector.Scanner.Sources;

namespace JellyInspector.Scanner.Engine;

/// <summary>
/// Motor independiente de anÃ¡lisis de bibliotecas multimedia.
/// </summary>
public sealed class ScannerEngine
{
    private readonly IReadOnlyList<IScannerRule> _rules;

    public ScannerEngine(IEnumerable<IScannerRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        _rules = rules
            .Where(rule => rule.IsEnabledByDefault)
            .ToArray();
    }

    public async Task<ScanResult> ScanAsync(
        IScannerDataSource dataSource,
        IProgress<ScannerProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dataSource);

        var result = new ScanResult
        {
            StartedUtc = DateTime.UtcNow
        };

        await foreach (var series in dataSource
                           .GetSeriesAsync(cancellationToken)
                           .WithCancellation(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            result.Statistics.SeriesProcessed++;

            foreach (var rule in _rules)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var issues = await rule.ExecuteAsync(
                    series,
                    cancellationToken);

                result.Statistics.RulesExecuted++;
                result.AddIssues(issues);

                progress?.Report(new ScannerProgress
                {
                    ProcessedSeries =
                        result.Statistics.SeriesProcessed,

                    CurrentSeries =
                        series.Name,

                    RulesExecuted =
                        result.Statistics.RulesExecuted,

                    IssuesDetected =
                        result.Statistics.IssuesDetected
                });
            }
        }

        result.FinishedUtc = DateTime.UtcNow;

        return result;
    }
}
