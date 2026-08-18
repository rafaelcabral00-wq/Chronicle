# RULESET-COMPLETION-006 Pre-Commit Authority and Completeness Audit

**Date:** 2026-08-17  
**Auditor:** Kilo (automated pre-commit audit)  
**Scope:** Ability catalog canonicalization, localization coverage, Lupus restriction completeness, test coverage, mechanical completeness accounting

---

## 1. Canonical Ability Inventory

### Exact Owned Domain Keys

- Ability allocation
- Dice operation inputs
- Localization

### Canonical Ability Total (30)

| Category | Count |
|----------|-------|
| Talents | 10 |
| Skills | 10 |
| Knowledges | 10 |
| **Total** | **30** |

### Exact Canonical Ability Keys by Category

**Talents (10):**
- `character.ability.alertness`
- `character.ability.athletics`
- `character.ability.brawl`
- `character.ability.dodge`
- `character.ability.empathy`
- `character.ability.expression`
- `character.ability.intimidation`
- `character.ability.primal-instinct`
- `character.ability.streetwise`
- `character.ability.subterfuge`

**Skills (10):**
- `character.ability.animal-empathy`
- `character.ability.crafts`
- `character.ability.drive`
- `character.ability.etiquette`
- `character.ability.firearms`
- `character.ability.leadership`
- `character.ability.melee`
- `character.ability.performance`
- `character.ability.stealth`
- `character.ability.survival`

**Knowledges (10):**
- `character.ability.computer`
- `character.ability.enigmas`
- `character.ability.investigation`
- `character.ability.law`
- `character.ability.linguistics`
- `character.ability.medicine`
- `character.ability.occult`
- `character.ability.politics`
- `character.ability.rituals`
- `character.ability.science`

### Entries Added (12)

1. `character.ability.dodge`
2. `character.ability.primal-instinct`
3. `character.ability.streetwise`
4. `character.ability.animal-empathy`
5. `character.ability.crafts`
6. `character.ability.firearms`
7. `character.ability.melee`
8. `character.ability.enigmas`
9. `character.ability.linguistics`
10. `character.ability.medicine`
11. `character.ability.rituals`
12. `character.ability.science`

### Entries Removed (0)

No canonical Ability keys were removed.

### Entries Renamed/Normalized (0)

No existing canonical Ability keys were renamed. All 18 pre-existing keys retain their stable identifiers.

### Stable-Key Compatibility Decisions

All pre-existing Ability identifiers remain unchanged:
- `character.ability.alertness`
- `character.ability.athletics`
- `character.ability.brawl`
- `character.ability.drive`
- `character.ability.empathy`
- `character.ability.etiquette`
- `character.ability.expression`
- `character.ability.intimidation`
- `character.ability.investigation`
- `character.ability.leadership`
- `character.ability.law`
- `character.ability.occult`
- `character.ability.performance`
- `character.ability.politics`
- `character.ability.stealth`
- `character.ability.subterfuge`
- `character.ability.survival`
- `character.ability.computer`

No breaking changes to existing stable keys.

---

## 2. Lupus Base Restrictions

### Exact Lupus Restricted Identity Set (9 abilities)

During base creation, Lupus CANNOT allocate dots to:

1. `character.ability.computer` (Computer)
2. `character.ability.crafts` (Crafts) - **NEW**
3. `character.ability.drive` (Drive)
4. `character.ability.etiquette` (Etiquette)
5. `character.ability.firearms` (Firearms) - **NEW**
6. `character.ability.law` (Law)
7. `character.ability.linguistics` (Linguistics) - **NEW**
8. `character.ability.politics` (Politics)
9. `character.ability.science` (Science) - **NEW**

### Previous Restricted Identity Set (5 abilities)

Previous code restricted:
1. `character.ability.computer`
2. `character.ability.drive`
3. `character.ability.etiquette`
4. `character.ability.law`
5. `character.ability.politics`

### Added in This Completion (4 abilities)

- `character.ability.crafts`
- `character.ability.firearms`
- `character.ability.linguistics`
- `character.ability.science`

### Source Authority

Source file `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` line 546 states Lupus cannot allocate dots to:
- Ofícios (Crafts)
- Condução (Drive)
- Etiqueta (Etiquette)
- Armas de Fogo (Firearms)
- Computador (Computer)
- Direito (Law)
- Linguística (Linguistics)
- Política (Politics)
- Ciência (Science)

Note: Source says these "podem ser adquiridos posteriormente com pontos de bônus/experiência via treino" - freebie timing is owned by RULESET-COMPLETION-007. Only canonicalized the restricted identity set here.

---

## 3. What Remains Deferred to RULESET-COMPLETION-007

RULESET-COMPLETION-007 owns:
- Freebie points operation implementation
- Lupus freebie spending timing semantics
- Ability freebie interaction with base allocation

The canonical restriction identity set (9 abilities) is now complete. Freebie timing rules are deferred.

---

## 4. Specialties Status

Boundary defined, not implemented. Specialties remain deferred per A-002. Dice algorithm is implemented; specialization selection/applicability and character state for chosen specializations are pending.

---

## 5. Base Allocation Limits

- Base rating: 0
- Maximum rating during creation: 3
- Budgets: 13/9/5 (primary/secondary/tertiary)

---

## 6. Action-Test Integration

Verified. `WerewolfActionTestDefinitionService` and `WerewolfActionRollInterpretationService` operate on Ability identifiers. The expanded 30-ability catalog is compatible with existing action-test operations. No changes required to action-test integration.

---

## 7. Localization Coverage

### English (`current-slice.json`)

All 30 abilities have display names:
- `character.ability.alertness.display-name`: "Alertness"
- `character.ability.athletics.display-name`: "Athletics"
- `character.ability.brawl.display-name`: "Brawl"
- `character.ability.dodge.display-name`: "Dodge"
- `character.ability.empathy.display-name`: "Empathy"
- `character.ability.expression.display-name`: "Expression"
- `character.ability.intimidation.display-name`: "Intimidation"
- `character.ability.primal-instinct.display-name`: "Primal Instinct"
- `character.ability.streetwise.display-name`: "Streetwise"
- `character.ability.subterfuge.display-name`: "Subterfuge"
- `character.ability.animal-empathy.display-name`: "Animal Empathy"
- `character.ability.crafts.display-name`: "Crafts"
- `character.ability.drive.display-name`: "Drive"
- `character.ability.etiquette.display-name": "Etiquette"
- `character.ability.firearms.display-name": "Firearms"
- `character.ability.leadership.display-name": "Leadership"
- `character.ability.melee.display-name": "Melee"
- `character.ability.performance.display-name": "Performance"
- `character.ability.stealth.display-name": "Stealth"
- `character.ability.survival.display-name": "Survival"
- `character.ability.computer.display-name": "Computer"
- `character.ability.enigmas.display-name": "Enigmas"
- `character.ability.investigation.display-name": "Investigation"
- `character.ability.law.display-name": "Law"
- `character.ability.linguistics.display-name": "Linguistics"
- `character.ability.medicine.display-name": "Medicine"
- `character.ability.occult.display-name": "Occult"
- `character.ability.politics.display-name": "Politics"
- `character.ability.rituals.display-name": "Rituals"
- `character.ability.science.display-name": "Science"

**Coverage: 30/30 (100%)**

### pt-BR (`current-slice.json`)

All 30 abilities have display names matching source terminology:
- `character.ability.alertness.display-name`: "Prontidao"
- `character.ability.athletics.display-name`: "Esportes"
- `character.ability.brawl.display-name`: "Briga"
- `character.ability.dodge.display-name`: "Esquiva"
- `character.ability.empathy.display-name`: "Empatia"
- `character.ability.expression.display-name`: "Expressao"
- `character.ability.intimidation.display-name`: "Intimidacao"
- `character.ability.primal-instinct.display-name`: "Instinto Primitivo"
- `character.ability.streetwise.display-name`: "Manha"
- `character.ability.subterfuge.display-name`: "Labia"
- `character.ability.animal-empathy.display-name`: "Empatia com Animais"
- `character.ability.crafts.display-name`: "Oficios"
- `character.ability.drive.display-name`: "Conducao"
- `character.ability.etiquette.display-name`: "Etiqueta"
- `character.ability.firearms.display-name`: "Armas de Fogo"
- `character.ability.leadership.display-name`: "Lideranca"
- `character.ability.melee.display-name`: "Armas Brancas"
- `character.ability.performance.display-name`: "Performance"
- `character.ability.stealth.display-name`: "Furtividade"
- `character.ability.survival.display-name`: "Sobrevivencia"
- `character.ability.computer.display-name`: "Computador"
- `character.ability.enigmas.display-name`: "Enigmas"
- `character.ability.investigation.display-name`: "Investigacao"
- `character.ability.law.display-name`: "Direito"
- `character.ability.linguistics.display-name`: "Linguistica"
- `character.ability.medicine.display-name`: "Medicina"
- `character.ability.occult.display-name`: "Ocultismo"
- `character.ability.politics.display-name`: "Politica"
- `character.ability.rituals.display-name`: "Rituais"
- `character.ability.science.display-name`: "Ciencia"

**Coverage: 30/30 (100%)**

---

## 8. Affected Completeness Rows

| Domain | Previous mechanicalCompleteness | Current mechanicalCompleteness | Previous currentSliceExecutable | Current currentSliceExecutable | Exact Reason |
|--------|--------------------------------|--------------------------------|--------------------------------|--------------------------------|--------------|
| Ability allocation | false | true | true | true | Complete 30-ability catalog with Lupus restrictions |

### Correct totals

- **Previous:** 22/68 mechanically complete, 34/68 current-slice executable
- **Current:** 23/68 mechanically complete, 34/68 current-slice executable
- **Net change:** +1 mechanical, +1 current-slice

---

## 9. Mechanical Completeness Before/After

| Metric | Before | After | Change |
|--------|--------|-------|--------|
| Mechanically complete | 22/68 (32.4%) | 23/68 (33.8%) | +1 |
| Current-slice executable | 34/68 (50.0%) | 34/68 (50.0%) | 0 |
| Incomplete | 34/68 (50.0%) | 33/68 (48.5%) | -1 |

---

## 10. Current-Slice Executable Before/After

Ability allocation moves from partial-executable (18 abilities) to complete current-slice catalog (30 abilities). The domain was already current-slice executable; this change elevates it to mechanically complete because the full current-slice catalog is now materialized.

---

## 11. Functional-Dashboard Impact

- Ability allocation domain transitions from `mechanicalCompleteness: false` to `mechanicalCompleteness: true`
- `packageSourceStatus` updated from "18 Abilities" to "30 Abilities"
- All 30 Ability identifiers are now declared in `WerewolfAbilitySelection.cs`
- All 30 Ability display names exist in both `en/current-slice.json` and `pt-BR/current-slice.json`
- Lupus base restrictions expanded from 5 to 9 abilities
- No breaking changes to existing stable Ability keys

---

## 12. Exact Files Changed

### Modified

- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfAbilitySelection.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfAbilitySelectionTests.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/Localization/en/current-slice.json`
- `rule-sets/Chronicle.RuleSets.Werewolf/Localization/pt-BR/current-slice.json`
- `docs/reviews/werewolf-rule-set-completeness/completeness-matrix.json`
- `docs/reviews/werewolf-rule-set-completeness/completeness-report.md`

### New

- `docs/reviews/werewolf-rule-set-completeness/completion-evidence/RULESET-COMPLETION-006.md` (this document)

---

## 13. Test Totals

### WerewolfAbilitySelectionTests

- `EveryAbility` TheoryData now covers 30 abilities (was 18)
- `ValidLupusAllocation` includes all 30 abilities with correct ratings
- `AbilitiesInCategory` helper updated for all 3 categories with correct ability counts
- `BudgetRatings` updated for all 3 categories with 10 abilities each
- `EnforcesApprovedLupusBaseRestrictions` tests all 9 restricted abilities
- `AllowsLupusAllocationWhenRestrictedAbilitiesRemainAtZero` asserts 0 for all 9 restricted abilities

### Test Method Changes

| Method | Change |
|--------|--------|
| `SupportsEveryCanonicalAbilityIdentifier` | Parameter set expanded to 30 |
| `AllocatesValidCompletePayloadForEveryPriorityOrdering` | Uses updated ValidAllocation (30 abilities) |
| `AccountsForAuthoritativeZeroBaseRatingConvention` | Uses updated budgets (13/9/5 for 10/10/10 abilities) |
| `EnforcesApprovedLupusBaseRestrictions` | Added 4 new restricted ability assertions |
| `AllowsLupusAllocationWhenRestrictedAbilitiesRemainAtZero` | Added 4 new restricted ability zero assertions |
| `RejectsMissingDuplicateUnknownMalformedRestrictedBelowMinimumAboveMaximumAndIncorrectTotals` | Uses updated ValidAllocation |
| `ReplacesPreviousAllocationAtomically` | Uses updated ValidAllocation |
| `UpdatesImmutablyAndPreservesPriorDraftState` | Uses updated ValidAllocation |
| `PriorityChangeClearsPriorAbilityAllocation` | Uses updated ValidAllocation |
| `RuntimeRegistryInvokesAbilityPriorityAndAllocation` | Uses updated ValidAllocation |

---

## 14. Package-Validator Result

Package validator tests pass: 8 passed, 0 failed.

---

## 15. Matrix Integrity

- `completeness-matrix.json` is valid JSON
- 68 domains present
- Ability allocation domain updated: `mechanicalCompleteness: true`, `currentSliceExecutable: true`
- `packageSourceStatus` updated from "18 Abilities" to "30 Abilities"

---

## 16. Localization Integrity

- `en/current-slice.json`: 30 Ability display names present, valid JSON
- `pt-BR/current-slice.json`: 30 Ability display names present, valid JSON
- All translations match source terminology from `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt`
- No invented translation distinctions that change mechanics

---

## 17. Git Diff -- Check

Clean. No whitespace errors.

---

## 18. Git Status

Modified files:
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfAbilitySelection.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfAbilitySelectionTests.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/Localization/en/current-slice.json`
- `rule-sets/Chronicle.RuleSets.Werewolf/Localization/pt-BR/current-slice.json`
- `docs/reviews/werewolf-rule-set-completeness/completeness-matrix.json`
- `docs/reviews/werewolf-rule-set-completeness/completeness-report.md`
- `docs/reviews/werewolf-rule-set-completeness/completion-evidence/RULESET-COMPLETION-005-AUDIT.md` (trailing whitespace cleanup)

New files:
- `docs/reviews/werewolf-rule-set-completeness/completion-evidence/RULESET-COMPLETION-006.md`

---

## 19. Unresolved Blockers

| Blocker | Owner | Status |
|---------|-------|--------|
| A-003: Lupus freebie spending timing | RULESET-COMPLETION-007 | Deferred |
| A-009: Ability catalog canonicalization | RULESET-COMPLETION-006 | **Resolved** |
| Soak and absorption | Combat package | Deferred (not owned by this package) |

All other blockers are resolved:
- Ability catalog canonicalized to 30 stable keys
- Lupus base restrictions expanded to 9 abilities
- Localization coverage complete for all 30 abilities in both languages
- Tests updated and passing
