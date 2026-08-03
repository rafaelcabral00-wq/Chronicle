---
id: RFC-0032
title: Rule Set Preferences and House Rules Contract
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-07-31
category: Contracts
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
  - RFC-0027
  - RFC-0028
  - RFC-0029
  - RFC-0030
  - RFC-0031
---

> **"A house rule may change how a Campaign is played. It must never become an undocumented exception that history can no longer explain."**

# Rule Set Preferences and House Rules Contract

## Abstract

This RFC defines Chronicle's provider-neutral contract for Rule Set Preferences and House Rules.

It establishes how a Rule Set package declares configurable options, how Chronicle validates and persists Campaign-specific selections, how those selections affect mechanics, Character creation, progression, generation, narration, and migration, and how historical operations preserve the exact configuration under which they occurred.

Preferences are structured, versioned, declared, and auditable.

Chronicle MUST NOT accept unrestricted rule-changing data or allow Narrative Intelligence, UI code, or provider prompts to invent mechanical configuration.

## 1. Purpose

Tabletop RPG systems frequently support optional rules, table preferences, tone controls, and campaign-specific mechanical choices.

Without a formal contract, those choices may become:

- hidden booleans in application code;
- provider prompt instructions with no mechanical enforcement;
- unversioned JSON blobs;
- UI-only toggles;
- silent behavior changes;
- incompatible Character data;
- impossible-to-replay historical Rolls;
- migration hazards;
- Rule Set-specific branches in Chronicle Core.

This RFC prevents those outcomes.

## 2. Scope

This RFC defines:

- preference identity;
- preference categories;
- value types;
- defaults;
- allowed values;
- validation;
- dependencies;
- conflicts;
- mechanical impact;
- narrative impact;
- lifecycle;
- mutability;
- Campaign persistence;
- operation snapshots;
- Character compatibility;
- progression compatibility;
- generation guidance;
- provider context;
- migrations;
- historical interpretation;
- UI metadata;
- privacy;
- observability;
- testing.

This RFC does not define:

- one specific Rule Set's preference catalog;
- exact Werewolf house rules;
- final UI controls;
- community package distribution;
- scripting language;
- arbitrary custom formulas;
- multiplayer voting;
- legal policy for third-party house-rule text.

## 3. Core Principle

A Rule Set Preference is valid only when the active Rule Set package declares it.

A House Rule is a declared Rule Set Preference that changes mechanical behavior.

Chronicle MUST NOT treat arbitrary user text as executable rules.

## 4. Terminology

This RFC uses:

```text
Preference
    A declared Campaign-level configuration value.

Rule Preference
    A Preference affecting mechanics or Rule Set validation.

Narrative Preference
    A Preference affecting generation, narration, tone, or presentation.

House Rule
    A declared Rule Preference that modifies default system behavior.

Content Boundary
    A safety or thematic constraint, governed primarily by product policy.

Preference Definition
    Package-owned schema describing one Preference.

Preference Selection
    Campaign-owned chosen value for one Preference Definition.

Preference Snapshot
    Immutable configuration captured for a historical operation.
```

## 5. Preference Ownership

The Rule Set package owns:

- Preference Definitions;
- stable keys;
- value types;
- defaults;
- validation;
- mechanical interpretation;
- compatibility;
- migration.

Chronicle owns:

- Campaign selections;
- identity;
- persistence;
- authorization;
- transaction boundaries;
- history;
- version checks;
- UI delivery.

Narrative Intelligence may receive selected values as context.

It MUST NOT define or change them.

## 6. Preference Definition

A `RuleSetPreferenceDefinition` SHOULD contain:

```text
PreferenceKey
DefinitionVersion
RuleSetReference
Category
ValueType
DefaultValue
AllowedValues
ValidationRules
Dependencies
Conflicts
Mutability
Visibility
MechanicalImpact
NarrativeImpact
CharacterCompatibility
MigrationMetadata
Localization
PresentationHints
Status
```

## 7. Preference Key

A PreferenceKey MUST be:

- stable;
- namespaced;
- language-neutral;
- unique within the Rule Set;
- independent from UI labels;
- independent from implementation class names.

Example:

```text
werewolf.preference.difficulty_mode
```

## 8. Definition Version

DefinitionVersion identifies the semantics of one Preference.

A breaking change requires a new version when it changes:

- value type;
- allowed values;
- default behavior;
- mechanical meaning;
- compatibility;
- migration behavior;
- lifecycle mutability.

## 9. Preference Categories

Initial categories MAY include:

```text
Mechanical
CharacterCreation
Progression
Narrative
CampaignGeneration
Difficulty
Visibility
OptionalSubsystem
Presentation
Accessibility
```

A Preference MAY belong to more than one semantic category, but one primary category SHOULD be declared.

## 10. Mechanical Preference

A Mechanical Preference changes Rule Set execution.

Examples:

- alternate difficulty model;
- optional damage rule;
- critical-result behavior;
- modifier cap;
- resource recovery variant.

Mechanical Preferences MUST be included in authoritative operation snapshots.

## 11. Character Creation Preference

This category affects:

- required fields;
- allowed options;
- initial point allocation;
- Character role restrictions;
- starting resources;
- initial progression.

It MUST be selected before Character validation when applicable.

## 12. Progression Preference

This category affects:

- award policy;
- costs;
- milestones;
- maximums;
- automatic advancement;
- correction rules.

It MUST be preserved in Award and Advancement calculations.

## 13. Narrative Preference

This category affects:

- prose style;
- thematic emphasis;
- terminology presentation;
- visible mechanics;
- pacing guidance;
- failure framing.

A Narrative Preference MUST NOT silently alter mechanical outcomes.

## 14. Campaign Generation Preference

This category affects:

- Campaign scale;
- NPC density;
- mystery emphasis;
- expected campaign length;
- initial Act detail;
- thematic focus.

It becomes input to the Campaign Generator.

## 15. Difficulty Preference

Difficulty Preferences MAY alter:

- default thresholds;
- opposition assumptions;
- modifier policies;
- challenge calibration;
- automatic result boundaries.

They remain Rule Set-validated.

## 16. Visibility Preference

Visibility Preferences MAY control:

- public Dice Roll details;
- modifier explanations;
- hidden target information;
- mechanical result detail;
- Character Sheet field visibility.

They MUST NOT weaken trusted application filtering.

## 17. Optional Subsystem Preference

A Rule Set MAY declare optional subsystems.

Examples:

- detailed equipment;
- extended social conflict;
- alternate injury system;
- downtime subsystem.

A disabled subsystem's operations and fields SHOULD become unavailable explicitly.

## 18. Presentation Preference

Presentation Preferences affect only rendering or explanation.

Examples:

- numeric versus symbolic display;
- compact Dice breakdown;
- terminology verbosity.

They MUST NOT affect mechanical state.

## 19. Accessibility Preference

Accessibility Preferences MAY affect:

- text density;
- motion;
- audio description;
- color-independent symbols;
- timing presentation.

They belong primarily to the application, but a Rule Set MAY expose semantic presentation choices.

## 20. Value Types

The initial Preference value system SHOULD support:

```text
Boolean
Integer
Decimal
Choice
MultiChoice
Text
Structured
```

The MVP SHOULD prefer Boolean, Integer, Choice, and MultiChoice.

## 21. Boolean Preference

A Boolean Preference defines:

```text
Enabled
Disabled
```

It SHOULD still provide stable labels and a declared default.

## 22. Integer Preference

An Integer Preference SHOULD define:

- minimum;
- maximum;
- step;
- default;
- numeric safety;
- mechanical meaning.

## 23. Decimal Preference

A Decimal Preference SHOULD define:

- minimum;
- maximum;
- precision;
- scale;
- step;
- rounding.

Decimal Preferences SHOULD be avoided when exact integer semantics are sufficient.

## 24. Choice Preference

A Choice Preference defines one selected option.

Each option MUST have:

- stable option key;
- label key;
- description key;
- mechanical metadata;
- status;
- compatibility;
- migration aliases.

## 25. MultiChoice Preference

A MultiChoice Preference SHOULD define:

- stable options;
- minimum selections;
- maximum selections;
- uniqueness;
- ordering significance;
- conflicts.

## 26. Text Preference

Text SHOULD be used only for nonmechanical bounded configuration.

Examples:

- custom terminology label;
- Campaign generation note.

Text MUST NOT be interpreted as executable mechanics.

## 27. Structured Preference

A Structured Preference MAY model a fixed typed object.

It MUST define:

- bounded schema;
- field keys;
- types;
- constraints;
- migration.

An unrestricted map is prohibited.

## 28. Default Value

Every Preference SHOULD define a default unless explicit selection is mandatory.

Defaults MUST be:

- deterministic;
- versioned;
- valid;
- compatible with the Rule Set;
- visible during Campaign setup.

## 29. Explicit Selection

A Preference MAY require explicit selection.

This SHOULD be used when:

- no safe default exists;
- legal or content implications differ;
- mechanical behavior changes substantially;
- migration would be ambiguous.

## 30. Default Application

Defaults are applied when a Campaign first initializes the Preference catalog.

A package update MUST NOT silently replace an already persisted selection with a new default.

## 31. Preference Selection

A `CampaignPreferenceSelection` SHOULD contain:

- Campaign identifier;
- PreferenceKey;
- DefinitionVersion;
- selected value;
- source;
- selected timestamp;
- selected by;
- selection version;
- status;
- visibility.

## 32. Selection Source

Possible sources include:

```text
Default
PlayerSelected
Imported
Migrated
RuleSetRequired
ApplicationRecommended
```

The source is diagnostic.

It does not change mechanical authority.

## 33. Campaign Scope

Rule Set Preferences are Campaign-scoped by default.

They MUST NOT leak into another Campaign.

User-level defaults MAY prefill Campaign setup but do not become Campaign state until accepted.

## 34. Character Scope

Some options may appear Character-specific.

Those SHOULD be modeled as Character Sheet fields or Character State unless they genuinely configure the entire Campaign.

The Rule Set package decides the correct ownership.

## 35. Session Scope

Temporary Session-level mechanical options SHOULD use explicit Session configuration or state.

They MUST NOT overwrite Campaign Preferences silently.

The MVP MAY omit Session-scoped Rule Set Preferences.

## 36. Preference Set

A `CampaignPreferenceSet` SHOULD contain:

- Campaign identifier;
- Rule Set identity;
- Rule Set version;
- catalog version;
- selections;
- set version;
- validation status;
- created timestamp;
- updated timestamp.

## 37. Catalog Version

A Rule Set package SHOULD expose a Preference Catalog version.

The catalog identifies the complete set of Preference Definitions for one Rule Set package version.

## 38. Exact Catalog Resolution

Chronicle MUST resolve the exact Preference catalog compatible with the Campaign's Rule Set version.

It MUST NOT silently use a newer incompatible catalog.

## 39. Preference Validation

Validation MUST verify:

- Preference exists;
- definition version is supported;
- value type;
- range;
- allowed options;
- required selection;
- dependencies;
- conflicts;
- Rule Set compatibility;
- Character compatibility;
- lifecycle mutability;
- Campaign ownership.

## 40. Validation Result

Validation SHOULD return:

```text
PreferenceValidationResult
├── IsValid
├── Errors
├── Warnings
├── NormalizedValue
├── AffectedCapabilities
├── RequiredMigrations
└── ValidationVersion
```

## 41. Dependencies

A Preference MAY depend on another Preference.

Example:

```text
advanced_combat_options
requires
combat_mode = detailed
```

Dependencies MUST be explicit and acyclic.

## 42. Conflicts

A Preference MAY conflict with another selection.

Example:

```text
automatic_successes
conflicts with
strict_failure_mode
```

Chronicle MUST reject invalid combinations.

## 43. Conditional Availability

A Preference or option MAY be available only when:

- another Preference has a value;
- a Rule Set capability exists;
- Campaign is still Draft;
- a Character schema supports it;
- a package extension is installed.

## 44. Dependency Graph

The Preference catalog SHOULD compile into a dependency graph.

Registration MUST fail when the graph contains:

- cycles;
- missing references;
- contradictory required values;
- impossible defaults.

## 45. Preference Status

Canonical definition statuses MAY include:

```text
Active
Experimental
Deprecated
Unavailable
InternalOnly
```

## 46. Experimental Preference

Experimental Preferences SHOULD be clearly marked.

The application MAY require explicit acknowledgement.

Historical operations must still preserve exact values.

## 47. Deprecated Preference

A deprecated Preference SHOULD define:

- replacement;
- deprecation reason;
- migration path;
- read behavior;
- edit behavior;
- removal policy.

## 48. Mutability

Canonical mutability values MAY include:

```text
CreationOnly
BeforeFirstSession
BetweenSessions
AnyTimeWhenIdle
MigrationOnly
ReadOnly
```

## 49. Creation-Only Preference

A CreationOnly Preference cannot change after Campaign creation completes.

Examples may include foundational mechanical modes.

Changing it later requires migration or a new Campaign.

## 50. Before-First-Session Preference

This Preference may change while Campaign is prepared but freezes once play begins.

## 51. Between-Sessions Preference

This Preference may change only when:

- no Session is active;
- no finalization is pending;
- no mechanical operation depends on the old value;
- required migration succeeds.

## 52. Idle-Time Preference

`AnyTimeWhenIdle` requires no active conflicting operation.

The change still creates a versioned Campaign update.

## 53. Read-Only Preference

A ReadOnly Preference may represent:

- package-selected mode;
- edition-specific constant;
- imported compatibility marker.

It is visible but not user-editable.

## 54. Preference Change Request

A `ChangeCampaignPreferenceRequest` SHOULD contain:

```text
OperationId
CampaignId
ExpectedCampaignVersion
ExpectedPreferenceSetVersion
PreferenceKey
ExpectedCurrentValue
RequestedValue
RuleSetReference
PlayerAuthorization
```

## 55. Change Validation

Before accepting a change, Chronicle MUST validate:

- lifecycle;
- current value;
- requested value;
- dependencies;
- conflicts;
- affected Characters;
- active operations;
- migration requirements;
- Rule Set compatibility;
- authorization.

## 56. Change Impact Analysis

The Rule Set SHOULD provide an impact analysis.

It MAY identify:

- changed operation behavior;
- affected Character fields;
- invalid Character values;
- progression recalculation risk;
- active Scene incompatibility;
- index rebuild need;
- Narrative Plan guidance change;
- migration requirement;
- historical interpretation impact.

## 57. Impact Categories

Possible categories include:

```text
PresentationOnly
NarrativeOnly
FutureMechanicsOnly
CharacterValidationChange
RequiresCharacterMigration
RequiresCampaignMigration
IncompatibleWithExistingHistory
```

## 58. Mechanical Impact

Every mechanical Preference Definition MUST declare which capabilities may be affected.

Examples:

- operation keys;
- pool calculations;
- difficulty;
- result interpretation;
- progression;
- Character validation;
- consequence policy.

## 59. Preference Snapshot

Every authoritative mechanical operation SHOULD preserve a Preference Snapshot or an immutable reference to one.

The snapshot SHOULD include:

- Preference set version;
- relevant selected values;
- definition versions;
- Rule Set version;
- fingerprint.

## 60. Minimal Snapshot

Chronicle MAY store only Preferences relevant to the operation.

It MUST still preserve enough information to replay the exact behavior.

## 61. Snapshot Fingerprint

A Preference fingerprint SHOULD be deterministic over:

- relevant keys;
- definition versions;
- normalized values;
- Rule Set version.

It supports replay and diagnostics.

## 62. Dice Operation Integration

A Dice Roll MUST preserve relevant mechanical Preferences.

A later Preference change MUST NOT reinterpret the historical Roll.

## 63. Progression Integration

Awards and Advancement MUST preserve relevant progression Preferences.

A later cost-table mode change MUST NOT alter historical spending.

## 64. Character Validation Integration

Character validation MUST use the current authoritative Preference Set.

Validation results SHOULD record the Preference set version.

## 65. Campaign Generation Integration

The Campaign Generator SHOULD receive selected generation and narrative Preferences.

It MUST distinguish:

- mechanical requirements;
- narrative guidance;
- public settings;
- hidden internal configuration.

## 66. Narrator Integration

The Narrator MAY receive Preferences relevant to:

- visible mechanics;
- style;
- difficulty framing;
- terminology;
- subsystem availability.

It MUST NOT receive unrelated configuration.

## 67. Archivist Integration

The Archivist MAY receive Preferences relevant to:

- progression criteria;
- finalization policy;
- visible summary style;
- optional subsystem interpretation.

It MUST not reinterpret mechanical settings independently.

## 68. Prompt Construction

Prompt Builder treats Preferences as structured authoritative context.

Player-authored text Preferences remain untrusted data.

Mechanical Preferences SHOULD be represented clearly and concisely.

## 69. House Rule Definition

A House Rule is a Preference Definition with:

```text
Category = Mechanical or Progression or OptionalSubsystem
ChangesDefaultSystemBehavior = true
```

It MUST declare the default system behavior it changes.

## 70. House Rule Metadata

A House Rule SHOULD include:

- rationale;
- default behavior summary;
- changed behavior summary;
- affected operations;
- affected Character fields;
- affected progression;
- migration policy;
- historical compatibility;
- warning level.

## 71. Built-In House Rules

The MVP MAY support package-declared built-in House Rules.

They are part of the Rule Set package and pass normal contract tests.

## 72. User-Defined House Rules

Arbitrary user-authored mechanical rules are outside the MVP.

Future support requires:

- a bounded declarative language;
- validation;
- sandboxing;
- migration;
- replay;
- UI tooling;
- security review.

## 73. No Free-Form Mechanical Interpretation

A text note such as:

```text
Make combat more realistic
```

MUST NOT alter mechanics.

It may be stored as narrative guidance only when explicitly modeled that way.

## 74. Preference Recommendations

The application or Rule Set MAY recommend selections.

Recommendations MUST be:

- nonauthoritative;
- explainable;
- separate from defaults;
- visible to the player.

Narrative Intelligence MAY suggest a Preference only through a bounded advisory workflow.

## 75. Provider Recommendation Boundary

Narrative Intelligence MUST NOT directly modify Preferences.

A provider recommendation SHOULD contain:

- PreferenceKey;
- proposed value;
- reason;
- affected experience;
- confidence.

Chronicle validates and requests player confirmation.

## 76. Atomic Preference Change

A Preference change and required migrations SHOULD commit atomically where practical.

The transaction MAY include:

- Preference update;
- Character migration;
- Campaign validation;
- read-model invalidation;
- Domain Events.

## 77. Long Migration

If migration cannot fit one transaction:

- create checkpoint;
- block play;
- persist migration operation;
- preserve old Preference Set;
- publish new state only after success;
- recover through RFC-0019.

## 78. Preference Change Event

A successful change SHOULD emit a trusted Domain Event such as:

```text
CampaignPreferenceChanged
```

The event SHOULD include:

- key;
- prior value;
- new value;
- definition version;
- Preference set version;
- impact summary;
- migration reference.

## 79. Historical Interpretation

Historical records MUST retain the configuration relevant when they occurred.

This includes:

- Dice Rolls;
- progression Awards;
- Advancement;
- Character validation snapshots;
- migrations;
- generated Campaign proposal where mechanically relevant.

## 80. Latest Preference for Current Play

Current play uses the active Preference Set.

Historical display uses recorded snapshots.

The two MUST not be confused.

## 81. Preference Migration

A package MAY define migration for:

- renamed key;
- changed option key;
- changed value type;
- split Preference;
- merged Preferences;
- removed Preference;
- changed default semantics.

## 82. Migration Entry

A migration SHOULD contain:

- migration identifier;
- source catalog version;
- target catalog version;
- source key and definition version;
- target keys and versions;
- transformation;
- warning;
- fallback policy;
- compatibility classification.

## 83. Migration Requirements

Preference migration MUST:

- be deterministic;
- preserve source selection;
- avoid silent fallback;
- produce warnings;
- validate target set;
- preserve historical snapshots;
- avoid provider calls;
- avoid arbitrary code.

## 84. Removed Preference

When a Preference is removed, migration MUST define whether it is:

- replaced;
- absorbed into default behavior;
- preserved as legacy read-only metadata;
- incompatible.

Chronicle MUST not discard it silently.

## 85. Changed Default

Changing a default affects only new Campaigns unless an explicit migration says otherwise.

Existing Campaign selections remain unchanged.

## 86. Imported Campaign

An imported Campaign MUST identify:

- Rule Set version;
- Preference catalog version;
- selections;
- unknown Preferences;
- historical snapshots when available.

Unknown selections MUST be preserved or rejected explicitly.

## 87. Unknown Preference

Unknown Preferences SHOULD be classified as:

```text
PreservedUnknown
Migratable
Deprecated
Invalid
```

They MUST not affect mechanics until resolved.

## 88. Visibility

Preference visibility MAY be:

```text
PlayerVisible
PlayerHidden
InternalOnly
DeveloperOnly
```

Mechanical Preferences affecting the Player Character SHOULD normally be visible.

## 89. Hidden Preference

A hidden Preference MUST not conceal unfair mechanical behavior without explicit product and Rule Set justification.

The application SHOULD expose safe operational consequences even when internal details remain hidden.

## 90. Localization

Every player-facing Preference and option SHOULD use localization keys.

Machine keys and persisted values remain language-neutral.

## 91. Presentation Hints

Preference Definitions MAY include:

- recommended control;
- grouping;
- order;
- help text;
- warning presentation;
- advanced-setting marker;
- restart or migration indicator.

These hints are nonauthoritative.

## 92. UI Grouping

Suggested UI groups MAY include:

```text
Core Rules
Difficulty
Character Creation
Progression
Optional Systems
Narrative
Visibility
Advanced
```

The UI MAY choose another layout without changing semantics.

## 93. Change Preview

Before applying a meaningful Preference change, the application SHOULD show:

- current value;
- requested value;
- mechanical impact;
- Character impact;
- migration requirement;
- reversibility;
- whether play will be blocked.

## 94. Confirmation

Explicit confirmation SHOULD be required when a change:

- alters mechanics;
- requires migration;
- invalidates Character data;
- affects progression;
- cannot be safely reversed;
- changes hidden-information behavior.

## 95. Reversion

A Preference may be changed back only when normal validation permits it.

Reversion is a new change.

It MUST NOT erase prior configuration history.

## 96. Preference History

Chronicle SHOULD preserve a history of accepted Preference changes.

A history record SHOULD include:

- OperationId;
- Preference key;
- prior value;
- new value;
- definition versions;
- actor;
- timestamp;
- impact;
- migration;
- result.

## 97. Error Model

Recommended errors include:

```text
PreferenceUnknown
PreferenceVersionUnsupported
PreferenceValueInvalid
PreferenceDependencyMissing
PreferenceConflict
PreferenceChangeNotAllowed
PreferenceMigrationRequired
PreferenceMigrationFailed
PreferenceSetVersionConflict
CharacterIncompatible
ActiveOperationConflict
PlayerConfirmationRequired
```

## 98. Retry Semantics

Typical behavior:

```text
Version conflict
    → SafeAfterRefresh

Transient migration failure
    → SafeWithSameOperationId

Invalid requested value
    → NotRetryable without changed input

Commit uncertainty
    → Query Preference history by OperationId

Player confirmation missing
    → RequiresUserDecision
```

## 99. Failure After Migration Preparation

If migration preparation succeeded but commit failed:

- preserve prepared output;
- verify authoritative versions;
- retry with same OperationId;
- do not apply partial Preference state.

## 100. Failure After Commit

If the Preference change committed but confirmation was lost:

- retry returns the existing result;
- migration does not repeat;
- history is not duplicated;
- Preference version remains stable.

## 101. Concurrency

The MVP SHOULD allow one state-changing Preference operation per Campaign at a time.

Optimistic version checks remain required.

## 102. Security

Preference handling MUST defend against:

- arbitrary keys;
- malformed values;
- oversized Structured Preferences;
- recursive structures;
- cross-Campaign updates;
- forged definition versions;
- provider-generated persistent IDs;
- unregistered validators;
- hidden code execution;
- integer overflow;
- invalid option aliases.

## 103. Resource Limits

Chronicle SHOULD limit:

- Preference count;
- option count;
- dependency depth;
- Structured Preference size;
- validation duration;
- migration duration;
- history payload size.

## 104. Observability

Chronicle SHOULD record:

- Campaign;
- Rule Set version;
- catalog version;
- Preference key;
- prior and new values in safe form;
- impact category;
- migration identifier;
- validation duration;
- transaction result;
- blocking state;
- error code.

## 105. Logging Safety

Logs SHOULD NOT expose:

- private content-boundary details unnecessarily;
- unrestricted player text;
- provider payloads;
- hidden Campaign Secrets;
- proprietary source text.

## 106. Metrics

Useful metrics include:

- Preference changes by key;
- migration frequency;
- invalid-selection rate;
- conflict rate;
- Character incompatibility rate;
- abandoned changes;
- most-used built-in House Rules;
- version-conflict rate.

Metrics MUST not alter defaults automatically.

## 107. Testing Strategy

### 107.1 Catalog Tests

Test:

- stable keys;
- defaults;
- types;
- options;
- dependencies;
- conflicts;
- cycles;
- localization.

### 107.2 Mechanical Tests

Test the same mechanical operation under each supported mechanical Preference.

### 107.3 Migration Tests

Test catalog updates and Character compatibility.

### 107.4 History Tests

Test exact historical replay with prior Preference snapshots.

## 108. Golden Preference Fixture

A golden fixture SHOULD contain:

```text
Given:
    Rule Set version
    Preference catalog version
    selected values
    Character snapshot
    mechanical operation

Expect:
    valid Preference Set
    calculated mechanical behavior
    snapshot fingerprint
```

## 109. Required Test Cases

Tests MUST cover:

- valid default set;
- missing mandatory selection;
- invalid Boolean type;
- invalid numeric range;
- unknown choice option;
- MultiChoice maximum;
- dependency satisfied;
- dependency missing;
- conflict;
- dependency cycle;
- CreationOnly change after play;
- BetweenSessions change during active Session;
- presentation-only change;
- mechanical change;
- Character invalidated by change;
- migration required;
- migration failure;
- changed default preserving existing Campaign;
- deprecated option alias;
- removed Preference;
- unknown imported Preference;
- provider recommendation requiring confirmation;
- duplicate OperationId;
- version conflict;
- failure after commit;
- historical Dice Roll using prior snapshot;
- progression using prior Preference version;
- cross-Campaign change;
- oversized Structured Preference.

## 110. Prohibited Patterns

### 110.1 Arbitrary Preference Map

Campaign mechanics MUST NOT depend on unrestricted key-value data.

### 110.2 Prompt-Only House Rule

Mechanical behavior MUST be implemented and validated by the Rule Set package.

### 110.3 UI-Only Preference

The UI MUST NOT be the sole enforcer of a Rule Preference.

### 110.4 Silent Default Update

Existing Campaigns MUST not change because a package default changed.

### 110.5 Unversioned Mechanical Selection

Historical operations must preserve the relevant Preference version.

### 110.6 Provider Changes Preferences Directly

Narrative Intelligence may only advise through a validated workflow.

### 110.7 Localized Option as Identity

Persist stable option keys, not labels.

### 110.8 Preference Change Rewrites History

Past Rolls, Awards, and Advances retain their original snapshots.

### 110.9 Free-Form Text Executes Mechanics

Text Preferences are nonexecutable.

### 110.10 Hidden Rule Set Branch in Chronicle Core

Preference interpretation remains inside the Rule Set package.

## 111. Current Delivery Decision

The MVP adopts:

- Campaign-scoped Rule Set Preferences;
- package-declared Preference catalog;
- stable namespaced keys;
- Boolean, Integer, Choice, and MultiChoice as primary types;
- versioned defaults and definitions;
- explicit dependencies and conflicts;
- lifecycle-aware mutability;
- exact catalog resolution;
- mechanical impact declaration;
- Preference snapshots for Rolls and progression;
- player confirmation for material changes;
- built-in package-declared House Rules only;
- deterministic migrations;
- Preference history;
- no arbitrary user-defined mechanical language;
- no prompt-only mechanics;
- no silent latest-default behavior.

## 112. Architecture Horizon

Future evolution MAY include:

- bounded user-authored House Rule DSL;
- community Preference packs;
- multiplayer voting;
- Campaign templates;
- shared user defaults;
- provider-assisted recommendations;
- visual impact comparison;
- package marketplace options;
- rule compatibility analyzers;
- reversible experimental branches;
- per-Session variants.

The MVP MUST NOT implement these capabilities without a later milestone.

## 113. Open Questions

The following remain open:

- Which Preferences are required by the initial Rule Set?
- Which value types are necessary for MVP?
- Should content boundaries share this contract or remain separate?
- How should Preference snapshots be persisted efficiently?
- Which changes require full Campaign migration?
- Should the official application expose advanced and experimental Preferences?
- How should Character incompatibility be repaired?
- Which Preference changes may occur between Sessions?
- Should Narrative Preferences live in the Rule Set package or product configuration?
- How should recommendation workflows be presented?
- Which built-in House Rules are needed for the first package?
- Should catalog compilation be code-based or data-based?
- How much Preference history should be visible to players?
- How should package upgrades present changed defaults?
- Which mechanical impact metadata must be machine-enforced?

These questions require persistence RFCs, UI RFCs, the first Rule Set package, and technology ADRs.

## 114. Compliance Checklist

An implementation complies when:

- every Preference is package-declared;
- keys and options are stable and language-neutral;
- definitions and catalogs are versioned;
- defaults do not overwrite existing selections;
- dependencies and conflicts are validated;
- mutability respects Campaign lifecycle;
- mechanical Preferences declare impact;
- Character compatibility is checked;
- historical operations preserve relevant snapshots;
- House Rules are structured and executable only through Rule Set logic;
- providers cannot modify Preferences directly;
- migrations are deterministic;
- unknown imported Preferences are not silently executed;
- material changes require authorization;
- history is preserved.

## 115. Final Principle

A Campaign may choose how its Rule Set is played.

Chronicle must ensure that every choice is declared, validated, remembered, and mechanically real.
