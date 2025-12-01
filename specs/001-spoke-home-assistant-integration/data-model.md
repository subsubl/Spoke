# Data Model: Spoke Home Assistant Integration

**Purpose**: Define the core data entities and relationships for the Spoke smart home controller
**Date**: 2025-12-01

## Core Entities

### Entity
Represents a Home Assistant device or service that can be controlled through the app.

**Attributes:**
- `Id`: Unique identifier (string, required)
- `Name`: Human-readable display name (string, required)
- `Type`: Entity type (enum: light, switch, sensor, climate, cover, binary_sensor, etc.)
- `State`: Current state value (string, e.g., "on", "off", "25.5")
- `Attributes`: Additional properties (Dictionary<string, object>)
- `LastUpdated`: Timestamp of last state change (DateTime)
- `IsAvailable`: Whether entity is currently reachable (bool)

**Relationships:**
- Belongs to HomeAssistantConfig (many-to-one)
- No direct relationships to other entities

**Validation Rules:**
- Id must be unique within Home Assistant instance
- Type must be valid Home Assistant entity type
- State must be compatible with entity type

### Wallet
Contains cryptographic identity and keys for blockchain-based authentication.

**Attributes:**
- `PublicKey`: Public key for identity verification (byte[], required)
- `PrivateKey`: Encrypted private key (byte[], required, encrypted at rest)
- `Address`: Derived blockchain address (string, computed)
- `Username`: Human-readable identifier (string, optional)
- `ProfileImage`: Base64-encoded avatar (string, optional)
- `Created`: Wallet creation timestamp (DateTime)

**Relationships:**
- One-to-one with user session
- Referenced by QuixiConnection for authentication

**Validation Rules:**
- Public/private key pair must be cryptographically valid
- Address must match derived value from public key
- Private key must be encrypted using platform secure storage

### QuixiConnection
Manages connection parameters for the QuIXI bridge.

**Attributes:**
- `Host`: Bridge server address (string, required)
- `Port`: Bridge server port (int, default 8001)
- `Username`: Authentication username (string, optional)
- `Password`: Authentication password (string, optional, encrypted)
- `IsConnected`: Current connection status (bool)
- `LastConnected`: Timestamp of last successful connection (DateTime)

**Relationships:**
- References Wallet for authentication
- Referenced by EntityManager for API calls

**Validation Rules:**
- Host must be valid IP address or hostname
- Port must be valid TCP port number
- Connection must be testable before saving

### HomeAssistantConfig
Stores Home Assistant instance configuration.

**Attributes:**
- `Url`: Home Assistant base URL (string, required)
- `AccessToken`: Long-lived access token (string, required, encrypted)
- `WebSocketUrl`: WebSocket API URL (string, computed from Url)
- `SyncInterval`: State sync frequency in seconds (int, default 30)
- `IsConfigured`: Whether configuration is complete (bool)

**Relationships:**
- One-to-many with Entity
- Referenced by QuixiClient for API communication

**Validation Rules:**
- Url must be valid HTTP/HTTPS URL
- AccessToken must be valid Home Assistant token
- WebSocket connection must be testable

## Data Flow

1. **Authentication**: Wallet provides identity for QuixiConnection
2. **Connection**: QuixiConnection enables communication with bridge
3. **Discovery**: HomeAssistantConfig + QuixiConnection retrieve Entity list
4. **Control**: Entity state changes flow through QuixiConnection to Home Assistant
5. **Sync**: Bidirectional state updates between Entity cache and Home Assistant

## Storage Strategy

- **Entity**: Local JSON file (entities.json) for offline access
- **Wallet**: Platform secure storage (Keychain/KeyStore)
- **QuixiConnection**: Platform secure storage with encryption
- **HomeAssistantConfig**: Platform secure storage with encryption

## Migration Considerations

- Entity schema must support Home Assistant API evolution
- Wallet format must remain compatible with Ixian-Core updates
- Connection configs must migrate between app versions
- No breaking changes to stored data without migration path