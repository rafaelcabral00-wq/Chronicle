---
id: RFC-0019
title: Durable Work Items, Scheduling, Leasing, Retry, and Canonical Lifecycle
status: Draft
version: 0.2.0
owner: Chronicle Team
last_updated: 2026-08-02
category: Application
supersedes:
  - RFC-0019@0.1.0
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
implements: []
related_to:
  - ADR-0013
  - ADR-0014
  - ADR-0024
  - ADR-0034
  - ADR-0036
  - ADR-0038
  - ADR-0041
---

> **"A Work Item schedules effort. It does not own truth, and it does not replace the Operation whose intent it serves."**

# Durable Work Items, Scheduling, Leasing, Retry, and Canonical Lifecycle

## 1. Status

**Draft**

This RFC defines Chronicle's canonical model for durable Work Items.

This revision resolves the lifecycle conflict between the earlier RFC and ADR-0014.

The canonical states are:

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

The decision is:

- use durable Work Items for asynchronous, delayed, retryable, restart-safe, or externally dependent Application work;
- keep Work Item and Operation Record as separate concepts;
- let Operation Record own logical intent and authoritative result;
- let Work Item own one schedulable execution unit;
- permit one Operation to create several Work Items;
- persist Work Item state transitions;
- use bounded claims or leases to prevent concurrent execution;
- release claims safely after crash or expiry;
- distinguish waiting for retry, user, and dependency;
- use explicit dependency references rather than polling arbitrary state;
- require idempotent handlers;
- forbid a Work Item from making an authoritative effect without an OperationId where the effect is logically idempotent;
- recover unknown outcomes through authoritative records;
- use English state keys and error keys;
- keep Work Items local-first and single-process compatible while leaving room for future worker separation;
- avoid building a general-purpose distributed job platform in the MVP.

## 2. Purpose

Chronicle has work that cannot always complete synchronously in one request.

Examples include:

- Narrative Intelligence provider calls;
- Narrator repair attempts;
- Narrative continuation after Dice;
- backup creation;
- restore validation;
- package validation;
- migration inspection;
- import staging;
- diagnostics generation;
- deferred cleanup;
- projection rebuild;
- retry after temporary failure.

Without durable Work Items, Chronicle may lose work during:

- process crash;
- operating-system restart;
- provider timeout;
- temporary filesystem failure;
- user pause;
- package unavailability;
- unknown commit outcome.

## 3. Scope

This RFC covers:

- WorkItem identity;
- canonical lifecycle;
- scheduling;
- claiming;
- leases;
- execution;
- retry;
- waiting states;
- dependency modeling;
- user waits;
- cancellation;
- supersession;
- recovery;
- retention;
- handler contracts;
- idempotency;
- diagnostics;
- tests.

## 4. Out of Scope

This RFC does not define:

- distributed multi-machine workers;
- cloud queues;
- exactly-once execution;
- server cluster ownership;
- external message brokers;
- arbitrary cron scheduling;
- general workflow scripting;
- Operation Record lifecycle;
- provider-specific retry semantics;
- Domain state machines.

## 5. Work Item Identity

Every Work Item has:

```text
WorkItemId
```

Chronicle generates it before scheduling.

## 6. Work Item Record

Recommended shape:

```text
WorkItem
├── WorkItemId
├── WorkItemTypeKey
├── WorkItemContractVersion
├── OperationId
├── ParentWorkItemId
├── CampaignId
├── NarrativeTurnId
├── ProviderAttemptId
├── State
├── Priority
├── ScheduledAtUtc
├── ClaimedBy
├── ClaimExpiresAtUtc
├── AttemptCount
├── MaxAttempts
├── RetryAfterUtc
├── DependencyReferences[]
├── UserActionReference
├── PayloadReference
├── ResultReference
├── FailureCode
├── CreatedAtUtc
├── UpdatedAtUtc
├── CompletedAtUtc
└── RowVersion
```

## 7. Work Item Type Key

Examples:

```text
narrative.provider-request
narrative.repair-request
narrative.continuation
backup.create
restore.inspect
package.validate
migration.inspect
projection.rebuild
cleanup.staging
```

Keys are invariant English identifiers.

## 8. Work Item Contract Version

Each Work Item type has a versioned payload and result contract.

## 9. Operation Relationship

A Work Item may reference:

```text
OperationId
```

The Operation owns logical intent.

The Work Item owns one execution unit.

## 10. One Operation, Several Work Items

One Operation may create Work Items for:

- initial execution;
- retry;
- continuation;
- cleanup;
- verification;
- recovery inspection.

## 11. No Operation Replacement

Work Items do not replace Operation Records.

## 12. Canonical State Machine

```text
Pending
    ↓
Scheduled
    ↓
Claimed
    ↓
Running
    ↓
Completed
```

Alternative waiting and failure paths are:

```text
WaitingForRetry
WaitingForUser
WaitingForDependency
Cancelled
FailedTerminal
RecoveryRequired
Superseded
```

## 13. Pending

The Work Item exists but is not yet eligible to execute.

## 14. Scheduled

The Work Item is eligible at or after `ScheduledAtUtc`.

## 15. Claimed

A worker owns a temporary claim.

Execution has not necessarily started.

## 16. Running

The handler is actively executing.

## 17. Waiting for Retry

Execution failed in a retryable way.

The Work Item has a future `RetryAfterUtc`.

## 18. Waiting for User

The Work Item cannot continue until explicit user action.

Examples:

- Dice release;
- confirmation;
- credential reconfiguration;
- recovery choice.

## 19. Waiting for Dependency

The Work Item waits for another durable condition.

Examples:

- package availability;
- completed Dice Roll;
- completed parent Work Item;
- migration checkpoint;
- provider profile repair.

## 20. Completed

The Work Item finished its defined execution successfully.

This does not necessarily mean the parent Operation is completed.

## 21. Cancelled

The Work Item was intentionally stopped before completion.

## 22. Failed Terminal

The Work Item cannot continue under its current contract and intent.

## 23. Recovery Required

Chronicle cannot safely determine the Work Item's outcome or next transition automatically.

## 24. Superseded

A newer Work Item or changed authoritative context makes this Work Item obsolete.

## 25. Terminal States

Terminal states are:

```text
Completed
Cancelled
FailedTerminal
Superseded
```

`RecoveryRequired` is not terminal.

## 26. Allowed Transitions

Canonical transitions include:

```text
Pending → Scheduled
Pending → Cancelled
Pending → Superseded

Scheduled → Claimed
Scheduled → WaitingForDependency
Scheduled → WaitingForUser
Scheduled → Cancelled
Scheduled → Superseded

Claimed → Running
Claimed → Scheduled
Claimed → RecoveryRequired
Claimed → Cancelled

Running → Completed
Running → WaitingForRetry
Running → WaitingForUser
Running → WaitingForDependency
Running → FailedTerminal
Running → RecoveryRequired
Running → Superseded

WaitingForRetry → Scheduled
WaitingForRetry → Cancelled
WaitingForRetry → Superseded

WaitingForUser → Scheduled
WaitingForUser → Cancelled
WaitingForUser → Superseded

WaitingForDependency → Scheduled
WaitingForDependency → Cancelled
WaitingForDependency → Superseded

RecoveryRequired → Scheduled
RecoveryRequired → Completed
RecoveryRequired → FailedTerminal
RecoveryRequired → Superseded
```

Work Item types may restrict this further.

## 27. Forbidden Transitions

Examples:

- `Completed → Running`;
- `Cancelled → Scheduled`;
- `FailedTerminal → Claimed`;
- `Superseded → Running`;
- `Pending → Running`;
- `WaitingForUser → Running` without rescheduling;
- `Running → Pending`.

## 28. State Registry

Chronicle maintains one canonical registry containing:

```text
State key
Meaning
Terminal status
Allowed predecessors
Allowed successors
Claim behavior
Retry behavior
User-visible meaning
Retention behavior
```

## 29. Work Item-Specific Detail

A Work Item type may persist typed phase detail.

It must not invent a competing root lifecycle.

## 30. Scheduling

A Work Item becomes eligible when:

- state is `Scheduled`;
- `ScheduledAtUtc` is not in the future;
- required dependencies are satisfied;
- no valid claim exists;
- execution policy allows it.

## 31. Priority

Priority influences selection order but does not override correctness.

Recommended initial values:

```text
Critical
High
Normal
Low
Maintenance
```

## 32. Deterministic Selection

Eligible Work Items are selected using deterministic ordering such as:

```text
Priority
ScheduledAtUtc
CreatedAtUtc
WorkItemId
```

## 33. No Starvation

The scheduler should include aging or equivalent policy if lower-priority work can starve.

## 34. Claiming

Before execution, the scheduler atomically claims one Work Item.

## 35. Claim Fields

```text
ClaimedBy
ClaimExpiresAtUtc
```

## 36. Claim Authority

A claim grants temporary execution ownership.

It does not grant authority to bypass Operation or Domain validation.

## 37. Claim Atomicity

Two workers must not successfully claim the same Work Item simultaneously.

## 38. Lease Duration

Claims use bounded lease duration.

## 39. Claim Renewal

Long-running handlers may renew claims before expiry.

## 40. Claim Expiry

An expired claim does not automatically prove execution failed.

Chronicle must classify the handler and inspect authoritative effects.

## 41. Local-First Worker Identity

In the MVP, `ClaimedBy` may identify:

- process instance;
- scheduler instance;
- in-process worker.

## 42. No Distributed Infrastructure Requirement

The MVP does not require Redis, RabbitMQ, Kafka, or another external broker.

## 43. Execution

After a valid claim, the Work Item moves to `Running`.

## 44. Handler Contract

Every Work Item handler declares:

```text
WorkItemTypeKey
ContractVersion
Idempotency Strategy
Operation Requirement
Timeout
Retry Policy
Dependency Policy
Cancellation Policy
Recovery Query
Result Contract
```

## 45. Idempotent Handler

Handlers must tolerate duplicate invocation.

## 46. At-Least-Once Execution Possibility

A Work Item may execute more than once after crash or lease expiry.

## 47. At-Most-Once Authoritative Effect

Any authoritative effect is protected through OperationId, unique constraints, and recovery queries.

## 48. Exactly-Once Rejection

Chronicle does not claim exactly-once handler execution.

## 49. Work Item Without Operation

Some maintenance Work Items may not require an OperationId if they have no authoritative Domain effect.

Examples:

- bounded log cleanup;
- stale staging cleanup;
- nonauthoritative cache rebuild.

## 50. Authoritative Work Requires Operation

A Work Item that may change authoritative Campaign state must reference an OperationId.

## 51. Retry

A retryable failure moves the Work Item to:

```text
WaitingForRetry
```

## 52. Retry Fields

```text
AttemptCount
MaxAttempts
RetryAfterUtc
FailureCode
```

## 53. Retry Scheduling

When eligible again:

```text
WaitingForRetry → Scheduled
```

## 54. Backoff

Automated retry uses bounded backoff.

## 55. Jitter

Jitter may be used where many concurrent retries could align.

## 56. Maximum Attempts

A Work Item type defines a maximum or policy-based attempt count.

## 57. Retry Exhaustion

After exhaustion:

```text
FailedTerminal
```

or:

```text
RecoveryRequired
```

depending on whether authoritative outcome is known.

## 58. Retry Classification

Recommended:

```text
RetryableTransient
RetryableAfterConfiguration
RetryableAfterDependency
RetryableAfterUserAction
Terminal
RecoveryInspectionRequired
```

## 59. Waiting for User

A Work Item enters `WaitingForUser` when a valid explicit action is required.

## 60. User Action Correlation

The Work Item records the expected user action type and correlation reference.

## 61. User Action Resume

Valid user action transitions the item back to `Scheduled`.

## 62. No Busy Polling for User

Chronicle does not repeatedly execute a Work Item while waiting for user input.

## 63. Waiting for Dependency

A Work Item enters `WaitingForDependency` when another durable condition must complete.

## 64. Dependency Reference

Recommended:

```text
DependencyType
DependencyId
RequiredState
FailurePolicy
```

## 65. Dependency Types

Examples:

- WorkItem;
- Operation;
- DiceRoll;
- NarrativeTurn;
- package installation;
- migration checkpoint;
- configuration repair.

## 66. Dependency Satisfaction

When the dependency reaches the required state, the item is rescheduled.

## 67. Dependency Failure

The Work Item type declares whether failure causes:

- retry;
- terminal failure;
- supersession;
- RecoveryRequired;
- user action.

## 68. No Arbitrary State Polling

Dependencies must use explicit durable references or approved queries.

## 69. Parent Work Item

A Work Item may reference a parent WorkItemId for observability and lifecycle grouping.

## 70. Parent Does Not Own Child Truth

Parent-child relationships do not create nested transaction semantics.

## 71. Completion

`Completed` means the Work Item's handler contract is satisfied.

## 72. Operation Completion

A parent Operation may remain `Committed` or otherwise incomplete after a Work Item completes.

## 73. Cancellation

A Work Item may be cancelled according to its type policy.

## 74. Cancellation Before Running

Pending or Scheduled items can usually cancel safely.

## 75. Cancellation While Running

Running cancellation is cooperative where possible.

## 76. Cancellation Does Not Undo Commit

If authoritative state already committed, cancellation stops remaining work only.

## 77. Cancellation Evidence

Chronicle records:

- requester;
- prior state;
- time;
- reason code.

## 78. Supersession

A Work Item becomes `Superseded` when its work is no longer valid.

Examples:

- a newer Narrative Turn replaced it;
- Campaign context changed;
- package version changed;
- user submitted a replacement action;
- another recovery path completed the same need.

## 79. Supersession Versus Cancellation

Cancellation expresses intentional stop.

Supersession expresses obsolescence.

## 80. Stale Context Check

Before authoritative work, the handler validates expected versions and references.

## 81. Stale Work Item

A stale item becomes:

```text
Superseded
```

or produces a typed conflict according to its policy.

## 82. Recovery Required

Use `RecoveryRequired` when:

- lease expired during uncertain execution;
- process crashed after possible commit;
- result record is inconsistent;
- dependency state is ambiguous;
- staged artifact outcome is unknown.

## 83. Recovery Query

Every authoritative Work Item type defines how to inspect:

- OperationRecord;
- result records;
- unique authoritative records;
- aggregate versions;
- artifact publication state;
- correlated Work Items.

## 84. Recovery Outcomes

Recommended:

```text
NotStarted
SafeToRetry
AlreadyCompleted
WaitingForDependency
Superseded
FailedTerminal
ManualRecoveryRequired
```

## 85. Not Started

Return to `Scheduled`.

## 86. Safe to Retry

Return to `Scheduled` with the same WorkItemId.

## 87. Already Completed

Set `Completed` and link the existing result.

## 88. Manual Recovery Required

Remain in `RecoveryRequired` and expose safe user actions.

## 89. Unknown Lease Outcome

Lease expiry alone never causes blind re-execution of an authoritative mutation.

## 90. Scheduler

Chronicle uses an in-process durable scheduler for MVP.

## 91. Scheduler Startup

On startup, it:

1. inspects RecoveryRequired items;
2. releases safely expired nonrunning claims;
3. detects interrupted Running items;
4. schedules eligible items;
5. respects Safe Mode restrictions.

## 92. Startup Recovery

Interrupted `Running` items do not automatically become `Scheduled`.

They pass through recovery classification.

## 93. Scheduler Shutdown

Graceful shutdown:

- stops claiming new work;
- requests cooperative cancellation where safe;
- persists current state;
- releases or allows claim expiry according to policy.

## 94. Safe Mode

Safe Mode executes only Work Item types explicitly permitted for recovery.

## 95. Safe Mode Allowed Work

Examples:

- backup inspection;
- restore inspection;
- migration recovery;
- diagnostics;
- staging cleanup;
- operation recovery.

## 96. Safe Mode Prohibited Work

Examples:

- new Narrator provider request;
- new gameplay mutation;
- automatic Scene continuation.

## 97. Narrative Turn Integration

Narrative orchestration may use Work Items for:

```text
NarratorProviderRequest
NarratorRepairRequest
NarratorContinuation
NarratorRecoveryInspection
```

Exact keys belong to the Work Item registry.

## 98. Provider Attempt Integration

A provider Work Item may reference:

```text
NarrativeTurnId
ProviderAttemptId
OperationId
```

## 99. Dice Integration

A continuation Work Item may wait for:

```text
DiceRoll state = Resolved
```

It must never regenerate Dice.

## 100. Backup Integration

Backup creation may use one or more Work Items for:

- staging;
- validation;
- publication;
- cleanup.

## 101. Import Integration

Future Campaign import may use Work Items for:

- inspection;
- migration;
- clone remapping;
- publication;
- cleanup.

## 102. Migration Integration

Migration recovery Work Items are restricted and must preserve checkpoint safety.

## 103. Payload Storage

Work Item payloads are versioned and bounded.

## 104. No Large Arbitrary Payload

Large prompts, raw responses, archives, or Campaign snapshots are stored by safe reference rather than embedded without limits.

## 105. Payload Privacy

Payloads must not contain raw credentials.

## 106. Result Storage

Results use typed records or safe references.

## 107. Result Idempotency

A completed Work Item returns the existing result on duplicate invocation.

## 108. Unique Constraints

Recommended:

```text
WorkItemId unique
Active claim ownership enforced atomically
Optional unique logical Work Item key per Operation phase
```

## 109. Logical Work Item Key

Some types may use:

```text
OperationId + WorkItemTypeKey + PhaseKey
```

to prevent duplicate scheduling.

## 110. Duplicate Scheduling

Duplicate scheduling returns or reuses the existing Work Item where policy requires uniqueness.

## 111. Time Source

Scheduling and lease logic use Chronicle's injected time abstraction.

## 112. Monotonic Consideration

Elapsed-time calculations should avoid relying only on wall-clock behavior where practical.

## 113. Clock Change

Large clock changes must not silently execute future work early without policy.

## 114. Priority Inversion

A high-priority item waiting for a low-priority dependency should surface that dependency appropriately.

## 115. Fairness

Scheduler selection should remain deterministic and fair enough for local workloads.

## 116. Resource Limits

The scheduler limits:

- concurrent provider calls;
- concurrent file operations;
- concurrent CPU-heavy tasks;
- total active Work Items.

## 117. Work Item Concurrency Class

Types may declare:

```text
ProviderNetwork
DatabaseMaintenance
FilePublication
CpuBound
General
```

## 118. Campaign-Level Serialization

Some Work Items may require one active mutation lane per Campaign.

## 119. No Global Serialization by Default

Unrelated Campaign-safe work should not be blocked unnecessarily.

## 120. Error Model

Recommended errors:

```text
work-item.not-found
work-item.type-unknown
work-item.version-unsupported
work-item.state-invalid
work-item.transition-invalid
work-item.claim-conflict
work-item.claim-expired
work-item.payload-invalid
work-item.operation-required
work-item.dependency-invalid
work-item.dependency-failed
work-item.retry-exhausted
work-item.cancel-not-allowed
work-item.superseded
work-item.result-missing
work-item.recovery-required
work-item.failed-terminal
```

## 121. Data Preservation State

Results should state:

```text
AuthoritativeDataUnchanged
WorkScheduled
WorkClaimed
WorkRunning
WaitingForRetry
WaitingForUser
WaitingForDependency
AuthoritativeChangeCommitted
WorkCompleted
WorkCancelled
WorkSuperseded
RecoveryRequired
```

## 122. Logging

Safe logs may include:

- WorkItemId;
- type key;
- state;
- OperationId;
- CampaignId;
- NarrativeTurnId;
- ProviderAttemptId;
- attempt count;
- claim owner;
- dependency type;
- result code;
- duration.

They must not include credentials, full prompts, raw provider responses, or full private Campaign payloads.

## 123. Metrics

Useful local metrics include:

```text
WorkItemQueueDepth
WorkItemWaitDuration
WorkItemRunDuration
WorkItemRetryCount
WorkItemRecoveryCount
WorkItemSupersededCount
WorkItemTerminalFailureCount
ExpiredClaimCount
```

No remote telemetry is required.

## 124. Retention

Retain Work Items required for:

- unresolved execution;
- Operation recovery;
- audit;
- failure diagnosis;
- dependent Work Items.

## 125. Completed Work Item Cleanup

Completed items may be compacted after:

- result remains available;
- no dependency references them;
- retry and recovery windows close;
- retention policy allows it.

## 126. No Cleanup of Unresolved Work

Do not delete:

- Scheduled;
- Claimed;
- Running;
- Waiting states;
- RecoveryRequired.

## 127. Language

State keys, type keys, error keys, and base documentation use English.

Localized UI labels remain separate.

## 128. Testing Strategy

The implementation requires:

```text
State Machine Tests
Scheduling Tests
Claim and Lease Tests
Handler Tests
Retry Tests
Dependency Tests
User Wait Tests
Cancellation Tests
Supersession Tests
Recovery Tests
Concurrency Tests
Persistence Tests
Crash Tests
```

## 129. State Machine Tests

Every allowed and forbidden transition must be tested.

## 130. Scheduling Tests

Tests cover:

- future schedule;
- eligible schedule;
- priority ordering;
- deterministic tie-break;
- fairness;
- Safe Mode restrictions.

## 131. Claim Tests

Tests cover:

- atomic claim;
- concurrent claim attempt;
- lease renewal;
- lease expiry;
- stale owner;
- shutdown.

## 132. Retry Tests

Tests cover:

- transient failure;
- backoff;
- retry exhaustion;
- configuration repair;
- same WorkItemId reuse.

## 133. User Wait Tests

Tests cover:

- entering WaitingForUser;
- restart;
- valid user action;
- wrong action;
- cancellation.

## 134. Dependency Tests

Tests cover:

- satisfied dependency;
- failed dependency;
- missing dependency;
- cyclic dependency detection;
- rescheduling.

## 135. Idempotency Tests

Duplicate execution must not duplicate authoritative effects.

## 136. Recovery Tests

Inject failure:

- before claim;
- after claim;
- during Running;
- after authoritative commit;
- before Work Item completion;
- after result publication.

## 137. Supersession Tests

Tests prove stale Narrative or Campaign work cannot execute after being superseded.

## 138. Concurrency Tests

Tests prove:

- one active claim;
- bounded concurrency class;
- Campaign mutation serialization where required.

## 139. Architecture Tests

Architecture tests must reject:

- authoritative Work Item without OperationId;
- Work Item used as a replacement for OperationRecord;
- duplicate root lifecycle enum;
- provider call in a database transaction;
- lease expiry causing blind authoritative retry;
- arbitrary busy polling for user input;
- localized state keys;
- cleanup deleting unresolved work;
- unbounded payloads.

## 140. Prohibited Patterns

### 140.1 Work Item as Domain Truth

It is scheduling evidence.

### 140.2 Work Item as Operation Record

Scheduling and logical intent are distinct.

### 140.3 Exactly-Once Handler Claim

Handlers may run more than once.

### 140.4 Blind Retry After Lease Expiry

Inspect authoritative state.

### 140.5 Busy Polling While Waiting for User

Persist a waiting state.

### 140.6 External Wait in Transaction

Release the transaction.

### 140.7 Arbitrary Dependency Polling

Use explicit durable references.

### 140.8 New WorkItemId for Every Retry

Retry the same Work Item unless policy explicitly creates a new phase item.

### 140.9 Delete Running or Recovery Work

Retain unresolved evidence.

## 141. Alternatives Considered

### In-Memory Queue Only

Rejected because work would be lost on restart.

### Use OperationRecord as Queue Item

Rejected because one Operation may need multiple execution units and waiting phases.

### External Broker in MVP

Rejected because local durable scheduling is sufficient and simpler.

### One State: Pending or Done

Rejected because claim, retry, user wait, dependency wait, supersession, and recovery have different semantics.

### New Work Item per Retry

Rejected as the default because it complicates identity and recovery. New items remain valid for distinct phases.

## 142. Consequences

### Positive

- one canonical Work Item lifecycle;
- restart-safe scheduling;
- explicit user and dependency waits;
- bounded leasing;
- safe retry;
- clear separation from Operation Record;
- reliable Narrative, backup, and migration integration;
- future worker separation remains possible.

### Negative

- more durable records;
- scheduler and lease logic require care;
- handlers need recovery queries;
- retention and cleanup are required;
- dependency graphs can become complex;
- fault-injection tests are necessary.

## 143. Risks

### Work Item Becomes a General Workflow Engine

Mitigation:

- bounded type registry;
- small canonical lifecycle;
- Application-owned handlers;
- no arbitrary scripting.

### Lease Expiry Duplicates Effects

Mitigation:

- OperationId;
- recovery query;
- unique constraints;
- no blind retry.

### Dependency Cycles

Mitigation:

- graph validation;
- bounded dependency types;
- cycle tests.

### Queue Starvation

Mitigation:

- deterministic ordering;
- aging or fairness policy;
- metrics.

### Excessive Retention

Mitigation:

- compaction after dependencies and recovery windows close.

## 144. Technology Spike

Before acceptance, implement:

1. WorkItem persistence;
2. canonical state enum;
3. transition validator;
4. in-process scheduler;
5. atomic claim;
6. lease renewal and expiry;
7. handler registry;
8. OperationId enforcement;
9. retry policy;
10. user wait resume;
11. dependency resume;
12. supersession;
13. recovery inspection;
14. bounded concurrency;
15. cleanup policy;
16. fault-injection tests;
17. architecture tests.

## 145. Spike Acceptance

The spike passes when:

- one item survives restart;
- two workers cannot own one valid claim;
- retry uses the same WorkItemId;
- user wait survives restart;
- dependency completion reschedules work;
- stale work becomes Superseded;
- crash after authoritative commit recovers without duplication;
- provider work and Narrative continuation use the same lifecycle;
- cleanup preserves unresolved items;
- no external broker is required.

## 146. Definition of Compliance

An implementation complies when:

- durable Work Items use the canonical states in this RFC;
- Operation Record and Work Item remain separate;
- one Operation may own multiple Work Items;
- claims are atomic and bounded;
- handlers are idempotent;
- authoritative work references an OperationId;
- lease expiry does not cause blind retry;
- retry, user wait, and dependency wait are distinct;
- unknown outcomes enter RecoveryRequired;
- superseded work cannot execute;
- unresolved work survives restart;
- all semantic keys use English;
- MVP uses a local scheduler without requiring distributed infrastructure.

## 147. Review Triggers

Review this RFC if:

- Chronicle introduces multiple worker processes;
- remote workers are introduced;
- cloud queues are introduced;
- server-hosted authority is introduced;
- distributed leases are required;
- Work Item dependencies become a general workflow language;
- cross-device scheduling is introduced;
- queue retention policy changes materially.

## 148. Deferred Decisions

Later decisions may define:

- remote worker protocol;
- external broker;
- distributed lease service;
- multi-node scheduler;
- cross-device queue;
- administrative work dashboard;
- recurring scheduled maintenance;
- dead-letter tooling;
- workflow visualization.

## 149. Final Decision

Chronicle will use one canonical Work Item lifecycle:

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

A Work Item schedules effort.

An Operation owns intent.

Chronicle owns truth.

Retries may repeat work.

They may not repeat authoritative effects.
