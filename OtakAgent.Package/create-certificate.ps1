# PowerShell script to create a self-signed certificate for MSIX package signing
param(
    [string]$CertificateName = "OtakAgentTestCert",
    [string]$Password = "TempPassword123!"
)

# Create a self-signed certificate for package signing
$cert = New-SelfSignedCertificate `
    -Type CodeSigningCert `
    -Subject "CN=TsuyoshiOtake" `
    -KeyUsage DigitalSignature `
    -FriendlyName $CertificateName `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}") `
    -NotAfter (Get-Date).AddYears(5)

Write-Host "Certificate created successfully!" -ForegroundColor Green
Write-Host "Thumbprint: $($cert.Thumbprint)"

# Export the certificate to PFX file
$pfxPath = ".\OtakAgent.pfx"
$pwd = ConvertTo-SecureString -String $Password -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $pwd | Out-Null

Write-Host "Certificate exported to: $pfxPath" -ForegroundColor Green

# Export public certificate for installation on other machines
$cerPath = ".\OtakAgent.cer"
Export-Certificate -Cert $cert -FilePath $cerPath | Out-Null
Write-Host "Public certificate exported to: $cerPath" -ForegroundColor Green

Write-Host "`nIMPORTANT:" -ForegroundColor Yellow
Write-Host "1. The certificate password is: $Password"
Write-Host "2. To install for testing, run: certutil -addstore TrustedPeople $cerPath"
Write-Host "3. Update the Package.appxmanifest Publisher field to match the certificate subject"