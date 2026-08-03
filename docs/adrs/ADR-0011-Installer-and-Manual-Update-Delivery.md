---
id: ADR-0011
title: Installer and Manual Update Delivery
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
  - ADR-0010
  - RFC-0034
  - RFC-0037
  - RFC-0038
  - RFC-0040
  - RFC-0041
  - RFC-0042
---

> **"An update may replace the application. It must never gamble with the Campaign."**

# Installer and Manual Update Delivery

## 1. Status

**Proposed**

This ADR defines Chronicle's first installer implementation and manual update workflow for Windows x64.

The decision is:

- use **Inno Setup** as the initial Windows installer technology;
- distribute a self-contained per-user installer;
- keep application binaries and user data strictly separate;
- use manual update delivery for the MVP;
- publish release manifests and SHA-256 checksums;
- support upgrade installation over a prior compatible version;
- execute storage migration only from Chronicle after installation;
- preserve Campaign data, configuration, and credentials during upgrade and uninstall;
- keep automatic update checking and automatic installation outside MVP;
- retain the ability to replace Inno Setup without changing Chronicle's Domain or Application layers.

The decision becomes **Accepted** after the installer spike and release-candidate test matrix pass.

## 2. Context

ADR-0006 selected:

- Windows x64 as the first supported operating system;
- a per-user installer as the primary package;
- a self-contained .NET runtime;
- a portable ZIP as a secondary artifact;
- manual updates for MVP;
- comparison between Inno Setup and WiX Toolset.

Chronicle needs a delivery path that is:

- easy for early users;
- repeatable in CI;
- compatible with code signing;
- capable of in-place upgrades;
- safe for local SQLite data;
- clear about uninstall behavior;
- independent from automatic update infrastructure.

The project does not need a background updater before the MVP proves the core Campaign loop.

## 3. Decision Drivers

The installer and delivery decision prioritizes:

1. low implementation complexity;
2. per-user installation;
3. no administrator requirement by default;
4. repeatable command-line builds;
5. reliable upgrade behavior;
6. code-signing support;
7. clear uninstall behavior;
8. Windows integration;
9. self-contained application packaging;
10. ease of inspection and maintenance;
11. compatibility with open-source release workflows;
12. no impact on Chronicle's architectural boundaries.

## 4. Decision Summary

Chronicle will use:

```text
Installer Technology
    Inno Setup

Primary Artifact
    Chronicle-{version}-windows-x64-setup.exe

Secondary Artifact
    Chronicle-{version}-windows-x64-portable.zip

Installation Scope
    Per user

Default Installation Directory
    User-local application program directory

Runtime
    Self-contained .NET runtime

Update Delivery
    Manual download and installation

Integrity
    SHA-256 checksum
    Code signature when available

Release Metadata
    Machine-readable release manifest

Storage Migration
    Executed by Chronicle on first launch
    Never by the installer

Uninstall
    Removes binaries
    Preserves user data and credentials by default
```

## 5. Why Inno Setup

Inno Setup is selected initially because it provides:

- mature Windows installer authoring;
- per-user installation;
- command-line compilation;
- upgrade and uninstall support;
- file and shortcut management;
- code-signing integration;
- low packaging overhead;
- straightforward scripting;
- broad use in desktop applications.

The installer script remains delivery infrastructure and does not own Chronicle state.

## 6. Why WiX Is Not Selected First

WiX remains a valid future option.

It was not selected first because:

- MSI authoring introduces more complexity;
- the MVP does not need enterprise deployment;
- Chronicle does not yet require machine-wide installation;
- installer development should not dominate the first release.

WiX may be reconsidered if Chronicle later needs:

- enterprise deployment;
- Group Policy distribution;
- advanced repair;
- machine-wide installation;
- MSI-specific tooling.

## 7. Installer Boundary

The installer owns:

- placing application binaries;
- creating shortcuts;
- registering uninstall metadata;
- replacing binaries during upgrade;
- recording application version;
- invoking optional post-install launch.

The installer does not own:

- SQLite schema migration;
- Campaign validation;
- backup creation;
- credential creation;
- Rule Set migration;
- import;
- restore;
- finalization recovery.

## 8. Installation Scope

The default installation is per-user.

It SHOULD not require administrative rights.

Conceptual install location:

```text
%LOCALAPPDATA%\Programs\Chronicle\
```

The final path depends on installer conventions.

## 9. Program Files Versus User Data

Program files contain immutable release content.

User data resides separately under Chronicle's per-user data directory.

Conceptually:

```text
Application
    %LOCALAPPDATA%\Programs\Chronicle\

User Data
    %LOCALAPPDATA%\Chronicle\
```

## 10. Installed Files

The installer SHOULD include:

- Chronicle executable;
- .NET runtime;
- Avalonia dependencies;
- SQLite native dependencies;
- official Rule Set package;
- migration assemblies and metadata;
- license files;
- third-party notices;
- release manifest;
- application icon;
- optional local help content.

## 11. Excluded Files

The installer MUST NOT include:

- provider credentials;
- user configuration;
- local databases;
- backups;
- Campaign exports;
- private fixtures;
- raw provider payloads;
- proprietary sourcebook text without authorization;
- signing keys;
- development secrets;
- development database files.

## 12. Installer Build Input

The installer is built from a previously produced Release publish directory.

Recommended pipeline:

```text
Restore
    ↓
Build
    ↓
Test
    ↓
Publish Windows x64 Self-Contained
    ↓
Validate Publish Directory
    ↓
Build Inno Setup Installer
    ↓
Generate Manifest and Checksum
    ↓
Sign
    ↓
Smoke Test
```

## 13. Exact Artifact Promotion

The installer tested as a release candidate SHOULD be the same installer published as final.

A final release SHOULD not rebuild the installer after validation unless all delivery gates are rerun.

## 14. Artifact Naming

Primary installer:

```text
Chronicle-{version}-windows-x64-setup.exe
```

Portable archive:

```text
Chronicle-{version}-windows-x64-portable.zip
```

Manifest:

```text
Chronicle-{version}-windows-x64-manifest.json
```

Checksum file:

```text
Chronicle-{version}-SHA256SUMS.txt
```

## 15. Installer Version

The installer version MUST map deterministically from Chronicle's application version.

Prerelease labels may require installer-compatible conversion.

The release manifest preserves the full semantic version.

## 16. Upgrade Identity

Inno Setup configuration MUST use stable application identity values.

These include:

- AppId;
- publisher identity;
- product name;
- uninstall identity.

Changing installer identity accidentally would create side-by-side installations instead of upgrades.

## 17. Stable AppId

The installer AppId MUST remain stable across compatible releases.

It is not the same as a Campaign or Domain identifier.

## 18. Upgrade Installation

Running a newer compatible installer SHOULD:

- detect the existing installation;
- preserve user-selected installation settings where practical;
- replace application binaries;
- update shortcuts;
- preserve user data;
- preserve credentials;
- preserve package versions required outside the install directory;
- not launch two versions concurrently.

## 19. Application Shutdown

Before replacing binaries, the installer SHOULD detect whether Chronicle is running.

The user should be asked to close Chronicle.

Forced termination is discouraged.

## 20. Active Session

The manual update documentation MUST instruct users not to update during an active Session.

The installer cannot reliably understand Campaign state.

Chronicle itself remains responsible for safe operation boundaries.

## 21. First Launch After Upgrade

After installation, Chronicle will:

1. acquire the data-directory lock;
2. inspect application and storage versions;
3. validate configuration;
4. create a migration checkpoint when required;
5. run storage migrations;
6. run Rule Set and Domain migrations where required;
7. validate integrity;
8. open normally or enter Safe Mode.

## 22. Installer Does Not Migrate Data

The installer MUST NOT:

- open `chronicle.db`;
- execute EF Core migrations;
- modify Character payloads;
- rewrite provider profiles;
- delete Work Items;
- update Campaign package bindings.

## 23. Migration Failure

If migration fails after installation:

- the new application remains installed;
- the prior data checkpoint remains available;
- Chronicle enters Safe Mode or blocked recovery;
- the user receives a safe recovery path;
- no partial migrated database is published.

## 24. Binary Rollback

The MVP does not promise automatic binary rollback.

A user MAY reinstall a prior version only when its release manifest declares compatibility with the current storage version.

Chronicle startup remains the final compatibility gate.

## 25. Manual Update Workflow

The supported MVP update process is:

1. finish or safely pause current play;
2. create or confirm a recent backup;
3. close Chronicle;
4. download the new official installer;
5. verify source, signature status, and checksum;
6. run the installer;
7. launch Chronicle;
8. allow validated migrations;
9. verify Campaign access.

## 26. No Automatic Update Check

The MVP does not automatically contact a release service.

There is no background update polling.

Users learn about updates through:

- project release page;
- documentation;
- community announcements;
- manually opened About or release information.

## 27. About Screen

The About screen SHOULD show:

- current version;
- build classification;
- source revision;
- supported platform;
- release channel;
- storage schema version;
- official build status;
- link or instructions for release information.

## 28. Manual Update Prompt

A manually provided newer-version artifact MAY be validated by Chronicle tooling later.

The MVP application itself does not install it automatically.

## 29. Download Responsibility

Chronicle MVP does not own artifact download.

The operating system, browser, or user-selected tool performs download.

## 30. Checksum

Every release artifact MUST have a SHA-256 checksum.

Checksums are published separately from the binary.

## 31. Checksum Verification

Release documentation SHOULD provide:

- PowerShell verification example;
- expected checksum format;
- warning that checksum must come from an official trusted release source.

## 32. Code Signing

Official Stable installers and executables SHOULD be Authenticode-signed when project resources permit.

Signing status is declared in the release manifest.

## 33. Unsigned Release

An unsigned build MUST be labeled clearly:

```text
Unsigned Development Build
```

or:

```text
Unsigned Preview Build
```

It must not imply verified publisher identity.

## 34. Signing Order

Recommended order:

1. build application binaries;
2. sign executables where required;
3. build installer;
4. sign installer;
5. generate final checksums;
6. validate signatures and checksums;
7. publish exact artifacts.

## 35. Signature Validation

Release validation SHOULD verify:

- signature exists when expected;
- signer identity matches policy;
- timestamping exists when configured;
- signed bytes match the published checksum.

## 36. Signing Failure

A Stable release requiring signing is blocked when signing fails.

Early project policy may explicitly permit unsigned prereleases.

## 37. Release Manifest

The manifest SHOULD contain:

```text
ApplicationVersion
FullSemanticVersion
SourceRevision
BuildNumber
ReleaseChannel
TargetOperatingSystem
TargetArchitecture
PackagingType
SelfContained
StorageSchemaVersion
MinimumSupportedStorageVersion
BundledRuleSetPackages
RequiredPackageVersions
ArtifactName
ArtifactSize
Sha256
SigningStatus
SignerIdentity when safe
BuildTimestampUtc
```

## 38. Manifest Format

Use versioned JSON.

Example key:

```text
ManifestContractVersion
```

The manifest is machine-readable and safe to publish.

## 39. Portable ZIP

The portable ZIP contains the same published application binaries without installer metadata.

It SHOULD use the normal per-user data directory by default.

## 40. Portable ZIP Upgrade

Portable users upgrade by:

- closing Chronicle;
- extracting the new package to a new or existing application directory;
- preserving the separate user-data directory;
- launching the new version.

Overwriting an active directory while Chronicle runs is unsupported.

## 41. Portable ZIP Limitations

The portable ZIP may lack:

- Start Menu integration;
- uninstall registration;
- automatic shortcut management;
- installer repair;
- verified upgrade detection.

## 42. Portable Data

True portable-data mode remains outside MVP.

The portable ZIP must not store credentials beside the executable.

## 43. Start Menu

The installer SHOULD create:

- Chronicle Start Menu entry;
- optional desktop shortcut if the user chooses;
- Safe Mode shortcut when implementation supports it.

## 44. Uninstall Registration

Chronicle appears in Windows installed applications with:

- product name;
- version;
- publisher;
- estimated size where practical;
- support or project reference;
- uninstall command.

## 45. Uninstall Behavior

Default uninstall removes:

- installed application binaries;
- shortcuts;
- installer metadata;
- application-specific file associations when later introduced.

It preserves:

- Campaign database;
- configuration;
- logs;
- diagnostics;
- backups;
- exports;
- credentials;
- managed historical Rule Set packages where stored in user data.

## 46. Uninstall Confirmation

The uninstaller SHOULD state clearly that user data is preserved.

## 47. Full Removal

Full data removal is a separate documented action.

It MUST require explicit confirmation.

The MVP uninstaller SHOULD not offer a casually selected checkbox that deletes all Campaign data.

## 48. Repair

Inno Setup reinstall of the same version MAY serve as basic binary repair.

Repair MUST NOT:

- reset configuration;
- remove credentials;
- replace user data;
- execute data migration.

## 49. Downgrade Detection

The installer SHOULD detect attempts to install an older application version.

It MAY warn or block.

Chronicle startup still validates storage compatibility.

## 50. Side-by-Side Channels

Stable and Development builds SHOULD use different:

- installer AppId;
- application directory;
- data directory;
- build classification.

Beta may share or separate identity depending on migration policy.

## 51. Development Installer

Development installers MUST NOT default to the Stable user-data directory.

## 52. Release Channels

Initial channels:

```text
Stable
Beta
Development
```

Nightly may be introduced later.

## 53. Channel Identification

The executable and installer SHOULD visibly identify non-Stable channels.

## 54. Channel Downgrade

Moving from Beta or Development to Stable may require:

- export and import;
- restored checkpoint;
- explicit compatibility validation.

It is not assumed safe.

## 55. Command-Line Installer Modes

The installer MAY support:

- interactive install;
- silent test install;
- uninstall;
- logging.

Silent installation is primarily for CI and testing in MVP.

## 56. Installer Logs

Installer logs MAY contain:

- selected install directory;
- version;
- copied-file status;
- installer errors.

They MUST NOT contain:

- credentials;
- Campaign content;
- database contents;
- provider configuration values beyond safe metadata.

## 57. Custom Installation Directory

The user MAY select another program directory when supported.

The installer must still keep user data separate.

## 58. Installation to Network Share

Installing binaries to a network location is not a supported default.

The installer MAY warn or block based on validation.

## 59. Installation to Removable Media

Installation to removable media is unsupported in MVP.

The portable ZIP is the better development artifact for that scenario, though Campaign data portability remains separate.

## 60. Required Disk Space

The installer SHOULD validate disk space for:

- application files;
- temporary extraction;
- basic update operation.

Chronicle separately validates space for migration checkpoints.

## 61. Low Disk Space

If installation lacks sufficient space:

- the installer fails safely;
- prior binaries remain usable where possible;
- user data remains unchanged.

## 62. Interrupted Install

An interrupted first install should not create a partially usable official installation.

An interrupted upgrade should preserve or recover the prior installation as far as installer technology permits.

This behavior requires explicit testing.

## 63. File Replacement

The installer SHOULD replace only managed application files.

User-created files must not be stored in the install directory.

## 64. Obsolete Files

The installer script SHOULD remove obsolete application files from prior versions when safe.

It MUST not delete unknown user files broadly.

## 65. Native Dependencies

The publish and installer process MUST include the correct Windows x64 native dependencies for:

- Avalonia;
- SQLite;
- any approved platform integration.

## 66. Runtime Configuration

Release runtime configuration belongs in the application package only when immutable and nonsecret.

User-editable configuration belongs in the data directory.

## 67. Release Notes

Each release SHOULD publish notes covering:

- features;
- defects fixed;
- migration behavior;
- known issues;
- backup recommendation;
- Rule Set package changes;
- provider compatibility;
- rollback limitations.

## 68. Breaking Change Notice

A release with significant migration impact MUST state:

- source version affected;
- expected migration duration;
- checkpoint behavior;
- rollback limitation;
- required package changes.

## 69. Release Page

The official release page SHOULD present:

- installer;
- portable ZIP;
- manifest;
- checksums;
- source archive;
- release notes;
- signature status;
- supported Windows versions.

## 70. Source Archive

Source release follows ADR-0002 and RFC-0041.

It is separate from the installer.

## 71. Build Reproducibility

The installer script, configuration, and build command are versioned.

A clean environment should reproduce functionally equivalent artifacts from the same declared inputs.

Exact byte-for-byte reproducibility may be limited by signing and timestamps.

## 72. Installer Script Location

Recommended location:

```text
build/packaging/windows/inno/
```

Example files:

```text
Chronicle.iss
includes/
assets/
README.md
```

## 73. Build Command

The repository SHOULD expose one documented command to build the installer.

Conceptually:

```text
build.ps1 package-windows
```

or:

```text
dotnet build build/Chronicle.Build.csproj --target PackageWindows
```

The exact orchestration tool is deferred to ADR-0012.

## 74. Environment Inputs

Installer build inputs SHOULD be explicit:

- version;
- source revision;
- publish directory;
- output directory;
- signing configuration;
- release channel.

## 75. Secrets in Build

Signing secrets are supplied through protected release infrastructure.

They MUST NOT be stored in installer scripts.

## 76. Quality Gates

Before publication, the installer MUST pass:

- malware or security scan where available;
- secret scan;
- file inventory validation;
- checksum generation;
- signature validation where enabled;
- clean-install smoke test;
- upgrade smoke test;
- uninstall-preserves-data test;
- Safe Mode startup;
- migration test;
- backup and restore validation.

## 77. Clean Install Test

A clean Windows VM test MUST verify:

1. no developer SDK installed;
2. installer runs;
3. no admin rights required;
4. Chronicle starts;
5. data directory is created;
6. desktop workflow loads;
7. Credential Manager integration is available;
8. uninstall preserves data.

## 78. Upgrade Test

Upgrade validation MUST cover:

1. install prior supported version;
2. create canonical Campaign;
3. store provider credential;
4. close Chronicle;
5. install new version;
6. launch;
7. migrate;
8. verify Campaign;
9. verify credential reference;
10. verify Dice and finalization history.

## 79. Failed Migration Test

Test flow:

1. install prior version;
2. create Campaign;
3. install new version;
4. inject migration failure;
5. launch;
6. enter Safe Mode;
7. verify checkpoint;
8. confirm no partial state was published.

## 80. Uninstall Test

Test flow:

1. install;
2. create Campaign;
3. configure provider credential;
4. uninstall;
5. verify binaries removed;
6. verify user data retained;
7. reinstall;
8. verify Campaign and credential reference resolve.

## 81. Portable ZIP Test

The portable package test MUST verify:

- clean extraction;
- launch;
- normal user-data path;
- no plaintext credential file;
- version display;
- same core behavior as installer build.

## 82. Security Tests

Installer security tests MUST cover:

- no test secrets;
- no private fixtures;
- no unrestricted writable executable directory behavior;
- no arbitrary command execution from manifest fields;
- safe quoting of paths;
- safe custom install path;
- no user-data deletion;
- signed artifact verification where enabled.

## 83. Accessibility

The installer SHOULD use standard accessible Windows installer controls.

Critical information must not rely on color alone.

## 84. Localization

The MVP installer MAY be English-only.

Installer localization can be added later.

The application language remains separate.

## 85. Telemetry

The installer MUST NOT send telemetry in MVP.

It does not report:

- installation;
- upgrade;
- uninstall;
- errors;
- machine identity.

## 86. Network Access

The installer SHOULD not require network access.

All runtime components are included.

## 87. Post-Install Launch

The installer MAY offer to launch Chronicle after success.

It should not launch automatically in silent CI mode.

## 88. Release Verification Tool

Chronicle MAY later provide a small tool to inspect:

- manifest;
- checksum;
- signature;
- platform compatibility.

It is not required for MVP.

## 89. Error Model

Delivery-related errors SHOULD include:

```text
InstallerBuildFailed
InstallerArtifactInvalid
InstallerSignatureMissing
InstallerSignatureInvalid
InstallerChecksumMismatch
InstallationBlockedByRunningProcess
InstallationDiskSpaceInsufficient
UpgradeVersionUnsupported
DowngradeBlocked
PostUpgradeMigrationRequired
PostUpgradeMigrationFailed
PortablePackageInvalid
```

## 90. User-Facing Recovery

When installation succeeds but Chronicle cannot open data, the application should guide the user toward:

- Safe Mode;
- checkpoint inspection;
- backup restore;
- diagnostics;
- compatible prior release only when safe.

## 91. Prohibited Patterns

### 91.1 Installer Opens the Database

Storage migration belongs to Chronicle.

### 91.2 User Data in Install Directory

Mutable Campaign data remains separate.

### 91.3 Uninstall Deletes Campaigns by Default

Prohibited.

### 91.4 Automatic Background Update in MVP

Deferred.

### 91.5 Download and Execute Without Verification

Manual releases publish checksums and signature status.

### 91.6 Rebuild After Release Candidate Validation

Promote exact validated artifacts.

### 91.7 Stable and Development Share Data Silently

They remain isolated.

### 91.8 Credentials in Installer Properties

Prohibited.

### 91.9 Portable ZIP Implies Portable Secrets

It does not.

### 91.10 Silent Downgrade

Downgrade requires compatibility validation.

## 92. Alternatives Considered

### WiX Toolset

Advantages:

- MSI ecosystem;
- mature enterprise installation;
- strong Windows Installer integration.

Not selected first because it adds complexity that the MVP does not need.

### MSIX

Advantages:

- modern packaging;
- clean deployment;
- built-in update possibilities.

Not selected first because its packaging and filesystem model may complicate Chronicle's local data, package retention, and flexible desktop behavior.

### Squirrel-Style Automatic Updater

Not selected because automatic update infrastructure is outside MVP and introduces extra trust, rollback, and release-service concerns.

### Portable ZIP Only

Rejected as the primary format because it gives weaker installation, upgrade, shortcut, repair, and uninstall experience.

### Custom Bootstrapper

Rejected because it would recreate mature installer behavior and increase security risk.

## 93. Consequences

### Positive

- practical Windows distribution;
- simple contributor-maintained packaging;
- clear manual update boundary;
- no update server required;
- safe user-data separation;
- predictable uninstall;
- straightforward CI artifact production;
- future signing support.

### Negative

- updates require user action;
- no automatic notification;
- users must verify downloads manually;
- Inno Setup is Windows-specific;
- enterprise MSI scenarios are deferred;
- rollback remains mostly manual;
- code signing may add cost.

## 94. Risks

### Installer Upgrade Removes Required File

Mitigation:

- file inventory test;
- exact artifact validation;
- clean and upgrade smoke tests.

### User Skips Backup

Mitigation:

- release notes;
- in-application backup reminder before known migration;
- automatic checkpoint during migration.

### Unsigned Warning

Mitigation:

- signing plan;
- checksums;
- official release source;
- clear build classification.

### Inno Setup Maintenance Risk

Mitigation:

- installer boundary;
- versioned scripts;
- ability to migrate to WiX or another packaging tool later.

### Manual Update Fragmentation

Mitigation:

- clear About screen;
- supported-version policy;
- explicit migration compatibility;
- release documentation.

## 95. Technology Spike

Before acceptance, implement:

1. Windows x64 self-contained publish;
2. Inno Setup installer;
3. stable AppId;
4. per-user install;
5. Start Menu shortcut;
6. optional desktop shortcut;
7. upgrade from test version;
8. uninstall preserving data;
9. Safe Mode shortcut or flag;
10. portable ZIP;
11. manifest generation;
12. SHA-256 checksums;
13. signature proof or unsigned classification;
14. clean VM smoke test;
15. failed migration recovery test.

## 96. Spike Acceptance

The spike passes when:

- the installer builds noninteractively;
- no administrator rights are required;
- Chronicle launches on a clean Windows VM;
- upgrade preserves Campaign data and credentials;
- uninstall preserves user data;
- failed migration enters Safe Mode;
- Stable and Development data remain isolated;
- checksums match published artifacts;
- signing status is correctly represented;
- the exact tested artifact can be promoted.

## 97. Definition of Compliance

An implementation complies when:

- Inno Setup builds the primary Windows installer;
- the application is self-contained;
- installation is per-user;
- user data remains outside the installation directory;
- updates are manual;
- Chronicle performs migrations after install;
- uninstall preserves data and credentials;
- checksums and manifests are published;
- signing status is explicit;
- portable ZIP remains secondary;
- Stable and Development environments are isolated;
- release-candidate artifacts are promoted without silent rebuild.

## 98. Review Triggers

This ADR must be reviewed if:

- enterprise deployment becomes a priority;
- automatic updates are introduced;
- Windows packaging requirements change;
- MSIX becomes clearly preferable;
- Inno Setup becomes unmaintained;
- code-signing policy changes;
- ARM64 support is added;
- machine-wide installation is required;
- package-manager distribution is introduced.

## 99. Deferred Decisions

Later ADRs MAY define:

- automatic update architecture;
- final code-signing provider;
- delta updates;
- Windows package-manager publication;
- MSI enterprise package;
- ARM64 artifact;
- file associations;
- URI scheme;
- full uninstall and secure data deletion;
- installer localization.

## 100. Final Decision

Chronicle will use Inno Setup for its first Windows x64 per-user installer.

The MVP will use manual update delivery, self-contained application binaries, release manifests, SHA-256 checksums, and explicit signing status.

The installer may replace Chronicle's executable files.

Only Chronicle may decide whether its history is safe to migrate.
