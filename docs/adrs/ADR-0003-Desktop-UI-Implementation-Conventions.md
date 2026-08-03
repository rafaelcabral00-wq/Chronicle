---
id: ADR-0003
title: Desktop UI Implementation Conventions
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
  - RFC-0038
  - RFC-0039
  - RFC-0040
  - RFC-0042
---

> **"The UI may adapt presentation freely, but it must never reinterpret Chronicle's state."**

# Desktop UI Implementation Conventions

## 1. Status

**Proposed**

This ADR defines the implementation conventions for Chronicle's Avalonia desktop user interface.

The decision becomes **Accepted** after a UI spike proves:

- one complete Campaign list flow;
- one live-play transcript;
- one structured Dice Roll interaction;
- one long-running operation with progress and cancellation;
- one recovery state;
- one accessible keyboard-only path;
- no direct persistence or provider access from Presentation code.

## 2. Context

ADR-0001 selected:

- C#;
- .NET 10 LTS;
- Avalonia UI;
- XAML;
- MVVM;
- CommunityToolkit.Mvvm.

RFC-0038 and RFC-0039 define the desktop host and interaction model.

The implementation still requires concrete conventions for:

- View and ViewModel boundaries;
- navigation;
- dependency injection;
- state ownership;
- command execution;
- query refresh;
- asynchronous behavior;
- UI threading;
- dialogs;
- notifications;
- operation status;
- validation;
- accessibility;
- localization;
- styling;
- testing.

Without explicit conventions, the UI could drift into:

- direct repository access;
- service locator usage;
- mutable Domain objects inside ViewModels;
- ViewModels containing mechanical logic;
- fragile code-behind;
- navigation coupled to concrete Views;
- duplicated operation state;
- unsafe optimistic updates;
- inaccessible controls;
- inconsistent error handling.

## 3. Decision Drivers

The conventions prioritize:

1. strict separation from Application and Infrastructure;
2. testable ViewModels;
3. predictable asynchronous behavior;
4. explicit operation state;
5. strong accessibility;
6. consistent navigation;
7. low code-behind;
8. reusable semantic components;
9. responsive long-running workflows;
10. straightforward contributor onboarding.

## 4. Decision Summary

Chronicle Desktop will use:

```text
Avalonia Views
    XAML-first presentation

ViewModels
    CommunityToolkit.Mvvm
    constructor injection
    purpose-specific query DTOs
    local presentation state

Application Interaction
    explicit command and query gateways
    OperationId-aware command execution
    no repository or DbContext access

Navigation
    route-based navigation service
    typed route parameters
    stable route identifiers
    ViewModel-first activation

Long Operations
    explicit operation state model
    cancellation tokens
    persistent status
    safe retry and recovery

Validation
    local form validation for responsiveness
    authoritative Application validation on submit

Dialogs
    abstract dialog service
    typed request and result contracts

Notifications
    centralized notification service
    persistent treatment for critical failures

Accessibility
    keyboard-first core workflows
    semantic labels
    focus management
    text equivalents for all outcomes
```

## 5. Presentation Boundary

The `Chronicle.Desktop` project is the only production project allowed to reference Avalonia.

Presentation code MUST NOT reference:

- EF Core;
- SQLite types;
- repositories;
- provider SDKs;
- Rule Set implementation classes;
- filesystem implementations;
- credential-store implementations.

It MAY reference Application contracts and desktop-safe abstractions.

## 6. View Responsibilities

A View owns:

- layout;
- bindings;
- visual states;
- focus behavior;
- semantic labels;
- visual hierarchy;
- desktop-specific interaction;
- trivial event forwarding where binding is impractical.

A View MUST NOT own:

- Campaign invariants;
- Rule Set mechanics;
- persistence;
- provider invocation;
- command idempotency;
- visibility decisions;
- retry classification.

## 7. ViewModel Responsibilities

A ViewModel owns:

- presentation state;
- local form state;
- command availability;
- query loading;
- mapping Application DTOs to display-ready values;
- orchestration of UI services;
- cancellation for current presentation operations;
- safe error presentation state.

A ViewModel MUST NOT:

- use DbContext;
- use repositories;
- mutate Domain entities;
- calculate authoritative Dice outcomes;
- infer hidden information;
- invoke provider adapters directly;
- become a long-lived cache of authoritative Campaign state.

## 8. ViewModel Base Types

Chronicle SHOULD avoid a large inheritance hierarchy.

A minimal base MAY provide:

```text
Property notification support
Disposal support
Cancellation scope
Safe operation helpers
```

Feature-specific behavior SHOULD use composition.

## 9. CommunityToolkit.Mvvm

ViewModels SHOULD use:

- `ObservableObject`;
- source-generated observable properties;
- relay commands;
- async relay commands;
- property change notification.

Generated properties and commands SHOULD remain explicit enough for code review.

## 10. Code-Behind Policy

Code-behind is permitted only for Presentation-specific behavior such as:

- focus transfer;
- drag-and-drop event adaptation;
- window sizing;
- visual-tree integration;
- animation coordination;
- control-specific workaround.

Business or Application behavior in code-behind is prohibited.

## 11. ViewModel-First Navigation

Navigation SHOULD activate a ViewModel and resolve its View by convention or registration.

This preserves testability and avoids concrete View dependencies in navigation decisions.

## 12. Route Model

Routes SHOULD use stable, language-neutral identifiers.

Examples:

```text
campaigns
campaign/{campaignId}
campaign/{campaignId}/play
campaign/{campaignId}/character
campaign/{campaignId}/memories
campaign/{campaignId}/progression
settings/providers
diagnostics
```

## 13. Typed Route Parameters

Route parameters MUST be parsed into typed values before ViewModel activation.

Invalid route parameters produce a safe navigation error.

## 14. Navigation Service

The navigation service SHOULD support:

- navigate;
- replace;
- back;
- can-go-back;
- open modal route;
- restore previous route;
- deep-link activation later.

It MUST NOT expose arbitrary View construction.

## 15. Navigation State

Navigation state is Presentation state.

Chronicle MAY persist:

- last opened Campaign;
- last top-level destination;
- window placement.

It SHOULD NOT persist transient modal or loading states as Campaign truth.

## 16. View Activation

A ViewModel SHOULD receive activation input through an explicit method or constructor contract.

Activation SHOULD:

- validate route input;
- load required query models;
- subscribe to operation updates;
- establish cancellation scope.

## 17. View Deactivation

Deactivation SHOULD:

- cancel UI-owned loading;
- unsubscribe local event handlers;
- preserve explicitly recoverable drafts;
- release large presentation caches;
- avoid canceling already committed durable work.

## 18. Application Gateway

Presentation SHOULD interact through narrow interfaces such as:

```text
ICommandGateway
IQueryGateway
IOperationStatusGateway
INavigationService
IDialogService
INotificationService
IClipboardService
IFileDialogService
```

## 19. Command Gateway

`ICommandGateway` SHOULD:

- accept typed commands;
- preserve OperationId;
- return typed Application results;
- expose retry classification;
- propagate cancellation appropriately;
- never bypass Application validation.

## 20. Query Gateway

`IQueryGateway` SHOULD:

- accept typed queries;
- return purpose-specific DTOs;
- support cancellation;
- return safe availability state;
- avoid returning persistence entities.

## 21. OperationId Ownership

For a new user intention, the UI creates or requests one OperationId.

For a retry of the same intention, the UI reuses that OperationId.

The ViewModel MUST distinguish:

```text
RetrySameOperation
StartNewOperation
RefreshStatus
```

## 22. Async Command Convention

All I/O-bound UI commands SHOULD be asynchronous.

An async command MUST define:

- execution state;
- cancellation policy;
- duplicate-execution policy;
- error mapping;
- final refresh behavior.

## 23. UI Thread Convention

Only Presentation mutation occurs on the UI thread.

Application and Infrastructure work runs outside the UI thread.

The UI dispatcher MUST be abstracted or isolated where practical.

## 24. No Blocking Waits

Production Presentation code MUST NOT use:

```text
Task.Wait()
Task.Result
GetAwaiter().GetResult()
Thread.Sleep()
```

for Application operations.

## 25. Busy State Model

A ViewModel SHOULD expose explicit state rather than a single boolean.

Recommended model:

```text
Idle
Loading
Submitting
WaitingForProvider
WaitingForPlayer
Applying
Recovering
Completed
Failed
Unavailable
```

## 26. Operation Status Model

A reusable operation-status model SHOULD include:

```text
OperationId
Phase
Status
SafeMessage
Progress
CanCancel
CanRetry
RetryKind
RequiresUserAction
ReferenceCode
```

## 27. Long-Running Operations

Long operations MUST remain visible after the initiating control disappears.

Examples:

- Session finalization;
- backup;
- restore;
- import;
- migration;
- knowledge indexing;
- provider generation.

A global operation area SHOULD preserve status.

## 28. Cancellation

UI cancellation requests are advisory.

The UI MUST display whether cancellation:

- completed;
- was refused because commit began;
- leaves durable work pending;
- requires later recovery.

## 29. Query Refresh

After a successful state-changing command, the ViewModel SHOULD refresh the relevant query model.

It MUST NOT manually patch authoritative state unless the change is purely presentational and safely reversible.

## 30. Optimistic UI

Optimistic UI is allowed only for low-risk Presentation state.

It is prohibited for:

- Dice results;
- progression spend;
- Session finalization;
- Preference migration;
- restore;
- import;
- Character schema migration.

## 31. Stale Data

Query DTOs SHOULD carry versions needed for commands.

If a stale-version error occurs, the UI SHOULD:

1. preserve safe draft input;
2. refresh the query;
3. explain what changed;
4. require confirmation again where material.

## 32. Forms

Forms SHOULD use a consistent state model:

```text
Pristine
Dirty
Validating
Valid
Invalid
Submitting
Submitted
Failed
```

## 33. Local Validation

Local validation MAY check:

- required fields;
- format;
- ranges;
- obvious dependencies;
- field length.

Authoritative validation remains in Application and Rule Set layers.

## 34. Validation Messages

Validation messages SHOULD be:

- associated with a field or form;
- accessible to screen readers;
- stable during correction;
- clear without exposing internal exception details.

## 35. Schema-Driven Forms

Character and Preference forms SHOULD be rendered from versioned schemas.

The renderer MUST support:

- sections;
- field types;
- requiredness;
- conditional visibility;
- read-only fields;
- derived fields;
- help text;
- validation;
- localization keys.

## 36. Schema Renderer Boundary

The schema renderer maps schema contracts to UI controls.

It MUST NOT evaluate arbitrary executable expressions.

Only supported declarative conditions are permitted.

## 37. Dialogs

Dialogs SHOULD use typed contracts.

Example:

```text
ConfirmEndSessionRequest
    → Confirmed | Cancelled

ConfirmAdvancementRequest
    → Confirmed | Cancelled

SelectImportModeRequest
    → InspectOnly | ImportAsNew | Cancelled
```

## 38. Dialog Service

The dialog service MUST:

- preserve modal ordering;
- support keyboard use;
- restore focus;
- avoid nested unbounded dialogs;
- expose typed results.

## 39. Critical Confirmation

Destructive or irreversible actions require explicit confirmation.

Examples:

- delete Campaign;
- restore backup over active state;
- apply migration;
- spend progression;
- remove credential;
- discard unsent draft.

## 40. Notifications

Chronicle distinguishes:

```text
Inline Message
Toast
Persistent Banner
Modal Dialog
Desktop Notification
```

## 41. Notification Selection

Use:

- inline messages for local field or section feedback;
- toast for transient success;
- persistent banner for degraded or recoverable state;
- modal dialog for required decisions;
- desktop notification for background completion when the application is not focused.

## 42. Critical Failure Rule

Critical failures MUST NOT rely on a toast alone.

## 43. Error Presentation

Every Application error SHOULD map to a safe Presentation error model:

```text
Title
Explanation
DataPreservationState
Retryability
SuggestedAction
ReferenceCode
DiagnosticsAvailable
```

## 44. Exception Boundary

Unexpected exceptions SHOULD be captured by a Presentation error boundary.

The UI MUST NOT display raw stack traces by default.

## 45. Recovery UI

Recovery states SHOULD provide explicit actions such as:

```text
Retry
Resume
UseExistingResult
Refresh
OpenDiagnostics
RepairConfiguration
EnterSafeMode
RestoreBackup
Cancel
```

## 46. Ambiguous Commit

When completion is unknown, the UI MUST query Operation status before offering a duplicate action.

## 47. Transcript Component

The transcript SHOULD:

- render accepted Messages in deterministic order;
- support virtualization;
- preserve accessibility order;
- distinguish roles;
- distinguish provisional output;
- support navigation to referenced Messages;
- retain selection where practical.

## 48. Provisional Narrative

Streaming or provisional narrative MUST:

- use separate state;
- avoid persistent Message identity until accepted;
- disappear or transform safely after validation;
- never expose unvalidated structured events.

## 49. Dice Roll Component

The Roll component MUST:

- show visible request data;
- expose one explicit Roll action;
- call Application command;
- disable duplicate execution;
- render persisted result;
- show continuation state;
- explain when retry will not reroll.

## 50. Dice Animation

Animation follows the authoritative result.

Animation does not create or modify Dice values.

Reduced-motion mode MUST provide an immediate textual result.

## 51. Hidden Information

Hidden mechanics and Secrets MUST be absent from player-facing DTOs.

Presentation code MUST NOT receive them and then hide them visually.

## 52. Character Sheet Component

The Character Sheet SHOULD support:

- section navigation;
- editable and read-only distinction;
- derived-value explanation;
- state-change indicators;
- keyboard navigation;
- responsive layout;
- schema-version awareness.

## 53. Memory Timeline Component

The Memory view SHOULD support:

- deterministic ordering;
- status;
- scope;
- origin Session;
- remembered-by;
- active versus archived distinction;
- filtering without changing authoritative state.

## 54. Relationship Component

Relationships MUST preserve directionality in labels and layout.

## 55. Knowledge Component

Character Knowledge MUST show epistemic state without revealing canonical truth.

## 56. Finalization Component

Finalization UI SHOULD display a staged operation model.

It MUST remain available after navigation and restart when work is durable.

## 57. Settings and Preferences Separation

Application Settings and Campaign Preferences MUST use separate routes, ViewModels, and visual grouping.

## 58. Credentials UI

Credential UI MUST:

- use protected entry;
- never redisplay stored values;
- support replace and delete;
- expose alias and health only;
- avoid copying credentials to clipboard;
- avoid logging entry state.

## 59. Styling

Chronicle SHOULD use centralized design tokens for:

- spacing;
- typography;
- shape;
- elevation;
- semantic color;
- motion duration;
- focus indicators.

## 60. Semantic Colors

Color tokens SHOULD represent meaning such as:

```text
Primary
Surface
Success
Warning
Error
Information
Focus
Disabled
```

No Domain state may depend solely on color.

## 61. Theme Support

The UI SHOULD support:

- system theme;
- light theme;
- dark theme.

Theme selection is a User Experience Preference.

## 62. Resource Dictionaries

Shared styles and templates SHOULD use organized resource dictionaries.

Feature-specific resources remain near the feature.

A single global resource file should not become a dumping ground.

## 63. Reusable Controls

Reusable controls SHOULD be semantic where behavior is Chronicle-specific.

Examples:

```text
OperationStatusBanner
RollRequestCard
CampaignCard
MemoryTimelineItem
AdvancementOptionCard
ProviderHealthBadge
```

## 64. Control Libraries

Third-party Avalonia control libraries require review for:

- license;
- accessibility;
- maintenance;
- theming;
- cross-platform behavior;
- testability.

## 65. Localization

All static UI text SHOULD use localization keys.

Machine-readable keys remain invariant.

Generated narrative language follows Campaign configuration and is not part of UI localization.

## 66. Formatting

Presentation services SHOULD format:

- dates;
- times;
- durations;
- numbers;
- percentages;
- progression values;
- file sizes.

Formatting MUST not change stored values.

## 67. Accessibility Baseline

Core workflows MUST support:

- keyboard-only navigation;
- visible focus;
- screen-reader labels;
- logical tab order;
- semantic status announcements;
- no color-only meaning;
- scalable text;
- reduced motion;
- text equivalents for Dice and progress.

## 68. Focus Management

Focus SHOULD move deliberately after:

- route navigation;
- dialog open;
- dialog close;
- validation failure;
- Roll request;
- critical error;
- Scene transition where appropriate.

## 69. Live Regions

Operation-status changes SHOULD use accessible live-region semantics where supported.

Announcements must be useful and not excessively noisy.

## 70. Keyboard Shortcuts

Shortcuts MAY include:

```text
Submit Player Input
Focus Transcript
Open Character Sheet
Open Active Roll
Cancel Current Dialog
Navigate Back
```

Every shortcut must have a nonshortcut path.

## 71. Responsive Layout

Chronicle is desktop-first but must support a practical range of window sizes.

At narrow widths:

- side panels collapse;
- secondary metadata moves below;
- player input and Roll action remain primary;
- critical status remains visible.

## 72. Multi-Window Policy

The MVP uses one primary window.

Secondary windows MAY be introduced later for:

- Character Sheet;
- diagnostics;
- settings;
- Director tools.

ViewModels and Application gateways remain reusable.

## 73. Window State

Window position and size MAY be persisted locally.

Invalid or off-screen positions MUST recover to a visible default.

## 74. Draft Preservation

Unsubmitted player input SHOULD be preserved when:

- navigating within the Campaign;
- refreshing a stale query;
- recovering from provider failure;
- closing accidentally where practical.

It MUST NOT be submitted automatically.

## 75. Clipboard Policy

Copy is explicit.

Hidden or credential content MUST NOT be copied automatically.

## 76. Drag and Drop

Drag-and-drop handlers adapt files into validated Application workflows.

Views MUST NOT parse imports directly.

## 77. File Dialogs

File dialogs return user-selected paths to Application services.

The UI does not assume that a selected path is valid or safe.

## 78. Desktop Notifications

Desktop notifications SHOULD contain minimal private information.

Default text SHOULD avoid:

- Secret names;
- full Campaign narrative;
- Character private data.

## 79. Event Subscription

ViewModels SHOULD subscribe only to scoped, typed UI or Application events.

Subscriptions MUST be disposed.

A global untyped event bus is discouraged.

## 80. UI Event Stream

An in-process UI event stream MAY notify:

- operation status changed;
- active Campaign changed;
- query invalidated;
- provider health changed;
- background work completed.

It MUST not carry mutable Domain entities.

## 81. ViewModel Lifetime

Recommended lifetimes:

```text
Application Shell ViewModel
    long-lived

Top-Level Route ViewModel
    route-scoped

Dialog ViewModel
    dialog-scoped

Reusable Item ViewModel
    parent-scoped
```

No ViewModel may assume process lifetime unless explicitly designed for it.

## 82. Disposal

ViewModels owning subscriptions, timers, or cancellation sources MUST implement disposal.

## 83. Timers

UI timers are allowed for presentation only.

They MUST NOT drive Domain transitions or operation retries.

## 84. Retry

Retry logic belongs primarily to Application and Infrastructure.

The UI decides whether to request retry based on typed retry guidance.

## 85. Data Binding

Bindings SHOULD be compiled or strongly checked where Avalonia supports it.

Binding errors SHOULD be visible in development diagnostics.

## 86. Converter Policy

Value converters SHOULD remain presentation-only and deterministic.

Complex logic belongs in ViewModels or formatting services.

## 87. Service Locator Prohibition

Views and ViewModels MUST NOT resolve dependencies from a global service provider.

Constructor injection is required, except where framework-created Views use a controlled View locator.

## 88. View Locator

A View locator MAY map ViewModel types to Views.

It MUST:

- use explicit registration or safe convention;
- avoid arbitrary reflection over untrusted assemblies;
- fail clearly when mapping is missing.

## 89. Design-Time Data

Design-time data MAY be used for layout development.

It MUST be synthetic and excluded from production behavior.

## 90. UI Logging

Presentation logs SHOULD record:

- route;
- action key;
- OperationId;
- state transition;
- safe error code;
- duration.

They MUST NOT log:

- player input text;
- transcript content;
- credentials;
- hidden information;
- full file paths.

## 91. Performance

UI implementation SHOULD support:

- transcript virtualization;
- paged Memories;
- incremental query refresh;
- cancellation of obsolete loads;
- bounded item ViewModels;
- lazy details.

## 92. Query Cancellation

When the user navigates away or changes filter, obsolete query operations SHOULD be canceled.

Durable Application work is not canceled merely because the View changed.

## 93. UI Testing Strategy

Chronicle will use:

```text
ViewModel Unit Tests
Component Tests
Headless Avalonia Tests
Accessibility Smoke Tests
Desktop Process Smoke Tests
Visual Regression Tests for Selected Components
```

## 94. ViewModel Test Requirements

ViewModel tests SHOULD prove:

- initial state;
- loading state;
- successful query;
- empty state;
- error state;
- command enablement;
- duplicate-click prevention;
- stale refresh;
- cancellation;
- recovery action;
- disposal.

## 95. Component Test Requirements

Component tests SHOULD prove:

- bindings;
- visible states;
- focus;
- keyboard interaction;
- semantic labels;
- reduced motion;
- narrow layout;
- error presentation.

## 96. Required UI Test Cases

Tests MUST cover:

- Campaign list load;
- Campaign selection;
- Character form validation;
- provider wait;
- provider failure;
- transcript ordering;
- provisional Message;
- Roll request;
- keyboard Roll;
- resolved Roll;
- no-reroll recovery;
- Character Sheet access;
- finalization progress;
- persistent finalization error;
- Memory timeline;
- stale advancement;
- Preference confirmation;
- credential replacement;
- offline state;
- ambiguous commit;
- navigation back;
- draft preservation;
- dialog focus restoration;
- screen-reader status;
- reduced-motion result;
- off-screen window recovery.

## 97. Architecture Tests

Architecture tests MUST reject:

- Avalonia references outside Desktop;
- repository or DbContext references from ViewModels;
- provider SDK references from Desktop Presentation;
- Rule Set implementation references from feature ViewModels;
- direct filesystem access from Views;
- service locator use;
- UI references from Application or Domain.

## 98. Prohibited Patterns

### 98.1 ViewModel as Repository

A ViewModel does not retain or mutate persistence entities.

### 98.2 Code-Behind Business Logic

Code-behind does not execute use cases directly except through bound or injected Presentation abstractions.

### 98.3 Boolean State Explosion

Long workflows use explicit state models rather than unrelated flags.

### 98.4 Generic Global Event Bus

Events are typed, scoped, and disposable.

### 98.5 UI Hides Secret Data

Hidden data must not arrive in the player-facing model.

### 98.6 Client-Side Dice

Presentation never generates authoritative randomness.

### 98.7 Blocking Async Work

The UI thread never waits synchronously for Application work.

### 98.8 Automatic Retry Without Operation Semantics

The UI follows typed retry guidance and preserves OperationId.

### 98.9 Static Service Access

No global service locator or mutable singleton Campaign state.

### 98.10 Toast-Only Critical Errors

Critical recovery remains persistent.

## 99. Consequences

### Positive

- ViewModels remain testable;
- UI technology stays isolated;
- state ownership is clear;
- long operations are understandable;
- recovery semantics remain visible;
- accessibility becomes architectural rather than cosmetic;
- duplicate actions are easier to prevent;
- future Presentation clients can reuse Application contracts.

### Negative

- explicit mapping adds code;
- typed dialogs and navigation require infrastructure;
- operation-state models are more verbose than simple booleans;
- accessibility testing adds delivery cost;
- strict separation may feel slower during early prototyping.

## 100. Risks

### ViewModel Overgrowth

Mitigation:

- one ViewModel per screen or bounded component;
- extract presentation services;
- organize by feature;
- avoid putting Domain logic in ViewModels.

### Navigation Complexity

Mitigation:

- typed routes;
- small route service;
- no arbitrary nested navigation model in MVP.

### Avalonia Platform Differences

Mitigation:

- cross-platform component tests;
- first-platform validation;
- semantic abstractions;
- avoid platform-specific behavior in feature ViewModels.

### Accessibility Gaps

Mitigation:

- accessibility spike;
- automated smoke tests;
- manual keyboard and screen-reader validation;
- avoid custom controls where native semantics suffice.

## 101. Technology Spike

Before acceptance, implement:

1. shell with route navigation;
2. Campaign list ViewModel;
3. live transcript with virtualization;
4. player input with draft preservation;
5. provider wait state;
6. Roll request and authoritative result;
7. finalization progress view;
8. critical recovery banner;
9. keyboard-only flow;
10. reduced-motion behavior;
11. typed dialog;
12. architecture test proving no repository access.

## 102. Spike Acceptance

The spike passes when:

- the UI remains responsive;
- a duplicate Roll click is prevented;
- retry preserves OperationId;
- hidden test data never reaches the ViewModel;
- keyboard navigation completes the Roll flow;
- finalization status survives route changes;
- a ViewModel can be tested without Avalonia runtime where practical;
- no Application or Domain project references Avalonia.

## 103. Definition of Compliance

An implementation complies when:

- Views remain presentation-only;
- ViewModels use constructor injection;
- Application commands and queries are explicit;
- OperationId is preserved across retries;
- long work has typed visible state;
- optimistic state does not claim authoritative completion;
- navigation uses stable routes;
- dialogs return typed results;
- hidden data is absent from player DTOs;
- core flows are keyboard-accessible;
- errors map to safe persistent UI;
- UI tests cover recovery and accessibility;
- architecture tests enforce the boundary.

## 104. Deferred Decisions

Later ADRs MAY define:

- detailed design tokens and visual language;
- localization file format;
- desktop navigation implementation library;
- transcript virtualization component;
- accessibility target standard;
- animation system;
- multi-window architecture;
- desktop notification implementation.

## 105. Final Decision

Chronicle Desktop will use XAML-first Avalonia Views and CommunityToolkit.Mvvm ViewModels.

Presentation will interact with Chronicle through explicit command, query, operation-status, navigation, dialog, and platform-service boundaries.

The UI may decide how Chronicle feels.

It may never decide what Chronicle means.
