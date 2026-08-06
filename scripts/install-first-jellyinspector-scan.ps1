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
    -Destination "$File.$Stamp.before-first-scan.bak" `
    -Force

$Content = [System.IO.File]::ReadAllText(
    $File,
    [System.Text.Encoding]::UTF8
)

$ScannerHtml = @'
<!-- JI-FIRST-SCANNER-START -->
<section class="ji-spa-view"
         data-ji-view="scanner"
         hidden>

    <div class="ji-scan-page">
        <header class="ji-scan-header">
            <div>
                <div class="ji-kicker">JELLYINSPECTOR</div>
                <h1>Escanear biblioteca</h1>
                <p>
                    Analiza las bibliotecas seleccionadas y obtiene
                    el total de series, temporadas y episodios.
                </p>
            </div>

            <button id="jiStartFirstScan"
                    type="button"
                    class="ji-scan-primary">
                Iniciar escaneo
            </button>
        </header>

        <section id="jiScanWarning"
                 class="ji-scan-message ji-scan-warning"
                 hidden>
        </section>

        <section id="jiScanError"
                 class="ji-scan-message ji-scan-error"
                 hidden>
        </section>

        <section class="ji-scan-progress-card">
            <div class="ji-scan-progress-top">
                <div>
                    <span class="ji-scan-label">Estado</span>
                    <strong id="jiScanStatus">
                        Preparado para escanear
                    </strong>
                </div>

                <strong id="jiScanPercent">0%</strong>
            </div>

            <div class="ji-scan-progress-track">
                <div id="jiScanProgress"
                     class="ji-scan-progress-bar"
                     style="width:0%">
                </div>
            </div>

            <div id="jiScanCurrent"
                 class="ji-scan-current">
                Selecciona una biblioteca y pulsa Iniciar escaneo.
            </div>
        </section>

        <section class="ji-scan-stats">
            <article>
                <span>Bibliotecas</span>
                <strong id="jiScanLibraries">0</strong>
            </article>

            <article>
                <span>Series</span>
                <strong id="jiScanSeries">0</strong>
            </article>

            <article>
                <span>Temporadas</span>
                <strong id="jiScanSeasons">0</strong>
            </article>

            <article>
                <span>Episodios</span>
                <strong id="jiScanEpisodes">0</strong>
            </article>

            <article>
                <span>Duraci&oacute;n</span>
                <strong id="jiScanDuration">0 s</strong>
            </article>
        </section>

        <section id="jiScanResults"
                 class="ji-scan-results"
                 hidden>
            <div class="ji-scan-results-title">
                <div>
                    <div class="ji-kicker">&Uacute;LTIMO ESCANEO</div>
                    <h2>Resumen por biblioteca</h2>
                </div>

                <span id="jiScanFinishedAt"></span>
            </div>

            <div id="jiScanResultsGrid"
                 class="ji-scan-results-grid">
            </div>
        </section>
    </div>
</section>
<!-- JI-FIRST-SCANNER-END -->
'@

$ScannerCss = @'
<!-- JI-FIRST-SCANNER-CSS-START -->
<style>
.ji-scan-page {
    width: 100%;
}

.ji-scan-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 24px;
    margin-bottom: 22px;
}

.ji-scan-header h1 {
    margin: 4px 0 6px;
    color: #f1f5f9;
    font-size: 2.35rem;
    line-height: 1;
}

.ji-scan-header p {
    max-width: 720px;
    margin: 0;
    color: #8d9bad;
    line-height: 1.5;
}

.ji-scan-primary {
    min-height: 44px;
    padding: 0 20px;
    border: 0;
    border-radius: 10px;
    background: linear-gradient(180deg,#3da5ff,#168df2);
    color: #fff;
    cursor: pointer;
    font-weight: 800;
    box-shadow: 0 8px 22px rgba(22,141,242,.28);
}

.ji-scan-primary:disabled {
    cursor: wait;
    opacity: .58;
}

.ji-scan-progress-card,
.ji-scan-results {
    border: 1px solid rgba(255,255,255,.09);
    border-radius: 16px;
    background: #151b24;
    box-shadow: 0 14px 34px rgba(0,0,0,.14);
}

.ji-scan-progress-card {
    padding: 22px;
}

.ji-scan-progress-top {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
}

.ji-scan-label {
    display: block;
    margin-bottom: 6px;
    color: #8d9bad;
    font-size: .76rem;
}

#jiScanStatus {
    color: #f1f5f9;
    font-size: 1.05rem;
}

#jiScanPercent {
    color: #40d98b;
    font-size: 1.65rem;
}

.ji-scan-progress-track {
    height: 8px;
    margin-top: 18px;
    overflow: hidden;
    border-radius: 999px;
    background: rgba(255,255,255,.08);
}

.ji-scan-progress-bar {
    height: 100%;
    border-radius: inherit;
    background: linear-gradient(90deg,#168df2,#40d98b);
    transition: width .24s ease;
}

.ji-scan-current {
    margin-top: 12px;
    color: #8d9bad;
    font-size: .83rem;
}

.ji-scan-stats {
    display: grid;
    grid-template-columns: repeat(5,minmax(0,1fr));
    gap: 14px;
    margin-top: 16px;
}

.ji-scan-stats article {
    min-height: 92px;
    padding: 18px;
    border: 1px solid rgba(255,255,255,.09);
    border-radius: 14px;
    background: #151b24;
}

.ji-scan-stats span {
    display: block;
    color: #8d9bad;
    font-size: .78rem;
}

.ji-scan-stats strong {
    display: block;
    margin-top: 8px;
    color: #f1f5f9;
    font-size: 1.55rem;
}

.ji-scan-results {
    margin-top: 16px;
    padding: 22px;
}

.ji-scan-results-title {
    display: flex;
    align-items: flex-end;
    justify-content: space-between;
    gap: 16px;
    margin-bottom: 16px;
}

.ji-scan-results-title h2 {
    margin: 5px 0 0;
    color: #f1f5f9;
}

#jiScanFinishedAt {
    color: #8d9bad;
    font-size: .8rem;
}

.ji-scan-results-grid {
    display: grid;
    gap: 10px;
}

.ji-scan-result-row {
    display: grid;
    grid-template-columns: minmax(220px,1.7fr) repeat(3,minmax(90px,.65fr));
    gap: 12px;
    align-items: center;
    padding: 15px 16px;
    border-radius: 11px;
    background: #19212c;
}

.ji-scan-result-name {
    overflow: hidden;
    color: #f1f5f9;
    font-weight: 800;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.ji-scan-result-value {
    text-align: right;
}

.ji-scan-result-value span {
    display: block;
    color: #8d9bad;
    font-size: .69rem;
}

.ji-scan-result-value strong {
    display: block;
    margin-top: 3px;
    color: #f1f5f9;
}

.ji-scan-message {
    margin-bottom: 16px;
    padding: 13px 16px;
    border-radius: 11px;
}

.ji-scan-warning {
    border: 1px solid rgba(255,179,71,.3);
    background: rgba(255,179,71,.1);
    color: #ffc777;
}

.ji-scan-error {
    border: 1px solid rgba(255,102,117,.3);
    background: rgba(255,102,117,.1);
    color: #ffabb3;
}

@media(max-width:900px) {
    .ji-scan-header {
        align-items: stretch;
        flex-direction: column;
    }

    .ji-scan-primary {
        width: 100%;
    }

    .ji-scan-stats {
        grid-template-columns: repeat(2,minmax(0,1fr));
    }

    .ji-scan-result-row {
        grid-template-columns: 1fr;
    }

    .ji-scan-result-value {
        text-align: left;
    }
}
</style>
<!-- JI-FIRST-SCANNER-CSS-END -->
'@

$ScannerJs = @'
<!-- JI-FIRST-SCANNER-JS-START -->
<script type="text/javascript">
(function () {
    const PLUGIN_ID =
        '7d3e3b70-29bc-4e51-b20f-68c416e73a8c';

    const STORAGE_KEY =
        'JellyInspector.FirstScanResult';

    let initialized = false;
    let running = false;

    function byId(id) {
        return document.getElementById(id);
    }

    async function fetchJson(path, query) {
        const response =
            await ApiClient.fetch({
                url: ApiClient.getUrl(path, query),
                type: 'GET'
            });

        if (!response.ok) {
            throw response;
        }

        return await response.json();
    }

    function normalizeId(value) {
        return String(value || '')
            .replaceAll('-', '')
            .trim()
            .toLowerCase();
    }

    function selectedIdsFromConfig(config) {
        return String(
            config.SelectedLibraryIds ||
            config.selectedLibraryIds ||
            '')
            .split(';')
            .map(normalizeId)
            .filter(Boolean);
    }

    function collectionType(folder) {
        return String(
            folder.CollectionType ||
            folder.collectionType ||
            '')
            .trim()
            .toLowerCase();
    }

    function folderId(folder) {
        return normalizeId(
            folder.ItemId ||
            folder.itemId ||
            folder.Id ||
            folder.id);
    }

    function folderName(folder) {
        return String(
            folder.Name ||
            folder.name ||
            'Biblioteca');
    }

    async function itemCount(libraryId, itemType) {
        const result =
            await fetchJson(
                'Items',
                {
                    ParentId: libraryId,
                    UserId:
                        ApiClient.getCurrentUserId(),
                    IncludeItemTypes:
                        itemType,
                    Recursive:
                        true,
                    Limit:
                        0,
                    EnableTotalRecordCount:
                        true
                });

        return Number(
            result.TotalRecordCount ??
            result.totalRecordCount ??
            0);
    }

    function setProgress(value, status, current) {
        const bounded =
            Math.max(
                0,
                Math.min(100, value));

        byId('jiScanProgress').style.width =
            String(bounded) + '%';

        byId('jiScanPercent').textContent =
            String(Math.round(bounded)) + '%';

        if (status) {
            byId('jiScanStatus').textContent =
                status;
        }

        if (current) {
            byId('jiScanCurrent').textContent =
                current;
        }
    }

    function setTotals(result, durationSeconds) {
        byId('jiScanLibraries').textContent =
            String(result.libraries.length);

        byId('jiScanSeries').textContent =
            String(result.totalSeries);

        byId('jiScanSeasons').textContent =
            String(result.totalSeasons);

        byId('jiScanEpisodes').textContent =
            String(result.totalEpisodes);

        byId('jiScanDuration').textContent =
            String(durationSeconds) + ' s';
    }

    function showWarning(message) {
        const element =
            byId('jiScanWarning');

        element.textContent = message;
        element.hidden = false;
    }

    function showError(message) {
        const element =
            byId('jiScanError');

        element.textContent = message;
        element.hidden = false;
    }

    function clearMessages() {
        byId('jiScanWarning').hidden = true;
        byId('jiScanError').hidden = true;
    }

    function renderResults(result) {
        const grid =
            byId('jiScanResultsGrid');

        grid.replaceChildren();

        result.libraries.forEach(
            function (library) {
                const row =
                    document.createElement('div');

                row.className =
                    'ji-scan-result-row';

                const name =
                    document.createElement('div');

                name.className =
                    'ji-scan-result-name';

                name.textContent =
                    library.name;

                row.appendChild(name);

                [
                    ['Series', library.series],
                    ['Temporadas', library.seasons],
                    ['Episodios', library.episodes]
                ].forEach(
                    function (entry) {
                        const cell =
                            document.createElement('div');

                        cell.className =
                            'ji-scan-result-value';

                        const label =
                            document.createElement('span');

                        label.textContent =
                            entry[0];

                        const value =
                            document.createElement('strong');

                        value.textContent =
                            String(entry[1]);

                        cell.appendChild(label);
                        cell.appendChild(value);
                        row.appendChild(cell);
                    });

                grid.appendChild(row);
            });

        byId('jiScanFinishedAt').textContent =
            new Date(result.finishedAt)
                .toLocaleString('es-ES');

        byId('jiScanResults').hidden = false;
    }

    async function startScan() {
        if (running) {
            return;
        }

        running = true;
        clearMessages();

        const button =
            byId('jiStartFirstScan');

        button.disabled = true;
        button.textContent =
            'Escaneando...';

        byId('jiScanResults').hidden = true;

        const startedAt =
            Date.now();

        setProgress(
            2,
            'Preparando escaneo',
            'Leyendo la configuración de JellyInspector...');

        const result = {
            startedAt:
                new Date(startedAt)
                    .toISOString(),
            finishedAt:
                null,
            totalSeries:
                0,
            totalSeasons:
                0,
            totalEpisodes:
                0,
            libraries:
                []
        };

        try {
            const config =
                await fetchJson(
                    'Plugins/' +
                    PLUGIN_ID +
                    '/Configuration');

            const selected =
                selectedIdsFromConfig(config);

            if (selected.length === 0) {
                showWarning(
                    'No hay bibliotecas seleccionadas. Abre Biblioteca y guarda al menos una selección.');

                setProgress(
                    0,
                    'Sin bibliotecas seleccionadas',
                    'No se ha iniciado el escaneo.');

                return;
            }

            const folders =
                await fetchJson(
                    'Library/VirtualFolders');

            const selectedFolders =
                folders.filter(
                    function (folder) {
                        const type =
                            collectionType(folder);

                        return selected.includes(
                            folderId(folder)) &&
                            (
                                type === 'tvshows' ||
                                type === 'tvshow' ||
                                type === 'series'
                            );
                    });

            if (selectedFolders.length === 0) {
                showWarning(
                    'Las bibliotecas seleccionadas ya no están disponibles o no son bibliotecas de series.');

                setProgress(
                    0,
                    'No hay bibliotecas válidas',
                    'Revisa la selección en Biblioteca.');

                return;
            }

            const totalSteps =
                selectedFolders.length * 3;

            let completedSteps = 0;

            for (const folder of selectedFolders) {
                const id =
                    folderId(folder);

                const libraryResult = {
                    id:
                        id,
                    name:
                        folderName(folder),
                    series:
                        0,
                    seasons:
                        0,
                    episodes:
                        0
                };

                const types = [
                    ['Series', 'Series', 'series'],
                    ['Season', 'temporadas', 'seasons'],
                    ['Episode', 'episodios', 'episodes']
                ];

                let inaccessible = false;

                for (const itemType of types) {
                    setProgress(
                        5 +
                        (
                            completedSteps /
                            totalSteps
                        ) * 90,
                        'Escaneando ' +
                        libraryResult.name,
                        'Contando ' +
                        itemType[1] +
                        '...');

                    try {
                        libraryResult[itemType[2]] =
                            await itemCount(
                                id,
                                itemType[0]);
                    }
                    catch (error) {
                        if (error instanceof Response &&
                            (
                                error.status === 401 ||
                                error.status === 403
                            )) {
                            inaccessible = true;
                            break;
                        }

                        throw error;
                    }

                    completedSteps += 1;

                    result.totalSeries +=
                        itemType[2] === 'series'
                            ? libraryResult.series
                            : 0;

                    result.totalSeasons +=
                        itemType[2] === 'seasons'
                            ? libraryResult.seasons
                            : 0;

                    result.totalEpisodes +=
                        itemType[2] === 'episodes'
                            ? libraryResult.episodes
                            : 0;

                    setTotals(
                        result,
                        Math.max(
                            0,
                            Math.round(
                                (
                                    Date.now() -
                                    startedAt
                                ) / 1000)));
                }

                if (inaccessible) {
                    showWarning(
                        'Se omitió al menos una biblioteca porque el usuario actual no tiene permiso para analizarla.');

                    completedSteps +=
                        3 -
                        (
                            libraryResult.series > 0
                                ? 1
                                : 0
                        );
                }
                else {
                    result.libraries.push(
                        libraryResult);
                }
            }

            result.finishedAt =
                new Date().toISOString();

            const durationSeconds =
                Math.max(
                    1,
                    Math.round(
                        (
                            Date.now() -
                            startedAt
                        ) / 1000));

            setTotals(
                result,
                durationSeconds);

            setProgress(
                100,
                'Escaneo completado',
                'El análisis inicial ha finalizado correctamente.');

            localStorage.setItem(
                STORAGE_KEY,
                JSON.stringify(result));

            renderResults(result);
        }
        catch (error) {
            console.error(
                'JellyInspector First Scan:',
                error);

            if (error instanceof Response) {
                showError(
                    'Error HTTP ' +
                    error.status +
                    ' durante el escaneo.');
            }
            else {
                showError(
                    'No se pudo completar el escaneo. ' +
                    String(
                        error &&
                        error.message
                            ? error.message
                            : error));
            }

            setProgress(
                0,
                'Escaneo interrumpido',
                'Revisa el error mostrado.');
        }
        finally {
            running = false;
            button.disabled = false;
            button.textContent =
                'Iniciar escaneo';
        }
    }

    function restoreLastResult() {
        try {
            const raw =
                localStorage.getItem(
                    STORAGE_KEY);

            if (!raw) {
                return;
            }

            const result =
                JSON.parse(raw);

            const durationSeconds =
                Math.max(
                    1,
                    Math.round(
                        (
                            new Date(
                                result.finishedAt
                            ).getTime() -
                            new Date(
                                result.startedAt
                            ).getTime()
                        ) / 1000));

            setTotals(
                result,
                durationSeconds);

            setProgress(
                100,
                'Último escaneo completado',
                'Puedes iniciar un nuevo análisis cuando quieras.');

            renderResults(result);
        }
        catch (error) {
            console.warn(
                'No se pudo restaurar el último escaneo:',
                error);
        }
    }

    function initialize() {
        if (initialized) {
            return;
        }

        const button =
            byId('jiStartFirstScan');

        if (!button) {
            return;
        }

        initialized = true;

        button.addEventListener(
            'click',
            startScan);

        restoreLastResult();
    }

    document.addEventListener(
        'pageshow',
        initialize);

    setTimeout(
        initialize,
        0);
})();
</script>
<!-- JI-FIRST-SCANNER-JS-END -->
'@

$ScannerPattern =
    '(?s)<section class="ji-spa-view"\s+data-ji-view="scanner".*?</section>'

if (-not [regex]::IsMatch(
    $Content,
    $ScannerPattern)) {
    throw "No se encontro la vista scanner de la SPA."
}

$Content = [regex]::Replace(
    $Content,
    $ScannerPattern,
    $ScannerHtml,
    1
)

$OldCssPattern =
    '(?s)<!-- JI-FIRST-SCANNER-CSS-START -->.*?<!-- JI-FIRST-SCANNER-CSS-END -->'

$Content = [regex]::Replace(
    $Content,
    $OldCssPattern,
    ''
)

$OldJsPattern =
    '(?s)<!-- JI-FIRST-SCANNER-JS-START -->.*?<!-- JI-FIRST-SCANNER-JS-END -->'

$Content = [regex]::Replace(
    $Content,
    $OldJsPattern,
    ''
)

$BodyClose =
    $Content.LastIndexOf(
        '</body>',
        [System.StringComparison]::OrdinalIgnoreCase
    )

if ($BodyClose -lt 0) {
    throw "No se encontro </body> en dashboard.html."
}

$Insertion =
    [Environment]::NewLine +
    $ScannerCss +
    [Environment]::NewLine +
    $ScannerJs +
    [Environment]::NewLine

$Content =
    $Content.Insert(
        $BodyClose,
        $Insertion
    )

[System.IO.File]::WriteAllText(
    $File,
    $Content,
    [System.Text.UTF8Encoding]::new($false)
)

Write-Host ""
Write-Host "Vista de primer escaneo instalada."
Write-Host ""
Write-Host "Compilando..."

dotnet build $Solution

if ($LASTEXITCODE -ne 0) {
    throw "La compilacion ha fallado."
}

Write-Host ""
Write-Host "Compilacion correcta."
