param(
    [string]$Version = "0.2.0.0",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $PSScriptRoot
$Project = Join-Path $Root "src\Jellyfin.Plugin.JellyInspector\Jellyfin.Plugin.JellyInspector.csproj"
$PublishDir = Join-Path $Root "artifacts\publish\JellyInspector"
$ReleaseDir = Join-Path $Root "artifacts\release"
$ZipName = "JellyInspector_$Version.zip"
$ZipPath = Join-Path $ReleaseDir $ZipName

if (Test-Path $PublishDir) {
    Remove-Item $PublishDir -Recurse -Force
}

if (Test-Path $ReleaseDir) {
    Remove-Item $ReleaseDir -Recurse -Force
}

New-Item -ItemType Directory -Path $PublishDir -Force | Out-Null
New-Item -ItemType Directory -Path $ReleaseDir -Force | Out-Null

Write-Host "[1/4] Restaurando..." -ForegroundColor Cyan
dotnet restore $Project

if ($LASTEXITCODE -ne 0) {
    throw "La restauración ha fallado."
}

Write-Host "[2/4] Publicando plugin..." -ForegroundColor Cyan
dotnet publish $Project `
    -c $Configuration `
    --no-restore `
    -o $PublishDir

if ($LASTEXITCODE -ne 0) {
    throw "La publicación ha fallado."
}

$RequiredFiles = @(
    "Jellyfin.Plugin.JellyInspector.dll",
    "JellyInspector.Scanner.dll"
)

foreach ($File in $RequiredFiles) {
    $Path = Join-Path $PublishDir $File

    if (-not (Test-Path $Path)) {
        throw "Falta el archivo requerido: $File"
    }
}

Get-ChildItem $PublishDir -Filter *.pdb |
    Remove-Item -Force -ErrorAction SilentlyContinue

Write-Host "[3/4] Creando ZIP..." -ForegroundColor Cyan
Compress-Archive `
    -Path (Join-Path $PublishDir "*") `
    -DestinationPath $ZipPath `
    -CompressionLevel Optimal `
    -Force

Write-Host "[4/4] Calculando MD5..." -ForegroundColor Cyan
$Checksum = (Get-FileHash $ZipPath -Algorithm MD5).Hash.ToLowerInvariant()

$Checksum |
    Set-Content `
        -Path (Join-Path $ReleaseDir "$ZipName.md5") `
        -Encoding ascii

Write-Host ""
Write-Host "[OK] Paquete creado" -ForegroundColor Green
Write-Host "ZIP: $ZipPath"
Write-Host "MD5: $Checksum"
