namespace JellyInspector.Application.Scanning;

public interface ILastScanService
{
    Task<ScanResult?> GetLastAsync(
        CancellationToken cancellationToken = default);
}