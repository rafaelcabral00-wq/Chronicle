---
id: ADR-0010
title: Identifier Generation and Representation Strategy
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
  - RFC-0001
  - RFC-0006
  - RFC-0017
  - RFC-0018
  - RFC-0025
  - RFC-0033
  - RFC-0034
  - RFC-0040
  - RFC-0042
---

> **"An identifier should tell Chronicle what something is, never force a human to remember what a random string means."**

# Identifier Generation and Representation Strategy

## 1. Status

**Proposed**

This ADR defines Chronicle's identifier strategy for:

- Domain entities;
- aggregate roots;
- operations;
- background work;
- provider requests;
- packages;
- schemas;
- contracts;
- imported and exported artifacts;
- database representation;
- logs;
- tests;
- UI routes.

The decision is:

- use strongly typed identifier value objects in Domain and Application code;
- use UUID-compatible 128-bit values for runtime-generated persistent identities;
- prefer UUID version 7 for newly generated time-ordered entity and operation identifiers when supported by the selected runtime and library;
- permit UUID version 4 only for compatibility, external input, or fallback;
- store identifiers in SQLite as fixed-width binary values where practical, with canonical text conversion at external boundaries;
- use stable human-authored string keys for package, schema, operation, field, Preference, and contract identities;
- never allow Narrative Intelligence to create authoritative persistent identifiers;
- use deterministic injected generators in tests;
- preserve identifiers during normal restore and `PreserveIdentity` import;
- remap identifiers explicitly during `CloneOnImport`.

The decision becomes **Accepted** after a spike proves:

- UUID v7 generation;
- monotonic ordering under realistic concurrency;
- round-trip through EF Core and SQLite;
- typed-ID serialization;
- JSON contract compatibility;
- deterministic test generation;
- duplicate detection;
- import remapping;
- route parsing;
- safe logging;
- cross-Campaign reference validation.

## 2. Context

Chronicle contains many identity classes:

```text
Campaign
Character
Session
Act
Scene
Message
Dice Roll
Memory
Relationship
Knowledge Item
Secret
Narrative Plan
Progression Award
Advancement
Preference Snapshot
Operation Record
Work Item
Backup
Export Package
Rule Set Package
Character Schema
Rule Operation
Provider Profile
```

Not all identity is the same.

Some identifiers are:

- generated at runtime;
- persistent;
- opaque;
- unique per installation or globally;
- copied during backup;
- remapped during clone import.

Others are:

- authored by package developers;
- semantically stable;
- readable;
- versioned;
- referenced in schemas and contracts.

A single untyped `Guid` or arbitrary string everywhere would weaken:

- compile-time safety;
- aggregate boundaries;
- cross-Campaign validation;
- serialization clarity;
- logging;
- test readability;
- import behavior.

Conversely, a complex custom identifier system could add unnecessary infrastructure.

This ADR chooses a simple, strongly typed, interoperable strategy.

## 3. Decision Drivers

The strategy prioritizes:

1. compile-time type safety;
2. globally low collision probability;
3. database locality;
4. stable serialization;
5. deterministic testing;
6. import and export portability;
7. readable semantic keys where appropriate;
8. provider neutrality;
9. no database-generated Domain identity;
10. low implementation complexity;
11. forward compatibility;
12. safe logging and diagnostics.

## 4. Decision Summary

Chronicle will use two primary identifier families.

### 4.1 Generated Opaque Identifiers

Use strongly typed wrappers around UUID-compatible 128-bit values.

Examples:

```text
CampaignId
CharacterId
SessionId
SceneId
MessageId
DiceRollId
MemoryId
OperationId
WorkItemId
BackupId
```

New identifiers SHOULD use UUID v7.

### 4.2 Stable Semantic Keys

Use validated language-neutral strings.

Examples:

```text
RuleSetPackageId
CharacterSchemaId
RuleOperationKey
CharacterFieldKey
PreferenceKey
ProgressionCurrencyKey
NarrativeEventTypeKey
ContractKey
```

These keys are human-authored and versioned separately.

## 5. Why Strongly Typed Identifiers

The following must not compile conceptually:

```text
LoadCampaign(CharacterId)
ApplyAdvancement(SessionId)
ResolveDiceRoll(MemoryId)
```

Strongly typed IDs reduce accidental cross-entity substitution.

They also make:

- signatures clearer;
- tests more readable;
- mapping explicit;
- serialization reviewable;
- architecture boundaries stronger.

## 6. Runtime Identifier Format

### Decision

Runtime-generated persistent identifiers use UUID-compatible 128-bit values.

Chronicle SHOULD generate UUID version 7 for new values.

### Rationale

UUID v7 provides:

- extremely low collision probability;
- chronological ordering characteristics;
- better database index locality than fully random UUIDs;
- interoperability with UUID-capable systems;
- no central identity server;
- generation before persistence;
- portability across installations.

## 7. UUID Version 7

UUID v7 encodes time-ordering information while preserving UUID compatibility.

Chronicle uses that ordering property for storage locality and diagnostics only.

UUID timestamps MUST NOT become authoritative Domain time.

## 8. Fallback

If the selected .NET runtime does not provide a sufficient UUID v7 implementation, Chronicle MAY use:

- a small reviewed implementation;
- a maintained library;
- or UUID v4 temporarily.

Any fallback must remain behind `IIdentifierGenerator`.

## 9. UUID Version 4

UUID v4 remains accepted for:

- imported legacy data;
- external compatible artifacts;
- test fixtures;
- migration;
- fallback generation.

Chronicle MUST NOT reject an otherwise valid identifier solely because it is not v7 unless a specific contract requires v7.

## 10. No Sequential Integer Domain IDs

SQLite integer keys are not used as public Domain identity.

Internal surrogate row identifiers MAY exist for storage optimization.

They MUST NOT cross repository boundaries or appear in external contracts.

## 11. No Database-Generated Domain Identity

Chronicle generates persistent identity before database insertion.

### Rationale

This supports:

- aggregate construction;
- Domain Events;
- offline operation;
- idempotency;
- import;
- deterministic tests;
- operation planning before persistence.

## 12. Identifier Value Objects

Each important runtime identity SHOULD have its own value type.

Conceptually:

```csharp
public readonly record struct CampaignId(Guid Value);
public readonly record struct CharacterId(Guid Value);
public readonly record struct OperationId(Guid Value);
```

The implementation may use source generation to reduce repetition.

## 13. Value Object Requirements

A typed identifier MUST provide:

- immutable value;
- equality;
- comparison where useful;
- canonical parsing;
- canonical formatting;
- JSON conversion;
- EF Core conversion;
- safe `ToString()`;
- invalid-empty rejection where required.

## 14. Empty Identifier

The all-zero UUID is invalid for persisted authoritative entities.

Default struct values must be rejected at boundaries.

## 15. Canonical Text Format

External text representation uses lowercase canonical UUID text:

```text
xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
```

No braces.

No uppercase normalization requirement on input, but output is canonical lowercase.

## 16. Compact Display

The UI MAY display a shortened identifier for support and diagnostics.

Example:

```text
01f3a9c2
```

The full identifier remains available for copy through an explicit diagnostics action.

Short forms are never accepted as authoritative input unless uniquely resolved in a controlled local context.

## 17. Identifier Generator Port

Chronicle defines:

```text
IIdentifierGenerator
```

Conceptually:

```csharp
public interface IIdentifierGenerator
{
    CampaignId NewCampaignId();
    CharacterId NewCharacterId();
    SessionId NewSessionId();
    OperationId NewOperationId();
}
```

A generic internal primitive MAY support generated wrappers.

## 18. Generator Ownership

The generator is an Infrastructure service consumed by Application factories and use cases.

Domain entities MAY receive identifiers during creation.

Domain code does not call static global generators.

## 19. Test Identifier Generator

Tests use a deterministic generator.

It SHOULD support:

- predefined sequences;
- stable fixture values;
- duplicate simulation;
- invalid-value simulation;
- reproducible property tests.

## 20. Provider Boundary

Narrative Intelligence may refer only to identifiers supplied by Chronicle.

Provider output containing a new authoritative identifier is rejected unless the contract explicitly defines a nonauthoritative proposal key.

## 21. Provider Proposal References

When a provider proposes multiple related items in one response, it MAY use temporary local proposal references.

Example:

```text
proposal-memory-1
proposal-relationship-2
```

These references:

- exist only inside one structured response;
- are not persisted as Domain identity;
- are mapped by Chronicle after validation;
- cannot reference another Campaign.

## 22. OperationId

`OperationId` is a generated persistent identifier representing one user or system intention.

It is central to:

- idempotency;
- retry;
- recovery;
- logs;
- Work Items;
- result lookup;
- ambiguous completion.

## 23. OperationId Creation

A new OperationId is created when a new intention begins.

A retry of the same intention reuses the same OperationId.

A materially different intention requires a new OperationId.

## 24. CorrelationId

`CorrelationId` groups related operations and diagnostics.

It does not enforce idempotency.

A CorrelationId MAY span:

- one UI workflow;
- one Session finalization;
- one backup and validation sequence;
- one import inspection and commit.

## 25. Trace Identifiers

OpenTelemetry or .NET Activity trace identifiers remain diagnostic.

They do not replace:

- OperationId;
- CorrelationId;
- Domain entity IDs.

## 26. WorkItemId

A Work Item has its own identity.

It also references the OperationId whose intention it continues.

Multiple Work Items MAY belong to one OperationId when a workflow has several durable stages.

## 27. MessageId and Sequence

A Message has:

- opaque `MessageId`;
- explicit sequence number within the Scene.

Identifier order does not replace Message sequence.

Even UUID v7 ordering is insufficient as the authoritative transcript order.

## 28. Session, Act, and Scene Sequence

Sessions, Acts, and Scenes each have:

- opaque identity;
- explicit parent-scoped sequence number.

Identity and sequence serve different purposes.

## 29. Stable Semantic Keys

Semantic keys are authored strings with stable meaning.

Examples:

```text
chronicle.ruleset.werewolf
werewolf.character.default
werewolf.test.basic
werewolf.field.rage
werewolf.preference.difficulty-mode
chronicle.narrative.roll-requested
```

## 30. Semantic Key Rules

Semantic keys MUST be:

- language-neutral;
- lowercase;
- ASCII where practical;
- dot-separated or another approved stable grammar;
- bounded in length;
- immutable once Stable;
- independent from display text;
- versioned externally when meaning changes.

## 31. Semantic Key Grammar

Recommended grammar:

```text
[a-z0-9]+([.-][a-z0-9]+)*
```

Exact grammar may vary by key family.

Whitespace and control characters are prohibited.

## 32. Display Names

Display names are localized and mutable.

They MUST NOT be used as identity.

Example:

```text
RuleOperationKey = "werewolf.test.basic"
DisplayName = "Basic Test"
```

## 33. RuleSetPackageId

A Rule Set package uses a stable semantic package identifier.

Recommended namespace:

```text
chronicle.ruleset.{name}
```

or:

```text
{publisher}.ruleset.{name}
```

The exact ecosystem namespace policy requires a later package-governance ADR.

## 34. Rule Set Version

Package identity and package version are separate.

```text
RuleSetPackageId
RuleSetVersion
```

must never be concatenated into one ambiguous string internally.

## 35. CharacterSchemaId

A Character schema uses a stable key independent from version.

Example:

```text
werewolf.character.player
```

The schema binding includes:

```text
CharacterSchemaId
CharacterSchemaVersion
```

## 36. RuleOperationKey

A mechanical operation uses a stable semantic key.

Example:

```text
werewolf.test.basic
```

Meaningful incompatible changes require an operation version change and potentially a new key.

## 37. Field Keys

Character Sheet fields use stable semantic keys.

Example:

```text
werewolf.field.attributes.strength
```

Field keys survive localization and UI reordering.

## 38. Preference Keys

Preference keys are package-owned and stable.

Example:

```text
werewolf.preference.botch-policy
```

## 39. Narrative Event Type Keys

Structured output event types use stable Chronicle-owned keys.

Example:

```text
chronicle.narrative.roll-requested
chronicle.narrative.scene-transition-proposed
```

## 40. Contract Versions

A contract SHOULD use:

- stable ContractKey;
- separate semantic version.

Example:

```text
ContractKey = "chronicle.narrator.output"
ContractVersion = "1.0.0"
```

## 41. Persistence Representation

### Decision

SQLite SHOULD store opaque UUID identifiers in a fixed-width 16-byte binary representation when EF Core mapping and query tooling remain reliable.

### Rationale

Binary storage offers:

- compact fixed width;
- consistent comparison;
- smaller indexes;
- no text-format variation.

## 42. Text Storage Fallback

Canonical UUID text storage is acceptable if binary mapping introduces disproportionate complexity or tooling friction.

The spike will compare:

```text
BLOB(16)
TEXT(36)
```

The final choice must remain hidden behind EF Core converters.

## 43. Byte Ordering

If UUID v7 is stored as binary, the converter MUST preserve the intended sortable byte order.

The exact byte-order strategy must be documented and tested.

Native `Guid` byte-array ordering must not be assumed without verification.

## 44. Database Indexes

Indexes on UUID v7 identifiers may benefit from improved insertion locality.

Chronicle MUST still use explicit sequence and timestamp columns for ordered Domain queries.

## 45. Foreign Keys

All foreign-key columns use the same physical representation as the referenced identifier.

Mixed text and binary representations for the same ID family are prohibited.

## 46. EF Core Converters

Typed identifiers use explicit EF Core value converters.

Converters MUST:

- round-trip exactly;
- reject invalid empty values;
- use stable binary or text representation;
- be covered by integration tests;
- avoid reflection-heavy runtime surprises.

## 47. EF Core Comparers

Where required, explicit value comparers SHOULD be configured for typed identifiers and collections.

## 48. JSON Serialization

Typed IDs serialize as canonical strings.

Example:

```json
{
  "campaignId": "0198f245-4df7-7d58-bd33-a92fba88e1ab"
}
```

They MUST NOT serialize as:

- internal object shape;
- byte array;
- base64;
- provider-specific type.

## 49. JSON Parsing

JSON parsers MUST reject:

- empty UUID;
- malformed text;
- oversized values;
- control characters;
- wrong identifier type where the schema can distinguish it.

## 50. Route Representation

Desktop routes use canonical identifier text.

Example:

```text
campaign/0198f245-4df7-7d58-bd33-a92fba88e1ab/play
```

Route parsing converts immediately to a typed identifier.

## 51. File Names

Opaque IDs MAY appear in managed internal filenames.

User-visible exports SHOULD use readable sanitized names plus a short identifier suffix when collision protection is needed.

Example:

```text
ashes-of-winter-01ab23cd.chronicle
```

The full authoritative ID remains inside the manifest.

## 52. Logs

Logs SHOULD record full opaque identifiers for correlation when safe.

Logs SHOULD NOT record display names merely to make IDs readable.

## 53. Diagnostic Reference Code

A user-facing reference code MAY derive from:

- OperationId;
- timestamp;
- error category.

It must not allow reconstruction of sensitive content.

## 54. Backup

Backup preserves identifiers exactly.

Restoring a backup to replace the same installation state retains all IDs.

## 55. Portable Export

Portable exports include stable canonical identifiers when required by the format.

The export manifest declares identity semantics.

## 56. Import Modes

Chronicle supports identity behavior such as:

```text
InspectOnly
PreserveIdentity
CloneOnImport
```

## 57. PreserveIdentity

`PreserveIdentity` retains source IDs.

It is valid only when:

- no conflicting entity exists;
- the package is a restore or trusted transfer;
- ownership and integrity checks pass.

## 58. CloneOnImport

`CloneOnImport` generates new IDs for imported Campaign-scoped entities.

It MUST rewrite all internal references consistently.

## 59. Import Identity Map

Clone import uses an explicit map:

```text
SourceCampaignId → NewCampaignId
SourceCharacterId → NewCharacterId
SourceSessionId → NewSessionId
...
```

The map exists only for the import operation and may be retained as safe audit metadata.

## 60. External References

References to external Rule Set keys and package IDs are not remapped during Campaign clone.

Runtime Campaign entity IDs are remapped.

## 61. Collision Handling

A generated-ID collision is extremely unlikely but still handled.

On unique-constraint violation:

- verify whether the OperationId already committed;
- otherwise generate a new ID within a bounded retry;
- log a safe Critical or Error event if collision is confirmed;
- never overwrite the existing entity.

## 62. Monotonic Generation

UUID v7 generation SHOULD preserve nondecreasing order within one process under the selected implementation.

Chronicle does not require globally strict monotonicity across machines.

## 63. Clock Regression

The generator MUST define behavior when the system clock moves backward.

Preferred behavior:

- preserve uniqueness;
- preserve local monotonic ordering where practical;
- do not block indefinitely;
- record a safe diagnostic event if material.

## 64. High-Throughput Generation

The generator MUST remain unique when many IDs are created within the same millisecond.

Tests should simulate bursts beyond normal MVP use.

## 65. Security

Identifiers are not secrets.

However, they must not be treated as authorization.

Knowledge of an ID does not grant access.

## 66. Enumeration Resistance

UUIDs reduce trivial sequential enumeration compared with integers.

Chronicle still validates:

- Campaign ownership;
- entity type;
- visibility;
- operation authorization.

## 67. Cross-Campaign Validation

Every identifier reference in a Campaign-scoped command or provider event MUST be validated against the active Campaign.

A valid CharacterId from another Campaign is still invalid.

## 68. Type Confusion

Typed IDs reduce local type confusion.

External structured data still requires schema validation to prevent one UUID string from being interpreted as the wrong entity type.

## 69. Information Leakage

UUID v7 contains approximate creation-time information.

Chronicle accepts this tradeoff for local application identity and database locality.

It MUST NOT use UUID v7 where creation-time leakage would be unacceptable without review.

## 70. Public Sharing

Portable exports may expose UUIDs.

They should not expose:

- local paths;
- user account identifiers;
- machine identifiers;
- credential aliases unless required and safe.

## 71. No Machine Identity

Chronicle IDs MUST NOT embed:

- MAC address;
- hostname;
- Windows SID;
- username;
- process ID;
- device ID.

## 72. No Hash-of-Content Identity for Mutable Entities

Mutable entities use generated IDs.

Content hashes may support:

- duplicate detection;
- integrity;
- immutable artifact identity.

They do not replace entity identity.

## 73. Immutable Artifact Hashes

Backups, exports, source files, and package artifacts MAY use cryptographic content hashes alongside their IDs.

The hash and ID serve different purposes.

## 74. Package Identity

Package identity is semantic and publisher-controlled.

Package artifact hashes verify exact bytes.

Example:

```text
PackageId = chronicle.ruleset.werewolf
PackageVersion = 0.1.0
ArtifactHash = ...
```

## 75. Source Document Identity

Rule Knowledge sources use generated SourceId plus a content hash.

A changed document retains or changes SourceId according to source-registration semantics, while content hash detects revision.

## 76. Deterministic Fixture IDs

Test fixtures SHOULD use stable explicit identifiers.

Example:

```text
CampaignId.Parse("01900000-0000-7000-8000-000000000001")
```

Fixture IDs should be visually grouped by entity type where valid UUID syntax permits.

## 77. Property-Based Tests

Property tests SHOULD generate:

- valid v7 IDs;
- valid v4 IDs;
- empty IDs;
- malformed strings;
- duplicate values;
- large concurrent batches.

## 78. Source Generation

Chronicle MAY use a source generator for typed ID boilerplate.

The generator must be:

- simple;
- repository-owned or well-reviewed;
- deterministic;
- testable;
- optional to understand generated public behavior.

## 79. Third-Party Strong ID Libraries

A third-party typed-ID library MAY be selected after review.

Selection criteria:

- license;
- maintenance;
- UUID v7 support;
- System.Text.Json support;
- EF Core support;
- source-generation behavior;
- AOT compatibility where relevant;
- no unwanted global conventions.

## 80. Public API

Typed IDs are public contracts where external package authors need them.

Their textual representation and equality semantics must remain stable.

## 81. Backward Compatibility

Changing the internal wrapper implementation is allowed if:

- canonical text remains stable;
- database conversion migrates safely;
- JSON contracts remain compatible;
- equality semantics remain unchanged.

## 82. Identifier Migration

If Chronicle moves from text to binary storage:

- canonical IDs remain unchanged;
- migration converts representation only;
- row counts and foreign keys are validated;
- backups and fixtures cover the change.

## 83. Legacy Integer Migration

If any prototype uses integer IDs, migration must:

- generate new typed UUIDs;
- build a complete old-to-new map;
- rewrite references;
- validate counts and ownership;
- preserve external semantic keys.

## 84. Stable Key Evolution

A semantic key must not change merely for naming preference after release.

If a rename is necessary:

- retain alias mapping;
- migrate references;
- preserve old-key compatibility where supported;
- document deprecation.

## 85. Key Namespace Ownership

Chronicle owns the `chronicle.*` namespace.

Rule Set packages own their declared package namespace.

Community namespace governance is deferred.

## 86. Key Collision

Package validation MUST reject duplicate semantic keys within a package scope.

Registry-level duplicate package identity requires publisher and trust policy.

## 87. Case Normalization

Semantic keys normalize to lowercase.

Input using uppercase SHOULD be rejected or normalized consistently before persistence.

The selected policy must be uniform.

## 88. Unicode

Display names may use Unicode.

Machine semantic keys SHOULD remain ASCII unless future internationalized identifiers are explicitly adopted.

## 89. Length Limits

Every identifier family MUST have explicit length limits.

Recommended initial maxima:

```text
UUID canonical text
    fixed 36 characters

Semantic key
    128 characters

Alias
    64 characters

Provider profile key
    128 characters
```

Exact limits may be adjusted through implementation validation.

## 90. Error Model

Identifier errors SHOULD map to typed results such as:

```text
IdentifierMissing
IdentifierMalformed
IdentifierEmpty
IdentifierTypeMismatch
IdentifierNotFound
IdentifierConflict
IdentifierBelongsToAnotherCampaign
SemanticKeyInvalid
SemanticKeyDuplicate
SemanticKeyDeprecated
```

## 91. User-Facing Errors

Normal users SHOULD not be asked to type UUIDs.

When an identifier error occurs, the UI presents:

- affected entity type;
- safe reference code;
- recovery action;
- diagnostics option.

## 92. Observability

Useful identifier-related events include:

```text
IdentifierGenerationFailed
IdentifierCollisionDetected
CrossCampaignIdentifierRejected
UnknownSemanticKeyRejected
ImportIdentityRemapCompleted
LegacyIdentifierMigrated
ClockRegressionDetected
```

## 93. Logging Restrictions

Logs MUST NOT infer or include private display content merely because an ID lookup is available.

## 94. Performance

The spike SHOULD compare:

- UUID v7 insertion locality;
- UUID v4 insertion;
- binary storage;
- text storage;
- index size;
- lookup latency;
- migration complexity.

The decision prioritizes correctness and simplicity over micro-optimization.

## 95. Testing Strategy

The identifier implementation requires:

```text
Unit Tests
Serialization Tests
Persistence Integration Tests
Concurrency Tests
Import Tests
Migration Tests
Architecture Tests
Security Tests
```

## 96. Unit Tests

Unit tests SHOULD cover:

- create;
- parse;
- format;
- equality;
- comparison;
- empty rejection;
- semantic-key grammar;
- case normalization;
- safe `ToString()`.

## 97. Serialization Tests

Tests MUST cover:

- System.Text.Json round-trip;
- canonical lowercase output;
- invalid input;
- null handling;
- typed ID separation;
- backward-compatible legacy UUID forms where supported.

## 98. Persistence Tests

SQLite integration tests MUST cover:

- EF Core mapping;
- binary or text representation;
- foreign keys;
- indexes;
- unique constraints;
- ordering behavior;
- migrations;
- raw SQL read-model conversion.

## 99. Concurrency Tests

Tests SHOULD generate large concurrent batches and assert:

- uniqueness;
- valid UUID version;
- local monotonic ordering where promised;
- safe clock regression behavior.

## 100. Import Tests

Tests MUST cover:

- PreserveIdentity without collision;
- PreserveIdentity collision;
- CloneOnImport remapping;
- complete reference rewrite;
- cross-Campaign rejection;
- package semantic keys unchanged.

## 101. Security Tests

Tests MUST prove:

- ID knowledge does not bypass ownership;
- wrong ID type is rejected;
- provider cannot create authoritative IDs;
- route manipulation cannot access another Campaign;
- identifiers contain no machine or user information;
- logs remain content-safe.

## 102. Required Test Cases

Tests MUST cover:

- generate CampaignId;
- generate OperationId;
- valid UUID v7;
- valid imported UUID v4;
- empty UUID;
- malformed UUID;
- duplicate generator output;
- collision retry;
- clock regression;
- same-millisecond burst;
- JSON round-trip;
- SQLite round-trip;
- canonical text;
- route parse;
- wrong-type parse path;
- semantic key valid;
- semantic key uppercase;
- semantic key whitespace;
- semantic key too long;
- duplicate RuleOperationKey;
- provider temporary proposal reference;
- provider invented persistent ID rejected;
- restore preserves IDs;
- clone remaps IDs;
- backup preserves IDs;
- text-to-binary storage migration;
- deterministic fixture generator.

## 103. Architecture Tests

Architecture tests MUST reject:

- raw `Guid` parameters in public Domain or Application APIs where a typed ID exists;
- database integer IDs escaping Infrastructure;
- provider-generated Domain IDs;
- display names used as keys;
- static global identifier generation in Domain code;
- machine-derived identifiers;
- untyped string IDs in stable contracts where a typed contract exists.

## 104. Prohibited Patterns

### 104.1 Raw Guid Everywhere

Important entity identities use distinct value types.

### 104.2 Database Identity as Domain Identity

SQLite row IDs do not define Chronicle entities.

### 104.3 Timestamp as Identity

Timestamps are not unique identity.

### 104.4 Display Name as Key

Display text is mutable and localized.

### 104.5 Provider-Created Persistent IDs

Chronicle generates authoritative identity.

### 104.6 ID as Authorization

Ownership and visibility are always validated.

### 104.7 Sequence Number as Global Identity

Sequence is parent-scoped ordering only.

### 104.8 Hidden Machine Information

IDs do not embed device or account data.

### 104.9 Silent Semantic-Key Rename

Stable keys require migration and compatibility handling.

### 104.10 Shortened ID as Stored Identity

Short IDs are display conveniences only.

## 105. Alternatives Considered

### UUID v4 Everywhere

Strengths:

- simple;
- widely supported;
- highly random.

Not selected as the preferred generator because UUID v7 offers better temporal locality while preserving UUID interoperability.

UUID v4 remains accepted for compatibility and fallback.

### ULID

Strengths:

- sortable;
- compact textual form;
- human-friendly base32.

Not selected because:

- UUID interoperability is stronger across .NET, SQLite tooling, and external contracts;
- UUID v7 provides similar temporal-ordering benefits;
- introducing a separate format would increase conversion and ecosystem complexity.

### Snowflake-Style 64-Bit IDs

Rejected because they require:

- machine or worker identity;
- clock coordination;
- collision strategy;
- centralized allocation assumptions.

They also risk leaking deployment topology.

### Sequential Integers

Rejected because they:

- require database generation;
- complicate offline creation;
- collide across imports;
- are easy to enumerate;
- weaken portability.

### Content-Addressed IDs

Rejected for mutable Domain entities.

Hashes remain useful for immutable artifacts and integrity.

## 106. Consequences

### Positive

- compile-time safety;
- provider-independent identity;
- low collision risk;
- better database locality than random UUIDs;
- easy offline generation;
- stable JSON and route representation;
- deterministic tests;
- explicit clone-import behavior;
- semantic keys remain readable.

### Negative

- typed wrappers add code;
- UUID v7 support may require a library or helper;
- binary database representation requires careful byte ordering;
- v7 exposes approximate creation time;
- semantic-key governance requires discipline;
- mapping and converters need testing.

## 107. Risks

### Incorrect UUID Byte Ordering

Mitigation:

- explicit converter;
- integration tests;
- compare binary and text behavior;
- document representation.

### Typed-ID Boilerplate

Mitigation:

- source generation or reviewed helper;
- small consistent API;
- no overengineering.

### Clock Regression

Mitigation:

- monotonic generator behavior;
- uniqueness-first policy;
- diagnostic event;
- concurrency tests.

### Semantic-Key Drift

Mitigation:

- package validation;
- deprecation aliases;
- migration fixtures;
- stable namespace policy.

### Accidental Raw Guid Leakage

Mitigation:

- analyzers;
- architecture tests;
- code review;
- public API conventions.

## 108. Technology Spike

Before acceptance, implement:

1. typed IDs for Campaign, Character, Session, Message, Dice Roll, Operation, and Work Item;
2. UUID v7 generator;
3. deterministic test generator;
4. System.Text.Json converters;
5. EF Core converters;
6. SQLite binary and text comparison;
7. canonical route parsing;
8. OperationId retry flow;
9. provider proposal-reference mapping;
10. PreserveIdentity import;
11. CloneOnImport remapping;
12. collision simulation;
13. clock-regression simulation;
14. architecture analyzer for raw Guid usage;
15. performance comparison.

## 109. Spike Acceptance

The spike passes when:

- all generated IDs are valid and unique;
- new IDs use UUID v7;
- imported v4 IDs round-trip;
- typed IDs cannot be substituted accidentally in public APIs;
- JSON uses canonical strings;
- SQLite mapping preserves exact values;
- clone import rewrites every internal reference;
- provider output cannot create authoritative IDs;
- deterministic tests use stable fixtures;
- the chosen storage representation has acceptable complexity and performance.

## 110. Definition of Compliance

An implementation complies when:

- runtime entity IDs use strongly typed UUID wrappers;
- new persistent IDs use UUID v7 when available;
- semantic identities use validated stable keys;
- Chronicle generates authoritative IDs;
- providers use supplied IDs or temporary proposal references only;
- OperationId is reused for retries of the same intention;
- SQLite representation is consistent;
- JSON representation is canonical;
- restore preserves identity;
- clone import remaps identity explicitly;
- tests inject deterministic generators;
- architecture tests reject untyped leakage.

## 111. Review Triggers

This ADR must be reviewed if:

- .NET introduces materially different built-in identifier support;
- UUID v7 implementation proves unreliable;
- synchronization requires globally ordered event IDs;
- multiplayer introduces server-assigned identity;
- package registry requires publisher-qualified identities;
- binary SQLite storage causes tooling problems;
- public APIs require a different interoperable format;
- security review rejects creation-time leakage for some ID family.

## 112. Deferred Decisions

Later ADRs MAY define:

- exact typed-ID source generator;
- exact SQLite binary byte order;
- exact semantic-key namespace governance;
- publisher identity format;
- multiplayer server identity;
- public API short codes;
- human-readable Campaign share codes;
- cryptographic artifact identity;
- distributed event identifiers.

## 113. Final Decision

Chronicle will use strongly typed UUID-based identifiers for runtime entities and stable semantic string keys for authored contracts.

New persistent IDs will prefer UUID version 7.

SQLite and JSON mappings will preserve one canonical identity across storage, routes, logs, backup, restore, and import.

Chronicle should know exactly which Campaign, Character, Roll, Memory, and operation it is handling.

The player should rarely need to see the full identifier at all.
