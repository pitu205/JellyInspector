$ErrorActionPreference = "Stop"

$Root = "C:\JellyInspector\Application"
$Version = "0.2.0.0"

$PluginProject = Join-Path $Root "src\Jellyfin.Plugin.JellyInspector\Jellyfin.Plugin.JellyInspector.csproj"
$PluginDll = Join-Path $Root "src\Jellyfin.Plugin.JellyInspector\bin\Release\net9.0\Jellyfin.Plugin.JellyInspector.dll"

$InstallerProject = Join-Path $Root "installer\JellyInspector.Installer.csproj"
$InstallerPayload = Join-Path $Root "installer\Jellyfin.Plugin.JellyInspector.dll"

$ReleaseDir = Join-Path $Root "Release"
$FinalExe = Join-Path $ReleaseDir "JellyInspector-Setup-$Version.exe"

if (-not (Test-Path $PluginProject)) {
    throw "No se encuentra el proyecto del plugin: $PluginProject"
}

if (-not (Test-Path $InstallerProject)) {
    throw "No se encuentra el proyecto del instalador: $InstallerProject"
}

New-Item -ItemType Directory -Path $ReleaseDir -Force | Out-Null

Write-Host "[1/4] Compilando plugin..." -ForegroundColor Cyan
dotnet build $PluginProject -c Release --no-incremental

if ($LASTEXITCODE -ne 0) {
    throw "La compilacion del plugin ha fallado."
}

if (-not (Test-Path $PluginDll)) {
    throw "No se ha generado la DLL del plugin: $PluginDll"
}

Write-Host "[2/4] Actualizando DLL integrada en el instalador..." -ForegroundColor Cyan
Copy-Item $PluginDll $InstallerPayload -Force

Write-Host "[3/4] Generando instalador EXE..." -ForegroundColor Cyan
dotnet publish `
    $InstallerProject `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true

if ($LASTEXITCODE -ne 0) {
    throw "La generacion del instalador ha fallado."
}

$PublishedExe = Join-Path `
    $Root `
    "installer\bin\Release\net9.0-windows\win-x64\publish\JellyInspector-Setup-$Version.exe"

if (-not (Test-Path $PublishedExe)) {
    throw "No se encuentra el EXE publicado: $PublishedExe"
}

Write-Host "[4/4] Copiando instalador final..." -ForegroundColor Cyan
Copy-Item $PublishedExe $FinalExe -Force

$Info = Get-Item $FinalExe
$Hash = (Get-FileHash $FinalExe -Algorithm SHA256).Hash

Write-Host ""
Write-Host "========================================" -ForegroundColor DarkGreen
Write-Host " BUILD COMPLETADO" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor DarkGreen
Write-Host "Archivo: $($Info.FullName)"
Write-Host "Tamano:  $($Info.Length) bytes"
Write-Host "SHA256:  $Hash"
