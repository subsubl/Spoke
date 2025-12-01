# Quickstart: Developer Setup (MAUI / .NET 10)

This quickstart documents the minimal steps to set up a developer machine for building the MAUI apps in this repository targeting .NET 10 (`10.0.100`).

Prerequisites
- Install .NET SDK `10.0.100` or higher (pinned via `global.json`).
- Install required platform toolchains for targets you intend to build (Android SDK, Xcode on macOS, Windows SDK/Visual Studio workloads on Windows).

Developer steps (PowerShell)

```powershell
# Verify .NET SDK
dotnet --version

# Install MAUI workloads (local dev)
dotnet workload install maui maui-android maui-ios maui-maccatalyst maui-windows

# Restore workloads (CI and local)
dotnet workload restore

# Restore solution packages
dotnet restore .\Spoke\SPOKE.sln

# Build solution
dotnet build .\Spoke\SPOKE.sln -c Release

# Run tests (if any)
dotnet test .\Spoke\SPOKE.sln -c Release
```

Notes
- Android: ensure Android SDK platform (API level 35) and command-line tools are installed for Android builds.
- macOS: Xcode 15+ is recommended for iOS/MacCatalyst targets.
- CI: the repository workflow `/.github/workflows/ci-dotnet-maui.yml` runs workload restore, restore, build and tests on push and PR to `main`.

If you need help installing platform-specific toolchains, see `specs/main/research.md` for recommendations and verification commands.
