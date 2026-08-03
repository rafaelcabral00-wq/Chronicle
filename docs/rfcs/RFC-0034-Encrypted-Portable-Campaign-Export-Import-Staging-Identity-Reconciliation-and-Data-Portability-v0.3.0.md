---
id: RFC-0034
title: Encrypted Portable Campaign Export, Import Staging, Identity Reconciliation, and Data Portability
status: Draft
version: 0.3.0
owner: Chronicle Team
last_updated: 2026-08-02
category: Data Portability
supersedes:
  - RFC-0034@0.2.0
  - RFC-0034@0.1.0
superseded_by: null
depends_on:
  - RFC-0012
  - RFC-0017
  - RFC-0019
  - RFC-0021
  - RFC-0025
  - RFC-0033
  - ADR-0004
  - ADR-0024
  - ADR-0026
  - ADR-0033
  - ADR-0034
  - ADR-0036
  - ADR-0041
implements: []
related_to:
  - ADR-0027
  - ADR-0028
  - ADR-0042
  - ADR-0044
---

> **"Portability must move the Campaign without exposing the installation that protected it."**

# Encrypted Portable Campaign Export, Import Staging, Identity Reconciliation, and Data Portability

## 1. Status

**Draft**

This RFC defines Chronicle's portable Campaign export and import contract.

This revision aligns portability with:

- encrypted local persistence;
- encrypted installation backups;
- generic Dice evidence;
- Narrative Turns;
- Rule Set package bindings;
- secure import staging;
- identity reconciliation;
- no-rework MVP foundations.

The decision is:

- portable Campaign export is distinct from installation backup;
- export uses explicit versioned public contracts, never raw EF entities or database tables;
- portable exports are encrypted from the MVP onward;
- portable export encryption uses a key domain separate from:
  - installation database keys;
  - backup keys;
  - provider credentials;
- exports must be importable on a clean installation;
- export never contains plaintext installation secrets;
- import treats the artifact as untrusted input;
- import validates in isolated encrypted staging;
- import never mutates the active Campaign while parsing or validating;
- import never merges blindly by database primary key;
- import uses explicit identity reconciliation;
- the default import mode creates a new local Campaign identity;
- overwrite or merge behavior is outside MVP unless separately approved;
- exact Rule Set package IDs, versions, operation contracts, Character schemas, Dice evidence, and historical results are preserved;
- unresolved provider attempts and raw provider staging are excluded by default;
- unresolved authoritative Campaign workflows are either rejected for export or normalized to a documented stable boundary;
- no portable contract assumes Werewolf, d10, one Dice pool, or one provider;
- imported content may be opened in restricted mode when required packages are missing;
- export and import preserve original authored languages;
- all technical keys remain English.

## 2. Purpose

Portable Campaign export enables a user to:

- move a Campaign to another Chronicle installation;
- archive one Campaign independently from installation state;
- share a Campaign package intentionally;
- preserve long-lived history outside one database;
- import into a clean installation;
- keep exact mechanical evidence and package bindings.

It must not expose:

- database keys;
- provider credentials;
- Credential Manager references;
- internal SQLite structure;
- raw provider threads;
- transient response staging;
- unrelated Campaigns;
- machine-local configuration.

## 3. Backup Versus Portable Export

```text
Installation Backup
    full local recovery
    internal persistence format allowed
    unresolved operational state preserved
    installation-level settings may be included
    encrypted through backup recovery contract

Portable Campaign Export
    one Campaign
    public versioned contract
    clean-install import
    installation secrets excluded
    internal database layout hidden
```

## 4. MVP Scope

MVP portability includes:

- manual export of one Campaign;
- encrypted artifact creation;
- export validation;
- manual import;
- hostile artifact validation;
- isolated staging;
- new Campaign identity creation;
- package compatibility inspection;
- restricted-mode import when safe;
- exact Dice evidence preservation;
- original content-language preservation.

## 5. Out of Scope

Not required in MVP:

- live sync;
- cloud transfer;
- multiplayer merge;
- partial Scene export;
- selective Character export;
- merge into an existing Campaign;
- overwrite of an existing Campaign;
- collaborative conflict resolution;
- cross-product conversion;
- automatic package download;
- unencrypted portable export.

## 6. Artifact Identity

The portable artifact is:

```text
ChronicleCampaignPackage
```

## 7. Artifact Extension

The exact file extension is implementation detail.

A dedicated extension is recommended.

## 8. Artifact Structure

Recommended logical structure:

```text
ChronicleCampaignPackage
├── public-header
├── encrypted-payload
│   ├── manifest.json
│   ├── campaign.json
│   ├── characters.json
│   ├── knowledge.json
│   ├── memories.json
│   ├── narrative.json
│   ├── dice.json
│   ├── progression.json
│   ├── corrections.json
│   ├── package-bindings.json
│   ├── resources/
│   └── integrity.json
└── authentication-tag
```

Exact framing belongs to ADR-0027.

## 9. Public Header

The public header contains only minimum unlock and compatibility metadata.

## 10. Public Header Fields

Recommended:

```text
ArtifactTypeKey
ArtifactContractVersion
EncryptionSchemeKey
KdfSchemeKey
KdfParameters
Salt
PackageId
CreatedAtUtc
MinimumChronicleVersion
EncryptedPayloadLength
```

## 11. Public Header Privacy

The public header must not expose:

- Campaign name;
- Character names;
- user name;
- provider profile;
- local paths;
- database key;
- backup key;
- recovery secret;
- private package content.

## 12. Export Encryption

Every published portable Campaign export is encrypted.

## 13. Export Key Domain

Portable export uses its own artifact-specific data key.

## 14. No Key Reuse

Do not reuse:

- installation database key;
- installation backup data key;
- provider credentials;
- code-signing keys.

## 15. Clean-Installation Import

A recipient or future installation must be able to import the artifact using the export recovery method.

## 16. Recovery Modes

Approved candidates:

```text
User Export Secret
Export Recovery Key File
Future Multiple Recipient Key Slots
```

The MVP must implement at least one clean-install-compatible method.

## 17. Authenticated Encryption

The artifact must protect:

- confidentiality;
- integrity;
- framing;
- entry ordering where relevant;
- version metadata binding.

## 18. No Plaintext Durable Staging

Export must not create a durable plaintext package before encryption.

## 19. Streaming

Large Campaign packages should be serialized and encrypted through a bounded-memory streaming pipeline.

## 20. Canonical Export Model

Export uses explicit portable DTOs.

## 21. No EF Entity Serialization

The following are prohibited as portable contracts:

- EF Core entities;
- DbContext change tracker state;
- database row dumps;
- raw SQL schema;
- provider-native response objects;
- internal filesystem models.

## 22. Contract Versioning

Every artifact and major record family has a contract version.

## 23. Contract Registry

Portable contract types are registered through stable English keys.

## 24. Campaign Identity

The export preserves:

```text
SourceCampaignId
SourceCampaignLineageId
SourceInstallationOpaqueId
```

as provenance only.

## 25. Local Identity on Import

Default import creates:

```text
NewCampaignId
```

## 26. Clone-on-Import

The default MVP behavior is:

```text
CloneOnImport
```

## 27. Why Clone-on-Import

This avoids:

- primary-key collisions;
- accidental overwrite;
- hidden merge;
- cross-installation identity assumptions;
- operation correlation conflicts.

## 28. Lineage

The imported Campaign records lineage to the source artifact.

## 29. Lineage Fields

Recommended:

```text
SourceArtifactId
SourceCampaignId
SourceCampaignLineageId
ImportedAtUtc
ImportOperationId
```

## 30. No Authority by Source ID

Source IDs do not become local database primary keys automatically.

## 31. Internal Reference Remapping

Import remaps internal references consistently.

## 32. Remapped Identity Families

At minimum:

- Campaign;
- Session;
- Act;
- Scene;
- Character;
- Message;
- Memory;
- Character Knowledge;
- Dice Roll;
- Dice evidence;
- correction;
- progression entry;
- package binding;
- stable narrative provenance where exported.

## 33. Identity Map

Import builds an explicit:

```text
ImportIdentityMap
```

## 34. Identity Map Persistence

The final imported Campaign may retain only lineage mappings required for provenance.

Temporary mapping detail is removed after successful publication unless needed for audit.

## 35. Campaign Hierarchy

Export preserves:

```text
Campaign
    → Sessions
        → Acts
            → Scenes
```

including stable ordering.

## 36. Messages

Export preserves authoritative Messages.

## 37. Narrative Turn Data

Portable export may include a normalized subset of Narrative Turn provenance.

## 38. Narrative Turn Inclusion

Recommended portable fields:

```text
PortableNarrativeTurnId
TriggerTypeKey
RoleKey
CompletionStatusKey
AcceptedAtUtc
OriginDiceRollId
AcceptedOutputHash
```

## 39. Narrative Turn Exclusion

Exclude:

- active provider retry details;
- raw prompts;
- raw responses;
- provider thread IDs unless explicitly safe and useful;
- credentials;
- transient staging paths.

## 40. Provider Attempts

Provider Attempts are excluded by default.

## 41. Accepted Provider Provenance

A minimal optional provenance record may include:

```text
ProviderAdapterKey
ModelProfileKey
AcceptedAtUtc
ContractVersion
CanonicalOutputHash
```

It remains nonauthoritative.

## 42. Structured Events

Export preserves authoritative effects and, where useful, normalized event provenance.

## 43. Event Application Exclusion

Internal retry and scheduling metadata need not be exported once the authoritative result is complete.

## 44. Unresolved Workflow Boundary

The MVP export requires a stable Campaign boundary.

## 45. Stable Export Boundary

Export is allowed only when there is no unresolved critical Campaign workflow that could make the package semantically incomplete.

## 46. Blocking Conditions

Examples:

- Dice evidence generation in progress;
- Dice in `RecoveryRequired`;
- Narrative acceptance commit uncertain;
- migration active;
- restore active;
- correction operation unresolved;
- package migration unresolved.

## 47. Nonblocking Conditions

Examples may include:

- provider configuration missing when no authoritative turn is in progress;
- completed rejected Provider Attempts;
- maintenance cleanup Work Items.

## 48. Export Readiness Inspection

Chronicle produces:

```text
Ready
ReadyWithWarnings
Blocked
```

## 49. Blocked Export

A blocked export does not create a published artifact.

## 50. Optional Future Pending Workflow Portability

Portable unresolved workflows are deferred.

They require their own public contracts.

## 51. Character Data

Export preserves:

- Character identity;
- package-defined fields;
- personality;
- relevant personal history;
- relationships;
- current mechanical state;
- provenance;
- visibility where applicable.

## 52. Character Field Contracts

Package-defined Character fields include:

```text
FieldKey
FieldContractVersion
ValueTypeKey
Value
ProvenanceKey
```

## 53. Character Knowledge

Campaign truth and Character Knowledge remain distinct in export.

## 54. Knowledge Records

Preserve:

- knower Character;
- subject;
- claim;
- acquisition provenance;
- confidence or status where supported;
- lifecycle;
- timestamps.

## 55. Campaign Memories

Memories preserve:

- content;
- scope;
- permanence;
- lifecycle;
- creation context;
- relevance metadata.

## 56. No Transcript Substitution

Export does not reconstruct memories from Messages.

It exports authoritative Memory records.

## 57. Dice Export

Portable Dice records preserve:

- DiceRoll identity;
- request contract;
- exact package binding;
- lifecycle terminal state;
- raw evidence;
- generation stages;
- causal links;
- resolution stages;
- evidence use;
- final result;
- corrections;
- continuation relationships where relevant.

## 58. Dice Evidence

Every evidence item preserves:

```text
PortableEvidenceItemId
GenerationStageKey
GroupKey
DieKey
DieOrdinal
DiceKindKey
FaceCount
RawFaceValue
RawSymbolValue
GenerationSequence
ParentEvidenceItemId
CauseKey
RandomSourceContractKey
CreatedAtUtc
```

## 59. No Flattened Dice Export

Do not export only:

- total successes;
- final integer;
- final prose;
- Werewolf-specific pool summary.

## 60. Keep/Drop Preservation

All original evidence remains.

Evidence-use records indicate selection.

## 61. Reroll and Explosion Preservation

Causal parent links survive import.

## 62. Corrections

Invalidated and replacement Rolls preserve their full history and relationships.

## 63. Historical Resolution

Import preserves the historical resolved result.

## 64. No Silent Recalculation

Import must not recompute old Rolls using the locally installed package version.

## 65. Exact Package Binding

Export includes:

```text
RuleSetPackageId
RuleSetPackageVersion
PackageContractVersion
ContentHash
Provenance
RequiredCapabilities
```

## 66. Package Binary Inclusion

Portable Campaign export does not include executable Rule Set package binaries by default.

## 67. Why Exclude Executable Packages

This reduces:

- malware risk;
- licensing risk;
- artifact size;
- implicit code installation.

## 68. Future Embedded Package Option

Embedding signed packages may be added later through a separately approved security policy.

## 69. Missing Package on Import

If the exact package is unavailable:

- import may proceed in restricted mode when data can be preserved safely;
- mechanical mutation remains blocked;
- historical data remains readable;
- package recovery is offered.

## 70. No Automatic Package Upgrade

Import must not silently bind the Campaign to a newer package version.

## 71. Package Compatibility

Import distinguishes:

```text
ExactPackageAvailable
CompatibleReaderAvailable
PackageMissing
PackageHashMismatch
PackageUntrusted
PackageUnsupported
```

## 72. Progression

Export preserves progression ledger entries and exact package-defined operation keys.

## 73. Resources

Package-owned resources and Character resource states are preserved through versioned contracts.

## 74. User Preferences

Only Campaign-scoped preferences are exported.

## 75. Installation Preferences

Exclude:

- window size;
- theme;
- provider credentials;
- local backup paths;
- data-root paths;
- release channel;
- local logging preferences.

## 76. UI Locale

Installation UI locale is excluded.

## 77. Narrative Language

Campaign narrative-language preference is included.

## 78. Authored Language

Existing content remains unchanged.

## 79. No Automatic Translation

Import does not translate content to the target installation locale.

## 80. Attachments and Resources

If Chronicle later supports Campaign-owned attachments, they must use:

- explicit manifest entries;
- size limits;
- content hashes;
- MIME allowlists;
- path safety;
- encrypted payload protection.

## 81. Export Operation

Every export uses an OperationId.

## 82. Export Work Items

Potential Work Items:

```text
campaign-export.inspect
campaign-export.project
campaign-export.encrypt
campaign-export.validate
campaign-export.publish
```

## 83. Export Lifecycle

Recommended internal lifecycle:

```text
Requested
Inspecting
Projecting
Serializing
Encrypting
Validating
ReadyToPublish
Publishing
Published
Completed
FailedRetryable
FailedTerminal
RecoveryRequired
Cancelled
```

## 84. Export Snapshot

Export reads from a coherent database snapshot.

## 85. Snapshot Consistency

The snapshot must preserve one stable Campaign state.

## 86. No Long Write Lock

Serialization and encryption occur outside long write transactions.

## 87. Export Projection

Projection reads only the selected Campaign and required referenced records.

## 88. No Cross-Campaign Leakage

Tests must prove unrelated Campaign data is absent.

## 89. Export Staging

The artifact is written under an opaque encrypted staging name.

## 90. Export Validation

Before publication:

1. reopen encrypted artifact;
2. unlock using configured export recovery material;
3. validate authentication;
4. validate manifest;
5. validate contract versions;
6. validate identity graph;
7. validate package bindings;
8. validate Dice evidence graph;
9. scan for prohibited secrets;
10. mark ready to publish.

## 91. Export Secret Scan

The artifact must exclude:

- database key;
- backup key;
- export recovery secret;
- provider credentials;
- Credential Manager references;
- private local paths;
- raw provider staging.

## 92. Import Operation

Every import uses an OperationId.

## 93. Import Lifecycle

Recommended:

```text
Requested
Inspecting
AwaitingRecoveryMaterial
DecryptingToProtectedStaging
ValidatingArtifact
ResolvingCompatibility
BuildingIdentityMap
ProjectingLocalRecords
ValidatingStagedCampaign
ReadyToPublish
Publishing
ValidatingPublishedCampaign
Completed
RolledBack
FailedRetryable
FailedTerminal
RecoveryRequired
Cancelled
```

## 94. Import Inspection

Inspection mutates no active Campaign state.

## 95. Artifact Trust

The package is untrusted until:

- cryptographic authentication passes;
- framing is valid;
- contracts are valid;
- limits are satisfied;
- identity graph is coherent;
- package metadata is acceptable.

## 96. Hostile Artifact Safety

Reject:

- path traversal;
- absolute paths;
- symbolic links;
- junctions;
- duplicate canonical entries;
- case collisions;
- oversized values;
- excessive nesting;
- decompression bombs;
- malformed Unicode;
- unsupported algorithms;
- executable payloads outside future approved contracts.

## 97. Decryption Staging

Decrypted logical content must not be persisted as plaintext files.

## 98. Protected Staging Options

Use:

- in-memory bounded processing;
- encrypted staging database;
- encrypted temporary container;
- another reviewed protected mechanism.

## 99. Import Staging Database

A temporary encrypted SQLite staging database is recommended for complex validation.

## 100. Staging Isolation

The staging database has:

- separate key;
- separate DataRootId;
- no normal Application registration;
- no provider credentials;
- no active Campaign mutation authority.

## 101. Contract Validation

Validate every record family against its declared version.

## 102. Referential Validation

Validate:

- hierarchy references;
- Character ownership;
- Message ordering;
- Knowledge references;
- Memory references;
- Dice evidence parent graph;
- correction graph;
- package binding;
- progression references.

## 103. Cycle Validation

Reject prohibited cycles in:

- hierarchy;
- evidence parent relationships;
- correction replacement chains;
- identity mapping.

## 104. Import Identity Mapping

All source-local IDs are mapped to new local IDs.

## 105. Deterministic Mapping Within Operation

The same import Operation reuses its existing identity map on retry.

## 106. Retry Idempotency

Retrying publication does not create duplicate Campaigns.

## 107. Import Fingerprint

The artifact has a stable fingerprint.

## 108. Duplicate Artifact Detection

Chronicle may warn that the same artifact was imported previously.

## 109. Duplicate Import Policy

The MVP may allow another clone after explicit user confirmation.

It must never silently merge.

## 110. Name Collision

Campaign-name collision does not block import.

## 111. Display Name Resolution

Chronicle may suggest:

```text
Campaign Name (Imported)
```

The user may rename it.

## 112. Publication Boundary

Publishing the imported Campaign is one bounded authoritative transaction where feasible.

## 113. Publication Contents

The final transaction persists:

- new Campaign identity;
- hierarchy;
- Characters;
- Knowledge;
- Memories;
- Messages;
- Dice records;
- progression;
- corrections;
- package bindings;
- lineage;
- import Operation result.

## 114. No Partial Campaign Publication

A failed final publication must not leave a partially visible Campaign.

## 115. Post-Publication Validation

Validate:

- aggregate ownership;
- hierarchy;
- message order;
- package bindings;
- Dice evidence graph;
- correction graph;
- lineage;
- encryption-backed persistence.

## 116. Rollback

If publication validation fails, remove or invalidate the new Campaign through the import transaction or a safe rollback Operation.

The existing installation's prior Campaigns remain untouched.

## 117. Provider Configuration

Import does not require the source provider profile.

## 118. Provider Independence

Accepted history remains usable without the original provider.

## 119. Future Narrative Work

New provider work uses the target installation's configured provider profile.

## 120. Export and Backup Interaction

A portable export may be included inside an encrypted installation backup as an ordinary file, but the contracts remain separate.

## 121. No Database Backup as Export

A raw or encrypted SQLite database file is not a portable Campaign package.

## 122. Encryption Parameter Versioning

The artifact header identifies:

- encryption scheme;
- KDF scheme;
- KDF parameters;
- contract version.

## 123. Algorithm Migration

A newer Chronicle may read supported older artifact encryption and re-export using a newer scheme.

## 124. No Source Artifact Rewrite

Import does not rewrite the original file.

## 125. Artifact Rewrap

Future versions may rewrap the artifact data key under new recovery material without rebuilding the Campaign payload.

## 126. Portability Across Platforms

The artifact contract must not depend on Windows Credential Manager.

## 127. Cross-Platform Readiness

Even though the first app is Windows desktop, export recovery material and artifact framing must be platform-neutral.

## 128. Security Disclosure

The UI must explain:

- the export is encrypted;
- recovery material is required;
- losing recovery material may make the artifact unusable;
- the package may contain private Campaign content;
- importing untrusted artifacts may be dangerous before validation;
- Chronicle has no universal recovery key.

## 129. Sharing

The user must share:

- the encrypted artifact;
- recovery material through a separate safe channel.

Chronicle should not encourage sending both together.

## 130. No Automatic Upload

Chronicle never uploads exports automatically.

## 131. Logging

Safe logs may include:

- ExportOperationId;
- ImportOperationId;
- ArtifactId;
- artifact fingerprint;
- contract version;
- encryption scheme key;
- record counts;
- package IDs;
- duration;
- safe error codes.

They must not include:

- recovery secrets;
- Campaign prose;
- Character details;
- credentials;
- database keys;
- raw provider responses.

## 132. Metrics

Useful local metrics include:

```text
CampaignExportDuration
CampaignExportSize
CampaignImportDuration
ImportValidationFailureCount
ImportPackageMissingCount
ImportRestrictedModeCount
IdentityRemapCount
```

No remote telemetry is required.

## 133. Error Model

Recommended export errors:

```text
export.campaign-not-found
export.campaign-not-ready
export.unresolved-critical-work
export.package-binding-invalid
export.projection-failed
export.encryption-failed
export.recovery-material-invalid
export.validation-failed
export.publication-failed
export.secret-leak-detected
export.recovery-required
```

Recommended import errors:

```text
import.artifact-not-found
import.artifact-invalid
import.artifact-unsafe
import.recovery-material-required
import.recovery-material-invalid
import.authentication-failed
import.contract-unsupported
import.identity-graph-invalid
import.reference-invalid
import.package-missing
import.package-hash-mismatch
import.package-untrusted
import.dice-evidence-invalid
import.correction-graph-invalid
import.staging-failed
import.publication-failed
import.post-publication-validation-failed
import.recovery-required
```

## 134. Data Preservation State

Results should state:

```text
AuthoritativeDataUnchanged
ExportReadinessBlocked
PortableProjectionCreated
ArtifactEncrypted
ArtifactValidated
ArtifactPublished
ImportArtifactAuthenticated
ImportStaged
IdentityMapCreated
NewCampaignPublished
ExistingCampaignsUnchanged
ImportedCampaignRestricted
NoPartialPublication
RecoveryRequired
```

## 135. Testing Strategy

The implementation requires:

```text
Portable Contract Tests
Encryption Tests
Clean-Installation Import Tests
Projection Tests
Cross-Campaign Isolation Tests
Identity Reconciliation Tests
Package Compatibility Tests
Dice Round-Trip Tests
Knowledge and Memory Tests
Hostile Artifact Tests
No-Plaintext Tests
Secret Exclusion Tests
Retry Tests
Publication Tests
Rollback Tests
Localization Invariance Tests
Cross-System Tests
Architecture Tests
```

## 136. Portable Contract Tests

Verify deterministic serialization, versioning, and unknown-field policy.

## 137. Encryption Tests

Verify:

- wrong recovery material fails;
- modified artifact fails authentication;
- active database key is absent;
- artifact remains unreadable without recovery material.

## 138. Clean-Installation Tests

Import on an installation with:

- no source database;
- no source keys;
- no source provider profile;
- only recovery material and compatible package support.

## 139. Projection Tests

Verify all required authoritative Campaign data is included.

## 140. Isolation Tests

Use private canaries in another Campaign and verify they never appear in the export.

## 141. Identity Tests

Verify every source identity maps consistently to one new local identity.

## 142. Retry Tests

Crash and retry during staging and publication must not create duplicate Campaigns.

## 143. Dice Round-Trip Tests

Preserve:

- multiple groups;
- mixed Dice sizes;
- rerolls;
- explosions;
- keep/drop;
- opposed groups;
- additional stages;
- post-Roll decisions;
- corrections;
- exact package binding.

## 144. Knowledge Tests

Campaign truth and Character Knowledge remain distinct.

## 145. Memory Tests

Memory permanence and lifecycle survive round-trip.

## 146. Hostile Artifact Tests

Test:

- traversal;
- links;
- collisions;
- malformed framing;
- excessive KDF parameters;
- oversized payloads;
- deep nesting;
- cyclic evidence graph;
- duplicate identities;
- unexpected executable entries.

## 147. No-Plaintext Tests

Scan export and import staging for Campaign content canaries.

## 148. Secret Tests

Synthetic keys and credentials must not appear in artifact payload, public header, logs, or staging metadata.

## 149. Package Tests

Test:

- exact package;
- missing package;
- hash mismatch;
- untrusted package;
- restricted-mode import.

## 150. Localization Tests

Export under one UI locale and import under another without changing technical identity or authored content.

## 151. Cross-System Fixture

A synthetic non-Werewolf Campaign round-trips through encrypted export and clean-install import without Core contract changes.

## 152. Architecture Tests

Architecture tests must reject:

- EF entity serialization as export contract;
- raw database file as portable export;
- unencrypted portable export;
- database key reused for export;
- plaintext import staging;
- provider credentials exported;
- provider attempts required for historical interpretation;
- silent merge into existing Campaign;
- source IDs reused blindly as local IDs;
- historical Dice recomputation;
- Werewolf-specific portable schema;
- automatic package installation from artifact;
- UI locale changing technical keys.

## 153. Prohibited Patterns

### 153.1 Backup Equals Export

They solve different problems.

### 153.2 Raw Database as Portable Package

Use public contracts.

### 153.3 Unencrypted Export

Portable private content is encrypted from MVP.

### 153.4 Reuse Installation Key

Use an artifact-specific key domain.

### 153.5 Plaintext Staging

Use protected staging.

### 153.6 Blind Primary-Key Reuse

Use identity reconciliation.

### 153.7 Silent Merge

Default to CloneOnImport.

### 153.8 Recompute Historical Dice

Preserve evidence and resolution.

### 153.9 Bundle Executable Package Automatically

Keep package trust separate.

### 153.10 Export Provider Secrets

Exclude all installation secrets.

## 154. Alternatives Considered

### Raw SQLite Copy

Rejected because it exposes internal schema, installation state, and key dependencies.

### Unencrypted Portable JSON

Rejected because Campaign exports contain private long-lived content and would require later redesign.

### Reuse Backup Format

Rejected because backup preserves installation recovery while export preserves public Campaign semantics.

### Preserve Source IDs as Local IDs

Rejected because collisions and hidden authority assumptions would occur.

### Merge by Campaign Name

Rejected because display names are not identity.

### Recalculate Dice on Import

Rejected because historical chance and package interpretation must remain stable.

### Include Provider Threads

Rejected because they are nonportable and nonauthoritative.

## 155. Consequences

### Positive

- Campaigns are portable without exposing installation internals;
- export is confidential from MVP;
- clean-install import works;
- identity conflicts are controlled;
- Dice evidence and package history remain exact;
- provider replacement remains possible;
- UI and authored languages remain independent;
- future platforms can use the same contract.

### Negative

- portable DTOs require maintenance;
- encryption and recovery UX are required;
- import staging and identity mapping add complexity;
- unresolved-work export is constrained;
- missing package behavior requires restricted mode;
- artifact testing surface is broad.

## 156. Risks

### Export Contract Mirrors Database Accidentally

Mitigation:

- explicit DTO ownership;
- architecture tests;
- independent contract versions;
- clean-install import tests.

### User Loses Recovery Material

Mitigation:

- verification;
- clear education;
- recovery-key alternatives;
- readiness indicator.

### Identity Remapping Misses a Reference

Mitigation:

- typed identity map;
- graph validation;
- cross-record integration tests.

### Import Executes Malicious Package Content

Mitigation:

- do not install executable packages from the artifact automatically;
- validate package metadata;
- restricted mode.

### Historical Data Cannot Be Interpreted Without Package

Mitigation:

- preserve normalized historical results;
- exact binding;
- restricted read mode;
- package recovery.

## 157. Technology Spike

Before acceptance, implement:

1. portable contract registry;
2. encrypted artifact framing;
3. recovery secret or key-file mode;
4. Campaign projection;
5. stable export-boundary inspection;
6. streaming encryption;
7. artifact reopen validation;
8. hostile artifact parser;
9. encrypted import staging database;
10. identity map;
11. CloneOnImport publication;
12. package compatibility inspection;
13. restricted-mode import;
14. Dice evidence round-trip;
15. Knowledge and Memory round-trip;
16. no-plaintext canary suite;
17. secret exclusion tests;
18. cross-system fixture;
19. architecture tests.

## 158. Spike Acceptance

The spike passes when:

- one Campaign exports as an encrypted portable artifact;
- the artifact contains no unrelated Campaign data;
- a clean installation imports it using recovery material;
- all local identities are remapped consistently;
- existing Campaigns remain unchanged;
- exact package bindings survive;
- missing package produces restricted mode rather than reinterpretation;
- complex Dice evidence survives exactly;
- provider credentials and raw staging are absent;
- no durable plaintext Campaign package is created;
- the synthetic non-Werewolf fixture passes without Core changes.

## 159. Definition of Compliance

An implementation complies when:

- backup and export remain separate;
- portable exports are encrypted;
- export keys are separate from database and backup keys;
- clean-install import works;
- public contracts are independent from EF and SQLite;
- import uses protected isolated staging;
- source identities are reconciled explicitly;
- default import creates a new Campaign;
- historical Dice and exact package bindings are preserved;
- provider credentials, raw staging, and machine-local configuration are excluded;
- unresolved critical workflows block export;
- missing packages never cause silent reinterpretation;
- authored language and technical-key invariance are preserved;
- no contract assumes Werewolf or d10.

## 160. Review Triggers

Review this RFC if:

- merge import becomes required;
- multiplayer sync is introduced;
- partial Campaign export is introduced;
- package binaries may be embedded;
- multi-recipient encryption is added;
- cloud sharing is introduced;
- digital signatures for Campaign authorship are added;
- cross-product conversion is introduced;
- server-hosted Campaign identity becomes authoritative.

## 161. Deferred Decisions

Later decisions may define:

- merge import;
- overwrite import;
- selective export;
- multi-recipient key slots;
- public-key recipient encryption;
- author signatures;
- cloud transfer;
- embedded signed packages;
- collaborative lineage;
- incremental Campaign packages.

## 162. Final Decision

Chronicle will export Campaigns through encrypted, versioned, provider-neutral, Rule Set-neutral public contracts.

The export will move the Campaign.

It will not move the installation's secrets.

Import will stage, validate, remap identity, and publish a new local Campaign.

It will not merge blindly.

It will not reroll history.

It will not reinterpret the past through whatever package happens to be installed today.
