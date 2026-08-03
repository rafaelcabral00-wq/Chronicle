---
id: RFC-0042
title: Official MVP Scope and Acceptance Criteria
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-08-03
category: Product
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
---

> **"The MVP is complete when one player can create a Character, live a coherent Campaign, resolve uncertainty, preserve memory, and safely continue after closing the application."**

# Official MVP Scope and Acceptance Criteria

## Abstract

This RFC defines the official Chronicle MVP.

It converts the architecture established by RFC-0000 through RFC-0041 into an explicit delivery boundary with measurable acceptance criteria.

The MVP is:

- local-first;
- single-user;
- single-player;
- desktop-first;
- one Player Character per Campaign;
- one Rule Set package complete for its declared release scope;
- one official Narrative Intelligence provider implementation;
- persistent across application restarts;
- deterministic for Dice and mechanics;
- recoverable after interrupted operations;
- explicit about Campaign Memories, Character Knowledge, Relationships, and progression;
- intentionally limited in scope.

This RFC is the authoritative answer to:

- What must exist before the MVP is considered complete?
- What may exist but is not required?
- What is explicitly excluded?
- Which workflows must pass end to end?
- Which quality and safety gates block release?
- What evidence proves acceptance?

## 1. Purpose

Chronicle has a broad architectural horizon.

Without a binding MVP boundary, the project risks:

- endless infrastructure preparation;
- speculative multiplayer support;
- premature plugin systems;
- excessive provider integration;
- multiple incomplete Rule Sets;
- unbounded UI work;
- delayed validation of the central play loop;
- scope expansion until the project becomes impossible to finish.

The MVP must validate Chronicle's central thesis:

> A local application can preserve a long-running tabletop RPG Campaign while Narrative Intelligence acts as Narrator, Chronicle owns state and chance, and Rule Set packages own deterministic mechanics.

## 2. Product Hypothesis

The MVP tests whether one player can:

1. create a valid Character;
2. create or generate a Campaign;
3. start a Session;
4. interact with a Narrator;
5. reach a mechanical uncertainty;
6. roll through Chronicle;
7. continue the narrative from the authoritative result;
8. end the Session;
9. finalize meaningful Memories and progression;
10. close the application;
11. return later without contradiction or data loss.

## 3. MVP Definition

The MVP is one complete vertical slice.

It is not a collection of disconnected technical demonstrations.

A complete MVP must integrate:

```text
Desktop Application
Campaign Domain
Character System
Narrative Intelligence
Rule Set Package
Dice Resolution
Persistence
Session Finalization
Memories
Progression
Recovery
Backup
Core UI
```

## 4. Delivery Principles

### 4.1 Complete Before Broad

One complete Rule Set is more valuable than several partial Rule Sets.

### 4.2 One Provider Before Many

One provider adapter that fully satisfies the contracts is enough for MVP.

### 4.3 Local Before Distributed

Local persistence is required. Cloud sync is not.

### 4.4 Recovery Before Convenience

A recoverable interrupted Roll is more important than rich animation.

### 4.5 Coherent Play Before Extensibility Marketplace

Framework boundaries must exist, but dynamic community loading is not required.

### 4.6 Explicit Exclusions

Features outside MVP are documented rather than half-implemented.

## 5. Target User

The MVP targets:

- one adult player;
- using one local desktop installation;
- controlling one Player Character;
- playing one Campaign at a time;
- interacting through text;
- accepting local or remote Narrative Intelligence configuration;
- willing to review generated Campaign setup and Character information.

## 6. Supported Play Mode

The MVP supports:

```text
One Human Player
One Player Character
One Chronicle-Directed Campaign
One Active Session per Campaign
Text-Based Interaction
Chronicle-Owned Dice
Rule Set-Validated Mechanics
```

## 7. Official Rule Set

The MVP MUST include one official Rule Set package complete for its declared release scope.

Effective 2026-08-03, DR-0002 defines Rule Set completeness for MVP acceptance.

A Rule Set is complete for a declared release scope when every advertised capability, mechanic, workflow, artifact, validation, test, localization requirement, security requirement, and compatibility promise within that scope is implemented and verified.

Completeness does not require implementing the entire source RPG system.

The official MVP Rule Set package MUST explicitly declare:

- supported scope;
- supported capabilities;
- excluded mechanics;
- disabled operations;
- known limitations;
- compatibility boundaries;
- evidence and validation status.

The MVP Rule Set may be accepted only when:

- its advertised scope is internally complete;
- exclusions are explicit and enforceable;
- no unavailable feature is presented as supported;
- required tests and evidence for that scope pass;
- package metadata, README content, and public status claims match actual behavior.

The initial development and validation target is:

```text
Werewolf: The Apocalypse
```

Exact edition, legal distribution model, and source-content strategy require implementation confirmation.

The package MUST be sufficient to validate the complete Chronicle architecture without distributing unauthorized proprietary text.

## 8. Rule Set Completeness

Rule Set completeness in this RFC means declared-scope completeness, not source-system completeness.

The following distinctions are normative:

- source-system completeness means the entire source RPG system is implemented;
- declared-scope completeness means all advertised support within the release scope is implemented and verified;
- implementation completeness means the declared scope has working package behavior, not only declarative artifacts;
- validation completeness means required evidence, tests, localization checks, security checks, and compatibility checks for the declared scope pass;
- publication status is separate from completeness.

The official MVP Rule Set package MUST include:

- package manifest;
- stable package identity;
- Rule Set version;
- Character Sheet schema;
- Character creation validation;
- required mechanical operations;
- Dice and Test interpretation;
- progression rules needed by MVP;
- Preferences required by MVP;
- migration identity;
- Rule Knowledge summaries or references;
- contract-test fixtures.

Any mechanic or operation outside the declared MVP Rule Set scope MUST be excluded, disabled, or hidden from supported workflows. Documentation and package metadata MUST NOT present excluded or disabled capabilities as supported.

## 9. Rule Set Legal Boundary

The MVP MUST NOT require bundling proprietary sourcebook text without authorization.

It MAY use:

- original summaries;
- user-supplied local sources;
- open or licensed material;
- stable references to legally obtained sources;
- original mechanical implementation where legally appropriate.

## 10. Narrative Intelligence

The MVP MUST include one working official provider adapter.

The adapter MUST support:

- Narrator capability;
- Archivist capability;
- Campaign Generator capability;
- structured output;
- timeout;
- cancellation;
- retry;
- repair;
- usage metadata when available;
- credential aliases;
- least-context construction.

## 11. Provider Neutrality

Core contracts MUST remain provider-neutral.

The MVP does not require multiple provider adapters.

It must prove that another adapter could be implemented without changing Domain rules.

## 12. Local Provider Support

Local Narrative Intelligence support is architecturally allowed.

It is not required for MVP unless selected as the official first provider path.

## 13. Campaign Creation

The player MUST be able to create a Campaign through a staged workflow.

The workflow MUST include:

1. select the Rule Set;
2. review Campaign Preferences;
3. create or select the Player Character;
4. provide Campaign-generation inputs;
5. request a generated Campaign proposal;
6. review the proposal;
7. confirm persistence.

## 14. Campaign Proposal

A Campaign proposal MUST be provisional until confirmed.

It SHOULD contain:

- title;
- premise;
- setting summary;
- main tensions;
- opening situation;
- initial relevant NPCs;
- visible Campaign assumptions;
- initial Narrative Plan.

## 15. Campaign Identity

A confirmed Campaign MUST receive stable identity and persist:

- Rule Set version;
- Player Character;
- Preferences;
- Narrative Plan;
- lifecycle state;
- creation metadata.

## 16. Campaign List

The MVP desktop application MUST provide a Campaign list showing at minimum:

- Campaign title;
- status;
- Rule Set;
- Player Character;
- last played;
- active or pending Session state.

## 17. Character Creation

The player MUST be able to create a Character through the Rule Set's schema.

The Character creation flow MUST support:

- required fields;
- typed values;
- conditional fields;
- Rule Set validation;
- Character Sheet sections;
- narrative profile;
- personal-history fields;
- final confirmation.

## 18. Character Completeness

The MVP Character model MUST include:

- stable identity;
- role;
- typed Character Sheet;
- Character State;
- personality traits;
- beliefs or values;
- fears or vulnerabilities;
- goals;
- important personal history;
- Rule Set and schema versions.

## 19. One Player Character

Each Campaign MUST have exactly one Player Character in MVP.

Persistent NPCs MAY exist.

Multiple human-controlled Player Characters are outside MVP.

## 20. Character Sheet

The player MUST be able to inspect the Character Sheet:

- before Session;
- during Session;
- after Session.

The UI MUST distinguish editable, derived, and state-controlled values.

## 21. Session Lifecycle

The MVP MUST support:

```text
Prepare
Start
Active
Ending
Finalizing
Completed
```

A Campaign may have at most one active Session.

## 22. Canonical Hierarchy

The implementation MUST preserve:

```text
Campaign → Session → Act → Scene
```

A Session MUST contain at least one Act and one Scene during normal play.

## 23. Session Start

The player MUST be able to start a Session when:

- Campaign is active;
- Character is valid;
- Rule Set is available;
- no blocking migration exists;
- no conflicting finalization exists;
- no unresolved critical operation blocks play.

## 24. Live Play

The MVP MUST provide a live play view with:

- accepted transcript;
- player text input;
- Narrator responses;
- operation status;
- Scene information;
- Character access;
- Roll interaction when required.

## 25. Player Input

The player MUST be able to submit text actions.

Submitted input MUST:

- receive OperationId;
- persist once accepted;
- be protected from duplicate submission;
- remain recoverable if provider continuation fails.

## 26. Narrator Response

A Narrator response MUST be:

- associated with the current Campaign, Session, Act, and Scene;
- produced from bounded context;
- structured according to the Narrator contract;
- validated before acceptance;
- persisted in deterministic Message order;
- rejected if stale.

## 27. Provisional Output

Streaming is optional.

If implemented, provisional output MUST be visually distinct from accepted history.

## 28. Scene Continuity

The Narrator MUST receive enough validated context to preserve:

- current location;
- present Characters;
- active visible conditions;
- recent Messages;
- relevant Memories;
- relevant Character Knowledge;
- relevant Relationships;
- required Rule Knowledge;
- relevant Preferences.

## 29. Scene Transition

The MVP MUST support ending one Scene and beginning another without losing Session continuity.

## 30. Act Transition

The MVP MUST support at least one explicit Act transition or be able to represent multiple Acts in a Campaign.

The first Session does not need to exhaust all Act capabilities.

## 31. Dice Request

The Narrator MAY request a Roll only through structured output.

The request MUST include:

- OperationKey;
- actor;
- target when applicable;
- stakes;
- required visible explanation;
- Rule Set context.

## 32. Roll Pause

When a mandatory Roll is requested:

- narration pauses;
- conflicting player input is blocked;
- a Roll card appears;
- the player explicitly initiates the Roll.

## 33. Chronicle-Owned Randomness

The official application MUST generate authoritative random values.

Neither Narrative Intelligence nor the Rule Set package may generate authoritative randomness.

## 34. Mechanical Resolution

The Rule Set package MUST:

- validate the operation;
- build the pool;
- validate modifiers;
- determine difficulty;
- interpret the raw Dice values;
- return structured outcome;
- return consequence proposals.

## 35. Roll Persistence

Every authoritative Roll MUST persist:

- Dice Roll identifier;
- OperationId;
- exact Rule Set version;
- OperationKey and version;
- raw values;
- pool;
- modifiers;
- difficulty;
- Preference Snapshot;
- result;
- consequences;
- timestamps.

## 36. No Reroll on Retry

If raw values were persisted and continuation later fails, retry MUST reuse the same Roll.

This is a blocking MVP acceptance criterion.

## 37. Roll Continuation

After result acceptance and consequence application, the Narrator MUST continue from the authoritative outcome.

## 38. Deterministic Mechanics

Given the same:

- exact Rule Set version;
- operation version;
- actor and target snapshots;
- modifiers;
- difficulty;
- raw values;
- Preferences;

the Rule Set MUST return the same result.

## 39. Immediate Consequences

The MVP MUST apply the immediate consequences required by the official Rule Set.

At minimum, the architecture must demonstrate one state-changing consequence such as:

- Resource change;
- Track change;
- Condition;
- temporary modifier;
- Character State update.

## 40. Transcript

The accepted transcript MUST persist across restart.

It MUST preserve deterministic Message ordering.

## 41. Session End

The player MUST be able to explicitly end a Session.

The UI MUST warn about unresolved critical operations.

## 42. Archivist Workflow

Session finalization MUST invoke the Archivist capability or a deterministic fallback explicitly approved by the architecture.

The Archivist may propose:

- Session summary;
- new Memories;
- Memory changes;
- progression evidence;
- Relationship changes;
- Character Knowledge changes;
- Secret changes;
- Character State changes.

## 43. Finalization Validation

Chronicle MUST validate every proposal before persistence.

The Archivist MUST NOT write state directly.

## 44. Finalization Atomicity

The finalization result MUST be applied atomically or through a guaranteed recoverable workflow.

The MVP MUST prevent:

- partial Memory creation;
- repeated Memory aging;
- duplicated progression;
- partial Session completion.

## 45. Session Summary

A completed Session MUST preserve a readable summary that is more meaningful than a raw transcript copy.

## 46. Campaign Memories

The MVP MUST support:

- permanent Memories;
- temporary Memories;
- importance;
- relevance;
- scope;
- age;
- lifetime;
- remembered-by;
- status;
- origin Session.

## 47. Memory Aging

Temporary Memories MUST age exactly once per successfully finalized Session.

This is a blocking acceptance criterion.

## 48. Memory Expiration

Expired Memories MUST be archived, expired, or made dormant.

They MUST NOT be silently hard-deleted.

## 49. Memory Retrieval

The Narrator MUST receive only relevant Memories within context budget.

The MVP does not require advanced semantic ranking if deterministic relevance selection is sufficient.

## 50. Relationships

The MVP MUST support persistent directional Relationships between Characters.

At least one visible Relationship change SHOULD be demonstrated in the acceptance Campaign.

## 51. Character Knowledge

The MVP MUST distinguish:

```text
Known
Believed
Suspected
Misunderstood
Unknown
```

The Narrator must use the active Character's Knowledge perspective.

## 52. Secrets

The MVP MUST support Campaign Secrets with visibility filtering.

A player-facing query model or export MUST not contain hidden canonical truth.

## 53. Progression Awards

The MVP MUST support at least one progression Award mechanism from the official Rule Set.

Awards MUST:

- derive from accepted evidence;
- be Rule Set-validated;
- apply once;
- persist with exact versions;
- update a progression balance or milestone.

## 54. Advancement

The MVP MUST support at least one player-selected advancement.

The workflow MUST include:

- available option;
- cost;
- prerequisites;
- preview;
- explicit confirmation;
- atomic spend and Character update;
- history.

## 55. Advancement Retry

A failed confirmation after commit MUST return the existing Advancement result and MUST NOT spend again.

## 56. Campaign Preferences

The MVP MUST support the package-declared Preferences required by the official Rule Set.

At minimum, the architecture SHOULD demonstrate:

- one mechanical Preference;
- one narrative or generation Preference;
- validation;
- persistence;
- operation snapshot where relevant.

## 57. House Rules

User-authored executable House Rules are excluded.

Package-declared built-in House Rules MAY be included.

## 58. Narrative Plan

The MVP MUST persist a versioned Narrative Plan.

The Plan SHOULD contain:

- premise;
- Acts;
- planned Scenes;
- Secrets or mysteries;
- possible developments;
- revision metadata.

## 59. Plan Revision

After Session finalization, the architecture MUST support revising future Plan content without rewriting completed history.

The first MVP may use a simple revision policy.

## 60. Persistence

The MVP MUST use one local transactional authoritative store.

It MUST persist:

- Campaigns;
- Characters;
- Sessions;
- Acts;
- Scenes;
- Messages;
- Dice Rolls;
- Memories;
- Relationships;
- Character Knowledge;
- Secrets;
- Narrative Plans;
- progression;
- Preferences;
- Operation Records;
- required Work Items.

## 61. Optimistic Concurrency

State-changing operations MUST use expected versions or equivalent concurrency protection.

## 62. Idempotency

The MVP MUST use OperationId for important state-changing operations.

At minimum:

- player input;
- Dice Roll;
- finalization;
- progression;
- Preference change;
- backup;
- import.

## 63. Background Work

Critical long-running work MUST be durable.

The application MUST recover incomplete Work Items after restart.

## 64. Application Restart

After closing and reopening the application, the player MUST be able to:

- open the Campaign;
- inspect accepted transcript;
- inspect Character state;
- inspect Memories;
- continue or start the next Session;
- recover pending operations where applicable.

## 65. Crash Recovery

The MVP acceptance suite MUST demonstrate recovery from at least:

- interruption after provider output but before commit;
- interruption after Roll persistence but before narration continuation;
- interruption during Session finalization;
- lost confirmation after commit.

## 66. Backup

The MVP MUST support creating a validated local backup.

The backup MUST:

- be transactionally consistent;
- contain a manifest;
- include integrity metadata;
- exclude credentials;
- preserve Campaign state required for restore.

## 67. Restore

The MVP MUST support validating and restoring a backup.

Restore MUST:

- validate before publication;
- preserve prior state through checkpoint when practical;
- avoid partial publication;
- report missing dependencies;
- recover a Campaign that passes integrity checks.

## 68. Export

A portable Campaign export is strongly preferred for MVP.

If not included in the first release, its architecture and format versioning MUST remain implemented enough to avoid coupling portability to raw database tables.

## 69. Import

If portable export is shipped, import MUST support:

- InspectOnly;
- ImportAsNewCampaign;
- identity validation;
- atomic publication;
- dependency reporting;
- malicious archive protection.

## 70. Configuration

The MVP MUST support versioned application configuration for:

- active provider profile;
- credential alias;
- storage;
- backup path;
- logging;
- Rule Knowledge;
- developer mode where included.

## 71. Credentials

Provider credentials MUST:

- remain outside Campaign data;
- be referenced by alias;
- be excluded from logs, backups, exports, and diagnostics;
- use an operating-system credential store or approved secure fallback.

## 72. Rule Knowledge

The MVP MUST support Rule Knowledge retrieval sufficient for the official Rule Set.

The implementation MUST preserve:

- Rule Set version;
- source provenance;
- bounded result count;
- citations or source references;
- transmission policy.

## 73. RAG Scope

The MVP does not require advanced autonomous RAG.

Chronicle retrieves and selects Rule Knowledge before provider invocation.

## 74. Security

The MVP MUST implement the minimum security requirements from RFC-0035, including:

- least context;
- structured output validation;
- cross-Campaign validation;
- Secret filtering;
- no provider persistence access;
- no provider-owned Dice;
- import safety;
- bounded logging;
- no dynamic untrusted package loading.

## 75. Observability

The MVP MUST include:

- structured local logs;
- OperationId correlation;
- safe error codes;
- provider usage metadata where available;
- finalization diagnostics;
- security-event logging;
- user-facing safe status;
- raw prompt logging disabled by default.

## 76. Desktop Application

The MVP MUST ship as a runnable desktop application or equivalent desktop development package accepted by the project.

It MUST provide:

- Campaign list;
- Campaign creation;
- Character creation;
- live play;
- Roll interaction;
- Character Sheet;
- Session finalization;
- Memories;
- progression;
- Preferences;
- Settings;
- safe errors;
- application restart persistence.

## 77. Desktop Process Model

The MVP SHOULD use:

- one main application process;
- one UI thread;
- asynchronous operations;
- one durable background worker;
- one active instance per data directory.

## 78. Safe Mode

Safe Mode is strongly preferred.

If omitted from the first user-facing release, equivalent recovery access MUST exist for:

- configuration repair;
- backup;
- export;
- migration failure.

## 79. Accessibility

The MVP core workflows MUST support:

- keyboard operation;
- visible focus;
- accessible labels;
- text-based Dice results;
- no color-only meaning;
- scalable text;
- reduced-motion behavior.

## 80. Offline Behavior

Without remote connectivity, the MVP MUST still permit:

- opening Campaigns;
- viewing history;
- Character management where safe;
- local Dice and mechanics;
- backup;
- export if implemented;
- local provider use if configured.

## 81. Build and Distribution

The MVP release MUST include:

- versioned Release build;
- clean source identity;
- dependency lockfiles;
- release manifest;
- checksums;
- license bundle;
- documented install or run instructions;
- application and user-data separation.

## 82. Update Scope

Automatic updates are excluded from MVP.

Manual upgrade instructions and migration safety are required.

## 83. Testing Requirements

The MVP release MUST pass:

- Domain tests;
- Application tests;
- Rule Set contract tests;
- provider adapter tests;
- persistence integration tests;
- migration tests;
- security tests;
- backup and restore tests;
- critical UI interaction tests;
- accessibility smoke tests;
- end-to-end smoke suite.

## 84. Required End-to-End Acceptance Scenario

The project MUST maintain one canonical acceptance scenario.

### Phase A — Setup

1. Launch Chronicle.
2. Configure provider profile and credential alias.
3. Select the official Rule Set.
4. Create a valid Character.
5. Provide Campaign-generation inputs.
6. Generate a Campaign proposal.
7. Review and confirm the Campaign.

### Phase B — Play

8. Start a Session.
9. Receive opening narration.
10. Submit a player action.
11. Receive coherent continuation.
12. Trigger a structured Roll request.
13. Review the Roll card.
14. Execute the Roll.
15. Persist raw Dice values.
16. Resolve mechanics through the Rule Set.
17. Apply a visible state consequence.
18. Continue narration from the accepted result.
19. Transition to another Scene.

### Phase C — Finalize

20. End the Session.
21. Run Archivist finalization.
22. Create or update at least one Memory.
23. Age temporary Memories once.
24. Apply one progression Award.
25. Apply one visible Relationship or Knowledge change.
26. Complete the Session.

### Phase D — Continue

27. Close Chronicle.
28. Reopen Chronicle.
29. Open the same Campaign.
30. Verify transcript, Roll, Character state, Memories, progression, and Session summary.
31. Start or prepare the next Session with prior context preserved.

## 85. Required Recovery Acceptance Scenarios

### Persisted Roll Recovery

1. Persist raw Dice values.
2. Simulate failure before narration continuation.
3. Restart the application.
4. Recover the same Roll.
5. Continue without reroll.

### Finalization Recovery

1. Begin finalization.
2. Simulate interruption before commit.
3. Restart.
4. Resume or retry safely.
5. Verify that Memory aging and progression apply once.

### Lost Confirmation

1. Commit an Advancement.
2. Simulate lost response.
3. Retry with same OperationId.
4. Return existing result.
5. Verify no second spend.

### Backup Restore

1. Create backup.
2. Validate backup.
3. Modify or remove active local state in a controlled test.
4. Restore.
5. Verify semantic Campaign equivalence.

## 86. Acceptance Evidence

MVP acceptance requires evidence artifacts such as:

- automated test results;
- release manifest;
- Rule Set conformance report;
- migration report;
- backup and restore report;
- security regression report;
- accessibility smoke report;
- canonical Campaign export or test database;
- screenshots or recordings of the end-to-end scenario;
- known-issues list.

## 87. Blocking Acceptance Criteria

The MVP MUST NOT be accepted if any of the following remains:

- data-loss defect;
- duplicate Roll;
- reroll on retry;
- duplicate progression;
- repeated Memory aging;
- Secret leakage;
- cross-Campaign mutation;
- incomplete finalization presented as complete;
- failed restart persistence;
- unrecoverable migration from the immediately prior supported version;
- invalid backup restore;
- provider credential exposure;
- inaccessible Roll or Session-finalization workflow;
- official Rule Set package failing conformance tests.
- official Rule Set package claiming support for a capability, mechanic, workflow, artifact, locale, security property, or compatibility promise that is not implemented and verified within the declared scope.

## 88. Nonblocking Limitations

The MVP MAY ship with documented limitations such as:

- one supported operating system initially;
- one provider adapter;
- no streaming;
- minimal Roll animation;
- limited Campaign-generation editing;
- basic Memory filtering;
- manual update;
- limited export format;
- English-only documentation or UI initially;
- no local model installer.

These limitations must not invalidate the core loop.

Known limitations MUST be reflected in package metadata, README content, public status claims, and user-facing documentation.

## 89. Explicitly Out of Scope

The following are outside MVP:

- multiplayer;
- multiple human players;
- multiple Player Characters per Campaign;
- mobile application;
- web application;
- television client;
- streaming integration;
- voice interaction;
- video;
- shared cloud Campaigns;
- cloud sync;
- remote Campaign server;
- user accounts;
- public Campaign marketplace;
- dynamic untrusted plugins;
- arbitrary user-authored Rule Set scripting;
- arbitrary House Rule DSL;
- multiple complete Rule Sets;
- autonomous NPC world simulation;
- Director multi-window control center;
- map engine;
- tactical grid;
- character portrait generation requirement;
- automatic updater requirement;
- full community package ecosystem.

## 90. Deferred but Architecturally Supported

The architecture may support later:

- more providers;
- more Rule Sets;
- local models;
- multiplayer;
- mobile clients;
- cloud sync;
- dynamic packages;
- richer export;
- automatic updates;
- collaborative Campaigns.

No MVP implementation work should be added solely for these futures unless it also supports the current vertical slice.

## 91. Scope Change Policy

Adding an MVP feature requires:

- explicit RFC-0042 amendment;
- justification;
- delivery impact;
- test impact;
- security impact;
- migration impact;
- removal or schedule adjustment elsewhere when necessary.

## 92. Scope Removal Policy

Removing a required feature requires:

- proof that the central hypothesis remains testable;
- updated acceptance scenario;
- architecture review;
- explicit project decision.

## 93. MVP Milestones

Recommended implementation milestones:

```text
M0 — Repository and Build Foundation
M1 — Domain and Persistence Skeleton
M2 — Character and Rule Set Vertical Slice
M3 — Narrative Intelligence Vertical Slice
M4 — Dice and Continuation
M5 — Session Finalization and Memory
M6 — Progression and Preferences
M7 — Backup, Recovery, and Migration
M8 — Desktop UX Completion
M9 — Hardening and Release Candidate
M10 — MVP Release
```

## 94. Milestone M0 — Foundation

Deliver:

- repository structure;
- build;
- CI;
- code style;
- test infrastructure;
- version identity;
- minimal documentation.

## 95. Milestone M1 — Domain and Persistence

Deliver:

- Campaign;
- Character;
- Session;
- Act;
- Scene;
- Message;
- local persistence;
- transactions;
- versions;
- Operation Records.

## 96. Milestone M2 — Character and Rule Set

Deliver:

- Rule Set package contract;
- official package skeleton;
- Character schema;
- Character creation;
- validation;
- Character Sheet UI.

## 97. Milestone M3 — Narrative Intelligence

Deliver:

- provider abstraction;
- official provider adapter;
- Prompt Builder;
- context selection;
- Narrator contract;
- Campaign Generator;
- structured output validation.

## 98. Milestone M4 — Dice

Deliver:

- Roll request;
- Roll UI;
- Chronicle random generation;
- deterministic Rule Set resolution;
- consequence application;
- continuation;
- recovery without reroll.

## 99. Milestone M5 — Finalization

Deliver:

- Archivist contract;
- Session summary;
- Memories;
- Relationship and Knowledge proposals;
- finalization transaction;
- Memory aging.

## 100. Milestone M6 — Progression and Preferences

Deliver:

- Awards;
- balances;
- advancement;
- Preferences;
- House Rule metadata;
- relevant UI.

## 101. Milestone M7 — Recovery

Deliver:

- Work Item recovery;
- backup;
- restore;
- migration fixtures;
- integrity checks;
- safe failure behavior.

## 102. Milestone M8 — Desktop UX

Deliver:

- Campaign list;
- setup workflows;
- live play;
- operation states;
- errors;
- accessibility;
- Settings;
- core diagnostics.

## 103. Milestone M9 — Hardening

Deliver:

- security regression;
- performance budgets;
- long-Campaign tests;
- packaging;
- release manifest;
- documentation;
- known issues.

## 104. Milestone M10 — Release

Deliver:

- validated release artifacts;
- source release;
- install instructions;
- checksums;
- license bundle;
- acceptance evidence;
- project announcement.

## 105. Definition of Done for a Feature

An MVP feature is done when:

- implementation exists;
- Domain and Application behavior is tested;
- persistence and recovery are tested where applicable;
- security and visibility are reviewed;
- observability exists;
- UI states are complete;
- accessibility is addressed;
- documentation is updated;
- no blocking defect remains.

## 106. Definition of Done for MVP

The MVP is done when:

- the canonical end-to-end scenario passes;
- required recovery scenarios pass;
- the official Rule Set passes conformance;
- the provider adapter passes contract tests;
- backup and restore pass;
- blocking criteria are clear;
- the application can be installed or run reproducibly;
- one full Campaign Session can be lived and continued after restart;
- scope exclusions remain excluded.

## 107. Product Success Signals

Early validation SHOULD look for:

- player completes Campaign setup;
- Character creation is understandable;
- provider configuration succeeds;
- live play reaches a Roll;
- Roll flow is trusted;
- player understands waiting and recovery states;
- Session finalization feels meaningful;
- Memories improve continuity;
- next Session reflects prior play;
- no manual database repair is required.

## 108. Technical Success Signals

Technical validation SHOULD show:

- no duplicate effects;
- deterministic Roll replay;
- stable persistence;
- bounded prompts;
- structured output acceptance;
- low finalization failure rate;
- successful restart recovery;
- successful backup restore;
- readable diagnostics;
- official Rule Set isolation.

## 109. Failure of the Hypothesis

The MVP hypothesis may be considered unproven if:

- continuity depends on manually feeding prior transcripts;
- provider output cannot be validated reliably;
- Session finalization produces noisy or meaningless Memories;
- mechanical resolution cannot remain deterministic;
- recovery frequently requires restarting the Campaign;
- Character context becomes too large for practical narration;
- the player cannot understand system state;
- one Session cannot complete without operator intervention.

These outcomes should trigger architecture revision, not silent scope expansion.

## 110. Current Delivery Decision

The official MVP includes:

- one desktop application;
- one local user;
- one Player Character per Campaign;
- one official Rule Set package complete for its declared release scope;
- one official provider adapter;
- schema-driven Character creation;
- Campaign generation and confirmation;
- persistent Sessions;
- Campaign → Session → Act → Scene;
- structured Narrator interaction;
- Chronicle-owned Dice;
- deterministic mechanics;
- Session finalization;
- Campaign Memories;
- Relationships;
- Character Knowledge;
- Secrets;
- progression;
- Preferences;
- local persistence;
- idempotency;
- crash recovery;
- backup and restore;
- core observability;
- security boundaries;
- accessible core UI;
- manual release and upgrade path.

## 111. Architecture Horizon

After MVP, Chronicle MAY expand into:

- additional Rule Sets;
- additional providers;
- local-provider packaging;
- multiplayer;
- mobile and web clients;
- cloud synchronization;
- community packages;
- richer Campaign tools;
- streaming;
- collaborative play;
- user-defined bounded rules.

None of these is required to validate MVP.

## 112. Open Questions

The following remain open before implementation planning is final:

- Which exact Werewolf edition and legal content strategy will be used?
- Which provider adapter will be official first?
- Which desktop framework and operating system are first?
- Is portable Campaign export required in MVP release one?
- Is Safe Mode fully user-facing in release one?
- Which progression operation best demonstrates advancement?
- Which mechanical Preference demonstrates House Rule support?
- How much Narrative Plan detail is visible to the player?
- Which Relationships and Knowledge views are mandatory in the first UI?
- Is provider streaming omitted initially?
- Which backup encryption behavior is required?
- What reference Campaign fixture demonstrates acceptance?
- Which performance budgets block release?
- Which localization scope is required?
- What exact public version number represents MVP?

These questions require ADRs and implementation planning, not additional Domain expansion.

## 113. Compliance Checklist

The MVP complies when:

- one complete vertical slice exists;
- Campaign setup is reviewable and persistent;
- one valid Character can be created;
- one Session can start and complete;
- narration uses bounded context;
- a structured Roll pauses the story;
- Chronicle generates and persists randomness;
- Rule Set resolution is deterministic;
- retries never reroll;
- finalization applies once;
- Memories age once;
- progression applies once;
- hidden information remains hidden;
- restart preserves continuity;
- interrupted operations recover;
- backup restores valid state;
- credentials remain protected;
- core UI is keyboard-accessible;
- release quality gates pass;
- excluded features remain excluded.

## 114. Final Principle

The MVP does not need to prove every future of Chronicle.

It needs to prove one thing completely:

A player can live a Chronicle, trust its mechanics, preserve its memories, and return to it later without the story forgetting what was lived.
