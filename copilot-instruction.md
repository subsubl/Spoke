# Copilot Instructions for Spoke Project

## Project Overview

**Spoke** is a secure, blockchain-based smart home control application built with .NET MAUI. It provides a decentralized alternative to traditional smart home hubs by leveraging the Ixian blockchain network for secure, private communication with Home Assistant.

### Key Features
- **Blockchain Security**: Uses Ixian Core with post-quantum cryptography (RSA + ECDH + ML-KEM)
- **Spixi Integration**: Cryptographic wallet creation and identity management
- **Real-time Sync**: WebSocket-based communication via QuIXI bridge
- **Cross-platform**: Windows, Android, iOS, macOS support
- **No Direct Internet Exposure**: Home Assistant remains local and secure

## Architecture

```
┌─────────────────────────────────────────────────┐
│              Spoke MAUI App                     │
├─────────────────────────────────────────────────┤
│  UI Layer                                       │
│  - Pages (Home, Settings, About, Onboarding)   │
│  - Controls (Toggle, Sensor, Gauge, Graph)     │
│  - Animations & Theming                        │
├─────────────────────────────────────────────────┤
│  Identity Layer                                 │
│  - Spixi Wallet Integration                    │
│  - Username & Avatar Management                │
├─────────────────────────────────────────────────┤
│  Services Layer                                 │
│  - SyncService (WebSocket real-time sync)      │
│  - NotificationService (cross-platform)        │
├─────────────────────────────────────────────────┤
│  Business Layer                                 │
│  - EntityManager (CRUD + persistence)          │
│  - QuixiClient (HTTP + WebSocket API)          │
│  - Node (Ixian lifecycle)                      │
├─────────────────────────────────────────────────┤
│  Data Layer                                     │
│  - Entity models (6 types)                     │
│  - Config (settings + secure storage)          │
│  - JSON persistence                            │
├─────────────────────────────────────────────────┤
│  Ixian Core + Spixi + Home Assistant           │
│  - Crypto libraries                            │
│  - Wallet Management                           │
│  - Local HA integration via QuIXI              │
└─────────────────────────────────────────────────┘
```

## How to Use Copilot Effectively

### General Guidelines
- **Be Specific**: Include file paths, class names, and context in your queries
- **Reference Existing Code**: Mention similar implementations or patterns
- **Specify Platforms**: Indicate if changes are platform-specific (Android/iOS/Windows)
- **Include Requirements**: State functional and non-functional requirements

### Common Query Patterns

#### Code Implementation
```
"Add a new entity type for climate control in Spoke/Data/Entity.cs"
"Implement OAuth authentication in Spoke/Services/"
"Create a custom control for thermostat in Spoke/Controls/"
```

#### Bug Fixes
```
"Fix the WebSocket reconnection logic in Spoke/Services/SyncService.cs"
"Resolve the gauge animation issue in Spoke/Controls/GaugeEntityControl.xaml.cs"
```

#### Architecture Questions
```
"How does the QuIXI bridge communicate with Home Assistant?"
"Explain the entity synchronization flow"
"Where should I add platform-specific notification code?"
```

#### Testing & Validation
```
"Add unit tests for EntityManager in Spoke.Tests/"
"Test the onboarding flow on Android emulator"
"Validate the WebSocket connection stability"
```

### Best Practices for Prompts

#### ✅ Good Examples
- "Implement a new sensor entity type for temperature readings, similar to the existing SensorEntity"
- "Add error handling to the QuixiClient.GetEntitiesAsync() method with retry logic"
- "Create a settings page for notification preferences in Spoke/Pages/SettingsPage.xaml"

#### ❌ Avoid These
- "Make it work" (too vague)
- "Fix everything" (too broad)
- "Add features" (insufficient detail)

### File Organization
- **Spoke/**: Main application code
  - **Data/**: Entity models and EntityManager
  - **Services/**: Background services (Sync, Notifications)
  - **Controls/**: Custom UI controls
  - **Pages/**: XAML pages and view models
  - **Utils/**: Converters, helpers, themes
  - **Platforms/**: Platform-specific code
- **Ixian-Core/**: Blockchain core libraries
- **Spixi/**: Wallet and messaging app
- **QuIXI/**: Home Assistant bridge
- **QuixiScripts/**: HA integration scripts

### Development Workflow

1. **Understand Requirements**: Review existing code and documentation
2. **Plan Changes**: Identify affected files and dependencies
3. **Implement Incrementally**: Make small, testable changes
4. **Test Thoroughly**: Use emulators/simulators for cross-platform testing
5. **Document Changes**: Update README and DEVELOPMENT.md as needed

### Key Technologies
- **.NET MAUI**: Cross-platform UI framework
- **CommunityToolkit.Mvvm**: MVVM pattern implementation
- **Microcharts**: Chart visualization
- **Microsoft.Maui.Graphics**: Custom drawing
- **WebSocketSharp**: Real-time communication
- **SQLite**: Local data persistence

### Security Considerations
- All communication goes through Ixian blockchain network
- No direct internet access to Home Assistant
- Secure storage for credentials and wallet data
- Post-quantum cryptographic algorithms

### Performance Tips
- Use ObservableCollection for reactive UI updates
- Implement proper async/await patterns
- Cache frequently accessed data
- Minimize WebSocket message frequency

### Troubleshooting
- Check DEVELOPMENT.md for current status
- Review build logs for platform-specific errors
- Test on target platforms early
- Use Git bisect for regression issues

---

*This document helps Copilot understand the Spoke project context and provide more accurate, relevant assistance. Update as the project evolves.*</content>
<parameter name="filePath">c:\Users\User\IxiHome\copilot-instruction.md