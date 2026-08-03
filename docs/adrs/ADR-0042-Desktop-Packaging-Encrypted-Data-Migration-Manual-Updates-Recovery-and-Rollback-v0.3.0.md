---
id: ADR-0042
title: Desktop Packaging, Encrypted Data Migration, Manual Updates, Recovery, and Rollback
status: Proposed
version: 0.3.0
owner: Chronicle Team
last_updated: 2026-08-02
category: Technology
supersedes:
  - ADR-0042@0.2.0
  - ADR-0042@0.1.0
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
  - RFC-0035
  - RFC-0036
  - RFC-0037
  - RFC-0038
  - RFC-0039
  - RFC-0040
  - RFC-0041
  - RFC-0042
  - ADR-0001
  - ADR-0002
  - ADR-0003
  - ADR-0004
  - ADR-0005
  - ADR-0006
  - ADR-0007
  - ADR-0008
  - ADR-0009
  - ADR-0010
  - ADR-0011
  - ADR-0012
  - ADR-0013
  - ADR-0014
  - ADR-0015
  - ADR-0016
  - ADR-0017
  - ADR-0018
  - ADR-0019
  - ADR-0020
  - ADR-0021
  - ADR-0022
  - ADR-0023
  - ADR-0024
  - ADR-0025
  - ADR-0026
  - ADR-0027
  - ADR-0028
  - ADR-0029
  - ADR-0030
  - ADR-0031
  - ADR-0032
  - ADR-0033
  - ADR-0034
  - ADR-0035
  - ADR-0036
  - ADR-0037
  - ADR-0038
  - ADR-0039
  - ADR-0040
  - ADR-0041
implements:
  - RFC-0042
related_to:
  - ADR-0043
  - ADR-0044
---

> **"An update may replace binaries. It must never gamble with encrypted user history."**

# Desktop Packaging, Encrypted Data Migration, Manual Updates, Recovery, and Rollback

## 1. Status

**Proposed**

This ADR defines Chronicle's desktop packaging, manual-update, encrypted-data migration, recovery, downgrade, and rollback architecture.

This revision corrects ADR-0042 version 0.2.0 by integrating the foundational encryption decisions from ADR-0004 version 0.3.0 and ADR-0026 version 0.3.0.

The decision is:

- continue using Inno Setup for the Windows installer;
- use manual download and installation for MVP;
- do not implement an in-app updater, background downloader, delta updater, or self-updating bootstrapper in MVP;
- sign Stable installers and publish cryptographic hashes;
- keep application binaries separate from the user data root;
- keep the authoritative SQLite database encrypted from the first production schema;
- keep database-key material separate from the data root;
- preserve the protected-key relationship during application updates;
- run database migrations from Chronicle at first launch, never from installer scripts;
- require an encrypted, validated pre-migration checkpoint before any destructive migration;
- prohibit plaintext migration copies or decrypted staging databases;
- treat schema migration and encryption-parameter migration as separate, explicit operations;
- journal key rotation, rekey, and provider-format migration without storing plaintext key material;
- block normal Campaign mutation while migration or key recovery is unresolved;
- recover into Safe Mode when migration, rekey, key publication, or validation fails;
- never create a replacement empty database when the encrypted database cannot be opened;
- preserve exact Rule Set package bindings and unresolved workflow state;
- support manual downgrade only when both schema and encryption format are compatible;
- otherwise require restore of a compatible encrypted checkpoint;
- keep Stable, Preview, and Development data roots isolated, including their protected keys;
- make ordinary uninstall preserve Campaign data, encrypted backups, and protected-key records unless the user explicitly requests complete deletion;
- ensure complete deletion warns about irreversible encrypted-data loss;
- keep installer, migration, recovery, and Safe Mode text in English as the base language;
- extend the same migration foundation later rather than replacing it when automated updates are introduced.

## 2. Context

Chronicle's updater and installer architecture cannot treat the database as a normal unprotected application file.

A release may change:

- application binaries;
- EF Core schema;
- Rule Set package contracts;
- persistence payload versions;
- encrypted SQLite provider version;
- cipher parameters;
- KDF parameters;
- page size;
- journal behavior;
- protected-key metadata;
- backup contract;
- recovery state.

A migration failure can therefore affect both:

- data structure;
- the ability to decrypt the database.

The application must always preserve at least one validated pairing of:

```text
Encrypted Database
Protected Key Reference
Compatible Application Version
Compatible Schema
Compatible Encryption Format
```

## 3. Scope

This ADR covers:

- installer packaging;
- release channels;
- manual update;
- installation repair;
- first-launch migration;
- encrypted checkpoint creation;
- schema migration;
- encrypted-provider migration;
- database rekey;
- key-reference publication;
- rollback;
- downgrade;
- uninstall;
- data-root isolation;
- Safe Mode;
- recovery UX;
- update and migration tests.

## 4. Out of Scope

This ADR does not implement in MVP:

- in-app update discovery;
- background download;
- automatic installation;
- delta patches;
- release-feed polling;
- forced updates;
- cloud migration service;
- remote key escrow;
- automatic cross-device migration.

## 5. Installer Technology

Chronicle uses:

```text
Inno Setup
```

for Windows packaging.

## 6. Installer Responsibilities

The installer may:

- install application binaries;
- install native runtime dependencies;
- create shortcuts;
- register uninstall metadata;
- display license and privacy notices;
- record release-channel installation metadata;
- launch Chronicle after installation.

## 7. Installer Prohibitions

The installer must not:

- open or migrate the encrypted database;
- retrieve the database key;
- rotate or rekey the database;
- modify Campaign records;
- copy plaintext user data;
- delete the user data root during ordinary update;
- install credentials;
- silently change Rule Set bindings.

## 8. Binary and Data Separation

Recommended layout:

```text
Program Files/
    Chronicle binaries

User data root/
    encrypted Chronicle database
    encrypted backups
    package files
    protected staging
    recovery journals
```

## 9. Key Storage Separation

Protected key records remain outside both:

- application binaries;
- encrypted database file.

## 10. Update Model

The MVP update model is:

```text
User downloads signed installer
    ↓
User verifies publisher and optionally hash
    ↓
Installer replaces compatible binaries
    ↓
Chronicle starts
    ↓
Chronicle inspects encrypted data compatibility
    ↓
Chronicle creates checkpoint if migration is needed
    ↓
Chronicle migrates and validates
```

## 11. No In-App Updater in MVP

The MVP does not include:

- release-feed client;
- background update service;
- automatic download;
- automatic restart;
- delta update;
- silent update.

## 12. Future Updater Compatibility

A future updater must call the same Chronicle-owned migration and recovery workflows.

It must not introduce a second migration path.

## 13. Release Signing

Stable installers must be code-signed.

## 14. Published Hashes

Release artifacts publish cryptographic hashes.

## 15. Release Channels

Chronicle supports:

```text
Stable
Preview
Development
```

## 16. Data-Root Isolation

Each channel uses a separate data root.

## 17. Key Isolation

Each channel has separate:

- DataRootId;
- installation database key;
- protected key reference;
- backup catalog;
- migration journal.

## 18. No Shared Database Across Channels

Stable, Preview, and Development must not open the same authoritative database.

## 19. No Shared Key Assumption

A key from one channel must not unlock another channel's data root by default.

## 20. First Launch After Update

Chronicle performs:

1. resolve release channel;
2. resolve data root;
3. resolve protected database key;
4. inspect encrypted database format;
5. inspect schema version;
6. inspect migration journal;
7. inspect encryption migration journal;
8. determine compatibility;
9. open normally, migrate, or enter Safe Mode.

## 21. No Empty Database on Failure

If an existing encrypted database cannot be opened:

- do not create a new database in the same data root;
- do not overwrite key metadata;
- do not classify the problem as ordinary first run;
- enter recovery.

## 22. Compatibility Dimensions

Chronicle checks:

```text
Application Version
Schema Version
Minimum Reader Version
Encrypted Provider Format
Cipher Version
KDF Profile
Page Size
Protected-Key Metadata Version
Rule Set Package Bindings
Migration State
```

## 23. Compatibility Result

Recommended outcomes:

```text
Compatible
MigrationRequired
EncryptionMigrationRequired
SchemaAndEncryptionMigrationRequired
NewerUnsupported
KeyUnavailable
WrongKey
Corrupt
RecoveryRequired
```

## 24. Migration Ownership

Chronicle application code owns migrations.

## 25. No Installer Migration

Inno Setup must not execute EF Core migrations or encryption commands.

## 26. Migration Operation

Every migration uses an OperationId.

## 27. Migration Work Items

Migration may use durable Work Items for:

- inspection;
- checkpoint creation;
- schema migration;
- encryption migration;
- validation;
- cleanup.

## 28. Migration Lifecycle

Recommended internal lifecycle:

```text
Requested
Inspecting
Checkpointing
CheckpointValidated
Preparing
MigratingSchema
MigratingEncryption
PublishingKeyReference
Validating
Completed
RolledBack
FailedRetryable
FailedTerminal
RecoveryRequired
```

This is migration-specific detail and does not replace Operation state.

## 29. Exclusive Mutation Window

Normal Campaign mutation is blocked during migration.

## 30. Read Access During Migration

Normal reads are also blocked unless a specific Safe Mode read-only path is proven safe.

## 31. Pre-Migration Checkpoint

Before any destructive migration, Chronicle creates a validated encrypted checkpoint.

## 32. Checkpoint Requirements

The checkpoint must preserve:

- encrypted database;
- matching recovery material;
- schema version;
- encryption format;
- protected-key compatibility metadata;
- package bindings;
- unresolved workflows;
- exact application compatibility information.

## 33. Checkpoint Validation

Chronicle must restore or reopen the staged checkpoint sufficiently to prove it is usable.

## 34. Checkpoint Failure

If checkpoint creation or validation fails:

- migration does not start;
- active data remains unchanged;
- user receives a typed recovery error.

## 35. Schema Migration

Schema migration runs against the encrypted database through Chronicle's encrypted connection factory.

## 36. No Plaintext Schema Copy

The migration process must not export the database to plaintext, migrate it, then re-encrypt it.

## 37. Encryption Migration

Changes to encryption technology or parameters require a separate explicit phase.

## 38. Encryption Migration Triggers

Examples:

- new encrypted SQLite provider;
- cipher upgrade;
- KDF profile change;
- page-size change;
- encrypted journal behavior change;
- protected-key metadata version change.

## 39. No Silent Encryption Upgrade

Encryption migration must never happen merely as an undocumented side effect of opening the database.

## 40. Rekey Operation

Where supported, database rekey uses an explicit OperationId and migration journal.

## 41. Rekey Journal

Recommended fields:

```text
MigrationOperationId
DataRootId
SourceEncryptionProfile
TargetEncryptionProfile
OldKeyReferenceId
NewKeyReferenceId
PhaseKey
CheckpointId
UpdatedAtUtc
```

No plaintext key material is stored.

## 42. Rekey Ordering

The workflow must preserve a recoverable key/database pairing at every durable boundary.

## 43. New Key Generation

A new high-entropy key is generated locally.

## 44. New Key Protection

The new key is protected before or as part of a recoverable publication protocol.

## 45. Rekey Validation

After rekey:

- close database;
- reopen with new key;
- verify DataRootId;
- run integrity checks;
- verify schema;
- verify critical records;
- only then retire old key material.

## 46. Old Key Retention During Migration

The old protected key reference remains available until the new database/key pairing is validated.

## 47. Key Retirement

Old key material is removed only after:

- migration completion;
- active validation;
- rollback window policy;
- no unresolved journal dependency.

## 48. Combined Schema and Encryption Migration

When both are required, the migration plan defines an explicit order.

## 49. Preferred Combined Order

The spike must select and validate one order based on provider behavior:

```text
Checkpoint
Schema migration
Validation
Encryption migration
Validation
Completion
```

or:

```text
Checkpoint
Encryption migration
Validation
Schema migration
Validation
Completion
```

The order must not vary ad hoc.

## 50. Migration Transactions

Each EF Core migration uses the safest supported transaction behavior.

## 51. No Long External Work in Transaction

Checkpoint packaging, archive encryption, and user confirmation occur outside database transactions.

## 52. Migration Validation

After migration, validate:

- encrypted open;
- correct key binding;
- SQLite integrity;
- foreign keys;
- schema version;
- lifecycle state registries;
- package bindings;
- unresolved Operations;
- pending Work Items;
- Narrative Turns;
- Dice evidence;
- continuation uniqueness.

## 53. Migration Completion

Migration is complete only after validation succeeds.

## 54. Migration Failure Before Mutation

Active data remains unchanged.

## 55. Migration Failure During Schema Change

Use transaction rollback where supported, then inspect state.

## 56. Migration Failure During Rekey

Enter recovery and preserve both old and new key references until outcome is known.

## 57. Unknown Migration Outcome

Chronicle inspects:

- migration journal;
- schema version;
- encryption profile;
- which key opens the database;
- checkpoint;
- integrity;
- current filesystem state.

## 58. No Guessing

Ambiguous outcomes enter `RecoveryRequired`.

## 59. Rollback

Rollback restores the validated encrypted pre-migration checkpoint.

## 60. Rollback Publication

Rollback uses the same staged, journaled, validation-first publication model as restore.

## 61. Rollback Key Pairing

The checkpoint database and its matching key-recovery relationship must be restored together.

## 62. No Row-Level Reverse Migration Requirement

Chronicle does not require every destructive migration to have a handcrafted reverse migration.

The validated checkpoint is the primary rollback path.

## 63. Safe Mode

Safe Mode opens when:

- key is unavailable;
- wrong key is detected;
- cipher is unsupported;
- migration is incomplete;
- rekey outcome is uncertain;
- active validation fails;
- rollback fails;
- database is corrupt;
- schema is newer than the binary supports.

## 64. Safe Mode Capabilities

Safe Mode can:

- inspect compatibility;
- retry key retrieval;
- inspect migration and rekey journals;
- validate checkpoints;
- restore a checkpoint;
- restore an encrypted backup;
- collect redacted diagnostics;
- exit without mutation.

## 65. Safe Mode Prohibitions

Safe Mode must not:

- show plaintext key material;
- create a replacement database over existing data;
- run provider requests;
- continue gameplay mutation;
- silently upgrade Rule Set packages;
- discard unresolved workflows.

## 66. Manual Downgrade

A user may manually install an older binary.

## 67. Downgrade Compatibility

The older binary may open the current data only if it supports:

- the schema version;
- the encrypted provider format;
- the cipher profile;
- the protected-key metadata;
- the package contracts.

## 68. Unsupported Downgrade

If incompatible, Chronicle must:

- refuse normal open;
- preserve active data;
- direct the user to a compatible encrypted checkpoint or backup;
- avoid any automatic destructive conversion.

## 69. No Silent Downgrade Migration

An older binary must not rewrite newer data into an older schema automatically.

## 70. Recovery Through Checkpoint

The supported downgrade path is restoration of a checkpoint created by a compatible version.

## 71. Rule Set Package Compatibility

Application update must preserve exact package bindings.

## 72. No Silent Package Upgrade

Migration must not reinterpret historical Campaign or Dice data through a newer package automatically.

## 73. Package Schema Migration

Package-owned payload migration requires:

- exact source contract;
- target contract;
- package migration handler;
- validation;
- checkpoint;
- provenance.

## 74. Missing Package

If a required package is missing after update:

- Campaign truth remains preserved;
- restricted read-only mode may open;
- mechanical mutation is blocked;
- package recovery is offered.

## 75. Native Encrypted Provider Packaging

The installer includes the approved native encrypted SQLite binaries.

## 76. Native Binary Integrity

Release build validates:

- expected hashes;
- architecture;
- version;
- license notices;
- SBOM entries.

## 77. Provider Upgrade

Upgrading the native encrypted SQLite provider requires compatibility tests against every supported database format.

## 78. No Unreviewed Native Replacement

A new native provider cannot be swapped in only because the EF Core API still compiles.

## 79. Installation Repair

Repair may replace application binaries and native dependencies.

## 80. Repair Prohibitions

Repair must not:

- reset protected keys;
- replace user data;
- delete encrypted backups;
- run destructive migration automatically outside Chronicle;
- reset package bindings.

## 81. Ordinary Uninstall

Ordinary uninstall removes application binaries and shortcuts.

## 82. Data Preservation on Uninstall

Ordinary uninstall preserves:

- encrypted database;
- encrypted backups;
- recovery journals;
- package files;
- protected database-key records;
- backup catalog.

## 83. Why Preserve Key Records

Leaving only the encrypted database while deleting its protected key would create avoidable data loss.

## 84. Complete Removal Option

A separate explicit complete-removal action may delete user data and protected key records.

## 85. Complete Removal Warning

The UI must warn that deleting key records and recovery material can make remaining encrypted files permanently unreadable.

## 86. Deletion Ordering

For complete removal:

1. confirm scope;
2. identify data roots and backup locations;
3. explain external backups are not automatically deleted;
4. delete local encrypted data as selected;
5. delete protected key references only after selected data handling;
6. record no private residual log.

## 87. No Casual Key Deletion

Protected key deletion must not be an unchecked installer default.

## 88. Preview and Development Removal

Removing one channel must not delete another channel's data or keys.

## 89. Installer Language

Base installer text is English.

## 90. Localization

Additional installer languages may be added later without changing technical identifiers.

## 91. Migration UX

Migration UI should show:

```text
Inspecting encrypted data
Creating recovery checkpoint
Migrating database
Updating encryption
Validating data
Completing update
Recovery required
```

## 92. Honest Progress

Indeterminate operations use indeterminate progress.

## 93. No Fake Percentage

Do not display exact percentages unless measurable.

## 94. Recovery Actions

Safe actions may include:

- retry validation;
- retry key retrieval;
- restore checkpoint;
- restore encrypted backup;
- open diagnostics;
- exit.

## 95. Error Model

Recommended errors:

```text
update.installer-signature-invalid
update.release-channel-mismatch
update.data-root-conflict
update.database-key-unavailable
update.database-wrong-key
update.cipher-unsupported
update.schema-newer-than-application
update.migration-required
update.checkpoint-failed
update.checkpoint-invalid
update.schema-migration-failed
update.encryption-migration-failed
update.rekey-failed
update.rekey-outcome-unknown
update.key-publication-failed
update.active-validation-failed
update.rollback-failed
update.package-missing
update.downgrade-incompatible
update.recovery-required
```

## 96. Data Preservation State

Results should state:

```text
AuthoritativeDataUnchanged
BinariesUpdated
EncryptedDataUnchanged
CheckpointCreated
CheckpointValidated
SchemaMigrationNotStarted
SchemaMigrated
EncryptionMigrationNotStarted
DatabaseRekeyed
NewKeyProtected
OldKeyRetained
MigrationValidated
RollbackCompleted
NormalMutationBlocked
RecoveryRequired
```

## 97. Logging

Safe logs may include:

- application version;
- release channel;
- DataRootId;
- migration ID;
- schema version;
- encryption profile key;
- native provider version;
- old and new key-reference IDs;
- checkpoint ID;
- phase;
- duration;
- safe error code.

They must not include:

- plaintext keys;
- recovery secrets;
- provider credentials;
- connection strings containing key material;
- Campaign prose;
- raw provider responses.

## 98. Metrics

Useful local metrics include:

```text
MigrationDuration
CheckpointDuration
SchemaMigrationFailureCount
EncryptionMigrationDuration
RekeyFailureCount
RollbackCount
WrongKeyCount
SafeModeEntryCount
DowngradeRefusalCount
```

No remote telemetry is required.

## 99. Testing Strategy

The implementation requires:

```text
Installer Tests
Signature Tests
Data Separation Tests
Encrypted First-Launch Tests
Schema Migration Tests
Encryption Migration Tests
Rekey Tests
Checkpoint Tests
Rollback Tests
Downgrade Tests
Uninstall Tests
Channel Isolation Tests
Native Provider Tests
Safe Mode Tests
Fault Injection Tests
Privacy Tests
Architecture Tests
```

## 100. Installer Tests

Tests prove:

- binaries install correctly;
- existing encrypted data is untouched;
- installer does not open database;
- repair preserves data and keys;
- ordinary uninstall preserves data and keys.

## 101. Encrypted First-Launch Tests

Tests cover:

- compatible database;
- migration required;
- wrong key;
- missing key;
- unsupported cipher;
- incomplete prior migration;
- clean first run.

## 102. Schema Migration Tests

Apply every supported migration against encrypted databases.

## 103. Encryption Migration Tests

Test provider, cipher, KDF, page-size, and metadata-version changes where supported.

## 104. Rekey Fault Tests

Inject failure:

- before new key generation;
- after generation;
- before rekey;
- during rekey;
- after rekey;
- before key protection;
- after key protection;
- before old-key retirement.

At least one valid key/database pairing must survive.

## 105. Checkpoint Tests

Prove checkpoint can restore:

- database;
- schema;
- encryption profile;
- unresolved workflows;
- package bindings;
- matching key relationship.

## 106. Rollback Tests

Prove failed migration restores the validated encrypted checkpoint.

## 107. Downgrade Tests

Test:

- compatible older binary;
- incompatible schema;
- incompatible encryption format;
- compatible checkpoint restore;
- no silent rewrite.

## 108. Uninstall Tests

Prove:

- ordinary uninstall preserves encrypted data;
- ordinary uninstall preserves protected key records;
- complete removal requires explicit confirmation;
- one channel does not remove another.

## 109. Native Provider Tests

Validate every packaged native binary and supported architecture.

## 110. Safe Mode Tests

Test every recovery trigger and ensure no normal mutation occurs.

## 111. No-Plaintext Tests

Search migration and recovery directories for known content and key canaries.

## 112. Privacy Tests

Keys, recovery secrets, connection strings, and private Campaign content must not appear in installer or migration logs.

## 113. Architecture Tests

Architecture tests must reject:

- installer migration scripts;
- plaintext checkpoint creation;
- direct database-key access from installer;
- empty-database creation after key failure;
- deletion of key records during ordinary uninstall;
- unjournaled rekey;
- old key removal before new pairing validation;
- one shared data root across channels;
- automatic updater implementation in MVP;
- Rule Set reinterpretation during generic app migration.

## 114. Prohibited Patterns

### 114.1 Installer Owns Migration

Chronicle application owns data migration.

### 114.2 Plaintext Migration Copy

All durable migration state remains encrypted.

### 114.3 Rekey Without Checkpoint

Create and validate recovery first.

### 114.4 Delete Old Key Too Early

Retain until the new pairing validates.

### 114.5 Create Empty Database After Wrong Key

Enter recovery.

### 114.6 Ordinary Uninstall Deletes Key

Preserve recoverability.

### 114.7 Downgrade Rewrites Newer Data

Restore a compatible checkpoint instead.

### 114.8 Shared Stable and Preview Data Root

Keep channels isolated.

### 114.9 In-App Updater in MVP

Manual installation remains the update mechanism.

### 114.10 Migration Reinterprets Rule Set History

Preserve exact package versions.

## 115. Alternatives Considered

### Installer-Driven EF Migration

Rejected because installer context must not retrieve keys or own Domain recovery.

### Plaintext Checkpoint for Simplicity

Rejected because migration would create an exposure and contradict the encrypted foundation.

### Delete Protected Keys on Uninstall

Rejected because ordinary uninstall must not make preserved data unreadable.

### Automatic Encryption Upgrade on Open

Rejected because encryption migration requires explicit checkpoints and recovery.

### Support Arbitrary Downgrade

Rejected because older binaries cannot safely interpret newer schema and encryption formats.

### Build In-App Updater Now

Rejected because manual updates satisfy MVP while the migration foundation remains reusable.

## 116. Consequences

### Positive

- update and encryption architecture are aligned from MVP;
- migrations do not create plaintext copies;
- rekey and provider-format changes are recoverable;
- ordinary uninstall preserves encrypted data usability;
- manual updates remain simple;
- future in-app updates can reuse the same migration engine;
- downgrade behavior is honest and safe;
- release channels remain isolated.

### Negative

- first-launch migration is more complex;
- key and schema compatibility both require testing;
- installer cannot repair data directly;
- complete-removal UX must distinguish data from keys;
- native encrypted provider upgrades require careful qualification.

## 117. Risks

### New Binary Cannot Open Old Encryption Format

Mitigation:

- compatibility matrix;
- bundled migration-capable provider;
- checkpoint;
- Safe Mode.

### Rekey Completes but Key Publication Fails

Mitigation:

- journal;
- retain old and new references;
- validation before retirement;
- fault injection.

### Uninstall Leaves Sensitive Data

Mitigation:

- explicit complete-removal path;
- clear disclosure;
- preserve by default to avoid accidental loss.

### Downgrade Confuses Users

Mitigation:

- compatibility explanation;
- checkpoint discovery;
- no destructive automatic conversion.

## 118. Technology Spike

Before acceptance, implement:

1. signed Inno Setup package;
2. binary/data separation;
3. channel-specific data roots and key references;
4. encrypted first-launch inspection;
5. migration compatibility matrix;
6. encrypted pre-migration checkpoint;
7. schema migration on encrypted SQLite;
8. encryption-profile migration;
9. rekey journal;
10. protected-key publication protocol;
11. rollback;
12. Safe Mode;
13. compatible downgrade detection;
14. ordinary uninstall preservation;
15. explicit complete removal;
16. native provider validation;
17. fault injection;
18. no-plaintext tests;
19. architecture tests.

## 119. Spike Acceptance

The spike passes when:

- an update replaces binaries without touching encrypted data directly;
- Chronicle creates a validated encrypted checkpoint before migration;
- EF migrations run against the encrypted database;
- rekey succeeds without losing recoverability;
- every injected rekey failure preserves one usable database/key pairing;
- migration failure restores the checkpoint;
- wrong key never creates an empty replacement database;
- ordinary uninstall preserves data and protected key records;
- incompatible downgrade refuses to open data;
- Stable and Preview cannot open each other's data roots;
- no plaintext Campaign or key canary appears in migration staging or logs;
- no in-app updater is required.

## 120. Definition of Compliance

An implementation complies when:

- Inno Setup remains the Windows installer;
- MVP updates are manual;
- installer scripts never migrate Chronicle data;
- encrypted database and key records remain separate from binaries;
- first launch performs compatibility inspection;
- destructive migration requires a validated encrypted checkpoint;
- schema and encryption migrations are explicit;
- rekey and key publication are journaled;
- old key material remains until new pairing validation;
- normal mutation is blocked during unresolved migration;
- Safe Mode supports recovery;
- downgrade requires full compatibility or checkpoint restore;
- ordinary uninstall preserves encrypted data and protected keys;
- release channels isolate data and keys;
- no Core migration logic assumes Werewolf.

## 121. Review Triggers

Review this ADR if:

- in-app updating becomes official;
- background download is introduced;
- automatic migration scheduling is introduced;
- macOS or Linux packaging is added;
- enterprise-managed keys are introduced;
- encrypted SQLite provider changes;
- release channels share package caches;
- server-hosted authority replaces local migration;
- package migration becomes independently deployable.

## 122. Deferred Decisions

Later decisions may define:

- in-app update discovery;
- background download;
- delta packages;
- automatic restart;
- multi-platform installers;
- enterprise update rings;
- remote policy;
- hardware-backed key migration;
- automatic rollback orchestration;
- signed migration bundles.

## 123. Final Decision

Chronicle updates may replace binaries.

They may migrate schemas.

They may upgrade encryption.

They may rotate keys.

But they must always preserve a validated recovery path and a valid encrypted database/key pairing.

The MVP will use manual updates.

Its migration foundation will already be the one the complete product needs.
