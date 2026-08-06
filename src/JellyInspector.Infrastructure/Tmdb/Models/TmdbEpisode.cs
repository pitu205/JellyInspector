namespace JellyInspector.Infrastructure.Tmdb.Models;

public sealed class TmdbEpisode
{
    public int EpisodeNumber { get; init; }

    public string Name { get; init; } = string.Empty;

    public DateTime? AirDate { get; init; }
}