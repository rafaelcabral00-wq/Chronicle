---
id: ADR-0015
title: Application Query Dispatch and Read Model Implementation
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
  - ADR-0013
  - ADR-0014
  - RFC-0010
  - RFC-0016
  - RFC-0017
  - RFC-0018
  - RFC-0033
  - RFC-0035
  - RFC-0036
  - RFC-0039
  - RFC-0040
  - RFC-0042
---

> **"A read model may simplify Chronicle's truth for a purpose. It must never invent, broaden, or conceal authority."**

# Application Query Dispatch and Read Model Implementation

## 1. Status

**Proposed**

This ADR defines Chronicle's Application query dispatch model and read-model implementation.

The decision is:

- use explicit typed query records and one primary handler per query;
- use a small Chronicle-owned query dispatcher;
- return purpose-specific immutable read models;
- project directly from persistence where aggregate behavior is unnecessary;
- use no-tracking EF Core queries by default;
- permit reviewed parameterized SQL for complex or performance-critical projections;
- enforce Campaign ownership, visibility, Secret filtering, and Character perspective before results leave Application boundaries;
- keep queries side-effect free;
- use explicit pagination, ordering, filtering, and continuation contracts;
- use version and freshness metadata where UI reconciliation requires it;
- avoid loading complete Campaign aggregates for ordinary screens;
- keep derived read models rebuildable;
- defer distributed CQRS, external read databases, and asynchronous projections until evidence requires them.

The decision becomes **Accepted** after a vertical-slice spike proves:

- Campaign list query;
- active play query;
- transcript pagination;
- Character Sheet projection;
- Memory timeline;
- Character Knowledge perspective;
- hidden Secret exclusion;
- durable operation-status projection;
- deterministic ordering;
- cancellation;
- stale-view reconciliation;
- acceptable performance on the canonical and complex Campaign fixtures.

## 2. Context

Chronicle's desktop application needs read views for:

- Campaign library;
- Campaign setup;
- active Session;
- live transcript;
- pending Roll;
- Character Sheet;
- Memories;
- Relationships;
- Character Knowledge;
- progression;
- Campaign Preferences;
- finalization progress;
- provider health;
- backup and restore;
- diagnostics;
- Rule Knowledge search.

These views do not require the same shape as Domain aggregates.

A UI-oriented query often needs:

- selected fields from several tables;
- safe display metadata;
- deterministic ordering;
- pagination;
- computed labels;
- current operation status;
- visibility-aware projections.

Loading and exposing complete Domain aggregates would:

- overfetch data;
- leak authority and hidden information;
- couple UI to persistence and Domain shape;
- increase memory use;
- complicate versioning;
- encourage accidental mutation.

RFC-0017 separates commands from queries.

ADR-0013 defines the command dispatcher and transaction pipeline.

ADR-0004 defines persistence, projections, no-tracking reads, and rebuildable derived models.

This ADR selects the concrete query approach.

## 3. Decision Drivers

The query architecture prioritizes:

1. read-side safety;
2. explicit visibility;
3. purpose-specific contracts;
4. predictable performance;
5. deterministic ordering;
6. cancellation;
7. low coupling to Domain entities;
8. minimal data transfer;
9. UI reconciliation support;
10. testability;
11. local-first simplicity;
12. future client reuse.

## 4. Decision Summary

Chronicle will use:

```text
Query Model
    typed immutable query records

Handler Model
    one primary handler per query

Dispatcher
    Chronicle-owned internal query dispatcher

Read Models
    purpose-specific immutable DTOs

Persistence Access
    no-tracking EF Core projections by default
    reviewed parameterized SQL when justified

Security
    ownership and visibility filtering in Application
    hidden data absent from result

Ordering
    explicit stable sort keys
    never database-default order

Pagination
    cursor-based where sequence exists
    offset-based only for bounded administration views

Freshness
    projection version and query timestamp where useful

Caching
    none by default
    explicit, bounded, invalidatable when later justified

Side Effects
    prohibited
```

## 5. Query Definition

A query requests information without changing authoritative state.

Examples:

```text
ListCampaignsQuery
GetCampaignWorkspaceQuery
GetActivePlayStateQuery
GetSceneTranscriptQuery
GetCharacterSheetQuery
GetMemoryTimelineQuery
GetRelationshipViewQuery
GetCharacterKnowledgeQuery
GetProgressionOptionsQuery
GetCampaignPreferencesQuery
GetOperationStatusQuery
GetRecoveryItemsQuery
```

## 6. Query Requirements

A query SHOULD contain:

- target identifiers;
- actor or perspective context;
- filter;
- ordering;
- pagination;
- locale where relevant;
- projection contract version where crossing durable boundaries.

A query MUST NOT contain:

- DbContext;
- repositories;
- UI controls;
- provider SDK objects;
- mutable Domain entities;
- executable expressions;
- unrestricted SQL or FTS syntax.

## 7. Query Immutability

Queries are immutable after creation.

Record-like value semantics are preferred.

## 8. Query Naming

Queries use descriptive read intent names.

Preferred:

```text
GetActivePlayStateQuery
ListCampaignsQuery
GetMemoryTimelineQuery
```

Avoid:

```text
GetDataQuery
LoadEverythingQuery
CampaignQuery
SearchQuery
```

## 9. Query Handler

Conceptually:

```csharp
public interface IQueryHandler<TQuery, TResult>
{
    Task<TResult> HandleAsync(
        TQuery query,
        QueryExecutionContext context,
        CancellationToken cancellationToken);
}
```

## 10. Handler Responsibilities

A query handler owns:

- query validation;
- visibility-aware selection;
- persistence projection;
- deterministic ordering;
- pagination;
- mapping to immutable read models;
- freshness metadata;
- typed error mapping.

It MUST NOT:

- mutate state;
- call `SaveChanges`;
- enqueue Work Items;
- modify operation status;
- generate authoritative identifiers;
- call remote providers;
- infer hidden truth for player views;
- return persistence entities.

## 11. Query Dispatcher

Chronicle will implement a small internal dispatcher.

Conceptually:

```csharp
public interface IQueryDispatcher
{
    Task<TResult> DispatchAsync<TQuery, TResult>(
        TQuery query,
        CancellationToken cancellationToken);
}
```

## 12. No Generic Mediator Requirement

The MVP does not require a third-party mediator library for queries.

The internal dispatcher keeps:

- handler registration;
- validation;
- visibility;
- observability;
- cancellation;
- result mapping;

explicit and testable.

## 13. Query Pipeline

The default query pipeline SHOULD execute:

```text
1. Initialize query context
2. Validate query envelope
3. Resolve actor and perspective
4. Apply authorization and visibility policy
5. Execute projection
6. Apply deterministic ordering and pagination
7. Map typed result
8. Emit safe observability
```

## 14. Query Execution Context

Recommended fields:

```text
CorrelationId
ActorContext
CampaignId when scoped
PerspectiveCharacterId when relevant
Locale
StartedAtUtc
ApplicationVersion
```

Queries do not require OperationId for idempotency.

## 15. Correlation

Queries MAY use CorrelationId to connect:

- UI interaction;
- Application query;
- persistence projection;
- diagnostics.

CorrelationId is not identity or authorization.

## 16. Read Model Principle

A read model exists for one purpose.

Examples:

```text
CampaignListItemReadModel
ActivePlayReadModel
TranscriptMessageReadModel
CharacterSheetReadModel
MemoryTimelineItemReadModel
OperationStatusReadModel
```

## 17. No Domain Entity Return

Query handlers MUST NOT return Domain entities directly.

### Rationale

Domain entities:

- expose mutation methods;
- reflect aggregate structure rather than screen purpose;
- may contain hidden state;
- may encourage long-lived tracking;
- complicate serialization.

## 18. Read Model Immutability

Read models SHOULD be immutable.

Collections SHOULD be read-only and bounded.

## 19. Read Model Versioning

Read models used only inside one application release may evolve with the application.

Contracts shared with:

- plugins;
- packages;
- external clients;
- portable artifacts;

require explicit contract versions.

## 20. Projection Sources

A query handler MAY project from:

- authoritative relational tables;
- immutable snapshots;
- derived read tables;
- operation records;
- Work Item status;
- Rule Set package metadata.

## 21. Aggregate Loading

A query SHOULD not load a Domain aggregate merely to display data.

Aggregate loading is justified only when the read requires Domain-owned derived behavior that cannot be represented safely as a projection.

## 22. EF Core Query Policy

Queries SHOULD use:

```text
AsNoTracking
```

by default.

Tracking is prohibited unless a reviewed read workflow requires identity resolution without mutation.

## 23. Direct SQL

Parameterized SQL is permitted for:

- complex projections;
- recursive or window queries;
- FTS retrieval;
- performance-critical timelines;
- integrity views.

SQL remains inside approved persistence adapters.

## 24. SQL Safety

Direct SQL MUST:

- use parameters;
- return a typed projection;
- be covered by SQLite integration tests;
- preserve visibility filtering;
- avoid dynamic identifier interpolation;
- remain versioned with schema migrations.

## 25. Projection Repositories

The read side MAY expose purpose-specific interfaces such as:

```text
ICampaignLibraryReader
IActivePlayReader
ITranscriptReader
ICharacterSheetReader
IMemoryTimelineReader
IOperationStatusReader
```

A generic read repository is discouraged.

## 26. Application Ownership

Persistence adapters execute the query.

Application handlers own:

- actor context;
- visibility intent;
- result contract;
- safe error mapping.

## 27. Visibility Is Not UI Filtering

Hidden data MUST be absent from the read model.

The UI must not receive:

- unrevealed Secrets;
- canonical truth outside Character perspective;
- hidden modifiers;
- Director-only notes;
- restricted provider metadata;
- credentials.

## 28. Campaign Ownership

Every Campaign-scoped query validates that referenced entities belong to the requested Campaign.

A valid ID from another Campaign is rejected.

## 29. Perspective

Queries that expose subjective information SHOULD require an explicit perspective.

Examples:

```text
PerspectiveCharacterId
ViewerRole
KnowledgeScope
```

## 30. Character Knowledge

A Character Knowledge read model includes:

- what the Character knows;
- believes;
- suspects;
- misunderstands;
- confidence where supported;
- evidence visible to that Character.

It does not include hidden canonical truth.

## 31. Secrets

A Secret query projection must enforce:

- reveal status;
- viewer role;
- Character visibility;
- Campaign scope;
- historical reveal rules.

## 32. Player-Safe Model

Player-facing queries SHOULD use dedicated models rather than a boolean `includeSecrets`.

## 33. Director Views

Director-facing views may receive broader data only through explicitly authorized query types.

They must not be reachable through ordinary player routes by changing one parameter.

## 34. Transcript Query

Transcript queries MUST:

- filter by Campaign, Session, Act, and Scene ownership;
- order by explicit sequence number;
- distinguish accepted from provisional content;
- exclude staging data from ordinary accepted transcript;
- support bounded pagination.

## 35. Transcript Pagination

Cursor-based pagination SHOULD use stable keys such as:

```text
SceneId
SequenceNumber
MessageId
```

## 36. Pagination Direction

Transcript queries MAY support:

- older messages;
- newer messages;
- range around a referenced Message.

## 37. Cursor Contract

A cursor SHOULD be:

- opaque to UI logic;
- versioned;
- signed or validated if exposed outside process;
- independent from database row offsets.

For the local MVP, a typed internal cursor may use sequence values directly.

## 38. Offset Pagination

Offset pagination MAY be used for:

- small settings lists;
- bounded package inventory;
- diagnostics views.

It is discouraged for large mutable timelines.

## 39. Deterministic Ordering

Every collection query MUST declare an order.

Examples:

```text
Campaigns
    UpdatedAtUtc descending, CampaignId ascending

Messages
    SequenceNumber ascending, MessageId ascending

Memories
    importance, recency, stable ID according to view policy

Operations
    LastUpdatedAtUtc descending, OperationId ascending
```

Database default order is prohibited.

## 40. Ordering and UUID v7

UUID v7 MAY be used as a stable tie-breaker.

It does not replace explicit Domain sequence.

## 41. Filtering

Filters SHOULD be typed.

Examples:

```text
MemoryStatusFilter
OperationStatusFilter
SessionRangeFilter
SourceTypeFilter
KnowledgeStateFilter
```

Free-form predicates from UI are prohibited.

## 42. Search

User-facing search strings are untrusted.

Search handlers MUST:

- enforce length limits;
- normalize safely;
- escape query syntax;
- use parameterized persistence access;
- limit result count.

## 43. Empty State

An empty result is distinct from:

- not found;
- unauthorized;
- unavailable;
- failed;
- not yet initialized.

Read models or result unions should preserve that distinction.

## 44. Not Found

A query for one entity returns a typed not-found result.

It does not return an empty default object that appears valid.

## 45. Degraded Result

Some queries MAY return degraded data with explicit metadata.

Examples:

- Rule Knowledge index unavailable;
- provider health unknown;
- historical package missing;
- optional diagnostic source unavailable.

Degradation must be visible and must not invent values.

## 46. Freshness Metadata

A read model MAY include:

```text
ProjectionVersion
CampaignAggregateVersion
EntityVersion
GeneratedAtUtc
SourceRevision
IsDegraded
```

## 47. Expected Version Use

The UI MAY use returned versions when constructing commands.

The command pipeline remains authoritative.

## 48. Stale UI Reconciliation

After a command completes, the UI should refresh relevant queries.

When a query result is older than a committed operation:

- refresh;
- preserve safe drafts;
- avoid optimistic claims of authoritative completion.

## 49. Query Consistency

Core local queries SHOULD be transactionally current against committed SQLite state.

The MVP does not require eventually consistent external projections.

## 50. Multi-Table Snapshot

A query spanning several related tables SHOULD use one consistent database read scope where inconsistent combinations would be misleading.

## 51. Read Transactions

A short read transaction MAY be used for:

- active play snapshot;
- backup inspection;
- integrity screen;
- finalization summary;
- multi-table Character view.

It must not become a long-lived UI session.

## 52. Derived Read Tables

Chronicle MAY introduce derived read tables for:

- Campaign cards;
- operation dashboard;
- search index metadata;
- progression balances;
- memory relevance ordering.

## 53. Derived Table Authority

Derived tables are rebuildable.

Authoritative truth remains in source tables and Domain history.

## 54. Synchronous Projection Updates

For core MVP views, derived rows SHOULD update in the same transaction as authoritative state when simple and reliable.

## 55. Asynchronous Projection Updates

Asynchronous projection is deferred unless:

- write latency becomes problematic;
- staleness is acceptable;
- rebuild and recovery are implemented;
- UI clearly communicates freshness.

## 56. Query Caching

Caching is disabled by default.

### Rationale

Chronicle is local, SQLite queries are expected to be fast, and invalidation complexity can outweigh benefit.

## 57. Cache Introduction

A cache may be introduced only after measurement.

It MUST define:

- key;
- scope;
- lifetime;
- invalidation;
- visibility context;
- memory bounds;
- stale behavior.

## 58. No Cross-Perspective Cache Reuse

A read model generated for one Character perspective or viewer role must not be reused for another unless the cache key includes the full visibility context.

## 59. In-Memory ViewModel State

UI-local state is not an Application query cache.

ViewModels may retain a current read model for presentation until refresh.

## 60. Incremental Refresh

Queries MAY support incremental refresh through:

- `afterSequence`;
- `afterVersion`;
- changed entity IDs;
- operation status since timestamp.

## 61. Invalidations

Post-commit notifications MAY announce safe invalidation categories:

```text
CampaignLibraryChanged
ActivePlayChanged
TranscriptChanged
CharacterSheetChanged
MemoryTimelineChanged
OperationStatusChanged
```

## 62. Invalidation Is Advisory

A lost invalidation signal must not make data inaccessible.

Manual or lifecycle refresh still retrieves current state.

## 63. Read Model Composition

A screen-level query MAY compose several projections when:

- one consistent snapshot is needed;
- network round trips do not exist;
- composition remains bounded;
- result shape is screen-specific.

## 64. Avoid Mega Query Models

A query must not return the entire Campaign graph merely to avoid multiple calls.

## 65. Campaign Library Query

`ListCampaignsQuery` SHOULD return only card-level fields such as:

```text
CampaignId
Title
RuleSetDisplayName
Status
PlayerCharacterDisplayName
LastPlayedAtUtc
ActiveOperationSummary
CompatibilityState
AggregateVersion
```

No transcript or Character Sheet payload is loaded.

## 66. Active Play Query

`GetActivePlayStateQuery` MAY return:

- Campaign identity;
- active Session, Act, and Scene;
- recent transcript window;
- pending Roll;
- player-input availability;
- operation status;
- safe Character summary;
- degraded-state indicators.

## 67. Character Sheet Query

The Character Sheet read model SHOULD contain:

- schema identity and version;
- display sections;
- typed field values;
- validation/display metadata;
- read-only and derived state;
- entity version.

It must not expose the persistence JSON shape directly.

## 68. Memory Timeline Query

The Memory timeline SHOULD support:

- active or archived status;
- origin Session;
- age;
- relevance;
- importance;
- scope;
- remembered-by;
- deterministic filters and ordering.

## 69. Relationship Query

The Relationship model MUST preserve directionality.

It should not flatten:

```text
A trusts B
```

and:

```text
B trusts A
```

into one symmetric row unless the Rule Set defines symmetry.

## 70. Progression Query

Progression options SHOULD be produced through a Rule Set-aware Application service.

The query may combine:

- current ledger;
- Character state;
- Rule Set catalog;
- Preferences;
- version.

It remains read-only.

## 71. Operation Status Query

The operation dashboard SHOULD combine:

- Operation Record;
- active Work Item;
- latest attempt;
- safe error;
- available recovery actions.

## 72. Recovery Action Projection

Available actions are derived from typed operation state.

Examples:

```text
RetrySameOperation
RepairCredential
Refresh
Resume
Cancel
OpenDiagnostics
RestoreCheckpoint
StartNewOperation
```

The UI does not invent recovery semantics.

## 73. Provider Health Query

Provider health read models include:

- profile identity;
- configured state;
- credential health;
- compatibility;
- rate-limit or quota state where known;
- last successful test time.

They do not expose credentials.

## 74. Query Cancellation

Queries MUST accept `CancellationToken`.

Obsolete UI queries SHOULD be canceled when:

- route changes;
- filter changes;
- a newer refresh supersedes the old one;
- application shuts down.

## 75. Cancellation Safety

Query cancellation has no Domain rollback requirement because queries do not mutate.

Persistence resources must still be disposed.

## 76. Query Timeout

Selected expensive queries MAY have bounded timeouts.

A timeout maps to a typed unavailable or timeout result.

## 77. Query Concurrency

Multiple read queries may execute concurrently.

They must not interfere with the one-mutating-operation-per-Campaign policy.

SQLite connection and busy behavior still require testing.

## 78. Read During Write

Queries may observe only committed state.

They must not expose partially committed command state.

## 79. WAL Behavior

When WAL mode is selected, readers should remain responsive during short writes.

The actual behavior must be validated through integration tests.

## 80. Error Model

Expected query errors include:

```text
QueryInvalid
EntityNotFound
CampaignNotFound
CrossCampaignReference
AccessDenied
PerspectiveRequired
ProjectionUnavailable
PackageUnavailable
QueryTimedOut
StorageUnavailable
UnsupportedProjectionVersion
```

## 81. Exception Handling

Infrastructure exceptions are mapped at the query boundary.

Raw database exceptions do not reach Presentation.

## 82. Observability

Query observability SHOULD include:

- query type;
- CorrelationId;
- CampaignId where safe;
- result category;
- duration;
- row count;
- pagination size;
- degraded flag;
- cache status if caching exists later.

## 83. Privacy

Logs MUST NOT include:

- free-form query text;
- transcript content;
- Character names by default;
- Secret content;
- full paths;
- Character Knowledge statements.

## 84. Slow Query Logging

Slow queries MAY log:

- query type;
- duration;
- safe filter categories;
- row count;
- query-plan identifier where available.

They must not log sensitive parameters.

## 85. Metrics

Useful metrics include:

```text
QueryDuration
QueryFailureCount
QueryCancellationCount
RowsProjected
TranscriptPageDuration
MemoryTimelineDuration
VisibilityRejectionCount
DegradedProjectionCount
```

## 86. Performance Budgets

Exact budgets require measurement.

The spike SHOULD establish baselines for:

- Campaign library;
- active play;
- transcript page;
- Character Sheet;
- Memory timeline;
- operation dashboard.

## 87. Large Campaign Strategy

For large Campaigns:

- transcript is virtualized and paged;
- Memories are paged or incrementally loaded;
- historical Plans are loaded on demand;
- Character details are not loaded with Campaign list;
- operation history is bounded.

## 88. Query Plan Review

Performance-critical queries SHOULD have reviewed indexes and query plans.

A query should not add an index without a known workload.

## 89. N+1 Prevention

Handlers SHOULD use projections and bounded joins rather than per-row repository calls.

N+1 query behavior in core screens is prohibited.

## 90. Data Size Limits

Read models SHOULD define maximum:

- collection size;
- string size;
- page size;
- nested depth.

## 91. Page Size

Each paged query MUST define:

- default page size;
- maximum page size;
- stable ordering;
- continuation behavior.

## 92. Localization

Read models SHOULD carry:

- machine keys;
- raw values;
- localization keys where appropriate.

Presentation performs ordinary UI localization.

Generated narrative remains stored content and is not retranslated by the query layer.

## 93. Formatting

Queries SHOULD not format values into presentation-specific strings unless the value is intrinsically display-only.

Dates, durations, numbers, and file sizes usually remain typed.

## 94. Nullability

Read model nullability must represent actual Domain absence.

Null must not ambiguously mean:

- hidden;
- not loaded;
- unavailable;
- unknown.

Use explicit status fields or unions.

## 95. Test Fixtures

Query tests use synthetic Campaign fixtures covering:

- minimal Campaign;
- active Session;
- long transcript;
- hidden Secrets;
- conflicting Character beliefs;
- archived Memories;
- failed Work Items;
- historical package versions.

## 96. Testing Strategy

The query implementation requires:

```text
Unit Tests
Handler Contract Tests
SQLite Integration Tests
Visibility Tests
Pagination Tests
Performance Tests
Security Tests
Architecture Tests
```

## 97. Unit Tests

Unit tests SHOULD cover:

- validation;
- filter mapping;
- cursor parsing;
- deterministic ordering;
- empty state;
- error mapping;
- read-model construction.

## 98. SQLite Integration Tests

Tests MUST use the real schema and SQLite provider.

They SHOULD cover:

- no-tracking projection;
- joins;
- indexes;
- read transaction;
- cancellation;
- pagination;
- concurrent writer behavior;
- direct SQL projection where used.

## 99. Visibility Tests

Tests MUST prove:

- unrevealed Secrets absent;
- Character perspective respected;
- Director-only query isolated;
- cross-Campaign identifiers rejected;
- credential metadata sanitized;
- restricted Rule Knowledge not returned to unauthorized views.

## 100. Pagination Tests

Tests MUST cover:

- first page;
- next page;
- previous page where supported;
- empty page;
- deleted or archived boundary item;
- concurrent append;
- stable cursor;
- maximum page size;
- invalid cursor.

## 101. Performance Tests

The complex Campaign fixture SHOULD include:

- many Sessions;
- long transcript;
- many Memories;
- many Relationships;
- operation history;
- multiple package versions.

## 102. Required Test Cases

Tests MUST cover:

- Campaign list;
- empty library;
- active Campaign workspace;
- no active Session;
- active Scene;
- pending Roll;
- transcript ordered by sequence;
- transcript pagination;
- provisional Message excluded from accepted view;
- Character Sheet projection;
- schema version metadata;
- Memory filters;
- archived Memory;
- directional Relationship;
- Character belief without canonical truth;
- hidden Secret;
- revealed Secret;
- progression balance;
- operation pending;
- operation failed;
- Work Item recovery action;
- provider health without credential;
- cancellation;
- timeout;
- storage unavailable;
- stale projection version;
- concurrent committed append;
- no N+1 on Campaign list.

## 103. Architecture Tests

Architecture tests MUST reject:

- query handlers calling `SaveChanges`;
- query handlers returning Domain entities;
- Presentation using DbContext or SQL;
- hidden data filtering only in UI;
- unbounded collection results;
- query handlers calling remote Narrative Intelligence;
- query handlers enqueuing Work Items;
- generic repository use that returns persistence entities;
- dynamic SQL from UI input.

## 104. Prohibited Patterns

### 104.1 Query Mutates State

Use a command.

### 104.2 Return Aggregate to ViewModel

Return a purpose-specific read model.

### 104.3 UI Hides Secret Fields

Hidden fields must not be selected.

### 104.4 Database Default Ordering

Every collection has an explicit order.

### 104.5 Load Entire Campaign for a Card

Project only required fields.

### 104.6 Offset Pagination for Long Mutable Transcript

Use sequence-aware cursors.

### 104.7 Generic `GetAll`

All list queries are bounded and purpose-specific.

### 104.8 Cache Without Visibility Context

Cache keys include perspective and authorization.

### 104.9 Query Calls Provider

Remote generation is a command or Work Item workflow.

### 104.10 Null Means Hidden or Unavailable

Use explicit state.

## 105. Alternatives Considered

### Return Domain Aggregates

Rejected because it couples the UI to mutation-capable Domain shape and risks hidden-data leakage.

### Generic Repository Read Methods

Rejected because purpose-specific projections provide clearer performance and visibility contracts.

### Full CQRS With Separate Read Database

Deferred because Chronicle is a local single-process application and SQLite projections are sufficient for MVP.

### Eventual-Consistency Projections

Deferred because core screens benefit from immediately committed local state.

### GraphQL Layer

Rejected for the desktop MVP because it adds schema, execution, authorization, and tooling complexity without a network-client requirement.

### OData or Dynamic Query API

Rejected because unrestricted dynamic querying weakens performance, visibility, and contract control.

## 106. Consequences

### Positive

- safe UI projections;
- strong hidden-information boundary;
- lower memory and I/O;
- predictable query performance;
- explicit pagination;
- easier ViewModel testing;
- clean future client contracts;
- no dependency on distributed read infrastructure.

### Negative

- many purpose-specific DTOs and handlers;
- projection mapping adds code;
- visibility rules require careful testing;
- some read-model duplication is intentional;
- complex SQL may require maintenance across migrations.

## 107. Risks

### Read Model Explosion

Mitigation:

- one model per real use case;
- shared small value DTOs;
- avoid premature generic abstraction;
- retire unused models.

### Hidden Data Leakage

Mitigation:

- dedicated player-safe queries;
- Application visibility policies;
- adversarial tests;
- no UI-only hiding.

### Query Performance Regression

Mitigation:

- complex fixture;
- query plan review;
- explicit indexes;
- performance tests;
- no N+1.

### Stale UI State

Mitigation:

- versions;
- refresh after commands;
- invalidation hints;
- explicit degraded and stale states.

### Persistence Coupling

Mitigation:

- projection interfaces;
- typed DTOs;
- persistence code remains Infrastructure;
- integration tests protect SQL.

## 108. Technology Spike

Before acceptance, implement:

1. query records;
2. internal dispatcher;
3. query context;
4. Campaign library projection;
5. active play projection;
6. transcript cursor pagination;
7. Character Sheet read model;
8. Memory timeline;
9. Character Knowledge perspective;
10. Secret exclusion;
11. operation dashboard;
12. no-tracking EF Core projection;
13. one reviewed direct SQL query;
14. cancellation;
15. complex Campaign benchmark.

## 109. Spike Acceptance

The spike passes when:

- no query mutates authoritative state;
- player-safe models contain no hidden Secret data;
- transcript order is deterministic;
- cursor pagination remains stable after new Messages append;
- Campaign list does not load transcript or Character payloads;
- operation status reflects durable Work Items;
- query cancellation releases resources;
- no Domain entity reaches the ViewModel;
- complex fixture performance is acceptable;
- the same query can be tested without Avalonia.

## 110. Definition of Compliance

An implementation complies when:

- queries and handlers are explicit and typed;
- one Chronicle-owned dispatcher executes the query pipeline;
- results are immutable purpose-specific read models;
- no-tracking projections are the default;
- direct SQL is parameterized and reviewed;
- hidden information is absent from unauthorized results;
- all collections are bounded and ordered;
- long timelines use cursor pagination;
- queries have no side effects;
- caching is explicit rather than assumed;
- versions support UI reconciliation where needed;
- architecture and visibility tests enforce the boundaries.

## 111. Review Triggers

This ADR must be reviewed if:

- a network or mobile client is introduced;
- multiplayer requires server-side authorization;
- query latency exceeds local SQLite targets;
- asynchronous projections become necessary;
- a separate read database is introduced;
- plugin-defined read models are allowed;
- public API contracts are exposed;
- synchronization creates eventual consistency;
- large Campaigns exceed local projection limits.

## 112. Deferred Decisions

Later ADRs MAY define:

- exact query result-union implementation;
- cursor encoding;
- query timeout values;
- cache implementation;
- read-model invalidation bus;
- asynchronous projection framework;
- public API query contracts;
- server-side paging;
- distributed read database;
- query performance budgets.

## 113. Final Decision

Chronicle will use explicit typed queries, one primary handler per query, and a small internal dispatcher.

Query handlers will project immutable, purpose-specific, visibility-safe read models from local persistence.

They will use explicit ordering, bounded pagination, cancellation, and freshness metadata.

A query may simplify Chronicle's state for one screen or one perspective.

It may never broaden who is allowed to know it.
