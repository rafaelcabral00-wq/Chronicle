---
id: ADR-0013
title: Application Command Dispatch and Transaction Pipeline
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
  - ADR-0007
  - ADR-0010
  - ADR-0012
  - RFC-0015
  - RFC-0016
  - RFC-0017
  - RFC-0018
  - RFC-0019
  - RFC-0033
  - RFC-0040
  - RFC-0042
---

> **"Every command should have one clear intention, one controlled transaction boundary, and one explainable result."**

# Application Command Dispatch and Transaction Pipeline

## 1. Status

**Proposed**

This ADR defines Chronicle's Application command dispatch model and transaction pipeline.

The decision is:

- use explicit typed command handlers;
- use an internal Chronicle-owned command dispatcher;
- avoid adopting a generic mediator library for the MVP;
- compose command behaviors through an explicit ordered pipeline;
- create or reuse `OperationId` before command execution;
- validate command shape before loading mutable state;
- enforce idempotency before applying side effects;
- acquire Campaign-scoped mutation coordination for Campaign-changing commands;
- open persistence transactions only around local authoritative state changes;
- never keep a transaction open across provider calls, user decisions, file dialogs, or long external work;
- commit Domain state and Operation Record outcome atomically where required;
- publish post-commit notifications only after successful commit;
- return typed results rather than infrastructure exceptions for expected failures;
- support durable continuation through Work Items when a workflow spans external or long-running stages.

The decision becomes **Accepted** after a vertical-slice spike proves:

- normal command success;
- validation failure;
- duplicate OperationId returning an existing result;
- conflicting OperationId fingerprint rejection;
- optimistic concurrency failure;
- failure before commit;
- failure after external call but before commit;
- ambiguous completion;
- post-commit notification failure;
- durable continuation after restart;
- no open SQLite transaction during provider wait.

## 2. Context

Chronicle Application use cases include:

- create Campaign;
- create Character;
- start Session;
- process player input;
- request narration;
- execute Dice Roll;
- continue narration;
- end Session;
- finalize Session;
- apply progression;
- change Preferences;
- create backup;
- restore;
- import;
- rebuild Rule Knowledge index.

These operations differ substantially.

Some are:

- short and fully local;
- read-only;
- state-changing;
- dependent on Rule Set mechanics;
- dependent on provider output;
- long-running;
- recoverable after restart;
- idempotent by OperationId;
- scoped to one Campaign;
- global to the installation.

A simplistic dispatcher could create hidden behavior and unclear transaction boundaries.

A large third-party mediator pipeline could also become architectural gravity before Chronicle's actual needs are known.

RFC-0017 defines command, query, and transaction concepts.

ADR-0001 selected explicit commands and queries without requiring a generic mediator library.

ADR-0004 defines SQLite and EF Core transaction constraints.

This ADR selects the concrete Application execution model.

## 3. Decision Drivers

The command pipeline prioritizes:

1. explicit use-case behavior;
2. idempotency;
3. short transactions;
4. Campaign mutation safety;
5. typed errors;
6. deterministic ordering;
7. observability;
8. recovery;
9. testability;
10. low framework coupling;
11. no hidden retries;
12. clear pre-commit and post-commit behavior.

## 4. Decision Summary

Chronicle will use:

```text
Command Model
    typed command records

Handler Model
    one primary handler per command

Dispatcher
    Chronicle-owned internal dispatcher

Pipeline
    explicit ordered behaviors

Core Behaviors
    Context initialization
    Authorization and visibility
    Input validation
    Operation identity
    Idempotency inspection
    Campaign mutation coordination
    Transaction orchestration
    Handler execution
    Commit
    Post-commit notification
    Result mapping
    Observability

Long Work
    Durable Work Items
    explicit continuation commands

Queries
    separate query dispatcher
    no command transaction pipeline by default
```

## 5. Command Definition

A command represents one intention to change authoritative state or initiate a controlled workflow.

Examples:

```text
CreateCampaignCommand
StartSessionCommand
ProcessPlayerInputCommand
ExecuteDiceRollCommand
FinalizeSessionCommand
AdvanceCharacterCommand
ChangeCampaignPreferenceCommand
CreateBackupCommand
RestoreBackupCommand
ImportCampaignCommand
```

## 6. Command Requirements

A command SHOULD contain:

- required input;
- expected entity or aggregate version where relevant;
- CampaignId when Campaign-scoped;
- OperationId;
- actor or execution context reference;
- contract version when crossing durable or external boundaries.

A command MUST NOT contain:

- repositories;
- DbContext;
- provider SDK objects;
- UI controls;
- credentials;
- mutable Domain entities;
- raw transport clients.

## 7. Command Immutability

Commands MUST be immutable after creation.

Record-like value semantics are preferred.

## 8. Command Naming

Commands use imperative intent names.

Preferred:

```text
FinalizeSessionCommand
ExecuteDiceRollCommand
ChangeCampaignPreferenceCommand
```

Avoid:

```text
SessionCommand
ProcessCommand
UpdateDataCommand
DoActionCommand
```

## 9. Handler Definition

Each command has one primary handler.

Conceptually:

```csharp
public interface ICommandHandler<TCommand, TResult>
{
    Task<TResult> HandleAsync(
        TCommand command,
        CommandExecutionContext context,
        CancellationToken cancellationToken);
}
```

The exact generic interface may differ.

## 10. Handler Responsibilities

A handler owns use-case orchestration.

It MAY:

- load required aggregate state;
- invoke Domain behavior;
- invoke deterministic Rule Set operations;
- create Domain records;
- request persistence through repositories;
- schedule durable Work Items;
- return a typed Application result.

It MUST NOT:

- begin arbitrary nested transactions;
- resolve dependencies through a service locator;
- bypass Operation Records;
- call UI services;
- hold a transaction open across provider calls;
- silently retry an ambiguous operation;
- swallow expected errors into logs only.

## 11. Internal Dispatcher

Chronicle will implement a small internal dispatcher.

Conceptually:

```csharp
public interface ICommandDispatcher
{
    Task<TResult> DispatchAsync<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken);
}
```

The dispatcher resolves:

- handler;
- ordered pipeline behaviors;
- execution context.

## 12. No Generic Mediator Dependency

The MVP will not adopt MediatR or another generic mediator library as an architectural requirement.

### Rationale

Chronicle needs:

- explicit pipeline ordering;
- strong OperationId semantics;
- transaction control;
- durable continuation;
- Campaign-scoped mutation coordination;
- predictable behavior.

A small internal dispatcher keeps these rules visible and avoids coupling public contracts to third-party abstractions.

## 13. Future Reconsideration

A mediator library MAY be adopted later if:

- it reduces implementation cost;
- it does not obscure pipeline semantics;
- it preserves explicit contracts;
- it passes architecture review.

## 14. Command Execution Context

Every command runs with a `CommandExecutionContext`.

Recommended fields:

```text
OperationId
CorrelationId
ActorContext
CampaignId when scoped
CommandType
StartedAtUtc
ApplicationVersion
ExpectedVersion
CancellationState
```

The context does not contain secrets or mutable Campaign state.

## 15. Actor Context

The MVP has one local user, but commands SHOULD still receive a typed actor context.

Initial actor categories may include:

```text
LocalPlayer
SystemRecovery
BackgroundWorker
Migration
ImportProcess
```

This prepares auditing and future authorization without implementing multiplayer identity.

## 16. Pipeline Order

The default command pipeline SHOULD execute in this order:

```text
1. Initialize execution context
2. Validate command envelope
3. Resolve authorization and visibility context
4. Inspect Operation Record and request fingerprint
5. Acquire mutation coordination when required
6. Validate expected version preconditions
7. Execute pre-transaction preparation
8. Open local transaction when required
9. Execute handler transactional stage
10. Persist Domain and Operation Record outcome
11. Commit
12. Release transaction
13. Publish post-commit notifications
14. Map final result
15. Emit completion observability
```

Not every command uses every stage.

## 17. Pipeline Behavior Interface

Conceptually:

```csharp
public interface ICommandBehavior<TCommand, TResult>
{
    Task<TResult> ExecuteAsync(
        TCommand command,
        CommandExecutionContext context,
        CommandExecutionDelegate<TResult> next,
        CancellationToken cancellationToken);
}
```

Behavior order is explicit in composition.

## 18. Behavior Ordering

Behavior ordering MUST be deterministic.

Registration order must not depend on assembly scanning order.

## 19. Command Envelope Validation

Envelope validation occurs before expensive state loading.

It checks:

- required OperationId;
- required CampaignId;
- identifier validity;
- command contract version;
- basic size limits;
- expected-version presence where required;
- forbidden null or empty values.

## 20. Business Validation

Business and Rule Set validation occurs after the required authoritative state is loaded.

Examples:

- Session can start;
- Character can advance;
- Roll request remains pending;
- Preference change is allowed;
- finalization is not already complete.

## 21. Authorization

The MVP uses local execution context, but authorization behavior still validates:

- command is allowed for actor type;
- target Campaign exists;
- target entity belongs to Campaign;
- player-facing operation does not request hidden-only state;
- system-only commands cannot be invoked through ordinary UI routes.

## 22. Visibility

Visibility checks occur before returning result data.

A successful command result MUST still use a safe projection for the caller.

## 23. Operation Identity

Every important state-changing command MUST have an OperationId.

The UI or initiating Application workflow creates it before first dispatch.

## 24. New Intention Versus Retry

A new intention creates a new OperationId.

Retrying the same intention reuses the existing OperationId.

Changing semantically important input requires a new OperationId.

## 25. Request Fingerprint

The pipeline computes a deterministic request fingerprint over semantically relevant, nonsecret command input.

The fingerprint excludes:

- timestamps added by infrastructure;
- correlation IDs;
- UI-only state;
- credentials;
- transient formatting.

## 26. Existing Operation Record

When the OperationId already exists:

### Same Fingerprint and Completed

Return the existing committed result.

### Same Fingerprint and In Progress

Return current operation status or attach to durable continuation.

### Same Fingerprint and Failed Retryable

Follow typed retry policy.

### Different Fingerprint

Reject with `OperationFingerprintConflict`.

## 27. Operation Record Creation

For a new operation, the pipeline creates an Operation Record before or with the first authoritative state transition according to the workflow.

The creation itself must be durable when later external work depends on it.

## 28. Operation Statuses

Recommended statuses:

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

## 29. Command Classification

Commands SHOULD declare execution characteristics.

Recommended metadata:

```text
ReadOnly
Transactional
CampaignMutating
GlobalMutating
RequiresIdempotency
MayScheduleWork
MayCallExternalBeforeTransaction
RequiresPostCommitNotification
```

## 30. Command Metadata

Metadata SHOULD be explicit through:

- command descriptor registration;
- attribute used only as metadata;
- static descriptor;
- handler registration configuration.

Critical behavior must not rely on fragile reflection conventions.

## 31. Campaign Mutation Coordination

Campaign-mutating commands acquire a logical Campaign mutation lease before loading mutable state.

Initial implementation MAY use an in-process keyed asynchronous lock.

SQLite concurrency and optimistic versions remain final safeguards.

## 32. Mutation Lock Key

The key is:

```text
CampaignId
```

Global operations use an explicit global key or separate coordinator.

## 33. Lock Lifetime

The lock SHOULD cover:

- authoritative state read;
- Domain decision;
- transaction;
- commit.

It MUST NOT cover:

- provider wait;
- player wait;
- long file parsing;
- long index rebuild;
- idle UI time.

## 34. External Work Before Mutation

For workflows requiring provider output:

1. create or update Operation Record;
2. gather safe context;
3. call provider outside transaction and mutation lock where possible;
4. validate response;
5. reacquire mutation coordination;
6. reload current state;
7. reject stale output if versions changed;
8. commit accepted result.

## 35. Staleness After External Call

Provider output MUST carry or be associated with the state version used to construct the request.

Before commit, Chronicle verifies:

- Campaign version;
- Session revision;
- Scene state;
- pending operation state;
- relevant Rule Set version.

## 36. Stale Response

A stale response is rejected or sent through a deliberate reconciliation path.

It is never accepted merely because the provider call succeeded.

## 37. Transaction Strategy

The command pipeline uses explicit Application transaction abstractions.

Conceptually:

```text
IApplicationTransaction
IApplicationTransactionFactory
```

The concrete implementation uses EF Core and SQLite.

## 38. Transaction Start

The transaction starts only when:

- all required external output is available;
- the command is ready to evaluate authoritative state;
- mutation coordination is held when required;
- expected versions can be checked.

## 39. Transaction Commit

The transaction commits:

- Domain state changes;
- append-only history;
- unique once-only records;
- Operation Record committed result;
- durable follow-up Work Items required for continuity.

## 40. Atomic Operation Result

For important operations, the committed result reference and Domain state MUST be in the same transaction.

Examples:

- Dice Roll and Operation Record;
- Advancement and progression ledger;
- finalization and Memory aging;
- Preference change and snapshot;
- import publication and import result.

## 41. Transaction Rollback

Any failure before commit rolls back local state.

Expected failures are mapped to typed results.

Unexpected failures are logged safely and mapped to a generic operation failure.

## 42. No Nested Business Transactions

Handlers SHOULD not begin nested business transactions.

Reusable Application services participate in the existing command transaction.

## 43. Savepoints

SQLite savepoints MAY be used internally for migrations or specialized workflows.

They are not the default command composition mechanism.

## 44. Post-Commit Work

Post-commit work includes:

- UI invalidation notification;
- safe local event publication;
- background wake signal;
- nonauthoritative metrics;
- desktop notification.

Post-commit failure MUST NOT roll back already committed Domain state.

## 45. Post-Commit Failure

If post-commit notification fails:

- the command result remains committed;
- the failure is logged;
- refresh may recover;
- a durable Work Item is used when the follow-up is required for correctness.

## 46. Domain Events

Domain Events MAY be collected during command execution.

They are applied in two categories:

```text
Transactional Domain Effects
Post-Commit Notifications
```

## 47. Transactional Domain Effects

Effects required for invariant correctness execute inside the same transaction.

Examples:

- progression ledger entry;
- Memory aging record;
- Relationship change history;
- Dice consequence application.

## 48. Post-Commit Notifications

Notifications that do not own authoritative state execute after commit.

Examples:

- refresh UI read model;
- wake background worker;
- publish local operation completion event.

## 49. Outbox Decision

A generic integration outbox is not required for MVP because Chronicle has no external authoritative message broker.

Durable Work Items serve the role for required asynchronous continuation.

## 50. Durable Continuation

A workflow that cannot complete in one local command uses explicit stages.

Example:

```text
ProcessPlayerInputCommand
    → persist accepted player Message
    → create NarrativeContinuation Work Item
    → commit

NarrativeWorker
    → call provider
    → validate response
    → dispatch AcceptNarrativeResponseCommand
```

## 51. Roll Workflow Example

```text
Request Roll
    → persist pending Roll request
    → commit

Player clicks Roll
    → ExecuteDiceRollCommand
    → inspect OperationId
    → acquire Campaign lock
    → open transaction
    → generate authoritative raw values
    → resolve through Rule Set
    → persist Roll and immediate consequences
    → create continuation Work Item
    → commit
    → return persisted Roll result
```

## 52. Finalization Workflow Example

```text
BeginFinalizationCommand
    → mark Session finalizing
    → create Archivist Work Item
    → commit

Archivist Work Item
    → gather evidence
    → call provider outside transaction
    → validate proposal
    → dispatch ApplyFinalizationProposalCommand

ApplyFinalizationProposalCommand
    → acquire Campaign lock
    → verify Session revision
    → open transaction
    → apply summary, Memories, aging, progression, Knowledge, Relationships
    → complete Session
    → record committed result
    → commit
```

## 53. Query Dispatch

Queries use a separate dispatcher.

Conceptually:

```csharp
public interface IQueryDispatcher
{
    Task<TResult> DispatchAsync<TQuery, TResult>(
        TQuery query,
        CancellationToken cancellationToken);
}
```

## 54. Query Pipeline

Typical query behaviors:

```text
Context
Authorization
Visibility
Validation
Execution
Caching when approved
Observability
```

Queries do not use OperationId for idempotency.

They may use CorrelationId.

## 55. Query Transactions

Read-only queries generally do not open explicit write transactions.

A consistent snapshot transaction MAY be used for:

- backup inspection;
- multi-query integrity view;
- restore validation;
- critical read consistency.

## 56. Query Side Effects

Queries MUST NOT change authoritative state.

Maintenance triggered by reads is prohibited unless explicitly separated into a command or background operation.

## 57. Result Types

Command handlers return typed results.

Example:

```text
CommandResult<TSuccess, TError>
```

The exact implementation may use discriminated-union patterns.

## 58. Expected Failures

Expected failures include:

- validation;
- not found;
- stale version;
- operation conflict;
- rule violation;
- provider unavailable;
- retry required;
- authorization;
- visibility denial;
- migration blocked.

These should not rely on exceptions for ordinary flow.

## 59. Unexpected Failures

Unexpected failures include:

- programming defect;
- corrupt internal state;
- unhandled infrastructure exception;
- impossible invariant breach.

They are logged and mapped to a safe error.

## 60. Cancellation Semantics

Cancellation is stage-sensitive.

### Before Commit

Cancellation may stop the command and roll back.

### During External Wait

Cancellation stops local waiting where possible.

### During Commit

Cancellation may be ignored after the commit boundary begins to avoid ambiguous partial handling.

### After Commit

Cancellation does not undo committed state.

## 61. Cancellation Result

The caller must be told whether:

```text
CancelledBeforeCommit
CancellationRequestedButCommitted
CancellationUnknownRequiresStatusCheck
```

## 62. Ambiguous Completion

If the caller loses confirmation:

1. reuse OperationId;
2. inspect Operation Record;
3. return committed result if available;
4. resume retryable stage when safe;
5. never duplicate the effect.

## 63. Retry Ownership

Retry responsibilities are split:

```text
Infrastructure
    bounded transport retry

Application
    semantic retry and OperationId policy

UI
    user decision and same/new intention distinction
```

## 64. No Hidden Semantic Retry

The command dispatcher MUST NOT silently redispatch a whole command after an ambiguous failure.

## 65. Optimistic Concurrency

Handlers provide expected versions to repositories.

A concurrency conflict maps to a typed result.

The command pipeline does not blindly retry state-changing Domain decisions.

## 66. Safe Automatic Retry

Automatic Application retry MAY occur only when:

- the operation is explicitly classified safe;
- no external nondeterministic output must be regenerated;
- the same OperationId is preserved;
- the handler reloads state;
- retry count is bounded.

## 67. Rule Set Invocation

Rule Set operations execute inside Application orchestration.

They MUST be:

- deterministic;
- side-effect free;
- version-bound;
- supplied with Chronicle-generated raw Dice where needed.

## 68. Provider Invocation

Provider invocation occurs outside the local transaction.

The response remains untrusted until:

- parsed;
- schema-valid;
- reference-valid;
- version-valid;
- visibility-valid;
- Rule Set-valid where relevant.

## 69. Repository Usage

Handlers depend on capability-specific repositories.

They MUST NOT depend directly on `DbContext`.

## 70. Unit of Work

The command transaction behavior controls Unit of Work completion.

Handlers MAY stage repository changes but do not call arbitrary global `SaveChanges`.

## 71. Validation Libraries

A validation library MAY support command-envelope validation.

It MUST remain an implementation detail.

Domain and Rule Set validation remain explicit and authoritative.

## 72. Pipeline Registration

The composition root registers:

- handlers;
- command descriptors;
- behaviors;
- transaction factory;
- mutation coordinator;
- operation services;
- repositories;
- observability.

## 73. Handler Discovery

Handler registration SHOULD be explicit or generated.

Uncontrolled assembly scanning is discouraged for critical command behavior.

## 74. Missing Handler

Dispatching a command without a registered handler is a configuration error.

It fails before any mutation.

## 75. Multiple Handlers

Exactly one primary handler is permitted per command.

Fan-out belongs in explicit post-commit notification mechanisms or durable Work Items.

## 76. Pipeline Reentrancy

A handler MAY dispatch another command only through documented composition rules.

Deep arbitrary nested dispatch is discouraged.

## 77. Command Composition

Prefer Application services or explicit workflow handlers when multiple Domain operations belong to one transaction.

Do not dispatch several independent commands merely to reuse code inside one transaction.

## 78. Child Operations

A parent workflow MAY create child OperationIds for independently retryable stages.

The parent CorrelationId links them.

## 79. Operation Hierarchy

Operation Records MAY include:

```text
ParentOperationId
RootOperationId
CorrelationId
```

## 80. Observability

The pipeline logs or traces:

- command type;
- OperationId;
- CorrelationId;
- CampaignId;
- behavior stage;
- duration;
- result category;
- retry classification;
- commit status;
- affected entity count where safe.

## 81. Sensitive Data

The dispatcher MUST NOT log full command objects.

It logs only approved safe fields.

## 82. Metrics

Useful metrics include:

```text
CommandDuration
CommandFailureCount
ConcurrencyConflictCount
IdempotentReplayCount
FingerprintConflictCount
TransactionDuration
PostCommitFailureCount
DurableContinuationCount
RecoveryCount
```

## 83. Performance

The pipeline SHOULD avoid excessive allocations and reflection.

Correctness and clarity take precedence over micro-optimization.

## 84. Pipeline Overhead

A local no-op or simple command should not incur unnecessary provider, file, or background infrastructure.

Behaviors may short-circuit when not applicable.

## 85. Testing Strategy

The pipeline requires:

```text
Unit Tests
Behavior Order Tests
Handler Tests
SQLite Integration Tests
Concurrency Tests
Recovery Tests
Security Tests
Architecture Tests
```

## 86. Behavior Order Tests

Tests MUST verify the exact behavior order.

A registration change that alters critical ordering must fail.

## 87. Handler Tests

Handlers SHOULD be testable with:

- deterministic IDs;
- fake clock;
- deterministic random source;
- scripted Rule Set;
- fake repositories where appropriate;
- typed transaction stub;
- scripted provider only outside transaction tests.

## 88. SQLite Integration Tests

Tests MUST prove:

- one transaction;
- rollback before commit;
- atomic Operation Record and Domain result;
- optimistic concurrency;
- unique once-only constraints;
- no transaction across provider call.

## 89. Concurrency Tests

Tests SHOULD run competing commands against the same Campaign.

Expected outcomes:

- one succeeds;
- one receives stale or concurrency result;
- no duplicate effect;
- no corrupted aggregate state.

## 90. Recovery Tests

Tests MUST cover:

- persisted player Message with pending provider continuation;
- persisted Roll with missing narration continuation;
- interrupted finalization;
- lost response after Advancement commit;
- expired Work Item lease;
- post-commit notification failure.

## 91. Security Tests

Tests MUST cover:

- cross-Campaign entity ID;
- hidden-only system command through player UI;
- provider-invented identifier;
- command payload oversize;
- credential-bearing command rejection;
- malicious durable payload;
- unsafe result projection.

## 92. Required Test Cases

Tests MUST cover:

- valid command;
- invalid envelope;
- missing handler;
- duplicate handler registration;
- new OperationId;
- completed OperationId replay;
- same OperationId in progress;
- same OperationId failed retryable;
- conflicting fingerprint;
- Campaign lock acquisition;
- cancellation before lock;
- cancellation before transaction;
- cancellation during commit;
- validation failure;
- Domain rule failure;
- Rule Set failure;
- optimistic concurrency conflict;
- repository failure;
- commit failure;
- post-commit failure;
- provider call before transaction;
- stale provider response;
- durable Work Item creation;
- Work Item continuation;
- duplicate Roll click;
- duplicate finalization retry;
- duplicate Advancement retry;
- query with no side effect;
- safe result visibility.

## 93. Architecture Tests

Architecture tests MUST reject:

- direct `DbContext` use in handlers;
- third-party mediator abstractions in public Application contracts;
- provider calls inside persistence transactions;
- UI services in Application handlers;
- multiple primary handlers per command;
- full command object logging;
- static global transaction access;
- Domain code dispatching Application commands;
- queries that call repositories with mutation methods.

## 94. Prohibited Patterns

### 94.1 Transaction Around Provider Call

External waits happen outside local transactions.

### 94.2 Generic Reflection-Driven Magic Pipeline

Critical behavior ordering remains explicit.

### 94.3 Command Without OperationId for Important Mutation

Idempotency is mandatory.

### 94.4 Blind Retry After Concurrency Conflict

Reload and reevaluate explicitly.

### 94.5 Handler Calls SaveChanges Arbitrarily

Transaction behavior controls commit.

### 94.6 Post-Commit Failure Rolls Back History

Committed Domain state remains committed.

### 94.7 Query Mutates State

Use a command.

### 94.8 Logging Whole Command

Only safe structured metadata is logged.

### 94.9 Nested Dispatch as Code Reuse

Use Application services or explicit workflows.

### 94.10 Provider Success Equals Accepted State

Chronicle validates and commits independently.

## 95. Alternatives Considered

### MediatR

Advantages:

- mature ecosystem;
- handler registration;
- pipeline behaviors;
- common .NET usage.

Not selected as an initial architectural dependency because Chronicle needs strict visibility into transaction, idempotency, recovery, and workflow semantics.

A small internal dispatcher is sufficient for MVP and avoids library-shaped architecture.

### Direct Service Calls From ViewModels

Rejected because it would weaken consistent pipeline behaviors, OperationId handling, and observability.

### One Global Transaction Middleware

Rejected because workflows differ and external waits must occur outside transactions.

### Distributed Saga Framework

Rejected because Chronicle is a local modular monolith with durable Work Items, not a distributed service system.

### Event Sourcing Command Bus

Rejected because Chronicle does not use event sourcing as its persistence model.

## 96. Consequences

### Positive

- explicit command semantics;
- consistent idempotency;
- short transactions;
- strong recovery model;
- clear provider boundary;
- testable handlers;
- predictable observability;
- no third-party mediator lock-in;
- durable multi-stage workflows.

### Negative

- custom dispatcher and behaviors must be implemented;
- pipeline metadata adds code;
- handlers require explicit result mapping;
- workflow decomposition may feel verbose;
- Campaign mutation coordination requires careful lifetime management.

## 97. Risks

### Custom Dispatcher Becomes a Framework Project

Mitigation:

- keep surface small;
- implement only Chronicle needs;
- avoid generic feature expansion;
- test behavior order.

### Hidden Nested Commands

Mitigation:

- architecture rules;
- explicit workflow services;
- code review.

### Lock Held Too Long

Mitigation:

- no external waits;
- timing metrics;
- bounded cancellation;
- targeted state loading.

### Operation Record Drift

Mitigation:

- one operation service;
- database constraints;
- contract tests;
- atomic commit.

### Post-Commit Required Work Lost

Mitigation:

- use durable Work Items for correctness-required continuation;
- reserve ephemeral notifications for nonauthoritative refresh.

## 98. Technology Spike

Before acceptance, implement:

1. typed command and result contracts;
2. internal dispatcher;
3. ordered behaviors;
4. execution context;
5. Operation Record inspection;
6. request fingerprint;
7. Campaign mutation coordinator;
8. transaction behavior;
9. one local transactional command;
10. one provider-before-transaction workflow;
11. one durable Work Item continuation;
12. optimistic concurrency conflict;
13. post-commit notification;
14. architecture tests;
15. end-to-end Roll command.

## 99. Spike Acceptance

The spike passes when:

- duplicate OperationId returns the original committed result;
- conflicting fingerprint is rejected;
- provider call occurs with no open SQLite transaction;
- stale provider response is rejected;
- one competing Campaign command fails safely;
- Roll persistence and Operation Record commit atomically;
- post-commit notification failure does not undo the Roll;
- required continuation survives restart through a Work Item;
- handler tests do not require UI or concrete provider SDKs;
- behavior ordering is deterministic and tested.

## 100. Definition of Compliance

An implementation complies when:

- commands and handlers are typed and explicit;
- one internal dispatcher runs an ordered pipeline;
- important mutations require OperationId;
- idempotency is checked before effects;
- Campaign mutation coordination is applied where needed;
- transactions are short and local;
- provider and player waits occur outside transactions;
- Domain state and committed operation result are atomic where required;
- post-commit work cannot reinterpret committed state;
- durable continuation uses Work Items;
- expected failures return typed results;
- architecture tests enforce the boundaries.

## 101. Review Triggers

This ADR must be reviewed if:

- a second application host needs remote command transport;
- multiplayer introduces concurrent remote actors;
- an external message broker is introduced;
- a generic mediator library becomes materially beneficial;
- command volume causes measurable dispatcher overhead;
- distributed transactions or hosted services become necessary;
- plugin commands are allowed;
- the Application layer is split into multiple processes.

## 102. Deferred Decisions

Later ADRs MAY define:

- exact result-union implementation;
- exact request-fingerprint algorithm;
- exact keyed-lock implementation;
- transaction isolation configuration;
- query caching;
- local event publication implementation;
- Activity tracing;
- command authorization policy;
- remote command transport;
- multiplayer command authority.

## 103. Final Decision

Chronicle will use explicit typed commands, one primary handler per command, and a small Chronicle-owned dispatcher with a deterministic behavior pipeline.

Idempotency, Campaign coordination, transactions, recovery, and observability will be visible parts of the Application architecture.

A command may ask Chronicle to change history.

The pipeline must prove that the change happened once, completely, and for the right Campaign.
