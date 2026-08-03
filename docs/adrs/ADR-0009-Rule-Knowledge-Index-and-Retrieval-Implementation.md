---
id: ADR-0009
title: Rule Knowledge Index and Retrieval Implementation
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
  - ADR-0004
  - ADR-0005
  - ADR-0007
  - ADR-0008
  - RFC-0014
  - RFC-0024
  - RFC-0029
  - RFC-0035
  - RFC-0036
  - RFC-0040
  - RFC-0042
---

> **"Rule Knowledge helps Chronicle find the right rule. It does not become the rule, and it does not become Campaign truth."**

# Rule Knowledge Index and Retrieval Implementation

## 1. Status

**Proposed**

This ADR selects Chronicle's initial Rule Knowledge indexing and retrieval implementation.

The decision is:

- use a local Rule Knowledge index;
- use SQLite-backed lexical full-text search for the MVP;
- prefer SQLite FTS5 when available through the selected runtime distribution;
- combine lexical search with exact-key retrieval and structured metadata filters;
- require explicit source provenance and Rule Set version binding;
- return bounded, cited excerpts;
- keep Rule Knowledge separate from Campaign Memories, Character Knowledge, and provider memory;
- defer vector embeddings and semantic retrieval until measured evidence justifies them;
- prohibit automatic ingestion of unauthorized proprietary sourcebooks into distributed artifacts.

The decision becomes **Accepted** after a retrieval spike proves:

- source ingestion;
- normalization;
- chunking;
- provenance preservation;
- exact-key lookup;
- lexical retrieval;
- version filtering;
- citation generation;
- deterministic ranking;
- index rebuild;
- source removal;
- restricted-content transmission controls;
- acceptable quality for the official MVP Rule Set.

## 2. Context

Chronicle needs Rule Knowledge to help the Narrative Intelligence layer and user-facing rule references retrieve relevant information about:

- Character creation;
- Dice pools;
- difficulty;
- modifiers;
- consequences;
- progression;
- Preferences;
- terminology;
- operation requirements;
- edge cases.

Rule Knowledge is not:

- Campaign state;
- Character Knowledge;
- a Memory;
- a Rule Set operation implementation;
- provider-owned context;
- a substitute for deterministic mechanical code.

RFC-0029 defines the Rule Knowledge retrieval architecture.

ADR-0001 selected a local SQLite lexical index for the MVP and deferred vector retrieval.

This ADR specifies the concrete indexing and retrieval approach.

## 3. Decision Drivers

The implementation prioritizes:

1. local-first operation;
2. deterministic retrieval;
3. low deployment complexity;
4. exact Rule Set version filtering;
5. source provenance;
6. citation support;
7. bounded context;
8. privacy;
9. legal distribution safety;
10. straightforward testing;
11. rebuildability;
12. future semantic-search extensibility.

## 4. Decision Summary

Chronicle will use:

```text
Primary Storage
    SQLite

Full-Text Search
    SQLite FTS5 when available

Retrieval Modes
    Exact Rule Key
    Lexical Full-Text Search
    Structured Metadata Filter
    Curated Cross-Reference Expansion

Ranking
    Deterministic weighted score
    lexical relevance
    exact-key bonus
    source priority
    Rule Set version match
    document section priority

Source Model
    explicit provenance
    license or distribution classification
    Rule Set identity and version
    source document identity
    section identity
    content hash
    ingestion version

Result Model
    bounded excerpts
    citation metadata
    relevance score
    transmission classification
    no raw unrestricted source dump

Semantic Embeddings
    Deferred
```

## 5. Rule Knowledge Boundary

Rule Knowledge belongs to a dedicated module.

It MUST remain separate from:

```text
Campaign Memories
Character Knowledge
Narrative Plan
Session Transcript
Provider Conversation State
Rule Set Mechanical Implementation
```

## 6. Authority Model

Rule Knowledge is advisory textual knowledge.

The authority order remains:

```text
Chronicle Domain
    owns authoritative Campaign state

Rule Set Package
    owns deterministic mechanics

Rule Knowledge
    helps explain and locate rules

Narrative Intelligence
    interprets retrieved context and proposes output
```

Rule Knowledge text MUST NOT override a deterministic Rule Set operation.

## 7. Local-First Index

The MVP index is stored locally.

The index MUST be usable without:

- remote vector service;
- cloud database;
- provider-owned file search;
- provider conversation memory;
- external search engine.

## 8. SQLite FTS5

### Decision

Use SQLite FTS5 when the selected SQLite distribution supports it reliably.

### Rationale

FTS5 provides:

- local deployment;
- lexical full-text search;
- deterministic query behavior;
- phrase search;
- ranking support;
- compact operational footprint;
- compatibility with the existing SQLite dependency;
- straightforward backup or rebuild strategy.

## 9. FTS5 Availability

The implementation spike MUST verify that the packaged SQLite runtime includes FTS5.

If FTS5 is unavailable in the selected runtime:

- bundle an approved SQLite build with FTS5;
- or use a tested equivalent local lexical index behind the same port.

The fallback must be documented through an ADR amendment.

## 10. Index Location

The Rule Knowledge index MAY live:

```text
in the primary Chronicle SQLite database
```

or:

```text
in a separate Chronicle-managed SQLite index database
```

### Initial Recommendation

Use a separate managed index database if this materially simplifies:

- rebuild;
- deletion;
- corruption isolation;
- package-specific index replacement;
- backup exclusion.

The implementation spike will determine the final physical placement.

## 11. Authoritative Versus Rebuildable

The Rule Knowledge index is derived and rebuildable.

Authoritative source metadata and user-selected source references MUST be preserved separately.

The index itself MAY be excluded from backups when rebuild inputs are available.

## 12. Source Categories

Chronicle supports Rule Knowledge sources such as:

```text
Official Package Summary
Open-License Rule Reference
User-Supplied Local Document
User-Authored Notes
Approved Structured Rule Catalog
Generated Original Summary
```

## 13. Source Classification

Every source MUST declare:

```text
SourceKind
DistributionClass
TransmissionClass
RuleSetPackageId
RuleSetVersionRange
Locale
SourceIdentity
SourceVersion
```

## 14. Distribution Classes

Recommended classes:

```text
Redistributable
LocallyOwned
Restricted
ReferenceOnly
Unknown
```

## 15. Transmission Classes

Recommended classes:

```text
RemoteTransmissionAllowed
RemoteTransmissionWithUserApproval
LocalOnly
Blocked
```

## 16. Legal Boundary

Chronicle MUST NOT distribute copyrighted proprietary sourcebook text without authorization.

The official repository and release artifacts SHOULD use:

- original summaries;
- open-license references;
- package-authored descriptions;
- user-supplied local sources;
- references to legally obtained material.

## 17. User-Supplied Sources

A user MAY register local Rule Knowledge sources.

Chronicle MUST:

- record the source path or imported identity safely;
- avoid copying restricted source text into exports by default;
- preserve provenance;
- allow source removal;
- respect transmission policy;
- avoid logging content.

## 18. Imported Versus Referenced Sources

Chronicle distinguishes:

```text
Referenced Source
    original file remains external

Imported Source
    Chronicle stores a managed local copy where policy permits
```

The MVP SHOULD prefer referenced sources unless reliable portability requires import and the distribution policy allows it.

## 19. Source Identity

A source identity SHOULD include:

- stable source ID;
- content hash;
- source version;
- Rule Set binding;
- locale;
- ingestion configuration version.

## 20. Content Hash

Content hashes MAY be used for:

- duplicate detection;
- index invalidation;
- source change detection;
- rebuild validation.

Hashes MUST not be used to imply distribution rights.

## 21. Source Normalization

Ingestion SHOULD normalize:

- Unicode;
- line endings;
- whitespace;
- heading structure;
- section boundaries;
- stable keys;
- page or location references;
- locale metadata.

Normalization MUST preserve citation traceability.

## 22. Unsupported Source Types

The MVP MAY support only a limited source set.

Recommended first supported inputs:

```text
Markdown
Plain Text
Structured JSON Rule Catalog
Package-Embedded Original Summaries
```

PDF ingestion is optional and requires separate parsing and legal review.

## 23. No OCR Requirement

The MVP does not require OCR.

Scanned sourcebooks are outside the initial ingestion scope.

## 24. Structured Rule Catalog

Rule Set packages SHOULD be able to provide a structured catalog.

A catalog entry MAY contain:

```text
RuleKey
Title
Summary
FullExplanation
Tags
OperationKeys
CharacterFieldKeys
PreferenceKeys
CrossReferences
SourceCitation
TransmissionClass
```

## 25. Exact Rule Keys

Stable Rule Keys SHOULD be the preferred retrieval path when the Application already knows the relevant operation.

Examples:

```text
werewolf.test.basic
werewolf.damage.soak
werewolf.rage.check
werewolf.progression.experience
```

Exact-key retrieval outranks free-text search.

## 26. Chunk Model

Each indexable unit is a Rule Knowledge chunk.

A chunk SHOULD contain:

```text
ChunkId
SourceId
RuleSetPackageId
RuleSetVersion
RuleKey
SectionKey
Title
Content
Locale
Tags
OperationKeys
CharacterFieldKeys
PreferenceKeys
CitationLocation
DistributionClass
TransmissionClass
ContentHash
IngestionVersion
```

## 27. Chunk Size

Chunks SHOULD be small enough for focused retrieval and large enough to preserve meaning.

Exact size limits require empirical evaluation.

The chunker SHOULD prefer semantic boundaries:

- heading;
- subsection;
- rule entry;
- example;
- exception;
- table row group.

## 28. Chunk Overlap

Overlap MAY be used sparingly when necessary to preserve context.

Duplicate overlap must not dominate ranking or context budget.

## 29. Tables

Structured tables SHOULD be represented as:

- normalized records;
- compact textual form;
- or both.

The representation must preserve citation and deterministic interpretation.

## 30. Examples

Rule examples SHOULD be separately tagged from normative rule statements.

The provider must not confuse illustrative examples with authority.

## 31. Normative Classification

Chunks MAY declare:

```text
Normative
Explanatory
Example
Glossary
DesignerNote
Errata
HouseRule
```

Ranking and prompt presentation SHOULD prefer normative content for mechanics.

## 32. Index Schema

Recommended metadata tables:

```text
RuleKnowledgeSources
RuleKnowledgeDocuments
RuleKnowledgeChunks
RuleKnowledgeTags
RuleKnowledgeCrossReferences
RuleKnowledgeIndexVersions
RuleKnowledgeIngestionRuns
```

FTS virtual table:

```text
RuleKnowledgeChunksFts
```

## 33. FTS Columns

The FTS index SHOULD include weighted searchable fields such as:

```text
Title
RuleKey
Content
Tags
OperationKeys
CharacterFieldKeys
PreferenceKeys
```

## 34. External Content Table

The implementation MAY use an FTS external-content table linked to normalized chunk metadata.

This can reduce duplication and improve update behavior.

The exact schema requires the spike.

## 35. Tokenization

Initial tokenization SHOULD use a Unicode-capable tokenizer.

Locale-specific stemming is deferred unless retrieval quality requires it.

## 36. Case and Diacritics

Search SHOULD be case-insensitive.

Diacritic handling must be tested for:

- English;
- Portuguese;
- package locales.

Exact Rule Keys remain invariant and language-neutral.

## 37. Query Model

A Rule Knowledge query SHOULD contain:

```text
RuleSetPackageId
RuleSetVersion
Locale
QueryText
ExactRuleKeys
OperationKeys
CharacterFieldKeys
PreferenceKeys
RequiredTags
ExcludedTags
NormativePreference
MaximumResults
MaximumTotalCharacters or tokens
TransmissionPolicy
```

## 38. Query Ownership

The Application or Prompt Construction layer builds the query.

The provider does not perform unrestricted search.

## 39. Retrieval Modes

### Exact Retrieval

Used when stable keys are known.

### Lexical Retrieval

Used for natural-language and terminology searches.

### Metadata Retrieval

Used for operation, field, Preference, locale, and version filters.

### Cross-Reference Expansion

Used to retrieve explicitly connected rules.

## 40. Retrieval Pipeline

Recommended pipeline:

```text
Validate Query
    ↓
Resolve Exact Keys
    ↓
Apply Rule Set and Version Filter
    ↓
Run Lexical Search
    ↓
Expand Approved Cross-References
    ↓
Deduplicate
    ↓
Rank Deterministically
    ↓
Apply Transmission Policy
    ↓
Apply Result Count and Context Budget
    ↓
Return Cited Results
```

## 41. Deterministic Ranking

Ranking MUST be deterministic for the same:

- index version;
- query;
- metadata;
- configuration.

## 42. Ranking Factors

Potential factors:

```text
Exact Rule Key match
Exact Operation Key match
Exact title match
Lexical relevance score
Normative classification
Rule Set version match
Source priority
Locale match
Curated package priority
Cross-reference distance
```

## 43. Ranking Tie-Breaking

Ties MUST use stable keys such as:

- score;
- source priority;
- Rule Key;
- ChunkId.

Database row order is not a valid tie-breaker.

## 44. Source Priority

Package-authored curated summaries MAY outrank user notes for deterministic mechanical requests.

User notes MAY outrank general sources when the query explicitly requests local notes.

Priority is policy, not hidden behavior.

## 45. Rule Set Version Filtering

Every retrieval MUST bind to an exact Rule Set version or an explicitly compatible range.

A Campaign using an older version MUST not silently receive newer incompatible rules.

## 46. Version Compatibility

A package MAY declare that a Rule Knowledge source applies to:

```text
ExactVersion
CompatibleRange
AllVersions
```

Compatibility must be explicit.

## 47. Locale Selection

The retrieval layer SHOULD:

1. prefer requested locale;
2. fall back to package default;
3. optionally fall back to English;
4. report fallback metadata.

Locale fallback must not change machine keys.

## 48. Citation Model

Every result MUST include a citation object.

Recommended fields:

```text
SourceId
SourceTitle
SourceKind
RuleKey
SectionKey
LocationLabel
PageNumber when available
ContentHash
RuleSetVersion
Locale
```

## 49. Citation Display

The UI and prompt may display a compact citation.

Examples:

```text
[Rule: werewolf.test.basic]
[Source: Official package summary, section 4.2]
[Local source: page 127]
```

## 50. Citation Safety

Citations MUST avoid leaking:

- full private paths;
- user account names;
- secret filenames when classified private.

A safe display name or source alias should be used.

## 51. Result Model

A result SHOULD contain:

```text
ChunkId
RuleKey
Title
Excerpt
Citation
Score
NormativeClass
TransmissionClass
RuleSetVersion
Truncated
```

## 52. Excerpt Bounds

Results MUST be bounded by:

- maximum characters or tokens per chunk;
- maximum results;
- maximum total budget;
- source transmission policy.

## 53. Excerpt Integrity

Truncation SHOULD preserve coherent boundaries.

The result must indicate when content is truncated.

## 54. No Full Source Dump

Retrieval MUST NOT return entire sourcebooks or large documents by default.

## 55. Prompt Integration

Prompt Construction receives only selected results.

It SHOULD include:

- exact Rule Set version;
- source citation;
- normative classification;
- bounded excerpt;
- instruction that deterministic Rule Set operations remain authoritative.

## 56. Provider Transmission

Before sending Rule Knowledge to a remote provider, Chronicle MUST enforce `TransmissionClass`.

Local-only sources remain local.

## 57. Local Provider

A local provider MAY receive broader locally permitted context, still subject to context budget and user policy.

## 58. User Approval

Sources classified `RemoteTransmissionWithUserApproval` require explicit configuration or per-operation approval according to the privacy policy.

## 59. Restricted Content

Restricted content MAY be indexed locally when legally possessed and configured by the user.

It MUST NOT be:

- shipped in the repository;
- included in default packages without authorization;
- sent remotely contrary to policy;
- copied into portable exports by default;
- included in diagnostic bundles.

## 60. Rule Knowledge and Rule Set Mechanics

When retrieved text conflicts with deterministic operation code:

- Chronicle uses the deterministic operation for resolution;
- the conflict is logged safely;
- the UI or diagnostics may report package inconsistency;
- the package requires correction.

## 61. Rule Knowledge and House Rules

Package-declared House Rules and Preferences SHOULD have explicit Rule Keys and metadata.

User-authored free-form notes do not become executable mechanics.

## 62. Index Build

Indexing is a durable background operation.

It SHOULD use:

- Work Item;
- OperationId;
- source hash;
- index version;
- checkpoints;
- bounded batches;
- safe retry.

## 63. Index Build Stages

Recommended stages:

```text
Inspect Source
Normalize
Parse
Chunk
Validate Metadata
Write Staging Records
Build FTS Entries
Validate Counts and Hashes
Publish Index Version
```

## 64. Staging

New or rebuilt indexes SHOULD be staged before publication.

A failed build must not corrupt the active index.

## 65. Index Version

The index MUST record:

```text
IndexSchemaVersion
ChunkingVersion
TokenizerVersion
RankingPolicyVersion
SourceSetHash
BuiltAtUtc
```

## 66. Rebuild

The index MUST be fully rebuildable from registered sources and package catalogs.

## 67. Incremental Update

Incremental update MAY be supported when:

- one source changes;
- one package updates;
- one locale is added.

Correctness is more important than avoiding a full rebuild.

## 68. Source Change Detection

Chronicle SHOULD compare:

- source existence;
- modification metadata;
- content hash;
- ingestion version.

Modification time alone is insufficient.

## 69. Source Removal

Removing a source SHOULD:

- remove its chunks from the next published index;
- preserve safe source-history metadata where needed;
- not delete Campaign history;
- report if active package behavior expected that source.

## 70. Missing Source

If a referenced local source becomes unavailable:

- existing index content MAY remain temporarily according to policy;
- the source is marked stale or unavailable;
- rebuild cannot claim completeness;
- transmission policy remains enforced.

The exact stale-index retention policy requires implementation review.

## 71. Package Update

A package update MAY include new or changed Rule Knowledge catalogs.

The package and index versions must remain explicit.

Campaigns using older package versions retain compatible indexed content.

## 72. Index Garbage Collection

Old index versions MAY be removed when:

- no active query uses them;
- no Campaign needs their package version;
- rebuild inputs remain available;
- retention policy permits.

## 73. Read Concurrency

Queries SHOULD continue using the active published index while a new index is built.

Publication should switch atomically.

## 74. Write Concurrency

Only one ingestion operation should mutate a given index target at a time.

## 75. Failure Recovery

After interruption:

- completed staging work may resume;
- incomplete staging remains nonauthoritative;
- the prior index remains active;
- retry uses the same OperationId where appropriate.

## 76. Corruption

If the index is corrupt:

- mark it unavailable;
- preserve authoritative sources;
- rebuild;
- allow Campaign access without Rule Knowledge where safe;
- block operations that require unavailable mandatory knowledge only if no deterministic fallback exists.

## 77. Degraded Mode

When Rule Knowledge is unavailable:

- deterministic Rule Set mechanics still operate;
- narration may continue with reduced rule explanations if policy permits;
- provider prompts omit unavailable sources;
- the UI reports degraded rule-reference support.

## 78. Search Query Safety

User and provider-derived query text is untrusted.

The query builder MUST:

- escape FTS syntax or use safe parameterization;
- enforce length limits;
- reject control characters;
- prevent SQL injection;
- bound wildcard or expansion behavior.

## 79. FTS Query Language

Chronicle SHOULD use an internal query builder rather than expose raw FTS query syntax to normal Application callers.

## 80. Stop Words

Stop-word policy is deferred until measured.

Aggressive stop-word removal may damage rules terminology.

## 81. Synonyms

Curated package synonyms MAY improve retrieval.

Examples:

```text
test → roll, check
health → damage track
rage → resource
```

Synonyms must be:

- versioned;
- package-owned;
- deterministic;
- testable.

## 82. Query Expansion

Automatic LLM-generated query expansion is not required for MVP.

If used later, it remains advisory and bounded.

## 83. Curated Cross-References

Rule packages MAY declare explicit cross-references.

These are preferred over unconstrained inferred links.

## 84. Result Deduplication

Duplicate or overlapping chunks SHOULD be merged or reduced.

Deduplication SHOULD use:

- ChunkId;
- content hash;
- source identity;
- overlap similarity where deterministic.

## 85. Context Budget

The retrieval layer MUST respect a caller-provided budget.

It should return the best set of results within:

- result count;
- total characters or tokens;
- required exact-key results;
- source diversity policy.

## 86. Required Results

Exact Rule Keys marked required MUST either:

- be returned;
- or produce an explicit missing-knowledge error.

They must not be silently omitted due to ranking.

## 87. Search Explanation

For diagnostics, a retrieval result MAY expose a safe explanation:

```text
ExactKeyMatch
OperationKeyMatch
LexicalMatch
CrossReference
LocaleFallback
```

It must not expose internal private content.

## 88. UI Search

The MVP MAY provide a user-facing Rule Reference search.

It SHOULD use the same retrieval service with a user-safe result projection.

## 89. User-Facing Rule Reference

A user-facing result SHOULD show:

- title;
- excerpt;
- citation;
- Rule Set version;
- source type;
- whether the content is official summary, local source, or user note.

## 90. Rule Knowledge Editing

The MVP does not require a rich Rule Knowledge editor.

User notes MAY be added through simple supported source files or a minimal notes workflow.

## 91. Ingestion Security

Ingestion MUST protect against:

- oversized files;
- decompression bombs;
- path traversal;
- malicious JSON;
- excessive nesting;
- unsupported encodings;
- invalid Unicode;
- hostile markup;
- infinite parser behavior.

## 92. File Access

Rule Knowledge file access occurs through approved filesystem abstractions.

Providers do not read local files directly.

## 93. Logging

Logs MAY include:

- SourceId;
- SourceKind;
- file type;
- byte size;
- chunk count;
- index version;
- duration;
- safe error code;
- source path hash.

Logs MUST NOT include:

- source content;
- full user paths;
- proprietary excerpts;
- search results;
- query text when it may contain private data.

## 94. Observability

Useful metrics MAY include:

```text
IndexBuildDuration
IndexBuildFailureCount
ChunkCount
SourceCount
QueryDuration
ResultCount
ExactKeyHitRate
MissingRequiredRuleCount
LocaleFallbackCount
RestrictedTransmissionBlockCount
```

## 95. Privacy

Rule Knowledge query logs SHOULD avoid storing free-form query text by default.

Safe query categories or hashes MAY be used for diagnostics when justified.

## 96. Backup

The index MAY be excluded from ordinary backup if fully rebuildable.

Backups MUST preserve:

- source registry;
- source metadata;
- package catalogs;
- user notes stored by Chronicle;
- index configuration.

## 97. Restore

After restore:

- validate source registry;
- detect missing local sources;
- restore package catalogs;
- rebuild index;
- report unresolved sources.

## 98. Export

Portable Campaign exports SHOULD NOT include Rule Knowledge sources by default.

They MAY include:

- package identity;
- Rule Set version;
- source references;
- required dependency metadata;
- user-authored notes when explicitly selected and allowed.

## 99. Testing Strategy

The implementation requires:

```text
Unit Tests
Index Integration Tests
Retrieval Contract Tests
Security Tests
Legal Boundary Tests
Performance Tests
Evaluation Tests
Migration Tests
```

## 100. Unit Tests

Unit tests SHOULD cover:

- source classification;
- normalization;
- chunking;
- Rule Key validation;
- citation mapping;
- query escaping;
- deterministic ranking;
- tie-breaking;
- budget selection;
- transmission filtering.

## 101. SQLite Integration Tests

Integration tests SHOULD use real SQLite with FTS enabled.

They MUST cover:

- index creation;
- insertion;
- update;
- deletion;
- exact lookup;
- lexical search;
- phrase search;
- metadata filter;
- version filter;
- locale filter;
- concurrent active index and staging index.

## 102. Retrieval Contract Tests

A common retrieval contract SHOULD prove:

- same query yields deterministic ordering;
- required exact keys are honored;
- incompatible versions are excluded;
- restricted results are filtered;
- citations are present;
- result limits are respected;
- missing required knowledge is explicit.

## 103. Security Tests

Tests MUST attempt:

- SQL injection;
- FTS syntax injection;
- oversized query;
- oversized source;
- path traversal;
- malicious JSON;
- unsupported encoding;
- source-content leakage into logs;
- restricted-content remote transmission;
- diagnostic-bundle inclusion.

## 104. Legal Boundary Tests

Public repository and Release builds MUST be scanned to ensure:

- no unauthorized sourcebook text;
- no private local source copies;
- no proprietary fixture excerpts;
- only approved summaries and references are included.

## 105. Retrieval Evaluation

The official Rule Set SHOULD maintain a question-and-expected-source evaluation set.

Each case contains:

```text
Query
Rule Set version
Required Rule Keys
Relevant Sources
Forbidden Sources
Expected Citation
Maximum Result Count
```

## 106. Quality Metrics

Useful retrieval metrics include:

```text
RequiredKeyRecall
TopKRelevantRecall
CitationAccuracy
VersionAccuracy
RestrictedContentLeakage
ContextBudgetCompliance
Determinism
```

## 107. MVP Quality Gate

The Rule Knowledge implementation passes MVP acceptance when:

- all required exact Rule Keys resolve;
- version-incompatible content is never returned;
- restricted content is never transmitted against policy;
- citations are accurate;
- the canonical gameplay scenarios retrieve the required rules;
- lexical retrieval quality is sufficient for the official Rule Set's evaluation set;
- index rebuild is reliable;
- no live embedding service is required.

## 108. Required Test Cases

Tests MUST cover:

- valid package catalog ingestion;
- duplicate source;
- changed source hash;
- missing local source;
- source removal;
- index rebuild;
- interrupted build;
- staging publication;
- exact Rule Key;
- unknown Rule Key;
- lexical search;
- phrase search;
- synonym expansion;
- cross-reference expansion;
- deterministic tie;
- version mismatch;
- locale fallback;
- normative preference;
- result truncation;
- total-budget enforcement;
- required-result preservation;
- transmission block;
- user approval requirement;
- corrupted index;
- degraded mode;
- backup and restore rebuild;
- FTS unavailability;
- unauthorized content scan.

## 109. Architecture Tests

Architecture tests MUST reject:

- Rule Knowledge code importing Campaign Memory persistence directly;
- provider adapters reading source files;
- Narrative Intelligence bypassing the retrieval port;
- Rule Set mechanical code depending on FTS results;
- UI issuing raw SQL or FTS queries;
- source content logging;
- embeddings or remote vector clients in MVP production registration without a later ADR.

## 110. Prohibited Patterns

### 110.1 Rule Knowledge as Mechanical Authority

Text retrieval never replaces deterministic Rule Set operations.

### 110.2 Provider-Side File Search

Providers do not own source ingestion or search.

### 110.3 Entire Sourcebook in Prompt

Only bounded relevant excerpts are included.

### 110.4 Versionless Retrieval

Every result is version-bound or explicitly version-independent.

### 110.5 Citation-Free Excerpt

Every result requires provenance.

### 110.6 Generic Unowned Text Dump

Every chunk has source, classification, and contract ownership.

### 110.7 Raw FTS Syntax From Untrusted Callers

Queries use a safe internal builder.

### 110.8 Automatic Remote Transmission of Local Sources

Transmission policy is explicit.

### 110.9 Index as Backup Authority

The index is rebuildable derived data.

### 110.10 Premature Embedding Infrastructure

Semantic retrieval is introduced only after evaluation proves lexical retrieval insufficient.

## 111. Alternatives Considered

### Remote Vector Database

Rejected for MVP because it introduces:

- external dependency;
- privacy concerns;
- credential management;
- network availability;
- cost;
- synchronization;
- operational complexity.

### Local Vector Database

Deferred because it still requires:

- embedding generation;
- model selection;
- index versioning;
- additional native dependencies;
- evaluation evidence.

### Provider File Search

Rejected because it would delegate source storage, indexing, retrieval, and retention to the provider.

### In-Memory Search

Rejected because the index must persist, scale, rebuild, and support citations.

### Plain SQL `LIKE`

Insufficient for practical lexical ranking and phrase retrieval.

### Embedded Search Engine Such as Lucene

Potentially powerful, but adds a larger independent indexing stack before SQLite FTS5 is proven insufficient.

## 112. Consequences

### Positive

- local and offline retrieval;
- low deployment complexity;
- deterministic ranking;
- exact version filtering;
- strong provenance;
- no remote vector dependency;
- simple rebuild and testing;
- alignment with existing SQLite stack.

### Negative

- lexical search may miss semantic matches;
- tokenizer behavior may vary by language;
- FTS ranking requires tuning;
- source ingestion remains a nontrivial subsystem;
- PDFs and scans are not automatically supported;
- exact legal source policy requires ongoing care.

## 113. Risks

### Insufficient Semantic Recall

Mitigation:

- exact Rule Keys;
- package tags;
- synonyms;
- curated cross-references;
- evaluation set;
- later hybrid semantic retrieval ADR.

### Unauthorized Content Distribution

Mitigation:

- source classification;
- repository scans;
- local-only source support;
- transmission policy;
- no source content in diagnostics.

### Version Mixing

Mitigation:

- mandatory package and Rule Set version filters;
- compatibility metadata;
- evaluation fixtures.

### Index Corruption

Mitigation:

- rebuildability;
- staging;
- atomic publication;
- source hashes;
- degraded mode.

### Query Injection

Mitigation:

- safe query builder;
- parameterized SQL;
- query-length limits;
- adversarial tests.

## 114. Technology Spike

Before acceptance, implement:

1. structured package catalog;
2. Markdown source ingestion;
3. source classification;
4. normalization;
5. semantic-boundary chunking;
6. SQLite FTS5 schema;
7. exact Rule Key lookup;
8. lexical search;
9. version filtering;
10. locale filtering;
11. citation generation;
12. deterministic ranking;
13. restricted transmission filtering;
14. staged rebuild;
15. canonical Rule Set evaluation set.

## 115. Spike Acceptance

The spike passes when:

- required rules for the canonical Roll workflow are retrieved;
- every result includes a valid citation;
- incompatible Rule Set versions are excluded;
- restricted local content is blocked from remote transmission;
- the same query returns stable ordering;
- a failed rebuild leaves the previous index active;
- the index can be deleted and rebuilt from registered sources;
- no provider API is required for indexing;
- repository fixtures contain no unauthorized source text;
- retrieval latency is acceptable for interactive prompt construction.

## 116. Definition of Compliance

An implementation complies when:

- Rule Knowledge uses a local lexical index;
- SQLite FTS5 is the preferred MVP engine;
- exact Rule Keys and metadata filters are supported;
- all chunks carry provenance and version;
- ranking is deterministic;
- results are bounded and cited;
- transmission policy is enforced before provider calls;
- Rule Knowledge remains separate from mechanics and Campaign memory;
- indexes are rebuildable;
- restricted content is excluded from public artifacts;
- vector embeddings remain deferred until a later decision.

## 117. Review Triggers

This ADR must be reviewed if:

- lexical retrieval fails the official evaluation threshold;
- a second Rule Set requires stronger semantic matching;
- local embedding infrastructure becomes necessary;
- FTS5 is unavailable or unstable in the packaged runtime;
- multilingual retrieval quality is inadequate;
- user-supplied PDF ingestion becomes a priority;
- cloud synchronization includes Rule Knowledge sources;
- package marketplace distribution changes legal requirements;
- retrieval performance degrades for large source libraries.

## 118. Deferred Decisions

Later ADRs MAY define:

- exact physical index database location;
- final SQLite FTS schema;
- tokenizer configuration;
- chunk-size policy;
- ranking weights;
- synonym format;
- PDF parsing;
- local embedding model;
- hybrid lexical and vector retrieval;
- vector storage;
- source encryption;
- Rule Knowledge editor;
- source portability.

## 119. Final Decision

Chronicle will implement Rule Knowledge retrieval as a local, deterministic, version-aware, citation-preserving service backed initially by SQLite FTS5.

It will combine exact keys, lexical relevance, structured filters, and curated cross-references.

Semantic vector retrieval remains deferred until real evaluation proves it necessary.

Chronicle does not need to remember every page.

It needs to find the right rule, show where it came from, and still let the Rule Set decide what it means.
