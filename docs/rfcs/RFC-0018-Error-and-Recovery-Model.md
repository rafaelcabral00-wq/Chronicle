---
id: RFC-0018
title: Error and Recovery Model
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-07-31
category: Application
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
---

> **"Failure is not corruption. Failure becomes corruption only when Chronicle hides it, repeats it, or forgets where truth stopped."**

# Error and Recovery Model

## Abstract

This RFC defines Chronicle's error classification, recovery behavior, retry semantics, interruption model, checkpoint use, stale operation handling, corruption boundaries, and user-visible failure states.

The objective is to ensure that failures remain explicit, recoverable where possible, and incapable of silently damaging Campaign truth.

Chronicle MUST prefer a visible interrupted workflow over a convincing but false success.

## 1. Purpose

Chronicle coordinates:

- persistent Campaign state;
- external Narrative Intelligence;
- Rule Set logic;
- long-running workflows;
- random operations;
- local storage;
- structured contracts;
- application restarts;
- retried commands.

Failures may occur at any stage.

The system MUST know:

- what failed;
- whether anything committed;
- whether retry is safe;
- whether user action is required;
- which state remains authoritative;
- whether external output is stale;
- whether a workflow can resume;
- whether a Roll or progression result must be reused.

## 2. Scope

This RFC defines:

- error taxonomy;
- error structure;
- severity;
- retryability;
- recoverability;
- failure stages;
- workflow interruption;
- checkpoints;
- recovery status;
- external dependency failure;
- persistence failure;
- contract failure;
- concurrency conflict;
- idempotency conflict;
- stale response handling;
- Dice Roll recovery;
- Session recovery;
- finalization recovery;
- Campaign generation recovery;
- storage corruption behavior;
- user-facing error boundaries;
- observability;
- testing.

This RFC does not define:

- exact exception classes;
- exact UI designs;
- remote incident response;
- cloud disaster recovery;
- support ticket workflows;
- complete backup implementation;
- operating-system-specific crash handling.

## 3. Core Recovery Principle

Chronicle MUST always be able to identify the last authoritative state.

The system follows:

```text
Last Consistent Checkpoint
        +
Operation Record
        +
Committed Domain State
        =
Recovery Basis
```

Unvalidated provider output MUST NOT be considered part of recovery truth.

## 4. Error Versus Failure

An `Error` is a classified problem detected by Chronicle.

A `Failure` is an operation that did not reach its intended successful outcome.

Not every error is fatal.

Examples:

```text
Validation warning
    = error information without operation failure

Concurrency conflict
    = failed mutation with intact Campaign state

Storage corruption
    = potentially critical failure requiring restricted recovery
```

## 5. Error Structure

An application error SHOULD contain:

- error code;
- category;
- severity;
- message safe for the user;
- internal diagnostic message;
- retryability;
- recoverability;
- operation identifier;
- correlation identifier;
- Campaign identifier when applicable;
- entity references when safe;
- failure stage;
- timestamp;
- causal error reference;
- remediation options;
- redaction status.

## 6. Error Categories

Canonical top-level categories are:

```text
ValidationError
AuthorizationError
NotFoundError
ConflictError
InvalidStateError
IdempotencyConflict
ContractError
ExternalDependencyError
RuleSetError
PersistenceError
MigrationError
DataIntegrityError
CorruptionError
RecoveryRequiredError
CancelledOperation
UnexpectedError
```

## 7. Validation Error

A `ValidationError` means input, structure, or a proposed change is invalid.

Examples:

- invalid Character field;
- unsupported modifier;
- missing required Campaign preference;
- malformed structured response;
- invalid Scene participant reference.

Validation errors SHOULD be nonretryable until input changes.

They MUST NOT alter authoritative state.

## 8. Authorization Error

An `AuthorizationError` means the actor cannot perform the requested operation.

The local single-user MVP may rarely produce this category.

The category SHOULD exist for future boundaries such as:

- Campaign ownership;
- hidden-information access;
- destructive commands;
- multiplayer permissions.

Authorization failures MUST NOT reveal protected information.

## 9. Not Found Error

A `NotFoundError` means the requested entity does not exist or is not visible to the actor.

Chronicle MAY intentionally return Not Found instead of revealing that a hidden entity exists.

## 10. Conflict Error

A `ConflictError` means the request conflicts with current authoritative state.

Examples:

- stale version;
- active Session already exists;
- Scene already transitioned;
- Memory changed concurrently;
- Rule Set version no longer matches.

The caller SHOULD refresh state before retrying.

## 11. Invalid State Error

An `InvalidStateError` means the requested transition is not permitted by the entity lifecycle.

Examples:

- finalizing a completed Session;
- processing input while awaiting a Roll;
- executing a cancelled Roll;
- editing a Character during restricted active play.

## 12. Idempotency Conflict

An `IdempotencyConflict` occurs when the same OperationId is reused with a different semantic request.

Chronicle MUST reject the request.

It MUST NOT choose one payload silently.

## 13. Contract Error

A `ContractError` means structured communication failed validation.

Examples:

- unknown contract version;
- missing required field;
- invalid enum;
- provider returned prose where a structured event was required;
- reference to nonexistent Character.

A Contract Error MAY trigger repair or regeneration.

## 14. External Dependency Error

An `ExternalDependencyError` originates outside Chronicle Core.

Examples:

- Narrator unavailable;
- Archivist timeout;
- provider rate limit;
- rule knowledge retrieval failure;
- local model process unavailable.

The error MUST identify whether retry is safe.

## 15. Rule Set Error

A `RuleSetError` means a required system-specific operation failed.

Examples:

- unsupported operation key;
- Character cannot be validated;
- dice resolution failed;
- migration required;
- exact Rule Set version unavailable.

The Narrator MUST NOT compensate by inventing mechanics.

## 16. Persistence Error

A `PersistenceError` means a storage operation failed without confirmed corruption.

Examples:

- transaction commit failed;
- store unavailable;
- file locked;
- disk full;
- write permission denied.

Chronicle MUST NOT report success without confirmed commit.

## 17. Migration Error

A `MigrationError` means persistent data could not be upgraded safely.

The application SHOULD restrict normal Campaign play until recovery or rollback is complete.

## 18. Data Integrity Error

A `DataIntegrityError` means persisted relationships or invariants are inconsistent.

Examples:

- active Scene references a missing Act;
- Relationship points across Campaigns;
- two Player Characters exist in one MVP Campaign;
- finalization applied but Session not completed.

Chronicle SHOULD block unsafe mutations until integrity is restored.

## 19. Corruption Error

A `CorruptionError` means stored data cannot be trusted or interpreted safely.

Examples:

- unreadable database;
- invalid checksum;
- truncated Campaign package;
- broken schema beyond known migration recovery.

Corruption is critical.

Chronicle MUST avoid automatic destructive repair without an explicit policy.

## 20. Recovery Required Error

A `RecoveryRequiredError` means the operation cannot continue normally but Chronicle has enough information to offer a bounded recovery action.

Examples:

- narration committed but UI response lost;
- resolved Roll waiting for continuation;
- finalization stopped after proposal validation;
- interrupted Scene transition.

## 21. Cancelled Operation

A `CancelledOperation` means a workflow was intentionally stopped before accepted changes were committed.

Cancellation is not necessarily a failure.

The operation status and reason SHOULD remain persisted.

## 22. Unexpected Error

An `UnexpectedError` represents an unclassified defect or exceptional runtime failure.

Chronicle SHOULD:

- preserve prior state;
- generate a diagnostic identifier;
- avoid exposing internal details;
- mark recovery status;
- fail closed where state safety is uncertain.

Unexpected errors SHOULD be reduced over time into explicit categories.

## 23. Severity Levels

Canonical severity levels are:

```text
Info
Warning
Error
Critical
```

### Info

Operational information requiring no intervention.

### Warning

Operation may continue with reduced behavior or user awareness.

### Error

Operation failed, but Campaign state remains known.

### Critical

Campaign integrity, storage, or safe continuation is at risk.

## 24. Retryability

Canonical retry classifications are:

```text
NotRetryable
SafeImmediately
SafeWithSameOperationId
SafeAfterRefresh
RequiresRegeneration
RequiresUserDecision
RequiresRepair
```

## 25. Safe Immediately

Used for transient failures before any meaningful external or persistent side effect.

Example:

- temporary rule knowledge service timeout before request acceptance.

## 26. Safe With Same OperationId

Used when retries must preserve idempotency.

Examples:

- provider timeout;
- failure after commit;
- finalization resume;
- Campaign generation resume;
- post-roll continuation.

## 27. Safe After Refresh

Used when current state must be reloaded.

Examples:

- optimistic concurrency conflict;
- stale Scene version;
- Campaign status changed.

## 28. Requires Regeneration

Used when external output is no longer valid.

Examples:

- stale Narrator response;
- plan proposal based on changed Character;
- Archivist proposal based on changed Session evidence.

## 29. Requires User Decision

Used when more than one safe recovery path exists.

Examples:

- resume interrupted Session;
- cancel pending generation;
- archive incompatible Campaign;
- choose backup restoration.

## 30. Requires Repair

Used when data or configuration must be corrected.

Examples:

- missing Rule Set;
- failed migration;
- invalid Campaign invariant;
- corrupted local store.

## 31. Recoverability

Canonical recoverability levels are:

```text
Automatic
Guided
Manual
Unavailable
```

### Automatic

Chronicle can recover without user action.

### Guided

Chronicle presents one or more bounded actions.

### Manual

Requires file, configuration, migration, or administrative intervention.

### Unavailable

No safe recovery path is known.

## 32. Failure Stages

Long-running operations SHOULD record the stage where failure occurred.

Common stages:

```text
BeforeLoad
Loading
PreconditionValidation
OperationRegistered
ContextAssembly
ExternalCall
ResponseValidation
ReadyToCommit
Committing
AfterCommit
RenderingResult
```

The same error category may require different recovery depending on stage.

## 33. Before Commit Versus After Commit

This distinction is fundamental.

### Before Commit

Accepted domain changes are not authoritative.

Retry or cancellation MAY be safe.

### After Commit

The state change is already authoritative.

Retry MUST return or continue from the committed result.

Chronicle MUST NOT repeat the mutation.

## 34. Last Consistent Checkpoint

A checkpoint is a known internally consistent Campaign version.

Recommended checkpoints include:

- Campaign creation accepted;
- Session started;
- narration turn committed;
- Dice Roll request committed;
- Dice Roll resolved;
- post-roll continuation committed;
- Scene transitioned;
- Session finalized.

Recovery begins from the latest confirmed checkpoint.

## 35. Recovery Status

Chronicle SHOULD expose a structured `RecoveryStatus`.

It MAY contain:

- Campaign identifier;
- affected operation;
- last consistent checkpoint;
- current workflow stage;
- pending user action;
- retry classification;
- recoverability;
- safe actions;
- hidden diagnostic identifier;
- whether normal play is blocked.

## 36. Recovery Actions

Canonical actions MAY include:

```text
Retry
Resume
RefreshState
Regenerate
CancelOperation
ReturnToCheckpoint
ResolvePendingRoll
ContinueAfterRoll
ResumeFinalization
ConfigureRuleSet
RestoreBackup
ArchiveCampaign
ExportForRepair
```

Only valid actions for the current state SHOULD be exposed.

## 37. Recoverable Operation Record

Long-running operations SHOULD persist:

- OperationId;
- command type;
- request fingerprint;
- current stage;
- external request identifier;
- retained structured response;
- validation result;
- expected versions;
- last successful stage;
- retry count;
- failure category;
- recovery actions;
- timestamps.

## 38. Application Restart Recovery

On application startup, Chronicle SHOULD inspect:

- active Campaigns;
- interrupted Sessions;
- pending Rolls;
- pending continuations;
- incomplete finalizations;
- Campaign generation workflows;
- failed migrations;
- unresolved operation records.

The application SHOULD present recovery state rather than pretending every Campaign is idle.

## 39. Session Recovery

An interrupted Session MUST restore one of:

```text
ReadyForInput
AwaitingRoll
AwaitingContinuation
RecoveringTransition
Finalizing
BlockedByIntegrityError
```

The restore mode MUST come from persisted state.

It MUST NOT be inferred from provider memory.

## 40. Narration Failure Before Commit

If Narrator invocation fails before accepted output:

- the player Message MAY remain pending operational input;
- no Narrator Message is committed;
- no state transition occurs;
- retry uses the same OperationId;
- current Scene remains authoritative.

The product decision about displaying pending player input remains open.

## 41. Narration Failure After Commit

If narration and events committed but the UI did not receive confirmation:

- retry returns the committed result;
- Messages are not duplicated;
- state transitions are not repeated;
- OperationId identifies the result.

## 42. Invalid Narrator Response

If the Narrator returns an invalid contract:

Chronicle MAY:

- perform deterministic repair;
- request structured repair;
- regenerate;
- reject and return a recoverable error.

Chronicle MUST NOT parse arbitrary prose into persistent mutations without a validated contract.

## 43. Stale Narrator Response

If the Scene or Campaign changed after request creation:

- the response is marked stale;
- it is not persisted as accepted narrative;
- retry classification is `RequiresRegeneration`;
- stale output MAY be retained only for diagnostics.

## 44. Rule Knowledge Failure

If rule knowledge retrieval fails:

### Deterministic mechanics available locally

Chronicle MAY continue mechanics and fail only narrative operations requiring missing context.

### Required rule information unavailable

Chronicle MUST stop the affected operation.

The Narrator MUST NOT invent rules.

## 45. Provider Rate Limit

A provider rate limit SHOULD be classified as retryable.

Chronicle SHOULD preserve:

- operation state;
- context version;
- request fingerprint;
- retry guidance.

If the delay makes context stale, regeneration is required after refresh.

## 46. Provider Authentication Failure

Authentication or credential failure requires configuration repair.

Automatic repeated retries SHOULD be avoided.

Secrets MUST NOT appear in user-facing errors or logs.

## 47. Campaign Generation Recovery

Campaign generation may fail during:

- context preparation;
- provider generation;
- contract validation;
- NPC validation;
- plan validation;
- persistence.

Before acceptance, the Campaign remains `Draft`.

Recovery MAY:

- retry generation;
- repair invalid sections;
- cancel generation;
- edit Character or preferences;
- regenerate from current state.

Partial generated entities MUST NOT become playable truth.

## 48. Dice Roll Recovery Principles

Dice recovery MUST distinguish:

```text
No random values accepted
Random values staged
Result committed
Continuation pending
```

Each stage has different rules.

## 49. Roll Failure Before Randomness

The Roll remains `Presented` or `Failed`.

Retry with the same OperationId is safe.

## 50. Roll Failure After Randomness Is Staged

If raw values are durably staged:

- retry MUST reuse them;
- the Rule Set may resolve them again deterministically;
- no new values may be generated.

The implementation SHOULD avoid exposing a Roll result before durable staging or commit.

## 51. Roll Failure After Result Commit

The Roll remains `Resolved`.

Retry returns the same Roll Result.

The only remaining failure may be narrative continuation.

## 52. Continuation Failure

If post-roll Narrator continuation fails:

- the Roll remains resolved;
- consequences already committed remain authoritative;
- the Scene enters a recoverable continuation state;
- no reroll occurs;
- retry uses a continuation OperationId.

## 53. Pending Roll on Restart

The application MUST restore:

- Roll reason;
- acting Character;
- validated pool details;
- status;
- Roll button state;
- active Scene.

The player MUST not receive a new Roll Request for the same interruption.

## 54. Session Finalization Recovery

Finalization recovery depends on stage.

### Before Archivist Proposal

Retry or cancel MAY be safe.

### Proposal Received

The proposal may be retained and revalidated.

### Change Set Ready

The Change Set may be applied if versions remain current.

### Commit Failed

Transaction rollback preserves prior state.

### Commit Succeeded

Retry returns finalization result.

## 55. Finalization Fallback

When the Archivist remains unavailable, Chronicle MAY offer a deterministic fallback if approved by product policy.

The fallback MUST preserve:

- Session transcript;
- resolved Rolls;
- immediate changes;
- deterministic Memory aging;
- operation trace.

It MUST NOT fabricate semantic Memories or progression.

## 56. Memory Aging Recovery

Memory aging MUST record the Session finalization that applied it.

A retry checks that marker.

Aging MUST NOT run twice.

## 57. Progression Recovery

Progression MUST be traceable to one accepted finalization operation.

A repeated workflow returns the existing award.

It MUST NOT recalculate and award again unless a correction workflow is explicitly invoked.

## 58. Scene Transition Recovery

A Scene transition SHOULD commit:

- old Scene completion or interruption;
- participant exits;
- next Scene creation or activation;
- Campaign State update.

The transition MUST be atomic.

On failure, the prior active hierarchy remains authoritative.

## 59. Plan Revision Recovery

If plan revision fails:

- current executed Campaign state remains valid;
- old Plan version remains authoritative;
- Chronicle may continue only if a valid Scene exists;
- otherwise normal narrative play is blocked with `PlanRevisionRequired`.

## 60. Persistence Failure

When storage is unavailable or a commit fails:

- Chronicle MUST not report success;
- uncommitted output MUST not become truth;
- operation state SHOULD record failure where safely possible;
- user should receive recovery guidance;
- repeated writes SHOULD avoid uncontrolled retry loops.

## 61. Disk Full

Disk-full behavior SHOULD be treated as a critical Persistence Error.

Chronicle SHOULD:

- stop accepting state-changing commands;
- preserve in-memory data only as nonauthoritative temporary state;
- instruct the user to free storage;
- retry only after confirmation;
- avoid creating further large logs.

## 62. Read-Only Recovery Mode

Chronicle SHOULD support a conceptual read-only recovery mode when mutation safety is uncertain.

In read-only mode, the user MAY:

- inspect Campaigns;
- view history;
- export data when safe;
- inspect diagnostics;
- configure recovery.

The user MUST NOT advance play.

## 63. Data Integrity Validation

Chronicle SHOULD be able to validate:

- Campaign ownership;
- active hierarchy;
- one Player Character invariant;
- Scene participant references;
- pending Roll references;
- finalization uniqueness;
- Rule Set availability;
- version consistency;
- operation result references.

Integrity checks MAY run:

- on startup;
- before sensitive workflows;
- after migration;
- during explicit diagnostics.

## 64. Integrity Repair

Automatic repair is allowed only when deterministic and lossless.

Examples:

- rebuilding a derived read model;
- restoring a missing cache;
- recalculating a derived field.

Automatic repair MUST NOT:

- invent missing Campaign truth;
- rewrite resolved Rolls;
- choose between conflicting histories;
- discard unknown fields.

## 65. Corruption Handling

On detected corruption, Chronicle SHOULD:

1. stop unsafe writes;
2. preserve the original data;
3. record diagnostics;
4. attempt read-only access;
5. offer backup restoration or export;
6. avoid destructive automatic repair;
7. clearly communicate uncertainty.

## 66. Backup Recovery Boundary

Backup and restore details are defined later.

This RFC requires that recovery architecture not prevent:

- checkpoint backup;
- full Campaign export;
- store backup;
- restore validation;
- migration-safe restoration.

## 67. Rule Set Unavailable

If the Campaign's exact Rule Set version is unavailable:

- normal play is blocked;
- safe historical views MAY remain available;
- the application offers configuration, installation, migration, or archive options;
- Chronicle MUST NOT substitute another version silently.

## 68. Migration Failure Recovery

Before migration, Chronicle SHOULD create a backup or checkpoint.

On failure:

- original data remains preserved;
- Campaign remains on old schema where possible;
- normal play may be blocked;
- migration diagnostics are recorded;
- retry or rollback is explicit.

## 69. Cancellation Semantics

Cancellation is allowed only before accepted changes commit.

Cancellation MUST NOT undo:

- resolved Dice Rolls;
- committed narration;
- applied finalization;
- completed history.

Undoing committed truth requires a separate correction or restoration workflow.

## 70. Retry Limits

Chronicle SHOULD bound automatic retries.

Retry policy MAY consider:

- error category;
- external provider guidance;
- elapsed time;
- operation stage;
- risk of staleness;
- user experience.

Infinite retry loops are forbidden.

## 71. Backoff

Transient external retries SHOULD use bounded backoff.

The exact algorithm is an infrastructure decision.

A delayed retry MUST revalidate context freshness.

## 72. Circuit Breaking Horizon

Circuit breakers MAY be introduced if repeated provider or infrastructure failures justify them.

They are not mandatory for the local MVP.

The system SHOULD still avoid repeated failing calls after clear authentication or configuration errors.

## 73. User-Facing Error Principles

User-facing errors SHOULD answer:

- What failed?
- Was anything saved?
- Is the Campaign safe?
- Can I retry?
- Must I do something first?
- Will retry duplicate anything?

They SHOULD avoid technical jargon unless the user opens diagnostics.

## 74. Error Message Example

Good:

```text
The dice result was saved, but the Narrator could not continue the Scene.
Retrying will use the same result and will not roll again.
```

Avoid:

```text
Something went wrong.
```

## 75. Hidden Information Safety

Errors MUST NOT reveal:

- hidden NPC names;
- Secret content;
- future Plan details;
- private Relationships;
- unknown Character Knowledge;
- proprietary rule content;
- credentials.

## 76. Diagnostic Identifier

Unexpected or critical errors SHOULD expose a diagnostic identifier.

The identifier allows logs to be correlated without exposing sensitive details.

## 77. Observability

Chronicle SHOULD record:

- error category;
- severity;
- failure stage;
- OperationId;
- retry classification;
- recovery action;
- prior checkpoint;
- commit status;
- provider status;
- persistence status;
- Campaign version;
- redaction status.

## 78. Error Redaction

Diagnostic logging SHOULD separate:

```text
Safe Operational Metadata
Sensitive Payload
```

Sensitive payload logging MUST be opt-in and protected.

## 79. Metrics

Useful metrics MAY include:

- provider failure rate;
- contract repair rate;
- stale response rate;
- persistence failure rate;
- concurrency conflict rate;
- recovery success rate;
- repeated retry count;
- finalization recovery count;
- pending Roll recovery count.

Metrics are operational signals.

They are not Campaign truth.

## 80. Recovery Use Cases

The Application layer SHOULD expose bounded use cases such as:

```text
GetRecoveryStatus
RetryOperation
CancelRecoverableOperation
ResumeSession
ContinueAfterDiceRoll
ResumeFinalization
RestoreLastCheckpoint
ValidateCampaignIntegrity
```

## 81. Automatic Startup Recovery

On startup, Chronicle MAY automatically:

- mark abandoned in-progress provider calls as recoverable;
- restore pending Roll views;
- return committed command results;
- rebuild disposable read models;
- detect incomplete finalization;
- validate active hierarchy.

It MUST NOT automatically reroll or invent missing semantic state.

## 82. Testing Strategy

### 82.1 Unit Tests

Test:

- category mapping;
- retry classification;
- recovery action selection;
- state-machine transitions;
- hidden-information redaction.

### 82.2 Application Tests

Test:

- retry with same OperationId;
- failure before commit;
- failure after commit;
- cancellation;
- stale-response regeneration;
- read-only recovery mode.

### 82.3 Integration Tests

Test:

- disk unavailable;
- transaction failure;
- provider timeout;
- application restart;
- migration failure;
- corrupted fixture;
- Rule Set missing.

## 83. Required Test Cases

Tests MUST cover:

- Narrator timeout before commit;
- narration commit followed by UI failure;
- invalid Narrator contract;
- stale Narrator response;
- provider rate limit;
- provider authentication failure;
- pending Roll restart;
- double-click Roll;
- Roll committed and continuation failed;
- finalization provider timeout;
- finalization commit retry;
- Memory aging once;
- Campaign generation partial failure;
- Scene transition rollback;
- stale command conflict;
- conflicting OperationId reuse;
- disk full;
- missing Rule Set;
- migration failure;
- integrity-check failure;
- hidden information in errors;
- read-only recovery behavior.

## 84. Prohibited Patterns

### 84.1 False Success

Chronicle MUST NOT report success before confirmed commit.

### 84.2 Generic Unknown Error for Known Cases

Known failures SHOULD use explicit categories.

### 84.3 Retry with New OperationId

The same logical retry MUST reuse its OperationId.

### 84.4 Reroll During Recovery

A persisted or staged Roll result MUST NOT be replaced during retry.

### 84.5 Provider Memory as Recovery

Chronicle MUST NOT ask the provider to reconstruct authoritative state.

### 84.6 Silent Automatic Repair

Chronicle MUST NOT invent or discard Campaign truth during repair.

### 84.7 Infinite Retry

Automatic retries MUST be bounded.

### 84.8 Hidden Information in Diagnostics

Player-visible errors and ordinary logs MUST NOT reveal protected data.

### 84.9 Continue on Unknown Integrity

Chronicle MUST NOT advance play when Campaign integrity is uncertain.

## 85. Current Delivery Decision

The MVP adopts:

- explicit error categories;
- structured retryability and recoverability;
- persistent workflow stages;
- last-consistent-checkpoint recovery;
- startup detection of interrupted workflows;
- resumable Sessions;
- resumable pending Rolls;
- resumable finalization;
- idempotent retry;
- read-only recovery mode as an architectural capability;
- no silent provider reconstruction;
- no automatic destructive repair;
- no unbounded retries;
- no cloud disaster recovery;
- no generic distributed resilience framework.

## 86. Architecture Horizon

Future evolution MAY include:

- remote incident reporting;
- automatic encrypted backups;
- cloud restoration;
- multi-device conflict recovery;
- distributed circuit breakers;
- background repair jobs;
- signed Campaign integrity manifests;
- recovery bundles for support;
- user-selectable recovery points;
- synchronized operation logs.

The MVP MUST NOT implement these capabilities without a later milestone.

## 87. Open Questions

The following remain open:

- Which failures should trigger automatic retry?
- How many provider retries are acceptable?
- Should player input be visibly retained after narration failure?
- How should read-only recovery mode be presented?
- Which integrity checks run at every startup?
- Should Chronicle create a checkpoint before every Session?
- What storage backup policy is required for MVP?
- How should staged random values be persisted?
- Which workflow responses may be retained for retry?
- How long should failed Operation Records remain?
- Should deterministic Archivist fallback be available in MVP?
- Which recovery actions require explicit confirmation?
- How should Campaign export for repair work?
- What exact behavior applies when the Rule Set is unavailable?
- Which critical failures should prevent the application from opening other Campaigns?

These questions require persistence, UI, infrastructure, and delivery RFCs.

## 88. Compliance Checklist

An implementation complies when:

- failures are classified explicitly;
- commit status is always known or treated as uncertain;
- retries preserve OperationId;
- failure after commit returns existing results;
- pending Rolls never reroll during recovery;
- stale provider output is rejected;
- finalization cannot duplicate changes;
- Campaign integrity blocks unsafe play;
- read-only recovery is possible where safe;
- hidden information is redacted;
- automatic retries are bounded;
- destructive repair is never silent;
- the last consistent checkpoint remains identifiable.

## 89. Final Principle

Chronicle may fail to continue a story.

It must never fail to remember which parts of that story became true.
