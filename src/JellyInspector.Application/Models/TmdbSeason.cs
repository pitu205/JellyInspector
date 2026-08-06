namespace JellyInspector.Application.Models;

public sealed class TmdbSeason
{
    public int SeasonNumber { get; init; }

    public string Name { get; init; } = string.Empty;

    public DateOnly? AirDate { get; init; }

    public int EpisodeCount { get; init; }

    public List<TmdbEpisode> Episodes { get; init; } = [];
}