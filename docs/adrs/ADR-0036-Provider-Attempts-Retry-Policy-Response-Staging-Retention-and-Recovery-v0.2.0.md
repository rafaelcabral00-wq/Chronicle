---
id: ADR-0036
title: Provider Attempts, Retry Policy, Response Staging, Retention, and Recovery
status: Proposed
version: 0.2.0
owner: Chronicle Team
last_updated: 2026-08-02
category: Technology
supersedes:
  - ADR-0036@0.1.0
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
  - RFC-0033
  - RFC-0034
  - ADR-0001
  - ADR-0002
  - ADR-0004
  - ADR-0005
  - ADR-0007
  - ADR-0008
  - ADR-0010
  - ADR-0013
  - ADR-0014
  - ADR-0017
  - ADR-0019
  - ADR-0020
  - ADR-0024
  - ADR-0025
  - ADR-0026
  - ADR-0033
  - ADR-0034
  - ADR-0035
  - ADR-0038
  - ADR-0040
  - ADR-0041
implements:
  - RFC-0021
  - RFC-0025
  - RFC-0033
related_to:
  - ADR-0027
  - ADR-0028
  - ADR-0037
  - ADR-0039
  - ADR-0042
---

> **"Provider work may be retried. Provider output may be repaired. Neither may become authority until Chronicle accepts it."**

# Provider Attempts, Retry Policy, Response Staging, Retention, and Recovery

## 1. Status

**Proposed**

This ADR defines Chronicle's concrete architecture for Narrative Intelligence provider execution.

This revision aligns provider execution with the corrected Narrative Turn and persistence model.

The decision is:

- represent every provider call as a durable `ProviderAttempt`;
- bind every Provider Attempt to exactly one `NarrativeTurnId`;
- keep Provider Attempt as operational evidence, never Campaign truth;
- use the canonical `NarratorTurnOutput` contract for Narrator responses;
- support bounded retry only when the failure classification permits it;
- support bounded repair attempts under the same NarrativeTurnId;
- use a durable Work Item for provider execution when restart-safe scheduling is required;
- create the Provider Attempt before sending the external request;
- never hold a database transaction while waiting for the provider;
- optionally stage the response durably before validation;
- use `NarrativeResponseStaging` only for crash recovery, validation retry, bounded repair, or explicit diagnostics;
- retain staged raw provider content for the shortest practical period;
- delete staged content after normalized acceptance, terminal rejection, or expiry;
- do not retain unrestricted prompts or raw responses indefinitely in Stable;
- never store credentials in Provider Attempt or staging records;
- treat provider-native thread or request identifiers as optional operational metadata;
- never make provider-native state required for recovery;
- reconstruct required context from Chronicle-owned local records;
- reject stale, duplicate, late, or superseded responses;
- use canonical payload hashing and unique constraints to prevent duplicate acceptance;
- distinguish provider transport success from Chronicle output acceptance;
- permit fallback to another provider only when explicitly configured and when the same provider-neutral contract can be preserved;
- keep provider fallback outside the mandatory MVP scope unless already selected elsewhere;
- never combine outputs from several providers into one authoritative response automatically;
- use bounded exponential backoff with jitter for transient failures;
- classify authentication and contract failures as nontransient until configuration or code changes;
- keep the user informed through safe typed errors and honest progress;
- use English semantic keys and base diagnostics;
- keep provider architecture independent from Werewolf and all Rule Set mechanics.

## 2. Context

Narrative Intelligence is an external dependency with failure modes Chronicle does not control.

A provider call may:

- time out;
- fail before sending;
- fail after receiving the request;
- return malformed structured output;
- return valid transport data with an invalid Chronicle contract;
- return after a retry already succeeded;
- return after the Narrative Turn became stale;
- become inaccessible after application restart;
- expose provider-native identifiers that cannot be recreated;
- partially stream content before final completion;
- return content that conflicts with Campaign authority.

Chronicle must recover without:

- duplicating Messages;
- duplicating Dice Rolls;
- applying events twice;
- depending on remote threads;
- retaining unnecessary private content;
- retrying terminal failures indefinitely;
- treating transport success as accepted narrative.

## 3. Decision Drivers

The architecture prioritizes:

1. provider neutrality;
2. durable recovery;
3. bounded retry;
4. idempotent acceptance;
5. privacy;
6. explicit failure classification;
7. local authority;
8. no hidden remote-state dependency;
9. clear user recovery;
10. MVP scope control;
11. testability;
12. compatibility with future providers.

## 4. Architectural Position

```text
Application
    Narrative Turn orchestration
    acceptance authority

Provider Adapter
    transport and provider translation

Provider Attempt
    durable execution evidence

Response Staging
    temporary raw-content recovery boundary

Accepted Output
    normalized Chronicle contract

Messages and Event Operations
    authoritative results after acceptance
```

## 5. Provider Attempt Identity

Every provider call has:

```text
ProviderAttemptId
```

Chronicle creates it before external execution.

## 6. Provider Attempt Record

Recommended fields:

```text
ProviderAttempt
├── ProviderAttemptId
├── NarrativeTurnId
├── WorkItemId
├── AttemptNumber
├── ProviderProfileId
├── AdapterKey
├── ModelProfileKey
├── RequestContractVersion
├── ExpectedOutputContractVersion
├── State
├── StartedAtUtc
├── RequestSentAtUtc
├── ResponseReceivedAtUtc
├── CompletedAtUtc
├── SafeProviderRequestId
├── ProviderResponseStagingId
├── InputTokenCount
├── OutputTokenCount
├── RetryClassification
├── FailureCode
├── CanonicalResponseHash
└── RowVersion
```

## 7. One Narrative Turn

A Provider Attempt belongs to exactly one Narrative Turn.

## 8. Attempt Number

AttemptNumber is monotonically increasing within one NarrativeTurnId.

## 9. Attempt Uniqueness

Recommended:

```text
NarrativeTurnId + AttemptNumber unique
```

## 10. Provider Attempt State Machine

The canonical Provider Attempt states are:

```text
Created
Requesting
ResponseReceived
Validating
Accepted
Rejected
FailedRetryable
FailedTerminal
Cancelled
Superseded
RecoveryRequired
```

## 11. Created

The durable attempt record exists.

No external request has started.

## 12. Requesting

The adapter is executing the provider request.

## 13. Response Received

A provider response was received and captured sufficiently for validation.

## 14. Validating

Chronicle is validating or repairing the response.

## 15. Accepted

This attempt produced the selected accepted output for the Narrative Turn.

## 16. Rejected

The response was received but not accepted.

Examples:

- invalid contract;
- stale context;
- authority violation;
- superseded response;
- exhausted repair.

## 17. Failed Retryable

The provider operation failed without producing an accepted response and may be retried safely.

## 18. Failed Terminal

The same attempt path cannot continue without configuration, code, or user intervention.

## 19. Cancelled

The attempt was intentionally cancelled before acceptance.

## 20. Superseded

Another attempt or Narrative Turn became authoritative first.

## 21. Recovery Required

Chronicle cannot safely determine request or response state automatically.

## 22. Terminal States

Terminal states are:

```text
Accepted
Rejected
FailedTerminal
Cancelled
Superseded
```

`FailedRetryable` and `RecoveryRequired` are not terminal for the parent turn.

## 23. Allowed Transitions

Canonical transitions include:

```text
Created → Requesting
Created → Cancelled
Created → Superseded

Requesting → ResponseReceived
Requesting → FailedRetryable
Requesting → FailedTerminal
Requesting → Cancelled
Requesting → RecoveryRequired
Requesting → Superseded

ResponseReceived → Validating
ResponseReceived → Rejected
ResponseReceived → RecoveryRequired
ResponseReceived → Superseded

Validating → Accepted
Validating → Rejected
Validating → FailedRetryable
Validating → FailedTerminal
Validating → RecoveryRequired
Validating → Superseded

FailedRetryable → Superseded
RecoveryRequired → ResponseReceived
RecoveryRequired → Rejected
RecoveryRequired → FailedRetryable
RecoveryRequired → FailedTerminal
RecoveryRequired → Superseded
```

A retry creates a new ProviderAttempt rather than returning the old attempt to `Requesting`.

## 24. Why New Attempt per Retry

Each external call has distinct:

- timing;
- provider request ID;
- failure;
- token usage;
- staged response;
- diagnostics.

The NarrativeTurnId and OperationId preserve one logical intent.

## 25. Forbidden Transitions

Examples:

- `Accepted → Requesting`;
- `Rejected → Accepted`;
- `FailedTerminal → Requesting`;
- `Superseded → Validating`;
- `Cancelled → Accepted`;
- reusing one AttemptNumber.

## 26. Provider Adapter Contract

Every adapter implements a provider-neutral interface such as:

```text
INarrativeIntelligenceProvider
```

## 27. Adapter Input

The adapter receives:

- safe provider profile reference;
- provider-neutral request contract;
- bounded context;
- expected output contract;
- cancellation token;
- ProviderAttemptId correlation.

## 28. Adapter Output

The adapter returns either:

```text
ProviderTransportResult
```

or a staged response reference.

## 29. No Adapter Authority

The adapter does not:

- accept Messages;
- persist Campaign truth;
- create Dice results;
- apply Structured Events;
- decide retries;
- own Narrative Turn state.

## 30. Provider-Neutral Output

Narrator responses normalize to:

```text
NarratorTurnOutput
```

## 31. Provider Tool Calls

Provider-native tool-call formats are adapter transport details.

They are converted into Chronicle's canonical contract before acceptance.

## 32. No Provider-Specific Root Contract

Application code must not depend on one provider's native response object.

## 33. Attempt Creation Boundary

Before the external call, Chronicle persists:

- NarrativeTurn;
- WorkItem where used;
- ProviderAttempt;
- attempt number;
- adapter and profile keys;
- expected output contract;
- initial state.

## 34. Transaction Boundary

Attempt creation commits before network execution.

## 35. No Network Call in Transaction

Provider network calls never occur inside the database transaction.

## 36. Request Payload

The full prompt or request body is not stored indefinitely by default.

## 37. Request Manifest

Chronicle may persist a safe request manifest containing:

- context-record references;
- contract version;
- role;
- selected package versions;
- prompt-template version;
- bounded hashes.

## 38. Prompt Privacy

Prompts may contain private Campaign content.

Stable retention is minimized.

## 39. Credentials

Credentials are resolved from Windows Credential Manager at execution time.

## 40. No Credential Persistence

ProviderAttempt and response staging must not contain:

- API keys;
- bearer tokens;
- secret headers;
- refresh tokens;
- signed secret URLs.

## 41. External Request Identity

A provider request ID may be stored as:

```text
SafeProviderRequestId
```

when allowed.

## 42. Request ID Limitation

The provider request ID is useful for diagnostics.

It is not required for Chronicle recovery.

## 43. Provider-Native Thread State

Remote conversation or thread IDs may be used as an optimization.

## 44. Thread State Nonauthority

Thread state is not:

- Campaign truth;
- required history;
- required portability data;
- required backup data except optional safe metadata;
- required for a provider replacement.

## 45. Local Context Reconstruction

Chronicle reconstructs provider context from:

- authoritative Messages;
- Character state;
- Character Knowledge;
- Campaign Memories;
- active hierarchy;
- Preferences;
- Rule Set context;
- resolved Dice evidence;
- Narrative Turn trigger;
- contract instructions.

## 46. Provider Thread Loss

Loss of remote thread state causes, at most:

- a new provider context request;
- reduced optimization;
- a recoverable provider failure.

It does not corrupt the Campaign.

## 47. Work Item Integration

Provider execution uses a Work Item when asynchronous or restart-safe handling is needed.

## 48. Recommended Work Item Type

```text
narrative.provider-request
```

## 49. Work Item Correlation

Recommended:

```text
WorkItem.NarrativeTurnId
WorkItem.ProviderAttemptId
WorkItem.OperationId
```

## 50. Work Item Lifecycle

The Work Item uses RFC-0019's canonical lifecycle.

It must not duplicate Provider Attempt states.

## 51. Responsibility Separation

```text
NarrativeTurn
    logical narrative workflow

WorkItem
    schedulable execution

ProviderAttempt
    one external call

OperationRecord
    idempotent authoritative effect

AcceptedOutput
    normalized accepted response
```

## 52. Timeout Policy

Each provider request has bounded timeouts.

## 53. Timeout Layers

Recommended:

```text
Connection timeout
Response-start timeout
Total request timeout
Idle-stream timeout
```

Exact values are configuration, not contract constants.

## 54. Cancellation

Cancellation is cooperative.

## 55. Cancellation Before Response

The attempt becomes `Cancelled` if no accepted response exists.

## 56. Late Response After Cancellation

A late response is staged only if necessary for diagnostics, then rejected or deleted.

## 57. Retry Policy

Retry is bounded and failure-classified.

## 58. Retry Categories

Canonical classifications:

```text
TransientTransport
RateLimited
ProviderOverloaded
TimeoutBeforeKnownResponse
TimeoutWithUnknownResponse
AuthenticationRequired
AuthorizationDenied
InvalidRequest
UnsupportedModel
ContractInvalid
ContentBlocked
ContextTooLarge
StaleContext
Cancelled
Unknown
```

## 59. Transient Transport

Examples:

- DNS failure;
- connection reset;
- temporary network failure.

Usually retryable.

## 60. Rate Limited

Retry only when provider metadata or policy supplies a safe delay.

## 61. Provider Overloaded

Retry with bounded backoff.

## 62. Timeout Before Known Response

May retry when no response could have been accepted.

## 63. Timeout With Unknown Response

Requires inspection or a new attempt with stale-response protection.

It must never allow two accepted outputs.

## 64. Authentication Required

Not automatically retried.

Wait for credential correction.

## 65. Authorization Denied

Terminal until configuration or provider policy changes.

## 66. Invalid Request

Terminal for the same request contract.

## 67. Unsupported Model

Terminal until profile or adapter configuration changes.

## 68. Contract Invalid

May trigger bounded repair when a response exists.

It is not a transport retry.

## 69. Content Blocked

Handled according to safety and role policy.

It is not automatically retried indefinitely.

## 70. Context Too Large

Requires context reduction or a new preparation phase.

## 71. Stale Context

The attempt is rejected or superseded.

A new Narrative Turn or re-prepared attempt may be required.

## 72. Unknown Failure

Defaults to no automatic retry unless the adapter can prove safe transient behavior.

## 73. Retry Count

Provider retry count is bounded per Narrative Turn.

## 74. Recommended MVP Limit

A small configurable limit, such as two or three transport attempts, is appropriate.

The exact number is configuration.

## 75. Backoff

Use bounded exponential backoff.

## 76. Jitter

Use jitter to avoid synchronized retries.

## 77. Retry-After

Provider-supplied Retry-After metadata is honored within configured bounds.

## 78. No Infinite Retry

Every retry path ends in:

- accepted response;
- waiting for configuration;
- user-visible failure;
- terminal failure;
- RecoveryRequired.

## 79. Same Logical Turn

Transport retries remain under one NarrativeTurnId.

## 80. New Provider Attempt

Each call receives a new ProviderAttemptId and AttemptNumber.

## 81. Repair

Repair is distinct from transport retry.

## 82. Repair Purpose

Repair asks the provider to correct:

- invalid schema;
- unknown event type;
- missing field;
- ordering conflict;
- stop-reason conflict;
- authority violation that can be reformulated;
- malformed typed payload.

## 83. Repair Boundaries

Repair may not:

- change committed Campaign state;
- invent Dice results;
- override Rule Set validation;
- rewrite accepted Messages;
- continue past an unresolved Roll.

## 84. Repair Attempt

A repair is a new ProviderAttempt under the same NarrativeTurnId.

## 85. Repair Input

Repair receives:

- bounded validation errors;
- the expected contract;
- safe context needed to correct the response;
- no unnecessary hidden instructions.

## 86. Repair Limit

Repair attempts are bounded separately from transport retries.

## 87. Recommended MVP Repair Limit

One repair attempt is the preferred MVP default.

## 88. Repair Exhaustion

After exhaustion, the turn becomes rejected, failed terminal, or user-retryable according to policy.

## 89. Response Receipt

When a response arrives, Chronicle captures enough data to survive validation interruption.

## 90. Response Staging

Chronicle may create:

```text
NarrativeResponseStaging
```

## 91. Staging Purpose

Staging is allowed for:

- crash after response receipt;
- validation retry;
- repair comparison;
- explicit diagnostics;
- unknown response outcome recovery.

## 92. Staging Is Not Authority

Raw staged content does not become a Message or Structured Event automatically.

## 93. Staging Fields

Recommended:

```text
NarrativeResponseStagingId
ProviderAttemptId
ContractVersionHint
PayloadHash
StorageReference
ByteLength
State
CreatedAtUtc
ExpiresAtUtc
DeletedAtUtc
```

## 94. Staging Location

Large response bytes should be stored in a protected local staging file rather than an unbounded relational column.

## 95. Staging File Name

Use opaque generated names.

Do not use Campaign names, Character names, or provider text.

## 96. Staging Permissions

Use restrictive user-local filesystem permissions.

## 97. Staging Size Limits

Enforce:

- maximum response bytes;
- maximum decompressed bytes if encoded;
- maximum JSON depth;
- maximum collection sizes.

## 98. Staging Hash

Compute a cryptographic hash for duplicate detection and integrity.

## 99. Staging State Machine

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

## 100. Staging Retention

Default Stable policy:

- delete after normalized acceptance;
- delete after terminal rejection;
- delete after expiry;
- preserve only while needed for unresolved recovery;
- preserve for diagnostics only with explicit user action.

## 101. Accepted Raw Response

After acceptance, Chronicle retains:

- normalized accepted output;
- accepted Messages;
- event application records;
- canonical payload hash;
- safe attempt metadata.

The raw response is normally deleted.

## 102. Rejected Raw Response

Rejected raw response is normally deleted after the repair or diagnostics window.

## 103. Diagnostic Capture

A user may explicitly create a diagnostic package.

## 104. Diagnostic Redaction

Diagnostic capture must:

- exclude credentials;
- warn about private narrative content;
- minimize raw content;
- never upload automatically.

## 105. Response Validation

Validation follows:

```text
Transport Decode
Root Contract Validation
NarrativeTurnId Validation
Narrative Block Validation
Structured Event Validation
Ordering Validation
Authority Validation
Rule Set Validation
Continuity Validation
Acceptance Decision
```

## 106. Transport Success Is Not Acceptance

A successful HTTP or provider response may still be rejected.

## 107. Accepted Attempt

At most one Provider Attempt can become `Accepted` for one Narrative Turn.

## 108. Acceptance Uniqueness

Recommended constraints:

```text
NarrativeAcceptedOutputs.NarrativeTurnId unique
```

and one selected accepted attempt reference.

## 109. Duplicate Response

A response with the same canonical payload hash under the same turn is idempotently ignored or linked.

## 110. Different Valid Response After Acceptance

A later valid response is `Superseded`.

It cannot replace the accepted output automatically.

## 111. Late Response

A response is late when:

- another attempt was accepted;
- the Narrative Turn was superseded;
- Campaign context changed incompatibly;
- the user cancelled the turn.

Late responses are not published.

## 112. Stale Context

Before acceptance, Chronicle verifies relevant versions.

## 113. Stale Output Policy

A stale output is:

- rejected;
- superseded;
- or sent through a new turn after rebuilding context.

It is not patched onto current state automatically.

## 114. Partial Streaming

Streaming tokens are nonauthoritative.

## 115. MVP Streaming Policy

The MVP may buffer the complete provider response before validation and display.

## 116. Future Provisional Streaming

Provisional display requires a separate Presentation decision and must be visually marked as uncommitted.

## 117. Provider Fallback

Provider fallback is not required for MVP.

## 118. Optional Future Fallback

When implemented, fallback must:

- use the same provider-neutral request and output contracts;
- create a new ProviderAttempt;
- preserve the same NarrativeTurnId where the intent is unchanged;
- expose provider change in safe diagnostics;
- never combine responses automatically;
- never bypass user provider policy;
- avoid duplicate acceptance.

## 119. No Automatic Multi-Provider Voting

The MVP does not send one turn to several providers and choose or merge outputs.

## 120. Model Change

Changing model profile during retry is explicit policy.

## 121. Default Model Stability

Automatic retry should use the same model profile unless the adapter or configuration explicitly allows fallback.

## 122. Provider Profile Missing

The turn waits for configuration or fails safely.

## 123. Credential Missing

The related Work Item enters `WaitingForUser` or equivalent configuration wait.

## 124. Credential Update

After correction, a new Provider Attempt may run under the same Narrative Turn if context remains valid.

## 125. Provider Removal

Removing a provider profile does not remove Campaign truth or accepted Messages.

## 126. Cost and Usage Metadata

Optional safe metadata may include:

- input tokens;
- output tokens;
- provider-reported usage;
- estimated cost where configured.

## 127. Usage Metadata Authority

Usage metadata is operational and may be incomplete.

## 128. No Required Telemetry

Chronicle does not send provider-attempt telemetry to Chronicle maintainers.

## 129. Offline Behavior

If no provider connection is available:

- Campaign history remains accessible;
- pending turns remain durable;
- provider work becomes retryable or waits for connectivity;
- no fake output is generated unless an explicitly selected local provider exists.

## 130. Application Shutdown

Graceful shutdown stops new provider attempts and requests cancellation for active calls where safe.

## 131. Crash During Request

Startup inspects:

- ProviderAttempt state;
- Work Item claim;
- staged response;
- accepted output;
- Narrative Turn state.

## 132. Crash Before Request

The attempt may be safely scheduled.

## 133. Crash After Request Sent

If no response exists, classify according to adapter guarantees.

A new attempt may run with stale-response protection.

## 134. Crash After Response Received

If staging exists, revalidate it.

## 135. Crash During Validation

Resume from staging or reject safely.

## 136. Crash After Acceptance Commit

Recovery returns the accepted output and Messages.

It does not accept again.

## 137. Unknown Acceptance Outcome

Query:

- NarrativeAcceptedOutput;
- Messages;
- event application records;
- NarrativeTurn state;
- accepted ProviderAttempt.

## 138. No Duplicate Acceptance

Recovery must prove whether acceptance exists before retrying validation publication.

## 139. Backup Inclusion

Backups include Provider Attempts needed for unresolved recovery.

## 140. Backup Exclusion

Completed low-value attempt detail may be compacted according to retention policy.

## 141. Staging in Backup

Active staging is included only when required for unresolved local recovery.

## 142. Portable Export

Portable Campaign export excludes Provider Attempts and raw staging by default.

## 143. Provider Independence in Export

Accepted Messages, Structured Event effects, Dice evidence, and package bindings remain sufficient to understand Campaign history.

## 144. Retention Classes

Provider data is classified as:

```text
Accepted Provenance
Unresolved Recovery
Transient Raw Content
Operational Diagnostics
Usage Metadata
```

## 145. Accepted Provenance Retention

Retain:

- ProviderAttemptId;
- adapter key;
- model profile key;
- accepted time;
- safe request ID where allowed;
- contract version;
- canonical response hash.

## 146. Unresolved Recovery Retention

Retain all safe metadata required to continue or inspect the turn.

## 147. Transient Raw Content Retention

Keep for the shortest practical period.

## 148. Operational Diagnostics Retention

Bound by support policy and privacy settings.

## 149. Usage Metadata Retention

Optional and independently configurable.

## 150. Compaction

Completed rejected and failed attempt details may be compacted after:

- recovery window closes;
- no diagnostics reference exists;
- no audit dependency remains;
- accepted-output provenance is preserved.

## 151. No Cleanup of Unresolved Attempts

Do not delete attempts in:

```text
Requesting
ResponseReceived
Validating
FailedRetryable
RecoveryRequired
```

without a recovery-aware transition.

## 152. Cleanup Work Item

Retention cleanup may use:

```text
provider-attempt.cleanup
```

## 153. Cleanup Safety

Before deletion:

- verify turn terminality;
- verify accepted provenance retained;
- verify staging no longer needed;
- verify no Work Item or Operation references the record.

## 154. Language

State keys, failure keys, adapter keys, model-profile keys, and base diagnostics use English.

Generated narrative language remains a Campaign or user preference.

## 155. Error Model

Recommended provider errors:

```text
provider.profile-not-found
provider.credential-missing
provider.authentication-failed
provider.authorization-denied
provider.model-unsupported
provider.request-invalid
provider.context-too-large
provider.rate-limited
provider.overloaded
provider.transport-failed
provider.timeout
provider.response-outcome-unknown
provider.response-too-large
provider.response-invalid
provider.contract-invalid
provider.content-blocked
provider.repair-exhausted
provider.response-stale
provider.response-duplicate
provider.response-superseded
provider.staging-failed
provider.staging-expired
provider.recovery-required
provider.failed-terminal
```

## 156. Data Preservation State

Results should state:

```text
CampaignStateUnchanged
AttemptCreated
RequestInProgress
ResponseStaged
ResponseRejected
RepairPending
AcceptedOutputCommitted
MessagesPublished
StructuredEventsRecorded
ProviderStateNonAuthoritative
RetryScheduled
WaitingForConfiguration
AttemptSuperseded
RecoveryRequired
```

## 157. Logging

Safe logs may include:

- NarrativeTurnId;
- ProviderAttemptId;
- WorkItemId;
- adapter key;
- model profile key;
- attempt number;
- state transition;
- retry classification;
- response byte count;
- token counts;
- payload hash;
- duration;
- safe failure code.

They must not include:

- credentials;
- authorization headers;
- full prompts;
- full raw responses;
- unrestricted Campaign prose;
- private Character details.

## 158. Metrics

Useful local metrics include:

```text
ProviderAttemptDuration
ProviderAttemptSuccessCount
ProviderAttemptRetryCount
ProviderAttemptRepairCount
ProviderAttemptTerminalFailureCount
ProviderTimeoutCount
ProviderRateLimitCount
ProviderStaleResponseCount
ProviderDuplicateResponseCount
ProviderStagingCleanupCount
```

No remote telemetry is required.

## 159. User Experience

The user-facing flow may show:

```text
Preparing context
Contacting provider
Waiting for response
Validating response
Repairing response
Retrying after temporary failure
Waiting for provider configuration
Recovery required
```

## 160. Honest Progress

Provider latency uses indeterminate progress.

## 161. Retry Disclosure

The UI may state that a temporary retry is occurring without exposing raw provider internals.

## 162. User Retry

After terminal or exhausted failure, the user may explicitly retry.

## 163. Same Turn or New Turn

Use the same NarrativeTurnId only when:

- the logical trigger is unchanged;
- context remains valid;
- no accepted output exists.

Otherwise create a new Narrative Turn.

## 164. Cancel

The user may cancel before accepted output publication where policy permits.

## 165. Cancel After Acceptance

Cancellation does not remove accepted Messages or effects.

## 166. Accessibility

Provider status and failure UI must support:

- keyboard navigation;
- screen readers;
- clear focus;
- noncolor error states;
- actionable recovery text;
- no endlessly animated required content.

## 167. Testing Strategy

The implementation requires:

```text
Provider Attempt State Tests
Transport Retry Tests
Repair Tests
Staging Tests
Acceptance Tests
Duplicate and Late Response Tests
Stale Context Tests
Crash Recovery Tests
Retention Tests
Backup Tests
Export Tests
Privacy Tests
Adapter Contract Tests
Fallback Tests when implemented
```

## 168. State Tests

Every allowed and forbidden Provider Attempt transition must be tested.

## 169. Retry Tests

Tests cover:

- transient network failure;
- rate limit;
- overload;
- timeout;
- authentication failure;
- unsupported model;
- context too large;
- retry exhaustion;
- bounded backoff.

## 170. New Attempt per Retry Test

Each external call creates a distinct ProviderAttemptId and AttemptNumber.

## 171. Repair Tests

Tests cover:

- malformed root contract;
- invalid Structured Event;
- sequence conflict;
- successful repair;
- repair exhaustion;
- authority violation that cannot be repaired safely.

## 172. Staging Tests

Tests cover:

- response receipt;
- crash before validation;
- hash mismatch;
- size limit;
- expiry;
- accepted cleanup;
- rejected cleanup;
- explicit diagnostics retention.

## 173. Credential Tests

Synthetic credentials must never appear in:

- ProviderAttempt;
- staging metadata;
- staging content;
- logs;
- diagnostics without explicit secure handling.

## 174. Acceptance Tests

Tests prove:

- transport success alone does not publish;
- one accepted output;
- one set of Messages;
- event records correlate correctly;
- raw response is deleted after policy permits.

## 175. Duplicate Response Tests

Duplicate payload under one turn creates no duplicate accepted output, Message, event, or Dice Roll.

## 176. Late Response Tests

A response arriving after another attempt was accepted becomes Superseded.

## 177. Stale Context Tests

Context version change before acceptance rejects the output.

## 178. Crash Recovery Tests

Inject failure:

- before request;
- after attempt creation;
- after request send;
- after response receipt;
- after staging;
- during validation;
- during repair;
- after acceptance commit;
- before cleanup.

## 179. Backup Tests

Tests prove unresolved provider recovery records survive backup and restore.

## 180. Export Tests

Tests prove portable Campaign export excludes Provider Attempts and raw staging by default.

## 181. Adapter Contract Tests

Every provider adapter must pass the same provider-neutral contract suite.

## 182. Cross-Provider Test

Two synthetic adapters must produce equivalent canonical `NarratorTurnOutput` handling.

## 183. Fallback Tests

Only required when fallback is implemented.

Tests must prove:

- new attempt;
- same canonical contract;
- no automatic merge;
- no duplicate acceptance;
- explicit provider policy.

## 184. Privacy Tests

Prompts, raw responses, credentials, and private Campaign canaries must not enter Stable logs or indefinite retention.

## 185. Architecture Tests

Architecture tests must reject:

- provider adapter writing repositories;
- provider adapter accessing DbContext;
- provider-native response type in Application contracts;
- provider thread required for recovery;
- credential field in ProviderAttempt;
- raw response retained indefinitely;
- one ProviderAttempt reused for several external calls;
- transport success treated as accepted output;
- several accepted attempts for one Narrative Turn;
- provider output generating Dice values;
- automatic multi-provider response merge;
- provider call inside database transaction.

## 186. Prohibited Patterns

### 186.1 One Provider Attempt for All Retries

Create one attempt per call.

### 186.2 Provider Thread as Chronicle Memory

Use local authoritative context.

### 186.3 Transport Success Equals Narrative Acceptance

Validate the Chronicle contract.

### 186.4 Infinite Automatic Retry

Bound retry and expose recovery.

### 186.5 Retry Authentication Failure Repeatedly

Wait for configuration.

### 186.6 Keep Raw Responses Forever

Use bounded staging.

### 186.7 Put Credentials in Operational Records

Resolve through Credential Manager.

### 186.8 Accept Late Provider Output

Reject or supersede it.

### 186.9 Merge Responses from Several Providers

Select at most one accepted output.

### 186.10 Provider Adapter Applies Events

Route through Chronicle Application.

## 187. Alternatives Considered

### No Durable Provider Attempt

Rejected because restart and duplicate response handling become unsafe.

### Store Only Provider Request ID

Rejected because provider-native identifiers do not represent Chronicle lifecycle or acceptance.

### Retain Every Prompt and Response Forever

Rejected because of privacy and storage risk.

### Retry the Same Attempt Record

Rejected because each external call needs separate evidence.

### Provider Thread as Primary Context

Rejected because it makes Chronicle provider-dependent and nonportable.

### Automatic Provider Voting

Rejected because it adds cost, latency, merge ambiguity, and duplicate-authority risk.

### No Response Staging

Rejected because crash after response receipt would force avoidable external retry and make unknown outcomes harder to inspect.

## 188. Consequences

### Positive

- durable provider recovery;
- clear attempt history;
- bounded retry;
- explicit repair;
- one canonical accepted output;
- provider replacement remains possible;
- remote thread loss is harmless;
- raw-content retention is minimized;
- duplicate and late responses are controlled;
- credentials stay outside operational records.

### Negative

- more workflow records;
- staging cleanup is required;
- adapter implementations need strict contract tests;
- retry and repair classification add complexity;
- some provider responses may be discarded after context becomes stale;
- future fallback requires additional policy.

## 189. Risks

### Staged Raw Content Leaks Private Data

Mitigation:

- restricted permissions;
- short expiry;
- no credentials;
- explicit diagnostics;
- cleanup tests.

### Timeout Produces Two Valid Responses

Mitigation:

- one accepted-output uniqueness constraint;
- stale and late response checks;
- attempt correlation.

### Retry Cost Grows Unexpectedly

Mitigation:

- small attempt limits;
- usage metadata;
- explicit user retry after exhaustion.

### Provider-Specific Behavior Leaks into Application

Mitigation:

- provider-neutral contracts;
- adapter contract tests;
- architecture tests.

### Recovery Depends on Provider Availability

Mitigation:

- local context and durable records;
- safe retry or user recovery;
- no provider-native authority.

## 190. Technology Spike

Before acceptance, implement:

1. ProviderAttempt persistence;
2. canonical Provider Attempt state enum;
3. attempt-number uniqueness;
4. provider-neutral adapter interface;
5. Work Item integration;
6. bounded timeout policy;
7. retry classification;
8. exponential backoff with jitter;
9. repair flow;
10. NarrativeResponseStaging;
11. staging expiry and cleanup;
12. canonical response hashing;
13. one accepted-output constraint;
14. stale-context rejection;
15. late-response supersession;
16. crash recovery;
17. credential exclusion;
18. Stable log redaction;
19. adapter contract test kit;
20. privacy tests;
21. architecture tests.

## 191. Spike Acceptance

The spike passes when:

- one Narrative Turn creates one durable Provider Attempt;
- a retry creates a second distinct attempt;
- transient failure retries within bounds;
- authentication failure waits for configuration;
- malformed output receives one bounded repair;
- a valid repaired response is accepted once;
- crash after response receipt resumes from staging;
- late response after acceptance is superseded;
- duplicate response creates no duplicate Message or event;
- provider thread deletion does not prevent context reconstruction;
- raw staging expires and is deleted;
- credentials never enter persistence or logs;
- two synthetic adapters pass the same canonical contract tests.

## 192. Definition of Compliance

An implementation complies when:

- each external provider call has one durable ProviderAttempt;
- every attempt belongs to one NarrativeTurnId;
- retries create new attempts;
- transport retry and output repair remain distinct;
- retries are bounded and classified;
- provider calls occur outside transactions;
- responses may be staged before validation;
- raw staged content is transient;
- one accepted output exists per Narrative Turn;
- stale, duplicate, late, and superseded responses are rejected safely;
- provider-native thread state is nonauthoritative;
- local Chronicle records can reconstruct required context;
- credentials are never persisted in provider records;
- accepted Messages and event effects remain provider-neutral;
- no provider behavior is specific to Werewolf.

## 193. Review Triggers

Review this ADR if:

- provider fallback becomes an MVP requirement;
- local on-device models become official;
- streaming output becomes player-visible before validation;
- provider-native tools become part of the canonical transport;
- multiple providers are invoked concurrently;
- server-hosted provider orchestration is introduced;
- retention requirements change materially;
- encrypted response staging becomes required;
- offline narrative generation becomes mandatory.

## 194. Deferred Decisions

Later decisions may define:

- official provider fallback;
- local model provider;
- encrypted transient staging;
- provisional streaming;
- multi-provider comparison;
- provider cost budgets;
- provider health scoring;
- enterprise provider policy;
- server-side provider gateway;
- longer diagnostic retention.

## 195. Final Decision

Chronicle will persist one Provider Attempt for every external provider call.

Retries will create new attempts.

Repairs will correct proposals under the same Narrative Turn.

Raw responses may exist briefly in protected staging.

Accepted output will exist once.

Provider threads may disappear.

Credentials may change.

Providers may be replaced.

Campaign truth will remain owned by Chronicle.
