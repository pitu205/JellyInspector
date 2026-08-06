namespace JellyInspector.Domain.Entities;

public class Episode
{
    public int Id { get; set; }

    public int SeasonId { get; set; }

    public int EpisodeNumber { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FullPath { get; set; } = string.Empty;

    public Season? Season { get; set; }
}