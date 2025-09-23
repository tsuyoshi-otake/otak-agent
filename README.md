# otak-agent

Modernizing the classic AgentTalk floating assistant into a single .NET 10 WinForms application without native dependencies.

## Highlights
- Targets `net10.0-windows` leveraging the latest .NET 10 RC features and performance improvements
- Keeps the nostalgic Clippy/Kairu personas, floating window, and double Ctrl+C capture that defined AgentTalk
- Simplifies deployment to one executable, JSON settings beside it, and optional history under `%AppData%/AgentTalk`
- Features expandable text area with 5x vertical expansion for longer inputs
- Interactive UI with right-click context menu and double-click bubble toggle

## Getting Started
### Prerequisites
- Windows 11 with desktop development tools enabled
- .NET 10 SDK RC1 (10.0.100-rc.1 or later) - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- OpenAI-compatible API key (or equivalent endpoint) for chat completions

### Build & Run
1. Install .NET 10 SDK RC1 if not already installed
2. Restore and build the solution: `dotnet build otak-agent.sln`
3. Run the WinForms front-end: `dotnet run --project src/OtakAgent.App`
4. Publish for distribution: `dotnet publish -c Release -r win-x64 --self-contained false`
5. Resources (GIF/PNG/WAV) are automatically copied from `src/OtakAgent.App/Resources` during build

### Creating MSIX Package for Microsoft Store

#### Prerequisites
- Windows SDK installed (includes makeappx.exe)
  - Install via: `winget install --id Microsoft.WindowsSDK.10.0.18362`

#### Step-by-Step Instructions

1. **Build Release Version**
   ```bash
   dotnet publish src/OtakAgent.App -c Release -r win-x64 --self-contained false -o ./publish
   ```

2. **Generate Store Assets** (if not already created)
   ```powershell
   cd OtakAgent.Package
   powershell -ExecutionPolicy Bypass -File generate-assets.ps1
   cd ..
   ```

3. **Create MSIX Structure**
   ```powershell
   powershell -ExecutionPolicy Bypass -File create-simple-msix.ps1
   ```

4. **Build MSIX Package**
   ```powershell
   powershell -ExecutionPolicy Bypass -File build-msix.ps1
   ```

   Or manually:
   ```powershell
   & "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\makeappx.exe" pack /d OtakAgent_MSIX /p OtakAgent.msix /nv /o
   ```

5. **Test Installation** (requires Developer Mode)
   ```powershell
   Add-AppxPackage -Path OtakAgent.msix -AllowUnsigned
   ```

#### Microsoft Store Submission
1. Upload `OtakAgent.msix` to [Partner Center](https://partner.microsoft.com/dashboard)
2. Microsoft will automatically sign the package
3. Fill in Store listing information
4. Submit for certification

#### Package Contents
- **OtakAgent.msix** (~49MB) contains:
  - Main application executable
  - AppxManifest.xml (package manifest)
  - Store icons (Square, Wide, Splash Screen)
  - Application resources (GIFs, WAVs, PNGs)

## Configuration
- Settings live beside the executable in `agenttalk.settings.json` and are managed by `SettingsService` in `OtakAgent.Core`.
- On first launch, `IniSettingsImporter` migrates legacy `agenttalk.ini` and `SystemPrompt.ini` when detected.
- Conversation history remains in memory by default and can optionally persist to `%AppData%/AgentTalk/history.json`.

## Personas & Hotkeys
- `PersonalityPromptBuilder` rebuilds the Clippy and Kairu prompts while allowing custom persona text when personalities are enabled.
- `ClipboardHotkeyService` listens for the double Ctrl+C gesture, applies configurable debounce, and forwards clipboard content into chat.
- UI shortcuts mirror the original AgentTalk experience (submit with Ctrl+Enter, reset with Ctrl+Backspace, toggle visibility with double-click).

## Project Structure
```
otak-agent/
  AGENT.md
  docs/
    modernization-architecture.md
    modernization-roadmap.md
  src/
    OtakAgent.Core/
    OtakAgent.App/
  agent-talk-main/
```

- `OtakAgent.Core` hosts configuration, chat, personality, and hotkey services.
- `OtakAgent.App` delivers the WinForms UI, dependency injection bootstrap, and resources.
- `agent-talk-main/` retains the legacy codebase for behavior parity checks.

## Development Notes
- Unit tests for configuration import and prompt builder behavior are planned under `OtakAgent.Core.Tests`.
- Trace logging and optional file logs will be added during the polish phase to aid diagnostics.
- Publish builds use `dotnet publish` and produce a self-contained-ready folder for distribution.

## Documentation
- Architecture overview: `docs/modernization-architecture.md`.
- Delivery plan and TODOs: `docs/modernization-roadmap.md`.
- Agent operations quick guide: `AGENT.md`.

## Legacy Reference
The original AgentTalk implementation (targeting .NET Framework 3.5 with a C++ bridge) remains in `agent-talk-main/` and can be used to compare UI or persona behaviors during modernization.
