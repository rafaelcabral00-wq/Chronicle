---
id: ADR-0026
title: Encrypted Backup, Restore, Recovery Checkpoints, and Authoritative Data Preservation
status: Proposed
version: 0.3.0
owner: Chronicle Team
last_updated: 2026-08-02
category: Technology
supersedes:
  - ADR-0026@0.2.0
  - ADR-0026@0.1.0
superseded_by: null
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
  - RFC-0032
  - RFC-0033
  - RFC-0034
  - ADR-0001
  - ADR-0002
  - ADR-0004
  - ADR-0005
  - ADR-0007
  - ADR-0008
  - ADR-0010
  - ADR-0013
  - ADR-0014
  - ADR-0017
  - ADR-0019
  - ADR-0020
  - ADR-0024
  - ADR-0025
  - ADR-0033
  - ADR-0034
  - ADR-0036
  - ADR-0037
  - ADR-0038
  - ADR-0039
  - ADR-0041
  - ADR-0042
implements:
  - RFC-0026
  - RFC-0033
related_to:
  - RFC-0034
  - ADR-0027
  - ADR-0028
  - ADR-0044
---

> **"A backup must preserve authority without exposing it."**

# Encrypted Backup, Restore, Recovery Checkpoints, and Authoritative Data Preservation

## 1. Status

**Proposed**

This ADR defines Chronicle's encrypted backup, restore, recovery-checkpoint, validation, retention, and failure-handling architecture.

This revision corrects ADR-0026 version 0.2.0.

The prior statement that Chronicle backups would remain unencrypted in MVP is withdrawn.

The decision is:

- backup and restore are mandatory MVP capabilities;
- every published backup artifact is encrypted;
- backup encryption is independent from the active database encryption key;
- a backup must remain recoverable on a clean Chronicle installation;
- backup creation must not write a plaintext authoritative database or plaintext archive to durable storage;
- use SQLite-safe snapshot behavior against the encrypted database;
- stage, validate, package, encrypt, reopen, and verify before publication;
- preserve all authoritative Campaign data and unresolved recovery state;
- include committed Dice evidence and exact Rule Set bindings;
- include unresolved Operations, Work Items, Narrative Turns, and required Provider Attempts;
- exclude raw credentials;
- exclude unnecessary provider-response staging;
- include active staging only when required for safe recovery and protected inside the encrypted artifact;
- restore only through isolated encrypted staging;
- validate before activation;
- create a validated encrypted rollback checkpoint before replacing active data;
- never merge restore content into the active database;
- allow restore to create a new installation database key;
- keep backup keys, database keys, and provider credentials separate;
- never upload backups automatically;
- use English semantic keys and base recovery text;
- design the MVP backup foundation so later automation, cloud destinations, and richer key policies do not require replacing the format or workflow.

## 2. Context

Chronicle's encrypted database protects data at rest on the active installation.

That alone is not sufficient for backup.

A raw copy of the encrypted database tied only to the current Windows secret store may become unusable after:

- machine loss;
- Windows account loss;
- clean operating-system installation;
- new computer;
- damaged Credential Manager entry;
- replacement data root.

Conversely, a backup that contains the active database key in plaintext would defeat the security architecture.

Chronicle therefore needs a separate encrypted backup contract that preserves both:

- confidentiality;
- recoverability.

## 3. Foundational Principle

```text
The active database key protects one installation.

The backup recovery mechanism protects one backup artifact.

They are related by workflow, not by key reuse.
```

## 4. Backup Versus Export

Chronicle distinguishes:

```text
Encrypted Installation Backup
    restores one complete local Chronicle state

Portable Campaign Export
    transfers one Campaign through public versioned contracts
```

Backup remains an internal recovery artifact.

Export remains a portability contract.

## 5. MVP Requirements

The MVP must support:

- manual encrypted backup;
- backup validation;
- encrypted backup catalog metadata;
- restore on the same installation;
- restore on a clean installation;
- pre-migration encrypted checkpoint;
- pre-restore encrypted checkpoint;
- Safe Mode recovery;
- recovery-material setup and verification.

## 6. Deferred Features

Not required for MVP:

- scheduled automatic backup;
- cloud upload;
- incremental backup;
- deduplication;
- remote retention;
- multi-device synchronization;
- enterprise key escrow;
- hardware-token recovery.

These may extend the same foundation later.

## 7. Backup Artifact

The internal artifact is:

```text
ChronicleEncryptedBackup
```

## 8. Backup Identity

Every artifact has:

```text
BackupId
```

## 9. Logical Contents

Recommended protected contents:

```text
ChronicleEncryptedBackup
├── public-header
├── encrypted-payload
│   ├── manifest.json
│   ├── integrity.json
│   ├── database/
│   │   └── chronicle.db
│   ├── package-inventory/
│   ├── configuration/
│   ├── recovery/
│   └── provenance/
└── authentication-tag
```

Exact binary framing belongs to implementation.

## 10. Public Header

The public header contains only the minimum data required to identify and unlock the artifact safely.

## 11. Public Header Fields

Recommended:

```text
BackupContractVersion
EncryptionSchemeKey
KdfSchemeKey
KdfParameters
Salt
BackupId
CreatedAtUtc
MinimumChronicleVersion
EncryptedPayloadLength
```

## 12. Public Header Prohibitions

The public header must not contain:

- Campaign names;
- Character names;
- user name;
- machine name;
- database key;
- provider credentials;
- package secrets;
- raw local paths;
- plaintext private metadata.

## 13. Encrypted Manifest

Sensitive manifest fields remain inside the encrypted payload.

## 14. Backup Manifest

Recommended encrypted fields:

```text
BackupId
BackupTypeKey
ApplicationVersion
ReleaseChannel
DatabaseSchemaVersion
ConfigurationContractVersion
SourceDataRootId
DatabaseFileHash
PackageInventoryHash
ContainsUnresolvedWork
ContainsSensitiveTransientData
CreatedAtUtc
```

## 15. Backup Types

```text
Manual
PreMigrationCheckpoint
PreRestoreCheckpoint
RecoveryCheckpoint
```

## 16. Backup Encryption Key

Every backup is encrypted using a backup-specific data-encryption key.

## 17. No Active Database Key Reuse

The active database key must not be used directly as the backup encryption key.

## 18. Envelope Encryption

Recommended pattern:

```text
Random Backup Data Key
    encrypts backup payload

Recovery Key or User Recovery Secret
    protects the Backup Data Key
```

## 19. Recovery Modes

The MVP supports at least one clean-install-compatible recovery mode.

Approved candidates include:

```text
User Recovery Secret
Recovery Key File
Recovery Code represented safely
```

The exact primary UX requires the security ADR and implementation spike.

## 20. Same-Installation Convenience

Chronicle may additionally protect the backup data key for the current Windows user.

This is convenience only.

It must not be the sole recovery path.

## 21. User Recovery Secret

When a user secret is used:

- derive a wrapping key through a modern memory-hard KDF;
- use unique salt;
- store KDF parameters in the public header;
- never store the plaintext secret;
- authenticate the encrypted payload.

## 22. Recovery Key File

When a recovery key file is used:

- generate high-entropy key material;
- require explicit user storage;
- never place it beside the backup automatically;
- identify it by opaque key ID;
- support verification without exposing the key in logs.

## 23. Recovery Material Setup

Before Chronicle considers backups fully recoverable, the user must complete a recovery-material setup or acknowledge the limitation explicitly.

## 24. Recovery Material Verification

Chronicle must test that recovery material can unlock a newly created validation artifact or backup.

## 25. No Security Theater

Creating an encrypted backup that cannot be restored on a clean installation does not satisfy this ADR.

## 26. Key Separation

Chronicle maintains separate secret domains:

```text
Installation Database Key
Backup Data Key
Backup Recovery Key or Secret
Provider Credentials
```

## 27. Secret Domain Independence

Rotating or losing one secret domain must not silently mutate another.

## 28. Backup Operation

Every backup creation uses an OperationId.

## 29. Work Items

Work Items may handle:

- snapshot;
- package;
- encryption;
- validation;
- publication;
- retention cleanup.

## 30. Backup Lifecycle

Recommended internal lifecycle:

```text
Requested
Inspecting
Snapshotting
SnapshotCreated
Packaging
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

This does not replace Operation or Work Item state.

## 31. Snapshot Source

The source database is already encrypted.

## 32. Snapshot Consistency

Use the SQLite backup API or another validated consistent snapshot method supported by the encrypted provider.

## 33. No Naive Active File Copy

Direct file copy while active writes may occur is prohibited.

## 34. WAL Awareness

Snapshot implementation must account for all encrypted SQLite journal behavior.

## 35. No Plaintext Database Snapshot

The staged database snapshot must remain encrypted at rest.

## 36. No Plaintext Archive Stage

Chronicle must not create a durable plaintext archive and encrypt it later as a separate unsafe stage.

## 37. Protected Streaming Pipeline

Preferred packaging pipeline:

```text
Encrypted SQLite Snapshot
    ↓
Streamed Backup Packaging
    ↓
Authenticated Encryption
    ↓
Encrypted Staging Artifact
```

## 38. Bounded Memory

Large backups are streamed with bounded memory.

## 39. Backup Preparation

Before snapshot:

- create OperationRecord;
- validate schema and migration state;
- resolve recovery protection;
- verify destination capacity;
- inspect unresolved workflows;
- create opaque staging paths;
- verify no restore publication is active.

## 40. Backup During Migration

Manual backup is blocked during active schema mutation.

The migration workflow creates its own encrypted checkpoint.

## 41. Backup During Restore

Backup creation is blocked during restore publication.

## 42. Unresolved Workflow Inclusion

The snapshot includes unresolved durable state, including:

- OperationRecords;
- WorkItems;
- NarrativeTurns;
- accepted outputs;
- required ProviderAttempts;
- pending Dice workflows;
- migration state.

## 43. Provider Response Staging

Active response staging is included only when:

- required for unresolved recovery;
- unexpired;
- secret checks pass;
- the staged bytes are protected by the encrypted backup payload.

## 44. Default Staging Exclusion

Expired, rejected, deleted, or unnecessary staging is excluded.

## 45. Credential Exclusion

Backups never include raw provider credentials or plaintext database keys.

## 46. Package Bindings

Backups preserve exact package bindings and package inventory.

## 47. Package Files

Official package files required by Campaigns may be included when licensing and provenance permit.

## 48. Unauthorized Content

Backup does not authorize redistribution of sourcebooks or unlicensed assets.

## 49. Rebuildable Data

Rebuildable caches and nonessential logs are excluded.

## 50. Backup Staging

The encrypted artifact is first written under an opaque staging name.

## 51. Staging Protection

Staging must:

- use restrictive permissions;
- never expose plaintext payload;
- be excluded from ordinary indexing where practical;
- be deleted after publication or failure cleanup.

## 52. Encryption Scheme

Use an authenticated-encryption construction approved by the security implementation ADR.

## 53. Authentication Requirement

Confidentiality without integrity is insufficient.

The artifact must detect modification.

## 54. Nonce Discipline

Nonces or initialization values must be unique according to the selected scheme.

## 55. Chunked Encryption

Large artifacts may use authenticated chunk framing to support streaming and bounded memory.

## 56. Chunk Integrity

Every chunk and the complete artifact structure must be authenticated.

## 57. Hashing

Chronicle computes hashes inside the encrypted manifest for:

- database snapshot;
- companion entries;
- package inventory;
- normalized artifact contents.

## 58. Backup Validation

Before publication, Chronicle:

1. closes the encrypted output stream;
2. reopens the artifact;
3. unlocks it using the configured recovery path;
4. verifies authentication;
5. validates the internal manifest;
6. opens the encrypted SQLite snapshot;
7. runs integrity and foreign-key checks;
8. validates workflow and Dice relationships;
9. confirms no prohibited secret content;
10. marks it ready for publication.

## 59. Wrong Recovery Secret

Validation must fail safely and leave no published backup.

## 60. Database Validation

At minimum:

- encrypted database opens;
- correct internal schema marker exists;
- integrity check passes;
- foreign-key check passes;
- schema version is supported;
- migration state is coherent.

## 61. Workflow Validation

Validate:

- canonical Operation states;
- Work Item states and dependencies;
- Narrative Turn correlations;
- accepted-output uniqueness;
- Provider Attempt ownership;
- Dice state and evidence consistency;
- continuation uniqueness.

## 62. Dice Validation

Validate:

- evidence ownership;
- evidence sequence uniqueness;
- committed generation-stage completeness;
- resolution evidence hashes;
- correction history;
- exact package binding.

## 63. Backup Publication

Only validated encrypted artifacts receive their final published name.

## 64. Atomic Publication

Use same-volume atomic rename where possible.

## 65. Backup Catalog

The local catalog may store:

```text
BackupId
BackupTypeKey
ArtifactLocation
CreatedAtUtc
ApplicationVersion
DatabaseSchemaVersion
EncryptionSchemeKey
RecoveryModeKey
ValidationStatusKey
ArtifactSize
LastVerifiedAtUtc
RetentionClassKey
```

## 66. Catalog Secret Exclusion

The catalog does not contain backup keys or user recovery secrets.

## 67. Artifact Self-Description

Loss of the catalog does not make a valid artifact unusable.

## 68. Manual Reverification

The user may reverify a backup at any time.

## 69. Restore Principle

Restore is replacement, not merge.

## 70. Restore Operation

Every restore uses an OperationId.

## 71. Restore Lifecycle

Recommended internal lifecycle:

```text
Requested
InspectingArtifact
AwaitingRecoveryMaterial
DecryptingToProtectedStaging
CompatibilityChecking
ValidatingStagedData
CheckpointingCurrentData
ReadyToPublish
Publishing
RekeyingForInstallation
ValidatingActiveData
Completed
RolledBack
FailedRetryable
FailedTerminal
RecoveryRequired
Cancelled
```

## 72. Artifact Inspection

Inspection does not mutate active data.

## 73. Recovery Material

Chronicle obtains recovery material through a secure user flow.

## 74. No Recovery Secret Logging

Recovery secrets never enter logs, metrics, crash reports, clipboard history intentionally, or persisted UI state.

## 75. Clean-Installation Restore

Restore must work when:

- no prior database exists;
- no prior installation database key exists;
- the backup recovery material is valid.

## 76. Isolated Staging

Restore uses an isolated staging data root.

## 77. No Plaintext Restore Files

The restored database remains encrypted throughout staging.

## 78. Backup Decryption and Database Protection

The implementation may:

- decrypt backup framing into an encrypted SQLite file;
- then rekey that database for the target installation.

It must not write a plaintext SQLite database.

## 79. New Installation Key

The target installation generates a new high-entropy database key.

## 80. Rekey on Restore

Before activation, Chronicle may rekey the staged database from its backup-contained encryption state to the new installation key.

## 81. Key Publication

The new protected installation key is published only through a journaled, recoverable sequence.

## 82. Compatibility Inspection

Inspect:

- backup contract;
- encryption scheme;
- KDF parameters;
- application version;
- schema version;
- package inventory;
- unresolved workflows;
- disk space;
- target platform support.

## 83. Unsupported Artifact

Unsupported encryption or schema prevents activation.

## 84. Hostile Artifact Policy

Treat backup artifacts as untrusted input.

## 85. Path Safety

Reject:

- traversal;
- absolute paths;
- links;
- duplicate canonical paths;
- case collisions;
- oversized entries;
- malformed framing;
- excessive chunk counts;
- decompression bombs;
- unsupported executable content.

## 86. Pre-Restore Checkpoint

Before replacing active data, create and validate an encrypted checkpoint of the current state.

## 87. Checkpoint Recovery Path

The checkpoint must have a valid recovery path independent from the data being replaced.

## 88. Checkpoint Failure

If checkpoint creation or verification fails, restore is blocked.

## 89. Publication Boundary

Normal database access is closed or isolated before activation.

## 90. Publication Sequence

Recommended:

1. validate restored staging;
2. validate current-data checkpoint;
3. close active database access;
4. journal target key and filesystem phases without plaintext secrets;
5. move active data to rollback staging;
6. move restored encrypted data into active location;
7. publish protected target key reference;
8. open with target installation key;
9. validate active data;
10. complete restore;
11. retain rollback material according to policy.

## 91. Restore Journal

The journal includes:

```text
RestoreOperationId
BackupId
CheckpointId
OldActiveLocation
NewStagedLocation
PublicationStep
TargetKeyReferenceId
ArtifactHash
UpdatedAtUtc
```

No plaintext key material is stored.

## 92. Crash Recovery

Startup inspects:

- restore journal;
- filesystem state;
- protected key references;
- active database compatibility;
- checkpoint availability.

## 93. No Guessing

Ambiguous state enters `RecoveryRequired`.

## 94. Active Validation

After publication:

- retrieve target installation key;
- open encrypted database;
- validate DataRootId binding;
- run integrity checks;
- validate schema;
- validate package bindings;
- inspect unresolved workflows.

## 95. Rollback

If active validation fails, restore the encrypted pre-restore checkpoint.

## 96. Rollback Key Handling

Rollback must restore the matching database and protected-key relationship atomically or journal-recoverably.

## 97. Safe Mode

Safe Mode can:

- inspect encrypted artifact compatibility;
- request recovery material;
- retry decryption;
- inspect restore journal;
- validate current and rollback databases;
- roll back;
- collect redacted diagnostics.

## 98. Safe Mode Secret Handling

Safe Mode never displays plaintext keys or persists entered recovery secrets.

## 99. Unresolved Work After Restore

Restored unresolved workflows are inspected before execution.

## 100. Work Item Claims

Claims from the source installation are not assumed valid.

## 101. Provider Attempts

Remote requests are nonauthoritative and may no longer exist.

## 102. Pending Dice

A Roll awaiting player release remains pending.

## 103. Committed Dice

Committed random evidence is reused exactly.

## 104. No Reroll After Restore

Restore or recovery must never regenerate a committed generation stage.

## 105. Narrative Continuation

Chronicle checks for an existing continuation before scheduling one.

## 106. Missing Provider Credentials

Campaign truth remains available.

Provider-dependent Work Items wait for configuration.

## 107. Backup Retention

Retention classes:

```text
Manual
MigrationCheckpoint
RestoreCheckpoint
Recovery
```

## 108. Manual Backups

Retained until explicit user deletion.

## 109. Migration Checkpoints

Retained through migration success and rollback window.

## 110. Restore Checkpoints

Retained through active validation and recovery window.

## 111. Recovery Checkpoints

Retained while related RecoveryRequired state exists.

## 112. No Deletion of Sole Recovery Path

Chronicle must not delete the only known valid recovery artifact.

## 113. Cleanup Work Item

Retention cleanup is durable and recovery-aware.

## 114. Cleanup Validation

Before deletion:

- verify artifact identity;
- verify no unresolved Operation references it;
- verify recovery alternatives;
- verify artifact is not an active checkpoint;
- verify path safety.

## 115. Backup Password or Recovery Secret Change

Changing a user recovery secret does not retroactively alter old backups unless explicitly rewrapped or recreated.

## 116. Backup Rewrap

A future workflow may rewrap the backup data key under new recovery material without reserializing all Campaign data, if the selected format supports it safely.

## 117. MVP Rewrap Scope

The format should permit rewrap, but a full user-facing rewrap manager may be deferred.

## 118. Key Rotation Interaction

Active database key rotation does not invalidate existing backups.

## 119. Backup Recovery Key Rotation

Backup recovery policy is independent and explicit.

## 120. No Automatic Upload

Chronicle never uploads backup artifacts or recovery material automatically.

## 121. External Destination

The user explicitly chooses the destination.

## 122. Removable Media

Chronicle verifies the published encrypted artifact after writing.

## 123. Network Shares

Network-share backup may be supported only after validation and warning.

## 124. Privacy Disclosure

The UI must explain:

- backups contain private Chronicle data;
- backups are encrypted;
- recovery material is required;
- losing all recovery material may make the backup unusable;
- Chronicle does not possess a master recovery key.

## 125. No Vendor Escrow by Default

OpenAI, Chronicle maintainers, and package authors do not receive backup keys by default.

## 126. Logging

Safe logs may include:

- BackupId;
- RestoreOperationId;
- encryption scheme key;
- recovery mode key;
- KDF parameter profile name;
- artifact size;
- stage;
- duration;
- safe error code.

They must not include:

- recovery secret;
- backup data key;
- installation database key;
- plaintext manifest;
- Campaign prose;
- provider credentials;
- raw staged response.

## 127. Metrics

Useful local metrics include:

```text
EncryptedBackupDuration
EncryptedBackupSize
BackupEncryptionDuration
BackupValidationDuration
RestoreUnlockDuration
RestoreRekeyDuration
RestoreRollbackCount
CheckpointValidationFailureCount
RecoveryRequiredCount
```

No remote telemetry is required.

## 128. Error Model

Recommended backup errors:

```text
backup.recovery-material-not-configured
backup.recovery-material-invalid
backup.encryption-scheme-unsupported
backup.encryption-failed
backup.authentication-failed
backup.not-enough-space
backup.snapshot-failed
backup.database-integrity-failed
backup.packaging-failed
backup.validation-failed
backup.publication-failed
backup.recovery-required
```

Recommended restore errors:

```text
restore.artifact-not-found
restore.artifact-invalid
restore.artifact-unsafe
restore.recovery-material-required
restore.recovery-material-invalid
restore.encryption-scheme-unsupported
restore.authentication-failed
restore.version-unsupported
restore.schema-unsupported
restore.staging-failed
restore.database-invalid
restore.checkpoint-failed
restore.rekey-failed
restore.key-publication-failed
restore.publication-failed
restore.active-validation-failed
restore.rollback-failed
restore.recovery-required
```

## 129. Data Preservation State

Results should state:

```text
AuthoritativeDataUnchanged
EncryptedSnapshotCreated
BackupEncrypted
BackupValidated
BackupPublished
RecoveryMaterialVerified
CurrentDataCheckpointCreated
RestoredDataEncrypted
RestoredDataStaged
TargetKeyGenerated
RestoredDatabaseRekeyed
RestoredDataPublished
RestoredDataValidated
PreviousDataRestored
NoPartialActivation
RecoveryRequired
```

## 130. Testing Strategy

The implementation requires:

```text
Encryption Tests
Recovery Material Tests
Clean-Install Restore Tests
Snapshot Tests
WAL Tests
No-Plaintext Tests
Backup Content Tests
Backup Validation Tests
Publication Tests
Hostile Artifact Tests
Restore Staging Tests
Rekey Tests
Restore Publication Tests
Rollback Tests
Unresolved Workflow Tests
Dice Recovery Tests
Provider Recovery Tests
Retention Tests
Privacy Tests
Fault Injection Tests
```

## 131. Encryption Tests

Tests prove:

- artifact payload is unreadable without recovery material;
- wrong material fails authentication;
- modified ciphertext fails;
- no active database key is embedded in plaintext.

## 132. Recovery Material Tests

Tests cover:

- valid recovery secret;
- wrong secret;
- key file;
- missing material;
- clean installation;
- verification flow.

## 133. No-Plaintext Tests

Search controlled directories for content canaries during:

- snapshot;
- packaging;
- encryption;
- validation;
- restore;
- rekey;
- rollback.

Canaries must not appear in durable plaintext files.

## 134. Clean-Installation Restore Tests

Restore a backup on a clean Chronicle installation with no original protected key record.

## 135. Backup Content Tests

Prove inclusion of:

- Campaign truth;
- Messages;
- Memories;
- Character Knowledge;
- progression;
- corrections;
- Operations;
- Work Items;
- Narrative Turns;
- accepted outputs;
- required Provider Attempts;
- Dice Rolls;
- random evidence;
- resolution stages;
- package bindings.

## 136. Backup Exclusion Tests

Prove exclusion of:

- raw credentials;
- installation database key;
- backup recovery secret;
- expired staging;
- unnecessary logs;
- caches.

## 137. Validation Tests

Corrupt:

- public header;
- authentication tag;
- encrypted chunk;
- internal manifest;
- database;
- package inventory;
- workflow relationship;
- Dice evidence.

Validation must fail.

## 138. Publication Tests

Inject failure:

- before encryption;
- during encrypted streaming;
- after encryption;
- during reopen validation;
- before rename;
- after rename;
- before catalog completion.

No invalid artifact is marked successful.

## 139. Hostile Artifact Tests

Cover:

- path traversal;
- malformed chunk framing;
- duplicate entries;
- oversized header;
- excessive KDF parameters;
- resource exhaustion;
- decompression bomb;
- unsupported algorithm identifiers.

## 140. Rekey Tests

Inject failure:

- before target key generation;
- after key generation;
- during database rekey;
- after rekey;
- before target-key protection;
- after target-key protection;
- before activation.

Recovery must retain a valid database and key pairing.

## 141. Restore Publication Tests

Inject failure at every publication and journal step.

## 142. Rollback Tests

Prove prior encrypted data and its key reference are restored consistently.

## 143. Unresolved Workflow Tests

Restore fixtures include:

- Operation in RecoveryRequired;
- Work Item WaitingForRetry;
- interrupted provider call;
- staged provider response;
- pending Dice;
- committed Dice evidence;
- resolved Dice;
- continuation pending.

## 144. Dice Tests

Prove no committed random evidence changes or regenerates.

## 145. Provider Tests

Prove Provider Attempts remain nonauthoritative and missing credentials pause safely.

## 146. Retention Tests

Prove cleanup preserves the only valid recovery path.

## 147. Privacy Tests

Synthetic database keys, backup keys, recovery secrets, provider credentials, and private content must not appear in logs or public headers.

## 148. Cross-System Test

A complex non-Werewolf Campaign round-trips through encrypted backup and clean-install restore without Core special cases.

## 149. Architecture Tests

Architecture tests must reject:

- plaintext backup publication;
- durable plaintext archive staging;
- active database key reused as backup key;
- recovery secret persisted;
- clean-install restore unsupported;
- restore extracted directly into active data root;
- restore merge;
- restore without rollback checkpoint;
- committed Dice evidence omitted;
- credentials included;
- automatic remote upload;
- backup format implemented as portable Campaign export.

## 150. Prohibited Patterns

### 150.1 Encrypted Database File Copy as Complete Backup

A backup needs an independent clean-install recovery path.

### 150.2 Plaintext Archive Then Encrypt

Use protected streaming or encrypted staging.

### 150.3 Database Key Inside Backup Manifest

Separate key domains.

### 150.4 Windows User Protection as Sole Recovery

Support clean-install recovery.

### 150.5 Restore in Place

Stage and validate first.

### 150.6 Restore Merge

Replacement only.

### 150.7 Reroll After Restore

Preserve committed evidence.

### 150.8 Backup Without Reopen Validation

Unlock and validate before publication.

### 150.9 Delete the Only Recovery Artifact

Protect active recovery paths.

### 150.10 Chronicle Master Key

No vendor-held universal recovery key.

## 151. Alternatives Considered

### Unencrypted Backup for MVP

Rejected because it would expose private Chronicle data and force a later format and workflow redesign.

### Copy Encrypted Database and Rely on Credential Manager

Rejected because clean-install and new-machine recovery would fail.

### Reuse Active Database Key

Rejected because it couples installation compromise, backup compromise, and key rotation.

### Password-Protected ZIP

Rejected as an architectural decision because algorithm, KDF, metadata leakage, streaming, and validation requirements need a Chronicle-controlled reviewed format.

### Store Backup Key Beside Artifact

Rejected because it defeats encryption.

### Cloud-Managed Recovery

Deferred because local independent recovery is the MVP requirement.

## 152. Consequences

### Positive

- no post-MVP backup-encryption rewrite;
- backups protect private content;
- clean-install recovery is supported;
- active database and backup keys remain separate;
- key rotation does not invalidate backups;
- restore can establish a new installation key;
- unresolved workflows and Dice evidence remain safe;
- future automation can extend the same format.

### Negative

- recovery-material UX is required in MVP;
- cryptographic framing and streaming add complexity;
- lost recovery material may make backups unrecoverable;
- restore needs rekey and key-publication recovery;
- testing surface is substantially larger.

## 153. Risks

### User Loses Recovery Material

Mitigation:

- setup verification;
- clear disclosure;
- optional multiple wrapping methods;
- recovery readiness status.

### Weak User Secret

Mitigation:

- memory-hard KDF;
- minimum policy;
- strength feedback;
- recovery key alternative.

### Plaintext Leak During Packaging

Mitigation:

- encrypted streaming;
- canary tests;
- restrictive staging;
- architecture tests.

### Restore Rekey Crash

Mitigation:

- restore journal;
- pre-restore checkpoint;
- preserve valid key pairings;
- fault injection.

### Algorithm Obsolescence

Mitigation:

- versioned encryption scheme;
- rewrap or migration support;
- explicit compatibility metadata.

## 154. Technology Spike

Before acceptance, implement:

1. encrypted backup format prototype;
2. authenticated streaming encryption;
3. recovery secret or key-file mode;
4. memory-hard KDF;
5. clean-install restore;
6. encrypted SQLite snapshot integration;
7. no-plaintext staging;
8. encrypted manifest;
9. reopen validation;
10. target installation key generation;
11. restore rekey;
12. key-publication journal;
13. pre-restore encrypted checkpoint;
14. rollback;
15. hostile artifact validation;
16. retention cleanup;
17. unresolved workflow fixtures;
18. complex Dice fixture;
19. privacy canary tests;
20. architecture tests.

## 155. Spike Acceptance

The spike passes when:

- a live encrypted Chronicle database produces an encrypted backup;
- no durable plaintext backup payload is created;
- wrong recovery material fails safely;
- a clean installation restores the backup;
- the restored database uses a new installation key;
- current data receives an encrypted rollback checkpoint;
- failure at every rekey and publication point is recoverable;
- unresolved Operations, Work Items, Narrative Turns, and Dice workflows survive;
- credentials and all key material remain absent from logs and public metadata;
- complex non-Werewolf Dice evidence survives exactly.

## 156. Definition of Compliance

An implementation complies when:

- every published backup is encrypted;
- the active database key is not the backup key;
- backup artifacts have a clean-install recovery path;
- no plaintext authoritative database or archive is written durably;
- backups are reopened and validated before publication;
- restore stages encrypted data before activation;
- restore creates or binds a target installation key safely;
- current data receives a validated encrypted rollback checkpoint;
- restore never merges;
- unresolved workflows and committed Dice evidence are preserved;
- credentials and plaintext secrets are excluded;
- Safe Mode handles ambiguous recovery;
- no automatic upload occurs;
- backup and portable export remain separate.

## 157. Review Triggers

Review this ADR if:

- hardware-backed recovery keys are introduced;
- cloud backup becomes official;
- enterprise escrow is required;
- cross-platform restore is implemented;
- backup algorithm policy changes;
- scheduled backup becomes mandatory;
- multi-user shared data roots are introduced;
- server-hosted persistence replaces desktop-local authority.

## 158. Deferred Decisions

Later decisions may define:

- multiple recovery-key slots;
- hardware security keys;
- enterprise escrow;
- automated backup scheduling;
- cloud destinations;
- incremental encrypted backup;
- deduplication;
- backup rewrap UI;
- cross-platform secret handling;
- signed public headers.

## 159. Final Decision

Chronicle backups will be encrypted from the MVP onward.

They will not depend solely on the original Windows account.

They will not contain the active database key in plaintext.

They will restore on a clean installation, establish a valid new installation key, and preserve unfinished work and committed history exactly.

The MVP backup system will be smaller than its future form.

It will not be disposable.
