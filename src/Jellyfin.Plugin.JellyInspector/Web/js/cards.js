function cardHtml(s){
    const issueList=Array.isArray(s.issues)?s.issues:[];

    const issuesHtml=
        issueList
            .slice(0,4)
            .map(issue=>`<div class="ji-issue">${html(issue)}</div>`)
            .join('')+
        (
            issueList.length>4
                ?`<div class="ji-issue more">+${issueList.length-4} incidencias más</div>`
                :''
        );

    const name=html(s.name);
    const id=html(s.id);
    const year=s.year||'—';
    const localSeasons=Number(s.localSeasons||0);
    const localEpisodes=Number(s.localEpisodes||0);
    const missingEpisodes=Number(s.missingEpisodes||0);
    const health=Math.max(0,Math.min(100,Number(s.health||0)));
    const status=String(s.status||'perfect');
    const statusText=statusLabel(status);
    const poster=posterUrl(s);

    const emptyIssuesHtml=
        '<div class="ji-issue" style="color:#76e7ad">Sin incidencias</div>';

    return `
        <article class="ji-series-card">
            <div
                class="ji-poster"
                style="background-image:url(&quot;${poster}&quot;)">
            </div>

            <div class="ji-card-body">
                <div class="ji-card-title">
                    <h3>${name}</h3>
                    <span class="ji-badge ${status}">
                        ${statusText}
                    </span>
                </div>

                <div class="ji-meta">
                    ${year} · ${localSeasons} temporadas · ${localEpisodes} episodios
                </div>

                <div class="ji-card-progress">
                    <div style="width:${health}%"></div>
                </div>

                <div class="ji-card-stats">
                    <span>${health}% completado</span>
                    <span>${missingEpisodes} episodios faltantes</span>
                </div>

                <div class="ji-issues">
                    ${issuesHtml||emptyIssuesHtml}
                </div>

                <div class="ji-card-actions">
                    <button
                        type="button"
                        data-open-jellyfin="${id}">
                        Jellyfin
                    </button>
                </div>
            </div>
        </article>
    `;
}
