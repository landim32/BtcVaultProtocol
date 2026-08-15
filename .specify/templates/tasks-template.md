---

description: "Task list template for feature implementation"
---

# Tasks: [FEATURE NAME]

**Input**: Design documents from `/specs/[###-feature-name]/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests against the official BIP test vectors are MANDATORY for every derivation path
(Constitution Principle V, non-waivable). Other test categories remain optional and are included
only when explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

The three-project layout is fixed by the Constitution ("Restrições Adicionais → Estrutura da
Solução"):

- **Domain (entropy, mnemonics, BIP32/BIP85 derivation, multisig)**: `src/BtcVaultProtocol.Wallet.Core/`
- **Console interaction (prompts, input validation, final report)**: `src/BtcVaultProtocol.Wallet.Console/`
- **Tests (official BIP vectors + flow rules)**: `tests/BtcVaultProtocol.Wallet.Tests/`
- Dependency flow: `Console → Core`. `Core` MUST NOT reference `Console` and MUST NOT use
  `System.Console` (Principle IV).
- Adjust folder names inside each project to the concrete structure declared in plan.md

<!-- 
  ============================================================================
  IMPORTANT: The tasks below are SAMPLE TASKS for illustration purposes only.
  
  The /speckit.tasks command MUST replace these with actual tasks based on:
  - User stories from spec.md (with their priorities P1, P2, P3...)
  - Feature requirements from plan.md
  - Entities from data-model.md
  - Endpoints from contracts/
  
  Tasks MUST be organized by user story so each story can be:
  - Implemented independently
  - Tested independently
  - Delivered as an MVP increment
  
  DO NOT keep these sample tasks in the generated tasks.md file.
  ============================================================================
-->

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Create the three-project solution: `src/BtcVaultProtocol.Wallet.Core/`, `src/BtcVaultProtocol.Wallet.Console/`, `tests/BtcVaultProtocol.Wallet.Tests/` with the `Console → Core` reference
- [ ] T002 Add NBitcoin to `Core`, and xUnit + `Microsoft.NET.Test.Sdk` + `xunit.runner.visualstudio` to `Tests` — no other NuGet package (Principle II)
- [ ] T003 [P] Enable `<Nullable>enable</Nullable>` and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` in all projects (Principle IV)

> **Constitution gate**: no NuGet package outside NBitcoin + xUnit, no ORM/HTTP client/telemetry
> SDK, and no task may require running `docker` locally (Principle II).

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

Examples of foundational tasks (adjust based on your project):

- [ ] T004 Implement the CSPRNG entropy source over `RandomNumberGenerator` in `Core/` (Principle I)
- [ ] T005 [P] Embed the official BIP39/BIP32/BIP85 test vectors in `tests/BtcVaultProtocol.Wallet.Tests/` (Principle V)
- [ ] T006 [P] Implement no-echo passphrase reading and sensitive-buffer zeroing helpers in `Console/`/`Core/` (Principle VI)
- [ ] T007 Implement the interactive prompt loop with input validation and re-ask on invalid input in `Console/` ("Restrições Adicionais → Tratamento de Erros")
- [ ] T008 Implement top-level exception handling in `Program.cs`: generic message, sensitive-memory cleanup, non-zero exit code, no stack trace or sensitive fragment leaked
- [ ] T009 [P] Implement non-interactive `stdout` detection with a redirection warning (Principle III)
- [ ] T010 [P] Implement the pre-generation best-practices warning and the post-generation terminal-clearing guidance (Principle VI)

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - [Title] (Priority: P1) 🎯 MVP

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 1 ⚠️

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation.**
> Official BIP vector tests are mandatory for any derivation touched by this story (Principle V).

- [ ] T011 [P] [US1] Official [BIP-XX] vector test for [derivation] in tests/BtcVaultProtocol.Wallet.Tests/[Area]/[Name]Tests.cs
- [ ] T012 [P] [US1] Determinism test: same input always yields the same output, in tests/BtcVaultProtocol.Wallet.Tests/[Area]/[Name]DeterminismTests.cs

### Implementation for User Story 1

- [ ] T013 [P] [US1] Create [immutable result type] in src/BtcVaultProtocol.Wallet.Core/[Area]/[Type].cs
- [ ] T014 [US1] Implement [derivation/generation] in src/BtcVaultProtocol.Wallet.Core/[Area]/[Component].cs using NBitcoin primitives only (depends on T013)
- [ ] T015 [US1] Implement the prompts and input validation for this step in src/BtcVaultProtocol.Wallet.Console/[Area]/[Prompt].cs
- [ ] T016 [US1] Implement the report rendering for this step in src/BtcVaultProtocol.Wallet.Console/[Area]/[Renderer].cs
- [ ] T017 [US1] Zero the sensitive buffers used by this story, including on error paths (Principle VI)

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - [Title] (Priority: P2)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 2 ⚠️

- [ ] T018 [P] [US2] Official [BIP-XX] vector test for [derivation] in tests/BtcVaultProtocol.Wallet.Tests/[Area]/[Name]Tests.cs
- [ ] T019 [P] [US2] Edge-case/validation test for [rule] in tests/BtcVaultProtocol.Wallet.Tests/[Area]/[Name]Tests.cs

### Implementation for User Story 2

- [ ] T020 [P] [US2] Create [immutable result type] in src/BtcVaultProtocol.Wallet.Core/[Area]/[Type].cs
- [ ] T021 [US2] Implement [derivation/generation] in src/BtcVaultProtocol.Wallet.Core/[Area]/[Component].cs
- [ ] T022 [US2] Implement the prompts, validation and report section in src/BtcVaultProtocol.Wallet.Console/[Area]/
- [ ] T023 [US2] Integrate with User Story 1 components (if needed)

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - [Title] (Priority: P3)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 3 ⚠️

- [ ] T024 [P] [US3] Official [BIP-XX] vector test for [derivation] in tests/BtcVaultProtocol.Wallet.Tests/[Area]/[Name]Tests.cs
- [ ] T025 [P] [US3] Edge-case/validation test for [rule] in tests/BtcVaultProtocol.Wallet.Tests/[Area]/[Name]Tests.cs

### Implementation for User Story 3

- [ ] T026 [P] [US3] Create [immutable result type] in src/BtcVaultProtocol.Wallet.Core/[Area]/[Type].cs
- [ ] T027 [US3] Implement [derivation/generation] in src/BtcVaultProtocol.Wallet.Core/[Area]/[Component].cs
- [ ] T028 [US3] Implement the prompts, validation and report section in src/BtcVaultProtocol.Wallet.Console/[Area]/

**Checkpoint**: All user stories should now be independently functional

---

[Add more user story phases as needed, following the same pattern]

---

## Phase N: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] TXXX [P] Documentation updates in docs/
- [ ] TXXX Code cleanup and refactoring (named constants, no magic numbers — Principle IV)
- [ ] TXXX Audit: no file/log/clipboard write and no network call anywhere in the solution (Principle III)
- [ ] TXXX Audit: every sensitive buffer is zeroed, including on error and interrupt paths (Principle VI)
- [ ] TXXX Audit: no sensitive material reaches exception messages or stack traces
- [ ] TXXX [P] Additional unit tests in tests/BtcVaultProtocol.Wallet.Tests/
- [ ] TXXX Run quickstart.md validation

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - May integrate with US1 but should be independently testable
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - May integrate with US1/US2 but should be independently testable

### Within Each User Story

- Vector tests MUST be written and FAIL before the derivation they cover is implemented
- Domain types before derivation components
- `Core` derivation before `Console` prompts and rendering
- Core implementation before integration
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
Task: "Official BIP-XX vector test for [derivation] in tests/BtcVaultProtocol.Wallet.Tests/[Area]/[Name]Tests.cs"
Task: "Determinism test for [derivation] in tests/BtcVaultProtocol.Wallet.Tests/[Area]/[Name]DeterminismTests.cs"

# Launch all domain types for User Story 1 together:
Task: "Create [Type1] in src/BtcVaultProtocol.Wallet.Core/[Area]/[Type1].cs"
Task: "Create [Type2] in src/BtcVaultProtocol.Wallet.Core/[Area]/[Type2].cs"
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
