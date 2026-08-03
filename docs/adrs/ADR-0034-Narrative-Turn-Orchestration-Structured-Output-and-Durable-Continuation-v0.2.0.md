---
id: ADR-0034
title: Narrative Turn Orchestration, Structured Output, and Durable Continuation
status: Proposed
version: 0.2.0
owner: Chronicle Team
last_updated: 2026-08-02
category: Technology
supersedes:
  - ADR-0034@0.1.0
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
  - ADR-0024
  - ADR-0025
  - ADR-0026
  - ADR-0033
implements:
  - RFC-0021
  - RFC-0025
related_to:
  - ADR-0035
  - ADR-0036
  - ADR-0038
  - ADR-0040
  - ADR-0041
---

> **"A Narrative Turn is durable orchestration around a proposal. It is not fictional truth until Chronicle accepts and persists its effects."**

# Narrative Turn Orchestration, Structured Output, and Durable Continuation

## 1. Status

**Proposed**

This ADR defines the concrete Application and Infrastructure architecture for Narrator turns.

This revision resolves the conflict between two incompatible output models.

The canonical output is now:

```text
NarratorTurnOutput
├── ContractVersion
├── NarrativeTurnId
├── CompletionStatus
├── NarrativeBlocks[]
├── StructuredEvents[]
├── StopReason
├── ContinuityClaims[]
├── Warnings[]
└── ProviderMetadataReference
```

The decision is:

- `NarrativeTurn` is a durable Application workflow record;
- it is not a Domain aggregate and does not itself define fictional truth;
- Chronicle creates the turn and owns its identity;
- the provider returns exactly one `NarratorTurnOutput` contract;
- prose uses `NarrativeBlocks[]`;
- every nonprose proposal uses `StructuredEvents[]`;
- fixed top-level fields such as `ProposedRoll`, `ProposedChoice`, `ProposedSceneTransition`, and `ProposedConsequences` are removed;
- `TurnDisposition` is derived by Chronicle from accepted events and Stop Reason;
- a closed event registry governs every executable proposal;
- Chronicle validates output before publishing prose or applying effects;
- a valid blocking event pauses the turn;
- `roll.requested` creates a Chronicle-owned Dice workflow and stops narration;
- continuation occurs in a new correlated Narrative Turn after Dice resolution;
- accepted Narrative Blocks become authoritative Messages;
- accepted Structured Events become approved Application operations;
- provider retries and repairs remain attempts under the same NarrativeTurnId;
- duplicate responses do not duplicate Messages or effects;
- provider-native thread state remains nonauthoritative;
- structured durable Choice is excluded from MVP;
- ordinary questions to the player remain prose;
- Narrative Turn, Provider Attempt, accepted Message, Dice Roll, Operation Record, and Work Item relationships must be persisted explicitly;
- all semantic keys and base diagnostics use English;
- the orchestration is independent from Werewolf or any other Rule Set.

## 2. Context

Narrative generation is not a single atomic database call.

A Narrator turn may involve:

- context construction;
- provider request;
- provider retry;
- structured output validation;
- bounded repair;
- accepted prose;
- proposed state changes;
- a Roll interruption;
- user input;
- Dice resolution;
- continuation;
- crash recovery;
- stale response rejection.

Chronicle must preserve authority while allowing this process to survive interruption.

The original ADR introduced useful durable concepts but also defined a second root response contract with fixed proposal fields.

That would have made provider adapters, persistence, routing, and tests incompatible with RFC-0021 and RFC-0025.

This revision keeps durable orchestration while adopting the event-based contract as the sole model.

## 3. Decision Drivers

The design prioritizes:

1. Chronicle authority;
2. durable recovery;
3. one canonical output contract;
4. provider neutrality;
5. idempotency;
6. explicit Dice interruption;
7. ordered accepted narrative;
8. typed event routing;
9. no hidden provider state;
10. future Rule Set complexity;
11. MVP scope control;
12. testability.

## 4. Architectural Position

```text
Domain
    Campaign truth

Application
    Narrative Turn orchestration

Narrative Intelligence Adapter
    provider transport and translation

Persistence
    durable workflow evidence

Presentation
    user-visible progress and accepted narrative
```

## 5. Narrative Turn Classification

`NarrativeTurn` is an Application workflow record.

It is not:

- a Campaign aggregate child defining fictional truth;
- a provider conversation thread;
- a Message;
- a Dice Roll;
- a Work Item;
- an Operation Record.

It correlates these concepts.

## 6. Narrative Turn Identity

Chronicle generates:

```text
NarrativeTurnId
```

before provider execution.

## 7. Narrative Turn Record

Recommended fields:

```text
NarrativeTurn
├── NarrativeTurnId
├── CampaignId
├── SessionId
├── ActId
├── SceneId
├── ParentNarrativeTurnId
├── ContinuationOfDiceRollId
├── TriggerType
├── TriggerReference
├── Role
├── ContractVersion
├── State
├── ContextSnapshotReference
├── AcceptedOutputReference
├── AcceptedAtUtc
├── CreatedAtUtc
├── UpdatedAtUtc
├── RowVersion
└── FailureSummary
```

## 8. Trigger Type

Initial values may include:

```text
PlayerMessage
DiceResolution
SceneStart
SessionStart
RecoveryContinuation
ChronicleDirectorRequest
```

## 9. Parent Turn

A continuation may reference the prior NarrativeTurnId.

## 10. Dice Continuation

A post-Roll continuation references the resolved DiceRollId.

## 11. Turn Role

The MVP role is:

```text
Narrator
```

Other Narrative Intelligence roles use separate contracts.

## 12. Narrative Turn State Machine

The canonical states are:

```text
Created
ContextBuilding
ReadyForProvider
ProviderInProgress
OutputReceived
Validating
RepairRequired
Accepted
AwaitingDiceResolution
AwaitingPlayerInput
AwaitingChronicleDecision
ContinuationScheduled
Completed
Superseded
Cancelled
RecoveryRequired
FailedTerminal
```

## 13. Created

The durable turn record exists.

No provider request has started.

## 14. Context Building

Chronicle is assembling the bounded context.

## 15. Ready for Provider

Context and contract are valid.

## 16. Provider In Progress

At least one Provider Attempt is active.

## 17. Output Received

A candidate response has been captured safely.

## 18. Validating

Chronicle is validating the canonical output.

## 19. Repair Required

The response is repairable and a bounded repair may run.

## 20. Accepted

The selected output passed validation.

Accepted does not mean every downstream event has completed.

## 21. Awaiting Dice Resolution

A valid `roll.requested` event has created or correlated a Dice Roll.

## 22. Awaiting Player Input

The accepted prose asks the player to respond.

No durable structured Choice is required.

## 23. Awaiting Chronicle Decision

Chronicle must perform or confirm another bounded workflow.

## 24. Continuation Scheduled

Required external or user-dependent work completed and a continuation turn is durably scheduled.

## 25. Completed

All required work for this turn stage is complete.

## 26. Superseded

A newer authoritative turn replaced this turn before acceptance.

## 27. Cancelled

The turn ended before authoritative acceptance.

## 28. Recovery Required

Chronicle cannot safely determine or resume the state automatically.

## 29. Failed Terminal

The turn cannot continue without a new explicit user or Application action.

## 30. Allowed Transition Principle

Transitions are explicit and persisted.

The state machine must not be reconstructed from provider logs.

## 31. Terminal States

Terminal states are:

```text
Completed
Superseded
Cancelled
FailedTerminal
```

`RecoveryRequired` is not terminal.

## 32. Turn Disposition

`TurnDisposition` is not a provider wire field.

Chronicle derives a presentation and orchestration summary such as:

```text
Completed
AwaitingDice
AwaitingPlayer
AwaitingChronicle
Repairing
Failed
```

## 33. Canonical Provider Output

Provider adapters return only:

```text
NarratorTurnOutput
```

as defined by RFC-0021.

## 34. No Fixed Proposal Fields

The following are prohibited in the root output:

```text
ProposedRoll
ProposedChoice
ProposedSceneTransition
ProposedConsequences
ProposedMemorySignals
```

## 35. Narrative Blocks

Accepted blocks become Chronicle Messages.

They remain nonauthoritative until the complete output passes the required validation and acceptance policy.

## 36. Structured Events

Every proposed effect is represented through the registered Structured Event catalog.

## 37. Event Router

Accepted events are routed by:

```text
EventTypeKey
EventContractVersion
```

into Application handlers.

## 38. Direct Persistence Prohibition

Provider DTOs and event handlers do not write arbitrary persistence directly.

Handlers invoke approved commands and use explicit transactions.

## 39. Output Acceptance Unit

Chronicle first validates the output as a coherent turn.

It then validates each event.

## 40. Partial Acceptance

Partial acceptance is allowed only when accepted prose remains truthful and coherent without rejected events.

Otherwise Chronicle requests repair or rejects the turn.

## 41. Message Publication

Accepted Messages and event application evidence should be published in a transactionally coherent way.

## 42. Preferred Publication Boundary

Recommended:

1. validate complete output;
2. create canonical accepted-output record;
3. create accepted Messages;
4. create event-application records;
5. commit accepted narrative evidence;
6. dispatch or schedule event operations;
7. update Narrative Turn state.

## 43. No External Wait in Publication Transaction

Provider calls, user input, Dice release, and long-running work occur outside database transactions.

## 44. Accepted Output Record

Recommended:

```text
NarrativeAcceptedOutput
├── NarrativeAcceptedOutputId
├── NarrativeTurnId
├── ContractVersion
├── CompletionStatus
├── StopReason
├── CanonicalPayloadHash
├── AcceptedAtUtc
└── ProviderAttemptId
```

## 45. Payload Storage

The implementation may store:

- normalized canonical output;
- normalized event payloads;
- accepted Message records;
- safe hash and metadata.

Raw provider payload retention follows ADR-0036 and privacy policy.

## 46. Canonical Payload Hash

A hash supports duplicate and forensic comparison.

It does not replace semantic validation.

## 47. Provider Attempt

Each provider call is represented by:

```text
ProviderAttempt
```

## 48. Provider Attempt Fields

Recommended:

```text
ProviderAttempt
├── ProviderAttemptId
├── NarrativeTurnId
├── AttemptNumber
├── ProviderProfileId
├── AdapterKey
├── ModelProfileKey
├── RequestContractVersion
├── StartedAtUtc
├── CompletedAtUtc
├── State
├── ProviderRequestReference
├── ProviderResponseStagingReference
├── SafeProviderMetadata
├── FailureCode
├── RetryClassification
└── RowVersion
```

## 49. Provider Attempt State

Recommended:

```text
Created
Requesting
ResponseReceived
FailedRetryable
FailedTerminal
Cancelled
Accepted
Rejected
Superseded
```

## 50. Attempt Authority

Provider Attempt is operational evidence.

It does not define Campaign truth.

## 51. Attempt Retention

Stable retention should preserve:

- timing;
- adapter;
- model profile;
- safe provider request ID where allowed;
- status;
- token or usage metadata where policy permits;
- failure classification.

It should not preserve unrestricted prompt or response content indefinitely.

## 52. Response Staging

A provider response may be staged durably before validation when needed for crash recovery.

## 53. Response Staging Record

Recommended:

```text
NarrativeResponseStaging
├── NarrativeResponseStagingId
├── ProviderAttemptId
├── ContractVersionHint
├── PayloadHash
├── EncryptedOrProtectedLocation
├── ByteLength
├── CreatedAtUtc
├── ExpiresAtUtc
└── State
```

## 54. Staging Retention

Staged raw content is deleted after:

- acceptance;
- terminal rejection;
- retention expiry;
- successful diagnostic capture where explicitly requested.

## 55. Staging Privacy

Staging must not contain credentials.

Its local storage policy follows security and privacy requirements.

## 56. Provider-Native Thread State

Remote thread or conversation IDs are nonauthoritative.

## 57. Thread State Use

An adapter may use provider-native state as an optimization.

Chronicle must remain capable of reconstructing required context from local records.

## 58. Thread Loss

Loss of provider thread state does not corrupt Campaign truth.

## 59. Context Snapshot

Each turn references the local context selection used for generation.

## 60. Context Snapshot Contents

The snapshot reference may identify:

- active Campaign hierarchy;
- selected Messages;
- Character state;
- Character Knowledge;
- Campaign Memories;
- Rule Set context;
- Preferences;
- unresolved Dice context;
- role instructions;
- contract version.

## 61. Context Snapshot Privacy

The snapshot must not include credentials.

## 62. Context Snapshot Storage

Chronicle may persist:

- a normalized context manifest;
- record references and versions;
- hashes;
- bounded rendered content when needed for recovery.

## 63. Context Staleness

Before accepting output, Chronicle verifies relevant authoritative versions.

## 64. Stale Context

If authoritative state changed incompatibly, the output is rejected or superseded.

## 65. Optimistic Concurrency

Narrative acceptance uses expected Campaign and Scene versions where relevant.

## 66. Player Message Trigger

A player message becomes authoritative before the Narrator turn starts.

## 67. No Duplicate Player Message

Narrative retry does not duplicate the triggering player Message.

## 68. Provider Execution

Provider execution occurs through the selected adapter and official provider profile.

## 69. Provider Independence

Application orchestration depends on provider-neutral contracts.

## 70. Retry Policy

Retry is bounded and classified under ADR-0036.

## 71. Repair Attempts

Repair attempts remain under the same NarrativeTurnId.

## 72. Attempt Number

Every provider call receives a monotonically increasing AttemptNumber within the turn.

## 73. Accepted Attempt

At most one ProviderAttempt is accepted for one Narrative Turn stage.

## 74. Late Attempt

A late response from a superseded attempt is recorded and rejected.

## 75. Duplicate Output

The same canonical payload hash under the same turn does not create duplicate effects.

## 76. Validation Pipeline

Recommended:

```text
Transport Decode
    ↓
Root Contract Validation
    ↓
NarrativeTurnId Validation
    ↓
Narrative Block Validation
    ↓
Structured Event Registry Validation
    ↓
Payload Validation
    ↓
Ordering and Stop-Boundary Validation
    ↓
Authority Validation
    ↓
Rule Set Validation
    ↓
Continuity and Context Validation
    ↓
Acceptance Decision
```

## 77. Root Validation

Checks:

- required root fields;
- supported version;
- size limits;
- CompletionStatus;
- StopReason;
- array limits.

## 78. Block Validation

Checks:

- sequence;
- text size;
- speaker;
- Scene;
- visibility;
- prohibited hidden authority.

## 79. Event Validation

Uses RFC-0025 and the event registry.

## 80. Stop-Boundary Validation

No accepted output may narrate or propose events beyond a hard unresolved boundary.

## 81. Authority Validation

Provider proposals cannot bypass Chronicle, Rule Set, user, or Archivist authority.

## 82. Rule Set Validation

Mechanical event payloads are validated through the exact Rule Set package version bound to the Campaign.

## 83. Rule Set Neutrality

Core orchestration does not assume:

- Werewolf;
- d10;
- one pool;
- one specialty model;
- one resolution stage.

## 84. Complex Future Dice

A `roll.requested` event may carry a Rule Set payload for future systems with:

- multiple Dice groups;
- mixed Dice sizes;
- specialties;
- modifiers;
- rerolls;
- exploding or chained Dice;
- keep/drop;
- custom symbols;
- opposed Rolls;
- staged resolution;
- post-Roll resource spending;
- player decisions.

## 85. No Premature Dice UI Requirement

The MVP UI supports only mechanics required by the first package.

The contract and persistence remain extensible.

## 86. Roll Request Handling

When `roll.requested` is accepted:

1. create an event-application OperationId;
2. validate the exact Rule Set payload;
3. create a Chronicle-owned DiceRoll record;
4. persist the relationship to NarrativeTurnId and EventId;
5. publish accepted pre-Roll Messages;
6. set Narrative Turn state to `AwaitingDiceResolution`;
7. show the Roll UI;
8. stop provider continuation.

## 87. Dice Generation

Chronicle generates and commits random evidence only after explicit player release according to ADR-0033.

## 88. Dice Resolution

The Rule Set resolves the committed evidence.

## 89. Post-Roll Continuation

After Dice resolution:

1. commit the resolved Dice state;
2. create a continuation NarrativeTurn;
3. set `ParentNarrativeTurnId`;
4. set `ContinuationOfDiceRollId`;
5. build new context including exact Dice evidence and result;
6. call the Narrator;
7. validate the new output;
8. publish continuation Messages and events.

## 90. No Resume of Same Provider Output

Chronicle does not ask the provider to continue an already accepted incomplete JSON document.

It starts a new correlated turn.

## 91. Dice Retry

Retry never regenerates already committed random evidence.

## 92. Player Input Handling

When StopReason is `AwaitingPlayerInput`:

- accepted prose is published;
- the turn enters `AwaitingPlayerInput`;
- normal chat input remains available;
- no Choice entity is required;
- the next player Message creates a new Narrative Turn.

## 93. No Structured Choice in MVP

The following are not implemented:

- Choice;
- ChoiceOption;
- Choice lifecycle;
- structured option cards;
- durable option selection command.

## 94. Future Choice

A future `choice.offered` event requires a separate ADR and persistence update.

## 95. Awaiting Chronicle Decision

Used for bounded Chronicle workflows that are not Dice and not player prose input.

## 96. Scene Transition Handling

A Scene transition remains a proposal until the Application handler validates and persists it.

## 97. Character Handling

Character entry, exit, and draft creation remain Application operations.

## 98. Knowledge Handling

Knowledge events create Character Knowledge only after validation.

## 99. Memory Handling

The Narrator does not persist Campaign Memories directly.

Memory creation remains under the appropriate Application or Archivist workflow.

## 100. Plan Revision

Plan revision suggestions update orchestration planning evidence, not Campaign truth.

## 101. Work Item Integration

Provider calls, repairs, and continuations may use durable Work Items.

## 102. Work Item Relationship

Recommended:

```text
WorkItem.NarrativeTurnId
WorkItem.ProviderAttemptId
```

where applicable.

## 103. Work Item Types

Potential types:

```text
NarratorProviderRequest
NarratorRepairRequest
NarratorContinuation
NarratorRecoveryInspection
```

Exact names follow the canonical Work Item registry.

## 104. Operation Record Integration

Every authoritative event application uses an Operation Record.

## 105. Operation Relationship

Recommended:

```text
OperationRecord.NarrativeTurnId
OperationRecord.NarrativeEventId
```

## 106. Message Relationship

Every accepted Narrator Message references:

```text
NarrativeTurnId
NarrativeAcceptedOutputId
```

## 107. Dice Relationship

Every Dice Roll created by a Narrator event references:

```text
OriginNarrativeTurnId
OriginNarrativeEventId
ContinuationNarrativeTurnId
```

where applicable.

## 108. Provider Attempt Relationship

Every attempt belongs to exactly one NarrativeTurnId.

## 109. Persistence Requirement

ADR-0004 and RFC-0033 must include tables and constraints for:

- NarrativeTurn;
- NarrativeAcceptedOutput;
- ProviderAttempt;
- NarrativeResponseStaging;
- event-application correlation;
- Message references;
- Dice references;
- Work Item references;
- Operation Record references.

## 110. Unique Constraints

Recommended constraints:

```text
ProviderAttempt(NarrativeTurnId, AttemptNumber) unique
NarrativeAcceptedOutput(NarrativeTurnId) unique per accepted stage
EventApplication(NarrativeTurnId, EventId) unique
Message(NarrativeTurnId, Sequence) unique
DiceRoll(OriginNarrativeTurnId, OriginNarrativeEventId) unique
```

## 111. Transaction Boundaries

Short transactions are used for:

- turn creation;
- attempt creation;
- response staging metadata;
- acceptance publication;
- Dice request creation;
- continuation scheduling;
- completion.

## 112. No Provider Call in Transaction

Provider network calls occur outside database transactions.

## 113. No User Wait in Transaction

Waiting for Roll release or player input occurs outside transactions.

## 114. Crash Before Provider Call

The turn remains resumable from `ReadyForProvider`.

## 115. Crash During Provider Call

The Provider Attempt is inspected and safely retried according to idempotency and provider policy.

## 116. Crash After Response Receipt

Staged response may be revalidated.

## 117. Crash During Acceptance

OperationIds and unique constraints determine whether publication committed.

## 118. Crash After Dice Request Creation

The existing DiceRoll is recovered and shown.

## 119. Crash While Awaiting Player Input

Accepted Messages and turn state are recovered.

## 120. Crash After Dice Resolution

Chronicle schedules or recovers the continuation turn without rerolling.

## 121. Unknown Commit

The recovery algorithm queries authoritative records by:

- NarrativeTurnId;
- EventId;
- OperationId;
- DiceRollId;
- accepted-output identity.

## 122. Safe Retry

A retry must never duplicate:

- accepted Messages;
- event effects;
- Dice Rolls;
- Scene transitions;
- Characters;
- Knowledge;
- continuation turns.

## 123. Supersession

A turn may be superseded when:

- Campaign state changes incompatibly;
- the user cancels and submits a new action;
- a recovery workflow replaces it;
- another accepted turn owns the same stage.

## 124. Superseded Output

Superseded provider output is not published.

## 125. Cancellation

Cancellation before acceptance leaves Campaign truth unchanged except for already authoritative trigger records such as the player's Message.

## 126. Terminal Failure

Terminal failure preserves:

- trigger;
- turn record;
- safe attempt evidence;
- user-visible recovery action;
- unchanged Campaign state unless earlier explicit operations already committed.

## 127. User Experience

Presentation consumes a Narrative Turn read model.

## 128. Read Model Fields

Recommended:

```text
NarrativeTurnId
DisplayState
StartedAtUtc
CurrentStage
RetryAvailable
CancelAvailable
AcceptedMessages
PendingRoll
FailureReference
```

## 129. Progress Language

Base UI text is English.

Examples:

```text
Preparing context
Generating narration
Validating response
Repairing response
Waiting for dice roll
Waiting for your response
Recovering narrative turn
```

## 130. Honest Progress

Provider latency uses indeterminate progress.

## 131. No Raw Provider Errors

Users receive typed Chronicle errors and safe recovery actions.

## 132. Retry UI

Retry reuses the same turn when semantically safe.

A new user action creates a new turn.

## 133. Cancel UI

Cancellation availability depends on whether authoritative effects have committed.

## 134. Roll UI

The Roll card is sourced from the Chronicle-owned DiceRoll record, not from raw provider output.

## 135. Accessibility

Narrative progress, errors, Roll boundaries, and player-input waits are accessible through keyboard and screen reader.

## 136. Language Policy

Semantic keys, contract names, internal states, diagnostics, and base UI text use English.

Narrative prose may use the Campaign-selected language.

## 137. Logging

Safe logs may include:

- NarrativeTurnId;
- ProviderAttemptId;
- WorkItemId;
- OperationId;
- state transition;
- adapter key;
- model profile key;
- event type keys;
- block count;
- attempt number;
- Stop Reason;
- duration;
- safe failure code.

They must not include:

- credentials;
- full prompts;
- full raw responses;
- full Campaign prose by default;
- private Character details;
- secret references.

## 138. Metrics

Useful local metrics include:

```text
NarrativeTurnDuration
ProviderAttemptCount
NarrativeRepairCount
NarrativeAcceptanceFailureCount
NarrativeTurnRecoveryCount
AwaitingDiceCount
AwaitingPlayerCount
DuplicateResponseCount
StaleResponseCount
```

No remote telemetry is required.

## 139. Error Model

Recommended errors:

```text
narrative-turn.not-found
narrative-turn.state-invalid
narrative-turn.context-build-failed
narrative-turn.context-stale
narrative-turn.provider-failed
narrative-turn.output-invalid
narrative-turn.repair-exhausted
narrative-turn.acceptance-conflict
narrative-turn.event-application-failed
narrative-turn.roll-creation-failed
narrative-turn.continuation-failed
narrative-turn.superseded
narrative-turn.cancel-not-allowed
narrative-turn.recovery-required
narrative-turn.failed-terminal
```

## 140. Data Preservation State

Results should state:

```text
CampaignStateUnchanged
TriggerPreserved
TurnCreated
ProviderAttemptRecorded
OutputStaged
OutputAccepted
MessagesPublished
EventsScheduled
DiceRollCreated
AwaitingDiceResolution
AwaitingPlayerInput
ContinuationScheduled
TurnCompleted
RecoveryRequired
```

## 141. Testing Strategy

The implementation requires:

```text
State Machine Tests
Persistence Tests
Provider Attempt Tests
Output Contract Tests
Event Routing Tests
Message Publication Tests
Dice Interruption Tests
Continuation Tests
Idempotency Tests
Crash Recovery Tests
Stale Context Tests
Privacy Tests
Presentation Read Model Tests
```

## 142. State Machine Tests

Tests must cover every allowed and forbidden transition.

## 143. Persistence Tests

Tests must prove:

- durable turn creation;
- unique attempt numbers;
- one accepted output;
- Message correlation;
- event correlation;
- Dice correlation;
- Work Item and Operation relationships.

## 144. Provider Attempt Tests

Tests cover:

- success;
- retryable failure;
- terminal failure;
- repair;
- late response;
- duplicate response;
- cancellation;
- supersession.

## 145. Output Contract Tests

Tests use only `NarratorTurnOutput`.

Fixed top-level proposal fields must fail.

## 146. Event Routing Tests

Every initial event routes through a registered Application handler.

## 147. Message Publication Tests

Tests prove accepted block ordering and no duplicate Messages.

## 148. Dice Interruption Tests

Tests must prove:

1. pre-Roll prose is accepted;
2. `roll.requested` creates one DiceRoll;
3. narration stops;
4. restart shows the same pending Roll;
5. player releases the Roll;
6. raw values commit;
7. Rule Set resolves;
8. continuation turn is created;
9. Dice are not regenerated.

## 149. Complex Dice Contract Test

A synthetic non-Werewolf package must support a request with:

- multiple groups;
- mixed Dice sizes;
- specialties;
- modifiers;
- rerolls;
- exploding Dice;
- keep/drop;
- opposed resolution;
- post-Roll decision metadata.

This is a contract test, not an MVP UI requirement.

## 150. Player Input Tests

Tests prove ordinary prose questions require no Choice entity and the next player Message creates a new turn.

## 151. Recovery Tests

Inject failure:

- before provider call;
- during provider call;
- after response staging;
- during validation;
- during acceptance;
- after Message commit;
- after Dice request creation;
- after Dice resolution;
- before continuation scheduling;
- after continuation scheduling.

## 152. Idempotency Tests

Retries create no duplicate:

- accepted output;
- Message;
- event effect;
- Dice Roll;
- continuation turn.

## 153. Stale Context Tests

A response generated from incompatible Campaign versions is rejected or superseded.

## 154. Privacy Tests

Credential, hidden prompt, and private-context canaries do not enter Stable logs or provider metadata records.

## 155. Architecture Tests

Architecture tests must reject:

- provider DTOs in Domain;
- fixed top-level proposal fields;
- direct repository access from provider adapters;
- direct DbContext access from event handlers;
- provider-native thread as required truth;
- Narrator-generated Dice values;
- d10-specific Core turn fields;
- active Choice persistence in MVP;
- uncorrelated accepted Messages;
- Dice Roll without origin NarrativeTurnId;
- provider call inside database transaction.

## 156. Prohibited Patterns

### 156.1 Narrative Turn as Domain Truth

It is Application orchestration.

### 156.2 Two Root Output Contracts

Use only `NarratorTurnOutput`.

### 156.3 Fixed Proposal Properties

Use Structured Events.

### 156.4 Direct Provider Persistence

Validate and route through Application.

### 156.5 Resume an Accepted Partial JSON Response

Create a correlated continuation turn.

### 156.6 Reroll on Narrative Retry

Reuse committed Dice evidence.

### 156.7 Provider Thread as Recovery Source

Recover from Chronicle records.

### 156.8 Structured Choice Without Full Design

Use prose in MVP.

### 156.9 Show Raw Provider Error

Use typed Chronicle errors.

### 156.10 Long Transaction Around Provider or User Wait

Persist state and release the transaction.

## 157. Alternatives Considered

### Stateless Narrator Calls

Rejected because crashes, retries, Dice interruptions, and stale responses require durable correlation.

### Provider Thread as Turn State

Rejected because it is nonportable and provider-owned.

### Fixed Root Proposal Fields

Rejected because they conflict with the canonical event model.

### One Turn Resumed After Dice

Rejected because a new correlated turn gives clearer idempotency, context, and recovery.

### Structured Choice in MVP

Rejected because free-text input already supports the product requirement.

### Store Raw Provider Payload Forever

Rejected because of privacy and retention cost.

## 158. Consequences

### Positive

- one canonical Narrator contract;
- durable recovery;
- explicit authority;
- clear Message and event correlation;
- safe Dice interruption;
- no reroll on continuation;
- provider replacement remains possible;
- structured Choice does not expand MVP;
- future complex Rule Sets fit the contract;
- stale and duplicate responses are controlled.

### Negative

- more durable workflow records;
- explicit cleanup policies are required;
- continuation creates additional turns;
- event and Message publication needs careful transaction design;
- repair and recovery increase implementation complexity;
- persistence ADRs require amendment.

## 159. Risks

### Narrative Turn Becomes a Second Domain Aggregate

Mitigation:

- Application classification;
- no fictional truth owned by the turn;
- accepted Messages and Campaign operations remain authoritative.

### Raw Response Staging Creates Privacy Risk

Mitigation:

- bounded retention;
- local protection;
- no credentials;
- deletion after acceptance;
- explicit diagnostics consent.

### Event Accepted but Message Publication Fails

Mitigation:

- coherent acceptance transaction;
- OperationId;
- recovery query;
- unique constraints.

### Dice Continuation Duplicates

Mitigation:

- unique origin and continuation relationships;
- idempotent scheduling;
- existing Dice evidence reuse.

### Future Dice Exceeds First Package

Mitigation:

- Rule Set-defined payloads;
- generic event envelope;
- synthetic cross-system tests.

## 160. Technology Spike

Before acceptance, implement:

1. NarrativeTurn persistence;
2. NarrativeAcceptedOutput persistence;
3. ProviderAttempt persistence;
4. bounded response staging;
5. turn state machine;
6. canonical `NarratorTurnOutput`;
7. event registry integration;
8. accepted Message publication;
9. event-application correlation;
10. Work Item and Operation Record correlation;
11. Roll interruption;
12. Dice continuation;
13. stale context detection;
14. retry and repair;
15. crash recovery;
16. read model;
17. privacy tests.

## 161. Spike Acceptance

The spike passes when:

- one player Message creates one durable turn;
- one provider attempt returns the canonical output;
- accepted blocks become ordered Messages;
- accepted events route through Application;
- a Roll creates one durable DiceRoll and pauses narration;
- restart recovers the pending Roll;
- Dice resolution creates one continuation turn;
- duplicate response creates no duplicate effect;
- late response is rejected;
- malformed output is repaired;
- raw staging is deleted according to policy;
- ordinary player questions use no Choice entity;
- a complex synthetic non-Werewolf Roll payload validates.

## 162. Definition of Compliance

An implementation complies when:

- NarrativeTurn is a durable Application workflow record;
- Chronicle owns its identity and state;
- providers return only `NarratorTurnOutput`;
- prose and events remain separate;
- fixed proposal fields are absent;
- accepted prose maps to Messages;
- accepted events map to Application operations;
- provider retries remain attempts under one turn;
- Dice requests pause narration;
- Dice continuation uses a new correlated turn;
- retries never reroll committed Dice;
- provider-native thread state is nonauthoritative;
- structured Choice is absent from MVP;
- persistence records correlate turns, attempts, Messages, Dice, Work Items, and Operations;
- no Core assumption is specific to Werewolf.

## 163. Review Triggers

Review this ADR if:

- structured Choice becomes required;
- provisional token streaming becomes player-visible;
- provider-native tools become authoritative transport;
- multiplayer introduces concurrent recipients;
- Narrative Turns become server-owned;
- raw response retention policy changes;
- Dice continuation semantics change;
- autonomous world simulation is introduced.

## 164. Deferred Decisions

Later decisions may define:

- structured Choice;
- provisional streaming;
- voice and multimodal blocks;
- server-hosted orchestration;
- package-defined event namespaces;
- longer raw-response retention;
- provider thread optimization policies;
- multiplayer turn fan-out.

## 165. Final Decision

Chronicle will persist Narrative Turns as Application workflows.

A provider will return one canonical `NarratorTurnOutput`.

Narrative Blocks will become Messages only after validation.

Structured Events will become Application operations only after authority checks.

A Roll request will pause narration, create a Chronicle-owned Dice workflow, and resume through a new correlated turn after resolution.

Provider retries may repeat computation.

They may never repeat truth.

Werewolf is the first Rule Set package.

It is not the shape of Narrative Turn orchestration.
