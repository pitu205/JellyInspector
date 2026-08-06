using JellyInspector.Application.Dashboard;
using JellyInspector.Application.Scanning;
using JellyInspector.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JellyInspector.Infrastructure.Dashboard;

public sealed class DashboardService
    : IDashboardService
{
    private readonly JellyInspectorDbContext _db;

    public DashboardService(
        JellyInspectorDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardDto> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var missingEpisodeType =
            ScanIssueType.MissingEpisode.ToString();

        var missingSeasonType =
            ScanIssueType.MissingSeason.ToString();

        var airingType =
            ScanIssueType.Airing.ToString();

        var upcomingType =
            ScanIssueType.Upcoming.ToString();

        var seriesRows = await _db.Series
            .AsNoTracking()
            .Select(series => new
            {
                series.JellyfinId,
                series.Name,
                SeasonCount = series.Seasons.Count,
                EpisodeCount = series.Seasons.Sum(
                    season => season.Episodes.Count),
                Issues = series.ScanIssues
                    .Select(issue => issue.Type)
                    .ToList()
            })
            .OrderBy(series => series.Name)
            .ToListAsync(cancellationToken);

        var lastScan = await _db.ScanSessions
            .AsNoTracking()
            .OrderByDescending(session => session.FinishedUtc)
            .Select(session => new
            {
                session.FinishedUtc,
                session.Duration,
                session.IssueCount
            })
            .FirstOrDefaultAsync(cancellationToken);

        var issueRows = seriesRows
            .Select(series =>
            {
                var actionable = series.Issues
                    .Where(type =>
                        type != airingType &&
                        type != upcomingType)
                    .ToList();

                var health = CalculateHealth(actionable);

                return new DashboardIssueDto
                {
                    JellyfinId = series.JellyfinId,
                    Name = series.Name,
                    IssueCount = actionable.Count,
                    SeasonCount = series.SeasonCount,
                    EpisodeCount = series.EpisodeCount,
                    MissingEpisodes = actionable.Count(
                        type => type == missingEpisodeType),
                    MissingSeasons = actionable.Count(
                        type => type == missingSeasonType),
                    HealthScore = health
                };
            })
            .ToList();

        var seriesCount = seriesRows.Count;
        var healthPercentage = seriesCount == 0
            ? 100d
            : issueRows.Average(item => item.HealthScore);

        return new DashboardDto
        {
            Series = seriesCount,

            Seasons = await _db.Seasons
                .CountAsync(cancellationToken),

            Episodes = await _db.Episodes
                .CountAsync(cancellationToken),

            SeriesWithIssues = issueRows.Count(
                item => item.IssueCount > 0),

            PerfectSeries = seriesRows.Count(series =>
                series.Issues.Count == 0),

            MissingEpisodes = seriesRows.Sum(series =>
                series.Issues.Count(type =>
                    type == missingEpisodeType)),

            MissingSeasons = seriesRows.Sum(series =>
                series.Issues.Count(type =>
                    type == missingSeasonType)),

            AiringSeries = seriesRows.Count(series =>
                series.Issues.Contains(airingType)),

            UpcomingSeries = seriesRows.Count(series =>
                series.Issues.Contains(upcomingType)),

            HealthPercentage = healthPercentage,

            LastScanUtc = lastScan?.FinishedUtc,
            LastScanDuration = lastScan?.Duration,
            LastScanIssueCount = lastScan?.IssueCount ?? 0,

            TopIssues = issueRows
                .Where(item => item.IssueCount > 0)
                .OrderBy(item => item.HealthScore)
                .ThenByDescending(item => item.MissingSeasons)
                .ThenByDescending(item => item.MissingEpisodes)
                .ThenBy(item => item.Name)
                .Take(6)
                .ToList()
        };
    }

    private static int CalculateHealth(
        IEnumerable<string> issueTypes)
    {
        var penalty = 0;

        foreach (var type in issueTypes)
        {
            penalty += type switch
            {
                nameof(ScanIssueType.MissingEpisode) => 1,
                nameof(ScanIssueType.MissingSeason) => 15,
                nameof(ScanIssueType.DuplicateEpisode) => 2,
                nameof(ScanIssueType.MissingPoster) => 5,
                nameof(ScanIssueType.MissingOverview) => 5,
                nameof(ScanIssueType.MissingTmdb) => 10,
                nameof(ScanIssueType.MetadataMismatch) => 5,
                _ => 0
            };
        }

        return Math.Clamp(100 - penalty, 0, 100);
    }
}
