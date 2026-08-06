using System.Globalization;
using Jellyfin.Plugin.JellyInspector.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.JellyInspector;

public sealed class Plugin
    : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public Plugin(
        IApplicationPaths applicationPaths,
        IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public static Plugin? Instance { get; private set; }

    public override string Name =>
        "JellyInspector";

    public override string Description =>
        "Analiza bibliotecas de series y detecta incidencias en Jellyfin.";

    public override Guid Id =>
        Guid.Parse(
            "7d3e3b70-29bc-4e51-b20f-68c416e73a8c");

    public IEnumerable<PluginPageInfo> GetPages()
    {
        // Jellyfin abre esta pagina al pulsar Ajustes porque su nombre
        // coincide con el nombre del complemento.
        yield return CreatePage(
            name: "JellyInspector",
            displayName: "Configuracion de JellyInspector",
            resource: "Configuration.configPage.html",
            enableInMainMenu: false);

        // Aplicacion principal visible en el menu lateral.
        yield return CreatePage(
            name: "JellyInspectorDashboard",
            displayName: "JellyInspector",
            resource: "Web.pages.dashboard.html",
            enableInMainMenu: true);
    }

    private PluginPageInfo CreatePage(
        string name,
        string displayName,
        string resource,
        bool enableInMainMenu)
    {
        return new PluginPageInfo
        {
            Name = name,
            DisplayName = displayName,
            EnableInMainMenu = enableInMainMenu,
            EmbeddedResourcePath = string.Format(
                CultureInfo.InvariantCulture,
                "{0}.{1}",
                GetType().Namespace,
                resource)
        };
    }
}
