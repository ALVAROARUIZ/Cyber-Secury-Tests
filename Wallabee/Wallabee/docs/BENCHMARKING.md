# Benchmarks

`POST /api/benchmark/run` es sincrono y cancelable: retorna 201 cuando el resultado y los eventos asociados se confirman en SQL Server / DiveViewLab. GET por ID y latest funcionan tras reiniciar. `Benchmark.Enabled=false` rechaza ejecuciones.

```json
{
  "datasetDirectory": "C:/lab/dataset",
  "experimentId": "EXP-0001",
  "roundId": "ROUND-0004",
  "honeyBadgerVersion": "0.2.0",
  "samplesGenerated": 2,
  "expectedLabels": {
    "sample.synthetic-anomaly.bin": true,
    "clean.txt": false
  }
}
```

LabRoot debe contener el dataset. Las claves de expectedLabels son rutas relativas al dataset con `/`, comparadas exactamente; true significa positivo esperado, false negativo esperado. El resultado es positivo si DetectionLevel >= Medium. Las muestras sin etiqueta cuentan para rendimiento y distribucion, pero no para la matriz de confusion. `labeledSamples` permite detectar cobertura incompleta. `samplesMissed` significa falsos negativos etiquetados. Sin positivos etiquetados, DetectionRate/EvasionRate/Recall son null; sin predicciones positivas, Precision es null. No interpretar valores no disponibles como cero.

| Metrica | Definicion |
|---|---|
| Precision | TP/(TP+FP) |
| Recall / DetectionRate | TP/(TP+FN) |
| F1 | media armonica de precision y recall; 0 si no hay aciertos con etiquetas |
| EvasionRate / FalseNegativeRate | FN/(TP+FN) |
| FalsePositiveRate | FP/(FP+TN) |
| FilesPerSecond | archivos / segundos transcurridos reales |
| BytesPerSecond | bytes / segundos transcurridos reales |
| MbPerSecond | bytes / 1048576 / segundos (MiB/s) |
| AverageScanMs | promedio de latencias por archivo |
| P50/P95/P99 | nearest-rank: sorted[ceil(n*p)-1] |

TotalScanTimeMs mide escaneo y persistencia local de archivos, excluye commit final del benchmark y publicacion HTTP en background. La latencia individual cubre metadata, hash, analisis y scoring; excluye commit local y cuarentena. Se usa Stopwatch monotonico. El throughput no divide entre la suma de latencias paralelas. Un dataset vacio devuelve contadores, latencias y throughput cero (o cero archivos/tiempo real), sin excepciones ni NaN.

Con RecordSystemMetrics=true se muestrea el **proceso Wallabee**, no todo el host: CPU normalizada por procesadores logicos y working set en MiB, cada 50 ms mas el tramo final. CpuPeak es maximo de intervalos observados; RamPeakMb es maximo del working set observado. Ejecuciones simultaneas comparten proceso, por lo que CPU/RAM no son atribuibles de forma aislada a un solo benchmark. Desactivado, esas metricas son null.

```mermaid
flowchart LR
    Dataset --> Scans[Scans persistidos con correlacion]
    Labels[Expected labels] --> Matrix[Matriz TP TN FP FN]
    Scans --> Matrix
    Scans --> Timing[Latencias y throughput]
    Process[CPU y working set] --> Result[BenchmarkDto]
    Matrix --> Result
    Timing --> Result
    Result --> SQL Server / DiveViewLab
    Result --> Round[RoundSummary si existen ExperimentId y RoundId]
    Result --> Outbox
    Round --> Outbox
```

RoundSummary usa todos los campos planos de BenchmarkDto y anade SamplesGenerated. Representa la ejecucion de benchmark solicitada, no todas las ejecuciones historicas de una ronda. DIVE VIEW debe usar BenchmarkId para distinguir repeticiones y elegir la ejecucion que corresponda. SamplesGenerated proviene del request, o del numero escaneado si se omite.

La persistencia EF traduce BenchmarkDto a columnas de benchmark.BenchmarkRuns, actualiza el resumen de ExperimentRounds y registra la matriz de confusión. ExpectedLabels se conserva en Samples.GroundTruthLabel para reporting. Ver [DATABASE.md](DATABASE.md).
