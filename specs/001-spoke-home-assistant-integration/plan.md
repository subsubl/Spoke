# Implementation Plan: Spoke Home Assistant Integration

**Branch**: `001-spoke-home-assistant-integration` | **Date**: 2025-12-01 | **Spec**: specs/001-spoke-home-assistant-integration/spec.md
**Input**: Feature specification from `/specs/001-spoke-home-assistant-integration/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Build the Spoke smart home controller app with QuixiScript integration for Home Assistant OS API. The app provides secure, decentralized control of Home Assistant devices through blockchain identity and QuIXI bridge communication, featuring post-quantum encryption and cross-platform support.

## Technical Context

**Language/Version**: C# / .NET 10 MAUI  
**Primary Dependencies**: Ixian-Core (blockchain/crypto), QuIXI (bridge communication), Home-assistant-android (reference SDK)  
**Storage**: Local JSON files for entities, platform secure storage for credentials and keys  
**Testing**: xUnit for unit tests, integration tests for QuIXI/Home Assistant communication  
**Target Platform**: Cross-platform mobile (Android, iOS, Windows, macOS)  
**Project Type**: Mobile application with bridge integration  
**Performance Goals**: Entity state sync within 2 seconds, onboarding completion under 5 minutes  
**Constraints**: Post-quantum secure encryption, no central servers, offline-capable for cached entities  
**Scale/Scope**: Single user smart home (up to 100 entities), cross-platform deployment

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Gates are derived from `.specify/memory/constitution.md`. Every plan MUST
include a `Constitution Compliance` subsection that documents, at minimum,
the following check items and their status (PASS/FAIL/NA):

- **Safety & Security (PASS)**: Private keys encrypted using Ixian-Core crypto, threat modeling includes QuIXI bridge attacks and Home Assistant API vulnerabilities.
- **Test-First (PASS)**: Unit tests for all models/services, integration tests for QuIXI communication, contract tests for Home Assistant API.
- **Observability & Debuggability (PASS)**: Structured logging for connection events, metrics for sync performance, traces for control commands.
- **API Stability & Versioning (PASS)**: Home Assistant API follows semantic versioning, QuIXI protocol maintains backward compatibility.
- **Simplicity & Minimal Surface Area (PASS)**: Clean MAUI MVVM architecture with minimal public interfaces.
- **Styleguide and SDK Compliance (PASS)**: Follows Ixian-Core crypto patterns, QuIXI communication protocols, Ixian-Docs structure, Home-assistant-android mobile architecture.
- **Home Assistant OS API Integration (PASS)**: API calls authenticated with tokens, rate-limited, error-handled via QuixiScript.
- **Additional Constraints (PASS)**: Release artifacts signed, CI includes security scans, environment variables for config.
- **Development Workflow (PASS)**: Feature tracked in GitHub issues, PRs require tests and reviews, migration plan for protocol changes.

All gates PASS - no violations or unresolved issues.

## Project Structure

### Documentation (this feature)

```text
specs/001-spoke-home-assistant-integration/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Spoke/
├── Spoke.sln              # Solution file
├── Spoke/
│   ├── Spoke.csproj       # Main project
│   ├── App.xaml           # MAUI app entry
│   ├── AppShell.xaml      # Navigation shell
│   ├── MauiProgram.cs     # DI and services
│   ├── Meta/
│   │   ├── Config.cs      # App configuration
│   │   └── Node.cs        # Ixian node management
│   ├── Network/
│   │   └── QuixiClient.cs # QuIXI bridge client
│   ├── Data/
│   │   ├── Entity.cs      # Entity models
│   │   └── EntityManager.cs # Entity CRUD
│   ├── Pages/
│   │   ├── HomePage.xaml  # Main dashboard
│   │   ├── SettingsPage.xaml
│   │   ├── AddEntityPage.xaml
│   │   └── EntityDetailPage.xaml
│   ├── Controls/          # Custom UI controls
│   └── Resources/         # Styles, images, fonts
├── Spoke.Core/            # Shared core logic
├── Spoke.Tests/           # Unit tests
└── Spoke.UnitTestsClean/  # Additional tests
```

**Structure Decision**: Uses existing MAUI project structure with clear separation of concerns - UI in Pages/, business logic in Data/Network/, shared code in Core/. Follows standard .NET MAUI patterns with XAML for UI and C# for logic.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
