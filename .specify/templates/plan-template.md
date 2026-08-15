# Implementation Plan: [FEATURE]

**Branch**: `[###-feature-name]` | **Date**: [DATE] | **Spec**: [link]
**Input**: Feature specification from `/specs/[###-feature-name]/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

[Extract from feature spec: primary requirement + technical approach from research]

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: .NET 8.0 LTS (fixed by Constitution Principle II)  
**Primary Dependencies**: NBitcoin + BCL only; xUnit for tests (fixed by Principle II)  
**Storage**: None — nothing is persisted (Principle III)  
**Testing**: xUnit against official BIP test vectors (Principle V)  
**Target Platform**: Cross-platform console (Windows/Linux/macOS), offline execution  
**Project Type**: Interactive console application  
**Performance Goals**: [domain-specific, e.g., full report for 10 wallets under 5s]  
**Constraints**: No network calls, no file/log/clipboard writes (Principle III); no NuGet package outside the fixed stack (Principle II); Docker NOT available locally  
**Scale/Scope**: [domain-specific, e.g., up to N derived wallets, K passphrases each]

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Mark each gate PASS / FAIL / N/A. Any FAIL must either be resolved or justified in
Complexity Tracking below — **except** Principles I, III and V, which are absolute blockers and
cannot be waived (Governance).

| Gate | Principle | Check |
|---|---|---|
| Crypto primitives | I 🔒 | No hand-rolled hashes, HMAC, EC math or address encoding — NBitcoin/BCL only. BIP85 composed solely from BIP32 hardened derivation + HMAC-SHA512, isolated in one component. Randomness from `RandomNumberGenerator`; no `System.Random` anywhere near key material. |
| Stack | II | No NuGet package outside NBitcoin + xUnit and its test-execution infrastructure (`Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`, test project only); terminal UI built on `System.Console` only; no ORM, DB driver, HTTP client or telemetry SDK; no task depends on running `docker` locally. |
| No persistence / no network | III 🔒 | No writes to file, DB, log, environment variable, registry or clipboard; no network call of any kind; non-interactive `stdout` is detected and the user is warned. |
| Code conventions | IV | PascalCase types/members, `_camelCase` private fields, UPPER_CASE named constants (no magic numbers), file-scoped namespaces, nullable enabled, warnings as errors, immutable result types, `*.Core` free of `System.Console`. |
| Test vectors | V 🔒 | Every derivation path is covered by a test against the official BIP vectors, embedded in the test project; determinism asserted explicitly; no expected value adjusted to match code output. |
| Memory & output hygiene | VI | Passphrases read without echo; sensitive buffers zeroed including on error/interrupt paths; no sensitive material in exception messages; pre-generation warning and post-generation terminal-clearing guidance present. |

🔒 = non-waivable. A FAIL here blocks the plan; it cannot be moved to Complexity Tracking.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

The three-project layout is fixed by the Constitution ("Restrições Adicionais → Estrutura da
Solução"). Additional projects require a formal amendment.

```text
BtcVaultProtocol.sln
src/BtcVaultProtocol.Wallet.Core/       # Domain: entropy, mnemonics, BIP32/BIP85 derivation, multisig
├── [folders per concern, e.g. Entropy/, Mnemonics/, Derivation/, Multisig/]
└── (no System.Console usage — Principle IV)

src/BtcVaultProtocol.Wallet.Console/    # Interaction: prompts, input validation, final report
├── Program.cs                    # Entry point + top-level exception handling
└── [folders per concern, e.g. Prompts/, Flow/, Rendering/]

tests/BtcVaultProtocol.Wallet.Tests/    # xUnit: official BIP vectors + flow rules
└── [mirrors Core structure; vectors embedded, no external files]
```

Dependency flow: `Console → Core`. `Core` MUST NOT reference `Console`. `Tests` references both.

**Structure Decision**: [Document the concrete folders created under each project for this
feature. Deviating from the three-project layout requires a constitutional amendment, not a
Complexity Tracking entry.]

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**
>
> Principles I (crypto primitives), III (no persistence/network) and V (test vectors) are
> non-waivable — a violation there requires a constitutional amendment and MUST NOT be recorded
> here as a justified exception.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., extra NuGet dependency] | [current need] | [why NBitcoin + BCL insufficient] |
| [e.g., 4th project] | [current need] | [why the fixed 3-project layout insufficient] |
