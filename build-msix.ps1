# Build MSIX package
$makeappx = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\makeappx.exe"

if (Test-Path $makeappx) {
    Write-Host "Building MSIX package..." -ForegroundColor Cyan
    & $makeappx pack /d OtakAgent_MSIX /p OtakAgent.msix /nv /o

    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nMSIX package created successfully: OtakAgent.msix" -ForegroundColor Green
        Write-Host "File size: $((Get-Item OtakAgent.msix).Length / 1MB) MB" -ForegroundColor Yellow
    } else {
        Write-Host "Failed to create MSIX package" -ForegroundColor Red
    }
} else {
    Write-Host "makeappx.exe not found at: $makeappx" -ForegroundColor Red
}