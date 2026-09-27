param(
    [string]$RepoRoot = "C:\JellyInspector2\Application"
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================================"
Write-Host " JellyInspector - JI067A"
Write-Host " Tolerancia a bibliotecas Jellyfin invalidas"
Write-Host "============================================================"
Write-Host ""

$SeriesClient = Join-Path $RepoRoot "src\JellyInspector.Infrastructure\Jellyfin\Clients\SeriesClient.cs"
$Project = Join-Path $RepoRoot "src\Jellyfin.Plugin.JellyInspector\Jellyfin.Plugin.JellyInspector.csproj"
$PluginDll = Join-Path $RepoRoot "src\Jellyfin.Plugin.JellyInspector\bin\Release\net9.0\Jellyfin.Plugin.JellyInspector.dll"

foreach ($Path in @($SeriesClient,$Project)) {
    if (-not (Test-Path $Path)) {
        throw "No se encuentra: $Path"
    }
}

$Content = [IO.File]::ReadAllText($SeriesClient)

if ($Content.Contains("/* JI067A INVALID LIBRARY GUARD START */")) {
    Write-Host "[INFO] JI067A ya esta aplicado. No se duplica."
    exit 0
}

$OldBlock = @'
            foreach (var libraryId in selectedLibraryIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var librarySeries =
                    await GetSeriesFromLibraryAsync(
                        userId,
                        libraryId,
                        cancellationToken);

                allSeries.AddRange(librarySeries);
            }
'@

$Count = ([regex]::Matches(
    $Content,
    [regex]::Escape($OldBlock)
)).Count

if ($Count -ne 1) {
    throw "Se esperaba exactamente 1 bloque de recorrido de bibliotecas y se encontraron $Count."
}

$Stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$Backup = "$SeriesClient.bak-JI067A-$Stamp"
Copy-Item $SeriesClient $Backup -Force

Write-Host "[OK] Backup creado:"
Write-Host "     $Backup"

$NewBlock = @'
            foreach (var libraryId in selectedLibraryIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                /* JI067A INVALID LIBRARY GUARD START */
                try
                {
                    var librarySeries =
                        await GetSeriesFromLibraryAsync(
                            userId,
                            libraryId,
                            cancellationToken);

                    allSeries.AddRange(librarySeries);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"[JELLYFIN LIBRARY WARNING] " +
                        $"Se omite biblioteca {libraryId}: " +
                        $"{ex.Message}");
                }
                /* JI067A INVALID LIBRARY GUARD END */
            }
'@

try {
    $Content = $Content.Replace($OldBlock,$NewBlock)

    [IO.File]::WriteAllText(
        $SeriesClient,
        $Content,
        [Text.UTF8Encoding]::new($false)
    )

    $Verify = [IO.File]::ReadAllText($SeriesClient)

    foreach ($Needle in @(
        "/* JI067A INVALID LIBRARY GUARD START */",
        "catch (OperationCanceledException)",
        "catch (Exception ex)",
        "[JELLYFIN LIBRARY WARNING]",
        "allSeries.AddRange(librarySeries);"
    )) {
        if (-not $Verify.Contains($Needle)) {
            throw "Verificacion fallida: $Needle"
        }
    }

    Write-Host "[OK] JI067A aplicado en SeriesClient.cs."
}
catch {
    Copy-Item $Backup $SeriesClient -Force
    Write-Host "[ERROR] JI067A no se ha aplicado."
    Write-Host "[OK] SeriesClient.cs restaurado."
    throw
}

Write-Host ""
Write-Host "[INFO] Compilando Release..."

dotnet build $Project -c Release

if ($LASTEXITCODE -ne 0) {
    Copy-Item $Backup $SeriesClient -Force
    throw "La compilacion fallo. SeriesClient.cs fue restaurado."
}

if (-not (Test-Path $PluginDll)) {
    Copy-Item $Backup $SeriesClient -Force
    throw "No se encontro la DLL compilada."
}

Write-Host "[OK] Compilacion correcta."

$Roots = @(
    "C:\ProgramData\Jellyfin",
    "$env:LOCALAPPDATA\Jellyfin",
    "$env:APPDATA\Jellyfin"
) | Where-Object { Test-Path $_ }

$Targets = @()
foreach ($Root in $Roots) {
    $Targets += Get-ChildItem `
        -Path $Root `
        -Recurse `
        -Filter "Jellyfin.Plugin.JellyInspector.dll" `
        -File `
        -ErrorAction SilentlyContinue
}
$Targets = @($Targets | Sort-Object FullName -Unique)

if ($Targets.Count -eq 0) {
    Write-Host "[AVISO] Codigo corregido y compilado, pero no se encontro DLL instalada."
    exit 0
}

$Processes = Get-CimInstance Win32_Process -ErrorAction SilentlyContinue |
    Where-Object {
        $_.Name -match "^jellyfin(\.exe)?$" -or
        $_.Name -match "^Jellyfin\.Windows\.Tray(\.exe)?$"
    }

$TrayPath = $null
foreach ($Proc in $Processes) {
    if ($Proc.Name -match "^Jellyfin\.Windows\.Tray" -and $Proc.ExecutablePath) {
        $TrayPath = $Proc.ExecutablePath
    }
}

foreach ($Proc in $Processes) {
    Stop-Process -Id $Proc.ProcessId -Force -ErrorAction Stop
    Write-Host "[OK] Cerrado: $($Proc.Name) PID $($Proc.ProcessId)"
}

if ($Processes) {
    Start-Sleep -Seconds 3
}

$SourceHash = (Get-FileHash $PluginDll -Algorithm SHA256).Hash
$DeployStamp = Get-Date -Format "yyyyMMdd-HHmmss"

foreach ($Target in $Targets) {
    $TargetBackup = "$($Target.FullName).bak-JI067A-$DeployStamp"

    Copy-Item $Target.FullName $TargetBackup -Force
    Copy-Item $PluginDll $Target.FullName -Force

    $TargetHash = (Get-FileHash $Target.FullName -Algorithm SHA256).Hash

    if ($TargetHash -ne $SourceHash) {
        throw "La DLL desplegada no coincide con la compilada."
    }

    Write-Host "[OK] DLL desplegada:"
    Write-Host "     $($Target.FullName)"
    Write-Host "[OK] SHA256 verificado."
}

if ($TrayPath -and (Test-Path $TrayPath)) {
    Start-Process -FilePath $TrayPath
}
else {
    $DefaultTray = "C:\Program Files\Jellyfin\Server\jellyfin-windows-tray\Jellyfin.Windows.Tray.exe"

    if (Test-Path $DefaultTray) {
        Start-Process -FilePath $DefaultTray
    }
}

Write-Host ""
Write-Host "============================================================"
Write-Host " JI067A COMPLETADO"
Write-Host "============================================================"
Write-Host ""
Write-Host "Nuevo comportamiento:"
Write-Host " - Una biblioteca inexistente o sin permisos ya no aborta el escaneo."
Write-Host " - JellyInspector la omite y continua con las bibliotecas validas."
Write-Host " - La cancelacion manual sigue funcionando normalmente."
Write-Host ""
Write-Host "Prueba ahora de nuevo Escanear serie."
Write-Host ""
