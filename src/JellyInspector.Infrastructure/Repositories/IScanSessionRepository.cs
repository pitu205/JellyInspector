using JellyInspector.Domain.Entities;

namespace JellyInspector.Infrastructure.Repositories;

public interface IScanSessionRepository
{
    Task SaveAsync(
        ScanSessionEntity session,
        CancellationToken cancellationToken = default);

    Task<ScanSessionEntity?> GetLastAsync(
        CancellationToken cancellationToken = default);

    Task RefreshIssueCountAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);
}
