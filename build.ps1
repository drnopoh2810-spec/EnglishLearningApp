# English Learning App Build Script
# Requires: .NET 8 SDK, WiX Toolset (for MSI)

param(
    [string]$Configuration = "Release",
    [switch]$BuildInstaller = $false
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "English Learning App - Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Step 1: Clean
Write-Host "`n[1/5] Cleaning..." -ForegroundColor Yellow
if (Test-Path "bin") { Remove-Item -Recurse -Force "bin" }
if (Test-Path "obj") { Remove-Item -Recurse -Force "obj" }

# Step 2: Restore packages
Write-Host "`n[2/5] Restoring packages..." -ForegroundColor Yellow
dotnet restore

# Step 3: Build
Write-Host "`n[3/5] Building ($Configuration)..." -ForegroundColor Yellow
dotnet build --configuration $Configuration --no-restore

# Step 4: Publish (self-contained for installer)
Write-Host "`n[4/5] Publishing..." -ForegroundColor Yellow
dotnet publish --configuration $Configuration --self-contained true --runtime win-x64 --output "bin\Publish" --no-build

# Step 5: Create Installer (optional)
if ($BuildInstaller) {
    Write-Host "`n[5/5] Building installer..." -ForegroundColor Yellow
    
    # Check for WiX
    $wixPath = "${env:WIX}bin\candle.exe"
    if (-not (Test-Path $wixPath)) {
        Write-Host "WiX Toolset not found. Installing via dotnet..." -ForegroundColor Yellow
        dotnet tool install --global wix --version 4.0.0
    }
    
    # Build MSI using WiX v4
    wix build -o "bin\EnglishLearningApp_1.0.0.msi" Installer\EnglishLearningApp.wxs
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "Build Complete!" -ForegroundColor Green
Write-Host "Output: $(Resolve-Path "bin\Publish")" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

if (-not $BuildInstaller) {
    Write-Host "`nTo build installer, run: .\build.ps1 -BuildInstaller" -ForegroundColor Cyan
}
