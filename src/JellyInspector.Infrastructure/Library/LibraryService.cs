using System.Diagnostics;
using JellyInspector.Application.Library;
using JellyInspector.Application.Scanning;
using JellyInspector.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JellyInspector.Infrastructure.Library;

public sealed class LibraryService : ILibraryService
{
    private readonly JellyInspectorDbContext _db;

    public LibraryService(
        JellyInspectorDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<LibrarySeriesDto>> GetSeriesAsync(
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        var missingEpisodeType =
            ScanIssueType.MissingEpisode.ToString();

        var missingSeasonType =
            ScanIssueType.MissingSeason.ToString();

        var result = await _db.Series
            .AsNoTracking()
            .Select(series => new LibrarySeriesDto
            {
                Id = series.Id,
                JellyfinId = series.JellyfinId,
                Name = series.Name,
                ProductionYear = series.ProductionYear,

                SeasonCount = series.Seasons.Count,

                EpisodeCount = series.Seasons
                    .Sum(season => season.Episodes.Count),

                DominantResolution = string.Empty,

                HasHdr = series.Seasons
                    .SelectMany(season => season.Episodes)
                    .Any(episode => episode.HasHdr),

                HasDolbyVision = series.Seasons
                    .SelectMany(season => season.Episodes)
                    .Any(episode => episode.HasDolbyVision),

                Overview = series.Overview,
                PosterTag = series.ImageTag,

                HasTmdb =
                    !string.IsNullOrWhiteSpace(series.TmdbId),

                TmdbVoteAverage =
                    series.TmdbVoteAverage,

                TmdbVoteCount =
                    series.TmdbVoteCount,

                MissingEpisodes = series.ScanIssues.Count(
                    issue => issue.Type == missingEpisodeType),

                MissingSeasons = series.ScanIssues.Count(
                    issue => issue.Type == missingSeasonType)
            })
            .OrderBy(series => series.Name)
            .ToListAsync(cancellationToken);

        stopwatch.Stop();

        Console.ForegroundColor = ConsoleColor.Green;

        Console.WriteLine(
            $"[LIBRARY SQLITE] {result.Count} series en " +
            $"{stopwatch.ElapsedMilliseconds} ms");

        Console.ResetColor();

        return result;
    }
}