param(
    [string]$Root = "C:\JellyInspector\Application"
)

$ErrorActionPreference = "Stop"

$PluginRoot = Join-Path $Root "src\Jellyfin.Plugin.JellyInspector"
$PluginFile = Join-Path $PluginRoot "Plugin.cs"
$PagesRoot = Join-Path $PluginRoot "Web\pages"
$Solution = Join-Path $Root "JellyInspector.sln"
$Stamp = Get-Date -Format "yyyyMMdd-HHmmss"

if (-not (Test-Path $PluginFile)) {
    throw "No existe Plugin.cs: $PluginFile"
}

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
<nav class="ji-app-nav" data-ji-active="__ACTIVE__">
    <button type="button"
            class="ji-app-nav-brand"
            data-ji-page="dashboard"
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
                data-ji-page="scanner"
                data-ji-target="JellyInspectorScanner">
            Escanear series
        </button>

        <button type="button"
                data-ji-page="series"
                data-ji-target="JellyInspectorSeries">
            Mis series
        </button>
    </div>
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
        background: rgba(21, 27, 36, .96);
        box-shadow: 0 12px 30px rgba(0, 0, 0, .14);
    }

    .ji-app-nav button {
        appearance: none;
        border: 0;
        cursor: pointer;
        font: inherit;
    }

    .ji-app-nav-brand {
        display: inline-flex;
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
    .ji-app-nav[data-ji-active="scanner"]
        [data-ji-page="scanner"],
    .ji-app-nav[data-ji-active="series"]
        [data-ji-page="series"] {
        background: rgba(51, 153, 255, .14);
        color: #69b6ff;
        box-shadow: inset 0 0 0 1px rgba(51, 153, 255, .26);
    }

    @media (max-width: 820px) {
        .ji-app-nav {
            width: min(100% - 26px, 1250px);
            align-items: flex-start;
            flex-direction: column;
        }

        .ji-app-nav-links {
            width: 100%;
        }

        .ji-app-nav-links button {
            flex: 1 1 150px;
        }
    }
</style>

<script type="text/javascript">
    (() => {
        const nav =
            document.currentScript
                ? document.currentScript
                    .previousElementSibling
                : null;

        const root =
            nav && nav.matches('style')
                ? nav.previousElementSibling
                : document.querySelector('.ji-app-nav');

        if (!root || root.dataset.jiBound === 'true') {
            return;
        }

        root.dataset.jiBound = 'true';

        root.querySelectorAll('[data-ji-target]')
            .forEach(button => {
                button.addEventListener(
                    'click',
                    () => {
                        const target =
                            button.dataset.jiTarget;

                        Dashboard.navigate(
                            'configurationpage?name=' +
                            target);
                    });
            });
    })();
</script>
<!-- JI-NAVIGATION-END -->
'@

function Remove-ExistingNavigation {
    param(
        [string]$Content
    )

    return [regex]::Replace(
        $Content,
        '(?s)<!-- JI-NAVIGATION-START -->.*?<!-- JI-NAVIGATION-END -->',
        ''
    )
}

function Insert-Navigation {
    param(
        [string]$Content,
        [string]$Navigation
    )

    $Pattern = '(?is)(<div\b[^>]*data-role=["'']content["''][^>]*>)'

    if ([regex]::IsMatch($Content, $Pattern)) {
        return [regex]::Replace(
            $Content,
            $Pattern,
            ('$1' + [Environment]::NewLine + $Navigation),
            1
        )
    }

    $PagePattern = '(?is)(<div\b[^>]*data-role=["'']page["''][^>]*>)'

    if ([regex]::IsMatch($Content, $PagePattern)) {
        return [regex]::Replace(
            $Content,
            $PagePattern,
            ('$1' + [Environment]::NewLine + $Navigation),
            1
        )
    }

    $BodyPattern = '(?is)(<body[^>]*>)'

    if ([regex]::IsMatch($Content, $BodyPattern)) {
        return [regex]::Replace(
            $Content,
            $BodyPattern,
            ('$1' + [Environment]::NewLine + $Navigation),
            1
        )
    }

    $FirstDivPattern = '(?is)(<div\b[^>]*>)'

    if ([regex]::IsMatch($Content, $FirstDivPattern)) {
        return [regex]::Replace(
            $Content,
            $FirstDivPattern,
            ('$1' + [Environment]::NewLine + $Navigation),
            1
        )
    }

    throw "No se encontro un punto donde insertar la navegacion."
}

Write-Host ""
Write-Host "=== LIMPIANDO MENU LATERAL ==="
Write-Host ""

Copy-Item `
    -Path $PluginFile `
    -Destination "$PluginFile.$Stamp.bak" `
    -Force

$PluginContent = [System.IO.File]::ReadAllText(
    $PluginFile,
    [System.Text.Encoding]::UTF8
)

$PluginContent = [regex]::Replace(
    $PluginContent,
    '(?s)(name:\s*"JellyInspectorLibraries".*?enableInMainMenu:\s*)true',
    '${1}false'
)

$PluginContent = [regex]::Replace(
    $PluginContent,
    '(?s)(name:\s*"JellyInspectorScanner".*?enableInMainMenu:\s*)true',
    '${1}false'
)

$PluginContent = [regex]::Replace(
    $PluginContent,
    '(?s)(name:\s*"JellyInspectorSeries".*?enableInMainMenu:\s*)true',
    '${1}false'
)

[System.IO.File]::WriteAllText(
    $PluginFile,
    $PluginContent,
    [System.Text.UTF8Encoding]::new($false)
)

Write-Host "Solo JellyInspector permanecera en el menu lateral."

Write-Host ""
Write-Host "=== ANADIENDO NAVEGACION INTERNA ==="
Write-Host ""

foreach ($Page in $Pages) {
    $Path = Join-Path $PagesRoot $Page.File

    if (-not (Test-Path $Path)) {
        throw "No existe la pagina: $Path"
    }

    Copy-Item `
        -Path $Path `
        -Destination "$Path.$Stamp.bak" `
        -Force

    $Content = [System.IO.File]::ReadAllText(
        $Path,
        [System.Text.Encoding]::UTF8
    )

    $Content = Remove-ExistingNavigation `
        -Content $Content

    $Navigation = $NavigationTemplate.Replace(
        "__ACTIVE__",
        $Page.Active
    )

    $Content = Insert-Navigation `
        -Content $Content `
        -Navigation $Navigation

    [System.IO.File]::WriteAllText(
        $Path,
        $Content,
        [System.Text.UTF8Encoding]::new($false)
    )

    Write-Host ("Actualizada: " + $Page.File)
}

Write-Host ""
Write-Host "=== COMPILANDO ==="
Write-Host ""

dotnet build $Solution

if ($LASTEXITCODE -ne 0) {
    throw "La compilacion ha fallado."
}

Write-Host ""
Write-Host "Navegacion aplicada y solucion compilada."
Write-Host ""
Write-Host "Ahora despliega el DLL con el procedimiento manual habitual."
