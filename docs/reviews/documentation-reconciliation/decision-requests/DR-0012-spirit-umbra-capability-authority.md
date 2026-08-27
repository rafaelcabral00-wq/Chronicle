---
id: DR-0012
title: Spirit-Umbra Capability Authority
status: accepted
accepted_option: Option A
accepted_date: 2026-08-27
decision_authority: human
blocks: Spirit/Umbra S2 runtime operation registration
---

# DR-0012: Spirit-Umbra Capability Authority

## Decision Record

Status: accepted.

Accepted option: Option A.

Effective date: 2026-08-27.

Decision authority: Human decision recorded 2026-08-27.

## Decision

This document records an explicit human decision approving `spirit-umbra` as a new official Werewolf package/runtime capability.

### Rationale

The S2 implementation requires 10 runtime operations that form a coherent Spirit/Umbra runtime capability:

1. `spirit-umbra.initialize-spirit`
2. `spirit-umbra.evaluate-crossing`
3. `spirit-umbra.compute-movement-speed`
4. `spirit-umbra.evaluate-detection`
5. `spirit-umbra.evaluate-materialization`
6. `spirit-umbra.spend-essence`
7. `spirit-umbra.execute-charm`
8. `spirit-umbra.evaluate-command`
9. `spirit-umbra.evaluate-possession`
10. `spirit-umbra.apply-spirit-damage`

These operations cannot reuse any of the 13 existing capabilities:

| Capability | Reason for Rejection |
|---|---|
| action-resolution | Action test resolution, not Spirit entity operations |
| character-completion | Character completion validation |
| character-creation | Character creation operations |
| character-model | Character model definitions |
| character-sheet | Character sheet schema |
| character-validation | Character validation rules |
| combat | Combat mechanics, not Spirit mechanics |
| fixture-driven-tests | Test fixture infrastructure |
| frenzy | Frenzy mechanics |
| generic-dice | Dice pool interpretation |
| post-creation-character-operations | Character resource operations (spend/recover Willpower/Rage/Gnosis, apply/recover damage). Spirit operations involve Spirit entities, not character resources. Overloading this capability would violate its character-specific contract. |
| runtime-gift-activation | Gift activation, not Spirit mechanics |
| runtime-gift-execution | Gift execution, not Spirit mechanics |

The RITE-WAVE-A precedent (RULESET-COMPLETION-RITE-WAVE-A.md) shows that when `rite-runtime` was not in the manifest, the fix was to CHANGE the capability to an existing one. However, Rites could semantically fit `post-creation-character-operations` because Rites are character operations. Spirit operations CANNOT fit any existing capability because they involve Spirit entity mechanics distinct from character mechanics.

## Accepted Model: Option A — New Capability `spirit-umbra`

**Label:** New partial-executable capability for Spirit/Umbra runtime mechanics

### Semantics

- Capability key: `spirit-umbra`
- Status: `partial-executable`
- Scope: Deterministic Werewolf Spirit/Umbra mechanics and Spirit runtime-state transitions

### Authorized Mechanics

- Spirit runtime initialization/state transitions
- Gauntlet crossing evaluation
- Umbra movement calculation
- Spirit detection
- Spirit materialization evaluation
- Essence operations
- Generic Spirit Charm invocation framework
- Spirit command mechanics
- Spirit possession mechanics
- Spirit damage mechanics

### Explicit Exclusions

This capability does NOT imply support for:
- All Umbra mechanics (full domain)
- S3 Gift integrations
- S4 Rite integrations
- S5 world/lifecycle/source-gap mechanics
- Chronicle world ownership
- Scene/location persistence
- Pack/Totem lifecycle
- Generic Spirit AI

### Package Semantics

- `spirit-umbra` in capabilities: a partial executable runtime surface exists
- `umbra` remains in excludedMechanics: the complete domain is not yet declared supported

This follows the existing Rites precedent where `rites` remains in excludedMechanics despite Rite Wave A being implemented.

## Consequences

- The 10 S2 operations have a valid capability authority
- Runtime registration succeeds without fabrication
- The capability is explicitly partial-executable, not full-domain
- Future S3/S4/S5 work requires separate capability decisions

## Impact

- Affects: `werewolf.package-manifest.json`, `WerewolfReferenceRuntime.cs`, `WerewolfSpiritMechanicServices.cs`
- Affects domains: Spirit/Umbra runtime mechanics
- Unblocks: Spirit/Umbra S2 runtime operation registration
- Does NOT affect: S3 Gift integrations, S4 Rite integrations, S5 mechanics

## Governance

- New decision request: this document.
- Existing decision set reopened: no.
- Decision set artifacts: DR-0012.
- Review record artifact: Spirit/Umbra S2 implementation validated.
