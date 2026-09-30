"use strict";
const $ = id => document.getElementById(id);
const levels = ["Clean", "Low", "Medium", "High", "Critical"];
const colors = ["#88d9af", "#8cafee", "#dec477", "#e6a273", "#ed7f83"];
const escapeHtml = value => String(value ?? "").replace(/[&<>"']/g, c => ({"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;","'":"&#39;"}[c]));
const number = (n, digits = 1) => n == null || !Number.isFinite(Number(n)) ? "—" : Number(n).toLocaleString("es-CR", {maximumFractionDigits: digits});
const time = date => date ? new Date(date).toLocaleTimeString("es-CR", {hour12:false,hour:"2-digit",minute:"2-digit",second:"2-digit"}) : "—";
const percent = value => value == null ? "—" : number(value * 100, 1) + "%";
const detected = scan => levels.indexOf(scan.detectionLevel) >= 2;
const kindName = kind => ({file:"Archivo", directory:"Carpeta", benchmark:"Benchmark"}[kind] || kind);
let snapshot = null, filter = "all", paused = false, stream = null, toastTimer = null, busy = false;
let previousScans = "", previousFeed = "", previousBenchmark = "";

function toast(message, error = false) {
  clearTimeout(toastTimer); $("toast").textContent = message; $("toast").classList.toggle("error", error); $("toast").hidden = false;
  toastTimer = setTimeout(() => $("toast").hidden = true, error ? 8500 : 5000);
}
function connection(state, text) {
  $("connection").className = "connection " + state;
  $("connection").querySelector("span").textContent = text;
}
function connect() {
  if (stream) stream.close();
  connection("connecting", "Conectando");
  stream = new EventSource("/api/dashboard/events");
  stream.addEventListener("snapshot", event => {
    if (paused) return;
    try { snapshot = JSON.parse(event.data); connection("", "En vivo"); render(); }
    catch { connection("offline", "Datos no disponibles"); }
  });
  stream.onerror = () => { if (!paused) connection("offline", "Reconectando"); };
}
function visibleScans() {
  const query = $("search").value.trim().toLocaleLowerCase();
  return (snapshot?.overview.scans || []).filter(s => (filter === "all" || (filter === "detected" ? detected(s) : s.detectionLevel === "Clean")) &&
    (!query || [s.fileName,s.experimentId,s.roundId,s.sampleId,s.sha256].some(value => String(value ?? "").toLocaleLowerCase().includes(query))));
}
function percentile(data, fraction) { return data.length ? [...data].sort((a,b)=>a-b)[Math.ceil(data.length * fraction)-1] : null; }
function render() {
  const view = snapshot.overview, scans = view.scans, activities = snapshot.activities;
  const hasData = view.updatedAt !== null;
  $("notice").hidden = !view.error;
  $("notice").textContent = view.error || "";
  $("files-value").textContent = hasData ? number(scans.length,0) : "—";
  $("window-label").textContent = hasData ? "Ventana de los últimos 200" : "Conectando con el laboratorio";
  const count = scans.filter(detected).length;
  $("detected-value").innerHTML = hasData ? number(count,0) + `<small>${scans.length ? percent(count/scans.length) : "0%"}</small>` : "—";
  $("latency-value").textContent = scans.length ? number(scans.reduce((n,s)=>n+s.durationMs,0)/scans.length,2) : "—";
  $("p95-value").textContent = number(percentile(scans.map(s=>s.durationMs),.95),2);
  $("p99-value").textContent = number(percentile(scans.map(s=>s.durationMs),.99),2);
  const running = activities.filter(a=>a.state === "Running");
  $("active-value").textContent = running.length;
  $("active-caption").textContent = running.length ? "Procesando pruebas en este servidor" : "Esperando actividad del laboratorio";
  $("last-update").textContent = view.updatedAt ? "Sincronizado " + time(view.updatedAt) : "Esperando sincronización";
  $("footer-status").textContent = view.available ? "PERSISTENCIA CONECTADA · ACTUALIZACIÓN AUTOMÁTICA" : "PERSISTENCIA " + (view.error ? "SIN CONEXIÓN" : "CONECTANDO");
  renderFeed(activities);
  const scanKey = JSON.stringify(scans);
  if (scanKey !== previousScans) { previousScans = scanKey; renderResults(); }
  const last = snapshot.resources.at(-1);
  $("cpu-value").textContent = number(last?.cpu,1); $("ram-value").textContent = number(last?.memoryMb,0);
  $("resource-time").textContent = last ? time(last.timestamp) : "—";
  lineChart($("resource-chart"), snapshot.resources.map(r=>({value:r.cpu,label:time(r.timestamp)})), {max:100,compact:true,suffix:"%"});
  const benchmarkKey = JSON.stringify(view.benchmark);
  if (benchmarkKey !== previousBenchmark) { previousBenchmark = benchmarkKey; renderBenchmark(view.benchmark); }
}
function renderFeed(activities) {
  const items = [...activities].sort((a,b)=>(b.state === "Running")-(a.state === "Running") || new Date(b.startedAt)-new Date(a.startedAt)).slice(0,50);
  const key = JSON.stringify(items);
  if (key === previousFeed) return;
  previousFeed = key;
  if (!items.length) return;
  $("activity-feed").innerHTML = items.map(a=>{
    const state = a.state.toLowerCase(), icon = ({Running:"↻",Completed:"✓",Failed:"!",Cancelled:"−"})[a.state] || "·";
    const progress = a.total != null ? `<div class="progress"><span style="width:${a.total ? Math.min(100,a.completed/a.total*100) : a.state === "Completed" ? 100 : 0}%"></span></div>` : "";
    return `<div class="feed-row"><span class="feed-status ${state}">${icon}</span><div class="feed-body"><h3 title="${escapeHtml(a.name)}">${escapeHtml(a.name || kindName(a.kind))}</h3><p>${kindName(a.kind)} · ${escapeHtml(a.stage)}${a.total != null ? " · " + a.completed + "/" + a.total : ""}</p>${progress}</div><span class="feed-time">${time(a.startedAt)}</span></div>`;
  }).join("");
}
function renderResults() {
  const scans = visibleScans();
  $("all-count").textContent = snapshot?.overview.scans.length || 0;
  $("table-count").textContent = scans.length + " resultados visibles";
  $("results-body").innerHTML = scans.length ? scans.map(s=>{
    const level = levels.includes(s.detectionLevel) ? s.detectionLevel : "Clean";
    return `<tr class="result-row"><td><span class="file-icon">▧</span>${escapeHtml(s.fileName)}<small>${escapeHtml(s.sampleId || s.scanId.slice(0,8))}</small></td><td>${escapeHtml(s.experimentId || "Sin asignar")}<br><small style="color:#607985">${escapeHtml(s.roundId || "—")}</small></td><td><div class="risk-score">${number(s.riskScore,1)}<span><i style="width:${Math.min(100,Math.max(0,s.riskScore))}%;background:${colors[levels.indexOf(level)]}"></i></span></div></td><td><span class="level ${level}"><i class="dot"></i>${level}</span></td><td>${number(s.durationMs,2)} <span style="color:#637a82">ms</span></td><td>${time(s.completedAt)}</td><td><button class="detail-button" data-scan="${escapeHtml(s.scanId)}" aria-label="Ver detalles de ${escapeHtml(s.fileName)}">↗</button></td></tr>`;
  }).join("") : `<tr><td colspan="7" class="table-empty">${$("search").value || filter !== "all" ? "No hay resultados que coincidan con el filtro." : "Sin análisis todavía. Inicia una prueba para ver sus resultados aquí."}</td></tr>`;
  drawResultCharts(scans);
}
function drawResultCharts(scans = visibleScans()) {
  const chartScans = [...scans].sort((a,b)=>new Date(a.completedAt)-new Date(b.completedAt)).slice(-60);
  $("latency-empty").hidden = chartScans.length > 0;
  $("chart-range").textContent = chartScans.length ? `${chartScans.length} análisis · ${time(chartScans[0].completedAt)} — ${time(chartScans.at(-1).completedAt)}` : "Sin resultados seleccionados";
  lineChart($("latency-chart"), chartScans.map(s=>({value:s.durationMs,label:time(s.completedAt)})),{suffix:" ms"});
  const counts = levels.map(level=>scans.filter(s=>s.detectionLevel === level).length);
  $("risk-count").textContent = scans.length;
  $("risk-average").textContent = (scans.length ? number(scans.reduce((v,s)=>v+s.riskScore,0)/scans.length,1) : "—") + " / 100";
  $("risk-legend").innerHTML = levels.map((level,i)=>`<div class="legend-row"><i class="dot" style="color:${colors[i]}"></i><span>${level}</span><strong>${counts[i]}</strong><small>${scans.length ? Math.round(counts[i]/scans.length*100) : 0}%</small></div>`).join("");
  donutChart($("risk-chart"),counts);
}
function canvasContext(canvas) {
  const rect=canvas.getBoundingClientRect(), ratio=Math.min(window.devicePixelRatio || 1,2);
  canvas.width=Math.round(rect.width*ratio); canvas.height=Math.round(rect.height*ratio);
  const ctx=canvas.getContext("2d"); ctx.scale(ratio,ratio); return {ctx,w:rect.width,h:rect.height};
}
function lineChart(canvas, data, options = {}) {
  const {ctx,w,h}=canvasContext(canvas); if(w<1 || h<1)return;
  const left=options.compact ? 30:45,right=9,top=14,bottom=options.compact?9:26,width=w-left-right,height=h-top-bottom;
  const values=data.map(d=>d.value).filter(Number.isFinite);
  const peak=options.max || Math.max(1,...values)*1.2;
  const max=options.max || Math.ceil(peak/(peak>100?10:peak>10?1:.1))*(peak>100?10:peak>10?1:.1);
  ctx.font="9px Consolas,monospace";ctx.fillStyle="#5e757f";ctx.textAlign="right";
  for(let i=0;i<=3;i++) { const y=top+height*i/3;ctx.beginPath();ctx.strokeStyle="#253239";ctx.setLineDash([2,4]);ctx.moveTo(left,y);ctx.lineTo(w-right,y);ctx.stroke();ctx.fillText(number(max*(1-i/3),max<5?1:0),left-9,y+3); }
  ctx.setLineDash([]);
  if (!data.length) return;
  const points=data.map((d,i)=>[left+(data.length === 1 ? .5 : i / (data.length - 1))*width,top+height*(1-Math.min(max,Math.max(0,d.value))/max)]);
  const gradient=ctx.createLinearGradient(0,top,0,h);gradient.addColorStop(0,"#b8f47625");gradient.addColorStop(1,"#b8f47600");
  ctx.beginPath();ctx.moveTo(points[0][0],top+height);points.forEach(([x,y])=>ctx.lineTo(x,y));ctx.lineTo(points.at(-1)[0],top+height);ctx.closePath();ctx.fillStyle=gradient;ctx.fill();
  ctx.beginPath();points.forEach(([x,y],i)=>i?ctx.lineTo(x,y):ctx.moveTo(x,y));ctx.strokeStyle="#b8f476";ctx.lineWidth=1.7;ctx.stroke();
  if(data.length<15 || !options.compact) { ctx.fillStyle="#b8f476";points.forEach(([x,y])=>{ctx.beginPath();ctx.arc(x,y,data.length<8?3:1.5,0,Math.PI*2);ctx.fill();}); }
  if(!options.compact) { ctx.fillStyle="#647d88";ctx.textAlign="left";ctx.fillText(data[0].label,left,h-5);if(data.length>1){ctx.textAlign="right";ctx.fillText(data.at(-1).label,w-right,h-5);} }
  canvas.setAttribute("aria-label",`${options.compact?"CPU":"Latencia"}: ${data.length} mediciones. Última ${number(data.at(-1).value,2)}${options.suffix || ""}`);
}
function donutChart(canvas, counts) {
  const {ctx,w,h}=canvasContext(canvas),cx=w/2,cy=h/2,r=Math.min(w,h)/2-12,total=counts.reduce((a,b)=>a+b,0);
  ctx.lineWidth=9;ctx.strokeStyle="#26343a";ctx.beginPath();ctx.arc(cx,cy,r,0,Math.PI*2);ctx.stroke();
  let angle=-Math.PI/2;
  counts.forEach((count,i)=>{if(!count)return;const arc=count/total*Math.PI*2;ctx.beginPath();ctx.strokeStyle=colors[i];ctx.arc(cx,cy,r,angle+.012,angle+arc-.012);ctx.stroke();angle+=arc;});
  canvas.setAttribute("aria-label",levels.map((level,i)=>`${level}: ${counts[i]}`).join(", "));
}
function renderBenchmark(b) {
  if(!b)return;
  $("benchmark-meta").textContent = `${b.experimentId || "Sin experimento"} / ${b.roundId || "Sin ronda"} · ${b.samplesScanned} muestras · ${b.labeledSamples} etiquetadas · ${new Date(b.completedAt).toLocaleString("es-CR")}`;
  const stats=[ ["DETECCIÓN",percent(b.detectionRate),`TP ${b.truePositives} · FN ${b.falseNegatives}`], ["PRECISIÓN",percent(b.precision),`FP ${b.falsePositives} · TN ${b.trueNegatives}`], ["F1 SCORE",b.f1Score==null?"—":number(b.f1Score,3),"Referencia etiquetada"], ["LATENCIA P95",number(b.p95ScanMs,2)+" ms",`P99 ${number(b.p99ScanMs,2)} ms`], ["THROUGHPUT",number(b.filesPerSecond,1)+" /s",`${number(b.mbPerSecond,2)} MiB/s`], ["EVASIÓN",percent(b.evasionRate),`Wallabee ${b.wallabeeVersion}`] ];
  $("benchmark-content").innerHTML=stats.map(([label,value,sub])=>`<div class="benchmark-stat"><span>${label}</span><strong>${escapeHtml(value)}</strong><small>${escapeHtml(sub)}</small></div>`).join("");
}
function openDetail(id) {
  const s=snapshot?.overview.scans.find(s=>s.scanId===id);if(!s)return;
  $("detail-title").textContent=s.fileName;
  const level=levels.includes(s.detectionLevel)?s.detectionLevel:"Clean";
  $("detail-content").innerHTML=`<div class="detail-summary"><span class="level ${level}"><i class="dot"></i>${level}</span><span>Riesgo <strong>${number(s.riskScore,2)}</strong> / 100</span></div><dl class="detail-meta">${[["SCAN ID",s.scanId],["DURACIÓN",number(s.durationMs,3)+" ms"],["EXPERIMENTO",s.experimentId || "—"],["RONDA",s.roundId || "—"],["MUESTRA",s.sampleId || "—"],["TAMAÑO",number(s.fileSize,0)+" bytes"],["WALLABEE",s.wallabeeVersion],["RESPUESTA",s.responseAction]].map(([k,v])=>`<div><dt>${k}</dt><dd>${escapeHtml(v)}</dd></div>`).join("")}</dl><div class="hash">SHA256<br>${escapeHtml(s.sha256 || "No disponible")}</div><div class="detail-indicators"><h3>Indicadores · ${s.indicators.length}</h3>${s.indicators.length?s.indicators.map(i=>`<div class="indicator"><strong>${escapeHtml(i.name)}</strong><p>${escapeHtml(i.description)}</p><small>${escapeHtml(i.source)} · peso ${number(i.weight,2)}</small></div>`).join(""):"<p>No se activaron indicadores en este análisis.</p>"}</div>`;
  $("detail-dialog").showModal();
}
function showRunDialog(type) {
  if(busy) { toast("Hay una ejecución iniciada desde esta pantalla. Sigue su progreso en Actividad del motor.");return; }
  if(type)$("run-type").value=type;
  updateRunType();$("form-error").hidden=true;$("analysis-dialog").showModal();
}
function updateRunType() {
  const type=$("run-type").value;
  $("sample-field").hidden=type!=="file";$("labels-field").hidden=type!=="benchmark";
  $("run-submit").textContent=type==="benchmark"?"Iniciar benchmark ↗":"Iniciar análisis ↗";
}
$("analysis-form").addEventListener("submit",async event=>{
  event.preventDefault();if(busy)return;
  const type=$("run-type").value,path=$("run-path").value.trim();
  const error=message=>{$("form-error").textContent=message;$("form-error").hidden=false;};
  if(!path){error("Escribe la ruta del archivo o carpeta.");return;}
  const request={};request[type==="file"?"filePath":type==="benchmark"?"datasetDirectory":"directoryPath"]=path;
  if($("run-experiment").value.trim())request.experimentId=$("run-experiment").value.trim();
  if($("run-round").value.trim())request.roundId=$("run-round").value.trim();
  if(type==="file"&&$("run-sample").value.trim())request.sampleId=$("run-sample").value.trim();
  if(type==="benchmark"&&$("run-labels").value.trim()) {
    try { const labels=JSON.parse($("run-labels").value);if(!labels||Array.isArray(labels)||typeof labels!=="object"||!Object.values(labels).every(v=>typeof v==="boolean"))throw Error();request.expectedLabels=labels; }
    catch{error("Las etiquetas deben ser un objeto JSON con rutas relativas y valores true o false.");return;}
  }
  busy=true;$("run-submit").disabled=true;$("analysis-dialog").close();toast("Ejecución enviada. Puedes seguir el progreso en vivo.");
  try {
    const response=await fetch(type==="benchmark"?"/api/benchmark/run":"/api/scan/"+type,{method:"POST",headers:{"Content-Type":"application/json"},body:JSON.stringify(request)});
    if(!response.ok) {
      let problem={};try{problem=await response.json();}catch{}
      throw Error(problem.detail||problem.title||"No se pudo completar la ejecución. Revisa la ruta y la conexión del laboratorio.");
    }
    toast(type==="benchmark"?"Benchmark completado. Sus métricas se actualizarán en unos segundos.":"Análisis completado. Resultados guardados.");
  } catch(e) { toast(e.message || "No se pudo conectar con WALLABEE.",true); }
  finally {busy=false;$("run-submit").disabled=false;}
});
$("run-type").addEventListener("change",updateRunType);
$("new-analysis").addEventListener("click",()=>showRunDialog());
$("new-benchmark").addEventListener("click",()=>showRunDialog("benchmark"));
document.querySelectorAll("[data-close]").forEach(button=>button.addEventListener("click",()=>$(button.dataset.close).close()));
$("results-body").addEventListener("click",e=>{const button=e.target.closest("[data-scan]");if(button)openDetail(button.dataset.scan);});
document.querySelectorAll("[data-filter]").forEach(button=>button.addEventListener("click",()=>{filter=button.dataset.filter;document.querySelectorAll("[data-filter]").forEach(b=>b.classList.toggle("selected",b===button));renderResults();}));
$("search").addEventListener("input",()=>renderResults());
$("pause").addEventListener("click",()=>{
  paused=!paused;$("pause").textContent=paused?"▶":"Ⅱ";$("pause").setAttribute("aria-label",paused?"Reanudar visualización":"Pausar visualización");$("pause").title=paused?"Reanudar visualización":"Pausar visualización";
  if(paused){stream?.close();connection("paused","Vista pausada");toast("Visualización pausada. Las pruebas siguen ejecutándose.");}else connect();
});
$("export").addEventListener("click",()=>{
  const scans=visibleScans();if(!scans.length){toast("No hay resultados visibles para exportar.");return;}
  const cell=v=>'"'+String(v??"").replace(/^[=+@\-\t\r]/,"'$&").replace(/"/g,'""')+'"';
  const rows=[["ScanId","Archivo","Experimento","Ronda","Muestra","SHA256","Riesgo","Nivel","DuracionMs","Fecha"],...scans.map(s=>[s.scanId,s.fileName,s.experimentId,s.roundId,s.sampleId,s.sha256,s.riskScore,s.detectionLevel,s.durationMs,s.completedAt])];
  const url=URL.createObjectURL(new Blob(["\uFEFF"+rows.map(row=>row.map(cell).join(",")).join("\r\n")],{type:"text/csv;charset=utf-8"}));
  const link=document.createElement("a");link.href=url;link.download="wallabee-resultados.csv";link.click();setTimeout(()=>URL.revokeObjectURL(url),1000);
});
document.querySelectorAll(".nav-link").forEach(link=>link.addEventListener("click",()=>document.querySelectorAll(".nav-link").forEach(l=>l.classList.toggle("active",l===link))));
let resizeTimer;window.addEventListener("resize",()=>{clearTimeout(resizeTimer);resizeTimer=setTimeout(()=>{if(snapshot){drawResultCharts();lineChart($("resource-chart"),snapshot.resources.map(r=>({value:r.cpu,label:time(r.timestamp)})),{max:100,compact:true,suffix:"%"});}},100);});
setInterval(()=>$("clock").textContent=time(new Date()),1000);
window.addEventListener("pagehide",()=>stream?.close());
window.addEventListener("pageshow",event=>{if(event.persisted&&!paused)connect();});
connect();
