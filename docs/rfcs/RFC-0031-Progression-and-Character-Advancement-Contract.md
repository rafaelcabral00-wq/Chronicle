---
id: RFC-0031
title: Progression and Character Advancement Contract
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-07-31
category: Contracts
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
---

> **"Advancement should preserve what the Character earned, explain what changed, and never confuse narrative recognition with mechanical authority."**

# Progression and Character Advancement Contract

## Abstract

This RFC defines Chronicle's provider-neutral contract for Character progression and advancement.

It establishes the separation between narrative evidence, Rule Set progression rules, progression awards, available balances, advancement proposals, spending, prerequisites, costs, Character Sheet changes, Session finalization, idempotency, auditability, versioning, migration, and player confirmation.

The Archivist may propose evidence and progression candidates.

The Rule Set validates and calculates mechanical advancement.

Chronicle owns identity, persistence, transaction boundaries, player authorization, and historical traceability.

## 1. Purpose

Progression is one of the most sensitive long-term mechanics in a Campaign.

Incorrect handling may cause:

- duplicated rewards;
- unsupported Character growth;
- lost progression currency;
- silent Character Sheet mutation;
- advancement without player choice;
- spending against stale Character state;
- historical ambiguity;
- provider-generated mechanics;
- incompatible migration;
- repeated finalization effects.

Chronicle requires a strict progression contract.

## 2. Scope

This RFC defines:

- progression terminology;
- progression currencies;
- evidence;
- award proposals;
- award calculation;
- award application;
- progression balances;
- advancement options;
- cost calculation;
- prerequisites;
- spending requests;
- Character Sheet changes;
- milestones;
- automatic advancement;
- player-selected advancement;
- deferred advancement;
- refunds and corrections;
- idempotency;
- versioning;
- historical records;
- migrations;
- visibility;
- observability;
- testing.

This RFC does not define:

- one specific Rule Set's advancement tables;
- exact Werewolf progression values;
- one UI workflow;
- respec policy;
- marketplace purchases;
- multiplayer approval;
- exact serialized schema;
- provider prompts.

## 3. Core Responsibility Separation

Progression is divided among three layers.

```text
Archivist
    proposes narrative evidence and possible progression relevance

Rule Set Package
    validates criteria, calculates awards, costs, prerequisites, and legal Character changes

Chronicle
    persists awards, protects balances, requests player authorization, applies changes, and prevents duplication
```

Narrative Intelligence MUST NOT directly award or spend progression.

## 4. Progression Definition

`Progression` is the mechanically recognized long-term development of a Character.

It MAY include:

- progression currency;
- milestone;
- rank;
- level;
- trait increase;
- new ability;
- unlocked option;
- narrative permission with mechanical consequences;
- permanent Character State change.

## 5. Advancement Definition

`Advancement` is the application of progression to a Character.

Examples:

- increasing an Attribute;
- learning an Ability;
- acquiring a Benefit;
- increasing a resource maximum;
- unlocking a form;
- satisfying a milestone;
- changing a rank.

## 6. Evidence Versus Award

```text
Progression Evidence
    = narrative facts supporting possible progression

Progression Award
    = validated mechanical reward granted by the Rule Set
```

Evidence alone is not spendable.

## 7. Award Versus Spend

```text
Award
    adds progression value or milestone state

Spend
    consumes progression value to apply advancement
```

These operations MUST remain separate unless a Rule Set explicitly defines automatic advancement.

## 8. Progression Currency

A `ProgressionCurrencyDefinition` SHOULD contain:

- stable currency key;
- Rule Set identity;
- version;
- display name key;
- description key;
- value type;
- minimum;
- maximum;
- visibility;
- expiration policy;
- transfer policy;
- award policy;
- spending policy.

## 9. Stable Currency Key

Currency keys MUST be:

- namespaced;
- language-neutral;
- version-aware;
- independent from UI labels.

Example:

```text
werewolf.progression.experience
```

## 10. Currency Types

A progression system MAY use:

```text
IntegerCurrency
DecimalCurrency
MilestoneCounter
BooleanUnlock
RankTrack
CompositeProgression
```

The initial implementation SHOULD use the minimum required by the first Rule Set.

## 11. Progression Balance

A `ProgressionBalance` SHOULD contain:

- Character identifier;
- currency key;
- current available amount;
- total awarded;
- total spent;
- reserved amount when applicable;
- version;
- last updated timestamp.

The invariant SHOULD be:

```text
Available = TotalAwarded - TotalSpent - Reserved
```

unless the Rule Set defines another explicit model.

## 12. Balance Authority

Chronicle persists balances.

The Rule Set validates arithmetic and policy.

Narrative Intelligence MUST NOT calculate authoritative balances.

## 13. Progression Evidence

A `ProgressionEvidence` SHOULD contain:

- evidence key;
- Character identifier;
- evidence type;
- Session identifier;
- Scene or Act reference;
- Message references;
- Dice Roll references;
- accepted event references;
- summary;
- visibility;
- confidence;
- source capability;
- evidence version.

## 14. Evidence Types

Possible evidence types include:

```text
GoalCompleted
MeaningfulFailure
RiskAccepted
CharacterBeliefExpressed
RelationshipChanged
SecretDiscovered
RuleSetCriterionMet
MilestoneReached
TrainingCompleted
ObjectiveAdvanced
```

The Rule Set defines which evidence types matter.

## 15. Evidence Authority

Evidence references MUST point to accepted Chronicle data.

Rejected or uncommitted provider output MUST NOT support progression.

## 16. Evidence Deduplication

The same underlying event MUST NOT create repeated awards accidentally.

Chronicle SHOULD use:

- Session identifier;
- evidence references;
- criterion key;
- Character;
- award operation identity.

## 17. Progression Award Proposal

A `ProgressionAwardProposal` SHOULD contain:

- proposal key;
- Character identifier;
- progression criterion key;
- evidence references;
- proposed currency or milestone;
- proposed amount or state;
- reason;
- confidence;
- visibility;
- source finalization operation.

The Archivist MAY produce this proposal.

## 18. Award Proposal Prohibitions

The Archivist MUST NOT:

- create unknown currency keys;
- calculate balances;
- bypass Rule Set criteria;
- apply the award;
- spend the award;
- duplicate immediate awards;
- invent persistent Award identifiers.

## 19. Award Evaluation Request

Chronicle SHOULD send a structured request to the Rule Set.

Conceptually:

```text
ProgressionAwardEvaluationRequest
├── OperationId
├── RuleSetReference
├── CharacterSnapshot
├── ProgressionState
├── Evidence
├── ProposedCriterion
├── SessionReference
├── CampaignPreferences
└── EvaluationMode
```

## 20. Award Evaluation Mode

Modes MAY include:

```text
ValidateProposal
CalculateAward
Preview
Replay
```

## 21. Award Evaluation Response

The Rule Set SHOULD return:

```text
ProgressionAwardEvaluationResponse
├── OperationId
├── CriterionKey
├── IsEligible
├── AwardComponents
├── RejectedEvidence
├── Warnings
├── CalculationVersion
├── Explanation
└── ValidationMetadata
```

## 22. Award Component

An `AwardComponent` SHOULD contain:

- currency or milestone key;
- amount or state;
- reason key;
- evidence references;
- visibility;
- maximum interaction;
- duplication key;
- calculation details.

## 23. Award Calculation

Award calculation MUST be:

- deterministic;
- versioned;
- side-effect free;
- independent from providers;
- independent from persistence access;
- based on accepted evidence;
- compatible with Campaign Preferences.

## 24. Award Maximum

The Rule Set MAY impose:

- per-Session maximum;
- per-criterion maximum;
- per-Character maximum;
- Campaign milestone limit;
- diminishing returns;
- duplicate-event suppression.

These policies MUST be explicit and testable.

## 25. Award Application

Chronicle applies a validated award through an application command.

The transaction SHOULD:

1. verify Character and balance versions;
2. verify Session finalization state;
3. check award idempotency;
4. persist Award record;
5. update balance or milestone;
6. emit Domain Event;
7. include result in finalization outcome.

## 26. Award Record

A persisted `ProgressionAward` SHOULD contain:

- AwardId;
- OperationId;
- Character identifier;
- Session identifier;
- Rule Set version;
- criterion key;
- award components;
- evidence references;
- calculation version;
- applied timestamp;
- visibility;
- correction status.

## 27. Award Idempotency

The same logical award MUST apply at most once.

Uniqueness SHOULD include:

- source finalization OperationId;
- Character;
- criterion;
- duplication key;
- award component.

## 28. Immediate Award

A Rule Set MAY define an award applied during play.

If so:

- the operation must be explicit;
- the award must be persisted immediately;
- the Archivist must receive it as an already-applied change;
- finalization MUST NOT award it again.

## 29. Deferred Award

Most Session-based awards SHOULD be applied during finalization.

The Session remains incomplete until critical progression processing succeeds or an approved fallback policy is used.

## 30. Milestone

A `MilestoneDefinition` SHOULD contain:

- milestone key;
- Rule Set version;
- criteria;
- repeatability;
- completion behavior;
- reward;
- visibility;
- expiration or reset behavior.

## 31. Milestone State

A Character milestone state MAY be:

```text
NotStarted
InProgress
Completed
Consumed
Expired
Superseded
```

The Rule Set defines valid transitions.

## 32. Advancement Option

An `AdvancementOption` describes one legal potential Character improvement.

It SHOULD contain:

- option key;
- target field or capability;
- current value;
- proposed value;
- currency cost;
- prerequisites;
- availability;
- visibility;
- consequences;
- Rule Set version;
- calculation version;
- explanation keys.

## 33. Advancement Catalog

The Rule Set SHOULD provide advancement options based on:

- Character Sheet schema;
- current Character values;
- progression balance;
- prerequisites;
- Campaign Preferences;
- Character role;
- Rule Set version.

The catalog is computed, not persisted as truth.

## 34. Advancement Availability

Availability statuses MAY include:

```text
Available
InsufficientCurrency
PrerequisiteMissing
MaximumReached
UnavailableInCurrentLifecycle
ConflictsWithState
Deprecated
Hidden
```

## 35. Cost Calculation

Advancement cost MUST be:

- deterministic;
- versioned;
- based on current Character state;
- based on Campaign Preferences when relevant;
- explainable;
- recalculated before commit.

## 36. Cost Preview

The UI MAY show a preview.

Preview is not authoritative.

The cost MUST be recalculated during the spending transaction.

## 37. Prerequisite

A prerequisite SHOULD contain:

- prerequisite key;
- type;
- target field or state;
- expected condition;
- current satisfaction;
- visibility;
- explanation key.

## 38. Prerequisite Types

Possible types include:

```text
FieldMinimum
FieldMaximum
OptionSelected
MilestoneCompleted
CurrencyAvailable
CharacterState
CampaignPreference
NarrativePermission
MutualExclusion
RuleSetCapability
```

## 39. Narrative Prerequisite

A Rule Set MAY require a validated narrative permission or milestone.

The permission MUST be represented structurally.

Free-form prose alone MUST NOT satisfy the prerequisite.

## 40. Advancement Request

A player-selected `CharacterAdvancementRequest` SHOULD contain:

```text
OperationId
CharacterId
ExpectedCharacterVersion
RuleSetReference
AdvancementOptionKey
ExpectedCurrentValue
RequestedTargetValue
ExpectedCost
CurrencySelection
PlayerAuthorization
CampaignReference
```

## 41. Player Authorization

Player-selected advancement requires explicit player authorization.

The application MUST NOT spend progression because the Narrator or Archivist suggested it.

## 42. Automatic Advancement

A Rule Set MAY define automatic advancement.

Automatic advancement MUST be:

- declared by the Rule Set;
- deterministic;
- tied to explicit validated conditions;
- visible to the player;
- idempotent;
- persisted with evidence.

Narrative Intelligence MUST NOT invent automatic advancement.

## 43. Advancement Validation

Before commit, Chronicle and the Rule Set validate:

- exact Character version;
- exact Rule Set version;
- option availability;
- current value;
- target value;
- prerequisites;
- cost;
- balance;
- lifecycle;
- Campaign ownership;
- no conflicting advancement;
- player authorization when required.

## 44. Advancement Result

A successful result SHOULD contain:

- advancement identifier;
- Character identifier;
- option key;
- prior values;
- new values;
- spent currencies;
- remaining balances;
- unlocked capabilities;
- Rule Set version;
- calculation version;
- applied timestamp;
- explanation.

## 45. Character Sheet Change Set

The Rule Set SHOULD return a structured Character Sheet Change Set.

It MAY contain:

- field changes;
- added options;
- removed options;
- derived value recalculation;
- resource maximum change;
- state extension change;
- unlocked operation;
- migration note.

## 46. Change Set Validation

Chronicle MUST validate the target schema and versions before applying the Change Set.

Unknown fields or invalid types MUST be rejected.

## 47. Atomic Spending

The following MUST commit atomically:

- progression spend;
- balance update;
- Character Sheet changes;
- progression record;
- derived recalculation;
- Domain Events.

Partial application is forbidden.

## 48. Reserved Currency

The architecture MAY support reserved progression for multi-step workflows.

The MVP SHOULD avoid reservation unless required.

If used, reservation MUST:

- have identity;
- expire or resolve explicitly;
- prevent double spending;
- remain recoverable after restart.

## 49. Multiple Currencies

An advancement MAY cost several currencies.

The request and result MUST preserve each component separately.

Chronicle MUST not collapse them into one generic amount.

## 50. Alternative Costs

A Rule Set MAY expose alternative valid costs.

The player must select one explicitly unless the Rule Set defines deterministic selection.

## 51. Refund

A `ProgressionRefund` reverses an accepted spend under an explicit correction or Rule Set policy.

Refunds MUST:

- reference the original advancement;
- preserve audit history;
- validate current Character state;
- avoid deleting the original record;
- create compensating changes.

## 52. Correction

A progression correction MAY be required after:

- package defect;
- duplicated award;
- invalid migration;
- administrative repair;
- data corruption recovery.

Correction MUST use an explicit workflow.

## 53. No Silent Rewrite

Chronicle MUST NOT silently edit:

- Award amount;
- spent amount;
- advancement history;
- evidence;
- balance history.

Corrections require new records and traceable status.

## 54. Respec

Character respec is outside the MVP.

Future respec requires:

- Rule Set policy;
- player authorization;
- dependency validation;
- refund calculation;
- Character Sheet Change Set;
- historical preservation.

## 55. Session Finalization Integration

Finalization SHOULD process progression in this order:

1. collect accepted evidence;
2. receive Archivist proposals;
3. validate proposal references;
4. evaluate Rule Set criteria;
5. calculate awards;
6. build finalization Change Set;
7. apply awards atomically with finalization;
8. age Memories once;
9. complete Session.

Exact transaction partitioning follows RFC-0013.

## 56. Criticality

Progression may be classified as:

```text
Critical
Deferrable
Optional
```

The initial Rule Set MUST declare the policy.

A Critical progression failure blocks finalization.

A Deferrable failure may complete the Session with pending progression.

## 57. Pending Progression

If deferral is allowed, Chronicle SHOULD persist a `PendingProgressionOperation`.

It MUST include:

- Session;
- Character;
- evidence;
- expected versions;
- retry state;
- blocking status;
- player-visible explanation.

## 58. Progression and Memories

Progression evidence MAY reference Campaign Memories.

A Memory does not mechanically award progression by itself.

The Rule Set must validate the criterion.

## 59. Progression and Relationships

Relationship change MAY satisfy a progression criterion.

The Relationship change and progression award remain separate domain operations.

Dependencies MUST be explicit in the finalization Change Set.

## 60. Progression and Character Knowledge

Learning a truth MAY satisfy progression criteria.

Chronicle MUST preserve:

- which Character learned it;
- certainty state;
- evidence;
- visibility.

Player omniscience is not assumed.

## 61. Progression Visibility

Progression data MAY be:

```text
PlayerVisible
PlayerHidden
InternalOnly
CharacterScoped
```

Player Character progression SHOULD normally be visible.

NPC progression may remain hidden.

## 62. Hidden Criteria

A Rule Set MAY have hidden criteria.

The application SHOULD reveal:

- that an award occurred;
- visible amount;
- safe explanation;

without exposing hidden Campaign information.

## 63. NPC Progression

Persistent NPCs MAY progress.

NPC progression MUST:

- use the same Rule Set contract;
- remain Campaign-scoped;
- use explicit evidence or deterministic policy;
- preserve visibility;
- avoid consuming Player Character currency.

## 64. NPC Automatic Progression

Automatic NPC progression is outside the MVP unless required by the first complete Rule Set flow.

Future support requires explicit bounded policy.

It MUST NOT become autonomous background world simulation.

## 65. Campaign-Level Progression

A Rule Set MAY define Campaign-level progression.

Examples:

- shared renown;
- organization rank;
- territory development;
- Campaign milestone.

This requires a separate typed progression owner.

The MVP MAY defer it.

## 66. Progression Owner

The generic contract SHOULD allow progression owner types such as:

```text
Character
Campaign
Organization
Group
```

The MVP SHOULD initially support Character only unless the first Rule Set requires more.

## 67. Historical Interpretation

Progression history MUST preserve:

- exact Rule Set version;
- criterion version;
- calculation version;
- Character schema version;
- evidence;
- balances before and after;
- advancement changes.

## 68. Historical Replay

Chronicle SHOULD be able to replay progression calculation using exact historical versions.

Replay is diagnostic.

It MUST NOT alter history.

## 69. Missing Historical Package

If the exact package is unavailable:

- persisted Award and Advancement records remain authoritative;
- full recalculation may be unavailable;
- display uses stored explanation and values;
- Chronicle MUST not recalculate with the latest package silently.

## 70. Version Compatibility

A package update may remain compatible when:

- currency semantics remain unchanged;
- cost calculations remain equivalent;
- prerequisite semantics remain equivalent;
- Character changes remain compatible.

Breaking changes require new versions and migration policy.

## 71. Progression Migration

A progression migration MAY transform:

- currency keys;
- balances;
- milestone states;
- advancement option keys;
- cost model references;
- deprecated Character fields.

## 72. Migration Requirements

Migration MUST:

- be deterministic;
- preserve totals;
- preserve evidence;
- preserve spend history;
- identify rounding;
- produce warnings;
- validate target Character;
- preserve source snapshot;
- avoid provider calls.

## 73. Rounding

If a migration changes numeric precision, the Rule Set MUST define:

- rounding direction;
- remainder handling;
- visible explanation;
- audit record.

Silent value loss is forbidden.

## 74. Import

Future Character import SHOULD preserve progression history when available.

An imported balance without history MUST be labeled as an opening balance or imported state.

It MUST not fabricate Award records.

## 75. Error Model

Recommended progression errors include:

```text
ProgressionCriterionUnknown
EvidenceInvalid
EvidenceDuplicate
AwardNotAllowed
AwardLimitExceeded
CurrencyUnknown
BalanceInsufficient
AdvancementUnavailable
PrerequisiteMissing
CostChanged
CharacterVersionConflict
ProgressionAlreadyApplied
MigrationRequired
HistoricalResolverUnavailable
```

## 76. Retry Semantics

Typical retry behavior:

```text
Provider finalization proposal failed
    → retry proposal with same finalization OperationId

Rule Set evaluation failed transiently
    → retry with same OperationId

Character changed before spend
    → refresh and request player confirmation again

Commit uncertainty
    → query existing Award or Advancement record

Insufficient balance
    → not retryable without changed balance or request
```

## 77. Award Failure After Calculation

If calculation succeeded but commit failed:

- reuse the same Award calculation when versions remain valid;
- do not invoke the Archivist again;
- preserve OperationId;
- recalculate only when authoritative state changed.

## 78. Spend Failure After Validation

If spending validation succeeded but commit failed:

- confirm whether commit occurred;
- reuse OperationId;
- revalidate Character version and cost;
- require renewed authorization if the cost changed.

## 79. Failure After Commit

If commit succeeded but confirmation was lost:

- retry returns the existing Advancement result;
- currency is not spent again;
- Character changes are not repeated.

## 80. Concurrency

Chronicle MUST prevent conflicting progression writes for the same Character.

Recommended MVP policy:

```text
One state-changing Character advancement operation at a time
```

Optimistic version checks remain required.

## 81. Stale Advancement Catalog

An AdvancementOption preview becomes stale when:

- Character changes;
- balance changes;
- Rule Set version changes;
- Campaign Preference changes;
- milestone changes;
- package configuration changes.

The option MUST be recalculated before commit.

## 82. Security

Progression workflows MUST defend against:

- negative costs;
- integer overflow;
- forged balances;
- cross-Campaign Character references;
- unknown currency keys;
- unsupported field changes;
- duplicated OperationIds;
- provider-generated persistent IDs;
- arbitrary advancement option names;
- hidden criteria leakage.

## 83. Numeric Safety

Currency and cost calculations SHOULD define:

- numeric type;
- minimum;
- maximum;
- overflow behavior;
- rounding;
- precision;
- zero-cost policy;
- negative-value prohibition.

## 84. Observability

Chronicle SHOULD record:

- OperationId;
- Character identifier;
- Rule Set version;
- criterion key;
- evidence count;
- currency key;
- amount awarded;
- amount spent;
- balance before and after;
- advancement option;
- calculation version;
- validation duration;
- transaction result;
- correction or refund reference.

## 85. Logging Safety

Logs SHOULD NOT expose:

- hidden Secret evidence;
- private NPC progression;
- unrestricted Character biography;
- provider payloads;
- proprietary source text.

## 86. Metrics

Useful metrics include:

- Awards per Session;
- average progression per Character;
- duplicate evidence rejection;
- advancement attempt count;
- insufficient-balance rate;
- stale-preview rate;
- correction rate;
- migration warning count;
- pending progression count;
- progression failure rate.

Metrics MUST not define game balance automatically.

## 87. Testing Strategy

### 87.1 Award Tests

Test:

- valid evidence;
- duplicate evidence;
- award maximum;
- multiple currencies;
- immediate versus deferred awards;
- finalization idempotency.

### 87.2 Advancement Tests

Test:

- legal option;
- missing prerequisite;
- insufficient balance;
- changed cost;
- maximum reached;
- atomic application.

### 87.3 Historical Tests

Test replay, migration, correction, and missing package behavior.

## 88. Golden Progression Fixture

A golden fixture SHOULD contain:

```text
Given:
    exact Rule Set version
    Character snapshot
    progression state
    Session evidence
    Campaign Preferences

Expect:
    eligibility
    award components
    calculation version
    explanation
```

## 89. Golden Advancement Fixture

A golden advancement fixture SHOULD contain:

```text
Given:
    Character snapshot
    balances
    selected option
    prerequisites
    Rule Set version

Expect:
    cost
    Character Change Set
    remaining balances
    unlocked behavior
```

## 90. Required Test Cases

Tests MUST cover:

- valid Session award;
- unknown criterion;
- duplicate evidence;
- per-Session award maximum;
- immediate award excluded from finalization;
- multiple award components;
- valid milestone completion;
- repeated nonrepeatable milestone;
- legal advancement;
- insufficient currency;
- stale expected cost;
- missing prerequisite;
- maximum field reached;
- invalid target field;
- multiple currencies;
- alternative cost selection;
- automatic advancement;
- unauthorized player-selected spend;
- duplicate OperationId;
- failure after calculation;
- failure after commit;
- Character version conflict;
- correction;
- refund;
- progression migration;
- rounding migration;
- missing historical package;
- hidden NPC progression filtering;
- cross-Campaign Character reference;
- numeric overflow.

## 91. Prohibited Patterns

### 91.1 Archivist Awards Directly

The Archivist proposes evidence and candidates only.

### 91.2 Narrator Spends Progression

Interactive narration MUST NOT mutate progression balances.

### 91.3 Silent Character Advancement

Player-selected advancement requires explicit authorization.

### 91.4 Award and Spend Collapsed Accidentally

Awarding progression MUST NOT automatically spend it unless the Rule Set explicitly defines automatic advancement.

### 91.5 Duplicate Finalization Reward

Retrying finalization MUST NOT repeat an Award.

### 91.6 UI Calculates Authoritative Cost

The UI may preview but the Rule Set validates at commit.

### 91.7 Latest Package Recalculates History

Historical values preserve exact versions.

### 91.8 Free-Form Prose as Prerequisite

Narrative prerequisites must use structured evidence.

### 91.9 Silent History Rewrite

Corrections use compensating records.

### 91.10 Generic Unvalidated Currency Map

Currencies and advancement options must be declared by the Rule Set.

## 92. Current Delivery Decision

The MVP adopts:

- Character-owned progression;
- declared progression currencies and milestones;
- structured evidence;
- Archivist award proposals;
- deterministic Rule Set award evaluation;
- persisted Award records;
- idempotent finalization awards;
- computed AdvancementOptions;
- Rule Set cost and prerequisite validation;
- explicit player authorization for selected advancement;
- atomic balance and Character Sheet updates;
- correction records rather than silent edits;
- exact historical version preservation;
- golden progression fixtures;
- no respec workflow;
- no autonomous NPC advancement;
- no Campaign-level progression unless required by the initial Rule Set;
- no provider mechanical authority.

## 93. Architecture Horizon

Future evolution MAY include:

- Character respec;
- Campaign-level progression;
- organization progression;
- multiplayer approval;
- training-time systems;
- advancement planning;
- automatic NPC progression;
- shared progression pools;
- progression recommendations;
- richer milestone graphs;
- community Rule Set progression extensions.

The MVP MUST NOT implement these capabilities without a later milestone.

## 94. Open Questions

The following remain open:

- Which progression currencies are required by the initial Rule Set?
- Are awards applied inside the finalization transaction or a guaranteed continuation?
- Which progression failures block Session completion?
- Should players review Awards before finalization commits?
- How should advancement options be grouped in the UI?
- Are Character Sheet changes expressed through the generic schema patch model?
- How much calculation trace should be persisted?
- Should automatic advancement exist in the first package?
- How are narrative prerequisites represented?
- Which correction permissions exist in the official application?
- Should NPC progression be supported at all in MVP?
- What migration behavior is needed before version 1.0?
- How should hidden award criteria be explained?
- Should progression balances be shown in Session summaries?
- Which cost previews may be cached?

These questions require persistence RFCs, UI RFCs, the initial Rule Set implementation, and technology ADRs.

## 95. Compliance Checklist

An implementation complies when:

- evidence is separate from Award;
- Award is separate from Spend;
- progression keys are stable and versioned;
- Rule Set logic calculates awards and costs deterministically;
- Chronicle persists balances and history;
- finalization rewards are idempotent;
- immediate rewards are not repeated;
- advancement options are recalculated before commit;
- player-selected spending requires authorization;
- prerequisites are structured;
- Character changes and balance updates are atomic;
- corrections preserve audit history;
- exact historical versions are retained;
- providers do not award or spend progression;
- cross-Campaign references are rejected.

## 96. Final Principle

Progression should recognize what the Character lived without allowing interpretation to become unreviewed power.

The story may justify growth.

The Rule Set defines its cost.

Chronicle ensures it happens once, visibly, and without losing history.
