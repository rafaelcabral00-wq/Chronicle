---
id: ADR-0018
title: Clock, Randomness, and Deterministic Execution Services
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
  - ADR-0010
  - ADR-0013
  - ADR-0014
  - ADR-0016
  - ADR-0017
  - RFC-0006
  - RFC-0012
  - RFC-0017
  - RFC-0018
  - RFC-0019
  - RFC-0033
  - RFC-0036
  - RFC-0040
  - RFC-0042
---

> **"Chronicle may use time and chance. It must never let either become an invisible dependency."**

# Clock, Randomness, and Deterministic Execution Services

## 1. Status

**Proposed**

This ADR defines Chronicle's abstractions and implementation rules for:

- current time;
- elapsed time;
- delays;
- identifier timestamps;
- retry scheduling;
- lease expiry;
- Dice randomness;
- nonauthoritative UI randomness;
- deterministic tests;
- replay-safe workflows.

The decision is:

- use injected clock and randomness services;
- prohibit direct use of ambient system time and global random generators in Domain and Application code;
- use UTC for persisted instants;
- distinguish wall-clock time from monotonic elapsed time;
- make Chronicle-owned Dice randomness explicit and auditable;
- persist raw Dice values before any narrative continuation;
- never regenerate authoritative randomness during retry or recovery;
- use cryptographically strong operating-system randomness for authoritative Dice seed material;
- avoid persisting a reusable global Dice seed for the MVP;
- use deterministic scripted implementations in tests;
- inject delay and scheduler abstractions into retrying and leased workflows;
- keep UI-only animation randomness separate from authoritative randomness;
- prohibit Narrative Intelligence from generating authoritative random values.

The decision becomes **Accepted** after a spike proves:

- fake-time tests;
- monotonic elapsed-time measurement;
- retry scheduling;
- lease expiry and reclaim;
- clock-regression handling;
- UUID v7 generation under fake clock;
- authoritative Dice generation;
- Dice persistence and replay;
- restart after Roll commit without reroll;
- deterministic statistical and rule-resolution tests;
- separation between animation and authoritative outcomes.

## 2. Context

Chronicle depends on time and randomness in many places.

Time is used for:

- timestamps;
- Session lifecycle records;
- Operation Records;
- Work Item leases;
- retry scheduling;
- backup manifests;
- migration metadata;
- log events;
- provider latency;
- UI status;
- UUID v7 generation.

Randomness is used for:

- authoritative Dice Rolls;
- possibly future randomized Campaign generation aids;
- UI animations;
- test fixtures;
- backoff jitter;
- identifiers.

These uses do not share the same authority or reproducibility requirements.

The main risks are:

- tests depending on the real clock;
- retry behavior becoming slow or flaky;
- clock changes invalidating leases;
- hidden calls to `DateTime.UtcNow`;
- duplicate Dice after retry;
- provider-generated randomness being accepted;
- UI animation being confused with the real result;
- static global `Random` state making tests nondeterministic;
- seed reuse introducing predictable outcomes;
- elapsed time measured with a wall clock that can move backward.

RFC-0012 defines Chronicle-owned Dice resolution.

ADR-0010 selects UUID v7 for generated identities.

ADR-0014 defines leases and delayed retries.

This ADR provides the execution primitives those decisions require.

## 3. Decision Drivers

The decision prioritizes:

1. deterministic tests;
2. authoritative Dice integrity;
3. explicit dependency boundaries;
4. restart-safe recovery;
5. monotonic duration measurement;
6. clock-regression tolerance;
7. secure random generation;
8. auditable outcomes;
9. low implementation complexity;
10. clear separation between Domain, Infrastructure, and Presentation concerns.

## 4. Decision Summary

Chronicle will use:

```text
Persisted Instants
    UTC

Current Time
    IClock

Elapsed Time
    IMonotonicClock or Stopwatch-backed abstraction

Delays
    IDelayScheduler

Authoritative Dice Randomness
    IAuthoritativeRandomSource

General Nonauthoritative Randomness
    INonAuthoritativeRandomSource where needed

Production Clock
    SystemClock

Test Clock
    FakeClock

Production Authoritative Randomness
    Operating-system cryptographic random source

Test Randomness
    ScriptedRandomSource
    Seeded deterministic source for statistical tests

Dice Recovery
    load persisted raw values
    never regenerate

UI Dice Animation
    presentation-only
    never source of result
```

## 5. Time Terminology

Chronicle distinguishes:

### Instant

A point on the UTC timeline.

### Local Display Time

A Presentation formatting of an Instant in the user's timezone.

### Duration

An amount of elapsed time.

### Monotonic Timestamp

A process-local measurement suitable for durations and timeout calculation.

### Scheduled Instant

A persisted UTC Instant at which work may become eligible.

## 6. UTC Persistence

All persisted timestamps MUST represent UTC.

Examples:

```text
CreatedAtUtc
UpdatedAtUtc
StartedAtUtc
CommittedAtUtc
LeaseExpiresAtUtc
NextAttemptAtUtc
```

Field names SHOULD include `Utc` where ambiguity would otherwise exist.

## 7. Local Time

Local time is a Presentation concern.

Domain and Application logic MUST NOT depend on the operating system's local timezone unless a specific future feature explicitly requires calendar-local behavior.

## 8. Clock Port

Chronicle will define a small clock abstraction.

Conceptually:

```csharp
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
```

A richer Instant type MAY be adopted later, but the MVP does not require a separate time library.

## 9. Clock Ownership

`IClock` is available to:

- Application services;
- Infrastructure services;
- Domain factories or methods through explicit time input where required.

Domain entities SHOULD prefer receiving the relevant Instant as a method parameter rather than resolving the clock themselves.

## 10. No Static Ambient Time

Production Domain and Application code MUST NOT directly use:

```text
DateTime.Now
DateTime.UtcNow
DateTimeOffset.Now
DateTimeOffset.UtcNow
Environment.TickCount for Domain time
```

Approved Infrastructure adapters may use system APIs only inside the concrete clock implementation or narrowly reviewed platform code.

## 11. System Clock Implementation

The production implementation is:

```text
SystemClock
```

It returns the operating system's current UTC time.

## 12. Fake Clock

Tests use:

```text
FakeClock
```

It SHOULD support:

- fixed current time;
- explicit advance;
- explicit set;
- clock-regression simulation;
- thread-safe access;
- deterministic scheduled workflows.

## 13. Clock Advancement

Tests advance fake time deliberately.

They MUST NOT use real sleep merely to allow a lease or retry interval to pass.

## 14. Wall Clock Versus Elapsed Time

Wall-clock time can move because of:

- synchronization;
- manual change;
- timezone changes;
- virtualization behavior;
- operating-system correction.

Therefore, wall clock is not the preferred source for measuring in-process elapsed duration.

## 15. Monotonic Clock

Chronicle SHOULD define a monotonic timing abstraction.

Conceptually:

```csharp
public interface IMonotonicClock
{
    MonotonicTimestamp GetTimestamp();
    TimeSpan GetElapsedTime(
        MonotonicTimestamp start,
        MonotonicTimestamp end);
}
```

The production implementation may use `Stopwatch`.

## 16. Monotonic Clock Uses

Use monotonic time for:

- operation duration;
- provider latency;
- query duration;
- transaction duration;
- timeout enforcement inside one process;
- performance measurements.

## 17. Wall Clock Uses

Use UTC wall-clock Instants for:

- persisted timestamps;
- scheduling future Work Item eligibility;
- leases that must survive restart;
- manifests;
- audit metadata;
- user-visible chronology.

## 18. Delay Scheduler

Chronicle will define an injectable delay abstraction.

Conceptually:

```csharp
public interface IDelayScheduler
{
    Task DelayAsync(
        TimeSpan delay,
        CancellationToken cancellationToken);
}
```

## 19. Delay Uses

The delay abstraction is used for:

- bounded transport retries;
- worker polling;
- backoff;
- lease renewal intervals;
- debounce where implemented outside Presentation.

## 20. No Direct Delay in Testable Logic

Application and worker orchestration SHOULD NOT call `Task.Delay` directly.

Concrete delay infrastructure may.

## 21. Scheduler and Persisted Retry

A Work Item does not remain in a process delay for its entire retry interval.

Instead:

1. calculate `NextAttemptAtUtc`;
2. persist it;
3. release execution resources;
4. wake or poll when due.

## 22. Backoff Policy

Backoff calculation SHOULD be a pure deterministic service given:

- attempt number;
- policy;
- provider retry guidance;
- jitter input.

## 23. Retry Jitter

Retry jitter uses nonauthoritative randomness.

It MUST NOT use the authoritative Dice source.

## 24. Jitter Reproducibility

Tests use a scripted or seeded jitter source.

Production may use ordinary secure or pseudorandom input because jitter is not Campaign truth.

## 25. Lease Time

Work Item lease expiry is persisted as UTC.

Lease ownership also uses:

- owner ID;
- expected entity version;
- status;
- transactional update.

Time alone never proves ownership.

## 26. Clock Regression and Leases

If wall clock moves backward:

- an active lease may appear valid longer;
- the system must still remain safe;
- ownership and versions prevent conflicting updates;
- recovery may be delayed but must not duplicate authoritative effects.

## 27. Clock Advancement and Leases

If wall clock moves forward sharply:

- leases may expire early;
- another worker may reclaim;
- at-least-once execution and idempotent publication remain required.

## 28. Clock Diagnostic

Material clock regression or advancement MAY emit a safe diagnostic event.

It does not alter Campaign history directly.

## 29. UUID v7 Time

UUID v7 generation may use current UTC time.

The identifier's embedded time is:

- approximate;
- nonauthoritative;
- used for ordering characteristics;
- not a substitute for persisted timestamps.

## 30. UUID v7 Clock Regression

The identifier generator SHOULD preserve uniqueness and local nondecreasing generation behavior when the clock moves backward.

Exact behavior belongs to the generator implementation.

## 31. Domain Timestamps

A Domain transition that requires time SHOULD receive an Instant explicitly.

Example:

```text
session.Start(startedAtUtc)
memory.Archive(archivedAtUtc)
```

This makes the decision visible and testable.

## 32. Timestamp Authority

The Application layer generally supplies accepted timestamps using `IClock`.

Providers may propose narrative chronology but cannot set authoritative persistence timestamps unless a specific validated contract maps a fictional in-world time distinct from system time.

## 33. In-World Time

Campaign fictional time is separate from system UTC time.

A future Domain model may represent:

- fictional date;
- lunar phase;
- Scene-relative time;
- calendar system.

This ADR governs system execution time, not fictional chronology.

## 34. Authoritative Randomness

Chronicle owns all authoritative Dice randomness.

The provider may request a Roll.

Only Chronicle generates raw Dice values.

## 35. Authoritative Random Source

Chronicle will define:

```text
IAuthoritativeRandomSource
```

Conceptually:

```csharp
public interface IAuthoritativeRandomSource
{
    int NextInt32(int minInclusive, int maxExclusive);
    void Fill(Span<byte> destination);
}
```

The exact surface may be more Dice-specific.

## 36. Dice-Specific Abstraction

A preferred alternative is a narrow service:

```csharp
public interface IDiceValueGenerator
{
    IReadOnlyList<int> Roll(
        int dieCount,
        int sides);
}
```

The final implementation SHOULD keep cryptographic randomness hidden from Domain mechanics while making Dice generation explicit.

## 37. Production Randomness

Production authoritative Dice generation uses operating-system cryptographic randomness through an approved .NET API.

### Rationale

This avoids:

- predictable global pseudorandom state;
- seed reuse;
- process-start correlations;
- accidental shared state;
- low-quality ad hoc generators.

## 38. No Global Random Instance

Authoritative Dice MUST NOT use:

- static `Random`;
- shared `Random`;
- timestamp-seeded generators;
- provider randomness;
- UI animation state.

## 39. Uniform Range Mapping

Mapping random bytes into die values MUST avoid modulo bias.

Use an approved bounded integer API or rejection sampling.

## 40. Dice Value Validation

Generated raw values MUST satisfy:

```text
1 <= value <= sides
```

The Dice service validates count and die size limits before generation.

## 41. Dice Generation Inputs

Authoritative generation receives:

- Dice Roll identity;
- number of dice;
- die sides or Rule Set die type;
- OperationId;
- optional generation policy version.

The random source does not decide Rule Set success.

## 42. Separation of Generation and Resolution

The Dice subsystem separates:

```text
Generate raw values
    Chronicle-owned randomness

Resolve outcome
    deterministic Rule Set operation
```

## 43. Persist Before Continuation

Raw Dice values and mechanical resolution evidence MUST commit before any narration continuation is accepted.

## 44. Retry After Roll

A retry after persisted Roll:

- loads existing `DiceRollId`;
- loads raw values;
- loads accepted resolution;
- continues from them;
- never invokes the generator again.

## 45. Failure Before Roll Commit

If generation occurs but the transaction fails before persistence:

- no authoritative Roll exists;
- retry may generate new values using the same OperationId;
- the failed in-memory values are not Campaign truth.

This behavior must be explicit in the UI only if the user had already seen an animation or provisional result, which should be avoided before commit.

## 46. UI Timing of Result

The UI SHOULD reveal the authoritative Dice result only after persistence succeeds.

Animation may begin earlier only if it does not reveal a provisional value.

## 47. Animation Randomness

Dice animation may use a separate:

```text
INonAuthoritativeRandomSource
```

or framework-local animation randomness.

Its outputs:

- are not persisted;
- are not used for Rule Set resolution;
- may differ on replay;
- disappear under reduced-motion mode.

## 48. Animation Follows Result

When the animation settles, it displays the already committed authoritative values.

## 49. No Seed in Campaign State

The MVP does not persist one reusable global Dice seed for the Campaign.

### Rationale

A persisted seed could:

- make future outcomes predictable if exposed;
- require careful generator-state persistence;
- complicate concurrency;
- create replay semantics the product does not need.

Chronicle persists actual outcomes instead.

## 50. Per-Roll Random Evidence

The MVP SHOULD persist:

- raw values;
- die positions;
- die type;
- generation policy version;
- generation timestamp;
- OperationId.

It does not need to persist secret random seed material.

## 51. Verifiable Randomness

Cryptographically verifiable public randomness is outside MVP.

A later multiplayer or trustless mode may introduce commit-reveal or externally auditable schemes.

## 52. Test Random Source

Tests use a:

```text
ScriptedRandomSource
```

that returns predefined values.

Example:

```text
[10, 1, 6, 8]
```

## 53. Script Exhaustion

A scripted source SHOULD fail clearly when a test requests more values than provided.

It must not silently fall back to production randomness.

## 54. Seeded Test Source

Statistical, property, or fuzz tests MAY use a seeded deterministic pseudorandom source.

Every failing test MUST report its seed.

## 55. Test Repeatability

The same:

- fixture;
- seed or script;
- clock;
- command input;
- Rule Set version;

must produce the same deterministic mechanical result.

## 56. Statistical Tests

Statistical tests MAY validate broad distribution properties.

They MUST:

- use deterministic seeds;
- avoid fragile exact-frequency expectations;
- use tolerances;
- remain separate from correctness tests.

## 57. Rule Set Determinism

Rule Set resolution MUST be deterministic given:

- raw Dice values;
- actor snapshot;
- target snapshot;
- modifiers;
- Preferences;
- operation version.

Rule Set code MUST NOT call a random service.

## 58. Provider Nondeterminism

Narrative Intelligence remains inherently nondeterministic.

Chronicle controls it through:

- structured contracts;
- validation;
- OperationId;
- state versions;
- persisted accepted output.

This ADR does not attempt to make provider generation deterministic.

## 59. Provider Random Values

If provider output contains proposed Dice values:

- reject them;
- ignore them only when the schema explicitly treats them as prose and they cannot affect mechanics;
- record a safe validation failure when structured output violates the contract.

## 60. Random Campaign Generation

Future procedural Campaign generation MAY use nonauthoritative randomness.

If its output becomes accepted Campaign state:

- the accepted generated result is persisted;
- retry semantics must be explicit;
- randomness must be injected;
- the same OperationId must not create duplicate accepted plans.

## 61. Deterministic Import

Import, restore, and migration MUST NOT use randomness except to generate new IDs during explicit clone behavior.

Those generated IDs use the identifier generator, not the Dice source.

## 62. Security Tokens

Security-sensitive random values such as:

- temporary nonces;
- future signed cursor material;
- local IPC challenge values;

must use a dedicated cryptographic source.

They MUST NOT use Dice or UI randomness.

## 63. Randomness Families

Chronicle distinguishes:

```text
Authoritative Dice Randomness
Identifier Randomness
Security Randomness
Retry Jitter
Test Fixture Randomness
UI Animation Randomness
```

These should not share one untyped global interface without clear wrappers.

## 64. Implementation Wrappers

Concrete Infrastructure may use one operating-system cryptographic primitive underneath.

Public ports remain purpose-specific.

## 65. Random Source Lifetime

Production random-source services MAY be singleton when the underlying API is thread-safe and stateless.

Stateful deterministic test sources should be scoped intentionally.

## 66. Concurrency

Authoritative Dice generation must be thread-safe.

Concurrent Rolls must not:

- share mutable buffer state;
- produce correlated sequences due to reused seeds;
- reorder die positions;
- write to the wrong Roll.

## 67. Die Position

Each raw value is associated with a stable position:

```text
0..N-1
```

or:

```text
1..N
```

The chosen convention must be consistent in storage and Rule Set contracts.

## 68. Generation Limits

The Dice service MUST enforce bounded:

- die count;
- sides;
- total random bytes;
- operation frequency where abuse protection is needed.

Limits may be Rule Set-specific within global safe maxima.

## 69. Invalid Dice Request

Invalid requests return typed validation errors.

Examples:

```text
dice.invalid-count
dice.invalid-sides
dice.request-too-large
dice.unsupported-die-type
```

## 70. Dice Generation Failure

An operating-system random-source failure is an unexpected Infrastructure failure.

No partial authoritative Roll is committed.

## 71. Randomness Logging

Logs MAY include:

- DiceRollId;
- die count;
- die type;
- generation policy version;
- duration;
- success or failure.

Default operational logs SHOULD NOT include raw Dice values unless specifically classified as safe and useful.

The authoritative values remain queryable from Campaign history.

## 72. Randomness Diagnostics

Developer diagnostics MAY display raw values through the normal Dice read model.

They must not display secret seed material because none is persisted.

## 73. Time Logging

Logs use UTC timestamps.

Durations use monotonic measurements where possible.

## 74. Clock in Logs

The logging provider may timestamp events independently.

Application events that require accepted business timestamps use `IClock`.

## 75. Timeout Model

Timeouts SHOULD be represented as durations.

Deadline Instants MAY be computed for persisted or cross-stage workflows.

## 76. Provider Timeout

Provider timeout uses:

- monotonic in-process measurement;
- cancellation;
- configured duration;
- safe error mapping.

## 77. Database Timeout

Database busy and command timeout behavior remains Infrastructure-specific.

The Application receives typed timeout or unavailable results.

## 78. User-Visible Time

The UI formats persisted UTC Instants using current local preferences.

Changing timezone does not rewrite Campaign records.

## 79. Relative Time

Relative labels such as:

```text
"3 minutes ago"
```

are Presentation state and refresh using a UI timer or clock service.

They do not affect Domain state.

## 80. Timer Boundary

UI timers must not trigger authoritative transitions.

Background scheduling uses durable Work Items and persisted due Instants.

## 81. Session Memory Aging

Memory aging occurs once per finalized Session.

It is driven by accepted finalization, not by wall-clock passage.

## 82. Expiration Semantics

Temporary Memory lifetime measured in Sessions does not depend on real-world time unless a future rule explicitly defines that behavior.

## 83. Operation Age

Operation status may display age using persisted timestamps.

Age does not alone determine whether replay is safe; Operation Record status remains authoritative.

## 84. Backup Timestamps

Backup manifests record UTC creation and validation Instants.

File modification time is not authoritative backup identity.

## 85. Migration Timestamps

Migration records use injected UTC time.

Tests control those values.

## 86. Test Infrastructure

`Chronicle.Testing` SHOULD provide:

```text
FakeClock
FakeMonotonicClock
ImmediateDelayScheduler
ControllableDelayScheduler
ScriptedRandomSource
SeededRandomSource
DeterministicDiceValueGenerator
```

## 87. Immediate Delay Scheduler

Most unit tests use an immediate scheduler that completes delays without waiting.

It should still record requested delays for assertions.

## 88. Controllable Scheduler

Recovery tests may use a scheduler that pauses until fake time is advanced or the test releases it.

## 89. Clock and Randomness Fixture

End-to-end tests SHOULD construct one explicit execution fixture containing:

- clock;
- monotonic clock;
- ID generator;
- Dice source;
- delay scheduler;
- provider script.

## 90. No Hidden Test Defaults

Tests should not accidentally fall back to production clock or randomness.

Test composition should fail fast when deterministic dependencies are missing.

## 91. Architecture Tests

Architecture tests MUST scan for prohibited APIs in Domain and Application assemblies.

Examples:

```text
DateTime.Now
DateTime.UtcNow
DateTimeOffset.Now
DateTimeOffset.UtcNow
new Random()
Random.Shared
Task.Delay
Thread.Sleep
```

Approved exceptions require documented suppression.

## 92. Analyzer

Chronicle SHOULD add or configure analyzers that flag direct time and randomness APIs in restricted projects.

## 93. Suppression Policy

A suppression must:

- be local;
- explain the reason;
- identify the approved boundary;
- be reviewed.

## 94. Testing Strategy

The implementation requires:

```text
Unit Tests
Property Tests
Concurrency Tests
SQLite Integration Tests
Recovery Tests
Security Tests
Architecture Tests
Performance Tests
```

## 95. Clock Unit Tests

Tests MUST cover:

- fixed UTC time;
- advance;
- regression;
- concurrent reads;
- boundary Instants;
- serialization;
- local display conversion outside Domain.

## 96. Monotonic Clock Tests

Tests MUST cover:

- elapsed duration;
- increasing timestamps;
- independent wall-clock change;
- timeout calculation.

## 97. Delay Tests

Tests MUST cover:

- immediate completion;
- cancellation;
- requested duration capture;
- backoff sequence;
- retry-after override;
- no real sleeping in unit tests.

## 98. Dice Unit Tests

Tests MUST cover:

- valid range;
- die count;
- stable positions;
- zero dice rejection where unsupported;
- invalid sides;
- scripted values;
- script exhaustion;
- Rule Set receives exact raw values.

## 99. Distribution Tests

Production-source statistical tests SHOULD validate absence of obvious severe bias.

They do not attempt to certify cryptographic quality beyond using the approved operating-system primitive.

## 100. Recovery Tests

Tests MUST cover:

- crash before Roll commit;
- crash after Roll commit;
- retry after Roll commit;
- provider continuation repeated;
- no reroll;
- finalization retry with same accepted Roll evidence.

## 101. Clock Regression Tests

Tests MUST cover:

- UUID v7 generation;
- active lease;
- expired lease;
- retry schedule;
- operation timestamps;
- no duplicate authoritative effects.

## 102. Concurrency Tests

Tests SHOULD execute many simultaneous Roll generations and assert:

- correct count;
- valid ranges;
- no shared state corruption;
- stable die positions;
- correct Roll ownership.

## 103. Security Tests

Tests MUST prove:

- no timestamp-seeded Dice;
- no provider-generated values accepted;
- no seed in logs;
- no seed in SQLite;
- no global deterministic production generator;
- UI animation cannot alter persisted result.

## 104. Required Test Cases

Tests MUST cover:

- production UTC clock;
- fake clock;
- monotonic duration;
- delay cancellation;
- persisted retry due time;
- lease reclaim after fake-time advance;
- clock moves backward;
- clock moves forward;
- UUID v7 generation;
- one d10;
- multiple d10;
- maximum supported pool;
- invalid request;
- transaction failure before Roll commit;
- successful Roll commit;
- duplicate click;
- restart continuation;
- scripted Rule Set resolution;
- animation with different random values;
- reduced-motion immediate result;
- provider output containing fake Dice values;
- architecture scan for ambient APIs.

## 105. Prohibited Patterns

### 105.1 `DateTime.UtcNow` in Domain Logic

Use injected time.

### 105.2 Real Sleep in Unit Tests

Use a controllable scheduler.

### 105.3 Static `Random` for Dice

Use the authoritative random source.

### 105.4 Timestamp Seed

Predictable and unsuitable.

### 105.5 Reroll on Recovery

Load persisted raw values.

### 105.6 UI Animation Produces Outcome

Animation follows committed state.

### 105.7 Rule Set Calls Randomness

Resolution is deterministic.

### 105.8 Provider Supplies Dice

Chronicle owns raw values.

### 105.9 One Global Random Interface for Every Purpose

Use purpose-specific wrappers.

### 105.10 Wall Clock for In-Process Duration

Use monotonic timing.

## 106. Alternatives Considered

### Direct Framework Time APIs

Rejected because tests and recovery behavior would depend on ambient state.

### `TimeProvider` Used Directly Everywhere

The .NET time abstraction may be used inside Chronicle adapters.

Chronicle still prefers its own narrow ports so Domain and Application contracts remain stable and purpose-specific.

### Seeded Pseudorandom Dice

Rejected as the production default because seed management and predictability provide no MVP benefit.

### Persist One Campaign Seed

Rejected because Chronicle needs durable outcomes rather than deterministic regeneration of future chance.

### Provider-Generated Dice

Rejected because the provider is advisory and cannot own mechanics.

### Hardware or Public Random Beacon

Unnecessary for the local single-player MVP.

## 107. Consequences

### Positive

- deterministic tests;
- explicit time semantics;
- stable retry and lease testing;
- secure authoritative Dice;
- no reroll during recovery;
- clean Rule Set determinism;
- easier failure injection;
- clear separation between UI effects and Campaign truth.

### Negative

- additional abstractions;
- analyzer and architecture-test maintenance;
- production randomness is not reproducible from a seed;
- clock-regression behavior requires careful testing;
- purpose-specific random wrappers add some code.

## 108. Risks

### Direct API Leakage

Mitigation:

- analyzers;
- architecture tests;
- reviewed suppressions.

### Biased Dice Mapping

Mitigation:

- approved bounded integer API;
- range and distribution tests;
- no custom modulo mapping.

### Reroll Through Retry Bug

Mitigation:

- unique OperationId;
- persisted DiceRollId;
- recovery tests;
- Dice generation only in one command stage.

### Clock Change Breaks Recovery

Mitigation:

- idempotency;
- entity versions;
- lease owner checks;
- fake-clock regression tests.

### Overgeneralized Time Abstraction

Mitigation:

- small ports;
- explicit Domain Instants;
- separate monotonic and wall-clock concerns.

## 109. Technology Spike

Before acceptance, implement:

1. `IClock`;
2. `SystemClock`;
3. `FakeClock`;
4. monotonic clock abstraction;
5. delay scheduler;
6. deterministic backoff policy;
7. authoritative Dice generator;
8. scripted Dice source;
9. UUID v7 generator integration;
10. lease expiry under fake time;
11. retry scheduling;
12. Dice persistence;
13. crash and replay flow;
14. UI animation separation;
15. prohibited-API architecture test.

## 110. Spike Acceptance

The spike passes when:

- unit tests use no real waiting;
- all accepted persisted Instants are UTC;
- operation durations use monotonic time;
- Work Item retry and lease tests advance fake time;
- authoritative Dice use operating-system cryptographic randomness;
- raw Dice values commit before display and continuation;
- retry after commit never calls the Dice generator;
- provider values cannot become authoritative;
- UI animation cannot change persisted results;
- Domain and Application assemblies pass ambient API scans.

## 111. Definition of Compliance

An implementation complies when:

- current time is injected;
- persisted time is UTC;
- durations use monotonic measurement where applicable;
- delays are abstracted in testable workflows;
- authoritative Dice randomness is Chronicle-owned;
- production Dice use approved operating-system cryptographic randomness;
- Rule Set resolution is deterministic;
- raw values are persisted before continuation;
- retries reuse persisted Rolls;
- UI animation uses nonauthoritative randomness;
- tests use fake time and scripted randomness;
- architecture tests reject ambient dependencies.

## 112. Review Triggers

This ADR must be reviewed if:

- multiplayer requires verifiable randomness;
- public Roll auditing is introduced;
- deterministic Campaign replay becomes a product requirement;
- a server process executes Rolls;
- recurring calendar scheduling becomes core;
- fictional calendar mechanics depend on real-world time;
- `TimeProvider` or runtime UUID support changes implementation value;
- cryptographic requirements change;
- external synchronization introduces clock-skew coordination.

## 113. Deferred Decisions

Later ADRs MAY define:

- exact .NET cryptographic API;
- exact Dice generator interface;
- verifiable multiplayer randomness;
- commit-reveal protocol;
- fictional calendar system;
- time-zone preference model;
- deterministic full Campaign replay;
- exact clock-regression thresholds;
- long-running scheduler implementation;
- performance benchmark timer implementation.

## 114. Final Decision

Chronicle will make time and randomness explicit dependencies.

Persisted Instants will use UTC.

Elapsed durations will use monotonic time.

Authoritative Dice will be generated by Chronicle from an approved operating-system cryptographic source, persisted before continuation, and never regenerated during retry.

The clock may move.

The process may stop.

The animation may change.

The Roll that entered the Campaign must not.
