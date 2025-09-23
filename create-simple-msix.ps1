# Simple MSIX Package Creator without Windows SDK
# Creates a basic MSIX structure that can be packaged later

param(
    [string]$OutputPath = ".\OtakAgent_MSIX"
)

Write-Host "Creating simplified MSIX package structure..." -ForegroundColor Cyan

# Create MSIX directory structure
$msixDir = $OutputPath
if (Test-Path $msixDir) {
    Remove-Item -Path $msixDir -Recurse -Force
}

New-Item -ItemType Directory -Path $msixDir | Out-Null
New-Item -ItemType Directory -Path "$msixDir\Assets" | Out-Null

# Copy application files
Write-Host "Copying application files..."
Copy-Item -Path ".\publish\*" -Destination $msixDir -Recurse -Force

# Copy Store assets
Write-Host "Copying Store assets..."
if (Test-Path ".\OtakAgent.Package\Images") {
    Copy-Item -Path ".\OtakAgent.Package\Images\*" -Destination "$msixDir\Assets" -Force
}

# Copy manifest
Write-Host "Copying AppxManifest..."
Copy-Item -Path ".\OtakAgent.Package\Package.appxmanifest" -Destination "$msixDir\AppxManifest.xml" -Force

# Create a simple batch file to run the app (for testing)
$runScript = @"
@echo off
echo Starting OtakAgent...
start "" "%~dp0OtakAgent.App.exe"
"@
$runScript | Out-File -FilePath "$msixDir\RunOtakAgent.bat" -Encoding ASCII

# Create installation instructions
$instructions = @"
# OtakAgent MSIX Package

This is a simplified MSIX package structure for OtakAgent.

## To complete the MSIX package:

1. **Install Windows SDK** (if not already installed):
   - Download from: https://developer.microsoft.com/windows/downloads/windows-sdk/
   - Or use: winget install Microsoft.WindowsSDK

2. **Create the MSIX package**:
   ```powershell
   # Navigate to the parent directory
   cd ..

   # Use makeappx (from Windows SDK)
   & "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\makeappx.exe" pack /d OtakAgent_MSIX /p OtakAgent.msix
   ```

3. **Sign the package** (optional for testing):
   ```powershell
   # Create a test certificate
   New-SelfSignedCertificate -Type Custom -Subject "CN=TsuyoshiOtake" -KeyUsage DigitalSignature -CertStoreLocation "Cert:\CurrentUser\My"

   # Sign the package
   & "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe" sign /a /fd SHA256 OtakAgent.msix
   ```

4. **Install for testing**:
   ```powershell
   Add-AppxPackage -Path OtakAgent.msix
   ```

## For Microsoft Store submission:
- Upload the unsigned MSIX to Partner Center
- Microsoft will sign it with their certificate

## Manual testing (without MSIX):
Run `RunOtakAgent.bat` to test the application directly.
"@
$instructions | Out-File -FilePath "$msixDir\README.md" -Encoding UTF8

Write-Host "`nSimplified MSIX structure created at: $msixDir" -ForegroundColor Green
Write-Host "Application files are ready for packaging." -ForegroundColor Yellow
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Install Windows SDK to get makeappx.exe"
Write-Host "2. Run the makeappx command to create the .msix file"
Write-Host "3. See $msixDir\README.md for detailed instructions"
Write-Host "`nFor immediate testing, run: $msixDir\RunOtakAgent.bat" -ForegroundColor Green