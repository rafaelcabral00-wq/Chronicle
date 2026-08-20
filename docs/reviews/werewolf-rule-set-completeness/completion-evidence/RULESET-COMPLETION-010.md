# RULESET-COMPLETION-010: Resolve Metis Deformity Modeling

## 1. Exact Package Scope

**Title:** Resolve Metis deformity modeling

**Owned domain keys:**
- `Metis deformity` (completeness-matrix.json domain)

**Exact ambiguity IDs:**
- A-008 — Metis Deformity Representation

**Exact completion condition:**
A-008 resolved; all source-defined Metis deformities cataloged with declarative mechanical effect models; selection operation supports full catalog; Character Completion validates deformity requirements.

**Exact expected executable artifacts:**
- `CharacterCreation/WerewolfMetisDeformityIdentifiers.cs` — stable machine keys, canonical `Supported` list, declarative `Effects` dictionary
- `CharacterCreation/WerewolfMetisDeformityEffects.cs` — `WerewolfMetisDeformityEffectKind` enum and `WerewolfMetisDeformityEffect` record
- `CharacterCreation/WerewolfMetisDeformitySelection.cs` — selection service supporting all 13 source-defined deformities
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfMetisDeformitySelectionTests.cs` — expanded test coverage
- `Localization/en/current-slice.json` — deformity display names
- `Localization/pt-BR/current-slice.json` — deformity display names

**What belongs to RULESET-COMPLETION-011 instead:**
- Terminology stabilization for Race/Breed keys (A-017, A-018)
- Localization terminology review beyond display-name coverage

**What belongs to RULESET-COMPLETION-012 instead:**
- Runtime enforcement of declarative effects not covered by existing runtime mechanisms (combat, social interaction, forms, movement, perception, health/regeneration beyond WeakImmuneSystem)
- Full mechanical completeness of deformity effects

## 2. Source Locators

Canonical source: `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt`

**Metis deformity table:** lines 522–538
**Mandatory selection rule:** line 523 — "(Deve ser escolhida obrigatoriamente na criação do personagem)"
**Metis creation context:** lines 511–521

## 3. Complete Deformity Inventory

Source defines **13** Metis deformities. All are mandatory selection (exactly one) during character creation for Metis characters.

| # | Machine Key | Source Name | Source Locator | Effect Count |
|---|-------------|-------------|----------------|--------------|
| 1 | `fits-of-madness` | Acessos de Loucura | line 526 | 1 |
| 2 | `albinism` | Albino | line 527 | 1 |
| 3 | `hairless` | Alopécico | line 528 | 1 |
| 4 | `blind` | Cego | line 529 | 1 |
| 5 | `seizures` | Convulsões | line 530 | 1 |
| 6 | `hunchback` | Corcunda | line 531 | 2 |
| 7 | `horns` | Cornos | line 532 | 3 |
| 8 | `tough-hide` | Couro Duro | line 533 | 2 |
| 9 | `debilitating-disease` | Doença Debilitante | line 534 | 1 |
| 10 | `withered-limb` | Membro Atrofiado | line 535 | 1 |
| 11 | `tailless` | Sem Cauda | line 536 | 2 |
| 12 | `no-sense-of-smell` | Sem Olfato | line 537 | 2 |
| 13 | `weak-immune-system` | Sistema Imunológico Fraco | line 538 | 1 |

**Total:** 13 deformities, 17 distinct mechanical effects.

## 4. Selection Semantics

**Eligibility:** Metis race only. Non-Metis (Homid, Lupus) cannot select a deformity.

**Count:** Exactly one deformity required. Source states: "(Deve ser escolhida obrigatoriamente na criação do personagem)"

**Random vs chosen:** Source presents a fixed table; no random selection rule is stated. Selection is player-choice from the catalog.

**Stage:** During character creation, before Character Completion. The `select-metis-deformity` required step is resolved by the selection operation.

**Duplicate selection:** Replaced in-place; DraftVersion increments. No duplicate rejection needed because only one deformity slot exists.

**Unknown/invalid keys:** Rejected with `UnknownDeformity` or `MalformedDeformity` error codes.

## 5. Declarative Effect Model

**Model:** Hybrid per A-008 recommendation — catalog identity + declarative static modifiers + compiled handler only for conditional/event-driven behavior.

**Effect kinds implemented:**

| EffectKind | Description | Used By |
|------------|-------------|---------|
| `DifficultyModifier` | +N difficulty to specific target under condition | Albinism, Hairless, Hunchback, Horns, DebilitatingDisease, WitheredLimb, Tailless |
| `AutomaticFailure` | Automatic failure for tests of a domain | Blind |
| `ConditionalTest` | Willpower test (difficulty 8, minimum 3 successes) under condition; failure causes consequence | FitsOfMadness, Seizures |
| `CombatDamage` | Damage value added to attack (bashing) | Horns |
| `RenownPenalty` | Permanent Renown adjustment | Horns |
| `AttributeMaximum` | Hard cap on Attribute rating | ToughHide |
| `DiceBonus` | +N dice to specific action | ToughHide |
| `HealthLevelRemoved` | Removes specific health level from character | WeakImmuneSystem |
| `SensoryFailure` | Automatic failure for sensory domain | NoSenseOfSmell |
| `TrackingPenalty` | +N difficulty to tracking attempts | NoSenseOfSmell |
| `FormRestricted` | Difficulty modifier restricted to specific forms | Tailless |

**Effect record fields:**
- `Kind` — effect category
- `Target` — affected Attribute/Ability/domain
- `Value` — numeric modifier (positive or negative)
- `Form` — form restriction (e.g., "Lupus,Hispo,Crinos")
- `Condition` — triggering condition (e.g., "daylight-without-protection")
- `Consequence` — failure consequence (e.g., "temporary-psychotic-episode")
- `Sense` — affected sense (e.g., "olfactory")
- `TestDifficulty` — for ConditionalTest effects
- `MinimumSuccesses` — for ConditionalTest effects
- `Notes` — additional context (e.g., "bashing")

## 6. Executable vs Dependency-Blocked Effects

### A. Fully Executable Now
- **Catalog completeness** — all 13 deformities cataloged with stable keys
- **Selection eligibility** — Metis-only enforcement implemented
- **Selection count** — exactly one deformity enforced by step resolution
- **Unknown key rejection** — implemented
- **Malformed key rejection** — implemented
- **DraftVersion immutability** — implemented
- **Character Completion validation** — MetisDeformityMissing check implemented
- **Localization coverage** — en and pt-BR display names added
- **WeakImmuneSystem health track effect** — existing `WerewolfHealthTrackComputer.Compute` already supports `hasWeakenedImmuneSystem` parameter; 010 linked declarative model to runtime via `WerewolfRuntimeCharacterState.FromSnapshot`

### B. Declaratively Modeled but Runtime Dependency Unavailable
These effects are represented in the `Effects` dictionary but cannot be enforced by current runtime:

| Deformity | Effect | Dependency Owner | Exact Blocker |
|-----------|--------|------------------|---------------|
| FitsOfMadness | Conditional Willpower test under tension | RULESET-COMPLETION-012 | No consumer for conditional tension tests |
| Seizures | Conditional test on critical failure | RULESET-COMPLETION-012 | No consumer for critical-failure triggers |
| Blind | Automatic failure for vision tests | RULESET-COMPLETION-012 | No automatic failure mechanism in action tests |
| Albinism | +2 difficulty Perception in daylight | RULESET-COMPLETION-012 | Action-test inputs lack daylight/context flag |
| NoSenseOfSmell | Automatic failure smell tests; +2 tracking | RULESET-COMPLETION-012 | No smell/tracking subsystem |
| Horns | +1 Social difficulty; combat damage; Renown penalty | RULESET-COMPLETION-012 | No combat/social subsystems |
| Tailless | +1 Social/Dexterity difficulty in Lupus/Hispo/Crinos | RULESET-COMPLETION-012 | No form subsystem; action-test inputs lack form context |
| DebilitatingDisease | +2 difficulty Stamina tests (including absorption) | RULESET-COMPLETION-012 | No consumer for deformity-derived difficulty modifiers |
| WitheredLimb | +2 difficulty Dexterity using withered limb | RULESET-COMPLETION-012 | Action-test inputs lack limb-specific context |
| Hunchback | +1 Social/Dexterity difficulty | RULESET-COMPLETION-012 | No social subsystem; no consumer for deformity modifiers |
| Hairless | +1 Social difficulty | RULESET-COMPLETION-012 | No social subsystem |
| ToughHide | Appearance max 1; +1 absorption dice | RULESET-COMPLETION-012 | No consumer for declarative effect application |

### C. Narrative/Non-Mechanical
None. All 13 deformities have explicit mechanical effects in source.

### D. Ambiguous
None. Source table provides explicit mechanical effects for all deformities.

### E. Outside 010
None. All Metis deformity effects are within scope.

## 7. Health/Regeneration Interaction

### Existing Pre-010 Runtime Behavior

`WerewolfHealthTrackComputer.Compute` already accepted a `hasWeakenedImmuneSystem` boolean parameter before 010. When true and total damage is 0, it bumps `effectiveTotal` to 1, causing the effective health track to start at `Machucado` instead of `Escoriado`. This mechanism was present but never connected to deformity selection.

**Audit finding:** The existing runtime behavior matches the source rule for Weak Immune System ("Não possui o nível de vitalidade Escoriado. O registro de dano inicia-se diretamente em Machucado."). 010 linked the declarative deformity catalog to this existing runtime mechanism.

### 010 Runtime Integration

`WerewolfRuntimeCharacterState.FromSnapshot` now inspects `snapshot.MetisDeformity` and passes `hasWeakenedImmuneSystem: true` when the selected deformity is `weak-immune-system`. This is a minimal deterministic adapter owned by 010.

**Classification:** Weak Immune System is **executable now**.

### Other Deformities

**DebilitatingDisease** modifies Stamina tests including absorption. Modeled as `DifficultyModifier` targeting `Stamina` with value 2 and note "includes absorption". Runtime enforcement depends on RULESET-COMPLETION-012 (no consumer reads declarative effects to adjust test difficulties).

**ToughHide** grants +1 absorption dice. Modeled as `DiceBonus` targeting `Absorption` with value 1. Runtime enforcement depends on RULESET-COMPLETION-012 (no consumer applies declarative dice bonuses).

No other deformities modify health, regeneration, wound penalties, Rage/Gnosis/Willpower, or Stay Active behavior per source.

## 8. Authority Boundary for Declarative Effects

**Is `WerewolfMetisDeformityEffect` merely metadata?**
No. It is a structured effect descriptor that one runtime consumer (`WerewolfRuntimeCharacterState.FromSnapshot`) already inspects for WeakImmuneSystem. It is intended to be the single canonical authority for deformity effects.

**Does action resolution currently inspect it?**
No. `WerewolfActionTestDefinitionService` accepts a `Modifier` parameter but does not read `WerewolfMetisDeformityIdentifiers.Effects`. No consumer automatically applies deformity difficulty modifiers to action tests.

**Does health runtime inspect it?**
Partially. `WerewolfHealthTrackComputer.Compute` does not inspect `WerewolfMetisDeformityIdentifiers.Effects`; it uses its own `hasWeakenedImmuneSystem` boolean parameter. The linkage is made in `WerewolfRuntimeCharacterState.FromSnapshot`, which bridges the declarative deformity identity to the existing runtime flag.

**Is there an effect resolver/aggregator?**
No. There is no central resolver that iterates `Effects` and applies them to runtime state. Introducing one is not required by 010's completion condition; it is explicitly deferred to 012.

**Preferred authority pattern:**
One canonical deformity identity (`WerewolfMetisDeformityIdentifiers.Supported`) feeds deterministic consumers. WeakImmuneSystem is consumed by `FromSnapshot` → `HealthTrackComputer`. Other effects remain declarative until 012 introduces consumers.

## 9. Action-Resolution Interaction

Effects that modify already-supported action resolution:

| Deformity | Effect | Existing Consumer Possible? | Executable Now? | Exact Blocker/Owner |
|-----------|--------|----------------------------|-----------------|---------------------|
| FitsOfMadness | Conditional Willpower test (diff 8, min 3) under tension | Generic test definition supports Willpower + difficulty + modifier | **No** | No consumer for conditional tension trigger; owner 012 |
| Seizures | Conditional test on critical failure | Generic test definition supports tests | **No** | No consumer for critical-failure trigger; owner 012 |
| Blind | Automatic failure for vision tests | No automatic-failure mechanism | **No** | No mechanism exists; owner 012 |
| Albinism | +2 difficulty Perception in daylight | Difficulty modifier representable | **No** | Action-test inputs lack daylight/vision context; owner 012 |
| WitheredLimb | +2 difficulty Dexterity using withered limb | Difficulty modifier representable | **No** | Action-test inputs lack limb-specific context; owner 012 |
| Tailless | +1 difficulty Dexterity (balance) in Lupus/Hispo/Crinos | Difficulty modifier + form restriction representable | **No** | Action-test inputs lack form context; owner 012 |
| Hunchback | +1 difficulty Social and Dexterity | Difficulty modifiers representable | **No** | No social subsystem; no consumer for deformity modifiers; owner 012 |
| Hairless | +1 difficulty Social | Difficulty modifier representable | **No** | No social subsystem; owner 012 |
| Horns | +1 difficulty Social | Difficulty modifier representable | **No** | No social subsystem; owner 012 |

All are modeled declaratively. None require extended/resisted tests or combat-specific resolution.

## 9. Weak Immune System Regression Test

**End-to-end test proves:**

Metis + `weak-immune-system`
→ completed character preserves deformity in `MetisDeformity` field
→ `WerewolfRuntimeCharacterState.FromSnapshot` sets `hasWeakenedImmuneSystem: true`
→ `WerewolfHealthTrackComputer.Compute` with `hasWeakenedImmuneSystem=true` starts effective health track at `Machucado` (not `Escoriado`)

**Test file:** `WerewolfRuntimeCharacterStateTests.cs`
- `FromSnapshotWithWeakImmuneSystemSetsHealthTrackFlag` — verifies flag and starting level
- `FromSnapshotWithoutWeakImmuneSystemLeavesHealthTrackNormal` — verifies normal start
- `FromSnapshotWithOtherDeformityLeavesHealthTrackNormal` — verifies non-Weak deformities don't trigger flag

**Test file:** `WerewolfApplyDamageTests.cs`
- `WeakenedImmuneSystemStartsTrackAtMachucado` — verifies effective total 0 maps to Machucado
- `WeakenedImmuneSystemDoesNotAdvanceTrackBeyondFirstLevel` — verifies Escoriado is not current level
- `NormalImmuneSystemStartsAtEscoriado` — verifies baseline behavior

**Architectural note:** Creation deformity is carried into runtime state through `WerewolfCharacterSnapshot.MetisDeformity`, which is populated by Character Completion from `Draft.MetisDeformity`. The runtime state constructor (`FromSnapshot`) bridges this to the existing health track mechanism.

## 10. Character Completion Behavior

**Metis without deformity:** Rejected with `MetisDeformityMissing` error. Already implemented in `WerewolfCharacterCompletionOperation.Complete`.

**Valid Metis with deformity:** Accepted. All 13 deformities are valid selections.

**Non-Metis with deformity:** Not possible through selection service (RaceNotMetis rejection). If somehow present in draft, completion does not explicitly reject it, but the selection service prevents it.

**Invalid/unknown deformity key:** Rejected by selection service with `UnknownDeformity` or `MalformedDeformity`. Completion validates via draft state.

**Invalid selection count:** Exactly one deformity required. Selection service enforces single-slot step resolution.

**Dependency-blocked declarative effect preservation:** Selected deformity identity is preserved in `MetisDeformity` field of completed snapshot. Declarative effect metadata is available via `WerewolfMetisDeformityIdentifiers.Effects` dictionary. Effects do not silently disappear.

**WeakImmuneSystem runtime linkage:** Completed snapshot with `MetisDeformity = "weak-immune-system"` produces runtime state with `HasWeakenedImmuneSystem = true`, which causes health track initialization to start at Machucado.

## 10. A-008 Disposition

**Original ambiguity:** Should a deformity be represented as (A) simple catalog selection, (B) catalog selection plus declarative effects, (C) structured effect graph, (D) compiled handler per deformity, or (E) hybrid?

**Source evidence:** Source table (lines 522–538) defines explicit mechanical effects for each deformity, including fixed penalties, conditional tests, automatic failures, field maxima, additional dice, missing health levels, attacks, and social consequences.

**Final disposition:** Resolved as hybrid (option E) per A-008 recommended direction:
- Catalog identity (stable machine keys)
- Declarative static modifiers (`WerewolfMetisDeformityEffect` records)
- Compiled handler reserved for conditional/event-driven behavior (deferred to 012)

**Source fully resolves:** Yes. The source table provides sufficient detail for declarative modeling without house rules.

**Human decision remaining:** No. The hybrid model matches the A-008 recommended direction and is fully derived from source structure.

## 11. A-008 Disposition

**Original ambiguity:** Should a deformity be represented as (A) simple catalog selection, (B) catalog selection plus declarative effects, (C) structured effect graph, (D) compiled handler per deformity, or (E) hybrid?

**Source evidence:** Source table (lines 522–538) defines explicit mechanical effects for each deformity, including fixed penalties, conditional tests, automatic failures, field maxima, additional dice, missing health levels, attacks, and social consequences.

**Final disposition:** Resolved as hybrid (option E) per A-008 recommended direction:
- Catalog identity (stable machine keys)
- Declarative static modifiers (`WerewolfMetisDeformityEffect` records)
- Compiled handler reserved for conditional/event-driven behavior (deferred to 012)

**Source fully resolves:** Yes. The source table provides sufficient detail for declarative modeling without house rules.

**Human decision remaining:** No. The hybrid model matches the A-008 recommended direction and is fully derived from source structure.

## 12. Other Ambiguities Discovered

None. A-017 (Metis terminology) is a separate ambiguity owned by RULESET-COMPLETION-011. No new ambiguity IDs required.

## 13. Affected Completeness Rows

**Updated row:** `Metis deformity` in `completeness-matrix.json`

Changes:
- `packageSourceStatus`: `partial-executable: horns only` → `complete: catalog and declarative effects modeled`
- `runtimeStatus`: `implemented for horns` → `catalog complete; effects declaratively modeled; selection executable; WeakImmuneSystem linked to existing health runtime`
- `completionImpact`: `Metis path completable with horns` → `Metis path completable with any source-defined deformity`
- `requiredRemediation`: `Expand deformity catalog; resolve A-008, A-017` → `Resolve A-017; runtime enforcement of declarative effects (except WeakImmuneSystem which is executable)`
- `catalogCoverage`: `partial` → `complete`
- `implementationCoverage`: `complete` → `complete for catalog, selection, declarative effects, and WeakImmuneSystem runtime linkage`
- `testCoverage`: `complete` → `complete for expanded catalog, effects, and runtime linkage`
- `packageExposure`: `partial` → `complete`
- `mechanicalCompleteness`: `false` (unchanged)
- `currentSliceExecutable`: `true` (unchanged)

**Unaffected rows:** All other 67 domains unchanged.

## 14. Completeness Counts

**Before:** 24/68 mechanically complete (35.3%), 35/68 current-slice executable (51.5%)

**After:** 24/68 mechanically complete (35.3%), 35/68 current-slice executable (51.5%)

**Reason:** The `Metis deformity` domain remains `currentSliceExecutable: true` and `mechanicalCompleteness: false`. The package expanded the catalog, added declarative effect modeling, and linked WeakImmuneSystem to existing runtime. Most effect enforcement still requires RULESET-COMPLETION-012, so mechanical completeness does not increase.

## 15. Tests by Project

**Project:** `Chronicle.RuleSets.Werewolf.Tests`

**New/modified test files:**
- `WerewolfMetisDeformitySelectionTests.cs` — expanded to 16 test methods
- `WerewolfApplyDamageTests.cs` — added 3 WeakImmuneSystem health track tests
- `WerewolfRuntimeCharacterStateTests.cs` — new file with 3 FromSnapshot linkage tests

**New/modified tests in `WerewolfMetisDeformitySelectionTests.cs`:**
1. `SelectsEveryCurrentSliceDeformity` — expanded from 1 to 13 inline data cases
2. `DeformityIdentifiersAreCanonicalAndLocalizationIndependent` — updated count assertion from 1 to 13
3. `RejectsInvalidMalformedAndUnknownDeformity` — unchanged
4. `RejectsStaleDraftVersion` — unchanged
5. `UpdatesDraftImmutablyAndIncrementsExactlyOnce` — unchanged
6. `ResolvesMetisDeformityNextStepWithoutApplyingEffects` — unchanged
7. `RaceChangesAwayFromAndBackToMetisRemainCoherent` — unchanged
8. `PreservesAuspiceTribeAndUnrelatedState` — unchanged
9. `RuntimeFlowCreatesDraftSelectsRaceAuspiceTribeAndMetisDeformity` — unchanged
10. `MetisDeformitySelectionHasNoForbiddenDependencies` — unchanged
11. `RejectsUnsetHomidAndLupusRace` — unchanged
12. `ReplacesExistingDeformityWhileRaceRemainsMetis` — unchanged
13. **NEW** `CatalogContainsExactlyThirteenSourceDefinedDeformities` — verifies count and effect presence
14. **NEW** `CatalogKeysAreUniqueAndCanonical` — verifies uniqueness and whitespace-free keys
15. **NEW** `EveryDeformityHasDeclaredEffects` — theory with 13 cases verifying effect count per deformity
16. **NEW** `EffectModelCoversAllRequiredKinds` — verifies all 11 effect kinds are present

**Total Werewolf test cases:** 833 (up from 811 pre-010 baseline)

**New tests in `WerewolfApplyDamageTests.cs`:**
1. `WeakenedImmuneSystemStartsTrackAtMachucado`
2. `WeakenedImmuneSystemDoesNotAdvanceTrackBeyondFirstLevel`
3. `NormalImmuneSystemStartsAtEscoriado`

**New tests in `WerewolfRuntimeCharacterStateTests.cs`:**
1. `FromSnapshotWithWeakImmuneSystemSetsHealthTrackFlag`
2. `FromSnapshotWithoutWeakImmuneSystemLeavesHealthTrackNormal`
3. `FromSnapshotWithOtherDeformityLeavesHealthTrackNormal`

## 16. Files Changed

### Source files (new)
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfMetisDeformityEffects.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfMetisDeformityIdentifiers.cs`

### Source files (modified)
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfMetisDeformitySelection.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRuntimeCharacterState.cs`
- `src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs`

### Test files (new)
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfRuntimeCharacterStateTests.cs`

### Test files (modified)
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfMetisDeformitySelectionTests.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfApplyDamageTests.cs`

### Localization files (modified)
- `rule-sets/Chronicle.RuleSets.Werewolf/Localization/en/current-slice.json`
- `rule-sets/Chronicle.RuleSets.Werewolf/Localization/pt-BR/current-slice.json`

### Documentation files (modified)
- `docs/reviews/werewolf-rule-set-completeness/completeness-matrix.json`
- `docs/reviews/werewolf-rule-set-completeness/completeness-report.md`

### Evidence file (new)
- `docs/reviews/werewolf-rule-set-completeness/completion-evidence/RULESET-COMPLETION-010.md`

## 17. Full-Solution Total

**Baseline before 010:** 861

**After 010 (including audit corrections):** 883

| Project | Baseline | After 010 |
|---------|----------|-----------|
| Domain | 1 | 1 |
| Contracts | 8 | 8 |
| Application | 9 | 9 |
| PackageValidator | 8 | 8 |
| Persistence | 1 | 1 |
| Werewolf | 811 | 833 |
| Infrastructure | 12 | 12 |
| Architecture | 11 | 11 |
| **Total** | **861** | **883** |

**Werewolf delta:** +22 tests (16 from initial 010 catalog expansion + 6 from audit: 3 health track + 3 runtime state linkage)

## 18. Package Validator Result

Valid. 46 files inventoried, 0 findings.

## 19. Matrix Integrity

Valid JSON. Verified with Python `json.load()`.

## 20. Localization Integrity

Both `en/current-slice.json` and `pt-BR/current-slice.json` are valid JSON. All 13 deformity keys added with concise display names.

## 21. git diff --check

Clean (CRLF normalization warning only, not an error).

## 22. git status --short

```
 M docs/reviews/werewolf-rule-set-completeness/completeness-matrix.json
 M docs/reviews/werewolf-rule-set-completeness/completeness-report.md
 M rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfMetisDeformitySelectionTests.cs
 M rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfApplyDamageTests.cs
 M rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfMetisDeformitySelection.cs
 M rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRuntimeCharacterState.cs
 M rule-sets/Chronicle.RuleSets.Werewolf/Localization/en/current-slice.json
 M rule-sets/Chronicle.RuleSets.Werewolf/Localization/pt-BR/current-slice.json
 M src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs
?? .kilo/
?? docs/reviews/werewolf-rule-set-completeness/completion-evidence/RULESET-COMPLETION-010.md
?? rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfRuntimeCharacterStateTests.cs
?? rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfMetisDeformityEffects.cs
?? rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfMetisDeformityIdentifiers.cs
```

`.kilo/` remains untracked.

## 23. Remaining Blockers

**0 ownerless blockers.**

**Deferred dependencies (all have exact owners):**
- Runtime enforcement of 12 declarative effects → RULESET-COMPLETION-012
- A-017 (Metis terminology) → RULESET-COMPLETION-011
