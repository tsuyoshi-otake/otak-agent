# otak-agentization Architecture

## Goals
- Target .NET 9 (net9.0-windows) to remove legacy .NET Framework dependencies.
- Replace the native C++ bridge and BackgroundWorker with async/await and HttpClient.
- Preserve AgentTalk user experience (floating assistant UI, hotkeys, dual language personalities).
- Simplify deployment: single WinForms executable + assets, no VC++ runtime.

## Solution layout
```
otak-agent/
  AgentTalk.sln
  src/
    OtakAgent.Core/
      OtakAgent.Core.csproj
      Configuration/
        AgentTalkSettings.cs
        SettingsService.cs
        IniSettingsImporter.cs
      Chat/
        ChatService.cs
        ChatMessage.cs
        ChatCompletionRequest.cs
        ChatCompletionResponse.cs
      Personality/
        PersonalityPromptBuilder.cs
      Hotkeys/
        ClipboardHotkeyService.cs
    OtakAgent.App/
      OtakAgent.App.csproj
      Program.cs
      Composition/ServiceConfigurator.cs
      Forms/
        MainForm.cs
        MainForm.Designer.cs
        SettingsForm.cs
        SettingsForm.Designer.cs
      Properties/
        Resources.resx
        Settings.settings (minimal)
      Resources/
        clippy.gif
        clippy_start.gif
        kairu.gif
        kairu_start.gif
        menucommand.wav
        kairu.wav
        clippy.wav
        windowTop.png
        windowCenter.png
        windowBottom.png
```

## Key design points
- `OtakAgent.Core` hosts configuration, chat API integration, personality prompt logic, and clipboard hotkey monitoring.
- WinForms layer is thin: injects `ChatService`, `SettingsService`, and `ClipboardHotkeyService` and binds UI events.
- Conversation history stays in memory and is persisted optionally to `%AppData%/AgentTalk/history.json` for future features.
- HTTP calls use `HttpClient` with dependency injection; supports custom `Host`/`Endpoint` in settings.
- Settings stored as JSON (`agenttalk.settings.json`) beside the executable, with one-time import of legacy `agenttalk.ini` / `SystemPrompt.ini`.
- Personality prompt builder recreates Clippy/Kairu defaults when `EnablePersonality` is true, but allows custom prompt editing.
- Clipboard hotkey service mimics double Ctrl+C behavior with P/Invoke + timer; configurable debounce and actions.
- Main window remains borderless, top-most, draggable, with notify icon context menu and tooltip updates.
- Async chat pipeline updates UI via `async/await` (`Task.Run` only for CPU-bound tasks) to keep UI responsive.

## Migration considerations
- Legacy `gpt.exe` and `OpenAIBridge` removed; all HTTP handled in managed code.
- New project uses SDK-style csproj; build/publish via `dotnet publish -c Release -r win-x64 --self-contained false`.
- Include instructions to copy legacy `images/` assets into the new `Resources/` folder until embedded resources are finalized.
- Automated tests planned in `OtakAgent.Core.Tests` targeting serialization and prompt builder behavior.


