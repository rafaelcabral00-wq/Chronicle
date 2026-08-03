---
id: RFC-0040
title: Testing Strategy and Quality Gates
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-08-03
category: Quality
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
---

> **"Chronicle may improvise stories. Its architecture, mechanics, persistence, and recovery must never improvise correctness."**

# Testing Strategy and Quality Gates

## Abstract

This RFC defines Chronicle's testing strategy and quality gates.

It establishes:

- testing layers;
- deterministic fixtures;
- Domain tests;
- Application tests;
- persistence tests;
- Rule Set contract tests;
- Character schema tests;
- Narrative Intelligence adapter tests;
- structured-output tests;
- prompt and context tests;
- security tests;
- migration tests;
- backup and restore tests;
- UI tests;
- accessibility tests;
- performance tests;
- resilience tests;
- release gates;
- regression policy;
- test-data policy;
- CI expectations.

Chronicle contains nondeterministic components, especially Narrative Intelligence.

The testing architecture MUST isolate nondeterminism and validate the deterministic boundaries around it.

A provider response is never considered correct merely because it is plausible.

## 1. Purpose

Chronicle combines:

- long-lived Campaign state;
- complex Domain invariants;
- Rule Set mechanics;
- random Dice Rolls;
- provider-generated narrative;
- structured provider output;
- persistence;
- migrations;
- background work;
- recovery;
- hidden information;
- local desktop interaction.

A defect may cause:

- repeated Dice Rolls;
- duplicated progression;
- broken Session finalization;
- Secret leakage;
- cross-Campaign corruption;
- invalid Character Sheets;
- lost Memories;
- stale provider output accepted;
- unrecoverable migration;
- malformed backup;
- inaccessible UI;
- history reinterpreted under a new Rule Set version.

Testing must prove more than isolated function correctness.

It must prove that Chronicle preserves truth across boundaries and failures.

## 2. Scope

This RFC defines:

- quality principles;
- test categories;
- test pyramid;
- deterministic test infrastructure;
- fixtures;
- fake providers;
- fake clocks;
- fake random generators;
- Domain testing;
- Application testing;
- integration testing;
- contract testing;
- persistence testing;
- migration testing;
- security testing;
- UI testing;
- performance testing;
- release quality gates;
- regression policy;
- test-data policy;
- CI expectations;
- flaky-test policy.

This RFC does not define:

- one testing framework;
- one CI provider;
- one code-coverage tool;
- one programming language;
- one load-testing product;
- exact release cadence.

## 3. Core Principle

Chronicle SHOULD test each boundary at the lowest layer that can prove the behavior.

```text
Pure invariant
    → Domain test

Use-case behavior
    → Application test

Adapter compliance
    → Contract or integration test

Persistence guarantee
    → Real storage integration test

End-user workflow
    → UI or end-to-end test

Provider quality
    → Evaluation fixture and bounded live-provider test
```

## 4. Quality Attributes

The test strategy protects:

```text
Correctness
Determinism
Idempotency
Recoverability
Compatibility
Security
Privacy
Accessibility
Performance
Observability
Portability
```

## 5. Testing Layers

Chronicle SHOULD maintain:

```text
Unit Tests
Domain Tests
Application Tests
Contract Tests
Integration Tests
Migration Tests
UI Component Tests
UI Interaction Tests
End-to-End Tests
Security Tests
Performance Tests
Evaluation Tests
Manual Exploratory Tests
```

## 6. Unit Tests

Unit tests cover small deterministic units.

Examples:

- value-object validation;
- key normalization;
- visibility classification;
- modifier stacking;
- context scoring;
- retry classification;
- migration mapping;
- redaction.

Unit tests SHOULD be fast and isolated.

## 7. Domain Tests

Domain tests prove invariants and lifecycle transitions.

They SHOULD avoid infrastructure and provider dependencies.

Examples:

- one Player Character per Campaign;
- one active Session;
- Scene belongs to one Act;
- Memory aging once per finalized Session;
- raw Dice values immutable;
- progression balance cannot become invalid;
- finalization cannot apply twice.

## 8. Application Tests

Application tests prove use-case orchestration.

They SHOULD use:

- fake repositories or in-memory adapters where appropriate;
- deterministic Rule Set implementations;
- scripted providers;
- fake clocks;
- deterministic random sources;
- operation records.

Examples:

- process player input;
- request Roll;
- execute Roll;
- continue narration;
- finalize Session;
- apply progression;
- change Preferences;
- recover interrupted work.

## 9. Contract Tests

Contract tests prove that an implementation satisfies a stable interface.

Required contract-test suites SHOULD exist for:

```text
Narrative Intelligence Provider Adapter
Rule Set Package
Character Sheet Schema
Repository Adapter
Credential Store
Knowledge Retrieval Adapter
Backup Artifact
Portable Campaign Package
Desktop Platform Adapter
```

## 10. Integration Tests

Integration tests use real infrastructure components where behavior depends on them.

Examples:

- real database transactions;
- real migration engine;
- actual archive parser;
- actual filesystem behavior;
- operating-system credential-store adapter where CI permits;
- desktop process lock;
- child-process management.

## 11. End-to-End Tests

End-to-end tests verify critical user journeys across the official application.

They SHOULD remain few, stable, and high-value.

Examples:

- create Campaign;
- create Character;
- start Session;
- submit action;
- perform Roll;
- continue narration;
- end Session;
- finalize;
- apply advancement;
- close and reopen application;
- export and re-import Campaign.

## 12. Evaluation Tests

Evaluation tests assess Narrative Intelligence quality under RFC-0026.

They SHOULD distinguish:

```text
Deterministic Contract Evaluation
Scripted Provider Evaluation
Recorded Response Evaluation
Live Provider Evaluation
Human Review
```

Live provider evaluation is not a replacement for deterministic tests.

## 13. Test Pyramid

Chronicle SHOULD prefer:

- many fast deterministic tests;
- fewer integration tests;
- a bounded number of end-to-end tests;
- carefully controlled live-provider evaluations.

The test suite MUST NOT depend primarily on external providers.

## 14. Determinism

Tests MUST control:

- time;
- randomness;
- identifiers;
- provider responses;
- scheduling;
- locale;
- timezone;
- filesystem roots;
- configuration;
- Rule Set version.

## 15. Fake Clock

A test clock SHOULD support:

- fixed time;
- controlled advancement;
- timezone-aware values;
- deterministic timestamps;
- timeout simulation.

Production code SHOULD depend on a clock abstraction where time affects behavior.

## 16. Deterministic Random Generator

Tests MUST use an injected deterministic random source.

It SHOULD support:

- predefined sequences;
- seed-based sequences;
- failure simulation;
- boundary values;
- repeated-value scenarios.

Production randomness MUST not be used in deterministic tests.

## 17. Deterministic Identifiers

Tests SHOULD support deterministic identifiers.

This improves:

- snapshot readability;
- fixture stability;
- operation correlation;
- failure diagnosis.

## 18. Scripted Provider

A `ScriptedNarrativeProvider` SHOULD support:

- exact response scripts;
- delayed response;
- timeout;
- malformed output;
- partial output;
- refusal;
- repeated response;
- stale response;
- oversized response;
- provider exception.

## 19. Recorded Provider Response

Recorded provider responses MAY support regression tests.

They MUST:

- contain no credentials;
- contain no private user data;
- contain no restricted source text;
- declare provider and contract version;
- remain reviewable;
- be stored as test fixtures.

## 20. Live Provider Tests

Live-provider tests SHOULD be:

- optional in normal local test runs;
- isolated from core release correctness;
- credential-gated;
- bounded in cost;
- tagged;
- tolerant of approved provider variability;
- focused on contract compatibility and quality signals.

Normal CI MUST NOT call a real LLM service.

Normal tests MUST NOT require paid API access, OpenAI credentials, Ollama, an installed local model, or provider network availability.

Real-provider integration tests MUST be explicit and opt-in.

## 21. Test Fixtures

Fixtures SHOULD be:

- minimal;
- explicit;
- versioned;
- deterministic;
- privacy-safe;
- reusable;
- easy to inspect.

## 22. Canonical Campaign Fixture

Chronicle SHOULD maintain one small canonical Campaign fixture containing:

- one Campaign;
- one Player Character;
- a few NPCs;
- one Session;
- several Scenes;
- a Roll;
- Memories;
- Relationships;
- Character Knowledge;
- one Secret;
- progression;
- Preferences.

## 23. Complex Campaign Fixture

A larger fixture SHOULD test:

- long transcript;
- many Sessions;
- archived Memories;
- multiple Narrative Plan versions;
- migrations;
- unresolved operation;
- historical Rule Set versions.

## 24. Adversarial Fixture

An adversarial fixture SHOULD contain:

- prompt injection;
- malformed references;
- cross-Campaign identifiers;
- oversized fields;
- hidden Secrets;
- invalid modifiers;
- hostile archive entries;
- unknown schema fields.

## 25. Golden Fixtures

Chronicle SHOULD maintain golden fixtures for:

- Character schema compilation;
- mechanical resolution;
- progression calculation;
- Preference validation;
- prompt construction;
- context selection;
- structured output parsing;
- Session finalization;
- portable export;
- migration.

## 26. Golden Fixture Policy

Golden fixtures are appropriate when the output is intentionally stable.

They MUST NOT hide semantic changes inside large unreadable snapshots.

Every changed golden result requires review.

## 27. Snapshot Tests

Snapshot tests MAY be used for:

- structured DTOs;
- UI component states;
- manifests;
- generated safe diagnostics;
- prompt plans without private content.

They SHOULD NOT be used as a substitute for targeted assertions.

## 28. Domain Test Requirements

Domain tests MUST cover:

- Campaign creation;
- hierarchy ownership;
- Session lifecycle;
- Act and Scene transitions;
- Character role uniqueness;
- Memory lifecycle;
- Relationship directionality;
- Knowledge states;
- Secret visibility state;
- Dice Roll lifecycle;
- progression balances;
- Preference mutability;
- archive state.

## 29. Application Use-Case Tests

Every state-changing use case MUST test:

- happy path;
- invalid input;
- stale version;
- duplicate OperationId;
- conflicting fingerprint;
- failure before commit;
- failure after external call;
- failure after commit;
- retry;
- authorization or visibility where applicable.

## 30. Query Tests

Queries SHOULD test:

- purpose-specific projection;
- visibility filtering;
- empty state;
- unavailable dependencies;
- stale read models;
- deterministic ordering;
- pagination.

## 31. Dice Tests

Dice tests MUST prove:

- Chronicle owns randomness;
- raw values are persisted;
- raw values never change;
- Rule Set resolution is deterministic;
- retries do not reroll;
- consequence application is idempotent;
- hidden mechanics remain filtered;
- historical replay uses exact versions.

## 32. Finalization Tests

Finalization tests MUST prove:

- evidence collection;
- Archivist proposal validation;
- duplicate proposal suppression;
- Rule Set validation;
- Memory creation and update;
- Memory aging exactly once;
- progression Award exactly once;
- Relationship and Knowledge changes;
- Character State changes;
- all-or-nothing commit;
- retry after interruption;
- stale Session revision rejection.

## 33. Memory Tests

Memory tests SHOULD cover:

- permanent and temporary Memory;
- lifetime decrement;
- relevance;
- importance;
- remembered-by;
- expiration;
- archival;
- duplicate suppression;
- Session-origin traceability.

## 34. Character Schema Tests

Schema contract tests MUST cover:

- field identity;
- type validation;
- requiredness;
- conditional fields;
- defaults;
- derived values;
- validation rules;
- localization keys;
- unknown fields;
- migration;
- malicious schema input;
- bounded complexity.

## 35. Rule Set Contract Tests

Every Rule Set package MUST pass a standard suite covering:

- manifest;
- package identity;
- operation registration;
- deterministic validation;
- deterministic resolution;
- Character schema compatibility;
- progression;
- Preferences;
- migration;
- no I/O;
- no randomness;
- no persistence access;
- bounded execution.

## 36. Provider Adapter Tests

Every provider adapter MUST test:

- request translation;
- response translation;
- timeout;
- cancellation;
- structured-output support;
- provider refusal;
- malformed response;
- usage metadata;
- credential error;
- endpoint validation;
- no credential logging.

## 37. Structured Output Tests

Structured output tests MUST cover:

- valid response;
- unknown event type;
- missing required field;
- duplicate event;
- invalid reference;
- oversized payload;
- unsupported contract version;
- repair success;
- repair failure;
- partial valid output rejection policy;
- visibility violation.

## 38. Prompt Construction Tests

Prompt tests SHOULD verify:

- stable section order;
- trusted instructions separated from untrusted data;
- Secret filtering;
- required context;
- budget;
- omission report;
- Rule Knowledge citations;
- provider-neutral contract;
- no credentials;
- no local paths.

## 39. Context Selection Tests

Context tests SHOULD cover:

- relevant Memories;
- irrelevant Memory omission;
- Character-scoped Knowledge;
- Relationship selection;
- Scene participants;
- Rule Set version filtering;
- budget pressure;
- required-context failure;
- deterministic tie-breaking.

## 40. Persistence Tests

Persistence integration tests MUST use the real selected storage engine.

They SHOULD cover:

- transaction atomicity;
- optimistic concurrency;
- unique constraints;
- foreign-key integrity;
- Operation Record idempotency;
- Message ordering;
- raw Dice immutability;
- finalization commit;
- crash recovery assumptions;
- archive behavior.

## 41. Repository Contract Tests

All repository implementations SHOULD pass a common contract suite.

This prevents behavior differences between:

- in-memory tests;
- local database;
- future alternate storage.

## 42. Migration Tests

Every migration MUST include:

- source fixture;
- target expectation;
- forward migration;
- failure behavior;
- checkpoint behavior;
- unknown-field preservation;
- version record;
- integrity check.

## 43. Migration Chain Tests

CI SHOULD test migration from:

- immediately prior version;
- oldest supported version;
- selected intermediate versions;
- realistic larger Campaign fixture.

## 44. Backup Tests

Backup tests MUST cover:

- consistent snapshot;
- manifest;
- checksum;
- validation;
- optional encryption;
- failed backup preserving prior valid backup;
- restoration;
- missing dependency reporting.

## 45. Export and Import Tests

Portable package tests MUST cover:

- PreserveIdentity;
- CloneOnImport;
- identifier remapping;
- visibility filtering;
- restricted-content exclusion;
- package integrity;
- malformed archive;
- atomic import;
- semantic round trip.

## 46. Security Tests

Security testing MUST include:

- prompt injection;
- prompt poisoning;
- Secret leakage;
- cross-Campaign references;
- fake Dice;
- duplicate operations;
- malicious package;
- malicious schema;
- malicious import;
- archive bomb;
- path traversal;
- credential leakage;
- log leakage;
- context overflow;
- unsafe endpoint;
- restricted-content transmission.

## 47. Privacy Tests

Privacy tests SHOULD verify:

- player-safe projections;
- player-safe exports;
- logs without narrative payload;
- diagnostic bundles without Campaign content;
- provider requests with least context;
- credentials excluded everywhere;
- retention cleanup.

## 48. UI Component Tests

UI component tests SHOULD cover:

- all states;
- keyboard operation;
- accessibility labels;
- loading;
- error;
- disabled actions;
- status rendering;
- responsive behavior.

## 49. UI Interaction Tests

Critical interaction tests SHOULD cover:

- Campaign creation;
- Character creation;
- Session start;
- player input;
- provider wait;
- Roll request;
- Roll execution;
- result continuation;
- Session end;
- finalization;
- progression;
- Preference change;
- recovery.

## 50. Accessibility Tests

Accessibility tests SHOULD include:

- keyboard-only flow;
- focus order;
- focus visibility;
- screen-reader labels;
- live-region announcements;
- text scaling;
- contrast;
- reduced motion;
- no color-only meaning;
- transcript reading order.

## 51. Visual Regression Tests

Visual regression tests MAY cover stable, critical components.

Examples:

- Roll card;
- operation status;
- finalization view;
- Character form;
- critical error banner.

They MUST allow deliberate design evolution without becoming an excessive maintenance burden.

## 52. Desktop Host Tests

Host tests SHOULD cover:

- startup;
- degraded startup;
- Safe Mode;
- single-instance lock;
- stale-lock recovery;
- background worker;
- child process;
- shutdown;
- forced crash recovery;
- offline mode.

## 53. Performance Tests

Performance tests SHOULD define budgets for:

- application startup;
- Campaign load;
- transcript rendering;
- Message append;
- Dice Roll commit;
- finalization commit;
- Character validation;
- Memory query;
- context selection;
- index query;
- backup;
- export;
- import.

## 54. Performance Budgets

Budgets SHOULD be measured on reference hardware.

They MUST distinguish:

- local deterministic work;
- external provider latency;
- local-model latency;
- UI rendering.

## 55. Long-Campaign Performance

Chronicle SHOULD test a Campaign with:

- many Sessions;
- long transcript;
- many Memories;
- many Relationships;
- several Plan versions;
- archived content;
- large progression history.

## 56. Resource Tests

Resource tests SHOULD cover:

- memory growth;
- file-handle cleanup;
- database connection cleanup;
- child-process cleanup;
- log rotation;
- large import rejection;
- context-budget bounds;
- worker queue bounds.

## 57. Resilience Tests

Resilience tests SHOULD simulate:

- provider timeout;
- provider refusal;
- provider malformed response;
- storage transient failure;
- disk full;
- network loss;
- application crash;
- worker crash;
- child-process crash;
- migration interruption;
- restore failure;
- stale version;
- duplicate submission.

## 58. Fault Injection

Chronicle SHOULD provide fault-injection hooks in test environments.

Examples:

```text
FailBeforeCommit
FailAfterCommit
FailAfterRawRollPersistence
FailDuringFinalizationValidation
FailDuringBackupPublication
FailDuringMigration
```

## 59. Recovery Assertions

A recovery test MUST assert:

- authoritative state;
- Operation Record state;
- retry behavior;
- user-facing status;
- duplicate prevention;
- observability event;
- no hidden data exposure.

## 60. Concurrency Tests

Concurrency tests SHOULD cover:

- two commands against same Campaign version;
- competing advancement;
- duplicate Roll click;
- finalization and player input conflict;
- Preference change during active Session;
- background worker lease collision;
- second desktop instance.

## 61. Property-Based Tests

Property-based testing MAY be valuable for:

- Character schema values;
- modifier combinations;
- Dice resolution invariants;
- serialization round trips;
- migration preservation;
- identifier remapping;
- archive parser limits.

## 62. Fuzz Testing

Fuzzing SHOULD target:

- import manifests;
- archive structure;
- structured provider output;
- Character schema parsing;
- configuration parsing;
- local file metadata;
- IPC messages if introduced.

## 63. Mutation Testing

Mutation testing MAY be used selectively for critical invariant code.

Priority targets:

- idempotency;
- visibility;
- Memory aging;
- progression balances;
- Dice immutability;
- version checks.

## 64. Code Coverage

Coverage is a diagnostic, not the goal.

Chronicle SHOULD track:

- line coverage;
- branch coverage;
- critical-contract coverage.

A high percentage MUST NOT excuse missing behavior tests.

## 65. Critical Path Coverage

The following MUST have explicit behavior tests regardless of coverage percentage:

- Campaign creation;
- Session lifecycle;
- Dice Roll;
- finalization;
- progression;
- persistence transaction;
- backup and restore;
- Secret filtering;
- idempotent retry;
- migration.

## 66. Test Data Policy

Test data MUST NOT contain:

- real user Campaigns;
- real credentials;
- private prompts;
- private provider responses;
- unauthorized proprietary source text;
- personal local file paths.

## 67. Synthetic Data

Chronicle SHOULD use synthetic Campaigns and original Rule Set summaries.

## 68. Restricted Content in Tests

Restricted Rule Set content MUST NOT enter public repositories unless distribution is authorized.

Tests SHOULD prefer:

- original summaries;
- open-license material;
- synthetic rules;
- private local fixtures excluded from source control.

## 69. Secret Scanning

CI SHOULD scan for:

- API keys;
- access tokens;
- private keys;
- accidental provider payloads;
- sensitive local paths.

## 70. Test Isolation

Tests MUST isolate:

- filesystem;
- database;
- environment variables;
- credential store;
- provider network access;
- locale;
- timezone;
- process lock;
- ports.

## 71. Parallel Test Safety

Tests running in parallel MUST use unique:

- data directories;
- database names;
- ports;
- OperationIds;
- temporary artifact paths;
- credential aliases.

## 72. Flaky Test Policy

A flaky test is a defect.

Flaky tests SHOULD:

- be quarantined only temporarily;
- receive an owner;
- receive a tracking issue;
- retain failure evidence;
- be fixed or removed promptly.

CI MUST NOT normalize repeated blind reruns as success.

## 73. Test Failure Diagnostics

Test failures SHOULD preserve:

- seed;
- fixture version;
- OperationId;
- relevant structured logs;
- assertion context;
- generated artifact;
- screenshot for UI failure;
- trace for integration failure.

They MUST not leak credentials.

## 74. Continuous Integration

CI SHOULD run stages such as:

```text
Static Analysis
Unit and Domain Tests
Application Tests
Contract Tests
Persistence Integration Tests
Security Tests
UI Component Tests
End-to-End Smoke Tests
Migration Tests
Packaging Validation
Artifact Publication
```

The default CI graph runs deterministic provider tests, adapter contract tests, recorded fixtures, and fakes only.

Default CI MUST NOT call OpenAI, Ollama, or any other real LLM service.

Provider credentials MUST NOT be required for default CI.

## 75. Pull Request Gate

A pull request SHOULD require:

- successful deterministic tests;
- static analysis;
- formatting or lint validation;
- no secret scan findings;
- no new Critical security failure;
- required RFC or ADR update;
- migration tests when persistence changes;
- contract tests when public contracts change.

## 76. Main Branch Gate

The main branch SHOULD remain releasable.

Merges that knowingly break required quality gates are prohibited unless an explicit emergency process is used.

## 77. Release Candidate Gate

A release candidate MUST pass:

- all deterministic suites;
- supported migration chains;
- backup and restore;
- portable export round trip;
- startup and Safe Mode;
- critical end-to-end journeys;
- security regression suite;
- accessibility smoke suite;
- package integrity validation;
- installer or package validation;
- open Critical defect review.

## 78. Release Gate

A release MUST NOT proceed with:

- known data-corruption defect;
- known Secret leakage;
- known duplicate Roll or progression defect;
- failed migration from supported version;
- failed backup restore;
- broken Campaign export;
- inaccessible critical workflow;
- invalid official Rule Set package.

## 79. Severity Model

Defects SHOULD be classified:

```text
Critical
High
Moderate
Low
Cosmetic
```

## 80. Critical Defects

Examples:

- data loss;
- cross-Campaign corruption;
- credential exposure;
- Secret leakage;
- fake or repeated authoritative Dice;
- duplicate progression;
- unrecoverable migration;
- restore corruption;
- remote code execution;
- inaccessible core workflow.

## 81. Quality Gate Exceptions

Any gate exception MUST include:

- reason;
- scope;
- risk;
- owner;
- expiration;
- mitigation;
- follow-up issue.

Permanent undocumented exceptions are prohibited.

## 82. Regression Policy

Every fixed defect SHOULD receive a regression test at the lowest useful layer.

Security and data-integrity defects MUST receive regression tests.

## 83. Contract Change Policy

A contract change requires:

- contract version review;
- compatibility review;
- updated fixtures;
- provider adapter tests;
- Rule Set tests where relevant;
- migration or fallback behavior;
- documentation update.

## 84. Schema Change Policy

A persistence or Character schema change requires:

- migration;
- source fixture;
- target fixture;
- rollback or safe-failure behavior;
- unknown-field preservation test;
- backup compatibility review.

## 85. Provider Change Policy

Changing provider adapter behavior requires:

- translation tests;
- structured-output tests;
- timeout and cancellation tests;
- usage-metadata tests;
- privacy review;
- bounded live-provider verification when available.

## 86. Rule Set Release Gate

A Rule Set package release MUST pass:

- package manifest validation;
- operation registration;
- Character schema compilation;
- mechanical golden fixtures;
- progression fixtures;
- Preference fixtures;
- migration fixtures;
- security restrictions;
- performance limits;
- no persistence or network access.

## 87. Official Application Smoke Suite

The official application SHOULD have a small fast smoke suite covering:

1. start application;
2. open fixture Campaign;
3. submit player input;
4. receive scripted narration;
5. perform Roll;
6. continue narration;
7. finalize Session;
8. close application;
9. reopen and verify state.

## 88. Manual Exploratory Testing

Manual testing remains necessary for:

- narrative feel;
- confusing state transitions;
- accessibility experience;
- provider behavior;
- visual hierarchy;
- recovery comprehension;
- long-session usability.

Manual testing does not replace deterministic automation.

## 89. Human Narrative Evaluation

Human review MAY assess:

- continuity;
- tone;
- agency;
- Secret discipline;
- mechanical faithfulness;
- repetition;
- pacing.

Results SHOULD be recorded through a repeatable rubric.

## 90. Quality Dashboard

Chronicle MAY maintain a local or CI dashboard showing:

- test status;
- flaky tests;
- coverage trends;
- migration support;
- performance budgets;
- security findings;
- evaluation results;
- open Critical defects.

The dashboard is advisory; release gates remain explicit.

## 91. Observability of Tests

Test infrastructure SHOULD emit structured results with:

- suite;
- fixture;
- duration;
- failure category;
- seed;
- artifact links;
- retry count;
- environment.

## 92. Development Workflow

Recommended workflow:

```text
Write or Update RFC/ADR
    ↓
Write Failing Test
    ↓
Implement Minimal Behavior
    ↓
Run Relevant Fast Suites
    ↓
Run Contract and Integration Tests
    ↓
Review Security and Migration Impact
    ↓
Merge Through Quality Gates
```

## 93. MVP Test Priorities

The MVP prioritizes:

1. Domain invariants;
2. Application idempotency;
3. Dice determinism;
4. Session finalization;
5. persistence atomicity;
6. provider contract validation;
7. Secret filtering;
8. Character schema;
9. Rule Set package contract;
10. backup and restore;
11. core UI flow;
12. migration safety.

## 94. Current Delivery Decision

The MVP adopts:

- deterministic unit and Domain tests;
- application use-case tests;
- reusable contract-test suites;
- real local-storage integration tests;
- scripted provider;
- deterministic development provider for CI, tests, demos, and reproducible development;
- fake clock and deterministic random source;
- canonical, complex, and adversarial Campaign fixtures;
- golden mechanical and progression fixtures;
- migration-chain tests;
- backup and export round-trip tests;
- security regression suite;
- critical UI interaction tests;
- accessibility smoke tests;
- bounded end-to-end smoke suite;
- no dependence on live providers for release correctness;
- no real LLM calls in default CI;
- no API key or installed local model requirement for normal tests;
- real-provider integration tests are explicit opt-in suites;
- explicit release gates;
- no tolerated permanent flaky tests.

## 95. Architecture Horizon

Future evolution MAY include:

- large-scale provider benchmarks;
- automated red-team suites;
- mutation testing expansion;
- cross-platform device farms;
- signed compatibility certification;
- community Rule Set conformance service;
- multiplayer load testing;
- cloud synchronization chaos testing;
- privacy-preserving production quality feedback.

The MVP MUST NOT implement these capabilities without a later milestone.

## 96. Open Questions

The following remain open:

- Which testing frameworks will be selected?
- Which CI platform will host the public repository?
- What coverage targets are useful without becoming vanity metrics?
- Which database engines and operating systems run in CI?
- How will desktop UI tests run headlessly?
- Which accessibility tooling is available for the selected UI framework?
- What reference hardware defines performance budgets?
- How often will live-provider evaluations run?
- Which provider responses may be safely recorded?
- How will private Rule Set fixtures be handled?
- Which migration versions remain officially supported?
- Should package authors receive a reusable conformance-test library?
- Which end-to-end tests are required for every pull request versus nightly runs?
- How will flaky tests be tracked publicly?
- Which release gates are automated and which require manual sign-off?

These questions require technology ADRs, CI implementation, framework selection, and release planning.

## 97. Compliance Checklist

An implementation complies when:

- deterministic boundaries are tested without live providers;
- time, randomness, identifiers, and scheduling are controllable;
- every state-changing use case tests idempotency and failure stages;
- Rule Set packages pass reusable contract tests;
- persistence tests use the real storage engine;
- migrations include source and target fixtures;
- Dice retries never reroll;
- finalization tests prove once-only effects;
- Secret and cross-Campaign protections have regression tests;
- backups and exports support semantic round trips;
- critical UI workflows are automated;
- accessibility is tested;
- test data contains no real credentials or private Campaigns;
- flaky tests are treated as defects;
- releases are blocked by data-integrity and security failures.

## 98. Final Principle

Chronicle can tolerate uncertainty in what the next sentence should be.

It cannot tolerate uncertainty in whether a Roll happened, whether a Session finalized, whether a Secret leaked, or whether history survived.

The story may be generated.

Trust must be tested.
