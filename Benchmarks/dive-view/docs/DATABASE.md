# Datos y SQL Server

Settings carga `.env` y variables DIVE_. SQLAlchemy usa pyodbc y ODBC Driver 18. La instancia local validada es `.\SQLEXPRESS`, base DiveViewLab, autenticación Windows. DIVE VIEW no ejecuta migrations ni modifica datos.

El repositorio web usa columnas explícitas de las tablas existentes de lab, benchmark y telemetry, además de `reporting.vw_IndicatorFrequency`. No requiere `reporting.vw_ScanExplorer`, que no existe en la instalación local revisada. El repositorio del cliente de escritorio se conserva por compatibilidad y mantiene sus requisitos anteriores.

Las consultas de listados usan parámetros, OFFSET/FETCH y desempate por ID. La búsqueda escapa los comodines LIKE. Los nombres de tablas, campos de búsqueda y órdenes provienen de una lista fija del servidor. Las listas permiten hasta 100 filas por respuesta. Las consultas se realizan en conexiones separadas; una página y su conteo pueden diferir transitoriamente si llegan eventos durante la lectura.

Los agregados del resumen se calculan en SQL. La proporción de análisis clasificados como detectados no se presenta como recall; las métricas de calidad provienen del benchmark etiquetado. Los timestamps datetime2 son UTC y se serializan con Z. Los NULL permanecen NULL, no se convierten en cero.

Eventos: unión de telemetría, eventos de experimento, operaciones de cuarentena y fallos de reintento, con IDs prefijados por fuente. Los detalles públicos excluyen PayloadJson, MetadataJson, ConfigurationJson, rutas de cuarentena y LastError. DIVE VIEW solo observa la cola; no cambia ACK, estados ni reintentos.
