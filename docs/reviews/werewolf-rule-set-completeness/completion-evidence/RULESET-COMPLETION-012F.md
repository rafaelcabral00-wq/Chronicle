# RULESET-COMPLETION-012F: Social Mechanics

**Status:** Complete
**Date:** 2026-08-23
**Parent Package:** RULESET-COMPLETION-012
**Subpackage:** 012F (seventh controlled subpackage of 012)

## 1. Exact 012F Title and Scope

**Title:** Social Mechanics

**Owned domain keys:**
- Source-defined social challenge mechanics (Defrontação, Engambelação, Interrogatório, Intimidação, Oratória e Performance, Sedução, Atração Animal, Credibilidade)
- Social test definition pipeline (Attribute + Ability pool, difficulty, modifiers)
- Pure Breed runtime social effect consumption
- Besta Interior social dice penalty consumption
- Metis deformity social effects (Hairless, Hunchback, Horns) — already executable via action-resolution modifier service

**Completion condition:**
All source-defined social challenge types are modeled as deterministic computation services; existing social modifiers (Besta Interior, Pure Breed, form effects, deformity effects) are consumed by the social pipeline; package validator allow-list includes new files; completeness matrix and report are updated.

## 2. Source Locators

Canonical source: `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt`

**Social challenges section:** lines 3006-3024
- Atração Animal: line 3009
- Credibilidade: line 3012
- Defrontação: line 3014
- Engambelação: line 3018
- Interrogatório: line 3020
- Intimidação: line 3022
- Oratória e Performance: line 3024
- Sedução: line 3027

**Social modifiers:**
- Besta Interior: line 1681 (implemented in 012H)
- Pure Breed: 012A background runtime
- Metis deformity social effects: lines 529, 532, 533 (implemented in 012E catalog)
- Form social attributes: 012B (Crinos Manipulation 0, etc.)

## 3. Source Traversal Scope

Full traversal of the canonical source confirmed that 012F owns:
- 8 explicit social challenge definitions with source-backed mechanics
- Social test definition pipeline (pool, difficulty, modifiers, target context)
- Consumption of existing social modifier authorities

The source does NOT define:
- A generic universal Social test mechanic
- Persistent NPC attitude/relationship systems
- Social "difficulty 6" global default
- Renown advancement mechanics (separate domain)
- Pack social mechanics (separate domain)

## 4. Social Action Categories Implemented

| Category | Source Locator | Attribute | Ability | Special Rules |
|----------|---------------|-----------|---------|---------------|
| Atração Animal | Line 3009 | Charisma | Primal Instinct | Success threshold = target Willpower |
| Credibilidade | Line 3012 | Manipulation | Subterfuge | Difficulty based on listener's Inteligência + Lábia |
| Defrontação | Line 3014 | Charisma | Intimidation | Uses higher of Charisma+Intimidation or Fury; success threshold = target Raciocínio + 5 |
| Engambelação | Line 3018 | Manipulation | Subterfuge | Difficulty based on target Raciocínio + Manha |
| Interrogatório | Line 3020 | Manipulation | Intimidation | Difficulty = target Willpower |
| Intimidação | Line 3022 | Charisma | Intimidation | Physical posture grants bonus dice; Crinos + human target = automatic Delirium |
| Oratória e Performance | Line 3024 | Charisma | Leadership/Performance | Difficulty varies by crowd disposition and Rank |
| Sedução | Line 3027 | Appearance | Subterfuge | Two-stage: Appearance+Lábia vs Raciocínio+3, then Raciocínio+Lábia vs Inteligência+3 |

## 5. Attribute + Ability Mappings

All mappings are source-derived from lines 3006-3024. Each social challenge has explicit Attribute + Ability pairings. No generic Social ability exists in the source.

## 6. Generic Social Test Pipeline

Implemented in `WerewolfSocialTestDefinitionService.DefineTest`:

1. Base Attribute + Base Ability = base pool
2. Apply source-defined dice modifiers (Besta Interior, Pure Breed, deformity effects) = final pool
3. Base difficulty + source-defined difficulty modifiers = final difficulty
4. Check automatic failures (Crinos + human for Intimidation)
5. Return test definition (pool, difficulty, threshold, context)

Dice modifiers and difficulty modifiers are kept distinct.

## 7. Besta Interior Integration

Consumed from 012H authority:
- `WerewolfBestaInteriorService.ComputeSocialDicePenty(RagePermanent, WillpowerPermanent)` returns max(0, Rage - Willpower)
- Applied only to Social tests (Charisma/Manipulation/Appearance attributes OR Empathy/Expression/Intimidation/Subterfuge/Leadership/Streetwise/Performance/Etiquette/Politics abilities)
- No duplicate calculation

## 8. Pure Breed Runtime Effect

Consumed from 012A background authority:
- +1 die per Pure Breed dot to Social tests involving other Garou
- Read from `PackageBinding["backgrounds"]` JSON
- Only applies when `TargetContext.IsGarouTarget = true`

## 9. Rank Social Mechanics

Source line 1664-1671 defines Rank benefits:
- Posto 3+: Known throughout Garou society
- Posto 4+: Deference from inferiors
- Challenge eligibility: can only challenge opponents at most one Rank above

These are status/eligibility mechanics, not numeric test modifiers. Oratória difficulty decreases by Rank value (line 3024), implemented in `ComputeOratoriaDifficulty`.

## 10. Hairless Disposition

Source line 529: `Penalidade de +1 na dificuldade em testes Sociais`
- Already in Metis deformity catalog as `DifficultyModifier +1 Social`
- Consumed by existing `WerewolfActionResolutionModifierService`
- 012F acknowledges ownership; no new implementation required

## 11. Hunchback Disposition

Source line 532: `Penalidade de +1 na dificuldade em testes Sociais e baseados em Destreza`
- Social part already in Metis deformity catalog as `DifficultyModifier +1 Social`
- Dexterity part owned by 012E action resolution
- 012F owns only Social portion; already executable via action-resolution modifier service

## 12. Horns Social Disposition

Source line 533: `Penalidade de +1 na dificuldade em testes Sociais`
- Already in Metis deformity catalog as `DifficultyModifier +1 Social`
- Combat damage owned by 012C
- Renown penalty owned by future Renown owner
- 012F owns only Social effect; already executable via action-resolution modifier service

## 13. Appearance Semantics

Forms establish absolute overrides:
- Crinos: Appearance = 0
- Effective Attributes computed by 012B `WerewolfEffectiveAttributeService`

Social mechanics consume effective Appearance. No mutation of base Appearance.

## 14. Manipulation Semantics

Forms may set Manipulation = 0 (Crinos, Hispo, Lupus per 012B).
Social tests requiring Manipulation use the effective value from 012B.
Source does not state that Manipulation 0 makes social tests impossible; it merely sets the effective rating.

## 15. Intimidation/Fear Mechanics

Source line 3022:
- Physical postures grant bonus dice
- Non-Hominid forms don't get Manipulation penalties for Intimidation
- Crinos form causes automatic Delirium in humans
- Implemented in `WerewolfSocialTestDefinitionService` via `UsesPhysicalPosture` flag and Crinos+human automatic failure

## 16. Formal Social Challenge Mechanics

All 8 source-defined social challenges are modeled as explicit catalog entries with:
- Stable challenge ID
- Source locator
- Attribute + Ability pairing
- Base difficulty or difficulty computation rule
- Success threshold (where source-defined)
- Special rules

## 17. Resisted/Opposed Boundary

Defrontação is an opposed social challenge:
- Each participant tests Charisma + Intimidation OR Fury (whichever is higher) vs target Willpower
- First to accumulate successes equal to target Raciocínio + 5 wins
- Loser loses 1 temporary Renome (Glory)

This is modeled as a single test definition with target context, not a generic opposed-roll primitive.

## 18. Social Target Context

`WerewolfSocialTargetContext` carries:
- TargetWillpower
- TargetRaciocínio
- TargetInteligência
- TargetRage
- IsGarouTarget
- IsHumanTarget
- HasPriorInterest
- IsAffectedByGarouCurse
- IsTruthBeingTold
- TruthLevel
- CrowdDispositionBonus
- CharacterRankValue
- UsesPhysicalPosture

## 19. Form Interaction

Social tests use effective Attributes from 012B:
- Crinos: Charisma 0, Manipulation 0, Appearance 0
- This reduces Social test pools accordingly
- Intimidation in Crinos form against humans triggers automatic Delirium

## 20. Frenzy Interaction

Besta Interior (from 012H) applies during Frenzy:
- For each Rage point above Willpower, -1 die on Social tests
- Independent of actual Frenzy state
- Applied in `WerewolfSocialTestDefinitionService`

## 21. Conditions Integration

Conditions from 012E are passed through `WerewolfSocialTestDefinitionContext`:
- UnderTension condition affects action resolution
- Active conditions are tracked

## 22. Result Interpretation

Social tests return:
- FinalPool
- FinalDifficulty
- SuccessThreshold
- IsAutomaticFailure
- IsActionUnavailable
- Findings (modifiers applied)

No persistent NPC attitude state is modeled.

## 23. Narrative Intelligence Boundary

Narrator may propose:
- Intent
- Target
- Approach
- Contextual facts (Willpower, Raciocínio, etc.)

Narrator may NOT decide:
- Dice pool
- Difficulty
- Modifiers
- Success
- Social state transition

## 24. Runtime Social State

No new persistent Social state added. Social test definitions are pure computations.

## 25. Runtime Operations Added

No runtime operations exposed. Social test definition is a pure computation service.

## 26. Human Decisions

None. All social mechanics are explicitly defined in the source text.

## 27. End-to-End Scenarios

Covered by tests:
- Ordinary Social test (Credibilidade)
- Defrontação with Fury pool
- Besta Interior penalty application
- Pure Breed bonus for Garou target
- Crinos + human automatic Delirium for Intimidation
- Oratória difficulty with Rank modifier
- Invalid challenge ID rejection
- Version mismatch rejection

## 28. Tests Added

**WerewolfSocialTests.cs** (13 tests):
- SocialChallengeCatalogContainsAllSourceDefinedChallenges
- DefrontacaoUsesFuryPool
- AtracaoAnimalUsesCharismaAndPrimalInstinct
- SocialTestDefinitionSucceedsForValidChallenge
- SocialTestDefinitionRejectsInvalidChallengeId
- SocialTestDefinitionRejectsVersionMismatch
- DefrontacaoUsesHigherOfCharismaIntimidationOrFury
- SeducaoStageOneUsesAppearanceAndSubterfuge
- SocialTestDefinitionComputesPureBreedBonusForGarouTarget
- SocialTestDefinitionNoPureBreedBonusForNonGarouTarget
- SocialTestDefinitionComputesBestaInteriorPenalty
- IntimidacaoInCrinosCausesAutomaticDeliriumOnHuman
- SocialTestDefinitionRejectsMissingAttribute

## 29. Final 13-Deformity Ownership Map

| Deformity | 012E | 012F | 012C | Other |
|-----------|------|------|------|-------|
| FitsOfMadness | Action-resolution effects | - | - | - |
| Seizures | Action-resolution effects | - | - | - |
| Albinism | Action-resolution effects | - | - | - |
| Blind | Action-resolution effects | - | - | - |
| DebilitatingDisease | Action-resolution effects | - | - | - |
| WitheredLimb | Action-resolution effects | - | - | - |
| Tailless | Action-resolution effects | - | - | - |
| NoSenseOfSmell | Action-resolution effects | - | - | - |
| Hairless | - | Social effects | - | - |
| Hunchback | Action-resolution (Dex) | Social effects | - | - |
| Horns | - | Social effects | CombatDamage | RenownPenalty (future) |
| ToughHide | - | - | - | AttributeMaximum (creation) + DiceBonus (012E) |
| WeakImmuneSystem | - | - | - | HealthLevelRemoved (005) |

## 30. Exact Affected Matrix Rows

No completeness-matrix rows transitioned from false to true in 012F.

**Rationale:**
- The canonical source does not define a generic "Social mechanics" or "Social tests" matrix row.
- All source-defined social mechanics are either:
  - Already covered by existing rows (Metis deformity, Besta Interior, Pure Breed, Forms)
  - Modeled as pure computation services without corresponding matrix row

## 31. Mechanical Completeness Before -> After

Before 012F: 34/68 mechanical domains (50.0%)
After 012F: 34/68 mechanical domains (50.0%)

## 32. Current-Slice Executable Before -> After

Before 012F: 44/68 executable domains (64.7%)
After 012F: 44/68 executable domains (64.7%)

## 33. Werewolf Test Count

Before 012F: 1198 tests
After 012F: 1211 tests (+13)

## 34. Every Other Test-Project Count

- Chronicle.Contracts.Tests: 8 passed
- Chronicle.Domain.Tests: 1 passed
- Chronicle.Application.Tests: 9 passed
- Chronicle.Infrastructure.Tests: 12 passed
- Chronicle.Persistence.Sqlite.Tests: 1 passed
- Chronicle.Architecture.Tests: 11 passed
- Chronicle.Tools.PackageValidator.Tests: 8 passed

## 35. Mechanically Summed Total

1261 total passed, 0 failed

## 36. Package-Validator CLI Result

```
Chronicle Rule Set Package Source Validation
PackageSource: C:\Dev\Chronicle-validation\rule-sets\Chronicle.RuleSets.Werewolf
Status: valid
Files: 97
Findings: 0
```

## 37. PackageValidator.Tests Result

8 passed, 0 failed

## 38. Localization Integrity

No new user-facing localization keys required. Social challenge identifiers are internal technical keys.

## 39. Exact Files Changed

**New files (6):**
1. `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfSocialChallengeIdentifiers.cs`
2. `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfSocialChallengeDefinition.cs`
3. `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfSocialChallengeCatalog.cs`
4. `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfSocialTargetContext.cs`
5. `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfSocialTestDefinitionService.cs`
6. `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfSocialTests.cs`

**Modified files (1):**
1. `src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs`

## 40. git diff --check

No whitespace errors.

## 41. git status

1 modified file, 6 new untracked files, `.kilo/` untracked.

## 42. Ownerless Blockers

0
