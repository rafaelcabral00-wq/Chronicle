---
id: ADR-0001
title: Technology Stack and Framework Selection
status: Proposed
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-08-01
category: Technology
supersedes: []
superseded_by: null
depends_on:
  - RFC-0005
  - RFC-0016
  - RFC-0017
  - RFC-0019
  - RFC-0020
  - RFC-0027
  - RFC-0033
  - RFC-0035
  - RFC-0036
  - RFC-0037
  - RFC-0038
  - RFC-0039
  - RFC-0040
  - RFC-0041
  - RFC-0042
---

> **"Choose a stack that makes the first Chronicle straightforward to build without making the framework difficult to replace, test, or understand."**

# Technology Stack and Framework Selection

## 1. Status

**Proposed**

This ADR selects the initial implementation stack for Chronicle's official desktop application and shared framework libraries.

The decision becomes **Accepted** after:

- a minimal technical spike validates packaging on the first supported operating system;
- SQLite migration and crash-recovery tests pass;
- an Avalonia accessibility spike validates the core live-play workflow;
- one provider adapter and one Rule Set package prove the dependency boundaries;
- no blocking licensing or distribution issue is found.

## 2. Context

RFC-0000 through RFC-0043 define Chronicle as:

- an open-source framework;
- an official desktop-first application;
- a modular monolith for MVP;
- local-first;
- single-user;
- persistence-heavy;
- provider-neutral;
- Rule Set-neutral;
- deterministic for mechanics;
- recoverable after interrupted operations;
- strict about separation between UI, Application, Domain, Rule Sets, providers, and persistence.

The selected stack must support:

- a rich desktop UI;
- Windows, Linux, and macOS architecture;
- local transactional storage;
- schema migrations;
- background work;
- dependency injection;
- structured logging;
- asynchronous provider calls;
- deterministic testing;
- strong typing;
- straightforward packaging;
- long-term maintainability;
- open-source contribution.

The stack must not require:

- a browser runtime as the authoritative application host;
- a remote server;
- cloud persistence;
- microservices;
- event sourcing;
- dynamic untrusted plugins;
- provider-specific Domain code.

## 3. Decision Drivers

The decision prioritizes:

1. strong compile-time type safety;
2. mature asynchronous programming;
3. high-quality desktop tooling;
4. cross-platform desktop support;
5. reliable embedded persistence;
6. testability;
7. long-term support;
8. clear architectural boundaries;
9. low operational complexity;
10. open-source-friendly licensing;
11. contributor accessibility;
12. compatibility with local and remote Narrative Intelligence providers.

## 4. Decision Summary

Chronicle will use the following initial stack:

```text
Language and Runtime
    C# on .NET 10 LTS

Desktop UI
    Avalonia UI
    XAML
    MVVM with CommunityToolkit.Mvvm

Application Host
    Microsoft.Extensions.Hosting
    Microsoft.Extensions.DependencyInjection
    Microsoft.Extensions.Configuration
    Microsoft.Extensions.Logging

Architecture
    Modular Monolith
    Domain-Driven Design
    Ports and Adapters
    Explicit Application Commands and Queries
    No generic mediator requirement

Persistence
    SQLite
    Entity Framework Core SQLite provider
    Explicit repositories and Unit of Work
    Direct SQL permitted only behind Infrastructure interfaces for targeted reads

Serialization
    System.Text.Json
    Explicit versioned DTOs
    No polymorphic deserialization from untrusted type names

HTTP and Providers
    HttpClient through IHttpClientFactory
    Provider SDKs isolated inside provider adapters
    Provider-neutral contracts in Chronicle Core

Rule Knowledge
    SQLite FTS5 or equivalent deterministic lexical index for MVP
    Semantic vector retrieval deferred until validated by a later ADR

Background Work
    Durable Work Item records in SQLite
    In-process bounded worker using .NET hosted services

Observability
    Microsoft.Extensions.Logging abstractions
    Structured local logging adapter
    OpenTelemetry-compatible semantic naming without requiring remote exporters

Testing
    xUnit
    deterministic fake clock
    deterministic random generator
    scripted Narrative Intelligence provider
    real SQLite integration tests
    Avalonia headless/component tests
    bounded end-to-end desktop smoke tests

Build and Packaging
    .NET SDK build
    locked dependencies
    platform-specific packaging selected by later ADRs
```

## 5. Language and Runtime

### Decision

Use **C#** on **.NET 10 LTS**.

### Rationale

C# and .NET provide:

- strong static typing;
- mature asynchronous APIs;
- records and pattern matching;
- source generation;
- dependency-injection infrastructure;
- cross-platform runtime support;
- strong test tooling;
- mature SQLite integration;
- long-term support;
- a large contributor ecosystem.

.NET 10 is the current Long-Term Support line at the time of this ADR.

### Constraints

Domain and Application projects MUST target framework-neutral .NET libraries where practical.

They MUST NOT depend on:

- Avalonia;
- desktop operating-system APIs;
- provider SDKs;
- EF Core implementation details;
- file-dialog APIs;
- concrete logging sinks.

## 6. Desktop UI Framework

### Decision

Use **Avalonia UI** for the official desktop application.

### Rationale

Avalonia provides:

- cross-platform desktop support;
- XAML-based declarative UI;
- strong alignment with C# and .NET;
- desktop-native application architecture;
- support for Windows, Linux, and macOS;
- headless and component-testing paths;
- separation between view and application logic;
- a route to multi-window support later.

### Constraints

Avalonia is a Presentation technology only.

No Domain or Application contract may reference:

- Avalonia controls;
- Avalonia properties;
- dispatcher types;
- XAML resource identifiers;
- view-specific navigation objects.

### Rejected Alternative: Web UI Inside Desktop Shell

A browser-based shell was not selected for MVP because it would add:

- browser-runtime distribution;
- additional IPC boundaries;
- greater memory overhead;
- more complicated local-file and credential integration;
- a temptation to move Application behavior into client-side code.

This does not prohibit a future web client.

### Rejected Alternative: Platform-Specific Native UI

Separate native UIs would maximize platform integration but multiply implementation and test effort before the product is proven.

### Rejected Alternative: .NET MAUI

MAUI was not selected because Chronicle's first target is a desktop application, while Avalonia provides a more direct desktop-first model and stronger Linux positioning.

## 7. UI Architectural Pattern

### Decision

Use **MVVM** with **CommunityToolkit.Mvvm** for Presentation state and commands.

### Rationale

MVVM fits:

- XAML binding;
- view-model testing;
- separation from Domain entities;
- explicit local UI state;
- command and query boundaries;
- accessibility-oriented presentation.

### Constraints

View Models:

- consume Application query DTOs;
- submit Application commands;
- may own local draft state;
- may not use repositories;
- may not execute Rule Set mechanics;
- may not invoke providers directly;
- may not persist Campaign state.

### No Secondary Domain Model

View Models MUST NOT recreate Campaign invariants.

They may expose display-specific projections only.

## 8. Application Hosting

### Decision

Use the `Microsoft.Extensions` hosting stack.

The Host will compose:

- configuration;
- dependency injection;
- logging;
- hosted background services;
- provider adapters;
- Rule Set registry;
- persistence;
- health services.

### Rationale

This stack is:

- native to modern .NET;
- mature;
- testable;
- independent from a web server;
- appropriate for one desktop process;
- compatible with future worker-process separation.

### Constraint

Chronicle does not become an ASP.NET application merely because it uses the generic host.

No HTTP server is required for MVP.

## 9. Dependency Injection

### Decision

Use `Microsoft.Extensions.DependencyInjection`.

### Registration Rules

Each architectural module registers through a narrow composition extension.

Example conceptual modules:

```text
AddChronicleDomain()
AddChronicleApplication()
AddChroniclePersistence()
AddChronicleNarrativeIntelligence()
AddChronicleRuleSets()
AddChronicleDesktop()
```

### Constraints

- Domain code does not resolve services from a container.
- Service locator usage is prohibited.
- Dependencies are constructor-injected.
- Lifetimes are explicit.
- State-changing handlers are scoped to an Application operation.
- Singleton services must be thread-safe and must not hold mutable Campaign state.

## 10. Application Commands and Queries

### Decision

Use explicit command/query interfaces and handlers.

Do not require a generic mediator library in the initial stack.

### Rationale

Explicit handlers provide:

- transparent dependencies;
- simple debugging;
- no hidden pipeline behavior;
- no dependency on a library whose abstractions may become architectural gravity;
- easy unit testing.

A lightweight internal dispatcher MAY be created only if repeated use proves necessary.

### Command Example

```text
ProcessPlayerInputCommand
ExecuteDiceRollCommand
FinalizeSessionCommand
AdvanceCharacterCommand
ChangeCampaignPreferenceCommand
```

### Query Example

```text
GetCampaignListQuery
GetActiveSceneQuery
GetCharacterSheetQuery
GetMemoryTimelineQuery
GetPendingOperationsQuery
```

## 11. Persistence Engine

### Decision

Use **SQLite** as the authoritative local persistence engine.

### Rationale

SQLite provides:

- embedded deployment;
- no database server;
- ACID transactions;
- crash-resistant transactional behavior;
- mature tooling;
- one local database file or controlled set of files;
- strong fit for a single-user desktop application;
- backup and migration options;
- cross-platform availability.

### Constraints

The MVP supports one active application instance per data directory.

SQLite's single-writer model aligns with Chronicle's bounded Campaign mutation policy.

### Rejected Alternative: PostgreSQL

PostgreSQL is excellent for server-hosted multi-user systems but introduces unnecessary deployment and operational cost for a local MVP.

### Rejected Alternative: Document Database

Chronicle requires:

- transactions;
- uniqueness constraints;
- references;
- migrations;
- ordered history;
- precise version checks.

A relational embedded database better matches these needs.

### Rejected Alternative: Raw JSON Files

Raw files would require Chronicle to recreate:

- transactional commits;
- indexing;
- concurrent-write protection;
- schema migration;
- integrity constraints;
- query behavior.

## 12. Persistence Mapping

### Decision

Use **Entity Framework Core** with the official SQLite provider.

### Rationale

EF Core provides:

- change tracking;
- migrations;
- transaction integration;
- typed mapping;
- optimistic concurrency support;
- mature tooling;
- testable database contexts;
- compatibility with SQLite.

### Architectural Boundary

EF Core is an Infrastructure detail.

Domain entities MUST NOT inherit from EF base types or expose persistence-specific APIs.

### Mapping Rules

Prefer:

- explicit entity configurations;
- explicit keys;
- explicit indexes;
- explicit concurrency tokens;
- explicit value converters;
- explicit owned-value mappings where appropriate;
- migrations committed to source control.

Avoid:

- convention-only critical mappings;
- lazy loading;
- unrestricted navigation graphs;
- loading an entire Campaign for every operation;
- provider-specific types in Domain contracts.

### Direct SQL

Direct SQL MAY be used for:

- targeted read models;
- integrity checks;
- high-volume transcript queries;
- migration verification.

It must remain behind Infrastructure interfaces and have integration tests.

## 13. Database Organization

### Decision

Use one authoritative SQLite database per installation for MVP.

### Rationale

This simplifies:

- Campaign list queries;
- migrations;
- backup;
- Operation Records;
- one-instance locking;
- diagnostics.

### Future Review

One database per Campaign may be reconsidered if:

- portability;
- corruption isolation;
- very large libraries;
- selective synchronization;

provide evidence for the change.

## 14. Database Concurrency

### Decision

Use:

- optimistic concurrency;
- short transactions;
- one Campaign-mutating operation at a time;
- bounded global writes;
- WAL mode where validated;
- busy timeout and explicit retry classification.

### Constraint

No transaction may remain open during:

- provider calls;
- player decisions;
- file selection;
- long background computation.

## 15. Serialization

### Decision

Use **System.Text.Json** for:

- provider-neutral DTOs;
- Operation Records;
- portable structured artifacts where JSON is selected;
- diagnostic metadata;
- configuration where appropriate.

### Rules

- DTOs are explicit and versioned.
- Unknown fields follow contract-specific policy.
- Enum serialization is stable.
- Persistent keys are language-neutral.
- Reference handling is explicit.
- Arbitrary runtime type-name deserialization is prohibited.
- Provider payload limits are enforced before full processing where practical.

## 16. Narrative Intelligence Adapters

### Decision

Use `HttpClient` and `IHttpClientFactory` for remote provider communication.

Official provider SDKs MAY be used inside adapters when they reduce protocol risk.

### Constraints

Provider SDK types MUST NOT cross Infrastructure boundaries.

Every adapter translates between:

```text
Chronicle Provider-Neutral Contract
    ↕
Provider-Specific Request and Response
```

### Credential Rule

Adapters receive credentials at invocation time through a Secrets Manager.

Credentials do not enter Application commands, Domain objects, or persisted provider-neutral DTOs.

## 17. Rule Set Packages

### Decision

Official MVP Rule Set packages are normal .NET assemblies loaded through an approved registry.

### Constraints

Packages:

- implement Chronicle contracts;
- run in-process;
- are deterministic;
- perform no I/O;
- do not access DI container directly;
- do not access database;
- do not access network;
- do not generate authoritative randomness;
- do not create persistent identifiers.

Dynamic untrusted assemblies are prohibited in MVP.

## 18. Rule Knowledge Retrieval

### Decision

Begin with deterministic lexical retrieval backed by **SQLite FTS5** or an equivalent SQLite-supported full-text capability.

### Rationale

This provides:

- local operation;
- version filtering;
- source provenance;
- low deployment cost;
- deterministic testability;
- useful retrieval before a vector stack is justified.

### Deferred Decision

Embedding-based semantic retrieval is deferred.

A future ADR must select:

- embedding provider;
- local or remote execution;
- vector store;
- index migration;
- privacy policy;
- evaluation thresholds.

### Hybrid Readiness

The retrieval contract remains capable of combining:

```text
Exact Key Retrieval
Lexical Retrieval
Structured Metadata Filtering
Future Semantic Retrieval
```

## 19. Background Work

### Decision

Use .NET hosted services for the execution loop and SQLite-backed durable Work Items for recovery.

### Rules

The in-process worker:

- polls or receives local wake signals;
- claims bounded leases;
- uses OperationId;
- commits checkpoints;
- supports cancellation;
- resumes after restart;
- never relies on memory alone for critical work.

### Rejected Alternative: External Message Broker

A broker is unnecessary for one local desktop process and would violate MVP scope.

## 20. Randomness

### Decision

Use a dedicated Chronicle random-service interface.

The production implementation uses a cryptographically strong operating-system-backed random source when practical.

Tests use deterministic scripted values.

### Constraint

No UI, provider, or Rule Set package may bypass this service for authoritative Rolls.

## 21. Time

### Decision

Use a Chronicle clock abstraction backed by system UTC time.

### Rules

- Persist timestamps in UTC.
- Format in user locale at Presentation.
- Tests use a fake clock.
- Domain behavior does not call static system time directly.

## 22. Identifiers

### Decision

Use stable application-generated identifiers.

The concrete identifier format SHOULD be UUID/GUID-compatible for MVP.

### Rules

- Chronicle generates persistent IDs.
- Providers do not generate authoritative IDs.
- Tests may inject deterministic IDs.
- Portable clone import can remap identifiers.

A later ADR may select UUID version strategy.

## 23. Logging and Observability

### Decision

Use `Microsoft.Extensions.Logging` abstractions with one structured local logging implementation selected during the implementation spike.

### Requirements

The logging adapter must support:

- structured properties;
- local rolling files;
- OperationId and CorrelationId;
- safe redaction;
- bounded retention;
- multiple severity levels.

### OpenTelemetry Compatibility

Span and metric names SHOULD follow OpenTelemetry-compatible semantics.

The MVP does not require:

- remote collector;
- remote exporter;
- hosted dashboard.

## 24. Configuration

### Decision

Use `Microsoft.Extensions.Configuration` as the source-composition mechanism.

Configuration sources MAY include:

- built-in defaults;
- versioned local configuration;
- environment variables;
- command-line overrides for development;
- test overrides.

### Constraint

Secrets are references only.

They are loaded through platform credential-store adapters.

## 25. Secrets

### Decision

Define an `ISecretsManager` port.

Implement platform-specific secure storage adapters.

Target platforms SHOULD use their native credential facilities.

### Fallback

A plaintext production fallback is not accepted.

A clearly labeled development-only fallback MAY exist for local testing.

## 26. Testing Stack

### Decision

Use **xUnit** as the primary test framework.

Use test-support libraries only where they remain optional implementation details.

### Required Test Infrastructure

Chronicle will provide:

- fake clock;
- deterministic random source;
- deterministic identifier source;
- scripted provider;
- fake credential store;
- temporary isolated data directory;
- real SQLite fixture;
- package conformance harness;
- migration fixture runner;
- adversarial import fixtures.

### Assertions

Tests SHOULD favor readable explicit assertions.

A fluent assertion library MAY be adopted after license and maintenance review.

## 27. UI Testing

### Decision

Use:

- View Model unit tests;
- Avalonia component tests;
- Avalonia headless tests where supported;
- a bounded number of desktop process smoke tests.

### Scope

The test suite must prove:

- no direct persistence access;
- Roll interaction;
- operation status;
- finalization recovery;
- visibility filtering;
- keyboard operation;
- reduced motion;
- safe error states.

## 28. Code Organization

### Decision

Use a modular solution organized by architecture and capability.

Recommended initial projects:

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
Chronicle.Tests.Unit
Chronicle.Tests.Integration
Chronicle.Tests.Desktop
```

Exact project count may be simplified to avoid unnecessary fragmentation.

## 29. Module Dependency Direction

Allowed direction:

```text
Desktop
    → Application
    → Domain

Infrastructure
    → Application Contracts
    → Domain

Rule Set Package
    → Rule Set Abstractions
    → Shared Contracts
```

Prohibited direction:

```text
Domain → Infrastructure
Domain → Desktop
Application → Avalonia
Rule Set → Persistence
Provider Adapter → Desktop
```

## 30. Source Layout

Recommended repository layout:

```text
/docs
  /rfcs
  /adrs

/src
  /Chronicle.Domain
  /Chronicle.Application
  /Chronicle.Contracts
  /Chronicle.Infrastructure
  /Chronicle.Desktop
  /RuleSets

/tests
  /Unit
  /Integration
  /Contract
  /Desktop
  /Fixtures

/tools
  /PackageValidator
  /MigrationRunner
  /FixtureBuilder
```

## 31. Package Management

### Decision

Use NuGet with committed lockfiles or locked restore behavior for Release builds.

### Rules

- dependencies are pinned;
- transitive changes are reviewed;
- license metadata is tracked;
- prerelease packages require explicit approval;
- unnecessary dependencies are avoided.

## 32. Static Analysis and Formatting

### Decision

Use .NET analyzers and deterministic formatting.

The repository SHOULD enforce:

- nullable reference types;
- warnings as errors for selected categories;
- code style;
- async correctness;
- disposal correctness;
- banned API rules for critical boundaries.

## 33. Nullable Reference Types

Nullable reference types MUST be enabled.

Nullability is part of public contract design.

## 34. Async Rules

- External and I/O operations are asynchronous.
- Async methods accept cancellation where appropriate.
- `.Result` and `.Wait()` are prohibited in production flow.
- UI dispatch is isolated in Presentation.
- Domain computation remains synchronous unless genuine asynchronous work exists.

## 35. Error Representation

Application and contract layers use typed result and error models from RFC-0018.

Exceptions are reserved for:

- unexpected failures;
- infrastructure faults;
- invariant violations that indicate defects.

Expected validation failures are not modeled as generic exceptions.

## 36. Packaging

The concrete installer and updater technologies are deferred to dedicated ADRs.

The selected desktop framework and runtime must prove:

- self-contained build where appropriate;
- application and user-data separation;
- clean install;
- manual upgrade;
- platform-native credential access;
- release manifest generation.

## 37. Supported Platforms

### Initial Decision

The architecture targets:

```text
Windows
Linux
macOS
```

### Delivery Constraint

The first MVP release MAY support only one platform fully.

The first supported platform must be selected by a later delivery ADR.

Cross-platform contracts MUST not be weakened by first-platform priority.

## 38. Technology Spike Requirements

Before accepting this ADR, create a spike that demonstrates:

1. Avalonia application startup;
2. Application and Domain projects without Avalonia dependency;
3. SQLite creation and migration;
4. optimistic concurrency conflict;
5. durable Work Item recovery;
6. provider adapter timeout;
7. platform credential alias;
8. structured local logging;
9. one Rule Set operation;
10. one Chronicle-owned Dice Roll;
11. application restart preserving state;
12. packaging on the first target platform.

## 39. Technology Spike Acceptance

The spike passes when:

- the UI remains responsive during provider delay;
- Roll values persist before narration continuation;
- retry does not reroll;
- database migration is repeatable;
- credentials do not appear in configuration or logs;
- package boundaries are enforceable;
- a clean build can run on a second machine of the target platform.

## 40. Alternatives Considered

### TypeScript + Electron

Strengths:

- broad web ecosystem;
- strong cross-platform UI capability;
- rapid front-end development.

Reasons not selected:

- larger runtime footprint;
- browser and Node security boundaries;
- greater risk of mixing UI and Application authority;
- more complex native credential and process integration;
- duplicate type systems across possible backend boundaries.

### TypeScript + Tauri

Strengths:

- smaller desktop shell;
- web UI ecosystem;
- native host capabilities.

Reasons not selected:

- two-language stack for the initial team;
- more complex cross-boundary serialization;
- Rust host plus TypeScript UI increases contributor surface;
- Chronicle's Domain and persistence needs fit one strongly typed .NET stack well.

### Kotlin + Compose Desktop

Strengths:

- strong language;
- cross-platform UI;
- mature JVM ecosystem.

Reasons not selected:

- smaller project alignment with the selected architecture and tooling;
- less direct fit with the current expected contributor and desktop ecosystem;
- .NET provides a more unified path for the chosen persistence and host stack.

### Flutter

Strengths:

- strong cross-platform rendering;
- good UI consistency.

Reasons not selected:

- desktop ecosystem and native integration are not the primary strength relative to Avalonia for this project;
- Dart would create a less direct fit for the selected Domain and infrastructure stack.

### Python Desktop Stack

Strengths:

- rapid prototyping;
- strong AI ecosystem.

Reasons not selected:

- packaging complexity;
- weaker compile-time guarantees for large Domain contracts;
- less suitable long-term desktop architecture for this project's invariants.

## 41. Consequences

### Positive

- one primary language across UI, Application, Domain, Rule Sets, and Infrastructure;
- strong type safety;
- cross-platform desktop path;
- embedded transactional persistence;
- mature testing;
- low local deployment complexity;
- clear provider isolation;
- reusable framework libraries;
- straightforward deterministic Rule Set packages.

### Negative

- Avalonia-specific expertise is required;
- cross-platform visual differences still require testing;
- EF Core may require careful performance tuning;
- SQLite supports one writer at a time;
- native packaging remains platform-specific;
- local-model Python ecosystems require process or protocol integration rather than direct embedding;
- mobile and web clients will require separate Presentation implementations.

### Risks

- Avalonia accessibility behavior may vary by platform;
- one-process architecture may later need provider isolation;
- EF Core mapping could leak into Domain if discipline is weak;
- FTS5 may be insufficient for advanced retrieval;
- package retention could increase installation size;
- platform credential APIs may complicate portability.

## 42. Risk Mitigations

### Avalonia Risk

- perform accessibility spike;
- maintain Presentation-only dependency;
- use component tests;
- keep query and command contracts host-neutral.

### EF Core Risk

- explicit mappings;
- no lazy loading;
- repository boundaries;
- real SQLite integration tests;
- direct SQL for measured query hotspots.

### SQLite Risk

- short transactions;
- one mutating operation per Campaign;
- WAL validation;
- backup and integrity tests;
- future storage port remains explicit.

### FTS5 Risk

- keep retrieval abstraction provider-neutral;
- measure quality;
- introduce semantic retrieval only through a later ADR.

### One-Process Risk

- durable Work Items;
- strict error boundaries;
- provider timeouts;
- future optional worker-process interfaces.

## 43. Licensing Requirements

Before ADR acceptance, every selected dependency MUST be reviewed for:

- open-source license;
- redistribution;
- static or dynamic linking implications;
- notice requirements;
- commercial-use compatibility;
- maintenance health.

Chronicle MUST avoid introducing dependencies whose licensing conflicts with open-source distribution.

## 44. Version Policy

Release builds MUST pin exact dependency versions.

Major framework upgrades require:

- compatibility review;
- migration review;
- UI regression tests;
- package tests;
- release notes.

The stack selection is stable.

Individual package versions evolve through routine dependency ADRs or update records when impact is material.

## 45. Security Consequences

The stack must preserve:

- no provider SDK in Domain;
- no direct UI database access;
- no plaintext credentials;
- strict JSON parsing;
- safe file handling;
- package allowlisting;
- bounded background work;
- local logging redaction.

## 46. Performance Expectations

The selected stack must meet budgets later defined by performance ADRs.

Initial expectations:

- normal startup should feel desktop-native;
- Campaign list queries should be local and immediate;
- Message append and Roll commit should be fast relative to provider latency;
- long transcripts require virtualization;
- index build must not block UI;
- finalization persistence must remain bounded.

## 47. Migration Strategy

The initial database schema begins at version 1.

Every storage change requires:

- EF Core migration;
- migration fixture;
- SQLite integration test;
- backup compatibility review;
- rollback or safe-failure behavior.

Rule Set and Character schema migrations remain separate from EF Core storage migrations.

## 48. Observability Strategy

All components use stable OperationId and CorrelationId.

Technology-specific diagnostics remain in Infrastructure.

Core contracts refer only to provider-neutral diagnostic concepts.

## 49. Definition of Compliance

An implementation complies with this ADR when:

- it uses C# and .NET 10 LTS;
- Avalonia remains Presentation-only;
- MVVM View Models consume Application contracts;
- the Application Host uses `Microsoft.Extensions`;
- SQLite is the authoritative local store;
- EF Core remains an Infrastructure concern;
- provider SDKs remain inside adapters;
- Rule Set packages perform no I/O;
- durable Work Items survive restart;
- System.Text.Json contracts are explicit and bounded;
- tests control time, randomness, identifiers, and providers;
- dependencies are pinned;
- credentials remain outside ordinary configuration and Campaign data.

## 50. Deferred Decisions

The following require later ADRs:

- ADR-0002: Repository and Solution Structure;
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

## 51. Final Decision

Chronicle will begin as a modular .NET desktop application written in C#, using Avalonia UI for Presentation, SQLite and EF Core for local persistence, `Microsoft.Extensions` for hosting and dependency composition, explicit provider and Rule Set adapters, and deterministic .NET test infrastructure.

This stack is selected because it makes the official MVP practical while preserving Chronicle's most important architectural promise:

The first application may be desktop-based.

The framework beneath it remains independent, testable, and ready to grow only when growth is earned.
