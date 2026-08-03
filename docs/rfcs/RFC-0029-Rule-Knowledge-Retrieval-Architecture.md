---
id: RFC-0029
title: Rule Knowledge Retrieval Architecture
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
  - RFC-0024
  - RFC-0025
  - RFC-0026
  - RFC-0027
  - RFC-0028
---

> **"Rule knowledge may explain the system. Executable Rule Set logic still decides what the system does."**

# Rule Knowledge Retrieval Architecture

## Abstract

This RFC defines Chronicle's Rule Knowledge Retrieval architecture.

It establishes how Rule Set-specific explanatory knowledge is registered, indexed, queried, filtered, cited, versioned, and assembled for Narrative Intelligence and player-facing rule explanations.

Rule knowledge is reference material.

It is not executable mechanics, Campaign truth, provider memory, or an unrestricted sourcebook mirror.

Chronicle MUST keep deterministic Rule Set logic authoritative while allowing relevant, minimal, licensing-safe rule knowledge to support narration, generation, finalization, diagnostics, and user guidance.

## 1. Purpose

Narrative Intelligence needs access to relevant system knowledge.

Examples include:

- what a Test represents;
- which terminology is canonical;
- what situations normally trigger a mechanic;
- how consequences should be described;
- what Character fields mean;
- what campaign themes fit the Rule Set;
- how progression is explained;
- which Rule Set operation key applies.

Sending an entire rulebook is unsafe, expensive, legally risky, and architecturally unnecessary.

Chronicle therefore requires bounded retrieval.

## 2. Scope

This RFC defines:

- Rule Knowledge identity;
- source registration;
- content provenance;
- source versions;
- indexing;
- chunking;
- metadata;
- topic keys;
- operation keys;
- retrieval requests;
- retrieval results;
- filters;
- ranking;
- query construction;
- context limits;
- citations;
- licensing boundaries;
- user-supplied knowledge;
- bundled summaries;
- stale index handling;
- validation;
- security;
- observability;
- testing.

This RFC does not define:

- one vector database;
- one embedding provider;
- one tokenizer;
- one search engine;
- one sourcebook ingestion workflow;
- OCR implementation;
- exact first Rule Set corpus;
- exact legal interpretation;
- online web retrieval;
- provider-side retrieval tools.

## 3. Core Principle

Rule Knowledge Retrieval answers:

```text
What explanatory material is relevant?
```

Executable Rule Set logic answers:

```text
What mechanic is valid and what result applies?
```

A retrieved passage MUST NOT override executable mechanics.

## 4. Rule Knowledge Definition

`RuleKnowledge` is structured reference content associated with one Rule Set identity and version.

It MAY include:

- original Chronicle summaries;
- open-licensed material;
- public-domain content;
- user-supplied legally obtained material;
- licensed excerpts;
- terminology;
- operation explanations;
- generation guidance;
- Character field descriptions;
- examples.

## 5. Rule Knowledge Prohibitions

Rule Knowledge MUST NOT become:

- executable mechanics;
- Campaign State;
- Character Knowledge;
- provider conversation memory;
- a persistence mutation path;
- an unrestricted copy of proprietary books;
- an excuse to ignore Rule Set validators.

## 6. Knowledge Source

A `KnowledgeSource` represents one registered body of rule reference material.

It SHOULD contain:

- source identifier;
- Rule Set identity;
- applicable Rule Set versions;
- source type;
- title;
- edition;
- language;
- provenance;
- license metadata;
- content hash;
- status;
- index version;
- access policy;
- citation policy.

## 7. Source Types

Canonical source types MAY include:

```text
BundledOriginalSummary
OpenLicensedDocument
PublicDomainDocument
UserSuppliedDocument
LicensedExcerpt
ExternalReference
GeneratedIndex
```

The source type affects distribution and retention policy.

## 8. Source Identity

Source identity MUST be stable.

It MUST not depend only on:

- file path;
- display title;
- provider upload identifier;
- temporary index identifier.

A source SHOULD preserve identity across index rebuilds.

## 9. Source Version

A source version identifies the exact content indexed.

It SHOULD include:

- source identifier;
- content version;
- content hash;
- applicable Rule Set version;
- index schema version.

A changed source requires a new content version or hash.

## 10. Provenance

Every source MUST declare provenance.

Chronicle SHOULD distinguish:

- authored by Chronicle;
- supplied by user;
- licensed by project;
- open licensed;
- public domain;
- externally referenced.

Unknown provenance SHOULD block distribution and MAY block indexing.

## 11. Licensing Metadata

Licensing metadata SHOULD contain:

- license type;
- license identifier;
- attribution requirement;
- redistribution permission;
- derivative-work permission;
- provider-transmission permission;
- retention restriction;
- excerpt restriction;
- trademark notice;
- review status.

## 12. Distribution Boundary

The open-source Chronicle repository MUST NOT include restricted rulebook text merely because the retrieval system can index it.

Code, schemas, original summaries, source references, and restricted source content SHOULD remain separable.

## 13. User-Supplied Sources

Chronicle MAY allow users to register their own legally obtained sources.

A user-supplied source SHOULD be:

- local by default;
- explicitly selected;
- indexed locally when possible;
- marked nonredistributable;
- excluded from diagnostics and shared fixtures;
- protected by local storage policy.

## 14. Provider Transmission Policy

A source MUST declare whether retrieved content may be sent to a remote Narrative Intelligence provider.

Possible policies:

```text
LocalOnly
RemoteAllowed
RemoteAllowedWithRedaction
NotForNarrativeUse
```

Chronicle MUST enforce the policy before prompt construction.

## 15. Knowledge Entry

A `KnowledgeEntry` is the normalized retrievable unit.

It SHOULD contain:

- entry identifier;
- source identifier;
- source version;
- Rule Set identity;
- Rule Set version range;
- language;
- title;
- topic keys;
- operation keys;
- Character field keys;
- capability tags;
- body;
- concise summary;
- citation location;
- visibility;
- legal policy;
- index metadata.

## 16. Entry Identity

Entry identity SHOULD be stable across index rebuilds when source segmentation remains semantically equivalent.

It MAY derive from:

- source identity;
- logical section;
- stable heading path;
- normalized position.

It MUST not become Campaign identity.

## 17. Topic Keys

Topic keys provide stable semantic retrieval labels.

Examples:

```text
tests.general
dice.pool
dice.difficulty
character.health
character.progression
narration.failure
campaign.theme.rage
```

Topic keys MUST be namespaced and language-neutral.

## 18. Operation Keys

Knowledge entries MAY reference Rule Set operation keys.

Example:

```text
werewolf.operation.frenzy_resist
```

This supports precise retrieval from a structured mechanic request.

## 19. Character Field Keys

Entries MAY reference Character Sheet field keys.

Examples:

```text
attributes.strength
resources.willpower
```

This supports explanations and context assembly for relevant mechanics.

## 20. Capability Tags

Entries SHOULD identify applicable Narrative Intelligence capabilities.

Examples:

```text
Narrator
CampaignGenerator
Archivist
PlanReviser
PlayerRuleExplanation
```

An entry may support several capabilities.

## 21. Retrieval Request

A `RuleKnowledgeQuery` SHOULD contain:

```text
RuleKnowledgeQuery
├── QueryId
├── RuleSetId
├── RuleSetVersion
├── Capability
├── OperationProfile
├── Locale
├── TopicKeys
├── OperationKeys
├── CharacterFieldKeys
├── NaturalLanguageQuery
├── ContextTerms
├── SourcePolicy
├── ResultLimit
├── SizeBudget
└── CitationRequired
```

Not every field is required.

## 22. Structured Query Preference

Chronicle SHOULD prefer structured keys over natural-language-only search.

Priority SHOULD generally be:

1. exact operation key;
2. exact topic key;
3. exact Character field key;
4. Rule Set version;
5. capability;
6. natural-language query;
7. contextual terms.

## 23. Natural-Language Query

Natural-language query MAY improve recall.

It MUST remain bounded and clearly treated as search input.

It MUST NOT become provider instructions.

## 24. Query Construction Ownership

The Application or Chronicle Director decides which rule topics are relevant.

The retrieval infrastructure decides how to search those topics.

The Prompt Builder only represents selected results.

## 25. Retrieval Result

A `RuleKnowledgeResult` SHOULD contain:

- query identifier;
- Rule Set identity;
- index version;
- entries;
- scores;
- match reasons;
- citation metadata;
- omission metadata;
- warnings;
- retrieval duration.

## 26. Retrieved Entry Projection

The response SHOULD return a safe projection rather than unrestricted source records.

A projection MAY include:

- entry identifier;
- title;
- concise excerpt or summary;
- topic keys;
- operation keys;
- source citation;
- score;
- legal-use policy;
- source language;
- source version.

## 27. Match Reasons

Match reasons SHOULD be machine-readable.

Examples:

```text
ExactOperationKey
ExactTopicKey
CharacterFieldMatch
SemanticMatch
KeywordMatch
CapabilityMatch
VersionMatch
```

This improves diagnostics and ranking transparency.

## 28. Ranking

Ranking MAY combine:

- exact key matches;
- semantic similarity;
- keyword relevance;
- Rule Set version compatibility;
- capability relevance;
- source priority;
- locale;
- recency of source version;
- entry specificity.

Exact structured matches SHOULD outrank weak semantic matches.

## 29. Source Priority

Chronicle MAY define source priority.

Suggested priority:

1. executable Rule Set-provided original summaries;
2. verified package guidance;
3. licensed or user-configured primary material;
4. open-licensed references;
5. generated summaries;
6. weak external references.

Priority MUST not override version incompatibility.

## 30. Version Filtering

Retrieval MUST filter by compatible Rule Set version.

A passage for another edition MUST NOT be returned silently as current guidance.

Cross-version material MAY be returned only when explicitly labeled.

## 31. Locale Filtering

Chronicle SHOULD prefer entries in the requested locale.

Fallback MAY use:

- Rule Set default locale;
- another configured locale;
- translated original summary;
- no result.

Machine keys remain language-neutral.

## 32. Capability Filtering

The retrieval layer SHOULD prefer entries tagged for the requesting capability.

Example:

- Campaign generation guidance differs from interactive mechanical explanation;
- Archivist progression guidance differs from Narrator prose guidance.

## 33. Size Budget

A Rule Knowledge query MUST define a bounded result size.

The budget MAY limit:

- result count;
- total characters;
- estimated tokens;
- excerpt length;
- per-source count.

## 34. Result Diversity

Retrieval SHOULD avoid returning several near-duplicate chunks from the same section when one concise result is sufficient.

Diversity MAY consider:

- source;
- topic;
- heading;
- operation;
- semantic overlap.

## 35. Minimum Sufficient Knowledge

The objective is not maximum retrieval.

The objective is the minimum material sufficient for the current operation.

## 36. No Result

When no relevant knowledge is found, the retrieval layer MUST return an explicit empty result.

It MUST NOT fabricate a rule explanation.

The calling workflow decides whether to:

- continue with executable mechanics only;
- request another query;
- block;
- notify the player;
- use generic guidance.

## 37. Low-Confidence Result

A low-confidence result SHOULD be labeled.

It SHOULD NOT be promoted to authoritative instruction.

The Application MAY exclude it from prompt context.

## 38. Citation

Every retrieved item SHOULD preserve citation metadata.

Citation metadata MAY contain:

- source title;
- source identifier;
- edition;
- section;
- page when legally and technically available;
- heading path;
- user-visible attribution;
- content hash;
- source version.

## 39. Citation Stability

Citations SHOULD remain stable across index rebuilds where the source has not changed.

Page numbers alone are insufficient identity.

## 40. Player-Facing Rule Explanation

When Chronicle explains a rule to the player, it SHOULD distinguish:

```text
Mechanical Result
Rule Explanation
Source Citation
```

The mechanical result comes from executable Rule Set logic.

The explanation comes from retrieved knowledge.

## 41. Narrative Intelligence Context

Retrieved knowledge sent to Narrative Intelligence SHOULD be:

- concise;
- relevant;
- version-compatible;
- licensing-safe;
- source-labeled;
- marked as reference material;
- separated from instructions.

## 42. Prompt Injection Boundary

Retrieved source text is untrusted data.

The Prompt Builder MUST mark it as reference content.

Instructions found inside source text MUST remain inert.

## 43. Direct Provider Retrieval

The MVP MUST NOT let Narrative Intelligence query the knowledge index directly through unrestricted tools.

Chronicle performs retrieval first and supplies selected context.

## 44. Index Definition

A `RuleKnowledgeIndex` is a derived search structure built from registered sources.

It MAY include:

- lexical index;
- semantic vector index;
- structured key index;
- metadata filters;
- citation map.

The architecture does not require all index types.

## 45. Index Version

Every index MUST identify:

- index identifier;
- index schema version;
- source set version;
- embedding profile when applicable;
- build timestamp;
- build status;
- content hashes.

## 46. Index Build

Index build SHOULD:

1. validate sources;
2. extract structured text;
3. normalize content;
4. segment entries;
5. assign metadata;
6. build indexes;
7. validate citations;
8. publish atomically.

## 47. Atomic Publication

A new index SHOULD become active only after successful build and validation.

The previous valid index SHOULD remain available until replacement succeeds.

## 48. Build Failure

A failed build MUST NOT corrupt the active index.

Chronicle SHOULD record:

- failed source;
- stage;
- error category;
- retryability;
- prior active index.

## 49. Index Status

Canonical statuses MAY include:

```text
Building
Ready
Degraded
Failed
Obsolete
Unavailable
```

## 50. Stale Index

An index is stale when:

- source content changed;
- Rule Set version changed;
- package knowledge metadata changed;
- index schema changed;
- embedding profile changed materially.

A stale index MAY remain readable only when policy explicitly allows it.

## 51. Chunking

Chunking divides sources into retrievable entries.

Chunking SHOULD preserve:

- semantic completeness;
- heading context;
- citation location;
- topic metadata;
- operation metadata;
- version metadata.

## 52. Chunk Size

Chunks SHOULD be large enough to explain one concept and small enough for precise retrieval.

The exact size depends on source format and retrieval implementation.

## 53. Heading Context

A chunk SHOULD retain its heading path.

Example:

```text
Combat
→ Damage
→ Aggravated Damage
```

This reduces ambiguity.

## 54. Overlap

Chunk overlap MAY improve recall.

It SHOULD be bounded to avoid excessive duplication and citation confusion.

## 55. Structured Source Ingestion

Rule Set package-provided knowledge SHOULD be ingested from structured records when possible.

Structured ingestion may avoid arbitrary chunking.

## 56. Document Source Ingestion

Document ingestion MAY support:

- plain text;
- Markdown;
- HTML;
- PDF text;
- other approved formats.

Each format requires a safe parser and citation strategy.

## 57. OCR

OCR is not required for the MVP.

If later introduced, OCR output MUST be:

- marked as derived;
- validated;
- linked to source pages;
- treated as potentially inaccurate;
- subject to licensing policy.

## 58. Generated Summaries

Chronicle MAY generate concise original summaries from permitted source material.

Generated summaries MUST:

- record source references;
- remain untrusted until reviewed where required;
- preserve Rule Set version;
- avoid copying excessive protected expression;
- not replace executable mechanics.

## 59. Summary Review

Bundled Chronicle summaries SHOULD be reviewed before distribution.

The review SHOULD cover:

- mechanical accuracy;
- originality;
- licensing;
- terminology;
- version compatibility;
- citation.

## 60. Embeddings

Semantic retrieval MAY use embeddings.

Embedding infrastructure MUST remain outside Domain and Rule Set mechanics.

The index MUST record:

- embedding provider or local profile;
- embedding model version;
- vector dimensions;
- normalization version.

## 61. Remote Embedding Policy

Source content MUST NOT be sent to a remote embedding provider unless source policy permits it.

User-supplied restricted content SHOULD default to local-only processing.

## 62. Embedding Replacement

Changing embedding implementation requires index rebuild.

It MUST NOT require Campaign migration.

## 63. Lexical Retrieval

Chronicle SHOULD support lexical or structured-key retrieval even if semantic search is used.

This provides:

- exact operation lookup;
- deterministic tests;
- fallback;
- provider independence;
- explainability.

## 64. Hybrid Retrieval

A hybrid retriever MAY combine:

- exact structured matches;
- keyword ranking;
- semantic ranking;
- metadata filtering.

The exact algorithm requires an ADR.

## 65. Reranking

A reranker MAY improve result quality.

It MUST:

- remain bounded;
- preserve source metadata;
- obey licensing policy;
- not fabricate passages;
- be versioned;
- be observable.

## 66. Query Expansion

Query expansion MAY add:

- synonyms;
- canonical terminology;
- operation aliases;
- Character field aliases;
- localized equivalents.

Expansion MUST use versioned package metadata.

## 67. Query Expansion Safety

Query expansion MUST NOT broaden the query into unrelated editions or systems without explicit labeling.

## 68. Retrieval Cache

A retrieval cache MAY store results for identical semantic requests.

Cache identity SHOULD include:

- Rule Set version;
- index version;
- structured keys;
- normalized query;
- locale;
- capability;
- source policy;
- result budget.

## 69. Cache Invalidation

Cache MUST invalidate when:

- index changes;
- source policy changes;
- Rule Set version changes;
- locale assets change;
- query contract changes.

## 70. Campaign Isolation

Rule Knowledge is Rule Set-scoped, not Campaign-owned.

However, user-supplied source configuration MAY be user- or installation-scoped.

Campaign-private content MUST not be mixed into the Rule Knowledge index.

## 71. Campaign Memory Separation

Campaign Memories and Rule Knowledge MUST use separate retrieval paths.

```text
Campaign Memory Retrieval:
What happened or matters in this Campaign?

Rule Knowledge Retrieval:
What does the Rule Set explain?
```

They MAY be combined only during context assembly.

## 72. Character Knowledge Separation

Character Knowledge records what a Character knows or believes.

Rule Knowledge records explanatory system material.

The two concepts MUST never share persistence identity or retrieval semantics.

## 73. Validation

Retrieved entries SHOULD be validated before use.

Validation checks:

- source exists;
- source version matches index metadata;
- Rule Set version is compatible;
- legal-use policy permits intended use;
- citation exists;
- entry body is within size limits;
- source status is allowed;
- no corruption detected.

## 74. Retrieval Policy

A `RuleKnowledgePolicy` SHOULD define:

- allowed source types;
- allowed transmission;
- maximum results;
- maximum size;
- locale fallback;
- minimum score;
- citation requirement;
- source diversity;
- stale-index behavior.

## 75. Capability-Specific Policies

Different capabilities MAY use different policies.

Example:

### Narrator

- concise;
- immediate operation guidance;
- minimal excerpts.

### Campaign Generator

- broader thematic and setting guidance;
- larger budget.

### Archivist

- progression and finalization guidance;
- evidence-oriented summaries.

## 76. Rule Explanation Policy

Player-facing rule explanations SHOULD require:

- exact Rule Set version;
- citation;
- executable result context;
- concise language;
- no hidden Campaign information.

## 77. Error Model

Recommended errors include:

```text
KnowledgeSourceMissing
KnowledgeSourceInvalid
KnowledgeSourceRestricted
IndexUnavailable
IndexStale
IndexBuildFailed
QueryInvalid
NoRelevantKnowledge
CitationUnavailable
RemoteTransmissionForbidden
RetrievalBudgetExceeded
```

Errors follow RFC-0018.

## 78. Retry

Retrieval MAY retry when:

- index store is temporarily unavailable;
- local file access fails transiently;
- background index build is incomplete.

It SHOULD NOT repeatedly retry:

- invalid license policy;
- missing required source;
- incompatible Rule Set version;
- forbidden remote transmission.

## 79. Background Work

Index building and rebuilding MAY use RFC-0019 background operations.

Index retrieval for an active turn SHOULD be interactive and bounded.

## 80. Startup Behavior

On startup, Chronicle SHOULD detect:

- missing index;
- stale index;
- failed prior build;
- changed source;
- unavailable user-supplied source;
- incompatible index schema.

## 81. Degraded Mode

Chronicle MAY run in degraded mode when:

- deterministic mechanics remain available;
- optional explanatory knowledge is missing;
- narration does not require unavailable guidance.

The UI SHOULD communicate the limitation.

## 82. Blocking Mode

Play SHOULD block only when required Rule Knowledge is essential and no safe deterministic fallback exists.

Examples may include:

- Campaign generation requiring unavailable setting guidance;
- unsupported package configuration;
- missing user-supplied source explicitly required by package policy.

## 83. Security

Knowledge ingestion and retrieval MUST defend against:

- malicious files;
- oversized documents;
- recursive archives;
- parser exploits;
- embedded scripts;
- prompt injection;
- unsafe links;
- path traversal;
- cross-user source leakage;
- unrestricted provider transmission.

## 84. File Handling

Source file processing SHOULD:

- use allowlisted formats;
- limit size;
- avoid executing macros;
- avoid following embedded external links automatically;
- store only required metadata;
- preserve source identity.

## 85. Logging

Logs SHOULD record:

- query identifier;
- Rule Set identity;
- index version;
- source identifiers;
- result count;
- match reasons;
- retrieval duration;
- cache status;
- warning codes.

Logs SHOULD NOT include unrestricted source text.

## 86. Observability

Chronicle SHOULD measure:

- index build duration;
- source count;
- entry count;
- failed source count;
- query latency;
- empty-result rate;
- low-confidence rate;
- exact-key hit rate;
- semantic-only hit rate;
- citation failures;
- stale-index incidents;
- prompt budget contribution.

## 87. Evaluation

Rule Knowledge Retrieval SHOULD be evaluated using versioned queries with expected:

- source;
- topic;
- operation key;
- citation;
- prohibited result;
- maximum result size.

## 88. Golden Retrieval Fixtures

A golden fixture SHOULD contain:

```text
Given:
    Rule Set version
    operation key
    topic keys
    locale
    source policy

Expect:
    required source or entry
    forbidden sources
    citation present
    size within budget
```

## 89. Adversarial Retrieval Fixtures

Adversarial tests SHOULD include:

- instructions embedded in source text;
- wrong-edition passage with high lexical similarity;
- restricted source under remote provider mode;
- duplicate chunks;
- invalid citation;
- malicious file metadata;
- empty source;
- oversized source.

## 90. Required Test Cases

Tests MUST cover:

- exact operation-key retrieval;
- topic-key retrieval;
- Character-field retrieval;
- natural-language fallback;
- wrong Rule Set version excluded;
- locale fallback;
- duplicate chunk suppression;
- result size budget;
- no-result response;
- low-confidence result;
- citation stability;
- source hash change;
- stale index;
- failed rebuild preserving old index;
- user-supplied local-only source;
- remote transmission forbidden;
- prompt injection in source text;
- source with unknown provenance;
- cross-system query;
- cache invalidation;
- lexical fallback without embeddings;
- Campaign Memory separation;
- Character Knowledge separation.

## 91. Prohibited Patterns

### 91.1 Entire Sourcebook in Every Prompt

Chronicle MUST retrieve only relevant bounded material.

### 91.2 Retrieved Text Overrides Mechanics

Executable Rule Set logic remains authoritative.

### 91.3 Provider Queries Index Directly

The MVP MUST not expose unrestricted retrieval tools to providers.

### 91.4 Restricted Content Bundled Without Permission

Architecture capability does not imply distribution rights.

### 91.5 One Shared Index Across Incompatible Versions

Rule Set versions MUST remain filterable and explicit.

### 91.6 Campaign Memories in Rule Index

Campaign history MUST use separate retrieval.

### 91.7 Character Knowledge in Rule Index

Character belief and knowledge MUST remain a Domain concept.

### 91.8 Source Text as Instructions

Retrieved content is untrusted reference data.

### 91.9 Silent Stale Index Use

Staleness MUST be detected and reported.

### 91.10 Semantic Search as Only Retrieval Path

Exact structured and lexical lookup SHOULD remain available.

## 92. Current Delivery Decision

The MVP adopts:

- one Rule Set-scoped knowledge architecture;
- stable source and entry identity;
- source provenance and license metadata;
- user-supplied local sources as an architectural capability;
- original bundled summaries where legally safe;
- structured topic, operation, and Character field keys;
- exact and lexical retrieval;
- optional semantic retrieval;
- version and locale filtering;
- bounded result budgets;
- citations;
- atomic index publication;
- background index building;
- source transmission policy;
- no provider-direct retrieval tools;
- no full sourcebook prompt injection;
- no mixing with Campaign Memories or Character Knowledge.

## 93. Architecture Horizon

Future evolution MAY include:

- pluggable vector stores;
- local embedding models;
- remote embedding providers;
- user-managed libraries;
- source preview and citation UI;
- licensed content integrations;
- package marketplace knowledge assets;
- multilingual indexes;
- semantic reranking;
- automatic query expansion;
- external documentation connectors;
- rule explanation chat mode.

The MVP MUST NOT implement these capabilities without a later milestone.

## 94. Open Questions

The following remain open:

- What exact index technology will the MVP use?
- Is semantic retrieval required for the first Rule Set?
- Which source formats are required initially?
- How will users configure local sourcebooks?
- What original summaries must the initial package bundle?
- What citation format will be exposed in the UI?
- How will PDF page references be preserved?
- Should indexing be automatic after source registration?
- What maximum source size is supported?
- How will locale fallback interact with provider output language?
- Should remote embedding ever be enabled by default?
- Which retrieval policies belong to the Rule Set package?
- How should wrong-edition passages be ranked or excluded?
- How much retrieved text may enter a Narrator prompt?
- What legal review process is required before distributing summaries?

These questions require technology ADRs, privacy RFCs, source-management RFCs, and the initial Rule Set implementation.

## 95. Compliance Checklist

An implementation complies when:

- Rule Knowledge remains separate from executable mechanics;
- sources declare identity, version, provenance, and license metadata;
- Rule Set version filtering is explicit;
- structured keys are supported;
- natural-language search is secondary to precise metadata where available;
- results are bounded;
- citations are preserved;
- remote transmission policy is enforced;
- stale indexes are detected;
- failed rebuilds preserve the previous valid index;
- retrieved content is treated as untrusted data;
- Campaign Memories remain separate;
- Character Knowledge remains separate;
- providers do not access the index directly;
- proprietary content is not distributed without authorization.

## 96. Final Principle

Rule Knowledge should help Chronicle explain, contextualize, and narrate the system.

It must never become a second, less reliable version of the system itself.
