---

description: "Task list for feature 'main' (scaffold + validation)"
---

# Tasks: main

**Input**: plan.md, research.md
**Prerequisites**: plan.md (required), spec.md (required)

## Phase 1: Setup (Shared Infrastructure)

+ [x] T001 Create `specs/main/` and scaffold `plan.md` (scaffolded)
+ [x] T002 Verify `global.json` pins SDK `10.0.100` (d:\Spoke\Spoke\global.json)
+ [x] T003 [P] Add CI step: `dotnet --version`, `dotnet workload restore`, `dotnet restore` for `SPOKE.sln` -> .github/workflows/ci-dotnet-maui.yml
+ [x] T004 [P] Add `specs/main/quickstart.md` with developer setup and workload install steps

## Phase 2: Foundational (Blocking Prerequisites)

- [ ] T005 Setup dependency vulnerability scanning (Dependabot or CI step)
- [ ] T006 [P] Add secrets & configuration guidance in `docs/SECURITY.md`
- [ ] T007 [P] Configure CI test matrix and platform runners for net10.0 builds

## Phase 3: US1 - Research & Plan (P1)

- [ ] T008 [US1] Create `specs/main/research.md` listing platform SDKs and library compatibility
- [ ] T009 [US1] Populate `plan.md` Technical Context and Constitution Compliance checklist

## Phase 4: US2 - Design & Contracts (P2)

- [ ] T010 [US2] Create `specs/main/data-model.md` (if applicable)
- [ ] T011 [US2] Create `specs/main/contracts/openapi.yaml` (if applicable)
- [ ] T012 [US2] Create `specs/main/quickstart.md`

## Phase 5: US3 - CI, Tests & Implementation (P3)

- [ ] T013 [US3] Add `.github/workflows/ci-dotnet-maui.yml` implementing matrix build/test and workload restore
- [ ] T014 [US3] Add unit/integration test templates and update test projects to target net10.0
- [ ] T015 [US3] Add contract tests to validate API contracts

## Final

- [ ] T016 Generate `specs/main/tasks.md` from template and map tasks to stories
- [ ] T017 Create feature branch `feature/main-plan`, commit specs/CI changes, and open PR with Sync Impact Report
- [ ] T018 Replace `TODO(RATIFICATION_DATE)` in `.specify/memory/constitution.md` with agreed date
