---
id: RFC-0023
title: Campaign Generator Contract
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
  - RFC-0022
---

> **"The Campaign Generator prepares a world of possibilities. Chronicle decides which structure is valid enough to begin."**

# Campaign Generator Contract

## Abstract

This RFC defines the provider-neutral contract between Chronicle and the Campaign Generator capability.

It establishes how Chronicle requests a new Campaign proposal, which inputs are authoritative, how the Player Character and Rule Set shape generation, how Campaign premises, Acts, Scenes, NPCs, Relationships, Secrets, mysteries, clues, possible outcomes, and initial Narrative Plan content are represented, and how Chronicle validates, repairs, rejects, and persists the result.

The Campaign Generator proposes a coherent starting structure.

Chronicle owns identifiers, validation, persistence, compatibility, and Campaign truth.

## 1. Purpose

Campaign generation is a high-leverage operation.

A weak contract can produce:

- generic Campaigns unrelated to the Player Character;
- invalid Character Sheets;
- unsupported mechanics;
- duplicate or incoherent NPCs;
- rigid scripts that undermine player agency;
- hidden information leaked to player-facing summaries;
- planned events accidentally stored as completed truth;
- inconsistent Rule Set terminology;
- partial Campaigns that appear playable.

The Campaign Generator contract exists to prevent those failures.

## 2. Scope

This RFC defines:

- Campaign generation operation profile;
- request envelope;
- Player Character input;
- Campaign Preferences;
- Rule Set generation guidance;
- content boundaries;
- generation constraints;
- response envelope;
- public Campaign metadata;
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
- mysteries and clues;
- possible outcomes;
- validation;
- temporary references;
- repair;
- retries;
- stale proposal detection;
- partial regeneration;
- testing.

This RFC does not define:

- exact serialized JSON;
- exact prompt text;
- exact provider SDK mapping;
- exact Werewolf campaign content;
- exact number of Acts or Scenes;
- UI wizard design;
- multimedia generation;
- community templates;
- imported published adventures.

## 3. Campaign Generator Definition

The `CampaignGenerator` is a Narrative Intelligence capability that transforms Chronicle-owned Campaign creation context into a structured Campaign proposal.

Conceptually:

```text
CampaignGenerationRequest
        ↓
Campaign Generator
        ↓
CampaignGenerationResponse
        ↓
Chronicle Validation
        ↓
Accepted Campaign Structure
```

The response is untrusted until Chronicle validates and applies it.

## 4. Generator Responsibilities

The Campaign Generator MAY propose:

- Campaign title;
- public premise;
- internal premise;
- tone;
- themes;
- dramatic thesis;
- central conflict;
- Player Character hook;
- initial Narrative Plan;
- planned Acts;
- planned Scenes;
- persistent NPCs;
- initial Relationships;
- Secrets;
- mysteries;
- clues;
- possible outcomes;
- failure directions;
- initial objectives;
- generation warnings.

## 5. Generator Prohibitions

The Campaign Generator MUST NOT:

- persist Campaign data;
- assign persistent identifiers;
- create a second Player Character;
- redefine Chronicle hierarchy;
- invent unsupported Rule Set mechanics;
- generate authoritative Dice Roll results;
- treat planned events as completed history;
- expose hidden plan content in public fields;
- bypass Character validation;
- assume access to proprietary rule content not supplied;
- create cross-Campaign references;
- finalize a Campaign as playable without Chronicle validation;
- depend on provider conversation memory as authority.

## 6. Operation Profile

The primary operation profile is:

```text
GenerateCampaign
```

Future profiles MAY include:

```text
RegenerateCampaignSection
GenerateNpcSet
GenerateInitialPlan
RepairCampaignProposal
```

The MVP SHOULD expose one complete generation profile and bounded repair.

## 7. Campaign Generation Request

A `CampaignGenerationRequest` SHOULD contain:

```text
CampaignGenerationRequest
├── OperationId
├── CorrelationId
├── ContractVersion
├── Locale
├── CampaignDraftReference
├── CampaignVersion
├── RuleSetReference
├── PlayerCharacterSnapshot
├── CampaignPreferences
├── GenerationConstraints
├── ContentBoundaries
├── RuleSetGenerationGuidance
├── ExistingDraftContent
├── OutputConstraints
└── VisibilityConstraints
```

## 8. Operation Identity

The request MUST include:

- OperationId;
- Campaign identifier;
- Campaign version;
- contract version.

The response MUST preserve them.

## 9. Campaign Draft Reference

The Campaign draft reference SHOULD include:

- Campaign identifier;
- optional player-provided title;
- status;
- locale;
- Rule Set identity and version;
- creation timestamp;
- existing draft revision.

The full Campaign aggregate is not required.

## 10. Player Character Snapshot

The Player Character is a central generation input.

The snapshot SHOULD include:

- Character identifier;
- role;
- public identity;
- Character Sheet fields relevant to Campaign generation;
- Narrative Profile;
- personality;
- key personal history events;
- goals;
- fears;
- values;
- unresolved personal conflicts;
- relevant Rule Set identity;
- validation status;
- visible customization;
- content restrictions.

## 11. Player Character Relevance

The generated Campaign MUST connect materially to the Player Character.

The proposal SHOULD reference the Character through:

- Campaign hook;
- central conflict;
- at least one NPC Relationship;
- at least one personal stake;
- at least one possible internal or external consequence;
- planned Scene relevance.

A generic Campaign that could ignore the Player Character SHOULD be rejected or repaired.

## 12. Character Sheet Boundary

The Campaign Generator MAY use Character Sheet values to shape:

- challenges;
- NPC contrast;
- social context;
- thematic pressure;
- Rule Set-compatible opportunities.

It MUST NOT:

- alter the Player Character Sheet;
- invent missing mechanical values;
- award progression;
- assume invalid fields are accepted.

## 13. Campaign Preferences

Campaign Preferences SHOULD include structured, validated player choices.

Possible fields include:

- desired tone;
- desired themes;
- campaign length guidance;
- horror intensity;
- political emphasis;
- mystery emphasis;
- combat frequency;
- social focus;
- personal drama focus;
- setting preference;
- approved house-rule options;
- content boundaries;
- language;
- prose preferences.

## 14. Preference Authority

The Campaign Generator MUST follow validated Campaign Preferences.

It MAY report conflicts.

It MUST NOT silently ignore or reinterpret a preference that materially affects generation.

## 15. Rule Set Reference

The Rule Set reference SHOULD include:

- Rule Set identifier;
- Rule Set version;
- edition;
- supported concepts;
- Character schema summary;
- NPC validation requirements;
- relevant terminology;
- Campaign generation constraints;
- prohibited mechanical inventions;
- power-level guidance;
- relevant source references;
- licensing-safe guidance.

## 16. Rule Set Generation Guidance

Guidance SHOULD be curated and relevant.

It MAY include:

- setting principles;
- supported character roles;
- typical conflicts;
- valid NPC requirements;
- supernatural or world constraints;
- recommended campaign structure;
- terminology;
- mechanical boundaries;
- required concepts.

The complete Rule Set SHOULD not be included by default.

## 17. Generation Constraints

Generation constraints SHOULD define:

- requested scale;
- target complexity;
- maximum NPC count;
- maximum Act count;
- maximum initial Scene count;
- level of detail;
- initial playable content requirement;
- future-plan detail limit;
- whether dynamic NPCs may be proposed later;
- required content categories;
- prohibited content categories;
- output size.

## 18. Content Boundaries

Content boundaries SHOULD distinguish:

```text
Allowed
Avoid
FadeToBlack
Forbidden
```

They MAY apply to:

- violence;
- body horror;
- abuse;
- sexuality;
- discrimination;
- harm to children;
- animal harm;
- addiction;
- specific phobias;
- other player-defined limits.

The exact product model will be defined later.

## 19. Safety of Content Boundaries

The Campaign Generator MUST NOT include forbidden content in:

- premise;
- NPC history;
- Secrets;
- planned Scenes;
- possible outcomes;
- hidden Plan content.

Hidden content is still subject to boundaries.

## 20. Existing Draft Content

The request MAY include player-approved draft content such as:

- Campaign title;
- custom premise fragment;
- required NPC concept;
- required location;
- approved theme;
- home setting;
- existing Relationship.

The Generator SHOULD preserve approved content unless it conflicts with validation.

## 21. Output Constraints

Output constraints SHOULD define:

- required sections;
- maximum counts;
- required temporary reference format;
- allowed proposal types;
- output language;
- public versus hidden fields;
- contract version;
- maximum prose length;
- maximum total payload size.

## 22. Visibility Constraints

Visibility constraints SHOULD define which fields are:

```text
PlayerVisible
PlayerHidden
PartiallyVisible
InternalOnly
```

Public Campaign fields MUST be safe to show immediately after acceptance.

## 23. Campaign Generation Response

A `CampaignGenerationResponse` SHOULD contain:

```text
CampaignGenerationResponse
├── OperationId
├── ContractVersion
├── CampaignVersion
├── CompletionStatus
├── PublicCampaignMetadata
├── InternalCampaignPremise
├── NarrativePlanProposal
├── NpcProposals
├── RelationshipProposals
├── SecretProposals
├── MysteryProposals
├── InitialObjectiveProposals
├── Warnings
└── ProviderMetadata
```

## 24. Completion Status

Canonical statuses are:

```text
Completed
Partial
Unable
Refused
Invalid
```

A proposal SHOULD be `Completed` only when all required Campaign components are present.

## 25. Public Campaign Metadata

Public metadata SHOULD include:

- proposed title;
- public premise;
- tone summary;
- visible themes;
- Rule Set display reference;
- Player Character hook summary;
- optional short description.

It MUST NOT reveal:

- hidden antagonist identity;
- future betrayals;
- Secret truth;
- planned endings;
- hidden NPC motives;
- mystery solutions.

## 26. Internal Campaign Premise

The internal premise MAY include:

- central dramatic situation;
- hidden causes;
- antagonist or opposing force;
- stakes;
- Secret truth;
- initial Campaign pressure;
- expected escalation;
- reason the Player Character matters.

It remains hidden from the player unless later revealed.

## 27. Dramatic Thesis

The proposal MAY include a dramatic thesis.

It SHOULD be concise and thematically useful.

Example:

```text
Loyalty without truth becomes another form of control.
```

The thesis guides generation and narration.

It does not force outcomes.

## 28. Tone Proposal

Tone MAY be represented as:

- structured tags;
- weighted dimensions;
- concise prose;
- hybrid form.

It SHOULD remain compatible with Campaign Preferences.

## 29. Theme Proposal

Themes SHOULD be represented with:

- stable theme key where possible;
- player-visible label;
- internal description;
- narrative importance;
- relationship to Player Character;
- related Acts or NPCs.

## 30. Central Conflict Proposal

The central conflict SHOULD contain:

- conflict identifier local to response;
- opposing forces;
- stakes;
- escalation pressure;
- Player Character relevance;
- visible layer;
- hidden layer;
- possible transformations;
- failure direction;
- completion signals.

## 31. Player Character Hook Proposal

The hook SHOULD contain:

- personal connection;
- immediate reason to act;
- long-term stake;
- related Character history;
- relevant NPC;
- related Secret or mystery;
- initial Scene connection.

The hook MUST not assign unapproved past actions to the Player Character as established truth.

## 32. Narrative Plan Proposal

The `NarrativePlanProposal` SHOULD include:

- temporary Plan key;
- premise;
- dramatic thesis;
- central conflict reference;
- planned Acts;
- near-term Scenes;
- mid-term guidance;
- possible outcomes;
- pacing guidance;
- revision triggers;
- hidden-information policy.

Chronicle assigns persistent identifiers after validation.

## 33. Planned Act Proposal

A planned Act SHOULD contain:

- temporary key;
- title;
- dramatic objective;
- purpose;
- conflict;
- expected NPC roles;
- related Secrets;
- related mysteries;
- planned Scenes;
- possible transitions;
- completion signals;
- failure direction;
- visibility;
- sequence guidance.

## 34. Act Granularity

Initial generation SHOULD provide:

- enough detail for the first Act;
- useful direction for the next Act;
- broad possibilities for later Campaign development.

It SHOULD NOT fully script every future Act.

## 35. Planned Scene Proposal

A planned Scene SHOULD contain:

- temporary key;
- parent Act key;
- title;
- purpose;
- location proposal;
- expected participant references;
- immediate objective;
- active conflict;
- required setup;
- visible information;
- hidden information;
- likely Rule Set operation categories;
- possible outcomes;
- transition guidance;
- optionality;
- sequence guidance.

## 36. Scene Proposal Rules

A planned Scene MUST NOT:

- be treated as executed history;
- contain a resolved Dice Roll;
- force the Player Character's choice;
- require a nonexistent Character;
- contradict Player Character history;
- depend on unsupported Rule Set mechanics.

## 37. Initial Playability Requirement

The proposal MUST contain enough validated content to begin play.

At minimum, Chronicle SHOULD be able to derive:

- initial Act;
- initial Scene;
- explicit participants;
- location;
- immediate objective;
- active conflict;
- Player Character hook;
- valid next interaction.

## 38. NPC Proposal

An `NpcProposal` SHOULD contain:

- temporary Character key;
- proposed name;
- role;
- power classification;
- public identity;
- hidden identity when relevant;
- Character Sheet values;
- Narrative Profile;
- goals;
- fears;
- motivations;
- visible traits;
- Secrets;
- planned relevance;
- initial location;
- persistence justification;
- visibility;
- validation notes.

## 39. NPC Persistent Identity

Only NPCs expected to matter SHOULD be proposed as persistent Characters.

Incidental background figures SHOULD remain transient.

A persistence justification SHOULD explain why the NPC requires:

- repeated appearance;
- mechanical state;
- Relationship;
- Secret ownership;
- Campaign Memory;
- plot-critical role.

## 40. NPC Character Sheet

NPC Character Sheet values MUST use:

- valid schema keys;
- valid data types;
- valid Rule Set version;
- supported values;
- declared power guidance.

Chronicle validates each NPC independently.

## 41. NPC Narrative Profile

The Narrative Profile SHOULD include:

- personality;
- speaking style;
- behavioral tendencies;
- values;
- goals;
- fears;
- contradictions;
- portrayal constraints.

It MUST not replace the Character Sheet.

## 42. NPC Role

NPC role is Narrative Plan metadata.

Possible roles include:

```text
Ally
Rival
Mentor
Antagonist
Witness
Victim
Authority
Dependent
UnknownForce
MoralCounterpoint
```

Role MUST not dictate immutable behavior.

## 43. NPC Power Classification

The proposal MAY use generic classifications such as:

```text
BelowPlayer
Comparable
AbovePlayer
Overwhelming
Situational
```

The Rule Set validates actual mechanics.

## 44. Initial Relationship Proposal

A `RelationshipProposal` SHOULD contain:

- temporary relationship key;
- source Character reference;
- target Character reference;
- structured dimensions;
- summary;
- origin reason;
- visibility;
- related Secret;
- related Plan role;
- confidence.

Directionality is mandatory.

## 45. Relationship Reference Rules

Temporary NPC keys MAY be used before Chronicle assigns identifiers.

The Player Character SHOULD be referenced through its existing persistent identifier.

A reciprocal Relationship requires a separate proposal.

## 46. Secret Proposal

A `SecretProposal` SHOULD contain:

- temporary Secret key;
- canonical truth;
- associated subject;
- initial known-by references;
- initial suspected-by references;
- player visibility;
- reveal status;
- importance;
- reveal conditions;
- partial reveal fragments;
- related Act or Scene;
- related mystery;
- content-boundary classification.

## 47. Secret Rules

A Secret MUST:

- have narrative purpose;
- have valid ownership;
- define who knows it;
- avoid public-field leakage;
- remain compatible with content boundaries;
- avoid depending on unsupported mechanics.

## 48. Mystery Proposal

A `MysteryProposal` SHOULD contain:

- temporary mystery key;
- public question;
- hidden answer;
- involved Characters;
- related Secret;
- stakes;
- clues;
- false leads when used;
- reveal constraints;
- completion conditions;
- related Acts and Scenes.

## 49. Clue Proposal

A clue SHOULD contain:

- temporary clue key;
- mystery reference;
- clue content;
- discovery location or source;
- required setup;
- discoverability;
- Rule Set operation category when relevant;
- partial knowledge produced;
- visibility;
- redundancy group.

## 50. Clue Redundancy

Important mysteries SHOULD not depend on one fragile clue.

Generation SHOULD provide multiple possible discovery paths where appropriate.

The exact rule remains a narrative quality guideline, not a strict invariant.

## 51. Possible Outcome Proposal

A possible outcome SHOULD contain:

- temporary key;
- related conflict or Act;
- outcome category;
- conditions;
- consequences;
- affected Characters;
- possible Plan direction;
- whether it is an ending or continuation;
- visibility.

Possible outcomes are not predetermined events.

## 52. Failure Direction

A failure direction SHOULD describe how Campaign play continues after meaningful failure.

It MAY include:

- changed objective;
- lost resource;
- stronger antagonist;
- displaced location;
- new obligation;
- survival phase;
- relationship consequence.

Failure SHOULD not automatically terminate the Campaign unless explicitly intended.

## 53. Initial Objective Proposal

An initial objective SHOULD contain:

- temporary objective key;
- owner;
- summary;
- visibility;
- status;
- related Act;
- related Scene;
- completion evidence guidance;
- related conflict.

## 54. Temporary References

New proposal entities MUST use response-scoped temporary keys.

Examples:

```text
npc:elder-mara
act:desert-war
scene:northern-gate
secret:stolen-fetish
mystery:silent-spirit
```

Temporary keys MUST be unique within the response.

They are not persistent identifiers.

## 55. Reference Resolution

Chronicle MUST resolve temporary references before persistence.

It SHOULD validate:

- key existence;
- type compatibility;
- no cycles where prohibited;
- no dangling references;
- correct ownership;
- correct parent-child hierarchy.

## 56. Persistent Identifier Assignment

Chronicle assigns persistent identifiers only after proposal validation.

The Generator MUST never choose storage identity.

## 57. Validation Pipeline

Chronicle MUST validate:

1. response envelope;
2. OperationId;
3. contract version;
4. Campaign version;
5. required sections;
6. temporary references;
7. hierarchy;
8. Player Character relevance;
9. Character schemas;
10. Rule Set mechanics;
11. Relationships;
12. Secrets;
13. visibility;
14. content boundaries;
15. duplication;
16. narrative coherence;
17. initial playability;
18. Change Set consistency.

## 58. Envelope Validation

Envelope validation checks:

- required fields;
- supported contract version;
- valid completion status;
- output size;
- correct Campaign reference;
- response identity.

## 59. Version Validation

The proposal MUST match:

- Campaign version;
- Player Character version;
- Rule Set version;
- Campaign Preferences version;
- generation contract version.

A changed Character or preference makes the proposal stale.

## 60. Hierarchy Validation

Chronicle MUST verify:

```text
Narrative Plan
└── Planned Acts
    └── Planned Scenes
```

Each Scene must reference exactly one valid parent Act.

Initial active candidates must be unambiguous.

## 61. Player Character Relevance Validation

Chronicle SHOULD verify that:

- the hook uses Character history or goals;
- the central conflict affects the Character;
- at least one NPC relationship matters;
- initial Scene includes the Player Character;
- the Campaign is not generic filler.

## 62. Rule Set Validation

The active Rule Set validates:

- NPC Character Sheets;
- operation categories;
- power classification;
- terminology;
- progression assumptions;
- setting-specific mechanics;
- Campaign Preferences affecting rules.

## 63. Relationship Validation

Chronicle verifies:

- valid source and target;
- directionality;
- allowed dimensions;
- valid values;
- no accidental duplicate;
- visibility;
- evidence in generation premise.

## 64. Secret Validation

Chronicle verifies:

- subject existence;
- known-by references;
- suspected-by references;
- reveal fragments;
- player visibility;
- content boundaries;
- no public-field leak.

## 65. Content Boundary Validation

Chronicle SHOULD detect direct violations in:

- premise;
- NPC histories;
- Secrets;
- Scenes;
- outcomes;
- hidden Plan content.

Provider compliance is not assumed.

## 66. Narrative Coherence Validation

Coherence validation SHOULD examine:

- premise and themes;
- Player Character hook;
- central conflict;
- Act progression;
- NPC motivations;
- Secret ownership;
- mystery and clue relationship;
- outcome plausibility;
- Rule Set fit.

Some checks MAY use a secondary Narrative Intelligence capability.

Final acceptance remains Chronicle-controlled.

## 67. Duplicate Validation

Chronicle SHOULD detect:

- duplicate NPCs;
- repeated names causing confusion;
- duplicate Secrets;
- duplicate Scene purpose;
- redundant Relationships;
- identical clues;
- overlapping Act objectives.

Not every similarity is an error.

## 68. Initial Playability Validation

The proposal MUST provide a valid initial play path.

Chronicle MUST identify:

- initial Act candidate;
- initial Scene candidate;
- valid participants;
- valid objective;
- location;
- conflict;
- no unresolved structural dependency.

## 69. Whole Proposal Acceptance

Campaign generation SHOULD normally be accepted as one coherent proposal.

Partial acceptance is risky because relationships between Plan, NPCs, Secrets, and Scenes are dense.

The default SHOULD be:

```text
Accept Complete Valid Proposal
```

or:

```text
Repair or Regenerate
```

## 70. Limited Partial Acceptance

Limited partial acceptance MAY be allowed for independent optional content.

Examples:

- omit one optional NPC;
- omit one optional Scene;
- omit a noncritical descriptive field.

It MUST NOT create dangling references or an unplayable Campaign.

## 71. Deterministic Repair

Chronicle MAY repair:

- enum casing;
- duplicate temporary-key formatting;
- safe aliases;
- missing metadata derived from request;
- ordering;
- harmless text normalization.

It MUST NOT invent Campaign meaning.

## 72. Provider-Assisted Repair

Repair MAY be requested for:

- invalid temporary references;
- invalid Character fields;
- public summary leaking hidden content;
- missing initial Scene detail;
- inconsistent NPC motivation;
- invalid Relationship values;
- content-boundary violation;
- unsupported Rule Set mechanics.

The repaired proposal is fully revalidated.

## 73. Section Regeneration

Chronicle MAY regenerate a bounded section when dependencies are clear.

Examples:

```text
Regenerate one invalid NPC
Regenerate initial Scene
Regenerate public premise
Regenerate invalid Secret set
```

Section regeneration MUST preserve:

- accepted Player Character;
- Campaign Preferences;
- valid existing proposal sections;
- temporary reference stability or explicit remapping.

## 74. Full Regeneration

Full regeneration is preferred when:

- premise is incoherent;
- Player Character hook is missing;
- hierarchy is broadly invalid;
- hidden information leaks across public fields;
- Rule Set fit is poor;
- content boundaries are violated broadly;
- reference repair would be unsafe.

## 75. Repair Limits

Repair and regeneration attempts MUST be bounded.

Chronicle MUST avoid unbounded paid or local compute work.

After limits are reached, generation fails recoverably.

## 76. Stale Proposal

A proposal is stale when:

- Player Character changed;
- Character validation status changed;
- Campaign Preferences changed;
- Rule Set version changed;
- Campaign draft version changed;
- another proposal was accepted;
- content boundaries changed.

Stale proposals MUST NOT be persisted.

## 77. Retry Semantics

Retrying the same generation intent uses the same logical OperationId.

Chronicle determines whether:

- no proposal exists;
- a proposal exists and is awaiting validation;
- repair is pending;
- a proposal is accepted;
- the operation failed terminally.

## 78. Duplicate Response

Duplicate provider responses MUST NOT create duplicate:

- NPCs;
- Relationships;
- Secrets;
- Acts;
- Scenes;
- objectives;
- Campaigns.

Operation identity and temporary-key mapping protect application.

## 79. Failure After Commit

If the Campaign was accepted but the UI did not receive confirmation:

- retry returns the accepted Campaign result;
- the Generator is not invoked again;
- persistent entities are not duplicated.

## 80. Provider Refusal

If the provider refuses:

- Campaign remains `Draft`;
- the operation records refusal;
- Chronicle may retry, switch provider, or request changed input;
- no partial proposal becomes playable.

## 81. Unable Response

The Generator SHOULD return `Unable` when:

- Rule Set guidance is insufficient;
- Character data is invalid;
- constraints conflict;
- content boundaries make the requested premise impossible;
- output size cannot satisfy requirements safely.

It SHOULD include machine-readable reasons.

## 82. Warning

Warnings MAY include:

- weak Player Character hook;
- high Campaign complexity;
- insufficient clue redundancy;
- possible content-boundary ambiguity;
- Rule Set uncertainty;
- too many persistent NPCs;
- overly rigid Act structure;
- high context cost.

Warnings are advisory unless policy marks them blocking.

## 83. Context Budget

The Campaign Generator may receive more context than the Narrator.

It MUST still be bounded.

Priority SHOULD be:

1. Rule Set identity and constraints;
2. Player Character;
3. Campaign Preferences;
4. content boundaries;
5. required existing draft content;
6. relevant setting guidance;
7. output contract;
8. optional inspiration.

## 84. Rule Knowledge Use

Generation SHOULD use only relevant Rule Set knowledge.

It MUST NOT receive or reproduce an entire proprietary sourcebook by default.

Rule excerpts should be minimal and traceable.

## 85. Public and Hidden Content Separation

The response MUST structurally separate:

- public Campaign information;
- hidden Plan information;
- NPC private data;
- Secret truth;
- mystery solution;
- possible future outcomes.

This separation MUST not rely only on prose instructions.

## 86. Campaign Title

The proposed title SHOULD:

- fit tone and themes;
- avoid revealing hidden plot truth;
- avoid infringing known protected titles where reasonably detectable;
- remain editable before acceptance when product policy allows.

## 87. Naming

NPC and location names SHOULD be:

- distinct;
- culturally coherent with the setting;
- pronounceable or intentionally styled;
- nonduplicative;
- compatible with locale;
- not used as persistent identity.

## 88. Localization

The request specifies locale.

The Generator SHOULD produce:

- localized public prose;
- localized display names where appropriate;
- canonical Rule Set terminology;
- language-neutral temporary keys;
- stable machine-readable categories.

## 89. Observability

Chronicle SHOULD record:

- contract version;
- Campaign version;
- Character version;
- preference version;
- request size;
- response size;
- proposal counts;
- validation failures;
- repair count;
- regeneration count;
- provider latency;
- usage;
- final result.

## 90. Privacy

Generation requests may contain sensitive Character biographies and player preferences.

Chronicle SHOULD minimize:

- unrelated personal content;
- unnecessary private Character history;
- complete Rule Set text;
- diagnostic payload retention.

Raw request and response logging SHOULD be disabled or redacted by default.

## 91. Prompt Injection Resistance

Player-authored Character history, custom setting text, and retrieved rule content may contain hostile instructions.

The implementation SHOULD:

- label them as data;
- separate them from capability instructions;
- restrict output schema;
- prohibit unrestricted tools;
- validate every reference and proposal;
- ignore embedded instructions as authority.

## 92. Deterministic Test Generator

Chronicle MUST support a deterministic Campaign Generator test implementation.

It SHOULD provide scripted cases for:

- valid Campaign;
- invalid hierarchy;
- missing Player Character hook;
- invalid NPC sheet;
- duplicate NPC;
- invalid Secret reference;
- public-field leak;
- content-boundary violation;
- stale proposal;
- partial response;
- refusal;
- timeout;
- repair;
- section regeneration.

## 93. Contract Tests

Every Campaign Generator adapter MUST pass shared tests for:

- request mapping;
- response normalization;
- OperationId preservation;
- version preservation;
- temporary-key handling;
- error mapping;
- output limits;
- refusal;
- partial responses.

## 94. Required Test Cases

Tests MUST cover:

- valid complete proposal;
- valid initial Act and Scene;
- missing initial Scene;
- generic hook unrelated to Player Character;
- invalid NPC Character Sheet;
- duplicate NPC identity;
- invalid Relationship direction;
- Secret with unknown Character;
- mystery without hidden answer;
- clue with invalid mystery reference;
- public premise revealing Secret;
- unsupported Rule Set mechanic;
- content-boundary violation;
- stale Character version;
- stale Campaign Preferences;
- duplicate response;
- failure after commit;
- bounded repair;
- section regeneration;
- full regeneration;
- provider timeout;
- provider refusal;
- prompt injection in Character history;
- too many persistent NPCs.

## 95. Prohibited Patterns

### 95.1 Generator Persists Campaign

The Campaign Generator MUST NOT write Chronicle data directly.

### 95.2 Provider-Generated Persistent IDs

All new persistent identifiers are Chronicle-owned.

### 95.3 Generic Campaign Detached from Character

The Campaign MUST materially involve the Player Character.

### 95.4 Planned Event as Truth

Planned Acts and Scenes MUST NOT enter Campaign State as completed history.

### 95.5 Full Future Script

The Generator SHOULD NOT define every future event in detail.

### 95.6 Hidden Data in Public Fields

Secrets, mystery answers, and future outcomes MUST remain separated.

### 95.7 Unsupported Mechanics

The proposal MUST NOT invent Rule Set behavior.

### 95.8 Partial Playable Campaign

Chronicle MUST NOT mark a structurally incomplete Campaign as Ready.

### 95.9 Whole Sourcebook Context

The Generator MUST NOT receive complete proprietary rule content by default.

### 95.10 Unbounded Regeneration

Repair and regeneration MUST have limits.

## 96. Current Delivery Decision

The MVP adopts:

- provider-neutral Campaign Generator port;
- one complete generation profile;
- Player Character-centered generation;
- structured Campaign proposal;
- public and hidden premise separation;
- initial Narrative Plan;
- planned Acts and Scenes;
- persistent NPC proposals;
- initial Relationships;
- Secrets;
- mysteries and clues;
- possible outcomes and failure directions;
- temporary response-scoped references;
- Chronicle-owned identifiers;
- whole-proposal validation by default;
- bounded repair and regeneration;
- deterministic test Generator;
- no multimedia generation;
- no multiple competing proposals requirement;
- no imported published adventure workflow.

## 97. Architecture Horizon

Future evolution MAY include:

- multiple proposal variants;
- collaborative player review;
- Campaign templates;
- community content;
- imported adventures;
- iterative world-building;
- map generation;
- image generation;
- soundtrack planning;
- multi-provider proposal comparison;
- local generation;
- shared multiplayer Campaign creation.

The MVP MUST NOT implement these capabilities without a later milestone.

## 98. Open Questions

The following remain open:

- What exact serialized schema will generation use?
- How many Acts should initial generation produce?
- How many Scenes should be detailed initially?
- How many persistent NPCs are appropriate for MVP?
- Should the player approve public premise before acceptance?
- Should generation happen in one provider call or staged calls?
- Which content boundaries are required in the first product?
- Should clue redundancy be validated formally?
- Should NPC generation be regenerated independently?
- How should temporary keys be normalized?
- Which warnings block acceptance?
- Should the Generator propose Character Sheet values for all NPCs?
- How much Rule Set content is required?
- Should possible outcomes be mandatory?
- Should Campaign title be generated, player-provided, or both?
- How should plan quality be evaluated automatically?

These questions require RFC-0024, RFC-0025, Rule Set package RFCs, official application RFCs, and technology ADRs.

## 99. Compliance Checklist

A Campaign Generator implementation complies when:

- the request is provider-neutral;
- Player Character data is authoritative and central;
- Campaign Preferences are explicit;
- Rule Set version is explicit;
- content boundaries apply to hidden and visible content;
- the response is structured and versioned;
- public and hidden fields are separated;
- new entities use temporary references;
- Chronicle assigns persistent identifiers;
- planned content remains possibility, not truth;
- NPCs pass Rule Set validation;
- Relationships remain directional;
- Secrets define knowledge boundaries;
- stale proposals are rejected;
- accepted Campaigns are initially playable;
- retries do not duplicate entities;
- repair and regeneration are bounded.

## 100. Final Principle

The Campaign Generator may imagine the beginning, the pressures, the people, and the possibilities.

Chronicle accepts only a structure that the Player Character can truly enter, the Rule Set can support, and future choice can still transform.
