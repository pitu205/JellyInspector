using JellyInspector.Domain.Entities;
using JellyInspector.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JellyInspector.Infrastructure.Repositories;

public sealed class SeriesRepository : ISeriesRepository
{
    private readonly JellyInspectorDbContext _db;

    public SeriesRepository(JellyInspectorDbContext db)
    {
        _db = db;
    }

    public async Task<List<SeriesEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _db.Series
            .AsNoTracking()
            .Include(series => series.Seasons)
            .ThenInclude(season => season.Episodes)
            .Include(series => series.ScanIssues)
            .ToListAsync(cancellationToken);
    }

    public async Task<SeriesEntity?> GetByJellyfinIdAsync(
        string jellyfinId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jellyfinId))
        {
            return null;
        }

        return await _db.Series
            .AsNoTracking()
            .Include(series => series.Seasons)
            .ThenInclude(season => season.Episodes)
            .Include(series => series.ScanIssues)
            .SingleOrDefaultAsync(
                series => series.JellyfinId == jellyfinId,
                cancellationToken);
    }

    public async Task SaveAsync(
        IEnumerable<SeriesEntity> series,
        CancellationToken cancellationToken = default)
    {
        await _db.Series.AddRangeAsync(
            series,
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task ReplaceAsync(
        SeriesEntity series,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(series);

        var existing = await _db.Series
            .SingleOrDefaultAsync(
                item => item.JellyfinId == series.JellyfinId,
                cancellationToken);

        if (existing is not null)
        {
            _db.Series.Remove(existing);
            await _db.SaveChangesAsync(cancellationToken);
        }

        await _db.Series.AddAsync(series, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearAsync(
        CancellationToken cancellationToken = default)
    {
        _db.Series.RemoveRange(_db.Series);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
