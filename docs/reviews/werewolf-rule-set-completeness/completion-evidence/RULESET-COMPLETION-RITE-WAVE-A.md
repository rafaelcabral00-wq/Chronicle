# RULESET-COMPLETION-RITE-WAVE-A: Werewolf Rite Hunting Stone Execution

**Status:** Complete
**Date:** 2026-08-25
**Auditor:** Kilo (automated implementation audit)
**Scope:** First dependency-free Rite execution slice — Hunting Stone only
**Canonical Source:** `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt`
**Baseline:** af6d26a

---

## 1. Exact Implemented Rite Count

**1 Rite implemented: `rite.mystic.hunting-stone`**

---

## 2. Canonical Hunting Stone Semantics

**Source locator:** Lines 2669-2671

| Property | Source-Derived Value |
|----------|---------------------|
| Stable key | `rite.mystic.hunting-stone` |
| Canonical name | Ritual da Pedra Caçadora |
| Category | Místicos |
| Level | 1 |
| Attribute | Raciocínio |
| Ability | Rituais |
| Base difficulty | 7 (standard Mystic difficulty, line 2662) |
| Difficulty modifier | -1 if possess a piece of target (line 2671) |
| Required successes | None (simple test, 1 success sufficient for general location) |
| Cost | None |
| Duration | Not explicitly stated for this Rite; general rule: minimum 10 minutes per level (line 2576) |
| Target | Person or object (locations excluded) |
| Effect | Fornece apenas a localização geral do alvo |
| Success semantics | Provides general location of target |
| Failure semantics | No information gained |
| Botch semantics | Generic botch semantics apply (source does not define Rite-specific botch) |

---

## 3. Architecture Introduced

### Types

| Type | Location | Purpose |
|------|----------|---------|
| `WerewolfRiteDefinition` | `CharacterCreation/WerewolfRiteDefinition.cs` | Typed Rite metadata record |
| `WerewolfRiteCatalog` | `CharacterCreation/WerewolfRiteCatalog.cs` | Static Rite catalog |
| `WerewolfRiteIdentifiers` | `CharacterCreation/WerewolfRiteIdentifiers.cs` | Stable Rite key constants |
| `WerewolfRiteExecutionRequest` | `CharacterCreation/WerewolfRiteExecutionRequest.cs` | Execution request record |
| `WerewolfRiteExecutionResult` | `CharacterCreation/WerewolfRiteExecutionResult.cs` | Execution result record |
| `WerewolfRiteFinding` | `CharacterCreation/WerewolfRiteFinding.cs` | Finding/info record |
| `WerewolfRiteExecutionService` | `CharacterCreation/WerewolfRiteExecutionService.cs` | Deterministic execution service |

### Runtime Operation

| Operation Key | Category | Status |
|---------------|----------|--------|
| `rite-runtime.execute-rite` | rite-runtime | Enabled |

---

## 4. Randomness Boundary

**Chronicle owns:**
- Raw dice generation
- Dice values provided via `diceValues` input

**Werewolf owns:**
- Rite definition lookup
- Difficulty calculation (base - modifier)
- Roll interpretation via existing `WerewolfActionRollInterpretationService`
- Effect determination based on success count

**Boundary rule:**
- Werewolf never generates dice internally
- Werewolf never mutates Chronicle-provided dice values

---

## 5. World/Location Boundary

Hunting Stone provides **only general location** ("localização geral").

Werewolf-side semantics:
- Determines success/failure
- Returns typed effect description indicating general location granted

Chronicle/world layer owns:
- Actual world coordinates
- Entity lookup
- Map/pathfinding resolution

Typed boundary: `WerewolfRiteExecutionResult.Effect` contains the canonical effect description. External systems interpret this to resolve world state.

---

## 6. Known-Rite/Learning Boundary

**Explicitly NOT implemented:**
- Rite learning/acquisition
- XP purchase
- Teacher/master availability
- Downtime/week progression
- Background/Knowledge semantics (A-010)

**Known-Rite execution boundary:**
- This package accepts a caller-authorized Rite execution request
- No proof of learning is validated here
- The execution service trusts the caller to authorize known-Rite execution
- Future persistence of learned-Rite state is owned by Progression/Chronicle

---

## 7. A-010 Preservation

A-010 (Background vs Knowledge vs ritual semantics) remains unresolved and applies ONLY to Rite learning/acquisition. It does not block Hunting Stone execution because:
- The `Rituais` ability identifier (`character.ability.rituals`) is unambiguous in the current model
- Execution references the existing canonical trait directly
- No Background/Knowledge collision occurs at execution time

---

## 8. RuntimeCharacterState Changes

**None.** Hunting Stone execution is stateless. No character-state mutation occurs. `WerewolfRuntimeCharacterState` is not modified.

---

## 9. Tests

### New Focused Tests (15 tests)

| Test | Description |
|------|-------------|
| `ExecuteHuntingStoneReturnsSuccessWhenSuccessesMeetDifficulty` | Success with 3 successes vs difficulty 7 |
| `ExecuteHuntingStoneReducesDifficultyWhenTargetPiecePossessed` | Difficulty reduced by 1 when target piece held |
| `ExecuteHuntingStoneReturnsFailureWhenNoSuccesses` | Ordinary failure returns no information |
| `ExecuteHuntingStoneReturnsBotchWhenOnesExceedSuccesses` | Botch semantics preserved |
| `ExecuteHuntingStoneRejectsUnknownRiteKey` | Unknown Rite returns error |
| `ExecuteHuntingStoneRejectsEmptyDiceValues` | Empty dice values return error |
| `ExecuteHuntingStoneRejectsInvalidDieFace` | Out-of-bounds die face returns error |
| `ExecuteHuntingStoneRejectsEmptyRequestId` | Empty requestId returns error |
| `ExecuteHuntingStoneRejectsEmptyRiteKey` | Empty RiteKey returns error |
| `ExecuteHuntingStoneReturnsGeneralLocationOnSuccess` | Effect matches canonical description |
| `ExecuteHuntingStoneReturnsNoInformationOnFailure` | Failure effect is "No information gained." |
| `ExecuteHuntingStoneDoesNotDependOnSpiritUmbra` | Effect contains no Spirit/Umbra references |
| `ExecuteHuntingStoneDoesNotDependOnPackSept` | Effect contains no Pack/Sept/Caern references |
| `ExecuteHuntingStoneDoesNotClaimLearningImplementation` | Effect contains no XP/Background/Knowledge references |
| `ExecuteHuntingStonePreservesProgressionState` | Execution does not mutate progression state |

### Regression Tests

Existing ordinary action resolution tests remain green. Extended/Resisted tests remain green.

---

## 10. Validation Results

| Check | Result |
|-------|--------|
| New focused tests | 15 passed, 0 failed |
| Full Werewolf suite | 1473 passed, 0 failed |
| PackageValidator | 8 passed, 0 failed |
| Contracts | 8 passed, 0 failed |
| Domain | 1 passed, 0 failed |
| Architecture | 11 passed, 0 failed |
| Application | 9 passed, 0 failed |
| Infrastructure | 12 passed, 0 failed |

### Root Cause Analysis

**46 Werewolf failures — root cause: undeclared operation capability causing runtime registration rejection**

- First divergent result: `RuleSetRuntimeRegistrationService.Register` rejected the Werewolf runtime because `ExecuteRiteOperation` was declared with capability `rite-runtime`, which is NOT present in the package manifest's capabilities list (`werewolf.package-manifest.json` lines 53-105).
- `ValidateRuntime` iterates all runtime operations and rejects the entire runtime if any operation's capability is undeclared and the operation is not explicitly disabled.
- With the runtime rejected, zero operations were available to the registry, causing every runtime-flow test to fail with `RuleSetOperationFailureCode.RuntimeNotRegistered` or `KeyNotFoundException: 'draftId'` when chaining operations.
- Exact failure count: 46
- Representative tests: `RuntimeFlowSelectsAllInitialGiftSources`, `EveryRaceAuspiceTribePathPassesClassificationAndInitialGiftPhase` (15 variations), `RuntimeFlowCreatesDraftAndSelectsRace`, `RuntimeRegistryInvokesResourceRankInitialization`, `LookupIsDeterministicAndImmutable`
- Exact assertion/exception: `System.Collections.Generic.KeyNotFoundException : The given key 'draftId' was not present in the dictionary.` and `RuleSetOperationFailureCode.RuntimeNotRegistered`
- Affected package/runtime resource: All runtime-flow tests in `WerewolfInitialGiftSelectionTests`, `WerewolfRaceSelectionTests`, `WerewolfAuspiceSelectionTests`, `WerewolfTribeSelectionTests`, `WerewolfResourceRankInitializationTests`, `RuleSetRuntimeRegistryTests`, and others

**2 PackageValidator failures — root cause: stale build artifacts**

- Exact failure count: 2
- Representative tests: `ValidWerewolfPackageReturnsValidExitCode`, `PackagePathOutsideRepositoryIsValidatedWhenExplicitlySupplied`
- Exact assertion/exception: `Assert.Equal() Failure. Values differ. Expected: 0, Actual: 1`
- Affected package/runtime resource: `PackageValidatorCommandTests`
- Root cause: Stale build artifacts in `tests/Chronicle.Tools.PackageValidator.Tests/bin/Debug/net10.0/` caused the test harness to use an outdated validator binary. After `dotnet clean` and rebuild, both tests pass with exit code 0.
- Clean baseline proof: Created temporary worktree at af6d26a with `git worktree add --detach`. Verified `git status --short` was empty and no RITE-WAVE-A files existed. Ran PackageValidator tests: **8 passed, 0 failed**. This definitively proves the 2 failures were NOT pre-existing on af6d26a.

### Minimum Corrections Made

1. **Changed `ExecuteRiteOperation` capability key** from `rite-runtime` to `post-creation-character-operations` in `WerewolfReferenceRuntime.cs` line 151, matching the existing package manifest capability.
2. **Moved `ExecuteRiteOperation` to correct alphabetical position** in `RuleSetRuntimeRegistryTests.cs` expected operation list (from position 34 to last position, after `ExecuteGiftEffectOperation`), matching the registry's alphabetical sort order.
3. **Updated `RuleSetPackageSourceValidation.cs` allow-list** to include new Rite source files (required for package-source validation).

---

## 11. Ownerless Blockers

| Blocker | Assigned Owner | Status |
|---------|----------------|--------|
| ExtendedTestDefinition primitive | Action Resolution | Resolved |
| ResistedTestDefinition primitive | Action Resolution | Resolved |
| Spirit/Umbra domain | Spirits/Umbra workstream | Pending |
| Pack/Sept/Totem aggregation | Pack/Sept workstream | Pending |
| Renown/Rank state machine | Progression workstream | Pending |
| Fetish/Talen domain | Items/Fetishes workstream | Pending |
| A-010 ambiguity resolution | Rites + Documentation | Pending (learning only) |
| Chronicle downtime lifecycle | Chronicle Application | Pending |

**Ownerless blockers = 0. All assigned.**

---

## 12. Exact Files Changed

**Created (9 files):**
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRiteDefinition.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRiteCatalog.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRiteExecutionRequest.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRiteExecutionResult.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRiteExecutionService.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRiteFinding.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRiteIdentifiers.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfRiteExecutionTests.cs`
- `docs/reviews/werewolf-rule-set-completeness/completion-evidence/AUDIT-WEREWOLF-RITES-EXECUTION-2026-08-25.md`

**Modified (4 files):**
- `rule-sets/Chronicle.RuleSets.Werewolf/WerewolfReferenceRuntime.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/WerewolfRuleSetPackage.cs`
- `src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/RuleSetRuntimeRegistryTests.cs`
