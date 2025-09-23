# OtakAgent MSIX Package

This is a simplified MSIX package structure for OtakAgent.

## To complete the MSIX package:

1. **Install Windows SDK** (if not already installed):
   - Download from: https://developer.microsoft.com/windows/downloads/windows-sdk/
   - Or use: winget install Microsoft.WindowsSDK

2. **Create the MSIX package**:
   `powershell
   # Navigate to the parent directory
   cd ..

   # Use makeappx (from Windows SDK)
   & "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\makeappx.exe" pack /d OtakAgent_MSIX /p OtakAgent.msix
   `

3. **Sign the package** (optional for testing):
   `powershell
   # Create a test certificate
   New-SelfSignedCertificate -Type Custom -Subject "CN=TsuyoshiOtake" -KeyUsage DigitalSignature -CertStoreLocation "Cert:\CurrentUser\My"

   # Sign the package
   & "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe" sign /a /fd SHA256 OtakAgent.msix
   `

4. **Install for testing**:
   `powershell
   Add-AppxPackage -Path OtakAgent.msix
   `

## For Microsoft Store submission:
- Upload the unsigned MSIX to Partner Center
- Microsoft will sign it with their certificate

## Manual testing (without MSIX):
Run RunOtakAgent.bat to test the application directly.
