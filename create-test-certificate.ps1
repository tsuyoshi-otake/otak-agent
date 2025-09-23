# Create and install test certificate for MSIX package
Write-Host "Creating test certificate for MSIX package signing..." -ForegroundColor Cyan

# Create certificate with matching Publisher
$cert = New-SelfSignedCertificate `
    -Type CodeSigningCert `
    -Subject "CN=TsuyoshiOtake, O=TsuyoshiOtake, C=JP" `
    -KeyUsage DigitalSignature `
    -FriendlyName "OtakAgent Test Certificate" `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

Write-Host "Certificate created successfully!" -ForegroundColor Green
Write-Host "Thumbprint: $($cert.Thumbprint)" -ForegroundColor Yellow

# Export certificate for installation
$cerPath = ".\OtakAgent_Test.cer"
Export-Certificate -Cert $cert -FilePath $cerPath | Out-Null

# Install certificate to Trusted People store
Write-Host "`nInstalling certificate to Trusted People store..." -ForegroundColor Cyan
Import-Certificate -FilePath $cerPath -CertStoreLocation Cert:\LocalMachine\TrustedPeople | Out-Null
Import-Certificate -FilePath $cerPath -CertStoreLocation Cert:\CurrentUser\TrustedPeople | Out-Null

Write-Host "Certificate installed successfully!" -ForegroundColor Green

# Sign the MSIX package
$signtool = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe"

if (Test-Path $signtool) {
    # First update manifest in MSIX structure
    Copy-Item ".\OtakAgent.Package\Package.appxmanifest" ".\OtakAgent_MSIX\AppxManifest.xml" -Force

    # Rebuild MSIX
    Write-Host "`nRebuilding MSIX package..." -ForegroundColor Cyan
    $makeappx = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\makeappx.exe"
    & $makeappx pack /d OtakAgent_MSIX /p OtakAgent_signed.msix /nv /o

    Write-Host "`nSigning MSIX package..." -ForegroundColor Cyan
    & $signtool sign /fd SHA256 /a /n "TsuyoshiOtake" OtakAgent_signed.msix

    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✅ Signed MSIX package created: OtakAgent_signed.msix" -ForegroundColor Green
        Write-Host "`nYou can now:" -ForegroundColor Cyan
        Write-Host "1. Double-click OtakAgent_signed.msix to install" -ForegroundColor White
        Write-Host "2. Or run: Add-AppxPackage -Path OtakAgent_signed.msix" -ForegroundColor White
    }
} else {
    Write-Host "signtool.exe not found!" -ForegroundColor Red
}