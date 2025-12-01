# Research: MAUI / .NET 10 Compatibility

**Created**: 2025-12-01
**Purpose**: Capture platform SDK, workload, and third-party dependency compatibility checks required to build and run MAUI apps targeting .NET 10 (`10.0.100`).

## Summary

This document lists the recommended developer and CI platform SDKs, .NET workloads, and a plan to validate third-party NuGet packages used by the repo (OneSignal, Plugin.Fingerprint, Open.Nat, etc.). The goal is to ensure we can reliably build and test the MAUI apps targeting `net10.0-*` TFMs on developer machines and CI runners.

## Required SDKs & Tools (Developer + CI)

- .NET SDK
  - **Required**: `10.0.100` (pinned via `global.json`). Verify with `dotnet --version`.

- MAUI workloads
  - Install workloads: `maui`, `maui-android`, `maui-ios`, `maui-maccatalyst`, `maui-windows` (if building on Windows/WinUI).
  - Install command:
    ```powershell
    dotnet workload install maui maui-android maui-ios maui-maccatalyst maui-windows
    ```
  - Restore workloads in CI before restore/build: `dotnet workload restore`.

- Android (for Android target)
  - Recommended Android SDK platform: **API level 35** (`android-35`) — repository `Spixi` currently uses `TargetSdkVersion=35`.
  - Install Android SDK command-line tools and the platform SDK(s) required by the Android workload.
  - Java JDK: either **Temurin/OpenJDK 11** or **17** (verify CI runner environment). Use `java -version`.

- iOS / MacCatalyst (for Apple targets)
  - Xcode: **Xcode 15.x** or newer (match the .NET/macOS toolchain requirements for MAUI on .NET 10). Projects use `SupportedOSPlatformVersion` 15.0 — Xcode 15 recommended.
  - Ensure `dotnet workload install maui-ios maui-maccatalyst` on macOS runners.

- Windows (for WinUI builds)
  - Windows SDK and Windows App SDK as required. The repo references `Microsoft.WindowsAppSDK` (e.g., 1.8.3); validate the Windows App SDK version for compatibility with .NET 10 / MAUI 10.
  - Ensure any required Visual Studio components or MSBuild components are present on Windows runners.

## MAUI Package Versions in Repo

- `Microsoft.Maui.Controls` / `Microsoft.Maui.Controls.Compatibility`: `10.0.10` (used in `Spoke` and `Spixi` projects) — aligns with .NET 10 in this workspace.
- `CommunityToolkit.Maui`: `13.0.0` — aligned across apps.

These versions are consistent with the repo edits and are the baseline for compatibility tests.

## Third-Party Packages to Validate

List of packages seen in the repo that should be validated for net10.0 compatibility:

- `OneSignalSDK.DotNet` (Push/notifications)
- `Plugin.Fingerprint` (biometrics)
- `Open.NAT` (UPnP)
- `BouncyCastle.Cryptography` (crypto)
- `Concentus` (audio)
- `NAudio` (desktop audio)
- `Newtonsoft.Json` (JSON serialization)

Validation steps for each package:

1. Check NuGet metadata for supported target frameworks: open package page or run:
   ```powershell
   dotnet nuget list source # (ensure nuget.org available)
   # Inspect metadata in browser: https://www.nuget.org/packages/<PackageName>
   ```
2. Add or restore packages on a small sample net10.0 MAUI project and attempt `dotnet build`.
3. Run `dotnet list package --vulnerable` in the repo to find vulnerability advisories.
4. If a package is incompatible, look for newer package versions, or an alternative package, or add compatibility shims with justification.

## Quick Validation Commands

From repo root (PowerShell):

```powershell
# Verify SDK
dotnet --version

# Restore workloads (local dev or CI)
dotnet workload restore

# Restore solution
dotnet restore .\Spoke\SPOKE.sln

# Build solution (example)
dotnet build .\Spoke\SPOKE.sln -c Release

# Run package vulnerability scan
dotnet list package --vulnerable
```

## CI Considerations

- CI runners must run `dotnet workload restore` prior to `dotnet restore`.
- Matrix jobs:
  - `ubuntu-latest` for shared logic and non-Windows builds
  - `macos-latest` for iOS/MacCatalyst builds and tests (Xcode required)
  - `windows-latest` for WinUI/Windows builds
- Use hosted runners that include required SDKs or install required toolchain steps in the workflow.
- If running UI or emulator tests: configure Android emulator setup on Ubuntu/Windows and require provisioning for signing on macOS.

## Risk Assessment

- High risk: Third-party packages that haven't been validated on net10.0 may fail at restore or runtime.
- Medium risk: CI runners lacking required workloads or SDK versions will cause pipeline failures.
- Low risk: Example/docs projects targeting older TFMs (net8.0) do not block the MAUI apps but should be noted for consistency.

## Action Items (owners should pick these)

- [ ] Validate all third-party NuGet packages by restoring and building a local net10.0 MAUI project (Owner: @maintainer)
- [ ] Add `dotnet workload restore` to CI workflow and verify builds on all required runners (Owner: @ci)
- [ ] Document developer quickstart steps in `specs/main/quickstart.md` including Android SDK install and Xcode requirements (Owner: @devexp)
- [ ] If incompatible packages are found, open issues/PRs to update or replace the packages (Owner: @maintainer)
- [ ] Add Dependabot config or CI `dotnet list package --vulnerable` check (Owner: @security)

## Notes

- The exact Android API level and Xcode version requirements may be tightened during Phase 0 research as MAUI/.NET 10 evolve. Use the above as initial targets; update after concrete verification.
