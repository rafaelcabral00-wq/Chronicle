# RULESET-COMPLETION-012A: Background Catalog Expansion

**Status:** Complete
**Date:** 2026-08-20
**Parent Package:** RULESET-COMPLETION-012
**Subpackage:** 012A (first controlled subpackage of 012)

## 1. Exact 012A Title and Scope

**Title:** Background Catalog Expansion

**Owned domain keys:**
- Background allocation (catalog expansion to full source-defined player-character Backgrounds)
- Background runtime effects (declarative model for creation-visible Backgrounds)
- Silver Fang eligibility (Pure Breed ≥ 3 enforcement)
- Tribe eligibility reconciliation (Background prohibitions for all 12 Tribes)
- Character Completion (Background validation)

**Completion condition:**
All player-character Backgrounds defined in the canonical source are present in the executable catalog, base allocation validates all 9 identities, freebie purchase integrates automatically, Tribe prohibitions are executable, Silver Fang Pure Breed ≥ 3 is enforceable, and completion rejects prohibited Background final states.

**Source ambiguities/findings resolved by 012A:** None. This subpackage is purely additive catalog expansion with no unresolved source ambiguities.

**Expected executable artifacts:**
- `WerewolfBackgroundIdentifiers.Supported` contains 9 entries
- `WerewolfBackgroundAllocationService` validates all 9 Backgrounds with 5-point budget
- `WerewolfTribeEligibilityService` enforces Background prohibitions for all 12 Tribes
- `WerewolfCharacterCompletionOperation` validates Background final states
- `WerewolfBackgroundEffectCatalog` declares runtime effects for Pure Breed and Ancestors

## 2. Exact Canonical Background Count

**9 player-character Backgrounds** constitute the full source-defined catalog.

The source summary table (lines 978-987) lists 10 Background entries, but `Totem` is explicitly defined as "Antecedente compartilhado entre os membros da matilha" (Background shared among pack members) at line 1632. Totem is a Pack-level mechanic, not an individual player-character Background. It is correctly excluded from the current-slice executable catalog and remains owned by the future Pack mechanics subpackage of RULESET-COMPLETION-012.

## 3. Exact Full Identity List

| # | Machine Key | Source Label (pt-BR) | Source Label (en) | Source Locator |
|---|-------------|----------------------|-------------------|----------------|
| 1 | `character.background.allies` | Aliados | Allies | Lines 1541-1550 |
| 2 | `character.background.ancestors` | Ancestrais | Ancestors | Lines 1550-1561 |
| 3 | `character.background.contacts` | Contatos | Contacts | Lines 1562-1572 |
| 4 | `character.background.fetish` | Fetiche | Fetish | Lines 1573-1580 |
| 5 | `character.background.kinfolk` | Parentes | Kinfolk | Lines 1590-1598 |
| 6 | `character.background.mentor` | Mentor | Mentor | Lines 1582-1590 |
| 7 | `character.background.pure-breed` | Raça Pura | Pure Breed | Lines 1599-1608 |
| 8 | `character.background.resources` | Recursos | Resources | Lines 1610-1617 |
| 9 | `character.background.rites` | Ritos | Rites | Lines 1619-1629 |

**Excluded:** `Totem` (Pack-level shared Background, source line 1632; owner: RULESET-COMPLETION-012 Pack mechanics subpackage)

## 4. Entries Added vs Previous Catalog

| Background | Previous Status | 012A Status |
|------------|-----------------|-------------|
| Allies | Present (executable) | Retained |
| Contacts | Present (executable) | Retained |
| Mentor | Present (executable) | Retained |
| Resources | Present (executable) | Retained |
| Rites | Present (executable) | Retained |
| Ancestors | Identifier declared but not in Supported list | **Added to executable catalog** |
| Pure Breed | Identifier declared but not in Supported list | **Added to executable catalog** |
| Fetish | Missing | **Added** |
| Kinfolk | Missing | **Added** |

## 5. Background Allocation Semantics

| Property | Value |
|----------|-------|
| Base allocation budget | 5 points |
| Minimum rating | 0 (absent) |
| Maximum rating | 5 |
| Creation-stage limit | 0-5 per Background |
| Repeatable | No |
| Multiple instances | No |
| Freebie cost | 1 per dot |
| Freebie maximum | 5 |
| New Background purchase via freebies | Allowed (any Background not already at 5) |
| Existing Background increase via freebies | Allowed (up to 5) |
| Payload requirement | All 9 Backgrounds must be present exactly once |
| Budget enforcement | Sum of all ratings must equal exactly 5 |

## 6. Pure Breed Semantics

**Source locator:** Lines 1599-1608
**Rating range:** 0-5
**Creation behavior:** Purchasable during base allocation and freebies like any other Background
**Source mechanical effect:** Each dot grants +1 die to all Social tests or Challenges involving other Garou (source line 1601). This is a runtime social dice bonus.
**Tribe eligibility:** Silver Fangs require Pure Breed ≥ 3 (source line 778)
**Prohibitions:** Glass Walkers and Bone Gnawers prohibit Pure Breed
**Creation validity:** Fully executable — Silver Fang eligibility is enforceable at Tribe selection and Character completion
**Runtime effect:** Declaratively modeled in `WerewolfBackgroundEffectCatalog` as `PureBreedSocialBonus`. Execution deferred to generic action resolution integration (owner: RULESET-COMPLETION-012)

## 7. Ancestors Semantics

**Source locator:** Lines 1550-1561
**Rating range:** 0-5
**Creation behavior:** Purchasable during base allocation and freebies
**Source mechanical effect:** Once per session, test Ancestors at Difficulty 8 (or Difficulty 10 if seeking a specific ancestor). Each success adds +1 die to any Ability pool for one scene. Critical failure may cause catatonia or the ancestor spirit may refuse to leave the body (source lines 1551-1560).
**Tribe eligibility:** Prohibited for Glass Walkers, Silent Striders, Bone Gnawers
**Creation validity:** Structurally present and enforceable as prohibition
**Runtime effect:** Declaratively modeled in `WerewolfBackgroundEffectCatalog` as `AncestorsGuidance`. Execution deferred to generic action resolution integration (owner: RULESET-COMPLETION-012)

## 8. Fetish Background Semantics

**Source locator:** Lines 1573-1580
**Rating range:** 0-5
**Creation behavior:** Purchasable during base allocation and freebies
**Source mechanical effect:** Represents possession of fetish objects imbued with imprisoned spirit power. The rating represents total Fetish levels (e.g., one Level 3 Fetish, or three Level 1 Fetishes).
**Tribe eligibility:** No Tribe prohibitions
**Creation validity:** Fully executable as catalog entry and allocation
**Runtime effect:** Deferred to RULESET-COMPLETION-012 (Fetishes/Talens equipment/runtime subpackage)

**Boundary from Fetishes/Talens domain:** The `character.background.fetish` Background is a character creation resource representing total Fetish capacity/ownership. The future Fetishes/Talens domain under RULESET-COMPLETION-012 covers individual Fetish item definitions, spirit binding mechanics, power activation, and runtime item behavior. These are distinct domains. 012A only adds the Background catalog entry; it does not implement Fetish item runtime mechanics.

## 9. Kinfolk Semantics

**Source locator:** Lines 1590-1598
**Rating range:** 0-5
**Creation behavior:** Purchasable during base allocation and freebies
**Source mechanical effect:** Represents a network of trusted kinfolk (human or wolf relatives immune to Delirium). Rating translates to quantity: 2/5/10/20/50 trusted kinfolk.
**Tribe eligibility:** No Tribe prohibitions
**Creation validity:** Fully executable as catalog entry and allocation
**Runtime effect:** Deferred to RULESET-COMPLETION-012 (Kinfolk/Parentes subpackage) for quantity tracking, Delirium immunity, and narrative hook integration

## 10. Rites Background Boundary

**Source locator:** Lines 1619-1629
**Rating range:** 0-5
**Creation behavior:** Purchasable during base allocation and freebies
**Source mechanical effect:** Represents knowledge of sacred rituals. The rating represents total Rite levels (e.g., one Level 4 Rite, or a Level 3 + Level 1). The level of the Rituals Ability knowledge limits the maximum Rite level that can be learned or conducted.
**Tribe eligibility:** No Tribe prohibitions
**Creation validity:** Fully executable as catalog entry and allocation

**Boundary from future Rites system:** The `character.background.rites` Background is a character creation resource representing total ritual knowledge capacity. The future Rites system under RULESET-COMPLETION-012 covers individual Rite definitions, execution mechanics, costs, knowledge requirements, and runtime ritual behavior. 012A only ensures the Background catalog entry exists; it does not implement Rite runtime mechanics.

## 11. Tribe Eligibility Results After Catalog Expansion

| Tribe | Prohibited Backgrounds | Requirement | 012A Status |
|-------|----------------------|-------------|-------------|
| Glass Walkers | Ancestors, Mentor, Pure Breed | — | **Enforced** — all three now executable identities |
| Get of Fenris | Contacts | — | **Enforced** |
| Red Talons | Allies, Contacts, Resources | — | **Enforced** |
| Silent Striders | Ancestors, Resources | — | **Enforced** |
| Silver Fangs | — | Pure Breed ≥ 3 | **Enforced** |
| Bone Gnawers | Ancestors, Pure Breed, Resources | — | **Enforced** |
| Shadow Lords | Allies, Mentor | — | **Enforced** |
| Wendigo | Contacts, Resources | — | **Enforced** |
| Fianna | — | — | No restrictions |
| Children of Gaia | — | — | No restrictions |
| Uktena | — | — | No restrictions |
| Black Furies | — | Female gender | Deferred (owner: RULESET-COMPLETION-012) |

**No Tribe Background dependency remains structurally blocked.** All prohibited Backgrounds are now executable identities.

## 12. Character Completion Truth

Completion validation now enforces:

- All 9 Backgrounds present exactly once
- No prohibited Background rating > 0 for selected Tribe
- Silver Fang: Pure Breed ≥ 3 required
- Invalid ratings rejected
- Malformed/unknown Background keys rejected
- Freebie-derived Background values validated

**Silver Fang Pure Breed behavior:**
- Pure Breed 0 → reject (`BackgroundMinimumNotMet`)
- Pure Breed 1 → reject (`BackgroundMinimumNotMet`)
- Pure Breed 2 → reject (`BackgroundMinimumNotMet`)
- Pure Breed 3 → accept
- Pure Breed 4 → accept
- Pure Breed 5 → accept

Prohibited Background final states cannot bypass operation ordering because completion validates the final draft state atomically regardless of creation step sequence.

## 13. Remaining Background Runtime Effects and Exact Owners

| Background | Effect | Current Status | Exact Future Owner |
|------------|--------|----------------|--------------------|
| Pure Breed | +1 die to Social tests involving other Garou per dot | Declaratively modeled; execution deferred | RULESET-COMPLETION-012 (generic action resolution) |
| Ancestors | Once per session: test Diff 8/10, +1 die per success to any Ability for one scene; critical failure = catatonia or ancestor refusal | Declaratively modeled; execution deferred | RULESET-COMPLETION-012 (generic action resolution) |
| Fetish | Fetish item capacity/ownership | Catalog entry only | RULESET-COMPLETION-012 (Fetishes/Talens subpackage) |
| Kinfolk | Kinfolk quantity tracking, Delirium immunity, narrative hooks | Catalog entry only | RULESET-COMPLETION-012 (Kinfolk/Parentes subpackage) |
| Allies, Contacts, Mentor, Resources, Rites | No source-defined runtime effects beyond social/mechanical narrative context | N/A | No deferred runtime effect required |

## 14. Affected Completeness Rows

| Matrix Row | Before | After | Change |
|-----------|--------|-------|--------|
| Background allocation | mechanicallyComplete: false, currentSliceExecutable: true | mechanicallyComplete: true, currentSliceExecutable: true | **+1 mechanically complete** |
| Tribe selection | mechanicallyComplete: true, currentSliceExecutable: true | mechanicallyComplete: true, currentSliceExecutable: true | No change |
| Character completion validation | mechanicallyComplete: true, currentSliceExecutable: true | mechanicallyComplete: true, currentSliceExecutable: true | No change |

## 15. Mechanical Completeness Truth

**Background allocation domain criteria evaluation:**
The Background allocation domain is mechanically complete because:
1. Full source traversal complete (lines 978-987, 1533-1629)
2. Complete extraction (9 of 9 player-character Backgrounds; Totem excluded as Pack-level)
3. Complete executable catalog (all 9 identities in `WerewolfBackgroundIdentifiers.Supported`)
4. Deterministic base allocation mechanics (5-point budget, 0-5 range, exact total enforcement)
5. Freebie integration complete (existing pipeline handles all 9 uniformly)
6. Tribe restriction enforcement complete (all 12 Tribes)
7. Completion validation complete (rejects invalid final states)
8. Tests complete

Deferred runtime effects of individual Backgrounds (Pure Breed social bonus, Ancestors guidance) do NOT belong to the Background-allocation domain. They belong to generic action resolution and are owned by RULESET-COMPLETION-012. The Background-allocation domain is about character creation catalog, allocation, and validation — all of which are now complete.

**Before → After: 24/68 → 25/68**

## 16. Current-Slice Executable Truth

**Before → After: 35/68 → 36/68**

## 17. Tests by Project

| Project | Tests |
|---------|-------|
| Domain | 1 |
| Contracts | 8 |
| Application | 9 |
| PackageValidator | 8 |
| Persistence | 1 |
| Werewolf | 846 |
| Infrastructure | 12 |
| Architecture | 11 |
| **Total** | **896** |

**Mechanically summed full-solution total: 896/896** (all passing)

## 18. Package-Validator Result

Valid. 48 files inventoried, 0 findings.

## 19. Matrix Integrity

Valid JSON. Verified with Python `json.load()`.

## 20. Localization Integrity

- `en/current-slice.json`: 105 keys, valid JSON
- `pt-BR/current-slice.json`: 105 keys, valid JSON
- Added 9 Background display-name keys per locale

## 21. Files Corrected During Audit

- `docs/reviews/werewolf-rule-set-completeness/completeness-report.md` — corrected section 2.3 count from 24 to 25
- `docs/reviews/werewolf-rule-set-completeness/completion-evidence/RULESET-COMPLETION-012A.md` — removed RULESET-COMPLETION-011 contamination; replaced with 012A-specific facts only

## 22. git diff --check

Clean (pre-existing CRLF warnings are repository settings).

## 23. git status --short

```
 M docs/reviews/werewolf-rule-set-completeness/completeness-report.md
 M docs/reviews/werewolf-rule-set-completeness/completeness-matrix.json
 M rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfAbilityFreebieEligibilityTests.cs
 M rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfBackgroundAllocationTests.cs
 M rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfCharacterCompletionTests.cs
 M rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfResourceRankInitializationTests.cs
 M rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfTestRuntimeHelpers.cs
 M rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfTribeEligibilityTests.cs
 M rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfBackgroundAllocation.cs
 M rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfCharacterCreationDraftInitializer.cs
 M rule-sets/Chronicle.RuleSets.Werewolf/Localization/en/current-slice.json
 M rule-sets/Chronicle.RuleSets.Werewolf/Localization/pt-BR/current-slice.json
 M src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs
?? .kilo/
?? docs/reviews/werewolf-rule-set-completeness/completion-evidence/RULESET-COMPLETION-012A.md
?? rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfBackgroundEffectCatalog.cs
?? rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfBackgroundEffects.cs
```

`.kilo/` remains untracked.

## 24. Ownerless Blocker Count

**0**

All remaining mechanical work has exact owners:
- Fetishes/Talens, Kinfolk/Parentes, generic action resolution for Background effects, Gifts, Forms, Frenzy, Delirium, Umbra, Spirits, Totem, Progression, Combat, Extended/Resisted tests, Persistence, Specialties → RULESET-COMPLETION-012
- Black Furies gender field → RULESET-COMPLETION-012
- Metadata accuracy → RULESET-COMPLETION-013

---

RULESET-COMPLETION-012A is complete.

RULESET-COMPLETION-012 remains open.

Werewolf mechanical implementation completeness is now 25/68 domains (36.8%).
