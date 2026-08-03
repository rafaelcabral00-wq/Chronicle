---
id: ADR-0030
title: Character Sheet Schema, Field Storage, and Derived Value Evaluation
status: Proposed
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-08-01
category: Technology
supersedes: []
superseded_by: null
depends_on:
  - ADR-0001
  - ADR-0002
  - ADR-0004
  - ADR-0005
  - ADR-0009
  - ADR-0010
  - ADR-0017
  - ADR-0018
  - ADR-0019
  - ADR-0024
  - ADR-0028
  - ADR-0029
  - RFC-0003
  - RFC-0004
  - RFC-0005
  - RFC-0006
  - RFC-0007
  - RFC-0008
  - RFC-0010
  - RFC-0013
  - RFC-0015
  - RFC-0021
  - RFC-0022
  - RFC-0023
  - RFC-0024
  - RFC-0025
  - RFC-0026
  - RFC-0027
  - RFC-0028
  - RFC-0029
  - RFC-0030
  - RFC-0031
  - RFC-0032
  - RFC-0040
  - RFC-0042
---

> **"A Character Sheet is not a form. It is a versioned mechanical model of who a Character is, what they know, and what the rules allow them to become."**

# Character Sheet Schema, Field Storage, and Derived Value Evaluation

## 1. Status

**Proposed**

This ADR defines Chronicle's Character Sheet schema language, field identity, storage model, validation, derived-value evaluation, migration, and Rule Set integration.

The decision is:

- define Character Sheets through versioned Rule Set package schemas;
- use stable semantic field keys independent from localized labels;
- separate core Character identity and narrative data from package-defined mechanical fields;
- store package-defined field values in a bounded versioned field-value model;
- keep frequently queried identity, lifecycle, and authority fields relational;
- prohibit arbitrary executable expressions in Character Sheet schemas for the MVP;
- calculate derived values through approved deterministic Rule Operations;
- track source schema identity and version on every Character;
- validate all Character mutations against the exact resolved schema version;
- preserve unknown or retired field values through explicit migration or archival policy;
- distinguish base, current, temporary, maximum, derived, and historical values;
- support typed fields rather than one unstructured property bag;
- keep field visibility and editability separate from mechanical authority;
- ensure Narrative Intelligence cannot directly alter Character fields;
- apply changes only through Application commands and Rule Set validation;
- preserve an auditable change history for authoritative mechanical values;
- detect circular derivation dependencies during package validation;
- support deterministic schema migration between package versions;
- never silently reinterpret a field because its label or display order changed.

The decision becomes **Accepted** after a vertical-slice spike proves:

- loading an official Character Sheet schema;
- creating a Character from that schema;
- storing typed package-defined values;
- validating required and bounded fields;
- editing an allowed field;
- rejecting a forbidden field edit;
- calculating derived values;
- rebuilding derived values after dependency changes;
- applying temporary and current-value changes;
- preserving change history;
- migrating one schema version to another;
- retaining archived removed-field values;
- rendering localized labels without changing field identity.

## 2. Context

Chronicle must support different RPG systems with different Character Sheets.

A Character may have:

- attributes;
- abilities;
- resources;
- health tracks;
- advantages;
- flaws;
- powers;
- equipment;
- conditions;
- morality;
- renown;
- forms;
- temporary modifiers;
- derived values;
- advancement costs;
- package-specific sections.

The Chronicle core cannot hardcode every system's fields.

At the same time, a completely unstructured JSON property bag would make it difficult to enforce:

- types;
- limits;
- requiredness;
- progression;
- derived values;
- field visibility;
- field editability;
- Rule Operation inputs;
- migrations;
- query behavior;
- historical audit.

The Character Sheet must also coexist with nonmechanical Character data such as:

- name;
- pronouns;
- description;
- personality;
- goals;
- fears;
- personal history;
- relationships;
- Memories;
- Character Knowledge.

Those concerns have different lifecycle and authority rules.

ADR-0028 defines Rule Set packages.

ADR-0029 defines Rule Operations.

ADR-0024 defines relational and JSON persistence conventions.

This ADR defines the Character Sheet model connecting them.

## 3. Decision Drivers

The design prioritizes:

1. multi-system extensibility;
2. stable field identity;
3. deterministic mechanics;
4. typed validation;
5. schema migration;
6. auditability;
7. package isolation;
8. localization;
9. queryability;
10. bounded storage;
11. UI generation;
12. historical preservation.

## 4. Decision Summary

Chronicle will use:

```text
Schema Owner
    Rule Set package

Schema Identity
    CharacterSchemaId
    CharacterSchemaVersion

Field Identity
    stable semantic FieldKey

Field Types
    explicit bounded type catalog

Storage
    relational Character core
    versioned typed field-value records
    relational history for authoritative changes

Derived Values
    approved deterministic Rule Operations

Mutation
    Application command
    schema validation
    Rule Set validation
    Domain application
    transaction

Localization
    display keys only
    never field identity

Migration
    explicit schema migration contract
```

## 5. Character Core Versus Character Sheet

Chronicle separates:

```text
Character Core
Character Sheet
Character Narrative Profile
Character Knowledge
Relationships
Memories
Progression History
```

## 6. Character Core

Character Core contains stable Chronicle-owned fields such as:

```text
CharacterId
CampaignId
CharacterType
LifecycleStatus
CreatedAtUtc
UpdatedAtUtc
AggregateVersion
CharacterSchemaId
CharacterSchemaVersion
RuleSetPackageId
RuleSetPackageVersion
```

## 7. Character Narrative Profile

Narrative profile may contain:

- display name;
- aliases;
- pronouns;
- description;
- personality notes;
- goals;
- fears;
- personal-history summaries.

These are not automatically mechanical fields.

## 8. Character Sheet

The Character Sheet contains package-defined mechanical state.

## 9. Schema Identity

Every Character Sheet schema has:

```text
CharacterSchemaId
CharacterSchemaVersion
RuleSetPackageId
RuleSetPackageVersion
```

## 10. Schema Key Example

Example:

```text
chronicle.schema.wta.player-character
```

## 11. Exact Schema Binding

Every Character stores the exact schema identity and version used by its current field values.

## 12. No Silent Schema Upgrade

Installing a newer package does not upgrade existing Characters automatically.

## 13. Schema Contract

A Character Sheet schema is a strict versioned contract.

Recommended contract key:

```text
chronicle.ruleset.character-schema
```

## 14. Schema Sections

A schema may define ordered sections.

Example:

```text
identity
attributes
abilities
advantages
resources
health
powers
conditions
progression
```

## 15. Section Identity

Sections use stable semantic keys.

Display titles are localized separately.

## 16. Field Identity

Each field uses a stable:

```text
FieldKey
```

Example:

```text
attribute.strength
ability.athletics
resource.willpower.current
resource.willpower.maximum
```

## 17. Field Key Stability

A field label may change without changing the FieldKey.

A FieldKey changes only when the mechanical meaning changes.

## 18. Field Display Name

Display names use localization keys.

Example:

```text
ruleset.wta.field.strength.label
```

## 19. Field Ordering

Display order is schema metadata.

It is not identity and must not be used for persistence references.

## 20. Field Type Catalog

The MVP supports a bounded catalog such as:

```text
Integer
Decimal
Boolean
Text
SingleChoice
MultipleChoice
Rating
Resource
Track
Reference
Collection
StructuredValue
```

The final type set is validated through the implementation spike.

## 21. Integer Field

An integer field defines:

```text
minimum
maximum
step
default
nullable
```

## 22. Decimal Field

A decimal field defines exact precision and scale.

Floating-point mechanics are avoided.

## 23. Boolean Field

A Boolean field has explicit default and editability.

## 24. Text Field

A text field defines:

- maximum length;
- multiline behavior;
- localization or free-text classification;
- whether it is mechanical or descriptive.

## 25. Single Choice

A single-choice field references one stable option key.

## 26. Multiple Choice

A multiple-choice field stores a bounded set of stable option keys.

## 27. Rating Field

A rating field represents an ordered mechanical scale.

Example:

```text
0..5
```

It may define display markers separately.

## 28. Resource Field

A resource SHOULD model:

```text
current
maximum
minimum
temporary modifiers
overflow policy
```

It should not be represented as an ambiguous pair of unrelated integers.

## 29. Track Field

A track models ordered boxes or states.

Examples:

- health;
- aggravated damage;
- willpower expenditure;
- condition tracks.

## 30. Reference Field

A reference field may point to:

- another Character;
- package-defined concept;
- equipment item;
- condition;
- approved external Domain entity.

Reference scope is explicit and validated.

## 31. Collection Field

A collection field defines:

- item contract;
- maximum count;
- ordering;
- duplicate policy;
- identity semantics.

## 32. Structured Value

A structured value uses a bounded versioned sub-contract.

It must not become an unrestricted JSON object.

## 33. No Arbitrary Object Field

A generic unvalidated object field is prohibited.

## 34. Field Definition

A field definition SHOULD contain:

```text
FieldKey
FieldType
LabelKey
DescriptionKey
SectionKey
DisplayOrder
Required
DefaultValue
ValidationRules
VisibilityPolicy
EditabilityPolicy
MechanicalRole
StoragePolicy
DerivationPolicy
ProgressionPolicy
MigrationMetadata
```

## 35. Mechanical Role

Recommended roles:

```text
Identity
BaseValue
CurrentValue
MaximumValue
TemporaryModifier
DerivedValue
Track
Choice
Reference
Descriptive
ProgressionControlled
SystemManaged
```

## 36. Storage Policy

Recommended storage policies:

```text
PersistedAuthoritative
PersistedProjection
ComputedOnDemand
HistoricalOnly
TransientUiOnly
```

## 37. Authoritative Field

A persisted authoritative field participates in Domain and Rule Set decisions.

## 38. Persisted Projection

A persisted projection is derived but stored for performance or inspection.

Its source operation and dependencies remain known.

## 39. Computed on Demand

A computed-on-demand field is calculated when needed and not stored as authoritative state.

## 40. Historical-Only Field

A historical-only field remains available for audit after migration or retirement.

## 41. UI-Only Field

A UI-only field never enters mechanical or durable state.

## 42. Field Value Model

Chronicle stores typed field values rather than one untyped dictionary.

Conceptually:

```text
CharacterFieldValue
    CharacterId
    FieldKey
    FieldType
    ValueContractVersion
    TypedValue
    Source
    UpdatedAtUtc
    CharacterVersion
```

## 43. Physical Storage

The initial physical model SHOULD combine:

- relational rows for field identity, type, source, and version;
- bounded typed columns or a versioned value payload for the field value;
- relational indexes for common mechanical lookups where needed.

## 44. One Row per Field

A practical initial approach is one current row per Character and FieldKey.

Historical changes remain in a separate ledger.

## 45. Typed Value Columns

A field-value table MAY use nullable typed columns such as:

```text
IntegerValue
DecimalValue
BooleanValue
TextValue
ReferenceId
JsonValue
```

A check constraint ensures exactly one compatible representation is used.

## 46. Versioned Payload Alternative

Complex bounded values may use:

```text
ValueContractKey
ValueContractVersion
JsonValue
```

## 47. No Full Sheet Blob as Sole Truth

The entire Character Sheet SHOULD NOT exist only as one opaque JSON blob.

### Rationale

Chronicle needs:

- typed validation;
- targeted changes;
- history;
- migration;
- indexing;
- mechanical inputs;
- conflict detection.

## 48. Sheet Snapshot

A full versioned Character Sheet snapshot MAY be stored for:

- export;
- audit;
- migration checkpoint;
- provider context;
- historical comparison.

It is not the only authoritative representation.

## 49. Field Source

Every current field value SHOULD have a source classification.

Recommended values:

```text
CharacterCreation
UserEdit
RuleOperation
Progression
SessionFinalization
Import
SchemaMigration
AdministrativeRecovery
DerivedCalculation
```

## 50. Field Change History

Authoritative mechanical changes append a history record.

Recommended fields:

```text
CharacterFieldChangeId
CharacterId
FieldKey
PreviousValue
NewValue
Source
ReasonKey
OperationId
RuleSetPackageVersion
RuleOperationKey
ChangedAtUtc
CharacterVersionBefore
CharacterVersionAfter
```

## 51. History Privacy

Field history is private Campaign data.

It must not be logged in ordinary diagnostics.

## 52. Current Value and History

The current field table supports efficient reads.

The history ledger supports audit and reconstruction.

## 53. Append-Only History

Historical change records are append-only.

Corrections use compensating entries or explicit recovery workflow.

## 54. Initial Character Creation

Character creation uses:

1. exact schema resolution;
2. default-value expansion;
3. user-provided input;
4. schema validation;
5. Rule Set Character validation;
6. Character Domain creation;
7. persistence of current values;
8. creation history;
9. commit.

## 55. Creation Defaults

Defaults are part of the schema version.

They are expanded explicitly and persisted when they represent authoritative values.

## 56. Missing Required Field

Missing required fields block Character creation.

## 57. Optional Field

Optional fields may be absent.

Absence is distinct from null and from a default value.

## 58. Field Mutation Command

Recommended command:

```text
ChangeCharacterFieldCommand
```

## 59. Multi-Field Mutation

Recommended command:

```text
ApplyCharacterFieldChangesCommand
```

Multi-field changes are atomic when one mechanical intention requires them.

## 60. Mutation Input

A mutation includes:

```text
CharacterId
ExpectedCharacterVersion
FieldChanges
OperationId
ChangeSource
Reason
```

## 61. Mutation Validation

The Application validates:

- Character exists;
- Campaign scope;
- exact schema;
- field exists;
- field type;
- editability;
- value bounds;
- package validation;
- progression rules;
- expected version.

## 62. Narrative Intelligence Mutation

Narrative Intelligence cannot call Character mutation directly as authority.

It may propose:

- consequence;
- condition;
- resource change;
- progression suggestion.

Chronicle validates and applies through commands.

## 63. User Editability

Field editability is separate from field visibility.

Recommended policies:

```text
UserEditable
CreationOnly
ProgressionOnly
RuleOperationOnly
SystemManaged
ReadOnlyDerived
AdministrativeRecoveryOnly
```

## 64. Visibility

Recommended policies:

```text
AlwaysVisible
OwnerVisible
DirectorVisible
HiddenUntilRevealed
ConditionallyVisible
InternalOnly
```

For the single-user MVP, the model still preserves future role semantics.

## 65. Visibility Is Not Security Alone

Presentation uses visibility metadata.

Application queries still enforce what may be returned.

## 66. Hidden Fields

Hidden fields are not included in provider context unless the role and operation permit them.

## 67. Derived Values

Derived values are calculated through approved Rule Operations.

## 68. Derivation Definition

A derived-field definition SHOULD declare:

```text
FieldKey
DerivationOperationKey
DependencyFieldKeys
PreferenceKeys
OutputContractVersion
StoragePolicy
InvalidationPolicy
```

## 69. No Arbitrary Expressions

The MVP does not evaluate arbitrary formulas, scripts, or embedded code from package schemas.

## 70. Approved Operation

The package points to a reviewed deterministic Rule Operation.

## 71. Dependency Graph

Chronicle constructs a directed dependency graph for derived fields.

## 72. Circular Dependency

Cycles are rejected during package activation.

## 73. Dependency Validation

Every declared dependency field must exist in the same compatible schema or an explicitly permitted external snapshot.

## 74. Derived Evaluation Flow

Recommended flow:

1. load required current field values;
2. resolve effective Preferences;
3. build immutable derivation input;
4. execute exact package Rule Operation;
5. validate output;
6. compare with current projection;
7. persist if storage policy requires;
8. append projection update evidence;
9. increment Character version only according to policy.

## 75. Derived Value Authority

The derivation algorithm is package-defined.

Chronicle decides whether the result is valid and may be persisted.

## 76. Persisted Projection Version

A persisted derived field SHOULD record:

```text
DerivationOperationKey
PackageVersion
InputFingerprint
CalculatedAtUtc
DependencyVersion
```

## 77. Derived Invalidation

When a dependency changes, dependent derived values become:

```text
Valid
Stale
RecalculationRequired
Invalid
```

## 78. Synchronous Recalculation

Small required derived values SHOULD be recalculated in the same Application operation before commit when needed for invariants.

## 79. Deferred Recalculation

Large noncritical projections MAY be recalculated through a Work Item.

Until completion, the stale state is explicit.

## 80. No Silent Stale Use

Mechanical operations must not use a stale derived value as current truth unless the operation explicitly recalculates it.

## 81. Derived Value Cache

Computed-on-demand results MAY be cached by:

- Character version;
- package version;
- Preference version;
- operation version;
- input fingerprint.

The cache is nonauthoritative.

## 82. Base and Current Values

Fields that distinguish base and current values use separate stable keys or one structured resource contract.

The package must not rely on label conventions such as “temporary” versus “permanent.”

## 83. Temporary Modifiers

Temporary modifiers SHOULD be modeled as explicit records when they have:

- source;
- duration;
- stacking;
- scope;
- expiration;
- history.

## 84. Modifier Record

A modifier MAY contain:

```text
ModifierId
TargetFieldKey
Amount or effect
SourceKey
AppliedAt
ExpiresByPolicy
StackingRule
OperationId
```

## 85. Modifier Calculation

Effective field values are calculated through a Rule Operation or approved deterministic policy.

## 86. No In-Place Loss of Base Value

Applying a temporary modifier must not overwrite the underlying base value.

## 87. Resource Values

Resources SHOULD distinguish:

```text
Current
Maximum
TemporaryMaximum
Minimum
Overflow
Spent
```

according to package semantics.

## 88. Resource Mutation

Resource changes occur through bounded commands or consequence proposals.

## 89. Track Values

Track updates preserve box order, damage type, or state type where mechanically relevant.

## 90. Collection Items

Collection items with independent identity SHOULD have stable item IDs.

Example:

- power instance;
- equipment item;
- condition instance;
- specialty;
- contact.

## 91. Embedded Item Identity

A collection item may use:

```text
CharacterFieldItemId
```

or a package-defined stable item identity.

## 92. Collection History

Add, update, reorder, and remove operations are explicit.

## 93. Remove Versus Archive

Items with historical significance SHOULD be retired or archived rather than deleted.

## 94. Reference Integrity

Reference fields and collection items validate:

- target type;
- Campaign scope;
- lifecycle;
- visibility;
- package compatibility.

## 95. Character-to-Character Reference

A reference to another Character uses `CharacterId`, not a name string.

## 96. Package Concept Reference

Package-defined concepts use semantic keys and package version context.

## 97. Character Schema Migration

Schema migration occurs during explicit package or Character migration workflow.

## 98. Migration Contract

A migration SHOULD declare:

```text
SourceSchemaId
SourceSchemaVersion
TargetSchemaId
TargetSchemaVersion
FieldMappings
ValueTransformations
RemovedFieldPolicies
NewFieldDefaults
DerivedRebuilds
ValidationOperations
```

## 99. Field Mapping

A field may be:

```text
Unchanged
RenamedByKey
Transformed
Split
Merged
Retired
Introduced
```

## 100. Display Rename

Changing only the label or localization key is not a field migration.

## 101. Semantic Rename

If the meaning changes, a new FieldKey is required.

## 102. Removed Field

A removed authoritative field must define one policy:

```text
ArchiveValue
MapToReplacement
TransformToHistory
BlockMigration
ExplicitlyDiscardWithCheckpoint
```

## 103. Silent Removal

Silent deletion of stored authoritative values is prohibited.

## 104. New Required Field

A new required field needs:

- deterministic default;
- migration transformation;
- user resolution;
- or blocked migration.

## 105. Split Field

A split transformation must define how one source value creates multiple target values.

## 106. Merge Field

A merge transformation must define conflict and precedence behavior.

## 107. Migration Determinism

Schema migration is deterministic and provider-free.

## 108. Migration Checkpoint

High-risk Character schema migration requires the package-upgrade checkpoint policy.

## 109. Migration Validation

After migration, Chronicle validates:

- target schema;
- required fields;
- field types;
- references;
- derived dependencies;
- Character-wide Rule Operation validation;
- progression consistency.

## 110. Migration History

Chronicle records:

```text
CharacterSchemaMigrationId
CharacterId
SourceSchemaVersion
TargetSchemaVersion
PackageVersions
OperationId
MigratedAtUtc
Outcome
ArchivedFieldCount
```

## 111. Legacy Field Archive

Retired values may be stored in an archival table or versioned historical snapshot.

They are not exposed as active mechanical fields.

## 112. Unknown Field During Load

An unknown current field under the declared schema indicates:

- incomplete migration;
- package mismatch;
- corruption;
- unsupported extension.

Normal mechanical mutation is blocked until resolved.

## 113. Missing Schema

If the Character's exact schema is unavailable:

- Character remains visible through generic projections;
- active mechanical editing is blocked;
- raw stored fields remain preserved;
- Safe Mode or compatibility UI explains the dependency.

## 114. Schema Upgrade and Historical Outcomes

Changing the schema does not rewrite historical Dice or progression evidence.

## 115. Query Model

Character Sheet queries return purpose-built projections.

## 116. Character Sheet Projection

A projection MAY include:

```text
Character metadata
Schema identity
Section definitions
Visible field definitions
Current typed values
Derived status
Validation issues
Editability
Display metadata
Character version
```

## 117. No EF Entity Exposure

Presentation receives contract DTOs, not persistence rows.

## 118. UI Generation

The UI MAY render Character Sheets from schema metadata.

## 119. UI Override

Rule Set packages may declare layout hints.

They do not provide executable UI code in the MVP.

## 120. Layout Hints

Examples:

```text
section grouping
column preference
compact rating control
resource track control
choice control
read-only summary
```

## 121. Accessibility

Schema-driven controls must support:

- keyboard navigation;
- accessible labels;
- screen-reader descriptions;
- noncolor state;
- scalable text;
- validation announcements.

## 122. Localization

Field and section labels use localization resources.

Stored values use semantic keys where appropriate.

## 123. Localized Choice

A choice stores:

```text
choice.wolf
```

not:

```text
"Lobo"
```

## 124. Search and Filtering

Frequently searched fields MAY receive dedicated relational indexes or read-model projections.

The package must declare indexable field intent.

## 125. Indexable Field

An indexable field should be:

- scalar;
- bounded;
- mechanically stable;
- commonly queried;
- privacy-reviewed.

## 126. No Arbitrary Dynamic Indexes

Packages cannot create arbitrary database indexes directly.

Chronicle controls physical indexing.

## 127. Provider Context

Prompt Construction may include selected Character Sheet fields according to:

- role;
- visibility;
- relevance;
- token budget;
- current state;
- mechanical need.

## 128. Provider Field Representation

Provider context uses:

- stable field meaning;
- localized or narrative label where helpful;
- accepted current value;
- derived status;
- no hidden or unauthorized fields.

## 129. Provider Cannot Infer Missing Truth

A field omitted from context must not be guessed into authoritative state.

## 130. Character Validation

The package SHOULD expose a Character validation Rule Operation.

## 131. Validation Issue

Recommended issue contract:

```text
IssueKey
Severity
FieldKey
RelatedFieldKeys
Parameters
Blocking
SuggestedActionKey
```

## 132. Validation Severity

Recommended values:

```text
Information
Warning
Error
Blocking
```

## 133. Validation Persistence

Current validation state may be projected.

Authoritative state remains the Character fields and schema.

## 134. Character Readiness

A Character may have readiness states such as:

```text
Draft
Valid
ValidWithWarnings
Invalid
MigrationRequired
PackageUnavailable
```

## 135. Draft Character

Draft Characters may temporarily omit required creation fields if the creation workflow explicitly supports drafts.

They cannot enter play until valid.

## 136. Player Character Constraint

The MVP permits one Player Character per Campaign.

The Character model remains capable of supporting NPCs and future participants.

## 137. Player Character Selection

The Campaign explicitly references the active Player Character.

It is not inferred from Character type alone.

## 138. Character Version

Any authoritative mechanical field mutation increments the Character version.

## 139. Narrative-Only Change Version

Narrative profile changes MAY use a separate version or the same Character aggregate version according to concurrency design.

The spike should verify whether separate versions reduce conflicts.

## 140. Field-Level Concurrency

The MVP does not adopt independent field-level concurrency by default.

Character-level optimistic concurrency remains simpler and safer.

## 141. Bulk Character Changes

A Rule Operation may propose multiple field changes.

They are validated and committed atomically.

## 142. Partial Acceptance

Partial acceptance of a multi-field mechanical proposal is prohibited unless the operation contract explicitly models independent optional outcomes.

## 143. Operation Evidence

Each mechanical change history entry SHOULD reference:

- OperationId;
- Rule Operation;
- package version;
- source Dice Roll when applicable;
- consequence result;
- Session and Scene context where relevant.

## 144. Character Snapshot

Chronicle MAY persist snapshots at:

- Session start;
- Session finalization;
- package migration;
- export;
- backup checkpoint.

Snapshots aid diagnostics and comparison but do not replace current rows and history.

## 145. Snapshot Contract

Snapshots use a versioned portable Character Sheet contract.

## 146. Character Deletion

Authoritative Characters are archived or retired.

Physical deletion is reserved for explicit full-data removal.

## 147. Retired Character

Retirement preserves:

- Sheet;
- history;
- Memories;
- Relationships;
- Knowledge;
- progression;
- Session references.

## 148. Error Model

Recommended errors:

```text
character.schema-not-found
character.schema-version-not-found
character.schema-incompatible
character.field-not-found
character.field-type-invalid
character.field-required
character.field-read-only
character.field-out-of-range
character.field-reference-invalid
character.field-value-invalid
character.version-conflict
character.derived-cycle
character.derived-stale
character.derived-calculation-failed
character.migration-required
character.migration-failed
character.validation-blocked
```

## 149. Data Preservation State

Character operation results SHOULD state:

```text
CharacterUnchanged
FieldHistoryAppended
DerivedValuesUpdated
MigrationCheckpointAvailable
LegacyValuesArchived
RecoveryRequired
```

## 150. Logging

Logs MAY include:

- CharacterId;
- CampaignId;
- schema ID;
- schema version;
- FieldKey;
- operation category;
- result code;
- Character version;
- duration.

They MUST NOT include private field values by default.

## 151. Metrics

Useful metrics include:

```text
CharacterValidationDuration
CharacterFieldChangeCount
CharacterFieldValidationFailureCount
DerivedValueCalculationDuration
DerivedValueRebuildCount
CharacterSchemaMigrationCount
CharacterSchemaMigrationFailureCount
```

## 152. Testing Strategy

The implementation requires:

```text
Schema Contract Tests
Field-Type Tests
Persistence Round-Trip Tests
Validation Tests
Derived-Value Tests
Migration Tests
Concurrency Tests
UI Projection Tests
Security Tests
Architecture Tests
```

## 153. Schema Contract Tests

Tests MUST cover:

- valid schema;
- duplicate section;
- duplicate field;
- invalid FieldKey;
- unsupported type;
- invalid default;
- invalid bound;
- missing localization key metadata;
- invalid operation reference;
- derivation cycle.

## 154. Field-Type Tests

Tests MUST cover every supported type and boundary.

## 155. Persistence Tests

Tests MUST prove:

- typed round-trip;
- exactly one active value representation;
- history append;
- ordered collection items;
- references;
- schema version preservation.

## 156. Mutation Tests

Tests MUST cover:

- allowed user edit;
- creation-only edit rejected after creation;
- progression-only field;
- Rule Operation-only field;
- system-managed field;
- version conflict;
- multi-field atomic update.

## 157. Derived Tests

Tests MUST cover:

- simple dependency;
- multiple dependencies;
- Preference dependency;
- stale invalidation;
- synchronous rebuild;
- failed calculation;
- cycle rejection;
- deterministic result;
- cached result invalidation.

## 158. Migration Tests

Tests MUST cover:

- label-only change;
- stable field unchanged;
- new optional field;
- new required field with default;
- field transform;
- split;
- merge;
- retired field archive;
- blocked migration;
- rollback or recovery.

## 159. Reference Tests

Tests MUST cover:

- valid same-Campaign Character;
- invalid cross-Campaign Character;
- retired target;
- missing package concept;
- clone import remapping;
- export and import preservation.

## 160. Provider Boundary Tests

Tests MUST prove:

- hidden fields omitted;
- provider cannot mutate fields;
- proposed change requires command;
- missing context does not create default authoritative values.

## 161. Required Test Cases

Tests MUST cover:

- official Player Character schema;
- draft creation;
- valid creation;
- invalid creation;
- rating field;
- resource field;
- track field;
- choice field;
- collection item;
- derived maximum;
- temporary modifier;
- progression change;
- consequence change;
- field history;
- exact schema resolution;
- missing schema;
- package upgrade;
- legacy archive;
- localized rendering;
- no arbitrary expression execution.

## 162. Architecture Tests

Architecture tests MUST reject:

- package field labels used as identity;
- arbitrary JSON property bag as sole current Character Sheet truth;
- provider directly mutating Character fields;
- derived values calculated in Presentation;
- arbitrary script execution from schema;
- package-created database indexes;
- public mutable field-value collections;
- persistence-specific annotations in Character Domain model;
- unversioned schema migration;
- direct overwrite of base value by temporary modifier.

## 163. Prohibited Patterns

### 163.1 Field Identity Is Display Label

Use stable semantic FieldKey.

### 163.2 Entire Sheet Is Opaque JSON

Use typed field records and versioned bounded payloads.

### 163.3 Derived Formula From Arbitrary Package Script

Use approved deterministic Rule Operations.

### 163.4 Narrative Provider Edits Sheet Directly

Providers only propose.

### 163.5 Silent Field Removal

Archive, transform, or block migration.

### 163.6 Temporary Modifier Overwrites Base Value

Keep source values separate.

### 163.7 Missing Schema Falls Back to Another Schema

Exact identity matters.

### 163.8 UI Determines Mechanical Validity

Application and Rule Set validation own it.

### 163.9 Partial Application of Mechanical Field Batch

Commit atomically.

### 163.10 Localized Choice Stored as Value Identity

Store semantic option key.

## 164. Alternatives Considered

### One JSON Blob per Character

Simple initially, but weak for validation, history, querying, migration, and targeted updates.

### One Database Column per Possible Rule Set Field

Rejected because every Rule Set would require core schema changes.

### Fully Entity-Attribute-Value Model Without Typed Contracts

Flexible, but too weakly typed and difficult to validate safely.

Chronicle uses a typed field-value model with schema ownership.

### Arbitrary Expression Language

Deferred because sandboxing, determinism, debugging, and migration semantics require separate design.

### Hardcoded Official WTA Character Model

Useful as a prototype, but would undermine the multi-system framework.

### Recalculate Every Derived Value on Every Read

Potentially simple but inefficient and difficult to diagnose.

Chronicle permits computed-on-demand and persisted projections according to schema policy.

## 165. Consequences

### Positive

- multi-system Character Sheets;
- stable field identity;
- deterministic derived values;
- strong validation;
- explicit history;
- schema-driven UI;
- package version reproducibility;
- migration without silent data loss;
- provider-safe mutation boundary.

### Negative

- typed dynamic field storage is more complex than a blob;
- schema and migration contracts require maintenance;
- collection and structured fields need careful design;
- persisted projections require invalidation;
- package authors need strong tooling;
- some queries may require dedicated projections.

## 166. Risks

### Typed Field Model Becomes Too Generic

Mitigation:

- bounded field-type catalog;
- system-specific Rule Operations;
- dedicated structures for resources and tracks.

### Performance With Many Field Rows

Mitigation:

- bounded Sheet size;
- aggregate load plan;
- indexes for declared hot fields;
- snapshots and projections;
- performance tests.

### Schema Migration Loses Meaning

Mitigation:

- explicit mappings;
- archived legacy values;
- checkpoint;
- semantic validation.

### Derived Projection Becomes Stale

Mitigation:

- dependency graph;
- invalidation state;
- synchronous rebuild for required mechanics;
- input fingerprint.

### Package Schema Is Invalid

Mitigation:

- activation validation;
- contract test kit;
- quarantine.

## 167. Technology Spike

Before acceptance, implement:

1. Character schema contract;
2. field-type catalog;
3. typed field-value persistence;
4. Character creation workflow;
5. field mutation command;
6. Character validation operation;
7. derived-value operation;
8. dependency graph;
9. field change history;
10. resource and track field examples;
11. schema-driven projection;
12. localization mapping;
13. one schema migration;
14. archived retired field storage;
15. architecture and performance tests.

## 168. Spike Acceptance

The spike passes when:

- an official package defines and loads a Player Character schema;
- a Character can be created and reloaded with typed values;
- invalid values are rejected before persistence;
- editability policies are enforced;
- derived values calculate deterministically;
- dependency changes invalidate or rebuild projections correctly;
- field history preserves old and new values;
- labels can change without changing identity;
- a removed field is archived rather than lost;
- exact schema resolution blocks silent fallback;
- no provider, UI, or arbitrary script can bypass the mutation pipeline.

## 169. Definition of Compliance

An implementation complies when:

- Character Sheets are package-defined and versioned;
- FieldKeys are stable and independent from labels;
- core Character state remains relational and Chronicle-owned;
- package-defined fields use typed bounded storage;
- full opaque JSON is not the sole truth;
- field mutation passes through Application and Rule Set validation;
- derived values use approved deterministic Rule Operations;
- dependency cycles are rejected;
- current values and history are both preserved;
- schema migrations are explicit and deterministic;
- retired values are archived or intentionally transformed;
- Narrative Intelligence cannot directly mutate the Sheet;
- UI rendering follows schema metadata without owning mechanics.

## 170. Review Triggers

This ADR must be reviewed if:

- arbitrary third-party Rule Set schemas become public;
- a scripting or formula language is introduced;
- Character Sheets require deeply nested structures;
- server or multiplayer editing introduces field-level concurrency;
- mobile clients require a public schema protocol;
- media-rich Character fields become first-class;
- field count or query performance becomes problematic;
- schema-defined custom UI components are introduced;
- package-defined database indexes become necessary;
- collaborative Character editing is introduced.

## 171. Deferred Decisions

Later ADRs MAY define:

- exact physical typed-value table layout;
- exact field-type catalog;
- constrained expression language;
- custom schema UI component protocol;
- field-level optimistic concurrency;
- media attachment fields;
- public Character Sheet schema SDK;
- Character Sheet diff UI;
- snapshot frequency;
- package-author tooling;
- indexable-field projection generation.

## 172. Final Decision

Chronicle will represent Character Sheets through exact, versioned Rule Set schemas and stable semantic field identities.

Character values will be typed, validated, historically auditable, and migrated explicitly.

Derived values will be calculated by deterministic Rule Operations rather than arbitrary scripts.

A label may change.

A layout may change.

A package may evolve.

Chronicle must never lose track of what a Character field meant when it became part of the Campaign.
