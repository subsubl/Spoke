# Home Assistant Integration (QuIXI)

This example shows how to create a **QuIXI script that connects to Home Assistant** and syncs smart home device states with Spoke via the Ixian blockchain network.

The script:
- Connects to Home Assistant via WebSocket API
- Syncs device states between Home Assistant and Spoke
- Responds to control commands from Spoke
- Provides real-time state updates

---

## 🛠 Requirements

* Linux system (Ubuntu/Debian/Raspberry Pi OS)
* Home Assistant instance with WebSocket API enabled
* .NET 8
* QuIXI + Ixian Core
* curl, jq, mosquitto-clients

---

## ⚡ Installation

```bash
# Install dependencies
sudo apt update
sudo apt install -y curl jq mosquitto mosquitto-clients python3 python3-pip

# Install Home Assistant Python library
pip3 install homeassistant-api

# Install .NET 8
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0 --version latest --verbose
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> ~/.bashrc
source ~/.bashrc

# Clone and build QuIXI + Ixian Core
git clone https://github.com/ixian-platform/Ixian-Core.git
git clone https://github.com/ixian-platform/QuIXI.git
cd QuIXI
dotnet build -c Release
```

---

## 📂 Project Structure

```
/HomeAssistant
 ├── start.sh                    # Launches QuIXI and Home Assistant sync script
 ├── ixiAutoAccept.sh           # Automatically accepts incoming contacts
 ├── ixiMessageHandler.sh       # Handles Spoke control commands
 ├── hass_sync.py              # Home Assistant WebSocket sync script
 ├── hass_config.json          # Home Assistant connection config
 ├── helpers.sh                # Utility functions
 ├── quixi.service             # Systemd service file
 └── README.md                 # This file
```

---

## 🔧 Configuration

1. **Configure Home Assistant connection** in `hass_config.json`:
```json
{
  "url": "ws://homeassistant.local:8123/api/websocket",
  "token": "your_long_lived_access_token",
  "sync_interval": 30
}
```

2. **Get Home Assistant Access Token**:
   - Go to Home Assistant → Profile → Long-Lived Access Tokens
   - Create a new token for QuIXI integration

---

## 🚀 Usage

1. **Start the service**:
```bash
./start.sh
```

2. **Available commands** (send via Ixian chat):
   - `status` - Get Home Assistant status
   - `devices` - List all devices
   - `control <entity_id> <state>` - Control device (e.g., `control light.living_room on`)
   - `sync` - Force sync all states
   - `help` - Show available commands

---

## 🔄 How It Works

1. **QuIXI Connection**: The script connects to QuIXI via Ixian network
2. **Home Assistant Sync**: Maintains WebSocket connection to Home Assistant
3. **State Synchronization**: Bidirectional sync between Spoke and Home Assistant
4. **Command Processing**: Processes control commands from Spoke users
5. **Real-time Updates**: Pushes state changes to Spoke immediately

---

## 🔒 Security

- All communication happens via the secure Ixian blockchain network
- Home Assistant access tokens are stored securely
- No direct internet exposure of Home Assistant
- Commands are validated before execution

---

## 📊 Supported Devices

- Lights (on/off/brightness/color)
- Switches
- Climate/HVAC controls
- Sensors (temperature, humidity, etc.)
- Binary sensors
- Locks
- Covers (blinds, garage doors)

---

## 🐛 Troubleshooting

**Connection Issues**:
- Check Home Assistant WebSocket URL and token
- Verify network connectivity
- Check QuIXI logs: `tail -f ~/.config/QuIXI/logs/*`

**Sync Problems**:
- Restart the sync script: `./hass_sync.py restart`
- Check Home Assistant logs
- Verify entity IDs are correct

**Command Issues**:
- Use `devices` command to list available entities
- Check entity states in Home Assistant UI
- Verify command format