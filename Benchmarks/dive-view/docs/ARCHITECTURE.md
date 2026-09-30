# Arquitectura web

`Navegador → FastAPI → WebService → WebRepository → SQL Server`

El punto de entrada `app/main.py` inicia Uvicorn. `web/app.py` compone los servicios, sirve assets locales y expone consultas de solo lectura. `web/static` contiene la interfaz responsive sin dependencias frontend externas. Los módulos de analítica existentes siguen independientes de la presentación.

`LiveState` consulta el resumen en un hilo de trabajo y conserva un snapshot compartido por instancia, sin bloquear el bucle asíncrono. Un fallo indica indisponibilidad y retiene los últimos datos; el siguiente ciclo reintenta. El ciclo se ejecuta aunque no haya navegadores abiertos. La recolección original pertenece a los productores, no a este ciclo.

`GET /api/live` transmite snapshots SSE y heartbeats. `GET /api/overview` entrega el snapshot JSON. `GET /api/tables/{kind}` aplica límite de 100 filas, búsqueda parametrizada y orden con desempate por ID. `GET /api/scans/{id}` agrega los indicadores. `GET /api/config` expone únicamente configuración pública. No hay endpoints de escritura.

Las tablas conservan su página hasta una actualización explícita. El resumen se actualiza automáticamente y el cliente reconecta tras una desconexión. SSE no garantiza entrega individual de cada evento: los eventos durables se consultan desde SQL.

Los assets se incluyen en el paquete Python. `app/desktop.py` y `ui/` conservan el cliente anterior como extra opcional. No son dependencias del inicio web. Los tokens y la guía visual están en `web/static/design-tokens.css` y `docs/WEB_DESIGN.md`.

Implementación HTTP basada en [respuestas de FastAPI](https://fastapi.tiangolo.com/advanced/custom-response/).
