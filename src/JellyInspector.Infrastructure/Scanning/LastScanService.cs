using JellyInspector.Application.Scanning;
using JellyInspector.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JellyInspector.Infrastructure.Scanning;

public sealed class LastScanService : ILastScanService
{
    private readonly JellyInspectorDbContext _db;

    public LastScanService(
        JellyInspectorDbContext db)
    {
        _db = db;
    }

    public async Task<ScanResult?> GetLastAsync(
        CancellationToken cancellationToken = default)
    {
        var session = await _db.ScanSessions
            .AsNoTracking()
            .OrderByDescending(x => x.FinishedUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (session is null)
        {
            return null;
        }

        var storedIssues = await _db.ScanIssues
            .AsNoTracking()
            .Where(x => x.ScanSessionId == session.Id)
            .Select(x => new
            {
                x.Type,
                SeriesName = x.Series.Name,
                x.SeasonNumber,
                x.EpisodeNumber,
                x.Message
            })
            .ToListAsync(cancellationToken);

        var issues = storedIssues
            .Select(x => new ScanIssue
            {
                Type = Enum.TryParse<ScanIssueType>(
                    x.Type,
                    ignoreCase: true,
                    out var type)
                        ? type
                        : ScanIssueType.MetadataMismatch,

                SeriesName = x.SeriesName,
                SeasonNumber = x.SeasonNumber,
                EpisodeNumber = x.EpisodeNumber,
                Message = x.Message
            })
            .ToList();

        return new ScanResult
        {
            Series = session.Series,
            Seasons = session.Seasons,
            Episodes = session.Episodes,

            MissingEpisodes = issues.Count(x =>
                x.Type == ScanIssueType.MissingEpisode),

            Issues = issues,
            Duration = session.Duration
        };
    }
}