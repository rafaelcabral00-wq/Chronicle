---
id: RFC-0036
title: Observability, Diagnostics, and Telemetry Architecture
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-08-01
category: Architecture
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
  - RFC-0035
---

> **"Chronicle must be able to explain what happened operationally without exposing what happened narratively."**

# Observability, Diagnostics, and Telemetry Architecture

## Abstract

This RFC defines Chronicle's observability architecture.

It establishes how Chronicle records, correlates, measures, diagnoses, and presents operational behavior across application use cases, Campaign lifecycle, persistence, Rule Set execution, Dice resolution, Narrative Intelligence, Prompt construction, Rule Knowledge retrieval, background work, backup and restore, import and export, migrations, and security controls.

The architecture distinguishes:

```text
Logs
Metrics
Traces
Diagnostics
Audit Records
User-Facing Status
Telemetry
```

These concepts MUST remain separate because they have different purposes, retention policies, privacy risks, and trust implications.

The MVP remains local-first and MUST NOT require remote telemetry.

## 1. Purpose

Chronicle is a stateful, AI-assisted, rules-driven application.

Operational failures may occur across several layers:

- provider timeout;
- invalid structured response;
- stale Campaign version;
- Rule Set validation failure;
- context budget overflow;
- missing knowledge index;
- duplicate OperationId;
- failed finalization;
- corrupted backup;
- migration conflict;
- read-model lag;
- credential failure;
- hidden-information violation.

Without coherent observability, these failures become difficult to diagnose and easy to misattribute.

Chronicle needs enough evidence to answer:

- What operation was attempted?
- Which state version was used?
- Which component failed?
- Did persistence commit?
- Was randomness generated?
- Was a provider called?
- Was repair attempted?
- Did the Rule Set reject the request?
- Was private information exposed?
- Can the operation be retried safely?
- What should the user do next?

## 2. Scope

This RFC defines:

- observability principles;
- event correlation;
- OperationId usage;
- structured logs;
- metrics;
- tracing;
- diagnostic bundles;
- user-facing status;
- audit records;
- telemetry boundaries;
- privacy and redaction;
- retention;
- local diagnostic storage;
- provider usage metadata;
- performance measurements;
- error correlation;
- security event observability;
- testing;
- MVP requirements.

This RFC does not define:

- one logging library;
- one tracing SDK;
- one metrics backend;
- one dashboard product;
- remote telemetry service;
- cloud monitoring;
- enterprise audit integration;
- final UI design;
- exact retention durations.

## 3. Core Principle

Observability MUST explain the system without leaking the story.

Chronicle SHOULD prefer:

```text
Selected 4 Memories
```

over:

```text
Selected Memory: "The Prince betrayed..."
```

Operational visibility and narrative confidentiality are separate concerns.

## 4. Observability Concepts

Chronicle distinguishes:

```text
Log
    A structured record of a discrete operational occurrence.

Metric
    A numeric measurement aggregated over time.

Trace
    A correlated path across components and stages.

Diagnostic Record
    Detailed local evidence for troubleshooting.

Audit Record
    Trusted history of security- or state-relevant administrative actions.

User-Facing Status
    Safe explanation presented to the player.

Telemetry
    Observability data transmitted outside the local installation.
```

## 5. Observability Ownership

Application and infrastructure layers emit observability data.

Domain entities MAY emit Domain Events that infrastructure observes.

Domain code SHOULD NOT depend on logging frameworks or telemetry SDKs.

## 6. Local-First Requirement

The MVP MUST function with:

- no remote telemetry;
- no external log service;
- no cloud metrics;
- no online tracing backend.

Local diagnostics are sufficient for the first delivery.

## 7. Correlation Model

Chronicle SHOULD correlate operations using:

```text
OperationId
CorrelationId
CampaignId
SessionId
SceneId
WorkItemId
ProviderRequestId
DiceRollId
MigrationId
ArtifactId
```

Not every record needs every identifier.

## 8. OperationId

OperationId is the primary correlation identity for one application operation.

Every state-changing use case MUST emit observability records containing OperationId.

## 9. CorrelationId

CorrelationId connects related operations.

Example:

```text
ProcessPlayerInput
    ↓
RequestRoll
    ↓
ExecuteRoll
    ↓
ContinueNarration
```

Each operation has its own OperationId.

All MAY share one CorrelationId.

## 10. Campaign Correlation

CampaignId SHOULD be included in Campaign-scoped records.

It MUST be treated as Internal metadata.

Logs SHOULD avoid including Campaign title by default.

## 11. Structured Logging

Chronicle MUST use structured logs rather than free-form prose as the primary diagnostic format.

A log record SHOULD contain:

```text
Timestamp
Severity
Component
EventName
OperationId
CorrelationId
CampaignId
Status
Duration
ErrorCode
SafeMetadata
```

## 12. Log Severity

Initial severities SHOULD include:

```text
Trace
Debug
Information
Warning
Error
Critical
```

Severity MUST reflect operational importance, not narrative drama.

## 13. Event Name

Every structured log SHOULD use a stable event name.

Examples:

```text
NarrativeOperationStarted
NarrativeOperationCompleted
NarrativeResponseRejected
RuleOperationResolved
PersistenceCommitFailed
SessionFinalizationCompleted
BackupValidationFailed
ImportRejected
SecurityPolicyBlocked
```

## 14. Safe Metadata

SafeMetadata MAY include:

- counts;
- versions;
- durations;
- enum values;
- hashes;
- identifiers;
- status;
- size estimates.

It SHOULD NOT include full narrative payloads.

## 15. Logging Prohibitions

Default logs MUST NOT contain:

- full prompts;
- full provider responses;
- API keys;
- encryption keys;
- Character biography text;
- complete Messages;
- canonical Secrets;
- restricted Rule Knowledge excerpts;
- absolute local paths;
- raw imported files;
- private attachment content.

## 16. Prompt and Provider Response Logging

Prompt and provider response logging SHOULD be disabled by default.

When developer diagnostics enable it:

- explicit warning is required;
- redaction is applied;
- retention is bounded;
- logs remain local;
- Secret and Restricted fields are excluded where possible.

Normal operation SHOULD log only:

- request and response sizes;
- contract version;
- event count;
- validation result;
- stop reason;
- repair count;
- provider usage metadata.

## 17. Error Logging

An error log SHOULD contain:

- ErrorCode;
- component;
- OperationId;
- safe summary;
- retry classification;
- stage;
- related version;
- stack trace in developer mode where appropriate.

## 18. Metrics

Metrics SHOULD measure trends without retaining narrative content.

Initial metric categories include:

```text
Application
Persistence
Narrative Intelligence
Rule Set
Dice
Background Work
Knowledge Retrieval
Backup and Restore
Import and Export
Security
```

## 19. Application and Persistence Metrics

Possible application metrics:

- use case duration;
- operation completion rate;
- operation failure rate;
- user-cancel rate;
- concurrency conflict rate;
- recovery rate.

Possible persistence metrics:

- transaction duration;
- commit failures;
- optimistic concurrency conflicts;
- database size;
- read-model lag;
- migration duration;
- integrity-check failures.

## 20. Narrative Intelligence Metrics

Possible metrics:

- provider latency;
- input size;
- output size;
- structured validity;
- repair rate;
- retry rate;
- refusal rate;
- timeout rate;
- stale-response rejection;
- context-insufficient rate.

## 21. Rule Set and Dice Metrics

Possible Rule Set metrics:

- operation count by key;
- validation failure rate;
- modifier rejection rate;
- progression evaluation failures;
- calculation duration;
- migration failures.

Possible Dice metrics:

- Rolls requested;
- Rolls executed;
- resolution failures;
- consequence application failures;
- duplicate-execution prevention;
- unresolved Roll recovery.

## 22. Background and Retrieval Metrics

Possible background metrics:

- queued Work Items;
- running Work Items;
- lease expiration;
- retry count;
- stale Work Items;
- terminal failures;
- average completion time.

Possible Rule Knowledge metrics:

- query duration;
- result count;
- no-result rate;
- low-confidence rate;
- exact-key hit rate;
- stale-index rate;
- index build duration;
- restricted-source block count.

## 23. Security Metrics

Possible metrics:

- visibility violations;
- cross-Campaign reference blocks;
- provider transmission blocks;
- prompt-injection detections;
- package integrity failures;
- unsafe import rejections;
- credential errors;
- replay detections.

Security metrics MUST avoid storing sensitive payloads.

## 24. Metric Labels and Cardinality

Metric labels MUST remain bounded.

Avoid labels containing:

- Campaign title;
- Character name;
- free text;
- Error message text;
- provider response text;
- arbitrary file paths.

OperationId, CampaignId, and MessageId SHOULD NOT normally be metric labels.

They belong in logs and traces.

## 25. Tracing

A trace represents one logical flow across components.

A trace MAY include spans such as:

```text
ApplicationUseCase
LoadCampaign
SelectContext
RetrieveRuleKnowledge
BuildPrompt
InvokeProvider
ParseResponse
ValidateEvents
ApplyChangeSet
CommitTransaction
UpdateReadModel
```

## 26. Trace Span Contract

Trace spans SHOULD record:

- start and end;
- duration;
- status;
- component;
- parent span;
- safe attributes.

## 27. Provider Trace

Provider spans SHOULD include:

- provider profile;
- capability;
- operation profile;
- model mapping identifier;
- timeout;
- input estimate;
- output usage;
- status.

They SHOULD NOT include full prompt or response content.

## 28. Persistence and Rule Set Traces

Persistence spans SHOULD include:

- repository or transaction name;
- operation type;
- expected version;
- resulting version;
- entity count where safe;
- duration;
- commit result.

Rule Set spans SHOULD include:

- package identity;
- Rule Set version;
- operation key;
- calculation version;
- validation result;
- duration.

## 29. Trace Retention

Local tracing MAY retain all failed operations and sample successful operations.

The MVP may use bounded retention rather than a full sampling engine.

## 30. Diagnostic Record

A Diagnostic Record contains richer troubleshooting data than normal logs.

It MAY include:

- normalized request metadata;
- version maps;
- omission report;
- event validation report;
- package descriptor;
- index status;
- migration status;
- safe stack trace;
- redacted payload fragments.

## 31. Diagnostic Activation

Detailed diagnostics SHOULD require:

- explicit developer mode;
- explicit operation capture;
- or automatic capture for Critical failures.

## 32. Diagnostic Bundle

Chronicle SHOULD support creating a local Diagnostic Bundle.

A bundle MAY contain:

- application version;
- platform information;
- enabled package descriptors;
- storage schema version;
- recent structured logs;
- recent failure traces;
- integrity results;
- redacted operation metadata;
- configuration without credentials.

## 33. Diagnostic Bundle Exclusions

A Diagnostic Bundle MUST exclude by default:

- provider credentials;
- encryption keys;
- full Campaign content;
- full prompts;
- full provider responses;
- proprietary source text;
- raw sourcebook files;
- private attachments.

## 34. Diagnostic Bundle Preview

Before export, the user SHOULD be able to see:

- included categories;
- time range;
- privacy warning;
- redaction status;
- approximate size.

## 35. Audit Records

Audit Records differ from logs.

They preserve trusted history for actions such as:

- Campaign Preference change;
- Rule Set migration;
- Character advancement correction;
- backup restore;
- Campaign replacement import;
- credential profile change;
- security policy override;
- destructive deletion.

## 36. Audit Record Contract

An Audit Record SHOULD contain:

- AuditId;
- OperationId;
- action type;
- target type;
- target identifier;
- actor classification;
- timestamp;
- prior and resulting versions;
- result;
- safe reason;
- related artifact or migration.

## 37. Audit Integrity

Audit Records SHOULD be append-oriented.

Corrections should create new records rather than silently rewriting history.

The MVP does not require a cryptographically append-only audit store.

## 38. User-Facing Status

User-facing status is not a raw error log.

It SHOULD explain:

- what Chronicle is doing;
- whether the operation is waiting;
- whether retry is safe;
- whether user action is required;
- what data remains preserved.

## 39. Safe Status Examples

```text
Waiting for the Narrative Intelligence provider.
```

```text
The Dice Roll was saved, but narration could not continue yet.
Retrying will not roll again.
```

```text
Session finalization is paused because the Campaign changed.
No finalization changes were applied.
```

## 40. Error Correlation

Every user-facing error SHOULD expose a safe reference identifier.

Example:

```text
Reference: OP-7F3A
```

This identifier maps to OperationId or a safe short form.

## 41. Telemetry Definition

Telemetry is observability data transmitted outside the local installation.

The MVP MUST NOT require telemetry.

## 42. Telemetry Consent

Future telemetry MUST be:

- clearly described;
- configurable;
- revocable;
- minimized;
- separated from provider requests;
- disabled where required by product mode.

## 43. Telemetry Categories

Possible future categories:

```text
OperationalHealth
Performance
FeatureUsage
CrashDiagnostics
SecuritySignals
QualityFeedback
```

Each category requires separate review.

## 44. Telemetry Data Minimization

Telemetry SHOULD use:

- counts;
- durations;
- version identifiers;
- coarse platform data;
- error codes.

It SHOULD avoid:

- Campaign content;
- Character names;
- Session text;
- Secrets;
- sourcebook text;
- local paths;
- persistent user identifiers unless necessary.

## 45. Provider Usage and Cost Observability

Chronicle SHOULD record provider usage metadata when available.

Examples:

- input units;
- output units;
- cached units;
- provider latency;
- model profile;
- finish reason.

Where providers charge for usage, Chronicle MAY calculate:

- operation cost;
- Session cost;
- Campaign generation cost;
- repair overhead;
- retry overhead.

Cost data SHOULD remain local by default.

## 46. Context Observability

Prompt construction SHOULD expose:

- selected fragment count;
- omitted fragment count;
- required context count;
- estimated input size;
- fixed instruction size;
- output reservation;
- safety margin;
- context budget result.

Context observability SHOULD use metadata and hashes rather than raw content.

## 47. Structured Output Observability

Chronicle SHOULD record:

- event count;
- event types;
- schema version;
- validation errors;
- accepted count;
- rejected count;
- repaired count;
- duplicate count.

## 48. Finalization Observability

Session finalization SHOULD record:

- evidence count;
- proposed change count;
- accepted change count;
- rejected change count;
- Memory aging result;
- progression result;
- Relationship change count;
- Knowledge change count;
- transaction result.

Counts SHOULD be preferred over content.

## 49. Recovery and Migration Observability

Recovery operations SHOULD record:

- discovered incomplete operation;
- prior status;
- current authoritative state;
- action taken;
- retry result;
- final status.

Migration records SHOULD include:

- migration identifier;
- source version;
- target version;
- scope;
- duration;
- warnings;
- checkpoint reference;
- validation result.

## 50. Retention

Chronicle SHOULD define separate retention for:

```text
Logs
Traces
Diagnostic Records
Audit Records
Provider Payloads
Telemetry Queue
```

## 51. Default Retention Principles

The MVP SHOULD use bounded local retention.

- normal logs rotate;
- failed-operation diagnostics live longer than successful traces;
- Audit Records follow authoritative history;
- provider payloads expire quickly;
- credentials are never retained in observability;
- telemetry queue does not exist unless telemetry is enabled.

Exact durations require an ADR.

## 52. Storage Pressure

When observability storage is constrained, Chronicle SHOULD:

1. stop optional verbose diagnostics;
2. remove expired payloads;
3. rotate old successful traces;
4. preserve recent errors;
5. preserve Audit Records;
6. warn the user when needed.

## 53. Redaction

Chronicle SHOULD support central redaction.

Redaction rules SHOULD cover:

- credentials;
- Secret fields;
- Restricted content;
- local file paths;
- provider authorization headers;
- free-text payloads;
- Character private fields.

If safe redaction cannot be guaranteed, Chronicle SHOULD omit the field.

## 54. Hashing

Hashes MAY be used to correlate content without storing it.

Hashes MUST NOT be presented as anonymization guarantees.

Low-entropy values may still be guessable.

## 55. Observability Configuration

Configuration MAY include:

- log level;
- local retention;
- developer diagnostics;
- payload capture;
- Diagnostic Bundle creation;
- future telemetry consent.

Security-critical Audit Records SHOULD not be disabled casually.

## 56. Developer and Production Modes

Developer mode MAY enable:

- Debug logs;
- more traces;
- prompt plan inspection;
- schema dumps;
- Rule Set calculation traces;
- provider payload capture.

It MUST show a privacy warning.

Production mode SHOULD default to:

- Information logs;
- bounded errors;
- no raw prompt capture;
- no full provider response capture;
- no remote telemetry;
- safe local metrics;
- Audit Records.

## 57. Component Health

Chronicle SHOULD expose local health states for:

```text
Persistence
Narrative Provider
Rule Set Package
Knowledge Index
Credential Store
Background Worker
Backup System
Migration System
```

Canonical health states MAY include:

```text
Healthy
Degraded
Unavailable
Blocked
Recovering
Unknown
```

Health states MUST not be confused with Domain state.

## 58. Diagnostics UI Horizon

The official application SHOULD eventually expose a diagnostics view containing:

- application version;
- package versions;
- provider profile status;
- knowledge index status;
- pending operations;
- backup status;
- recent safe errors;
- storage health.

The MVP UI scope requires a later RFC.

## 59. Security Event Observability

Security-relevant events SHOULD use stable names.

Examples:

```text
VisibilityViolationBlocked
CrossCampaignReferenceBlocked
ProviderTransmissionBlocked
PackageIntegrityFailed
ImportArchiveRejected
OperationReplayBlocked
CredentialReadFailed
```

Security events MAY require longer local retention than ordinary successful traces.

## 60. Quality Evaluation Integration

RFC-0026 evaluation results SHOULD use the same stable metric and event vocabulary where practical.

Offline evaluation data remains separate from production telemetry.

## 61. Observability Contract Versioning

Structured observability records SHOULD have schema versions.

Breaking changes require:

- new schema version;
- migration or parser compatibility;
- diagnostic-tool updates.

## 62. Event, Metric, and Span Catalogs

Chronicle SHOULD maintain an observability event catalog containing:

- EventName;
- severity;
- required fields;
- optional fields;
- privacy classification;
- retention category;
- owning component.

It SHOULD also maintain metric and span catalogs with stable names, units, labels, and ownership.

## 63. Error Code Integration

Observability SHOULD use RFC-0018 error codes.

Logs, metrics, traces, user-facing status, and Operation Records should reference the same canonical code.

## 64. Failure Attribution

Observability SHOULD support attribution to the correct layer.

Potential layers:

```text
Application
Persistence
ProviderAdapter
Provider
PromptBuilder
ContextSelection
RuleSetPackage
KnowledgeRetrieval
Validation
Migration
SecurityPolicy
UI
```

A provider-facing failure is not automatically a provider defect.

## 65. Diagnostic Privacy Review

Every new diagnostic field SHOULD answer:

- Why is this needed?
- What classification does it have?
- Can a count or hash replace content?
- Who can see it?
- How long is it retained?
- Can it enter an export?
- Can it enter telemetry?

## 66. Testing Strategy

### 66.1 Logging Tests

Test:

- required fields;
- stable event names;
- severity;
- redaction;
- no credentials;
- no full prompts by default.

### 66.2 Metric Tests

Test:

- units;
- bounded labels;
- no high-cardinality identifiers;
- correct increments;
- privacy-safe labels.

### 66.3 Trace Tests

Test:

- parent-child relationships;
- OperationId propagation;
- error status;
- safe attributes;
- provider and persistence spans.

### 66.4 Diagnostic Bundle Tests

Test:

- manifest;
- redaction;
- exclusions;
- integrity;
- user preview;
- safe export.

### 66.5 Audit Tests

Test:

- append behavior;
- prior and resulting versions;
- correction records;
- privacy.

## 67. Required Test Cases

Tests MUST cover:

- ProcessPlayerInput trace;
- provider timeout;
- invalid structured response;
- repair attempt;
- stale response rejection;
- Rule Set validation failure;
- Dice Roll commit;
- retry after lost confirmation;
- Session finalization failure;
- Memory aging success;
- backup validation failure;
- import rejection;
- migration warning;
- cross-Campaign block;
- credential error redaction;
- Secret omitted from logs;
- prompt capture disabled;
- developer diagnostics warning;
- metric cardinality protection;
- Diagnostic Bundle without Campaign content;
- Audit Record for Preference change;
- Audit Record for restore;
- log rotation;
- low-storage behavior;
- health-state degradation;
- telemetry disabled by default.

## 68. Prohibited Patterns

### 68.1 Full Content in Default Logs

Operational diagnosis MUST not require routinely storing narrative content.

### 68.2 Metrics With Free-Text Labels

Metric labels must remain bounded.

### 68.3 OperationId as Metric Label

Per-operation identity belongs in logs and traces.

### 68.4 Raw Error to Player

User-facing errors must be safe and actionable.

### 68.5 Telemetry Required for Operation

The MVP must work fully offline except for explicitly selected remote providers.

### 68.6 Logs as Audit History

Operational logs do not replace trusted Audit Records.

### 68.7 Audit Records Contain Full Payloads

Audit history should preserve decisions, not unrestricted content.

### 68.8 Provider Thread as Trace Store

Provider-side state does not replace Chronicle observability.

### 68.9 Read Model Health Authorizes Writes

Health and diagnostics do not bypass authoritative validation.

### 68.10 Silent Diagnostic Capture

Rich payload capture requires explicit developer-mode behavior.

## 69. Current Delivery Decision

The MVP adopts:

- local structured logs;
- OperationId and CorrelationId propagation;
- safe Campaign, Session, Scene, Work Item, and Dice Roll correlation;
- bounded local metrics;
- lightweight local tracing;
- stable event and metric catalogs;
- user-facing safe status;
- append-oriented Audit Records for sensitive state changes;
- local Diagnostic Bundles;
- central redaction;
- raw prompts and responses disabled by default;
- no mandatory remote telemetry;
- no cloud observability dependency;
- no high-cardinality metrics;
- no narrative payloads in normal logs.

## 70. Architecture Horizon

Future evolution MAY include:

- optional remote telemetry;
- crash reporting;
- OpenTelemetry-compatible exporters;
- distributed tracing;
- hosted dashboards;
- privacy-preserving analytics;
- signed Diagnostic Bundles;
- enterprise audit export;
- multi-user activity logs;
- remote support workflows;
- automated anomaly detection.

The MVP MUST NOT implement these capabilities without a later milestone.

## 71. Open Questions

The following remain open:

- Which logging library will be used?
- Which local log format will be used?
- Will traces use OpenTelemetry semantics internally?
- Which metrics are required for the first MVP release?
- How long should normal logs be retained?
- How long should Audit Records be retained?
- Should Audit Records live in the authoritative database?
- Which failures automatically create a Diagnostic Record?
- Should the diagnostics view be included in MVP?
- How should users export a Diagnostic Bundle?
- Which platform details are safe to include?
- Should provider cost be displayed to the user?
- How should local-only metrics be persisted?
- Which security events are Critical?
- What telemetry categories may be offered after MVP?
- How will telemetry consent be represented and revoked?
- Should developer mode allow raw prompt inspection?
- How should redaction be tested across new fields?

These questions require technology ADRs, UI RFCs, privacy review, and implementation evidence.

## 72. Compliance Checklist

An implementation complies when:

- observability concepts remain distinct;
- OperationId correlates state-changing work;
- structured logs use stable event names;
- logs exclude sensitive payloads by default;
- metrics use bounded labels;
- traces preserve stage and component attribution;
- diagnostics remain local by default;
- Diagnostic Bundles are redacted;
- Audit Records preserve sensitive administrative actions;
- user-facing errors remain safe;
- provider usage metadata is captured without full payloads;
- security events are observable;
- retention is bounded;
- telemetry is not required;
- developer diagnostics require explicit activation;
- no observability system becomes a second Campaign store.

## 73. Final Principle

Chronicle must be able to reconstruct how an operation behaved without reconstructing private narrative content inside its logs.

The Campaign belongs in memory.

The machinery belongs in diagnostics.
