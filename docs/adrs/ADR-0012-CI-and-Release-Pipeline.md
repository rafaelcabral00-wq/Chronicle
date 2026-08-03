---
id: ADR-0012
title: CI and Release Pipeline
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
  - ADR-0005
  - ADR-0006
  - ADR-0007
  - ADR-0008
  - ADR-0009
  - ADR-0010
  - ADR-0011
  - RFC-0035
  - RFC-0036
  - RFC-0040
  - RFC-0041
  - RFC-0042
---

> **"The pipeline should make the safe path the easiest path and the unsafe path impossible to publish by accident."**

# CI and Release Pipeline

## 1. Status

**Proposed**

This ADR defines Chronicle's initial continuous integration and release pipeline.

The decision is:

- use **GitHub Actions** as the initial CI and release platform;
- keep pipeline definitions in the Chronicle monorepo;
- use separate workflows for validation, packaging, release candidate creation, and final release publication;
- require deterministic tests for normal pull requests;
- keep live-provider tests opt-in and outside mandatory correctness gates;
- build Windows x64 release artifacts on controlled Windows runners;
- generate manifests, checksums, test reports, and dependency inventories;
- sign release artifacts only in protected release jobs;
- promote exact validated release-candidate artifacts rather than rebuilding;
- block release when required quality, migration, security, backup, restore, or installer gates fail;
- allow contributors to run the same logical pipeline locally through repository scripts.

The decision becomes **Accepted** after a pipeline spike validates one complete prerelease from source revision to downloadable installer and portable archive.

## 2. Context

Chronicle is:

- open source;
- built as a .NET modular monolith;
- distributed initially as a Windows x64 desktop application;
- packaged with Inno Setup;
- backed by SQLite;
- dependent on deterministic Domain and Application behavior;
- integrated with an external Narrative Intelligence provider;
- strict about credentials, private fixtures, and proprietary Rule Set content.

The pipeline must protect against:

- broken architecture boundaries;
- failing migrations;
- nondeterministic Dice behavior;
- duplicate finalization effects;
- credential leakage;
- unsafe installer behavior;
- unreproducible artifacts;
- accidental publication from dirty or unreviewed source;
- release artifact replacement after validation;
- untrusted pull requests accessing release secrets;
- live-provider variability blocking ordinary development.

RFC-0040 defines testing and quality gates.

RFC-0041 defines build, packaging, release, and update architecture.

ADR-0011 selects Inno Setup and manual update delivery.

## 3. Decision Drivers

The pipeline prioritizes:

1. transparent open-source configuration;
2. strong pull-request validation;
3. protected release credentials;
4. deterministic correctness;
5. exact artifact promotion;
6. Windows packaging support;
7. migration and backup validation;
8. low contributor friction;
9. no live-provider dependency for normal CI;
10. traceable release evidence;
11. supply-chain hygiene;
12. maintainable workflow complexity.

## 4. Decision Summary

Chronicle will use:

```text
CI Platform
    GitHub Actions

Repository
    GitHub monorepo

Primary Validation Host
    Ubuntu runner for fast cross-platform .NET checks

Windows Validation Host
    Windows runner for Desktop, SQLite native behavior, packaging, and installer tests

Release Host
    Protected Windows runner or protected GitHub-hosted release job

Build Orchestration
    Repository-owned scripts
    dotnet CLI
    PowerShell for Windows packaging where needed

Package Management
    NuGet locked restore

Artifacts
    Test reports
    Coverage reports
    Architecture reports
    Migration reports
    Windows installer
    Portable ZIP
    Release manifest
    SHA-256 checksums
    SBOM or dependency inventory

Release Trigger
    Signed or protected version tag
    Manual approval through protected environment

Artifact Promotion
    Promote validated release-candidate artifacts
    No silent final rebuild

Live Provider Tests
    Optional
    Credential-gated
    Synthetic data only
    Not required for deterministic release correctness
```

## 5. Why GitHub Actions

GitHub Actions is selected because Chronicle intends to be hosted publicly on GitHub and needs:

- pull-request integration;
- branch protection;
- public workflow visibility;
- Windows and Linux runners;
- protected environments;
- artifact storage;
- release publication;
- secret isolation;
- matrix execution;
- dependency update integration.

The pipeline remains designed so core build and test logic resides in repository scripts rather than only in workflow YAML.

## 6. Pipeline-as-Code

All workflow definitions MUST be versioned under:

```text
.github/workflows/
```

Recommended workflows:

```text
ci.yml
security.yml
desktop-smoke.yml
package-windows.yml
release-candidate.yml
release.yml
nightly.yml
provider-live-tests.yml
dependency-review.yml
```

Not every workflow is required immediately.

## 7. Repository-Owned Build Logic

Workflow YAML should orchestrate rather than contain all build logic.

Recommended repository scripts:

```text
build/
├── build.ps1
├── build.sh
├── Test-Unit.ps1
├── Test-Integration.ps1
├── Test-Migrations.ps1
├── Test-Security.ps1
├── Package-Windows.ps1
├── Generate-Manifest.ps1
├── Generate-Checksums.ps1
└── Verify-Release.ps1
```

A .NET-based build orchestrator MAY replace or supplement scripts later.

## 8. Local and CI Parity

A developer SHOULD be able to run locally the same logical stages used by CI:

```text
restore
build
test
integration
architecture
security
package
verify
```

CI-specific wrappers may configure environment, caching, and artifact upload.

## 9. Workflow Categories

Chronicle distinguishes:

```text
Pull Request Validation
Main Branch Validation
Scheduled Validation
Release Candidate Build
Final Release Publication
Optional Live Provider Evaluation
```

## 10. Pull Request Validation

Every pull request SHOULD run:

1. repository policy checks;
2. dependency restore;
3. formatting or style checks;
4. compilation;
5. static analysis;
6. architecture tests;
7. unit and Domain tests;
8. Application tests;
9. contract tests;
10. selected SQLite integration tests;
11. security scanning;
12. documentation validation.

## 11. Main Branch Validation

Main branch validation SHOULD add:

- full SQLite integration suite;
- migration-chain tests;
- backup and restore tests;
- portable export/import tests;
- Windows Desktop component tests;
- accessibility smoke tests;
- long-running recovery tests;
- package conformance tests;
- installer build smoke when practical.

## 12. Scheduled Validation

A scheduled workflow MAY run:

- full long-Campaign suite;
- performance regression;
- fuzz tests;
- dependency vulnerability scan;
- optional live-provider evaluation;
- installer tests on clean Windows images;
- test-flakiness detection.

## 13. Release Candidate Pipeline

A release candidate pipeline MUST:

1. validate version and tag;
2. restore locked dependencies;
3. build from clean source;
4. run all mandatory deterministic tests;
5. run migration chains;
6. run backup and restore;
7. run export/import round trip where shipped;
8. run security suite;
9. run desktop smoke tests;
10. publish self-contained Windows x64 output;
11. build Inno Setup installer;
12. build portable ZIP;
13. generate manifest;
14. generate checksums;
15. generate dependency inventory or SBOM;
16. sign artifacts where enabled;
17. verify signatures;
18. run clean-install smoke test;
19. run upgrade test;
20. store exact validated artifacts.

## 14. Final Release Pipeline

The final release pipeline MUST promote the exact artifacts produced and validated by the release-candidate pipeline.

It MUST NOT rebuild binaries or installer silently.

Promotion SHOULD include:

- release notes;
- installer;
- portable ZIP;
- manifest;
- checksum file;
- source archive reference;
- SBOM or dependency inventory;
- signing status;
- supported platform metadata.

## 15. Release Trigger

A release SHOULD begin from:

```text
a protected version tag
```

Example:

```text
v0.1.0
v0.1.0-beta.1
```

The exact tag pattern must be validated.

## 16. Protected Environment

Release jobs MUST use a protected GitHub Environment such as:

```text
release
```

It SHOULD require:

- maintainer approval;
- branch or tag restrictions;
- isolated secrets;
- explicit deployment record.

## 17. Secret Isolation

Release and provider secrets MUST NOT be available to:

- ordinary pull-request jobs;
- workflows triggered from untrusted forks;
- arbitrary branch builds;
- documentation-only jobs;
- test jobs that do not need them.

## 18. Fork Pull Requests

Pull requests from forks run only with safe read-only permissions.

They MUST NOT receive:

- OpenAI API key;
- code-signing secret;
- package-publishing token;
- release token with write privileges;
- private Rule Set fixtures.

## 19. Workflow Permissions

Every workflow SHOULD declare minimal permissions explicitly.

Examples:

```text
contents: read
pull-requests: read
security-events: write only where needed
contents: write only in protected release publication
```

Broad default write permissions are prohibited.

## 20. Dependency Restore

NuGet restore MUST use locked mode for Release and CI validation.

Unexpected lockfile drift fails the build.

## 21. Dependency Cache

CI MAY cache NuGet packages.

The cache is an optimization only.

Correctness must not depend on cache state.

## 22. SDK Pinning

The repository's `global.json` pins the .NET SDK version or accepted roll-forward policy.

CI MUST honor it.

## 23. Build Configuration

Normal CI uses:

```text
Release
```

for compile and tests where practical.

Development-only behavior must not hide Release failures.

## 24. Deterministic Build Flags

Release builds SHOULD enable:

- deterministic compilation;
- continuous integration build metadata;
- source revision embedding;
- nullable reference types;
- selected warnings as errors;
- locked restore.

## 25. Dirty Source

Official release jobs MUST operate from committed source and fail if generated version or repository state does not match the release tag.

## 26. Version Source

Application version SHOULD be derived from one authoritative repository source.

Possible location:

```text
Directory.Build.props
```

or a version file consumed by build scripts.

The same version must feed:

- assembly metadata;
- installer;
- manifest;
- portable archive;
- release notes;
- diagnostics UI.

## 27. Version Validation

The release pipeline MUST verify:

- tag matches declared application version;
- prerelease label matches release channel;
- installer-compatible version is valid;
- manifest version matches binaries;
- bundled Rule Set versions are declared.

## 28. Build Matrix

Initial validation matrix SHOULD include:

```text
Ubuntu latest
    restore
    build
    unit
    Domain
    Application
    architecture
    contract
    security static checks

Windows latest
    SQLite integration
    Desktop tests
    Windows credential contract tests where possible
    packaging
    installer smoke
```

## 29. macOS Validation

macOS builds MAY be added later as development compatibility checks.

The pipeline MUST not imply official support until packaging and platform gates exist.

## 30. Test Categories

Tests SHOULD be tagged or separated by category:

```text
Unit
Domain
Application
Contract
Integration
Migration
Security
Desktop
Accessibility
EndToEnd
Performance
LiveProvider
FlakyQuarantine
```

## 31. Test Filtering

Workflows MAY select categories by stage.

Mandatory release correctness includes all nonoptional deterministic categories.

## 32. Test Reports

CI SHOULD upload machine-readable test reports.

Reports SHOULD include:

- test name;
- duration;
- outcome;
- fixture;
- seed where relevant;
- failure artifacts;
- environment.

## 33. Coverage

CI MAY collect code coverage.

Coverage is informative and may become a gate only through an explicit threshold decision.

Critical invariant tests remain mandatory regardless of percentage.

## 34. Coverage Artifacts

Coverage reports SHOULD avoid exposing private test fixtures or secrets.

## 35. Architecture Tests

Architecture tests are mandatory on every pull request.

They MUST enforce at least:

- Domain independence;
- no Avalonia outside Desktop;
- no EF Core outside persistence;
- no provider SDK leakage;
- no repositories in ViewModels;
- no Rule Set I/O;
- no raw Guid leakage where typed IDs exist;
- no test-support references in Release production output.

## 36. Static Analysis

CI SHOULD run:

- .NET analyzers;
- nullable analysis;
- formatting checks;
- banned API checks;
- dependency analysis;
- secret scanning;
- license review where available.

## 37. Security Scanning

Security workflow SHOULD include:

- dependency vulnerability review;
- secret scanning;
- static code analysis;
- package inventory;
- malicious fixture checks;
- installer-content inspection;
- restricted-content scan.

## 38. Restricted Content Scan

The pipeline MUST scan public artifacts and repository fixtures for unauthorized proprietary Rule Set text according to project policy.

A detection blocks release pending review.

## 39. Credential Leak Scan

The pipeline MUST search:

- source;
- build output;
- logs;
- test reports;
- installer contents;
- portable ZIP;
- diagnostic bundles;
- manifests.

Synthetic canary secrets SHOULD be used in security tests.

## 40. Live Provider Tests

Live OpenAI tests are separated into a dedicated workflow.

They require:

- protected secret;
- explicit manual or scheduled trigger;
- synthetic content;
- cost limit;
- timeout;
- test tagging;
- no untrusted fork execution.

## 41. Live Tests Are Not Core Correctness

A provider outage or temporary model issue MUST NOT make it impossible to validate deterministic Chronicle correctness.

However, a release may still be blocked manually if the selected official provider is known to be incompatible.

## 42. Provider Compatibility Gate

Before release, one bounded compatibility test SHOULD confirm:

- configured model availability;
- structured-output support;
- Narrator response;
- Archivist response;
- Campaign Generator response;
- safe credential handling.

This may be a manual protected gate rather than ordinary CI.

## 43. Test Data

CI uses only:

- synthetic Campaigns;
- synthetic credentials;
- original Rule Set summaries;
- open or licensed fixtures;
- redacted recorded provider responses.

## 44. Private Fixtures

If private legal fixtures are ever required, they MUST:

- remain outside public pull-request jobs;
- use protected storage;
- be optional to open-source correctness where possible;
- never enter artifacts;
- have explicit ownership and retention.

## 45. SQLite Integration

SQLite integration tests MUST use the real SQLite provider and native library selected for Release.

In-memory database substitutes do not replace these tests.

## 46. Migration Pipeline

Migration validation MUST test:

- prior supported version;
- oldest supported version;
- selected intermediate fixtures;
- complex Campaign;
- failure injection;
- checkpoint preservation;
- post-migration integrity.

## 47. Migration Artifact

The pipeline SHOULD generate a migration report containing:

- source versions tested;
- target version;
- fixture identities;
- result;
- duration;
- integrity status.

## 48. Backup and Restore Gate

Release candidate validation MUST:

1. create a canonical Campaign;
2. create a backup;
3. validate backup;
4. restore to isolated storage;
5. run integrity checks;
6. compare semantic state.

## 49. Export and Import Gate

If portable Campaign export ships, release validation MUST test:

- export;
- manifest;
- checksum;
- import as new;
- identity remap;
- dependency reporting;
- malicious archive rejection.

## 50. Desktop Testing

Windows jobs SHOULD run:

- ViewModel tests;
- Avalonia component tests;
- headless tests;
- one desktop process smoke test;
- keyboard workflow;
- reduced-motion workflow;
- window placement recovery.

## 51. Installer Build

The installer job:

- consumes validated publish output;
- invokes Inno Setup noninteractively;
- embeds declared version metadata;
- includes release manifest;
- excludes user data and secrets;
- produces deterministic file inventory where practical.

## 52. Installer Inspection

The pipeline SHOULD inspect installer content for:

- unexpected files;
- private fixtures;
- development configuration;
- test binaries;
- symbols if not intended;
- credentials;
- unauthorized source content.

## 53. Portable ZIP Build

The portable archive MUST be built from the same validated publish directory as the installer.

## 54. Manifest Generation

The release manifest MUST be generated from build inputs and verified against actual artifacts.

It SHOULD not rely solely on manually maintained values.

## 55. Checksum Generation

SHA-256 checksums are generated after final artifact signing.

The checksum file is itself stored with release evidence.

## 56. Signing

Signing occurs only in protected release jobs.

Possible signing materials include:

- certificate;
- private key;
- hardware or remote signing credentials;
- timestamp service configuration.

## 57. Signing Secret Handling

Signing secrets MUST:

- never be available to pull requests;
- never be written to disk longer than necessary;
- never be printed;
- use protected GitHub secrets or external signing service;
- support rotation.

## 58. Unsigned Prereleases

The pipeline MAY produce unsigned Development or early Beta artifacts.

Their manifest and filenames MUST identify the unsigned classification.

## 59. Artifact Attestation

Chronicle SHOULD evaluate generating artifact provenance or build attestations.

This is recommended but not required for initial MVP acceptance.

## 60. SBOM

The release pipeline SHOULD generate a Software Bill of Materials or equivalent dependency inventory.

The exact format is deferred, with SPDX or CycloneDX as candidates.

## 61. License Inventory

Release validation MUST produce or verify third-party license notices.

## 62. Artifact Retention

CI artifact retention SHOULD distinguish:

```text
Pull Request Artifacts
    short retention

Main Branch Artifacts
    moderate retention

Release Candidate Artifacts
    retained through release decision

Published Release Artifacts
    long-term release retention
```

## 63. Release Evidence Bundle

Each release SHOULD retain:

- source revision;
- workflow run identity;
- test reports;
- migration report;
- security scan summary;
- backup/restore report;
- installer smoke report;
- manifest;
- checksums;
- SBOM;
- signing result;
- release notes.

## 64. Exact Artifact Identity

Every artifact SHOULD have:

- artifact name;
- byte size;
- SHA-256;
- build revision;
- application version;
- packaging type.

## 65. Release Candidate Promotion

A release-candidate workflow SHOULD output one immutable artifact set.

Final release promotion references that set by workflow run or artifact ID.

## 66. No Rebuild Rule

If any artifact is rebuilt after validation:

- it receives a new identity;
- checksums change;
- all relevant gates rerun;
- prior approval is invalidated.

## 67. Branch Protection

The `main` branch SHOULD require:

- pull request;
- required status checks;
- architecture tests;
- deterministic tests;
- review approval;
- no unresolved merge conflict;
- up-to-date branch according to repository policy.

## 68. Direct Push

Direct push to `main` SHOULD be restricted.

Emergency maintainer bypass requires documented reason and immediate follow-up validation.

## 69. Required Reviews

Changes to high-risk areas SHOULD require relevant review.

Examples:

```text
Persistence
Migrations
Security
Credentials
Installer
Release Workflows
Rule Set Mechanics
Structured Output Contracts
```

## 70. CODEOWNERS

The repository MAY use CODEOWNERS for critical paths.

Example areas:

```text
/.github/workflows/
/build/packaging/
/src/Chronicle.Persistence.Sqlite/
/src/Chronicle.Infrastructure/Security/
/src/Chronicle.NarrativeIntelligence.OpenAI/
/src/Chronicle.RuleSets.Werewolf/
/docs/adrs/
```

## 71. Workflow Change Security

Workflow files are privileged code.

Changes to release or secret-using workflows MUST require maintainer review.

## 72. Dependency Updates

Automated dependency update pull requests MAY be enabled.

They must pass all ordinary gates.

Major runtime, Avalonia, EF Core, SQLite, provider SDK, installer, or security dependency upgrades require explicit review.

## 73. Generated Files

CI SHOULD verify generated files are current.

Examples:

- schemas;
- migration snapshots;
- manifests;
- package catalogs;
- source-generated API baselines.

## 74. Documentation Validation

CI SHOULD verify:

- RFC and ADR front matter;
- unique identifiers;
- dependency references;
- broken internal links;
- required status field;
- no duplicate document IDs.

## 75. Changelog Validation

Release workflow SHOULD verify that the target version has release notes or changelog content.

## 76. Package Version Validation

Bundled Rule Set package versions MUST be:

- declared;
- compatible;
- tested;
- listed in the release manifest.

## 77. Performance Tests

Performance tests MAY run on scheduled or release workflows.

They SHOULD use reference thresholds for:

- startup;
- Campaign load;
- Message append;
- Dice Roll commit;
- finalization;
- backup;
- migration.

## 78. Performance Regression Policy

A significant regression requires:

- explanation;
- explicit approval;
- or correction before release.

Exact thresholds require a later performance ADR.

## 79. Flaky Test Handling

CI MUST not treat repeated blind reruns as proof of success.

A flaky test MAY be quarantined only with:

- issue;
- owner;
- expiration;
- visible status.

Release-blocking critical-path tests cannot remain quarantined indefinitely.

## 80. Failure Artifacts

Failed workflows SHOULD upload useful safe artifacts such as:

- test report;
- screenshot;
- structured test log;
- migration diff;
- installer log;
- fixture identifier;
- deterministic seed.

They MUST exclude secrets and private Campaign content.

## 81. Cancellation

Superseded pull-request runs MAY be canceled.

Release jobs SHOULD not be canceled automatically after signing or publication starts without explicit operator awareness.

## 82. Concurrency Groups

GitHub Actions concurrency SHOULD prevent:

- two release publications for the same version;
- overlapping protected signing jobs;
- duplicate packaging for the same tag;
- uncontrolled nightly overlap.

## 83. Timeouts

Every job MUST have a bounded timeout.

Timeouts should reflect stage complexity.

A timeout produces explicit failure, not silent success.

## 84. Retry

Workflow-level retry is manual or explicitly bounded.

The pipeline MUST not hide persistent failures through automatic reruns.

## 85. Runner Trust

GitHub-hosted runners are acceptable initially.

Self-hosted runners MAY be introduced for:

- hardware signing;
- specialized Windows installer tests;
- performance stability;
- local model evaluation.

Self-hosted runners require hardening and maintenance.

## 86. Self-Hosted Runner Security

A self-hosted release runner MUST:

- be isolated;
- be ephemeral where practical;
- not run untrusted pull-request code;
- clear workspace and secrets;
- receive security updates;
- restrict network and account permissions.

## 87. Release Publication

Final publication SHOULD create a GitHub Release associated with the version tag.

It includes:

- notes;
- supported platform;
- installer;
- portable ZIP;
- checksums;
- manifest;
- SBOM;
- signing status;
- known issues.

## 88. Draft Releases

Release candidates MAY be published as draft or prerelease releases for manual validation.

## 89. Stable Release

Stable release requires explicit approval after all required gates pass.

## 90. Release Rollback

If a published release is defective:

1. mark release as withdrawn or affected;
2. stop recommending download;
3. preserve evidence;
4. publish advisory;
5. issue corrected release;
6. do not silently replace existing artifact bytes under the same version.

## 91. Immutable Published Artifacts

A published version's artifact bytes MUST NOT be replaced silently.

A correction requires a new version.

## 92. Release Notes Generation

Release notes MAY be partially generated from labels or changelog fragments.

Maintainer review is required.

## 93. Changelog Fragments

The project MAY later adopt structured changelog fragments.

Not required for MVP.

## 94. CI Cost Control

To control CI usage:

- fast tests run on every PR;
- expensive suites run on main, schedule, or release;
- caches are used safely;
- matrices remain intentional;
- live-provider tests remain opt-in.

Cost control must not remove critical safety gates.

## 95. Open-Source Transparency

Public workflows and test results SHOULD make release quality visible.

Sensitive details remain redacted.

## 96. Error Model

Pipeline failures SHOULD be categorized:

```text
RestoreFailed
BuildFailed
StaticAnalysisFailed
ArchitectureViolation
UnitTestFailed
IntegrationTestFailed
MigrationTestFailed
SecurityScanFailed
RestrictedContentDetected
CredentialLeakDetected
DesktopSmokeFailed
PackagingFailed
InstallerVerificationFailed
SignatureFailed
ChecksumFailed
ManifestMismatch
ReleaseApprovalMissing
ArtifactPromotionFailed
PublicationFailed
```

## 97. Prohibited Patterns

### 97.1 Live Provider Required for Every Pull Request

Deterministic tests must stand alone.

### 97.2 Release Secrets in Fork Workflows

Prohibited.

### 97.3 Rebuild Between Candidate and Final

Promote exact artifacts.

### 97.4 Unpinned Dependencies in Release

Locked restore is required.

### 97.5 Release From Dirty or Untagged Source

Prohibited.

### 97.6 Installer Published Without Upgrade Test

Blocked.

### 97.7 Migration Change Without Migration Fixtures

Blocked.

### 97.8 Silent Artifact Replacement

A new version is required.

### 97.9 Workflow YAML as the Only Build Definition

Core logic must be locally runnable.

### 97.10 Broad Default Workflow Permissions

Permissions are explicit and minimal.

## 98. Alternatives Considered

### Azure DevOps Pipelines

Capable and mature, especially for .NET and Windows.

Not selected initially because Chronicle's public GitHub repository benefits from native GitHub pull-request, release, and contributor integration.

### GitLab CI

Strong integrated CI platform.

Not selected because the project intends to use GitHub as its primary public collaboration platform.

### Jenkins

Flexible but introduces server maintenance and security burden inappropriate for the MVP.

### Local Manual Release Only

Rejected because it weakens repeatability, evidence, secret isolation, and contributor trust.

### Nuke or Cake as Immediate Build Orchestrator

Potentially useful, but not required before repository scripts prove the workflow.

A later ADR may select one if scripting complexity grows.

## 99. Consequences

### Positive

- transparent CI;
- strong GitHub integration;
- Windows packaging support;
- protected release secrets;
- exact artifact promotion;
- contributor-friendly pull-request checks;
- deterministic correctness independent from providers;
- traceable release evidence.

### Negative

- GitHub Actions becomes an operational dependency;
- Windows runner time may be slower or more expensive;
- workflow maintenance adds complexity;
- signing may require external infrastructure;
- artifact retention limits require policy;
- cross-platform matrix grows over time.

## 100. Risks

### Workflow Secret Exposure

Mitigation:

- minimal permissions;
- protected environments;
- no secrets for forks;
- workflow code review;
- isolated signing.

### Candidate and Final Artifact Drift

Mitigation:

- immutable artifact set;
- checksum identity;
- promotion workflow;
- no rebuild rule.

### CI and Local Divergence

Mitigation:

- repository-owned scripts;
- same commands locally;
- pinned SDK and dependencies.

### Flaky Windows UI Tests

Mitigation:

- component tests first;
- bounded process smoke tests;
- stable fixtures;
- explicit quarantine policy.

### Excessive Pipeline Time

Mitigation:

- layered workflows;
- category filtering;
- main and scheduled deep suites;
- caching;
- measured optimization.

## 101. Technology Spike

Before acceptance, implement:

1. `ci.yml`;
2. Linux restore, build, analyzers, architecture, and unit tests;
3. Windows SQLite and Desktop tests;
4. locked NuGet restore;
5. secret scan;
6. restricted-content scan;
7. migration test report;
8. backup and restore gate;
9. Windows self-contained publish;
10. Inno Setup package;
11. portable ZIP;
12. manifest and SHA-256 generation;
13. protected release environment;
14. release-candidate artifact upload;
15. final artifact promotion to a prerelease.

## 102. Spike Acceptance

The spike passes when:

- a fork pull request cannot access secrets;
- architecture violations fail the PR;
- SQLite migrations run on Windows;
- the installer is built noninteractively;
- the exact release-candidate installer is promoted;
- checksums match;
- manifest matches binaries and package versions;
- a clean Windows environment installs and starts Chronicle;
- uninstall preserves data;
- no live OpenAI call is required for ordinary success;
- release evidence is retained.

## 103. Definition of Compliance

An implementation complies when:

- GitHub Actions hosts the initial CI and release workflows;
- core build logic is locally runnable;
- pull requests run deterministic quality gates;
- release secrets are isolated;
- dependencies are locked;
- migration, backup, restore, security, and installer gates block release;
- live-provider tests are optional and protected;
- Windows artifacts are built through controlled jobs;
- manifests, checksums, and dependency inventory are generated;
- release-candidate artifacts are promoted without rebuilding;
- published artifact bytes are immutable per version.

## 104. Review Triggers

This ADR must be reviewed if:

- Chronicle moves away from GitHub;
- CI cost becomes unsustainable;
- self-hosted signing becomes necessary;
- Linux or macOS packaging becomes official;
- automatic updates require additional publication metadata;
- package registry publication is introduced;
- public NuGet packages are published;
- performance testing requires stable dedicated hardware;
- GitHub Actions security or availability becomes unsuitable.

## 105. Deferred Decisions

Later ADRs MAY define:

- exact build orchestration framework;
- exact SBOM format;
- artifact attestation;
- code-signing provider;
- performance benchmark infrastructure;
- automatic release notes;
- NuGet publication;
- package registry pipeline;
- Linux and macOS release jobs;
- automatic update-feed publication;
- self-hosted release runners.

## 106. Final Decision

Chronicle will use GitHub Actions for its initial CI and release pipeline.

Pull requests will prove architecture and deterministic correctness.

Protected release jobs will build, inspect, sign when available, verify, and package the Windows application.

Final releases will promote the exact artifacts already validated as release candidates.

The pipeline does not decide whether the story is good.

It decides whether Chronicle is safe to trust with the story.
