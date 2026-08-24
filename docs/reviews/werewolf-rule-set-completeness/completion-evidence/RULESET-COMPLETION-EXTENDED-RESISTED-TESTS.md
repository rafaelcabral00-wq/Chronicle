# RULESET-COMPLETION-EXTENDED-RESISTED-TESTS: Werewolf Extended and Resisted Test Primitives

**Status:** Complete
**Date:** 2026-08-24
**Auditor:** Kilo (automated implementation audit)
**Scope:** Shared action-resolution primitives for Extended and Resisted tests
**Canonical Source:** `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt`
**Baseline:** 8b67441 plus local Rites audit evidence commit

---

## 1. Source Locators

**Extended tests (Ações Prolongadas):**
- Line 2743-2745: Definition and critical failure semantics
- Line 2774: Glossary definition
- Line 2588: Rite example — "Teste prolongado e resistido"
- Line 2602: Rite example — "Teste prolongado... acúmulo de 40 sucessos"
- Line 2578: Learning example — "acumulando sucessos equivalentes ao nível do ritual ao longo de semanas"
- Line 3010: Gift example — Atração Animal
- Line 3036: Gift example — Interpretação de Sonhos
- Line 3044: Gift example — Caçada
- Line 3049: Gift example — Procura
- Line 3186: Combat example — Cerco

**Resisted tests (Ações Resistidas):**
- Line 2746-2748: Core mechanics and extended variant
- Line 2775: Glossary definition
- Line 1912: Gift example — Fragilizar Corpos
- Line 1959: Gift example — Drenagem Espiritual
- Line 1966: Gift example — Modelagem de Espírito
- Line 2008: Gift example — Imposição
- Line 2031: Gift example — Chamado da Wyrm
- Line 2483: Gift example — Obediência
- Line 2449: Gift example — Aproveitar a Vantagem
- Line 3000: Stealth example — simultaneous opposed tests
- Line 2588: Rite example — Caern opening opposed test
- Line 3135: Combat example — Encontrão
- Line 3173: Combat example — Mandíbula de Ferro

---

## 2. Extracted Generic Semantics

### Extended Test Semantics

| Property | Source-Derived Value |
|----------|---------------------|
| Pool | Defined by mechanic (Attribute + Ability, or fixed) |
| Difficulty | 2-10, defined by mechanic |
| Required successes | Defined by mechanic (e.g., 40 for Caern creation, level for Rite learning) |
| Interval | Consecutive turns (source says "turnos consecutivos") |
| Accumulation | Final successes added to accumulated total per roll |
| Botch semantics | Botch cancels ALL accumulated successes, must restart or abort |
| Failure semantics | Ordinary failure adds no successes, progress continues |
| Completion | Accumulated successes >= required successes |
| Maximum attempts | Not defined by source (mechanic may define) |
| Randomness | Chronicle-owned (dice generation, success counting) |

### Resisted Test Semantics

| Property | Source-Derived Value |
|----------|---------------------|
| Side A pool | Defined by mechanic |
| Side A difficulty | Defined by mechanic (may differ from Side B) |
| Side B pool | Defined by mechanic |
| Side B difficulty | Defined by mechanic (may differ from Side A) |
| Comparison | Side A successes cancel Side B successes |
| Winner | Remaining net successes determine winner |
| Tie | Equal net successes (both > 0) |
| Botch | Individual side botch; source does not define universal cross-side botch consequence |
| Extended variant | Accumulate net successes until target reached before opponent |

---

## 3. Implemented Types

### Extended Test Types

**`WerewolfExtendedTestDefinition`** (`rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfExtendedTestDefinition.cs`)
- `RequestId`: string
- `DicePool`: int
- `Difficulty`: int
- `RequiredSuccesses`: int

**`WerewolfExtendedTestProgress`** (`rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfExtendedTestProgress.cs`)
- `RequestId`: string
- `AccumulatedSuccesses`: int
- `AttemptCount`: int
- `IsBotched`: bool
- `Status`: `WerewolfExtendedTestStatus` enum (InProgress, Completed, Failed, Botched)

**`WerewolfExtendedTestResult`** (`rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfExtendedTestResult.cs`)
- `Succeeded`: bool
- `Findings`: IReadOnlyList<WerewolfExtendedTestFinding>
- `RequestId`: string
- `UpdatedProgress`: WerewolfExtendedTestProgress
- `Status`: string
- `SerializedProgress`: string

### Resisted Test Types

**`WerewolfResistedTestDefinition`** (`rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfResistedTestDefinition.cs`)
- `RequestId`: string
- `SideADicePool`: int
- `SideADifficulty`: int
- `SideBDicePool`: int
- `SideBDifficulty`: int

**`WerewolfResistedTestResult`** (`rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfResistedTestResult.cs`)
- `Succeeded`: bool
- `Findings`: IReadOnlyList<WerewolfResistedTestFinding>
- `RequestId`: string
- `SideASuccesses`: int
- `SideBSuccesses`: int
- `NetSuccesses`: int
- `Winner`: `WerewolfResistedTestWinner` enum (None, SideA, SideB, Tie)
- `Status`: string
- `SerializedResult`: string

---

## 4. Extended State-Transition Behavior

**Input:**
- Test definition (pool, difficulty, requiredSuccesses)
- Previous progress (accumulated successes, attempt count, status)
- Chronicle-provided resolved dice result (IReadOnlyList<int>)

**Output:**
- Updated accumulated progress
- Terminal state: Completed / Failed / Botched

**Transition rules:**
1. Botch (finalSuccesses == 0 && ones > 0):
   - `IsBotched = true`
   - `Status = Botched`
   - `AccumulatedSuccesses = 0`
   - Source: "Uma falha crítica durante uma ação prolongada anula todos os sucessos acumulados"

2. Success (finalSuccesses > 0):
   - `AccumulatedSuccesses += finalSuccesses`
   - If `AccumulatedSuccesses >= RequiredSuccesses`: `Status = Completed`
   - Else: `Status = InProgress`

3. Ordinary failure (finalSuccesses == 0 && ones == 0):
   - `AttemptCount++`
   - Status remains InProgress

4. Terminal state protection:
   - If Status is Completed or Botched, further advances return error

---

## 5. Resisted/Opposed Interpretation Behavior

**Input:**
- Test definition (Side A pool/difficulty, Side B pool/difficulty)
- Chronicle-provided resolved dice for Side A
- Chronicle-provided resolved dice for Side B

**Output:**
- Side A successes
- Side B successes
- Net successes
- Winner (None/SideA/SideB/Tie)
- Status string

**Interpretation rules:**
1. Each side interpreted independently using existing `WerewolfActionRollInterpretationService`
2. Net successes = SideA - SideB
3. Winner determined by net successes:
   - net > 0: SideA
   - net < 0: SideB
   - net == 0 && both > 0: Tie
   - else: None
4. Botch status is preserved per side but does not create cross-side effects in primitive
5. Consumer owns botch consequences

---

## 6. Exact Operation Keys

| Operation Key | Category | Status |
|---------------|----------|--------|
| `character-runtime.define-extended-test` | generic-dice | Enabled |
| `character-runtime.advance-extended-test` | generic-dice | Enabled |
| `character-runtime.define-resisted-test` | generic-dice | Enabled |
| `character-runtime.interpret-resisted-test` | generic-dice | Enabled |

---

## 7. Randomness Boundary

**Chronicle owns:**
- Dice generation (`IDiceValueGenerator`)
- Raw dice values
- Success counting (handled by `WerewolfActionRollInterpretationService`)

**Werewolf owns:**
- Test definition (pool, difficulty, required successes)
- Extended progress accumulation
- Botch consequence (reset accumulated successes)
- Resisted comparison and net success calculation
- Winner determination

**Boundary rule:**
- Werewolf never generates dice internally
- Werewolf never modifies Chronicle-provided dice values
- All randomness originates from Chronicle

---

## 8. Consumer-Domain Examples

### Rites (proven representable)

| Rite | Extended? | Resisted? | Representation |
|------|-----------|-----------|----------------|
| `rite.caern.opening` | Yes | Yes | ExtendedTestDefinition + ResistedTestDefinition composed |
| `rite.caern.creation` | Yes | No | ExtendedTestDefinition with RequiredSuccesses=40 |
| Rite learning (all 32) | Yes | No | ExtendedTestDefinition with RequiredSuccesses=rite level |

### Gifts (proven representable)

| Gift | Extended? | Resisted? | Representation |
|------|-----------|-----------|----------------|
| Fragilizar Corpos | No | Yes | ResistedTestDefinition |
| Drenagem Espiritual | No | Yes | ResistedTestDefinition |
| Modelagem de Espírito | No | Yes | ResistedTestDefinition |
| Imposição | No | Yes | ResistedTestDefinition |
| Chamado da Wyrm | No | Yes | ResistedTestDefinition |
| Obediência | No | Yes | ResistedTestDefinition |
| Atração Animal | Yes | No | ExtendedTestDefinition |
| Interpretação de Sonhos | Yes | No | ExtendedTestDefinition |
| Caçada | Yes | No | ExtendedTestDefinition |
| Procura | Yes | No | ExtendedTestDefinition |
| Materialização de Sonhos | Yes | No | ExtendedTestDefinition |
| Cerco | Yes | No | ExtendedTestDefinition |

### Social/Combat (proven representable)

| Example | Extended? | Resisted? | Representation |
|---------|-----------|-----------|----------------|
| Discurso Público (4 sucessos) | Yes | No | ExtendedTestDefinition with RequiredSuccesses=4 |
| Discurso Público / Multidão | Yes | No | ExtendedTestDefinition |
| Intimidação Física | No | Yes | ResistedTestDefinition |
| Mandíbula de Ferro escape | No | Yes | ResistedTestDefinition |
| Aproveitar a Vantagem | No | Yes | ResistedTestDefinition |

---

## 9. Tests

### Extended Test Tests (13 tests)

| Test | Description |
|------|-------------|
| `CreateInitialProgressReturnsInProgress` | Initial state is 0 accumulated, 0 attempts, InProgress |
| `CreateInitialProgressRejectsInvalidDefinition` | Invalid definition returns Failed status |
| `AdvanceAddsSuccessesOnSuccess` | Successful roll adds finalSuccesses to accumulated |
| `AdvanceCompletesWhenThresholdReached` | Status transitions to Completed when threshold met |
| `AdvanceKeepsInProgressBelowThreshold` | Status remains InProgress below threshold |
| `AdvanceIncrementsAttemptsOnFailure` | Ordinary failure increments AttemptCount |
| `AdvanceDoesNotAddSuccessesOnFailure` | Ordinary failure adds no successes |
| `AdvanceBotchResetsAccumulated` | Botch sets AccumulatedSuccesses=0, Status=Botched |
| `AdvanceRejectsTerminalProgress` | Cannot advance Completed or Botched progress |
| `AdvanceDoesNotMutatePreviousProgress` | Previous progress record is immutable |
| `AdvanceRejectsNegativePool` | Negative dice pool returns error |
| `AdvanceRejectsInvalidDifficulty` | Difficulty outside 2-10 returns error |
| `AdvanceRejectsZeroRequiredSuccesses` | RequiredSuccesses <= 0 returns error |

### Resisted Test Tests (12 tests)

| Test | Description |
|------|-------------|
| `InterpretSideAWinsWhenNetPositive` | Side A wins when net > 0 |
| `InterpretSideBWinsWhenNetNegative` | Side B wins when net < 0 |
| `InterpretTieWhenNetZero` | Tie when net == 0 and both succeeded |
| `InterpretSideAWinsWhenOnlySideASucceeds` | Side A wins when only A succeeds |
| `InterpretSideBWinsWhenOnlySideBSucceeds` | Side B wins when only B succeeds |
| `InterpretBothFailWhenNeitherSucceeds` | Both fail when neither succeeds |
| `InterpretBotchOnSideA` | Side A botch produces side-a-botch status |
| `InterpretBotchOnSideB` | Side B botch produces side-b-botch status |
| `InterpretBothBotch` | Both botch produces both-botch status |
| `InterpretDifferentPoolsAndDifficulties` | Different pools/difficulties work correctly |
| `InterpretNetSuccessesComputedCorrectly` | Net successes = A - B |
| `InterpretRejectsInvalidDiceCount` | Dice count mismatch returns error |

---

## 10. Exclusions

**Not implemented in this work package:**
- Rite catalog/runtime
- Spirit/Umbra mechanics
- Pack/Totem mechanics
- Renown/Rank advancement
- Fetish/Talen domain
- Chronicle persistence/lifecycle
- Gift-specific mechanics beyond regression compatibility
- Extended+Resisted composed primitive (left to consumer composition)
- Specialty applicability hooks
- Session persistence for extended progress

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
| A-010 ambiguity resolution | Rites + Documentation | Pending |
| Chronicle downtime lifecycle | Chronicle Application | Pending |

---

## 12. Integration Hotspot Files

| File | Change |
|------|--------|
| `rule-sets/Chronicle.RuleSets.Werewolf/WerewolfReferenceRuntime.cs` | Added 4 operation constants, metadata entries, and Execute handlers |
| `rule-sets/Chronicle.RuleSets.Werewolf/WerewolfRuleSetPackage.cs` | Added 4 operation constants |
| `src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs` | Added new files to package allow-list |
| `rule-sets/Chronicle.RuleSets.Werewolf.Tests/RuleSetRuntimeRegistryTests.cs` | Updated expected operation list |

---

## 13. Validation Results

| Check | Result |
|-------|--------|
| New focused tests | 25 passed, 0 failed |
| Full Werewolf suite | 1373 passed, 0 failed |
| PackageValidator | 8 passed, 0 failed |
| Contracts | 8 passed, 0 failed |
| Domain | 1 passed, 0 failed |
| Application | 9 passed, 0 failed |
| Architecture | 11 passed, 0 failed |
| git diff --check | Clean (no whitespace errors) |

---

## 14. Files Changed

**Created (11 files):**
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfExtendedTestDefinition.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfExtendedTestFinding.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfExtendedTestProgress.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfExtendedTestResult.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfExtendedTestService.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfResistedTestDefinition.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfResistedTestFinding.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfResistedTestResult.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfResistedTestService.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfExtendedTestTests.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfResistedTestTests.cs`

**Modified (4 files):**
- `rule-sets/Chronicle.RuleSets.Werewolf/WerewolfReferenceRuntime.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/WerewolfRuleSetPackage.cs`
- `src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/RuleSetRuntimeRegistryTests.cs`

---

## 15. Git Status

```text
 M rule-sets/Chronicle.RuleSets.Werewolf.Tests/RuleSetRuntimeRegistryTests.cs
 M rule-sets/Chronicle.RuleSets.Werewolf/WerewolfReferenceRuntime.cs
 M rule-sets/Chronicle.RuleSets.Werewolf/WerewolfRuleSetPackage.cs
 M src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs
?? rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfExtendedTestTests.cs
?? rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfResistedTestTests.cs
?? rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfExtendedTestDefinition.cs
?? rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfExtendedTestFinding.cs
?? rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfExtendedTestProgress.cs
?? rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfExtendedTestResult.cs
?? rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfExtendedTestService.cs
?? rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfResistedTestDefinition.cs
?? rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfResistedTestFinding.cs
?? rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfResistedTestResult.cs
?? rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfResistedTestService.cs
```

No commits made. No pushes made.
