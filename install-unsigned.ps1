# Install unsigned MSIX package for testing
# Requires Developer Mode to be enabled

$packagePath = ".\OtakAgent.msix"

Write-Host "=== OtakAgent Unsigned Package Installer ===" -ForegroundColor Cyan
Write-Host ""

# Check if package exists
if (-not (Test-Path $packagePath)) {
    Write-Host "Error: OtakAgent.msix not found!" -ForegroundColor Red
    Write-Host "Please run build-msix.ps1 first to create the package." -ForegroundColor Yellow
    exit 1
}

# Check if developer mode is enabled
$devMode = Get-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock" -Name "AllowDevelopmentWithoutDevLicense" -ErrorAction SilentlyContinue
if ($devMode.AllowDevelopmentWithoutDevLicense -ne 1) {
    Write-Host "WARNING: Developer Mode might not be enabled!" -ForegroundColor Yellow
    Write-Host "To enable Developer Mode:" -ForegroundColor Yellow
    Write-Host "1. Open Settings (Win + I)" -ForegroundColor Gray
    Write-Host "2. Go to System > For developers" -ForegroundColor Gray
    Write-Host "3. Turn on Developer Mode" -ForegroundColor Gray
    Write-Host ""
    $confirm = Read-Host "Do you want to continue anyway? (y/n)"
    if ($confirm -ne 'y') {
        exit
    }
}

Write-Host "Installing unsigned MSIX package..." -ForegroundColor Yellow
Write-Host "Package: $packagePath" -ForegroundColor Gray
Write-Host ""

try {
    # Remove old version if exists
    $oldPackage = Get-AppxPackage -Name "TsuyoshiOtake.OtakAgent" -ErrorAction SilentlyContinue
    if ($oldPackage) {
        Write-Host "Removing old version..." -ForegroundColor Yellow
        Remove-AppxPackage -Package $oldPackage.PackageFullName
    }

    # Install new package
    Add-AppxPackage -Path $packagePath -AllowUnsigned -ForceApplicationShutdown

    Write-Host ""
    Write-Host "✅ Installation successful!" -ForegroundColor Green
    Write-Host ""
    Write-Host "You can now launch 'Otak Agent' from:" -ForegroundColor Cyan
    Write-Host "• Start Menu" -ForegroundColor White
    Write-Host "• Search 'Otak Agent' in Windows Search" -ForegroundColor White
    Write-Host ""

    # Ask if user wants to launch now
    $launch = Read-Host "Do you want to launch Otak Agent now? (y/n)"
    if ($launch -eq 'y') {
        Start-Process "shell:AppsFolder\TsuyoshiOtake.OtakAgent_8a7b5a8c1e2d4f3b9c6d2e4f5a6b7c8d!App"
    }
}
catch {
    Write-Host ""
    Write-Host "❌ Installation failed!" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "1. Make sure Developer Mode is enabled" -ForegroundColor Gray
    Write-Host "2. Run PowerShell as Administrator" -ForegroundColor Gray
    Write-Host "3. Check Windows version (requires Windows 10 1903 or later)" -ForegroundColor Gray
}