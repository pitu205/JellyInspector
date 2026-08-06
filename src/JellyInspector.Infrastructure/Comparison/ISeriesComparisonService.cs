namespace JellyInspector.Application.Comparison;

public interface ISeriesComparisonService
{
    Task<SeriesComparisonDto?> GetAsync(
        string jellyfinId,
        CancellationToken cancellationToken = default);
}