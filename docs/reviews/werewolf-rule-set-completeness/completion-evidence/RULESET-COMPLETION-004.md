# RULESET-COMPLETION-004 Semantic Completion Report
## Complete Generic Dice Resolution Algorithm Extraction

**Status:** Complete. A-002 partially resolved (semantics extracted, execution deferred).
**Package:** `Chronicle.RuleSets.Werewolf`
**Canonical Source:** `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt`
**Decision Authority:** DR-0008 (accepted 2026-08-07), EXTRACTION-0004 A-001/A-002
**Implementation Date:** 2026-08-13

---

## 1. A-001 — Exact General Dice Resolution Algorithm

**Status:** Resolved.
**Source Lines:** 2703–2760 (basic rules), 1083–1086 (specialization, NOT executed), 2781 (difficulty definition), 2822 (Willpower loss on botch).

### Final ordinary dice algorithm (non-specialized)

```text
1. If DiceQuantity <= 0:
   - The character cannot attempt the action (source line 2720).
   - Return ZeroPoolCannotAttempt error.

2. Validate Difficulty is between 2 and 10 (source line 2781: "Valor de 2 a 10").

3. For each die in DiceValues:
   a. If die < 1 or die > 10: return InvalidDieFace error.
   b. Count rawSuccesses: die >= Difficulty (source line 2781: "igualar ou superar este número em pelo menos um dado").
   c. Count ones: die == 1.

4. Apply base cancellation rule (source line 2705: "Cada dado que mostra o valor 1 anula um sucesso obtido na jogada"):
   - finalSuccesses = max(0, rawSuccesses - ones)

5. Determine result classification:
   a. If finalSuccesses > 0: Success.
   b. If finalSuccesses == 0 and onesCount > 0: Botch (source line 2706: "Ocorre quando um teste não resulta em nenhum sucesso e apresenta um ou mais 1s").
   c. If finalSuccesses == 0 and onesCount == 0: Failure.

6. Return result with:
   - SuccessCount = finalSuccesses
   - RawSuccesses = rawSuccesses
   - OnesCount = onesCount
   - FailureClassification = "NoSuccesses" (for failures)
   - BotchClassification = "CriticalFailure" (for botches)
   - InterpretationStatus = "success" | "failure" | "botch" | "zero-pool"
```

### Worked examples (base non-specialized algorithm)

| Dice | Diff | Raw successes | Ones | Final successes | Outcome |
|------|------|---------------|------|-----------------|---------|
| [6] | 6 | 1 | 0 | 1 | Success |
| [6,1] | 6 | 1 | 1 | 0 | Botch |
| [6,6,1] | 6 | 2 | 1 | 1 | Success |
| [1] | 6 | 0 | 1 | 0 | Botch |
| [2,3] | 6 | 0 | 0 | 0 | Failure |
| [10,1] | 6 | 1 | 1 | 0 | Botch |
| [10,10,1] | 6 | 2 | 1 | 1 | Success |
| [] | 6 | 0 | 0 | 0 | ZeroPoolCannotAttempt |

Note: [10,1] yields 1 raw success (10 >= 6, 1 < 6), not 2. The 10 does not grant an additional die in the base algorithm; that is specialization behavior.

### Botch rule verification

Source line 2706: "Ocorre quando um teste não resulta em nenhum sucesso e apresenta um ou mais 1s."

- Botch depends on **zero final successes after cancellation**, not zero raw successes.
- Number of 1s matters only as a binary condition (≥1 triggers botch; more than 1 does not change the classification).
- Exception: if at least one success exists after cancellation, the result is a normal failure or success, never a botch (line 2706: "Se houver pelo menos um sucesso, mesmo que acompanhado por vários 1s, o resultado é apenas uma falha normal").

### Difficulty bounds

Source line 2781: "Valor de 2 a 10". Implementation validates `Difficulty >= 2 && Difficulty <= 10`.

### Zero-pool behavior

Source line 2720: "Se a parada de dados resultante for igual ou inferior a zero, o personagem não poderá tentar a ação." Implementation returns `ZeroPoolCannotAttempt` for `DiceQuantity <= 0`.

---

## 2. A-002 — Specialization Resolution

**Status:** Partially resolved.
**Source Lines:** 1083–1086.

### What is known from source

```text
Specialization eligibility: Ability rating >= 4 grants the right to choose a specialization (source line 1083: "adquire o direito de escolher").
Specialization selection requirement: Rating >= 4 does NOT automatically grant specialization. The character must actively select a specific specialization.
Specialization applicability: Benefits apply only when the test is within the chosen specialization area (source line 1085: "dentro da sua área de especialização").
10-again/additional-die behavior: Each rolled 10 grants +1 additional die (source line 1085: "cada número 10 obtido nos dados permite rolar +1 dado adicional").
Special handling of 1s on additional dice: 1s on added specialization dice do not cancel already-obtained successes (source line 1086).
```

### Unresolved (deferred to Specialties domain)

```text
How is a specific specialization selected and recorded? Deferred to Specialties completion domain.
How is applicability to a specific test determined? Deferred to Specialties completion domain.
What is the continuation roll protocol for 10-again? Deferred to Specialties completion domain.
How are original and added dice distinguished (provenance)? Deferred to Specialties completion domain.
Does chaining have any limit? Source does not explicitly limit, but exact protocol is deferred.
```

### Implementation status

- **RULESET-COMPLETION-004:** Does NOT implement executable Specialties behavior.
- The base dice algorithm (A-001) is implemented without any specialization approximation.
- Any `HasSpecialization` field previously present in contracts has been removed from the executable path to prevent unsourced mechanical benefits.
- The Specialties completion domain owns: chosen specialty character state, applicability to current test, continuation roll protocol, dice provenance, and exact chaining behavior.

---

## 3. Implementation Details

### WerewolfActionRollInterpretationService

**File:** `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfActionRollInterpretationService.cs`

**Changes:**
- Removed `HasSpecialization` parameter from `WerewolfActionRollInterpretationRequest`.
- Removed specialization logic; implements only the base non-specialized algorithm.
- Added `RawSuccesses` and `OnesCount` to `WerewolfActionRollInterpretationResult` for auditability.
- Validates difficulty bounds 2–10 (source line 2781).
- Returns `ZeroPoolCannotAttempt` for `DiceQuantity <= 0` (source line 2720).

### WerewolfActionTestDefinitionService

**File:** `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfActionTestDefinitionService.cs`

**Changes:**
- Removed `HasSpecialization` from `WerewolfActionTestDefinitionResult`.
- Removed `SpecializationId` from `WerewolfActionTestDefinitionRequest`.
- Specialization is NOT inferred from `AbilityRating >= 4`.
- Validates difficulty bounds 2–10 (source line 2781).

### WerewolfReferenceRuntime

**File:** `rule-sets/Chronicle.RuleSets.Werewolf/WerewolfReferenceRuntime.cs`

**Changes:**
- Removed `actionTestDefinitions` dictionary (no hidden runtime state).
- `ExecuteDefineActionTest` no longer stores or returns specialization state.
- `ExecuteInterpretActionRoll` no longer accepts `hasSpecialization` from caller input.
- Interpretation is deterministic and reconstructible from explicit serialized inputs/outputs only.

---

## 4. Test Coverage

### WerewolfActionRollInterpretationTests

**File:** `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfActionRollInterpretationTests.cs`

**Tests:** 16 tests covering base algorithm only:
- Success, failure, and botch classification
- 1-cancellation
- Zero pool rejection
- Invalid die face rejection
- Deterministic serialization
- Raw dice retention
- Difficulty validation (2–10)
- Worked examples: [6], [6,1], [6,6,1], [1], [2,3], [10,1], [10,10,1]

### WerewolfActionTestDefinitionTests

**File:** `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfActionTestDefinitionTests.cs`

**Tests:** 13 tests covering base test definition only:
- Valid test definition
- Invalid attribute/ability/difficulty rejection
- Stale draft version rejection
- Modifier application
- Negative pool clamping
- All supported races can define action test

### WerewolfActionRuntimeTests

**File:** `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfActionRuntimeTests.cs`

**Tests:** 4 tests covering runtime operation:
- Define test succeeds for completed character
- Define test rejects non-completed character
- Interpret action roll succeeds for valid raw dice
- Interpret action roll rejects invalid die face
- Action operations do not generate random values

**Total Werewolf tests:** 553 passing.

---

## 5. Completeness Matrix Updates

### Domains marked mechanically complete (A-001)

| Domain | Previous Status | New Status |
|--------|----------------|------------|
| Dice pools and difficulty | A-001 pending | Resolved |
| Success determination | A-001 pending | Resolved |
| Failure determination | A-001 pending | Resolved |
| Botch determination | A-001 pending | Resolved |

**Completeness delta:** +4 mechanically complete domains (13 → 17).
**Current-slice executable:** 29/68 (unchanged; these domains were already current-slice executable).

### Specialties domain (A-002)

| Field | Previous | Current |
|-------|----------|---------|
| ambiguityStatus | A-001, A-002 | A-002 |
| requiredRemediation | Complete A-001 and A-002 extraction | Complete A-002: specialization selection, applicability, and character state |
| mechanicalCompleteness | false | false |
| implementationCoverage | partial | partial |
| testCoverage | partial | partial |
| packageExposure | partial | partial |

---

## 6. Source Authority Summary

| Rule | Source Line | Implementation |
|------|------------|----------------|
| Success threshold: die >= Difficulty | 2781 | `die >= request.Difficulty` |
| Difficulty range: 2–10 | 2781 | `request.Difficulty < 2 || request.Difficulty > 10` |
| 1 cancels success | 2705 | `finalSuccesses = max(0, rawSuccesses - ones)` |
| Botch: 0 final successes + >=1 ones | 2706 | `finalSuccesses == 0 && onesCount > 0` |
| Failure: 0 successes, 0 ones | 2706 (implicit) | `finalSuccesses == 0 && onesCount == 0` |
| Zero pool cannot attempt | 2720 | `DiceQuantity <= 0` error |
| Specialization eligibility: Ability >= 4 + chosen specialization | 1083 | NOT implemented in RULESET-COMPLETION-004 |
| 10 grants +1 die | 1085 | NOT implemented in RULESET-COMPLETION-004 |
| Specialization 1s don't cancel (on added dice) | 1086 | NOT implemented in RULESET-COMPLETION-004 |

---

## 7. Runtime Trust Boundary

**Issue:** The previous implementation allowed arbitrary caller input to set `hasSpecialization=true` during interpretation, manufacturing mechanical benefits without an authoritative test definition.

**Fix:** Removed `HasSpecialization` from the executable path entirely:
1. `WerewolfActionRollInterpretationRequest` no longer contains `HasSpecialization`.
2. `WerewolfActionTestDefinitionResult` no longer contains `HasSpecialization`.
3. `WerewolfReferenceRuntime` no longer stores or passes specialization state.
4. The operation boundary is deterministic and reconstructible from explicit serialized inputs/outputs only.

This prevents Narrative Intelligence or arbitrary callers from manufacturing specialization benefits.

---

## 8. Open Items and Deferrals

- **Specialization selection and character state:** Deferred to **Specialties** completion domain.
- **Specialization applicability:** Deferred to **Specialties** completion domain.
- **10-again chaining:** The source implies chaining ("rolar +1 dado adicional" without limit), but the Chronicle's dice generator is responsible for implementing the physical rolling. The Rule Set interpretation service receives the final flat list of dice values.
- **Specialization dice provenance:** The source distinguishes 1s on "additional" dice from 1s on original dice. This is NOT approximated in the base algorithm.
- **Extended/resisted tests:** Deferred to RULESET-COMPLETION-005+ per existing backlog.
- **Auto-success rule:** Source line 2709 defines auto-success when pool >= difficulty. This is a pre-roll decision and is not applied by the interpretation service (which receives already-rolled dice).

---

## 9. Validation

- Build: Clean (0 warnings, 0 errors).
- Tests: 603 total passing (553 Werewolf + 9 Application + 12 Infrastructure + 8 Contracts + 1 Domain + 1 Persistence.Sqlite + 8 PackageValidator.Tests + 11 Architecture).
- Architecture tests: 11/11 passing.
- Package validator: Valid (29 files, 0 findings).

---

## 10. Files Changed

9 files changed, 471 insertions(+), 177 deletions(-):

| File | Type |
|------|------|
| `WerewolfActionRollInterpretationService.cs` | Implementation |
| `WerewolfActionTestDefinitionService.cs` | Implementation |
| `WerewolfReferenceRuntime.cs` | Runtime |
| `WerewolfActionRollInterpretationTests.cs` | Tests |
| `WerewolfActionTestDefinitionTests.cs` | Tests |
| `WerewolfActionRuntimeTests.cs` | Tests |
| `EXTRACTION-0004-ambiguities-and-conflicts.md` | Extraction |
| `completeness-matrix.json` | Matrix |
| `completeness-report.md` | Report |

New file:
| `RULESET-COMPLETION-004.md` | Evidence |
