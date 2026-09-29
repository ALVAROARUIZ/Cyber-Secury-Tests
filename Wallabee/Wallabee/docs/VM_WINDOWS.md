# Wallabee en Windows x64

## Primera instalacion

1. Descarga `Wallabee-win-x64.zip` de la ultima release `wallabee-*` en
   https://github.com/ALVAROARUIZ/Cyber-Secury-Tests/releases/latest
   (inicia sesion en GitHub porque el repositorio es privado).
2. Extrae **todo** el ZIP, por ejemplo en `C:\Wallabee`. Conserva `versions`,
   `current.txt` y `wallabee.settings.json` junto a `Wallabee.exe`.
3. Configura el acceso a GitHub como se explica debajo y ejecuta el
   **Wallabee.exe de la carpeta principal**. No hace falta instalar .NET,
   Visual Studio ni Git, ni ejecutar como administrador.
4. Comprueba http://localhost:5212/api/health. El campo `build` identifica
   el commit instalado. Coloca los archivos de prueba dentro de `C:\Wallabee\lab`.
   Wallabee sigue siendo una API; el ejecutable no agrega una interfaz grafica.

## Acceso a las actualizaciones de un repositorio privado

Crea en GitHub un token personal de acceso granular (fine-grained), limitado a
`ALVAROARUIZ/Cyber-Secury-Tests`, con **Contents: Read-only**. En PowerShell dentro
de la VM ejecuta este bloque; el token se solicita oculto y no se guarda en el
historial ni en archivos del proyecto:

```powershell
$secureToken = Read-Host 'Token de GitHub (Contents: Read-only)' -AsSecureString
$credential = New-Object System.Net.NetworkCredential('', $secureToken)
$env:WALLABEE_GITHUB_TOKEN = $credential.Password
& 'C:\Wallabee\Wallabee.exe'
```

Esto configura el token solo para esa sesion. Para conservarlo entre sesiones,
antes de ejecutar Wallabee puedes guardarlo como variable del usuario de Windows:

```powershell
[Environment]::SetEnvironmentVariable('WALLABEE_GITHUB_TOKEN', $credential.Password, 'User')
```

La variable persistente es texto accesible para tu usuario, no una caja fuerte.
Usa un token de solo lectura y una fecha de expiracion. Abre una terminal nueva
o vuelve a iniciar sesion si Windows no refleja la variable al abrir el EXE.
Sin token valido, o sin Internet, la version instalada sigue funcionando;
el error de actualizacion queda en `logs/updates.log`.

## Funcionamiento

- Cada **push a main** ejecuta las pruebas y crea una release identificada por
  el SHA del commit. Guardar un archivo o hacer un commit sin push no publica
  una actualizacion. Si las pruebas o la publicacion fallan, sigue la release anterior.
- Wallabee consulta GitHub al iniciar y cada 300 segundos mientras esta abierto.
  No instala servicios ni tareas programadas; debes abrirlo de nuevo al reiniciar la VM.
- Descarga el paquete, verifica SHA-256 y comprueba su identificador de version.
  Reinicia la API y valida `/api/health`. Si la nueva API no inicia, vuelve a la
  version anterior y no vuelve a intentar esa misma version automaticamente.
- El reinicio **interrumpe escaneos en curso**. Benchmarks y registros de
  cuarentena que solo existen en memoria se pierden al reiniciar. Los archivos
  del laboratorio y de cuarentena permanecen en disco.
- `lab`, `logs` y `wallabee.settings.json` quedan fuera de las versiones y se
  conservan. Las versiones anteriores se mantienen en `versions` para recuperacion.
- El EXE de entrada es un bootstrap estable. La API y el supervisor de
  actualizaciones se ejecutan desde la version activa y se actualizan juntos.
- Cierra con **Ctrl+C**. El puerto escucha solo en la VM (`127.0.0.1`).

Puedes cambiar `port`, `checkIntervalSeconds` (minimo 10), `autoUpdate` y
`allowRestore` en `wallabee.settings.json` y reiniciar Wallabee. Para una prueba
que no deba interrumpirse, usa `"autoUpdate": false` temporalmente.

## Recuperacion manual

Cierra Wallabee. Cambia el contenido de `current.txt` por el nombre de una
carpeta existente bajo `versions` y pon `autoUpdate` en `false`. Vuelve a abrir
`Wallabee.exe`. Para reintentar una version rechazada, elimina
`failed-version.txt` con Wallabee cerrado y reactiva `autoUpdate`.

## Compilar localmente

Desde el proyecto, con .NET SDK 10 instalado:

```powershell
dotnet test HoneyBadgerVsWallabee.sln -c Release
powershell -ExecutionPolicy Bypass -File scripts/Publish-Wallabee.ps1
```

El resultado queda bajo `artifacts/<identificador>/`. Los binarios se publican
como archivos de GitHub Releases; no se guardan en el historial Git.

Referencias: [publicacion autocontenida de .NET](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview),
[descargas de assets privados de GitHub](https://docs.github.com/en/rest/releases/assets).
