namespace Jellyfin.Plugin.JellyInspector.Api.Models;

/// <summary>
/// Resumen actual de la biblioteca de series.
/// </summary>
public sealed class DashboardSummary
{
    /// <summary>
    /// Gets or sets the number of series.
    /// </summary>
    public int Series { get; set; }

    /// <summary>
    /// Gets or sets the number of seasons.
    /// </summary>
    public int Seasons { get; set; }

    /// <summary>
    /// Gets or sets the number of episodes.
    /// </summary>
    public int Episodes { get; set; }

    /// <summary>
    /// Gets or sets the UTC generation date.
    /// </summary>
    public DateTime GeneratedUtc { get; set; }
}
