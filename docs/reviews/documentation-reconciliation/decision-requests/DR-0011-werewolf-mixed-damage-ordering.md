---
id: DR-0011
title: Werewolf Mixed Damage Ordering and Conversion
status: accepted
accepted_option: Option B
accepted_date: 2026-08-14
decision_authority: human
blocks: RULESET-COMPLETION-005 damage application semantics
---

# DR-0011: Werewolf Mixed Damage Ordering and Conversion

## Decision Record

Status: accepted.

Accepted option: Option B.

Effective date: 2026-08-14.

Decision authority: Human decision recorded 2026-08-14.

## Decision

This document records an explicit human decision accepting Option B as a **Chronicle Rule Set interpretation / house rule**. This is NOT a claim that the canonical Werewolf source defines mixed-damage ordering, conversion, or healing priority.

### Rationale

The canonical source (`.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt`) was exhaustively searched and does not:

- define mixed-damage slot ordering;
- define automatic category conversion or upgrading;
- define automatic healing/removal priority.

The source states "O dano desloca o estado do personagem em direção a Incapacitado" but provides no explicit rule for how simultaneous multi-category damage is ordered on the health track.

Option B is accepted because:

1. It preserves the maximum source-authorized information (per-category counts, total damage, fatal damage type);
2. It does not invent ordering or conversion mechanics not present in source;
3. It has the lowest compatibility consequences;
4. It allows deterministic implementation of all source-resolved mechanics (health levels, wound penalties, Permanecer Ativo, regeneration, death semantics);
5. It keeps the representation lossless enough to migrate later if authoritative errata/source material defines canonical ordering/conversion.

## Accepted Model: Option B — Category-Independent Filling

**Label:** Chronicle Rule Set interpretation / house rule

### Semantics

- Health track capacity remains 7 levels.
- Bashing, Lethal, and Aggravated are independently counted.
- Total accumulated damage determines current Health level.
- No category has automatic slot precedence.
- No category automatically upgrades or converts another category.
- Incoming damage simply adds to its category count.
- Recovery requires an explicit category parameter.
- Recovery removes only from that category.
- No implicit healing priority is inferred between categories.
- Invalid recovery from a category with zero remaining damage is rejected.
- The representation preserves per-category counts and damage marks, enabling future migration if canonical ordering is later defined.

### Consequences

- The damage model is deterministic and source-minimal.
- Future source extraction that defines canonical ordering can be layered on top of the existing per-category counts without breaking the base model.
- All source-resolved mechanics (Permanecer Ativo, regeneration, death thresholds) are executable.

### Compatibility Note

This is a house rule. If future canonical material defines severity-ordered filling or category conversion, the existing `WerewolfDamageMark` list and per-category counts provide sufficient information to reconstruct slot provenance and migrate to the canonical model.

### Future Migration Requirement

If authoritative errata or additional source material defines canonical mixed-damage ordering or conversion:

1. The existing `DamageMarks` list preserves application order.
2. The existing `BashingCount`, `LethalCount`, and `AggravatedCount` preserve per-category totals.
3. A migration layer can reorder or convert marks without changing the public API surface.

## Impact

- Affects: `WerewolfHealthTrack`, `WerewolfApplyDamageService`, `WerewolfRecoverDamageService`, `WerewolfRegenerationService`, `WerewolfPermanecerAtivoService`, `WerewolfRuntimeCharacterState`
- Affects domains: Damage categories, Health levels, Wound penalties, Incapacitation and death
- Unblocks: Damage categories domain mechanical completeness
- Does NOT affect: Soak (delegated to Combat package)

## Governance

- New decision request: this document.
- Existing decision set reopened: no.
- Decision set artifacts: DR-0010, DR-0011.
- Review record artifact: implementation complete and validated.
