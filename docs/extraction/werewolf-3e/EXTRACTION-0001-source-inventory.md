---
id: EXTRACTION-0001
title: Werewolf 3e Cleaned Source Inventory
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-08-02
category: Rule Set Extraction
source_document:
  file_name: Werewolf the Apocalypse 3e-pt_br(1).txt
  logical_title: Werewolf the Apocalypse 3e - Cleaned Portuguese Working Source
  language: pt-BR
  source_type: cleaned-working-summary
  edition_claim: 3e
  exact_original_edition_verification: pending
  byte_size: 380686
  character_count: 370394
  line_count: 3948
  heading_count: 705
  sha256: a4302e2938a137fb42d154c45decd43e02a19a2ba0eb3030b6eb1de942dab64a
normalization_policy: FidelityWithNormalization
distribution_status: private-source-reference
depends_on:
  - SPEC-0001
related_to:
  - EXTRACTION-0002
  - EXTRACTION-0003
  - EXTRACTION-0004
  - EXTRACTION-0005
---

> **"This inventory describes the source we have. It does not certify every statement in that source as an approved rule."**

# Werewolf 3e Cleaned Source Inventory

## 1. Purpose

This document registers and evaluates the cleaned Werewolf source provided for the first Chronicle Rule Set extraction cycle.

It records:

- source identity;
- physical characteristics;
- structural organization;
- apparent content coverage;
- extraction suitability;
- known limitations;
- review risks;
- recommended extraction order;
- publication restrictions;
- next actions.

This inventory is descriptive.

It does not approve mechanical rules, legal redistribution, translations, terminology, or editorial interpretations.

## 2. Registered Source

```text
File:
    Werewolf the Apocalypse 3e-pt_br(1).txt

Working Source ID:
    source.werewolf3e.cleaned-ptbr

Claimed System:
    Werewolf: The Apocalypse

Claimed Edition:
    Third Edition

Language:
    Brazilian Portuguese

Format:
    UTF-8 plain text with Markdown-like headings, lists, tables, and emphasis

Source Role:
    private extraction input

Package Role:
    none directly
```

## 3. File Fingerprint

```text
Size:
    380,686 bytes

Characters:
    370,394

Lines:
    3,948

Markdown-style headings:
    705

SHA-256:
    a4302e2938a137fb42d154c45decd43e02a19a2ba0eb3030b6eb1de942dab64a
```

The hash identifies the exact working file used by this extraction batch.

Any modified source must receive a new source-document version and fingerprint.

## 4. Source Characterization

The file appears to be a manually cleaned, reorganized, and condensed working source rather than a raw transcription of a published book.

It contains a mixture of:

- setting summaries;
- terminology;
- social and cosmological descriptions;
- character creation procedures;
- structured tables;
- mechanical definitions;
- restrictions;
- combat and dramatic systems;
- health and damage rules;
- spiritual travel;
- storyteller guidance;
- antagonists;
- Gifts, Rites, Totems, Fetishes, and related content;
- examples and editorial summaries.

The document is substantially more extraction-friendly than an unprocessed sourcebook because many concepts are already represented as headings, lists, tables, formulas, and explicit rule summaries.

## 5. Structural Quality

### 5.1 Positive Characteristics

The source contains:

- extensive heading hierarchy;
- many explicit tables;
- named mechanical sections;
- numbered creation steps;
- summarized resource values;
- explicit restrictions;
- formula-like descriptions;
- repeated terminology;
- clear divisions between many major subjects;
- compact rule statements suitable for segmentation.

### 5.2 Structural Weaknesses

The source also contains:

- repeated topics under different headings;
- duplicated or partially overlapping summaries;
- inconsistent heading depth;
- mixed Portuguese and English terminology;
- occasional apparent typographical artifacts;
- editorial statements mixed with rule statements;
- unclear preservation of original page references;
- no stable source-segment IDs;
- no explicit distinction between original wording and later normalization;
- no embedded legal or redistribution metadata.

## 6. Major Content Areas

The source includes substantial material in the following areas.

### 6.1 Setting and Core Concepts

Observed areas include:

- myths versus truths;
- Gaia, Wyrm, Wyld, and Weaver;
- Apocalypse;
- Umbra;
- Delirium and the Veil;
- Garou origins and mythology;
- Impergium;
- War of Rage;
- Caerns;
- Litany;
- Garou society;
- packs, septs, tribes, and moots;
- political geography;
- antagonistic forces.

Primary extraction destination:

```text
narrative-guidance/
knowledge/
terminology/
rules/ when an explicit mechanical effect exists
```

### 6.2 Character Identity

Observed areas include:

- Breed;
- Auspice;
- Tribe;
- forms;
- Rank;
- Renown;
- pack and totem relationships;
- social roles;
- initial Gifts;
- initial Rage and Gnosis associations.

Primary extraction destination:

```text
character-model/catalogs/
character-model/creation/
terminology/
rules/
localization/
```

### 6.3 Character Creation

Observed areas include:

- character concept;
- identification fields;
- Race, Auspice, and Tribe selection;
- Attributes;
- Abilities;
- Backgrounds;
- Gifts;
- Rage;
- Gnosis;
- Willpower;
- freebie points;
- Prelude and First Change guidance;
- creation summaries and examples.

Primary extraction destination:

```text
character-model/
character-sheet/
operations/character-creation/
rules/character-creation/
fixtures/character-creation/
```

This is the strongest candidate for the first vertical slice.

### 6.4 Attributes and Abilities

Observed areas include:

- Attribute definitions;
- ratings from one to five;
- suggested specializations;
- Talents;
- Skills;
- Knowledges;
- restrictions by character origin;
- level-based descriptions;
- specialization behavior.

Primary extraction destination:

```text
character-model/fields/
character-model/catalogs/
rules/
operations/
localization/
```

### 6.5 Backgrounds

Observed areas include:

- Background definitions;
- level tables;
- social and material assets;
- Totem;
- restrictions on advancement;
- narrative acquisition behavior;
- package-specific exceptions.

Primary extraction destination:

```text
character-model/catalogs/backgrounds/
progression/
rules/
operations/
narrative-guidance/
```

This content is especially useful for validating the distinction between numerical progression and narrative change.

### 6.6 Resources and Tracks

Observed areas include:

- Rage;
- Gnosis;
- Willpower;
- Health;
- Renown;
- Rank;
- damage conditions;
- regeneration;
- temporary and permanent values.

Primary extraction destination:

```text
character-model/resources/
progression/
operations/
rules/
character-sheet/
```

### 6.7 Dice and General Tests

The source appears to contain:

- general action resolution;
- pools;
- difficulty;
- successes;
- failure and botch behavior;
- specialties;
- extended or resisted situations;
- dramatic systems;
- task categories.

Primary extraction destination:

```text
dice/operations/
dice/resolution/
rules/dice/
fixtures/dice/
```

This area requires exact segmentation and semantic review before implementation.

### 6.8 Combat

Observed areas include:

- turns and actions;
- initiative or action organization;
- close combat;
- ranged combat;
- maneuvers;
- pack tactics;
- damage;
- soak;
- health states;
- environmental damage;
- silver;
- falling;
- fire;
- poison;
- asphyxiation;
- regeneration;
- battle scars.

Primary extraction destination:

```text
operations/combat/
dice/
rules/combat/
character-model/resources/health/
fixtures/combat/
```

Combat is broad and should not be part of the first extraction slice beyond what is necessary to prove the generic Dice contract.

### 6.9 Frenzy and Mental States

Observed areas include:

- Frenzy;
- mental conditions;
- influence of the Wyrm;
- the Curse;
- Delirium;
- dramatization guidance.

Primary extraction destination:

```text
rules/conditions/
operations/
narrative-guidance/
character-model/state/
```

The material likely combines deterministic triggers, tests, outcomes, and portrayal guidance. These must be separated.

### 6.10 Forms and Transformation

Observed areas include:

- Homid;
- Glabro;
- Crinos;
- Hispo;
- Lupus;
- natural form;
- transformation;
- social and physical consequences.

Primary extraction destination:

```text
character-model/catalogs/forms/
operations/form-change/
rules/forms/
character-sheet/
```

### 6.11 Gifts, Rites, Totems, Fetishes, and Spirits

Observed areas include:

- initial Gifts;
- Gift descriptions;
- Rites;
- ritual knowledge;
- Totems;
- spirit hierarchy;
- Charms;
- Fetishes;
- Talens or single-use items;
- Umbra and spiritual travel.

Primary extraction destination:

```text
character-model/catalogs/
operations/
rules/
knowledge/
narrative-guidance/
terminology/
```

These areas are likely to reveal the need for:

- costs;
- dynamic difficulty;
- duration;
- target selection;
- prerequisites;
- multi-stage resolution;
- resource spending;
- narrative effects;
- package-owned structured payloads.

### 6.12 Progression

Observed areas include:

- experience spending;
- Rank;
- Renown gain and loss;
- advancement restrictions;
- Background exceptions;
- optional rules;
- narrative-only changes.

Primary extraction destination:

```text
progression/
operations/progression/
rules/progression/
fixtures/progression/
```

### 6.13 Storytelling Guidance

Observed areas include:

- storytelling chapters;
- dramatic techniques;
- Chronicle management;
- pacing and narrative advice;
- Prelude construction;
- atmosphere and theme.

Primary extraction destination:

```text
narrative-guidance/
knowledge/
```

This material must not become deterministic mechanics unless an explicit rule is separately identified.

### 6.14 Antagonists

Observed areas include:

- Fomori;
- spirits;
- human agencies;
- other supernatural beings;
- Wyrm servants;
- Black Spiral Dancers;
- corporate threats.

Primary extraction destination:

```text
character-model/archetypes/
rules/antagonists/
knowledge/
narrative-guidance/
```

Antagonists should be processed only after the player-character foundation is stable.

### 6.15 Glossary and Terminology

The source contains a substantial Garou glossary, including:

- common terms;
- archaic terms;
- slang;
- social ranks;
- cosmological concepts;
- Umbra terminology;
- forms;
- factions;
- spiritual entities.

Primary extraction destination:

```text
terminology/
localization/
knowledge/
```

This is a high-value early extraction area because stable technical keys and aliases will be reused by all later artifacts.

## 7. Preliminary Extraction Suitability

```text
Character creation:
    High

Character fields and catalogs:
    High

Character-sheet layout:
    High

Terminology:
    High

Narrative guidance:
    High

Generic Dice foundation:
    Medium to High

Combat:
    Medium to High

Progression:
    Medium

Gifts and Rites:
    Medium

Spirits and Umbra:
    Medium

Antagonists:
    Medium

Direct automatic package publication:
    Not suitable

Unreviewed deterministic authority:
    Not suitable
```

## 8. Why the Source Is Suitable for Starting

The source is adequate for beginning because it provides a real vertical path from source material to executable package concepts:

```text
Race, Auspice, and Tribe
    ↓
initial resource values
    ↓
Character fields and catalogs
    ↓
creation steps
    ↓
allocation rules
    ↓
sheet layout
    ↓
generic test construction
    ↓
Dice resolution candidate
    ↓
fixtures
```

It also contains enough complexity to expose weaknesses in the generic contract without requiring the entire Rule Set to be extracted first.

## 9. Why the Source Is Not Yet Package Authority

The source must not be copied directly into an installed package as authoritative content because:

- it is a cleaned and editorially transformed source;
- original wording versus summary wording is not consistently identified;
- original page provenance is incomplete;
- repeated rules may disagree;
- examples may appear as general rules;
- narrative interpretation may appear beside mechanics;
- terminology may vary;
- legal redistribution status is not recorded;
- no semantic review has been completed;
- no deterministic tests have been generated.

## 10. Primary Risk Categories

### 10.1 Edition Risk

The filename and content claim Third Edition, but exact source-edition verification remains pending.

No external edition correction should be applied silently.

### 10.2 Editorial Transformation Risk

The text appears substantially reorganized and summarized.

Some mechanical precision may have been lost or editorially inferred.

### 10.3 Duplicate Rule Risk

The document revisits major concepts in summary and detailed sections.

A later detailed section may conflict with an earlier summary.

### 10.4 Translation Risk

Terms may have:

- multiple Portuguese translations;
- untranslated English names;
- spelling variation;
- culturally or mechanically significant wording differences.

### 10.5 Context Loss Risk

A condensed rule may omit:

- exceptions;
- timing;
- prerequisites;
- scope;
- optional status;
- storyteller discretion;
- interaction with another chapter.

### 10.6 Example-as-Rule Risk

Worked examples and descriptive prose may appear mechanically prescriptive.

### 10.7 Narrative-as-Mechanic Risk

Role descriptions, stereotypes, mood, and social expectations may be mistaken for mandatory Character restrictions.

### 10.8 Rule-as-Guidance Risk

The stated “Golden Rule” and storyteller discretion may qualify otherwise mechanical statements.

These qualifications need explicit representation rather than silent removal.

### 10.9 Legal and Redistribution Risk

The source's legal status and redistribution permissions are not established by the file itself.

Raw or lightly transformed source text must remain a private extraction reference until reviewed.

## 11. Required Source Metadata Still Missing

The following should be supplied or established later where possible:

```text
Exact original book title
Exact printing or edition identifier
Original language
Source used to create the cleaned file
Whether multiple sources were combined
Original page mapping
Sections intentionally removed
Sections intentionally rewritten
Known house rules
Known optional rules
Known corrections
Redistribution policy
Reviewer identity
```

Missing metadata does not block the first prototype extraction.

It does block final publication approval.

## 12. Source Retention Policy

Recommended policy:

```text
Raw cleaned source:
    private development reference

Installed package:
    must not include the raw file

Provenance records:
    may include hashes, heading paths, short locators, and transformation notes

Curated knowledge:
    newly authored and minimal

Mechanical artifacts:
    structured and reviewable

Long narrative passages:
    excluded unless redistribution is explicitly approved
```

## 13. Source Segment Strategy

The source should be segmented by semantic unit rather than arbitrary fixed-size chunks.

Recommended units:

```text
one rule
one exception
one table
one catalog entry
one procedure step
one resource definition
one operation
one terminology concept
one narrative-guidance concept
one example
```

A single heading may produce many source segments.

A single rule may also require several linked segments.

## 14. Locator Strategy

Because original page provenance is incomplete, the first extraction pass should use:

```text
source document ID
source version
heading path
local segment ordinal
line range
content fingerprint
```

Example:

```text
source.werewolf3e.cleaned-ptbr
    / SISTEMA DE CRIAÇÃO DE PERSONAGEM GAROU
    / RAÇAS GAROU
    / table-initial-gnosis
```

## 15. Recommended Extraction Order

### Slice 001 — Character Creation Foundation

Extract:

- Character identity fields;
- Race catalog;
- Auspice catalog;
- Tribe catalog;
- Attributes;
- Abilities;
- Backgrounds required for creation;
- Rage;
- Gnosis;
- Willpower;
- initial Gifts;
- freebie-point rules;
- one generic Dice test;
- initial Character sheet layout.

### Slice 002 — Core Resolution

Extract:

- generic test construction;
- difficulty;
- successes;
- failure;
- botch;
- specialties;
- resisted and extended tests where supported.

### Slice 003 — Forms, Health, and Basic Combat

Extract:

- forms;
- transformation;
- Health;
- damage;
- soak;
- regeneration;
- basic attack operations.

### Slice 004 — Frenzy and Resource Operations

Extract:

- Rage use;
- Frenzy triggers;
- Frenzy resolution;
- Willpower use;
- Gnosis use;
- mental states.

### Slice 005 — Progression and Renown

Extract:

- experience;
- Rank;
- Renown;
- narrative Background changes;
- progression restrictions.

### Later Slices

- Gifts;
- Rites;
- Spirits;
- Umbra;
- Totems;
- Fetishes;
- antagonists;
- extended setting knowledge.

## 16. First-Slice Publication Boundary

The first slice is a prototype and must be labeled:

```text
Candidate
Not for gameplay completeness
Not legally cleared for redistribution
Not a complete Werewolf implementation
```

Its purpose is to validate:

- package structure;
- Character schema;
- sheet rendering;
- operation contracts;
- Dice handoff;
- provenance;
- review workflow.

## 17. Expected First-Slice Outputs

```text
docs/extraction/werewolf-3e/
    EXTRACTION-0002-content-classification.md
    EXTRACTION-0003-character-creation-slice.md
    EXTRACTION-0004-ambiguities-and-conflicts.md
    EXTRACTION-0005-contract-findings.md

rule-sets/Chronicle.RuleSets.Werewolf/prototype/
    manifest.json
    character-model/
    character-sheet/
    operations/
    dice/
    rules/
    terminology/
    localization/
    provenance/
    fixtures/
```

## 18. Review Requirements

Before a candidate mechanical artifact becomes approved, require:

- extraction review;
- semantic review;
- implementation review;
- fixture review;
- provenance review;
- terminology review;
- legal and redistribution review for publication.

## 19. Initial Decision

The source is accepted as:

```text
Suitable for guided extraction:
    Yes

Suitable for the first vertical slice:
    Yes

Suitable for direct automatic conversion:
    No

Suitable as final runtime knowledge:
    No

Suitable as unreviewed mechanical authority:
    No

Suitable for generating candidate package artifacts:
    Yes
```

## 20. Next Action

Create:

```text
docs/extraction/werewolf-3e/
    EXTRACTION-0002-content-classification.md
```

That document will establish the first classification map, beginning with the Character creation vertical slice and separating:

- deterministic mechanics;
- Character fields;
- catalogs;
- operations;
- terminology;
- narrative guidance;
- examples;
- ambiguities;
- deferred content.

## 21. Final Assessment

The submitted file is a strong working source for Chronicle's first real package experiment.

It already contains enough structure to test the architecture against a real game.

It is not clean enough to bypass extraction contracts, provenance, review, or tests.

That is the correct balance for this stage:

```text
real enough to challenge the architecture
structured enough to extract
uncertain enough to require discipline
```
