---
id: RFC-0024
title: Prompt Construction and Context Budget
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-07-31
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
---

> **"A good prompt is not the Campaign. It is the smallest faithful window through which the current operation can see it."**

# Prompt Construction and Context Budget

## Abstract

This RFC defines Chronicle's prompt construction architecture and context budgeting model.

It establishes how curated Chronicle context becomes a provider-facing request without allowing prompt templates, provider token limits, retrieval payloads, or model-specific formatting to leak into the Domain.

The Prompt Builder is responsible for representation.

The Chronicle Director and capability-specific application workflows are responsible for relevance.

Provider Adapters are responsible for transport and provider-specific translation.

The architecture prioritizes correctness, bounded context, hidden-information safety, reproducibility, observability, and provider replacement.

## 1. Purpose

Narrative Intelligence quality depends heavily on the information supplied to it.

Sending too little context can cause:

- contradiction;
- forgotten Character state;
- incorrect participation;
- invented mechanics;
- loss of continuity;
- weak Player Character relevance.

Sending too much context can cause:

- hidden-information leakage;
- higher cost;
- higher latency;
- lower attention quality;
- provider limit failures;
- repetition;
- prompt injection exposure;
- accidental dependence on full transcripts.

Chronicle therefore requires an explicit prompt construction and budgeting model.

## 2. Scope

This RFC defines:

- Prompt Builder responsibility;
- context selection versus representation;
- prompt document structure;
- instruction hierarchy;
- context sections;
- capability templates;
- token and size budgets;
- selection priorities;
- truncation and compression;
- rule knowledge inclusion;
- Memory inclusion;
- recent Message windows;
- Character snapshots;
- hidden-information filtering;
- prompt injection resistance;
- versioning;
- reproducibility;
- observability;
- caching;
- testing.

This RFC does not define:

- exact final prompt wording;
- exact tokenizer;
- exact provider message roles;
- exact XML or JSON format;
- exact provider model limits;
- concrete retrieval algorithm;
- UI prompt editing;
- provider SDK implementation.

## 3. Core Responsibility Separation

Chronicle separates three responsibilities.

```text
Chronicle Director or Application Workflow
    decides what information is relevant

Prompt Builder
    decides how relevant information is represented

Provider Adapter
    decides how the representation is sent
```

These responsibilities MUST remain distinct.

## 4. Prompt Builder Definition

A `PromptBuilder` is an application or infrastructure service that transforms a provider-neutral capability request into a provider-neutral prompt document.

It SHOULD accept:

- operation profile;
- contract version;
- curated context;
- output contract;
- style guidance;
- visibility constraints;
- context budget;
- locale.

It SHOULD produce:

- ordered prompt sections;
- structured data blocks;
- provider-neutral instructions;
- size estimates;
- omission report;
- prompt template version;
- prompt fingerprint.

## 5. Prompt Builder Prohibitions

The Prompt Builder MUST NOT:

- query repositories for additional Campaign truth;
- decide which Memories are relevant;
- add Characters not selected by the application;
- invent missing facts;
- execute Rule Set mechanics;
- assign visibility;
- persist Campaign state;
- depend on provider conversation state;
- silently discard critical context;
- mutate the capability contract.

## 6. Prompt Document

A `PromptDocument` is the provider-neutral intermediate representation of a Narrative Intelligence request.

Conceptually:

```text
PromptDocument
├── Metadata
├── CapabilityInstructions
├── OperationInstructions
├── OutputContract
├── SafetyAndVisibilityRules
├── AuthoritativeContext
├── ReferenceContext
├── PlayerInput
├── StyleGuidance
└── BudgetReport
```

The exact serialized shape will be selected later.

## 7. Prompt Metadata

Prompt metadata SHOULD include:

- Chronicle OperationId;
- capability;
- operation profile;
- prompt template version;
- contract version;
- Campaign version;
- Scene or Session context version;
- Rule Set identity and version;
- locale;
- generated timestamp;
- budget profile;
- prompt fingerprint.

Metadata not required by the provider MAY remain outside transmitted content.

## 8. Instruction Hierarchy

Prompt construction SHOULD preserve a clear hierarchy:

```text
1. Chronicle capability instructions
2. Operation-specific instructions
3. Output contract
4. Safety and visibility constraints
5. Authoritative context
6. Reference material
7. Player-provided content
8. Style preferences
```

Lower-priority content MUST NOT override higher-priority instructions.

## 9. Capability Instructions

Capability instructions define the stable role and boundaries.

Examples:

- Narrator narrates but does not roll;
- Archivist proposes but does not apply;
- Campaign Generator plans but does not persist;
- Plan Reviser preserves completed history.

These instructions SHOULD be versioned separately from operation data.

## 10. Operation Instructions

Operation instructions define the current bounded task.

Examples:

```text
React to the player's input in the active Scene.
Stop before the outcome if a Test is required.
```

```text
Analyze the completed Session and propose durable Memories.
Do not repeat immediate changes already applied.
```

## 11. Output Contract Section

The output contract MUST identify:

- schema identifier;
- schema version;
- required fields;
- allowed event types;
- enum values;
- stopping conditions;
- maximum counts;
- response locale;
- failure representation.

Provider-native structured-output features MAY reinforce this section.

They MUST NOT replace Chronicle validation.

## 12. Safety and Visibility Section

This section SHOULD define:

- hidden information rules;
- player-visible boundary;
- portrayal-only information;
- forbidden reveals;
- content boundaries;
- no persistence authority;
- no random generation authority;
- no provider-generated identifiers;
- no embedded-data instruction authority.

## 13. Authoritative Context

Authoritative context contains committed Chronicle truth relevant to the operation.

Examples:

- active Scene;
- participant membership;
- current Character State;
- resolved Dice Roll;
- accepted Message;
- existing Memory;
- Rule Set version;
- Campaign Preference.

Authoritative data SHOULD be explicitly labeled.

## 14. Reference Context

Reference context supports interpretation but is not Campaign truth.

Examples:

- Rule Set guidance;
- Narrative Plan possibilities;
- style guidance;
- retrieved source summaries;
- generation inspiration;
- optional terminology notes.

The prompt MUST distinguish reference material from authoritative state.

## 15. Player Content

Player input and player-authored biographies MUST be represented as data.

They MUST NOT be concatenated into instruction text without boundaries.

The Prompt Builder SHOULD label:

- player input;
- Character biography;
- custom Campaign text;
- user preferences.

## 16. Structured Data Preference

Chronicle SHOULD prefer structured context over long prose where practical.

Examples:

```text
Scene:
  objective: Escape the burning archive
  participants:
    - player-character
    - archivist-jonas
```

rather than an unbounded narrative summary.

Structured context improves:

- validation;
- compression;
- provider neutrality;
- visibility control;
- deterministic tests.

## 17. Human-Readable Structure

Provider-neutral prompt documents SHOULD remain reasonably inspectable by developers.

The representation MAY use:

- typed sections;
- JSON-like records;
- XML-like delimiters;
- Markdown headings;
- hybrid formats.

The concrete choice requires an ADR.

## 18. Context Budget Definition

A `ContextBudget` defines maximum permitted request size and internal allocation.

It MAY specify:

- maximum estimated tokens;
- maximum bytes;
- maximum Characters;
- maximum Memories;
- maximum Messages;
- maximum rule knowledge items;
- maximum Plan items;
- maximum output reservation;
- required safety margin.

## 19. Budget Profiles

Chronicle SHOULD define stable budget profiles.

Initial profiles MAY include:

```text
InteractiveNarration
PostRollContinuation
SceneOpening
CampaignGeneration
SessionFinalization
PlanRevision
StructuredRepair
```

Infrastructure maps profiles to concrete provider limits.

## 20. Input and Output Reservation

The total model limit SHOULD be conceptually divided into:

```text
Provider Limit
    -
Reserved Output
    -
Safety Margin
    =
Maximum Input Budget
```

Chronicle MUST reserve enough output space for the required contract.

## 21. Safety Margin

A safety margin protects against:

- tokenizer estimation error;
- provider-added framing;
- schema overhead;
- Unicode variation;
- adapter formatting;
- model-specific counting differences.

The exact margin is infrastructure-specific.

## 22. Provider-Neutral Estimates

The Prompt Builder SHOULD produce a provider-neutral size estimate.

The Provider Adapter MAY refine the estimate using a provider tokenizer.

The Application MUST remain able to budget without depending on one tokenizer implementation.

## 23. Hard and Soft Limits

A budget MAY define:

```text
SoftLimit
HardLimit
```

### Soft Limit

Triggers compression or lower-priority omission.

### Hard Limit

The request MUST NOT exceed it.

## 24. Context Priority Classes

Selected context SHOULD be classified as:

```text
Required
High
Normal
Optional
```

### Required

Cannot be omitted safely.

### High

Strongly affects correctness.

### Normal

Improves quality and continuity.

### Optional

May be dropped first.

## 25. Required Context

Required context MAY include:

- operation identity;
- contract version;
- active hierarchy;
- Player Character;
- active participants;
- unresolved Roll;
- authoritative Roll Result;
- hidden-information constraints;
- required Rule Set operation;
- output schema.

If required context cannot fit, the operation MUST fail explicitly.

## 26. Narrator Budget Priority

For normal Narrator operations, priority SHOULD be:

1. capability and output instructions;
2. active Scene truth;
3. Player Character action and state;
4. participant presence and relevant state;
5. unresolved or resolved Roll context;
6. visibility and Secret constraints;
7. relevant Character Knowledge;
8. relevant Relationships;
9. high-value Campaign Memories;
10. relevant Rule Set knowledge;
11. recent Messages;
12. Plan guidance;
13. style detail.

## 27. Archivist Budget Priority

For Session finalization, priority SHOULD be:

1. output contract;
2. executed hierarchy;
3. resolved Rolls;
4. immediate changes;
5. affected Character baselines;
6. accepted Session evidence;
7. existing related Memories;
8. Relationships and Knowledge;
9. Secrets;
10. Rule Set finalization guidance;
11. Plan context;
12. optional style instructions.

## 28. Campaign Generator Budget Priority

For Campaign generation, priority SHOULD be:

1. output contract;
2. Player Character;
3. Rule Set constraints;
4. Campaign Preferences;
5. content boundaries;
6. required existing draft content;
7. generation guidance;
8. setting references;
9. stylistic inspiration.

## 29. Plan Revision Budget Priority

For Plan revision, priority SHOULD be:

1. completed Campaign truth;
2. current Plan version;
3. divergence evidence;
4. active unresolved objectives;
5. relevant Memories;
6. affected Characters and Relationships;
7. Rule Set constraints;
8. future Plan content requiring revision;
9. optional style guidance.

## 30. Context Assembly Report

The application SHOULD provide the Prompt Builder with selection metadata.

The resulting report MAY include:

- selected item count;
- omitted item count;
- priority distribution;
- estimated size by section;
- truncation performed;
- compression performed;
- required items preserved;
- warnings.

## 31. Omission Transparency

The Prompt Builder MUST report omitted context.

It SHOULD identify omission reasons such as:

```text
BudgetExceeded
LowerPriority
Duplicate
Superseded
NotVisible
NotApplicable
```

This supports diagnostics and testing.

## 32. No Silent Critical Omission

Critical context MUST NOT be silently omitted.

If omission would make the operation unsafe, the Prompt Builder MUST fail with an explicit budget error.

## 33. Character Snapshot Budgeting

Character context SHOULD use operation-specific snapshots.

A snapshot MAY include:

- identity;
- relevant state;
- selected sheet fields;
- portrayal guidance;
- local objective;
- selected Relationships;
- selected Knowledge;
- selected Memories;
- visibility constraints.

Unrelated fields SHOULD be excluded.

## 34. Participant Limit

The active Scene participant set is authoritative.

If the participant count is too large for the budget:

- required participants remain;
- low-relevance detail is compressed;
- Character snapshots become smaller;
- the operation may fail if coherent portrayal is impossible.

The Prompt Builder MUST NOT remove a present Character from authoritative participation.

## 35. Campaign Memory Budgeting

Selected Memories SHOULD be represented concisely.

A Memory prompt record MAY include:

- identifier;
- summary;
- scope;
- involved Characters;
- relevance;
- importance;
- status;
- perspective;
- visibility;
- permitted use.

Full Memory descriptions SHOULD be included only when necessary.

## 36. Memory Ordering

Memory order SHOULD reflect operation relevance.

Suggested sort factors:

- required status;
- direct participant involvement;
- active objective relevance;
- active conflict relevance;
- importance;
- current relevance;
- recency;
- permanent status.

The selection algorithm itself is defined elsewhere.

## 37. Memory Deduplication

The Prompt Builder MAY collapse duplicate representations when the same Memory appears through:

- Character snapshot;
- Campaign context;
- Relationship context;
- recent Message summary.

It MUST preserve distinct Character perspectives.

## 38. Recent Message Window

Recent Messages support local continuity.

The window SHOULD be bounded by:

- count;
- estimated tokens;
- Scene;
- operation profile.

Messages from prior Scenes SHOULD normally be represented through summaries or Memories.

## 39. Message Selection

Recent Message selection SHOULD preserve:

- latest player input;
- latest Narrator response;
- unresolved dialogue;
- Roll interruption text;
- direct references needed for continuity.

Older low-value dialogue may be omitted first.

## 40. Message Compression

A Message window MAY be compressed into a Scene-local continuity summary.

The summary MUST:

- remain clearly identified as contextual;
- preserve unresolved commitments;
- preserve participant actions;
- preserve Roll interruption;
- avoid inventing events.

## 41. Rule Knowledge Budgeting

Rule knowledge SHOULD be:

- relevant;
- concise;
- versioned;
- source-referenced;
- licensing-safe;
- separated from capability instructions.

Unrelated rules MUST be omitted.

## 42. Rule Knowledge Precedence

Executable Rule Set logic remains authoritative.

Natural-language rule knowledge is explanatory.

The prompt SHOULD state that retrieved text must not override structured mechanics supplied by Chronicle.

## 43. Narrative Plan Budgeting

Plan guidance SHOULD include only the portion needed for the active operation.

For the Narrator, this MAY include:

- current Scene purpose;
- current Act pressure;
- one or more allowed transition possibilities;
- prohibited reveal;
- revision trigger.

The full future Campaign Plan MUST NOT be included by default.

## 44. Secret Budgeting

Secrets SHOULD be excluded unless the operation requires them.

When included, the prompt MUST state:

- permitted use;
- reveal restriction;
- knowledgeable Characters;
- suspected-by Characters;
- partial reveal boundary.

## 45. Hidden Information Minimization

The safest hidden information is information not sent.

Chronicle SHOULD prefer:

```text
Exclude
```

over:

```text
Include and instruct not to reveal
```

when portrayal does not require the hidden fact.

## 46. Compression Levels

Chronicle MAY define standard representation levels:

```text
Full
Compact
Summary
ReferenceOnly
Omitted
```

The Application or Prompt Builder MAY choose a level according to priority and budget.

## 47. Full Representation

Used for critical active context.

Example:

- Player Character current state;
- authoritative Roll Result;
- active Scene objective.

## 48. Compact Representation

Used when structured detail matters but prose detail does not.

Example:

- NPC portrayal traits;
- relevant Relationship dimensions;
- selected Rule Set operation.

## 49. Summary Representation

Used for broader context.

Example:

- prior Act;
- older Scene;
- long Character history;
- previous Session.

## 50. Reference-Only Representation

Used when identity and linkage are sufficient.

Example:

- related Memory identifier;
- source reference;
- previous Plan version.

## 51. Semantic Compression

Narrative Intelligence MAY assist in generating summaries.

Such summaries MUST be:

- generated before the target operation;
- validated against source evidence where important;
- versioned;
- treated as derived context;
- replaceable by source data.

They MUST NOT silently become authoritative Campaign truth.

## 52. Deterministic Compression

Where possible, Chronicle SHOULD use deterministic compression.

Examples:

- select field subset;
- omit null fields;
- collapse repeated labels;
- use stable keys;
- limit descriptions;
- replace repeated entity details with references.

## 53. Truncation

Arbitrary text truncation SHOULD be avoided.

Truncation MAY be safe only for:

- optional descriptive prose;
- diagnostic content;
- noncritical source excerpts.

Structured records and required instructions MUST not be truncated into invalid forms.

## 54. Truncation Marker

When text is truncated, the Prompt Document SHOULD indicate that truncation occurred.

The provider SHOULD not infer that the truncated text was complete.

## 55. Prompt Template

A `PromptTemplate` defines the stable arrangement and wording for one capability and operation profile.

It SHOULD include:

- template identifier;
- version;
- capability;
- supported contract version;
- section order;
- instruction content;
- required sections;
- optional sections;
- output constraints;
- localization behavior.

## 56. Template Versioning

Prompt templates MUST be versioned.

Historical operation diagnostics SHOULD record the template version used.

A template update MUST NOT require Campaign migration.

## 57. Template Compatibility

A template MUST declare compatible:

- capability contract versions;
- operation profiles;
- provider adapter capabilities;
- output schema versions.

Incompatible combinations MUST fail before provider invocation.

## 58. Localization of Prompts

Capability instructions MAY be authored in one canonical language if the provider performs reliably.

Player-facing output locale remains explicit.

Rule Set terms and player content SHOULD preserve the requested locale.

The implementation decision requires testing.

## 59. Provider-Specific Adaptation

The Provider Adapter MAY transform the Prompt Document into:

- system messages;
- developer messages;
- user messages;
- structured-output schema;
- tool declarations;
- local-model prompt format.

It MUST preserve semantic section boundaries and precedence.

## 60. Provider Role Mapping

Provider message roles are infrastructure details.

Chronicle contracts MUST NOT assume that every provider supports the same role hierarchy.

The adapter is responsible for safe mapping.

## 61. Provider Structured Output

When supported, the adapter SHOULD use provider-native structured output.

The Prompt Document still includes enough contract meaning to support:

- test providers;
- local providers;
- providers without native schemas;
- diagnostics.

## 62. Prompt Fingerprint

A prompt fingerprint SHOULD represent the semantic request sent to Narrative Intelligence.

It MAY include:

- capability;
- profile;
- template version;
- contract version;
- normalized Prompt Document;
- context versions;
- output schema.

It SHOULD exclude credentials and unstable transport metadata.

## 63. Fingerprint Uses

Prompt fingerprints support:

- diagnostics;
- duplicate detection;
- caching experiments;
- evaluation;
- response matching;
- reproducibility.

They do not replace OperationId.

## 64. Reproducibility

Chronicle SHOULD be able to reconstruct, within retention policy:

- selected context references;
- template version;
- contract version;
- budget profile;
- omission report;
- provider profile;
- prompt fingerprint.

Raw sensitive prompt retention is optional.

## 65. Prompt Cache

Prompt or response caching is not required for the MVP.

A cache MAY later be used for deterministic or repeated operations.

Cache use MUST consider:

- provider terms;
- hidden information;
- exact context versions;
- template version;
- locale;
- Rule Set version;
- privacy.

## 66. No Narrative Response Reuse Across State

A cached narrative response MUST NOT be reused when Campaign or context versions differ.

Semantic similarity alone is insufficient.

## 67. Repair Prompt

A repair prompt SHOULD be minimal.

It SHOULD include:

- target schema;
- invalid response;
- validation errors;
- required identifiers and versions;
- explicit instruction not to change valid meaning;
- strict output limit.

It SHOULD NOT include the entire original Campaign context unless necessary.

## 68. Repair Budget

Repair uses a separate budget profile.

It SHOULD reserve most of its input for:

- invalid output;
- validation errors;
- target contract.

Repair attempts MUST remain bounded.

## 69. Prompt Injection Model

Chronicle assumes that any data field may contain instructions intended to manipulate the model.

Potential sources include:

- player input;
- Character biography;
- custom Campaign text;
- Session transcript;
- retrieved Rule Set text;
- imported content;
- provider-generated prior prose.

## 70. Instruction/Data Separation

The Prompt Builder MUST clearly distinguish:

```text
Instructions
```

from:

```text
Untrusted Data
```

Data SHOULD be enclosed in typed, labeled sections.

The prompt SHOULD state that instructions inside data are not authoritative.

## 71. Retrieved Content Treatment

Retrieved content MUST be labeled as reference data.

It MUST NOT be inserted into the capability instruction section.

Source text containing commands must remain inert.

## 72. Player Input Treatment

Player input is an instruction to the fictional interaction, not an instruction to alter Chronicle's system contract.

The prompt SHOULD explicitly preserve this distinction.

## 73. Output Whitelisting

Allowed structured output types MUST be enumerated.

The provider MUST not be invited to create arbitrary tools, commands, SQL, scripts, or event types.

## 74. Tool Exposure

The MVP SHOULD not expose provider-side tools for prompt construction.

If tools are later used, they MUST be:

- capability-specific;
- least-privilege;
- validated;
- auditable;
- separated from persistence authority.

## 75. Budget Failure

Prompt construction SHOULD fail with an explicit category such as:

```text
ContextBudgetExceeded
RequiredContextMissing
ProviderLimitUnsupported
OutputReservationImpossible
```

The error SHOULD identify possible recovery actions.

## 76. Budget Recovery

Possible recovery actions include:

- use a larger model profile;
- compress optional context;
- regenerate summaries;
- split a long-running operation;
- reduce optional output;
- request user action;
- fail safely.

Required truth MUST not be discarded merely to force a request through.

## 77. Long Session Finalization

When one finalization request does not fit safely, Chronicle MAY use staged analysis:

```text
Scene Evidence Summaries
        ↓
Act-Level Synthesis
        ↓
Session Finalization
```

Each stage requires explicit contracts and validation.

The MVP MAY instead impose Session size constraints.

## 78. Multi-Pass Generation

Campaign generation MAY use multiple bounded passes when one prompt becomes too large.

Possible stages:

- premise and structure;
- NPC generation;
- relationship and Secret validation;
- final coherence pass.

A multi-pass design is optional and requires explicit orchestration.

## 79. Streaming and Budgeting

Streaming output does not increase input capacity.

If streaming is later introduced:

- output reservation policy still applies;
- structured event completion remains required;
- partial streamed prose remains provisional;
- budget reporting remains operation-level.

## 80. Observability

Chronicle SHOULD record:

- template identifier and version;
- budget profile;
- estimated input size;
- provider-calculated input size when available;
- reserved output;
- actual output usage;
- section sizes;
- selected item counts;
- omitted item counts;
- compression levels;
- budget failures;
- prompt fingerprint;
- provider profile.

## 81. Content-Safe Logging

Logs SHOULD record section metadata rather than full content.

Examples:

```text
Selected 6 Memories
Included 4 participants
Omitted 11 low-priority Messages
```

Raw prompt logging SHOULD be disabled or redacted by default.

## 82. Evaluation Support

Prompt construction SHOULD support offline evaluation.

An evaluation fixture MAY include:

- curated request;
- expected required context;
- expected omissions;
- Prompt Document;
- scripted provider response;
- validation result.

This allows templates to evolve without relying only on manual play.

## 83. Testing Strategy

### 83.1 Prompt Builder Unit Tests

Test:

- section ordering;
- instruction/data separation;
- required context preservation;
- budget allocation;
- omission reporting;
- deterministic fingerprinting;
- template compatibility.

### 83.2 Budget Tests

Test:

- soft-limit compression;
- hard-limit failure;
- output reservation;
- safety margin;
- provider-specific refinement;
- oversized required context.

### 83.3 Security Tests

Test:

- prompt injection in player input;
- prompt injection in Character biography;
- prompt injection in Rule Set text;
- hidden Secret omission;
- forbidden event request.

### 83.4 Integration Tests

Test Prompt Document translation through at least one Provider Adapter.

## 84. Required Test Cases

Tests MUST cover:

- normal Narrator prompt;
- ContinueAfterRoll prioritization;
- Scene opening context;
- Archivist prompt;
- Campaign Generator prompt;
- Plan revision prompt;
- required context fits exactly;
- soft limit exceeded;
- hard limit exceeded;
- optional Messages omitted;
- high-value Memory retained;
- hidden Secret excluded;
- portrayal-only Secret constrained;
- participant details compressed;
- provider tokenizer reports larger count;
- output reservation impossible;
- prompt fingerprint stability;
- template version change;
- incompatible contract version;
- injection in retrieved rule content;
- raw prompt logging disabled;
- omission report correctness.

## 85. Prohibited Patterns

### 85.1 Prompt Builder Selects Campaign Truth

The Prompt Builder MUST NOT query or choose additional domain data.

### 85.2 Entire Campaign Dump

The full Campaign MUST NOT be sent by default.

### 85.3 Full Transcript Dependency

Narrative continuity MUST NOT depend on resending the complete transcript.

### 85.4 Hidden Data Included Without Need

Secrets MUST NOT be sent merely because the provider might use them.

### 85.5 Silent Critical Truncation

Required context and contracts MUST NOT be silently cut.

### 85.6 Provider Tokenizer in Domain

Provider token-counting types MUST NOT enter Domain code.

### 85.7 Player Data as System Instructions

Player-authored text MUST remain labeled as untrusted data.

### 85.8 Template Without Version

Every production prompt template MUST be versioned.

### 85.9 Raw Prompt Logging by Default

Sensitive prompt content MUST NOT be logged routinely.

### 85.10 Budget as Afterthought

The system MUST NOT construct an oversized prompt and rely on the provider to reject it.

## 86. Current Delivery Decision

The MVP adopts:

- provider-neutral Prompt Document;
- capability and operation templates;
- explicit instruction hierarchy;
- structured context sections;
- explicit context budgets;
- input, output, and safety-margin allocation;
- priority classes;
- operation-specific budget profiles;
- bounded recent Message windows;
- compact Campaign Memory representation;
- relevant Rule Set knowledge only;
- hidden-information minimization;
- prompt template versioning;
- omission reporting;
- prompt fingerprinting;
- provider-specific final size validation;
- no full Campaign dumps;
- no raw prompt logging by default;
- no prompt cache requirement;
- no mandatory multi-pass prompting.

## 87. Architecture Horizon

Future evolution MAY include:

- adaptive budget allocation;
- provider-specific prompt optimizers;
- automatic context-quality scoring;
- learned Memory selection;
- multi-pass Session finalization;
- semantic prompt caching;
- local-model compact templates;
- prompt experimentation framework;
- automated template evaluation;
- user-selectable narrative verbosity;
- cost-aware context reduction;
- multilingual template variants.

The MVP MUST NOT implement these capabilities without a later milestone.

## 88. Open Questions

The following remain open:

- What provider-neutral Prompt Document format will be used?
- Which tokenizer strategy will the MVP use?
- What budget values apply to each operation profile?
- How large should the recent Message window be?
- Which Character fields belong in each snapshot profile?
- Should importance and relevance scores be exposed to providers?
- How should omission reports be persisted?
- Which prompt metadata should be transmitted versus retained locally?
- Should Prompt Builder belong to Application or Infrastructure?
- Will capability instructions be localized?
- How should long Sessions be constrained or staged?
- Should compressed summaries be created synchronously?
- Which provider-native structured-output features will be required?
- How should prompt template quality be evaluated?
- Should the official application expose advanced context diagnostics?

These questions require RFC-0025, infrastructure RFCs, privacy RFCs, and technology ADRs.

## 89. Compliance Checklist

An implementation complies when:

- relevance selection remains outside Prompt Builder;
- Prompt Builder only represents curated context;
- Provider Adapter owns provider-specific transport;
- instructions and data are separated;
- output contract is explicit;
- visibility constraints are explicit;
- context budgets are defined before invocation;
- required context is never silently omitted;
- optional context is reduced by priority;
- recent Messages are bounded;
- Campaign Memories are selected and compact;
- Rule Set knowledge is relevant and minimal;
- templates are versioned;
- omission is observable;
- prompt fingerprints are stable;
- prompt injection sources remain untrusted;
- raw sensitive logging is disabled by default.

## 90. Final Principle

A prompt should contain everything the capability must know.

It should contain nothing merely because Chronicle happens to remember it.
