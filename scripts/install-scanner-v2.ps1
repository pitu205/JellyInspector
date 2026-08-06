param([string]$Root = "C:\JellyInspector\Application")
$ErrorActionPreference = "Stop"
$File = Join-Path $Root "src\Jellyfin.Plugin.JellyInspector\Web\pages\dashboard.html"
if (-not (Test-Path $File)) { throw "No existe: $File" }
$Stamp = Get-Date -Format "yyyyMMdd-HHmmss"
Copy-Item $File "$File.$Stamp.scanner-v2.bak" -Force
$Text = [System.IO.File]::ReadAllText($File,[System.Text.Encoding]::UTF8)
$Scanner = @'
<!-- JI-SCANNER-V2-START -->
<section class="ji-spa-view" data-ji-view="scanner" hidden>
  <div class="jiv2-scan">
    <header class="jiv2-head">
      <div>
        <div class="ji-kicker">JELLYINSPECTOR</div>
        <h1>Escaneo</h1>
        <p>Compara los episodios de Jellyfin con los episodios ya emitidos en TMDb.</p>
      </div>
      <button id="jiv2Start" type="button" class="jiv2-primary">Iniciar escaneo</button>
    </header>

    <section id="jiv2Message" class="jiv2-message" hidden></section>

    <section class="jiv2-health">
      <div class="jiv2-health-main">
        <div class="jiv2-health-icon">&#10003;</div>
        <div>
          <span>Salud global</span>
          <strong id="jiv2HealthText">Sin escanear</strong>
          <small id="jiv2HealthDetail">Inicia un escaneo para analizar la biblioteca.</small>
        </div>
      </div>
      <div class="jiv2-health-score">
        <strong id="jiv2HealthScore">--</strong><span>%</span>
      </div>
      <div class="jiv2-status-grid">
        <div><span class="dot excellent"></span><small>Excelentes</small><strong id="jiv2Excellent">0</strong></div>
        <div><span class="dot perfect"></span><small>Perfectas</small><strong id="jiv2Perfect">0</strong></div>
        <div><span class="dot airing"></span><small>En emisión</small><strong id="jiv2Airing">0</strong></div>
        <div><span class="dot upcoming"></span><small>Próximamente</small><strong id="jiv2Upcoming">0</strong></div>
        <div><span class="dot warning"></span><small>Avisos</small><strong id="jiv2Warnings">0</strong></div>
        <div><span class="dot critical"></span><small>Críticas</small><strong id="jiv2Critical">0</strong></div>
      </div>
      <div class="jiv2-progress"><div id="jiv2Progress" style="width:0%"></div></div>
      <div class="jiv2-progress-line"><span id="jiv2ProgressText">Preparado</span><strong id="jiv2Percent">0%</strong></div>
    </section>

    <section class="jiv2-summary">
      <article><span>Series</span><strong id="jiv2Series">0</strong><small>Total analizadas</small></article>
      <article><span>Episodios</span><strong id="jiv2Episodes">0</strong><small>En Jellyfin</small></article>
      <article><span>Con incidencias</span><strong id="jiv2Affected">0</strong><small>Series afectadas</small></article>
      <article><span>Episodios faltantes</span><strong id="jiv2MissingEpisodes">0</strong><small>No encontrados</small></article>
      <article><span>Temporadas faltantes</span><strong id="jiv2MissingSeasons">0</strong><small>Completas ausentes</small></article>
      <article><span>Duración</span><strong id="jiv2Duration">0 s</strong><small id="jiv2Finished">Sin ejecutar</small></article>
    </section>

    <section id="jiv2Tools" class="jiv2-tools" hidden>
      <div class="jiv2-tools-row">
        <label class="jiv2-search"><span>&#128269;</span><input id="jiv2Search" type="search" placeholder="Buscar serie..."></label>
        <select id="jiv2Status"><option value="all">Todos los estados</option><option value="issues">Con incidencias</option><option value="critical">Críticas</option><option value="warning">Avisos</option><option value="perfect">Perfectas</option><option value="airing">En emisión</option><option value="upcoming">Próximamente</option></select>
        <select id="jiv2Sort"><option value="name">Nombre</option><option value="issues">Más incidencias</option><option value="health">Menor salud</option><option value="year">Año</option></select>
      </div>
      <div class="jiv2-tools-row bottom">
        <label><input id="jiv2Specials" type="checkbox"> Mostrar especiales</label>
        <span id="jiv2ResultCount">0 series</span>
        <div class="jiv2-view-switch"><button id="jiv2CardsBtn" class="active" type="button">Tarjetas</button><button id="jiv2TableBtn" type="button">Tabla</button></div>
        <button id="jiv2Csv" type="button">Exportar CSV</button>
        <button id="jiv2Excel" type="button">Exportar Excel</button>
      </div>
    </section>

    <section id="jiv2Cards" class="jiv2-cards" hidden></section>
    <section id="jiv2TableWrap" class="jiv2-table-wrap" hidden>
      <table class="jiv2-table"><thead><tr><th>Serie</th><th>Año</th><th>Salud</th><th>Episodios</th><th>Faltantes</th><th>Temporadas</th><th>Estado</th></tr></thead><tbody id="jiv2TableBody"></tbody></table>
    </section>
  </div>
</section>
<!-- JI-SCANNER-V2-END -->
'@
$Css = @'
<!-- JI-SCANNER-V2-CSS-START -->
<style>
.jiv2-scan{width:100%;color:#eef4fb}.jiv2-head{display:flex;align-items:center;justify-content:space-between;gap:24px;margin-bottom:20px}.jiv2-head h1{margin:5px 0 8px;font-size:clamp(2rem,4vw,3rem);line-height:1}.jiv2-head p{margin:0;color:#8d9bad}.jiv2-primary{min-height:44px;padding:0 22px;border:0;border-radius:10px;background:linear-gradient(180deg,#3da5ff,#168df2);color:#fff;font-weight:800;cursor:pointer;box-shadow:0 8px 22px rgba(22,141,242,.28)}.jiv2-primary:disabled{opacity:.55;cursor:wait}.jiv2-message{margin-bottom:14px;padding:13px 16px;border:1px solid rgba(255,179,71,.3);border-radius:11px;background:rgba(255,179,71,.1);color:#ffc777}.jiv2-health{padding:22px;border:1px solid rgba(255,255,255,.09);border-radius:16px;background:#151b24;box-shadow:0 14px 34px rgba(0,0,0,.14)}.jiv2-health-main{display:flex;align-items:center;gap:14px}.jiv2-health-icon{display:grid;width:46px;height:46px;place-items:center;border-radius:50%;background:rgba(64,217,139,.14);color:#40d98b;font-size:1.4rem;font-weight:900}.jiv2-health-main span,.jiv2-health-main small{display:block;color:#8d9bad}.jiv2-health-main strong{display:block;margin:3px 0;color:#f1f5f9;font-size:1.15rem}.jiv2-health-score{float:right;margin-top:-52px;color:#40d98b}.jiv2-health-score strong{font-size:2.6rem}.jiv2-health-score span{font-size:1rem}.jiv2-status-grid{display:grid;grid-template-columns:repeat(6,minmax(0,1fr));gap:10px;margin-top:22px}.jiv2-status-grid>div{padding:12px;border-radius:10px;background:#19212c}.jiv2-status-grid small{display:block;color:#8d9bad}.jiv2-status-grid strong{display:block;margin-top:5px;font-size:1.15rem}.dot{display:inline-block;width:8px;height:8px;margin-right:5px;border-radius:50%}.dot.excellent{background:#2ec4ff}.dot.perfect{background:#40d98b}.dot.airing{background:#a970ff}.dot.upcoming{background:#ffc857}.dot.warning{background:#ff9f43}.dot.critical{background:#ff5d73}.jiv2-progress{height:8px;margin-top:18px;overflow:hidden;border-radius:999px;background:rgba(255,255,255,.08)}.jiv2-progress>div{height:100%;border-radius:inherit;background:linear-gradient(90deg,#168df2,#40d98b);transition:width .2s ease}.jiv2-progress-line{display:flex;justify-content:space-between;margin-top:9px;color:#8d9bad;font-size:.8rem}.jiv2-progress-line strong{color:#40d98b}.jiv2-summary{display:grid;grid-template-columns:repeat(6,minmax(0,1fr));gap:14px;margin-top:16px}.jiv2-summary article{padding:18px;border:1px solid rgba(255,255,255,.09);border-radius:14px;background:#151b24}.jiv2-summary span,.jiv2-summary small{display:block;color:#8d9bad}.jiv2-summary strong{display:block;margin:8px 0 4px;font-size:1.65rem}.jiv2-tools{margin-top:16px;padding:15px;border:1px solid rgba(255,255,255,.09);border-radius:14px;background:#151b24}.jiv2-tools-row{display:flex;align-items:center;gap:10px;flex-wrap:wrap}.jiv2-tools-row.bottom{margin-top:12px}.jiv2-search{display:flex;align-items:center;gap:8px;flex:1;min-width:260px;padding:0 12px;border:1px solid rgba(255,255,255,.1);border-radius:9px;background:#101722}.jiv2-search input{width:100%;height:40px;border:0;outline:0;background:transparent;color:#fff}.jiv2-tools select,.jiv2-tools button{height:40px;padding:0 13px;border:1px solid rgba(255,255,255,.1);border-radius:9px;background:#19212c;color:#dce7f3}.jiv2-tools button{cursor:pointer}.jiv2-view-switch{margin-left:auto}.jiv2-view-switch button.active{border-color:#3399ff;background:rgba(51,153,255,.16);color:#69b6ff}.jiv2-cards{display:grid;grid-template-columns:repeat(auto-fill,minmax(260px,1fr));gap:14px;margin-top:16px}.jiv2-card{overflow:hidden;border:1px solid rgba(255,255,255,.09);border-radius:15px;background:#151b24;box-shadow:0 12px 28px rgba(0,0,0,.13)}.jiv2-poster{position:relative;height:245px;background:#101722 center/cover no-repeat}.jiv2-poster:after{content:"";position:absolute;inset:55% 0 0;background:linear-gradient(transparent,#151b24)}.jiv2-card-body{padding:15px}.jiv2-card-title{display:flex;justify-content:space-between;gap:10px}.jiv2-card-title h3{overflow:hidden;margin:0;font-size:1.05rem;text-overflow:ellipsis;white-space:nowrap}.jiv2-badge{flex:0 0 auto;padding:4px 8px;border-radius:999px;font-size:.68rem;font-weight:800}.jiv2-badge.perfect{background:rgba(64,217,139,.13);color:#76e7ad}.jiv2-badge.warning{background:rgba(255,159,67,.13);color:#ffb26b}.jiv2-badge.critical{background:rgba(255,93,115,.13);color:#ff8c9c}.jiv2-badge.airing{background:rgba(169,112,255,.13);color:#c5a0ff}.jiv2-badge.upcoming{background:rgba(255,200,87,.13);color:#ffd77f}.jiv2-meta{margin-top:5px;color:#8d9bad;font-size:.76rem}.jiv2-card-progress{height:6px;margin-top:13px;overflow:hidden;border-radius:999px;background:rgba(255,255,255,.08)}.jiv2-card-progress div{height:100%;background:linear-gradient(90deg,#168df2,#40d98b)}.jiv2-card-stats{display:flex;justify-content:space-between;margin-top:9px;color:#8d9bad;font-size:.75rem}.jiv2-issues{margin-top:12px;padding-top:10px;border-top:1px solid rgba(255,255,255,.07)}.jiv2-issue{margin-top:6px;color:#ff9aaa;font-size:.77rem}.jiv2-issue.more{color:#8d9bad}.jiv2-table-wrap{margin-top:16px;overflow:auto;border:1px solid rgba(255,255,255,.09);border-radius:14px;background:#151b24}.jiv2-table{width:100%;border-collapse:collapse}.jiv2-table th,.jiv2-table td{padding:13px 14px;border-bottom:1px solid rgba(255,255,255,.07);text-align:left}.jiv2-table th{color:#8d9bad;font-size:.72rem;text-transform:uppercase}.jiv2-table td{font-size:.84rem}.jiv2-table tr:last-child td{border-bottom:0}@media(max-width:1000px){.jiv2-status-grid,.jiv2-summary{grid-template-columns:repeat(3,1fr)}}@media(max-width:700px){.jiv2-head{align-items:stretch;flex-direction:column}.jiv2-primary{width:100%}.jiv2-status-grid,.jiv2-summary{grid-template-columns:repeat(2,1fr)}.jiv2-health-score{float:none;margin:14px 0 0}.jiv2-view-switch{margin-left:0}}
</style>
<!-- JI-SCANNER-V2-CSS-END -->
'@
$Js = @'
<!-- JI-SCANNER-V2-JS-START -->
<script type="text/javascript">
(function(){
const PLUGIN_ID='7d3e3b70-29bc-4e51-b20f-68c416e73a8c';
const STORE='JellyInspector.ScanV2';
let all=[], filtered=[], running=false, view='cards';
const $=id=>document.getElementById(id);
const norm=v=>String(v||'').replaceAll('-','').toLowerCase();
async function jf(path,q){const r=await ApiClient.fetch({url:ApiClient.getUrl(path,q),type:'GET'});if(!r.ok)throw r;return r.json();}
async function tmdb(path,key,lang){const u='https://api.themoviedb.org/3'+path+(path.includes('?')?'&':'?')+'api_key='+encodeURIComponent(key)+'&language='+encodeURIComponent(lang||'es-ES');const r=await fetch(u);if(!r.ok)throw new Error('TMDb '+r.status);return r.json();}
function msg(t){const e=$('jiv2Message');e.textContent=t;e.hidden=!t;}
function progress(p,t){$('jiv2Progress').style.width=p+'%';$('jiv2Percent').textContent=Math.round(p)+'%';$('jiv2ProgressText').textContent=t||'';}
function selected(c){return String(c.SelectedLibraryIds||c.selectedLibraryIds||'').split(';').map(norm).filter(Boolean);}
function fId(f){return norm(f.ItemId||f.itemId||f.Id||f.id)}
function fType(f){return String(f.CollectionType||f.collectionType||'').toLowerCase()}
function statusOf(s){if(s.missingSeasons>0||s.missingEpisodes>=6)return'critical';if(s.missingEpisodes>0||s.missingTmdb)return'warning';if(s.upcoming)return'upcoming';if(s.airing)return'airing';return s.health===100?'perfect':'excellent'}
function label(st){return{critical:'Crítica',warning:'Aviso',perfect:'Perfecta',excellent:'Excelente',airing:'En emisión',upcoming:'Próximamente'}[st]||st}
function summarize(){const total=all.length, affected=all.filter(x=>x.missingEpisodes||x.missingSeasons||x.missingTmdb).length, me=all.reduce((a,x)=>a+x.missingEpisodes,0), ms=all.reduce((a,x)=>a+x.missingSeasons,0), eps=all.reduce((a,x)=>a+x.localEpisodes,0), health=total?Math.round(all.reduce((a,x)=>a+x.health,0)/total):0; $('jiv2Series').textContent=total;$('jiv2Episodes').textContent=eps;$('jiv2Affected').textContent=affected;$('jiv2MissingEpisodes').textContent=me;$('jiv2MissingSeasons').textContent=ms;$('jiv2HealthScore').textContent=health;$('jiv2HealthText').textContent=affected?affected+' series con incidencias':'Biblioteca en excelente estado';$('jiv2HealthDetail').textContent=(total-affected)+' de '+total+' series sin incidencias.';['excellent','perfect','airing','upcoming','warning','critical'].forEach(k=>{const id='jiv2'+(k==='warning'?'Warnings':k[0].toUpperCase()+k.slice(1));const e=$(id);if(e)e.textContent=all.filter(x=>x.status===k).length;});}
function apply(){const q=$('jiv2Search').value.trim().toLowerCase(), st=$('jiv2Status').value, sort=$('jiv2Sort').value;filtered=all.filter(x=>(!q||x.name.toLowerCase().includes(q))&&(st==='all'||(st==='issues'?(x.missingEpisodes||x.missingSeasons||x.missingTmdb):x.status===st)));filtered.sort((a,b)=>sort==='issues'?(b.missingEpisodes+b.missingSeasons*10)-(a.missingEpisodes+a.missingSeasons*10):sort==='health'?a.health-b.health:sort==='year'?(b.year||0)-(a.year||0):a.name.localeCompare(b.name,'es'));render();}
function poster(s){return s.poster?'https://image.tmdb.org/t/p/w500'+s.poster:''}
function render(){ $('jiv2ResultCount').textContent=filtered.length+' series';const cards=$('jiv2Cards'),body=$('jiv2TableBody');cards.replaceChildren();body.replaceChildren();filtered.forEach(s=>{const c=document.createElement('article');c.className='jiv2-card';const issues=s.issues.slice(0,3).map(i=>'<div class="jiv2-issue">'+esc(i)+'</div>').join('')+(s.issues.length>3?'<div class="jiv2-issue more">+'+(s.issues.length-3)+' incidencias más</div>':'');c.innerHTML='<div class="jiv2-poster" style="background-image:url(&quot;'+poster(s)+'&quot;)"></div><div class="jiv2-card-body"><div class="jiv2-card-title"><h3>'+esc(s.name)+'</h3><span class="jiv2-badge '+s.status+'">'+label(s.status)+'</span></div><div class="jiv2-meta">'+(s.year||'—')+' · '+s.localSeasons+' temporadas · '+s.localEpisodes+' episodios</div><div class="jiv2-card-progress"><div style="width:'+s.health+'%"></div></div><div class="jiv2-card-stats"><span>'+s.health+'% completado</span><span>Salud '+s.health+'</span></div><div class="jiv2-issues">'+(issues||'<div class="jiv2-issue" style="color:#76e7ad">Sin incidencias</div>')+'</div></div>';cards.appendChild(c);const tr=document.createElement('tr');tr.innerHTML='<td>'+esc(s.name)+'</td><td>'+(s.year||'—')+'</td><td>'+s.health+'%</td><td>'+s.localEpisodes+'</td><td>'+s.missingEpisodes+'</td><td>'+s.missingSeasons+'</td><td><span class="jiv2-badge '+s.status+'">'+label(s.status)+'</span></td>';body.appendChild(tr);});cards.hidden=view!=='cards';$('jiv2TableWrap').hidden=view!=='table';}
function esc(v){const d=document.createElement('div');d.textContent=String(v??'');return d.innerHTML}
async function scan(){if(running)return;running=true;msg('');const btn=$('jiv2Start');btn.disabled=true;btn.textContent='Escaneando...';const started=Date.now();all=[];try{const cfg=await jf('Plugins/'+PLUGIN_ID+'/Configuration');if(!cfg.TmdbApiKey)throw new Error('Falta configurar la clave API de TMDb en la configuración de JellyInspector.');const ids=selected(cfg);if(!ids.length)throw new Error('No hay bibliotecas seleccionadas. Abre Biblioteca y guarda una selección.');const folders=await jf('Library/VirtualFolders');const valid=folders.filter(f=>ids.includes(fId(f))&&['tvshows','tvshow','series'].includes(fType(f)));let series=[];for(const f of valid){const r=await jf('Items',{ParentId:fId(f),UserId:ApiClient.getCurrentUserId(),IncludeItemTypes:'Series',Recursive:true,Fields:'ProviderIds,ProductionYear,ImageTags',Limit:10000});series.push(...(r.Items||r.items||[]));}const unique=[...new Map(series.map(x=>[norm(x.Id||x.id),x])).values()];for(let i=0;i<unique.length;i++){const x=unique[i],id=x.Id||x.id,name=x.Name||x.name||'Serie';progress((i/Math.max(1,unique.length))*96,'Analizando '+name+' ('+(i+1)+'/'+unique.length+')');const items=await jf('Items',{ParentId:id,UserId:ApiClient.getCurrentUserId(),IncludeItemTypes:'Season,Episode',Recursive:true,Fields:'IndexNumber,ParentIndexNumber,ProviderIds',Limit:10000});const arr=items.Items||items.items||[], seasons=arr.filter(z=>String(z.Type||z.type).toLowerCase()==='season'), episodes=arr.filter(z=>String(z.Type||z.type).toLowerCase()==='episode');const local=new Set(episodes.map(e=>(e.ParentIndexNumber??e.parentIndexNumber)+'x'+(e.IndexNumber??e.indexNumber)));const tmdbId=(x.ProviderIds||x.providerIds||{}).Tmdb||(x.ProviderIds||x.providerIds||{}).tmdb;let out={id,name,year:x.ProductionYear||x.productionYear,localSeasons:seasons.filter(s=>(s.IndexNumber??s.indexNumber)!==0).length,localEpisodes:episodes.length,expectedEpisodes:episodes.length,missingEpisodes:0,missingSeasons:0,missingTmdb:!tmdbId,issues:[],poster:null,airing:false,upcoming:false,health:100,status:'perfect'};if(tmdbId){const tv=await tmdb('/tv/'+tmdbId,cfg.TmdbApiKey,cfg.TmdbLanguage);out.poster=tv.poster_path;out.year=out.year||Number(String(tv.first_air_date||'').slice(0,4))||null;out.airing=!!tv.in_production;const today=new Date().toISOString().slice(0,10), includeSpecials=$('jiv2Specials').checked;let expected=0;for(const sn of (tv.seasons||[])){if(sn.season_number===0&&!includeSpecials)continue;if(sn.air_date&&sn.air_date>today){out.upcoming=true;continue;}const sd=await tmdb('/tv/'+tmdbId+'/season/'+sn.season_number,cfg.TmdbApiKey,cfg.TmdbLanguage);const aired=(sd.episodes||[]).filter(e=>e.air_date&&e.air_date<=today);if(!aired.length)continue;expected+=aired.length;let seasonMissing=0;for(const e of aired){if(!local.has(sn.season_number+'x'+e.episode_number)){out.missingEpisodes++;seasonMissing++;out.issues.push('Falta T'+String(sn.season_number).padStart(2,'0')+'E'+String(e.episode_number).padStart(2,'0')+(e.air_date?' · emitido '+formatDate(e.air_date):''));}}if(seasonMissing===aired.length){out.missingSeasons++;out.issues.unshift('Falta la temporada '+sn.season_number+' completa');}}out.expectedEpisodes=expected;out.health=expected?Math.max(0,Math.round(((expected-out.missingEpisodes)/expected)*100)):100;}else out.issues.push('Sin identificador TMDb; no se puede comparar.');out.status=statusOf(out);all.push(out);summarize();}progress(100,'Escaneo completado');const dur=Math.max(1,Math.round((Date.now()-started)/1000));$('jiv2Duration').textContent=dur+' s';$('jiv2Finished').textContent=new Date().toLocaleString('es-ES');localStorage.setItem(STORE,JSON.stringify({date:new Date().toISOString(),duration:dur,series:all}));$('jiv2Tools').hidden=false;apply();}catch(e){console.error(e);msg(e.message||('Error '+(e.status||'')));progress(0,'Escaneo interrumpido');}finally{running=false;btn.disabled=false;btn.textContent='Iniciar escaneo';}}
function formatDate(v){const [y,m,d]=v.split('-');return d+'/'+m+'/'+y}
function exportFile(excel){const rows=[['Serie','Año','Salud','Episodios locales','Episodios faltantes','Temporadas faltantes','Estado'],...filtered.map(s=>[s.name,s.year||'',s.health,s.localEpisodes,s.missingEpisodes,s.missingSeasons,label(s.status)])];if(excel){const html='<table>'+rows.map(r=>'<tr>'+r.map(c=>'<td>'+esc(c)+'</td>').join('')+'</tr>').join('')+'</table>';download('\ufeff'+html,'JellyInspector.xls','application/vnd.ms-excel');}else{const csv='\ufeff'+rows.map(r=>r.map(c=>'"'+String(c).replaceAll('"','""')+'"').join(';')).join('\r\n');download(csv,'JellyInspector.csv','text/csv;charset=utf-8');}}
function download(data,name,type){const a=document.createElement('a');a.href=URL.createObjectURL(new Blob([data],{type}));a.download=name;a.click();setTimeout(()=>URL.revokeObjectURL(a.href),1000)}
function restore(){try{const x=JSON.parse(localStorage.getItem(STORE)||'null');if(!x||!Array.isArray(x.series))return;all=x.series;$('jiv2Duration').textContent=(x.duration||0)+' s';$('jiv2Finished').textContent=new Date(x.date).toLocaleString('es-ES');progress(100,'Último escaneo cargado');$('jiv2Tools').hidden=false;summarize();apply();}catch(e){console.warn(e)}}
function init(){const b=$('jiv2Start');if(!b||b.dataset.bound)return;b.dataset.bound='1';b.addEventListener('click',scan);['jiv2Search','jiv2Status','jiv2Sort'].forEach(id=>$(id).addEventListener(id==='jiv2Search'?'input':'change',apply));$('jiv2Specials').addEventListener('change',()=>msg('El cambio de especiales se aplicará en el próximo escaneo.'));$('jiv2CardsBtn').onclick=()=>{view='cards';$('jiv2CardsBtn').classList.add('active');$('jiv2TableBtn').classList.remove('active');render()};$('jiv2TableBtn').onclick=()=>{view='table';$('jiv2TableBtn').classList.add('active');$('jiv2CardsBtn').classList.remove('active');render()};$('jiv2Csv').onclick=()=>exportFile(false);$('jiv2Excel').onclick=()=>exportFile(true);restore();}
document.addEventListener('pageshow',init);setTimeout(init,0);
})();
</script>
<!-- JI-SCANNER-V2-JS-END -->
'@
$ScannerPatterns = @(
 '(?s)<!-- JI-SCANNER-V2-START -->.*?<!-- JI-SCANNER-V2-END -->',
 '(?s)<!-- JI-FIRST-SCANNER-START -->.*?<!-- JI-FIRST-SCANNER-END -->'
)
$Replaced = $false
foreach($Pattern in $ScannerPatterns){ if([regex]::IsMatch($Text,$Pattern)){ $Text=[regex]::Replace($Text,$Pattern,$Scanner,1);$Replaced=$true;break } }
if(-not $Replaced){ throw "No se encontro la vista de Escaneo en dashboard.html" }
$Text=[regex]::Replace($Text,'(?s)<!-- JI-SCANNER-V2-CSS-START -->.*?<!-- JI-SCANNER-V2-CSS-END -->','')
$Text=[regex]::Replace($Text,'(?s)<!-- JI-SCANNER-V2-JS-START -->.*?<!-- JI-SCANNER-V2-JS-END -->','')
$Text=[regex]::Replace($Text,'(?s)<!-- JI-FIRST-SCANNER-CSS-START -->.*?<!-- JI-FIRST-SCANNER-CSS-END -->','')
$Text=[regex]::Replace($Text,'(?s)<!-- JI-FIRST-SCANNER-JS-START -->.*?<!-- JI-FIRST-SCANNER-JS-END -->','')
$Text=[regex]::Replace($Text,'(?s)<!-- JI-SCAN-UI-FIX-START -->.*?<!-- JI-SCAN-UI-FIX-END -->','')
$Close=$Text.LastIndexOf('</body>',[System.StringComparison]::OrdinalIgnoreCase)
if($Close -lt 0){throw "No se encontro </body>"}
$Insert="`r`n$Css`r`n$Js`r`n"
$Text=$Text.Insert($Close,$Insert)
[System.IO.File]::WriteAllText($File,$Text,[System.Text.UTF8Encoding]::new($false))
Write-Host "Escaneo V2 instalado."
dotnet build (Join-Path $Root "JellyInspector.sln")
if($LASTEXITCODE -ne 0){throw "La compilacion ha fallado"}
Write-Host "Compilacion correcta."
