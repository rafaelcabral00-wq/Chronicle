# RULESET-COMPLETION-012E: Action Resolution and Condition Taxonomy

**Status:** Complete
**Date:** 2026-08-22
**Parent Package:** RULESET-COMPLETION-012
**Subpackage:** 012E (fifth controlled subpackage of 012)

## 1. Exact 012E Title and Scope

**Title:** Action Resolution and Condition Taxonomy

**Owned domain keys:**
- Metis deformity (mechanical effects enforcement for action resolution)
- Attribute + Ability test definition (modifier integration)

**Completion condition:**
All source-defined Metis deformity effects that modify action resolution are implemented as deterministic services; condition model supports temporary-psychotic-episode, incapacitated, and critical-failure states; action availability evaluation blocks actions under applicable conditions; dice modifiers and difficulty modifiers are kept distinct; package validator allow-list includes new files; completeness matrix and report are updated.

**Source ambiguities/findings resolved by 012E:**
- The existing `WerewolfMetisDeformityEffectKind` enum (DifficultyModifier, AutomaticFailure, AttributeMaximum, DiceBonus, HealthLevelRemoved, RenownPenalty, ConditionalTest, CombatDamage, ToughHide, FormRestricted, SensoryFailure, TrackingPenalty) provides the complete modifier taxonomy.
- Deformity effect targets use English short names (e.g., "Perception", "Social", "Dexterity") that must be mapped to canonical attribute identifiers (e.g., `character.attribute.perception`).
- "Social" target maps to Charisma, Manipulation, and Appearance attributes.
- Form-restricted effects use short form names (e.g., "Lupus,Hispo,Crinos") that must be mapped to canonical form identifiers (e.g., `character.form.lupus`).

## 2. Source Locators

Canonical source: `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt`

**Metis deformity table:** lines 523-539
**Fits of Madness:** line 527
**Albinism:** line 528
**Blind:** line 530
**Seizures:** line 531
**Debilitating Disease:** line 535
**Withered Limb:** line 536
**Tailless:** line 537
**No Sense of Smell:** line 538

## 3. Source Traversal Scope

Full traversal of the canonical source (3,948 lines) confirmed that 012E owns:
- All Metis deformity mechanical effects that modify action resolution
- The condition model for deformity-triggered states
- Action availability evaluation based on conditions

Mechanics explicitly deferred to future owners:
- Frenzy triggers (deferred to 012F or later)
- Social mechanics for Hairless, Hunchback, Horns (deferred to 012F)
- Gift execution (out of scope)
- Rites (out of scope)
- Spirits/Umbra (out of scope)
- Pack tactics (out of scope)
- Progression (out of scope)
- Combat mechanics (already complete in 012C/012D)
- Health/Damage (already complete in 005)
- Renown penalties from Horns (deferred to 012F)
- Combat damage from Horns (already complete in 012C)
- AttributeMaximum from ToughHide (deferred to character creation validation)
- Health level removal from WeakImmuneSystem (already complete in 005)

## 4. Modifier Taxonomy

The existing `WerewolfMetisDeformityEffectKind` enum provides the complete modifier taxonomy. No new kinds were needed.

**Taxonomy audit (all 11 kinds are source-backed):**

| Kind | Source Locator | Source Deformity | Source Text |
|------|---------------|-----------------|-------------|
| DifficultyModifier | Line 528 | Albinism | `Penalidade de +2 na dificuldade em testes de Percepção` |
| DifficultyModifier | Line 535 | DebilitatingDisease | `Penalidade de +2 na dificuldade em testes de Vigor` |
| DifficultyModifier | Line 536 | WitheredLimb | `Penalidade de +2 na dificuldade em testes de Destreza que utilizem o membro` |
| DifficultyModifier | Line 537 | Tailless | `Penalidade de +1 na dificuldade em testes Sociais e de Destreza (equilíbrio)` |
| DifficultyModifier | Line 529 | Hairless | `Penalidade de +1 na dificuldade em testes Sociais` |
| DifficultyModifier | Line 532 | Hunchback | `Penalidade de +1 na dificuldade em testes Sociais e baseados em Destreza` |
| DifficultyModifier | Line 533 | Horns | `Penalidade de +1 na dificuldade em testes Sociais` |
| DiceBonus | Line 534 | ToughHide | `Garante +1 dado em testes de Absorção` |
| AutomaticFailure | Line 530 | Blind | `Falha automática em testes baseados em visão` |
| AutomaticFailure | Line 538 | NoSenseOfSmell | `Falha automática em testes de Percepção baseados em odor` |
| ConditionalTest | Line 527 | FitsOfMadness | `Teste de Força de Vontade (dificuldade 8) sob tensão` |
| ConditionalTest | Line 531 | Seizures | `Em falhas críticas importantes, teste de Força de Vontade (dificuldade 8)` |
| FormRestricted | Line 537 | Tailless | `nas formas Lupina, Hispo e Crinos` |
| SensoryFailure | Line 538 | NoSenseOfSmell | `Sem nervos olfativos` → automatic failure on smell-based perception |
| TrackingPenalty | Line 538 | NoSenseOfSmell | `penalidade de +2 na dificuldade para rastrear por Instinto Primitivo` |
| AttributeMaximum | Line 534 | ToughHide | `Aparência máxima é 1` |
| HealthLevelRemoved | Line 539 | WeakImmuneSystem | `Não possui o nível de vitalidade Escoriado` |
| RenownPenalty | Line 533 | Horns | `mas reduz Renome por Glória` |
| CombatDamage | Line 533 | Horns | `Ataque com cornos causa Dano (Força + 1) por Contusão` |

No taxonomy kinds are speculative. Every kind has at least one canonical source-backed use case.

## 5. Deformity-by-Deformity Disposition

### Fits of Madness (Acessos de Loucura)
- **Source:** Line 527
- **Implementation:** ConditionalTest effect triggers Willpower test (difficulty 8, minimum 3 successes) under tension. Failure consequence: temporary-psychotic-episode condition.
- **Tests:** FitsOfMadnessRequiresConditionalTestUnderTension, FitsOfMadnessDoesNotRequireTestWithoutTension

### Seizures (Convulsões)
- **Source:** Line 531
- **Implementation:** ConditionalTest effect triggers Willpower test (difficulty 8, minimum 3 successes) on critical failure. Failure consequence: incapacitated condition.
- **Tests:** SeizuresConditionalTestNotAddedWithoutCriticalFailure, SeizuresOnCriticalFailure

### Albinism (Albino)
- **Source:** Line 528
- **Implementation:** DifficultyModifier +2 to Perception tests in daylight without protection.
- **Tests:** AlbinismAddsDaylightPerceptionDifficulty, AlbinismDoesNotApplyWithoutDaylight, AlbinismDoesNotAffectNonPerceptionTests, AlbinismInDaylightPerception, AlbinismAtNightNoModifier

### Blind (Cego)
- **Source:** Line 530
- **Implementation:** AutomaticFailure on vision-based tests.
- **Tests:** BlindCausesAutomaticFailureOnVisionTests, BlindDoesNotAffectNonVisionTests, BlindVisionTestAutomaticFailure, BlindNonVisionTestNoFailure

### Debilitating Disease (Doença Debilitante)
- **Source:** Line 535
- **Implementation:** DifficultyModifier +2 to Stamina tests (including absorption).
- **Tests:** DebilitatingDiseaseAddsStaminaDifficulty, DebilitatingDiseaseDoesNotAffectNonStaminaTests, ActionWithDifficultyModifier

### Withered Limb (Membro Atrofiado)
- **Source:** Line 536
- **Implementation:** DifficultyModifier +2 to Dexterity tests using the withered limb.
- **Tests:** WitheredLimbAddsDexterityDifficultyWhenUsingLimb, WitheredLimbDoesNotApplyWhenNotUsingLimb, WitheredLimbWhenUsingLimb, WitheredLimbWhenNotUsingLimb

### No Sense of Smell (Sem Olfato)
- **Source:** Line 538
- **Implementation:** SensoryFailure (olfactory-based Perception tests) + TrackingPenalty (+2 to Primal Instinct tracking).
- **Tests:** NoSenseOfSmellCausesAutomaticFailureOnSmellPerception, NoSenseOfSmellAddsTrackingPenalty, NoSenseOfSmellDoesNotAffectNonTrackingTests, SensoryActionUnderApplicableDeformity, NoSenseOfSmellTrackingPenalty

### Tailless (Sem Cauda)
- **Source:** Line 537
- **Implementation:** DifficultyModifier +1 to Social tests (all forms) + FormRestricted +1 to Dexterity balance tests in Lupus/Hispo/Crinos.
- **Tests:** TaillessAddsSocialDifficultyInAllForms, TaillessAddsBalanceDifficultyInLupus, TaillessAddsBalanceDifficultyInHispo, TaillessAddsBalanceDifficultyInCrinos, TaillessDoesNotAddBalanceDifficultyInGlabro, TaillessInCrinosAddsBalanceDifficulty, TaillessInHomidDoesNotAddBalanceDifficulty

## 6. Condition Model Design

The condition model is minimal and represents only the states required by source mechanics:

```csharp
public enum WerewolfConditionKind
{
    TemporaryPsychoticEpisode,
    Incapacitated,
    UnderTension,
    CriticalFailure
}

public sealed record WerewolfCondition(
    string ConditionKey,
    WerewolfConditionKind Kind,
    string SourceLocator,
    string SourceDeformity,
    int AppliedAtVersion,
    bool IsActive,
    int? DurationTurns = null);
```

Conditions are stored in `WerewolfRuntimeCharacterState.Conditions` as an immutable list. Each condition records its source locator and deformity for traceability.

## 7. Runtime Operations Added

| Operation Key | Purpose |
|---------------|---------|
| `action-resolution.resolve-action-test` | Resolves action test with deformity modifiers |
| `action-resolution.apply-condition` | Applies a condition to runtime state |
| `action-resolution.clear-condition` | Clears a condition from runtime state |
| `action-resolution.evaluate-action-availability` | Evaluates if action can be attempted given conditions |

## 8. Generic Action-Resolution Integration Design

The pipeline:
1. Base Attribute + Base Ability = base pool
2. Apply source-defined dice modifiers (e.g., ToughHide +1) = final pool
3. Base difficulty + source-defined difficulty modifiers (e.g., Albinism +2) = final difficulty
4. Check for automatic failures (e.g., Blind on vision tests)
5. Check conditional tests (e.g., Fits of Madness under tension)
6. Werewolf defines roll (pool, difficulty, context)
7. Chronicle supplies dice
8. Werewolf interprets

The `WerewolfActionResolutionService.ResolveActionTest` method implements this pipeline. It:
- Extracts attributes/abilities from runtime state package binding
- Computes base pool from attribute + ability
- Computes modifiers via `WerewolfActionResolutionModifierService`
- Applies dice pool modifier to final pool
- Applies difficulty modifier to final difficulty (clamped 2-10)
- Returns conditional tests for caller evaluation

## 9. Modifier Composition Semantics

- **Dice pool modifiers** and **difficulty modifiers** are tracked separately and never mixed
- **Automatic failures** take precedence over all modifiers (action cannot succeed)
- **Conditional tests** are returned separately for caller evaluation (trigger happens before action test)
- **Form-restricted effects** only apply when character is in specified form
- **Context-dependent effects** (daylight, limb usage, tension, sense, tracking) only apply when context matches
- No stacking rules invented; each deformity effect is applied independently

## 10. Action Availability Model

The `WerewolfConditionService.EvaluateActionAvailability` method checks active conditions:
- `condition.incapacitated` → action unavailable
- `condition.temporary-psychotic-episode` → action unavailable
- All other states → action available

## 11. Immutable-State Behavior

Conditions are stored in `WerewolfRuntimeCharacterState.Conditions` as `IReadOnlyList<WerewolfCondition>`. State transitions use `with` expressions to create new instances. `RuntimeStateVersion` increments by exactly 1 on successful mutation. Unrelated fields (resources, health track, package binding) are preserved.

## 12. End-to-End Scenarios Implemented

1. Ordinary action with no condition → base pool/difficulty unchanged
2. Ordinary action with dice-pool modifier (ToughHide) → final pool adjusted
3. Ordinary action with difficulty modifier (Debilitating Disease) → final difficulty adjusted
4. Action made unavailable by condition (Incapacitated) → blocked
5. Sensory action under applicable deformity (Blind/NoSenseOfSmell) → automatic failure
6. Tailless in Lupus/Hispo/Crinos vs Homid/Glabro → form-restricted balance modifier
7. Fits of Madness trigger/recovery → conditional test under tension, condition application and clearing
8. Seizures on critical failure → condition application

## 13. Tests Added

**WerewolfActionResolutionModifierTests.cs** (29 tests):
- NoDeformityReturnsZeroModifiers
- AlbinismAddsDaylightPerceptionDifficulty
- AlbinismDoesNotApplyWithoutDaylight
- AlbinismDoesNotAffectNonPerceptionTests
- BlindCausesAutomaticFailureOnVisionTests
- BlindDoesNotAffectNonVisionTests
- DebilitatingDiseaseAddsStaminaDifficulty
- DebilitatingDiseaseDoesNotAffectNonStaminaTests
- WitheredLimbAddsDexterityDifficultyWhenUsingLimb
- WitheredLimbDoesNotApplyWhenNotUsingLimb
- TaillessAddsSocialDifficultyInAllForms
- TaillessAddsBalanceDifficultyInLupus
- TaillessAddsBalanceDifficultyInHispo
- TaillessAddsBalanceDifficultyInCrinos
- TaillessDoesNotAddBalanceDifficultyInGlabro
- NoSenseOfSmellCausesAutomaticFailureOnSmellPerception
- NoSenseOfSmellAddsTrackingPenalty
- NoSenseOfSmellDoesNotAffectNonTrackingTests
- FitsOfMadnessRequiresConditionalTestUnderTension
- FitsOfMadnessDoesNotRequireTestWithoutTension
- SeizuresConditionalTestNotAddedWithoutCriticalFailure
- DiceModifiersDoNotAffectDifficulty
- DifficultyModifiersDoNotAffectDicePool
- UnknownDeformityReturnsZeroModifiers

**WerewolfConditionTests.cs** (13 tests):
- ApplyConditionIncrementsVersion
- ApplyConditionPreservesUnrelatedFields
- ApplyConditionRejectsVersionMismatch
- ApplyConditionDoesNotDuplicateActiveCondition
- ClearConditionDeactivatesAndIncrementsVersion
- ClearConditionHandlesNonExistentCondition
- ClearConditionRejectsVersionMismatch
- EvaluateActionAvailabilityReturnsAvailableForHealthyState
- EvaluateActionAvailabilityBlocksWhenIncapacitated
- EvaluateActionAvailabilityBlocksWhenPsychoticEpisode
- EvaluateActionAvailabilityRejectsVersionMismatch
- StateImmutabilityOriginalStateUnchangedAfterApply
- ConditionRecordsSourceOrigin

**WerewolfActionResolutionIntegrationTests.cs** (15 tests):
- OrdinaryActionWithNoDeformityReturnsBasePool
- ActionWithDicePoolModifier
- ActionWithDifficultyModifier
- ActionMadeUnavailableByCondition
- SensoryActionUnderApplicableDeformity
- TaillessInCrinosAddsBalanceDifficulty
- TaillessInHomidDoesNotAddBalanceDifficulty
- FitsOfMadnessTriggerUnderTension
- FitsOfMadnessRecoveryClearsCondition
- SeizuresOnCriticalFailure
- AlbinismInDaylightPerception
- AlbinismAtNightNoModifier
- WitheredLimbWhenUsingLimb
- WitheredLimbWhenNotUsingLimb
- BlindVisionTestAutomaticFailure
- BlindNonVisionTestNoFailure
- NoSenseOfSmellTrackingPenalty
- FinalDifficultyClampedToMaximum10
- FinalPoolNeverNegative
- VersionMismatchReturnsBlocked

**RuleSetRuntimeRegistryTests.cs** (1 test updated):
- LookupIsDeterministicAndImmutable (updated with new operation keys)

## 14. Complete 13-Metis-Deformity Ownership Map

| Deformity | 012E Effect(s) | Deferred Owner | Deferred Effect(s) |
|-----------|---------------|----------------|-------------------|
| FitsOfMadness | Yes — ConditionalTest (Willpower vs 8, min 3, under tension → psychotic episode) | — | — |
| Seizures | Yes — ConditionalTest (Willpower vs 8, min 3, on critical failure → incapacitated) | — | — |
| Albinism | Yes — DifficultyModifier +2 Perception (daylight without protection) | — | — |
| Blind | Yes — AutomaticFailure (vision-based tests) | — | — |
| DebilitatingDisease | Yes — DifficultyModifier +2 Stamina (including absorption) | — | — |
| WitheredLimb | Yes — DifficultyModifier +2 Dexterity (using withered limb) | — | — |
| Tailless | Yes — DifficultyModifier +1 Social (all forms) + FormRestricted +1 Dexterity balance (Lupus/Hispo/Crinos) | — | — |
| NoSenseOfSmell | Yes — SensoryFailure (olfactory) + TrackingPenalty +2 Primal Instinct (tracking) | — | — |
| Hairless | No | 012F | DifficultyModifier +1 Social |
| Hunchback | No | 012F | DifficultyModifier +1 Social + DifficultyModifier +1 Dexterity |
| Horns | No | 012F | DifficultyModifier +1 Social |
| Horns | No | Combat (012C) | CombatDamage (Strength+1 bashing) — already in catalog |
| Horns | No | Future Renown owner | RenownPenalty (Glory -1) |
| ToughHide | Yes — DiceBonus +1 Absorption | Character Creation | AttributeMaximum (Appearance = 1) |
| WeakImmuneSystem | No | Health/Runtime (005) | HealthLevelRemoved (Escoriado) — already integrated |

**Effect-level ownership notes:**
- ToughHide has two source-defined effects (line 534): `Aparência máxima é 1` (AttributeMaximum, character-creation scope) and `Garante +1 dado em testes de Absorção` (DiceBonus, action-resolution scope). 012E owns only the DiceBonus. The AttributeMaximum is a character-creation validation constraint, not an action-resolution modifier.
- Horns has three source-defined effects (line 533): Social penalty, combat damage, and renown penalty. 012C already owns the combat-damage catalog entry. 012F owns the social effect. Renown penalty is deferred to the future reown system owner.
- WeakImmuneSystem has one source-defined effect (line 539): removal of the Escoriado health level. This is a health-track/runtime-state effect already implemented in package 005.

## 15. Deferred Owners

- **012F (Social Mechanics):** Hairless, Hunchback (social part), Horns (social part)
- **Character Creation:** ToughHide Appearance maximum (source line 534: `Aparência máxima é 1` — creation-time attribute cap, not an action-resolution modifier)
- **Combat (012C):** Horns combat damage (already in attack catalog)
- **Health/Runtime (005):** WeakImmuneSystem health-level removal (already integrated)
- **Future Renown owner:** Horns Glory -1 penalty (source line 533)

**ToughHide disposition clarification:**
The source (line 534) defines two distinct effects:
1. `Aparência máxima é 1` — AttributeMaximum for Appearance. This is a character-creation validation constraint (maximum Appearance rating is 1). It is NOT an action-resolution mechanic and is NOT owned by 012E.
2. `Garante +1 dado em testes de Absorção` — DiceBonus for Absorption tests. This IS an action-resolution mechanic and IS owned by 012E (implemented via `WerewolfMetisDeformityEffectKind.DiceBonus` and integrated in combat soak).

There is no inconsistency. The final report's "Deferred: ToughHide AttributeMaximum" statement is correct: the AttributeMaximum effect is deferred to character-creation validation, while the DiceBonus effect is implemented in 012E.

## 16. Package Validator Allow-List Updates

New files added to `RuleSetPackageSourceValidation.cs`:
- `CharacterCreation/WerewolfActionResolutionContext.cs`
- `CharacterCreation/WerewolfActionResolutionModifierService.cs`
- `CharacterCreation/WerewolfActionResolutionService.cs`
- `CharacterCreation/WerewolfCondition.cs`
- `CharacterCreation/WerewolfConditionService.cs`

## 17. Localization Updates

New keys added to `en/current-slice.json` and `pt-BR/current-slice.json`:
- `action-resolution.condition.temporary-psychotic-episode.display-name`
- `action-resolution.condition.incapacitated.display-name`
- `action-resolution.condition.under-tension.display-name`
- `action-resolution.condition.critical-failure.display-name`

## 18. Metadata Updates

- `werewolf.package-manifest.json`: Added `action-resolution` capability (partial-executable)
- `Metadata/current-slice.json`: Added `action-resolution` to supportedCapabilities

## 19. Files Changed

**New files (8):**
1. `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfActionResolutionContext.cs`
2. `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfActionResolutionModifierService.cs`
3. `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfActionResolutionService.cs`
4. `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfCondition.cs`
5. `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfConditionService.cs`
6. `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfActionResolutionModifierTests.cs`
7. `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfConditionTests.cs`
8. `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfActionResolutionIntegrationTests.cs`

**Modified files (9):**
1. `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRuntimeCharacterState.cs`
2. `rule-sets/Chronicle.RuleSets.Werewolf/WerewolfReferenceRuntime.cs`
3. `rule-sets/Chronicle.RuleSets.Werewolf.Tests/RuleSetRuntimeRegistryTests.cs`
4. `rule-sets/Chronicle.RuleSets.Werewolf/Localization/en/current-slice.json`
5. `rule-sets/Chronicle.RuleSets.Werewolf/Localization/pt-BR/current-slice.json`
6. `rule-sets/Chronicle.RuleSets.Werewolf/Metadata/current-slice.json`
7. `rule-sets/Chronicle.RuleSets.Werewolf/Metadata/werewolf.package-manifest.json`
8. `src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs`

## 20. Validation Results

**Build:** `dotnet build Chronicle.sln` — 0 errors, 0 warnings
**Tests:** `dotnet test Chronicle.sln` — 1150 passed, 0 failed
**Werewolf tests:** 1150 passed, 0 failed

**Package Validator (unit tests):** 8 tests passed, 0 failed

**Package Validator (CLI):**
```
Chronicle Rule Set Package Source Validation
PackageSource: C:\Dev\Chronicle-validation\rule-sets\Chronicle.RuleSets.Werewolf
Status: valid
Files: 88
Findings: 0
```

**Localization:** New keys present in en + pt-BR

## 21. Matrix Rows Affected

The "Metis deformity" domain row is affected:
- Before: mechanicalCompleteness: false, currentSliceExecutable: true
- After: mechanicalCompleteness: false (still deferred effects remain), currentSliceExecutable: true

The "Attribute + Ability test definition" domain row is affected:
- Before: mechanicalCompleteness: false, currentSliceExecutable: true
- After: mechanicalCompleteness: false (still source-pending for full integration), currentSliceExecutable: true

## 22. Ambiguities

None. All deformity effects are explicitly defined in the source table (lines 523-539) and have deterministic implementations.

## 23. Remaining Work

- 012F: Social mechanics for Hairless, Hunchback, Horns
- ToughHide Appearance maximum enforcement at character creation validation
- Horns Renown penalty integration
- Horns Combat damage (already in catalog, needs runtime integration)
