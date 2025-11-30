# AI Coding Instructions for Spoke

## Project Overview

**Spoke** is a decentralized, cross-platform smart home controller built on the [Ixian Platform](https://www.ixian.io). It provides secure, privacy-first control of Home Assistant devices through QuIXI bridge integration, using post-quantum encryption and blockchain-based identity. No central servers or direct internet exposure of Home Assistant.

## Architecture

### Core Components
- **Ixian-Core**: Provides post-quantum cryptography, wallet management, and secure networking primitives
- **QuIXI Bridge**: Python WebSocket client running on Home Assistant server for secure bidirectional communication
- **MAUI App**: Cross-platform UI using .NET MAUI with MVVM architecture
- **Entity System**: 6 entity types (Toggle, Sensor, Light, Gauge, Graph) with ObservableObject for reactive UI

### Data Flow
```
Spoke App (MAUI) ↔ Ixian Network ↔ QuIXI Bridge ↔ Home Assistant

```

 # AI coding instructions — Spoke (concise)

 This file gives focused, codebase-specific guidance so an AI coding agent can be productive quickly.

 Overview
 - Spoke is a .NET MAUI client that communicates with a local QuIXI bridge and the Ixian platform. All Home Assistant traffic must go through QuIXI (no direct HA API usage).

 Quick start (read these files first)
 - App bootstrap: `Spoke/MauiProgram.cs`, `Spoke/App.xaml.cs`
 - Node & config: `Spoke/Meta/Node.cs`, `Spoke/Meta/Config.cs` (see `Config.entitiesFilePath`)
 - Entity model & persistence: `Spoke/Data/EntityManager.cs` (`EntityManager.Instance`, call `SaveEntitiesAsync()`)
 - Network: `Spoke/Network/QuixiClient.cs` and `Spoke/Network/IQuixiClient.cs` (HTTP + WebSocket)
 - Real-time syncing: `Spoke/Services/SyncService.cs` (WebSocket preferred; 30s polling fallback)

 Key patterns & rules (project-specific)
 - Always route HA interactions through QuIXI; do not add direct HA API calls.
 - Use interface-first changes: update `IQuixiClient`/`IEntityManager` then implement concrete classes and tests.
 - Many managers are singletons (e.g., `EntityManager.Instance`, `Node.Instance`). Avoid creating parallel instances.
 - Do not start Node or background services in the App constructor; initialize after window creation (e.g., `CreateWindow` / `OnLaunched`) to avoid platform activation races on Windows/MAUI.
 - Treat secrets carefully: use `Meta/Config.cs` (SecureStorage) and avoid logging sensitive values.

 Concrete examples
 - Send a command: `await Node.Instance.quixiClient.SendCommandAsync(entityId, command, parameters)`
 - Save entities after edits: `await EntityManager.Instance.SaveEntitiesAsync()`
 - Subscribe to entity updates: `quixiClient.EntityStateChanged += (s,e) => EntityManager.Instance.UpdateEntityState(...);`

 Build & developer commands
 ```powershell
 # install MAUI workload (one-time)
 dotnet workload install maui

 cd Spoke
 dotnet restore Spoke.sln
 dotnet build Spoke.sln
 ``` 
 - Targeted builds: use `dotnet build -f <TFM>` (e.g., `net9.0-android`, `net9.0-windows10.0.19041.0`). Check `global.json` for SDK pinning.

 Testing & contribution notes
 - Unit tests should mock `IQuixiClient` (see `Spoke/Network/IQuixiClient.cs`).
 - When adding behavior that impacts entities, include a focused unit test that verifies persistence (`EntityManager`) and uses an in-memory/mock QuIXI client.

 Where to run manual QA
 - Onboarding: `Spoke/Pages/Onboarding/OnboardingPage.xaml.cs`
 - Local quick-run helper: `Spoke/TestRunner.cs` (used for manual inspection)

 Integration points & dependencies
 - QuIXI bridge: Python WebSocket client — Spoke expects `/entities` and `/command` endpoints and WebSocket events.
 - Ixian core: cryptography and identity live under `Ixian-Core/` (do not modify without understanding crypto implications).

 Common pitfalls
 - Never bypass QuIXI for Home Assistant access.
 - UI updates must run on the main thread (use dispatcher where necessary).
 - Remember to call `SaveEntitiesAsync()` after state changes to persist JSON state.

 If something is unclear or you need more examples (e.g., typical PR checklist, testing commands, or preferred logging formats), tell me which area to expand.

 Files to mention in PR descriptions
 - Typical important files: `Meta/Node.cs`, `Network/QuixiClient.cs`, `Data/EntityManager.cs`, `Services/SyncService.cs`.
