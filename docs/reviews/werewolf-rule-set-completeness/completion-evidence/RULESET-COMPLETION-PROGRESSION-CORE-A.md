# RULESET-COMPLETION-PROGRESSION-CORE-A

## 1. Source Locators

| Mechanic | Source Locator | Evidence |
|----------|---------------|----------|
| Attribute cost: current × 4 | `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` | AUDIT-WEREWOLF-PROGRESSION-2026-08-24.md |
| Ability cost: current × 2 | `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` | AUDIT-WEREWOLF-PROGRESSION-2026-08-24.md |
| New Ability cost: 3 XP | `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` | AUDIT-WEREWOLF-PROGRESSION-2026-08-24.md |
| own Breed/Auspice/Tribe Gift: level × 3 | `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` | AUDIT-WEREWOLF-PROGRESSION-2026-08-24.md |
| other Breed/Auspice/Tribe Gift: level × 5 | `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` | AUDIT-WEREWOLF-PROGRESSION-2026-08-24.md |
| Rage cost: current permanent rating | `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` | AUDIT-WEREWOLF-PROGRESSION-2026-08-24.md |
| Gnosis cost: current permanent rating × 2 | `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` | AUDIT-WEREWOLF-PROGRESSION-2026-08-24.md |
| Willpower cost: current permanent rating | `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` | AUDIT-WEREWOLF-PROGRESSION-2026-08-24.md |
| Backgrounds non-Totem: cannot increase with XP | `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` | AUDIT-WEREWOLF-PROGRESSION-2026-08-24.md |
| Totem XP cost: unresolved (A-012) | `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` lines 1633, 2820 | AUDIT-WEREWOLF-PROGRESSION-2026-08-24.md |
| Gift prerequisite: RankValue >= Gift level | `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` | AUDIT-WEREWOLF-PROGRESSION-2026-08-24.md |
| Specialty eligibility: trait rating >= 4 | `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` | AUDIT-WEREWOLF-PROGRESSION-2026-08-24.md |
| Willpower permanent loss on critical failure | `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` | AUDIT-WEREWOLF-PROGRESSION-2026-08-24.md |

## 2. Implemented Mechanics

### 2.1 XP State Model
- Added `UnspentXp` (int, default 0) to `WerewolfRuntimeCharacterState` (`CharacterCreation/WerewolfRuntimeCharacterState.cs:31`)
- Added `PostCreationAttributeRatings` and `PostCreationAbilityRatings` (IReadOnlyDictionary<string, int>, default null) to `WerewolfRuntimeCharacterState`
- Immutable record; no persistence implementation inside Rule Set
- No session lifecycle ownership
- Deterministic transitions via `with` expressions

### 2.2 Advancement Transaction Service
- `WerewolfAdvancementService.Advance` implements deterministic advancement transaction
- Validates target, calculates cost, validates eligibility, deducts XP, mutates state, increments version
- Rejected requests preserve state, XP, and RuntimeStateVersion

### 2.3 Cost Calculation Service
- `WerewolfAdvancementCostService.CalculateCost` provides deterministic cost calculation
- Supports: attribute, ability, new-ability, rage, gnosis, willpower, gift, other-gift, totem

### 2.4 Attribute Advancement
- Cost: `current rating × 4`
- Reads current rating from `PostCreationAttributeRatings` if present, otherwise from package binding `attributes`
- Stores incremented value in `PostCreationAttributeRatings`
- Package binding (immutable snapshot) is NEVER modified
- No post-creation max of 5 enforced

### 2.5 Ability Advancement / New Ability
- Existing Ability cost: `current rating × 2`
- New Ability cost: `3 XP`; new rating = 1
- Reads/writes via `PostCreationAbilityRatings`
- Lupus creation restrictions do not prohibit post-creation acquisition
- No creation base-allocation max of 3 applied to post-creation

### 2.6 Resource Advancement
- Rage: advances `RagePermanent`; cost = current `RagePermanent`
- Gnosis: advances `GnosisPermanent`; cost = current `GnosisPermanent × 2`
- Willpower: advances `WillpowerPermanent`; cost = current `WillpowerPermanent`
- Current values are NOT automatically refilled on permanent advancement

### 2.7 Background XP Prohibition
- Non-Totem Background advancement rejected with `BackgroundNotPurchasableWithExperience`
- Totem advancement rejected with `TotemExperienceCostUnresolved` (A-012)

### 2.8 Gift Cost / Rank Eligibility
- `WerewolfGiftAdvancementEligibilityService.Evaluate` calculates cost and validates eligibility
- Own category cost: `Gift level × 3`
- Other category cost: `Gift level × 5`
- Rank eligibility: `RankValue >= Gift level` (reads `rankValue` from package binding)
- Already-known check: rejects with `GiftAlreadyKnown`
- Does NOT mutate `KnownGiftKeys`; provides typed eligibility/cost boundary

### 2.9 Specialty Eligibility
- `WerewolfSpecialtyEligibilityService.Evaluate` determines eligibility
- Trait rating >= 4 is eligible
- Supports `attribute` and `ability` trait types
- Does NOT implement naming, applicability, exploding-10, or 1-protection

### 2.10 Willpower Permanent Loss
- **Deferred**: No clean deterministic integration point identified for critical-failure detection and Gift-use exemption
- No implementation added; no speculative architecture introduced

## 3. Regression Investigation

### 3.1 Root-cause groups for original 65 Werewolf failures

| Group | Count | Root Cause | Production Change Responsible |
|-------|-------|------------|------------------------------|
| Package source validation | 5 | New CORE-A source files not in hardcoded allow-list | `RuleSetPackageSourceValidation.cs` missing entries |
| Package discovery/registration | 12 | Validation failure caused empty ValidatedPackages, breaking catalog lookup | `RuleSetPackageSourceValidation.cs` |
| Runtime registration | 20 | Package discovery failure cascaded to runtime not registered | `RuleSetPackageSourceValidation.cs` |
| Runtime operation lookup | 25 | Runtime not registered caused OperationUndeclared/RuntimeNotRegistered | `RuleSetPackageSourceValidation.cs` |
| Operation ordering snapshot | 1 | Test expected exact operation list without new progression operations | `RuleSetRuntimeRegistryTests.cs` |

**All 65 failures were CORE-A regressions.** Baseline d3cf174 proves 0 failures in Werewolf tests (1311 passed) and PackageValidator tests (8 passed).

### 3.2 Root causes for original 2 PackageValidator failures

| Failure | Root Cause | Fix |
|---------|------------|-----|
| `ValidWerewolfPackageReturnsValidExitCode` | New source files undeclared in allow-list | Added to `RuleSetPackageSourceValidation.cs` |
| `PackagePathOutsideRepositoryIsValidatedWhenExplicitlySupplied` | Same undeclared resource validation failure | Added to `RuleSetPackageSourceValidation.cs` |

### 3.3 Baseline proof

Clean worktree at d3cf174 executed:
- Werewolf: 1311 passed, 0 failed
- PackageValidator: 8 passed, 0 failed

No pre-existing failures existed.

## 4. RuntimeCharacterState Compatibility Fix

### Problem
Adding `UnspentXp` as a positional parameter risked breaking existing positional constructions.

### Solution
- `UnspentXp` added as the last positional parameter with default value `0`
- `PostCreationAttributeRatings` and `PostCreationAbilityRatings` added as trailing parameters with default value `null!`
- All existing positional constructions in tests and production code continue to compile without modification
- `FromSnapshot` initializes new fields to empty dictionaries

### Compatibility verification
- 1348 Werewolf tests pass without modifying any test helper constructors
- No test required updating for `WerewolfRuntimeCharacterState` construction

## 5. XP Preservation Across Unrelated Runtime Transitions

### Verified transitions
- `character-runtime.spend-resource`: XP unchanged
- `character-runtime.recover-resource`: XP unchanged
- `character-runtime.apply-damage`: XP unchanged
- `character-runtime.recover-damage`: XP unchanged
- `character-runtime.regenerate`: XP unchanged
- `frenzy.enter/suppress/end`: XP unchanged
- `combat.*`: XP unchanged
- `gift-runtime.activate-gift`: XP unchanged
- `gift-runtime.execute-gift-effect`: XP unchanged

### Mechanism
All existing services use `with` record reconstruction which preserves all fields including `UnspentXp` and the new post-creation rating dictionaries.

### Regression tests added
- `ResourceTransitionPreservesUnspentXp`: spends Rage, verifies XP unchanged
- `AdvancementPreservesRuntimeStateVersionOnRejection`: rejected advancement preserves version

## 6. Post-creation Attribute Ownership Model

### Model
- Immutable creation snapshot stores baseline ratings in `PackageBinding["attributes"]`
- Mutable post-creation ratings live in `WerewolfRuntimeCharacterState.PostCreationAttributeRatings`
- Advancement reads from post-creation dictionary if present, otherwise falls back to snapshot
- Advancement writes ONLY to `PostCreationAttributeRatings`
- `PackageBinding` is NEVER mutated by progression

### Verification
- `AdvanceAttributeStoresRatingInPostCreationDictionaryAndPreservesPackageBinding`: confirms post-creation dict updated, snapshot unchanged

## 7. Post-creation Ability Ownership Model

### Model
- Same pattern as Attributes
- Immutable creation snapshot in `PackageBinding["abilities"]`
- Mutable post-creation ratings in `WerewolfRuntimeCharacterState.PostCreationAbilityRatings`
- New Ability acquisition writes to `PostCreationAbilityRatings` with rating 1

### Verification
- `AdvanceAbilityStoresRatingInPostCreationDictionaryAndPreservesPackageBinding`: confirms post-creation dict updated, snapshot unchanged

## 8. Creation Snapshot Immutability Confirmation

The `PackageBinding` dictionary inside `WerewolfRuntimeCharacterState` is never modified by any CORE-A operation. All post-creation mutations target:
- `UnspentXp`
- `PostCreationAttributeRatings`
- `PostCreationAbilityRatings`
- `RagePermanent` / `GnosisPermanent` / `WillpowerPermanent`
- `KnownGiftKeys`

The immutable creation snapshot evidence (`PackageBinding["attributes"]`, `PackageBinding["abilities"]`) remains unchanged.

## 9. Operation-Registration/Metadata Disposition

### Files modified for metadata/registration
| File | Change | Reason |
|------|--------|--------|
| `rule-sets/Chronicle.RuleSets.Werewolf/Metadata/werewolf.package-manifest.json` | Removed `"progression"` from `excludedMechanics` | Package must allow progression source files |
| `rule-sets/Chronicle.RuleSets.Werewolf/Metadata/current-slice.json` | Moved `"progression"` from `disabledCapabilities` to `supportedCapabilities` | Package must declare progression as supported |
| `src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs` | Added 5 new CORE-A source files to hardcoded allow-list | Package source validation must accept new files |
| `rule-sets/Chronicle.RuleSets.Werewolf/WerewolfReferenceRuntime.cs` | Added 4 operation constants, descriptors, handlers | Runtime registration |
| `rule-sets/Chronicle.RuleSets.Werewolf.Tests/RuleSetRuntimeRegistryTests.cs` | Updated operation ordering snapshot | Test reflects actual alphabetical sort |

### Operation registration consistency
| Operation | Exists | Handler | Registered | Contract allows | Validator accepts |
|-----------|--------|---------|------------|-----------------|------------------|
| `character-runtime.calculate-advancement-cost` | Yes | Yes | Yes | Yes (post-creation-character-operations) | Yes |
| `character-runtime.advance-trait` | Yes | Yes | Yes | Yes (post-creation-character-operations) | Yes |
| `character-runtime.evaluate-specialty-eligibility` | Yes | Yes | Yes | Yes (post-creation-character-operations) | Yes |
| `character-runtime.evaluate-gift-advancement` | Yes | Yes | Yes | Yes (post-creation-character-operations) | Yes |

## 10. Test Inventory / Results

| Test Category | Count | Result |
|---------------|-------|--------|
| Progression focused tests | 37 | 37 pass |
| Full Werewolf suite | 1348 | 1348 pass, 0 fail |
| Domain tests | 1 | 1 pass |
| Contracts tests | 8 | 8 pass |
| PackageValidator tests | 8 | 8 pass |
| Architecture tests | 1 | 1 pass |

## 11. Architecture Boundary Validation

- No new .NET package or assembly created
- Werewolf remains one Rule Set package
- `WerewolfRuntimeCharacterState` extended with `UnspentXp` and two post-creation rating dictionaries
- Creation snapshot (`PackageBinding`) is immutable and never modified by progression
- No persistence implementation inside Rule Set
- No session lifecycle ownership
- Deterministic transitions only
- No internal randomness

## 12. Remaining Blockers

| Blocker | Owner | Status |
|---------|-------|--------|
| A-012 Totem XP cost conflict | Human Decision | Unresolved |
| Rank advancement thresholds absent from source | Human Decision | Unresolved |
| GOV-RENOWN-001 runtime governance divergence | Owner assigned | Not ownerless |
| Willpower permanent loss on critical failure | Deferred | No clean integration point |

## 13. Ownerless Blockers

0

## 14. Integration Hotspot Files Touched

| File | Change |
|------|--------|
| `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRuntimeCharacterState.cs` | Added `UnspentXp`, `PostCreationAttributeRatings`, `PostCreationAbilityRatings` |
| `rule-sets/Chronicle.RuleSets.Werewolf/WerewolfReferenceRuntime.cs` | Registered 4 new operations and added 4 execute methods |
| `rule-sets/Chronicle.RuleSets.Werewolf/Metadata/werewolf.package-manifest.json` | Removed `"progression"` from `excludedMechanics` |
| `rule-sets/Chronicle.RuleSets.Werewolf/Metadata/current-slice.json` | Moved `"progression"` from `disabledCapabilities` to `supportedCapabilities` |
| `src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs` | Added 5 new CORE-A source files to allow-list |
| `rule-sets/Chronicle.RuleSets.Werewolf.Tests/RuleSetRuntimeRegistryTests.cs` | Updated operation ordering snapshot |
| `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfProgressionTests.cs` | New test class (37 tests) |

## 15. Exact Files Changed

### Modified
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRuntimeCharacterState.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/WerewolfReferenceRuntime.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/Metadata/werewolf.package-manifest.json`
- `rule-sets/Chronicle.RuleSets.Werewolf/Metadata/current-slice.json`
- `src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/RuleSetRuntimeRegistryTests.cs`

### New
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfProgressionTests.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfProgressionContracts.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfAdvancementCostService.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfAdvancementService.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfSpecialtyEligibilityService.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfGiftAdvancementEligibilityService.cs`

## 16. Git Status

```
 M rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRuntimeCharacterState.cs
 M rule-sets/Chronicle.RuleSets.Werewolf/Metadata/current-slice.json
 M rule-sets/Chronicle.RuleSets.Werewolf/Metadata/werewolf.package-manifest.json
 M rule-sets/Chronicle.RuleSets.Werewolf/WerewolfReferenceRuntime.cs
 M rule-sets/Chronicle.RuleSets.Werewolf.Tests/RuleSetRuntimeRegistryTests.cs
 M src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs
?? docs/reviews/werewolf-rule-set-completeness/completion-evidence/AUDIT-WEREWOLF-PROGRESSION-2026-08-24.md
?? docs/reviews/werewolf-rule-set-completeness/completion-evidence/RULESET-COMPLETION-PROGRESSION-CORE-A.md
?? rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfProgressionTests.cs
?? rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfAdvancementCostService.cs
?? rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfAdvancementService.cs
?? rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfGiftAdvancementEligibilityService.cs
?? rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfProgressionContracts.cs
?? rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfSpecialtyEligibilityService.cs
```
