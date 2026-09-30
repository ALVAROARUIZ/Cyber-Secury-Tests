# Informe: migración de WALLABEE a SQL Server

## Resultado verificado

- SQL Server Express local, instancia `.\SQLEXPRESS`, base **DiveViewLab**.
- `dotnet restore HoneyBadgerVsWallabee.sln --disable-parallel`: correcto.
- `dotnet build HoneyBadgerVsWallabee.sln -c Release --no-restore`: correcto, **0 errores, 0 advertencias**.
- `dotnet test HoneyBadgerVsWallabee.sln -c Release --no-build --no-restore`: **53 aprobadas, 0 fallidas, 0 omitidas**.
- Desglose: Core 13, Application 3, Infrastructure 12, Benchmark 4, Telemetry 8, API 3, Launcher 10.
- **3 migrations aplicadas**, sin cambios de modelo pendientes.
- **38 foreign keys**, ninguna deshabilitada ni no confiable; **81 índices** (incluye claves/índices existentes).
- **8 vistas de reporting** existentes conservadas.
- Script idempotente `database/09_EfMigrations.sql` ejecutado nuevamente sin errores ni recreación de datos.
- GET /api/health real respondió HTTP 200: API Healthy, SQL Server Healthy, Persistence ready.
- Se recuperaron por HTTP un scan sintético y un benchmark guardados en DiveViewLab.
- No quedan bases temporales WallabeeTests_* de esta ejecución.
- `git diff --check`: correcto. Dependencias directas/transitivas de Infrastructure revisadas: sin SQLite.

Se usó Release porque el proceso Debug de la API ya abierto por el usuario mantiene DLLs bloqueadas. No se detuvo ese proceso. Se inició otra instancia de verificación en 127.0.0.1:5289 y se cerró al terminar; reiniciar la API habitual para cargar los nuevos binarios/configuración.

## Paquetes y composición

Eliminado: Microsoft.Data.Sqlite 10.0.12 (era el proveedor real anterior; no había Microsoft.EntityFrameworkCore.Sqlite).

Agregados: Microsoft.EntityFrameworkCore.SqlServer, Microsoft.EntityFrameworkCore.Design y Microsoft.EntityFrameworkCore.Tools, todos **10.0.12**. Design/Tools tienen PrivateAssets=all. dotnet-ef 10.0.12 queda fijado en dotnet-tools.json. No se actualizaron paquetes ajenos a esta migración.

DbContext final: [DiveViewDbContext](../src/Wallabee.Infrastructure/Persistence/DiveViewDbContext.cs). Usa ApplyConfigurationsFromAssembly; entidades y Fluent configurations están separadas de Core y de los DTOs HTTP.

Repositorios: EfScanResultRepository, EfBenchmarkResultRepository, EfTelemetryQueue, EfQuarantineRepository y EfSampleLabelRepository. El puerto IResultDocumentRepository se conserva por compatibilidad; su implementación decodifica BenchmarkDto y almacena columnas relacionales, nunca un documento gigante de métricas.

AddWallabeePersistence registra UseSqlServer, contexto scoped y factory. Cada operación/paralelismo usa un contexto independiente. EfUnitOfWork coordina referencias, scans, indicadores y eventos con una transacción Serializable y reintentos; consultas AsNoTracking, async y cancelables.

Conexión configurada, sin contraseña:

```text
Server=.SQLEXPRESS;Database=DiveViewLab;Trusted_Connection=True;TrustServerCertificate=True;
```

La arquitectura final, entidades, esquema Mermaid, comandos de inicialización/EF, SQL Authentication, Windows Authentication, user secrets, backups e inspección SSMS están en [DATABASE.md](DATABASE.md).

## Migrations y estrategia

```text
20260930010707_InitialSqlServerMigration
20260930011426_PreserveVersionContractLengths
20260930011839_AlignForeignKeyIndexes
```

Se generaron con `dotnet ef migrations add` y se aplicaron/verificaron mediante `dotnet ef database update` y `migrations list`, usando `--project src/Wallabee.Infrastructure --startup-project src/Wallabee.API --configuration Release`.

La primera migración adopta las tablas previas lab/benchmark/telemetry y conserva referencias, vistas y procedimientos. No elimina tablas ni datos reales. Las tablas físicas siguen siendo ScanIndicators, BenchmarkRuns y SystemMetricSamples para preservar sus consumidores. El script generado también sirve para SSMS. Las fechas de nombres de migration usan UTC, que en esta ejecución corresponde al día siguiente respecto a Costa Rica.

Se creó un backup COPY_ONLY con CHECKSUM y se verificó con RESTORE VERIFYONLY:

```text
C:Program FilesMicrosoft SQL ServerMSSQL16.SQLEXPRESSMSSQLBackupDiveViewLab_before_EF_20260929_BB3667CA86404BFB8FBC3120BC7F6715.bak
```

Ese backup se creó durante la validación; su nombre no debe interpretarse como garantía de snapshot anterior a toda inicialización automática. Para futuras migraciones, tomar el backup antes de iniciar cualquier host Development.

El SQLite inspeccionado estaba vacío (sin scans/benchmarks/cuarentena/eventos), por lo que no hubo datos que importar. Se conservaron sus archivos. No se ejecutó ningún script de reset ni se desarrolló DIVE VIEW.

## Pruebas y datos de validación

Las pruebas de integración usan SQL Server real y crean una DB WallabeeTests_GUID propia, con limpieza al finalizar o si falla la inicialización. Se validan Save/Get scan, indicadores, hash, experimento, ronda, benchmark, FK inválidas, rollback de la unidad completa, decimales, conversión enum, índices reales, versiones de 80 caracteres, salud SQL, concurrencia, cuarentena y recuperación. Unit tests de telemetría usan un fake del puerto, no SQLite ni un proveedor EF alternativo.

Durante la primera prueba HTTP se detectó resolución prematura de la conexión, antes del override de WebApplicationFactory. Se corrigió DI para resolver IConfiguration al crear las opciones EF y se añadió una aserción del nombre WallabeeTests_ antes de enviar requests. Esa primera verificación dejó **tres scans sintéticos y un benchmark** en DiveViewLab, bajo experimento EXP, rondas R1/R2; se conservaron explícitamente como resultados de validación:

- Scan `c3f15bc9-3568-4047-8456-95c2a46c52ba`: sample.synthetic-anomaly.bin, Medium.
- Scan `49f46763-6116-422f-84c9-5bd255246a7c`: clean.txt.
- Scan `918843db-0a54-40a5-b95e-a701e06d7e89`: positive.synthetic-anomaly.bin.
- Benchmark `5fa81f87-731c-4159-a1a8-f16f06a94cee`: 2 muestras, TP=1, TN=1.

No se almacenó su contenido. Los archivos temporales de prueba fueron eliminados; solo queda metadata. Las bases temporales creadas durante pruebas de migration fallidas se eliminaron por sus nombres exactos, sin tocar DiveViewLab.

## Límites y tareas no ejecutadas

- No se abrió la UI de SSMS; se verificó la misma instancia mediante sqlcmd, EF y HTTP real.
- No se probó SQL Authentication con contraseñas reales; está documentado y se verificó Windows Authentication.
- No se probó restaurar el backup en otra DB; RESTORE VERIFYONLY con CHECKSUM sí pasó.
- No existe importación automática para otros archivos SQLite con datos históricos. La instalación inspeccionada no la requería.
- La API Debug que ya estaba abierta sigue usando los binarios anteriores hasta reiniciarla.
- SQL Server debe estar disponible para confirmar nuevos scans; la exportación HTTP es opcional y está desactivada por defecto. No hay fallback de persistencia offline.

## Archivos modificados en esta migración

- [Directory.Packages.props](../Directory.Packages.props)
- [README.md](../README.md)
- [docs/ARCHITECTURE.md](../docs/ARCHITECTURE.md)
- [docs/BENCHMARKING.md](../docs/BENCHMARKING.md)
- [docs/DEVELOPMENT_SETUP.md](../docs/DEVELOPMENT_SETUP.md)
- [docs/DIVE_VIEW_INTEGRATION.md](../docs/DIVE_VIEW_INTEGRATION.md)
- [docs/HONEY_BADGER_INTEGRATION.md](../docs/HONEY_BADGER_INTEGRATION.md)
- [docs/IMPLEMENTATION_REPORT.md](../docs/IMPLEMENTATION_REPORT.md)
- [docs/TELEMETRY.md](../docs/TELEMETRY.md)
- [src/Wallabee.API/Configuration/RuntimeConfiguration.cs](../src/Wallabee.API/Configuration/RuntimeConfiguration.cs)
- [src/Wallabee.API/Controllers/HealthController.cs](../src/Wallabee.API/Controllers/HealthController.cs)
- [src/Wallabee.API/Program.cs](../src/Wallabee.API/Program.cs)
- [src/Wallabee.API/Wallabee.API.csproj](../src/Wallabee.API/Wallabee.API.csproj)
- [src/Wallabee.API/appsettings.Development.json](../src/Wallabee.API/appsettings.Development.json)
- [src/Wallabee.API/appsettings.json](../src/Wallabee.API/appsettings.json)
- [src/Wallabee.Benchmark/Runner/BenchmarkRunner.cs](../src/Wallabee.Benchmark/Runner/BenchmarkRunner.cs)
- [src/Wallabee.Core/Configuration/WallabeeOptions.cs](../src/Wallabee.Core/Configuration/WallabeeOptions.cs)
- [src/Wallabee.Core/Interfaces/PersistenceContracts.cs](../src/Wallabee.Core/Interfaces/PersistenceContracts.cs)
- [src/Wallabee.Infrastructure/DependencyInjection/DependencyInjection.cs](../src/Wallabee.Infrastructure/DependencyInjection/DependencyInjection.cs)
- [src/Wallabee.Infrastructure/Wallabee.Infrastructure.csproj](../src/Wallabee.Infrastructure/Wallabee.Infrastructure.csproj)
- [tests/Wallabee.API.Tests/HealthControllerTests.cs](../tests/Wallabee.API.Tests/HealthControllerTests.cs)
- [tests/Wallabee.API.Tests/ScanIntegrationTests.cs](../tests/Wallabee.API.Tests/ScanIntegrationTests.cs)
- [tests/Wallabee.API.Tests/Wallabee.API.Tests.csproj](../tests/Wallabee.API.Tests/Wallabee.API.Tests.csproj)
- [tests/Wallabee.Infrastructure.Tests/PersistenceTests.cs](../tests/Wallabee.Infrastructure.Tests/PersistenceTests.cs)
- [tests/Wallabee.Infrastructure.Tests/Wallabee.Infrastructure.Tests.csproj](../tests/Wallabee.Infrastructure.Tests/Wallabee.Infrastructure.Tests.csproj)
- [tests/Wallabee.Telemetry.Tests/TelemetryTests.cs](../tests/Wallabee.Telemetry.Tests/TelemetryTests.cs)
- [tests/Wallabee.Telemetry.Tests/Wallabee.Telemetry.Tests.csproj](../tests/Wallabee.Telemetry.Tests/Wallabee.Telemetry.Tests.csproj)

## Archivos creados en esta migración

- [dotnet-tools.json](../dotnet-tools.json)
- [docs/DATABASE.md](../docs/DATABASE.md)
- [docs/SQLSERVER_MIGRATION_REPORT.md](../docs/SQLSERVER_MIGRATION_REPORT.md)
- [database/09_EfMigrations.sql](../database/09_EfMigrations.sql)
- [src/Wallabee.API/Configuration/DatabaseStartup.cs](../src/Wallabee.API/Configuration/DatabaseStartup.cs)
- [src/Wallabee.Infrastructure/DependencyInjection/PersistenceServiceCollectionExtensions.cs](../src/Wallabee.Infrastructure/DependencyInjection/PersistenceServiceCollectionExtensions.cs)
- [tests/Shared/SqlServerTestDatabase.cs](../tests/Shared/SqlServerTestDatabase.cs)
- [src/Wallabee.Infrastructure/Persistence/Configurations/BenchmarkResultEntityConfiguration.cs](../src/Wallabee.Infrastructure/Persistence/Configurations/BenchmarkResultEntityConfiguration.cs)
- [src/Wallabee.Infrastructure/Persistence/Configurations/DatasetEntityConfiguration.cs](../src/Wallabee.Infrastructure/Persistence/Configurations/DatasetEntityConfiguration.cs)
- [src/Wallabee.Infrastructure/Persistence/Configurations/DetectionConfusionMatrixEntityConfiguration.cs](../src/Wallabee.Infrastructure/Persistence/Configurations/DetectionConfusionMatrixEntityConfiguration.cs)
- [src/Wallabee.Infrastructure/Persistence/Configurations/ExperimentEntityConfiguration.cs](../src/Wallabee.Infrastructure/Persistence/Configurations/ExperimentEntityConfiguration.cs)
- [src/Wallabee.Infrastructure/Persistence/Configurations/ExperimentEventEntityConfiguration.cs](../src/Wallabee.Infrastructure/Persistence/Configurations/ExperimentEventEntityConfiguration.cs)
- [src/Wallabee.Infrastructure/Persistence/Configurations/ExperimentRoundEntityConfiguration.cs](../src/Wallabee.Infrastructure/Persistence/Configurations/ExperimentRoundEntityConfiguration.cs)
- [src/Wallabee.Infrastructure/Persistence/Configurations/HoneyBadgerVersionEntityConfiguration.cs](../src/Wallabee.Infrastructure/Persistence/Configurations/HoneyBadgerVersionEntityConfiguration.cs)
- [src/Wallabee.Infrastructure/Persistence/Configurations/IndicatorDefinitionEntityConfiguration.cs](../src/Wallabee.Infrastructure/Persistence/Configurations/IndicatorDefinitionEntityConfiguration.cs)
- [src/Wallabee.Infrastructure/Persistence/Configurations/QuarantineEventEntityConfiguration.cs](../src/Wallabee.Infrastructure/Persistence/Configurations/QuarantineEventEntityConfiguration.cs)
- [src/Wallabee.Infrastructure/Persistence/Configurations/SampleEntityConfiguration.cs](../src/Wallabee.Infrastructure/Persistence/Configurations/SampleEntityConfiguration.cs)
- [src/Wallabee.Infrastructure/Persistence/Configurations/ScanEntityConfiguration.cs](../src/Wallabee.Infrastructure/Persistence/Configurations/ScanEntityConfiguration.cs)
- [src/Wallabee.Infrastructure/Persistence/Configurations/SystemMetricEntityConfiguration.cs](../src/Wallabee.Infrastructure/Persistence/Configurations/SystemMetricEntityConfiguration.cs)
- [src/Wallabee.Infrastructure/Persistence/Configurations/TelemetryEventEntityConfiguration.cs](../src/Wallabee.Infrastructure/Persistence/Configurations/TelemetryEventEntityConfiguration.cs)
- [src/Wallabee.Infrastructure/Persistence/Configurations/TelemetryFailureEntityConfiguration.cs](../src/Wallabee.Infrastructure/Persistence/Configurations/TelemetryFailureEntityConfiguration.cs)
- [src/Wallabee.Infrastructure/Persistence/Configurations/TestHostEntityConfiguration.cs](../src/Wallabee.Infrastructure/Persistence/Configurations/TestHostEntityConfiguration.cs)
- [src/Wallabee.Infrastructure/Persistence/Configurations/ThreatIndicatorEntityConfiguration.cs](../src/Wallabee.Infrastructure/Persistence/Configurations/ThreatIndicatorEntityConfiguration.cs)
- [src/Wallabee.Infrastructure/Persistence/Configurations/WallabeeVersionEntityConfiguration.cs](../src/Wallabee.Infrastructure/Persistence/Configurations/WallabeeVersionEntityConfiguration.cs)
- [src/Wallabee.Infrastructure/Persistence/DiveViewDbContext.cs](../src/Wallabee.Infrastructure/Persistence/DiveViewDbContext.cs)
- [src/Wallabee.Infrastructure/Persistence/DiveViewDbContextFactory.cs](../src/Wallabee.Infrastructure/Persistence/DiveViewDbContextFactory.cs)
- [src/Wallabee.Infrastructure/Persistence/EfUnitOfWork.cs](../src/Wallabee.Infrastructure/Persistence/EfUnitOfWork.cs)
- [src/Wallabee.Infrastructure/Persistence/Entities/BenchmarkResultEntity.cs](../src/Wallabee.Infrastructure/Persistence/Entities/BenchmarkResultEntity.cs)
- [src/Wallabee.Infrastructure/Persistence/Entities/DatasetEntity.cs](../src/Wallabee.Infrastructure/Persistence/Entities/DatasetEntity.cs)
- [src/Wallabee.Infrastructure/Persistence/Entities/DetectionConfusionMatrixEntity.cs](../src/Wallabee.Infrastructure/Persistence/Entities/DetectionConfusionMatrixEntity.cs)
- [src/Wallabee.Infrastructure/Persistence/Entities/ExperimentEntity.cs](../src/Wallabee.Infrastructure/Persistence/Entities/ExperimentEntity.cs)
- [src/Wallabee.Infrastructure/Persistence/Entities/ExperimentEventEntity.cs](../src/Wallabee.Infrastructure/Persistence/Entities/ExperimentEventEntity.cs)
- [src/Wallabee.Infrastructure/Persistence/Entities/ExperimentRoundEntity.cs](../src/Wallabee.Infrastructure/Persistence/Entities/ExperimentRoundEntity.cs)
- [src/Wallabee.Infrastructure/Persistence/Entities/HoneyBadgerVersionEntity.cs](../src/Wallabee.Infrastructure/Persistence/Entities/HoneyBadgerVersionEntity.cs)
- [src/Wallabee.Infrastructure/Persistence/Entities/IndicatorDefinitionEntity.cs](../src/Wallabee.Infrastructure/Persistence/Entities/IndicatorDefinitionEntity.cs)
- [src/Wallabee.Infrastructure/Persistence/Entities/QuarantineEventEntity.cs](../src/Wallabee.Infrastructure/Persistence/Entities/QuarantineEventEntity.cs)
- [src/Wallabee.Infrastructure/Persistence/Entities/SampleEntity.cs](../src/Wallabee.Infrastructure/Persistence/Entities/SampleEntity.cs)
- [src/Wallabee.Infrastructure/Persistence/Entities/ScanEntity.cs](../src/Wallabee.Infrastructure/Persistence/Entities/ScanEntity.cs)
- [src/Wallabee.Infrastructure/Persistence/Entities/SystemMetricEntity.cs](../src/Wallabee.Infrastructure/Persistence/Entities/SystemMetricEntity.cs)
- [src/Wallabee.Infrastructure/Persistence/Entities/TelemetryEventEntity.cs](../src/Wallabee.Infrastructure/Persistence/Entities/TelemetryEventEntity.cs)
- [src/Wallabee.Infrastructure/Persistence/Entities/TelemetryFailureEntity.cs](../src/Wallabee.Infrastructure/Persistence/Entities/TelemetryFailureEntity.cs)
- [src/Wallabee.Infrastructure/Persistence/Entities/TestHostEntity.cs](../src/Wallabee.Infrastructure/Persistence/Entities/TestHostEntity.cs)
- [src/Wallabee.Infrastructure/Persistence/Entities/ThreatIndicatorEntity.cs](../src/Wallabee.Infrastructure/Persistence/Entities/ThreatIndicatorEntity.cs)
- [src/Wallabee.Infrastructure/Persistence/Entities/WallabeeVersionEntity.cs](../src/Wallabee.Infrastructure/Persistence/Entities/WallabeeVersionEntity.cs)
- [src/Wallabee.Infrastructure/Persistence/Mapping/BenchmarkEntityMapping.cs](../src/Wallabee.Infrastructure/Persistence/Mapping/BenchmarkEntityMapping.cs)
- [src/Wallabee.Infrastructure/Persistence/Mapping/ReferenceResolver.cs](../src/Wallabee.Infrastructure/Persistence/Mapping/ReferenceResolver.cs)
- [src/Wallabee.Infrastructure/Persistence/Mapping/ScanEntityMapping.cs](../src/Wallabee.Infrastructure/Persistence/Mapping/ScanEntityMapping.cs)
- [src/Wallabee.Infrastructure/Persistence/Migrations/20260930010707_InitialSqlServerMigration.Designer.cs](../src/Wallabee.Infrastructure/Persistence/Migrations/20260930010707_InitialSqlServerMigration.Designer.cs)
- [src/Wallabee.Infrastructure/Persistence/Migrations/20260930010707_InitialSqlServerMigration.cs](../src/Wallabee.Infrastructure/Persistence/Migrations/20260930010707_InitialSqlServerMigration.cs)
- [src/Wallabee.Infrastructure/Persistence/Migrations/20260930011426_PreserveVersionContractLengths.Designer.cs](../src/Wallabee.Infrastructure/Persistence/Migrations/20260930011426_PreserveVersionContractLengths.Designer.cs)
- [src/Wallabee.Infrastructure/Persistence/Migrations/20260930011426_PreserveVersionContractLengths.cs](../src/Wallabee.Infrastructure/Persistence/Migrations/20260930011426_PreserveVersionContractLengths.cs)
- [src/Wallabee.Infrastructure/Persistence/Migrations/20260930011839_AlignForeignKeyIndexes.Designer.cs](../src/Wallabee.Infrastructure/Persistence/Migrations/20260930011839_AlignForeignKeyIndexes.Designer.cs)
- [src/Wallabee.Infrastructure/Persistence/Migrations/20260930011839_AlignForeignKeyIndexes.cs](../src/Wallabee.Infrastructure/Persistence/Migrations/20260930011839_AlignForeignKeyIndexes.cs)
- [src/Wallabee.Infrastructure/Persistence/Migrations/Baseline/01_CreateSchemas.sql](../src/Wallabee.Infrastructure/Persistence/Migrations/Baseline/01_CreateSchemas.sql)
- [src/Wallabee.Infrastructure/Persistence/Migrations/Baseline/02_CreateTables.sql](../src/Wallabee.Infrastructure/Persistence/Migrations/Baseline/02_CreateTables.sql)
- [src/Wallabee.Infrastructure/Persistence/Migrations/Baseline/03_CreateConstraints.sql](../src/Wallabee.Infrastructure/Persistence/Migrations/Baseline/03_CreateConstraints.sql)
- [src/Wallabee.Infrastructure/Persistence/Migrations/Baseline/04_CreateIndexes.sql](../src/Wallabee.Infrastructure/Persistence/Migrations/Baseline/04_CreateIndexes.sql)
- [src/Wallabee.Infrastructure/Persistence/Migrations/Baseline/05_EfAdditions.sql](../src/Wallabee.Infrastructure/Persistence/Migrations/Baseline/05_EfAdditions.sql)
- [src/Wallabee.Infrastructure/Persistence/Migrations/Baseline/ValidateBaseline.sql](../src/Wallabee.Infrastructure/Persistence/Migrations/Baseline/ValidateBaseline.sql)
- [src/Wallabee.Infrastructure/Persistence/Migrations/DiveViewDbContextModelSnapshot.cs](../src/Wallabee.Infrastructure/Persistence/Migrations/DiveViewDbContextModelSnapshot.cs)
- [src/Wallabee.Infrastructure/Persistence/Migrations/SqlServerBaseline.cs](../src/Wallabee.Infrastructure/Persistence/Migrations/SqlServerBaseline.cs)
- [src/Wallabee.Infrastructure/Persistence/Repositories/EfBenchmarkResultRepository.cs](../src/Wallabee.Infrastructure/Persistence/Repositories/EfBenchmarkResultRepository.cs)
- [src/Wallabee.Infrastructure/Persistence/Repositories/EfQuarantineRepository.cs](../src/Wallabee.Infrastructure/Persistence/Repositories/EfQuarantineRepository.cs)
- [src/Wallabee.Infrastructure/Persistence/Repositories/EfSampleLabelRepository.cs](../src/Wallabee.Infrastructure/Persistence/Repositories/EfSampleLabelRepository.cs)
- [src/Wallabee.Infrastructure/Persistence/Repositories/EfScanResultRepository.cs](../src/Wallabee.Infrastructure/Persistence/Repositories/EfScanResultRepository.cs)
- [src/Wallabee.Infrastructure/Persistence/Repositories/EfTelemetryQueue.cs](../src/Wallabee.Infrastructure/Persistence/Repositories/EfTelemetryQueue.cs)
- [src/Wallabee.Infrastructure/Persistence/SqlServerHealthCheck.cs](../src/Wallabee.Infrastructure/Persistence/SqlServerHealthCheck.cs)

## Implementaciones retiradas

- `src/Wallabee.Infrastructure/Persistence/LocalDatabase.cs`
- `src/Wallabee.Infrastructure/Persistence/Migrations/001_initial.sql`
- `src/Wallabee.Infrastructure/Persistence/Repositories/SqliteScanResultRepository.cs`
- `src/Wallabee.Infrastructure/Persistence/Repositories/SqliteResultDocumentRepository.cs`
- `src/Wallabee.Infrastructure/Persistence/Repositories/SqliteTelemetryQueue.cs`
- `src/Wallabee.Infrastructure/Persistence/Repositories/SqliteQuarantineRepository.cs`
