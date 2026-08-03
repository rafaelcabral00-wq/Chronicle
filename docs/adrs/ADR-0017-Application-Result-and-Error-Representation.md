---
id: ADR-0017
title: Application Result and Error Representation
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
  - ADR-0003
  - ADR-0004
  - ADR-0005
  - ADR-0007
  - ADR-0010
  - ADR-0013
  - ADR-0014
  - ADR-0015
  - ADR-0016
  - RFC-0016
  - RFC-0017
  - RFC-0018
  - RFC-0020
  - RFC-0035
  - RFC-0036
  - RFC-0039
  - RFC-0040
  - RFC-0042
---

> **"A failure should tell Chronicle what happened, what remains safe, and what can be done next."**

# Application Result and Error Representation

## 1. Status

**Proposed**

This ADR defines Chronicle's Application result and error representation.

The decision is:

- use explicit typed result unions for expected outcomes;
- keep exceptions for unexpected failures and infrastructure boundaries;
- define stable machine-readable error codes;
- classify retryability, recoverability, commit state, and user action separately;
- preserve `OperationId`, `CorrelationId`, and safe reference metadata;
- avoid exposing provider, SQLite, filesystem, or platform exception types outside Infrastructure;
- map Application errors into Presentation-safe error models;
- never include credentials, raw provider payloads, unrestricted narrative, or hidden Campaign information in errors;
- distinguish validation, conflict, unavailable, rejected, ambiguous, committed-with-warning, and terminal failure;
- make result contracts versionable when used in durable payloads or external contracts;
- avoid a single generic `Result<T>` whose untyped error bag becomes an informal exception system.

The decision becomes **Accepted** after a vertical-slice spike proves:

- success;
- validation failure;
- not found;
- authorization failure;
- stale version;
- OperationId replay;
- fingerprint conflict;
- provider timeout;
- provider refusal;
- rate limit;
- credential repair requirement;
- commit failure;
- cancellation before commit;
- cancellation after commit;
- ambiguous completion;
- committed result with post-commit notification failure;
- safe UI mapping;
- safe logging;
- deterministic serialization for durable status.

## 2. Context

Chronicle operations can fail in many ways.

Examples include:

- invalid player input;
- missing Campaign;
- cross-Campaign reference;
- Character schema violation;
- Rule Set rejection;
- stale aggregate version;
- duplicate operation;
- conflicting OperationId fingerprint;
- provider timeout;
- provider refusal;
- provider rate limit;
- unavailable credential;
- malformed structured output;
- SQLite concurrency conflict;
- migration failure;
- backup validation failure;
- restore incompatibility;
- Work Item terminal failure;
- user cancellation;
- process interruption after commit.

Not every failure means the same thing.

A useful result model must answer:

```text
Did the intended authoritative effect commit?
Can the same OperationId be retried?
Must the user change input or configuration?
Is the Campaign still safe?
Is recovery automatic, manual, or impossible?
What safe information may the UI display?
```

A raw exception does not answer these questions reliably.

A generic success/failure boolean is also insufficient.

RFC-0018 defines the conceptual error and recovery model.

ADR-0013 defines command execution and transaction behavior.

ADR-0014 defines durable retries and Work Item recovery.

ADR-0015 defines query result behavior.

This ADR selects the concrete representation.

## 3. Decision Drivers

The result and error model prioritizes:

1. explicit expected outcomes;
2. safe recovery semantics;
3. provider and persistence abstraction;
4. stable machine-readable codes;
5. Presentation-safe mapping;
6. idempotency;
7. cancellation clarity;
8. commit-state clarity;
9. testability;
10. observability;
11. privacy;
12. future API compatibility.

## 4. Decision Summary

Chronicle will use:

```text
Expected Outcomes
    typed result unions

Unexpected Failures
    exceptions caught at architectural boundaries

Error Identity
    stable ErrorCode
    stable ErrorCategory

Recovery Metadata
    CommitState
    RetryClassification
    Recoverability
    RequiredUserAction

Correlation
    OperationId
    CorrelationId
    ReferenceCode

Presentation
    safe mapped error model
    no raw exception

Serialization
    explicit versioned contracts where durable
```

## 5. Result Families

Chronicle distinguishes:

```text
Command Result
Query Result
Work Item Execution Result
Provider Result
Infrastructure Adapter Result
Presentation Error Model
```

These families may share common primitives but should not be collapsed into one unrestricted universal type.

## 6. Command Result

A command result represents one operation intention.

Conceptually:

```csharp
public abstract record CommandResult<TSuccess>;

public sealed record CommandSucceeded<TSuccess>(
    TSuccess Value,
    OperationReceipt Receipt)
    : CommandResult<TSuccess>;

public sealed record CommandFailed<TSuccess>(
    ApplicationError Error,
    OperationReceipt Receipt)
    : CommandResult<TSuccess>;
```

The exact implementation may use a discriminated-union library, source generation, or repository-owned types.

## 7. Query Result

A query result represents one read request.

Recommended result categories:

```text
Found
Empty
NotFound
Forbidden
Unavailable
Failed
```

`Empty` is a valid successful collection state and is not the same as `NotFound`.

## 8. Work Item Result

A Work Item execution result uses explicit workflow outcomes:

```text
Completed
RetryAfter
WaitingForUser
WaitingForDependency
Cancelled
Superseded
FailedTerminal
RecoveryRequired
```

These map into Work Item status transitions.

## 9. Provider Result

Provider adapters return provider-neutral outcomes such as:

```text
ProviderSucceeded
ProviderRefused
ProviderRateLimited
ProviderTimedOut
ProviderUnavailable
ProviderAuthenticationFailed
ProviderQuotaExceeded
ProviderOutputMalformed
ProviderOutputIncomplete
ProviderCancelled
```

Provider SDK exceptions do not escape the adapter.

## 10. Success Type

A success type SHOULD contain only the data required by the caller.

Examples:

```text
CampaignCreatedResult
DiceRollExecutedResult
SessionFinalizedResult
BackupCreatedResult
OperationStatusReadModel
```

Success types SHOULD be immutable.

## 11. Operation Receipt

Every important command result SHOULD include an `OperationReceipt`.

Recommended fields:

```text
OperationId
CorrelationId
OperationType
CommitState
OperationStatus
CommittedAtUtc when applicable
ResultReferenceType
ResultReferenceId
CurrentAggregateVersion when relevant
ReferenceCode
```

## 12. Commit State

Chronicle defines explicit commit state:

```text
NotStarted
NotCommitted
Committed
CommitUnknown
NotApplicable
```

## 13. Commit State Meaning

### NotStarted

No authoritative execution began.

### NotCommitted

Execution began, but no authoritative effect committed.

### Committed

The intended authoritative effect committed.

### CommitUnknown

The caller cannot yet determine whether commit occurred and must inspect Operation status.

### NotApplicable

The result does not involve an authoritative write.

## 14. Error Category

Recommended categories:

```text
Validation
NotFound
Authorization
Visibility
Conflict
Concurrency
Idempotency
RuleSet
Provider
Credential
Configuration
Persistence
Migration
Backup
Restore
Import
Export
Package
Security
Cancellation
Unavailable
Integrity
Internal
```

## 15. Error Code

Every expected error has a stable machine-readable code.

Examples:

```text
campaign.not-found
campaign.cross-reference
session.already-active
session.stale-version
operation.fingerprint-conflict
operation.commit-unknown
dice.roll-already-committed
provider.rate-limited
provider.authentication-failed
provider.output-malformed
credential.not-configured
credential.store-unavailable
backup.validation-failed
restore.package-missing
security.hidden-reference-rejected
```

## 16. Error Code Rules

Error codes MUST be:

- stable;
- language-neutral;
- lowercase;
- bounded;
- documented;
- independent from localized display text;
- versioned through compatibility policy when meaning changes.

## 17. Error Code Grammar

Recommended grammar:

```text
[a-z0-9]+([.-][a-z0-9]+)*
```

## 18. Error Object

A common Application error SHOULD contain:

```text
ErrorCode
ErrorCategory
SafeMessageKey
SafeArguments
CommitState
RetryClassification
Recoverability
RequiredUserAction
OperationId when relevant
CorrelationId
ReferenceCode
DataPreservationState
DiagnosticsAvailable
DetailsContractVersion
SafeDetails
```

## 19. Safe Details

`SafeDetails` contains only reviewed typed metadata.

Examples:

- expected and current version;
- missing package identifier;
- retry-after duration;
- invalid field keys;
- required configuration area;
- affected entity type;
- maximum allowed size.

It MUST NOT be an arbitrary dictionary accepting any object.

## 20. No Arbitrary Metadata Bag

A generic `Dictionary<string, object>` error payload is prohibited for stable contracts.

Typed detail records are preferred.

## 21. Safe Message Key

Application errors carry a stable message key rather than finalized localized prose.

Example:

```text
errors.provider.rateLimited
```

Presentation resolves localization and formatting.

## 22. Safe Arguments

Message arguments MUST be explicitly approved.

Allowed examples:

- safe package display name;
- retry duration;
- bounded numeric count;
- field display label;
- operation reference code.

Disallowed examples:

- raw player input;
- Secret text;
- API key fragments;
- raw exception message;
- provider response body;
- local full path.

## 23. Retry Classification

Recommended values:

```text
DoNotRetry
RetrySameOperationImmediately
RetrySameOperationAfterDelay
RetrySameOperationAfterRepair
CheckOperationStatus
StartNewOperation
UserDecisionRequired
```

## 24. Retry Same Operation

A retry of the same intention reuses the same OperationId.

This is appropriate when:

- the prior effect did not commit;
- the operation is safely resumable;
- configuration or transient infrastructure has been repaired;
- the request fingerprint is unchanged.

## 25. Start New Operation

A new OperationId is required when:

- semantically important input changes;
- the user chooses a different action;
- prior operation is terminal and cannot resume;
- the new action is not the same intention.

## 26. Recoverability

Recommended values:

```text
AutomaticallyRecoverable
RecoverableByRetry
RecoverableByConfiguration
RecoverableByUserDecision
RecoverableByRestore
RecoverableByMigration
NotRecoverable
Unknown
```

## 27. Required User Action

Recommended values:

```text
None
CorrectInput
RefreshState
ConfirmAgain
ConfigureCredential
RepairProviderProfile
InstallRequiredPackage
RestoreBackup
OpenSafeMode
ChooseDifferentFile
Retry
ContactSupport
```

## 28. Data Preservation State

Every significant failure SHOULD declare what happened to user data.

Recommended values:

```text
Unchanged
CommittedSafely
PreservedInCheckpoint
PartiallyStagedNotPublished
UnknownRequiresInspection
AtRisk
```

## 29. Validation Error

Validation errors SHOULD expose:

```text
FieldErrors
FormErrors
RuleKeys
Maximums or ranges where safe
```

## 30. Field Error

A field error SHOULD contain:

```text
FieldKey
ErrorCode
MessageKey
SafeArguments
```

It should not depend on UI control names.

## 31. Multiple Validation Errors

Validation may return several independent field errors.

The collection MUST be bounded.

## 32. Domain Rule Failure

A Domain rule failure represents an invariant or lifecycle rejection.

Examples:

- Session already active;
- Scene cannot close;
- Character cannot advance;
- Memory state transition invalid.

It is not an internal exception.

## 33. Rule Set Failure

A Rule Set failure represents deterministic mechanical rejection or package incompatibility.

It SHOULD include:

- RuleOperationKey;
- RuleSetPackageId;
- RuleSetVersion;
- safe rule failure code;
- safe parameters.

## 34. Not Found Versus Hidden

A caller may receive `NotFound` rather than `Forbidden` when revealing entity existence would expose hidden information.

This policy is explicit per query or command.

## 35. Authorization Failure

Authorization errors state that the actor cannot perform the operation.

They must not expose hidden target details.

## 36. Visibility Failure

Visibility errors state that the requested projection or reference is not visible from the caller's perspective.

The response should reveal no hidden content.

## 37. Concurrency Error

A concurrency error SHOULD include:

```text
ExpectedVersion
CurrentVersion when safe
EntityType
EntityId
RefreshRequired
```

It MUST NOT trigger blind semantic retry.

## 38. Idempotent Replay

An OperationId replay with matching fingerprint is a successful result, not an error.

The result SHOULD indicate:

```text
WasReplayed = true
```

or equivalent safe receipt metadata.

## 39. Fingerprint Conflict

A repeated OperationId with different semantic input returns:

```text
operation.fingerprint-conflict
```

Commit state is based on the existing operation.

The new request is rejected.

## 40. Provider Error Mapping

Provider-specific errors map to stable Chronicle codes.

Examples:

```text
HTTP 401
    → provider.authentication-failed

HTTP 429
    → provider.rate-limited

provider refusal
    → provider.refused

malformed structured output
    → provider.output-malformed
```

## 41. Provider Details

Safe provider details MAY include:

- provider profile ID;
- capability;
- retry-after duration;
- model profile;
- safe provider request identifier;
- compatibility state.

They MUST NOT include raw prompt or response.

## 42. Credential Error

Credential errors distinguish:

```text
NotConfigured
StoreUnavailable
AccessDenied
RejectedByProvider
ReferenceInvalid
```

## 43. Persistence Error

Expected persistence errors include:

- optimistic concurrency;
- unique constraint representing duplicate effect;
- database unavailable;
- disk full;
- database locked beyond policy;
- integrity check failure.

Raw SQLite error codes remain inside Infrastructure but may be mapped to safe detail fields.

## 44. Unique Constraint Mapping

Known unique constraints MUST map to semantic outcomes.

Examples:

```text
unique DiceRoll OperationId
    → existing Roll result

unique finalization SessionId
    → existing finalization result

unique advancement OperationId
    → existing advancement result
```

Unknown unique violations remain internal errors.

## 45. Migration Error

Migration errors SHOULD state:

- source storage version;
- target storage version;
- failing migration ID;
- checkpoint availability;
- Safe Mode requirement;
- data preservation state.

## 46. Backup Error

Backup errors SHOULD distinguish:

```text
TargetUnavailable
InsufficientSpace
SnapshotFailed
ManifestFailed
IntegrityFailed
PublicationFailed
Cancelled
```

## 47. Restore Error

Restore errors SHOULD distinguish:

```text
ArchiveInvalid
ChecksumMismatch
UnsupportedVersion
RequiredPackageMissing
CredentialReconfigurationRequired
IntegrityFailed
PublicationBlocked
```

## 48. Cancellation

Cancellation is represented explicitly.

Recommended outcomes:

```text
CancelledBeforeExecution
CancelledBeforeCommit
CancellationRequestedButCommitted
CancellationUnknownCheckStatus
CancellationNotSupportedAtCurrentStage
```

## 49. Cancellation Is Not Always Error

User-requested cancellation before work begins may be a normal terminal outcome.

The result type may represent it separately from failure.

## 50. Ambiguous Completion

When commit status is unknown, return:

```text
operation.commit-unknown
```

with:

```text
CommitState = CommitUnknown
RetryClassification = CheckOperationStatus
RequiredUserAction = RefreshState
```

## 51. Committed With Warning

A command may commit successfully while post-commit notification or optional work fails.

This returns success with warnings, not failure.

Recommended structure:

```text
Success Value
Operation Receipt
Warnings
```

## 52. Warning

A warning SHOULD contain:

```text
WarningCode
MessageKey
SafeArguments
Recoverability
```

Warnings do not imply rollback.

## 53. Warning Examples

```text
notification.desktop-failed
read-model.refresh-delayed
diagnostic.metric-dropped
optional-index-update-pending
```

## 54. Partial Success

The term `partial success` is discouraged for authoritative commands.

A command either:

- committed its defined authoritative intention;
- did not commit it;
- or has unknown commit state.

Optional consequences may produce warnings.

## 55. Batch Operations

A true batch command MAY return per-item outcomes only when the contract explicitly allows independent item commits.

The result must state whether the batch is:

- atomic;
- best effort;
- staged;
- all-or-nothing.

## 56. Exceptions

Exceptions are reserved for:

- programming defects;
- violated internal assumptions;
- unexpected infrastructure failures;
- corrupt state;
- framework failures not mapped at the adapter boundary.

## 57. Boundary Catching

Exceptions are caught at:

- provider adapter boundary;
- persistence adapter boundary;
- filesystem adapter boundary;
- Work Item worker boundary;
- command dispatcher boundary;
- query dispatcher boundary;
- desktop process boundary.

## 58. No Exception-Driven Expected Flow

Expected states such as validation failure, not found, stale version, rate limit, and cancellation must not require exception control flow across Application boundaries.

## 59. Exception Sanitization

Before logging or mapping, exceptions are sanitized.

Potentially unsafe contents include:

- provider response body;
- SQL parameters;
- local paths;
- file names;
- user content;
- authorization headers.

## 60. Internal Error

Unexpected failures map to:

```text
internal.unexpected-failure
```

or a more specific safe internal code.

The UI receives:

- safe message;
- reference code;
- data preservation state;
- recovery action.

It does not receive stack trace.

## 61. Reference Code

A reference code helps correlate user-visible errors with logs and Operation Records.

It SHOULD be:

- short enough to copy;
- opaque;
- nonsecret;
- stable for the error occurrence;
- connected to CorrelationId or OperationId internally.

## 62. Reference Code Format

Example:

```text
CHR-8F3A-19D2
```

The exact algorithm is deferred.

## 63. Correlation Metadata

Errors SHOULD preserve:

```text
OperationId
CorrelationId
WorkItemId when relevant
ProviderRequestId when safe
```

## 64. Error Serialization

Errors stored in:

- Operation Records;
- Work Items;
- durable recovery state;
- portable diagnostics;

must use explicit versioned contracts.

## 65. Error Contract Version

Recommended field:

```text
ErrorContractVersion
```

## 66. Durable Error Payload

A durable error payload SHOULD contain only:

- stable code;
- category;
- recovery metadata;
- safe details;
- timestamps;
- reference IDs.

It MUST NOT serialize exception objects.

## 67. Backward Compatibility

Old durable error codes remain interpretable.

A changed semantic meaning requires:

- new code;
- or explicit version mapping.

## 68. Presentation Mapping

Application errors map into a Presentation model.

Recommended fields:

```text
TitleKey
MessageKey
SafeArguments
Severity
PrimaryAction
SecondaryAction
CanDismiss
PersistsAcrossNavigation
ReferenceCode
DiagnosticsAvailable
```

## 69. Presentation Severity

Recommended values:

```text
Information
Warning
Error
Critical
```

Severity is separate from ErrorCategory.

## 70. Error Display Selection

The UI chooses among:

- inline field error;
- form summary;
- toast;
- persistent banner;
- modal;
- recovery screen.

The choice depends on Presentation severity and required action.

## 71. Critical Error Persistence

Critical errors requiring recovery must remain visible after navigation.

They must not rely on transient toast display.

## 72. No Raw Exception in UI

The UI MUST NOT display:

- exception type;
- stack trace;
- SQL error;
- raw HTTP body;
- raw provider refusal payload;
- Windows platform error text;

unless explicitly shown in protected developer diagnostics.

## 73. Developer Diagnostics

Developer mode MAY expose:

- exception type;
- sanitized stack trace;
- adapter code;
- safe underlying error code;
- diagnostic event links.

It still must not expose credentials or unrestricted Campaign content.

## 74. Localization

Error codes and message keys are stable English machine keys.

Displayed text is localized in Presentation.

## 75. Logging

Logs SHOULD record:

- ErrorCode;
- ErrorCategory;
- CommitState;
- RetryClassification;
- Recoverability;
- OperationId;
- CorrelationId;
- reference code;
- safe details;
- exception type for unexpected failures.

## 76. No Duplicate Logging

Expected errors SHOULD be logged once at the appropriate boundary.

Lower layers should not repeatedly log and rethrow the same expected failure.

## 77. Logging Level Guidance

Recommended mapping:

```text
Validation
    Debug or Information

NotFound
    Debug or Information

Concurrency
    Warning

RateLimit
    Warning

CredentialRequired
    Warning

OperationFingerprintConflict
    Warning or Security depending on context

PersistenceFailure
    Error

IntegrityFailure
    Critical

UnexpectedInternalFailure
    Error or Critical
```

## 78. Security Errors

Security-related errors SHOULD use stable codes.

Examples:

```text
security.cross-campaign-reference
security.hidden-reference-rejected
security.path-traversal-blocked
security.untrusted-package
security.provider-tool-request-rejected
```

## 79. Security Error Detail

Security errors must not echo malicious payloads in full.

## 80. Query Error Mapping

Queries distinguish:

- invalid query;
- not found;
- forbidden;
- unavailable;
- timeout;
- storage failure.

A query `Empty` result remains successful.

## 81. UI Draft Preservation

Error metadata SHOULD tell Presentation whether local drafts may be preserved.

Recommended field:

```text
DraftDisposition
```

Values:

```text
Preserve
Clear
PreserveUntilRefresh
NotApplicable
```

## 82. Retry Action Safety

The UI must not infer retry safety from error category alone.

It uses `RetryClassification`.

## 83. Operation Status Inspection

For ambiguous completion or durable work, the UI uses:

```text
GetOperationStatusQuery
```

rather than repeating the command blindly.

## 84. Error Catalog

Chronicle SHOULD maintain a central error catalog.

The catalog includes:

- ErrorCode;
- category;
- default message key;
- commit-state expectations;
- retry classification;
- recoverability;
- documentation link key;
- owner module.

## 85. Catalog Ownership

Each module owns its error-code namespace.

Examples:

```text
campaign.*
session.*
dice.*
operation.*
provider.*
credential.*
persistence.*
backup.*
restore.*
security.*
```

## 86. Duplicate Error Code

Duplicate code registration is a build or startup error.

## 87. Error Code Removal

Published error codes SHOULD not be removed casually.

They may be deprecated and mapped.

## 88. Result Helpers

Chronicle MAY provide small helper APIs for:

- success creation;
- error creation;
- propagation;
- mapping;
- warning attachment.

Helpers must not obscure commit state or recovery metadata.

## 89. Monadic Composition

Functional result composition MAY be used internally.

It must remain readable to contributors and must not create deeply nested generic signatures.

## 90. Third-Party Union Library

A third-party discriminated-union library MAY be adopted after review.

Selection criteria:

- license;
- maintenance;
- serialization behavior;
- source generation;
- debugger experience;
- AOT compatibility where relevant;
- no framework leakage into Domain contracts.

## 91. Repository-Owned Union

A small repository-owned result union is acceptable and likely preferred for MVP.

## 92. Null and Default Values

A successful result must not use null to represent a hidden error.

A failed result must not carry a seemingly valid success value.

## 93. Exhaustive Handling

Callers SHOULD be forced or strongly encouraged to handle all result cases.

Switch expressions or generated match methods are preferred.

## 94. Unhandled Result Case

An unhandled new result variant is a compile-time or test failure where possible.

## 95. Metrics

Useful metrics include:

```text
ApplicationErrorCount
ErrorCountByCode
RetryableErrorCount
CommitUnknownCount
ConcurrencyConflictCount
ProviderFailureCount
RecoveryRequiredCount
CommittedWithWarningCount
UnhandledExceptionCount
```

## 96. Privacy

Metrics and logs must not include:

- user text;
- Secret text;
- Character names by default;
- file paths;
- credentials;
- provider payloads.

## 97. Testing Strategy

The result model requires:

```text
Unit Tests
Serialization Tests
Command Integration Tests
Query Integration Tests
Provider Mapping Tests
Persistence Mapping Tests
Presentation Mapping Tests
Security Tests
Architecture Tests
```

## 98. Unit Tests

Unit tests SHOULD cover:

- error construction;
- code validation;
- recovery metadata;
- warning attachment;
- safe details;
- exhaustive matching;
- reference code generation.

## 99. Serialization Tests

Tests MUST cover:

- durable error round-trip;
- contract version;
- unknown old error code;
- safe detail types;
- rejection of exception serialization;
- no arbitrary object metadata.

## 100. Command Integration Tests

Tests MUST prove correct results for:

- validation;
- Domain rejection;
- stale version;
- idempotent replay;
- fingerprint conflict;
- commit success;
- commit failure;
- ambiguous completion;
- committed with warning.

## 101. Provider Mapping Tests

Tests MUST map:

- authentication;
- rate limit;
- timeout;
- unavailable;
- refusal;
- malformed output;
- incomplete output;
- quota;
- cancellation;
- unknown provider failure.

## 102. Persistence Mapping Tests

Tests MUST map:

- known unique constraints;
- concurrency conflict;
- disk full;
- database unavailable;
- integrity failure;
- unknown SQLite failure.

## 103. Presentation Mapping Tests

Tests MUST prove:

- field validation becomes inline;
- credential repair becomes actionable banner;
- rate limit shows retry timing;
- critical integrity failure persists;
- raw exception is absent;
- reference code is visible where appropriate;
- draft disposition is respected.

## 104. Security Tests

Tests MUST attempt to place in errors:

- API key;
- raw prompt;
- provider response;
- Secret content;
- Character biography;
- full local path;
- SQL parameters;
- malicious input.

The user-visible and durable error payloads MUST not contain those values.

## 105. Required Test Cases

Tests MUST cover:

- success;
- success with warning;
- empty query;
- not found;
- hidden-as-not-found;
- forbidden;
- validation field errors;
- Domain rule failure;
- Rule Set failure;
- stale version;
- OperationId replay;
- fingerprint conflict;
- provider rate limit;
- provider timeout;
- provider refusal;
- malformed output;
- credential missing;
- credential store unavailable;
- disk full;
- commit rollback;
- commit unknown;
- cancellation before commit;
- cancellation after commit;
- failed post-commit notification;
- migration checkpoint available;
- restore incompatible;
- internal unexpected exception;
- durable serialization;
- Presentation localization key mapping.

## 106. Architecture Tests

Architecture tests MUST reject:

- provider SDK exceptions in Application public APIs;
- SQLite exception types in Presentation;
- raw exception serialization;
- arbitrary error metadata dictionaries;
- secrets in error detail types;
- success and error populated simultaneously;
- UI retry logic based only on message text;
- Domain dependence on Presentation error models;
- localized prose as machine error identity.

## 107. Prohibited Patterns

### 107.1 Boolean Success With String Error

Insufficient for recovery and commit semantics.

### 107.2 Raw Exception as Application Result

Infrastructure exceptions are mapped.

### 107.3 Generic Arbitrary Error Bag

Use typed details.

### 107.4 Error Message as Code

Machine identity and display text remain separate.

### 107.5 All Failures Are Retryable

Retry classification is explicit.

### 107.6 Unknown Commit State Treated as Failure and Retried

Inspect Operation status first.

### 107.7 Partial Success Without Contract

Authoritative intention and optional warnings must be explicit.

### 107.8 Secret in Error for Diagnostics

Use safe reference metadata.

### 107.9 UI Parses Error Strings

UI switches on typed codes and metadata.

### 107.10 Expected Failure Logged at Every Layer

Log once at the correct boundary.

## 108. Alternatives Considered

### Exceptions for All Failures

Rejected because expected failures and recovery semantics would remain implicit and difficult to test.

### One Universal `Result<T>`

A small shared primitive may exist, but one unrestricted error type for every layer risks becoming an untyped bag.

Chronicle prefers bounded result families with shared stable metadata.

### HTTP Problem Details as Internal Model

Not selected because Chronicle is a local desktop application and internal result semantics include commit, recovery, and OperationId behavior beyond HTTP concerns.

A future API adapter may map Chronicle errors to Problem Details.

### Error Enums Only

Insufficient because errors require typed details, versioning, recovery, and localization keys.

### Third-Party Functional Library Immediately

Deferred until the exact ergonomics and serialization requirements are proven.

## 109. Consequences

### Positive

- clear recovery behavior;
- safe UI mapping;
- stable machine-readable errors;
- no infrastructure exception leakage;
- better idempotency handling;
- explicit cancellation and ambiguous completion;
- deterministic tests;
- future API mapping remains straightforward.

### Negative

- many result and detail types;
- mapping code across boundaries;
- error catalog maintenance;
- developers must classify failures carefully;
- exhaustive handling adds some verbosity;
- contract evolution requires discipline.

## 110. Risks

### Error Type Proliferation

Mitigation:

- shared primitives;
- module-owned namespaces;
- typed details only where useful;
- catalog review.

### Inconsistent Classification

Mitigation:

- central catalog;
- code review;
- contract tests;
- examples and analyzers.

### Sensitive Data Leakage

Mitigation:

- safe detail types;
- no arbitrary bags;
- security tests;
- sanitization at adapter boundaries.

### Incorrect Retry Guidance

Mitigation:

- retry classification owned by Application workflow;
- OperationId tests;
- ambiguous completion tests.

### Result Boilerplate

Mitigation:

- source generation or small helpers;
- avoid overly generic abstractions;
- preserve readability.

## 111. Technology Spike

Before acceptance, implement:

1. command result union;
2. query result union;
3. `ApplicationError`;
4. stable error-code type;
5. Operation Receipt;
6. commit-state model;
7. retry and recoverability enums;
8. validation detail model;
9. provider-error mapper;
10. persistence-error mapper;
11. Presentation error mapper;
12. reference-code generator;
13. durable error serialization;
14. architecture tests;
15. end-to-end ambiguous-completion flow.

## 112. Spike Acceptance

The spike passes when:

- expected failures cross no boundary as raw exceptions;
- provider and SQLite details are mapped safely;
- the UI can distinguish retry, repair, refresh, and restore actions;
- a committed operation with notification failure remains success;
- ambiguous completion requires status inspection;
- the same OperationId replay returns the existing success;
- durable errors survive restart;
- no synthetic secret appears in logs, SQLite error payloads, or UI messages;
- callers handle all result variants explicitly;
- result types remain independent from Avalonia and provider SDKs.

## 113. Definition of Compliance

An implementation complies when:

- expected outcomes use typed result unions;
- stable error codes identify failures;
- commit state is explicit;
- retryability and recoverability are separate;
- OperationId and reference metadata are preserved;
- Infrastructure exceptions do not escape adapters;
- UI receives safe Presentation models;
- warnings do not reinterpret committed success;
- ambiguous completion does not trigger blind retry;
- durable errors are versioned;
- secrets and hidden Campaign data are excluded;
- architecture and security tests enforce the boundary.

## 114. Review Triggers

This ADR must be reviewed if:

- Chronicle exposes a public network API;
- plugins consume Application results;
- multiplayer introduces remote authorization errors;
- distributed services require standardized cross-process error contracts;
- a third-party discriminated-union library is adopted;
- support tooling depends on stable external error documentation;
- localization architecture changes;
- error volume or complexity becomes difficult to maintain.

## 115. Deferred Decisions

Later ADRs MAY define:

- exact discriminated-union implementation;
- exact reference-code algorithm;
- public API mapping to Problem Details;
- error documentation generation;
- support knowledge-base links;
- localized message catalog format;
- plugin-facing error compatibility;
- telemetry aggregation by error code;
- redacted developer diagnostic views.

## 116. Final Decision

Chronicle will represent expected Application outcomes through explicit typed result unions and stable machine-readable errors.

Every important failure will state whether authoritative state committed, whether the same operation may be retried, what remains preserved, and what action is required.

Exceptions will remain for unexpected defects and adapter boundaries, where they will be sanitized and mapped.

Chronicle should never force the player to guess whether a failed screen means the Campaign changed.

The result must say.
