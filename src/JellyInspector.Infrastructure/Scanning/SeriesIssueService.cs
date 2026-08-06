using JellyInspector.Application.Scanning;
using JellyInspector.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JellyInspector.Infrastructure.Scanning;

public sealed class SeriesIssueService : ISeriesIssueService
{
    private readonly JellyInspectorDbContext _db;

    public SeriesIssueService(
        JellyInspectorDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ScanIssue>> GetByJellyfinIdAsync(
        string jellyfinId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jellyfinId))
        {
            return [];
        }

        var rows = await _db.ScanIssues
            .AsNoTracking()
            .Where(issue =>
                issue.Series.JellyfinId == jellyfinId)
            .OrderBy(issue => issue.SeasonNumber)
            .ThenBy(issue => issue.EpisodeNumber)
            .Select(issue => new
            {
                issue.Type,
                issue.Series.Name,
                issue.SeasonNumber,
                issue.EpisodeNumber,
                issue.Message
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new ScanIssue
            {
                Type = Enum.TryParse<ScanIssueType>(
                    row.Type,
                    ignoreCase: true,
                    out var parsedType)
                        ? parsedType
                        : ScanIssueType.MetadataMismatch,

                SeriesName = row.Name,
                SeasonNumber = row.SeasonNumber,
                EpisodeNumber = row.EpisodeNumber,
                Message = row.Message
            })
            .ToList();
    }
}