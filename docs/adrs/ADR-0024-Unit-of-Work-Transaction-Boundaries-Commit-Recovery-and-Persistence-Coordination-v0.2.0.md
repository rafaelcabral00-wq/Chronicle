---
id: ADR-0024
title: Unit of Work, Transaction Boundaries, Commit Recovery, and Persistence Coordination
status: Proposed
version: 0.2.0
owner: Chronicle Team
last_updated: 2026-08-02
category: Technology
supersedes:
  - ADR-0024@0.1.0
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
  - ADR-0025
  - ADR-0026
  - ADR-0033
  - ADR-0034
  - ADR-0036
  - ADR-0038
  - ADR-0041
implements:
  - RFC-0017
  - RFC-0019
  - RFC-0033
related_to:
  - ADR-0027
  - ADR-0028
  - ADR-0037
  - ADR-0039
  - ADR-0042
---

> **"A transaction protects one authoritative boundary. It must never be stretched around uncertainty that Chronicle does not control."**

# Unit of Work, Transaction Boundaries, Commit Recovery, and Persistence Coordination

## 1. Status

**Proposed**

This ADR defines Chronicle's concrete transaction, unit-of-work, commit-recovery, and persistence-coordination model.

This revision aligns the implementation architecture with the corrected durable records:

- Operation Records;
- Work Items;
- Narrative Turns;
- accepted Narrative outputs;
- Provider Attempts;
- Structured Event applications;
- Dice Rolls;
- random evidence;
- multi-stage Dice resolution;
- corrections;
- backup and migration workflows.

The decision is:

- use EF Core `DbContext` as the technical unit-of-work implementation;
- create one short-lived DbContext per bounded Application operation or transaction phase;
- never use a global tracked DbContext;
- never hold a database transaction while waiting for:
  - Narrative Intelligence;
  - player input;
  - Dice release;
  - filesystem publication;
  - package download or resolution;
  - long Rule Set computation;
  - UI confirmation;
- persist durable workflow state before every external wait;
- use `OperationRecord` as the idempotent logical intent boundary;
- use `WorkItem` as the durable schedulable execution boundary;
- use explicit transaction coordinators for multi-record authoritative mutations;
- commit authoritative state and its required idempotency evidence atomically;
- distinguish `Committed` from `Completed`;
- use optimistic concurrency and expected versions;
- use database uniqueness as a correctness boundary, not merely as optimization;
- recover unknown commit outcomes by querying authoritative records;
- never blindly retry after an uncertain commit;
- keep provider attempts and response staging outside Campaign authority;
- keep one Campaign mutation lane where necessary, without globally serializing all work;
- use no distributed transaction coordinator in MVP;
- use no ambient transaction as the default;
- prohibit repository methods from silently opening nested independent transactions;
- require every authoritative handler to declare its transaction and recovery strategy;
- use English state and error keys;
- keep transaction logic independent from any specific Rule Set.

## 2. Context

Chronicle performs workflows that cross several different kinds of uncertainty.

Examples:

- a provider call may time out;
- the player may take minutes to release a Roll;
- a Roll may require several generation stages;
- a process may crash after database commit but before returning success;
- a file may be staged and fail during publication;
- a package may be missing;
- a migration may fail after a checkpoint;
- a Narrative Turn may be accepted while later event effects remain pending.

A naive unit-of-work implementation could:

- keep transactions open for too long;
- lock SQLite unnecessarily;
- duplicate authoritative effects on retry;
- publish prose without event correlation;
- create two Dice Rolls from one event;
- regenerate committed random evidence;
- mark an Operation complete before post-commit work finishes;
- lose track of uncertain commits.

## 3. Decision Drivers

The architecture prioritizes:

1. authoritative consistency;
2. short SQLite write transactions;
3. idempotency;
4. restart recovery;
5. explicit transaction ownership;
6. low lock contention;
7. clear Application boundaries;
8. deterministic testing;
9. local-first simplicity;
10. future extensibility without distributed transactions.

## 4. Core Principle

Chronicle separates:

```text
Preparation
    validation and external work

Commit
    short authoritative transaction

Post-Commit
    follow-up work and result publication
```

## 5. Technical Unit of Work

EF Core `DbContext` is the technical unit of work.

## 6. Scope of DbContext

One DbContext instance belongs to one bounded phase.

Examples:

- load and validate;
- authoritative commit;
- recovery inspection;
- read-model projection.

## 7. No Long-Lived Context

The following are prohibited:

- application-wide singleton DbContext;
- one tracked context for a whole Session;
- one tracked context across provider calls;
- one tracked context across player input;
- one tracked context retained by Presentation.

## 8. Context Factory

Infrastructure provides a context factory such as:

```text
IChronicleDbContextFactory
```

or the framework-supported equivalent.

## 9. Transaction Coordinator

Application uses explicit coordinators for bounded authoritative workflows.

Recommended abstraction:

```text
ITransactionCoordinator
```

## 10. Coordinator Responsibility

The coordinator owns:

- DbContext lifetime;
- transaction start;
- isolation policy;
- SaveChanges;
- commit;
- rollback;
- conflict translation;
- unknown-commit classification.

## 11. No Generic Business Logic in Coordinator

The transaction coordinator does not contain Domain decisions.

## 12. Repository Responsibility

Repositories:

- load aggregates or workflow records;
- persist changes through the active DbContext;
- expose bounded queries;
- do not commit independently.

## 13. No Repository-Level Commit

A repository method must not call an independent `SaveChanges` or commit unless its contract explicitly defines a complete standalone unit of work.

The default is Application-owned commit.

## 14. No Hidden Nested Transaction

Repository and handler code must not silently create nested transactions.

## 15. Ambient Transactions

Ambient transactions are not the default.

## 16. No Distributed Transaction

Chronicle MVP does not use:

- MSDTC;
- two-phase commit;
- distributed transaction coordinators;
- transaction spanning SQLite and provider APIs;
- transaction spanning SQLite and filesystem atomically.

## 17. External Resource Coordination

Database and filesystem coordination use:

```text
stage
validate
commit metadata or intent
publish atomically
recover by operation identity
```

rather than distributed transactions.

## 18. Transaction Phases

The canonical phases are:

```text
Prepare
Wait
Revalidate
Commit
PostCommit
Recover
```

## 19. Prepare

Preparation may:

- parse command;
- load current state;
- validate permissions;
- resolve exact Rule Set package;
- build provider context;
- calculate deterministic Rule Set data;
- stage files;
- create OperationRecord;
- create WorkItem;
- create NarrativeTurn;
- create ProviderAttempt metadata.

## 20. Wait

Wait may include:

- provider execution;
- user input;
- Dice release;
- package availability;
- file I/O;
- external process completion.

No database transaction remains open.

## 21. Revalidate

Before commit, Chronicle revalidates:

- expected aggregate version;
- current Scene;
- Campaign ownership;
- package binding;
- workflow state;
- uniqueness assumptions;
- cancellation or supersession;
- prerequisite records.

## 22. Commit

Commit performs the minimal authoritative mutation and required evidence atomically.

## 23. Post-Commit

Post-commit may:

- schedule Work Items;
- update read models;
- clean staging;
- return a response;
- begin narrative continuation;
- emit safe diagnostics.

## 24. Recover

Recovery inspects durable state when commit or post-commit outcome is uncertain.

## 25. Operation Record Boundary

Every authoritative multi-step action has an OperationId.

## 26. Operation Creation

OperationRecord is created before external uncertainty where practical.

## 27. Operation Preparation

Preparation persists:

- type;
- request fingerprint;
- expected versions;
- trigger references;
- current state;
- correlation IDs.

## 28. Ready to Commit

An operation enters `ReadyToCommit` only after all required input exists and final validation passes.

## 29. Committing

Immediately before or within the authoritative transaction, state moves to `Committing`.

## 30. Committed

The authoritative transaction must persist:

- Domain mutation;
- result reference;
- idempotency evidence;
- Operation state `Committed`;

in one transaction where feasible.

## 31. Completed

`Completed` is written after all required post-commit work is durably done.

## 32. Committed Is Not Completed

Example:

```text
Dice evidence committed
but
Narrative continuation not yet scheduled
```

The Operation is `Committed`.

## 33. Work Item Boundary

Work Items schedule one execution unit.

## 34. Work Item Claim Transaction

Claiming is one short atomic update.

## 35. Work Item Execution

The Work Item runs outside a long transaction.

## 36. Work Item Completion

Completion persists:

- result reference;
- terminal Work Item state;
- attempt metadata;
- claim release.

## 37. Work Item and Operation Coordination

A Work Item may complete while its Operation remains incomplete.

## 38. Narrative Turn Transaction Boundaries

Narrative orchestration uses separate transactions for:

1. turn creation;
2. ProviderAttempt creation;
3. response staging metadata;
4. accepted output publication;
5. each authoritative event application;
6. Dice request creation;
7. continuation scheduling;
8. turn completion.

## 39. No Provider Call in Transaction

Provider execution occurs after durable turn and attempt records exist.

## 40. Narrative Acceptance Transaction

The preferred acceptance transaction persists:

- `NarrativeAcceptedOutput`;
- accepted Messages;
- `NarrativeEventApplications`;
- NarrativeTurn accepted state;
- output hash and correlation.

## 41. Event Effects

Structured Events do not all execute inside the Narrative acceptance transaction by default.

## 42. Why Event Effects Are Separate

Separate event Operations allow:

- independent validation;
- idempotency;
- recovery;
- blocking behavior;
- Rule Set authority;
- partial scheduling without partial hidden mutation.

## 43. Atomic Narrative Acceptance

The acceptance transaction is atomic with respect to:

- selected output;
- accepted prose;
- event proposal records;
- turn state.

## 44. Event Application Transaction

Each authoritative event handler commits:

- state mutation;
- Operation result;
- event-application outcome;
- required record references;

atomically where feasible.

## 45. Dice Creation Transaction

One accepted `roll.requested` event creates exactly one DiceRoll.

The same transaction persists:

- DiceRoll;
- origin NarrativeTurn;
- origin event application;
- Operation correlation;
- initial state.

## 46. Dice Release Transaction

Player release transaction persists:

- idempotent release Operation;
- released timestamp;
- transition to `Generating` or durable generation intent.

## 47. Random Generation Boundary

Random values are generated outside an unnecessarily long transaction but under one idempotent generation stage.

## 48. Random Evidence Commit

One transaction persists:

- all evidence items for the stage;
- stage identity;
- uniqueness evidence;
- `RawValuesCommitted`;
- commit timestamp;
- Operation result.

## 49. No Partial Evidence Stage

A generation stage is committed atomically.

## 50. Generation Retry

Before generating, recovery checks whether the stage already has committed evidence.

## 51. Resolution Transaction

Resolution computation may happen outside the write transaction.

The commit persists:

- exact input evidence hash;
- resolution payload;
- evidence-use relationships;
- outcome;
- additional-generation or decision requirement;
- Dice state;
- Operation result.

## 52. Continuation Scheduling Transaction

One transaction persists:

- continuation NarrativeTurn;
- Dice continuation reference;
- optional WorkItem;
- Dice state transition.

## 53. No Duplicate Continuation

A unique constraint protects one continuation for one defined stage.

## 54. Session and Scene Transactions

Session, Act, and Scene changes use aggregate-version checks.

## 55. Campaign Mutation Lane

The MVP may serialize conflicting authoritative mutations per Campaign.

## 56. Mutation Lane Purpose

The lane reduces:

- SQLite write contention;
- aggregate-version races;
- conflicting Scene transitions;
- concurrent progression changes.

## 57. No Global Lock

Unrelated Campaign-safe reads and operations are not globally serialized.

## 58. Mutation Lane Implementation

The implementation may use:

- Application-level async keyed lock;
- durable Work Item ordering;
- expected-version concurrency;
- short SQLite transaction.

## 59. In-Process Lock Limitation

An in-process lock is a contention optimization, not authoritative correctness.

Database constraints and versions remain the source of truth.

## 60. Optimistic Concurrency

Mutable records use an explicit version token.

## 61. Expected Versions

Commands may include expected versions for:

- Campaign;
- Scene;
- Character;
- NarrativeTurn;
- DiceRoll;
- OperationRecord;
- WorkItem.

## 62. Concurrency Conflict

A stale version produces a typed conflict.

## 63. No Automatic Semantic Merge

Chronicle does not automatically merge conflicting authoritative mutations.

## 64. Retry After Conflict

Retry requires:

- reloading current state;
- revalidation;
- same OperationId if intent remains identical;
- new OperationId if user intent changes materially.

## 65. SQLite Isolation

Use SQLite transaction behavior validated for Chronicle's workload.

## 66. Write Transactions

Write transactions should start as late as possible.

## 67. Read Snapshot

Longer read-only projection or export may use a validated snapshot strategy.

## 68. Busy Handling

Database busy errors receive bounded retry only when no unknown commit is possible.

## 69. No Infinite Busy Retry

Retries are bounded and observable.

## 70. SaveChanges Failure Before Commit

If failure is known before commit:

- rollback;
- operation remains retryable or terminal;
- authoritative state is unchanged.

## 71. Commit Confirmation Failure

If the process cannot confirm commit outcome:

```text
OperationRecord → RecoveryRequired
```

or the durable equivalent discovered on restart.

## 72. Unknown Commit Recovery

Recovery queries:

- OperationRecord;
- result record;
- unique authoritative rows;
- aggregate version;
- correlated event application;
- Dice evidence stage;
- continuation turn;
- artifact publication metadata.

## 73. Recovery Outcomes

```text
NotCommitted
Committed
Completed
Conflict
ManualRecoveryRequired
```

## 74. Not Committed

Return safely to `ReadyToCommit`.

## 75. Committed

Correct state to `Committed` and resume post-commit work.

## 76. Completed

Return the existing result.

## 77. Conflict

Mark terminal or request explicit reconciliation.

## 78. Manual Recovery Required

Remain in `RecoveryRequired`.

## 79. No Blind Retry

The same mutation must not run again until absence of prior commit is proven.

## 80. Idempotency Constraints

Use unique constraints for logical one-time effects.

## 81. Examples of Idempotency Constraints

```text
OperationId unique
NarrativeTurnId + ProviderEventId unique
OriginNarrativeTurnId + OriginNarrativeEventApplicationId unique
DiceRollId + GenerationStageKey + GenerationSequence unique
DiceRollId + continuation-stage-key unique
NarrativeTurnId + NarrativeBlockSequence unique
```

## 82. Constraint Violation Handling

A uniqueness violation is translated into:

- existing result lookup;
- idempotent success;
- request mismatch;
- real conflict.

It is not automatically a generic internal error.

## 83. Request Fingerprints

OperationRecord stores a canonical request fingerprint.

## 84. Same ID, Different Request

This fails before authoritative mutation.

## 85. Filesystem Coordination

Filesystem work cannot share an atomic transaction with SQLite.

## 86. File Publication Pattern

Use:

1. create OperationRecord;
2. stage file;
3. validate file;
4. persist publication intent or metadata;
5. atomically rename on same volume;
6. verify publication;
7. mark Operation complete;
8. recover by artifact fingerprint and path state.

## 87. Backup Publication

Backup follows the file publication pattern.

## 88. Export Publication

Portable export follows the same pattern when implemented.

## 89. Import Publication

Import validates in staging, then publishes the complete Campaign inside a bounded final transaction.

## 90. Migration Coordination

Migration is a special exclusive workflow.

## 91. Migration Lock

Normal Campaign mutation is blocked during migration.

## 92. Migration Checkpoint

A verified checkpoint exists before destructive migration.

## 93. Migration Transactions

Each migration uses the safest supported database transaction behavior.

## 94. No Installer Migration

Installer scripts do not mutate Chronicle data.

## 95. Migration Unknown Outcome

Startup inspects:

- schema version;
- migration history;
- checkpoint;
- migration marker;
- integrity.

## 96. Backup Coordination

Backup may require:

- snapshot transaction;
- SQLite backup API;
- checkpoint metadata;
- post-copy validation.

## 97. No Long Write Lock for Compression

Compression and publication occur after snapshot capture.

## 98. Restore Coordination

Restore occurs with normal database access closed or isolated.

## 99. Restore Publication

Restored data becomes active only after full validation.

## 100. Rule Set Computation

Rule Set validation and deterministic calculation occur outside the write transaction where practical.

## 101. Rule Set Authority

Chronicle persists the result.

Rule Set packages never open transactions or access DbContext.

## 102. Provider Attempt Persistence

ProviderAttempt creation occurs before provider execution.

## 103. Response Staging

Response bytes may be staged before validation.

## 104. Accepted Output

Only normalized accepted output and authoritative Messages enter long-term Campaign history.

## 105. Raw Response Cleanup

Cleanup is post-commit work and may use a Work Item.

## 106. Failure to Clean Staging

Cleanup failure does not undo accepted Campaign truth.

It creates retryable maintenance work.

## 107. Read Models

Read-model updates may occur:

- in the same transaction when required for correctness;
- post-commit when rebuildable.

## 108. Projection Classification

Each projection declares:

```text
Authoritative
TransactionallyRequired
Rebuildable
EventuallyUpdated
```

## 109. Rebuildable Projection Failure

Failure does not roll back authoritative Domain commit.

A Work Item schedules repair.

## 110. Outbox

A local durable outbox may be used for post-commit internal notifications.

## 111. MVP Outbox Scope

The MVP may use a simple local table if needed.

It does not require an external message broker.

## 112. Outbox Event

An outbox record is operational delivery evidence, not Domain truth by itself.

## 113. Outbox Transaction

When used, the authoritative mutation and outbox record commit together.

## 114. Outbox Delivery

Delivery is at-least-once.

Consumers must be idempotent.

## 115. No Mandatory Event Bus

A broad event bus is not required for MVP.

## 116. Cancellation

Cancellation is permitted before irreversible commit according to Operation and Work Item policy.

## 117. Cancellation During External Wait

Persist cancellation and supersession state.

## 118. Cancellation During Commit

The transaction either commits or rolls back.

The UI cannot assume cancellation succeeded until durable state confirms it.

## 119. Cancellation After Commit

Does not undo the authoritative result.

## 120. Corrections

A correction is a new Operation and transaction.

## 121. No Generic Rollback of Domain History

Chronicle uses:

- correction;
- invalidation;
- replacement;
- restore;

according to the workflow.

## 122. Error Model

Recommended errors:

```text
transaction.context-scope-invalid
transaction.already-active
transaction.nested-not-allowed
transaction.external-wait-prohibited
transaction.concurrency-conflict
transaction.unique-conflict
transaction.commit-failed
transaction.commit-outcome-unknown
transaction.rollback-failed
transaction.recovery-required
transaction.result-not-found
transaction.request-fingerprint-mismatch
transaction.operation-id-required
transaction.work-item-operation-required
transaction.filesystem-publication-uncertain
```

## 123. Data Preservation State

Results should state:

```text
AuthoritativeDataUnchanged
PreparationPersisted
ExternalWaitPending
TransactionNotStarted
TransactionRolledBack
CommitOutcomeUnknown
AuthoritativeChangeCommitted
PostCommitPending
OperationCompleted
RecoveryRequired
```

## 124. Logging

Safe transaction logs may include:

- OperationId;
- WorkItemId;
- NarrativeTurnId;
- DiceRollId;
- transaction kind;
- expected version;
- state transition;
- duration;
- retry count;
- safe error code.

They must not include credentials, full prompts, raw provider responses, or full private Campaign content.

## 125. Metrics

Useful local metrics include:

```text
TransactionDuration
TransactionRollbackCount
ConcurrencyConflictCount
UniqueConflictCount
UnknownCommitCount
RecoverySuccessCount
DatabaseBusyCount
PostCommitFailureCount
```

No remote telemetry is required.

## 126. Testing Strategy

The implementation requires:

```text
Unit of Work Tests
Transaction Boundary Tests
Concurrency Tests
Idempotency Tests
Unknown Commit Tests
Work Item Tests
Narrative Acceptance Tests
Dice Commit Tests
Filesystem Coordination Tests
Migration Tests
Backup and Restore Tests
Fault Injection Tests
Architecture Tests
```

## 127. Unit of Work Tests

Tests prove:

- one short-lived DbContext per phase;
- repository methods do not commit independently;
- no tracked entity leaks across scopes;
- rollback discards changes.

## 128. Transaction Boundary Tests

Tests prove no transaction spans:

- provider call;
- player wait;
- Dice release wait;
- long file operation;
- UI confirmation.

## 129. Concurrency Tests

Tests cover:

- two Scene mutations;
- two Narrative acceptances;
- duplicate Dice release;
- duplicate event application;
- two Work Item claims;
- stale aggregate version.

## 130. Idempotency Tests

Tests prove one authoritative effect under duplicate command delivery.

## 131. Unknown Commit Tests

Inject failure:

- before SaveChanges;
- during SaveChanges;
- after database commit;
- before Operation state response;
- before UI response.

Recovery must return or complete the existing result.

## 132. Narrative Acceptance Tests

Tests prove:

- accepted output, Messages, event proposals, and turn state commit coherently;
- provider retry does not duplicate Messages;
- event effects remain separately idempotent.

## 133. Dice Tests

Tests prove:

- one DiceRoll per event;
- one release;
- one evidence stage commit;
- no partial evidence stage;
- resolution uses committed evidence;
- one continuation;
- crash never causes reroll.

## 134. Work Item Tests

Tests prove:

- one atomic claim;
- handler execution outside a long transaction;
- completion result persists;
- lease expiry triggers recovery rather than blind retry.

## 135. Filesystem Tests

Tests inject failure:

- during staging;
- after staging;
- before rename;
- after rename;
- before metadata completion.

Recovery uses operation identity and artifact fingerprint.

## 136. Migration Tests

Tests prove:

- checkpoint exists;
- normal writes are blocked;
- failure opens recovery;
- old binary cannot open unsupported schema.

## 137. Backup Tests

Tests prove snapshot consistency without holding a write transaction during compression.

## 138. Architecture Tests

Architecture tests must reject:

- singleton DbContext;
- repository-level hidden SaveChanges;
- provider call inside transaction;
- user wait inside transaction;
- Rule Set package accessing DbContext;
- provider adapter accessing DbContext;
- authoritative Work Item without OperationId;
- blind retry after unknown commit;
- ambient transaction as implicit default;
- filesystem and SQLite assumed atomically committed;
- `Committed` treated as equivalent to `Completed`.

## 139. Prohibited Patterns

### 139.1 Long Transaction Around a Workflow

Split prepare, wait, commit, and post-commit.

### 139.2 SaveChanges Hidden in Repository

Application owns the boundary.

### 139.3 Provider Call Inside Transaction

Persist attempt, release transaction, then call.

### 139.4 User Wait Inside Transaction

Persist waiting state.

### 139.5 Blind Retry After Commit Timeout

Recover by OperationId.

### 139.6 One Transaction for All Structured Events

Apply each authoritative effect through its own Operation unless a specific invariant requires a shared transaction.

### 139.7 Treat Filesystem Rename as Database Commit

Coordinate through staging and recovery.

### 139.8 In-Process Lock as Source of Truth

Use database constraints and versions.

### 139.9 Rule Set-Owned Transaction

Chronicle owns persistence.

### 139.10 Cancellation as Rollback

Use correction or compensation.

## 140. Alternatives Considered

### One Transaction for Entire User Turn

Rejected because provider and player waits would hold SQLite locks and make recovery unsafe.

### Repository-Owned Transactions

Rejected because cross-record invariants belong to Application use cases.

### Distributed Transaction Coordinator

Rejected because MVP is local and external APIs cannot participate meaningfully.

### Save Everything Then Compensate

Rejected because generic compensation cannot safely undo narrative and mechanical history.

### In-Memory Idempotency Only

Rejected because restart would lose the boundary.

### Serialize All Application Work Globally

Rejected because it would unnecessarily block independent work.

## 141. Consequences

### Positive

- clear transaction ownership;
- short SQLite locks;
- reliable idempotency;
- durable external waits;
- explicit commit recovery;
- coherent Narrative acceptance;
- safe Dice evidence commit;
- clearer separation of Operation and Work Item;
- future filesystem workflows remain recoverable;
- provider and Rule Set replacement remain possible.

### Negative

- workflows require more phases;
- more durable operational records exist;
- recovery logic must be implemented per operation type;
- transaction coordinators and tests add complexity;
- post-commit failure must be handled explicitly;
- developers must avoid convenient but unsafe hidden commits.

## 142. Risks

### Too Many Small Transactions Produce Complexity

Mitigation:

- standard phase templates;
- shared coordinators;
- clear use-case ownership;
- integration tests.

### Event Effects Become Inconsistent with Accepted Prose

Mitigation:

- validate causal coherence before acceptance;
- persist event proposals atomically with prose;
- stop at hard boundaries;
- repair invalid outputs.

### Unknown Commit Recovery Is Incomplete

Mitigation:

- mandatory recovery query;
- unique constraints;
- fault injection;
- Operation result references.

### Campaign Mutation Lane Becomes a Bottleneck

Mitigation:

- serialize only conflicting mutations;
- use optimistic concurrency;
- keep transactions short.

## 143. Technology Spike

Before acceptance, implement:

1. short-lived DbContext factory;
2. transaction coordinator;
3. repository no-commit policy;
4. OperationRecord integration;
5. WorkItem integration;
6. expected-version handling;
7. request fingerprint handling;
8. unique-conflict translation;
9. unknown-commit recovery;
10. Narrative acceptance transaction;
11. event-application transaction;
12. Dice creation, evidence, resolution, and continuation transactions;
13. filesystem staging coordinator;
14. migration exclusivity;
15. backup snapshot integration;
16. fault injection;
17. architecture tests.

## 144. Spike Acceptance

The spike passes when:

- no provider or user wait holds a transaction;
- one duplicate command creates one authoritative result;
- crash after commit recovers the existing result;
- accepted Narrative output and Messages remain coherent;
- Structured Event effects are independently idempotent;
- Dice evidence commits atomically by generation stage;
- continuation is unique;
- Work Item claim is atomic;
- filesystem publication recovers after every injected failure point;
- migration blocks normal mutation;
- no repository silently commits.

## 145. Definition of Compliance

An implementation complies when:

- EF Core DbContext is short-lived;
- Application owns transaction boundaries;
- repositories do not commit independently by default;
- external waits occur outside transactions;
- OperationRecord owns logical idempotent intent;
- WorkItem owns schedulable execution;
- authoritative mutation and required idempotency evidence commit together;
- `Committed` and `Completed` remain distinct;
- unknown commit outcomes are recovered by querying truth;
- optimistic concurrency and uniqueness enforce invariants;
- Narrative, Dice, backup, migration, and filesystem workflows follow explicit phase boundaries;
- providers and Rule Sets never own persistence transactions;
- no transaction model is specific to Werewolf.

## 146. Review Triggers

Review this ADR if:

- Chronicle becomes multi-process;
- server-hosted authority is introduced;
- distributed workers are introduced;
- PostgreSQL replaces SQLite;
- cloud synchronization is introduced;
- multiple devices mutate one Campaign;
- a real distributed transaction requirement appears;
- package code is proposed to participate in persistence;
- event effects require a new cross-aggregate atomicity model.

## 147. Deferred Decisions

Later decisions may define:

- server transaction boundaries;
- distributed locks;
- remote worker leases;
- external message broker;
- transactional outbox standardization;
- saga orchestration;
- cross-device conflict resolution;
- distributed idempotency keys.

## 148. Final Decision

Chronicle will coordinate persistence through short, explicit, recoverable transaction phases.

Operations own intent.

Work Items own scheduled effort.

Transactions own one bounded authoritative mutation.

Providers, users, filesystems, and Rule Sets remain outside the transaction while Chronicle waits.

When a commit outcome is uncertain, Chronicle will inspect truth before acting again.

It will never trade a user's Campaign for the convenience of a simpler retry.
