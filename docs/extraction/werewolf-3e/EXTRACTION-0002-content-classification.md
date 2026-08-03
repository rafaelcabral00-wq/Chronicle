---
id: EXTRACTION-0002
title: Werewolf 3e Content Classification for Character Creation Slice
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-08-02
category: Rule Set Extraction
source_document_id: source.werewolf3e.cleaned-ptbr
source_document_version: 1
source_fingerprint: a4302e2938a137fb42d154c45decd43e02a19a2ba0eb3030b6eb1de942dab64a
slice_id: werewolf3e.character-creation.slice-001
normalization_policy: FidelityWithNormalization
publication_status: candidate-only
depends_on:
  - SPEC-0001
  - EXTRACTION-0001
related_to:
  - EXTRACTION-0003
  - EXTRACTION-0004
  - EXTRACTION-0005
---

> **"Classification decides where information belongs before implementation decides how it runs."**

# Werewolf 3e Content Classification for Character Creation Slice

## 1. Purpose

This document classifies the source material required for the first Werewolf vertical slice.

The slice covers:

```text
Character concept
Race
Auspice
Tribe
Attributes
Abilities
Backgrounds
initial Gifts
initial Renown
Rank
Rage
Gnosis
Willpower
freebie points
specializations
one generic Dice test
```

The purpose is to separate:

- authoritative mechanical candidates;
- Character fields;
- catalogs;
- creation operations;
- Dice rules;
- terminology;
- narrative guidance;
- examples;
- ambiguities;
- deferred content.

This document does not yet approve the extracted rules.

## 2. Classification Method

Each source concept receives:

```text
Primary Classification
Secondary Tags
Target Package Artifact
Runtime Authority
Review Requirement
Slice Decision
```

Possible Slice Decisions:

```text
Include
IncludeAsCandidate
IncludeAsGuidance
ReferenceOnly
Defer
Exclude
NeedsClarification
```

## 3. Slice Boundary

### Included

The first slice includes enough material to:

- create a valid Garou Character candidate;
- render the initial Character sheet;
- validate the principal allocation budgets;
- initialize core resources;
- select initial Gifts;
- calculate freebie spending;
- create one generic Attribute + Ability Dice pool;
- resolve a basic success test candidate;
- preserve source provenance.

### Deferred

The first slice defers:

- full combat;
- form modifiers;
- Frenzy;
- damage and soak;
- regeneration;
- Renown advancement;
- experience progression;
- complete Gift execution;
- Rites;
- Totems as executable group mechanics;
- pack creation;
- Umbra;
- spirit systems;
- antagonist creation.

## 4. High-Level Classification Summary

```text
CharacterFieldDefinition:
    high volume

CharacterCreationRule:
    high volume

MechanicalConstraint:
    high volume

MechanicalOperation:
    medium volume

MechanicalResolutionRule:
    medium volume

ResourceRule:
    medium volume

Terminology:
    high volume

NarrativeGuidance:
    medium volume

Example:
    medium volume

Ambiguous:
    nontrivial

Deferred:
    high outside slice
```

## 5. Character Identity Classification

### 5.1 Character Concept

```text
Source Concept:
    Character concept and basic identity

Primary Classification:
    CharacterFieldDefinition

Secondary Tags:
    user-authored
    narrative
    creation

Target Artifact:
    character-model/fields/identity.json
    character-sheet/sections/identity.json

Runtime Authority:
    UserAuthored

Slice Decision:
    Include
```

Recommended candidate fields:

```text
character.identity.name
character.identity.player-name
character.identity.chronicle-name
character.identity.concept
character.identity.pack-name
character.identity.deed-name
character.identity.appearance
character.identity.notes
```

Only fields explicitly supported by the source should be included in the first prototype. Others remain candidate placeholders.

### 5.2 Race

```text
Primary Classification:
    CharacterFieldDefinition
    CharacterCreationRule

Secondary Tags:
    catalog
    initialization
    restriction
    terminology

Target Artifacts:
    character-model/fields/race.json
    character-model/catalogs/races.json
    character-model/creation/select-race.json
    rules/character-creation/race/
```

Candidate catalog entries:

```text
character.race.homid
character.race.metis
character.race.lupus
```

Source-derived mechanical candidates:

```text
Homid:
    initial Gnosis 1
    no initial Ability restrictions identified in the summarized creation material
    choose one initial Race Gift from the listed Race catalog

Metis:
    initial Gnosis 3
    mandatory deformity
    natural form Crinos
    choose one initial Race Gift from the listed Race catalog

Lupus:
    initial Gnosis 5
    restricted initial Ability allocation
    choose one initial Race Gift from the listed Race catalog
```

Slice Decision:

```text
IncludeAsCandidate
```

### 5.3 Auspice

```text
Primary Classification:
    CharacterFieldDefinition
    CharacterCreationRule
    ResourceRule

Secondary Tags:
    catalog
    initialization
    role
    gift-selection
    renown

Target Artifacts:
    character-model/fields/auspice.json
    character-model/catalogs/auspices.json
    character-model/creation/select-auspice.json
    rules/character-creation/auspice/
```

Candidate entries:

```text
character.auspice.ragabash
character.auspice.theurge
character.auspice.philodox
character.auspice.galliard
character.auspice.ahroun
```

Source-derived mechanical candidates:

```text
Ragabash:
    initial Rage 1

Theurge:
    initial Rage 2

Philodox:
    initial Rage 3

Galliard:
    initial Rage 4

Ahroun:
    initial Rage 5
```

The source also associates initial Renown and one initial Gift with Auspice.

Slice Decision:

```text
IncludeAsCandidate
```

Role descriptions, mood, crescent/waning personality distinctions, and social expectations are classified separately as Narrative Guidance.

### 5.4 Tribe

```text
Primary Classification:
    CharacterFieldDefinition
    CharacterCreationRule
    MechanicalConstraint
    ResourceRule

Secondary Tags:
    catalog
    initial-willpower
    background-restriction
    gift-selection
    terminology
    narrative-guidance

Target Artifacts:
    character-model/fields/tribe.json
    character-model/catalogs/tribes.json
    character-model/creation/select-tribe.json
    rules/character-creation/tribe/
```

Each Tribe candidate may define:

```text
entryKey
initialWillpower
allowedBackgrounds
prohibitedBackgrounds
requiredBackgroundMinimums
initialGiftOptions
display resources
narrative guidance references
provenance
```

Slice Decision:

```text
IncludeAsCandidate
```

Only Tribes with mechanically complete source records should be enabled in the first executable prototype. Incomplete entries remain disabled candidates.

## 6. Attributes Classification

### 6.1 Attribute Categories

```text
Primary Classification:
    CharacterFieldDefinition
    CharacterCreationRule

Target Artifact:
    character-model/catalogs/attribute-categories.json
```

Candidate categories:

```text
attribute-category.physical
attribute-category.social
attribute-category.mental
```

### 6.2 Attribute Fields

Candidate fields:

```text
character.attribute.strength
character.attribute.dexterity
character.attribute.stamina

character.attribute.charisma
character.attribute.manipulation
character.attribute.appearance

character.attribute.perception
character.attribute.intelligence
character.attribute.wits
```

### 6.3 Attribute Allocation

```text
Primary Classification:
    CharacterCreationRule
    MechanicalConstraint
    MechanicalOperation

Source-Derived Candidate:
    all Attributes begin at 1
    choose Primary, Secondary, and Tertiary categories
    distribute 7 / 5 / 3 additional points
    no Attribute exceeds 5 during creation
```

Target Artifacts:

```text
character-model/creation/attribute-priority.json
operations/character-creation/allocate-attributes.json
rules/character-creation/attributes/
fixtures/character-creation/attributes/
```

Slice Decision:

```text
IncludeAsCandidate
```

### 6.4 Attribute Specializations

```text
Primary Classification:
    MechanicalConstraint
    DiceRule
    CharacterFieldDefinition

Source-Derived Candidate:
    rating 4 or higher permits a specialization
```

The source also describes additional Dice behavior when a test falls within the specialization.

Target Artifacts:

```text
character-model/fields/specializations.json
rules/specializations/
dice/modifiers/specialization.json
```

Slice Decision:

```text
IncludeAsCandidate
```

This requires semantic review because the source summary describes an extra-die-on-10 behavior that must be checked for exact timing and interaction with results of 1.

## 7. Abilities Classification

### 7.1 Ability Categories

Candidate categories:

```text
ability-category.talents
ability-category.skills
ability-category.knowledges
```

Primary Classification:

```text
CharacterFieldDefinition
Terminology
```

### 7.2 Ability Fields

The exact Ability catalog should be generated from the dedicated source sections.

Examples appearing in the source include:

```text
Alertness
Athletics
Brawl
Empathy
Expression
Intimidation
Primal-Urge
Streetwise
Subterfuge

Animal-Ken
Crafts
Drive
Etiquette
Firearms
Leadership
Melee
Performance
Stealth
Survival

Computer
Enigmas
Investigation
Law
Linguistics
Medicine
Occult
Politics
Rituals
Science
```

The canonical English keys require terminology review.

### 7.3 Ability Allocation

```text
Primary Classification:
    CharacterCreationRule
    MechanicalConstraint
    MechanicalOperation

Source-Derived Candidate:
    all Abilities begin at 0
    choose Primary, Secondary, and Tertiary categories
    distribute 13 / 9 / 5 points
```

The creation-time maximum requires exact confirmation from the source's detailed Ability allocation section before approval.

Target Artifacts:

```text
character-model/creation/ability-priority.json
operations/character-creation/allocate-abilities.json
rules/character-creation/abilities/
fixtures/character-creation/abilities/
```

Slice Decision:

```text
IncludeAsCandidate
```

### 7.4 Lupus Creation Restrictions

```text
Primary Classification:
    MechanicalConstraint
    CharacterCreationRule

Condition:
    character.race == character.race.lupus

Source-Derived Restricted Initial Abilities:
    Crafts
    Drive
    Etiquette
    Firearms
    Computer
    Law
    Linguistics
    Politics
    Science
```

The source states that these may be acquired later with freebie points or experience through training.

This creates an important ambiguity:

```text
Does the restriction apply only to the 13 / 9 / 5 base allocation,
while freebie spending during creation is allowed?
```

Target:

```text
rules/character-creation/race/lupus-ability-restrictions.json
EXTRACTION-0004 ambiguity record
```

Slice Decision:

```text
NeedsClarification
```

## 8. Backgrounds Classification

### 8.1 Background Catalog

```text
Primary Classification:
    CharacterFieldDefinition
    CharacterCreationRule
    Terminology

Candidate Entries:
    Allies
    Ancestors
    Contacts
    Fetish
    Mentor
    Kinfolk
    Pure Breed
    Resources
    Rites
    Totem
```

Target:

```text
character-model/catalogs/backgrounds.json
character-model/fields/backgrounds.json
```

### 8.2 Background Allocation

```text
Primary Classification:
    CharacterCreationRule
    MechanicalConstraint

Source-Derived Candidate:
    distribute 5 points among Backgrounds
    Tribe restrictions apply
```

Target:

```text
operations/character-creation/allocate-backgrounds.json
rules/character-creation/backgrounds/
```

Slice Decision:

```text
IncludeAsCandidate
```

### 8.3 Tribal Background Restrictions

Examples in the source include:

```text
mandatory minimums
prohibited Backgrounds
no restrictions
```

These should be represented generically as:

```text
RequiredMinimum
ProhibitedSelection
Maximum
ConditionalPermission
```

Target:

```text
rules/character-creation/tribe/background-restrictions/
```

Slice Decision:

```text
IncludeAsCandidate
```

### 8.4 Totem Background

Totem is mechanically shared with the pack rather than purely individual.

Classification:

```text
CharacterCreationRule
GroupResourceRule
Deferred
```

Slice Decision:

```text
ReferenceOnly
```

The first slice may preserve the selected value and display it, but full group aggregation is deferred.

## 9. Gifts Classification

### 9.1 Initial Gift Selection

```text
Primary Classification:
    CharacterCreationRule
    MechanicalConstraint

Source-Derived Candidate:
    select three Level One Gifts:
        one Race Gift
        one Auspice Gift
        one Tribe Gift
```

Target:

```text
character-model/fields/gifts.json
character-model/catalogs/gifts-initial.json
operations/character-creation/select-initial-gifts.json
rules/character-creation/gifts/
```

Slice Decision:

```text
IncludeAsCandidate
```

### 9.2 Gift Execution

Full Gift behavior is outside Slice 001.

Classification:

```text
MechanicalOperation
MechanicalResolutionRule
Deferred
```

Only identity, category, level, and selection eligibility enter the first prototype.

## 10. Renown and Rank Classification

### 10.1 Initial Renown

```text
Primary Classification:
    CharacterCreationRule
    ResourceRule

Dimensions:
    Glory
    Honor
    Wisdom

Initialization:
    determined by Auspice
```

Target:

```text
character-model/resources/renown.json
rules/character-creation/initial-renown.json
```

Slice Decision:

```text
IncludeAsCandidate
```

### 10.2 Initial Rank

```text
Primary Classification:
    CharacterCreationRule

Source-Derived Candidate:
    Rank 1
    Cliath
```

Target:

```text
character-model/fields/rank.json
rules/character-creation/initial-rank.json
```

Slice Decision:

```text
IncludeAsCandidate
```

Rank advancement is deferred.

## 11. Core Resource Classification

### 11.1 Rage

```text
Primary Classification:
    ResourceRule
    CharacterFieldDefinition

Initialization:
    determined by Auspice

Target:
    character-model/resources/rage.json
```

### 11.2 Gnosis

```text
Primary Classification:
    ResourceRule
    CharacterFieldDefinition

Initialization:
    determined by Race

Target:
    character-model/resources/gnosis.json
```

### 11.3 Willpower

```text
Primary Classification:
    ResourceRule
    CharacterFieldDefinition

Initialization:
    determined by Tribe

Target:
    character-model/resources/willpower.json
```

### 11.4 Permanent and Temporary Values

The source uses these resources in ways that may distinguish permanent rating from temporary pool.

Classification:

```text
MechanicalDefinition
NeedsClarification
```

The first Character schema should be prepared to represent:

```text
permanent
current
maximum
```

but must not assume all three apply identically to every resource without review.

## 12. Freebie Point Classification

### 12.1 Budget

```text
Primary Classification:
    CharacterCreationRule
    ResourceRule

Source-Derived Candidate:
    15 freebie points
```

### 12.2 Cost Table

Candidate costs:

```text
Attribute:
    5 per point

Ability:
    2 per point

Background:
    1 per point

Level One Gift:
    7 per Gift

Rage:
    1 per point

Gnosis:
    2 per point

Willpower:
    1 per point
```

Target Artifacts:

```text
character-model/creation/freebie-budget.json
progression/creation-costs/freebie-costs.json
operations/character-creation/spend-freebies.json
rules/character-creation/freebies/
```

Slice Decision:

```text
IncludeAsCandidate
```

### 12.3 Freebie Timing

Freebie spending occurs after base allocation.

Classification:

```text
MechanicalOperation
CharacterCreationRule
```

The operation must preserve:

- purchase category;
- prior value;
- new value;
- unit count;
- unit cost;
- total cost;
- remaining budget;
- rule reference.

## 13. Character Sheet Classification

The sheet should be generated from the Character Model.

Recommended first layout:

```text
Identity
    Name
    Player
    Chronicle
    Concept
    Breed
    Auspice
    Tribe
    Pack
    Totem

Attributes
    Physical
    Social
    Mental

Abilities
    Talents
    Skills
    Knowledges

Advantages
    Backgrounds
    Gifts

Renown
    Glory
    Honor
    Wisdom
    Rank

Resources
    Rage
    Gnosis
    Willpower

Health
    display placeholder only in Slice 001

Notes
    Appearance
    Personality
    First Change
```

Classification:

```text
PresentationHint
```

Slice Decision:

```text
Include
```

No sheet rule may become the sole source of mechanical validation.

## 14. Generic Dice Test Classification

### 14.1 Pool Construction

Candidate operation:

```text
dice.test.attribute-plus-ability
```

Primary Classification:

```text
MechanicalOperation
DicePlan
```

Candidate input:

```text
CharacterId
AttributeKey
AbilityKey
Difficulty
SpecializationContext
ModifierSet
Purpose
```

Candidate Dice plan:

```text
one d10 group
quantity = effective Attribute + effective Ability + Dice modifiers
```

### 14.2 Success Resolution

The source contains rules for difficulty, successes, results of 1, failure, botch, and specialization behavior.

Primary Classification:

```text
MechanicalResolutionRule
```

Slice Decision:

```text
IncludeAsCandidate
```

Exact resolution must be extracted from the dedicated system section before implementation. The classification document does not freeze the algorithm yet.

### 14.3 Specialization

Candidate behavior from the source summary:

```text
rating 4 or higher permits a specialization
when the specialization applies, each result of 10 permits one additional die
results of 1 on added specialization Dice do not cancel existing successes
```

Classification:

```text
DiceResolutionRule
NeedsSemanticReview
```

### 14.4 Storyteller-Set Difficulty

Difficulty may be contextual and proposed by the Narrator or selected by the Rule Set operation.

Chronicle must validate it against the Rule Set contract.

Classification:

```text
MechanicalInput
NotRandomAuthority
```

## 15. Narrative Guidance Classification

The following are not deterministic creation restrictions by default:

- personality descriptions for each Auspice;
- expected social role;
- Tribe culture;
- typical appearance;
- nicknames;
- common territory;
- stereotypical attitudes;
- moon waxing or waning temperament;
- example motivations;
- sample First Change events.

Target:

```text
narrative-guidance/character/
knowledge/character-concepts/
```

Slice Decision:

```text
IncludeAsGuidance
```

## 16. Terminology Classification

Early terminology required by the slice:

```text
Garou
Race
Homid
Metis
Lupus
Auspice
Ragabash
Theurge
Philodox
Galliard
Ahroun
Tribe
Gift
Background
Rage
Gnosis
Willpower
Renown
Glory
Honor
Wisdom
Rank
Cliath
Pack
Totem
First Change
```

Target:

```text
terminology/concepts.json
terminology/synonyms.json
localization/pt-BR/
localization/en/
```

Slice Decision:

```text
Include
```

## 17. Example Classification

The source contains worked Character examples with full allocations and narrative detail.

Classification:

```text
Example
FixtureCandidate
NarrativeGuidance
```

Potential uses:

- creation fixture;
- freebie accounting fixture;
- sheet rendering fixture;
- provenance example.

They must not become universal rules.

## 18. Source Segments to Create

The next extraction pass should create at least these logical segments:

```text
source.werewolf3e.creation.overview
source.werewolf3e.creation.identity
source.werewolf3e.creation.attribute-allocation
source.werewolf3e.creation.ability-allocation
source.werewolf3e.creation.background-allocation
source.werewolf3e.creation.initial-gifts
source.werewolf3e.creation.initial-renown
source.werewolf3e.creation.initial-rank
source.werewolf3e.creation.initial-rage
source.werewolf3e.creation.initial-gnosis
source.werewolf3e.creation.initial-willpower
source.werewolf3e.creation.freebie-budget
source.werewolf3e.creation.freebie-costs
source.werewolf3e.race.homid
source.werewolf3e.race.metis
source.werewolf3e.race.lupus
source.werewolf3e.auspice.ragabash
source.werewolf3e.auspice.theurge
source.werewolf3e.auspice.philodox
source.werewolf3e.auspice.galliard
source.werewolf3e.auspice.ahroun
source.werewolf3e.dice.generic-test
source.werewolf3e.dice.specialization
```

Each Tribe and Background receives a separate source segment.

## 19. Candidate Package Artifact Map

```text
rule-sets/Chronicle.RuleSets.Werewolf/prototype/
├── manifest.json
├── character-model/
│   ├── model.json
│   ├── fields/
│   │   ├── identity.json
│   │   ├── race.json
│   │   ├── auspice.json
│   │   ├── tribe.json
│   │   ├── attributes.json
│   │   ├── abilities.json
│   │   ├── backgrounds.json
│   │   ├── gifts.json
│   │   ├── renown.json
│   │   ├── rank.json
│   │   └── resources.json
│   ├── catalogs/
│   │   ├── races.json
│   │   ├── auspices.json
│   │   ├── tribes.json
│   │   ├── attributes.json
│   │   ├── abilities.json
│   │   ├── backgrounds.json
│   │   └── initial-gifts.json
│   └── creation/
│       ├── profile.json
│       ├── attribute-allocation.json
│       ├── ability-allocation.json
│       ├── background-allocation.json
│       ├── initial-gifts.json
│       ├── resource-initialization.json
│       └── freebie-spending.json
├── character-sheet/
│   └── default-sheet.json
├── operations/
│   ├── character-creation/
│   └── dice/
├── dice/
│   ├── operations/
│   └── resolution/
├── rules/
│   └── character-creation/
├── terminology/
├── localization/
├── provenance/
└── fixtures/
```

## 20. Runtime Authority Matrix

```text
Character field schema:
    Rule Set authoritative

Character sheet layout:
    Presentation hint

Creation budget:
    Rule Set authoritative

Catalog display text:
    Localized presentation

Catalog mechanical metadata:
    Rule Set authoritative

Narrative role description:
    Guidance

Initial resource values:
    Rule Set authoritative

Tribal restriction:
    Rule Set authoritative after review

Gift prose:
    Reference or guidance in Slice 001

Gift eligibility:
    Rule Set authoritative

Worked example:
    Fixture candidate

Dice values:
    Chronicle authoritative

Dice resolution:
    Rule Set authoritative

Difficulty selection:
    operation input validated by Rule Set

Source text:
    evidence only
```

## 21. Required Reviews Before Implementation

### Extraction Review

Confirm that every candidate matches the cleaned source.

### Semantic Review

Confirm timing, limits, exceptions, and interactions.

### Terminology Review

Resolve stable keys and Portuguese/English labels.

### Implementation Review

Decide declarative versus compiled handlers.

### Fixture Review

Verify budgets and examples.

### Legal Review

Determine what may be distributed.

## 22. Known Ambiguity Candidates

The following must be moved into `EXTRACTION-0004`:

1. exact creation-time Ability maximum;
2. whether Lupus restricted Abilities may be purchased with freebie points during creation;
3. exact specialization resolution;
4. whether all Tribe records in the cleaned source are mechanically complete;
5. exact initial Renown values for each Auspice;
6. whether initial Gifts are fixed candidate lists or affected by another source rule;
7. permanent versus temporary representation of Rage, Gnosis, and Willpower;
8. whether Totem Background may be assigned individually before pack creation;
9. whether Rites is a Background, Knowledge, or both in different contexts;
10. exact freebie interaction with creation limits.

## 23. Deferred Classification

The following source content is intentionally not classified in detail yet:

```text
combat maneuvers
damage
soak
forms
Frenzy
silver
regeneration
Umbra
Rites execution
Gift execution
spirit Charms
Fetishes
Totems as active entities
Renown advancement
experience costs
antagonist statistics
```

They remain registered in the source inventory.

## 24. Slice Readiness

```text
Source coverage:
    sufficient

Classification coverage:
    sufficient for candidate extraction

Mechanical approval:
    not complete

Terminology approval:
    not complete

Legal approval:
    not complete

Prototype artifact generation:
    ready to begin

Official package publication:
    blocked
```

## 25. Next Document

Create:

```text
docs/extraction/werewolf-3e/
    EXTRACTION-0003-character-creation-slice.md
```

It will turn this classification into a concrete extraction plan and define:

- source segments;
- candidate rules;
- Character schema;
- creation workflow;
- package artifact files;
- fixtures;
- acceptance tests;
- unresolved blockers.

## 26. Final Classification Decision

The first Werewolf slice will not begin by copying a Character sheet.

It will begin by extracting:

```text
identity
catalogs
fields
budgets
constraints
initialization
operations
Dice contracts
guidance
terminology
provenance
```

The sheet will be generated from those artifacts.

That keeps Werewolf inside its package and keeps Chronicle independent.
