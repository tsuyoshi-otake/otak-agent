# Build MSIX Package for Microsoft Store
# This script creates a properly configured MSIX package with all required Store assets

param(
    [string]$OutputPath = ".\publish\otak-agent.msix"
)

Write-Host "=== Building MSIX Package for Microsoft Store ===" -ForegroundColor Cyan

# 1. Build portable version
Write-Host "`n[1/4] Building portable version..." -ForegroundColor Yellow
dotnet publish src\OtakAgent.App -c Release -r win-x64 --self-contained false -o .\publish\portable -p:PublishSingleFile=true
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# 2. Copy Store images
Write-Host "`n[2/4] Copying Store images..." -ForegroundColor Yellow
Copy-Item -Path OtakAgent.Package\Images -Destination publish\portable\Images -Recurse -Force
Write-Host "Store images copied successfully" -ForegroundColor Green

# 3. Copy and fix manifest
Write-Host "`n[3/4] Preparing manifest..." -ForegroundColor Yellow
# NOTE: Package.appxmanifest must have correct Identity values for Microsoft Partner Center:
# - Name: TsuyoshiOtake.otak-agent
# - Publisher: CN=446F33A3-1C24-4281-B1A1-017002C04FEB (from Partner Center)
Copy-Item OtakAgent.Package\Package.appxmanifest publish\portable\AppxManifest.xml -Force
Write-Host "Manifest prepared successfully" -ForegroundColor Green

# 4. Create MSIX package
Write-Host "`n[4/4] Creating MSIX package..." -ForegroundColor Yellow

# Find makeappx.exe
$makeappxPath = @(
    "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\makeappx.exe",
    "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\makeappx.exe",
    "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22000.0\x64\makeappx.exe",
    "C:\Program Files (x86)\Windows Kits\10\bin\10.0.20348.0\x64\makeappx.exe",
    "C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x64\makeappx.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $makeappxPath) {
    Write-Host "makeappx.exe not found! Please install Windows SDK." -ForegroundColor Red
    exit 1
}

Write-Host "Using makeappx: $makeappxPath" -ForegroundColor Gray

& $makeappxPath pack /d publish\portable /p $OutputPath /nv

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✅ MSIX package created successfully: $OutputPath" -ForegroundColor Green

    $fileInfo = Get-Item $OutputPath
    $sizeMB = [Math]::Round($fileInfo.Length / 1MB, 2)
    Write-Host "Package size: $sizeMB MB" -ForegroundColor Cyan

    Write-Host "`n=== Package ready for Microsoft Store submission ===" -ForegroundColor Cyan
    Write-Host "Important notes:" -ForegroundColor Yellow
    Write-Host "1. The package includes all required Store images" -ForegroundColor White
    Write-Host "2. Languages supported: ja-JP, en-US" -ForegroundColor White
    Write-Host "3. The runFullTrust capability requires approval (normal for desktop apps)" -ForegroundColor White
    Write-Host "4. You may need to sign the package before Store submission" -ForegroundColor White
} else {
    Write-Host "❌ MSIX package creation failed!" -ForegroundColor Red
    exit 1
}