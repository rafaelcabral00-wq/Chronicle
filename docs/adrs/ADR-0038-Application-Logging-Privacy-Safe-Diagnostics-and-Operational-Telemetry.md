---
id: ADR-0038
title: Application Logging, Privacy-Safe Diagnostics, and Operational Telemetry
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
  - ADR-0006
  - ADR-0007
  - ADR-0008
  - ADR-0011
  - ADR-0012
  - ADR-0013
  - ADR-0014
  - ADR-0017
  - ADR-0019
  - ADR-0020
  - ADR-0021
  - ADR-0022
  - ADR-0023
  - ADR-0025
  - ADR-0026
  - ADR-0034
  - ADR-0035
  - ADR-0036
  - ADR-0037
  - RFC-0018
  - RFC-0020
  - RFC-0033
  - RFC-0034
  - RFC-0036
  - RFC-0038
  - RFC-0039
  - RFC-0040
  - RFC-0041
  - RFC-0042
---

> **"Diagnostics must explain what Chronicle did without exposing what the Campaign lived."**

# Application Logging, Privacy-Safe Diagnostics, and Operational Telemetry

## 1. Status

**Proposed**

This ADR defines Chronicle's logging, diagnostic events, tracing, metrics, local log retention, privacy controls, redaction, diagnostic bundles, user-facing reference codes, crash diagnostics, Development behavior, and telemetry policy.

The decision is:

- use structured logging rather than free-form diagnostic text;
- centralize log configuration, enrichment, filtering, redaction, storage, and retention;
- classify every log field before it is allowed into Stable diagnostics;
- exclude narrative content, Character biographies, full Character Sheets, Memories, Character Knowledge, provider prompts, provider responses, Rule Knowledge text, credentials, authorization headers, and raw secret values by default;
- use identifiers, semantic operation keys, versions, counts, durations, state transitions, result codes, and fingerprints as the normal diagnostic vocabulary;
- correlate user actions, Application commands, Operation Records, Work Items, provider attempts, Rule Operations, Dice Rolls, imports, backups, migrations, and recovery through stable correlation fields;
- keep logs local by default;
- send no operational telemetry to Chronicle maintainers in the MVP;
- require explicit future consent and a separate ADR before remote telemetry or crash upload is introduced;
- support privacy-safe local diagnostic bundles created only through explicit user action;
- make diagnostic bundles inspectable before sharing;
- use bounded retention and file-size limits;
- keep Development diagnostics richer but still secret-safe;
- prohibit full prompt and raw provider-response logging in Stable;
- provide safe user-facing reference codes for errors and recovery;
- distinguish logs, audit evidence, metrics, and authoritative Domain history;
- never use logs as the source of truth;
- treat any secret or private Campaign-content leakage into logs as a release-blocking defect.

The decision becomes **Accepted** after a diagnostics spike proves:

- structured logs with correlation;
- bounded local file retention;
- secret and authorization-header redaction;
- no Campaign prose leakage;
- provider-attempt observability without prompts;
- Work Item and OperationId tracing;
- Dice state transition diagnostics;
- migration and restore diagnostics;
- diagnostic-bundle generation and inspection;
- Stable and Development policy separation;
- crash-safe flushing;
- deterministic tests scanning logs and bundles for synthetic private canaries.

## 2. Context

Chronicle contains long-lived private Campaign data and executes complex durable workflows.

Failures may occur in:

- startup;
- database migration;
- package loading;
- provider configuration;
- provider transport;
- Prompt Construction;
- structured-output validation;
- Dice generation;
- Rule Operation resolution;
- Session finalization;
- import and export;
- backup and restore;
- Work Item recovery;
- filesystem operations;
- application shutdown.

Without diagnostics, users and maintainers cannot understand:

- which operation failed;
- whether data was preserved;
- whether a retry is safe;
- which provider attempt was involved;
- which package version was active;
- whether a migration committed;
- which Work Item remains pending;
- whether a Roll was already generated;
- whether startup entered Safe Mode.

However, ordinary logs can easily leak:

- player messages;
- Narrator prose;
- private Character history;
- Memories;
- Character Knowledge;
- safety boundaries;
- sourcebook excerpts;
- prompts;
- provider responses;
- API keys;
- filesystem paths;
- local usernames;
- endpoint query secrets.

Chronicle needs useful diagnostics without turning logs into a second copy of the Campaign.

## 3. Decision Drivers

The design prioritizes:

1. privacy;
2. recoverability;
3. structured correlation;
4. bounded local storage;
5. actionable errors;
6. secret exclusion;
7. testability;
8. local-first operation;
9. no hidden telemetry;
10. supportability;
11. honest crash diagnostics;
12. separation from authoritative history.

## 4. Decision Summary

Chronicle will use:

```text
Logging
    structured
    local
    bounded
    redacted

Correlation
    OperationId
    WorkItemId
    CampaignId
    NarrativeTurnId
    DiceRollId
    ProviderAttemptId
    BackupId
    ImportId

Stable Content
    identifiers
    versions
    counts
    durations
    state transitions
    result codes
    fingerprints

Stable Exclusions
    credentials
    prompts
    responses
    narrative prose
    Character private content
    Rule Knowledge text
    raw local paths

Telemetry
    none sent remotely in MVP

Diagnostic Bundle
    explicit user action
    scrubbed
    inspectable
```

## 5. Diagnostic Categories

Chronicle distinguishes:

```text
Operational Logs
Security Logs
Recovery Logs
Performance Metrics
Audit Evidence
Authoritative Domain History
User-Facing Error Reports
```

## 6. Operational Logs

Operational logs explain system execution.

They are nonauthoritative and disposable.

## 7. Security Logs

Security logs record safe events such as:

- credential missing;
- invalid secret reference;
- blocked unsafe archive;
- blocked provider disclosure;
- redaction failure;
- prohibited package content.

They do not contain the secret or blocked private content.

## 8. Recovery Logs

Recovery logs explain:

- interrupted Work Items;
- unknown commits;
- Safe Mode decisions;
- restore rollback;
- migration checkpoints;
- orphaned staging cleanup.

## 9. Performance Metrics

Performance metrics summarize durations, counts, sizes, and failures.

They contain no Campaign narrative.

## 10. Audit Evidence

Audit evidence belongs in authoritative Chronicle records such as:

- Dice evidence;
- progression ledger;
- Character field history;
- Preference history;
- migration history;
- Operation Records.

Logs do not replace these records.

## 11. Authoritative Domain History

Campaign history remains in Domain storage, not log files.

## 12. User-Facing Error Report

A user-facing error report contains:

```text
Safe Title
Safe Explanation
Error Code
Reference Code
Data Preservation State
Recovery Actions
Timestamp
```

## 13. Structured Logging

Chronicle uses structured event properties.

Example:

```text
EventName = "dice.roll.state-transition"
DiceRollId = "..."
FromState = "RawValuesCommitted"
ToState = "Resolving"
OperationId = "..."
```

## 14. No String Concatenation as Primary Pattern

Primary logging should avoid:

```text
"Failed to process " + object
```

Use event templates and allowlisted properties.

## 15. Event Name

Every significant event SHOULD have a stable semantic event name.

## 16. Event Name Examples

```text
application.startup.started
application.startup.safe-mode
database.migration.started
database.migration.completed
work-item.claimed
work-item.failed
provider.attempt.started
provider.attempt.rate-limited
narrative.turn.committed
dice.roll.state-transition
backup.validation.failed
restore.rollback.started
package.quarantined
```

## 17. Event Schema

Each event has a defined field allowlist.

## 18. Event Version

Events used by tooling SHOULD include an event schema version.

## 19. Log Levels

Recommended levels:

```text
Trace
Debug
Information
Warning
Error
Critical
```

## 20. Stable Level Policy

Stable Release defaults to:

```text
Information
```

with category overrides for noisy libraries.

## 21. Development Level Policy

Development may use Debug or Trace selectively.

Secret and privacy exclusions still apply.

## 22. Trace Use

Trace is intended for:

- test environments;
- local debugging;
- short explicitly enabled sessions.

## 23. Critical Event

Critical is reserved for conditions such as:

- authoritative storage cannot open;
- restore publication outcome uncertain;
- migration rollback unavailable;
- secret detected in generated artifact;
- repeated corruption;
- startup cannot establish safe operation.

## 24. Logging Boundary

All projects may emit structured events through Chronicle's logging abstraction.

## 25. Infrastructure Enrichment

Infrastructure adds:

- process version;
- release channel;
- operating-system class;
- session identifier;
- thread or task metadata where useful.

## 26. No Environment Dump

Chronicle never logs the full process environment.

## 27. No Configuration Dump

Chronicle never logs the complete configuration object.

## 28. Correlation Fields

Recommended fields:

```text
ApplicationSessionId
OperationId
WorkItemId
CampaignId
SessionId
ActId
SceneId
CharacterId
NarrativeTurnId
ProviderAttemptId
DiceRollId
BackupId
ExportId
ImportId
MigrationId
```

Only applicable fields are present.

## 29. Correlation Scope

Application commands establish an OperationId logging scope.

## 30. Work Item Scope

Workers add:

```text
WorkItemId
WorkType
AttemptNumber
LeaseOwnerShortId
```

## 31. Provider Attempt Scope

Provider adapters add:

```text
ProviderProfileId
ProviderFamilyKey
ModelKey
ProviderAttemptId
```

No credential data is included.

## 32. Rule Operation Scope

Rule Operations add:

```text
RuleSetPackageId
PackageVersion
OperationKey
InputFingerprint
```

## 33. Database Scope

Database events MAY include:

- operation category;
- migration ID;
- transaction duration;
- retry count;
- concurrency result.

They SHOULD NOT include SQL parameter values in Stable.

## 34. SQL Logging

Raw SQL logging is disabled in Stable by default.

## 35. Sensitive Data Logging

EF Core sensitive-data logging is disabled in Stable.

## 36. Development SQL Logging

Development MAY log SQL text for synthetic data.

Parameter values remain disabled unless explicitly enabled in an isolated environment.

## 37. Filesystem Paths

Stable logs SHOULD avoid full user paths.

## 38. Safe Path Representation

Use classifications such as:

```text
ManagedDataDirectory
ManagedBackupDirectory
UserSelectedExternalFile
PackageDirectory
StagingDirectory
```

or sanitized relative paths.

## 39. Local Username

Local usernames are not logged.

## 40. Machine Name

Machine names are not logged by default.

## 41. Campaign Content Exclusions

Stable logs MUST NOT contain:

- Campaign title where avoidable;
- Character names where avoidable;
- player-message text;
- Narrator prose;
- Scene descriptions;
- Character biographies;
- goals or fears;
- Memories;
- Character Knowledge;
- relationship notes;
- safety-boundary explanations;
- progression reasons containing free text.

## 42. Identifier Preference

Use stable IDs and semantic keys rather than display names.

## 43. Provider Content Exclusions

Stable logs MUST NOT contain:

- full prompt package;
- raw system instructions;
- user prompt text;
- raw provider response;
- structured Narrator prose;
- tool-call argument content when private;
- Rule Knowledge excerpts.

## 44. Provider Metadata Allowed

Stable logs MAY contain:

- provider family;
- model key;
- attempt number;
- request-size estimate;
- output-size estimate;
- token counts;
- finish reason;
- latency;
- context fingerprint;
- contract version;
- safe error category.

## 45. Credential Exclusions

Logs MUST NOT contain:

- API keys;
- bearer tokens;
- authorization headers;
- client secrets;
- refresh tokens;
- secret-store payloads;
- credentials embedded in URLs.

## 46. Secret Reference Logging

A short opaque Secret Reference ID MAY be logged only for credential workflow diagnostics.

## 47. Redaction Pipeline

Chronicle uses:

```text
Prevention
Allowlisting
Structured Redaction
Artifact Scanning
```

## 48. Prevention

Sensitive values should never be passed to logging calls.

## 49. Allowlisting

Known event schemas define permitted properties.

## 50. Structured Redaction

A redaction service sanitizes:

- headers;
- endpoint query parameters;
- provider errors;
- file paths;
- known sensitive fields.

## 51. Artifact Scanning

Tests scan logs and bundles for synthetic canary values.

## 52. Redaction Marker

Redacted values use a stable marker:

```text
[REDACTED]
```

## 53. Redaction Failure

A detected secret in Stable output is a release-blocking defect.

## 54. Exceptions

Exceptions are logged through safe mapping.

## 55. Expected Errors

Expected validation and conflict outcomes do not require full exception logging.

## 56. Unexpected Errors

Unexpected errors include:

- safe exception type;
- safe message after sanitization;
- reference code;
- stack trace according to environment;
- correlation fields.

## 57. Stack Traces

Stable local logs MAY include stack traces for Chronicle code when they contain no known private payload.

## 58. Provider Stack Traces

Provider SDK exception details require adapter sanitization.

## 59. User-Facing Stack Traces

Stack traces are not shown in ordinary UI.

## 60. Reference Code

Every significant failure receives a generated safe reference code.

## 61. Reference Code Purpose

The code links:

- UI error;
- local log event;
- Operation Record;
- diagnostic bundle.

## 62. Reference Code Content

The code must not encode private content.

## 63. Data Preservation State

Failure events SHOULD include a safe preservation state.

Examples:

```text
AuthoritativeDataUnchanged
RawDiceCommitted
NarrativeMessageCommitted
BackupNotPublished
RestoreRollbackAvailable
OutcomeUnknownRequiresInspection
```

## 64. Recovery Action

Failure events SHOULD include a safe recovery-action key.

## 65. Log Sink

The official desktop MVP writes structured local files.

## 66. File Format

Recommended format:

```text
JSON Lines
```

## 67. Human Readability

A local diagnostic viewer may format structured events for humans.

## 68. Console Sink

Development may also use console output.

## 69. Debug Sink

Development may use debugger output.

## 70. Stable Console

Stable desktop operation does not depend on a visible console.

## 71. Log Directory

Logs reside in the Chronicle-managed data directory under a dedicated subdirectory.

## 72. Atomic File Behavior

Log sink behavior must tolerate process interruption without corrupting authoritative storage.

## 73. Log Failure

Failure to write logs must not corrupt Campaign data.

## 74. Logging Backpressure

Logging must not block critical Application operations indefinitely.

## 75. Bounded Queue

An asynchronous log sink MAY use a bounded queue.

## 76. Queue Overflow

When the queue overflows:

- lower-priority events may be dropped;
- a safe dropped-event counter is recorded when possible;
- Critical recovery events should use a higher-reliability path.

## 77. No Unbounded Memory Buffer

Unbounded in-memory logging queues are prohibited.

## 78. Flush Policy

Chronicle SHOULD flush:

- on Warning or higher according to sink policy;
- during orderly shutdown;
- before controlled restart;
- before restore publication where practical;
- after Critical recovery events.

## 79. Crash Limitation

Chronicle cannot guarantee every final log event survives abrupt process termination.

## 80. Log Rotation

Logs use bounded rotation.

## 81. Retention Dimensions

Retention MAY use:

```text
MaximumFileSize
MaximumFileCount
MaximumAge
MaximumTotalSize
```

## 82. Default Retention

Exact defaults are implementation policy, but must remain bounded.

## 83. No Infinite Retention

Unbounded local log retention is prohibited.

## 84. Retention Ordering

Retention uses Chronicle-controlled timestamps and sequence, not only filesystem modification time.

## 85. Log Deletion

Log retention may delete old log files because logs are nonauthoritative.

## 86. Active Log Protection

The active file is not deleted by retention.

## 87. User Clear Logs

The user may clear local logs through an explicit command.

## 88. Clear Logs Claim

Clearing logs is ordinary deletion, not forensic secure erasure.

## 89. Telemetry Policy

Chronicle sends no operational telemetry to project maintainers in the MVP.

## 90. No Background Analytics

The MVP does not send:

- usage analytics;
- feature counts;
- Campaign metrics;
- provider usage;
- crash reports;
- device identifiers;
- installation identifiers.

## 91. Provider Telemetry

Provider services may have their own request and retention policies.

Chronicle must describe those separately from Chronicle telemetry.

## 92. Future Telemetry

Any future remote telemetry requires:

- separate ADR;
- explicit purpose;
- user consent;
- data minimization;
- disclosure;
- disablement;
- retention policy;
- transport security;
- schema inventory.

## 93. Metrics

Chronicle may collect in-process or local metrics for diagnostics.

## 94. Local Metrics

Examples:

```text
OperationDuration
WorkItemQueueDepth
ProviderAttemptCount
ProviderLatency
PromptBudgetEstimate
DiceResolutionDuration
BackupDuration
MigrationDuration
ErrorCount
```

## 95. Metric Labels

Metric labels must remain low-cardinality.

## 96. Prohibited Metric Labels

Do not use:

- Campaign title;
- Character name;
- raw error message;
- prompt text;
- full IDs when aggregation does not need them;
- arbitrary user text.

## 97. Metric Persistence

The MVP may calculate metrics from logs rather than maintain a separate time-series database.

## 98. Remote Metrics

No remote metrics export exists in MVP.

## 99. Tracing

Chronicle uses local correlation scopes rather than requiring a distributed tracing backend.

## 100. Trace Identity

A workflow trace may use:

```text
TraceId
OperationId
```

## 101. Span Concepts

Logical spans include:

```text
Application Command
Database Transaction
Work Item Attempt
Provider Attempt
Rule Operation
Backup Stage
Restore Stage
Migration Stage
```

## 102. Provider Trace Headers

Chronicle does not send internal trace context to a provider unless the adapter and privacy policy explicitly allow it.

## 103. Diagnostic Viewer

Chronicle SHOULD include a safe local diagnostics screen.

## 104. Diagnostic Viewer Content

It MAY show:

- application version;
- release channel;
- storage health;
- package health;
- provider-profile health;
- pending Work Items;
- recent safe errors;
- log location;
- bundle creation action.

## 105. Diagnostic Viewer Exclusions

It does not display:

- credentials;
- raw prompts;
- raw provider responses;
- private Campaign prose by default.

## 106. Diagnostic Bundle

A diagnostic bundle is an explicitly generated archive intended for user inspection and optional sharing.

## 107. Bundle Command

Recommended command:

```text
CreateDiagnosticBundleCommand
```

## 108. Bundle Is Not Automatic

Chronicle does not create or upload a bundle silently.

## 109. Bundle Contents

A bundle MAY include:

```text
manifest
application version
release channel
safe operating-system metadata
database schema version
package inventory
provider profile capability and health metadata
safe recent logs
Work Item summaries
Operation failure summaries
migration status
backup and restore status
redaction report
```

## 110. Bundle Exclusions

A bundle MUST exclude:

- provider credentials;
- authorization headers;
- full prompts;
- raw provider responses;
- Campaign transcript;
- Character Sheets;
- Memories;
- Character Knowledge;
- Rule Knowledge text;
- raw database;
- backups;
- full local paths;
- environment variables;
- memory dumps.

## 111. Optional Private Content

Including private Campaign content in a support artifact is deferred and would require per-content explicit consent.

## 112. Bundle Manifest

Recommended fields:

```text
DiagnosticBundleId
CreatedAtUtc
ApplicationVersion
BundleContractVersion
IncludedCategories
ExcludedCategories
RedactionPolicyVersion
ContentHashes
```

## 113. Bundle Staging

Bundle creation follows safe staging and atomic publication.

## 114. Bundle Validation

Before publication, Chronicle:

- reopens archive;
- validates manifest;
- verifies hashes;
- scans for synthetic and known secret patterns;
- confirms exclusions.

## 115. User Inspection

The UI should show the bundle content categories before final creation or sharing.

## 116. No Automatic Upload

The MVP does not upload diagnostic bundles.

## 117. Bundle Retention

User-selected bundles are not automatically deleted unless stored in a managed diagnostics directory under explicit policy.

## 118. Support Reference

The bundle may include safe reference codes selected by the user.

## 119. Crash Handling

Chronicle SHOULD register safe top-level exception handling.

## 120. Crash Event

A crash event MAY record:

- exception type;
- sanitized message;
- reference code;
- active OperationId;
- active WorkItemId;
- application version;
- last safe startup state.

## 121. Crash Recovery

On next startup, Chronicle may show:

- a safe crash notice;
- data-preservation status when known;
- Safe Mode recommendation;
- diagnostic bundle action.

## 122. Crash Dump

Chronicle does not create or upload full memory dumps by default.

## 123. OS Crash Artifacts

Users should be warned that OS-generated crash artifacts may contain sensitive memory.

## 124. Last-Run Marker

Chronicle MAY persist a small safe startup/shutdown marker.

## 125. Clean Shutdown Marker

A clean shutdown records:

```text
ApplicationSessionId
CompletedAtUtc
```

## 126. Unclean Shutdown

Missing clean shutdown triggers recovery inspection, not an assumption of corruption.

## 127. Development Diagnostics

Development may enable:

- richer stack traces;
- fake provider payload capture;
- SQL command text;
- test fixture context;
- local prompt inspection with synthetic data.

## 128. Development Still Redacts Secrets

No environment permits credential logging.

## 129. Stable Diagnostics

Stable emphasizes:

- identifiers;
- versions;
- counts;
- state;
- duration;
- safe error classification.

## 130. Diagnostic Feature Flags

Diagnostic flags are explicit and nonsecret.

## 131. Sensitive Flag Warning

Any flag that increases content visibility must:

- be unavailable or strongly guarded in Stable;
- show a warning;
- expire or reset;
- never include credentials.

## 132. Raw Prompt Capture

Raw prompt capture is prohibited in Stable MVP.

## 133. Raw Response Capture

Indefinite raw response capture is prohibited in Stable MVP.

## 134. Temporary Provider Response Staging

Temporary response staging for validation and recovery is governed by ADR-0034 and is not ordinary logging.

## 135. Staging Retention

Staged response data is cleaned after completion or bounded recovery retention.

## 136. Audit Event Separation

Security-sensitive configuration changes MAY create safe audit records separate from logs.

## 137. Safe Audit Examples

- provider profile created;
- credential reference changed;
- package installed;
- package quarantined;
- backup restored;
- migration completed;
- logs cleared.

## 138. Audit Record Content

Audit records use IDs, operation keys, timestamps, and safe outcomes.

## 139. No User Behavior Analytics

Audit records are operational evidence, not behavioral analytics.

## 140. Event Sampling

Stable local logs MAY sample high-frequency noncritical events.

## 141. Never Sample Critical Recovery State

Do not sample:

- migration failure;
- restore publication;
- raw Dice commit;
- Operation commit uncertainty;
- secret leak detection;
- package quarantine;
- database integrity failure.

## 142. Rate-Limited Logging

Repeated identical failures may be rate-limited.

## 143. Suppression Summary

When events are suppressed, Chronicle logs a safe count summary.

## 144. Log Injection

User-controlled text is never used as an event template.

## 145. Newline Safety

Structured sinks encode values safely.

## 146. Viewer Rendering

Diagnostic viewers render text safely and do not execute HTML or links.

## 147. Error Model

Recommended errors:

```text
diagnostics.log-directory-unavailable
diagnostics.log-write-failed
diagnostics.log-queue-overflow
diagnostics.redaction-failed
diagnostics.bundle-create-failed
diagnostics.bundle-validation-failed
diagnostics.bundle-secret-detected
diagnostics.bundle-publication-failed
diagnostics.retention-failed
diagnostics.recovery-required
```

## 148. Data Preservation State

Diagnostic failures SHOULD state:

```text
AuthoritativeDataUnchanged
LoggingDegraded
BundleNotPublished
ExistingLogsPreserved
RecoveryRequired
```

## 149. Logging Failure Policy

Logging failure does not fail an otherwise valid Domain operation unless:

- the operation is a security-critical workflow requiring mandatory audit evidence;
- policy explicitly requires a durable safety record;
- continuing would make recovery unsafe.

## 150. Mandatory Evidence Is Not Ordinary Log

When durable evidence is required, it belongs in authoritative storage or Operation Records.

## 151. Test Strategy

The implementation requires:

```text
Structured Event Tests
Redaction Tests
Canary Tests
Retention Tests
Concurrency Tests
Crash Tests
Diagnostic Bundle Tests
Provider Diagnostics Tests
Database Diagnostics Tests
Architecture Tests
Performance Tests
```

## 152. Structured Event Tests

Tests MUST verify:

- stable event name;
- allowed properties;
- event schema version;
- correlation fields;
- level;
- no arbitrary object serialization.

## 153. Redaction Tests

Tests MUST cover:

- authorization header;
- API key;
- bearer token;
- endpoint query secret;
- embedded URL credential;
- provider error echo;
- local path;
- username;
- private free text.

## 154. Campaign Canary Tests

Synthetic Campaign canaries placed in:

- player messages;
- Narrator prose;
- Character biography;
- Memory;
- Character Knowledge;
- safety Preference;
- Rule Knowledge

must not appear in Stable logs or bundles.

## 155. Secret Canary Tests

Synthetic secrets must not appear in:

- logs;
- exception output;
- bundle;
- UI error;
- crash marker;
- metric labels.

## 156. Retention Tests

Tests MUST cover:

- file size rotation;
- file count;
- age;
- total size;
- active file preservation;
- deletion failure;
- user clear action.

## 157. Concurrency Tests

Tests MUST prove:

- parallel events remain valid JSON lines;
- scopes do not leak between operations;
- bounded queue behavior;
- no deadlock during shutdown.

## 158. Crash Tests

Tests SHOULD terminate the process:

- during log write;
- during rotation;
- during bundle creation;
- after Critical event;
- before clean-shutdown marker.

## 159. Bundle Tests

Tests MUST cover:

- expected safe inventory;
- excluded private categories;
- manifest;
- checksums;
- staging;
- atomic publication;
- secret scan;
- user inspection summary.

## 160. Provider Diagnostic Tests

Tests MUST prove provider errors retain useful category, model, latency, attempt, and request reference without prompt, response, or credential leakage.

## 161. Work Item Tests

Tests MUST prove one workflow can be traced through:

```text
OperationId
WorkItemId
AttemptNumber
Final Outcome
```

## 162. Dice Tests

Tests MUST prove Roll state transitions are diagnosable without logging raw private narrative.

Raw Dice values MAY remain in authoritative Roll evidence and need not be duplicated in ordinary logs.

## 163. Migration Tests

Tests MUST prove migration ID, stage, checkpoint, duration, and outcome are visible without schema-data leakage.

## 164. Restore Tests

Tests MUST prove restore publication and rollback are traceable with safe paths and IDs.

## 165. Required Test Cases

Tests MUST cover:

- normal startup;
- Safe Mode startup;
- command success;
- validation failure;
- concurrency conflict;
- Work Item retry;
- provider timeout;
- provider rate limit;
- malformed provider output;
- Dice resolution;
- backup failure;
- restore rollback;
- migration failure;
- package quarantine;
- credential missing;
- log rotation;
- diagnostic bundle;
- no remote telemetry.

## 166. Architecture Tests

Architecture tests MUST reject:

- `Console.WriteLine` as production diagnostics;
- raw prompt logging;
- raw response logging;
- credential logging;
- full entity serialization;
- EF sensitive-data logging in Stable;
- full configuration dump;
- environment dump;
- unbounded logging queue;
- remote telemetry client in MVP composition;
- diagnostic bundle including database or Campaign content.

## 167. Prohibited Patterns

### 167.1 Log the Whole Request

Log safe metadata and fingerprint.

### 167.2 Log the Whole Entity

Use identifiers, versions, counts, and state.

### 167.3 Use Logs as Audit Truth

Persist authoritative evidence separately.

### 167.4 Enable Sensitive SQL Logging in Stable

Keep parameter values disabled.

### 167.5 Send Telemetry Without Explicit Decision

MVP telemetry remains local.

### 167.6 Upload Crash Reports Automatically

User action and a future ADR are required.

### 167.7 Put User Text in Event Templates

Use structured safe properties only when allowed.

### 167.8 Retain Logs Forever

Use bounded retention.

### 167.9 Hide Data Preservation State

Every serious error explains what changed or remained intact.

### 167.10 Treat Redaction as the Only Protection

Prevent private values from entering diagnostics first.

## 168. Alternatives Considered

### Plain Text Logs

Rejected because structured filtering, redaction, correlation, and bundle tooling are weaker.

### No Logs for Privacy

Rejected because recovery and support would become unsafe and opaque.

### Log Full Prompts Locally

Rejected as a Stable default because prompts contain substantial private Campaign data.

### Automatic Cloud Telemetry

Rejected for MVP because it conflicts with local-first expectations and requires consent, retention, transport, and governance decisions.

### Database-Backed Log Store

Rejected for ordinary logs because it competes with authoritative storage and complicates recovery.

### Depend Entirely on OS Event Logs

Insufficient for portable local diagnostics and structured workflow correlation.

## 169. Consequences

### Positive

- failures are traceable without duplicating Campaign content;
- user-facing errors become actionable;
- Work Items and provider attempts correlate cleanly;
- local-first privacy remains clear;
- diagnostic bundles are safe and inspectable;
- no hidden project telemetry exists;
- secret leakage becomes testable;
- logs remain bounded and disposable.

### Negative

- event schemas and allowlists require maintenance;
- some bugs are harder to diagnose without raw content;
- redaction and bundle scanning add implementation work;
- local logs still consume disk space;
- support may require user-created bundles;
- Development and Stable policies require separate testing.

## 170. Risks

### Important Diagnostic Detail Is Omitted

Mitigation:

- stable identifiers;
- fingerprints;
- versions;
- state transitions;
- explicit Development repro workflow;
- synthetic fixtures.

### Private Content Enters Logs Accidentally

Mitigation:

- event allowlists;
- architecture tests;
- canary scanning;
- redaction;
- release-blocking policy.

### Logging Failure Hides Recovery State

Mitigation:

- authoritative Operation Records;
- Safe Mode inspection;
- user-facing preservation state;
- best-effort Critical flush.

### Log Volume Becomes Excessive

Mitigation:

- bounded retention;
- level policy;
- sampling;
- rate limiting;
- metrics.

### Diagnostic Bundle Is Shared Without Inspection

Mitigation:

- explicit creation;
- content-category preview;
- local-only generation;
- no automatic upload.

## 171. Technology Spike

Before acceptance, implement:

1. structured event conventions;
2. central logging configuration;
3. correlation scopes;
4. JSON Lines local sink;
5. bounded queue;
6. file rotation and retention;
7. redaction service;
8. provider error sanitizer;
9. user-facing reference-code generator;
10. Safe Mode diagnostic viewer;
11. diagnostic-bundle contract;
12. bundle validator and secret scanner;
13. clean-shutdown marker;
14. Stable and Development policy tests;
15. synthetic Campaign and secret canary suite.

## 172. Spike Acceptance

The spike passes when:

- one Application command can be traced through OperationId and Work Item attempts;
- provider failures remain diagnosable without prompt or response content;
- Dice, backup, migration, and restore stages emit useful safe events;
- local log storage rotates and stays bounded;
- Stable logs contain none of the synthetic Campaign or secret canaries;
- a diagnostic bundle contains the declared safe inventory only;
- bundle validation blocks publication when a canary is detected;
- no remote telemetry or crash upload occurs;
- Development may enable richer diagnostics without weakening secret protections;
- logging degradation never corrupts authoritative state.

## 173. Definition of Compliance

An implementation complies when:

- logs are structured, local, bounded, and centrally configured;
- event fields follow allowlisted schemas;
- correlation spans commands, Work Items, provider attempts, Rule Operations, Dice, backup, migration, and recovery;
- credentials, prompts, responses, Campaign prose, Character private data, and Rule Knowledge text remain excluded in Stable;
- EF sensitive-data logging and raw SQL parameters remain disabled in Stable;
- user-facing errors include safe codes, preservation state, and recovery actions;
- logs are not treated as authoritative history;
- no remote telemetry is sent in MVP;
- diagnostic bundles require explicit creation, validation, and inspection;
- retention, crash behavior, and logging degradation are bounded and tested;
- canary leakage is a release-blocking defect.

## 174. Review Triggers

This ADR must be reviewed if:

- remote telemetry is introduced;
- automatic crash upload is introduced;
- server hosting requires centralized observability;
- multiplayer requires cross-client tracing;
- encrypted diagnostic archives are introduced;
- public support tooling consumes logs automatically;
- a user opts into content-bearing diagnostics;
- OpenTelemetry export is introduced;
- regulatory logging requirements arise;
- Stable prompt capture is proposed.

## 175. Deferred Decisions

Later ADRs MAY define:

- exact logging library;
- exact retention defaults;
- OpenTelemetry integration;
- remote opt-in telemetry;
- automatic crash reporting;
- encrypted diagnostic bundles;
- content-bearing support bundles with granular consent;
- centralized server logs;
- public diagnostic schema;
- user-facing operation timeline;
- privacy-preserving aggregate analytics.

## 176. Final Decision

Chronicle will produce structured, correlated, privacy-safe local diagnostics.

Logs will describe operations, versions, states, counts, durations, and failures.

They will not become another transcript, another Character archive, or another credential store.

The MVP will send no telemetry to Chronicle maintainers.

Chronicle must be able to explain what went wrong without exposing what the player entrusted to the story.
