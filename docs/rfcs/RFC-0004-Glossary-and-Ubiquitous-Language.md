---
id: RFC-0004
title: Glossary and Ubiquitous Language
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-07-31
category: Foundation
depends_on:
  - RFC-0000
  - RFC-0001
  - RFC-0002
  - RFC-0003
---

> **"A shared language is the first architecture of a shared world."**

# Glossary and Ubiquitous Language

## Abstract

This RFC defines the canonical vocabulary of Chronicle.

Its purpose is to ensure that domain concepts have one stable meaning across documentation, code, contracts, persistence, tests, prompts, user interface text, and contributor discussions.

Chronicle MUST use this vocabulary consistently.

When a term in this RFC conflicts with an informal synonym, the canonical term defined here prevails.

## 1. Purpose

Chronicle contains concepts that are easily confused:

- Campaign and Chronicle;
- Session and Scene;
- Act and Scene;
- Campaign State and Campaign Memory;
- Narrative Event and Campaign Memory;
- Character and NPC;
- Rule Set and RAG;
- Narrator and Narrative Intelligence;
- Chronicle Director and Narrator.

Ambiguous names create architectural drift.

This RFC exists to prevent that drift.

## 2. Language Rules

### 2.1 Canonical Terms

A canonical term MUST be used in:

- RFCs;
- ADRs;
- code identifiers;
- domain contracts;
- test names;
- persistence mappings;
- internal APIs;
- contributor discussions.

User-facing translations MAY vary by locale, but they MUST preserve the same concept.

### 2.2 English as the Documentation Language

The official project documentation is written in English.

Product UI MAY be localized.

Domain identifiers in code SHOULD remain in English.

### 2.3 Singular and Plural

Entity names are singular when referring to a type:

```text
Campaign
Character
Session
Act
Scene
CampaignMemory
```

Collection names are plural:

```text
campaigns
characters
sessions
acts
scenes
campaignMemories
```

### 2.4 Avoided Synonyms

A synonym SHOULD NOT be introduced when a canonical term already exists.

Examples:

```text
Story
Adventure
Questline
Chapter
Encounter
Beat
Thread
World Event
Fact Log
Memory Record
Game Master Service
AI Agent
```

These terms MAY appear in narrative prose or external Rule Set material.

They MUST NOT replace the canonical domain vocabulary unless formally approved by a later RFC.

## 3. Chronicle

### Definition

`Chronicle` is the name of the open-source framework and official product family.

It is not a domain entity.

### Correct Usage

```text
Chronicle Core
Chronicle application
Chronicle project
Chronicle documentation
```

### Incorrect Usage

```text
A Campaign is a Chronicle entity named Chronicle.
```

### Notes

The word may appear in user-facing prose as a thematic synonym for a long-lived campaign, but code MUST use `Campaign` for the domain aggregate.

## 4. Chronicle Core

### Definition

`Chronicle Core` is the implementation-neutral center of the framework responsible for domain rules, orchestration contracts, and application-controlled state transitions.

### Responsibilities

Chronicle Core includes or coordinates:

- domain model;
- Campaign invariants;
- Chronicle Director;
- deterministic Dice Roll behavior;
- validation;
- state transitions;
- application ports.

### Exclusions

Chronicle Core does not mean:

- a specific database;
- a specific UI;
- a provider SDK;
- a vector database;
- a desktop shell.

### Code Identifier

```text
ChronicleCore
```

## 5. Campaign

### Definition

A `Campaign` is the complete persistent tabletop role-playing experience owned by Chronicle.

It is the aggregate root of gameplay data.

### Owns

- Player Character;
- NPCs;
- Relationships;
- Campaign Memories;
- Campaign State;
- Narrative Plan;
- Sessions;
- Rule Set reference.

### Correct Usage

```text
Create a Campaign.
Resume the Campaign.
Archive the Campaign.
```

### Avoid

```text
Story
Adventure
Game
Chronicle
World
```

when referring to the domain aggregate.

## 6. Campaign State

### Definition

`Campaign State` is the authoritative snapshot of what is true now and what is required to continue play.

### Answers

```text
What is true now?
Where is the player?
Which Session is active?
Which Scene is active?
Is a Dice Roll unresolved?
```

### Examples

- active Session identifier;
- active Act identifier;
- active Scene identifier;
- current in-world time;
- active objective;
- current locations;
- pending Dice Roll;
- current conditions.

### Distinction

Campaign State is not Campaign Memory.

Campaign State represents current truth.

Campaign Memory represents preserved meaning from prior experience.

### Code Identifier

```text
CampaignState
```

## 7. Campaign Memory

### Definition

A `Campaign Memory` is a persistent representation of something meaningful that was lived during the Campaign and may influence future play.

### Answers

```text
What should still matter?
What should be remembered?
Who remembers it?
How relevant is it now?
```

### Examples

```text
The Alpha discovered the player's lie.
The pack promised to protect Silent Rain.
The ritual dagger was lost.
```

### Required Distinction

A Campaign Memory is not:

- a raw message;
- a complete transcript;
- a UI notification;
- a Narrative Event;
- automatically every occurrence in the story.

### Code Identifier

```text
CampaignMemory
```

### Collection Identifier

```text
campaignMemories
```

### Avoid

```text
Event
WorldEvent
StoryEvent
Fact
HistoryItem
MemoryRecord
```

unless a future RFC introduces a distinct concept.

## 8. Memory Lifetime

### Definition

`Memory Lifetime` defines whether a Campaign Memory is permanent or temporary and, when temporary, how long it remains eligible for normal relevance processing.

### Canonical Forms

```text
Permanent
Temporary
```

The MVP uses remaining Sessions for temporary lifetime.

### Code Identifier

```text
MemoryLifetime
```

## 9. Memory Relevance

### Definition

`Memory Relevance` represents the current usefulness of a Campaign Memory for context selection and narrative continuity.

### Distinction

Relevance is not importance.

Importance describes the intrinsic significance of the Memory.

Relevance describes how useful it is now.

A permanent Memory may be important but not relevant to the current Scene.

### Code Identifier

```text
relevance
```

## 10. Memory Importance

### Definition

`Memory Importance` represents the intrinsic narrative significance of a Campaign Memory.

### Examples

High importance:

```text
The player caused the death of the pack leader.
```

Low importance:

```text
The player misplaced a common backpack.
```

### Code Identifier

```text
importance
```

## 11. Session

### Definition

A `Session` is a player-controlled period of play.

It begins when the player starts or resumes gameplay and ends through explicit finalization.

### Owns

- Acts;
- messages;
- narrative events;
- Dice Rolls;
- finalization result;
- Session summary.

### Correct Usage

```text
Start a Session.
Resume an interrupted Session.
Finalize the Session.
```

### Avoid

```text
Chat
Conversation
Run
Match
Episode
```

when referring to the domain entity.

## 12. Act

### Definition

An `Act` is a major dramatic division within a Session.

It groups Scenes that contribute to the same broad movement, objective, conflict, or dramatic phase.

### Hierarchy

```text
Campaign
└── Session
    └── Act
        └── Scene
```

### Example

```text
Act: The Desert War
```

### Correct Interpretation

An Act is broader than a Scene.

### Avoid

```text
Chapter
Arc
Sequence
Phase
```

as code-level replacements.

## 13. Scene

### Definition

A `Scene` is the smallest directed unit of active narrative context.

It defines what is happening now.

### Defines

- location;
- participants;
- immediate objective;
- active conflict;
- local state;
- relevant Memories;
- relevant Rule Set knowledge.

### Example

```text
Act: The Desert War
├── Scene: Battle at the Northern Gate
├── Scene: Defense of the Fortress
└── Scene: Assault on the Hill
```

### Invariant

Participants in one Scene are not automatically participants in another Scene.

### Avoid

```text
Act
Encounter
Beat
Room
Thread
Segment
```

as code-level replacements.

## 14. Participant

### Definition

A `Participant` is a Character explicitly assigned to a Scene.

Participation is contextual.

It does not define Character existence.

### May Include

- Character identifier;
- participation role;
- local objective;
- visibility;
- entry point;
- exit point;
- local state.

### Code Identifier

```text
SceneParticipant
```

## 15. Character

### Definition

A `Character` is a persistent campaign participant with identity, sheet, narrative profile, state, and continuity.

### Roles

```text
Player
NonPlayer
```

### Important Rule

Player Character and NPC SHOULD be represented by the same core entity unless their invariants require divergence.

### Code Identifier

```text
Character
```

## 16. Player Character

### Definition

The `Player Character` is the Character controlled by the player.

The MVP supports exactly one Player Character per Campaign.

### Code Representation

```text
CharacterRole.Player
```

### Avoid

```text
Hero
Avatar
UserCharacter
MainCharacter
PCEntity
```

as the primary domain term.

The abbreviation `PC` MAY appear in informal discussion but SHOULD NOT be used in public contracts.

## 17. Non-Player Character

### Definition

A `Non-Player Character` is a persistent Character not controlled by the player.

### Code Representation

```text
CharacterRole.NonPlayer
```

### Accepted Abbreviation

```text
NPC
```

`NPC` is accepted in documentation and product language.

### Important Rule

An NPC exists independently from visibility or Scene participation.

## 18. Character Sheet

### Definition

A `Character Sheet` is the structured Rule Set-specific game data associated with a Character.

### MVP Representation

The MVP uses generic field-and-value entries.

### Code Identifier

```text
CharacterSheet
```

### Avoid

```text
Stats
Profile
SheetData
CharacterData
```

when referring to the complete mechanical structure.

## 19. Narrative Profile

### Definition

A `Narrative Profile` contains persistent nonmechanical information used to portray a Character consistently.

### May Include

- personality;
- fears;
- beliefs;
- objectives;
- personal history;
- secrets;
- behavioral tendencies.

### Code Identifier

```text
NarrativeProfile
```

## 20. Character State

### Definition

`Character State` represents the Character's current mutable condition.

### May Include

- wounds;
- temporary effects;
- emotional condition;
- resources;
- location;
- availability;
- alive, dead, missing, or unknown status.

### Code Identifier

```text
CharacterState
```

## 21. Relationship

### Definition

A `Relationship` is the directional state of one Character toward another Character.

### Directionality

```text
A trusts B
```

does not imply:

```text
B trusts A
```

### Code Identifier

```text
Relationship
```

### Avoid

```text
Bond
Affinity
SocialLink
Connection
```

as replacements for the domain entity.

## 22. Rule Set

### Definition

A `Rule Set` is the domain abstraction representing one supported tabletop RPG system.

### Provides

- Character Sheet schema;
- validation rules;
- terminology;
- dice mechanics;
- progression;
- supported actions;
- system-specific guidance.

### Example

```text
Werewolf: The Apocalypse
```

### Code Identifier

```text
RuleSet
```

### Important Distinction

A Rule Set is not a RAG implementation.

## 23. RAG

### Definition

`RAG` means Retrieval-Augmented Generation.

It is an infrastructure technique used to retrieve relevant Rule Set knowledge.

### Architectural Position

RAG belongs to infrastructure.

It does not belong to the core domain vocabulary.

### Correct Usage

```text
The Rule Set knowledge adapter may use RAG.
```

### Incorrect Usage

```text
Campaign owns a RAG.
Character references a RAG entity.
```

### Code Guidance

Infrastructure names MAY include:

```text
RuleKnowledgeRetriever
RagRuleKnowledgeAdapter
```

Domain names MUST use `RuleSet`.

## 24. Narrative Intelligence

### Definition

`Narrative Intelligence` is the technology-neutral capability used to generate or interpret narrative output.

It may be implemented by:

- an LLM;
- a local model;
- a remote provider;
- another future narrative technology.

### Architectural Purpose

The term exists to avoid binding Chronicle's identity to one model class.

### Code Guidance

Core contracts SHOULD reference role-specific abstractions such as `Narrator`.

Provider infrastructure MAY reference models and inference concepts.

## 25. Narrator

### Definition

The `Narrator` is the replaceable role responsible for generating player-facing narrative and structured narrative proposals from curated context.

### May

- narrate prose;
- portray Characters;
- request Dice Rolls;
- propose Narrative Events;
- continue after a validated result.

### Must Not

- own memory;
- own Campaign State;
- roll dice;
- persist state directly;
- query the database independently;
- redefine Character Sheets.

### Code Identifier

```text
Narrator
```

### Avoid

```text
LLM
AI
GameMaster
DungeonMaster
NarrativeAgent
```

as the domain-facing interface name.

Provider code MAY use provider-specific terminology internally.

## 26. Chronicle Director

### Definition

The `Chronicle Director` is the orchestration role responsible for preparing, coordinating, and advancing the active Campaign experience.

### Coordinates

- active Session;
- active Act;
- active Scene;
- participants;
- objectives;
- relevant Memories;
- Character state;
- Rule Set knowledge requests;
- Narrator invocation;
- Dice Roll interruption;
- Scene and Act transitions.

### Must Not

- narrate prose;
- roll dice;
- persist data directly;
- own Rule Set mechanics;
- replace the Archivist.

### Code Identifier

```text
ChronicleDirector
```

### User-Facing Translation

Portuguese:

```text
Diretor da Crônica
```

## 27. Archivist

### Definition

The `Archivist` is the role responsible for proposing persistent meaning from completed play.

### Proposes

- new Campaign Memories;
- Memory updates;
- progression;
- Relationship changes;
- Character State changes;
- discovered information;
- Session summary.

### Must Not

- become the source of truth;
- rewrite transcripts;
- alter Dice Roll results;
- persist unvalidated changes.

### Code Identifier

```text
Archivist
```

### User-Facing Translation

Portuguese:

```text
Arquivista
```

## 28. Prompt Builder

### Definition

The `Prompt Builder` is an application service that transforms curated Chronicle context into a provider-ready request.

### Responsibilities

- assemble instructions;
- include selected state;
- include selected Memories;
- include relevant Rule Set knowledge;
- enforce token or context budgets;
- apply provider-independent contracts.

### Must Not

- select authoritative state independently;
- become the domain owner of context;
- persist narrative output;
- define Campaign rules.

### Code Identifier

```text
PromptBuilder
```

## 29. Context

### Definition

`Context` is the temporary information package provided to a role such as the Narrator or Archivist for one operation.

### Context May Include

- task instructions;
- active Scene;
- participants;
- relevant Campaign State;
- relevant Campaign Memories;
- Rule Set knowledge;
- recent messages;
- pending Dice Roll result.

### Important Rule

Context is temporary.

Context is not memory.

### Avoid

```text
Full Database
Complete Campaign Dump
Permanent AI Memory
```

## 30. Narrative Message

### Definition

A `Narrative Message` is a persisted conversational entry created by the player, Narrator, or system during a Session.

### Authors

```text
Player
Narrator
System
```

### Important Rule

Messages preserve the transcript.

They do not define authoritative Campaign truth by themselves.

### Code Identifier

```text
NarrativeMessage
```

## 31. Narrative Event

### Definition

A `Narrative Event` is a structured instruction or occurrence intended for immediate orchestration or presentation.

### Examples

- narration block;
- Dice Roll request;
- NPC entry;
- NPC exit;
- combat start;
- combat end;
- ambient state change;
- proposed Scene transition.

### Important Distinction

Narrative Event is not Campaign Memory.

A Narrative Event concerns what Chronicle should process now.

A Campaign Memory concerns what should remain meaningful later.

### Code Identifier

```text
NarrativeEvent
```

## 32. Dice Roll

### Definition

A `Dice Roll` is the complete Chronicle-controlled lifecycle of a requested random game operation.

### Includes

- request;
- presentation;
- random execution;
- Rule Set resolution;
- validated result;
- persistence.

### Code Identifier

```text
DiceRoll
```

### Avoid

```text
Test
Check
RandomResult
RollEvent
```

as replacements for the complete entity.

`Test` MAY describe the fictional or mechanical challenge presented to the player.

## 33. Roll Request

### Definition

A `Roll Request` is the structured request for a Dice Roll.

### May Include

- acting Character;
- reason;
- dice pool;
- difficulty;
- modifiers;
- stakes;
- dramatic importance;
- Rule Set operation.

### Code Identifier

```text
RollRequest
```

## 34. Roll Result

### Definition

A `Roll Result` is the validated and persisted outcome of a Dice Roll.

### May Include

- raw dice;
- successes;
- failures;
- critical result;
- botch or equivalent;
- applied modifiers;
- interpreted outcome.

### Code Identifier

```text
RollResult
```

## 35. Narrative Plan

### Definition

The `Narrative Plan` is the adaptable planned structure of the Campaign.

### May Include

- premise;
- planned Acts;
- planned Scenes;
- NPC roles;
- conflicts;
- mysteries;
- revelations;
- possible outcomes.

### Important Rule

The Narrative Plan guides play.

It does not override player agency or validated history.

### Code Identifier

```text
NarrativePlan
```

## 36. Campaign Preferences

### Definition

`Campaign Preferences` are player-provided customizations that modify tone, emphasis, or approved Rule Set behavior for one Campaign.

### Examples

```text
Focus on horror.
Use a lighter tone.
Disable tests to enter the Umbra.
```

### Important Rule

Preferences MUST be validated against the capabilities and safety boundaries of the active Rule Set.

### Code Identifier

```text
CampaignPreferences
```

### Avoid

```text
Custom Prompt
User Prompt
System Prompt
Campaign Instructions
```

as the domain concept.

## 37. Provider

### Definition

A `Provider` is an infrastructure implementation that supplies Narrative Intelligence or related model capabilities.

### Examples

- remote hosted model provider;
- local inference runtime;
- self-hosted model endpoint.

### Architectural Position

Provider is infrastructure.

It is not part of Campaign truth.

### Code Guidance

```text
NarrativeProvider
ModelProvider
```

MAY be used in infrastructure modules.

## 38. Structured Contract

### Definition

A `Structured Contract` is a versioned machine-readable request or response shape used across nondeterministic or external boundaries.

### Examples

- Narrator request;
- Narrator response;
- Roll Request;
- Archivist proposal;
- Campaign generation response.

### Important Rule

A Structured Contract is not a domain entity unless the domain explicitly persists it.

## 39. Application Service

### Definition

An `Application Service` coordinates use cases across domain objects and ports.

### Examples

- create Campaign;
- start Session;
- process player input;
- execute Dice Roll;
- finalize Session.

### Important Rule

Application Services orchestrate use cases.

They MUST NOT become alternative domain models.

## 40. Domain Service

### Definition

A `Domain Service` performs domain behavior that does not naturally belong to one entity or value object.

### Examples

- Chronicle Director;
- Memory aging policy;
- Scene transition validation.

### Important Rule

A Domain Service MUST represent genuine domain behavior.

It MUST NOT be used as a generic container for unrelated logic.

## 41. Entity

### Definition

An `Entity` is a domain object defined by stable identity across time.

### Examples

- Campaign;
- Character;
- Relationship;
- Campaign Memory;
- Session;
- Act;
- Scene;
- Dice Roll.

### Important Rule

Two Entities with identical data are still different when their identifiers differ.

## 42. Value Object

### Definition

A `Value Object` is a domain object defined by its values rather than by identity.

### Possible Examples

- Campaign identifier;
- Character identifier;
- Memory Lifetime;
- relevance score;
- Scene objective;
- Roll Result components;
- provider-independent settings.

### Important Rule

Value Objects SHOULD be immutable.

## 43. Aggregate

### Definition

An `Aggregate` is a consistency boundary containing Entities and Value Objects governed through one Aggregate Root.

### Initial Aggregate Root

```text
Campaign
```

### Important Rule

External operations MUST modify aggregate-owned state through approved Campaign operations or application use cases.

## 44. Aggregate Root

### Definition

An `Aggregate Root` is the only external entry point for modifications inside an Aggregate.

### Chronicle Usage

`Campaign` is the initial Aggregate Root.

This does not require one giant in-memory object or one database document.

It defines consistency and ownership, not storage shape.

## 45. Persistence

### Definition

`Persistence` is the infrastructure capability that stores and restores Chronicle state.

### Important Rule

Persistence technology MUST NOT redefine domain meaning.

### Avoid

Using database table names as domain terminology when they differ from canonical concepts.

## 46. Current Delivery

### Definition

`Current Delivery` is the set of capabilities required by the active milestone.

### Equivalent Term

```text
Delivery Horizon
```

### Important Rule

Current Delivery is a commitment.

Architecture Horizon is not.

## 47. Architecture Horizon

### Definition

`Architecture Horizon` represents future evolution the design should not unnecessarily prevent.

### Examples

- multiplayer;
- mobile clients;
- television clients;
- streaming;
- voice;
- generated images;
- multiple providers;
- community Rule Sets.

### Important Rule

Architecture Horizon MUST NOT become automatic implementation scope.

## 48. MVP

### Definition

`MVP` means the smallest complete Chronicle experience that validates the product.

### Includes

- one local user;
- single-player Campaign;
- one Player Character;
- Werewolf: The Apocalypse Rule Set;
- persistent Sessions;
- Chronicle Director;
- Narrator;
- Archivist;
- deterministic Dice Rolls;
- Campaign Memories;
- desktop-first client.

### Important Rule

MVP does not mean disposable prototype.

MVP behavior MUST preserve the approved architecture.

## 49. Official Application

### Definition

The `official Chronicle application` is the first reference product built on the Chronicle framework.

### Important Rule

The official application is not the entire framework.

The framework is not required to implement multiple applications during the MVP.

## 50. Deprecated and Rejected Terms

The following terms are rejected as canonical names at this stage:

| Rejected Term | Use Instead | Reason |
|---|---|---|
| ChronicleAI | Chronicle | Product renamed; AI is not the identity |
| World Event | Campaign Memory | Memories belong to the Campaign and preserve meaning |
| Story Manager | Campaign domain services or Archivist | Too broad and ambiguous |
| World Engine | Chronicle Core or specific domain service | Implies general world simulation |
| Campaign Engine | Chronicle Core / Chronicle Director | Ambiguous responsibility |
| Game Director | Chronicle Director | Chronicle-specific identity approved |
| Director | Chronicle Director | Too generic |
| LLM Master | Narrator | Provider-bound and role ambiguity |
| AI Game Master | Narrator + Chronicle Director | Incorrectly combines responsibilities |
| Knowledge Base | Rule Set knowledge infrastructure | Infrastructure term, not core domain |
| RAG Entity | Rule Set or retrieval adapter | RAG is infrastructure |
| Event History | Campaign Memories + Session transcript | Conflates transcript and meaning |
| Chapter | Act | Canonical hierarchy uses Act |
| Encounter | Scene | Too combat-oriented and system-specific |
| Chat | Session transcript or narrative interface | UI metaphor, not domain entity |
| Story State | Campaign State | Campaign is the canonical aggregate |
| AI Memory | Campaign Memory or Context | Chronicle owns memory |

## 51. Canonical Hierarchies

### Product Structure

```text
Chronicle
├── Chronicle Core
└── Official Chronicle Application
```

### Campaign Structure

```text
Campaign
└── Session
    └── Act
        └── Scene
```

### Character Classification

```text
Character
├── Player
└── NonPlayer
```

### Narrative Processing

```text
Chronicle Director
      ↓
Curated Context
      ↓
Narrator
      ↓
Narrative Events
      ↓
Validated Chronicle Operations
```

### Session Finalization

```text
Completed Session
      ↓
Archivist Proposal
      ↓
Validation
      ↓
Campaign Memories and State Changes
```

## 52. Naming Guidelines for Code

### 52.1 Types

Use PascalCase:

```text
CampaignMemory
ChronicleDirector
NarrativeEvent
RuleSet
```

### 52.2 Variables and Properties

Use the language's standard local naming convention.

Example in camelCase:

```text
campaignMemory
activeScene
ruleSetId
```

### 52.3 Identifiers

Use explicit identifier types where supported:

```text
CampaignId
CharacterId
SessionId
ActId
SceneId
MemoryId
DiceRollId
```

Avoid passing unrelated identifiers as generic strings.

### 52.4 Interfaces

Interface naming follows the selected language and framework convention.

The domain name itself MUST remain canonical.

Examples:

```text
Narrator
INarrator
NarratorPort
```

The final style will be defined in a technology-specific RFC or style guide.

### 52.5 Services

Service names MUST describe one clear responsibility.

Good:

```text
ChronicleDirector
SessionFinalizer
MemoryAgingPolicy
RuleKnowledgeRetriever
```

Avoid:

```text
CampaignService
StoryManager
AiService
UtilityManager
CoreHelper
```

unless the responsibility is formally precise.

## 53. Naming Guidelines for Contracts

Structured contracts SHOULD express intent.

Good:

```text
NarrateSceneRequest
NarrateSceneResponse
FinalizeSessionRequest
ArchivistProposal
RequestDiceRoll
ContinueAfterRollRequest
```

Avoid:

```text
AiRequest
GenericResponse
Payload
Data
ResultObject
```

Contracts SHOULD be versioned when compatibility matters.

## 54. Naming Guidelines for UI

UI text MAY be more natural than code identifiers.

Examples in Portuguese:

| Canonical Concept | Suggested UI Text |
|---|---|
| Campaign | Crônica or Campanha, based on product decision |
| Chronicle Director | Diretor da Crônica |
| Narrator | Narrador |
| Archivist | Arquivista |
| Campaign Memory | Fato Passado or Memória da Crônica |
| Session | Sessão |
| Act | Ato |
| Scene | Cena |
| Dice Roll | Rolagem de Dados |
| Rule Set | Sistema de RPG |

UI terminology that differs from domain terminology MUST be mapped explicitly.

The product MUST avoid using two UI terms for the same canonical concept without a documented reason.

## 55. Open Terminology Decisions

The following user-facing names remain open:

- whether the official Portuguese UI uses `Crônica` or `Campanha` for `Campaign`;
- whether `Campaign Memory` appears as `Memória`, `Memória da Crônica`, or `Fato Passado`;
- whether the English UI uses `Campaign` consistently or uses `Chronicle` as a thematic display label;
- whether `Rule Set` appears as `RPG System` in all user-facing contexts;
- whether `Narrative Intelligence` is ever exposed to end users.

These questions affect presentation, not the canonical domain model.

## 56. Review Checklist

Before introducing a new domain term, contributors MUST ask:

- Does an existing canonical term already express this concept?
- Is this a genuinely new domain concept?
- Is the term domain-level or infrastructure-level?
- Does it duplicate ownership?
- Can it be confused with Session, Act, Scene, State, Memory, or Event?
- Does it bind the domain to one provider or technology?
- Is it needed by Current Delivery?
- Will the same term be used consistently in code and documentation?

A new term SHOULD NOT be accepted without clear answers.

## 57. Final Principle

The same concept must have the same name.

Different concepts must not share one name.

In Chronicle, naming is not decoration.

Naming is architecture.
