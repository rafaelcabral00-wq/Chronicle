---
id: DR-0010
title: Werewolf Health and Damage Boundary
status: accepted
accepted_option: Option A
accepted_date: 2026-08-11
updated_date: 2026-08-14
---

# DR-0010: Werewolf Health and Damage Boundary

## Decision Record

Status: accepted.

Accepted option: Option A (original boundary decision).

Effective date: 2026-08-11.

Updated: 2026-08-14 (implementation complete).

## Original Decision

This document records a new explicit Chronicle project decision. It is not a historical approval from the prototype review process. EXTRACTION-0003 explicitly defers "damage", "soak", "Health resolution", "regeneration", and "Frenzy" from Slice 001. EXTRACTION-0004 contains no health/damage ambiguities because the source material was not extracted for those mechanics. The prototype `resources.json` and `set-resource-rating.json` contain no health or damage fields. No pre-existing approved artifact authorizes the executable behavior that would be required for PLAYABLE-003.

**PLAYABLE-003 was blocked until the following source ambiguities were resolved:**

1. Health-track structure (number and names/order of health levels)
2. Damage category structure (bashing/lethal/aggravated or alternative categories)
3. Mixed damage ordering and conversion rules
4. Overflow behavior when damage exceeds health track capacity
5. Healing/removal priority among mixed damage types
6. Incapacitation/death semantics and boundaries
7. Wound penalty derivation from current health state

Until these were resolved, Chronicle must not:
- invent a health-track structure from general World of Darkness knowledge;
- hard-code damage categories without source authority;
- silently normalize `.permanent` health identifiers as `.current` mutations;
- assign wound-penalty, incapacity, or death semantics to unresolved state.

## Resolution

All ambiguities have been resolved through dedicated extraction and semantic review:

1. **Health-track structure**: Resolved via DR-0011 and source analysis. 7-level track with Bashing/Lethal/Aggravated categories.
2. **Damage category structure**: Resolved. Three canonical categories: Bashing, Lethal, Aggravated.
3. **Mixed damage ordering**: Resolved via DR-0011 (Option B). Category-independent filling from the bottom. No conversion.
4. **Overflow behavior**: Resolved. Death threshold at Incapacitated + additional damage, differentiated by damage type.
5. **Healing/removal priority**: Resolved via DR-0011 (Option B). Caller explicitly identifies category to heal; no automatic priority.
6. **Incapacitation/death semantics**: Resolved. State machine implemented in `WerewolfHealthTrack`.
7. **Wound penalty derivation**: Resolved. Derived from total damage count.

## Implementation

The following types implement the resolved health/damage model:

- `WerewolfHealthTrack`: Immutable record for the 7-level health track
- `WerewolfHealthTrackComputer`: Deterministic computation of health track state
- `WerewolfApplyDamageService`: Damage application as deterministic state transition
- `WerewolfRecoverDamageService`: Damage recovery as deterministic state transition
- `WerewolfRegenerationService`: Regeneration rules including Vigor test for lethal healing
- `WerewolfPermanecerAtivoService`: Permanecer Ativo rule implementation
- `WerewolfRuntimeCharacterState`: Runtime state including HealthTrack

## Authority Audit Summary

### Approved Artifacts Inspected

- **EXTRACTION-0003**: Character Creation Vertical Slice. Explicitly defers: `damage`, `soak`, `Health resolution`, `regeneration`, `Frenzy`, `Rites`, `Umbra`, `spirits`, `Totem group aggregation`, `experience progression`, `Renown advancement`, `antagonists`, `pack creation`.
- **EXTRACTION-0004**: Ambiguities and Conflicts. Contains 18 ambiguities (A-001 through A-018). None relate to health, damage, wound penalties, incapacitation, death, or damage categories.
- **EXTRACTION-0005**: Contract Findings. Does not validate any health/damage generic contract.
- **DR-0005**: Resource initialization boundary. Blocks IMPLEMENT-019B and all Renown behavior. Does not mention health/damage.
- **DR-0007**: Character completion boundary. Defines immutable `WerewolfCharacterSnapshot`. Does not mention health/damage.
- **DR-0008**: Action resolution boundary. Defines `generic-dice` capability for Attribute+Ability tests. Does not mention damage or health.
- **DR-0009**: Runtime resource boundary. Defines spend/recover for Rage/Gnosis/Willpower. Explicitly defers combat, damage, Frenzy.
- **DR-0011**: Mixed Damage Ordering and Conversion. Accepted Option B (Category-Independent Filling).
- **Prototype `resources.json`**: Structural constraints for Rage/Gnosis/Willpower only. No health fields.
- **Prototype `set-resource-rating.json`**: Creation-time resource mutation only. No health/damage operations.
- **Current package metadata**: Health-related capabilities, operations, and fields now implemented. `post-creation-character-operations` capability includes apply-damage, recover-damage, permanecer-ativo, and regenerate operations.
- **Current runtime state**: `WerewolfRuntimeCharacterState` carries Rage/Gnosis/Willpower current/permanent values, version, and `HealthTrack`.

### Source Evidence

| Topic | Source Lines | Finding |
|-------|-------------|---------|
| Health-track structure | 2860 | RESOLVED. 7 levels: Healthy, Escoriado, Machucado, Ferido, Contundido, Incapacitado, Morto. |
| Damage categories | 2861-2864 | RESOLVED. Bashing (Contusão), Lethal (Letal), Aggravated (Agravado). |
| Mixed damage ordering | 2860-2864 | RESOLVED via DR-0011. Category-independent filling. |
| Damage conversion/upgrading | 2861-2864, 539 | RESOLVED via DR-0011. No conversion. |
| Overflow beyond track | 2866-2869 | RESOLVED. Death threshold explicit. |
| Healing priority | 2872-2873 | RESOLVED via DR-0011. Caller-specified. |
| Wound penalty derivation | 2866-2869 | RESOLVED. Derived from total damage count. |
| Incapacitation/death semantics | 2866-2869 | RESOLVED. State machine implemented. |
| Soak test definition | 3096-3100 | RESOLVED. Garou test Vigor to absorb damage. |
| Soak interpretation | 3098-3100 | RESOLVED. Soak reduces incoming damage before application. |
| Permanecer Ativo | 2870 | RESOLVED. Test permanent Fury difficulty 8. |
| Lethal healing Vigor test | 1713, 2873 | RESOLVED. Test Vigor difficulty 8 for lethal healing in stressful situations. |
| Regeneration conditions | 2872-2873 | RESOLVED. Bashing/lethal: 1 level per turn. Aggravated: rest in alternate form. |

## Governance

- New decision request: DR-0011 (accepted 2026-08-14).
- Existing decision set reopened: no.
- Decision set artifacts: DR-0010 (this document), DR-0011.
- Review record artifact: implementation completed under accepted DR-0011 Option B (Chronicle Rule Set interpretation / house rule).
- Decision boundary: records the stop condition for PLAYABLE-003; DR-0011 accepted; health/damage mechanics are complete under Option B authority.
