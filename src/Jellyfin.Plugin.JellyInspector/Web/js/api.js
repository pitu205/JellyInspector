window.JellyInspector ??= {};

JellyInspector.api = {

    async get(path) {

        const url =
            ApiClient.getUrl(path);

        return await ApiClient.fetch({
            url,
            type: "GET"
        });

    },

    async dashboard() {

        return await this.get(
            "JellyInspector/Dashboard");

    }

};