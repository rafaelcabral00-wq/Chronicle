---
id: RFC-0030
title: Rule Operation and Mechanical Resolution Contract
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
---

> **"Chronicle owns chance. The Rule Set owns interpretation. Neither may impersonate the other."**

# Rule Operation and Mechanical Resolution Contract

## Abstract

This RFC defines the provider-neutral contract used by Chronicle to request, validate, execute, interpret, and persist mechanical operations supplied by a Rule Set package.

It establishes:

- stable Rule Operation identity;
- actor, target, and context inputs;
- pool construction;
- modifiers;
- difficulty;
- random input;
- deterministic resolution;
- generic outcomes;
- system-specific results;
- immediate consequence proposals;
- narrative guidance;
- versioning;
- idempotency;
- replay;
- validation;
- error handling;
- explainability;
- testing.

Chronicle generates and persists authoritative random values.

The Rule Set validates the operation and deterministically interprets those values.

Narrative Intelligence may request or narrate a Test, but it never resolves one authoritatively.

## 1. Purpose

Chronicle must support mechanical resolution without embedding one tabletop RPG system into generic application code.

A weak boundary would allow:

- Narrator-generated dice;
- hidden provider arithmetic;
- Rule Set packages writing directly to Campaign state;
- ambiguous operation names;
- unversioned calculations;
- results that cannot be replayed;
- consequences that are applied twice;
- historical Rolls changing after a package update;
- UI-specific mechanics;
- cross-Campaign references.

This contract prevents those failures.

## 2. Scope

This RFC defines:

- Rule Operation descriptors;
- operation keys and versions;
- request and response envelopes;
- operation availability;
- actor and target requirements;
- Character snapshots;
- Scene and Campaign context;
- modifier contracts;
- pool construction;
- difficulty;
- random input;
- deterministic resolution;
- generic outcome mapping;
- system-specific result payload;
- consequence proposals;
- narrative guidance;
- validation;
- idempotency;
- historical replay;
- migration and compatibility;
- observability;
- security;
- testing.

This RFC does not define:

- exact dice notation syntax;
- exact Werewolf mechanics;
- one programming language;
- one serialization technology;
- exact UI flow;
- exact random number generator;
- provider prompts;
- complete progression rules;
- every future mechanical operation.

## 3. Core Principle

Mechanical resolution is divided into three responsibilities.

```text
Narrative Intelligence
    may propose that uncertainty requires a Test

Chronicle
    validates intent, generates randomness, persists identity, and applies accepted consequences

Rule Set Package
    validates the operation, constructs mechanics, and deterministically interprets the random input
```

No layer may silently absorb the others.

## 4. Rule Operation

A `RuleOperation` is a stable mechanical action defined by a Rule Set package.

Examples:

```text
generic.social.influence
generic.physical.contest
werewolf.operation.frenzy_resist
werewolf.operation.track_spirit
```

A Rule Operation is not a UI button, provider tool name, or localized sentence.

## 5. Operation Identity

Every Rule Operation MUST define:

```text
RuleSetId
RuleSetVersion
OperationKey
OperationVersion
```

Together, these identify the exact mechanical contract.

## 6. Operation Key

An OperationKey MUST be:

- stable;
- language-neutral;
- namespaced;
- documented;
- unique within the Rule Set version;
- independent from implementation class names;
- independent from UI labels.

## 7. Operation Version

OperationVersion identifies the exact semantics of one operation.

A breaking change to:

- input shape;
- pool rule;
- difficulty rule;
- result interpretation;
- consequence meaning;
- outcome mapping;

requires a new OperationVersion.

## 8. Operation Descriptor

A Rule Set package SHOULD expose an `OperationDescriptor`.

Conceptually:

```text
OperationDescriptor
├── RuleSetReference
├── OperationKey
├── OperationVersion
├── DisplayNameKey
├── DescriptionKey
├── ActorRequirements
├── TargetRequirements
├── ContextRequirements
├── ModifierPolicy
├── DifficultyPolicy
├── RandomInputPolicy
├── OutcomeModel
├── ConsequencePolicy
├── VisibilityPolicy
├── GuidanceReferences
└── Status
```

## 9. Operation Status

Canonical statuses MAY include:

```text
Available
Experimental
Deprecated
Unavailable
InternalOnly
```

A deprecated operation remains resolvable for historical data when its package version is available.

## 10. Operation Availability

Availability MAY depend on:

- Character role;
- Character Sheet values;
- Character State;
- Scene state;
- target presence;
- Campaign Preferences;
- Rule Set capabilities;
- unresolved Roll state;
- operation preconditions.

Availability MUST be evaluated deterministically.

## 11. Operation Request

A `RuleOperationRequest` SHOULD contain:

```text
RuleOperationRequest
├── OperationId
├── CorrelationId
├── RuleSetReference
├── OperationKey
├── OperationVersion
├── CampaignReference
├── SessionReference
├── ActReference
├── SceneReference
├── ActorSnapshot
├── TargetSnapshot
├── ContextSnapshot
├── ProposedModifiers
├── DifficultyInput
├── RandomInput
├── CampaignPreferences
├── VisibilityContext
└── ResolutionMode
```

Some fields are optional depending on operation stage.

## 12. Operation Stages

Mechanical resolution MAY be divided into:

```text
ValidateIntent
BuildTest
ExecuteRandomness
ResolveResult
ApplyConsequences
ExplainResult
```

A simple implementation MAY combine some stages internally.

The contracts between Chronicle and the Rule Set MUST remain explicit.

## 13. Validate Intent

`ValidateIntent` checks whether the proposed operation is mechanically meaningful.

It SHOULD verify:

- operation exists;
- actor is eligible;
- target requirements are satisfied;
- Scene allows the operation;
- required Character fields exist;
- Campaign Preferences are compatible;
- no unresolved conflicting Test exists.

It MUST NOT generate randomness.

## 14. Build Test

`BuildTest` determines the exact mechanical inputs required before randomness.

It SHOULD return:

- validated operation;
- pool definition;
- difficulty;
- accepted modifiers;
- rejected modifiers;
- random input specification;
- stakes guidance;
- calculation version;
- warnings.

## 15. Execute Randomness

Chronicle executes randomness according to the returned specification.

Chronicle MUST:

- generate raw values;
- create Dice Roll identity;
- persist the raw result;
- preserve generator metadata required for audit;
- avoid provider-generated values;
- avoid Rule Set package-owned random sources.

## 16. Resolve Result

The Rule Set receives the persisted raw random input and deterministically resolves it.

It SHOULD return:

- generic outcome;
- system-specific result;
- success or failure details;
- margins or degrees;
- critical states;
- accepted modifiers;
- consequence proposals;
- narrative guidance;
- calculation trace;
- warnings.

## 17. Apply Consequences

Chronicle validates and applies consequence proposals.

The Rule Set MUST NOT mutate:

- Character;
- Scene;
- Campaign;
- Relationship;
- Knowledge;
- Memory;
- persistence.

## 18. Explain Result

The Rule Set MAY return structured explanatory data.

Rule Knowledge Retrieval MAY supplement this explanation.

Narrative Intelligence MAY narrate the accepted result.

The mechanical result remains authoritative.

## 19. Resolution Mode

Canonical modes MAY include:

```text
Preview
Authoritative
Replay
ValidationOnly
```

### Preview

Calculates possible structure without generating or applying authoritative randomness.

### Authoritative

Used for real Campaign resolution.

### Replay

Reinterprets persisted raw values using the exact historical versions.

### ValidationOnly

Checks request validity without building a full Test.

## 20. Actor Snapshot

An `ActorSnapshot` SHOULD contain only the fields required by the operation.

It MAY include:

- Character identifier;
- Character version;
- role;
- schema identifier;
- schema version;
- relevant Character Sheet values;
- relevant Character State;
- relevant active effects;
- relevant Relationship mechanics;
- relevant Knowledge mechanics;
- visibility.

## 21. Target Snapshot

A target MAY be:

- another Character;
- Scene obstacle;
- object;
- abstract difficulty;
- environment;
- group;
- no target.

The target type MUST be explicit.

## 22. Context Snapshot

A ContextSnapshot MAY include:

- Scene conditions;
- location tags;
- time or phase;
- active environmental effects;
- prior accepted consequences;
- relevant Campaign Preferences;
- Rule Set-specific context keys;
- visibility constraints.

Context MUST be bounded and typed.

## 23. Snapshot Versioning

Actor, target, and context snapshots SHOULD preserve expected versions.

Before authoritative resolution or consequence application, Chronicle MUST validate that relevant versions remain current.

## 24. Proposed Modifier

A `ProposedModifier` SHOULD contain:

```text
ModifierKey
SourceType
SourceReference
Value
StackingCategory
Reason
Visibility
ExpectedVersion
```

Provider-generated prose is not a valid modifier by itself.

## 25. Modifier Sources

Allowed modifier sources MAY include:

- Character Sheet field;
- Character State;
- target state;
- Scene condition;
- Campaign Preference;
- equipment;
- Relationship mechanic;
- prior mechanical consequence;
- explicit Rule Set override;
- validated narrative event.

## 26. Modifier Validation

The Rule Set MUST validate:

- modifier key;
- source;
- value;
- stacking;
- duplication;
- compatibility;
- visibility;
- operation applicability.

Invalid modifiers MUST be rejected explicitly.

## 27. Modifier Stacking

The operation descriptor SHOULD define stacking behavior.

Possible policies:

```text
Additive
HighestOnly
LowestOnly
UniqueBySource
Replace
NonStacking
CustomDeterministic
```

## 28. Modifier Explanation

Accepted and rejected modifiers SHOULD include structured explanations.

The UI and diagnostics SHOULD be able to display why the final pool changed.

## 29. Pool Definition

A `PoolDefinition` SHOULD contain:

- pool type;
- component list;
- base value;
- accepted modifiers;
- final size or expression;
- minimum;
- maximum;
- zero-pool policy;
- calculation version;
- explanation keys.

## 30. Pool Components

Each pool component SHOULD identify:

- source field or state;
- value;
- contribution;
- calculation rule;
- visibility;
- source version.

## 31. Pool Calculation

Pool calculation MUST be:

- deterministic;
- side-effect free;
- versioned;
- reproducible;
- independent from UI;
- independent from Narrative Intelligence;
- independent from storage access.

## 32. Zero Pool

The operation descriptor MUST define behavior when the pool is zero or below minimum.

Possible behavior:

```text
OperationUnavailable
ChanceDie
AutomaticFailure
UseMinimumPool
AlternativeResolution
```

The package MUST not improvise silently.

## 33. Difficulty

Difficulty MAY be represented as:

- target number;
- required successes;
- opposing pool;
- threshold;
- rank;
- static category;
- no explicit difficulty.

The exact form is Rule Set-specific but must be structured.

## 34. Difficulty Input

A `DifficultyInput` MAY include:

- requested value;
- source;
- reason;
- proposed category;
- target-derived value;
- Scene-derived value;
- Narrator guidance.

The Rule Set validates the final difficulty.

## 35. Difficulty Authority

Narrative Intelligence may propose stakes or difficulty guidance.

The Rule Set determines whether the value is valid.

Chronicle persists the accepted final difficulty.

## 36. Random Input Specification

The Rule Set SHOULD return a `RandomInputSpecification`.

It MAY define:

- die type;
- count;
- reroll policy;
- exploding behavior;
- special die categories;
- grouping;
- deterministic transformations;
- visibility;
- maximum generated values.

## 37. Random Input

`RandomInput` is the authoritative raw output generated by Chronicle.

It SHOULD contain:

- Dice Roll identifier;
- operation identity;
- generated values;
- generator identifier;
- generator version;
- timestamp;
- visibility;
- execution attempt identity;
- checksum or integrity metadata when useful.

## 38. Random Input Immutability

Once persisted, raw random values MUST be immutable.

Corrections require an explicit compensating or replacement workflow.

They MUST not be silently edited.

## 39. Random Generator

The concrete random generator belongs to Chronicle infrastructure.

It SHOULD be:

- testable;
- replaceable;
- unbiased according to selected implementation;
- isolated from providers;
- isolated from Rule Set package code.

## 40. Test Random Generator

Automated tests MUST support deterministic random sequences.

Test determinism MUST never leak into production Rolls accidentally.

## 41. Result Resolution Request

A `ResultResolutionRequest` SHOULD contain:

- OperationId;
- Dice Roll identifier;
- exact Rule Set version;
- exact OperationVersion;
- calculation version;
- validated pool;
- accepted modifiers;
- final difficulty;
- raw random values;
- actor snapshot;
- target snapshot;
- relevant context;
- Campaign Preferences.

## 42. Result Resolution Response

A `ResultResolutionResponse` SHOULD contain:

```text
ResultResolutionResponse
├── OperationId
├── RuleSetReference
├── OperationKey
├── OperationVersion
├── CalculationVersion
├── GenericOutcome
├── SystemResult
├── Degree
├── CriticalState
├── ConsequenceProposals
├── NarrativeGuidance
├── CalculationTrace
├── Warnings
└── ValidationMetadata
```

## 43. Generic Outcome

Chronicle SHOULD use a bounded generic outcome vocabulary.

Initial values MAY include:

```text
AutomaticSuccess
CriticalSuccess
Success
PartialSuccess
Failure
CriticalFailure
ContestWon
ContestLost
Tie
NoResolution
```

Not every Rule Set uses every value.

## 44. Generic Outcome Purpose

Generic outcome supports:

- application workflows;
- Narrative Intelligence;
- UI presentation;
- cross-system diagnostics;
- evaluation.

It MUST NOT erase the richer system-specific result.

## 45. System Result

`SystemResult` contains Rule Set-specific structured meaning.

It SHOULD use namespaced keys.

Example:

```text
werewolf.result.rage_complication
```

The payload MUST be versioned and bounded.

## 46. Degree

A result MAY include a degree, margin, or success count.

The representation SHOULD define:

- value;
- semantic key;
- bounds;
- sign;
- interpretation version.

## 47. Critical State

A `CriticalState` MAY identify:

- none;
- critical success;
- critical failure;
- exceptional condition;
- system-specific complication.

It MUST not be inferred only from prose.

## 48. Contest

A contested operation SHOULD define:

- participants;
- each side's validated mechanical input;
- random input per side;
- comparison rule;
- tie behavior;
- outcome mapping;
- consequence routing.

## 49. Group Operation

A group operation MAY involve several actors.

The descriptor MUST define:

- contributor policy;
- leader policy;
- pool combination;
- shared or individual randomness;
- consequence distribution;
- visibility.

The MVP MAY support only operations required by the initial Rule Set.

## 50. Passive Resolution

Some operations MAY use a passive value rather than a Roll.

The operation descriptor MUST state this explicitly.

A passive result remains deterministic and versioned.

## 51. Automatic Result

An operation MAY resolve automatically when preconditions guarantee an outcome.

The response SHOULD still produce:

- operation identity;
- result;
- reason;
- calculation version;
- consequence proposals;
- no fabricated random values.

## 52. Consequence Proposal

A `MechanicalConsequenceProposal` SHOULD contain:

- consequence key;
- consequence version;
- target reference;
- consequence type;
- prior expected state;
- proposed state or delta;
- duration;
- stacking policy;
- visibility;
- source Dice Roll;
- source operation;
- application timing;
- Rule Set validation metadata.

## 53. Consequence Types

Possible generic categories include:

```text
ResourceChange
TrackChange
ConditionApplied
ConditionRemoved
PositionChange
ObjectiveProgress
EquipmentStateChange
TemporaryModifier
NarrativePermission
NarrativeRestriction
```

Rule Set-specific categories MAY use namespaced payloads.

## 54. Consequence Timing

Canonical timing values MAY include:

```text
Immediate
BeforeNarrativeContinuation
EndOfScene
EndOfSession
DeferredReview
```

Chronicle routes consequences to the correct application workflow.

## 55. Consequence Application

Before application, Chronicle MUST validate:

- target still exists;
- target version;
- consequence not already applied;
- operation committed;
- Rule Set version;
- stacking;
- lifecycle;
- visibility;
- transaction compatibility.

## 56. Applied Consequence Identity

An applied consequence SHOULD be traceable to:

- OperationId;
- Dice Roll identifier;
- consequence key;
- target;
- result reference.

This prevents duplicate application.

## 57. Consequence Rejection

Chronicle MAY reject a consequence while preserving the mechanical Roll result when:

- target state changed;
- consequence is invalid;
- application policy forbids it;
- package output is inconsistent.

The workflow MUST then determine whether repair or recovery is required.

## 58. Narrative Guidance

The Rule Set MAY return narrative guidance.

It SHOULD contain:

- result summary key;
- tone guidance;
- visible mechanical facts;
- hidden mechanical facts;
- suggested consequence framing;
- prohibited interpretations;
- Rule Knowledge topic keys.

It MUST not contain authoritative prose that bypasses Narrator validation.

## 59. Visible and Hidden Result Data

Mechanical results MAY contain hidden information.

The response MUST structurally separate:

- player-visible result;
- Character-scoped result;
- internal result;
- Narrator-only guidance.

## 60. Rule Knowledge Linkage

A resolution MAY return Rule Knowledge query keys.

Example:

```text
TopicKeys:
  - dice.critical-failure
OperationKeys:
  - werewolf.operation.frenzy_resist
```

Chronicle retrieves explanatory content separately.

## 61. Calculation Trace

A `CalculationTrace` SHOULD expose enough structured detail to explain the result.

It MAY include:

- source values;
- accepted modifiers;
- rejected modifiers;
- pool calculation;
- difficulty;
- random values;
- success evaluation;
- critical evaluation;
- outcome mapping;
- consequence derivation.

## 62. Trace Safety

Calculation traces MUST:

- avoid proprietary source text;
- avoid arbitrary code representation;
- avoid provider secrets;
- remain bounded;
- respect visibility.

## 63. Explainability

A player-facing explanation SHOULD be derivable from:

- operation descriptor;
- calculation trace;
- result;
- Rule Knowledge citations.

Chronicle SHOULD not require a provider to explain the arithmetic accurately.

## 64. Idempotency

Authoritative mechanical operations MUST be idempotent.

The same OperationId and request fingerprint MUST not create:

- a second Dice Roll;
- new random values;
- a second result;
- duplicate consequences;
- duplicate Messages.

## 65. Request Fingerprint

A mechanical request fingerprint SHOULD include:

- Rule Set identity and version;
- OperationKey and version;
- actor and target identifiers and versions;
- accepted context;
- modifiers;
- difficulty;
- Campaign Preferences;
- resolution mode.

It MUST exclude generated random values before execution.

## 66. Conflicting Reuse

Reusing an OperationId with a conflicting fingerprint MUST fail.

Chronicle MUST not guess which request is intended.

## 67. Failure Before Randomness

If failure occurs before random generation:

- no Dice Roll is created;
- the operation may be safely corrected or retried;
- no result exists.

## 68. Failure After Randomness

If random values were persisted but resolution failed:

- the same raw values MUST be reused;
- the Rule Set resolution may retry;
- no new Roll is generated;
- application remains recoverable.

## 69. Failure After Resolution

If result resolution succeeded but consequence commit failed:

- the resolved result remains preserved;
- consequence application may retry idempotently;
- Narrative continuation must wait when consequences are required first.

## 70. Failure After Commit

If commit succeeded but the client did not receive confirmation:

- retry returns the existing Dice Roll and result;
- no random values are regenerated;
- consequences are not repeated;
- Narrator continuation uses the accepted result.

## 71. Historical Replay

Chronicle MUST be able to replay or inspect a historical mechanical result using:

- exact Rule Set version;
- exact OperationVersion;
- exact calculation version;
- actor and target snapshots;
- modifiers;
- difficulty;
- raw random values;
- Campaign Preferences.

## 72. Replay Purpose

Replay supports:

- diagnostics;
- verification;
- migration testing;
- historical display;
- regression tests.

Replay MUST NOT rewrite historical state.

## 73. Missing Historical Package

If the exact historical package is unavailable:

- persisted result remains authoritative;
- raw values remain visible according to policy;
- full recalculation may be unavailable;
- Campaign play may require restoration or migration;
- Chronicle MUST not reinterpret with the latest package silently.

## 74. Compatibility

A package update MAY remain compatible when:

- operation input shape remains compatible;
- calculation semantics remain identical;
- result payload remains compatible;
- consequence meaning remains unchanged.

Otherwise, a new version is required.

## 75. Operation Alias

A package MAY declare an alias for an old OperationKey.

Aliases are allowed only when semantics remain equivalent or a migration explicitly maps them.

Historical records preserve the original key.

## 76. Calculation Version

CalculationVersion identifies executable mechanical logic.

It SHOULD change when implementation behavior changes even if the public OperationVersion remains compatible.

Historical records SHOULD preserve it.

## 77. Validation Pipeline

Chronicle and the Rule Set collectively validate:

1. request identity;
2. package availability;
3. operation existence;
4. operation version;
5. Campaign ownership;
6. actor eligibility;
7. target eligibility;
8. Scene lifecycle;
9. Character schema compatibility;
10. modifiers;
11. difficulty;
12. random input specification;
13. random input integrity;
14. result shape;
15. consequences;
16. visibility;
17. idempotency;
18. transaction applicability.

## 78. Request Validation

Request validation MUST reject:

- unknown operation;
- incompatible Rule Set version;
- wrong Character schema;
- cross-Campaign target;
- missing actor;
- absent required target;
- invalid Scene;
- conflicting unresolved Roll.

## 79. Result Validation

Chronicle MUST validate that the Rule Set response:

- matches OperationId;
- matches exact versions;
- references the persisted Dice Roll;
- preserves raw values;
- uses registered result keys;
- contains bounded payloads;
- proposes valid consequences;
- respects visibility.

## 80. Rule Set Failure

A Rule Set package failure MUST map to RFC-0018.

Possible codes include:

```text
OperationUnavailable
OperationVersionUnsupported
ActorInvalid
TargetInvalid
ContextInvalid
ModifierInvalid
DifficultyInvalid
RandomInputInvalid
ResolutionFailed
ConsequenceInvalid
HistoricalResolverUnavailable
```

## 81. Retry Classification

Typical classifications:

```text
Invalid actor or target
    → NotRetryable until state changes

Transient package host failure
    → SafeWithSameOperationId

Stale Character version
    → SafeAfterRefresh

Missing historical package
    → RequiresRepair

Invalid consequence
    → RequiresRepair
```

## 82. Security

Mechanical contracts MUST defend against:

- cross-Campaign references;
- oversized modifier collections;
- arbitrary operation names;
- malicious package payloads;
- integer overflow;
- invalid numeric ranges;
- recursive consequence structures;
- unbounded calculation;
- provider-controlled code names;
- hidden-information leakage.

## 83. Numeric Safety

Calculations SHOULD use explicit numeric bounds.

The package MUST define behavior for:

- overflow;
- underflow;
- invalid decimal precision;
- NaN or infinity where relevant;
- negative pools;
- excessive dice count.

## 84. Resource Limits

Chronicle SHOULD impose limits for:

- maximum modifiers;
- maximum pool size;
- maximum generated dice;
- maximum consequence count;
- maximum result payload size;
- maximum calculation duration;
- maximum trace depth.

## 85. Observability

Chronicle SHOULD record:

- OperationId;
- Rule Set identity;
- package version;
- OperationKey;
- OperationVersion;
- calculation version;
- actor and target references;
- pool size;
- modifier count;
- difficulty;
- Dice Roll identifier;
- generic outcome;
- consequence count;
- validation duration;
- resolution duration;
- error code.

## 86. Logging Safety

Logs SHOULD NOT expose:

- hidden target identity when not permitted;
- unrestricted Character Sheet values;
- proprietary source text;
- provider data;
- secret Campaign information.

## 87. Metrics

Useful metrics include:

- operation count by key;
- validation failure rate;
- modifier rejection rate;
- average pool size;
- critical outcome rate;
- resolution latency;
- consequence failure rate;
- retry rate;
- replay mismatch count;
- missing historical package incidents.

## 88. Testing Strategy

### 88.1 Descriptor Tests

Test:

- stable keys;
- versions;
- actor and target requirements;
- modifier policy;
- difficulty policy;
- result registration.

### 88.2 Pool Tests

Test:

- base pool;
- modifiers;
- stacking;
- minimum and maximum;
- zero-pool behavior;
- trace.

### 88.3 Resolution Tests

Test fixed raw values against expected results.

### 88.4 Idempotency Tests

Test duplicate execution at every stage.

### 88.5 Replay Tests

Test exact historical reproduction.

## 89. Golden Mechanical Fixture

A golden fixture SHOULD contain:

```text
Given:
    exact Rule Set and operation versions
    actor snapshot
    target snapshot
    context
    modifiers
    difficulty
    raw random values

Expect:
    pool
    accepted modifiers
    generic outcome
    system result
    consequences
    calculation trace
```

## 90. Required Test Cases

Tests MUST cover:

- valid operation;
- unknown operation;
- incompatible OperationVersion;
- actor missing required field;
- target from another Campaign;
- valid modifier;
- duplicate modifier;
- invalid stacking;
- invalid difficulty;
- zero pool;
- maximum pool;
- deterministic automatic result;
- valid raw dice;
- malformed random input;
- critical success;
- critical failure;
- contested tie;
- consequence proposal;
- invalid consequence target;
- duplicate OperationId;
- conflicting request fingerprint;
- failure before randomness;
- failure after randomness;
- failure after resolution;
- failure after commit;
- historical replay;
- missing historical package;
- package update with changed calculation;
- hidden result filtering;
- excessive dice count;
- integer overflow protection.

## 91. Prohibited Patterns

### 91.1 Narrator Resolves Mechanics

Narrative Intelligence MUST NOT produce authoritative results.

### 91.2 Rule Set Generates Authoritative Randomness

Random generation remains Chronicle-owned.

### 91.3 Rule Set Writes State

The package returns consequence proposals only.

### 91.4 Localized Operation Identity

Operation keys MUST not use translated labels.

### 91.5 Latest Version Reinterprets History

Historical results MUST preserve exact versions.

### 91.6 Retry Rerolls

A retry after persisted randomness MUST reuse the same values.

### 91.7 Prose-Only Mechanical Result

State-relevant results MUST be structured.

### 91.8 Hidden Arithmetic

The result SHOULD provide an explainable calculation trace.

### 91.9 Arbitrary Dynamic Operation

Providers and users MUST not invoke unregistered operation names as executable code.

### 91.10 Consequence Applied Twice

Application must trace consequence identity to the source operation and Roll.

## 92. Current Delivery Decision

The MVP adopts:

- stable namespaced OperationKeys;
- explicit OperationVersion and calculation version;
- separate intent validation, Test building, randomness, resolution, and consequence application;
- Chronicle-owned random generation;
- Rule Set-owned deterministic interpretation;
- typed actor, target, and context snapshots;
- structured modifiers;
- structured difficulty;
- immutable raw random input;
- bounded generic outcomes;
- system-specific result payloads;
- consequence proposals;
- calculation traces;
- idempotent retries;
- exact historical version preservation;
- golden mechanical fixtures;
- no Rule Set persistence access;
- no Narrative Intelligence mechanical authority;
- no silent reinterpretation with latest versions.

## 93. Architecture Horizon

Future evolution MAY include:

- richer contested operations;
- simultaneous group actions;
- card, token, or table-based randomness;
- physical dice integration;
- verifiable randomness;
- multiplayer Roll authorization;
- custom Rule Set operation extensions;
- visual calculation inspectors;
- simulation tools;
- probabilistic previews;
- remote trusted mechanical services.

The MVP MUST NOT implement these capabilities without a later milestone.

## 94. Open Questions

The following remain open:

- What exact serialized request and response schemas will be used?
- Which generic outcomes are required by the first Rule Set?
- How will dice types and special dice be represented?
- Should pool construction and result resolution be separate public interfaces?
- Which modifiers may originate from Narrative Events?
- How will hidden modifiers be presented to the player?
- What exact random generator will the official application use?
- Which generator metadata must be persisted?
- How should contested operations be modeled in MVP?
- Should consequence groups support atomic application?
- How much calculation trace should be stored permanently?
- Which system-specific result payloads belong in the initial Werewolf package?
- How should automatic results appear in the UI?
- What maximum pool and modifier limits are appropriate?
- How should historical package versions be retained and distributed?

These questions require RFC-0031, persistence RFCs, UI RFCs, the initial Rule Set implementation, and technology ADRs.

## 95. Compliance Checklist

An implementation complies when:

- Rule Operations have stable identity and versions;
- actor, target, and context are typed;
- modifiers are validated;
- difficulty is structured;
- Chronicle generates authoritative randomness;
- raw random values are immutable;
- the Rule Set resolves results deterministically;
- generic and system-specific outcomes are preserved;
- consequences remain proposals until Chronicle applies them;
- retries never reroll persisted randomness;
- OperationId prevents duplicate effects;
- calculation versions are recorded;
- historical results preserve exact package semantics;
- cross-Campaign references are rejected;
- mechanical traces are explainable;
- providers never become mechanical authorities.

## 96. Final Principle

A mechanical resolution must be reproducible enough to trust, structured enough to apply, and bounded enough to survive change.

Chronicle remembers the chance that occurred.

The Rule Set explains what that chance meant.
