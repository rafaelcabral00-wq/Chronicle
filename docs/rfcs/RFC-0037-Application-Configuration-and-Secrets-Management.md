---
id: RFC-0037
title: Application Configuration and Secrets Management
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
  - RFC-0036
---

> **"Configuration tells Chronicle how to operate. Secrets authorize access. Neither belongs inside the Campaign."**

# Application Configuration and Secrets Management

## Abstract

This RFC defines Chronicle's application-configuration and secrets-management architecture.

It establishes:

- configuration categories;
- source precedence;
- environment-specific values;
- provider profiles;
- credential references;
- operating-system secret stores;
- validation;
- startup behavior;
- runtime reload;
- configuration migrations;
- local paths;
- feature flags;
- developer settings;
- privacy boundaries;
- export and backup exclusions;
- observability;
- testing.

Campaign data, Rule Set Preferences, application configuration, and credentials are distinct forms of state.

They MUST NOT be merged into one generic settings store.

The MVP remains local-first and desktop-first.

Provider credentials MUST be stored outside Campaign persistence and referenced through opaque credential aliases.

## 1. Purpose

Chronicle requires configurable infrastructure.

Examples include:

- Narrative Intelligence provider selection;
- provider endpoint;
- model mapping;
- timeout;
- retry limits;
- context budgets;
- local database location;
- backup directory;
- Rule Knowledge source paths;
- log level;
- payload retention;
- developer mode;
- feature availability.

Some settings are harmless.

Others are sensitive or security-critical.

Without explicit boundaries, Chronicle could:

- store API keys in Campaign exports;
- embed credentials in prompts;
- mix user preferences with mechanics;
- silently change provider behavior;
- load unsafe endpoints;
- expose local paths;
- apply invalid settings;
- depend on environment variables for all desktop behavior;
- make startup unrecoverable after one bad value.

This RFC prevents those outcomes.

## 2. Scope

This RFC defines:

- configuration categories;
- configuration ownership;
- configuration sources;
- precedence;
- configuration schemas;
- defaults;
- validation;
- provider profiles;
- model profiles;
- credential aliases;
- secret storage;
- secret retrieval;
- secret rotation;
- local path handling;
- feature flags;
- developer mode;
- runtime changes;
- restart requirements;
- configuration history;
- migration;
- import and export policy;
- backup policy;
- observability;
- testing.

This RFC does not define:

- one dependency-injection framework;
- one concrete file format;
- one operating-system credential API;
- one provider;
- one model;
- cloud administration;
- enterprise policy management;
- multiplayer user accounts;
- remote secrets vault integration.

## 3. Core Distinction

Chronicle distinguishes:

```text
Application Configuration
    Controls how the installation operates.

Campaign Preferences
    Controls how one Campaign and Rule Set behave.

User Experience Preferences
    Controls presentation and interaction.

Secrets
    Authorize access to protected external or local resources.

Operational State
    Records running and completed operations.
```

These categories MUST have separate ownership and persistence rules.

## 4. Application Configuration

Application Configuration belongs to one Chronicle installation.

Examples:

- selected provider profile;
- provider timeouts;
- local storage location;
- backup policy;
- log level;
- knowledge index location;
- developer mode.

It MUST NOT become Campaign truth.

## 5. Campaign Preferences

Campaign Preferences are governed by RFC-0032.

They are:

- Rule Set-aware;
- Campaign-scoped;
- versioned;
- historically relevant.

Application Configuration MUST NOT override them silently.

## 6. User Experience Preferences

User Experience Preferences MAY include:

- language;
- theme;
- font scale;
- animation;
- default export directory;
- diagnostics display.

They SHOULD remain mechanically inert.

## 7. Secrets

Secrets include:

- provider API keys;
- access tokens;
- encryption keys;
- backup passwords when retained;
- future connector credentials;
- local protected-service credentials.

Secrets MUST NOT be stored as ordinary configuration values.

## 8. Configuration Categories

Initial application-configuration categories SHOULD include:

```text
Storage
NarrativeIntelligence
RuleKnowledge
Backup
Export
Observability
Security
UserExperience
Developer
FeatureFlags
```

## 9. Storage Configuration

Storage configuration MAY include:

- database path;
- database mode;
- read-model settings;
- migration behavior;
- checkpoint directory;
- integrity-check policy.

Storage configuration changes often require restart.

## 10. Narrative Intelligence Configuration

Narrative Intelligence configuration MAY include:

- active provider profile;
- capability-to-model mapping;
- timeout;
- retry count;
- repair limit;
- streaming flag;
- local or remote mode;
- context-budget profile;
- provider data-handling options.

## 11. Rule Knowledge Configuration

Rule Knowledge configuration MAY include:

- index location;
- enabled source identifiers;
- local source references;
- index build concurrency;
- semantic retrieval enabled;
- embedding profile;
- remote transmission policy.

## 12. Backup Configuration

Backup configuration MAY include:

- backup directory;
- automatic backup enabled;
- retention;
- encryption preference;
- pre-migration checkpoint policy;
- validation policy.

## 13. Observability Configuration

Observability configuration MAY include:

- log level;
- retention;
- developer diagnostics;
- payload capture;
- trace sampling;
- diagnostic-bundle location.

Sensitive capture remains disabled by default.

## 14. Security Configuration

Security configuration MAY include:

- allowed provider endpoints;
- local-only mode;
- restricted-source transmission policy;
- package integrity enforcement;
- unsafe import behavior;
- developer overrides.

Security defaults SHOULD be conservative.

## 15. Configuration Schema

Every configuration section SHOULD have a versioned schema.

A configuration schema SHOULD define:

- stable key;
- value type;
- default;
- requiredness;
- allowed values;
- validation;
- sensitivity;
- restart requirement;
- runtime reload policy;
- localization key;
- migration behavior.

## 16. Stable Configuration Keys

Configuration keys MUST be:

- language-neutral;
- stable;
- documented;
- namespaced;
- independent from UI labels.

Examples:

```text
narrative.provider.active_profile
storage.database.path
observability.log.level
security.remote_provider.enabled
```

## 17. Configuration Value Types

The configuration system SHOULD support:

```text
Boolean
Integer
Decimal
Text
Choice
MultiChoice
Path
Duration
Size
Structured
CredentialReference
```

## 18. CredentialReference Type

A CredentialReference contains an opaque alias.

Example:

```text
credential://providers/openai/default
```

It MUST NOT contain the secret value.

## 19. Configuration Sources

Chronicle MAY load configuration from:

```text
Built-In Defaults
Application Configuration File
Environment Variables
Command-Line Overrides
Operating-System Integration
User-Selected Runtime Values
Developer Test Overrides
```

The exact supported set depends on delivery environment.

## 20. Source Precedence

Recommended precedence:

```text
Test Override
Command-Line Override
Environment Variable
User-Saved Configuration
Platform Integration
Built-In Default
```

Secrets remain outside this precedence chain except through references.

## 21. Precedence Transparency

Chronicle SHOULD be able to report:

- effective value;
- source;
- whether overridden;
- restart requirement.

It MUST NOT expose secret values.

## 22. Built-In Defaults

Defaults MUST be:

- safe;
- deterministic;
- versioned;
- valid without credentials where possible;
- local-first.

A fresh installation SHOULD start without remote-provider access until configured.

## 23. Configuration File

If a configuration file is used, it SHOULD:

- contain no plaintext secrets;
- use a versioned format;
- support atomic writes;
- preserve unknown fields safely where policy allows;
- be validated before publication.

## 24. Environment Variables

Environment variables MAY support:

- development;
- CI;
- containerized builds;
- temporary overrides.

They SHOULD NOT be the only configuration mechanism for the desktop application.

## 25. Command-Line Overrides

Command-line overrides MAY support:

- developer mode;
- alternate data directory;
- safe mode;
- diagnostics;
- test provider selection.

Secrets SHOULD NOT be passed through command-line arguments because process listings may expose them.

## 26. Effective Configuration

Chronicle SHOULD compile all sources into an immutable `EffectiveConfiguration`.

It SHOULD contain:

- configuration schema version;
- normalized values;
- source metadata;
- validation status;
- fingerprint;
- restart-sensitive values;
- unresolved credential references.

## 27. Configuration Fingerprint

A configuration fingerprint SHOULD be deterministic over nonsecret effective values.

Secret values MUST NOT enter the fingerprint.

Credential aliases MAY enter it when relevant.

## 28. Validation Pipeline

Configuration validation SHOULD occur in this order:

1. parse;
2. schema-version validation;
3. key validation;
4. type validation;
5. range validation;
6. dependency validation;
7. path validation;
8. endpoint policy validation;
9. credential-reference validation;
10. component compatibility validation;
11. startup-readiness evaluation.

## 29. Invalid Configuration

An invalid value MUST NOT be applied partially.

Chronicle SHOULD:

- preserve the prior valid configuration;
- report the invalid key;
- explain safe correction;
- enter safe mode when required.

## 30. Unknown Configuration Keys

Unknown keys SHOULD be:

```text
Preserved
Warned
Rejected
```

according to schema policy.

They MUST NOT activate behavior dynamically.

## 31. Configuration Dependencies

A configuration key MAY depend on another.

Examples:

```text
semantic_retrieval.enabled = true
requires
embedding_profile configured
```

```text
remote_provider.enabled = true
requires
provider profile and credential reference
```

## 32. Configuration Conflicts

Conflicting values MUST be rejected.

Example:

```text
security.local_only_mode = true
conflicts with
remote_provider.enabled = true
```

## 33. Startup Validation

Startup SHOULD validate:

- storage path;
- schema compatibility;
- credential-store availability;
- active provider profile;
- package registry;
- knowledge index;
- backup path;
- log path;
- security policy.

## 34. Startup Readiness

Components SHOULD report:

```text
Ready
Degraded
Blocked
Unavailable
```

Chronicle MAY start in degraded mode when optional features are unavailable.

## 35. Safe Mode

Safe mode SHOULD allow:

- Campaign read access;
- configuration repair;
- diagnostics;
- backup;
- export;
- credential repair.

It SHOULD block unsafe state-changing operations.

## 36. Provider Profile

A `ProviderProfile` defines one Narrative Intelligence provider configuration.

It SHOULD contain:

```text
ProviderProfileId
ProviderAdapterKey
DisplayName
EndpointPolicy
CredentialReference
CapabilityMappings
Timeouts
RetryPolicy
DataHandlingPolicy
Enabled
ProfileVersion
```

## 37. Provider Profile Identity

ProviderProfileId MUST be stable and installation-scoped.

Campaigns SHOULD not persist concrete credentials or provider secrets.

## 38. Capability Mapping

A provider profile MAY map capabilities to Model Profiles.

Example:

```text
Narrator → interactive-large
Archivist → analysis-large
CampaignGenerator → generation-large
Repairer → structured-small
```

## 39. Model Profile

A Model Profile SHOULD define provider-neutral intent.

It MAY contain:

- profile identifier;
- context class;
- output class;
- latency class;
- quality class;
- structured-output requirement;
- local resource requirement;
- provider model mapping.

Chronicle contracts MUST not depend on concrete model names.

## 40. Concrete Model Mapping

The provider adapter maps Model Profiles to concrete provider models.

The mapping is configuration, not Domain state.

## 41. Endpoint Policy

Remote provider endpoints MUST be validated.

The MVP SHOULD prefer:

- known provider endpoints;
- explicitly configured local endpoints;
- HTTPS for remote endpoints;
- no provider-supplied arbitrary callback URLs.

## 42. Custom Endpoint

A custom endpoint MAY be supported for local or compatible providers.

It SHOULD require:

- explicit user configuration;
- endpoint validation;
- local or secure transport warning;
- provider adapter compatibility.

## 43. Provider Data-Handling Policy

A provider profile SHOULD declare:

- remote or local;
- restricted data allowed;
- payload retention preference;
- training-use setting when available;
- source-content transmission;
- maximum data classification accepted.

## 44. Classification Enforcement

Before provider invocation, Chronicle MUST compare:

- context data classification;
- source transmission policy;
- provider profile policy.

Forbidden data blocks invocation.

## 45. Secrets Manager

Chronicle SHOULD expose a `SecretsManager` application contract.

It SHOULD support:

- create or store secret;
- retrieve secret by alias;
- replace secret;
- delete secret;
- test availability;
- list aliases without values.

## 46. Secret Store

The official application SHOULD use a platform credential store where available.

Examples conceptually include:

- operating-system keychain;
- credential vault;
- protected secret service.

The exact implementation requires an ADR.

## 47. Secret Store Fallback

If no secure store is available, Chronicle SHOULD:

- block remote-provider configuration;
- or require an explicitly acknowledged temporary development fallback.

Plaintext production storage SHOULD NOT be the default fallback.

## 48. Secret Alias

A Secret Alias SHOULD be:

- stable;
- nonsecret;
- namespaced;
- installation-scoped;
- safe for logs.

## 49. Secret Retrieval

Secret retrieval SHOULD occur only immediately before the protected operation.

The secret SHOULD:

- remain in memory briefly;
- not enter logs;
- not enter configuration fingerprints;
- not enter provider-independent DTOs unnecessarily;
- not enter Campaign persistence.

## 50. Secret Lifetime in Memory

Secrets SHOULD be released from application memory as soon as practical.

Chronicle MUST avoid claiming guaranteed memory erasure where the runtime cannot provide it.

## 51. Secret Rotation

Secret rotation SHOULD:

- replace the value under an alias;
- preserve provider profiles;
- avoid Campaign migration;
- validate the new secret;
- retain no old plaintext value.

## 52. Secret Deletion

Deleting a secret SHOULD:

- remove it from the secure store;
- leave provider profile metadata;
- mark dependent profiles unavailable;
- not delete Campaigns.

## 53. Credential Test

Chronicle MAY test a credential.

The test MUST:

- use a bounded provider operation;
- avoid Campaign content;
- avoid persistence mutation;
- record only safe result metadata.

## 54. Credential Errors

Credential errors SHOULD distinguish:

```text
Missing
Unavailable
Rejected
Expired
InsufficientPermission
StoreFailure
```

They MUST not expose secret content.

## 55. Backup and Secrets

Backups SHOULD exclude secret values by default.

They MAY include:

- credential aliases;
- provider profile metadata;
- reconfiguration instructions.

## 56. Export and Secrets

Portable Campaign exports MUST exclude:

- API keys;
- access tokens;
- encryption keys;
- credential-store identifiers that expose platform internals.

## 57. Configuration Export

A future application-configuration export MAY include nonsecret settings.

Secret aliases MAY be included only when useful and safe.

## 58. Local Paths

Path configuration MAY include:

- data directory;
- backup directory;
- export directory;
- knowledge source path;
- diagnostics directory.

Paths MUST be normalized and validated.

## 59. Path Validation

Path validation SHOULD defend against:

- invalid syntax;
- path traversal;
- inaccessible location;
- read-only location;
- unsafe network location;
- file-versus-directory mismatch;
- symlink policy violations;
- restricted system directories.

## 60. Absolute Paths

Absolute paths MAY be stored locally.

They MUST NOT enter:

- prompts;
- provider requests;
- public exports;
- ordinary logs.

## 61. Portable Path References

Portable artifacts SHOULD use logical references rather than installation-specific paths.

## 62. Feature Flags

Feature flags control delivery of incomplete or optional capabilities.

A Feature Flag SHOULD define:

- stable key;
- default;
- owner;
- maturity;
- restart behavior;
- dependencies;
- removal plan.

## 63. Feature Flag Prohibitions

Feature flags MUST NOT:

- alter completed Campaign history;
- bypass security validation;
- weaken Domain invariants;
- replace Rule Set Preferences;
- become permanent undocumented configuration.

## 64. Developer Flags

Developer-only flags MAY enable:

- scripted providers;
- fixture loading;
- verbose diagnostics;
- schema inspection;
- migration dry run;
- unsafe test endpoints.

They MUST be clearly separated from production behavior.

## 65. Configuration Change Request

A runtime configuration change SHOULD contain:

```text
OperationId
ConfigurationKey
ExpectedConfigurationVersion
ExpectedCurrentValue
RequestedValue
AuthorizationContext
```

## 66. Configuration Version

Saved application configuration SHOULD have a version.

Changes use optimistic concurrency.

## 67. Runtime Reload

Each key SHOULD declare:

```text
Immediate
NextOperation
RequiresComponentRestart
RequiresApplicationRestart
MigrationRequired
```

## 68. Immediate Reload

Safe examples MAY include:

- log level;
- UI language;
- diagnostic display;
- default export directory.

## 69. Next-Operation Reload

Examples MAY include:

- provider timeout;
- retry limit;
- active provider profile;
- context-budget profile.

The current operation preserves its starting configuration snapshot.

## 70. Component Restart

Examples MAY include:

- knowledge index engine;
- local provider process;
- background worker concurrency.

## 71. Application Restart

Examples MAY include:

- database location;
- storage engine mode;
- some credential-store integrations;
- core plugin loading.

## 72. Configuration Snapshot

Long-running operations SHOULD preserve the relevant configuration snapshot.

This supports:

- reproducibility;
- diagnostics;
- retry;
- provider comparison;
- safe completion after later setting changes.

## 73. Secret Snapshot Prohibition

Operation snapshots MUST NOT copy secret values.

They preserve credential alias and profile identity only.

## 74. Configuration History

Chronicle SHOULD preserve history for material configuration changes.

Examples:

- provider profile changed;
- database path changed;
- telemetry enabled;
- unsafe developer mode enabled;
- backup encryption changed.

## 75. Configuration Audit Record

An audit record SHOULD include:

- OperationId;
- key;
- prior safe value;
- new safe value;
- source;
- timestamp;
- restart requirement;
- actor classification;
- result.

## 76. Sensitive Configuration Values

Some nonsecret configuration may still be sensitive.

Examples:

- local paths;
- provider endpoint;
- telemetry identifier;
- source directory.

Audit and logs SHOULD redact them as appropriate.

## 77. Configuration Migration

Configuration schemas evolve.

A migration SHOULD define:

- source schema version;
- target schema version;
- key mappings;
- default behavior;
- removed keys;
- warnings;
- restart requirement.

## 78. Migration Requirements

Configuration migration MUST:

- be deterministic;
- preserve prior file or checkpoint;
- avoid secret-value extraction;
- validate the target;
- publish atomically;
- report warnings.

## 79. Missing Secret After Migration

If a migrated profile references a missing secret:

- migration may succeed;
- the profile becomes unavailable;
- the user receives repair guidance;
- Chronicle MUST not fabricate a credential.

## 80. Removed Configuration Key

A removed key SHOULD be:

- migrated;
- preserved as deprecated metadata;
- or reported.

It MUST not activate unknown behavior.

## 81. Configuration Backup

Application backups MAY include nonsecret configuration.

They SHOULD exclude:

- secret values;
- transient developer overrides;
- environment-variable values unless explicitly materialized;
- temporary test endpoints.

## 82. Restore Behavior

After restore, Chronicle SHOULD:

- validate configuration;
- check credential aliases;
- report missing local paths;
- preserve Campaign data;
- allow reconfiguration before play.

## 83. Installation Identity

Configuration MAY include an installation identifier for diagnostics.

It MUST NOT be used as user identity.

## 84. Environment Classification

Chronicle MAY classify runtime environment as:

```text
Development
Test
Production
SafeMode
```

Behavioral differences MUST be explicit.

## 85. Production Defaults

Production defaults SHOULD include:

- developer mode off;
- raw prompt capture off;
- remote telemetry off;
- strict package validation on;
- import safety on;
- local-first storage;
- no unsafe endpoint.

## 86. Test Configuration

Tests SHOULD use isolated configuration and secret stores.

They MUST NOT read developer or production secrets accidentally.

## 87. CI Secrets

CI credentials, if required, SHOULD use the CI platform's secret mechanism.

They MUST not enter test fixtures or artifacts.

## 88. Configuration Observability

Chronicle SHOULD record:

- configuration schema version;
- effective profile identifiers;
- source categories;
- validation result;
- missing credential aliases;
- restart-required status;
- configuration fingerprint.

## 89. Configuration Logging

Logs MUST NOT record:

- secret values;
- authorization headers;
- full sensitive paths;
- encryption passwords.

## 90. Provider Configuration Observability

Safe metadata MAY include:

- ProviderProfileId;
- adapter key;
- local or remote;
- Model Profile identifiers;
- timeout;
- data-handling classification;
- enabled status.

## 91. Health Integration

Configuration health SHOULD contribute to component health.

Examples:

```text
Provider profile valid but credential missing
    → Provider Unavailable

Knowledge source path missing
    → Knowledge Degraded

Database path inaccessible
    → Application Blocked
```

## 92. Error Model

Recommended errors include:

```text
ConfigurationInvalid
ConfigurationVersionConflict
ConfigurationDependencyMissing
ConfigurationConflict
ConfigurationMigrationFailed
RestartRequired
ProviderProfileInvalid
ProviderEndpointRejected
CredentialReferenceMissing
SecretStoreUnavailable
SecretUnavailable
SecretRejected
PathInvalid
PathUnavailable
FeatureFlagInvalid
```

## 93. Retry Semantics

Typical behavior:

```text
Secret store temporarily unavailable
    → SafeWithSameOperationId

Invalid endpoint
    → NotRetryable without changed input

Version conflict
    → SafeAfterRefresh

Missing credential
    → RequiresUserAction

Restart-required change
    → Complete configuration write, activate after restart
```

## 94. Security Requirements

Configuration handling MUST defend against:

- arbitrary key activation;
- malicious endpoints;
- plaintext secret storage;
- command-line secret exposure;
- path traversal;
- configuration injection;
- unsafe deserialization;
- unbounded Structured values;
- environment leakage;
- secret logging.

## 95. Configuration Parser

The parser SHOULD:

- use a strict schema;
- reject duplicate critical keys;
- bound nesting;
- bound text size;
- avoid polymorphic deserialization;
- preserve safe unknown values according to policy.

## 96. Required Test Cases

Tests MUST cover:

- built-in defaults;
- user-saved override;
- environment override;
- command-line override;
- source precedence;
- unknown key;
- invalid type;
- dependency missing;
- conflicting settings;
- safe-mode startup;
- provider profile without credential;
- credential alias resolution;
- secret rotation;
- secret deletion;
- credential test without Campaign content;
- secret excluded from logs;
- secret excluded from backup;
- secret excluded from export;
- invalid provider endpoint;
- restricted source with incompatible provider policy;
- invalid path;
- path traversal;
- next-operation reload;
- restart-required change;
- configuration version conflict;
- migration success;
- migration failure preserving prior configuration;
- developer mode warning;
- test environment isolated from production secrets.

## 97. Prohibited Patterns

### 97.1 Secrets in Campaign Records

Campaign persistence MUST not contain provider credentials.

### 97.2 Secrets in Ordinary Configuration

Configuration stores references, not plaintext secret values.

### 97.3 Secret in Command-Line Arguments

Process listings may expose command-line values.

### 97.4 Campaign Preference as Application Configuration

Mechanical choices remain Campaign-scoped and Rule Set-controlled.

### 97.5 Application Configuration Changes History

Provider or timeout changes MUST not reinterpret accepted Campaign history.

### 97.6 Arbitrary Configuration Keys

Unknown keys MUST not activate code paths dynamically.

### 97.7 UI-Only Validation

Application services validate all configuration changes.

### 97.8 Silent Partial Configuration

Invalid configuration must not be partially applied.

### 97.9 Provider Model Name in Domain

Concrete model mappings remain infrastructure configuration.

### 97.10 Secret Values in Diagnostic Bundles

Diagnostics preserve aliases and errors, not credentials.

## 98. Current Delivery Decision

The MVP adopts:

- versioned application-configuration schemas;
- built-in safe defaults;
- local saved configuration;
- optional environment and command-line overrides for development;
- explicit source precedence;
- immutable EffectiveConfiguration snapshots;
- provider and Model Profiles;
- opaque credential aliases;
- platform credential-store integration target;
- no plaintext secret storage by default;
- runtime reload classification;
- configuration history for material changes;
- safe mode;
- path validation;
- production-safe defaults;
- no cloud secrets vault;
- no enterprise policy service;
- no Campaign-stored credentials.

## 99. Architecture Horizon

Future evolution MAY include:

- remote secrets vaults;
- enterprise configuration policies;
- per-user provider profiles;
- managed cloud configuration;
- encrypted configuration synchronization;
- multi-installation profile transfer;
- signed configuration bundles;
- plugin permission settings;
- remote administration;
- organization-wide model policies.

The MVP MUST NOT implement these capabilities without a later milestone.

## 100. Open Questions

The following remain open:

- Which configuration file format will be used?
- Which platform credential stores are required initially?
- How should portable desktop builds handle missing secure stores?
- Which configuration keys must be editable in the MVP UI?
- Should provider profiles be exportable without credentials?
- Which provider data-handling settings are required?
- How should custom local provider endpoints be validated?
- Should model mappings be user-visible or advanced-only?
- Which changes require application restart?
- How will configuration migrations be tested across releases?
- Should application configuration live in the main database or a separate file?
- How should path permissions be checked across operating systems?
- Should safe mode activate automatically after repeated startup failure?
- What default backup path should the official application use?
- Which feature flags are needed before first release?

These questions require technology ADRs, provider selection, UI RFCs, and implementation evidence.

## 101. Compliance Checklist

An implementation complies when:

- application configuration remains separate from Campaign Preferences;
- secrets remain separate from ordinary configuration;
- credentials are referenced through aliases;
- secret values do not enter Campaign persistence;
- configuration precedence is explicit;
- effective configuration is validated;
- invalid changes are atomic and recoverable;
- provider profiles remain infrastructure concepts;
- concrete model names do not enter Domain contracts;
- local paths are validated and not leaked;
- runtime reload behavior is declared;
- long operations preserve configuration snapshots without secret values;
- migrations are deterministic;
- logs, backups, exports, and diagnostics exclude secrets;
- production defaults remain conservative.

## 102. Final Principle

Configuration determines how Chronicle reaches its dependencies.

Secrets prove that it is allowed to reach them.

Neither should ever become part of the story, the rules, or the Campaign's memory.
