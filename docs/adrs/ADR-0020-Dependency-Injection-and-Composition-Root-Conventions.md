---
id: ADR-0020
title: Dependency Injection and Composition Root Conventions
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
  - ADR-0005
  - ADR-0007
  - ADR-0008
  - ADR-0009
  - ADR-0010
  - ADR-0013
  - ADR-0014
  - ADR-0015
  - ADR-0016
  - ADR-0017
  - ADR-0018
  - ADR-0019
  - RFC-0005
  - RFC-0015
  - RFC-0016
  - RFC-0017
  - RFC-0019
  - RFC-0020
  - RFC-0035
  - RFC-0037
  - RFC-0038
  - RFC-0040
  - RFC-0042
---

> **"Dependencies should be assembled at the edge, expressed through contracts, and invisible to the Domain's decisions."**

# Dependency Injection and Composition Root Conventions

## 1. Status

**Proposed**

This ADR defines Chronicle's dependency-injection strategy, service-lifetime rules, registration conventions, startup validation, and composition-root ownership.

The decision is:

- use `Microsoft.Extensions.DependencyInjection` as the initial dependency-injection container;
- keep the Desktop host as the primary composition root;
- define module-owned registration extensions with explicit dependencies;
- prohibit service locator usage;
- prohibit dependency injection into Domain entities and value objects;
- prefer constructor injection;
- use Singleton, Scoped, and Transient lifetimes deliberately rather than by default convention;
- create explicit Application operation scopes;
- create separate Work Item execution scopes;
- keep EF Core `DbContext` scoped and short-lived;
- keep provider clients, clocks, immutable configuration snapshots, and safe stateless services singleton where appropriate;
- validate registrations and scope usage at startup and in tests;
- avoid uncontrolled assembly scanning for critical handlers and adapters;
- require one selected implementation for exclusive ports;
- use keyed or named registrations only where multiple implementations are a real product requirement;
- keep test composition explicit and replaceable;
- ensure Safe Mode can build a reduced service graph without requiring unavailable providers or damaged subsystems.

The decision becomes **Accepted** after a composition spike proves:

- normal Desktop startup;
- Safe Mode startup;
- scoped command execution;
- scoped Work Item execution;
- DbContext lifetime isolation;
- handler registration validation;
- provider adapter replacement;
- Rule Set package registration;
- credential-store platform selection;
- duplicate and missing service detection;
- test-host replacement;
- clean disposal during application shutdown.

## 2. Context

Chronicle is a modular monolith with multiple architectural layers and implementation modules.

Examples include:

```text
Domain
Application
Contracts
Persistence
Narrative Intelligence
Rule Knowledge
Rule Set packages
Desktop Presentation
Security
Diagnostics
Testing
```

These modules require collaboration without violating boundaries.

Examples:

- Application commands depend on repository and transaction ports;
- provider workflows depend on Narrative Intelligence contracts;
- Rule Set operations depend on package-owned implementations;
- Desktop ViewModels depend on command and query dispatchers;
- Work Item handlers depend on provider-neutral services and Application dispatch;
- persistence depends on EF Core and SQLite;
- credential resolution depends on a platform adapter;
- tests replace time, randomness, providers, and persistence behavior.

Without clear composition rules, Chronicle risks:

- service locator usage;
- hidden global dependencies;
- accidental singleton `DbContext`;
- singleton services capturing scoped dependencies;
- Domain code depending on Infrastructure;
- duplicate handler registration;
- provider SDK types leaking across layers;
- startup succeeding with a broken graph;
- test hosts behaving differently from production;
- Safe Mode requiring the exact subsystem that has failed.

ADR-0002 defines project boundaries and makes the Desktop project the composition root.

ADR-0013, ADR-0014, and ADR-0015 define command, Work Item, and query dispatchers.

This ADR defines how their implementations are assembled.

## 3. Decision Drivers

The dependency strategy prioritizes:

1. explicit architecture boundaries;
2. predictable lifetimes;
3. test replacement;
4. startup validation;
5. low framework coupling;
6. modular registration;
7. safe disposal;
8. no hidden global access;
9. platform-specific adapter selection;
10. Safe Mode resilience;
11. clear ownership;
12. maintainable open-source configuration.

## 4. Decision Summary

Chronicle will use:

```text
Container
    Microsoft.Extensions.DependencyInjection

Primary Composition Root
    Chronicle.Desktop

Registration Style
    module-owned extension methods
    explicit options and dependencies

Injection Style
    constructor injection

Domain
    no DI container references
    no injected services in entities

Application Execution
    explicit operation scope

Work Item Execution
    one scope per attempt

DbContext
    scoped
    never singleton
    never captured by singleton

Exclusive Port
    exactly one active implementation

Multiple Named Implementations
    explicit registry or keyed registration
    only where required

Startup
    registration validation
    options validation
    handler uniqueness validation
    platform capability validation

Safe Mode
    reduced graph
    optional provider and worker services disabled
```

## 5. Container Selection

Chronicle will use the built-in .NET dependency-injection abstractions and container:

```text
Microsoft.Extensions.DependencyInjection
```

### Rationale

It provides:

- native .NET hosting integration;
- conventional lifetime support;
- `IServiceCollection`;
- `IServiceProvider`;
- options integration;
- hosted-service integration;
- low dependency overhead;
- broad contributor familiarity;
- sufficient functionality for the MVP.

## 6. Container Boundary

The DI container is an Infrastructure and host concern.

Domain and ordinary Application contracts MUST NOT reference:

- `IServiceProvider`;
- `IServiceCollection`;
- `ServiceDescriptor`;
- container-specific types.

## 7. Composition Root

The primary production composition root is the Desktop executable project.

It owns:

- host construction;
- configuration loading;
- logging initialization;
- platform detection;
- service registration;
- startup validation;
- database initialization;
- migration coordination;
- worker activation;
- root ViewModel and window creation;
- graceful shutdown.

## 8. Secondary Composition Roots

Chronicle MAY have additional composition roots for:

```text
Migration Test Host
Integration Test Host
Packaging Verification Host
Future CLI Tool
Future Worker Process
```

Each root must follow the same module contracts.

## 9. No Registration in Domain

The Domain project contains no service-registration code.

It may expose:

- interfaces it owns;
- factories;
- Domain services;
- policy objects.

The host assembles implementations.

## 10. Application Registration

The Application module MAY expose:

```csharp
services.AddChronicleApplication(options => ...);
```

This registration method registers:

- command dispatcher;
- query dispatcher;
- pipeline behaviors;
- Application services;
- handler descriptors;
- validation;
- operation services.

It does not register concrete SQLite, Avalonia, or provider SDK implementations.

## 11. Infrastructure Registration

Infrastructure modules expose bounded registration functions such as:

```text
AddChronicleSqlitePersistence
AddChronicleOpenAiNarrativeIntelligence
AddChronicleWindowsCredentialStore
AddChronicleRuleKnowledge
AddChronicleStructuredLogging
```

## 12. Module Registration Contract

A module registration method SHOULD:

- register only services owned by that module;
- require explicit options;
- avoid silently replacing an existing exclusive service;
- validate mandatory configuration;
- avoid resolving services during registration;
- return `IServiceCollection` for composition.

## 13. Registration Dependencies

Registration order should not be semantically fragile.

A module requiring another module should:

- depend on its public port;
- validate required service presence at startup;
- document the dependency.

## 14. No BuildServiceProvider During Registration

Registration code MUST NOT call:

```text
services.BuildServiceProvider()
```

to resolve dependencies during service registration.

### Rationale

This creates:

- duplicate singleton instances;
- hidden lifetimes;
- premature resolution;
- disposal problems;
- inconsistent options.

## 15. Constructor Injection

Constructor injection is the default dependency mechanism.

A service constructor should make required dependencies explicit.

## 16. Optional Dependencies

Optional behavior should be represented through:

- explicit null-object implementation;
- optional capability registry;
- feature descriptor;
- optional typed wrapper.

Optional constructor parameters and ambient service lookup are discouraged.

## 17. Property Injection

Property injection is prohibited for production services.

It may be tolerated only in UI framework objects that require framework construction and after explicit review.

## 18. Method Injection

Method parameters may carry operation-specific collaborators or context where they are part of the contract.

They should not become a mechanism for bypassing constructor dependency clarity.

## 19. Service Locator

The following is prohibited in ordinary implementation code:

```text
IServiceProvider.GetService
IServiceProvider.GetRequiredService
static global service provider
ambient dependency registry
```

Allowed locations are limited to:

- composition root;
- dispatcher internals resolving typed handlers;
- hosted-service scope creation;
- approved factory adapters where the service type is itself the explicit product capability.

## 20. Domain Injection

Domain entities and value objects MUST NOT receive Infrastructure services.

They may receive explicit values such as:

- current UTC Instant;
- raw Dice values;
- Rule Set result;
- identifier;
- policy object with pure Domain semantics.

## 21. Domain Services

Pure Domain services may be created directly or registered when stateless and convenient.

They must not depend on repositories, logging, providers, clocks, or the DI container.

## 22. Lifetime Categories

Chronicle uses:

```text
Singleton
Scoped
Transient
```

Every registration should have an intentional lifetime.

## 23. Singleton

Use Singleton for services that are:

- stateless;
- immutable after startup;
- thread-safe;
- expensive to construct and safe to share;
- process-wide infrastructure.

Potential examples:

```text
SystemClock
MonotonicClock
IdentifierGenerator
Serializer Profiles
Rule Set Package Registry
Provider Adapter Registry
Immutable Configuration Snapshot
HTTP Client Factory
Safe Redaction Policy
```

## 24. Singleton Restrictions

A singleton MUST NOT capture:

- DbContext;
- scoped repository;
- command execution context;
- Work Item execution context;
- UI route scope;
- mutable Campaign aggregate;
- transient secret lease.

## 25. Scoped

Use Scoped for one logical Application execution boundary.

Examples:

```text
DbContext
repositories
transaction
Unit of Work
command execution context
query execution context where needed
Work Item attempt context
```

## 26. Scope Meaning

The meaning of a scope must be explicit.

The MVP uses distinct scopes for:

```text
one command dispatch
one query dispatch when needed
one Work Item attempt
one startup migration stage
one integration-test operation
```

A scope is not the lifetime of the entire desktop application.

## 27. Transient

Use Transient for lightweight stateless collaborators that:

- hold no disposable resource;
- hold no mutable shared state;
- are inexpensive to create;
- have behavior tied to one resolution.

Examples may include:

```text
validators
mappers
small policy evaluators
notification handlers
```

## 28. Transient Disposable Services

Transient disposable services are discouraged because disposal ownership becomes unclear.

Prefer scoped lifetime or explicit factory ownership.

## 29. DbContext Lifetime

`ChronicleDbContext` is scoped.

It MUST NOT be:

- singleton;
- held by ViewModel;
- held by hosted worker singleton;
- retained across provider calls;
- retained across user think time;
- reused across command executions.

## 30. DbContext Factory

The persistence module MAY use:

```text
IDbContextFactory<ChronicleDbContext>
```

or an internal equivalent for explicit scope construction.

Its use must not bypass Application transaction conventions.

## 31. Command Scope

Each command dispatch creates or uses one Application operation scope.

The scope contains:

- scoped repositories;
- transaction services;
- command context;
- handler;
- scoped pipeline behaviors.

## 32. Nested Command Dispatch

Nested dispatch within an active operation must not accidentally create an independent DbContext and transaction when the workflow requires one transaction.

The command architecture should prefer explicit Application services for same-transaction composition.

## 33. Query Scope

Simple read queries MAY create one short scope.

Queries requiring no scoped service may still execute within a dispatcher-created scope for consistency.

## 34. Work Item Scope

The worker service is Singleton.

It creates one scope per Work Item attempt.

The scope is disposed after:

- completion;
- retry transition;
- cancellation;
- failure.

## 35. Worker Scope Restrictions

A Work Item handler must not retain scoped services beyond its attempt.

Checkpoints and durable state carry continuation across attempts.

## 36. Hosted Services

Hosted services are singleton by the .NET host model.

They MUST create scopes through:

```text
IServiceScopeFactory
```

for scoped execution.

## 37. Background Worker Activation

The Work Item hosted service should start only after:

- storage initialization;
- migration success;
- application-mode validation;
- Safe Mode decision.

## 38. UI Services

Desktop shell services MAY be singleton when they represent process-wide UI concerns.

Examples:

```text
NavigationService
DialogCoordinator
WindowStateService
ApplicationLifetimeService
```

They must not capture scoped Application or persistence services.

## 39. ViewModels

ViewModels SHOULD be:

- transient;
- route-scoped through a navigation-owned lifetime;
- or singleton only for true shell-level state.

They depend on:

- command dispatcher;
- query dispatcher;
- safe UI services;
- notification subscriptions.

They do not depend on repositories or DbContext.

## 40. ViewModel Disposal

ViewModels with:

- subscriptions;
- timers;
- cancellation sources;
- file watchers;

must implement explicit disposal or deactivation.

## 41. Views

Views are normally created by Avalonia and bound to ViewModels.

The DI container SHOULD not become a general-purpose visual-tree factory.

## 42. Rule Set Registry

Multiple Rule Set packages are real product capabilities.

They SHOULD be registered through a typed registry keyed by stable `RuleSetPackageId`.

## 43. Rule Set Registration

A package registration SHOULD declare:

```text
RuleSetPackageId
PackageVersion
Character Schemas
Rule Operations
Preferences
Progression Contracts
Rule Knowledge Sources
```

## 44. Duplicate Rule Set Package

Duplicate active registrations for the same package identity and version are startup errors unless an explicit override policy exists.

## 45. Narrative Intelligence Providers

Multiple provider implementations may exist.

Use a typed registry such as:

```text
INarrativeIntelligenceProviderRegistry
```

keyed by stable provider key.

## 46. Provider Selection

Provider profile configuration chooses a provider by stable key.

The Application receives the provider-neutral contract.

## 47. Keyed Services

Built-in keyed DI services MAY be used when they improve registration clarity.

However, product registries are preferred when Chronicle needs:

- metadata;
- compatibility inspection;
- diagnostics;
- enumeration;
- package validation.

## 48. Exclusive Ports

Ports that must have exactly one active implementation include examples such as:

```text
IClock
IMonotonicClock
IAuthoritativeRandomSource
ISecretsManager for current platform
IApplicationTransactionFactory
```

Duplicate registration is a startup error.

## 49. Decorators

Cross-cutting behavior should normally use command, query, or worker pipelines rather than container decorators.

Decorators MAY be used for narrow Infrastructure concerns such as:

- safe logging;
- metrics;
- bounded retry;
- caching after approval.

## 50. Decorator Visibility

Decorator ordering MUST be explicit and tested.

## 51. Open Generic Registration

Open generic registration is permitted for well-defined patterns such as:

```text
ICommandHandler<TCommand, TResult>
IQueryHandler<TQuery, TResult>
IValidator<T>
```

Handler uniqueness must still be validated.

## 52. Assembly Scanning

Uncontrolled assembly scanning is discouraged for critical registrations.

### Allowed Approaches

- explicit registration;
- source-generated registry;
- controlled scan of known assemblies and known interfaces;
- scan validated by startup uniqueness checks.

## 53. Reflection Safety

Assembly scanning must not instantiate arbitrary types or load untrusted package assemblies without package trust validation.

## 54. Plugin Boundary

The MVP does not support arbitrary runtime plugin assembly loading.

Rule Set packages follow the approved package architecture.

## 55. Handler Registration

Every command and query type has exactly one primary handler.

Startup validation fails for:

- missing handler;
- duplicate handler;
- incompatible result type;
- unregistered pipeline metadata.

## 56. Notification Handler Registration

Post-commit notifications may have zero or more handlers.

Required behavior must not depend on an unvalidated optional notification handler.

## 57. Work Item Handler Registration

Every executable Work Type has exactly one handler and one payload contract owner.

Missing or duplicate registration blocks worker startup.

## 58. Options Pattern

Chronicle SHOULD use typed options for Infrastructure configuration.

Examples:

```text
SqliteOptions
OpenAiProviderOptions
WorkItemOptions
LoggingOptions
RuleKnowledgeOptions
BackupOptions
```

## 59. Options Validation

Options MUST be validated:

- at startup where required;
- before feature activation;
- without resolving secrets unnecessarily.

## 60. Secret References in Options

Options may contain credential references.

They MUST NOT contain credential values.

## 61. Options Mutability

Core production options SHOULD be immutable snapshots after startup unless dynamic reload is an explicit feature.

## 62. Dynamic Configuration

Dynamic configuration is deferred for most critical services.

A future reload mechanism must define:

- atomic replacement;
- validation;
- active operation behavior;
- rollback;
- provider-client refresh.

## 63. Configuration Binding

Configuration binding occurs at the host boundary.

Domain and Application services receive typed settings or policy values, not raw configuration providers.

## 64. Environment Variables

Environment variables may configure Development or CI behavior.

They must not become a silent production secret fallback.

## 65. HTTP Client Management

External provider adapters SHOULD use `IHttpClientFactory` or an equivalent approved client factory.

## 66. HTTP Client Registration

Named or typed clients may define:

- base endpoint;
- timeout;
- transport handler;
- safe diagnostics;
- retry behavior limited to transport-safe cases.

Credentials are attached at request time, not stored in client default state where profile mixing is possible.

## 67. Provider Client Lifetime

Provider adapter services may be singleton when:

- stateless;
- thread-safe;
- they create request-specific authentication;
- they do not retain Campaign context.

## 68. Secret Lease Lifetime

A `SecretLease` is never registered in DI.

It is resolved explicitly for one provider call and disposed immediately.

## 69. Repository Lifetime

Repositories are scoped and share the operation's DbContext or Unit of Work.

## 70. Transaction Lifetime

The transaction service is scoped to the command or migration operation.

It must not be singleton.

## 71. Dispatcher Lifetime

Command and query dispatchers MAY be singleton if they:

- hold only registries and scope factories;
- create operation scopes;
- do not capture scoped handlers.

Alternatively, they may be scoped inside explicit host-created scopes.

The spike will select the simplest safe model.

## 72. Registry Lifetime

Immutable handler and adapter registries are singleton after startup validation.

## 73. Event Dispatcher Lifetime

The post-commit dispatcher may be singleton when it resolves short-lived handlers safely.

It must not retain route-scoped UI subscribers indefinitely.

## 74. Notification Subscriptions

UI notification subscriptions belong to a dedicated local event-stream service with explicit subscription disposal.

They are not created through ad hoc container resolution.

## 75. Safe Mode

Safe Mode uses a reduced service graph.

It SHOULD initialize:

- configuration reader;
- logging;
- SQLite inspection;
- migration diagnostics;
- backup and restore;
- credential metadata inspection;
- error and diagnostics UI;
- package inventory;
- operation recovery queries.

## 76. Safe Mode Exclusions

Safe Mode MAY disable:

- provider calls;
- background worker execution;
- Campaign mutation;
- Rule Knowledge rebuild;
- automatic package migration;
- narration;
- Session finalization.

## 77. Safe Mode Composition

Safe Mode should not require successful construction of:

- OpenAI provider adapter;
- active Rule Set execution registry;
- normal worker scheduler;
- broken optional subsystem.

## 78. Startup Phases

Recommended startup phases:

```text
1. Bootstrap configuration
2. Bootstrap logging
3. Detect platform and application mode
4. Register core services
5. Register selected Infrastructure
6. Validate service graph
7. Initialize storage
8. Run or inspect migrations
9. Validate packages and providers
10. Start workers if allowed
11. Create Desktop shell
```

## 79. Bootstrap Logger

A minimal bootstrap logger MAY exist before the full container is built.

It must later hand off cleanly to the configured logging system.

## 80. Service Graph Validation

Production and test hosts MUST enable:

```text
ValidateScopes
ValidateOnBuild
```

or equivalent checks where supported.

## 81. Additional Validation

Chronicle SHOULD validate:

- exclusive port uniqueness;
- handler uniqueness;
- Work Type ownership;
- Rule Set package duplicates;
- provider key duplicates;
- missing required options;
- unsupported platform adapter;
- singleton-to-scoped dependency chains;
- serializer profile availability.

## 82. Startup Failure

If the normal graph is invalid:

- do not open the main application as healthy;
- log a safe diagnostic;
- attempt Safe Mode when possible;
- present a recovery-oriented error.

## 83. Lazy Resolution

Lazy resolution MAY be used for expensive optional services.

It must not hide missing mandatory dependencies until a critical operation is underway.

## 84. Factory Abstractions

Factories are appropriate when:

- a fresh scoped object is required;
- a type requires runtime parameters;
- creation selects a registered implementation;
- explicit ownership and disposal are needed.

## 85. Factory Restrictions

A generic `Func<Type, object>` or service-provider wrapper is a disguised service locator and is prohibited.

## 86. Typed Factory Example

Preferred:

```text
INarrativeIntelligenceProviderResolver
IRuleSetResolver
IApplicationScopeFactory
IWorkItemHandlerRegistry
```

## 87. Scope Factory

Chronicle MAY define a narrow `IApplicationScopeFactory` wrapping the container's scope factory.

This keeps container APIs inside Infrastructure and dispatcher internals.

## 88. Disposal

The .NET Host owns root-container disposal.

Scopes own disposal of:

- DbContext;
- transactions;
- scoped repositories;
- attempt-local resources.

## 89. Async Disposal

Services supporting asynchronous disposal should be disposed asynchronously where the host or scope API permits.

## 90. Shutdown Order

Recommended shutdown order:

1. stop accepting new UI operations;
2. stop claiming new Work Items;
3. cancel cancellable background operations;
4. finish short commit stages;
5. dispose UI subscriptions;
6. stop hosted services;
7. flush logs;
8. dispose service provider.

## 91. Singleton Disposal

Disposable singleton services must be registered through the container rather than created externally unless explicit ownership is documented.

## 92. Captive Dependency

A singleton depending directly or indirectly on scoped service is prohibited.

Startup tests should detect this.

## 93. Mutable Singleton State

Mutable singleton state is allowed only for explicit process-wide coordination such as:

- one-instance state;
- bounded worker scheduler;
- UI shell state;
- immutable-registry publication.

It must be thread-safe and tested.

## 94. Static State

Static mutable state is prohibited for:

- service access;
- active Campaign;
- current DbContext;
- provider profile;
- clock;
- random source;
- user configuration.

## 95. Thread Safety

Singleton services must document thread-safety assumptions.

## 96. Test Composition

Tests SHOULD create explicit test hosts or service collections.

They may replace:

- `IClock`;
- random sources;
- provider adapters;
- credential store;
- filesystem;
- worker wake mechanism;
- persistence location.

## 97. Replacement Policy

Test replacement SHOULD use explicit remove-and-add helpers.

Silent duplicate registration with last-one-wins behavior is discouraged.

## 98. Test Isolation

Every integration test host should use:

- isolated database;
- isolated data directory;
- isolated credential target namespace;
- deterministic options;
- deterministic clocks and random sources.

## 99. Unit Tests Without Container

Pure unit tests SHOULD instantiate Domain and small Application services directly.

The DI container is not required for every unit test.

## 100. Container Tests

Separate tests verify:

- production graph;
- Safe Mode graph;
- test graph;
- Development graph;
- handler registration;
- lifetime validation.

## 101. Development Services

Development-only services may include:

- in-memory credential store;
- scripted provider;
- verbose diagnostics;
- test package catalog.

They MUST NOT appear silently in Stable Release composition.

## 102. Release Composition Audit

CI MUST inspect the Release graph or output to ensure:

- no test provider registered as default;
- no plaintext secret adapter;
- no Development database path;
- no fake clock;
- no scripted random source;
- no debug-only endpoint.

## 103. Platform Selection

The host selects platform-specific adapters.

For Windows MVP:

```text
ISecretsManager
    → WindowsCredentialManagerSecretsManager
```

Unsupported platforms fail closed for credential-dependent features.

## 104. Conditional Registration

Conditional registration is permitted for:

- operating system;
- application mode;
- release channel;
- test host;
- Safe Mode;
- explicitly enabled optional feature.

It must not depend on hidden environment state.

## 105. Feature Flags

Feature flags are deferred as a broad system.

Small explicit build or configuration switches may control experimental features after validation.

## 106. Package Registration

Official bundled packages register through a package-loader boundary rather than arbitrary host code references where practical.

Package trust and compatibility are validated before activation.

## 107. Circular Dependencies

Project and service circular dependencies are prohibited.

When two services depend on each other, responsibilities must be reexamined or mediated through a smaller contract.

## 108. Dependency Direction

The intended direction remains:

```text
Presentation
    → Application Contracts

Infrastructure
    → Application Ports
    → Domain Contracts

Application
    → Domain

Domain
    → no outer layer
```

The composition root knows all concrete modules.

## 109. Public Constructor Size

A service with an excessive number of constructor dependencies may indicate:

- too many responsibilities;
- missing policy object;
- missing workflow service;
- overly broad handler.

No arbitrary numeric limit is adopted, but code review should treat unusually large constructors as a design signal.

## 110. Dependency Grouping

Grouping unrelated services into one facade merely to reduce constructor parameters is prohibited.

Facades must represent a real capability.

## 111. Logging Injection

Services use typed or category logging abstractions where useful.

Domain entities do not receive loggers.

## 112. Clock Injection

Application and Infrastructure services receive clock abstractions.

Domain transitions receive explicit Instants.

## 113. Randomness Injection

Only authorized Infrastructure and Application orchestration services receive randomness ports.

Rule Set resolution receives raw values, not a random source.

## 114. Cancellation

Cancellation tokens are operation parameters, not injected singleton state.

## 115. Execution Context

Command, query, and Work Item execution contexts are scoped or explicit method inputs.

They are not stored in ambient static context.

## 116. Ambient Context

`AsyncLocal` ambient context is discouraged for authority-bearing data.

It MAY be used narrowly for logging correlation if:

- absence is safe;
- values are nonauthoritative;
- tests cover leakage across operations.

## 117. Configuration Errors

Missing registration and invalid options use stable startup errors such as:

```text
composition.missing-service
composition.duplicate-service
composition.invalid-lifetime
composition.missing-handler
composition.duplicate-handler
composition.unsupported-platform
composition.invalid-options
composition.safe-mode-required
```

## 118. Observability

Startup diagnostics SHOULD report safe metadata:

- module registered;
- implementation key;
- service lifetime;
- package count;
- provider count;
- Safe Mode status;
- validation outcome.

They MUST NOT print secret configuration values.

## 119. Service Inventory

Developer diagnostics MAY expose a safe service inventory.

It should list contracts and implementation type names only in protected diagnostics.

## 120. Performance

Container resolution should not dominate Application execution.

Chronicle should avoid:

- deep decorator chains;
- repeated root-provider rebuild;
- reflection-heavy scans on every dispatch;
- dynamic proxy infrastructure without need.

## 121. Startup Performance

Registries and source-generated registration MAY be introduced if startup scanning becomes measurable.

## 122. Testing Strategy

The DI implementation requires:

```text
Registration Unit Tests
Graph Validation Tests
Lifetime Tests
Scope Tests
Safe Mode Tests
Release Composition Tests
Architecture Tests
Shutdown Tests
```

## 123. Registration Tests

Tests MUST verify each module's registration method in isolation with required dependencies.

## 124. Graph Validation Tests

Tests MUST build:

- normal production graph;
- Safe Mode graph;
- Development graph;
- test graph.

## 125. Lifetime Tests

Tests MUST detect:

- singleton capturing scoped service;
- DbContext reused across operations;
- Work Item handler retained after attempt;
- ViewModel retaining disposed scope;
- transient disposable leak.

## 126. Command Scope Tests

Tests SHOULD prove:

- one DbContext per command scope;
- same DbContext shared by scoped repositories;
- scope disposed after dispatch;
- provider call does not retain scoped transaction.

## 127. Work Item Scope Tests

Tests SHOULD prove:

- one scope per attempt;
- retry creates a new scope;
- checkpoint persists across scopes;
- scoped service disposal occurs after failure.

## 128. Safe Mode Tests

Tests MUST prove Safe Mode can start with:

- invalid provider configuration;
- unavailable credential store;
- failed package activation;
- migration failure;
- disabled worker.

## 129. Release Composition Tests

Tests MUST prove Stable Release excludes:

- fake clocks;
- scripted random sources;
- test providers;
- in-memory production stores;
- plaintext secret adapters;
- Development-only diagnostics.

## 130. Required Test Cases

Tests MUST cover:

- normal host build;
- duplicate exclusive port;
- missing exclusive port;
- missing command handler;
- duplicate command handler;
- missing Work Item handler;
- duplicate provider key;
- duplicate Rule Set package;
- invalid options;
- unsupported platform;
- scoped DbContext;
- singleton provider adapter;
- singleton captures scoped service;
- command scope disposal;
- query scope disposal;
- Work Item scope disposal;
- graceful shutdown;
- Safe Mode startup;
- test replacement;
- Release graph audit;
- no service locator in restricted assemblies.

## 131. Architecture Tests

Architecture tests MUST reject:

- `IServiceProvider` in Domain;
- `IServiceCollection` in Domain or ordinary Application contracts;
- service locator use in ViewModels and handlers;
- static mutable service registry;
- singleton DbContext;
- repositories registered singleton;
- provider SDK types in Application constructors;
- Domain entity constructors with Infrastructure services;
- test implementations referenced from Stable production projects;
- arbitrary runtime plugin assembly scanning.

## 132. Prohibited Patterns

### 132.1 Global Service Provider

All dependencies remain explicit.

### 132.2 DI in Domain Entities

Domain decisions receive values and pure policies.

### 132.3 Singleton DbContext

Persistence is scoped.

### 132.4 Hosted Service Captures Repository

It creates one execution scope per attempt.

### 132.5 BuildServiceProvider During Registration

One host owns one root provider.

### 132.6 Last Registration Silently Wins for Exclusive Port

Duplicates are errors.

### 132.7 Hidden Environment-Based Production Adapter Selection

Selection is explicit and validated.

### 132.8 Test Service in Stable Release

Release composition is audited.

### 132.9 Arbitrary Assembly Scanning

Critical contracts use controlled registration.

### 132.10 Generic Factory as Service Locator

Factories are typed and capability-specific.

## 133. Alternatives Considered

### Autofac

Powerful and mature, with advanced module and decorator support.

Not selected initially because the built-in container satisfies Chronicle's MVP needs with lower dependency and conceptual overhead.

### Lamar, DryIoc, or Simple Injector

Capable alternatives, but no requirement currently justifies replacing the platform container.

### Manual Composition Without Container

Explicit and simple for small systems, but Chronicle has hosted services, multiple Infrastructure modules, options, and test replacement needs that benefit from the standard container.

### Static Service Registry

Rejected because it hides dependencies, weakens tests, and creates global mutable state.

### Service Locator

Rejected for the same reasons and because it undermines architecture review.

## 134. Consequences

### Positive

- familiar .NET hosting model;
- explicit module boundaries;
- predictable scope ownership;
- safe DbContext lifetime;
- replaceable test infrastructure;
- controlled provider and Rule Set registries;
- Safe Mode composition;
- low third-party dependency burden.

### Negative

- startup validation code must be implemented;
- module registration methods add maintenance;
- built-in container lacks some advanced features;
- explicit registries add code;
- lifetime errors require disciplined testing;
- Safe Mode needs a separately validated graph.

## 135. Risks

### Captive Scoped Dependency

Mitigation:

- scope validation;
- graph tests;
- code review;
- explicit worker scopes.

### Service Locator Creep

Mitigation:

- architecture scans;
- typed factories;
- small composition root;
- contributor conventions.

### Registration Drift

Mitigation:

- module-owned registrations;
- startup validation;
- production-graph tests;
- generated registries where useful.

### Safe Mode Depends on Broken Service

Mitigation:

- reduced graph;
- optional subsystem boundaries;
- dedicated Safe Mode tests.

### Multiple Implementation Ambiguity

Mitigation:

- explicit registries;
- stable keys;
- duplicate rejection;
- profile selection validation.

## 136. Technology Spike

Before acceptance, implement:

1. Desktop Generic Host;
2. Application registration;
3. SQLite persistence registration;
4. OpenAI provider registration;
5. Windows credential-store registration;
6. Rule Set registry;
7. command and query handler registries;
8. Work Item handler registry;
9. command scope;
10. Work Item attempt scope;
11. startup graph validation;
12. Safe Mode graph;
13. test-host overrides;
14. release composition audit;
15. graceful shutdown and disposal test.

## 137. Spike Acceptance

The spike passes when:

- normal Desktop composition builds with validation enabled;
- Safe Mode builds without provider or worker activation;
- command and Work Item executions receive isolated scoped DbContexts;
- singleton services capture no scoped dependencies;
- missing and duplicate handlers fail startup;
- provider and Rule Set selection use stable registries;
- test hosts replace clock, randomness, and provider explicitly;
- Stable Release graph contains no Development implementations;
- all scoped disposables are released after operation completion;
- Domain and Application contracts contain no container references.

## 138. Definition of Compliance

An implementation complies when:

- the built-in .NET DI container is used;
- the Desktop host is the primary composition root;
- modules own bounded registration methods;
- constructor injection is the default;
- Domain is container-independent;
- service locator use is prohibited;
- lifetimes are intentional and validated;
- DbContext and repositories are scoped;
- hosted workers create one scope per attempt;
- exclusive ports have one active implementation;
- provider and Rule Set multiplicity use typed registries;
- Safe Mode uses a reduced graph;
- tests and CI validate production composition and disposal.

## 139. Review Triggers

This ADR must be reviewed if:

- the built-in DI container cannot support required composition safely;
- runtime plugins are introduced;
- Chronicle becomes multi-process;
- a server host is added;
- remote clients require per-request scopes;
- provider or Rule Set hot reload is introduced;
- dynamic module loading becomes necessary;
- startup performance becomes unacceptable;
- AOT or trimming changes registration requirements;
- an advanced decorator or interception model becomes necessary.

## 140. Deferred Decisions

Later ADRs MAY define:

- exact source-generated registration mechanism;
- exact typed registry implementation;
- runtime package activation lifecycle;
- server request scope;
- plugin isolation;
- provider hot reload;
- dynamic configuration reload;
- dependency graph visualization;
- AOT registration strategy;
- custom analyzer rules for service lifetimes.

## 141. Final Decision

Chronicle will use the built-in .NET dependency-injection container, with the Desktop application as the primary composition root.

Dependencies will be registered by bounded modules, validated at startup, and resolved through explicit scopes.

The Domain will remain unaware of the container.

Chronicle's architecture should make it obvious where a dependency comes from, how long it lives, and which boundary owns it.
