---
id: ADR-0023
title: Database Connection, Transaction, and Concurrency Configuration
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
  - ADR-0007
  - ADR-0010
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
  - RFC-0006
  - RFC-0017
  - RFC-0018
  - RFC-0019
  - RFC-0033
  - RFC-0034
  - RFC-0036
  - RFC-0040
  - RFC-0042
---

> **"SQLite may serialize writers. Chronicle must still make every write boundary intentional, short, and recoverable."**

# Database Connection, Transaction, and Concurrency Configuration

## 1. Status

**Proposed**

This ADR defines Chronicle's concrete SQLite connection, transaction, locking, concurrency, retry, and EF Core configuration.

The decision is:

- use one authoritative SQLite database per installation;
- use one short-lived `DbContext` per command, query scope, Work Item attempt, migration stage, or recovery operation;
- enable foreign-key enforcement on every connection;
- prefer WAL journal mode for normal operation after validation;
- configure a bounded busy timeout;
- use explicit write transactions for authoritative mutations;
- avoid holding database transactions across provider calls, file dialogs, user decisions, delays, or other external waits;
- use optimistic concurrency through explicit entity or aggregate version columns;
- coordinate in-process Campaign mutations through the Application mutation coordinator;
- rely on database constraints as the final duplicate-effect safeguard;
- map known lock, busy, unique, foreign-key, disk-full, and corruption failures into typed Application outcomes;
- avoid blind retry of complete Domain commands;
- permit bounded retry only for explicitly safe database operations;
- use consistency-safe SQLite backup APIs rather than copying the active database file;
- treat WAL and SHM files as persistence-owned runtime artifacts;
- validate PRAGMA settings and connection behavior in real SQLite integration tests.

The decision becomes **Accepted** after a persistence spike proves:

- foreign-key enforcement;
- WAL activation and persistence;
- bounded busy handling;
- concurrent reads during short writes;
- one writer per transaction;
- optimistic concurrency;
- duplicate OperationId protection;
- rollback on failure;
- no open transaction during provider wait;
- crash recovery;
- consistency-safe backup;
- disk-full and database-lock error mapping;
- migration behavior under the selected configuration.

## 2. Context

Chronicle uses SQLite through EF Core for:

- Campaign aggregates;
- Sessions, Acts, and Scenes;
- Messages;
- Dice Rolls;
- Memories;
- Relationships;
- Character Knowledge;
- progression;
- Preferences;
- Operation Records;
- Work Items;
- Audit Records;
- package metadata;
- application configuration metadata.

The MVP is a local single-user desktop application, but concurrent activity still exists inside one process:

- UI commands;
- read queries;
- Work Item execution;
- backup;
- Rule Knowledge indexing;
- startup recovery;
- migration;
- diagnostics.

SQLite allows many readers but only one writer at a time.

Without explicit configuration, Chronicle risks:

- foreign keys not being enforced;
- long write locks;
- indefinite busy waits;
- hidden transaction scopes;
- duplicate effects after retries;
- stale aggregate updates;
- active database copies producing inconsistent backups;
- worker and UI contention;
- improper WAL cleanup;
- low-level SQLite errors leaking to Presentation.

ADR-0004 selected EF Core with SQLite and one database per installation.

ADR-0013 defines short Application transactions.

ADR-0014 defines Work Item leases and at-least-once execution.

ADR-0015 defines no-tracking read projections.

This ADR defines the concrete database behavior.

## 3. Decision Drivers

The database configuration prioritizes:

1. transactional integrity;
2. predictable local concurrency;
3. short writer lifetime;
4. explicit optimistic concurrency;
5. duplicate-effect protection;
6. responsive reads;
7. safe backup;
8. typed recovery;
9. deterministic integration testing;
10. low operational complexity;
11. migration safety;
12. clear Infrastructure ownership.

## 4. Decision Summary

Chronicle will use:

```text
Database
    one SQLite database per installation

ORM
    EF Core

Connection Ownership
    one short-lived context per operation scope

Journal Mode
    WAL preferred

Foreign Keys
    enabled on every connection

Busy Timeout
    bounded and configurable

Write Transactions
    explicit
    short
    local only

Read Queries
    AsNoTracking by default

Concurrency
    Application Campaign mutation coordinator
    optimistic version columns
    unique constraints
    SQLite writer serialization

Retry
    no blind command retry
    bounded safe infrastructure retry only

Backup
    SQLite backup API or equivalent consistent snapshot

Sidecar Files
    owned by persistence
```

## 5. Database Identity

Chronicle uses one authoritative database file per selected data profile.

Initial path:

```text
data/chronicle.db
```

The path is resolved through `IDataPathResolver`.

## 6. One Database per Installation

The MVP does not use one database per Campaign.

### Rationale

One installation database simplifies:

- atomic cross-Campaign metadata;
- configuration references;
- Operation Records;
- Work Items;
- backup;
- migration;
- package inventory;
- application startup.

## 7. Database File Ownership

Only the persistence subsystem may:

- open the database for mutation;
- configure SQLite pragmas;
- create migrations;
- manage WAL checkpoints;
- invoke backup APIs;
- inspect SQLite error codes;
- coordinate connection behavior.

## 8. DbContext Lifetime

`ChronicleDbContext` is scoped and short-lived.

It MUST NOT survive:

- provider calls;
- player think time;
- UI navigation;
- Work Item retry delay;
- file picker interaction;
- application shutdown boundary.

## 9. Context Creation

Contexts are created through the scoped operation boundary or approved factory.

Each command or Work Item attempt receives a fresh context.

## 10. Connection Pooling

SQLite connection pooling MAY be enabled through the provider default when validated.

Pooling must not allow per-connection PRAGMA assumptions to become inconsistent.

## 11. Connection Initialization

Every opened connection MUST establish required settings.

Recommended required PRAGMAs:

```text
foreign_keys = ON
busy_timeout = configured bounded value
```

WAL mode is configured and validated at database initialization.

## 12. Foreign Keys

Foreign-key enforcement is mandatory.

Chronicle MUST NOT depend only on EF navigation correctness.

## 13. Foreign-Key Verification

Startup or integration tests SHOULD verify:

```text
PRAGMA foreign_keys
```

returns enabled for active connections.

## 14. Journal Mode

Chronicle SHOULD use:

```text
journal_mode = WAL
```

for normal operation.

### Rationale

WAL generally supports:

- concurrent readers during writes;
- reduced reader-writer blocking;
- suitable local desktop behavior;
- recovery after process interruption.

## 15. WAL Validation

The implementation spike MUST prove WAL behavior on the packaged Windows SQLite runtime.

## 16. WAL Fallback

If WAL cannot be enabled safely on a supported local filesystem:

- startup reports degraded or blocked storage capability;
- Chronicle does not silently assume WAL behavior;
- fallback journal mode requires documented validation.

## 17. WAL Sidecars

The persistence subsystem owns:

```text
chronicle.db-wal
chronicle.db-shm
```

Other modules MUST NOT delete, copy, archive, rename, or inspect them as temporary files.

## 18. WAL Checkpointing

Chronicle MAY allow SQLite automatic checkpoints initially.

Manual checkpointing may be used before:

- selected maintenance;
- shutdown;
- backup planning;
- database-size diagnostics.

Checkpoint policy requires measurement.

## 19. Checkpoint Safety

Manual checkpoint operations must be:

- bounded;
- cancellable where possible;
- safe with active readers;
- Infrastructure-owned.

## 20. Synchronous Mode

The initial production setting SHOULD use a durability-oriented mode appropriate for WAL.

The exact `synchronous` value must be validated in the persistence spike.

The selection should favor committed Campaign integrity over marginal write-speed gains.

## 21. Busy Timeout

Every connection uses a bounded busy timeout.

The exact initial value is configurable and determined by testing.

A conceptual initial range is:

```text
2 to 10 seconds
```

The final value must not be copied from this ADR without measurement.

## 22. No Indefinite Wait

Chronicle MUST NOT wait indefinitely for a database lock.

After the bounded timeout, Infrastructure maps the failure into a typed busy or unavailable result.

## 23. Busy Error

A busy database may map to:

```text
persistence.database-busy
```

with:

```text
RetryClassification = RetrySameOperationAfterDelay
```

only when the Application workflow classifies retry as safe.

## 24. Locked Error

A locked database caused by another process or unsupported ownership state may map to:

```text
persistence.database-locked
```

and usually requires startup or user recovery rather than automatic command retry.

## 25. Transaction Boundary

Authoritative mutation commands use explicit transactions controlled by the Application transaction behavior.

## 26. Transaction Start

A transaction begins only after:

- input validation;
- idempotency inspection;
- required provider or external output is available;
- Campaign mutation coordination is acquired;
- the command is ready to load and change authoritative state.

## 27. Transaction End

The transaction ends immediately after:

- Domain changes are persisted;
- required history is appended;
- Operation Record result is stored;
- required Work Items are enqueued;
- constraints are validated;
- commit succeeds or rollback completes.

## 28. No Transaction Across External Work

A database transaction MUST NOT remain open across:

- Narrative Intelligence calls;
- credential retrieval beyond immediate request preparation;
- file dialogs;
- user confirmation;
- delay or backoff;
- external file parsing;
- long Rule Knowledge indexing;
- desktop notification;
- UI rendering.

## 29. Transaction Isolation

SQLite transaction behavior should provide a consistent local write boundary.

The exact EF Core and SQLite transaction mode is an implementation concern.

Chronicle SHOULD avoid escalation to an early exclusive lock unless required.

## 30. Deferred Versus Immediate Write

For commands ready to mutate state, an immediate write transaction MAY reduce late write-lock surprises.

The spike should compare:

```text
deferred transaction
immediate transaction
```

under Chronicle's contention model.

## 31. Transaction Mode Decision

The persistence adapter may select immediate transactions for Campaign mutations when:

- the mutation coordinator is already held;
- the transaction remains short;
- lock acquisition failure is easier to surface early.

This must be proven through integration tests.

## 32. SaveChanges Ownership

Handlers do not call arbitrary global `SaveChanges`.

The Unit of Work or transaction behavior controls the final persistence flush and commit.

## 33. SaveChanges Batching

Chronicle SHOULD persist one logical authoritative command through as few `SaveChanges` calls as practical.

Multiple calls inside one transaction are allowed only when required for:

- generated relational values;
- explicit stage validation;
- migration;
- complex graph ordering.

## 34. Nested Transactions

Nested business transactions are prohibited.

SQLite savepoints MAY support narrow infrastructure or migration behavior.

## 35. Savepoints

Savepoints are not used to hide partial command semantics.

If a command is atomic, its authoritative intention still commits or rolls back as one outcome.

## 36. Command Concurrency

Chronicle coordinates Campaign-mutating commands by `CampaignId` in process.

This reduces avoidable conflicts but is not the final integrity mechanism.

## 37. Database Final Safeguards

Final safeguards include:

- optimistic concurrency versions;
- foreign keys;
- unique constraints;
- check constraints;
- transaction boundaries;
- OperationId uniqueness.

## 38. Aggregate Version

Campaign and selected aggregate roots SHOULD carry explicit version columns.

Example:

```text
AggregateVersion
```

## 39. Version Increment

A successful authoritative mutation increments the relevant version exactly once according to aggregate policy.

## 40. EF Core Concurrency Token

Version columns SHOULD be configured as concurrency tokens.

An update includes the expected prior version in its predicate.

## 41. Concurrency Failure

If zero rows are affected due to version mismatch:

- transaction rolls back;
- Application receives a typed stale or concurrency result;
- no blind Domain retry occurs;
- the caller refreshes and decides whether to retry.

## 42. Child Entity Versions

Frequently updated child records MAY have their own version when independent conflict detection is needed.

The MVP should avoid versioning every row without evidence.

## 43. Sequence Constraints

Explicit parent-scoped sequence values SHOULD be protected by unique constraints.

Examples:

```text
CampaignId + SessionSequence
SessionId + ActSequence
ActId + SceneSequence
SceneId + MessageSequence
DiceRollId + DiePosition
```

## 44. OperationId Constraints

Once-only effects SHOULD have unique OperationId constraints.

Examples:

```text
DiceRoll.OperationId
Advancement.OperationId
Finalization.SessionId or OperationId
Backup.OperationId
```

## 45. Known Unique Violation

Known unique violations map to idempotent replay or semantic conflict.

They do not automatically become internal errors.

## 46. Unknown Unique Violation

An unknown unique-constraint failure maps to an internal or integrity error after safe logging.

## 47. Check Constraints

Check constraints SHOULD protect simple relational invariants such as:

- nonnegative sequence;
- positive version;
- valid status range where represented numerically;
- bounded boolean combinations;
- valid Dice position.

Complex Domain logic remains in Domain and Rule Set code.

## 48. Nullable Foreign Keys

Nullable references are permitted only when the Domain relationship is genuinely optional.

They must not hide incomplete required state.

## 49. Delete Behavior

Cascade behavior is explicit.

Chronicle SHOULD prefer restrictive deletion for historical and authoritative entities.

## 50. Soft Deletion

Historical entities generally use lifecycle status rather than physical deletion.

Examples:

- Memory archived;
- Campaign archived;
- Character retired.

## 51. Physical Deletion

Physical deletion is reserved for:

- nonauthoritative staging;
- temporary records;
- derived index data;
- explicit full-data removal workflows.

## 52. Query Context

Read queries use no-tracking contexts by default.

## 53. Read Consistency

A screen-level read may use one short transaction or connection snapshot when several projections must agree.

## 54. Query During Write

Queries should observe committed state only.

WAL should allow many read scenarios to remain responsive during short writes.

## 55. Long Read Transaction

Long-lived read transactions are discouraged because they may delay WAL cleanup and retain stale snapshots.

## 56. Transcript Pagination

Transcript queries use explicit sequence ordering and bounded pages.

They do not keep one database reader open across user interaction.

## 57. Streaming Queries

Streaming may be used for exports or diagnostics.

It must not hold a long read transaction that blocks required maintenance without explicit design.

## 58. Work Item Claiming

Work Item lease claiming uses an atomic update under a short transaction.

The claim verifies:

- eligible status;
- due time;
- lease state;
- expected version.

## 59. Claim Race

When multiple workers race:

- exactly one update succeeds;
- losers observe zero affected rows;
- losers select another candidate;
- no exception-driven retry loop is required.

## 60. Work Item Lease Renewal

Lease renewal is a short isolated write.

It verifies:

- current owner;
- expected Work Item version;
- running status.

## 61. Backup Concurrency

Backup uses a consistency-safe SQLite backup API or equivalent provider-supported snapshot.

It does not copy the active database file directly.

## 62. Backup Read Load

Backup may coexist with ordinary activity when tested.

Chronicle MAY temporarily reduce or coordinate high-volume writes if required for predictable backup duration.

## 63. Backup Publication

The database snapshot is only one component of a valid backup.

Manifest, checksums, and file publication follow ADR-0022.

## 64. Restore

Restore validation occurs against isolated storage.

Active database replacement occurs only through a controlled startup or recovery workflow with exclusive ownership.

## 65. Database Replacement

The active database MUST NOT be replaced while ordinary contexts, workers, or UI commands remain active.

## 66. Migration Connection

Storage migrations use a dedicated controlled context or connection.

Workers and normal commands remain disabled.

## 67. Migration Transaction

Each migration uses the transaction strategy appropriate to its operations.

Large migrations may require staged checkpoints rather than one unbounded transaction.

## 68. Migration Lock

Migration owns exclusive application-level mutation access.

## 69. PRAGMA Ownership

Only persistence initialization and migration code may set database-wide PRAGMAs.

Application handlers MUST NOT issue arbitrary PRAGMA statements.

## 70. Connection String

The production connection string is built by Infrastructure.

It SHOULD specify only reviewed settings.

It MUST NOT contain credentials.

## 71. Connection String Logging

Connection strings are not logged in full.

Diagnostics may report:

- database mode;
- pooling state;
- journal mode;
- safe path classification;
- busy timeout.

## 72. Shared Cache

SQLite shared-cache mode is not selected by default.

It may introduce surprising locking and is unnecessary without evidence.

## 73. Read-Only Connection

Safe Mode MAY use a read-only connection when:

- migration is blocked;
- mutation is unsafe;
- the database can still be inspected safely.

## 74. Read-Only Constraints

Read-only mode must not:

- start workers;
- run migrations;
- update operation status;
- create backups through mutation of the source database unless the backup API supports safe read-only operation.

## 75. Database Encryption

Application-level database encryption is outside MVP.

If introduced, it requires a separate ADR for:

- key management;
- startup unlock;
- backup;
- migration;
- recovery;
- performance.

## 76. Corruption Detection

Chronicle reacts to:

- malformed database;
- failed integrity checks;
- impossible schema;
- foreign-key violations;
- missing required tables.

## 77. Corruption Response

On suspected corruption:

- block normal mutation;
- preserve files;
- enter Safe Mode;
- offer backup, integrity diagnostics, or restore;
- do not recreate the database silently.

## 78. Disk Full

A disk-full database write:

- rolls back when SQLite guarantees permit;
- maps to a typed error;
- preserves the prior committed state;
- may leave WAL growth requiring recovery inspection;
- blocks repeated unsafe mutation until space is available.

## 79. I/O Error

Filesystem or device I/O errors are treated as serious persistence failures.

Normal mutation may be blocked until health is revalidated.

## 80. Database Busy Retry

Infrastructure MAY retry a narrow database operation when:

- no Domain decision would be repeated;
- the operation has not committed;
- the retry is bounded;
- the same context or a safe fresh context is used appropriately;
- cancellation is honored.

## 81. No Whole-Command Blind Retry

The persistence adapter must not rerun the entire command handler after a busy or concurrency failure.

## 82. Safe Retry Examples

Potential safe retries include:

- opening a connection;
- applying a lease renewal;
- a metadata read;
- a narrowly scoped idempotent claim update.

## 83. Unsafe Retry Examples

Unsafe automatic retries include:

- regenerating Dice;
- re-invoking provider output;
- repeating Character advancement logic;
- reapplying Session finalization;
- re-running import publication.

## 84. Execution Strategy

EF Core execution strategies that transparently rerun transactions are not enabled without explicit review.

## 85. Error Mapping

SQLite and EF Core errors map to stable Chronicle codes such as:

```text
persistence.database-busy
persistence.database-locked
persistence.concurrency-conflict
persistence.unique-conflict
persistence.foreign-key-conflict
persistence.disk-full
persistence.read-only
persistence.io-failure
persistence.database-corrupt
persistence.transaction-failed
persistence.commit-unknown
```

## 86. Commit Unknown

If the process or provider loses confirmation around commit:

- OperationId is reused;
- Operation Record is inspected;
- unique constraints are checked;
- Chronicle does not repeat the effect blindly.

## 87. Connection Failure

A connection failure before mutation maps to `NotCommitted`.

A failure during or after commit may require `CommitUnknown`.

## 88. Logging

Persistence logs MAY include:

- operation type;
- DbContext scope ID;
- transaction duration;
- safe table or constraint key;
- SQLite result category;
- affected row count;
- journal mode;
- busy duration;
- retry count;
- OperationId;
- CampaignId.

## 89. SQL Logging

Raw SQL logging is disabled by default in Release.

Parameter values are never logged by default.

## 90. Sensitive Data Logging

EF Core sensitive-data logging is prohibited in Stable Release.

## 91. Detailed Errors

Detailed EF errors may be enabled only in Development and test environments.

They still must avoid secret-bearing data.

## 92. Query Diagnostics

Slow-query diagnostics should record:

- query type;
- duration;
- row count;
- query-plan identifier where available.

They should not record transcript text or Character data.

## 93. Metrics

Useful metrics include:

```text
DatabaseOpenDuration
TransactionDuration
DatabaseBusyCount
ConcurrencyConflictCount
UniqueConflictCount
RollbackCount
CommitUnknownCount
WalSize
CheckpointDuration
BackupSnapshotDuration
```

## 94. Connection Health

Startup and diagnostics MAY inspect:

- open success;
- journal mode;
- foreign-key enforcement;
- schema version;
- integrity indicators;
- read-write capability.

## 95. Testing Strategy

The persistence configuration requires:

```text
Unit Tests
SQLite Integration Tests
Concurrency Tests
Crash Tests
Migration Tests
Backup Tests
Failure Injection Tests
Architecture Tests
Performance Tests
```

## 96. Real SQLite Requirement

Core persistence tests MUST use the real SQLite provider.

EF Core in-memory provider is not an acceptable substitute.

## 97. Connection Tests

Tests MUST prove:

- foreign keys enabled;
- WAL selected;
- busy timeout applied;
- connection string safe;
- pooling behavior acceptable;
- read-only mode behavior.

## 98. Transaction Tests

Tests MUST cover:

- commit;
- rollback;
- exception before SaveChanges;
- exception after SaveChanges before commit;
- commit failure;
- post-commit notification failure;
- no transaction during provider call.

## 99. Concurrency Tests

Tests MUST cover:

- two reads;
- read during write;
- two competing Campaign writes;
- optimistic version mismatch;
- duplicate Message sequence;
- duplicate OperationId;
- two Work Item claimers;
- lease renewal race.

## 100. Busy Tests

Tests SHOULD hold a write lock deliberately and verify:

- bounded wait;
- typed busy result;
- cancellation;
- no indefinite hang;
- no duplicate Domain effect.

## 101. Disk-Full Tests

Fault injection or constrained filesystem tests SHOULD verify:

- rollback;
- typed error;
- valid prior database;
- recovery after space becomes available.

## 102. Crash Tests

Tests SHOULD terminate the process:

- during write transaction;
- after commit before result return;
- during WAL activity;
- during lease claim;
- during migration.

Restart must preserve committed state and recover incomplete work.

## 103. Backup Tests

Tests MUST prove:

- active database backup is consistent;
- WAL activity does not corrupt snapshot;
- restore opens;
- semantic state matches;
- direct file copy is not used.

## 104. Migration Tests

Tests MUST cover:

- WAL database migration;
- prior journal mode;
- checkpoint;
- foreign keys;
- large table transformation;
- failed migration rollback or staged recovery.

## 105. Required Test Cases

Tests MUST cover:

- first database creation;
- foreign-key violation;
- unique constraint;
- check constraint;
- optimistic concurrency;
- transaction rollback;
- duplicate Dice Roll OperationId;
- duplicate finalization;
- duplicate Work Item claim;
- read during write;
- busy timeout;
- lock cancellation;
- database read-only;
- disk full;
- database corrupt;
- backup during activity;
- WAL checkpoint;
- Safe Mode read-only open;
- no sensitive SQL logging;
- DbContext disposed after scope.

## 106. Architecture Tests

Architecture tests MUST reject:

- DbContext in ViewModels;
- DbContext in Domain;
- singleton DbContext;
- repository singleton;
- provider call inside transaction;
- arbitrary PRAGMA in Application;
- active database file copy in backup code;
- whole-command EF execution strategy retry;
- sensitive-data logging enabled in Release;
- deletion of WAL or SHM outside persistence.

## 107. Prohibited Patterns

### 107.1 Long-Lived DbContext

Contexts are operation-scoped.

### 107.2 Transaction Across Provider Call

External waits remain outside transactions.

### 107.3 Foreign Keys Disabled

Relational integrity is mandatory.

### 107.4 Blind Retry of Domain Command

Retry semantics remain Application-owned.

### 107.5 Copy Active Database File

Use SQLite's consistency-safe backup mechanism.

### 107.6 Delete WAL as Cleanup

Persistence owns sidecars.

### 107.7 Database Default Order

Queries use explicit ordering.

### 107.8 Hidden SaveChanges in Repository Methods

The Unit of Work controls commit.

### 107.9 Sensitive EF Logging in Stable

Prohibited.

### 107.10 Recreate Corrupt Database Automatically

Enter Safe Mode and preserve evidence.

## 108. Alternatives Considered

### Rollback Journal Mode

Simpler and broadly compatible, but WAL is preferred for responsive concurrent reads under Chronicle's desktop workload.

Rollback mode remains a fallback only after validation.

### One DbContext for Entire Application

Rejected because it creates stale tracking, memory growth, transaction ambiguity, and concurrency hazards.

### Database per Campaign

Rejected for MVP because installation-wide operations, Work Items, package metadata, backup, and migration benefit from one database.

### Pessimistic Locking Only

SQLite writer serialization exists, but Chronicle still needs optimistic versions and Application coordination.

### Automatic EF Execution Strategy Retries

Rejected as a default because transparent transaction replay can repeat Domain decisions and nondeterministic work.

### External Database Server

Rejected because Chronicle is local-first and single-user in MVP.

## 109. Consequences

### Positive

- strong local integrity;
- responsive read behavior;
- predictable writer boundaries;
- clear concurrency conflicts;
- duplicate-effect protection;
- safe backup;
- typed persistence failures;
- realistic integration testing;
- low deployment complexity.

### Negative

- SQLite still permits one writer at a time;
- WAL behavior requires platform testing;
- optimistic version handling adds code;
- busy and lock policies require tuning;
- backup and migration need specialized handling;
- database corruption recovery remains a user-facing concern.

## 110. Risks

### WAL Not Available or Reliable

Mitigation:

- startup validation;
- packaged-runtime integration tests;
- explicit fallback review.

### Busy Timeout Too Short

Mitigation:

- workload measurement;
- typed retry;
- short transactions;
- mutation coordination.

### Busy Timeout Too Long

Mitigation:

- bounded configuration;
- cancellation;
- UI progress;
- metrics.

### Version Conflict Handling Is Inconsistent

Mitigation:

- shared repository conventions;
- concurrency-token tests;
- typed Application results.

### Commit Outcome Ambiguous

Mitigation:

- Operation Records;
- unique constraints;
- status inspection;
- no blind retry.

## 111. Technology Spike

Before acceptance, implement:

1. connection factory;
2. connection initializer;
3. foreign-key enforcement;
4. WAL activation;
5. busy timeout;
6. transaction factory;
7. aggregate version concurrency token;
8. Campaign mutation conflict test;
9. OperationId unique constraints;
10. Work Item atomic claim;
11. safe error mapper;
12. active SQLite backup;
13. crash injection;
14. Release logging configuration;
15. database diagnostics report.

## 112. Spike Acceptance

The spike passes when:

- all connections enforce foreign keys;
- the packaged runtime uses validated WAL behavior;
- read queries remain available during short writes;
- two Campaign mutations cannot silently overwrite each other;
- duplicate OperationId returns existing authoritative result;
- provider waits occur with no open database transaction;
- busy locks terminate within policy;
- backup during normal activity restores correctly;
- crashes preserve committed state;
- raw SQL parameters and private data do not appear in Release logs.

## 113. Definition of Compliance

An implementation complies when:

- one SQLite database is used per installation;
- DbContexts are short-lived and scoped;
- foreign keys are enabled;
- WAL is preferred and validated;
- busy waits are bounded;
- write transactions are explicit and short;
- external waits occur outside transactions;
- optimistic concurrency versions protect mutable aggregates;
- unique constraints protect once-only effects;
- retries are narrow and safe;
- backup uses SQLite snapshot APIs;
- persistence errors are mapped into typed Application outcomes;
- WAL and SHM remain under persistence ownership;
- real SQLite tests enforce the configuration.

## 114. Review Triggers

This ADR must be reviewed if:

- multiple Chronicle processes share one database;
- a separate worker process is introduced;
- server or multiplayer hosting is added;
- write contention becomes frequent;
- a database per Campaign becomes desirable;
- SQLite encryption is introduced;
- cloud synchronization touches active storage;
- an external database engine is considered;
- WAL proves unreliable on a supported platform;
- performance requires materially different transaction behavior.

## 115. Deferred Decisions

Later ADRs MAY define:

- exact busy timeout;
- exact synchronous mode;
- exact WAL checkpoint policy;
- exact immediate versus deferred transaction selection;
- database encryption;
- multi-process coordination;
- server database engine;
- per-Campaign database split;
- read replica or derived database;
- persistence performance budgets.

## 116. Final Decision

Chronicle will use short-lived EF Core contexts over one local SQLite database, with foreign keys enabled, WAL preferred, bounded busy handling, explicit transactions, optimistic versions, and unique constraints.

Application coordination reduces contention.

Database constraints prove correctness.

A writer may need to wait.

It may never hold the Campaign hostage while waiting on the outside world.
