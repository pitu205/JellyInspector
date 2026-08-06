param(
    [string]$Root = "C:\JellyInspector\Application"
)

$ErrorActionPreference = "Stop"

$Archivo = Join-Path $Root "src\Jellyfin.Plugin.JellyInspector\Web\pages\dashboard.html"
$Solucion = Join-Path $Root "JellyInspector.sln"
$Marca = Get-Date -Format "yyyyMMdd-HHmmss"

if (-not (Test-Path $Archivo)) {
    throw "No existe: $Archivo"
}

Copy-Item `
    -Path $Archivo `
    -Destination "$Archivo.$Marca.before-scanner-cleanup.bak" `
    -Force

$Texto = [System.IO.File]::ReadAllText(
    $Archivo,
    [System.Text.Encoding]::UTF8
)

function Get-RequiredBlock {
    param(
        [string]$Content,
        [string]$Pattern,
        [string]$Description
    )

    $Match = [regex]::Match($Content, $Pattern)

    if (-not $Match.Success) {
        throw "No se encontro $Description."
    }

    return $Match.Value
}

$PatronV2Html = '(?s)<!-- JI-SCANNER-V2-START -->.*?<!-- JI-SCANNER-V2-END -->'
$PatronV2Css  = '(?s)<!-- JI-SCANNER-V2-CSS-START -->.*?<!-- JI-SCANNER-V2-CSS-END -->'
$PatronV2Js   = '(?s)<!-- JI-SCANNER-V2-JS-START -->.*?<!-- JI-SCANNER-V2-JS-END -->'

$BloqueV2Html = Get-RequiredBlock `
    -Content $Texto `
    -Pattern $PatronV2Html `
    -Description "la vista Scanner V2"

$BloqueV2Css = Get-RequiredBlock `
    -Content $Texto `
    -Pattern $PatronV2Css `
    -Description "el CSS de Scanner V2"

$BloqueV2Js = Get-RequiredBlock `
    -Content $Texto `
    -Pattern $PatronV2Js `
    -Description "el JavaScript de Scanner V2"

# Eliminar bloques antiguos y parches duplicados.
$PatronesAntiguos = @(
    '(?s)<!-- JI-FIRST-SCANNER-START -->.*?<!-- JI-FIRST-SCANNER-END -->',
    '(?s)<!-- JI-FIRST-SCANNER-CSS-START -->.*?<!-- JI-FIRST-SCANNER-CSS-END -->',
    '(?s)<!-- JI-FIRST-SCANNER-JS-START -->.*?<!-- JI-FIRST-SCANNER-JS-END -->',
    '(?s)<!-- JI-SCAN-UI-FIX-START -->.*?<!-- JI-SCAN-UI-FIX-END -->',
    $PatronV2Css,
    $PatronV2Js
)

foreach ($Patron in $PatronesAntiguos) {
    $Texto = [regex]::Replace(
        $Texto,
        $Patron,
        '',
        1
    )
}

# Asegurar una sola vista Scanner V2.
$Texto = [regex]::Replace(
    $Texto,
    $PatronV2Html,
    $BloqueV2Html,
    1
)

# Corregir texto dañado del bloque Scanner V2.
$Reemplazos = [ordered]@{
    'En emisiÃƒÂ³n' = 'En emisión'
    'PrÃƒÂ³ximamente' = 'Próximamente'
    'CrÃƒÂ­ticas' = 'Críticas'
    'DuraciÃƒÂ³n' = 'Duración'
    'AÃƒÂ±o' = 'Año'
    'MÃƒÂ¡s incidencias' = 'Más incidencias'
    'configuraciÃƒÂ³n' = 'configuración'
    'selecciÃƒÂ³n' = 'selección'
    'incidencias mÃ¡s' = 'incidencias más'
    'Â·' = '·'
    'â€”' = '—'
    'Ãšltimo escaneo cargado' = 'Último escaneo cargado'
}

foreach ($Entrada in $Reemplazos.GetEnumerator()) {
    $Texto = $Texto.Replace(
        [string]$Entrada.Key,
        [string]$Entrada.Value
    )
}

# Eliminar una regla CSS incompleta dejada por parches anteriores.
$Texto = [regex]::Replace(
    $Texto,
    '(?m)^\s*\.ji-spa-nav\[data-active="scanner"\]\s*$\r?\n?',
    ''
)

# Insertar CSS y JS V2 dentro de la pagina Jellyfin, antes de cerrar el div raiz.
$PatronCierrePagina = '(?s)(</div>\s*)(<script\s+type="text/javascript"\s+src="\.\./js/api\.js"></script>)'

if (-not [regex]::IsMatch($Texto, $PatronCierrePagina)) {
    throw "No se encontro el cierre de JellyInspectorDashboard anterior a api.js."
}

$Insercion = @"

$BloqueV2Css

$BloqueV2Js

"@

$Texto = [regex]::Replace(
    $Texto,
    $PatronCierrePagina,
    ($Insercion + '$1$2'),
    1
)

# Reforzar visualmente el boton principal aunque Jellyfin aplique estilos propios.
$RefuerzoCss = @'
<!-- JI-SCANNER-V2-BUTTON-FIX-START -->
<style>
#JellyInspectorDashboard #jiv2Start.jiv2-primary {
    appearance: none !important;
    display: inline-flex !important;
    align-items: center !important;
    justify-content: center !important;
    min-height: 44px !important;
    padding: 0 22px !important;
    border: 0 !important;
    border-radius: 10px !important;
    background: linear-gradient(180deg,#3da5ff,#168df2) !important;
    color: #fff !important;
    cursor: pointer !important;
    font: inherit !important;
    font-weight: 800 !important;
    box-shadow: 0 8px 22px rgba(22,141,242,.28) !important;
}

#JellyInspectorDashboard #jiv2Start.jiv2-primary:disabled {
    cursor: wait !important;
    opacity: .58 !important;
}
</style>
<!-- JI-SCANNER-V2-BUTTON-FIX-END -->
'@

$Texto = $Texto.Replace(
    '<!-- JI-SCANNER-V2-JS-START -->',
    ($RefuerzoCss + [Environment]::NewLine + '<!-- JI-SCANNER-V2-JS-START -->')
)

[System.IO.File]::WriteAllText(
    $Archivo,
    $Texto,
    [System.Text.UTF8Encoding]::new($false)
)

Write-Host ""
Write-Host "=== VALIDACION ==="

$Validacion = [System.IO.File]::ReadAllText(
    $Archivo,
    [System.Text.Encoding]::UTF8
)

$Comprobaciones = [ordered]@{
    "Vista Scanner V2" = ([regex]::Matches($Validacion, '<!-- JI-SCANNER-V2-START -->').Count -eq 1)
    "CSS Scanner V2" = ([regex]::Matches($Validacion, '<!-- JI-SCANNER-V2-CSS-START -->').Count -eq 1)
    "JS Scanner V2" = ([regex]::Matches($Validacion, '<!-- JI-SCANNER-V2-JS-START -->').Count -eq 1)
    "Boton jiv2Start" = ($Validacion.Contains('id="jiv2Start"'))
    "Funcion scan" = ($Validacion.Contains('async function scan()'))
    "Sin Scanner antiguo" = (-not $Validacion.Contains('JI-FIRST-SCANNER-START'))
}

foreach ($Comprobacion in $Comprobaciones.GetEnumerator()) {
    $Estado = if ($Comprobacion.Value) { "OK" } else { "ERROR" }
    Write-Host ("[{0}] {1}" -f $Estado, $Comprobacion.Key)

    if (-not $Comprobacion.Value) {
        throw "Ha fallado la validacion: $($Comprobacion.Key)"
    }
}

Write-Host ""
Write-Host "=== COMPILANDO ==="

dotnet build $Solucion

if ($LASTEXITCODE -ne 0) {
    throw "La compilacion ha fallado."
}

Write-Host ""
Write-Host "Scanner V2 limpiado, recolocado y compilado correctamente."
