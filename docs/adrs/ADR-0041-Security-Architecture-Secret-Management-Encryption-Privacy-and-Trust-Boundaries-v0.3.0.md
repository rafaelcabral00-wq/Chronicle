---
id: ADR-0041
title: Security Architecture, Secret Management, Encryption, Privacy, and Trust Boundaries
status: Proposed
version: 0.3.0
owner: Chronicle Team
last_updated: 2026-08-02
category: Security
supersedes:
  - ADR-0041@0.2.0
  - ADR-0041@0.1.0
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
  - ADR-0027
  - ADR-0028
  - ADR-0033
  - ADR-0034
  - ADR-0036
  - ADR-0038
  - ADR-0039
  - ADR-0040
  - ADR-0042
implements: []
related_to:
  - ADR-0043
  - ADR-0044
---

> **"The LLM is an advisor, never the authority. Chronicle decides, persists, and protects truth."**

# Security Architecture, Secret Management, Encryption, Privacy, and Trust Boundaries

## 1. Status

**Proposed**

This ADR defines Chronicle's security architecture from MVP onward.

This revision consolidates and corrects the security decisions required by encrypted persistence, encrypted backup, provider isolation, package trust, privacy, recovery, and no-rework MVP foundations.

The decision is:

- encrypt the authoritative SQLite database at rest from the first production schema;
- encrypt every published installation backup from MVP onward;
- keep installation database keys, backup recovery material, and provider credentials in separate secret domains;
- protect installation database keys through Windows-native secret protection;
- never persist plaintext secrets in SQLite, files, logs, crash reports, manifests, portable exports, or provider prompts;
- use authenticated encryption for backup artifacts;
- support clean-install recovery through backup-specific recovery material;
- use Chronicle-owned abstractions for encrypted connection, key retrieval, rekey, backup encryption, restore, and secure staging;
- treat Narrative Intelligence providers, Rule Set packages, imported artifacts, backups, and generated output as untrusted inputs;
- allow providers and Rule Sets to propose or validate only through explicit contracts;
- keep Chronicle as the sole authority for persistence, random generation, state transition, and history;
- use allowlisted Structured Events and closed contract registries;
- validate all external and package-defined payloads before authoritative application;
- keep provider-native threads and output nonauthoritative;
- use Windows Credential Manager or an approved DPAPI-backed implementation for provider credentials and installation key protection;
- never use provider credentials as database or backup keys;
- keep raw provider responses transient and protected;
- prevent unauthorized sourcebook content from entering the repository or distributed packages;
- sign Stable installers and verify release artifacts;
- preserve privacy by default and collect no required remote telemetry;
- make security failures preserve data rather than reset or overwrite it;
- keep English as the base language for security keys, logs, diagnostics, and documentation;
- implement the foundation in MVP so the complete product does not require architectural replacement.

## 2. Security Principle

Chronicle follows:

```text
A provider may propose.
A Rule Set may validate.
Chronicle decides.
Chronicle persists.
Chronicle owns truth.
Chronicle protects truth.
```

## 3. Security Goals

Chronicle must protect:

- Campaign confidentiality;
- Character confidentiality;
- database integrity;
- Dice evidence integrity;
- backup confidentiality;
- migration recoverability;
- provider credentials;
- database keys;
- backup recovery material;
- package provenance;
- release integrity;
- user ownership of local data.

## 4. Non-Goals

The MVP does not claim to protect against:

- a fully compromised operating-system administrator;
- malware executing as the current user while Chronicle is unlocked;
- physical memory extraction from a running process;
- malicious firmware;
- compromised provider accounts;
- user disclosure of recovery secrets;
- unavailable provider-side data after sending a request.

## 5. Trust Boundaries

Canonical boundaries:

```text
Trusted Chronicle Core
    Domain
    Application
    approved Persistence and Infrastructure implementations

Conditionally Trusted Components
    signed official desktop binaries
    approved encrypted SQLite provider
    approved official Rule Set packages

Untrusted Inputs
    provider responses
    imported packages
    backups before validation
    portable exports before validation
    user-authored package content
    filesystem artifacts
    external URLs
    generated Structured Events
```

## 6. Authority Boundary

Only Chronicle Application may authorize authoritative mutation.

## 7. Domain Authority

Domain invariants remain the final semantic guard.

## 8. Provider Boundary

Provider adapters may:

- send provider-neutral requests;
- normalize responses;
- expose safe metadata.

They may not:

- persist Campaign truth;
- generate authoritative Dice values;
- apply Structured Events;
- retrieve database keys;
- access DbContext;
- manage backups.

## 9. Rule Set Boundary

Rule Set packages may:

- validate package-owned mechanics;
- resolve committed Dice evidence;
- provide schemas and operation contracts;
- propose deterministic effects.

They may not:

- access DbContext;
- access provider credentials;
- open arbitrary files;
- perform unrestricted network calls;
- mutate Campaign state directly;
- generate Chronicle-owned randomness.

## 10. Presentation Boundary

Presentation may request actions and display results.

It may not access secrets, DbContext, provider SDKs, or encryption APIs directly.

## 11. Database Encryption

Chronicle encrypts SQLite at rest from the first production schema.

## 12. Encryption Provider Requirements

The selected provider must support:

- transparent encryption;
- EF Core;
- maintained native binaries;
- authenticated or integrity-checked database behavior as supported;
- WAL or documented journal compatibility;
- rekey;
- migration;
- backup and restore;
- supported licensing.

## 13. Database Key

Each data root receives a unique high-entropy database key.

## 14. Key Generation

Use the operating-system cryptographic random source.

## 15. Key Prohibitions

The database key is not:

- hardcoded;
- derived from username;
- derived from machine name;
- derived from Campaign metadata;
- reused as a provider credential;
- reused directly as a backup key.

## 16. Key Protection

The database key is protected using:

```text
Windows Credential Manager
```

or an approved DPAPI-backed secret store.

## 17. Key Storage Separation

The protected key remains outside the database file and application binaries.

## 18. Key Reference

Only an opaque reference may be persisted in nonsecret metadata.

## 19. Key Retrieval

Key retrieval occurs only through approved Infrastructure services.

## 20. Key Lifetime

Plaintext key material remains in memory for the shortest practical period.

## 21. Missing Key

If the key is missing:

- do not open the database;
- do not create a replacement database over existing data;
- enter recovery;
- preserve all files unchanged.

## 22. Wrong Key

Wrong-key failure is distinguished from corruption.

## 23. Key Rotation

The architecture supports key rotation from MVP.

## 24. Rekey Safety

Rekey requires:

- OperationId;
- exclusive database access;
- encrypted checkpoint;
- journal;
- old and new protected key references;
- post-rekey validation;
- rollback path.

## 25. Key Retirement

Old key material is removed only after the new database/key pairing validates.

## 26. Backup Encryption

Every published backup is encrypted.

## 27. Backup Key Separation

Backup data keys are independent from installation database keys.

## 28. Backup Recovery

At least one clean-install-compatible recovery method is mandatory.

## 29. Backup Recovery Candidates

Approved candidates:

- recovery secret protected through a modern memory-hard KDF;
- high-entropy recovery key file;
- multiple wrapping slots in a future revision.

## 30. No Sole Windows Binding

Windows user protection may provide convenience unlocking but cannot be the only backup recovery path.

## 31. Authenticated Encryption

Backup encryption must protect confidentiality and integrity.

## 32. No Plaintext Backup Staging

Backup creation must not write a durable plaintext archive or database copy.

## 33. Restore Security

Restore:

- treats the artifact as hostile input;
- validates framing and authentication;
- stages encrypted data;
- validates the encrypted SQLite database;
- creates an encrypted rollback checkpoint;
- publishes only after validation.

## 34. Secret Domains

Canonical domains:

```text
Installation Database Key
Backup Data Key
Backup Recovery Secret or Key
Provider Credentials
Code-Signing Keys
Package-Signing Keys
```

## 35. Secret Reuse Prohibition

No secret domain is reused as another.

## 36. Provider Credentials

Provider credentials are stored only in Windows Credential Manager or an approved equivalent.

## 37. Credential Prohibitions

Credentials must not enter:

- SQLite;
- config files;
- backup manifests;
- portable exports;
- logs;
- diagnostics;
- provider-response staging;
- package payloads.

## 38. Provider Request Privacy

Chronicle sends only the minimum context required for the selected role and operation.

## 39. Context Minimization

Provider context selection must be explicit and testable.

## 40. No Automatic Full Campaign Upload

Chronicle must not send the entire Campaign history by default.

## 41. Provider Disclosure

The UI must disclose when content is sent to a remote provider.

## 42. Provider Thread Nonauthority

Remote threads are optional optimization only.

## 43. Raw Provider Response

Raw responses are transient.

## 44. Response Staging

Staging must be:

- protected locally;
- bounded in size;
- bounded in lifetime;
- excluded from ordinary export;
- deleted after acceptance or terminal rejection when policy allows.

## 45. Structured Output Validation

All provider output passes:

```text
transport validation
contract validation
event registry validation
payload validation
authority validation
Rule Set validation
continuity validation
staleness validation
```

## 46. Closed Event Registry

Only registered Structured Event types may reach Application routing.

## 47. Unknown Event

Unknown events are rejected.

## 48. Narrative Text Is Not Command Authority

Narrative prose cannot mutate state merely by containing imperative text.

## 49. Dice Security

Chronicle owns random generation.

## 50. Random Evidence

Committed random evidence is append-only and immutable.

## 51. No Provider Dice

Providers may request a Roll but may not supply authoritative Dice values.

## 52. Rule Set Dice Role

Rule Sets interpret committed evidence.

They do not replace the Chronicle random source.

## 53. Retry Integrity

A retry must not regenerate committed random evidence.

## 54. Package Security

Rule Set packages are treated as bounded extension packages.

## 55. Package Manifest

Every package declares:

- PackageId;
- version;
- contract compatibility;
- requested capabilities;
- provenance;
- license;
- content hashes;
- signature status where supported.

## 56. Capability Model

Packages receive only approved capabilities.

## 57. Default Package Restrictions

By default, packages do not receive:

- arbitrary network access;
- arbitrary filesystem access;
- secret access;
- process execution;
- DbContext access;
- UI injection beyond declared contracts.

## 58. Package Signature

Official packages should be signed once the package format supports verified signatures.

## 59. Unsigned Package Policy

Unsigned third-party packages require explicit user trust and restricted handling.

## 60. Package Hash Verification

Installed package files are hash-verified.

## 61. Package Provenance

Committed package knowledge artifacts require:

- source classification;
- license status;
- transformation provenance;
- redistribution permission;
- reviewer.

## 62. Unauthorized Sourcebook Content

Chronicle repositories and distributed official packages must not include unauthorized sourcebook text or assets.

## 63. Import Security

Imported Campaign packages, backups, and Rule Set packages are untrusted.

## 64. Archive Safety

Reject:

- path traversal;
- absolute paths;
- symlinks;
- junctions;
- duplicate canonical paths;
- case collisions;
- oversized entries;
- decompression bombs;
- malformed Unicode;
- unexpected executable content.

## 65. Size Limits

All imported or provider-provided content uses bounded:

- bytes;
- nesting depth;
- item counts;
- string lengths;
- decompressed size.

## 66. Deserialization

Use strict, versioned, bounded deserialization.

## 67. Unknown Fields

Unknown-field behavior is contract-specific and explicit.

## 68. No Polymorphic Arbitrary Type Loading

Do not deserialize arbitrary runtime types from untrusted data.

## 69. Filesystem Security

Chronicle uses managed data roots and opaque staging paths.

## 70. Path Safety

All paths are canonicalized and validated before use.

## 71. Same-Volume Publication

Sensitive artifact publication prefers same-volume atomic rename.

## 72. Temporary Files

Sensitive temporary files must remain encrypted or be avoided.

## 73. Logs

Stable logs must not contain:

- credentials;
- database keys;
- backup keys;
- recovery secrets;
- connection strings with secrets;
- full prompts;
- raw provider responses;
- full Campaign prose;
- private Character details.

## 74. Safe Log Fields

Allowed examples:

- typed IDs;
- state keys;
- contract versions;
- package IDs;
- provider adapter key;
- safe hashes;
- durations;
- safe error codes.

## 75. EF Core Sensitive Logging

Development-only and still prohibited from revealing encryption keys.

## 76. Crash Reports

Crash artifacts are redacted and local by default.

## 77. Telemetry

No remote telemetry is required for Chronicle operation.

## 78. Future Telemetry

Any future telemetry must be:

- opt-in or separately approved;
- privacy-minimized;
- documented;
- free of Campaign content and secrets.

## 79. Network Security

Remote provider communication uses supported TLS through the provider SDK or HTTP stack.

## 80. Certificate Validation

Certificate validation must not be disabled.

## 81. Proxy Behavior

Proxy support must not leak credentials to unapproved destinations.

## 82. URL Handling

External URLs are opened only through explicit user action and safe platform APIs.

## 83. Release Security

Stable installers are code-signed.

## 84. Release Hashes

Published release artifacts include cryptographic hashes.

## 85. Dependency Integrity

Builds use:

- pinned package versions;
- lock or central package management;
- SBOM;
- vulnerability review;
- native binary provenance;
- secret scanning.

## 86. Code-Signing Key

Code-signing keys are never committed to the repository.

## 87. Build Isolation

Release signing occurs in a controlled environment.

## 88. Update Security

MVP updates are manual.

## 89. Future Updater

A future updater must verify:

- signed metadata;
- artifact signature;
- hashes;
- release channel;
- rollback compatibility.

## 90. Installer Boundary

Installer does not retrieve database keys or migrate data.

## 91. Migration Security

Migration runs through Chronicle's encrypted connection boundary.

## 92. Migration Checkpoint

Destructive migration requires a validated encrypted checkpoint.

## 93. No Plaintext Migration

No durable plaintext database copy is permitted.

## 94. Encryption Migration

Cipher, KDF, page-size, or provider changes require explicit migration and rollback.

## 95. Uninstall

Ordinary uninstall preserves encrypted data and protected key records.

## 96. Complete Removal

Complete removal is explicit and warns about irreversible loss.

## 97. Channel Isolation

Stable, Preview, and Development use separate:

- data roots;
- database keys;
- backup catalogs;
- migration journals.

## 98. No Shared Secret Assumption

Release channels do not share protected database keys by default.

## 99. Safe Mode

Safe Mode handles:

- missing key;
- wrong key;
- unsupported cipher;
- migration failure;
- rekey uncertainty;
- corrupt backup;
- restore failure;
- package incompatibility.

## 100. Safe Mode Restrictions

Safe Mode blocks normal Campaign mutation and provider execution.

## 101. Recovery Principle

Security failures preserve data and evidence.

They do not trigger silent reset.

## 102. Security Error Model

Recommended errors:

```text
security.secret-unavailable
security.secret-reference-invalid
security.secret-access-denied
security.wrong-database-key
security.cipher-unsupported
security.encryption-initialization-failed
security.rekey-failed
security.backup-authentication-failed
security.recovery-material-invalid
security.credential-missing
security.provider-output-untrusted
security.package-untrusted
security.package-signature-invalid
security.archive-unsafe
security.path-unsafe
security.content-limit-exceeded
security.provenance-missing
security.release-signature-invalid
security.recovery-required
```

## 103. Data Preservation State

Security-sensitive results should state:

```text
AuthoritativeDataUnchanged
SecretUnavailable
EncryptedDataPreserved
InputRejected
PackageQuarantined
BackupRejected
MigrationBlocked
NormalMutationBlocked
RecoveryRequired
```

## 104. User-Facing Security UX

The UI must communicate:

- database encryption status;
- backup recovery readiness;
- missing credentials;
- package trust;
- remote provider disclosure;
- recovery requirements;
- irreversible deletion warnings.

## 105. Security Readiness Indicator

Chronicle should expose a local status summarizing:

```text
Database encrypted
Backup recovery configured
Latest backup verified
Provider credentials configured
Package trust status
Recovery action required
```

## 106. No False Assurance

The UI must not claim that encryption protects data while Chronicle is unlocked and the current process can access it.

## 107. Recovery-Material Education

The user must understand that losing all recovery material can make encrypted backups unrecoverable.

## 108. Threat Modeling

Security-sensitive features require lightweight threat modeling before implementation.

## 109. Required Threat Models

At minimum:

- encrypted database and key storage;
- encrypted backup and clean-install restore;
- provider context and response handling;
- package loading;
- import/export;
- update and migration;
- complete data deletion.

## 110. Security Testing

The implementation requires:

```text
Secret Storage Tests
Database Encryption Tests
Backup Encryption Tests
No-Plaintext Tests
Provider Isolation Tests
Package Sandbox Tests
Archive Safety Tests
Structured Output Tests
Dice Integrity Tests
Migration Security Tests
Logging Tests
Release Integrity Tests
Fault Injection Tests
Architecture Tests
```

## 111. Secret Storage Tests

Tests prove:

- database key is protected;
- provider credentials are separate;
- wrong identity cannot silently unlock;
- secrets never enter SQLite or config files.

## 112. No-Plaintext Tests

Use known canaries and scan:

- database;
- WAL;
- journal;
- temp directories;
- backup staging;
- restore staging;
- migration staging;
- logs;
- crash reports.

## 113. Provider Isolation Tests

Tests prove providers cannot:

- access DbContext;
- access secrets;
- write authoritative state;
- provide Dice values;
- bypass event validation.

## 114. Package Security Tests

Tests prove packages cannot access ungranted capabilities.

## 115. Archive Tests

Test traversal, links, collisions, bombs, oversized inputs, and malformed framing.

## 116. Dice Integrity Tests

Tests prove retries preserve committed evidence.

## 117. Migration Security Tests

Tests prove checkpoint, rekey, rollback, and no plaintext staging.

## 118. Logging Tests

Synthetic keys and Campaign canaries must not appear in Stable logs.

## 119. Release Tests

Tests verify signatures, hashes, SBOM, and native binary inventory.

## 120. Architecture Tests

Architecture tests must reject:

- Domain references to secret or encryption implementations;
- Application references to provider SDKs;
- provider adapter access to Persistence;
- Rule Set access to DbContext;
- Presentation access to secrets;
- direct SQLite connections outside Persistence;
- credential fields in persistence entities;
- raw provider response indefinite retention;
- plaintext production database creation;
- plaintext backup publication;
- provider-generated Dice values;
- unknown Structured Event routing;
- unproven package provenance in official artifacts.

## 121. Security Review Gates

Security review is required before:

- selecting the encrypted SQLite provider;
- selecting backup encryption framing;
- enabling unsigned third-party packages;
- adding remote telemetry;
- adding an automatic updater;
- adding cloud backup;
- adding server-hosted authority;
- adding shared multi-user data roots.

## 122. Prohibited Patterns

### 122.1 LLM as Authority

Providers propose only.

### 122.2 Rule Set Direct Persistence

Chronicle applies validated results.

### 122.3 Secrets in SQLite

Use protected secret stores.

### 122.4 Hardcoded Encryption Key

Use installation-specific random keys.

### 122.5 Database Key Reused for Backup

Separate secret domains.

### 122.6 Plaintext Staging

Encrypt or avoid.

### 122.7 Full Campaign Upload by Default

Minimize context.

### 122.8 Raw Provider Response Forever

Use bounded staging.

### 122.9 Unknown Event Execution

Use a closed registry.

### 122.10 Silent Reset on Security Failure

Preserve data and enter recovery.

## 123. Alternatives Considered

### OS Protection Only

Rejected because database and backup files require application-level confidentiality from MVP.

### Encrypt Only Sensitive Columns

Rejected because nearly all Chronicle data can be sensitive and field-level encryption would complicate queries, migrations, and evidence integrity.

### One Master Product Key

Rejected because compromise would expose every user.

### Provider-Owned Conversation State

Rejected because it weakens privacy, portability, and local authority.

### Trust All Rule Set Packages

Rejected because packages are executable extensions with provenance and capability risks.

### Collect Diagnostic Telemetry by Default

Rejected because local-first privacy is the default.

## 124. Consequences

### Positive

- security foundation exists from MVP;
- no later encryption rewrite;
- provider and package trust boundaries are explicit;
- backups are confidential and recoverable;
- secret domains remain isolated;
- migration and uninstall behavior preserve recoverability;
- privacy and local ownership are clear;
- architecture tests can enforce boundaries.

### Negative

- MVP implementation is more demanding;
- key and recovery UX are required early;
- native encrypted SQLite provider must be maintained;
- package capability controls add work;
- security testing surface is broad;
- key loss can make data unrecoverable.

## 125. Risks

### Recovery Material Loss

Mitigation:

- setup verification;
- clear education;
- multiple future wrapping slots;
- backup readiness indicator.

### Native Encryption Dependency Risk

Mitigation:

- provenance;
- pinned versions;
- SBOM;
- compatibility testing;
- migration plan.

### Provider Privacy Leakage

Mitigation:

- context minimization;
- explicit disclosure;
- local authoritative memory;
- no automatic full-history upload.

### Malicious Package

Mitigation:

- capability restriction;
- provenance;
- signatures;
- explicit trust;
- architecture and sandbox tests.

### Plaintext Leakage Through Tooling

Mitigation:

- canary scans;
- safe staging;
- Stable log restrictions;
- review gates.

## 126. Technology Spike

Before acceptance, implement:

1. encrypted SQLite provider evaluation;
2. Windows-protected secret store;
3. database key lifecycle;
4. encrypted backup framing;
5. clean-install restore;
6. provider credential isolation;
7. response-staging protection;
8. closed event registry validation;
9. package capability prototype;
10. package provenance scanner;
11. archive safety validator;
12. Stable log redaction;
13. no-plaintext canary suite;
14. release signing verification;
15. security architecture tests;
16. recovery UX prototype.

## 127. Spike Acceptance

The spike passes when:

- production SQLite is encrypted;
- clean-install encrypted backup restore works;
- no plaintext key or Campaign canary appears in durable staging or logs;
- provider credentials remain outside SQLite;
- provider output cannot mutate state directly;
- unknown Structured Events are rejected;
- provider retries do not regenerate Dice;
- an untrusted package cannot access DbContext, secrets, or unrestricted filesystem;
- installer and migration preserve encrypted data;
- ordinary uninstall preserves key/data recoverability;
- security architecture tests fail on intentionally introduced boundary violations.

## 128. Definition of Compliance

An implementation complies when:

- Chronicle owns all authoritative state;
- database and backups are encrypted from MVP;
- secret domains are separated;
- keys and credentials never enter ordinary persistence or logs;
- clean-install backup recovery exists;
- providers and Rule Sets operate through bounded contracts;
- raw provider content is transient;
- Structured Events use a closed registry;
- Dice evidence is Chronicle-owned and immutable;
- packages have provenance and bounded capabilities;
- imports and backups are treated as hostile until validated;
- release artifacts are signed and hashed;
- security failures preserve data and enter recovery;
- no security foundation is intentionally deferred in a way that requires architectural replacement.

## 129. Review Triggers

Review this ADR if:

- Chronicle supports macOS or Linux;
- hardware-backed keys are added;
- cloud backup is introduced;
- remote telemetry is introduced;
- automatic updates are introduced;
- third-party executable packages become broadly supported;
- server-hosted authority is introduced;
- multi-user local data roots are introduced;
- enterprise key escrow is required.

## 130. Deferred Decisions

Later decisions may define:

- exact encrypted SQLite distribution;
- exact backup cipher and KDF;
- hardware-backed secret storage;
- launch-password profile;
- multiple backup key slots;
- enterprise escrow;
- package sandbox implementation;
- signed third-party package marketplace;
- cloud backup;
- opt-in telemetry;
- automatic updater security.

## 131. Final Decision

Chronicle's MVP security architecture is not temporary.

The database is encrypted.

Backups are encrypted and recoverable.

Secrets are separated.

Providers and Rule Sets remain outside authority.

Imported content is untrusted until validated.

Failures preserve data.

Chronicle may begin with fewer features.

It will not begin with disposable security.
