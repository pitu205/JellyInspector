using JellyInspector.Domain.Entities;
using JellyInspector.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JellyInspector.Infrastructure.Repositories;

public sealed class ScanIssueRepository
    : IScanIssueRepository
{
    private readonly JellyInspectorDbContext _db;

    public ScanIssueRepository(
        JellyInspectorDbContext db)
    {
        _db = db;
    }

    public async Task ClearAsync(
        CancellationToken cancellationToken = default)
    {
        await _db.ScanIssues
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task SaveAsync(
        IEnumerable<ScanIssueEntity> issues,
        CancellationToken cancellationToken = default)
    {
        await _db.ScanIssues.AddRangeAsync(
            issues,
            cancellationToken);

        await _db.SaveChangesAsync(
            cancellationToken);
    }
}