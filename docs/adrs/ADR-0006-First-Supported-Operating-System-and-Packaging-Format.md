---
id: ADR-0006
title: First Supported Operating System and Packaging Format
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
  - RFC-0034
  - RFC-0035
  - RFC-0037
  - RFC-0038
  - RFC-0040
  - RFC-0041
  - RFC-0042
---

> **"Support one operating system completely before claiming three incompletely."**

# First Supported Operating System and Packaging Format

## 1. Status

**Proposed**

This ADR selects **Windows x64** as Chronicle's first fully supported desktop operating system and selects a **signed per-user installer** as the primary MVP distribution format.

A **portable ZIP package** will be supported as a secondary development and recovery artifact when the security and credential-store limitations are clearly documented.

The decision becomes **Accepted** after a packaging spike proves:

- clean installation;
- first launch;
- user-data separation;
- Windows Credential Manager integration;
- one-instance locking;
- upgrade installation;
- uninstall without deleting Campaign data;
- backup and restore;
- Safe Mode launch;
- checksum verification;
- installer signing strategy;
- successful execution on a clean Windows machine.

## 2. Context

Chronicle's architecture targets:

- Windows;
- Linux;
- macOS.

The MVP does not need to claim complete support for all three platforms.

Chronicle is desktop-first and local-first. The first supported platform must provide a reliable path for:

- Avalonia desktop execution;
- .NET runtime distribution;
- SQLite storage;
- local filesystem access;
- credential storage;
- file dialogs;
- process locking;
- packaging;
- installation;
- upgrade;
- uninstallation;
- diagnostics;
- backup;
- restore.

Supporting every desktop operating system before the core product is proven would multiply:

- packaging work;
- CI complexity;
- code-signing requirements;
- filesystem variations;
- credential-store implementations;
- accessibility validation;
- installer testing;
- support burden.

This ADR deliberately chooses one platform for full MVP support while preserving platform abstractions for later expansion.

## 3. Decision Drivers

The first platform decision prioritizes:

1. likely availability among early users and contributors;
2. mature .NET desktop support;
3. mature Avalonia support;
4. straightforward local application deployment;
5. reliable native credential storage;
6. practical installer tooling;
7. accessible virtual-machine and CI testing;
8. low operational burden;
9. clear user-data separation;
10. reasonable code-signing path.

## 4. Decision Summary

Chronicle's first supported distribution target will be:

```text
Operating System
    Windows 11
    Windows 10 supported when technically compatible and still within project support policy

Architecture
    x64

Primary Distribution
    Per-user installer
    No administrator requirement by default

Secondary Distribution
    Portable ZIP
    Development and recovery use
    Limited credential behavior documented

Runtime Model
    Self-contained .NET application by default

User Data
    Stored outside installation directory
    Preserved on upgrade and uninstall by default

Credentials
    Windows Credential Manager through ISecretsManager

Configuration
    Per-user local application data

Logs and Diagnostics
    Per-user local application data

Campaign Database
    Per-user local application data

Backups and Exports
    User-selectable locations
```

## 5. Why Windows First

Windows is selected as the first fully supported platform because it offers:

- broad desktop availability;
- strong .NET support;
- mature Avalonia support;
- practical virtual-machine testing;
- native credential management;
- established installer conventions;
- straightforward per-user application-data locations;
- broad accessibility tooling and user familiarity.

This decision is about delivery sequence, not architectural preference.

Chronicle remains designed for Linux and macOS.

## 6. Supported Windows Scope

The MVP target SHOULD be:

```text
Windows 11 x64
```

Windows 10 MAY be supported when:

- the selected .NET runtime supports it;
- Avalonia behavior is acceptable;
- installer behavior is validated;
- credential storage works;
- accessibility smoke tests pass.

The release manifest MUST declare the exact supported versions.

## 7. Architecture Scope

The first MVP release supports:

```text
x64
```

ARM64 is deferred.

x86 is not supported.

### Rationale

Supporting one architecture reduces:

- artifact count;
- packaging matrix;
- native dependency complexity;
- CI requirements;
- testing burden.

## 8. Primary Packaging Format

### Decision

Use a **per-user Windows installer** as the primary distribution.

The exact installer technology is deferred to implementation validation, but the package MUST support:

- install without administrator privileges by default;
- versioned upgrade;
- repair or reinstall;
- uninstall;
- shortcuts;
- application registration;
- clear installation path;
- no Campaign deletion during uninstall.

## 9. Per-User Installation

The default installer SHOULD install into a per-user application location.

Conceptually:

```text
%LOCALAPPDATA%\Programs\Chronicle\
```

The exact path follows the selected installer technology and Windows conventions.

### Rationale

Per-user installation:

- avoids unnecessary administrator prompts;
- reduces privilege requirements;
- simplifies individual developer and player use;
- aligns with local-first single-user scope.

## 10. Machine-Wide Installation

Machine-wide installation is outside MVP.

It MAY be supported later for:

- shared computers;
- enterprise deployment;
- managed environments.

It requires separate administrator and multi-user data considerations.

## 11. Self-Contained Runtime

### Decision

The primary installer SHOULD contain a self-contained .NET runtime.

### Rationale

This avoids requiring the player to:

- install the correct .NET runtime manually;
- understand runtime channels;
- repair a missing runtime independently.

### Consequence

Installer size increases.

This is acceptable for MVP reliability.

## 12. Framework-Dependent Build

A framework-dependent build MAY be produced for:

- development;
- contributor testing;
- package-manager experiments.

It is not the default user distribution.

## 13. Single-File Publishing

Single-file publishing MAY be evaluated.

It is not required.

The decision depends on:

- startup behavior;
- native library extraction;
- diagnostics;
- Avalonia resources;
- SQLite deployment;
- code signing;
- update behavior.

A directory-based self-contained application is acceptable.

## 14. Secondary Portable ZIP

### Decision

Produce a portable ZIP as a secondary artifact when practical.

### Intended Uses

- development;
- testing;
- recovery;
- users unable to use an installer;
- preview builds.

### Limitations

Portable mode MUST clearly document:

- where application data is stored;
- how credentials are stored;
- whether the Windows Credential Manager remains used;
- how upgrades are performed;
- how instance locking works;
- that moving the ZIP does not automatically move Campaign data.

## 15. Portable Data Mode

The portable executable package does not automatically imply portable Campaign data.

Two concepts MUST remain separate:

```text
Portable Application Binaries
Portable Application Data
```

The MVP portable ZIP SHOULD still use the normal per-user data directory unless an explicit portable-data mode is selected later.

## 16. Portable-Data Mode

True portable-data mode is deferred.

It introduces risks around:

- plaintext credentials;
- removable-drive reliability;
- filesystem permissions;
- concurrent access;
- backup;
- data loss when the directory is deleted.

## 17. Installation Directory

The installation directory contains:

- Chronicle executable;
- .NET runtime;
- Avalonia resources;
- official Rule Set package;
- migration assets;
- licenses;
- static documentation or documentation links;
- release manifest.

It MUST NOT contain mutable Campaign data.

## 18. User Data Directory

Chronicle SHOULD use a per-user local application-data directory.

Conceptually:

```text
%LOCALAPPDATA%\Chronicle\
```

Recommended structure:

```text
Chronicle/
├── data/
│   └── chronicle.db
├── config/
├── logs/
├── diagnostics/
├── indexes/
├── temp/
├── checkpoints/
└── packages/
```

Backups and exports MAY use user-selected directories outside this root.

## 19. User Data Separation

Installer operations MUST NOT overwrite or delete:

- Campaign database;
- backups;
- exports;
- credential entries;
- user Rule Knowledge sources;
- diagnostics selected for retention.

## 20. Roaming Versus Local Data

Chronicle SHOULD use local application data rather than roaming application data for:

- database;
- indexes;
- logs;
- package cache;
- diagnostics.

### Rationale

The database is not safe to roam through generic Windows profile synchronization without explicit synchronization architecture.

## 21. Documents Folder

Chronicle MUST NOT store its authoritative database in the user's Documents folder by default.

Documents may be used for:

- exports;
- user-selected backups;
- visible portable artifacts.

## 22. Credential Storage

### Decision

Use **Windows Credential Manager** through Chronicle's `ISecretsManager` port.

Credentials MUST NOT be stored in:

- configuration files;
- registry values;
- SQLite Campaign tables;
- installer properties;
- command-line arguments;
- logs;
- backups;
- exports.

## 23. Credential Identity

Credential aliases SHOULD be namespaced by application and provider.

Example:

```text
Chronicle/providers/openai/default
```

## 24. Credential Cleanup

Uninstall MUST NOT delete provider credentials by default.

The uninstaller MAY offer an explicit separate option later.

Chronicle Settings must allow credential deletion from inside the application.

## 25. Configuration Storage

Versioned application configuration SHOULD be stored in the per-user data directory.

Configuration files contain:

- nonsecret settings;
- credential aliases;
- provider profiles;
- paths;
- feature flags;
- UI preferences.

Writes MUST be atomic.

## 26. Windows Registry

The MVP SHOULD avoid using the Windows Registry for ordinary Chronicle configuration.

Registry use MAY be limited to:

- installer metadata;
- uninstall registration;
- file associations later;
- URI scheme registration later.

## 27. Campaign Database

The SQLite database remains in the user data directory.

The installer MUST NOT open, modify, or migrate the database.

Migrations run through Chronicle on first launch after update.

## 28. Package Storage

Official Rule Set package binaries may be installed with the application.

Historical package versions required by Campaigns MAY be retained in a per-user managed package directory.

The updater and uninstaller must respect package retention policy.

## 29. Rule Knowledge Sources

User-supplied source files remain at user-selected locations unless explicitly imported.

Indexes and normalized local metadata live in Chronicle-managed local data.

Absolute paths MUST not appear in normal logs or portable Campaign exports.

## 30. Backup Location

The MVP SHOULD provide a default backup suggestion in a user-visible folder.

The exact default requires UX validation.

Chronicle must allow the player to select another location.

## 31. Export Location

Exports SHOULD default to a user-visible location such as Documents or the last selected folder.

They MUST not default to hidden internal application directories.

## 32. Temporary Files

Temporary files belong in:

- Chronicle-managed temp directory;
- or Windows temporary storage through an abstraction.

They MUST be cleaned after successful workflows and during safe startup maintenance.

## 33. Process Lock

Chronicle will enforce one active instance per data directory.

The Windows implementation MAY use:

- named mutex;
- lock file plus process validation;
- local IPC endpoint;
- combination of these.

The exact mechanism requires a later implementation decision.

## 34. Secondary Launch

A second launch SHOULD:

1. detect the active instance;
2. forward safe activation information;
3. focus the existing window;
4. exit.

If forwarding fails, it MUST not open the database concurrently as a second writer.

## 35. Installer Upgrade Model

The installer MUST support versioned in-place upgrade.

An upgrade should:

- replace application binaries;
- preserve user data;
- preserve configuration;
- preserve credentials;
- retain required package versions;
- launch Chronicle migration only after installation completes.

## 36. Update Boundary

The installer updates binaries.

Chronicle updates storage.

This boundary is mandatory.

## 37. First Launch After Upgrade

Chronicle SHOULD:

1. detect application version change;
2. acquire the data-directory lock;
3. inspect storage version;
4. create a checkpoint when required;
5. run migrations;
6. validate integrity;
7. open normally or enter Safe Mode.

## 38. Downgrade

Downgrade is not guaranteed.

The installer SHOULD detect older-version installation attempts where possible.

Chronicle's release manifest and startup checks remain authoritative for data compatibility.

## 39. Repair Installation

A repair operation MAY restore missing or corrupted application binaries.

It MUST NOT reset Campaign data or configuration.

## 40. Uninstall

Default uninstall removes:

- application binaries;
- shortcuts;
- installer metadata.

It preserves:

- Campaign data;
- configuration;
- logs;
- diagnostics;
- backups;
- exports;
- credentials.

## 41. Full Data Removal

A full local-data removal workflow is separate from uninstall.

It requires explicit confirmation and should ideally be initiated inside Chronicle or through a documented manual process.

## 42. Installer Privileges

The default installer SHOULD require no administrator privileges.

Any elevation request must have a clear purpose.

## 43. Code Signing

### Decision

Official Stable Windows installers and executables SHOULD be code-signed when project resources permit.

### MVP Constraint

If signing is not available for the earliest development release:

- the artifact MUST be labeled as unsigned;
- checksums MUST be published;
- the release MUST not imply verified publisher identity;
- Stable release readiness must explicitly review the risk.

## 44. Signing Scope

Where supported, sign:

- installer;
- main executable;
- relevant helper executables;
- updater later;
- optional local-model manager later.

## 45. Signing Key Security

Signing credentials MUST:

- remain outside the repository;
- use protected CI or signing infrastructure;
- have limited access;
- support rotation;
- be documented in incident response.

## 46. Checksums

Every distributed artifact MUST have a published cryptographic checksum.

The exact algorithm is deferred, but SHA-256 is the initial candidate.

## 47. Release Manifest

Every Windows release MUST include a manifest containing:

- application version;
- source revision;
- Windows version support;
- architecture;
- package type;
- runtime mode;
- storage schema version;
- bundled Rule Set package versions;
- checksums;
- signing status;
- release channel.

## 48. Installer Technology Selection Criteria

The exact installer tool SHOULD be selected based on:

- open-source or acceptable license;
- per-user installation;
- upgrade support;
- signing support;
- command-line automation;
- CI compatibility;
- uninstall behavior;
- Windows integration;
- project maintenance.

## 49. Candidate Installer Technologies

Candidates MAY include:

- WiX Toolset;
- MSIX;
- Inno Setup;
- Squirrel-style frameworks;
- other mature Windows packaging tools.

The final selection requires a focused spike.

## 50. MSIX Considerations

MSIX offers modern Windows deployment features but may introduce:

- packaging restrictions;
- application-container expectations;
- file-location behavior;
- signing requirements;
- update assumptions.

It should be selected only after verifying Chronicle's local database, package retention, and file access model.

## 51. WiX Considerations

WiX offers mature MSI-based installation and upgrade behavior.

It may require more packaging configuration and Windows Installer expertise.

## 52. Inno Setup Considerations

Inno Setup offers straightforward installer authoring and per-user installation.

Its upgrade, signing, and CI behavior must be validated against Chronicle's release requirements.

## 53. Initial Packaging Recommendation

The first spike SHOULD compare:

```text
Inno Setup
WiX Toolset
```

MSIX MAY be evaluated if its sandbox and signing constraints align with Chronicle.

## 54. File Associations

File associations are deferred.

When introduced, they MAY support Chronicle portable Campaign packages.

Opening an associated file must still execute safe import inspection.

## 55. URI Scheme

A custom `chronicle://` URI scheme is outside MVP.

It requires strict parsing and single-instance forwarding.

## 56. Start Menu and Shortcut

The installer SHOULD create:

- Start Menu entry;
- optional desktop shortcut based on user choice.

## 57. Application Identity

The application SHOULD use stable metadata:

```text
ProductName: Chronicle
Publisher: Chronicle Project or selected legal publisher name
ApplicationId: stable Windows identity
```

The final publisher identity requires project governance and signing decisions.

## 58. Version Display

The application must expose:

- application version;
- build classification;
- source revision where appropriate;
- architecture;
- packaging type;
- signing or official-build status.

## 59. Safe Mode Launch

The Windows package SHOULD support a documented Safe Mode launch path.

Possible mechanisms:

- command-line flag;
- Start Menu shortcut;
- recovery button after failed startup.

## 60. Command-Line Arguments

Supported arguments MAY include:

```text
--safe-mode
--data-directory <path>
--diagnostics
--open <artifact>
```

Secrets MUST NOT be accepted through command-line arguments.

## 61. Custom Data Directory

A custom data-directory option MAY support development and recovery.

Production UI support is deferred.

The path must pass normal safety validation.

## 62. Windows Defender and SmartScreen

Unsigned or low-reputation builds may trigger warnings.

Chronicle documentation MUST be honest about:

- signing status;
- checksum verification;
- official release source;
- publisher identity.

## 63. Antivirus Interaction

Chronicle SHOULD avoid behavior commonly associated with malicious software:

- self-modifying binaries;
- hidden background services;
- arbitrary script execution;
- unrestricted child processes;
- unexpected network listeners.

## 64. Firewall

The remote-provider MVP uses outbound HTTPS only.

Chronicle does not require an inbound firewall rule.

## 65. Network Listeners

The desktop MVP MUST NOT open a public network listener.

Local provider integrations may use loopback endpoints configured explicitly.

## 66. Accessibility on Windows

The first supported release MUST validate:

- keyboard navigation;
- screen-reader labels;
- focus behavior;
- high-DPI scaling;
- text scaling;
- reduced motion;
- contrast;
- Windows theme behavior.

At least one common Windows screen-reader workflow SHOULD be tested manually.

## 67. High-DPI Support

Chronicle MUST behave correctly on common scaling configurations.

UI layout must not assume 100% scaling.

## 68. Window Placement

Window state SHOULD recover safely when:

- monitor configuration changes;
- saved monitor is unavailable;
- DPI changes;
- prior coordinates are off-screen.

## 69. File Paths

Windows path behavior must account for:

- long paths;
- invalid characters;
- reserved names;
- network paths;
- removable drives;
- symlinks or junctions;
- case-insensitive comparison.

## 70. Network Paths

The authoritative SQLite database SHOULD NOT be placed on a network share by default.

Custom network data directories require explicit warning or may be blocked in MVP.

## 71. Removable Drives

Running the authoritative database from removable media is not a supported default.

Backups and exports may be stored there.

## 72. Filesystem Permissions

Chronicle SHOULD validate read and write access before:

- database creation;
- backup;
- restore;
- export;
- index build;
- diagnostics bundle.

## 73. Long Path Support

Packaging and runtime should avoid unnecessarily deep internal paths.

Chronicle SHOULD test long Campaign titles without mapping them directly to unsafe filenames.

## 74. Crash Dumps

Windows crash dumps are not enabled automatically by Chronicle.

Developer or operating-system crash diagnostics may be used under explicit privacy policy.

## 75. Logging Paths

Logs belong in per-user local application data.

The diagnostics UI SHOULD provide a safe way to open the log directory.

## 76. Installer Logging

Installer logs MAY be generated for troubleshooting.

They MUST NOT contain credentials or Campaign content.

## 77. Backup Before Upgrade

Manual MVP upgrade instructions SHOULD recommend a validated backup.

When Chronicle detects a migration on first launch, it SHOULD create a checkpoint automatically according to RFC-0034 and ADR-0004.

## 78. Update Delivery

Automatic updates are not required.

The MVP update flow is:

```text
Download official artifact
Verify source and checksum
Close Chronicle
Run installer
Launch Chronicle
Run validated migrations
```

## 79. Release Channels

The Windows package SHOULD visibly distinguish:

- Stable;
- Beta;
- Development.

Different channels SHOULD not silently overwrite each other's data without compatibility checks.

## 80. Side-by-Side Installation

Stable and Development builds MAY need separate application identities and data directories.

This prevents development builds from migrating or corrupting Stable Campaigns.

## 81. Development Build Isolation

Development builds SHOULD default to a development data directory.

They MUST not open production Campaign data accidentally.

## 82. CI Packaging

CI SHOULD produce unsigned test artifacts for every relevant build.

Official signed artifacts are produced only from controlled release workflow.

## 83. Windows Packaging Test Matrix

At minimum:

```text
Clean Install
First Launch
Upgrade From Previous Supported Version
Repair
Uninstall
Reinstall
Safe Mode
Portable ZIP Launch
No Credential Present
Credential Present
Database Migration
Backup Restore
Low Disk Space
Interrupted Installer
```

## 84. Clean Machine Test

Release candidates MUST run on a clean Windows machine or virtual machine without developer tools installed.

## 85. Required Test Cases

Tests MUST cover:

- x64 package identity;
- unsupported architecture rejection;
- per-user install;
- no administrator requirement;
- self-contained runtime;
- application-data creation;
- user-data preservation on upgrade;
- user-data preservation on uninstall;
- credential persistence across upgrade;
- no credential in installer;
- one-instance behavior;
- second-launch forwarding;
- Safe Mode launch;
- clean migration after upgrade;
- failed migration recovery;
- missing disk space;
- installer interruption;
- repair install;
- portable ZIP behavior;
- development-data isolation;
- high-DPI layout;
- off-screen window recovery;
- Windows path validation;
- backup to user-selected path;
- restore from backup;
- checksum verification;
- signing verification where enabled.

## 86. Security Tests

Security testing MUST include:

- installer cannot write arbitrary attacker-controlled paths;
- command-line arguments reject credentials;
- custom data path prevents traversal;
- untrusted portable Campaign files use import inspection;
- no inbound listener;
- provider key remains in Credential Manager;
- logs contain no secrets;
- uninstall does not expose or copy credentials;
- development builds cannot silently open Stable data.

## 87. Performance Tests

The package SHOULD be evaluated for:

- installer size;
- installed size;
- first-launch time;
- normal startup time;
- upgrade time;
- migration time;
- memory baseline.

Exact budgets require later measurement.

## 88. Documentation Requirements

Windows release documentation MUST explain:

- supported Windows versions;
- x64 requirement;
- installer and portable options;
- location of user data;
- location of backups;
- credential storage;
- uninstall behavior;
- Safe Mode;
- manual update procedure;
- checksum verification;
- signing status;
- known antivirus or SmartScreen warnings.

## 89. Support Boundary

The project will classify Windows environments as:

```text
Supported
BestEffort
Unsupported
```

Exact classification belongs in the release manifest and support policy.

## 90. Linux and macOS

Linux and macOS remain architecture targets.

They are not fully supported in the first MVP release.

Contributors MAY run development builds on those platforms when the stack permits.

The project MUST not claim release support without packaging, credential, accessibility, backup, and upgrade validation.

## 91. Linux Future Requirements

Linux support will require decisions for:

- package formats;
- desktop integration;
- Secret Service or equivalent credential store;
- distribution compatibility;
- Wayland and X11 behavior;
- file locations;
- signing or repository distribution;
- accessibility.

## 92. macOS Future Requirements

macOS support will require decisions for:

- application bundle;
- code signing;
- notarization;
- Keychain integration;
- sandboxing;
- Gatekeeper;
- packaging;
- application-data locations;
- accessibility.

## 93. Alternatives Considered

### Linux First

Strengths:

- open-source alignment;
- package flexibility;
- strong developer community.

Not selected first because:

- distribution fragmentation;
- credential-store variation;
- packaging matrix;
- desktop-environment differences;
- accessibility variance.

### macOS First

Strengths:

- consistent hardware and OS environment;
- strong desktop user experience.

Not selected first because:

- signing and notarization requirements;
- higher release-cost barrier;
- narrower early contributor environment.

### All Three Platforms at MVP

Rejected because it would multiply delivery risk before the core vertical slice is validated.

### Portable ZIP Only

Rejected as the primary distribution because:

- users expect installation integration;
- upgrade behavior is less clear;
- shortcuts and uninstall are manual;
- support becomes harder;
- credential and data-location expectations are less obvious.

### MSIX Only

Not selected without a spike because packaging constraints may conflict with Chronicle's local data and package-retention model.

## 94. Consequences

### Positive

- focused test matrix;
- practical early user distribution;
- native credential storage;
- clear installer expectations;
- simpler support;
- faster MVP packaging;
- Windows accessibility can be validated deeply.

### Negative

- Linux and macOS users lack official MVP support;
- code signing may cost money;
- installer maintenance becomes Windows-specific;
- portable build limitations must be documented;
- cross-platform issues may be discovered later.

## 95. Risks

### Windows-Specific Leakage

Mitigation:

- platform abstractions;
- Windows code stays in Desktop platform services;
- no Windows types in Domain or Application;
- contributor builds on other platforms where practical.

### Installer Data Loss

Mitigation:

- strict application/user-data separation;
- upgrade tests;
- uninstall tests;
- installer never runs Domain migrations.

### Credential Portability

Mitigation:

- credential aliases;
- no credentials in exports;
- explicit reconfiguration on another machine.

### Unsigned Build Warnings

Mitigation:

- code-signing plan;
- checksums;
- official release documentation;
- honest build classification.

### Windows Version Drift

Mitigation:

- release manifest;
- supported-version testing;
- framework support review;
- explicit end-of-support policy.

## 96. Technology Spike

Before acceptance, implement:

1. self-contained Windows x64 build;
2. per-user installer using two candidate technologies;
3. clean install;
4. Start Menu shortcut;
5. user-data directory initialization;
6. Windows Credential Manager adapter;
7. one-instance lock;
8. upgrade over prior test version;
9. migration on first launch;
10. uninstall preserving data;
11. portable ZIP;
12. Safe Mode command;
13. checksum generation;
14. signing proof or documented unsigned path;
15. clean-machine smoke test.

## 97. Spike Acceptance

The spike passes when:

- no administrator rights are required for default install;
- the app launches on a clean Windows machine;
- provider credentials survive upgrade and remain outside files;
- uninstall preserves Campaign data;
- migration is executed by Chronicle, not installer;
- a failed migration enters Safe Mode;
- a second instance cannot mutate the database;
- the portable ZIP cannot silently compromise credential policy;
- checksums verify;
- the chosen installer technology supports repeatable CI builds.

## 98. Definition of Compliance

An implementation complies when:

- Windows x64 is the first fully supported platform;
- the primary artifact is a per-user installer;
- the runtime is self-contained by default;
- user data is outside installation files;
- credentials use Windows Credential Manager;
- installer upgrade preserves Campaigns;
- uninstall preserves user data by default;
- Chronicle owns migrations;
- checksums are published;
- signing status is explicit;
- portable ZIP behavior is documented;
- Linux and macOS are not falsely advertised as supported.

## 99. Review Triggers

This ADR must be reviewed if:

- .NET or Avalonia drops required Windows support;
- Windows 10 support changes materially;
- installer technology becomes unmaintained;
- code-signing requirements change;
- MSIX becomes clearly advantageous;
- early users strongly require Linux or macOS;
- local model packaging changes distribution needs;
- package size becomes impractical;
- enterprise installation becomes a priority.

## 100. Deferred Decisions

Later ADRs MAY define:

- final installer technology;
- code-signing provider;
- exact supported Windows versions;
- ARM64 support;
- file associations;
- URI scheme;
- automatic update mechanism;
- portable-data mode;
- Linux packaging;
- macOS packaging and notarization.

## 101. Final Decision

Chronicle's first official MVP release will target Windows x64.

The primary package will be a self-contained, per-user installer that preserves Campaign data and uses Windows Credential Manager for provider credentials.

A portable ZIP may be distributed as a secondary artifact with explicit limitations.

Chronicle will support one operating system completely before claiming support for the rest.

That focus protects the Campaign, the release, and the project's ability to finish what it starts.
