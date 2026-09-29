[CmdletBinding()]
param([Parameter(Mandatory)][string]$InstallDirectory)
$ErrorActionPreference = 'Stop'
$InstallDirectory = (Resolve-Path -LiteralPath $InstallDirectory).Path
$settingsPath = Join-Path $InstallDirectory 'wallabee.settings.json'
$originalSettings = [IO.File]::ReadAllText($settingsPath)
$listener = New-Object System.Net.Sockets.TcpListener([Net.IPAddress]::Loopback, 0)
$listener.Start()
$port = $listener.LocalEndpoint.Port
$listener.Stop()
$settings = $originalSettings | ConvertFrom-Json
$settings.port = $port
$settings.autoUpdate = $false
$settings | ConvertTo-Json | Set-Content -LiteralPath $settingsPath -Encoding utf8
$launcher = $null
try {
    $launcher = Start-Process -FilePath (Join-Path $InstallDirectory 'Wallabee.exe') -WorkingDirectory $InstallDirectory -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $InstallDirectory 'smoke.stdout.log') -RedirectStandardError (Join-Path $InstallDirectory 'smoke.stderr.log')
    $expected = (Get-Content -LiteralPath (Join-Path $InstallDirectory 'current.txt') -Raw).Trim()
    $healthy = $false
    for ($attempt = 0; $attempt -lt 45; $attempt++) {
        if ($launcher.HasExited) { throw "Launcher exited: $(Get-Content (Join-Path $InstallDirectory 'smoke.stderr.log') -Raw)" }
        try {
            $health = Invoke-RestMethod "http://127.0.0.1:$port/api/health" -TimeoutSec 2
            if ($health.build -eq $expected) { $healthy = $true; break }
        } catch { }
        Start-Sleep -Seconds 1
    }
    if (-not $healthy) { throw 'Packaged API failed its health check.' }
    $sample = Join-Path $InstallDirectory 'lab/smoke-test.txt'
    [IO.File]::WriteAllText($sample, 'benign sample')
    $body = @{filePath = $sample} | ConvertTo-Json
    $scan = Invoke-RestMethod "http://127.0.0.1:$port/api/scan/file" -Method Post -ContentType 'application/json' -Body $body
    if ($scan.sha256 -ne (Get-FileHash -LiteralPath $sample -Algorithm SHA256).Hash) { throw 'Unexpected SHA-256 from packaged API.' }
    Write-Host "PASS: packaged launcher, API health/build, and file scan ($expected)."
} finally {
    if ($launcher -and -not $launcher.HasExited) {
        # Closing the bootstrap's job must also terminate its API and supervisor.
        Stop-Process -Id $launcher.Id -Force
        $launcher.WaitForExit()
    }
    [IO.File]::WriteAllText($settingsPath, $originalSettings)
}
Start-Sleep -Seconds 1
$stillRunning = $false
try { $null = Invoke-RestMethod "http://127.0.0.1:$port/api/health" -TimeoutSec 1; $stillRunning = $true } catch { }
if ($stillRunning) { throw 'API survived launcher shutdown.' }
Write-Host 'PASS: no orphan API after launcher exits.'
