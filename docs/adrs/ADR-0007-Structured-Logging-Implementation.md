---
id: ADR-0007
title: Structured Logging Implementation
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
  - ADR-0006
  - RFC-0018
  - RFC-0019
  - RFC-0020
  - RFC-0026
  - RFC-0035
  - RFC-0036
  - RFC-0037
  - RFC-0040
  - RFC-0041
  - RFC-0042
---

> **"Chronicle should record enough to explain what the system did, never enough to betray what the player lived."**

# Structured Logging Implementation

## 1. Status

**Proposed**

This ADR selects Chronicle's initial structured logging implementation for the official desktop application.

The decision is:

- use `Microsoft.Extensions.Logging` as the application-wide abstraction;
- use **Serilog** as the initial concrete structured logging provider;
- write local rolling log files;
- emit structured JSON for machine-readable diagnostic logs;
- optionally emit a concise human-readable development console sink;
- maintain strict privacy and redaction boundaries;
- keep remote log export disabled in the MVP.

The decision becomes **Accepted** after a logging spike proves:

- structured event emission;
- OperationId and CorrelationId propagation;
- bounded rolling-file retention;
- no credentials or narrative payloads in default logs;
- exception capture without raw sensitive context;
- safe diagnostic-bundle export;
- log rotation under application restart;
- acceptable performance during long Campaigns.

## 2. Context

Chronicle must explain failures across:

- Desktop UI;
- Application commands and queries;
- provider calls;
- Rule Set operations;
- persistence;
- Work Items;
- migrations;
- backup and restore;
- imports and exports;
- Session finalization;
- Dice resolution.

At the same time, Chronicle handles sensitive information:

- player input;
- Character biographies;
- Campaign Secrets;
- hidden canonical truth;
- provider credentials;
- Rule Knowledge excerpts;
- local file paths;
- provider requests and responses.

A useful logging implementation must preserve diagnostic value without turning logs into an unsafe copy of the Campaign.

RFC-0035 defines security and privacy boundaries.

RFC-0036 defines observability architecture.

This ADR chooses the concrete MVP logging stack and implementation conventions.

## 3. Decision Drivers

The logging implementation prioritizes:

1. structured properties;
2. compatibility with `Microsoft.Extensions.Logging`;
3. local-first operation;
4. rolling-file support;
5. strong filtering and redaction;
6. low runtime overhead;
7. readable development diagnostics;
8. machine-readable support bundles;
9. mature .NET ecosystem;
10. future OpenTelemetry compatibility;
11. no remote service requirement;
12. simple Windows packaging.

## 4. Decision Summary

Chronicle will use:

```text
Logging Abstraction
    Microsoft.Extensions.Logging

Concrete Provider
    Serilog

Production Sink
    Rolling JSON files

Development Sink
    Human-readable console output
    Optional development rolling file

Storage Location
    Per-user Chronicle logs directory

Default Retention
    Bounded by age and total file count
    Exact values configured later

Remote Export
    Disabled in MVP

Sensitive Data Logging
    Disabled

Raw Prompt and Response Logging
    Disabled by default

Correlation
    OperationId
    CorrelationId
    CampaignId where safe
    SessionId where safe
    WorkItemId where safe
```

## 5. Why Serilog

Serilog is selected because it provides:

- mature structured logging for .NET;
- direct integration with `Microsoft.Extensions.Logging`;
- rich property-based events;
- rolling-file sinks;
- JSON formatting;
- filtering;
- enrichment;
- broad ecosystem familiarity;
- straightforward local desktop deployment.

Serilog remains an Infrastructure detail.

Chronicle code SHOULD use `ILogger<T>` rather than concrete Serilog APIs outside the logging bootstrap and adapter layer.

## 6. Abstraction Boundary

Production code SHOULD depend on:

```text
Microsoft.Extensions.Logging.ILogger<T>
```

Only logging bootstrap and Infrastructure logging code MAY reference:

- Serilog;
- Serilog sinks;
- Serilog enrichers;
- Serilog formatters.

Domain code SHOULD avoid logging except for narrowly justified Domain-level diagnostics through an injected abstraction at a higher layer.

## 7. Logging Ownership by Layer

### Domain

Domain SHOULD return typed outcomes and invariant failures.

It SHOULD NOT emit operational logs directly.

### Application

Application logs:

- operation start;
- operation completion;
- operation failure category;
- retry decision;
- concurrency conflict;
- finalization phase;
- idempotency resolution.

### Infrastructure

Infrastructure logs:

- provider transport;
- persistence;
- filesystem;
- backup;
- migration;
- credential-store status;
- Work Item execution;
- package loading.

### Presentation

Presentation logs:

- route;
- action key;
- safe UI state transition;
- safe error reference;
- accessibility-relevant control failure where useful.

## 8. Log Event Model

Every log event SHOULD have:

```text
TimestampUtc
Level
EventId
EventName
MessageTemplate
Properties
ExceptionMetadata when present
ApplicationVersion
BuildClassification
ProcessId
ThreadId when useful
```

## 9. Stable Event Names

Important events MUST use stable event names.

Examples:

```text
CampaignOpened
SessionStarted
PlayerInputAccepted
NarrativeRequestStarted
NarrativeRequestCompleted
NarrativeOutputRejected
DiceRollPersisted
DiceResolutionApplied
SessionFinalizationStarted
SessionFinalizationCompleted
MigrationStarted
MigrationFailed
BackupCompleted
RestoreCompleted
WorkItemRecovered
SecurityViolationBlocked
```

## 10. Event Identifiers

Important events SHOULD have stable numeric or string identifiers.

Event identity should not depend on the human-readable message template.

## 11. Message Templates

Logs SHOULD use structured message templates.

Preferred:

```text
"Session finalization {OperationId} completed for session {SessionId}"
```

Avoid interpolated strings that erase property structure.

## 12. Correlation Properties

Chronicle SHOULD propagate:

```text
OperationId
CorrelationId
CampaignId
SessionId
ActId
SceneId
CharacterId
DiceRollId
WorkItemId
ProviderProfileId
RuleSetPackageId
```

Only properties relevant to the current operation are included.

## 13. Safe Identifier Policy

Stable identifiers are generally permitted because they aid diagnosis without containing narrative meaning.

However:

- IDs MUST not encode user names;
- IDs MUST not encode Campaign titles;
- IDs MUST not include secrets;
- IDs SHOULD be random or opaque.

## 14. Operation Scope

Every Application command SHOULD begin a structured log scope containing:

```text
OperationId
OperationType
CorrelationId
CampaignId when scoped
ExpectedVersion when relevant
```

Nested Infrastructure logs inherit this scope.

## 15. Provider Scope

Provider calls SHOULD add:

```text
ProviderProfileId
Capability
ModelProfile
ConcreteModelIdentifier
Attempt
ContractVersion
```

They MUST NOT add raw prompts or responses.

## 16. Persistence Scope

Persistence operations MAY add:

```text
Repository
TransactionKind
ExpectedVersion
AffectedRowCount
DatabaseOperation
```

They MUST NOT add SQL parameters containing private content.

## 17. Work Item Scope

Background work SHOULD add:

```text
WorkItemId
WorkType
Attempt
LeaseOwner
OperationId
```

## 18. Log Levels

Chronicle will use standard levels:

```text
Trace
Debug
Information
Warning
Error
Critical
```

## 19. Trace

Trace is reserved for highly detailed developer diagnostics.

It is disabled by default in Release builds.

## 20. Debug

Debug may include:

- internal state-transition names;
- query timing;
- adapter compatibility details;
- cache decisions;
- nonprivate configuration resolution.

It MUST still obey sensitive-data rules.

## 21. Information

Information records meaningful successful lifecycle events.

Examples:

- Campaign opened;
- Session started;
- Roll persisted;
- finalization completed;
- backup created;
- migration applied.

Information SHOULD not record every low-level method call.

## 22. Warning

Warning indicates recoverable or degraded conditions.

Examples:

- provider rate limited;
- stale response rejected;
- retry scheduled;
- package unavailable;
- index stale;
- Safe Mode entered;
- invalid optional configuration.

## 23. Error

Error indicates a failed operation that requires retry, recovery, or user awareness.

Examples:

- provider request failed;
- backup failed;
- finalization failed before commit;
- restore rejected;
- migration aborted safely.

## 24. Critical

Critical is reserved for severe integrity or security threats.

Examples:

- database corruption suspected;
- Secret leakage detected;
- invariant breach after commit;
- credential exposure detected;
- unrecoverable startup failure;
- compromised release metadata.

## 25. Default Level Policy

Release builds SHOULD default to:

```text
Information
```

with category overrides such as:

```text
Microsoft.* = Warning
System.Net.Http.* = Warning
EF Core command detail = Warning or disabled
```

Development builds MAY default to Debug.

## 26. Production Sink

Release builds use rolling JSON files.

Each line SHOULD be one structured JSON event.

### Rationale

JSON logs support:

- filtering;
- automated support tooling;
- field-level redaction review;
- stable diagnostics;
- future OpenTelemetry mapping.

## 27. Development Console Sink

Development builds MAY emit concise human-readable logs to console.

The console sink is not the authoritative support artifact.

## 28. Log Directory

On Windows, logs SHOULD reside under the per-user Chronicle data root.

Conceptually:

```text
%LOCALAPPDATA%\Chronicle\logs\
```

## 29. File Naming

Log file names SHOULD include:

- application;
- date;
- process or sequence where needed.

Example:

```text
chronicle-2026-08-01.jsonl
```

## 30. Rolling Policy

Logs SHOULD roll by date and MAY also roll by file size.

The implementation must prevent unbounded disk growth.

## 31. Retention Policy

Retention MUST be bounded.

Initial policy SHOULD consider:

- maximum age;
- maximum retained file count;
- maximum total log size.

Exact limits require operational testing.

## 32. Log Cleanup

Cleanup SHOULD occur:

- at startup;
- after rotation;
- during safe maintenance.

Cleanup must not block the UI significantly.

## 33. Disk-Full Behavior

Logging failure MUST NOT corrupt Campaign state.

If the log sink cannot write:

- Chronicle continues where safe;
- a bounded in-memory or UI warning MAY be used;
- repeated sink failures are throttled;
- no recursive logging loop occurs.

## 34. Sensitive Data Principle

Chronicle logs system behavior, not Campaign content.

The following are prohibited by default:

```text
Player input
Narrator prose
Archivist prose
Campaign summaries
Character biographies
Secrets
Knowledge statements
Relationship descriptions
Rule Knowledge excerpts
Raw prompts
Raw provider responses
Credentials
Authorization headers
Full local file paths
```

## 35. Data Classification

Every structured logging helper SHOULD assume values are private unless explicitly classified safe.

## 36. Safe Log Properties

Generally safe properties include:

- opaque identifiers;
- enum-like status keys;
- event names;
- version numbers;
- durations;
- counts;
- sizes;
- error codes;
- retry classifications;
- provider profile identifiers;
- package identifiers;
- schema versions;
- booleans;
- bounded numeric metrics.

## 37. Unsafe Log Properties

Unsafe properties include:

- free-form user text;
- generated narrative;
- filenames selected by users;
- local directory paths;
- provider payloads;
- Character names when avoidable;
- Campaign titles;
- Secret names;
- sourcebook excerpts;
- stack traces containing raw request data.

## 38. Redaction

Chronicle will implement centralized redaction for known sensitive patterns.

Examples:

- API keys;
- bearer tokens;
- authorization headers;
- credential aliases if classified sensitive;
- connection strings;
- signed URLs;
- private file paths;
- email-like identifiers when not needed.

## 39. Redaction Is Not Authorization

Redaction is defense in depth.

Call sites remain responsible for not logging sensitive values in the first place.

## 40. Credential Redaction

Any property name matching credential-related patterns SHOULD be removed or replaced.

Examples:

```text
ApiKey
Authorization
Token
Secret
Password
Credential
```

## 41. Exception Logging

Exceptions MAY be logged with:

- exception type;
- safe message;
- stack trace;
- inner exception type;
- provider or SQLite error code.

Exception data dictionaries and request objects MUST be sanitized.

## 42. Exception Message Risk

Some third-party exceptions include:

- URLs;
- SQL;
- response bodies;
- local paths.

Infrastructure adapters MUST normalize or sanitize exception details before logging.

## 43. Raw SQL Logging

EF Core sensitive-data logging is disabled.

Detailed SQL command logging is disabled by default.

Development mode MAY log SQL structure without values when practical.

## 44. HTTP Logging

Default .NET HTTP logging must be configured to avoid:

- authorization headers;
- request bodies;
- response bodies;
- sensitive query strings.

## 45. Provider Logging

Provider logs MAY include:

- request started;
- request completed;
- status code;
- latency;
- usage;
- provider request ID;
- finish status;
- retry state.

They MUST not include prompt or response text.

## 46. Prompt Diagnostics

Raw prompt capture is a separate explicit diagnostic mode.

It is:

- off by default;
- unavailable in normal Release operation unless explicitly enabled;
- clearly warned;
- bounded;
- redactable;
- excluded from ordinary diagnostic bundles by default.

## 47. Response Diagnostics

Raw provider-response capture follows the same policy as prompt diagnostics.

Accepted narrative remains in Campaign persistence, not logs.

## 48. Audit Versus Log

Chronicle distinguishes:

```text
Operational Log
Audit Record
Domain History
```

### Operational Log

Explains system behavior.

### Audit Record

Records important accepted operations using safe metadata.

### Domain History

Preserves authoritative Campaign facts.

Logs MUST NOT substitute for Domain history.

## 49. Audit Record Implementation

Audit records are persisted in SQLite through Chronicle's persistence model.

Serilog files are not authoritative audit storage.

## 50. Security Events

Security-relevant events SHOULD use stable names and classifications.

Examples:

```text
CrossCampaignReferenceBlocked
ProviderToolRequestRejected
CredentialExposurePrevented
UnsafeImportRejected
PathTraversalBlocked
SecretProjectionViolation
UntrustedPackageBlocked
StructuredOutputReferenceRejected
```

## 51. Security Event Content

Security events may record:

- event type;
- OperationId;
- safe target type;
- safe identifier;
- detection rule;
- action taken;
- application version.

They must not echo the malicious payload in full.

## 52. User-Facing Reference Codes

Errors shown to the player SHOULD include a safe reference code.

The reference code maps to:

- OperationId;
- ErrorCode;
- timestamp window;
- relevant logs.

It must not expose internal stack or private data.

## 53. Diagnostic Bundle

Chronicle SHOULD support creation of a diagnostic bundle containing:

- selected log files;
- application version;
- release manifest;
- safe configuration summary;
- package inventory;
- migration status;
- integrity-check results;
- environment summary;
- explicit privacy manifest.

## 54. Diagnostic Bundle Exclusions

By default, a diagnostic bundle excludes:

- Campaign database;
- backups;
- exports;
- prompts;
- provider responses;
- Character data;
- Secrets;
- credentials;
- unrestricted file paths.

## 55. Diagnostic Bundle Consent

The user MUST review and explicitly create a diagnostic bundle.

Automatic upload is excluded from MVP.

## 56. Bundle Redaction Pass

Before publication, Chronicle SHOULD scan the bundle for:

- credential patterns;
- authorization headers;
- known local user paths;
- accidental narrative payloads;
- provider request bodies.

A detected high-risk value blocks bundle creation until handled.

## 57. Remote Logging

Remote log export is disabled in MVP.

No sink may send logs to:

- hosted telemetry service;
- analytics platform;
- crash-reporting service;
- project server;

without a later privacy and telemetry decision.

## 58. OpenTelemetry Compatibility

Chronicle does not require an OpenTelemetry exporter in MVP.

However, event and span names SHOULD be designed so that future mapping is straightforward.

## 59. Activity and Trace Context

Application operations MAY create .NET `Activity` scopes.

If adopted, they SHOULD propagate:

- TraceId;
- SpanId;
- OperationId;
- CorrelationId.

Serilog enrichers MAY include these values.

## 60. Trace Ownership

Trace context is diagnostic.

It does not replace OperationId or Domain identity.

## 61. Metrics

Metrics are defined by RFC-0036 and are not implemented through log parsing alone.

Some event-derived counters MAY exist, but logs are not the sole metrics backend.

## 62. Performance

Logging MUST be nonblocking enough not to degrade:

- player input;
- Dice Roll commit;
- transcript rendering;
- Session finalization;
- application shutdown.

## 63. Asynchronous Writing

The file sink MAY use bounded asynchronous buffering.

The buffer MUST define:

- capacity;
- overflow behavior;
- shutdown flush;
- critical-event handling.

## 64. Buffer Overflow

When a log buffer is full:

- low-level events MAY be dropped according to policy;
- Error and Critical events SHOULD be preserved where practical;
- a drop counter SHOULD be recorded safely;
- application state must not block indefinitely.

## 65. Shutdown

On normal shutdown, Chronicle SHOULD flush pending log events within a bounded timeout.

It must not delay shutdown indefinitely.

## 66. Crash Behavior

A process crash may lose buffered low-severity events.

Critical persistence and Domain correctness must not depend on log flush.

## 67. Logging Configuration

Logging configuration SHOULD support:

```text
MinimumLevel
CategoryOverrides
FileRetention
FileSizeLimit
ConsoleEnabled
DiagnosticMode
RawPromptCaptureEnabled
RawResponseCaptureEnabled
```

Raw capture options are disabled by default and may be restricted to Development builds.

## 68. Configuration Reload

Dynamic logging-level reload MAY be supported.

Changes affecting sensitive-data capture require explicit confirmation and may require restart.

## 69. Environment Variables

Development environments MAY override log levels through environment variables.

Credentials and narrative content remain prohibited.

## 70. Application Startup

Logging initializes as early as practical.

A bootstrap logger MAY capture startup failures before the full Host is available.

## 71. Bootstrap Logger

The bootstrap logger MUST use the same privacy constraints.

It SHOULD write to a small bounded startup log file.

## 72. Host Integration

Serilog will integrate with the .NET generic Host.

It SHOULD replace default providers after bootstrap initialization to avoid duplicate logs.

## 73. Duplicate Event Prevention

The configuration MUST prevent the same event from being written multiple times through overlapping providers.

## 74. Serilog Enrichers

Approved enrichers MAY add:

- application version;
- process ID;
- thread ID;
- environment/build classification;
- Activity trace identifiers.

Enrichers that inspect arbitrary request or object state are prohibited.

## 75. Destructuring Policy

Automatic deep object destructuring is discouraged.

Only explicitly approved small safe types may be destructured.

## 76. Collection Limits

Logged collections MUST have bounded item counts.

Large lists should log:

- total count;
- selected safe identifiers;
- truncation indicator.

## 77. String Length Limits

Free-form diagnostic strings, when allowed, MUST have length limits.

Truncation should be explicit.

## 78. File Path Policy

Prefer:

- path category;
- file extension;
- root type;
- path hash;
- safe basename only when reviewed.

Avoid full user paths.

## 79. Campaign and Character Names

Campaign and Character names SHOULD not appear in default logs.

Opaque identifiers are sufficient.

## 80. Log Schema Version

Structured JSON log events SHOULD include a `LogSchemaVersion`.

Schema changes require compatibility consideration for diagnostic tooling.

## 81. Event Versioning

Long-lived event names SHOULD have stable property semantics.

Material changes MAY require:

- event version;
- new event name;
- compatibility mapping.

## 82. Localization

Log event names and property keys are language-neutral English machine keys.

Human-readable UI errors are localized separately.

## 83. Time

All log timestamps are UTC.

Presentation tools may display local time.

## 84. Testing Strategy

The logging implementation requires:

```text
Unit Tests
Integration Tests
Security Tests
Retention Tests
Diagnostic Bundle Tests
Performance Tests
Crash and Shutdown Tests
```

## 85. Unit Tests

Unit tests SHOULD cover:

- redaction;
- safe-property classification;
- event-name mapping;
- reference-code generation;
- exception sanitization;
- path sanitization;
- collection limits.

## 86. Integration Tests

Integration tests SHOULD cover:

- Host startup;
- Serilog provider registration;
- JSON file output;
- rolling;
- retention;
- scope propagation;
- OperationId enrichment;
- file access failure;
- shutdown flush.

## 87. Security Tests

Security tests MUST attempt to log:

- API key;
- bearer token;
- prompt;
- response;
- Campaign Secret;
- Character biography;
- full local path;
- SQL parameters;
- authorization header.

The resulting production log MUST not contain the sensitive value.

## 88. Diagnostic Bundle Tests

Tests MUST prove:

- ordinary logs included;
- Campaign database excluded;
- credentials excluded;
- raw prompts excluded;
- privacy manifest included;
- redaction scan runs;
- unsafe bundle blocked.

## 89. Performance Tests

Performance tests SHOULD measure:

- sustained Message operations;
- long Session finalization;
- provider retry loops;
- background Work Item execution;
- rolling-file transition;
- application shutdown flush.

## 90. Required Test Cases

Tests MUST cover:

- Information event written;
- Debug event filtered in Release;
- category override;
- OperationId scope;
- CorrelationId scope;
- provider metadata without content;
- persistence metadata without SQL values;
- exception sanitization;
- credential redaction;
- path redaction;
- collection truncation;
- file rotation;
- retention cleanup;
- disk-full simulation;
- locked log file;
- asynchronous buffer overflow;
- normal shutdown flush;
- crash-loss tolerance;
- duplicate-provider prevention;
- development console output;
- diagnostic bundle creation;
- unsafe diagnostic bundle rejection;
- remote sink absent.

## 91. Architecture Tests

Architecture tests MUST reject:

- direct Serilog references outside approved logging Infrastructure and bootstrap;
- raw prompt logging calls;
- raw response logging calls;
- `EnableSensitiveDataLogging` in Release configuration;
- logging of credential-bearing objects;
- logging dependencies in Domain entities;
- remote telemetry sinks in MVP production configuration.

## 92. Prohibited Patterns

### 92.1 String Interpolation for Structured Events

Use message templates and named properties.

### 92.2 Logging Entire Objects

Do not destructure commands, responses, Domain entities, or provider payloads by default.

### 92.3 Logging Credentials Then Redacting Later

Sensitive values should never enter the log pipeline.

### 92.4 Raw Prompt Logging by Default

Disabled.

### 92.5 Logs as Audit Authority

Audit records and Domain history remain separate.

### 92.6 Remote Sink Without Explicit Architecture Decision

Prohibited in MVP.

### 92.7 User-Facing Error Equals Raw Exception

Errors are mapped and sanitized.

### 92.8 Full File Paths in Normal Logs

Use safe path metadata.

### 92.9 Unbounded Retention

Logs must rotate and expire.

### 92.10 Logging Inside Tight Domain Loops

Log meaningful operation boundaries, not every calculation.

## 93. Alternatives Considered

### Built-In Logging Providers Only

The default .NET providers are useful but do not alone provide the desired local rolling structured JSON experience with the same maturity and flexibility.

### NLog

NLog is mature and capable.

Serilog was selected because its structured-event model and common .NET ecosystem usage align strongly with Chronicle's observability requirements.

NLog remains a viable fallback if implementation or licensing concerns arise.

### OpenTelemetry-Only Logging

Not selected as the sole MVP implementation because Chronicle needs simple local rolling files without requiring a collector or exporter.

### Custom Logger

Rejected because it would recreate mature behavior:

- rolling;
- formatting;
- scopes;
- filtering;
- enrichment;
- sinks;
- integration.

### Remote Crash Reporting Service

Deferred because it introduces a new external data boundary and privacy burden.

## 94. Consequences

### Positive

- structured local diagnostics;
- strong .NET integration;
- clear correlation;
- simple support bundles;
- bounded disk usage;
- future telemetry compatibility;
- no remote dependency;
- mature ecosystem.

### Negative

- additional dependency and configuration;
- logging schema requires discipline;
- asynchronous buffering can lose recent low-severity events during crash;
- redaction requires continuous testing;
- JSON logs are less convenient for casual manual reading.

## 95. Risks

### Sensitive Data Leakage

Mitigation:

- no object dumping;
- centralized redaction;
- architecture tests;
- adversarial log tests;
- diagnostic-bundle scanner.

### Excessive Log Volume

Mitigation:

- Information-level discipline;
- category overrides;
- retention;
- size limits;
- aggregation.

### Logging Failure Affects Application

Mitigation:

- bounded nonblocking sink;
- no correctness dependency;
- sink-failure throttling;
- disk-full testing.

### Serilog Coupling

Mitigation:

- `ILogger<T>` in application code;
- Serilog restricted to Infrastructure;
- stable event conventions independent from sink.

### Difficult Support Interpretation

Mitigation:

- stable event names;
- OperationId;
- reference codes;
- log schema version;
- diagnostic viewer later.

## 96. Technology Spike

Before acceptance, implement:

1. bootstrap logger;
2. Host integration;
3. rolling JSON sink;
4. development console sink;
5. OperationId and CorrelationId scopes;
6. provider-call event;
7. Dice Roll event;
8. Session-finalization event;
9. sanitized exception;
10. credential-redaction test;
11. disk-full simulation;
12. diagnostic bundle;
13. retention cleanup;
14. shutdown flush;
15. long-Campaign performance run.

## 97. Spike Acceptance

The spike passes when:

- structured events are valid JSON;
- OperationId connects UI, Application, provider, and persistence events;
- no test credential appears in logs;
- no synthetic Campaign narrative appears in default logs;
- rotation and retention work after restart;
- a sink failure does not break a committed Dice Roll;
- diagnostic bundles exclude Campaign data;
- the logging abstraction remains `ILogger<T>`;
- Release configuration contains no remote sink.

## 98. Definition of Compliance

An implementation complies when:

- `Microsoft.Extensions.Logging` is the application abstraction;
- Serilog is confined to logging Infrastructure;
- Release logs use rolling structured JSON files;
- retention is bounded;
- OperationId and CorrelationId propagate;
- raw prompts, responses, and Campaign prose are excluded by default;
- credentials are never logged;
- exception details are sanitized;
- diagnostic bundles require explicit user action;
- remote export is disabled;
- logging failures do not affect Domain correctness;
- security tests validate redaction.

## 99. Review Triggers

This ADR must be reviewed if:

- Serilog licensing or maintenance changes materially;
- a remote telemetry service is proposed;
- crash reporting is introduced;
- OpenTelemetry collection becomes a product requirement;
- log volume harms performance;
- privacy incidents reveal insufficient redaction;
- desktop support expands to platforms with different logging conventions;
- support tooling requires a different event format.

## 100. Deferred Decisions

Later ADRs MAY define:

- exact retention values;
- exact file-size limits;
- exact JSON formatter;
- OpenTelemetry tracing;
- metrics implementation;
- crash-reporting service;
- local log viewer;
- encrypted diagnostic bundles;
- opt-in remote telemetry;
- security-event retention policy.

## 101. Final Decision

Chronicle will use `Microsoft.Extensions.Logging` throughout application code and Serilog as the initial concrete local structured logging implementation.

Release builds will write bounded rolling JSON logs, preserve OperationId-based correlation, exclude Campaign content and credentials, and offer explicit safe diagnostic bundles.

Chronicle must be able to explain its behavior.

It must never explain the player's secrets to the wrong place.
