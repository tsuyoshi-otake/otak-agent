# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Common Commands

### Build and Run
- **Build the solution**: `dotnet build otak-agent.sln`
- **Run the application**: `dotnet run --project src/OtakAgent.App`
- **Clean build**: `dotnet clean && dotnet build`

### Publishing
- **Publish for distribution**: `dotnet publish -c Release -r win-x64 --self-contained false`
- **Publish self-contained**: `dotnet publish -c Release -r win-x64 --self-contained true`

### Testing (when implemented)
- **Run all tests**: `dotnet test`
- **Run specific test project**: `dotnet test src/OtakAgent.Core.Tests`

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

## Current Tasks (TODO)

- [ ] 吹き出しの縁やキャラクター周辺に残るマゼンタのにじみを解消する。現状はカラーキー処理で除去しきれていない部分がある。
- [ ] キャラクターが起動直後に表示されないケースを再確認し、アニメーション開始前でも常に可視化できるようにする。

## Important Context

### Asset Management
GIF/PNG/WAV resources are currently in `src/OtakAgent.App/Resources/` and copied to output on build. The magenta color key (RGB 255,0,255) is used for transparency.

### Japanese Language Support
The application supports bilingual personalities (Japanese/English) and uses System.Globalization for locale detection. UI strings are currently hardcoded but prepared for localization.

### Legacy Compatibility
The `agent-talk-main/` folder contains the original .NET Framework 3.5 implementation for reference. Do not modify legacy code - it's preserved for behavior comparison only.

### Windows-Specific Features
- Uses P/Invoke for clipboard monitoring and window management
- Requires Windows 11 with desktop development tools
- Target framework is `net9.0-windows`