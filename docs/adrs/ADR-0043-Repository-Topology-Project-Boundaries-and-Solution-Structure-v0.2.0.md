---
id: ADR-0043
title: Repository Topology, Project Boundaries, and Solution Structure
status: Accepted
version: 0.2.0
owner: Chronicle Team
last_updated: 2026-08-03
category: Technology
supersedes:
  - ADR-0043@0.1.0
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
  - RFC-0035
  - RFC-0036
  - RFC-0037
  - RFC-0038
  - RFC-0039
  - RFC-0040
  - RFC-0041
  - RFC-0042
  - RFC-0043
  - ADR-0001
  - ADR-0002
  - ADR-0003
  - ADR-0004
  - ADR-0005
  - ADR-0006
  - ADR-0007
  - ADR-0008
  - ADR-0009
  - ADR-0010
  - ADR-0011
  - ADR-0012
  - ADR-0013
  - ADR-0014
  - ADR-0015
  - ADR-0016
  - ADR-0017
  - ADR-0018
  - ADR-0019
  - ADR-0020
  - ADR-0021
  - ADR-0022
  - ADR-0023
  - ADR-0024
  - ADR-0025
  - ADR-0026
  - ADR-0027
  - ADR-0028
  - ADR-0029
  - ADR-0030
  - ADR-0031
  - ADR-0032
  - ADR-0033
  - ADR-0034
  - ADR-0035
  - ADR-0036
  - ADR-0037
  - ADR-0038
  - ADR-0039
  - ADR-0040
  - ADR-0041
  - ADR-0042
  - ADR-0044
implements: []
related_to:
  - ADR-0004
  - ADR-0033
  - ADR-0034
  - ADR-0036
---

> **"Repository structure should reveal the architecture already chosen. It must not quietly invent a second one."**

# Repository Topology, Project Boundaries, and Solution Structure

## 1. Status

**Accepted**

This ADR defines Chronicle's repository and solution topology.

This revision resolves the conflict between ADR-0002 and the earlier ADR-0043.

Effective 2026-08-03, DR-0001 accepted Option B and made this ADR authoritative for the concrete current repository topology, project boundaries, paths, project names, and dependency layout. This status change records that already accepted authority; it does not change architectural semantics.

The decision is:

- ADR-0002 remains the canonical architecture and dependency-direction decision;
- ADR-0043 does not redefine the architecture;
- ADR-0043 is authoritative for the concrete current repository topology, project boundaries, paths, project names, and dependency layout;
- where concrete topology statements conflict, ADR-0043 prevails;
- this ADR translates ADR-0002 into one concrete repository and solution layout;
- project names must be stable, explicit, and aligned to architectural responsibilities;
- use one main solution;
- separate production code, tests, tools, documentation, samples, and packaged Rule Sets;
- keep Domain independent from Application, Infrastructure, Presentation, provider adapters, and Rule Set implementations;
- keep Application independent from concrete persistence, desktop UI, provider SDKs, installer tooling, and exact Rule Set packages;
- keep `Persistence.Sqlite` as the concrete persistence project name;
- keep `RuleSets` as the repository area for package-facing contracts and implementations;
- keep `Presentation.Desktop` as the official desktop UI project;
- keep provider-neutral Narrative Intelligence contracts separate from provider adapters;
- do not introduce a generic `RuleSet.Runtime` project in MVP unless a proven shared-runtime responsibility appears;
- do not introduce a separate `RuleKnowledge` production project in MVP;
- place rules-only source material and generated package knowledge artifacts outside Chronicle Core code and under explicit provenance controls;
- organize tests by production boundary and test purpose;
- prohibit circular dependencies;
- prohibit provider SDKs, EF Core, desktop UI, or system-specific package code from leaking inward;
- use English names for folders, projects, namespaces, contracts, and technical documentation;
- keep Werewolf as the first official Rule Set package, not as a Core project dependency;
- keep the topology small enough for the MVP while leaving explicit extension points.

## 2. Context

Chronicle's architecture already defines several major boundaries:

- Domain;
- Application;
- Infrastructure;
- Persistence;
- Narrative Intelligence;
- Rule Set packages;
- Presentation;
- desktop host;
- tests and tools.

The previous ADR-0043 introduced alternative project names and boundaries that conflicted with ADR-0002.

Examples of ambiguity included:

- `Persistence` versus `Persistence.Sqlite`;
- `RuleSet` versus `RuleSets`;
- whether Presentation existed as a separate project;
- whether a generic `RuleSet.Runtime` project was mandatory;
- whether `RuleKnowledge` was a Core production project;
- competing test layouts;
- overlapping host and desktop responsibilities.

A repository cannot serve two incompatible architecture maps.

## 3. Decision Drivers

The topology prioritizes:

1. alignment with ADR-0002;
2. clear dependency direction;
3. low MVP project count;
4. testability;
5. replaceable infrastructure;
6. Rule Set neutrality;
7. provider neutrality;
8. open-source readability;
9. future package growth;
10. prevention of architectural leakage;
11. simple build and release;
12. stable naming.

## 4. Canonical Authority

Effective 2026-08-03, DR-0001 accepted Option B for topology authority.

ADR-0002 defines:

- architectural layers;
- dependency direction;
- inward-facing contracts;
- replaceability boundaries.

This ADR defines:

- folders;
- project names;
- solution organization;
- test placement;
- repository conventions;
- concrete current repository topology;
- project boundaries;
- paths;
- dependency layout.

## 5. Conflict Resolution Rule

When an architecture-semantic interpretation conflicts with ADR-0002:

```text
ADR-0002 wins
```

A future architecture-semantics change must supersede ADR-0002 explicitly.

When a concrete topology, path, project-name, project-boundary, test-placement, or dependency-layout statement conflicts between ADR-0002 and ADR-0043 v0.2.0:

```text
ADR-0043 wins
```

This is a partial supersession of ADR-0002 concrete materialization examples only. It does not supersede ADR-0002's architectural semantics.

## 6. Repository Root

Recommended root:

```text
Chronicle/
├── Chronicle.sln
├── Directory.Build.props
├── Directory.Build.targets
├── Directory.Packages.props
├── global.json
├── .editorconfig
├── .gitignore
├── LICENSE
├── NOTICE
├── CONTRIBUTING.md
├── SECURITY.md
├── TRADEMARKS.md
├── README.md
├── src/
├── tests/
├── rule-sets/
├── tools/
├── docs/
├── samples/
├── build/
└── artifacts/
```

## 7. Main Solution

Use one primary solution:

```text
Chronicle.sln
```

## 8. Secondary Solutions

Secondary filtered solutions may be added later for developer convenience.

They must not define different dependency rules.

## 9. Production Source Root

Production code lives under:

```text
src/
```

## 10. Canonical Production Projects

Recommended MVP production projects:

```text
src/
├── Chronicle.Domain/
├── Chronicle.Application/
├── Chronicle.Contracts/
├── Chronicle.RuleSets.Abstractions/
├── Chronicle.NarrativeIntelligence.Abstractions/
├── Chronicle.Infrastructure/
├── Chronicle.Persistence.Sqlite/
├── Chronicle.NarrativeIntelligence.OpenAI/
├── Chronicle.Presentation.Desktop/
└── Chronicle.Desktop/
```

## 11. Chronicle.Domain

Responsibilities:

- Campaign aggregate;
- Domain entities;
- value objects;
- Domain invariants;
- Domain services where necessary;
- Domain events where selected;
- system-neutral mechanical concepts;
- no infrastructure dependencies.

## 12. Domain Prohibitions

`Chronicle.Domain` must not reference:

- EF Core;
- SQLite;
- provider SDKs;
- Windows UI frameworks;
- filesystem implementations;
- installer code;
- specific Rule Set packages;
- OpenAI or another provider;
- JSON serializer implementations unless explicitly justified as pure contract support.

## 13. Chronicle.Application

Responsibilities:

- use cases;
- commands and queries;
- orchestration;
- Operation Records;
- Work Items;
- Narrative Turns;
- event routing;
- transaction-boundary contracts;
- repositories and ports;
- authorization and validation coordination;
- recovery workflows;
- package selection and binding;
- provider-neutral execution coordination.

## 14. Application Dependencies

`Chronicle.Application` may reference:

```text
Chronicle.Domain
Chronicle.Contracts
Chronicle.RuleSets.Abstractions
Chronicle.NarrativeIntelligence.Abstractions
```

## 15. Application Prohibitions

It must not reference:

- EF Core;
- SQLite;
- provider SDKs;
- desktop UI frameworks;
- Inno Setup;
- specific official Rule Set implementations;
- specific provider adapters;
- Windows Credential Manager implementation.

## 16. Chronicle.Contracts

Responsibilities:

- versioned public and cross-boundary contracts;
- canonical identifiers where shared;
- portable DTOs;
- stable error envelopes;
- serialization-neutral contract definitions;
- event and result envelopes shared between allowed projects.

## 17. Contract Discipline

`Chronicle.Contracts` must not become a dumping ground.

Only contracts that genuinely cross a boundary belong there.

## 18. Contracts Prohibitions

It must not reference:

- Domain implementation;
- EF Core;
- provider SDKs;
- desktop UI;
- Rule Set implementation packages.

## 19. Chronicle.RuleSets.Abstractions

Responsibilities:

- Rule Set package interfaces;
- package descriptors;
- schema contracts;
- operation contracts;
- Dice request and resolution extension points;
- Character field schemas;
- package validation;
- package compatibility contracts;
- package execution boundary.

## 20. Rule Set Abstraction Neutrality

This project must not contain Werewolf-specific mechanics.

## 21. No Mandatory RuleSet.Runtime Project

The MVP does not create:

```text
Chronicle.RuleSet.Runtime
```

as a generic production project.

## 22. Runtime Responsibility Placement

Shared package execution coordination belongs initially in:

```text
Chronicle.Application
Chronicle.Infrastructure
Chronicle.RuleSets.Abstractions
```

according to responsibility.

## 23. Future Runtime Extraction

A dedicated runtime project may be introduced only when:

- at least two package implementations need shared executable infrastructure;
- the responsibility cannot remain cleanly in existing boundaries;
- dependency direction is preserved;
- a new ADR approves it.

## 24. Chronicle.NarrativeIntelligence.Abstractions

Responsibilities:

- provider-neutral request contracts;
- `NarratorTurnOutput`;
- Narrative Blocks;
- Structured Events;
- provider-neutral adapter interface;
- provider capability contracts;
- safe usage metadata;
- provider failure classifications.

## 25. Narrative Abstraction Prohibitions

It must not reference:

- OpenAI SDK;
- specific provider response classes;
- EF Core;
- desktop UI;
- Rule Set implementations.

## 26. Chronicle.Infrastructure

Responsibilities:

- generic infrastructure implementations not tied exclusively to SQLite or one provider;
- filesystem abstraction;
- time;
- randomness;
- hashing;
- archive primitives;
- platform services;
- Windows Credential Manager implementation where no separate project is needed;
- process and environment services;
- safe staging;
- package filesystem loading.

## 27. Infrastructure Dependencies

Infrastructure may reference:

```text
Chronicle.Application
Chronicle.Contracts
Chronicle.RuleSets.Abstractions
Chronicle.NarrativeIntelligence.Abstractions
```

as required by implemented ports.

## 28. Infrastructure Prohibition

It must not become a second Application layer.

Business orchestration remains in Application.

## 29. Chronicle.Persistence.Sqlite

Responsibilities:

- `ChronicleDbContext`;
- EF Core mappings;
- SQLite configuration;
- migrations;
- repository implementations;
- transaction coordinator;
- persistence queries;
- Work Item claims;
- Operation persistence;
- Narrative Turn persistence;
- Provider Attempt persistence;
- Dice evidence persistence;
- backup snapshot integration;
- restore staging database support.

## 30. Canonical Persistence Project Name

The concrete project name is:

```text
Chronicle.Persistence.Sqlite
```

not a generic `Chronicle.Persistence` implementation project.

## 31. Why Concrete Naming

The name makes the technology choice visible and leaves room for future alternatives without renaming abstractions.

## 32. Persistence Dependencies

It may reference:

```text
Chronicle.Application
Chronicle.Domain
Chronicle.Contracts
Chronicle.RuleSets.Abstractions
```

where mapping and port implementation require them.

## 33. Persistence Prohibitions

It must not reference:

- Presentation;
- desktop host;
- OpenAI adapter;
- official Rule Set implementation projects.

## 34. Chronicle.NarrativeIntelligence.OpenAI

Responsibilities:

- official OpenAI provider adapter;
- provider request translation;
- provider response normalization;
- capability detection;
- safe provider metadata mapping;
- provider-specific retry hints;
- adapter contract tests.

## 35. Provider Adapter Dependencies

It may reference:

```text
Chronicle.NarrativeIntelligence.Abstractions
Chronicle.Contracts
Chronicle.Application
```

only as needed to implement declared ports.

## 36. Provider Adapter Prohibitions

It must not reference:

- EF Core;
- `ChronicleDbContext`;
- desktop UI;
- Rule Set implementation packages;
- Domain repositories.

## 37. Provider Naming

A provider project is named by provider implementation:

```text
Chronicle.NarrativeIntelligence.<Provider>
```

## 38. Chronicle.Presentation.Desktop

Responsibilities:

- desktop presentation models;
- views and view models;
- navigation;
- accessibility;
- localization resources;
- user interaction;
- Roll card;
- recovery views;
- Safe Mode views;
- installer-independent desktop UI logic.

## 39. Presentation Technology

The exact desktop UI framework follows its own selected ADR.

## 40. Presentation Dependencies

It may reference:

```text
Chronicle.Application
Chronicle.Contracts
```

and UI framework packages.

## 41. Presentation Prohibitions

It must not:

- access DbContext;
- call provider SDKs;
- load Rule Set implementation internals directly;
- generate Dice outcomes;
- write files outside approved Application services.

## 42. Chronicle.Desktop

Responsibilities:

- composition root;
- application startup;
- dependency injection;
- process lifetime;
- configuration bootstrap;
- desktop window host;
- Safe Mode composition;
- release-channel composition;
- logging initialization;
- migration and startup orchestration.

## 43. Desktop Host Dependency

`Chronicle.Desktop` may reference all concrete implementation projects needed for composition.

## 44. No Business Logic in Host

The host composes.

It does not own Domain or Application decisions.

## 45. Official Rule Set Location

Official Rule Set implementations live under:

```text
rule-sets/
```

## 46. Rule Set Repository Layout

Recommended:

```text
rule-sets/
├── Chronicle.RuleSets.Werewolf/
├── Chronicle.RuleSets.Werewolf.Tests/
├── schemas/
├── packages/
└── README.md
```

## 47. Werewolf Package

`Chronicle.RuleSets.Werewolf` is the first official package implementation.

## 48. No Core Dependency on Werewolf

No production project under `src/` references the Werewolf project directly except the desktop composition root if required to bundle or register the first official package.

## 49. Preferred Package Discovery

The long-term preference is package discovery through package descriptors and loading contracts rather than a hardcoded implementation dependency.

## 50. MVP Bundling

The desktop host may bundle the official Werewolf package for first-run convenience.

That does not make Werewolf part of Core.

## 51. Rules-Only Source Material

Rules-only extracted source material does not belong in Chronicle Core source projects.

## 52. Rule Knowledge Location

Recommended private or provenance-controlled working area:

```text
rule-sets/Chronicle.RuleSets.Werewolf/knowledge/
```

or an external noncommitted source directory.

## 53. No Generic RuleKnowledge Project

The MVP does not create:

```text
Chronicle.RuleKnowledge
```

as a production project.

## 54. Why No RuleKnowledge Project

Rules knowledge is package-specific content and build input, not a universal Core runtime responsibility.

## 55. Package Knowledge Artifacts

Generated package artifacts may include:

- indexed rules data;
- schemas;
- operation definitions;
- validation fixtures;
- citations to permitted source references;
- package-owned RAG data.

## 56. Provenance Requirement

Every committed knowledge artifact must have:

- source classification;
- license status;
- transformation provenance;
- redistribution permission;
- reviewer.

## 57. Unauthorized Sourcebook Content

Unauthorized sourcebook text or assets must not be committed.

## 58. Test Root

Tests live under:

```text
tests/
```

## 59. Canonical Test Projects

Recommended:

```text
tests/
├── Chronicle.Domain.Tests/
├── Chronicle.Application.Tests/
├── Chronicle.Architecture.Tests/
├── Chronicle.Contracts.Tests/
├── Chronicle.Infrastructure.Tests/
├── Chronicle.Persistence.Sqlite.Tests/
├── Chronicle.NarrativeIntelligence.ContractTests/
├── Chronicle.NarrativeIntelligence.OpenAI.Tests/
├── Chronicle.Presentation.Desktop.Tests/
├── Chronicle.Desktop.IntegrationTests/
└── Chronicle.EndToEnd.Tests/
```

## 60. Rule Set Tests

Rule Set implementation tests remain beside the Rule Set area:

```text
rule-sets/Chronicle.RuleSets.Werewolf.Tests/
```

or under a mirrored `tests/rule-sets/` layout if tooling requires it.

One convention must be selected and used consistently.

## 61. Recommended Rule Set Test Convention

Keep package implementation tests next to `rule-sets/` to preserve package autonomy.

## 62. Domain Tests

Cover:

- aggregate invariants;
- value objects;
- history rules;
- correction behavior;
- system-neutral mechanics.

## 63. Application Tests

Cover:

- use cases;
- Operation lifecycle;
- Work Items;
- Narrative Turns;
- event routing;
- recovery;
- idempotency.

## 64. Architecture Tests

Enforce dependency direction and forbidden references.

## 65. Contract Tests

Validate:

- versioned serialization;
- unknown-field behavior;
- event schemas;
- portable DTOs;
- provider-neutral outputs.

## 66. Persistence Tests

Use real SQLite behavior for:

- migrations;
- constraints;
- transactions;
- WAL;
- backup;
- recovery;
- Dice evidence.

## 67. Provider Contract Tests

Every provider adapter must pass one shared provider-neutral suite.

## 68. Desktop Integration Tests

Cover:

- composition;
- startup;
- migration;
- Safe Mode;
- dependency registration;
- release-channel behavior.

## 69. End-to-End Tests

Cover representative vertical slices using real persistence and fake external providers.

## 70. No Test Project Per Class

Project count follows architectural boundaries, not arbitrary file grouping.

## 71. Test Doubles

Reusable test doubles may live under:

```text
tests/Chronicle.Testing/
```

only if several test projects need them.

## 72. Tools Root

Development and release tools live under:

```text
tools/
```

## 73. Tool Examples

```text
tools/
├── Chronicle.Tools.ContractValidator/
├── Chronicle.Tools.PackageValidator/
├── Chronicle.Tools.MigrationInspector/
├── Chronicle.Tools.BackupInspector/
└── Chronicle.Tools.ProvenanceScanner/
```

## 74. Tool Dependency Rule

Tools may reference public or internal tooling contracts.

They must not become runtime dependencies of Domain or Application.

## 75. Build Root

Build and packaging scripts live under:

```text
build/
```

## 76. Build Contents

Examples:

- Inno Setup scripts;
- release scripts;
- signing orchestration;
- hash generation;
- SBOM generation;
- notice generation;
- artifact scanning.

## 77. Build Prohibitions

Signing credentials and secrets are never committed.

## 78. Documentation Root

Technical documentation lives under:

```text
docs/
```

## 79. Documentation Layout

Recommended:

```text
docs/
├── adr/
├── rfc/
├── architecture/
├── rule-sets/
├── security/
├── operations/
├── contributing/
├── diagrams/
├── DOCUMENTATION-INDEX.md
└── DOCUMENTATION-ROADMAP.md
```

## 80. Werewolf Profile Location

The official MVP profile belongs at:

```text
docs/rule-sets/werewolf/WEREWOLF-MVP-PROFILE.md
```

## 81. Samples Root

Samples live under:

```text
samples/
```

## 82. Sample Purpose

Samples demonstrate contracts and package development without becoming production dependencies.

## 83. Artifacts Root

Generated local artifacts may use:

```text
artifacts/
```

## 84. Artifact Git Policy

Build outputs, packages, temporary databases, and generated archives are ignored unless intentionally published as fixtures.

## 85. Namespace Convention

Namespaces mirror projects.

Examples:

```text
Chronicle.Domain
Chronicle.Application
Chronicle.Persistence.Sqlite
Chronicle.NarrativeIntelligence.OpenAI
Chronicle.Presentation.Desktop
```

## 86. Folder Convention Inside Projects

Organize primarily by feature or bounded responsibility, not only by technical type.

## 87. Domain Internal Layout

Recommended:

```text
Campaigns/
Characters/
Memories/
Knowledge/
Progression/
Dice/
Common/
```

Only system-neutral Dice concepts belong in Domain.

## 88. Application Internal Layout

Recommended:

```text
Campaigns/
Sessions/
Scenes/
NarrativeTurns/
NarrativeEvents/
Dice/
Operations/
WorkItems/
BackupRestore/
Packages/
Configuration/
Common/
```

## 89. Persistence Internal Layout

Recommended:

```text
Db/
Mappings/
Migrations/
Repositories/
Queries/
Operations/
WorkItems/
Narrative/
Dice/
BackupRestore/
```

## 90. No Layer Folder Duplication

A project should not recreate the entire solution architecture as internal folders.

## 91. Dependency Direction

Canonical production dependency direction:

```text
Domain
    ← Application
    ← Infrastructure implementations
    ← Presentation and Desktop composition
```

with shared abstraction projects referenced only where explicitly allowed.

## 92. Detailed Dependency Matrix

```text
Chronicle.Domain
    references no Chronicle production project

Chronicle.Contracts
    references minimal shared primitives only

Chronicle.RuleSets.Abstractions
    references Contracts and approved Domain abstractions only

Chronicle.NarrativeIntelligence.Abstractions
    references Contracts and approved shared primitives only

Chronicle.Application
    references Domain, Contracts, RuleSets.Abstractions,
    NarrativeIntelligence.Abstractions

Chronicle.Infrastructure
    references Application and abstraction projects

Chronicle.Persistence.Sqlite
    references Application, Domain, Contracts,
    RuleSets.Abstractions

Chronicle.NarrativeIntelligence.OpenAI
    references NarrativeIntelligence.Abstractions,
    Contracts, and approved Application ports

Chronicle.Presentation.Desktop
    references Application and Contracts

Chronicle.Desktop
    references required concrete projects for composition
```

## 93. Circular Dependencies

Circular project references are prohibited.

## 94. Friend Assemblies

`InternalsVisibleTo` is limited to narrowly justified test projects.

## 95. Public API Discipline

Public types are minimized.

## 96. Package References

Central package version management uses:

```text
Directory.Packages.props
```

## 97. Shared Build Settings

Use:

```text
Directory.Build.props
Directory.Build.targets
```

for common settings.

## 98. Nullable and Warnings

Production projects enable nullable reference types.

Warnings required by policy are treated as errors in CI.

## 99. Analyzer Policy

Use analyzers for:

- code quality;
- architecture;
- security;
- async correctness;
- API compatibility where useful.

## 100. Formatting

`.editorconfig` is authoritative.

## 101. Language

Project names, folder names, namespaces, contract keys, code comments, and technical documentation use English.

## 102. Localization Resources

User-facing localization resources belong in Presentation or package-specific resource areas.

## 103. No Localized Code Identifiers

Code identifiers remain English.

## 104. Solution Folders

The solution may use logical folders:

```text
Core
Infrastructure
Presentation
Hosts
RuleSets
Tests
Tools
```

Solution folders do not alter project dependencies.

## 105. Composition Root

Only `Chronicle.Desktop` composes:

- SQLite persistence;
- provider adapter;
- official Rule Set registration;
- desktop Presentation;
- Windows platform services;
- logging;
- configuration.

## 106. Test Composition

Integration and end-to-end tests may provide alternate composition roots.

## 107. No Service Locator

Dependency resolution occurs through constructor injection and explicit factories.

## 108. Plugin Boundary

Future installable Rule Sets use declared package contracts.

## 109. No Arbitrary Assembly Scanning by Default

The MVP may register bundled packages explicitly.

Dynamic discovery must be bounded and security-reviewed.

## 110. Provider Plugin Boundary

Future providers follow the Narrative Intelligence abstraction.

The first official provider adapter does not become the abstraction.

## 111. Database Migration Location

EF Core migrations live in:

```text
Chronicle.Persistence.Sqlite
```

## 112. Migration Execution

The desktop host invokes migration workflows through Application and Persistence services.

## 113. No Migration Project Split in MVP

A separate migrations project is not required unless tooling proves it necessary.

## 114. Installer Location

Inno Setup definitions live under:

```text
build/installer/windows/
```

## 115. No Installer Project Reference

Production .NET projects do not reference installer artifacts.

## 116. Open-Source Licensing

Project and source headers follow ADR-0044.

## 117. SPDX

Source files and generated artifacts use SPDX identifiers where policy requires them.

## 118. Third-Party Notices

Third-party notices are generated into release artifacts.

## 119. Rule Set Licensing

Rule Set package code and content preserve separate provenance and license metadata.

## 120. CI Build Graph

CI builds:

1. restore;
2. compile production projects;
3. run architecture tests;
4. run unit tests;
5. run contract tests;
6. run SQLite integration tests;
7. run provider adapter tests;
8. run Rule Set tests;
9. run desktop integration tests;
10. run end-to-end tests;
11. run provenance and secret scans;
12. build package artifacts;
13. build installer for release workflows.

## 121. Fast Local Loop

Developers may run filtered test groups locally.

## 122. No Hidden Generated Source Dependency

Generated source required for compilation must be reproducible from committed definitions or committed according to policy.

## 123. Code Generation

Contract or schema generation tools must have:

- deterministic output;
- versioned input;
- CI verification;
- no secret dependency.

## 124. Rule Knowledge Build

Package knowledge generation remains separate from the Core compilation path.

## 125. Missing Knowledge Source

Core must still compile without private source material.

## 126. Official Package Build

The official package may require permitted derived artifacts committed with provenance or generated in an approved secure workflow.

## 127. Package Test Kit

Shared Rule Set contract tests may live in:

```text
tests/Chronicle.RuleSets.ContractTests/
```

or a testing library under `tests/Chronicle.Testing`.

## 128. Architecture Enforcement

Use automated architecture tests to enforce project-reference rules.

## 129. Forbidden Reference Examples

The test suite must reject:

- Domain → Application;
- Domain → EF Core;
- Application → Persistence.Sqlite;
- Application → OpenAI SDK;
- Application → Presentation.Desktop;
- Persistence.Sqlite → Presentation.Desktop;
- OpenAI adapter → Persistence.Sqlite;
- Presentation.Desktop → Persistence.Sqlite;
- Core projects → Werewolf package implementation;
- Rule Set package → `ChronicleDbContext`;
- provider adapter → `ChronicleDbContext`.

## 130. Namespace Enforcement

Tests may verify that persistence namespaces do not appear in Domain or Application source.

## 131. Package Dependency Enforcement

Rule Set implementations may depend on:

```text
Chronicle.RuleSets.Abstractions
Chronicle.Contracts
approved Domain primitives
```

They must not depend on Presentation or Persistence.

## 132. Presentation Rule Set Use

Presentation receives Rule Set-defined display contracts through Application read models.

It does not inspect package implementation internals.

## 133. Provider Rule Set Use

Provider adapters do not interpret Rule Set mechanics.

## 134. Persistence Rule Set Use

Persistence stores package-defined payloads through approved versioned contracts.

It does not encode package semantics in columns.

## 135. Error Model

Repository governance errors may use:

```text
architecture.project-reference-forbidden
architecture.circular-dependency
architecture.namespace-violation
architecture.public-api-leak
architecture.provider-sdk-leak
architecture.persistence-leak
architecture.presentation-leak
architecture.rule-set-specific-core-leak
architecture.missing-contract-version
architecture.unapproved-project
architecture.duplicate-responsibility
architecture.provenance-missing
```

## 136. Documentation Requirement

Every production project includes a short README describing:

- responsibility;
- allowed dependencies;
- forbidden dependencies;
- public extension points;
- test project.

## 137. New Project Approval

A new production project requires justification covering:

- distinct responsibility;
- dependency placement;
- why an existing project is insufficient;
- ownership;
- tests;
- deployment impact.

## 138. No Project for Every Namespace

Projects represent deployment or architectural boundaries, not arbitrary organization.

## 139. Project Removal

Unused or placeholder production projects should be removed rather than preserved for imagined future needs.

## 140. MVP Project Count

The MVP should remain near the canonical set defined in this ADR.

## 141. Optional Project Extraction

A project may be extracted later when:

- independent deployment or packaging exists;
- dependency isolation materially improves;
- test isolation is valuable;
- responsibility has sufficient size and stability.

## 142. No Premature Micro-Libraries

Small abstractions stay with their owning boundary until a real reuse case appears.

## 143. Migration from Previous Topology

The repository migration should:

1. inventory current projects;
2. map each project to one canonical responsibility;
3. rename `Persistence` to `Persistence.Sqlite` where needed;
4. merge or remove placeholder `RuleSet.Runtime`;
5. move system-specific knowledge out of Core;
6. establish `Presentation.Desktop`;
7. establish `Chronicle.Desktop` composition root;
8. align test projects;
9. update namespaces;
10. update solution references;
11. add architecture tests;
12. update documentation links.

## 144. No Big-Bang Requirement

Topology migration may occur incrementally if every intermediate step preserves a buildable solution and dependency direction.

## 145. Compatibility Shims

Temporary namespace or project shims are permitted only with:

- explicit removal issue;
- bounded lifetime;
- no new references;
- CI warning.

## 146. Technology Spike

Before acceptance, implement:

1. canonical folder tree;
2. `Chronicle.sln`;
3. central package management;
4. Domain project;
5. Application project;
6. Contracts project;
7. RuleSets.Abstractions project;
8. NarrativeIntelligence.Abstractions project;
9. Infrastructure project;
10. Persistence.Sqlite project;
11. OpenAI adapter project;
12. Presentation.Desktop project;
13. Desktop host project;
14. Werewolf package area;
15. canonical test projects;
16. architecture tests;
17. project READMEs;
18. namespace migration;
19. CI build graph;
20. provenance checks.

## 147. Spike Acceptance

The spike passes when:

- the full solution builds;
- no circular project reference exists;
- Domain references no infrastructure package;
- Application references no concrete persistence or provider SDK;
- Persistence.Sqlite contains EF Core and migrations;
- OpenAI adapter contains the provider SDK;
- Presentation contains desktop UI dependencies;
- Desktop host is the only composition root;
- Werewolf builds as a package outside Core;
- no Core project references Werewolf mechanics;
- no generic RuleKnowledge production project exists;
- no placeholder RuleSet.Runtime project is required;
- architecture tests fail on intentionally introduced forbidden references;
- all test groups run in CI.

## 148. Definition of Compliance

An implementation complies when:

- ADR-0002 remains the architecture authority;
- this ADR defines one noncompeting topology;
- production projects use the canonical responsibilities;
- concrete persistence is named `Chronicle.Persistence.Sqlite`;
- desktop Presentation is explicit;
- desktop host is the composition root;
- Narrative Intelligence abstractions and OpenAI implementation are separate;
- Rule Set abstractions and Werewolf implementation are separate;
- no Core project depends on Werewolf;
- no mandatory generic RuleSet.Runtime exists;
- no generic RuleKnowledge production project exists;
- tests mirror architectural boundaries;
- architecture tests enforce dependency direction;
- all technical identifiers are English.

## 149. Review Triggers

Review this ADR if:

- a second official desktop platform is added;
- a server host is introduced;
- Rule Sets become separately installed binary plugins;
- provider adapters become separately deployable;
- a second persistence implementation is introduced;
- a dedicated shared Rule Set runtime becomes justified;
- package knowledge becomes an independently deployed service;
- the repository becomes a multi-repository organization;
- mobile or web clients become official.

## 150. Deferred Decisions

Later decisions may define:

- server solution projects;
- mobile Presentation;
- web Presentation;
- dedicated plugin loader;
- signed package runtime;
- second persistence provider;
- shared Rule Set runtime extraction;
- multi-repository split;
- package SDK distribution;
- external contributor templates.

## 151. Final Decision

Chronicle will have one repository topology that reflects the architecture already selected in ADR-0002.

Core will remain small and system-neutral.

Persistence will be concrete and visible.

Presentation will be explicit.

Providers will remain adapters.

Rule Sets will remain packages.

Werewolf will be the first official package, not a hidden dependency of the framework.

The repository will make these boundaries obvious before a developer reads a single line of implementation code.
