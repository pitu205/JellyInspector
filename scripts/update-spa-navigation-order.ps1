param(
    [string]$Root = "C:\JellyInspector\Application"
)

$ErrorActionPreference = "Stop"

$File = Join-Path $Root "src\Jellyfin.Plugin.JellyInspector\Web\pages\dashboard.html"
$Solution = Join-Path $Root "JellyInspector.sln"
$Stamp = Get-Date -Format "yyyyMMdd-HHmmss"

if (-not (Test-Path $File)) {
    throw "No existe: $File"
}

Copy-Item `
    -Path $File `
    -Destination "$File.$Stamp.nav-order.bak" `
    -Force

$Content = [System.IO.File]::ReadAllText(
    $File,
    [System.Text.Encoding]::UTF8
)

$OldNavPattern = @'
(?s)<div class="ji-spa-links">\s*
<button type="button"\s+data-view="dashboard">Dashboard</button>\s*
<button type="button"\s+data-view="libraries">Biblioteca</button>\s*
<button type="button"\s+data-view="series">Mis\s*series</button>\s*
</div>\s*
</div>\s*
<button type="button"\s+class="ji-spa-scan"\s+data-view="scanner">.*?</button>
'@

$NewNav = @'
<div class="ji-spa-links">
      <button type="button" data-view="dashboard">Dashboard</button>
      <button type="button" data-view="series">Mis series</button>
      <button type="button" data-view="scanner">Escaneo</button>
      <button type="button" data-view="libraries">Biblioteca</button>
    </div>
  </div>
'@

if (-not [regex]::IsMatch($Content, $OldNavPattern)) {
    throw "No se encontro el bloque actual de navegacion."
}

$Content = [regex]::Replace(
    $Content,
    $OldNavPattern,
    $NewNav,
    1
)

$OldActiveCss = @'
(?s)\.ji-spa-nav\[data-active="dashboard"\]\s+\[data-view="dashboard"\],\s*
\.ji-spa-nav\[data-active="libraries"\]\s+\[data-view="libraries"\],\s*
\.ji-spa-nav\[data-active="series"\]\s+\[data-view="series"\]\s*\{
'@

$NewActiveCss = @'
.ji-spa-nav[data-active="dashboard"] [data-view="dashboard"],
.ji-spa-nav[data-active="series"] [data-view="series"],
.ji-spa-nav[data-active="scanner"] [data-view="scanner"],
.ji-spa-nav[data-active="libraries"] [data-view="libraries"] {
'@

if (-not [regex]::IsMatch($Content, $OldActiveCss)) {
    throw "No se encontro el bloque CSS de pestañas activas."
}

$Content = [regex]::Replace(
    $Content,
    $OldActiveCss,
    $NewActiveCss,
    1
)

$Content = [regex]::Replace(
    $Content,
    '(?s)\.ji-spa-scan\s*\{.*?\}\s*',
    '',
    1
)

$Content = [regex]::Replace(
    $Content,
    '(?s)\.ji-spa-nav\[data-active="scanner"\]\s+\.ji-spa-scan\s*\{.*?\}\s*',
    '',
    1
)

$Content = $Content.Replace(
    '.ji-spa-scan{width:100%}',
    ''
)

[System.IO.File]::WriteAllText(
    $File,
    $Content,
    [System.Text.UTF8Encoding]::new($false)
)

Write-Host ""
Write-Host "Navegacion actualizada:"
Write-Host "Dashboard | Mis series | Escaneo | Biblioteca"
Write-Host ""
Write-Host "Compilando..."

dotnet build $Solution

if ($LASTEXITCODE -ne 0) {
    throw "La compilacion ha fallado."
}

Write-Host ""
Write-Host "Compilacion correcta."
