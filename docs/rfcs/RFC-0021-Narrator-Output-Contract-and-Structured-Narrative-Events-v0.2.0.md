---
id: RFC-0021
title: Narrator Output Contract and Structured Narrative Events
status: Draft
version: 0.2.0
owner: Chronicle Team
last_updated: 2026-08-02
category: Narrative Intelligence
supersedes:
  - RFC-0021@0.1.0
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
implements: []
related_to:
  - RFC-0025
  - ADR-0010
  - ADR-0034
  - ADR-0035
  - ADR-0036
  - ADR-0040
---

> **"The Narrator may describe what could happen. Chronicle decides which structured consequences are allowed to become true."**

# Narrator Output Contract and Structured Narrative Events

## 1. Status

**Draft**

This RFC defines the single provider-neutral output contract used by the Narrator role.

This revision resolves the conflict between:

- an event-based response containing `NarrativeBlocks` and `StructuredEvents`;
- a fixed-field response containing `ProposedRoll`, `ProposedChoice`, `ProposedSceneTransition`, `ProposedConsequences`, and similar optional properties.

The decision is:

- use one canonical `NarratorTurnOutput`;
- carry player-facing prose in `NarrativeBlocks[]`;
- carry every proposed nonprose consequence in `StructuredEvents[]`;
- use a closed, versioned event registry;
- reject arbitrary provider-defined event types;
- preserve block and event ordering explicitly;
- keep event payloads typed and independently validated;
- let Chronicle accept, reject, normalize, defer, or request repair for proposals;
- stop at unresolved authority boundaries, especially Dice;
- treat `TurnDisposition` as a Chronicle-derived summary rather than a second provider wire contract;
- remove fixed top-level proposal fields;
- keep durable structured user choices outside the MVP;
- let the Narrator ask ordinary questions through prose;
- keep the contract independent from Werewolf and every other specific RPG system;
- use English for semantic keys and base contract documentation.

## 2. Purpose

Chronicle needs a reliable bridge between generated narrative and authoritative state.

The Narrator may:

- produce narration and dialogue;
- request a Roll;
- propose Character entry or exit;
- propose Character Knowledge acquisition;
- propose a bounded state change;
- propose Scene completion or transition;
- suggest plan revision;
- state that no structured change is proposed.

The Narrator may not:

- mutate Campaign state;
- generate Dice results;
- decide Rule Set mechanics;
- persist Messages;
- create arbitrary semantic event types;
- bypass Application validation;
- turn prose into hidden authority.

## 3. Scope

This RFC covers:

- response envelope;
- Narrative Blocks;
- Structured Events;
- event registry;
- ordering;
- stop reasons;
- completion status;
- Roll interruption;
- Character, knowledge, state, Scene, and planning proposals;
- continuity claims;
- warnings;
- repair;
- idempotency;
- future event extension;
- MVP choice policy.

## 4. Out of Scope

This RFC does not define:

- provider transport;
- prompt construction;
- provider credentials;
- provider retry scheduling;
- exact persistence tables;
- exact Dice mechanics;
- Archivist output;
- UI technology;
- multiplayer;
- provisional streaming acceptance.

## 5. Canonical Envelope

The canonical response is:

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

## 6. Single-Contract Rule

No second canonical Narrator response shape exists.

The following top-level fields are prohibited:

```text
ProposedRoll
ProposedChoice
ProposedSceneTransition
ProposedConsequences
ProposedMemorySignals
```

Their semantics, when supported, belong in typed Structured Events.

## 7. Contract Version

`ContractVersion` versions the Narrator output independently from:

- application version;
- provider API version;
- Rule Set version;
- Dice contract version;
- package version;
- database schema version.

## 8. Narrative Turn Identity

Chronicle creates `NarrativeTurnId`.

The provider echoes it.

Unknown or mismatched identities are rejected.

## 9. Completion Status

Initial values are:

```text
Complete
Partial
Blocked
Invalid
```

`Complete` means the requested step finished.

`Partial` means the Narrator stopped at a valid unresolved boundary.

`Blocked` means required context or authority was unavailable.

`Invalid` indicates that the returned shape cannot satisfy the role contract, subject to Chronicle's own validation.

## 10. Narrative Blocks

`NarrativeBlocks` contain prose that may become player-visible after acceptance.

Recommended shape:

```text
NarrativeBlock
├── BlockId
├── Sequence
├── BlockType
├── Text
├── SpeakerReference
├── SceneReference
├── Visibility
└── Tags[]
```

## 11. Narrative Block Types

Initial types are:

```text
Narration
Dialogue
Description
SystemFacingSummary
```

`SystemFacingSummary` is not player-visible unless an explicit Presentation policy permits it.

## 12. Narrative Ordering

Every block has an explicit sequence.

Chronicle does not infer ordering from timestamps or array accident.

## 13. Speaker References

Dialogue may reference an authoritative CharacterId.

A new speaker requires a validated Character proposal before becoming canonical.

## 14. Scene References

Blocks may reference the active Scene or another permitted Scene context.

The reference does not create or transition a Scene.

## 15. Prose Has No Mutation Authority

Prose cannot by itself:

- create a Character;
- change a Character Sheet;
- grant knowledge;
- change resources;
- resolve Dice;
- modify progression;
- complete a Scene;
- persist a Campaign Memory.

A structured proposal is required.

## 16. Structured Events

Every nonprose proposal is represented as:

```text
NarrativeEvent
├── EventId
├── Sequence
├── EventTypeKey
├── EventContractVersion
├── Payload
├── CorrelationReferences[]
├── RequiredAuthority
├── BlockingBehavior
└── SourceBlockReferences[]
```

## 17. Event Ordering

Events and Narrative Blocks use one deterministic ordering domain or an equivalent unambiguous interleaving model.

A typical Roll boundary is:

```text
Narrative Block
    then
roll.requested
    then
stop
```

## 18. Event Registry

`EventTypeKey` must exist in Chronicle's closed event registry.

Providers cannot create new event semantics dynamically.

## 19. Initial Event Catalog

The initial registry contains:

```text
roll.requested
character.entry-proposed
character.exit-proposed
knowledge.acquisition-proposed
immediate-state-change-proposed
scene.completion-proposed
scene.transition-proposed
plan.revision-suggested
no-state-change
```

## 20. Future Event Catalog

Potential future additions include:

```text
choice.offered
relationship.change-proposed
resource-spend-proposed
condition-change-proposed
memory.signal-proposed
```

Each requires explicit registration, versioning, validation, authority, persistence, recovery, and tests.

## 21. Event Contract Version

Each event type has its own payload contract version.

The root Narrator contract does not need a new version whenever one event payload evolves compatibly.

## 22. Typed Payloads

`Payload` is validated according to:

```text
EventTypeKey
EventContractVersion
```

A free-form generic consequence blob is not allowed.

## 23. Required Authority

Initial authority values are:

```text
ChronicleApplication
RuleSet
User
Archivist
None
```

This field declares which authority must validate or complete the proposal.

It does not grant authority to the Narrator.

## 24. Blocking Behavior

Initial values are:

```text
NonBlocking
StopAfterEvent
AwaitChronicleResolution
AwaitUserInput
```

## 25. Roll Requested Event

`roll.requested` proposes a Rule Set-governed Roll.

Recommended payload:

```text
RollRequestedPayload
├── ActorReference
├── TargetReference
├── Intent
├── RuleOperationKey
├── RollContractKey
├── RollContractVersion
├── RuleSetRequestPayload
├── NarrativeReason
└── PresentationHint
```

## 26. Rule Set-Neutral Roll Contract

The Core payload does not assume:

- d10;
- one Dice pool;
- one Dice type;
- one target number;
- success counting;
- one specialty model;
- one modifier model.

Complex mechanics belong to the selected Rule Set's versioned request payload.

## 27. Future Dice Compatibility

The Roll contract must permit future systems with:

- multiple Dice groups;
- multiple Dice sizes;
- mixed or custom Dice;
- specialties;
- modifiers;
- rerolls;
- exploding and chained Dice;
- keep/drop rules;
- custom symbols;
- opposed Rolls;
- staged Rolls;
- resource spending;
- post-Roll player decisions.

The MVP does not need to implement all these mechanics.

The Core must only avoid making them impossible.

## 28. Roll Validation

Chronicle validates:

- actor and target;
- active Campaign and Scene;
- selected Rule Set package;
- exact package version;
- operation key;
- payload schema;
- Preferences;
- permissions;
- current authoritative state.

## 29. Narrator Does Not Generate Dice

A `roll.requested` event contains no raw Dice outcomes.

Chronicle owns random generation.

The Rule Set owns mechanical interpretation.

## 30. Roll Stop Boundary

A valid unresolved Roll normally produces:

```text
CompletionStatus = Partial
StopReason = AwaitingDiceResolution
BlockingBehavior = AwaitChronicleResolution
```

The Narrator must not narrate beyond the unresolved outcome.

## 31. Character Entry Proposed

`character.entry-proposed` proposes that a Character becomes present in the active Scene.

The payload may reference an existing Character or provide a bounded new-Character draft.

Chronicle validates and persists any new Character.

## 32. Character Exit Proposed

`character.exit-proposed` proposes that a Character leaves the active Scene.

Chronicle validates current presence, Scene context, and relevant rules.

## 33. Knowledge Acquisition Proposed

`knowledge.acquisition-proposed` proposes a Character Knowledge change.

Recommended payload:

```text
KnowerCharacterId
KnowledgeSubject
KnowledgeContent
AcquisitionBasis
ConfidenceHint
Visibility
```

This event does not alter Campaign truth.

## 34. Immediate State Change Proposed

`immediate-state-change-proposed` proposes a bounded mutation through an approved Chronicle or Rule Set operation.

Recommended payload:

```text
TargetReference
StateChangeContractKey
StateChangeContractVersion
RuleSetPayload
NarrativeBasis
```

Arbitrary unregistered state mutation is forbidden.

## 35. Scene Completion Proposed

`scene.completion-proposed` signals that the active Scene may have reached a completion boundary.

Chronicle decides whether completion is valid.

## 36. Scene Transition Proposed

`scene.transition-proposed` proposes moving to another Scene context.

Recommended payload:

```text
CurrentSceneId
TransitionReason
TargetSceneReference
NewSceneDraft
ActReference
```

The Narrator cannot create or activate a Scene directly.

## 37. Plan Revision Suggested

`plan.revision-suggested` tells Chronicle Director that current planning assumptions may need revision.

It does not mutate Campaign truth.

## 38. No State Change

`no-state-change` explicitly declares that the output proposes no structured mutation.

In the initial contract, it cannot coexist with a mutation-proposing event.

## 39. Campaign Memory Policy

The Narrator does not persist Campaign Memories.

A future memory signal must use a separately registered event and remain subject to Chronicle or Archivist review.

## 40. Structured Choice Policy

Durable structured Choice is outside the MVP.

Ordinary questions are asked through prose and answered through normal player input.

The canonical output has no top-level `ProposedChoice`.

## 41. Future Choice Event

A future structured Choice must use:

```text
choice.offered
```

Its implementation requires:

- use-case specification;
- persistence;
- lifecycle state machine;
- restart recovery;
- UI;
- accessibility;
- tests.

## 42. Continuity Claims

`ContinuityClaims` expose assertions relied upon or introduced by the Narrator.

Recommended shape:

```text
ContinuityClaim
├── ClaimId
├── ClaimType
├── SubjectReference
├── PredicateKey
├── Value
├── Confidence
└── SourceReferences[]
```

Claims remain advisory until Chronicle validates them.

## 43. Continuity Conflict

Chronicle may:

- reject the entire output;
- reject affected events;
- accept safe prose with warnings;
- request repair;
- defer the claim.

Partial acceptance is allowed only when narrative causality remains coherent.

## 44. Warnings

Warnings contain provider-declared concerns.

Recommended shape:

```text
WarningCode
Message
RelatedEventIds[]
RelatedBlockIds[]
SeverityHint
```

Chronicle independently determines severity and recovery.

## 45. Provider Metadata Reference

The response may carry an opaque reference to safe provider-attempt metadata owned by Chronicle.

It must not contain:

- credentials;
- request headers;
- full prompt;
- unrestricted raw response;
- required remote provider-thread state.

## 46. Stop Reason

Initial Stop Reasons are:

```text
TurnComplete
AwaitingDiceResolution
AwaitingPlayerInput
AwaitingChronicleDecision
ContextInsufficient
AuthorityBoundaryReached
ProviderConstraint
ValidationRepairRequired
```

The Stop Reason must agree with CompletionStatus and blocking events.

## 47. Awaiting Player Input

For the MVP, this normally means the prose asks the player to respond freely.

It does not create a durable Choice entity.

## 48. Event Acceptance Results

Chronicle may classify each event as:

```text
Accepted
Rejected
Normalized
Deferred
RequiresRepair
```

## 49. Partial Acceptance

Chronicle may accept valid prose and reject an event only when:

- the prose does not claim the rejected consequence already happened;
- ordering remains coherent;
- no authority boundary is crossed;
- user interpretation will not be misleading.

Otherwise the turn is repaired or rejected atomically.

## 50. Validation Layers

Validation includes:

```text
Schema
Contract Version
Narrative Turn Identity
Sequence
References
Campaign Context
Authority
Rule Set
Continuity
Blocking Semantics
Safety
Privacy
Size and Complexity
```

## 51. Unknown Event Type

Unknown event types are rejected.

## 52. Unknown Fields

The initial authoritative event-payload policy is strict rejection of unknown fields unless a specific contract version allows safe preservation.

## 53. Repair

Chronicle may request bounded provider repair.

Repair instructions may identify:

- malformed schema;
- unknown event;
- invalid reference;
- authority violation;
- missing field;
- sequence conflict;
- stop-reason mismatch;
- prose after blocking boundary.

## 54. Repair Identity

Repairs remain under the same NarrativeTurnId.

Only one accepted output may become authoritative for a given stage of the turn.

## 55. Duplicate Responses

Duplicate or retried provider responses must not duplicate:

- Messages;
- Dice Rolls;
- Character entries;
- state changes;
- Scene transitions;
- knowledge records.

Chronicle uses OperationIds and event correlation.

## 56. Late Responses

A response arriving after the turn has advanced, completed, or been superseded is rejected as stale.

## 57. Streaming

Provider token streaming is nonauthoritative.

The MVP may buffer the complete response and validate it before displaying accepted prose.

Future provisional streaming requires a separate Presentation and authority policy.

## 58. Message Mapping

Accepted Narrative Blocks become Chronicle Messages according to the persistence policy.

Mapping preserves:

- sequence;
- speaker;
- Scene;
- NarrativeTurnId;
- accepted time;
- correction history.

## 59. Event Mapping

Accepted Structured Events are routed into approved Application workflows.

The event router does not write arbitrary persistence directly.

## 60. Event Router

Handlers are registered by:

```text
EventTypeKey
EventContractVersion
```

Each handler declares:

- authority;
- validator;
- idempotency;
- persistence behavior;
- blocking behavior;
- recovery behavior;
- tests.

## 61. No Arbitrary Dynamic Dispatch

Unregistered event or payload types cannot execute through reflection, provider naming, or package-supplied arbitrary code.

## 62. Event Correlation

Application evidence retains:

```text
NarrativeTurnId
EventId
OperationId
ResultingRecordIds
Outcome
```

## 63. Roll Correlation

An accepted `roll.requested` event creates or references a Chronicle-owned DiceRollId.

The continuation Narrative Turn references the resolved DiceRollId.

## 64. Language

Semantic keys, contract names, errors, and base documentation use English.

Generated narrative language follows Campaign Preference or user instruction.

Localized labels never replace semantic keys.

## 65. Rule Set Independence

No event or Core payload assumes Werewolf.

Werewolf is the first Rule Set package, not the shape of Chronicle Narrative Intelligence.

Future packages use the same event envelope and provide their own versioned mechanical payloads.

## 66. Package Extensions

A Rule Set package may provide a typed payload under an approved generic event.

A package cannot introduce a new global event lifecycle without Chronicle registration.

## 67. Limits

Chronicle enforces limits on:

- output bytes;
- block count;
- event count;
- block length;
- payload depth;
- collection sizes;
- claim count;
- warning count;
- validation time.

## 68. Error Model

Recommended errors:

```text
narrator-output.contract-invalid
narrator-output.version-unsupported
narrator-output.turn-id-mismatch
narrator-output.sequence-invalid
narrator-output.event-type-unknown
narrator-output.event-payload-invalid
narrator-output.authority-violation
narrator-output.reference-invalid
narrator-output.rule-set-validation-failed
narrator-output.continuity-conflict
narrator-output.stop-reason-invalid
narrator-output.block-after-boundary
narrator-output.size-limit-exceeded
narrator-output.repair-exhausted
narrator-output.superseded
```

## 69. Data Preservation State

Results should state:

```text
CampaignStateUnchanged
NoNarrativeAccepted
NarrativeBlocksAccepted
StructuredEventsAccepted
RepairPending
TurnSuperseded
RollRequestCreated
AwaitingPlayerInput
AwaitingDiceResolution
```

## 70. Logging

Safe logs may contain:

- NarrativeTurnId;
- provider attempt ID;
- contract version;
- block count;
- event count;
- event type keys;
- Stop Reason;
- validation result;
- repair count;
- duration.

They must not contain full prose by default, credentials, full prompts, raw provider responses, or unrestricted Campaign context.

## 71. Metrics

Useful local metrics include:

```text
NarratorOutputValidationDuration
NarratorOutputInvalidCount
UnknownEventTypeCount
AuthorityViolationCount
RepairAttemptCount
RepairSuccessCount
RollRequestEventCount
ContinuityConflictCount
```

No remote telemetry is required.

## 72. Testing Strategy

The implementation requires:

```text
Envelope Tests
Narrative Block Tests
Event Registry Tests
Ordering Tests
Authority Tests
Rule Set Payload Tests
Roll Boundary Tests
Repair Tests
Idempotency Tests
Provider Adapter Contract Tests
Cross-System Tests
Privacy Tests
```

## 73. Envelope Tests

Tests must cover:

- complete prose-only turn;
- partial turn;
- invalid version;
- mismatched NarrativeTurnId;
- warnings;
- continuity claims;
- blocked output;
- invalid output.

## 74. Narrative Block Tests

Tests must cover:

- narration;
- dialogue;
- speaker reference;
- Scene reference;
- ordering;
- size limits;
- prose attempting hidden mutation.

## 75. Event Registry Tests

Every initial event type requires positive and negative tests.

Unknown events must fail.

## 76. Roll Tests

Tests must cover:

- valid simple Roll request;
- invalid actor;
- missing package;
- invalid operation;
- Dice values illegally included;
- prose after unresolved Roll;
- duplicate Roll event;
- restart correlation.

## 77. Cross-System Dice Test

A synthetic non-Werewolf Rule Set must validate a Roll request describing:

- several Dice groups;
- mixed Dice sizes;
- specialty metadata;
- modifiers;
- reroll capability;
- exploding Dice capability;
- keep/drop capability;
- opposed-resolution metadata;
- possible post-Roll decision.

This proves extensibility without making those mechanics MVP features.

## 78. Character Event Tests

Tests must cover:

- existing Character entry;
- new Character draft;
- exit;
- invalid Scene;
- unknown Character;
- duplicate presence.

## 79. Knowledge Tests

Tests must preserve the separation between Character Knowledge and Campaign truth.

## 80. State Change Tests

Tests must reject arbitrary unregistered mutation.

## 81. Scene Tests

Tests must cover Scene completion and transition proposals.

## 82. Choice Tests

MVP tests must prove:

- no top-level `ProposedChoice`;
- ordinary questions remain prose;
- `choice.offered` is rejected unless a future contract explicitly enables it.

## 83. Ordering Tests

Tests must cover:

- duplicate sequence;
- invalid sequence;
- event after hard boundary;
- prose after hard boundary;
- valid block-event interleaving.

## 84. Repair Tests

Tests must cover:

- malformed schema;
- unknown event;
- authority violation;
- successful repair;
- repair exhaustion;
- stale repair response.

## 85. Idempotency Tests

One Narrative Turn must not create duplicate effects under retry or duplicate delivery.

## 86. Privacy Tests

Secret and hidden-context canaries must not appear in accepted output or Stable logs.

## 87. Architecture Tests

Architecture tests must reject:

- fixed top-level proposal fields;
- arbitrary provider event keys;
- provider-generated Dice values;
- direct persistence from provider DTOs;
- d10-specific fields in Core event contracts;
- localized semantic event keys;
- unversioned payloads;
- event handlers without declared authority.

## 88. Prohibited Patterns

### 88.1 Two Competing Root Contracts

Use only `NarratorTurnOutput`.

### 88.2 Generic Consequence Blob

Use typed Structured Events.

### 88.3 Provider-Defined Event Semantics

Use the closed registry.

### 88.4 Narration Past an Unresolved Roll

Stop at the boundary.

### 88.5 Prose as State Mutation

Require an event.

### 88.6 Dice Results in Narrator Output

Chronicle generates Dice.

### 88.7 Werewolf-Specific Core Payloads

Use Rule Set contracts.

### 88.8 Structured Choice Without Full Lifecycle Design

Keep free-text questions in the MVP.

### 88.9 Trust Provider Authority Labels

Chronicle validates independently.

### 88.10 Treat Streaming Tokens as Committed Content

Validate the completed contract first.

## 89. Alternatives Considered

### Fixed Top-Level Fields

Rejected because each new proposal changes the root schema and creates invalid combinations of optional fields.

### Free-Form Event Dictionary

Rejected because arbitrary semantics cannot be validated safely.

### Prose-Only Output

Rejected because Roll requests and state proposals require machine-readable authority boundaries.

### Provider Tool Calls as Canonical Contract

Rejected because provider tools are adapter-specific and do not define Chronicle authority.

### One Event per Turn

Rejected because a turn may contain several ordered nonblocking proposals before one blocking boundary.

### Structured Choice in MVP

Rejected because free-text input already satisfies the immediate product need.

## 90. Consequences

### Positive

- one canonical output contract;
- provider neutrality;
- typed validation;
- deterministic ordering;
- explicit Roll interruption;
- no hidden state mutation in prose;
- future event extensibility;
- support for complex future Dice systems;
- no premature Choice subsystem;
- clean mapping to persistence and Application handlers.

### Negative

- event registry governance is required;
- payload schemas require maintenance;
- prompts must teach blocking boundaries;
- partial acceptance is complex;
- repair may be more frequent than with loose output;
- future event types require explicit design.

## 91. Risks

### Event Registry Becomes Too Broad

Mitigation:

- generic lifecycle events;
- Rule Set-specific payloads;
- explicit review.

### Payload Becomes an Untyped Blob

Mitigation:

- contract keys;
- schemas;
- package validation;
- size limits.

### Rejected Event Contradicts Accepted Prose

Mitigation:

- causal validation;
- atomic rejection where necessary;
- repair.

### Future Dice Exceeds Current Assumptions

Mitigation:

- generic envelope;
- versioned package payload;
- synthetic cross-system tests;
- no d10 assumptions.

### Structured Choice Is Needed Earlier

Mitigation:

- reserved future event;
- separate decision;
- no need to change the root envelope.

## 92. Technology Spike

Before acceptance, implement:

1. `NarratorTurnOutput`;
2. Narrative Block DTO;
3. Narrative Event envelope;
4. event registry;
5. initial event payload schemas;
6. sequence validator;
7. authority validator;
8. Roll boundary validator;
9. event router;
10. repair contract;
11. provider adapter contract test;
12. complex synthetic Rule Set Roll payload;
13. OperationId correlation;
14. privacy canary tests.

## 93. Spike Acceptance

The spike passes when:

- a prose-only turn validates;
- a Character entry plus Scene completion validates;
- a Roll request stops correctly;
- prose after the Roll is rejected;
- unknown events fail;
- authority violations fail;
- malformed output can be repaired;
- duplicate delivery creates no duplicate effect;
- a complex non-Werewolf Roll payload validates;
- no fixed top-level proposal field exists;
- no structured Choice subsystem is required.

## 94. Definition of Compliance

An implementation complies when:

- every Narrator response uses `NarratorTurnOutput`;
- prose uses `NarrativeBlocks[]`;
- nonprose proposals use `StructuredEvents[]`;
- event types are closed, registered, and versioned;
- event payloads are typed;
- ordering is deterministic;
- unresolved Rolls stop narration;
- Chronicle owns random generation;
- Rule Sets own mechanical interpretation;
- no Core contract assumes Werewolf or d10;
- ordinary MVP questions remain prose;
- accepted effects correlate to Chronicle operations;
- retries do not duplicate authoritative effects.

## 95. Review Triggers

Review this RFC if:

- structured Choice becomes required;
- provider-native tools become part of adapter transport;
- provisional streaming becomes visible;
- packages may introduce global event types;
- multiplayer introduces recipient-specific events;
- voice or multimodal output is added;
- Dice request contracts change materially;
- autonomous world simulation is introduced.

## 96. Deferred Decisions

Later decisions may define:

- `choice.offered`;
- relationship-change events;
- memory-signal events;
- voice and audio blocks;
- image proposal events;
- provisional streaming;
- package event namespaces;
- multiplayer targeting;
- event deprecation policy.

## 97. Final Decision

Chronicle will use one Narrator output contract:

```text
NarratorTurnOutput
    with
NarrativeBlocks[]
    and
StructuredEvents[]
```

Prose will describe.

Structured Events will propose.

Chronicle and the selected Rule Set will validate.

A Roll request will stop narration before the unresolved outcome.

Future RPG systems may use substantially more complex Dice mechanics without changing the root Narrator contract.

Werewolf is one package.

It is not the shape of Narrative Intelligence.
