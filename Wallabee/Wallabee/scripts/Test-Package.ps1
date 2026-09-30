[CmdletBinding()]
param([Parameter(Mandatory)][string]$InstallDirectory)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$InstallDirectory = (Resolve-Path -LiteralPath $InstallDirectory).Path
$archivePath = Join-Path (Split-Path $InstallDirectory -Parent) 'Wallabee-win-x64.zip'
$smokeRoot = Join-Path (Split-Path $InstallDirectory -Parent) ('smoke-' + [Guid]::NewGuid().ToString('N'))
$download = Join-Path $smokeRoot 'download'
$data = Join-Path $smokeRoot 'appdata'
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::ExtractToDirectory($archivePath, $download)
$files = @(Get-ChildItem -LiteralPath $download -Force)
if ($files.Count -ne 1 -or $files[0].Name -ne 'Wallabee.exe') { throw 'End-user ZIP must contain only Wallabee.exe.' }
New-Item -ItemType Directory -Path $data -Force | Out-Null
$listener = New-Object System.Net.Sockets.TcpListener([Net.IPAddress]::Loopback, 0)
$listener.Start()
$port = $listener.LocalEndpoint.Port
$listener.Stop()
$baseConnection = $env:WALLABEE_TEST_SQLSERVER
if (-not $baseConnection) {
    $config = Get-Content -LiteralPath (Join-Path $projectRoot 'src/Wallabee.API/appsettings.json') -Raw | ConvertFrom-Json
    $baseConnection = $config.ConnectionStrings.DiveViewDatabase
}
$builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
# Use CLR accessors: PowerShell's dictionary adapter can shadow ConnectionString.
$builder.set_ConnectionString($baseConnection)
$databaseName = 'WallabeePackageTests_' + [Guid]::NewGuid().ToString('N')
$builder.set_InitialCatalog($databaseName)
$testConnection = $builder.get_ConnectionString()
$verifiedBuilder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$verifiedBuilder.set_ConnectionString($testConnection)
if ($verifiedBuilder.get_InitialCatalog() -cne $databaseName) { throw 'Effective test database does not match the generated database.' }
if ($databaseName -notmatch '^WallabeePackageTests_[a-f0-9]{32}$') { throw 'Invalid temporary database name.' }
@{ port = $port; autoUpdate = $false; checkIntervalSeconds = 300; connectionString = $testConnection } |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $data 'wallabee.settings.json') -Encoding utf8
$previousData = $env:WALLABEE_DATA_DIRECTORY
$previousConnection = $env:ConnectionStrings__DiveViewDatabase
$launcher = $null
$databaseCreated = $false
$efArgs = @('--project', (Join-Path $projectRoot 'src/Wallabee.Infrastructure'), '--startup-project', (Join-Path $projectRoot 'src/Wallabee.API'), '--configuration', 'Release', '--no-build', '--context', 'DiveViewDbContext')
try {
    $env:WALLABEE_DATA_DIRECTORY = $data
    $env:ConnectionStrings__DiveViewDatabase = $testConnection
    dotnet tool restore --tool-manifest (Join-Path $projectRoot 'dotnet-tools.json')
    if ($LASTEXITCODE -ne 0) { throw 'EF tool restore failed.' }
    $databaseCreated = $true
    dotnet ef database update --connection $testConnection @efArgs
    if ($LASTEXITCODE -ne 0) { throw 'Smoke database migration failed.' }
    $verification = New-Object System.Data.SqlClient.SqlConnection($testConnection)
    try {
        $verification.Open()
        $verifyCommand = $verification.CreateCommand()
        $verifyCommand.CommandText = 'SELECT DB_NAME()'
        if ($verifyCommand.ExecuteScalar() -cne $databaseName) { throw 'Connected to an unexpected database; refusing to run the smoke test.' }
    } finally { $verification.Dispose() }
    $launcher = Start-Process -FilePath (Join-Path $download 'Wallabee.exe') -ArgumentList '--headless','--no-browser' -WorkingDirectory $download -WindowStyle Hidden -PassThru
    $healthy = $false
    for ($attempt = 0; $attempt -lt 60; $attempt++) {
        if ($launcher.HasExited) {
            $log = Join-Path $data 'logs/launcher.log'
            if (Test-Path -LiteralPath $log) { Write-Host (Get-Content -LiteralPath $log -Raw) }
            throw 'Packaged launcher exited before readiness.'
        }
        try {
            $expected = (Get-Content -LiteralPath (Join-Path $data 'current.txt') -Raw -ErrorAction Stop).Trim()
            $health = Invoke-RestMethod "http://127.0.0.1:$port/api/health" -TimeoutSec 2
            if ($health.build -eq $expected) { $healthy = $true; break }
        } catch { }
        Start-Sleep -Seconds 1
    }
    if (-not $healthy) { throw 'Packaged API failed its health check.' }
    $html = Invoke-WebRequest "http://127.0.0.1:$port/" -UseBasicParsing
    if ($html.Content -notmatch 'WALLABEE' -or $html.Content -notmatch 'dashboard.js') { throw 'Packaged dashboard missing.' }
    foreach ($asset in @('dashboard.js', 'dashboard.css', 'favicon.ico')) {
        $null = Invoke-WebRequest "http://127.0.0.1:$port/$asset" -UseBasicParsing
    }
    $sample = Join-Path $data 'lab/smoke-test.txt'
    [IO.File]::WriteAllText($sample, 'benign sample')
    $body = @{filePath = $sample} | ConvertTo-Json
    $scan = Invoke-RestMethod "http://127.0.0.1:$port/api/scan/file" -Method Post -ContentType 'application/json' -Body $body
    if ($scan.sha256 -ne (Get-FileHash -LiteralPath $sample -Algorithm SHA256).Hash) { throw 'Unexpected SHA-256 from packaged API.' }
    $verification = New-Object System.Data.SqlClient.SqlConnection($testConnection)
    try {
        $verification.Open()
        $verifyCommand = $verification.CreateCommand()
        $verifyCommand.CommandText = 'SELECT COUNT_BIG(*) FROM lab.Scans WHERE ScanId=@scan'
        $null = $verifyCommand.Parameters.AddWithValue('@scan', [Guid]$scan.scanId)
        if ($verifyCommand.ExecuteScalar() -ne 1) { throw 'Packaged API did not persist in the isolated test database.' }
    } finally { $verification.Dispose() }
    $duplicate = Start-Process -FilePath (Join-Path $download 'Wallabee.exe') -ArgumentList '--headless','--no-browser' -WindowStyle Hidden -PassThru
    if (-not $duplicate.WaitForExit(10000) -or $duplicate.ExitCode -ne 0) { throw 'Second launch did not reuse the running instance.' }
    if (@(Get-ChildItem -LiteralPath $download -Force).Count -ne 1) { throw 'Runtime files leaked into the download folder.' }
    Write-Host "PASS: one EXE, extracted runtime, SQL health, dashboard/assets, scan and second launch ($expected)."
} finally {
    if ($launcher -and -not $launcher.HasExited) {
        Stop-Process -Id $launcher.Id -Force
        $launcher.WaitForExit()
    }
    if ($databaseCreated) {
        if ($databaseName -notmatch '^WallabeePackageTests_[a-f0-9]{32}$') { throw 'Refusing to drop an unrelated database.' }
        # Never use a design-time factory or an inferred database for cleanup.
        $cleanupBuilder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
        $cleanupBuilder.set_ConnectionString($testConnection)
        if ($cleanupBuilder.get_InitialCatalog() -cne $databaseName) { throw 'Cleanup database mismatch.' }
        $cleanupBuilder.set_InitialCatalog('master')
        $cleanup = New-Object System.Data.SqlClient.SqlConnection($cleanupBuilder.get_ConnectionString())
        try {
            $cleanup.Open()
            $command = $cleanup.CreateCommand()
            $command.CommandText = "IF DB_ID(@name) IS NOT NULL BEGIN ALTER DATABASE [$databaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$databaseName]; END"
            $null = $command.Parameters.AddWithValue('@name', $databaseName)
            $null = $command.ExecuteNonQuery()
        } finally { $cleanup.Dispose() }
    }
    $env:WALLABEE_DATA_DIRECTORY = $previousData
    $env:ConnectionStrings__DiveViewDatabase = $previousConnection
}
Start-Sleep -Seconds 1
$stillRunning = $false
try { $null = Invoke-RestMethod "http://127.0.0.1:$port/api/runtime" -TimeoutSec 1; $stillRunning = $true } catch { }
if ($stillRunning) { throw 'API survived launcher shutdown.' }
Write-Host 'PASS: no orphan API after launcher exits.'
