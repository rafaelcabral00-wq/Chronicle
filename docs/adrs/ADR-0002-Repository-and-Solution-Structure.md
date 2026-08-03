---
id: ADR-0002
title: Repository and Solution Structure
status: Proposed
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-08-03
category: Technology
supersedes: []
superseded_by: null
depends_on:
  - ADR-0001
  - RFC-0005
  - RFC-0016
  - RFC-0017
  - RFC-0019
  - RFC-0020
  - RFC-0027
  - RFC-0033
  - RFC-0038
  - RFC-0040
  - RFC-0041
  - RFC-0042
related_to:
  - ADR-0043
---

> **"The repository should make the architecture visible before a contributor reads a single implementation file."**

# Repository and Solution Structure

## 1. Status

**Proposed**

This ADR defines the initial Git repository, .NET solution, project boundaries, dependency direction, test organization, documentation layout, tooling layout, naming conventions, and contribution constraints for Chronicle.

Decision-resolution note, effective 2026-08-03:

- this ADR remains authoritative for repository and solution architecture semantics, dependency direction, and boundary principles;
- ADR-0043 v0.2.0 is authoritative for the concrete current repository topology, project boundaries, paths, project names, and dependency layout;
- where this ADR's concrete topology examples conflict with ADR-0043 v0.2.0, ADR-0043 prevails;
- this is a partial supersession of concrete materialization details only, not a supersession of this ADR's architectural semantics.

The decision becomes **Accepted** after:

- the initial solution compiles;
- dependency-direction tests pass;
- the first Domain, Application, Persistence, Rule Set, and Desktop vertical slice can be placed without circular references;
- the build and test commands work from a clean checkout;
- a new contributor can identify where a change belongs without relying on hidden conventions.

## 2. Context

Chronicle is both:

- an open-source framework;
- an official desktop application;
- a host for approved Rule Set packages;
- a persistence-heavy local application;
- a provider-neutral Narrative Intelligence system.

The repository must make these distinctions explicit.

A weak structure could cause:

- UI code to depend directly on persistence;
- Domain code to reference Avalonia or EF Core;
- Rule Set packages to access infrastructure;
- provider SDK types to leak into contracts;
- circular project references;
- duplicated test fixtures;
- migrations scattered across unrelated projects;
- documentation separated from implementation decisions;
- excessive project fragmentation;
- contributors placing code by technical type rather than business capability.

This ADR defines a structure that is strict enough to preserve boundaries and simple enough to support the MVP.

## 3. Decision Drivers

The structure prioritizes:

1. visible architecture;
2. compile-time dependency enforcement;
3. modular-monolith delivery;
4. low onboarding cost;
5. testability;
6. isolated infrastructure;
7. provider and Rule Set neutrality;
8. predictable documentation;
9. manageable project count;
10. future package extraction without premature distribution.

## 4. Decision Summary

Chronicle will use one primary Git repository and one primary .NET solution.

Concrete materialization details, including current folder paths, project names, solution organization, test placement, and dependency layout, defer to ADR-0043 v0.2.0 where ADR-0043 is more specific or conflicts with the examples below.

The repository will be organized into:

```text
/docs
/src
/tests
/tools
/build
/samples
```

The initial solution will contain a small set of architecture-level projects:

```text
Chronicle.Domain
Chronicle.Application
Chronicle.Contracts
Chronicle.Infrastructure
Chronicle.Persistence.Sqlite
Chronicle.NarrativeIntelligence
Chronicle.RuleKnowledge
Chronicle.RuleSets.Abstractions
Chronicle.RuleSets.Werewolf
Chronicle.Desktop
Chronicle.Testing
```

Test projects will be separated by test purpose rather than mirroring every production project mechanically.

The structure MUST preserve dependency direction through project references, architecture tests, and code-review rules.

## 5. Repository Model

### Decision

Use a **monorepo** for the framework, official desktop application, official Rule Set package, documentation, tests, and development tools.

### Rationale

A monorepo provides:

- atomic contract changes;
- synchronized migrations;
- shared test fixtures;
- one versioned documentation history;
- simpler CI;
- easier refactoring before 1.0;
- consistent quality gates;
- one contributor workflow.

### Constraints

The monorepo MUST not become a justification for unrestricted dependencies.

Logical package boundaries remain explicit.

## 6. Top-Level Repository Layout

```text
Chronicle/
├── .config/
├── .github/
├── build/
├── docs/
├── samples/
├── src/
├── tests/
├── tools/
├── .editorconfig
├── .gitignore
├── Directory.Build.props
├── Directory.Build.targets
├── Directory.Packages.props
├── global.json
├── LICENSE
├── README.md
├── SECURITY.md
├── CONTRIBUTING.md
├── CHANGELOG.md
└── Chronicle.slnx
```

A traditional `.sln` file MAY be used if tool support requires it.

## 7. `/docs`

Documentation is versioned beside the code.

Recommended structure:

```text
docs/
├── README.md
├── roadmap/
│   └── DOCUMENTATION-ROADMAP.md
├── rfcs/
│   ├── RFC-0000-Project-Charter.md
│   └── ...
├── adrs/
│   ├── ADR-0001-Technology-Stack-and-Framework-Selection.md
│   └── ...
├── guides/
├── architecture/
├── development/
├── operations/
└── assets/
```

## 8. Documentation Rules

- RFCs define product, Domain, or architectural contracts.
- ADRs record concrete technology and implementation decisions.
- Guides explain usage and contribution.
- Diagrams SHOULD be stored as source where possible.
- Generated documentation MUST be reproducible.
- Documentation references stable RFC and ADR identifiers.
- Superseded decisions remain in history.

## 9. `/src`

Production code belongs in `/src`.

Recommended initial structure:

```text
src/
├── Chronicle.Domain/
├── Chronicle.Application/
├── Chronicle.Contracts/
├── Chronicle.Infrastructure/
├── Chronicle.Persistence.Sqlite/
├── Chronicle.NarrativeIntelligence/
├── Chronicle.RuleKnowledge/
├── Chronicle.RuleSets.Abstractions/
├── Chronicle.RuleSets.Werewolf/
├── Chronicle.Desktop/
└── Chronicle.Testing/
```

The project count MAY be reduced if two adjacent projects have no meaningful independent boundary.

## 10. `Chronicle.Domain`

`Chronicle.Domain` owns:

- Campaign entities;
- Character entities and value objects;
- Session, Act, and Scene lifecycle;
- Memories;
- Relationships;
- Character Knowledge;
- Secrets;
- progression state;
- Preference selection state;
- Domain Events;
- invariant errors;
- pure Domain services.

It MUST NOT reference:

- Application;
- Contracts transport DTOs;
- Infrastructure;
- EF Core;
- Avalonia;
- provider SDKs;
- filesystem;
- HTTP;
- logging implementations.

## 11. `Chronicle.Application`

`Chronicle.Application` owns:

- commands;
- queries;
- use-case handlers;
- transaction orchestration;
- idempotency coordination;
- background-work orchestration;
- authorization context;
- visibility orchestration;
- Domain-to-query projection;
- ports required from Infrastructure;
- recovery decisions.

It MAY reference:

```text
Chronicle.Domain
Chronicle.Contracts
Chronicle.RuleSets.Abstractions
```

It MUST NOT reference:

- Chronicle.Desktop;
- Chronicle.Persistence.Sqlite;
- provider-specific SDKs;
- Avalonia;
- EF Core;
- operating-system APIs.

## 12. `Chronicle.Contracts`

`Chronicle.Contracts` owns stable provider-neutral and host-neutral contracts.

Examples:

- Narrative Intelligence request and response DTOs;
- structured output DTOs;
- Operation Record contracts;
- background Work Item contracts;
- export and import manifests;
- diagnostic DTOs;
- shared version identifiers;
- integration error contracts.

It MUST remain dependency-light.

It SHOULD avoid depending on Domain entities directly.

## 13. Domain Versus Contract Types

A Domain type represents Chronicle meaning.

A Contract type represents a boundary.

The two MUST not be merged merely because their fields look similar.

Mapping is explicit.

## 14. `Chronicle.Infrastructure`

`Chronicle.Infrastructure` owns cross-cutting infrastructure adapters not specific to SQLite or one provider.

Examples:

- configuration;
- clock;
- identifier generation;
- random generation;
- filesystem abstraction;
- backup orchestration;
- import/export infrastructure;
- logging adapters;
- platform service abstractions;
- provider-adapter support.

It MAY reference:

```text
Chronicle.Application
Chronicle.Contracts
Chronicle.Domain
Chronicle.RuleSets.Abstractions
```

It MUST NOT be referenced by Domain or Application.

## 15. `Chronicle.Persistence.Sqlite`

This project owns:

- EF Core DbContext;
- entity mappings;
- SQLite migrations;
- repositories;
- Unit of Work implementation;
- read models;
- integrity queries;
- backup integration specific to SQLite;
- persistence contract tests.

It MAY reference:

```text
Chronicle.Application
Chronicle.Domain
Chronicle.Contracts
```

No other project may use its internal EF Core types directly.

## 16. `Chronicle.NarrativeIntelligence`

This project owns:

- provider-neutral Narrative Intelligence orchestration support;
- prompt construction;
- context construction;
- structured-output parsing;
- output validation;
- repair coordination;
- provider adapter interfaces;
- one or more provider-specific subfolders or later projects.

### MVP Rule

The first provider adapter MAY live inside this project if separation remains clear.

If a second provider is added, provider-specific projects SHOULD be extracted:

```text
Chronicle.NarrativeIntelligence.OpenAI
Chronicle.NarrativeIntelligence.Anthropic
```

No provider SDK type may leave the provider-specific boundary.

## 17. `Chronicle.RuleKnowledge`

This project owns:

- Rule Knowledge source model;
- source provenance;
- lexical indexing;
- FTS queries;
- retrieval policies;
- citations;
- source transmission policy;
- index migrations;
- retrieval evaluation fixtures.

It MUST remain separate from Campaign Memories and Character Knowledge.

## 18. `Chronicle.RuleSets.Abstractions`

This project owns:

- Rule Set package interfaces;
- package manifest contracts;
- operation descriptors;
- Character schema contracts;
- progression contracts;
- Preference contracts;
- migration contracts;
- Rule Set validation errors.

It MUST NOT depend on the official Werewolf package.

## 19. `Chronicle.RuleSets.Werewolf`

This project owns the first official Rule Set package.

It contains:

- package manifest;
- Character schema;
- operation definitions;
- mechanics;
- progression;
- Preferences;
- migrations;
- original summaries or legal references;
- golden fixtures where package-local.

It MAY reference:

```text
Chronicle.RuleSets.Abstractions
Chronicle.Contracts
```

It MUST NOT reference:

- Chronicle.Persistence.Sqlite;
- Chronicle.Desktop;
- provider adapters;
- filesystem;
- network;
- dependency-injection container.

## 20. `Chronicle.Desktop`

This project owns:

- Avalonia application;
- Views;
- View Models;
- navigation;
- desktop Host composition;
- platform service implementations;
- resource dictionaries;
- localization resources;
- desktop-specific diagnostics UI;
- process startup and shutdown.

It MAY reference:

```text
Chronicle.Application
Chronicle.Contracts
Chronicle.Infrastructure
Chronicle.Persistence.Sqlite
Chronicle.NarrativeIntelligence
Chronicle.RuleKnowledge
Chronicle.RuleSets.Abstractions
Chronicle.RuleSets.Werewolf
```

This broad reference set is permitted only because the Desktop Host is the composition root.

Presentation classes MUST still consume Application contracts rather than Infrastructure directly.

## 21. Composition Root

The dependency-injection composition root lives in the Desktop Host.

Only composition code may know all implementation projects.

Business code MUST not resolve services dynamically.

## 22. `Chronicle.Testing`

This production-adjacent test-support library owns reusable deterministic test infrastructure:

- fake clock;
- deterministic random source;
- deterministic identifier source;
- scripted provider;
- fake credential store;
- fixture builders;
- temporary data directory;
- contract-test base classes;
- failure injection;
- semantic comparison helpers.

It MUST NOT be referenced by Release production projects.

## 23. `/tests`

Recommended structure:

```text
tests/
├── Chronicle.Tests.Domain/
├── Chronicle.Tests.Application/
├── Chronicle.Tests.Contracts/
├── Chronicle.Tests.Integration/
├── Chronicle.Tests.RuleSets.Werewolf/
├── Chronicle.Tests.Desktop/
├── Chronicle.Tests.Architecture/
├── Chronicle.Tests.Security/
└── Fixtures/
```

## 24. Test Project Philosophy

Test projects are organized by test purpose.

They do not need one test project per production project.

This avoids unnecessary fragmentation and encourages behavior-oriented tests.

## 25. `Chronicle.Tests.Domain`

Owns:

- pure invariant tests;
- lifecycle tests;
- value-object tests;
- Memory aging;
- Relationship directionality;
- Knowledge state;
- progression balance;
- Preference lifecycle.

## 26. `Chronicle.Tests.Application`

Owns:

- use-case tests;
- idempotency;
- retry behavior;
- stale-version handling;
- finalization orchestration;
- Roll workflow;
- recovery behavior.

It primarily uses deterministic adapters.

## 27. `Chronicle.Tests.Contracts`

Owns reusable conformance suites for:

- Rule Set packages;
- provider adapters;
- repositories;
- credential stores;
- Character schemas;
- export packages.

## 28. `Chronicle.Tests.Integration`

Owns tests using real Infrastructure, especially:

- SQLite;
- EF Core migrations;
- filesystem;
- backup and restore;
- import and export;
- process locks;
- hosted workers.

## 29. `Chronicle.Tests.RuleSets.Werewolf`

Owns package-specific tests:

- Character creation;
- operations;
- Dice interpretation;
- progression;
- Preferences;
- migrations;
- legal fixture boundaries.

## 30. `Chronicle.Tests.Desktop`

Owns:

- View Model tests;
- Avalonia component tests;
- headless UI tests;
- desktop Host tests;
- core interaction tests;
- accessibility smoke tests.

## 31. `Chronicle.Tests.Architecture`

Owns automated dependency rules.

It MUST verify at minimum:

- Domain references no higher layer;
- Application does not reference Desktop or concrete Infrastructure;
- Rule Set packages do not reference persistence or providers;
- Contracts remain free from UI dependencies;
- provider SDK namespaces do not appear outside adapters;
- EF Core namespaces do not appear outside persistence projects;
- Avalonia namespaces do not appear outside Desktop tests and Desktop code;
- test-support projects do not leak into production output.

## 32. `Chronicle.Tests.Security`

Owns:

- prompt-injection fixtures;
- Secret leakage tests;
- import attack tests;
- path traversal;
- archive bombs;
- credential redaction;
- cross-Campaign reference tests;
- package tampering;
- unsafe deserialization tests.

## 33. `/tests/Fixtures`

Shared fixtures SHOULD be grouped by purpose:

```text
tests/Fixtures/
├── Campaigns/
├── Characters/
├── RuleSets/
├── Providers/
├── Migrations/
├── Exports/
├── Backups/
├── Security/
└── UI/
```

Fixture files MUST be synthetic, original, or legally redistributable.

## 34. `/tools`

Development and maintenance tools belong in `/tools`.

Initial candidates:

```text
tools/
├── Chronicle.PackageValidator/
├── Chronicle.MigrationRunner/
├── Chronicle.FixtureBuilder/
├── Chronicle.IntegrityCheck/
└── Chronicle.ExportInspector/
```

Tools MUST use public or internal Application contracts.

They MUST not duplicate Domain logic.

## 35. `/build`

Build orchestration belongs in `/build`.

Possible contents:

```text
build/
├── scripts/
├── packaging/
├── signing/
├── manifests/
└── ci/
```

Build scripts MUST be deterministic and cross-platform where practical.

## 36. `/samples`

Samples are educational and nonauthoritative.

Possible contents:

```text
samples/
├── MinimalRuleSet/
├── ScriptedProvider/
├── CharacterSchema/
└── PortableCampaign/
```

Samples MUST not contain production credentials or restricted source content.

## 37. Namespace Convention

Namespaces SHOULD follow project and capability.

Examples:

```text
Chronicle.Domain.Campaigns
Chronicle.Domain.Characters
Chronicle.Application.Sessions
Chronicle.Application.Dice
Chronicle.Contracts.Narrative
Chronicle.Persistence.Sqlite.Campaigns
Chronicle.Desktop.Play
Chronicle.RuleSets.Werewolf.Characters
```

## 38. Feature Folder Convention

Inside projects, organize by capability first, then technical role.

Preferred:

```text
Chronicle.Application/
└── Sessions/
    ├── StartSession/
    ├── EndSession/
    └── FinalizeSession/
```

Avoid a project-wide layout such as:

```text
Controllers/
Services/
Models/
Helpers/
```

when it obscures Domain capability.

## 39. File Naming

Files SHOULD use the primary type or capability name.

Examples:

```text
FinalizeSessionCommand.cs
FinalizeSessionHandler.cs
SessionFinalizationResult.cs
CampaignConfiguration.cs
DiceRollEntityConfiguration.cs
```

Avoid generic names such as:

```text
Manager.cs
Helper.cs
Utils.cs
Common.cs
BaseService.cs
```

unless the abstraction has precise documented meaning.

## 40. Public API Surface

Projects SHOULD expose the smallest practical public API.

Use:

- `internal` by default;
- explicit public contracts;
- InternalsVisibleTo only for controlled tests where necessary;
- package-level documentation for public types.

## 41. Project Reference Rules

Allowed references:

```text
Chronicle.Domain
    → none

Chronicle.Contracts
    → minimal shared primitives only

Chronicle.RuleSets.Abstractions
    → Chronicle.Contracts

Chronicle.Application
    → Domain, Contracts, RuleSets.Abstractions

Chronicle.Infrastructure
    → Application, Domain, Contracts, RuleSets.Abstractions

Chronicle.Persistence.Sqlite
    → Application, Domain, Contracts

Chronicle.NarrativeIntelligence
    → Application, Contracts, Domain where required

Chronicle.RuleKnowledge
    → Application, Contracts

Chronicle.RuleSets.Werewolf
    → RuleSets.Abstractions, Contracts

Chronicle.Desktop
    → composition dependencies
```

Exact references SHOULD be validated by architecture tests.

## 42. Circular Dependencies

Circular project references are prohibited.

Circular capability dependencies inside one project SHOULD trigger boundary review.

## 43. Shared Kernel

Chronicle SHOULD avoid a large `Common` or `Shared` project.

A small shared kernel MAY contain only truly universal primitives, such as:

- strongly typed identifiers;
- version value objects;
- result primitives;
- pagination primitives.

Every addition requires review.

## 44. No Dumping-Ground Project

`Chronicle.Contracts` and `Chronicle.Infrastructure` MUST NOT become dumping grounds.

A type belongs there only when its boundary role is clear.

## 45. Internal Module Boundaries

Even inside one project, modules SHOULD use internal interfaces and explicit mapping.

This reduces the cost of later extraction.

## 46. Solution Filters

Solution filters MAY be provided for:

- Domain development;
- Desktop development;
- Rule Set development;
- Infrastructure;
- tests.

They are convenience tools, not independent build definitions.

## 47. Central Build Configuration

Use:

```text
Directory.Build.props
Directory.Build.targets
Directory.Packages.props
global.json
```

to centralize:

- target framework;
- nullable settings;
- warnings;
- analyzer configuration;
- package versions;
- deterministic build settings;
- source-link settings where used.

## 48. Target Framework

All production projects SHOULD target the same .NET target framework for MVP unless a strong reason exists.

Multi-targeting is deferred.

## 49. Warnings and Analyzers

The solution SHOULD enable:

- nullable reference types;
- deterministic builds;
- analyzers;
- selected warnings as errors;
- package lock enforcement;
- banned API checks.

## 50. Generated Code

Generated code MUST be placed in clearly identified locations or intermediate output.

Generated code SHOULD NOT be hand-edited.

The generation source and command MUST be versioned.

## 51. Migrations Location

SQLite and EF Core storage migrations belong only in:

```text
Chronicle.Persistence.Sqlite/Migrations
```

Rule Set and Character schema migrations belong in their package or contract-specific module.

Storage migrations and Domain migrations MUST remain distinct.

## 52. Localization Location

Desktop localization resources belong in `Chronicle.Desktop`.

Rule Set localization keys and resources belong in their Rule Set package.

Machine keys remain language-neutral.

## 53. Configuration Templates

Safe configuration templates MAY live in:

```text
src/Chronicle.Desktop/Configuration
samples/
docs/development/
```

They MUST contain credential aliases only, never secret values.

## 54. Secrets and Local Files

Local development secrets MUST remain outside the repository.

The repository MUST include:

- secret setup documentation;
- example aliases;
- ignored local settings paths;
- secret-scanning rules.

## 55. Branch Strategy

The repository SHOULD use a lightweight trunk-based model.

Recommended:

```text
main
short-lived feature branches
release tags
```

Long-lived environment branches are discouraged.

## 56. Commit Scope

Commits SHOULD be cohesive.

Cross-layer changes are acceptable when required by one vertical slice.

Artificially splitting one contract change across many broken commits is discouraged.

## 57. Pull Request Structure

A pull request SHOULD identify:

- capability;
- affected RFCs or ADRs;
- migration impact;
- security impact;
- test evidence;
- screenshots for UI changes;
- compatibility impact.

## 58. Ownership

CODEOWNERS or equivalent MAY be used for:

- Domain;
- security;
- persistence;
- Rule Set packages;
- desktop UI;
- build and release.

Ownership supports review, not siloing.

## 59. Versioning Inside Monorepo

The official application and framework MAY initially share one repository release version.

Rule Set packages retain their own package versions.

Independent package publication may be introduced later.

## 60. NuGet Packaging

Internal projects do not need to be published as NuGet packages for MVP.

Package extraction SHOULD occur only when:

- a real external consumer exists;
- versioning responsibility is clear;
- API stability is acceptable;
- test and release overhead is justified.

## 61. Repository Size Control

The repository SHOULD exclude:

- generated binaries;
- local databases;
- backups;
- exports;
- provider payloads;
- large sourcebook files;
- model weights;
- temporary indexes;
- credentials.

Large legitimate assets require explicit review.

## 62. Git LFS

Git LFS MAY be used later for legitimate large binary test assets.

It is not required for MVP.

## 63. Sample Data

Sample Campaigns SHOULD be small, synthetic, and versioned.

Large performance fixtures MAY be generated during tests rather than committed.

## 64. Architecture Enforcement

Architecture is enforced through:

- project references;
- `internal` access;
- architecture tests;
- banned namespaces;
- code review;
- CI;
- ADR updates.

Documentation alone is insufficient.

## 65. Initial Architecture Rules

CI MUST fail when:

- Domain references EF Core;
- Application references Avalonia;
- Rule Set package references persistence;
- provider SDK types appear in Contracts;
- Desktop View Models use DbContext or repositories;
- test-only code is included in Release output;
- credentials or private fixtures are committed.

## 66. Build Commands

The repository SHOULD support simple documented commands.

Conceptually:

```text
dotnet restore
dotnet build
dotnet test
dotnet run --project src/Chronicle.Desktop
```

Packaging and migration commands will be added through tools or build scripts.

## 67. Clean Checkout Requirement

A new contributor SHOULD be able to:

1. clone;
2. install the declared SDK;
3. restore;
4. build;
5. run deterministic tests;
6. launch the desktop shell without provider credentials in degraded mode.

## 68. Local Developer Experience

The repository SHOULD provide:

- SDK version pinning;
- editor configuration;
- launch profiles;
- sample local configuration;
- scripted provider;
- test Rule Set fixtures;
- seed Campaign option for development;
- clear credential setup.

## 69. Continuous Integration

CI SHOULD evaluate:

- formatting;
- analyzers;
- architecture tests;
- unit tests;
- contract tests;
- SQLite integration tests;
- security tests;
- desktop smoke tests;
- packaging checks when relevant.

## 70. Repository Security

The repository SHOULD include:

```text
SECURITY.md
dependency update policy
secret scanning
vulnerability reporting
release verification guidance
```

## 71. License Placement

The root contains Chronicle's license.

Third-party and package-specific notices belong in documented locations.

Bundled Rule Set packages MUST include appropriate legal metadata.

## 72. README Responsibilities

The root README SHOULD explain:

- what Chronicle is;
- current maturity;
- MVP boundary;
- build prerequisites;
- basic run command;
- documentation links;
- contribution links;
- license;
- security reporting.

It SHOULD not duplicate all RFC content.

## 73. Contribution Guide

`CONTRIBUTING.md` SHOULD explain:

- architecture;
- where code belongs;
- test expectations;
- RFC and ADR process;
- commit and PR expectations;
- restricted-content policy;
- provider credential policy.

## 74. Change Log

`CHANGELOG.md` SHOULD track user-visible and compatibility-relevant changes.

Detailed internal commit history remains in Git.

## 75. Risks

### Too Many Projects

Excessive project count can slow navigation and build.

Mitigation:

- begin with architecture-level projects;
- split only when a boundary has independent reason;
- merge empty abstractions.

### Too Few Projects

A single project would weaken compile-time boundaries.

Mitigation:

- preserve Domain, Application, Persistence, Rule Set, and Desktop separation at minimum.

### Monorepo Coupling

Atomic changes can encourage hidden coupling.

Mitigation:

- architecture tests;
- package contracts;
- explicit references;
- independent versions where meaningful.

### Contracts Dumping Ground

Mitigation:

- contract ownership review;
- namespaces by boundary;
- remove internal-only types.

## 76. Alternatives Considered

### Repository Per Component

Rejected for MVP because it would complicate atomic contract changes, CI, fixtures, and pre-1.0 refactoring.

### One Production Project

Rejected because it would make architecture primarily conventional rather than enforced.

### One Project Per Domain Aggregate

Rejected because it would create excessive fragmentation before aggregate boundaries are validated in code.

### Horizontal Technical Folders Only

Rejected because `Services`, `Models`, and `Helpers` obscure use-case and Domain ownership.

### Publish Every Internal Library

Rejected because package publication would add compatibility and release burden without current external consumers.

## 77. Consequences

### Positive

- architecture is visible;
- dependency direction is compile-time enforceable;
- one repository supports atomic evolution;
- Rule Set and provider boundaries are explicit;
- test infrastructure is reusable;
- docs and implementation remain synchronized;
- future extraction remains possible.

### Negative

- contributors must understand several projects;
- composition root references many implementations;
- mapping between Domain and boundary DTOs adds code;
- architecture tests require maintenance;
- monorepo CI may grow over time.

## 78. Migration From Initial Empty Folder

The first implementation sequence SHOULD be:

1. create repository metadata;
2. move generated RFC and ADR files into `/docs`;
3. create central build files;
4. create empty architecture-level projects;
5. add reference rules;
6. add architecture tests;
7. add one minimal Domain type;
8. add one Application command;
9. add SQLite persistence spike;
10. add Desktop shell;
11. add scripted provider;
12. add first Rule Set operation.

## 79. Definition of Compliance

The repository complies when:

- top-level structure is recognizable;
- Domain is dependency-free from Infrastructure and UI;
- Application depends on abstractions;
- Persistence and provider code remain adapters;
- official Rule Set package remains isolated;
- Desktop is the composition root;
- tests are organized by purpose;
- architecture rules are automated;
- docs, migrations, fixtures, and tools have explicit locations;
- a clean checkout can build and test without private files.

## 80. Deferred Decisions

Later ADRs will define:

- ADR-0003: Desktop UI Framework Implementation Conventions;
- ADR-0004: SQLite Schema, EF Core Mapping, and Migration Strategy;
- ADR-0005: First Narrative Intelligence Provider;
- ADR-0006: First Supported Operating System and Packaging Format;
- ADR-0007: Structured Logging Implementation;
- ADR-0008: Credential Store Implementations;
- ADR-0009: Rule Knowledge Index and Retrieval Implementation;
- ADR-0010: Identifier Strategy;
- ADR-0011: Installer and Manual Update Delivery;
- ADR-0012: CI and Release Pipeline.

## 81. Final Decision

Chronicle will use one monorepo and one primary .NET solution.

The repository will separate Domain, Application, Contracts, Infrastructure, SQLite persistence, Narrative Intelligence, Rule Knowledge, Rule Set abstractions, the official Werewolf package, Desktop Presentation, and reusable test support.

The structure will be enforced by project references, architecture tests, analyzers, and CI.

The repository should not merely contain Chronicle.

It should teach contributors how Chronicle is allowed to grow.
