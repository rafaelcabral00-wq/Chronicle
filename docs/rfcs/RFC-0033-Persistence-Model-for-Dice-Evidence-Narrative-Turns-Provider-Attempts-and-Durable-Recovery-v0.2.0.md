---
id: RFC-0033
title: Persistence Model for Dice Evidence, Narrative Turns, Provider Attempts, and Durable Recovery
status: Draft
version: 0.2.0
owner: Chronicle Team
last_updated: 2026-08-02
category: Persistence
supersedes:
  - RFC-0033@0.1.0
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
implements: []
related_to:
  - ADR-0004
  - ADR-0010
  - ADR-0013
  - ADR-0014
  - ADR-0024
  - ADR-0025
  - ADR-0026
  - ADR-0033
  - ADR-0034
  - ADR-0036
  - ADR-0038
  - ADR-0041
---

> **"Persistence must remember not only what became true, but also enough evidence to prove how Chronicle got there."**

# Persistence Model for Dice Evidence, Narrative Turns, Provider Attempts, and Durable Recovery

## 1. Status

**Draft**

This RFC defines the persistence requirements for:

- Dice Rolls;
- random evidence;
- multi-stage Dice resolution;
- Dice corrections and invalidation;
- Narrative Turns;
- accepted Narrative output;
- Provider Attempts;
- bounded provider-response staging;
- event application correlation;
- Message correlation;
- Work Item correlation;
- Operation Record correlation;
- restart recovery;
- export and backup behavior;
- retention and privacy.

This revision resolves two major gaps:

1. the earlier Dice persistence model used a lifecycle incompatible with the canonical Dice state machine;
2. durable Narrative Turn and Provider Attempt records introduced later were not integrated into the persistence architecture.

The decision is:

- use the canonical Dice lifecycle from RFC-0012;
- persist random evidence as immutable append-only records;
- persist Dice resolution by stage;
- preserve exact Rule Set package and contract versions;
- persist `NarrativeTurn` as an Application workflow record;
- persist `NarrativeAcceptedOutput`;
- persist `ProviderAttempt`;
- allow bounded `NarrativeResponseStaging` for crash recovery;
- correlate accepted Messages, Structured Events, Work Items, Operations, and Dice Rolls to Narrative Turns;
- keep provider-native threads and raw credentials nonauthoritative;
- keep durable records system-agnostic;
- avoid Werewolf-specific columns;
- preserve future Dice complexity through generic envelopes and Rule Set-defined payloads;
- define backup, export, retention, and recovery behavior explicitly;
- use English semantic keys.

## 2. Purpose

Chronicle's persistence model must support:

- authoritative Campaign history;
- deterministic recovery;
- idempotent retries;
- auditable Dice evidence;
- narrative interruption and continuation;
- provider failure and repair;
- package version binding;
- future complex Rule Sets;
- safe backup and portability;
- privacy boundaries.

A model that stores only final prose and final Dice outcome is insufficient.

Chronicle must know:

- which provider attempt produced accepted output;
- which Narrative Event caused a Dice Roll;
- whether raw evidence committed;
- whether resolution completed;
- whether continuation was scheduled;
- which exact package interpreted the Roll;
- whether a retry already applied an effect;
- which records may be safely retained, compacted, or excluded from export.

## 3. Scope

This RFC covers logical persistence contracts for:

- Narrative Turn records;
- accepted Narrative output;
- Narrative Event application records;
- Provider Attempts;
- response staging;
- Dice Roll records;
- random evidence items;
- resolution stages;
- corrections;
- Message references;
- Operation and Work Item references;
- constraints;
- indexes;
- concurrency;
- recovery;
- retention;
- backup;
- export;
- privacy;
- testing.

## 4. Out of Scope

This RFC does not define:

- exact SQL syntax;
- exact EF Core mappings;
- exact archive format;
- exact provider API;
- exact Rule Set mechanics;
- exact UI;
- cryptographic database encryption;
- cloud synchronization;
- multiplayer persistence;
- analytics warehouse.

## 5. Persistence Principles

Chronicle persistence follows:

```text
Chronicle owns authoritative records.
Providers produce proposals and operational evidence.
Rule Sets produce deterministic mechanical interpretation.
Random evidence is append-only.
Accepted narrative is explicitly correlated.
Unknown outcomes are recoverable from durable state.
```

## 6. Aggregate Boundary

Campaign remains the Domain aggregate root.

Narrative Turns, Provider Attempts, Work Items, and Operation Records are Application persistence records correlated to Campaign state.

They are not new Domain aggregate roots.

## 7. Logical Storage Areas

The logical model contains:

```text
Domain Tables
Application Workflow Tables
Narrative Intelligence Operational Tables
Dice Evidence Tables
History and Correction Tables
Projection Tables
Artifact and Recovery Tables
```

## 8. Narrative Turn Table

Recommended logical table:

```text
NarrativeTurns
```

## 9. Narrative Turn Fields

```text
NarrativeTurnId
CampaignId
SessionId
ActId
SceneId
ParentNarrativeTurnId
ContinuationOfDiceRollId
TriggerTypeKey
TriggerReference
RoleKey
ContractVersion
StateKey
ContextSnapshotReference
AcceptedOutputId
CreatedAtUtc
UpdatedAtUtc
AcceptedAtUtc
CompletedAtUtc
FailureCode
FailureDetailsReference
RowVersion
```

## 10. Narrative Turn Identity

`NarrativeTurnId` is a strongly typed Chronicle identifier.

## 11. Narrative Turn Ownership

Every Narrative Turn belongs to exactly one Campaign.

## 12. Optional Hierarchy References

Session, Act, and Scene references may be nullable only when the trigger legitimately occurs outside an active hierarchy state.

## 13. Parent Narrative Turn

A continuation may reference one parent Narrative Turn.

## 14. Dice Continuation Reference

A post-Roll continuation may reference one DiceRollId.

## 15. Narrative Turn State

The persisted state uses ADR-0034's canonical state registry.

No second enum is stored.

## 16. Trigger Type

Initial trigger keys include:

```text
player-message
dice-resolution
scene-start
session-start
recovery-continuation
chronicle-director-request
```

## 17. Narrative Turn Row Version

Optimistic concurrency protects state transitions.

## 18. Narrative Accepted Output Table

Recommended logical table:

```text
NarrativeAcceptedOutputs
```

## 19. Accepted Output Fields

```text
NarrativeAcceptedOutputId
NarrativeTurnId
ProviderAttemptId
ContractVersion
CompletionStatusKey
StopReasonKey
CanonicalPayloadHash
NormalizedPayloadReference
AcceptedAtUtc
RowVersion
```

## 20. One Accepted Output

At most one accepted output exists for one Narrative Turn stage.

## 21. Accepted Output Uniqueness

Recommended unique constraint:

```text
NarrativeTurnId unique
```

if one record represents the complete accepted stage.

If future continuation stages are modeled inside one turn, use:

```text
NarrativeTurnId + StageKey unique
```

The MVP uses one accepted output per Narrative Turn.

## 22. Canonical Payload

Chronicle may store normalized accepted output for recovery and audit.

It must not treat raw provider JSON as the canonical persisted contract.

## 23. Canonical Payload Hash

A hash supports duplicate detection and forensic comparison.

## 24. Narrative Blocks

Accepted Narrative Blocks become authoritative Messages.

## 25. Message Correlation

Messages produced from Narrator output store:

```text
NarrativeTurnId
NarrativeAcceptedOutputId
NarrativeBlockId
NarrativeBlockSequence
```

## 26. Message Uniqueness

Recommended:

```text
NarrativeTurnId + NarrativeBlockSequence unique
```

## 27. Message Authority

The Message is authoritative transcript state.

The provider response is not.

## 28. Structured Event Application Table

Recommended:

```text
NarrativeEventApplications
```

## 29. Event Application Fields

```text
NarrativeEventApplicationId
NarrativeTurnId
NarrativeAcceptedOutputId
ProviderEventId
EventSequence
EventTypeKey
EventContractVersion
OperationId
StateKey
ResultReference
CreatedAtUtc
AppliedAtUtc
FailureCode
RowVersion
```

## 30. Event Application Uniqueness

Recommended:

```text
NarrativeTurnId + ProviderEventId unique
```

and, where appropriate:

```text
NarrativeTurnId + EventSequence unique
```

## 31. Event Application State

Recommended values:

```text
Pending
Accepted
Rejected
Normalized
Deferred
Applying
Applied
RepairRequired
FailedTerminal
Superseded
RecoveryRequired
```

This lifecycle is specific to event-application evidence and does not replace Operation Record state.

## 32. Operation Relationship

Every authoritative event application references one OperationId.

## 33. Result Reference

The event application stores references to created or changed authoritative records.

## 34. Provider Attempt Table

Recommended logical table:

```text
ProviderAttempts
```

## 35. Provider Attempt Fields

```text
ProviderAttemptId
NarrativeTurnId
AttemptNumber
ProviderProfileId
AdapterKey
ModelProfileKey
RequestContractVersion
StateKey
StartedAtUtc
CompletedAtUtc
SafeProviderRequestId
ProviderResponseStagingId
InputTokenCount
OutputTokenCount
RetryClassificationKey
FailureCode
RowVersion
```

Usage counters remain optional and policy-governed.

## 36. Provider Attempt Uniqueness

Recommended:

```text
NarrativeTurnId + AttemptNumber unique
```

## 37. Provider Attempt State

The persisted state follows ADR-0034 and ADR-0036.

No alternate state vocabulary is allowed.

## 38. Provider Profile Reference

ProviderProfileId is installation-scoped operational metadata.

It is not required to interpret Campaign truth.

## 39. Safe Provider Request ID

A safe provider request identifier may be retained when allowed.

It must not become a recovery dependency.

## 40. Provider Token Metadata

Token usage may be retained for diagnostics or cost awareness.

It must not contain content.

## 41. Raw Prompt Exclusion

Full prompts are not retained indefinitely in Stable storage.

## 42. Raw Response Exclusion

Raw provider responses are not retained indefinitely by default.

## 43. Response Staging Table

Recommended logical table:

```text
NarrativeResponseStaging
```

## 44. Response Staging Fields

```text
NarrativeResponseStagingId
ProviderAttemptId
ContractVersionHint
PayloadHash
StorageReference
ByteLength
StateKey
CreatedAtUtc
ExpiresAtUtc
DeletedAtUtc
RowVersion
```

## 45. Response Staging Purpose

Staging exists only to support:

- crash recovery after response receipt;
- validation retry;
- bounded repair;
- explicit diagnostics with consent.

## 46. Response Staging State

Recommended:

```text
Available
ValidationInProgress
Accepted
Rejected
Expired
Deleted
RecoveryRequired
```

## 47. Staging Storage

Large staged content may live in a protected local file with database metadata.

## 48. Staging Protection

Staged content must:

- exclude credentials;
- use restricted local permissions;
- use bounded retention;
- never be uploaded automatically;
- be deleted after policy allows.

## 49. Staging Retention

Default Stable behavior:

- delete after accepted output is durably normalized;
- delete after terminal rejection;
- delete after expiry;
- preserve temporarily only for explicit recovery or diagnostics.

## 50. Dice Roll Table

Recommended logical table:

```text
DiceRolls
```

## 51. Dice Roll Fields

```text
DiceRollId
CampaignId
SessionId
ActId
SceneId
OriginNarrativeTurnId
OriginNarrativeEventApplicationId
ContinuationNarrativeTurnId
OperationId
RuleSetPackageId
RuleSetPackageVersion
RuleOperationKey
RollContractKey
RollContractVersion
StateKey
RequestPayloadReference
OutcomeSummaryReference
CreatedAtUtc
ValidatedAtUtc
ReleasedAtUtc
RawValuesCommittedAtUtc
ResolvedAtUtc
CompletedAtUtc
FailureCode
RowVersion
```

## 52. Dice State

`StateKey` uses RFC-0012 and ADR-0033's canonical lifecycle:

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

## 53. No Legacy Dice State Values

The following legacy alternatives are not persisted:

```text
Requested
Ready
Applied
Presented
Executing
CancelledBeforeExecution
Corrected
```

Any old data using them requires an explicit migration mapping.

## 54. Dice Origin Uniqueness

Recommended unique constraint:

```text
OriginNarrativeTurnId + OriginNarrativeEventApplicationId unique
```

where the Roll originated from Narrative Intelligence.

## 55. Rule Set Binding

Every Dice Roll stores exact package and contract versions.

## 56. Request Payload

Rule Set-specific request data is stored through a versioned typed payload reference.

## 57. No Werewolf Columns

DiceRolls must not contain generic columns such as:

- RageDiceCount;
- SuccessThreshold;
- Specialty;
- BotchCount;
- HungerDiceCount;
- HumanityDifficulty.

Those belong to package payloads where applicable.

## 58. Random Evidence Table

Recommended:

```text
RandomEvidenceItems
```

## 59. Random Evidence Fields

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
RowVersion
```

## 60. Append-Only Evidence

Committed RandomEvidenceItems are immutable.

## 61. Evidence Uniqueness

Recommended:

```text
DiceRollId + GenerationStageKey + GenerationSequence unique
```

## 62. Evidence Identity

Every generated item has a stable strongly typed ID.

## 63. Multiple Dice Groups

`GroupKey` allows one Roll to preserve several groups.

## 64. Mixed Dice Types

`DiceKindKey`, `FaceCount`, and optional symbol data allow different Dice types.

## 65. Reroll Persistence

A reroll creates a new RandomEvidenceItem.

`ParentEvidenceItemId` references the rerolled item.

## 66. Exploding Dice Persistence

An exploding or chained die creates a new item with:

```text
ParentEvidenceItemId
CauseKey = explosion | chain
```

## 67. Keep and Drop

Evidence remains unchanged.

The resolution stage records which evidence contributed.

## 68. Custom Faces

Custom symbols use typed package payload or `RawSymbolValue`.

## 69. No Result Overwrite

A later stage never overwrites earlier raw evidence.

## 70. Dice Resolution Stage Table

Recommended:

```text
DiceResolutionStages
```

## 71. Resolution Stage Fields

```text
DiceResolutionStageId
DiceRollId
StageKey
StageOrdinal
ResolutionContractKey
ResolutionContractVersion
InputEvidenceHash
ResolutionPayloadReference
OutcomeClassificationKey
StatusKey
RequiresAdditionalGeneration
RequiresUserDecision
CreatedAtUtc
ResolvedAtUtc
RowVersion
```

## 72. Resolution Stage Uniqueness

Recommended:

```text
DiceRollId + StageKey unique
```

or:

```text
DiceRollId + StageOrdinal unique
```

according to the package contract.

## 73. Resolution Status

Recommended:

```text
Pending
Resolving
AwaitingAdditionalDice
AwaitingUserDecision
Resolved
Invalidated
RecoveryRequired
FailedTerminal
```

This is stage-level detail and does not replace DiceRoll state.

## 74. Resolution Payload

The payload is versioned and package-owned.

## 75. Input Evidence Hash

This proves which evidence set was used.

## 76. Applied Evidence Table

Where useful, preserve explicit relationships:

```text
DiceResolutionEvidenceUse
```

## 77. Evidence Use Fields

```text
DiceResolutionStageId
RandomEvidenceItemId
UsageKey
ContributionValue
ReasonKey
```

## 78. Usage Keys

Examples:

```text
used
ignored
dropped
kept
rerolled
triggered-explosion
opposed
bonus
penalty
```

## 79. Additional Generation Request

If another generation stage is required, persist a typed request linked to the resolution stage.

## 80. User Decision Record

Future post-Roll decisions may use a dedicated Application record.

The MVP does not require a general Choice entity.

## 81. Dice Correction Table

Recommended:

```text
DiceCorrections
```

## 82. Dice Correction Fields

```text
DiceCorrectionId
DiceRollId
CorrectionOperationId
CorrectionTypeKey
ReasonCode
ReplacementDiceRollId
CreatedAtUtc
CreatedByReference
```

## 83. Non-Destructive Correction

Correction never deletes original evidence.

## 84. Invalidation

Invalidation updates the Dice Roll state and creates correction evidence.

## 85. Replacement Roll

A replacement Roll references the original through DiceCorrection.

## 86. Continuation Relationship

DiceRoll stores the NarrativeTurnId created after resolution.

## 87. Continuation Uniqueness

Recommended:

```text
DiceRollId + continuation-stage-key unique
```

or one `ContinuationNarrativeTurnId` for MVP.

## 88. No Duplicate Continuation

Retrying continuation scheduling returns the existing Narrative Turn.

## 89. Operation Record Relationship

Dice creation, release, generation, resolution, effect application, and correction use Operation Records as appropriate.

## 90. Work Item Relationship

Work Items may reference:

```text
DiceRollId
NarrativeTurnId
ProviderAttemptId
OperationId
```

## 91. Work Item Table Amendment

Recommended additional nullable fields:

```text
WorkItems.NarrativeTurnId
WorkItems.ProviderAttemptId
WorkItems.DiceRollId
```

or a normalized correlation table.

## 92. Operation Record Amendment

Recommended additional nullable fields:

```text
OperationRecords.NarrativeTurnId
OperationRecords.NarrativeEventApplicationId
OperationRecords.DiceRollId
```

or a normalized correlation table.

## 93. Correlation Strategy

Choose one consistent pattern:

```text
Typed nullable foreign keys
```

or:

```text
OperationCorrelations
WorkItemCorrelations
```

Do not mix ad hoc string references across modules.

## 94. Recommended MVP Strategy

Use typed nullable foreign keys for high-value known relationships.

Add generalized correlation tables only when a concrete need appears.

## 95. Foreign Keys

Foreign-key constraints should enforce:

- Campaign ownership;
- Narrative Turn ownership;
- Provider Attempt ownership;
- Dice origin;
- continuation relationship;
- evidence ownership;
- resolution ownership;
- correction ownership.

## 96. Delete Behavior

Authoritative history uses restrictive delete behavior.

## 97. No Cascade Delete of History

Deleting a parent record must not silently erase:

- Messages;
- Dice evidence;
- Provider attempt evidence required for recovery;
- corrections;
- progression;
- Memories.

## 98. Archival Over Deletion

Completed history is archived or corrected rather than destructively deleted.

## 99. Indexes

Recommended indexes include:

```text
NarrativeTurns(CampaignId, StateKey)
NarrativeTurns(SceneId, CreatedAtUtc)
ProviderAttempts(NarrativeTurnId, AttemptNumber)
NarrativeEventApplications(NarrativeTurnId, EventSequence)
Messages(NarrativeTurnId, NarrativeBlockSequence)
DiceRolls(CampaignId, CreatedAtUtc)
DiceRolls(StateKey)
DiceRolls(OriginNarrativeTurnId)
RandomEvidenceItems(DiceRollId, GenerationStageKey, GenerationSequence)
DiceResolutionStages(DiceRollId, StageOrdinal)
WorkItems(StateKey, ScheduledAtUtc)
OperationRecords(StateKey, UpdatedAtUtc)
```

## 100. Concurrency

Optimistic concurrency protects:

- Narrative Turn transitions;
- Provider Attempt transitions;
- event application;
- Dice state transitions;
- resolution-stage transitions;
- Work Item claims;
- Operation Record transitions.

## 101. Short Transactions

External calls and user waits remain outside transactions.

## 102. Narrative Acceptance Transaction

A preferred acceptance transaction persists:

- NarrativeAcceptedOutput;
- accepted Messages;
- NarrativeEventApplications;
- NarrativeTurn state;
- event correlation evidence.

## 103. Event Effect Transactions

Each authoritative event effect uses its own Operation transaction.

## 104. Dice Creation Transaction

Accepted `roll.requested` creates one DiceRoll and origin relationship atomically.

## 105. Dice Generation Transaction

Generation transaction persists:

- transition to Generating where needed;
- RandomEvidenceItems;
- `RawValuesCommitted` state;
- commit timestamp.

## 106. Dice Resolution Transaction

Resolution transaction persists:

- stage result;
- evidence usage;
- outcome;
- proposed/applied effects references;
- `Resolved` or `AwaitingAdditionalDice`.

## 107. Continuation Scheduling Transaction

Persist:

- continuation NarrativeTurn;
- DiceRoll continuation reference;
- Work Item where used;
- state `AwaitingNarrativeContinuation` or `Completed` according to policy.

## 108. Unknown Commit Recovery

Recovery uses unique constraints and direct queries.

## 109. Narrative Recovery Queries

Inspect:

- NarrativeTurn;
- accepted output;
- Messages;
- event applications;
- Provider Attempts;
- staging.

## 110. Dice Recovery Queries

Inspect:

- DiceRoll state;
- RandomEvidenceItems;
- resolution stages;
- effect records;
- continuation NarrativeTurn;
- Operations.

## 111. Work Recovery Queries

Inspect:

- WorkItem;
- OperationRecord;
- claims;
- result references;
- related Narrative or Dice records.

## 112. Recovery Before Retry

No retry may create new authoritative evidence until recovery proves it is absent.

## 113. Migration from Legacy Dice States

If earlier builds persisted old values, migration must map them explicitly.

## 114. Example Legacy Mapping

Potential mapping:

```text
Requested → Proposed
Validated → Validated
Presented → AwaitingPlayerRelease
Executing → Generating or RecoveryRequired
Resolved → Resolved
CancelledBeforeExecution → Cancelled
Corrected → Invalidated with correction record
Applied → AwaitingNarrativeContinuation or Completed
Ready → AwaitingPlayerRelease
```

The exact mapping depends on available evidence and must never guess when outcome is ambiguous.

## 115. Ambiguous Legacy State

Ambiguous records enter:

```text
RecoveryRequired
```

## 116. Provider Attempt Retention

Retain minimal safe metadata for:

- unresolved turns;
- failure diagnosis;
- cost or usage display;
- accepted-output provenance;
- support windows.

## 117. Provider Attempt Compaction

After the recovery and support window:

- remove unnecessary raw references;
- retain safe status and timing where useful;
- preserve accepted-output linkage;
- preserve failure summaries required for audit.

## 118. Response Staging Retention

Raw staging is short-lived.

## 119. Narrative Turn Retention

Narrative Turns correlated to accepted Messages and Dice should remain while their history remains relevant.

## 120. Completed Turn Compaction

A completed turn may have operational details compacted only if:

- accepted Messages remain;
- event effects remain;
- Dice correlations remain;
- no unresolved Work Item exists;
- no correction or audit dependency exists.

## 121. Dice Evidence Retention

Authoritative Dice evidence remains with Campaign history.

## 122. No Routine Dice Evidence Deletion

Do not delete evidence merely to reduce storage.

## 123. Backup Inclusion

Installation backup includes:

- Narrative Turns needed for history or recovery;
- accepted outputs;
- Provider Attempts needed for recovery;
- response staging only when required for unresolved recovery;
- Dice Rolls;
- all random evidence;
- resolution stages;
- corrections;
- Work Items;
- Operation Records.

## 124. Backup Secret Exclusion

Backups exclude raw credentials.

## 125. Portable Campaign Export

Portable Campaign export includes:

- accepted Messages;
- Narrative Turn identity and accepted-output provenance as needed;
- Dice Rolls;
- RandomEvidenceItems;
- resolution stages;
- corrections;
- exact Rule Set package bindings.

## 126. Provider Attempt Export

Completed Provider Attempts are excluded by default from portable Campaign export.

## 127. Response Staging Export

Response staging is excluded.

## 128. Pending Narrative Export

The first export implementation should require a stable boundary with no unresolved critical Narrative or Dice workflow.

## 129. Restricted Portability

If future portability includes pending work, all required Application records must have explicit portable contracts.

## 130. Rule Set Neutrality

The persistence model supports future systems with:

- multiple Dice groups;
- mixed Dice sizes;
- specialties;
- modifiers;
- rerolls;
- explosions;
- keep/drop;
- custom symbols;
- opposed Rolls;
- staged resolution;
- post-Roll decisions.

## 131. No Premature UI Requirement

Persistence support does not require the MVP UI to expose all future mechanics.

## 132. Privacy Classification

Recommended classes:

```text
AuthoritativeCampaign
ApplicationOperational
ProviderOperational
SensitiveTransient
DiagnosticSafe
```

## 133. Classification Examples

```text
Message
    AuthoritativeCampaign

Dice evidence
    AuthoritativeCampaign

NarrativeTurn
    ApplicationOperational

ProviderAttempt
    ProviderOperational

Response staging
    SensitiveTransient
```

## 134. Raw Credentials

Raw credentials never enter these tables.

## 135. Secret References

Opaque secret references may exist only in provider profile configuration, not Campaign portability records.

## 136. Logging

Persistence logs may include:

- typed IDs;
- state transitions;
- row counts;
- version keys;
- package IDs;
- safe hashes;
- durations;
- safe error codes.

They must not log full prompts, raw responses, credentials, or full private Campaign content.

## 137. Error Model

Recommended errors:

```text
persistence.narrative-turn-conflict
persistence.narrative-output-duplicate
persistence.provider-attempt-duplicate
persistence.response-staging-expired
persistence.event-application-duplicate
persistence.dice-origin-duplicate
persistence.dice-state-invalid
persistence.random-evidence-conflict
persistence.random-evidence-immutable
persistence.resolution-stage-conflict
persistence.continuation-duplicate
persistence.correction-invalid
persistence.foreign-key-invalid
persistence.unknown-commit
persistence.recovery-required
```

## 138. Data Preservation State

Results should state:

```text
AuthoritativeDataUnchanged
NarrativeTurnPersisted
ProviderAttemptPersisted
ResponseStaged
NarrativeAccepted
MessagesCommitted
EventsCorrelated
DiceRollPersisted
RawEvidenceCommitted
ResolutionCommitted
ContinuationCorrelated
OriginalEvidencePreserved
RecoveryRequired
```

## 139. Testing Strategy

The implementation requires:

```text
Schema Tests
Constraint Tests
State Persistence Tests
Narrative Correlation Tests
Provider Attempt Tests
Response Staging Tests
Dice Evidence Tests
Resolution Stage Tests
Correction Tests
Concurrency Tests
Recovery Tests
Migration Tests
Backup Tests
Export Tests
Privacy Tests
Cross-System Tests
```

## 140. Schema Tests

Tests verify every required table, field, foreign key, and index.

## 141. Constraint Tests

Tests cover:

- one accepted output;
- unique provider attempt number;
- unique event application;
- unique Dice origin;
- unique evidence sequence;
- unique resolution stage;
- unique continuation.

## 142. Narrative Correlation Tests

Tests prove:

- accepted Messages reference the correct turn;
- events reference the accepted output;
- Operations reference events;
- Dice reference origin events;
- continuation references the resolved Roll.

## 143. Provider Attempt Tests

Tests cover:

- success;
- retry;
- late response;
- supersession;
- raw staging cleanup;
- accepted-output provenance.

## 144. Dice Evidence Tests

Tests prove:

- append-only evidence;
- mixed Dice sizes;
- several groups;
- reroll links;
- explosion links;
- keep/drop evidence preservation;
- no overwrite.

## 145. Resolution Stage Tests

Tests cover:

- one-stage resolution;
- additional Dice;
- post-Roll decision;
- opposed resolution;
- deterministic retry;
- exact package version.

## 146. Correction Tests

Tests prove:

- invalidation preserves original;
- replacement links to original;
- no destructive delete;
- correction operation is persisted.

## 147. Concurrency Tests

Tests cover simultaneous:

- turn acceptance;
- provider response;
- Dice release;
- evidence generation;
- resolution;
- continuation scheduling;
- Work Item claim;
- Operation recovery.

## 148. Recovery Tests

Inject failure:

- after provider response receipt;
- during acceptance;
- after Message commit;
- after Dice creation;
- during evidence commit;
- after evidence commit;
- during resolution;
- after resolution;
- before continuation;
- after continuation commit.

No duplicate authoritative record may appear.

## 149. Migration Tests

Tests cover every legacy Dice state mapping and ambiguous recovery case.

## 150. Backup Tests

Tests prove all required recovery records and Dice evidence survive backup and restore.

## 151. Export Tests

Tests prove:

- Dice evidence and exact package version round-trip;
- Provider Attempts and staging are excluded by default;
- stable Campaign history remains understandable;
- complex non-Werewolf Dice evidence survives.

## 152. Privacy Tests

Synthetic credentials, hidden prompts, and provider raw payloads must not appear in:

- authoritative Campaign tables;
- portable export;
- Stable logs;
- unbounded retention.

## 153. Cross-System Test

A synthetic non-Werewolf Rule Set persists and restores:

- d20 group;
- d6 group;
- specialty metadata;
- modifier;
- reroll;
- explosion;
- keep-highest;
- opposed group;
- post-Roll decision;
- final resolution.

No Core schema change is allowed.

## 154. Architecture Tests

Architecture tests must reject:

- direct EF serialization as portable contract;
- Werewolf-specific Core columns;
- fixed d10-only evidence table;
- mutable raw evidence;
- ProviderAttempt treated as Campaign truth;
- raw response retained indefinitely;
- Message without NarrativeTurnId when Narrator-generated;
- Dice Roll without origin correlation;
- continuation without unique correlation;
- Work Item or Operation using untyped string-only references where typed correlation is required;
- legacy Dice state enum reintroduced.

## 155. Prohibited Patterns

### 155.1 Final Outcome Without Evidence

Preserve raw evidence.

### 155.2 One Dice Row with Serialized History Blob

Use explicit evidence and stage records.

### 155.3 Provider Response as Authoritative Message

Persist accepted Messages separately.

### 155.4 Provider Thread as Recovery Source

Recover from Chronicle records.

### 155.5 Mutable Reroll Value

Append linked evidence.

### 155.6 Direct Package-Specific Columns

Use versioned package payloads.

### 155.7 Indefinite Raw Response Retention

Use bounded staging.

### 155.8 Delete Invalid Roll

Preserve and correct.

### 155.9 Duplicate Continuation

Use unique correlation and OperationId.

### 155.10 Guess Legacy State

Use RecoveryRequired when evidence is ambiguous.

## 156. Alternatives Considered

### Persist Only Messages and Final Dice Result

Rejected because recovery, audit, correction, and complex mechanics would be impossible.

### Store Provider Raw Response Forever

Rejected because of privacy and retention cost.

### Use One Generic JSON Blob for All Dice

Rejected because evidence identity, immutability, ordering, and causal links require explicit structure.

### Put Narrative Turn in Domain

Rejected because it is Application orchestration, not fictional truth.

### Use Provider Attempt as Work Item

Rejected because execution attempt, scheduling unit, and logical turn are distinct.

### Recompute Historical Resolution on Load

Rejected because exact package version and committed result must remain preserved.

## 157. Consequences

### Positive

- persistence now matches canonical Dice lifecycle;
- Narrative workflow becomes restart-safe;
- provider attempts are explicit but nonauthoritative;
- accepted prose and events are traceable;
- complex future Dice systems fit without schema redesign;
- recovery has durable evidence;
- backup and export semantics are clear;
- privacy retention is bounded;
- no Werewolf coupling enters Core.

### Negative

- more tables and constraints;
- migration complexity increases;
- retention policies are required;
- event and turn correlations require careful transactions;
- response staging requires secure cleanup;
- tests become broader.

## 158. Risks

### Workflow Tables Become Too Large

Mitigation:

- bounded operational retention;
- preserve authoritative correlations;
- compact completed operational detail safely.

### Generic Payloads Become Opaque

Mitigation:

- contract registry;
- schema validation;
- exact version binding;
- explicit evidence tables.

### Recovery Logic Diverges from State Machines

Mitigation:

- one canonical registry;
- generated transition tests;
- shared persistence mappings.

### Raw Response Leakage

Mitigation:

- transient staging;
- restricted permissions;
- expiry;
- no automatic upload;
- privacy tests.

### First Rule Set Shapes Schema

Mitigation:

- synthetic cross-system fixture;
- no package-specific columns;
- package-owned payload contracts.

## 159. Technology Spike

Before acceptance, implement:

1. NarrativeTurns table;
2. NarrativeAcceptedOutputs table;
3. NarrativeEventApplications table;
4. ProviderAttempts table;
5. NarrativeResponseStaging table;
6. DiceRolls table with canonical states;
7. RandomEvidenceItems table;
8. DiceResolutionStages table;
9. DiceResolutionEvidenceUse table;
10. DiceCorrections table;
11. Message correlations;
12. Work Item correlations;
13. Operation Record correlations;
14. unique constraints;
15. indexes;
16. optimistic concurrency;
17. retention cleanup;
18. legacy state migration;
19. backup and export tests;
20. fault-injection tests;
21. cross-system Dice fixture.

## 160. Spike Acceptance

The spike passes when:

- one Narrator turn survives restart;
- one accepted output creates ordered Messages;
- one Provider Attempt is correlated safely;
- one Roll is created from one event;
- raw evidence commits once;
- mixed Dice groups persist;
- reroll and explosion links persist;
- resolution stage persists;
- continuation is unique;
- duplicate provider output creates no duplicate Message or Roll;
- ambiguous legacy state enters RecoveryRequired;
- backup restores unresolved workflow;
- portable export preserves Dice but excludes raw staging;
- a complex non-Werewolf Roll round-trips without Core schema changes.

## 161. Definition of Compliance

An implementation complies when:

- Narrative Turns are durable Application records;
- accepted output, Messages, events, attempts, Operations, Work Items, and Dice are explicitly correlated;
- Provider Attempts remain nonauthoritative;
- raw provider content uses bounded staging;
- Dice uses the canonical lifecycle;
- random evidence is immutable and append-only;
- multiple groups, mixed Dice, rerolls, explosions, keep/drop, opposed Rolls, and staged resolution fit the model;
- exact Rule Set package and contract versions are preserved;
- retries do not duplicate evidence or effects;
- historical Rolls are never recomputed silently;
- backup preserves recovery state;
- export preserves authoritative evidence and excludes operational raw content by default;
- no Core table assumes Werewolf.

## 162. Review Triggers

Review this RFC if:

- server-hosted authority is introduced;
- multiplayer requires concurrent Dice ownership;
- raw response retention policy changes;
- encrypted database storage is introduced;
- package-defined storage extensions are proposed;
- provider-native threads become required;
- physical Dice become authoritative;
- cross-device synchronization is introduced;
- large-scale archival compaction is introduced.

## 163. Deferred Decisions

Later decisions may define:

- encrypted transient staging;
- provider-attempt archival tiers;
- server-side persistence;
- distributed correlation;
- package-specific auxiliary tables;
- physical Dice evidence;
- cryptographic Dice commitments;
- cross-device sync;
- historical cold storage.

## 164. Final Decision

Chronicle will persist the complete chain from proposal to accepted truth:

```text
Narrative Turn
    → Provider Attempt
    → Accepted Output
    → Messages and Structured Events
    → Operations and Work Items
    → Dice Request
    → Random Evidence
    → Rule Set Resolution
    → Narrative Continuation
```

Provider attempts may disappear from long-term operational detail.

Accepted history and Dice evidence may not.

The first Rule Set will not dictate the schema.

Chronicle persistence will be ready for the systems that come after it.
