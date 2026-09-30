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
$publishArgs = @('-c', 'Release', '-r', 'win-x64', '--self-contained', 'true', '--artifacts-path', (Join-Path $OutputDirectory 'build'), '-p:PublishSingleFile=true', '-p:IncludeNativeLibrariesForSelfExtract=true', '-p:DebugType=None', '-p:DebugSymbols=false', "-p:BuildId=$BuildId", '--nologo')
dotnet publish (Join-Path $projectRoot 'src/Wallabee.API/Wallabee.API.csproj') @publishArgs -o $payload
if ($LASTEXITCODE -ne 0) { throw 'API publish failed.' }
dotnet publish (Join-Path $projectRoot 'src/Wallabee.Launcher/Wallabee.Launcher.csproj') @publishArgs -o $payload
if ($LASTEXITCODE -ne 0) { throw 'Launcher publish failed.' }
@{ build = $BuildId; packageFormat = 2 } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $payload 'version.json') -Encoding utf8
@{ repository = $Repository; port = 5212; autoUpdate = $true; checkIntervalSeconds = 300; allowRestore = $false } |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $payload 'default.settings.json') -Encoding utf8
# Development configuration is not part of the VM distribution.
$developmentConfig = Join-Path $payload 'appsettings.Development.json'
if (Test-Path -LiteralPath $developmentConfig) { Remove-Item -LiteralPath $developmentConfig }
Add-Type -AssemblyName System.IO.Compression.FileSystem
$updateArchive = Join-Path $OutputDirectory 'Wallabee-update-win-x64.zip'
[IO.Compression.ZipFile]::CreateFromDirectory($payload, $updateArchive, [IO.Compression.CompressionLevel]::Optimal, $false)
# Publish the same stable entry point with the complete runtime/UI payload embedded.
# Only this executable is placed in the end-user ZIP; nothing is hidden beside it.
$bootstrap = Join-Path $OutputDirectory 'bootstrap'
dotnet publish (Join-Path $projectRoot 'src/Wallabee.Launcher/Wallabee.Launcher.csproj') @publishArgs "-p:BundlePayload=$updateArchive" -o $bootstrap
if ($LASTEXITCODE -ne 0) { throw 'Single-executable bootstrap publish failed.' }
New-Item -ItemType Directory -Path $portable -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $bootstrap 'Wallabee.exe') -Destination $portable
$userArchive = Join-Path $OutputDirectory 'Wallabee-win-x64.zip'
[IO.Compression.ZipFile]::CreateFromDirectory($portable, $userArchive, [IO.Compression.CompressionLevel]::Optimal, $false)
foreach ($archive in @($updateArchive, $userArchive)) {
    $hash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
    "$hash  $([IO.Path]::GetFileName($archive))" | Set-Content -LiteralPath "$archive.sha256" -Encoding ascii
}
Write-Host "Portable application: $portable"
Write-Host "Release packages: $OutputDirectory"
