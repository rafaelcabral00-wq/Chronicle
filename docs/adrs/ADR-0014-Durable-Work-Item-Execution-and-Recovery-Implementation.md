---
id: ADR-0014
title: Durable Work Item Execution and Recovery Implementation
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
  - ADR-0012
  - ADR-0013
  - RFC-0018
  - RFC-0019
  - RFC-0020
  - RFC-0033
  - RFC-0036
  - RFC-0040
  - RFC-0042
---

> **"A long operation is not complete because a process started it. It is complete when Chronicle can prove the work reached a durable outcome."**

# Durable Work Item Execution and Recovery Implementation

## 1. Status

**Proposed**

This ADR defines Chronicle's implementation for durable background Work Items and restart-safe continuation.

The decision is:

- persist Work Items in SQLite;
- execute them through a .NET hosted background service;
- use transactional lease claiming;
- use explicit Work Item type keys and versioned payload contracts;
- keep payloads bounded and independently validated;
- preserve `OperationId`, `CorrelationId`, and Campaign scope;
- implement at-least-once execution with idempotent completion;
- require each handler to define retry, cancellation, staleness, and recovery behavior;
- use checkpoints only for meaningful durable stages;
- keep provider calls and other external waits outside database transactions;
- make authoritative publication happen through Application commands;
- expose durable status to the UI;
- recover expired leases after restart;
- move terminally failed items to a visible failed state rather than silently discarding them;
- avoid an external queue or broker in the MVP.

The decision becomes **Accepted** after a spike proves:

- enqueue and commit atomically with triggering state;
- lease claim;
- lease expiry and reclaim;
- graceful shutdown;
- forced process termination;
- restart recovery;
- provider timeout;
- rate-limit retry;
- stale-result rejection;
- cancellation before commit;
- committed-result replay;
- dead-letter-equivalent failed state;
- no duplicate Dice, progression, Memory aging, or Session finalization effects.

## 2. Context

Chronicle has workflows that cannot or should not complete synchronously inside one UI interaction.

Examples:

- narration after accepted player input;
- narration after a persisted Dice Roll;
- Session finalization;
- Archivist analysis;
- Campaign generation;
- backup creation;
- restore validation;
- Rule Knowledge indexing;
- package migration;
- integrity checks;
- diagnostics bundle creation.

These workflows may encounter:

- provider latency;
- rate limits;
- process shutdown;
- application crash;
- network failure;
- stale Campaign state;
- user cancellation;
- retryable infrastructure failure;
- ambiguous provider completion;
- a successful local commit followed by failed UI notification.

An in-memory task queue would lose work after restart.

An external message broker would add deployment and operational complexity beyond the MVP.

RFC-0019 defines background operations and scheduling.

ADR-0004 defines durable `WorkItems`, attempts, checkpoints, and leases.

ADR-0013 defines command dispatch and durable continuation.

This ADR selects the concrete worker model.

## 3. Decision Drivers

The Work Item implementation prioritizes:

1. restart-safe continuation;
2. at-least-once execution;
3. idempotent authoritative effects;
4. local deployment simplicity;
5. bounded concurrency;
6. visible recovery;
7. short transactions;
8. typed retry policy;
9. cancellation semantics;
10. testability;
11. privacy;
12. no external broker dependency.

## 4. Decision Summary

Chronicle will use:

```text
Durable Queue
    SQLite WorkItems table

Worker Host
    .NET BackgroundService

Claiming
    transactional lease

Execution Guarantee
    at least once

Authoritative Completion
    Application command with OperationId

Payload
    versioned bounded JSON
    validated before execution

Retry
    typed policy
    bounded attempts
    scheduled NextAttemptAtUtc
    jitter where appropriate

Recovery
    expired lease reclaim
    restart scan
    explicit RecoveryRequired state

Terminal Failure
    visible FailedTerminal state
    no silent deletion

Concurrency
    bounded global worker count
    one mutating operation per Campaign

External Calls
    outside database transaction

UI
    reads durable operation and Work Item status
```

## 5. Work Item Definition

A Work Item represents durable continuation of one known workflow stage.

It is not a generic arbitrary job.

Examples:

```text
NarrativeContinuation
ArchivistFinalization
CampaignGeneration
BackupCreation
RestoreValidation
RuleKnowledgeIndexBuild
PackageMigration
IntegrityCheck
DiagnosticsBundleCreation
```

## 6. Work Item Versus Operation

`OperationId` represents the user or system intention.

`WorkItemId` represents one durable execution unit within that intention.

One Operation may have:

- one Work Item;
- several sequential Work Items;
- several independent child Work Items.

## 7. Work Item Type Key

Every Work Item type uses a stable semantic key.

Examples:

```text
chronicle.work.narrative-continuation
chronicle.work.session-finalization
chronicle.work.rule-index-build
chronicle.work.backup-create
```

Display names are separate.

## 8. Work Item Record

A Work Item SHOULD contain:

```text
WorkItemId
OperationId
ParentWorkItemId when applicable
CorrelationId
CampaignId when applicable
WorkType
PayloadContractVersion
PayloadJson
Status
Priority
AttemptCount
MaximumAttempts
NextAttemptAtUtc
LeaseOwner
LeaseAcquiredAtUtc
LeaseExpiresAtUtc
CreatedAtUtc
StartedAtUtc
CompletedAtUtc
LastFailureCode
LastFailureAtUtc
CheckpointVersion
EntityVersion
```

## 9. Work Item Statuses

Recommended statuses:

```text
Pending
Scheduled
Claimed
Running
WaitingForRetry
WaitingForUser
WaitingForDependency
Completed
Cancelled
FailedTerminal
RecoveryRequired
Superseded
```

## 10. Status Meaning

### Pending

Ready to be considered for claim.

### Scheduled

Eligible only after `NextAttemptAtUtc`.

### Claimed

A worker owns an active lease but execution has not yet begun or persisted its running transition.

### Running

The handler is executing.

### WaitingForRetry

A retryable failure occurred.

### WaitingForUser

The workflow requires explicit user action.

### WaitingForDependency

Another durable operation must complete first.

### Completed

The intended durable stage reached its accepted outcome.

### Cancelled

Cancellation was accepted before authoritative completion.

### FailedTerminal

No automatic retry remains.

### RecoveryRequired

State is ambiguous or requires guided recovery.

### Superseded

A newer accepted workflow made this item irrelevant.

## 11. Queue Storage

The SQLite `WorkItems` table is the authoritative queue.

In-memory channels MAY wake workers, but they are only accelerators.

A lost wake signal must not lose durable work.

## 12. Enqueue Boundary

When a Work Item is required for correctness, enqueue occurs in the same transaction as the triggering authoritative state.

Examples:

- player Message accepted and narration continuation enqueued;
- Roll persisted and narration continuation enqueued;
- Session marked finalizing and Archivist Work Item enqueued.

## 13. Optional Follow-Up

Nonessential work MAY be enqueued post-commit.

If its loss would violate correctness or leave the workflow incomplete, it is not optional and must be committed atomically.

## 14. Hosted Service

Chronicle will use a .NET `BackgroundService` inside the desktop process.

The service:

- scans or waits for eligible Work Items;
- claims leases;
- resolves handlers;
- executes bounded work;
- renews leases when appropriate;
- records outcome;
- wakes the UI through safe post-commit notification.

## 15. Desktop Process Ownership

The active Chronicle desktop process owns Work Item execution for the selected data directory.

The one-instance data-directory lock prevents competing application workers.

Database lease correctness still remains necessary for crash recovery and testing.

## 16. No Separate Worker Process in MVP

The MVP does not launch a separate worker service.

A separate process may be introduced later for:

- local model hosting;
- crash isolation;
- long jobs independent from UI lifetime;
- multi-user server deployment.

## 17. Worker Identity

Each process execution creates an opaque `WorkerInstanceId`.

The lease owner MAY contain:

```text
ApplicationInstanceId
WorkerInstanceId
```

It MUST NOT contain hostname, username, or machine identity in portable records.

## 18. Claim Query

A worker may claim a Work Item when:

- status is eligible;
- `NextAttemptAtUtc` is due;
- no valid lease exists;
- dependencies are satisfied;
- cancellation is not pending;
- Campaign mutation policy allows execution.

## 19. Transactional Claim

Claiming MUST be atomic.

Conceptually:

1. select one eligible candidate;
2. update lease owner and expiry with expected version;
3. increment or prepare attempt metadata;
4. commit;
5. only the successful updater executes.

## 20. SQLite Claim Strategy

The implementation MAY use:

- an immediate write transaction;
- compare-and-update by `EntityVersion`;
- update with eligibility predicate;
- affected-row verification.

The exact SQL or EF Core mapping is an implementation detail.

## 21. Lease Duration

Lease duration is configured per Work Item type or workload class.

It must be:

- long enough to avoid unnecessary reclaim;
- short enough to recover after crash;
- renewable for legitimately long stages.

Exact values require measurement.

## 22. Lease Renewal

A running handler MAY renew its lease periodically.

Renewal:

- updates only lease metadata;
- uses expected owner and version;
- stops when cancellation or ownership loss is detected.

## 23. Lease Ownership Loss

If a worker cannot renew or finds the lease is no longer its own:

- it must stop before authoritative publication where possible;
- any late external result is treated as potentially stale;
- completion must still pass OperationId and version checks;
- it must not overwrite another worker's status.

## 24. Lease Expiry

An expired lease does not prove the prior worker did nothing.

Therefore reclaimed execution follows at-least-once semantics and idempotent completion rules.

## 25. Attempt Record

Each execution attempt SHOULD create a `WorkItemAttempt`.

Recommended fields:

```text
WorkItemAttemptId
WorkItemId
AttemptNumber
WorkerInstanceId
StartedAtUtc
FinishedAtUtc
Outcome
FailureCode
RetryClassification
ProviderRequestId when safe
Duration
```

Attempt records contain safe metadata only.

## 26. Attempt Increment

Attempt count increments when execution actually begins, not merely when a scanner observes the item.

## 27. Maximum Attempts

Every Work Item type MUST declare:

- maximum automatic attempts;
- retryable failure classes;
- backoff policy;
- terminal-failure behavior.

Unbounded retry is prohibited.

## 28. Retry Classification

Recommended classifications:

```text
ImmediateSafeRetry
TransientBackoff
RateLimited
DependencyUnavailable
ConfigurationRequired
CredentialRequired
UserDecisionRequired
StaleInput
TerminalInvalid
AmbiguousCompletion
Cancelled
```

## 29. Automatic Retry

Automatic retry is allowed only when:

- the failure class permits it;
- attempt limit remains;
- the same OperationId is preserved;
- authoritative effects remain idempotent;
- retry does not regenerate prohibited randomness;
- the payload remains valid and current.

## 30. Backoff

Transient retries SHOULD use bounded exponential backoff with jitter.

Provider-supplied retry timing SHOULD be honored when compatible with local limits.

## 31. Rate Limit

Rate-limited Work Items transition to `WaitingForRetry` with a due time.

They do not keep:

- a database transaction;
- Campaign mutation lock;
- active UI modal;
- worker thread blocked for the whole delay.

## 32. Credential Failure

Missing or rejected credentials transition to:

```text
WaitingForUser
```

or:

```text
FailedTerminal
```

according to the workflow.

The UI presents a credential repair action.

## 33. Configuration Failure

An incompatible provider profile, missing package, or invalid setting requires user action.

Automatic retry should not loop until configuration changes.

## 34. Stale Input

If the Campaign or Session changed since the Work Item was created:

- reject stale authoritative publication;
- determine whether the item is superseded;
- rebuild a new Work Item only through explicit Application logic;
- never silently apply output to a different state.

## 35. Dependency Model

A Work Item MAY depend on another durable result.

Dependencies SHOULD use explicit references such as:

```text
DependsOnOperationId
DependsOnWorkItemId
RequiredResultType
```

## 36. Dependency Completion

A dependent item becomes eligible only when the required result is committed.

Dependency cycles MUST be detected and rejected.

## 37. Payload

The payload contains only the minimum durable input needed to resume.

It SHOULD reference authoritative entities rather than duplicate large Campaign state.

## 38. Payload Rules

Every payload MUST have:

- Work Type owner;
- contract version;
- parser;
- validator;
- size limit;
- migration policy;
- test fixture;
- data-classification review.

## 39. Payload Contents

A payload MAY contain:

- CampaignId;
- SessionId;
- SceneId;
- OperationId;
- expected versions;
- provider profile ID;
- Rule Set package version;
- source references;
- bounded workflow options.

## 40. Payload Prohibitions

Payloads MUST NOT contain:

- credentials;
- full Campaign graph;
- unrestricted transcript copy;
- raw provider response unless a narrowly approved staging contract requires it;
- UI control state;
- repositories;
- file handles;
- provider SDK objects;
- unvalidated arbitrary type metadata.

## 41. Payload Serialization

Use versioned `System.Text.Json` contracts.

Polymorphism must use an explicit allowlist.

Type-name-based arbitrary deserialization is prohibited.

## 42. Payload Migration

A Work Item created by an older application version may need payload migration.

Migration MUST be:

- explicit;
- versioned;
- idempotent;
- tested;
- safe before handler execution.

## 43. Unsupported Payload Version

If no safe migration exists:

- do not execute;
- mark `RecoveryRequired` or `FailedTerminal`;
- preserve the original payload;
- provide diagnostics and recovery guidance.

## 44. Checkpoints

Checkpoints are used only when a Work Item has meaningful restartable stages.

Examples:

- index source inspection completed;
- backup snapshot created;
- restore archive extracted and validated;
- migration batch N completed;
- provider response received and staged.

## 45. Checkpoint Model

A checkpoint SHOULD contain:

```text
WorkItemId
CheckpointKey
CheckpointVersion
PayloadContractVersion
CheckpointPayload
CreatedAtUtc
SourceAttemptNumber
ContentHash
```

## 46. Checkpoint Authority

A checkpoint is durable workflow state.

It is not automatically Campaign truth.

Authoritative Campaign state changes only through the Application command pipeline.

## 47. Provider Response Staging

A validated but not yet accepted provider response MAY be stored in a bounded staging record when required for crash recovery.

The staging record MUST:

- remain nonauthoritative;
- reference OperationId;
- preserve contract version;
- record expected Campaign and Session versions;
- be independently revalidated before commit;
- follow privacy classification and retention rules.

## 48. Raw Provider Payload

Raw provider payload persistence is discouraged.

Prefer storing the parsed provider-neutral proposal required for continuation.

## 49. Authoritative Publication

A Work Item handler does not mutate Campaign tables directly.

It dispatches a typed Application command for authoritative publication.

Examples:

```text
AcceptNarrativeResponseCommand
ApplyFinalizationProposalCommand
PublishGeneratedCampaignPlanCommand
CompleteBackupCommand
PublishRuleKnowledgeIndexCommand
```

## 50. Completion Atomicity

When authoritative publication succeeds, Work Item completion and Operation Record outcome SHOULD be committed atomically when practical.

If completion status is written afterward, replay must safely detect the already committed result.

## 51. At-Least-Once Execution

Chronicle assumes a Work Item handler may run more than once.

Therefore:

- handlers must be replay-safe;
- publication uses OperationId;
- unique constraints prevent duplicate effects;
- external calls are not assumed idempotent;
- accepted local effects are authoritative.

## 52. External Call Duplication

A provider call may be repeated after crash if completion was not durably staged.

This is acceptable only when:

- the repeated response is advisory;
- cost is acknowledged;
- authoritative publication remains idempotent;
- Dice or other Chronicle-owned randomness is not repeated.

## 53. Avoiding Duplicate Provider Calls

Where beneficial, Chronicle MAY persist safe provider request metadata or a validated proposal checkpoint before proceeding.

Provider-specific remote conversation state is not authoritative.

## 54. Dice Continuation

A narration continuation after a Roll references the persisted `DiceRollId`.

Retry never executes the Roll again.

The handler queries the existing result and asks the provider to continue from it.

## 55. Finalization

Session finalization MUST guarantee once-only:

- Memory aging;
- progression Awards;
- Relationship updates;
- Knowledge updates;
- Session completion.

The Work Item may retry analysis, but Application publication uses the same OperationId and unique constraints.

## 56. Backup Creation

Backup Work Items SHOULD checkpoint:

- target validation;
- snapshot completion;
- manifest completion;
- integrity validation;
- publication.

A partially written backup is not published as valid.

## 57. Restore Validation

Restore validation operates on isolated storage.

It does not replace active storage until an explicit commit command is approved.

## 58. Rule Knowledge Indexing

Index builds use staging and atomic publication.

The active index remains available while rebuild executes.

## 59. Cancellation Request

Cancellation is a durable state request.

Recommended fields:

```text
CancellationRequestedAtUtc
CancellationRequestedBy
CancellationReasonKey
```

Free-form cancellation reasons should be bounded or avoided.

## 60. Cancellation Handling

A handler checks cancellation:

- before external work;
- between meaningful stages;
- before authoritative publication;
- during long local loops.

## 61. Cancellation Before Commit

The Work Item transitions to `Cancelled` when no authoritative effect was committed.

## 62. Cancellation After Commit

If authoritative publication already committed:

- the Work Item completes or reports `CancellationRequestedButCommitted`;
- committed state is not rolled back automatically.

## 63. Non-Cancellable Stage

A handler MAY declare a short non-cancellable commit stage.

The UI must show that cancellation is pending or no longer possible.

## 64. Application Shutdown

On graceful shutdown:

1. stop claiming new items;
2. request cancellation for cancellable in-flight local work where appropriate;
3. allow short commit stages to finish;
4. release or allow leases to expire;
5. flush logs;
6. exit within a bounded timeout.

## 65. Forced Shutdown

A crash or forced termination may leave leases active until expiry.

Restart recovery reclaims them safely.

## 66. Startup Recovery

On startup, Chronicle SHOULD:

1. acquire the data-directory lock;
2. inspect Work Item schema and payload versions;
3. mark obviously invalid states;
4. reclaim expired leases;
5. resume eligible work;
6. expose `WaitingForUser`, `FailedTerminal`, and `RecoveryRequired` items to the UI.

## 67. Recovery Scan

The recovery scan MUST be bounded and indexed.

It should not load every completed historical Work Item.

## 68. Orphan Detection

Chronicle SHOULD detect:

- Work Item references missing Operation Record;
- missing Campaign;
- missing Session;
- unsupported handler;
- dependency cycle;
- completed operation with noncompleted Work Item;
- committed result with expired running lease.

## 69. Orphan Handling

Orphans transition to:

```text
RecoveryRequired
```

or:

```text
Superseded
```

after explicit deterministic inspection.

They are not deleted automatically.

## 70. Terminal Failure

When automatic recovery is exhausted:

- status becomes `FailedTerminal`;
- last safe failure code is preserved;
- Operation status is updated;
- UI exposes recovery actions;
- payload and attempts remain available under retention policy.

## 71. Dead-Letter Equivalent

Chronicle does not need a separate broker dead-letter queue.

`FailedTerminal` and `RecoveryRequired` records serve the equivalent purpose locally.

## 72. Retry by User

A user-triggered retry SHOULD:

- reuse the same OperationId when continuing the same intention;
- create a new Work Item or reset eligibility through an explicit command;
- preserve attempt history;
- require repaired configuration where relevant.

## 73. Start New Intention

When the user chooses a materially different action:

- create a new OperationId;
- supersede the old Work Item where appropriate;
- preserve historical failure records.

## 74. Priority

The MVP MAY support simple priority levels:

```text
Interactive
Normal
Maintenance
Low
```

## 75. Priority Semantics

Interactive continuation may outrank maintenance.

Priority MUST NOT starve maintenance indefinitely.

## 76. Scheduling

`NextAttemptAtUtc` supports delayed retries.

Cron-like recurring scheduling is outside this ADR.

## 77. Worker Concurrency

Worker concurrency is bounded.

Initial recommendation:

- small global worker count;
- one active Campaign-mutating Work Item per Campaign;
- separate allowance for read-only maintenance where safe.

Exact values require measurement.

## 78. Campaign Coordination

Before authoritative Campaign publication, the handler uses the Application command pipeline's Campaign mutation coordination.

Long external work does not hold that lock.

## 79. Global Work

Global maintenance such as package migration or restore publication may require a global mutation key and may block Campaign mutation temporarily.

## 80. Fairness

The scheduler SHOULD avoid one repeatedly failing item monopolizing execution.

Backoff and due-time ordering provide basic fairness.

## 81. Candidate Ordering

Eligible candidates SHOULD be ordered deterministically by:

```text
Priority
NextAttemptAtUtc
CreatedAtUtc
WorkItemId
```

## 82. Polling and Wake Signals

The worker MAY combine:

- in-memory wake channel after enqueue;
- short adaptive polling;
- startup scan.

Correctness does not depend on wake delivery.

## 83. Poll Interval

Polling should balance:

- interactive continuation latency;
- idle CPU use;
- battery use;
- database load.

Exact intervals require measurement.

## 84. UI Status

The UI reads durable status through query models.

Recommended visible fields:

```text
Operation type
Current stage
Status
Started time
Last update
Can cancel
Can retry
Requires action
Safe error
Reference code
```

## 85. Global Operation Area

Long-running Work Items remain visible after navigation.

Critical failures persist until acknowledged or resolved.

## 86. Notifications

Desktop notifications MAY announce completion when the application is unfocused.

They must contain minimal private information.

## 87. Logging

Work Item logs MAY include:

- WorkItemId;
- OperationId;
- WorkType;
- attempt;
- status transition;
- lease result;
- duration;
- retry classification;
- safe failure code;
- CampaignId where safe.

They MUST NOT include payload JSON or Campaign narrative.

## 88. Metrics

Useful metrics include:

```text
PendingWorkItemCount
OldestPendingAge
WorkItemExecutionDuration
WorkItemRetryCount
LeaseReclaimCount
TerminalFailureCount
RecoveryRequiredCount
CancellationCount
StaleResultCount
```

## 89. Health

Worker health MAY be:

```text
Healthy
Degraded
Paused
BlockedByMigration
BlockedByConfiguration
Failed
```

## 90. Worker Pause

Workers SHOULD pause during:

- storage migration;
- restore publication;
- database integrity repair;
- application Safe Mode when mutation is disabled.

## 91. Retention

Completed Work Items and attempts require bounded retention.

Retention must preserve enough evidence for:

- idempotency;
- recovery;
- support;
- audit.

The exact period depends on Operation Record retention and backup policy.

## 92. Deletion

Deleting old completed Work Items must not remove:

- Operation Records required for idempotency;
- Domain history;
- audit records;
- required provider usage metadata.

## 93. Payload Privacy

Payloads may contain private identifiers and workflow parameters.

They remain local and follow database backup policy.

Restricted content should be referenced rather than duplicated when practical.

## 94. Backup

Backups MUST include active and recoverable Work Items if they are part of the authoritative installation state.

## 95. Restore

After restore:

- active leases from the source installation are treated as expired or invalid;
- Work Items undergo recovery inspection;
- external credentials may be unresolved;
- pending work does not execute until dependencies and configuration are valid.

## 96. Clock Use

Lease and retry calculations use an injected clock.

UTC timestamps are persisted.

## 97. Clock Regression

The implementation must tolerate system-clock regression.

Lease ownership also uses owner identity and entity version, not time alone.

## 98. Handler Registry

Every Work Type maps to exactly one handler and payload contract.

Missing or duplicate handlers are startup configuration errors.

## 99. Handler Interface

Conceptually:

```csharp
public interface IWorkItemHandler<TPayload>
{
    Task<WorkItemExecutionResult> ExecuteAsync(
        WorkItemExecutionContext context,
        TPayload payload,
        CancellationToken cancellationToken);
}
```

## 100. Execution Result

Recommended result categories:

```text
Completed
RetryAfter
WaitingForUser
WaitingForDependency
Cancelled
Superseded
FailedTerminal
RecoveryRequired
```

## 101. Handler Context

The execution context SHOULD provide:

- WorkItemId;
- OperationId;
- CorrelationId;
- attempt;
- lease owner;
- checkpoint service;
- command dispatcher;
- query dispatcher;
- clock;
- safe logger;
- cancellation token.

It MUST NOT expose a global service provider.

## 102. Handler Dependencies

Handlers use constructor injection.

They may depend on:

- provider-neutral Narrative Intelligence;
- filesystem abstractions;
- backup service;
- index builder;
- command/query dispatchers;
- package registry;
- checkpoint service.

## 103. Direct Persistence

Handlers MAY update their own Work Item checkpoint and status through approved infrastructure.

They MUST NOT directly mutate Campaign authoritative tables.

## 104. Error Mapping

Unexpected exceptions are caught at the worker boundary.

They map to a typed retry or terminal result through the handler policy.

The worker must not crash permanently because one item failed.

## 105. Poison Work Item

A repeatedly crashing item becomes `FailedTerminal` after its bounded attempts.

It must not block later eligible work.

## 106. Process-Level Failure

If the worker service itself fails unexpectedly:

- the Host logs a Critical event;
- the UI reports degraded background processing;
- Chronicle may restart the worker service or require application restart;
- Campaign browsing remains available where safe.

## 107. Testing Strategy

The Work Item system requires:

```text
Unit Tests
Repository Contract Tests
SQLite Integration Tests
Concurrency Tests
Crash Recovery Tests
Provider Failure Tests
Cancellation Tests
Migration Tests
Security Tests
End-to-End Tests
```

## 108. Unit Tests

Unit tests SHOULD cover:

- status transitions;
- retry classification;
- backoff;
- due-time calculation;
- payload validation;
- dependency logic;
- cancellation decisions;
- candidate ordering.

## 109. SQLite Integration Tests

Tests MUST prove:

- enqueue atomicity;
- transactional claim;
- single lease winner;
- lease renewal;
- lease expiry;
- reclaim;
- optimistic concurrency;
- attempt history;
- status persistence;
- completed-result replay.

## 110. Crash Recovery Tests

Tests SHOULD terminate the worker process at stages such as:

- after claim;
- during provider call;
- after provider response staging;
- before Application commit;
- after Application commit;
- before Work Item completion update.

After restart, expected outcomes must be deterministic and duplicate-free.

## 111. Provider Failure Tests

Tests MUST cover:

- timeout;
- connection failure;
- rate limit;
- authentication failure;
- quota exhaustion;
- malformed output;
- refusal;
- stale response;
- successful response with local commit failure.

## 112. Cancellation Tests

Tests MUST cover:

- cancel pending;
- cancel scheduled;
- cancel during provider call;
- cancel during local parsing;
- cancel before commit;
- cancel after commit begins;
- repeated cancellation;
- cancellation after completion.

## 113. Migration Tests

Tests MUST cover Work Items created under prior payload versions.

## 114. Security Tests

Tests MUST prove:

- payload type allowlist;
- malicious polymorphic type rejected;
- oversized payload rejected;
- credentials absent;
- cross-Campaign references rejected;
- Work Item cannot invoke arbitrary command type;
- logs exclude payload content;
- restored foreign lease is not trusted.

## 115. Required Test Cases

Tests MUST cover:

- enqueue;
- duplicate WorkItemId;
- duplicate OperationId continuation;
- wake signal lost;
- worker startup scan;
- two workers race for claim;
- lease renewal;
- lease owner mismatch;
- lease expiry;
- retry backoff;
- maximum attempts;
- terminal failure;
- user retry;
- superseded item;
- dependency completion;
- dependency cycle;
- stale Campaign version;
- no provider transaction;
- Roll continuation without reroll;
- finalization retry without duplicate aging;
- backup interrupted;
- index publication after rebuild;
- unsupported payload version;
- application shutdown;
- forced process termination;
- restore recovery scan;
- retention cleanup.

## 116. Architecture Tests

Architecture tests MUST reject:

- in-memory-only queue as authoritative implementation;
- Work Item handlers directly mutating Campaign repositories;
- provider calls inside database transactions;
- unbounded retry loops;
- arbitrary type-name payload deserialization;
- credentials in Work Item payloads;
- UI owning worker execution;
- service locator use in handlers;
- missing Work Type registration;
- multiple handlers for one Work Type.

## 117. Prohibited Patterns

### 117.1 Fire-and-Forget Task for Required Work

Correctness-required continuation must be durable.

### 117.2 Lease Means Exactly Once

Leases provide ownership windows, not exactly-once guarantees.

### 117.3 Handler Mutates Campaign Directly

Authoritative state changes use Application commands.

### 117.4 Retry Regenerates Dice

Persisted Chronicle randomness is reused.

### 117.5 Infinite Retry

All policies are bounded.

### 117.6 Large Campaign Snapshot in Payload

Payloads use references and expected versions.

### 117.7 Raw Provider Response as Permanent Queue Data

Store only bounded validated staging data when needed.

### 117.8 External Broker in MVP

SQLite is sufficient for the local modular monolith.

### 117.9 Silent Terminal Failure

Failures remain visible and recoverable.

### 117.10 In-Memory Wake Signal as Durability

Wake signals only reduce latency.

## 118. Alternatives Considered

### In-Memory Channel Only

Rejected because required work would be lost on shutdown or crash.

### Quartz.NET

Useful for recurring scheduling, but Chronicle's main need is durable workflow continuation with OperationId, leases, and Application publication rather than calendar scheduling.

### Hangfire

Provides durable jobs and dashboards, but introduces a framework model and storage conventions broader than the MVP needs.

Chronicle's Work Item semantics are closely tied to Campaign versions, provider recovery, and Application commands.

### External Broker

RabbitMQ, Azure Service Bus, Kafka, and similar systems are inappropriate for a local single-user desktop MVP.

### Separate Windows Service

Rejected initially because it complicates installation, privileges, process coordination, and user-data ownership.

## 119. Consequences

### Positive

- restart-safe workflows;
- no external infrastructure;
- visible operation state;
- bounded recovery;
- duplicate-safe authoritative effects;
- good fit with SQLite and the desktop host;
- deterministic testing;
- clear provider and transaction boundaries.

### Negative

- Chronicle must implement lease and retry infrastructure;
- at-least-once behavior requires disciplined handlers;
- desktop shutdown may delay work;
- provider calls may repeat after ambiguous failure;
- payload migration adds maintenance;
- long jobs do not continue while the application is closed.

## 120. Risks

### Duplicate External Calls

Mitigation:

- staged validated proposals where valuable;
- OperationId;
- no duplicate authoritative effects;
- provider request metadata;
- explicit cost awareness.

### Lease Bugs

Mitigation:

- transactional claim;
- concurrency tests;
- owner/version checks;
- short, reviewed implementation.

### Payload Drift

Mitigation:

- versioned contracts;
- migration fixtures;
- unsupported-version recovery.

### Worker Starvation

Mitigation:

- priority with fairness;
- backoff;
- bounded concurrency;
- metrics.

### Application Closed During Long Work

Mitigation:

- durable state;
- restart recovery;
- visible pending status;
- later separate-process review if necessary.

## 121. Technology Spike

Before acceptance, implement:

1. Work Item schema;
2. attempt schema;
3. checkpoint schema;
4. handler registry;
5. `BackgroundService`;
6. transactional claim;
7. lease renewal;
8. retry scheduler;
9. cancellation request;
10. provider continuation handler;
11. Application publication command;
12. crash injection points;
13. startup recovery scan;
14. UI operation-status query;
15. end-to-end persisted Roll continuation.

## 122. Spike Acceptance

The spike passes when:

- required Work Item enqueue is atomic with triggering state;
- only one worker claims an item at a time;
- expired work is reclaimed after restart;
- a provider call occurs outside SQLite transaction;
- a crash after Roll commit never causes reroll;
- a crash after finalization commit never duplicates aging or Awards;
- terminal failures remain visible;
- user retry reuses the correct OperationId;
- unsupported payload versions do not execute;
- no credential or narrative payload appears in logs;
- the worker can be replaced by a deterministic test runner.

## 123. Definition of Compliance

An implementation complies when:

- Work Items are persisted in SQLite;
- a .NET hosted service executes them;
- claims use transactional leases;
- execution is treated as at least once;
- authoritative publication uses Application commands;
- OperationId preserves idempotency;
- external calls occur outside transactions;
- retries are typed and bounded;
- payloads are versioned, validated, and bounded;
- cancellation is durable and stage-aware;
- expired leases recover after restart;
- terminal failures are visible;
- duplicate Dice, progression, finalization, and Memory aging effects are impossible.

## 124. Review Triggers

This ADR must be reviewed if:

- Chronicle needs work to continue while the desktop app is closed;
- a local model process becomes a durable worker;
- multiplayer or server hosting is introduced;
- multiple Chronicle processes share one database;
- external synchronization requires a broker;
- Work Item volume outgrows SQLite;
- recurring schedules become a core requirement;
- provider background APIs become authoritative workflow dependencies;
- process isolation becomes necessary for reliability.

## 125. Deferred Decisions

Later ADRs MAY define:

- exact lease durations;
- exact polling strategy;
- exact global and per-Campaign concurrency;
- payload compression;
- Work Item retention periods;
- separate worker process;
- recurring scheduler;
- remote broker integration;
- provider background-response reconciliation;
- local model job execution;
- operation-history UI.

## 126. Final Decision

Chronicle will execute required background work through durable SQLite-backed Work Items processed by a bounded .NET hosted service.

Work will be leased, retried, recovered, and completed under explicit contracts.

Handlers may perform external analysis.

Only Application commands may publish authoritative Campaign changes.

A process may stop.

The Campaign's unfinished work must still know how to continue.
