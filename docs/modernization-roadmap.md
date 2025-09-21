# otak-agentization Roadmap

## Phase 1 - Project bootstrap
1. Scaffold `otak-agent/AgentTalk.sln` and WinForms/ class library projects targeting net9.0-windows.
2. Copy/Reference legacy assets (gif/png/wav) into `OtakAgent.App/Resources` and wire them up via designer resources.
3. Implement configuration service with JSON persistence and legacy INI migrator.

## Phase 2 - Core services
1. Implement `ChatService` using `HttpClient` and strongly typed request/response models.
2. Add personality prompt builder with English/Kairu templates and custom text support.
3. Implement clipboard hotkey service mirroring double Ctrl+C behavior and exposing events.
4. Provide unit tests for configuration import and prompt builder.

## Phase 3 - UI
1. Recreate `MainForm` layout (borderless, draggable, top-most, notify icon, tooltips) in WinForms.
2. Bind UI actions to `ChatService` using async/await, conversation history, and clipboard integration.
3. Implement `SettingsForm` for editing configuration including quick-select for models and toggles.
4. Support expanded textbox mode, auto-copy to clipboard, and personality toggling.

## Phase 4 - Polish & Packaging
1. Implement error handling, status messaging, and logging (Trace / optional file log).
2. Persist conversation history optionally to `%AppData%/AgentTalk/history.json`.
3. Provide publishing profile (`dotnet publish`) and update README with modern usage instructions.
4. Rip out legacy C++ projects and update repository structure.

## Immediate next actions
- [ ] Scaffold solution & projects (Phase 1.1).
- [ ] Port assets into new project (Phase 1.2).
- [ ] Implement configuration service + INI importer (Phase 1.3).


