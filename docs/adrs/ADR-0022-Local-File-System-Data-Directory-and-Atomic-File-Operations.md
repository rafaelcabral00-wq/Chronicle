---
id: ADR-0022
title: Local File System, Data Directory, and Atomic File Operations
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
  - ADR-0018
  - ADR-0019
  - ADR-0020
  - ADR-0021
  - RFC-0033
  - RFC-0034
  - RFC-0035
  - RFC-0036
  - RFC-0037
  - RFC-0038
  - RFC-0040
  - RFC-0041
  - RFC-0042
---

> **"Chronicle may write many temporary files. It must publish only complete, validated artifacts as truth."**

# Local File System, Data Directory, and Atomic File Operations

## 1. Status

**Proposed**

This ADR defines Chronicle's local filesystem abstraction, data-directory layout, path-safety rules, temporary-file policy, file locking, and atomic publication conventions.

The decision is:

- define a Chronicle-owned filesystem port for Application and Infrastructure workflows;
- keep direct `System.IO` usage inside approved filesystem adapters and low-level host code;
- use one resolved per-user data root for authoritative local state;
- keep application binaries separate from mutable user data;
- organize data into bounded, purpose-specific directories;
- normalize and validate every path before use;
- never trust user-supplied archive paths or package entry names;
- publish files through write-to-staging, flush, validate, and atomic replace or rename;
- keep incomplete artifacts distinguishable from published artifacts;
- use same-volume staging when atomic rename semantics are required;
- preserve prior valid artifacts until replacement succeeds;
- use file and directory permissions appropriate to per-user local data;
- never store credentials in ordinary files;
- use explicit retention and cleanup for temporary files;
- make deletion best-effort and truthful, without claiming guaranteed secure erasure;
- use injectable filesystem and path services for deterministic tests;
- treat network shares and removable media as unsupported for authoritative MVP storage.

The decision becomes **Accepted** after a filesystem spike proves:

- first-run directory creation;
- safe path resolution;
- path traversal rejection;
- symlink or reparse-point policy;
- atomic settings replacement;
- atomic backup publication;
- interrupted export cleanup;
- cross-volume replacement rejection or fallback;
- locked-file failure;
- low-disk failure;
- stale temporary-file recovery;
- uninstall and reinstall data preservation;
- test-host isolation.

## 2. Context

Chronicle writes and reads local files for:

```text
SQLite database
SQLite sidecar files
configuration
logs
backups
exports
diagnostic bundles
Rule Set packages
Rule Knowledge sources and indexes
migration checkpoints
temporary staging
installer and portable-package metadata
window and application state
```

Filesystem operations can fail because of:

- invalid paths;
- permissions;
- locked files;
- process interruption;
- disk exhaustion;
- antivirus or indexing interference;
- cross-volume move limitations;
- malicious archive entries;
- stale temporary files;
- symlink or reparse-point redirection;
- unavailable external drives;
- partial writes.

A successful stream write does not necessarily mean a complete artifact is safe to publish.

Chronicle must distinguish:

```text
authoritative active file
validated immutable artifact
staging file
temporary file
orphaned incomplete file
user-selected external destination
```

ADR-0004 defines SQLite persistence.

ADR-0011 defines installation and data separation.

ADR-0021 defines data-directory ownership and startup recovery.

This ADR defines the concrete filesystem boundary.

## 3. Decision Drivers

The decision prioritizes:

1. data integrity;
2. crash-safe publication;
3. path security;
4. per-user local storage;
5. testability;
6. backup and export reliability;
7. clear ownership;
8. bounded cleanup;
9. no credential leakage;
10. compatibility with Windows MVP;
11. future platform adapters;
12. low operational complexity.

## 4. Decision Summary

Chronicle will use:

```text
Filesystem Port
    IChronicleFileSystem

Path Policy
    IDataPathResolver
    IPathSafetyPolicy

Authoritative Data Root
    per-user local application data

Installation Directory
    binaries only

Publication Pattern
    create staging file
    write
    flush
    validate
    atomic rename or replace
    preserve prior valid artifact until success

Temporary Files
    managed temp directory
    explicit naming
    bounded retention
    startup cleanup

External Destinations
    user-selected
    validated
    nonauthoritative until publication succeeds

Credentials
    never stored in ordinary files
```

## 5. Filesystem Port

Chronicle will define a narrow filesystem abstraction.

Conceptually:

```csharp
public interface IChronicleFileSystem
{
    Task<Stream> OpenReadAsync(
        ChroniclePath path,
        CancellationToken cancellationToken);

    Task<AtomicWriteResult> WriteAtomicallyAsync(
        ChroniclePath destination,
        Func<Stream, CancellationToken, Task> writer,
        AtomicWriteOptions options,
        CancellationToken cancellationToken);

    Task<FileMetadata?> GetFileMetadataAsync(
        ChroniclePath path,
        CancellationToken cancellationToken);

    Task CreateDirectoryAsync(
        ChroniclePath path,
        CancellationToken cancellationToken);

    Task DeleteFileAsync(
        ChroniclePath path,
        DeleteOptions options,
        CancellationToken cancellationToken);
}
```

The exact API may be split into capability-specific ports.

## 6. Port Scope

The abstraction SHOULD cover operations that require:

- path policy;
- atomicity;
- test replacement;
- safe metadata;
- logging and error mapping.

It SHOULD NOT attempt to wrap every `System.IO` type generically.

## 7. Direct `System.IO`

Direct `System.IO` use is permitted only inside:

- filesystem Infrastructure adapters;
- installer or packaging scripts;
- narrowly reviewed startup bootstrap code;
- test-support implementations.

Domain, Application handlers, and ViewModels must not perform arbitrary direct filesystem access.

## 8. Path Value Object

Chronicle SHOULD use a validated path value object such as:

```text
ChroniclePath
```

It distinguishes:

- resolved internal path;
- user-selected external path;
- relative managed path;
- archive entry path.

These are not interchangeable.

## 9. Data Root

The primary mutable data root is per-user and local.

Conceptually on Windows:

```text
%LOCALAPPDATA%\Chronicle\
```

The final resolved path follows platform conventions.

## 10. Installation Directory

The installation directory contains:

- executable;
- runtime;
- immutable bundled assets;
- licenses;
- release metadata.

It MUST NOT contain mutable Campaign data.

## 11. Recommended Data Layout

Recommended initial layout:

```text
Chronicle/
├── data/
│   ├── chronicle.db
│   ├── chronicle.db-wal
│   └── chronicle.db-shm
├── config/
│   ├── appsettings.json
│   └── ui-state.json
├── backups/
├── exports/
├── diagnostics/
├── logs/
├── packages/
├── rule-knowledge/
│   ├── sources/
│   └── index/
├── checkpoints/
├── staging/
└── temp/
```

The exact physical layout may be adjusted through implementation validation.

## 12. Directory Ownership

Each directory has one owning subsystem.

Examples:

```text
data
    persistence

backups
    backup subsystem

exports
    export subsystem

packages
    package manager

logs
    observability

staging
    atomic publication workflows
```

## 13. No Arbitrary Root Writes

Modules must not create ad hoc files directly under the data root.

All paths are resolved through the path service.

## 14. Data Path Resolver

Chronicle will define:

```text
IDataPathResolver
```

It provides approved paths such as:

```text
GetDatabasePath()
GetConfigurationPath()
GetBackupDirectory()
GetExportDirectory()
GetLogDirectory()
GetStagingDirectory()
GetPackageDirectory()
GetRuleKnowledgeIndexDirectory()
```

## 15. Path Resolution

Path resolution MUST:

- normalize separators;
- resolve relative segments;
- reject path traversal;
- enforce root containment;
- enforce maximum path and segment lengths;
- reject invalid characters;
- distinguish files and directories;
- avoid using display names as authoritative path identity.

## 16. Root Containment

Managed internal paths MUST remain under their approved root.

A path that escapes through:

```text
..
absolute path
drive prefix
UNC prefix
symlink or reparse-point redirection
```

is rejected according to policy.

## 17. User-Selected External Paths

Backups, exports, imports, and sources may use user-selected external paths.

These remain explicitly classified as external and are not automatically trusted.

## 18. Path Display

The UI may display a user-friendly path.

Logs and diagnostics SHOULD prefer:

- safe basename;
- managed relative path;
- path hash;
- classified root key.

Full paths are included only in protected diagnostics when necessary.

## 19. File Naming

Managed files SHOULD use:

- stable semantic prefix;
- timestamp where useful;
- short opaque identifier suffix;
- approved extension;
- sanitized display component when user-facing.

## 20. User Display Names in File Names

Campaign titles may contribute to an export filename only after sanitization.

They are not used as internal identity.

## 21. Filename Sanitization

Sanitization MUST handle:

- reserved Windows names;
- invalid characters;
- trailing spaces and periods;
- excessive length;
- empty output;
- collisions;
- control characters.

## 22. Collision Handling

Filename collisions use:

- overwrite confirmation for user-selected export;
- short ID suffix;
- incremented safe suffix;
- atomic replace only when explicitly requested.

Silent overwrite is prohibited.

## 23. Atomic Write Pattern

Managed file publication follows:

```text
Resolve destination
    ↓
Create same-volume staging path
    ↓
Open with exclusive creation
    ↓
Write complete content
    ↓
Flush application buffers
    ↓
Flush file buffers where supported
    ↓
Close stream
    ↓
Validate staged artifact
    ↓
Atomically rename or replace
    ↓
Validate published metadata
    ↓
Clean obsolete staging
```

## 24. Same-Volume Staging

When publication relies on atomic rename, the staging file must be on the same filesystem volume as the destination.

## 25. Temporary Directory Selection

General temporary work may use Chronicle's managed temp directory.

Publication staging SHOULD use a destination-adjacent or same-volume staging directory.

## 26. Atomic Rename

A rename is treated as atomic only where the platform and filesystem semantics support it.

The adapter owns platform-specific behavior.

## 27. Atomic Replace

When replacing an existing file:

- preserve the existing valid file until the new file is complete;
- use platform-supported replace semantics where available;
- optionally retain a short-lived previous version;
- never truncate the active file before new validation.

## 28. Cross-Volume Move

Cross-volume move is not atomic.

For user-selected external destinations on another volume:

1. write to staging on the destination volume;
2. validate there;
3. rename within that volume.

Copying from internal staging directly into the final external filename is insufficient.

## 29. Validation Before Publication

Validation depends on artifact type.

Examples:

```text
Configuration
    parse and validate schema

Backup
    validate manifest, checksums, and snapshot

Export
    validate archive and manifest

Diagnostic bundle
    inspect allowlisted contents

Package
    validate package manifest and trust

Rule index
    validate index metadata and counts
```

## 30. Published Artifact

An artifact is published only after successful validation and final rename or replace.

The UI must not present a staging path as completed output.

## 31. Partial Files

Partial or staging files use names that cannot be mistaken for valid artifacts.

Recommended suffixes:

```text
.tmp
.partial
.staging
```

## 32. Staging Identity

Staging names SHOULD include:

- OperationId;
- WorkItemId when applicable;
- random or unique suffix;
- intended artifact type.

## 33. Staging Metadata

A staging workflow MAY persist a small sidecar or Work Item checkpoint containing:

- target;
- staging path reference;
- operation;
- artifact type;
- stage;
- created time;
- expected hash.

## 34. Stale Staging Recovery

Startup recovery inspects stale staging files.

It may:

- resume validation;
- complete publication when provably safe;
- delete nonauthoritative incomplete files;
- mark recovery required.

## 35. No Blind Temp Deletion

Chronicle must not delete every unknown file in staging or temp.

Cleanup applies only to Chronicle-owned recognizable entries.

## 36. Temporary File Retention

Temporary files have bounded retention based on:

- ownership marker;
- age;
- active Work Item reference;
- application instance;
- recovery status.

## 37. Cleanup Work

Cleanup MAY run:

- at startup;
- after successful operation;
- during maintenance;
- before shutdown.

It must be bounded and cancellable.

## 38. Deletion Semantics

Deletion means Chronicle requests ordinary filesystem deletion.

Chronicle MUST NOT claim guaranteed secure erasure on modern filesystems or SSDs.

## 39. Sensitive Temporary Files

Sensitive content should not be written to temporary files unless no safer design exists.

If required:

- use managed private directory;
- minimize retention;
- restrict permissions;
- delete promptly;
- exclude from logs and backups.

## 40. Credentials

Credentials MUST NOT be stored in:

- configuration files;
- temporary files;
- backup archives;
- export archives;
- package files;
- diagnostic bundles.

## 41. Permissions

Chronicle's data root SHOULD be accessible only to the current operating-system user under ordinary platform semantics.

## 42. Permission Claims

Chronicle does not claim protection beyond the operating-system account and filesystem policy.

Administrators or compromised user sessions may still access files.

## 43. File Creation Permissions

New files and directories SHOULD inherit or apply restrictive per-user permissions where practical.

Platform-specific implementation requires testing.

## 44. Read-Only Data Root

If the data root becomes read-only:

- normal mutation is blocked;
- queries may remain available where safe;
- Safe Mode reports the condition;
- no fallback database is created elsewhere silently.

## 45. Disk Space

Before large operations, Chronicle SHOULD estimate required free space.

Examples:

- backup;
- restore;
- migration checkpoint;
- export;
- Rule Knowledge rebuild.

## 46. Disk Space Is Advisory

Free-space checks reduce predictable failures but do not guarantee success.

The operation must still handle mid-write exhaustion safely.

## 47. Disk Full Failure

On disk-full:

- active valid artifact remains;
- staging remains incomplete and unpublished;
- transaction or Work Item records failure;
- cleanup and recovery are available;
- no zero-byte destination replaces valid content.

## 48. File Locking

Chronicle uses file locking for:

- data-directory ownership;
- selected artifact publication;
- external import inspection where required.

SQLite owns its own database locking behavior.

## 49. Lock Files

A lock file may contain safe metadata.

The lock itself must rely on an actual operating-system ownership primitive, not only file existence.

## 50. External File Locked

If an import or export destination is locked:

- return a typed error;
- preserve user selection;
- do not retry indefinitely;
- offer retry or choose another destination.

## 51. Sharing Modes

File open modes should be explicit.

Examples:

- logs may permit selected readers;
- active configuration replacement requires controlled access;
- backup validation opens read-only;
- staged publication uses exclusive creation.

## 52. SQLite Files

The persistence subsystem owns:

```text
chronicle.db
chronicle.db-wal
chronicle.db-shm
```

Other modules must not copy or delete these files directly.

## 53. SQLite Backup

Backups use the approved consistency-safe SQLite backup mechanism, not an ordinary active database file copy.

## 54. SQLite Sidecars

Cleanup must never delete WAL or SHM sidecar files based only on age.

## 55. Configuration Files

Configuration writes use atomic replacement.

A configuration update flow:

1. load current;
2. validate requested change;
3. serialize new version to staging;
4. parse and validate staging;
5. replace active file;
6. retain prior file temporarily when policy permits.

## 56. Configuration Corruption

If the active configuration cannot be parsed:

- preserve it;
- inspect previous validated copy if available;
- enter Safe Mode;
- require explicit repair or rollback.

## 57. UI State Files

Noncritical UI state may use a less expensive atomic write profile.

Corruption may reset only that UI state after preserving or reporting the invalid file.

It must not affect Campaign data.

## 58. Logs

Logs use rolling file management under the logging subsystem.

Atomic replacement rules apply to configuration and diagnostic packaging, not every append operation.

## 59. Log Directory Failure

If file logging is unavailable:

- bootstrap or console diagnostics may continue;
- Chronicle reports degraded observability;
- Campaign mutation policy depends on whether required audit records remain available in SQLite.

## 60. Backup Files

A backup file is valid only after:

- snapshot completion;
- manifest creation;
- checksum validation;
- atomic publication.

## 61. Backup Naming

Recommended pattern:

```text
chronicle-backup-{utcTimestamp}-{shortId}.chronicle-backup
```

## 62. Export Files

A Campaign export is valid only after:

- content generation;
- manifest;
- checksums;
- archive validation;
- final publication.

## 63. Import Files

Import inspection uses read-only access.

Archive entries are treated as untrusted.

## 64. Archive Path Safety

Archive extraction MUST reject:

- absolute paths;
- drive-qualified paths;
- UNC paths;
- `..` traversal;
- entries escaping extraction root;
- device names;
- symlink entries unless explicitly supported and safe;
- duplicate conflicting entries;
- excessive path length.

## 65. Archive Bomb Protection

Import and package extraction MUST enforce:

- maximum archive size;
- maximum extracted size;
- maximum entry count;
- maximum compression ratio;
- maximum nesting;
- cancellation;
- timeout or work budget.

## 66. Symlink Policy

The MVP SHOULD reject symlink and junction entries in imported archives and managed packages.

## 67. Managed Source References

Rule Knowledge sources referenced outside Chronicle's root remain external.

Chronicle records:

- source identity;
- safe display name;
- content hash;
- availability;
- transmission policy.

It does not assume permanent availability.

## 68. File Watchers

Filesystem watchers are not authoritative.

They MAY hint that an external Rule Knowledge source changed.

Chronicle confirms changes through explicit metadata and content hash.

## 69. File Watcher Failure

A missed watcher event is recovered by later inspection.

## 70. Package Files

Package installation uses:

- staging;
- manifest validation;
- trust validation;
- extraction into isolated version directory;
- atomic activation metadata.

## 71. Package Replacement

An active package version is not modified in place.

Install a new version beside it and update activation references after validation.

## 72. Rule Knowledge Index

A Rule Knowledge index is derived.

New index builds use staging and atomic publication.

The active index remains available until replacement succeeds.

## 73. Diagnostic Bundles

Diagnostic bundles are built from an explicit allowlist.

They are validated before publication.

## 74. Diagnostic Bundle Destination

A user-selected diagnostic destination is treated as external.

The bundle is written and published atomically on that destination volume.

## 75. File Metadata

Safe metadata MAY include:

```text
size
last write time UTC
creation time UTC when meaningful
content hash
file type
managed relative path
```

## 76. Metadata Trust

Filesystem timestamps are advisory.

Content hashes and Chronicle manifests are stronger artifact-integrity evidence.

## 77. Hashing

Content hashes SHOULD be computed through streaming.

The approved initial hash is SHA-256 for artifact integrity.

## 78. Hash Timing

Hashing occurs after the staged file is closed or through a carefully controlled stream pipeline.

The final published bytes must match the recorded hash.

## 79. Large Files

Large backup and export operations SHOULD use streaming.

They must not load the full artifact into memory.

## 80. Buffering

Buffer sizes are implementation details and should be measured.

They must not change contract semantics.

## 81. Cancellation

Long file operations accept cancellation.

Cancellation before publication:

- closes streams;
- leaves no valid published artifact;
- marks staging for cleanup or recovery.

## 82. Cancellation During Final Rename

Final rename or replace is a short non-cancellable stage where practical.

The result is then verified.

## 83. Process Crash

After crash:

- prior valid destination remains where replace semantics worked correctly;
- staging may remain;
- startup recovery identifies Chronicle-owned staging;
- no staging file is treated as valid automatically.

## 84. External Volume Removal

If a removable or external volume disappears:

- operation fails safely;
- internal authoritative state remains unchanged;
- the external artifact remains unpublished or incomplete.

Authoritative data roots on removable media are unsupported in MVP.

## 85. Network Shares

Network shares are unsupported for the authoritative Chronicle data root.

Reasons:

- lock semantics;
- latency;
- disconnects;
- rename behavior;
- SQLite safety;
- permission variability.

## 86. Cloud-Synchronized Folders

Cloud-synchronized folders are not recommended for active authoritative storage.

Backup or export artifacts may be placed there after publication.

## 87. Portable ZIP

The portable application package does not imply portable mutable data.

By default, it uses the normal local data root.

## 88. Multiple Data Profiles

Multiple first-class data roots or profiles are deferred.

The path service should avoid assumptions that make them impossible later.

## 89. File Associations

File associations for Chronicle exports or backups are deferred.

Opening an associated file must eventually route through safe inspection rather than immediate mutation.

## 90. Error Model

Filesystem errors SHOULD map to stable codes such as:

```text
filesystem.path-invalid
filesystem.path-outside-root
filesystem.permission-denied
filesystem.file-not-found
filesystem.directory-not-found
filesystem.file-locked
filesystem.disk-full
filesystem.destination-exists
filesystem.cross-volume-atomicity-unavailable
filesystem.atomic-replace-failed
filesystem.validation-failed
filesystem.archive-entry-unsafe
filesystem.archive-too-large
filesystem.cleanup-failed
filesystem.read-only
```

## 91. Data Preservation State

Filesystem failures SHOULD state whether:

```text
existing destination preserved
staging retained
staging deleted
publication unknown
authoritative data unchanged
```

## 92. Logging

Filesystem logs MAY include:

- operation type;
- managed path category;
- file size;
- duration;
- result code;
- artifact ID;
- path hash;
- staging cleanup result.

They SHOULD NOT include full paths or file content by default.

## 93. Observability

Useful metrics include:

```text
AtomicWriteDuration
AtomicWriteFailureCount
StaleStagingCount
CleanupFailureCount
DiskFullCount
UnsafeArchiveEntryCount
CrossVolumeFallbackCount
ExternalDestinationFailureCount
```

## 94. Test Filesystem

Chronicle.Testing SHOULD provide:

```text
InMemoryChronicleFileSystem
TemporaryDirectoryFileSystem
FaultInjectingFileSystem
RecordingFileSystem
```

## 95. In-Memory Filesystem

An in-memory adapter is useful for unit tests but does not replace real filesystem integration tests.

## 96. Temporary Directory Tests

Integration tests use isolated temporary directories with deterministic cleanup.

## 97. Fault Injection

The test filesystem SHOULD simulate:

- short write;
- disk full;
- permission denied;
- file locked;
- rename failure;
- delete failure;
- stream failure;
- cancellation;
- cross-volume behavior;
- stale staging.

## 98. Testing Strategy

The implementation requires:

```text
Unit Tests
Path Security Tests
Filesystem Integration Tests
Crash Tests
Archive Security Tests
Installer Interaction Tests
Backup and Export Tests
Architecture Tests
```

## 99. Path Security Tests

Tests MUST cover:

- valid managed path;
- `..` traversal;
- absolute path injection;
- UNC path;
- drive-qualified entry;
- reserved Windows name;
- invalid characters;
- overlong path;
- empty sanitized filename;
- root containment;
- symlink or reparse-point behavior.

## 100. Atomic Write Tests

Tests MUST cover:

- new file publication;
- existing file replacement;
- writer failure;
- flush failure;
- validation failure;
- rename failure;
- process termination before rename;
- process termination after rename;
- prior artifact preservation;
- same-volume requirement.

## 101. Backup and Export Tests

Tests MUST prove:

- partial artifact not visible as valid;
- checksum matches published file;
- external destination staging occurs on destination volume;
- cancellation leaves no valid artifact;
- overwrite requires explicit intent.

## 102. Archive Tests

Tests MUST cover:

- safe archive;
- traversal entry;
- absolute entry;
- duplicate entry;
- symlink entry;
- archive bomb;
- too many files;
- oversized extracted content;
- malformed manifest;
- checksum mismatch.

## 103. Permission Tests

Windows integration tests SHOULD cover:

- normal user access;
- read-only directory;
- denied external destination;
- locked configuration file;
- unavailable log directory.

## 104. Recovery Tests

Tests MUST cover:

- stale configuration staging;
- stale backup staging;
- stale export staging;
- active Work Item staging;
- unknown file in temp;
- cleanup failure;
- startup after interrupted publication.

## 105. Required Test Cases

Tests MUST cover:

- first-run directory creation;
- install directory remains immutable;
- configuration atomic write;
- configuration rollback;
- backup publication;
- export publication;
- diagnostic bundle publication;
- package version staging;
- Rule Knowledge index replacement;
- disk full;
- file locked;
- read-only root;
- external drive removal;
- network path rejection;
- stale temp cleanup;
- uninstall preserves data;
- reinstall reuses data;
- no credential file created;
- no direct filesystem use in restricted assemblies.

## 106. Architecture Tests

Architecture tests MUST reject:

- `System.IO` use in Domain;
- arbitrary `System.IO` use in Application handlers;
- ViewModels opening files directly;
- credentials written through filesystem ports;
- SQLite files copied by backup code;
- archive extraction without path policy;
- direct writes to final artifact paths for atomic workflows;
- unmanaged ad hoc directories under data root.

## 107. Prohibited Patterns

### 107.1 Write Directly to Final Backup File

Write, validate, and publish atomically.

### 107.2 Use Campaign Title as Internal Path Identity

Use typed IDs and sanitized display names only for user-facing filenames.

### 107.3 Trust Archive Entry Paths

Every entry is validated.

### 107.4 Delete SQLite Sidecars as Temp Files

Persistence owns them.

### 107.5 Plaintext Credential File

Use the operating-system credential store.

### 107.6 Treat File Existence as Lock Ownership

Use an operating-system lock primitive.

### 107.7 Copy Active SQLite Database Directly

Use the approved backup mechanism.

### 107.8 Silent Cross-Volume Rename Assumption

Stage on the destination volume.

### 107.9 Recreate Missing Authoritative File Automatically

Classify and recover explicitly.

### 107.10 Claim Secure Erasure

Ordinary deletion has limited guarantees.

## 108. Alternatives Considered

### Direct `System.IO` Everywhere

Rejected because path policy, fault injection, testing, and atomic publication would become inconsistent.

### One Generic Filesystem Library as Public Architecture

A third-party filesystem abstraction may be used internally, but Chronicle still needs product-specific path, publication, and security contracts.

### Store Everything in SQLite

Rejected because backups, exports, logs, packages, and diagnostic artifacts remain file-oriented.

### Store Campaigns as Individual JSON Files

Rejected because ADR-0004 selects relational SQLite persistence with transactional guarantees.

### Use Installation Directory for Data

Rejected because upgrades, permissions, portable packaging, and uninstall semantics require separation.

### Use Operating-System Temp Directory for All Staging

Rejected because atomic rename often requires same-volume staging and Chronicle needs owned cleanup.

## 109. Consequences

### Positive

- crash-safe artifact publication;
- consistent path security;
- testable file failures;
- clean data layout;
- safer imports and packages;
- no partial backup or export presented as valid;
- installer-independent mutable state;
- future platform adapters remain possible.

### Negative

- filesystem port and path types add code;
- atomic semantics vary by platform;
- staging and cleanup require maintenance;
- external destination workflows become more complex;
- symlink and reparse-point handling requires careful Windows testing.

## 110. Risks

### False Assumption of Atomicity

Mitigation:

- platform adapter;
- same-volume staging;
- integration tests;
- post-publication verification.

### Path Traversal

Mitigation:

- normalized typed paths;
- root containment;
- archive-entry validation;
- adversarial tests.

### Temp Accumulation

Mitigation:

- ownership markers;
- bounded retention;
- startup cleanup;
- metrics.

### Disk Exhaustion

Mitigation:

- free-space preflight;
- streaming;
- preserve existing artifact;
- typed recovery.

### External Filesystem Semantics

Mitigation:

- external paths classified separately;
- destination-volume staging;
- unsupported network-root policy.

## 111. Technology Spike

Before acceptance, implement:

1. `ChroniclePath`;
2. `IDataPathResolver`;
3. `IChronicleFileSystem`;
4. Windows filesystem adapter;
5. test filesystem adapters;
6. atomic configuration write;
7. atomic backup publication;
8. atomic export publication;
9. archive path validator;
10. destination-volume staging;
11. staging ownership metadata;
12. startup stale-staging scan;
13. fault injection;
14. Release data-layout test;
15. architecture scan for direct filesystem use.

## 112. Spike Acceptance

The spike passes when:

- all managed paths remain under approved roots;
- unsafe archive entries are rejected;
- failed writes preserve the prior valid file;
- backup and export artifacts are visible only after validation;
- cross-volume external publication stages on the destination volume;
- disk-full and locked-file failures produce typed results;
- startup distinguishes active from stale staging;
- no credential file is created;
- uninstall removes binaries without deleting data;
- Domain, Application handlers, and ViewModels contain no uncontrolled direct filesystem access.

## 113. Definition of Compliance

An implementation complies when:

- filesystem behavior is accessed through Chronicle-owned ports;
- mutable data is separate from installation files;
- paths are normalized, validated, and root-contained;
- atomic workflows use staging, flush, validation, and rename or replace;
- prior valid artifacts survive failed replacement;
- archives are treated as hostile input;
- temporary files have explicit ownership and retention;
- SQLite files remain under persistence ownership;
- credentials never use ordinary file storage;
- network shares are unsupported for authoritative MVP storage;
- filesystem faults are covered by deterministic tests.

## 114. Review Triggers

This ADR must be reviewed if:

- Linux or macOS becomes officially supported;
- multiple data profiles are introduced;
- encrypted filesystem storage is added;
- cloud synchronization manages active files;
- Chronicle supports network-hosted data;
- a separate worker process uses shared staging;
- file associations are introduced;
- package marketplace installation changes trust boundaries;
- sandboxed packaging changes writable paths;
- true portable-data mode becomes a requirement.

## 115. Deferred Decisions

Later ADRs MAY define:

- exact Windows atomic-replace API;
- exact file-permission implementation;
- exact lock-file mechanism;
- encrypted local artifact storage;
- secure deletion best-effort workflow;
- multiple data profiles;
- file associations;
- removable-media export workflow;
- cloud-sync compatibility;
- Linux and macOS filesystem adapters;
- storage quota policy.

## 116. Final Decision

Chronicle will treat local files as controlled artifacts with explicit ownership, path policy, staging, validation, and publication.

It will keep application binaries separate from user data.

It will never overwrite a valid artifact until the replacement is complete.

A temporary file may contain unfinished work.

Only an atomically published, validated file may claim to be a backup, export, package, configuration, or index.
