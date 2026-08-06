using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.JellyInspector.Configuration;

public sealed class PluginConfiguration
    : BasePluginConfiguration
{
    public bool Enabled { get; set; } = true;

    public string TmdbApiKey { get; set; } = string.Empty;

    public string TmdbLanguage { get; set; } = "es-ES";

    public string SelectedLibraryIds { get; set; } = string.Empty;
}
