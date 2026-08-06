namespace JellyInspector.Scanner.Media;

/// <summary>
/// Temporada multimedia normalizada.
/// </summary>
public sealed class MediaSeason
{
    private readonly List<MediaEpisode> _episodes = [];

    public string Id { get; init; } = string.Empty;

    public int Number { get; init; }

    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<MediaEpisode> Episodes => _episodes;

    public void AddEpisode(MediaEpisode episode)
    {
        ArgumentNullException.ThrowIfNull(episode);
        _episodes.Add(episode);
    }

    public void AddEpisodes(IEnumerable<MediaEpisode> episodes)
    {
        ArgumentNullException.ThrowIfNull(episodes);

        foreach (var episode in episodes)
        {
            AddEpisode(episode);
        }
    }
}
