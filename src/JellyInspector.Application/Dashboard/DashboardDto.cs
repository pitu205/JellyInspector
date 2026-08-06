namespace JellyInspector.Application.Dashboard;

public sealed class DashboardDto
{
    public int Series { get; init; }

    public int Seasons { get; init; }

    public int Episodes { get; init; }

    public int SeriesWithIssues { get; init; }

    public int PerfectSeries { get; init; }

    public int MissingEpisodes { get; init; }

    public int MissingSeasons { get; init; }

    public int AiringSeries { get; init; }

    public int UpcomingSeries { get; init; }

    public double HealthPercentage { get; init; } = 100d;

    public DateTime? LastScanUtc { get; init; }

    public TimeSpan? LastScanDuration { get; init; }

    public int LastScanIssueCount { get; init; }

    public IReadOnlyList<DashboardIssueDto> TopIssues { get; init; } = [];
}

public sealed class DashboardIssueDto
{
    public string JellyfinId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int IssueCount { get; init; }

    public int SeasonCount { get; init; }

    public int EpisodeCount { get; init; }

    public int MissingEpisodes { get; init; }

    public int MissingSeasons { get; init; }

    public int HealthScore { get; init; }
}
