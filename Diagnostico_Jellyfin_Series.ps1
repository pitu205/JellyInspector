$ErrorActionPreference = 'Continue'
$Target = 'F:\Series'
$Out = Join-Path $env:USERPROFILE 'Desktop\Diagnostico_Jellyfin_Series.txt'

function W($s='') { $s | Tee-Object -FilePath $Out -Append }
Remove-Item $Out -Force -ErrorAction SilentlyContinue

W '=== DIAGNOSTICO JELLYFIN - F:\Series ==='
W ("Fecha: {0}" -f (Get-Date))
W ("Equipo: {0}" -f $env:COMPUTERNAME)
W ''

W '--- 1. Ruta y acceso ---'
W ("Existe F:\Series: {0}" -f (Test-Path -LiteralPath $Target -PathType Container))
try {
    $root = Get-Item -LiteralPath $Target -Force -ErrorAction Stop
    W ("Ruta resuelta: {0}" -f $root.FullName)
    W ("Atributos: {0}" -f $root.Attributes)
} catch { W ("ERROR accediendo a la ruta: {0}" -f $_.Exception.Message) }
W ''

W '--- 2. Carpetas de series ---'
try {
    Get-ChildItem -LiteralPath $Target -Directory -Force -ErrorAction Stop |
        Sort-Object Name | ForEach-Object { W ("DIR  {0}" -f $_.FullName) }
} catch { W ("ERROR enumerando carpetas: {0}" -f $_.Exception.Message) }
W ''

W '--- 3. Archivos de video detectables ---'
$videoExt = '.mkv','.mp4','.avi','.m4v','.mov','.ts','.m2ts','.wmv','.mpg','.mpeg','.webm','.strm'
try {
    $videos = Get-ChildItem -LiteralPath $Target -File -Recurse -Force -ErrorAction SilentlyContinue |
        Where-Object { $videoExt -contains $_.Extension.ToLowerInvariant() }
    W ("Total videos: {0}" -f @($videos).Count)
    $videos | Select-Object -First 200 | ForEach-Object { W ("VIDEO {0}" -f $_.FullName) }
    if (@($videos).Count -gt 200) { W '... salida limitada a los primeros 200 videos.' }
} catch { W ("ERROR buscando videos: {0}" -f $_.Exception.Message) }
W ''

W '--- 4. Archivos/atributos que pueden provocar exclusiones ---'
try {
    Get-ChildItem -LiteralPath $Target -Recurse -Force -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -match '^\.ignore$|^\.nomedia$|^\.hidden$|^\.jellyfin' -or ($_.Attributes -band [IO.FileAttributes]::Hidden) } |
        ForEach-Object { W ("SOSPECHOSO {0} [{1}]" -f $_.FullName,$_.Attributes) }
} catch { W ("ERROR comprobando exclusiones: {0}" -f $_.Exception.Message) }
W ''

W '--- 5. Servicio Jellyfin ---'
$svc = Get-CimInstance Win32_Service -Filter "Name='Jellyfin'" -ErrorAction SilentlyContinue
if (-not $svc) { $svc = Get-CimInstance Win32_Service | Where-Object { $_.Name -match 'jellyfin' -or $_.DisplayName -match 'jellyfin' } | Select-Object -First 1 }
if ($svc) {
    W ("Servicio: {0}" -f $svc.Name)
    W ("Estado: {0}" -f $svc.State)
    W ("Cuenta: {0}" -f $svc.StartName)
    W ("Ejecutable: {0}" -f $svc.PathName)
} else { W 'No se encontro un servicio Windows de Jellyfin.' }
W ''

W '--- 6. Configuracion Jellyfin: rutas que contienen F:\ ---'
$searchRoots = @(
    'C:\ProgramData\Jellyfin\Server',
    'C:\ProgramData\Jellyfin',
    (Join-Path $env:LOCALAPPDATA 'Jellyfin'),
    (Join-Path $env:APPDATA 'Jellyfin')
) | Where-Object { Test-Path $_ } | Select-Object -Unique

foreach ($r in $searchRoots) {
    W ("Buscando en: {0}" -f $r)
    Get-ChildItem -LiteralPath $r -File -Recurse -Force -ErrorAction SilentlyContinue |
        Where-Object { $_.Extension -in '.xml','.json','.log' -and $_.Length -lt 50MB } |
        ForEach-Object {
            try {
                $m = Select-String -LiteralPath $_.FullName -Pattern 'F:\\','F:\\Series','J:\\series' -SimpleMatch -ErrorAction SilentlyContinue
                foreach ($x in $m) { W ("MATCH {0}:{1}: {2}" -f $_.FullName,$x.LineNumber,$x.Line.Trim()) }
            } catch {}
        }
}
W ''

W '--- 7. Bibliotecas virtuales (options.xml) ---'
foreach ($r in $searchRoots) {
    Get-ChildItem -LiteralPath $r -Filter options.xml -File -Recurse -Force -ErrorAction SilentlyContinue |
        ForEach-Object {
            W ("OPTIONS {0}" -f $_.FullName)
            try {
                Select-String -LiteralPath $_.FullName -Pattern 'F:\','J:\','Series','series' -SimpleMatch -ErrorAction SilentlyContinue |
                    ForEach-Object { W ("  L{0}: {1}" -f $_.LineNumber,$_.Line.Trim()) }
            } catch {}
        }
}
W ''

W '--- 8. Permisos ACL de F:\Series ---'
try {
    $acl = Get-Acl -LiteralPath $Target -ErrorAction Stop
    W ("Propietario: {0}" -f $acl.Owner)
    foreach ($a in $acl.Access) { W ("ACL {0} | {1} | {2} | inherited={3}" -f $a.IdentityReference,$a.FileSystemRights,$a.AccessControlType,$a.IsInherited) }
} catch { W ("ERROR leyendo ACL: {0}" -f $_.Exception.Message) }
W ''

W '=== FIN ==='
W ("Informe guardado en: {0}" -f $Out)
Write-Host "`nDiagnostico terminado. Informe: $Out" -ForegroundColor Green
Write-Host 'Adjunta Diagnostico_Jellyfin_Series.txt aqui para revisarlo.' -ForegroundColor Cyan
