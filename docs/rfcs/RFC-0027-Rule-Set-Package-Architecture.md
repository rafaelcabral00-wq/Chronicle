---
id: RFC-0027
title: Rule Set Package Architecture
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-08-03
category: Architecture
depends_on:
  - RFC-0000
  - RFC-0001
  - RFC-0002
  - RFC-0003
  - RFC-0004
  - RFC-0005
  - RFC-0006
  - RFC-0007
  - RFC-0008
  - RFC-0009
  - RFC-0010
  - RFC-0011
  - RFC-0012
  - RFC-0013
  - RFC-0014
  - RFC-0015
  - RFC-0016
  - RFC-0017
  - RFC-0018
  - RFC-0019
  - RFC-0020
  - RFC-0021
  - RFC-0022
  - RFC-0023
  - RFC-0024
  - RFC-0025
  - RFC-0026
  - RFC-0031
---

> **"A Rule Set package carries mechanics into Chronicle. It must never carry ownership of the Campaign with it."**

# Rule Set Package Architecture

## Abstract

This RFC defines the package architecture used to implement, distribute, register, validate, version, load, test, and maintain Rule Sets in Chronicle.

The Rule Set package is the concrete delivery unit behind the generic contract established by RFC-0014.

It may contain executable mechanics, declarative schemas, terminology, localization, knowledge references, generation guidance, migrations, metadata, and tests.

Chronicle Core remains independent from any one system.

The MVP may statically register one Rule Set package that is complete for its declared release scope, but the package boundary must already be explicit enough to support future systems without restructuring the Domain.

Effective 2026-08-03, DR-0002 defines complete for Rule Set package releases:

A Rule Set is complete for a declared release scope when every advertised capability, mechanic, workflow, artifact, validation, test, localization requirement, security requirement, and compatibility promise within that scope is implemented and verified.

Completeness does not require implementing the entire source RPG system.

## 1. Purpose

Chronicle needs one concrete place for system-specific behavior.

Without a package boundary, Rule Set concerns may leak into:

- generic Character entities;
- Chronicle Director logic;
- Dice application services;
- Prompt Builder templates;
- persistence schemas;
- UI components;
- Narrative Intelligence providers;
- Campaign lifecycle code.

The Rule Set package architecture prevents that leakage.

## 2. Scope

This RFC defines:

- Rule Set package identity;
- package layout;
- manifest;
- package components;
- executable versus declarative content;
- registration;
- loading;
- capability discovery;
- dependency rules;
- Character Sheet schema packaging;
- mechanics packaging;
- operation catalogs;
- progression;
- terminology;
- localization;
- knowledge references;
- generation guidance;
- migrations;
- validation;
- testing;
- licensing metadata;
- integrity;
- security;
- static and future dynamic loading.

This RFC does not define:

- one concrete programming language;
- one final folder layout;
- one packaging format;
- marketplace behavior;
- remote package distribution;
- code-signing technology;
- sandbox runtime;
- exact Werewolf implementation;
- exact legal policy.

## 3. Package Definition

A `RuleSetPackage` is the deployable implementation unit for one Rule Set identity and version.

Conceptually:

```text
RuleSetPackage
├── Manifest
├── Domain Definitions
├── Character Sheet Schemas
├── Validators
├── Operation Catalog
├── Mechanical Resolvers
├── Progression
├── State Extensions
├── Terminology
├── Localization
├── Rule Knowledge References
├── Narrative Guidance
├── Migrations
└── Tests
```

Not every optional component must exist.

## 4. Core Boundary

The package owns system-specific mechanics.

Chronicle owns:

- Campaign;
- Session;
- Act;
- Scene;
- persistent identity;
- Dice random generation;
- transaction boundaries;
- provider orchestration;
- Campaign Memory lifecycle;
- application use cases;
- persistence ownership;
- visibility enforcement.

The package MUST NOT redefine these responsibilities.

## 5. Package Identity

Every package MUST identify:

- Rule Set identifier;
- Rule Set version;
- package version;
- edition;
- contract version;
- package name;
- publisher or maintainer;
- status.

The Rule Set version identifies mechanics.

The package version identifies the delivery artifact.

They MAY be the same but MUST remain conceptually distinct.

## 5.1 Materialization Roles

Effective 2026-08-03, DR-0004 and SPEC-0001 define the normative materialization roles for Rule Set packages:

- documentation prototype;
- package source;
- packaged artifact;
- installed artifact.

The documentation prototype is review and authoring evidence only.

It is never executable, packaging, installation, activation, Campaign-binding, or publication authority merely because it exists in the repository.

Progression between roles requires explicit transformation, identity preservation, fingerprints, validation, reconciliation, and accepted evidence as defined by SPEC-0001.

This RFC does not treat the Werewolf documentation prototype path as the generic package source layout and does not create runtime implementation details, package artifacts, installation records, enablement records, activation records, publication records, or Campaign bindings.

## 6. Package Manifest

Every package MUST include a manifest.

A manifest SHOULD contain:

```text
RuleSetId
RuleSetVersion
PackageVersion
DisplayName
Edition
Description
Status
Capabilities
SupportedLocales
ChronicleContractRange
SchemaVersions
OperationCatalogVersion
MigrationEntries
LicenseMetadata
ContentProvenance
IntegrityMetadata
EntryPoints
TestMetadata
```

The exact serialized format requires an ADR.

## 7. Manifest Validation

Before registration, Chronicle MUST validate:

- required fields;
- stable identifiers;
- version syntax;
- capability declarations;
- compatible Chronicle contract;
- supported schema versions;
- package integrity;
- licensing metadata;
- declared entry points;
- migration references;
- locale references.

An invalid package MUST NOT become selectable.

## 8. Package Status

Package status is not a single value.

Chronicle tracks four separate status dimensions:

- package maturity;
- publication status;
- installation status;
- runtime activation status.

These dimensions MUST NOT be collapsed into one display string in package metadata, README content, public status claims, UI capability discovery, or user-facing documentation.

## 9. Rule Set Lifecycle

Effective 2026-08-03, DR-0003 defines the shared normative Rule Set lifecycle.

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

Package maturity covers `proposed` through `promoted`.

Publication status covers whether a promoted artifact is unpublished, published, deprecated, or withdrawn.

Installation status covers whether a published or locally provided artifact is installed in a Chronicle environment.

Runtime activation status covers whether an installed package is enabled and whether a Campaign is actively bound to it.

Publication MUST NOT imply installation, enablement, or activation.

Promotion and publication are separate:

- promotion means the package satisfies the declared quality gate for a release scope;
- publication means an artifact was distributed through an approved channel;
- a package may be `promotion-eligible` or `promoted` without being `published`;
- publication never implies `installed`, `enabled`, or `active`.

## 10. Lifecycle Transitions

Allowed forward transitions:

| From | To | Required evidence | Owner | Reversible |
| --- | --- | --- | --- | --- |
| proposed | source-registered | source identity, ownership/provenance notes, initial scope intent | Rule Set author or maintainer | yes |
| source-registered | slice-defined | declared release scope, supported capability intent, exclusions, disabled operations | Rule Set maintainer with Chronicle architecture review for official packages | yes |
| slice-defined | extracted | extraction records, source fingerprints, classification records | Extraction workflow and reviewer | yes, by superseding extraction |
| extracted | modeled | package artifacts or models, stable keys, declared artifact families | Rule Set authoring workflow | yes, by replacing models |
| modeled | structurally-valid | schema validation, reference resolution, dependency checks, artifact uniqueness | Validation tooling | yes, if inputs change |
| structurally-valid | substantively-reviewed | semantic/source review records, ambiguity disposition, legal/provenance review where required | Human reviewer or delegated review process | yes, if evidence becomes stale |
| substantively-reviewed | decision-set-finalized | accepted decision records for scope, exclusions, blockers, and source ambiguities | Rule Set maintainer or formal review authority | yes, by superseding decisions |
| decision-set-finalized | evidence-complete | required tests, localization checks, security checks, compatibility checks, reconciliation, and freshness evidence | Validation and release tooling | yes, if evidence becomes stale |
| evidence-complete | promotion-eligible | declared quality gate passes for the release scope with no blocking issue | Promotion gate evaluator | yes, if gate inputs change |
| promotion-eligible | promoted | promotion record binding exact artifacts, fingerprints, scope, and evidence | Release authority | no, must supersede or withdraw |
| promoted | published | approved distribution channel record and publication metadata | Release authority | no, must withdraw/deprecate |
| published | installed | installation manifest, integrity verification, compatibility check | Installer or package manager | yes, by uninstalling when no active Campaign requires it |
| installed | enabled | user/admin enablement or policy enablement, compatibility still valid | User/admin or application policy | yes |
| enabled | active | Campaign binding to exact package identity and version | Application runtime through Campaign creation or migration flow | no silent reversal; Campaign migration required |
| active | deprecated | deprecation notice, replacement guidance, compatibility notes | Maintainer/release authority | no, but replacement may be published |
| published, installed, enabled, deprecated | withdrawn | withdrawal reason, safety/legal notice, replacement or preservation policy | Release authority | no, must issue new release |

Blocking conditions include:

- missing source registration or fingerprint;
- undefined release scope;
- advertised but unimplemented capability;
- unresolved source ambiguity inside advertised scope;
- failed structural validation;
- missing substantive review;
- incomplete decision set;
- stale evidence;
- failing required tests;
- missing localization validation for advertised locales;
- missing security validation;
- unresolved blocker or active critical issue;
- incompatible Chronicle contract version;
- missing publication metadata for publication;
- failed integrity check during installation;
- package withdrawn for legal, safety, or provenance reasons.

## 11. Status Effects

Status affects application presentation and availability.

Status MUST NOT alter mechanical behavior silently.

An active Campaign MUST remain bound to the exact package identity and version unless an explicit migration or compatibility flow changes that binding.

## 12. Capability Declaration

The manifest MUST declare supported capabilities.

Every Rule Set release MUST also declare:

- supported scope;
- supported capabilities;
- excluded mechanics;
- disabled operations;
- known limitations;
- compatibility boundaries;
- evidence and validation status.

No unavailable feature may be presented as supported in package metadata, README content, public status claims, UI capability discovery, or user-facing documentation.

Initial capabilities MAY include:

```text
CharacterSheetSchema
CharacterValidation
DiceTests
DeterministicTests
Progression
CharacterStateExtensions
RelationshipMechanics
KnowledgeMechanics
CampaignPreferences
Terminology
Localization
RuleKnowledge
CampaignGenerationGuidance
NpcGenerationGuidance
Migration
```

Chronicle MUST not assume undeclared capabilities.

## 10. Entry Points

The package manifest SHOULD define logical entry points.

Examples:

```text
DescriptorProvider
CharacterSchemaProvider
CharacterValidator
OperationCatalogProvider
TestResolver
ProgressionProvider
PreferenceValidator
TerminologyProvider
KnowledgeReferenceProvider
MigrationProvider
```

Entry points are application contracts.

They are not arbitrary executable file names at the Domain boundary.

## 11. Package Composition

A package MAY be composed from several internal modules.

Example:

```text
werewolf.package
├── manifest
├── schemas
├── mechanics
├── progression
├── terminology
├── localization
├── knowledge
├── guidance
├── migrations
└── tests
```

The exact physical organization is implementation-specific.

Logical separation is mandatory.

## 12. Executable and Declarative Content

A package MAY contain:

```text
Declarative Content
Executable Content
Hybrid Content
```

### Declarative Content

Examples:

- Character Sheet schemas;
- operation definitions;
- allowed choices;
- terminology;
- localization;
- validation constraints;
- preference definitions.

### Executable Content

Examples:

- dice pool calculation;
- result interpretation;
- progression calculation;
- migration transformation.

### Hybrid Content

Declarative definitions interpreted by generic executable engines.

## 13. Declarative Preference

Chronicle SHOULD prefer declarative content where behavior remains clear, deterministic, and testable.

Benefits include:

- easier validation;
- easier localization;
- easier inspection;
- safer future package loading;
- fewer arbitrary execution risks.

Declarative representation MUST NOT be forced where it obscures complex mechanics.

## 14. Executable Mechanics

Executable package code MUST:

- implement explicit Rule Set contracts;
- remain deterministic where required;
- avoid persistence access;
- avoid provider access;
- avoid network access;
- avoid UI dependencies;
- avoid system clock access except through supplied context;
- avoid random generation;
- return structured results;
- be testable in isolation.

## 15. Randomness Boundary

The package interprets randomness.

Chronicle generates randomness.

```text
Chronicle:
Generate raw dice

Rule Set Package:
Interpret raw dice
```

The package MUST NOT call an independent random source for authoritative results.

## 16. Persistence Boundary

A Rule Set package MUST NOT:

- open the database;
- persist Characters;
- save Dice Rolls;
- mutate Campaign records;
- create repository transactions;
- write provider files;
- access local Campaign files directly.

It returns values to Chronicle.

Chronicle applies them through application use cases.

## 17. Provider Boundary

A Rule Set package MUST NOT depend on:

- Narrative Intelligence provider SDKs;
- prompt templates;
- model names;
- provider threads;
- embeddings;
- vector database clients.

It MAY provide provider-neutral narrative guidance and knowledge references.

## 18. UI Boundary

A Rule Set package MAY provide presentation metadata.

Examples:

- section ordering;
- field labels;
- formatting hints;
- grouping;
- recommended controls;
- icons by semantic key.

It MUST NOT provide concrete application components as part of the generic contract.

## 19. Character Sheet Schema Package

Character Sheet schemas SHOULD be versioned independently.

A schema package SHOULD define:

- schema identifier;
- schema version;
- Character role;
- sections;
- fields;
- stable keys;
- data types;
- required values;
- defaults;
- ranges;
- choices;
- derived fields;
- editability;
- visibility;
- localization keys;
- progression metadata.

## 20. Supported Field Types

The initial generic field system SHOULD support a bounded set of types.

Possible types:

```text
Integer
Decimal
Boolean
Text
LongText
Choice
MultiChoice
Reference
Resource
Track
Composite
```

The exact MVP list requires implementation evidence.

## 21. Custom Field Types

A Rule Set SHOULD not introduce arbitrary UI or storage field types.

Future custom types require:

- generic serialization;
- validation;
- migration;
- presentation fallback;
- compatibility declaration.

## 22. Schema Versioning

Breaking schema changes require a new schema version.

A package MUST declare which Rule Set versions use which Character schema versions.

Historical Characters retain the version required for interpretation.

## 23. Character Validator Package

Character validation MAY combine:

- generic schema validation;
- declarative constraints;
- executable cross-field rules;
- role-specific rules;
- Campaign Preference rules.

Validation MUST return structured:

- errors;
- warnings;
- field references;
- error codes;
- override policy;
- normalization results.

## 24. Operation Catalog Package

The operation catalog defines stable mechanical operations.

Each operation SHOULD include:

- operation key;
- operation version;
- display name key;
- description key;
- actor requirements;
- target requirements;
- input fields;
- pool rule reference;
- difficulty rule;
- modifier policy;
- result resolver reference;
- generic outcome mapping;
- visibility;
- guidance references.

## 25. Operation Catalog Stability

Operation keys MUST remain stable across compatible package updates.

Breaking changes require:

- alias;
- migration;
- major version;
- or explicit deprecation.

Historical Dice Rolls preserve original keys and versions.

## 26. Mechanical Resolver Package

A mechanical resolver SHOULD receive only structured input.

Example input:

```text
RuleSetVersion
OperationKey
ActorSnapshot
TargetSnapshot
SceneConditions
CampaignPreferences
Modifiers
Difficulty
RawDice
```

It SHOULD return:

```text
ValidatedPool
Resolution
GenericOutcome
MechanicalConsequences
NarrativeGuidance
Warnings
CalculationVersion
```

## 27. Pool Calculation

Pool calculation MUST be:

- deterministic;
- versioned;
- explainable;
- independent from provider output;
- independent from persistence;
- validated against declared modifiers.

The result SHOULD expose a breakdown.

## 28. Result Resolution

Result resolution MUST preserve:

- raw dice;
- operation version;
- calculation version;
- Rule Set version;
- outcome details;
- critical conditions;
- consequence categories.

It MUST not mutate Character State directly.

## 29. Mechanical Consequences

A resolver MAY return structured consequence proposals.

Examples:

- resource loss;
- temporary condition;
- wound;
- progress;
- opposition advantage.

Chronicle validates and applies consequences through application workflows.

## 30. Deterministic Operations

The package MAY define operations without randomness.

These SHOULD use the same operation catalog when practical.

A deterministic result MUST not create fake dice.

## 31. Progression Package

A progression component SHOULD define:

- progression currencies;
- award criteria;
- spending rules;
- prerequisites;
- field costs;
- maximums;
- milestone logic;
- validation;
- result structure.

The Archivist provides evidence.

The package validates or calculates mechanics.

## 32. Character State Extensions

The package MAY define typed state extensions.

Examples:

- health track;
- temporary form;
- supernatural resource;
- status;
- morality;
- corruption;
- turn effect.

Each extension SHOULD define:

- stable key;
- type;
- default;
- range;
- duration model;
- stacking;
- visibility;
- validation;
- localization.

## 33. Relationship Extensions

The package MAY define:

- additional Relationship dimensions;
- valid ranges;
- labels;
- mechanical modifiers;
- visibility rules;
- change constraints.

Generic source-to-target directionality remains Chronicle-owned.

## 34. Knowledge Extensions

The package MAY define system-specific Knowledge behavior.

Examples:

- supernatural concealment;
- memory alteration;
- detection difficulty;
- false perception;
- forced uncertainty.

Generic Knowledge states remain Chronicle-owned.

## 35. Campaign Preference Definitions

The package MAY expose supported Campaign Preferences.

Each definition SHOULD contain:

- stable key;
- value type;
- default;
- allowed values;
- mechanical impact;
- narrative impact;
- compatibility;
- localization;
- migration behavior.

## 36. House Rules

House rules MUST be declared extension points.

A package MUST NOT accept arbitrary rule-changing blobs.

A house-rule option SHOULD be:

- versioned;
- validated;
- explicit;
- persisted;
- included in pool and result calculation;
- included in historical interpretation.

## 37. Terminology Package

Terminology SHOULD use stable semantic keys.

Examples:

```text
character.attribute.strength
result.critical_success
resource.willpower
```

Localized labels map to these keys.

Mechanics MUST not depend on translated text.

## 38. Localization Package

A localization component SHOULD provide:

- package display name;
- Character Sheet labels;
- validation messages;
- operation labels;
- result descriptions;
- terminology;
- preference labels;
- generation guidance labels.

Missing locale behavior MUST be explicit.

## 39. Localization Fallback

A package SHOULD declare a default locale.

When a requested locale is unavailable, Chronicle MAY:

- use the default locale;
- use a generic Chronicle label;
- block only the affected presentation;
- warn the user.

Mechanical execution MUST not depend on localized text.

## 40. Rule Knowledge References

A package MAY provide references used by Rule Knowledge infrastructure.

These MAY include:

- topic keys;
- operation keys;
- source identifiers;
- section references;
- licensing metadata;
- short original summaries;
- index build instructions;
- user-supplied source requirements.

The package MUST NOT require one retrieval technology.

## 41. Knowledge Assets

Knowledge assets MAY be:

```text
Bundled
UserSupplied
ExternallyConfigured
GeneratedIndex
```

The manifest MUST identify provenance and distribution rights.

## 42. Proprietary Content

A package MUST NOT bundle proprietary sourcebook text without authorization.

Package code, original summaries, schemas, and references SHOULD remain separated from restricted content.

A package MAY require the player to configure legally obtained material.

## 43. Campaign Generation Guidance

A package MAY provide structured generation guidance.

It SHOULD include:

- setting principles;
- Character relevance guidance;
- valid conflict types;
- NPC creation rules;
- power expectations;
- terminology;
- required concepts;
- prohibited inventions;
- thematic suggestions.

Guidance is reference content.

It does not become Campaign truth.

## 44. NPC Generation Guidance

NPC guidance SHOULD define:

- required Character fields;
- valid archetypes;
- power classification;
- mechanical completeness;
- persistent versus transient guidance;
- common state defaults;
- terminology;
- validation behavior.

## 45. Narration Guidance

A package MAY provide concise narrative guidance for:

- Test opportunities;
- result interpretation;
- system tone;
- canonical terminology;
- visibility of mechanics;
- common consequences.

The Narrator remains provider-neutral.

## 46. Guidance Versioning

Narrative guidance SHOULD be versioned.

A guidance update MAY change generated prose without changing mechanics.

The manifest SHOULD distinguish mechanical and guidance revisions when useful.

## 47. Migration Package

A package MAY include migrations.

Each migration SHOULD declare:

- source Rule Set version;
- target Rule Set version;
- source schema versions;
- target schema versions;
- migration identifier;
- compatibility type;
- transformed entities;
- preconditions;
- validation;
- warnings;
- backup requirement.

## 48. Migration Execution

Migrations MUST:

- be deterministic;
- operate on structured data;
- preserve original data;
- avoid provider calls;
- avoid randomness;
- return explicit warnings;
- validate output;
- preserve historical Roll interpretation.

## 49. Migration Chain

Chronicle MAY apply a sequence of migrations.

The package MUST declare a valid path.

Ambiguous or cyclic migration paths MUST be rejected.

## 50. Migration Absence

If no compatible migration exists:

- Campaign upgrade is blocked;
- the old package version remains required;
- historical views may remain available;
- the user receives explicit options.

Chronicle MUST not guess a migration.

## 51. Package Dependencies

A Rule Set package SHOULD minimize dependencies.

Allowed dependencies MAY include:

- Chronicle Rule Set contract;
- approved generic validation utilities;
- approved deterministic calculation utilities.

A package MUST NOT depend on another full Rule Set package unless a future extension model explicitly supports it.

## 52. Shared Libraries

Shared mechanical libraries MAY be introduced when multiple Rule Sets need the same behavior.

They MUST remain:

- generic;
- versioned;
- deterministic;
- provider-independent;
- persistence-independent.

A shared library MUST not become a hidden system-specific dependency in Chronicle Core.

## 53. Package Isolation

A package SHOULD be isolated so that:

- it can be tested without the official UI;
- it can be registered in a test host;
- its mechanics can run without a provider;
- its schemas can be validated independently;
- its failures do not corrupt another Rule Set.

## 54. Static Registration

The MVP MAY statically register the initial Rule Set package.

Static registration means:

- package code is compiled or bundled with the application;
- package discovery is explicit;
- no untrusted dynamic loading occurs;
- package boundaries remain logical and testable.

## 55. Static Registration Requirements

Even when statically registered:

- Chronicle Core MUST depend on Rule Set contracts;
- the package MUST have a manifest;
- capabilities MUST be discovered through a descriptor;
- Werewolf-specific concepts MUST not appear in generic application services;
- contract tests MUST run.

## 56. Dynamic Loading Horizon

Future dynamic loading may support:

- external packages;
- community Rule Sets;
- local package installation;
- package updates;
- independent release cycles.

It requires:

- trust policy;
- code signing;
- sandboxing;
- permission declarations;
- integrity verification;
- compatibility negotiation;
- user approval.

It is outside the MVP.

## 57. Package Registry

Chronicle SHOULD expose a `RuleSetRegistry`.

The registry SHOULD support:

- register package;
- list descriptors;
- resolve exact Rule Set version;
- resolve capability;
- validate compatibility;
- detect duplicate identity;
- report unavailable version.

## 58. Exact Version Resolution

Campaign play MUST resolve the exact stored Rule Set version.

The registry MUST NOT silently return the latest version.

Fallback to another version requires explicit compatibility and migration policy.

## 59. Duplicate Package Identity

Two packages claiming the same:

```text
RuleSetId + RuleSetVersion
```

MUST cause a conflict unless they are byte-identical or explicitly treated as the same package artifact.

The application MUST not choose arbitrarily.

## 60. Package Availability

The registry SHOULD report:

```text
Available
Deprecated
Incompatible
Missing
Invalid
Disabled
```

Campaign compatibility depends on this result.

## 61. Package Integrity

A package SHOULD support integrity verification.

Integrity metadata MAY include:

- package checksum;
- component checksums;
- package build identifier;
- source revision;
- signature metadata.

The exact technology requires an ADR.

## 62. Package Tampering

If integrity verification fails:

- package registration fails;
- affected Campaign play is blocked;
- safe historical data may remain visible;
- diagnostics are recorded;
- Chronicle MUST not execute the package.

## 63. Security Model

Rule Set packages are trusted application components in the MVP.

Even trusted packages MUST obey architectural restrictions.

Future external packages MUST be treated as untrusted until verified and sandboxed.

## 64. Prohibited Package Capabilities

A package MUST NOT receive unrestricted access to:

- filesystem;
- network;
- operating-system commands;
- provider credentials;
- Campaign repositories;
- other Campaigns;
- UI process;
- arbitrary reflection or dynamic execution.

The MVP's compiled package may technically run in-process, but contract and code review must preserve these restrictions.

## 65. Resource Limits

Future package execution MAY require limits for:

- calculation time;
- memory;
- recursion;
- payload size;
- migration duration.

The MVP SHOULD still avoid unbounded algorithms.

## 66. Package Configuration

Package configuration SHOULD be separated into:

```text
Package Metadata
Application Configuration
Campaign Preferences
User-Supplied Knowledge Configuration
```

These have different lifecycles and ownership.

## 67. Application Configuration

Application configuration MAY include:

- enabled packages;
- knowledge source paths;
- diagnostics;
- default locale;
- developer mode.

It MUST not become Campaign mechanical state unless explicitly persisted as a Campaign Preference.

## 68. Development Package

A development package MAY expose additional diagnostics.

Examples:

- schema dump;
- operation catalog inspection;
- calculation traces;
- fixture runner;
- migration dry run.

Developer diagnostics MUST not alter production mechanics.

## 69. Package Test Suite

Every supported package SHOULD include:

- manifest tests;
- schema tests;
- Character validation tests;
- operation catalog tests;
- pool calculation tests;
- result resolution tests;
- progression tests;
- preference tests;
- migration tests;
- localization tests;
- knowledge reference tests;
- contract tests;
- golden fixtures.

## 70. Golden Mechanical Fixtures

A golden fixture SHOULD contain:

```text
Given:
    Rule Set version
    Character snapshot
    Operation
    Modifiers
    Difficulty
    Raw dice

Expect:
    Pool breakdown
    Outcome
    Consequences
    Warnings
```

Golden fixtures protect deterministic mechanics.

## 71. Schema Fixtures

Schema fixtures SHOULD test:

- valid Player Character;
- invalid Player Character;
- valid NPC;
- invalid NPC;
- minimum and maximum values;
- cross-field constraints;
- derived fields;
- localization keys.

## 72. Migration Fixtures

Migration fixtures SHOULD include:

- representative old Character data;
- Campaign Preferences;
- Character State;
- operation aliases;
- unknown values;
- rollback or preserved source data.

## 73. Contract Test Host

Chronicle SHOULD provide a shared Rule Set contract test host.

A package passes only when it satisfies:

- descriptor contract;
- capability declarations;
- deterministic behavior;
- error model;
- no provider dependency;
- no persistence dependency;
- version behavior;
- safe registration.

## 74. Compliance Report

Package validation SHOULD produce a report containing:

- manifest result;
- contract compatibility;
- capabilities;
- schema result;
- operation catalog result;
- migration result;
- localization result;
- licensing warnings;
- integrity result;
- test summary.

## 75. Package Build

A package build SHOULD be reproducible where practical.

Build metadata MAY include:

- source revision;
- build timestamp;
- compiler or runtime version;
- package checksum;
- test result.

## 76. Package Release

A supported package release SHOULD require:

- manifest validation;
- contract tests;
- mechanical regression tests;
- migration tests when applicable;
- licensing review;
- Narrative Intelligence guidance evaluation;
- compatibility report.

Release does not mean publication unless an approved distribution channel actually receives the artifact.

A package release MAY be:

- promotion-eligible but not promoted;
- promoted but not published;
- published but not installed;
- installed but not enabled;
- enabled but not active in any Campaign.

## 77. Package Deprecation

A package version MAY be deprecated.

Deprecation SHOULD provide:

- reason;
- replacement version;
- migration availability;
- support timeline;
- compatibility notes.

Existing Campaigns MUST not be silently upgraded.

## 78. Package Removal

Removing a package version may make Campaigns unplayable.

The application SHOULD avoid removal while local Campaigns depend on it unless:

- the user explicitly accepts;
- migration succeeds;
- backup exists;
- safe archive behavior is available.

## 79. Version Compatibility

A package MUST declare compatible Chronicle contract versions.

Chronicle SHOULD reject:

- too-old contract;
- too-new contract;
- missing required capability;
- unsupported schema;
- incompatible event extension.

## 80. Rule Set Event Extensions

A package MAY eventually define Rule Set-specific Narrative Event payloads.

The MVP SHOULD prefer generic event types plus Rule Set keys.

Future extension requires:

- namespacing;
- schema registration;
- capability permission;
- validator;
- application router;
- compatibility policy.

## 81. Namespacing

Rule Set-specific keys SHOULD be namespaced.

Examples:

```text
werewolf.resource.rage
werewolf.operation.frenzy_resist
```

Namespacing prevents collisions.

Generic Chronicle keys remain unprefixed or use a Chronicle namespace.

## 82. Stable Machine Keys

All package-facing machine keys MUST be:

- stable;
- language-neutral;
- documented;
- version-aware;
- unique within namespace.

Display labels are separate.

## 83. Error Model

Package operations SHOULD return errors compatible with RFC-0018.

Recommended package-specific codes include:

```text
PackageInvalid
CapabilityUnavailable
SchemaUnsupported
OperationUnsupported
CharacterInvalid
ModifierInvalid
DifficultyInvalid
ResolutionFailed
MigrationUnavailable
MigrationFailed
KnowledgeSourceMissing
LocaleUnavailable
IntegrityFailed
```

## 84. Package Failure Isolation

If one optional package capability fails:

- unrelated capabilities MAY remain usable;
- the registry reports degraded state;
- normal play blocks only when the failed capability is required;
- Campaign truth remains unchanged.

## 85. Knowledge Source Missing

If user-supplied knowledge is missing:

- deterministic mechanics MAY remain available;
- rule explanation may be degraded;
- generation or narration requiring the source may block;
- the application offers configuration repair.

## 86. Observability

Chronicle SHOULD record:

- Rule Set identity;
- Rule Set version;
- package version;
- capability invoked;
- operation key;
- calculation version;
- schema version;
- migration identifier;
- package latency;
- package error;
- integrity status.

Logs MUST not include restricted source text unnecessarily.

## 87. Diagnostics

Developer diagnostics MAY expose:

- descriptor;
- capability matrix;
- schema list;
- operation catalog;
- preference definitions;
- migration paths;
- localization coverage;
- knowledge source status;
- test status.

## 88. Privacy

Rule Set packages SHOULD not collect or transmit Campaign data.

A package that requires external services would need a separate explicit architecture and privacy review.

The MVP MUST not allow this.

## 89. Licensing Metadata

The manifest SHOULD distinguish:

```text
CodeLicense
DataLicense
DocumentationLicense
KnowledgeContentLicense
TrademarkNotice
AttributionRequirements
DistributionRestrictions
```

## 90. Content Provenance

Every bundled content asset SHOULD declare provenance.

Possible values:

```text
Original
OpenLicensed
PublicDomain
UserSupplied
ProprietaryLicensed
ExternalReference
GeneratedIndex
```

## 91. Legal Boundary

Package architecture cannot decide legal permission by itself.

It MUST provide enough metadata and separation to support review.

The open-source repository MUST not include restricted material merely because the code can reference it.

## 92. Initial Werewolf Package

The first development package SHOULD prove:

- generic Character Sheet schema;
- Character validation;
- one complete operation path;
- Chronicle-generated Dice Rolls;
- Rule Set result interpretation;
- progression contract;
- terminology;
- Campaign Preferences;
- generation guidance;
- rule knowledge references;
- contract tests.

It MUST avoid embedding unauthorized sourcebook text.

## 93. MVP Package Scope

The MVP SHOULD deliver one Rule Set package complete for its declared release scope, not several incomplete packages.

Completeness means the package can support the end-to-end declared MVP release scope.

It does not mean implementing every rule in every sourcebook or the entire source RPG system.

The declared MVP release scope MUST be internally complete:

- advertised capabilities are implemented and verified;
- advertised mechanics and workflows are implemented and verified;
- required artifacts are present and validated;
- required tests pass;
- required localization checks pass;
- required security checks pass;
- compatibility promises are verified;
- exclusions and disabled operations are explicit and enforceable;
- package metadata and user-facing documentation match actual behavior.

Publication status remains separate from declared-scope completeness.

## 94. Package Completeness Criteria

The initial package is complete when it supports:

- valid Player Character creation;
- valid NPC creation;
- required Campaign generation;
- all MVP Dice operations;
- Character State required by those operations;
- progression required by finalization;
- required terminology;
- required knowledge retrieval;
- required Campaign Preferences;
- migration behavior appropriate to first release;
- automated contract and regression tests.

## 95. Prohibited Patterns

### 95.1 Rule Set Logic in Chronicle Core

System-specific mechanics MUST not appear in generic modules.

### 95.2 Package Accesses Persistence

The package MUST not save or load Campaign entities directly.

### 95.3 Package Generates Randomness

Authoritative randomness remains Chronicle-owned.

### 95.4 Provider Dependency

The package MUST not depend on a Narrative Intelligence provider.

### 95.5 Localized Keys as Identity

Translated text MUST not serve as machine identity.

### 95.6 Arbitrary House-Rule Blob

Mechanical customization MUST use declared preferences.

### 95.7 Silent Latest-Version Resolution

Campaigns MUST resolve the exact stored version.

### 95.8 Proprietary Content Mixed with Code

Restricted knowledge assets MUST remain separated and licensed.

### 95.9 Dynamic Loading Without Trust Model

External executable packages MUST not be loaded before signing, sandboxing, and permission policies exist.

### 95.10 Several Incomplete Rule Sets

The MVP MUST not trade one complete system for many shallow implementations.

## 96. Current Delivery Decision

The MVP adopts:

- one explicit Rule Set package architecture;
- one statically registered initial package;
- manifest and capability declaration;
- declarative schemas;
- deterministic executable mechanics where required;
- stable operation keys;
- namespaced Rule Set-specific keys;
- provider-neutral knowledge references;
- terminology and localization assets;
- Campaign generation guidance;
- progression implementation;
- package contract tests;
- integrity metadata as an architectural requirement;
- no external dynamic loading;
- no marketplace;
- no arbitrary package network or persistence access;
- no bundled proprietary sourcebook text without authorization.

## 97. Architecture Horizon

Future evolution MAY include:

- dynamically installed Rule Sets;
- community packages;
- signed packages;
- sandboxed execution;
- package marketplace;
- remote package registry;
- independent package updates;
- homebrew schema editors;
- data-only Rule Sets;
- hosted rule services;
- Rule Set-specific event extensions;
- package permission manifests.

The MVP MUST NOT implement these capabilities without a later milestone.

## 98. Open Questions

The following remain open:

- What concrete package format will be used?
- Will Rule Set packages be separate modules, assemblies, or source packages?
- Which mechanics should be declarative versus executable?
- What field types are required for the first Character schema?
- How should package versions relate to Rule Set editions?
- What exact namespace policy will be used?
- How will package integrity be calculated?
- Should localization live inside the package or a shared localization system?
- How will user-supplied rule knowledge be configured?
- Which migration capabilities are required before version 1.0?
- Should operation catalogs be data files or code definitions?
- How will contract test fixtures be distributed?
- Which diagnostics should be available in production?
- What legal metadata is mandatory?
- What minimum package completeness test blocks release?

These questions require RFC-0028 through RFC-0032 and technology ADRs.

## 99. Compliance Checklist

A package architecture complies when:

- Rule Set identity and package identity are explicit;
- a manifest exists;
- capabilities are declared;
- Chronicle Core remains system-neutral;
- Character schemas are versioned;
- operation keys are stable;
- mechanics are deterministic where required;
- randomness remains Chronicle-owned;
- persistence remains Chronicle-owned;
- provider dependencies remain outside the package;
- localization does not alter machine identity;
- knowledge assets declare provenance;
- migrations are explicit;
- exact package versions are resolved;
- contract tests exist;
- proprietary content remains legally separated;
- dynamic loading is not introduced without a trust model.

## 100. Final Principle

A Rule Set package should be powerful enough to define a game's mechanics.

It should remain constrained enough that Chronicle can replace, validate, version, and protect it without surrendering the Campaign.
