namespace JellyInspector.Domain.Entities;

public class AppSettings
{
    public int Id { get; set; }

    // Jellyfin
    public string? JellyfinServerUrl { get; set; }
    public string? JellyfinApiKey { get; set; }

    // TMDb
    public string? TmdbApiKey { get; set; }

    // Bibliotecas de series seleccionadas (IDs separados por punto y coma)
    public string? SelectedSeriesLibraryIds { get; set; }

    // General
    public bool DarkMode { get; set; } = false;
}