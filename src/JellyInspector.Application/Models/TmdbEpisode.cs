namespace JellyInspector.Application.Models;

public sealed class TmdbEpisode
{
    public int SeasonNumber { get; init; }

    public int EpisodeNumber { get; init; }

    public string Name { get; init; } = string.Empty;

    public DateOnly? AirDate { get; init; }

    public int Runtime { get; init; }
}