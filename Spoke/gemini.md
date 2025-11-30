# Gemini Coder Notes: Spoke Project

This file contains notes about the Spoke project.

## Project Description

Spoke is a .NET MAUI application that functions as a decentralized, secure smart home controller for Home Assistant.

## Key Features

*   Cross-platform (Android, iOS, Windows, macOS).
*   Uses QuIXI to communicate with Home Assistant.
*   Integrates Spixi for identity and wallet functionality.
*   Built on the Ixian-Core SDK.

## Technical Details

*   **Framework:** .NET MAUI
*   **Language:** C#
*   **Solution File:** `SPOKE.sln`
*   **Project File:** `Spoke/Spoke.csproj`

## Build Issue Fix

The original `Spoke.csproj` file only targeted Windows. This was causing a build issue. I updated the `<TargetFrameworks>` to match the multi-targeting setup in `Spixi.csproj`, which should resolve the problem.

```xml
<TargetFrameworks>net9.0-android;net9.0-ios;net9.0-maccatalyst</TargetFrameworks>
<TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('windows'))">$(TargetFrameworks);net9.0-windows10.0.19041.0</TargetFrameworks>
```