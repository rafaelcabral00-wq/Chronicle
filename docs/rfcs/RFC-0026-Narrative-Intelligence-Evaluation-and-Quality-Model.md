---
id: RFC-0026
title: Narrative Intelligence Evaluation and Quality Model
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-07-31
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
---

> **"Narrative quality is not how impressive one response sounds. It is how reliably the Campaign remains coherent, playable, and worth remembering."**

# Narrative Intelligence Evaluation and Quality Model

## Abstract

This RFC defines Chronicle's quality model and evaluation architecture for Narrative Intelligence.

It establishes how Narrator, Campaign Generator, Archivist, Plan Reviser, Prompt Builder, structured-output contracts, provider adapters, and model profiles are evaluated before and during delivery.

The model separates:

- contract correctness;
- domain safety;
- Rule Set compliance;
- continuity;
- player agency;
- narrative quality;
- latency;
- cost;
- privacy;
- operational reliability.

Chronicle MUST not select a provider or prompt solely because individual examples appear impressive.

Evaluation MUST use repeatable fixtures, explicit rubrics, regression suites, failure injection, and human review.

## 1. Purpose

Narrative Intelligence is nondeterministic.

A model or prompt may:

- produce excellent prose in one test;
- violate Character Knowledge in another;
- expose a Secret;
- forget a Scene participant;
- continue past a required Dice Roll;
- invent unsupported mechanics;
- overrule player agency;
- generate invalid structured output;
- fail after a provider update;
- become too slow or expensive for interactive play.

Chronicle requires a quality system capable of detecting those failures before they become product behavior.

## 2. Scope

This RFC defines:

- quality dimensions;
- hard gates;
- soft quality scores;
- evaluation levels;
- deterministic fixtures;
- scenario suites;
- contract tests;
- domain-safety tests;
- Rule Set tests;
- narrative rubrics;
- human review;
- model comparison;
- prompt comparison;
- regression detection;
- provider-change evaluation;
- online operational metrics;
- privacy of evaluation data;
- release gates;
- failure reporting.

This RFC does not define:

- exact first provider;
- exact first model;
- final numerical thresholds;
- exact evaluation framework;
- external benchmark procurement;
- public leaderboards;
- user telemetry consent implementation;
- full content-safety policy.

## 3. Evaluation Principle

Chronicle evaluates Narrative Intelligence in this order:

```text
Safety and Contract Correctness
        ↓
Domain and Rule Compliance
        ↓
Continuity and Player Agency
        ↓
Narrative Quality
        ↓
Latency, Cost, and Operational Fit
```

A beautiful response that violates a hard invariant is a failed response.

## 4. Quality Is Multidimensional

No single score can represent Narrative Intelligence quality adequately.

Chronicle SHOULD maintain separate dimensions for:

- structural validity;
- reference validity;
- version validity;
- visibility safety;
- Domain consistency;
- Rule Set consistency;
- continuity;
- Character consistency;
- player agency;
- narrative relevance;
- prose clarity;
- pacing;
- emotional coherence;
- useful uncertainty;
- recovery behavior;
- latency;
- usage;
- cost;
- privacy exposure.

## 5. Hard Gates and Soft Scores

Evaluation dimensions are classified as:

```text
Hard Gate
Soft Score
Operational Constraint
```

### Hard Gate

A failure disqualifies the response or candidate configuration.

### Soft Score

Used to compare valid candidates.

### Operational Constraint

Defines whether the candidate is viable in the product environment.

## 6. Hard Gate Examples

Hard gates include:

- valid structured contract;
- correct OperationId;
- correct contract version;
- no cross-Campaign references;
- no unauthorized Secret reveal;
- no authoritative provider-generated randomness;
- no continuation beyond unresolved Roll;
- no duplicate finalization effects;
- no invalid Rule Set mechanics accepted;
- no direct persistent identifier invention;
- no completed history rewrite;
- no prohibited content-boundary violation.

## 7. Soft Score Examples

Soft scores include:

- prose quality;
- Scene atmosphere;
- Character voice;
- pacing;
- relevance;
- emotional impact;
- creativity within constraints;
- clarity of stakes;
- quality of Session summary;
- usefulness of Memory proposals;
- quality of Campaign hook.

## 8. Operational Constraint Examples

Operational constraints include:

- latency;
- request size;
- output size;
- rate limits;
- local hardware requirements;
- provider availability;
- cost per operation;
- privacy policy;
- structured-output support;
- supported locale.

## 9. Evaluation Targets

Chronicle SHOULD evaluate configurations, not models in isolation.

An evaluation target MAY include:

```text
Capability
+ Operation Profile
+ Provider Adapter
+ Model Profile
+ Prompt Template Version
+ Contract Version
+ Context Budget Profile
+ Repair Policy
```

Changing any component may change quality.

## 10. Evaluation Levels

Chronicle SHOULD support four levels:

```text
L1 — Static Contract Evaluation
L2 — Scenario Evaluation
L3 — Workflow Evaluation
L4 — Controlled Product Evaluation
```

## 11. L1 — Static Contract Evaluation

L1 validates one request and response pair.

It tests:

- schema;
- enums;
- identifiers;
- references;
- event permissions;
- output limits;
- parser safety;
- visibility fields;
- deterministic normalization.

L1 SHOULD run quickly and frequently.

## 12. L2 — Scenario Evaluation

L2 evaluates one bounded fictional situation.

Examples:

- social deception Scene;
- combat interruption;
- post-roll failure;
- Secret near-reveal;
- Relationship tension;
- Campaign generation from a Character biography;
- Session finalization with conflicting evidence.

L2 measures both hard gates and quality scores.

## 13. L3 — Workflow Evaluation

L3 evaluates multiple operations in sequence.

Examples:

```text
Open Scene
    ↓
Player Input
    ↓
Roll Request
    ↓
Execute Roll
    ↓
Continue After Roll
    ↓
Complete Scene
```

It tests continuity, idempotency, recovery, and accumulated error.

## 14. L4 — Controlled Product Evaluation

L4 evaluates the official application in controlled use.

It MAY include:

- internal playtests;
- invited testers;
- structured feedback;
- operational telemetry with consent;
- release-candidate evaluation.

L4 MUST not replace automated evaluation.

## 15. Capability-Specific Evaluation

Each capability requires its own evaluation model.

Initial capability suites are:

```text
Narrator Evaluation
Campaign Generator Evaluation
Archivist Evaluation
Plan Reviser Evaluation
Structured Repair Evaluation
```

## 16. Narrator Quality Dimensions

Narrator evaluation SHOULD include:

- active Scene adherence;
- participant correctness;
- Character perspective;
- Character voice;
- Knowledge consistency;
- Secret safety;
- Rule Set terminology;
- player agency;
- Roll interruption correctness;
- consequence fidelity;
- continuity;
- prose clarity;
- pacing;
- dramatic relevance;
- stop reason correctness.

## 17. Campaign Generator Quality Dimensions

Campaign Generator evaluation SHOULD include:

- Player Character relevance;
- Rule Set compatibility;
- Campaign Preference compliance;
- content-boundary compliance;
- initial playability;
- Act and Scene coherence;
- NPC quality;
- NPC persistence justification;
- Relationship directionality;
- Secret quality;
- mystery and clue structure;
- failure directions;
- openness to player choice;
- public versus hidden separation.

## 18. Archivist Quality Dimensions

Archivist evaluation SHOULD include:

- evidence fidelity;
- summary accuracy;
- public-summary safety;
- Memory significance;
- Memory deduplication;
- lifetime appropriateness;
- progression evidence;
- immediate-change deduplication;
- Relationship proportionality;
- Knowledge certainty;
- Secret reveal scope;
- unresolved-consequence usefulness;
- Plan revision signal quality.

## 19. Plan Reviser Quality Dimensions

Plan Reviser evaluation SHOULD include:

- preservation of completed history;
- response to actual divergence;
- removal of obsolete future assumptions;
- continuity with current Campaign State;
- retention of important themes;
- Player Character relevance;
- new Scene playability;
- avoidance of over-scripting;
- version and reference validity.

## 20. Repair Quality Dimensions

Structured repair evaluation SHOULD include:

- correction of reported validation errors;
- preservation of valid meaning;
- no creation of unsupported events;
- no hidden-information expansion;
- bounded output;
- stable EventKeys when appropriate;
- full post-repair contract validity.

## 21. Evaluation Fixture

An `EvaluationFixture` SHOULD contain:

- fixture identifier;
- fixture version;
- capability;
- operation profile;
- input context;
- expected hard constraints;
- allowed variation;
- forbidden outcomes;
- rubric;
- optional expected structured elements;
- reference validation result;
- sensitivity classification;
- tags.

## 22. Fixture Stability

Fixtures SHOULD use stable Chronicle identifiers and deterministic supporting data.

Provider-generated output is not embedded as the only expected answer unless the test specifically checks normalization.

## 23. Fixture Categories

Recommended categories include:

```text
HappyPath
Boundary
Adversarial
Recovery
LongContext
RuleSet
Visibility
PlayerAgency
Continuity
Localization
Performance
```

## 24. Happy Path Fixtures

Happy path fixtures verify expected normal behavior.

Examples:

- valid prose-only turn;
- valid Roll Request;
- valid Campaign proposal;
- valid Session summary;
- valid Plan revision.

They are necessary but insufficient.

## 25. Boundary Fixtures

Boundary fixtures test:

- maximum participant count;
- maximum Memories;
- near-budget prompt;
- one unresolved Roll;
- ambiguous Character Knowledge;
- overlapping Relationships;
- temporary reference limits.

## 26. Adversarial Fixtures

Adversarial fixtures SHOULD include:

- player input attempting to override system rules;
- Character biography containing prompt injection;
- retrieved rule text containing instructions;
- hidden Secret that is tempting to reveal;
- invalid cross-Campaign identifier;
- provider output containing arbitrary event types;
- unsupported mechanical claim.

## 27. Recovery Fixtures

Recovery fixtures SHOULD test:

- provider timeout;
- invalid structured response;
- stale Scene context;
- duplicate response;
- failure after commit;
- pending Roll restart;
- finalization restart;
- repair limit reached.

## 28. Long-Context Fixtures

Long-context fixtures SHOULD test:

- many Campaign Memories;
- large Scene participant set;
- long Session transcript;
- dense Rule Set references;
- Campaign generation with extensive Character history.

They verify selection, compression, omission, and budget failure.

## 29. Rule Set Fixtures

Rule Set fixtures SHOULD test:

- valid operation key;
- invalid operation key;
- modifier validation;
- deterministic result interpretation;
- progression limits;
- Character schema compliance;
- terminology.

The first complete suite targets the initial Werewolf Rule Set implementation without distributing unauthorized source text.

## 30. Visibility Fixtures

Visibility fixtures SHOULD test:

- hidden NPC;
- private Relationship;
- Character-scoped Knowledge;
- partial Secret reveal;
- player-visible summary;
- internal-only Plan content;
- errors and warnings.

## 31. Player Agency Fixtures

Player agency tests SHOULD detect when the Narrator:

- chooses the Player Character's intention;
- invents irreversible dialogue;
- decides a major action;
- narrates internal thoughts as fact;
- resolves an uncertain action without a Test;
- ignores a direct player choice.

## 32. Localization Fixtures

Localization fixtures SHOULD verify:

- requested output language;
- canonical Character names;
- stable machine identifiers;
- localized Rule Set terminology;
- no language drift;
- visibility and contract fields unaffected by localization.

## 33. Golden Fixtures

A golden fixture contains a stable input and expected validation outcome.

It MAY define:

- required event;
- forbidden event;
- expected stop reason;
- expected accepted references;
- expected rejection code;
- acceptable score range.

It SHOULD NOT require exact prose matching for creative output.

## 34. Exact-Match Tests

Exact matching SHOULD be limited to deterministic elements such as:

- contract fields;
- identifiers;
- event types;
- enum values;
- Rule Set results;
- error codes;
- operation status;
- selected context references.

## 35. Semantic Evaluation

Creative output MAY require semantic evaluation.

Semantic evaluation MAY use:

- deterministic rules;
- rubric-based human review;
- a separate evaluator model;
- hybrid scoring.

No evaluator model score should be trusted without calibration.

## 36. Deterministic Evaluators

Chronicle SHOULD maximize deterministic evaluation where possible.

Examples:

- schema validity;
- reference validity;
- Secret string appearance;
- prohibited identifier;
- stop reason;
- event count;
- Roll outcome contradiction;
- unsupported operation key;
- Message duplication;
- budget compliance.

## 37. Model-Based Evaluators

A model-based evaluator MAY assess:

- Character voice;
- pacing;
- thematic coherence;
- Player Character relevance;
- summary quality;
- proportionality of Relationship change.

Its output is advisory and should be calibrated against human review.

## 38. Evaluator Isolation

The evaluator SHOULD NOT receive hidden reference answers that would invalidate the test unless those answers are explicitly part of the rubric.

Evaluator prompts and models MUST be versioned.

## 39. Evaluator Bias

Chronicle SHOULD consider evaluator bias such as:

- preference for longer prose;
- preference for one writing style;
- provider self-preference;
- language-specific performance;
- overrewarding confident explanations;
- underdetecting subtle Secret leakage.

## 40. Human Review

Human review is required for subjective quality dimensions.

Reviewers SHOULD use explicit rubrics rather than free-form preference alone.

A review MAY score:

```text
1 — Unacceptable
2 — Weak
3 — Acceptable
4 — Strong
5 — Excellent
```

## 41. Human Review Rubric

A Narrator review rubric MAY include:

- respects player agency;
- maintains Scene focus;
- portrays Characters consistently;
- uses relevant Memories naturally;
- preserves uncertainty;
- creates meaningful forward motion;
- avoids unnecessary exposition;
- writes clearly in the requested language.

## 42. Reviewer Guidance

Reviewers SHOULD distinguish:

- personal stylistic preference;
- product quality;
- hard contract violation;
- Rule Set violation;
- missing context caused by Prompt Builder;
- provider failure;
- adapter failure.

## 43. Inter-Rater Reliability

When possible, subjective evaluation SHOULD use more than one reviewer.

Chronicle MAY track agreement between reviewers.

Low agreement indicates:

- unclear rubric;
- highly subjective dimension;
- insufficient fixture context;
- inconsistent reviewer training.

## 44. Pairwise Comparison

Pairwise comparison MAY be used to compare two valid configurations.

Reviewers answer:

```text
Which response better satisfies the rubric?
```

Pairwise comparison can be more reliable than isolated scoring.

## 45. Blind Evaluation

Provider, model, and prompt identity SHOULD be hidden from reviewers where practical.

This reduces brand and expectation bias.

## 46. Evaluation Run

An `EvaluationRun` SHOULD record:

- run identifier;
- timestamp;
- candidate configuration;
- fixture set version;
- evaluator versions;
- provider adapter version;
- model profile;
- prompt template versions;
- context budget profiles;
- results;
- failures;
- aggregate metrics.

## 47. Candidate Configuration

A candidate configuration SHOULD identify:

- provider;
- concrete model mapping;
- capability;
- operation profile;
- prompt template;
- contract version;
- context budget;
- repair policy;
- timeout;
- retry policy.

## 48. Reproducibility

For each evaluation result, Chronicle SHOULD retain:

- fixture version;
- request fingerprint;
- template version;
- selected context references;
- omission report;
- provider profile;
- output or redacted output;
- validation result;
- evaluator result.

Raw sensitive data retention follows policy.

## 49. Nondeterministic Repetition

Creative tests SHOULD run a fixture multiple times where budget permits.

This measures:

- failure frequency;
- variance;
- rare hard-gate violations;
- stability of quality;
- repair rate.

One successful sample is insufficient.

## 50. Pass Rate

Hard-gate results SHOULD be reported as pass rates.

Examples:

```text
Contract validity: 99.5%
Secret safety: 100%
Correct Roll stopping: 98.8%
```

Release thresholds will be defined later.

## 51. Severity-Weighted Failure

Not all failures have equal severity.

Example classification:

```text
Critical:
Secret leak, cross-Campaign mutation, false Roll result

Major:
Player agency violation, invalid Scene transition, unsupported mechanics

Moderate:
Weak continuity, wrong Character tone, unnecessary exposition

Minor:
Formatting issue, awkward phrasing
```

## 52. Critical Failure Policy

A single Critical failure MAY block release depending on fixture validity and reproducibility.

Critical failures MUST be investigated individually.

Aggregate averages MUST NOT hide them.

## 53. Score Aggregation

Soft scores MAY be aggregated by:

- capability;
- operation profile;
- locale;
- fixture category;
- Rule Set scenario;
- provider;
- prompt version.

A single global score SHOULD NOT be the only decision measure.

## 54. Regression Baseline

Every release candidate SHOULD be compared against an approved baseline.

The baseline SHOULD include:

- hard-gate pass rates;
- soft-score distributions;
- latency;
- usage;
- repair rate;
- retry rate;
- provider failure rate.

## 55. Regression Definition

A regression occurs when a candidate:

- introduces a new hard-gate failure;
- reduces a critical pass rate;
- materially lowers a soft quality dimension;
- increases latency beyond product tolerance;
- increases usage or cost without approved benefit;
- worsens privacy exposure;
- increases repair dependency.

## 56. Prompt Regression

Every production prompt template change SHOULD run the relevant regression suite.

Prompt changes MUST NOT be deployed solely from manual inspection.

## 57. Provider Model Update

When a provider changes a model behind a stable name or profile, Chronicle SHOULD rerun evaluation where detectable.

Operational monitoring SHOULD watch for:

- contract-validity decline;
- increased refusal;
- increased latency;
- changed output length;
- new Secret leakage;
- higher repair rate.

## 58. Adapter Regression

Provider Adapter changes require tests for:

- request mapping;
- structured-output schema;
- response normalization;
- enum handling;
- usage metadata;
- error mapping;
- timeout behavior;
- Unicode handling.

## 59. Contract Regression

Contract version changes require:

- schema compatibility tests;
- event permission tests;
- repair tests;
- adapter contract tests;
- fixture migration;
- backward-compatibility evaluation.

## 60. Context Selection Regression

Changes to Memory selection or Character snapshots SHOULD be evaluated for:

- omitted critical context;
- unnecessary context growth;
- continuity;
- Character consistency;
- hidden-information exposure;
- cost and latency.

## 61. Quality Gates by Capability

Each capability SHOULD define minimum release gates.

Example categories:

### Narrator

- contract validity;
- Roll interruption;
- Secret safety;
- agency safety;
- continuity score;
- latency.

### Campaign Generator

- valid initial playability;
- Player Character relevance;
- Rule Set compliance;
- content-boundary compliance;
- reference validity.

### Archivist

- evidence validity;
- no duplicate immediate changes;
- public-summary safety;
- progression validation;
- Memory quality.

## 62. Release Gate Structure

A release gate MAY contain:

```text
Required Hard-Gate Thresholds
Required Soft-Score Minimums
Maximum Critical Failures
Maximum Latency
Maximum Repair Rate
Maximum Cost or Usage
Required Human Review Approval
```

Exact values require implementation evidence.

## 63. Quality Budget

Chronicle MAY define a quality budget for known noncritical limitations.

Example:

```text
One minor style regression accepted
in exchange for a major latency reduction
```

Hard safety gates are not tradable.

## 64. Repair Rate

Repair rate is a key quality metric.

A high repair rate indicates:

- weak model fit;
- weak prompt;
- schema too complex;
- adapter mismatch;
- insufficient context;
- provider structured-output instability.

Repair should be a recovery path, not normal operation.

## 65. Regeneration Rate

Regeneration rate SHOULD be tracked separately from repair.

Frequent regeneration may indicate:

- stale-context races;
- weak output reliability;
- poor provider selection;
- unclear operation instructions.

## 66. Refusal Rate

Refusal rate SHOULD be measured by capability and fixture type.

A refusal may be correct in some safety scenarios.

Evaluation MUST distinguish appropriate and inappropriate refusal.

## 67. Context Insufficiency Rate

`ContextInsufficient` responses SHOULD be tracked.

A high rate may indicate:

- overly aggressive context reduction;
- missing Character snapshot fields;
- weak Memory selection;
- unclear Prompt Builder contract.

## 68. Online Metrics

With appropriate privacy controls, production MAY track:

- provider latency;
- structured-output validity;
- repair count;
- retry count;
- refusal;
- timeout;
- stale response;
- event rejection;
- user retry;
- Session interruption;
- finalization failure;
- context size.

## 69. Product Feedback

User feedback MAY include:

- response helpfulness;
- Character consistency;
- excessive verbosity;
- loss of agency;
- contradiction;
- weak pacing;
- wrong tone;
- incorrect rules;
- Secret reveal.

Feedback SHOULD map to quality dimensions.

## 70. Feedback Is Not Automatic Truth

User feedback is valuable but may be:

- subjective;
- incomplete;
- based on hidden information the user cannot see;
- caused by Rule Set or UI behavior rather than Narrative Intelligence.

It requires classification.

## 71. Privacy in Evaluation

Evaluation fixtures SHOULD use:

- synthetic Campaigns;
- consented test data;
- redacted real examples;
- licensing-safe Rule Set summaries.

Private Campaign data MUST NOT enter shared evaluation sets without explicit authorization.

## 72. Sensitive Fixture Classification

Fixtures SHOULD declare sensitivity such as:

```text
Synthetic
Internal
Restricted
UserConsented
LicensedRuleContent
```

Access and retention follow classification.

## 73. Proprietary Rule Content

Evaluation MUST NOT redistribute proprietary sourcebook text.

Rule Set tests SHOULD use:

- executable rules;
- original summaries;
- licensed excerpts where permitted;
- synthetic mechanical fixtures.

## 74. Evaluation Storage

Evaluation storage MAY retain:

- inputs;
- outputs;
- scores;
- failures;
- metadata;
- redacted prompts.

It MUST remain separate from Campaign truth.

## 75. Failure Corpus

Chronicle SHOULD maintain a curated failure corpus.

Examples:

- Secret leak;
- invalid Roll result;
- agency violation;
- wrong participant;
- stale response;
- unsupported mechanic;
- duplicated Memory;
- invalid Relationship symmetry;
- public-summary leak.

Every fixed failure SHOULD become a regression fixture where practical.

## 76. Evaluation Tags

Fixtures SHOULD support tags such as:

```text
narrator
roll
agency
secret
werewolf
relationship
knowledge
long-context
recovery
portuguese
```

Tags allow targeted suites.

## 77. Smoke Suite

A fast smoke suite SHOULD run frequently.

It SHOULD cover:

- contract validity;
- one Narrator turn;
- one Roll Request;
- one post-roll continuation;
- one Campaign generation;
- one finalization;
- one invalid-response repair;
- one Secret-safety test.

## 78. Full Regression Suite

A full suite SHOULD run before significant release or provider change.

It SHOULD cover:

- all capabilities;
- all fixture categories;
- supported locales;
- initial Rule Set;
- recovery;
- adversarial inputs;
- long context;
- performance.

## 79. Nightly or Scheduled Evaluation

Scheduled evaluation MAY be introduced later.

It is useful when:

- providers change frequently;
- model aliases are unstable;
- many prompt experiments exist;
- cost permits repeated testing.

It is not required before the core evaluation harness exists.

## 80. Performance Evaluation

Performance evaluation SHOULD measure:

- context assembly time;
- Prompt Builder time;
- provider latency;
- validation time;
- repair time;
- transaction time;
- end-to-end user wait.

## 81. Latency Percentiles

Latency SHOULD be observed through distributions such as:

- median;
- 90th percentile;
- 95th percentile;
- timeout rate.

Average latency alone is insufficient.

## 82. Interactive Latency

Narrator and post-roll continuation have stricter latency expectations than:

- Campaign generation;
- Session finalization;
- Plan revision.

Capability-specific limits SHOULD be defined.

## 83. Cost Evaluation

Where provider cost exists, Chronicle SHOULD measure:

- input usage;
- output usage;
- repair usage;
- retry usage;
- cost by capability;
- cost by completed Session;
- cost by generated Campaign.

## 84. Quality-Cost Tradeoff

Chronicle MAY compare valid candidates on quality versus cost.

It MUST NOT choose a cheaper candidate that fails hard gates.

## 85. Local Model Evaluation

Local providers SHOULD be evaluated with the same contracts.

Additional operational dimensions include:

- hardware requirements;
- startup time;
- memory usage;
- sustained throughput;
- context limit;
- structured-output reliability;
- thermal or resource impact.

## 86. Provider Diversity

Chronicle SHOULD avoid evaluation criteria that assume one provider's behavior.

Shared fixtures and provider-neutral contracts enable comparison.

Provider-specific strengths MAY still influence profile mapping.

## 87. Evaluation Harness

The evaluation harness SHOULD be able to:

- load versioned fixtures;
- invoke a selected capability configuration;
- capture Prompt Document metadata;
- invoke provider or test double;
- validate output;
- run deterministic evaluators;
- run optional model evaluators;
- collect human review;
- compare baseline;
- generate reports.

## 88. Dry Run Mode

The harness SHOULD support a dry run that:

- builds context;
- constructs Prompt Document;
- estimates budget;
- validates configuration;
- does not invoke a paid or external provider.

## 89. Recorded Response Mode

The harness MAY replay recorded provider responses.

This supports:

- adapter testing;
- validator testing;
- repair testing;
- deterministic regression;
- reduced cost.

Recorded responses MUST not replace live provider evaluation entirely.

## 90. Scripted Provider Mode

A scripted provider MUST support precise failure cases.

Examples:

- malformed envelope;
- invalid event;
- delayed response;
- refusal;
- stale version;
- Secret leak;
- duplicate response.

## 91. Live Provider Mode

Live provider evaluation SHOULD:

- use bounded concurrency;
- respect cost limits;
- record configuration;
- avoid sensitive data;
- run repeated samples;
- detect rate limits and provider instability.

## 92. Evaluation Report

An evaluation report SHOULD include:

- candidate configuration;
- fixture-set version;
- hard-gate results;
- critical failures;
- soft-score summary;
- capability breakdown;
- latency;
- usage;
- repair rate;
- regression comparison;
- recommendation;
- unresolved risks.

## 93. Release Recommendation

A report MAY recommend:

```text
Approve
ApproveWithKnownLimitations
Reject
NeedsMoreEvidence
```

The recommendation MUST identify the reasons.

## 94. Approval Authority

Narrative Intelligence release approval SHOULD require both:

- automated gate success;
- responsible human review.

The exact ownership process will be defined later.

## 95. Incident Evaluation

When a production narrative defect is reported:

1. preserve safe diagnostics;
2. classify the defect;
3. identify responsible layer;
4. reproduce with a fixture;
5. add to failure corpus;
6. fix the correct layer;
7. rerun targeted and regression suites.

## 96. Layer Attribution

A quality failure MAY originate in:

- context selection;
- Prompt Builder;
- prompt template;
- provider adapter;
- model behavior;
- contract validator;
- Rule Set validator;
- Chronicle Director;
- UI representation.

Evaluation SHOULD avoid blaming the model automatically.

## 97. Example Attribution

```text
NPC omitted from response
```

Possible causes:

- Scene participant missing in persistence;
- Director failed to select NPC;
- Prompt Builder dropped required participant;
- provider ignored participant;
- validator accepted contradiction;
- UI failed to display dialogue.

The investigation must identify the actual layer.

## 98. Prohibited Patterns

### 98.1 Evaluate Only Prose Beauty

Narrative quality MUST NOT be reduced to writing style.

### 98.2 One Successful Demo as Evidence

A single impressive output MUST NOT justify release.

### 98.3 Average Hides Critical Failure

Critical safety failures MUST not disappear inside aggregate scores.

### 98.4 Exact Prose Matching

Creative output SHOULD NOT be tested through exact text matching.

### 98.5 Evaluator Model as Sole Judge

Model-based evaluation MUST NOT replace deterministic validation and human review.

### 98.6 Production Data Without Consent

Private Campaign content MUST NOT enter shared evaluation automatically.

### 98.7 Provider Change Without Regression

Meaningful provider or model changes SHOULD trigger reevaluation.

### 98.8 Repair Dependency Accepted as Normal

A configuration with excessive repair rate SHOULD not be considered healthy.

### 98.9 One Global Score

Chronicle MUST NOT use one opaque score as the only release decision.

### 98.10 Test Only Happy Paths

Adversarial, recovery, visibility, and long-context tests are mandatory.

## 99. Current Delivery Decision

The MVP adopts:

- provider-neutral evaluation targets;
- hard gates and soft scores;
- capability-specific quality dimensions;
- versioned synthetic fixtures;
- deterministic contract and safety evaluators;
- scripted test providers;
- human rubric review;
- smoke suite;
- full regression suite before major changes;
- failure corpus;
- prompt and adapter regression testing;
- latency and usage measurement;
- no public benchmark requirement;
- no automatic production-data collection;
- no single global quality score;
- no evaluator model as sole authority.

## 100. Architecture Horizon

Future evolution MAY include:

- automated nightly live-provider evaluation;
- multi-provider leaderboards;
- user-opted anonymized quality telemetry;
- learned evaluators;
- continuous prompt experiments;
- shadow provider testing;
- personalized style evaluation;
- automated red-team generation;
- multilingual reviewer pools;
- cost-quality optimization;
- local model hardware matrix.

The MVP MUST NOT implement these capabilities without a later milestone.

## 101. Open Questions

The following remain open:

- What exact hard-gate thresholds are required for MVP?
- How many repeated samples should each creative fixture use?
- Which subjective dimensions require two reviewers?
- Which evaluator model, if any, should be used?
- How should inter-rater reliability be measured?
- What latency targets apply to Narrator and post-roll continuation?
- What maximum repair rate is acceptable?
- How should cost be normalized across providers?
- Which fixtures require live provider execution?
- How should provider model alias changes be detected?
- How should Portuguese narrative quality be reviewed?
- Which Werewolf mechanics belong in the initial Rule Set fixture suite?
- How large should the initial failure corpus be?
- Which evaluation reports must block release automatically?
- Where will fixtures and results be stored?

These questions require technology ADRs, delivery RFCs, provider selection, and initial implementation evidence.

## 102. Compliance Checklist

An evaluation system complies when:

- evaluation targets include prompt, provider, contract, and budget versions;
- hard gates are separate from soft scores;
- critical failures are visible individually;
- each capability has a dedicated rubric;
- fixtures are versioned;
- adversarial and recovery scenarios exist;
- deterministic validation is maximized;
- model evaluators remain advisory;
- human review uses explicit rubrics;
- prompt and provider changes run regression suites;
- repeated samples measure nondeterminism;
- latency, usage, and repair rate are tracked;
- private Campaign data is protected;
- fixed production failures become regression fixtures;
- no single score controls release alone.

## 103. Final Principle

Narrative Intelligence earns trust through repeatable behavior, not isolated brilliance.

Chronicle should release imagination only after proving that it can remain inside memory, rules, agency, and truth.
