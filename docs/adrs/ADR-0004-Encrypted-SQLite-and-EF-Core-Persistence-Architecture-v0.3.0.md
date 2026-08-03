---
id: ADR-0004
title: Encrypted SQLite and EF Core Persistence Architecture
status: Proposed
version: 0.3.0
owner: Chronicle Team
last_updated: 2026-08-02
category: Technology
supersedes:
  - ADR-0004@0.2.0
  - ADR-0004@0.1.0
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
  - ADR-0003
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
  - ADR-0026
  - ADR-0033
  - ADR-0034
  - ADR-0036
  - ADR-0038
  - ADR-0041
implements:
  - RFC-0033
related_to:
  - ADR-0027
  - ADR-0028
  - ADR-0037
  - ADR-0039
  - ADR-0042
  - ADR-0044
---

> **"The MVP may postpone features. It may not choose a foundation that must be replaced later."**

# Encrypted SQLite and EF Core Persistence Architecture

## 1. Status

**Proposed**

This ADR defines Chronicle's local persistence architecture using encrypted SQLite and EF Core.

This revision corrects ADR-0004 version 0.2.0.

The prior statement that Chronicle would use unencrypted SQLite in MVP is withdrawn.

The decision is:

- use SQLite as Chronicle's official local relational database;
- encrypt the database at rest from the MVP onward;
- use an encryption-capable SQLite distribution or provider compatible with EF Core;
- isolate encryption behind Chronicle-owned connection, key-management, migration, backup, restore, and recovery abstractions;
- use EF Core as the primary persistence mapper and migration framework;
- keep Domain entities independent from EF Core, SQLite, and encryption technology;
- store the database key separately from the database file;
- protect the installation-specific database key through Windows-native secret protection;
- never persist the plaintext database key in SQLite, configuration files, logs, crash reports, backup manifests, or portable exports;
- include encrypted persistence in the first schema, migration, backup, restore, test, and recovery implementation;
- avoid a future plaintext-to-encrypted database rewrite as normal product evolution;
- preserve Campaign as the Domain aggregate root;
- store durable Application workflow records in separate persistence tables;
- use short transactions and optimistic concurrency;
- preserve append-only Dice evidence and correction history;
- use exact Rule Set package and contract version bindings;
- keep provider-native state nonauthoritative;
- keep all technical identifiers in English;
- avoid Werewolf-specific Core columns.

## 2. Context

Chronicle stores private Campaign content that may include:

- prose;
- Characters;
- personal history;
- memories;
- decisions;
- mechanical state;
- Character Knowledge;
- provider context;
- unresolved workflows;
- Dice evidence.

Relying only on operating-system account protection would leave the primary database readable if the file were copied from an unlocked or compromised profile.

Adding encryption after the persistence model is mature would affect:

- connection creation;
- startup;
- migrations;
- backup;
- restore;
- Safe Mode;
- diagnostics;
- automated tests;
- data-root movement;
- key recovery;
- packaging.

That would create architectural rework.

## 3. Foundational Principle

Chronicle follows:

```text
MVP simplification may defer product features.
MVP simplification may not require replacing the persistence foundation later.
```

## 4. Technology Selection

Chronicle uses:

```text
Database Model
    SQLite

Database Protection
    encrypted SQLite from first production schema

ORM
    EF Core

Migration Ownership
    Chronicle application

Key Protection
    Windows-native user-scoped secret protection

Primary Runtime
    local desktop application
```

## 5. Encryption-Capable SQLite Distribution

The exact implementation must provide:

- transparent database encryption;
- EF Core compatibility;
- supported migrations;
- WAL compatibility or a documented alternative;
- parameterized key application;
- integrity validation;
- backup and restore support;
- maintained native binaries;
- licensing compatible with Chronicle distribution.

## 6. Candidate Technology Validation

A technology spike must evaluate suitable implementations such as an approved SQLCipher-compatible distribution or equivalent encrypted SQLite provider.

No specific library becomes final merely by appearing as a candidate.

## 7. No Plaintext Production Database

Stable Chronicle builds must not create a plaintext authoritative database.

## 8. Development Databases

Development and test profiles may use deterministic test keys.

They must exercise the same encrypted connection path unless a narrowly scoped test explicitly replaces the persistence provider.

## 9. Domain Independence

`Chronicle.Domain` must not reference:

- EF Core;
- SQLite;
- encryption libraries;
- Windows secret APIs;
- database key types;
- migration APIs.

## 10. Persistence Boundary

Encryption is an Infrastructure and Persistence responsibility.

## 11. Chronicle DbContext

The recommended context remains:

```text
ChronicleDbContext
```

## 12. Encrypted Connection Factory

All database access goes through a Chronicle-owned abstraction such as:

```text
IChronicleConnectionFactory
```

## 13. Connection Factory Responsibilities

The factory owns:

- data-root resolution;
- encrypted provider selection;
- key retrieval;
- connection-string construction;
- connection opening;
- encryption activation;
- required SQLite configuration;
- schema compatibility checks;
- safe failure translation.

## 14. No Ad Hoc Connections

Application, Presentation, Rule Set packages, and provider adapters may not construct SQLite connections directly.

## 15. DbContext Factory

`ChronicleDbContext` instances are created through a factory using the encrypted connection boundary.

## 16. Short-Lived Context

Use one short-lived DbContext per bounded operation or transaction phase.

## 17. No Global Context

A singleton tracked DbContext is prohibited.

## 18. Database Key

Each Chronicle installation data root has a high-entropy database key.

## 19. Key Generation

The key is generated locally using an operating-system cryptographic random source.

## 20. Key Scope

The MVP uses an installation/data-root-specific key.

It is not derived from:

- username;
- machine name;
- Campaign name;
- provider credential;
- hardcoded application secret.

## 21. Key Storage

The plaintext database key is never stored in the database directory.

## 22. Windows Protection

The protected key record uses a Windows-native mechanism such as:

```text
Windows Credential Manager
```

or a DPAPI-protected local secret store selected by the security implementation ADR.

## 23. Key Identifier

SQLite may store only an opaque key-reference identifier where needed.

## 24. Key Material Prohibitions

Plaintext key material must not appear in:

- `appsettings`;
- environment files committed to the repository;
- registry values without protection;
- logs;
- traces;
- exception messages;
- telemetry;
- backup manifests;
- portable Campaign exports;
- provider requests.

## 25. Key Retrieval

Key retrieval occurs immediately before opening the database.

## 26. Memory Lifetime

Plaintext key material remains in memory only as long as required by the provider API.

## 27. Memory Handling

Where practical, key material uses bounded-lifetime buffers and is cleared after use.

## 28. Locked or Missing Key

If the key cannot be retrieved:

- the database is not opened;
- Chronicle does not create a replacement database automatically;
- normal mutation remains blocked;
- recovery UI explains the condition safely;
- the original data remains unchanged.

## 29. No Silent Data Reset

Missing key state must never trigger automatic creation of an empty Chronicle database over the existing data root.

## 30. Key and Data Binding

The protected key record includes enough metadata to bind it to the intended Chronicle data root without exposing private user information.

## 31. Data Root Identity

Use an opaque:

```text
DataRootId
```

## 32. Moving the Data Root

Moving an installation data root requires an explicit supported workflow that preserves or rebinds the protected key safely.

## 33. Copying Only the Database File

Copying only the encrypted database file is not considered a complete recoverable backup.

## 34. Recovery Material

Backup and restore design must provide an approved path for restoring encrypted data without exposing the active database key.

## 35. Key Rotation

Key rotation is an architectural capability from the start.

## 36. MVP Key Rotation Scope

The MVP must at least define and test the internal rotation workflow, even if the user-facing rotation UI is minimal or administrative.

## 37. Rotation Operation

Key rotation uses:

- OperationId;
- exclusive database access;
- verified pre-rotation checkpoint;
- new key generation;
- encrypted database rekey;
- integrity validation;
- protected-key publication;
- rollback recovery.

## 38. Rotation Ordering

The workflow must avoid a state where:

- the database uses the new key;
- but only the old protected key is available;

or the inverse without recovery evidence.

## 39. Rotation Journal

A durable journal records the rotation phase without recording plaintext key material.

## 40. Rotation Failure

Failure enters `RecoveryRequired`.

## 41. No Routine Rotation Requirement

Automatic scheduled rotation is not required in MVP.

The foundation must support it without changing the database architecture.

## 42. Database Schema

The logical schema includes:

```text
Domain Tables
Application Workflow Tables
Narrative Intelligence Operational Tables
Dice Evidence Tables
History and Correction Tables
Recovery and Artifact Tables
```

## 43. Required Workflow Tables

```text
OperationRecords
WorkItems
NarrativeTurns
NarrativeAcceptedOutputs
NarrativeEventApplications
ProviderAttempts
NarrativeResponseStaging
```

## 44. Required Dice Tables

```text
DiceRolls
RandomEvidenceItems
DiceResolutionStages
DiceResolutionEvidenceUse
DiceCorrections
```

## 45. Campaign Root

Campaign remains the aggregate root for fictional and mechanical truth.

## 46. Application Records

Narrative Turns, Provider Attempts, Operations, and Work Items remain Application records, not Domain aggregate roots.

## 47. Rule Set-Neutral Storage

No Core table contains Werewolf-specific mechanics.

## 48. Typed Payloads

Package-specific payloads use versioned and bounded contracts.

## 49. No Whole-Campaign Blob

Campaigns are not persisted as one opaque JSON document.

## 50. Encryption Does Not Replace Authorization

Database encryption protects the file at rest.

It does not replace:

- Application authorization;
- process isolation;
- safe logging;
- secret management;
- input validation;
- backup protection.

## 51. SQLite Configuration

The encrypted provider profile must validate:

```text
Foreign Keys
Busy Timeout
Durability Settings
WAL or approved journal mode
Secure temporary storage behavior
```

## 52. WAL Compatibility

WAL is permitted only if the selected encrypted provider protects and manages all related database files correctly.

## 53. Companion Files

The architecture must account for:

- WAL;
- shared memory;
- rollback journal;
- temporary files;
- staged migration files.

## 54. No Plaintext Temporary Database

Migration, restore, integrity-check, and backup workflows must not create unprotected plaintext copies of authoritative data.

## 55. Temporary Storage

Sensitive temporary files must be:

- encrypted by the same mechanism;
- stored in an encrypted container;
- or avoided.

## 56. SQLite Temporary Behavior

Provider configuration must prevent sensitive temporary data from being written as unencrypted persistent files where supported.

## 57. Transactions

Transactions remain short.

## 58. External Wait Prohibition

Do not hold a transaction while waiting for:

- provider calls;
- player input;
- Dice release;
- filesystem publication;
- long Rule Set computation;
- UI confirmation.

## 59. Optimistic Concurrency

Use explicit version columns.

## 60. Unique Constraints

Database uniqueness remains a correctness boundary for:

- Operation identity;
- accepted Narrative output;
- Provider Attempt number;
- Narrative Events;
- Dice origin;
- random evidence sequence;
- resolution stages;
- continuation identity.

## 61. Random Evidence

Committed Dice evidence remains append-only.

## 62. Exact Rule Set Binding

Campaigns and Dice Rolls preserve exact package and contract versions.

## 63. Credentials

Provider credentials remain in Windows Credential Manager.

## 64. Credential Separation

The database encryption key and provider credentials are separate secrets.

Compromise or rotation of one must not require redesign of the other.

## 65. Backup Architecture

Installation backups must preserve encrypted data without requiring plaintext database extraction.

## 66. Backup Key Strategy

Backup encryption and database encryption are separate concerns.

## 67. Mandatory Backup Protection

Published Chronicle backups must be encrypted from MVP onward.

## 68. Backup Protection Design

The backup ADR must define one of these approved patterns:

```text
A backup-specific encryption key protected for the current user
```

or:

```text
A user-supplied recovery secret used through a modern KDF
```

or another reviewed design providing portable recovery.

## 69. Database Key Exclusion from Backup Manifest

The active database key must not appear in plaintext in the backup artifact or manifest.

## 70. Local-Only Backup Caveat

A backup encrypted only with the current Windows user protection is not portable to another machine or account.

## 71. Portability Requirement

Because restore and disaster recovery may require another installation, the final backup design must include an explicit portable recovery-key path.

## 72. MVP Backup Requirement

The MVP must implement a recoverable encrypted backup path, not merely copy the encrypted database and assume the original secret store survives.

## 73. Restore Architecture

Restore must:

- inspect artifact metadata before activation;
- obtain or recover required key material;
- stage the encrypted database;
- validate decryption;
- validate SQLite integrity;
- create a pre-restore checkpoint;
- publish atomically;
- preserve rollback.

## 74. No Plaintext Restore Staging

Restore staging must remain encrypted.

## 75. Backup and Restore Key Separation

Restoring a backup may produce a new installation database key after successful decryption and import of the protected snapshot.

## 76. Rekey on Restore

The preferred architecture permits:

```text
backup key
    ↓ decrypt staged backup
new installation database key
    ↓ re-encrypt active database
```

without exposing plaintext durable storage.

## 77. Portable Campaign Export

Portable Campaign export remains independent from database encryption and SQLite schema.

## 78. Export Encryption

Portable export confidentiality is governed by RFC-0034 and ADR-0027.

It must not reuse the active database key.

## 79. Migration Architecture

Every migration opens the encrypted database through the same connection factory.

## 80. Pre-Migration Checkpoint

A verified encrypted checkpoint exists before destructive migration.

## 81. No Plaintext Migration Copy

Migration tools must not decrypt into an unprotected database file.

## 82. Migration Key Compatibility

Schema migration must not silently alter key protection or encryption parameters.

## 83. Encryption Parameter Migration

Any change to:

- cipher;
- KDF;
- page size;
- provider format;
- key derivation parameters;

requires an explicit security migration plan and checkpoint.

## 84. First Encrypted Schema

The first production database schema is encrypted.

## 85. No Planned Plaintext-to-Encrypted Product Migration

Plaintext-to-encrypted migration exists only for development prototypes or predecision builds, not as a normal post-MVP product milestone.

## 86. Legacy Plaintext Detection

If a legacy prototype database is detected:

- do not open it as normal Stable data;
- require an explicit conversion workflow;
- create a backup;
- encrypt into a new database;
- validate;
- preserve rollback evidence.

## 87. Startup

Startup order:

1. resolve data root;
2. read nonsecret database metadata;
3. resolve protected key;
4. open encrypted connection;
5. validate encryption;
6. validate schema compatibility;
7. inspect migration and recovery state;
8. start normal Application services.

## 88. Encryption Validation

A successful file open is insufficient.

Chronicle validates:

- expected schema marker;
- known data-root identity;
- integrity;
- encryption-provider compatibility.

## 89. Wrong Key

Wrong key produces a typed recovery error.

It must not be reported as generic database corruption without inspection.

## 90. Error Separation

Distinguish:

```text
KeyUnavailable
WrongKey
UnsupportedCipher
DatabaseCorrupt
SchemaUnsupported
RecoveryRequired
```

## 91. Safe Mode

Safe Mode can:

- inspect protected-key metadata;
- inspect database compatibility;
- restore encrypted backup;
- retry key retrieval;
- run approved key recovery;
- collect redacted diagnostics.

## 92. Safe Mode Prohibition

Safe Mode must not display plaintext key material.

## 93. Logging

Logs may include:

- DataRootId;
- encryption provider version;
- cipher compatibility status;
- key-reference ID;
- schema version;
- OperationId;
- safe error code.

They must not include:

- plaintext key;
- derived key;
- password;
- recovery secret;
- connection string containing key material;
- provider credentials;
- raw SQL parameters with user content.

## 94. Connection String Redaction

Connection strings are always redacted before logging.

## 95. EF Core Sensitive Logging

Sensitive-data logging is Development-only and must still exclude key material.

## 96. Crash Reports

Crash diagnostics must redact:

- key references where unnecessary;
- full database paths where private;
- connection strings;
- encrypted secret payloads;
- Campaign content.

## 97. Database File Disclosure

Documentation must state that Chronicle encrypts the database at rest but unlocked application access can still expose content to the current process and user session.

## 98. Key Loss Disclosure

Documentation must state that loss of both protected key access and recovery material may make encrypted data unrecoverable.

## 99. Recovery UX

Chronicle must make backup and recovery-key creation understandable before the user accumulates significant Campaign history.

## 100. First-Run Requirement

The first-run flow must establish:

- encrypted database;
- protected key;
- data-root identity;
- backup/recovery readiness indicator.

## 101. No Mandatory Password on Every Launch

The MVP does not require a user-entered database password on every start unless the selected security profile explicitly chooses that model.

## 102. User-Scoped Unlock

The default desktop profile may unlock through the current Windows user secret protection.

## 103. Future Lock Profiles

Future security profiles may add:

- launch password;
- hardware-backed key;
- biometric-gated secret release;
- enterprise key escrow.

The current abstraction must allow them.

## 104. Test Strategy

The implementation requires:

```text
Encrypted Open Tests
Wrong-Key Tests
Missing-Key Tests
No-Plaintext Tests
Migration Tests
Backup Tests
Restore Tests
Rekey Tests
Crash Recovery Tests
WAL Tests
Temporary File Tests
Logging Tests
Architecture Tests
Cross-System Persistence Tests
```

## 105. Encrypted Open Tests

Tests prove:

- correct key opens;
- missing key fails;
- wrong key fails;
- plaintext SQLite tools cannot read authoritative content.

## 106. No-Plaintext Tests

Search controlled test environments for known content canaries in:

- database file;
- WAL;
- journal;
- temporary files;
- migration staging;
- backup staging;
- logs.

Plaintext canaries must not appear outside explicitly allowed in-memory or test-controlled diagnostic locations.

## 107. Migration Tests

Tests apply migrations to encrypted databases from every supported schema version.

## 108. Rekey Tests

Tests inject failure:

- before rekey;
- during rekey;
- after database rekey;
- before protected-key publication;
- after protected-key publication;
- during validation.

Recovery must preserve at least one valid key and database pairing.

## 109. Backup Tests

Tests prove:

- backup artifact is encrypted;
- active database key is not exposed;
- recovery material restores on a clean installation;
- unresolved workflows survive;
- Dice evidence survives;
- wrong recovery secret fails safely.

## 110. Restore Tests

Tests restore to:

- same Windows account;
- clean Chronicle installation;
- replacement data root;
- new installation database key.

## 111. WAL Tests

Tests prove related journal files do not expose plaintext canaries.

## 112. Crash Tests

Inject failure during:

- connection startup;
- migration;
- evidence commit;
- key rotation;
- backup;
- restore;
- checkpoint publication.

## 113. Logging Tests

Synthetic secrets must not appear in logs, errors, diagnostics, or crash artifacts.

## 114. Architecture Tests

Architecture tests must reject:

- direct SQLite connection creation outside Persistence;
- key retrieval outside approved Infrastructure;
- key in configuration models;
- credentials in SQLite;
- plaintext backup implementation;
- unencrypted migration staging;
- connection-string logging;
- Rule Set or provider adapter access to encryption APIs;
- Domain reference to encryption technology;
- plaintext production database creation.

## 115. Persistence Schema Tests

All tables and lifecycle constraints from ADR-0004 v0.2.0 remain required, including:

- Operations;
- Work Items;
- Narrative Turns;
- Provider Attempts;
- response staging;
- Dice evidence;
- corrections;
- package bindings.

## 116. Cross-System Test

A synthetic non-Werewolf Rule Set persists and restores complex Dice evidence through the encrypted database without schema changes.

## 117. Error Model

Recommended errors:

```text
persistence.database-unavailable
persistence.database-locked
persistence.key-unavailable
persistence.key-reference-invalid
persistence.wrong-key
persistence.cipher-unsupported
persistence.encryption-initialization-failed
persistence.encryption-validation-failed
persistence.rekey-failed
persistence.rekey-outcome-unknown
persistence.plaintext-database-prohibited
persistence.foreign-key-violation
persistence.unique-constraint-violation
persistence.concurrency-conflict
persistence.schema-unsupported
persistence.migration-required
persistence.migration-failed
persistence.integrity-check-failed
persistence.recovery-required
```

## 118. Data Preservation State

Results should state:

```text
AuthoritativeDataUnchanged
EncryptedDatabaseCreated
EncryptedDatabaseOpened
KeyProtected
KeyUnavailable
WrongKey
CheckpointCreated
RekeyNotStarted
RekeyCommitted
RekeyOutcomeUnknown
MigrationNotApplied
MigrationApplied
BackupEncrypted
RestoreValidated
RecoveryRequired
```

## 119. Metrics

Useful local metrics include:

```text
EncryptedDatabaseOpenDuration
KeyRetrievalDuration
EncryptionInitializationFailureCount
WrongKeyCount
RekeyDuration
MigrationDuration
BackupEncryptionDuration
RestoreDecryptionDuration
RecoveryRequiredCount
```

No remote telemetry is required.

## 120. Prohibited Patterns

### 120.1 Plaintext MVP Database

Encryption is foundational, not deferred.

### 120.2 Key Beside Database

Store the protected key separately.

### 120.3 Hardcoded Application Key

Generate installation-specific high-entropy keys.

### 120.4 Database Key in Backup Manifest

Use a reviewed backup recovery design.

### 120.5 Plaintext Migration Staging

Keep all durable staging encrypted.

### 120.6 Connection String Logging

Always redact.

### 120.7 Key Recovery Through Provider Credentials

Keep secret domains separate.

### 120.8 Silent Empty Database on Key Failure

Preserve data and enter recovery.

### 120.9 Encryption Library Types in Domain

Keep encryption in Infrastructure and Persistence.

### 120.10 Add Encryption After MVP

The first production schema is already encrypted.

## 121. Alternatives Considered

### Plain SQLite for MVP

Rejected because encryption would later affect every persistence lifecycle and create architectural rework.

### Rely Only on BitLocker or OS Account Protection

Rejected as the sole control because copied database files would remain readable outside Chronicle when the underlying filesystem protection is absent or bypassed.

### User Password as Direct Database Key

Rejected as the default because weak passwords, KDF design, password resets, and daily UX require a broader security decision.

### One Hardcoded Product Key

Rejected because compromise would expose every installation.

### Store Key in the Database

Rejected because it defeats the protection boundary.

### Encrypt Only Campaign Prose Columns

Rejected because operational records, Dice evidence, memories, and metadata also contain private information and field-level encryption would create query and migration complexity.

### Replace SQLite With a Server Database

Rejected because local encrypted SQLite remains appropriate for the desktop local-first architecture.

## 122. Consequences

### Positive

- no post-MVP encryption rewrite;
- authoritative local data is encrypted at rest;
- connection and recovery architecture are correct from the start;
- backup and restore become security-aware;
- key rotation remains possible;
- provider and Rule Set boundaries stay clean;
- SQLite local-first simplicity remains;
- future security profiles can reuse the abstraction.

### Negative

- native distribution becomes more complex;
- key recovery requires deliberate UX;
- backup design must include portable encrypted recovery;
- migrations and restore need additional testing;
- native provider licensing and maintenance must be reviewed;
- loss of all key recovery material may make data unrecoverable.

## 123. Risks

### Key Loss

Mitigation:

- recovery-ready backup;
- protected key record;
- explicit user guidance;
- tested restore on clean installation.

### Native Provider Supply-Chain Risk

Mitigation:

- approved source;
- pinned versions;
- hashes;
- SBOM;
- signature verification where available;
- upgrade testing.

### Plaintext Leakage Through Temporary Files

Mitigation:

- encrypted provider configuration;
- canary tests;
- protected staging;
- no plaintext database copies.

### Rekey Crash

Mitigation:

- checkpoint;
- journal;
- fault injection;
- preserve old and new key references until validation completes.

### Backup Not Portable

Mitigation:

- backup-specific recovery key design;
- clean-install restore test.

## 124. Technology Spike

Before acceptance, implement:

1. encryption-capable SQLite provider evaluation;
2. EF Core integration;
3. encrypted connection factory;
4. Windows-protected key store;
5. DataRootId binding;
6. first-run encrypted database creation;
7. wrong-key and missing-key recovery;
8. encrypted migrations;
9. WAL and temporary-file validation;
10. encrypted backup proof of concept;
11. clean-install restore;
12. rekey workflow and journal;
13. key-loss recovery UX;
14. Stable logging redaction;
15. plaintext canary tests;
16. architecture tests;
17. license and native-binary review.

## 125. Spike Acceptance

The spike passes when:

- Chronicle creates no plaintext production database;
- EF Core migrations run on the encrypted database;
- the database cannot be opened without the correct key;
- known content canaries do not appear in the database, WAL, journal, or staging files;
- the key is absent from files and logs;
- a clean installation can restore an encrypted backup using approved recovery material;
- rekey succeeds;
- every injected rekey failure preserves a recoverable state;
- unresolved Operations, Narrative Turns, and Dice evidence survive backup and restore;
- no Domain, Rule Set, provider adapter, or Presentation project accesses encryption implementation details.

## 126. Definition of Compliance

An implementation complies when:

- SQLite remains the official local relational database;
- the first production schema is encrypted at rest;
- all database access uses Chronicle's encrypted connection factory;
- EF Core remains the primary mapper and migration system;
- keys are installation-specific, high entropy, and protected separately;
- plaintext keys never enter files, logs, backups, exports, or provider requests;
- backup and restore include encrypted portable recovery;
- migrations and temporary storage do not create plaintext authoritative copies;
- key rotation is supported by the architecture and tested;
- key failure never causes silent data replacement;
- canonical persistence tables and lifecycles remain enforced;
- no Core schema is specific to Werewolf.

## 127. Review Triggers

Review this ADR if:

- Chronicle supports macOS or Linux;
- enterprise-managed keys are introduced;
- hardware-backed keys become required;
- cloud synchronization is introduced;
- server-hosted authority is introduced;
- a new encrypted SQLite provider is selected;
- backup password or recovery-key UX changes;
- multi-user Windows profiles must share one data root;
- compliance requirements impose new cryptographic parameters.

## 128. Deferred Decisions

Later decisions may define:

- exact encrypted SQLite distribution after spike approval;
- hardware-backed secret storage;
- launch-password security profile;
- enterprise key escrow;
- automated scheduled key rotation;
- cross-platform secret stores;
- multi-user shared data roots;
- cloud key management;
- cryptographic parameter upgrade policy.

## 129. Final Decision

Chronicle will begin with encrypted persistence.

Not after MVP.

Not after users have created years of Campaign history.

The first production database, its migrations, its backups, its restores, and its recovery workflows will all understand encryption.

The MVP may be smaller than the complete product.

Its foundation will not be temporary.
