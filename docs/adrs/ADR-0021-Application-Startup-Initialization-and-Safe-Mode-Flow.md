---
id: ADR-0021
title: Application Startup, Initialization, and Safe Mode Flow
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
  - ADR-0003
  - ADR-0004
  - ADR-0006
  - ADR-0007
  - ADR-0008
  - ADR-0011
  - ADR-0012
  - ADR-0013
  - ADR-0014
  - ADR-0015
  - ADR-0017
  - ADR-0018
  - ADR-0019
  - ADR-0020
  - RFC-0018
  - RFC-0019
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
---

> **"Chronicle should start by proving that the Campaign is safe to open, not by assuming that every subsystem is healthy."**

# Application Startup, Initialization, and Safe Mode Flow

## 1. Status

**Proposed**

This ADR defines Chronicle's startup sequence, initialization stages, degraded-mode behavior, startup recovery, and Safe Mode flow.

The decision is:

- use an explicit staged startup coordinator;
- distinguish bootstrap, validation, initialization, recovery, and activation;
- initialize logging before the normal dependency graph;
- acquire the data-directory ownership lock before mutable storage access;
- validate configuration and platform capability before feature activation;
- inspect storage compatibility before constructing the normal workspace;
- create migration checkpoints before any destructive storage migration;
- recover or classify incomplete operations before workers start;
- activate background workers only after storage, packages, and operation state are safe;
- enter Safe Mode when normal startup cannot prove safe operation;
- keep Safe Mode read-oriented and recovery-focused;
- preserve Campaign data and unresolved durable work;
- make every startup failure typed, visible, and diagnosable;
- avoid silently resetting configuration, deleting state, or recreating databases.

The decision becomes **Accepted** after a startup spike proves:

- clean first run;
- ordinary restart;
- restart with pending Work Items;
- restart after crash during provider work;
- upgrade with migration;
- migration failure;
- corrupt configuration;
- unavailable credential store;
- incompatible Rule Set package;
- database integrity failure;
- data-directory lock conflict;
- forced Safe Mode;
- normal-mode recovery after repair.

## 2. Context

Chronicle's startup is responsible for more than opening a window.

The application must safely coordinate:

- configuration;
- logging;
- platform capabilities;
- user-data paths;
- one-instance ownership;
- SQLite initialization;
- schema compatibility;
- migrations;
- backups and checkpoints;
- Rule Set packages;
- provider profiles;
- credential-store availability;
- Work Item recovery;
- Operation Record inspection;
- Rule Knowledge state;
- UI composition;
- background workers.

A naive startup sequence could:

- start workers before migrations finish;
- open a Campaign on incompatible storage;
- overwrite a damaged database;
- activate a provider before credentials are available;
- run two processes against the same data directory;
- hide a failed migration behind an empty library;
- discard pending durable work;
- require a broken subsystem to open recovery tools.

Chronicle must prefer a recoverable degraded state over uncertain mutation.

## 3. Decision Drivers

Startup prioritizes:

1. Campaign safety;
2. recoverability;
3. explicit stage ownership;
4. deterministic initialization;
5. typed failure;
6. Safe Mode availability;
7. no silent data reset;
8. short startup transactions;
9. visible progress;
10. testability;
11. platform capability validation;
12. clean shutdown symmetry.

## 4. Decision Summary

Chronicle will use:

```text
Startup Coordinator
    explicit staged state machine

Bootstrap
    paths
    minimal configuration
    bootstrap logging
    application mode

Ownership
    acquire data-directory lock

Composition
    build validated service graph

Storage
    inspect
    migrate
    validate
    recover

Capabilities
    credential store
    Rule Set packages
    provider profiles
    Rule Knowledge

Activation
    workers
    notifications
    main workspace

Fallback
    Safe Mode

Failure Policy
    preserve state
    expose typed recovery
    never silently recreate or delete
```

## 5. Startup Modes

Chronicle supports:

```text
Normal
SafeMode
MigrationOnly
Diagnostics
Development
Test
```

The MVP user-facing modes are primarily:

```text
Normal
SafeMode
```

## 6. Startup State Machine

Recommended states:

```text
NotStarted
Bootstrapping
AcquiringOwnership
BuildingServices
InspectingStorage
MigratingStorage
ValidatingStorage
RecoveringOperations
ValidatingPackages
ValidatingCapabilities
ActivatingWorkers
CreatingWorkspace
Ready
Degraded
SafeMode
Failed
ShuttingDown
Stopped
```

## 7. Startup Coordinator

A dedicated startup coordinator owns stage progression.

Conceptually:

```csharp
public interface IApplicationStartupCoordinator
{
    Task<StartupResult> StartAsync(
        StartupRequest request,
        CancellationToken cancellationToken);
}
```

It does not own UI rendering directly.

## 8. Startup Request

A startup request MAY include:

```text
RequestedMode
DataDirectory
ApplicationVersion
ReleaseChannel
ForceSafeMode
SkipWorkerActivation
DiagnosticVerbosity
```

Secrets are never included.

## 9. Startup Result

The result SHOULD contain:

```text
FinalMode
StartupState
Capabilities
StorageStatus
MigrationStatus
RecoveryStatus
PackageStatus
WorkerStatus
Warnings
Errors
ReferenceCode
```

## 10. Startup Progress

Startup progress is represented through typed stage updates.

Example:

```text
Initializing application
Checking Chronicle data
Applying storage migration
Recovering interrupted work
Validating Rule Set packages
Opening workspace
```

Progress text is localized by Presentation.

## 11. Bootstrap Phase

Bootstrap performs only what is required before the normal service graph exists.

It includes:

- executable identity;
- application version;
- release channel;
- command-line parsing;
- data-path resolution;
- minimal configuration;
- bootstrap logger;
- requested startup mode.

## 12. Bootstrap Restrictions

Bootstrap MUST NOT:

- open Campaign data for mutation;
- call providers;
- resolve credentials;
- start workers;
- execute Rule Set operations;
- write normal application state.

## 13. Data Directory Resolution

The data directory is resolved before storage initialization.

It MUST be:

- explicit;
- normalized;
- validated;
- writable when normal mode requires mutation;
- separate from the application install directory.

## 14. Path Validation

Chronicle validates:

- path syntax;
- existence or safe creation;
- permissions;
- unsupported network location policy;
- symbolic-link or reparse-point concerns where applicable;
- available space for initialization and migrations.

## 15. First Run

On first run, Chronicle MAY create:

- data directory;
- configuration directory;
- log directory;
- backup directory;
- empty initial database through approved migrations.

It MUST NOT create a sample Campaign unless explicitly requested by the user.

## 16. Data-Directory Lock

Chronicle acquires exclusive ownership before normal mutable access.

The lock represents:

```text
one active Chronicle process per data directory
```

## 17. Lock Metadata

Safe lock metadata MAY include:

```text
ApplicationInstanceId
ProcessId
ApplicationVersion
AcquiredAtUtc
Mode
```

It MUST NOT be treated as authorization.

## 18. Lock Conflict

If another live process owns the directory:

- normal startup stops;
- Chronicle presents an already-running result;
- no second mutable host starts.

## 19. Stale Lock

A stale lock is inspected carefully.

Chronicle may reclaim it only when the prior owner is confirmed unavailable according to platform policy.

## 20. No Data Mutation Before Ownership

Normal startup MUST NOT migrate, recover, or mutate storage before ownership is acquired.

## 21. Bootstrap Logging

A minimal bootstrap logger starts before full DI composition.

It records:

- application version;
- startup mode;
- startup stage;
- safe failures;
- reference code.

It excludes secrets and private Campaign content.

## 22. Full Logging Handoff

After configuration validation, the full structured logging system replaces or extends bootstrap logging.

The handoff must avoid duplicate providers and lost startup events.

## 23. Configuration Load

Configuration loading occurs in layers:

```text
compiled defaults
application configuration
user configuration
mode-specific overrides
Development or test overrides
```

## 24. Configuration Validation

Validation distinguishes:

```text
Fatal
Recoverable
FeatureBlocking
Warning
```

## 25. Invalid Configuration

Chronicle MUST NOT silently discard an invalid configuration file and replace it with defaults.

It should:

- preserve the invalid file;
- report safe validation errors;
- enter Safe Mode when needed;
- allow repair or reset through explicit user action.

## 26. Configuration Backup

Before an explicit configuration reset or migration, Chronicle SHOULD preserve the prior configuration.

## 27. Service Graph Construction

The service graph is built after bootstrap configuration and mode selection.

Normal and Safe Mode use distinct validated composition profiles.

## 28. Graph Validation Failure

If the normal graph fails:

- startup records a typed composition failure;
- Safe Mode composition is attempted;
- no main workspace is presented as healthy.

## 29. Platform Capability Detection

Chronicle detects capabilities such as:

- supported operating system;
- architecture;
- credential store;
- file locking;
- SQLite native support;
- desktop notification support;
- code-signature status where relevant.

## 30. Unsupported Platform

Unsupported platform status may allow:

- diagnostics;
- read-only inspection;
- source-development use.

It must not imply official support.

## 31. Storage Discovery

Chronicle inspects the expected database and related storage artifacts.

Possible states:

```text
NotInitialized
Compatible
MigrationRequired
FutureVersion
Corrupt
Unavailable
ReadOnly
RecoveryRequired
```

## 32. Missing Database

If storage is expected but missing:

- do not silently create a replacement;
- inspect configuration and backup metadata;
- classify as missing storage;
- offer restore or explicit new-installation initialization.

## 33. Empty First-Run Storage

Only a confirmed first run may initialize a new database automatically.

## 34. Storage Version

Chronicle reads storage version through a minimal compatible inspection path before ordinary repositories activate.

## 35. Future Storage Version

If the database was written by a newer unsupported Chronicle version:

- normal mutation is blocked;
- downgrade is not attempted;
- Safe Mode may offer backup/export-compatible inspection only when safe.

## 36. Migration Planning

Before migration, Chronicle determines:

- source version;
- target version;
- migration chain;
- expected space;
- package dependencies;
- checkpoint requirement;
- rollback limitations.

## 37. Migration Checkpoint

A consistency-safe checkpoint is created before destructive migration.

The checkpoint is validated before migration begins.

## 38. Migration Approval

The MVP MAY apply routine compatible migrations automatically after presenting clear progress.

High-risk migrations MAY require confirmation.

The migration contract defines which class applies.

## 39. Migration Execution

During migration:

- workers remain stopped;
- ordinary Campaign mutation remains blocked;
- migration uses explicit transactions and checkpoints;
- progress is durable where needed;
- cancellation follows migration policy.

## 40. Migration Failure

On failure:

- the target is not published as healthy;
- checkpoint remains available;
- original or last consistent state remains preserved;
- startup enters Safe Mode;
- the failure is typed and diagnosable.

## 41. Storage Integrity Validation

After migration or ordinary open, Chronicle validates:

- schema version;
- required tables;
- foreign keys;
- migration history;
- selected integrity checks;
- Operation Record consistency;
- Work Item status validity.

## 42. Full Integrity Checks

Expensive full-database integrity checks are not required at every startup.

They may run:

- after suspicious shutdown;
- after migration;
- after restore;
- on explicit diagnostics request;
- when corruption indicators appear.

## 43. Unclean Shutdown Detection

Chronicle MAY record clean-shutdown metadata.

On an unclean prior exit, startup performs additional recovery inspection.

## 44. Recovery Phase

Recovery runs before workers activate.

It inspects:

- expired Work Item leases;
- in-progress Operation Records;
- staged provider proposals;
- interrupted finalization;
- pending narration;
- backup or restore staging;
- migration remnants;
- orphaned temporary files.

## 45. Recovery Classification

Each unresolved operation becomes:

```text
AutomaticallyRecoverable
WaitingForConfiguration
WaitingForUser
Superseded
FailedTerminal
RecoveryRequired
```

## 46. Automatic Recovery

Automatic recovery may:

- expire foreign leases;
- requeue safe Work Items;
- finalize already committed operation status;
- remove invalid nonpublished temporary artifacts;
- rebuild derived indexes.

It MUST NOT repeat authoritative effects.

## 47. User-Guided Recovery

User action is required for:

- missing credentials;
- incompatible package;
- ambiguous restore target;
- unsupported durable payload;
- unresolved integrity conflict;
- destructive reset.

## 48. Provider Calls During Startup

Normal startup MUST NOT perform arbitrary narration or generation calls.

A bounded provider health check may occur only after the workspace is available or when explicitly requested.

## 49. Credential Store Validation

Startup checks capability and metadata, not secret values.

Possible states:

```text
Available
Unavailable
AccessDenied
Unsupported
Degraded
```

## 50. Credential Failure

Credential-store failure blocks credential-dependent provider features.

It does not block:

- Campaign browsing;
- local Dice;
- backup;
- export;
- Safe Mode;
- local Rule Set mechanics.

## 51. Rule Set Package Validation

Startup validates active package metadata:

- identity;
- version;
- compatibility;
- required contracts;
- duplicate keys;
- package trust;
- migration requirements.

## 52. Missing Rule Set Package

A Campaign with a missing required package remains visible but incompatible.

Chronicle MUST NOT substitute another package silently.

## 53. Incompatible Rule Set Package

Normal play is blocked for affected Campaigns until:

- compatible package installed;
- package migration completed;
- or supported restore selected.

## 54. Rule Knowledge Validation

Startup inspects Rule Knowledge index state.

Possible states:

```text
Ready
RebuildRequired
Missing
Corrupt
RestrictedSourceUnavailable
Disabled
```

## 55. Rule Knowledge Failure

Rule Knowledge failure is usually degraded, not fatal.

Chronicle may continue with:

- deterministic mechanics;
- reduced rule explanations;
- rebuild Work Item after activation.

## 56. Provider Profile Validation

Startup validates:

- provider key exists;
- model profile configuration parses;
- credential reference format;
- required capabilities;
- endpoint policy.

It does not require a live network call.

## 57. Capability Model

Startup produces a capability snapshot.

Example:

```text
CanBrowseCampaigns
CanMutateCampaigns
CanStartSession
CanUseRemoteNarration
CanUseLocalNarration
CanExecuteDice
CanFinalizeSession
CanCreateBackup
CanRestore
CanExport
CanRunWorkers
CanRebuildRuleKnowledge
```

## 58. Capability Ownership

Application queries and commands still validate capabilities at execution time.

The startup snapshot is not permanent authorization.

## 59. Worker Activation

Workers start only after:

- storage is compatible;
- recovery scan completes;
- mutation is allowed;
- payload handlers are registered;
- startup mode permits execution.

## 60. Worker Activation Order

Recommended order:

```text
operation recovery
Work Item worker
maintenance scheduler
read-model invalidation services
optional desktop notifications
```

## 61. Main Workspace Creation

The normal workspace is created only after minimum safe readiness.

Minimum readiness includes:

- validated graph;
- storage available;
- migration complete;
- recovery classified;
- Campaign queries functional.

## 62. Ready State

`Ready` means:

- the application may safely expose normal allowed capabilities;
- degraded optional features are clearly indicated;
- no unresolved fatal startup issue remains hidden.

## 63. Degraded State

Normal mode may be `Degraded` when optional capability is unavailable.

Examples:

- provider unavailable;
- credential store unavailable;
- Rule Knowledge rebuild needed;
- desktop notifications unsupported.

## 64. Safe Mode Purpose

Safe Mode exists to inspect and repair Chronicle without activating risky mutation workflows.

## 65. Safe Mode Capabilities

Safe Mode SHOULD support:

- storage status inspection;
- migration diagnostics;
- backup creation where safe;
- backup validation;
- restore planning;
- export when storage can be read safely;
- configuration repair;
- credential metadata inspection;
- package inventory;
- operation and Work Item inspection;
- diagnostic bundle creation;
- full integrity check;
- checkpoint restoration.

## 66. Safe Mode Restrictions

Safe Mode SHOULD disable by default:

- starting or continuing Sessions;
- accepting player input;
- Dice execution;
- provider narration;
- Archivist finalization;
- progression;
- package mutation;
- background Work Item execution;
- automatic migrations after failure;
- destructive repair without confirmation.

## 67. Safe Mode UI

Safe Mode uses a dedicated recovery workspace.

It should present:

- problem summary;
- preserved data status;
- available actions;
- blocked features;
- reference code;
- diagnostics;
- path back to Normal mode.

## 68. Forced Safe Mode

Safe Mode may be requested through:

- command-line option;
- keyboard startup gesture;
- recovery action;
- automatic fallback after startup failure.

## 69. Safe Mode Persistence

Entering Safe Mode is not itself a Campaign mutation.

A safe application-mode preference may be stored when the user requests next-start Safe Mode.

## 70. Exiting Safe Mode

After repair, Chronicle may:

- revalidate within the same process;
- or require restart into Normal mode.

Restart is preferred when composition or migration state changed materially.

## 71. No Empty-Library Fallback

If storage cannot be opened, Chronicle MUST NOT show an ordinary empty Campaign library as though no Campaigns exist.

## 72. No Automatic Database Recreation

A corrupt or missing database is never replaced automatically under the same data identity.

## 73. No Automatic Destructive Repair

Destructive actions require:

- explicit explanation;
- validated backup or checkpoint where possible;
- confirmation;
- typed operation;
- audit metadata.

## 74. Startup Cancellation

Startup MAY be cancellable before critical initialization stages.

Once migration commit begins, cancellation follows migration policy.

## 75. Window Close During Startup

Closing the window requests startup cancellation or orderly shutdown.

It must not terminate during an unsafe commit stage without clear handling.

## 76. Startup Timeout

Selected stages may have bounded timeouts.

Examples:

- data lock acquisition;
- storage open;
- hosted service stop;
- optional capability checks.

Migration duration is progress-driven rather than governed by a short generic timeout.

## 77. Splash Screen

A splash or startup window MAY show stage progress.

It must not block access to a visible failure indefinitely.

## 78. Accessibility

Startup and Safe Mode UI MUST support:

- keyboard navigation;
- screen readers;
- noncolor status indicators;
- reduced motion;
- readable progress and error text.

## 79. Localization

Startup state keys and error codes are machine-stable.

Presentation localizes display text.

## 80. Startup Error Model

Recommended error codes include:

```text
startup.data-directory-invalid
startup.data-directory-locked
startup.configuration-invalid
startup.composition-invalid
startup.platform-unsupported
startup.storage-missing
startup.storage-future-version
startup.storage-corrupt
startup.migration-required
startup.migration-failed
startup.recovery-required
startup.package-incompatible
startup.safe-mode-required
```

## 81. Data Preservation Reporting

Every fatal startup result SHOULD state:

```text
DataPreservationState
CheckpointAvailable
BackupRecommended
MutationBlocked
```

## 82. Startup Warnings

Warnings may include:

```text
credential-store-unavailable
provider-profile-disabled
rule-index-rebuild-required
desktop-notification-unavailable
unclean-shutdown-detected
```

## 83. Observability

Startup logs SHOULD include:

- startup stage;
- application version;
- release channel;
- mode;
- stage duration;
- storage version;
- migration IDs;
- package counts;
- recovery counts;
- capability state;
- final outcome.

## 84. Privacy

Startup logs MUST NOT include:

- Campaign title by default;
- Character names;
- credentials;
- provider prompts;
- narrative content;
- full local paths unless protected diagnostics explicitly require them.

## 85. Startup Metrics

Useful local metrics include:

```text
StartupDuration
StartupStageDuration
SafeModeEntryCount
MigrationFailureCount
RecoveryItemCount
UncleanShutdownCount
StorageLockConflictCount
```

## 86. Startup History

Chronicle MAY retain bounded safe startup history for diagnostics.

It is not Campaign history.

## 87. First-Run Experience

After successful first initialization, Chronicle opens onboarding or the empty Campaign library.

Onboarding does not run before storage readiness.

## 88. First-Run Provider Setup

Provider credential setup is optional to complete first-run startup.

The user may browse or configure local features without it.

## 89. Startup and Installer

The installer places binaries only.

Startup owns:

- data initialization;
- migration;
- compatibility;
- recovery.

## 90. Startup and Portable ZIP

Portable ZIP startup follows the same data-directory and migration flow.

It must not assume application-directory write access.

## 91. Upgrade Detection

Startup compares:

- application version;
- storage schema version;
- package versions;
- contract versions.

Application-version change alone does not imply migration.

## 92. Downgrade

If an older application cannot safely read current storage:

- startup blocks normal mode;
- no automatic downgrade migration runs;
- Safe Mode provides guidance.

## 93. Shutdown Symmetry

Successful startup activates resources in a known order.

Shutdown deactivates them in reverse-safe order.

## 94. Graceful Shutdown

Recommended shutdown flow:

```text
stop new commands
stop worker claims
cancel safe in-flight operations
finish short commits
persist window state
record clean shutdown
flush logs
release data lock
dispose host
```

## 95. Clean Shutdown Record

The clean-shutdown marker is written only after authoritative operations are stable and workers have stopped.

## 96. Forced Termination

Forced termination leaves recovery evidence for next startup.

The next startup must not assume prior in-memory notifications completed.

## 97. Testing Strategy

Startup requires:

```text
Unit Tests
State-Machine Tests
Integration Tests
Migration Tests
Crash Recovery Tests
Safe Mode Tests
UI Tests
Platform Tests
Security Tests
Architecture Tests
```

## 98. State-Machine Tests

Tests MUST verify:

- allowed transitions;
- forbidden transitions;
- terminal states;
- fallback to Safe Mode;
- cancellation;
- restart requirement.

## 99. First-Run Tests

Tests MUST cover:

- no data directory;
- directory creation;
- database initialization;
- initial package registration;
- no provider credential;
- ready workspace.

## 100. Restart Tests

Tests MUST cover:

- clean restart;
- unclean restart;
- pending Work Item;
- expired lease;
- completed operation with stale Work Item status;
- interrupted provider continuation.

## 101. Migration Tests

Tests MUST cover:

- migration required;
- checkpoint success;
- migration success;
- migration failure;
- insufficient disk space;
- future storage version;
- unsupported migration path.

## 102. Lock Tests

Tests MUST cover:

- one process owns directory;
- second process blocked;
- stale lock;
- inaccessible lock;
- lock release on graceful shutdown.

## 103. Safe Mode Tests

Tests MUST prove Safe Mode starts with:

- invalid provider configuration;
- unavailable credential store;
- failed package activation;
- migration failure;
- storage integrity warning;
- workers disabled.

## 104. Security Tests

Tests MUST prove:

- invalid path cannot escape approved root;
- malicious configuration cannot register arbitrary types;
- provider is not called during bootstrap;
- secrets are not logged;
- unsupported package code does not execute before validation;
- a second process cannot mutate the same data directory.

## 105. UI Tests

Tests SHOULD cover:

- progress states;
- visible stage failure;
- keyboard navigation;
- Safe Mode actions;
- retry;
- restart request;
- reference code;
- no false empty-library display.

## 106. Required Test Cases

Tests MUST cover:

- clean first run;
- normal ready startup;
- forced Safe Mode;
- invalid data path;
- data-directory lock conflict;
- corrupt configuration;
- missing expected database;
- confirmed first-run database creation;
- future database version;
- migration success;
- migration failure;
- checkpoint available;
- failed integrity check;
- missing Rule Set package;
- duplicate package;
- credential store unavailable;
- provider disabled;
- Rule Knowledge rebuild required;
- pending Work Item recovery;
- expired lease reclaim;
- unsupported Work Item payload;
- worker activation;
- worker disabled in Safe Mode;
- graceful shutdown;
- crash before clean-shutdown marker;
- restart after crash.

## 107. Architecture Tests

Architecture tests MUST reject:

- workers started before startup coordinator approval;
- UI directly running migrations;
- installer code opening Campaign storage;
- provider calls during bootstrap;
- database recreation on open failure;
- normal workspace creation before storage validation;
- Safe Mode depending on normal provider activation;
- mutable storage access before data-directory ownership.

## 108. Prohibited Patterns

### 108.1 Open Main Window Then Initialize in Background

Minimum safety initialization completes first.

### 108.2 Empty Database on Open Failure

Never hide missing or damaged state.

### 108.3 Start Workers Before Recovery

Recovery classification comes first.

### 108.4 Provider Health Check as Startup Requirement

Remote availability is not required for local readiness.

### 108.5 Automatic Destructive Repair

Explicit recovery is required.

### 108.6 Safe Mode Uses Normal Service Graph

Safe Mode remains independently viable.

### 108.7 Installer Runs EF Migrations

Chronicle owns storage migration.

### 108.8 Configuration Reset Without Preservation

Invalid configuration is retained.

### 108.9 Migration Without Checkpoint

Destructive migration requires validated recovery evidence.

### 108.10 Second Process Mutates Same Data Directory

Ownership is exclusive.

## 109. Alternatives Considered

### Single Monolithic Startup Method

Rejected because stage ownership, recovery, testing, and degraded behavior would be unclear.

### Lazy Initialize Every Feature on First Use

Useful for optional features, but insufficient for storage, migration, and operation recovery.

### Always Start in Normal Mode

Rejected because some failures require a reduced recovery graph.

### Automatically Recreate Damaged State

Rejected because Chronicle must preserve user history and make destructive actions explicit.

### Separate Recovery Executable

Potentially useful later, but a built-in Safe Mode is simpler for MVP and shares validated recovery services.

## 110. Consequences

### Positive

- safer upgrades;
- clear recovery path;
- no false empty state;
- deterministic worker activation;
- testable startup stages;
- resilient optional capabilities;
- explicit Safe Mode;
- improved diagnostics;
- clean separation from installer behavior.

### Negative

- startup coordinator adds code;
- first launch may take longer during migration;
- Safe Mode requires separate UI and composition testing;
- capability modeling adds complexity;
- stage progress and cancellation require careful design.

## 111. Risks

### Startup State Machine Becomes Overcomplicated

Mitigation:

- keep stages coarse;
- use explicit contracts;
- avoid feature-specific logic in coordinator;
- delegate to stage services.

### Safe Mode Cannot Start

Mitigation:

- reduced graph;
- no provider dependency;
- dedicated tests;
- minimal persistence inspection adapter.

### Migration Leaves Unclear State

Mitigation:

- checkpoint;
- atomic publication;
- typed migration status;
- Safe Mode fallback.

### Worker Starts Too Early

Mitigation:

- explicit activation gate;
- hosted-service startup coordination;
- integration tests.

### Lock Is Incorrectly Reclaimed

Mitigation:

- platform-specific ownership checks;
- conservative policy;
- no reclaim based only on age.

## 112. Technology Spike

Before acceptance, implement:

1. startup state model;
2. startup coordinator;
3. bootstrap logger;
4. path resolution;
5. data-directory lock;
6. normal and Safe Mode composition;
7. storage inspector;
8. migration planner and checkpoint gate;
9. recovery scanner;
10. package validation;
11. capability snapshot;
12. worker activation gate;
13. recovery workspace;
14. clean-shutdown marker;
15. crash and restart integration suite.

## 113. Spike Acceptance

The spike passes when:

- no mutable storage is accessed before ownership;
- first run initializes safely;
- an expected missing database is not replaced silently;
- migration creates a validated checkpoint;
- migration failure opens Safe Mode;
- pending Work Items recover before worker activation;
- provider and credential failures degrade features without hiding Campaigns;
- Safe Mode starts without normal provider or worker services;
- the main workspace appears only after storage readiness;
- clean shutdown releases the data lock;
- crash restart preserves all committed state.

## 114. Definition of Compliance

An implementation complies when:

- startup is an explicit staged flow;
- bootstrap logging precedes normal composition;
- data-directory ownership precedes mutation;
- configuration and platform capabilities are validated;
- storage compatibility is inspected before repositories activate;
- destructive migrations require checkpoints;
- recovery classification occurs before workers start;
- optional subsystem failure produces degradation where safe;
- fatal uncertainty enters Safe Mode;
- Safe Mode is recovery-focused and independently composable;
- no state is silently reset or recreated;
- startup and shutdown are covered by integration and crash tests.

## 115. Review Triggers

This ADR must be reviewed if:

- Chronicle gains a separate worker process;
- server hosting is introduced;
- cloud synchronization affects startup;
- multiple profiles or data directories become a first-class feature;
- automatic updates coordinate restart;
- runtime plugins are loaded;
- startup duration becomes unacceptable;
- platform support expands;
- Safe Mode requires an external recovery host;
- encrypted storage introduces unlock workflow.

## 116. Deferred Decisions

Later ADRs MAY define:

- exact data-directory lock implementation;
- exact startup progress UI;
- clean-shutdown marker schema;
- automatic migration approval policy;
- read-only emergency mode;
- encrypted database unlock flow;
- multiple profile selection;
- recovery executable;
- startup performance budgets;
- automatic update restart coordination.

## 117. Final Decision

Chronicle will start through an explicit, staged coordinator that validates ownership, configuration, storage, migrations, recovery state, packages, and capabilities before activating normal workflows.

Background work begins only after recovery has been classified.

When Chronicle cannot prove that mutation is safe, it will enter Safe Mode rather than improvise.

The application may start with fewer capabilities.

It must never start by pretending the Campaign is fine.
