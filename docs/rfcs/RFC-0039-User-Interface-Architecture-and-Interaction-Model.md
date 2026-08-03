---
id: RFC-0039
title: User Interface Architecture and Interaction Model
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
  - RFC-0038
---

> **"The interface should make the story feel immediate while keeping every mechanical and historical boundary visible when it matters."**

# User Interface Architecture and Interaction Model

## Abstract

This RFC defines the architecture and interaction model of Chronicle's official user interface.

It establishes:

- information architecture;
- navigation;
- Campaign selection;
- Campaign creation;
- Character creation;
- Session preparation;
- live Scene interaction;
- Narrative Intelligence states;
- Dice Roll interruption;
- result continuation;
- Character Sheet interaction;
- Memories;
- Relationships;
- Character Knowledge;
- Session finalization;
- progression;
- Preferences;
- background operations;
- errors;
- recovery;
- diagnostics;
- accessibility;
- responsive layout;
- desktop-first scope.

The interface renders Application query models and submits Application commands.

It MUST NOT become a second Domain model, a direct persistence client, or an authority over mechanics.

The MVP is optimized for one player operating one local desktop application with one Player Character per Campaign.

## 1. Purpose

Chronicle's user interface must support two apparently conflicting goals:

1. make play feel fluid and narrative;
2. preserve clarity about mechanics, state, waiting, recovery, and authority.

A weak interface could:

- hide when the Narrator is waiting for a Roll;
- allow duplicate commands;
- imply that generated prose is already persisted;
- confuse Character Knowledge with Campaign truth;
- hide finalization failures;
- let stale previews authorize advancement;
- expose Secrets accidentally;
- overwhelm the player with architecture details;
- bury the story under system panels;
- make recovery feel like starting over.

This RFC defines how the UI presents Chronicle without weakening its contracts.

## 2. Scope

This RFC defines:

- UI architectural principles;
- application shell;
- navigation;
- view models;
- command submission;
- operation status;
- optimistic interaction;
- Campaign workflows;
- live Session workflows;
- Dice Roll UX;
- Character Sheet UX;
- Memory and Knowledge UX;
- finalization UX;
- progression UX;
- Preferences UX;
- background-operation UX;
- error and recovery UX;
- accessibility;
- desktop layout;
- testing;
- MVP decisions.

This RFC does not define:

- final visual branding;
- one UI framework;
- exact typography;
- final color palette;
- mobile UX;
- multiplayer UX;
- streaming overlays;
- animation assets;
- exact illustration style.

## 3. Core Principle

The interface is a projection and command surface.

```text
Application Query Models
    → UI rendering

User Intention
    → Application Commands

Operation Status
    → UI feedback
```

The UI does not own authoritative Campaign state.

## 4. UI Responsibilities

The interface owns:

- presentation;
- interaction;
- navigation;
- local draft state;
- accessibility;
- progress visualization;
- confirmation;
- safe error explanation;
- explicit player decisions.

It does not own:

- Campaign invariants;
- Rule Set validation;
- Dice randomness;
- progression cost;
- persistence;
- idempotency;
- visibility authority.

## 5. Interaction Principles

Chronicle adopts:

### 5.1 Story First

The active narrative remains visually primary during play.

### 5.2 State Clarity

The UI clearly shows whether Chronicle is:

- ready for input;
- waiting for provider output;
- waiting for a Dice Roll;
- resolving mechanics;
- applying consequences;
- finalizing;
- recovering.

### 5.3 One Authoritative Action

The player should understand what action is currently authoritative.

### 5.4 No Hidden Commit

The UI should not imply that provisional output has been accepted before commit.

### 5.5 Recovery Without Repetition

When an operation committed but confirmation failed, the UI should recover the existing result rather than invite duplicate action.

### 5.6 Progressive Disclosure

The story remains simple by default.

Mechanics, diagnostics, and calculation details are available when useful.

### 5.7 Visibility Safety

Hidden information is filtered before reaching presentation models.

## 6. Application Shell

The desktop shell SHOULD include:

```text
Global Navigation
Main Content Region
Contextual Side Panel
Operation Status Area
Notification Area
Modal or Sheet Layer
```

## 7. Global Navigation

Initial destinations SHOULD include:

```text
Campaigns
Active Campaign
Character
Memories
Settings
Diagnostics
```

Diagnostics MAY be hidden under advanced settings in MVP.

## 8. Campaign-Scoped Navigation

Within one Campaign, navigation MAY include:

```text
Play
Character
Chronicle
Memories
Relationships
Knowledge
Progression
Preferences
```

The MVP MAY combine some destinations.

## 9. Navigation State

Navigation state is UI-local.

The application persists only what has product value, such as the last opened Campaign when appropriate.

## 10. Application Query Models

Every screen SHOULD render a purpose-specific view model.

Examples:

```text
CampaignListView
CampaignOverviewView
ActiveSessionView
ActiveSceneView
CharacterSheetView
MemoryTimelineView
RelationshipView
KnowledgeView
ProgressionView
PreferenceView
PendingOperationView
DiagnosticsView
```

## 11. View Model Rules

View models SHOULD:

- contain already-filtered visibility;
- contain display-ready status;
- preserve stable identifiers;
- avoid exposing persistence entities;
- include versions needed for commands;
- distinguish unavailable from empty;
- include safe action permissions.

## 12. Action Permission

A view model MAY expose whether an action is currently available.

The Application layer MUST revalidate on command execution.

UI permission is guidance, not authority.

## 13. Command Submission

Every state-changing UI action SHOULD:

1. validate local form shape;
2. create or reuse OperationId;
3. submit an Application command;
4. disable duplicate submission where appropriate;
5. display operation state;
6. handle completion or recovery.

## 14. Duplicate Submission

The UI SHOULD prevent obvious duplicate clicks.

The Application layer still enforces idempotency.

## 15. Local Optimism

The UI MAY optimistically update local presentation for low-risk, reversible state.

It MUST NOT optimistically claim success for:

- Dice Roll;
- progression spend;
- Session finalization;
- Preference migration;
- restore;
- import;
- Character schema migration.

## 16. Loading States

Loading states SHOULD distinguish:

```text
InitialLoading
Refreshing
WaitingForProvider
WaitingForPlayer
Applying
Recovering
Unavailable
```

A generic spinner alone is insufficient for long workflows.

## 17. Campaign List

The Campaign list SHOULD show:

- title;
- status;
- Rule Set;
- Player Character;
- last played;
- active Session state;
- pending recovery;
- archive state.

It MUST avoid hidden Campaign content.

## 18. Empty State

The empty Campaign list SHOULD offer:

- create Campaign;
- import Campaign;
- restore from backup;
- open documentation.

The MVP may prioritize create and import.

## 19. Campaign Creation Flow

Campaign creation SHOULD be a staged workflow:

```text
Choose Rule Set
    ↓
Choose Campaign Preferences
    ↓
Create or Import Character
    ↓
Define Campaign Premise Inputs
    ↓
Generate Campaign Proposal
    ↓
Review and Confirm
    ↓
Persist Campaign
```

## 20. Creation Draft

Campaign setup remains a draft until confirmed.

Generated proposals MUST NOT become persisted Campaign truth automatically.

## 21. Campaign Generation Review

The review SHOULD show:

- premise;
- initial setting;
- main tensions;
- planned opening;
- visible NPCs;
- selected Preferences;
- warnings;
- regeneration or editing options.

Hidden Plan details MAY remain collapsed or Director-only according to product policy.

## 22. Character Creation

Character creation SHOULD be schema-driven.

The UI renders fields from RFC-0028 while preserving Rule Set validation.

## 23. Character Form

A Character form SHOULD support:

- sections;
- field descriptions;
- required indicators;
- conditional fields;
- typed input;
- validation messages;
- point or budget summary;
- creation progress;
- draft save.

## 24. Character Validation

The UI MAY perform immediate client-side validation for responsiveness.

The Application and Rule Set layers perform authoritative validation.

## 25. Derived Fields

Derived Character fields SHOULD be visibly distinguished from editable fields.

The player SHOULD be able to inspect how a derived value was calculated when the Rule Set provides an explanation.

## 26. Character Narrative Profile

Personality and personal-history fields SHOULD not be presented as secondary technical metadata.

They are core Character context.

The UI SHOULD support:

- traits;
- beliefs;
- fears;
- goals;
- important history;
- relationships;
- boundaries.

## 27. Campaign Overview

The overview SHOULD present:

- current Campaign status;
- active Session;
- Player Character summary;
- latest Memories;
- unresolved operations;
- next available action;
- backup or integrity warnings.

## 28. Start Session

Starting a Session requires:

- Campaign active;
- no conflicting finalization;
- required Rule Set available;
- Player Character valid;
- no blocking migration;
- no unresolved critical operation.

The UI presents blocking reasons separately.

## 29. Live Play Layout

The live play screen SHOULD prioritize:

```text
Narrative Transcript
Player Input
Current Operation State
Dice Roll Action When Required
Contextual Character and Scene Information
```

## 30. Transcript

The transcript SHOULD display accepted Messages in deterministic order.

Message roles MAY include:

- Player;
- Narrator;
- System;
- Rule Result;
- Recovery Notice.

## 31. Provisional Output

Streaming or provisional provider output MUST be visually distinguishable from accepted Messages.

If validation fails, provisional output may disappear or be replaced with a safe recovery state.

## 32. Message Identity

UI rendering SHOULD use stable Message identifiers.

It MUST not infer uniqueness from text or timestamp.

## 33. Player Input

Player input SHOULD support:

- multiline text;
- draft preservation;
- submission;
- disabled state during incompatible operations;
- clear retry behavior;
- keyboard accessibility.

## 34. Input Availability

Player input SHOULD be unavailable when Chronicle is:

- waiting for a mandatory Roll;
- applying a critical result;
- finalizing;
- performing a blocking migration;
- recovering an ambiguous operation.

The UI should explain why.

## 35. Provider Waiting State

When waiting for Narrative Intelligence, the UI SHOULD show:

- capability in progress;
- elapsed state without promising exact completion;
- cancellation availability;
- local actions still available;
- provider health when degraded.

## 36. Provider Failure

A provider failure SHOULD present:

- safe error summary;
- whether input was saved;
- whether retry is safe;
- whether another provider profile may be selected;
- whether local mechanics remain usable.

## 37. Dice Roll Interruption

When the Narrator requests a Roll:

1. accepted narration ends at the uncertainty point;
2. the input area becomes unavailable if required;
3. a structured Roll card appears;
4. the player reviews the pool and stakes according to visibility;
5. the player explicitly rolls;
6. Chronicle generates and persists randomness;
7. the result appears;
8. narration continues.

## 38. Roll Card

A Roll card SHOULD contain:

- action label;
- actor;
- target when visible;
- pool;
- modifiers;
- difficulty when visible;
- stakes;
- Roll button;
- explanation affordance;
- status.

## 39. Roll Authority

The Roll button calls Chronicle.

It MUST NOT trigger client-side random generation.

## 40. Roll Animation

Animation MAY visualize a result after Chronicle has generated it.

Animation MUST NOT determine the result.

## 41. Roll Button State

Possible states:

```text
Ready
Rolling
Resolved
Applying
WaitingForNarration
RecoveryRequired
```

## 42. Roll Result

The result SHOULD show:

- generic outcome;
- Rule Set-specific result;
- raw dice when visible;
- accepted consequences;
- calculation explanation;
- continuation state.

## 43. Roll Retry

If raw values were persisted but narration failed, the UI MUST say that retrying will not reroll.

## 44. Hidden Mechanics

Hidden modifiers or target details MUST not be present in the player-facing view model.

The UI MUST not merely hide them with CSS.

## 45. Character Sheet During Play

The Character Sheet SHOULD remain accessible without leaving the active Session context completely.

Possible implementation:

- side panel;
- secondary view;
- modal;
- separate window later.

## 46. Character State Changes

State changes SHOULD appear as trusted updates.

Examples:

- resource loss;
- condition applied;
- track increased;
- temporary modifier.

The UI SHOULD identify the source operation when useful.

## 47. Scene Information

The live view MAY show:

- Scene title;
- location;
- visible participants;
- current objective;
- active visible conditions;
- Act and Session context.

It MUST not expose future Plan content.

## 48. Scene Transition

A Scene transition SHOULD be visually clear.

The transcript history remains accessible.

The UI SHOULD avoid making Scene transition look like a new Campaign or lost context.

## 49. Act Transition

Act transitions are less frequent and MAY use stronger visual separation.

The canonical hierarchy remains:

```text
Campaign → Session → Act → Scene
```

## 50. End Session

Ending a Session is explicit.

The UI SHOULD show:

- unresolved Roll warnings;
- pending operation warnings;
- confirmation;
- finalization status;
- whether input becomes locked.

## 51. Finalization View

Finalization SHOULD expose stages such as:

```text
Collecting Evidence
Consulting Archivist
Validating Changes
Applying Memories
Applying Progression
Updating Relationships and Knowledge
Aging Memories
Completing Session
```

This may be simplified visually while preserving useful recovery information.

## 52. Finalization Review

Depending on Rule Set and product policy, the player MAY review:

- Session summary;
- new Memories;
- progression Awards;
- visible Relationship changes;
- visible Knowledge changes;
- Character State changes.

Hidden information remains filtered.

## 53. Finalization Failure

The UI MUST show whether:

- no changes were applied;
- some deterministic state was already committed;
- retry is safe;
- user intervention is required;
- the Session remains active, finalizing, or pending.

## 54. Session Summary

A completed Session SHOULD have a readable summary containing:

- title or sequence;
- important events;
- new Memories;
- visible progression;
- key Character changes;
- next Campaign state.

It is not a raw transcript summary alone.

## 55. Memories View

The Memories interface SHOULD present Campaign Memories as meaningful lived experiences.

It MAY support:

- timeline;
- filtering;
- status;
- permanent or temporary;
- age;
- scope;
- involved Characters;
- remembered-by;
- origin Session.

## 56. Memory Status

The UI SHOULD distinguish:

```text
Active
Dormant
Archived
Expired
Superseded
```

Expired does not mean deleted.

## 57. Memory Explanation

The player MAY inspect:

- why the Memory matters;
- when it was created;
- who remembers it;
- how long it remains active;
- related Sessions.

## 58. Relationship View

The Relationship view SHOULD preserve directionality.

It may show:

- source and target;
- dimensions;
- summary;
- visible changes;
- related Memories;
- current status.

## 59. Knowledge View

The Knowledge view MUST distinguish:

```text
Known
Believed
Suspected
Misunderstood
Unknown
```

It MUST not show canonical Campaign truth unless the Character legitimately knows it.

## 60. Secret-Safe Presentation

The interface SHOULD render only projections already filtered by the Application layer.

No hidden canonical field should be present in a player-facing DTO.

## 61. Progression View

The progression interface SHOULD show:

- current balances;
- recent Awards;
- available advancement options;
- costs;
- prerequisites;
- unavailability reasons;
- advancement history.

## 62. Advancement Preview

A preview is informative.

Before commit, the UI MUST show:

- selected advancement;
- current value;
- target value;
- cost;
- remaining balance;
- prerequisites;
- side effects;
- confirmation.

## 63. Stale Advancement

If Character state or cost changes before commit:

- the command fails safely;
- the UI refreshes;
- changed cost is shown;
- confirmation is requested again.

## 64. Progression Suggestions

Narrative Intelligence suggestions MAY be displayed as nonauthoritative recommendations.

They MUST be visually distinct from Rule Set-validated options.

## 65. Preferences View

Campaign Preferences SHOULD be grouped by:

```text
Core Rules
Difficulty
Character Creation
Progression
Optional Systems
Narrative
Visibility
Advanced
```

## 66. Preference Change Preview

Before a material change, the UI SHOULD show:

- current value;
- requested value;
- affected mechanics;
- Character impact;
- migration requirement;
- restart or lifecycle requirement;
- reversibility.

## 67. Preference Mutability

The UI MUST respect statuses such as:

```text
CreationOnly
BeforeFirstSession
BetweenSessions
AnyTimeWhenIdle
MigrationOnly
ReadOnly
```

The Application layer revalidates.

## 68. House Rule Presentation

House Rules SHOULD be clearly labeled as changes to default system behavior.

The UI SHOULD provide:

- default behavior summary;
- changed behavior summary;
- impact warning;
- compatibility information.

## 69. Settings View

Application Settings SHOULD remain separate from Campaign Preferences.

Settings MAY include:

- provider profiles;
- credentials;
- storage;
- backups;
- Rule Knowledge;
- diagnostics;
- appearance;
- developer mode.

## 70. Secret Entry

Credential entry SHOULD:

- use protected input;
- never display stored secret value;
- support replacement;
- support deletion;
- support safe test;
- show only alias and status afterward.

## 71. Provider Profile UI

A provider profile SHOULD show:

- adapter;
- local or remote;
- capability mappings;
- credential status;
- endpoint status;
- privacy policy summary;
- health;
- test action.

## 72. Background Operations

A global operations area SHOULD show:

- active operation;
- phase;
- progress;
- cancellation;
- retry;
- blocking state;
- related Campaign.

## 73. Notification Model

Chronicle MAY use:

- inline status;
- toast;
- persistent banner;
- dialog;
- desktop notification.

The choice depends on severity and required action.

## 74. Notification Severity

Suggested levels:

```text
Information
Success
Warning
Error
Critical
```

Critical is operational, not narrative.

## 75. Toast Prohibitions

Transient toast alone is insufficient for:

- failed finalization;
- failed restore;
- missing Rule Set package;
- migration failure;
- security violation;
- ambiguous commit.

These require persistent status.

## 76. Error Presentation

An error view SHOULD include:

- safe title;
- safe explanation;
- data-preservation status;
- retryability;
- user action;
- reference identifier;
- diagnostics option when appropriate.

## 77. Recovery Actions

Possible recovery actions:

```text
Retry
Refresh
Use Existing Result
Resume
Select Provider
Repair Configuration
Open Diagnostics
Enter Safe Mode
Restore Backup
Cancel
```

## 78. Ambiguous Commit UX

When commit status is uncertain, the UI SHOULD display:

```text
Checking whether the operation completed...
```

It MUST not immediately offer a duplicate action.

## 79. Offline UX

Offline mode SHOULD clearly distinguish:

- local features available;
- remote provider unavailable;
- pending operations;
- local provider status;
- safe retry after connectivity returns.

## 80. Empty, Error, and Unavailable States

Every major view SHOULD define:

- loading;
- empty;
- content;
- degraded;
- error;
- unavailable;
- permission-blocked.

## 81. Accessibility

The MVP SHOULD target strong keyboard and screen-reader accessibility.

Requirements include:

- keyboard navigation;
- visible focus;
- semantic controls;
- accessible labels;
- status announcements;
- no color-only meaning;
- scalable text;
- reduced-motion support;
- adequate contrast.

## 82. Dice Accessibility

Dice results MUST be available as text.

Animation is optional.

The Roll action must be keyboard-operable.

## 83. Narrative Accessibility

Transcript content SHOULD support:

- selectable text;
- screen-reader order;
- message-role labels;
- adjustable text size;
- clear paragraph spacing;
- no required hover interaction.

## 84. Motion

Motion SHOULD reinforce state changes.

It MUST not:

- determine mechanics;
- conceal delays;
- block interaction unnecessarily;
- ignore reduced-motion preference.

## 85. Responsive Desktop Layout

The interface SHOULD support a practical range of desktop window sizes.

At smaller widths:

- side panels may collapse;
- secondary metadata may move below content;
- narrative and Roll actions remain primary.

## 86. Minimum Window Behavior

The application SHOULD define a usable minimum size.

Below it, the UI should scroll or reflow rather than overlap critical controls.

## 87. Design System

Chronicle SHOULD maintain reusable UI primitives for:

- buttons;
- inputs;
- cards;
- status;
- dialogs;
- tables;
- timelines;
- message bubbles;
- Roll cards;
- validation;
- banners;
- navigation.

## 88. Semantic Components

Domain-relevant components SHOULD use semantic names.

Examples:

```text
CampaignCard
SceneTranscript
RollRequestCard
MemoryTimeline
AdvancementOptionCard
OperationStatusBanner
```

## 89. Localization

UI text SHOULD use localization keys.

Persisted machine keys remain language-neutral.

Generated narrative may use Campaign language preferences.

## 90. Date and Number Formatting

Dates, numbers, durations, and currency-like progression values SHOULD use locale-aware formatting without changing stored semantics.

## 91. Performance

The UI SHOULD remain responsive with long Campaign histories.

Strategies MAY include:

- transcript virtualization;
- paged Memory timelines;
- lazy detail loading;
- bounded query models;
- incremental refresh.

## 92. Transcript Virtualization

Virtualization MUST preserve:

- deterministic order;
- accessibility;
- navigation to referenced Messages;
- selection where practical.

## 93. Read Model Refresh

The UI SHOULD refresh purpose-specific view models after committed operations.

It MUST not mutate stale cached entities locally.

## 94. Real-Time Updates

Within one desktop process, operation updates MAY use:

- local event stream;
- observable state;
- polling;
- query invalidation.

The implementation must remain replaceable.

## 95. Testing Strategy

### 95.1 Component Tests

Test reusable UI components and states.

### 95.2 View Model Tests

Test visibility-safe mapping, action permissions, empty states, and status representation.

### 95.3 Interaction Tests

Test Campaign creation, live play, Roll flow, finalization, progression, and Preferences.

### 95.4 Accessibility Tests

Test keyboard operation, labels, focus, announcements, contrast, reduced motion, and text scaling.

### 95.5 Recovery Tests

Test stale responses, lost confirmation, provider failure, failed finalization, and offline recovery.

## 96. Required Test Cases

Tests MUST cover:

- empty Campaign list;
- Campaign creation draft;
- generated proposal review;
- schema-driven Character form;
- Character validation error;
- start Session blocked;
- accepted transcript ordering;
- provisional provider output;
- provider timeout;
- Roll request interruption;
- keyboard Roll action;
- Chronicle-generated result display;
- no reroll after continuation failure;
- hidden modifier absent from DTO;
- Scene transition;
- end Session confirmation;
- finalization progress;
- finalization failure before commit;
- finalization failure after partial workflow checkpoint;
- Memory timeline;
- directional Relationship;
- Character Knowledge without canonical truth;
- progression preview;
- stale progression cost;
- Preference change impact;
- credential replacement;
- active background operation;
- ambiguous commit recovery;
- offline mode;
- persistent critical error;
- screen-reader transcript order;
- reduced-motion Dice result;
- narrow-window layout;
- long transcript performance.

## 97. Prohibited Patterns

### 97.1 UI as Domain Authority

Presentation cannot decide mechanics or invariants.

### 97.2 Hidden Fields Sent Then Concealed

Player-facing DTOs must already be visibility-safe.

### 97.3 Client-Side Dice

The UI visualizes but does not generate authoritative randomness.

### 97.4 Generic Spinner for Every State

Long operations require meaningful status.

### 97.5 Provisional Prose Shown as Accepted

Streaming output must remain distinguishable until commit.

### 97.6 Toast-Only Critical Failure

Important failures require persistent recovery UI.

### 97.7 Stale Preview Authorizes Spend

Advancement and Preference changes are revalidated at commit.

### 97.8 Application Settings Mixed With House Rules

Installation behavior and Campaign mechanics remain separate.

### 97.9 Animation Determines Outcome

Visual effects follow accepted state.

### 97.10 Architecture Vocabulary Overwhelms Player

The interface should expose technical detail progressively, not by default.

## 98. Current Delivery Decision

The MVP adopts:

- desktop-first application shell;
- Campaign list and overview;
- staged Campaign creation;
- schema-driven Character creation;
- one primary live-play screen;
- accepted transcript;
- explicit Narrative Intelligence status;
- structured Roll card;
- Chronicle-owned Roll action;
- Character Sheet access during play;
- explicit Session finalization;
- Memories view;
- basic Relationships and Knowledge views;
- progression view;
- Campaign Preferences view;
- application Settings;
- background-operation status;
- safe persistent recovery states;
- keyboard accessibility;
- reduced-motion support;
- no multiplayer UI;
- no mobile-specific UI;
- no streaming overlay;
- no Director multi-window workspace in MVP.

## 99. Architecture Horizon

Future evolution MAY include:

- richer Campaign dashboard;
- multiple windows;
- tablet layout;
- mobile companion;
- television display;
- spectator mode;
- multiplayer presence;
- collaborative Character creation;
- streaming overlays;
- visual maps;
- voice input and output;
- customizable layout;
- theme marketplace;
- Director workspace.

The MVP MUST NOT implement these capabilities without a later milestone.

## 100. Open Questions

The following remain open:

- Which UI framework will be selected?
- What is the primary desktop navigation pattern?
- Should Character Sheet open as panel, page, or window?
- How much Roll calculation detail is visible by default?
- Should finalization changes require player review before commit?
- Which Memories filters are required in MVP?
- How should hidden NPC Relationships be represented?
- Is the Knowledge view required in the first release?
- How should Campaign generation proposals be edited?
- Should the transcript support Markdown?
- Which narrative formatting is allowed?
- How should provisional streaming output look?
- Which operations require desktop notifications?
- Is a diagnostics screen required in MVP?
- What minimum window size will be supported?
- Which accessibility standard will be used as the implementation target?
- How should local provider download and setup be presented?
- Should the official application offer a compact play mode?

These questions require design exploration, technology ADRs, usability testing, and implementation evidence.

## 101. Compliance Checklist

An implementation complies when:

- the UI renders query models rather than persistence entities;
- all writes use Application commands;
- duplicate submissions remain idempotent;
- operation state is visible;
- provisional output is distinguishable;
- Dice randomness remains Chronicle-owned;
- hidden information is absent from player-facing DTOs;
- finalization has persistent progress and recovery states;
- progression and Preferences are revalidated at commit;
- application Settings remain separate from Campaign Preferences;
- critical errors are persistent and actionable;
- accessibility is built into core flows;
- the interface remains usable offline where local features permit;
- presentation technology does not enter Domain contracts.

## 102. Final Principle

The interface should let the player inhabit the story without losing confidence in what Chronicle is doing.

The narrative must feel alive.

The mechanics must remain trustworthy.

The history must remain clear.
