# otak-agent

Modernizing the classic AgentTalk floating assistant into a single .NET 9 WinForms application without native dependencies.

## Highlights
- Targets `net9.0-windows` and replaces the legacy C++ bridge with modern async/await HttpClient pipelines.
- Keeps the nostalgic Clippy/Kairu personas, floating window, and double Ctrl+C capture that defined AgentTalk.
- Simplifies deployment to one executable, JSON settings beside it, and optional history under `%AppData%/AgentTalk`.

## Getting Started
### Prerequisites
- Windows 11 with desktop development tools enabled.
- .NET 9 SDK (net9.0-windows target).
- OpenAI-compatible API key (or equivalent endpoint) for chat completions.

### Build & Run
1. Restore and build the solution: `dotnet build otak-agent.sln`.
2. Run the WinForms front-end: `dotnet run --project src/OtakAgent.App`.
3. Publish for distribution: `dotnet publish -c Release -r win-x64 --self-contained false`.
4. Copy the legacy GIF/PNG/WAV assets into `src/OtakAgent.App/Resources` until embedded resources are finalized.

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
