# otak-agent agent guide

## Purpose
- Modernize the legacy AgentTalk assistant into a single managed WinForms app while preserving its floating persona UX.
- Remove the native C++ bridge and .NET Framework 3.5 dependencies in favor of async/await, HttpClient, and dependency injection on .NET 9.
- Ship a deployment-friendly package: one executable, JSON settings beside it, and optional history in `%AppData%/AgentTalk`.

## Quick start
1. Install .NET SDK 9 (net9.0-windows) and ensure Windows 11 with desktop features enabled.
2. Restore and build: `dotnet build otak-agent.sln`.
3. Run the WinForms app: `dotnet run --project src/OtakAgent.App`.
4. Publish for distribution: `dotnet publish -c Release -r win-x64 --self-contained false`.
5. Copy legacy GIF/PNG/WAV assets into `src/OtakAgent.App/Resources` until embedded resources are finalized.

## Project map
- `src/OtakAgent.Core` - configuration, chat integration, personality prompts, clipboard hotkeys.
- `src/OtakAgent.App` - WinForms UI, dependency injection bootstrap, resource packaging.
- `docs/modernization-architecture.md` - detailed architecture overview.
- `docs/modernization-roadmap.md` - phased delivery plan and immediate tasks.
- `agent-talk-main/` - legacy reference implementation kept for comparison.

## Core responsibilities
- `Configuration` - `SettingsService` persists `agenttalk.settings.json`; `IniSettingsImporter` migrates `agenttalk.ini` and `SystemPrompt.ini` once.
- `Chat` - `ChatService` wraps HttpClient calls using typed request/response models and maintains conversation history in memory.
- `Personality` - `PersonalityPromptBuilder` recreates Clippy/Kairu prompts while allowing custom persona text.
- `Hotkeys` - `ClipboardHotkeyService` mirrors the double Ctrl+C gesture, configurable debounce, and clipboard actions.

## Runtime flow
1. `Program.cs` bootstraps WinForms, builds the service provider, and ensures settings are imported.
2. The main form receives `ChatService`, `SettingsService`, and `ClipboardHotkeyService` through DI.
3. Clipboard gestures trigger chat requests; responses update the borderless floating window and optional conversation log.
4. Settings edits write to JSON beside the executable; conversation history can be persisted to `%AppData%/AgentTalk/history.json`.

## Modernization highlights
- Async chat pipeline keeps the UI responsive without BackgroundWorker.
- HttpClient enables custom endpoints and modern error handling.
- JSON settings replace INI files; history persistence is opt-in for privacy.
- Input textbox uses localized placeholder guidance that clears when focused and returns if left empty.
- Asset handling stays compatible with legacy personas while preparing for embedded resources.

## Related references
- Architecture deep dive: `docs/modernization-architecture.md`.
- Delivery phases and TODOs: `docs/modernization-roadmap.md`.
- Legacy behavior reference: `agent-talk-main/README.md`.
