param(
    [Parameter(Mandatory = $true)]
    [string]$JellyfinDataPath,

    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $PSScriptRoot
$BuildScript = Join-Path $PSScriptRoot "build-plugin.ps1"

& $BuildScript -Configuration $Configuration

$Source = Join-Path $Root "artifacts\Jellyfin.Plugin.JellyInspector"
$Target = Join-Path $JellyfinDataPath "plugins\JellyInspector_0.1.0.0"

New-Item $Target -ItemType Directory -Force | Out-Null

Copy-Item `
    (Join-Path $Source "Jellyfin.Plugin.JellyInspector.dll") `
    $Target `
    -Force

Write-Host ""
Write-Host "Plugin copiado en:"
Write-Host $Target
Write-Host "Reinicia Jellyfin para cargarlo."
