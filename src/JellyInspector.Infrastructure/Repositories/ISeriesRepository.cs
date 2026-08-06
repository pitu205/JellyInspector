using JellyInspector.Domain.Entities;

namespace JellyInspector.Infrastructure.Repositories;

public interface ISeriesRepository
{
    Task SaveAsync(
        IEnumerable<SeriesEntity> series,
        CancellationToken cancellationToken = default);

    Task<List<SeriesEntity>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<SeriesEntity?> GetByJellyfinIdAsync(
        string jellyfinId,
        CancellationToken cancellationToken = default);

    Task ReplaceAsync(
        SeriesEntity series,
        CancellationToken cancellationToken = default);

    Task ClearAsync(
        CancellationToken cancellationToken = default);
}
