---
id: RFC-0012
title: Dice Requests, Player Release, Random Evidence, Resolution, and Canonical Lifecycle
status: Draft
version: 0.2.0
owner: Chronicle Team
last_updated: 2026-08-02
category: Domain and Application
supersedes:
  - RFC-0012@0.1.0
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
implements: []
related_to:
  - RFC-0021
  - RFC-0025
  - RFC-0033
  - ADR-0013
  - ADR-0014
  - ADR-0033
  - ADR-0034
  - ADR-0040
  - ADR-0041
---

> **"The Narrator may request uncertainty. The player releases it. Chronicle generates the evidence. The Rule Set interprets it."**

# Dice Requests, Player Release, Random Evidence, Resolution, and Canonical Lifecycle

## 1. Status

**Draft**

This RFC defines Chronicle's canonical Dice workflow.

This revision resolves the lifecycle conflict between the earlier Dice RFCs and ADR-0033.

The canonical states are:

```text
Proposed
Validated
AwaitingPlayerRelease
Generating
RawValuesCommitted
Resolving
AwaitingAdditionalDice
Resolved
AwaitingNarrativeContinuation
Completed
Cancelled
Superseded
Invalidated
RecoveryRequired
FailedTerminal
```

The decision is:

- Narrative Intelligence may propose a Roll but never generate Dice outcomes;
- Chronicle validates the request;
- narration stops before the unresolved outcome;
- the player explicitly releases the Roll;
- Chronicle generates authoritative random evidence;
- raw values commit before mechanical interpretation or narrative continuation;
- the selected Rule Set resolves the Roll;
- complex systems may require additional Dice or post-generation decisions;
- resolution preserves all evidence and causal relationships;
- historical Rolls are never rerolled during retry, restore, import, migration, or continuation;
- one canonical lifecycle is used across Core, persistence, UI, and tests;
- Dice contracts remain system-agnostic;
- Rule Set packages define versioned request and resolution payloads;
- the MVP implements only the mechanics required by the first official Rule Set;
- Core remains ready for future systems with more complex Dice models;
- all semantic keys and base diagnostics use English.

## 2. Purpose

Dice are a critical authority boundary.

Chronicle must guarantee that:

- the provider cannot invent results;
- the player sees when uncertainty exists;
- the Roll waits for explicit release;
- random values persist durably;
- resolution uses the exact Rule Set package version;
- retries do not regenerate values;
- crashes do not lose or duplicate evidence;
- narrative continuation receives the committed result;
- future Rule Sets can use more complex mechanics without redesigning Core.

## 3. Scope

This RFC covers:

- Dice request;
- validation;
- player release;
- random evidence generation;
- persistence;
- Rule Set resolution;
- additional Dice;
- post-generation decisions;
- continuation;
- cancellation;
- invalidation;
- correction;
- recovery;
- generic contracts;
- UI boundaries;
- errors;
- tests.

## 4. Out of Scope

This RFC does not define:

- exact mechanics of one RPG system;
- visual Dice animation;
- physical Dice integration;
- cryptographic gambling-grade randomness;
- multiplayer Roll ownership;
- remote Dice services;
- provider-generated random values;
- exact UI framework.

## 5. Terminology

Chronicle uses:

```text
Roll Request
    the proposed mechanical uncertainty

Random Evidence
    committed raw generated outcomes

Resolution
    Rule Set interpretation of evidence

Outcome
    the typed mechanical result

Narrative Continuation
    narration produced after resolution
```

## 6. Dice Roll Identity

Every Roll has:

```text
DiceRollId
```

Chronicle creates it after accepting a valid proposal.

## 7. Dice Roll Record

Recommended shape:

```text
DiceRoll
├── DiceRollId
├── CampaignId
├── SessionId
├── ActId
├── SceneId
├── OriginNarrativeTurnId
├── OriginNarrativeEventId
├── ContinuationNarrativeTurnId
├── OperationId
├── RuleSetPackageId
├── RuleSetPackageVersion
├── RuleOperationKey
├── RollContractKey
├── RollContractVersion
├── State
├── RequestPayload
├── RandomEvidence[]
├── ResolutionPayload
├── OutcomeSummary
├── CreatedAtUtc
├── ReleasedAtUtc
├── RawValuesCommittedAtUtc
├── ResolvedAtUtc
├── CompletedAtUtc
├── RowVersion
└── FailureSummary
```

## 8. Authority Ownership

```text
Narrator
    proposes

Chronicle
    validates
    generates random evidence
    persists
    owns lifecycle

Rule Set
    validates mechanics
    resolves evidence

Player
    explicitly releases the Roll
```

## 9. Canonical State Machine

```text
Proposed
    ↓
Validated
    ↓
AwaitingPlayerRelease
    ↓
Generating
    ↓
RawValuesCommitted
    ↓
Resolving
    ↓
Resolved
    ↓
AwaitingNarrativeContinuation
    ↓
Completed
```

Alternative paths include:

```text
AwaitingAdditionalDice
Cancelled
Superseded
Invalidated
RecoveryRequired
FailedTerminal
```

## 10. Proposed

A Narrator event or another approved Application workflow proposed the Roll.

The request is not yet mechanically valid.

## 11. Validated

Chronicle and the selected Rule Set validated:

- actor;
- target;
- operation;
- request contract;
- Campaign context;
- Preferences;
- package binding;
- current state.

## 12. Awaiting Player Release

The Roll is visible and waits for explicit player action.

No random evidence exists yet.

## 13. Generating

Chronicle is generating random evidence.

This is a short, locally controlled operation.

## 14. Raw Values Committed

All generated evidence required for the current generation stage is durably committed.

The values must never be regenerated for the same stage.

## 15. Resolving

The exact Rule Set package is interpreting committed evidence.

## 16. Awaiting Additional Dice

The current evidence is valid but the Rule Set requires further generation or a post-generation decision.

Examples:

- exploding Dice;
- chained Dice;
- reroll selection;
- spend to add Dice;
- opposed second group;
- follow-up damage;
- keep/drop selection.

## 17. Resolved

The Rule Set produced a typed mechanical result and Chronicle committed it.

## 18. Awaiting Narrative Continuation

The resolved Roll is waiting for a correlated Narrator continuation.

## 19. Completed

All required Dice workflow work is complete, including continuation scheduling or accepted continuation according to policy.

## 20. Cancelled

The Roll ended before random evidence generation.

## 21. Superseded

A newer valid request or changed Campaign state made the Roll obsolete before execution.

## 22. Invalidated

The Roll or its prior resolution was later determined to be invalid through an explicit correction workflow.

Historical evidence remains preserved.

## 23. Recovery Required

Chronicle cannot determine the next safe state automatically.

## 24. Failed Terminal

The Roll cannot proceed under the same request without a new explicit action.

## 25. Terminal States

Terminal states are:

```text
Completed
Cancelled
Superseded
Invalidated
FailedTerminal
```

`RecoveryRequired` is not terminal.

## 26. Allowed Transitions

Canonical transitions include:

```text
Proposed → Validated
Proposed → Invalidated
Proposed → Cancelled
Proposed → Superseded

Validated → AwaitingPlayerRelease
Validated → Invalidated
Validated → Superseded
Validated → Cancelled

AwaitingPlayerRelease → Generating
AwaitingPlayerRelease → Cancelled
AwaitingPlayerRelease → Superseded
AwaitingPlayerRelease → Invalidated

Generating → RawValuesCommitted
Generating → RecoveryRequired
Generating → FailedTerminal

RawValuesCommitted → Resolving
RawValuesCommitted → RecoveryRequired

Resolving → Resolved
Resolving → AwaitingAdditionalDice
Resolving → RecoveryRequired
Resolving → FailedTerminal

AwaitingAdditionalDice → Generating
AwaitingAdditionalDice → Resolving
AwaitingAdditionalDice → Cancelled only if no committed authoritative stage would be erased
AwaitingAdditionalDice → RecoveryRequired
AwaitingAdditionalDice → FailedTerminal

Resolved → AwaitingNarrativeContinuation
Resolved → Completed
Resolved → Invalidated
Resolved → RecoveryRequired

AwaitingNarrativeContinuation → Completed
AwaitingNarrativeContinuation → RecoveryRequired
AwaitingNarrativeContinuation → Invalidated

RecoveryRequired → AwaitingPlayerRelease
RecoveryRequired → RawValuesCommitted
RecoveryRequired → Resolving
RecoveryRequired → AwaitingAdditionalDice
RecoveryRequired → Resolved
RecoveryRequired → AwaitingNarrativeContinuation
RecoveryRequired → Completed
RecoveryRequired → FailedTerminal
```

Exact Rule Set contracts may restrict this further.

## 27. Forbidden Transitions

Examples:

- `AwaitingPlayerRelease → Resolved`;
- `Proposed → Generating`;
- `RawValuesCommitted → AwaitingPlayerRelease`;
- `Resolved → Generating` for the same generation stage;
- `Completed → Generating`;
- `Cancelled → RawValuesCommitted`;
- `Invalidated → Completed`;
- retry that discards committed evidence.

## 28. State Registry

Chronicle maintains one canonical Dice state registry containing:

```text
State key
Meaning
Terminal status
Allowed predecessors
Allowed successors
Evidence requirement
User-visible meaning
Retry behavior
Cancellation behavior
Recovery behavior
```

## 29. Rule Set-Specific Substates

A Rule Set may persist typed phase detail inside its request or resolution payload.

It must not introduce a competing root Dice lifecycle.

## 30. Roll Proposal

The canonical Narrator event is:

```text
roll.requested
```

## 31. Proposal Payload

Recommended generic fields:

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

## 32. No Provider Dice Values

The proposal must not contain generated outcomes.

## 33. Proposal Validation

Chronicle validates structural and contextual rules.

The Rule Set validates mechanical meaning.

## 34. Validation Failure

An invalid proposal does not create an executable Roll.

It may produce:

- Narrator repair;
- user-visible error;
- terminal rejection;
- corrected proposal.

## 35. Exact Package Binding

Every Roll records the exact:

```text
RuleSetPackageId
RuleSetPackageVersion
RollContractVersion
```

used for validation and resolution.

## 36. No Silent Package Substitution

A newer or similar package version cannot reinterpret an existing Roll silently.

## 37. Player Release

The Roll button or equivalent action represents explicit release.

## 38. Release Authority

Only an allowed user action may move:

```text
AwaitingPlayerRelease → Generating
```

## 39. Release Idempotency

Repeated click, double click, retry, or UI reconnection creates only one generation operation.

## 40. Release Operation

The release uses a Chronicle OperationId.

## 41. No Automatic Release in MVP

The MVP does not release Dice automatically after a timer.

## 42. No Provider Release

Narrative Intelligence cannot trigger generation after proposing the Roll.

## 43. Generation Ownership

Chronicle's random service generates evidence.

## 44. Random Service Contract

The Core depends on an abstraction such as:

```text
IRandomEvidenceGenerator
```

## 45. Random Source

The first implementation may use an operating-system cryptographic random source or another approved local source.

## 46. Random Source Version

Generation evidence records a source-contract identifier, not secret internal state.

## 47. Generation Input

The generator receives a Rule Set-validated generation plan.

## 48. Generation Plan

A generation plan may include:

```text
GenerationStageKey
DiceGroups[]
RequestedByRuleSet
EvidenceContractVersion
```

## 49. Dice Group

Recommended generic shape:

```text
DiceGroup
├── GroupKey
├── DiceKindKey
├── FaceCount
├── Quantity
├── Labels[]
├── ModifierMetadata
└── RuleSetPayload
```

## 50. No Single-Pool Assumption

One Roll may have several groups.

## 51. No Single-Die-Type Assumption

Groups may use different Dice kinds and sizes.

## 52. Custom Dice

Future Rule Sets may define custom-faced Dice using a package contract.

## 53. Random Evidence Item

Recommended:

```text
RandomEvidenceItem
├── EvidenceItemId
├── GenerationStageKey
├── GroupKey
├── DieKey
├── DieOrdinal
├── DiceKindKey
├── FaceCount
├── RawFaceValue
├── RawSymbolValue
├── GenerationSequence
├── ParentEvidenceItemId
├── CauseKey
└── RandomSourceContract
```

Only applicable value fields are used for a given Dice kind.

## 54. Evidence Identity

Every generated item has stable identity.

## 55. Generation Sequence

Sequence preserves deterministic evidence order.

## 56. Parent Evidence

Exploding, chained, replacement, or rerolled Dice may reference the evidence item that caused them.

## 57. Cause Key

Examples:

```text
initial
reroll
explosion
chain
replacement
bonus
opposed
follow-up
```

Keys remain Rule Set-compatible and versioned where needed.

## 58. Raw Evidence Commit

Chronicle commits generated evidence before:

- revealing an authoritative mechanical result;
- asking the Rule Set to resolve beyond safe deterministic preparation;
- requesting narrative continuation.

## 59. Commit Before Reveal

The UI must not present generated values as final before they are durably committed.

## 60. Animation

Animation is outside the MVP.

Any future animation must:

- follow committed values;
- never generate or alter results;
- support reduced motion;
- be skippable;
- remain nonauthoritative.

## 61. Raw Evidence Immutability

Committed evidence is append-only.

It is not edited to represent rerolls or corrections.

## 62. Rerolls

A reroll creates new evidence linked to the prior item.

The prior value remains preserved.

## 63. Exploding Dice

Explosion creates additional evidence with a causal parent reference.

## 64. Keep and Drop

The resolution payload identifies which evidence contributed to the outcome.

Evidence is never deleted because it was dropped.

## 65. Specialties

Specialties belong to the Rule Set request and resolution contracts.

Core stores their typed evidence but does not define their meaning.

## 66. Modifiers

Modifiers are versioned Rule Set data.

They must be distinguishable from generated random evidence.

## 67. Opposed Rolls

Opposed resolution may use:

- multiple groups in one DiceRoll;
- linked DiceRoll records;
- a Rule Set-defined compound resolution.

## 68. Staged Rolls

A Roll may require several generation and resolution stages.

## 69. Awaiting Additional Dice

Use this state when additional random evidence or user selection is required before final resolution.

## 70. Post-Roll Decisions

A Rule Set may require:

- spend resource;
- choose rerolls;
- select kept Dice;
- accept consequence;
- trigger follow-up Roll.

These remain typed Rule Set operations under Chronicle orchestration.

## 71. MVP Boundary

The MVP implements only the mechanics required by the first official Rule Set package.

Future capabilities are contract-ready, not necessarily UI-ready.

## 72. Rule Set Resolution

The selected Rule Set receives:

- exact request payload;
- committed random evidence;
- Character and Campaign mechanical state;
- Preferences;
- prior stage results.

## 73. Resolution Determinism

Given identical:

- Rule Set version;
- request;
- evidence;
- relevant state;
- Preferences;

resolution should be deterministic unless the contract explicitly requests another Chronicle generation stage.

## 74. Resolution Output

Recommended:

```text
DiceResolution
├── ResolutionContractKey
├── ResolutionContractVersion
├── Status
├── OutcomeClassification
├── MechanicalEvidence
├── AppliedEvidenceItemIds[]
├── IgnoredEvidenceItemIds[]
├── RequiredAdditionalGeneration
├── RequiredUserDecision
├── ProposedEffects[]
└── PresentationSummary
```

## 75. Proposed Effects

Rule Set resolution may propose mechanical effects.

Chronicle validates and applies them through approved Application operations.

## 76. No Rule Set Persistence

The Rule Set does not write repositories directly.

## 77. Resolution Commit

Chronicle persists:

- resolution contract;
- outcome;
- evidence use;
- package version;
- proposed or applied effect references;
- timestamps.

## 78. Resolved State

`Resolved` means the mechanical result is committed.

## 79. Narrative Continuation

After resolution, Chronicle creates a correlated Narrative Turn.

## 80. Continuation Input

The continuation receives:

- DiceRollId;
- exact raw evidence;
- exact mechanical resolution;
- applied effects;
- relevant pre-Roll narrative;
- current authoritative context.

## 81. No Provider Reinterpretation

The Narrator may describe the result.

It cannot change the mechanical outcome.

## 82. Continuation Idempotency

Only one continuation stage is scheduled for a given resolved Roll unless an explicit correction requires another.

## 83. Completion Policy

A Roll may become `Completed` when:

- continuation is durably scheduled;
- or accepted continuation is published;

according to the selected Application policy.

That policy must be consistent across persistence and tests.

## 84. Recommended Completion Point

For MVP, `Completed` should mean the required continuation has been durably scheduled or determined unnecessary.

The Narrative Turn owns its own later completion.

## 85. Cancellation

Cancellation is normally allowed only before `Generating`.

## 86. Cancellation After Evidence

Committed random evidence cannot be erased through cancellation.

## 87. Abandoned Post-Generation Roll

If a Roll cannot continue after evidence commit, it enters recovery or terminal failure with evidence preserved.

## 88. Supersession

A pending Roll may be superseded if:

- its originating turn was superseded;
- Campaign state changed before release;
- a corrected Roll request replaced it;
- the active Scene changed incompatibly.

## 89. No Supersession After Evidence Without History

After committed evidence, replacement requires explicit invalidation or correction while preserving the original.

## 90. Invalidation

Invalidation records that the Roll should not remain mechanically authoritative.

## 91. Invalidation Reasons

Examples:

- wrong Rule Set contract;
- invalid actor state;
- corrupted request;
- confirmed software defect;
- explicit adjudication correction.

## 92. Invalidation Evidence

Chronicle records:

- original Roll;
- reason;
- authorizing operation;
- time;
- replacement Roll if any.

## 93. No Destructive Deletion

Historical Rolls are not deleted merely because they were invalidated.

## 94. Correction

A correction creates new authoritative evidence linked to the original Roll.

## 95. Correction Does Not Rewrite Random Evidence

Original raw values remain immutable.

## 96. Recovery

Recovery is required when Chronicle cannot determine:

- whether generation committed;
- whether resolution committed;
- whether additional Dice were generated;
- whether continuation was scheduled;
- whether effects were applied.

## 97. Recovery Queries

Recovery inspects:

- DiceRoll state;
- RandomEvidenceItem records;
- OperationRecord;
- Rule Set resolution record;
- effect records;
- continuation NarrativeTurn;
- unique constraints.

## 98. Unknown Generation Outcome

Chronicle never generates again until it proves no evidence committed.

## 99. Unknown Resolution Outcome

Chronicle checks for an existing resolution before running the Rule Set again.

Deterministic recalculation may be permitted only if no authoritative resolution committed.

## 100. Unknown Continuation Outcome

Chronicle checks for an existing continuation NarrativeTurn.

## 101. Crash Before Release

The Roll remains `AwaitingPlayerRelease`.

## 102. Crash During Generation

Recovery determines whether raw evidence committed.

## 103. Crash After Raw Commit

The same evidence is resolved.

## 104. Crash During Resolution

Recovery checks for existing committed resolution.

## 105. Crash After Resolution

The same result is used to schedule continuation.

## 106. Retry Law

Retry may repeat:

- validation;
- Rule Set deterministic computation;
- continuation provider generation.

Retry may never repeat committed random generation for the same generation stage.

## 107. Persistence Relationships

Recommended:

```text
DiceRoll
    1 → many RandomEvidenceItem

DiceRoll
    1 → many DiceResolutionStage

DiceRoll
    1 → many DiceCorrection

DiceRoll
    → OriginNarrativeTurn

DiceRoll
    → OriginNarrativeEvent

DiceRoll
    → ContinuationNarrativeTurn

DiceRoll
    → OperationRecord
```

## 108. Unique Constraints

Recommended:

```text
OriginNarrativeTurnId + OriginNarrativeEventId unique
DiceRollId + GenerationStageKey + GenerationSequence unique
DiceRollId + ResolutionStageKey unique
DiceRollId + ContinuationStageKey unique
```

## 109. Transaction Boundaries

Short transactions are used for:

- Roll creation;
- release transition;
- raw evidence commit;
- resolution commit;
- effect application;
- continuation scheduling;
- correction.

## 110. No User Wait in Transaction

Awaiting player release or post-Roll decision occurs outside transactions.

## 111. No Provider Call in Transaction

Narrative continuation occurs outside Dice transactions.

## 112. No Long Rule Set Transaction

Complex computation occurs before the short commit transaction where practical.

## 113. Presentation Read Model

Recommended:

```text
DiceRollId
DisplayState
Intent
Actor
Target
RuleSetDisplayName
RollSummary
ReleaseAvailable
RawEvidence
MechanicalOutcome
AdditionalAction
FailureReference
```

## 114. Roll Card

The Roll card is based on persisted Dice state, not raw Narrator output.

## 115. Player Release Control

The release control:

- is keyboard accessible;
- has an accessible name;
- disables after accepted release;
- resists duplicate activation;
- shows progress honestly.

## 116. Result Presentation

The UI distinguishes:

```text
Raw Evidence
Mechanical Resolution
Narrative Description
```

## 117. No Color-Only Result

Success, failure, critical, and other states use text or symbols in addition to color.

## 118. Reduced Motion

Future animation respects reduced-motion preferences.

## 119. English Base Language

State labels, errors, semantic keys, and base UI strings use English.

Generated narrative may use the Campaign-selected language.

## 120. Error Model

Recommended errors:

```text
dice.roll-not-found
dice.request-invalid
dice.rule-set-missing
dice.rule-set-version-mismatch
dice.operation-unsupported
dice.state-invalid
dice.transition-invalid
dice.release-not-allowed
dice.release-duplicate
dice.generation-failed
dice.generation-outcome-unknown
dice.raw-evidence-missing
dice.raw-evidence-conflict
dice.resolution-failed
dice.additional-dice-required
dice.user-decision-required
dice.continuation-failed
dice.cancel-not-allowed
dice.superseded
dice.invalidated
dice.recovery-required
dice.failed-terminal
```

## 121. Data Preservation State

Results should state:

```text
CampaignStateUnchanged
RollProposed
RollValidated
AwaitingPlayerRelease
GenerationNotStarted
RawValuesCommitted
ResolutionPending
AdditionalDicePending
MechanicalResultCommitted
NarrativeContinuationPending
RollCompleted
OriginalEvidencePreserved
RecoveryRequired
```

## 122. Logging

Safe logs may include:

- DiceRollId;
- OperationId;
- NarrativeTurnId;
- Rule Set package and version;
- operation key;
- state transition;
- evidence count;
- generation stage;
- resolution status;
- duration;
- safe error code.

They must not include credentials, hidden prompts, full Campaign prose, or private Character data beyond approved identifiers.

## 123. Metrics

Useful local metrics include:

```text
DiceRollCount
DiceGenerationDuration
DiceResolutionDuration
AdditionalDiceStageCount
DiceRecoveryCount
DuplicateReleaseCount
InvalidatedRollCount
ContinuationFailureCount
```

No remote telemetry is required.

## 124. Testing Strategy

The implementation requires:

```text
State Machine Tests
Request Validation Tests
Player Release Tests
Random Evidence Tests
Resolution Tests
Additional Dice Tests
Idempotency Tests
Recovery Tests
Correction Tests
Persistence Tests
Presentation Tests
Cross-System Contract Tests
```

## 125. State Machine Tests

Every allowed and forbidden transition must be tested.

## 126. Request Tests

Tests cover:

- valid request;
- invalid actor;
- invalid target;
- missing package;
- unsupported operation;
- stale context;
- provider-supplied Dice values.

## 127. Player Release Tests

Tests cover:

- normal release;
- duplicate click;
- retry;
- restart before release;
- cancellation;
- supersession.

## 128. Random Evidence Tests

Tests prove:

- evidence commits once;
- sequence is stable;
- multiple groups work;
- mixed Dice sizes work;
- rerolls preserve prior evidence;
- exploding Dice preserve causal links;
- keep/drop does not delete evidence.

## 129. Resolution Tests

Tests prove:

- exact package version used;
- deterministic resolution;
- proposed effects routed through Application;
- no Rule Set direct persistence;
- no provider reinterpretation.

## 130. Additional Dice Tests

Synthetic tests cover:

- explosion;
- reroll selection;
- opposed group;
- staged damage;
- post-Roll resource spend;
- additional user decision.

These are contract tests and do not require full MVP UI implementation.

## 131. Recovery Tests

Inject failure:

- before release commit;
- during generation;
- after raw evidence commit;
- during resolution;
- after resolution;
- before continuation scheduling;
- after continuation scheduling.

No test may generate a second set of evidence for the same committed stage.

## 132. Correction Tests

Tests prove:

- invalidation preserves evidence;
- replacement Roll links to original;
- no destructive deletion;
- audit history remains complete.

## 133. Cross-System Contract Test

A synthetic non-Werewolf Rule Set must model:

- multiple Dice groups;
- d20 and d6 groups;
- specialty metadata;
- modifiers;
- reroll;
- explosion;
- keep-highest;
- opposed resolution;
- post-Roll spending.

The test verifies Core contract flexibility only.

## 134. Persistence Tests

Tests cover:

- exact relationships;
- unique constraints;
- raw evidence immutability;
- continuation correlation;
- import/export round trip;
- package-version preservation.

## 135. Presentation Tests

Tests cover:

- pending Roll card;
- accessible release;
- duplicate release prevention;
- committed values;
- mechanical result;
- recovery state;
- no animation requirement.

## 136. Architecture Tests

Architecture tests must reject:

- provider-generated Dice outcomes;
- d10-specific Core fields;
- one-pool-only Core schema;
- raw evidence overwrite;
- reroll replacing original value;
- direct Rule Set persistence;
- generation inside provider adapter;
- narrative continuation before resolution;
- animation determining outcome;
- retry regenerating committed evidence;
- duplicate root Dice lifecycle enum.

## 137. Prohibited Patterns

### 137.1 Provider Rolls Dice

Chronicle owns random generation.

### 137.2 Narration Beyond Unresolved Roll

Stop before outcome.

### 137.3 Generate Before Player Release

Wait for explicit action.

### 137.4 Reveal Before Commit

Persist evidence first.

### 137.5 Reroll by Overwriting

Append linked evidence.

### 137.6 Hardcode d10 Pools

Use Rule Set contracts.

### 137.7 Retry by Regenerating

Reuse committed evidence.

### 137.8 Rule Set Writes Database

Chronicle persists.

### 137.9 Animation as Random Source

Animation only follows committed evidence.

### 137.10 Delete Invalid Historical Roll

Preserve and correct.

## 138. Alternatives Considered

### Let Narrative Intelligence Produce Results

Rejected because provider output is nondeterministic and nonauthoritative.

### Generate Dice Immediately When Requested

Rejected because the intended UX requires explicit player release.

### One Fixed Dice Pool Schema

Rejected because Chronicle will support different RPG systems.

### Store Only Final Outcome

Rejected because audit, replay, correction, and complex mechanics require raw evidence.

### Recalculate Historical Rolls on Package Upgrade

Rejected because history must preserve the exact original package and result.

### Implement Every Future Dice Mechanic in MVP

Rejected because extensible contracts are enough until another Rule Set needs them.

## 139. Consequences

### Positive

- one canonical Dice lifecycle;
- clear player agency;
- authoritative random evidence;
- restart safety;
- exact Rule Set binding;
- future system flexibility;
- auditability;
- safe rerolls and explosions;
- no dependence on Werewolf;
- no animation burden in MVP.

### Negative

- richer persistence model;
- additional stage and evidence records;
- Rule Set contracts require versioning;
- recovery is more complex;
- UI must handle waiting and additional-action states later;
- synthetic cross-system tests are required.

## 140. Risks

### Generic Contract Becomes an Opaque Blob

Mitigation:

- Chronicle envelope;
- typed Rule Set payloads;
- contract registry;
- schema validation;
- package tests.

### Additional Dice State Is Overused

Mitigation:

- package-defined necessity;
- bounded stage count;
- explicit transitions;
- size limits.

### Recovery Accidentally Regenerates

Mitigation:

- committed evidence query;
- unique constraints;
- fault injection;
- stage identity.

### First Package Shapes Core

Mitigation:

- no system-specific fields;
- synthetic non-Werewolf tests;
- package ownership of mechanics.

## 141. Technology Spike

Before acceptance, implement:

1. DiceRoll persistence;
2. canonical state enum;
3. transition validator;
4. Roll request contract;
5. Rule Set validation adapter;
6. player-release operation;
7. random evidence generator;
8. evidence commit;
9. resolution contract;
10. additional-Dice state;
11. continuation correlation;
12. recovery queries;
13. correction and invalidation records;
14. accessible Roll read model;
15. synthetic complex Rule Set fixture;
16. architecture tests.

## 142. Spike Acceptance

The spike passes when:

- a Narrator Roll request creates one validated DiceRoll;
- the Roll waits for player release;
- duplicate release creates one generation;
- raw evidence commits before resolution;
- restart after commit reuses the same evidence;
- Rule Set resolves with the exact package version;
- continuation is correlated;
- invalidation preserves history;
- a synthetic complex non-Werewolf Roll completes several stages;
- no d10-specific Core field exists;
- no animation is required.

## 143. Definition of Compliance

An implementation complies when:

- every Roll uses the canonical lifecycle in this RFC;
- providers only propose;
- players explicitly release;
- Chronicle generates and commits random evidence;
- Rule Sets resolve mechanics;
- raw values persist before result or continuation;
- retry never regenerates committed evidence;
- complex future mechanics fit versioned package contracts;
- historical Rolls preserve package version and evidence;
- invalidation and correction are non-destructive;
- continuation uses a correlated Narrative Turn;
- Core remains independent from Werewolf and every specific Dice system.

## 144. Review Triggers

Review this RFC if:

- multiplayer changes who releases a Roll;
- physical Dice input becomes authoritative;
- remote random services are introduced;
- cryptographic verifiability becomes a product requirement;
- real-time shared Rolls are introduced;
- Rule Sets require simultaneous hidden Dice;
- Dice contracts move into provider output;
- automatic Roll release is proposed.

## 145. Deferred Decisions

Later decisions may define:

- physical Dice capture;
- remote shared Dice;
- cryptographic commitment schemes;
- user-selectable visual Dice animation;
- advanced Roll builder UI;
- multiplayer Roll permissions;
- hidden GM Dice;
- public Roll verification;
- custom 3D Dice assets.

## 146. Final Decision

Chronicle will use one canonical Dice lifecycle:

```text
Proposed
Validated
AwaitingPlayerRelease
Generating
RawValuesCommitted
Resolving
AwaitingAdditionalDice
Resolved
AwaitingNarrativeContinuation
Completed
Cancelled
Superseded
Invalidated
RecoveryRequired
FailedTerminal
```

Narrative Intelligence proposes uncertainty.

The player releases it.

Chronicle generates and preserves the evidence.

The Rule Set interprets it.

Narrative continues only after the mechanical result is committed.

The first Rule Set may be simple.

The Dice architecture will not be.
