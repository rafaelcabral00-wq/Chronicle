---
id: RFC-0028
title: Character Sheet Schema Definition Language
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
---

> **"A Character Sheet schema should describe a game system precisely without turning Chronicle into that game system."**

# Character Sheet Schema Definition Language

## Abstract

This RFC defines the provider-neutral Character Sheet Schema Definition Language used by Rule Set packages.

The schema language describes Character Sheet structure, stable field identity, types, constraints, choices, resources, tracks, composite values, derived values, visibility, editability, progression metadata, localization, validation hooks, migration behavior, and presentation hints.

The schema is declarative where practical.

It does not replace Rule Set code for complex cross-field mechanics.

Chronicle interprets the schema generically while Rule Set packages retain authority over system-specific validation and calculations.

## 1. Purpose

Chronicle requires one generic way to represent Character Sheets without hard-coding the concepts of a particular tabletop RPG system.

The schema language must support:

- Player Characters;
- persistent NPCs;
- different Rule Sets;
- localized labels;
- form generation;
- validation;
- derived values;
- Character snapshots;
- progression;
- migrations;
- provider-safe context projection;
- historical schema interpretation.

Without this contract, every Rule Set would require custom storage, UI, validation, and migration code.

## 2. Scope

This RFC defines:

- schema identity;
- schema versions;
- Character roles;
- sections and groups;
- stable field keys;
- field types;
- scalar fields;
- choice fields;
- resources and tracks;
- references;
- composite fields;
- collections;
- defaults;
- requiredness;
- visibility;
- editability;
- constraints;
- conditional rules;
- derived values;
- normalization;
- progression metadata;
- presentation hints;
- localization;
- provider context hints;
- validation extensions;
- migration metadata;
- compatibility;
- security;
- testing.

This RFC does not define:

- exact JSON, YAML, or code syntax;
- exact UI widget implementation;
- exact database schema;
- every future field type;
- one Rule Set's complete Character Sheet;
- arbitrary executable expressions;
- visual theme design.

## 3. Schema Definition

A `CharacterSheetSchema` is a versioned declarative contract describing the valid structure and generic behavior of a Character Sheet.

Conceptually:

```text
CharacterSheetSchema
├── Identity
├── Character Roles
├── Sections
│   └── Fields
├── Constraints
├── Derived Values
├── Validation Extensions
├── Progression Metadata
├── Localization
├── Presentation Hints
└── Migration Metadata
```

## 4. Schema Authority

The schema is authoritative for:

- field existence;
- field identity;
- generic data type;
- requiredness;
- generic ranges;
- allowed choices;
- basic visibility;
- basic editability;
- declarative defaults;
- generic derivation metadata;
- localization keys.

Rule Set validators remain authoritative for complex mechanical validity.

## 5. Schema Identity

Every schema MUST define:

```text
SchemaId
SchemaVersion
RuleSetId
RuleSetVersionRange
CharacterRoles
```

Example schema identifiers:

```text
werewolf.player-character
werewolf.npc
generic-fantasy.character
```

Schema identifiers MUST be stable and language-neutral.

## 6. Schema Version

Schema version identifies the exact structural contract used to interpret Character Sheet data.

A Character MUST persist:

- SchemaId;
- SchemaVersion.

Breaking structural changes require a new schema version and migration.

## 7. Rule Set Compatibility

A schema MUST declare compatibility with:

- one Rule Set identity;
- one exact Rule Set version or supported version range;
- one Chronicle schema-language version.

Chronicle MUST reject incompatible combinations.

## 8. Schema Language Version

The definition language itself MUST be versioned.

Example:

```text
SchemaLanguageVersion: 1
```

This version governs available field types, operators, and metadata.

## 9. Character Roles

A schema MUST declare which Character roles it supports.

Initial roles include:

```text
Player
Npc
```

Future roles MAY include:

```text
Companion
Minion
TemporaryEntity
Organization
```

The MVP SHOULD implement Player and Npc only.

## 10. One Schema or Multiple Schemas

A Rule Set package MAY define:

- one shared schema with role-specific fields;
- separate Player and NPC schemas;
- several NPC schemas by mechanical depth.

The package MUST identify which schema applies during Character creation.

## 11. Schema Root Metadata

A schema SHOULD contain:

- schema identifier;
- version;
- display name key;
- description key;
- Rule Set reference;
- supported roles;
- default locale;
- sections;
- global constraints;
- derived definitions;
- validator hooks;
- progression metadata;
- migration aliases;
- status.

## 12. Section

A `Section` groups related fields.

A section SHOULD define:

- stable section key;
- label key;
- description key;
- order;
- visibility;
- collapsibility hint;
- child fields;
- optional child groups.

Examples:

```text
identity
attributes
abilities
advantages
background
narrative-profile
```

## 13. Section Identity

Section keys MUST be stable and language-neutral.

Reordering a section MUST NOT change its identity.

## 14. Group

A `Group` is an optional nested presentation and semantic grouping inside a section.

A group MAY define:

- group key;
- label key;
- order;
- layout hint;
- child field references;
- visibility condition.

Groups SHOULD not create additional persistence identity unless explicitly required.

## 15. Field Definition

Every field MUST define:

- stable field key;
- field type;
- role applicability;
- requiredness;
- visibility;
- editability;
- localization key;
- serialization behavior;
- version metadata.

## 16. Stable Field Key

Field keys are the primary machine identity of Character values.

Examples:

```text
identity.name
attributes.strength
resources.willpower.current
narrative.fear
```

Field keys MUST:

- remain stable across localization;
- remain stable across presentation changes;
- be unique within the schema;
- use namespacing where system-specific;
- avoid storage implementation details.

## 17. Field Key Naming

Recommended format:

```text
<section>.<concept>[.<subconcept>]
```

Examples:

```text
attributes.strength
skills.empathy
health.current
identity.pronouns
```

The exact naming convention will be finalized in an ADR.

## 18. Field Type System

The initial type system SHOULD support:

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
Collection
```

A smaller subset MAY be implemented first if the initial Rule Set remains fully supported.

## 19. Integer Field

An Integer field MAY define:

- minimum;
- maximum;
- step;
- default;
- zero allowed;
- display format;
- progression cost metadata.

## 20. Decimal Field

A Decimal field MAY define:

- minimum;
- maximum;
- precision;
- scale;
- step;
- default.

Decimal values SHOULD be avoided for mechanics that require exact integer semantics.

## 21. Boolean Field

A Boolean field defines a true-or-false value.

It MAY include:

- default;
- label keys for true and false;
- visibility;
- editability.

Tri-state behavior requires an explicit optional or choice model.

## 22. Text Field

A Text field SHOULD define:

- minimum length;
- maximum length;
- normalization policy;
- allowed pattern when safe;
- default;
- provider-context inclusion policy.

It is intended for short values.

## 23. LongText Field

A LongText field supports narrative content such as:

- biography;
- beliefs;
- fears;
- goals;
- appearance;
- history.

It SHOULD define:

- maximum length;
- formatting policy;
- provider-context projection;
- prompt-injection treatment;
- visibility.

## 24. Choice Field

A Choice field defines exactly one selected option.

It SHOULD define:

- option set;
- stable option keys;
- labels;
- descriptions;
- optional mechanical metadata;
- default;
- whether custom value is allowed.

Choice identity MUST use option keys, not localized labels.

## 25. MultiChoice Field

A MultiChoice field defines zero or more selected options.

It SHOULD define:

- option set;
- minimum selections;
- maximum selections;
- uniqueness;
- ordering policy;
- custom value policy.

## 26. Option Definition

Each option SHOULD contain:

- stable option key;
- label key;
- description key;
- status;
- availability condition;
- mechanical metadata;
- progression cost;
- provider guidance;
- deprecation metadata.

## 27. Reference Field

A Reference field points to another Chronicle entity or Rule Set definition.

Possible targets include:

- Character;
- Memory;
- Rule Set option;
- equipment definition;
- location definition;
- organization.

A reference MUST declare:

- target type;
- cardinality;
- Campaign ownership rule;
- visibility;
- allowed unresolved state.

## 28. Resource Field

A Resource models a bounded current value and capacity.

Conceptually:

```text
Resource
├── Current
├── Maximum
├── Minimum
└── TemporaryAdjustment
```

It MAY define:

- current default;
- maximum derivation;
- regeneration rule reference;
- spending validation key;
- visibility;
- presentation hint.

## 29. Track Field

A Track models ordered positions or marked boxes.

It MAY define:

- track length;
- marked count;
- named positions;
- overflow policy;
- severity labels;
- fill direction;
- derived capacity;
- reset behavior reference.

## 30. Resource Versus Track

Use Resource when numeric quantity is primary.

Use Track when ordered marked states or boxes are primary.

The Rule Set package decides which semantic model fits.

## 31. Composite Field

A Composite field groups a fixed structure of child values.

Examples:

- damage with type and severity;
- identity name structure;
- temporary effect record;
- equipment item.

A Composite field MUST define:

- child schema;
- required children;
- serialization;
- validation;
- visibility inheritance.

## 32. Collection Field

A Collection field contains repeated structured entries.

Examples:

- equipment;
- merits;
- flaws;
- contacts;
- temporary effects.

It SHOULD define:

- item schema;
- minimum count;
- maximum count;
- uniqueness;
- ordering;
- stable item identity policy;
- add and remove permissions.

## 33. Collection Item Identity

Collection entries requiring persistent references SHOULD have Chronicle-owned item identifiers.

Simple immutable values MAY use value identity.

The schema MUST declare the policy.

## 34. Optional Values

Optionality MUST be explicit.

Chronicle MUST distinguish:

```text
Missing
Null
Empty
Zero
False
```

These values MUST NOT be treated as interchangeable automatically.

## 35. Default Value

A field MAY define a default.

Defaults MUST be:

- deterministic;
- schema-versioned;
- valid under generic constraints;
- applied only at documented lifecycle stages.

Existing Characters MUST not receive new defaults silently after schema updates.

## 36. Requiredness

Requiredness MAY be:

```text
Always
OnCreation
BeforePlay
Conditional
Optional
```

### Always

Value must always exist.

### OnCreation

Required when creating the Character.

### BeforePlay

Draft may omit it, but Campaign play may not begin.

### Conditional

Required only when an explicit condition is true.

## 37. Visibility

Canonical visibility values SHOULD include:

```text
PlayerVisible
PlayerHidden
CharacterScoped
InternalOnly
Conditional
```

Visibility may be refined by application policy.

## 38. Visibility Is Not UI Hiding

Visibility is a trusted application boundary.

Hidden values MUST be filtered before leaving trusted application layers.

The UI MUST not receive all values and hide them cosmetically.

## 39. Editability

Canonical editability values MAY include:

```text
Editable
ReadOnly
CreationOnly
BetweenSessions
RuleSetControlled
Derived
Conditional
```

Editability must be enforced by application use cases.

## 40. Character Role Overrides

A field MAY define role-specific differences.

Example:

```text
Player:
    EditableBetweenSessions

Npc:
    RuleSetControlled
```

Overrides MUST be explicit.

## 41. Generic Constraints

The schema language SHOULD support generic constraints such as:

- required;
- minimum;
- maximum;
- length;
- pattern;
- allowed choices;
- selection count;
- unique collection;
- reference type;
- numeric step;
- value comparison;
- conditional presence.

## 42. Constraint Definition

A constraint SHOULD define:

- constraint key;
- type;
- field references;
- parameters;
- severity;
- error code;
- message key;
- validation stage;
- override policy.

## 43. Constraint Severity

Constraint severity SHOULD be:

```text
Error
Warning
Information
```

An Error blocks the relevant operation.

A Warning requires presentation but may allow continuation.

## 44. Validation Stages

Constraints MAY apply during:

```text
Draft
Creation
BeforePlay
DuringPlay
Progression
Migration
Import
```

A schema MUST not apply creation-only constraints indiscriminately during historical read.

## 45. Conditional Rules

The schema MAY support bounded conditional rules.

Example:

```text
When field A equals option X,
field B becomes required.
```

Supported operators SHOULD be limited and explicit.

## 46. Conditional Operator Set

Initial operators MAY include:

```text
Equals
NotEquals
GreaterThan
GreaterThanOrEqual
LessThan
LessThanOrEqual
Contains
IsPresent
IsEmpty
All
Any
Not
```

Arbitrary code expressions are prohibited.

## 47. Field Comparison

Cross-field declarative comparisons MAY be supported.

Example:

```text
current <= maximum
```

Complex mechanics SHOULD remain in Rule Set validators.

## 48. Constraint Evaluation

Constraint evaluation MUST be:

- deterministic;
- side-effect free;
- bounded;
- versioned;
- safe for untrusted schema content in future dynamic packages.

## 49. Derived Field

A Derived field is calculated from other values.

It SHOULD define:

- field key;
- output type;
- source fields;
- calculation reference;
- persistence policy;
- recalculation triggers;
- visibility;
- editability as Derived;
- calculation version.

## 50. Derived Calculation Forms

A derived calculation MAY be:

```text
Declarative
Executable Rule Set Function
```

Simple arithmetic MAY be declarative.

Complex mechanics SHOULD reference a package-owned deterministic function.

## 51. Declarative Expression Limits

If arithmetic expressions are supported, they MUST use:

- a restricted grammar;
- no reflection;
- no function loading by arbitrary name;
- no filesystem;
- no network;
- no random source;
- bounded execution.

The exact expression language requires an ADR.

## 52. Derived Persistence Policy

A Derived field MAY be:

```text
ComputedOnly
PersistedAndVerified
PersistedSnapshot
```

### ComputedOnly

Calculated whenever read.

### PersistedAndVerified

Stored for performance and checked against calculation.

### PersistedSnapshot

Preserved as a historical value for a specific operation.

## 53. Recalculation Triggers

Triggers MAY include:

- source field changed;
- Campaign Preference changed;
- Rule Set version changed;
- migration;
- progression application;
- temporary state change.

## 54. Circular Derivation

Circular derived dependencies are prohibited.

Schema validation MUST detect cycles.

## 55. Normalization

A field MAY define safe normalization.

Examples:

- trim surrounding whitespace;
- normalize line endings;
- canonicalize option aliases;
- normalize Unicode form;
- remove duplicate empty selections.

Normalization MUST not alter narrative meaning silently.

## 56. Text Sanitization

Chronicle MUST preserve player-authored text while ensuring safe:

- storage;
- rendering;
- prompt construction;
- logging.

Sanitization MUST not be used to rewrite a Character's biography.

## 57. Progression Metadata

A field MAY define progression metadata such as:

- progression category;
- cost reference;
- prerequisite reference;
- maximum rank;
- advancement visibility;
- spend operation key;
- whether direct editing is prohibited.

The Rule Set progression engine remains authoritative.

## 58. Cost Definition

Simple costs MAY be declarative.

Complex costs SHOULD reference a deterministic Rule Set progression function.

A UI MAY display estimated costs.

Only Rule Set validation authorizes spending.

## 59. Prerequisite Definition

A prerequisite MAY use:

- field value;
- selected option;
- Character role;
- Campaign Preference;
- Rule Set capability;
- progression state.

Complex narrative prerequisites require application or Rule Set validation.

## 60. Presentation Hints

The schema MAY include nonauthoritative presentation hints.

Examples:

- recommended control;
- column span;
- grouping;
- compact mode;
- display order;
- icon key;
- help text;
- sensitive input marker;
- multiline preference.

The official application MAY ignore unsupported hints.

## 61. Presentation Hint Safety

Presentation hints MUST NOT affect mechanical meaning.

Two different UIs rendering the same schema MUST preserve the same Character data.

## 62. Recommended Control Types

Possible control hints include:

```text
NumberInput
Checkbox
SingleSelect
MultiSelect
ShortText
LongText
Dots
Boxes
ResourceCounter
ReferencePicker
CollectionEditor
```

These are advisory.

## 63. Localization

Every player-facing label SHOULD use localization keys.

Examples:

- field label;
- field description;
- section label;
- option label;
- validation message;
- help text.

Localized strings MUST remain outside machine identity.

## 64. Missing Localization

When localization is missing, Chronicle MAY:

- use the package default locale;
- display the stable key in developer mode;
- use a generic fallback;
- warn the player.

The Character value remains valid.

## 65. Provider Context Metadata

A field MAY define how it appears in Narrative Intelligence context.

Possible policies:

```text
AlwaysInclude
IncludeWhenRelevant
SummaryOnly
MechanicsOnly
NeverInclude
Sensitive
```

This metadata is advisory to context selection.

It does not bypass visibility.

## 66. Provider Field Description

A schema MAY provide a concise provider-safe semantic description.

It SHOULD explain meaning without embedding copyrighted rule text.

## 67. Prompt Injection Treatment

LongText and player-authored fields MUST always be treated as untrusted data during prompt construction.

A schema MUST NOT mark player-authored content as provider instructions.

## 68. Validation Hook

A schema MAY reference package-owned validation hooks.

A hook SHOULD define:

- stable validator key;
- applicable stage;
- input field set;
- output error codes;
- deterministic behavior;
- validator version.

## 69. Hook Registration

Validator keys MUST be registered by the Rule Set package.

Unknown validators cause package validation failure.

The schema MUST not name arbitrary executable code.

## 70. Hook Input

A validator SHOULD receive only the required structured Character data and validation context.

It MUST not access persistence or Narrative Intelligence.

## 71. Global Validator

A schema MAY declare global Character validators for cross-field rules.

Examples:

- point allocation total;
- mutually exclusive options;
- role-specific requirements;
- progression legality.

## 72. Validation Result

Schema and Rule Set validation SHOULD return:

```text
ValidationResult
├── IsValid
├── Errors
├── Warnings
├── NormalizedValues
├── DerivedValues
└── ValidationVersion
```

## 73. Error Reference

A validation issue SHOULD reference:

- field key;
- collection item when applicable;
- constraint or validator key;
- error code;
- localized message key;
- severity;
- safe parameters.

## 74. Unknown Fields

Unknown persisted fields MUST NOT be silently discarded.

Chronicle SHOULD classify them as:

```text
Known
Deprecated
Migratable
PreservedUnknown
Invalid
```

This protects forward and backward compatibility.

## 75. Additional Properties

The schema SHOULD reject arbitrary undeclared properties by default.

A package MAY define an explicit extension map when necessary.

Extension maps MUST be namespaced and versioned.

## 76. Deprecated Field

A field MAY be deprecated.

Deprecation metadata SHOULD include:

- replacement field;
- deprecation version;
- migration path;
- read behavior;
- edit behavior;
- removal policy.

## 77. Field Alias

A schema MAY declare old keys as aliases for migration or import.

Aliases MUST not create multiple active identities.

The canonical key is persisted after successful migration.

## 78. Schema Migration Metadata

The schema SHOULD declare migration relationships.

A migration entry SHOULD include:

- source SchemaId;
- source version;
- target SchemaId;
- target version;
- migration identifier;
- field mappings;
- removed fields;
- transformed fields;
- default policy;
- warning policy.

## 79. Migration Rules

Migration MUST:

- preserve original data;
- preserve unknown fields when safe;
- avoid silent value loss;
- produce warnings;
- validate the target Character;
- create or require a checkpoint;
- remain deterministic.

## 80. Historical Interpretation

Completed historical records MUST remain interpretable using their original schema and Rule Set versions.

Migration of the active Character MUST not rewrite historical Roll snapshots.

## 81. Schema Compatibility

A schema change is backward-compatible when existing valid Character data remains valid and semantically equivalent without migration.

Examples MAY include:

- adding an optional field;
- adding localization;
- adding a presentation hint;
- adding a warning-only constraint.

## 82. Breaking Schema Changes

Breaking changes include:

- renaming a field key;
- changing field type incompatibly;
- changing requiredness for existing Characters;
- reducing allowed range;
- removing an option;
- changing collection item identity;
- changing derived semantics;
- altering visibility materially.

Breaking changes require a new version and migration policy.

## 83. Schema Registry

Chronicle SHOULD maintain a Character Schema Registry.

It SHOULD support:

- register schema;
- resolve exact schema version;
- validate language version;
- list schemas by Rule Set and role;
- detect duplicate identity;
- resolve migration path;
- report deprecation.

## 84. Exact Resolution

A Character MUST resolve its exact SchemaId and SchemaVersion.

Chronicle MUST not interpret it using the latest schema silently.

## 85. Schema Compilation

Chronicle MAY compile a declarative schema into runtime validators and read models.

Compilation SHOULD produce:

- normalized schema;
- dependency graph;
- derived-field graph;
- constraint plan;
- localization references;
- presentation metadata;
- provider-context metadata.

## 86. Compilation Failure

Schema registration MUST fail when:

- field keys duplicate;
- section references are invalid;
- option keys duplicate;
- derived dependencies cycle;
- validators are unknown;
- constraints are invalid;
- localization keys are malformed;
- migration aliases conflict;
- unsupported field types are used.

## 87. Storage Representation

The schema language MUST not require one database model.

Character values MAY be stored as:

- structured document;
- normalized records;
- hybrid model.

Persistence MUST preserve field identity, type, version, and unknown-value safety.

## 88. Typed Value Representation

Chronicle SHOULD represent values through typed value objects rather than unvalidated generic strings.

Examples:

```text
IntegerValue
ChoiceValue
ResourceValue
CollectionValue
```

The exact implementation depends on technology selection.

## 89. Serialization

Serialization MUST preserve:

- field key;
- field type;
- value;
- schema version;
- collection identity;
- null versus missing;
- unknown fields when policy requires.

## 90. Import and Export

Future import and export SHOULD use the schema contract.

An imported Character MUST:

- identify Rule Set;
- identify schema;
- validate types;
- validate references;
- preserve unknown fields safely;
- avoid executing embedded code.

Import is not required for MVP.

## 91. Security

Schemas are trusted package assets in the MVP.

The schema engine MUST still defend against:

- excessive nesting;
- excessive collection limits;
- cyclic references;
- catastrophic patterns;
- oversized defaults;
- arbitrary executable expressions;
- unknown validators;
- malicious localization content;
- parser ambiguity.

## 92. Resource Limits

The engine SHOULD impose limits for:

- maximum fields;
- maximum nesting depth;
- maximum options;
- maximum collection size;
- maximum expression complexity;
- maximum text length;
- maximum constraint count.

Exact limits require implementation evidence.

## 93. Schema Inspection

Developer tooling SHOULD be able to display:

- schema identity;
- version;
- roles;
- sections;
- fields;
- constraints;
- derived graph;
- validator hooks;
- localization coverage;
- migration paths;
- compilation warnings.

## 94. Testing Strategy

Every schema SHOULD include tests for:

- compilation;
- valid Character;
- invalid Character;
- required fields;
- ranges;
- choices;
- conditional rules;
- derived values;
- cycles;
- visibility;
- editability;
- progression metadata;
- localization;
- migration.

## 95. Golden Character Fixtures

A package SHOULD provide Character fixtures such as:

```text
Minimum Valid Player Character
Typical Valid Player Character
Maximum Boundary Character
Valid Persistent NPC
Invalid Missing-Field Character
Invalid Cross-Field Character
```

## 96. UI Contract Tests

The official application SHOULD test generic rendering for every supported field type.

A schema must remain usable without Rule Set-specific UI components.

## 97. Provider Context Tests

Tests SHOULD verify:

- sensitive fields are excluded;
- relevant fields are selected;
- LongText remains untrusted data;
- localized labels do not replace keys;
- Character snapshots preserve mechanics.

## 98. Migration Tests

Migration tests MUST verify:

- source data preserved;
- target values correct;
- warnings emitted;
- unknown fields handled;
- target validation;
- historical snapshots unchanged.

## 99. Required Test Cases

Tests MUST cover:

- duplicate field key;
- missing section reference;
- invalid option key;
- required creation field;
- before-play requirement;
- conditional required field;
- invalid integer range;
- invalid resource current greater than maximum;
- invalid track length;
- collection maximum;
- duplicate collection item identity;
- reference to another Campaign;
- derived field calculation;
- derived cycle;
- unknown validator hook;
- role-specific editability;
- hidden field filtering;
- provider-context exclusion;
- missing localization fallback;
- deprecated field alias;
- breaking migration;
- preserved unknown field;
- malicious expression;
- excessive nesting;
- unsupported language version.

## 100. Prohibited Patterns

### 100.1 Rule Set Fields in Chronicle Core

Generic Chronicle entities MUST not hard-code system-specific Character fields.

### 100.2 Localized Label as Field Identity

Machine identity MUST use stable keys.

### 100.3 Arbitrary Executable Expression

Schemas MUST not execute unrestricted code.

### 100.4 UI Component as Schema Meaning

Mechanical semantics MUST not depend on one UI widget.

### 100.5 Unknown Field Deletion

Unrecognized persisted data MUST not be silently discarded.

### 100.6 Direct Edit of Derived Field

Derived fields MUST not be edited as normal input.

### 100.7 Schema Automatically Upgrades Character

Exact versions must be resolved and migrations explicit.

### 100.8 Visibility Only in UI

Hidden fields must be filtered in trusted application code.

### 100.9 Player Text as Provider Instruction

Biography and LongText fields remain untrusted data.

### 100.10 One Unlimited Generic Map

The Character Sheet MUST not collapse into an unrestricted key-value blob without schema validation.

## 101. Current Delivery Decision

The MVP adopts:

- versioned Character Sheet schemas;
- stable field and option keys;
- Player and NPC role support;
- sections and groups;
- bounded generic field type system;
- Integer, Boolean, Text, LongText, Choice, MultiChoice, Resource, Track, Composite, and Collection support as target capabilities;
- declarative generic constraints;
- deterministic Rule Set validation hooks;
- derived field references;
- role-aware visibility and editability;
- localization keys;
- progression metadata;
- provider-context metadata;
- schema registry;
- explicit migrations;
- no arbitrary executable expressions;
- no Rule Set-specific UI requirement;
- no unrestricted generic property map.

## 102. Architecture Horizon

Future evolution MAY include:

- visual schema editor;
- community homebrew schemas;
- schema composition;
- reusable field libraries;
- organization and companion schemas;
- dynamic custom field types;
- signed external schemas;
- richer layout metadata;
- schema import and export;
- collaborative Character creation;
- provider-assisted schema authoring.

The MVP MUST NOT implement these capabilities without a later milestone.

## 103. Open Questions

The following remain open:

- What exact serialization format will schemas use?
- Which field types are truly required by the first Rule Set?
- Should Resource and Track be separate runtime types?
- How should Composite and Collection item identity be represented?
- Which conditional operators belong in version 1?
- Should simple derived expressions be supported declaratively?
- Where should schema compilation occur?
- Should unknown fields block play or remain preserved with warnings?
- What maximum schema complexity should be allowed?
- How should presentation hints map to the official desktop UI?
- Which validation stages are required in MVP?
- Should Player and NPC use separate schemas?
- How should provider-context metadata interact with Chronicle Director policies?
- How should Rule Set-specific option catalogs be shared across schemas?
- Which migration tools are required before the first public release?

These questions require RFC-0029, RFC-0030, persistence RFCs, UI RFCs, and technology ADRs.

## 104. Compliance Checklist

A schema-language implementation complies when:

- schema and language versions are explicit;
- exact schema versions are resolved;
- field keys are stable and language-neutral;
- types are bounded and validated;
- requiredness is lifecycle-aware;
- visibility and editability are explicit;
- generic constraints are deterministic;
- complex mechanics use registered Rule Set hooks;
- derived dependencies are cycle-free;
- localized labels remain separate from identity;
- provider-context metadata does not bypass visibility;
- player text remains untrusted data;
- migrations preserve data;
- unknown fields are not silently discarded;
- arbitrary executable expressions are prohibited;
- generic UI rendering remains possible.

## 105. Final Principle

A Character Sheet schema should be expressive enough to describe the Character a game requires.

It should remain constrained enough that Chronicle can validate, render, migrate, remember, and protect that Character generically.
