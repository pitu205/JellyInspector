namespace JellyInspector.Application.Scanning;

public sealed class ScanResult
{
    public int Series { get; set; }

    public int Seasons { get; set; }

    public int Episodes { get; set; }

    public int MissingEpisodes { get; set; }

    public TimeSpan Duration { get; set; }

    public List<ScanIssue> Issues { get; set; } = [];
}