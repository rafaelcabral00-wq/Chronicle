---
id: RFC-0016
title: Application Use Cases
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-07-31
category: Application
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
---

> **"Use cases are where Chronicle turns architectural intent into controlled action."**

# Application Use Cases

## Abstract

This RFC defines the initial application use cases of Chronicle.

It establishes the commands, queries, preconditions, orchestration boundaries, expected results, transaction expectations, idempotency rules, failure behavior, and participating modules for the MVP.

The purpose of this document is to define what the official application may ask Chronicle to do without coupling those actions to a specific user interface, transport protocol, framework, or persistence technology.

## 1. Purpose

The domain defines what Chronicle is.

Application use cases define what Chronicle can do.

Each use case MUST:

- express one clear user or system intent;
- validate preconditions;
- coordinate domain behavior;
- invoke external ports only when required;
- preserve ownership boundaries;
- produce an explicit result;
- fail safely;
- avoid duplicating work on retry;
- remain independent from presentation technology.

## 2. Scope

This RFC defines MVP use cases for:

- Campaign creation;
- Campaign generation;
- Campaign listing and loading;
- Character creation and editing;
- Session start and resume;
- player input processing;
- Dice Roll execution;
- post-roll continuation;
- Scene and Act transitions;
- Session finalization;
- Campaign Memory review and relevance adjustment;
- Campaign pause, completion, archive, and delete;
- Rule Set discovery;
- read-model queries;
- recovery.

This RFC does not define:

- exact API routes;
- exact method signatures;
- exact DTO serialization;
- exact UI screens;
- database queries;
- provider-specific APIs;
- multiplayer commands;
- administrative back-office tooling.

## 3. Application Boundary

The Application layer coordinates use cases.

It MAY depend on:

- Domain entities;
- Domain services;
- Chronicle Director;
- Rule Set ports;
- Narrator port;
- Archivist port;
- repositories;
- transaction abstraction;
- clock;
- random source;
- operation store;
- read model providers.

It MUST NOT depend on:

- concrete UI widgets;
- concrete database types;
- provider SDK response classes;
- operating-system-specific presentation details.

## 4. Command and Query Separation

Chronicle distinguishes:

```text
Command
    = requests a state change

Query
    = requests information without changing authoritative state
```

A command MAY return a result.

A query MUST NOT alter Campaign truth.

The MVP does not require full CQRS infrastructure.

The conceptual separation MUST remain explicit.

## 5. Use Case Contract

Each use case SHOULD define:

- name;
- intent;
- command or query;
- actor;
- inputs;
- preconditions;
- orchestration;
- state changes;
- output;
- idempotency;
- transaction boundary;
- failure categories;
- authorization boundary for future use.

## 6. Common Command Envelope

Commands that may be retried SHOULD use a common envelope.

```text
CommandEnvelope
├── OperationId
├── ExpectedVersion
├── RequestedAt
├── CorrelationId
├── Locale
└── Payload
```

### 6.1 OperationId

Uniquely identifies the logical operation.

### 6.2 ExpectedVersion

Identifies the state version observed by the caller when concurrency protection is required.

### 6.3 CorrelationId

Groups related operations for diagnostics.

### 6.4 Locale

Indicates presentation language where generated or localized output requires it.

## 7. Common Result Model

Use cases SHOULD return one of:

```text
Success
ValidationFailure
Conflict
NotFound
InvalidState
ExternalDependencyFailure
PersistenceFailure
RecoveryRequired
Cancelled
```

A successful result SHOULD include:

- operation identifier;
- resulting entity or read-model reference;
- resulting version;
- warnings;
- safe user-facing information.

## 8. Common Failure Rules

Use cases MUST:

- fail explicitly;
- avoid partial success unless documented;
- preserve prior consistent state;
- distinguish retryable from nonretryable failure;
- avoid exposing hidden Campaign information;
- record enough diagnostics for recovery.

## 9. Common Idempotency Rules

A retriable command MUST:

- receive an operation identifier;
- persist operation status where needed;
- detect repeated equivalent requests;
- return an existing result where safe;
- reject reuse with conflicting input;
- avoid duplicate state transitions.

## 10. Common Concurrency Rules

Commands modifying Campaign-owned state SHOULD validate:

- expected Campaign version;
- relevant entity version;
- active hierarchy version;
- operation status;
- Rule Set version where required.

Stale commands MUST fail explicitly.

## 11. Campaign Use Cases

The MVP defines:

```text
CreateCampaignDraft
GenerateCampaign
GetCampaignGenerationStatus
ListCampaigns
GetCampaign
GetCampaignDashboard
PauseCampaign
CompleteCampaign
ArchiveCampaign
DeleteCampaign
```

## 12. UC-001 — Create Campaign Draft

### Intent

Create a nonplayable Campaign container before Character and Campaign generation are complete.

### Actor

Player.

### Input

- Rule Set identifier;
- Campaign title when manually supplied;
- Campaign Preferences;
- locale;
- operation identifier.

### Preconditions

- Rule Set is registered and compatible;
- Campaign Preferences are structurally valid;
- no conflicting operation exists.

### Orchestration

1. validate Rule Set;
2. validate preferences;
3. create Campaign in `Draft`;
4. persist Rule Set identity and version;
5. persist preferences;
6. create initial Campaign State;
7. commit atomically.

### Output

- Campaign identifier;
- Campaign status;
- Rule Set summary;
- preferences summary;
- Campaign version.

### Idempotency

Repeated requests return the existing draft.

### Transaction

Single atomic transaction.

## 13. UC-002 — Create Player Character

### Intent

Create the Campaign's Player Character.

### Actor

Player.

### Input

- Campaign identifier;
- Character identity;
- Character Sheet values;
- Narrative Profile;
- operation identifier;
- expected Campaign version.

### Preconditions

- Campaign is `Draft`;
- no Player Character exists;
- Rule Set is available;
- field structure is valid.

### Orchestration

1. load Campaign;
2. validate structural data;
3. invoke Rule Set Character validation;
4. return errors or warnings;
5. create Character with `Player` role;
6. persist Character and updated Campaign state;
7. commit atomically.

### Output

- Character identifier;
- validation result;
- Character summary;
- resulting versions.

### Idempotency

The same operation MUST NOT create a second Player Character.

## 14. UC-003 — Update Draft Player Character

### Intent

Edit the Player Character before Campaign readiness.

### Actor

Player.

### Input

- Campaign identifier;
- Character identifier;
- typed field changes;
- Narrative Profile changes;
- validation mode;
- operation identifier;
- expected Character version.

### Preconditions

- Campaign is `Draft`;
- Character is the Player Character;
- Character is not archived.

### Orchestration

1. validate changed fields;
2. invoke Rule Set validation;
3. apply approved Character operations;
4. persist revisions.

### Output

- updated Character view;
- errors and warnings;
- resulting version.

## 15. UC-004 — Generate Campaign

### Intent

Generate and persist the initial Campaign proposal.

### Actor

Player through the official application.

### Input

- Campaign identifier;
- Player Character identifier;
- Campaign Preferences;
- generation constraints;
- operation identifier;
- expected Campaign version.

### Preconditions

- Campaign is `Draft`;
- Player Character is valid;
- Rule Set is available;
- no accepted generation already exists.

### Orchestration

1. load Campaign and Character;
2. retrieve relevant Rule Set generation guidance;
3. construct generation context;
4. persist operation state;
5. invoke Campaign Generator;
6. validate structured proposal;
7. validate NPCs and Relationships;
8. validate Narrative Plan;
9. create Chronicle-owned identifiers;
10. persist NPCs, Secrets, Relationships, Plan, and public metadata;
11. transition Campaign to `Ready`;
12. commit atomically.

### Output

- Campaign public summary;
- generation status;
- generated Character count;
- resulting Campaign version;
- warnings.

### Idempotency

Repeated execution returns the accepted Campaign.

### External Calls

Outside long-running transaction.

## 16. UC-005 — Get Campaign Generation Status

### Intent

Read current generation progress.

### Actor

Player or UI polling workflow.

### Type

Query.

### Output

Possible statuses:

```text
NotStarted
Requested
Generating
Validating
Accepted
Failed
Cancelled
```

The query MUST NOT expose hidden generated content before acceptance.

## 17. UC-006 — List Campaigns

### Intent

List Campaigns available to the local user.

### Type

Query.

### Output

Each item SHOULD include:

- Campaign identifier;
- public title;
- Rule Set;
- status;
- Player Character name;
- last played timestamp;
- active or pending workflow indicator;
- archive state.

Hidden plan data MUST NOT appear.

## 18. UC-007 — Get Campaign

### Intent

Load Campaign public and application-safe details.

### Type

Query.

### Input

- Campaign identifier.

### Output

- metadata;
- Player Character summary;
- status;
- Rule Set;
- active Session state;
- visible objectives;
- visible Memories;
- recovery status.

## 19. UC-008 — Get Campaign Dashboard

### Intent

Provide the official application with a consolidated Campaign overview.

### Type

Query.

### Output MAY include

- Player Character summary;
- current Campaign status;
- last Session summary;
- active objective;
- visible NPC summaries;
- recent visible Memories;
- progress indicators;
- resume or start action;
- pending recovery action.

The dashboard is a read model.

It MUST NOT become a write path.

## 20. Character Use Cases

The MVP defines:

```text
GetPlayerCharacter
GetCharacterSheet
UpdateCharacterBetweenSessions
GetVisibleCharacter
ListVisibleCharacters
GetCharacterRelationships
GetCharacterKnowledge
```

## 21. UC-009 — Get Player Character

### Type

Query.

### Output

A Player Character read model containing only player-visible information.

## 22. UC-010 — Get Character Sheet

### Type

Query.

### Output

- Rule Set schema;
- current field values;
- derived visible values;
- validation state;
- edit permissions;
- Character revision.

## 23. UC-011 — Update Character Between Sessions

### Intent

Allow approved Character edits when no Session is active.

### Preconditions

- Campaign is not active;
- Character is editable;
- update does not bypass progression rules;
- Rule Set allows the changed fields.

### Orchestration

1. load Character;
2. validate edit permission per field;
3. validate through Rule Set;
4. apply accepted changes;
5. persist atomically.

### Notes

This use case MUST NOT act as an unrestricted Character override.

## 24. UC-012 — Get Visible Character

### Type

Query.

### Input

- Campaign identifier;
- Character identifier.

### Output

Player-visible Character details only.

Hidden fields, private Relationships, Secrets, and plan roles MUST be filtered.

## 25. UC-013 — List Visible Characters

### Type

Query.

### Output

Only Characters whose visibility permits player display.

Hidden NPC existence MUST NOT leak through counts or metadata.

## 26. UC-014 — Get Character Relationships

### Type

Query.

### Output

Player-visible Relationship interpretations.

Raw hidden dimensions MAY remain internal.

## 27. UC-015 — Get Character Knowledge

### Type

Query.

### Output

Knowledge, beliefs, suspicions, and disproven information visible to the player.

Certainty MUST be explicit.

## 28. Session Use Cases

The MVP defines:

```text
StartSession
ResumeSession
GetActiveSession
ProcessPlayerInput
InterruptSession
RequestSessionFinalization
GetSessionFinalizationStatus
GetSessionHistory
GetSessionDetails
```

## 29. UC-016 — Start Session

### Intent

Start a new playable Session for a Ready or Paused Campaign.

### Actor

Player.

### Preconditions

- Campaign can play;
- no active Session exists;
- no unresolved finalization blocks play;
- Player Character is valid;
- Rule Set is available.

### Orchestration

1. load Campaign;
2. determine next Act and Scene;
3. validate planned or dynamic Scene;
4. create Session, Act, and Scene;
5. activate hierarchy;
6. request opening narration through Chronicle Director;
7. validate response;
8. persist active hierarchy and opening turn atomically.

### Output

- Active Session view;
- opening narrative;
- pending Roll when immediately requested;
- versions.

### Idempotency

Repeated start returns the already created Session.

## 30. UC-017 — Resume Session

### Intent

Restore an interrupted Session.

### Preconditions

- Session is resumable;
- Campaign State is consistent.

### Orchestration

1. load active hierarchy;
2. determine resume mode;
3. reconstruct read model;
4. do not invoke Narrator unless a bounded continuation operation requires it.

### Output Modes

```text
ReadyForInput
AwaitingRoll
AwaitingContinuation
Finalizing
RecoveryRequired
```

## 31. UC-018 — Get Active Session

### Type

Query.

### Output

- active Session;
- active Act;
- active Scene;
- visible participants;
- recent Messages;
- current input state;
- pending Roll;
- safe recovery action.

## 32. UC-019 — Process Player Input

### Intent

Advance the active Scene from player input.

### Actor

Player.

### Input

- Campaign identifier;
- Session identifier;
- player text or structured action;
- operation identifier;
- expected Scene context version.

### Preconditions

- Session is active;
- Scene is active;
- no Roll is pending;
- no finalization is running.

### Orchestration

Delegates bounded turn coordination to the Chronicle Director:

1. validate active hierarchy;
2. persist or stage operation intent;
3. select context;
4. retrieve relevant rules;
5. invoke Narrator;
6. validate response;
7. classify immediate and deferred changes;
8. persist player Message, Narrator Message, and accepted events;
9. return narration or pending Roll.

### Output

```text
NarrativeProduced
RollRequired
SceneTransitioned
PlanRevisionRequired
RecoverableFailure
```

### Idempotency

A repeated operation MUST NOT append duplicate Messages.

## 33. UC-020 — Interrupt Session

### Intent

Pause an active Session without finalizing it.

### Preconditions

- Session is active or awaiting Roll;
- no atomic state transition is in progress.

### Orchestration

- preserve pending Roll or continuation;
- transition Session to `Interrupted`;
- persist last consistent checkpoint;
- preserve active Act and Scene.

### Output

- interruption confirmation;
- resumable state.

## 34. UC-021 — Request Session Finalization

### Intent

Finalize the active or interrupted Session.

### Preconditions

- Session is eligible;
- no unresolved Roll remains;
- no applied finalization exists.

### Orchestration

Delegates to RFC-0013 workflow:

1. create finalization operation;
2. transition Session to `Finalizing`;
3. assemble evidence;
4. invoke Archivist;
5. validate proposal;
6. construct Change Set;
7. apply atomically;
8. complete Session;
9. pause Campaign or prepare next-session state.

### Output

- finalization result;
- Session summary;
- visible Memories;
- progression;
- visible changes;
- resulting Campaign version.

## 35. UC-022 — Get Session Finalization Status

### Type

Query.

### Output

- current workflow state;
- retry availability;
- visible progress;
- applied result when complete;
- safe error state.

## 36. UC-023 — Get Session History

### Type

Query.

### Output

Ordered completed and interrupted Sessions with:

- sequence;
- start and end timestamps;
- status;
- summary;
- Act count;
- visible major Memories;
- recovery indicator.

## 37. UC-024 — Get Session Details

### Type

Query.

### Output MAY include

- Session summary;
- Acts;
- Scenes;
- Messages;
- visible Dice Rolls;
- visible Memories;
- visible consequences.

Hidden Campaign truth MUST be filtered.

## 38. Dice Use Cases

The MVP defines:

```text
GetPendingDiceRoll
ExecuteDiceRoll
ContinueAfterDiceRoll
GetDiceRollHistory
```

## 39. UC-025 — Get Pending Dice Roll

### Type

Query.

### Output

- Roll identifier;
- reason;
- acting Character;
- visible pool information;
- visible modifiers;
- stakes;
- status;
- Roll button availability.

## 40. UC-026 — Execute Dice Roll

### Intent

Execute the active pending Roll.

### Actor

Player.

### Preconditions

- Roll exists;
- Roll belongs to active Scene;
- Roll is presented;
- Scene context remains valid;
- operation is not already resolved.

### Orchestration

1. load and lock through optimistic version;
2. validate Rule Set operation;
3. calculate authoritative pool;
4. generate randomness;
5. resolve through Rule Set;
6. apply required immediate mechanical consequences;
7. persist raw dice and result;
8. mark continuation pending;
9. commit atomically.

### Output

- visible Roll Result;
- continuation status;
- resulting versions.

### Idempotency

Repeated execution returns the same persisted result.

## 41. UC-027 — Continue After Dice Roll

### Intent

Continue narration from a resolved Roll.

### Preconditions

- Roll is resolved;
- continuation not already accepted;
- active Scene is still valid.

### Orchestration

1. load Roll and interruption context;
2. assemble post-roll context;
3. invoke Narrator;
4. validate continuation;
5. apply accepted events and transitions;
6. restore Scene and Session state;
7. persist atomically.

### Output

- continued narrative;
- next pending Roll when valid;
- Scene transition result;
- recoverable failure.

### Idempotency

Retry MUST NOT duplicate continuation Messages.

## 42. UC-028 — Get Dice Roll History

### Type

Query.

### Output

Ordered player-visible Roll history with:

- reason;
- pool;
- raw dice;
- result;
- Scene;
- timestamp.

## 43. Narrative Structure Use Cases

The MVP defines:

```text
GetActiveAct
GetActiveScene
CompleteScene
PrepareNextScene
CompleteAct
RequestPlanRevision
```

Most commands are normally initiated by Chronicle Director orchestration rather than direct UI actions.

## 44. UC-029 — Get Active Act

### Type

Query.

### Output

- title;
- visible objective;
- status;
- visible progress;
- active Scene summary.

Hidden Plan content remains excluded.

## 45. UC-030 — Get Active Scene

### Type

Query.

### Output

- title;
- location;
- visible objective;
- participants;
- current state;
- recent narrative;
- pending action.

## 46. UC-031 — Complete Scene

### Intent

Apply a validated Scene completion.

### Actor

Chronicle Director or controlled application workflow.

### Preconditions

- Scene is active or interrupted;
- no unresolved Roll;
- completion proposal is valid;
- next hierarchy state is known.

### Orchestration

1. close Scene state;
2. persist exits and immediate consequences;
3. activate next Scene, complete Act, or leave Session at boundary;
4. update Campaign State atomically.

### Output

- completed Scene reference;
- next active hierarchy.

## 47. UC-032 — Prepare Next Scene

### Intent

Prepare and activate the next valid Scene.

### Orchestration

1. select planned or dynamic Scene;
2. validate participants;
3. validate objective and location;
4. create executed Scene if needed;
5. increment context version;
6. activate atomically.

## 48. UC-033 — Complete Act

### Intent

Complete or interrupt an Act.

### Preconditions

- no active unresolved Scene state;
- Act objective resolution is valid.

### Output

- completed Act;
- next Act or Session boundary.

## 49. UC-034 — Request Plan Revision

### Intent

Request a versioned Narrative Plan update after Campaign divergence.

### Actor

Chronicle Director or finalization workflow.

### Orchestration

1. capture reason and evidence;
2. load current Plan version;
3. invoke plan revision capability;
4. validate proposal;
5. persist new version;
6. preserve completed history.

### Idempotency

The same divergence event MUST NOT create duplicate revisions.

## 50. Campaign Memory Use Cases

The MVP defines:

```text
ListCampaignMemories
GetCampaignMemory
AdjustMemoryRelevance
GetMemoriesByCharacter
GetMemoriesBySession
```

## 51. UC-035 — List Campaign Memories

### Type

Query.

### Input MAY include

- status;
- scope;
- Character;
- Session;
- minimum relevance;
- permanent-only;
- search term.

### Output

Player-visible Memories only.

## 52. UC-036 — Get Campaign Memory

### Type

Query.

### Output

- summary;
- visible description;
- origin;
- visible involved Characters;
- age;
- lifetime type;
- relevance presentation;
- status;
- source Session navigation.

## 53. UC-037 — Adjust Memory Relevance

### Intent

Allow the player to emphasize or de-emphasize a visible Memory.

### Preconditions

- Memory is visible;
- Memory is not rejected;
- requested value is allowed;
- expected Memory version matches.

### Orchestration

1. load Memory;
2. validate adjustment policy;
3. preserve prior value;
4. apply player-curated relevance;
5. persist source and operation.

### Rules

- historical truth remains unchanged;
- importance remains unchanged unless a separate workflow exists;
- lifetime remains unchanged by default.

## 54. UC-038 — Get Memories by Character

### Type

Query.

Returns visible Memories scoped to or involving a Character.

## 55. UC-039 — Get Memories by Session

### Type

Query.

Returns visible Memories originating from a Session.

## 56. Relationship and Knowledge Use Cases

The MVP defines:

```text
GetVisibleRelationships
GetKnownFacts
GetSuspicions
GetDisprovenBeliefs
```

These are read-only in normal player workflows.

Durable updates occur through play and finalization.

## 57. UC-040 — Get Visible Relationships

### Type

Query.

Returns player-visible Relationship interpretations.

It MUST preserve source and target direction internally.

## 58. UC-041 — Get Known Facts

### Type

Query.

Returns player-visible Character Knowledge classified as known.

## 59. UC-042 — Get Suspicions

### Type

Query.

Returns visible suspicions and uncertainty.

## 60. UC-043 — Get Disproven Beliefs

### Type

Query.

Returns visible beliefs that were explicitly disproven.

## 61. Rule Set Use Cases

The MVP defines:

```text
ListRuleSets
GetRuleSetDetails
GetCharacterSheetSchema
ValidateCharacterSheet
CheckCampaignCompatibility
```

## 62. UC-044 — List Rule Sets

### Type

Query.

### Output

Registered and selectable Rule Sets with:

- identity;
- display name;
- version;
- edition;
- support status;
- languages;
- short description.

## 63. UC-045 — Get Rule Set Details

### Type

Query.

Returns safe metadata and declared capabilities.

It MUST NOT expose restricted source content.

## 64. UC-046 — Get Character Sheet Schema

### Type

Query.

Returns the selected Rule Set's player-facing Character Sheet schema.

## 65. UC-047 — Validate Character Sheet

### Intent

Validate a draft Character Sheet without necessarily persisting it.

### Type

Query-like application operation.

Although no Campaign truth changes, it may invoke deterministic Rule Set behavior.

### Output

- errors;
- warnings;
- normalized values;
- override permissions.

## 66. UC-048 — Check Campaign Compatibility

### Type

Query.

Determines whether the Campaign's stored Rule Set version and schemas are available and playable.

## 67. Campaign Lifecycle Use Cases

## 68. UC-049 — Pause Campaign

### Intent

Transition an inactive Campaign to `Paused`.

An active Session MUST be interrupted first.

## 69. UC-050 — Complete Campaign

### Intent

Mark the Campaign narratively complete.

### Preconditions

- no active Session;
- final Session is completed;
- completion is explicit;
- no critical finalization remains pending.

### Output

- completion timestamp;
- final Campaign summary reference;
- resulting status.

## 70. UC-051 — Archive Campaign

### Intent

Remove the Campaign from normal play while preserving data.

### Preconditions

- no unsafe active operation;
- active Session is interrupted or completed.

### Output

Archived Campaign reference.

## 71. UC-052 — Delete Campaign

### Intent

Permanently delete Campaign-owned data.

### Requirements

- explicit confirmation;
- deletion policy validation;
- no accidental operation retry;
- orphan prevention;
- result audit metadata where appropriate.

This is one of the few intentionally destructive use cases.

## 72. Recovery Use Cases

The MVP defines:

```text
GetRecoveryStatus
RetryOperation
CancelRecoverableOperation
RestoreLastCheckpoint
```

## 73. UC-053 — Get Recovery Status

### Type

Query.

### Output

- pending operation;
- last consistent checkpoint;
- retry availability;
- cancellation availability;
- user action required;
- safe diagnostic category.

## 74. UC-054 — Retry Operation

### Intent

Retry a recoverable workflow using the same operation identifier.

### Rules

- original request fingerprint is reused;
- committed work is not duplicated;
- stale external responses are revalidated;
- Dice Rolls are not regenerated after commit.

## 75. UC-055 — Cancel Recoverable Operation

### Intent

Cancel an incomplete operation before accepted changes are applied.

Examples:

- Campaign generation;
- finalization before application;
- invalid pending narration request.

Cancellation MUST preserve committed Campaign state.

## 76. UC-056 — Restore Last Checkpoint

### Intent

Recover the Campaign to its last known internally consistent checkpoint.

### Restrictions

This use case MUST NOT silently discard committed history.

It is primarily an operational recovery workflow.

The exact MVP exposure remains open.

## 77. Query Read Model Principles

Queries SHOULD return purpose-built read models.

They SHOULD NOT return mutable Domain entities directly.

Read models MAY be:

- denormalized;
- localized;
- filtered;
- presentation-oriented;
- cached.

They MUST remain derived from committed state.

## 78. Hidden Information Filtering

Every player-facing query MUST apply visibility filtering.

Filtering MUST account for:

- hidden Characters;
- private Relationships;
- Character Knowledge;
- Secrets;
- Narrative Plan;
- hidden Memories;
- hidden Dice Rolls where later supported;
- internal operation metadata.

## 79. Localization Boundary

Use cases MAY accept locale.

Domain state remains language-neutral where possible.

Localization applies to:

- labels;
- validation messages;
- summaries;
- Rule Set terminology;
- read models.

Stable identifiers MUST not be localized.

## 80. Transaction Classification

Use cases SHOULD be classified as:

```text
ReadOnly
SingleAggregateWrite
MultiModuleAtomicWrite
ExternalCallThenWrite
DestructiveWrite
```

Examples:

| Use Case | Classification |
|---|---|
| ListCampaigns | ReadOnly |
| AdjustMemoryRelevance | SingleAggregateWrite |
| ExecuteDiceRoll | MultiModuleAtomicWrite |
| ProcessPlayerInput | ExternalCallThenWrite |
| DeleteCampaign | DestructiveWrite |

## 81. Long-Running Operations

The following MAY be long-running:

- Campaign generation;
- Narrator invocation;
- Session finalization;
- plan revision;
- provider response repair.

Long-running operations SHOULD expose status queries.

They MUST NOT hold long database transactions.

## 82. Authorization Horizon

The MVP is local single-user.

Application contracts SHOULD still preserve an actor or principal boundary where inexpensive.

Future authorization MAY govern:

- Campaign ownership;
- player role;
- contributor role;
- spectator access;
- hidden information;
- destructive operations.

The MVP MUST NOT build a full authorization system without need.

## 83. Use Case Naming

Use case names SHOULD use verb-noun form.

Good:

```text
StartSession
ExecuteDiceRoll
AdjustMemoryRelevance
```

Avoid:

```text
SessionManager
DoRoll
HandleData
ProcessEverything
```

## 84. Application Service Boundaries

Use cases MAY be grouped into explicit application services.

Possible groups:

```text
CampaignApplicationService
CharacterApplicationService
SessionApplicationService
DiceApplicationService
MemoryApplicationService
RuleSetApplicationService
RecoveryApplicationService
```

These groups MUST NOT become generic God Services.

Each public method should map to one documented use case.

## 85. Event Publication

A successful use case MAY emit in-process Domain Events.

Examples:

```text
CampaignCreated
CampaignReady
SessionStarted
DiceRollResolved
SceneCompleted
SessionFinalized
CampaignArchived
```

Critical state changes remain inside the originating transaction.

## 86. Observability

Each command SHOULD record:

- operation identifier;
- use case name;
- Campaign identifier where applicable;
- duration;
- result category;
- retry count;
- external dependency duration;
- concurrency conflict;
- persistence result.

Logs MUST redact hidden Campaign data.

## 87. Testing Strategy

Every command SHOULD have tests for:

- success;
- validation failure;
- invalid state;
- not found;
- concurrency conflict;
- persistence failure;
- idempotent retry;
- hidden-information safety where applicable.

Queries SHOULD have tests for:

- filtering;
- ordering;
- localization;
- read-only behavior;
- performance-relevant projection behavior.

## 88. End-to-End MVP Scenario

The use cases MUST support this complete flow:

```text
List Rule Sets
    ↓
Create Campaign Draft
    ↓
Create Player Character
    ↓
Generate Campaign
    ↓
Get Campaign Dashboard
    ↓
Start Session
    ↓
Process Player Input
    ↓
Execute Dice Roll
    ↓
Continue After Dice Roll
    ↓
Process More Input
    ↓
Request Session Finalization
    ↓
Review Finalization Result
    ↓
List Campaign Memories
    ↓
Resume Later
```

A product that cannot complete this flow has not delivered the Chronicle MVP.

## 89. Prohibited Patterns

### 89.1 UI-Owned Use Case Logic

Presentation components MUST NOT implement domain workflows directly.

### 89.2 Generic Save Endpoint

Chronicle MUST NOT expose one unrestricted command that persists arbitrary Campaign data.

### 89.3 Mutable Entity Exposure

Queries MUST NOT return Domain entities for direct client mutation.

### 89.4 Hidden Side Effects in Queries

Queries MUST NOT alter authoritative state.

### 89.5 Provider Call from UI

The UI MUST NOT invoke the Narrator or Archivist directly.

### 89.6 Duplicate Command Application

Retries MUST NOT duplicate state transitions.

### 89.7 Use Case as Repository Wrapper

Application services MUST coordinate meaningful behavior rather than merely exposing CRUD for every record.

### 89.8 One Massive Application Service

All use cases MUST NOT be collapsed into a generic `ChronicleService`.

## 90. Current Delivery Decision

The MVP adopts:

- explicit command and query use cases;
- operation identifiers for retriable commands;
- purpose-built read models;
- Chronicle Director orchestration for narrative turns;
- explicit Session and Dice workflows;
- explicit finalization workflow;
- explicit Memory relevance adjustment;
- status queries for long-running operations;
- recovery commands;
- no full CQRS infrastructure;
- no generic CRUD API;
- no multiplayer authorization;
- no background autonomous commands.

## 91. Architecture Horizon

Future use cases MAY include:

- multiplayer invitations;
- multiple Player Characters;
- synchronized turns;
- Campaign sharing;
- import and export;
- cloud backup;
- voice narration;
- generated media;
- streaming;
- Rule Set installation;
- community content;
- spectator access;
- remote administration.

They are not part of Current Delivery.

## 92. Open Questions

The following remain open:

- Which commands should be exposed directly by the official application?
- Should Campaign draft creation and Player Character creation be one wizard transaction or separate use cases?
- Should finalization require a review-and-confirm command?
- Should `ValidateCharacterSheet` be modeled as a query or command-like operation?
- Which read models should be cached?
- Should `ContinueAfterDiceRoll` run automatically after execution or remain a separate visible operation?
- How should long-running operation progress be represented?
- Should `CompleteScene` and `CompleteAct` remain internal-only?
- Which recovery commands should be visible to players?
- Should Campaign deletion support a grace period?
- Which application events require persistence?
- How should locale affect generated narrative contracts?
- Should `ProcessPlayerInput` support structured actions in the MVP?
- How should operation identifiers be generated by the client?
- Which use cases require dedicated permissions after MVP?

These questions require RFC-0017, RFC-0018, UI RFCs, and technology ADRs.

## 93. Compliance Checklist

An application implementation complies when:

- every state-changing action maps to an explicit command;
- queries are read-only;
- use cases validate preconditions;
- Domain ownership remains intact;
- external calls occur through ports;
- long transactions do not wrap provider calls;
- retries are idempotent;
- stale versions are rejected;
- read models filter hidden information;
- the UI does not mutate persistence directly;
- the UI does not call providers directly;
- complete MVP flow is supported;
- no generic unrestricted save operation exists.

## 94. Final Principle

The Application layer does not decide what Chronicle means.

It ensures that every meaningful request reaches the right domain rule, external capability, and transaction in the right order.
