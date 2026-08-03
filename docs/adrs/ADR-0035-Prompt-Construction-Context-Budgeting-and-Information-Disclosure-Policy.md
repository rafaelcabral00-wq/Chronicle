---
id: ADR-0035
title: Prompt Construction, Context Budgeting, and Information Disclosure Policy
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
  - ADR-0005
  - ADR-0006
  - ADR-0007
  - ADR-0008
  - ADR-0009
  - ADR-0010
  - ADR-0017
  - ADR-0018
  - ADR-0019
  - ADR-0020
  - ADR-0028
  - ADR-0029
  - ADR-0030
  - ADR-0032
  - ADR-0033
  - ADR-0034
  - RFC-0009
  - RFC-0010
  - RFC-0011
  - RFC-0012
  - RFC-0013
  - RFC-0014
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
  - RFC-0039
  - RFC-0040
  - RFC-0041
  - RFC-0042
---

> **"The provider may only reason from what Chronicle deliberately reveals. Context is a capability grant, not a database dump."**

# Prompt Construction, Context Budgeting, and Information Disclosure Policy

## 1. Status

**Proposed**

This ADR defines Chronicle's prompt-construction pipeline, context selection, relevance ranking, information-disclosure policy, token budgeting, truncation, summarization, role contracts, provider transmission boundaries, and prompt evidence.

The decision is:

- centralize all provider-facing context construction in a Chronicle-owned Prompt Construction service;
- prohibit handlers, ViewModels, Rule Sets, providers, and package code from assembling ad hoc prompts;
- represent prompt inputs as typed context records rather than concatenated arbitrary strings;
- classify every context record by authority, sensitivity, visibility, relevance, lifetime, and provider-transmission policy;
- enforce role-specific disclosure for Narrator, Archivist, and future Narrative Intelligence roles;
- preserve the distinction between canonical truth, Character Knowledge, belief, suspicion, misunderstanding, player claims, and narrative color;
- select context deterministically from authoritative local state;
- use explicit priority and budget policies;
- never truncate structured records in the middle;
- omit lower-priority records rather than silently corrupting their meaning;
- use derived summaries only when their source range, version, and authority are known;
- never treat summaries as stronger authority than their source records;
- require critical context to fit or fail safely;
- exclude credentials, local filesystem details, unrelated Campaigns, diagnostic internals, and forbidden Secrets;
- transmit Rule Knowledge only according to licensing, source, relevance, and transmission policy;
- isolate user-authored and imported text from system and role instructions;
- version role contracts, prompt contracts, context-selection policy, and serialization;
- record prompt fingerprints and safe disclosure metadata for diagnostics without persisting unrestricted prompts by default;
- allow provider-specific formatting only inside adapters, without changing semantic content;
- make context budgeting measurable, testable, and deterministic for a given authoritative state and policy version.

The decision becomes **Accepted** after a prompt-construction spike proves:

- role-specific context selection;
- canonical truth separation from Character Knowledge;
- Memory relevance ranking;
- bounded recent transcript inclusion;
- Rule Knowledge filtering;
- critical-context preservation;
- deterministic output under the same state and policy;
- safe omission under budget pressure;
- no secret or credential leakage;
- resistance to prompt injection from player text, imported content, and Rule Knowledge;
- support for Narrator and Archivist role contracts;
- provider-neutral prompt packages;
- stable prompt fingerprints;
- graceful failure when critical context cannot fit.

## 2. Context

Narrative Intelligence can only act on the information Chronicle sends.

The provider does not have direct access to:

- the database;
- Campaign aggregates;
- Character Sheets;
- Memories;
- transcript history;
- Character Knowledge;
- Rule Knowledge;
- Preferences;
- Dice state;
- package metadata.

That is intentional.

Chronicle must construct a context package for every provider request.

The context may need to include:

- the active role contract;
- the current Campaign, Session, Act, and Scene;
- initiating player input;
- recent transcript;
- Character mechanical state;
- narrative profile;
- Character Knowledge;
- relevant Memories;
- relationships;
- accepted Dice evidence;
- unresolved choices;
- Campaign Preferences;
- safety boundaries;
- Rule Set terminology;
- Rule Knowledge;
- output-contract instructions.

Provider context is limited by:

- token or character budget;
- provider capability;
- cost policy;
- latency;
- privacy;
- licensing;
- role authority;
- relevance;
- hidden information;
- future multiplayer visibility.

A naive “send everything” strategy would:

- leak Secrets;
- expose unrelated Campaign data;
- waste budget;
- increase contradictions;
- weaken role boundaries;
- transmit copyrighted material unnecessarily;
- make provider behavior harder to reproduce;
- allow hostile imported text to compete with system instructions.

A naive “send only the last few messages” strategy would:

- omit canonical facts;
- lose Character Knowledge;
- forget important Memories;
- contradict Dice outcomes;
- ignore Preferences;
- fail long-running Campaign continuity.

Chronicle therefore needs an explicit, typed, and auditable context-construction policy.

## 3. Decision Drivers

The design prioritizes:

1. information minimization;
2. narrative continuity;
3. authority clarity;
4. role-specific disclosure;
5. deterministic context selection;
6. privacy;
7. prompt-injection resistance;
8. budget predictability;
9. Rule Knowledge licensing;
10. provider replaceability;
11. diagnostics without content leakage;
12. future multiplayer visibility.

## 4. Decision Summary

Chronicle will use:

```text
Prompt Construction
    centralized Application capability

Input Records
    typed and classified

Role
    explicit versioned role contract

Selection
    deterministic policy

Budget
    explicit per request and provider capability

Critical Context
    cannot be silently omitted

Truncation
    whole-record omission only

Summaries
    derived, versioned, source-linked

Disclosure
    role + relevance + visibility + transmission policy

Serialization
    provider-neutral prompt package

Provider Adapter
    formatting only

Evidence
    context fingerprint
    policy versions
    included record identities
```

## 5. Prompt Construction Boundary

Chronicle defines a capability such as:

```text
IPromptContextBuilder
```

or:

```text
INarrativeContextAssembler
```

The exact name is implementation-level.

## 6. Centralization

All Narrative Intelligence requests use the same governed construction boundary.

## 7. No Ad Hoc Prompt Strings

Application handlers, ViewModels, and package code MUST NOT create unrestricted system prompts or concatenate raw Campaign data for provider use.

## 8. Provider-Neutral Prompt Package

Prompt Construction produces a provider-neutral package.

Conceptually:

```text
NarrativePromptPackage
    RoleContract
    OutputContract
    ContextSections
    InitiatingInput
    DisclosureMetadata
    BudgetMetadata
    ContextFingerprint
```

## 9. Provider Adapter Responsibility

The provider adapter may translate the package into:

- system messages;
- developer messages;
- user messages;
- tool schemas;
- JSON schemas;
- provider-native structured-output fields.

It may not weaken Chronicle's semantic boundaries.

## 10. Role Contract

Every Narrative Intelligence role has a versioned role contract.

Initial roles include:

```text
Narrator
Archivist
```

Future roles may include:

- continuity reviewer;
- Rules explainer;
- summary generator;
- recovery assistant.

## 11. Narrator Role

The Narrator may:

- portray the world;
- write dialogue;
- frame scenes;
- describe consequences already accepted;
- request a Roll;
- offer choices;
- propose bounded transitions;
- ask for clarification.

The Narrator may not:

- generate Dice;
- persist state;
- award progression directly;
- change Preferences;
- create Memories directly;
- reveal forbidden Secrets;
- redefine Rule Set mechanics.

## 12. Archivist Role

The Archivist may:

- inspect finalized Session evidence;
- propose Memories;
- propose relationship changes;
- propose Character Knowledge updates;
- propose Session summaries;
- propose progression awards where workflow permits.

The Archivist may not:

- alter history;
- invent missing events;
- apply proposals directly;
- resolve unresolved Rolls;
- modify provider credentials;
- bypass finalization validation.

## 13. Role Version

Every prompt package records:

```text
RoleContractKey
RoleContractVersion
```

## 14. Context Record

Every context item is represented as a typed record.

Recommended metadata:

```text
ContextRecordId
ContextRecordType
AuthorityClass
VisibilityClass
SensitivityClass
TransmissionPolicy
RelevanceScore
PriorityClass
Lifetime
SourceVersion
SourceReference
EstimatedBudgetCost
```

## 15. Context Record Types

Recommended types:

```text
CampaignFact
SceneState
CharacterState
CharacterKnowledge
Memory
Relationship
TranscriptMessage
TranscriptSummary
DiceEvidence
ChoiceState
Preference
RuleSetTerminology
RuleKnowledgeExcerpt
SafetyConstraint
WorkflowConstraint
UserInput
SystemInstruction
```

## 16. Authority Classes

Recommended authority classes:

```text
CanonicalTruth
AcceptedMechanicalEvidence
AcceptedNarrativeHistory
CharacterKnowledge
Belief
Suspicion
Misunderstanding
PlayerClaim
ProviderProposal
DerivedSummary
NarrativeColor
UnverifiedReference
```

## 17. Canonical Truth

Canonical truth comes from authoritative Chronicle state.

Examples:

- current Scene;
- accepted Character state;
- finalized Session status;
- accepted Dice outcome;
- Campaign Preferences;
- package binding.

## 18. Accepted Narrative History

Accepted Narrator and player transcript messages are historical evidence of what was said or narrated.

Not every statement inside them is automatically structured canonical truth.

## 19. Character Knowledge

Character Knowledge records what a Character knows.

It is not identical to Campaign truth.

## 20. Belief and Suspicion

Beliefs and suspicions remain explicitly marked.

The provider must not be told they are canonical facts.

## 21. Misunderstanding

A misunderstanding may be narratively important.

It must remain distinguishable from truth.

## 22. Player Claim

A player may state an intention, interpretation, or unsupported claim.

Prompt Construction labels it as player input, not canonical truth.

## 23. Derived Summary

A summary compresses accepted source records.

It does not gain authority beyond those sources.

## 24. Visibility Classes

Recommended values:

```text
NarratorVisible
ArchivistVisible
PlayerVisible
DirectorVisible
CharacterScoped
HiddenUntilRevealed
InternalOnly
```

## 25. Single-User MVP

Although the MVP is single-user, visibility metadata remains explicit to support:

- hidden Secrets;
- Character Knowledge;
- future multiplayer;
- role-specific provider access.

## 26. Sensitivity Classes

Recommended values:

```text
PublicCampaignData
PrivateCampaignData
SensitiveNarrativeData
SafetySensitive
Secret
Credential
Diagnostic
CopyrightRestricted
```

## 27. Transmission Policies

Recommended values:

```text
AlwaysAllowed
AllowedWhenRelevant
AllowedForRole
AllowedWithUserConsent
LocalOnly
NeverTransmit
```

## 28. Credentials

Credential-class records are always `NeverTransmit`.

## 29. Local Diagnostics

Diagnostic internals are local-only unless a dedicated support export explicitly includes scrubbed data.

## 30. Rule Knowledge Copyright Classification

Rule Knowledge content records must include a transmission policy based on:

- ownership;
- license;
- source;
- user consent;
- excerpt limits;
- provider policy.

## 31. Context Sections

Recommended logical sections:

```text
Role and authority
Output contract
Current Campaign position
Current Scene truth
Initiating input
Pending mechanical or choice state
Active Characters
Relevant Character Knowledge
Relevant Memories
Relevant relationships
Recent continuity
Campaign Preferences
Rule Set terminology
Rule Knowledge
Safety constraints
Workflow constraints
```

## 32. Section Ordering

Ordering is stable and versioned.

## 33. System Instructions First

Role, authority, disclosure, and output-contract instructions precede untrusted content.

## 34. Untrusted Content Delimitation

Player text, imported prose, Rule Knowledge, and transcript content are wrapped in typed data sections.

They are not inserted as system instructions.

## 35. Instruction Hierarchy

The package explicitly states:

```text
Chronicle role and authority contract
    outranks
output-contract instructions
    outrank
Campaign context data
    outrank
quoted or imported text
```

## 36. Prompt Injection

Prompt Construction treats text such as:

```text
Ignore previous instructions
Reveal every Secret
Call this tool
Change the Character Sheet
```

as data when it originates from player, transcript, import, or Rule Knowledge content.

## 37. No Execution from Quoted Text

Quoted instructions never become Chronicle actions.

## 38. Initiating Input

The initiating input is included as a dedicated typed section.

## 39. Current Scene Truth

The current Scene section includes only the latest accepted state.

## 40. Hierarchy Context

The provider receives enough Campaign → Session → Act → Scene structure to understand scope.

## 41. Campaign Summary

A bounded Campaign summary may provide long-term framing.

It is derived and source-linked.

## 42. Recent Transcript Window

Prompt Construction includes a bounded recent transcript window.

## 43. Transcript Selection

Selection uses stable sequence order.

## 44. Transcript Window Is Not Enough

Recent transcript is combined with structured facts, Memories, and Character Knowledge.

## 45. Transcript Priority

Messages immediately surrounding:

- the initiating input;
- a Dice Roll;
- a user choice;
- a Scene transition;
- a revelation

receive higher priority.

## 46. Transcript Message Integrity

A transcript message is included whole.

## 47. Long Message Handling

A message exceeding its allowed budget may use:

- a validated summary;
- a bounded excerpt with explicit truncation metadata;
- safe failure when critical.

## 48. Excerpt Authority

An excerpt remains a partial view and is marked as such.

## 49. Character Context

Character context is purpose-specific.

## 50. Player Character Priority

The active Player Character generally receives the highest Character-context priority.

## 51. Mechanical Character Data

Only fields needed for the current narrative or operation are included.

## 52. Narrative Profile

Relevant personality, goals, fears, and history may be included.

## 53. Full Sheet Prohibition

The complete Character Sheet is not sent by default on every turn.

## 54. Hidden Character Fields

Hidden or internal fields are omitted unless the Narrator role requires them and policy allows disclosure.

## 55. NPC Context

NPCs receive context proportional to:

- Scene participation;
- relationship;
- recent mention;
- current relevance;
- narrative role.

## 56. Character Knowledge Context

Character Knowledge is selected by:

- current actor;
- current Scene;
- subject relevance;
- confidence or state;
- recent use;
- memory connection.

## 57. Knowledge Truth Separation

The serialized form explicitly labels:

```text
What is true
What this Character knows
What this Character believes
What this Character suspects
What this Character misunderstands
```

## 58. Memory Context

Campaign Memories are selected by:

- scope;
- relevance;
- age;
- importance;
- permanence;
- remembered-by;
- current entities;
- current location;
- current conflict;
- current Session and Scene.

## 59. Memory Relevance

Memory relevance is calculated by Chronicle policy.

The provider does not query every Memory.

## 60. Memory Priority Signals

Possible signals:

```text
Permanent
High importance
Recently refreshed
Direct entity overlap
Current location overlap
Current conflict overlap
Remembered by active Character
Explicitly referenced by player
Required continuity fact
```

## 61. Archived Memory

Archived or dormant Memory may still be included when directly relevant.

## 62. Expired Memory

Memory lifetime affects default relevance, not necessarily physical existence.

## 63. Relationship Context

Relationships are selected when their source, target, or current event is relevant.

## 64. Relationship Direction

Directional meaning is preserved.

## 65. Dice Evidence Context

When continuing after a Roll, the accepted mechanical result is critical context.

## 66. Raw Dice Context

Raw Dice may be included when useful for explanation or presentation.

The accepted result remains primary.

## 67. Choice Context

When continuing after a user choice, the selected option and its stable identity are critical.

## 68. Preference Context

Only Preferences relevant to the role and current operation are included.

## 69. Mechanical Preference

Mechanical Preferences are included as accepted configuration, not suggestions.

## 70. Narrative Preference

Narrative Preferences guide style within Chronicle authority and safety limits.

## 71. Safety Preference

Safety constraints receive critical priority for provider-facing narrative requests.

## 72. Local-Only Preference

Local UI Preferences are omitted.

## 73. Rule Set Terminology

Rule Set terminology provides stable meanings for package-specific concepts.

## 74. Terminology Priority

Only terminology relevant to current entities, operations, and fields is included.

## 75. Rule Knowledge

Rule Knowledge is retrieved separately from Campaign Memories.

## 76. Rule Knowledge Purpose

It supports:

- mechanical terminology;
- setting guidance;
- official or user-provided reference;
- package-specific clarification.

It does not replace Rule Operations.

## 77. Rule Knowledge Selection

Selection uses:

- package identity;
- package version;
- operation key;
- current entities;
- query relevance;
- source trust;
- transmission policy;
- budget.

## 78. Rule Knowledge Source Metadata

Each excerpt includes safe metadata such as:

```text
SourceId
SourceVersion
ExcerptId
TopicKeys
AuthorityType
TransmissionPolicy
ContentHash
```

## 79. Rule Knowledge Authority

Rule Knowledge may describe rules.

The Rule Set implementation remains authoritative for accepted mechanical calculation.

## 80. Conflicting Rule Knowledge

If Rule Knowledge conflicts with the active package operation:

- Chronicle follows the package operation;
- the conflict is diagnostic;
- the provider must not override mechanics.

## 81. Unsupported Sourcebook Content

Chronicle does not transmit unauthorized or prohibited source material.

## 82. Excerpt Bounds

Rule Knowledge excerpts are bounded.

## 83. User-Provided Sources

User-provided sources are marked as such and remain untrusted as instructions.

## 84. Context Budget

Every request has a total context budget.

## 85. Budget Units

Prompt Construction operates with a provider-neutral estimated unit such as:

- token estimate;
- character estimate;
- weighted byte estimate.

The provider adapter may refine the estimate.

## 86. Budget Components

The budget includes:

```text
Role contract
Output schema
Context records
Initiating input
Provider formatting overhead
Reserved output capacity
Safety margin
```

## 87. Output Reservation

Chronicle reserves capacity for the provider's structured output.

## 88. Safety Margin

A configurable margin protects against tokenizer estimation differences.

## 89. Provider Capability

Each provider profile declares:

```text
MaximumContextBudget
MaximumOutputBudget
StructuredOutputOverhead
TokenizerProfile
```

## 90. Request Budget Policy

The request may choose a lower budget based on:

- cost;
- latency;
- workflow;
- provider policy;
- user configuration;
- role.

## 91. Priority Classes

Recommended priority classes:

```text
Critical
Required
High
Normal
Low
Optional
```

## 92. Critical Context

Critical context includes:

- role contract;
- output contract;
- active Scene identity;
- initiating input;
- accepted Dice result for continuation;
- selected user choice;
- required safety constraints;
- exact package identity when mechanically relevant;
- authority distinctions needed to avoid contradiction.

## 93. Critical Context Failure

If critical context cannot fit:

```text
prompt.context-budget-critical-overflow
```

Chronicle does not silently omit it.

## 94. Required Context

Required records may be summarized only through approved source-linked transformations.

## 95. Optional Context

Optional records are omitted first.

## 96. Deterministic Selection

Given the same:

```text
authoritative state
role
initiating input
provider capability
budget
policy version
```

selection order and result should be deterministic.

## 97. Stable Tie Breaking

Equal relevance uses stable tie breakers such as:

```text
PriorityClass
RelevanceScore
Importance
Sequence
ContextRecordId
```

## 98. Relevance Score

Relevance is a Chronicle-owned numeric or ordinal value.

## 99. Relevance Inputs

Possible inputs:

- entity overlap;
- recency;
- explicit player mention;
- current location;
- current conflict;
- Memory importance;
- remembered-by;
- role;
- workflow;
- Rule Operation dependency.

## 100. Provider Does Not Rank Full Private Corpus

Chronicle does not send all records to the provider for selection.

## 101. Selection Pipeline

Recommended pipeline:

```text
Determine Role and Workflow
    ↓
Load Candidate Records
    ↓
Apply Visibility and Transmission Filters
    ↓
Classify Authority and Sensitivity
    ↓
Mark Critical and Required Records
    ↓
Calculate Relevance
    ↓
Estimate Budget Cost
    ↓
Select by Stable Priority
    ↓
Apply Approved Summaries
    ↓
Validate Critical Completeness
    ↓
Serialize Provider-Neutral Package
    ↓
Fingerprint
```

## 102. Filter Before Ranking

Forbidden records are removed before relevance ranking.

## 103. No Budget-Based Secret Leakage

A Secret never becomes eligible merely because budget remains.

## 104. Record Budget Cost

Each record receives an estimated serialized cost.

## 105. Serialization-Aware Estimation

Estimation should reflect the actual provider-neutral representation.

## 106. Whole-Record Inclusion

Structured records are included whole.

## 107. Structured List Trimming

For large collections, trim by complete item.

## 108. Field-Level Minimization

A context record may have a role-specific projection containing only permitted fields.

## 109. No Accidental Object Serialization

Chronicle MUST NOT serialize Domain or EF objects directly into provider context.

## 110. Prompt DTOs

Prompt contracts use dedicated immutable DTOs.

## 111. Summarization

Summaries are derived context records.

## 112. Summary Creation

A summary is created through a governed workflow.

It may use:

- deterministic local transformation;
- Narrative Intelligence proposal followed by validation;
- Archivist proposal;
- explicit user-approved summary.

## 113. Summary Source Range

Every summary records:

```text
SourceType
SourceStart
SourceEnd
SourceVersion
SummaryContractVersion
CreatedAtUtc
ValidationStatus
```

## 114. Summary Staleness

A summary becomes stale when its source range or source version changes.

## 115. Summary Scope

Summaries may exist for:

- Scene;
- Act;
- Session;
- Campaign arc;
- Character relationship;
- transcript range;
- Rule Knowledge source.

## 116. Summary Authority Label

The provider receives summaries as derived representations.

## 117. Summary Contradiction

When a summary conflicts with current canonical state, canonical state wins and the summary is marked stale.

## 118. No Recursive Summary Drift

Chronicle SHOULD prefer summarizing authoritative sources rather than repeatedly summarizing prior summaries.

## 119. Compression Levels

A summary may have bounded levels:

```text
Detailed
Standard
Compact
Minimal
```

## 120. Budget Adaptation

Prompt Construction may choose a more compact validated summary under budget pressure.

## 121. Transcript Summaries

Older transcript windows may be represented by Session or Scene summaries.

## 122. Memory Versus Summary

A Memory represents a lived event with lifecycle and relevance.

A summary compresses a source range.

They remain distinct.

## 123. Context Deduplication

Prompt Construction removes redundant representations when safe.

## 124. Deduplication Preference

Prefer:

```text
current canonical record
over
stale summary

accepted mechanical evidence
over
narrative paraphrase

structured Character Knowledge
over
duplicate transcript claim
```

## 125. Deduplication Does Not Erase Disagreement

Contradictory beliefs and truth are both included when narratively relevant, with labels.

## 126. Context Conflict Detection

Before serialization, Chronicle detects incompatible records.

## 127. Conflict Resolution

Resolution follows authority ordering.

Unresolved material conflicts may block provider invocation.

## 128. Authority Ordering

Recommended precedence:

```text
Current canonical state
Accepted mechanical evidence
Accepted lifecycle history
Current Preference snapshot
Character Knowledge classification
Accepted transcript
Validated summaries
Narrative color
Unverified content
```

## 129. Historical Truth

Historical records are not overwritten by current state.

The context distinguishes:

- what was true then;
- what is true now.

## 130. Temporal Labels

Records may include fictional and accepted-time labels.

## 131. Clock Use

System timestamps are metadata, not narrative chronology.

## 132. Fictional Time

Fictional time is included only when represented as Campaign state.

## 133. Prompt Contract Version

Every package records:

```text
PromptContractVersion
ContextPolicyVersion
SerializationVersion
```

## 134. Fingerprint

The context fingerprint includes canonical serialized semantic content, not provider-specific formatting.

## 135. Fingerprint Uses

The fingerprint supports:

- retry identity;
- stale-context detection;
- diagnostics;
- provider comparison;
- caching;
- test assertions.

## 136. Fingerprint Privacy

A fingerprint is not reversible content and may be logged.

## 137. Included Record Evidence

Chronicle MAY persist safe metadata listing included ContextRecordIds and source versions.

## 138. Full Prompt Retention

Full prompts are not persisted indefinitely by default.

## 139. Development Retention

Development mode may retain prompts with synthetic or explicitly consented data.

## 140. Diagnostic Capture

A user may explicitly create a scrubbed diagnostic package.

## 141. Prompt Scrubbing

Scrubbing removes or replaces:

- names;
- narrative text;
- Character biographies;
- Secrets;
- user-provided Rule Knowledge;
- provider credentials;
- local paths.

## 142. Provider Cache

Provider-side prompt caching MAY be used when supported.

## 143. Cache Key

The cache key must include:

- role contract version;
- prompt contract version;
- package version;
- context fingerprint;
- provider formatting version.

## 144. Cache Authority

Provider cache is an optimization.

Chronicle still owns the request content and result validation.

## 145. Context Reuse

Static role and output-contract sections may be reused.

Dynamic Campaign context is rebuilt from current state.

## 146. Stale Context

Before provider result commit, ADR-0034 rechecks material state versions.

## 147. Context Rebuild

A stale turn rebuilds context from current authoritative state.

## 148. No Prompt Mutation During Attempt

One ProviderAttempt uses one immutable prompt package.

## 149. Provider-Specific Limits

Adapters may reject packages that exceed provider limits after precise tokenization.

## 150. Final Adapter Check

Before transmission, the adapter verifies:

- final size;
- output reservation;
- required structured-output capability;
- no unsupported content type.

## 151. Adapter Overflow

If provider-specific formatting causes overflow, the request returns to Prompt Construction for a smaller package.

The adapter must not silently drop records.

## 152. Multi-Modal Context

Images, audio, and other media are deferred.

## 153. Future Media Policy

Future media context must have the same disclosure, licensing, and budget controls.

## 154. Context for Dice Continuation

A Dice continuation package prioritizes:

- pre-Roll narrative boundary;
- accepted Roll evidence;
- applied consequences;
- current Scene state;
- relevant Character reactions;
- narrative Preferences.

## 155. Context for Choice Continuation

A choice continuation prioritizes:

- original choice prompt;
- selected option;
- resulting authoritative changes;
- current Scene state.

## 156. Context for Archivist

Archivist context prioritizes:

- finalized Session transcript;
- accepted Dice and consequences;
- Scene and Act boundaries;
- existing Memories;
- Character Knowledge;
- progression evidence;
- relationship changes;
- finalization rules.

## 157. Archivist Budget

Archivist may use larger context or staged summarization because finalization is not a live turn.

## 158. Archivist Source Completeness

Finalization must not omit critical accepted Session events merely due to convenience.

It may use validated Scene and Act summaries when source-linked.

## 159. Narrator Budget

Narrator requests prioritize latency and immediate continuity.

## 160. Different Role, Different Selection

Narrator and Archivist do not receive identical context packages.

## 161. Rule Knowledge for Archivist

The Archivist receives only Rule Knowledge needed for classification or progression proposals.

## 162. Safety Constraints

Safety constraints are included in every narrative-generation request where relevant.

## 163. Safety Priority

Safety constraints are Critical.

## 164. Safety Minimization

Only the necessary operational form is sent.

Sensitive personal explanation may remain local.

## 165. Disclosure Audit

Chronicle SHOULD support a user-facing or diagnostic view of context categories sent, without exposing internal system prompts by default.

## 166. Disclosure Summary

A safe summary may show:

```text
Current Scene
Recent messages
3 relevant Memories
Player Character mechanical summary
2 relationships
Rule Set terminology
Safety constraints
1 Rule Knowledge excerpt
```

## 167. User Control

The MVP may expose high-level disclosure controls, not per-record prompt editing.

## 168. No Raw Prompt Editor

A raw system-prompt editor is outside MVP.

## 169. User-Provided Narrative Guidance

Future custom guidance must be represented as bounded, classified Campaign configuration.

## 170. Error Model

Recommended errors:

```text
prompt.role-contract-not-found
prompt.context-policy-not-found
prompt.context-build-failed
prompt.context-budget-exceeded
prompt.context-budget-critical-overflow
prompt.critical-record-missing
prompt.context-conflict
prompt.record-not-transmittable
prompt.rule-knowledge-blocked
prompt.summary-stale
prompt.serialization-failed
prompt.provider-limit-exceeded
prompt.provider-capability-incompatible
prompt.recovery-required
```

## 171. Data Preservation State

Prompt results SHOULD state:

```text
NoAuthoritativeMutation
ContextBuilt
CriticalContextComplete
OptionalContextOmitted
SummaryUsed
RuleKnowledgeIncluded
DisclosureBlocked
ProviderInvocationNotStarted
```

## 172. Logging

Logs MAY include:

- NarrativeTurnId;
- role key;
- policy versions;
- context fingerprint;
- included record counts by type;
- omitted record counts by priority;
- estimated budget;
- provider final budget;
- Rule Knowledge excerpt count;
- failure code.

They MUST NOT include full prompt content by default.

## 173. Metrics

Useful metrics include:

```text
PromptBuildDuration
PromptEstimatedTokens
PromptFinalTokens
CriticalOverflowCount
OptionalRecordOmissionCount
SummaryUseCount
RuleKnowledgeExcerptCount
PromptDisclosureBlockCount
PromptContextConflictCount
```

## 174. Budget Telemetry

Telemetry SHOULD compare estimated and provider-reported token use when available.

## 175. Privacy Telemetry

Telemetry must not include private record content.

## 176. Testing Strategy

The implementation requires:

```text
Policy Unit Tests
Deterministic Selection Tests
Budget Tests
Disclosure Tests
Role Tests
Memory Ranking Tests
Rule Knowledge Tests
Summary Tests
Prompt Injection Tests
Provider Adapter Tests
Architecture Tests
```

## 177. Determinism Tests

Tests MUST prove identical input state and policy produce the same selected records, order, serialization, and fingerprint.

## 178. Role Tests

Tests MUST compare Narrator and Archivist context.

## 179. Authority Tests

Tests MUST prove truth, Knowledge, belief, suspicion, and misunderstanding remain distinct.

## 180. Budget Tests

Tests MUST cover:

- generous budget;
- exact fit;
- optional omission;
- summary substitution;
- critical overflow;
- adapter-specific overflow;
- output reservation;
- safety margin.

## 181. Whole-Record Tests

Tests MUST prove no structured record is truncated into invalid meaning.

## 182. Memory Tests

Tests MUST cover:

- permanent high-relevance Memory;
- temporary recent Memory;
- archived directly relevant Memory;
- unrelated high-importance Memory;
- remembered-by active Character;
- entity overlap;
- stable tie breaking.

## 183. Transcript Tests

Tests MUST cover:

- recent window;
- Roll boundary;
- choice boundary;
- long message;
- summary use;
- sequence order;
- no unrelated Scene leakage.

## 184. Character Context Tests

Tests MUST cover:

- Player Character priority;
- relevant NPC;
- hidden field omission;
- mechanical subset;
- narrative profile subset;
- missing package.

## 185. Preference Tests

Tests MUST prove:

- relevant mechanical Preference included;
- narrative Preference included;
- local UI Preference omitted;
- safety Preference prioritized;
- version preserved.

## 186. Rule Knowledge Tests

Tests MUST cover:

- official allowed excerpt;
- user-provided allowed source;
- local-only source;
- copyright-restricted source;
- conflicting source;
- bounded excerpt;
- package-version filter.

## 187. Prompt Injection Tests

Tests MUST include malicious text in:

- player input;
- transcript;
- Character biography;
- Memory;
- imported Campaign;
- Rule Knowledge;
- provider prior output.

The system must preserve it as data.

## 188. Secret Tests

Synthetic secrets placed in:

- credentials;
- hidden Character data;
- local files;
- diagnostics;
- unrelated Campaign;
- Safety Preference explanation

must not leak unless policy explicitly allows a safe transformed form.

## 189. Summary Tests

Tests MUST cover:

- valid source-linked summary;
- stale summary;
- summary conflict;
- compact level;
- no recursive drift;
- canonical override.

## 190. Provider Adapter Tests

Tests MUST prove adapter formatting does not alter:

- role;
- authority labels;
- record order;
- output schema;
- disclosure policy.

## 191. Required Test Cases

Tests MUST cover:

- normal Narrator turn;
- Dice continuation;
- choice continuation;
- Scene opening;
- Archivist finalization;
- small budget;
- large Campaign;
- conflicting belief and truth;
- hidden Secret;
- relevant archived Memory;
- unauthorized Rule Knowledge;
- prompt injection;
- stale summary;
- provider token mismatch;
- deterministic fingerprint;
- no full prompt logging.

## 192. Architecture Tests

Architecture tests MUST reject:

- prompt strings built in ViewModels;
- provider context built in adapters from Domain entities;
- direct Domain or EF serialization;
- credentials in prompt DTOs;
- unrestricted database dump;
- package code adding system instructions;
- Rule Set code invoking Prompt Construction;
- provider adapter dropping records silently;
- raw prompt logging in Stable;
- summaries without source metadata.

## 193. Prohibited Patterns

### 193.1 Send Everything

Context is selected and minimized.

### 193.2 Send Only Recent Chat

Structured truth, Knowledge, Memories, Preferences, and mechanics remain necessary.

### 193.3 Let Provider Choose from Full Private Corpus

Chronicle selects before transmission.

### 193.4 Treat Belief as Truth

Authority classes remain explicit.

### 193.5 Truncate JSON or Structured Record Midway

Omit or summarize whole records.

### 193.6 Hide Critical Overflow by Dropping Facts

Fail safely.

### 193.7 Insert Imported Text as System Instruction

Untrusted text remains data.

### 193.8 Let Adapter Rewrite Semantic Prompt

Adapters format only.

### 193.9 Persist Every Full Prompt Forever

Use fingerprints and safe metadata by default.

### 193.10 Send Unauthorized Rule Knowledge

Transmission policy and licensing apply.

## 194. Alternatives Considered

### Entire Campaign Dump

Rejected because of privacy, cost, contradiction risk, and poor relevance.

### Last-N Messages Only

Rejected because long-term continuity and structured truth require more than recency.

### Provider-Side Retrieval Over Local Database

Rejected because it would weaken local authority, privacy, and deterministic disclosure.

### One Prompt Template for Every Role

Rejected because Narrator and Archivist require different authority and evidence.

### Free-Form String Concatenation

Rejected because it is difficult to validate, budget, test, and secure.

### Persist Full Prompts Indefinitely

Rejected as a default due to privacy and storage concerns.

## 195. Consequences

### Positive

- explicit privacy boundary;
- better long-running continuity;
- lower prompt-injection risk;
- deterministic provider inputs;
- clear role separation;
- measurable budgets;
- Rule Knowledge licensing enforcement;
- provider-neutral architecture;
- diagnosable omissions and overflows.

### Negative

- context records and classifiers add implementation work;
- relevance policy requires tuning;
- summaries require lifecycle management;
- strict critical-context handling may block some requests;
- provider token estimates vary;
- role-specific context increases test scope.

## 196. Risks

### Relevance Ranking Omits Important Context

Mitigation:

- critical classifications;
- source-specific rules;
- fixtures;
- user-visible disclosure summary;
- metrics.

### Context Becomes Too Large

Mitigation:

- priority;
- summaries;
- bounded transcript;
- role-specific projection;
- explicit failure.

### Prompt Injection Still Influences Model

Mitigation:

- clear instruction hierarchy;
- typed data sections;
- minimal tool authority;
- output validation;
- adversarial tests.

### Summary Drift

Mitigation:

- source links;
- stale detection;
- canonical precedence;
- avoid recursive summaries.

### Token Estimate Is Wrong

Mitigation:

- provider adapter final check;
- safety margin;
- telemetry;
- rebuild with smaller budget.

## 197. Technology Spike

Before acceptance, implement:

1. prompt context-record contracts;
2. role contracts for Narrator and Archivist;
3. disclosure classifier;
4. Memory relevance ranker;
5. transcript selector;
6. Character context projector;
7. Preference selector;
8. Rule Knowledge selector;
9. budget allocator;
10. summary selection;
11. provider-neutral serializer;
12. context fingerprint;
13. provider adapter token check;
14. prompt-injection test corpus;
15. disclosure diagnostics.

## 198. Spike Acceptance

The spike passes when:

- Narrator and Archivist receive different correct contexts;
- canonical truth and Character Knowledge remain distinct;
- relevant Memories outrank unrelated records deterministically;
- required Dice and choice evidence always remains;
- optional content is omitted before critical content;
- critical overflow blocks provider invocation;
- no credential, unrelated Campaign data, or forbidden Secret leaks;
- Rule Knowledge follows transmission and licensing policy;
- malicious imported instructions remain inert data;
- the same state and policy produce the same fingerprint;
- provider adapters do not change semantic content.

## 199. Definition of Compliance

An implementation complies when:

- all provider context passes through one Chronicle-owned construction boundary;
- records are typed, classified, and role-filtered;
- truth, Knowledge, belief, suspicion, misunderstanding, and claims remain distinct;
- selection and ordering are deterministic;
- budgets reserve output capacity and preserve critical context;
- structured records are not truncated mid-record;
- summaries are versioned and source-linked;
- credentials, local diagnostics, unrelated Campaigns, and prohibited Secrets remain excluded;
- Rule Knowledge follows relevance, licensing, and transmission policy;
- user and imported text remain data rather than instructions;
- provider-neutral packages are fingerprinted;
- adapters format but do not alter semantics;
- full prompt retention and logging remain privacy-safe by default.

## 200. Review Triggers

This ADR must be reviewed if:

- public third-party provider adapters are introduced;
- provider-side retrieval becomes a requirement;
- multi-player role visibility is introduced;
- media context becomes supported;
- custom user prompts become a product feature;
- local models with very large contexts change budget assumptions;
- server hosting centralizes context construction;
- semantic vector retrieval becomes authoritative;
- encrypted prompt archives are introduced;
- regulatory or privacy requirements change provider disclosure.

## 201. Deferred Decisions

Later ADRs MAY define:

- exact relevance scoring formula;
- exact token-estimation library;
- vector retrieval implementation;
- custom user guidance contracts;
- multi-player visibility filtering;
- image and audio context;
- encrypted prompt diagnostics;
- provider-side caching protocol;
- public disclosure inspector;
- summary-generation pipeline;
- local-only Narrative Intelligence mode.

## 202. Final Decision

Chronicle will construct every provider request from typed, classified, role-specific context selected under an explicit budget.

Canonical truth, Character Knowledge, belief, suspicion, misunderstanding, narrative history, and Rule Knowledge will remain distinguishable.

Critical context will never be silently discarded.

Secrets and unrelated data will never be sent merely because they exist.

The provider receives only the world Chronicle deliberately reveals.

That disclosure is part of Chronicle's authority.
