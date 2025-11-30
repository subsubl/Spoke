# Gemini Coder Notes: Spoke Repository

This document contains my notes and understanding of the Spoke repository and the broader Ixian ecosystem.

## Overall Architecture

The system is a multi-layered, decentralized application ecosystem.

1.  **Spoke (Frontend):** A .NET MAUI application that acts as a secure, decentralized smart home controller for Home Assistant.
2.  **QuIXI (Middleware):** A gateway service that bridges the Ixian network with other protocols. Spoke communicates with QuIXI via a REST API. QuIXI, in turn, communicates with a Home Assistant instance via an MQTT broker.
3.  **Spixi (Identity):** A separate communication application that also provides wallet and identity management features. Spoke includes the Spixi project to leverage its wallet and identity code.
4.  **Ixian-Core (Foundation):** The core SDK providing the fundamental building blocks for the entire platform, including post-quantum cryptography, P2P networking, and blockchain logic.

**Data Flow:** `Spoke <-> QuIXI <-> Home Assistant`

## Key Technologies

*   **.NET MAUI:** For the Spoke cross-platform application.
*   **C#:** The primary language for Ixian-Core, Spoke, Spixi, and QuIXI.
*   **Ixian DLT:** A custom blockchain for consensus and value transfer.
*   **Ixian S2:** A peer-to-peer streaming and communication layer.
*   **Post-Quantum Cryptography:** Hybrid cryptographic schemes for future-proof security.

## Sub-Projects

*   [Spoke](./Spoke/gemini.md): The main application in this repository.
*   [Ixian-Core](./Ixian-Core/gemini.md): The foundational SDK.
*   [QuIXI](./QuIXI/gemini.md): The integration gateway.
*   [Spixi](./Spixi/gemini.md): The identity and wallet provider.
*   [Ixian-Docs](./Ixian-Docs/gemini.md): The documentation project.
