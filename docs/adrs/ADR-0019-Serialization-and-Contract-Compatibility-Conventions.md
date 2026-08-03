---
id: ADR-0019
title: Serialization and Contract Compatibility Conventions
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
  - ADR-0008
  - ADR-0009
  - ADR-0010
  - ADR-0013
  - ADR-0014
  - ADR-0015
  - ADR-0017
  - ADR-0018
  - RFC-0014
  - RFC-0017
  - RFC-0018
  - RFC-0020
  - RFC-0023
  - RFC-0024
  - RFC-0025
  - RFC-0033
  - RFC-0034
  - RFC-0035
  - RFC-0040
  - RFC-0042
---

> **"A serialized contract is a promise to a future version of Chronicle. It must say exactly what it means and fail safely when that promise cannot be kept."**

# Serialization and Contract Compatibility Conventions

## 1. Status

**Proposed**

This ADR defines Chronicle's serialization, schema-versioning, compatibility, and contract-evolution conventions.

The decision is:

- use `System.Text.Json` as the default serializer;
- define explicit versioned contracts at every durable or external boundary;
- avoid serializing Domain entities and EF Core entities directly;
- use dedicated immutable DTOs;
- use stable lowercase camelCase JSON property names;
- use stable semantic keys and canonical UUID strings;
- require explicit polymorphic allowlists;
- prohibit arbitrary type-name serialization;
- ignore unknown properties by default only where forward compatibility is safe;
- reject unknown discriminators, unsupported required versions, malformed identifiers, and invalid enum values;
- distinguish additive compatible change from breaking change;
- persist contract version separately from application version;
- use explicit migration adapters for durable payloads;
- preserve unknown extension data only in narrowly approved pass-through contracts;
- generate and validate JSON Schemas for provider-facing structured outputs and selected package contracts;
- use canonical serialization rules only where hashing, fingerprinting, or signatures require them;
- keep secrets, credentials, raw exceptions, and unrestricted private content out of serialized diagnostics and operational contracts.

The decision becomes **Accepted** after a spike proves:

- deterministic round-trip;
- typed identifier conversion;
- semantic-key validation;
- enum and discriminator behavior;
- backward-compatible additive fields;
- rejection of unknown required variants;
- durable Work Item payload migration;
- Operation Record result migration;
- provider structured-output schema validation;
- package schema validation;
- canonical request fingerprint generation;
- export manifest compatibility;
- safe redaction and secret exclusion.

## 2. Context

Chronicle serializes data across several boundaries:

```text
SQLite JSON columns
Work Item payloads
Operation Record results
provider requests
provider structured outputs
Rule Set package manifests
Character Sheet schema definitions
Rule Knowledge catalogs
backup manifests
portable Campaign exports
diagnostic bundles
application configuration
test fixtures
```

These contracts have different compatibility and security requirements.

A serializer default is not a compatibility policy.

Without explicit conventions, Chronicle risks:

- breaking pending Work Items after an upgrade;
- misreading historical Operation Records;
- accepting provider output with unknown meaning;
- changing request fingerprints because of property order;
- storing secrets accidentally;
- serializing implementation details;
- allowing unsafe polymorphic deserialization;
- conflating application version with contract version;
- silently defaulting unknown values;
- making exports impossible to restore.

ADR-0001 selected `System.Text.Json`.

ADR-0004 permits versioned JSON only where justified.

ADR-0014 requires versioned durable Work Item payloads.

ADR-0017 requires versioned durable error contracts.

This ADR defines the shared conventions.

## 3. Decision Drivers

The conventions prioritize:

1. explicit compatibility;
2. safe deserialization;
3. stable external contracts;
4. readable JSON;
5. deterministic testing;
6. provider schema validation;
7. upgrade resilience;
8. privacy;
9. low framework coupling;
10. clear migration ownership;
11. portable artifacts;
12. future API readiness.

## 4. Decision Summary

Chronicle will use:

```text
Default Serializer
    System.Text.Json

Contract Style
    dedicated immutable DTOs

Property Naming
    camelCase

Identifier Encoding
    canonical lowercase UUID text
    stable semantic-key strings

Contract Version
    explicit field or envelope metadata

Polymorphism
    explicit discriminator
    explicit allowlist

Unknown Properties
    tolerated only where safe

Unknown Discriminator
    rejected

Unknown Required Version
    rejected or migrated explicitly

Enums
    stable string values
    unknown values rejected unless contract defines Unknown

Canonicalization
    only for hashes, fingerprints, signatures

Durable Migration
    explicit version-to-version adapters

Provider Structured Output
    JSON Schema plus independent validation

Domain and Persistence Entities
    never serialized directly
```

## 5. Contract Categories

Chronicle defines the following categories:

```text
Internal Ephemeral Contract
Durable Internal Contract
Provider Contract
Package Contract
Backup Contract
Export Contract
Configuration Contract
Diagnostic Contract
Future Public API Contract
```

## 6. Internal Ephemeral Contract

Used only within one running application version.

Examples:

- command DTO passed in memory;
- query DTO;
- notification payload;
- ViewModel state.

These may evolve with the application but should still follow stable naming and type conventions.

## 7. Durable Internal Contract

Stored across process restarts or upgrades.

Examples:

- Work Item payload;
- Work Item checkpoint;
- Operation Record result;
- staged provider proposal;
- durable error detail.

These MUST have explicit contract versions and migration policy.

## 8. Provider Contract

Sent to or received from Narrative Intelligence.

These MUST have:

- explicit contract key;
- explicit version;
- JSON Schema;
- strict discriminator validation;
- bounded size;
- independent semantic validation.

## 9. Package Contract

Distributed by Rule Set or extension packages.

Examples:

- package manifest;
- Character Sheet schema;
- Rule Operation catalog;
- Preferences definition;
- Rule Knowledge catalog.

Package contracts require compatibility policy independent from Chronicle application releases.

## 10. Backup Contract

Describes installation backup contents and compatibility.

It MUST remain sufficient for:

- validation;
- restore planning;
- migration;
- dependency reporting;
- integrity verification.

## 11. Export Contract

Describes portable Campaign artifacts.

It MUST be:

- versioned;
- portable;
- independent from local paths;
- safe without credentials;
- explicit about identity semantics.

## 12. Configuration Contract

Stores nonsecret application settings and references.

Configuration contracts MUST tolerate some additive evolution but reject unsafe unknown critical values.

## 13. Diagnostic Contract

Contains only approved safe metadata.

It MUST never become a route for unrestricted object serialization.

## 14. Dedicated DTOs

Every durable or external contract uses a dedicated DTO.

Chronicle MUST NOT directly serialize:

- Domain aggregate;
- Domain entity;
- EF Core entity;
- DbContext-tracked object;
- provider SDK request or response type;
- exception object;
- UI control;
- service object.

## 15. Contract Ownership

Each serialized contract MUST have an owning module.

The owner is responsible for:

- schema;
- version;
- migration;
- compatibility tests;
- documentation;
- security classification.

## 16. Contract Key

Important contracts SHOULD use a stable semantic key.

Examples:

```text
chronicle.work.narrative-continuation
chronicle.operation.dice-roll-result
chronicle.narrator.output
chronicle.export.campaign
chronicle.backup.manifest
chronicle.ruleset.package-manifest
```

## 17. Contract Version

Contract version is separate from:

- application version;
- package version;
- storage schema version;
- provider model;
- Rule Set version.

## 18. Version Representation

Use semantic versioning or a constrained integer schema version according to contract needs.

Recommended:

```text
Semantic version
    public, package, provider-facing contract

Positive integer
    tightly controlled durable internal payload
```

The choice must be documented per contract family.

## 19. Envelope

Durable and external payloads SHOULD use an envelope when common metadata is required.

Example:

```json
{
  "contractKey": "chronicle.work.narrative-continuation",
  "contractVersion": 2,
  "payload": {
    "campaignId": "0198f245-4df7-7d58-bd33-a92fba88e1ab",
    "sceneId": "0198f245-50f1-7a2a-94bd-8af38bd6e42f"
  }
}
```

## 20. Envelope Fields

An envelope MAY include:

```text
contractKey
contractVersion
createdAtUtc
operationId
correlationId
payload
```

It MUST NOT duplicate metadata already authoritative elsewhere without a reason.

## 21. JSON Naming

JSON properties use lowercase camelCase.

Examples:

```text
campaignId
ruleSetVersion
createdAtUtc
operationStatus
```

## 22. CLR Naming

CLR properties use normal .NET PascalCase.

Serialization attributes SHOULD be avoided when the global convention already produces the correct stable name.

Explicit names are appropriate when preserving a published contract.

## 23. Property Stability

Published property names are compatibility commitments.

Renaming a CLR property must not silently rename a published JSON property.

## 24. Required Properties

Properties required for interpretation MUST be marked and validated explicitly.

Missing required properties cause deserialization or validation failure.

## 25. Optional Properties

Optional properties should represent truly optional meaning.

A field must not become optional merely to hide migration complexity.

## 26. Null Semantics

Null MUST have one documented meaning.

It must not ambiguously mean:

- unknown;
- hidden;
- absent;
- not loaded;
- default;
- unsupported.

Use explicit state fields or union variants where needed.

## 27. Default Values

Deserialization MUST NOT silently replace missing required values with dangerous defaults.

Examples:

- empty UUID;
- zero version;
- false authorization flag;
- empty semantic key;
- unknown operation type.

## 28. Identifier Serialization

Strongly typed runtime identifiers serialize as canonical lowercase UUID text.

Example:

```json
{
  "sessionId": "0198f24a-0b37-7db4-9fb8-21cc7f3e8a70"
}
```

## 29. Empty Identifier

All-zero UUID values are rejected for persisted authoritative identities.

## 30. Semantic Key Serialization

Semantic keys serialize as validated lowercase strings.

They are not localized.

## 31. Timestamp Serialization

Persisted Instants use ISO 8601 UTC representation.

Recommended output:

```text
2026-08-01T20:06:00.0000000Z
```

Exact fractional precision should be consistent per contract family.

## 32. Timestamp Parsing

Parsing MUST:

- require an offset or UTC marker for persisted Instants;
- normalize to UTC;
- reject ambiguous local timestamps in durable contracts.

## 33. Duration Serialization

Durations SHOULD use:

- integer milliseconds for internal operational contracts;
- ISO 8601 duration only when external interoperability requires it.

The unit must be explicit in the property name or schema.

## 34. Decimal and Numeric Values

Game mechanics requiring exact decimal values SHOULD use `decimal`, integer units, or explicit rational structures.

Floating-point values must not be used for exact progression, cost, or ledger quantities without review.

## 35. Large Integers

Contracts intended for JavaScript interoperability should avoid unsafe integer ranges or serialize them as strings when required.

The MVP local contracts do not need to optimize for JavaScript unless exposed externally.

## 36. Enum Serialization

Enums SHOULD serialize as stable lowercase semantic strings.

Examples:

```text
pending
completed
failedTerminal
```

The exact casing convention must be consistent.

## 37. Enum Numeric Values

Numeric enum serialization is prohibited in published or durable contracts.

Reordering CLR enum members must not change serialized meaning.

## 38. Unknown Enum Value

Unknown enum strings are rejected unless the contract explicitly defines:

```text
unknown
```

and the caller can operate safely with it.

## 39. Flags Enums

Flags enums are discouraged in external contracts.

Use arrays of stable semantic values where multiple independent capabilities are intended.

## 40. Union Representation

Discriminated unions use an explicit property such as:

```text
kind
type
eventType
resultType
```

## 41. Discriminator Stability

Discriminator values are stable semantic strings.

They are not CLR type names.

## 42. Polymorphic Allowlist

Every supported discriminator maps to one approved concrete DTO.

Unknown values are rejected.

## 43. No Type Name Handling

Chronicle MUST NOT use:

- assembly-qualified CLR type names;
- runtime type metadata from untrusted input;
- unrestricted reflection-based polymorphism;
- `$type` patterns that instantiate arbitrary classes.

## 44. Unknown Properties

Unknown properties MAY be ignored when:

- the contract supports additive forward compatibility;
- missing interpretation cannot change authority;
- security is unaffected;
- required data remains validated.

## 45. Strict Contracts

Unknown properties SHOULD be rejected for:

- provider structured outputs;
- security-sensitive manifests;
- command-like durable payloads where extra data may conceal intent;
- package contracts under strict schema validation.

The policy is declared per contract.

## 46. Extension Data

`JsonExtensionData` or equivalent unknown-field preservation is prohibited by default.

It MAY be used only for approved pass-through contracts.

## 47. Pass-Through Contract

A pass-through contract preserves unknown data without interpreting it.

It must:

- never execute unknown content;
- retain size limits;
- preserve ownership;
- avoid security-sensitive decisions;
- document round-trip guarantees.

## 48. Collections

Collections MUST define:

- maximum count;
- ordering semantics;
- duplicate policy;
- null policy.

## 49. Set Semantics

A semantic set SHOULD serialize in deterministic sorted order when canonical comparison or hashing is required.

Otherwise, consumer code must not infer order.

## 50. Dictionary Keys

Dictionary keys in serialized contracts MUST be stable validated strings.

Complex object keys are prohibited.

## 51. Duplicate JSON Properties

Duplicate property names SHOULD be rejected or treated as invalid for strict contracts.

Ambiguous last-value-wins behavior is unsafe.

## 52. Maximum Depth

Serializer options MUST define bounded maximum depth.

Deep or recursive payloads beyond contract expectations are rejected.

## 53. Maximum Size

Every durable or external contract MUST define a maximum serialized size.

Examples:

- Work Item payload;
- provider response;
- package manifest;
- diagnostics entry;
- export manifest.

## 54. String Limits

Schemas and validators MUST bound important strings.

Examples:

- semantic key;
- title;
- alias;
- error detail;
- source reference;
- provider text field.

## 55. Unicode

Strings use UTF-8.

Normalization SHOULD be applied where machine identity or comparison requires it.

Narrative content is preserved without destructive normalization beyond storage safety.

## 56. Control Characters

Invalid or unsafe control characters are rejected or sanitized according to contract purpose.

Machine keys prohibit them.

## 57. Comments

JSON comments are not accepted in durable and external contracts unless a specific human-authored configuration format explicitly enables them.

## 58. Trailing Commas

Durable and external strict contracts SHOULD reject trailing commas.

Human-authored local configuration MAY permit them only if documented.

## 59. Number Handling

String-to-number coercion is prohibited by default.

The JSON token type must match the schema.

## 60. Case Sensitivity

Published JSON property matching SHOULD be case-sensitive.

Permissive case-insensitive reading may be allowed only for explicitly human-authored local configuration.

## 61. Serialization Options

Chronicle SHOULD define named serializer profiles rather than one global mutable options object.

Recommended profiles:

```text
StrictDurableJson
ProviderContractJson
PackageContractJson
ConfigurationJson
DiagnosticJson
CanonicalJson
```

## 62. Options Immutability

Serializer options SHOULD be constructed once and treated as immutable.

Modules must not mutate global options at runtime.

## 63. Source Generation

`System.Text.Json` source generation SHOULD be used for:

- known durable contracts;
- provider contracts;
- package contracts;
- performance-sensitive paths;
- trimming or AOT readiness.

## 64. Reflection Serialization

Reflection-based serialization MAY remain for low-risk internal tools during MVP.

Critical contracts should migrate to generated metadata.

## 65. Converter Registration

Custom converters must be explicitly registered per profile.

Examples:

- typed identifiers;
- semantic keys;
- version types;
- safe Instant representation;
- result unions.

## 66. Converter Safety

Converters MUST:

- reject malformed input;
- avoid hidden defaults;
- avoid external calls;
- be deterministic;
- have round-trip tests;
- not log raw content.

## 67. Domain Value Objects

Selected stable value objects MAY have dedicated converters.

The serialized shape must be an explicit contract, not an automatic dump of internal fields.

## 68. EF Core JSON Columns

When JSON is stored inside SQLite:

- the DTO shape is versioned;
- a discriminator or contract key identifies the payload;
- EF Core entity shape remains separate;
- migrations can inspect and transform the payload;
- required indexes are not hidden exclusively inside JSON.

## 69. Relational Preference

Stable queryable fields SHOULD remain relational columns.

JSON is appropriate for:

- versioned provider-neutral proposals;
- bounded package-defined field payloads;
- durable Work Item payloads;
- immutable contract snapshots.

## 70. JSON Column Migration

JSON payload migrations MUST:

- identify source version;
- parse strictly;
- transform deterministically;
- validate target;
- preserve original until commit;
- be covered by fixtures.

## 71. Work Item Payload Compatibility

A Work Item handler declares supported payload versions.

On startup or claim:

1. read envelope;
2. validate contract key;
3. inspect version;
4. migrate if supported;
5. validate target payload;
6. execute.

## 72. Operation Result Compatibility

Operation Records storing typed results MUST retain enough version metadata to return or migrate historical results after upgrade.

## 73. Durable Error Compatibility

Errors persisted for recovery follow ADR-0017 and use stable codes with explicit detail versions.

## 74. Provider Request Contracts

Provider-neutral requests SHOULD serialize only after Prompt Construction has selected bounded context.

They MUST exclude credentials and local Infrastructure details.

## 75. Provider Response Contracts

Provider response schemas MUST:

- reject unknown event types;
- require contract version;
- bound arrays and strings;
- use stable identifiers supplied by Chronicle;
- prohibit arbitrary tool calls unless a later contract permits them;
- distinguish prose from structured events.

## 76. Schema Validation Is Not Sufficient

A JSON document may satisfy schema and still be semantically invalid.

Chronicle must also validate:

- identifier ownership;
- Campaign scope;
- Rule Set compatibility;
- visibility;
- state versions;
- duplicate effects;
- references;
- business invariants.

## 77. JSON Schema

Chronicle SHOULD generate or maintain JSON Schema for:

- Narrator output;
- Archivist output;
- Campaign Generator output;
- Rule Set package manifest;
- Character Sheet schema language;
- selected Rule Knowledge catalogs.

## 78. Schema Identity

Every schema SHOULD declare:

```text
schemaId
schemaVersion
```

or equivalent metadata.

## 79. Schema Publication

Schemas MAY be stored in the repository under:

```text
contracts/schemas/
```

or generated during build.

## 80. Schema Drift

CI MUST detect when DTO changes and committed schema artifacts disagree.

## 81. Package Contract Validation

Rule Set packages are validated before activation.

Validation includes:

- JSON Schema;
- semantic key grammar;
- version compatibility;
- duplicate identities;
- bounded content;
- prohibited data;
- reference integrity.

## 82. Backup Manifest

The backup manifest MUST be strict and versioned.

It should include:

- manifest contract version;
- application version;
- storage schema version;
- artifact list;
- checksums;
- package dependencies;
- creation Instant;
- identity.

## 83. Export Manifest

The portable export manifest MUST declare:

- export contract version;
- identity mode;
- Campaign identity;
- Rule Set package;
- package version;
- included content classes;
- omitted credential and local-only dependencies;
- checksums.

## 84. Configuration Serialization

Configuration contains only nonsecret settings and credential references.

Unknown optional properties MAY be ignored.

Unknown critical provider, security, or storage settings should block unsafe use.

## 85. Diagnostics Serialization

Diagnostic serialization uses a dedicated allowlisted contract.

It MUST NOT recursively serialize arbitrary application objects.

## 86. Exception Serialization

Exception objects MUST NOT be serialized into durable or user-shareable artifacts.

Use sanitized typed error metadata.

## 87. Secret Exclusion

Secret-bearing types MUST:

- have no serializer contract;
- fail serialization where practical;
- return redacted `ToString()`;
- remain absent from DTOs.

## 88. Redaction Is Defense in Depth

The primary defense is never adding secret values to the serialized object graph.

Redaction filters are secondary protection.

## 89. Canonical JSON

Canonical serialization is required only where byte-stable representation matters.

Examples:

- request fingerprint;
- artifact signature;
- content hash;
- deduplication hash.

## 90. Canonicalization Rules

A canonical profile SHOULD define:

- UTF-8;
- no insignificant whitespace;
- deterministic property order;
- deterministic collection order where semantic;
- stable number formatting;
- canonical timestamps;
- canonical identifiers;
- no ignored default ambiguity.

## 91. Property Order

Normal JSON consumers must not depend on property order.

Canonical hashing may sort properties deterministically.

## 92. Request Fingerprint

The request fingerprint pipeline SHOULD:

1. map command to a dedicated fingerprint DTO;
2. remove nonsemantic and secret fields;
3. canonicalize;
4. hash using an approved cryptographic hash;
5. store only the digest and fingerprint contract version.

## 93. Fingerprint Version

The request fingerprint contract version MUST be persisted.

Changing fingerprint semantics must not create false OperationId conflicts.

## 94. Content Hash

Content hashes identify exact bytes or canonical contract representation.

They do not replace semantic identity.

## 95. Serialization Failure

Serialization and deserialization failures map to stable errors.

Examples:

```text
serialization.invalid-json
serialization.unsupported-version
serialization.unknown-discriminator
serialization.required-property-missing
serialization.value-out-of-range
serialization.payload-too-large
serialization.contract-mismatch
```

## 96. No Partial Acceptance

Strict durable contracts are either fully accepted or rejected.

Chronicle must not partially apply a malformed payload.

## 97. Recovery

If a durable payload cannot be read:

- preserve original bytes;
- mark Work Item or operation as `RecoveryRequired`;
- do not execute;
- expose safe diagnostics;
- allow a future migration tool.

## 98. Original Payload Preservation

Failed migration or parsing should preserve the original durable payload until the user resolves or discards it explicitly.

## 99. Migration Chain

Contract migrations SHOULD be incremental:

```text
v1 → v2
v2 → v3
```

A direct shortcut MAY exist when tested, but all supported source versions must be covered.

## 100. Migration Purity

A contract migration SHOULD be deterministic and side-effect free.

It must not:

- call providers;
- mutate Campaign state;
- generate authoritative Dice;
- depend on local timezone;
- read credentials.

## 101. Migration Time

If a migration needs a timestamp, it receives one explicitly or preserves historical source meaning.

It must not call ambient time.

## 102. Unknown Future Version

A payload with a future unsupported required version is rejected.

Chronicle must not guess.

## 103. Older Reader Compatibility

A newer writer may preserve older-reader compatibility only where the contract explicitly commits to it.

The MVP primarily guarantees supported upgrade direction, not arbitrary downgrade.

## 104. Additive Compatible Change

Usually compatible changes include:

- optional property with safe default meaning;
- new enum value only when readers define unknown handling;
- new optional metadata;
- relaxed maximum where safe.

Compatibility must be tested rather than assumed.

## 105. Breaking Change

Breaking changes include:

- property rename;
- required property addition;
- discriminator semantic change;
- enum meaning change;
- unit change;
- identifier representation change;
- null meaning change;
- property type change;
- removal of required property.

These require a new contract version and migration or compatibility adapter.

## 106. Deprecation

Deprecated properties MAY remain readable for a defined period.

Writers should stop producing them once migration is complete.

## 107. Dual Read

Chronicle MAY support dual-read during migration.

It should prefer the new representation and emit safe diagnostics when legacy fields are encountered.

## 108. Dual Write

Dual-write is discouraged because it can create divergence.

Use it only with an explicit migration plan and consistency tests.

## 109. Contract Fixtures

Every durable or external contract SHOULD have fixture files.

Recommended fixture categories:

```text
valid current
valid prior version
valid additive future sample where supported
missing required field
unknown discriminator
invalid identifier
oversized payload
malicious payload
```

## 110. Golden Files

Golden JSON files MAY be used for stable contract review.

They must not contain secrets or proprietary content.

## 111. Snapshot Test Caution

Snapshot approval is not sufficient alone.

Tests must assert semantic behavior, version, required fields, and security boundaries.

## 112. Round-Trip Testing

Round-trip tests verify:

```text
object → JSON → object
```

They must not be the only compatibility test because both writer and reader may share the same defect.

## 113. Cross-Version Testing

Tests MUST read prior-version fixtures with the current reader and validate expected migration.

## 114. Unknown-Field Testing

Each contract declares and tests whether unknown properties are:

- ignored;
- preserved;
- rejected.

## 115. Malicious Input Testing

Tests MUST include:

- excessive depth;
- huge arrays;
- duplicate properties;
- invalid Unicode;
- unknown discriminator;
- type confusion;
- path traversal strings;
- number overflow;
- malformed UUID;
- arbitrary `$type`;
- embedded credentials;
- oversized strings.

## 116. Performance

Serialization performance matters for:

- provider requests;
- transcript pages where JSON payloads exist;
- Work Item scans;
- package loading;
- exports.

Correctness and safety take precedence over micro-optimization.

## 117. Streaming

Large exports and backups SHOULD use streaming serialization where practical.

Large documents must not require one unbounded in-memory object graph.

## 118. Cancellation

Long serialization operations SHOULD accept cancellation where the API supports it.

Cancellation must not publish partial artifacts as valid.

## 119. Atomic File Publication

Serialized files such as manifests or exports should be written to a temporary location, flushed, validated, and atomically published.

## 120. Encoding

External JSON files use UTF-8 without a required byte-order mark.

## 121. Line Endings

Serialized JSON should use a consistent line-ending policy.

Compact machine-generated JSON may contain no line breaks.

Human-readable manifests MAY be indented.

## 122. Pretty Printing

Pretty printing is allowed for:

- manifests;
- fixtures;
- user-inspectable exports;
- package files.

Canonical hashing uses compact canonical bytes.

## 123. Logging

Serialization logs MAY include:

- contract key;
- version;
- byte size;
- duration;
- result code;
- migration source and target version.

They MUST NOT include full payloads by default.

## 124. Metrics

Useful metrics include:

```text
SerializationDuration
DeserializationFailureCount
UnsupportedContractVersionCount
ContractMigrationCount
ProviderSchemaFailureCount
PackageValidationFailureCount
PayloadSizeRejectedCount
```

## 125. Source Control

Generated schemas, fixtures, and converters SHOULD be versioned where needed for review and reproducibility.

## 126. CI Gates

CI MUST validate:

- schema generation;
- schema drift;
- prior-version fixtures;
- package fixtures;
- provider output fixtures;
- no unsafe polymorphism;
- no secret-bearing DTOs;
- canonical fingerprint stability;
- export and backup manifest compatibility.

## 127. Architecture Tests

Architecture tests MUST reject:

- Domain or EF entity serialization in durable boundaries;
- arbitrary CLR type-name handling;
- provider SDK DTO persistence;
- unversioned Work Item payloads;
- exception serialization;
- secret types registered in serializer profiles;
- numeric enums in durable contracts;
- global mutable serializer options;
- direct use of permissive serializer settings in strict boundaries.

## 128. Prohibited Patterns

### 128.1 Serialize Domain Entity Directly

Use a dedicated contract DTO.

### 128.2 Persist Provider SDK Response

Map to provider-neutral validated contract.

### 128.3 Arbitrary `$type` Deserialization

Use explicit discriminators and allowlists.

### 128.4 Application Version as Payload Version

Contract versions evolve independently.

### 128.5 Unknown Enum Defaults to Zero

Reject or map through explicit unknown policy.

### 128.6 Secret Redaction After Generic Serialization

The secret must not enter the object graph.

### 128.7 Hash Ordinary JSON Without Canonicalization

Property order and formatting may differ.

### 128.8 Durable Payload Without Migration Policy

Every durable version has an owner and policy.

### 128.9 Silent Future-Version Guessing

Unsupported versions fail safely.

### 128.10 Generic Object Metadata Bag

Use typed contracts.

## 129. Alternatives Considered

### Newtonsoft.Json

Mature and flexible, but not selected because `System.Text.Json` is the platform-native default and supports source generation, modern .NET integration, and lower dependency complexity.

### MessagePack or Protocol Buffers Everywhere

Potentially compact and fast, but less human-readable and unnecessary for the local MVP's primary contracts.

They may be considered for future interprocess or network protocols.

### Serialize EF Core Entities

Rejected due to tracking, navigation cycles, hidden fields, and persistence coupling.

### Lenient Deserialization Everywhere

Rejected because permissiveness can hide incompatible or malicious input.

### Strict Deserialization Everywhere

Also not selected universally because some configuration and additive contracts benefit from safe forward compatibility.

Strictness is selected per contract.

### One Global Serializer Configuration

Rejected because provider, package, diagnostics, and configuration boundaries have different requirements.

## 130. Consequences

### Positive

- clear durable compatibility;
- safe polymorphism;
- predictable provider contracts;
- migration-ready Work Items;
- stable manifests and exports;
- reduced implementation leakage;
- deterministic fingerprints;
- stronger security testing;
- future API readiness.

### Negative

- multiple serializer profiles;
- more DTO and migration code;
- schema artifacts require maintenance;
- strict contracts may reject malformed legacy data;
- source generation adds build complexity;
- compatibility fixtures increase repository size.

## 131. Risks

### Contract Drift

Mitigation:

- schema generation;
- golden fixtures;
- CI checks;
- explicit ownership.

### Overly Permissive Reader

Mitigation:

- per-contract strictness;
- semantic validation;
- adversarial tests.

### Overly Strict Additive Evolution

Mitigation:

- compatibility matrix;
- unknown-property policy;
- prior and future fixture tests.

### Migration Bug

Mitigation:

- pure incremental migrations;
- original payload preservation;
- fixture chain;
- rollback and recovery state.

### Secret Leakage

Mitigation:

- no secret DTOs;
- dedicated serializer profiles;
- architecture scans;
- canary tests.

## 132. Technology Spike

Before acceptance, implement:

1. named serializer profiles;
2. typed ID converters;
3. semantic-key converters;
4. strict enum converters;
5. Work Item envelope;
6. Work Item v1-to-v2 migration;
7. Operation result envelope;
8. durable error contract;
9. Narrator structured-output schema;
10. Rule Set package manifest schema;
11. backup manifest;
12. export manifest;
13. canonical fingerprint serializer;
14. schema drift CI test;
15. malicious payload test suite.

## 133. Spike Acceptance

The spike passes when:

- Domain and EF entities never cross durable serialization;
- prior Work Item payloads migrate successfully;
- unknown discriminators are rejected;
- safe additive unknown fields behave according to policy;
- provider output is schema-validated and semantically revalidated;
- canonical request fingerprints remain stable;
- backup and export manifests round-trip;
- unsupported future versions enter recovery rather than execute;
- no synthetic credential appears in any serialized artifact;
- generated schemas match current contract DTOs.

## 134. Definition of Compliance

An implementation complies when:

- `System.Text.Json` is the default serializer;
- durable and external contracts are explicit and versioned;
- Domain, EF, provider SDK, exception, and secret types are not serialized directly;
- JSON naming, identifiers, timestamps, enums, and unions follow the conventions;
- polymorphism uses explicit allowlists;
- unknown-field behavior is declared per contract;
- durable payloads have migration paths;
- provider and package contracts have schema validation;
- canonicalization is used for fingerprints and hashes;
- unsupported versions fail safely;
- CI validates schemas, fixtures, migrations, and secret exclusion.

## 135. Review Triggers

This ADR must be reviewed if:

- a public network API is introduced;
- plugins exchange serialized contracts;
- Chronicle adopts interprocess communication;
- MessagePack or Protocol Buffers becomes justified;
- browser or mobile clients require JavaScript-safe number conventions;
- provider APIs require a materially different schema strategy;
- package marketplace compatibility becomes formalized;
- digital signatures are added to exports or packages;
- a source generator proves unsuitable;
- contract migration volume becomes difficult to maintain.

## 136. Deferred Decisions

Later ADRs MAY define:

- exact schema-generation library;
- exact JSON canonicalization standard;
- exact semantic-version compatibility rules;
- public API serialization;
- binary interprocess contracts;
- plugin contract registry;
- signed package manifests;
- signed portable exports;
- schema documentation generation;
- compatibility support duration;
- contract deprecation policy.

## 137. Final Decision

Chronicle will use explicit, versioned, purpose-owned serialization contracts built with `System.Text.Json`.

Durable and external payloads will use dedicated DTOs, strict identity rules, safe polymorphism, bounded validation, and explicit migration.

Chronicle will be tolerant only where tolerance is safe.

When a payload's meaning is unknown, Chronicle will preserve it, report it, and refuse to invent an interpretation.
