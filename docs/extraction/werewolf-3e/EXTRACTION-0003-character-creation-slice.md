---
id: EXTRACTION-0003
title: Werewolf 3e Character Creation Vertical Slice
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
publication_status: prototype-candidate
depends_on:
  - SPEC-0001
  - EXTRACTION-0001
  - EXTRACTION-0002
related_to:
  - EXTRACTION-0004
  - EXTRACTION-0005
---

> **"The first slice is complete only when a source rule becomes a field, an operation, a validation, a fixture, and a provenance record."**

# Werewolf 3e Character Creation Vertical Slice

## 1. Purpose

This document converts the classified Character creation material into a concrete extraction and prototype implementation plan.

It defines:

- the exact boundary of Slice 001;
- source segments to register;
- package artifacts to create;
- candidate fields and catalogs;
- the Character creation workflow;
- candidate mechanical operations;
- initialization and allocation rules;
- the first generic Dice operation;
- fixture and acceptance-test requirements;
- unresolved blockers;
- criteria for considering the vertical slice complete.

This document remains source-derived and candidate-only.

It does not certify complete Werewolf compatibility.

## 2. Slice Goal

Slice 001 must prove that Chronicle can:

1. discover a Rule Set package;
2. read a package manifest;
3. load a declarative Character Model;
4. render a Character sheet without a Werewolf-specific screen;
5. guide a user through package-defined Character creation;
6. validate package-defined allocation budgets;
7. initialize package-defined resources;
8. enforce package-defined restrictions;
9. create a generic Attribute + Ability Dice plan;
10. let Chronicle generate and commit Dice evidence;
11. let the package resolve that evidence;
12. trace every approved candidate to source provenance;
13. do all of this without Werewolf references in Chronicle Core.

## 3. Slice Scope

### 3.1 Included

```text
Character identity
Race
Auspice
Tribe
Attributes
Abilities
Backgrounds
initial Gifts
initial Renown
initial Rank
initial Rage
initial Gnosis
initial Willpower
freebie points
specializations
one generic Attribute + Ability test
default Character sheet layout
provenance
fixtures
validation reports
```

### 3.2 Explicitly Deferred

```text
full Gift execution
combat
forms and transformation
damage
soak
Health resolution
regeneration
Frenzy
Rites
Umbra
spirits
Totem group aggregation
experience progression
Renown advancement
antagonists
pack creation
```

## 4. Source-Derived Creation Sequence

The source presents the following Character creation sequence:

```text
Step 1:
    Concept, Race, Auspice, and Tribe

Step 2:
    Prioritize and allocate Attributes
    7 / 5 / 3

Step 3:
    Prioritize and allocate Abilities
    13 / 9 / 5

Step 4:
    Advantages
    Backgrounds: 5
    Gifts: 3
    initial Renown
    initial Rank

Step 5:
    Final details
    initial Rage
    initial Gnosis
    initial Willpower
    freebie points: 15
```

The prototype workflow preserves this order unless a later semantic review finds a required dependency change.

## 5. Source Segment Registry

The first extraction batch should register these source segments.

### 5.1 Creation Overview

```yaml
sourceSegmentId: source.werewolf3e.creation.overview
classification: CharacterCreationRule
headingPath:
  - SISTEMA DE CRIAÇÃO DE PERSONAGEM
  - VISÃO GERAL DAS ETAPAS
expectedArtifacts:
  - character-model/creation/profile.json
```

### 5.2 Race Selection

```yaml
sourceSegmentId: source.werewolf3e.creation.race-selection
classification: CharacterCreationRule
headingPath:
  - SISTEMA DE CRIAÇÃO DE PERSONAGEM GAROU
  - RAÇAS GAROU
expectedArtifacts:
  - character-model/fields/race.json
  - character-model/catalogs/races.json
  - operations/character-creation/select-race.json
```

### 5.3 Auspice Selection

```yaml
sourceSegmentId: source.werewolf3e.creation.auspice-selection
classification: CharacterCreationRule
headingPath:
  - AUGÚRIOS
expectedArtifacts:
  - character-model/fields/auspice.json
  - character-model/catalogs/auspices.json
  - operations/character-creation/select-auspice.json
```

### 5.4 Tribe Selection

```yaml
sourceSegmentId: source.werewolf3e.creation.tribe-selection
classification: CharacterCreationRule
headingPath:
  - TRIBOS GAROU
expectedArtifacts:
  - character-model/fields/tribe.json
  - character-model/catalogs/tribes.json
  - operations/character-creation/select-tribe.json
```

### 5.5 Attribute Allocation

```yaml
sourceSegmentId: source.werewolf3e.creation.attribute-allocation
classification: CharacterCreationRule
headingPath:
  - SISTEMA DE CRIAÇÃO DE PERSONAGEM
  - PASSO DOIS
expectedArtifacts:
  - character-model/catalogs/attributes.json
  - character-model/creation/attribute-allocation.json
  - operations/character-creation/allocate-attributes.json
```

### 5.6 Ability Allocation

```yaml
sourceSegmentId: source.werewolf3e.creation.ability-allocation
classification: CharacterCreationRule
headingPath:
  - SISTEMA DE CRIAÇÃO DE PERSONAGEM
  - PASSO TRÊS
expectedArtifacts:
  - character-model/catalogs/abilities.json
  - character-model/creation/ability-allocation.json
  - operations/character-creation/allocate-abilities.json
```

### 5.7 Advantages

```yaml
sourceSegmentId: source.werewolf3e.creation.advantages
classification: CharacterCreationRule
headingPath:
  - SISTEMA DE CRIAÇÃO DE PERSONAGEM
  - PASSO QUATRO
expectedArtifacts:
  - character-model/creation/background-allocation.json
  - character-model/creation/initial-gifts.json
  - character-model/creation/initial-renown.json
  - character-model/creation/initial-rank.json
```

### 5.8 Final Details and Freebies

```yaml
sourceSegmentId: source.werewolf3e.creation.final-details
classification: CharacterCreationRule
headingPath:
  - SISTEMA DE CRIAÇÃO DE PERSONAGEM
  - PASSO CINCO
expectedArtifacts:
  - character-model/creation/resource-initialization.json
  - character-model/creation/freebie-spending.json
  - progression/creation-costs/freebie-costs.json
```

### 5.9 Generic Dice Test

```yaml
sourceSegmentId: source.werewolf3e.dice.generic-test
classification: MechanicalResolutionRule
headingPath:
  - dedicated system resolution section
expectedArtifacts:
  - dice/operations/attribute-plus-ability-test.json
  - dice/resolution/basic-success-test.json
```

The exact heading path must be captured during the detailed extraction pass.

## 6. Prototype Repository Layout

Create:

```text
rule-sets/
└── Chronicle.RuleSets.Werewolf/
    └── prototype/
        ├── manifest.json
        ├── character-model/
        │   ├── model.json
        │   ├── fields/
        │   ├── catalogs/
        │   ├── resources/
        │   ├── creation/
        │   └── validation/
        ├── character-sheet/
        │   └── default-sheet.json
        ├── operations/
        │   ├── character-creation/
        │   └── dice/
        ├── dice/
        │   ├── plans/
        │   └── resolution/
        ├── progression/
        │   └── creation-costs/
        ├── rules/
        │   └── character-creation/
        ├── terminology/
        ├── localization/
        │   ├── en/
        │   └── pt-BR/
        ├── provenance/
        ├── fixtures/
        │   ├── valid/
        │   └── invalid/
        └── validation/
```

## 7. Manifest Candidate

Target:

```text
rule-sets/Chronicle.RuleSets.Werewolf/prototype/manifest.json
```

Candidate semantic content:

```yaml
packageId: chronicle.rulesets.werewolf
packageVersion: 0.1.0-prototype
publisherId: chronicle
ruleSetContractVersion: 1
displayNameResourceKey: ruleset.werewolf.display-name
descriptionResourceKey: ruleset.werewolf.description
supportedLocales:
  - en
  - pt-BR
capabilities:
  - character-model
  - character-creation
  - character-sheet
  - generic-dice-test
publicationStatus: prototype-candidate
```

The final package ID and publisher namespace remain governance decisions.

## 8. Character Model Root

Target:

```text
character-model/model.json
```

Candidate responsibilities:

```text
model identity
contract version
field registry
catalog registry
resource registry
creation profile reference
default sheet reference
validation registry
```

The model must not contain a Werewolf-specific CLR type reference.

## 9. Candidate Character Fields

### 9.1 Identity Fields

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

Fields not clearly supported by the source should remain optional prototype candidates.

### 9.2 Classification Fields

```text
character.classification.race
character.classification.auspice
character.classification.tribe
```

Value type:

```text
SingleChoice
```

Each references a package-owned catalog.

### 9.3 Attributes

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

Candidate type:

```text
RankTrack
```

Candidate range:

```text
minimum: 1
maximum: 5
creationBase: 1
```

### 9.4 Abilities

The exact catalog will be extracted from dedicated sections.

Candidate type:

```text
RankTrack
```

Candidate range:

```text
minimum: 0
maximum: 5
creationBase: 0
```

Base allocation limit:

```text
maximumDuringBaseAllocation: 3
```

Levels 4 and 5 may be reached through freebie spending according to the source summary.

### 9.5 Backgrounds

Candidate representation:

```text
StructuredCollection
```

Each item:

```text
backgroundKey
rating
source
restrictionsApplied
```

Candidate range:

```text
0 to 5
```

Exact per-Background limits require semantic review.

### 9.6 Gifts

Candidate representation:

```text
ReferenceCollection
```

Each initial Gift stores:

```text
giftKey
level
sourceCategory:
    Race
    Auspice
    Tribe
selectionProvenance
```

### 9.7 Renown

Candidate fields:

```text
character.renown.glory.permanent
character.renown.honor.permanent
character.renown.wisdom.permanent
```

Temporary Renown is deferred unless required by the source's initial model.

### 9.8 Rank

```text
character.rank
```

Candidate initialization:

```text
rank.cliath
```

Candidate numeric level:

```text
1
```

### 9.9 Resources

```text
character.resource.rage.permanent
character.resource.rage.current

character.resource.gnosis.permanent
character.resource.gnosis.current

character.resource.willpower.permanent
character.resource.willpower.current
```

Whether `current` begins equal to `permanent` must be confirmed semantically.

## 10. Catalog Candidates

### 10.1 Race Catalog

```text
character.race.homid
character.race.metis
character.race.lupus
```

Candidate initialization:

```text
Homid:
    Gnosis 1

Metis:
    Gnosis 3

Lupus:
    Gnosis 5
```

Additional candidates:

```text
Metis:
    mandatory deformity

Lupus:
    base-allocation Ability restrictions
```

### 10.2 Auspice Catalog

```text
character.auspice.ragabash
character.auspice.theurge
character.auspice.philodox
character.auspice.galliard
character.auspice.ahroun
```

Candidate initial Rage:

```text
Ragabash: 1
Theurge: 2
Philodox: 3
Galliard: 4
Ahroun: 5
```

Candidate initial Renown:

```text
Ragabash:
    3 points in a source-defined free combination

Theurge:
    Wisdom 3

Philodox:
    Honor 3

Galliard:
    Glory 2
    Wisdom 1

Ahroun:
    Glory 2
    Honor 1
```

These values remain candidates until the detailed source segment is captured and reviewed.

### 10.3 Tribe Catalog

Each enabled Tribe entry requires:

```text
tribe key
initial Willpower
initial Gift options
Background restrictions
display resources
guidance references
provenance
```

No Tribe should be enabled if any required mechanical field is missing.

## 11. Character Creation Profile

Target:

```text
character-model/creation/profile.json
```

Candidate ordered steps:

```text
10 enter-identity
20 select-race
30 select-auspice
40 select-tribe
50 prioritize-attributes
60 allocate-attributes
70 prioritize-abilities
80 allocate-abilities
90 allocate-backgrounds
100 select-initial-gifts
110 initialize-renown
120 initialize-rank
130 initialize-resources
140 spend-freebie-points
150 review-character
160 complete-character
```

## 12. Step Preconditions

### Select Race

```text
requires:
    identity initialized enough to start creation
```

### Select Auspice

```text
requires:
    none beyond active creation profile
```

### Select Tribe

```text
requires:
    race selected
    auspice selected
```

The source order supports selection in one conceptual step, but operations may remain individually revisable until dependent allocations are committed.

### Allocate Backgrounds

```text
requires:
    Tribe selected
```

### Select Initial Gifts

```text
requires:
    Race selected
    Auspice selected
    Tribe selected
```

### Initialize Resources

```text
requires:
    Race
    Auspice
    Tribe
```

### Spend Freebies

```text
requires:
    all base allocations complete
```

## 13. Attribute Allocation Operation

Target:

```text
operations/character-creation/allocate-attributes.json
```

Candidate input:

```text
CharacterId
PrimaryCategoryKey
SecondaryCategoryKey
TertiaryCategoryKey
AllocationsByField
```

Candidate validation:

```text
all three categories are distinct
all Attributes start at 1
Primary additional points total 7
Secondary additional points total 5
Tertiary additional points total 3
no Attribute exceeds 5
no negative allocation
all fields belong to their declared categories
```

Candidate result:

```text
ValidatedAttributeAllocation
```

## 14. Ability Allocation Operation

Target:

```text
operations/character-creation/allocate-abilities.json
```

Candidate input:

```text
CharacterId
PrimaryCategoryKey
SecondaryCategoryKey
TertiaryCategoryKey
AllocationsByField
```

Candidate validation:

```text
all categories distinct
all Abilities start at 0
Primary total 13
Secondary total 9
Tertiary total 5
no Ability exceeds 3 during base allocation
Race-specific restrictions apply
```

Lupus candidate restriction during base allocation:

```text
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

The package must use canonical keys, not localized labels.

## 15. Background Allocation Operation

Target:

```text
operations/character-creation/allocate-backgrounds.json
```

Candidate validation:

```text
total allocated points equals 5
ratings are nonnegative
ratings respect general limits
Tribe-specific restrictions apply
required minimums are satisfied
prohibited Backgrounds remain zero
```

Totem is stored as a Character candidate value in Slice 001, but pack-level aggregation remains deferred.

## 16. Initial Gift Selection Operation

Target:

```text
operations/character-creation/select-initial-gifts.json
```

Candidate validation:

```text
exactly 3 Gifts selected
each Gift is Level 1
exactly 1 Gift comes from Race eligibility
exactly 1 Gift comes from Auspice eligibility
exactly 1 Gift comes from Tribe eligibility
no duplicate Gift unless explicitly permitted
```

Full Gift execution is outside this slice.

## 17. Resource Initialization Operation

Target:

```text
operations/character-creation/initialize-resources.json
```

Candidate lookup sources:

```text
Race:
    initial Gnosis

Auspice:
    initial Rage

Tribe:
    initial Willpower
```

Candidate result:

```text
InitializedResourceSet
```

No resource value is hardcoded in Chronicle Core.

## 18. Initial Renown Operation

Target:

```text
operations/character-creation/initialize-renown.json
```

Candidate behavior:

```text
look up Auspice initial Renown profile
apply exact Glory, Honor, and Wisdom values
allow Ragabash free combination only within the source-defined total and constraints
```

Ragabash allocation requires a dedicated input and validation branch.

## 19. Initial Rank Operation

Target:

```text
operations/character-creation/initialize-rank.json
```

Candidate result:

```text
rankKey: rank.cliath
rankLevel: 1
```

## 20. Freebie Budget

Target:

```text
character-model/creation/freebie-spending.json
```

Candidate budget:

```text
15 points
```

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

These values must be represented as package-owned creation costs.

## 21. Freebie Spending Operation

Target:

```text
operations/character-creation/spend-freebies.json
```

Candidate input item:

```text
purchaseTypeKey
targetKey
quantity
```

Candidate result item:

```text
priorValue
newValue
unitCost
totalCost
ruleRef
```

Candidate validation:

```text
total cost does not exceed remaining budget
target is permitted
creation limits are respected
Race restrictions are applied according to the resolved interpretation
Gift is Level 1 where required
```

## 22. Specializations

Candidate Character representation:

```text
character.specializations[]
```

Each specialization:

```text
parentFieldKey
specializationKey or user-authored label
status
```

Candidate eligibility:

```text
Attribute or Ability rating >= 4
```

The exact Dice effect must remain behind a candidate rule until semantic review.

## 23. Default Character Sheet

Target:

```text
character-sheet/default-sheet.json
```

Candidate sections:

```text
Identity
Classification
Attributes
Abilities
Advantages
Renown and Rank
Resources
Health Placeholder
Narrative Details
Creation Review
```

The layout may contain Werewolf-specific field references because it belongs to the Werewolf package.

Chronicle Presentation must only understand generic layout elements.

## 24. Generic Dice Operation

Target:

```text
dice/operations/attribute-plus-ability-test.json
```

Candidate operation key:

```text
dice.test.attribute-plus-ability
```

Candidate input:

```text
CharacterId
AttributeFieldKey
AbilityFieldKey
Difficulty
PurposeKey
ModifierSet
SpecializationContext
```

Candidate plan:

```text
DiceKind:
    d10

GroupCount:
    1

Quantity:
    effective Attribute + effective Ability + Dice modifiers

ResolutionContract:
    werewolf.basic-success-test.v1
```

Chronicle owns:

```text
player release
random generation
raw evidence persistence
evidence identity
recovery
```

The Rule Set package owns:

```text
pool construction
difficulty validation
success interpretation
failure interpretation
botch interpretation
specialization interpretation
```

## 25. Dice Resolution Extraction Requirement

Before implementing the resolver, extract exact source segments for:

```text
success threshold
difficulty range
result of 1
result of 10
zero successes
botch
multiple successes
specialization
automatic success or failure rules
minimum Dice pool behavior
modifiers
```

No algorithm may be inferred from general World of Darkness knowledge.

## 26. Provenance Layout

Create:

```text
provenance/source-document.json
provenance/source-segments/
provenance/artifact-links/
provenance/review-status.json
```

Every candidate artifact must include:

```text
sourceSegmentRefs
transformation type
normalization notes
extraction confidence
semantic review status
implementation review status
legal review status
```

## 27. Localization Layout

Create at least:

```text
localization/pt-BR/strings.json
localization/en/strings.json
```

The Portuguese file may initially preserve source terminology.

The English file may contain reviewed canonical technical labels or temporary placeholders marked as unapproved.

Technical keys remain English and invariant.

## 28. Fixture Set

### 28.1 Valid Minimal Character

Must prove:

```text
valid Race
valid Auspice
valid Tribe
7 / 5 / 3 Attributes
13 / 9 / 5 Abilities
5 Background points
3 eligible initial Gifts
initial resources
initial Renown
Rank 1
15 or fewer freebie points spent
```

### 28.2 Valid Lupus Character

Must prove:

```text
restricted Abilities remain zero during base allocation
Gnosis initializes to 5
valid Race Gift
freebie interpretation follows the resolved rule
```

### 28.3 Valid Metis Character

Must prove:

```text
Gnosis initializes to 3
mandatory deformity selected
valid Race Gift
```

### 28.4 Invalid Attribute Budget

Examples:

```text
8 / 5 / 3
duplicate priority category
Attribute above 5
negative allocation
```

### 28.5 Invalid Ability Budget

Examples:

```text
13 / 10 / 5
Ability above 3 during base allocation
Lupus restricted Ability assigned during base allocation
```

### 28.6 Invalid Background Allocation

Examples:

```text
more than 5 points
prohibited Tribal Background
missing required Tribal minimum
```

### 28.7 Invalid Gift Selection

Examples:

```text
fewer than 3 Gifts
wrong source category count
Gift above Level 1
Gift not eligible
```

### 28.8 Invalid Freebie Spending

Examples:

```text
cost above 15
wrong unit cost
prohibited target
unresolved Race restriction
```

### 28.9 Generic Dice Fixture

Candidate fixture:

```text
Attribute 3
Ability 2
Difficulty from explicit test input
Pool quantity 5
known committed d10 evidence
expected resolved success state
```

Expected resolution remains pending exact Dice extraction.

## 29. Acceptance Tests

### Package Neutrality

```text
Chronicle Core contains no Werewolf namespace reference.
```

### Declarative Sheet

```text
Removing the Werewolf package removes the Werewolf sheet without changing Chronicle Presentation code.
```

### Allocation Validation

```text
The package validates 7 / 5 / 3 and 13 / 9 / 5.
```

### Initialization

```text
Race, Auspice, and Tribe initialize their respective resources through package data.
```

### Restriction

```text
Lupus creation restrictions are enforced by package rules.
```

### Freebie Accounting

```text
Every purchase produces deterministic cost evidence.
```

### Dice Boundary

```text
The package builds a d10 plan.
Chronicle commits raw values.
The package resolves the same values.
Retry does not reroll.
```

### Provenance

```text
Every deterministic candidate has at least one source reference.
```

### Cross-System Protection

```text
A synthetic package can define different fields and Dice without Race, Auspice, or Tribe.
```

## 30. Completion States

Artifacts move through:

```text
ExtractedCandidate
StructurallyValid
SemanticallyReviewed
ImplementationMapped
FixtureCovered
PrototypeReady
ApprovedForPackage
```

Slice 001 is complete at:

```text
PrototypeReady
```

It is not automatically:

```text
ApprovedForPackage
```

## 31. Blockers for PrototypeReady

The following block prototype completion:

1. missing exact Tribe mechanical table;
2. missing exact Ability catalog;
3. unresolved creation-time Ability limit;
4. unresolved Lupus freebie exception;
5. missing exact initial Renown mapping;
6. missing exact Tribe Willpower values;
7. missing exact Tribe Background restrictions;
8. missing exact initial Gift catalogs;
9. missing exact generic Dice algorithm;
10. missing artifact schemas;
11. missing fixtures;
12. unresolved package ID governance only if required by the loader prototype.

## 32. Items That Do Not Block the Prototype

The following may remain deferred:

```text
complete narrative Tribe descriptions
complete Gift power text
complete combat
complete forms
complete experience progression
full legal redistribution approval
production signature
production package installer
```

They block official release, not the architectural prototype.

## 33. Work Breakdown

### Batch A — Source Extraction

```text
A1 exact Character creation source segments
A2 Race table
A3 Auspice table
A4 Tribe mechanical summaries
A5 Attribute catalog
A6 Ability catalog
A7 Background catalog
A8 Gift eligibility catalogs
A9 freebie table
A10 generic Dice section
```

### Batch B — Declarative Artifacts

```text
B1 manifest
B2 Character fields
B3 catalogs
B4 resources
B5 creation profile
B6 default sheet
B7 localization
B8 provenance
```

### Batch C — Operations

```text
C1 select classification values
C2 allocate Attributes
C3 allocate Abilities
C4 allocate Backgrounds
C5 select Gifts
C6 initialize Renown
C7 initialize Rank
C8 initialize resources
C9 spend freebies
C10 build generic Dice plan
C11 resolve generic Dice
```

### Batch D — Validation

```text
D1 schemas
D2 reference validation
D3 fixtures
D4 candidate semantic review
D5 contract findings
D6 ambiguity report
```

## 34. Next Files

After this document, create:

```text
docs/extraction/werewolf-3e/
    EXTRACTION-0004-ambiguities-and-conflicts.md
```

Then:

```text
docs/extraction/werewolf-3e/
    EXTRACTION-0005-contract-findings.md
```

After the extraction reports, begin the concrete prototype artifacts under:

```text
rule-sets/Chronicle.RuleSets.Werewolf/prototype/
```

## 35. Final Slice Decision

The first Werewolf implementation will not start with a custom Character screen.

It will start with:

```text
source segments
    ↓
catalogs and fields
    ↓
creation operations
    ↓
validation rules
    ↓
declarative sheet
    ↓
Dice plan
    ↓
fixtures and provenance
```

Werewolf will prove the Rule Set package architecture.

It will not define Chronicle's identity.
