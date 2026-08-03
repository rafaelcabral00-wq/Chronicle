---
id: ADR-0029
title: Rule Operation Dispatch, Validation, and Mechanical Resolution Pipeline
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
  - ADR-0005
  - ADR-0009
  - ADR-0010
  - ADR-0013
  - ADR-0017
  - ADR-0018
  - ADR-0019
  - ADR-0020
  - ADR-0023
  - ADR-0024
  - ADR-0028
  - RFC-0005
  - RFC-0006
  - RFC-0007
  - RFC-0010
  - RFC-0011
  - RFC-0012
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

> **"The Rule Set calculates. Chronicle decides whether that calculation is valid, relevant, and safe to become history."**

# Rule Operation Dispatch, Validation, and Mechanical Resolution Pipeline

## 1. Status

**Proposed**

This ADR defines Chronicle's Rule Operation dispatch, input assembly, deterministic execution, result validation, mechanical evidence, error handling, and acceptance pipeline.

The decision is:

- expose Rule Set mechanics through explicit versioned Rule Operations;
- resolve operations by package identity, package version, and operation key;
- build immutable operation inputs from authoritative Chronicle state;
- prohibit Rule Operations from loading data or resolving dependencies themselves;
- validate operation input before dispatch;
- execute Rule Operations outside database write transactions;
- require deterministic behavior for identical package version, input, Preferences, and raw Dice values;
- separate mechanical calculation from authoritative state mutation;
- treat Rule Operation output as a proposal;
- independently validate every output before Application code may apply it;
- preserve operation version, package version, input fingerprint, raw Dice evidence, output contract, and accepted result;
- prohibit fallback to Narrative Intelligence when mechanical execution fails;
- use typed failure categories;
- support operation-specific time and resource budgets;
- keep Rule Set exceptions behind a safe Infrastructure boundary;
- permit no hidden randomness, ambient time, filesystem, network, provider, database, UI, or DI container access;
- require golden mechanical fixtures and determinism tests for official Rule Sets;
- keep Chronicle's Application command responsible for persistence and history.

The decision becomes **Accepted** after a mechanics spike proves:

- operation discovery;
- exact package-version resolution;
- typed input construction;
- pre-dispatch validation;
- deterministic execution;
- Dice pool construction;
- Roll resolution;
- application of a mechanical consequence;
- invalid output rejection;
- stale-state rejection;
- Preference-aware behavior;
- operation timeout;
- exception mapping;
- persistence of mechanical evidence;
- replay using persisted raw Dice without rerolling.

## 2. Context

Chronicle must support mechanics from multiple RPG systems without embedding every system rule directly into the core Application.

Typical mechanical actions include:

- validate a Character;
- calculate a derived value;
- build a Dice pool;
- apply modifiers;
- resolve raw Dice values;
- calculate damage;
- calculate healing;
- validate advancement;
- calculate advancement cost;
- apply progression;
- determine exceptional outcomes;
- evaluate package-specific conditions.

These operations must remain system-specific.

However, Rule Set code cannot be allowed to decide:

- what data to load;
- which Campaign is active;
- whether an operation is authorized;
- whether state is stale;
- when to persist;
- what transaction to use;
- whether a provider should be called;
- whether a result becomes Campaign truth.

Chronicle therefore needs a mechanical pipeline that separates:

```text
authoritative input selection
Rule Set calculation
output validation
Domain application
persistence
history
```

ADR-0028 defines package loading and runtime boundaries.

ADR-0018 defines Chronicle-owned randomness.

ADR-0013 defines Application command execution.

This ADR defines the mechanical operation pipeline between those decisions.

## 3. Decision Drivers

The pipeline prioritizes:

1. deterministic mechanics;
2. Chronicle authority;
3. exact package-version behavior;
4. testability;
5. explicit evidence;
6. no hidden data access;
7. safe failure;
8. idempotency;
9. stale-state protection;
10. package isolation;
11. auditability;
12. future multi-system support.

## 4. Decision Summary

Chronicle will use:

```text
Operation Identity
    RuleSetPackageId
    PackageVersion
    OperationKey
    InputContractVersion
    OutputContractVersion

Dispatch
    IRuleOperationDispatcher

Execution
    deterministic official mechanics implementation

Input
    immutable authoritative snapshot

Output
    typed proposal

Validation
    contract
    scope
    state version
    numerical bounds
    semantic invariants
    expected operation kind

Persistence
    Application command only

Randomness
    raw Dice supplied by Chronicle

Failure
    typed
    no provider fallback
    no partial authoritative mutation
```

## 5. Rule Operation Definition

A Rule Operation is a versioned deterministic function exposed by a Rule Set package.

Conceptually:

```text
Input
    +
Package Version
    +
Operation Version
    +
Preferences
    +
Raw Dice where required
    ↓
Deterministic Mechanical Result
```

## 6. Operation Identity

Every operation is identified by:

```text
RuleSetPackageId
PackageVersion
OperationKey
```

Example:

```text
chronicle.ruleset.werewolf-the-apocalypse
1.0.0
chronicle.rules.build-dice-pool
```

## 7. Operation Contract Version

Every operation declares:

```text
InputContractVersion
OutputContractVersion
```

These are separate from the package version.

## 8. Operation Categories

Recommended categories:

```text
Validation
Derivation
DicePoolConstruction
RollResolution
ConsequenceCalculation
ProgressionValidation
ProgressionCalculation
ProgressionApplicationProposal
PreferenceValidation
CharacterMigrationValidation
SessionFinalizationMechanics
```

## 9. Pure Calculation Preference

Rule Operations SHOULD be pure calculations.

They receive all required information explicitly and return one result.

## 10. No Data Loading

Rule Operations MUST NOT:

- query repositories;
- open DbContext;
- call read models;
- inspect global Campaign state;
- resolve current Character;
- load package files on demand.

Chronicle loads and passes validated input.

## 11. No State Mutation

Rule Operations MUST NOT mutate:

- Campaign aggregates;
- Character entities;
- Preferences;
- Work Items;
- Operation Records;
- database rows;
- UI state.

## 12. Dispatcher Port

Chronicle defines:

```text
IRuleOperationDispatcher
```

Conceptually:

```csharp
public interface IRuleOperationDispatcher
{
    Task<RuleOperationResult<TOutput>> ExecuteAsync<TInput, TOutput>(
        RuleOperationRequest<TInput> request,
        CancellationToken cancellationToken);
}
```

## 13. Request Contract

A Rule Operation request SHOULD include:

```text
RuleSetPackageId
PackageVersion
OperationKey
InputContractVersion
ExpectedOutputContractVersion
OperationId
CampaignId
ExpectedCampaignVersion
Input
ExecutionBudget
```

## 14. No Secrets in Request

A Rule Operation request contains no:

- provider credential;
- access token;
- API key;
- local secret;
- unrestricted file path.

## 15. Request Ownership

The Application service or command handler constructs the request.

Presentation and provider code do not construct Rule Operation inputs directly.

## 16. Input Assembly

Input assembly maps authoritative Chronicle state into an immutable operation DTO.

## 17. Input Sources

Inputs may include:

- Character mechanical snapshot;
- target snapshot;
- action key;
- accepted modifiers;
- Campaign Preferences;
- fictional time;
- raw Dice values;
- current aggregate version;
- package-specific field payload;
- operation context.

## 18. Mechanical Snapshot

A mechanical snapshot contains only state needed by the operation.

It should avoid including:

- full transcript;
- unrelated Characters;
- hidden provider context;
- unrestricted Campaign history;
- credentials;
- Infrastructure metadata.

## 19. Snapshot Immutability

Operation input DTOs are immutable.

Rule Set implementations must not retain references to mutable aggregate objects.

## 20. Snapshot Version

Inputs SHOULD contain:

```text
CampaignVersion
CharacterVersion where applicable
PreferenceVersion
CharacterSchemaVersion
```

## 21. Input Validation Layers

Input validation occurs in layers:

```text
Contract Validation
Chronicle Scope Validation
State-Version Validation
Rule Set Input Validation
Operation-Specific Validation
```

## 22. Contract Validation

Checks:

- contract version;
- required fields;
- field types;
- collection bounds;
- semantic-key grammar;
- identifier syntax;
- allowed discriminator values.

## 23. Scope Validation

Checks:

- Character belongs to Campaign;
- target belongs to allowed scope;
- Preference belongs to Campaign;
- Dice Roll belongs to intended operation;
- references resolve exactly once.

## 24. State-Version Validation

Checks expected versions against authoritative state.

If stale:

```text
ruleset.input-stale
```

The operation is not executed.

## 25. Rule Set Input Validation

The resolved package may perform deterministic input validation through an approved validator.

## 26. Invalid Input

Invalid input produces a typed validation result.

It does not throw for expected user or state errors.

## 27. Operation Resolution

The dispatcher resolves the exact operation implementation from:

```text
RuleSetPackageId
PackageVersion
OperationKey
```

## 28. No Nearest-Version Resolution

If the exact package version is unavailable, execution fails.

Chronicle does not choose the nearest installed version.

## 29. Missing Operation

If the package does not declare the operation:

```text
ruleset.operation-not-found
```

## 30. Contract Mismatch

If the request contract does not match the operation declaration:

```text
ruleset.operation-contract-mismatch
```

## 31. Execution Boundary

The dispatcher calls one approved mechanics implementation through a narrow interface.

## 32. Mechanics Interface

Conceptually:

```csharp
public interface IRuleOperation<TInput, TOutput>
{
    RuleOperationExecution<TOutput> Execute(
        TInput input,
        RuleOperationExecutionContext context);
}
```

## 33. Execution Context

The execution context MAY include:

```text
Package identity
Package version
Operation key
Operation version
Preferences snapshot
Cancellation token
Execution budget
```

It MUST NOT include:

- DbContext;
- repositories;
- service provider;
- random source;
- clock;
- provider adapter;
- filesystem;
- HTTP client;
- UI service.

## 34. Synchronous Pure Core

The core mechanics calculation SHOULD be synchronous where practical.

Asynchronous operation is allowed only for bounded internal computation that genuinely requires it.

No external I/O is allowed.

## 35. Execution Outside Write Transaction

Rule Operation execution occurs before the authoritative write transaction.

### Rationale

This avoids holding SQLite writer locks during mechanical calculation.

## 36. Read Snapshot and Write Recheck

Recommended flow:

1. load authoritative read state;
2. build immutable input;
3. record expected versions;
4. execute Rule Operation;
5. validate output;
6. enter mutation coordination;
7. reload or recheck versions;
8. apply accepted result;
9. commit.

## 37. Stale After Execution

If state changed after Rule Operation execution:

- output is not applied;
- caller receives stale-state conflict;
- operation may be recomputed from fresh state;
- raw Dice handling follows the specific workflow.

## 38. Stale Dice Workflow

For a committed Dice Roll, raw values remain fixed.

If mechanical application becomes stale:

- reload current state;
- rerun deterministic resolution with the same raw values when semantically valid;
- or require explicit user recovery;
- never reroll automatically.

## 39. Determinism Requirement

For identical:

```text
package version
operation key
operation contract version
input
Preferences
raw Dice
```

the output must be semantically identical.

## 40. Deterministic Serialization

Official operations SHOULD support canonical output serialization for determinism tests.

## 41. No Ambient Randomness

Rule Set code cannot use:

```text
Random
Random.Shared
RandomNumberGenerator
GUID generation as chance
timestamp-seeded values
```

## 42. No Ambient Time

Rule Set code cannot use:

```text
DateTime.Now
DateTime.UtcNow
DateTimeOffset.UtcNow
Stopwatch for rule decisions
```

Timing for diagnostics belongs outside the mechanics implementation.

## 43. Dice Pool Construction

A Dice pool operation may calculate:

```text
base pool
attribute contribution
ability contribution
bonuses
penalties
minimum pool
maximum pool
special rule flags
explanation terms
```

## 44. Dice Pool Output

Recommended output:

```text
DiceCount
DieType
AppliedModifiers
RejectedModifiers
SpecialRules
MechanicalExplanationKeys
OperationEvidence
```

## 45. Dice Pool Acceptance

Chronicle validates:

- count bounds;
- supported die type;
- modifier identity;
- Character field ownership;
- Preference compatibility;
- no hidden Roll result.

## 46. Roll Request

After Dice pool acceptance, Chronicle creates a Dice Roll request.

The Rule Set does not generate raw values.

## 47. Raw Dice Input

Roll resolution receives raw values from Chronicle's authoritative Dice subsystem.

## 48. Raw Dice Validation

Before dispatch, Chronicle validates:

- count matches accepted pool or approved special rule;
- die bounds;
- stable positions;
- Dice Roll identity;
- operation relationship;
- no duplicate or missing value.

## 49. Additional Draws

Some systems may require:

- exploding Dice;
- rerolls;
- special bonus Dice;
- cancellation Dice.

The MVP pipeline must model these explicitly.

## 50. Additional Draw Request

A Rule Operation MAY return:

```text
AdditionalDiceRequest
```

only when the package contract declares iterative resolution.

## 51. Iterative Resolution

Recommended controlled loop:

1. execute with current raw values;
2. validate additional-draw request;
3. Chronicle generates requested additional raw values;
4. append them with stable sequence;
5. persist when workflow requires;
6. execute next deterministic stage;
7. stop at declared maximum iterations.

## 52. Iteration Limits

Every iterative operation declares:

```text
MaximumIterations
MaximumAdditionalDice
MaximumTotalDice
```

## 53. Infinite Loop Protection

Chronicle rejects additional-draw requests beyond declared limits.

## 54. No Hidden Reroll

A Rule Set cannot discard or replace raw values without explicitly declaring the mechanical treatment.

## 55. Roll Resolution Output

Recommended output may include:

```text
OutcomeKind
SuccessCount
FailureCount
ExceptionalState
BotchState
CancelledDice
ExplodedDice
AppliedSpecialRules
ConsequenceProposal
MechanicalExplanationKeys
Evidence
```

## 56. Outcome Kind

Outcome kinds are stable semantic values declared by the package.

## 57. Mechanical Explanation

Rule Set output may include machine-stable explanation keys and structured terms.

It SHOULD NOT produce unrestricted narrative prose as authoritative mechanics.

## 58. Narrative Rendering

The Narrator may later render the accepted mechanical outcome into prose.

The mechanical result remains authoritative regardless of prose.

## 59. Consequence Proposal

A Rule Operation may propose state consequences such as:

- health change;
- resource change;
- condition added;
- condition removed;
- advancement cost;
- progression entry;
- derived-value update.

## 60. Proposal Is Not Mutation

Chronicle Application code applies the proposal through Domain methods.

## 61. Output Validation Layers

Output validation occurs in layers:

```text
Contract Validation
Operation-Kind Validation
Numerical-Bound Validation
Reference Validation
State-Version Validation
Domain Applicability Validation
Evidence Validation
```

## 62. Contract Validation

Checks:

- output version;
- result discriminator;
- required fields;
- bounded collections;
- supported semantic values.

## 63. Operation-Kind Validation

A Dice pool operation cannot return a progression result.

## 64. Numerical Bounds

Chronicle validates:

- Dice counts;
- success counts;
- resource deltas;
- damage bounds;
- advancement costs;
- sequence values;
- iteration counts.

## 65. Reference Validation

Every output reference must point to an input-authorized entity or declared package concept.

## 66. Unauthorized Reference

An output referencing an unrelated Character or Campaign is rejected.

## 67. State-Version Validation

Output must correspond to the input versions.

It cannot claim a later version.

## 68. Domain Applicability

Domain methods still decide whether the proposed transition is valid.

Example:

```text
Rule Set proposes damage
Domain rejects damage on finalized immutable state
```

## 69. Evidence Validation

Mechanical evidence must match:

- operation identity;
- raw Dice;
- modifiers;
- Preferences;
- package version;
- output.

## 70. Accepted Result

A result becomes accepted only after:

- output validation;
- mutation recheck;
- Domain application;
- persistence commit.

## 71. Mechanical Evidence Record

Chronicle SHOULD persist evidence containing:

```text
RuleSetPackageId
PackageVersion
OperationKey
InputContractVersion
OutputContractVersion
OperationImplementationVersion
InputFingerprint
PreferenceVersion
CharacterSchemaVersion
ExpectedStateVersion
RawDiceReference
OutputContract
AcceptedAtUtc
OperationId
```

## 72. Input Fingerprint

The input fingerprint uses a canonical safe DTO.

It must exclude:

- secrets;
- irrelevant timestamps;
- local paths;
- provider metadata;
- nondeterministic ordering.

## 73. Full Input Persistence

Chronicle does not need to persist every full Rule Operation input if authoritative state and references can reconstruct it.

For operations requiring exact audit replay, a bounded input snapshot MAY be persisted.

## 74. Replay

Replay uses:

- persisted package version;
- operation version;
- Preferences;
- accepted state snapshot or reconstructable state;
- persisted raw Dice.

It never generates new Dice.

## 75. Replay Purpose

Replay may be used for:

- tests;
- diagnostics;
- migration validation;
- corruption detection;
- future audit UI.

It does not rewrite accepted history automatically.

## 76. Output Persistence

Persist the accepted typed mechanical result or sufficient evidence to reconstruct it safely.

## 77. Provider Boundary

Narrative Intelligence receives the accepted mechanical outcome.

It does not receive authority to reinterpret success into failure or failure into success.

## 78. No Provider Fallback

If Rule Operation execution fails:

- Chronicle returns a mechanical failure;
- narration pauses;
- user sees recovery or retry options;
- provider improvisation is prohibited.

## 79. Failure Categories

Recommended categories:

```text
InputInvalid
StateStale
PackageUnavailable
OperationUnavailable
ContractMismatch
ExecutionTimeout
ExecutionCancelled
ExecutionFault
OutputInvalid
DomainRejected
PersistenceConflict
RecoveryRequired
```

## 80. Error Codes

Recommended errors:

```text
ruleset.input-invalid
ruleset.input-stale
ruleset.package-unavailable
ruleset.operation-not-found
ruleset.operation-contract-mismatch
ruleset.execution-timeout
ruleset.execution-cancelled
ruleset.execution-failed
ruleset.output-contract-invalid
ruleset.output-reference-invalid
ruleset.output-out-of-range
ruleset.output-nondeterministic
ruleset.domain-rejected
ruleset.recovery-required
```

## 81. Expected Validation Error

Expected mechanical invalidity is returned as a typed result, not an exception.

## 82. Unexpected Exception

Unexpected mechanics exceptions are caught at the dispatcher boundary.

They map to:

```text
ruleset.execution-failed
```

with a safe reference code.

## 83. Stack Trace

Stack traces remain in protected Development diagnostics.

They do not reach users or provider prompts.

## 84. Timeout

Each operation category may define a maximum execution duration.

## 85. Timeout Enforcement

Because official operations run in process, timeout is cooperative and diagnostic.

A truly hostile infinite loop cannot be safely terminated in-process.

This reinforces the prohibition on arbitrary third-party executable mechanics.

## 86. Cancellation

Operations SHOULD check cancellation in bounded loops and large calculations.

## 87. Resource Budget

An execution budget MAY include:

```text
MaximumInputItems
MaximumOutputItems
MaximumIterations
MaximumDice
MaximumRecursionDepth
MaximumDuration
```

## 88. Memory Budget

Large unbounded allocation is prohibited.

Official operations must use bounded collections derived from validated input limits.

## 89. Dispatcher Lifetime

The Rule Operation dispatcher may be singleton when it holds immutable registries and no scoped state.

## 90. Mechanics Instance Lifetime

Approved mechanics implementations SHOULD be stateless singleton services.

Stateful operation instances are discouraged.

## 91. Thread Safety

Singleton mechanics implementations must be thread-safe.

## 92. Caching

Pure derivation results MAY be cached only when:

- input fingerprint includes every semantic dependency;
- package version is included;
- Preference version is included;
- cache is nonauthoritative;
- invalidation is safe.

## 93. No Cache for Accepted Mutation Decision Alone

A cache result still undergoes current-state validation before application.

## 94. Dispatch Registration

Operation registration maps:

```text
MechanicsImplementationKey
OperationKey
Input Type
Output Type
Contract Versions
```

## 95. Duplicate Operation Registration

Duplicate active registrations for the same exact operation identity are startup errors.

## 96. Missing Registration

A package manifest declaring an unregistered official operation becomes incompatible.

## 97. Generic Operation Interface

A generic interface may be used internally.

Runtime dispatch must still validate the concrete input and output contract types.

## 98. No Reflection Guessing

The dispatcher must not infer arbitrary operation types from untrusted CLR type names.

## 99. Operation Catalog

The active package descriptor exposes an immutable operation catalog.

## 100. Catalog Entry

Recommended metadata:

```text
OperationKey
Category
InputContractVersion
OutputContractVersion
ImplementationKey
Deterministic
RequiresRawDice
SupportsIterativeDice
Limits
PreferenceKeys
```

## 101. Preference Resolution

Chronicle resolves effective Preferences before dispatch.

## 102. Preference Snapshot

The operation receives a validated immutable Preference snapshot.

## 103. Missing Preference

Missing required Preference yields typed input invalidity unless the package defines a default.

## 104. Unknown Preference

Unknown Preferences are not passed to the operation unless the contract explicitly supports extension data.

## 105. Character Field Resolution

Package-defined Character fields are mapped by semantic field key.

## 106. Missing Character Field

Missing required mechanical field blocks execution.

## 107. Derived Values

Derived values may be:

- calculated on demand;
- persisted as validated projection;
- cached.

The source operation and version must remain known.

## 108. Derived Value Cycles

Circular derivation dependencies are rejected during package validation.

## 109. Progression Validation

Progression operations may calculate:

- eligibility;
- cost;
- prerequisites;
- resulting value;
- ledger proposal.

## 110. Progression Application

Chronicle applies progression through Domain methods and appends an authoritative ledger entry.

## 111. No Direct Character Rewrite

Rule Set output cannot provide an unrestricted replacement Character Sheet.

It proposes bounded field changes or a versioned migration result.

## 112. Character Validation

A Character validation operation returns structured issues:

```text
IssueKey
Severity
FieldKey
Parameters
Blocking
```

## 113. Validation Localization

Issue keys are localized by Presentation using package resources.

## 114. Session Finalization Mechanics

A Rule Set may calculate package-specific finalization results.

The Archivist does not own these mechanics.

## 115. Finalization Input

Input may include:

- accepted Session outcomes;
- progression evidence;
- Rule Set-specific counters;
- Preferences;
- Character state versions.

## 116. Finalization Output

Output remains a proposal validated and applied by Chronicle.

## 117. Idempotency

The Application command owns OperationId and persistence idempotency.

Rule Operation execution itself may be repeated because it is deterministic and side-effect free.

## 118. Duplicate Command

A duplicate committed OperationId returns the persisted result without mechanical reapplication.

## 119. Mechanical Re-execution

Chronicle MAY re-execute mechanics for diagnostics, but it must compare against persisted evidence and never create a second effect.

## 120. Nondeterminism Detection

If repeated official execution with identical inputs produces different outputs:

- package implementation is considered invalid;
- affected operation may be disabled;
- diagnostics record a nondeterminism failure;
- automatic reapplication is blocked.

## 121. Package Upgrade

A newer package version may produce different mechanics.

That is expected only after explicit Campaign upgrade.

## 122. Historical Version Availability

Historical evidence remains readable even if the old implementation is unavailable.

Exact mechanical replay may require retaining the old package implementation.

## 123. Implementation Retention

Official mechanics modules required by existing Campaigns should remain available across supported application versions or have explicit compatibility migration.

## 124. Logging

Rule Operation logs MAY include:

- package ID;
- package version;
- operation key;
- contract versions;
- input fingerprint;
- duration;
- result category;
- validation failure key;
- OperationId;
- CampaignId.

They MUST NOT include full Character Sheets, narrative content, Secrets, or credentials by default.

## 125. Metrics

Useful metrics include:

```text
RuleOperationExecutionDuration
RuleOperationValidationFailureCount
RuleOperationTimeoutCount
RuleOperationExceptionCount
RuleOperationStaleStateCount
RuleOperationOutputRejectedCount
RuleOperationDeterminismFailureCount
```

## 126. Observability Boundary

Performance timing is measured outside the mechanics code using the monotonic clock.

## 127. Test Kit

Chronicle SHOULD provide a Rule Operation test kit.

Recommended capabilities:

```text
contract validation
determinism repetition
golden fixtures
boundary values
invalid references
Preference matrix
raw Dice matrix
timeout simulation
output validator
round-trip evidence
```

## 128. Golden Fixtures

Official Rule Sets MUST define synthetic golden cases.

## 129. Golden Fixture Content

Each fixture SHOULD include:

```text
package version
operation key
input contract
Preferences
raw Dice when required
expected output contract
expected validation status
```

## 130. Property Tests

Property tests SHOULD cover:

- result bounds;
- monotonic cost rules where applicable;
- no impossible Dice count;
- no negative resource beyond policy;
- deterministic repetition;
- reference containment.

## 131. Mutation Tests

Mutation testing MAY be used for critical mechanics to ensure tests detect altered rules.

## 132. Testing Strategy

The implementation requires:

```text
Contract Tests
Dispatcher Tests
Determinism Tests
Golden Mechanical Tests
Property Tests
Concurrency Tests
Timeout Tests
Persistence Integration Tests
Architecture Tests
Security Tests
```

## 133. Dispatcher Tests

Tests MUST cover:

- exact operation resolution;
- missing package;
- missing version;
- missing operation;
- duplicate registration;
- contract mismatch;
- invalid input;
- valid output;
- invalid output;
- exception mapping.

## 134. Dice Tests

Tests MUST cover:

- pool construction;
- raw Dice count;
- die bounds;
- stable positions;
- additional draws;
- iteration maximum;
- no reroll on retry;
- identical resolution on replay.

## 135. Stale-State Tests

Tests MUST cover:

- stale before dispatch;
- state changes during execution;
- stale before commit;
- re-resolution with same raw Dice;
- conflict result.

## 136. Preference Tests

Tests MUST cover:

- default;
- explicit value;
- invalid value;
- missing required value;
- version change;
- operation not supporting Preference.

## 137. Consequence Tests

Tests MUST cover:

- valid Domain application;
- Domain rejection;
- out-of-range delta;
- unauthorized target;
- partial proposal rejection;
- no mutation before validation.

## 138. Timeout Tests

Tests SHOULD include a deliberately slow official test operation and verify:

- cancellation request;
- timeout classification;
- no persistence;
- no provider fallback.

## 139. Concurrency Tests

Tests MUST prove parallel executions:

- do not share mutable state;
- do not mix inputs;
- return deterministic outputs;
- do not change registry state.

## 140. Persistence Tests

Tests MUST prove accepted evidence includes:

- exact package version;
- operation key;
- contract versions;
- input fingerprint;
- raw Dice reference;
- accepted output;
- OperationId.

## 141. Security Tests

Tests MUST prove mechanics receive no:

- DbContext;
- repository;
- filesystem;
- network client;
- secret manager;
- provider adapter;
- random source;
- system clock;
- UI service;
- service provider.

## 142. Required Test Cases

Tests MUST cover:

- Character validation;
- derived value;
- Dice pool;
- Roll success;
- Roll failure;
- exceptional result;
- iterative Dice;
- damage proposal;
- progression validation;
- progression cost;
- invalid Character field;
- stale Campaign;
- package unavailable;
- operation unavailable;
- output reference violation;
- output numerical violation;
- exception;
- timeout;
- replay;
- duplicate OperationId;
- package version difference.

## 143. Architecture Tests

Architecture tests MUST reject:

- mechanics referencing EF Core;
- mechanics referencing repositories;
- mechanics using `System.IO`;
- mechanics using HTTP or sockets;
- mechanics using provider SDKs;
- mechanics using credentials;
- mechanics using ambient clock;
- mechanics using random APIs;
- mechanics resolving DI services;
- mechanics mutating Domain aggregate directly;
- command handler accepting unvalidated Rule Set output;
- provider fallback for mechanical failure.

## 144. Prohibited Patterns

### 144.1 Rule Set Loads Its Own Character

Chronicle constructs the input.

### 144.2 Rule Set Mutates Campaign State

It returns a proposal.

### 144.3 Mechanics Inside Open Write Transaction

Calculate first, recheck, then commit.

### 144.4 Provider Resolves Failed Mechanics

Mechanical failure remains explicit.

### 144.5 Raw Dice Generated by Rule Set

Chronicle owns randomness.

### 144.6 Output Trusted Because Package Is Official

Every output is validated.

### 144.7 Reroll on Stale State

Reuse committed raw Dice.

### 144.8 Arbitrary Character Sheet Replacement

Use bounded typed changes.

### 144.9 Hidden Service Locator

Execution context is capability-limited.

### 144.10 Nondeterministic Official Mechanics

Identical inputs must produce identical results.

## 145. Alternatives Considered

### Put Mechanics Directly in Command Handlers

Rejected because system-specific rules would spread across core Application code.

### Let Rule Set Mutate Domain Aggregates Directly

Rejected because it would blur authority, persistence, and package isolation.

### Execute Mechanics Inside the Write Transaction

Rejected because long or faulty calculations would hold SQLite writer locks.

### Provider-Based Mechanical Resolution

Rejected because provider output is nondeterministic and advisory.

### One Generic Dictionary Input and Output

Flexible but unsafe, weakly typed, difficult to validate, and poor for compatibility.

### Arbitrary Scripting

Deferred because deterministic sandboxing and security require a separate runtime design.

## 146. Consequences

### Positive

- deterministic and testable mechanics;
- exact package-version reproducibility;
- Chronicle remains authoritative;
- no hidden persistence or external I/O;
- strong stale-state handling;
- auditable mechanical evidence;
- provider cannot override mechanics;
- clean multi-system extensibility.

### Negative

- more DTOs and validators;
- Rule Set result application requires explicit mapping;
- iterative Dice mechanics add pipeline complexity;
- exact historical replay may require retaining old implementations;
- large mechanical snapshots require careful design;
- in-process timeout cannot safely terminate hostile code.

## 147. Risks

### Input Snapshot Omits a Required Dependency

Mitigation:

- operation contract metadata;
- golden fixtures;
- input builders;
- fingerprint versioning.

### Output Validator Duplicates Rule Logic

Mitigation:

- validate authority and bounds, not recompute every rule;
- use invariant-focused validation;
- determinism tests.

### Stale State After Expensive Resolution

Mitigation:

- Campaign mutation coordination;
- short execution;
- version recheck;
- deterministic recomputation.

### Historical Operation Cannot Replay

Mitigation:

- persist accepted output and evidence;
- retain supported package versions;
- migration policy.

### In-Process Infinite Loop

Mitigation:

- first-party reviewed code only;
- operation budgets;
- no arbitrary runtime plugins;
- future process isolation.

## 148. Technology Spike

Before acceptance, implement:

1. Rule Operation catalog;
2. dispatcher;
3. typed request and result contracts;
4. input builder;
5. input validators;
6. official WTA mechanics registry;
7. Dice pool operation;
8. Roll resolution operation;
9. iterative additional-draw flow;
10. output validators;
11. consequence proposal application;
12. mechanical evidence persistence;
13. stale-state recheck;
14. determinism test kit;
15. forbidden-capability architecture tests.

## 149. Spike Acceptance

The spike passes when:

- exact package-version operation resolution works;
- invalid or stale input never executes;
- mechanics run outside the write transaction;
- identical fixtures produce identical outputs repeatedly;
- raw Dice are Chronicle-supplied and persisted;
- iterative draws remain bounded and explicit;
- invalid outputs are rejected before Domain mutation;
- accepted consequences apply through Domain methods;
- provider fallback is impossible;
- persisted evidence supports replay without reroll;
- mechanics have no forbidden dependencies.

## 150. Definition of Compliance

An implementation complies when:

- Rule Operations are explicit, typed, versioned, and package-resolved;
- Chronicle builds immutable authoritative inputs;
- input validation precedes dispatch;
- execution is deterministic and side-effect free;
- no database, filesystem, network, provider, credential, UI, time, randomness, or DI access exists;
- output is treated as a proposal;
- Chronicle validates and applies proposals through Domain methods;
- persistence occurs only through the Application transaction;
- package and operation versions are preserved;
- raw Dice evidence is retained;
- failures are typed and never replaced by provider improvisation;
- official operations pass golden and determinism tests.

## 151. Review Triggers

This ADR must be reviewed if:

- third-party executable mechanics are introduced;
- mechanics move out of process;
- a scripting or WebAssembly runtime is added;
- server-side mechanics execute at scale;
- collaborative play requires shared rule authority;
- streaming mechanical results become necessary;
- package-defined operations require external data;
- exact replay becomes a formal product feature;
- mechanics require high-cost simulation;
- multi-stage combat engines exceed the current operation model.

## 152. Deferred Decisions

Later ADRs MAY define:

- exact mechanical evidence schema;
- exact input fingerprint canonicalization;
- out-of-process mechanics protocol;
- scripting or WebAssembly execution;
- deterministic simulation engine;
- package-operation performance budgets;
- historical implementation retention period;
- advanced multi-stage combat workflow;
- mechanical replay UI;
- formal proof or verification for selected mechanics.

## 153. Final Decision

Chronicle will dispatch Rule Set mechanics through explicit, typed, versioned Rule Operations.

Chronicle will assemble the input, resolve the exact package version, provide raw Dice, validate the output, apply the accepted proposal through Domain behavior, and persist the result.

The Rule Set may calculate what should happen under its rules.

Only Chronicle may decide whether that calculation becomes part of the Campaign.
