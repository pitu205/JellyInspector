namespace JellyInspector.Application.Scanning;

public interface IScannerService
{
    ScanProgress Progress { get; }

    bool IsRunning { get; }

    Task<ScanResult>? ActiveScanTask { get; }

    bool CanCancel { get; }

    string? ActiveSeriesScanId { get; }

    bool IsSeriesRunning(string jellyfinSeriesId);

    Task<ScanResult> ScanAsync(
        string libraryPath,
        CancellationToken cancellationToken = default);

    Task CancelScanAsync();

    Task<ScanResult> ScanSeriesAsync(
        string jellyfinSeriesId,
        CancellationToken cancellationToken = default);
}
