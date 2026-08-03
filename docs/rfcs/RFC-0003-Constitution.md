---
id: RFC-0003
title: Constitution
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-07-31
category: Foundation
depends_on:
  - RFC-0000
  - RFC-0001
  - RFC-0002
---

> **"Narrative may interpret reality. It must never become reality by itself."**

# Constitution

## Abstract

This RFC defines the constitutional laws of Chronicle.

These laws govern architecture, domain behavior, implementation decisions, and project scope. They are intentionally concise and normative.

Any RFC, ADR, implementation, integration, or contribution that conflicts with this Constitution MUST be revised or rejected.

Chronicle follows the normative language defined by RFC 2119:

- **MUST** and **MUST NOT** indicate mandatory requirements;
- **SHOULD** and **SHOULD NOT** indicate strong recommendations;
- **MAY** indicates an allowed option.

## 1. Authority of This Document

This Constitution is the highest-level technical and product authority of the Chronicle project.

When documents conflict, the following order applies:

```text
Constitution
    ↓
Approved RFCs
    ↓
Approved ADRs
    ↓
Implementation
```

Implementation does not redefine architecture.

Code that contradicts approved documentation is incorrect until the documentation is deliberately amended.

## 2. LAW-001 — Single Source of Truth

The Chronicle Core MUST be the sole authority over persistent Campaign truth.

No Narrator, provider, client, prompt, transcript, cache, or external component MAY become the authoritative source of persistent state.

Persistent truth includes, but is not limited to:

- Characters;
- Character Sheets;
- Campaign State;
- Campaign Memories;
- Relationships;
- Sessions;
- Acts;
- Scenes;
- Dice Rolls;
- progression;
- Rule Set identity;
- unresolved actions.

## 3. LAW-002 — Narrative Follows State

Narrative MUST be generated from validated Campaign State.

Narrative MUST NOT independently redefine Campaign State.

Narrative Intelligence MAY propose:

- actions;
- interpretations;
- transitions;
- Memories;
- state changes;
- Dice Roll requests.

Chronicle MUST validate every persistent consequence before applying it.

## 4. LAW-003 — Memory Belongs to Chronicle

Persistent memory MUST belong to Chronicle.

Narrative Intelligence MUST receive temporary, curated context.

Chronicle MUST NOT depend on provider conversation memory, hidden model state, or replay of complete transcripts as the primary memory mechanism.

Session transcripts MAY support audit, recovery, summarization, and context reconstruction.

They MUST NOT replace explicit domain memory.

## 5. LAW-004 — Randomness Belongs to Chronicle

Narrative Intelligence MUST NOT generate authoritative random results.

All Dice Rolls and other game-relevant random operations MUST be executed by Chronicle or by a deterministic Rule Set adapter controlled by Chronicle.

The validated result MUST be persisted before narrative continuation.

## 6. LAW-005 — Rules Belong to the Rule Set

RPG rules MUST be represented through the active Rule Set and Chronicle-controlled logic.

Narrative Intelligence MUST NOT become the final authority on:

- dice mechanics;
- Character Sheet validity;
- resource calculation;
- progression;
- mechanical outcomes;
- Rule Set terminology.

Narrative Intelligence MAY assist in interpreting ambiguous rules only when the result is explicitly treated as a proposal.

## 7. LAW-006 — Context Is Curated

Chronicle MUST send only the context required for the current task.

The entire database MUST NOT be exposed to Narrative Intelligence by default.

The active Scene SHOULD be the primary narrative context boundary.

Context selection MUST respect:

- participant isolation;
- hidden information;
- Character knowledge;
- Campaign Memory relevance;
- Rule Set relevance;
- current objectives;
- unresolved tests;
- provider limits.

## 8. LAW-007 — Structured Boundaries

Communication across nondeterministic boundaries MUST use structured contracts.

This includes communication with:

- the Narrator;
- the Archivist;
- Campaign generation services;
- Rule Set retrieval services;
- future multimedia services.

Free-form text MAY exist inside fields explicitly intended for prose.

State-changing intent MUST be represented structurally.

Invalid structured output MUST NOT alter persistent state.

## 9. LAW-008 — Replaceable Narrative Intelligence

Narrative Intelligence MUST remain replaceable.

Chronicle MUST NOT require architectural redesign when changing:

- model;
- provider;
- local or remote execution;
- inference library;
- prompt implementation;
- structured-output mechanism.

Provider-specific behavior MUST remain behind explicit abstractions.

The MVP MAY implement only one provider.

## 10. LAW-009 — Stable Domain, Replaceable Infrastructure

The domain model MUST NOT depend directly on:

- databases;
- vector stores;
- embedding models;
- transport protocols;
- UI frameworks;
- deployment platforms;
- provider SDKs;
- serialization libraries.

Infrastructure MAY implement domain interfaces.

Infrastructure MUST NOT define domain meaning.

## 11. LAW-010 — One Owner per Persistent Responsibility

Every persistent concept MUST have one authoritative owner.

Duplicated ownership is forbidden.

Examples:

- Character Sheet state belongs to the Character domain;
- Campaign Memories belong to the Campaign;
- Dice Roll execution belongs to Chronicle;
- narrative prose belongs to the Narrator response;
- Session finalization proposals belong to the Archivist workflow;
- persistence belongs to the persistence layer.

Modules MAY collaborate.

They MUST NOT silently own the same state.

## 12. LAW-011 — The Chronicle Director Orchestrates

The Chronicle Director MUST coordinate the active experience.

The Chronicle Director MUST NOT absorb the responsibilities of other modules.

It MUST NOT:

- generate narrative prose;
- roll dice;
- become the database;
- define Rule Set mechanics;
- directly rewrite Character Sheets;
- replace the Archivist;
- become a provider-specific agent.

The Chronicle Director directs.

It does not perform every task.

## 13. LAW-012 — Scene Isolation

The active Scene MUST define the immediate narrative boundary.

Scene participants MUST be explicit.

Characters in one Scene MUST NOT be assumed to participate in sibling Scenes.

Scene-local state, objectives, hidden information, and conflicts MUST remain isolated unless an explicit transition or shared Campaign fact connects them.

This law exists to prevent narrative leakage and context contamination.

## 14. LAW-013 — Persistent Characters

Characters MUST exist independently from their visibility in the current narrative.

An NPC who is absent, hidden, missing, retired, or dead remains a persistent Character unless explicitly removed through an approved domain operation.

Narrative Intelligence MUST NOT recreate persistent Characters as new identities.

Character identity MUST remain stable across Sessions.

## 15. LAW-014 — History Is Preserved

Completed Sessions, Acts, Scenes, Messages, Dice Rolls, and accepted Memories SHOULD be preserved.

Normal operation MUST favor archival, supersession, and correction over destructive deletion.

Completed history MUST NOT be silently rewritten.

Explicit correction workflows MAY be introduced later.

## 16. LAW-015 — Finalization Is Idempotent

Session finalization MUST be idempotent.

Repeating a finalization operation MUST NOT duplicate:

- experience;
- Memories;
- Relationship changes;
- progression;
- Character State changes;
- summaries;
- rewards.

The system MUST be able to detect whether a finalization result has already been applied.

## 17. LAW-016 — Unresolved Tests Block Progression

When a Scene is awaiting a Dice Roll, normal narrative progression MUST stop.

The application MUST preserve the interruption point.

Narration MAY continue only after:

1. the player triggers the roll;
2. Chronicle executes it;
3. the Rule Set resolves it;
4. the result is persisted;
5. the Narrator receives the validated outcome.

## 18. LAW-017 — Player Agency Prevails

The Narrative Plan MUST guide play without becoming a rigid script.

Validated player action MAY:

- change Scene order;
- interrupt an Act;
- remove planned content;
- create new consequences;
- redirect objectives;
- produce alternative outcomes.

Chronicle MUST NOT force a planned outcome by invalidating meaningful player decisions.

## 19. LAW-018 — Hidden Information Is Protected

Chronicle MUST preserve the boundary between:

- what exists;
- what the player knows;
- what each Character knows;
- what the Narrator may reveal.

Hidden NPCs, secrets, planned revelations, and private Character knowledge MUST NOT be included in player-visible output unless explicitly authorized by the current narrative transition.

## 20. LAW-019 — Architecture Supports Growth; Delivery Remains Focused

Chronicle architecture SHOULD support future evolution.

Current implementation MUST include only what is required by the active milestone.

Future possibilities MUST NOT justify premature implementation.

The existence of an architectural extension point does not create a delivery commitment.

The project MUST prefer:

- one complete Rule Set over several incomplete Rule Sets;
- one complete client over several unfinished clients;
- one reliable provider integration over speculative provider breadth;
- one finished player journey over disconnected features.

## 21. LAW-020 — Framework First, Product Complete

Chronicle is a framework with an official application.

Framework concerns MUST NOT prevent the official application from delivering a complete, focused experience.

The framework MUST remain reusable.

The MVP MUST remain playable.

Neither goal may be used to neglect the other.

## 22. LAW-021 — No Architecture by Prompt Accident

Prompt behavior MUST NOT become undocumented architecture.

If a prompt introduces a persistent rule, workflow, domain concept, or contract dependency, that behavior MUST be documented and reviewed.

Prompt engineering MAY refine communication.

It MUST NOT silently define core behavior.

Architecture takes precedence over prompt cleverness.

## 23. LAW-022 — Fail Explicitly

Chronicle MUST NOT report success when a persistent operation failed.

Failures involving:

- Campaign saves;
- Dice Rolls;
- Session finalization;
- Character updates;
- Memory updates;
- provider responses;
- structured validation;

MUST be explicit, recoverable where possible, and observable for debugging.

Silent data loss is unconstitutional.

## 24. LAW-023 — Security and Privacy by Boundary

Narrative Intelligence MUST receive only data required for the active operation.

Secrets, hidden Campaign content, provider credentials, internal metadata, and unrelated user data MUST remain outside prompts and client-visible output.

Future integrations MUST follow least-privilege access.

## 25. LAW-024 — Testable Determinism

Deterministic domain behavior MUST be testable without Narrative Intelligence.

The following SHOULD be testable independently:

- lifecycle transitions;
- Campaign invariants;
- Scene isolation;
- Dice Roll resolution;
- Memory aging;
- finalization idempotency;
- Rule Set validation;
- state persistence;
- structured response validation.

A provider outage MUST NOT make deterministic domain tests impossible.

## 26. LAW-025 — Names Carry Domain Meaning

Domain names MUST remain consistent across:

- RFCs;
- ADRs;
- code;
- contracts;
- database mappings;
- user-facing terminology where appropriate.

Synonyms that blur distinct concepts SHOULD be avoided.

The following hierarchy is canonical:

```text
Campaign
└── Session
    └── Act
        └── Scene
```

The following role name is canonical:

```text
Chronicle Director
```

In code, the canonical identifier is:

```text
ChronicleDirector
```

## 27. Constitutional Review

A proposed change to this Constitution MUST include:

- the law being changed;
- the problem the change solves;
- affected RFCs and ADRs;
- migration consequences;
- compatibility impact;
- scope impact;
- reasons the existing law is insufficient.

Constitutional changes SHOULD be rare.

They MUST NOT be made merely to legitimize an implementation shortcut.

## 28. Compliance Checklist

A new architecture or implementation proposal MUST answer:

- Does Chronicle remain the source of truth?
- Is persistent state validated?
- Is randomness Chronicle-controlled?
- Are Rule Set responsibilities preserved?
- Is context minimal and isolated?
- Is Narrative Intelligence replaceable?
- Are structured contracts used?
- Is ownership unambiguous?
- Is the active milestone still focused?
- Can deterministic behavior be tested independently?
- Does the proposal preserve player agency?
- Does it protect hidden information?
- Does it avoid silent failure?

A proposal that cannot answer these questions SHOULD NOT be approved.

## 29. Final Principle

The Narrator may be imaginative.

The Chronicle Director may be adaptive.

The Archivist may be interpretive.

Chronicle itself must remain trustworthy.
