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

    # Check if WiX is available
    $wixPath = Get-Command candle.exe -ErrorAction SilentlyContinue
    if ($wixPath) {
        Write-Host "Found WiX toolset" -ForegroundColor Cyan

        # Create WiX source file if it doesn't exist
        $wxsPath = ".\installer\OtakAgent.wxs"
        if (!(Test-Path $wxsPath)) {
            Write-Host "Creating WiX source file..." -ForegroundColor Yellow
            New-Item -ItemType Directory -Path ".\installer" -Force | Out-Null

            $wxsContent = @'
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
    <Product Id="*"
             Name="OtakAgent"
             Language="1033"
             Version="1.0.0.0"
             Manufacturer="OtakAgent Team"
             UpgradeCode="12345678-1234-1234-1234-123456789ABC">

        <Package InstallerVersion="200"
                 Compressed="yes"
                 InstallScope="perUser" />

        <MajorUpgrade DowngradeErrorMessage="A newer version of [ProductName] is already installed." />
        <MediaTemplate EmbedCab="yes" />

        <Feature Id="ProductFeature" Title="OtakAgent" Level="1">
            <ComponentGroupRef Id="ProductComponents" />
            <ComponentRef Id="ApplicationShortcut" />
        </Feature>

        <Icon Id="OtakAgent.ico" SourceFile="..\src\OtakAgent.App\Resources\app.ico" />
        <Property Id="ARPPRODUCTICON" Value="OtakAgent.ico" />
    </Product>

    <Fragment>
        <Directory Id="TARGETDIR" Name="SourceDir">
            <Directory Id="LocalAppDataFolder">
                <Directory Id="INSTALLFOLDER" Name="OtakAgent" />
            </Directory>
            <Directory Id="ProgramMenuFolder">
                <Directory Id="ApplicationProgramsFolder" Name="OtakAgent"/>
            </Directory>
        </Directory>
    </Fragment>

    <Fragment>
        <ComponentGroup Id="ProductComponents" Directory="INSTALLFOLDER">
            <Component Id="OtakAgent.App.exe">
                <File Source="..\publish\portable\OtakAgent.App.exe" />
            </Component>
        </ComponentGroup>

        <Component Id="ApplicationShortcut" Directory="ApplicationProgramsFolder">
            <Shortcut Id="ApplicationStartMenuShortcut"
                      Name="OtakAgent"
                      Description="AI Agent Assistant"
                      Target="[#OtakAgent.App.exe]"
                      WorkingDirectory="INSTALLFOLDER"/>
            <RemoveFolder Id="ApplicationProgramsFolder" On="uninstall"/>
            <RegistryValue Root="HKCU" Key="Software\OtakAgent" Name="installed" Type="integer" Value="1" KeyPath="yes"/>
        </Component>
    </Fragment>
</Wix>
'@
            $wxsContent | Out-File -FilePath $wxsPath -Encoding UTF8
        }

        # Compile and link MSI
        $objPath = ".\installer\OtakAgent.wixobj"
        $msiPath = Join-Path $OutputDir "OtakAgent.msi"

        & candle.exe -out $objPath $wxsPath
        if ($LASTEXITCODE -eq 0) {
            & light.exe -out $msiPath $objPath
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ MSI installer created: $msiPath" -ForegroundColor Green
            }
        }
    } else {
        Write-Host "✗ WiX toolset not found. MSI build requires WiX v3 or v4." -ForegroundColor Yellow
        Write-Host "  Download from: https://wixtoolset.org/releases/" -ForegroundColor Yellow

        # Create a simple self-extracting archive as alternative
        Write-Host "`nCreating self-extracting archive as alternative..." -ForegroundColor Cyan
        $sfxPath = Join-Path $OutputDir "OtakAgent-Setup.exe"

        # This would require 7-Zip or similar tool
        if (Get-Command 7z -ErrorAction SilentlyContinue) {
            $tempDir = Join-Path $env:TEMP "OtakAgent-SFX"
            New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
            Copy-Item "$OutputDir\portable\*" $tempDir -Recurse -Force

            # Create config for SFX
            $sfxConfig = @"
;!@Install@!UTF-8!
Title="OtakAgent Installer"
BeginPrompt="Install OtakAgent?"
RunProgram="OtakAgent.App.exe"
;!@InstallEnd@!
"@
            $sfxConfig | Out-File -FilePath "$tempDir\config.txt" -Encoding UTF8

            & 7z a -sfx7z.sfx "$sfxPath" "$tempDir\*"
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Self-extracting installer created: $sfxPath" -ForegroundColor Green
            }
            Remove-Item $tempDir -Recurse -Force
        } else {
            Write-Host "  7-Zip not found. Cannot create self-extracting archive." -ForegroundColor Yellow
        }
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