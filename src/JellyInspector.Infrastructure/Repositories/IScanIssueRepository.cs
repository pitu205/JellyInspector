using JellyInspector.Domain.Entities;

namespace JellyInspector.Infrastructure.Repositories;

public interface IScanIssueRepository
{
    Task ClearAsync(
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        IEnumerable<ScanIssueEntity> issues,
        CancellationToken cancellationToken = default);
}