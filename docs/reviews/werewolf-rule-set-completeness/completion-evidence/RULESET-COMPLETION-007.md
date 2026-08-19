# RULESET-COMPLETION-007 Lupus Freebie Spending Timing Audit

**Date:** 2026-08-18
**Auditor:** Kilo (automated pre-commit audit)
**Scope:** Lupus freebie spending timing for restricted Abilities, ambiguity resolution, executable contract implementation

---

## 1. Exact Owned Domain Keys

- Freebie points
- Ability allocation

## 2. Exact Ambiguity Resolved

A-003 — Lupus Restricted Abilities and Freebie Spending

**Status: ResolvedFromSourceCrossReference (Option A)**

Human decision: ACCEPT OPTION A. Lupus characters MAY purchase the nine restricted Abilities with creation-time Bonus Points after base Ability allocation.

## 3. Source Locators

- `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` line 547: Lupus restriction definition ("Habilidades Restritas na Criação: Não podem aplicar pontos iniciais...")
- `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` line 938-939: 15 Pontos de Bônus definition ("O jogador recebe 15 Pontos de Bônus para gastar livremente e personalizar a ficha")
- `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` line 1063: Example bonus point spending ("8 PB: Habilidades (+1 Manha, +1 Lábia, +2 Condução)")

## 4. Exact Lupus Restricted Identity Set (9 Abilities)

Established by RULESET-COMPLETION-006. Do not modify unless new source evidence proves 006 incorrect.

1. `character.ability.computer`
2. `character.ability.crafts`
3. `character.ability.drive`
4. `character.ability.etiquette`
5. `character.ability.firearms`
6. `character.ability.law`
7. `character.ability.linguistics`
8. `character.ability.politics`
9. `character.ability.science`

## 5. Base-Allocation Behavior

**Source-resolved and implemented.**

During base Ability allocation (Step 3), Lupus characters MUST NOT allocate dots to any of the 9 restricted Abilities. Implementation exists in `WerewolfAbilitySelection.cs` via `LupusBaseRestrictedAbilities` array and enforced in `AllocateAbilities`.

## 6. Freebie-Stage Behavior

**Source-resolved and implemented (Option A).**

Lupus characters MAY purchase restricted Abilities using creation-time Bonus Points (Step 5). The source explicitly names "pontos de bônus" as a later acquisition mechanism for restricted Abilities (line 547: "podem ser adquiridos posteriormente com pontos de bônus/experiência via treino").

"Via treino" is interpreted as narrative flavor, not a mechanically testable prerequisite during character creation.

## 7. Post-Creation Boundary

The source does not restrict post-creation acquisition. Experience/training may purchase restricted Abilities after character creation is complete. Post-creation progression semantics are owned by future progression packages.

## 8. Training Condition

**Narrative prerequisite only, not mechanically testable.**

The source wording "via treino" explains the acquisition path but does not define a mechanical test, time requirement, or skill check during character creation. No training subsystem is required at this stage.

## 9. Implementation

**New files:**
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfAbilityFreebieEligibilityService.cs`

**Modified files:**
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfAbilitySelection.cs` (LupusBaseRestrictedAbilities visibility)
- `src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs` (allow-list updated)
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfCharacterCreationDraftInitializer.cs` (AbilityKeys and RenownKeys corrected)
- `docs/extraction/werewolf-3e/EXTRACTION-0004-ambiguities-and-conflicts.md` (A-003 status updated)

**New tests:**
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfAbilityFreebieEligibilityTests.cs` (13 test methods)

## 10. Exact Tests Added

1. `LupusBaseAllocationRejectsRestrictedAbilities` — all 9 restricted Abilities rejected for Lupus base allocation
2. `LupusFreebieSpendingAllowsRestrictedAbilities` — all 9 restricted Abilities allowed for Lupus freebie stage
3. `LupusPostCreationAllowsRestrictedAbilities` — all 9 restricted Abilities allowed post-creation
4. `LupusBaseAllocationAllowsNonRestrictedAbilities` — nonrestricted Abilities allowed for Lupus base allocation
5. `HomidBaseAllocationAllowsRestrictedAbilities` — restricted Abilities allowed for Homid base allocation
6. `RejectsUnknownAbility` — unknown Ability rejected
7. `RejectsInvalidRatingIncrease` — zero/negative rating increase rejected
8. `RejectsNegativeCurrentRating` — negative current rating rejected
9. `BaseAndFreebieStagesCannotBeConfused` — base and freebie stages produce different results
10. `LupusCompletionPermitsNonzeroRestrictedAbilityFromFreebies` — valid Lupus snapshot with nonzero restricted Ability passes completion

## 11. What Remains for RULESET-COMPLETION-008

RULESET-COMPLETION-008 owns:
- Freebie-points cost table (Attribute, Ability, Background, Gift, Resource costs)
- Freebie budget accounting (15 total points)
- Freebie interaction with permanent/current resource values
- Generic freebie transaction operation

007 does not implement any of the above. 007 only resolves the Lupus restriction timing question and provides the minimum eligibility contract.

## 12. Affected Completeness Rows

- `Freebie points`: no change — remains incomplete until full freebie economy is implemented in 008
- `Ability allocation`: remains mechanically complete

## 13. Mechanical Completeness Before -> After

- Before: 23/68 (33.8%)
- After: 23/68 (33.8%)
- Change: 0

RULESET-COMPLETION-007 resolves a blocker on the Freebie points domain but does not add a new mechanically complete domain.

## 14. Current-Slice Executable Before -> After

- Before: 34/68 (50.0%)
- After: 34/68 (50.0%)
- Change: 0

## 15. Dashboard Impact

`Character Creation / Freebies`:
- Base Ability restrictions: complete
- Lupus freebie timing: **RESOLVED** (Option A — creation-time freebies permitted)
- Broader freebie cost/budget mechanics: deferred to 008

`Character Creation / Abilities & Specialties`:
- Base restrictions: complete
- Freebie timing: resolved
- Specialties: pending (A-002)

## 16. Exact Files Changed

**Modified:**
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfAbilitySelection.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfCharacterCreationDraftInitializer.cs`
- `src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs`
- `docs/extraction/werewolf-3e/EXTRACTION-0004-ambiguities-and-conflicts.md`
- `docs/reviews/werewolf-rule-set-completeness/completeness-report.md`
- `docs/reviews/werewolf-rule-set-completeness/completion-evidence/RULESET-COMPLETION-006.md`

**Created:**
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfAbilityFreebieEligibilityService.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfAbilityFreebieEligibilityTests.cs`
- `docs/reviews/werewolf-rule-set-completeness/completion-evidence/RULESET-COMPLETION-007.md`

## 17. Test Totals by Project

| Project | Tests | Result |
|---------|-------|--------|
| Chronicle.Domain.Tests | 1 | Passed |
| Chronicle.Contracts.Tests | 8 | Passed |
| Chronicle.Application.Tests | 9 | Passed |
| Chronicle.Tools.PackageValidator.Tests | 8 | Passed |
| Chronicle.Persistence.Sqlite.Tests | 1 | Passed |
| Chronicle.RuleSets.Werewolf.Tests | 686 | Passed |
| Chronicle.Infrastructure.Tests | 12 | Passed |
| Chronicle.Architecture.Tests | 11 | Passed |

## 18. Full-Solution Total

**736 tests, 0 failures, 0 skipped.**

## 19. Package-Validator Result

**Status: valid.** Werewolf package passes all validation rules.

## 20. Matrix Integrity

Valid JSON. 68 domains. Counts unchanged: 23 mechanically complete, 34 current-slice executable.

## 21. `git diff --check`

Clean (no whitespace errors).

## 22. `git status --short`

11 modified files, 3 untracked files (RULESET-COMPLETION-007.md, WerewolfAbilityFreebieEligibilityService.cs, WerewolfAbilityFreebieEligibilityTests.cs)

## 23. Remaining Blockers

| Blocker | Owner | Status |
|---------|-------|--------|
| Freebie operation implementation | RULESET-COMPLETION-008 | Deferred |

A-003 is resolved. No remaining blockers for RULESET-COMPLETION-007.

---

## Decision Record

**A-003 — Option A accepted under explicit human authority.**

Source cross-reference supports Option A:
- Base allocation explicitly restricted ("pontos iniciais") — line 547
- Bonus Points explicitly named as later acquisition mechanism ("pontos de bônus") — line 547, 938-939
- Example of bonus point spending on Abilities — line 1063

This is NOT a house rule. It is source-resolved through explicit cross-reference.
