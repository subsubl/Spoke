# Spoke Project Constitution

## Purpose
Define the intent and high-level constraints for the Spoke application.

## Goals
- Provide a lightweight UI for interacting with the Ixian network.
- Support Home Assistant integration via QuIXI without coupling core networking.
- Secure local wallet generation, storage, and recovery.
- Be testable, auditable, and suitable for community contributions.

## Boundaries
- The app connects to the Ixian network for blockchain interactions.
- Integrations (QuIXI/Home Assistant) must be pluggable adapters.

## Decision Drivers
- Security: private keys must be encrypted at rest.
- Simplicity: avoid unnecessary network coupling between modules.
- Observability: surface clear connection and sync states in UI.

## Contributors
- Core maintainers: subsubl and project contributors.

## Review
- This constitution may be updated through normal PR workflow.
