# Create portable installer package
Write-Host "Creating portable installer package..." -ForegroundColor Cyan

# Create package directory
$packageDir = "otak-agent-portable"
if (Test-Path $packageDir) {
    Remove-Item $packageDir -Recurse -Force
}
New-Item -ItemType Directory -Path $packageDir | Out-Null

# Copy application files
Write-Host "Copying application files..."
Copy-Item -Path "publish\*" -Destination $packageDir -Recurse

# Create installer batch file
$installerBat = @"
@echo off
title otak-agent Installer
echo =======================================
echo     otak-agent Installer
echo     AI-powered Desktop Assistant
echo =======================================
echo.

set INSTALL_PATH=%ProgramFiles%\otak-agent

echo Installing to: %INSTALL_PATH%
echo.

:: Check for admin rights
net session >nul 2>&1
if %errorLevel% NEQ 0 (
    echo This installer requires administrator privileges.
    echo Please run as administrator.
    pause
    exit /b 1
)

:: Create installation directory
if not exist "%INSTALL_PATH%" mkdir "%INSTALL_PATH%"

:: Copy files
echo Copying files...
xcopy /E /Y /I "*.exe" "%INSTALL_PATH%" >nul
xcopy /E /Y /I "*.dll" "%INSTALL_PATH%" >nul
xcopy /E /Y /I "*.pdb" "%INSTALL_PATH%" >nul
xcopy /E /Y /I "*.json" "%INSTALL_PATH%" >nul
xcopy /E /Y /I "Resources" "%INSTALL_PATH%\Resources" >nul

:: Create Start Menu shortcut
echo Creating Start Menu shortcut...
powershell -Command "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%APPDATA%\Microsoft\Windows\Start Menu\Programs\otak-agent.lnk'); $Shortcut.TargetPath = '%INSTALL_PATH%\OtakAgent.App.exe'; $Shortcut.Description = 'AI-powered Desktop Assistant'; $Shortcut.WorkingDirectory = '%INSTALL_PATH%'; $Shortcut.IconLocation = '%INSTALL_PATH%\OtakAgent.App.exe'; $Shortcut.Save()"

:: Create Desktop shortcut (optional)
choice /C YN /M "Create desktop shortcut"
if %errorlevel%==1 (
    echo Creating desktop shortcut...
    powershell -Command "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%USERPROFILE%\Desktop\otak-agent.lnk'); $Shortcut.TargetPath = '%INSTALL_PATH%\OtakAgent.App.exe'; $Shortcut.Description = 'AI-powered Desktop Assistant'; $Shortcut.WorkingDirectory = '%INSTALL_PATH%'; $Shortcut.IconLocation = '%INSTALL_PATH%\OtakAgent.App.exe'; $Shortcut.Save()"
)

echo.
echo =======================================
echo Installation completed successfully!
echo =======================================
echo.
echo otak-agent has been installed to:
echo %INSTALL_PATH%
echo.
echo You can find it in your Start Menu
echo.
pause
"@

$installerBat | Out-File -FilePath "$packageDir\Install.bat" -Encoding ASCII

# Create uninstaller
$uninstallerBat = @"
@echo off
title otak-agent Uninstaller
echo =======================================
echo     otak-agent Uninstaller
echo =======================================
echo.

set INSTALL_PATH=%ProgramFiles%\otak-agent

:: Check for admin rights
net session >nul 2>&1
if %errorLevel% NEQ 0 (
    echo This uninstaller requires administrator privileges.
    echo Please run as administrator.
    pause
    exit /b 1
)

echo This will uninstall otak-agent from your system.
choice /C YN /M "Continue with uninstallation"
if %errorlevel%==2 exit /b 0

:: Kill running processes
echo Stopping otak-agent if running...
taskkill /F /IM OtakAgent.App.exe >nul 2>&1

:: Remove installation directory
echo Removing application files...
if exist "%INSTALL_PATH%" rmdir /S /Q "%INSTALL_PATH%"

:: Remove shortcuts
echo Removing shortcuts...
del "%APPDATA%\Microsoft\Windows\Start Menu\Programs\otak-agent.lnk" >nul 2>&1
del "%USERPROFILE%\Desktop\otak-agent.lnk" >nul 2>&1

echo.
echo =======================================
echo Uninstallation completed!
echo =======================================
echo.
pause
"@

$uninstallerBat | Out-File -FilePath "$packageDir\Uninstall.bat" -Encoding ASCII

# Create README
$readme = @"
# otak-agent Portable Installer

## Installation
1. Run `Install.bat` as Administrator
2. Follow the prompts

## Uninstallation
1. Run `Uninstall.bat` as Administrator

## Manual Installation
1. Copy all files to your desired location
2. Run `OtakAgent.App.exe`

## Requirements
- Windows 11
- .NET 10 Runtime
- Administrator privileges (for installation)

## Support
https://github.com/tsuyoshi-otake/otak-agent/issues
"@

$readme | Out-File -FilePath "$packageDir\README.txt" -Encoding UTF8

# Create ZIP archive
Write-Host "Creating ZIP archive..."
$zipFile = "otak-agent-installer.zip"
if (Test-Path $zipFile) {
    Remove-Item $zipFile -Force
}

Compress-Archive -Path "$packageDir\*" -DestinationPath $zipFile -CompressionLevel Optimal

Write-Host ""
Write-Host "Portable installer created successfully!" -ForegroundColor Green
Write-Host "  ZIP Archive: $zipFile" -ForegroundColor Cyan
Write-Host "  Extract and run Install.bat as Administrator" -ForegroundColor Cyan
Write-Host ""
Write-Host "This ZIP can be distributed as a simple installer." -ForegroundColor Yellow