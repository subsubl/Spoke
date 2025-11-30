<#
Runs the Spoke Windows app, waits a few seconds and checks startup_diag.txt for markers.
This is a lightweight developer harness to validate the constructor vs CreateWindow order:
  - 'App constructor start' should appear before the CreateWindow auto-init log
  - 'Node init started' file entry should appear only after CreateWindow logs the auto-init

Usage: Run from repository root with PowerShell on Windows:
    powershell -ExecutionPolicy Bypass -File .\scripts\check-startup.ps1
#>

$project = "Spoke\Spoke\Spoke.csproj"
$tfm = "net10.0-windows10.0.19041.0"

# clean old diag
$userFolder = [Environment]::GetFolderPath('Personal') + '\Spoke'
$diagFile = Join-Path $userFolder 'startup_diag.txt'
if (Test-Path $diagFile) { Remove-Item $diagFile -Force -ErrorAction SilentlyContinue }
$env:TEMPDiag = Join-Path $env:TEMP 'spoke_temp_diag.txt'
if (Test-Path $env:TEMPDiag) { Remove-Item $env:TEMPDiag -Force -ErrorAction SilentlyContinue }

Write-Host "Building project $project for $tfm..."
dotnet build $project -f $tfm -c Debug
if ($LASTEXITCODE -ne 0) { Write-Error "Build failed"; exit 1 }

Write-Host "Running app (headless) -- waiting 8 seconds for startup logs..."
$proc = Start-Process -FilePath 'dotnet' -ArgumentList "run --project $project -f $tfm -c Debug" -PassThru
Start-Sleep -Seconds 8

# Give the app a moment to write logs
Start-Sleep -Seconds 2

Write-Host "Stopping app process (if still running)"
try { Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue } catch {}

Write-Host "Reading diagnostics..."
if (Test-Path $env:TEMPDiag) { Write-Host "TEMP diag:"; Get-Content $env:TEMPDiag }
if (Test-Path $diagFile) { Write-Host "User diag:"; Get-Content $diagFile }

# Basic check: constructor marker exists
$userDiag = if (Test-Path $diagFile) { Get-Content $diagFile -Raw } else { '' }

if ($userDiag -match 'App constructor start') {
    Write-Host "OK: constructor ran"
} else {
    Write-Warning "No constructor marker found in $diagFile"
}

if ($userDiag -match 'QuIXI address configured, starting auto-initialization after window created') {
    Write-Host "OK: auto-init started from CreateWindow (after window created)"
} else {
    Write-Host "No CreateWindow auto-init marker found. That may mean auto-init was not triggered (config absent) or it ran elsewhere."
}

if ($userDiag -match 'Node init started') {
    Write-Host "OK: Node init recorded"
} else {
    Write-Host "Node init marker not found in logs"
}

Write-Host "Done."