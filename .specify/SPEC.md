# Spoke - Initial Spec

Overview
- Spoke provides a UI for users to interact with the Ixian network, manage a local wallet, and optionally connect to Home Assistant via QuIXI.

Key Capabilities
- Start and stop node/network connections.
- Create / import a wallet and encrypt private keys locally.
- Display connection and sync status for Ixian and QuIXI.
- Allow user to configure QuIXI settings and test connection.

Non-Functional Requirements
- Cross-platform: primary targets include Windows (MAUI) and mobile platforms.
- Secure: use proven crypto primitives and avoid leaking private keys.
- Observable: clearly surface errors and connectivity state to the user.

Selected Scenarios
1. Onboarding: user generates wallet, optionally enters QuIXI info, app stores secure settings.
2. Reconnect: user requests a test connection; app retries with exponential backoff.
3. Background sync: app keeps minimal inventory and shows last synced block/time.

Open Questions
- Preferred secret storage across platforms (SecureStorage is used, confirm platform behaviors).
- Which tests should run headless vs. requiring a device/emulator (MAUI constraints).
