---
id: RFC-0043
title: Post-MVP Roadmap and Evolution Strategy
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
  - RFC-0042
---

> **"Chronicle should grow by proving one new capability at a time, without making the present pay for every possible future."**

# Post-MVP Roadmap and Evolution Strategy

## Abstract

This RFC defines Chronicle's post-MVP evolution strategy.

It establishes:

- roadmap principles;
- evidence-driven expansion;
- compatibility policy;
- staged capability growth;
- provider expansion;
- Rule Set expansion;
- local-model support;
- extensibility;
- multiplayer;
- cloud synchronization;
- additional clients;
- creator tooling;
- ecosystem governance;
- deprecation;
- long-term support;
- architectural review gates.

The roadmap is intentionally not a promise of dates.

It is a sequencing model.

Every future capability must justify its cost through validated user need, architectural readiness, security maturity, and evidence from the current product.

## 1. Purpose

Chronicle's architecture intentionally supports more than the MVP delivers.

That creates a risk: architectural possibility may be mistaken for roadmap commitment.

Without a disciplined evolution strategy, Chronicle could:

- pursue multiplayer before single-player is proven;
- add multiple incomplete Rule Sets;
- build a plugin marketplace before package isolation is mature;
- add cloud sync before local persistence is trustworthy;
- optimize for mobile before desktop interaction is understood;
- create provider abstractions that no real adapter needs;
- expand infrastructure faster than the project can maintain;
- break Campaign continuity in the name of rapid innovation.

This RFC defines how Chronicle evolves without abandoning scope discipline.

## 2. Scope

This RFC defines:

- roadmap principles;
- phase sequencing;
- evidence requirements;
- compatibility expectations;
- deprecation;
- migration expectations;
- post-MVP capability groups;
- release maturity;
- ecosystem growth;
- contributor governance;
- architectural decision gates;
- non-goals.

This RFC does not define:

- calendar dates;
- staffing plans;
- funding;
- exact release numbers;
- specific provider partnerships;
- marketplace commercial terms;
- hosting costs;
- legal agreements.

## 3. Core Principle

Chronicle expands only when the current layer is trustworthy.

```text
Prove
    ↓
Stabilize
    ↓
Measure
    ↓
Generalize
    ↓
Expand
```

Generalization before proof is prohibited.

## 4. Roadmap Philosophy

Chronicle follows:

### 4.1 Evidence Before Expansion

A feature enters committed roadmap only after validated need.

### 4.2 Vertical Slices Before Platforms

Complete capabilities are preferred over broad infrastructure.

### 4.3 Backward Compatibility Before Convenience

Campaign continuity has priority over rapid schema change.

### 4.4 One New Risk at a Time

A milestone SHOULD not introduce several major risk classes simultaneously.

### 4.5 Architecture Supports Future, Delivery Serves Present

The architecture may reserve boundaries without implementing unused machinery.

### 4.6 Open Source With Explicit Governance

Community contribution does not mean uncontrolled compatibility.

## 5. Roadmap States

Future capabilities SHOULD be classified as:

```text
Idea
Exploration
Candidate
Committed
InDevelopment
Experimental
Stable
Deprecated
Removed
```

## 6. Idea

An Idea is documented but has no implementation commitment.

## 7. Exploration

Exploration may include:

- prototype;
- user interview;
- architecture spike;
- legal review;
- performance test.

Exploration artifacts are not production guarantees.

## 8. Candidate

A Candidate has:

- clear problem statement;
- initial acceptance criteria;
- architectural fit;
- identified risks;
- evidence of user value.

## 9. Committed

A Committed capability has:

- approved RFC or ADR;
- assigned milestone;
- scope boundary;
- test strategy;
- migration strategy;
- ownership.

## 10. Experimental

Experimental features may be shipped behind explicit flags or channels.

They MUST:

- preserve data safety;
- declare compatibility limits;
- avoid silent activation;
- avoid irreversible migration where possible.

## 11. Stable

A Stable capability becomes part of Chronicle's supported product contract.

Removing it requires deprecation and migration policy.

## 12. Evidence Sources

Roadmap evidence MAY include:

- MVP usability studies;
- issue frequency;
- Campaign completion data provided voluntarily;
- support requests;
- contributor proposals;
- performance measurements;
- provider reliability;
- Rule Set implementation difficulty;
- security review;
- accessibility review;
- legal feasibility.

## 13. Evidence Boundaries

Telemetry is optional.

Chronicle MUST be able to gather roadmap evidence through:

- user reports;
- opt-in feedback;
- public issue tracking;
- manual evaluation;
- local diagnostic bundles shared explicitly.

## 14. Phase Model

Post-MVP evolution SHOULD use capability phases.

Recommended phases:

```text
P1 — Stabilization and Product Fit
P2 — Provider and Rule Set Expansion
P3 — Local Intelligence and Offline Depth
P4 — Extensibility and Creator Tooling
P5 — Multiplayer Foundations
P6 — Multi-Device and Cloud Continuity
P7 — Ecosystem and Advanced Experiences
```

These are sequencing categories, not dates.

## 15. P1 — Stabilization and Product Fit

P1 begins immediately after MVP.

Its goal is to make the existing vertical slice reliable, understandable, and maintainable.

## 16. P1 Priorities

P1 SHOULD prioritize:

- defect reduction;
- performance;
- Campaign continuity;
- finalization quality;
- Memory relevance;
- recovery clarity;
- accessibility;
- diagnostics;
- packaging;
- migration stability;
- documentation;
- legal clarity for the official Rule Set.

## 17. P1 Exit Criteria

P1 is complete when:

- no recurring data-integrity defect remains;
- provider failure states are understandable;
- backup and restore are trustworthy;
- long Campaign performance is acceptable;
- Session finalization quality is consistently useful;
- migration from supported versions is reliable;
- the official Rule Set can support meaningful repeated play.

## 18. P1 Candidate Capabilities

Potential P1 features:

- improved Campaign dashboard;
- richer Memory filtering;
- better Session summaries;
- provider cost display;
- stronger Safe Mode;
- portable Campaign export;
- installer maturity;
- better diagnostics;
- additional accessibility refinements;
- performance improvements.

## 19. P2 — Provider and Rule Set Expansion

P2 validates Chronicle's abstraction boundaries.

The goal is not maximum quantity.

The goal is proof that Chronicle can support variation without changing Core contracts.

The Narrative Intelligence provider implementation order is:

1. generic provider contracts;
2. deterministic development provider for tests, CI, demos, and reproducible development;
3. Ollama as the first real local provider;
4. OpenAI as an optional remote provider;
5. provider selection and configuration after the adapters exist.

This order does not imply that Ollama or OpenAI is implemented already.

## 20. Additional Provider Adapter

A second provider adapter SHOULD be added only after:

- the first adapter is stable;
- provider-neutral gaps are identified;
- adapter conformance tests exist;
- data-handling policy is explicit;
- model differences are measurable.

Under the current provider strategy, Ollama is the first real local provider adapter target and OpenAI remains the first official remote provider target.

Neither provider becomes mandatory for normal development, build, CI, tests, startup, or non-narrative functionality.

## 21. Provider Selection UX

Future provider selection MAY support:

- capability-specific profiles;
- local versus remote choice;
- cost and latency preferences;
- privacy classification;
- fallback policy;
- health comparison.

## 22. Provider Fallback

Automatic fallback is a high-risk feature.

It requires explicit policy for:

- duplicated requests;
- stale responses;
- cost;
- privacy;
- model behavior;
- idempotency.

It SHOULD not be added casually.

## 23. Second Rule Set

A second complete Rule Set is the strongest proof of framework generality.

It SHOULD differ meaningfully from the first in areas such as:

- Character schema;
- Dice model;
- progression;
- Preferences;
- operation types;
- narrative assumptions.

## 24. Rule Set Expansion Gate

Before accepting a second Rule Set, Chronicle SHOULD have:

- stable package contracts;
- reusable conformance tests;
- migration strategy;
- package documentation;
- clear legal distribution policy;
- package version retention.

## 25. No Partial Rule Set Catalog

Chronicle SHOULD prefer two complete Rule Sets over ten incomplete packages.

## 26. P2 Exit Criteria

P2 is complete when:

- two provider adapters satisfy the same contracts;
- two complete Rule Set packages run without Core branching;
- historical package versions remain readable;
- package migrations are proven;
- UI handles schema and mechanical variation.

## 27. P3 — Local Intelligence and Offline Depth

P3 reduces dependence on remote services.

## 28. Local Provider Support

Local Narrative Intelligence MAY support:

- offline narration;
- private Campaigns;
- lower recurring cost;
- custom models;
- user-controlled infrastructure.

Ollama is the first real local provider target.

The initial implementation MUST NOT automatically download models or start/manage Ollama processes.

Missing Ollama installation or missing local model is a normal unavailable-provider state, not a startup failure.

## 29. Local Provider Risks

Local support introduces:

- hardware variability;
- model installation;
- process management;
- memory pressure;
- model compatibility;
- slower generation;
- packaging complexity;
- security of local endpoints.

## 30. Local Model Manager

A future local-model manager MAY:

- detect hardware;
- install supported models;
- launch processes;
- report health;
- enforce resource limits;
- map capabilities to profiles.

It requires a separate RFC.

## 31. Offline Rule Knowledge

P3 MAY strengthen:

- fully local indexing;
- local embeddings;
- local source ingestion;
- source privacy;
- offline citations;
- index portability.

## 32. Graceful Hybrid Mode

Chronicle MAY allow:

- local Narrator;
- remote Campaign Generator;
- local Rule Knowledge;
- remote repair;
- or other capability-specific combinations.

Each data flow remains explicit.

## 33. P3 Exit Criteria

P3 is complete when:

- one supported local provider can run the core Session loop;
- resource limits are visible;
- local failures recover safely;
- remote credentials are not required for local mode;
- Campaign continuity remains equivalent.

## 34. P4 — Extensibility and Creator Tooling

P4 opens Chronicle to broader creators.

It MUST not begin with arbitrary executable plugins.

## 35. Creator Tooling Priorities

Potential tools:

- Character schema editor;
- Rule Set package validator;
- mechanical fixture runner;
- Preference catalog editor;
- Rule Knowledge source builder;
- package manifest generator;
- migration tester;
- Campaign template editor.

## 36. Package Author SDK

A future SDK SHOULD provide:

- contracts;
- sample packages;
- conformance tests;
- documentation;
- fixture tools;
- versioning guidance;
- security restrictions.

## 37. Declarative Before Executable

Chronicle SHOULD prefer declarative extension formats where practical.

Executable extension increases:

- security risk;
- compatibility risk;
- debugging cost;
- signing requirements;
- sandbox complexity.

## 38. Dynamic Package Loading

Dynamic package loading requires:

- package signature;
- manifest;
- permission declaration;
- compatibility checks;
- isolation;
- revocation;
- resource limits;
- clear trust UX.

## 39. Plugin Sandboxing

Untrusted plugins MUST NOT run with unrestricted application permissions.

A sandboxed plugin host requires a dedicated architecture phase.

## 40. Community Package Registry

A package registry is outside early P4.

Before a registry, Chronicle needs:

- package identity;
- signing;
- review policy;
- malware response;
- version retention;
- takedown process;
- licensing metadata;
- compatibility reporting.

## 41. User-Defined House Rule DSL

A bounded House Rule language MAY be considered after built-in package preferences are proven.

It requires:

- deterministic semantics;
- static validation;
- no arbitrary I/O;
- migration;
- replay;
- debugging;
- safety limits.

## 42. P4 Exit Criteria

P4 is complete when a third party can create and validate a package without modifying Chronicle Core and without bypassing security boundaries.

## 43. P5 — Multiplayer Foundations

Multiplayer is a major product and architecture transition.

It MUST NOT be implemented as an extension of single-user local state without explicit redesign.

## 44. Multiplayer Questions

Multiplayer requires answers for:

- user identity;
- authentication;
- authorization;
- Campaign ownership;
- invitation;
- Player Character ownership;
- Director authority;
- visibility;
- Secret distribution;
- Roll authorization;
- concurrent input;
- message order;
- conflict resolution;
- offline participants;
- moderation;
- abuse prevention.

## 45. Multiplayer Authority

A multiplayer system needs one authoritative Campaign host.

Potential models:

```text
Host-Owned Local Server
Dedicated Remote Server
Peer-to-Peer With Authority Election
Managed Cloud Service
```

Each requires a separate RFC.

## 46. Multiplayer Event Ordering

Concurrent player actions require:

- ordering;
- conflict policy;
- Scene turn model or freeform policy;
- authoritative acceptance;
- replay prevention;
- synchronized results.

## 47. Multiplayer Dice

Multiplayer Dice requires:

- who may request;
- who may execute;
- who sees values;
- verification;
- hidden Rolls;
- latency handling;
- retry behavior.

## 48. Multiplayer Secrets

Secrets require per-user and per-Character visibility.

Player-facing projections must become identity-aware.

## 49. Multiplayer MVP

The first multiplayer milestone SHOULD be a narrow vertical slice, such as:

```text
One Host
Two Players
One Campaign
Text Interaction
Shared Scene
Chronicle-Owned Dice
No Spectators
No Offline Merge
```

## 50. P5 Exit Criteria

P5 is complete only when:

- authority is explicit;
- concurrent actions are deterministic;
- per-user visibility is enforced;
- reconnect is safe;
- Dice remain trustworthy;
- Campaign history remains consistent.

## 51. P6 — Multi-Device and Cloud Continuity

Cloud sync is not merely file replication.

It affects:

- identity;
- conflict resolution;
- encryption;
- package availability;
- offline edits;
- operation idempotency;
- device trust;
- backup;
- privacy.

## 52. Cloud Account Model

P6 requires user identity and account lifecycle.

The architecture MUST distinguish:

- local installation;
- user account;
- device;
- Campaign owner;
- collaborator.

## 53. Sync Model

Potential sync models:

```text
Authoritative Cloud Store
Local-First Replication
Explicit Upload and Download
Hosted Campaign Service
```

The choice requires a dedicated RFC.

## 54. Sync Conflict

Chronicle MUST NOT use last-write-wins for all Domain state without analysis.

Conflicts may involve:

- Message order;
- Roll execution;
- progression spend;
- finalization;
- Preference change;
- Narrative Plan revision.

## 55. Encryption

Cloud continuity requires explicit:

- transport encryption;
- at-rest encryption;
- key ownership;
- backup policy;
- credential separation;
- access revocation.

## 56. Multi-Device Clients

Potential clients:

- desktop;
- tablet;
- mobile;
- web;
- TV display.

Each client should consume stable Application contracts rather than duplicate Domain logic.

## 57. P6 Exit Criteria

P6 is complete when one Campaign can move or synchronize across supported devices without losing history, duplicating effects, or weakening visibility.

## 58. P7 — Ecosystem and Advanced Experiences

P7 includes mature ecosystem capabilities after Core, extensibility, and synchronization are proven.

## 59. Potential P7 Capabilities

Potential capabilities include:

- package marketplace;
- Campaign template sharing;
- public Campaign archives;
- streaming overlays;
- spectator mode;
- voice input;
- voice narration;
- maps;
- handouts;
- media assets;
- collaborative worldbuilding;
- advanced Director tools;
- analytics;
- community evaluations.

## 60. Voice

Voice interaction introduces:

- transcription;
- speaker identity;
- latency;
- accessibility;
- privacy;
- audio retention;
- interruption handling.

It requires separate contracts.

## 61. Streaming and Spectators

Streaming requires:

- broadcast-safe projections;
- Secret suppression;
- delay;
- overlay API;
- audience permissions;
- content policy.

## 62. Maps and Tactical Play

A map or tactical grid should not enter Chronicle merely because RPG applications often have one.

It requires validated demand and Rule Set integration.

## 63. Autonomous World Simulation

Autonomous NPC world simulation is intentionally deferred.

It introduces:

- background mutation;
- unbounded compute;
- causality;
- player agency risks;
- explanation burden;
- rollback complexity.

It requires a dedicated research phase.

## 64. Compatibility Strategy

Post-MVP development MUST preserve explicit compatibility dimensions.

These include:

```text
Application Compatibility
Persistence Compatibility
Rule Set Package Compatibility
Character Schema Compatibility
Provider Contract Compatibility
Portable Export Compatibility
Preference Compatibility
Narrative Contract Compatibility
```

## 65. Compatibility Promise

Before 1.0, breaking changes MAY occur.

They still require:

- documented migration;
- version bump;
- fixtures;
- recovery path;
- release notes.

After 1.0, Stable public contracts SHOULD follow semantic-versioning expectations.

## 66. Persistence Support Window

Chronicle SHOULD declare the oldest storage version supported for direct migration.

Older versions may require intermediate upgrades or import/export paths.

## 67. Package Support Window

Rule Set package versions referenced by active Campaigns SHOULD remain available locally.

A package may be unsupported for new Campaigns while remaining readable for historical ones.

## 68. Deprecation Lifecycle

A stable capability SHOULD move through:

```text
Active
Deprecated
ReadOnly
UnsupportedForNewUse
RemovedAfterMigration
```

## 69. Deprecation Notice

Deprecation SHOULD state:

- reason;
- replacement;
- impact;
- migration path;
- removal target;
- rollback limits.

## 70. Removal Gate

A capability may be removed only when:

- supported data can migrate;
- required historical interpretation remains possible;
- release notes are explicit;
- tests cover the transition;
- no active Campaign is silently invalidated.

## 71. Experimental Feature Policy

Experimental features SHOULD:

- be disabled by default or clearly opted into;
- avoid irreversible data formats;
- use isolated schema versions;
- provide exit or export path;
- never bypass security gates.

## 72. Feature Flag Retirement

Every feature flag MUST have:

- owner;
- expected lifetime;
- removal criteria;
- stable or abandoned outcome.

Permanent unknown flags are prohibited.

## 73. Architecture Review Gate

A future capability requires architecture review when it introduces:

- new trust boundary;
- new persistence owner;
- new network protocol;
- new user identity;
- new executable extension;
- new random authority;
- new synchronization model;
- new Secret visibility dimension;
- new portable format;
- irreversible migration.

## 74. RFC Requirement

A new RFC is required for major capabilities such as:

- multiplayer;
- cloud sync;
- dynamic plugins;
- local model management;
- marketplace;
- voice;
- mobile;
- web host;
- remote Campaign server;
- House Rule DSL.

## 75. ADR Requirement

An ADR is appropriate for technology choices within an accepted capability.

Examples:

- database engine;
- desktop framework;
- IPC technology;
- archive format;
- signature algorithm;
- local model runtime.

## 76. Product Review Gate

A capability should answer:

- What user problem does it solve?
- Why now?
- What evidence exists?
- What becomes more complex?
- What is removed or delayed?
- How is success measured?
- How is failure reversed?

## 77. Security Review Gate

A capability must undergo security review if it:

- sends new data externally;
- accepts new file types;
- loads executable code;
- introduces accounts;
- changes Secret visibility;
- changes credential behavior;
- exposes APIs;
- enables sharing.

## 78. Quality Gate

Every roadmap phase inherits RFC-0040 quality gates.

New capabilities must add:

- contract tests;
- migration tests;
- security tests;
- recovery tests;
- compatibility fixtures.

## 79. Documentation Gate

Stable capabilities require:

- user documentation;
- package-author documentation where relevant;
- migration notes;
- limitations;
- troubleshooting;
- privacy behavior.

## 80. Governance

Chronicle SHOULD use transparent open-source governance.

Potential governance artifacts:

- public roadmap;
- RFC process;
- ADR log;
- issue labels;
- release policy;
- security policy;
- package contribution guide;
- code of conduct.

## 81. Maintainer Authority

Maintainers protect:

- architectural boundaries;
- compatibility;
- scope;
- security;
- release quality.

Popularity alone does not justify violating those constraints.

## 82. Contributor Proposals

A contributor proposal SHOULD include:

- problem;
- user value;
- scope;
- architecture impact;
- migration impact;
- test strategy;
- legal considerations;
- alternatives.

## 83. Community Rule Sets

Community Rule Sets SHOULD not be declared compatible solely because they compile.

Compatibility requires passing conformance tests and package validation.

## 84. Official Versus Community Packages

Chronicle SHOULD distinguish:

```text
Official
Verified
Community
LocalDevelopment
Unknown
```

Exact verification policy is deferred.

## 85. Ecosystem Trust

Trust labels MUST be explainable.

Chronicle MUST not imply security review that did not occur.

## 86. Sustainability

Roadmap decisions SHOULD consider:

- maintenance burden;
- test cost;
- package retention;
- platform support;
- provider churn;
- legal review;
- contributor capacity;
- documentation burden.

## 87. Platform Support

Chronicle SHOULD add operating systems deliberately.

A platform enters Stable support only when:

- packaging works;
- persistence is tested;
- credential storage works;
- backup and restore work;
- UI accessibility is acceptable;
- release artifacts are supportable.

## 88. Provider Churn

Provider APIs and models change frequently.

Chronicle SHOULD isolate this through:

- adapters;
- capability profiles;
- contract tests;
- versioned configuration;
- fallback documentation.

Core contracts SHOULD not chase provider-specific features without strong evidence.

## 89. Model Evolution

A new model should be evaluated for:

- structured reliability;
- context behavior;
- latency;
- cost;
- privacy;
- refusal behavior;
- narrative quality;
- compatibility.

Newer does not automatically mean preferred.

## 90. Data Ownership

Future services must preserve:

- local export;
- backup;
- Campaign portability;
- ability to leave a provider;
- ability to leave a hosting service where practical.

## 91. No Lock-In Principle

Chronicle SHOULD avoid making Campaign continuation depend permanently on:

- one provider;
- one cloud service;
- one device;
- one package registry;
- one proprietary export format.

## 92. Research Track

Chronicle MAY maintain a noncommitted research track for:

- long-context memory;
- semantic compression;
- agent planning;
- multi-agent narration;
- verifiable randomness;
- distributed synchronization;
- autonomous world simulation;
- voice.

Research does not enter production contracts automatically.

## 93. Post-MVP Success Metrics

Useful success signals MAY include:

- Campaigns continued across multiple Sessions;
- low recovery failure rate;
- meaningful Memory reuse;
- low duplicate-effect incidents;
- successful backup restore;
- low provider-contract failure rate;
- successful second Rule Set implementation;
- contributor package success;
- accessibility satisfaction;
- user trust in Dice and progression.

## 94. Roadmap Failure Signals

A roadmap phase should be reconsidered when:

- infrastructure work produces no user-visible validation;
- maintenance burden exceeds project capacity;
- migrations become fragile;
- provider costs become prohibitive;
- community packages bypass safety;
- cloud features weaken portability;
- multiplayer compromises narrative clarity;
- scope grows faster than test coverage.

## 95. Roadmap Review Cadence

The roadmap SHOULD be reviewed:

- after MVP acceptance;
- after every major stable release;
- after significant security incidents;
- after provider or platform disruption;
- when core assumptions are disproven.

## 96. Current Post-MVP Priority Order

The current recommended order is:

1. stabilize MVP;
2. improve continuity and usability;
3. complete provider-neutral contracts and deterministic development provider;
4. prove Ollama as the first real local provider;
5. keep OpenAI as an optional remote provider and prove it through opt-in live tests;
6. add provider selection and configuration after adapters exist;
7. prove a second Rule Set;
8. strengthen local/offline intelligence beyond the initial Ollama adapter;
9. build creator tooling;
10. introduce sandboxed extensibility;
11. design multiplayer foundations;
12. design cloud and multi-device continuity;
13. expand ecosystem experiences.

This order may change only through explicit review.

## 97. Explicit Noncommitments

This RFC does not commit Chronicle to:

- cloud hosting;
- subscriptions;
- marketplace monetization;
- mobile applications;
- multiplayer;
- local model distribution;
- voice;
- maps;
- streaming;
- AI-generated images;
- autonomous NPC simulation.

These remain possibilities, not promises.

## 98. Open Questions

The following remain open:

- What evidence threshold promotes a Candidate to Committed?
- Which post-MVP improvements belong in the first stabilization release?
- Which second Rule Set best tests architectural variation?
- Which second provider best tests adapter neutrality?
- Is local-provider support more valuable before provider expansion?
- When should package author tooling become public?
- What trust label should community packages receive?
- Which contracts become Stable at 1.0?
- What migration support window is sustainable?
- Should cloud continuity ever be an official service or only a protocol?
- What multiplayer authority model best fits Chronicle?
- Which clients should follow desktop?
- How should roadmap voting interact with maintainer responsibility?
- What sustainability model supports long-term package and platform maintenance?

These questions require evidence from MVP operation, community participation, and technical experiments.

## 99. Compliance Checklist

A roadmap decision complies when:

- it solves a validated problem;
- current capabilities are stable enough to support it;
- scope is explicit;
- architecture impact is reviewed;
- security boundaries are reviewed;
- migration and rollback exist;
- quality gates expand with the feature;
- compatibility expectations are documented;
- unsupported futures are not implemented speculatively;
- Campaign continuity remains the highest priority.

## 100. Final Principle

Chronicle should not race toward every possible form it might someday take.

It should earn each new capability by preserving what already works.

First, prove one Chronicle can be lived.

Then prove it can grow without forgetting why it exists.
