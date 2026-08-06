param(
    [string]$Version = "0.2.0.0",
    [string]$TargetAbi = "10.11.8.0",
    [string]$Owner = "pitu205"
)

$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $PSScriptRoot
$ZipName = "JellyInspector_$Version.zip"
$ZipPath = Join-Path $Root "artifacts\release\$ZipName"
$ManifestPath = Join-Path $Root "manifest.json"

if (-not (Test-Path $ZipPath)) {
    throw "No se encuentra el paquete: $ZipPath"
}

$Checksum = (Get-FileHash $ZipPath -Algorithm MD5).Hash.ToLowerInvariant()
$Timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
$Tag = "v$Version"

$SourceUrl = "https://github.com/$Owner/JellyInspector/releases/download/$Tag/$ZipName"

$Manifest = @(
    [ordered]@{
        guid = "7d3e3b70-29bc-4e51-b20f-68c416e73a8c"
        name = "JellyInspector"
        overview = "Analiza y supervisa bibliotecas de series en Jellyfin."
        description = "Detecta episodios y temporadas pendientes, compara la colección con TMDb y muestra el estado de la biblioteca mediante un panel integrado."
        owner = $Owner
        category = "General"
        versions = @(
            [ordered]@{
                version = $Version
                changelog = "Primera versión pública de JellyInspector."
                targetAbi = $TargetAbi
                sourceUrl = $SourceUrl
                checksum = $Checksum
                timestamp = $Timestamp
            }
        )
    }
)

$Manifest |
    ConvertTo-Json -Depth 10 |
    Set-Content -Path $ManifestPath -Encoding utf8

Write-Host ""
Write-Host "[OK] manifest.json generado." -ForegroundColor Green
Write-Host "Ruta: $ManifestPath"
Write-Host "URL del ZIP: $SourceUrl"
Write-Host "MD5: $Checksum"