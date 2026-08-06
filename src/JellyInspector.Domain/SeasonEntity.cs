namespace JellyInspector.Domain.Entities;

public class SeasonEntity
{
    public Guid Id { get; set; }

    public string JellyfinId { get; set; } = string.Empty;

    public Guid SeriesId { get; set; }

    public int SeasonNumber { get; set; }

    public string Name { get; set; } = string.Empty;

    public SeriesEntity Series { get; set; } = null!;

    public ICollection<EpisodeEntity> Episodes { get; set; } = [];
}