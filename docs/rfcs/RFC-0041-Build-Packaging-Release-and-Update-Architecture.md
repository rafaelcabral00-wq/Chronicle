---
id: RFC-0041
title: Build, Packaging, Release, and Update Architecture
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-08-01
category: Delivery
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
  - RFC-0037
  - RFC-0038
  - RFC-0039
  - RFC-0040
---

> **"A release is not complete when the binaries exist. It is complete when users can install, upgrade, recover, and verify them without risking their Campaigns."**

# Build, Packaging, Release, and Update Architecture

## Abstract

This RFC defines Chronicle's build, packaging, release, distribution, upgrade, rollback, and update architecture.

It establishes:

- reproducible builds;
- version identity;
- artifact composition;
- platform packaging;
- package signing;
- checksums;
- release channels;
- release manifests;
- Rule Set package compatibility;
- storage migration coordination;
- update staging;
- rollback;
- failure recovery;
- installer behavior;
- portable builds;
- open-source release artifacts;
- supply-chain controls;
- release observability;
- quality gates.

The release system MUST protect Campaign continuity.

An application update MUST NOT silently invalidate active Campaigns, replace required Rule Set package versions, reinterpret historical mechanics, or publish partially migrated storage.

The MVP may begin with manual update delivery.

Its architecture MUST still preserve a safe path toward signed automatic updates later.

## 1. Purpose

Chronicle will be distributed as an open-source framework and an official desktop application.

That introduces delivery risks beyond compilation:

- incompatible binaries;
- missing runtime components;
- unsigned or tampered installers;
- migration failure;
- overwritten Campaign data;
- Rule Set package incompatibility;
- stale configuration;
- broken rollback;
- platform-specific path errors;
- accidental inclusion of credentials;
- unreproducible release artifacts;
- supply-chain compromise;
- users installing unsupported builds.

This RFC defines how Chronicle moves from source code to a trustworthy installed application.

## 2. Scope

This RFC defines:

- source and build boundaries;
- versioning;
- build profiles;
- reproducibility;
- artifact naming;
- desktop packages;
- installers;
- portable builds;
- manifests;
- signatures and checksums;
- release channels;
- release process;
- migration coordination;
- update staging;
- rollback;
- package compatibility;
- dependency inventory;
- release notes;
- source distribution;
- observability;
- testing;
- MVP delivery decisions.

This RFC does not define:

- one CI platform;
- one desktop framework;
- one package manager;
- one code-signing authority;
- one updater library;
- exact release cadence;
- marketplace publication;
- cloud deployment.

## 3. Core Principle

Delivery is part of data safety.

A release is acceptable only when:

- its contents are known;
- its versions are explicit;
- its integrity can be verified;
- its migrations are tested;
- its dependencies are declared;
- failure does not destroy the user's prior working state.

## 4. Release Units

Chronicle distinguishes:

```text
Framework Source Release
Official Desktop Application Release
Rule Set Package Release
Character Schema Release
Portable Artifact Format Release
Documentation Release
```

These MAY share a repository and release train, but they retain independent version identities where needed.

## 5. Application Version

The official application MUST expose a stable application version.

It SHOULD identify:

- major;
- minor;
- patch;
- prerelease label;
- build metadata where useful.

Example:

```text
1.2.0-beta.3+build.481
```

## 6. Contract Versions

Application version is separate from:

- persistence schema version;
- export format version;
- Rule Set package version;
- Character schema version;
- Narrative contract version;
- Preference catalog version;
- observability schema version.

A release manifest MUST preserve these distinctions.

## 7. Versioning Policy

Chronicle SHOULD use semantic versioning principles.

### Major Version

May include intentionally breaking public contracts or unsupported migration boundaries.

### Minor Version

Adds backward-compatible features or new contracts.

### Patch Version

Fixes behavior without intentional public contract breakage.

Pre-1.0 compatibility expectations MUST still be documented explicitly.

## 8. Build Identity

Every build SHOULD have:

```text
ApplicationVersion
SourceRevision
BuildTimestamp
BuildProfile
TargetPlatform
TargetArchitecture
BuildNumber
DirtySourceFlag
```

Production releases MUST use clean source state.

## 9. Build Profiles

Initial profiles SHOULD include:

```text
Development
Test
Release
Diagnostic
```

## 10. Development Build

May include:

- developer diagnostics;
- scripted providers;
- local fixtures;
- unsigned binaries;
- verbose logs.

It MUST be visually distinguishable from a production release where practical.

## 11. Test Build

Used for CI and automated validation.

It MAY include fault-injection hooks and test adapters.

It MUST NOT be distributed as a production build.

## 12. Release Build

A Release build MUST use:

- production-safe configuration;
- developer mode disabled;
- no test credentials;
- no private fixtures;
- no unrestricted diagnostics;
- optimized and validated binaries;
- release manifest;
- license inventory.

## 13. Diagnostic Build

A Diagnostic build MAY enable additional local instrumentation.

It requires explicit distribution and privacy warnings.

## 14. Reproducible Builds

Chronicle SHOULD move toward reproducible builds.

A reproducible build means that equivalent declared inputs produce equivalent artifacts, subject to documented platform limits.

## 15. Reproducibility Inputs

Build inputs SHOULD include:

- source revision;
- dependency lockfiles;
- compiler and runtime versions;
- build scripts;
- platform SDK versions;
- package manifests;
- localization resources;
- migration assets.

## 16. Nonreproducible Inputs

Builds SHOULD avoid:

- current-time-dependent generated content;
- network-fetched unpinned dependencies;
- local developer paths;
- machine-specific configuration;
- environment-dependent file ordering;
- embedded credentials.

## 17. Dependency Locking

Release builds MUST use pinned or locked dependency versions where supported.

Lockfiles SHOULD be committed and reviewed.

## 18. Dependency Inventory

Every release SHOULD generate a dependency inventory.

It SHOULD include:

- package name;
- version;
- license;
- source;
- direct or transitive status;
- security-review status where available.

## 19. Software Bill of Materials

Chronicle SHOULD support generating a software bill of materials for Release builds.

The exact format requires an ADR.

## 20. Build Artifact Types

A release MAY produce:

```text
Installer
Portable Archive
Package Manager Artifact
Source Archive
Symbols or Debug Metadata
Checksums
Signatures
Release Manifest
SBOM
License Bundle
```

## 21. Artifact Naming

Artifact names SHOULD include:

- product;
- version;
- platform;
- architecture;
- package type.

Example:

```text
chronicle-1.0.0-windows-x64-installer
```

The exact extension depends on platform.

## 22. Release Manifest

Every official release MUST include a machine-readable manifest.

It SHOULD contain:

```text
ApplicationVersion
SourceRevision
ReleaseChannel
SupportedPlatforms
SupportedArchitectures
StorageSchemaVersion
MinimumSupportedStorageVersion
ExportFormatVersions
BundledRuleSetPackages
RequiredRuntime
ArtifactChecksums
SignatureMetadata
MigrationMetadata
KnownCompatibilityLimits
ReleaseTimestamp
```

## 23. Manifest Authority

The manifest is authoritative for artifact composition.

File names and release-page prose are informative but not sufficient.

## 24. Platform Packages

Each platform package SHOULD follow native expectations for:

- installation;
- uninstallation;
- shortcuts;
- file permissions;
- application-data location;
- credential-store integration;
- executable signing;
- update integration.

## 25. Installer Responsibilities

An installer MAY:

- install binaries;
- install required runtime;
- create shortcuts;
- register file associations;
- register URI schemes later;
- install uninstaller metadata;
- configure application location.

It MUST NOT:

- create provider credentials;
- overwrite Campaign data;
- silently move the data directory;
- execute destructive migrations without application safeguards.

## 26. User Data Separation

Application binaries and user data MUST use separate locations.

Updating or uninstalling binaries MUST NOT delete Campaign data by default.

## 27. Uninstall Behavior

The uninstaller SHOULD distinguish:

```text
Remove Application
Remove Application Configuration
Remove Local Campaign Data
Remove Backups and Exports
Remove Credentials
```

Destructive user-data removal requires explicit confirmation.

## 28. Portable Build

A portable build MAY store binaries and application data in a user-selected directory.

It requires explicit behavior for:

- credential storage;
- data directory;
- backups;
- process locks;
- updates;
- file permissions.

Portable mode MUST be clearly identified.

## 29. Installer Versus Portable

Installer and portable packages MAY coexist.

They MUST not silently share or move data directories.

## 30. Bundled Rule Set Packages

The official application MAY bundle one or more approved Rule Set packages.

The release manifest MUST list exact package identities and versions.

## 31. Historical Package Compatibility

Updating the application MUST NOT remove a Rule Set package version required to read or continue an existing Campaign without:

- compatible replacement;
- explicit migration;
- archival retention;
- or a clear blocked state.

## 32. Package Retention

Chronicle SHOULD preserve installed package versions referenced by active or archived Campaigns.

Garbage collection requires proof that no authoritative record depends on them.

## 33. Package Upgrade

A Rule Set package upgrade is separate from application upgrade.

It MAY require:

- Campaign migration;
- Character migration;
- Preference migration;
- new mechanical fixtures;
- user confirmation.

## 34. Release Channels

Initial channels MAY include:

```text
Stable
Beta
Nightly
Development
```

The MVP MAY publish only Stable and Development.

## 35. Stable Channel

Stable releases MUST satisfy all mandatory quality gates.

## 36. Beta Channel

Beta releases MAY expose incomplete capabilities.

They MUST:

- be clearly labeled;
- preserve data safety;
- use separate expectations for support;
- avoid silent promotion to Stable.

## 37. Nightly Channel

Nightly builds MAY be automated and unsigned during early development.

They MUST NOT be presented as safe for irreplaceable Campaign data without explicit warnings.

## 38. Channel Switching

Switching to a less stable channel SHOULD require confirmation.

Downgrading after a storage migration may be blocked.

## 39. Release Candidate

A release candidate is a build intended for final validation.

It MUST use the same production build process as the final release except for version labeling where applicable.

## 40. Release Process

Recommended release flow:

1. freeze release scope;
2. update versions;
3. validate changelog;
4. run deterministic test suites;
5. run migration chains;
6. run backup and restore;
7. run export round trip;
8. run security tests;
9. build platform artifacts;
10. generate manifest, checksums, and SBOM;
11. sign artifacts where supported;
12. run package-install smoke tests;
13. publish release candidate;
14. complete manual validation;
15. promote exact artifacts;
16. publish release notes.

## 41. Exact Artifact Promotion

Final release SHOULD promote already validated artifacts.

It SHOULD NOT rebuild from source after release-candidate approval unless the new build is fully revalidated.

## 42. Release Notes

Release notes SHOULD identify:

- new capabilities;
- fixed defects;
- migration behavior;
- compatibility changes;
- known issues;
- Rule Set package changes;
- backup recommendation;
- rollback limitations.

## 43. Breaking Change Notice

A breaking change MUST be visible before update.

The notice SHOULD state:

- affected contracts;
- affected Campaigns;
- migration requirement;
- rollback implications;
- export recommendation.

## 44. Update Architecture

An update MAY be:

```text
Manual
Application-Assisted
Automatic Download With Confirmation
Automatic Install
```

The MVP MAY begin with Manual updates.

## 45. Update Check

A future update checker SHOULD retrieve only:

- current release metadata;
- channel;
- version;
- manifest;
- signature information.

It MUST NOT transmit Campaign content.

## 46. Update Staging

Updates SHOULD be staged outside the active installation.

A staged update MUST be validated before activation.

## 47. Update Validation

Validation SHOULD include:

- artifact checksum;
- signature where supported;
- platform compatibility;
- architecture compatibility;
- current version compatibility;
- storage migration plan;
- disk space;
- package dependencies.

## 48. Pre-Update Checkpoint

Before an update requiring migration, Chronicle SHOULD create a validated checkpoint.

## 49. Update Activation

Activation SHOULD occur when:

- no state-changing operation is active;
- no finalization is in progress;
- no backup restore is running;
- staged artifacts are valid;
- required checkpoint exists;
- user confirmation is obtained where policy requires.

## 50. Update and Active Session

Chronicle SHOULD NOT update during an active Session automatically.

The user may be asked to finish or pause safely.

## 51. Migration Coordination

The application update installs binaries first or stages them, but authoritative storage migration occurs under Chronicle's migration architecture.

The updater itself MUST NOT rewrite Domain data independently.

## 52. First Launch After Update

First launch SHOULD:

1. validate installation;
2. acquire data lock;
3. load prior configuration;
4. inspect storage version;
5. validate checkpoint;
6. run required migrations;
7. run integrity checks;
8. validate Rule Set packages;
9. publish updated state;
10. open normal UI or Safe Mode.

## 53. Update Failure Before Migration

If artifact activation fails before migration:

- prior version remains available;
- Campaign data remains unchanged;
- staged files may be removed safely.

## 54. Migration Failure After Update

If new binaries launch but migration fails:

- Chronicle enters Safe Mode or blocked recovery;
- checkpoint remains available;
- prior binaries MAY be restored if compatible;
- no partial migrated state is published.

## 55. Rollback

Rollback has two distinct forms:

```text
Binary Rollback
Data Rollback
```

## 56. Binary Rollback

Binary rollback restores a prior application version.

It is safe only when the current data format remains compatible.

## 57. Data Rollback

Data rollback restores a checkpoint or backup.

It may discard changes made after that checkpoint.

It requires explicit user confirmation.

## 58. Downgrade Compatibility

A release manifest SHOULD declare whether downgrade is supported.

Chronicle MUST NOT claim rollback safety merely because old binaries can launch.

## 59. Failed Update Recovery

Recovery options MAY include:

- retry activation;
- restore prior binaries;
- restore checkpoint;
- enter Safe Mode;
- export Campaign data;
- repair installation.

## 60. Package Signing

Official release artifacts SHOULD be signed where platform and project resources permit.

Signing helps prove publisher identity and artifact integrity.

## 61. Checksum Publication

Official artifacts MUST publish cryptographic checksums.

The exact algorithm requires an ADR.

## 62. Signature Verification

A future updater MUST verify signatures before activation.

A failed signature blocks installation.

## 63. Unsigned Development Builds

Unsigned development builds MUST be clearly labeled.

They SHOULD not share the Stable update channel.

## 64. Supply-Chain Security

The release process SHOULD defend against:

- dependency substitution;
- compromised build environment;
- malicious release artifact;
- leaked signing key;
- unreviewed build script change;
- package-name confusion;
- forged update metadata.

## 65. Build Environment

Official releases SHOULD be built in a controlled environment.

The environment SHOULD:

- use pinned tools;
- minimize long-lived credentials;
- isolate signing;
- preserve build logs;
- produce attestable metadata where possible.

## 66. Signing Keys

Signing keys MUST:

- remain outside source control;
- use protected storage;
- have limited access;
- support rotation;
- have incident procedures.

## 67. Compromised Release

If a release is suspected to be compromised:

1. stop distribution;
2. revoke or withdraw update metadata;
3. publish advisory;
4. preserve evidence;
5. rotate affected keys;
6. issue corrected release;
7. document affected versions.

## 68. Open-Source Source Release

Every official source release SHOULD include:

- tagged source revision;
- build instructions;
- dependency lockfiles;
- license files;
- migration assets;
- public documentation;
- release manifest or equivalent metadata.

## 69. Source Archive Safety

Source archives MUST exclude:

- credentials;
- private fixtures;
- restricted sourcebooks;
- local configuration;
- generated Campaign data;
- signing material.

## 70. Community Builds

Community builds MAY exist.

They MUST NOT be presented as official unless produced and signed through the official release process.

## 71. Official Build Identity

The application SHOULD expose whether it is:

```text
OfficialRelease
OfficialPrerelease
DevelopmentBuild
CommunityBuild
UnknownBuild
```

## 72. Package Manager Distribution

Future package-manager distribution MAY be supported.

The package recipe MUST preserve:

- data separation;
- exact dependencies;
- application version;
- update policy;
- package integrity.

## 73. File Associations

Chronicle MAY later register portable Campaign packages with the operating system.

Opening an artifact still requires normal import validation.

## 74. Localization Packaging

Localization resources SHOULD be versioned and included in release artifacts.

Missing localization MUST fall back safely without changing machine keys.

## 75. Documentation Packaging

The desktop package SHOULD include or link to compatible documentation for that application version.

Documentation changes that alter contracts require version alignment.

## 76. License Packaging

Every distribution MUST include required license notices for:

- Chronicle;
- third-party dependencies;
- bundled Rule Set packages;
- included assets;
- fonts where applicable.

## 77. Build-Time Secrets

Build-time secrets MAY include:

- signing credentials;
- package-manager tokens;
- release-service tokens.

They MUST not be embedded in output artifacts.

## 78. Release Observability

The release pipeline SHOULD record:

- source revision;
- build environment;
- artifact identifiers;
- checksums;
- signing result;
- test results;
- migration results;
- package smoke tests;
- publication status.

## 79. Installed Build Diagnostics

The application SHOULD expose:

- version;
- channel;
- source revision;
- platform;
- architecture;
- storage schema;
- bundled package versions;
- official-build classification.

## 80. Privacy

Update checks and release diagnostics MUST not transmit:

- Campaign content;
- Character data;
- provider credentials;
- Rule Knowledge content;
- local paths.

## 81. Update Telemetry

Update success telemetry, if introduced later, follows RFC-0035 and RFC-0036.

It MUST not be mandatory for update correctness.

## 82. Quality Gates

A release artifact MUST pass RFC-0040 gates.

Additional delivery gates SHOULD include:

- clean build;
- manifest validation;
- checksum verification;
- signature verification where applicable;
- clean install;
- upgrade install;
- uninstall behavior;
- Safe Mode startup;
- migration checkpoint;
- rollback scenario;
- artifact-size sanity;
- license inventory.

## 83. Platform Smoke Tests

Each supported platform SHOULD test:

- install;
- first launch;
- data-directory creation;
- credential-store access;
- Campaign creation;
- restart;
- upgrade;
- uninstall without data loss;
- portable package where supported.

## 84. Update Test Matrix

The update test matrix SHOULD include:

```text
Fresh Install → Current
Previous Stable → Current
Oldest Supported → Current
Current Beta → Current Stable where supported
Failed Migration → Recovery
Low Disk Space → Safe Failure
Interrupted Activation → Recovery
```

## 85. Migration Fixture Gate

A release that changes storage or Domain schemas MUST include migration fixtures from every supported source version.

## 86. Backup Compatibility Gate

Before release, Chronicle MUST verify that:

- current backups restore;
- prior supported backups restore or migrate;
- restored Campaigns pass integrity checks.

## 87. Export Compatibility Gate

Portable Campaign packages from supported versions SHOULD import or provide an explicit migration path.

## 88. Release Artifact Retention

Official releases SHOULD retain:

- source tags;
- manifests;
- checksums;
- signatures;
- release notes;
- migration metadata.

Historical binary retention policy depends on hosting resources and security status.

## 89. Withdrawn Releases

A withdrawn release SHOULD remain documented but unavailable for normal installation.

The reason SHOULD be public when security permits.

## 90. End-of-Support Policy

Chronicle SHOULD eventually declare:

- supported application versions;
- supported migration source versions;
- supported Rule Set packages;
- supported platforms.

The MVP may initially support only the latest development line.

## 91. Error Model

Recommended release and update errors include:

```text
BuildInputInvalid
ArtifactGenerationFailed
ManifestInvalid
ChecksumMismatch
SignatureInvalid
PlatformUnsupported
ArchitectureUnsupported
UpdateNotCompatible
InsufficientDiskSpace
ActiveOperationBlocksUpdate
CheckpointFailed
MigrationFailed
ActivationFailed
RollbackUnavailable
PackageDependencyMissing
OfficialBuildVerificationFailed
```

## 92. Retry Semantics

Typical behavior:

```text
Transient artifact download failure
    → SafeWithSameUpdateOperation

Checksum mismatch
    → Redownload required

Signature invalid
    → NotRetryable with same artifact

Active Session
    → RequiresUserDecision or later retry

Checkpoint failure
    → Update blocked

Migration failure
    → RequiresRecovery

Lost confirmation after activation
    → Inspect installed build and operation record
```

## 93. Testing Strategy

### Build Tests

Verify clean build, locked dependencies, reproducibility properties, manifest generation, and secret exclusion.

### Packaging Tests

Verify file layout, runtime inclusion, licensing, configuration defaults, and platform metadata.

### Installer Tests

Verify install, upgrade, repair, uninstall, and user-data preservation.

### Update Tests

Verify staging, validation, activation, migration, failure, rollback, and restart.

### Supply-Chain Tests

Verify checksums, signatures, dependency inventory, and artifact provenance.

## 94. Required Test Cases

Tests MUST cover:

- clean Release build;
- dirty-source build rejection;
- dependency lock enforcement;
- manifest generation;
- artifact checksum;
- signature verification where enabled;
- credentials excluded from artifacts;
- private fixtures excluded;
- clean install;
- application and data separation;
- uninstall preserving Campaigns;
- portable build data isolation;
- previous version upgrade;
- oldest supported upgrade;
- active Session blocking update;
- pre-update checkpoint;
- low disk space;
- migration failure;
- binary rollback when compatible;
- data restore when binary rollback is incompatible;
- missing Rule Set dependency;
- historical package retention;
- unsigned development-build labeling;
- community-build labeling;
- corrupted update artifact;
- compromised-signature simulation;
- release manifest mismatch;
- source archive completeness.

## 95. Prohibited Patterns

### 95.1 Update Deletes User Data

Application replacement and Campaign storage remain separate.

### 95.2 Updater Performs Domain Migration

Domain migration belongs to Chronicle's migration services.

### 95.3 Latest Rule Set Replaces Historical Version Silently

Required package versions remain available or migrate explicitly.

### 95.4 Rebuild After Release Candidate Approval

Promote validated artifacts rather than rebuilding silently.

### 95.5 Credentials Embedded in Build

No build-time or user credential may ship in artifacts.

### 95.6 Unsigned Artifact Presented as Official Stable

Official identity must be verifiable.

### 95.7 Automatic Update During Active Session

Updates wait for a safe boundary.

### 95.8 Binary Rollback Claimed as Data Rollback

These are separate recovery actions.

### 95.9 Raw Database Dump as Portable Release Artifact

Backups and portable exports remain distinct.

### 95.10 Release Without Migration Evidence

Schema-changing releases require tested migration paths.

## 96. Current Delivery Decision

The MVP adopts:

- versioned Release builds;
- clean source requirement;
- dependency lockfiles;
- release manifest;
- cryptographic checksums;
- platform-specific desktop package;
- application and user-data separation;
- manual update delivery initially;
- explicit pre-update backup recommendation;
- tested migration path;
- no automatic background installation;
- no update during active Session;
- approved bundled Rule Set package;
- official-build classification;
- source release with build instructions;
- no cloud update service requirement;
- no marketplace distribution requirement.

## 97. Architecture Horizon

Future evolution MAY include:

- signed automatic updates;
- delta updates;
- package-manager publishing;
- installer repair;
- staged rollout;
- release telemetry;
- signed build provenance;
- reproducible-build verification;
- package revocation;
- plugin marketplace;
- enterprise channels;
- long-term support releases.

The MVP MUST NOT implement these capabilities without a later milestone.

## 98. Open Questions

The following remain open:

- Which desktop packaging formats will be used?
- Which operating systems and architectures are supported first?
- Which semantic-versioning guarantees apply before 1.0?
- Which checksum and signature mechanisms will be selected?
- Is code signing affordable for the initial open-source release?
- Will the MVP ship installer, portable archive, or both?
- Which CI environment builds official artifacts?
- How will release signing keys be protected?
- What is the minimum supported upgrade version?
- How long must historical Rule Set packages be retained?
- Should automatic update checking exist in MVP?
- How should community builds be identified visually?
- Which package-manager channels may be supported later?
- How will update integration tests run across operating systems?
- Which rollback scenarios are guaranteed?
- What exact artifacts belong in each release?

These questions require technology ADRs, framework selection, CI implementation, and release planning.

## 99. Compliance Checklist

An implementation complies when:

- application and user data remain separate;
- Release builds use declared, locked inputs;
- artifacts include version and manifest identity;
- checksums are published;
- secrets and private fixtures are excluded;
- platform packages follow native safety expectations;
- upgrades wait for safe application state;
- required checkpoints and migrations are validated;
- Rule Set versions needed by Campaigns are preserved;
- failed updates do not publish partial migrated state;
- binary and data rollback remain distinct;
- release artifacts pass RFC-0040 quality gates;
- official builds are distinguishable from community or development builds;
- source releases remain buildable and documented.

## 100. Final Principle

Chronicle's releases must evolve the application without gambling with the Campaign.

Users should be able to trust not only what the next version adds, but also what it refuses to lose.
