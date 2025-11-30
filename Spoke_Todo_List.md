# Todo List: Features for Spoke Inspired by Home Assistant App

Based on analysis of the Home Assistant Android app repository (https://github.com/home-assistant/android), here's a prioritized todo list of features to implement in Spoke (a MAUI-based Windows app for smart home control on the Ixian network).

## 1. Device Entity Controls (High Priority)
- **Description**: Implement controls for various smart home entities like lights, switches, climate (thermostats), covers (blinds/curtains), fans, locks, media players, and vacuums. Allow users to toggle, adjust sliders, and send commands via the app UI.
- **Why**: Core to smart home control; HA app has dedicated control classes (e.g., `LightControl.kt`, `ClimateControl.kt`).
- **Implementation Steps**:
  - Create control classes for each entity type (e.g., `LightControl`, `SwitchControl`).
  - Integrate with Ixian network API for sending commands.
  - Add UI components (buttons, sliders) in MAUI pages.
  - Support quick actions from home screen or notifications.

## 2. Sensor Data Collection and Reporting (High Priority)
- **Description**: Collect and report sensor data from the device (e.g., location, battery level, Wi-Fi state, health data like steps/heart rate if available on Windows).
- **Why**: HA app includes extensive sensors (e.g., `AppSensorManager.kt`, `GeocodeSensorManager.kt`, `HealthConnectSensorManager.kt`); enables automation based on device state.
- **Implementation Steps**:
  - Implement sensor managers for Windows-specific data (e.g., location via Windows.Devices.Geolocation).
  - Periodically collect and send data to Ixian network.
  - Add settings to enable/disable sensors.
  - Display sensor data in the app UI.

## 3. Notifications Handling and Actions (High Priority)
- **Description**: Receive notifications from the smart home system and allow interactive actions (e.g., reply, dismiss, trigger automations).
- **Why**: HA app has robust notification management (e.g., `MessagingManager.kt`, `NotificationActionReceiver.kt`); critical for real-time alerts.
- **Implementation Steps**:
  - Integrate with Windows Notification API.
  - Parse incoming notifications from Ixian network.
  - Add action buttons (e.g., "Turn off light") in notifications.
  - Log notification history in the app.

## 4. Widget Support (Medium Priority)
- **Description**: Provide desktop widgets for quick access to entities (e.g., button widgets, entity state widgets, media controls).
- **Why**: HA app supports Android widgets (e.g., `button/`, `entity/`, `mediaplayer/`); enhances usability without opening the app.
- **Implementation Steps**:
  - Use Windows Widgets API (if available) or create custom overlay windows.
  - Implement widget providers for common entities.
  - Allow configuration via settings.

## 5. Webview Integration for Dashboards (Medium Priority)
- **Description**: Embed a webview for custom dashboards or HA-like interfaces, allowing users to view and interact with web-based controls.
- **Why**: HA app uses webviews for frontend access (e.g., `WebViewActivity.kt`); useful for complex UIs.
- **Implementation Steps**:
  - Add a WebView control in MAUI.
  - Load Ixian-based web interfaces or custom HTML.
  - Handle authentication and secure connections.

## 6. Settings Management (Medium Priority)
- **Description**: Comprehensive settings for server connections, sensors, notifications, themes, and more.
- **Why**: HA app has extensive settings (e.g., `settings/` directory with sub-modules for sensors, notifications, etc.); essential for customization.
- **Implementation Steps**:
  - Create settings pages for each category (e.g., server URL, sensor toggles).
  - Persist settings using MAUI Preferences.
  - Include advanced options like developer tools.

## 7. Onboarding and Authentication (High Priority)
- **Description**: Guided setup process for connecting to Ixian network, including server discovery, authentication, and initial configuration.
- **Why**: HA app has a full onboarding flow (e.g., `onboarding/` with authentication, discovery); improves first-time user experience.
- **Implementation Steps**:
  - Create onboarding screens (welcome, server setup, auth).
  - Implement server discovery (e.g., via network scanning).
  - Handle login and token management.

## 8. Voice Assistant Integration (Low-Medium Priority)
- **Description**: Integrate voice commands for controlling devices (e.g., "Turn on the lights").
- **Why**: HA app includes assist features (e.g., `assist/`); leverages Windows Cortana or custom speech recognition.
- **Implementation Steps**:
  - Use Windows.Media.SpeechRecognition API.
  - Parse commands and map to entity actions.
  - Add a voice input UI.

## 9. Theme and Appearance Customization (Low Priority)
- **Description**: Support light/dark themes and dynamic colors.
- **Why**: HA app has theme management (e.g., `themes/`); enhances personalization.
- **Implementation Steps**:
  - Implement theme switching in MAUI.
  - Detect system theme changes.

## 10. Websocket for Real-Time Updates (High Priority)
- **Description**: Maintain a websocket connection for live data updates (e.g., entity states, events).
- **Why**: HA app uses websockets (e.g., `WebsocketManager.kt`); ensures real-time responsiveness.
- **Implementation Steps**:
  - Implement websocket client for Ixian network.
  - Handle reconnections and background updates.
  - Update UI in real-time.

## 11. Additional Integrations (Low-Medium Priority)
- **NFC and Barcode Scanning**: For quick actions or device pairing (HA has `nfc/`, `barcode/`).
- **Quick Settings (QS) Tiles**: Windows equivalent for quick toggles (HA has `qs/`).
- **Shortcuts and Gestures**: Custom shortcuts for actions (HA has `shortcuts/`, `gestures/`).
- **Vehicle and Wear Support**: If applicable, but lower priority for desktop (HA has `vehicle/`, `wear/`).
- **Health and Activity Sensors**: Expand sensor support (HA has extensive health permissions).

## Notes
- **Prioritization**: Focused on core smart home features first (controls, sensors, notifications). Lower-priority items can be added iteratively.
- **Adaptation**: Since Spoke is Windows/MAUI-based, adapt Android-specific features (e.g., Android permissions) to Windows APIs (e.g., location via `Windows.Devices.Geolocation`).
- **Dependencies**: Ensure compatibility with Ixian network APIs. May need to add packages like `System.Net.WebSockets` for websockets.
- **Testing**: Each feature should include unit tests and UI validation.
- **Security**: Handle authentication securely, avoiding plain-text storage.
- **Architecture Note**: Home Assistant entities sync to MQTT, which pushes to QuIXI via MQTT. Authentication handled by QuIXI. Spoke communicates via Ixian network to Spoke, then MQTT to Home Assistant. Only initialization needed is adding QuIXI user to Spoke.

## Current Polish Backlog

- **Onboarding polish**: Add a skip path once the wallet exists and the username is chosen so power users can jump straight into AppShell while still allowing QuIXI settings to be filled later.
- **Connection messaging**: Surface the same “Connecting to Ixian network…”/“Connected to Ixian network” phrasing from Spixi inside the settings/test flow so the Windows UX matches the established terminology.
- **Post-setup validation**: Double-check wallet storage, secure preferences, and node initialization happen reliably after onboarding so we can remove early run blockers and focus on UX improvements.