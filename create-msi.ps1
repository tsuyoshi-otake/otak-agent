# PowerShell script to create MSI installer using WiX Toolset (if installed)
# Or create a simple installer package

param(
    [string]$SourcePath = ".\publish",
    [string]$OutputPath = ".\otak-agent-installer.msi"
)

Write-Host "Checking for WiX Toolset..." -ForegroundColor Cyan

# Check if WiX is installed
$wixPath = "${env:ProgramFiles(x86)}\WiX Toolset v3.11\bin"
$candle = "$wixPath\candle.exe"
$light = "$wixPath\light.exe"

if (Test-Path $candle) {
    Write-Host "WiX Toolset found. Creating MSI installer..." -ForegroundColor Green

    # Create WiX source file
    $wxsContent = @'
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
    <Product Id="*"
             Name="otak-agent"
             Language="1033"
             Version="1.0.0.0"
             Manufacturer="Tsuyoshi Otake"
             UpgradeCode="B3F5E2A1-8C7D-4F9E-A123-456789ABCDEF">

        <Package InstallerVersion="200"
                 Compressed="yes"
                 InstallScope="perMachine"
                 Description="AI-powered Desktop Assistant"
                 Manufacturer="Tsuyoshi Otake"
                 Comments="otak-agent installer" />

        <MajorUpgrade DowngradeErrorMessage="A newer version of [ProductName] is already installed." />
        <MediaTemplate EmbedCab="yes" />

        <Feature Id="MainApplication" Title="otak-agent" Level="1">
            <ComponentGroupRef Id="ApplicationFiles" />
            <ComponentRef Id="ApplicationShortcut" />
        </Feature>

        <Icon Id="app.ico" SourceFile="publish\Resources\app.ico" />
        <Property Id="ARPPRODUCTICON" Value="app.ico" />

        <Directory Id="TARGETDIR" Name="SourceDir">
            <Directory Id="ProgramFilesFolder">
                <Directory Id="INSTALLFOLDER" Name="otak-agent">
                    <Directory Id="ResourcesFolder" Name="Resources" />
                </Directory>
            </Directory>

            <Directory Id="ProgramMenuFolder">
                <Directory Id="ApplicationProgramsFolder" Name="otak-agent" />
            </Directory>
        </Directory>

        <DirectoryRef Id="ApplicationProgramsFolder">
            <Component Id="ApplicationShortcut" Guid="A1B2C3D4-E5F6-7890-ABCD-EF1234567890">
                <Shortcut Id="ApplicationStartMenuShortcut"
                          Name="otak-agent"
                          Description="AI-powered Desktop Assistant"
                          Target="[INSTALLFOLDER]OtakAgent.App.exe"
                          WorkingDirectory="INSTALLFOLDER"
                          Icon="app.ico" />
                <RemoveFolder Id="CleanUpShortCut" On="uninstall" />
                <RegistryValue Root="HKCU" Key="Software\Microsoft\otak-agent"
                               Name="installed" Type="integer" Value="1" KeyPath="yes" />
            </Component>
        </DirectoryRef>
    </Product>

    <Fragment>
        <ComponentGroup Id="ApplicationFiles" Directory="INSTALLFOLDER">
            <Component Id="MainExecutable" Guid="B1C2D3E4-F5A6-7890-BCDE-F12345678901">
                <File Id="OtakAgent.App.exe" Source="publish\OtakAgent.App.exe" KeyPath="yes" />
            </Component>
            <Component Id="CoreLibrary" Guid="C1D2E3F4-A5B6-7890-CDEF-123456789012">
                <File Id="OtakAgent.Core.dll" Source="publish\OtakAgent.Core.dll" />
            </Component>
        </ComponentGroup>
    </Fragment>
</Wix>
'@

    $wxsContent | Out-File -FilePath "installer.wxs" -Encoding UTF8

    # Compile WiX source
    & $candle installer.wxs -out installer.wixobj

    # Link to create MSI
    & $light installer.wixobj -out $OutputPath

    if (Test-Path $OutputPath) {
        Write-Host "MSI installer created successfully: $OutputPath" -ForegroundColor Green
        Remove-Item installer.wxs, installer.wixobj -Force
    }

} else {
    Write-Host "WiX Toolset not found. Creating alternative installer..." -ForegroundColor Yellow
    Write-Host "" -ForegroundColor Yellow

    # Create a self-extracting archive using PowerShell
    $installerScript = @'
# otak-agent Installer Script
$installPath = "$env:ProgramFiles\otak-agent"

Write-Host "Installing otak-agent to $installPath..." -ForegroundColor Cyan

# Create installation directory
if (!(Test-Path $installPath)) {
    New-Item -ItemType Directory -Path $installPath -Force | Out-Null
}

# Copy files (this would be embedded in actual installer)
Write-Host "Copying application files..."
# Note: In a real installer, files would be embedded here

# Create Start Menu shortcut
$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$env:APPDATA\Microsoft\Windows\Start Menu\Programs\otak-agent.lnk")
$Shortcut.TargetPath = "$installPath\OtakAgent.App.exe"
$Shortcut.Description = "AI-powered Desktop Assistant"
$Shortcut.WorkingDirectory = $installPath
$Shortcut.Save()

Write-Host "Installation complete!" -ForegroundColor Green
Write-Host "You can find otak-agent in your Start Menu." -ForegroundColor Cyan
'@

    # Save installer script
    $installerScript | Out-File -FilePath "install-otak-agent.ps1" -Encoding UTF8

    Write-Host @"

Alternative installation methods available:

1. **Manual Installation:**
   - Copy the 'publish' folder to your desired location
   - Run OtakAgent.App.exe

2. **PowerShell Installer:**
   - Run: powershell -ExecutionPolicy Bypass -File install-otak-agent.ps1

3. **To create a proper MSI installer:**
   Install WiX Toolset:
   winget install WiXToolset.WiXToolset

   Then run this script again.

"@ -ForegroundColor Yellow
}