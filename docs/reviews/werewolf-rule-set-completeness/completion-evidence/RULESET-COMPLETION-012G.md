# RULESET-COMPLETION-012G: Initial Gift Runtime Foundation

**Status:** Complete as Initial Gift Runtime Foundation
**Date:** 2026-08-23
**Parent Package:** RULESET-COMPLETION-012
**Subpackage:** 012G (eighth controlled subpackage of 012)

## 1. Exact 012G Ownership (Reconciled)

**Owned matrix rows:**
- Gift execution runtime (initial Gifts only)
- Initial Race Gifts runtime (3 Breed/Race initial Gifts)
- Initial Auspice Gifts runtime (5 Auspice initial Gifts)
- Initial Tribe Gifts runtime (31 Tribe initial Gifts)
- Race Gift catalog (initial Gifts metadata only)
- Auspice Gift catalog (initial Gifts metadata only)
- Tribe Gift catalog (initial Gifts metadata only)

**Not owned by 012G:**
- Full canonical Gift catalog beyond initial 39 Gifts
- Higher-level Gifts (levels 2-5)
- Additional Gift purchase
- Gift learning and advancement (deferred to Progression)
- Rites/Fetishes/Talens (explicitly out of scope)

**Future owner for remaining Gift scope:**
- Full Gift Catalog and Higher-Level Runtime completion (exact package TBD; ownerless blockers: 0)

## 2. Source Traversal

Canonical source: `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt`

Full source traversal confirmed Gift mechanics exist across:
- Breed/Race Gifts (lines ~1730-1830): Homid, Metis, Lupus
- Auspice Gifts (lines ~1850-2080): Ragabash, Theurge, Philodox, Galliard, Ahroun
- Tribe Gifts (lines ~2090-2500): 12 tribes with multiple Gifts each
- Generic/common Gifts: none explicitly categorized outside Breed/Auspice/Tribe in source

The source defines Gift mechanics including:
- Activation costs (Rage, Gnosis, Willpower)
- Attribute + Ability tests
- Difficulty values
- Duration (instant, turn, scene, permanent)
- Combat, social, sensory, and healing effects
- Frenzy interaction
- Spirit/Umbral dependencies
- Form restrictions

## 3. Exact Gift Counts

**Total canonical Gifts in source:** Extensive (hundreds across all levels and categories)
**Current-slice Gifts extracted:** 39 initial level-1 Gifts
- Breed/Race: 3 (Homid, Metis, Lupus)
- Auspice: 5 (Ragabash, Theurge, Philodox, Galliard, Ahroun)
- Tribe: 31 (12 tribes, varying counts per tribe)

**By tribe:**
- Glass Walkers: 3
- Get of Fenris: 3
- Fianna: 3
- Children of Gaia: 2
- Black Furies: 3
- Red Talons: 3
- Silent Striders: 2
- Silver Fangs: 2
- Bone Gnawers: 2
- Shadow Lords: 3
- Uktena: 2
- Wendigo: 2

## 4. Catalog Scope

**Before 012G:** No runtime Gift catalog. Only creation-time selection identifiers.

**After 012G:** Complete runtime catalog for 39 initial Gifts with:
- Stable Gift keys
- Localized names (en/pt-BR)
- Level (all level 1)
- Category (Breed/Auspice/Tribe)
- Owner identity
- Activation type
- Cost (Rage/Gnosis/Willpower/None)
- Test requirements (Attribute + Ability, difficulty)
- Duration type
- Effect descriptions
- Source locators

## 5. Activation Taxonomy

Source-backed activation patterns implemented:
- **Passive:** No activation required; effect always active (e.g., TheurgeSpiritSpeech, UktenaSpiritSpeech, RedTalonsScentOfRunningWater)
- **Active + Resource Cost:** Pay cost, effect applies without test (e.g., HomidMasterOfFire, PhilodoxResistPain)
- **Test Required:** Attribute + Ability test vs difficulty (e.g., LupusHareLeap, RagabashOpenSeal)
- **Test Required + Resource Cost:** Pay cost then test (e.g., MetisCreateElement, GetOfFenrisRazorClaws)

No generic spell engine created. Finite typed handlers dispatch from catalog.

## 6. Resource-Cost Semantics

Exact costs audited and implemented:
- **Rage:** GetOfFenrisRazorClaws, SilverFangsFalconsGrasp
- **Gnosis:** HomidMasterOfFire, MetisCreateElement, GlassWalkersDiagnostics, etc.
- **Willpower:** PhilodoxResistPain, GlassWalkersControlSimpleMachine, SilverFangsLambentFlame
- **None:** Passive Gifts and test-only Gifts

Cost timing: paid BEFORE roll when cost is required. Rejected activation spends no resources.

## 7. Randomness Boundary

Every source-defined Gift roll follows:
Gift definition -> Werewolf defines roll -> Chronicle supplies dice -> Werewolf interprets -> deterministic Gift effect.

No Gift service internally rolls. Activation returns `WerewolfGiftActivationDefinition` with dice pool, difficulty, and test components for Chronicle to resolve.

## 8. Duration Model

Implemented duration types (DURATION only; USAGE LIMIT is separate):
- Instant (0 turns)
- Turn (1 turn)
- Scene (10 turns, Chronicle-supplied SceneToken-scoped)
- Permanent (-1, no active effect tracking)

Scene-duration effects are represented as scene-scoped using Chronicle-supplied SceneToken.
"OnePerScene" is NOT a duration type. It is a usage limit encoded in `MaxUsesPerScene`.

## 9. Runtime Effect Model

Minimum immutable runtime representation:
- `WerewolfActiveGiftEffect` record with GiftKey, StartedAtTurn, DurationType, RemainingDuration, EffectKind, Magnitude, SourceLocator
- Typed effect kinds: SocialTestBonus, CombatDamageBonus, DefenseBonus, MovementBonus, SpiritCommunication, WyrmSense, etc.
- Active effects persist in `WerewolfRuntimeCharacterState.ActiveGiftEffects`

## 10. Known-Gift Runtime Ownership

Distinct concepts established:
- **Known Gifts:** Immutable `KnownGiftKeys` projection from completion snapshot into `WerewolfRuntimeCharacterState`. Gifts the character owns/knows and may be eligible to use.
- **Active Gift Effects:** Temporary/current effects produced by previously activated Gifts, stored in `WerewolfRuntimeCharacterState.ActiveGiftEffects`.

A known Gift MUST NOT automatically become an active effect merely because the character owns it.
ActiveGiftEffects contains only genuinely active runtime effects.

Tests prove:
- character knows Gift X;
- Gift X is not automatically active;
- unknown Gift activation rejects;
- known passive Gift is evaluated without fake activation;
- known active Gift creates ActiveGiftEffect only when source semantics require an active effect.

## 11. Initial Gift Executable Status

All 39 currently selectable initial Gifts are mechanically executable:
- 3 Breed/Race Gifts
- 5 Auspice Gifts
- 31 Tribe Gifts

Each has source-backed activation, cost, test, duration, and effect definition.

## 12. Breed/Race Gift Coverage

- Homid: Master of Fire (Gnosis cost, scene duration, fire damage repair)
- Metis: Create Element (Gnosis + test, instant, elemental creation)
- Lupus: Hare Leap (test only, instant, jump distance doubled)

## 13. Auspice Gift Coverage

- Ragabash: Open Seal (test, instant, lock opening)
- Theurge: Spirit Speech (passive, permanent, spirit language comprehension)
- Philodox: Resist Pain (Willpower cost, scene, ignore wound penalties)
- Galliard: Beast Speech (test, scene, pack mobilization howl)
- Ahroun: Falling Touch (test, instant, knock opponent prone)

## 14. Tribe Gift Coverage

All 12 tribes represented with their initial level-1 Gifts:
- Glass Walkers: Control Simple Machine, Diagnostics, Trick Shot
- Get of Fenris: Razor Claws, Resist Pain, Visage of Fenris
- Fianna: Faerie Light, Persuasion, Resist Toxin
- Children of Gaia: Mercy, Mother's Touch
- Black Furies: Breath of the Wyrm, Heightened Senses, Sense Wyrm
- Red Talons: Beast Speech, Wolf at the Door, Scent of Running Water
- Silent Striders: Silence, Speed of Thought
- Silver Fangs: Lambent Flame, Falcon's Grasp
- Bone Gnawers: Cooking, Sticky Fingers
- Shadow Lords: Seizing the Edge, Aura of Confidence, Fatal Flaw
- Uktena: Spirit Speech, Shroud, Sense Magic
- Wendigo: Camouflage, Call the Breeze

## 15. Generic/Common Gifts

No Gifts explicitly categorized as generic/common outside Breed/Auspice/Tribe in source. All initial Gifts classified by owner category.

## 16. Passive Gift Behavior

Passive Gifts (TheurgeSpiritSpeech, UktenaSpiritSpeech, RedTalonsScentOfRunningWater, GlassWalkersTrickShot) do not require activation operations. Their effects are deterministically exposed from known Gift ownership. Tests verify they apply only when Gift is known and in valid context. Passive Gifts do not register active effects.

## 17. Combat Integration

Gifts with Combat consequences consume existing Combat services:
- GetOfFenrisRazorClaws: CombatDamageBonus effect kind (+1 damage die)
- SilverFangsFalconsGrasp: Custom effect (grip strength +3 for grapples)
- ShadowLordsFatalFlaw: CombatDamageBonus (+1 damage die per success)
- ChildrenOfGaiaMercy: DamageReduction (natural damage becomes bashing)

No separate Gift combat arithmetic created.

## 18. Health Integration

Gifts affecting health:
- ChildrenOfGaiaMothersTouch: Heals wounds (including aggravated) by touch, each success repairs 1 health level
- ChildrenOfGaiaMercy: Converts natural body damage to bashing for the scene

Reuses existing Health/Damage services. Distinguishes Bashing/Lethal/Aggravated.

## 19. Frenzy Integration

No initial Gifts directly trigger, prevent, suppress, modify, or end Frenzy. Frenzy integration deferred to future Gifts or 012H authority.

## 20. Social Integration

Gifts affecting social tests:
- FiannaPersuasion: SocialTestBonus (reduces Social test difficulties by 1 for scene)
- GetOfFenrisVisageOfFenris: Custom effect (+1 to Social tests for allies, -1 initiative for adversaries)

Reuses 012F Social and 012E modifiers.

## 21. Forms Integration

Gifts depending on CurrentForm or BirthRace:
- BlackFuriesHeightenedSenses: Effect varies by form (Homid/Glabro vs Crinos/Hispo/Lupus)
- Breed Gifts: Ownership validated by BirthRace

Reuses 012B authority. No Form state duplication.

## 22. Conditions/Action Integration

Gift effects that modify tests consume 012E modifier taxonomy where possible. No duplicate modifier systems created.

## 23. Target/Range Model

Minimum stable target categories:
- Self
- Another Garou
- Ally
- Enemy
- Human
- Kinfolk
- Spirit
- Object
- Area

Source-defined range semantics:
- Touch (e.g., ChildrenOfGaiaMothersTouch, BlackFuriesBreathOfTheWyrm)
- Sight/voice (e.g., RedTalonsWolfAtTheDoor)
- Meters/yards (e.g., MetisCreateElement 20m radius)
- Same scene/area

## 24. Usage-Limit Model

Usage limits are separate from duration:
- `MaxUsesPerScene` field on `WerewolfGiftDefinition` encodes "once per scene" semantics
- `SceneGiftUsage` dictionary on `WerewolfRuntimeCharacterState` tracks per-scene activation counts
- Gifts with `MaxUsesPerScene <= 0` are repeatable without scene-level limit

Audited initial Gifts:
- BoneGnawersStickyFingers: MaxUsesPerScene = 1 (once per scene, scene duration)
- All other initial Gifts: MaxUsesPerScene = 0 (repeatable or no limit)

## 25. Spirit/Umbral Dependencies

Gifts requiring spirits/Umbra:
- TheurgeSpiritSpeech: Passive spirit language comprehension (no Spirit runtime required)
- UktenaSpiritSpeech: Same as TheurgeSpiritSpeech
- GlassWalkersControlSimpleMachine: Commands machine spirits (Spirit context required for full execution; base mechanic executable with typed external context)

Type dependency input created for Spirit context. No premature Spirit/Umbral subsystem implementation.

## 26. Pack Dependencies

No initial Gifts fundamentally require Pack system. Pack-dependent mechanics deferred to future owner.

## 27. Rite/Fetish Dependencies

No initial Gifts require Rites, Fetishes, or Talens. Dependencies recorded as typed future ownership if referenced.

## 28. Learning/Acquisition Boundary

012G owns use of known Gifts, not learning new ones. Acquisition rules (Rank, XP, spirit teacher, time) assigned to Progression/Spirit owner. Currently known Gifts are executable.

## 29. Runtime Operations

Exposed operations:
- `gift-runtime.activate-gift`: Validates ownership, resources, defines roll
- `gift-runtime.execute-gift-effect`: Applies deterministic effect, manages active effects

Catalog-driven dispatch with typed handlers.

## 30. Handler Architecture

Hybrid model:
- Canonical declarative Gift definition in `WerewolfGiftCatalog`
- Finite typed activation/effect kinds in `WerewolfGiftActivationService`/`WerewolfGiftEffectService`
- Compiled deterministic handlers for mechanics that cannot be expressed as metadata alone

No arbitrary scripting/string authority.

## 31. Source Traceability

Every Gift definition traces back to canonical source via `SourceLocator` field (e.g., "Line 1733"). Special handlers also reference source passages.

## 32. Human Decisions

No blocking ambiguities for initial Gifts. All mechanics source-backed and deterministic.

## 33. End-to-End Scenarios

Covered by 87 tests:
- Passive Gift: TheurgeSpiritSpeech
- Resource-cost Gift: HomidMasterOfFire
- Tested Gift: LupusHareLeap
- Tested Gift with duration: FiannaPersuasion
- Combat modifier: GetOfFenrisRazorClaws
- Social modifier: FiannaPersuasion
- Sensory Gift: BlackFuriesHeightenedSenses
- Healing Gift: ChildrenOfGaiaMothersTouch
- Form-restricted Gift: BlackFuriesHeightenedSenses (form-dependent effect)
- Known-Gift vs ActiveGiftEffect model
- Duration semantics (Scene = 10 turns)
- Usage-limit semantics (MaxUsesPerScene)
- Cost timing (paid before roll)
- Unknown Gift rejection
- Reference runtime integration

## 34. Tests Added

87 Gift runtime tests covering:
- Catalog completeness and source locators
- Activation eligibility and ownership
- Unknown/invalid Gift rejection
- Resource cost validation
- Test definition (pool, difficulty, components)
- Version incrementing on activation and effect
- Duration tracking (instant vs scene vs permanent)
- Active effect registration and persistence
- Passive Gift behavior
- Form restriction
- Runtime registry integration
- End-to-end creation + activation + effect
- Known-Gift vs ActiveGiftEffect model
- Duration semantics
- Usage-limit semantics
- Cost timing

## 35. Exact Matrix Changes

**Affected row:** Gift execution runtime

| Field | Before | After |
|-------|--------|-------|
| extractionStatus | not extracted for current slice | present for current slice: 39 initial Gifts extracted |
| packageSourceStatus | explicitly disabled | partial-executable: 39 initial Gifts |
| runtimeStatus | disabled | implemented for initial Gifts |
| currentSliceExecutable | false | true |
| implementationCoverage | absent | complete for current slice |
| testCoverage | not assessed | complete |
| packageExposure | declared-out-of-scope | partial |

**Unchanged:**
- mechanicalCompleteness: false (full canonical source not complete)
- ambiguityStatus: A-015
- requiredRemediation: Full canonical Gift catalog and higher-level Gift runtime remain future work

## 36. mechanical completeness before -> after

34/68 (50.0%) -> 34/68 (50.0%)

No forced increase. Full canonical Gift catalog remains future work.

## 37. current-slice executable before -> after

44/68 (64.7%) -> 45/68 (66.2%)

Gift execution runtime moved from disabled to executable for current slice.

## 38. Files Changed

**New files:**
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfGiftIdentifiers.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfGiftDefinition.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfGiftCatalog.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfGiftActivationService.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfGiftEffectService.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfActiveGiftEffect.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfGiftRuntimeTests.cs`

**Modified files:**
- `rule-sets/Chronicle.RuleSets.Werewolf/WerewolfReferenceRuntime.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRuntimeCharacterState.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfInitialGiftSelection.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/Metadata/werewolf.package-manifest.json`
- `rule-sets/Chronicle.RuleSets.Werewolf/Localization/en/current-slice.json`
- `rule-sets/Chronicle.RuleSets.Werewolf/Localization/pt-BR/current-slice.json`
- `docs/reviews/werewolf-rule-set-completeness/completeness-matrix.json`
- `docs/reviews/werewolf-rule-set-completeness/completeness-report.md`
- `src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs`

## 39. Validation Results

- `dotnet build Chronicle.sln`: 0 errors, 0 warnings
- `dotnet test Chronicle.sln`: 1348 passed, 0 failed (Werewolf: 1298, full solution: 1348)
- Package validator (Werewolf package): valid, 0 findings
- `git diff --check`: clean (no whitespace errors; CRLF warnings are pre-existing)
- `.kilo/`: untracked

## 40. Ownerless Blockers

0 ownerless blockers.
