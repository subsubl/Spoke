---

description: "Task list for Spoke Home Assistant Integration implementation"
---

# Tasks: Spoke Home Assistant Integration

**Input**: Design documents from `/specs/001-spoke-home-assistant-integration/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Basic unit and integration tests included per constitution requirements.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **MAUI App**: `Spoke/Spoke/` for main code, `Spoke/Spoke.Tests/` for tests
- **Core logic**: `Spoke/Spoke/Data/`, `Spoke/Spoke/Network/`
- **UI**: `Spoke/Spoke/Pages/`
- **Models**: `Spoke/Spoke/Data/Models/`

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Create .NET MAUI project structure per implementation plan
- [ ] T002 Add Ixian-Core, QuIXI dependencies to Spoke.csproj
- [ ] T003 [P] Configure .NET 10 MAUI build settings for cross-platform support
- [ ] T004 [P] Setup basic project references and solution structure

## Constitution Compliance (FOUNDATIONAL TASKS)

The Foundational phase MUST include top-level tasks that map to the
Constitution's non-negotiable items. Ensure the following tasks (or
equivalents) exist in Phase 1 or Phase 2 and are completed before user
stories proceed:

- Security: Configure secure storage for cryptographic keys and credentials
- Tests: Setup xUnit test framework and CI pipeline configuration
- Observability: Implement structured logging with Serilog
- Versioning: Document API versioning policy for Home Assistant integration
- Additional Constraints: Setup release checklists and artifact signing
- Development Workflow: Configure PR templates and issue tracking
- Styleguide and SDK Compliance: Review and align with Ixian-Core, QuIXI, Home-assistant-android patterns
- Home Assistant OS API Integration: Implement secure API client with rate limiting

These items are blocking: user story work SHOULD NOT begin until they are
addressed and marked done.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T005 [P] Create base Entity model in Spoke/Spoke/Data/Models/Entity.cs
- [X] T006 [P] Create Wallet model in Spoke/Spoke/Data/Models/Wallet.cs
- [X] T007 [P] Create QuixiConnection model in Spoke/Spoke/Data/Models/QuixiConnection.cs
- [X] T008 [P] Create HomeAssistantConfig model in Spoke/Spoke/Data/Models/HomeAssistantConfig.cs
- [X] T009 Implement secure storage service in Spoke/Spoke/Data/SecureStorageService.cs
- [X] T010 Setup dependency injection in Spoke/Spoke/MauiProgram.cs
- [X] T011 Implement basic error handling and logging infrastructure
- [X] T012 Create base test project structure in Spoke/Spoke.Tests/

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Create Secure Blockchain Identity (Priority: P1) 🎯 MVP

**Goal**: Enable users to create and manage secure blockchain-based identities for authentication

**Independent Test**: Can be fully tested by creating a wallet, verifying cryptographic operations, and confirming secure storage

### Tests for User Story 1 ⚠️

- [X] T013 [P] [US1] Unit tests for Wallet model in Spoke/Spoke.Tests/Models/WalletTests.cs
- [X] T014 [P] [US1] Unit tests for SecureStorageService in Spoke/Spoke.Tests/Services/SecureStorageServiceTests.cs

### Implementation for User Story 1

- [X] T015 [US1] Implement Ixian-Core wallet generation in Spoke/Spoke/Data/WalletService.cs
- [X] T016 [US1] Create wallet creation UI in Spoke/Spoke/Pages/CreateWalletPage.xaml
- [X] T017 [US1] Implement wallet validation and error handling
- [X] T018 [US1] Add wallet persistence with secure storage integration
- [X] T019 [US1] Create wallet management view model in Spoke/Spoke/ViewModels/WalletViewModel.cs

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Connect to Home Assistant via QuIXI (Priority: P2)

**Goal**: Establish secure connection to Home Assistant through QuIXI bridge

**Independent Test**: Can be fully tested by configuring connection settings, verifying bridge communication, and loading entity list

### Tests for User Story 2 ⚠️

- [ ] T020 [P] [US2] Unit tests for QuixiClient in Spoke/Spoke.Tests/Network/QuixiClientTests.cs
- [ ] T021 [P] [US2] Integration tests for connection setup in Spoke/Spoke.Tests/Integration/ConnectionTests.cs

### Implementation for User Story 2

- [ ] T022 [US2] Implement QuixiClient for REST API communication in Spoke/Spoke/Network/QuixiClient.cs
- [ ] T023 [US2] Create connection configuration UI in Spoke/Spoke/Pages/SettingsPage.xaml
- [ ] T024 [US2] Implement connection testing and validation
- [ ] T025 [US2] Add entity loading from QuIXI bridge
- [ ] T026 [US2] Create connection status monitoring and error handling
- [ ] T027 [US2] Implement Home Assistant WebSocket client in Spoke/Spoke/Network/HomeAssistantClient.cs

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Control Smart Home Entities (Priority: P3)

**Goal**: Provide UI for viewing and controlling Home Assistant entities

**Independent Test**: Can be fully tested by loading entities, performing control actions, and verifying state synchronization

### Tests for User Story 3 ⚠️

- [ ] T028 [P] [US3] Unit tests for EntityManager in Spoke/Spoke.Tests/Data/EntityManagerTests.cs
- [ ] T029 [P] [US3] Integration tests for entity control in Spoke/Spoke.Tests/Integration/EntityControlTests.cs

### Implementation for User Story 3

- [ ] T030 [US3] Implement EntityManager for CRUD operations in Spoke/Spoke/Data/EntityManager.cs
- [ ] T031 [US3] Create main dashboard UI in Spoke/Spoke/Pages/HomePage.xaml
- [ ] T032 [US3] Implement entity widgets (Toggle, Sensor, Gauge, Graph)
- [ ] T033 [US3] Add entity control commands and state synchronization
- [ ] T034 [US3] Create entity detail view in Spoke/Spoke/Pages/EntityDetailPage.xaml
- [ ] T035 [US3] Implement real-time state updates via WebSocket
- [ ] T036 [US3] Add entity search and filtering capabilities

**Checkpoint**: All user stories should now be independently functional

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T037 [P] Add comprehensive error handling and user feedback
- [ ] T038 [P] Implement dark/light theme support
- [ ] T039 [P] Add push notifications for entity state changes
- [ ] T040 Optimize performance and memory usage
- [ ] T041 [P] Add unit tests for remaining components
- [ ] T042 Update README and documentation
- [ ] T043 Run quickstart.md validation and testing

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-5)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Phase 6)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - May integrate with US1 but should be independently testable
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - May integrate with US1/US2 but should be independently testable

### Within Each User Story

- Tests (if included) MUST be written and FAIL before implementation
- Models before services
- Services before UI
- Core functionality before advanced features
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- All tests for a user story marked [P] can run in parallel
- Models within a story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together:
Task: "Unit tests for Wallet model in Spoke/Spoke.Tests/Models/WalletTests.cs"
Task: "Unit tests for SecureStorageService in Spoke/Spoke.Tests/Services/SecureStorageServiceTests.cs"

# Launch all models for User Story 1 together:
Task: "Create base Entity model in Spoke/Spoke/Data/Models/Entity.cs"
Task: "Create Wallet model in Spoke/Spoke/Data/Models/Wallet.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1
   - Developer B: User Story 2
   - Developer C: User Story 3
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
