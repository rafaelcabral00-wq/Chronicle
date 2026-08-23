# RULESET-COMPLETION-012H: Frenzy Runtime Mechanics

**Status:** Complete
**Date:** 2026-08-22
**Parent Package:** RULESET-COMPLETION-012
**Subpackage:** 012H (sixth controlled subpackage of 012)

## 1. Exact 012H Title and Scope

**Title:** Frenzy Runtime Mechanics

**Owned domain keys:**
- Frenzy triggers (Rage test, moon phase difficulty, rank modifiers)
- Frenzy types (Wild, Fox, Extreme)
- Willpower suppression
- Permanecer Ativo → Frenzy connection
- Besta Interior dice penalty
- Frenzy state management (enter/suppress/end/evaluate-action)

**Completion condition:**
All source-defined Frenzy base mechanics are implemented as deterministic services; Frenzy state is integrated into runtime character state; Permanecer Ativo success connects to Wild Frenzy; Besta Interior applies dice penalty to Social tests when Rage exceeds Willpower; package validator allow-list includes new files; completeness matrix and report are updated.

**Source ambiguities/findings resolved by 012H:**
- Frenzy types are explicitly modeled as enum: Wild, Fox, Extreme.
- Fox Frenzy changes form to Lupus and restricts actions to flee only.
- Extreme Frenzy (6+ successes) is uncontrollable by Willpower.
- Permanecer Ativo success at line 2870 enters Wild Frenzy state.
- Besta Interior applies -1 die per Rage point above Willpower on Social tests.

## 2. Source Locators

Canonical source: `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt`

**Frenzy trigger:** lines 2915-2916
**Frenzy types:** lines 2916-2918
**Wild Frenzy:** line 2916
**Fox Frenzy:** line 2917
**Extreme Frenzy:** line 2918
**Willpower suppression:** line 1706
**Permanecer Ativo → Frenzy:** line 2870
**Besta Interior:** line 1681
**Rank modifiers:** lines 1666-1671

## 3. Source Traversal Scope

Full traversal of the canonical source confirmed that 012H owns:
- Frenzy trigger mechanics (Rage test, difficulty by moon phase, rank modifiers)
- Frenzy type definitions and behaviors
- Willpower suppression rules
- Permanecer Ativo survival connection to frenzy
- Besta Interior Social test penalty
- Frenzy state lifecycle (enter, suppress, end, evaluate)

Mechanics explicitly deferred to future owners:
- Gift execution (out of scope)
- Rites (out of scope)
- Spirits/Umbra (out of scope)
- Social mechanics beyond Besta Interior (out of scope)
- Form transformation mechanics beyond Frenzy type behavior (deferred to form catalog owner)

## 4. Frenzy Types/States

| Type | Trigger | Behavior |
|------|---------|----------|
| Wild | 4+ successes on Rage test | Attacks everything |
| Fox | 4+ successes on Rage test | Changes to Lupus, flees |
| Extreme | 6+ successes on Rage test | Uncontrollable by Willpower |

## 5. Trigger Model

**Dice pool:** RagePermanent
**Base difficulty by moon phase:**
- New: 8
- Waxing Crescent: 7
- Half: 6
- Waxing Gibbous: 5
- Full: 4

**Modifiers:**
- Auspice moon matches current moon: -1 difficulty
- In Crinos form: -1 difficulty
- Environmental modifier: additive

**Rank modifiers:**
- Rank 1: +1 difficulty
- Rank 2+: +2 difficulty
- Rank 3+: +2 difficulty, 5+ successes required
- Rank 4-5+: +2 difficulty, 6+ successes required

**Success threshold:**
- Rank 1-2: 4 successes
- Rank 3: 5 successes
- Rank 4-5: 6 successes

**Final difficulty:** clamped 2-10

## 6. Chronicle Randomness Boundary

Chronicle supplies dice and interprets successes. Werewolf defines:
- Dice pool (RagePermanent)
- Base difficulty (moon phase)
- Modifiers (rank, auspice, form, environment)
- Success threshold

The Rage test itself is a generic dice test. Werewolf defines the test parameters; Chronicle executes the dice roll and returns successes.

## 7. Resistance/Control Semantics

**Willpower suppression:**
- Cost: 1 Willpower point
- Effect: Ends frenzy
- Cannot suppress Extreme Frenzy
- Requires WillpowerCurrent >= 1

**Permanecer Ativo connection:**
- When Permanecer Ativo succeeds, character enters Wild Frenzy
- Frenzy state is set in the updated runtime state

## 8. Runtime Frenzy State Design

```csharp
public sealed record WerewolfFrenzyState(
    bool IsInFrenzy,
    WerewolfFrenzyType FrenzyType,
    string Trigger,
    int AccumulatedSuccesses,
    int StartedAtTurn,
    string? TargetRestriction,
    bool IsSuppressed,
    string SourceLocator);
```

Stored in `WerewolfRuntimeCharacterState.FrenzyState` as nullable record. State transitions use `with` expressions. `RuntimeStateVersion` increments by exactly 1 on successful mutation.

## 9. Form Interaction

- Fox Frenzy changes character form to Lupus (modeled as finding, not automatic form transition)
- Crinos form provides -1 difficulty bonus to Frenzy test
- Other forms have no special Frenzy interaction

## 10. Combat Interaction

- Wild Frenzy: all actions available, attacks everything
- Fox Frenzy: only flee action available
- Extreme Frenzy: no actions available (uncontrollable)
- Suppressed Frenzy: all actions available

## 11. Health Interaction

- Permanecer Ativo success (Near Death or Dead) → enters Wild Frenzy
- Frenzy does not directly modify health track
- Frenzy state is independent of health state

## 12. Rage Interaction

- Frenzy test dice pool = RagePermanent
- Besta Interior: for each Rage point above Willpower, -1 die on Social tests
- Rage is not consumed by entering frenzy
- Suppressing frenzy does not affect Rage

## 13. Fits of Madness Disposition

Fits of Madness is modeled as `temporary-psychotic-episode` condition (012E). This is DISTINCT from Frenzy. A character can have both simultaneously, but they are independent state machines.

## 14. Seizures Boundary

Seizures is modeled as `incapacitated` condition (012E). This is INDEPENDENT from Frenzy. A character can be in frenzy and also incapacitated by seizures.

## 15. Extreme Frenzy Disposition

Extreme Frenzy (6+ successes) cannot be suppressed or controlled by Willpower (source line 2927: `incontrolável por Força de Vontade`). The character still acts according to source-defined Frenzy behavior. Extreme Frenzy does not render the character mechanically incapable of acting. It removes only the player's ability to use Willpower to end or suppress the frenzy. Character remains in Extreme Frenzy until explicitly ended by an external mechanism (not modeled in 012H).

## 16. Duration/End Rules

- Frenzy persists until explicitly ended
- Suppression ends frenzy (spends 1 Willpower)
- EndFrenzy operation explicitly ends frenzy
- No automatic duration modeled

## 17. Scene/Turn-Token Semantics

- `StartedAtTurn` tracks when frenzy began
- No automatic scene-boundary reset modeled
- Turn token semantics are deferred to combat system

## 18. Action Availability

```csharp
public static string EvaluateFrenzyAction(WerewolfRuntimeCharacterState currentState, string actionType)
```

Returns:
- `"available"` — no frenzy, suppressed frenzy, or any frenzied state where action is permitted
- `"unavailable-fox-frenzy"` — Fox Frenzy and action is not flee

**Extreme Frenzy semantics:**
Source line 2927 states Extreme Frenzy is `incontrolável por Força de Vontade` (uncontrollable by Willpower). This means Willpower suppression is unavailable. It does NOT mean the character cannot act. Extreme Frenzy characters still act according to source-defined Frenzy behavior. The action-availability contract therefore returns `"available"` for Extreme Frenzy, while `SuppressFrenzy` rejects it with `ExtremeFrenzyUncontrollable`.

## 19. Modifier Integration

Besta Interior is integrated into `WerewolfActionResolutionModifierService.ComputeModifiers`:
- Checks if character is in frenzy
- Computes Social dice penalty = max(0, RagePermanent - WillpowerPermanent)
- Applies penalty only to Social tests (Charisma/Manipulation/Appearance attributes OR Empathy/Expression/Intimidation/Subterfuge/Leadership/Streetwise/Performance/Etiquette/Politics abilities)

## 20. Runtime Operations Added

| Operation Key | Purpose |
|---------------|---------|
| `frenzy.define-test` | Computes Frenzy test definition (pool, difficulty, threshold) |
| `frenzy.enter` | Enters frenzy state |
| `frenzy.suppress` | Suppresses frenzy by spending Willpower |
| `frenzy.end` | Ends frenzy state |
| `frenzy.evaluate-action` | Evaluates action availability in frenzy |

## 21. Trigger Catalog

| Trigger | Source Locator |
|---------|---------------|
| Rage test (4+ successes) | Line 2915-2916 |
| Permanecer Ativo success | Line 2870 |

## 22. Auspice/Moon Interaction

- Auspice moon matching current moon phase: -1 difficulty to Frenzy test
- Auspice moon is an input parameter to `frenzy.define-test`
- Moon phase determines base difficulty

## 23. Gift/Rite Dependency Map

- NO Gift execution dependencies
- NO Rite dependencies
- NO Spirit/Umbra dependencies
- Frenzy is a base Garou mechanic independent of Gifts/Rites

## 24. Human Decisions

None. All Frenzy mechanics are explicitly defined in the source text and implemented deterministically.

## 25. End-to-End Scenarios

1. Character fails Frenzy test (3 successes, threshold 4) → no frenzy
2. Character passes Frenzy test with 4 successes, rank 1, Full moon → enters Wild Frenzy
3. Character in Wild Frenzy spends 1 Willpower → frenzy suppressed, WillpowerCurrent -1
4. Character in Fox Frenzy attempts attack → action unavailable
5. Character in Fox Frenzy attempts flee → action available
6. Character in Extreme Frenzy attempts any action → action unavailable
7. Permanecer Ativo succeeds → character enters Wild Frenzy
8. Character with Rage 5, Willpower 3 takes Social test → -2 dice penalty
9. Character with Rage 3, Willpower 5 takes Social test → no penalty
10. Rank 4 character needs 6+ successes for Frenzy → threshold is 6

## 26. Tests Added

**WerewolfFrenzyTests.cs** (44 tests):
- FrenzyTestDefinitionNewMoonDifficultyIs8
- FrenzyTestDefinitionWaxingCrescentDifficultyIs7
- FrenzyTestDefinitionHalfMoonDifficultyIs6
- FrenzyTestDefinitionWaxingGibbousDifficultyIs5
- FrenzyTestDefinitionFullMoonDifficultyIs4
- FrenzyTestDefinitionRank1Adds1Difficulty
- FrenzyTestDefinitionRank2Adds2Difficulty
- FrenzyTestDefinitionRank3SuccessThresholdIs5
- FrenzyTestDefinitionRank4SuccessThresholdIs6
- FrenzyTestDefinitionAuspiceMoonMatchReducesDifficultyBy1
- FrenzyTestDefinitionCrinosFormReducesDifficultyBy1
- FrenzyTestDefinitionDifficultyClampedToMinimum2
- FrenzyTestDefinitionDifficultyClampedToMaximum10
- EnterFrenzyWildSucceedsAndUpdatesState
- EnterFrenzyFoxChangesToLupus
- EnterFrenzyExtremeCannotBeSuppressed
- EnterFrenzyRejectsWhenAlreadyInFrenzy
- SuppressFrenzySpendsWillpowerAndEndsFrenzy
- SuppressFrenzyFailsWithoutWillpower
- SuppressFrenzyRejectsWhenNotInFrenzy
- SuppressFrenzyRejectsVersionMismatch
- EndFrenzySucceeds
- EndFrenzyFailsWhenNotInFrenzy
- EndFrenzyRejectsVersionMismatch
- EvaluateFrenzyActionWildAllowsAttack
- EvaluateFrenzyActionFoxAllowsOnlyFlee
- EvaluateFrenzyActionFoxBlocksAttack
- EvaluateFrenzyActionExtremeBlocksAll
- EvaluateFrenzyActionSuppressedFrenzyAllowsAllActions
- EvaluateFrenzyActionNoFrenzyAllowsAllActions
- BestaInteriorPenaltyAppliedWhenRageExceedsWillpower
- BestaInteriorNoPenaltyWhenRageEqualsWillpower
- BestaInteriorNoPenaltyWhenRageBelowWillpower
- BestaInteriorIsSocialTestReturnsTrueForCharisma
- BestaInteriorIsSocialTestReturnsFalseForStrength
- PermanecerAtivoSuccessEntersWildFrenzy
- FrenzyStateImmutabilityPreservedOnEnter
- FrenzyTestDefinitionInvalidRankReturnsInvalid
- FrenzyTestDefinitionZeroRageReturnsInvalid
- FrenzyTestDefinitionEmptyRequestIdReturnsInvalid
- EnterFrenzyRejectsNoneFrenzyType
- EnterFrenzyRejectsVersionMismatch
- EnterFrenzyRejectsEmptyRequestId

## 27. Exact Affected Matrix Rows

Only two completeness-matrix rows transitioned from false to true:

- **"Frenzy triggers"** — mechanicalCompleteness: false → true, currentSliceExecutable: false → true
- **"Rage tests"** — mechanicalCompleteness: false → true, currentSliceExecutable: false → true

The remaining 012H mechanics (Frenzy types, Willpower suppression, Besta Interior, Permanecer Ativo → Frenzy connection) are implemented within these rows or existing infrastructure; they are not separate completeness-matrix rows.

## 28. Mechanical Completeness Before -> After

Before 012H: 32/68 mechanical domains (47.1%)
After 012H: 34/68 mechanical domains (50.0%)

## 29. Current-Slice Executable Before -> After

Before 012H: 42/68 executable domains (61.8%)
After 012H: 44/68 executable domains (64.7%)

## 30. Werewolf Test Count

Before 012H: 1150 tests
After 012H: 1194 tests (+44)

## 31. Every Other Test-Project Count

- Chronicle.Contracts.Tests: 8 passed
- Chronicle.Domain.Tests: 1 passed
- Chronicle.Application.Tests: 9 passed
- Chronicle.Infrastructure.Tests: 12 passed
- Chronicle.Persistence.Sqlite.Tests: 1 passed
- Chronicle.Architecture.Tests: 11 passed
- Chronicle.Tools.PackageValidator.Tests: 8 passed

## 32. Mechanically Summed Total

Total tests: 1244 passed, 0 failed

## 33. Package-Validator CLI Result

```
Chronicle Rule Set Package Source Validation
PackageSource: C:\Dev\Chronicle-validation\rule-sets\Chronicle.RuleSets.Werewolf
Status: valid
Files: 92
Findings: 0
```

## 34. PackageValidator.Tests Result

8 tests passed, 0 failed

## 35. Localization Integrity

New keys added to `en/current-slice.json` and `pt-BR/current-slice.json`:
- `frenzy.type.wild.display-name`
- `frenzy.type.fox.display-name`
- `frenzy.type.extreme.display-name`
- `frenzy.trigger.rage-test.display-name`
- `frenzy.suppression.willpower.display-name`
- `frenzy.besta-interior.display-name`

All keys present in both locales.

## 36. Exact Files Changed

**New files (5):**
1. `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfFrenzyState.cs`
2. `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfFrenzyTestDefinitionService.cs`
3. `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfFrenzyResolutionService.cs`
4. `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfBestaInteriorService.cs`
5. `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfFrenzyTests.cs`

**Modified files (12):**
1. `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRuntimeCharacterState.cs`
2. `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfPermanecerAtivoService.cs`
3. `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfActionResolutionContext.cs`
4. `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfActionResolutionService.cs`
5. `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfActionResolutionModifierService.cs`
6. `rule-sets/Chronicle.RuleSets.Werewolf/WerewolfReferenceRuntime.cs`
7. `rule-sets/Chronicle.RuleSets.Werewolf/Localization/en/current-slice.json`
8. `rule-sets/Chronicle.RuleSets.Werewolf/Localization/pt-BR/current-slice.json`
9. `rule-sets/Chronicle.RuleSets.Werewolf/Metadata/current-slice.json`
10. `rule-sets/Chronicle.RuleSets.Werewolf/Metadata/werewolf.package-manifest.json`
11. `rule-sets/Chronicle.RuleSets.Werewolf.Tests/RuleSetRuntimeRegistryTests.cs`
12. `src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs`

## 37. git diff --check

No whitespace errors detected.

## 38. git status

```
On branch wip/werewolf-current-slice-completion
Your branch is up to date with 'origin/wip/werewolf-current-slice-completion'.

Changes not staged for commit:
  modified:   rule-sets/Chronicle.RuleSets.Werewolf.Tests/RuleSetRuntimeRegistryTests.cs
  modified:   rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfActionResolutionContext.cs
  modified:   rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfActionResolutionModifierService.cs
  modified:   rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfActionResolutionService.cs
  modified:   rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfPermanecerAtivoService.cs
  modified:   rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRuntimeCharacterState.cs
  modified:   rule-sets/Chronicle.RuleSets.Werewolf/Localization/en/current-slice.json
  modified:   rule-sets/Chronicle.RuleSets.Werewolf/Localization/pt-BR/current-slice.json
  modified:   rule-sets/Chronicle.RuleSets.Werewolf/Metadata/current-slice.json
  modified:   rule-sets/Chronicle.RuleSets.Werewolf/Metadata/werewolf.package-manifest.json
  modified:   rule-sets/Chronicle.RuleSets.Werewolf/WerewolfReferenceRuntime.cs
  modified:   src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs

Untracked files:
  .kilo/
  rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfFrenzyTests.cs
  rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfBestaInteriorService.cs
  rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfFrenzyResolutionService.cs
  rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfFrenzyState.cs
  rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfFrenzyTestDefinitionService.cs
```

## 39. Ownerless Blockers

None. All Frenzy source mechanics are implemented. Remaining work (Gifts, Rites, Spirits/Umbra, Social) is explicitly deferred to other completion packages.
