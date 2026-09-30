# DIVE VIEW · Web Observatory

DIVE VIEW ahora se abre en el navegador y conserva el backend Python, los módulos de análisis y el acceso de solo lectura a SQL Server. Comparte la estética de WALLABEE: carbón, paneles sobrios, acentos lima y gráficas centradas en los datos.

## Iniciar

Requiere Python 3.12+, Microsoft ODBC Driver 18 y acceso a DiveViewLab.

```powershell
# Desde Benchmarks/dive-view, usando el entorno existente:
../.venv/Scripts/python.exe -m pip install -e ".[dev]"
$env:DIVE_DB_SERVER='.\SQLEXPRESS'
$env:DIVE_VIEW_DEMO_MODE='false'
../.venv/Scripts/python.exe -m dive_view.app.main
```

Abre **http://127.0.0.1:8790**. El comando `dive-view` también inicia el servidor tras instalar el paquete. No requiere Node ni un proceso frontend separado.

En otras instalaciones, instala el paquete con tu Python y configura `.env` a partir de `.env.example`. `DIVE_WEB_HOST` y `DIVE_WEB_PORT` controlan la dirección de escucha. Las credenciales SQL permanecen en el servidor. La configuración predeterminada usa loopback; la publicación en otra máquina o Internet requiere configurar el alojamiento y el acceso. Esta entrega está disponible localmente y no se ha publicado en Internet ni incorpora autenticación web.

## Pantallas disponibles

- Resumen: eventos, análisis, experimentos, rondas, latencia media, volumen diario, distribución de riesgo y último benchmark.
- Exploradores de eventos, experimentos, análisis, rondas, benchmarks, recursos, sistemas, versiones e indicadores.
- Búsqueda y paginación en servidor, filtros de riesgo, enlace compartible mediante URL, detalle de registros e indicadores por análisis.
- CSV de la página visible. Las fechas SQL UTC se muestran en la zona horaria del navegador.
- Actualización del resumen por SSE cada cinco segundos por defecto, reconexión y conservación explícita de datos anteriores si SQL falla.

Los exploradores avisan de actualizaciones; usa **Actualizar** para incorporarlas sin que las filas cambien mientras lees. **Pausar** congela la vista; no detiene el laboratorio. Los agregados describen el historial completo; las gráficas diarias muestran los últimos 30 días con análisis. Un día sin registro no se interpreta como cero actividad. El último benchmark muestra tasas solo cuando existen etiquetas.

## Cobertura real de eventos

La vista reúne `telemetry.TelemetryEvents`, `lab.ExperimentEvents`, `lab.QuarantineEvents` y `telemetry.TelemetryFailures`. No modifica ni consume esos registros; Pending/Published describe la entrega de telemetría y no impide consultarlos. Los reintentos se presentan como entradas separadas.

DIVE VIEW era y sigue siendo un observador de datos persistidos. WALLABEE y los demás productores guardan los eventos independientemente del navegador. Capturar interacciones que todavía no se registran exige instrumentar su origen; esta migración no convierte al cliente web en un recolector universal. No incluye payloads binarios ni expone credenciales, rutas de cuarentena o JSON completo de eventos.

El SSE transmite snapshots del resumen, no un log durable de cada evento. Al reconectar se recibe el resumen actual; el historial se consulta en el explorador paginado. Los conteos y las páginas son consultas de solo lectura y pueden avanzar mientras llegan datos nuevos.

## Demo y escritorio anterior

```powershell
$env:DIVE_VIEW_DEMO_MODE='true'
../.venv/Scripts/python.exe -m dive_view.app.main
```

La demo está claramente identificada, usa 1.275 análisis sintéticos y sus eventos derivados; las vistas sin fixtures aparecen vacías. No sustituye datos reales automáticamente ante un fallo SQL.

El código PySide6 queda disponible como alternativa opcional:

```powershell
python -m pip install -e ".[desktop]"
python -m dive_view.app.desktop
```

El inicio predeterminado ya es web. Las funciones analíticas existentes conservan sus pruebas.

## Calidad y documentación

```powershell
python -m ruff check .
python -m mypy src
python -m pytest -q
```

La validación incluye assets web, paginación, filtros, consultas parametrizadas, serialización UTC, errores SQL y recuperación del resumen. También se comprobaron las nueve vistas contra DiveViewLab y dos snapshots consecutivos del servidor SSE. La revisión visual automatizada en navegador queda pendiente por indisponibilidad de la herramienta de control.

- [Sistema visual y guía de evolución](docs/WEB_DESIGN.md)
- [Tokens de diseño reutilizables](src/dive_view/web/static/design-tokens.css)
- [Arquitectura](docs/ARCHITECTURE.md)
- [Acceso a datos](docs/DATABASE.md)
- [Analítica](docs/ANALYTICS.md)
