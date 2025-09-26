# CLAUDE.md

This file provides guidelines for Claude Code (claude.ai/code) when working with this repository's code.

## Important Notes
As of September 25, 2025, this application supports GPT-5 and GPT-5 Codex. GPT-5 Codex is the default model. The application automatically uses the latest /v1/responses endpoint for GPT-5 series and GPT-4.1 series models.

## Framework Requirements
- **.NET 10 RC1** (10.0.100-rc.1 or later)
- Windows 11 with desktop development tools
- Target Frameworks:
  - `net10.0` - For OtakAgent.Core
  - `net10.0-windows` - For OtakAgent.App

## Common Commands

### Build and Run
- **Build Solution**: `dotnet build otak-agent.sln`
- **Run Application**: `dotnet run --project src/OtakAgent.App`
- **Clean Build**: `dotnet clean && dotnet build`
- **Restore Packages**: `dotnet restore`

### Package Creation
Using the integrated build script `build-packages.ps1`:
- **Build All Packages**: `powershell -ExecutionPolicy Bypass -File build-packages.ps1 -All`
- **Portable Only**: `powershell -ExecutionPolicy Bypass -File build-packages.ps1 -Portable`
- **MSI Only**: `powershell -ExecutionPolicy Bypass -File build-packages.ps1 -MSI`
- **MSIX Only**: `powershell -ExecutionPolicy Bypass -File build-packages.ps1 -MSIX`

Prerequisites:
- Portable: .NET 10 SDK
- MSI: WiX v5 (`dotnet tool install -g wix`)
- MSIX: Visual Studio 2022 + Windows Application Packaging Project extension

### Version Management
**IMPORTANT**: Always update the version number in `installer/OtakAgent.wxs` when releasing a new version.

#### Current Version
- **v1.2.0.0** (September 26, 2025)

#### Version Update Steps
1. Open `installer/OtakAgent.wxs`
2. Update `Version="X.Y.Z.0"` to the new version
3. Rebuild MSI package: `powershell -ExecutionPolicy Bypass -File build-packages.ps1 -MSI`

#### MSI Upgrade Configuration
- **UpgradeCode**: `B7E5D3F2-8A4C-4E9B-9D1A-F5C8E3A2B1D0` (fixed value - DO NOT CHANGE)
- **MajorUpgrade**: Automatically uninstalls older versions
- **AllowSameVersionUpgrades**: Allows overwriting same version

#### Version Number Convention
- Use **Major.Minor.Patch.0** format
- Example: 1.2.0.0
- MSI always uses 0 for the fourth number (build number)

### Testing (When Implemented)
- **Run All Tests**: `dotnet test`
- **Run Specific Test Project**: `dotnet test src/OtakAgent.Core.Tests`

## Recent UI Improvements

### Bubble Window Rendering
- Top and bottom images display fully without cropping
- Bottom extends 5px lower for proper appearance
- Proper transparency using magenta color key (RGB 255,0,255)

### Expandable Text Area
- Toggle button (▼/▲) in bubble's top-right corner (position: 200,8)
- When enabled, text area expands vertically 5x
- Form expands upward while maintaining bottom position
- Character position adjusts based on expansion state

### UI Element Positioning
- Prompt label: (8, 8)
- Text input: (8, 32) - normal mode, 295px height when expanded
- Buttons: Dynamically positioned below text area
- Character: Position adjusts based on expansion state

### Interactive Features
- Double-click character to toggle bubble visibility
- Right-click character for context menu
- Text area expand/collapse button
- Animation transitions handled via BeginInvoke to prevent freezing

### Conversation Continuation Mode
- "Input" button or Ctrl+Enter during response display continues conversation
- New input possible while maintaining conversation history
- Placeholder text not displayed
- "Reset" button always visible

## Project Structure

### Directory Layout
```
otak-agent/
├── build-packages.ps1      # Integrated package build script
├── otak-agent.sln          # Visual Studio solution
├── README.md               # Project documentation (Japanese)
├── CLAUDE.md               # This file (Claude Code guidelines)
├── LICENSE                 # License file
├── .gitignore              # Git ignore settings
│
├── src/                    # Source code
│   ├── OtakAgent.Core/     # Business logic layer
│   │   ├── Configuration/  # Configuration management
│   │   └── Services/       # Chat services
│   └── OtakAgent.App/      # Presentation layer
│       ├── Forms/          # WinForms UI
│       └── Resources/      # Assets (GIF, PNG, WAV files)
│
├── installer/              # MSI installer definitions
│   ├── OtakAgent.wxs       # WiX v5 definition file
│   └── license.rtf        # Installer license
│
├── OtakAgent.Package/      # MSIX/Store packaging
│   ├── Package.appxmanifest    # MSIX manifest
│   ├── Images/                 # Store assets
│   ├── create-certificate.ps1  # Certificate generation script
│   └── generate-assets.ps1     # Asset generation script
│
├── docs/                   # GitHub Pages documentation
│   ├── index.md            # Homepage (Japanese)
│   ├── privacy.md          # Privacy policy (Japanese)
│   └── _config.yml         # Jekyll configuration
│
└── publish/                # Build output (gitignored)
    ├── otak-agent-portable.zip  # Portable version
    ├── otak-agent.msi           # MSI installer
    └── portable/                # Portable version working directory
```

## Architecture Overview

### Solution Structure
This is parody software recreating Microsoft Office assistants (Clippy, Kairu, etc.). It's a .NET 10 WinForms application bringing back nostalgic desktop mascots with modern technology. The solution follows clean architecture with separation of concerns:

- **OtakAgent.Core** (Class Library): Business logic, services, models
  - Configuration management (settings, INI migration)
  - Chat service for OpenAI-compatible API integration
  - Personality prompt builder for Clippy/Kairu personas
  - Clipboard hotkey monitoring service

- **OtakAgent.App** (WinForms App): Presentation layer
  - Dependency injection setup via Microsoft.Extensions.DependencyInjection
  - Main floating window with borderless design
  - Settings form for configuration management
  - Resource assets (GIF, WAV, PNG)

### Key Service Dependencies
The application uses the following core services via constructor dependency injection:
- `ChatService`: Handles API communication with OpenAI-compatible endpoints
- `SettingsService`: Manages JSON settings persistence
- `PersonalityPromptBuilder`: Builds persona-specific system prompts
- `UpdateChecker`: Checks for updates from GitHub releases

### Settings Flow
1. Settings saved as `agenttalk.settings.json` next to executable
2. Default settings created on first launch
3. Optionally persist history to `%AppData%/AgentTalk/history.json`

### UI Behavior Patterns
- Borderless, always-on-top floating window
- Move via drag with mouse down/move events
- Double-click to toggle visibility
- System tray icon with context menu
- Ctrl+Enter to send, Ctrl+Backspace to reset
- Context preservation in conversation continuation mode

## Important Context

### Project Configuration
All packaging processes are managed through a single integrated build script `build-packages.ps1`. Unnecessary scripts have been removed.

### Important Files
- **build-packages.ps1**: Integrated packaging script (supports Portable/MSI/MSIX)
- **installer/OtakAgent.wxs**: WiX v5 MSI definition file
- **OtakAgent.Package/**: MSIX-related files
  - `Package.appxmanifest`: MSIX manifest
  - `create-certificate.ps1`: Signing certificate generation
  - `generate-assets.ps1`: Store asset generation

### Asset Management
GIF/PNG/WAV resources are currently in `src/OtakAgent.App/Resources/` and are copied to output during build. Uses magenta color key (RGB 255,0,255) for transparency.

### Microsoft Store Assets
Store assets are placed in `OtakAgent.Package/Images/` for MSIX packaging:
- **Square150x150Logo.png** - Medium tile (150x150)
- **Square44x44Logo.png** - Small app icon (44x44)
- **Square44x44Logo.targetsize-24_altform-unplated.png** - Taskbar icon (24x24)
- **Wide310x150Logo.png** - Wide tile (310x150)
- **SmallTile.png** - Small tile (71x71)
- **LargeTile.png** - Large tile (310x310)
- **StoreLogo.png** - Store listing logo (50x50)
- **SplashScreen.png** - App splash screen (620x300)

Use `generate-assets.ps1` script to regenerate these assets from base icon.

### MSIX Packaging and Microsoft Store Distribution
`OtakAgent.Package/` directory contains Windows Application Packaging Project for MSIX package creation:
- **OtakAgent.Package.wapproj** - Visual Studio packaging project file
- **Package.appxmanifest** - Application manifest defining app ID, capabilities, and visual elements
- **create-certificate.ps1** - Self-signed certificate generation script for package signing
- **generate-assets.ps1** - Script to generate Store icons from base image

#### Building MSIX Package

##### Method 1: Using Windows SDK makeappx tool (Recommended)
```powershell
# 1. Build portable version
dotnet publish src/OtakAgent.App -c Release -r win-x64 --self-contained false -o ./publish/portable

# 2. Deploy AppxManifest
Copy-Item OtakAgent.Package\Package.appxmanifest publish\portable\AppxManifest.xml

# 3. Create MSIX package (Windows SDK required)
& "C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x64\makeappx.exe" pack /d publish\portable /p publish\otak-agent.msix /nv
```

##### Method 2: Using build-packages.ps1 script
```powershell
powershell -ExecutionPolicy Bypass -File build-packages.ps1 -MSIX
```

##### Method 3: Using Visual Studio 2022 (after .NET 10 SDK support)
```powershell
msbuild OtakAgent.Package\OtakAgent.Package.wapproj /p:Configuration=Release /p:Platform=x64
```

#### Microsoft Store Submission
- Upload generated MSIX package to Partner Center
- Package supports x64 architecture
- Requires Windows 11 version 22621.0 or higher
- Signing is mandatory (Store signing or test self-signed certificate)

### Japanese Language Support
The application supports bilingual personas (Japanese/English) and uses System.Globalization for locale detection. UI strings are currently hardcoded but ready for localization.

### Windows-Specific Features
- Uses P/Invoke for clipboard monitoring and window management
- Requires Windows 11 with desktop development tools
- Target framework is `net10.0-windows`