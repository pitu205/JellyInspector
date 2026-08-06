window.JellyInspector ??= {};

JellyInspector.dashboard = {

    async load() {

        try {

            const data =
                await JellyInspector.api.dashboard();

            console.log("Dashboard:", data);

            this.update(data);

        }
        catch (error) {

            console.error(
                "Error cargando Dashboard:",
                error);

        }

    },

    update(data) {

        const series =
            document.getElementById("jiSeries");

        const seasons =
            document.getElementById("jiSeasons");

        const episodes =
            document.getElementById("jiEpisodes");

        if (series)
            series.textContent = data.series;

        if (seasons)
            seasons.textContent = data.seasons;

        if (episodes)
            episodes.textContent = data.episodes;

    }

};

document.addEventListener(
    "pageshow",
    () => JellyInspector.dashboard.load());