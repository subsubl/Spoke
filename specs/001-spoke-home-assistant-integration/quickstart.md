# Quick Start: Spoke Home Assistant Integration

**Get started with Spoke in 10 minutes**

## Prerequisites

- **.NET 10 SDK** installed
- **QuIXI Bridge** running and accessible
- **Home Assistant** instance with WebSocket API enabled
- **Long-lived access token** from Home Assistant

## 1. Clone and Setup

```bash
# Clone the repository
git clone https://github.com/ixian-platform/Spoke.git
cd Spoke

# Restore dependencies
dotnet restore Spoke.sln
```

## 2. Configure Home Assistant

1. **Enable WebSocket API** (usually enabled by default)
2. **Create access token**:
   - Go to Home Assistant → Profile → Long-Lived Access Tokens
   - Create new token: "Spoke Integration"
   - Copy the token for later use

## 3. Setup QuIXI Bridge

```bash
# Ensure QuIXI is running on default port 8001
# Configure connection to your Home Assistant instance
```

## 4. Build and Run

```bash
# Build for your platform
dotnet build -c Release -f net10.0-android    # Android
dotnet build -c Release -f net10.0-ios        # iOS
dotnet build -c Release -f net10.0-windows10.0.19041.0 -p:Platform=x64  # Windows

# Run the app
dotnet run --project Spoke/Spoke.csproj
```

## 5. First-Time Setup

1. **Launch Spoke** - App opens to welcome screen
2. **Create Wallet** - Tap "Create New Wallet"
   - Generate secure cryptographic keys
   - Set username and profile picture
3. **Configure Connection**:
   - **QuIXI Host**: IP address of QuIXI bridge (e.g., `192.168.1.100`)
   - **Port**: `8001` (default)
   - **Home Assistant URL**: Your HA instance (e.g., `http://homeassistant.local:8123`)
   - **Access Token**: Paste the token from step 2
4. **Test Connection** - Verify all connections work
5. **Load Entities** - Import your smart home devices

## 6. Control Your Home

- **Home Page**: View all your entities
- **Tap entities** to control (lights, switches, etc.)
- **Long-press** for advanced controls (brightness, color)
- **Settings** for configuration changes

## Troubleshooting

### Connection Issues
- **QuIXI not reachable**: Check bridge is running on correct port
- **Home Assistant auth failed**: Verify access token is valid
- **No entities found**: Ensure Home Assistant has devices configured

### Build Issues
- **Missing .NET 10**: Install latest .NET SDK
- **Platform tools**: Ensure Android/iOS/Windows development tools installed

### Runtime Issues
- **Wallet corrupted**: Clear app data and recreate wallet
- **Sync not working**: Check network connectivity and restart app

## Next Steps

- Explore advanced features in the full documentation
- Join the Ixian community for support
- Contribute improvements back to the project

## Security Notes

- Your private keys never leave your device
- All communication is end-to-end encrypted
- No data is stored on central servers
- Access tokens are encrypted locally