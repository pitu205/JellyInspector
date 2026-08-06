using JellyInspector.Domain.Entities;
using JellyInspector.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JellyInspector.Infrastructure.Repositories;

public sealed class ScanSessionRepository
    : IScanSessionRepository
{
    private readonly JellyInspectorDbContext _db;

    public ScanSessionRepository(
        JellyInspectorDbContext db)
    {
        _db = db;
    }

    public async Task SaveAsync(
        ScanSessionEntity session,
        CancellationToken cancellationToken = default)
    {
        await _db.ScanSessions.AddAsync(
            session,
            cancellationToken);

        await _db.SaveChangesAsync(
            cancellationToken);
    }

    public Task<ScanSessionEntity?> GetLastAsync(
        CancellationToken cancellationToken = default)
    {
        return _db.ScanSessions
            .OrderByDescending(session => session.FinishedUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task RefreshIssueCountAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _db.ScanSessions
            .SingleOrDefaultAsync(
                item => item.Id == sessionId,
                cancellationToken);

        if (session is null)
        {
            return;
        }

        session.IssueCount = await _db.ScanIssues
            .CountAsync(
                issue => issue.ScanSessionId == sessionId,
                cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
    }
}
