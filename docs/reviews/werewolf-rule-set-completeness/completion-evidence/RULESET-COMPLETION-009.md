# RULESET-COMPLETION-009: Resolve Tribe Eligibility Restrictions

**Status:** Complete
**Priority:** Medium
**Completed:** 2026-08-19
**Owner:** Kilo (Chronicle Werewolf package)

## 1. Exact Owned Domain Keys

- `Tribe selection`
- `Background allocation`
- `Initial Gift selection`
- `Character completion validation`

## 2. Exact Completion Condition

A-016 resolved; all 12 Tribe eligibility restrictions explicitly extracted from source, classified by executability, and enforced where dependencies are available.

## 3. All 12 Tribe Eligibility Summaries

| Tribe | Race/Breed | Background Restrictions | Dependencies | Source Locator | Executable? |
|-------|-----------|------------------------|--------------|----------------|-------------|
| glass-walkers | None | Prohibited: Ancestors, Mentor, Pure Breed | None | Lines 654-655 | Yes |
| get-of-fenris | None | Prohibited: Contacts | None | Line 669 | Yes |
| fianna | None | None | None | Line 686 | Yes |
| children-of-gaia | None | None | None | Line 704 | Yes |
| black-furies | None | None | Female gender | Lines 712, 725 | Blocked |
| red-talons | Lupus only | Prohibited: Allies, Contacts, Resources | Lupus Race | Lines 733, 741 | Yes |
| silent-striders | None | Prohibited: Ancestors, Resources | None | Line 761 | Yes |
| silver-fangs | None | Required: Pure Breed >= 3 | Pure Breed Background | Lines 778-779 | Blocked |
| bone-gnawers | None | Prohibited: Ancestors, Pure Breed, Resources | None | Line 795 | Yes |
| shadow-lords | None | Prohibited: Allies, Mentor | None | Line 812 | Yes |
| uktena | None | None | None | Line 829 | Yes |
| wendigo | None | Prohibited: Contacts, Resources | None | Line 846 | Yes |

## 4. Race/Breed Restriction Matrix

| Tribe | Restriction | Source Lines | Executable? | Implementation |
|-------|-------------|--------------|-------------|----------------|
| Red Talons | Lupus only | 733, 970 | Yes | `WerewolfTribeEligibilityService.CheckRaceBreedEligibility` |

## 5. Sex/Gender Restriction Matrix

| Tribe | Restriction | Source Lines | Executable? | Implementation |
|-------|-------------|--------------|-------------|----------------|
| Black Furies | Female only | 712, 102 | No | `CheckDependencies` returns DependencyUnavailable; owner: RULESET-COMPLETION-012 (Character identity field implementation) |

## 6. Background Restriction Matrix

| Tribe | Type | Background | Minimum | Source Lines | Executable? |
|-------|------|------------|---------|--------------|-------------|
| Glass Walkers | Prohibited | Ancestors | 0 | 654, 965 | Yes |
| Glass Walkers | Prohibited | Mentor | 0 | 654, 965 | Yes |
| Glass Walkers | Prohibited | Pure Breed | 0 | 654, 965 | Yes |
| Get of Fenris | Prohibited | Contacts | 0 | 669, 966 | Yes |
| Red Talons | Prohibited | Allies | 0 | 741, 970 | Yes |
| Red Talons | Prohibited | Contacts | 0 | 741, 970 | Yes |
| Red Talons | Prohibited | Resources | 0 | 741, 970 | Yes |
| Silent Striders | Prohibited | Ancestors | 0 | 761, 971 | Yes |
| Silent Striders | Prohibited | Resources | 0 | 761, 971 | Yes |
| Silver Fangs | Required | Pure Breed | >= 3 | 778, 972 | Blocked |
| Bone Gnawers | Prohibited | Ancestors | 0 | 795, 973 | Yes |
| Bone Gnawers | Prohibited | Pure Breed | 0 | 795, 973 | Yes |
| Bone Gnawers | Prohibited | Resources | 0 | 795, 973 | Yes |
| Shadow Lords | Prohibited | Allies | 0 | 812, 974 | Yes |
| Shadow Lords | Prohibited | Mentor | 0 | 812, 974 | Yes |
| Wendigo | Prohibited | Contacts | 0 | 846, 976 | Yes |
| Wendigo | Prohibited | Resources | 0 | 846, 976 | Yes |

## 7. Silver Fang / Pure Breed Disposition

Source requires Pure Breed >= 3 for Silver Fangs (line 778). Pure Breed is not in the current executable Background catalog (`WerewolfBackgroundIdentifiers.Supported` contains only Allies, Contacts, Mentor, Resources, Rites).

**Disposition:** Dependency-blocked. `WerewolfTribeEligibilityService.CheckBackgroundMinimums` returns `DependencyUnavailable` when Pure Breed is absent. Character completion also rejects Silver Fangs with `TribeDependencyUnavailable`.

**Future owner:** RULESET-COMPLETION-012 (explicit scope: "full Background catalog").

## 8. Structural/Reference-Only Background Disposition

| Background | Tribes Affected | Status | Future Owner |
|------------|-----------------|--------|--------------|
| Ancestors | Glass Walkers, Silent Striders, Bone Gnawers | Prohibited; not in current slice | RULESET-COMPLETION-012 |
| Pure Breed | Glass Walkers, Bone Gnawers, Silver Fangs | Prohibited/required; not in current slice | RULESET-COMPLETION-012 |

These Backgrounds are referenced in source restriction tables but are not in the current executable catalog. Prohibitions are not enforced because the Backgrounds cannot be allocated. Required minimums (Silver Fangs) are flagged as dependency-blocked.

## 9. Gift Interaction

Verified that:
- Invalid Tribe eligibility cannot be bypassed via Tribe Gift selection
- `WerewolfInitialGiftSelectionService.SelectGift` requires a valid Tribe selection (Tribe must be set in draft)
- Changing Tribe clears TribeGift (`WerewolfTribeSelectionService.SelectTribe` line 120)
- All 12 Tribe Gift identifiers remain selectable for eligible Tribes

## 10. Character Completion Integration

`WerewolfCharacterCompletionOperation.Complete` now invokes `WerewolfTribeEligibilityService.CheckEligibility` when Tribe is present. New error codes:
- `TribeRaceBreedIneligible` — Red Talons with non-Lupus Race
- `TribeBackgroundMinimumNotMet` — Silver Fangs with Pure Breed < 3 (if Pure Breed becomes available)
- `TribeDependencyUnavailable` — Silver Fangs without Pure Breed catalog; Black Furies without gender field

## 11. Ambiguities Resolved

| ID | Status | Resolution |
|----|--------|------------|
| A-016 | ResolvedFromSource | Source table (lines 963-976) provides explicit per-Tribe restriction catalog. Narrative descriptions excluded from mechanical enforcement per A-016 rule. |

## 12. Remaining Blockers

| Blocker | Owner | Description |
|---------|-------|-------------|
| Pure Breed Background not in current slice | RULESET-COMPLETION-012 | Blocks Silver Fangs Pure Breed >= 3 requirement |
| Ancestors Background not in current slice | RULESET-COMPLETION-012 | Blocks Glass Walkers, Silent Striders, Bone Gnawers Pure Breed/Ancestors prohibitions |
| Gender field not in character model | RULESET-COMPLETION-012 | Blocks Black Furies female-only restriction |

No other blockers.

## 13. Affected Completeness Rows

- `Tribe selection` — updated runtimeStatus and packageSourceStatus
- `Background allocation` — no change (already complete for current slice)
- `Initial Tribe Gifts` — no change (already validates Tribe eligibility)
- `Character completion validation` — updated to include Tribe eligibility validation

Counts unchanged: 24/68 mechanically complete, 35/68 current-slice executable.

## 14. Tests Added

`WerewolfTribeEligibilityTests.cs` (49 tests):
- `CheckRaceBreedEligibilityReturnsExpectedResult` — 36 cases (12 tribes x 3 races)
- `CheckRaceBreedEligibilityProducesExpectedFinding` — 5 cases
- `SilverFangsDependencyUnavailableWhenPureBreedNotInBackgrounds` — 1 case
- `SilverFangsBackgroundMinimumNotMetWhenPureBreedBelowThree` — 1 case
- `SilverFangsEligibleWhenPureBreedMinimumMet` — 1 case
- `RejectsUnknownOrMalformedTribe` — 3 cases
- `AllTwelveCanonicalTribesAreRecognized` — 1 case
- `NoDuplicateKeysOrOrphanRestrictions` — 1 case

`WerewolfTribeSelectionTests.cs` (updated):
- Added `RejectsTribeSelectionForIneligibleRaceOrDependency` — 8 cases
- Added `RedTalonsLupusSelectionSucceeds` — 1 case
- Added `UnrestrictedTribesRemainSelectableForAllRaces` — 9 cases
- Updated `SelectsEveryCurrentSliceTribe` to use Homid race
- Updated `InitializesCorrectWillpowerForTribe` to exclude dependency-blocked tribes
- Updated `TribeGiftIsEligibleForTribe` to exclude dependency-blocked tribes

`WerewolfCharacterCompletionTests.cs` (updated):
- Added `RejectsRedTalonsWithHomidRace` — 1 case
- Added `RejectsRedTalonsWithMetisRace` — 1 case
- Added `AcceptsRedTalonsWithLupusRace` — 1 case
- Added `RejectsSilverFangsDueToPureBreedDependency` — 1 case
- Added `RejectsBlackFuriesDueToGenderDependency` — 1 case
- Added `AcceptsUnrestrictedTribesForAnyRace` — 9 cases

## 15. Files Changed

- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfTribeEligibilityService.cs` (new)
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfTribeSelection.cs` (modified)
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfCharacterCompletion.cs` (modified)
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfTribeEligibilityTests.cs` (new)
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfTribeSelectionTests.cs` (modified)
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfCharacterCompletionTests.cs` (modified)
- `src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs` (modified)
- `docs/reviews/werewolf-rule-set-completeness/completeness-matrix.json` (modified)
- `docs/reviews/werewolf-rule-set-completeness/completeness-report.md` (modified)

## 16. Validation Results

| Check | Result |
|-------|--------|
| Build | 0 errors, 0 warnings |
| Werewolf tests | 811 passed |
| Full solution tests | 861 passed |
| Package validator | valid |
| Matrix integrity | valid JSON |
| Whitespace check | clean |
| `.kilo/` status | untracked |

## 17. Before/After Counts

| Metric | Before | After |
|--------|--------|-------|
| Mechanically complete | 24/68 (35.3%) | 24/68 (35.3%) |
| Current-slice executable | 35/68 (51.5%) | 35/68 (51.5%) |
| Werewolf tests | 747 | 811 (+64) |
| Full solution tests | 797 | 861 (+64) |

Counts unchanged because Tribe selection, Background allocation, and Character completion validation were already marked complete/executable. This package hardened enforcement and added explicit eligibility validation.
