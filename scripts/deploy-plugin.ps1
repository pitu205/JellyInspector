param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$Root =
    "C:\JellyInspector\Application"

$Project =
    Join-Path `
        $Root `
        "src\Jellyfin.Plugin.JellyInspector\Jellyfin.Plugin.JellyInspector.csproj"

$SourceDll =
    Join-Path `
        $Root `
        "src\Jellyfin.Plugin.JellyInspector\bin\$Configuration\net9.0\Jellyfin.Plugin.JellyInspector.dll"

$PluginsRoot =
    "C:\ProgramData\Jellyfin\Server\plugins"

$TargetFolder =
    Join-Path `
        $PluginsRoot `
        "JellyInspector_0.2.0.0"

$TargetDll =
    Join-Path `
        $TargetFolder `
        "Jellyfin.Plugin.JellyInspector.dll"

Write-Host ""
Write-Host "=== COMPILANDO PLUGIN ==="

dotnet build `
    $Project `
    -c $Configuration

if ($LASTEXITCODE -ne 0) {
    throw "La compilación del plugin ha fallado."
}

if (-not (Test-Path $SourceDll)) {
    throw "No se ha generado el DLL esperado: $SourceDll"
}

Write-Host ""
Write-Host "=== CERRANDO JELLYFIN ==="

Get-Process jellyfin `
    -ErrorAction SilentlyContinue |
Stop-Process -Force

Start-Sleep -Seconds 2

Write-Host ""
Write-Host "=== LIMPIANDO VERSIONES ANTERIORES ==="

Get-ChildItem `
    $PluginsRoot `
    -Directory `
    -Filter "JellyInspector_*" `
    -ErrorAction SilentlyContinue |
Where-Object {
    $_.FullName -ne $TargetFolder
} |
Remove-Item `
    -Recurse `
    -Force

Write-Host ""
Write-Host "=== COPIANDO PLUGIN ==="

New-Item `
    -Path $TargetFolder `
    -ItemType Directory `
    -Force |
Out-Null

Copy-Item `
    -Path $SourceDll `
    -Destination $TargetDll `
    -Force

$Installed =
    Get-Item $TargetDll

Write-Host ""
Write-Host "Plugin instalado:"
Write-Host $Installed.FullName
Write-Host "Tamaño:" $Installed.Length
Write-Host "Versión:" $Installed.VersionInfo.FileVersion

Write-Host ""
Write-Host "=== INICIANDO JELLYFIN ==="

$PossibleExecutables = @(
    "C:\Program Files\Jellyfin\Server\jellyfin.exe",
    "C:\Program Files\Jellyfin\jellyfin.exe",
    "C:\Jellyfin\Server\jellyfin.exe"
)

$JellyfinExe =
    $PossibleExecutables |
    Where-Object { Test-Path $_ } |
    Select-Object -First 1

if ($JellyfinExe) {
    Start-Process $JellyfinExe

    Write-Host "Jellyfin iniciado desde:"
    Write-Host $JellyfinExe
}
else {
    Write-Warning `
        "No se encontró automáticamente jellyfin.exe. Ábrelo manualmente."
}

Write-Host ""
Write-Host "=== DESPLIEGUE COMPLETADO ==="
