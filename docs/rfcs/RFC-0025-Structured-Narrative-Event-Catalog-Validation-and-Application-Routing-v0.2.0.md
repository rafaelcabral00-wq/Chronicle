---
id: RFC-0025
title: Structured Narrative Event Catalog, Validation, and Application Routing
status: Draft
version: 0.2.0
owner: Chronicle Team
last_updated: 2026-08-02
category: Narrative Intelligence
supersedes:
  - RFC-0025@0.1.0
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
implements: []
related_to:
  - ADR-0010
  - ADR-0033
  - ADR-0034
  - ADR-0035
  - ADR-0036
  - ADR-0040
---

> **"A structured event is not an instruction from the model. It is a typed request for Chronicle to evaluate."**

# Structured Narrative Event Catalog, Validation, and Application Routing

## 1. Status

**Draft**

This RFC defines the canonical Structured Event model used inside `NarratorTurnOutput`.

This revision aligns fully with RFC-0021 version 0.2.0.

The decision is:

- use `StructuredEvents[]` as the only nonprose proposal channel in Narrator output;
- define a closed event catalog;
- version each event payload independently;
- route accepted events through registered Application handlers;
- prohibit direct persistence from provider DTOs;
- require authority, idempotency, ordering, validation, recovery, and test metadata for every event type;
- keep event semantics system-agnostic;
- allow Rule Set-specific typed payloads only inside approved generic event envelopes;
- keep durable structured Choice outside MVP;
- preserve future complex Dice support without adding those mechanics to the MVP;
- use English semantic keys.

## 2. Purpose

Narrative Intelligence needs a bounded way to propose effects without gaining authority over Chronicle state.

A Structured Event must answer:

- what is being proposed;
- which contract version defines the payload;
- what authority must validate it;
- whether narration must stop;
- which records it references;
- how retries avoid duplication;
- what Application workflow handles it;
- what recovery behavior applies.

## 3. Canonical Event Envelope

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

## 4. Event Identity

`EventId` is turn-local provider output identity.

Chronicle may map it to a canonical internal event-application identity.

## 5. Sequence

Every event has a deterministic sequence.

Sequence is validated relative to Narrative Blocks and other events.

## 6. Event Type Key

`EventTypeKey` is an invariant English semantic key.

Examples:

```text
roll.requested
character.entry-proposed
scene.transition-proposed
```

## 7. Closed Registry

The provider may only emit registered event types.

Unknown keys fail validation.

## 8. Contract Version

Every event type has an independent payload version.

## 9. Typed Payload

The payload must satisfy the schema registered for:

```text
EventTypeKey
EventContractVersion
```

## 10. Correlation References

Events may reference:

- NarrativeTurnId;
- CharacterId;
- SceneId;
- MessageId;
- DiceRollId;
- Rule Operation key;
- another event in the same turn.

## 11. Required Authority

Initial values:

```text
ChronicleApplication
RuleSet
User
Archivist
None
```

The field declares who must validate or complete the proposal.

## 12. Blocking Behavior

Initial values:

```text
NonBlocking
StopAfterEvent
AwaitChronicleResolution
AwaitUserInput
```

## 13. Source Block References

An event may reference prose blocks that introduced or motivated it.

This supports contradiction detection and repair.

## 14. Initial Event Catalog

The MVP catalog is:

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

## 15. Catalog Governance

Every event registration must define:

- semantic meaning;
- payload schema;
- authority;
- blocking behavior;
- allowed contexts;
- idempotency;
- Application handler;
- persistence effect;
- recovery behavior;
- error keys;
- test suite;
- deprecation policy.

## 16. Roll Requested

`roll.requested` proposes a Rule Set-governed Roll.

Recommended payload:

```text
ActorReference
TargetReference
Intent
RuleOperationKey
RollContractKey
RollContractVersion
RuleSetRequestPayload
NarrativeReason
PresentationHint
```

## 17. Dice Neutrality

The generic event does not assume:

- d10;
- one Dice pool;
- one Dice size;
- success counting;
- one specialty model;
- one modifier model.

## 18. Future Dice Systems

The Rule Set payload must be capable of describing future systems with:

- multiple Dice groups;
- mixed Dice sizes;
- specialties;
- modifiers;
- rerolls;
- exploding Dice;
- chained Dice;
- keep/drop rules;
- custom symbols;
- opposed Rolls;
- staged Rolls;
- resource spending;
- post-Roll choices.

The Core supports the contract shape without implementing every mechanic in MVP.

## 19. Dice Authority

The Narrator proposes the Roll.

Chronicle generates random evidence.

The Rule Set resolves mechanics.

## 20. Roll Boundary

A valid unresolved Roll uses:

```text
BlockingBehavior = AwaitChronicleResolution
```

No prose or event may claim its result before resolution.

## 21. Character Entry Proposed

Proposes that a Character becomes present in the active Scene.

Payload may contain:

```text
ExistingCharacterId
NewCharacterDraft
SceneId
EntryReason
Visibility
```

Only one Character source is valid.

## 22. Character Exit Proposed

Proposes that a present Character leaves the active Scene.

Chronicle validates presence and Scene context.

## 23. Knowledge Acquisition Proposed

Proposes a Character Knowledge mutation.

Recommended payload:

```text
KnowerCharacterId
KnowledgeSubject
KnowledgeContent
AcquisitionBasis
ConfidenceHint
Visibility
```

It does not change Campaign truth.

## 24. Immediate State Change Proposed

Proposes a bounded mutation through an approved Chronicle or Rule Set operation.

Recommended payload:

```text
TargetReference
StateChangeContractKey
StateChangeContractVersion
RuleSetPayload
NarrativeBasis
```

Arbitrary object patches are prohibited.

## 25. Scene Completion Proposed

Signals that the active Scene may be complete.

Chronicle validates closure.

## 26. Scene Transition Proposed

Proposes a transition to another Scene.

Recommended payload:

```text
CurrentSceneId
TransitionReason
TargetSceneId
NewSceneDraft
ActReference
```

Chronicle creates or activates the Scene.

## 27. Plan Revision Suggested

Advises Chronicle Director that planning assumptions may need revision.

It does not mutate Campaign truth.

## 28. No State Change

Declares that the turn proposes no structured mutation.

In the initial contract it cannot coexist with a mutation-proposing event.

## 29. Structured Choice

`choice.offered` is reserved but disabled in MVP.

Ordinary questions remain prose.

Enabling structured Choice requires a separate lifecycle and persistence decision.

## 30. Future Events

Potential future events include:

```text
choice.offered
relationship.change-proposed
condition.change-proposed
resource.spend-proposed
memory.signal-proposed
```

They are not active merely because their names are reserved.

## 31. Package-Specific Payloads

A Rule Set may define a typed payload inside an approved generic event.

It cannot define an arbitrary new global lifecycle without Chronicle registration.

## 32. Event Validation Pipeline

```text
Envelope Validation
    ↓
Registry Lookup
    ↓
Payload Schema Validation
    ↓
Reference Validation
    ↓
Context Validation
    ↓
Authority Validation
    ↓
Rule Set Validation
    ↓
Ordering and Blocking Validation
    ↓
Continuity Validation
    ↓
Application Routing
```

## 33. Envelope Validation

Checks:

- required fields;
- supported values;
- sequence presence;
- valid EventId;
- payload size.

## 34. Registry Lookup

Unknown event type or version fails.

## 35. Payload Validation

Payload must match the registered typed schema.

## 36. Reference Validation

All references must be:

- authoritative;
- valid drafts allowed by contract;
- scoped to the current Campaign;
- not stale.

## 37. Context Validation

Chronicle validates:

- Campaign;
- Session;
- Act;
- Scene;
- current Narrative Turn;
- pending Dice or recovery state.

## 38. Authority Validation

The event may not bypass:

- Chronicle Application;
- Rule Set;
- player;
- Archivist;
- security policy.

## 39. Rule Set Validation

Mechanical payloads are validated through the exact selected package version.

## 40. Ordering Validation

Chronicle rejects:

- duplicate sequence;
- invalid sequence;
- event after a hard unresolved boundary;
- prose that narrates a rejected consequence as completed.

## 41. Continuity Validation

The proposal is checked against selected authoritative history.

## 42. Event Application Result

Recommended values:

```text
Accepted
Rejected
Normalized
Deferred
RequiresRepair
```

## 43. Normalized

Chronicle may normalize representation without changing meaning.

Example:

- canonical identifier formatting;
- bounded display hint;
- normalized enum alias.

## 44. Deferred

An event may be valid but require later user or Application action.

## 45. Requires Repair

Used when the provider can safely correct a malformed or contradictory event.

## 46. Partial Acceptance

Partial acceptance is allowed only when:

- accepted prose remains truthful;
- rejected events are not already narrated as fact;
- no causal gap is created;
- blocking semantics remain valid.

Otherwise the turn is rejected or repaired atomically.

## 47. Event Router

Chronicle routes events through a registered Application event router.

## 48. Router Key

```text
EventTypeKey
EventContractVersion
```

## 49. Handler Contract

Each handler declares:

```text
Authority
Validator
OperationId Strategy
Persistence Effects
Blocking Result
Recovery Behavior
Error Mapping
```

## 50. No Direct Provider Persistence

Provider DTOs never write repositories or DbContext directly.

## 51. Application Commands

Accepted events become approved Application commands or workflow requests.

## 52. Operation Identity

Each accepted event effect uses a Chronicle OperationId.

## 53. Idempotency

Retrying or redelivering the same accepted event must not duplicate authoritative effects.

## 54. Correlation Record

Chronicle records:

```text
NarrativeTurnId
EventId
OperationId
HandlerKey
Outcome
ResultingRecordIds
```

## 55. Roll Correlation

A Roll request creates or references a Chronicle-owned DiceRollId.

Narrative continuation references that DiceRollId after resolution.

## 56. Failure Before Commit

The event remains unapplied.

## 57. Unknown Commit

Chronicle recovers through OperationId and authoritative queries.

## 58. Failure After Commit

Retry returns the committed result.

## 59. Blocking Result

A handler returns whether the Narrative Turn:

- continues;
- stops for Dice;
- waits for player input;
- waits for Chronicle;
- completes;
- requires repair.

## 60. Error Model

Recommended errors:

```text
narrative-event.type-unknown
narrative-event.version-unsupported
narrative-event.payload-invalid
narrative-event.reference-invalid
narrative-event.context-invalid
narrative-event.authority-violation
narrative-event.rule-set-validation-failed
narrative-event.order-invalid
narrative-event.blocking-conflict
narrative-event.continuity-conflict
narrative-event.handler-missing
narrative-event.duplicate
narrative-event.repair-required
narrative-event.superseded
```

## 61. Data Preservation State

Results should state:

```text
CampaignStateUnchanged
EventNotApplied
EventApplied
EventDeferred
RepairPending
RollCreated
AwaitingPlayerInput
NarrativeTurnStopped
```

## 62. Logging

Safe logs may include:

- NarrativeTurnId;
- EventId;
- event key;
- contract version;
- handler key;
- outcome;
- OperationId;
- duration.

They must not contain credentials, full payloads by default, private prose, raw provider responses, or unrestricted Campaign data.

## 63. Metrics

Useful local metrics include:

```text
NarrativeEventValidationDuration
UnknownEventCount
AuthorityViolationCount
EventRepairCount
EventApplicationFailureCount
DuplicateEventCount
RollRequestedCount
```

## 64. Language

All event keys, contract keys, errors, and base documentation use English.

Localized UI labels remain separate.

## 65. Rule Set Independence

No Core event contract is specific to Werewolf.

Werewolf is the first package implementation only.

## 66. Testing Strategy

The implementation requires:

```text
Registry Tests
Schema Tests
Reference Tests
Authority Tests
Ordering Tests
Handler Tests
Idempotency Tests
Recovery Tests
Rule Set Contract Tests
Cross-System Dice Tests
Privacy Tests
```

## 67. Catalog Tests

Every active event type requires:

- valid example;
- invalid payload;
- invalid reference;
- invalid context;
- authority violation;
- handler test;
- retry test.

## 68. Roll Event Tests

Tests must cover:

- simple valid Roll;
- complex synthetic Roll;
- invalid package;
- invalid operation;
- provider-supplied Dice values;
- narration after unresolved Roll;
- duplicate request;
- continuation correlation.

## 69. Cross-System Dice Test

A synthetic package unrelated to Werewolf must validate a request containing:

- several Dice groups;
- mixed Dice sizes;
- specialty metadata;
- modifiers;
- reroll capability;
- exploding Dice capability;
- keep/drop capability;
- opposed-resolution metadata;
- post-Roll decision metadata.

## 70. Character Event Tests

Tests cover known Character entry, new draft, exit, invalid Scene, unknown Character, and duplicate presence.

## 71. Knowledge Event Tests

Tests ensure Character Knowledge does not become Campaign truth.

## 72. State Change Tests

Arbitrary property patches and unknown state contracts fail.

## 73. Scene Event Tests

Completion and transition remain proposals until Chronicle accepts them.

## 74. Choice Tests

MVP tests prove `choice.offered` is inactive and ordinary questions remain prose.

## 75. Ordering Tests

Tests cover valid interleaving, duplicate sequence, event after boundary, and prose-event contradiction.

## 76. Idempotency Tests

Duplicate delivery creates no duplicate:

- Dice Roll;
- Character;
- knowledge record;
- state mutation;
- Scene transition.

## 77. Recovery Tests

Tests cover failure before commit, unknown commit, failure after commit, restart, and superseded turn.

## 78. Architecture Tests

Architecture tests must reject:

- arbitrary event keys;
- unversioned payloads;
- provider DTO repository access;
- direct DbContext writes;
- d10-specific Core event fields;
- event handler without declared authority;
- fixed top-level proposal fields;
- active structured Choice without lifecycle specification.

## 79. Prohibited Patterns

### 79.1 Event as Command Authority

An event is a proposal, not an instruction.

### 79.2 Arbitrary Provider Event Type

Use the closed registry.

### 79.3 Untyped Payload Blob

Use a registered versioned schema.

### 79.4 Direct Persistence

Route through Application handlers.

### 79.5 Hidden Rule Set Assumptions

Use package payloads.

### 79.6 Roll Results from Provider

Chronicle generates random evidence.

### 79.7 Duplicate Event Effects

Use OperationId correlation.

### 79.8 Event After Hard Boundary

Stop the turn.

### 79.9 Structured Choice Without Full Design

Keep it disabled in MVP.

### 79.10 Localized Semantic Keys

Use invariant English keys.

## 80. Alternatives Considered

### Fixed Root Fields

Rejected because the root contract expands for every new proposal type.

### Free-Form Consequence List

Rejected because it cannot be validated safely.

### Provider Tool Calls as Events

Rejected as the canonical contract because provider tools are adapter-specific.

### Package-Defined Arbitrary Event Types

Rejected because Chronicle must know authority and lifecycle semantics.

### One Generic State Patch Event

Rejected because arbitrary mutation would bypass Domain and Rule Set boundaries.

## 81. Consequences

### Positive

- one event model;
- bounded authority;
- deterministic routing;
- independent payload versioning;
- strong idempotency;
- clear recovery;
- system-neutral mechanics;
- future Dice extensibility;
- no premature Choice subsystem.

### Negative

- registry governance is required;
- every event needs a schema and handler;
- validation is more complex;
- package payloads require version management;
- partial acceptance needs careful causal analysis.

## 82. Risks

### Catalog Proliferation

Mitigation:

- generic lifecycle events;
- package-specific payloads;
- review.

### Payload Opaqueness

Mitigation:

- schemas;
- contract registry;
- package test kit;
- limits.

### Provider Emits Semantically Invalid Event

Mitigation:

- independent validation;
- repair;
- rejection.

### Future System Needs New Lifecycle

Mitigation:

- explicit new event proposal;
- no arbitrary extension.

## 83. Technology Spike

Before acceptance, implement:

1. event registry;
2. event envelope;
3. schemas for initial catalog;
4. validator pipeline;
5. Application router;
6. handler metadata;
7. OperationId correlation;
8. Roll handler;
9. Character and Scene handlers;
10. repair mapping;
11. synthetic complex Rule Set payload;
12. privacy tests;
13. architecture tests.

## 84. Spike Acceptance

The spike passes when:

- every initial event validates and routes;
- unknown events fail;
- invalid authority fails;
- Roll stops the turn;
- provider-supplied Dice values fail;
- duplicate event delivery is idempotent;
- a complex non-Werewolf Roll payload validates;
- no fixed top-level proposal field remains;
- structured Choice remains disabled.

## 85. Definition of Compliance

An implementation complies when:

- all nonprose Narrator proposals use registered Structured Events;
- payloads are typed and versioned;
- events are proposals;
- Chronicle validates authority and context;
- Rule Sets validate mechanics;
- Application handlers own effects;
- OperationIds prevent duplicates;
- unresolved Rolls stop the turn;
- no Core event assumes Werewolf or one Dice model;
- ordinary MVP questions remain prose;
- unknown events and payloads are rejected safely.

## 86. Review Triggers

Review this RFC if:

- structured Choice becomes required;
- packages may introduce global event types;
- multiplayer adds recipient-specific event routing;
- provisional streaming affects event order;
- multimodal events are introduced;
- autonomous world simulation is introduced;
- Dice lifecycle changes materially.

## 87. Deferred Decisions

Later decisions may define:

- `choice.offered`;
- relationship events;
- condition events;
- memory signals;
- multiplayer targeting;
- event deprecation;
- package event namespaces;
- multimodal proposals.

## 88. Final Decision

Chronicle will use a closed, versioned Structured Event catalog.

Narrative Intelligence may propose events.

It may not invent authority.

Chronicle will validate context, identity, ordering, and lifecycle.

Rule Sets will validate mechanics.

Application handlers will own effects.

Future RPG systems may bring radically different Dice and resolution models without changing the event envelope.

Werewolf is one implementation.

It is not the event architecture.
