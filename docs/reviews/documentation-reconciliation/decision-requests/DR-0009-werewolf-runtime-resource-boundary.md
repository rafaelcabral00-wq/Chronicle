---
id: DR-0009
title: Werewolf Runtime Resource Boundary
status: accepted
accepted_option: Option A
accepted_date: 2026-08-07
---

# DR-0009: Werewolf Runtime Resource Boundary

## Decision Record

Status: accepted.

Accepted option: Option A.

Effective date: 2026-08-07.

Decision:

This document records a new explicit Chronicle project decision. It is not a historical approval from the prototype review process. The prototype `resources.json` marks `runtimeCapability.spendSupported: false` and `runtimeCapability.restoreSupported: false`. The prototype `set-resource-rating.json` is scoped to `CharacterCreation` and does not authorize post-completion mutation. DR-0005 explicitly blocks IMPLEMENT-019B and all Renown behavior. No pre-existing approved artifact authorizes the executable behavior defined below.

Chronicle hereby approves the following runtime resource boundary for PLAYABLE-002:

- Completed-character creation snapshots remain immutable historical completion evidence.
- A separate mutable runtime character state carries current gameplay values.
- Mutable runtime values are the `.current` values of:
  - `character.resource.rage`
  - `character.resource.gnosis`
  - `character.resource.willpower`
- `.permanent` values remain unchanged by PLAYABLE-002.
- Runtime mutation is deterministic and Rule Set-owned.
- Canonical operations:
  - `character-runtime.spend-resource`
  - `character-runtime.recover-resource`
- Both operations require:
  - completed character state binding;
  - resource identifier;
  - positive integer amount;
  - expected runtime-state version (optimistic concurrency token).
- Spending cannot reduce current below zero.
- Recovery cannot increase current above permanent.
- Invalid transitions are rejected rather than silently clamped.
- Exact resource-specific narrative/trigger rules are outside scope.
- No automatic regeneration or time-based recovery.
- No Renown mutation.
- No persistence side effect.
- Rule Set returns a new immutable runtime state suitable for Chronicle persistence later.

## Authority Audit Summary

### Approved Artifacts

- **DR-0005**: Deterministic Resource initialization from Race/Auspice/Tribe. Current initialized equal to permanent. Renown blocked under IMPLEMENT-019B. Operation boundary: `character-creation.initialize-resources-and-rank`.
- **DR-0007**: Completion produces immutable `WerewolfCharacterSnapshot`. Completion does not persist through Chronicle infrastructure in this work package. Rule Set returns deterministic snapshot data.
- **DR-0008**: Runtime operation pattern established under `generic-dice` capability. `RuleSetRuntimeRegistry` dispatch. Chronicle.Application owns orchestration sequence.
- **Prototype `resources.json`**: Structural constraints present as `candidate` status:
  - `character.resource.rage.current` minimum `0`, maximum source `permanent`
  - `character.resource.gnosis.current` minimum `0`, maximum source `permanent`
  - `character.resource.willpower.current` minimum `0`, maximum source `permanent`
  - `character.resource.rage.permanent` minimum `0`, maximum `10` (candidate)
  - `character.resource.gnosis.permanent` minimum `0`, maximum `10` (candidate)
  - `character.resource.willpower.permanent` minimum `0`, maximum `10` (candidate)
  - Validation rules: `character.resources.current-not-negative`, `character.resources.current-not-above-permanent`
- **Prototype `set-resource-rating.json`**: Creation-time resource mutation only. Runtime spending explicitly marked `unavailable`. Post-completion operations listed but marked `unavailable`.

### Contradictions Found

None. The proposed spend/recover bounds do not contradict any approved authority. They preserve already-materialized structural constraints and extend them to runtime mutation within a new explicit decision boundary.

## Ownership Boundary

- **Rule Set owns**: resource identifier validation, spend/recover transition validation, current/permanent bound enforcement, deterministic state transition, canonical findings, new immutable runtime state.
- **Chronicle Application owns**: orchestration sequence, runtime state pass-through, result delivery.
- **Chronicle Core owns**: persistence, aggregate activation, transaction coordination (future, not PLAYABLE-002).
- **Rule Set must not**: call database, filesystem, network, clock, RNG, provider, or persistence APIs; mutate permanent values; mutate Renown; narrate prose; call Narrative Intelligence.
- **Narrative Intelligence must not**: own rules, randomness, canonical state, persistence, or mechanical outcome selection.

## Creation Snapshot vs Runtime State

- **Creation snapshot** (`WerewolfCharacterSnapshot`): immutable historical completion evidence. Produced by `character-creation.complete-character`. Never mutated by PLAYABLE-002.
- **Runtime state** (`WerewolfRuntimeCharacterState`): mutable gameplay state derived from and bound to the creation snapshot. Carries current/permanent Resources and a deterministic runtime state version.
- `DraftVersion` from creation is not overloaded for post-creation gameplay. Runtime state version is a separate integer incremented exactly once per successful transition.
- Chronicle does not persist runtime state in PLAYABLE-002.

## Runtime State Shape

```csharp
public sealed record WerewolfRuntimeCharacterState(
    string PackageId,
    string PackageVersion,
    string DraftId,
    int RuntimeStateVersion,
    IReadOnlyDictionary<string, string> PackageBinding,
    int RagePermanent,
    int RageCurrent,
    int GnosisPermanent,
    int GnosisCurrent,
    int WillpowerPermanent,
    int WillpowerCurrent);
```

Properties:
- `PackageId`, `PackageVersion`: bind runtime state to the originating Rule Set package.
- `DraftId`: references the completed creation draft identity.
- `RuntimeStateVersion`: monotonically increasing integer, starts at `1` after initialization from snapshot.
- `PackageBinding`: copied from `WerewolfCharacterSnapshot.PackageBinding` (contains `packageId`, `packageVersion`, `declaredReleaseScope`, `contractVersion`).
- `RagePermanent`, `GnosisPermanent`, `WillpowerPermanent`: immutable after creation.
- `RageCurrent`, `GnosisCurrent`, `WillpowerCurrent`: mutable through approved operations.

## Approved Operations

### character-runtime.spend-resource

Inputs:
- `requestId`: correlation identifier
- `currentState`: JSON serialized `WerewolfRuntimeCharacterState`
- `expectedRuntimeStateVersion`: optimistic concurrency token
- `resourceId`: resource identifier (`character.resource.rage`, `character.resource.gnosis`, `character.resource.willpower`)
- `amount`: positive integer

Outputs:
- `requestId`: echoed correlation identifier
- `newState`: JSON serialized updated `WerewolfRuntimeCharacterState`
- `newRuntimeStateVersion`: incremented version
- `succeeded`: finding code (`ResourceSpendSucceeded` or error code)
- `resourceId`: echoed resource identifier
- `amount`: echoed amount
- `previousCurrent`: current value before transition
- `newCurrent`: current value after transition
- `previousPermanent`: permanent value before transition (unchanged)
- `newPermanent`: permanent value after transition (unchanged)

Validation:
- State present and well-formed
- Package binding valid and matches expected package
- Draft bound to completed character
- Expected runtime state version matches current version
- Resource identifier recognized
- Amount is positive integer
- Current value >= amount (sufficient resource)

Transition:
- `newCurrent = current - amount`
- `newPermanent = permanent` (unchanged)
- `runtimeStateVersion += 1`
- All other state preserved

### character-runtime.recover-resource

Inputs: identical to spend-resource.

Outputs: identical to spend-resource.

Validation:
- State present and well-formed
- Package binding valid
- Draft bound to completed character
- Expected runtime state version matches current version
- Resource identifier recognized
- Amount is positive integer
- Current value + amount <= permanent (recovery does not exceed permanent)

Transition:
- `newCurrent = current + amount`
- `newPermanent = permanent` (unchanged)
- `runtimeStateVersion += 1`
- All other state preserved

## Validation Codes

| Code | Severity | Description |
|------|----------|-------------|
| `MissingState` | Error | Runtime state is null or malformed. |
| `InvalidPackageBinding` | Error | Package binding is missing or does not match expected package. |
| `CharacterNotCompleted` | Error | Draft is not bound to a completed character. |
| `StaleRuntimeStateVersion` | Error | Expected runtime state version does not match current version. |
| `UnknownResource` | Error | Resource identifier is not recognized. |
| `MalformedResourceIdentifier` | Error | Resource identifier is null, empty, or malformed. |
| `AmountMissingOrZero` | Error | Amount is missing, zero, or not a positive integer. |
| `AmountNegative` | Error | Amount is negative. |
| `InsufficientCurrentValue` | Error | Spend amount exceeds current value. |
| `RecoveryExceedsPermanent` | Error | Recovery would raise current above permanent. |
| `InvalidSourceCurrentAbovePermanent` | Error | Source state current exceeds permanent (invalid state). |
| `ResourceSpendSucceeded` | Information | Spend transition succeeded. |
| `ResourceRecoverSucceeded` | Information | Recover transition succeeded. |

## Explicitly Non-Blocking for PLAYABLE-002

- Renown (unresolved under IMPLEMENT-019B)
- Permanent value mutation
- Automatic regeneration or time-based recovery
- Resource-specific narrative/trigger rules
- Combat, damage, Frenzy, Gifts runtime, advancement
- Session persistence
- Narrative Intelligence integration
- Freebie-points interaction with runtime Resources
- Difficulty, dice, or action resolution

## Exclusions

- Database persistence
- Chronicle aggregate activation
- Runtime state persistence
- Timestamp generation
- Event emission
- Combat, damage, or opposed rolls
- Extended or resisted tests
- Dramatic systems or task categories
- Narrative Intelligence integration
- OpenAI/Ollama/network/database dependencies

## Governing Authority

- `docs/reviews/documentation-reconciliation/decision-requests/DR-0005-initial-resources-rank-and-renown-boundary.md` (Resource initialization, Renown blocked)
- `docs/reviews/documentation-reconciliation/decision-requests/DR-0007-werewolf-character-completion-boundary.md` (immutable snapshot, completion boundary)
- `docs/reviews/documentation-reconciliation/decision-requests/DR-0008-action-resolution-and-randomness-boundary.md` (runtime operation pattern, orchestration boundary)
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/character-model/fields/resources.json` (structural constraints: current >= 0, current <= permanent)
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/character-creation/set-resource-rating.json` (creation-time boundary, runtime spending unavailable)
- `docs/specs/SPEC-0001-rule-set-package-artifact-model-and-extraction-contract.md` (materialization role authority)

## Governance

- New decision request: required (this document).
- Existing decision set reopened: no.
- Decision set artifact: this document.
- Review record artifact: to be attached after playable-runtime review advances.
- Decision boundary: approves only PLAYABLE-002 runtime resource spend/recover operations and mutable runtime state boundary; does not approve Renown, permanent mutation, persistence, activation, narrative triggers, or Chronicle-owned state storage.
