[CmdletBinding()]
param(
    [ValidatePattern('^[a-zA-Z0-9-]{1,80}$')]
    [string]$BuildId = ('local-' + (Get-Date -Format 'yyyyMMddHHmmss')),
    [string]$OutputDirectory,
    [string]$Repository = 'ALVAROARUIZ/Cyber-Secury-Tests'
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $projectRoot "artifacts/$BuildId" }
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path -LiteralPath $OutputDirectory) { throw "Output directory already exists: $OutputDirectory. Choose a new directory." }
$payload = Join-Path $OutputDirectory 'payload'
$portable = Join-Path $OutputDirectory 'Wallabee'
$publishArgs = @('-c', 'Release', '-r', 'win-x64', '--self-contained', 'true', '-p:PublishSingleFile=true', '-p:IncludeNativeLibrariesForSelfExtract=true', '-p:DebugType=None', '-p:DebugSymbols=false', "-p:BuildId=$BuildId", '--nologo')
dotnet publish (Join-Path $projectRoot 'src/Wallabee.API/Wallabee.API.csproj') @publishArgs -o $payload
if ($LASTEXITCODE -ne 0) { throw 'API publish failed.' }
dotnet publish (Join-Path $projectRoot 'src/Wallabee.Launcher/Wallabee.Launcher.csproj') @publishArgs -o $payload
if ($LASTEXITCODE -ne 0) { throw 'Launcher publish failed.' }
@{ build = $BuildId } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $payload 'version.json') -Encoding utf8
# Development configuration is not part of the VM distribution.
$developmentConfig = Join-Path $payload 'appsettings.Development.json'
if (Test-Path -LiteralPath $developmentConfig) { Remove-Item -LiteralPath $developmentConfig }
$versionDirectory = Join-Path $portable "versions/$BuildId"
New-Item -ItemType Directory -Path $versionDirectory -Force | Out-Null
Copy-Item -Path "$payload/*" -Destination $versionDirectory -Recurse
Copy-Item -LiteralPath (Join-Path $payload 'Wallabee.exe') -Destination $portable
$BuildId | Set-Content -LiteralPath (Join-Path $portable 'current.txt') -Encoding ascii
@{ repository = $Repository; port = 5212; autoUpdate = $true; checkIntervalSeconds = 300; allowRestore = $false } |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $portable 'wallabee.settings.json') -Encoding utf8
Copy-Item -LiteralPath (Join-Path $projectRoot 'docs/VM_WINDOWS.md') -Destination (Join-Path $portable 'README.md')
Add-Type -AssemblyName System.IO.Compression.FileSystem
foreach ($entry in @(@{Source=$payload;Name='Wallabee-update-win-x64.zip'}, @{Source=$portable;Name='Wallabee-win-x64.zip'})) {
    $archive = Join-Path $OutputDirectory $entry.Name
    [IO.Compression.ZipFile]::CreateFromDirectory($entry.Source, $archive, [IO.Compression.CompressionLevel]::Optimal, $false)
    $hash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
    "$hash  $($entry.Name)" | Set-Content -LiteralPath "$archive.sha256" -Encoding ascii
}
Write-Host "Portable application: $portable"
Write-Host "Release packages: $OutputDirectory"
