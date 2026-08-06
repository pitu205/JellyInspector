namespace JellyInspector.Application.Library;

public sealed class LibrarySeriesDto
{
    public Guid Id { get; init; }

    public string JellyfinId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int ProductionYear { get; init; }

    public int SeasonCount { get; init; }

    public int EpisodeCount { get; init; }

    public string DominantResolution { get; init; } = string.Empty;

    public bool HasHdr { get; init; }

    public bool HasDolbyVision { get; init; }

    public string? Overview { get; init; }

    public string? PosterTag { get; init; }

    public bool HasTmdb { get; init; }

    public double TmdbVoteAverage { get; init; }

    public int TmdbVoteCount { get; init; }

    public int MissingEpisodes { get; init; }

    public int MissingSeasons { get; init; }

    public bool HasPoster =>
        !string.IsNullOrWhiteSpace(PosterTag);

    public bool HasOverview =>
        !string.IsNullOrWhiteSpace(Overview);

    public bool Complete =>
        MissingEpisodes == 0 &&
        MissingSeasons == 0;

    // Alias para el futuro LibraryBrowserService.
    public int Seasons => SeasonCount;

    public int Episodes => EpisodeCount;
}