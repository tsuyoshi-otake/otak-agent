# Script to sign MSIX package with self-signed certificate

Write-Host "Creating and signing MSIX package..." -ForegroundColor Cyan

# Create self-signed certificate
Write-Host "Creating self-signed certificate..." -ForegroundColor Yellow
$cert = New-SelfSignedCertificate `
    -Type Custom `
    -Subject "CN=TsuyoshiOtake, O=TsuyoshiOtake, L=Tokyo, S=Tokyo, C=JP" `
    -KeyUsage DigitalSignature `
    -FriendlyName "OtakAgent Test Certificate" `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}") `
    -NotAfter (Get-Date).AddYears(5)

Write-Host "Certificate created with thumbprint: $($cert.Thumbprint)" -ForegroundColor Green

# Export certificate for installation
$cerPath = ".\OtakAgent.cer"
Export-Certificate -Cert $cert -FilePath $cerPath | Out-Null
Write-Host "Certificate exported to: $cerPath" -ForegroundColor Green

# Sign the MSIX package
$signtool = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe"
if (Test-Path $signtool) {
    Write-Host "Signing MSIX package..." -ForegroundColor Yellow
    & $signtool sign /fd SHA256 /a /n "TsuyoshiOtake" OtakAgent.msix

    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nPackage signed successfully!" -ForegroundColor Green

        Write-Host "`nTo install the signed package:" -ForegroundColor Cyan
        Write-Host "1. Install the certificate (run as Administrator):" -ForegroundColor Yellow
        Write-Host "   certutil -addstore TrustedPeople OtakAgent.cer"
        Write-Host ""
        Write-Host "2. Double-click OtakAgent.msix to install" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Or install via PowerShell:" -ForegroundColor Yellow
        Write-Host "   Add-AppxPackage -Path OtakAgent.msix"
    } else {
        Write-Host "Failed to sign package" -ForegroundColor Red
    }
} else {
    Write-Host "signtool.exe not found" -ForegroundColor Red
    Write-Host "Please install Windows SDK first" -ForegroundColor Yellow
}