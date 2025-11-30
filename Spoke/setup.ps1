# Spoke Build Setup Script
# This script helps set up the build environment for Spoke

Write-Host "=== Spoke Build Setup ===" -ForegroundColor Cyan
Write-Host ""

# Check .NET SDK version
Write-Host "Checking .NET SDK versions..." -ForegroundColor Yellow
$sdks = dotnet --list-sdks
Write-Host $sdks
Write-Host ""

# Check if .NET 9 is installed
$hasNet9 = $sdks -match "9\."
if (-not $hasNet9) {
    Write-Host "WARNING: .NET 9 SDK not found!" -ForegroundColor Red
    Write-Host "The project is configured for .NET 9 but you have .NET 8 installed." -ForegroundColor Red
    Write-Host ""
    Write-Host "You have two options:" -ForegroundColor Yellow
    Write-Host "1. Install .NET 9 SDK from: https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor Green
    Write-Host "2. Downgrade the project to .NET 8 (run: .\downgrade-to-net8.ps1)" -ForegroundColor Green
    Write-Host ""
    
    $choice = Read-Host "Enter 1 to open download page, 2 to downgrade, or Q to quit"
    
    if ($choice -eq "1") {
        Start-Process "https://dotnet.microsoft.com/download/dotnet/9.0"
        Write-Host "After installing .NET 9 SDK, run this script again." -ForegroundColor Yellow
        exit
    }
    elseif ($choice -eq "2") {
        Write-Host "Downgrading project to .NET 8..." -ForegroundColor Yellow
        
        $csprojPath = "Spoke\Spoke.csproj"
        if (Test-Path $csprojPath) {
            $content = Get-Content $csprojPath -Raw
            $content = $content -replace 'net9\.0-', 'net8.0-'
            Set-Content $csprojPath -Value $content
            Write-Host "✓ Project downgraded to .NET 8" -ForegroundColor Green
        }
        else {
            Write-Host "ERROR: Could not find $csprojPath" -ForegroundColor Red
            exit 1
        }
    }
    else {
        Write-Host "Setup cancelled." -ForegroundColor Yellow
        exit
    }
}
else {
    Write-Host "✓ .NET 9 SDK found!" -ForegroundColor Green
}

Write-Host ""
Write-Host "Checking MAUI workload..." -ForegroundColor Yellow
$workloads = dotnet workload list
Write-Host $workloads
Write-Host ""

# Check if MAUI is installed
if ($workloads -notmatch "maui") {
    Write-Host "WARNING: MAUI workload not installed!" -ForegroundColor Red
    Write-Host "Installing MAUI workload (this may take a few minutes)..." -ForegroundColor Yellow
    dotnet workload install maui
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ MAUI workload installed successfully!" -ForegroundColor Green
    }
    else {
        Write-Host "ERROR: Failed to install MAUI workload" -ForegroundColor Red
        Write-Host "Try running: dotnet workload install maui" -ForegroundColor Yellow
        exit 1
    }
}
else {
    Write-Host "✓ MAUI workload found!" -ForegroundColor Green
}

Write-Host ""
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore SPOKE.sln

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Packages restored successfully!" -ForegroundColor Green
}
else {
    Write-Host "ERROR: Failed to restore packages" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Setup Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Open SPOKE.sln in Visual Studio 2022" -ForegroundColor White
Write-Host "2. Select your target platform (Android/iOS/Windows)" -ForegroundColor White
Write-Host "3. Build and run the project" -ForegroundColor White
Write-Host ""
Write-Host "Or build from command line:" -ForegroundColor Yellow
Write-Host "  dotnet build SPOKE.sln -f net9.0-android" -ForegroundColor Cyan
Write-Host "  dotnet build SPOKE.sln -f net9.0-windows10.0.19041.0" -ForegroundColor Cyan
Write-Host ""
Write-Host "See DEVELOPMENT.md for detailed instructions." -ForegroundColor Green
