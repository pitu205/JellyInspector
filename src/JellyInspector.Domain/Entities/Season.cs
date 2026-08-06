namespace JellyInspector.Domain.Entities;

public class Season
{
    public int Id { get; set; }

    public int SeriesId { get; set; }

    public int SeasonNumber { get; set; }

    public Series? Series { get; set; }
}