---
id: RFC-0038
title: Desktop Application Architecture and Process Model
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-08-01
category: Architecture
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
---

> **"The desktop application is the first home of Chronicle, not the definition of Chronicle."**

# Desktop Application Architecture and Process Model

## Abstract

This RFC defines the architecture of Chronicle's official desktop application.

It establishes process boundaries, application composition, UI and application-service separation, local background workers, provider execution, Rule Set package hosting, persistence ownership, startup, shutdown, single-instance behavior, inter-process communication, crash containment, updates, local file access, safe mode, diagnostics, and testing.

The official desktop application is the first implementation of Chronicle. It MUST implement Chronicle's contracts without becoming the definition of the framework.

The MVP remains local-first, single-user, and desktop-first.

## 1. Purpose

Chronicle requires a concrete execution model for its official application.

Without one, implementation decisions may accidentally:

- place business rules inside UI components;
- allow the renderer to access persistence directly;
- expose credentials to presentation code;
- block the interface during provider calls;
- lose critical background work on restart;
- mix provider SDKs into Domain code;
- make one desktop technology a permanent framework constraint;
- weaken process and security boundaries.

This RFC prevents those outcomes.

## 2. Scope

This RFC defines:

- desktop application responsibilities;
- architectural layers;
- process topology;
- UI-thread rules;
- application commands and queries;
- durable background work;
- provider and Rule Set hosting;
- persistence ownership;
- single-instance behavior;
- IPC requirements;
- startup and shutdown;
- crash recovery;
- safe mode;
- local file interaction;
- platform abstractions;
- packaging;
- resource limits;
- observability;
- testing.

It does not define one desktop framework, one UI toolkit, final visual design, mobile or web architecture, multiplayer hosting, or exact updater technology.

## 3. Core Principle

The desktop application is an adapter around Chronicle's contracts.

```text
Desktop UI
    ↓
Application API
    ↓
Application Services
    ↓
Domain and Contracts
    ↓
Infrastructure Adapters
```

The UI MUST NOT bypass the Application layer.

## 4. Architectural Layers

The official application SHOULD contain:

```text
Presentation
Application
Domain
Contracts
Infrastructure
Host
```

### Presentation

Owns windows, views, navigation, user input, accessibility, rendering, progress display, and confirmation dialogs.

It does not own Campaign truth.

### Application

Owns use-case orchestration, commands, queries, transactions, idempotency, recovery decisions, provider coordination, and Rule Set coordination.

### Domain

Owns Campaign invariants, entities, value objects, lifecycle transitions, and accepted-state semantics.

### Contracts

Define provider-neutral DTOs, Rule Set interfaces, repositories, structured outputs, Work Items, and diagnostics.

### Infrastructure

Implements persistence, providers, credential storage, Rule Knowledge indexing, random generation, backup, import/export, logging, package loading, and filesystem access.

### Host

Composes services and owns startup, configuration, service lifetime, worker initialization, shutdown, safe mode, and health checks.

## 5. Process Topology

The architecture SHOULD permit:

```text
Desktop UI Process
Application Host Process
Optional Provider Worker Process
Optional Local Model Process
```

The MVP SHOULD begin with one main operating-system process:

```text
Main Application Process
├── UI Thread
├── Application Services
├── Local Background Worker
├── Persistence Adapters
├── Approved Rule Set Packages
└── Provider Adapters
```

Future process separation remains possible but MUST NOT be introduced without evidence.

## 6. UI Thread

The UI thread MUST remain responsive.

It MUST NOT directly execute:

- provider calls;
- storage migrations;
- Rule Knowledge indexing;
- backup creation;
- import parsing;
- large exports;
- mechanical simulations;
- blocking filesystem operations.

## 7. UI State

UI state MAY contain:

- selected Campaign;
- current view;
- scroll position;
- unsent input;
- open dialog;
- filters;
- animation state.

It MUST NOT be treated as authoritative Campaign state.

## 8. Command Flow

A typical write flow is:

```text
User Action
    ↓
Presentation Validation
    ↓
Application Command
    ↓
OperationId
    ↓
Application and Domain Validation
    ↓
Rule Set or Provider Work
    ↓
Transaction Commit
    ↓
Query Model Refresh
    ↓
UI Update
```

## 9. Query Flow

A typical read flow is:

```text
UI Request
    ↓
Application Query
    ↓
Authoritative or Read Model Store
    ↓
Purpose-Specific DTO
    ↓
UI Rendering
```

The UI MUST NOT issue SQL, use repositories directly, or retain mutable Domain entities.

## 10. Background Worker

The Host SHOULD run a local background worker implementing RFC-0019.

It may process:

- provider continuation;
- Session finalization;
- Narrative Plan revision;
- backup;
- export;
- index build;
- migration;
- diagnostic bundle generation.

Critical work MUST be durable before execution.

In-memory tasks alone are insufficient for finalization, post-Roll continuation, migration, backup publication, or import publication.

## 11. Worker Concurrency

The MVP SHOULD use bounded concurrency.

Recommended policy:

```text
One Campaign-mutating Work Item per Campaign
Limited global provider concurrency
Limited global index-build concurrency
```

## 12. Progress and Cancellation

Long operations SHOULD expose status, phase, meaningful progress, cancellation availability, retry state, and safe explanation.

Cancellation MUST NOT:

- interrupt an atomic commit;
- delete a persisted Dice Roll;
- partially apply finalization;
- abandon a published restore;
- corrupt an export.

## 13. Provider Hosting

Provider adapters MAY run in-process in the MVP.

They MUST use bounded timeouts, cancellation, structured contracts, credential aliases, least-context prompts, and safe observability.

A local model may run embedded, as a child process, or as an externally managed local service. The choice requires an ADR.

Provider failure MUST NOT corrupt Campaign state or terminate the application.

## 14. Rule Set Hosting

Approved Rule Set packages MAY run in-process in the MVP.

They MUST:

- implement stable contracts;
- remain deterministic;
- avoid I/O;
- avoid provider calls;
- avoid persistence access;
- respect resource limits.

Dynamic untrusted package loading is outside the MVP and requires a sandboxed host.

## 15. Persistence Ownership

Only Application and Infrastructure services access persistence.

The main application process owns the local authoritative store.

Database transactions remain scoped to application operations and MUST NOT span provider or player waits.

## 16. Single-Instance Policy

The official application SHOULD allow one active instance per local data directory.

This prevents:

- conflicting migrations;
- duplicate background workers;
- concurrent writers;
- inconsistent backup operations.

A second launch SHOULD focus the existing instance or forward a supported activation intent, then exit safely.

## 17. Data Directory Lock

Chronicle SHOULD use a process lock or equivalent guard.

A stale lock may be removed only after confirming that no active process owns it.

The lock is not a substitute for database integrity controls.

## 18. Multi-Window Support

The MVP MAY use one main window.

Future secondary windows may show Character Sheets, diagnostics, exports, or settings. All windows share the same Application services and authoritative state.

## 19. Inter-Process Communication

If processes are separated, IPC MUST use:

- versioned messages;
- bounded payloads;
- explicit methods;
- request identifiers;
- timeouts;
- validation;
- local authentication appropriate to the platform.

IPC MUST NOT expose arbitrary method invocation, raw database handles, unrestricted file operations, credentials, or code evaluation.

## 20. Startup Sequence

Recommended startup order:

1. initialize minimal logging;
2. resolve the data directory;
3. acquire the instance lock;
4. load configuration;
5. initialize the credential store;
6. initialize persistence;
7. validate storage schema;
8. run or schedule migrations;
9. register Rule Set packages;
10. validate package integrity;
11. initialize the background worker;
12. recover incomplete operations;
13. validate read models and indexes;
14. evaluate component health;
15. open the UI.

## 21. Startup Modes

Startup may be:

```text
Normal
Degraded
SafeMode
Blocked
```

Examples:

```text
Missing provider credential
    → Degraded

Knowledge index unavailable
    → Degraded

Migration failed
    → SafeMode or Blocked

Database inaccessible
    → Blocked

Required Rule Set package invalid
    → SafeMode
```

## 22. Fast UI Startup

The UI MAY open before optional initialization finishes.

Unavailable actions must remain blocked and component health must be visible.

## 23. Startup Recovery

Chronicle SHOULD inspect:

- incomplete Operation Records;
- expired Work Item leases;
- pending finalization;
- unresolved persisted Rolls;
- interrupted backups;
- staged imports;
- incomplete migrations.

## 24. Shutdown Sequence

Recommended shutdown:

1. stop accepting new state-changing commands;
2. signal cancellable operations;
3. allow atomic commits to finish;
4. checkpoint durable work;
5. flush logs;
6. close persistence cleanly;
7. stop child processes;
8. release the instance lock.

Forced shutdown recovery relies on transactions, Operation Records, Work Item leases, persisted Rolls, staged artifacts, and startup integrity checks.

## 25. Draft Recovery

The application MAY preserve unsent player input locally.

Recovered drafts MUST NOT be submitted automatically.

## 26. Child Process Management

Child processes SHOULD be managed by a dedicated service tracking:

- process identity;
- purpose;
- start time;
- health;
- resource usage;
- exit code;
- restart policy.

A child process receives only the environment and secrets required for its purpose.

Automatic restart must be bounded to avoid crash loops.

## 27. Safe Mode

Safe Mode SHOULD permit:

- Campaign inspection;
- configuration repair;
- backup;
- export;
- diagnostics;
- migration recovery;
- disabling a problematic provider or package.

It SHOULD block normal Campaign mutation, narration, automatic finalization, and unsafe package loading.

Safe Mode may be triggered by startup failure, repeated crash, failed migration, integrity failure, command-line option, or user choice.

## 28. Local File Interaction

The UI SHOULD use platform file dialogs for:

- import;
- export;
- backup location;
- Rule Knowledge source selection;
- future attachments.

Every selected path passes through application validation.

Drag-and-drop input remains untrusted and uses the same validation pipeline.

## 29. Notifications and Clipboard

Desktop notifications MAY report background completion or required user decisions.

They MUST NOT expose Secrets or private narrative content by default.

Clipboard operations must be explicit. Chronicle must not copy hidden content automatically.

## 30. Deep Links

Future deep links may open Campaigns, settings, operations, or imports.

They require allowlisted commands, strict parsing, validated parameters, no embedded credentials, and safe single-instance forwarding.

They are not required for MVP.

## 31. Updates

Automatic updating is not required by this RFC.

Future update architecture SHOULD support signed packages, user control, rollback, and migration coordination.

An update MUST NOT publish successfully when a required migration fails.

Rule Set package updates remain distinct from application updates.

## 32. Desktop Packaging

The official application SHOULD package:

- executable;
- runtime;
- official Rule Set package;
- migrations;
- local documentation;
- default configuration;
- licenses.

Provider credentials are never packaged.

## 33. Platform Abstractions

Platform-specific behavior SHOULD sit behind interfaces such as:

```text
CredentialStore
FileDialogService
NotificationService
ProcessLock
ApplicationPathService
ClipboardService
UpdateService
```

Desktop framework and operating-system APIs MUST NOT enter Domain contracts.

## 34. Resource Limits

The Host SHOULD enforce limits for:

- provider concurrency;
- worker concurrency;
- memory use;
- import size;
- export size;
- indexing concurrency;
- child-process restarts;
- UI event backlog.

Under pressure, Chronicle should preserve authoritative transactions and UI responsiveness before optional indexing or verbose diagnostics.

## 35. Offline Behavior

The MVP SHOULD support offline:

- Campaign viewing;
- local persistence;
- Character management;
- Rule Set mechanics;
- Dice Rolls;
- Memories;
- backup;
- export;
- local providers when configured.

Remote narration remains unavailable without connectivity.

## 36. Error Boundaries

### Presentation

Unexpected UI failures should be contained and must not corrupt Domain state.

### Application Host

Unhandled failures should stop unsafe continuation, preserve diagnostics, determine transaction state, and offer restart or Safe Mode.

### Provider

Provider errors remain scoped to their OperationId.

### Rule Set

Unexpected package failure fails the operation, preserves state, degrades package health, and creates diagnostics.

### Migration

Migration failure blocks publication and preserves the prior version or checkpoint.

## 37. Observability

The Host SHOULD record:

- startup phase duration;
- component health;
- process identity;
- worker backlog;
- child-process exits;
- shutdown duration;
- Safe Mode activation;
- instance-lock conflicts;
- recovery actions.

Observability MUST follow RFC-0035 and RFC-0036.

## 38. Testing Strategy

Host tests cover composition, startup order, degraded mode, Safe Mode, shutdown, and instance locking.

UI boundary tests prove that commands go through the Application layer and that Presentation does not access repositories.

Worker tests cover durable execution, restart, cancellation, lease expiry, and bounded concurrency.

Process tests cover child launch, failure, restart limits, environment minimization, and shutdown.

Recovery tests cover crashes after provider output, after Roll persistence, during finalization, and during migration.

## 39. Required Test Cases

Tests MUST cover:

- normal startup;
- startup without provider credentials;
- startup with missing index;
- inaccessible database;
- single-instance enforcement;
- stale-lock recovery;
- UI-to-Application command flow;
- responsive UI during provider calls;
- durable Work Item execution;
- cancellation before commit;
- cancellation during commit;
- forced shutdown after Roll persistence;
- startup recovery;
- provider exception containment;
- Rule Set exception containment;
- child-process crash loop;
- Safe Mode;
- backup and export from Safe Mode;
- malicious drag-and-drop input;
- notification privacy;
- shutdown with active work;
- child-process secret minimization;
- offline Campaign access;
- remote-provider outage while local mechanics continue.

## 40. Prohibited Patterns

### Business Logic in Presentation

The UI does not own Domain rules.

### Direct Database Access From UI

All reads and writes pass through Application contracts.

### Blocking UI on External Work

Provider, indexing, backup, import, and migration work remain asynchronous.

### In-Memory-Only Critical Work

Recoverable workflows require durable records.

### Global Mutable Campaign Object

Authoritative state remains governed by Domain and persistence boundaries.

### Provider Failure Crashes Application

External faults must be contained.

### Untrusted Packages In Process

Only approved packages run in-process in MVP.

### Long Transaction During User or Provider Wait

Transactions remain short.

### Desktop Framework in Domain

Chronicle remains host-neutral beneath the Presentation layer.

### Child Process Receives All Secrets

Capabilities and credentials are minimized.

## 41. Current Delivery Decision

The MVP adopts:

- one main desktop application process;
- one UI thread;
- asynchronous Application operations;
- one local durable background worker;
- in-process approved Rule Set packages;
- in-process provider adapters;
- optional externally managed local provider service;
- one application instance per data directory;
- platform abstractions;
- Safe Mode;
- startup recovery;
- coordinated shutdown;
- local-first offline capabilities;
- no dynamic external plugins;
- no distributed process architecture;
- no mandatory updater;
- no mobile or web host.

## 42. Architecture Horizon

Future evolution MAY include:

- separate Application Host;
- isolated provider worker;
- sandboxed plugin host;
- local-model process manager;
- multi-window Director tools;
- mobile and web clients;
- remote Campaign hosting;
- multiplayer synchronization;
- signed automatic updates;
- notifications;
- deep links;
- file associations.

The MVP MUST NOT implement these capabilities without a later milestone.

## 43. Open Questions

The following remain open:

- Which desktop framework will be selected?
- Which operating systems are supported first?
- Will UI and Application Host remain in one process?
- How will single-instance behavior be implemented?
- Which scheduler will run durable Work Items?
- Will Chronicle launch local providers or connect to them?
- Is Safe Mode part of the first visible MVP?
- Are multiple windows needed initially?
- How should draft input be recovered?
- Is an update mechanism needed before first release?
- Should installer and portable builds both exist?
- Which operations remain available offline?
- How will process-level integration tests run in CI?

These questions require technology ADRs, UI RFCs, packaging decisions, and implementation evidence.

## 44. Compliance Checklist

An implementation complies when:

- the desktop host remains an adapter around Chronicle contracts;
- Presentation does not own business logic;
- Presentation does not access persistence directly;
- long operations do not block the UI thread;
- critical background work is durable;
- provider and Rule Set failures are contained;
- only approved packages run in-process;
- one instance owns one data directory;
- startup and shutdown are coordinated;
- interrupted work recovers;
- Safe Mode protects data;
- child processes receive minimum privilege;
- local-first operation remains possible;
- desktop technology does not enter Domain contracts.

## 45. Final Principle

The desktop application gives Chronicle a body.

It must not become Chronicle's mind, memory, or rules.

Those belong to the architecture beneath it.
