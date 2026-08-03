---
id: EXTRACTION-0005
title: Werewolf 3e Rule Set Contract Findings
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
  - EXTRACTION-0002
  - EXTRACTION-0003
  - EXTRACTION-0004
related_to:
  - RFC-0044
  - RFC-0045
  - RFC-0046
  - RFC-0047
---

> **"A real Rule Set should pressure the contracts without being allowed to redefine Chronicle around itself."**

# Werewolf 3e Rule Set Contract Findings

## 1. Purpose

This document records which generic Rule Set package contracts were:

- validated by the first Werewolf extraction slice;
- challenged by real source material;
- shown to require refinement;
- shown to require later RFC or ADR formalization;
- shown to be Werewolf-specific and therefore forbidden from leaking into Chronicle Core.

These findings are architectural evidence.

They are not final Chronicle contracts by themselves.

## 2. Summary

The first extraction slice validates the overall package direction:

```text
Rule Set package
    =
declarative artifacts
    +
deterministic module
    +
presentation hints
    +
curated knowledge
    +
narrative guidance
    +
localization
    +
provenance
    +
tests
```

It also reveals that a usable Rule Set package requires more than a flat Character schema.

The source pressures the contract in these areas:

```text
staged Character creation
catalog-driven initialization
conditional restrictions
shared and group-owned values
heterogeneous mechanical effects
permanent and current resources
cross-field dependencies
source-rule ambiguity
narrative progression
linked Dice stages
terminology aliases
prototype decisions separated from source truth
```

## 3. Finding Status

Allowed statuses:

```text
Validated
ValidatedWithRefinement
ContractExtensionRequired
RFCRequired
ADRRequired
Deferred
RejectedAsSystemSpecific
```

## 4. CF-001 — Package as a Versioned Directory

```yaml
findingId: contract-finding.package-versioned-directory
status: Validated
severity: High
```

### Evidence

Werewolf content naturally separates into:

- manifest;
- Character model;
- sheet layout;
- operations;
- Dice;
- rules;
- terminology;
- guidance;
- localization;
- provenance;
- fixtures.

### Finding

A versioned package directory is the correct installation abstraction.

### Generic consequence

Chronicle should discover packages from manifests, not from hardcoded registrations.

### Required formalization

```text
RFC:
    Rule Set Package Model and Lifecycle

ADR:
    Package Discovery, Installation, and Version Isolation
```

## 5. CF-002 — Character Sheet Must Be Generated

```yaml
findingId: contract-finding.character-sheet-generated
status: Validated
severity: High
```

### Evidence

The source defines:

- identity fields;
- classification fields;
- grouped Attributes;
- grouped Abilities;
- Backgrounds;
- Gifts;
- Renown;
- Rank;
- resources;
- narrative notes.

These can be represented as fields and presentation metadata.

### Finding

A hardcoded Werewolf sheet is unnecessary and architecturally harmful.

### Generic consequence

Chronicle Presentation needs a generic sheet renderer driven by package artifacts.

### Required formalization

```text
RFC:
    Declarative Character Model and Sheet Rendering

ADR:
    Generic Character Sheet Rendering Architecture
```

## 6. CF-003 — Character Creation Is a Workflow

```yaml
findingId: contract-finding.character-creation-workflow
status: ValidatedWithRefinement
severity: High
```

### Evidence

Creation includes ordered dependencies:

```text
Race, Auspice, Tribe
    before
resource initialization, Background restrictions, and Gift eligibility

base allocations
    before
freebie spending

all required steps
    before
Character completion
```

### Finding

Character creation cannot be represented only as editable fields plus final validation.

It requires a versioned workflow with:

- ordered steps;
- prerequisites;
- draft state;
- recalculation;
- completion requirements;
- step-specific validation.

### Generic consequence

Add a first-class `CreationProfile` contract.

## 7. CF-004 — Classification Selections Initialize Other Values

```yaml
findingId: contract-finding.catalog-driven-initialization
status: Validated
severity: High
```

### Evidence

Source-derived candidates include:

```text
Race:
    initializes Gnosis

Auspice:
    initializes Rage and Renown

Tribe:
    initializes Willpower
    constrains Backgrounds
    supplies Gift eligibility
```

### Finding

Catalog entries need structured mechanical metadata and rule references.

### Generic consequence

A catalog is not merely localized display data.

It may own:

- initialization effects;
- constraints;
- eligibility lists;
- tags;
- handler references;
- provenance.

## 8. CF-005 — Selection Revision Requires Dependency Recalculation

```yaml
findingId: contract-finding.selection-recalculation
status: ContractExtensionRequired
severity: High
```

### Evidence

Changing Race, Auspice, or Tribe may invalidate:

- resource values;
- restricted Abilities;
- Backgrounds;
- Gifts;
- Renown;
- freebie accounting.

### Finding

The generic workflow needs a dependency-impact contract.

### Proposed extension

```text
ChangePreview
    preserved values
    recalculated values
    invalidated values
    removed selections
    refunded costs
    newly required selections
```

### Generic consequence

Chronicle should present and confirm the impact before applying the change.

## 9. CF-006 — Creation Budgets Need Typed Allocation Contracts

```yaml
findingId: contract-finding.typed-allocation-budget
status: Validated
severity: High
```

### Evidence

The source uses several distinct budgets:

```text
Attributes:
    7 / 5 / 3 by prioritized category

Abilities:
    13 / 9 / 5 by prioritized category

Backgrounds:
    5 total

Gifts:
    one from each eligibility source

Freebies:
    15 with category-specific costs
```

### Finding

A generic `AllocationBudget` must support:

- priority categories;
- exact totals;
- per-field limits;
- per-stage limits;
- category-specific costs;
- selection quotas;
- conditional restrictions;
- refunds during revision.

### Generic consequence

A flat `pointsRemaining` field is insufficient.

## 10. CF-007 — Base Allocation and Purchase Stages Differ

```yaml
findingId: contract-finding.creation-stage-specific-limits
status: ContractExtensionRequired
severity: High
```

### Evidence

The source distinguishes:

- base Ability allocation;
- later freebie spending;
- post-creation progression.

Different limits may apply at each stage.

### Finding

Field constraints need a stage-aware context.

### Proposed generic context

```text
CreationBaseAllocation
CreationBonusPurchase
PostCreationProgression
NarrativeChange
Correction
Import
```

### Generic consequence

`maximumDuringCreation` is too coarse.

## 11. CF-008 — Resources Need More Than One Numeric Value

```yaml
findingId: contract-finding.resource-multi-value
status: ValidatedWithRefinement
severity: High
```

### Evidence

Rage, Gnosis, Willpower, Renown, and later Health imply distinctions between:

- permanent rating;
- current value;
- maximum;
- temporary accumulation;
- historical change.

### Finding

`ResourceTrack` must support package-defined dimensions rather than assuming one current/max pair.

### Proposed shape

```text
ResourceDefinition
    dimension definitions
    initialization rules
    spend rules
    restore rules
    maximum rules
    display hints
    ledger policy
```

### Generic consequence

Chronicle should not encode Rage-like semantics in Core.

## 12. CF-009 — Heterogeneous Effects Require a Hybrid Model

```yaml
findingId: contract-finding.heterogeneous-effect-model
status: ValidatedWithRefinement
severity: High
```

### Evidence

Metis deformities may cause:

- static difficulty modifiers;
- automatic failures;
- maximum-value changes;
- extra Dice;
- missing Health levels;
- conditional tests;
- attacks;
- narrative or Renown consequences.

### Finding

A single generic modifier table is insufficient.

A fully compiled-only approach would also hide too much structure.

### Recommended model

```text
declarative identity
    +
declarative static effects
    +
bounded event hooks
    +
compiled handlers for complex behavior
```

### Generic consequence

Introduce a versioned `EffectDefinition` registry and compiled handler references.

## 13. CF-010 — Rules Need Explicit Scope and Timing

```yaml
findingId: contract-finding.rule-scope-and-timing
status: ContractExtensionRequired
severity: High
```

### Evidence

The source distinguishes rules that apply:

- only during base creation;
- during freebie spending;
- later through training;
- under stress;
- in specific forms;
- only for certain tests;
- only when a specialization applies.

### Finding

Every `RuleDefinition` needs explicit:

```text
scope
phase
trigger
timing
duration
precedence
exception links
```

### Generic consequence

An expression and effect alone are insufficient.

## 14. CF-011 — Narrative Descriptions Must Not Become Hard Constraints

```yaml
findingId: contract-finding.guidance-authority-separation
status: Validated
severity: Critical
```

### Evidence

Auspices and Tribes contain:

- social roles;
- personality tendencies;
- cultural expectations;
- stereotypes;
- historical context;
- occasional explicit mechanics.

### Finding

The package must separate:

```text
Narrative Guidance
Mechanical Rule
Eligibility Constraint
Terminology
Setting Knowledge
```

### Generic consequence

Guidance artifacts need an explicit nonauthoritative marker.

## 15. CF-012 — Terminology Requires Alias and Context Support

```yaml
findingId: contract-finding.terminology-alias-context
status: ValidatedWithRefinement
severity: Medium
```

### Evidence

The source includes:

- Portuguese labels;
- English names;
- archaic terms;
- slang;
- honorifics;
- derogatory in-world terms;
- overlapping terms such as Rites.

### Finding

Terminology needs:

```text
canonical key
localized display labels
synonyms
historical labels
derogatory-context marker
concept category
relationship links
disambiguation notes
```

### Generic consequence

Localization alone is insufficient for terminology modeling.

## 16. CF-013 — Shared Values Need Ownership Metadata

```yaml
findingId: contract-finding.shared-resource-ownership
status: ContractExtensionRequired
severity: Medium
```

### Evidence

Totem may involve individual Background contributions toward a pack-owned entity.

### Finding

A value may be:

```text
Character-owned
Campaign-owned
Group-owned
Contributed-by-Character
Derived-from-Group
```

### Generic consequence

Field and resource definitions need ownership and aggregation policy.

### Slice decision

Full group aggregation is deferred.

## 17. CF-014 — Narrative Progression Requires Authorized Operations

```yaml
findingId: contract-finding.narrative-progression
status: Validated
severity: High
```

### Evidence

Some Backgrounds appear to change through narrative events rather than ordinary experience spending.

### Finding

Progression must support at least:

```text
CostedProgression
NarrativeAward
NarrativeLoss
AdministrativeCorrection
ImportedHistory
```

### Generic consequence

Chronicle must persist why a value changed, not only the new value.

## 18. CF-015 — Gift Selection and Gift Execution Are Separate

```yaml
findingId: contract-finding.selection-versus-execution
status: Validated
severity: Medium
```

### Evidence

Slice 001 can model Gift:

- identity;
- level;
- eligibility;
- selection source.

It cannot yet model all Gift activation behavior.

### Finding

Package contracts should separate:

```text
CatalogEntry
AcquisitionRule
CharacterOwnership
ActivationOperation
ResolutionRule
Effect
```

### Generic consequence

Owning an ability does not imply one universal execution contract.

## 19. CF-016 — Dice Specialization May Require Linked Evidence

```yaml
findingId: contract-finding.linked-additional-dice
status: ValidatedWithRefinement
severity: High
```

### Evidence

The source summary describes additional Dice generated by qualifying results under a specialization.

### Finding

Chronicle's generic Dice model is correctly designed to support:

```text
base evidence
    →
linked additional-Dice request
    →
additional evidence
    →
combined resolution
```

### Generic consequence

Do not collapse additional Dice into untraceable extra successes.

### Required source work

The exact Werewolf chaining and result-of-1 behavior remains unresolved.

## 20. CF-017 — Historical Dice Must Bind to Exact Package Version

```yaml
findingId: contract-finding.dice-package-binding
status: Validated
severity: High
```

### Evidence

The resolver is package-specific and may evolve.

### Finding

Every resolved Roll must preserve:

```text
PackageId
PackageVersion
Rule Contract Version
Resolver Key
Resolver Version
```

### Generic consequence

Historical results must not be recalculated silently after a package upgrade.

## 21. CF-018 — Source Truth and Prototype Decisions Must Be Separate

```yaml
findingId: contract-finding.source-versus-prototype-decision
status: Validated
severity: Critical
```

### Evidence

Several rules remain ambiguous, but the prototype may need temporary behavior to proceed.

### Finding

The package workspace needs separate records for:

```text
Source-Derived Candidate
Prototype Decision
Approved Rule
Implementation Mapping
```

### Generic consequence

A prototype choice must never masquerade as extracted source truth.

## 22. CF-019 — Provenance Is Required at Artifact Level

```yaml
findingId: contract-finding.artifact-level-provenance
status: Validated
severity: Critical
```

### Evidence

One source segment may generate several artifacts:

```text
catalog entry
initialization rule
operation validation
localization string
fixture
guidance entry
```

### Finding

Provenance must link at artifact and rule level, not only package level.

### Generic consequence

Every deterministic artifact requires source and normalization references.

## 23. CF-020 — The Source File Must Stay Outside Runtime

```yaml
findingId: contract-finding.no-raw-source-runtime
status: Validated
severity: Critical
```

### Evidence

The source is:

- editorially transformed;
- legally unclassified;
- broad;
- redundant;
- mixed in authority.

### Finding

The raw cleaned file must not become runtime mechanical authority.

### Generic consequence

Installed packages should contain:

- approved structured mechanics;
- curated knowledge;
- approved guidance;
- provenance metadata;

not the private source file by default.

## 24. CF-021 — Package Legal Status Must Be Artifact-Specific

```yaml
findingId: contract-finding.artifact-legal-status
status: ContractExtensionRequired
severity: High
```

### Evidence

Different outputs have different redistribution risks:

```text
structured numeric rule
canonical term
newly authored explanation
short source-derived label
long narrative passage
source locator
private source segment
```

### Finding

Legal status cannot be one package-wide boolean during extraction.

### Proposed statuses

```text
PrivateReference
ReviewRequired
StructuredFactCandidate
ChronicleAuthored
Redistributable
Restricted
Rejected
```

## 25. CF-022 — Package Capability Declarations Are Necessary

```yaml
findingId: contract-finding.package-capabilities
status: Validated
severity: High
```

### Evidence

Slice 001 needs only:

```text
character-model
character-creation
character-sheet
generic-dice-test
```

It does not yet need:

```text
combat
Gift execution
Rites
Umbra
progression
```

### Finding

A package must declare which capabilities are actually implemented.

### Generic consequence

Chronicle should not infer capability from folder presence alone.

## 26. CF-023 — Partial Package Support Needs Explicit Readiness

```yaml
findingId: contract-finding.capability-readiness
status: ContractExtensionRequired
severity: Medium
```

### Evidence

A prototype may contain valid Character creation artifacts but incomplete combat.

### Finding

Each capability needs a readiness state:

```text
Unavailable
Candidate
Prototype
Supported
Deprecated
Restricted
```

### Generic consequence

The UI and Application layer must not advertise unsupported mechanics.

## 27. CF-024 — Contract Versioning Must Be Per Artifact Family

```yaml
findingId: contract-finding.artifact-family-versioning
status: ValidatedWithRefinement
severity: High
```

### Evidence

Character fields, creation workflows, Dice resolvers, terminology, and presentation evolve at different rates.

### Finding

One package version is not enough to interpret every artifact.

### Generic consequence

Use:

```text
PackageVersion
RuleSetContractVersion
ArtifactFamilyContractVersion
OperationVersion
ResolverVersion
```

## 28. CF-025 — Generic Core Must Be Tested Against a Non-Werewolf Fixture

```yaml
findingId: contract-finding.synthetic-cross-system-fixture
status: Validated
severity: Critical
```

### Evidence

The Werewolf slice naturally uses:

```text
Race
Auspice
Tribe
d10 pools
Rage
Gnosis
Willpower
```

These are not universal RPG concepts.

### Finding

Every generic contract introduced because of this slice must be tested with a synthetic package that uses different concepts.

### Required fixture characteristics

```text
no Race
no Auspice
no Tribe
different Dice kind
different Character fields
different creation order
different resource model
```

## 29. Rejected Werewolf-Specific Core Concepts

The following must not enter Chronicle Core:

```text
Garou
Race as a universal Character concept
Auspice
Tribe
Rage
Gnosis
Willpower as fixed Core resources
Renown
Glory
Honor
Wisdom
Cliath
Gift
Totem
Metis deformity
d10 success rules
specialization-on-10 behavior
Werewolf Health levels
forms
Frenzy
```

Status:

```text
RejectedAsSystemSpecific
```

These belong to the package.

## 30. Validated Generic Concepts

The source validates these generic concepts:

```text
versioned package
manifest
field definition
catalog
resource definition
creation workflow
allocation budget
selection constraint
initialization rule
operation contract
effect definition
Dice plan
Dice resolver
progression operation
presentation hint
terminology entry
narrative guidance
provenance
fixture
capability
review status
legal status
```

## 31. RFCs Required

The findings support creating these RFCs.

### RFC — Rule Set Package Model and Lifecycle

Should define:

- package identity;
- directory structure;
- manifest;
- installation;
- discovery;
- compatibility;
- capability declaration;
- version coexistence;
- activation;
- restricted mode.

### RFC — Declarative Character Model and Sheet Rendering

Should define:

- field types;
- catalogs;
- resources;
- ownership;
- layout elements;
- generic renderer;
- validation separation;
- computed fields.

### RFC — Rule Definitions and Mechanical Operations

Should define:

- rule scope;
- timing;
- expressions;
- effects;
- handlers;
- operation contracts;
- stage context;
- dependency impact;
- error keys.

### RFC — Rule Set Provenance and Knowledge Separation

Should define:

- source segments;
- provenance;
- curated knowledge;
- narrative guidance;
- legal status;
- review workflow;
- source retention.

## 32. ADRs Required

The findings support creating these ADRs.

### ADR — Package Discovery and Version Isolation

Decision topics:

- managed directory;
- manifest scan;
- compatibility validation;
- integrity verification;
- version binding;
- no hardcoded Werewolf registration.

### ADR — Hybrid Declarative and Compiled Package Model

Decision topics:

- what remains declarative;
- handler interfaces;
- capability boundary;
- no DbContext;
- no unrestricted filesystem or network.

### ADR — Generic Character Sheet Rendering

Decision topics:

- renderer ownership;
- layout registry;
- custom component policy;
- validation boundary;
- localization.

### ADR — Rule Set Module Execution and Trust Boundary

Decision topics:

- trust;
- package signatures;
- code loading;
- isolation expectations;
- failure behavior;
- package disablement.

## 33. SPEC-0001 Changes Recommended

The first source pass suggests these future refinements to `SPEC-0001`.

### Add Creation Stage Context

```text
CreationBaseAllocation
CreationBonusPurchase
PostCreationProgression
NarrativeChange
Correction
Import
```

### Add Rule Timing

```text
trigger
phase
duration
precedence
exceptionRefs
```

### Add Ownership Policy

```text
Character
Group
Campaign
Contribution
Derived
```

### Add Capability Readiness

```text
Unavailable
Candidate
Prototype
Supported
Deprecated
Restricted
```

### Add Prototype Decision Artifact

Keep source truth separate from temporary implementation choices.

### Add Artifact-Specific Legal Status

Do not rely only on package-level legal metadata.

### Add Dependency Impact Preview

Support safe revision of selections with dependent values.

## 34. Prototype Artifact Readiness

Based on these findings:

```text
Manifest skeleton:
    Ready

Character Model skeleton:
    Ready

Creation workflow skeleton:
    Ready

Sheet skeleton:
    Ready

Race catalog:
    Partially ready

Auspice catalog:
    Partially ready

Tribe catalog:
    Blocked by completeness extraction

Attribute catalog:
    Ready for detailed extraction

Ability catalog:
    Ready for detailed extraction

Background catalog:
    Partially ready

Initial Gift catalog:
    Partially ready

Resource model:
    Ready as provisional multi-dimension contract

Freebie operation:
    Partially ready

Generic Dice plan:
    Ready

Generic Dice resolver:
    Blocked by exact source extraction
```

## 35. Recommended Next Step

The documentation phase for the initial extraction is complete enough to begin concrete package artifacts.

The next file should no longer be placed under `docs/extraction`.

Create:

```text
rule-sets/Chronicle.RuleSets.Werewolf/prototype/manifest.json
```

Then proceed through:

```text
character-model/model.json
character-model/catalogs/races.json
character-model/catalogs/auspices.json
character-model/fields/
character-model/creation/profile.json
character-sheet/default-sheet.json
```

The Tribe, Ability, Gift, and Dice resolver artifacts should remain incomplete or disabled until their exact source segments are extracted.

## 36. Final Finding

The source confirms the package architecture.

It also proves that the architecture must support more than a static schema.

A real Rule Set package needs:

```text
data
    +
workflow
    +
rules
    +
operations
    +
deterministic code
    +
presentation
    +
knowledge
    +
provenance
    +
review
```

Werewolf supplies the first pressure test.

Chronicle remains the framework.
