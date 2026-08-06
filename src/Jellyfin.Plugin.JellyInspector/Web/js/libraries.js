(() => {
    const PLUGIN_ID =
        '7d3e3b70-29bc-4e51-b20f-68c416e73a8c';

    const PAGE_ID =
        'JellyInspectorLibraries';

    let page;
    let elements;
    let libraries = [];
    const selectedIds = new Set();

    function normalizeId(value) {
        return String(value || '')
            .replaceAll('-', '')
            .trim()
            .toLowerCase();
    }

    function getPage() {
        return document.getElementById(PAGE_ID);
    }

    function getElements() {
        return {
            loading:
                page.querySelector('#jiLoading'),

            grid:
                page.querySelector('#jiLibrariesGrid'),

            empty:
                page.querySelector('#jiEmpty'),

            error:
                page.querySelector('#jiError'),

            success:
                page.querySelector('#jiSuccess'),

            count:
                page.querySelector('#jiSelectedCount'),

            refresh:
                page.querySelector('#jiRefreshLibraries'),

            selectAll:
                page.querySelector('#jiSelectAll'),

            clear:
                page.querySelector('#jiClearSelection'),

            save:
                page.querySelector('#jiSave'),

            saveAndScan:
                page.querySelector('#jiSaveAndScan')
        };
    }

    function hideMessages() {
        elements.error.hidden = true;
        elements.success.hidden = true;
    }

    function showError(message) {
        elements.success.hidden = true;
        elements.error.textContent = message;
        elements.error.hidden = false;
    }

    function showSuccess(message) {
        elements.error.hidden = true;
        elements.success.textContent = message;
        elements.success.hidden = false;
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

    async function putJson(path, body) {
        const response =
            await ApiClient.fetch({
                url: ApiClient.getUrl(path),
                type: 'POST',
                data:
                    JSON.stringify(body),
                contentType:
                    'application/json'
            });

        if (!response.ok) {
            throw response;
        }

        return response;
    }

    async function readPluginConfiguration() {
        return await fetchJson(
            'Plugins/' +
            PLUGIN_ID +
            '/Configuration');
    }

    async function savePluginConfiguration(config) {
        return await putJson(
            'Plugins/' +
            PLUGIN_ID +
            '/Configuration',
            config);
    }

    function getSelectedIdsFromConfig(config) {
        return String(
            config.SelectedLibraryIds ||
            config.selectedLibraryIds ||
            '')
            .split(';')
            .map(normalizeId)
            .filter(Boolean);
    }

    function getCollectionType(library) {
        return String(
            library.CollectionType ||
            library.collectionType ||
            '')
            .trim()
            .toLowerCase();
    }

    function isSeriesLibrary(library) {
        const type =
            getCollectionType(library);

        return type === 'tvshows' ||
               type === 'tvshow' ||
               type === 'series';
    }

    function getLibraryId(library) {
        return normalizeId(
            library.ItemId ||
            library.itemId ||
            library.Id ||
            library.id);
    }

    function getLibraryName(library) {
        return String(
            library.Name ||
            library.name ||
            'Biblioteca');
    }

    function getLibraryLocations(library) {
        const locations =
            library.Locations ||
            library.locations ||
            [];

        return Array.isArray(locations)
            ? locations
            : [];
    }

    async function getSeriesCount(libraryId) {
        try {
            const result =
                await fetchJson(
                    'Items',
                    {
                        ParentId:
                            libraryId,

                        UserId:
                            ApiClient
                                .getCurrentUserId(),

                        IncludeItemTypes:
                            'Series',

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
        catch (error) {
            if (error instanceof Response) {
                console.warn(
                    'Biblioteca sin acceso:',
                    libraryId,
                    error.status);
            }
            else {
                console.warn(
                    'No se pudo contar la biblioteca:',
                    libraryId,
                    error);
            }

            return 0;
        }
    }

    function updateSelectionCount() {
        elements.count.textContent =
            String(selectedIds.size);
    }

    function updateCardSelection(card, libraryId) {
        const selected =
            selectedIds.has(libraryId);

        card.classList.toggle(
            'is-selected',
            selected);

        const checkbox =
            card.querySelector(
                '.ji-library-checkbox');

        if (checkbox) {
            checkbox.checked = selected;
        }
    }

    function toggleLibrary(libraryId, card) {
        if (selectedIds.has(libraryId)) {
            selectedIds.delete(libraryId);
        }
        else {
            selectedIds.add(libraryId);
        }

        updateCardSelection(
            card,
            libraryId);

        updateSelectionCount();
        hideMessages();
    }

    function createLibraryCard(library) {
        const card =
            document.createElement('label');

        card.className =
            'ji-library-card';

        card.innerHTML = [
            '<input class="ji-library-checkbox" type="checkbox">',
            '<span class="ji-library-check">✓</span>',
            '<span class="ji-library-icon">▣</span>',
            '<h2 class="ji-library-name"></h2>',
            '<span class="ji-library-count"></span>',
            '<span class="ji-library-path"></span>'
        ].join('');

        card.querySelector(
            '.ji-library-name')
            .textContent =
                library.name;

        card.querySelector(
            '.ji-library-count')
            .textContent =
                String(library.seriesCount) +
                ' serie(s)';

        card.querySelector(
            '.ji-library-path')
            .textContent =
                library.locations.length > 0
                    ? library.locations.join(' · ')
                    : 'Sin ruta informada';

        card.addEventListener(
            'click',
            event => {
                event.preventDefault();

                toggleLibrary(
                    library.id,
                    card);
            });

        updateCardSelection(
            card,
            library.id);

        return card;
    }

    function renderLibraries() {
        elements.grid.replaceChildren();

        for (const library of libraries) {
            elements.grid.appendChild(
                createLibraryCard(library));
        }

        updateSelectionCount();
    }

    async function loadLibraries() {
        hideMessages();

        elements.loading.hidden = false;
        elements.grid.hidden = true;
        elements.empty.hidden = true;

        try {
            const virtualFolders =
                await fetchJson(
                    'Library/VirtualFolders');

            const config =
                await readPluginConfiguration();

            selectedIds.clear();

            for (const id of
                 getSelectedIdsFromConfig(config)) {
                selectedIds.add(id);
            }

            const seriesLibraries =
                virtualFolders
                    .filter(isSeriesLibrary);

            libraries =
                await Promise.all(
                    seriesLibraries.map(
                        async folder => {
                            const id =
                                getLibraryId(folder);

                            return {
                                id:
                                    id,

                                name:
                                    getLibraryName(
                                        folder),

                                locations:
                                    getLibraryLocations(
                                        folder),

                                seriesCount:
                                    await getSeriesCount(
                                        id)
                            };
                        }));

            libraries.sort(
                (left, right) =>
                    left.name.localeCompare(
                        right.name,
                        'es',
                        {
                            sensitivity:
                                'base'
                        }));

            elements.loading.hidden = true;

            if (libraries.length === 0) {
                elements.empty.hidden = false;
                return;
            }

            renderLibraries();
            elements.grid.hidden = false;
        }
        catch (error) {
            elements.loading.hidden = true;

            console.error(
                'JellyInspector Libraries:',
                error);

            if (error instanceof Response) {
                showError(
                    'Error HTTP ' +
                    error.status +
                    ' al cargar las bibliotecas.');

                return;
            }

            showError(
                'No se pudieron cargar las bibliotecas. ' +
                String(
                    error?.message ||
                    error));
        }
    }

    async function saveSelection(goToScanner) {
        hideMessages();
        Dashboard.showLoadingMsg();

        try {
            const config =
                await readPluginConfiguration();

            config.SelectedLibraryIds =
                Array.from(selectedIds)
                    .sort()
                    .join(';');

            await savePluginConfiguration(
                config);

            showSuccess(
                'Selección guardada correctamente.');

            if (goToScanner) {
                setTimeout(
                    () => {
                        Dashboard.navigate(
                            'configurationpage?name=' +
                            'JellyInspectorScanner');
                    },
                    350);
            }
        }
        catch (error) {
            console.error(
                'JellyInspector Save Libraries:',
                error);

            if (error instanceof Response) {
                showError(
                    'Error HTTP ' +
                    error.status +
                    ' al guardar la selección.');
            }
            else {
                showError(
                    'No se pudo guardar la selección. ' +
                    String(
                        error?.message ||
                        error));
            }
        }
        finally {
            Dashboard.hideLoadingMsg();
        }
    }

    function bindEvents() {
        elements.refresh.addEventListener(
            'click',
            loadLibraries);

        elements.selectAll.addEventListener(
            'click',
            () => {
                selectedIds.clear();

                for (const library of libraries) {
                    selectedIds.add(
                        library.id);
                }

                renderLibraries();
                hideMessages();
            });

        elements.clear.addEventListener(
            'click',
            () => {
                selectedIds.clear();
                renderLibraries();
                hideMessages();
            });

        elements.save.addEventListener(
            'click',
            () => saveSelection(false));

        elements.saveAndScan.addEventListener(
            'click',
            () => saveSelection(true));
    }

    function initialize() {
        page = getPage();

        if (!page) {
            console.error(
                'No se encontró la página ' +
                PAGE_ID +
                '.');

            return;
        }

        if (page.dataset.jiInitialized === 'true') {
            loadLibraries();
            return;
        }

        page.dataset.jiInitialized = 'true';
        elements = getElements();

        bindEvents();
        loadLibraries();
    }

    document.addEventListener(
        'pageshow',
        event => {
            const target =
                event.target;

            if (target?.id === PAGE_ID ||
                getPage()) {
                initialize();
            }
        });

    if (document.readyState === 'loading') {
        document.addEventListener(
            'DOMContentLoaded',
            initialize,
            {
                once:
                    true
            });
    }
    else {
        initialize();
    }
})();
