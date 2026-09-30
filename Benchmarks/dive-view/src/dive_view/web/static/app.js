"use strict";
const $ = id => document.getElementById(id);
const esc = value => String(value ?? "").replace(/[&<>"']/g, c => ({"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;","'":"&#39;"}[c]));
const fmt = (value, digits = 1) => value == null || !Number.isFinite(Number(value)) ? "—" : Number(value).toLocaleString("es-CR", {maximumFractionDigits:digits});
const date = value => value ? new Date(value).toLocaleString("es-CR") : "—";
const rate = value => value == null ? "—" : fmt(value * 100) + "%";
const names = {overview:"Resumen",events:"Eventos",experiments:"Experimentos",scans:"Análisis",rounds:"Rondas",benchmarks:"Benchmarks",resources:"Recursos",systems:"Sistemas",versions:"Versiones",indicators:"Indicadores",settings:"Configuración"};
const descriptions = {events:"Telemetría, eventos de experimentos, cuarentena y fallos de entrega.",experiments:"Sigue los experimentos y sus ejecuciones relacionadas.",scans:"Resultados persistidos, señales de riesgo e indicadores por archivo.",rounds:"Resultados y rendimiento de cada ronda del laboratorio.",benchmarks:"Compara calidad, latencia y rendimiento de ejecuciones etiquetadas.",resources:"Muestras de CPU y memoria registradas por host y benchmark.",systems:"Equipos registrados y su última actividad reportada.",versions:"Versiones de Honey Badger y WALLABEE presentes en la base.",indicators:"Frecuencia y peso de las señales activadas.",settings:"Fuentes de información y configuración de esta instancia."};
const labels = {Id:"ID",FileName:"Archivo",Experiment:"Experimento",Round:"Ronda",Sample:"Muestra",Size:"Bytes",RiskScore:"Riesgo",DetectionLevel:"Nivel",Detected:"Detectado",DurationMs:"Duración (ms)",WallabeeVersion:"WALLABEE",HoneyBadgerVersion:"Honey Badger",Timestamp:"Fecha",ResponseAction:"Respuesta",EventType:"Evento",Source:"Origen",Severity:"Severidad",State:"Estado",ScanId:"Análisis ID",BenchmarkId:"Benchmark ID",Message:"Mensaje",Name:"Nombre",Status:"Estado",Rounds:"Rondas",Scans:"Análisis",SamplesScanned:"Muestras",LabeledSamples:"Etiquetadas",DetectionRate:"Detección",EvasionRate:"Evasión",PrecisionScore:"Precisión",RecallScore:"Recall",F1Score:"F1",P95ScanMs:"P95 (ms)",P99ScanMs:"P99 (ms)",FilesPerSecond:"Archivos/s",MbPerSecond:"MiB/s",CpuAveragePercent:"CPU media (%)",MemoryPeakMb:"RAM pico (MiB)",HostName:"Host",HostAlias:"Alias",OperatingSystem:"Sistema operativo",Architecture:"Arquitectura",CpuLogicalCores:"Núcleos lógicos",TotalMemoryMb:"RAM total (MiB)",EnvironmentName:"Entorno",IsActive:"Activo registrado",CpuPercent:"CPU (%)",MemoryUsedMb:"RAM (MiB)",ProcessCpuPercent:"CPU proceso (%)",ProcessMemoryMb:"RAM proceso (MiB)",HostId:"Host ID",ThreadCount:"Hilos",Component:"Componente",Version:"Versión",IndicatorName:"Indicador",TriggerCount:"Activaciones",DistinctScanCount:"Análisis únicos",AverageWeight:"Peso medio",PercentageOfScans:"% de análisis",Weight:"Peso",Triggered:"Activado",Description:"Descripción"};
const columnSets = {
 events:["EventType","Source","Severity","State","Timestamp","Experiment"],
 scans:["FileName","Experiment","Round","RiskScore","DetectionLevel","DurationMs","Timestamp"],
 experiments:["Experiment","Name","Status","WallabeeVersion","HoneyBadgerVersion","Scans","Rounds"],
 rounds:["Round","SamplesScanned","DetectionRate","F1Score","P95ScanMs","FilesPerSecond","Timestamp"],
 benchmarks:["Experiment","Round","SamplesScanned","LabeledSamples","DetectionRate","F1Score","P95ScanMs","Timestamp"],
 resources:["Timestamp","HostId","CpuPercent","MemoryUsedMb","ProcessCpuPercent","ProcessMemoryMb"],
 systems:["HostName","OperatingSystem","Architecture","CpuLogicalCores","IsActive","Timestamp"],
 versions:["Component","Version","CommitHash","Timestamp"],
 indicators:["IndicatorName","TriggerCount","DistinctScanCount","AverageWeight","PercentageOfScans"]
};
const levels = ["Clean","Low","Medium","High","Critical"];
const colors = ["#88d9af","#8cafee","#dec477","#e6a273","#ed7f83"];
let view = "overview", page = 1, pageSize = 50, rows = [], total = 0, snapshot = null;
let stream, paused = false, requestNumber = 0, loadedRevision = -1, toastTimer, searchTimer;
let tableAbort = null, detailRequest = 0;
function toast(message) { clearTimeout(toastTimer); $("toast").textContent=message; $("toast").hidden=false; toastTimer=setTimeout(()=>$("toast").hidden=true,5000); }
function connection(state,label) { $("connection").className="connection "+state; $("connection").querySelector("span").textContent=label; }
async function json(url, options) { const response=await fetch(url,options); if(!response.ok) { let body={};try{body=await response.json();}catch{} throw Error(typeof body.detail==="string"?body.detail:"No se pudo completar la consulta."); } return response.json(); }
function connect() {
  stream?.close(); connection("connecting","Conectando"); stream=new EventSource("/api/live");
  stream.addEventListener("snapshot",event=>{
    if(paused)return;
    try {
      snapshot=JSON.parse(event.data);
      connection(snapshot.available?"":"offline",snapshot.available?(snapshot.demo?"Demo activa":"En vivo"):"Sin conexión SQL");
      $("mode").textContent=snapshot.demo?"DEMO · DATOS SINTÉTICOS":"SQL SERVER";
      $("notice").hidden=!snapshot.error && !snapshot.demo;
      $("notice").textContent=snapshot.error || "Modo demostración: los datos son sintéticos y no representan el laboratorio real.";
      $("last-update").textContent=snapshot.updatedAt?"Últimos datos · "+date(snapshot.updatedAt):"Esperando sincronización";
      if(snapshot.data)renderOverview(snapshot.data);
      if(columnSets[view] && loadedRevision>=0 && snapshot.revision!==loadedRevision && snapshot.available) {
        $("reload").textContent="Actualización disponible ↻";
      }
    } catch {connection("offline","Datos no disponibles");}
  });
  stream.onerror=()=>{if(!paused)connection("offline","Reconectando");};
}
function chart(id, points, key, unit) {
  const target=$(id), valid=points.filter(p=>p[key]!=null && Number.isFinite(Number(p[key])));
  if(!valid.length){target.innerHTML='<p class="empty-message">Aún no hay mediciones registradas.</p>';target.setAttribute("aria-label","Sin mediciones");return;}
  const max=Math.max(1,...valid.map(p=>Number(p[key])))*1.15, width=680,height=200,left=42,right=12,top=15,bottom=30;
  const innerW=width-left-right, innerH=height-top-bottom;
  const xy=valid.map((p,i)=>[left+(valid.length === 1 ? .5 : i / (valid.length - 1))*innerW,top+innerH*(1-Number(p[key])/max)]);
  const path=xy.map(([x,y],i)=>`${i?'L':'M'}${x.toFixed(2)},${y.toFixed(2)}`).join(' ');
  const grid=[0,1,2,3].map(i=>{const y=top+innerH*i/3;return `<line x1="${left}" y1="${y}" x2="${width-right}" y2="${y}" stroke="#253239" stroke-dasharray="2 4"/><text x="${left-8}" y="${y+3}" text-anchor="end">${fmt(max*(1-i/3),max<10?1:0)}</text>`;}).join('');
  target.innerHTML=`<svg viewBox="0 0 ${width} ${height}" role="presentation"><defs><linearGradient id="${id}-fill" x1="0" y1="0" x2="0" y2="1"><stop offset="0%" stop-color="#b8f476" stop-opacity=".17"/><stop offset="100%" stop-color="#b8f476" stop-opacity="0"/></linearGradient></defs>${grid}<path d="${path} L${xy.at(-1)[0]},${top+innerH} L${xy[0][0]},${top+innerH} Z" fill="url(#${id}-fill)"/><path d="${path}" fill="none" stroke="#b8f476" stroke-width="1.8"/>${xy.map(([x,y],i)=>`<circle cx="${x}" cy="${y}" r="3" fill="#b8f476"><title>${esc(String(valid[i].Day).slice(0,10))}: ${fmt(valid[i][key])} ${unit}</title></circle>`).join('')}<text x="${left}" y="${height-5}">${esc(String(valid[0].Day).slice(0,10))}</text>${valid.length>1?`<text x="${width-right}" y="${height-5}" text-anchor="end">${esc(String(valid.at(-1).Day).slice(0,10))}</text>`:""}</svg>`;
  target.setAttribute("aria-label",`${valid.length} días con datos. Último: ${fmt(valid.at(-1)[key])} ${unit}. Mínimo ${fmt(Math.min(...valid.map(p=>p[key])))}; máximo ${fmt(Math.max(...valid.map(p=>p[key])))}.`);
}
function renderOverview(data) {
  const m=data.metrics;
  $("events-value").textContent=fmt(m.Events,0);$("scans-value").textContent=fmt(m.Scans,0);
  $("experiments-value").textContent=fmt(m.Experiments,0);$("latency-value").textContent=fmt(m.AverageMs,2);
  $("detected-caption").textContent=fmt(m.Detected,0)+" con nivel Medium o superior";
  $("rounds-caption").textContent=fmt(m.Rounds,0)+" rondas registradas";
  chart("volume-chart",data.trends,"Scans","análisis");chart("latency-chart",data.trends,"AverageMs","ms");
  const counts=levels.map(l=>data.risk.find(r=>r.Level===l)?.Count || 0),sum=counts.reduce((a,b)=>a+b,0);
  $("risk-chart").innerHTML=levels.map((l,i)=>`<div class="risk-bar-row"><span><i class="dot" style="color:${colors[i]}"></i>${l}</span><div class="bar-track"><i style="width:${sum?counts[i]/sum*100:0}%;background:${colors[i]}"></i></div><strong>${fmt(counts[i],0)}</strong><small>${sum?fmt(counts[i]/sum*100):0}%</small></div>`).join('');
  $("activity-feed").innerHTML=data.events.length?data.events.map(e=>`<div class="feed-row"><span class="feed-status ${e.State==='Failed'?'failed':'completed'}">${e.State==='Failed'?'!':'↗'}</span><div class="feed-body"><h3>${esc(e.EventType)}</h3><p>${esc(e.Source)} · ${esc(e.State)}</p></div><span class="feed-time">${esc(new Date(e.Timestamp).toLocaleTimeString("es-CR",{hour:"2-digit",minute:"2-digit"}))}</span></div>`).join(''):'<p class="empty-message">No hay eventos persistidos todavía.</p>';
  const b=data.benchmark;
  if(!b){$("benchmark-meta").textContent="Sin benchmark registrado.";$("benchmark-content").innerHTML='<p class="empty-message">Las métricas aparecerán cuando exista una ejecución registrada.</p>';return;}
  $("benchmark-meta").textContent=`${b.Experiment || 'Sin experimento'} / ${b.Round || 'Sin ronda'} · ${fmt(b.LabeledSamples,0)} muestras etiquetadas · ${date(b.Timestamp)}`;
  const stats=[["DETECCIÓN",rate(b.DetectionRate),`TP ${b.TruePositives} · FN ${b.FalseNegatives}`],["PRECISIÓN",rate(b.PrecisionScore),`FP ${b.FalsePositives} · TN ${b.TrueNegatives}`],["F1 SCORE",fmt(b.F1Score,3),"Referencia etiquetada"],["LATENCIA P95",fmt(b.P95ScanMs,2)+" ms","P99 "+fmt(b.P99ScanMs,2)+" ms"],["THROUGHPUT",fmt(b.FilesPerSecond)+" /s",fmt(b.MbPerSecond,2)+" MiB/s"],["EVASIÓN",rate(b.EvasionRate),"Wallabee "+b.WallabeeVersion]];
  $("benchmark-content").innerHTML=stats.map(([label,value,sub])=>`<div class="benchmark-stat"><span>${label}</span><strong>${esc(value)}</strong><small>${esc(sub)}</small></div>`).join('');
}
function writeUrl() {
  const params=new URLSearchParams();if($("search").value)params.set("search",$("search").value);if(page>1)params.set("page",page);if(pageSize!==50)params.set("size",pageSize);if(view==='scans'&&$("level").value)params.set("level",$("level").value);
  history.replaceState(null,"",`#${view}${params.size?'?'+params:''}`);
}
async function route() {
  tableAbort?.abort();clearTimeout(searchTimer);requestNumber++;
  const [candidate,query]=(location.hash.slice(1)||"overview").split('?');view=names[candidate]?candidate:"overview";
  const params=new URLSearchParams(query||'');page=Math.min(1000000,Math.max(1,parseInt(params.get('page'),10)||1));pageSize=[25,50,100].includes(Number(params.get('size')))?Number(params.get('size')):50;
  $("search").value=(params.get('search')||'').slice(0,200);$("size").value=String(pageSize);$("level").value=levels.includes(params.get('level'))?params.get('level'):'';
  $("overview-view").hidden=view!=="overview";$("explorer-view").hidden=!columnSets[view];$("settings-view").hidden=view!=="settings";
  $("section-label").textContent=names[view].toUpperCase();$("title").innerHTML=view==='overview'?'Todo conectado. <span>Todo a la vista.</span>':esc(names[view]);
  $("subtitle").textContent=descriptions[view]||"Explora las señales, los resultados y la historia de cada experimento.";
  document.querySelectorAll('.nav-link').forEach(a=>{a.classList.toggle('active',a.hash==='#'+view);if(a.hash==='#'+view)a.setAttribute('aria-current','page');else a.removeAttribute('aria-current');});
  $("navigation").classList.remove('open');$("menu").setAttribute('aria-expanded','false');
  if(columnSets[view]){$("table-title").textContent=names[view];$("level-field").hidden=view!=='scans';loadedRevision=-1;await loadTable();}
  if(view==='settings')await loadSettings();
}
function cell(key,value) {
  if(value==null)return '—';
  if(key==='Timestamp')return esc(date(value));
  if(['DetectionRate','EvasionRate','PrecisionScore','RecallScore','PercentageOfScans'].includes(key))return rate(value);
  if(key==='DetectionLevel' && levels.includes(value))return `<span class="level ${value}"><i class="dot"></i>${value}</span>`;
  if(typeof value==='boolean')return value?'Sí':'No';
  return typeof value==='number'?fmt(value,3):esc(value);
}
async function loadTable() {
  tableAbort?.abort();tableAbort=new AbortController();const sequence=++requestNumber,kind=view;
  rows=[];$("export").disabled=true;$("previous").disabled=true;$("next").disabled=true;
  const columns=columnSets[kind];$("table-head").innerHTML='<tr>'+columns.map(c=>'<th scope="col">'+esc(labels[c]||c)+'</th>').join('')+'<th scope="col"><span class="sr-only">Detalles</span></th></tr>';
  $("table-body").innerHTML=`<tr><td class="table-empty" colspan="${columns.length+1}">Consultando registros…</td></tr>`;$("table-notice").hidden=true;$("reload").textContent="Actualizar";$("page-info").textContent="Cargando…";
  writeUrl();const params=new URLSearchParams({page,size:pageSize,search:$("search").value,level:kind==='scans'?$("level").value:''});
  try {
    const result=await json('/api/tables/'+kind+'?'+params,{signal:tableAbort.signal});if(sequence!==requestNumber)return;
    rows=result.rows;total=result.total;loadedRevision=snapshot?.revision??0;
    $("table-body").innerHTML=rows.length?rows.map((row,i)=>'<tr>'+columns.map(c=>`<td title="${esc(row[c]??'')}">${cell(c,row[c])}</td>`).join('')+`<td><button class="detail-button" data-row="${i}" aria-label="Abrir detalle del registro ${i+1}">↗</button></td></tr>`).join(''):`<tr><td class="table-empty" colspan="${columns.length+1}">${page>1?'No hay registros en esta página. Vuelve a la página anterior.':$("search").value||$("level").value?'No hay coincidencias con estos filtros.':'Aún no hay registros para esta vista.'}</td></tr>`;
    $("page-info").textContent=`Página ${page} de ${Math.max(1,Math.ceil(total/pageSize))} · ${fmt(total,0)} registros · ${new Date().toLocaleTimeString('es-CR')}`;
    $("table-caption").textContent=`${names[kind]}: página ${page}, ${rows.length} filas de ${total} resultados.`;
    $("previous").disabled=page<=1;$("next").disabled=page*pageSize>=total;$("export").disabled=!rows.length;
  } catch(error) {
    if(sequence!==requestNumber||error.name==='AbortError')return;
    $("table-notice").textContent=error.message;$("table-notice").hidden=false;$("page-info").textContent="Consulta no disponible";
    $("table-body").innerHTML=`<tr><td class="table-empty" colspan="${columns.length+1}">No se pudieron cargar los registros. Usa Actualizar para reintentar.</td></tr>`;
  }
}
async function detail(index) {
  let row=rows[index];if(!row)return;const kind=view,sequence=++detailRequest;
  $("detail-title").textContent=row.FileName||row.EventType||row.Name||names[kind];$("detail-content").textContent="Cargando detalle…";$("detail-dialog").showModal();
  try {
    if(kind==='scans')row=await json('/api/scans/'+encodeURIComponent(row.Id));
    if(sequence!==detailRequest||!$("detail-dialog").open)return;
    $("detail-content").innerHTML='<dl class="detail-meta">'+Object.entries(row).filter(([k])=>k!=='Indicators').map(([k,v])=>`<div><dt>${esc(labels[k]||k)}</dt><dd>${cell(k,v)}</dd></div>`).join('')+'</dl>';
    if(row.Indicators)$("detail-content").innerHTML+='<div class="detail-indicators"><h3>Indicadores · '+row.Indicators.length+'</h3>'+row.Indicators.map(i=>`<div class="indicator"><strong>${esc(i.Name)}</strong><p>${esc(i.Description)}</p><small>${esc(i.Source)} · peso ${fmt(i.Weight)}</small></div>`).join('')+'</div>';
    if(kind==='events'&&row.ScanId){const button=document.createElement('button');button.className='button secondary';button.textContent='Buscar análisis relacionado ↗';button.onclick=()=>{ $("detail-dialog").close(); location.hash='scans?search='+encodeURIComponent(row.ScanId); };$("detail-content").append(button);}
  }catch(error){if(sequence===detailRequest)$("detail-content").textContent=error.message;}
}
async function loadSettings(){
  try{const config=await json('/api/config');$("settings-details").innerHTML=Object.entries({Aplicación:config.application,Versión:config.version,Modo:config.demo?'Demo sintética':'Consulta de datos reales',Base:config.database,Actualización:config.refreshSeconds+' segundos',Acceso:'Solo lectura'}).map(([k,v])=>`<div><dt>${esc(k)}</dt><dd>${esc(v)}</dd></div>`).join('');$("sources").innerHTML=config.sources.map(s=>'<span>'+esc(s)+'</span>').join('');}catch(error){$("settings-details").textContent=error.message;}
}
$("reload").onclick=loadTable;
$("previous").onclick=()=>{if(page>1){page--;loadTable();}};$("next").onclick=()=>{if(page*pageSize<total){page++;loadTable();}};
$("size").onchange=()=>{pageSize=Number($("size").value);page=1;loadTable();};$("level").onchange=()=>{page=1;loadTable();};
$("search").oninput=()=>{clearTimeout(searchTimer);searchTimer=setTimeout(()=>{page=1;loadTable();},300);};
$("table-body").onclick=e=>{const button=e.target.closest('[data-row]');if(button)detail(Number(button.dataset.row));};
$("close-detail").onclick=()=>$("detail-dialog").close();$("detail-dialog").addEventListener('close',()=>detailRequest++);
$("menu").onclick=()=>{const open=$("navigation").classList.toggle('open');$("menu").setAttribute('aria-expanded',String(open));};
$("pause").onclick=()=>{paused=!paused;$("pause").textContent=paused?'▶':'Ⅱ';$("pause").setAttribute('aria-label',paused?'Reanudar actualizaciones':'Pausar actualizaciones');$("pause").title=paused?'Reanudar actualizaciones':'Pausar actualizaciones';if(paused){stream?.close();connection('paused','Vista pausada');toast('La vista está pausada. Los productores del laboratorio siguen registrando datos.');}else connect();};
$("export").onclick=()=>{if(!rows.length)return;const keys=Object.keys(rows[0]);const csvCell=v=>'"'+String(v??'').replace(/^[=+@\-\t\r]/,"'$&").replace(/"/g,'""')+'"';const text=[keys,...rows.map(r=>keys.map(k=>r[k]))].map(row=>row.map(csvCell).join(',')).join('\r\n');const url=URL.createObjectURL(new Blob(['\uFEFF'+text],{type:'text/csv;charset=utf-8'}));const link=document.createElement('a');link.href=url;link.download=`dive-view-${view}-pagina-${page}.csv`;link.click();setTimeout(()=>URL.revokeObjectURL(url),1000);};
window.addEventListener('hashchange',route);window.addEventListener('pagehide',()=>stream?.close());window.addEventListener('pageshow',e=>{if(e.persisted&&!paused)connect();});
route();connect();
