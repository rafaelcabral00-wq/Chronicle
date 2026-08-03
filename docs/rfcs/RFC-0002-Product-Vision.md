---
id: RFC-0002
title: Product Vision
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-08-03
category: Foundation
depends_on:
  - RFC-0000
  - RFC-0001
---

> **"Chronicle is designed for the future. The official application is built for today."**

# Product Vision

## Abstract

This RFC defines the product vision of Chronicle.

It explains the experience Chronicle intends to create, the problems it exists to solve, the people it serves, the principles that shape product decisions, and the boundaries that protect the project from uncontrolled scope growth.

This RFC does not define implementation details. It defines the direction against which future architecture, roadmap, and product decisions MUST be evaluated.

## 1. Vision Statement

Chronicle aims to become the definitive open-source framework for persistent tabletop role-playing campaigns directed through Narrative Intelligence.

The official Chronicle application is the first product built on that framework.

Its initial purpose is simple:

> Allow one player to live a coherent, long-form tabletop RPG campaign with a persistent world, consistent characters, deterministic rules, meaningful consequences, and an artificial Narrator directed by Chronicle.

Chronicle MUST feel like a role-playing experience.

It MUST NOT feel like a generic chatbot with an RPG prompt.

## 2. Product Promise

Chronicle promises the player that:

- the campaign will remember;
- characters will remain consistent;
- actions will have consequences;
- dice will be impartial;
- rules will not be invented casually;
- the story will pause when a test matters;
- unfinished Sessions can be resumed;
- the world will not depend on the temporary memory of a model;
- long campaigns will remain understandable and recoverable.

The player should feel that the campaign continues to exist even when the application is closed.

## 3. The Experience Chronicle Intends to Create

Chronicle SHOULD create the sensation of sitting at a table with an experienced Game Master who:

- knows the player's character;
- remembers past promises, failures, victories, and betrayals;
- introduces NPCs with stable motivations;
- understands the active Scene;
- applies the selected RPG system;
- knows when to ask for a test;
- waits for the result before continuing;
- adapts the planned campaign to player choices;
- preserves the emotional and mechanical consequences of play.

Chronicle does not attempt to reproduce every social aspect of a human table in the MVP.

It focuses on the parts that can be made reliable:

- continuity;
- direction;
- memory;
- rules;
- pacing;
- consequence;
- persistence.

## 4. Core Player Journey

The initial player journey is:

```text
Open Chronicle
      ↓
Select an existing Campaign
or create a new one
      ↓
Choose a Rule Set
      ↓
Define optional campaign preferences
      ↓
Create the player Character
      ↓
Chronicle prepares the Campaign
      ↓
Begin a Session
      ↓
Play through directed narrative
      ↓
Resolve tests through Chronicle
      ↓
Finalize the Session
      ↓
Preserve Memories, progression, and consequences
      ↓
Return later and continue
```

Every major product feature MUST support this journey directly or protect its quality.

## 5. Campaign Creation Vision

Campaign creation SHOULD feel like the preparation of a real tabletop Chronicle.

The player chooses:

- the RPG system;
- optional narrative preferences;
- the player Character.

Chronicle then prepares hidden and visible campaign material.

This MAY include:

- Campaign title;
- premise;
- tone;
- setting;
- major conflicts;
- initial Acts;
- initial Scenes;
- NPCs;
- antagonist roles;
- mysteries;
- possible narrative directions.

The player does not need to see every generated element.

Hidden campaign information exists to improve continuity and direction, not to expose spoilers.

NPCs SHOULD exist before their first appearance.

The Narrative Plan SHOULD exist before the Campaign begins, while remaining adaptable to player choice.

## 6. Session Experience Vision

The Session screen is the heart of the official application.

The primary focus is the narrative.

The player SHOULD see:

- the active narrative;
- the player Character Sheet;
- structured tests when required;
- clear indication when the story is waiting for player action;
- the current Session state;
- a way to finalize the Session deliberately.

The experience SHOULD minimize visible technical complexity.

The player SHOULD NOT need to understand:

- prompt construction;
- retrieval;
- model providers;
- context windows;
- structured contracts;
- validation pipelines;
- orchestration internals.

The system may expose product-friendly role names such as:

- Chronicle Director;
- Narrator;
- Archivist.

These names SHOULD reinforce the identity of Chronicle without requiring the user to understand the underlying architecture.

## 7. Dramatic Dice Vision

Dice tests are dramatic interruptions, not background calculations.

When a test is required:

1. the Narrator advances the fiction up to the uncertain moment;
2. the narrative stops;
3. Chronicle presents the test;
4. the player triggers the roll;
5. Chronicle executes and validates the result;
6. the Narrator continues from the exact dramatic interruption.

The MVP MAY use a simple button and textual dice result.

The experience MUST already communicate:

> The story is waiting for this result.

Animated dice MAY be added later.

The drama does not depend on animation. It depends on interruption, uncertainty, and continuation.

## 8. Memory Vision

Chronicle does not preserve only transcripts.

It preserves what remains meaningful.

Campaign Memories SHOULD allow the system to distinguish between:

- something that happened;
- something that still matters;
- something a Character remembers;
- something that has become less relevant;
- something that can never be forgotten.

A Session transcript answers:

> What was said and narrated?

Campaign Memory answers:

> What from this experience should continue shaping the Chronicle?

The player SHOULD be able to review past Memories.

The player MAY increase the relevance of a Memory through an explicit workflow.

The product MUST preserve the distinction between user-curated relevance and historical truth.

## 9. Character Vision

Characters are persistent identities, not temporary prompt content.

A Character SHOULD preserve:

- game-system data;
- progression;
- personality;
- goals;
- fears;
- history;
- relationships;
- conditions;
- meaningful Memories;
- current state.

NPCs SHOULD behave as characters with continuity.

They MUST NOT be regenerated as unrelated versions of themselves every time they appear.

A hidden NPC is unknown to the player, not nonexistent.

## 10. Chronicle Director Vision

The Chronicle Director is the coordinating intelligence of the experience.

It does not replace the Narrator.

It prepares the stage.

The Chronicle Director SHOULD determine or coordinate:

- active Session;
- active Act;
- active Scene;
- participants;
- immediate objective;
- relevant Memories;
- relevant Character state;
- relevant Rule Set knowledge;
- whether the narrative is waiting for a roll;
- whether a Scene or Act transition is valid.

The Chronicle Director SHOULD make Narrator prompts smaller, clearer, and more focused.

The Chronicle Director MUST protect context boundaries.

For example, participants in one Scene MUST NOT leak into another Scene merely because both Scenes belong to the same Act.

## 11. Narrator Vision

The Narrator exists to make the Campaign felt.

The Narrator SHOULD:

- write evocative prose;
- portray Characters consistently;
- respect the active Scene;
- follow the selected tone;
- ask for tests through structured requests;
- continue after validated outcomes;
- avoid exposing hidden information;
- avoid inventing persistent facts casually.

The Narrator is replaceable.

Chronicle MUST remain functional if the implementation of Narrative Intelligence changes.

Narrative quality is important, but it MUST NOT override consistency, rules, or persistence.

## 12. Archivist Vision

The Archivist preserves the meaning of completed play.

At Session finalization, the Archivist SHOULD help identify:

- new Memories;
- changed Memories;
- progression;
- Relationship changes;
- Character State changes;
- discovered information;
- unresolved consequences;
- Session summary.

The Archivist does not own truth.

It proposes structured changes based on the Session.

Chronicle validates and persists accepted changes.

The player SHOULD finish a Session knowing that the Campaign has meaningfully advanced.

## 13. Rule Set Vision

Chronicle is system-agnostic at the framework level.

The first official Rule Set used during development is Werewolf: The Apocalypse.

A Rule Set SHOULD encapsulate system-specific knowledge and behavior, including:

- sheet structure;
- terminology;
- validation;
- dice mechanics;
- progression;
- relevant rules;
- system-specific guidance.

Adding a new Rule Set SHOULD require new data, configuration, retrieval content, and bounded adapters.

It SHOULD NOT require redesigning the Chronicle Core.

The MVP only needs one Rule Set that is complete for its declared release scope.

Complete for a declared release scope means every advertised capability, mechanic, workflow, artifact, validation, test, localization requirement, security requirement, and compatibility promise within that scope is implemented and verified.

It does not require implementing the entire source RPG system.

Excluded mechanics, disabled operations, known limitations, compatibility boundaries, and evidence status must be explicit and must not be presented as supported behavior.

A second incomplete Rule Set is less valuable than one reliable first experience.

## 14. Official Application Vision

The official Chronicle application is a reference implementation and the first playable product.

The MVP is:

- desktop-first;
- single-player;
- local-first;
- focused on one active player Character;
- focused on one Rule Set that is complete for its declared release scope;
- designed for long-form Campaigns.

The official application MAY later expand to:

- web;
- mobile;
- tablet;
- television;
- multiplayer;
- streaming;
- voice;
- images;
- music;
- alternative clients.

These possibilities belong to the Architecture Horizon.

They are not current delivery commitments.

## 15. Framework Vision

Chronicle SHOULD provide a stable core that can support future applications without requiring a rewrite of domain behavior.

The framework vision includes the possibility of:

- multiple official or community clients;
- additional Rule Sets;
- alternative Narrative Intelligence providers;
- local models;
- external integrations;
- multiplayer orchestration;
- multimedia presentation;
- streaming and spectator experiences.

The framework vision is a design constraint, not a backlog.

Chronicle MUST avoid prematurely implementing generic infrastructure that has no immediate use in the active milestone.

## 16. Scope Discipline

Chronicle adopts two independent horizons.

### 16.1 Architecture Horizon

The Architecture Horizon asks:

> Can the design evolve without fundamental reconstruction?

It protects future flexibility.

### 16.2 Delivery Horizon

The Delivery Horizon asks:

> What must be built now to deliver the next complete player experience?

It protects execution.

A capability MAY be anticipated architecturally while remaining absent from the product.

A future extension point MUST NOT justify:

- additional user flows;
- speculative services;
- empty modules;
- premature abstractions;
- unsupported configuration;
- untested provider implementations;
- incomplete alternative clients.

Chronicle MUST crawl before it walks and walk before it runs.

## 17. MVP Vision

The MVP is not a technical prototype.

It is the smallest complete Chronicle experience.

A player MUST be able to:

- create a Campaign;
- create a Character;
- start a Session;
- receive directed narrative;
- trigger and resolve dice tests;
- finish the Session;
- preserve Memories and progression;
- return later;
- continue coherently.

The MVP does not need:

- visual spectacle;
- platform breadth;
- multiplayer;
- voice;
- generated art;
- animated dice;
- marketplace support;
- multiple providers in the UI.

The MVP MUST prioritize completion over breadth.

## 18. Product Principles

### 18.1 Consistency Before Novelty

A coherent Campaign is more valuable than surprising but contradictory prose.

### 18.2 Persistence Before Context Size

Chronicle MUST store truth explicitly rather than repeatedly sending entire transcripts.

### 18.3 Direction Before Improvisation

The Campaign SHOULD have a plan.

The plan MAY change.

It MUST NOT become a rigid script.

### 18.4 Rules Before Convenience

Deterministic mechanics MUST remain outside narrative generation.

### 18.5 Player Agency Before Planned Outcomes

The Narrative Plan guides play.

It MUST NOT invalidate meaningful player choices.

### 18.6 Focus Before Expansion

A complete single-player Campaign is more valuable than several unfinished platforms.

### 18.7 Replaceability Before Provider Lock-In

Narrative Intelligence is a dependency, not the product identity.

### 18.8 Memory Before Transcript

Chronicle SHOULD preserve what matters, not merely what was said.

## 19. Emotional Design Goals

Chronicle SHOULD make the player feel:

- remembered;
- recognized;
- responsible for consequences;
- uncertain before important tests;
- curious about hidden motives;
- attached to recurring Characters;
- confident that the Campaign can continue later;
- surprised without feeling deceived by inconsistency.

Chronicle SHOULD avoid making the player feel:

- that the Narrator forgot everything;
- that outcomes were fabricated after the fact;
- that NPCs exist only when visible;
- that rules change arbitrarily;
- that choices do not matter;
- that a new Session is a reset;
- that the system is merely replaying a prompt.

## 20. User Trust

Trust is a product feature.

Chronicle SHOULD make important system behavior understandable.

The player SHOULD be able to distinguish:

- narrative;
- test request;
- dice result;
- persisted Memory;
- Character progression;
- Session finalization.

Chronicle MUST NOT pretend that an invalid or missing state transition was successfully persisted.

Failures SHOULD be explicit and recoverable.

The player MUST NOT lose a Campaign silently.

## 21. Product Boundaries

Chronicle is not intended to replace:

- the social experience of every human group;
- every possible interpretation of every RPG rule;
- professional adventure writing;
- virtual tabletop map systems;
- general-purpose world simulation;
- generic chatbot platforms.

Chronicle focuses on directed, persistent, character-centered tabletop narrative.

## 22. Success Measures

The primary success measure is not message count.

The product succeeds when players can sustain Campaigns over time.

Useful measures MAY include:

- Campaigns continued after the first Session;
- completed Session finalizations;
- successful Session resumes;
- low rate of state contradictions;
- low rate of invalid structured responses;
- stable NPC continuity;
- successful dice interruption and continuation;
- player retention across Sessions;
- player-reported sense of consequence and memory.

Token usage, response latency, and provider cost are important engineering measures.

They are not substitutes for player experience.

## 23. Failure Conditions

The product vision is not achieved if:

- the Narrator becomes the only source of truth;
- Campaign continuity depends on replaying full transcripts;
- dice outcomes are generated by prose;
- hidden NPCs are recreated inconsistently;
- the active Scene cannot isolate participants and context;
- Session finalization loses or duplicates progression;
- the Campaign cannot be resumed reliably;
- scope expansion prevents delivery of the first complete Campaign experience.

## 24. Long-Term Direction

Chronicle may one day support groups playing together across phones, tablets, televisions, desktop computers, and streamed sessions.

That future is compatible with this vision.

It is not required to validate it.

The first proof of Chronicle is smaller:

> One player.  
> One Character.  
> One persistent Campaign.  
> One complete Rule Set.  
> One experience worth continuing.

## 25. Final Principle

Chronicle does not need to simulate everything.

It needs to remember what matters, direct what happens now, and make the player want to return for the next Session.
