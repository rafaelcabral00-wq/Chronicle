---
id: RFC-0014
title: Rule Set Domain Contract
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-07-31
category: Domain
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
---

> **"Chronicle owns the Campaign. The Rule Set defines how that Campaign obeys its game."**

# Rule Set Domain Contract

## Abstract

This RFC defines the generic Rule Set contract of Chronicle.

It establishes the responsibilities, boundaries, capabilities, lifecycle, versioning, validation behavior, Character Sheet schema, test resolution, progression, terminology, knowledge retrieval, campaign customization, and migration expectations of a supported tabletop RPG system.

The first development Rule Set is Werewolf: The Apocalypse.

Werewolf-specific rules MUST remain outside the generic Chronicle domain.

## 1. Purpose

Chronicle is intended to support more than one tabletop RPG system over time.

The Chronicle Core MUST remain stable when a new Rule Set is introduced.

A Rule Set provides system-specific meaning for:

- Character Sheets;
- validation;
- tests;
- dice mechanics;
- progression;
- temporary effects;
- terminology;
- supported actions;
- campaign creation constraints;
- relevant rules knowledge.

The Rule Set contract prevents system-specific concepts from leaking into the generic domain.

## 2. Scope

This RFC defines:

- Rule Set identity;
- Rule Set version;
- capabilities;
- Character Sheet schema;
- structural and mechanical validation;
- operation catalog;
- test and dice resolution;
- deterministic outcomes;
- progression;
- Character State extensions;
- Relationship extensions;
- Campaign Preferences;
- terminology;
- rule knowledge retrieval boundary;
- package expectations;
- compatibility;
- migration;
- registration;
- failure behavior;
- licensing boundary.

This RFC does not define:

- the complete Werewolf Rule Set;
- exact package file format;
- exact programming language interfaces;
- database schema;
- retrieval implementation;
- copyrighted rule content;
- provider prompt design;
- community marketplace behavior.

## 3. Rule Set Definition

A `RuleSet` is the domain abstraction representing one supported tabletop RPG system.

A Rule Set MUST provide the system-specific behavior required by Chronicle without redefining Chronicle's core entities.

The Rule Set answers questions such as:

```text
What fields define a Character?
Is this Character valid?
What test applies to this action?
How is the dice pool calculated?
How are raw dice interpreted?
How does progression work?
Which terms should the Narrator use?
```

## 4. Rule Set Is Not RAG

A Rule Set is a domain capability.

RAG is one possible infrastructure technique used to retrieve relevant rules knowledge.

```text
RuleSet
    = domain contract

Rule Knowledge Retrieval
    = application capability

RAG
    = optional infrastructure implementation
```

The Domain MUST NOT depend on:

- embeddings;
- vector databases;
- document chunks;
- similarity scores;
- provider file stores.

## 5. Rule Set Identity

Every Rule Set MUST have a stable identifier.

Examples:

```text
werewolf-the-apocalypse
generic-fantasy
custom-system
```

The identifier MUST remain stable across:

- versions;
- localization;
- package updates;
- UI labels;
- persistence.

Display names MAY change.

## 6. Rule Set Version

Every Rule Set MUST have an explicit version.

A version identifies the complete behavior used by a Campaign, including:

- Character Sheet schema;
- validation rules;
- operation catalog;
- dice mechanics;
- progression;
- terminology;
- compatibility.

A Campaign MUST persist both:

```text
RuleSetId
RuleSetVersion
```

## 7. Rule Set Version Semantics

Rule Set versions SHOULD follow a consistent versioning policy.

The exact policy will be selected later.

Conceptually:

```text
Major:
Breaking mechanical or schema change

Minor:
Backward-compatible capability or content addition

Patch:
Correction that preserves Campaign compatibility
```

A Rule Set package version and sourcebook edition are related but not necessarily identical.

## 8. Immutable Campaign Association

A Campaign MUST use one Rule Set identity during normal MVP operation.

A Campaign MUST NOT silently switch Rule Sets.

A Campaign MAY migrate to a newer version only through an explicit migration workflow.

Historical Dice Rolls and completed Sessions retain their original Rule Set version references.

## 9. Rule Set Capabilities

A Rule Set SHOULD declare capabilities explicitly.

Possible capabilities include:

```text
CharacterSheetSchema
CharacterValidation
DiceTests
DeterministicTests
Progression
RelationshipMechanics
KnowledgeMechanics
TemporaryEffects
CampaignPreferences
RuleKnowledge
NpcGenerationGuidance
CampaignGenerationGuidance
Localization
Migration
```

Chronicle MUST NOT assume every Rule Set implements every optional capability.

## 10. Capability Declaration

A Rule Set descriptor SHOULD include:

- Rule Set identifier;
- version;
- display name;
- edition;
- supported languages;
- capability list;
- minimum Chronicle contract version;
- package metadata;
- licensing metadata;
- status.

Possible statuses:

```text
Development
Experimental
Supported
Deprecated
Unavailable
```

## 11. Required MVP Capabilities

The first Rule Set MUST support:

- Character Sheet schema;
- Character structural validation;
- at least advisory mechanical validation;
- stable operation keys;
- Dice Roll resolution;
- Rule Set terminology;
- progression guidance;
- rule knowledge retrieval;
- NPC generation guidance;
- Campaign generation guidance.

## 12. Character Sheet Schema Contract

A Rule Set MUST provide a Character Sheet schema compatible with RFC-0007.

The schema SHOULD define:

- schema identifier;
- schema version;
- sections;
- fields;
- stable field keys;
- data types;
- labels;
- localization keys;
- defaults;
- required fields;
- editable fields;
- hidden fields;
- derived fields;
- validation rules;
- progression metadata;
- display order.

## 13. Stable Field Keys

Rule Set field keys MUST be stable.

Example:

```text
attributes.strength
abilities.empathy
advantages.willpower
```

A localization change MUST NOT change field keys.

A display reorganization SHOULD NOT change field keys.

Breaking key changes require migration.

## 14. Field Metadata

A Rule Set field MAY define:

- minimum value;
- maximum value;
- allowed choices;
- default value;
- category;
- cost;
- prerequisite;
- source reference;
- visibility;
- derivation;
- formatting hint;
- prompt-safe description.

UI hints remain advisory.

The Character Sheet domain remains authoritative.

## 15. Derived Fields

A Rule Set MAY define derived fields.

A derived field SHOULD specify:

- field key;
- source field keys;
- calculation version;
- calculation behavior;
- persistence policy;
- recalculation triggers.

Derived fields MUST be calculated deterministically.

Narrative Intelligence MUST NOT calculate authoritative derived values.

## 16. Character Validation Contract

A Rule Set MUST expose Character validation behavior.

Validation SHOULD accept:

- Rule Set version;
- Character role;
- Character Sheet values;
- Character State when relevant;
- validation mode;
- Campaign Preferences.

Validation SHOULD return:

- overall status;
- errors;
- warnings;
- affected field keys;
- machine-readable codes;
- Rule Set references;
- override permission;
- normalized values when safe.

## 17. Validation Modes

The generic contract recognizes:

```text
Strict
Advisory
Disabled
```

A Rule Set MUST declare which modes it supports.

`Disabled` mechanical validation does not disable Chronicle structural validation.

## 18. Normalization

A Rule Set MAY normalize values.

Examples:

- trim input;
- canonicalize option identifiers;
- calculate derived values;
- reorder sets;
- resolve aliases.

Normalization MUST be deterministic.

It MUST NOT silently alter player intent beyond safe canonicalization.

## 19. Rule Set Operation Catalog

A Rule Set MUST expose stable operation keys for supported tests and mechanical actions.

Examples:

```text
social.detect_lie
combat.melee_attack
ritual.enter_umbra
movement.climb
knowledge.occult_recall
```

Each operation SHOULD define:

- operation key;
- display label;
- description;
- required actor data;
- optional target;
- pool calculation;
- difficulty behavior;
- allowed modifiers;
- result contract;
- consequence categories;
- visibility;
- whether randomness is required.

## 20. Operation Stability

Operation keys MUST remain stable across nonbreaking versions.

Renaming an operation key requires:

- alias support;
- migration;
- or major version change.

Historical Dice Rolls preserve the original key.

## 21. Semantic Test Request

The Chronicle Director or Narrator SHOULD express action intent semantically.

Example:

```text
The player attempts to determine whether the elder is lying.
```

The Rule Set maps that intent to:

```text
social.detect_lie
```

The provider MAY propose an operation key.

Chronicle validates it through the Rule Set.

## 22. Dice Pool Calculation

The Rule Set owns authoritative dice pool calculation.

A calculation MAY use:

- Character Sheet fields;
- Character State;
- target data;
- Scene conditions;
- temporary effects;
- equipment;
- Relationships;
- Campaign Preferences;
- prior validated consequences.

The result SHOULD include a breakdown.

## 23. Pool Calculation Result

A pool calculation result SHOULD contain:

- operation key;
- source fields;
- base pool;
- modifiers;
- final pool;
- difficulty;
- special rules;
- validation warnings;
- calculation version.

The UI MAY display a simplified version.

## 24. Roll Resolution Contract

A Rule Set MUST interpret raw random values.

The resolution input SHOULD include:

- operation key;
- Rule Set version;
- actor snapshot;
- target snapshot when relevant;
- authoritative pool;
- difficulty;
- modifiers;
- raw dice;
- Scene conditions;
- Campaign Preferences.

The result SHOULD include:

- outcome category;
- Rule Set-specific result;
- successes or equivalent;
- failures or equivalent;
- critical flags;
- degree of success;
- mechanical consequences;
- narrative guidance;
- resolution version.

## 25. Generic Outcome Mapping

A Rule Set SHOULD map its result to a generic orchestration category where possible:

```text
CriticalFailure
Failure
PartialSuccess
Success
CriticalSuccess
```

The Rule Set-specific result remains authoritative.

A Rule Set MAY indicate that no generic mapping is meaningful.

## 26. Deterministic Test Resolution

A Rule Set MAY define tests that do not require randomness.

Examples:

- automatic success;
- passive threshold;
- impossible action;
- resource comparison;
- fixed transformation.

The contract SHOULD support deterministic resolution without creating fake dice values.

## 27. Contested Tests

A Rule Set MAY support contested tests.

The contract SHOULD define:

- participants;
- each participant's pool;
- resolution order;
- comparison rule;
- tie rule;
- visibility;
- result structure.

The MVP MAY implement only the contested tests required by the initial Rule Set.

## 28. Extended Tests

A Rule Set MAY declare extended-test capability.

An extended test SHOULD define:

- target progress;
- maximum attempts;
- accumulated progress;
- failure effects;
- interval;
- completion rule.

The generic DiceRoll entity remains one execution record.

An extended-test workflow owns multiple Dice Rolls.

## 29. Modifiers

A Rule Set MUST define valid modifier sources and ranges.

A modifier SHOULD include:

- key;
- value;
- source category;
- reason;
- stacking rule;
- duration;
- visibility.

Chronicle MUST reject unsupported modifiers.

## 30. Temporary Effects

A Rule Set MAY define temporary effects.

An effect definition SHOULD include:

- effect key;
- display name;
- duration model;
- applicable Character fields;
- modifiers;
- stacking behavior;
- expiration behavior;
- visibility;
- narrative guidance.

Effect expiration MUST be deterministic.

## 31. Character State Extension

The generic Character State remains Chronicle-owned.

A Rule Set MAY provide typed extensions for:

- wounds;
- forms;
- resources;
- statuses;
- corruption;
- morality;
- supernatural conditions;
- turn-specific effects.

Extensions MUST be versioned and validated.

## 32. Progression Contract

A Rule Set MUST define progression behavior when progression is supported.

It SHOULD define:

- progression currency;
- award criteria;
- spending rules;
- field costs;
- prerequisites;
- maximums;
- milestone behavior;
- validation;
- result structure.

The Archivist MAY propose progression evidence.

The Rule Set calculates or validates authoritative progression.

## 33. Progression Proposal

A progression proposal SHOULD distinguish:

```text
Narrative Evidence
Requested Award
Rule Set Calculation
Accepted Progression
```

The Rule Set MUST NOT depend on free-form prose alone when deterministic criteria exist.

## 34. Progression Application

A progression application SHOULD return:

- awarded value;
- available value;
- spent value;
- changed fields;
- unlocked options;
- warnings;
- Rule Set version;
- calculation version.

It MUST be idempotent at the application workflow level.

## 35. Relationship Mechanics

A Rule Set MAY extend Relationship behavior.

It MAY define:

- additional dimensions;
- allowed ranges;
- mechanical effects;
- social test modifiers;
- status labels;
- decay or recovery policies.

Generic directionality and ownership remain unchanged.

## 36. Character Knowledge Mechanics

A Rule Set MAY define:

- perception mechanics;
- deception mechanics;
- concealment;
- supernatural secrecy;
- memory alteration;
- confidence effects;
- knowledge erasure;
- partial revelation.

The generic Knowledge states remain Chronicle concepts.

## 37. Campaign Preferences Contract

A Rule Set MAY expose Campaign Preference definitions.

A preference SHOULD define:

- preference key;
- display label;
- description;
- data type;
- default;
- allowed values;
- affected operations;
- validation behavior;
- whether it changes mechanics or only narrative guidance.

Example:

```text
ritual.enter_umbra.requires_test = false
```

## 38. Preference Safety

A Campaign Preference MUST NOT:

- bypass Chronicle ownership;
- allow invalid data structures;
- redefine Campaign hierarchy;
- permit provider-generated randomness;
- disable persistence integrity;
- expose hidden information.

Mechanics-changing preferences MUST be explicit and Rule Set-supported.

## 39. Terminology Contract

A Rule Set SHOULD provide canonical game terminology.

Examples:

- names of attributes;
- names of resources;
- names of test outcomes;
- character roles;
- supernatural concepts;
- progression terms.

Terminology SHOULD include localization keys.

The Narrator SHOULD use Rule Set terminology consistently.

## 40. Localization

A Rule Set MAY provide localized:

- display name;
- field labels;
- section labels;
- operation labels;
- validation messages;
- terminology;
- result descriptions.

Localization MUST NOT change stable identifiers.

## 41. Rule Knowledge Contract

A Rule Set SHOULD expose a provider-neutral rule knowledge capability.

The Application requests relevant knowledge using a structured query.

A query MAY include:

- operation key;
- Character Sheet field keys;
- topic;
- Scene context;
- Rule Set version;
- language;
- maximum result size.

The response SHOULD include:

- concise rule excerpts or summaries;
- source references;
- applicability metadata;
- confidence;
- version.

## 42. Knowledge Retrieval Boundary

The Rule Set contract defines what knowledge is needed.

Infrastructure decides how to retrieve it.

Possible implementations:

- structured rules;
- keyword search;
- hybrid search;
- vector retrieval;
- local documents;
- external service.

The Domain MUST remain unaware of the method.

## 43. Rule Knowledge Source References

Knowledge results SHOULD include traceable source references.

A source reference MAY identify:

- licensed document;
- section;
- page;
- rule identifier;
- package content entry.

Chronicle SHOULD avoid copying proprietary text unnecessarily.

## 44. Campaign Generation Guidance

A Rule Set MAY provide guidance for Campaign generation.

It SHOULD include:

- setting constraints;
- supported themes;
- NPC creation boundaries;
- power-level guidance;
- important system concepts;
- expected conflict types;
- prohibited mechanical inventions;
- Character hook guidance.

Generation guidance is not a fixed Campaign template.

## 45. NPC Generation Guidance

A Rule Set SHOULD provide:

- NPC Character Sheet requirements;
- power classification guidance;
- valid archetypes;
- required mechanical fields;
- common temporary state;
- terminology;
- validation behavior.

Generated NPCs remain normal Chronicle Characters.

## 46. Rule Set Descriptor

A Rule Set descriptor SHOULD include:

```text
id
version
displayName
edition
description
status
supportedLanguages
capabilities
minimumChronicleContractVersion
schemaVersions
operationCatalogVersion
licensing
packageMetadata
```

The exact serialized shape will be defined later.

## 47. Registration

A Rule Set MUST be registered before use.

Registration SHOULD verify:

- unique identity and version;
- compatible Chronicle contract;
- valid Character Sheet schema;
- valid operation catalog;
- required capabilities;
- migration metadata;
- licensing metadata;
- package integrity.

Invalid Rule Sets MUST NOT become selectable.

## 48. Discovery

The official application MAY list registered Rule Sets.

The player-facing list SHOULD include:

- display name;
- edition;
- version;
- support status;
- language availability;
- short description.

Internal infrastructure details MUST remain hidden.

## 49. Package Boundary

A Rule Set package MAY contain:

- descriptor;
- Character Sheet schemas;
- operation catalog;
- deterministic validators;
- dice resolvers;
- progression logic;
- terminology;
- localization;
- rule knowledge indexes or references;
- migration definitions;
- generation guidance;
- tests.

The exact package structure will be defined in RFC-0027.

## 50. Static and Dynamic Registration

The MVP MAY use static Rule Set registration.

Dynamic loading is an Architecture Horizon capability.

Static registration is acceptable when:

- only one Rule Set is delivered;
- package boundaries remain explicit;
- Chronicle Core does not hard-code Werewolf semantics.

## 51. Compatibility Contract

A Rule Set MUST declare compatibility with a Chronicle Rule Set contract version.

Chronicle SHOULD reject packages requiring unsupported contract features.

Compatibility checks SHOULD occur before Campaign creation.

## 52. Campaign Compatibility

A Campaign is compatible when:

- the exact Rule Set identity is available;
- the stored version is available;
- the Character Sheet schema is interpretable;
- required operations are available;
- migration is not mandatory or has been applied.

An incompatible Campaign MUST NOT be opened as if normal play were safe.

## 53. Deprecation

A Rule Set version MAY become deprecated.

Deprecation MUST NOT delete existing Campaign support automatically.

The application SHOULD warn the player and preserve existing play when possible.

## 54. Rule Set Migration

Migration MAY be required when:

- Character Sheet fields change;
- operation keys change;
- dice mechanics change;
- progression changes;
- state extensions change;
- terminology identifiers change.

Migration MUST be explicit.

## 55. Migration Contract

A migration SHOULD define:

- source version;
- target version;
- compatibility class;
- preconditions;
- Character Sheet transformation;
- Character State transformation;
- preference transformation;
- operation aliases;
- warnings;
- rollback or backup requirement;
- validation.

## 56. Migration Safety

Migration MUST:

- preserve original data;
- record source and target versions;
- avoid silent field loss;
- preserve historical Dice Roll interpretation;
- validate migrated Characters;
- produce warnings for unmapped values;
- create a checkpoint or backup.

## 57. Historical Integrity

Rule Set migration MUST NOT reinterpret completed history.

Historical records preserve:

- original Rule Set version;
- original operation key;
- original raw dice;
- original resolution;
- original Character Sheet snapshot where relevant.

New play uses the migrated version after successful migration.

## 58. Provider Neutrality

A Rule Set MUST NOT depend on one Narrative Intelligence provider.

Prompts and provider SDK types MUST remain outside the Rule Set domain contract.

A Rule Set MAY supply provider-neutral narrative guidance.

## 59. Deterministic Boundary

The following Rule Set behavior MUST be deterministic where defined:

- Character validation;
- field normalization;
- derived fields;
- pool calculation;
- difficulty validation;
- dice interpretation;
- progression calculation;
- temporary-effect expiration;
- migration.

Narrative Intelligence MAY explain or propose.

It MUST NOT replace deterministic behavior.

## 60. Error Model

Rule Set operations SHOULD return explicit errors.

Recommended categories:

```text
UnsupportedOperation
InvalidCharacter
InvalidField
InvalidModifier
InvalidDifficulty
InvalidPreference
IncompatibleVersion
MigrationRequired
RuleKnowledgeUnavailable
ResolutionFailure
```

Errors MUST be safe for application handling.

## 61. Unavailable Rule Set

If the required Rule Set is unavailable:

- the Campaign MUST NOT continue normal play;
- the application SHOULD remain able to display safe historical information;
- the user SHOULD receive a clear recovery path;
- Chronicle MUST NOT substitute another Rule Set automatically.

## 62. Rule Knowledge Unavailable

If rule knowledge retrieval fails:

- deterministic mechanics MAY continue if sufficient local logic exists;
- narrative generation requiring missing rules SHOULD fail or degrade explicitly;
- the Narrator MUST NOT invent authoritative rules.

## 63. Licensing Metadata

A Rule Set descriptor SHOULD declare:

- content license;
- code license;
- trademarks or attribution requirements;
- distribution restrictions;
- external source requirements;
- whether proprietary sourcebooks are bundled.

Chronicle MUST NOT assume that access to a sourcebook permits redistribution.

## 64. Proprietary Content Boundary

The open-source Chronicle repository MUST NOT include proprietary sourcebook text without authorization.

A Rule Set MAY require the user to provide or configure legally obtained source content.

Generated summaries and indexes MUST respect applicable rights and licenses.

## 65. Rule Set Testing Contract

Every supported Rule Set SHOULD include tests for:

- schema validity;
- Character validation;
- operation catalog;
- pool calculation;
- modifiers;
- dice resolution;
- edge cases;
- progression;
- preferences;
- migration;
- terminology identifiers;
- compatibility.

## 66. Golden Test Cases

A Rule Set SHOULD provide golden test cases.

Example:

```text
Given Character Sheet X
And operation Y
And raw dice Z
Expect result R
```

Golden tests protect mechanics across refactoring.

## 67. Property Tests

Where practical, Rule Set tests SHOULD verify properties such as:

- dice values remain in range;
- result resolution is deterministic for fixed input;
- invalid fields are rejected;
- migration preserves known values;
- versioned operations remain stable;
- progression cannot spend unavailable resources.

## 68. Chronicle Contract Tests

Chronicle SHOULD run shared contract tests against each Rule Set.

Contract tests SHOULD verify:

- descriptor validity;
- required capabilities;
- schema compatibility;
- valid operation keys;
- deterministic behavior;
- structured errors;
- no provider dependency;
- no persistence dependency.

## 69. Read Models

Recommended Rule Set read models include:

```text
RuleSetListItem
RuleSetDetailsView
CharacterSheetSchemaView
RuleOperationView
ValidationResultView
RuleSetCompatibilityView
```

These are presentation models, not domain contracts.

## 70. Observability

Chronicle SHOULD record:

- Rule Set identity and version;
- validation duration;
- operation key;
- pool calculation version;
- resolution version;
- migration result;
- knowledge retrieval result;
- compatibility failure.

Logs MUST avoid proprietary rule content unless explicitly permitted.

## 71. Security

Rule Set packages and knowledge sources MUST be treated as untrusted input.

Chronicle MUST validate:

- package integrity;
- structured schemas;
- operation identifiers;
- migration behavior;
- retrieved content;
- configuration.

The MVP MUST NOT execute arbitrary Rule Set code from untrusted third parties.

## 72. Sandboxing Horizon

Future dynamic community Rule Sets may require:

- sandboxing;
- signed packages;
- permission declarations;
- restricted execution;
- package review;
- capability isolation.

These are not MVP requirements.

## 73. Prohibited Patterns

### 73.1 Werewolf in Generic Domain

Generic Chronicle entities MUST NOT contain Werewolf-specific fields.

### 73.2 RAG as Rule Set Entity

The Domain MUST NOT model embeddings, chunks, or vector stores.

### 73.3 Prompt-Only Mechanics

Rules MUST NOT exist only as natural-language prompt instructions.

### 73.4 Provider-Owned Validation

Character or Roll validation MUST NOT depend solely on provider judgment.

### 73.5 Silent Version Upgrade

Campaigns MUST NOT change Rule Set version automatically.

### 73.6 Unversioned Operation Keys

Tests MUST NOT depend on unstable free-form names.

### 73.7 Historical Reinterpretation

New rules MUST NOT rewrite prior Dice Roll results.

### 73.8 Proprietary Content Bundling by Accident

Rule packages MUST NOT include restricted source text without authorization.

### 73.9 Arbitrary Dynamic Code in MVP

The MVP MUST NOT load untrusted executable Rule Set code dynamically.

## 74. Current Delivery Decision

The MVP adopts:

- one generic Rule Set domain contract;
- one statically registered Rule Set;
- Werewolf: The Apocalypse as the first implementation;
- stable Rule Set identity and version;
- generic Character Sheet schema;
- advisory or strict validation according to implementation readiness;
- stable operation keys;
- Chronicle-controlled randomness;
- Rule Set-controlled resolution;
- progression contract;
- provider-neutral rule knowledge port;
- explicit Campaign Preferences;
- no dynamic plug-in loading;
- no automatic Rule Set migration;
- no bundled proprietary sourcebook text without authorization.

## 75. Architecture Horizon

Future evolution MAY include:

- multiple installed Rule Sets;
- dynamic package loading;
- community Rule Sets;
- signed packages;
- package marketplace;
- sandboxed mechanics;
- remote rule services;
- shared schema editors;
- custom homebrew Rule Sets;
- automatic compatibility checks;
- migration assistants;
- multiple editions of one system.

The MVP MUST NOT implement these capabilities without a later milestone.

## 76. Open Questions

The following remain open:

- Which Werewolf edition will be the initial target?
- What exact versioning scheme will Rule Sets use?
- What minimum field types are required?
- Will Rule Set mechanics be implemented in code, data, or a hybrid?
- How should operation intent map to stable operation keys?
- What validation mode will the first release use?
- Which Campaign Preferences are required?
- How should Rule Set localization be packaged?
- How will legally obtained rule content be indexed locally?
- Which deterministic mechanics belong in the first Werewolf package?
- Should one package include multiple character schemas?
- How should migrations be authored and tested?
- How should Rule Set package integrity be verified?
- Should custom homebrew systems be supported before post-MVP?
- Which contract versioning policy should Chronicle adopt?

These questions require RFC-0027, RFC-0029, RFC-0030, RFC-0031, RFC-0032, and technology ADRs.

## 77. Compliance Checklist

A Rule Set implementation complies when:

- identity and version are explicit;
- capabilities are declared;
- Character Sheet keys are stable;
- validation is deterministic where defined;
- operation keys are stable;
- pool calculation is Rule Set-owned;
- raw dice interpretation is Rule Set-owned;
- progression is Rule Set-owned;
- Campaign Preferences are explicit;
- retrieval infrastructure does not leak into the Domain;
- provider types do not enter the contract;
- historical versions remain interpretable;
- migration is explicit;
- proprietary content is handled legally;
- Werewolf-specific concepts remain outside generic Chronicle code.

## 78. Final Principle

Chronicle defines how a Campaign persists.

A Rule Set defines how actions become mechanics.

Neither should need to become the other.
