---
id: ADR-0031
title: Character Progression Ledger and Advancement Application Pipeline
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
  - ADR-0005
  - ADR-0009
  - ADR-0010
  - ADR-0013
  - ADR-0014
  - ADR-0017
  - ADR-0018
  - ADR-0019
  - ADR-0023
  - ADR-0024
  - ADR-0028
  - ADR-0029
  - ADR-0030
  - RFC-0005
  - RFC-0006
  - RFC-0007
  - RFC-0010
  - RFC-0013
  - RFC-0015
  - RFC-0016
  - RFC-0017
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
  - RFC-0040
  - RFC-0042
---

> **"Progression is not an edit to a Character Sheet. It is a proven exchange, recorded as history, that changes what the Character is allowed to become."**

# Character Progression Ledger and Advancement Application Pipeline

## 1. Status

**Proposed**

This ADR defines Chronicle's progression ledger, earned and spent advancement resources, advancement validation, cost calculation, application, reversal, and audit behavior.

The decision is:

- represent progression as an append-only ledger;
- separate earned resources, spent resources, adjustments, and advancement applications;
- never treat a mutable balance field as the only source of truth;
- compute or transactionally project balances from ledger entries;
- require every advancement to pass an exact-version Rule Operation;
- preserve package, schema, Preference, Character, and operation versions;
- apply advancement changes atomically with ledger entries and Character field history;
- use OperationId and unique constraints to prevent duplicate application;
- prohibit Narrative Intelligence from awarding or spending progression directly;
- allow providers and the Archivist to propose progression only through structured requests;
- distinguish automatic Session awards from player-selected advancements;
- make finalization awards and later purchases separate operations;
- preserve negative corrections and administrative recovery as explicit ledger entries;
- prohibit destructive editing of historical entries;
- support deterministic rollback through compensating entries rather than deletion;
- reject stale Character or balance state before commit;
- keep Rule Set mechanics outside the write transaction and recheck state before application;
- maintain enough evidence to explain every current mechanical value affected by advancement.

The decision becomes **Accepted** after a progression spike proves:

- Session-earned progression award;
- current available-balance projection;
- advancement-cost calculation;
- prerequisite validation;
- successful field advancement;
- insufficient-resource rejection;
- stale-state rejection;
- duplicate OperationId replay;
- atomic Character and ledger persistence;
- compensating correction;
- package-version evidence;
- deterministic rebuild of balance from ledger;
- recovery after crash near commit.

## 2. Context

RPG systems model Character growth in different ways.

Examples include:

- experience points;
- freebie points;
- renown;
- milestones;
- training points;
- skill dots;
- level advancement;
- Session awards;
- story awards;
- package-specific currencies;
- prerequisites;
- escalating costs;
- special permissions;
- temporary discounts.

Chronicle must support system-specific advancement without losing auditability.

A naive implementation might store:

```text
Experience = 14
Strength = 3
```

and overwrite those values when advancement occurs.

That approach cannot reliably answer:

- how the Character earned the points;
- how much an advancement cost;
- which Rule Set version calculated the cost;
- whether the same purchase was applied twice;
- whether a correction occurred;
- which Session granted the award;
- whether a package upgrade changed future cost rules;
- what happened before a crash;
- why the current field value is valid.

Chronicle therefore needs an append-only progression ledger integrated with Rule Operations and Character field history.

## 3. Decision Drivers

The progression model prioritizes:

1. auditability;
2. idempotency;
3. deterministic cost calculation;
4. exact package-version behavior;
5. atomic Character mutation;
6. recoverability;
7. no silent balance edits;
8. package extensibility;
9. clear Session-finalization integration;
10. player control;
11. stale-state protection;
12. historical explanation.

## 4. Decision Summary

Chronicle will use:

```text
Progression Source of Truth
    append-only ledger

Entry Types
    Earn
    Spend
    Adjustment
    Refund
    Transfer where package permits
    AdvancementApplied
    Correction

Balance
    derived or transactionally projected

Advancement Validation
    exact Rule Set package operation

Application
    one Application command
    one transaction
    ledger + Character field history + result

Idempotency
    OperationId
    unique constraints

Rollback
    compensating entry
    no destructive deletion

Narrative Intelligence
    proposal only
```

## 5. Progression Domain

Progression includes:

```text
earned resources
available balance
spent resources
advancement eligibility
advancement cost
field changes
prerequisites
discounts
special permissions
history
```

## 6. Progression Currency

A Rule Set package defines one or more progression currencies.

Examples:

```text
experience
freebie
renown.glory
renown.honor
renown.wisdom
milestone
```

## 7. Currency Identity

Every currency uses a stable semantic key.

## 8. Currency Definition

A package currency definition SHOULD contain:

```text
CurrencyKey
LabelKey
DescriptionKey
ValueType
MinimumBalance
CanGoNegative
CanTransfer
CanExpire
AwardOperationKey
SpendOperationKey
DisplayPrecision
```

## 9. Exact Numeric Type

Progression values use exact numeric representation.

Preferred:

- integer units;
- decimal with explicit precision;
- bounded rational representation only if required.

Floating-point values are prohibited.

## 10. Ledger

The progression ledger is append-only.

Each entry records one accepted progression fact.

## 11. Ledger Entry Identity

Every entry has:

```text
ProgressionEntryId
```

## 12. Ledger Entry Types

Recommended entry types:

```text
Earned
Spent
Refunded
Adjusted
Corrected
TransferredIn
TransferredOut
Expired
AdvancementApplied
AdvancementReverted
```

The package may restrict which types are supported.

## 13. Earned Entry

An earned entry increases available progression resources.

Typical sources:

- Session finalization;
- story milestone;
- manual Director award;
- package-defined achievement;
- import;
- administrative recovery.

## 14. Spent Entry

A spent entry records resource consumption for an advancement.

## 15. Adjustment Entry

An adjustment explicitly changes progression balance without pretending an original event never happened.

## 16. Correction Entry

A correction references an earlier entry and compensates for an error.

## 17. Refund Entry

A refund returns previously spent progression according to a supported policy.

## 18. Expiration Entry

If a package supports expiring currency, expiration is an explicit ledger event.

It is not inferred only from current time.

## 19. Transfer

Transfers between currencies are permitted only when the Rule Set declares a deterministic conversion operation.

## 20. Ledger Entry Fields

Recommended fields:

```text
ProgressionEntryId
CampaignId
CharacterId
CurrencyKey
EntryType
Amount
BalanceEffect
SourceType
SourceReferenceId
ReasonKey
OperationId
RuleSetPackageId
RuleSetPackageVersion
RuleOperationKey
RuleOperationVersion
CharacterVersionBefore
CharacterVersionAfter
CreatedAtUtc
RelatedProgressionEntryId
MetadataContractVersion
```

## 21. Source Types

Recommended values:

```text
SessionFinalization
ManualAward
AdvancementPurchase
Import
SchemaMigration
PackageMigration
AdministrativeRecovery
Correction
Refund
Expiration
```

## 22. No Mutable Balance as Sole Truth

A current balance column may exist as a projection.

It is not the only source of truth.

## 23. Balance Reconstruction

Chronicle must be able to reconstruct a balance from valid ledger entries.

## 24. Balance Projection

For performance, Chronicle MAY maintain:

```text
CharacterProgressionBalance
```

transactionally with the ledger.

## 25. Projection Fields

Recommended fields:

```text
CharacterId
CurrencyKey
EarnedTotal
SpentTotal
AdjustmentTotal
AvailableBalance
LastProgressionSequence
ProjectionVersion
```

## 26. Projection Verification

Diagnostics and tests MUST compare projected balance with ledger reconstruction.

## 27. Balance Drift

If the projection differs from the ledger:

- normal spending is blocked;
- a rebuild may occur;
- Safe Mode or recovery is used when authority is uncertain.

## 28. Ledger Ordering

Entries use a stable Character-scoped sequence.

## 29. Sequence Constraint

A unique constraint protects:

```text
CharacterId + ProgressionSequence
```

## 30. OperationId Constraint

Once-only progression effects use unique OperationId constraints.

## 31. Advancement Definition

An advancement is an explicit proposal to change one or more Character fields according to Rule Set progression rules.

## 32. Advancement Examples

Examples:

- increase Strength from 2 to 3;
- purchase a new ability;
- learn a power;
- increase resource maximum;
- add a specialty;
- improve renown;
- unlock a package-specific trait.

## 33. Advancement Request

Recommended request fields:

```text
CharacterId
ExpectedCharacterVersion
CurrencyKey
AdvancementKey
TargetFieldKey
CurrentValue
RequestedValue
SelectedOptions
ExpectedAvailableBalance
OperationId
```

## 34. Advancement Key

Each advancement type uses a stable semantic key.

## 35. Player-Selected Advancement

Player-selected advancement is initiated through UI and Application command.

## 36. Automatic Award

Automatic awards are separate from spending.

Session finalization may award resources without immediately spending them.

## 37. No Forced Spending During Finalization

The MVP SHOULD NOT require progression spending during Session finalization unless the Rule Set makes it unavoidable.

## 38. Finalization Award Operation

The Rule Set may expose:

```text
calculate-session-progression-award
```

## 39. Award Proposal

The Archivist or finalization workflow may propose:

- eligible awards;
- evidence;
- package-specific award categories.

Chronicle validates them through a Rule Operation.

## 40. Provider Cannot Award Directly

Narrative Intelligence cannot append progression entries directly.

## 41. Advancement Pipeline

Recommended flow:

```text
Receive Advancement Request
    ↓
Load Character and Progression Snapshot
    ↓
Resolve Exact Rule Set Package
    ↓
Validate Input and Preferences
    ↓
Execute Advancement Validation Rule Operation
    ↓
Execute Cost Calculation Rule Operation
    ↓
Validate Mechanical Proposal
    ↓
Acquire Character Mutation Coordination
    ↓
Reload and Recheck Versions and Balance
    ↓
Apply Character Field Changes
    ↓
Append Spend and Advancement Ledger Entries
    ↓
Append Character Field History
    ↓
Update Balance Projection
    ↓
Persist Operation Result
    ↓
Commit
```

## 42. Validation Operation

Recommended operation:

```text
validate-advancement
```

## 43. Cost Operation

Recommended operation:

```text
calculate-advancement-cost
```

## 44. Combined Operation

A package MAY combine validation and cost into one typed operation when the result contract remains explicit.

## 45. Validation Input

May include:

- Character mechanical snapshot;
- requested target;
- current value;
- package Preferences;
- progression balance;
- prior progression history summary;
- prerequisites;
- Session or story evidence where required.

## 46. Validation Output

Recommended fields:

```text
Eligible
BlockingIssues
Warnings
RequiredCurrency
Cost
Prerequisites
FieldChanges
LedgerEffects
MechanicalExplanationKeys
```

## 47. Cost Is Rule Set-Owned

The Rule Set calculates the mechanical cost.

Chronicle validates bounds, currency, balance, scope, and state version.

## 48. Chronicle Does Not Recalculate Package Cost

Chronicle should not duplicate every package cost formula.

It validates authority and invariants.

## 49. Cost Bounds

Chronicle validates:

- nonnegative cost unless contract explicitly allows otherwise;
- supported currency;
- exact numeric type;
- maximum safe amount;
- no overflow;
- no unauthorized transfer.

## 50. Insufficient Balance

If spending would exceed allowed balance:

```text
progression.insufficient-balance
```

## 51. Negative Balance

Negative balances are prohibited unless the package explicitly permits them.

## 52. Prerequisite Validation

Prerequisites must reference:

- current Character fields;
- progression history;
- package concepts;
- explicit story flags;
- Preferences.

## 53. Narrative Prerequisite

A package may require a story prerequisite.

It must be represented as an authoritative Chronicle fact or approved evidence key.

Provider prose alone is insufficient.

## 54. Story Evidence

Examples:

```text
completed.training.ritual
earned.rank.fostern
mentor.approval
campaign.milestone.chapter-one
```

## 55. Evidence Identity

Evidence uses stable semantic keys and source references.

## 56. Field Changes

An advancement result proposes bounded field changes compatible with ADR-0030.

## 57. Multi-Field Advancement

An advancement may atomically change several fields.

Example:

- add power;
- reduce experience;
- increase derived maximum;
- append progression history.

## 58. Partial Advancement

Partial acceptance is prohibited.

## 59. Character Mutation

Field changes apply through Character Domain methods.

## 60. Ledger Mutation

Ledger entries append in the same transaction as Character changes.

## 61. Atomicity

Character field changes, spend entries, advancement record, history, projection, and Operation Record commit atomically.

## 62. Advancement Record

Chronicle SHOULD persist a dedicated advancement record.

Recommended fields:

```text
AdvancementId
CharacterId
AdvancementKey
TargetFieldKey
PreviousValue
NewValue
Cost
CurrencyKey
RuleSetPackageVersion
RuleOperationKey
OperationId
AppliedAtUtc
SourceContext
```

## 63. Advancement Versus Spend Entry

The advancement record explains what changed.

The spend entry explains the progression resource effect.

Both reference the same OperationId.

## 64. Free Advancement

A zero-cost advancement still creates an advancement record.

It may omit a spend entry or create a zero-effect entry according to ledger policy.

## 65. Discount

Discounts are explicit in Rule Set output.

Recommended evidence:

```text
BaseCost
Discounts
FinalCost
DiscountReasonKeys
```

## 66. Cost Increase

Surcharges are also explicit.

## 67. Preferences

Progression calculations receive a validated Preference snapshot.

## 68. Preference Version

The accepted result records the Preference version.

## 69. Package Version

The accepted result records the exact package and operation version.

## 70. Historical Cost

A later package upgrade does not retroactively change the cost of an accepted advancement.

## 71. Historical Recalculation

Diagnostics may compare old and new rule outcomes.

They do not rewrite history automatically.

## 72. Stale State

If Character version or balance changed before commit:

- the proposal is rejected as stale;
- no progression entry is appended;
- the request may be recalculated;
- OperationId semantics remain explicit.

## 73. Stale Request Error

Recommended:

```text
progression.state-stale
```

## 74. Recalculation After Stale State

Because Rule Operations are deterministic, Chronicle may recompute from fresh state before any authoritative commit.

## 75. No Open Write Transaction During Calculation

Rule validation and cost calculation occur before the write transaction.

## 76. Commit Recheck

Inside the write transaction, Chronicle rechecks:

- Character version;
- balance projection version;
- relevant prerequisite evidence;
- package binding;
- schema version.

## 77. Duplicate OperationId

If the OperationId already completed:

- return the persisted advancement result;
- do not spend again;
- do not reapply fields.

## 78. Conflicting OperationId

Same OperationId with a different fingerprint returns conflict.

## 79. Commit Unknown

After uncertain commit:

- inspect Operation Record;
- inspect advancement unique constraint;
- inspect progression ledger OperationId;
- never repeat blindly.

## 80. Progression Award Pipeline

Recommended flow:

```text
Receive Award Proposal
    ↓
Resolve Award Rule Operation
    ↓
Validate Source and Evidence
    ↓
Calculate Award
    ↓
Recheck Session Finalization State
    ↓
Append Earned Entries
    ↓
Update Projection
    ↓
Persist Finalization Result
    ↓
Commit
```

## 81. Session Finalization

Session progression awards belong to Session finalization.

They are applied once.

## 82. Finalization Idempotency

A unique constraint or one-finalization rule prevents duplicate Session awards.

## 83. Manual Award

Manual awards require:

- explicit user intent;
- reason;
- currency;
- amount;
- Character;
- OperationId;
- package validation where applicable.

## 84. Manual Award Policy

The MVP may permit manual awards only through a Director or recovery-oriented UI.

## 85. Adjustment

An adjustment must include:

- reason;
- source;
- OperationId;
- prior balance;
- resulting balance;
- user confirmation where destructive.

## 86. No Direct Balance Editor

The UI MUST NOT expose an unrestricted “set current experience” field as the normal workflow.

## 87. Administrative Recovery

Recovery may create a correction entry after:

- projection drift;
- import repair;
- package migration issue;
- user-confirmed historical correction.

## 88. Compensating Entry

To reverse an advancement:

1. validate reversibility;
2. calculate refund or penalty;
3. apply compensating Character field changes;
4. append reversal advancement record;
5. append refund or adjustment entry;
6. preserve original records.

## 89. No Deletion of Advancement History

Accepted advancement and ledger entries are never deleted in ordinary workflows.

## 90. Irreversible Advancement

Some advancements may be irreversible under the Rule Set.

The package validation operation determines this.

## 91. Reversal Operation

Recommended operation:

```text
validate-advancement-reversal
```

## 92. Refund Calculation

Recommended operation:

```text
calculate-advancement-refund
```

## 93. Refund Policy

Refund may be:

- full;
- partial;
- none;
- package-specific;
- administrative only.

## 94. Reversal Atomicity

Reversal field changes and compensating ledger entries commit atomically.

## 95. Import

Campaign import preserves progression history.

Clone import remaps progression entry IDs, Character IDs, and related references.

## 96. Export

Campaign export includes progression ledger and advancement records required to explain Character state.

## 97. Schema Migration

Character schema migration preserves progression evidence.

If a field is retired, related advancements remain historical.

## 98. Package Upgrade

Package upgrade may require progression compatibility validation.

## 99. Pending Advancement

Pending advancement workflows must resolve under the exact package version or be migrated explicitly.

## 100. Derived Values

Advancement-induced field changes trigger derived-value invalidation or recalculation according to ADR-0030.

## 101. Projection Rebuild

Balance projection may be rebuilt from ledger.

## 102. Rebuild Work Item

A large rebuild MAY use a durable Work Item.

## 103. Projection Rebuild Safety

Rebuild must:

- read the immutable ledger;
- calculate deterministically;
- compare with current projection;
- publish atomically;
- avoid changing ledger history.

## 104. Ledger Integrity Rules

Recommended rules:

```text
Amount has exact type
Currency exists
Character belongs to Campaign
Sequence unique
OperationId unique where once-only
Related entry exists when required
Balance policy respected
Package version recorded
```

## 105. Cross-Currency Spend

Spending one currency to buy an advancement priced in another is prohibited unless an explicit conversion operation exists.

## 106. Conversion Ledger

A conversion creates matched transfer-out and transfer-in entries with one OperationId.

## 107. Progression Read Model

Queries return purpose-built projections.

## 108. Progression Summary

Recommended fields:

```text
CurrencyKey
EarnedTotal
SpentTotal
AvailableBalance
PendingAdjustments
LastEntrySequence
ProjectionStatus
```

## 109. Advancement History Projection

Recommended fields:

```text
AdvancementId
AdvancementKey
Target
PreviousValue
NewValue
Cost
Currency
AppliedAtUtc
Source
PackageVersion
CanRequestReversal
```

## 110. UI Workflow

The advancement UI SHOULD show:

- current value;
- requested value;
- prerequisites;
- base cost;
- discounts or surcharges;
- final cost;
- available balance;
- resulting balance;
- affected fields;
- warnings;
- confirmation.

## 111. Preview

Advancement preview is nonauthoritative.

It records the expected Character and balance versions.

## 112. Preview Expiration

A preview becomes stale when:

- Character changes;
- balance changes;
- Preferences change;
- package version changes;
- schema changes.

## 113. Confirmation

Confirmation submits a command with expected versions and OperationId.

## 114. Accessibility

Progression UI must support:

- keyboard operation;
- screen-reader explanations;
- noncolor affordability state;
- explicit before-and-after values;
- accessible validation messages.

## 115. Localization

Currency, advancement, and issue labels use localization keys.

Stable semantic keys remain persisted.

## 116. Provider Context

Narrative Intelligence may receive a bounded progression summary when relevant.

It should not receive unrestricted historical ledger details unless necessary.

## 117. Provider Proposals

Providers may propose:

- “award experience”;
- “eligible advancement”;
- “progression opportunity.”

Chronicle converts these into structured proposals and validates them.

## 118. Provider Cannot Choose Final Cost

The Rule Set calculates final cost.

## 119. Archivist Role

The Archivist may propose Session awards during finalization.

It does not persist them directly.

## 120. Director Role

The Chronicle Director orchestrates award and advancement workflows.

It does not calculate costs.

## 121. Error Model

Recommended errors:

```text
progression.currency-not-found
progression.amount-invalid
progression.insufficient-balance
progression.negative-balance-not-allowed
progression.advancement-not-found
progression.advancement-ineligible
progression.prerequisite-missing
progression.cost-invalid
progression.state-stale
progression.version-conflict
progression.duplicate-operation
progression.projection-stale
progression.projection-corrupt
progression.reversal-not-allowed
progression.refund-invalid
progression.package-unavailable
progression.recovery-required
```

## 122. Data Preservation State

Results SHOULD state:

```text
CharacterUnchanged
LedgerUnchanged
AdvancementApplied
LedgerEntryAppended
ProjectionUpdated
CompensatingEntryAppended
RecoveryRequired
```

## 123. Logging

Logs MAY include:

- CharacterId;
- CampaignId;
- currency key;
- advancement key;
- amount;
- OperationId;
- package version;
- result code;
- Character version;
- duration.

They MUST NOT include unrelated private Character fields or narrative content.

## 124. Metrics

Useful metrics include:

```text
ProgressionAwardCount
AdvancementAppliedCount
AdvancementRejectedCount
InsufficientBalanceCount
ProgressionConflictCount
ProjectionRebuildCount
ProgressionCorrectionCount
AdvancementCalculationDuration
```

## 125. Testing Strategy

The implementation requires:

```text
Ledger Unit Tests
Rule Operation Tests
Persistence Tests
Concurrency Tests
Idempotency Tests
Projection Tests
Migration Tests
Import and Export Tests
Security Tests
Architecture Tests
```

## 126. Ledger Tests

Tests MUST cover:

- earn;
- spend;
- refund;
- adjustment;
- correction;
- transfer;
- sequence;
- reconstruction;
- projection comparison.

## 127. Advancement Tests

Tests MUST cover:

- valid increase;
- invalid target;
- missing prerequisite;
- exact cost;
- discount;
- surcharge;
- free advancement;
- multi-field change;
- package-specific rule.

## 128. Balance Tests

Tests MUST cover:

- exact zero;
- sufficient balance;
- insufficient balance;
- negative allowed;
- negative prohibited;
- overflow;
- decimal precision where supported.

## 129. Atomicity Tests

Tests MUST prove:

- field update and spend entry commit together;
- failure rolls back both;
- projection and ledger remain consistent;
- field history and advancement record agree.

## 130. Idempotency Tests

Tests MUST cover:

- same OperationId same request;
- same OperationId different request;
- crash after commit;
- retry after unknown result;
- duplicate Session award.

## 131. Stale-State Tests

Tests MUST cover:

- Character changed after preview;
- balance changed after preview;
- Preference changed;
- package changed;
- schema changed;
- revalidation before commit.

## 132. Reversal Tests

Tests MUST cover:

- reversible advancement;
- partial refund;
- no refund;
- irreversible advancement;
- compensating entries;
- history preserved.

## 133. Projection Tests

Tests MUST cover:

- rebuild from ledger;
- corrupted projection detection;
- deterministic rebuild;
- read blocked during uncertain authority;
- large ledger performance.

## 134. Import and Export Tests

Tests MUST prove:

- preserve-identity progression history;
- clone remapping;
- no duplicate OperationIds where remapping is required;
- balance equivalence;
- advancement evidence preserved.

## 135. Provider Boundary Tests

Tests MUST prove:

- provider cannot append ledger;
- provider-proposed award requires validation;
- provider-proposed cost is ignored as authority;
- no direct Character progression mutation.

## 136. Required Test Cases

Tests MUST cover:

- Session award;
- manual award;
- Character advancement;
- multiple currencies;
- package-specific prerequisite;
- exact version resolution;
- insufficient balance;
- duplicate operation;
- stale preview;
- crash recovery;
- correction;
- refund;
- irreversible advancement;
- schema migration;
- package upgrade;
- ledger rebuild;
- projection drift;
- no direct balance edit.

## 137. Architecture Tests

Architecture tests MUST reject:

- mutable balance as sole progression truth;
- direct deletion of progression entries;
- provider writing progression;
- UI calculating authoritative cost;
- Rule Set persisting ledger entries;
- advancement field mutation without ledger entry;
- ledger entry without OperationId for once-only effect;
- nonexact numeric progression type;
- silent negative balance;
- open write transaction during cost calculation.

## 138. Prohibited Patterns

### 138.1 Set Experience Balance Directly

Append an explicit ledger entry.

### 138.2 Delete a Mistaken Entry

Append a correction.

### 138.3 Apply Character Field Before Spend

Commit both atomically.

### 138.4 Provider Awards Progression Directly

Provider output is proposal only.

### 138.5 UI Owns Cost Formula

Rule Set Rule Operation owns it.

### 138.6 Recalculate Historical Cost Under New Package

Preserve accepted evidence.

### 138.7 Partial Multi-Field Advancement

The intention commits or rolls back.

### 138.8 Retry Advancement Blindly

Inspect OperationId and persisted result.

### 138.9 Use Floating-Point Currency

Use exact numeric representation.

### 138.10 Force Negative Balance Without Package Policy

Reject the operation.

## 139. Alternatives Considered

### Store Only Current Balance

Rejected because it cannot explain history or prevent duplicate application reliably.

### Event Source the Entire Character

Too broad for the MVP.

Chronicle uses a progression-specific append-only ledger plus current Character state.

### Cost Tables Only

Useful for some systems, but insufficient for prerequisites, discounts, Preferences, and nonlinear rules.

### Provider-Calculated Advancement

Rejected because provider output is nondeterministic and advisory.

### Delete and Recreate Advancement on Correction

Rejected because historical evidence must remain visible.

### Apply Advancement During Session Finalization Only

Rejected because users may choose purchases later and many systems separate earning from spending.

## 140. Consequences

### Positive

- every progression change is explainable;
- balance can be rebuilt;
- duplicate spending is prevented;
- package-version mechanics remain auditable;
- corrections preserve history;
- Session awards and purchases remain distinct;
- Character and progression state commit atomically;
- provider authority remains constrained.

### Negative

- ledger and projection require more tables and code;
- reversal logic is package-specific;
- historical evidence increases storage;
- multi-currency systems add complexity;
- migration must preserve ledger semantics;
- UI must explain more than a simple balance field.

## 141. Risks

### Projection Drift

Mitigation:

- transactional updates;
- rebuild;
- integrity checks;
- comparison tests.

### Advancement Applied Without Correct Spend

Mitigation:

- one transaction;
- shared OperationId;
- unique constraints;
- atomicity tests.

### Cost Changes Across Package Upgrade

Mitigation:

- exact version evidence;
- explicit upgrade;
- no historical rewrite.

### Correction Becomes Confusing

Mitigation:

- linked entries;
- clear UI;
- reason keys;
- before-and-after projection.

### Large Ledger Slows Queries

Mitigation:

- balance projection;
- indexed sequence;
- paginated history;
- performance tests.

## 142. Technology Spike

Before acceptance, implement:

1. progression currency contracts;
2. append-only ledger;
3. balance projection;
4. ledger reconstruction;
5. Session award operation;
6. advancement validation operation;
7. cost operation;
8. advancement Application command;
9. Character field integration;
10. advancement record;
11. correction and refund flow;
12. stale-state recheck;
13. duplicate OperationId constraints;
14. import and export mapping;
15. projection integrity diagnostics.

## 143. Spike Acceptance

The spike passes when:

- Session finalization awards progression exactly once;
- balances rebuild from ledger;
- advancement cost is calculated by the exact Rule Set version;
- insufficient balance blocks mutation;
- successful advancement updates Character, history, ledger, projection, and Operation Record atomically;
- duplicate OperationId does not spend twice;
- stale preview cannot commit;
- correction uses compensating entries;
- package upgrade does not rewrite prior cost evidence;
- crash recovery returns the committed result or leaves state unchanged.

## 144. Definition of Compliance

An implementation complies when:

- progression uses an append-only ledger;
- current balance is derived or projected from ledger entries;
- currencies use stable semantic keys and exact numeric values;
- advancements use exact-version deterministic Rule Operations;
- providers and the Archivist only propose progression;
- field changes, spend, history, projection, and operation result commit atomically;
- OperationId and constraints prevent duplicate effects;
- stale Character and balance state are rechecked;
- corrections use compensating entries;
- historical entries remain immutable;
- package and operation evidence is preserved;
- ledger reconstruction and projection consistency are tested.

## 145. Review Triggers

This ADR must be reviewed if:

- multiplayer introduces shared progression authority;
- server hosting processes concurrent advancement requests;
- packages need expiring real-time currencies;
- cross-Character resource transfers become core;
- marketplace content grants entitlements;
- progression becomes account-wide;
- event sourcing is adopted more broadly;
- Character-level concurrency becomes too coarse;
- external character builders integrate directly;
- progression rules require high-complexity optimization.

## 146. Deferred Decisions

Later ADRs MAY define:

- exact physical ledger schema;
- exact projection rebuild strategy;
- progression history UI;
- batch advancements;
- cross-Character transfers;
- account-wide currencies;
- real-time expiration;
- external character-builder protocol;
- advancement planner;
- formal ledger reconciliation report;
- server-side concurrency strategy.

## 147. Final Decision

Chronicle will record progression as an append-only ledger of earned, spent, adjusted, refunded, and corrected value.

Advancement cost and eligibility will be calculated by the exact Rule Set version.

Chronicle will apply Character changes, progression entries, history, balance projection, and operation result in one transaction.

A Character may grow.

Chronicle must always be able to explain what was earned, what was spent, which rule allowed it, and why the Character changed.
