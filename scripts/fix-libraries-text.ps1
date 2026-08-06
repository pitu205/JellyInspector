param(
    [string]$Root = "C:\JellyInspector\Application"
)

$ErrorActionPreference = "Stop"

$File = Join-Path $Root "src\Jellyfin.Plugin.JellyInspector\Web\pages\libraries.html"

if (-not (Test-Path $File)) {
    throw "No existe: $File"
}

$Backup = "$File.textfix.bak"
Copy-Item -Path $File -Destination $Backup -Force

$Text = [System.IO.File]::ReadAllText(
    $File,
    [System.Text.Encoding]::UTF8
)

$Text = [regex]::Replace(
    $Text,
    'Solo se analizar[^<\r\n]*las bibliotecas marcadas\.',
    'Solo se analizar&aacute;n las bibliotecas marcadas.'
)

$Text = [regex]::Replace(
    $Text,
    'como programas de televis[^<\r\n]*\.',
    'como programas de televisi&oacute;n.'
)

$Text = [regex]::Replace(
    $Text,
    'Guardar selecci[^<\r\n]*',
    'Guardar selecci&oacute;n'
)

$Text = [regex]::Replace(
    $Text,
    '<div class="ji-empty-icon">.*?</div>',
    '<div class="ji-empty-icon">&#9632;</div>'
)

$Text = [regex]::Replace(
    $Text,
    '<span class="ji-library-check">.*?</span>',
    '<span class="ji-library-check">&#10003;</span>'
)

$Text = [regex]::Replace(
    $Text,
    '<span class="ji-library-icon">.*?</span>',
    '<span class="ji-library-icon">&#9632;</span>'
)

$Text = [regex]::Replace(
    $Text,
    "\?\s*library\.locations\.join\('.*?'\)",
    "? library.locations.join(' \u00b7 ')"
)

$Text = [regex]::Replace(
    $Text,
    "'Selecci[^']*guardada correctamente\.'",
    "'Selecci\u00f3n guardada correctamente.'"
)

$Text = [regex]::Replace(
    $Text,
    "' al guardar la selecci[^']*\.'",
    "' al guardar la selecci\u00f3n.'"
)

$Text = [regex]::Replace(
    $Text,
    "'No se pudo guardar la selecci[^']*\. '",
    "'No se pudo guardar la selecci\u00f3n. '"
)

$Text = [regex]::Replace(
    $Text,
    "'No se encontr[^']*la p[^']*gina '",
    "'No se encontr\u00f3 la p\u00e1gina '"
)

[System.IO.File]::WriteAllText(
    $File,
    $Text,
    [System.Text.UTF8Encoding]::new($false)
)

Write-Host ""
Write-Host "Archivo corregido:"
Write-Host $File
Write-Host ""
Write-Host "Copia de seguridad:"
Write-Host $Backup
Write-Host ""
Write-Host "Compilando..."

dotnet build (Join-Path $Root "JellyInspector.sln")

if ($LASTEXITCODE -ne 0) {
    throw "La compilacion ha fallado."
}

Write-Host ""
Write-Host "Compilacion correcta."
