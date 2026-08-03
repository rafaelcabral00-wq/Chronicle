---
id: ADR-0039
title: Application Configuration Sources, Validation, and Environment Overrides
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
  - ADR-0023
  - ADR-0026
  - ADR-0032
  - ADR-0036
  - ADR-0037
  - ADR-0038
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

> **"Configuration may change how Chronicle runs. It must never become an invisible source of Campaign truth."**

# Application Configuration Sources, Validation, and Environment Overrides

## 1. Status

**Proposed**

This ADR defines Chronicle's application configuration sources, precedence, validation, environment-specific overrides, runtime mutability, persistence, secrets separation, startup behavior, compatibility, and diagnostics.

The decision is:

- separate application configuration from Campaign Preferences and authoritative Campaign state;
- use strongly typed configuration contracts;
- define an explicit, deterministic precedence order for configuration sources;
- use packaged defaults as the baseline;
- use a user-local Chronicle configuration file for persistent nonsecret application settings;
- allow command-line and environment overrides only for explicitly declared keys;
- reserve environment variables primarily for Development, CI, packaging, and managed deployment scenarios;
- prohibit environment variables from silently changing Campaign mechanics;
- prohibit raw secrets in ordinary configuration;
- validate all effective configuration before normal startup;
- treat unknown, malformed, unsupported, or unsafe configuration as a typed startup issue;
- enter Safe Mode or fail fast when invalid configuration could threaten data integrity or security;
- permit noncritical UI and diagnostic settings to fall back safely;
- classify settings by restart requirement and runtime mutability;
- apply runtime changes only through validated Application services;
- publish configuration files atomically;
- preserve the prior valid configuration when a write fails;
- version configuration contracts and support explicit migration;
- keep configuration diagnostics privacy-safe;
- avoid dumping the complete effective configuration to logs;
- prevent Development-only overrides from being accepted silently in Stable;
- expose a clear configuration inspection view that shows source, effective value classification, and restart requirements without exposing secrets.

The decision becomes **Accepted** after a configuration spike proves:

- packaged defaults;
- user-local file loading;
- explicit environment override;
- explicit command-line override;
- deterministic precedence;
- typed validation;
- unknown-key handling;
- malformed-file recovery;
- atomic save;
- rollback to prior valid configuration;
- Stable rejection of Development-only keys;
- Safe Mode startup for critical invalid values;
- runtime update of a permitted setting;
- restart-required setting behavior;
- no secret leakage in configuration or diagnostics.

## 2. Context

Chronicle requires installation-level configuration for concerns such as:

- data-directory location;
- backup-directory location;
- logging level;
- log retention;
- UI appearance;
- language;
- release channel;
- provider-profile defaults;
- request timeout limits;
- package discovery paths;
- Safe Mode flags;
- Development diagnostics;
- test adapters;
- update policy;
- feature availability;
- performance budgets.

These settings are distinct from Campaign Preferences.

Campaign Preferences define how a specific Campaign is played.

Application configuration defines how the installed Chronicle application operates.

Mixing them would create dangerous ambiguity.

Examples of failure:

- an environment variable changes Dice rules;
- a global config file overrides a Campaign's progression policy;
- a restored backup imports machine-specific paths;
- a Development setting enables plaintext secrets in Stable;
- a malformed config file prevents access to otherwise healthy Campaign data;
- a command-line argument changes storage location without visibility;
- a user edits JSON manually and creates an unsupported value;
- a package adds arbitrary configuration keys that bypass validation.

Chronicle therefore needs a strict configuration model with explicit source precedence and startup handling.

## 3. Decision Drivers

The design prioritizes:

1. deterministic startup;
2. strong validation;
3. separation from Campaign truth;
4. privacy;
5. safe overrides;
6. atomic persistence;
7. Development and Stable separation;
8. clear diagnostics;
9. runtime mutability control;
10. migration;
11. portability;
12. testability.

## 4. Decision Summary

Chronicle will use:

```text
Configuration Ownership
    Chronicle application

Configuration Type
    strongly typed versioned contracts

Default Source
    packaged defaults

Persistent User Source
    Chronicle-managed local configuration file

Override Sources
    explicitly allowed environment variables
    explicitly allowed command-line options

Secrets
    excluded
    secret references only

Precedence
    command line
    environment
    user-local file
    packaged defaults

Validation
    complete before normal startup

Critical Failure
    fail fast or Safe Mode

Persistence
    atomic replace
    prior valid copy preserved

Runtime Change
    Application service only
```

## 5. Configuration Versus Campaign Preferences

Application configuration controls the installation.

Campaign Preferences control one Campaign.

## 6. Examples of Application Configuration

Examples:

```text
UI language
appearance
log retention
backup directory
provider timeout defaults
release channel
package discovery behavior
update policy
diagnostic mode
```

## 7. Examples of Campaign Preferences

Examples:

```text
critical threshold
progression cost variant
narrative tone
safety boundary
finalization behavior
```

## 8. No Cross-Authority

Application configuration MUST NOT override authoritative Campaign Preferences.

## 9. Configuration Contract

Chronicle defines a versioned root contract.

Recommended key:

```text
chronicle.application-configuration
```

## 10. Configuration Version

The root configuration includes:

```text
ConfigurationContractVersion
```

## 11. Strongly Typed Sections

Recommended sections:

```text
Application
Storage
Logging
Diagnostics
Providers
Packages
Backup
Updates
Presentation
Development
```

## 12. Section Ownership

Each section is Chronicle-owned.

Rule Set packages cannot add unrestricted root configuration sections.

## 13. Package Configuration

Package-specific persistent behavior belongs in:

- package manifest;
- Campaign Preferences;
- explicit package installation metadata.

It does not become arbitrary application configuration.

## 14. Configuration Sources

Chronicle supports:

```text
PackagedDefaults
UserLocalConfiguration
EnvironmentOverrides
CommandLineOverrides
RuntimeSessionOverrides
```

## 15. Packaged Defaults

Packaged defaults are compiled or shipped with the application.

## 16. User-Local Configuration

Persistent nonsecret user settings are stored in a Chronicle-managed local file.

## 17. Environment Overrides

Environment variables are supported only for declared keys.

## 18. Command-Line Overrides

Command-line options are supported only for declared operational scenarios.

## 19. Runtime Session Overrides

Temporary in-memory overrides may exist for:

- tests;
- Safe Mode;
- controlled diagnostics;
- one startup session.

They are not persisted unless explicitly converted through a settings workflow.

## 20. Precedence

Highest to lowest:

```text
1. Runtime Session Override
2. Command-Line Override
3. Environment Override
4. User-Local Configuration
5. Packaged Default
```

## 21. Precedence Is Explicit

The effective source of each value is inspectable.

## 22. No Hidden Source

Registry keys, current working directory files, arbitrary `.env` files, and provider SDK defaults do not silently enter configuration.

## 23. User Configuration Path

The user configuration file resides under the Chronicle-managed application data root.

## 24. File Name

Recommended:

```text
appsettings.user.json
```

The exact name is implementation-level.

## 25. No Repository Configuration

User-local configuration is not read from the source repository during Stable execution.

## 26. Development Configuration

Development may load additional explicit files such as:

```text
appsettings.Development.json
```

only in Development composition.

## 27. Stable Guard

Stable startup rejects Development-only settings when they would weaken security, privacy, or integrity.

## 28. Environment Variable Prefix

All Chronicle environment variables use a dedicated prefix.

Example:

```text
CHRONICLE_
```

## 29. Environment Key Registry

Every allowed environment override must be declared in a registry.

## 30. Unknown Environment Variables

Unknown `CHRONICLE_` variables are reported safely.

Stable may ignore noncritical unknown keys with a warning or fail when ambiguity is risky.

## 31. No Arbitrary Binding

Chronicle does not bind every environment variable automatically into the configuration tree.

## 32. Command-Line Registry

Every command-line option has:

- stable name;
- type;
- allowed release channels;
- default;
- validation;
- help text;
- persistence behavior.

## 33. Command-Line Secret Prohibition

Raw secrets are prohibited in command-line arguments.

## 34. Secret Separation

Configuration stores only:

- provider profile IDs;
- Secret References;
- credential status metadata.

It never stores secret values.

## 35. Sensitive Configuration

Sensitive nonsecret values may require redacted diagnostics.

## 36. Configuration Field Metadata

Every field SHOULD declare:

```text
ConfigurationKey
ValueType
Default
Required
AllowedSources
ReleaseChannels
SensitivityClass
RuntimeMutability
RestartRequirement
ValidationRules
MigrationPolicy
```

## 37. Allowed Sources

A field may allow only selected sources.

Example:

```text
Storage.DataDirectory
    PackagedDefault
    UserLocalConfiguration
    CommandLineOverride
```

## 38. Environment Not Universally Allowed

Not every field accepts an environment override.

## 39. Runtime Mutability

Recommended values:

```text
ImmutableAfterStartup
MutableWithRestart
MutableAtRuntime
MutableForCurrentSessionOnly
```

## 40. Restart Requirement

Recommended values:

```text
None
WindowRestart
ApplicationRestart
SafeModeRestart
```

## 41. Criticality

Recommended values:

```text
Critical
Important
Optional
DiagnosticOnly
```

## 42. Critical Configuration

Critical examples:

- data directory;
- database path policy;
- secret-store implementation;
- release channel;
- package execution policy.

## 43. Important Configuration

Examples:

- provider timeout;
- backup destination;
- log retention.

## 44. Optional Configuration

Examples:

- appearance;
- animation;
- default window size.

## 45. Diagnostic-Only Configuration

Examples:

- additional trace categories;
- fault-injection controls;
- synthetic provider mode.

## 46. Validation Pipeline

Recommended pipeline:

```text
Load Packaged Defaults
    ↓
Parse User Configuration
    ↓
Apply Declared Environment Overrides
    ↓
Apply Declared Command-Line Overrides
    ↓
Apply Runtime Session Overrides
    ↓
Bind Strongly Typed Contract
    ↓
Validate Fields
    ↓
Validate Cross-Field Constraints
    ↓
Validate Release-Channel Policy
    ↓
Classify Startup Outcome
```

## 47. Parse Failure

A malformed user configuration file does not overwrite or destroy the file.

## 48. Last Known Valid Configuration

Chronicle SHOULD preserve:

```text
appsettings.user.last-known-valid.json
```

or an equivalent validated checkpoint.

## 49. Malformed Current File

When the current file is malformed:

- preserve it for inspection;
- try the last known valid configuration;
- enter Safe Mode or degraded startup;
- report the exact safe issue;
- do not silently rewrite it.

## 50. No Silent Reset

Chronicle must not silently reset all user settings to defaults after a parse failure.

## 51. Unknown Keys

Unknown keys are classified:

```text
IgnoredWithWarning
PreservedForFutureVersion
UnsupportedCritical
```

## 52. Strict Root Contract

Unknown critical root sections are rejected.

## 53. Extension Data

Future-compatible noncritical extension data MAY be preserved without activation.

## 54. Field Validation

Validation includes:

- type;
- requiredness;
- bounds;
- path policy;
- enum;
- duration;
- collection size;
- semantic key;
- release-channel support.

## 55. Cross-Field Validation

Examples:

- backup path cannot be inside temporary staging;
- Development secret store requires Development channel;
- remote telemetry endpoint cannot be configured because telemetry is disabled in MVP;
- package directory cannot equal active database directory;
- update channel and application release channel must be compatible.

## 56. Path Validation

Paths are normalized and validated through ADR-0022 policies.

## 57. Relative Paths

Relative paths resolve only against declared Chronicle roots.

## 58. Current Working Directory

The process current working directory is not used as an implicit configuration base.

## 59. Environment Expansion

Environment-variable expansion inside arbitrary path strings is prohibited unless explicitly supported for a field.

## 60. Tilde and Shell Expansion

Shell-specific expansion is not assumed.

## 61. Unsupported Value

Unsupported values produce typed validation errors.

## 62. Configuration Result

Recommended result:

```text
Valid
ValidWithWarnings
Degraded
SafeModeRequired
InvalidFatal
```

## 63. Valid

Normal startup proceeds.

## 64. Valid With Warnings

Normal startup proceeds with safe noncritical warnings.

## 65. Degraded

Selected optional capabilities are disabled.

## 66. Safe Mode Required

Startup proceeds with restricted mutation and recovery UI.

## 67. Invalid Fatal

The process cannot safely continue.

## 68. Safe Mode Integration

Safe Mode may ignore or replace selected noncritical configuration through explicit session overrides.

## 69. Safe Mode Does Not Hide Cause

The invalid source and key remain visible in safe diagnostics.

## 70. Fallback Policy

Fallback is allowed only when the field metadata declares it safe.

## 71. Critical Fallback

Critical storage or security configuration does not fall back silently.

## 72. UI Fallback

Presentation settings may fall back to packaged defaults with a warning.

## 73. Provider Timeout Fallback

Invalid provider timeout may fall back to a safe bounded default when policy declares it noncritical.

## 74. Logging Fallback

If configured log directory is unavailable, Chronicle may use a temporary safe local directory and report degraded diagnostics.

## 75. Configuration Persistence

User changes are written through an Application settings service.

## 76. No Direct File Editing by UI

ViewModels do not write configuration files directly.

## 77. Settings Service

Conceptual service:

```text
IApplicationSettingsService
```

## 78. Settings Change Command

Recommended command:

```text
ChangeApplicationSettingCommand
```

## 79. Batch Settings Command

Recommended:

```text
ApplyApplicationSettingsCommand
```

## 80. Settings Mutation Flow

```text
Load Current Effective Configuration
    ↓
Validate Proposed Changes
    ↓
Build New User Configuration Document
    ↓
Validate Complete Effective Configuration
    ↓
Write Destination-Volume Staging File
    ↓
Flush
    ↓
Reopen and Parse
    ↓
Validate
    ↓
Atomically Replace
    ↓
Update Last Known Valid Copy
    ↓
Apply Runtime-Mutable Changes
    ↓
Report Restart Requirements
```

## 81. Atomic Publication

Configuration file writes follow ADR-0022.

## 82. Previous File Preservation

The previous valid file remains until the new file validates and publishes.

## 83. Write Failure

A write failure leaves the previous valid configuration active.

## 84. Partial Runtime Apply

A batch change either:

- persists fully;
- applies all runtime-mutable values;
- reports restart-required values;
- or fails without partial persistent update.

## 85. Runtime Mutable Settings

Examples may include:

- appearance;
- language when supported;
- log level within safe bounds;
- reduced motion;
- local diagnostic viewer filters.

## 86. Restart-Required Settings

Examples may include:

- data directory;
- release channel;
- package discovery root;
- secret-store implementation;
- provider adapter composition.

## 87. Runtime Change Event

A successful mutable change emits a local configuration-changed event.

## 88. No Domain Event

Application configuration changes are not Campaign Domain Events.

## 89. Configuration Snapshot

Components receive an immutable configuration snapshot or typed options.

## 90. No Ambient Global Reads

Core Application code SHOULD avoid reading environment variables or files directly after startup.

## 91. Options Lifetimes

Immutable-after-startup settings use singleton snapshots.

Runtime-mutable settings use a controlled change-notification mechanism.

## 92. No Service-Locator Configuration

Components receive only the specific typed settings they need.

## 93. Configuration History

Chronicle MAY maintain a safe local history of configuration metadata changes.

## 94. History Exclusions

History does not retain secrets or sensitive raw values when unnecessary.

## 95. Safe History Fields

Examples:

```text
ConfigurationKey
Source
ChangedAtUtc
OperationId
RestartRequirement
Outcome
```

## 96. Value History

Private values may be omitted or represented by safe fingerprints.

## 97. Configuration Migration

Configuration contracts are versioned.

## 98. Migration Trigger

Migration occurs when the user file uses an older supported contract version.

## 99. Migration Staging

Migration writes a new staged file and preserves the old file.

## 100. Migration Determinism

Configuration migration is deterministic and local.

## 101. Migration Operations

A setting may be:

```text
Renamed
Split
Merged
Transformed
Retired
Introduced
```

## 102. Retired Setting

Retired settings are:

- archived;
- preserved as extension data;
- explicitly removed after migration;
- or block migration when meaning is unsafe.

## 103. No Silent Semantic Change

A setting key remains only if its meaning remains stable.

## 104. New Required Setting

A new required setting needs:

- safe packaged default;
- deterministic migration;
- user resolution;
- or Safe Mode.

## 105. Newer Configuration Version

A configuration file from a newer unsupported Chronicle version is not rewritten.

## 106. Newer Version Behavior

Chronicle enters Safe Mode or fails safely and preserves the file.

## 107. Backup

Backups include nonsecret application configuration and version metadata.

## 108. Backup Exclusions

Secret values remain excluded.

## 109. Restore

Restore selectively applies portable configuration.

## 110. Machine-Specific Paths

Machine-specific paths require revalidation on restore.

## 111. Restore Precedence

Restored user configuration does not override command-line or session Safe Mode overrides.

## 112. Campaign Export

Campaign export excludes application configuration except portable provider-neutral preferences explicitly required by the Campaign artifact.

## 113. Environment Overrides and Backup

Environment values are not serialized into backup as explicit user choices unless separately persisted through a settings command.

## 114. Source Inspection

The UI SHOULD show the effective source of a setting.

Example:

```text
Data Directory
    Effective source: Command line
    User file value: C:\...
    Current effective value: D:\...
```

Sensitive values are redacted.

## 115. Configuration UI

The settings UI is generated from curated metadata, not from arbitrary reflection over every field.

## 116. Advanced Settings

Advanced and Development-only settings are clearly separated.

## 117. Manual File Editing

Chronicle may document the configuration file for advanced users.

The application still validates it strictly.

## 118. Configuration Editor

A future built-in raw editor is not required for MVP.

## 119. User Feedback

After saving, Chronicle shows:

- applied now;
- restart required;
- ignored due to override;
- blocked by policy;
- failed with previous value preserved.

## 120. Override Visibility

A user-local change shadowed by environment or command-line override is not presented as effective.

## 121. Reset to Default

Reset removes the user override and resolves the next source.

## 122. Reset Preview

The UI shows the resulting effective value before confirmation.

## 123. Release Channels

Recommended channels:

```text
Development
Preview
Stable
```

## 124. Channel Policy

Each setting declares allowed release channels.

## 125. Development-Only Settings

Examples:

- fake provider;
- fault injection;
- raw synthetic prompt inspection;
- in-memory secret store;
- migration chaos testing.

## 126. Stable Rejection

Stable rejects Development-only settings rather than ignoring dangerous behavior silently.

## 127. Environment Detection

Release channel is determined from signed or packaged application metadata, not only an environment variable.

## 128. No Channel Escalation by Config

A Stable binary cannot become a Development binary merely through user configuration.

## 129. Test Composition

Tests may construct configuration entirely in memory through a test source.

## 130. Test Source

The test source has highest precedence only in test composition.

## 131. Deterministic Tests

Tests do not depend on developer-machine environment variables unless explicitly testing environment override behavior.

## 132. Configuration Diagnostics

Diagnostics MAY include:

- contract version;
- source counts;
- validation result;
- warning keys;
- restart-required count;
- safe value classifications;
- fingerprints.

## 133. No Full Effective Configuration Log

The complete configuration object is not logged.

## 134. Sensitive Value Logging

Sensitive values are redacted or represented by a safe classification.

## 135. Path Diagnostics

Use path classifications or sanitized paths.

## 136. Configuration Fingerprint

Chronicle MAY compute a fingerprint over nonsecret effective settings for diagnostics.

## 137. Fingerprint Scope

The fingerprint excludes:

- secrets;
- volatile values;
- machine identifiers;
- session-only random values.

## 138. Error Model

Recommended errors:

```text
configuration.file-not-found
configuration.file-malformed
configuration.contract-version-unsupported
configuration.key-unknown
configuration.value-type-invalid
configuration.value-out-of-range
configuration.source-not-allowed
configuration.release-channel-blocked
configuration.cross-constraint-failed
configuration.path-invalid
configuration.write-failed
configuration.publication-failed
configuration.migration-required
configuration.migration-failed
configuration.restart-required
configuration.safe-mode-required
configuration.recovery-required
```

## 139. Data Preservation State

Results SHOULD state:

```text
PreviousConfigurationPreserved
NewConfigurationPublished
RuntimeChangesApplied
RestartRequired
LastKnownValidUsed
MalformedFilePreserved
SafeModeRequired
AuthoritativeCampaignDataUnchanged
```

## 140. Logging

Configuration logs MAY include:

- key;
- source;
- validation result;
- contract version;
- OperationId;
- restart requirement;
- safe failure code.

They MUST NOT include secret or sensitive raw values.

## 141. Metrics

Useful metrics include:

```text
ConfigurationLoadDuration
ConfigurationValidationFailureCount
ConfigurationFallbackCount
ConfigurationSafeModeCount
ConfigurationMigrationCount
ConfigurationWriteFailureCount
ConfigurationOverrideCount
```

## 142. Testing Strategy

The implementation requires:

```text
Precedence Tests
Binding Tests
Validation Tests
Cross-Field Tests
File Fault Tests
Migration Tests
Runtime Update Tests
Release-Channel Tests
Backup and Restore Tests
Security Tests
Architecture Tests
```

## 143. Precedence Tests

Tests MUST cover every source combination.

## 144. Binding Tests

Tests MUST cover:

- valid types;
- invalid types;
- missing required values;
- unknown keys;
- extension data;
- unsupported version.

## 145. Validation Tests

Tests MUST cover:

- bounds;
- enums;
- durations;
- paths;
- source restrictions;
- release-channel policy.

## 146. Cross-Field Tests

Tests MUST cover:

- conflicting directories;
- Development secret store in Stable;
- disabled telemetry configuration;
- incompatible update and release channel;
- invalid provider timeout relationships.

## 147. File Fault Tests

Tests MUST simulate:

- malformed JSON;
- permission denied;
- disk full;
- rename failure;
- locked file;
- partial staging write;
- invalid last-known-valid copy.

## 148. Runtime Update Tests

Tests MUST cover:

- mutable setting;
- restart-required setting;
- batch atomicity;
- override shadowing;
- reset to default;
- duplicate OperationId.

## 149. Migration Tests

Tests MUST cover:

- rename;
- split;
- merge;
- new default;
- retired key;
- newer unsupported version;
- migration write failure;
- prior file preservation.

## 150. Release-Channel Tests

Tests MUST prove:

- Stable blocks Development-only settings;
- Preview permits only declared Preview settings;
- test composition can inject test source;
- config cannot escalate binary channel.

## 151. Backup and Restore Tests

Tests MUST prove:

- nonsecret settings preserved;
- secrets excluded;
- machine paths revalidated;
- unresolved provider credentials reported;
- environment overrides not persisted as user values.

## 152. Security Tests

Tests MUST prove:

- raw secrets rejected from config;
- command-line secrets rejected;
- unknown environment keys do not bind;
- full config not logged;
- path and endpoint values sanitized.

## 153. Required Test Cases

Tests MUST cover:

- default startup;
- user file;
- environment override;
- command-line override;
- session override;
- malformed file;
- last-known-valid recovery;
- critical invalid setting;
- optional invalid setting;
- Safe Mode;
- runtime appearance update;
- restart-required storage update;
- Stable Development guard;
- configuration migration;
- backup;
- restore;
- no Campaign Preference override.

## 154. Architecture Tests

Architecture tests MUST reject:

- direct environment reads in Domain or Application;
- direct configuration-file reads in ViewModels;
- raw secret fields in configuration contracts;
- arbitrary environment binding;
- current working directory as implicit root;
- untyped configuration dictionaries in core workflows;
- package-defined root configuration sections;
- Stable acceptance of Development-only values;
- direct file write without atomic publication;
- use of application config as Campaign mechanics authority.

## 155. Prohibited Patterns

### 155.1 Treat Application Config as Campaign Preferences

Keep installation behavior and Campaign rules separate.

### 155.2 Bind Every Environment Variable Automatically

Only declared keys participate.

### 155.3 Put API Keys in JSON

Use Secret References.

### 155.4 Reset Malformed Config Silently

Preserve and report it.

### 155.5 Apply Restart-Required Setting Partially

Persist and report restart, or reject.

### 155.6 Let Stable Enable Development Mode Through Config

Release channel is packaging authority.

### 155.7 Read Config Ad Hoc Throughout the Application

Use typed snapshots and services.

### 155.8 Write Settings Directly from UI

Use Application settings workflow.

### 155.9 Log the Entire Effective Configuration

Log safe metadata only.

### 155.10 Allow Environment Override to Change Dice Rules

Campaign Preferences and Rule Sets remain authoritative.

## 156. Alternatives Considered

### One Unstructured JSON Dictionary

Rejected because validation, migration, source restrictions, and runtime mutability would be weak.

### Environment Variables Only

Rejected for desktop usability and because they are poor persistent user settings.

### Database-Only Application Configuration

Possible, but startup storage and recovery settings must be available before the database is fully usable.

### Registry-Only Configuration

Rejected because it reduces portability and complicates inspection and versioning.

### Silent Defaults on Every Error

Rejected because critical ambiguity could threaten storage and security.

### Hot Reload Every Setting

Rejected because many settings cannot change safely during runtime.

## 157. Consequences

### Positive

- startup behavior is deterministic;
- invalid settings are visible;
- Stable and Development remain separated;
- Campaign mechanics cannot be changed by environment;
- settings writes are recoverable;
- migrations preserve intent;
- overrides are inspectable;
- tests can construct configuration deterministically.

### Negative

- configuration metadata and validators require maintenance;
- multiple sources increase test combinations;
- Safe Mode needs a configuration-inspection UI;
- some changes require restart;
- last-known-valid handling adds files and recovery logic;
- advanced users cannot rely on arbitrary keys.

## 158. Risks

### User File and Effective Value Differ

Mitigation:

- source inspection;
- override warnings;
- reset preview.

### Critical Setting Falls Back Unsafely

Mitigation:

- field criticality;
- explicit fallback metadata;
- Safe Mode or fail fast.

### Configuration Migration Loses Intent

Mitigation:

- versioned migrations;
- old file preservation;
- semantic tests.

### Development Setting Reaches Stable

Mitigation:

- packaged release-channel authority;
- Stable guard;
- architecture and release tests.

### Manual Editing Causes Startup Failure

Mitigation:

- last-known-valid copy;
- Safe Mode;
- precise validation messages;
- no silent overwrite.

## 159. Technology Spike

Before acceptance, implement:

1. root typed configuration contract;
2. field metadata registry;
3. packaged defaults;
4. user-local JSON source;
5. explicit environment registry;
6. command-line parser;
7. deterministic precedence;
8. validation pipeline;
9. last-known-valid recovery;
10. atomic settings writer;
11. runtime settings service;
12. restart-requirement reporting;
13. one configuration migration;
14. Stable Development guard;
15. configuration-inspection diagnostics.

## 160. Spike Acceptance

The spike passes when:

- the same source inputs always produce the same effective configuration;
- unknown environment variables cannot bind accidentally;
- Campaign mechanics remain unaffected by application overrides;
- malformed configuration preserves the bad file and uses explicit recovery;
- critical invalid configuration enters Safe Mode or fails safely;
- valid changes publish atomically;
- failed writes preserve the prior valid file;
- runtime-mutable settings apply without restart;
- restart-required settings remain pending and clearly reported;
- Stable blocks Development-only configuration;
- no raw secret reaches the configuration file or diagnostics.

## 161. Definition of Compliance

An implementation complies when:

- application configuration is distinct from Campaign Preferences;
- contracts are typed and versioned;
- sources and precedence are explicit;
- only declared environment and command-line overrides are accepted;
- secrets remain outside ordinary configuration;
- complete effective configuration is validated before normal startup;
- critical invalid values fail safely or require Safe Mode;
- user configuration writes use staging, validation, and atomic publication;
- prior valid configuration is preserved;
- runtime mutability and restart requirements are explicit;
- configuration migrations are deterministic;
- Stable cannot be converted to Development by configuration;
- diagnostics expose source and outcome without leaking sensitive values.

## 162. Review Triggers

This ADR must be reviewed if:

- Linux or macOS support changes configuration locations;
- server hosting introduces centralized configuration;
- remote administration is introduced;
- cloud synchronization includes application settings;
- organization policy overrides are added;
- feature flags become remotely managed;
- a plugin SDK needs package-specific installation settings;
- configuration encryption is proposed;
- multiple user profiles are introduced;
- live reload becomes required for critical settings.

## 163. Deferred Decisions

Later ADRs MAY define:

- exact JSON serializer settings;
- exact configuration file path per OS;
- organization-managed policy source;
- remote feature flags;
- cloud-synchronized settings;
- multiple user profiles;
- encrypted sensitive configuration;
- public configuration schema;
- raw advanced configuration editor;
- live reload for additional settings;
- server environment precedence.

## 164. Final Decision

Chronicle will load application configuration from explicit, typed, versioned sources with deterministic precedence.

It will validate the complete effective configuration before normal startup.

Secrets will remain outside ordinary settings.

Development overrides will remain outside Stable authority.

Configuration may decide where Chronicle stores files, how it logs, and which provider profile it may use.

It will never silently decide what happened in a Campaign or how its rules should be interpreted.
