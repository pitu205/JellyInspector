using JellyInspector.Scanner.Media;

namespace JellyInspector.Scanner.Sources;

/// <summary>
/// Fuente de datos normalizada para el motor.
/// </summary>
public interface IScannerDataSource
{
    IAsyncEnumerable<MediaSeries> GetSeriesAsync(
        CancellationToken cancellationToken = default);
}
