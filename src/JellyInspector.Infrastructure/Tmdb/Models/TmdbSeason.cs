namespace JellyInspector.Infrastructure.Tmdb.Models;

public sealed class TmdbSeason
{
    public int SeasonNumber { get; init; }

    public string Name { get; init; } = string.Empty;

    public List<TmdbEpisode> Episodes { get; init; } = [];
}