---
id: ADR-0037
title: Provider Credential Storage, Secret Resolution, and Sensitive Configuration Handling
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
  - ADR-0014
  - ADR-0017
  - ADR-0019
  - ADR-0020
  - ADR-0021
  - ADR-0022
  - ADR-0026
  - ADR-0027
  - ADR-0032
  - ADR-0034
  - ADR-0035
  - ADR-0036
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

> **"A secret may unlock a provider. It must never become part of the Campaign, the prompt, the backup, or the story."**

# Provider Credential Storage, Secret Resolution, and Sensitive Configuration Handling

## 1. Status

**Proposed**

This ADR defines Chronicle's credential storage, secret references, runtime secret resolution, sensitive configuration classification, redaction, backup and export exclusion, provider-profile integration, rotation, deletion, diagnostics, and recovery behavior.

The decision is:

- store provider secrets only through an OS-backed secret-store abstraction;
- use Windows Credential Manager or the approved Windows secret mechanism for the official desktop MVP;
- store only opaque secret references in Chronicle's database and configuration;
- prohibit secret values from entering Domain, Application commands, Campaign aggregates, Work Items, Operation Records, logs, exports, backups, prompts, or provider-neutral DTOs;
- resolve secrets only inside the Infrastructure provider-adapter boundary;
- minimize secret lifetime in memory;
- prohibit persistence of resolved secrets in caches, static fields, files, crash metadata, or diagnostic bundles;
- classify configuration fields explicitly as public, private, sensitive, or secret;
- keep endpoint, model, provider-family, and policy metadata separate from secret material;
- require explicit credential creation, update, test, rotation, and deletion workflows;
- make credential deletion safe when provider profiles still reference the secret;
- preserve provider profiles during backup and restore while excluding secret values;
- mark restored profiles as requiring credential resolution when the destination machine lacks the referenced secret;
- redact known secret values and sensitive headers from logs and exceptions;
- never display full credentials after creation;
- avoid claiming perfect memory erasure in a managed runtime;
- make Development overrides explicit, isolated, and prohibited from Stable defaults;
- support deterministic fake credentials for automated tests without using real provider secrets;
- treat environment-variable credentials as an opt-in Development or deployment source, not the normal desktop storage mechanism;
- block provider invocation when secret resolution is missing, ambiguous, invalid, or policy-incompatible.

The decision becomes **Accepted** after a security spike proves:

- credential creation in the OS-backed store;
- opaque reference persistence;
- runtime resolution only inside the provider adapter;
- credential test without database persistence;
- rotation;
- deletion;
- missing-secret behavior after restore;
- log and exception redaction;
- backup and Campaign export exclusion;
- no secret presence in Work Items, Operation Records, prompt packages, crash artifacts, or provider-attempt records;
- Stable rejection of Development plaintext secret configuration;
- deterministic tests using a fake secret store.

## 2. Context

Chronicle may need credentials to access remote Narrative Intelligence providers.

Examples include:

- API keys;
- access tokens;
- bearer tokens;
- client secrets;
- provider-specific authentication values;
- future local-service authentication tokens.

These values are sensitive.

A credential leak could occur through:

- the local database;
- provider-profile JSON;
- logs;
- exception messages;
- diagnostic bundles;
- screenshots;
- backups;
- Campaign exports;
- clipboard handling;
- environment dumps;
- crash reports;
- prompt construction;
- provider-attempt metadata;
- Work Item payloads;
- automated tests;
- source control.

Chronicle is local-first, but local storage is not automatically safe.

The application must also handle:

- credential rotation;
- provider profile deletion;
- machine transfer;
- restore on another computer;
- missing or invalid credentials;
- multiple provider profiles;
- custom endpoints;
- Development environments;
- automated CI tests.

ADR-0036 defines provider profiles and provider adapters.

This ADR defines the secret boundary beneath them.

## 3. Decision Drivers

The secret design prioritizes:

1. credential confidentiality;
2. minimal exposure;
3. OS-native protection;
4. clear provider-profile integration;
5. backup and export safety;
6. redaction;
7. testability;
8. rotation;
9. failure transparency;
10. restore portability;
11. no false security claims;
12. bounded MVP scope.

## 4. Decision Summary

Chronicle will use:

```text
Secret Store
    OS-backed abstraction

Official Windows MVP
    Windows Credential Manager or approved Windows secret storage

Database
    opaque SecretReference only

Resolution
    Infrastructure adapter boundary

Provider-Neutral DTOs
    no secret values

Backup and Export
    exclude secret values

Restore
    preserve references
    require local reconfiguration when unresolved

Logging
    structured redaction

Testing
    fake in-memory secret store

Plaintext Files
    prohibited in Stable
```

## 5. Secret Definition

A secret is data that grants authentication, authorization, decryption, or privileged access.

## 6. Credential Definition

A credential is a secret or secret set used to authenticate with a provider or service.

## 7. Sensitive Configuration

Sensitive configuration may not itself grant access but still requires protection.

Examples:

- custom private endpoint;
- tenant identifier;
- account identifier;
- private deployment name;
- regional routing choice;
- internal provider project identifier.

## 8. Configuration Classification

Every configuration field SHOULD be classified as:

```text
Public
Private
Sensitive
Secret
```

## 9. Public Configuration

Safe to display and persist normally.

Examples:

- provider display name;
- public model key;
- nonprivate capability metadata.

## 10. Private Configuration

User-specific but not authentication material.

Examples:

- profile selection;
- preference for a provider;
- local health state.

## 11. Sensitive Configuration

May expose infrastructure or account metadata.

Examples:

- private endpoint host;
- tenant or deployment identifier;
- account project key.

## 12. Secret Configuration

Grants access or authorization.

Examples:

- API key;
- bearer token;
- refresh token;
- client secret.

## 13. Secret Store Port

Chronicle defines an Infrastructure-facing abstraction such as:

```text
ISecretStore
```

## 14. Conceptual Interface

```csharp
public interface ISecretStore
{
    Task<SecretReference> StoreAsync(
        SecretDescriptor descriptor,
        SecretValue secret,
        CancellationToken cancellationToken);

    Task<SecretResolutionResult> ResolveAsync(
        SecretReference reference,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        SecretReference reference,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(
        SecretReference reference,
        CancellationToken cancellationToken);
}
```

## 15. Secret Reference

A Secret Reference is an opaque identifier.

It must not contain the secret value.

## 16. Secret Reference Fields

Recommended fields:

```text
SecretReferenceId
SecretStoreKind
CredentialPurpose
ProviderProfileId
Version
CreatedAtUtc
LastRotatedAtUtc
```

The persisted form may be smaller, but it remains opaque to Application logic.

## 17. Secret Value Type

Secret values SHOULD use a dedicated type that:

- avoids accidental string interpolation;
- avoids default serialization;
- avoids ordinary equality logging;
- provides explicit access;
- supports bounded lifetime.

## 18. No Secret String in Application Contracts

Provider-neutral Application DTOs MUST NOT contain raw secret strings.

## 19. Provider Adapter Resolution

The provider adapter receives a Secret Reference and resolves it through the secret-store boundary.

## 20. Last Responsible Moment

Resolution occurs immediately before building the authenticated provider transport request.

## 21. Secret Lifetime

Resolved secrets remain in memory only as long as necessary for the provider call.

## 22. Managed Runtime Limitation

Chronicle cannot guarantee complete memory erasure in a managed runtime.

Documentation and UI must not claim otherwise.

## 23. Best-Effort Memory Handling

Implementation SHOULD:

- minimize copies;
- avoid interpolation;
- avoid converting to multiple strings;
- avoid static storage;
- avoid caching;
- release references promptly;
- use dedicated wrappers where practical.

## 24. No Secret Cache

Resolved secrets are not cached in:

- singleton dictionaries;
- provider profiles;
- Work Items;
- process-wide static fields;
- persistent HTTP headers;
- diagnostics.

## 25. HTTP Authorization

The adapter applies credentials to one outbound request or approved client handler scope.

## 26. Persistent HTTP Client

Long-lived HTTP clients may be reused.

Raw secret values should not be permanently attached to shared global client defaults when multiple profiles can use different credentials.

## 27. Request Header Scope

Authorization headers SHOULD be set per request.

## 28. Provider SDK Handling

When an SDK requires credential construction:

- the adapter creates the SDK credential object;
- the object remains Infrastructure-local;
- the SDK object is not stored in Application state;
- lifetime is bounded and documented.

## 29. Official Windows Store

The official desktop MVP uses the approved Windows OS-backed secret store.

## 30. Windows Credential Manager

Windows Credential Manager is the preferred initial implementation when it satisfies:

- current-user protection;
- opaque reference lookup;
- update and delete;
- local desktop compatibility;
- acceptable payload limits.

## 31. Alternative Windows Mechanism

A DPAPI-protected Chronicle-owned secret file MAY be selected only if the spike proves Credential Manager insufficient.

It requires a separate implementation note and equivalent exclusion rules.

## 32. No Plaintext Secret File

Stable Release MUST NOT persist secrets in plaintext JSON, YAML, INI, `.env`, database rows, or ordinary application settings.

## 33. Development Environment Variables

Environment variables MAY be supported for Development and CI.

## 34. Environment Variable Scope

Environment-variable secret sourcing must be:

- explicit;
- disabled by default in Stable desktop operation;
- clearly labeled;
- excluded from diagnostics;
- treated as externally managed.

## 35. Command-Line Secrets

Passing secrets through command-line arguments is prohibited.

### Rationale

Command-line values may be visible through process inspection, shell history, or crash metadata.

## 36. Clipboard

Credential setup may accept paste from clipboard.

Chronicle should not retain clipboard content after use.

## 37. Clipboard Clearing

Automatic clipboard clearing is not guaranteed and may be surprising.

The MVP should warn users rather than claim secure clipboard erasure.

## 38. Credential Creation Workflow

Recommended command:

```text
CreateProviderCredentialCommand
```

## 39. Creation Input Boundary

The raw secret enters through Presentation and is passed directly to the secret-store application service.

It is not persisted in the command payload or Operation Record.

## 40. Sensitive Command Handling

Commands carrying raw secrets must:

- avoid ordinary command logging;
- avoid durable serialization;
- avoid retry persistence;
- use one in-process execution boundary;
- return only a Secret Reference.

## 41. No Durable Work Item for Raw Secret

A raw credential MUST NOT be placed in a durable Work Item.

## 42. Creation Flow

Recommended flow:

1. validate provider profile and purpose;
2. accept secret through protected UI binding;
3. validate basic shape locally;
4. store in OS secret store;
5. receive opaque reference;
6. persist reference to provider profile;
7. optionally test provider access;
8. clear UI field state;
9. return safe status.

## 43. Credential Shape Validation

Local validation may check:

- nonempty;
- length bounds;
- known prefix when provider contract defines one;
- no surrounding whitespace;
- supported character constraints.

It must not log the value.

## 44. Credential Test

A provider credential test uses a minimal nonprivate provider request.

## 45. Test Input

The test must not include Campaign content.

## 46. Test Result

Recommended result:

```text
Valid
Invalid
Unauthorized
ProviderUnavailable
CapabilityMismatch
RateLimited
Unknown
```

## 47. Test Is Not Persistence Authority

A successful test does not replace profile validation or capability negotiation.

## 48. Credential Update

Updating a credential creates a new secret version or replaces the secret atomically according to the store implementation.

## 49. Rotation

Credential rotation SHOULD preserve:

- provider profile identity;
- secret reference stability when supported;
- rotation timestamp;
- safe audit metadata;
- no secret value history.

## 50. Secret History

Chronicle does not retain old credential values.

## 51. Rotation Operation

Recommended command:

```text
RotateProviderCredentialCommand
```

## 52. Rotation Flow

1. accept new secret;
2. store or replace securely;
3. test if requested;
4. update safe metadata;
5. invalidate provider health cache;
6. never persist old or new secret values.

## 53. Failed Rotation

If secure storage of the new secret fails, the old credential remains active when possible.

## 54. Deletion

Recommended command:

```text
DeleteProviderCredentialCommand
```

## 55. Deletion Preconditions

Before deletion, Chronicle inspects provider profiles referencing the Secret Reference.

## 56. Referenced Secret Deletion

Deletion requires explicit confirmation when active profiles still reference the secret.

## 57. Post-Deletion Profile State

Affected profiles become:

```text
CredentialMissing
ConfigurationRequired
```

## 58. Delete Is Not Secure Erase Claim

Chronicle requests deletion from the OS secret store.

It does not claim forensic secure erasure.

## 59. Provider Profile Persistence

Provider profiles persist only:

- Secret Reference;
- credential status metadata;
- rotation timestamp;
- last test status;
- no value.

## 60. Credential Status

Recommended states:

```text
Unknown
Configured
Missing
Invalid
Expired
Revoked
TestRequired
Unavailable
```

## 61. Expiration

If provider credentials have known expiration metadata, Chronicle may persist the expiration time.

The secret remains in the OS store.

## 62. Refresh Tokens

Automatic refresh-token handling is deferred unless required by the official provider.

## 63. OAuth

Interactive OAuth flows require a separate ADR.

## 64. API-Key MVP

The initial credential model targets static API-key-like secrets.

## 65. Multiple Secrets per Profile

A profile MAY reference multiple secrets when the adapter contract requires them.

Each secret has a separate purpose key.

## 66. Credential Purpose

Examples:

```text
api-key
client-secret
access-token
refresh-token
local-service-token
```

## 67. Secret Reference Scope

A Secret Reference should be scoped to:

- user;
- installation;
- provider profile;
- credential purpose.

## 68. Shared Secret

Sharing one Secret Reference across profiles is discouraged but may be supported explicitly.

## 69. Shared Secret Deletion

Deletion must report every referencing profile.

## 70. Backup

Backups include:

- provider profile metadata;
- opaque secret references;
- credential status metadata.

They exclude secret values.

## 71. Backup Manifest

The manifest SHOULD declare:

```text
CredentialsIncluded = false
```

## 72. Restore to Same Machine

Secret references may resolve if the OS secret store still contains them.

## 73. Restore to Another Machine

Secret references generally do not resolve.

Profiles become `CredentialMissing`.

## 74. Restore UX

Restore preview and post-restore diagnostics must explain which provider profiles need credentials.

## 75. Campaign Export

Campaign exports exclude:

- secret references where installation-specific;
- secret values always;
- credential status details not needed for portability.

## 76. Provider Preference in Export

A Campaign may export a provider-neutral preference or profile display reference.

It does not export authentication.

## 77. Import

Imported Campaigns never create or overwrite credentials.

## 78. Logs

Logs MUST redact:

- known secret values;
- authorization headers;
- provider API keys;
- secret-store payloads;
- credential setup input;
- full custom endpoint query strings when sensitive.

## 79. Structured Logging

Logging uses named properties and allowlists.

## 80. No Object Dump

Chronicle must not log entire provider request, SDK client, HTTP request, headers, profile entity, or exception data object without filtering.

## 81. Redaction Service

Chronicle SHOULD define a redaction capability for Infrastructure diagnostics.

## 82. Redaction Rules

Recommended behavior:

```text
Authorization header
    remove entirely

Known secret property
    replace with [REDACTED]

API key-like query parameter
    remove value

Secret Reference
    safe to log only as short opaque ID where needed

Endpoint
    log scheme and host according to sensitivity policy
```

## 83. Redaction Is Defense in Depth

The primary control is preventing secrets from entering log calls.

Redaction is secondary protection.

## 84. Exception Messages

Provider and secret-store exceptions are mapped to typed safe errors.

## 85. Provider Error Echo

Some providers may echo part of a credential or request header in an error.

Adapters must sanitize provider exception text.

## 86. Stack Traces

Stack traces remain restricted to protected Development diagnostics.

They must still avoid secret-bearing local variables where possible.

## 87. Crash Dumps

Chronicle does not claim that operating-system crash dumps cannot contain in-memory secrets.

## 88. Crash Dump Policy

Stable support guidance SHOULD discourage unrestricted crash-dump sharing when a provider call was active.

## 89. Diagnostic Bundles

Diagnostic bundles exclude:

- secret-store content;
- environment variables;
- authorization headers;
- raw provider clients;
- credential input;
- full memory dumps.

## 90. Diagnostic Credential Metadata

A bundle MAY include:

```text
Credential configured: yes/no
Credential test status
Secret store kind
Last rotation time
Safe error category
```

## 91. Prompt Construction

Prompt DTOs cannot reference secret values.

## 92. Provider Context

No credential metadata is sent to the provider except the authentication material applied by the adapter transport layer.

## 93. Rule Set Isolation

Rule Set packages have no access to Secret References or the secret store.

## 94. Narrative Intelligence Roles

Narrator and Archivist have no access to credential status or secret metadata.

## 95. Work Items

Work Item payloads contain:

- ProviderProfileId;
- workflow data;
- attempt metadata.

They do not contain secrets.

## 96. Operation Records

Operation Records may record credential workflow result codes.

They do not record secret values.

## 97. Secret Commands and Idempotency

Raw-secret creation and rotation commands cannot be safely replayed from durable payloads.

## 98. In-Process Idempotency

The UI should prevent duplicate submission.

The secret store and profile update should use a local transactional or compensating workflow.

## 99. Creation Commit Boundary

Secret storage and database profile update cannot generally share one atomic transaction.

## 100. Partial Failure: Secret Stored, Profile Update Failed

Chronicle should:

- delete the newly stored secret when safe;
- or preserve it as an orphaned secret reference for cleanup;
- record a safe recovery item;
- never expose the value.

## 101. Partial Failure: Profile Updated, Secret Missing

Profile validation detects the missing reference and blocks provider use.

## 102. Orphaned Secret Cleanup

Chronicle MAY maintain safe metadata for orphaned Secret References.

## 103. Orphan Cleanup

Cleanup verifies no profile references the secret before deletion.

## 104. Orphan Metadata

Recommended fields:

```text
SecretReferenceId
CreatedAtUtc
Purpose
ProviderProfileId
CleanupStatus
```

No secret value is stored.

## 105. Secret Store Unavailable

If the OS secret store is unavailable:

- provider invocation is blocked;
- Campaign data remains usable;
- profile status becomes unavailable;
- user receives a typed recovery message.

## 106. Locked User Session

If secret resolution fails because the OS session is locked or unavailable, Chronicle does not fall back to plaintext storage.

## 107. Permission Failure

Permission failures map to:

```text
secret.permission-denied
```

## 108. Missing Secret

Missing references map to:

```text
secret.not-found
```

## 109. Invalid Reference

Malformed or unsupported references map to:

```text
secret.reference-invalid
```

## 110. Secret Store Kind Mismatch

A restored reference to an unsupported store kind becomes unresolved.

## 111. Secret Migration

Moving secrets between store implementations requires an explicit migration workflow.

## 112. Migration Input

Migration resolves the old secret and stores it in the new store within one in-process operation.

## 113. Migration Failure

The old secret remains until the new reference is verified.

## 114. Development Secret Store

Development MAY use:

- environment-variable adapter;
- in-memory fake store;
- local developer secret manager.

## 115. Development Warning

Development secret stores must be visibly non-production.

## 116. Stable Guard

Stable startup MUST reject known Development plaintext secret-store configuration unless an explicit unsupported override is enabled outside normal UX.

## 117. Source Control

No credential file is generated into the repository.

## 118. Example Configuration

Sample configuration uses placeholders only.

## 119. CI

CI uses ephemeral test secrets or fake providers.

## 120. Real Provider Integration Tests

Real provider tests are opt-in and run only when externally supplied secrets are available.

## 121. Test Output

Tests must never print supplied credentials.

## 122. Fake Secret Store

Automated tests use a deterministic in-memory fake.

## 123. Fake Store Capabilities

The fake store supports:

- create;
- resolve;
- update;
- delete;
- missing secret;
- permission failure;
- unavailable store;
- rotation;
- orphan cleanup.

## 124. Synthetic Canary Secrets

Security tests SHOULD use synthetic canary values and scan:

- database;
- logs;
- backup;
- Campaign export;
- diagnostic bundle;
- Work Item payload;
- Operation Record;
- prompt package;
- provider attempt metadata.

## 125. Canary Test Success

No artifact may contain the synthetic secret.

## 126. UI Credential Entry

Credential fields should:

- use password-style masking;
- prevent accidental display;
- avoid browser-like autocomplete where inappropriate;
- offer explicit paste;
- not show stored value after save.

## 127. Reveal Secret

The MVP does not provide a “show saved credential” function.

## 128. Replace Secret

Users replace or rotate credentials rather than retrieving them.

## 129. Copy Secret

Chronicle does not offer a copy-existing-secret action.

## 130. Credential Status UI

The UI may show:

```text
Configured
Last tested
Last rotated
Current health
Requires reconfiguration
```

## 131. Error UX

Credential errors should distinguish:

- missing;
- invalid;
- revoked;
- provider unavailable;
- secret store unavailable;
- permission denied;
- profile capability mismatch.

## 132. Safe Error Reference

User-facing errors include a safe reference code.

## 133. Custom Endpoint Sensitivity

Custom endpoint URLs may contain sensitive hostnames, paths, or query parameters.

## 134. Endpoint Storage

Endpoints are stored separately from secrets.

## 135. Endpoint Query Secrets

Endpoints containing credential query parameters are rejected.

## 136. Embedded Credentials in URL

URLs such as:

```text
https://user:password@example.invalid
```

are rejected.

## 137. Endpoint Logging

Logs may record only the allowed sanitized endpoint representation.

## 138. Headers

Custom secret headers require dedicated secret references.

They are not stored as arbitrary profile dictionaries.

## 139. Multiple Secret Headers

An adapter contract must define each supported secret-bearing header purpose explicitly.

## 140. Sensitive Settings History

Chronicle may retain change history for metadata.

It must not retain historical secret values.

## 141. Audit Metadata

Safe audit fields include:

```text
ProfileId
Credential purpose
Secret store kind
Created time
Rotated time
Deleted time
Operation result
```

## 142. User Identity

The local MVP does not require Chronicle account identity for secret storage.

OS user scope is the primary boundary.

## 143. Multi-User Machine

Credentials stored under one OS user must not be assumed available to another OS user.

## 144. Machine Transfer

Campaign data may transfer independently from credentials.

This is intentional.

## 145. Portable Mode

A future portable installation cannot reuse the same secret assumptions without a separate security design.

## 146. Encryption at Rest

Chronicle relies on the OS-backed secret store for credential protection.

The general Chronicle database remains governed by separate storage decisions.

## 147. Secret Encryption Claim

Chronicle must state only what the selected OS store guarantees.

## 148. Threat Model

This ADR protects against:

- accidental database inclusion;
- backup leakage;
- log leakage;
- prompt leakage;
- ordinary local file inspection;
- accidental source-control inclusion;
- provider-profile export leakage.

## 149. Threat Model Limits

This ADR does not fully protect against:

- compromised OS account;
- malicious process with equivalent user privileges;
- memory scraping during active use;
- administrator or kernel compromise;
- malicious first-party binary;
- unrestricted crash-memory capture.

## 150. No False Assurance

Documentation must communicate these limits accurately.

## 151. Error Model

Recommended errors:

```text
secret.store-unavailable
secret.store-unsupported
secret.permission-denied
secret.not-found
secret.reference-invalid
secret.value-invalid
secret.create-failed
secret.resolve-failed
secret.rotate-failed
secret.delete-failed
secret.in-use
secret.orphan-cleanup-failed
secret.development-store-blocked
secret.recovery-required
credential.missing
credential.invalid
credential.revoked
credential.expired
credential.test-failed
```

## 152. Data Preservation State

Results SHOULD state:

```text
CampaignDataUnchanged
ProviderProfileUnchanged
SecretStored
SecretReferencePersisted
OldSecretPreserved
ProfileRequiresCredential
OrphanedReferenceRecorded
SecretDeleted
RecoveryRequired
```

## 153. Logging Policy

Secret operations log only:

- operation category;
- Secret Reference short ID;
- provider profile ID;
- store kind;
- result code;
- duration.

## 154. Metrics

Useful metrics include:

```text
SecretCreateCount
SecretResolveFailureCount
SecretRotationCount
SecretDeletionCount
CredentialMissingCount
CredentialInvalidCount
SecretStoreUnavailableCount
RedactionFailureCount
OrphanSecretCount
```

## 155. Redaction Failure

Any detected secret in a generated artifact is a release-blocking security defect.

## 156. Testing Strategy

The implementation requires:

```text
Secret Store Contract Tests
Windows Integration Tests
Provider Adapter Tests
Backup and Export Tests
Logging Tests
Diagnostic Bundle Tests
Failure Injection Tests
UI Tests
Architecture Tests
Security Scans
```

## 157. Secret Store Contract Tests

Tests MUST cover:

- store;
- resolve;
- update;
- delete;
- missing reference;
- invalid reference;
- permission failure;
- unavailable store;
- duplicate purpose;
- cancellation.

## 158. Windows Integration Tests

Tests SHOULD prove current-user storage and retrieval using disposable test credentials.

## 159. Provider Adapter Tests

Tests MUST prove secret resolution occurs only inside Infrastructure and only for the active request.

## 160. Backup Tests

Synthetic secrets must be absent from backup archives.

## 161. Export Tests

Synthetic secrets and machine-bound secret references must be absent from Campaign exports.

## 162. Log Tests

Tests MUST scan:

- structured logs;
- exception logs;
- retry logs;
- provider errors;
- health diagnostics.

## 163. Prompt Tests

Prompt packages and provider context must contain no secret values or secret-store metadata.

## 164. Work Item Tests

Durable payloads must contain no raw secrets.

## 165. Operation Record Tests

Operation Records must contain no raw secrets.

## 166. Crash Recovery Tests

Tests SHOULD simulate:

- secret stored before profile update;
- profile updated before secret verification;
- process crash during rotation;
- store unavailable after restart;
- orphan cleanup.

## 167. UI Tests

Tests MUST cover:

- masked entry;
- no saved-value reveal;
- rotation;
- missing credential;
- restore requiring reconfiguration;
- accessible labels and error states.

## 168. Stable Guard Tests

Stable configuration must reject plaintext Development stores.

## 169. Required Test Cases

Tests MUST cover:

- create credential;
- test credential;
- invalid credential;
- rotate credential;
- delete unused credential;
- delete referenced credential;
- backup;
- restore on same machine;
- restore on different machine simulation;
- Campaign export;
- provider call;
- custom endpoint;
- secret header;
- environment-variable Development mode;
- fake secret store;
- log redaction;
- diagnostic bundle;
- no prompt leakage.

## 170. Architecture Tests

Architecture tests MUST reject:

- secret fields in Domain entities;
- secret fields in Application DTOs;
- secret values in provider profiles;
- raw secret in Work Item payload;
- raw secret in Operation Record;
- raw secret in backup DTO;
- raw secret in Campaign export contract;
- secret-store access from Rule Set code;
- secret-store access from Presentation except through credential workflow;
- provider adapter logging authorization headers;
- plaintext Stable secret configuration.

## 171. Prohibited Patterns

### 171.1 API Key in Database

Persist only an opaque Secret Reference.

### 171.2 API Key in `.env` for Stable Desktop

Use the OS-backed secret store.

### 171.3 Raw Secret in Durable Command Payload

Credential operations remain short in-process workflows.

### 171.4 Provider Profile Exports Credential

Profiles export metadata only.

### 171.5 Show Saved Credential

Support replacement, not retrieval.

### 171.6 Log Full HTTP Request

Use allowlisted safe metadata.

### 171.7 Cache Credential Globally

Resolve at the adapter boundary.

### 171.8 Put Credential in URL

Reject embedded or query-string secrets.

### 171.9 Claim Secure Erasure

State the OS and managed-runtime limitations.

### 171.10 Fall Back to Plaintext When Secret Store Fails

Block provider use and report recovery.

## 172. Alternatives Considered

### Store Encrypted Secrets in SQLite

Rejected for MVP because key management would still require a secure external root and would increase implementation risk.

### Plaintext Local Configuration

Rejected because backups, file inspection, diagnostics, and source-control mistakes would expose credentials.

### Environment Variables as Primary Desktop Storage

Rejected because they are awkward for users and may leak through process or system diagnostics.

### Ask for Credential Every Launch

Reduces persistence risk but produces poor UX and still leaves clipboard and memory concerns.

### One Global Provider Credential

Rejected because multiple provider profiles and future provider families require explicit scope.

### Reveal Saved Secret in UI

Rejected because Chronicle should not retrieve secrets for display.

## 173. Consequences

### Positive

- credentials remain outside Campaign data;
- backups and exports are safer;
- provider adapters receive secrets only when needed;
- machine transfer does not leak authentication;
- rotation and deletion are explicit;
- logs and diagnostics have a clear redaction policy;
- fake-store testing is practical;
- provider profiles remain portable as metadata.

### Negative

- secret storage and database updates cannot share one ordinary transaction;
- restore requires credential reconfiguration;
- OS-specific implementation is required;
- managed-runtime memory cannot be perfectly erased;
- Development and Stable paths must remain distinct;
- support diagnostics cannot inspect credentials directly.

## 174. Risks

### Secret Stored but Profile Update Fails

Mitigation:

- compensating deletion;
- orphan-reference metadata;
- cleanup workflow.

### Secret Appears in Provider Exception

Mitigation:

- adapter sanitization;
- allowlisted error mapping;
- canary tests.

### Restore Leaves Broken Profiles

Mitigation:

- explicit credential status;
- restore preview;
- reconfiguration workflow.

### Developer Enables Plaintext Store Accidentally

Mitigation:

- Stable guard;
- release tests;
- Development labeling.

### OS Store Is Unavailable

Mitigation:

- block provider use;
- preserve Campaign access;
- typed recovery;
- no plaintext fallback.

## 175. Technology Spike

Before acceptance, implement:

1. `ISecretStore`;
2. Windows secret-store adapter;
3. deterministic fake store;
4. Secret Reference value type;
5. provider-profile credential metadata;
6. create workflow;
7. rotate workflow;
8. delete workflow;
9. credential test flow;
10. provider-adapter resolution;
11. orphan cleanup;
12. backup and export exclusion;
13. restore reconfiguration state;
14. redaction service;
15. synthetic canary artifact scans.

## 176. Spike Acceptance

The spike passes when:

- a real disposable credential can be stored and resolved under the current Windows user;
- only an opaque reference reaches the database;
- provider calls resolve secrets only inside Infrastructure;
- raw secrets never enter durable workflow payloads;
- rotation preserves profile identity;
- deletion produces correct profile state;
- restore without the OS secret produces a clear reconfiguration requirement;
- backups, exports, logs, prompts, diagnostics, and Operation Records contain no canary secret;
- Stable blocks plaintext Development storage;
- the fake store supports deterministic automated tests.

## 177. Definition of Compliance

An implementation complies when:

- provider secrets use an OS-backed secret store;
- Chronicle persists only opaque references;
- secret resolution occurs only inside provider adapters;
- resolved values have bounded lifetime and are not cached;
- provider-neutral contracts contain no raw secrets;
- raw-secret workflows are not durably serialized;
- credentials are excluded from backup, export, prompt, logs, diagnostics, Work Items, and Operation Records;
- rotation, deletion, and missing-secret states are explicit;
- restore preserves profiles but requires local secret reconfiguration when necessary;
- Development secret sources are isolated and blocked in Stable defaults;
- redaction and canary tests protect every generated artifact;
- security claims remain honest about OS and memory limitations.

## 178. Review Triggers

This ADR must be reviewed if:

- OAuth becomes required;
- refresh-token rotation is introduced;
- server hosting centralizes user credentials;
- multiple OS platforms are supported;
- portable installations are introduced;
- account synchronization includes provider profiles;
- encrypted database secret storage is proposed;
- hardware-backed keys are required;
- organization-managed credentials are introduced;
- provider tools require additional secret-bearing headers.

## 179. Deferred Decisions

Later ADRs MAY define:

- OAuth authorization-code flow;
- refresh-token handling;
- macOS Keychain adapter;
- Linux Secret Service adapter;
- hardware-backed credential storage;
- organization credential policies;
- portable-mode secrets;
- encrypted secret export;
- user account synchronization;
- provider credential sharing;
- exact Windows storage implementation if the spike changes the preferred mechanism.

## 180. Final Decision

Chronicle will keep provider credentials outside Campaign state, outside ordinary configuration, and outside every portable artifact.

Only opaque references will be persisted.

Secrets will be resolved at the Infrastructure provider boundary through the operating system's protected secret storage.

A credential may allow Chronicle to speak with a provider.

It will never become part of what Chronicle remembers.
