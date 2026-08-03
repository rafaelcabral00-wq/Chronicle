---
id: RFC-0017
title: Operation Records, Idempotency, Commit Recovery, and Canonical Lifecycle
status: Draft
version: 0.2.0
owner: Chronicle Team
last_updated: 2026-08-02
category: Application
supersedes:
  - RFC-0017@0.1.0
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
implements: []
related_to:
  - ADR-0013
  - ADR-0014
  - ADR-0024
  - ADR-0033
  - ADR-0034
  - ADR-0036
  - ADR-0041
---

> **"An operation may be retried. Its authoritative effect may happen only once."**

# Operation Records, Idempotency, Commit Recovery, and Canonical Lifecycle

## 1. Status

**Draft**

This RFC defines Chronicle's canonical model for durable Application operations.

This revision resolves the lifecycle conflict between the earlier RFC and ADR-0013.

The canonical states are now:

```text
Created
Prepared
WaitingForExternalInput
WaitingForPlayer
ReadyToCommit
Committing
Committed
Completed
FailedRetryable
FailedTerminal
Cancelled
RecoveryRequired
```

The decision is:

- every authoritative multi-step Application action receives an `OperationId`;
- Chronicle persists an `OperationRecord`;
- retries reuse the same OperationId when they represent the same user or system intent;
- authoritative effects commit at most once;
- waiting for providers, players, Dice release, or other external input occurs outside database transactions;
- `Committed` means authoritative mutation committed;
- `Completed` means all required post-commit work and result publication completed;
- unknown commit outcome enters `RecoveryRequired`;
- recovery inspects authoritative state before retrying;
- terminal failure preserves already committed truth and reports its preservation state;
- Operation Record is Application orchestration evidence, not Domain truth by itself;
- all state keys and error keys use English;
- Rule Set packages and Narrative Intelligence may request work, but Chronicle owns Operation lifecycle and persistence.

## 2. Purpose

Chronicle performs operations that may span:

- validation;
- database reads;
- Rule Set execution;
- provider calls;
- Dice workflows;
- player confirmation;
- file publication;
- backup or migration;
- event application;
- post-commit projections;
- crash recovery.

Without a durable operation model, retry may:

- duplicate a Message;
- create two Dice Rolls;
- apply progression twice;
- transition a Scene twice;
- create duplicate Memories;
- import the same Campaign twice;
- lose track of an uncertain commit.

## 3. Scope

This RFC covers:

- OperationId;
- OperationRecord;
- canonical lifecycle;
- idempotency;
- transaction boundaries;
- waiting states;
- commit semantics;
- unknown commit recovery;
- cancellation;
- retry classification;
- result persistence;
- correlation;
- cleanup and retention;
- errors;
- testing.

## 4. Out of Scope

This RFC does not define:

- Work Item scheduling;
- provider retry policy details;
- exact database technology;
- exact UI components;
- Rule Set mechanics;
- Dice state machine;
- Narrative Turn state machine;
- distributed multi-node consensus.

## 5. Operation Identity

Every operation has:

```text
OperationId
```

Chronicle generates it before authoritative work begins.

## 6. Same Intent, Same Operation

A retry of the same logical intent reuses the same OperationId.

## 7. New Intent, New Operation

A materially new user action or changed command uses a new OperationId.

## 8. Operation Record

Recommended shape:

```text
OperationRecord
├── OperationId
├── OperationTypeKey
├── OperationContractVersion
├── CampaignId
├── State
├── TriggerReference
├── ParentOperationId
├── CorrelationReferences[]
├── RequestFingerprint
├── ExpectedVersions
├── ResultReference
├── FailureCode
├── RetryAfterUtc
├── CreatedAtUtc
├── UpdatedAtUtc
├── CommittedAtUtc
├── CompletedAtUtc
└── RowVersion
```

## 9. Operation Type Key

Examples:

```text
campaign.create
scene.transition
dice.roll-request
dice.resolve
narrative-event.apply
session.finalize
backup.create
campaign.import
configuration.migrate
```

Keys are invariant English identifiers.

## 10. Operation Contract Version

Each operation type has a versioned command and result contract.

## 11. Request Fingerprint

A canonical fingerprint may detect accidental reuse of one OperationId with a different request.

## 12. Fingerprint Mismatch

The same OperationId with a materially different request fails.

## 13. Trigger Reference

An operation may be triggered by:

- user command;
- Narrative Event;
- Work Item;
- Dice continuation;
- migration;
- recovery;
- system startup.

## 14. Parent Operation

Nested workflows may reference a parent OperationId.

Parent-child linkage does not merge commit authority.

## 15. Canonical State Machine

```text
Created
    ↓
Prepared
    ↓
WaitingForExternalInput | WaitingForPlayer | ReadyToCommit
    ↓
ReadyToCommit
    ↓
Committing
    ↓
Committed
    ↓
Completed
```

Failure and recovery paths may enter:

```text
FailedRetryable
FailedTerminal
Cancelled
RecoveryRequired
```

## 16. Created

The durable Operation Record exists.

No authoritative work has begun.

## 17. Prepared

Input, permissions, context, and prerequisites have been validated sufficiently to continue.

## 18. Waiting for External Input

The operation waits for something not controlled synchronously by the current transaction.

Examples:

- provider response;
- file-system availability;
- package resolution;
- another durable workflow.

## 19. Waiting for Player

The operation waits for explicit player action.

Examples:

- Dice release;
- destructive confirmation;
- recovery choice.

## 20. Ready to Commit

All required input is available and final precommit validation passed.

## 21. Committing

Chronicle is attempting the authoritative transaction.

## 22. Committed

The authoritative transaction committed successfully.

This state must be queryable durably.

## 23. Completed

All required post-commit activities completed.

Examples:

- result record published;
- read model updated;
- dependent Work Item scheduled;
- UI response made recoverable.

## 24. Failed Retryable

The operation did not complete and may be retried safely.

## 25. Failed Terminal

The operation cannot continue under the same intent without explicit intervention or a new operation.

## 26. Cancelled

The operation ended through valid cancellation.

Cancellation does not undo already committed truth.

## 27. Recovery Required

Chronicle cannot safely determine the authoritative outcome or next action automatically.

## 28. Terminal States

Terminal states are:

```text
Completed
FailedTerminal
Cancelled
```

`Committed` is not terminal if required post-commit work remains.

`RecoveryRequired` is not terminal.

## 29. Allowed Transitions

Canonical transitions include:

```text
Created → Prepared
Prepared → WaitingForExternalInput
Prepared → WaitingForPlayer
Prepared → ReadyToCommit
WaitingForExternalInput → Prepared
WaitingForExternalInput → ReadyToCommit
WaitingForPlayer → Prepared
WaitingForPlayer → ReadyToCommit
ReadyToCommit → Committing
Committing → Committed
Committed → Completed
Created|Prepared|Waiting*|ReadyToCommit → Cancelled
Created|Prepared|Waiting*|ReadyToCommit|Committing|Committed → FailedRetryable
Any nonterminal state → RecoveryRequired
FailedRetryable → Prepared
FailedRetryable → ReadyToCommit
RecoveryRequired → Prepared
RecoveryRequired → ReadyToCommit
RecoveryRequired → Committed
RecoveryRequired → Completed
RecoveryRequired → FailedTerminal
```

Exact operation types may restrict this further.

## 30. Forbidden Transitions

Examples:

- `Completed → Committing`;
- `Cancelled → Committed`;
- `FailedTerminal → ReadyToCommit`;
- `Created → Committed`;
- `WaitingForPlayer → Committing` without `ReadyToCommit`;
- `Committed → Cancelled` as an undo mechanism.

## 31. State Registry

Chronicle maintains one canonical registry containing:

```text
State key
Meaning
Terminal status
Allowed predecessors
Allowed successors
Persistence requirement
User-visible meaning
Retry behavior
Cancellation behavior
```

## 32. Operation-Specific Substates

Operation types may persist typed detail outside the canonical state enum.

They must not invent a competing root lifecycle.

## 33. Idempotency Contract

An operation handler must define:

- idempotency key;
- request fingerprint;
- authoritative uniqueness boundary;
- result lookup;
- recovery query;
- duplicate handling.

## 34. At-Most-Once Authoritative Effect

Chronicle guarantees at-most-once effect through:

- OperationId;
- database unique constraints;
- transaction boundaries;
- result records;
- recovery queries.

## 35. Not Exactly-Once Execution

Computation, provider calls, and validation may happen more than once.

Only authoritative effects are constrained to one committed result.

## 36. Duplicate Request Before Commit

A duplicate invocation observes the same Operation Record and does not start an independent effect.

## 37. Duplicate Request After Commit

Chronicle returns the committed or completed result.

## 38. Different Request with Same ID

Fails with:

```text
operation.request-fingerprint-mismatch
```

## 39. Transaction Rule

No external wait occurs inside an authoritative database transaction.

## 40. Preparation Phase

Preparation may:

- read state;
- validate;
- construct Rule Set request;
- inspect package bindings;
- build context;
- stage files;
- wait for provider or user.

## 41. Commit Phase

The commit phase is short and contains only the authoritative mutation and required evidence.

## 42. Post-Commit Phase

Post-commit may:

- update projections;
- schedule continuation;
- publish safe result;
- clean staging;
- emit local logs.

## 43. Commit Evidence

The transaction persists enough evidence to prove whether it committed.

## 44. Result Record

An operation may persist a typed result record or references to authoritative records.

## 45. Committed Versus Completed

This distinction is mandatory.

Example:

```text
Dice evidence committed
    but
Narrative continuation not yet scheduled
```

The operation is `Committed`, not yet `Completed`.

## 46. Unknown Commit

A process may fail after sending commit but before receiving confirmation.

Chronicle must not blindly rerun the mutation.

## 47. Unknown Commit Recovery

Recovery checks:

- OperationRecord state;
- unique authoritative records;
- result references;
- aggregate version;
- transaction marker;
- correlated child records.

## 48. Recovery Outcomes

Recovery may determine:

```text
NotCommitted
Committed
Completed
Conflict
ManualRecoveryRequired
```

## 49. Not Committed

The operation may safely return to `ReadyToCommit`.

## 50. Committed

The state is corrected to `Committed`, then post-commit work resumes.

## 51. Completed

The existing result is returned.

## 52. Conflict

A different authoritative operation consumed the expected state.

The operation becomes terminal or requires explicit reconciliation.

## 53. Manual Recovery Required

Chronicle remains in `RecoveryRequired` and presents safe actions.

## 54. Cancellation

Cancellation is allowed only before irreversible authoritative commit unless the operation defines a separate compensating workflow.

## 55. No Implicit Undo

Cancellation after commit is not rollback.

A new correction or compensating operation is required.

## 56. Cancellation Evidence

Chronicle records:

- who or what requested cancellation;
- time;
- prior state;
- safe reason code.

## 57. Retry Classification

Failures are classified as:

```text
RetryableSameOperation
RetryableAfterInput
TerminalSameOperation
RecoveryInspectionRequired
```

## 58. Retryable Same Operation

The same OperationId can retry without new user intent.

## 59. Retryable After Input

The operation waits for corrected configuration, package, credential, player choice, or other input.

## 60. Terminal Same Operation

A new explicit action is required.

## 61. Recovery Inspection Required

Chronicle must inspect authoritative state first.

## 62. Backoff

Automated retries use bounded backoff where appropriate.

## 63. Player-Waiting Operations

A waiting operation survives restart.

## 64. Dice Example

A Dice request operation may:

1. create OperationRecord;
2. prepare request;
3. enter `WaitingForPlayer`;
4. receive explicit release;
5. enter `ReadyToCommit`;
6. generate and commit evidence;
7. enter `Committed`;
8. schedule resolution or continuation;
9. enter `Completed`.

The exact Dice lifecycle remains governed by ADR-0033.

## 65. Narrative Event Example

A Narrative Event application may:

1. create OperationRecord;
2. validate event;
3. enter `ReadyToCommit`;
4. apply one state change;
5. commit correlation;
6. schedule dependent work;
7. complete.

## 66. Provider Example

A provider call itself is usually a Work Item or Provider Attempt under a parent Narrative operation.

Provider execution does not own Campaign commit authority.

## 67. Rule Set Example

A Rule Set may calculate or validate a proposed operation.

Chronicle still owns OperationRecord, transaction, and persistence.

## 68. File Publication Example

A backup operation may:

- prepare and stage;
- wait for filesystem;
- validate;
- publish atomically;
- commit artifact metadata;
- complete cleanup.

## 69. Work Item Relationship

Work Items schedule execution.

Operation Records own logical intent and authoritative result.

## 70. One Operation, Multiple Work Items

Retries or phases may use several Work Items under one OperationId.

## 71. Work Item Correlation

Recommended:

```text
WorkItem.OperationId
```

## 72. Narrative Turn Correlation

Recommended:

```text
OperationRecord.NarrativeTurnId
OperationRecord.NarrativeEventId
```

## 73. Dice Correlation

Recommended:

```text
OperationRecord.DiceRollId
```

where applicable.

## 74. Error Model

Recommended errors:

```text
operation.not-found
operation.type-unknown
operation.version-unsupported
operation.state-invalid
operation.transition-invalid
operation.request-fingerprint-mismatch
operation.expected-version-conflict
operation.already-committed
operation.cancel-not-allowed
operation.retry-not-allowed
operation.commit-failed
operation.commit-outcome-unknown
operation.result-missing
operation.recovery-required
operation.failed-terminal
```

## 75. Data Preservation State

Operation results should state:

```text
AuthoritativeDataUnchanged
PreparedOnly
WaitingForExternalInput
WaitingForPlayer
CommitNotStarted
CommitOutcomeUnknown
AuthoritativeChangeCommitted
PostCommitPending
OperationCompleted
CancellationRecorded
RecoveryRequired
```

## 76. Logging

Safe logs may include:

- OperationId;
- type key;
- state transition;
- CampaignId;
- parent operation;
- WorkItemId;
- NarrativeTurnId;
- DiceRollId;
- result code;
- duration.

They must not include credentials, full prompts, raw responses, or private Campaign content by default.

## 77. Metrics

Useful local metrics include:

```text
OperationDuration
OperationRetryCount
OperationRecoveryCount
UnknownCommitCount
OperationConflictCount
OperationTerminalFailureCount
OperationCancellationCount
```

No remote telemetry is required.

## 78. Retention

Operation Records required for:

- unresolved work;
- idempotency;
- correction;
- audit;
- recovery;

remain retained according to policy.

## 79. Completed Operation Compaction

Old completed records may be compacted only when:

- authoritative result remains linked;
- no retry window remains;
- no correction or audit dependency exists;
- retention policy permits it.

## 80. No Deletion of Unresolved Operations

Unresolved, committed-not-completed, and RecoveryRequired operations must not be deleted by routine cleanup.

## 81. Language

State keys, operation keys, error keys, and base documentation use English.

Localized UI labels remain separate.

## 82. Testing Strategy

The implementation requires:

```text
State Machine Tests
Idempotency Tests
Commit Tests
Unknown Commit Tests
Cancellation Tests
Retry Tests
Concurrency Tests
Persistence Tests
Crash Recovery Tests
Cross-Workflow Tests
```

## 83. State Machine Tests

Every allowed and forbidden transition must be tested.

## 84. Idempotency Tests

Tests must prove:

- duplicate before commit;
- duplicate during wait;
- duplicate after commit;
- duplicate after completion;
- fingerprint mismatch;
- same operation after restart.

## 85. Commit Tests

Tests must prove:

- one authoritative effect;
- short transaction;
- result evidence;
- committed versus completed distinction.

## 86. Unknown Commit Tests

Inject failure:

- before transaction;
- during transaction;
- after database commit;
- before state update;
- before response.

Recovery must not duplicate effects.

## 87. Cancellation Tests

Tests cover:

- cancellation before preparation;
- while waiting;
- before commit;
- after commit rejection;
- compensating operation where supported.

## 88. Concurrency Tests

Tests cover two workers or callers attempting the same OperationId.

Only one authoritative commit may occur.

## 89. Expected Version Tests

Stale expected versions produce conflict without mutation.

## 90. Cross-Workflow Tests

Use OperationRecord with:

- Dice;
- Narrative Event;
- Scene transition;
- Session finalization;
- backup;
- migration.

## 91. Architecture Tests

Architecture tests must reject:

- authoritative handler without OperationId;
- external wait inside transaction;
- provider call inside commit transaction;
- duplicate root lifecycle enum;
- direct transition from Created to Committed;
- cancellation used as rollback;
- retry that creates a new OperationId for the same intent without policy;
- cleanup deleting unresolved operations.

## 92. Prohibited Patterns

### 92.1 Retry by Recreating Intent

Reuse OperationId for the same logical action.

### 92.2 Exactly-Once Computation Claim

Only authoritative effect is at-most-once.

### 92.3 External Wait in Transaction

Persist state and release the transaction.

### 92.4 Treat Committed as Completed

Post-commit work may remain.

### 92.5 Blind Retry After Unknown Commit

Inspect authoritative state.

### 92.6 Cancel as Undo

Use a new correction or compensation.

### 92.7 Operation State Inferred from Logs

Persist the canonical lifecycle.

### 92.8 Package-Owned Operation Persistence

Chronicle owns the operation record.

## 93. Alternatives Considered

### No Durable Operation Record

Rejected because restart and duplicate delivery become unsafe.

### One State: Pending or Done

Rejected because waiting, committing, committed, and recovery have different semantics.

### Reuse Work Item as Operation Record

Rejected because scheduling execution and owning logical idempotent intent are different responsibilities.

### Automatic Compensation for Every Failure

Rejected because many committed Domain effects require explicit correction rather than generic rollback.

### Provider Request ID as Idempotency Key

Rejected because provider IDs do not own Chronicle authority.

## 94. Consequences

### Positive

- one canonical lifecycle;
- reliable idempotency;
- safe crash recovery;
- explicit wait states;
- clear committed/completed distinction;
- consistent integration across Dice, Narrative, backup, and migration;
- simpler testing and diagnostics.

### Negative

- more durable records;
- handlers need explicit recovery logic;
- result contracts require discipline;
- cleanup and retention become necessary;
- state transitions add implementation complexity.

## 95. Risks

### OperationRecord Becomes a Generic God Object

Mitigation:

- small canonical lifecycle;
- typed operation-specific data;
- separate Work Items and Domain records.

### Recovery Logic Is Incomplete

Mitigation:

- mandatory recovery query per operation type;
- fault injection;
- unique constraints.

### Excessive Retention

Mitigation:

- policy-based compaction;
- preserve unresolved and audit-relevant records.

### Same ID Used with Different Request

Mitigation:

- canonical fingerprint;
- strict mismatch error.

## 96. Technology Spike

Before acceptance, implement:

1. OperationRecord persistence;
2. canonical state enum;
3. transition validator;
4. OperationId middleware;
5. request fingerprint;
6. committed-result lookup;
7. unknown-commit recovery;
8. cancellation;
9. retry classification;
10. Work Item correlation;
11. Narrative Turn and Dice correlation;
12. fault-injection tests;
13. architecture tests.

## 97. Spike Acceptance

The spike passes when:

- one operation survives restart;
- duplicate invocation creates one effect;
- fingerprint mismatch fails;
- waiting does not hold a transaction;
- crash after commit recovers the result;
- committed and completed remain distinct;
- cancellation before commit works;
- cancellation after commit is rejected;
- two concurrent callers produce one commit;
- Dice and Narrative Event examples use the same canonical lifecycle.

## 98. Definition of Compliance

An implementation complies when:

- every authoritative multi-step action has an OperationId;
- one durable OperationRecord owns its lifecycle;
- the canonical states in this RFC are used;
- same intent reuses the same OperationId;
- external waits occur outside transactions;
- authoritative effects commit at most once;
- `Committed` and `Completed` remain distinct;
- unknown commit outcomes enter recovery;
- recovery inspects truth before retry;
- cancellation does not undo committed state;
- operation keys and state keys are invariant English identifiers;
- Rule Sets and providers never own operation persistence.

## 99. Review Triggers

Review this RFC if:

- Chronicle becomes multi-node;
- server-hosted authority is introduced;
- distributed transactions are introduced;
- collaborative editing requires conflict-free replicated operations;
- operation retention policy changes materially;
- one workflow requires compensation semantics beyond correction;
- Work Item and Operation responsibilities are proposed to merge.

## 100. Deferred Decisions

Later decisions may define:

- distributed operation ownership;
- server leases;
- cross-device idempotency;
- long-term archival compaction;
- generic compensation framework;
- external API idempotency headers;
- operational dashboards.

## 101. Final Decision

Chronicle will use one canonical Operation lifecycle:

```text
Created
Prepared
WaitingForExternalInput
WaitingForPlayer
ReadyToCommit
Committing
Committed
Completed
FailedRetryable
FailedTerminal
Cancelled
RecoveryRequired
```

Operations may retry computation.

They may wait.

They may recover.

Their authoritative effect may commit only once.
