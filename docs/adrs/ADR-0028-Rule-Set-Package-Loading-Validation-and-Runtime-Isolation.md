---
id: ADR-0028
title: Rule Set Package Loading, Validation, and Runtime Isolation
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
  - ADR-0005
  - ADR-0008
  - ADR-0009
  - ADR-0010
  - ADR-0018
  - ADR-0019
  - ADR-0020
  - ADR-0021
  - ADR-0022
  - ADR-0024
  - ADR-0025
  - ADR-0027
  - RFC-0003
  - RFC-0004
  - RFC-0005
  - RFC-0006
  - RFC-0007
  - RFC-0008
  - RFC-0010
  - RFC-0011
  - RFC-0012
  - RFC-0013
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

> **"A Rule Set may define how a world behaves. It may not become a hidden authority over Chronicle itself."**

# Rule Set Package Loading, Validation, and Runtime Isolation

## 1. Status

**Proposed**

This ADR defines how Chronicle discovers, validates, installs, activates, resolves, and isolates Rule Set packages.

The decision is:

- treat a Rule Set package as a versioned declarative and executable capability bundle;
- require a strict package manifest;
- use stable package identity and explicit semantic versioning;
- install packages into immutable versioned directories;
- never modify an installed package version in place;
- validate archive safety, manifest schema, content hashes, contracts, references, and compatibility before activation;
- keep package activation separate from package installation;
- maintain an explicit active package registry;
- select a package by stable identity and exact resolved version for each Campaign;
- preserve the package version used by accepted Campaign state;
- prohibit silent substitution of another Rule Set or version;
- prohibit arbitrary EF Core migrations from packages;
- prohibit unrestricted assembly scanning and unrestricted runtime code loading in the MVP;
- allow official executable Rule Set implementations only through Chronicle-owned compiled modules or a future reviewed plugin boundary;
- support declarative package content for Character Sheets, operations, Preferences, progression, and Rule Knowledge;
- keep Rule Set execution deterministic;
- deny access to provider credentials, filesystem, network, UI, clock, randomness, and database internals;
- pass only explicit Rule Set execution inputs;
- validate every Rule Set result before Chronicle accepts it;
- quarantine invalid, incompatible, duplicate, or untrusted packages;
- make missing packages visible without corrupting or silently changing Campaign state.

The decision becomes **Accepted** after a package-loading spike proves:

- official package discovery;
- manifest validation;
- immutable versioned installation;
- duplicate package detection;
- compatibility evaluation;
- exact Campaign package resolution;
- declarative Character Sheet schema loading;
- deterministic Rule Operation invocation;
- Rule Knowledge registration;
- invalid package quarantine;
- package upgrade side-by-side installation;
- Campaign pinning to old version;
- blocked silent substitution;
- no package access to forbidden capabilities.

## 2. Context

Chronicle is intended to support multiple tabletop RPG systems.

A Rule Set defines system-specific behavior such as:

- Character Sheet structure;
- attributes and abilities;
- derived values;
- Dice pool construction;
- success resolution;
- exceptional results;
- damage or consequence mechanics;
- progression rules;
- Preferences;
- supported narrative operations;
- Rule Knowledge references;
- package-specific terminology.

The architecture requires a separate system-specific knowledge and mechanics boundary.

Adding a new Rule Set must not require changing Chronicle's core Domain for every game-specific detail.

At the same time, Rule Sets must not become unrestricted plugins that can:

- read arbitrary files;
- access credentials;
- call the network;
- execute provider requests;
- mutate the database directly;
- generate their own authoritative randomness;
- bypass Chronicle transactions;
- change UI behavior secretly;
- install arbitrary schema migrations;
- inspect unrelated Campaigns.

The MVP needs one complete official Rule Set, with Werewolf: The Apocalypse as the development target.

The package model must support future growth without introducing a general untrusted plugin runtime prematurely.

## 3. Decision Drivers

The package design prioritizes:

1. deterministic mechanics;
2. Chronicle authority;
3. package portability;
4. explicit compatibility;
5. immutable installation;
6. Campaign reproducibility;
7. security boundaries;
8. no silent rules changes;
9. clear package ownership;
10. declarative extensibility;
11. future plugin readiness;
12. copyright compliance.

## 4. Decision Summary

Chronicle will use:

```text
Package Identity
    stable RuleSetPackageId

Version
    semantic version

Installation
    immutable version directory

Package Manifest
    strict versioned JSON contract

Activation
    explicit validated registry

Campaign Binding
    package ID + exact resolved version

Declarative Content
    Character Sheet schemas
    operations catalog
    Preferences
    progression contracts
    terminology
    Rule Knowledge metadata

Executable Mechanics in MVP
    Chronicle-reviewed compiled official modules

General Runtime Plugin Loading
    deferred

Package Authority
    propose deterministic mechanical results
    Chronicle validates and persists

Forbidden Package Capabilities
    database
    filesystem
    network
    provider credentials
    UI
    ambient time
    randomness
    DI container
```

## 5. Rule Set Package Definition

A Rule Set package is a cohesive versioned bundle containing some or all of:

```text
manifest
Character Sheet schemas
Rule Operation declarations
Preferences definitions
progression definitions
terminology catalogs
Rule Knowledge metadata
Rule Knowledge documents when permitted
mechanics implementation reference
compatibility metadata
localization resources
content hashes
license metadata
```

## 6. Package Identity

Every package has a stable:

```text
RuleSetPackageId
```

Example:

```text
chronicle.ruleset.werewolf-the-apocalypse
```

The final key must comply with the semantic-key conventions.

## 7. Package Version

Every package uses semantic versioning.

Example:

```text
1.0.0
```

Prerelease versions may use:

```text
1.0.0-alpha.1
```

according to release-channel policy.

## 8. Package Identity Versus Display Name

Package identity is machine-stable and not localized.

Display name may be localized.

## 9. Exact Campaign Binding

A Campaign stores:

```text
RuleSetPackageId
ResolvedRuleSetPackageVersion
RuleSetContractVersion
```

Chronicle must know exactly which rules version governed accepted state.

## 10. Version Range

A Campaign or export MAY also record an allowed compatibility range for future resolution.

The exact previously resolved version remains part of historical evidence.

## 11. No Silent Upgrade

Chronicle MUST NOT silently move a Campaign to a newer package version.

## 12. Package Upgrade

A Campaign package upgrade is an explicit Application workflow.

It requires:

- compatibility inspection;
- package migration plan;
- validation;
- checkpoint when needed;
- user confirmation;
- post-upgrade verification.

## 13. Side-by-Side Versions

Multiple versions of one Rule Set package may be installed side by side.

## 14. Immutable Installation

An installed package version is immutable.

Updates install a new directory.

## 15. Package Directory

Recommended layout:

```text
packages/
└── {packageId}/
    └── {version}/
        ├── manifest.json
        ├── schemas/
        ├── operations/
        ├── preferences/
        ├── progression/
        ├── terminology/
        ├── rule-knowledge/
        ├── localization/
        └── content/
```

Executable official implementation may remain in Chronicle binaries and be referenced by manifest key.

## 16. Package Installation Staging

Installation follows:

1. copy or extract to staging;
2. validate archive safety;
3. parse manifest;
4. verify hashes;
5. validate contracts;
6. validate compatibility;
7. validate licensing and trust metadata;
8. publish immutable version directory;
9. register as installed;
10. activate only when selected.

## 17. Package Archive Safety

Package archives are untrusted input.

Chronicle rejects:

- path traversal;
- absolute paths;
- drive-qualified paths;
- symlinks;
- junctions;
- duplicate conflicting entries;
- excessive extraction size;
- excessive entry count;
- compression bombs;
- unsupported executable content.

## 18. Package Manifest

Recommended contract key:

```text
chronicle.ruleset.package-manifest
```

## 19. Manifest Fields

Recommended fields:

```text
ManifestContractVersion
RuleSetPackageId
PackageVersion
DisplayNameKey
DescriptionKey
Publisher
License
CopyrightNotice
HomepageReference
MinimumChronicleVersion
MaximumChronicleVersion when needed
SupportedRuleSetContractVersion
MechanicsImplementationKey
CharacterSchemaEntries
OperationCatalogEntries
PreferenceEntries
ProgressionEntries
RuleKnowledgeEntries
LocalizationEntries
ContentHashes
PackageCapabilities
PackageDependencies
MigrationMetadata
TrustMetadata
```

## 20. Manifest Strictness

The package manifest is strict.

Unknown critical fields or discriminators are rejected.

## 21. Content Hashes

Every required package file is listed with SHA-256 or another approved hash.

## 22. Package Integrity

Package activation verifies required file hashes.

A modified installed package version becomes invalid.

## 23. Package Trust

The MVP distinguishes:

```text
OfficialBundled
OfficialInstalled
Development
Unknown
Invalid
```

## 24. Official Bundled Package

An official bundled package ships with Chronicle and is validated against release metadata.

## 25. Development Package

A Development package may be loaded only in Development mode.

It must be visibly marked and cannot be mistaken for Stable content.

## 26. Unknown Package

An unknown package may be inspectable but is not executable in the MVP unless a future trust model permits it.

## 27. Package Signatures

Cryptographic package signatures are deferred.

The package manifest reserves trust metadata for future use.

## 28. General Plugin Runtime

Chronicle does not load arbitrary third-party executable assemblies at runtime in the MVP.

### Rationale

A secure general plugin runtime would require decisions for:

- code trust;
- process isolation;
- capability sandboxing;
- API compatibility;
- signing;
- crash containment;
- resource limits;
- update policy;
- malware response.

## 29. Official Mechanics Modules

The MVP may include official mechanics implementations compiled with Chronicle or in reviewed first-party assemblies.

They are resolved through stable implementation keys.

## 30. Mechanics Implementation Key

The manifest may declare:

```text
MechanicsImplementationKey
```

Example:

```text
chronicle.mechanics.wta.v1
```

## 31. Implementation Registry

Chronicle maintains an immutable registry:

```text
IRuleSetMechanicsRegistry
```

mapping approved implementation keys to reviewed implementations.

## 32. Missing Implementation

A package whose mechanics implementation is unavailable remains installed but incompatible for play.

## 33. Declarative Package Content

Declarative contracts are the preferred extension mechanism.

They include:

- field definitions;
- constraints;
- labels;
- operation declarations;
- Preference definitions;
- progression tables;
- terminology;
- Rule Knowledge metadata.

## 34. Declarative Does Not Mean Authoritative by Itself

All declarative content is validated before activation.

## 35. Character Sheet Schema

A package may define:

```text
sections
fields
field types
requiredness
limits
derived-field declarations
visibility
editability
progression metadata
Rule Operation references
```

## 36. Character Sheet Schema Version

Every schema has its own identity and version.

Campaign Characters record which schema version governs their stored field payload.

## 37. Character Schema Validation

Validation includes:

- duplicate field keys;
- invalid types;
- circular derived-field dependencies;
- invalid constraints;
- missing operation references;
- prohibited executable expressions;
- unsupported contract version.

## 38. Executable Expressions

Arbitrary scripting or expression execution is prohibited in MVP declarative schemas.

Derived values use approved operations or a constrained future expression language.

## 39. Rule Operation Catalog

The package declares supported Rule Operations.

Example categories:

```text
buildDicePool
resolveRoll
applyDamage
applyHealing
calculateDerivedValue
validateAdvancement
applyAdvancement
validateCharacter
finalizeSessionProgression
```

## 40. Rule Operation Identity

Every operation uses a stable semantic key.

## 41. Rule Operation Contract

An operation declares:

```text
OperationKey
InputContractVersion
OutputContractVersion
DeterminismRequired
SupportedPreferences
RequiredCharacterFields
PossibleResultKinds
```

## 42. Rule Operation Resolution

Chronicle resolves an operation through:

```text
RuleSetPackageId
PackageVersion
OperationKey
```

## 43. Deterministic Execution

A Rule Operation MUST be deterministic for identical:

- package version;
- operation version;
- input snapshot;
- Preferences;
- raw Dice values;
- execution options.

## 44. No Hidden Randomness

Rule Set code receives raw Dice values.

It does not receive a random source.

## 45. No Ambient Time

Rule Set code does not receive system time unless a specific explicit input is part of the mechanical contract.

## 46. Fictional Time

Fictional Campaign time may be passed as Domain data where relevant.

It is not read from the operating system clock.

## 47. Rule Set Input

Inputs are explicit immutable DTOs or Domain snapshots.

They may include:

- actor mechanical state;
- target state;
- selected action;
- Preferences;
- raw Dice values;
- accepted modifiers;
- Rule Set version;
- operation context.

## 48. Rule Set Output

Outputs are explicit typed proposals.

Examples:

```text
DicePoolProposal
RollResolution
DamageApplicationProposal
AdvancementValidation
DerivedValueResult
CharacterValidationResult
```

## 49. Chronicle Validation

Chronicle validates output before acceptance.

Validation includes:

- contract version;
- result kind;
- identifier scope;
- numerical bounds;
- expected state version;
- operation compatibility;
- no unauthorized mutations;
- deterministic replay where tested.

## 50. Rule Set Cannot Persist

Rule Set implementations MUST NOT:

- open the database;
- call repositories;
- call `SaveChanges`;
- create Work Items directly;
- publish Domain Events directly;
- mutate unrelated aggregate state.

## 51. Rule Set Cannot Call Providers

Rule Set implementations MUST NOT invoke Narrative Intelligence.

## 52. Rule Set Cannot Access Credentials

Rule Set implementations MUST NOT receive secret managers, credentials, or provider profiles.

## 53. Rule Set Cannot Access Filesystem

Rule Set execution MUST NOT read arbitrary files.

Validated package content is loaded by Chronicle and passed as immutable data.

## 54. Rule Set Cannot Access Network

Rule Set execution MUST NOT make network requests.

## 55. Rule Set Cannot Access UI

Rule Set implementations do not display dialogs, notifications, or controls.

## 56. Rule Set Cannot Access DI Container

Rule Set code does not resolve arbitrary services.

## 57. Rule Set Resource Limits

Rule Set execution SHOULD have:

- input-size limits;
- operation timeout or budget;
- cancellation;
- bounded collection sizes;
- recursion limits where relevant.

## 58. In-Process Isolation

Official mechanics run in process in MVP.

Isolation is architectural and capability-based, not a security sandbox.

## 59. Security Claim

Chronicle does not claim that in-process first-party code is hostile-code sandboxed.

This is why arbitrary third-party executable packages are not loaded.

## 60. Future Out-of-Process Isolation

A future plugin model MAY execute third-party mechanics in a separate process with a strict protocol.

That requires a separate ADR.

## 61. Package Loader

Chronicle defines:

```text
IRuleSetPackageLoader
```

responsible for:

- manifest read;
- contract validation;
- hash verification;
- metadata construction;
- declarative-content loading;
- activation candidate creation.

## 62. Package Registry

Chronicle defines:

```text
IRuleSetPackageRegistry
```

responsible for:

- installed packages;
- active validated descriptors;
- exact version lookup;
- compatibility lookup;
- duplicate detection;
- diagnostics.

## 63. Registry Immutability

The runtime registry is immutable after startup activation in MVP.

Package installation or removal may require registry rebuild or application restart.

## 64. Package Activation

Activation means a validated package version becomes available for Campaign resolution.

It does not alter any Campaign automatically.

## 65. Activation Preconditions

A package may activate only if:

- manifest is supported;
- hashes match;
- implementation exists;
- dependencies resolve;
- contracts validate;
- package identity is unique;
- Chronicle version is compatible;
- trust mode permits activation.

## 66. Package Dependency

A package may declare dependencies on approved Chronicle package types.

Circular dependencies are rejected.

## 67. Rule Set Package Dependency Limits

Rule Set-to-Rule Set dependency is discouraged.

Shared declarative libraries may be supported later through a separate package type.

## 68. Duplicate Identity

Two installed artifacts claiming the same package ID and version but different content hashes are a critical package conflict.

Neither is activated automatically.

## 69. Same Content Duplicate

Identical duplicate package content may be deduplicated during installation.

## 70. Package Quarantine

Invalid or conflicting packages are moved or marked under a quarantine state.

## 71. Quarantine States

Recommended states:

```text
InvalidManifest
UnsupportedContract
HashMismatch
DuplicateIdentityConflict
MissingImplementation
MissingDependency
IncompatibleChronicleVersion
UntrustedExecutableContent
LicensePolicyBlocked
```

## 72. Quarantine Behavior

Quarantined packages:

- are not activated;
- are not used for Campaign mutation;
- remain inspectable through safe diagnostics;
- may be removed explicitly.

## 73. Missing Package at Startup

A Campaign requiring a missing package remains visible.

Chronicle reports:

```text
CampaignCompatibilityState = MissingRuleSetPackage
```

## 74. Missing Version

If the package ID exists but the required exact version does not:

- Chronicle does not select a nearby version silently;
- compatibility planning may suggest supported upgrade options;
- normal play is blocked until resolved.

## 75. Incompatible Package

If package and Chronicle contract versions are incompatible:

- package is not activated;
- affected Campaigns remain readable where generic projections allow;
- mutation is blocked.

## 76. Package Upgrade Discovery

Chronicle MAY detect a newer installed compatible package.

It may present an upgrade option.

It must not apply it automatically.

## 77. Campaign Package Upgrade

A package upgrade workflow SHOULD:

1. resolve source package;
2. resolve target package;
3. inspect migration metadata;
4. create checkpoint when required;
5. migrate package-defined Character payloads;
6. revalidate Characters and Preferences;
7. revalidate unresolved Work Items;
8. update Campaign package binding;
9. append upgrade history;
10. run post-upgrade checks.

## 78. Package Migration Contract

A package may declare migration adapters between supported package versions.

Executable migration code remains reviewed first-party code in MVP.

## 79. Package Data Migration

Package migration MUST be:

- deterministic;
- versioned;
- local;
- provider-free;
- random-free;
- recoverable;
- validated.

## 80. Character Schema Migration

Changing field identity or meaning requires explicit migration.

Renaming a display label does not require data migration when the semantic key is stable.

## 81. Field Removal

Removing a stored field requires a policy:

```text
preserve archived value
map to replacement
transform
block migration
```

Silent deletion is prohibited.

## 82. Operation Contract Migration

Pending Work Items using old Rule Operation contracts must be:

- migrated;
- completed under old installed package;
- superseded;
- or marked RecoveryRequired.

## 83. Historical Mechanics Evidence

Accepted historical Dice and results retain the source package and operation versions.

A package upgrade does not rewrite old outcomes.

## 84. Historical Replay

Replay uses persisted accepted evidence.

Chronicle does not rerun old mechanics automatically under a newer package.

## 85. Rule Knowledge Registration

A package declares Rule Knowledge entries containing:

- source identity;
- title metadata;
- content classification;
- transmission policy;
- version;
- content hash;
- indexing hints;
- operation references.

## 86. Rule Knowledge Copyright

Packages MUST NOT include unauthorized copyrighted sourcebooks or scans.

## 87. User-Provided Rule Knowledge

User-provided sources remain separately classified and are not transformed into official package content.

## 88. Package Localization

Packages may include localization resources for:

- field labels;
- operation names;
- Preference descriptions;
- terminology;
- validation messages.

## 89. Localization Keys

Package contracts use stable localization keys rather than embedded UI text where practical.

## 90. Missing Localization

Missing localization falls back according to Chronicle's localization policy.

It does not invalidate mechanics.

## 91. Package Preference Definitions

A package defines Preferences through stable keys and typed value contracts.

## 92. Preference Validation

Validation includes:

- supported type;
- default;
- allowed range;
- operation applicability;
- migration behavior;
- compatibility.

## 93. Progression Contracts

A package defines progression rules through explicit operations and metadata.

It cannot modify Character state directly outside Chronicle's progression workflow.

## 94. Package Installation Command

Recommended Application command:

```text
InstallRuleSetPackageCommand
```

## 95. Package Removal Command

Recommended command:

```text
RemoveRuleSetPackageVersionCommand
```

## 96. Removal Preconditions

A package version cannot be removed when:

- a Campaign depends on it;
- pending Work Items require it;
- migration checkpoint references it;
- restore compatibility requires it;
- a package upgrade is incomplete.

## 97. Force Removal

Force removal is outside MVP.

## 98. Bundled Package Update

An application update may ship a newer official package.

It installs side by side.

## 99. Bundled Package Removal

Application updates should not remove an old package version still required by user Campaigns without an explicit compatibility plan.

## 100. Package Inventory

Chronicle maintains installed package metadata in authoritative local storage.

## 101. Inventory Fields

Recommended fields:

```text
RuleSetPackageId
PackageVersion
InstallPathReference
ContentHash
TrustState
ValidationState
InstalledAtUtc
ActivatedAtUtc
ManifestVersion
MechanicsImplementationKey
CompatibilityState
```

## 102. Filesystem Is Not Sole Inventory

Directory presence alone does not prove activation.

## 103. Startup Package Scan

Startup inspects installed package directories and reconciles them with inventory.

## 104. Startup Performance

Package manifests and hashes MAY be cached.

Critical integrity checks must still run according to policy.

## 105. Content Hash Cache

A hash cache may use:

- file length;
- last write metadata;
- prior validated hash;
- immutable-directory guarantee.

It remains advisory and must fail safe.

## 106. Package Hot Reload

Package hot reload is deferred.

Activation changes may require application restart.

## 107. Development Reload

Development mode MAY support package reload with explicit warnings and no Stable guarantees.

## 108. Package Diagnostics

Diagnostics SHOULD show:

- package ID;
- version;
- trust;
- validation;
- implementation key;
- dependencies;
- Campaign usage count;
- quarantine reason.

## 109. Logging

Package logs MAY include:

- package ID;
- version;
- manifest version;
- content hash prefix;
- trust state;
- activation outcome;
- validation failure code;
- load duration.

They MUST NOT include copyrighted content or private Rule Knowledge text.

## 110. Metrics

Useful metrics include:

```text
PackageDiscoveryDuration
PackageValidationFailureCount
PackageActivationCount
PackageQuarantineCount
MissingPackageCampaignCount
PackageUpgradeCount
RuleOperationDuration
RuleOperationFailureCount
```

## 111. Error Model

Recommended errors:

```text
ruleset.package-not-found
ruleset.package-version-not-found
ruleset.manifest-invalid
ruleset.manifest-unsupported
ruleset.hash-mismatch
ruleset.identity-conflict
ruleset.missing-implementation
ruleset.missing-dependency
ruleset.chronicle-version-incompatible
ruleset.contract-incompatible
ruleset.package-quarantined
ruleset.operation-not-found
ruleset.operation-input-invalid
ruleset.operation-output-invalid
ruleset.execution-failed
ruleset.execution-timeout
ruleset.migration-required
ruleset.migration-failed
```

## 112. Rule Operation Failure

A Rule Operation failure:

- produces no authoritative mutation;
- returns a typed error;
- preserves the input state;
- does not fall back to provider improvisation.

## 113. Rule Operation Timeout

A timeout does not permit partial acceptance.

## 114. Exception Boundary

Exceptions from approved mechanics implementations are caught and mapped.

Raw stack traces do not reach the user.

## 115. Determinism Verification

Selected Rule Operations SHOULD support a test harness that executes the same fixture repeatedly and compares output bytes or semantic output.

## 116. Rule Set Test Kit

Chronicle SHOULD provide:

```text
RuleSetContractTestKit
CharacterSchemaTestKit
RuleOperationDeterminismTestKit
PackageManifestTestKit
PackageMigrationTestKit
```

## 117. Package Contract Fixtures

Every package version SHOULD include or reference synthetic fixtures.

## 118. Golden Mechanical Cases

Official packages SHOULD define golden cases for:

- basic Dice pool;
- modifiers;
- success;
- failure;
- exceptional outcome;
- damage;
- progression;
- invalid Character;
- Preference variation.

## 119. Testing Strategy

The implementation requires:

```text
Manifest Tests
Archive Security Tests
Contract Tests
Determinism Tests
Compatibility Tests
Migration Tests
Startup Tests
Security Tests
Architecture Tests
Performance Tests
```

## 120. Manifest Tests

Tests MUST cover:

- valid manifest;
- missing required field;
- unknown critical field;
- invalid package ID;
- invalid version;
- duplicate entry;
- hash mismatch;
- incompatible Chronicle version.

## 121. Installation Tests

Tests MUST cover:

- staged installation;
- atomic publication;
- identical duplicate;
- conflicting duplicate;
- immutable installed directory;
- cancellation;
- disk full;
- quarantined package.

## 122. Registry Tests

Tests MUST cover:

- exact version resolution;
- missing version;
- duplicate identity;
- side-by-side versions;
- inactive package;
- startup reconciliation.

## 123. Character Schema Tests

Tests MUST cover:

- duplicate field key;
- unsupported type;
- circular dependency;
- invalid constraint;
- missing operation;
- schema migration.

## 124. Rule Operation Tests

Tests MUST cover:

- exact input;
- deterministic output;
- invalid input;
- invalid output;
- timeout;
- cancellation;
- no random source;
- no clock;
- no provider call;
- no database access.

## 125. Package Upgrade Tests

Tests MUST cover:

- compatible additive upgrade;
- Character payload migration;
- field rename by stable key;
- removed field preservation;
- pending Work Item compatibility;
- checkpoint;
- failure rollback or recovery.

## 126. Campaign Resolution Tests

Tests MUST cover:

- required exact version installed;
- only newer version installed;
- only older version installed;
- missing package;
- quarantined package;
- incompatible contract;
- no silent substitution.

## 127. Copyright Tests

Release package inventory SHOULD be audited to verify only approved distributable content is bundled.

## 128. Security Tests

Tests MUST prove package execution cannot receive:

- filesystem service;
- network client;
- secret manager;
- provider adapter;
- DbContext;
- repository;
- random source;
- DI service provider;
- UI service.

## 129. Required Test Cases

Tests MUST cover:

- official bundled package;
- official installed package;
- Development package;
- invalid manifest;
- malicious archive;
- hash mismatch;
- duplicate identity;
- immutable version;
- side-by-side versions;
- missing implementation;
- package activation;
- Campaign exact resolution;
- Character schema load;
- Dice pool operation;
- deterministic Roll resolution;
- invalid result rejection;
- package upgrade;
- missing package Campaign;
- package removal blocked;
- no arbitrary assembly loading.

## 130. Architecture Tests

Architecture tests MUST reject:

- Rule Set code referencing EF Core;
- Rule Set code referencing provider SDKs;
- Rule Set code referencing credential services;
- Rule Set code using `System.IO`;
- Rule Set code using network APIs;
- Rule Set code using ambient time;
- Rule Set code using randomness;
- Rule Set code resolving DI services;
- package-defined EF migrations;
- arbitrary executable assembly scanning;
- direct Campaign persistence from Rule Set code.

## 131. Prohibited Patterns

### 131.1 Silent Package Upgrade

Campaign rules change only through explicit workflow.

### 131.2 Modify Installed Version in Place

Install a new immutable version.

### 131.3 Package Accesses DbContext

Chronicle owns persistence.

### 131.4 Package Rolls Dice

Chronicle owns randomness.

### 131.5 Package Calls Narrative Provider

Mechanics and narration remain separated.

### 131.6 Arbitrary Runtime Assembly Loading

General plugins are deferred.

### 131.7 Package Adds Core Database Migration

Core schema remains Chronicle-owned.

### 131.8 Missing Package Falls Back to Similar Package

Exact identity matters.

### 131.9 Rule Set Output Accepted Without Validation

Chronicle decides what enters truth.

### 131.10 Bundle Unauthorized Sourcebooks

Copyright policy remains explicit.

## 132. Alternatives Considered

### Hardcode One Rule Set Into Chronicle

Simpler initially, but conflicts with the framework's intended multi-system architecture.

### Fully Dynamic Third-Party Plugins in MVP

Rejected because secure executable isolation and compatibility require substantial additional scope.

### Declarative Mechanics Only

Attractive for portability, but complex game systems may require logic beyond a safe MVP schema language.

Chronicle therefore uses declarative metadata plus reviewed official mechanics implementations.

### JavaScript or Lua Scripting

Deferred because sandboxing, determinism, resource limits, debugging, and security require a separate design.

### WebAssembly Sandbox

Promising future option, but not selected without a validated host API, deterministic execution policy, and package trust model.

### One Active Package Version Globally

Rejected because different Campaigns may require different versions.

## 133. Consequences

### Positive

- multi-system foundation;
- deterministic mechanics;
- exact Campaign reproducibility;
- no silent rule changes;
- safe declarative extensibility;
- immutable package history;
- clear copyright boundary;
- future plugin evolution remains possible.

### Negative

- arbitrary third-party executable Rule Sets are not supported in MVP;
- official mechanics modules require Chronicle release integration;
- side-by-side versions increase storage and maintenance;
- package migration contracts add complexity;
- declarative schemas require strict validation;
- package restart requirements may reduce convenience.

## 134. Risks

### Declarative Model Is Too Limited

Mitigation:

- reviewed compiled mechanics;
- operation contracts;
- future constrained runtime ADR.

### Official Implementation Key Drift

Mitigation:

- stable registry;
- compatibility fixtures;
- startup validation;
- release audit.

### Package Upgrade Changes Historical Meaning

Mitigation:

- exact historical version;
- no rewrite of accepted outcomes;
- explicit migration;
- checkpoint.

### Invalid Package Affects Startup

Mitigation:

- quarantine;
- continue with unaffected Campaigns;
- Safe Mode diagnostics.

### Third-Party Demand Arrives Early

Mitigation:

- stable package contracts;
- explicit future plugin boundary;
- avoid pretending in-process code is sandboxed.

## 135. Technology Spike

Before acceptance, implement:

1. package manifest contract;
2. package archive validator;
3. immutable package installer;
4. package inventory;
5. package registry;
6. exact Campaign resolver;
7. official mechanics registry;
8. Character Sheet schema loader;
9. Rule Operation dispatcher;
10. deterministic test harness;
11. Rule Knowledge registration;
12. quarantine workflow;
13. side-by-side version support;
14. package upgrade plan;
15. forbidden-capability architecture tests.

## 136. Spike Acceptance

The spike passes when:

- an official package installs into an immutable version directory;
- invalid and conflicting packages are quarantined;
- a Campaign resolves only its exact required package version;
- newer or older packages are not substituted silently;
- Character schemas and operation catalogs load from declarative contracts;
- official mechanics execute deterministically;
- mechanics cannot access persistence, provider, credentials, filesystem, network, time, or randomness;
- package upgrade installs side by side and requires explicit Campaign migration;
- historical Dice outcomes remain unchanged;
- no unauthorized sourcebook content is bundled.

## 137. Definition of Compliance

An implementation complies when:

- packages use stable identity and semantic version;
- installed versions are immutable;
- manifests, hashes, contracts, dependencies, and compatibility are validated;
- activation is separate from installation;
- Campaigns resolve exact package versions;
- missing or incompatible packages block mutation without hiding data;
- arbitrary runtime executable plugins are not loaded;
- official mechanics use reviewed implementation keys;
- declarative package content is bounded and validated;
- Rule Set execution is deterministic and capability-restricted;
- Chronicle validates every output before persistence;
- package upgrades are explicit, recoverable, and historical-state preserving.

## 138. Review Triggers

This ADR must be reviewed if:

- third-party executable Rule Sets become a requirement;
- a scripting language is introduced;
- WebAssembly execution is introduced;
- package signatures become mandatory;
- a package marketplace is introduced;
- runtime hot reload becomes necessary;
- server-side Rule Set execution is added;
- multiple processes share package registries;
- package-specific relational storage becomes necessary;
- official support expands beyond bundled or reviewed packages.

## 139. Deferred Decisions

Later ADRs MAY define:

- signed package format;
- public package marketplace;
- third-party trust policy;
- out-of-process plugin host;
- WebAssembly mechanics runtime;
- scripting language;
- capability-based plugin protocol;
- package hot reload;
- public package SDK;
- package compatibility support window;
- automated package update discovery.

## 140. Final Decision

Chronicle will load Rule Set packages as immutable, versioned, validated capability bundles.

Campaigns will bind to exact package versions.

Declarative content will describe schemas, operations, Preferences, progression, terminology, and Rule Knowledge.

Executable mechanics in the MVP will be limited to reviewed Chronicle implementations.

A Rule Set may calculate what the rules imply.

It may never decide what Chronicle is allowed to trust, persist, reveal, or execute.
