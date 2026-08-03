# Werewolf Executable Completeness Audit

Status: completed
Date: 2026-08-03

## Purpose

This audit determines whether the Werewolf current-slice character-creation runtime can complete every character path it currently permits, using only repository-authoritative source registration, extraction, prototype decisions, accepted evidence, package source, localization, and runtime implementation.

The audit treats the documentation prototype as review and authoring evidence only. It does not treat prototype catalogs as executable package authority unless the data propagated into accepted decisions, package source, localization, and runtime implementation.

## Artifacts Produced

- `docs/reviews/werewolf-executable-completeness/executable-completeness-matrix.json`
- `docs/reviews/werewolf-executable-completeness/supported-path-matrix.json`
- `docs/reviews/werewolf-executable-completeness/discrepancy-register.json`
- `docs/reviews/werewolf-executable-completeness/executable-completeness-audit-report.md`

## Authority Inspected

Primary source registration and extraction:

- `docs/extraction/werewolf-3e/EXTRACTION-0001-source-inventory.md`
- `docs/extraction/werewolf-3e/EXTRACTION-0002-content-classification.md`
- `docs/extraction/werewolf-3e/EXTRACTION-0003-character-creation-slice.md`
- `docs/extraction/werewolf-3e/EXTRACTION-0004-ambiguities-and-conflicts.md`
- `docs/extraction/werewolf-3e/EXTRACTION-0005-contract-findings.md`

Prototype decisions and accepted evidence:

- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/prototype-readiness.json`
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/prototype-work-status.json`
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/reviews/current-slice-boundary-review.json`
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/reviews/creation-mechanics-review.json`
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/reviews/initial-gift-review-record.json`
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/reviews/race-review-record.json`
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/reviews/auspice-review-record.json`
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/reviews/tribe-review-record.json`
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/reviews/metis-deformity-review-record.json`
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/reviews/attribute-review-record.json`
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/reviews/ability-review-record.json`
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/reviews/background-review-record.json`
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/reviews/rank-review-record.json`

Package source, localization, and runtime:

- `rule-sets/Chronicle.RuleSets.Werewolf/Metadata/werewolf.package-manifest.json`
- `rule-sets/Chronicle.RuleSets.Werewolf/Metadata/current-slice.json`
- `rule-sets/Chronicle.RuleSets.Werewolf/Localization/en/current-slice.json`
- `rule-sets/Chronicle.RuleSets.Werewolf/Localization/pt-BR/current-slice.json`
- `rule-sets/Chronicle.RuleSets.Werewolf/WerewolfReferenceRuntime.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfCharacterCreationDraftContracts.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfCharacterCreationDraftInitializer.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRaceSelection.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfAuspiceSelection.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfTribeSelection.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfMetisDeformitySelection.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfInitialGiftSelection.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfAttributePrioritySelection.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfAttributeAllocation.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfAbilitySelection.cs`

## Verdict

The Werewolf current executable runtime is not complete for any currently permitted Race x Auspice x Tribe character path.

The package can initialize a draft and perform several explicit current-slice selections, but it cannot reach a valid completion state because required downstream creation data and operations are absent from runtime implementation. The strongest contradiction is that runtime accepts all five Auspices while executable initial Auspice Gift support exists only for Ragabash and Philodox. Theurge, Galliard, and Ahroun paths are therefore permitted earlier than the runtime can complete.

## Supported Path Summary

Runtime-permitted path basis:

- Races: `homid`, `metis`, `lupus`
- Auspices: `ragabash`, `theurge`, `philodox`, `galliard`, `ahroun`
- Tribes: `glass-walkers`

Path counts:

- Runtime-permitted paths: 15
- Currently completable paths: 0
- Currently blocked paths: 15
- Paths blocked at initial Auspice Gift selection: 9
- Paths with implemented mandatory classification/Gift selections but blocked later by allocations, resources, identity, and completion validation: 6

## Key Findings

The current runtime preserves important authority boundaries: it does not execute Gift effects, does not allow additional Gift purchase, and does not perform persistence, Campaign binding, UI, provider access, network access, filesystem access, or randomness.

However, documented current-slice completion requires more than the runtime currently provides. Missing executable areas now include Background allocation/restrictions, initial resources, initial Renown, Rank, narrative or identity fields required for completion, and completion validation.

The broad prototype Gift catalog contains candidate level-one Auspice Gift entries for all five Auspices, but the accepted executable Gift review approves only six current-slice identities: one Race Gift for each Race, one Auspice Gift for Ragabash, one Auspice Gift for Philodox, and one Tribe Gift for Glass Walkers. The audit therefore records the missing Theurge, Galliard, and Ahroun executable Gift support as a decision/runtime gap, not as data that can be silently promoted from prototype catalog to runtime.

Package metadata has been corrected through IMPLEMENT-016 to describe implemented character-creation operations through Attribute and Ability allocation increments. The package remains a partial executable increment, not a complete character-creation implementation.

## Required Remediation Work Packages

- REM-WEC-001: Decide and implement executable Auspice completion scope. Either approve and implement current-slice initial Auspice Gifts for Theurge, Galliard, and Ahroun, or restrict Auspice selection so unsupported Auspices are not presented as completable.
- REM-WEC-002: Implement individual Attribute allocation using the approved categories, catalog, and priority budgets. Status: remediated by IMPLEMENT-015.
- REM-WEC-003: Implement Ability priority and allocation, including approved Lupus restrictions. Status: remediated by IMPLEMENT-016.
- REM-WEC-004: Implement Background allocation and Glass Walker restriction handling from accepted evidence.
- REM-WEC-005: Implement initial resources, Auspice-derived Renown, and initial Rank.
- REM-WEC-006: Implement completion-required identity/narrative field capture.
- REM-WEC-007: Implement completion validation, ledger/package binding evidence, invalidation rules, and atomic finalization.
- REM-WEC-008: Correct Werewolf package metadata and localization status claims after the executable scope decision is made.

## Implementation Scope Note

IMPLEMENT-014 and IMPLEMENT-015 remain independently valid Attribute increments. IMPLEMENT-016 adds Ability priority/allocation and approved Lupus base restriction enforcement without claiming complete character creation or implementing omitted downstream completion behavior.
