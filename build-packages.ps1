# OtakAgent Package Builder Script
# This script builds all distribution packages for OtakAgent

param(
    [switch]$Portable,
    [switch]$MSIX,
    [switch]$MSI,
    [switch]$All
)

$ErrorActionPreference = "Stop"

# Set configuration
$Configuration = "Release"
$Platform = "x64"
$Runtime = "win-x64"
$OutputDir = ".\publish"

# Ensure output directory exists
if (!(Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

Write-Host "=== OtakAgent Package Builder ===" -ForegroundColor Cyan

# Build Release configuration
Write-Host "`nBuilding Release configuration..." -ForegroundColor Yellow
dotnet build -c $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# 1. Build Portable Package
if ($Portable -or $All) {
    Write-Host "`n[1/3] Creating Portable Package..." -ForegroundColor Green

    $portableDir = Join-Path $OutputDir "portable"
    dotnet publish src/OtakAgent.App/OtakAgent.App.csproj `
        -c $Configuration `
        -r $Runtime `
        --self-contained false `
        -o $portableDir `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true

    if ($LASTEXITCODE -eq 0) {
        # Create ZIP archive
        $zipPath = Join-Path $OutputDir "OtakAgent-Portable.zip"
        Write-Host "Creating ZIP archive: $zipPath" -ForegroundColor Cyan
        Compress-Archive -Path "$portableDir\*" -DestinationPath $zipPath -Force
        Write-Host "✓ Portable package created: $zipPath" -ForegroundColor Green
    } else {
        Write-Host "✗ Portable package build failed!" -ForegroundColor Red
    }
}

# 2. Build MSIX Package
if ($MSIX -or $All) {
    Write-Host "`n[2/3] Creating MSIX Package..." -ForegroundColor Green

    # Check if Visual Studio MSBuild is available
    $vsWhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vsWhere) {
        $vsPath = & $vsWhere -latest -property installationPath
        $msbuildPath = Join-Path $vsPath "MSBuild\Current\Bin\MSBuild.exe"

        if (Test-Path $msbuildPath) {
            Write-Host "Found MSBuild at: $msbuildPath" -ForegroundColor Cyan

            # Ensure certificate exists
            if (!(Test-Path "OtakAgent.Package\OtakAgent_TemporaryKey.pfx")) {
                Write-Host "Creating signing certificate..." -ForegroundColor Yellow
                & ".\OtakAgent.Package\create-certificate.ps1"
            }

            # Build MSIX
            & $msbuildPath OtakAgent.Package\OtakAgent.Package.wapproj `
                /p:Configuration=$Configuration `
                /p:Platform=$Platform `
                /p:UapAppxPackageBuildMode=StoreUpload `
                /p:AppxBundle=Never `
                /p:GenerateAppInstallerFile=False `
                /p:AppxPackageSigningEnabled=True

            if ($LASTEXITCODE -eq 0) {
                $msixPath = Get-ChildItem -Path "OtakAgent.Package\AppPackages" -Filter "*.msix" -Recurse | Select-Object -First 1
                if ($msixPath) {
                    $destPath = Join-Path $OutputDir "OtakAgent.msix"
                    Copy-Item $msixPath.FullName $destPath -Force
                    Write-Host "✓ MSIX package created: $destPath" -ForegroundColor Green
                }
            } else {
                Write-Host "✗ MSIX package build failed!" -ForegroundColor Red
            }
        } else {
            Write-Host "✗ Visual Studio with Windows App SDK not found. MSIX build requires Visual Studio 2022." -ForegroundColor Yellow
            Write-Host "  Install Visual Studio 2022 with '.NET desktop development' and 'Windows application development' workloads." -ForegroundColor Yellow
        }
    } else {
        Write-Host "✗ Visual Studio not found. MSIX build requires Visual Studio 2022." -ForegroundColor Yellow
    }
}

# 3. Build MSI Installer
if ($MSI -or $All) {
    Write-Host "`n[3/3] Creating MSI Installer..." -ForegroundColor Green

    # Check if WiX v5 is available
    $wixPath = Get-Command wix -ErrorAction SilentlyContinue
    if ($wixPath) {
        Write-Host "Found WiX v5 toolset" -ForegroundColor Cyan

        # Check if WiX source file exists
        $wxsPath = ".\installer\OtakAgent.wxs"
        if (!(Test-Path $wxsPath)) {
            Write-Host "WiX source file not found at $wxsPath" -ForegroundColor Red
            Write-Host "Please create the installer\OtakAgent.wxs file first." -ForegroundColor Yellow
            return
        }

        # Build MSI with WiX v5
        $msiPath = Join-Path $OutputDir "OtakAgent.msi"

        Push-Location installer
        & wix build OtakAgent.wxs -arch x64 -ext WixToolset.UI.wixext -o "publish\OtakAgent.msi"
        Pop-Location

        if ($LASTEXITCODE -eq 0) {
            # Copy MSI from installer/publish to main publish directory
            $installerMsi = ".\installer\publish\OtakAgent.msi"
            if (Test-Path $installerMsi) {
                Copy-Item $installerMsi $msiPath -Force
                Write-Host "✓ MSI installer created: $msiPath" -ForegroundColor Green
            } else {
                Write-Host "✓ MSI installer created: $msiPath" -ForegroundColor Green
            }
        } else {
            Write-Host "✗ MSI build failed!" -ForegroundColor Red
        }
    } else {
        Write-Host "✗ WiX toolset not found. MSI build requires WiX v5." -ForegroundColor Yellow
        Write-Host "  Install with: dotnet tool install -g wix" -ForegroundColor Yellow
    }
}

Write-Host "`n=== Package Build Complete ===" -ForegroundColor Cyan
Write-Host "`nPackages created in: $OutputDir" -ForegroundColor Green

# List created packages
$packages = Get-ChildItem $OutputDir -File | Select-Object Name, @{Name="Size (MB)";Expression={[math]::Round($_.Length/1MB, 2)}}
if ($packages) {
    Write-Host "`nAvailable packages:" -ForegroundColor Cyan
    $packages | Format-Table -AutoSize
}