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

## Build Commands

### Development Build
```bash
# Windows
dotnet build -c Debug -f net9.0-windows10.0.19041.0 -p:Platform=x64

# Android  
dotnet build -c Debug -f net9.0-android

# iOS (requires Mac)
dotnet build -c Debug -f net9.0-ios
```

### Release Build
```bash
dotnet build -c Release -f net9.0-android
# Output: bin/Release/net9.0-android/io.ixian.Spoke-Signed.apk
```

### Prerequisites
- .NET 9 SDK
- Visual Studio 2022 with MAUI workload
- Ixian-Core dependency (referenced via projitems)

## Development Patterns

### MVVM Architecture
- Use `CommunityToolkit.Mvvm` for ViewModels and commands
- `ObservableCollection<Entity>` for reactive entity lists
- `ObservableObject` for entity models with property change notifications

### Entity Management
```csharp
// Add entity
var entity = await EntityManager.Instance.AddEntityAsync(new ToggleEntity { 
    Name = "Living Room Light",
    EntityId = "light.living_room" 
});

// Update state
entity.State = "on";
await EntityManager.Instance.UpdateEntityAsync(entity);
```

### QuIXI Communication
```csharp
// Send command
await quixiClient.SendCommandAsync("light.living_room", "turn_on");

// Subscribe to updates
quixiClient.EntityStateChanged += (s, e) => {
    EntityManager.Instance.UpdateEntityState(e.EntityId, e.State, e.Attributes);
};
```

### Persistence
- Entities stored as JSON in `Config.entitiesFilePath`
- Settings in `Preferences.Default` and `SecureStorage.Default`
- Automatic save on entity changes

### Security
- Ixian post-quantum encryption for all network communication
- SecureStorage for credentials, Preferences for settings
- No direct Home Assistant internet exposure

### Async Patterns
- All network operations are `async Task`
- Use `CancellationToken` for cancellable operations
- WebSocket reconnection with exponential backoff

### Error Handling
- Log errors with `IXICore.Logging.error()`
- Graceful fallbacks for network failures
- User-friendly error messages in UI

## Code Organization

### File Structure
```
Spoke/
├── MauiProgram.cs          # App initialization & lifecycle
├── App.xaml/cs             # Shell navigation & Ixian node management
├── Meta/
│   ├── Config.cs           # Settings & secure storage
│   └── Node.cs             # Ixian node lifecycle
├── Data/
│   ├── Entity.cs           # Entity models (6 types)
│   └── EntityManager.cs    # CRUD operations & persistence
├── Network/
│   └── QuixiClient.cs      # QuIXI HTTP/WebSocket client
├── Services/
│   ├── SyncService.cs      # Real-time sync
│   └── NotificationService.cs # Cross-platform notifications
├── Pages/                  # XAML pages (HomePage, SettingsPage, etc.)
├── Controls/               # Custom entity controls
└── Utils/                  # Converters, selectors, managers
```

### Naming Conventions
- ViewModels: `PageNameViewModel.cs`
- Services: `ServiceName.cs` (static Instance property)
- Entities: `EntityTypeEntity.cs` (ToggleEntity, SensorEntity, etc.)
- Interfaces: `IInterfaceName.cs`

### Naming Conventions
- ViewModels: `PageNameViewModel.cs`
- Services: `ServiceName.cs` (static Instance property)
- Entities: `EntityTypeEntity.cs` (ToggleEntity, SensorEntity, etc.)
- Interfaces: `IInterfaceName.cs`

### Dependencies
- `CommunityToolkit.Maui` - UI toolkit
- `CommunityToolkit.Mvvm` - MVVM framework  
- `Newtonsoft.Json` - JSON serialization
- `Microcharts.Maui` - Charts for GraphEntity
- `Microsoft.Toolkit.Uwp.Notifications` - Windows notifications
- `BouncyCastle.Cryptography` - Crypto utilities
- `Open.NAT` - UPnP/NAT traversal

## Testing

### Unit Tests
- Test entity CRUD operations
- Mock QuixiClient for network tests
- Test converters and utilities

### Integration Tests  
- Test full sync cycle with mock QuIXI
- Test WebSocket reconnection
- Test entity state updates

## Deployment

### Android
- APK signed with platform key
- Supports Android 5.0+ (API 21)
- Uses Android App Bundles

### Windows
- WinUI 3 app packaged as MSIX
- Windows 10 version 1903+ (build 18362)
- Self-contained deployment

### iOS/macOS
- Requires Apple developer account
- Signed with distribution certificate
- App Store deployment

## Key Integration Points

### QuIXI Bridge Setup
1. Deploy Python WebSocket client on HA server
2. Configure MQTT connection in QuIXI
3. Set bridge IP/port in Spoke settings
4. Test connection before adding entities

### Home Assistant Integration
- No direct API calls - all through QuIXI
- MQTT broker handles HA communication
- Entities discovered via QuIXI `/entities` endpoint
- Commands sent via QuIXI `/command` endpoint

### Ixian Identity
- Wallet creation during onboarding
- Username/avatar selection
- Cryptographic address as identity
- Secure key storage

## Common Pitfalls

- **Direct HA Access**: Never call HA API directly - always through QuIXI
- **Threading**: UI updates must be on main thread
- **Persistence**: Always call `SaveEntitiesAsync()` after entity changes
- **WebSocket**: Handle reconnection gracefully, don't block UI
- **Security**: Never log sensitive data, use SecureStorage for credentials

## Performance Considerations

- WebSocket preferred over polling for real-time updates
- Entity state caching in memory with JSON backup
- Lazy loading for large entity lists
- Background sync with battery optimization
- Minimal UI redraws using ObservableObject properly
