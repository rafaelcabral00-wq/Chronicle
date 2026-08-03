---
id: ADR-0032
title: Campaign Preferences Definition, Resolution, and Versioned Application
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
  - ADR-0013
  - ADR-0017
  - ADR-0018
  - ADR-0019
  - ADR-0024
  - ADR-0028
  - ADR-0029
  - ADR-0030
  - ADR-0031
  - RFC-0005
  - RFC-0006
  - RFC-0007
  - RFC-0010
  - RFC-0013
  - RFC-0015
  - RFC-0016
  - RFC-0017
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

> **"A Preference may shape how a Campaign is played. It may not silently rewrite the meaning of what was already accepted."**

# Campaign Preferences Definition, Resolution, and Versioned Application

## 1. Status

**Proposed**

This ADR defines Chronicle's Campaign Preference model, package-defined preference contracts, override resolution, validation, versioning, historical evidence, and application to mechanics and narration.

The decision is:

- treat Preferences as explicit versioned Campaign configuration;
- distinguish Chronicle-owned Preferences from Rule Set-owned Preferences;
- use stable semantic preference keys independent from labels;
- define every Preference through a strict typed contract;
- store explicit Campaign values separately from defaults;
- resolve effective values deterministically through a documented precedence chain;
- snapshot or version effective Preferences when they affect accepted mechanical or narrative outcomes;
- prohibit silent default changes from retroactively changing historical meaning;
- validate every Preference value against the exact defining package version;
- apply Preference changes only through Application commands;
- prevent Narrative Intelligence from modifying Preferences directly;
- classify Preferences by effect domain, including mechanics, narration, safety, presentation, and workflow;
- require explicit confirmation for changes that may materially alter rules or Campaign behavior;
- invalidate or recompute affected derived projections after changes;
- preserve historical Preference values used by Dice resolution, progression, Character derivation, and finalization;
- allow missing or incompatible package Preferences to remain preserved but unavailable for mutation;
- avoid storing secrets, provider credentials, or unrestricted prompt text as ordinary Preferences;
- prohibit Preferences from bypassing Rule Set contracts or Chronicle authority.

The decision becomes **Accepted** after a vertical-slice spike proves:

- loading Chronicle and Rule Set Preference definitions;
- rendering typed Preference controls;
- applying defaults;
- storing explicit overrides;
- deterministic effective-value resolution;
- Preference change validation;
- version increment;
- mechanical result evidence containing Preference version;
- derived-value invalidation;
- package upgrade migration;
- preservation of unknown legacy Preferences;
- rejection of invalid and unauthorized changes.

## 2. Context

Different Campaigns may use different rules and play styles even under the same Rule Set.

Examples include:

- optional damage rules;
- difficulty conventions;
- exceptional-success thresholds;
- critical-failure behavior;
- progression cost variants;
- Session award policies;
- narrative tone;
- content boundaries;
- pacing preferences;
- automatic versus manual confirmations;
- Dice presentation;
- memory aging policies within supported limits;
- package-specific variants.

These choices must be represented explicitly.

If Preferences are handled as arbitrary settings or hidden prompt instructions, Chronicle risks:

- mechanics changing without audit;
- package defaults changing historical outcomes;
- provider prose overriding rules;
- invalid values entering Rule Operations;
- migrations losing old meaning;
- user-facing labels becoming identity;
- hidden global settings affecting only some Campaigns;
- inability to explain why two identical Dice pools resolved differently.

Chronicle therefore needs a versioned, typed, Campaign-scoped Preference model.

## 3. Decision Drivers

The Preference design prioritizes:

1. explicit Campaign behavior;
2. deterministic resolution;
3. typed validation;
4. exact package-version semantics;
5. historical reproducibility;
6. user control;
7. no hidden defaults;
8. safe package evolution;
9. provider isolation;
10. queryability;
11. UI generation;
12. future multiplayer readiness.

## 4. Decision Summary

Chronicle will use:

```text
Definition Owner
    Chronicle core or Rule Set package

Preference Identity
    stable semantic PreferenceKey

Value
    strict typed contract

Scope
    Campaign in MVP

Storage
    explicit values plus definition reference

Effective Resolution
    explicit Campaign override
    package or Chronicle default
    no ambient hidden fallback

Version
    CampaignPreferenceVersion

Application
    immutable Preference snapshot

Historical Evidence
    exact Preference version and relevant values or fingerprint

Mutation
    Application command only
```

## 5. Preference Categories

Chronicle distinguishes:

```text
Mechanical
Narrative
Safety
Workflow
Presentation
Diagnostic
```

## 6. Mechanical Preference

A Mechanical Preference changes Rule Operation behavior.

Examples:

- difficulty convention;
- critical threshold;
- optional combat rule;
- advancement cost variant.

## 7. Narrative Preference

A Narrative Preference guides presentation or storytelling.

Examples:

- tone;
- descriptive density;
- pacing;
- dialogue emphasis.

It does not override mechanical truth.

## 8. Safety Preference

A Safety Preference controls content boundaries or handling.

It requires separate privacy and provider-context enforcement.

## 9. Workflow Preference

A Workflow Preference changes interaction flow.

Examples:

- require confirmation before applying proposed consequence;
- automatic Session-title suggestion;
- finalization review mode.

## 10. Presentation Preference

A Presentation Preference affects UI only.

Examples:

- Dice animation;
- compact Character Sheet layout;
- timestamp display.

It must not alter authoritative mechanics.

## 11. Diagnostic Preference

Diagnostic settings are generally installation-scoped rather than Campaign-scoped.

They are included here only when a package requires a safe Campaign-specific diagnostic toggle.

## 12. Preference Ownership

Every Preference has one owner:

```text
ChronicleCore
RuleSetPackage
```

## 13. Chronicle-Owned Preference

Chronicle-owned Preferences control core workflow or presentation.

## 14. Rule Set-Owned Preference

Rule Set-owned Preferences affect package-defined mechanics or behavior.

## 15. Preference Identity

Every Preference uses a stable semantic:

```text
PreferenceKey
```

Example:

```text
chronicle.workflow.confirm-mechanical-consequence
ruleset.wta.roll.critical-threshold
ruleset.wta.progression.cost-model
```

## 16. Label Independence

Localized labels and descriptions are not identity.

## 17. Preference Definition

A Preference definition SHOULD contain:

```text
PreferenceKey
OwnerType
OwnerId
DefinitionVersion
ValueType
DefaultValue
AllowedValues
Minimum
Maximum
Required
Category
AffectsMechanics
AffectsNarration
AffectsDerivedValues
AffectsProgression
AffectsFinalization
RequiresConfirmation
VisibilityPolicy
EditabilityPolicy
LocalizationKeys
MigrationMetadata
```

## 18. Definition Version

Every Preference definition has a version.

A package version may contain multiple definition changes.

## 19. Value Types

The MVP supports a bounded type catalog:

```text
Boolean
Integer
Decimal
Text
SingleChoice
MultipleChoice
DurationPolicy
StructuredValue
```

## 20. Boolean Preference

Defines:

- default;
- editability;
- effect category.

## 21. Integer Preference

Defines:

- minimum;
- maximum;
- step;
- default.

## 22. Decimal Preference

Uses exact decimal representation with explicit precision.

## 23. Text Preference

Text Preferences require:

- maximum length;
- classification;
- provider transmission policy;
- whether free text is allowed.

## 24. Single Choice

Stores one stable option key.

## 25. Multiple Choice

Stores a bounded ordered or unordered set according to definition.

## 26. Duration Policy

A duration-policy Preference uses a structured bounded contract.

It does not store arbitrary executable scheduling logic.

## 27. Structured Value

Structured values use versioned bounded DTOs.

Unrestricted JSON objects are prohibited.

## 28. No Secret Preference

Preferences MUST NOT store:

- API keys;
- tokens;
- credentials;
- encryption keys;
- unrestricted secret references;
- provider passwords.

## 29. No Arbitrary Prompt Preference

An unrestricted free-text system prompt is not a normal Campaign Preference.

Narrative customization must use explicit bounded contracts or a separately reviewed feature.

## 30. Preference Scope

The MVP uses Campaign scope for authoritative play Preferences.

## 31. Installation Defaults

Chronicle MAY provide installation-level suggested defaults.

They do not silently override a Campaign's explicit or package-defined values.

## 32. Character Scope

Character-specific Preferences are deferred.

Character mechanics should normally be represented as Character fields, traits, or conditions.

## 33. Session Scope

Temporary Session overrides are deferred unless a specific workflow explicitly models them as scoped accepted configuration.

## 34. Effective Value Resolution

Recommended precedence:

```text
1. Explicit Campaign value
2. Migrated explicit Campaign value
3. Exact package-version default
4. Chronicle core default
5. Missing or invalid
```

Only applicable layers participate for a given Preference.

## 35. No Ambient Fallback

Environment variables, UI defaults, provider defaults, or code constants must not silently change authoritative Campaign behavior.

## 36. Explicit Value

An explicit value is persisted because the user or an approved migration selected it.

## 37. Default Value

A default belongs to one exact Preference definition version.

## 38. Default Expansion

When a default affects accepted mechanics, Chronicle SHOULD capture the effective value or Preference snapshot version used.

## 39. Default Change

A package update may change a default for new or migrated Campaigns.

It must not retroactively reinterpret accepted historical operations.

## 40. Preference Record

Recommended current-value model:

```text
CampaignPreference
    CampaignId
    PreferenceKey
    OwnerId
    DefinitionVersion
    ValueContractVersion
    TypedValue
    ValueSource
    UpdatedAtUtc
    CampaignPreferenceVersion
```

## 41. Value Source

Recommended values:

```text
ExplicitUserSelection
PackageDefault
ChronicleDefault
Migration
Import
AdministrativeRecovery
```

The database may store only explicit values while the resolved snapshot records source.

## 42. Campaign Preference Version

Every accepted authoritative Preference change increments:

```text
CampaignPreferenceVersion
```

## 43. Version Scope

The version applies to the effective Preference set for one Campaign.

## 44. Preference Set Snapshot

Chronicle SHOULD construct an immutable:

```text
CampaignPreferenceSnapshot
```

## 45. Snapshot Fields

Recommended fields:

```text
CampaignId
CampaignPreferenceVersion
RuleSetPackageId
RuleSetPackageVersion
ResolvedValues
DefinitionVersions
ValueSources
SnapshotFingerprint
```

## 46. Snapshot Use

The snapshot is passed to:

- Rule Operations;
- Prompt Construction;
- Session finalization;
- progression calculation;
- Character derivation;
- workflow decisions.

## 47. Snapshot Immutability

A snapshot cannot change during one operation.

## 48. Snapshot Fingerprint

The fingerprint uses canonical ordering and includes every relevant effective value.

## 49. Relevant Subset

An operation MAY fingerprint only the declared relevant Preference subset.

The operation catalog must declare that subset.

## 50. Historical Evidence

Accepted mechanical results SHOULD preserve:

- CampaignPreferenceVersion;
- relevant Preference keys;
- definition versions;
- snapshot or subset fingerprint;
- explicit values when needed for independent interpretation.

## 51. Full Snapshot Persistence

Chronicle need not persist every full snapshot for every operation if exact reconstruction is guaranteed.

Critical operations MAY persist a bounded relevant subset.

## 52. Mechanical Operation Integration

Rule Operations receive a validated immutable Preference snapshot.

## 53. Declared Preference Dependencies

Every Rule Operation declares which Preference keys it supports or requires.

## 54. Unknown Preference to Operation

Unknown or unrelated Preferences are not passed automatically.

## 55. Missing Required Preference

Execution fails unless the definition supplies a valid default.

## 56. Invalid Preference

Invalid values block affected mechanics.

Chronicle does not ask the provider to improvise.

## 57. Narrative Integration

Prompt Construction may include narrative and safety Preferences according to transmission policy.

## 58. Mechanical Versus Narrative Authority

A Narrative Preference cannot change a mechanical Rule Operation result.

## 59. Provider Interpretation

Providers may interpret bounded narrative Preferences but cannot mutate their values.

## 60. Preference Mutation Command

Recommended command:

```text
ChangeCampaignPreferenceCommand
```

## 61. Batch Mutation Command

Recommended command:

```text
ApplyCampaignPreferenceChangesCommand
```

A batch is atomic when related Preferences must remain consistent.

## 62. Mutation Input

Recommended fields:

```text
CampaignId
ExpectedCampaignPreferenceVersion
PreferenceChanges
OperationId
ChangeReason
```

## 63. Mutation Validation

Chronicle validates:

- Campaign exists;
- exact package binding;
- Preference definition exists;
- value type;
- bounds;
- option keys;
- editability;
- cross-Preference constraints;
- package compatibility;
- expected version.

## 64. Cross-Preference Constraints

A package may declare constraints such as:

```text
Preference A requires Preference B
Preference C excludes Preference D
Choice X requires threshold within range
```

## 65. Constraint Evaluation

Cross-Preference validation uses an approved deterministic Rule Operation or bounded declarative constraint model.

## 66. No Arbitrary Expressions

Arbitrary scripts or expressions are prohibited in MVP Preference definitions.

## 67. Confirmation Requirement

Material mechanical changes SHOULD require explicit confirmation.

## 68. Change Preview

The UI SHOULD show:

- current value;
- new value;
- affected mechanics;
- affected derived fields;
- affected progression;
- historical non-retroactivity;
- required recalculations;
- package compatibility warnings.

## 69. Change Application

Recommended flow:

```text
Load Current Preference State
    ↓
Resolve Definition
    ↓
Validate Proposed Value
    ↓
Build Proposed Effective Snapshot
    ↓
Run Cross-Preference Validation
    ↓
Determine Affected Projections and Work
    ↓
Acquire Campaign Mutation Coordination
    ↓
Recheck Preference Version and Package Binding
    ↓
Persist Values and History
    ↓
Increment Preference Version
    ↓
Invalidate or Rebuild Dependents
    ↓
Commit
```

## 70. Preference Change History

Every authoritative change appends history.

Recommended fields:

```text
CampaignPreferenceChangeId
CampaignId
PreferenceKey
PreviousEffectiveValue
NewEffectiveValue
PreviousDefinitionVersion
NewDefinitionVersion
PreviousSource
NewSource
OperationId
ChangedAtUtc
PreferenceVersionBefore
PreferenceVersionAfter
ChangeReason
```

## 71. Append-Only History

Preference history is append-only.

Corrections use a new change.

## 72. Clear Explicit Override

A user may remove an explicit override and return to the exact current default.

This is an explicit change with history.

## 73. Clear Behavior

The preview must show the resulting resolved default and definition version.

## 74. No Delete Without Meaning

Deleting an override record is not enough.

The history must preserve that the effective value changed.

## 75. Derived Value Impact

A Preference definition declares whether it affects Character derived values.

## 76. Invalidation

After change, affected derived values become stale or are recalculated according to ADR-0030.

## 77. Progression Impact

A Preference affecting progression changes future calculations.

It does not rewrite accepted ledger entries.

## 78. Dice Impact

A Preference affecting Dice resolution changes future Rolls only.

Accepted Roll evidence preserves the prior Preference version.

## 79. Finalization Impact

A Preference affecting Session finalization applies to finalizations accepted after the change.

## 80. Active Workflow Impact

If a pending Work Item captured an older Preference snapshot:

- execute under the captured version when contract permits;
- migrate or supersede it;
- or mark recovery-required.

It must not silently use new values.

## 81. Pending Dice Request

A pending Dice Roll request SHOULD preserve the Preference snapshot version used to construct the pool.

## 82. Resolution Preference

Roll resolution uses the appropriate captured or current snapshot according to the Rule Operation contract.

The contract must be explicit.

## 83. Mid-Roll Preference Change

Mechanical Preferences affecting an active Roll SHOULD be locked or the Roll marked stale.

## 84. Session Boundary

Some Preferences may declare:

```text
EffectiveImmediately
EffectiveNextScene
EffectiveNextAct
EffectiveNextSession
RequiresCampaignMigration
```

## 85. Effective-Time Policy

The policy is explicit in the definition.

## 86. No Implicit Temporal Semantics

Chronicle does not infer when a Preference begins to apply from its category alone.

## 87. Deferred Activation

A deferred change is persisted as pending configuration with an activation boundary.

## 88. Pending Preference Change

Recommended fields:

```text
PendingPreferenceChangeId
PreferenceKey
RequestedValue
ActivationPolicy
RequestedAtUtc
OperationId
Status
```

## 89. Activation

Activation is an explicit Application operation at the declared boundary.

## 90. MVP Scope for Deferred Activation

The initial MVP MAY support only:

```text
EffectiveImmediately
EffectiveNextSession
```

More granular boundaries may be deferred.

## 91. Safety Preference

Safety Preferences may define:

- disallowed content categories;
- fade-to-black behavior;
- intensity bounds;
- provider transmission wording;
- violation handling.

## 92. Safety Authority

Safety Preferences constrain narration and prompt construction.

They do not depend on provider goodwill alone.

## 93. Safety Change

A safety change SHOULD take effect immediately for new provider requests.

## 94. Safety History Privacy

Safety Preference history is private and should not appear in ordinary diagnostics.

## 95. Provider Transmission Policy

Every narrative or safety Preference declares whether it may be sent to Narrative Intelligence.

## 96. Local-Only Preference

Local-only values affect UI or workflow and never enter provider context.

## 97. Preference Localization

Definitions use localization keys for:

- label;
- description;
- options;
- warnings;
- effect summary.

## 98. Stored Choice Identity

Chronicle stores semantic option keys, not translated labels.

## 99. UI Generation

Preference screens MAY be schema-driven.

## 100. UI Grouping

Definitions may include:

- category;
- section;
- display order;
- advanced flag;
- warning level;
- restart requirement.

## 101. Accessibility

Generated controls must support:

- keyboard access;
- screen-reader labels;
- explicit current and default values;
- noncolor warnings;
- accessible change summaries.

## 102. Rule Set Package Upgrade

A package upgrade may:

- keep a Preference unchanged;
- change its definition;
- add a Preference;
- retire a Preference;
- transform a value;
- change a default;
- change effect policy.

## 103. Preference Migration Contract

A migration SHOULD declare:

```text
SourcePreferenceKey
SourceDefinitionVersion
TargetPreferenceKey
TargetDefinitionVersion
ValueTransformation
DefaultChangePolicy
RetiredValuePolicy
ActivationPolicy
```

## 104. Stable Meaning

If only label or description changes, the PreferenceKey remains.

## 105. Semantic Change

If meaning changes materially, use a new PreferenceKey or explicit value migration.

## 106. Retired Preference

A retired Preference value must be:

```text
Archived
Mapped
Transformed
Blocked
ExplicitlyDiscardedWithCheckpoint
```

## 107. Silent Retirement

Silent loss of explicit Campaign configuration is prohibited.

## 108. Changed Default

Migration must distinguish:

- Campaign used explicit value;
- Campaign relied on old default;
- Campaign should adopt new default;
- Campaign must preserve old effective behavior.

## 109. Preserve Effective Behavior

For mechanical Preferences, package upgrade SHOULD preserve prior effective behavior unless the migration explicitly requires and explains change.

## 110. Materialize Old Default

To preserve behavior, migration may convert an old default into an explicit value.

## 111. New Required Preference

A new required Preference needs:

- deterministic default;
- migration choice;
- user confirmation;
- or blocked upgrade.

## 112. Unknown Legacy Preference

Unknown values are preserved in an archival compatibility record.

## 113. Missing Definition

If an active Campaign value has no matching definition:

- preserve it;
- mark compatibility issue;
- block affected mechanics;
- do not guess a replacement.

## 114. Import

Campaign import includes explicit Preference values, definition versions, and effective-history evidence.

## 115. Clone Import

Clone preserves Preference values while remapping Campaign identity.

## 116. Export

Campaign export includes enough Preference metadata to reproduce future behavior.

## 117. Backup

Installation backup preserves all Preference current state and history.

## 118. Query Model

Preference queries return projections.

## 119. Preference Projection

Recommended fields:

```text
PreferenceKey
Category
DefinitionVersion
ValueType
ExplicitValue
DefaultValue
EffectiveValue
ValueSource
Editability
EffectSummary
RequiresConfirmation
ValidationIssues
PendingChange
```

## 120. No Persistence Entity Exposure

Presentation receives contracts, not EF entities.

## 121. Preference Cache

Effective snapshots MAY be cached by:

```text
CampaignId
CampaignPreferenceVersion
RuleSetPackageVersion
```

## 122. Cache Authority

The cache is nonauthoritative and replaceable.

## 123. Cache Invalidation

Any accepted Preference or package-binding change invalidates the snapshot cache.

## 124. Concurrency

Preference changes use optimistic concurrency on `CampaignPreferenceVersion`.

## 125. Same Preference Concurrent Edit

One succeeds; the stale request receives a conflict.

## 126. Different Preference Concurrent Edit

The MVP may still conflict at set version level for simplicity.

Field-level Preference concurrency is deferred.

## 127. Idempotency

Every Preference change uses OperationId.

## 128. Duplicate Operation

A duplicate committed OperationId returns the existing result.

## 129. Conflicting Operation

The same OperationId with different requested values returns conflict.

## 130. Commit Unknown

Recovery inspects:

- Operation Record;
- Preference history OperationId;
- current Preference version;
- pending activation state.

## 131. Error Model

Recommended errors:

```text
preference.definition-not-found
preference.definition-version-not-found
preference.value-type-invalid
preference.value-out-of-range
preference.option-invalid
preference.required
preference.read-only
preference.cross-constraint-failed
preference.version-conflict
preference.package-incompatible
preference.migration-required
preference.migration-failed
preference.pending-boundary-invalid
preference.snapshot-stale
preference.recovery-required
```

## 132. Data Preservation State

Results SHOULD state:

```text
PreferencesUnchanged
PreferenceChanged
HistoryAppended
PendingChangeCreated
DerivedValuesInvalidated
MigrationRequired
LegacyValueArchived
RecoveryRequired
```

## 133. Logging

Logs MAY include:

- CampaignId;
- PreferenceKey;
- definition version;
- value source;
- category;
- OperationId;
- result code;
- Preference version;
- duration.

They MUST NOT include sensitive free-text safety values or private narrative configuration by default.

## 134. Metrics

Useful metrics include:

```text
PreferenceChangeCount
PreferenceValidationFailureCount
PreferenceConflictCount
PreferenceMigrationCount
PreferenceMigrationFailureCount
PreferenceSnapshotBuildDuration
DerivedInvalidationFromPreferenceCount
```

## 135. Testing Strategy

The implementation requires:

```text
Definition Contract Tests
Value-Type Tests
Resolution Tests
Mutation Tests
Snapshot Tests
Rule Operation Integration Tests
Migration Tests
Import and Export Tests
Security Tests
Architecture Tests
```

## 136. Definition Tests

Tests MUST cover:

- valid definition;
- duplicate key;
- invalid owner;
- unsupported type;
- invalid default;
- invalid bounds;
- invalid option;
- missing localization metadata;
- illegal category and effect combination.

## 137. Resolution Tests

Tests MUST cover:

- explicit value;
- package default;
- Chronicle default;
- cleared override;
- missing value;
- changed default;
- deterministic ordering;
- fingerprint stability.

## 138. Mutation Tests

Tests MUST cover:

- valid change;
- invalid type;
- out-of-range value;
- read-only value;
- cross-constraint failure;
- version conflict;
- batch atomicity;
- duplicate OperationId.

## 139. Mechanical Integration Tests

Tests MUST prove:

- Rule Operation receives declared Preferences only;
- Preference version is recorded;
- changed Preference affects future operation;
- accepted past result remains unchanged;
- stale snapshot is rejected.

## 140. Derived-Value Tests

Tests MUST prove Preference changes invalidate or rebuild declared Character fields.

## 141. Progression Tests

Tests MUST prove changed progression Preferences affect future cost calculations only.

## 142. Deferred Activation Tests

Tests SHOULD cover:

- immediate activation;
- next-Session activation;
- activation boundary;
- cancellation;
- package upgrade while pending.

## 143. Migration Tests

Tests MUST cover:

- label-only change;
- unchanged explicit value;
- old default preserved as explicit;
- new default adopted;
- transformed value;
- retired Preference archive;
- missing required choice;
- blocked migration.

## 144. Import and Export Tests

Tests MUST prove:

- explicit values preserved;
- definition versions preserved;
- clone identity remapping;
- unknown legacy value retained;
- no credential or secret leakage.

## 145. Safety Tests

Tests MUST prove:

- local-only Preference omitted from provider context;
- allowed safety Preference included correctly;
- provider cannot mutate Preference;
- sensitive values not logged.

## 146. Required Test Cases

Tests MUST cover:

- Chronicle-owned Preference;
- Rule Set-owned Preference;
- Boolean;
- integer;
- decimal;
- choice;
- multiple choice;
- structured value;
- explicit override;
- default;
- clear override;
- mechanical change;
- narrative change;
- safety change;
- next-Session activation;
- package migration;
- missing package;
- exact version resolution;
- snapshot evidence;
- no arbitrary prompt Preference.

## 147. Architecture Tests

Architecture tests MUST reject:

- display label used as Preference identity;
- provider directly changing Preferences;
- Rule Set persisting Preference values;
- arbitrary JSON object Preference;
- credential stored as Preference;
- unbounded prompt text as Preference;
- mechanics reading ambient global settings;
- historical result without relevant Preference evidence;
- silent default reinterpretation;
- arbitrary script-based constraint evaluation.

## 148. Prohibited Patterns

### 148.1 Preference Is a UI Setting Only

Authoritative Preferences are Application and Domain-relevant configuration.

### 148.2 Hidden Global Default Changes Campaign Rules

Effective resolution is explicit and versioned.

### 148.3 Provider Changes Preference

Provider output is never Preference authority.

### 148.4 Store Localized Label as Value

Store semantic option key.

### 148.5 Put API Key in Preference

Use the credential boundary.

### 148.6 Recalculate Historical Result Under Current Preferences

Preserve accepted evidence.

### 148.7 Delete Retired Explicit Value Silently

Archive or migrate it.

### 148.8 Pass Every Preference to Every Rule Operation

Pass only declared dependencies.

### 148.9 Apply Material Rule Change Without Confirmation

Show impact and confirm.

### 148.10 Arbitrary Free-Text Prompt Override

Use bounded narrative contracts.

## 149. Alternatives Considered

### One JSON Settings Blob per Campaign

Simple, but weak for type validation, migration, history, and mechanical dependency tracking.

### Global Application Preferences Only

Rejected because different Campaigns may intentionally use different rules.

### Package Defaults Without Persistence Evidence

Rejected because package upgrades could silently change behavior.

### Hardcode Preferences in Rule Operations

Rejected because users need explicit control and inspection.

### Provider Prompt as Preference System

Rejected because prompts are untyped, nondeterministic, and not suitable for mechanical authority.

### Field-Level Preference Concurrency

Deferred because set-level versioning is simpler for the local single-user MVP.

## 150. Consequences

### Positive

- explicit Campaign configuration;
- deterministic mechanics;
- package-safe defaults;
- historical reproducibility;
- schema-driven UI;
- migration without silent meaning loss;
- provider authority remains limited;
- derived and progression dependencies are traceable.

### Negative

- Preference definitions and migrations add package complexity;
- effective snapshot construction requires caching and tests;
- deferred activation introduces workflow state;
- history increases storage;
- preserving old defaults may create explicit values during upgrade;
- safety Preferences require careful privacy treatment.

## 151. Risks

### Default Changes Alter Behavior Unexpectedly

Mitigation:

- exact definition versions;
- preserve-effective-behavior migration;
- explicit preview.

### Preference Dependency Is Omitted

Mitigation:

- operation catalog declarations;
- golden tests;
- snapshot fingerprints;
- package validation.

### Too Many Preferences Overwhelm Users

Mitigation:

- grouping;
- defaults;
- advanced sections;
- effect summaries;
- minimal MVP set.

### Safety Values Leak

Mitigation:

- classification;
- provider transmission policy;
- log redaction;
- contract tests.

### Pending Change Applies at Wrong Boundary

Mitigation:

- explicit activation policy;
- durable pending state;
- boundary integration tests.

## 152. Technology Spike

Before acceptance, implement:

1. Preference definition contract;
2. typed value catalog;
3. Campaign Preference persistence;
4. effective-value resolver;
5. immutable snapshot and fingerprint;
6. change command;
7. history ledger;
8. Rule Operation dependency integration;
9. derived-value invalidation;
10. next-Session activation;
11. package migration example;
12. schema-driven UI projection;
13. import and export mapping;
14. safety transmission policy;
15. architecture tests.

## 153. Spike Acceptance

The spike passes when:

- Chronicle and Rule Set definitions load and validate;
- explicit values and defaults resolve deterministically;
- changing a mechanical Preference affects only future operations;
- accepted past evidence retains the old Preference version;
- invalid values never reach Rule Operations;
- derived fields are invalidated or rebuilt correctly;
- package migration preserves old effective behavior where required;
- unknown legacy values remain preserved;
- providers cannot change Preferences;
- credentials and unrestricted prompts cannot be stored as Preferences.

## 154. Definition of Compliance

An implementation complies when:

- Preferences use stable semantic keys;
- definitions are typed, versioned, and owned;
- Campaign explicit values are separate from defaults;
- effective resolution is deterministic and documented;
- every authoritative change uses Application commands, history, OperationId, and optimistic concurrency;
- Rule Operations receive immutable declared Preference subsets;
- historical mechanical evidence preserves relevant Preference versioning;
- default changes do not silently rewrite history;
- derived values and pending workflows react explicitly;
- package migrations preserve, transform, archive, or block values intentionally;
- providers cannot mutate Preferences;
- credentials and arbitrary prompt overrides are excluded.

## 155. Review Triggers

This ADR must be reviewed if:

- multiplayer introduces per-player Preferences;
- Session- or Scene-scoped overrides become common;
- public third-party packages define Preferences;
- custom prompt templates become a supported feature;
- cloud synchronization merges Preference changes;
- server hosting requires distributed concurrency;
- Preference count causes performance or usability problems;
- safety tooling gains shared-table features;
- account-wide defaults become authoritative;
- mobile clients require a public Preference protocol.

## 156. Deferred Decisions

Later ADRs MAY define:

- per-player Preference scope;
- Character Preference scope;
- Scene- and Act-scoped overrides;
- exact structured-value catalog;
- public package Preference SDK;
- custom prompt-template boundary;
- account-wide suggested defaults;
- field-level Preference concurrency;
- Preference diff and rollback UI;
- synchronization conflict resolution;
- advanced safety-tool contracts.

## 157. Final Decision

Chronicle will treat Campaign Preferences as typed, versioned, explicit configuration with stable semantic identity.

Effective values will be resolved deterministically from Campaign choices and exact package defaults.

Mechanical and narrative workflows will receive immutable Preference snapshots.

Historical outcomes will preserve the Preference version that shaped them.

A Campaign may change how it is played.

Chronicle must always know when that change began, what it affected, and what it did not rewrite.
