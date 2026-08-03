---
id: RFC-0035
title: Privacy, Security, and Sensitive Data Boundaries
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-07-31
category: Security
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
---

> **"The LLM may propose. The Rule Set may validate. Chronicle decides, persists, and owns truth."**

# Privacy, Security, and Sensitive Data Boundaries

## Abstract

This RFC defines Chronicle's privacy model, trust model, threat model, sensitive-data classification, least-context principle, Secret-handling rules, provider boundaries, Rule Set isolation, prompt security, retrieval security, credential storage, logging policy, export and backup protections, and minimum MVP security requirements.

Chronicle assumes that every external component may fail, return malformed output, become unavailable, expose data accidentally, or attempt to influence higher-trust layers. Authoritative state is therefore never delegated.

Narrative Intelligence tells stories. Chronicle keeps history.

## 1. Purpose

Chronicle handles private player content, hidden Campaign information, local files, Rule Set knowledge, external Narrative Intelligence providers, generated prose, executable mechanics, and long-lived history.

This creates risks such as:

- prompt injection inside Character biographies;
- Secret leakage;
- private data sent to an unauthorized provider;
- proprietary Rule Set text copied into prompts or exports;
- fake Dice results;
- malicious packages or schemas;
- unrestricted file access;
- credential leakage;
- cross-Campaign references;
- corrupted imports and backups;
- sensitive logs;
- token and context exhaustion;
- replayed or duplicated operations.

This RFC defines the architectural controls that prevent those failures.

## 2. Scope

This RFC defines:

- security principles;
- trust zones and boundaries;
- data classification;
- least privilege and least context;
- provider isolation;
- prompt and retrieval security;
- Rule Set package isolation;
- file and artifact boundaries;
- credential storage;
- logging and telemetry policy;
- persistence and operation protections;
- threat modeling;
- incident response;
- minimum MVP requirements.

It does not define a concrete cryptographic library, sandbox technology, cloud identity system, multiplayer authorization model, or legal compliance framework.

## 3. Security Principles

### 3.1 Chronicle Owns Truth

No provider, Rule Set package, plugin, file, or user-authored text becomes authoritative without Chronicle validation.

### 3.2 The LLM Is an Advisor

Narrative Intelligence may propose, summarize, classify, narrate, and recommend.

It may not persist, generate authoritative identity, resolve Dice Rolls, bypass Rule Set validation, or alter Campaign state directly.

### 3.3 Least Privilege

Every component receives only the capabilities required for its function.

### 3.4 Least Context

Every Narrative Intelligence operation receives only the minimum context required for the current task.

### 3.5 Deny by Default

Unknown access, events, package capabilities, fields, files, and data flows are rejected by default.

### 3.6 Historical Integrity

Security controls must preserve the interpretation of completed Campaign history.

### 3.7 Local-First Privacy

Campaign data remains local by default. Remote transmission occurs only for explicitly configured provider operations.

### 3.8 Recoverable Failure

Security failure blocks unsafe continuation without corrupting accepted history.

### 3.9 Observable Without Overexposure

Chronicle records enough metadata for diagnosis while minimizing sensitive payload retention.

## 4. Trust Zones

Chronicle defines the following conceptual zones:

```text
Zone A — Trusted Core
Zone B — Trusted Application Infrastructure
Zone C — Approved Extensibility
Zone D — External Services
Zone E — Untrusted Content
Zone F — Portable Artifacts
```

### 4.1 Zone A — Trusted Core

Contains Domain entities, invariants, application commands, transaction rules, visibility rules, and idempotency logic.

It MUST NOT depend on provider SDKs, file parsers, or UI code.

### 4.2 Zone B — Trusted Application Infrastructure

Contains persistence adapters, provider adapters, credential stores, indexes, migrations, backup services, and random generator implementation.

These components implement trusted contracts but remain constrained.

### 4.3 Zone C — Approved Extensibility

Contains approved Rule Set packages and, in the future, approved plugins.

MVP Rule Set packages are statically registered trusted components, but they still obey explicit restrictions.

### 4.4 Zone D — External Services

Contains remote Narrative Intelligence providers, remote embedding providers, and future network services.

All data crossing this boundary is validated and minimized.

### 4.5 Zone E — Untrusted Content

Contains player messages, biographies, imported text, retrieved knowledge, generated prose, local documents, and attachments.

Content may be meaningful but is never executable authority.

### 4.6 Zone F — Portable Artifacts

Contains backups, exports, imports, archives, and migration artifacts.

Artifacts must be validated before publication or ingestion.

## 5. Data Classification

Chronicle classifies data as:

```text
Public
Internal
Private
Secret
Restricted
Credential
```

### 5.1 Public

Intentionally shareable data, such as public documentation or a player-approved archive.

Public does not mean automatically published.

### 5.2 Internal

Operational information not intended for players or providers by default, such as versions, identifiers, validation metadata, and work status.

### 5.3 Private

User- or Campaign-owned content not publicly shareable, such as biographies, transcripts, preferences, and private NPC notes.

### 5.4 Secret

Information whose disclosure changes narrative experience or violates visibility, including unrevealed truths, motives, future Plan content, hidden modifiers, and Character-scoped Knowledge.

### 5.5 Restricted

Data with licensing, legal, or strong handling restrictions, including proprietary Rule Set text, licensed excerpts, and user-supplied sourcebooks.

### 5.6 Credential

API keys, access tokens, encryption keys, and recovery keys.

Credentials are never Campaign content.

## 6. Data Handling Rules

Each classification must define rules for persistence, encryption, logging, backup, export, provider transmission, retention, and UI exposure.

### 6.1 Private Data

Private data remains local by default, is transmitted only when required, is redacted from logs, and is excluded from public exports.

### 6.2 Secret Data

Secret data is filtered before UI exposure, omitted from providers unless required, excluded from player-safe exports, and structurally separated from public data.

### 6.3 Restricted Data

Restricted data follows provenance, license, retention, and transmission policy. It may be local-only.

### 6.4 Credential Data

Credentials MUST:

- use a platform credential store or equivalent secure mechanism;
- remain outside Campaign records;
- never enter prompts;
- never enter exports;
- never appear in logs;
- never be passed to Rule Set packages.

## 7. Least-Context Principle

Chronicle MUST NOT send the full Campaign to Narrative Intelligence by default.

Typical Narrator context includes only:

- active Scene;
- active participants;
- relevant Character state;
- recent Messages;
- relevant Memories;
- relevant Knowledge;
- relevant Relationships;
- minimal Rule Knowledge;
- relevant Preferences;
- required Secret constraints.

Context selection order SHOULD be:

1. capability filtering;
2. operation-profile filtering;
3. Campaign ownership filtering;
4. visibility filtering;
5. relevance filtering;
6. budget filtering;
7. representation minimization.

Visibility filtering MUST occur before relevance ranking.

The following are excluded by default:

- full transcripts;
- all Memories;
- all NPCs;
- all Secrets;
- full future Narrative Plan;
- unrelated Rule Knowledge;
- local file paths;
- credentials;
- unrestricted diagnostics;
- entire sourcebooks.

## 8. Provider Isolation

Narrative Intelligence providers receive provider-neutral DTOs or rendered Prompt Documents.

They MUST NOT receive:

- database handles;
- repositories;
- filesystem access;
- application credentials;
- unrestricted tools;
- direct Rule Knowledge index access;
- authority to create persistent identifiers.

All provider output is untrusted until it is parsed, structurally validated, reference-validated, visibility-validated, Domain-validated, Rule Set-validated, and transactionally accepted.

Local providers remain untrusted computational components even though no remote transmission occurs.

## 9. Prompt Security

Prompt injection may appear in player input, biographies, imported text, Rule Knowledge, generated prose, attachments, and Campaign notes.

Chronicle assumes every text field may contain hostile instructions.

Prompt construction MUST separate:

```text
Trusted Instructions
Structured Authoritative Context
Untrusted Data
Reference Material
Output Contract
```

Only Chronicle-authored capability and operation instructions are authoritative.

Instructions embedded in data are inert.

Sanitization SHOULD include:

- delimiter escaping;
- bounded text length;
- safe Unicode normalization;
- invalid control-character removal;
- schema validation;
- prohibited-field removal;
- visibility filtering;
- source labeling.

Sanitization MUST NOT rewrite narrative meaning unnecessarily.

## 10. Prompt Poisoning and Durable Context

Repeated malicious content must not become durable trusted context through Memories, summaries, Character profiles, imports, or knowledge indexes.

Only validated, bounded, typed derivative records may enter long-term context selection.

A provider response for one OperationId MUST NOT be accepted for another request.

Chronicle validates:

- OperationId;
- Campaign version;
- Scene or Session version;
- context version;
- contract version;
- request fingerprint.

## 11. Context Exhaustion Controls

Chronicle MUST bound:

- Memory count;
- Message count;
- Character count;
- Rule Knowledge count;
- text-field size;
- summary size;
- event count;
- schema depth;
- total context budget.

Context overflow fails safely.

Critical constraints MUST NOT be silently removed to fit a provider limit.

## 12. Rule Knowledge and RAG Security

Rule Knowledge retrieval MUST enforce:

- source provenance;
- Rule Set version;
- source policy;
- transmission policy;
- bounded results;
- citations;
- prompt-injection treatment;
- size limits.

Narrative Intelligence MUST NOT query unrestricted RAG infrastructure directly in the MVP.

Chronicle retrieves, validates, filters, and selects content first.

Mitigations against poisoned knowledge include:

- allowlisted sources;
- content hashes;
- version filtering;
- source status;
- structured topic keys;
- reviewed bundled summaries;
- untrusted-data labeling.

User-supplied knowledge remains scoped to the local installation and defaults to local-only handling when restricted.

## 13. Secret Boundary

A Secret is any information whose disclosure changes the narrative experience or violates visibility.

Secret handling preserves:

- canonical truth;
- known-by Characters;
- suspected-by Characters;
- revealed fragments;
- reveal state;
- visibility;
- source;
- version.

The safest Secret is one not transmitted.

When a Secret is included in provider context, Chronicle MUST distinguish:

```text
MayUseForPortrayal
MayHint
MayPartiallyReveal
MayFullyReveal
MustNotReveal
```

Chronicle MUST validate that public prose, warnings, structured events, logs, and player-safe exports do not reveal prohibited information.

Character Knowledge is not player knowledge. Provider context must use the correct Character perspective.

## 14. Rule Set Package Isolation

A Rule Set package MUST NOT:

- access the database;
- access arbitrary files;
- access the network;
- access provider credentials;
- invoke Narrative Intelligence;
- generate authoritative randomness;
- create persistent IDs;
- bypass application transactions;
- read another Campaign.

It receives bounded structured snapshots and returns bounded structured results.

Package identity, compatibility, and integrity metadata MUST be validated before loading.

Dynamic external packages require signing, permission manifests, sandboxing, resource limits, revocation, and user approval. They are outside the MVP.

## 15. File and Artifact Security

User-supplied files are untrusted.

Chronicle SHOULD:

- allowlist formats;
- limit size;
- prevent path traversal;
- avoid macro execution;
- avoid automatic external-link traversal;
- isolate parsing;
- preserve provenance;
- avoid logging absolute paths.

Imports must be validated for archive safety, checksums, duplicate identifiers, cross-Campaign references, unsupported schemas, malicious paths, oversized payloads, and executable content.

Backups and restores require integrity validation and safe publication.

Exports MUST exclude credentials, unrestricted diagnostics, restricted content without permission, hidden truth in player-safe mode, and unsafe local paths.

## 16. Credential Storage

Provider credentials SHOULD use an operating-system credential vault or equivalent secure store.

Provider profiles should reference credential aliases rather than embed secrets.

Credentials must be replaceable without Campaign migration.

Credential failures must not expose the credential in logs or UI messages.

## 17. Logging Policy

Logs SHOULD be structured and minimal.

Default logs MAY include:

- OperationId;
- Campaign identifier;
- component;
- status;
- version;
- error code;
- duration;
- count metadata.

Logs MUST NOT contain by default:

- full prompts;
- full provider responses;
- API keys;
- encryption keys;
- complete biographies;
- complete transcripts;
- canonical Secrets;
- restricted Rule Knowledge;
- absolute source paths;
- raw imported files.

Developer diagnostics may enable richer payload retention only through explicit activation, clear warning, bounded retention, and redaction.

## 18. Telemetry

The MVP SHOULD NOT require remote telemetry.

Future telemetry must be controlled, documented, minimized, revocable, and free of narrative content by default.

Safe metrics include operation duration, error counts, context size, repair count, timeout rate, and event counts.

## 19. Persistence and Operation Security

Persistence MUST enforce:

- Campaign ownership;
- expected versions;
- unique OperationId;
- immutable raw Dice Rolls;
- finalization idempotency;
- exact Rule Set versions;
- visibility-aware queries;
- no direct UI writes.

Every Campaign-scoped query and write must validate Campaign ownership. Entity identity alone is insufficient.

State-changing operations require:

- OperationId;
- request fingerprint;
- expected versions;
- authorization context;
- idempotency;
- bounded retry.

## 20. Replay, Fake Dice, and Duplicate Effects

Replayed commands are detected through OperationId, fingerprint, accepted-result lookup, version checks, and unique constraints.

Only Chronicle's random generator creates authoritative values.

Narrative Intelligence and Rule Set packages cannot supply authoritative Dice results.

Progression, Memory aging, consequences, and finalization use uniqueness or idempotency keys to prevent duplicate application.

Provider responses become stale when Campaign, Scene, Session, Roll, operation, or contract versions change.

## 21. Threat Model

The initial threat model includes:

```text
Prompt Injection
Prompt Poisoning
Prompt Replay
Secret Leakage
Context Leakage
Cross-Campaign Reference
Fake Dice
Duplicate Operation
Package Tampering
Malicious Rule Package
Malicious Character Schema
Malicious Import
Corrupted Backup
Archive Bomb
Path Traversal
Credential Leakage
Log Leakage
Token Exhaustion
Memory Flooding
Relationship Explosion
Context Overflow
Stale Provider Response
Provider Data Retention
Restricted Content Redistribution
Unsafe Migration
Partial Finalization
Read Model Authorization
```

## 22. Threat Mitigations

### 22.1 Prompt Injection

- instruction/data separation;
- bounded text;
- output schemas;
- event allowlists;
- no unrestricted provider tools;
- full validation;
- no direct persistence.

### 22.2 Secret Leakage

- visibility filtering before ranking;
- least context;
- structured reveal permissions;
- output validation;
- safe projections;
- redacted logs.

### 22.3 Fake Dice

- Chronicle-owned randomness;
- immutable raw values;
- provider-result rejection;
- deterministic Rule Set interpretation.

### 22.4 Duplicate Operations

- OperationId;
- request fingerprint;
- unique constraints;
- committed-result lookup;
- idempotent retry.

### 22.5 Package Tampering

- manifest validation;
- integrity metadata;
- exact identity;
- no dynamic loading in MVP;
- contract tests.

### 22.6 Malicious Schema

- bounded types;
- no arbitrary executable expressions;
- depth limits;
- unknown-validator rejection;
- compilation validation.

### 22.7 Malicious Import

- isolated workspace;
- checksum validation;
- archive safety;
- no partial publication;
- dependency and identity validation.

### 22.8 Token Exhaustion

- fixed budgets;
- collection and field limits;
- omission policy;
- required-context failure;
- no full-Campaign dump.

### 22.9 Memory and Relationship Flooding

- proposal limits;
- evidence requirements;
- deduplication;
- relevance selection;
- finalization validation;
- active-status limits.

### 22.10 Restricted Content Redistribution

- provenance;
- license metadata;
- transmission policy;
- export filtering;
- original summaries;
- local-only user sources.

## 23. Security Failure Classification

Security failures SHOULD be classified as:

```text
Critical
High
Moderate
Low
Informational
```

Critical examples include credential exposure, cross-Campaign mutation, prohibited Secret disclosure, arbitrary code execution, corrupted restore publication, unauthorized mutation, and fake authoritative Dice results.

## 24. Incident Response

A security incident workflow SHOULD:

1. stop unsafe writes;
2. preserve safe diagnostics;
3. isolate the operation, provider, or package;
4. determine exposure;
5. restore trusted state if necessary;
6. add regression tests;
7. document remediation.

## 25. Security Error Model

Recommended errors include:

```text
AccessDenied
VisibilityViolation
CrossCampaignReference
CredentialUnavailable
CredentialExposurePrevented
PromptInjectionDetected
ContextPolicyViolation
ProviderTransmissionForbidden
PackageIntegrityFailed
SchemaSecurityViolation
ImportUnsafe
BackupIntegrityFailed
RestrictedContentViolation
OperationReplayDetected
```

User-facing messages must explain what was blocked without revealing hidden data or credentials.

## 26. Security Testing

Chronicle SHOULD include:

- unit tests;
- contract tests;
- adversarial fixtures;
- property tests;
- parser tests;
- import fuzzing;
- prompt injection tests;
- visibility tests;
- credential redaction tests;
- restore-failure tests;
- package-integrity tests.

Required cases include:

- hostile player input;
- biography containing system-like instructions;
- Rule Knowledge containing hostile commands;
- hidden Secret omitted from prompt;
- portrayal-only Secret constrained;
- provider revealing a prohibited Secret;
- cross-Campaign references;
- provider-generated Dice result;
- duplicate OperationId;
- stale Scene response;
- malicious package manifest;
- schema with arbitrary expression;
- path traversal;
- archive bomb;
- corrupted backup;
- wrong encryption password;
- credential redaction;
- restricted source blocked from remote transmission;
- oversized context rejection;
- player-safe export filtering.

## 27. Secure Development Requirements

Contributors SHOULD:

- keep secrets out of source control;
- minimize dependencies;
- monitor dependency vulnerabilities;
- review parsers carefully;
- avoid unsafe deserialization;
- avoid provider-controlled reflection;
- add regression tests for security defects;
- document new trust boundaries;
- update the threat model.

Chronicle MUST NOT deserialize untrusted data into arbitrary runtime types based on attacker-controlled class names.

## 28. Network Boundary

Remote provider communication SHOULD use secure transport.

Chronicle validates configured endpoint, provider profile, destination, timeout, and platform certificate behavior.

Imported content and providers MUST NOT specify arbitrary URLs for Chronicle to fetch in the MVP.

Future connectors require allowlists and request controls.

## 29. Privacy by Design

Privacy decisions occur before prompt construction, logging, backup, export, telemetry, and provider invocation.

Redaction after exposure is insufficient.

Retention must be explicit for provider payloads, failed imports, diagnostics, Operation Records, backups, exports, temporary files, and indexes.

Chronicle should not claim physical secure deletion where the operating system or storage technology cannot guarantee it.

## 30. MVP Minimum Security Requirements

The MVP MUST include:

- local-first persistence;
- credentials outside Campaign data;
- least-context prompt construction;
- structured-output validation;
- no direct provider tools;
- no provider persistence access;
- Chronicle-owned randomness;
- Rule Set isolation by contract;
- cross-Campaign validation;
- immutable raw Dice Rolls;
- idempotent operations;
- Secret visibility enforcement;
- bounded logging;
- raw prompt logging disabled by default;
- backup integrity validation;
- import archive safety;
- restricted-content transmission policy;
- no dynamic untrusted package loading.

## 31. Current Delivery Decision

The MVP adopts:

- explicit trust zones;
- Public, Internal, Private, Secret, Restricted, and Credential classifications;
- least privilege;
- least context;
- deny by default;
- provider-neutral DTO boundaries;
- instruction/data separation;
- no provider-direct retrieval;
- local-only default for restricted user sources;
- platform credential-store integration target;
- visibility filtering before provider calls;
- package manifest and integrity validation;
- isolated import and restore workspaces;
- structured minimal logging;
- no mandatory telemetry;
- no cloud sync;
- no dynamic external plugins;
- no arbitrary remote file fetching.

## 32. Architecture Horizon

Future evolution MAY include:

- sandboxed plugins;
- signed packages;
- encrypted database fields;
- secure cloud synchronization;
- user identity and tenant isolation;
- multiplayer authorization;
- security audit logs;
- privacy-preserving telemetry;
- secret scanning;
- external security review.

The MVP MUST NOT implement these capabilities without a later milestone.

## 33. Open Questions

The following remain open:

- Which operating-system credential stores will be supported first?
- Is database encryption required before first release?
- Which provider privacy options must be configurable?
- Should local providers run in a separate process?
- What package-integrity mechanism will be used?
- Which logs are retained in production builds?
- How long should failed provider payloads be retained?
- Should player-safe output validation use deterministic Secret matching?
- Which file formats are allowed in MVP?
- Must local Rule Knowledge indexing remain entirely offline?
- What content-safety policy belongs in Chronicle Core?
- Which incidents require automatic checkpoints?
- Should exports support password encryption in MVP?
- How will security advisories be published?
- Which threat-model format will live beside the codebase?

## 34. Compliance Checklist

An implementation complies when:

- Chronicle remains the authority for accepted state;
- providers are treated as untrusted;
- Rule Set packages cannot persist or access unrestricted infrastructure;
- data classification is explicit;
- least-context filtering occurs before provider invocation;
- Secrets are omitted unless required;
- use and reveal permissions are distinct;
- player-authored text remains untrusted data;
- provider output is fully validated;
- credentials remain outside Campaign data;
- logs exclude sensitive content by default;
- cross-Campaign references are rejected;
- Dice randomness remains Chronicle-owned;
- imports and backups are validated before publication;
- restricted content follows transmission and export policy;
- critical failures block unsafe writes;
- the threat model is maintained.

## 35. Final Principle

Chronicle assumes that every external component may fail, lie, become unavailable, or behave unpredictably.

Therefore, authoritative state is never delegated.

Chronicle owns truth.

Chronicle validates mechanics.

Chronicle controls persistence.

Chronicle decides what is remembered.

The AI tells stories.

Chronicle keeps history.
