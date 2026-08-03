---
id: ADR-0008
title: Credential Store Implementations
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
  - ADR-0006
  - ADR-0007
  - RFC-0020
  - RFC-0035
  - RFC-0037
  - RFC-0038
  - RFC-0040
  - RFC-0041
  - RFC-0042
---

> **"Chronicle may remember a credential reference. It must never remember the credential itself as Campaign history."**

# Credential Store Implementations

## 1. Status

**Proposed**

This ADR defines Chronicle's credential-storage abstraction and selects the initial Windows implementation.

The decision is:

- define an `ISecretsManager` port in the Application or Contracts boundary;
- implement the first production adapter using **Windows Credential Manager**;
- store only credential references in Chronicle configuration;
- retrieve credential values only at the transport boundary;
- never persist credential values in SQLite, Campaign data, logs, backups, exports, prompts, diagnostics, or command-line arguments;
- provide a development-only in-memory or environment-backed implementation that is explicitly excluded from Release builds;
- require platform-specific secure adapters before claiming support on Linux or macOS.

The decision becomes **Accepted** after a security spike proves:

- create;
- replace;
- retrieve;
- delete;
- missing credential behavior;
- Windows account isolation;
- upgrade persistence;
- uninstall preservation;
- no credential leakage through configuration, logs, dumps, diagnostic bundles, or provider DTOs;
- safe behavior when the credential store is unavailable.

## 2. Context

Chronicle requires credentials for external integrations such as:

- OpenAI API;
- future Narrative Intelligence providers;
- future remote Rule Knowledge services;
- future cloud or synchronization services.

Credentials are highly sensitive and must remain outside:

- Campaign aggregates;
- Character data;
- provider-neutral request DTOs;
- SQLite persistence;
- backup and export artifacts;
- logs;
- diagnostics;
- source control.

ADR-0005 selects OpenAI as the first provider.

ADR-0006 selects Windows x64 as the first fully supported platform and Windows Credential Manager as the first secure credential mechanism.

This ADR defines the implementation contract and lifecycle.

## 3. Decision Drivers

The credential architecture prioritizes:

1. no plaintext production storage;
2. operating-system account protection;
3. minimal credential lifetime in memory;
4. provider neutrality;
5. safe replacement and deletion;
6. no credential migration with Campaign data;
7. testability;
8. clear unavailable-store behavior;
9. future platform adapters;
10. low MVP operational complexity.

## 4. Decision Summary

Chronicle will use:

```text
Application Port
    ISecretsManager

Production Windows Adapter
    WindowsCredentialManagerSecretsManager

Stored Chronicle Configuration
    CredentialReference only

Credential Value Retrieval
    At provider transport boundary
    On demand
    Bounded lifetime

Development Adapter
    InMemorySecretsManager
    Optional EnvironmentSecretsManager
    Never enabled silently in Release

Backup and Export
    Credential references excluded or sanitized
    Credential values always excluded

Uninstall
    Credentials preserved by default

Provider Configuration
    References credential alias
    Never embeds secret value
```

## 5. Terminology

### Credential

A secret value used to authenticate to an external service.

Examples:

- API key;
- access token;
- client secret;
- future refresh token.

### Credential Reference

A stable nonsecret identifier used by Chronicle to request a credential from the secure store.

Example:

```text
credential://providers/openai/default
```

### Credential Alias

A human-manageable name inside the Chronicle namespace.

Example:

```text
openai/default
```

### Secrets Manager

The Chronicle port that creates, retrieves, replaces, deletes, and inspects credential metadata without exposing storage-specific details.

## 6. Application Port

Conceptually:

```csharp
public interface ISecretsManager
{
    Task<SecretStoreAvailability> GetAvailabilityAsync(
        CancellationToken cancellationToken);

    Task<SecretMetadata?> GetMetadataAsync(
        SecretReference reference,
        CancellationToken cancellationToken);

    Task<SecretLease> GetSecretAsync(
        SecretReference reference,
        CancellationToken cancellationToken);

    Task<StoreSecretResult> StoreSecretAsync(
        SecretReference reference,
        SecretValue value,
        SecretMetadata metadata,
        CancellationToken cancellationToken);

    Task<DeleteSecretResult> DeleteSecretAsync(
        SecretReference reference,
        CancellationToken cancellationToken);
}
```

The exact API may differ, but the semantics are mandatory.

## 7. Port Ownership

The port belongs outside platform-specific Infrastructure.

It MUST NOT reference:

- Windows credential types;
- registry APIs;
- provider SDK types;
- Avalonia controls;
- SQLite;
- Serilog;
- concrete encryption libraries.

## 8. Provider Neutrality

Credential references are not provider-specific types.

Provider profiles may associate a credential reference with a provider adapter.

Example:

```text
ProviderProfile
    ProviderKey = "openai"
    CredentialReference = "credential://providers/openai/default"
```

## 9. Secret Reference Format

Credential references SHOULD use a stable URI-like syntax.

Recommended form:

```text
credential://{scope}/{provider}/{alias}
```

Initial scope:

```text
providers
```

Examples:

```text
credential://providers/openai/default
credential://providers/openai/testing
credential://providers/anthropic/private
```

## 10. Reference Validation

References MUST be validated for:

- supported scheme;
- allowed scope;
- normalized provider key;
- normalized alias;
- length;
- forbidden path traversal;
- control characters;
- reserved names.

## 11. Reference Case

Reference keys SHOULD use a documented normalization strategy.

Recommended:

- scheme and provider keys are lowercase;
- aliases preserve display casing separately if needed;
- storage identity uses normalized invariant form.

## 12. Secret Value Type

Secret values SHOULD use a dedicated type rather than ordinary strings where practical.

The type SHOULD:

- discourage logging;
- avoid accidental serialization;
- expose value only through an explicit method;
- support disposal or buffer clearing where feasible;
- have a safe `ToString()` implementation.

## 13. Secret Value Serialization

Secret values MUST NOT be serializable by ordinary JSON configuration or persistence code.

Any attempted serialization SHOULD fail or emit a protected placeholder.

## 14. Secret Lease

Retrieval SHOULD return a bounded `SecretLease` or equivalent.

The lease:

- contains the secret value;
- has explicit disposal;
- is scoped to one external call or short operation;
- must not be cached in long-lived services.

## 15. Memory Lifetime

Credentials SHOULD remain in managed memory only as long as needed.

Chronicle acknowledges that .NET cannot guarantee perfect secret erasure from all managed-memory copies.

Mitigations include:

- no unnecessary copies;
- no string interpolation;
- no exception messages containing values;
- no persistence;
- short lease lifetime;
- transport-boundary retrieval.

## 16. Retrieval Boundary

The provider adapter retrieves the credential immediately before constructing the authenticated transport request.

Application commands and provider-neutral requests carry only the credential reference or provider profile identifier.

## 17. No ViewModel Retrieval

Desktop ViewModels MUST NOT retrieve credential values.

The Settings UI may:

- accept a new secret value;
- submit it through a protected command;
- receive success or safe failure;
- display metadata only afterward.

## 18. No Redisplay

After storing a credential, Chronicle MUST NOT redisplay its value.

The UI may show:

- alias;
- provider;
- created or updated timestamp;
- health state;
- last successful validation time;
- masked status such as `Configured`.

## 19. Windows Production Adapter

The initial production implementation is:

```text
WindowsCredentialManagerSecretsManager
```

It uses Windows Credential Manager through an approved .NET interop or library boundary.

## 20. Windows Credential Type

The implementation SHOULD use a credential type appropriate for application-owned generic secrets.

The exact Windows API mapping requires the implementation spike.

## 21. Windows Target Naming

The Windows target name SHOULD derive from the normalized Chronicle credential reference.

Example conceptual target:

```text
Chronicle/providers/openai/default
```

Target names MUST avoid user-controlled unrestricted values.

## 22. Windows Account Isolation

Credentials stored by the Windows adapter are scoped to the current Windows user according to operating-system behavior.

Chronicle must not claim cross-user secrecy beyond Windows account protection.

## 23. Credential Persistence

Credentials SHOULD survive:

- Chronicle restart;
- application upgrade;
- repair installation;
- default uninstall.

They are not tied to the application binary directory.

## 24. Uninstall Behavior

Default uninstall preserves credentials.

Reasons:

- uninstall may be temporary;
- Campaign data is preserved;
- silent credential deletion could lock users out of configured providers.

A later full-data-removal workflow may offer explicit credential deletion.

## 25. Delete Behavior

Credential deletion requires explicit user intent.

Deleting a credential:

- removes the value from the secure store;
- leaves provider profile metadata in a degraded or unconfigured state unless the user removes it;
- does not delete Campaign data;
- does not alter historical provider-operation metadata.

## 26. Replace Behavior

Replacing a credential SHOULD be atomic from Chronicle's perspective.

The old value remains usable until the new value is safely stored, when the platform API permits.

If replacement fails:

- the old credential should remain;
- configuration should not claim the new credential is active;
- a safe error is returned.

## 27. Create Behavior

Creating a credential with an existing reference requires explicit replace semantics.

Silent overwrite is prohibited.

## 28. Retrieval Failure

Retrieval may fail because:

- reference is missing;
- credential store is unavailable;
- access is denied;
- stored data is corrupt;
- platform API fails;
- current user context changed.

The error MUST be mapped to a provider-neutral secret-store error.

## 29. Availability Model

The Secrets Manager SHOULD expose:

```text
Available
Unavailable
Degraded
UnsupportedPlatform
AccessDenied
Unknown
```

## 30. Secret Metadata

Safe metadata MAY include:

```text
SecretReference
ProviderKey
Alias
Exists
CreatedAtUtc when available
UpdatedAtUtc when available
StoreKind
Health
LastValidatedAtUtc
```

It MUST NOT include:

- secret length if it could be sensitive;
- secret prefix;
- last characters;
- hash usable for offline guessing;
- raw Windows target details beyond safe normalized identity.

## 31. Validation Workflow

Chronicle MAY validate a stored credential through a bounded provider-profile test.

The Secrets Manager itself does not call providers.

Validation flow:

```text
Settings UI
    → TestProviderProfileCommand
    → Provider Adapter
    → ISecretsManager retrieval
    → Bounded provider request
    → Safe result
```

## 32. Validation Persistence

Chronicle MAY persist safe metadata:

- last test time;
- test success;
- safe error code;
- provider-profile version.

It MUST NOT persist the secret.

## 33. Configuration

Provider configuration stores:

```text
ProviderProfileId
ProviderKey
CredentialReference
Endpoint
ModelProfileMappings
Timeouts
DataHandlingPolicy
Enabled
```

No secret values appear.

## 34. SQLite Boundary

The SQLite database MAY store credential references.

It MUST NOT store:

- API keys;
- tokens;
- encrypted credential blobs;
- Windows Credential Manager payloads;
- recoverable secret hashes.

## 35. Backup Boundary

Backups MUST exclude credential values.

Credential references MAY be included only when needed to preserve configuration, with clear metadata that credentials must exist or be reconfigured on restore.

## 36. Restore Behavior

After restore on the same machine:

- matching credential references may resolve normally.

After restore on another machine or user account:

- references may be unresolved;
- provider profiles become unconfigured;
- Campaign data remains readable;
- the user is prompted to add credentials.

## 37. Export Boundary

Portable Campaign exports SHOULD omit provider profiles and credential references unless the export contract explicitly includes nonsecret runtime preferences.

Credential values are always excluded.

## 38. Diagnostic Boundary

Diagnostic bundles may include:

- provider profile identifier;
- credential reference hash or safe alias where useful;
- exists/not configured status;
- store availability.

They MUST NOT include credential values or Windows credential payloads.

## 39. Logging

Logging MAY include:

- credential reference identifier if classified safe;
- operation type;
- store kind;
- result state;
- safe error code;
- duration.

Logging MUST NOT include:

- secret value;
- secret prefix or suffix;
- authorization header;
- raw Windows credential structure;
- provider request header.

## 40. `ToString()` Safety

Credential-bearing types MUST override or design `ToString()` so the secret value is never returned.

Recommended output:

```text
[REDACTED SECRET]
```

## 41. Exception Safety

Secret-store exceptions may contain:

- target names;
- platform error messages;
- user names;
- system paths.

The Windows adapter MUST sanitize them before passing to logging or UI layers.

## 42. Clipboard

Chronicle MUST NOT copy stored credentials to the clipboard.

The credential-entry form MAY allow standard paste behavior for user input.

## 43. Credential Entry Control

The UI SHOULD use a protected text-entry control with:

- masked display;
- reveal only while explicitly held or toggled, if supported and approved;
- no automatic spellcheck;
- no autocomplete unless platform behavior is safe;
- no persistence of draft after submission;
- no analytics or logging.

## 44. Credential Draft

Before storage, a credential exists as a transient UI draft.

The draft:

- remains Presentation-local until command submission;
- is cleared after success;
- is cleared after explicit cancel;
- is not stored in navigation state;
- is not included in crash recovery;
- is not persisted in application settings.

## 45. Command Boundary

A credential-setting command is exceptional because it carries a secret transiently.

It MUST:

- avoid ordinary command serialization;
- avoid Operation Record request payload persistence;
- store only a safe fingerprint or no fingerprint when riskier;
- avoid logging;
- invoke the secure adapter immediately;
- return metadata only.

## 46. Idempotency

Credential storage operations require careful idempotency.

A repeated user submission SHOULD either:

- return that the desired reference is configured;
- or explicitly replace according to the original operation intent.

The Operation Record MUST NOT store the secret value.

## 47. Request Fingerprint

If a fingerprint is required, it MUST use a one-way keyed mechanism or nonsecret operation metadata that does not enable secret guessing.

Plain hashes of low-entropy credentials are prohibited.

The simplest MVP policy may avoid fingerprinting the secret value entirely and scope idempotency to the operation state plus reference.

## 48. Development In-Memory Adapter

Chronicle will provide:

```text
InMemorySecretsManager
```

for deterministic tests.

It:

- never writes to disk;
- supports create, replace, retrieve, and delete;
- can simulate failure;
- clears values at process end;
- is not registered in Release builds.

## 49. Environment Adapter

An optional:

```text
EnvironmentSecretsManager
```

MAY exist for local development or CI.

It is permitted only when:

- explicitly selected;
- documented as development-only;
- variable names are configured, not values;
- environment values are never logged;
- Release builds do not silently fall back to it.

## 50. Environment Risks

Environment variables may leak through:

- process inspection;
- crash dumps;
- CI logs;
- child processes;
- shell history.

Therefore they are not the default production store.

## 51. Plaintext File Adapter

A plaintext file credential adapter is prohibited in production.

A local development-only file adapter is discouraged and requires explicit approval if ever introduced.

## 52. Encrypted File Fallback

An application-managed encrypted file is not selected for MVP.

It would require decisions for:

- key derivation;
- master password;
- key storage;
- recovery;
- portability;
- platform integration;
- cryptographic library review.

## 53. Unsupported Platform Behavior

If Chronicle runs on a platform without an accepted secure store:

- provider configuration that requires credentials is unavailable;
- the UI explains the unsupported secure-store state;
- Campaign data remains readable;
- Chronicle MUST NOT silently store secrets in plaintext.

## 54. Linux Future Adapter

A future Linux adapter SHOULD evaluate:

- Secret Service API;
- GNOME Keyring;
- KWallet compatibility;
- headless environments;
- unavailable desktop session;
- distribution variation.

Linux support is not complete until the adapter passes the contract suite.

## 55. macOS Future Adapter

A future macOS adapter SHOULD use Keychain Services or an accepted equivalent.

macOS support is not complete until credential storage passes:

- signing;
- entitlement;
- account isolation;
- upgrade;
- uninstall;
- contract tests.

## 56. Credential Store Contract Suite

All production adapters MUST pass a common conformance suite.

## 57. Contract Requirements

The suite MUST verify:

- availability;
- create;
- retrieve;
- replace;
- delete;
- missing value;
- duplicate create;
- cancellation;
- concurrent access;
- special-character alias handling;
- normalization;
- process restart persistence;
- account or store isolation where testable;
- safe metadata;
- no value in exceptions;
- no value in logs;
- no value in diagnostics;
- unavailable-store behavior.

## 58. Concurrency

Credential operations SHOULD be safe under concurrent requests.

The same reference must not produce corrupted state.

Chronicle MAY serialize writes per credential reference.

## 59. Cancellation

Cancellation before a platform call should stop the operation.

Cancellation after the platform has committed may produce an ambiguous result.

Chronicle must inspect metadata before retrying blindly.

## 60. Ambiguous Store Result

If storage completion is unknown:

1. query metadata or attempt a controlled retrieval;
2. determine whether the credential exists;
3. never display the value;
4. return a safe status;
5. require explicit retry when uncertain.

## 61. Credential Rotation

Credential rotation is implemented as replace.

Chronicle SHOULD support:

- storing the new value;
- testing it;
- preserving the old value until successful replacement where practical;
- updating validation metadata.

Provider-side key revocation remains external.

## 62. Multiple Credentials

Chronicle MAY support multiple aliases per provider.

Examples:

```text
default
personal
testing
low-cost
```

The provider profile selects one reference.

## 63. Credential Scope

MVP credentials are application-user scoped.

Campaign-specific credentials are discouraged.

A Campaign may reference a provider profile, but credential ownership remains application configuration.

## 64. Shared Computers

Chronicle assumes one operating-system user account per local credential boundary.

Shared-computer deployments require separate Windows accounts or future account architecture.

## 65. Remote Desktop

Credential access under remote desktop follows Windows user context.

Chronicle does not add a second credential-sharing mechanism.

## 66. Child Processes

Credentials MUST NOT be passed to child processes through:

- command-line arguments;
- environment variables by default;
- temporary files;
- standard output.

A future local-provider process should not need remote API credentials unless explicitly designed.

## 67. Provider SDK Configuration

Provider clients SHOULD receive the credential through in-memory transport configuration.

They MUST NOT write provider SDK config files containing secrets.

## 68. HTTP Header Construction

The adapter constructs authorization headers immediately before the request.

Headers are not stored in request diagnostics.

## 69. HTTP Client Lifetime

Long-lived `HttpClient` instances MAY be reused.

Credential-bearing headers SHOULD be request-scoped rather than permanent default headers where that reduces leakage and profile-mixing risk.

## 70. Profile Mixing

When multiple provider profiles exist, one profile's credential MUST never be reused for another profile accidentally.

Tests must verify isolation.

## 71. Credential Reference Migration

Credential reference syntax may evolve through configuration migration.

A migration MUST NOT require reading or rewriting the credential value unless the platform store target identity changes.

## 72. Store Target Migration

If a future version changes Windows target naming:

- read the old target;
- write the new target;
- verify;
- delete the old target only after success;
- record safe migration status;
- never expose the value.

## 73. Application Version Upgrade

Application upgrades MUST preserve credential-store compatibility.

A breaking credential-reference change requires:

- migration;
- fallback lookup;
- explicit diagnostics;
- test fixtures.

## 74. Release Build Enforcement

Release builds MUST fail quality checks if they include:

- plaintext secret configuration;
- development environment fallback enabled by default;
- test credentials;
- credential dump tooling;
- secret values in samples;
- secret-bearing logs.

## 75. Source Control

The repository MUST include secret scanning.

Sample configuration contains only aliases and placeholders.

## 76. Test Credentials

Tests use synthetic values.

Live-provider tests retrieve credentials from the approved local or CI secret store.

They MUST never embed them in fixture files.

## 77. CI Credentials

CI credentials, if used for opt-in live tests:

- live in CI secret storage;
- are scoped and revocable;
- use least privilege;
- are injected only into the relevant job;
- are never exposed to pull requests from untrusted forks;
- are never printed.

## 78. Security Events

Relevant safe security events include:

```text
CredentialStoreUnavailable
CredentialReferenceInvalid
CredentialAccessDenied
CredentialRetrievalFailed
CredentialWriteFailed
CredentialDeletionRequested
CredentialLeakagePrevented
PlaintextCredentialConfigurationRejected
```

## 79. User-Facing Errors

Safe errors SHOULD explain:

- credential not configured;
- credential store unavailable;
- access denied;
- provider rejected credential;
- credential must be replaced;
- unsupported platform.

They MUST not repeat secret values or sensitive platform details.

## 80. Health Model

A provider profile's credential state may be:

```text
NotConfigured
ConfiguredUntested
Valid
Invalid
StoreUnavailable
AccessDenied
Unknown
```

## 81. Offline Behavior

When the secure store is unavailable or the credential is missing:

- remote provider operations are blocked;
- local Campaign browsing remains available;
- local Dice and Rule Set mechanics remain available;
- backup and export remain available;
- local provider profiles may continue if they require no credential.

## 82. Safe Mode

Safe Mode SHOULD allow:

- inspecting credential aliases;
- deleting broken references;
- replacing credentials;
- testing store availability;
- exporting diagnostics without secrets.

## 83. Backup and Recovery Documentation

Documentation MUST explain:

- credentials are not in backups;
- restoring on another machine requires credential reconfiguration;
- deleting a provider profile does not necessarily delete the stored credential unless explicitly requested;
- uninstall preserves credentials by default.

## 84. Privacy Documentation

Documentation SHOULD state that credentials remain under the operating-system user's secure store and are retrieved only for the configured integration call.

It MUST not claim perfect memory erasure or guarantees beyond the operating system.

## 85. Testing Strategy

The credential architecture requires:

```text
Unit Tests
Adapter Contract Tests
Windows Integration Tests
UI Tests
Security Tests
Upgrade Tests
Uninstall Tests
Failure Injection
```

## 86. Unit Tests

Unit tests SHOULD cover:

- reference parsing;
- normalization;
- validation;
- safe `ToString()`;
- metadata mapping;
- error mapping;
- idempotency state;
- UI health state.

## 87. Windows Integration Tests

Tests SHOULD run under an isolated test target namespace.

They MUST clean up test credentials after execution.

They SHOULD cover:

- create;
- retrieve;
- replace;
- delete;
- restart persistence;
- special aliases;
- access failure simulation where possible.

## 88. Security Tests

Security tests MUST inspect:

- SQLite database;
- configuration files;
- logs;
- diagnostic bundles;
- backups;
- exports;
- crash-safe error output;
- provider-neutral DTOs;
- Operation Records.

The synthetic secret MUST not appear.

## 89. UI Tests

UI tests MUST cover:

- masked entry;
- save;
- replace;
- delete confirmation;
- no redisplay;
- test provider profile;
- missing credential;
- store unavailable;
- keyboard operation;
- focus restoration;
- draft clearing.

## 90. Upgrade Tests

Upgrade tests MUST verify:

- references remain stable;
- stored credential remains accessible;
- no migration writes plaintext;
- provider profile continues to resolve;
- failed reference migration is recoverable.

## 91. Uninstall Tests

Tests SHOULD verify:

- default uninstall preserves credential;
- reinstall resolves the prior credential reference;
- full removal, if implemented later, requires explicit choice.

## 92. Required Test Cases

Tests MUST cover:

- valid reference;
- invalid scheme;
- path traversal alias;
- control characters;
- create new secret;
- duplicate create rejected;
- replace secret;
- retrieve missing secret;
- delete secret;
- double delete;
- store unavailable;
- access denied;
- cancellation before commit;
- ambiguous completion;
- safe metadata;
- safe exception;
- no logging leakage;
- no configuration leakage;
- no database leakage;
- no backup leakage;
- no export leakage;
- no diagnostic leakage;
- no command-line leakage;
- provider-profile isolation;
- application restart;
- application upgrade;
- default uninstall;
- unsupported platform;
- development adapter absent from Release.

## 93. Architecture Tests

Architecture tests MUST reject:

- provider keys stored in configuration models;
- secret values in Domain or persistence entities;
- `ISecretsManager` implementation references from Domain;
- Windows credential APIs outside the Windows adapter;
- environment fallback registration in Release;
- credential-bearing command serialization;
- secret-bearing log properties;
- plaintext sample credentials.

## 94. Prohibited Patterns

### 94.1 API Key in Configuration File

Only references are stored.

### 94.2 API Key in SQLite

Prohibited even if encrypted at the application layer.

### 94.3 Credential in Command-Line Argument

Prohibited.

### 94.4 Secret in Environment by Default Production Policy

Environment access is development-only and explicit.

### 94.5 Redisplaying Stored Secret

Chronicle shows metadata only.

### 94.6 Passing Secret Through Application DTO Graph

Retrieval occurs at the transport boundary.

### 94.7 Logging Secret Then Redacting Downstream

The value must not enter logging.

### 94.8 Silent Plaintext Fallback

Unsupported secure storage blocks credential use.

### 94.9 Campaign-Owned Provider Credential

Credentials belong to application-user configuration, not Campaign truth.

### 94.10 Secret in Backup or Export

Always excluded.

## 95. Alternatives Considered

### Encrypted Values in SQLite

Rejected because:

- encryption-key storage becomes the real unresolved problem;
- Campaign backup could accidentally carry credentials;
- application data and authentication secrets would be coupled;
- platform secure stores already provide the intended boundary.

### Environment Variables for Production

Rejected as default because of process and operational leakage risks.

### Plaintext User Settings

Rejected.

### Master-Password Vault

Potentially useful for true portable mode, but deferred because it introduces cryptographic, UX, recovery, and migration complexity.

### Third-Party Cross-Platform Vault Library

May be evaluated later, but it must still map securely to platform facilities and pass contract tests. A generic abstraction is not accepted merely because it has one API.

## 96. Consequences

### Positive

- credentials remain outside Campaign data;
- native Windows protection;
- provider-neutral configuration;
- safe backup and export;
- clean adapter testing;
- future platform implementations remain possible;
- application upgrades do not need secret migration normally.

### Negative

- restored Campaigns on another machine require credential setup;
- platform-specific adapters are required;
- secrets still exist transiently in managed memory;
- portable-data mode remains incomplete;
- uninstall semantics require documentation;
- integration tests interact with operating-system state.

## 97. Risks

### Secret Leakage Through Accidental Serialization

Mitigation:

- dedicated types;
- no serializer support;
- architecture tests;
- security scans;
- DTO boundaries.

### Store Unavailable

Mitigation:

- degraded mode;
- clear UI;
- no plaintext fallback;
- local features remain available.

### Target Name Migration

Mitigation:

- stable reference format;
- fallback lookup;
- atomic copy-and-delete migration.

### Credential Manager Library Risk

Mitigation:

- isolate interop;
- prefer maintained implementation;
- integration tests;
- ability to replace adapter without Application changes.

### User Believes Backup Contains Credentials

Mitigation:

- explicit backup manifest;
- restore warning;
- documentation;
- provider-profile health after restore.

## 98. Technology Spike

Before acceptance, implement:

1. `SecretReference`;
2. `SecretValue`;
3. `SecretLease`;
4. `ISecretsManager`;
5. in-memory test adapter;
6. Windows Credential Manager adapter;
7. provider-profile configuration using reference only;
8. masked credential-entry UI;
9. OpenAI profile test;
10. replacement;
11. deletion;
12. unavailable-store simulation;
13. database, log, backup, export, and diagnostics leak scan;
14. upgrade test;
15. uninstall and reinstall test.

## 99. Spike Acceptance

The spike passes when:

- the OpenAI adapter can authenticate without receiving a persisted API key;
- the key exists only in the Windows secure store and transient memory;
- configuration contains only the reference;
- replacing a key does not expose the old or new value;
- no synthetic key appears in logs, SQLite, backup, export, or diagnostics;
- uninstall preserves the key;
- reinstall resolves it;
- unavailable secure storage blocks remote calls without blocking Campaign access;
- the in-memory adapter can replace the Windows adapter in deterministic tests.

## 100. Definition of Compliance

An implementation complies when:

- all production credentials use `ISecretsManager`;
- Windows Credential Manager is the first production adapter;
- only credential references are persisted;
- secret values are retrieved at the transport boundary;
- stored values are never redisplayed;
- no plaintext fallback exists in Release;
- backups, exports, logs, diagnostics, and Campaign data exclude secrets;
- uninstall preserves credentials by default;
- unsupported platforms do not store plaintext;
- adapters pass the common contract suite;
- security tests prove nonleakage.

## 101. Review Triggers

This ADR must be reviewed if:

- Windows Credential Manager behavior becomes unsuitable;
- the interop library becomes unmaintained;
- Linux or macOS becomes officially supported;
- true portable-data mode is introduced;
- cloud accounts require refresh tokens;
- OAuth authorization flows are introduced;
- multiple users share one Chronicle installation;
- encrypted application-managed vaults become necessary;
- a credential leakage incident occurs.

## 102. Deferred Decisions

Later ADRs MAY define:

- exact Windows interop library;
- OAuth and refresh-token handling;
- Linux Secret Service adapter;
- macOS Keychain adapter;
- master-password portable vault;
- enterprise credential integration;
- credential rotation reminders;
- full uninstall and secure local-data removal;
- hardware-backed key protection.

## 103. Final Decision

Chronicle will store production provider credentials in the operating system's secure credential store through an `ISecretsManager` abstraction.

Windows Credential Manager is the first production implementation.

Chronicle configuration, Campaigns, backups, exports, and logs will carry references, never secret values.

A Campaign may depend on a provider.

It must never become a container for the provider's key.
