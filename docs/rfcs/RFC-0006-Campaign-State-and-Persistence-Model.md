---
id: RFC-0006
title: Campaign State and Persistence Model
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-07-31
category: Architecture
depends_on:
  - RFC-0000
  - RFC-0001
  - RFC-0002
  - RFC-0003
  - RFC-0004
  - RFC-0005
---

> **"What happened may be narrated. What is true must be persisted."**

# Campaign State and Persistence Model

## Abstract

This RFC defines how Chronicle represents, protects, persists, restores, and evolves authoritative Campaign State.

It establishes the persistence model for the MVP, including aggregate boundaries, snapshots, historical records, optimistic concurrency, idempotency, transaction boundaries, recovery, archival, and the relationship between current state and completed history.

This document remains technology-neutral. It does not select a database engine, ORM, serialization library, or migration tool.

## 1. Purpose

Chronicle is designed for long-lived Campaigns.

A Campaign MUST survive:

- application restarts;
- provider failures;
- interrupted Sessions;
- unresolved Dice Rolls;
- repeated commands;
- partial retries;
- schema evolution;
- long periods without play.

The persistence architecture MUST preserve both:

```text
What is true now
```

and:

```text
What was lived before
```

These concerns are related but not identical.

## 2. Persistence Principles

Chronicle persistence follows these principles:

1. Campaign State is authoritative.
2. Completed history is preserved.
3. Provider output is never persisted as truth without validation.
4. Persistent operations are atomic where consistency requires it.
5. External calls do not hold database transactions open.
6. Commands that may be retried are idempotent.
7. Campaign writes are concurrency-protected.
8. Deletion is exceptional; archival is preferred.
9. The domain model is independent from storage shape.
10. Recovery is a product requirement, not an operational afterthought.

## 3. Persistence Strategy

The MVP SHOULD use a hybrid persistence strategy:

```text
Authoritative Current State
        +
Immutable or Append-Oriented History
```

### 3.1 Current State

Current State is stored in queryable persistent records representing the latest accepted values.

Examples:

- Campaign status;
- active Session;
- active Act;
- active Scene;
- Character Sheet;
- Character State;
- Relationship values;
- active Campaign Memories;
- pending Dice Roll;
- Narrative Plan version.

### 3.2 History

History preserves completed or append-oriented records.

Examples:

- completed Sessions;
- completed Acts;
- completed Scenes;
- Narrative Messages;
- resolved Dice Rolls;
- Session finalization results;
- Memory lifecycle changes;
- domain operation records.

### 3.3 Why Hybrid

A pure transcript model is insufficient because Chronicle requires explicit current truth.

A full event-sourcing implementation is not required for the MVP because it introduces substantial complexity in:

- event versioning;
- projection rebuilding;
- migration;
- debugging;
- contributor onboarding;
- vibe-coded delivery.

Chronicle SHOULD preserve enough historical information for auditability and recovery without requiring every state mutation to be reconstructed exclusively from an event stream.

## 4. Aggregate Persistence Boundary

`Campaign` is the domain aggregate root.

This does not require storing the entire Campaign as one serialized document.

The persistence model MAY normalize data into multiple records or documents.

The aggregate boundary means:

- all persistent gameplay data belongs to one Campaign;
- consistency rules are enforced through Campaign-scoped use cases;
- cross-Campaign references are forbidden in the MVP;
- operations that modify related Campaign state coordinate through the Application layer.

## 5. Canonical Persistent Record Groups

The persistence model SHOULD represent the following logical record groups.

```text
Campaigns
CampaignPreferences
CampaignStates
Characters
CharacterSheetFields
CharacterStates
NarrativeProfiles
Relationships
CampaignMemories
MemoryKnowledge
NarrativePlans
Sessions
Acts
Scenes
SceneParticipants
NarrativeMessages
NarrativeEvents
DiceRolls
SessionFinalizations
OperationRecords
RuleSetReferences
SchemaMigrations
```

These are logical record groups.

They do not prescribe exact table, collection, or file names.

## 6. Campaign Record

The Campaign record is the root persistence record.

It SHOULD include:

- Campaign identifier;
- title;
- status;
- active Rule Set identifier;
- active Rule Set version;
- creation timestamp;
- last update timestamp;
- completion timestamp;
- archival timestamp;
- concurrency version;
- active Session reference;
- soft-delete or archival metadata;
- ownership reference for future multi-user support.

### Invariants

- Campaign identifier is immutable.
- Rule Set identity is immutable during normal MVP play.
- Campaign version increments on authoritative writes.
- active Session reference MUST match Campaign State.
- archived Campaigns MUST remain readable.

## 7. Campaign State Record

The Campaign State record represents what Chronicle needs to continue play.

It SHOULD include:

- Campaign identifier;
- state version;
- active Session identifier;
- active Act identifier;
- active Scene identifier;
- current in-world time representation;
- current world conditions;
- active objectives;
- current location references;
- unresolved Dice Roll identifier;
- pending continuation operation identifier;
- current Narrative Plan version;
- last consistent checkpoint timestamp.

### Invariants

- At most one active Session exists.
- At most one active Act exists.
- At most one active Scene exists.
- An unresolved Dice Roll MUST reference the active Scene.
- A pending continuation MUST reference a resolved Dice Roll.
- every referenced entity belongs to the same Campaign.

## 8. Character Persistence

A Character SHOULD be persisted using distinct logical concerns.

```text
Character Identity
Character Sheet
Narrative Profile
Character State
Progression
Visibility
```

This separation supports Rule Set-specific Character Sheets without coupling all Character data to one schema.

### 8.1 Character Identity

Contains:

- Character identifier;
- Campaign identifier;
- Character role;
- display name;
- stable identity metadata;
- lifecycle status;
- visibility;
- creation source;
- creation timestamp.

### 8.2 Character Sheet

The MVP Character Sheet SHOULD be persisted as structured field-and-value data.

Each field SHOULD include:

- stable field key;
- display label;
- section;
- data type;
- value;
- Rule Set metadata;
- display order;
- validation metadata;
- version.

Field keys MUST remain stable across localization.

Labels MAY be localized.

### 8.3 Narrative Profile

Narrative Profile persistence MAY use structured records or a versioned structured document.

It SHOULD remain queryable enough to select relevant traits without loading unrelated data.

### 8.4 Character State

Character State SHOULD be stored separately from long-lived sheet definitions.

This supports frequent updates to:

- wounds;
- resources;
- location;
- temporary effects;
- emotional state;
- availability.

## 9. Relationship Persistence

Relationships are directional and Campaign-scoped.

A Relationship record SHOULD include:

- Relationship identifier;
- Campaign identifier;
- source Character identifier;
- target Character identifier;
- structured relationship dimensions;
- narrative notes;
- player visibility;
- last change Session;
- last change Scene;
- concurrency version;
- timestamps.

A unique constraint SHOULD prevent duplicate active relationships for the same ordered Character pair.

## 10. Campaign Memory Persistence

Campaign Memories require enough structure for:

- relevance selection;
- lifetime aging;
- scope filtering;
- Character knowledge;
- player review;
- archival;
- supersession.

A Campaign Memory SHOULD include:

- Memory identifier;
- Campaign identifier;
- summary;
- optional detailed description;
- scope;
- importance;
- relevance;
- lifetime type;
- remaining Sessions;
- age in Sessions;
- status;
- origin Session;
- origin Act;
- origin Scene;
- creation source;
- created timestamp;
- updated timestamp;
- superseded-by reference;
- concurrency version.

### 10.1 Memory Knowledge

Which Characters remember or know a Memory SHOULD be persisted separately.

```text
MemoryKnowledge
├── MemoryId
├── CharacterId
├── KnowledgeStatus
├── LearnedAtSessionId
├── Confidence
└── Visibility
```

This prevents all Characters from implicitly knowing all Campaign Memories.

### 10.2 Memory Aging

Memory aging MUST be deterministic.

At successful Session finalization:

- permanent Memories increment age;
- temporary Memories increment age;
- temporary Memories decrement remaining Sessions;
- relevance MAY be adjusted by policy;
- expired Memories transition to dormant or archived state;
- no Memory is aged twice for the same Session finalization.

## 11. Narrative Plan Persistence

The Narrative Plan is adaptable and versioned.

It SHOULD preserve:

- current plan version;
- plan creation source;
- Campaign premise;
- planned Acts;
- planned Scenes;
- NPC roles;
- hidden information;
- possible outcomes;
- pacing guidance;
- revision history.

The MVP MAY store the Narrative Plan as a versioned structured document if its internal shape changes frequently.

Completed history MUST NOT be rewritten when the plan changes.

A plan revision SHOULD reference:

- prior version;
- reason for change;
- Session that caused the revision;
- timestamp.

## 12. Session Persistence

A Session record SHOULD include:

- Session identifier;
- Campaign identifier;
- status;
- sequence number;
- started timestamp;
- ended timestamp;
- interrupted timestamp;
- finalization operation identifier;
- finalization status;
- summary;
- concurrency version.

The Session sequence number SHOULD be monotonic within one Campaign.

It provides a stable basis for:

- Memory age;
- lifetime decrement;
- history ordering;
- player display.

## 13. Act Persistence

An Act record SHOULD include:

- Act identifier;
- Session identifier;
- Campaign identifier;
- title;
- description;
- dramatic objective;
- order;
- status;
- started timestamp;
- ended timestamp;
- interruption reason;
- planned source reference;
- concurrency version.

An Act's order is scoped to its Session.

## 14. Scene Persistence

A Scene record SHOULD include:

- Scene identifier;
- Act identifier;
- Session identifier;
- Campaign identifier;
- title;
- description;
- location;
- immediate objective;
- active conflict;
- order;
- status;
- started timestamp;
- ended timestamp;
- interruption reason;
- context version;
- concurrency version.

### 14.1 Scene Context Version

A Scene SHOULD have a context version.

The context version increments when authoritative Scene inputs change, such as:

- participants;
- objective;
- local state;
- location;
- unresolved Roll;
- hidden-information permissions.

This helps prevent stale Narrator responses from being applied after the Scene has changed.

## 15. Scene Participant Persistence

Scene participants MUST be explicit.

A Scene Participant record SHOULD include:

- Scene identifier;
- Character identifier;
- participation role;
- visibility;
- local objective;
- entered timestamp;
- exited timestamp;
- local state;
- participant order.

A sibling Scene MUST have its own participant records.

Participant membership MUST NOT be inferred from Act membership.

## 16. Narrative Message Persistence

Narrative Messages SHOULD be append-only.

A Message record SHOULD include:

- Message identifier;
- Campaign identifier;
- Session identifier;
- Act identifier;
- Scene identifier;
- author;
- content;
- content format;
- operation identifier;
- sequence number;
- created timestamp;
- correction status;
- optional superseded-by reference.

Sequence numbers SHOULD be monotonic within a Session.

Messages MAY be corrected only through an explicit future workflow.

## 17. Narrative Event Persistence

Narrative Events MAY be persisted when needed for:

- orchestration;
- audit;
- replay;
- UI recovery;
- unresolved action state;
- debugging.

A Narrative Event record SHOULD include:

- Narrative Event identifier;
- Campaign identifier;
- Session identifier;
- Scene identifier;
- type;
- structured payload;
- contract version;
- status;
- source operation;
- validation result;
- created timestamp;
- processed timestamp;
- rejection reason.

Events with no persistence or recovery value MAY remain transient.

The exact retention policy will be defined later.

## 18. Dice Roll Persistence

Dice Rolls are permanent Campaign history.

A Dice Roll record SHOULD include:

- Dice Roll identifier;
- Campaign identifier;
- Session identifier;
- Act identifier;
- Scene identifier;
- acting Character identifier;
- status;
- request payload;
- Rule Set operation;
- random algorithm version;
- random source metadata;
- raw dice;
- resolved result;
- created timestamp;
- presented timestamp;
- executed timestamp;
- resolved timestamp;
- operation identifier;
- concurrency version.

### Invariants

- A resolved Roll is immutable.
- One pending Roll MUST NOT be executed twice.
- Raw dice MUST be preserved.
- Rule resolution version MUST be preserved.
- automatic retries MUST NOT regenerate persisted randomness.

## 19. Session Finalization Persistence

Session finalization is a first-class persisted workflow.

A Session Finalization record SHOULD include:

- finalization identifier;
- Campaign identifier;
- Session identifier;
- operation identifier;
- status;
- Archivist contract version;
- request snapshot reference;
- raw structured proposal;
- validation result;
- accepted changes;
- rejection details;
- started timestamp;
- completed timestamp;
- applied Campaign version.

### Status

```text
Requested
InProgress
Proposed
Validated
Applied
Failed
```

### Invariants

- One applied finalization exists per Session.
- repeated requests reuse the same logical operation.
- accepted changes are applied atomically.
- Memory aging is part of the same finalization transaction.
- Session status changes to `Completed` only after successful application.

## 20. Operation Record

Idempotent workflows require persistent operation tracking.

An Operation Record SHOULD include:

- operation identifier;
- operation type;
- Campaign identifier;
- optional Session identifier;
- optional Scene identifier;
- request fingerprint;
- status;
- result reference;
- created timestamp;
- updated timestamp;
- failure category;
- retry count.

### Operation Types

Examples:

```text
CreateCampaign
GenerateCampaign
ProcessPlayerInput
RequestNarration
ExecuteDiceRoll
ContinueAfterRoll
FinalizeSession
AdjustMemoryRelevance
```

### Invariants

- operation identifiers are unique;
- repeated equivalent requests return the existing result where safe;
- conflicting requests using the same identifier fail explicitly;
- operation records are not Campaign truth, but protect Campaign truth.

## 21. Snapshot Model

Chronicle SHOULD persist current state directly.

It MAY also create explicit Campaign snapshots at meaningful checkpoints.

Recommended checkpoints:

- Campaign creation completed;
- Session started;
- Dice Roll resolved;
- Scene completed;
- Session finalized;
- Campaign completed;
- before a destructive migration.

A snapshot MAY include:

- Campaign State;
- active Character State;
- active Relationships;
- active Memories;
- Narrative Plan version;
- active hierarchy;
- schema version.

Snapshots support recovery and diagnostics.

They MUST NOT become an alternative write path.

## 22. Checkpointing

A checkpoint represents a known internally consistent Campaign version.

The system SHOULD record the last consistent checkpoint after atomic operations.

If the application stops during an external call, the Campaign remains at the prior checkpoint until a validated result is committed.

This prevents half-applied narrative operations.

## 23. Transaction Boundaries

The following operations SHOULD be atomic.

### 23.1 Campaign Creation Commit

Persist:

- Campaign;
- Campaign Preferences;
- Player Character;
- generated NPCs;
- Narrative Plan;
- initial Campaign State;
- Rule Set reference.

### 23.2 Session Start

Persist:

- Session;
- first or active Act;
- first or active Scene;
- Campaign State update;
- Campaign status update.

### 23.3 Narration Commit

Persist:

- player Message;
- Narrator Message;
- validated Narrative Events;
- pending Dice Roll when requested;
- Scene state changes;
- operation result.

### 23.4 Dice Resolution Commit

Persist:

- raw dice;
- resolved result;
- Roll status;
- Campaign State continuation marker;
- operation result.

### 23.5 Post-Roll Continuation Commit

Persist:

- continuation Message;
- validated events;
- pending Roll or transition;
- Campaign State update.

### 23.6 Session Finalization Commit

Persist:

- accepted Memories;
- Memory updates;
- Memory aging;
- Relationship changes;
- Character progression;
- Character State changes;
- Session summary;
- Session completion;
- Campaign State update;
- finalization result.

## 24. External Calls and Transactions

Provider and retrieval calls MUST occur outside long-running persistence transactions.

Recommended sequence:

```text
Load State
    ↓
Validate Preconditions
    ↓
Persist Operation Intent when needed
    ↓
Call External Service
    ↓
Validate Structured Response
    ↓
Reload or Revalidate Version
    ↓
Commit Accepted Changes Atomically
```

If Campaign version changed while awaiting a provider response, Chronicle MUST reject, retry, or rebase the response explicitly.

It MUST NOT apply stale output silently.

## 25. Optimistic Concurrency

The MVP SHOULD use optimistic concurrency.

Campaign-scoped mutable records SHOULD include a version token.

A write command supplies the version it observed.

The persistence layer commits only if the version remains current.

On conflict:

- the operation fails explicitly;
- Chronicle reloads current state;
- automatic retry MAY occur if behavior remains safe;
- provider output MAY need regeneration when context changed.

### Stale Narrator Response

A Narrator response is stale when:

- Campaign version changed;
- active Scene changed;
- Scene context version changed;
- pending Dice Roll changed;
- participant set changed.

Stale responses MUST NOT alter state.

## 26. Idempotency Model

Idempotency applies at the application operation level.

A request SHOULD carry an operation identifier generated before execution.

Chronicle checks:

1. Has this operation completed?
2. Is this operation currently in progress?
3. Does the request fingerprint match?
4. Can the previous result be returned?
5. Is a recovery action required?

### Examples

#### Repeated Roll Click

The same `ExecuteDiceRoll` operation MUST return the already persisted Roll Result.

It MUST NOT roll again.

#### Repeated Session Finalization

The same finalization operation MUST return the already applied result.

It MUST NOT award progression twice.

#### Repeated Narrator Response Processing

The same provider response MUST NOT append duplicate Messages or Narrative Events.

## 27. Recovery Model

Chronicle MUST support recovery from interruption at known workflow points.

### 27.1 Interrupted Before External Call

No state change is committed beyond optional operation intent.

The workflow MAY resume or restart.

### 27.2 Interrupted During External Call

Campaign remains at prior consistent checkpoint.

The operation record remains pending or failed.

### 27.3 Interrupted After Provider Response but Before Commit

The response MAY be retained temporarily.

Chronicle MUST revalidate Campaign and Scene versions before applying it.

### 27.4 Interrupted After Commit but Before UI Confirmation

The client retries using the same operation identifier.

Chronicle returns the persisted result.

### 27.5 Interrupted with Pending Dice Roll

The Campaign restores:

- active Scene;
- interrupted narrative;
- pending Roll Request;
- Roll status.

The player sees the same Roll control after restart.

### 27.6 Interrupted During Finalization

The finalization record identifies the last completed stage.

Chronicle resumes safely without duplicating applied changes.

## 28. Resume Semantics

Resuming a Campaign MUST restore the last valid state, not reconstruct the Campaign by asking Narrative Intelligence what happened.

For an active or interrupted Session, resume SHOULD restore:

- current Session;
- current Act;
- current Scene;
- Scene participants;
- recent Messages;
- unresolved Roll;
- Character State;
- relevant Campaign Memories;
- current objectives;
- Narrative Plan version.

The resumed UI MUST reflect whether Chronicle is:

- ready for player input;
- waiting for a Dice Roll;
- waiting for continuation;
- finalizing the Session;
- recovering from failure.

## 29. Deletion and Archival

Chronicle SHOULD prefer archival.

### 29.1 Archive Campaign

Archiving:

- marks the Campaign unavailable for normal play;
- preserves all Campaign history;
- preserves export capability;
- preserves internal references.

### 29.2 Delete Campaign

Permanent deletion MAY be supported as an explicit user operation.

It MUST:

- require clear confirmation;
- delete or anonymize all Campaign-owned records;
- avoid leaving orphaned data;
- account for backups according to documented policy.

### 29.3 Entity Removal

Characters, Memories, Sessions, Acts, Scenes, Messages, and Dice Rolls SHOULD NOT be physically deleted during normal gameplay.

Use:

- retired;
- archived;
- superseded;
- cancelled;
- skipped;
- dormant.

## 30. Data Retention

The MVP is local-first.

By default, Campaign data SHOULD remain until the user archives or deletes it.

Temporary operational data MAY have shorter retention.

Examples:

- provider raw responses;
- transient prompt payloads;
- debug traces;
- retrieval diagnostics.

Retention of provider inputs and outputs MUST respect privacy and security requirements.

## 31. Export and Backup Boundary

Campaign export is an important future-facing persistence capability.

The MVP architecture SHOULD avoid preventing export.

A future Campaign package MAY include:

- Campaign metadata;
- Rule Set identity and version;
- Characters;
- Relationships;
- Memories;
- Narrative Plan;
- Sessions;
- Messages;
- Dice Rolls;
- assets;
- schema version;
- integrity manifest.

Export is not required by this RFC to be implemented immediately.

The storage model MUST avoid undocumented references that make export impossible.

## 32. Schema Versioning

Every persistent store MUST track schema version.

Migrations MUST be:

- ordered;
- repeatable only when explicitly safe;
- transactional where supported;
- testable against representative Campaign data;
- backward-aware;
- recoverable through backup or checkpoint.

Domain contract versions and storage schema versions are separate concepts.

## 33. Rule Set Versioning

A Campaign stores:

- Rule Set identifier;
- Rule Set version.

A Campaign MUST continue using the version under which its authoritative mechanics were created unless an explicit migration occurs.

Rule Set upgrades MAY require:

- Character Sheet migration;
- dice behavior migration;
- terminology migration;
- progression migration;
- Rule knowledge index update.

Automatic Rule Set upgrades are forbidden in the MVP.

## 34. Contract Versioning

Persisted structured provider responses SHOULD record their contract version when retained.

This applies to:

- Campaign generation;
- Narrator responses;
- Archivist proposals;
- Rule knowledge response metadata.

Old persisted contracts MUST remain interpretable or safely ignorable after upgrades.

## 35. Integrity Constraints

The persistence implementation MUST enforce integrity where supported.

Examples:

- Character belongs to Campaign;
- Relationship endpoints belong to same Campaign;
- Session belongs to Campaign;
- Act belongs to Session;
- Scene belongs to Act;
- Scene Participant Character belongs to Campaign;
- Dice Roll belongs to Scene;
- Memory origin references belong to same Campaign;
- active references point to nonarchived valid entities;
- unique Player Character per Campaign in MVP;
- unique applied finalization per Session;
- unique operation identifier.

Domain validation remains required even when storage constraints exist.

## 36. Persistence Ports

The Application layer SHOULD depend on explicit repository or store ports.

Possible ports include:

```text
CampaignRepository
CharacterRepository
MemoryRepository
SessionRepository
DiceRollRepository
OperationRepository
UnitOfWork
```

Repository granularity MUST avoid both extremes:

- one generic repository for every entity;
- one repository method for every storage operation.

The final repository design will be refined with the chosen technology.

## 37. Unit of Work

A `UnitOfWork` or equivalent transaction abstraction SHOULD coordinate atomic operations across modules.

It MAY provide:

- begin;
- commit;
- rollback;
- optimistic concurrency verification;
- operation record persistence;
- outbox staging if later required.

The Domain MUST remain unaware of the Unit of Work implementation.

## 38. Domain Events

Chronicle MAY use in-process domain events for coordination.

Examples:

```text
SessionStarted
SceneActivated
DiceRollResolved
SessionFinalized
MemoryExpired
CharacterRevealed
```

The MVP does not require full event sourcing.

Domain Events MAY trigger:

- read model updates;
- logging;
- local notifications;
- noncritical follow-up behavior.

Critical consistency changes MUST remain within the originating transaction or explicit workflow.

## 39. Outbox Pattern

An outbox is not mandatory for the local modular monolith MVP.

The architecture MAY introduce an outbox if Chronicle later requires reliable asynchronous side effects.

Examples:

- remote synchronization;
- streaming events;
- external notifications;
- asset generation jobs.

An outbox MUST NOT be implemented speculatively.

## 40. Read Models

Read models MAY be denormalized for UI efficiency.

Recommended read models include:

```text
CampaignListItem
CampaignDashboardView
CharacterSheetView
ActiveSessionView
SceneNarrativeView
CampaignMemoryListItem
SessionHistoryItem
PendingDiceRollView
```

Read models are projections.

They MUST NOT become authoritative write models.

## 41. Query Consistency

For local MVP operations, read-after-write behavior SHOULD be strongly consistent from the user's perspective.

After:

- saving a Character;
- resolving a Dice Roll;
- finalizing a Session;
- adjusting Memory relevance;

the next UI view SHOULD reflect the committed result immediately.

Eventual consistency MAY be introduced later for noncritical projections.

## 42. Prompt and Provider Data Persistence

Chronicle MUST minimize persistence of raw prompts and provider responses.

The system MAY persist:

- operation metadata;
- contract version;
- token usage;
- latency;
- response validation status;
- provider request identifier;
- redacted diagnostic content.

Full prompt persistence SHOULD be opt-in for development or debugging.

Hidden Campaign data and secrets MUST NOT be logged casually.

## 43. Randomness Persistence

The persistence model MUST retain enough information to audit Dice Rolls.

At minimum:

- Rule Set operation;
- requested dice pool;
- difficulty and modifiers;
- raw generated values;
- interpreted result;
- algorithm version;
- timestamp.

The MVP does not require cryptographic verifiability.

The implementation SHOULD use a quality random source appropriate for game mechanics.

## 44. Local-First Storage Requirements

The MVP persistent store SHOULD support:

- operation without internet;
- transactional writes;
- schema migrations;
- efficient Campaign loading;
- indexing of Memories by relevance and status;
- ordered Session history;
- backup through ordinary local file practices where possible;
- safe application shutdown;
- corruption detection or recovery strategy.

The exact technology remains open.

## 45. Performance Expectations

The persistence model SHOULD optimize for:

- one active local player;
- dozens of Campaigns;
- hundreds of Sessions per long Campaign;
- thousands of Messages;
- hundreds or thousands of Memories;
- many persistent NPCs;
- fast active Scene loading.

Chronicle SHOULD NOT load the complete Campaign history for every turn.

The active operation SHOULD load only required state.

## 46. Suggested Indexes

The eventual storage implementation SHOULD consider indexes for:

- Campaign status and last update;
- Character by Campaign and role;
- Relationship by source and target;
- Memory by Campaign, status, relevance, and scope;
- Memory knowledge by Character;
- Session by Campaign and sequence;
- Act by Session and order;
- Scene by Act and order;
- active Scene by Campaign;
- Messages by Session and sequence;
- Dice Roll by Scene and status;
- Operation Record by operation identifier;
- finalization by Session.

These are logical recommendations, not database-specific commands.

## 47. Privacy Boundary

Campaign persistence may contain sensitive creative content.

Chronicle MUST:

- keep local data local by default in the MVP;
- send only required data to providers;
- avoid including unrelated Campaign data in prompts;
- keep credentials outside Campaign records;
- document what provider-bound data leaves the device;
- support future deletion and export workflows.

## 48. Failure Classification

Persistence failures SHOULD be categorized.

```text
ValidationFailure
ConcurrencyConflict
ConstraintViolation
MigrationFailure
CorruptionDetected
StorageUnavailable
TransactionFailure
IdempotencyConflict
RecoveryRequired
```

The Application layer maps these to safe product responses.

## 49. Testing Requirements

Persistence tests MUST cover:

- Campaign creation transaction;
- active hierarchy consistency;
- Scene participant isolation;
- optimistic concurrency conflict;
- duplicate operation handling;
- repeated Dice Roll execution;
- repeated Session finalization;
- interrupted Session recovery;
- pending Roll recovery;
- Memory aging exactly once;
- archival behavior;
- migration against existing data;
- Rule Set version preservation;
- stale Narrator response rejection.

## 50. Prohibited Persistence Patterns

The MVP MUST NOT use:

### 50.1 Transcript as Database

Campaign truth MUST NOT exist only inside Message text.

### 50.2 Provider Conversation as Storage

Provider threads or assistant memory MUST NOT be required to resume a Campaign.

### 50.3 Unversioned Structured Blobs

Persistent structured provider output MUST NOT be stored without contract version when future interpretation may be required.

### 50.4 Direct UI Persistence

The UI MUST NOT write storage records directly.

### 50.5 Silent Last-Write-Wins

Concurrency conflicts MUST NOT silently overwrite authoritative state.

### 50.6 Re-Roll on Retry

A retried Dice Roll operation MUST NOT generate new randomness after a result has been persisted.

### 50.7 Destructive Aging

Memory expiration MUST NOT physically delete historical meaning by default.

## 51. Current Delivery Decision

For the MVP, Chronicle adopts:

- a modular monolith;
- one local persistent store;
- explicit current-state records;
- append-oriented Session history;
- versioned Narrative Plan;
- optimistic concurrency;
- operation-level idempotency;
- atomic Session finalization;
- no full event sourcing;
- no distributed transactions;
- no remote synchronization;
- no speculative outbox.

## 52. Architecture Horizon

Future persistence evolution MAY include:

- cloud synchronization;
- multiplayer concurrency;
- remote Campaign hosting;
- event streaming;
- full audit event streams;
- cross-device resume;
- Campaign sharing;
- encrypted backups;
- community-hosted servers.

The MVP persistence model SHOULD avoid blocking these capabilities.

It MUST NOT implement them now.

## 53. Open Questions

The following remain open:

- Which local database technology will be selected?
- Will the MVP use an ORM, query mapper, or direct storage API?
- Should Campaign State be stored in one record or multiple module-owned records?
- Which parts of Narrative Plan should be relational versus document-shaped?
- How long should raw provider responses be retained?
- Should explicit Campaign snapshots be stored after every Session or only major checkpoints?
- How should local backups be surfaced to the user?
- Should the official application support Campaign export in the MVP or immediately after it?
- What numeric range and precision should Memory relevance and importance use?
- How should in-world time be persisted?
- How should structured custom Character Sheet values be typed?
- Should domain events be persisted for audit?
- How should storage corruption be detected and repaired?

These decisions require dedicated ADRs after the technology stack is selected.

## 54. Compliance Checklist

A persistence implementation complies when:

- Campaign truth is explicit;
- current state is resumable;
- completed history is preserved;
- Scene participants are isolated;
- Dice Rolls are immutable after resolution;
- operations are idempotent;
- finalization cannot duplicate progression;
- Campaign writes are concurrency-protected;
- stale provider responses are rejected;
- migrations are versioned;
- Rule Set versions are preserved;
- archival is preferred over destructive deletion;
- external calls do not hold long transactions;
- recovery paths are tested.

## 55. Final Principle

Chronicle must never ask the Narrator what the Campaign currently is.

Chronicle already knows.

That knowledge is what persistence exists to protect.
