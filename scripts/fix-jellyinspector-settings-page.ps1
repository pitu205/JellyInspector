param(
    [string]$Root = "C:\JellyInspector\Application"
)

$ErrorActionPreference = "Stop"

$PluginFile = Join-Path $Root "src\Jellyfin.Plugin.JellyInspector\Plugin.cs"
$Solution = Join-Path $Root "JellyInspector.sln"
$Stamp = Get-Date -Format "yyyyMMdd-HHmmss"

if (-not (Test-Path $PluginFile)) {
    throw "No existe: $PluginFile"
}

Copy-Item `
    -Path $PluginFile `
    -Destination "$PluginFile.$Stamp.before-settings-fix.bak" `
    -Force

$PluginSource = @'
using System.Globalization;
using Jellyfin.Plugin.JellyInspector.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.JellyInspector;

public sealed class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public Plugin(
        IApplicationPaths applicationPaths,
        IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public static Plugin? Instance { get; private set; }

    public override string Name => "JellyInspector";

    public override string Description =>
        "Analiza bibliotecas de series y detecta incidencias en Jellyfin.";

    public override Guid Id =>
        Guid.Parse("7d3e3b70-29bc-4e51-b20f-68c416e73a8c");

    public IEnumerable<PluginPageInfo> GetPages()
    {
        // Jellyfin uses the first registered page for the Settings button.
        yield return CreatePage(
            name: "JellyInspectorSettings",
            displayName: "Configuracion de JellyInspector",
            resource: "Configuration.configPage.html",
            enableInMainMenu: false);

        // This is the only entry visible in the left menu.
        yield return CreatePage(
            name: "JellyInspector",
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
'@

[System.IO.File]::WriteAllText(
    $PluginFile,
    $PluginSource,
    [System.Text.UTF8Encoding]::new($false)
)

Write-Host "Plugin.cs corregido."
Write-Host "Configuracion registrada antes que Dashboard."
Write-Host ""
Write-Host "Compilando..."

Push-Location $Root
try {
    dotnet build $Solution

    if ($LASTEXITCODE -ne 0) {
        throw "La compilacion ha fallado."
    }
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "Compilacion correcta."
Write-Host "Ahora despliega el DLL con el procedimiento habitual."
