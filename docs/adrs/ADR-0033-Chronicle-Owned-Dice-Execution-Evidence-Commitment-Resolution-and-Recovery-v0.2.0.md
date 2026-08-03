---
id: ADR-0033
title: Chronicle-Owned Dice Execution, Evidence Commitment, Resolution, and Recovery
status: Proposed
version: 0.2.0
owner: Chronicle Team
last_updated: 2026-08-02
category: Domain and Application Architecture
supersedes:
  - ADR-0033@0.1.0
superseded_by: null
depends_on:
  - RFC-0012
  - RFC-0017
  - RFC-0019
  - RFC-0021
  - RFC-0025
  - RFC-0033
  - ADR-0002
  - ADR-0004
  - ADR-0024
  - ADR-0026
  - ADR-0034
  - ADR-0036
  - ADR-0041
implements:
  - RFC-0012
related_to:
  - ADR-0013
  - ADR-0014
  - ADR-0027
  - ADR-0038
---

> **"The Narrator may ask for chance. Chronicle releases it, records it, and never rewrites it."**

# Chronicle-Owned Dice Execution, Evidence Commitment, Resolution, and Recovery

## 1. Status

**Proposed**

This ADR defines the concrete architecture for Dice execution.

The decision is:

- Chronicle owns authoritative random generation;
- the Narrator may propose a Roll but may not generate its values;
- the player explicitly releases a pending Roll in the MVP interaction model;
- Chronicle validates the request before generation;
- Chronicle commits raw random evidence before any mechanical resolution or narrative continuation;
- Rule Set packages interpret committed evidence through versioned contracts;
- retries and restart recovery must reuse committed evidence;
- rerolls, explosions, chained Dice, and additional Dice append new linked evidence instead of overwriting old evidence;
- keep/drop and other selection mechanics preserve all original evidence;
- Dice history is never recomputed silently;
- corrections preserve the original Roll and create explicit invalidation or replacement records;
- every Roll is bound to the exact Rule Set package, package version, operation key, and contract version;
- Application orchestrates the workflow;
- Domain owns system-neutral Dice identities and invariants;
- Infrastructure supplies the approved cryptographic random source;
- Persistence records immutable evidence and durable lifecycle state;
- Presentation displays requests and evidence but does not generate or resolve authoritative outcomes;
- no Core contract assumes d10, a fixed pool, Werewolf terminology, or one-stage resolution;
- animation is outside MVP and must not become part of correctness;
- the first Rule Set may use only a subset of the generic contract, but Core must not need redesign for later systems.

## 2. Context

Dice are a central authority boundary.

If the LLM generates values, Chronicle cannot prove that the Roll was:

- random;
- released by the player;
- persisted once;
- reused after a crash;
- interpreted by the correct Rule Set;
- protected from retry duplication.

If Chronicle stores only the final outcome, it also loses:

- raw evidence;
- reroll causality;
- explosion causality;
- keep/drop selection;
- opposed groups;
- staged resolution;
- correction history;
- forensic recovery.

The architecture must support a simple MVP Roll without constraining future Rule Sets to that shape.

## 3. Responsibility Matrix

```text
Narrator
    proposes that a Roll is needed
    explains fictional stakes
    waits before the outcome

Player
    releases the Roll when required
    may make later Rule Set-defined decisions

Chronicle Application
    validates workflow state
    orchestrates generation
    persists evidence
    invokes Rule Set resolution
    schedules continuation
    recovers idempotently

Chronicle Domain
    defines system-neutral Roll identity and invariants

Random Source
    produces raw values only

Rule Set Package
    validates mechanical request
    interprets committed evidence
    returns deterministic resolution

Persistence
    preserves lifecycle, evidence, and correction history

Presentation
    displays request, release action, evidence, and outcome
```

## 4. Canonical Dice Lifecycle

The only canonical `DiceRoll` states are:

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

## 5. State Meanings

### 5.1 Proposed

A Roll request exists but has not yet passed Chronicle and Rule Set validation.

### 5.2 Validated

The request is valid and bound to an exact package and contract.

### 5.3 AwaitingPlayerRelease

Chronicle is waiting for the player to trigger generation.

### 5.4 Generating

A defined generation stage is executing.

### 5.5 RawValuesCommitted

All raw evidence for the current generation stage has been durably committed.

### 5.6 Resolving

The exact Rule Set is interpreting committed evidence.

### 5.7 AwaitingAdditionalDice

Resolution requires another generation stage.

### 5.8 Resolved

The mechanical result is durably committed.

### 5.9 AwaitingNarrativeContinuation

The result exists, but the continuation Narrative Turn is not yet durably complete.

### 5.10 Completed

The Dice workflow and required continuation handoff are complete.

### 5.11 Cancelled

The Roll was cancelled before irreversible evidence commitment.

### 5.12 Superseded

Another valid workflow replaced this request before completion.

### 5.13 Invalidated

The Roll remains in history but is no longer authoritative for current interpretation.

### 5.14 RecoveryRequired

Chronicle cannot determine the safe next action automatically.

### 5.15 FailedTerminal

The workflow cannot continue under the current contract or data.

## 6. State Registry Authority

RFC-0012 defines the canonical lifecycle contract.

This ADR defines its implementation architecture.

No alternate Dice state enum is permitted.

## 7. Roll Request

The Narrator proposes a Roll through:

```text
roll.requested
```

## 8. Request Contents

A canonical request includes:

```text
RollRequestId
NarrativeTurnId
NarrativeEventId
RuleOperationKey
RollContractKey
RollContractVersion
PurposeKey
FictionalStakes
MechanicalInputPayload
DisplayHints
```

## 9. Request Neutrality

The request may describe:

- one die;
- one pool;
- multiple groups;
- mixed Dice sizes;
- symbolic Dice;
- opposed groups;
- staged operations;
- package-defined modifiers.

It must not assume one universal shape.

## 10. Narrator Limitation

The Narrator must not provide:

- authoritative face values;
- authoritative success counts;
- authoritative critical classification;
- authoritative reroll values;
- authoritative explosion values;
- final mechanical effects.

## 11. Validation

Application validates:

- Campaign ownership;
- active hierarchy;
- Narrative Turn and event identity;
- no accepted duplicate request;
- exact Rule Set package availability;
- operation contract;
- payload schema;
- player-release policy;
- current aggregate versions;
- authorization;
- package compatibility.

## 12. Rule Set Validation

The exact Rule Set package validates package-owned mechanical semantics.

## 13. Validation Output

Validation produces a normalized immutable generation plan.

## 14. Generation Plan

Recommended shape:

```text
DiceGenerationPlan
├── DiceRollId
├── GenerationStageKey
├── Groups[]
├── RandomSourceContractKey
├── RuleSetPackageId
├── RuleSetPackageVersion
├── RollContractKey
└── RollContractVersion
```

## 15. Group Shape

A group may include:

```text
GroupKey
DiceKindKey
FaceCount
Quantity
SymbolSetKey
OrderingPolicy
Metadata
```

## 16. No Werewolf-Specific Fields

Core generation contracts must not contain:

- rage dice;
- hunger dice;
- auspice;
- tribe;
- success threshold fixed for all systems;
- d10-only fields.

Package-specific concepts belong to versioned payloads.

## 17. Player Release

In MVP, a validated player-facing Roll normally enters:

```text
AwaitingPlayerRelease
```

## 18. Release Action

The UI invokes an Application command using:

```text
DiceRollId
OperationId
ExpectedVersion
```

## 19. Release Idempotency

Repeated release of the same intent returns the existing workflow state.

## 20. No UI Randomness

Presentation does not generate authoritative values.

## 21. Animation

Animation may visualize already-generated evidence later.

It may never:

- decide values;
- delay durability requirements;
- become required for recovery;
- affect mechanical interpretation.

Animation is outside MVP.

## 22. Random Source

Chronicle uses an approved cryptographic random source supplied by Infrastructure.

## 23. Random Source Contract

Recommended:

```text
IRandomEvidenceSource
```

## 24. Random Source Output

The source returns raw random material or bounded values according to the generation plan.

## 25. Random Source Prohibitions

The random source does not:

- resolve Rule Set mechanics;
- persist Campaign state;
- select Narrative outcomes;
- call providers;
- alter the generation plan.

## 26. Generation Stage Identity

Every generation stage has a stable key.

## 27. One Stage, One Commitment

A generation stage is generated and committed as one idempotent logical unit.

## 28. Evidence Item

Each generated item receives:

```text
RandomEvidenceItemId
DiceRollId
GenerationStageKey
GroupKey
DieKey
DieOrdinal
DiceKindKey
FaceCount
RawFaceValue
RawSymbolValue
GenerationSequence
ParentEvidenceItemId
CauseKey
RandomSourceContractKey
CreatedAtUtc
```

## 29. Raw Evidence Authority

The evidence item is the authoritative record of what chance produced.

## 30. Evidence Immutability

Committed evidence is append-only.

Normal update and delete are prohibited.

## 31. Commit Before Resolution

Chronicle must commit raw evidence before invoking authoritative Rule Set resolution.

## 32. Commit Boundary

The evidence transaction persists:

- evidence items;
- generation-stage identity;
- uniqueness records;
- Roll transition to `RawValuesCommitted`;
- commit timestamp;
- Operation result.

## 33. No Partial Stage

A generation stage cannot be partially committed.

## 34. Crash Before Commit

No committed evidence exists.

The stage may be generated again under the same intent.

## 35. Crash After Commit

The committed evidence is reused.

It must never be generated again.

## 36. Unknown Commit

Chronicle queries:

- DiceRoll state;
- evidence-stage identity;
- evidence sequence;
- Operation result.

If the outcome remains ambiguous:

```text
RecoveryRequired
```

## 37. Retry Rule

```text
Retry may repeat orchestration.
Retry may not replace committed chance.
```

## 38. Rule Set Resolution

After raw evidence commits, Application invokes the exact bound Rule Set package.

## 39. Resolution Input

The package receives:

- normalized Roll request;
- exact committed evidence;
- prior stage results;
- package-owned Character data;
- relevant resources;
- explicit player decisions;
- exact contract versions.

## 40. Deterministic Resolution

Given the same:

- package version;
- contract version;
- request payload;
- committed evidence;
- relevant authoritative state;

resolution must be deterministic.

## 41. Resolution Output

Recommended shape:

```text
DiceResolutionResult
├── StageKey
├── OutcomeClassificationKey
├── EvidenceUse[]
├── MechanicalEffects[]
├── RequiresAdditionalGeneration
├── AdditionalGenerationRequest
├── RequiresPlayerDecision
├── PlayerDecisionContract
├── NarrativeSummaryData
└── ResolutionPayload
```

## 42. Evidence Use

The package identifies how each evidence item was used.

Examples:

```text
used
kept
dropped
ignored
rerolled
triggered-explosion
opposed
bonus
penalty
```

## 43. Keep and Drop

Keep/drop mechanics never delete evidence.

They create evidence-use records.

## 44. Rerolls

A reroll creates new evidence.

## 45. Reroll Causality

New evidence references the original through:

```text
ParentEvidenceItemId
CauseKey = reroll
```

## 46. Exploding or Chained Dice

Additional Dice generated because of prior evidence append new linked items.

## 47. Explosion Causality

Use:

```text
ParentEvidenceItemId
CauseKey = explosion
```

or another registered package-neutral cause key.

## 48. Additional Dice

If resolution requires another stage:

```text
DiceRoll → AwaitingAdditionalDice
```

## 49. Additional Stage

The next stage has its own:

- stage key;
- generation plan;
- evidence commitment;
- resolution record.

## 50. Same Dice Roll

Additional stages remain under the same DiceRollId unless the Rule Set contract explicitly defines a distinct linked Roll.

## 51. Opposed Rolls

The generic architecture supports opposed groups within one Roll or linked Rolls, according to the package contract.

## 52. Post-Roll Decisions

The architecture permits a Rule Set-defined player decision after evidence commitment.

## 53. MVP Choice Scope

This does not introduce the general Narrative `Choice` feature into MVP.

A mechanical Dice decision may use a narrow Dice-specific contract.

## 54. Resolution Persistence

The resolution transaction persists:

- stage record;
- input-evidence hash;
- evidence-use relationships;
- outcome;
- package payload;
- additional-stage or decision requirement;
- Roll state;
- Operation result.

## 55. Input Evidence Hash

Resolution records the exact evidence set it interpreted.

## 56. Historical Stability

Historical resolved Rolls are not silently recalculated when:

- application version changes;
- package version changes;
- algorithm changes;
- provider changes.

## 57. Recalculation

Any explicit recalculation is a new auditable operation and does not overwrite the original result.

## 58. Mechanical Effects

Rule Set output proposes typed mechanical effects.

## 59. Effect Authority

Chronicle validates and applies effects through Application operations.

## 60. No Package Direct Mutation

Rule Set packages cannot persist or mutate Campaign state directly.

## 61. Resolution Completion

When the mechanical result is committed:

```text
DiceRoll → Resolved
```

## 62. Narrative Continuation

Application creates a new correlated Narrative Turn for post-Roll narration.

## 63. Continuation Correlation

The relationship includes:

```text
OriginNarrativeTurnId
DiceRollId
ContinuationNarrativeTurnId
```

## 64. Continuation State

Before continuation is safely scheduled:

```text
AwaitingNarrativeContinuation
```

## 65. Continuation Idempotency

Only one continuation exists for one defined Roll continuation stage.

## 66. Duplicate Continuation Prevention

Use:

- OperationId;
- unique constraints;
- direct recovery lookup.

## 67. Completion

The Roll becomes `Completed` after the required continuation handoff is durably complete.

## 68. Provider Boundary

The continuation Narrator receives:

- fictional context;
- committed Roll result;
- safe evidence summary;
- mechanical effects;
- continuity constraints.

## 69. Provider Prohibition

The provider may not reinterpret or replace the mechanical result.

## 70. Cancellation

Cancellation is permitted only before irreversible evidence commitment, unless the package contract defines a safe later abandonment state.

## 71. Cancel After Evidence

Committed evidence remains in history.

The workflow may be invalidated or superseded, not erased.

## 72. Supersession

A Roll may be superseded before completion when another valid workflow replaces it.

## 73. Invalidation

Invalidation preserves:

- original request;
- raw evidence;
- resolution;
- reason;
- correcting Operation.

## 74. Correction

A correction creates:

```text
DiceCorrection
```

## 75. Correction Types

Examples:

```text
Invalidation
ReplacementRoll
MechanicalReclassification
AdministrativeAnnotation
```

## 76. Replacement Roll

A replacement has a new DiceRollId and explicit link to the original.

## 77. No Evidence Editing

Correction does not edit face values.

## 78. Persistence Model

Required records:

```text
DiceRolls
RandomEvidenceItems
DiceResolutionStages
DiceResolutionEvidenceUse
DiceCorrections
```

## 79. Exact Package Binding

Each DiceRoll persists:

```text
RuleSetPackageId
RuleSetPackageVersion
RuleOperationKey
RollContractKey
RollContractVersion
```

## 80. Missing Package

If the exact package is unavailable:

- historical evidence remains readable;
- historical resolution remains preserved;
- new interpretation is blocked;
- Campaign may open in restricted mode;
- package recovery is offered.

## 81. No Automatic Package Upgrade

Chronicle must not resolve a pending historical Roll with a different package version silently.

## 82. Encryption

All Dice records are stored in the encrypted Chronicle database.

## 83. Backup

Encrypted backups include:

- pending Rolls;
- committed evidence;
- resolution stages;
- corrections;
- continuation correlation;
- exact package bindings.

## 84. Restore

Restore must preserve evidence exactly.

## 85. No Reroll on Restore

A restored `RawValuesCommitted` Roll resumes from committed evidence.

## 86. Export

Portable Campaign export preserves generic typed Dice evidence and exact package bindings according to RFC-0034 and ADR-0027.

## 87. Export Prohibition

Portable export must not flatten all Dice into one final success count.

## 88. Work Item Integration

Durable work may include:

```text
dice.generate
dice.resolve
dice.schedule-continuation
dice.recover
```

## 89. Operation Integration

Authoritative operations include:

```text
dice.create
dice.release
dice.commit-evidence
dice.resolve
dice.apply-effects
dice.correct
dice.schedule-continuation
```

## 90. Work Item and Operation Separation

Work Item schedules execution.

OperationRecord owns idempotent intent and result.

## 91. Concurrency

DiceRoll uses optimistic concurrency.

## 92. Expected Version

Release, resolution, correction, and continuation commands include expected version where appropriate.

## 93. Campaign Mutation Lane

Conflicting authoritative Dice effects may participate in the Campaign mutation lane.

## 94. In-Process Lock

Any in-process lock is only an optimization.

Database constraints remain authoritative.

## 95. Security

Dice security requires:

- Chronicle-owned random source;
- no provider-generated values;
- encrypted persistence;
- append-only evidence;
- exact package binding;
- safe logs;
- no silent retry regeneration.

## 96. Random Source Version

Evidence records the random-source contract key, not private internal entropy.

## 97. No Seed Disclosure Requirement

Chronicle does not need to expose or persist a reusable seed in MVP.

## 98. Future Verifiable Randomness

Cryptographic commitments or externally verifiable randomness may be added later without changing the evidence identity model.

## 99. Logging

Safe logs may include:

- DiceRollId;
- OperationId;
- WorkItemId;
- state transition;
- generation-stage key;
- evidence item count;
- package ID and version;
- duration;
- safe error code.

Logs must not include unnecessary Campaign prose or secret material.

## 100. Metrics

Useful local metrics include:

```text
DiceGenerationDuration
DiceResolutionDuration
DiceRecoveryCount
DiceDuplicateReleaseCount
DiceAdditionalStageCount
DiceCorrectionCount
```

No remote telemetry is required.

## 101. Error Model

Recommended errors:

```text
dice.request-invalid
dice.request-duplicate
dice.package-unavailable
dice.package-version-mismatch
dice.state-invalid
dice.release-duplicate
dice.generation-plan-invalid
dice.generation-failed
dice.evidence-conflict
dice.evidence-immutable
dice.evidence-stage-incomplete
dice.resolution-failed
dice.resolution-nondeterministic
dice.additional-stage-invalid
dice.player-decision-required
dice.continuation-duplicate
dice.correction-invalid
dice.recovery-required
dice.failed-terminal
```

## 102. Data Preservation State

Results should state:

```text
CampaignStateUnchanged
DiceRequestPersisted
AwaitingPlayerRelease
RawEvidenceNotCommitted
RawEvidenceCommitted
CommittedEvidenceReused
ResolutionCommitted
AdditionalDiceRequired
PlayerDecisionRequired
ContinuationScheduled
OriginalEvidencePreserved
RollInvalidated
RecoveryRequired
```

## 103. User Experience

The MVP flow is:

```text
Narrator reaches the uncertainty
    ↓
Roll card appears
    ↓
Player releases the Roll
    ↓
Chronicle generates and commits evidence
    ↓
Rule Set resolves
    ↓
Result is displayed
    ↓
Narrator continues
```

## 104. Roll Card

The Roll card displays:

- fictional stakes;
- mechanical summary;
- release action;
- current state;
- final evidence and result when available;
- recovery state when needed.

## 105. Pre-Outcome Stop

Narration must stop before revealing the outcome.

## 106. No Fabricated Preview

The UI must not display simulated or decorative values before authoritative generation.

## 107. Accessibility

The Roll flow supports:

- keyboard release;
- screen-reader labels;
- noncolor states;
- deterministic focus;
- no animation dependency;
- clear recovery text.

## 108. Testing Strategy

The implementation requires:

```text
State Machine Tests
Request Validation Tests
Random Source Tests
Evidence Commitment Tests
Retry Tests
Crash Recovery Tests
Resolution Tests
Reroll Tests
Explosion Tests
KeepDrop Tests
Opposed Roll Tests
Additional Stage Tests
Correction Tests
Continuation Tests
Persistence Tests
Backup Restore Tests
Export Import Tests
Security Tests
Cross-System Tests
Architecture Tests
```

## 109. State Tests

Every allowed and forbidden lifecycle transition is tested.

## 110. Request Tests

Test:

- valid request;
- duplicate request;
- invalid package;
- invalid contract;
- stale Narrative Turn;
- unsupported payload.

## 111. Random Source Tests

Test bounded generation for several Dice kinds without Rule Set semantics.

## 112. Evidence Commitment Tests

Prove:

- all-or-nothing stage commit;
- immutable evidence;
- stable sequence;
- exact stage identity;
- package binding.

## 113. Retry Tests

Repeated release, Work Item retry, and process restart must create no new evidence after commitment.

## 114. Crash Tests

Inject failure:

- before generation;
- during generation;
- before evidence commit;
- after evidence commit;
- during resolution;
- after resolution;
- before continuation;
- after continuation commit.

## 115. Resolution Tests

Given identical authoritative inputs, resolution is identical.

## 116. Reroll Tests

Original and new evidence both remain.

## 117. Explosion Tests

Causal parent relationships remain intact.

## 118. KeepDrop Tests

Dropped evidence remains persisted.

## 119. Opposed Tests

Multiple groups preserve identity and ordering.

## 120. Additional Stage Tests

A second stage commits independently under the same Roll.

## 121. Correction Tests

Invalidation and replacement preserve original history.

## 122. Continuation Tests

Duplicate scheduling creates one Narrative Turn.

## 123. Backup Restore Tests

Pending, committed, resolved, and corrected Rolls survive encrypted backup and restore.

## 124. Export Import Tests

Generic evidence survives round-trip without a database-schema assumption.

## 125. Security Tests

Providers cannot provide authoritative values, and logs contain no sensitive canaries.

## 126. Cross-System Fixture

A synthetic non-Werewolf Rule Set must exercise:

- d20 group;
- d6 group;
- mixed pool;
- specialty metadata;
- modifier;
- reroll;
- explosion;
- keep-highest;
- opposed group;
- additional generation;
- post-Roll decision;
- final resolution.

No Core schema or contract change is allowed.

## 127. Architecture Tests

Architecture tests must reject:

- provider-generated Dice values;
- UI-generated authoritative randomness;
- Rule Set direct persistence;
- mutable evidence entities;
- fixed d10-only contracts;
- Werewolf-specific Core fields;
- resolution before evidence commitment;
- retry that regenerates committed evidence;
- continuation without correlation;
- package version omitted;
- historical recomputation on load;
- animation used as correctness state.

## 128. Prohibited Patterns

### 128.1 Narrator Rolls the Dice

Narrator requests only.

### 128.2 UI Generates Values

Infrastructure generates through Application orchestration.

### 128.3 Resolve Before Commit

Commit evidence first.

### 128.4 Retry Regenerates Evidence

Recover and reuse.

### 128.5 Reroll Overwrites Original

Append linked evidence.

### 128.6 Keep/Drop Deletes Dice

Preserve all evidence.

### 128.7 One Final Success Integer Only

Preserve generic evidence and resolution.

### 128.8 Recalculate History with New Package

Preserve historical result and exact binding.

### 128.9 Core Assumes Werewolf or d10

Keep contracts system-neutral.

### 128.10 Animation Determines Result

Animation is Presentation only.

## 129. Alternatives Considered

### LLM-Generated Rolls

Rejected because authority, auditability, and recovery would be weak.

### Persist Only Final Outcome

Rejected because complex mechanics and correction history would be lost.

### Generate and Resolve in One Opaque Step

Rejected because crash recovery could not distinguish committed chance from mechanical interpretation.

### Store One JSON Blob per Roll

Rejected because identity, append-only evidence, causal links, and uniqueness need explicit persistence.

### Recompute on Load

Rejected because package versions and algorithms may change.

### Werewolf-Specific MVP Schema

Rejected because the first package must not define the framework permanently.

## 130. Consequences

### Positive

- authoritative randomness is local and auditable;
- restart cannot alter committed chance;
- future complex Dice systems fit;
- Rule Set packages remain replaceable;
- Narrative continuation is safely correlated;
- corrections preserve history;
- backup and export retain meaningful evidence;
- no future d10-to-generic redesign is required.

### Negative

- persistence is more detailed;
- workflow has several phases;
- package contracts require discipline;
- recovery and fault-injection tests are substantial;
- the MVP implements generic boundaries beyond the first package's immediate mechanics.

## 131. Risks

### Generic Contract Becomes Overengineered

Mitigation:

- keep payloads versioned;
- implement only needed first-package features;
- test future compatibility with synthetic fixtures;
- avoid speculative UI.

### Random Evidence Commit Fails After Generation

Mitigation:

- stage only in memory;
- commit atomically;
- no user-visible outcome before commit.

### Package Resolution Changes

Mitigation:

- exact version binding;
- persisted result;
- no silent recomputation.

### Duplicate Provider Continuation

Mitigation:

- unique correlation;
- OperationId;
- recovery lookup.

## 132. Technology Spike

Before acceptance, implement:

1. canonical Dice state machine;
2. system-neutral request contract;
3. player release command;
4. cryptographic random source;
5. generation-plan normalization;
6. immutable evidence persistence;
7. atomic evidence-stage commit;
8. exact package binding;
9. deterministic Rule Set resolution;
10. evidence-use records;
11. reroll and explosion causality;
12. additional generation stage;
13. continuation correlation;
14. correction records;
15. encrypted backup and restore fixture;
16. portable export fixture;
17. cross-system synthetic package;
18. fault injection;
19. architecture tests.

## 133. Spike Acceptance

The spike passes when:

- a Narrator request creates one validated Roll;
- player release generates one committed evidence stage;
- retry after commit creates no new values;
- crash after commit resumes from evidence;
- the exact package resolves deterministically;
- reroll and explosion preserve causal history;
- keep/drop preserves all evidence;
- a second generation stage works;
- one continuation is scheduled;
- correction preserves the original;
- encrypted backup and restore preserve the Roll exactly;
- the synthetic non-Werewolf fixture passes without Core changes.

## 134. Definition of Compliance

An implementation complies when:

- Chronicle owns random generation;
- the player releases MVP Rolls where required;
- the Narrator never supplies authoritative values;
- raw evidence commits before resolution;
- committed evidence is immutable and append-only;
- retries and restores reuse committed evidence;
- Rule Set resolution is exact-version-bound and deterministic;
- rerolls and explosions append linked evidence;
- keep/drop does not delete evidence;
- continuation is uniquely correlated;
- corrections preserve original history;
- Core assumes neither Werewolf nor d10;
- animation is outside correctness and outside MVP.

## 135. Review Triggers

Review this ADR if:

- physical Dice become authoritative;
- multiplayer introduces shared Roll release;
- server authority is introduced;
- cryptographic public verification is required;
- external hardware random sources are added;
- Rule Sets require hidden Dice;
- simultaneous opposed participants become official;
- streamed Dice animation becomes authoritative UX.

## 136. Deferred Decisions

Later decisions may define:

- cryptographic Dice commitments;
- public verifiability;
- physical Dice ingestion;
- shared multiplayer release;
- hidden Game Master Dice;
- hardware random sources;
- advanced animation;
- remote authoritative Dice service.

## 137. Final Decision

Chronicle owns chance.

The Narrator may request it.

The player may release it.

The Rule Set may interpret it.

But once Chronicle commits the evidence, retries, restarts, providers, and future package versions may not rewrite what happened.
