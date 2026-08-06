namespace JellyInspector.Scanner.Media;

/// <summary>
/// Serie multimedia normalizada para el motor de anÃ¡lisis.
/// </summary>
public sealed class MediaSeries
{
    private readonly List<MediaSeason> _seasons = [];

    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int? ProductionYear { get; init; }

    public string? Overview { get; init; }

    public string? PosterPath { get; init; }

    public string? BackdropPath { get; init; }

    public string? TmdbId { get; init; }

    public string? TvdbId { get; init; }

    public string? ImdbId { get; init; }

    public IReadOnlyList<MediaSeason> Seasons => _seasons;

    public void AddSeason(MediaSeason season)
    {
        ArgumentNullException.ThrowIfNull(season);
        _seasons.Add(season);
    }

    public void AddSeasons(IEnumerable<MediaSeason> seasons)
    {
        ArgumentNullException.ThrowIfNull(seasons);

        foreach (var season in seasons)
        {
            AddSeason(season);
        }
    }
}
