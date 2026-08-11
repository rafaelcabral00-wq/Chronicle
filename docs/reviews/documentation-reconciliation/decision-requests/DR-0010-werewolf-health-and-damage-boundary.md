---
id: DR-0010
title: Werewolf Health and Damage Boundary
status: accepted
accepted_option: Option A
accepted_date: 2026-08-11
---

# DR-0010: Werewolf Health and Damage Boundary

## Decision Record

Status: accepted.

Accepted option: Option A.

Effective date: 2026-08-11.

Decision:

This document records a new explicit Chronicle project decision. It is not a historical approval from the prototype review process. EXTRACTION-0003 explicitly defers "damage", "soak", "Health resolution", "regeneration", and "Frenzy" from Slice 001. EXTRACTION-0004 contains no health/damage ambiguities because the source material was not extracted for those mechanics. The prototype `resources.json` and `set-resource-rating.json` contain no health or damage fields. No pre-existing approved artifact authorizes the executable behavior that would be required for PLAYABLE-003.

**PLAYABLE-003 cannot proceed without resolving the following source ambiguities through dedicated extraction and semantic review:**

1. Health-track structure (number and names/order of health levels)
2. Damage category structure (bashing/lethal/aggravated or alternative categories)
3. Mixed damage ordering and conversion rules
4. Overflow behavior when damage exceeds health track capacity
5. Healing/removal priority among mixed damage types
6. Incapacitation/death semantics and boundaries
7. Wound penalty derivation from current health state

Until these are resolved, Chronicle must not:
- invent a health-track structure from general World of Darkness knowledge;
- hard-code damage categories without source authority;
- silently normalize `.permanent` health identifiers as `.current` mutations;
- assign wound-penalty, incapacity, or death semantics to unresolved state.

## Authority Audit Summary

### Approved Artifacts Inspected

- **EXTRACTION-0003**: Character Creation Vertical Slice. Explicitly defers: `damage`, `soak`, `Health resolution`, `regeneration`, `Frenzy`, `Rites`, `Umbra`, `spirits`, `Totem group aggregation`, `experience progression`, `Renown advancement`, `antagonists`, `pack creation`.
- **EXTRACTION-0004**: Ambiguities and Conflicts. Contains 18 ambiguities (A-001 through A-018). None relate to health, damage, wound penalties, incapacitation, death, or damage categories.
- **EXTRACTION-0005**: Contract Findings. Does not validate any health/damage generic contract.
- **DR-0005**: Resource initialization boundary. Blocks IMPLEMENT-019B and all Renown behavior. Does not mention health/damage.
- **DR-0007**: Character completion boundary. Defines immutable `WerewolfCharacterSnapshot`. Does not mention health/damage.
- **DR-0008**: Action resolution boundary. Defines `generic-dice` capability for Attribute+Ability tests. Does not mention damage or health.
- **DR-0009**: Runtime resource boundary. Defines spend/recover for Rage/Gnosis/Willpower. Explicitly defers combat, damage, Frenzy.
- **Prototype `resources.json`**: Structural constraints for Rage/Gnosis/Willpower only. No health fields.
- **Prototype `set-resource-rating.json`**: Creation-time resource mutation only. No health/damage operations.
- **Current package metadata**: No health-related capabilities, operations, or fields. `post-creation-character-operations` capability covers only resource transitions.
- **Current runtime state**: `WerewolfRuntimeCharacterState` carries only Rage/Gnosis/Willpower current/permanent values and version. No health sub-state.

### Contradictions Found

None. The absence of health/damage authority is consistent across all inspected artifacts. There is no approved source material that would authorize PLAYABLE-003 to freeze any health-track or damage-category semantics.

## Stop Condition

PLAYABLE-003 stops at Phase 2 (Decision Boundary). The following must be resolved before implementation can proceed:

| Unresolved Item | Why Required | Current Status |
|-----------------|--------------|----------------|
| Health-track structure | Cannot represent health state without knowing track shape | Not extracted |
| Damage category identifiers | Cannot apply/recover damage without canonical type keys | Not extracted |
| Mixed damage ordering | Cannot heal without knowing which damage type to remove first | Not extracted |
| Overflow behavior | Cannot validate amount without knowing track capacity | Not extracted |
| Healing-removal priority | Cannot implement recover-damage without priority rules | Not extracted |
| Wound penalty derivation | Cannot expose derived state without unambiguous source | Not extracted |
| Incapacitation/death semantics | Cannot enforce bounds without knowing boundary conditions | Not extracted |

## What PLAYABLE-003 Would Require

If and when the above ambiguities are resolved, PLAYABLE-003 would implement:

- Health-track initialization from completed character snapshot
- Damage application as deterministic Rule Set state transition
- Damage recovery/removal as deterministic Rule Set state transition
- Derived wound-penalty and incapacity/death state (if authority supports it)
- Chronicle-neutral orchestration for health state transitions
- Versioned immutable runtime state updates

## Deferred Until Extraction Complete

- All health-track modeling
- All damage-category modeling
- All wound-penalty modeling
- All incapacitation/death modeling
- All healing-priority rules
- All overflow rules
- All damage-conversion rules

## Governing Authority for Future PLAYABLE-003

- `docs/extraction/werewolf-3e/EXTRACTION-0003-character-creation-slice.md` (damage/soak/Health resolution explicitly deferred)
- `docs/extraction/werewolf-3e/EXTRACTION-0004-ambiguities-and-conflicts.md` (no health/damage ambiguities extracted)
- `docs/extraction/werewolf-3e/EXTRACTION-0005-contract-findings.md` (must validate any future health/damage contract)
- Dedicated health/damage extraction document (does not yet exist)
- Dedicated semantic review of wound penalties, incapacitation, and death rules (does not yet exist)

## Governance

- New decision request: required (this document).
- Existing decision set reopened: no.
- Decision set artifact: this document.
- Review record artifact: to be attached after health/damage extraction and semantic review advance.
- Decision boundary: records the stop condition for PLAYABLE-003; does not approve any health/damage mechanics.
