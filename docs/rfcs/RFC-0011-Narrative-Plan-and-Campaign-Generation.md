---
id: RFC-0011
title: Narrative Plan and Campaign Generation
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-07-31
category: Domain
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
---

> **"A plan gives the Campaign direction. Player choice gives it life."**

# Narrative Plan and Campaign Generation

## Abstract

This RFC defines the Narrative Plan and Campaign Generation model of Chronicle.

It establishes how a new Campaign is prepared, how campaign premises, Acts, Scenes, NPCs, conflicts, mysteries, and possible outcomes are proposed, validated, persisted, revised, hidden, and used by the Chronicle Director.

The Narrative Plan is guidance.

It is not immutable truth and MUST NOT override validated player action.

## 1. Purpose

Chronicle requires enough preparation to avoid directionless improvisation.

A Campaign SHOULD begin with:

- a coherent premise;
- a central conflict;
- meaningful NPCs;
- an initial dramatic structure;
- potential mysteries;
- possible consequences;
- a reason for the Player Character to become involved.

At the same time, Chronicle MUST preserve player agency.

The Narrative Plan exists to provide direction without becoming a rigid script.

## 2. Scope

This RFC defines:

- Campaign generation workflow;
- Campaign generation inputs;
- Campaign proposal;
- Narrative Plan;
- Campaign premise;
- themes and tone;
- central conflict;
- Acts;
- planned Scenes;
- NPC generation;
- secrets and mysteries;
- possible outcomes;
- hidden information;
- plan revision;
- divergence;
- validation;
- persistence;
- idempotency;
- Chronicle Director access.

This RFC does not define:

- exact provider prompt;
- exact provider schema;
- exact UI flow;
- Rule Set retrieval implementation;
- detailed NPC Character Sheet schema;
- Dice Roll algorithms;
- finalization behavior;
- multimedia asset generation.

## 3. Campaign Generation Definition

Campaign Generation is the controlled workflow that transforms:

```text
Rule Set
+
Player Character
+
Campaign Preferences
+
Generation Constraints
```

into:

```text
Validated Campaign Proposal
```

The proposal is not persistent Campaign truth until Chronicle validates and applies it.

## 4. Campaign Generation Goals

Campaign Generation SHOULD produce:

- Campaign identity;
- premise;
- tone;
- themes;
- central conflict;
- initial Narrative Plan;
- initial Acts;
- initial Scenes;
- persistent NPCs;
- initial Relationships;
- initial Secrets;
- possible outcomes;
- Campaign-specific metadata.

It SHOULD create enough material to begin play.

It SHOULD NOT attempt to fully script the entire Campaign.

## 5. Campaign Generation Inputs

A generation request SHOULD include:

- Rule Set identifier;
- Rule Set version;
- Player Character snapshot;
- Campaign Preferences;
- requested tone;
- requested themes;
- campaign length guidance;
- content boundaries;
- allowed customization;
- generation contract version;
- operation identifier.

The request MAY include:

- preferred setting;
- desired conflict type;
- desired narrative intensity;
- desired horror level;
- desired political focus;
- desired combat frequency;
- desired mystery emphasis;
- custom house rules already validated by the Rule Set.

## 6. Campaign Preferences

Campaign Preferences represent player-approved narrative customization.

Examples:

```text
Focus on personal horror.
Keep political intrigue central.
Use a darker tone.
Reduce combat frequency.
Disable tests for a specific approved action.
```

Preferences MUST be validated before generation.

They MUST NOT redefine core domain invariants.

They MAY modify Rule Set behavior only through approved Rule Set extension points.

## 7. Campaign Proposal

A `CampaignProposal` is structured provider output.

It SHOULD contain:

- proposed Campaign title;
- premise;
- tone;
- themes;
- central conflict;
- Player Character hook;
- planned Acts;
- planned Scenes;
- NPC proposals;
- initial Relationships;
- Secrets;
- mysteries;
- possible outcomes;
- risk notes;
- validation metadata.

A proposal MUST NOT assign persistent identifiers.

Chronicle creates identifiers after validation.

## 8. Narrative Plan Definition

The `NarrativePlan` is the persistent adaptable structure that guides Campaign direction.

It MAY include:

- Campaign premise;
- dramatic thesis;
- central conflict;
- planned Acts;
- planned Scenes;
- NPC roles;
- mysteries;
- Secrets;
- revelations;
- possible outcomes;
- pacing guidance;
- revision history.

The plan is hidden by default.

## 9. Narrative Plan Is Not Campaign Truth

The Narrative Plan represents intended possibilities.

Campaign State represents current truth.

Campaign Memory represents preserved meaning.

```text
Narrative Plan:
What may happen

Campaign State:
What is true now

Campaign Memory:
What remains meaningful from what happened
```

A planned event MUST NOT be treated as completed history.

## 10. Campaign Premise

The Campaign premise SHOULD explain:

- who the Player Character is in this Campaign;
- what situation surrounds them;
- what force creates tension;
- why action matters;
- what kind of story Chronicle intends to support.

A good premise is specific enough to guide play and open enough to allow divergence.

## 11. Dramatic Thesis

The Narrative Plan MAY define a dramatic thesis.

Examples:

```text
Power without trust destroys the pack.
```

```text
Every alliance in the city has a hidden cost.
```

The thesis guides tone and thematic consistency.

It MUST NOT force outcomes.

## 12. Tone

Tone MAY include structured values such as:

```text
Horror
Mystery
Political
Tragic
Heroic
Intimate
Violent
Melancholic
Hopeful
```

The exact model may be:

- categorical;
- weighted;
- free-form with validation;
- hybrid.

Tone SHOULD influence Narrator behavior and Campaign generation.

## 13. Themes

Themes represent recurring narrative concerns.

Examples:

- loyalty;
- identity;
- corruption;
- responsibility;
- sacrifice;
- memory;
- power;
- family;
- survival.

Themes SHOULD influence:

- NPC motivations;
- conflicts;
- Acts;
- Scenes;
- Secrets;
- possible outcomes.

## 14. Central Conflict

A Campaign SHOULD have at least one central conflict.

A central conflict SHOULD define:

- opposing forces;
- stakes;
- why the Player Character matters;
- what may worsen over time;
- what success and failure could mean.

The conflict MAY evolve during play.

## 15. Campaign Hook

The Player Character hook explains why the Character becomes involved.

It SHOULD connect:

- Character history;
- Character goals;
- Character fears;
- Character Relationships;
- Rule Set identity;
- central conflict.

A generic hook unrelated to the Player Character SHOULD be rejected or repaired.

## 16. Planned Act

A planned Act SHOULD contain:

- title;
- dramatic objective;
- purpose;
- expected conflict;
- planned Scenes;
- important NPC roles;
- relevant Secrets;
- possible transitions;
- completion signals;
- optional failure direction;
- order guidance.

A planned Act is not yet an executed Act.

## 17. Planned Scene

A planned Scene SHOULD contain:

- title;
- purpose;
- expected location;
- intended participants;
- immediate objective;
- active conflict;
- required setup;
- hidden information;
- possible Roll opportunities;
- possible outcomes;
- transition guidance;
- optional status.

Planned participants are proposals.

Executed Scene participants MUST be validated explicitly at activation.

## 18. Plan Granularity

Campaign Generation SHOULD create enough Scenes to begin directionally.

It SHOULD NOT create every future Scene in detail.

Recommended approach:

```text
Near-term:
Detailed Acts and Scenes

Mid-term:
Acts with partial Scene guidance

Long-term:
Broad conflicts and possibilities
```

This reduces waste when player choices change direction.

## 19. NPC Generation

Campaign Generation MAY propose persistent NPCs.

Each NPC proposal SHOULD include:

- name;
- narrative role;
- power classification;
- Character Sheet values;
- Narrative Profile;
- goals;
- fears;
- motivations;
- Secrets;
- visibility;
- initial Relationships;
- planned Act or Scene relevance;
- reason for inclusion.

NPC proposals MUST be validated through the Character and Rule Set models.

## 20. NPC Role Diversity

Campaign Generation SHOULD avoid generating NPCs with redundant roles.

Useful roles MAY include:

- ally;
- rival;
- mentor;
- antagonist;
- witness;
- victim;
- authority;
- dependent;
- unknown force;
- moral counterpoint.

Role labels are planning metadata.

They MUST NOT dictate permanent behavior.

## 21. NPC Power Diversity

Generation SHOULD create a varied cast.

Initial guidance MAY include:

- several NPCs comparable to the Player Character;
- some weaker or situationally vulnerable NPCs;
- some stronger or dangerous NPCs;
- at least one force the Player Character cannot defeat casually.

The exact distribution is not a domain invariant.

## 22. Initial Relationships

Campaign Generation MAY propose initial Relationships.

A proposed Relationship MUST include:

- source Character;
- target Character;
- structured dimensions;
- reason;
- visibility;
- related Secret or plan role;
- confidence.

Chronicle validates and persists accepted Relationships.

## 23. Secrets

Campaign Generation MAY propose Secrets.

A Secret SHOULD include:

- canonical truth;
- associated entity;
- who knows it initially;
- player visibility;
- intended reveal conditions;
- importance;
- relationship to the central conflict;
- possible partial revelations.

Secrets MUST NOT exist only inside provider prose.

## 24. Mysteries

A mystery is a planned unanswered question.

Examples:

```text
Who betrayed the pack?
Why has the spirit stopped answering?
What happened beneath the northern gate?
```

A mystery SHOULD define:

- question;
- underlying truth;
- clues;
- possible false leads;
- involved Characters;
- reveal constraints;
- consequences of discovery.

Mystery truth MUST be persisted as hidden Campaign data.

## 25. Clues

A clue MAY be planned as:

- Scene content;
- Character knowledge;
- Object;
- Campaign Memory;
- Secret fragment;
- Rule Set effect.

Clues MUST NOT guarantee discovery unless the plan explicitly requires it.

Dice Rolls and player action may determine acquisition.

## 26. Possible Outcomes

A Narrative Plan MAY define possible outcomes.

Examples:

- alliance;
- betrayal;
- victory;
- partial success;
- loss;
- escape;
- corruption;
- sacrifice;
- unresolved continuation.

Possible outcomes are not predetermined results.

They help Chronicle prepare coherent consequences.

## 27. Failure Directions

The plan SHOULD include ways for failure to continue the Campaign.

Example:

```text
Failure:
The fortress falls.

Continuation:
The Campaign shifts into survival and resistance.
```

Chronicle SHOULD avoid plans where one failed test destroys all future play without explicit design intent.

## 28. Hidden Information

The Narrative Plan is hidden system information by default.

The player MUST NOT see:

- planned betrayals;
- future revelations;
- hidden NPC roles;
- possible outcome probabilities;
- provider reasoning;
- unused alternative Scenes;
- secret truth.

The Chronicle Director MAY access plan data required for the current operation.

## 29. Campaign Generation Validation

Validation SHOULD verify:

- valid Rule Set;
- valid Player Character;
- Campaign Preferences compatibility;
- structural contract validity;
- no duplicate NPC identities;
- valid Character Sheets;
- valid Relationship references;
- valid Secrets;
- coherent Campaign hook;
- valid Act and Scene hierarchy;
- hidden-information classification;
- no contradiction with Character history;
- no prohibited content;
- no unsupported Rule Set mechanics.

## 30. Narrative Coherence Validation

Chronicle SHOULD validate that:

- the premise matches the tone;
- the central conflict involves the Player Character;
- planned Acts support the premise;
- NPC motivations are not obviously contradictory;
- Secrets have valid owners;
- planned Scenes belong to valid Acts;
- outcomes do not predefine player decisions;
- the plan contains enough material to begin play.

Some coherence checks MAY use Narrative Intelligence.

Acceptance remains Chronicle-controlled.

## 31. Proposal Repair

If a proposal is structurally invalid, Chronicle MAY:

- reject it;
- request provider repair;
- repair deterministic formatting;
- regenerate only the invalid section;
- fall back to a simpler proposal.

Repair MUST NOT invent domain meaning through unsafe parsing.

## 32. Campaign Creation Transaction

After validation, Chronicle SHOULD persist atomically:

- Campaign;
- Campaign Preferences;
- Player Character;
- generated NPCs;
- initial Relationships;
- initial Secrets;
- Narrative Plan;
- initial Campaign State;
- Rule Set identity;
- operation result.

Partial Campaign creation MUST NOT produce a playable Campaign.

## 33. Campaign Generation Idempotency

Campaign Generation MUST be idempotent.

Repeated execution with the same operation identifier MUST:

- return the accepted Campaign;
- return the existing proposal status;
- or fail safely on request conflict.

It MUST NOT create duplicate NPCs, Plans, or Campaigns.

## 34. Campaign Generation States

A Campaign generation workflow MAY use:

```text
Requested
Generating
Proposed
Validating
Accepted
Failed
Cancelled
```

The Campaign remains `Draft` until accepted generation and Character validation complete.

## 35. Plan Versioning

The Narrative Plan MUST be versioned.

Each revision SHOULD include:

- plan version;
- prior version;
- revision source;
- reason;
- affected Acts and Scenes;
- triggering Session;
- timestamp;
- operation identifier.

Completed history MUST remain unchanged.

## 36. Plan Revision

A plan MAY be revised when:

- player action changes direction;
- an important NPC dies;
- an alliance forms unexpectedly;
- a Secret is revealed early;
- a planned conflict becomes irrelevant;
- a new obligation emerges;
- a planned Act is completed differently;
- a Rule Set consequence demands adaptation.

Revision SHOULD preserve continuity with accepted Campaign truth.

## 37. Plan Divergence

Divergence occurs when executed play no longer follows the expected plan.

Divergence is not an error.

Chronicle SHOULD respond by:

- preserving executed history;
- marking obsolete planned content;
- revising future Acts or Scenes;
- retaining reusable NPCs and Secrets where valid;
- creating new possible directions.

## 38. Obsolete Plan Content

Planned content MAY become:

```text
Active
Available
Obsolete
Skipped
Consumed
Revised
Archived
```

Obsolete content SHOULD remain historically inspectable.

It SHOULD NOT enter normal context selection.

## 39. Plan Revision Proposal

A `PlanRevisionProposal` SHOULD contain:

- reason;
- triggering occurrence;
- affected plan version;
- proposed changes;
- new Acts;
- revised Acts;
- new Scenes;
- skipped Scenes;
- NPC role changes;
- Secret changes;
- expected consequences;
- validation metadata.

The proposal MUST NOT rewrite completed entities.

## 40. Chronicle Director Use

The Chronicle Director uses the Narrative Plan to:

- select the next dramatic objective;
- prepare an Act;
- prepare a Scene;
- identify relevant planned NPCs;
- identify hidden information;
- maintain pacing;
- propose transitions;
- request plan revision.

The Director MUST also consider:

- Campaign State;
- Campaign Memories;
- Character Knowledge;
- Relationships;
- Player action;
- Rule Set constraints.

The plan is one input, not the authority.

## 41. Scene Activation from Plan

A planned Scene becomes an executed Scene only after:

- ownership validation;
- participant validation;
- Character existence validation;
- hidden-information filtering;
- Scene objective validation;
- compatibility with current Campaign State;
- persistence.

The executed Scene receives its own persistent identifier.

## 42. Dynamic Scene Creation

The Chronicle Director MAY create a Scene outside the original plan when required by player action.

A dynamic Scene SHOULD still define:

- purpose;
- Act relationship;
- participants;
- location;
- objective;
- conflict;
- hidden information;
- transition guidance.

Dynamic Scenes become part of executed history.

They MAY trigger plan revision.

## 43. Dynamic NPC Creation

The MVP SHOULD minimize unplanned persistent NPC creation during play.

When necessary, a new NPC proposal MUST be structured and validated.

Minor unnamed background figures MAY remain transient if they do not require persistence.

A transient figure MUST become a persistent Character before:

- repeated appearance;
- meaningful Relationship;
- Campaign Memory association;
- mechanical state;
- Secret ownership;
- plot-critical role.

## 44. Transient Narrative Elements

Not every described entity requires persistence.

Transient elements MAY include:

- unnamed crowd members;
- incidental environmental details;
- nonmeaningful objects;
- generic background activity.

Chronicle SHOULD promote a transient element to persistent state only when it becomes meaningful.

## 45. Campaign Length Guidance

Campaign Preferences MAY include length guidance.

Examples:

```text
Short
Medium
Long
OpenEnded
```

Length guidance MAY influence:

- Act count;
- pacing;
- mystery complexity;
- NPC count;
- conflict escalation.

It MUST NOT guarantee exact Session count.

## 46. Pacing Guidance

The Narrative Plan MAY define pacing hints.

Examples:

- slow reveal;
- immediate crisis;
- alternating tension;
- escalating danger;
- quiet recovery Scene;
- major revelation after setup.

Pacing is guidance.

The Chronicle Director adapts it to actual play.

## 47. Campaign Completion Conditions

The Narrative Plan MAY define completion signals.

Examples:

- central conflict resolved;
- defining choice made;
- primary antagonist defeated or transformed;
- Campaign thesis concluded;
- Player Character objective resolved.

Chronicle validates completion based on executed state.

A planned ending is not an automatic Campaign completion.

## 48. Alternative Endings

The plan MAY include multiple ending directions.

They SHOULD reflect possible consequences rather than fixed scripts.

Chronicle SHOULD avoid exposing them to the player.

## 49. Rule Set Integration

The Rule Set MAY influence:

- allowed Campaign concepts;
- Character creation;
- NPC mechanics;
- dice opportunities;
- progression;
- system terminology;
- setting constraints;
- supernatural rules;
- power levels.

Campaign Generation MUST not invent unsupported mechanics.

## 50. Rule Knowledge Retrieval

Campaign Generation MAY request relevant Rule Set knowledge.

It SHOULD retrieve:

- Character creation rules;
- setting guidance;
- NPC requirements;
- system terminology;
- mechanical constraints;
- Campaign-specific concepts.

It MUST NOT load the entire Rule Set indiscriminately.

## 51. Licensing Boundary

Chronicle MUST NOT persist or distribute proprietary sourcebook text merely because it was used for retrieval.

Generated plan content SHOULD be original and Campaign-specific.

Rule Set packages MUST comply with licensing requirements.

This RFC does not authorize distribution of copyrighted game text.

## 52. Provider Neutrality

Campaign Generation MUST use a provider-neutral port.

The Domain MUST not depend on:

- provider model names;
- chat threads;
- provider assistants;
- provider file stores;
- provider-specific tool calls.

Provider adapters implement the generation capability.

## 53. Structured Contract

Campaign Generation output MUST be structured.

Free-form prose MAY exist inside approved fields such as:

- premise;
- description;
- motivation;
- summary.

Entity relationships, hierarchy, visibility, and references MUST be machine-readable.

## 54. Stale Proposal Detection

A Campaign proposal becomes stale when:

- Player Character changed;
- Campaign Preferences changed;
- Rule Set version changed;
- generation request version changed;
- another proposal was accepted.

Stale proposals MUST NOT be applied.

## 55. Concurrency

Campaign generation and plan revision SHOULD use optimistic concurrency.

A Plan revision based on an old Campaign version MUST fail or be regenerated.

Two Plan revisions MUST NOT silently overwrite one another.

## 56. Auditability

Chronicle SHOULD preserve:

- generation operation;
- contract version;
- provider metadata;
- validation result;
- accepted proposal;
- rejected sections;
- plan versions;
- revision reasons;
- triggering Sessions;
- affected content.

Provider hidden reasoning is not required and SHOULD NOT be persisted.

## 57. Read Models

Recommended read models include:

```text
CampaignCreationProgressView
CampaignPublicSummary
NarrativePlanInternalView
PlannedActInternalView
PlannedSceneInternalView
CampaignGenerationFailureView
```

The player-facing Campaign summary MUST exclude hidden plan data.

## 58. UI Behavioral Requirements

The official application SHOULD show:

- generation progress;
- validation failures;
- recoverable retry options;
- final Campaign title and public premise;
- Character validation status.

The UI SHOULD NOT display raw provider contracts.

## 59. Failure Recovery

### 59.1 Provider Failure

The Campaign remains `Draft`.

The operation MAY be retried.

### 59.2 Structural Validation Failure

Chronicle MAY request repair or regeneration.

### 59.3 Rule Set Validation Failure

Invalid Characters, NPCs, or mechanics MUST be repaired or rejected.

### 59.4 Persistence Failure

No partial playable Campaign may remain.

### 59.5 Application Restart

Generation state SHOULD be recoverable through operation records.

## 60. Testing Requirements

Tests MUST cover:

- valid Campaign proposal;
- invalid hierarchy;
- invalid NPC Character Sheet;
- duplicate NPC identity;
- incompatible Campaign Preferences;
- hidden-information filtering;
- stale proposal rejection;
- generation idempotency;
- atomic Campaign creation;
- plan versioning;
- plan revision;
- player-driven divergence;
- dynamic Scene creation;
- completed history preservation;
- provider failure recovery;
- Rule Set version preservation.

## 61. Prohibited Patterns

### 61.1 Full Campaign Script

Generation MUST NOT fully script every future event as fixed history.

### 61.2 Plan as Truth

Planned outcomes MUST NOT enter Campaign State automatically.

### 61.3 Provider-Owned Persistence

The provider MUST NOT create persistent Campaign entities directly.

### 61.4 Hidden Plan in Player Context

Secrets and future outcomes MUST NOT leak into player-visible output.

### 61.5 Regenerate Existing NPC Identity

Persistent NPCs MUST NOT be replaced by new versions during plan revision.

### 61.6 Rewrite Completed History

Plan changes MUST NOT alter completed Sessions, Acts, or Scenes.

### 61.7 Rule Invention

Campaign generation MUST NOT create unsupported Rule Set mechanics.

### 61.8 Speculative Content Explosion

The system SHOULD NOT generate large volumes of unused detailed content.

## 62. Current Delivery Decision

The MVP adopts:

- structured Campaign generation;
- one Campaign proposal workflow;
- Player Character-aware premise;
- initial NPC generation;
- initial Relationships and Secrets;
- initial Narrative Plan;
- planned Acts and Scenes;
- hidden plan data;
- versioned plan;
- player-driven divergence;
- plan revision;
- dynamic Scene creation;
- minimal dynamic persistent NPC creation;
- no full procedural world simulation;
- no multimedia generation;
- no multiple competing plan engines.

## 63. Architecture Horizon

Future evolution MAY include:

- collaborative Campaign creation;
- player-selectable proposal variants;
- imported adventures;
- community Campaign templates;
- procedural world regions;
- dynamic faction simulation;
- long-running autonomous NPC planning;
- multimedia pre-generation;
- multiple generation providers;
- Campaign remixing;
- shared multiplayer planning.

The MVP MUST NOT implement these capabilities without a later milestone.

## 64. Open Questions

The following remain open:

- How many Acts and Scenes should initial generation create?
- Should the player review or approve the public premise before acceptance?
- Should NPC generation happen in one request or separate requests?
- How much Rule Set knowledge is required during generation?
- How should content boundaries be represented?
- Should generation produce one proposal or multiple options?
- Which Plan fields should be relational and which document-shaped?
- When should plan revision occur automatically?
- How much of the Plan may the Chronicle Director revise deterministically?
- Should unused planned NPCs be archived after Campaign completion?
- How should mystery clues be represented?
- Should failure directions be mandatory?
- Which Campaign Preferences may modify Rule Set behavior?
- What plan content may be exposed in a Campaign dashboard?
- How should plan quality be evaluated?

These questions require later contract, Rule Set, and UI RFCs.

## 65. Compliance Checklist

An implementation complies when:

- generation is structured;
- proposals are validated;
- persistent identifiers are Chronicle-generated;
- the Player Character influences the Campaign hook;
- plan and truth remain separate;
- hidden information is protected;
- planned hierarchy is valid;
- NPCs are persistent and validated;
- player action may diverge from the plan;
- revisions preserve completed history;
- stale proposals are rejected;
- generation is idempotent;
- Campaign creation is atomic;
- Rule Set mechanics remain authoritative;
- provider-specific behavior stays outside the Domain.

## 66. Final Principle

The Narrative Plan prepares possibilities.

The Chronicle Director chooses what is relevant.

The player decides what actually becomes the Campaign.
