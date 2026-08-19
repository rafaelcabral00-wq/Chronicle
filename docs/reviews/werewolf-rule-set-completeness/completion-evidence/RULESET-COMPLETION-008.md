# RULESET-COMPLETION-008 Freebie Points Interaction with Resources Audit

**Date:** 2026-08-18
**Auditor:** Kilo (automated pre-commit audit)
**Scope:** Freebie points cost table, creation limits, resource interaction, executable contract implementation

---

## 1. Exact Owned Domain Keys

- Freebie points
- Resource initialization

## 2. Exact Ambiguities Resolved

A-006 — Permanent and Current Resource Initialization

**Status: ResolvedFromSource**

A-007 — Freebie Points and Creation Limits

**Status: ResolvedFromSource**

## 3. Source Locators

- `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` line 938-939: 15 Pontos de Bônus definition
- `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` line 988-997: Bonus cost table
- `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` line 906-908: Attribute creation maximum (5)
- `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` line 918-920: Ability base limit (3) and bonus point exception (levels 4-5)
- `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` line 1063-1068: Bonus point spending example

## 4. Freebie Cost Table (Source-Explicit)

| Category | Cost Per Unit | Creation Maximum | Source Locator |
|---|---|---|---|
| Attribute | 5 | 5 | Line 991, 908 |
| Ability | 2 | 5 | Line 992, 920 |
| Background | 1 | 5 | Line 993 |
| Gift (Level 1 only) | 7 per Gift | 1 | Line 994 |
| Rage | 1 | None stated | Line 995 |
| Gnosis | 2 | None stated | Line 996 |
| Willpower | 1 | None stated | Line 997 |

## 5. Budget

**15 total bonus points** (source line 939).

## 6. Resource Interaction Resolution (A-006)

**Resolved from source:**

- Listed resource values (Gnosis, Rage, Willpower) ARE permanent ratings.
- Current pool begins equal to permanent rating at initialization.
- Freebie points increase the permanent rating; current pool follows permanent.
- No explicit maximum stated for resources during creation.
- Resources are initialized in Step 5; base values first, then bonus point increases.

**Implementation:** Existing `WerewolfResourceRankInitialization.cs` already sets permanent = current. Freebie purchase service updates both permanent and current together.

## 7. Creation Limits Resolution (A-007)

**Resolved from source:**

- Ability above base limit of 3: YES (line 920)
- Attribute to 5: YES (line 908)
- Exceed normal maxima: NO
- New Background purchase: YES (example shows Ritos and Totem)
- Fourth Gift: YES (cost table includes Gifts)
- Renown/Rank change: NO (source-excluded, not ambiguous)
- Permanent resources only: YES

## 8. Renown/Rank Bonus Point Disposition

**NotApplicable / source-excluded.**

The source Bonus Point cost table (lines 988-997) explicitly lists only:
- Atributo, Habilidade, Antecedentes, Dons, Fúria, Gnose, Força de Vontade

Renown and Rank are NOT listed as purchasable categories. The source does not mention them in the Bonus Point context. This is not an ambiguity — they are simply not included.

No future owner required.

## 9. Resource Maximum Disposition

**NotApplicable / source-excluded.**

The source gives no explicit creation maximum for resources (Rage, Gnosis, Willpower) beyond the available Bonus Points. The current implementation enforces no additional cap, which matches source silence.

No future owner required.

## 10. Implementation

**New files:**
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfFreebieEligibilityService.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfFreebiePurchaseService.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfFreebieEligibilityTests.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfFreebiePurchaseServiceTests.cs`

**Modified files:**
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfCharacterCreationDraftContracts.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfCharacterCreationDraftInitializer.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfCharacterCompletion.cs`
- `src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs`
- `docs/extraction/werewolf-3e/EXTRACTION-0004-ambiguities-and-conflicts.md`
- `docs/reviews/werewolf-rule-set-completeness/completeness-report.md`
- `docs/reviews/werewolf-rule-set-completeness/completeness-matrix.json`

## 11. Eligibility vs Execution Support Matrix

Capability | Executable now? | Exact artifact | Owner if missing
---|---|---|---
A. catalog/cost knowledge | YES | `WerewolfFreebieCostCatalog` | -
B. single-purchase eligibility | YES | `WerewolfFreebieEligibilityService.CheckEligibility` | -
C. single-purchase cost calculation | YES | `WerewolfFreebieEligibilityService.CheckEligibility` | -
D. multi-purchase budget accounting | YES | `WerewolfFreebiePurchaseService.Purchase` | -
E. state mutation | YES | `WerewolfFreebiePurchaseService.ApplyPurchase` | -
F. purchase history/provenance | YES | `WerewolfFreebieLedgerEntry` + `FreebieLedger` in draft | -
G. resource permanent/current update | YES | `WerewolfFreebiePurchaseService.ApplyPurchase` | -
H. Attribute update | YES | `WerewolfFreebiePurchaseService.ApplyPurchase` | -
I. Ability update | YES | `WerewolfFreebiePurchaseService.ApplyPurchase` | -
J. Background update | YES | `WerewolfFreebiePurchaseService.ApplyPurchase` | -
K. Gift selection | YES | `WerewolfFreebiePurchaseService.ApplyPurchase` | -
L. Character Completion validation | YES | `WerewolfCharacterCompletionOperation.Complete` | -

## 12. 15-Point Budget Lifecycle

**Executable and tested (EndToEndFifteenPointLifecycleSucceeds):**

- Initial budget exactly 15: YES (`FreebieBudgetTotal = 15`)
- Purchase cost deducted: YES (`FreebieBudgetSpent` incremented by cost)
- Remaining balance cannot go negative: YES (validated before purchase)
- Multiple purchases accumulate: YES (ledger entries concatenated, spent summed)
- Repeated calls cannot reuse original budget: YES (immutable draft replacement required)
- Exact purchase provenance preserved: YES (ledger entries with ItemId, Category, Cost, ResultingRating, RequestId)
- Final Character reflects every accepted purchase: YES (snapshot includes FreebieLedger)
- Deterministic replacement/versioning: YES (DraftVersion incremented on each purchase)
- Invalid purchase leaves state unchanged: YES (returns null draft on failure)

## 13. Category Mutation Behavior

**Attribute:** cost 5, resulting permanent rating updated, maximum 5 enforced.
**Ability:** cost 2, ratings 4-5 allowed, Lupus A-003 behavior correctly applied (base rejected, freebie allowed).
**Background:** cost 1, existing and new purchase allowed, maximum 5 enforced.
**Gift:** cost 7, Level 1 only (max 1), added to Gifts list, duplicate rejected.
**Rage:** cost 1, permanent increases, current follows permanent.
**Gnosis:** cost 2, permanent/current behavior identical.
**Willpower:** cost 1, permanent/current behavior identical.

## 14. Completion-State Invariant / Provenance Validation

**Deterministic invariant enforced:**

`WerewolfCharacterCompletionOperation.Complete` validates that `FreebieBudgetSpent` equals the sum of all `FreebieLedger` entry costs (`FreebieBudgetLedgerMismatch` error code).

This prevents:
- Forged drafts with manually inflated trait ratings but zero ledger entries
- Forged drafts where `FreebieBudgetSpent` does not match actual purchase history
- Inconsistent state where budget accounting diverges from provenance trail

The invariant is: `FreebieBudgetSpent == FreebieLedger.Sum(entry => entry.Cost)`

Any draft violating this invariant is rejected at completion.

## 15. Freebie State Representation

Minimum immutable state introduced:

- `FreebieBudgetTotal` (int): total budget, initialized to 15
- `FreebieBudgetSpent` (int): cumulative spent points
- `FreebieLedger` (IReadOnlyList<WerewolfFreebieLedgerEntry>): accepted purchases with ItemId, Category, Cost, ResultingRating, RequestId
- `DraftVersion`: incremented on each purchase for deterministic versioning

No persistence authority. No mutable hidden global state. State transitions produce replacement creation state/draft.

## 16. Resource Permanent/Current Mutation Proof

**Executable and tested:**

- Initial Rage permanent/current = N/N
- Purchase +2 Rage with freebies → result = (N+2)/(N+2)
- Same behavior verified for Gnosis and Willpower

## 17. Tests Added

**WerewolfFreebieEligibilityTests.cs (13 tests):**
1. LupusBaseAllocationRejectsRestrictedAbilities
2. LupusFreebieSpendingAllowsRestrictedAbilities
3. LupusPostCreationAllowsRestrictedAbilities
4. LupusBaseAllocationAllowsNonRestrictedAbilities
5. HomidBaseAllocationAllowsRestrictedAbilities
6. RejectsUnknownAbility
7. RejectsInvalidRatingIncrease
8. RejectsNegativeCurrentRating
9. BaseAndFreebieStagesCannotBeConfused
10. LupusCompletionPermitsNonzeroRestrictedAbilityFromFreebies

**WerewolfFreebiePurchaseServiceTests.cs (28 tests):**
1. InitialBudgetIsFifteen
2. SinglePurchaseDeductsCorrectCost (7 inline cases)
3. MultiPurchaseAccumulatesDeductions
4. ZeroSpendLeadsToZeroSpentBudget
5. InsufficientBudgetRejectsPurchase
6. RejectedPurchaseDoesNotMutateState
7. AttributeMaximumIsEnforced
8. AbilityRatingsFourAndFiveAreAllowed (3 cases)
9. BackgroundPurchaseSucceeds
10. LevelOneGiftPurchaseSucceeds
11. RagePermanentAndCurrentIncreaseTogether
12. GnosisPermanentAndCurrentIncreaseTogether
13. WillpowerPermanentAndCurrentIncreaseTogether
14. LupusRestrictedAbilityViaBonusPointsSucceeds
15. CompletionWithValidLedgerSucceeds
16. CompletionRejectsOverspentFreebieBudget
17. CompletionRejectsForgedBudgetSpent
18. PurchaseProducesVersionedImmutableTransition
19. RepeatedCallsCannotReuseOriginalBudget
20. EndToEndFifteenPointLifecycleSucceeds

## 18. What Remains for Future Packages

- Post-creation experience-based purchases (RULESET-COMPLETION-012 / Progression)
- Renown/Rank freebie interaction: NotApplicable (source-excluded)
- Resource maximum enforcement: NotApplicable (source-excluded)

## 19. Affected Completeness Rows

- `Freebie points`: mechanicalCompleteness = true, currentSliceExecutable = true
- `Resource initialization`: A-006 resolved

## 20. Mechanical Completeness Before -> After

- Before: 23/68 (33.8%)
- After: 24/68 (35.3%)
- Change: +1 (Freebie points)

## 21. Current-Slice Executable Before -> After

- Before: 34/68 (50.0%)
- After: 35/68 (51.5%)
- Change: +1 (Freebie points)

## 22. Dashboard Impact

`Character Creation / Freebies`:
- Cost table: complete
- Creation limits: complete
- Resource interaction: complete
- Budget accounting: complete
- Transaction execution: complete
- Completion validation: complete
- Renown/Rank: NotApplicable (source-excluded)
- Resource maximum: NotApplicable (source-excluded)

## 23. Exact Files Changed

**Modified:**
- `docs/extraction/werewolf-3e/EXTRACTION-0004-ambiguities-and-conflicts.md`
- `docs/reviews/werewolf-rule-set-completeness/completeness-report.md`
- `docs/reviews/werewolf-rule-set-completeness/completeness-matrix.json`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfAbilitySelection.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfCharacterCompletion.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfCharacterCreationDraftContracts.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfCharacterCreationDraftInitializer.cs`
- `src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs`

**Created:**
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfAbilityFreebieEligibilityService.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfFreebieEligibilityService.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfFreebiePurchaseService.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfAbilityFreebieEligibilityTests.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfFreebieEligibilityTests.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfFreebiePurchaseServiceTests.cs`
- `docs/reviews/werewolf-rule-set-completeness/completion-evidence/RULESET-COMPLETION-008.md`

## 24. Test Totals by Project

| Project | Tests | Result |
|---------|-------|--------|
| Chronicle.Domain.Tests | 1 | Passed |
| Chronicle.Contracts.Tests | 8 | Passed |
| Chronicle.Application.Tests | 9 | Passed |
| Chronicle.Tools.PackageValidator.Tests | 8 | Passed |
| Chronicle.Persistence.Sqlite.Tests | 1 | Passed |
| Chronicle.RuleSets.Werewolf.Tests | 747 | Passed |
| Chronicle.Infrastructure.Tests | 12 | Passed |
| Chronicle.Architecture.Tests | 11 | Passed |

## 25. Full-Solution Total

**797 tests, 0 failures, 0 skipped.**

## 26. Package-Validator Result

**Status: valid.** Werewolf package passes all validation rules.

## 27. Matrix Integrity

Valid JSON. 68 domains. Counts updated: 24 mechanically complete, 35 current-slice executable.

## 28. `git diff --check`

Clean (no whitespace errors). Note: CRLF normalization warnings for matrix and 006 evidence are line-ending notices, not errors.

## 29. `git status --short`

14 modified files, 5 untracked files. `.kilo/` remains untracked.

## 30. Remaining Blockers

No remaining blockers for RULESET-COMPLETION-008 within current-slice scope.

Post-creation progression (experience-based freebies) is deferred to RULESET-COMPLETION-012.

---

## Decision Record

**A-006 — Resolved from source.** Resource values are permanent ratings; current follows permanent; freebies increase permanent.

**A-007 — Resolved from source.** Cost table and creation limits extracted directly from source text.

**Renown/Rank — NotApplicable.** Source Bonus Point cost table excludes Renown and Rank.

**Resource maximum — NotApplicable.** Source gives no explicit creation maximum for resources.
