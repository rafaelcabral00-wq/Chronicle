---
id: RFC-0022
title: Archivist Contract
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-07-31
category: Contracts
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
---

> **"The Archivist does not decide what happened. It identifies what should continue to matter."**

# Archivist Contract

## Abstract

This RFC defines the provider-neutral contract between Chronicle and the Archivist capability.

It establishes the finalization request, evidence model, proposal envelope, Session summary, Campaign Memory proposals, Character progression proposals, Relationship changes, Character Knowledge changes, Secret updates, unresolved consequences, Narrative Plan revision suggestions, validation requirements, item-level acceptance, repair, retries, and privacy boundaries.

The Archivist produces structured proposals from authoritative Session evidence.

Chronicle validates and applies only accepted changes.

## 1. Purpose

The Archivist is responsible for interpreting completed play without becoming the source of Campaign truth.

Its contract must enable Chronicle to determine:

- what the Session meant;
- what should become a Campaign Memory;
- what existing Memory became more or less relevant;
- whether Characters progressed;
- which persistent Character State changes are justified;
- which Relationships changed;
- what each Character learned, believed, suspected, or disproved;
- whether Secrets were revealed;
- which consequences remain unresolved;
- whether the Narrative Plan should be revised;
- how the Session should be summarized for the player.

The contract MUST remain structured, auditable, provider-neutral, and safe to retry.

## 2. Scope

This RFC defines:

- Archivist operation profile;
- request envelope;
- finalization evidence;
- pre-Session baselines;
- immediate changes;
- deferred changes;
- response envelope;
- Session summary;
- Memory proposals;
- Memory update proposals;
- progression proposals;
- Character State proposals;
- Relationship proposals;
- Character Knowledge proposals;
- Secret proposals;
- unresolved consequence proposals;
- Narrative Plan revision suggestions;
- evidence references;
- confidence;
- warnings;
- validation;
- partial acceptance;
- repair;
- stale response detection;
- retries;
- testing.

This RFC does not define:

- exact serialized JSON;
- exact prompt text;
- exact provider SDK mapping;
- Rule Set-specific progression formulas;
- exact finalization UI;
- exact Memory scoring algorithm;
- exact Plan revision contract.

## 3. Archivist Definition

The `Archivist` is a Narrative Intelligence capability that transforms authoritative Session evidence into a structured finalization proposal.

Conceptually:

```text
SessionFinalizationRequest
        ↓
Archivist Capability
        ↓
ArchivistResponse
        ↓
Chronicle Validation
        ↓
FinalizationChangeSet
```

The Archivist does not persist or apply the Change Set.

## 4. Archivist Responsibilities

The Archivist MAY propose:

- player-visible Session summary;
- internal Session summary;
- new Campaign Memories;
- updates to existing Memories;
- Character progression evidence;
- long-term Character State changes;
- Relationship changes;
- Character Knowledge changes;
- Secret reveal or correction;
- unresolved consequences;
- Narrative Plan revision suggestion;
- warnings and ambiguities.

## 5. Archivist Prohibitions

The Archivist MUST NOT:

- rewrite Session transcript;
- alter raw Dice Roll values;
- reinterpret a Rule Set result contrary to the Rule Set;
- persist entities;
- generate persistent identifiers;
- apply Character Sheet changes directly;
- award progression authoritatively;
- age Memories;
- duplicate immediate changes;
- expose hidden information in player-visible summaries;
- mark the Session completed;
- rewrite completed Campaign history;
- create unsupported Rule Set mechanics;
- infer evidence from provider conversation history.

## 6. Operation Profile

The primary Archivist operation profile is:

```text
FinalizeSession
```

Future profiles MAY include:

```text
ReevaluateFinalization
RepairArchivistOutput
GenerateHistoricalSummary
```

The MVP SHOULD implement only `FinalizeSession` and optional repair.

## 7. Archivist Request Envelope

A `SessionFinalizationRequest` SHOULD contain:

```text
SessionFinalizationRequest
├── OperationId
├── CorrelationId
├── ContractVersion
├── Locale
├── CampaignReference
├── CampaignVersion
├── SessionReference
├── SessionRevision
├── RuleSetReference
├── PlayerCharacterBaseline
├── RelevantCharacterBaselines
├── ExecutedActs
├── ExecutedScenes
├── AcceptedMessages
├── ResolvedDiceRolls
├── ImmediateChanges
├── ExistingMemories
├── ExistingRelationships
├── ExistingKnowledge
├── ActiveSecrets
├── CampaignPreferences
├── NarrativePlanReference
├── UnresolvedObjectives
├── OutputConstraints
└── VisibilityConstraints
```

## 8. Campaign Reference

The Campaign reference SHOULD include:

- Campaign identifier;
- public title;
- premise summary;
- current status;
- relevant themes;
- Rule Set identity;
- Campaign Preferences relevant to interpretation.

The complete Campaign MUST NOT be included by default.

## 9. Session Reference

The Session reference SHOULD include:

- Session identifier;
- sequence number;
- start and end timestamps;
- status;
- active or interrupted ending;
- executed Act count;
- executed Scene count;
- finalization reason.

## 10. Rule Set Reference

The request MUST identify:

- Rule Set identifier;
- Rule Set version;
- relevant progression guidance;
- relevant terminology;
- relevant finalization rules;
- relevant Character Sheet schema version.

The Archivist MUST not invent mechanics absent from this context.

## 11. Pre-Session Baseline

The request SHOULD include the pre-Session baseline for affected entities.

This MAY include:

- Player Character Sheet snapshot;
- Character State;
- relevant NPC State;
- Relationship values;
- Character Knowledge;
- active Campaign Memories;
- unresolved objectives;
- Narrative Plan version.

The baseline allows the Archivist to identify change rather than repeat existing truth.

## 12. Player Character Baseline

The Player Character baseline SHOULD include:

- Character identifier;
- Character role;
- relevant Character Sheet fields;
- Character State before Session;
- Narrative Profile summary;
- active objectives;
- relevant Memories;
- progression state;
- visible Relationships;
- Knowledge relevant to the Session.

## 13. Relevant Character Baselines

NPC baselines SHOULD be limited to Characters who:

- participated in executed Scenes;
- were materially affected;
- became the subject of a Memory;
- had a Relationship or Knowledge change;
- own or know a relevant Secret.

The complete Campaign cast SHOULD not be sent.

## 14. Executed Acts

Each executed Act SHOULD include:

- identifier;
- title;
- objective;
- status;
- executed Scene references;
- visible outcome;
- unresolved objective;
- whether planned or dynamically created.

## 15. Executed Scenes

Each executed Scene SHOULD include:

- identifier;
- title;
- location;
- objective;
- participants;
- status;
- major accepted events;
- Scene completion reason;
- unresolved consequences;
- dynamic or planned origin.

## 16. Accepted Messages

Messages provided to the Archivist MUST be committed and accepted.

Each Message SHOULD include:

- Message identifier;
- sequence;
- speaker role;
- Character reference when applicable;
- Scene reference;
- content;
- visibility;
- originating OperationId.

Rejected, stale, or uncommitted provider output MUST NOT be included as evidence.

## 17. Resolved Dice Rolls

Each resolved Roll SHOULD include:

- Dice Roll identifier;
- Scene;
- acting Character;
- target when applicable;
- operation key;
- stakes;
- raw dice;
- authoritative Rule Set result;
- applied immediate consequences;
- visibility;
- Rule Set version.

The Archivist MUST not reinterpret mechanics.

## 18. Immediate Changes

Immediate changes are authoritative state transitions already applied during play.

Each change SHOULD include:

- change type;
- target entity;
- prior value;
- resulting value;
- source operation;
- Scene;
- Dice Roll when applicable;
- visibility;
- persistence status.

The Archivist MUST treat them as already applied.

## 19. Existing Memories

The request SHOULD include existing Memories that are relevant to:

- Session participants;
- Session objectives;
- repeated events;
- possible duplicate proposals;
- reinforcement;
- supersession;
- unresolved consequences.

Each Memory SHOULD include:

- identifier;
- summary;
- scope;
- importance;
- relevance;
- lifetime;
- age;
- status;
- involved Characters;
- remembered-by state;
- origin.

## 20. Existing Relationships

Relevant Relationship context SHOULD include:

- Relationship identifier;
- source Character;
- target Character;
- dimensions;
- status;
- visibility;
- existing summary;
- recent meaningful changes;
- associated Memories.

Directionality MUST remain explicit.

## 21. Existing Knowledge

Relevant Character Knowledge SHOULD include:

- Knowledge identifier;
- Character;
- subject;
- state;
- confidence;
- source;
- relation to Campaign Truth;
- visibility;
- status.

## 22. Active Secrets

Relevant Secrets SHOULD include:

- Secret identifier;
- canonical truth;
- associated subject;
- known-by Characters;
- suspected-by Characters;
- current reveal status;
- partial reveal state;
- visibility;
- reveal constraints.

The Archivist MUST not place full hidden truth in player-visible output.

## 23. Narrative Plan Reference

The request MAY include bounded Plan context:

- current Plan version;
- current Act purpose;
- planned future direction relevant to divergence;
- obsolete planned content candidates;
- known divergence triggers.

The Archivist SHOULD not receive unrelated future Plan content.

## 24. Unresolved Objectives

The request SHOULD identify objectives that were active before or during the Session.

Each objective MAY include:

- objective identifier;
- owner;
- status before Session;
- related Act or Scene;
- completion evidence;
- unresolved state;
- visibility.

## 25. Output Constraints

Output constraints SHOULD define:

- allowed proposal types;
- maximum proposal count by type;
- required summary fields;
- Memory limits;
- whether progression proposals are allowed;
- whether Plan revision suggestions are allowed;
- player-visible locale;
- output size;
- contract version.

## 26. Visibility Constraints

Visibility constraints SHOULD define:

- player-visible information;
- hidden internal information;
- partially revealable information;
- content forbidden from public summary;
- Character-specific secrecy boundaries.

## 27. Archivist Response Envelope

An `ArchivistResponse` SHOULD contain:

```text
ArchivistResponse
├── OperationId
├── ContractVersion
├── CampaignVersion
├── SessionRevision
├── CompletionStatus
├── PlayerVisibleSummary
├── InternalSummary
├── MemoryProposals
├── MemoryUpdateProposals
├── ProgressionProposals
├── CharacterStateProposals
├── RelationshipProposals
├── KnowledgeProposals
├── SecretProposals
├── UnresolvedConsequenceProposals
├── PlanRevisionSuggestion
├── Warnings
└── ProviderMetadata
```

## 28. Completion Status

Canonical statuses are:

```text
Completed
Partial
Unable
Refused
Invalid
```

The response SHOULD be `Partial` when noncritical proposal areas are missing but a valid summary or subset exists.

## 29. Player-Visible Summary

The player-visible summary SHOULD describe:

- major Acts and Scenes;
- important choices;
- meaningful victories and failures;
- visible Dice Roll consequences;
- visible Character changes;
- visible unresolved threads;
- visible new Memories.

It MUST NOT reveal:

- hidden Secrets;
- private NPC motives;
- future Plan content;
- hidden Relationship values;
- unknown Character Knowledge;
- rejected or speculative proposals.

## 30. Internal Summary

The internal summary MAY contain a richer operational interpretation.

It MAY include:

- hidden consequence context;
- private NPC changes;
- Secret state;
- plan divergence;
- internal warnings;
- finalization rationale.

The internal summary is not player-facing.

It is not authoritative by itself.

## 31. Memory Proposal

A new `MemoryProposal` SHOULD contain:

- temporary proposal key;
- summary;
- optional description;
- scope;
- type;
- importance;
- initial relevance;
- lifetime type;
- remaining Sessions when temporary;
- involved Characters;
- remembered-by proposals;
- emotional tags;
- origin references;
- evidence references;
- visibility;
- confidence;
- duplicate candidates;
- proposed consequences already handled or deferred.

The proposal key is local to the response.

Chronicle assigns the persistent Memory identifier.

## 32. Memory Proposal Rules

A proposed Memory SHOULD:

- represent meaningful lived experience;
- be specific;
- avoid transcript duplication;
- avoid repeating existing Memory without reinforcement intent;
- distinguish Campaign truth from Character belief;
- preserve hidden-information boundaries;
- cite evidence.

## 33. Memory Update Proposal

A `MemoryUpdateProposal` SHOULD contain:

- existing Memory identifier;
- update type;
- proposed relevance change;
- proposed importance correction;
- proposed status change;
- proposed lifetime change when policy allows;
- remembered-by update;
- emotional tag update;
- supersession target or replacement;
- merge candidates;
- reason;
- evidence;
- confidence.

The Archivist MUST NOT decrement lifetime or increment age.

## 34. Memory Reinforcement

A Memory update MAY propose reinforcement when the Session makes an existing Memory newly important.

A reinforcement SHOULD identify:

- existing Memory;
- new evidence;
- relevance adjustment;
- possible lifetime extension under policy;
- affected Characters;
- reason.

Chronicle applies any lifetime policy deterministically.

## 35. Memory Supersession

A supersession proposal SHOULD identify:

- old Memory;
- replacement proposal or existing Memory;
- reason;
- whether the old Memory was incorrect, incomplete, or obsolete;
- affected Character perspectives;
- evidence.

## 36. Progression Proposal

A `ProgressionProposal` SHOULD contain:

- Character identifier;
- progression type;
- narrative evidence;
- relevant Rule Set criteria;
- proposed award or milestone;
- proposed spend only when explicitly requested by workflow;
- confidence;
- visibility;
- evidence references.

The Rule Set calculates or validates authoritative progression.

## 37. Progression Rules

The Archivist MUST NOT:

- invent progression currencies;
- exceed Rule Set bounds;
- apply progression;
- duplicate already applied progression;
- treat prose quality as a mechanical criterion without Rule Set support.

## 38. Character State Proposal

A `CharacterStateProposal` SHOULD contain:

- Character identifier;
- state key;
- prior expected value;
- proposed value or transition;
- duration when applicable;
- reason;
- source;
- evidence;
- whether already applied;
- visibility;
- Rule Set operation or validation key where needed.

## 39. State Proposal Rules

The Archivist SHOULD propose only persistent or long-term state changes.

Immediate state already applied MUST be marked and MUST NOT be proposed again as a new mutation.

## 40. Relationship Proposal

A `RelationshipProposal` SHOULD contain:

- Relationship identifier when existing;
- source Character;
- target Character;
- proposal type;
- changed dimensions;
- expected prior values;
- proposed deltas or values;
- narrative reason;
- associated Memory;
- visibility;
- evidence;
- confidence.

## 41. Relationship Proposal Types

Possible proposal types:

```text
Create
Update
End
Reactivate
Archive
```

The MVP SHOULD primarily use `Create` and `Update`.

## 42. Relationship Directionality

Every proposal MUST identify:

```text
Source Character → Target Character
```

A reciprocal change requires a separate proposal.

The Archivist MUST NOT assume symmetry.

## 43. Relationship Evidence

Relationship changes SHOULD be supported by:

- explicit promise;
- betrayal;
- rescue;
- abandonment;
- public humiliation;
- repeated cooperation;
- discovered lie;
- accepted Dice Roll consequence;
- significant Session outcome.

Minor dialogue alone SHOULD not automatically produce major changes.

## 44. Knowledge Proposal

A `KnowledgeProposal` SHOULD contain:

- Character identifier;
- subject;
- existing Knowledge identifier when applicable;
- proposed knowledge state;
- confidence;
- source;
- relation to Campaign Truth;
- visibility;
- evidence;
- immediate or deferred classification.

## 45. Knowledge Proposal States

Allowed target states SHOULD follow RFC-0010:

```text
Known
Believed
Suspected
Misunderstood
Forgotten
Disproven
```

The Archivist MUST distinguish certainty.

## 46. Belief Correction Proposal

A correction SHOULD identify:

- old belief or suspicion;
- new Knowledge state;
- evidence;
- whether an existing Memory should be superseded;
- possible Relationship consequence;
- visibility.

The old record SHOULD not be silently deleted.

## 47. Secret Proposal

A `SecretProposal` SHOULD contain:

- Secret identifier;
- proposal type;
- affected Character;
- prior reveal state;
- proposed reveal state;
- exact revealed fragment;
- evidence;
- visibility;
- confidence.

## 48. Secret Proposal Types

Possible types:

```text
PartialReveal
FullReveal
KnowledgeUpdate
CorrectMisconception
MarkObsolete
```

Creating a new Secret during finalization SHOULD be rare and may require a separate workflow.

## 49. Unresolved Consequence Proposal

An `UnresolvedConsequenceProposal` SHOULD contain:

- consequence type;
- subject;
- summary;
- status;
- urgency;
- related objective;
- related Memory;
- related Character;
- related Scene;
- visibility;
- suggested destination.

Suggested destinations MAY include:

```text
CampaignState
CharacterState
Objective
Memory
Relationship
PlanRevision
```

Chronicle decides the actual operation.

## 50. Plan Revision Suggestion

A `PlanRevisionSuggestion` SHOULD contain:

- current Plan version;
- reason;
- triggering evidence;
- affected future area;
- obsolete planned content candidates;
- new narrative pressure;
- urgency;
- whether play can continue before revision;
- visibility.

It MUST NOT contain a full unrestricted rewritten Plan unless the operation profile explicitly requests revision.

## 51. Warning

Warnings MAY include:

- ambiguous evidence;
- conflicting Character perspectives;
- possible duplicate Memory;
- progression uncertainty;
- missing Rule Set guidance;
- incomplete Session evidence;
- hidden-information risk;
- plan divergence.

Warnings SHOULD be machine-readable.

## 52. Evidence Reference

Every state-relevant proposal SHOULD cite evidence.

An evidence reference MAY point to:

- Message identifier;
- Dice Roll identifier;
- Scene identifier;
- Act identifier;
- immediate change identifier;
- existing Memory;
- Character Knowledge record;
- Relationship record;
- Secret record.

## 53. Evidence Sufficiency

Chronicle MUST evaluate whether evidence supports the proposal.

The Archivist's confidence does not replace evidence.

Unsupported proposals MUST be rejected, repaired, or downgraded.

## 54. Confidence

Confidence MAY use a normalized bounded score.

It represents proposal certainty.

It MUST NOT determine acceptance alone.

Chronicle SHOULD use it for:

- review ordering;
- diagnostics;
- repair decisions;
- warnings.

## 55. Proposal Identity

New proposals use temporary response-scoped keys.

Updates reference existing Chronicle identifiers.

The Archivist MUST NOT generate persistent identifiers.

## 56. Proposal Ordering

Proposals SHOULD be ordered when dependencies exist.

Example:

```text
1. Create Memory proposal
2. Relationship update references that Memory proposal key
3. Plan revision suggestion references the same occurrence
```

Chronicle resolves temporary references before Change Set construction.

## 57. Validation Pipeline

Chronicle MUST validate:

1. response envelope;
2. OperationId;
3. contract version;
4. Campaign version;
5. Session revision;
6. proposal types;
7. references;
8. evidence;
9. visibility;
10. duplicates;
11. Domain invariants;
12. Rule Set mechanics;
13. internal Change Set consistency.

## 58. Operation Identity Validation

The response OperationId MUST match the finalization request.

A response from another Session or operation MUST be rejected.

## 59. Version Validation

The response MUST match:

- Campaign version;
- Session revision;
- Rule Set version;
- Narrative Plan version where referenced.

If Session evidence changed, the response is stale.

## 60. Reference Validation

Chronicle MUST verify:

- Character ownership;
- Scene and Act ownership;
- existing Memory references;
- Relationship direction;
- Knowledge ownership;
- Secret existence;
- Roll references;
- Rule Set operation keys.

## 61. Visibility Validation

Chronicle MUST ensure:

- public summary is safe;
- visible proposal results do not expose hidden truth;
- Character-specific knowledge is not treated as player knowledge automatically;
- private Relationships remain hidden;
- plan content remains internal.

## 62. Duplicate Validation

Chronicle SHOULD detect:

- duplicate Memory proposals;
- proposal matching existing Memory;
- repeated progression;
- repeated Relationship delta;
- repeated Knowledge acquisition;
- repeated Secret reveal;
- repeated unresolved consequence.

## 63. Domain Validation

Domain validation checks:

- Memory invariants;
- Relationship directionality;
- knowledge transitions;
- Secret reveal rules;
- Session ownership;
- Campaign lifecycle;
- immutable completed history.

## 64. Rule Set Validation

The active Rule Set validates:

- progression;
- Character Sheet fields;
- Character State mechanics;
- Relationship mechanics when system-specific;
- knowledge mechanics when system-specific;
- temporary effects;
- resource changes.

## 65. Change Set Consistency

Accepted proposals MUST form one coherent Change Set.

Examples of inconsistency:

- Memory says Character died while State proposal keeps them active without explanation;
- Secret marked fully revealed but Knowledge remains unknown for the affected Character;
- Relationship change references a rejected Memory proposal;
- progression spends currency not awarded or available.

## 66. Partial Acceptance

Archivist proposals MAY be accepted item by item.

Chronicle MAY accept:

- summary;
- some Memories;
- some Relationship changes;
- some Knowledge changes;

while rejecting:

- invalid progression;
- unsupported Secret reveal;
- duplicate Memory.

## 67. Dependency-Aware Acceptance

A proposal that depends on a rejected proposal MUST be:

- rejected;
- repaired;
- or rewritten to remove the dependency.

Partial acceptance MUST not create dangling references.

## 68. Critical Proposal Areas

The following MAY be considered critical:

- progression;
- Character death or irreversible State;
- Secret reveal;
- Memory aging interaction;
- Session lifecycle completion;
- Rule Set-specific changes.

Critical validation failure MAY block finalization.

## 69. Noncritical Proposal Areas

The following MAY be noncritical:

- optional Memory description;
- emotional tags;
- summary phrasing;
- optional warning;
- minor categorization.

Noncritical failures MAY be omitted.

## 70. Deterministic Repair

Chronicle MAY repair:

- enum casing;
- duplicate temporary proposal keys;
- known aliases;
- missing request metadata;
- safe text normalization;
- ordering.

It MUST NOT invent evidence or mechanical meaning.

## 71. Provider-Assisted Repair

Repair MAY be requested for:

- malformed proposal;
- missing evidence reference;
- invalid temporary reference;
- unsafe player summary;
- ambiguous Memory scope;
- contradictory proposal set.

The repaired response is fully revalidated.

## 72. Regeneration

Regeneration is preferred when:

- Session revision changed;
- core evidence was omitted;
- hidden information leaked broadly;
- proposal contradicts authoritative Session outcome;
- wrong Rule Set was used;
- response belongs to another Session.

## 73. Repair Limits

Repair and regeneration attempts MUST be bounded.

After limits are reached, Chronicle SHOULD:

- fail recoverably;
- apply deterministic fallback where approved;
- or request user action.

## 74. Stale Response

An Archivist response is stale when:

- new Messages were committed;
- a Dice Roll was corrected or added;
- immediate changes changed;
- Campaign version changed materially;
- another finalization was applied;
- Rule Set version changed.

Stale proposals MUST NOT be applied.

## 75. Retry Semantics

Retrying the same finalization uses the same logical OperationId.

Chronicle MUST determine whether:

- no response exists;
- response exists but is unvalidated;
- response is validated;
- Change Set is ready;
- finalization already committed.

The workflow resumes from the last safe stage.

## 76. Duplicate Response

Duplicate Archivist responses MUST NOT duplicate:

- Memories;
- progression;
- Relationship changes;
- Knowledge changes;
- Secret reveals;
- unresolved consequences.

Operation and proposal identities protect application.

## 77. Failure After Commit

If finalization committed but the UI did not receive the result:

- retry returns the existing finalization result;
- the Archivist is not invoked again;
- Memory aging is not repeated;
- progression is not repeated.

## 78. Deterministic Fallback

A fallback Archivist implementation MAY produce:

- minimal Session summary marker;
- no semantic Memory proposals;
- no Relationship proposals;
- no Knowledge proposals;
- no Secret proposals;
- no progression proposal unless Rule Set can calculate deterministically.

Chronicle still applies deterministic finalization behavior.

## 79. Context Budget

Archivist context MAY be larger than Narrator context.

It MUST still be bounded.

Priority SHOULD be:

1. executed Scene structure;
2. accepted Messages;
3. resolved Dice Rolls;
4. immediate changes;
5. affected Character baselines;
6. relevant Memories;
7. Relationships and Knowledge;
8. Secrets;
9. Plan context;
10. Rule Set finalization guidance.

## 80. Long Session Strategy

For long Sessions, Chronicle MAY provide:

- Scene summaries;
- Act summaries;
- selected raw Messages;
- all resolved Rolls;
- all immediate changes.

Intermediate summaries are evidence aids.

They are not authoritative state.

## 81. Summary Quality Rules

The player-visible summary SHOULD:

- remain concise;
- preserve meaningful choices;
- mention major consequences;
- distinguish failure from success;
- avoid inventing motives;
- avoid revealing hidden information;
- avoid becoming a raw transcript.

## 82. Memory Quality Rules

Memory proposals SHOULD:

- represent enduring narrative meaning;
- avoid trivial detail unless currently relevant;
- avoid duplicating summary content mechanically;
- distinguish event, promise, discovery, belief, and consequence;
- use clear scope;
- assign realistic lifetime;
- cite evidence.

## 83. Progression Quality Rules

Progression proposals SHOULD:

- reference Rule Set criteria;
- avoid rewarding the same event twice;
- distinguish award from spending;
- identify uncertainty;
- avoid unsupported mechanical fields.

## 84. Relationship Quality Rules

Relationship proposals SHOULD:

- remain directional;
- use proportional changes;
- avoid large shifts from weak evidence;
- connect changes to meaningful events;
- preserve hidden private state;
- avoid automatic symmetry.

## 85. Knowledge Quality Rules

Knowledge proposals SHOULD:

- preserve uncertainty;
- distinguish told information from verified truth;
- represent false beliefs explicitly;
- avoid making every participant know everything;
- cite acquisition evidence.

## 86. Secret Quality Rules

Secret proposals SHOULD:

- identify exact reveal scope;
- support partial revelation;
- update only affected Characters;
- preserve hidden remaining detail;
- avoid public-summary leakage.

## 87. Observability

Chronicle SHOULD record:

- contract version;
- Campaign version;
- Session revision;
- request size;
- response size;
- proposal counts by type;
- accepted counts;
- rejected counts;
- validation failures;
- repair count;
- provider latency;
- usage;
- finalization result.

## 88. Privacy

Archivist requests may contain more Campaign history than Narrator requests.

Chronicle SHOULD minimize:

- unrelated Sessions;
- unrelated Characters;
- complete Campaign transcript;
- proprietary Rule Set text;
- hidden content unnecessary for finalization.

Raw payload logging SHOULD be disabled or redacted by default.

## 89. Prompt Injection Resistance

Session Messages and Character biographies may contain hostile instructions.

The implementation SHOULD:

- label all evidence as data;
- separate system instructions;
- restrict proposal types;
- disallow provider tools;
- validate references;
- ignore instructions embedded in evidence;
- treat all output as untrusted.

## 90. Deterministic Test Archivist

Chronicle MUST support a deterministic test Archivist.

It SHOULD produce scripted cases for:

- valid summary;
- Memory creation;
- Memory duplicate;
- progression proposal;
- invalid progression;
- Relationship update;
- Knowledge change;
- Secret reveal;
- Plan revision suggestion;
- partial response;
- malformed response;
- stale response;
- timeout;
- refusal.

## 91. Contract Tests

Every Archivist adapter MUST pass shared contract tests for:

- request mapping;
- response normalization;
- proposal type support;
- OperationId preservation;
- version preservation;
- error mapping;
- output limits;
- partial responses.

## 92. Required Test Cases

Tests MUST cover:

- successful finalization proposal;
- player-visible summary safety;
- hidden Secret in public summary;
- valid Memory proposal;
- duplicate Memory proposal;
- Memory reinforcement;
- invalid lifetime;
- progression unsupported by Rule Set;
- immediate change duplicated;
- directional Relationship proposal;
- false symmetry;
- Knowledge state correction;
- partial Secret reveal;
- Plan revision suggestion;
- unsupported persistent identifier;
- stale Session revision;
- missing evidence;
- partial acceptance;
- dependency on rejected proposal;
- provider timeout;
- duplicate response;
- failure after commit;
- repair success;
- repair limit reached;
- prompt injection in Session Message.

## 93. Prohibited Patterns

### 93.1 Archivist as Persistence Layer

The Archivist MUST NOT write Campaign data directly.

### 93.2 Summary as State

The Session summary MUST NOT become the source of mechanical or domain changes.

### 93.3 Archivist Ages Memory

Memory aging MUST remain deterministic Chronicle behavior.

### 93.4 Unstructured Progression

Progression MUST NOT be applied from prose alone.

### 93.5 Duplicate Immediate Change

Already applied state MUST NOT be proposed as new mutation.

### 93.6 Hidden Information in Player Summary

Public output MUST NOT reveal Secrets or private state.

### 93.7 Provider-Generated Persistent IDs

New entities receive identifiers from Chronicle.

### 93.8 Entire Campaign Dump

The complete Campaign MUST NOT be sent by default.

### 93.9 Confidence Replaces Evidence

High confidence MUST NOT bypass evidence validation.

### 93.10 Whole Proposal Auto-Accept

Chronicle MUST NOT persist the full proposal without item and Change Set validation.

## 94. Current Delivery Decision

The MVP adopts:

- provider-neutral Archivist port;
- one `FinalizeSession` profile;
- structured finalization request;
- structured proposal envelope;
- player-visible and internal summaries;
- Memory creation and update proposals;
- progression proposals;
- Character State proposals;
- Relationship proposals;
- Character Knowledge proposals;
- Secret proposals;
- unresolved consequences;
- Plan revision suggestion;
- evidence references;
- item-level partial acceptance;
- bounded repair;
- deterministic test Archivist;
- no direct persistence;
- no authoritative Memory aging;
- no full Campaign transcript by default.

## 95. Architecture Horizon

Future evolution MAY include:

- player review feedback sent back to Archivist;
- multi-stage Session analysis;
- Scene-level Archivist;
- local Archivist models;
- group finalization;
- multimedia highlights;
- emotional arc analysis;
- Campaign analytics;
- automated journal generation;
- historical reevaluation;
- specialized progression evaluator.

The MVP MUST NOT implement these capabilities without a later milestone.

## 96. Open Questions

The following remain open:

- What exact serialized schema will the Archivist use?
- Which proposal types are mandatory for MVP?
- Should progression proposals contain deltas or only evidence?
- Should Memory lifetime be proposed numerically or categorically?
- How many new Memories may one Session create?
- Should Relationship changes use deltas or final values?
- Which changes are critical enough to block finalization?
- How should Scene and Act summaries be generated for long Sessions?
- Should the player review the finalization proposal before commit?
- Which proposal warnings should be visible to the player?
- How should partial Secret revelation be serialized?
- Should Plan revision run automatically after finalization?
- How much raw transcript should be included?
- Should rejected proposals be retained for diagnostics?
- How should deterministic fallback behave in the first release?

These questions require RFC-0024, RFC-0025, Rule Set implementation RFCs, UI RFCs, and technology ADRs.

## 97. Compliance Checklist

An Archivist implementation complies when:

- requests contain authoritative Session evidence;
- pre-Session baseline is distinguishable;
- immediate changes are explicit;
- output is structured and versioned;
- player and internal summaries are separated;
- proposals cite evidence;
- new identifiers remain Chronicle-owned;
- progression is Rule Set-validated;
- Relationships remain directional;
- Character Knowledge preserves uncertainty;
- Secret revelation is scoped;
- stale responses are rejected;
- partial acceptance preserves consistency;
- retries do not duplicate finalization effects;
- Memory aging remains Chronicle-controlled;
- the provider never persists directly.

## 98. Final Principle

The Archivist may propose what the Session deserves to leave behind.

Chronicle accepts only what the evidence, the rules, and the Campaign can safely preserve.
