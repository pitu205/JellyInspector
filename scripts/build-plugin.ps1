param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $PSScriptRoot
$Project = Join-Path $Root "src\Jellyfin.Plugin.JellyInspector\Jellyfin.Plugin.JellyInspector.csproj"
$Output = Join-Path $Root "artifacts\Jellyfin.Plugin.JellyInspector"

if (Test-Path $Output) {
    Remove-Item $Output -Recurse -Force
}

dotnet publish $Project `
    -c $Configuration `
    -o $Output

Write-Host ""
Write-Host "Plugin compilado en:"
Write-Host $Output
