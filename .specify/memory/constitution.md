# <!--
# Sync Impact Report
# Version change: (none / template) -> 0.1.0
# Modified principles:
# - [PRINCIPLE_1_NAME] -> Safety & Security
# - [PRINCIPLE_2_NAME] -> Test-First
# - [PRINCIPLE_3_NAME] -> Observability & Debuggability
# - [PRINCIPLE_4_NAME] -> API Stability & Versioning
# - [PRINCIPLE_5_NAME] -> Simplicity & Minimal Surface Area
# Added sections:
# - Additional Constraints
# - Development Workflow
# Removed sections:
# - none
# Templates requiring updates:
# - d:\\Spoke\\.specify\\templates\\plan-template.md ✅ updated
# - d:\\Spoke\\.specify\\templates\\spec-template.md ✅ updated
# - d:\\Spoke\\.specify\\templates\\tasks-template.md ✅ updated
# - d:\\Spoke\\.specify\\templates\\commands\\* ⚠ pending (directory missing)
# Follow-up TODOs:
# - TODO(RATIFICATION_DATE): Provide the original adoption/ratification date.
# -->

# Spoke Project Constitution
<!-- Spoke Project governing constitution for development, testing, and
	release practices. This file captures non-negotiable principles that
	all contributors MUST follow. -->

## Core Principles

### Safety & Security (NON-NEGOTIABLE)
All code that touches networking, cryptography, identity, or persistent storage
MUST follow secure-by-default rules. Contributors MUST not check secrets into
the repository. Security controls MUST include threat modelling for
significant features, dependency vulnerability scanning, and CI checks for
known insecure patterns. Security findings that affect consensus or network
integrity MUST block merges until mitigations are documented and reviewed.

### Test-First (NON-NEGOTIABLE)
All new behavior MUST be accompanied by automated tests. Tests MUST be
written before or alongside implementation and include unit tests for logic,
integration tests for cross-component behavior, and contract tests for public
APIs or network protocols. CI pipelines MUST run the test suite and prevent
merges on failures.

### Observability & Debuggability
All services and libraries MUST expose structured logs, metrics, and where
applicable traces. Logs MUST include contextual identifiers (request IDs,
peer IDs) to make distributed debugging feasible. Design plans MUST include an
observability section describing how behavior will be measured and diagnosed.

### API Stability & Versioning
Public APIs and on-wire protocols MUST follow semantic versioning. Any
backwards-incompatible change to public contracts or network protocols
requires a MAJOR version bump and a documented migration plan. MINOR
versions add functionality in a backwards-compatible way; PATCH versions
are for documentation, typo fixes, and non-semantic clarifications.

### Simplicity & Minimal Surface Area
Designs SHOULD prefer minimal, well-documented interfaces. Avoid premature
generalization: add complexity only when there is a demonstrated need and a
testing strategy. Each public surface MUST have clear ownership and tests.

## Additional Constraints
Security, correctness, and reproducibility take precedence over convenience.
All releases that include cryptographic code or network protocol changes MUST
include a review checklist and signed release artifacts. Contributors MUST
follow the repository's CI/CD gating policies and publishing rules. Avoid
committing environment-specific configuration; use documented environment
variables and secrets management instead.

## Development Workflow
All work MUST be tracked in an issue or spec that describes the user-facing
behavior, tests, and migration concerns. Pull requests MUST:
- Reference the related issue/spec and include a short summary of changes.
- Include tests that demonstrate the new behavior (unit + integration where
	applicable).
- Pass all CI checks including linters, test suite, and security scans.
At least one approver other than the author MUST review and approve changes
for non-trivial areas (cryptography, consensus, networking). Merge of
governance-affecting changes (e.g., protocol rules) MUST be accompanied by a
migration plan and explicit maintainer sign-off.

## Governance
<!-- Example: Constitution supersedes all other practices; Amendments require documentation, approval, migration plan -->

This constitution defines the baseline rules for development, review, and
release. Amendments to the constitution itself MUST be proposed as a pull
request that includes a rationale, tests (where applicable), and a migration
or communication plan. Amendments are categorized and versioned according to
the Versioning Policy below.

Governance expectations:
- Day-to-day development decisions SHOULD follow this constitution.
- Maintainers and code owners are responsible for enforcing the principles in
	reviews and CI gates.
- Exceptions to non-negotiable items (e.g., Security/Test-First) MUST be
	documented in the PR and explicitly approved by project maintainers.

**Versioning Policy**:
- MAJOR: Backwards-incompatible governance or principle removals/definitions
	(requires broad maintainer consensus and migration plan).
- MINOR: Addition of a new principle or material expansion of an existing
	principle (requires review and documented impact analysis).
- PATCH: Wording clarifications, typos, or non-semantic refinements.

**Compliance Review**: All release PRs targeting components that affect
security, consensus, or network behavior MUST include a compliance checklist
that maps to the Core Principles above. The checklist MUST be included in the
PR description and verified by reviewers before merge.

**Version**: 0.1.0 | **Ratified**: TODO(RATIFICATION_DATE): provide original adoption date | **Last Amended**: 2025-12-01
<!-- Example: Version: 2.1.1 | Ratified: 2025-06-13 | Last Amended: 2025-07-16 -->
