# IxiHome

**IxiHome** is a decentralized, cross-platform smart home controller built on the [Ixian Platform](https://www.ixian.io). It provides secure, privacy-first control of your Home Assistant devices through QuIXI bridge integration, with post-quantum encryption and no central servers.

---

## 🚀 Features

- 🏠 **Smart Home Control** - Manage lights, switches, sensors, and more
- 🔒 **Post-Quantum Secure** - Uses Ixian's hybrid RSA + ECDH + ML-KEM encryption
- 🌐 **Decentralized** - No central servers, communicates via QuIXI bridge
- 📱 **Cross-Platform** - Android, iOS, Windows, and macOS support
- 🎨 **Customizable Widgets** - Choose from toggles, gauges, graphs, and sensors
- 📊 **Real-Time Updates** - Live entity state synchronization with 30s polling
- 🔔 **Push Notifications** - Get notified on important entity state changes
- 🌓 **Dark Mode** - Beautiful light and dark themes
- 🔐 **Secure Storage** - Local entity configuration with optional authentication
- ♻️ **Background Sync** - Automatic state synchronization in the background

---

## 📋 Prerequisites

- **.NET 9 SDK** or later
- **Visual Studio 2022** (with .NET MAUI workload) or VS Code
- **QuIXI Bridge** running and accessible
- **Home Assistant** instance (connected to QuIXI via MQTT)
- **MQTT Broker** (Mosquitto or similar)

---

## 🛠️ Build Instructions

### 1. Clone Repositories

```bash
# Clone Ixian-Core (required dependency)
git clone -b development https://github.com/ixian-platform/Ixian-Core.git

# Clone IxiHome
git clone https://github.com/ixian-platform/IxiHome.git
cd IxiHome
```

### 2. Restore Dependencies

```bash
dotnet restore IXIHOME.sln
```

### 3. Build the Project

#### Using Visual Studio
1. Open `IXIHOME.sln` in Visual Studio 2022
2. Select your target platform (Android, iOS, Windows, macOS)
3. Press `F5` to build and run

#### Using Command Line

```bash
# For Android
dotnet build -t:Run -f net9.0-android

# For iOS (requires Mac)
dotnet build -t:Run -f net9.0-ios

# For Windows
dotnet build -t:Run -f net9.0-windows10.0.19041.0 -p:Platform=x64

# For macOS
dotnet build -t:Run -f net9.0-maccatalyst
```

### 4. Release Build

```bash
dotnet build -c Release -f net9.0-android
# Output: IxiHome/bin/Release/net9.0-android/io.ixian.ixihome-Signed.apk
```

---

## ⚙️ Configuration

### QuIXI Setup

1. **Start QuIXI Bridge** with Home Assistant integration:
   ```bash
   cd QuIXI
   dotnet run
   ```

2. **Configure QuIXI** in `ixian.cfg`:
   ```ini
   apiPort = 8001
   mqDriver = mqtt
   mqHost = localhost
   mqPort = 1883
   ```

3. **Connect Home Assistant to MQTT**:
   - In Home Assistant, go to Settings → Integrations → MQTT
   - Configure broker address (same as QuIXI `mqHost`)

### IxiHome Setup

1. Launch IxiHome app
2. Open **Settings** (hamburger menu → Settings)
3. Configure QuIXI connection:
   - **QuIXI Address**: IP address of QuIXI bridge (e.g., `192.168.1.100`)
   - **Port**: `8001` (default)
   - **Username/Password**: (if configured in QuIXI)
4. Configure Home Assistant:
   - **Home Assistant URL**: Your HA instance URL
   - **Access Token**: Generate from HA (Profile → Long-Lived Access Tokens)
5. Tap **Test Connection** to verify
6. Tap **Save Settings**

---

## 📱 Usage

### Adding Entities

1. Go to **Home** page
2. Tap **+ Add Entity**
3. Tap **Load Entities from QuIXI**
4. Browse available Home Assistant entities
5. Select an entity to add
6. Choose widget type:
   - **Toggle** - For switches, lights (on/off control)
   - **Sensor** - For temperature, humidity (read-only display)
   - **Gauge** - For numeric values with visual gauge
   - **Graph** - For historical data visualization
7. Customize name and icon
8. Tap **Save**

### Controlling Entities

- **Toggle entities**: Tap to switch on/off
- **Lights**: Long-press for brightness/color control (coming soon)
- **View details**: Tap entity to see full state and attributes

### Organizing Entities

- **Reorder**: Long-press and drag entities (coming soon)
- **Edit**: Tap entity → Edit button
- **Delete**: Tap entity → Delete button → Confirm

---

## 🏗️ Architecture

### Project Structure

```
IxiHome/
├── IXIHOME.sln              # Solution file
├── IxiHome/
│   ├── IxiHome.csproj       # Project file
│   ├── App.xaml             # Application entry
│   ├── AppShell.xaml        # Shell navigation
│   ├── MauiProgram.cs       # MAUI initialization
│   ├── Meta/
│   │   ├── Config.cs        # App configuration
│   │   └── Node.cs          # Ixian node management
│   ├── Network/
│   │   └── QuixiClient.cs   # QuIXI bridge communication
│   ├── Data/
│   │   ├── Entity.cs        # Entity models
│   │   └── EntityManager.cs # Entity CRUD operations
│   ├── Pages/
│   │   ├── HomePage.xaml    # Main dashboard
│   │   ├── SettingsPage.xaml
│   │   ├── AddEntity/       # Add entity workflow
│   │   └── EntityDetail/    # Entity detail/edit
│   ├── Controls/            # Custom UI controls (coming soon)
│   └── Resources/
│       ├── Styles/          # Colors and styles
│       ├── Images/          # Icons and images
│       └── Fonts/           # OpenSans fonts
```

### Data Flow

```
┌─────────────────┐
│   IxiHome App   │
│   (MAUI UI)     │
└────────┬────────┘
         │
         ↓
┌────────────────────┐
│   QuixiClient      │
│  (REST/WebSocket)  │
└────────┬───────────┘
         │
         ↓
┌────────────────────┐
│   QuIXI Bridge     │
│  (Ixian Gateway)   │
└────────┬───────────┘
         │
         ↓ MQTT
┌────────────────────┐
│   MQTT Broker      │
│   (Mosquitto)      │
└────────┬───────────┘
         │
         ↓
┌────────────────────┐
│  Home Assistant    │
│   (Smart Home)     │
└────────────────────┘
```

### Key Components

- **EntityManager**: Singleton managing entity CRUD and persistence with JSON storage
- **QuixiClient**: HTTP client implementing IQuixiClient for QuIXI REST API
- **SyncService**: Handles bidirectional sync with 30s polling and event-based updates
- **NotificationService**: Cross-platform notification system for state changes
- **Node**: Manages Ixian node lifecycle and QuIXI connection
- **Config**: Handles app configuration and secure storage

---

## 🔐 Security

### Data Storage

- **Entity Configuration**: Stored locally in `entities.json`
- **Credentials**: Stored in platform-specific secure storage (Keychain/KeyStore)
- **Device ID**: Generated on first run, persists across app launches

### Network Security

- All communication with QuIXI uses Ixian's post-quantum encryption
- MQTT messages encrypted by Home Assistant/MQTT broker
- Optional: Enable authentication for app lock screen

---

## 🚧 Roadmap

### Phase 1 (Current - v0.1.0)
- ✅ Basic entity management (add, edit, delete)
- ✅ 4 widget types (Toggle, Sensor, Gauge, Graph)
- ✅ QuIXI HTTP client integration
- ✅ Settings page with secure storage
- ✅ Cross-platform support (Android, iOS, Windows, macOS)
- ✅ Background sync service with 30s polling
- ✅ Platform-specific notification services
- ✅ Interfaces for testability (IQuixiClient, IEntityManager, INotificationService)

### Phase 2 (v0.2.0)
- ⬜ Gauge widgets (circular/linear)
- ⬜ Graph widgets with historical data
- ⬜ Light brightness and color control
- ⬜ Drag-and-drop entity reordering
- ⬜ Entity grouping/folders

### Phase 3 (v0.3.0)
- ⬜ WebSocket/MQTT real-time updates
- ⬜ Push notifications for entity state changes
- ⬜ Automation/scene support
- ⬜ Widget customization (size, color schemes)

### Phase 4 (v0.4.0)
- ⬜ Multi-language support
- ⬜ Backup/restore configuration
- ⬜ Multi-QuIXI support (multiple homes)
- ⬜ Voice control integration

### Phase 5 (v1.0.0)
- ⬜ Dashboard layouts (grid, list, custom)
- ⬜ Advanced charting (energy monitoring, weather)
- ⬜ Camera/media entity support
- ⬜ Spixi Mini App integration

---

## 🤝 Contributing

We welcome contributions from developers and smart home enthusiasts!

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🔗 Links

- **Ixian Platform**: [https://www.ixian.io](https://www.ixian.io)
- **Ixian Documentation**: [https://docs.ixian.io](https://docs.ixian.io)
- **QuIXI Bridge**: [https://github.com/ixian-platform/QuIXI](https://github.com/ixian-platform/QuIXI)
- **Ixian Core**: [https://github.com/ixian-platform/Ixian-Core](https://github.com/ixian-platform/Ixian-Core)
- **Home Assistant**: [https://www.home-assistant.io](https://www.home-assistant.io)

---

## 💬 Support

- **Issues**: [GitHub Issues](https://github.com/ixian-platform/IxiHome/issues)
- **Discord**: [Ixian Community](https://discord.gg/ixian)
- **Email**: support@ixian.io

---

## 🙏 Acknowledgments

- Built on the [Ixian Platform](https://www.ixian.io)
- Uses [.NET MAUI](https://dotnet.microsoft.com/apps/maui) for cross-platform development
- Inspired by [Home Assistant](https://www.home-assistant.io) and [Spixi](https://github.com/ixian-platform/Spixi)
- Icons from [Material Design Icons](https://materialdesignicons.com/)

---

**Made with ❤️ by the Ixian Platform community**
