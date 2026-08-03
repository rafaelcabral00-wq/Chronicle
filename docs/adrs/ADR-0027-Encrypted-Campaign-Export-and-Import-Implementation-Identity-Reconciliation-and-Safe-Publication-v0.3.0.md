---
id: ADR-0027
title: Encrypted Campaign Export and Import Implementation, Identity Reconciliation, and Safe Publication
status: Proposed
version: 0.3.0
owner: Chronicle Team
last_updated: 2026-08-02
category: Technology
supersedes:
  - ADR-0027@0.2.0
  - ADR-0027@0.1.0
superseded_by: null
depends_on:
  - RFC-0012
  - RFC-0017
  - RFC-0019
  - RFC-0021
  - RFC-0025
  - RFC-0033
  - RFC-0034
  - ADR-0004
  - ADR-0024
  - ADR-0026
  - ADR-0033
  - ADR-0034
  - ADR-0036
  - ADR-0041
implements:
  - RFC-0034
related_to:
  - ADR-0028
  - ADR-0037
  - ADR-0042
  - ADR-0044
---

> **"Import is not a database copy. It is a validated translation from one Campaign identity graph into another."**

# Encrypted Campaign Export and Import Implementation, Identity Reconciliation, and Safe Publication

## 1. Status

**Proposed**

This ADR defines the concrete implementation architecture for encrypted portable Campaign export and import.

The decision is:

- implement portable Campaign export through explicit contract projections;
- serialize no EF Core entity or raw database row as the public contract;
- encrypt every published Campaign package;
- use an artifact-specific key domain independent from database keys, backup keys, and provider credentials;
- support clean-install import using explicit export recovery material;
- stream serialization and authenticated encryption with bounded memory;
- use no durable plaintext package or import staging;
- validate a completed export by reopening and decrypting it before publication;
- treat import packages as hostile input;
- inspect, authenticate, bound, decrypt, deserialize, and validate before any authoritative mutation;
- use a separate encrypted SQLite staging database for complex import validation;
- create an explicit typed identity map;
- use `CloneOnImport` as the only MVP publication mode;
- allocate new local IDs for all authoritative imported entities;
- preserve source lineage as provenance, not local authority;
- publish the new Campaign in one bounded authoritative transaction where feasible;
- make retries idempotent through OperationId, artifact fingerprint, and persisted identity mapping;
- never merge or overwrite an existing Campaign in MVP;
- preserve exact historical Dice evidence, results, package bindings, Knowledge, Memories, progression, and corrections;
- never recompute historical Dice or silently upgrade package bindings;
- exclude credentials, installation keys, provider attempts, raw provider response staging, and machine-local settings;
- permit restricted-mode publication when exact Rule Set execution is unavailable but data can be preserved safely;
- use English contract and error keys while preserving authored content languages.

## 2. Context

Portable export and import cross several trust and identity boundaries:

- database schema to public contract;
- encrypted local storage to encrypted portable artifact;
- one installation's IDs to another installation's IDs;
- one package inventory to another;
- untrusted artifact to authoritative local state;
- completed Campaign history to a new local aggregate.

A naive implementation could:

- leak unrelated Campaign data;
- expose database or provider secrets;
- serialize EF-specific structure permanently;
- import malicious paths or oversized content;
- collide with existing IDs;
- publish half a Campaign;
- recalculate historical mechanics using a different package;
- create duplicates after retry.

## 3. Architectural Components

Recommended components:

```text
CampaignExportCoordinator
CampaignExportReadinessInspector
CampaignPortableProjector
PortableContractSerializer
PortableArtifactEncryptor
PortableArtifactValidator
CampaignImportCoordinator
PortableArtifactInspector
PortableArtifactDecryptor
ImportStagingStore
ImportContractValidator
ImportIdentityMapper
ImportCompatibilityResolver
ImportedCampaignPublisher
ImportRecoveryService
```

## 4. Project Placement

Recommended placement:

```text
Chronicle.Application
    coordinators, readiness, identity policy, publication use cases

Chronicle.Contracts
    portable DTOs and contract versions

Chronicle.Infrastructure
    streaming encryption, artifact framing, safe files

Chronicle.Persistence.Sqlite
    coherent reads, encrypted staging database, final publication

Chronicle.RuleSets.Abstractions
    package compatibility and payload contracts

Chronicle.Presentation.Desktop
    user interaction and progress
```

## 5. Export Operation

Every export begins with:

```text
OperationId
CampaignId
Destination
RecoveryMode
ExpectedCampaignVersion
```

## 6. Export Idempotency

The same OperationId and request fingerprint must return or recover the same logical result.

## 7. Export Readiness Inspection

Before projection, inspect:

- Campaign existence;
- expected version;
- migration and restore state;
- unresolved critical Operations;
- pending Narrative acceptance;
- Dice generation and resolution state;
- correction workflows;
- package migration state;
- package bindings;
- destination capacity;
- recovery-material readiness.

## 8. Readiness Result

```text
Ready
ReadyWithWarnings
Blocked
```

## 9. Blocking Conditions

Block export when a coherent portable snapshot cannot be guaranteed.

Examples:

```text
DiceRoll.Generating
DiceRoll.RecoveryRequired
OperationRecord.Committing with unknown outcome
NarrativeTurn acceptance outcome unknown
active migration
active restore
unresolved correction publication
```

## 10. Coherent Snapshot

Export reads one stable Campaign snapshot.

## 11. Snapshot Strategy

Use one of:

- validated SQLite read snapshot;
- bounded transaction producing immutable projections;
- equivalent consistent mechanism.

## 12. No Long Write Transaction

Projection, serialization, compression, encryption, and destination publication occur outside long write transactions.

## 13. Export Query Boundary

Queries must filter by the selected CampaignId and required ownership graph.

## 14. Cross-Campaign Isolation

Every exported record must prove ownership by the selected Campaign or an explicitly permitted shared package descriptor.

## 15. Portable Projection

Application maps persistence and Domain state into versioned portable DTOs.

## 16. Projection Order

Recommended logical order:

1. Campaign;
2. package bindings;
3. hierarchy;
4. Characters;
5. Knowledge;
6. Memories;
7. Messages and narrative provenance;
8. Dice;
9. progression;
10. corrections;
11. Campaign-scoped preferences;
12. lineage and integrity metadata.

## 17. Projection Purity

Projection performs no authoritative mutation.

## 18. Portable IDs

The artifact uses stable portable IDs scoped to the package.

## 19. Portable ID Strategy

Portable IDs may preserve source IDs inside the artifact for internal reference consistency.

Import still allocates new local IDs.

## 20. Contract Serialization

Use deterministic serialization where hashing or signatures depend on byte stability.

## 21. Contract Version

Every record family includes a contract version directly or through its containing section.

## 22. No Runtime Type Serialization

Do not serialize CLR type names or arbitrary polymorphic runtime metadata.

## 23. Unknown Fields

Unknown-field handling follows the declared contract version policy.

## 24. Artifact Framing

Use a Chronicle-owned versioned framing format.

## 25. Public Header

The public header contains only safe unlock and compatibility metadata.

## 26. Encrypted Payload

All private Campaign metadata remains inside authenticated encryption.

## 27. Export Data Key

Generate a new high-entropy data key per artifact.

## 28. Key Wrapping

Protect the data key using the selected export recovery method.

## 29. Recovery Material

The MVP supports at least one platform-neutral clean-install method, such as:

- export secret with memory-hard KDF;
- export recovery key file.

## 30. No Windows-Only Portability

The artifact must not require the source Windows Credential Manager entry.

## 31. Authenticated Encryption

Use the security-approved authenticated-encryption framing.

## 32. Streaming Pipeline

Recommended:

```text
Portable Projector
    → Contract Serializer
    → Optional bounded compression
    → Authenticated Encryption
    → Encrypted Staging File
```

## 33. Compression Ordering

When compression is used, compress before encryption.

## 34. Compression Safety

Import applies strict decompression limits.

## 35. No Plaintext Staging

The pipeline must not persist an intermediate plaintext package.

## 36. Encrypted Staging File

The output is written under an opaque temporary name with restrictive permissions.

## 37. Export Validation

After writing:

1. close streams;
2. reopen artifact;
3. parse public header;
4. unlock using configured recovery method;
5. verify authentication;
6. parse manifest;
7. validate all section hashes;
8. validate reference graph;
9. validate Dice evidence graph;
10. scan contract data for prohibited secret classes;
11. mark ready to publish.

## 38. Secret Exclusion

Export validation rejects evidence of:

- provider credentials;
- database keys;
- backup keys;
- recovery secrets;
- connection strings;
- Credential Manager identifiers not explicitly safe;
- raw provider response content;
- local absolute paths;
- unrelated Campaign canaries.

## 39. Publication

Publish through same-volume atomic rename where possible.

## 40. Final Name

Human-readable file naming is convenience only.

Artifact identity comes from its internal ID and fingerprint.

## 41. Export Result

The result includes:

```text
ArtifactId
ArtifactFingerprint
ArtifactLocation
ContractVersion
EncryptionSchemeKey
CreatedAtUtc
SizeBytes
ValidationStatus
```

No secret material is returned.

## 42. Import Operation

Every import begins with:

```text
OperationId
ArtifactLocation
RecoveryMaterialInput
ImportMode = CloneOnImport
```

## 43. Artifact Fingerprint

Compute a stable fingerprint before expensive processing where safe.

## 44. Prior Import Detection

Chronicle checks prior successful imports of the same artifact fingerprint.

## 45. Duplicate Import Behavior

The UI warns the user.

Another clone requires explicit confirmation and a new OperationId.

## 46. Import Inspection

Inspection performs no active Campaign mutation.

## 47. Inspection Stages

```text
Open
Read public header
Validate bounds
Resolve recovery method
Authenticate artifact
Read encrypted manifest
Check contract compatibility
Check package requirements
Plan staging
```

## 48. Resource Limits

Before expensive work, validate:

- file size;
- header size;
- KDF parameter bounds;
- encrypted chunk count;
- declared decompressed size;
- section count;
- maximum object counts.

## 49. KDF Parameter Safety

Reject maliciously excessive KDF parameters that could cause resource exhaustion.

## 50. Hostile Archive Controls

Reject:

- path traversal;
- absolute paths;
- links;
- duplicate canonical paths;
- case collisions;
- malformed entry names;
- executable payloads outside an approved future contract;
- decompression bombs;
- deep nesting;
- oversized strings and collections.

## 51. Decryption

Decrypt through bounded streaming.

## 52. No Plaintext Files

Decrypted logical records are:

- processed in bounded memory;
- or written directly into the encrypted import staging database.

## 53. Import Staging Store

Use a separate encrypted SQLite staging database.

## 54. Staging Key

Generate a temporary high-entropy staging key.

## 55. Staging Key Lifetime

The staging key is held through the import operation and protected if restart recovery requires persistence.

## 56. Staging Isolation

The staging database:

- has a distinct DataRootId;
- is not registered as an active Chronicle database;
- contains no provider credentials;
- cannot be opened by ordinary Campaign services;
- is deleted after successful publication and recovery window.

## 57. Staging Schema

The staging schema represents portable contracts and validation status, not normal EF entity persistence directly.

## 58. Why Separate Staging Schema

It allows:

- hostile-data isolation;
- full graph validation;
- identity mapping;
- version conversion;
- restart recovery;
- no partial authoritative state.

## 59. Contract Upgrade

Supported older portable contract versions may be normalized inside staging.

## 60. No Source Artifact Mutation

The original artifact is never rewritten.

## 61. Unsupported Contract

Unsupported newer contracts block import safely.

## 62. Validation Layers

Import validation proceeds through:

```text
Cryptographic Validation
Framing Validation
Contract Validation
Limit Validation
Reference Validation
Identity Validation
Package Compatibility Validation
Dice Evidence Validation
Domain Semantic Validation
Publication Readiness Validation
```

## 63. Reference Validation

Every reference must resolve within the artifact or an explicitly permitted package descriptor.

## 64. Ownership Validation

All Campaign-owned records must trace to one exported Campaign root.

## 65. Hierarchy Validation

Validate:

```text
Campaign
    → Session
        → Act
            → Scene
```

with legal ordering and no cycles.

## 66. Character Validation

Validate Character fields against the declared package contracts when available.

## 67. Knowledge Validation

Preserve separation between Campaign truth and Character Knowledge.

## 68. Memory Validation

Validate memory lifecycle and scope.

## 69. Dice Validation

Validate:

- exact package binding;
- canonical state;
- generation stages;
- evidence sequence;
- parent-evidence graph;
- no prohibited cycles;
- resolution-stage order;
- input evidence hashes where present;
- correction links;
- continuation references.

## 70. No Historical Recalculation

Import validates stored historical results.

It does not recalculate them using the target package.

## 71. Package Compatibility Resolver

The resolver returns:

```text
ExactPackageAvailable
CompatibleReadOnlySupport
PackageMissing
PackageHashMismatch
PackageUntrusted
PackageUnsupported
```

## 72. Exact Package

Full supported behavior may be enabled after publication.

## 73. Compatible Read-Only Support

Historical interpretation is available, but new mechanical mutation may remain blocked.

## 74. Package Missing

Data may publish in restricted mode if all portable records can be preserved.

## 75. Package Hash Mismatch

Do not treat a same-ID, same-version, different-hash package as exact.

## 76. Untrusted Package

Import does not install or execute it automatically.

## 77. Identity Mapping

Create a typed identity map for every imported authoritative entity family.

## 78. Identity Map Record

Recommended:

```text
ImportOperationId
EntityTypeKey
SourcePortableId
TargetLocalId
MappingStateKey
CreatedAtUtc
```

## 79. Mapping Uniqueness

Enforce:

```text
ImportOperationId + EntityTypeKey + SourcePortableId unique
```

## 80. Target ID Allocation

Allocate new strongly typed local IDs.

## 81. Stable Retry

A retried import reuses the persisted identity map.

## 82. No Blind Source ID Reuse

Source IDs remain provenance only.

## 83. Lineage

The new Campaign stores:

- SourceArtifactId;
- SourceArtifactFingerprint;
- SourceCampaignId;
- SourceCampaignLineageId;
- ImportedAtUtc;
- ImportOperationId.

## 84. Naming

Display-name collision is resolved separately from identity.

## 85. CloneOnImport

The MVP publishes a new Campaign.

## 86. No Merge

No record is merged with an existing Campaign.

## 87. No Overwrite

No existing Campaign is replaced.

## 88. Local Projection

After mapping, staging projects portable records into local persistence commands or validated local persistence models.

## 89. Domain Validation

Before publication, Chronicle validates the complete target aggregate graph.

## 90. Publication Plan

The publication plan includes:

- target IDs;
- hierarchy;
- package bindings;
- Characters;
- Knowledge;
- Memories;
- Messages;
- Dice;
- progression;
- corrections;
- lineage;
- restricted-mode flags.

## 91. Publication Transaction

Publish the new Campaign atomically in one bounded transaction where feasible.

## 92. Large Campaign Strategy

If one transaction becomes impractical, use an invisible staged local aggregate plus one final activation transaction.

## 93. Visibility Rule

A partially imported Campaign must never appear in the normal Campaign list.

## 94. Activation Flag

A local state such as:

```text
Importing
Active
Restricted
ImportFailed
```

may protect visibility.

This is local publication state, not Campaign fictional state.

## 95. Final Activation

Only after all records and invariants validate does the Campaign become visible.

## 96. Operation Result

The import Operation records the new CampaignId and publication result atomically with activation.

## 97. Unknown Commit Recovery

After uncertain publication, query:

- Import Operation;
- artifact fingerprint;
- identity map;
- lineage record;
- target Campaign activation state.

## 98. No Duplicate Campaign on Retry

Retry resumes or returns the existing imported Campaign.

## 99. Post-Publication Validation

Validate:

- Campaign ownership graph;
- hierarchy ordering;
- package bindings;
- Character references;
- Knowledge separation;
- Message order;
- Dice graph;
- corrections;
- progression;
- lineage;
- encrypted database persistence.

## 100. Validation Failure

If post-publication validation fails:

- keep Campaign hidden;
- roll back transaction where possible;
- otherwise mark import recovery state;
- preserve existing Campaigns unchanged;
- enter `RecoveryRequired` if safe automated cleanup cannot be proven.

## 101. Staging Cleanup

Delete staging only after:

- successful publication;
- post-publication validation;
- recovery window completion;
- no unresolved Operation reference.

## 102. Cleanup Failure

Cleanup failure does not invalidate the imported Campaign.

It creates maintenance work.

## 103. Restricted Mode

A Campaign may be published as restricted when:

- exact Rule Set package is unavailable;
- package hash is unavailable but historical records remain self-contained;
- new mechanical mutation cannot be validated.

## 104. Restricted Mode Capabilities

At minimum:

- read Campaign history;
- inspect Characters;
- inspect Memories and Knowledge;
- inspect historical Dice evidence and results;
- export again;
- recover package compatibility.

## 105. Restricted Mode Prohibitions

Block:

- new Dice resolution;
- package-owned progression;
- package-dependent Character mutation;
- Narrative operations requiring unavailable mechanics.

## 106. Provider Independence

The source provider profile is not needed.

## 107. New Narrative Work

The target installation uses its own configured provider.

## 108. Narrative Provenance

Minimal safe accepted-output provenance may remain.

## 109. Provider Attempt Exclusion

Operational ProviderAttempt rows are not imported.

## 110. Raw Response Exclusion

Raw prompts and responses are not imported.

## 111. Language Handling

Technical keys remain unchanged.

## 112. Authored Text

Messages, Memories, Character history, and notes preserve original text.

## 113. Narrative Language Preference

Campaign narrative-language preference is imported.

## 114. UI Locale

Installation UI locale is not imported.

## 115. Encryption Boundary

The portable artifact encryption is independent of Chronicle database encryption.

## 116. Import into Encrypted Database

The final local Campaign is persisted through the target installation's encrypted connection factory.

## 117. No Key Transfer

Import never transfers the source installation database key.

## 118. Export Recovery Secret Handling

Recovery secrets exist only in the secure interaction boundary and bounded cryptographic operation.

## 119. Clipboard

Chronicle should avoid automatically copying recovery secrets to the clipboard.

## 120. Sharing Guidance

UI advises sending artifact and recovery material through separate channels.

## 121. Logging

Safe logs may include:

- OperationId;
- ArtifactId;
- fingerprint;
- contract versions;
- section counts;
- mapping counts;
- package compatibility status;
- target CampaignId;
- duration;
- safe error code.

They must not include:

- recovery material;
- private Campaign prose;
- Character details;
- secret keys;
- provider credentials;
- raw provider content.

## 122. Metrics

Useful local metrics include:

```text
ExportProjectionDuration
ExportEncryptionDuration
ExportValidationDuration
ImportAuthenticationDuration
ImportStagingDuration
ImportIdentityMappingDuration
ImportPublicationDuration
ImportRestrictedCount
ImportRecoveryCount
```

No remote telemetry is required.

## 123. Error Model

Recommended errors:

```text
portability.export-campaign-not-ready
portability.export-projection-failed
portability.export-secret-detected
portability.export-encryption-failed
portability.export-validation-failed
portability.export-publication-failed
portability.import-artifact-invalid
portability.import-artifact-unsafe
portability.import-recovery-material-invalid
portability.import-authentication-failed
portability.import-contract-unsupported
portability.import-staging-failed
portability.import-reference-invalid
portability.import-identity-conflict
portability.import-package-missing
portability.import-package-hash-mismatch
portability.import-package-untrusted
portability.import-dice-evidence-invalid
portability.import-publication-failed
portability.import-post-validation-failed
portability.import-recovery-required
```

## 124. Data Preservation State

Results should state:

```text
AuthoritativeDataUnchanged
ExportBlocked
PortableProjectionCreated
EncryptedArtifactCreated
EncryptedArtifactValidated
ArtifactPublished
ImportArtifactAuthenticated
EncryptedStagingCreated
IdentityMapPersisted
NewCampaignStaged
NewCampaignActivated
ExistingCampaignsUnchanged
ImportedCampaignRestricted
NoPartialCampaignVisible
RecoveryRequired
```

## 125. Testing Strategy

The implementation requires:

```text
Projection Tests
Contract Tests
Encryption Tests
Secret Exclusion Tests
Cross-Campaign Isolation Tests
Hostile Artifact Tests
Staging Tests
Identity Mapping Tests
Package Compatibility Tests
Publication Tests
Retry and Recovery Tests
Dice Round-Trip Tests
Knowledge and Memory Tests
Localization Tests
Performance Tests
Architecture Tests
```

## 126. Projection Tests

Verify complete selected-Campaign projection and no unrelated data.

## 127. Contract Tests

Verify deterministic versioned serialization and supported contract upgrades.

## 128. Encryption Tests

Test valid, wrong, missing, and modified recovery material/artifact cases.

## 129. No-Plaintext Tests

Scan export and import staging directories for known content canaries.

## 130. Secret Tests

Synthetic database keys, backup keys, provider credentials, and recovery secrets must never appear in artifacts or logs.

## 131. Hostile Artifact Tests

Test:

- malformed headers;
- excessive KDF parameters;
- path traversal;
- entry collisions;
- oversized collections;
- deep nesting;
- decompression bombs;
- cyclic references;
- invalid Unicode;
- unexpected executable content.

## 132. Staging Tests

Verify staging database encryption, isolation, restart recovery, and cleanup.

## 133. Identity Tests

Verify complete one-to-one mapping and stable retry reuse.

## 134. Publication Tests

Verify no partial Campaign is visible under every injected failure point.

## 135. Retry Tests

Crash and retry after uncertain commit must return the same target Campaign.

## 136. Package Tests

Test exact, missing, mismatched, untrusted, and read-only-compatible packages.

## 137. Dice Tests

Round-trip:

- mixed Dice kinds;
- several groups;
- rerolls;
- explosions;
- keep/drop;
- opposed groups;
- additional stages;
- mechanical decisions;
- corrections;
- exact historical result.

## 138. Knowledge and Memory Tests

Verify Campaign truth, Character Knowledge, and Memories remain separate and intact.

## 139. Localization Tests

Import across different UI locales without changing technical keys or authored content.

## 140. Performance Tests

Use representative large Campaigns with bounded memory expectations.

## 141. Architecture Tests

Architecture tests must reject:

- EF entity serialization as portable contract;
- raw SQLite file as export;
- unencrypted artifact publication;
- database key reuse;
- plaintext staging;
- ProviderAttempt import;
- source ID used directly as local identity;
- merge or overwrite code path enabled in MVP;
- package execution before trust resolution;
- historical Dice recalculation;
- partial imported Campaign visibility;
- Presentation performing identity reconciliation.

## 142. Prohibited Patterns

### 142.1 Export DbContext Rows

Project to public contracts.

### 142.2 Decrypt to a Plaintext Directory

Stream into protected staging.

### 142.3 Source IDs Become Local IDs

Map them explicitly.

### 142.4 Publish Record by Record Visibly

Activate only after complete validation.

### 142.5 Retry Creates Another Clone Accidentally

Use OperationId and artifact fingerprint.

### 142.6 Missing Package Triggers Upgrade

Use restricted mode.

### 142.7 Import Provider Runtime State

Import accepted history, not provider execution.

### 142.8 Recalculate Dice

Preserve historical evidence and result.

### 142.9 Merge by Name

Names are not identity.

### 142.10 Reuse Database or Backup Keys

Use a separate portable-artifact key domain.

## 143. Alternatives Considered

### Import Directly into Active Tables

Rejected because hostile or invalid data could partially mutate authority.

### In-Memory-Only Import

Rejected for large Campaigns and restart recovery.

### Plain Temporary JSON Files

Rejected because they would expose private Campaign data.

### Reuse Source Primary Keys

Rejected because collision and cross-installation authority assumptions are unsafe.

### Merge into Existing Campaign

Deferred because semantic conflict resolution is not MVP scope.

### Include Exact Provider Execution State

Rejected because provider execution is nonportable and nonauthoritative.

## 144. Consequences

### Positive

- clean separation between public contracts and persistence;
- encrypted portable artifacts;
- safe hostile-input boundary;
- restart-safe import;
- no identity collisions;
- no partial visible Campaign;
- exact historical mechanics preserved;
- package absence handled honestly;
- future platforms can reuse the implementation model.

### Negative

- encrypted staging database adds complexity;
- identity mapping must cover every entity family;
- publication and recovery tests are extensive;
- restricted mode requires UI support;
- export readiness may block during unresolved critical work.

## 145. Risks

### Mapping Omits a Reference

Mitigation:

- typed entity registry;
- graph validation;
- completeness tests.

### Artifact Authentication Passes but Semantics Are Malicious

Mitigation:

- authentication establishes integrity, not trust;
- strict semantic validation;
- package trust separation.

### Import Publication Is Too Large for One Transaction

Mitigation:

- invisible local staging aggregate;
- final activation transaction;
- bounded batching with no normal visibility.

### Staging Key Is Lost During Restart

Mitigation:

- protect staging key through the approved local secret mechanism when durable recovery is enabled;
- otherwise classify staging as nonresumable before execution.

## 146. Technology Spike

Before acceptance, implement:

1. export readiness inspector;
2. portable DTO registry;
3. deterministic serializer;
4. streaming authenticated encryption;
5. artifact reopen validator;
6. hostile artifact inspector;
7. encrypted staging database;
8. contract normalization;
9. typed identity map;
10. package compatibility resolver;
11. CloneOnImport projector;
12. invisible publication state;
13. atomic activation;
14. import recovery;
15. restricted mode;
16. complex Dice round-trip;
17. no-plaintext canary suite;
18. architecture tests.

## 147. Spike Acceptance

The spike passes when:

- one selected Campaign exports without unrelated data;
- the artifact is encrypted and validated;
- a clean installation authenticates and stages it;
- every imported entity receives a new local ID;
- retry reuses the same mapping;
- no partial Campaign appears;
- publication activates exactly one new Campaign;
- existing Campaigns remain unchanged;
- missing package produces restricted mode;
- complex Dice history survives exactly;
- no provider credentials, database keys, or plaintext staging appear.

## 148. Definition of Compliance

An implementation complies when:

- export uses explicit public contracts;
- every portable artifact is encrypted;
- portable keys are separate from database and backup keys;
- import uses encrypted isolated staging;
- hostile content is fully validated before publication;
- default mode is CloneOnImport;
- all authoritative IDs are remapped;
- lineage is preserved as provenance;
- publication is atomic or invisibly staged with atomic activation;
- retries do not create duplicate Campaigns;
- historical Dice and package bindings remain exact;
- provider runtime state and installation secrets are excluded;
- missing packages never trigger silent reinterpretation;
- no Core portability contract is specific to Werewolf.

## 149. Review Triggers

Review this ADR if:

- merge import is introduced;
- overwrite import is introduced;
- cloud sharing is added;
- multi-recipient public-key encryption is added;
- embedded executable packages are allowed;
- collaborative lineage becomes authoritative;
- partial Campaign import is added;
- server identity replaces local Campaign identity.

## 150. Deferred Decisions

Later decisions may define:

- merge engine;
- overwrite workflow;
- selective export;
- multi-recipient encryption;
- artifact signatures;
- embedded signed packages;
- cloud transfer;
- partial import;
- collaborative conflict resolution;
- external portability SDK.

## 151. Final Decision

Chronicle will export a Campaign as an encrypted public contract.

It will import that contract through encrypted staging.

It will validate the complete graph.

It will create new local identities.

It will publish one complete new Campaign or none.

It will never mistake portability for a database copy.
