---
id: ADR-0016
title: Domain Event Collection and Post-Commit Notification Implementation
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
  - RFC-0001
  - RFC-0016
  - RFC-0017
  - RFC-0018
  - RFC-0019
  - RFC-0033
  - RFC-0036
  - RFC-0040
  - RFC-0042
---

> **"A Domain Event may explain that something happened. It must never make Chronicle uncertain about whether it happened."**

# Domain Event Collection and Post-Commit Notification Implementation

## 1. Status

**Proposed**

This ADR defines how Chronicle collects Domain Events, distinguishes transactional consequences from post-commit notifications, and dispatches local notifications after persistence succeeds.

The decision is:

- Domain entities and aggregates may record immutable Domain Events;
- Domain Events represent already-decided Domain facts, not commands;
- the Application command handler or Unit of Work collects events before commit;
- effects required for correctness execute transactionally before commit;
- post-commit notifications execute only after successful commit;
- post-commit handlers are local, typed, bounded, and nonauthoritative;
- required asynchronous continuation uses durable Work Items rather than ephemeral notifications;
- notification failure never rolls back committed Domain state;
- handlers are idempotent where duplicate local delivery is possible;
- Domain Events are not a substitute for audit records, Operation Records, or persisted Campaign history;
- the MVP will not introduce an external message broker or generic distributed event bus.

The decision becomes **Accepted** after a vertical-slice spike proves:

- aggregate event collection;
- transactional consequence execution;
- commit;
- post-commit publication;
- notification-handler failure;
- duplicate delivery safety;
- UI invalidation;
- Work Item wake-up;
- no notification before commit;
- no rollback after committed notification failure;
- no Domain dependency on Application or Infrastructure.

## 2. Context

Chronicle needs to react to accepted Domain changes such as:

- Campaign created;
- Session started;
- Scene opened;
- player Message accepted;
- Roll requested;
- Dice Roll persisted;
- Session marked for finalization;
- Memory archived;
- Character advanced;
- Preference changed;
- backup completed;
- package compatibility changed.

Some reactions are part of the authoritative change itself.

Examples:

- progression ledger entry created with advancement;
- Memory aging applied during finalization;
- Roll consequence persisted;
- Session lifecycle history appended.

Other reactions are useful but not authoritative.

Examples:

- refresh a ViewModel;
- invalidate a read model;
- wake a worker;
- display a desktop notification;
- update a nonauthoritative metric;
- write an operational log.

If these categories are mixed, Chronicle risks:

- committing partial Domain state;
- publishing notifications for rolled-back work;
- rolling back committed history because a UI refresh failed;
- losing required continuation after process termination;
- creating hidden chains of business behavior;
- coupling Domain code to infrastructure.

ADR-0013 defines the command transaction pipeline.

ADR-0014 defines durable Work Items.

ADR-0015 defines read-model invalidation needs.

This ADR defines the event boundary between them.

## 3. Decision Drivers

The implementation prioritizes:

1. transactional correctness;
2. clear authority boundaries;
3. no pre-commit external side effects;
4. simple local dispatch;
5. testability;
6. bounded handler behavior;
7. failure isolation;
8. deterministic ordering;
9. durable continuation where required;
10. no framework-shaped Domain model;
11. safe observability;
12. future extensibility.

## 4. Decision Summary

Chronicle will use:

```text
Domain Event
    immutable Domain fact
    recorded by aggregate or Domain service

Collection
    aggregate-local event collection
    gathered by Application Unit of Work

Transactional Consequence
    executes before commit
    required for correctness
    persisted in same transaction

Post-Commit Notification
    executes after commit
    local and nonauthoritative
    may fail without rollback

Required Async Continuation
    durable Work Item
    committed with triggering state

Dispatcher
    Chronicle-owned internal notification dispatcher

External Broker
    not used in MVP
```

## 5. Domain Event Definition

A Domain Event states that an accepted Domain fact occurred.

Examples:

```text
CampaignCreated
SessionStarted
SceneOpened
PlayerMessageAccepted
RollRequested
DiceRollResolved
SessionFinalizationStarted
MemoryArchived
CharacterAdvanced
CampaignPreferenceChanged
```

## 6. Domain Event Semantics

A Domain Event is written in past tense because the aggregate has already accepted the transition.

Preferred:

```text
SessionStarted
DiceRollResolved
MemoryArchived
```

Avoid:

```text
StartSession
ResolveDice
ArchiveMemory
```

Those are commands or intentions.

## 7. Domain Event Requirements

A Domain Event SHOULD contain:

- EventId;
- EventTypeKey;
- EventContractVersion;
- OccurredAtUtc supplied through a Domain-safe clock input or Application context;
- AggregateId;
- AggregateVersion;
- OperationId where relevant;
- minimal fact data;
- CampaignId for Campaign-scoped events.

It MUST NOT contain:

- repositories;
- DbContext;
- UI types;
- provider SDK types;
- credentials;
- service provider;
- arbitrary executable callbacks;
- unbounded transcript or sourcebook content.

## 8. Event Identity

Each recorded Domain Event SHOULD receive an `EventId`.

The EventId:

- is distinct from OperationId;
- identifies one event occurrence;
- supports deduplication and diagnostics;
- is generated by Chronicle;
- is not provider-controlled.

## 9. Event Type Key

Every Domain Event type uses a stable semantic key.

Examples:

```text
chronicle.domain.campaign-created
chronicle.domain.session-started
chronicle.domain.player-message-accepted
chronicle.domain.dice-roll-resolved
```

## 10. Event Version

Event type and contract version remain separate.

Example:

```text
EventTypeKey = "chronicle.domain.dice-roll-resolved"
EventContractVersion = "1.0.0"
```

## 11. Event Immutability

Domain Events are immutable after recording.

A later correction is represented by another accepted event or explicit state transition, not by mutating an emitted event.

## 12. Event Collection in Aggregates

Aggregate roots MAY maintain an internal collection of uncommitted Domain Events.

Conceptually:

```csharp
IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
void ClearDomainEvents();
```

The exact API may use a protected recording method and internal collection.

## 13. Event Recording

Domain methods record events only after the invariant-preserving state transition succeeds.

Example:

```text
Validate Session can start
Apply Session state
Increment aggregate version
Record SessionStarted
```

## 14. No Event for Rejected Transition

A rejected command does not produce a success Domain Event.

It may produce:

- typed error;
- safe operational log;
- security event where relevant.

## 15. Aggregate Ownership

An aggregate records events about transitions it owns.

It MUST NOT record facts for unrelated aggregates merely to trigger behavior.

Cross-aggregate orchestration belongs to Application logic.

## 16. Domain Service Events

A Domain service MAY return Domain Events when it owns a decision spanning value objects or several aggregate inputs.

The Application remains responsible for applying and persisting aggregate changes consistently.

## 17. Event Collection Boundary

The Application Unit of Work collects events from tracked aggregates before commit.

The Domain layer does not dispatch them directly.

## 18. Clear Timing

Uncommitted events are cleared only after:

- successful collection into the command execution context;
- and according to implementation, after commit or after safe transfer to persistence orchestration.

A failed commit must not cause the same in-memory aggregate instance to publish success notifications accidentally.

## 19. Event Categories

Chronicle distinguishes three categories.

### 19.1 Domain Fact

The immutable event recorded by Domain logic.

### 19.2 Transactional Consequence

A required state change derived from the Domain fact and committed atomically.

### 19.3 Post-Commit Notification

A nonauthoritative reaction after commit.

## 20. Transactional Consequences

A transactional consequence is required for correctness.

Examples:

- append Session lifecycle history;
- create progression ledger entry;
- apply Roll consequences;
- create Memory aging record;
- persist Preference snapshot;
- enqueue required Work Item;
- update Operation Record result.

## 21. Transactional Consequence Implementation

Transactional consequences SHOULD be executed by:

- the primary command handler;
- an explicit Application service;
- or a deterministic pre-commit Domain Event handler registered as transactional.

The preferred MVP approach is explicit handler or Application service orchestration.

## 22. Transactional Handler Restrictions

A transactional event handler MUST:

- run inside the current transaction;
- be deterministic;
- avoid external calls;
- avoid UI interaction;
- avoid long work;
- avoid hidden command dispatch;
- preserve explicit ordering;
- return typed failure before commit.

## 23. Hidden Business Logic Risk

Chronicle SHOULD avoid placing core use-case behavior in a large network of event handlers.

If a consequence is central to the command's meaning, it belongs visibly in the command handler or an explicit Application service.

## 24. Appropriate Transactional Event Uses

Transactional event handlers are appropriate for narrow cross-cutting Domain persistence concerns such as:

- lifecycle-history append;
- consistent audit metadata;
- explicit local invariant projection;
- required Work Item enqueue.

They are not a substitute for use-case orchestration.

## 25. Post-Commit Notifications

A post-commit notification announces that committed state is available.

Examples:

```text
CampaignChangedNotification
TranscriptChangedNotification
OperationStatusChangedNotification
BackgroundWorkAvailableNotification
DesktopNotificationRequested
```

## 26. Notification Type

Post-commit notifications MAY be derived from Domain Events or explicitly created by the command pipeline.

The external notification contract should remain distinct from the Domain Event when its audience or payload differs.

## 27. Notification Payload

A post-commit notification SHOULD contain only:

- EventId or NotificationId;
- OperationId;
- CorrelationId;
- CampaignId where safe;
- affected entity type and ID;
- safe invalidation category;
- committed version;
- notification contract version.

It SHOULD NOT contain full Domain entities or private prose.

## 28. Notification Dispatcher

Chronicle will implement an internal local notification dispatcher.

Conceptually:

```csharp
public interface IPostCommitNotificationDispatcher
{
    Task PublishAsync(
        IReadOnlyCollection<IPostCommitNotification> notifications,
        CancellationToken cancellationToken);
}
```

## 29. Handler Model

Conceptually:

```csharp
public interface IPostCommitNotificationHandler<TNotification>
{
    Task HandleAsync(
        TNotification notification,
        CancellationToken cancellationToken);
}
```

## 30. Handler Registration

Notification handlers are registered explicitly or through controlled generation.

Critical ordering must not depend on uncontrolled assembly scanning.

## 31. Multiple Handlers

A post-commit notification may have multiple handlers.

Examples:

```text
TranscriptChanged
    → invalidate active-play view
    → notify shell status service
    → emit metric
```

## 32. Notification Ordering

Notifications from one committed command SHOULD preserve the order in which the Application prepared them.

Handlers for one notification SHOULD have deterministic registration order only when ordering is required.

Prefer independent handlers.

## 33. No Ordering Assumption Across Commands

Chronicle does not guarantee global notification ordering across unrelated concurrent commands beyond committed Domain ordering.

Consumers must query authoritative state rather than infer full truth from notification order.

## 34. Commit Boundary

The transaction commits before any post-commit handler executes.

This is mandatory.

## 35. No Notification Before Commit

The system MUST NOT emit a success notification for state that might still roll back.

## 36. Post-Commit Failure

If a post-commit handler fails:

- committed Domain state remains committed;
- the failure is logged safely;
- remaining handlers MAY continue according to handler isolation policy;
- the dispatcher returns a post-commit delivery summary;
- required lost work is recovered through durable Work Items, not retrying the Domain transaction.

## 37. Handler Isolation

The dispatcher SHOULD isolate handler failures.

One failed nonauthoritative handler should not prevent unrelated handlers where safe.

## 38. Critical Post-Commit Work

If a reaction is required for correctness, it must not rely solely on a post-commit notification.

It must be:

- committed transactionally;
- represented by a durable Work Item;
- or derivable and safely reconstructed.

## 39. UI Invalidation

UI invalidation is nonauthoritative.

A lost invalidation notification is recoverable by:

- route activation refresh;
- manual refresh;
- operation-status polling;
- next query.

## 40. Worker Wake-Up

A Work Item wake-up notification is an optimization.

The Work Item row is durable.

If the wake-up is lost, polling or startup recovery finds the work.

## 41. Desktop Notification

A desktop notification is nonauthoritative.

Failure to display it does not alter operation completion.

## 42. Metrics

Metrics emission is nonauthoritative.

Failure to record a metric does not roll back Campaign state.

## 43. Logging

Operational logging may occur:

- during command pipeline stages;
- after commit;
- inside post-commit handlers.

Logs do not own event delivery.

## 44. Event Persistence

Chronicle does not adopt full event sourcing.

Domain Events are not necessarily persisted as a complete replay log.

## 45. Persisted Domain History

Historically important Domain facts are persisted through explicit authoritative records such as:

- lifecycle history;
- Messages;
- Dice Rolls;
- progression ledger;
- Memory lifecycle history;
- Relationship change history;
- audit records.

## 46. Event Audit Metadata

Chronicle MAY persist safe event metadata for diagnostics or audit.

This does not make the event stream the source of truth.

## 47. Audit Record Relationship

A Domain Event MAY cause an Audit Record to be created transactionally.

The Audit Record:

- has its own schema;
- contains safe metadata;
- follows retention policy;
- does not store unrestricted narrative content.

## 48. Operation Record Relationship

Domain Events MAY reference OperationId.

Operation Records remain the authority for idempotency and workflow outcome.

## 49. Durable Work Item Relationship

A Domain Event MAY lead to a Work Item.

The Work Item MUST be persisted in the same transaction if continuation is required.

## 50. Example: Player Message

```text
ProcessPlayerInputCommand
    → Campaign accepts player Message
    → records PlayerMessageAccepted
    → transaction persists Message
    → transaction creates NarrativeContinuation Work Item
    → transaction updates Operation Record
    → commit
    → post-commit TranscriptChanged notification
    → post-commit BackgroundWorkAvailable notification
```

## 51. Example: Dice Roll

```text
ExecuteDiceRollCommand
    → Chronicle generates raw Dice
    → Rule Set resolves outcome
    → Campaign records DiceRollResolved
    → transaction persists Roll and consequence
    → transaction creates narration continuation Work Item
    → commit
    → post-commit ActivePlayChanged
    → post-commit OperationStatusChanged
```

## 52. Example: Session Finalization

```text
ApplyFinalizationProposalCommand
    → Session finalizes
    → Memories age
    → progression Awards are recorded
    → Relationship and Knowledge changes persist
    → SessionFinalized Domain Event recorded
    → transaction commits all required records
    → post-commit CampaignChanged
    → post-commit MemoryTimelineChanged
    → post-commit OperationStatusChanged
```

## 53. Example: Preference Change

```text
ChangeCampaignPreferenceCommand
    → validates Rule Set preference
    → records CampaignPreferenceChanged
    → persists new preference and snapshot
    → commit
    → post-commit ActivePlayChanged
    → post-commit CharacterSheetChanged when relevant
```

## 54. Duplicate Delivery

Post-commit notification delivery is best-effort local delivery and may be duplicated in some recovery designs.

Handlers SHOULD be idempotent where duplication is plausible.

## 55. Idempotent Handler Examples

Safe handlers include:

- mark read-model category invalid;
- wake worker channel;
- set latest operation status;
- increment a deduplicated metric by EventId;
- refresh query state.

## 56. Non-Idempotent Handler Risk

A handler that sends an email, charges money, or creates external state would require a durable outbox and external idempotency design.

Such behavior is outside the MVP.

## 57. Notification Identity

A notification MAY reuse the Domain Event's EventId when it is a one-to-one projection.

Otherwise it receives its own NotificationId and references EventId.

## 58. Delivery Summary

The dispatcher SHOULD return safe delivery metadata:

```text
NotificationCount
HandlerSuccessCount
HandlerFailureCount
FailedHandlerKeys
Duration
```

This summary is diagnostic, not Domain truth.

## 59. Retry of Post-Commit Handler

Ephemeral post-commit handlers SHOULD not have hidden unbounded retries.

A bounded immediate retry MAY be used for trivial local transient failures.

Required retryable work belongs in Work Items.

## 60. Application Shutdown

During graceful shutdown:

- no new post-commit batches should begin after dispatcher stop;
- in-flight bounded handlers may complete;
- lost nonauthoritative notifications remain recoverable by refresh;
- durable Work Items remain persisted.

## 61. Process Crash After Commit

If the process crashes after commit but before post-commit notification:

- authoritative state remains valid;
- UI refresh on restart retrieves current state;
- pending Work Items are recovered;
- no Domain transaction is replayed merely to resend notifications.

## 62. Local Event Stream

The Desktop host MAY expose an in-process typed event stream backed by post-commit notifications.

This stream is for:

- ViewModel invalidation;
- shell status;
- operation dashboard refresh;
- local component coordination.

## 63. No Global Untyped Event Bus

A global string-based or object-based event bus is prohibited.

All notifications are typed and registered.

## 64. Subscription Lifetime

Long-lived UI subscribers MUST dispose subscriptions.

Route-scoped subscribers are removed on deactivation.

## 65. UI Thread Boundary

Post-commit notification handlers that affect Presentation state must schedule work through the approved UI scheduler abstraction.

The Application dispatcher does not depend on Avalonia.

## 66. Handler Execution Context

A post-commit handler context MAY include:

```text
CorrelationId
OperationId
ApplicationVersion
NotificationId
CommittedAtUtc
CancellationToken
Safe Logger
```

It MUST NOT provide:

- DbContext;
- mutable Domain aggregate;
- global service provider;
- credentials.

## 67. Handler Time Limits

Post-commit handlers SHOULD be short.

Long work is converted into a Work Item or handled through query refresh.

## 68. Cancellation

Post-commit cancellation stops local nonauthoritative work.

It does not undo the committed command.

## 69. Result to Caller

The command result should primarily report committed Domain outcome.

A post-commit failure MAY be included as degraded notification status when relevant, but must not report the command itself as uncommitted.

## 70. UI Response

The UI SHOULD reconcile through the committed command result and refreshed query.

It must not assume a missing local notification means the command failed.

## 71. Event Schema Evolution

Domain Event contracts used only in-memory may evolve with the application.

Persisted event metadata or durable event-derived payloads require explicit versioning and migration.

## 72. Event Serialization

In-memory Domain Events do not require generic serialization.

If an event is persisted or becomes a Work Item payload, it is mapped into a dedicated versioned contract.

## 73. No Arbitrary Type Serialization

Chronicle MUST NOT serialize arbitrary Domain Event type names for later reflection-based reconstruction.

## 74. Event Mapping

Mapping from Domain Event to:

- Audit Record;
- Work Item;
- Post-Commit Notification;

is explicit.

## 75. Event Handler Discovery

Registration SHOULD be explicit, generated, or validated at startup.

Missing required transactional mappings must fail before command execution.

## 76. Duplicate Transactional Handler

A duplicated transactional handler registration is a startup configuration error.

## 77. Transactional Ordering

Where several transactional consequences are necessary, ordering MUST be explicit.

Example:

```text
Apply Domain state
Persist lifecycle history
Persist Operation Record result
Enqueue required Work Item
Commit
```

## 78. Event Cascade

A transactional consequence MAY create another Domain Event only when a real Domain transition occurs.

Unbounded event cascades are prohibited.

## 79. Cascade Depth

The implementation SHOULD bound or detect recursive event processing.

A cycle is a configuration or design error.

## 80. Event Processing Completion

Before commit, all required transactional consequences and newly created events must reach a stable processed state.

## 81. Cross-Aggregate Events

A Domain Event from one aggregate may inform an Application decision involving another aggregate.

For the MVP, the command handler should orchestrate the required aggregates explicitly.

The event should not become a hidden distributed workflow.

## 82. Eventual Consistency

The MVP does not use eventual consistency for core Campaign invariants.

Post-commit notifications may be eventually observed by UI, but authoritative state is already committed.

## 83. Read-Model Invalidation

Notifications SHOULD identify invalidation categories rather than embed complete replacement read models.

Examples:

```text
CampaignLibraryChanged
ActivePlayChanged
TranscriptChanged
MemoryTimelineChanged
CharacterSheetChanged
OperationStatusChanged
```

## 84. Fine-Grained Versus Coarse Invalidation

Start with coarse feature-level invalidation.

Introduce fine-grained entity invalidation only when measured query cost justifies it.

## 85. Notification Coalescing

The Desktop host MAY coalesce repeated invalidations within a short bounded window.

Coalescing is Presentation behavior and must not hide operation status changes that require immediate visibility.

## 86. Notification Backpressure

The local notification channel must be bounded.

On overflow:

- coalescible invalidations may merge;
- noncritical metrics may drop;
- failures are logged;
- authoritative state remains unaffected.

## 87. Critical Local Notifications

No local notification is critical for Domain correctness.

Critical user-visible recovery state must remain queryable from Operation Records and Work Items.

## 88. Logging

Event and notification logs MAY include:

- EventId;
- EventTypeKey;
- NotificationId;
- NotificationTypeKey;
- OperationId;
- CampaignId;
- aggregate type;
- aggregate version;
- handler key;
- duration;
- outcome.

They MUST NOT include:

- event payload dumps;
- transcript prose;
- Secret content;
- credentials;
- Character biography;
- full command object.

## 89. Metrics

Useful metrics include:

```text
DomainEventCount
TransactionalEventHandlerDuration
PostCommitNotificationCount
PostCommitHandlerFailureCount
NotificationDispatchDuration
NotificationQueueOverflowCount
NotificationCoalescedCount
```

## 90. Diagnostics

A diagnostic view MAY show:

- event type;
- operation;
- commit time;
- notification delivery summary;
- failed handler key.

It must not expose private payload content by default.

## 91. Testing Strategy

The implementation requires:

```text
Unit Tests
Aggregate Tests
Pipeline Integration Tests
SQLite Integration Tests
Failure Injection Tests
UI Invalidation Tests
Architecture Tests
Security Tests
```

## 92. Aggregate Tests

Aggregate tests SHOULD prove:

- accepted transition records expected event;
- rejected transition records no success event;
- event contains correct aggregate version;
- events are immutable;
- clearing follows expected lifecycle.

## 93. Transactional Handler Tests

Tests MUST prove:

- handler executes before commit;
- failure rolls back transaction;
- external calls are absent;
- duplicate registration fails;
- ordering is deterministic;
- required Work Item enqueues atomically.

## 94. Post-Commit Tests

Tests MUST prove:

- notification occurs only after commit;
- rollback produces no notification;
- handler failure does not roll back;
- remaining handlers continue where policy allows;
- duplicate delivery is safe;
- cancellation does not undo state.

## 95. Crash Tests

Tests SHOULD inject process failure:

- before commit;
- after commit before dispatch;
- during first notification handler;
- after Work Item enqueue before wake-up.

Expected state must remain recoverable.

## 96. UI Invalidation Tests

Tests MUST cover:

- transcript invalidation;
- Character Sheet invalidation;
- Memory timeline invalidation;
- operation dashboard invalidation;
- lost notification followed by route refresh;
- duplicate notification coalescing.

## 97. Security Tests

Tests MUST prove:

- hidden data absent from notification payload;
- no credentials;
- no raw provider response;
- no full Domain entity serialization;
- player UI cannot subscribe to Director-only notification payloads;
- logs exclude private event payloads.

## 98. Required Test Cases

Tests MUST cover:

- CampaignCreated event;
- SessionStarted event;
- PlayerMessageAccepted event;
- DiceRollResolved event;
- SessionFinalized event;
- rejected command no success event;
- transactional history append;
- transactional Work Item enqueue;
- commit success;
- rollback;
- notification after commit;
- no notification before commit;
- notification-handler exception;
- multiple handlers;
- deterministic handler order;
- duplicate notification;
- recursive event detection;
- shutdown during notification;
- crash after commit;
- UI refresh without notification;
- operation status remains queryable;
- event version mapping.

## 99. Architecture Tests

Architecture tests MUST reject:

- Domain referencing notification dispatcher;
- Domain referencing Application or Infrastructure;
- post-commit handlers mutating Campaign repositories;
- provider calls from transactional handlers;
- external transport in post-commit handlers without later ADR;
- generic untyped event bus;
- reflection-based arbitrary event deserialization;
- full Domain entity payloads in notifications;
- required continuation implemented only as ephemeral notification.

## 100. Prohibited Patterns

### 100.1 Publish Before Commit

A rolled-back change must never produce a success notification.

### 100.2 Notification Owns Correctness

Required work uses the transaction or a durable Work Item.

### 100.3 Domain Dispatches Its Own Events

Application controls collection and dispatch.

### 100.4 Event as Command

Events state facts; commands express intentions.

### 100.5 Event Handler Network Call Inside Transaction

External work is staged outside transaction.

### 100.6 Global Untyped Event Bus

All events and notifications are typed.

### 100.7 Full Aggregate in Notification

Use IDs, versions, and invalidation categories.

### 100.8 Notification Failure Rolls Back Commit

Committed history remains committed.

### 100.9 Event Sourcing by Accident

Domain Events do not replace authoritative relational records.

### 100.10 Hidden Business Workflow in Handler Cascade

Core orchestration remains visible in command handlers and Work Items.

## 101. Alternatives Considered

### Generic Mediator Notifications

A mediator library could dispatch notifications.

Not selected as an architectural dependency because Chronicle already uses internal command and query dispatchers and needs explicit commit-boundary behavior.

### External Message Broker

Rejected for MVP because Chronicle is a local desktop modular monolith.

### Database Outbox for All Notifications

Not required for local nonauthoritative UI invalidations.

Durable Work Items already cover required continuation.

A true outbox may be introduced later for external integrations.

### No Domain Events

Possible, but would make important accepted facts and cross-cutting transactional consequences less explicit.

### Full Event Sourcing

Rejected because Chronicle persists authoritative current and historical models directly and does not need complete aggregate reconstruction from events.

## 102. Consequences

### Positive

- explicit Domain facts;
- no notification before commit;
- clear separation of correctness and convenience;
- safe UI invalidation;
- robust crash behavior;
- no external messaging dependency;
- testable event collection;
- future external integration path remains open.

### Negative

- event mapping adds code;
- transactional versus post-commit classification requires discipline;
- handler registration and ordering need tests;
- duplicate-safe notification handlers add complexity;
- developers may overuse events if conventions are not enforced.

## 103. Risks

### Hidden Event-Driven Workflow

Mitigation:

- use handlers only for narrow concerns;
- keep use-case orchestration explicit;
- architecture review;
- cascade-depth checks.

### Missed Post-Commit Notification

Mitigation:

- authoritative query refresh;
- durable Work Items for required work;
- route activation reload;
- operation dashboard.

### Duplicate Notification

Mitigation:

- idempotent handlers;
- EventId;
- invalidation semantics;
- coalescing.

### Sensitive Payload Leakage

Mitigation:

- minimal contracts;
- no entity serialization;
- security tests;
- safe logging.

### Event Version Drift

Mitigation:

- explicit version on persisted mappings;
- dedicated durable contracts;
- migration fixtures.

## 104. Technology Spike

Before acceptance, implement:

1. `IDomainEvent`;
2. aggregate event collection;
3. event type keys and versions;
4. Unit of Work event collection;
5. one transactional event consequence;
6. one required Work Item enqueue;
7. post-commit notification dispatcher;
8. transcript invalidation handler;
9. operation-status invalidation handler;
10. worker wake-up handler;
11. handler-failure isolation;
12. duplicate-delivery test;
13. crash-after-commit test;
14. architecture rules;
15. end-to-end Dice Roll event flow.

## 105. Spike Acceptance

The spike passes when:

- accepted aggregate transitions record expected events;
- rejected transitions record no success event;
- required consequences commit atomically;
- notifications are absent on rollback;
- notifications begin only after commit;
- failed notification handlers do not undo committed state;
- a lost worker wake-up does not lose the Work Item;
- duplicate notification delivery does not duplicate Domain effects;
- no Domain project references dispatcher or Infrastructure;
- no private narrative data appears in notification payloads or logs.

## 106. Definition of Compliance

An implementation complies when:

- Domain Events are immutable accepted facts;
- aggregates record but do not dispatch events;
- Application collects events;
- correctness-required consequences occur before commit;
- required async continuation uses durable Work Items;
- post-commit notifications execute only after commit;
- notification failure cannot roll back Domain state;
- notification payloads are minimal and safe;
- handlers are typed and bounded;
- UI invalidation is recoverable through queries;
- event sourcing and external brokers are not introduced implicitly;
- architecture and failure tests enforce the boundary.

## 107. Review Triggers

This ADR must be reviewed if:

- Chronicle introduces external integrations requiring guaranteed delivery;
- multiplayer or server hosting is added;
- a network API exposes event subscriptions;
- a message broker is introduced;
- asynchronous read projections become authoritative;
- plugins may register event handlers;
- Domain event volume becomes a performance concern;
- audit requirements require durable event retention;
- the application is split into multiple processes.

## 108. Deferred Decisions

Later ADRs MAY define:

- exact event base interfaces;
- exact handler registration mechanism;
- outbox for external integrations;
- event retention policy;
- plugin event subscriptions;
- external event contracts;
- distributed event ordering;
- notification coalescing implementation;
- Activity tracing for event dispatch;
- durable UI notification history.

## 109. Final Decision

Chronicle will collect immutable Domain Events from aggregates and use them to make accepted facts explicit.

Required consequences will complete inside the command transaction or through a durable Work Item committed with that transaction.

Post-commit notifications will remain local, typed, bounded, and nonauthoritative.

Chronicle may notify many parts of the application that history changed.

Only the committed transaction may decide that it did.
