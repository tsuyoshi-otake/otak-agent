# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Important Note
As of September 24, 2025, GPT-5, GPT-5 Turbo, and GPT-5 Codex are supported by this application. The app automatically uses the modern /v1/responses endpoint for GPT-5 series, GPT-4.1, GPT-4o, and o1 series models.

## Framework Requirements
- **.NET 10 RC1** (10.0.100-rc.1 or later)
- Windows 11 with desktop development tools
- Target frameworks:
  - `net10.0` for OtakAgent.Core
  - `net10.0-windows` for OtakAgent.App

## Common Commands

### Build and Run
- **Build the solution**: `dotnet build otak-agent.sln`
- **Run the application**: `dotnet run --project src/OtakAgent.App`
- **Clean build**: `dotnet clean && dotnet build`
- **Restore packages**: `dotnet restore`

### Publishing
- **Publish for distribution**: `dotnet publish -c Release -r win-x64 --self-contained false`
- **Publish self-contained**: `dotnet publish -c Release -r win-x64 --self-contained true`

### Testing (when implemented)
- **Run all tests**: `dotnet test`
- **Run specific test project**: `dotnet test src/OtakAgent.Core.Tests`

## Recent UI Improvements

### Bubble Window Rendering
- Top and bottom images now display fully without cropping
- Bottom extends 5px below for proper appearance
- Proper transparency with magenta color key (RGB 255,0,255)

### Expandable Text Area
- Toggle button (▼/▲) in top-right corner of bubble (position: 200,8)
- Expands text area 5x vertically when enabled
- Form grows upward maintaining bottom position
- Character position adjusts when expanded

### UI Element Positioning
- Prompt label: (8, 8)
- Text input: (8, 32) - normal mode, expands to 295px height
- Buttons: Positioned dynamically below text area
- Character adjusts position based on expansion state

### Interactive Features
- Double-click character to toggle bubble visibility
- Right-click character for context menu
- Expand/collapse button for text area
- Animation transitions handled with BeginInvoke to prevent freezing

## Architecture Overview

### Solution Structure
This is a .NET 9 WinForms application modernizing the legacy AgentTalk assistant. The solution follows a clean architecture with separation of concerns:

- **OtakAgent.Core** (Class Library): Business logic, services, and models
  - Configuration management (settings, INI migration)
  - Chat service with OpenAI-compatible API integration
  - Personality prompt builder for Clippy/Kairu personas
  - Clipboard hotkey monitoring service
  
- **OtakAgent.App** (WinForms App): Presentation layer
  - Dependency injection setup via Microsoft.Extensions.DependencyInjection
  - Main floating window with borderless design
  - Settings form for configuration management
  - Resource assets (GIFs, WAVs, PNGs)

### Key Service Dependencies
The application uses constructor dependency injection with these core services:
- `ChatService`: Handles API communication with OpenAI-compatible endpoints
- `SettingsService`: Manages JSON settings persistence
- `ClipboardHotkeyService`: Monitors double Ctrl+C gesture
- `PersonalityPromptBuilder`: Constructs persona-specific system prompts

### Configuration Flow
1. Settings stored in `agenttalk.settings.json` beside the executable
2. Legacy `agenttalk.ini` and `SystemPrompt.ini` auto-imported on first launch
3. Optional history persistence to `%AppData%/AgentTalk/history.json`

### UI Behavior Patterns
- Borderless, always-on-top floating window
- Drag-to-move via mouse down/move events
- Double-click to toggle visibility
- System tray icon with context menu
- Ctrl+Enter to submit, Ctrl+Backspace to reset

## Important Context

### Asset Management
GIF/PNG/WAV resources are currently in `src/OtakAgent.App/Resources/` and copied to output on build. The magenta color key (RGB 255,0,255) is used for transparency.

### Microsoft Store Assets
Store assets are located in `OtakAgent.Package/Images/` for MSIX packaging:
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
The `OtakAgent.Package/` directory contains the Windows Application Packaging Project for creating MSIX packages:
- **OtakAgent.Package.wapproj** - Packaging project file for Visual Studio
- **Package.appxmanifest** - Application manifest defining app identity, capabilities, and visual elements
- **create-certificate.ps1** - Script to generate self-signed certificate for package signing
- **generate-assets.ps1** - Script to generate Store icons from base image

To build MSIX package:
1. Run `create-certificate.ps1` to create signing certificate
2. Open solution in Visual Studio 2022 and build OtakAgent.Package project
3. Or use MSBuild: `msbuild OtakAgent.Package\OtakAgent.Package.wapproj /p:Configuration=Release`

For Microsoft Store submission:
- Upload the generated MSIX package to Partner Center
- Package supports x64, x86, and ARM64 architectures
- Requires Windows 11 version 22621.0 or higher
- See `README_MSIX.md` for detailed Store submission guide

### Japanese Language Support
The application supports bilingual personalities (Japanese/English) and uses System.Globalization for locale detection. UI strings are currently hardcoded but prepared for localization.

### Legacy Compatibility
The `agent-talk-main/` folder contains the original .NET Framework 3.5 implementation for reference. Do not modify legacy code - it's preserved for behavior comparison only.

### Windows-Specific Features
- Uses P/Invoke for clipboard monitoring and window management
- Requires Windows 11 with desktop development tools
- Target framework is `net9.0-windows`