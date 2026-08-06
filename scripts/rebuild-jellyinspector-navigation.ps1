param(
    [string]$Root = "C:\JellyInspector\Application"
)

$ErrorActionPreference = "Stop"

$PagesRoot = Join-Path $Root "src\Jellyfin.Plugin.JellyInspector\Web\pages"
$Solution = Join-Path $Root "JellyInspector.sln"
$Stamp = Get-Date -Format "yyyyMMdd-HHmmss"

$Pages = @(
    @{ File = "dashboard.html"; Active = "dashboard" },
    @{ File = "libraries.html"; Active = "libraries" },
    @{ File = "scanner.html"; Active = "scanner" },
    @{ File = "series.html"; Active = "series" }
)

$Template = @'
<!-- JI-NAVIGATION-START -->
<nav id="jiAppNav"
     class="ji-app-nav"
     data-active="__ACTIVE__">

    <div class="ji-app-nav-left">
        <button type="button"
                class="ji-app-brand"
                data-target="JellyInspector">
            <span class="ji-app-brand-icon">JI</span>
            <span>JellyInspector</span>
        </button>

        <div class="ji-app-links">
            <button type="button"
                    data-page="dashboard"
                    data-target="JellyInspector">
                Dashboard
            </button>

            <button type="button"
                    data-page="libraries"
                    data-target="JellyInspectorLibraries">
                Biblioteca
            </button>

            <button type="button"
                    data-page="series"
                    data-target="JellyInspectorSeries">
                Mis series
            </button>
        </div>
    </div>

    <button type="button"
            class="ji-app-scan"
            data-page="scanner"
            data-target="JellyInspectorScanner">
        <span aria-hidden="true">&#9906;</span>
        <span>Escanear biblioteca</span>
    </button>
</nav>

<style>
#jiAppNav {
    box-sizing: border-box;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 24px;
    width: min(1250px, calc(100% - 48px));
    margin: 0 auto 24px;
    padding: 14px 18px;
    border: 1px solid rgba(255,255,255,.09);
    border-radius: 14px;
    background: linear-gradient(90deg,#151b24,#111d2b);
    box-shadow: 0 12px 30px rgba(0,0,0,.14);
}

#jiAppNav button {
    appearance: none;
    border: 0;
    cursor: pointer;
    font: inherit;
}

#jiAppNav .ji-app-nav-left {
    display: flex;
    align-items: center;
    gap: 28px;
    min-width: 0;
}

#jiAppNav .ji-app-brand {
    display: inline-flex;
    flex: 0 0 auto;
    align-items: center;
    gap: 10px;
    padding: 0;
    background: transparent;
    color: #f1f5f9;
    font-weight: 800;
}

#jiAppNav .ji-app-brand-icon {
    display: grid;
    width: 32px;
    height: 32px;
    place-items: center;
    border-radius: 9px;
    background: rgba(51,153,255,.16);
    color: #3399ff;
    font-size: .76rem;
    font-weight: 900;
}

#jiAppNav .ji-app-links {
    display: flex;
    align-items: center;
    gap: 6px;
    flex-wrap: wrap;
}

#jiAppNav .ji-app-links button {
    min-height: 38px;
    padding: 0 14px;
    border-radius: 9px;
    background: transparent;
    color: #9aa9ba;
    font-size: .87rem;
    font-weight: 700;
}

#jiAppNav .ji-app-links button:hover {
    background: rgba(255,255,255,.055);
    color: #f1f5f9;
}

#jiAppNav[data-active="dashboard"] [data-page="dashboard"],
#jiAppNav[data-active="libraries"] [data-page="libraries"],
#jiAppNav[data-active="series"] [data-page="series"] {
    background: rgba(51,153,255,.14);
    color: #69b6ff;
    box-shadow: inset 0 0 0 1px rgba(51,153,255,.26);
}

#jiAppNav .ji-app-scan {
    display: inline-flex;
    flex: 0 0 auto;
    align-items: center;
    justify-content: center;
    gap: 8px;
    min-height: 42px;
    padding: 0 18px;
    border-radius: 10px;
    background: linear-gradient(180deg,#3da5ff,#168df2);
    color: #fff;
    font-size: .86rem;
    font-weight: 800;
    box-shadow: 0 8px 22px rgba(22,141,242,.28);
}

#jiAppNav[data-active="scanner"] .ji-app-scan {
    box-shadow:
        0 0 0 2px rgba(255,255,255,.6),
        0 11px 26px rgba(22,141,242,.34);
}

@media (max-width:860px) {
    #jiAppNav {
        width: min(100% - 26px,1250px);
        align-items: stretch;
        flex-direction: column;
    }

    #jiAppNav .ji-app-nav-left {
        align-items: flex-start;
        flex-direction: column;
        gap: 12px;
    }

    #jiAppNav .ji-app-links {
        width: 100%;
    }

    #jiAppNav .ji-app-links button {
        flex: 1 1 120px;
    }

    #jiAppNav .ji-app-scan {
        width: 100%;
    }
}
</style>

<script type="text/javascript">
(function () {
    function setupJellyInspectorNav() {
        const nav = document.getElementById('jiAppNav');

        if (!nav) {
            return;
        }

        if (nav.dataset.bound !== 'true') {
            nav.dataset.bound = 'true';

            nav.querySelectorAll('[data-target]')
                .forEach(function (button) {
                    button.addEventListener('click', function () {
                        Dashboard.navigate(
                            'configurationpage?name=' +
                            button.dataset.target
                        );
                    });
                });
        }

        document.querySelectorAll('button, a')
            .forEach(function (element) {
                if (nav.contains(element)) {
                    return;
                }

                const label = String(element.textContent || '')
                    .replace(/\s+/g, ' ')
                    .trim()
                    .toLowerCase();

                if (label === 'escanear biblioteca') {
                    element.hidden = true;
                    element.style.display = 'none';
                }
            });
    }

    document.addEventListener('pageshow', setupJellyInspectorNav);
    setTimeout(setupJellyInspectorNav, 0);
    setTimeout(setupJellyInspectorNav, 500);
})();
</script>
<!-- JI-NAVIGATION-END -->
'@

$MarkerPattern =
    '(?s)<!-- JI-NAVIGATION-START -->.*?<!-- JI-NAVIGATION-END -->'

foreach ($Page in $Pages) {
    $Path = Join-Path $PagesRoot $Page.File

    if (-not (Test-Path $Path)) {
        throw "No existe: $Path"
    }

    Copy-Item `
        -Path $Path `
        -Destination "$Path.$Stamp.bak" `
        -Force

    $Content = [System.IO.File]::ReadAllText(
        $Path,
        [System.Text.Encoding]::UTF8
    )

    $Navigation = $Template.Replace(
        "__ACTIVE__",
        $Page.Active
    )

    if ([regex]::IsMatch($Content, $MarkerPattern)) {
        $Content = [regex]::Replace(
            $Content,
            $MarkerPattern,
            $Navigation,
            1
        )
    }
    else {
        $ContentPattern =
            '(?is)(<div\b[^>]*data-role=["'']content["''][^>]*>)'

        if (-not [regex]::IsMatch($Content, $ContentPattern)) {
            throw "No se encontro el bloque de navegacion ni data-role=content en $($Page.File)."
        }

        $Content = [regex]::Replace(
            $Content,
            $ContentPattern,
            ('$1' + [Environment]::NewLine + $Navigation),
            1
        )
    }

    [System.IO.File]::WriteAllText(
        $Path,
        $Content,
        [System.Text.UTF8Encoding]::new($false)
    )

    Write-Host ("Actualizada: " + $Page.File)
}

Write-Host ""
Write-Host "Compilando..."
dotnet build $Solution

if ($LASTEXITCODE -ne 0) {
    throw "La compilacion ha fallado."
}

Write-Host ""
Write-Host "Navegacion reconstruida correctamente."
