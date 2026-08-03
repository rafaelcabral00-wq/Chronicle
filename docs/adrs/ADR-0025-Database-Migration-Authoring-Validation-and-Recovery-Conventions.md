---
id: ADR-0025
title: Database Migration Authoring, Validation, and Recovery Conventions
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
  - ADR-0011
  - ADR-0012
  - ADR-0014
  - ADR-0017
  - ADR-0018
  - ADR-0019
  - ADR-0020
  - ADR-0021
  - ADR-0022
  - ADR-0023
  - ADR-0024
  - RFC-0018
  - RFC-0020
  - RFC-0033
  - RFC-0034
  - RFC-0036
  - RFC-0037
  - RFC-0040
  - RFC-0041
  - RFC-0042
---

> **"A migration is not a schema edit. It is a controlled transformation of someone’s history."**

# Database Migration Authoring, Validation, and Recovery Conventions

## 1. Status

**Proposed**

This ADR defines Chronicle's database migration authoring, execution, validation, checkpoint, rollback, compatibility, and recovery conventions.

The decision is:

- use EF Core migrations as the authoritative relational schema migration mechanism;
- keep all production migrations inside the persistence Infrastructure project;
- require meaningful migration identifiers and reviewed generated code;
- separate relational schema migration from durable JSON contract migration;
- classify migrations by risk and reversibility;
- inspect source storage version before normal repositories activate;
- create a validated checkpoint before destructive or high-risk migrations;
- execute migrations only under exclusive application-level storage ownership;
- keep workers, providers, and ordinary commands disabled during migration;
- validate preconditions before each migration;
- validate postconditions after each migration;
- preserve original data until the migrated target is proven usable;
- avoid automatic destructive rollback when rollback cannot faithfully restore meaning;
- use explicit recovery states rather than pretending a failed migration completed;
- test the complete migration chain from supported historical versions using real SQLite;
- prohibit package-defined arbitrary EF migrations in the MVP;
- prohibit the installer from applying application-database migrations.

The decision becomes **Accepted** after a migration spike proves:

- empty-database initialization;
- upgrade from at least two historical schema versions;
- additive migration;
- table rebuild migration;
- data transformation;
- durable JSON payload migration;
- migration checkpoint creation;
- low-disk preflight;
- migration failure and Safe Mode entry;
- restart after interrupted migration;
- post-migration integrity validation;
- historical fixture compatibility;
- no worker or provider activity during migration.

## 2. Context

Chronicle's database schema will evolve as the application gains or refines:

- Campaign state;
- Session hierarchy;
- Messages;
- Character Sheets;
- Dice evidence;
- Memories;
- Relationships;
- Character Knowledge;
- progression;
- Preferences;
- Operation Records;
- Work Items;
- package metadata;
- audit history;
- compatibility metadata.

Schema evolution may require:

- adding tables or columns;
- adding constraints;
- renaming concepts;
- splitting tables;
- merging structures;
- rebuilding SQLite tables;
- backfilling values;
- converting JSON payload versions;
- rebuilding derived indexes;
- preserving old identifiers;
- reconciling historical states.

SQLite has limited direct alteration capabilities compared with server databases.

Many structural changes are implemented through:

```text
create replacement table
copy and transform data
drop old table
rename replacement
recreate indexes and constraints
```

A migration failure can threaten the user's only authoritative local Campaign history.

Therefore Chronicle must treat migration as a recoverable product workflow, not merely as generated ORM code.

## 3. Decision Drivers

The migration approach prioritizes:

1. preservation of Campaign history;
2. deterministic upgrade behavior;
3. recoverability;
4. explicit compatibility;
5. real SQLite validation;
6. low-risk automatic upgrades;
7. visible high-risk behavior;
8. auditable migration ownership;
9. no silent data loss;
10. bounded startup complexity;
11. package independence;
12. reproducible release validation.

## 4. Decision Summary

Chronicle will use:

```text
Relational Migration Engine
    EF Core migrations

Migration Ownership
    Chronicle persistence Infrastructure

Execution Point
    application startup or explicit recovery workflow

Prerequisite
    exclusive data-directory ownership

Normal Activity During Migration
    disabled

Checkpoint
    required for destructive or high-risk migration

Validation
    preconditions
    migration execution
    postconditions
    integrity checks

Historical Compatibility
    real SQLite fixtures in CI

Failure Outcome
    Safe Mode
    preserved checkpoint
    typed recovery state

Installer
    binaries only
    no application database migration
```

## 5. Migration Types

Chronicle classifies migrations as:

```text
InitialCreation
Additive
ConstraintStrengthening
DataBackfill
TableRebuild
ContractPayloadMigration
DerivedDataRebuild
DestructiveTransformation
RecoveryMigration
```

## 6. Initial Creation

Initial creation applies the full current migration chain to an empty confirmed first-run database.

It is not used to replace missing expected storage.

## 7. Additive Migration

An additive migration may:

- add a table;
- add an optional column;
- add an index;
- add nonbreaking metadata.

It is generally low risk when defaults and compatibility are explicit.

## 8. Constraint-Strengthening Migration

A constraint-strengthening migration adds or tightens:

- uniqueness;
- foreign key;
- check constraint;
- requiredness;
- maximum semantic validity.

It requires a preflight query proving current data satisfies the new rule or an explicit repair transformation.

## 9. Data-Backfill Migration

A data-backfill migration computes new values from existing authoritative state.

It must define:

- source fields;
- deterministic algorithm;
- null behavior;
- invalid-source handling;
- verification query;
- performance expectations.

## 10. Table-Rebuild Migration

A table-rebuild migration replaces a table to support changes SQLite cannot apply safely in place.

It must preserve:

- primary keys;
- foreign keys;
- indexes;
- constraints;
- sequence meaning;
- lifecycle state;
- concurrency versions.

## 11. Contract-Payload Migration

A contract-payload migration transforms versioned JSON stored inside relational records.

Examples:

- Work Item payload;
- Operation result;
- durable error detail;
- staged provider proposal;
- Character field payload.

## 12. Derived-Data Rebuild

Derived data may be dropped and rebuilt when:

- source truth remains intact;
- rebuild is deterministic;
- normal use is blocked or degraded appropriately;
- the process is restart-safe.

## 13. Destructive Transformation

A destructive transformation removes, merges, or irreversibly changes authoritative representation.

It requires:

- validated checkpoint;
- explicit risk classification;
- release review;
- high-confidence test fixtures;
- recovery instructions;
- post-migration semantic validation.

## 14. Recovery Migration

A recovery migration is an explicit tool or workflow used to repair a known incomplete or invalid historical state.

It is not mixed casually into ordinary startup migration.

## 15. Migration Identifier

Every migration has a stable identifier.

Recommended format:

```text
yyyyMMddHHmmss_MeaningfulName
```

Example:

```text
20260801170000_AddOperationRecordCommitState
```

## 16. Meaningful Name

Names describe the schema or data change.

Avoid:

```text
UpdateDatabase
FixStuff
Changes
Migration2
```

## 17. Migration Ordering

Migration ordering is determined by the authoritative migration chain.

Manual reordering after release is prohibited.

## 18. Migration Ownership

One Chronicle persistence project owns the migration history.

Rule Set packages and provider modules MUST NOT add arbitrary production EF Core migrations.

## 19. Package Evolution

Package-specific data evolves through:

- package contract versions;
- Chronicle-approved extension payloads;
- package migration contracts;
- Application workflows.

It does not directly modify the core relational schema in the MVP.

## 20. Migration Authoring

Developers create migrations from reviewed model changes.

The generated migration must be read and understood before merge.

## 21. Generated Code Review

Reviewers must inspect:

- created and dropped tables;
- renamed versus recreated columns;
- foreign keys;
- indexes;
- default values;
- nullability;
- data-copy SQL;
- cascade behavior;
- concurrency columns;
- unintended destructive operations.

## 22. Migration Snapshot Review

The EF Core model snapshot is reviewed as part of the schema change.

Unexpected snapshot differences block merge.

## 23. No Blind Scaffold Acceptance

A scaffolded migration is never accepted solely because EF generated it.

## 24. Explicit SQL

Custom SQL is permitted when EF migration operations are insufficient.

It MUST be:

- SQLite-compatible;
- parameter-independent or safely constructed;
- deterministic;
- reviewed;
- tested against historical fixtures.

## 25. Dynamic SQL

Dynamic SQL based on user data or untrusted identifiers is prohibited in migrations.

## 26. Source Version Inspection

Startup inspects schema and migration history before ordinary repositories activate.

Possible states:

```text
Current
MigrationRequired
FutureVersion
UnknownHistory
IncompleteMigration
CorruptHistory
```

## 27. Future Version

An older application must not migrate or open newer storage for mutation.

Downgrade is blocked.

## 28. Unknown Migration History

Unknown or divergent migration history requires Safe Mode.

Chronicle must not guess that the schema is compatible.

## 29. Migration Plan

Before execution, Chronicle creates a migration plan containing:

```text
SourceSchemaVersion
TargetSchemaVersion
MigrationIds
RiskClass
CheckpointRequired
EstimatedAdditionalSpace
ExpectedDerivedRebuilds
ExpectedPackageActions
CanCancelBeforeStart
RequiresRestart
```

## 30. Risk Classes

Recommended classes:

```text
Low
Moderate
High
RecoveryOnly
```

## 31. Low-Risk Migration

Typical low-risk operations:

- additive table;
- additive nullable column;
- nonunique index;
- metadata table.

## 32. Moderate-Risk Migration

Typical moderate operations:

- deterministic backfill;
- new constraint after validated cleanup;
- bounded JSON payload migration;
- derived-table rebuild.

## 33. High-Risk Migration

Typical high-risk operations:

- table rebuild with authoritative history;
- identifier representation change;
- relationship restructuring;
- destructive merge;
- large Campaign transformation.

## 34. Checkpoint Requirement

A checkpoint is mandatory when:

- migration is destructive;
- rollback is not trivial;
- multiple authoritative tables are rebuilt;
- identifier semantics change;
- substantial data transformation occurs;
- release policy classifies risk as High.

## 35. Checkpoint Content

A migration checkpoint SHOULD contain:

- consistency-safe database snapshot;
- source schema version;
- application version;
- package dependency inventory;
- manifest;
- checksums;
- migration plan;
- creation Instant;
- checkpoint identifier.

## 36. Checkpoint Validation

Chronicle validates the checkpoint before starting the migration.

Validation includes:

- snapshot opens;
- manifest parses;
- checksums match;
- source schema matches;
- required files exist;
- sufficient free space remains.

## 37. No Unvalidated Safety Claim

Creating a file is not sufficient to claim a valid checkpoint.

## 38. Disk-Space Preflight

Before migration, Chronicle estimates space for:

- checkpoint;
- replacement tables;
- WAL growth;
- temporary indexes;
- final database;
- safety margin.

## 39. Space Failure

Insufficient expected space blocks migration before mutation.

The user receives a typed recovery action.

## 40. Exclusive Ownership

Migration requires:

- data-directory lock;
- no active normal worker;
- no active command scope;
- no active backup or restore publication;
- no second Chronicle process.

## 41. Migration Mode

During migration, Chronicle enters a dedicated migration mode.

Normal UI may show progress but cannot mutate Campaign state.

## 42. Provider Disablement

Provider calls are disabled during migration.

## 43. Worker Disablement

Work Item execution is disabled until migration and recovery complete.

## 44. Read Access During Migration

Ordinary Campaign browsing is disabled unless a specific migration can safely support read-only inspection.

The MVP should prefer a dedicated progress and recovery view.

## 45. Migration Transaction Scope

Small migrations SHOULD execute transactionally when SQLite and EF Core support the required operations safely.

## 46. Large Migration Scope

A large migration may use staged transactions only when one unbounded transaction is impractical.

Such a migration requires:

- durable stage markers;
- checkpoint;
- idempotent stages;
- restart logic;
- explicit publication point.

## 47. Atomic Publication

When a migrated database is produced in isolated staging, it becomes active only after full validation and atomic publication according to ADR-0022.

## 48. In-Place Migration

In-place migration is allowed for low- and moderate-risk changes when:

- checkpoint policy is satisfied;
- transaction behavior is understood;
- failure leaves the prior committed state recoverable.

## 49. Precondition Validation

Each migration SHOULD validate assumptions such as:

- source table exists;
- expected column shape;
- no invalid nulls;
- no duplicate future unique keys;
- known status values;
- valid foreign references;
- expected JSON contract versions.

## 50. Precondition Failure

A failed precondition stops migration before destructive steps.

It maps to a typed migration error.

## 51. Postcondition Validation

After each migration or migration group, Chronicle SHOULD validate:

- migration history;
- table shape;
- required indexes;
- constraints;
- row counts;
- transformed-value counts;
- foreign keys;
- payload versions.

## 52. Semantic Validation

Schema validity alone is insufficient.

Semantic checks may include:

- one active Session per Campaign;
- Message sequence uniqueness;
- valid Dice positions;
- no duplicate once-only progression;
- Memory lifecycle consistency;
- Work Item handler compatibility;
- Operation Record status consistency.

## 53. Row-Count Validation

Table-rebuild migrations SHOULD compare row counts.

Any intentional difference must be documented and tested.

## 54. Hash or Aggregate Validation

For large transformations, Chronicle MAY compare:

- identity counts;
- grouped counts;
- aggregate sums;
- content hashes;
- key-set equality.

## 55. Foreign-Key Validation

After migration, Chronicle SHOULD run:

```text
PRAGMA foreign_key_check
```

or equivalent approved validation.

## 56. Integrity Check

Selected migrations require a bounded integrity check.

High-risk or suspicious migrations may require a full integrity check.

## 57. Derived Data

Derived indexes or projections may be marked:

```text
RebuildRequired
```

rather than rebuilt inside the schema transaction.

## 58. Rebuild Work Items

Required rebuild Work Items are created only after the migrated schema is accepted and normal durable-work execution is safe.

## 59. Durable Payload Migration

Payload migration occurs through explicit contract adapters.

The relational migration may:

- update version metadata;
- transform payload bytes;
- mark unsupported rows as recovery-required.

## 60. Unsupported Payload

A payload that cannot be safely migrated is preserved.

Affected Work Items or Operations become:

```text
RecoveryRequired
```

rather than executing under guessed semantics.

## 61. Provider Output Staging

Old staged provider proposals must be:

- migrated;
- invalidated;
- or marked recovery-required.

They must not be accepted under a new contract without revalidation.

## 62. Migration Time

Migration code uses injected UTC time only when the target schema requires a new accepted technical timestamp.

It must not invent historical Domain time.

## 63. Historical Time

When backfilling a timestamp:

- prefer preserved source time;
- use explicit unknown state when possible;
- use migration execution time only for technical metadata;
- document the semantic limitation.

## 64. Identifier Migration

Changing identifier representation is High risk.

The migration must preserve exact identity across:

- primary keys;
- foreign keys;
- JSON references;
- manifests;
- Operation Records;
- Work Items.

## 65. Sequence Migration

Changing hierarchy or sequence representation must preserve deterministic order.

## 66. Enum or Status Migration

Status values use explicit mappings.

Unknown source values block or enter recovery.

## 67. Nullability Migration

Before making a column required:

1. inspect null rows;
2. determine valid deterministic backfill;
3. backfill;
4. validate;
5. apply required constraint.

## 68. Unique Constraint Migration

Before adding uniqueness:

1. identify duplicates;
2. classify them;
3. repair deterministically or block;
4. add constraint;
5. validate.

## 69. Foreign-Key Migration

Before adding a foreign key:

1. identify orphan rows;
2. repair, archive, or block;
3. add relationship;
4. validate with foreign-key check.

## 70. Table Rename

When SQLite and EF support a safe rename, use it.

When semantics also change, prefer an explicit replacement table and transformation.

## 71. Column Rename

A column rename must be distinguished from drop-and-add.

Reviewers must ensure data is preserved.

## 72. Column Type Change

SQLite's dynamic typing does not remove the need for explicit conversion and validation.

## 73. Large Campaigns

Migration performance must be tested against a complex Campaign fixture.

## 74. Progress Reporting

Long migrations SHOULD report:

- migration name;
- current stage;
- completed units where meaningful;
- estimated remaining work only when reliable;
- cancellation availability.

## 75. Durable Progress

High-risk staged migrations SHOULD persist progress markers.

Low-risk transactional migrations need not persist every intermediate stage.

## 76. Cancellation

Cancellation is allowed before mutation begins.

During a short transactional migration:

- cancellation may request rollback;
- commit stage may be non-cancellable.

During a staged migration:

- cancellation occurs only at safe checkpoints.

## 77. Process Crash

After crash, startup inspects:

- migration history;
- stage markers;
- checkpoint;
- active database;
- staging database;
- migration Work Item or Operation Record.

## 78. Incomplete Migration

An incomplete migration enters:

```text
MigrationRecoveryRequired
```

or resumes only when the migration defines deterministic restart behavior.

## 79. No Guessing After Crash

Chronicle must not mark a migration complete because some expected columns exist.

Completion requires the migration history and postconditions to agree.

## 80. Rollback

Automatic rollback is allowed only when:

- the migration has a proven reversible path;
- rollback preserves all meaning;
- the active source remains identifiable;
- rollback itself is tested.

## 81. Checkpoint Restore

For irreversible or unclear failure, restore from the validated checkpoint through an explicit recovery workflow.

## 82. Down Migrations

EF Core `Down` methods are required for development clarity where practical.

They are not automatically trusted as production data rollback.

## 83. Production Downgrade

Chronicle does not support arbitrary production downgrade through reverse migrations.

## 84. Migration Failure State

Recommended failure codes:

```text
migration.precondition-failed
migration.checkpoint-failed
migration.insufficient-space
migration.unsupported-source-version
migration.unknown-history
migration.data-conflict
migration.payload-version-unsupported
migration.execution-failed
migration.postcondition-failed
migration.integrity-failed
migration.recovery-required
```

## 85. Data Preservation State

Migration failures MUST report:

```text
OriginalPreserved
CheckpointAvailable
TargetStagingPreserved
ActiveDatabaseUnchanged
ActiveDatabaseRequiresInspection
```

## 86. Safe Mode

Any failure that leaves migration status uncertain enters Safe Mode.

## 87. Recovery UI

The recovery UI SHOULD expose:

- source and target versions;
- failing migration ID;
- checkpoint status;
- preserved-data status;
- retry eligibility;
- restore action;
- diagnostic export;
- application restart requirement.

## 88. No Silent Retry Loop

A failed migration is not retried automatically on every startup without classification.

## 89. Retry Eligibility

Retry is permitted when:

- root cause is repaired;
- migration is idempotent or restarted from a known source;
- same checkpoint remains valid;
- no unsupported partial state exists.

## 90. New Checkpoint on Retry

A new checkpoint may be required if source state changed after repair.

## 91. Migration Audit

Chronicle SHOULD persist safe migration execution records.

Recommended fields:

```text
MigrationExecutionId
MigrationId
SourceVersion
TargetVersion
StartedAtUtc
CompletedAtUtc
Outcome
CheckpointId
ApplicationVersion
FailureCode
```

## 92. Migration Logs

Logs MAY include:

- migration ID;
- stage;
- duration;
- row counts;
- source and target schema;
- checkpoint result;
- safe failure code.

They MUST NOT include narrative content or raw payloads.

## 93. Metrics

Useful metrics include:

```text
MigrationDuration
MigrationFailureCount
MigrationCheckpointDuration
MigrationRowsTransformed
MigrationRecoveryCount
MigrationPostconditionFailureCount
```

## 94. Release Process

A release containing migrations MUST identify them in release metadata.

## 95. Release Notes

User-facing release notes SHOULD mention migrations only when they may affect:

- startup duration;
- backup recommendation;
- compatibility;
- downgrade;
- required user action.

## 96. Migration Manifest

The build MAY generate a migration manifest containing:

```text
MigrationIds
TargetSchemaVersion
RiskClasses
CheckpointRequirements
MinimumSupportedSourceVersion
```

## 97. CI Gates

CI MUST validate:

- migration compiles;
- model snapshot matches;
- empty database upgrades;
- supported historical fixtures upgrade;
- postconditions pass;
- downgrade is not accidentally assumed;
- package fixtures remain compatible;
- migration manifest matches code.

## 98. Historical Fixtures

Chronicle SHOULD retain representative database fixtures for supported historical versions.

## 99. Fixture Privacy

Historical fixtures must contain synthetic data only.

## 100. Fixture Coverage

Fixtures SHOULD include:

- minimal Campaign;
- complex Campaign;
- pending Work Items;
- completed and failed Operation Records;
- archived Memories;
- package-version history;
- edge-case Unicode;
- large transcript.

## 101. Full-Chain Testing

CI MUST apply every migration from the oldest supported fixture to current.

## 102. Stepwise Testing

Selected tests SHOULD validate each intermediate migration state.

## 103. Clean-Schema Comparison

The final migrated schema SHOULD be compared with a newly created current schema.

Differences must be explained.

## 104. Semantic Comparison

A migrated Campaign should be semantically equivalent to the source meaning after expected transformations.

## 105. Performance Testing

High-risk migrations require performance testing on a representative complex fixture.

## 106. Failure Injection

Migration tests SHOULD inject failure:

- before checkpoint;
- after checkpoint;
- during table copy;
- before old-table removal;
- after replacement-table creation;
- during payload conversion;
- before migration-history update;
- after history update before postcondition validation.

## 107. Required Test Cases

Tests MUST cover:

- empty creation;
- no migration required;
- one additive migration;
- multiple chained migrations;
- nullable-to-required conversion;
- unique constraint with clean data;
- duplicate-data precondition failure;
- table rebuild;
- identifier preservation;
- sequence preservation;
- JSON payload upgrade;
- unsupported JSON version;
- checkpoint creation;
- checkpoint validation failure;
- insufficient disk;
- migration cancellation;
- process crash;
- restart inspection;
- postcondition failure;
- foreign-key check failure;
- Safe Mode entry;
- successful retry;
- checkpoint restore.

## 108. Architecture Tests

Architecture tests MUST reject:

- migrations outside the persistence project;
- installer invoking Chronicle database migrations;
- Rule Set package registering EF migrations;
- provider calls from migration code;
- ambient time in migration transformation;
- deletion of source before checkpoint requirement is satisfied;
- unversioned JSON transformation;
- normal worker activation during migration;
- arbitrary production downgrade path.

## 109. Prohibited Patterns

### 109.1 Trust Generated Migration Without Review

Generated code is only a draft.

### 109.2 Drop and Recreate Authoritative Table Without Verified Copy

Preserve and validate data.

### 109.3 Add Unique Constraint Without Duplicate Preflight

Existing data must be proven compatible.

### 109.4 Use Current Time as Historical Event Time

Technical migration time is not Domain history.

### 109.5 Execute Provider During Migration

Migration is local and deterministic.

### 109.6 Package-Owned EF Migration

Core schema remains Chronicle-owned.

### 109.7 Installer Applies Database Migration

The application owns startup, checkpoint, and recovery.

### 109.8 Silent Automatic Rollback

Rollback semantics must be proven.

### 109.9 Retry Unknown Partial Migration Blindly

Inspect and recover explicitly.

### 109.10 Delete Checkpoint Immediately After Startup

Checkpoint retention follows a deliberate policy.

## 110. Alternatives Considered

### Manual SQL Migration System

Provides full control but duplicates EF Core model and migration infrastructure.

Chronicle will use EF migrations with reviewed custom SQL where needed.

### Recreate Database and Reimport

Rejected because it risks identity loss, omitted history, and incomplete semantic transfer.

### One Giant Migration per Release

Rejected because smaller meaningful migrations are easier to test, review, and recover.

### Automatically Run All Migrations Without Checkpoint

Rejected for destructive or high-risk changes.

### Support Arbitrary Down Migration

Rejected because reverse schema operations may not restore lost meaning.

### Package-Owned Database Extensions

Deferred because arbitrary package migrations would weaken core schema governance and recovery.

## 111. Consequences

### Positive

- safer upgrades;
- reproducible migration behavior;
- explicit recovery;
- tested historical compatibility;
- no package-driven schema drift;
- preserved original state;
- meaningful release review;
- clear Safe Mode integration.

### Negative

- historical fixtures require maintenance;
- high-risk releases require more testing;
- checkpoint creation increases disk and startup cost;
- migration code and postconditions add effort;
- old payload migrations may accumulate;
- downgrade remains intentionally limited.

## 112. Risks

### Historical Fixture Drift

Mitigation:

- generate fixtures from tagged versions;
- validate manifests;
- use synthetic canonical scenarios.

### Migration Appears Successful but Changes Meaning

Mitigation:

- semantic postconditions;
- Domain-level comparison;
- representative fixtures;
- explicit review.

### Checkpoint Consumes Too Much Space

Mitigation:

- preflight;
- streaming backup;
- user guidance;
- cleanup policy after successful validation.

### Staged Migration Complexity

Mitigation:

- use only for necessary high-risk changes;
- explicit state machine;
- idempotent stages;
- crash tests.

### Unsupported Old Version

Mitigation:

- minimum supported source policy;
- migration bridge tool or intermediate release;
- clear Safe Mode guidance.

## 113. Technology Spike

Before acceptance, implement:

1. migration planner;
2. migration risk metadata;
3. migration manifest;
4. checkpoint gate;
5. empty-schema initialization;
6. additive migration;
7. SQLite table rebuild;
8. deterministic data backfill;
9. JSON payload migrator;
10. postcondition validator;
11. foreign-key validation;
12. migration execution record;
13. failure injection;
14. Safe Mode recovery screen;
15. historical fixture CI matrix.

## 114. Spike Acceptance

The spike passes when:

- a first-run database reaches the current schema;
- supported historical fixtures upgrade successfully;
- a high-risk migration cannot begin without a validated checkpoint;
- a failed precondition prevents mutation;
- process interruption produces a classified recovery state;
- original authoritative data remains preserved;
- postconditions detect intentional fixture corruption;
- unsupported payloads remain preserved and blocked;
- workers and providers remain disabled throughout migration;
- migrated and freshly created schemas match semantically.

## 115. Definition of Compliance

An implementation complies when:

- EF Core migrations are the authoritative relational migration chain;
- migrations live in Chronicle persistence Infrastructure;
- generated migrations are reviewed;
- risk and checkpoint requirements are explicit;
- source compatibility is inspected before normal activation;
- migration runs under exclusive ownership;
- ordinary commands, workers, and providers remain disabled;
- preconditions and postconditions are validated;
- high-risk changes preserve a checkpoint;
- failed migrations enter Safe Mode;
- historical fixtures upgrade in CI using real SQLite;
- package modules and installers do not own database migration.

## 116. Review Triggers

This ADR must be reviewed if:

- Chronicle changes database engine;
- multiple processes share storage;
- package-defined relational extensions are introduced;
- cloud synchronization affects migration;
- encrypted storage changes checkpoint behavior;
- migration time becomes unacceptable;
- minimum supported source-version policy changes;
- online migration becomes necessary;
- server hosting requires zero-downtime migration;
- schema branching becomes unavoidable.

## 117. Deferred Decisions

Later ADRs MAY define:

- exact checkpoint retention period;
- exact migration risk-class format;
- exact supported historical-version window;
- migration bridge releases;
- automated semantic schema comparison;
- staged migration framework;
- encrypted checkpoint handling;
- package extension migration model;
- server migration strategy;
- release-note generation from migration manifest.

## 118. Final Decision

Chronicle will treat database migration as a controlled transformation of authoritative history.

Every migration will have an owner, source assumptions, target guarantees, validation, and recovery behavior.

Low-risk changes may be automatic.

High-risk changes require proven checkpoints.

When Chronicle cannot prove that a migration preserved meaning, it will stop, preserve evidence, and enter Safe Mode.
