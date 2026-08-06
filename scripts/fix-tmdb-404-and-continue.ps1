param(
    [string]$Root = "C:\JellyInspector\Application"
)

$ErrorActionPreference = "Stop"

$File = Join-Path $Root "src\Jellyfin.Plugin.JellyInspector\Web\pages\dashboard.html"
$Solution = Join-Path $Root "JellyInspector.sln"
$Stamp = Get-Date -Format "yyyyMMdd-HHmmss"

if (-not (Test-Path $File)) {
    throw "No existe dashboard.html: $File"
}

Copy-Item `
    -Path $File `
    -Destination "$File.$Stamp.before-tmdb404-fix.bak" `
    -Force

$Text = [System.IO.File]::ReadAllText(
    $File,
    [System.Text.Encoding]::UTF8
)

$Changes = 0

# 1. Un 404 de TMDb devuelve null en vez de detener todo el escaneo.
$Old = "if(!r.ok)throw new Error('TMDb '+r.status);return r.json();"
$New = "if(r.status===404)return null;if(!r.ok)throw new Error('TMDb '+r.status+' '+path);return r.json();"

if ($Text.Contains($Old)) {
    $Text = $Text.Replace($Old, $New)
    $Changes++
}
else {
    throw "No se encontro la funcion tmdb esperada."
}

# 2. Si TMDb no encuentra una serie, registrar aviso y continuar.
$Old = "const tv=await tmdb('/tv/'+tmdbId,cfg.TmdbApiKey,cfg.TmdbLanguage);out.poster=tv.poster_path;"
$New = "const tv=await tmdb('/tv/'+tmdbId,cfg.TmdbApiKey,cfg.TmdbLanguage);if(!tv){out.missingTmdb=true;out.issues.push('TMDb no encontro esta serie.');out.status=statusOf(out);all.push(out);summarize();continue;}out.poster=tv.poster_path;"

if ($Text.Contains($Old)) {
    $Text = $Text.Replace($Old, $New)
    $Changes++
}
else {
    throw "No se encontro la consulta de serie TMDb esperada."
}

# 3. Si TMDb no encuentra una temporada, omitirla y continuar.
$Old = "const sd=await tmdb('/tv/'+tmdbId+'/season/'+sn.season_number,cfg.TmdbApiKey,cfg.TmdbLanguage);const aired="
$New = "const sd=await tmdb('/tv/'+tmdbId+'/season/'+sn.season_number,cfg.TmdbApiKey,cfg.TmdbLanguage);if(!sd){out.issues.push('TMDb no encontro la temporada '+sn.season_number+'.');continue;}const aired="

if ($Text.Contains($Old)) {
    $Text = $Text.Replace($Old, $New)
    $Changes++
}
else {
    throw "No se encontro la consulta de temporada TMDb esperada."
}

[System.IO.File]::WriteAllText(
    $File,
    $Text,
    [System.Text.UTF8Encoding]::new($false)
)

Write-Host ""
Write-Host "Correcciones aplicadas: $Changes"
Write-Host "Los errores TMDb 404 ya no detendran el escaneo."
Write-Host ""
Write-Host "Compilando..."

dotnet build $Solution

if ($LASTEXITCODE -ne 0) {
    throw "La compilacion ha fallado."
}

Write-Host ""
Write-Host "Compilacion correcta."
