namespace JellyInspector.Application.Scanning;

public sealed class ScanProgress
{
    public string CurrentSeries { get; set; } = "";

    public int Current { get; set; }

    public int Total { get; set; }

    public double Percentage =>
        Total == 0 ? 0 : Current * 100d / Total;
}