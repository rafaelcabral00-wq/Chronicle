---
id: SPEC-0001
title: Rule Set Package Artifact Model and Extraction Contract
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-08-03
category: Rule Set Architecture
canonical_language: English
applies_to:
  - Chronicle Rule Set packages
  - official Rule Sets
  - third-party Rule Sets
  - source-document extraction workflows
depends_on:
  - RFC-0012
  - RFC-0021
  - RFC-0025
  - RFC-0031
  - RFC-0033
  - ADR-0002
  - ADR-0004
  - ADR-0024
  - ADR-0033
  - ADR-0040
  - ADR-0041
  - ADR-0043
validated_by:
  - Werewolf 3e Extraction Slice 001
---

> **"A sourcebook is evidence. A Rule Set package is an executable, reviewed interpretation of that evidence."**

# Rule Set Package Artifact Model and Extraction Contract

## 1. Purpose

This specification defines:

- the artifact families contained in an installable Chronicle Rule Set package;
- the separation between declarative data, deterministic implementation, curated knowledge, and presentation hints;
- the extraction workflow used to transform source material into package artifacts;
- provenance, confidence, review, validation, and publication requirements;
- the boundary between Chronicle Core and system-specific content;
- the minimum contracts required to validate the architecture against a real Rule Set.

This specification is Rule Set-neutral.

Werewolf is the first reference package used to test the contract. It receives no privileged Core behavior.

## 2. Foundational Rules

```text
Chronicle owns framework authority.

A Rule Set package owns system-specific mechanics.

A source document does not become executable authority automatically.

Extracted content is a candidate until reviewed.

Narrative guidance is not deterministic mechanics.

Presentation hints are not validation rules.

The first package must use the same package path as future packages.
```

## 3. Rule Set Package Definition

A Rule Set package is a versioned, integrity-verifiable installation unit that may contain:

```text
Declarative Artifacts
Compiled Deterministic Module
Curated Rule Knowledge
Narrative Guidance
Localization Resources
Provenance Records
Fixtures and Validation Metadata
Integrity Metadata
```

A package is not:

- a raw sourcebook;
- a folder of unclassified text;
- an embedded Chronicle Core subsystem;
- a fixed screen implementation;
- a provider prompt collection with no deterministic rules;
- a direct database plugin.

## 3.1 Shared Rule Set Lifecycle

Effective 2026-08-03, DR-0003 defines the shared normative Rule Set lifecycle used by extraction, package validation, promotion, publication, installation, and runtime activation.

Lifecycle states:

```text
proposed
source-registered
slice-defined
extracted
modeled
structurally-valid
substantively-reviewed
decision-set-finalized
evidence-complete
promotion-eligible
promoted
published
installed
enabled
active
deprecated
withdrawn
```

The lifecycle has four distinct dimensions:

- package maturity: `proposed` through `promoted`;
- publication status: unpublished, `published`, `deprecated`, or `withdrawn`;
- installation status: not-installed or `installed`;
- runtime activation status: disabled, `enabled`, or `active`.

Promotion and publication are separate:

- promotion means the package satisfies the declared quality gate for a release scope;
- publication means an artifact was distributed through an approved channel;
- a package may be `promotion-eligible` or `promoted` without being `published`;
- publication never implies `installed`, `enabled`, or `active`.

## 3.2 Lifecycle Transition Contract

Allowed forward transitions:

| From | To | Required evidence | Owner | Reversible |
| --- | --- | --- | --- | --- |
| proposed | source-registered | source identity, provenance notes, initial scope intent | Rule Set author or maintainer | yes |
| source-registered | slice-defined | release scope, capability intent, exclusions, disabled operations | Rule Set maintainer | yes |
| slice-defined | extracted | extraction records, source fingerprints, classification records | Extraction workflow and reviewer | yes, by superseding extraction |
| extracted | modeled | package artifacts or models, stable keys, artifact family declarations | Rule Set authoring workflow | yes |
| modeled | structurally-valid | schema validation, reference resolution, dependency checks, artifact uniqueness | Validation tooling | yes, if inputs change |
| structurally-valid | substantively-reviewed | source/semantic review records, ambiguity disposition, legal/provenance review where required | Review authority | yes, if evidence becomes stale |
| substantively-reviewed | decision-set-finalized | accepted decisions for scope, exclusions, blockers, and source ambiguities | Rule Set maintainer or formal review authority | yes, by superseding decisions |
| decision-set-finalized | evidence-complete | required tests, localization checks, security checks, compatibility checks, reconciliation, freshness evidence | Validation and release tooling | yes, if evidence becomes stale |
| evidence-complete | promotion-eligible | declared quality gate passes with no blocking issue | Promotion gate evaluator | yes, if gate inputs change |
| promotion-eligible | promoted | promotion record binding exact artifacts, fingerprints, scope, and evidence | Release authority | no, must supersede or withdraw |
| promoted | published | approved distribution channel record and publication metadata | Release authority | no, must withdraw/deprecate |
| published | installed | installation manifest, integrity verification, compatibility check | Installer or package manager | yes, by uninstalling when no active Campaign requires it |
| installed | enabled | user/admin enablement or policy enablement, compatibility still valid | User/admin or application policy | yes |
| enabled | active | Campaign binding to exact package identity and version | Application runtime | no silent reversal; Campaign migration required |
| active | deprecated | deprecation notice, replacement guidance, compatibility notes | Maintainer/release authority | no |
| published, installed, enabled, deprecated | withdrawn | withdrawal reason, safety/legal notice, replacement or preservation policy | Release authority | no |

Blocking conditions include missing source registration, undefined release scope, advertised but unimplemented capability, unresolved source ambiguity inside advertised scope, failed structural validation, missing review, incomplete decision set, stale evidence, failing required tests, missing localization validation for advertised locales, missing security validation, unresolved blockers, incompatible Chronicle contract versions, missing publication metadata, failed installation integrity, or withdrawal for legal, safety, or provenance reasons.

## 3.3 Materialization Role Mapping

Effective 2026-08-03, DR-0004 defines the normative mapping between documentation prototype, package source, packaged artifact, and installed artifact.

These roles are distinct:

| Role | Authority | May contain | Must not imply |
| --- | --- | --- | --- |
| Documentation prototype | Review and authoring evidence | extracted models, candidate package artifacts, review records, issues, blockers, tests, security notes, localization reports, reconciliation evidence | executable authority, package source authority, packaged artifact authority, installation, enablement, activation, Campaign binding, promotion, or publication |
| Package source | Authoritative source inputs for building a package artifact | source files, declarative artifacts, compiled-module source, manifests, fixtures, tests, provenance, validation configuration | installation, enablement, activation, Campaign binding, or publication |
| Packaged artifact | Versioned build output eligible for integrity checks, promotion, publication, and installation | manifest, package content, compiled module when applicable, integrity metadata, license/provenance metadata, validation evidence references | installation, enablement, activation, Campaign binding, or publication unless the corresponding lifecycle transition occurs |
| Installed artifact | Local verified package instance available to the application environment | exact packaged artifact content, installation manifest, integrity verification result, compatibility result | enablement, activation, Campaign binding, or migration |

The Werewolf documentation prototype path is reference evidence for the first package. It is not the generic package source layout.

This specification does not select runtime implementation details, serialization format beyond existing authorized layouts, package build tooling, or installation technology.

## 3.4 Materialization Progression Requirements

Progression between materialization roles requires explicit transformation and accepted evidence.

Documentation prototype to package source requires:

- declared release scope;
- identity preservation plan for package ID, Rule Set version, package version, artifact keys, operation keys, and localization keys;
- source fingerprints and artifact fingerprints;
- accepted extraction, substantive review, and decision records;
- explicit exclusions and disabled operations;
- reconciliation from prototype records to source-controlled package inputs.

Package source to packaged artifact requires:

- reproducible or traceable build record;
- manifest validation;
- structural validation;
- artifact reference validation;
- compatibility validation;
- source and package fingerprint binding;
- executable test evidence for advertised executable behavior;
- localization validation for advertised locales;
- security validation for package boundaries;
- migration validation when package migration is advertised or required;
- reconciliation between source inputs and packaged output.

Packaged artifact to installed artifact requires:

- installation manifest;
- integrity verification;
- Chronicle contract compatibility verification;
- dependency and capability verification;
- confirmation that the artifact is not withdrawn for legal, security, provenance, or compatibility reasons.

Installed artifact to enabled or active follows the lifecycle transition contract in this specification and RFC-0027.

No materialization role may skip lifecycle states or infer authority from filesystem presence.

## 3.5 Required Evidence Families

The minimum evidence families for materialization and promotion readiness are:

```text
StructuralValidation
SubstantiveReview
FinalizedDecisions
SourceProvenance
LocalizationValidation
FixtureValidation
ExecutableTestResults
SecurityValidation
ReconciliationReport
CompatibilityValidation
MigrationValidationWhenApplicable
PromotionReadiness
```

Evidence must include enough identity and fingerprint data to prove which source, prototype record, package source input, packaged artifact, installed artifact, test environment, and validation result were evaluated.

Stale evidence blocks progression until refreshed or explicitly superseded.

Accepted evidence may be referenced by later roles, but each role must bind evidence to the exact artifact or input being advanced.

## 3.6 Materialization Blocking Rule

Repository materialization for a Rule Set package remains blocked until the normative mapping and validation contract in this specification is present and accepted.

After this section is accepted, materialization may be planned against this contract, but this specification still does not create source directories, package artifacts, installed artifacts, Campaign bindings, publication records, or runtime activation.

## 4. Installed Package Layout

Recommended installed layout:

```text
rule-sets/
└── <package-id>/
    └── <package-version>/
        ├── manifest.json
        ├── module/
        ├── contracts/
        ├── character-model/
        ├── character-sheet/
        ├── operations/
        ├── dice/
        ├── progression/
        ├── rules/
        ├── knowledge/
        ├── narrative-guidance/
        ├── terminology/
        ├── localization/
        ├── provenance/
        └── integrity/
```

The exact serialization format may evolve. The semantic boundaries are normative.

## 5. Repository Layout

Recommended repository layout:

```text
rule-sets/
└── Chronicle.RuleSets.<SystemName>/
    ├── src/
    ├── package/
    ├── fixtures/
    ├── tests/
    ├── extraction/
    └── tooling/
```

Development-only files must not be shipped automatically.

## 6. Package Identity

Every package declares:

```text
PackageId
PackageVersion
PublisherId
RuleSetContractVersion
MinimumChronicleVersion
MaximumChronicleVersionPolicy
DisplayNameResourceKey
SupportedLocales
Capabilities
Dependencies
EntryPoints
ContentHashes
SignatureMetadata
LicenseMetadata
ProvenanceSummary
```

## 7. Package ID

`PackageId` is:

- globally stable;
- lowercase or canonical normalized form;
- publisher-namespaced;
- never localized;
- independent from display name.

Example:

```text
chronicle.rulesets.werewolf
```

The final Werewolf PackageId remains a governance decision.

## 8. Artifact Families

The canonical artifact families are:

```text
Manifest
Character Model
Character Sheet Presentation
Mechanical Operations
Dice Contracts
Progression Model
Rule Definitions
Terminology
Curated Rule Knowledge
Narrative Guidance
Localization
Provenance
Validation Metadata
Integrity Metadata
Compiled Module
```

## 9. Declarative Versus Compiled Responsibility

### Declarative artifacts should represent:

- field definitions;
- value types;
- catalogs;
- selection options;
- simple constraints;
- section and layout composition;
- operation input/output schemas;
- resource metadata;
- terminology;
- localization;
- provenance;
- deterministic rule metadata;
- display hints.

### Compiled deterministic code should represent:

- complex validation;
- interdependent prerequisites;
- derived calculations;
- Dice plan construction;
- Dice resolution;
- progression costs;
- contextual modifiers;
- multi-stage mechanics;
- complex effects;
- package migrations.

No rule is compiled merely because extraction is difficult. The selection depends on runtime behavior.

## 10. Manifest Artifact

File recommendation:

```text
manifest.json
```

Required semantic fields:

```text
PackageId
PackageVersion
PublisherId
RuleSetContractVersion
ChronicleCompatibility
DisplayNameResourceKey
DescriptionResourceKey
Capabilities
Dependencies
ArtifactIndex
ModuleEntryPoints
SupportedLocales
IntegrityReference
ProvenanceReference
LicenseReference
```

## 11. Character Model

The Character Model defines authoritative system-specific Character structure.

It does not define a hardcoded desktop screen.

Recommended artifacts:

```text
character-model/
├── model.json
├── fields/
├── catalogs/
├── resources/
├── derived-values/
├── creation/
└── validation/
```

## 12. Character Field Definition

Canonical conceptual shape:

```yaml
fieldKey: character.attribute.strength
contractVersion: 1
valueType: integer
required: true
defaultValue: 1
constraints:
  minimum: 1
  maximum: 5
displayResourceKey: character.attribute.strength
helpResourceKey: character.attribute.strength.help
authority: rule-set
persistence:
  mode: authoritative
provenanceRefs:
  - source.werewolf3e.attributes.strength
```

## 13. Supported Initial Field Types

The first implementation should support:

```text
Text
LongText
Integer
Decimal
Boolean
SingleChoice
MultipleChoice
RankTrack
ResourceTrack
Reference
ReferenceCollection
StructuredObject
StructuredCollection
ComputedDisplay
```

New types require contract versioning.

## 14. Field Authority

Field authority may be:

```text
UserAuthored
RuleSetAuthoritative
ChronicleAuthoritative
Derived
ReadOnlyHistorical
```

## 15. Character Catalogs

Catalogs represent stable package-owned options such as:

- origins;
- archetypes;
- roles;
- classes;
- races;
- auspices;
- tribes;
- abilities;
- backgrounds;
- gifts;
- forms;
- resources;
- ranks.

A catalog entry is not automatically a Character field value. The Character Model defines the relationship.

## 16. Character Catalog Entry

Conceptual shape:

```yaml
entryKey: character.race.homid
displayResourceKey: character.race.homid
descriptionResourceKey: character.race.homid.description
tags:
  - character-origin
mechanicalData:
  initialResourceValues:
    gnosis: 1
provenanceRefs:
  - source.werewolf3e.race.homid
```

## 17. Character Creation Contract

Character creation is a versioned workflow, not unrestricted direct editing.

Recommended structure:

```text
CreationProfile
CreationStep[]
AllocationBudget[]
SelectionConstraint[]
DerivedInitialization[]
ValidationRule[]
CompletionRequirement[]
```

## 18. Character Creation Step

Conceptual shape:

```yaml
stepKey: character-creation.select-race
order: 10
operationKey: character.creation.select-race
required: true
inputSchemaRef: contracts/character/select-race-input
outputSchemaRef: contracts/character/select-race-result
```

## 19. Character Sheet Presentation

The package supplies declarative presentation hints.

Recommended artifacts:

```text
character-sheet/
├── sheet.json
├── sections/
└── views/
```

## 20. Presentation Is Nonauthoritative

Sheet layout controls:

- grouping;
- order;
- section labels;
- preferred controls;
- visibility hints;
- read-only summaries;
- contextual help.

It must not be the only location of validation, prerequisites, derived logic, mechanical limits, or operation authorization.

## 21. Initial Layout Elements

```text
Section
Group
Tabs
Columns
Field
ResourceTrack
RankTrack
RepeatingList
ReferenceList
ReadOnlySummary
HelpBlock
```

## 22. Mechanical Operations

A mechanical operation is the package-owned deterministic contract for an allowed system action.

Examples:

```text
character.creation.select-race
character.creation.allocate-attributes
character.update-field
resource.spend
resource.restore
dice.build-plan
dice.resolve
progression.purchase
form.change
gift.activate
```

## 23. Operation Contract

Every operation defines:

```text
OperationKey
OperationVersion
Purpose
InputSchema
OutputSchema
RequiredCapabilities
Preconditions
ValidationHandler
ExecutionHandler
ProposedEffects
ErrorKeys
ProvenanceRefs
TestFixtureRefs
```

## 24. No Direct Persistence

Operation handlers return validated proposals or results to Chronicle Application.

They do not access DbContext.

## 25. Dice Contracts

Rule Set packages convert mechanical context into Chronicle's generic Dice structures.

Recommended artifacts:

```text
dice/
├── operations/
├── plans/
├── resolution/
├── modifiers/
└── display/
```

## 26. Dice Plan Responsibility

The Rule Set package defines which Dice are needed, groups, Dice kinds, quantities, modifiers, selection rules, stage requirements, and post-Roll decisions.

Chronicle generates and commits raw evidence.

## 27. Dice Resolution Responsibility

The exact Rule Set package deterministically interprets committed evidence.

## 28. Dice Contract Neutrality

Package contracts may use a simple d10 pool.

Chronicle Core must remain capable of representing:

- multiple Dice kinds;
- multiple groups;
- mixed pools;
- rerolls;
- exploding or chained Dice;
- keep/drop;
- symbols;
- opposed Rolls;
- staged Rolls;
- resource spending;
- post-Roll decisions.

## 29. Progression Model

Recommended artifacts:

```text
progression/
├── resources/
├── costs/
├── prerequisites/
├── operations/
└── ledgers/
```

## 30. Progression Principles

Progression is performed through operations.

Direct value editing must not bypass cost, prerequisites, package rules, provenance, or ledger history.

## 31. Narrative Progression

Some changes may depend on Campaign events rather than experience expenditure.

These require Chronicle-authorized narrative operations, not arbitrary Character editing.

## 32. Rule Definition

`RuleDefinition` is the canonical structured representation of an extracted rule candidate or approved package rule.

## 33. Rule Definition Shape

```yaml
ruleKey: character.creation.race-determines-initial-gnosis
ruleVersion: 1
status: candidate
category: character-creation
ruleType: initialization
summaryResourceKey: rule.character.creation.race-gnosis.summary

scope:
  entityTypes:
    - character

appliesWhen:
  expressionContractVersion: 1
  expression:
    field: character.race
    operator: in
    values:
      - character.race.homid
      - character.race.metis
      - character.race.lupus

effects:
  - effectType: set-initial-resource
    targetFieldKey: character.resource.gnosis
    valueSource:
      lookupTableRef: tables/race-initial-gnosis

references:
  fieldKeys:
    - character.race
    - character.resource.gnosis
  operationKeys:
    - character.creation.select-race

provenanceRefs:
  - source.werewolf3e.character-creation.races.initial-gnosis

review:
  extractionConfidence: high
  semanticReviewStatus: required
  implementationReviewStatus: required
```

## 34. Rule Status

```text
Candidate
NeedsClarification
Reviewed
Approved
Implemented
Tested
Deferred
Rejected
Superseded
```

Only approved release states may enter an official package.

## 35. Rule Types

Initial registry:

```text
Definition
Constraint
Initialization
Calculation
Prerequisite
Permission
Prohibition
Cost
ResourceChange
DicePlan
DiceResolution
Effect
Duration
Exception
Override
Progression
NarrativeTrigger
PresentationHint
```

## 36. Expression Contract

Simple rules may use a bounded declarative expression language.

Initial allowlisted concepts:

```text
all
any
not
equals
not-equals
greater-than
greater-than-or-equal
less-than
less-than-or-equal
in
contains
exists
lookup
add
subtract
multiply
minimum
maximum
```

No arbitrary script execution is permitted.

## 37. Complex Rules

Rules requiring loops, cross-aggregate interpretation, multi-stage state, or complex contextual logic may reference compiled handlers.

## 38. Handler Reference

```yaml
implementation:
  mode: compiled
  handlerKey: werewolf.character.creation.validate-lupus-ability-restrictions
  handlerVersion: 1
```

The handler remains package-owned and capability-bounded.

## 39. Terminology

Terminology defines canonical package terms and relationships.

Recommended artifacts:

```text
terminology/
├── concepts.json
├── synonyms.json
├── relationships.json
└── glossary/
```

## 40. Curated Rule Knowledge

Curated Rule Knowledge supports provider context, user help, mechanical explanations, rule lookup, and diagnostics.

It does not replace deterministic rules.

## 41. Narrative Guidance

Narrative Guidance supports tone, themes, social context, sensory framing, role portrayal, scenario boundaries, and storyteller cautions.

It must not silently define deterministic mechanics.

## 42. Source Content Classification

Every extracted source segment receives one primary classification:

```text
MechanicalDefinition
MechanicalConstraint
MechanicalOperation
MechanicalResolutionRule
CharacterFieldDefinition
CharacterCreationRule
ProgressionRule
ResourceRule
Terminology
NarrativeGuidance
SettingKnowledge
Example
EditorialCommentary
Fluff
Ambiguous
Unsupported
Deferred
```

## 43. Source Segment

A `SourceSegment` is the minimum traceable extraction unit.

## 44. Source Segment Shape

```yaml
sourceSegmentId: source.werewolf3e.character-creation.races.initial-gnosis
sourceDocumentId: source.werewolf3e.cleaned-ptbr
sourceDocumentVersion: 1
locator:
  headingPath:
    - SISTEMA DE CRIAÇÃO DE PERSONAGEM GAROU
    - RAÇAS GAROU
  lineRange:
    start: null
    end: null
  page:
    start: null
    end: null
contentFingerprint: sha256:<value>
language: pt-BR
classification: CharacterCreationRule
secondaryTags:
  - race
  - gnosis
sourceTextRetentionPolicy: restricted-reference
```

## 45. Locator Policy

Use the strongest available locator:

1. original edition page and section;
2. cleaned-document heading path and line range;
3. content fingerprint;
4. extraction batch and segment order.

Missing original pagination must be recorded, not invented.

## 46. Provenance

Every package artifact must trace to one or more source segments, Chronicle-authored decisions, normalization decisions, tests, and reviewer approvals.

## 47. Extraction Confidence

Allowed values:

```text
High
Medium
Low
Unknown
```

Confidence measures extraction clarity, not legal permission or correctness.

## 48. Review Dimensions

Every candidate may require:

```text
Extraction Review
Semantic Review
Implementation Review
Test Review
Localization Review
Provenance Review
Legal and Redistribution Review
```

## 49. Normalization Policy

The default policy is:

```text
FidelityWithNormalization
```

This means:

- preserve mechanical behavior;
- reorganize into explicit contracts;
- separate general rule from exceptions;
- normalize names to canonical keys;
- record editorial choices;
- mark ambiguities;
- never silently simplify.

## 50. Forbidden Extraction Behavior

Do not:

- invent missing numbers;
- reconcile conflicting rules silently;
- convert examples into universal rules automatically;
- treat narrative descriptions as mandatory mechanics;
- copy long protected text into distributed artifacts without authorization;
- mark uncertain content as approved;
- flatten exceptions into a misleading simple rule;
- use provider output as the only review.

## 51. Ambiguity Record

```yaml
ambiguityId: ambiguity.<key>
sourceSegmentRefs:
  - <source>
question: <precise unresolved issue>
candidateInterpretations:
  - interpretationKey: a
    description: <summary>
  - interpretationKey: b
    description: <summary>
impact:
  artifactRefs:
    - <artifact>
  severity: high
resolutionStatus: unresolved
```

## 52. Extraction Workflow

Canonical workflow:

```text
Source Registration
    ↓
Segmentation
    ↓
Classification
    ↓
Candidate Extraction
    ↓
Reference Linking
    ↓
Provenance Creation
    ↓
Structural Validation
    ↓
Semantic Review
    ↓
Implementation Mapping
    ↓
Fixture Generation
    ↓
Approval
    ↓
Package Publication
```

## 53. Source Registration

Record source identity, edition, language, file hash, source type, cleaned or original status, known omissions, known transformations, and redistribution policy.

## 54. Structural Validation

Validate schema, IDs, references, contract versions, duplicate keys, cycles, value domains, and package boundaries.

## 55. Semantic Review

A human reviewer confirms that normalized behavior matches the available source.

## 56. Fixture Generation

Each approved deterministic rule should have a valid example, an invalid example where applicable, a boundary example, an exception example, and provenance.

## 57. Publication Gate

A package artifact is publishable only when it is schema-valid, provenance-complete, required reviews are approved, tests pass, license status is acceptable, and integrity metadata exists.

Publishable means publication may be allowed.

Publishable does not mean promoted, published, installed, enabled, or active.

Published means an exact artifact was distributed through an approved channel.

Publication MUST NOT imply installation, enablement, activation, or Campaign binding.

## 58. Validation Report

Each extraction batch produces:

```text
ValidationReport
├── source summary
├── extracted artifact count
├── classification count
├── unresolved ambiguities
├── conflicts
├── missing references
├── unsupported mechanics
├── deferred content
├── tests generated
└── publication readiness
```

## 59. Localization

All technical keys are English.

The source may be Portuguese.

The package may provide Portuguese and English display resources.

## 60. Legal and Redistribution Boundary

The extraction workflow distinguishes:

```text
Private Source Reference
Structured Mechanical Fact
Chronicle-Authored Explanation
Short Attributed Terminology
Protected Narrative Text
Redistributable Package Content
Nonredistributable Reference Content
```

## 61. Official Package Content

Official distributed packages must include only content with an approved legal and provenance status.

## 62. Package Module Interfaces

The hybrid module may implement contracts such as:

```text
IRuleSetPackage
ICharacterDefinitionProvider
ICharacterCreationHandler
IMechanicalOperationHandler
IDicePlanBuilder
IDiceResolver
IProgressionHandler
IPackageMigrationHandler
```

Exact interface names require later RFC and ADR formalization.

## 63. Capability Boundary

The module may receive only approved capabilities.

It must not receive unrestricted filesystem, network, secret store, process execution, DbContext, provider SDK, or Presentation services.

## 64. Reference Implementation Constraint

The Werewolf package must be discovered through manifests, validated through the same package loader, loaded through the same interfaces, stored outside Chronicle Core projects, absent from Core database columns, and replaceable by another package.

## 65. First Vertical Slice

The first extraction slice should cover:

```text
Character creation foundation
    Race
    Auspice
    Tribe
    Attributes
    Abilities
    Backgrounds
    initial Rage
    initial Gnosis
    initial Willpower
    initial Gifts
    freebie allocation
    one generic Dice test
```

## 66. Slice Output

The first slice should produce:

```text
manifest candidate
source inventory
content classification
race catalog
auspice catalog
tribe catalog
Character fields
creation workflow
sheet layout
resource definitions
creation operations
generic Dice operation
rule definitions
terminology
localization
provenance
fixtures
ambiguity report
contract findings report
```

## 67. Source-Derived Initial Findings

The submitted cleaned Werewolf document demonstrates that:

- Character identity is composed from Race, Auspice, and Tribe;
- Race may initialize Gnosis and impose creation restrictions;
- Auspice may initialize Rage and define role terminology;
- some options provide initial Gift catalogs;
- deformities combine selection, validation, and mechanical effects;
- rituals and Gifts may combine costs, tests, dynamic difficulties, durations, and effects;
- antagonist definitions may use baseline fields plus variant powers;
- terminology requires aliases, categories, and social context.

These are extraction findings, not yet approved package behavior.

## 68. Contract Findings Process

When the real source does not fit this specification:

1. record the mismatch;
2. do not force data into an inaccurate shape;
3. create a Contract Finding;
4. propose the smallest generic extension;
5. test it with a synthetic non-Werewolf case;
6. revise this SPEC or the relevant RFC;
7. migrate prototype artifacts.

## 69. No Premature Generalization

A contract is not expanded solely because a theoretical system might need it.

Expansion requires a real source need or an approved synthetic architecture test protecting a known future boundary.

## 70. Testing Requirements

Required test groups:

```text
Manifest Tests
Schema Tests
Reference Tests
Character Creation Tests
Sheet Rendering Tests
Operation Contract Tests
Dice Contract Tests
Progression Tests
Provenance Tests
Localization Tests
Legal Status Tests
Package Integrity Tests
Capability Boundary Tests
Cross-System Tests
```

## 71. Cross-System Test

A synthetic package must prove that Chronicle can load a non-Werewolf system with different Character fields, different Dice kinds, different resource names, a different creation workflow, and no Race/Auspice/Tribe assumptions.

## 72. Architecture Tests

Tests must reject:

- Core reference to the Werewolf package;
- hardcoded Werewolf sheet;
- Werewolf-specific persistence columns;
- package direct DbContext access;
- package secret access;
- localized technical identifiers;
- raw sourcebook installed as package authority;
- unreviewed candidate rule in an official release;
- missing provenance for deterministic rules;
- UI layout as sole mechanical validation;
- provider prose used as deterministic resolution.

## 73. Error Keys

Recommended extraction and package errors:

```text
ruleset.manifest-invalid
ruleset.contract-unsupported
ruleset.artifact-missing
ruleset.artifact-reference-invalid
ruleset.module-entrypoint-invalid
ruleset.capability-denied
ruleset.integrity-invalid
ruleset.provenance-missing
ruleset.rule-not-approved
ruleset.localization-missing
extraction.source-unregistered
extraction.segment-invalid
extraction.classification-required
extraction.ambiguity-unresolved
extraction.conflict-unresolved
extraction.semantic-review-required
extraction.legal-review-required
extraction.publication-blocked
```

## 74. Acceptance Criteria

This specification is validated when:

- a real Werewolf creation slice can be represented without Core-specific fields;
- the Chronicle renderer can build a usable sheet from declarative artifacts;
- deterministic creation validation runs through package contracts;
- one generic Dice test produces a Chronicle Dice plan;
- Chronicle generates and commits evidence;
- the package resolves it deterministically;
- provenance traces every approved artifact to source and normalization decisions;
- ambiguities remain visible;
- no raw protected source is required at runtime;
- a synthetic non-Werewolf package passes without changing Core contracts.

## 75. Next Artifacts

This specification is followed by:

```text
docs/extraction/werewolf-3e/
    EXTRACTION-0001-source-inventory.md
    EXTRACTION-0002-content-classification.md
    EXTRACTION-0003-character-creation-slice.md
    EXTRACTION-0004-ambiguities-and-conflicts.md
    EXTRACTION-0005-contract-findings.md

rule-sets/Chronicle.RuleSets.Werewolf/prototype/
    initial package artifacts
```

## 76. Final Contract

A Chronicle Rule Set package is not a book stored in a folder.

It is a reviewed, versioned, integrity-verifiable package of:

- structured definitions;
- deterministic mechanics;
- presentation hints;
- curated knowledge;
- narrative guidance;
- localization;
- provenance;
- tests.

The sourcebook informs the package.

It does not execute itself.
