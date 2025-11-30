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

### Key Classes
- `EntityManager`: Singleton managing entity CRUD with JSON persistence
- `QuixiClient`: HTTP/WebSocket client for QuIXI API communication  
- `SyncService`: Handles real-time sync via WebSocket with 30s polling fallback
- `Config`: Secure storage for settings using Preferences/SecureStorage
- `Node`: Ixian node lifecycle management

````instructions
# Spoke — quick AI coding guide (short & actionable)

Overview: Spoke is a .NET MAUI cross-platform app (Spoke/Spoke/) that communicates with a local QuIXI bridge and the Ixian platform (Ixian-Core/) — QuIXI is the only permitted path to Home Assistant.

Where to start (quick reads):
- App/bootstrap: `Spoke/MauiProgram.cs`, `Spoke/App.xaml.cs`
- Node & config: `Spoke/Meta/Node.cs`, `Spoke/Meta/Config.cs` (watch `Config.entitiesFilePath`)
- Entity model + persistence: `Spoke/Data/EntityManager.cs` (Singleton; call SaveEntitiesAsync())
- Network layer: `Spoke/Network/QuixiClient.cs` (implements HTTP + WebSocket methods declared in `IQuixiClient`)
- Real-time sync: `Spoke/Services/SyncService.cs` (WebSocket preferred, 30s polling fallback)

Important patterns & examples (concrete):
- Use interface-first testing: mock `IQuixiClient` when writing unit tests (see `IQuixiClient.cs`).
- Entities are global-singletons: use `EntityManager.Instance` and call `SaveEntitiesAsync()` after edits.
- Connection example: `await Node.Instance.quixiClient.TestConnectionAsync()` (used in Onboarding/Settings pages).
- Command example: `await Node.Instance.quixiClient.SendCommandAsync(entityId, command, parameters)`.

Build & dev commands (fast path):
1. Install workloads (PowerShell): `dotnet workload install maui` (setup.ps1 contains helpers).
2. Restore and build: `cd Spoke; dotnet restore Spoke.sln; dotnet build Spoke.sln`.
3. Targeted builds: `dotnet build -f net9.0-android` or `-f net9.0-windows10.0.19041.0` (check `global.json` for SDK version; repo currently references SDK 10.0.100).

Project-specific rules for automated agents:
- Never modify Home Assistant integration to call HA APIs directly — all HA traffic must go through QuIXI.
 - Never modify Home Assistant integration to call HA APIs directly — all HA traffic must go through QuIXI.
 - Important Windows/MAUI rule: do not auto-initialize Node or start background services that can interact with UI from the App constructor. Start them after the first window is created (e.g., in CreateWindow or OnLaunched) to avoid platform activation/early-window creation races.
- Prefer making changes to an interface (IQuixiClient, IEntityManager) and update/implement the concrete class.
- Avoid logging secrets; use `SecureStorage` via `Meta/Config.cs`.

Low-effort fixes & TODOs (where agents can contribute):
- QuixiClient: several TODOs about fully implementing IQuixiClient and WebSocket subscription handling (`Network/QuixiClient.cs`).
- EntityManager and other managers include TODOs for interface conformance and persistent storage improvements.

Where to test locally:
- Onboarding flow & manual QA: `Spoke/Pages/Onboarding/OnboardingPage.xaml.cs`.
- Try the sample `Spoke/TestRunner.cs` for quick local behavior inspection.

When in doubt, search for TODO comments and existing tests; prefer interface-based changes and include a focused unit test that mocks `IQuixiClient`.

Files to mention in PR description: key file change list (e.g., `Meta/Node.cs`, `Network/QuixiClient.cs`, `Data/EntityManager.cs`, `Services/SyncService.cs`).
````