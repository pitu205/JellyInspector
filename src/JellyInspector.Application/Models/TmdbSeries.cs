namespace JellyInspector.Application.Models;

public sealed class TmdbSeries
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string OriginalName { get; init; } = string.Empty;

    public string Overview { get; init; } = string.Empty;

    public string Tagline { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public bool InProduction { get; init; }

    public DateOnly? FirstAirDate { get; init; }

    public DateOnly? LastAirDate { get; init; }

    public string PosterPath { get; init; } = string.Empty;

    public string BackdropPath { get; init; } = string.Empty;

    public double VoteAverage { get; init; }

    public int VoteCount { get; init; }

    public string OriginalLanguage { get; init; } = string.Empty;

    public int NumberOfSeasons { get; init; }

    public int NumberOfEpisodes { get; init; }

    public List<string> Genres { get; init; } = [];

    public List<string> Networks { get; init; } = [];

    public List<string> ProductionCountries { get; init; } = [];

    public TmdbEpisode? NextEpisodeToAir { get; init; }

    public TmdbEpisode? LastEpisodeToAir { get; init; }

    public List<TmdbSeason> Seasons { get; init; } = [];
}
