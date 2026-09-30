# WALLABEE Live Lab

La UI está integrada en `src/Wallabee.API/wwwroot` y se sirve desde `/` en la misma API. No necesita Node, paquetes frontend ni CDN.

```powershell
dotnet run --project src/Wallabee.API -c Release --no-launch-profile -- --urls http://127.0.0.1:5289 --environment Development --WallabeeRuntime:UseHttpsRedirection=false
```

Abre http://127.0.0.1:5289. Se requiere la conexión SQL Server configurada para la API.

## Funciones

- Actividad de archivos, carpetas y benchmarks, incluidos los iniciados por Honey Badger mediante la API: estado, fase y progreso.
- Gráficas de latencia (hasta 60 resultados seleccionados), distribución por nivel y CPU del proceso, con lectura de RAM.
- Últimos 200 resultados persistidos, búsqueda, filtros, indicadores y exportación CSV de la selección.
- Métricas del último benchmark; tasas sin etiquetas de referencia aparecen como no disponibles.
- Nuevo análisis permite ejecutar archivos, carpetas o benchmarks. Las rutas corresponden al servidor y deben estar dentro de `LabRoot`; el formulario no sube archivos. Las etiquetas son un objeto JSON de rutas relativas a valores booleanos.

## Tiempo real y alcance

`GET /api/dashboard/events` transmite snapshots SSE cada segundo; el navegador reconecta automáticamente. `GET /api/dashboard` devuelve el estado como JSON. Los resultados SQL se refrescan cada tres segundos. Un error conserva los últimos resultados e indica la desconexión.

CPU y RAM corresponden al proceso de la API, no al equipo completo. CPU se normaliza por procesadores lógicos; el historial cubre aproximadamente dos minutos. La actividad conserva todas las operaciones activas y alrededor de 200 terminadas, en memoria y por instancia. Reiniciar limpia actividad y recursos, pero conserva los resultados SQL. Las operaciones breves pueden aparecer directamente como completadas. Una carpeta o benchmark y sus archivos cuentan como operaciones independientes.

Pausar detiene únicamente la visualización. Cerrar la página durante una ejecución iniciada desde ella puede cancelar la solicitud. Este panel monitoriza análisis y benchmarks del laboratorio; no ejecuta pruebas xUnit.

El comando publica únicamente en loopback. La UI usa los endpoints existentes de la API y no incorpora autenticación de usuarios.

## Validación

Pruebas de progreso concurrente, finalización, fallo, cancelación y retención de actividad. La integración verifica archivos estáticos, actividad real de escaneos/benchmarks y dos snapshots SSE consecutivos contra una base SQL de pruebas aislada. La revisión visual en navegador quedó pendiente por indisponibilidad de la herramienta de control del navegador.
