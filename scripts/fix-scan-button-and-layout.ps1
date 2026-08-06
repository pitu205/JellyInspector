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
    -Destination "$File.$Stamp.fix-scan-ui.bak" `
    -Force

$Text = [System.IO.File]::ReadAllText(
    $File,
    [System.Text.Encoding]::UTF8
)

# Elimina bloques anteriores para evitar duplicados.
$Text = [regex]::Replace(
    $Text,
    '(?s)<!-- JI-SCAN-UI-FIX-START -->.*?<!-- JI-SCAN-UI-FIX-END -->',
    ''
)

$Fix = @'
<!-- JI-SCAN-UI-FIX-START -->
<style>
[data-ji-view="scanner"] .ji-scan-page {
    width: 100%;
}

[data-ji-view="scanner"] .ji-scan-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 24px;
    margin-bottom: 22px;
}

[data-ji-view="scanner"] .ji-scan-header h1 {
    margin: 5px 0 8px;
    color: #f1f5f9;
    font-size: clamp(2rem,4vw,3rem);
    line-height: 1;
}

[data-ji-view="scanner"] .ji-scan-header p {
    max-width: 760px;
    margin: 0;
    color: #8d9bad;
    line-height: 1.5;
}

[data-ji-view="scanner"] .ji-scan-primary {
    appearance: none;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-height: 44px;
    padding: 0 22px;
    border: 0;
    border-radius: 10px;
    background: linear-gradient(180deg,#3da5ff,#168df2);
    color: #fff;
    cursor: pointer;
    font: inherit;
    font-weight: 800;
    box-shadow: 0 8px 22px rgba(22,141,242,.28);
    transition:
        transform .16s ease,
        box-shadow .16s ease,
        opacity .16s ease;
}

[data-ji-view="scanner"] .ji-scan-primary:hover {
    transform: translateY(-1px);
    box-shadow: 0 11px 26px rgba(22,141,242,.34);
}

[data-ji-view="scanner"] .ji-scan-primary:disabled {
    cursor: wait;
    opacity: .58;
}

[data-ji-view="scanner"] .ji-scan-progress-card,
[data-ji-view="scanner"] .ji-scan-results {
    border: 1px solid rgba(255,255,255,.09);
    border-radius: 16px;
    background: #151b24;
    box-shadow: 0 14px 34px rgba(0,0,0,.14);
}

[data-ji-view="scanner"] .ji-scan-progress-card {
    padding: 22px;
}

[data-ji-view="scanner"] .ji-scan-progress-top {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
}

[data-ji-view="scanner"] .ji-scan-label {
    display: block;
    margin-bottom: 6px;
    color: #8d9bad;
    font-size: .76rem;
}

[data-ji-view="scanner"] #jiScanStatus {
    color: #f1f5f9;
    font-size: 1.05rem;
}

[data-ji-view="scanner"] #jiScanPercent {
    color: #40d98b;
    font-size: 1.65rem;
}

[data-ji-view="scanner"] .ji-scan-progress-track {
    height: 8px;
    margin-top: 18px;
    overflow: hidden;
    border-radius: 999px;
    background: rgba(255,255,255,.08);
}

[data-ji-view="scanner"] .ji-scan-progress-bar {
    height: 100%;
    border-radius: inherit;
    background: linear-gradient(90deg,#168df2,#40d98b);
    transition: width .24s ease;
}

[data-ji-view="scanner"] .ji-scan-current {
    margin-top: 12px;
    color: #8d9bad;
    font-size: .83rem;
}

[data-ji-view="scanner"] .ji-scan-stats {
    display: grid;
    grid-template-columns: repeat(5,minmax(0,1fr));
    gap: 14px;
    margin-top: 16px;
}

[data-ji-view="scanner"] .ji-scan-stats article {
    min-height: 92px;
    padding: 18px;
    border: 1px solid rgba(255,255,255,.09);
    border-radius: 14px;
    background: #151b24;
}

[data-ji-view="scanner"] .ji-scan-stats span {
    display: block;
    color: #8d9bad;
    font-size: .78rem;
}

[data-ji-view="scanner"] .ji-scan-stats strong {
    display: block;
    margin-top: 8px;
    color: #f1f5f9;
    font-size: 1.55rem;
}

[data-ji-view="scanner"] .ji-scan-results {
    margin-top: 16px;
    padding: 22px;
}

[data-ji-view="scanner"] .ji-scan-results-title {
    display: flex;
    align-items: flex-end;
    justify-content: space-between;
    gap: 16px;
    margin-bottom: 16px;
}

[data-ji-view="scanner"] .ji-scan-results-title h2 {
    margin: 5px 0 0;
    color: #f1f5f9;
}

[data-ji-view="scanner"] .ji-scan-results-grid {
    display: grid;
    gap: 10px;
}

[data-ji-view="scanner"] .ji-scan-result-row {
    display: grid;
    grid-template-columns: minmax(220px,1.7fr) repeat(3,minmax(90px,.65fr));
    gap: 12px;
    align-items: center;
    padding: 15px 16px;
    border-radius: 11px;
    background: #19212c;
}

[data-ji-view="scanner"] .ji-scan-result-name {
    color: #f1f5f9;
    font-weight: 800;
}

[data-ji-view="scanner"] .ji-scan-result-value {
    text-align: right;
}

[data-ji-view="scanner"] .ji-scan-result-value span {
    color: #8d9bad;
    font-size: .69rem;
}

[data-ji-view="scanner"] .ji-scan-result-value strong {
    display: block;
    margin-top: 3px;
    color: #f1f5f9;
}

[data-ji-view="scanner"] .ji-scan-message {
    margin-bottom: 16px;
    padding: 13px 16px;
    border-radius: 11px;
}

[data-ji-view="scanner"] .ji-scan-warning {
    border: 1px solid rgba(255,179,71,.3);
    background: rgba(255,179,71,.1);
    color: #ffc777;
}

[data-ji-view="scanner"] .ji-scan-error {
    border: 1px solid rgba(255,102,117,.3);
    background: rgba(255,102,117,.1);
    color: #ffabb3;
}

@media(max-width:900px) {
    [data-ji-view="scanner"] .ji-scan-header {
        align-items: stretch;
        flex-direction: column;
    }

    [data-ji-view="scanner"] .ji-scan-primary {
        width: 100%;
    }

    [data-ji-view="scanner"] .ji-scan-stats {
        grid-template-columns: repeat(2,minmax(0,1fr));
    }

    [data-ji-view="scanner"] .ji-scan-result-row {
        grid-template-columns: 1fr;
    }

    [data-ji-view="scanner"] .ji-scan-result-value {
        text-align: left;
    }
}
</style>

<script type="text/javascript">
(function () {
    function bindScannerButton() {
        const button =
            document.getElementById('jiStartFirstScan');

        if (!button || button.dataset.scanBound === 'true') {
            return;
        }

        button.dataset.scanBound = 'true';

        button.addEventListener('click', function () {
            if (typeof window.JellyInspectorStartScan === 'function') {
                window.JellyInspectorStartScan();
                return;
            }

            const fallback =
                document.querySelector(
                    '[data-ji-view="scanner"] script');

            console.error(
                'JellyInspector: no se encontró la función de escaneo.',
                fallback
            );
        });
    }

    document.addEventListener('pageshow', bindScannerButton);
    setTimeout(bindScannerButton, 0);
    setTimeout(bindScannerButton, 500);
})();
</script>
<!-- JI-SCAN-UI-FIX-END -->
'@

$BodyClose = $Text.LastIndexOf(
    '</body>',
    [System.StringComparison]::OrdinalIgnoreCase
)

if ($BodyClose -lt 0) {
    throw "No se encontro </body> en dashboard.html."
}

$Text = $Text.Insert(
    $BodyClose,
    [Environment]::NewLine + $Fix + [Environment]::NewLine
)

# Expone startScan para el botón reforzado.
$Text = [regex]::Replace(
    $Text,
    '(?s)(async function startScan\(\)\s*\{)',
    "window.JellyInspectorStartScan = startScan;`r`n`r`n`$1",
    1
)

[System.IO.File]::WriteAllText(
    $File,
    $Text,
    [System.Text.UTF8Encoding]::new($false)
)

Write-Host ""
Write-Host "Estilos y boton de escaneo restaurados."
Write-Host ""
Write-Host "Compilando..."

dotnet build $Solution

if ($LASTEXITCODE -ne 0) {
    throw "La compilacion ha fallado."
}

Write-Host ""
Write-Host "Compilacion correcta."
