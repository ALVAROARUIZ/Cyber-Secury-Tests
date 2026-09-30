# WALLABEE para Windows x64

## Descargar, descomprimir y abrir

1. En [GitHub Releases](https://github.com/ALVAROARUIZ/Cyber-Secury-Tests/releases/latest), descarga **Wallabee-win-x64.zip** de una release nueva que indique el formato de ejecutable único.
2. Descomprime el ZIP. Encontrarás únicamente **Wallabee.exe**, con el icono de WALLABEE.
3. Haz doble clic. El ejecutable prepara sus componentes y abre el panel web en el navegador predeterminado, normalmente en http://127.0.0.1:5212/.

No hace falta instalar .NET, Visual Studio ni Git. La aplicación no pide permisos de administrador. En un repositorio privado debes iniciar sesión en GitHub para descargar el ZIP. **Source code.zip** es el código para desarrolladores y **Wallabee-update-win-x64.zip** es el paquete interno de actualización.

El panel se sirve desde la API local; no se abre como un archivo HTML suelto. La carpeta descomprimida permanece con un único EXE incluso después de utilizarlo.

## Cerrar y volver a abrir

WALLABEE muestra su icono en la bandeja de Windows, junto al reloj. Desde allí puedes:

- Abrir WALLABEE en el navegador.
- Abrir la carpeta del laboratorio para colocar muestras.
- Abrir los registros de diagnóstico.
- Salir de WALLABEE y detener su API.

Cerrar la pestaña no cierra el motor. Abrir nuevamente el ejecutable reutiliza la instancia que ya está funcionando. El ejecutable no instala servicios ni tareas de inicio automático.

## Datos y conexión

Los componentes y datos locales se guardan en **`%LOCALAPPDATA%\Wallabee`**, fuera de la carpeta descargada:

- `lab`: muestras del laboratorio y cuarentena.
- `versions`: API, supervisor, HTML y componentes de cada versión.
- `logs`: diagnósticos.
- `wallabee.settings.json`: configuración persistente.
- `current.txt`: versión activa; `bundle-version.txt`: descarga instalada.

SQL Server sigue siendo necesario para guardar y consultar resultados. La configuración incluida espera `.\SQLEXPRESS`, base `DiveViewLab`, autenticación Windows. Un responsable del laboratorio debe provisionar esa base con las migrations indicadas en [DATABASE.md](DATABASE.md), o configurar una conexión existente. El paquete no instala SQL Server ni migra automáticamente una base de producción.

Si SQL no responde, el panel puede abrirse y muestra la desconexión; los análisis no se presentan como guardados. El ejecutable incluye .NET y sus componentes, no un servidor SQL.

Un responsable puede establecer `ConnectionStrings__DiveViewDatabase` en el entorno o añadir `connectionString` a `wallabee.settings.json`. Esa cadena permanece en el equipo y no debe incorporarse con credenciales a una release. La opción de settings tiene precedencia sobre la variable heredada.

## Actualizaciones

El supervisor consulta GitHub al iniciar y cada 300 segundos. Sin acceso a Internet o al repositorio privado, la versión instalada continúa funcionando. Para actualizaciones privadas se admite `WALLABEE_GITHUB_TOKEN` con Contents: Read-only; el launcher no pasa ese token al motor de análisis.

La actualización comprueba el SHA-256, el identificador y el formato del paquete. Activa la nueva API tras verificar su salud; si falla, vuelve a la anterior. La activación reinicia el motor y puede interrumpir análisis en curso. Los resultados ya persistidos en SQL permanecen guardados. Para sesiones que no deban interrumpirse, configura `autoUpdate` en `false`.

La misma descarga no sobrescribe una actualización posterior al volver a abrirse. Descargar y ejecutar un bundle diferente instala esa versión de forma explícita, conservando configuración y muestras.

## Compilar y validar (desarrolladores)

```powershell
dotnet test HoneyBadgerVsWallabee.sln -c Release
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Publish-Wallabee.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Package.ps1 -InstallDirectory artifacts/<build>/Wallabee
```

El publicador genera:

- `Wallabee-win-x64.zip`: contiene solo `Wallabee.exe`, para usuarios.
- `Wallabee-update-win-x64.zip`: payload interno del supervisor y la API.
- Archivos SHA-256 para ambos ZIP.

La primera publicación compila API y supervisor autocontenidos. La segunda incorpora ese payload como recurso del EXE de entrada. El icono original se conserva en `src/Wallabee.Launcher/Assets/wallabee.ico`; la UI incluye el mismo icono como favicon.

La prueba del paquete extrae el ZIP en una carpeta nueva, emplea una base `WallabeePackageTests_<guid>`, comprueba la identidad efectiva de esa base, ejecuta un análisis y verifica dónde se guardó. La limpieza nombra explícitamente esa base temporal; no usa `dotnet ef database drop`. La prueba se ejecuta con Windows PowerShell para usar su cliente SQL integrado.

`WALLABEE_DATA_DIRECTORY` permite aislar los archivos de una ejecución de pruebas. `--headless --no-browser` omite la bandeja y el navegador para automatización. No son necesarios para el usuario final.

GitHub Actions ejecuta las pruebas con SQL Server LocalDB y publica los assets cuando se suben los cambios a main. Los binarios no forman parte del historial de código. Los permisos de GitHub no permiten mostrar una carpeta diferente según si el visitante es desarrollador; la distribución de usuarios se separa mediante Releases.

Referencia: [publicación de .NET como archivo único](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview).
