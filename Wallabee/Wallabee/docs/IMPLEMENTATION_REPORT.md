> Informe histórico de la implementación SQLite anterior. La persistencia vigente es SQL Server; consultar [DATABASE.md](DATABASE.md) y [SQLSERVER_MIGRATION_REPORT.md](SQLSERVER_MIGRATION_REPORT.md). Las rutas históricas de repositorios SQLite ya fueron sustituidas.

# Informe de implementacion WALLABEE

Se adapto la solucion existente, conservando Launcher, Core independiente y controllers delgados. No se desplego una base central ni se subieron muestras.

## Validacion realizada

- `dotnet build HoneyBadgerVsWallabee.sln --no-restore --nologo -v minimal`: correcto, 0 errores, 0 advertencias.
- `dotnet test HoneyBadgerVsWallabee.sln --no-build --no-restore --nologo -v minimal`: 47 aprobadas, 0 fallidas, 0 omitidas.
- Core: 13; Application: 3; Infrastructure: 7; Benchmark: 4; Telemetry: 8; API: 2; Launcher: 10.
- `git diff --check`: sin errores de whitespace.
- SDK local: .NET 10.0.401; plataforma de ejecucion Windows.

Se probo hashing conocido, reglas independientes, limites de scoring, politica de cuarentena, persistencia y consulta por hash/experimento/ronda, indicadores normalizados, rollback atomico ante fallo de outbox, historial de fallos tras reinicio, retry y ACK, cancelacion sin perdida de pendientes, restauracion de cuarentena tras reinicio, P50/P95/P99, dataset vacio, throughput con tiempo real, confusion matrix y endpoints HTTP reales bajo WebApplicationFactory. Las muestras de prueba son texto sintetico temporal. Las pruebas de API ejercitan la composicion DI completa y recuperan scans tras reiniciar el host. El receptor central se simula con HttpMessageHandler: no se afirma una prueba contra un servidor DIVE real.

## Dependencias, flujos y ejemplos

- [Dependencias de proyectos, flujo completo de scan y esquema SQLite](ARCHITECTURE.md).
- [Flujo de publicacion, ACK, estados y retries](TELEMETRY.md).
- [Benchmark y significado de metricas](BENCHMARKING.md).
- [Requests y responses HTTP de Honey Badger](HONEY_BADGER_INTEGRATION.md).
- [Modelos publicados para DIVE VIEW y contrato del receptor](DIVE_VIEW_INTEGRATION.md).
- `DDL exacto local` (retirado en la migración SQL Server).

Ruta del flujo operativo: Honey Badger -> API -> Application -> scanner/pipeline -> scoring/politica -> transaccion SQLite (Scans + ThreatIndicators + PendingTelemetry) -> respuesta. En paralelo, BackgroundPublisher -> Dive Telemetry API -> ACK coincidente -> Published. La base central y DIVE VIEW quedan al otro lado del contrato HTTP externo.

Adaptaciones de compatibilidad: ScanResult conserva el agregado original; FileScanResult es el scan individual ampliado. Los contratos publicos son ScanResponse, ScanTelemetry, BenchmarkDto y RoundSummary. La solucion existente no se renombro; Wallabee.slnx ahora abre los mismos modulos operativos. No se alteraron los archivos preexistentes de .vscode ni los archivos de database que aparecieron durante la implementacion.

## Archivos creados

Total: 48.

- [data/local/.gitkeep](../data/local/.gitkeep)
- [data/quarantine/.gitkeep](../data/quarantine/.gitkeep)
- [docs/ARCHITECTURE.md](../docs/ARCHITECTURE.md)
- [docs/BENCHMARKING.md](../docs/BENCHMARKING.md)
- [docs/DIVE_VIEW_INTEGRATION.md](../docs/DIVE_VIEW_INTEGRATION.md)
- [docs/HONEY_BADGER_INTEGRATION.md](../docs/HONEY_BADGER_INTEGRATION.md)
- [docs/IMPLEMENTATION_REPORT.md](../docs/IMPLEMENTATION_REPORT.md)
- [docs/TELEMETRY.md](../docs/TELEMETRY.md)
- [src/Wallabee.API/Configuration/ApiExceptionHandler.cs](../src/Wallabee.API/Configuration/ApiExceptionHandler.cs)
- [src/Wallabee.API/Configuration/RuntimeConfiguration.cs](../src/Wallabee.API/Configuration/RuntimeConfiguration.cs)
- [src/Wallabee.API/Controllers/ScansController.cs](../src/Wallabee.API/Controllers/ScansController.cs)
- [src/Wallabee.API/Controllers/TelemetryController.cs](../src/Wallabee.API/Controllers/TelemetryController.cs)
- [src/Wallabee.Application/DependencyInjection/CoreServiceCollectionExtensions.cs](../src/Wallabee.Application/DependencyInjection/CoreServiceCollectionExtensions.cs)
- [src/Wallabee.Application/Mapping/ScanMapping.cs](../src/Wallabee.Application/Mapping/ScanMapping.cs)
- [src/Wallabee.Application/Scanning/Services/ScanQueryService.cs](../src/Wallabee.Application/Scanning/Services/ScanQueryService.cs)
- [src/Wallabee.Application/Telemetry/TelemetryService.cs](../src/Wallabee.Application/Telemetry/TelemetryService.cs)
- [src/Wallabee.Benchmark/Profiling/ProcessProfiler.cs](../src/Wallabee.Benchmark/Profiling/ProcessProfiler.cs)
- [src/Wallabee.Contracts/Benchmark/BenchmarkDto.cs](../src/Wallabee.Contracts/Benchmark/BenchmarkDto.cs)
- [src/Wallabee.Contracts/Experiments/RoundSummary.cs](../src/Wallabee.Contracts/Experiments/RoundSummary.cs)
- [src/Wallabee.Contracts/Requests/ScanRequests.cs](../src/Wallabee.Contracts/Requests/ScanRequests.cs)
- [src/Wallabee.Contracts/Responses/ScanResponse.cs](../src/Wallabee.Contracts/Responses/ScanResponse.cs)
- [src/Wallabee.Contracts/Telemetry/TelemetryModels.cs](../src/Wallabee.Contracts/Telemetry/TelemetryModels.cs)
- [src/Wallabee.Contracts/Wallabee.Contracts.csproj](../src/Wallabee.Contracts/Wallabee.Contracts.csproj)
- [src/Wallabee.Core/Enums/ResponseAction.cs](../src/Wallabee.Core/Enums/ResponseAction.cs)
- [src/Wallabee.Core/Heuristics/StaticRules.cs](../src/Wallabee.Core/Heuristics/StaticRules.cs)
- [src/Wallabee.Core/Interfaces/PersistenceContracts.cs](../src/Wallabee.Core/Interfaces/PersistenceContracts.cs)
- [src/Wallabee.Core/Policies/PolicyEngine.cs](../src/Wallabee.Core/Policies/PolicyEngine.cs)
- [src/Wallabee.Infrastructure/FileSystem/LocalFileCatalog.cs](../src/Wallabee.Infrastructure/FileSystem/LocalFileCatalog.cs)
- [src/Wallabee.Infrastructure/FileSystem/StaticFileAnalyzer.cs](../src/Wallabee.Infrastructure/FileSystem/StaticFileAnalyzer.cs)
- `src/Wallabee.Infrastructure/Persistence/LocalDatabase.cs` (retirado en la migración SQL Server)
- `src/Wallabee.Infrastructure/Persistence/Migrations/001_initial.sql` (retirado en la migración SQL Server)
- `src/Wallabee.Infrastructure/Persistence/Repositories/SqliteQuarantineRepository.cs` (retirado en la migración SQL Server)
- `src/Wallabee.Infrastructure/Persistence/Repositories/SqliteResultDocumentRepository.cs` (retirado en la migración SQL Server)
- `src/Wallabee.Infrastructure/Persistence/Repositories/SqliteScanResultRepository.cs` (retirado en la migración SQL Server)
- `src/Wallabee.Infrastructure/Persistence/Repositories/SqliteTelemetryQueue.cs` (retirado en la migración SQL Server)
- [src/Wallabee.Telemetry/BackgroundServices/TelemetryBackgroundPublisher.cs](../src/Wallabee.Telemetry/BackgroundServices/TelemetryBackgroundPublisher.cs)
- [src/Wallabee.Telemetry/Configuration/TelemetryOptions.cs](../src/Wallabee.Telemetry/Configuration/TelemetryOptions.cs)
- [src/Wallabee.Telemetry/DependencyInjection/DependencyInjection.cs](../src/Wallabee.Telemetry/DependencyInjection/DependencyInjection.cs)
- [src/Wallabee.Telemetry/Publisher/HttpTelemetryPublisher.cs](../src/Wallabee.Telemetry/Publisher/HttpTelemetryPublisher.cs)
- [src/Wallabee.Telemetry/Retry/TelemetryDispatcher.cs](../src/Wallabee.Telemetry/Retry/TelemetryDispatcher.cs)
- [src/Wallabee.Telemetry/Wallabee.Telemetry.csproj](../src/Wallabee.Telemetry/Wallabee.Telemetry.csproj)
- [tests/Wallabee.API.Tests/ScanIntegrationTests.cs](../tests/Wallabee.API.Tests/ScanIntegrationTests.cs)
- [tests/Wallabee.Application.Tests/ScanFlowTests.cs](../tests/Wallabee.Application.Tests/ScanFlowTests.cs)
- [tests/Wallabee.Benchmark.Tests/PercentileTests.cs](../tests/Wallabee.Benchmark.Tests/PercentileTests.cs)
- [tests/Wallabee.Core.Tests/PolicyAndStaticRuleTests.cs](../tests/Wallabee.Core.Tests/PolicyAndStaticRuleTests.cs)
- [tests/Wallabee.Infrastructure.Tests/PersistenceTests.cs](../tests/Wallabee.Infrastructure.Tests/PersistenceTests.cs)
- [tests/Wallabee.Telemetry.Tests/TelemetryTests.cs](../tests/Wallabee.Telemetry.Tests/TelemetryTests.cs)
- [tests/Wallabee.Telemetry.Tests/Wallabee.Telemetry.Tests.csproj](../tests/Wallabee.Telemetry.Tests/Wallabee.Telemetry.Tests.csproj)

## Archivos modificados

Total: 31.

- [.gitignore](../.gitignore)
- [Directory.Packages.props](../Directory.Packages.props)
- [HoneyBadgerVsWallabee.sln](../HoneyBadgerVsWallabee.sln)
- [README.md](../README.md)
- [Wallabee.slnx](../Wallabee.slnx)
- [src/Wallabee.API/Contracts/Requests/ScanRequests.cs](../src/Wallabee.API/Contracts/Requests/ScanRequests.cs)
- [src/Wallabee.API/Controllers/BenchmarkController.cs](../src/Wallabee.API/Controllers/BenchmarkController.cs)
- [src/Wallabee.API/Controllers/QuarantineController.cs](../src/Wallabee.API/Controllers/QuarantineController.cs)
- [src/Wallabee.API/Controllers/ScanController.cs](../src/Wallabee.API/Controllers/ScanController.cs)
- [src/Wallabee.API/Program.cs](../src/Wallabee.API/Program.cs)
- [src/Wallabee.API/Wallabee.API.csproj](../src/Wallabee.API/Wallabee.API.csproj)
- [src/Wallabee.API/appsettings.json](../src/Wallabee.API/appsettings.json)
- [src/Wallabee.Application/DependencyInjection/DependencyInjection.cs](../src/Wallabee.Application/DependencyInjection/DependencyInjection.cs)
- [src/Wallabee.Application/Quarantine/Services/QuarantineApplicationService.cs](../src/Wallabee.Application/Quarantine/Services/QuarantineApplicationService.cs)
- [src/Wallabee.Application/Scanning/Services/ScanDirectoryService.cs](../src/Wallabee.Application/Scanning/Services/ScanDirectoryService.cs)
- [src/Wallabee.Application/Scanning/Services/ScanFileService.cs](../src/Wallabee.Application/Scanning/Services/ScanFileService.cs)
- [src/Wallabee.Application/Wallabee.Application.csproj](../src/Wallabee.Application/Wallabee.Application.csproj)
- [src/Wallabee.Benchmark/DependencyInjection/DependencyInjection.cs](../src/Wallabee.Benchmark/DependencyInjection/DependencyInjection.cs)
- [src/Wallabee.Benchmark/Runner/BenchmarkRunner.cs](../src/Wallabee.Benchmark/Runner/BenchmarkRunner.cs)
- [src/Wallabee.Benchmark/Statistics/BenchmarkMetricCalculator.cs](../src/Wallabee.Benchmark/Statistics/BenchmarkMetricCalculator.cs)
- [src/Wallabee.Benchmark/Wallabee.Benchmark.csproj](../src/Wallabee.Benchmark/Wallabee.Benchmark.csproj)
- [src/Wallabee.Core/Configuration/WallabeeOptions.cs](../src/Wallabee.Core/Configuration/WallabeeOptions.cs)
- [src/Wallabee.Core/Interfaces/ScanningContracts.cs](../src/Wallabee.Core/Interfaces/ScanningContracts.cs)
- [src/Wallabee.Core/Models/ScanModels.cs](../src/Wallabee.Core/Models/ScanModels.cs)
- [src/Wallabee.Core/Scoring/IndicatorRiskScorer.cs](../src/Wallabee.Core/Scoring/IndicatorRiskScorer.cs)
- [src/Wallabee.Infrastructure/DependencyInjection/DependencyInjection.cs](../src/Wallabee.Infrastructure/DependencyInjection/DependencyInjection.cs)
- [src/Wallabee.Infrastructure/FileSystem/LabPathValidator.cs](../src/Wallabee.Infrastructure/FileSystem/LabPathValidator.cs)
- [src/Wallabee.Infrastructure/FileSystem/LocalFileSystemScanner.cs](../src/Wallabee.Infrastructure/FileSystem/LocalFileSystemScanner.cs)
- [src/Wallabee.Infrastructure/Quarantine/LocalQuarantineService.cs](../src/Wallabee.Infrastructure/Quarantine/LocalQuarantineService.cs)
- [src/Wallabee.Infrastructure/Wallabee.Infrastructure.csproj](../src/Wallabee.Infrastructure/Wallabee.Infrastructure.csproj)
- [tests/Wallabee.API.Tests/Wallabee.API.Tests.csproj](../tests/Wallabee.API.Tests/Wallabee.API.Tests.csproj)

## Limites operativos documentados

- Se necesita configurar Telemetry:BaseUrl y disponer del receptor central con ACK y deduplicacion por EventId; no hay escritura directa a SQL remoto.
- Entrega al menos una vez, una instancia publicadora por DB local; el receptor debe tolerar duplicados.
- No hay purga automatica de metadata. La retencion fisica es responsabilidad del laboratorio.
- CPU/RAM corresponden al proceso y a intervalos observados durante benchmarks.
- Las heuristicas son experimentales; el prefijo de analisis estatico tiene 64 KiB y SHA256 cubre todo el archivo.
- La persistencia local y el movimiento de cuarentena no forman una transaccion distribuida; las intenciones y los estados de recuperacion quedan registrados.
