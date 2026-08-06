param(
    [string]$Root = "C:\JellyInspector\Application"
)

$ErrorActionPreference = "Stop"

$PagesRoot = Join-Path $Root "src\Jellyfin.Plugin.JellyInspector\Web\pages"
$Solution = Join-Path $Root "JellyInspector.sln"
$Stamp = Get-Date -Format "yyyyMMdd-HHmmss"

$Pages = @(
    @{
        File = "dashboard.html"
        Active = "dashboard"
    },
    @{
        File = "libraries.html"
        Active = "libraries"
    },
    @{
        File = "scanner.html"
        Active = "scanner"
    },
    @{
        File = "series.html"
        Active = "series"
    }
)

$NavigationTemplate = @'
<!-- JI-NAVIGATION-START -->
<nav class="ji-app-nav"
     data-ji-active="__ACTIVE__">

    <div class="ji-app-nav-left">
        <button type="button"
                class="ji-app-nav-brand"
                data-ji-target="JellyInspector">
            <span class="ji-app-nav-brand-mark">JI</span>
            <span>JellyInspector</span>
        </button>

        <div class="ji-app-nav-links">
            <button type="button"
                    data-ji-page="dashboard"
                    data-ji-target="JellyInspector">
                Dashboard
            </button>

            <button type="button"
                    data-ji-page="libraries"
                    data-ji-target="JellyInspectorLibraries">
                Biblioteca
            </button>

            <button type="button"
                    data-ji-page="series"
                    data-ji-target="JellyInspectorSeries">
                Mis series
            </button>
        </div>
    </div>

    <button type="button"
            class="ji-app-nav-scan"
            data-ji-page="scanner"
            data-ji-target="JellyInspectorScanner">
        <span class="ji-app-nav-scan-icon">&#9906;</span>
        <span>Escanear biblioteca</span>
    </button>
</nav>

<style>
    .ji-app-nav {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 24px;
        width: min(1250px, calc(100% - 48px));
        margin: 0 auto 24px;
        padding: 14px 18px;
        border: 1px solid rgba(255, 255, 255, .09);
        border-radius: 14px;
        background:
            linear-gradient(
                90deg,
                rgba(21, 27, 36, .98),
                rgba(17, 29, 43, .98));
        box-shadow: 0 12px 30px rgba(0, 0, 0, .14);
    }

    .ji-app-nav button {
        appearance: none;
        border: 0;
        cursor: pointer;
        font: inherit;
    }

    .ji-app-nav-left {
        display: flex;
        align-items: center;
        gap: 28px;
        min-width: 0;
    }

    .ji-app-nav-brand {
        display: inline-flex;
        flex: 0 0 auto;
        align-items: center;
        gap: 10px;
        padding: 0;
        background: transparent;
        color: #f1f5f9;
        font-weight: 800;
        letter-spacing: .02em;
    }

    .ji-app-nav-brand-mark {
        display: grid;
        width: 32px;
        height: 32px;
        place-items: center;
        border-radius: 9px;
        background: rgba(51, 153, 255, .16);
        color: #3399ff;
        font-size: .76rem;
        font-weight: 900;
    }

    .ji-app-nav-links {
        display: flex;
        align-items: center;
        gap: 6px;
        flex-wrap: wrap;
    }

    .ji-app-nav-links button {
        min-height: 38px;
        padding: 0 14px;
        border-radius: 9px;
        background: transparent;
        color: #9aa9ba;
        font-size: .87rem;
        font-weight: 700;
        transition:
            background-color .16s ease,
            color .16s ease;
    }

    .ji-app-nav-links button:hover {
        background: rgba(255, 255, 255, .055);
        color: #f1f5f9;
    }

    .ji-app-nav[data-ji-active="dashboard"]
        [data-ji-page="dashboard"],
    .ji-app-nav[data-ji-active="libraries"]
        [data-ji-page="libraries"],
    .ji-app-nav[data-ji-active="series"]
        [data-ji-page="series"] {
        background: rgba(51, 153, 255, .14);
        color: #69b6ff;
        box-shadow:
            inset 0 0 0 1px
            rgba(51, 153, 255, .26);
    }

    .ji-app-nav-scan {
        display: inline-flex;
        flex: 0 0 auto;
        align-items: center;
        justify-content: center;
        gap: 8px;
        min-height: 42px;
        padding: 0 18px;
        border-radius: 10px !important;
        background:
            linear-gradient(
                180deg,
                #3da5ff,
                #168df2) !important;
        color: #fff !important;
        font-size: .86rem;
        font-weight: 800;
        box-shadow:
            0 8px 22px
            rgba(22, 141, 242, .28);
        transition:
            transform .16s ease,
            box-shadow .16s ease;
    }

    .ji-app-nav-scan:hover {
        transform: translateY(-1px);
        box-shadow:
            0 11px 26px
            rgba(22, 141, 242, .34);
    }

    .ji-app-nav[data-ji-active="scanner"]
        .ji-app-nav-scan {
        box-shadow:
            0 0 0 2px rgba(255, 255, 255, .6),
            0 11px 26px rgba(22, 141, 242, .34);
    }

    .ji-app-nav-scan-icon {
        font-size: 1rem;
        line-height: 1;
    }

    @media (max-width: 860px) {
        .ji-app-nav {
            width: min(100% - 26px, 1250px);
            align-items: stretch;
            flex-direction: column;
        }

        .ji-app-nav-left {
            align-items: flex-start;
            flex-direction: column;
            gap: 12px;
        }

        .ji-app-nav-links {
            width: 100%;
        }

        .ji-app-nav-links button {
            flex: 1 1 120px;
        }

        .ji-app-nav-scan {
            width: 100%;
        }
    }
</style>

<script type="text/javascript">
    (() => {
        const currentScript =
            document.currentScript;

        const root =
            currentScript
                ? currentScript
                    .previousElementSibling
                    .previousElementSibling
                : document.querySelector(
                    '.ji-app-nav');

        if (!root ||
            root.dataset.jiBound === 'true') {
            return;
        }

        root.dataset.jiBound = 'true';

        root.querySelectorAll(
            '[data-ji-target]')
            .forEach(button => {
                button.addEventListener(
                    'click',
                    () => {
                        Dashboard.navigate(
                            'configurationpage?name=' +
                            button.dataset.jiTarget);
                    });
            });

        document
            .querySelectorAll('button, a')
            .forEach(element => {
                if (root.contains(element)) {
                    return;
                }

                const text =
                    String(
                        element.textContent || '')
                        .trim()
                        .toLowerCase();

                if (text ===
                    'escanear biblioteca') {
                    element.style.display =
                        'none';
                }
            });
    })();
</script>
<!-- JI-NAVIGATION-END -->
'@

function Replace-Navigation {
    param(
        [string]$Content,
        [string]$Navigation
    )

    $Pattern =
        '(?s)<!-- JI-NAVIGATION-START -->.*?<!-- JI-NAVIGATION-END -->'

    if (-not [regex]::IsMatch(
        $Content,
        $Pattern)) {
        throw "La pagina no contiene el bloque de navegacion esperado."
    }

    return [regex]::Replace(
        $Content,
        $Pattern,
        $Navigation,
        1
    )
}

Write-Host ""
Write-Host "=== ACTUALIZANDO NAVEGACION ==="
Write-Host ""

foreach ($Page in $Pages) {
    $Path =
        Join-Path $PagesRoot $Page.File

    if (-not (Test-Path $Path)) {
        throw "No existe: $Path"
    }

    Copy-Item `
        -Path $Path `
        -Destination "$Path.$Stamp.bak" `
        -Force

    $Content =
        [System.IO.File]::ReadAllText(
            $Path,
            [System.Text.Encoding]::UTF8
        )

    $Navigation =
        $NavigationTemplate.Replace(
            "__ACTIVE__",
            $Page.Active
        )

    $Content =
        Replace-Navigation `
            -Content $Content `
            -Navigation $Navigation

    [System.IO.File]::WriteAllText(
        $Path,
        $Content,
        [System.Text.UTF8Encoding]::new(
            $false)
    )

    Write-Host (
        "Actualizada: " +
        $Page.File)
}

Write-Host ""
Write-Host "=== COMPILANDO ==="
Write-Host ""

dotnet build $Solution

if ($LASTEXITCODE -ne 0) {
    throw "La compilacion ha fallado."
}

Write-Host ""
Write-Host "Navegacion compacta aplicada."
Write-Host "Compilacion correcta."
