using JellyInspector.Application.Models;
using JellyInspector.Application.Scanning;
using JellyInspector.Domain.Entities;

namespace JellyInspector.Infrastructure.Comparison;

public sealed class LibraryComparisonService
{
    public List<ScanIssue> Compare(
        SeriesEntity jellyfinSeries,
        TmdbSeries tmdbSeries)
    {
        var issues = new List<ScanIssue>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var isOngoing = IsOngoing(tmdbSeries);

        foreach (var tmdbSeason in tmdbSeries.Seasons)
        {
            // De momento ignoramos los especiales.
            if (tmdbSeason.SeasonNumber == 0)
            {
                continue;
            }

            var airedEpisodes = tmdbSeason.Episodes
                .Where(episode =>
                    episode.AirDate.HasValue &&
                    episode.AirDate.Value <= today)
                .OrderBy(episode => episode.EpisodeNumber)
                .ToList();

            var futureOrUnscheduledEpisodes = tmdbSeason.Episodes
                .Where(episode =>
                    !episode.AirDate.HasValue ||
                    episode.AirDate.Value > today)
                .OrderBy(episode => episode.EpisodeNumber)
                .ToList();

            var seasonHasStarted =
                airedEpisodes.Count > 0 ||
                tmdbSeason.AirDate is not null &&
                tmdbSeason.AirDate.Value <= today;

            var seasonIsFuture =
                !seasonHasStarted &&
                (tmdbSeason.AirDate is null ||
                 tmdbSeason.AirDate.Value > today);

            var jellyfinSeason =
                jellyfinSeries.Seasons.FirstOrDefault(
                    season =>
                        season.SeasonNumber ==
                        tmdbSeason.SeasonNumber);

            if (jellyfinSeason is null)
            {
                if (seasonIsFuture)
                {
                    issues.Add(CreateUpcomingIssue(
                        jellyfinSeries,
                        tmdbSeason));

                    continue;
                }

                if (isOngoing &&
                    futureOrUnscheduledEpisodes.Count > 0)
                {
                    issues.Add(CreateAiringIssue(
                        jellyfinSeries,
                        tmdbSeries,
                        tmdbSeason,
                        airedEpisodes.Count));

                    foreach (var airedEpisode in airedEpisodes)
                    {
                        issues.Add(CreateMissingEpisodeIssue(
                            jellyfinSeries,
                            tmdbSeason.SeasonNumber,
                            airedEpisode.EpisodeNumber,
                            airedEpisode.AirDate));
                    }

                    continue;
                }

                issues.Add(new ScanIssue
                {
                    Type = ScanIssueType.MissingSeason,
                    SeriesName = jellyfinSeries.Name,
                    SeasonNumber = tmdbSeason.SeasonNumber,
                    Message =
                        $"Falta la temporada " +
                        $"{tmdbSeason.SeasonNumber}."
                });

                continue;
            }

            foreach (var tmdbEpisode in airedEpisodes)
            {
                var exists =
                    jellyfinSeason.Episodes.Any(
                        episode =>
                            episode.EpisodeNumber ==
                            tmdbEpisode.EpisodeNumber);

                if (exists)
                {
                    continue;
                }

                issues.Add(CreateMissingEpisodeIssue(
                    jellyfinSeries,
                    tmdbSeason.SeasonNumber,
                    tmdbEpisode.EpisodeNumber,
                    tmdbEpisode.AirDate));
            }

            if (isOngoing &&
                futureOrUnscheduledEpisodes.Count > 0)
            {
                issues.Add(CreateAiringIssue(
                    jellyfinSeries,
                    tmdbSeries,
                    tmdbSeason,
                    airedEpisodes.Count));
            }
        }

        AddSeriesLevelBroadcastStatus(
            issues,
            jellyfinSeries,
            tmdbSeries,
            today);

        return issues
            .GroupBy(issue => new
            {
                issue.Type,
                issue.SeriesName,
                issue.SeasonNumber,
                issue.EpisodeNumber,
                issue.Message
            })
            .Select(group => group.First())
            .ToList();
    }

    private static void AddSeriesLevelBroadcastStatus(
        List<ScanIssue> issues,
        SeriesEntity jellyfinSeries,
        TmdbSeries tmdbSeries,
        DateOnly today)
    {
        if (tmdbSeries.NextEpisodeToAir is not null &&
            tmdbSeries.NextEpisodeToAir.AirDate is not null &&
            tmdbSeries.NextEpisodeToAir.AirDate.Value > today)
        {
            var nextEpisode = tmdbSeries.NextEpisodeToAir;

            var alreadyHasStatus = issues.Any(issue =>
                issue.Type is
                    ScanIssueType.Airing or
                    ScanIssueType.Upcoming &&
                issue.SeasonNumber == nextEpisode.SeasonNumber);

            if (!alreadyHasStatus)
            {
                issues.Add(new ScanIssue
                {
                    Type = ScanIssueType.Airing,
                    SeriesName = jellyfinSeries.Name,
                    SeasonNumber = nextEpisode.SeasonNumber,
                    EpisodeNumber = nextEpisode.EpisodeNumber,
                    Message =
                        $"En emisión. Próximo episodio: " +
                        $"T{nextEpisode.SeasonNumber:00}" +
                        $"E{nextEpisode.EpisodeNumber:00} " +
                        $"el {nextEpisode.AirDate.Value:dd/MM/yyyy}."
                });
            }
        }
    }

    private static ScanIssue CreateAiringIssue(
        SeriesEntity jellyfinSeries,
        TmdbSeries tmdbSeries,
        TmdbSeason tmdbSeason,
        int airedEpisodeCount)
    {
        var nextEpisode = tmdbSeries.NextEpisodeToAir;

        var message =
            $"Temporada {tmdbSeason.SeasonNumber} en emisión. " +
            $"{airedEpisodeCount} episodio(s) emitido(s)";

        if (nextEpisode is not null &&
            nextEpisode.SeasonNumber == tmdbSeason.SeasonNumber &&
            nextEpisode.AirDate is not null)
        {
            message +=
                $". Próximo episodio: " +
                $"T{nextEpisode.SeasonNumber:00}" +
                $"E{nextEpisode.EpisodeNumber:00} " +
                $"el {nextEpisode.AirDate.Value:dd/MM/yyyy}";
        }

        return new ScanIssue
        {
            Type = ScanIssueType.Airing,
            SeriesName = jellyfinSeries.Name,
            SeasonNumber = tmdbSeason.SeasonNumber,
            EpisodeNumber = nextEpisode?.EpisodeNumber,
            Message = message + "."
        };
    }

    private static ScanIssue CreateUpcomingIssue(
        SeriesEntity jellyfinSeries,
        TmdbSeason tmdbSeason)
    {
        var dateText = tmdbSeason.AirDate.HasValue
            ? $" Estreno previsto: {tmdbSeason.AirDate.Value:dd/MM/yyyy}."
            : string.Empty;

        return new ScanIssue
        {
            Type = ScanIssueType.Upcoming,
            SeriesName = jellyfinSeries.Name,
            SeasonNumber = tmdbSeason.SeasonNumber,
            Message =
                $"Temporada {tmdbSeason.SeasonNumber} próximamente." +
                dateText
        };
    }

    private static ScanIssue CreateMissingEpisodeIssue(
        SeriesEntity jellyfinSeries,
        int seasonNumber,
        int episodeNumber,
        DateOnly? airDate)
    {
        var dateText = airDate.HasValue
            ? $" Emitido el {airDate.Value:dd/MM/yyyy}."
            : string.Empty;

        return new ScanIssue
        {
            Type = ScanIssueType.MissingEpisode,
            SeriesName = jellyfinSeries.Name,
            SeasonNumber = seasonNumber,
            EpisodeNumber = episodeNumber,
            Message =
                $"Falta T{seasonNumber:00}" +
                $"E{episodeNumber:00}." +
                dateText
        };
    }

    private static bool IsOngoing(
        TmdbSeries tmdbSeries)
    {
        if (tmdbSeries.InProduction)
        {
            return true;
        }

        return tmdbSeries.Status.Equals(
                   "Returning Series",
                   StringComparison.OrdinalIgnoreCase)
               ||
               tmdbSeries.Status.Equals(
                   "In Production",
                   StringComparison.OrdinalIgnoreCase)
               ||
               tmdbSeries.Status.Equals(
                   "Planned",
                   StringComparison.OrdinalIgnoreCase)
               ||
               tmdbSeries.Status.Equals(
                   "Pilot",
                   StringComparison.OrdinalIgnoreCase);
    }
}