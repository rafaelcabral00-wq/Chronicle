---
id: ADR-0005
title: First Narrative Intelligence Provider
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
  - RFC-0011
  - RFC-0020
  - RFC-0021
  - RFC-0022
  - RFC-0023
  - RFC-0024
  - RFC-0025
  - RFC-0026
  - RFC-0035
  - RFC-0036
  - RFC-0037
  - RFC-0040
  - RFC-0042
---

> **"The first provider should prove Chronicle's contracts, not become Chronicle's architecture."**

# First Narrative Intelligence Provider

## 1. Status

**Proposed**

This ADR selects **OpenAI through the Responses API** as Chronicle's first official remote Narrative Intelligence provider.

The provider implementation will be isolated behind Chronicle's provider-neutral contracts.

The decision becomes **Accepted** after a provider spike demonstrates:

- Narrator capability;
- Archivist capability;
- Campaign Generator capability;
- schema-constrained structured output;
- cancellation;
- timeout;
- retry classification;
- malformed-output handling;
- repair;
- usage metadata capture;
- rate-limit handling;
- credential isolation;
- least-context enforcement;
- data-retention configuration review;
- deterministic scripted-provider parity for tests.

No concrete OpenAI model name becomes a Domain contract.

## 2. Context

Chronicle needs one complete Narrative Intelligence provider implementation for the official MVP.

The provider must support:

- long-form narrative;
- structured output;
- distinct capability profiles;
- reliable contract adherence;
- large contextual inputs;
- usage metadata;
- explicit timeout and cancellation;
- safe credential management;
- future model replacement;
- provider-neutral orchestration.

The provider remains advisory.

It may propose:

- narrative prose;
- Roll requests;
- Session summaries;
- Memory candidates;
- progression evidence;
- Relationship changes;
- Character Knowledge changes;
- Narrative Plan content.

It may not:

- persist state;
- execute authoritative Dice Rolls;
- alter Character Sheets directly;
- bypass Rule Set validation;
- invent accepted identifiers;
- decide Campaign truth.

## 3. Decision Drivers

The selection prioritizes:

1. structured-output reliability;
2. strong text-generation quality;
3. API maturity;
4. official .NET integration path;
5. model choice within one adapter;
6. usage and error metadata;
7. documented rate-limit behavior;
8. privacy and retention controls;
9. future portability;
10. testability without live API dependency.

## 4. Decision Summary

Chronicle will implement:

```text
Provider
    OpenAI API

Primary API Surface
    Responses API

Protocol Client
    Official OpenAI .NET SDK when suitable
    HttpClient-based adapter boundaries remain explicit

Provider Adapter
    Chronicle.NarrativeIntelligence.OpenAI

Authentication
    API key resolved through ISecretsManager

Output
    Provider-constrained structured JSON where supported
    Chronicle performs independent validation afterward

Conversation State
    Chronicle-managed by default
    provider-side persistent conversation state disabled unless explicitly approved later

Tools
    No built-in provider tools for MVP
    No provider-side web search
    No provider-side file search
    No provider-side code execution
    No provider-side computer use

Streaming
    Optional and not required for initial MVP acceptance

Background Mode
    Not required for MVP
    Chronicle durable Work Items remain authoritative

Model Selection
    Provider Model Profiles in application configuration
    no concrete model names in Domain or Application contracts
```

## 5. Why OpenAI

OpenAI is selected as the first provider because its current API platform provides:

- the Responses API as the recommended primitive for new projects;
- structured model outputs;
- asynchronous and streaming capabilities;
- documented rate limits;
- usage metadata;
- data-control documentation;
- official SDK support.

This is an implementation choice, not an endorsement of permanent provider dependence.

## 6. Responses API

### Decision

Use the Responses API rather than building the MVP adapter around Chat Completions.

### Rationale

OpenAI currently recommends Responses for new projects.

The API provides one unified response abstraction and supports structured outputs and multiple interaction modes.

### Constraint

Chronicle will use only the subset required by its provider contract.

The existence of provider-side tools does not authorize their use.

## 7. Provider Tools

### Decision

Disable provider-hosted tools in MVP.

Chronicle will not enable:

- web search;
- file search;
- code interpreter;
- computer use;
- remote MCP;
- image generation;
- arbitrary function execution.

### Rationale

Chronicle already owns:

- Rule Knowledge retrieval;
- local file policy;
- persistence;
- Dice;
- operation authorization;
- tool boundaries.

Allowing provider-side tools would blur trust and observability boundaries.

## 8. Tool Calling

Chronicle's structured Narrative Events are not provider tools.

The provider returns proposals.

Chronicle parses, validates, authorizes, and applies them through Application services.

## 9. Conversation State

### Decision

Chronicle manages conversational context explicitly.

Provider-side stored conversations or chained response state are disabled by default.

### Rationale

Chronicle must control:

- what is remembered;
- what is forgotten;
- context budgets;
- Secret visibility;
- replay;
- provider portability;
- historical evidence.

### Provider Request Rule

Every request is independently constructible from Chronicle state and operation context.

Provider response identifiers MAY be retained as diagnostic metadata, but they are not Campaign identity.

## 10. Storage Flag

Requests SHOULD use provider settings that avoid persistent provider-side state where supported and compatible with the selected data policy.

The exact request configuration must be verified against the current OpenAI API documentation during implementation.

## 11. Capability Profiles

The OpenAI adapter will support provider-neutral capability profiles.

Initial profiles:

```text
NarratorInteractive
ArchivistAnalytical
CampaignGeneratorCreative
StructuredRepair
ContextSummarizer
```

Not every capability requires a different concrete model.

## 12. Narrator Profile

The Narrator profile prioritizes:

- continuity;
- instruction adherence;
- prose quality;
- structured event reliability;
- acceptable interactive latency;
- Secret discipline;
- Roll-boundary compliance.

## 13. Archivist Profile

The Archivist profile prioritizes:

- evidence grounding;
- conservative proposals;
- structured output;
- duplicate avoidance;
- relationship and Knowledge precision;
- Session-finalization reliability.

## 14. Campaign Generator Profile

The Campaign Generator profile prioritizes:

- coherent premise;
- structured Narrative Plan;
- Character relevance;
- Preference adherence;
- future flexibility;
- noncanonical proposal status.

## 15. Repair Profile

The repair profile receives:

- failed structured payload;
- validation errors;
- required schema;
- minimal context.

It does not receive the full Campaign unless required.

## 16. Model Profiles

Chronicle will configure logical Model Profiles rather than embed concrete model names in code.

Example:

```text
openai.narrator.interactive
openai.archivist.analytical
openai.generator.creative
openai.repair.structured
```

Each maps to a concrete provider model through application configuration.

## 17. Model Name Isolation

Concrete model names belong only in:

- provider profile configuration;
- provider diagnostics;
- release-tested compatibility fixtures.

They MUST NOT appear in:

- Domain entities;
- Rule Set packages;
- Campaign invariants;
- portable Campaign formats as requirements.

A historical provider model identifier MAY be recorded as nonauthoritative operation metadata.

## 18. Model Change

Changing the concrete model does not migrate Campaign state.

It creates a new provider-configuration snapshot for future operations.

Accepted history remains unchanged.

## 19. SDK Selection

### Decision

Prefer the official OpenAI .NET SDK when it supports the required Responses API and structured-output features adequately.

### Fallback

Use direct HTTP through `IHttpClientFactory` when:

- an SDK feature is missing;
- response metadata is inaccessible;
- cancellation behavior is unsuitable;
- dependency constraints conflict with Chronicle.

### Boundary

SDK types remain inside `Chronicle.NarrativeIntelligence.OpenAI`.

## 20. Adapter Responsibilities

The adapter owns:

- request translation;
- authentication header attachment;
- HTTP execution;
- timeout;
- cancellation;
- provider error parsing;
- response translation;
- usage metadata extraction;
- provider request identifiers;
- rate-limit metadata;
- safe logging;
- compatibility checks.

It does not own:

- Prompt construction policy;
- context selection;
- Domain validation;
- event authorization;
- persistence;
- retries that require Application semantics.

## 21. Prompt Builder Ownership

Chronicle's provider-neutral Prompt Builder constructs the logical Prompt Document.

The OpenAI adapter translates it into the Responses API request structure.

## 22. Input Role Mapping

Trusted Chronicle instructions, operation context, and untrusted data MUST remain structurally distinguishable in the logical prompt.

The adapter must not flatten those distinctions accidentally.

## 23. Structured Output

### Decision

Use OpenAI structured-output capabilities where supported by the selected model.

### Rule

Provider schema enforcement is a first validation layer, not final authority.

Chronicle MUST still perform:

- JSON parsing;
- contract-version validation;
- event allowlist validation;
- reference validation;
- Campaign ownership validation;
- visibility validation;
- Rule Set validation;
- size and collection limits.

## 24. Schema Ownership

Chronicle owns the canonical JSON schemas for:

- Narrator output;
- Archivist output;
- Campaign Generator output;
- repair output.

Provider-specific schema transformations remain adapter details.

## 25. Schema Restrictions

Schemas SHOULD remain within the subset reliably supported by the provider.

Chronicle contracts must not become less rigorous; when provider schema features are insufficient, Chronicle validates the remaining rules locally.

## 26. Refusal Handling

The adapter MUST distinguish a provider refusal from:

- malformed output;
- timeout;
- rate limit;
- authentication failure;
- content filter;
- provider internal error.

Refusal is not treated as a valid empty narrative response.

## 27. Incomplete Output

The adapter MUST detect:

- output truncation;
- incomplete JSON;
- token-limit termination;
- missing required content;
- interrupted stream.

Incomplete output enters repair or retry policy.

## 28. Repair

Structured repair MAY use a second provider request.

Repair is bounded by:

- maximum attempt count;
- OperationId;
- original contract version;
- minimal necessary context;
- cost policy;
- timeout.

A repair response is validated from the beginning.

## 29. Repair Prohibitions

Repair MUST NOT:

- change accepted Campaign state;
- reroll Dice;
- invent missing authoritative identifiers;
- silently remove required events;
- broaden Secret visibility;
- loop indefinitely.

## 30. Request Identity

Every provider request SHOULD carry local metadata linking it to:

- OperationId;
- CorrelationId;
- capability;
- attempt;
- contract version;
- provider profile;
- configuration snapshot.

Provider metadata fields must not contain private narrative text.

## 31. Idempotency

Provider invocation itself may not be provider-idempotent.

Chronicle's Application layer remains responsible for:

- deciding whether another call is safe;
- rejecting stale responses;
- preventing duplicate persistence;
- returning existing committed results.

## 32. Timeouts

Timeouts are configured per capability.

A Narrator request should generally have a shorter interactive timeout than a Campaign Generator or Archivist request.

Exact values require measurement.

## 33. Cancellation

Cancellation SHOULD abort local waiting and HTTP work where possible.

Cancellation does not imply the provider did not process the request.

Chronicle must still reject any late or stale response.

## 34. Retry Policy

Retry policy distinguishes:

```text
SafeTransientRetry
RateLimitedRetry
RetryAfterCredentialRepair
RetryAfterConfigurationChange
NotRetryable
RequiresUserDecision
```

## 35. Automatically Retryable Conditions

Bounded automatic retry MAY apply to:

- transient network failure before response;
- selected server errors;
- explicit rate-limit responses with safe delay;
- connection reset;
- temporary service unavailability.

## 36. Nonautomatic Retry Conditions

Automatic retry SHOULD NOT apply blindly to:

- authentication failure;
- billing or quota exhaustion;
- invalid request;
- invalid schema;
- content refusal;
- incompatible model;
- provider policy rejection;
- ambiguous completion after local cancellation.

## 37. Exponential Backoff

Transient retries SHOULD use bounded exponential backoff with jitter.

Provider-provided retry guidance SHOULD be respected where safe.

## 38. Rate Limits

The adapter MUST parse and expose rate-limit conditions through provider-neutral errors and safe metadata.

Rate-limit handling MUST NOT keep a database transaction open.

## 39. Spend Limits and Quota

Quota or spend-limit failure requires user action.

The UI should explain that Campaign state remains preserved.

## 40. Usage Metadata

The adapter SHOULD capture, when available:

- input units;
- output units;
- cached input units;
- reasoning or provider-specific usage units;
- total usage;
- provider request identifier;
- finish status;
- latency.

Provider-specific fields remain in an adapter metadata envelope.

## 41. Cost Estimation

Chronicle MAY estimate operation cost from configuration.

Cost is informative and may be inaccurate when provider pricing changes.

Pricing tables MUST NOT be compiled permanently into Domain logic.

## 42. Authentication

The adapter uses an API key resolved at invocation time through `ISecretsManager`.

The key MUST NOT enter:

- Campaign persistence;
- prompts;
- provider-neutral DTOs;
- Operation Records;
- logs;
- diagnostic bundles;
- exports;
- backups.

## 43. Credential Alias

The provider profile stores only a credential alias.

Example:

```text
credential://providers/openai/default
```

## 44. Credential Test

Chronicle MAY provide a provider-profile test using a bounded request without Campaign content.

The result records:

- success or failure;
- safe error category;
- latency;
- profile compatibility.

## 45. Data Classification

Before invocation, Chronicle compares context classifications to the provider profile's transmission policy.

The adapter MUST reject a request marked as forbidden for remote transmission.

## 46. Restricted Rule Content

Restricted Rule Knowledge is not sent remotely unless:

- policy allows it;
- provenance allows it;
- the user has configured the provider accordingly;
- the excerpt is necessary and bounded.

## 47. Data Retention Review

OpenAI data-control and retention behavior is external and may evolve.

The implementation MUST:

- document the provider configuration assumptions;
- expose relevant privacy settings when controllable;
- avoid claiming stronger guarantees than the configured service provides;
- review the current official documentation before release.

## 48. Provider-Side Training Claims

Chronicle documentation MUST distinguish:

- API data-use policy;
- provider retention;
- account configuration;
- contractual options.

It MUST not make broad privacy claims based on outdated assumptions.

## 49. Least Context

The adapter receives an already minimized request.

It MUST NOT enrich it automatically with:

- full Campaign history;
- unrelated Memories;
- other Campaign data;
- local files;
- provider conversation history;
- arbitrary Rule Knowledge.

## 50. Prompt Injection

The provider adapter does not solve prompt injection alone.

Chronicle uses layered controls:

- instruction/data separation;
- untrusted-content labeling;
- bounded context;
- no provider tools;
- structured output;
- event allowlists;
- independent validation;
- no direct persistence.

## 51. Moderation and Content Filters

Provider policy behavior may refuse or alter some content.

The adapter MUST surface this as a provider limitation rather than silently changing Campaign truth.

Chronicle MAY later support a separate content-policy workflow.

## 52. Provider Safety Output

Provider safety metadata MAY be captured when available.

It remains advisory and does not replace Chronicle security validation.

## 53. Streaming

### Decision

Streaming is optional for the MVP.

The first accepted implementation may use nonstreaming responses.

### If Implemented

Streaming output is provisional until the complete structured response is validated.

The UI may display prose progressively only when:

- it is clearly provisional;
- no structured event is applied;
- Secret validation policy is respected;
- failure can remove or replace it safely.

## 54. Background Mode

Provider background mode is not required.

Chronicle's durable Work Items remain the authoritative mechanism for long-running recovery.

Provider background identifiers MAY be supported later as adapter metadata.

## 55. Webhooks

Webhooks are not used in the desktop MVP.

A local desktop application SHOULD not expose a public callback endpoint solely for provider completion.

## 56. Provider Request Persistence

Chronicle SHOULD persist safe operation metadata, not full raw requests by default.

Persisted metadata MAY include:

- provider profile;
- capability;
- model mapping;
- contract version;
- attempt count;
- request hash;
- provider request identifier;
- timestamps;
- usage;
- status.

## 57. Raw Prompt Retention

Raw prompts are not persisted by default.

Developer capture requires explicit activation, warning, redaction, and bounded retention.

## 58. Raw Response Retention

Raw provider responses are not persisted as diagnostics by default.

Accepted narrative content and accepted structured effects are persisted through Chronicle's normal Domain models.

## 59. Observability

Provider observability SHOULD include:

- OperationId;
- capability;
- provider profile;
- model profile;
- concrete model identifier;
- attempt;
- latency;
- input and output usage;
- finish status;
- repair count;
- retry classification;
- safe error code.

## 60. Logging

Logs MUST NOT include:

- API key;
- authorization header;
- full request;
- full response;
- Secret content;
- restricted Rule Knowledge;
- Character biography.

## 61. Provider Health

The adapter SHOULD report:

```text
Healthy
Degraded
RateLimited
CredentialInvalid
QuotaBlocked
Unavailable
Incompatible
Unknown
```

Provider health does not alter Campaign integrity.

## 62. Configuration

The OpenAI provider profile SHOULD support:

```text
ProviderProfileId
CredentialReference
Endpoint
CapabilityMappings
Timeouts
RetryPolicy
StreamingEnabled
DataHandlingPolicy
OrganizationOrProjectMetadata when needed
Enabled
ProfileVersion
```

## 63. Endpoint

Use the official OpenAI API endpoint by default.

Custom compatible endpoints are outside this ADR and require explicit configuration and trust labeling.

## 64. Organization and Project Scoping

When account scoping is supported, identifiers MAY be configured as nonsecret provider metadata.

They MUST not be confused with Chronicle Campaign or user identity.

## 65. Model Compatibility Registry

The adapter SHOULD maintain a tested compatibility record for configured models.

Possible states:

```text
Tested
Compatible
Experimental
Unsupported
Unknown
```

## 66. Compatibility Test

A compatibility check SHOULD validate:

- model availability;
- structured-output support;
- required context capacity;
- response behavior;
- capability profile requirements.

## 67. Model Removal

If a configured model becomes unavailable:

- the provider profile becomes degraded;
- Campaign state remains readable;
- new operations are blocked or mapped after user approval;
- accepted history remains unchanged.

## 68. Model Alias Risk

Provider aliases may change behavior without Chronicle code changes.

Stable dated model versions SHOULD be preferred for release-tested profiles when available and practical.

The exact policy requires implementation review.

## 69. OpenAI Adapter Project

The adapter SHOULD live in:

```text
src/Chronicle.NarrativeIntelligence.OpenAI/
```

If project count is temporarily minimized, it MAY begin in a provider-specific namespace inside `Chronicle.NarrativeIntelligence`, but extraction occurs before a second provider is added.

## 70. Project Dependencies

The adapter MAY reference:

```text
Chronicle.Contracts
Chronicle.Application abstractions
OpenAI SDK
Microsoft.Extensions.Http
Microsoft.Extensions.Logging abstractions
```

It MUST NOT reference:

- Chronicle.Desktop Views or ViewModels;
- Chronicle.Persistence.Sqlite;
- official Rule Set implementation;
- EF Core;
- Campaign repositories directly.

## 71. Provider-Neutral Interface

Conceptually, the adapter implements:

```text
INarrativeIntelligenceProvider
```

with operations such as:

```text
GenerateNarrationAsync
GenerateArchivistProposalAsync
GenerateCampaignProposalAsync
RepairStructuredOutputAsync
TestProfileAsync
```

The exact interface remains defined by RFC-0020 through RFC-0025.

## 72. Request DTO

A provider-neutral request SHOULD contain:

- OperationId;
- Capability;
- Prompt Document;
- Output Contract;
- Model Profile;
- Timeout profile;
- Data classifications;
- configuration snapshot;
- cancellation.

It does not contain the API key.

## 73. Response DTO

A provider-neutral response SHOULD contain:

- provider-neutral output payload;
- provider request identifier;
- provider profile;
- model identifier as metadata;
- usage;
- finish status;
- refusal information;
- timing;
- raw adapter diagnostics reference when enabled.

## 74. Error Mapping

Provider-specific errors map to Chronicle errors such as:

```text
ProviderAuthenticationFailed
ProviderRateLimited
ProviderQuotaExceeded
ProviderTimeout
ProviderUnavailable
ProviderRequestInvalid
ProviderModelUnavailable
ProviderOutputIncomplete
ProviderOutputMalformed
ProviderRefused
ProviderPolicyBlocked
ProviderCancelled
ProviderResponseStale
```

## 75. HTTP Resilience

HTTP resilience MAY use standard .NET resilience handlers.

The adapter must preserve Chronicle's retry semantics and must not create hidden unbounded retries.

## 76. Retry Ownership

Transport-level transient retry may occur inside the adapter.

Semantic retries and repair remain visible to Application orchestration.

## 77. Provider Request Size

Before sending, the adapter SHOULD verify:

- estimated input size;
- configured model context capacity;
- output reservation;
- provider request limits;
- schema size.

Oversized requests fail before network transmission where possible.

## 78. Token Counting

Chronicle MAY use provider token-counting support or local estimation.

Token counting is advisory for budgeting.

Actual provider usage metadata is recorded after response where available.

## 79. Context Capacity Changes

Model context capacity is configuration metadata, not hardcoded Domain truth.

The adapter must handle provider changes safely.

## 80. Testing Strategy

The OpenAI adapter requires:

```text
Translation Unit Tests
HTTP Contract Tests
Recorded Response Tests
Error Mapping Tests
Structured Output Tests
Rate-Limit Tests
Cancellation Tests
Privacy Tests
Bounded Live API Tests
```

## 81. Scripted Provider Parity

Every Application test uses Chronicle's scripted provider, not the live OpenAI API.

The scripted provider must reproduce:

- success;
- timeout;
- refusal;
- malformed JSON;
- repair;
- rate limit;
- stale response;
- cancellation;
- usage metadata.

## 82. Recorded Fixtures

Recorded provider fixtures MUST:

- be redacted;
- contain synthetic Campaign data;
- contain no credentials;
- contain no proprietary Rule Set text;
- identify API and contract version;
- be reviewable.

## 83. Live Tests

Live tests SHOULD be:

- opt-in;
- credential-gated;
- cost-bounded;
- tagged separately;
- excluded from deterministic release correctness;
- run against synthetic content;
- compatible with current rate limits.

## 84. Required Test Cases

Tests MUST cover:

- profile credential missing;
- credential rejected;
- valid Narrator request;
- valid Archivist request;
- valid Campaign Generator request;
- structured-output success;
- malformed response;
- output truncation;
- provider refusal;
- timeout;
- cancellation;
- network failure;
- rate limit;
- quota failure;
- provider internal error;
- repair success;
- repair exhaustion;
- stale response rejection;
- usage metadata extraction;
- response identifier capture;
- raw prompt logging disabled;
- API key redaction;
- forbidden data classification;
- restricted source blocked;
- provider-side tools absent;
- provider conversation state disabled by default;
- concrete model name absent from Domain;
- model unavailable;
- profile compatibility test;
- retry does not duplicate committed state.

## 85. Evaluation Gate

The first provider must pass RFC-0026 evaluations for:

- continuity;
- Character consistency;
- Secret discipline;
- Roll-boundary compliance;
- structured event validity;
- Archivist precision;
- Memory quality;
- progression evidence quality;
- Campaign Generator coherence;
- repair effectiveness.

## 86. MVP Acceptance Threshold

Exact numeric thresholds require the evaluation implementation.

At minimum:

- schema validity must be high enough that repair remains exceptional;
- Secret leakage in the required adversarial suite is blocking;
- provider-generated authoritative Dice is always rejected;
- stale responses are never accepted;
- Session finalization remains recoverable;
- the canonical end-to-end Campaign scenario completes.

## 87. Alternatives Considered

### Anthropic as First Provider

Strengths:

- strong long-form reasoning and narrative capability;
- structured tool-use support;
- mature API.

Not selected first because the project needs one provider only, and OpenAI's current Responses API and structured-output path align well with the initial contract spike.

Anthropic remains a strong candidate for the second provider, which should test provider neutrality.

### Google Gemini as First Provider

Strengths:

- broad model portfolio;
- long-context capabilities;
- multimodal support.

Not selected first to avoid widening the initial integration surface.

It remains a future adapter candidate.

### Open-Source Local Model as First Provider

Strengths:

- privacy;
- offline play;
- provider independence.

Not selected first because local-model packaging, hardware variability, process management, and structured reliability add risk beyond the initial vertical slice.

Local support remains a post-MVP priority candidate.

### Multi-Provider Gateway

Not selected because it would add another external trust and compatibility layer before direct provider contracts are proven.

### OpenAI-Compatible Generic Endpoint First

Not selected because compatibility labels often hide behavioral differences.

Chronicle should first implement and test one real provider explicitly.

## 88. Consequences

### Positive

- one mature API path for all required capabilities;
- structured-output support;
- one adapter can map several logical Model Profiles;
- official documentation and SDK path;
- clear second-provider comparison later;
- strong fit with provider-neutral contracts.

### Negative

- remote connectivity is required for this provider;
- usage incurs external cost;
- provider behavior and model availability can change;
- rate limits may interrupt play;
- content policy may refuse some narrative;
- privacy depends partly on external service configuration;
- current API details require ongoing maintenance.

## 89. Risks

### Provider Lock-In

Mitigation:

- provider-neutral DTOs;
- no provider conversation authority;
- no provider tools;
- no concrete model names in Domain;
- scripted provider tests;
- second adapter planned after stabilization.

### Model Drift

Mitigation:

- tested Model Profiles;
- stable versions where possible;
- evaluation suite;
- configuration snapshots;
- compatibility status.

### Structured Output Failure

Mitigation:

- provider schema enforcement;
- independent validation;
- bounded repair;
- deterministic fallback behavior where approved.

### Data Exposure

Mitigation:

- least context;
- data classification;
- local Rule Knowledge retrieval;
- no raw prompt logging;
- explicit provider data policy;
- credentials in secure store.

### Cost and Rate Limits

Mitigation:

- usage capture;
- bounded retries;
- configurable profiles;
- cost display later;
- durable recovery;
- no open transaction during provider wait.

## 90. Security Requirements

The adapter MUST:

- use secure transport;
- attach credentials only at transport boundary;
- validate endpoint policy;
- disable unneeded tools;
- bound request and response size;
- redact headers;
- reject forbidden data classifications;
- treat every response as untrusted;
- preserve cancellation and timeout;
- avoid provider-controlled local file access.

## 91. Documentation Requirements

The official application documentation MUST explain:

- an OpenAI API account and key are required for this provider;
- usage may incur provider charges;
- provider availability is external;
- Campaign data may be transmitted according to configured operations;
- Chronicle sends bounded context, not the entire database;
- credentials remain outside Campaign exports and backups;
- the provider is replaceable architecturally.

## 92. Technology Spike

Before acceptance, implement:

1. provider profile and credential alias;
2. official API client initialization;
3. Narrator request;
4. schema-constrained response;
5. Archivist request;
6. Campaign Generator request;
7. timeout;
8. cancellation;
9. rate-limit simulation;
10. malformed-output repair;
11. usage metadata;
12. provider health;
13. least-context inspection;
14. no-tool verification;
15. restart recovery after a persisted Roll awaiting continuation.

## 93. Spike Acceptance

The spike passes when:

- one provider profile executes all three required capabilities;
- structured responses map to Chronicle contracts;
- invalid output is rejected;
- repair is bounded;
- provider failure does not mutate Campaign state;
- API key appears nowhere outside secure retrieval and HTTP authentication;
- late responses are rejected when stale;
- retry after persisted Roll does not reroll;
- live-provider tests are not required for normal test-suite success;
- the adapter can be replaced by the scripted provider without Application changes.

## 94. Definition of Compliance

An implementation complies when:

- it uses OpenAI Responses API as the first official provider path;
- the provider remains behind `INarrativeIntelligenceProvider`;
- provider-side tools are disabled;
- Chronicle manages context and memory;
- structured output is independently validated;
- credentials use `ISecretsManager`;
- concrete models are configuration;
- timeouts, cancellation, retries, and rate limits are typed;
- raw prompts and responses are not logged by default;
- data classification is enforced before transmission;
- provider failures never authorize state;
- deterministic tests do not call the live API.

## 95. Review Triggers

This ADR must be reviewed if:

- OpenAI deprecates the Responses API;
- required structured-output support changes materially;
- provider data policy changes;
- the official .NET SDK becomes unsuitable;
- model availability breaks the accepted profiles;
- cost or rate limits make the core loop impractical;
- legal or geographic availability affects the target release;
- a local provider becomes the preferred first-run experience.

## 96. External References

Current implementation must be validated against the latest official OpenAI documentation:

- Responses API and migration guidance:
  `https://developers.openai.com/api/docs/guides/migrate-to-responses`
- Structured outputs:
  `https://developers.openai.com/api/docs/guides/structured-outputs`
- Rate limits:
  `https://developers.openai.com/api/docs/guides/rate-limits`
- Data controls:
  `https://developers.openai.com/api/docs/guides/your-data`
- API reference:
  `https://developers.openai.com/api/reference`

These references are implementation inputs, not permanent Chronicle contracts.

## 97. Deferred Decisions

Later ADRs MAY define:

- concrete Model Profile mappings;
- exact timeout values;
- exact retry limits;
- streaming implementation;
- prompt-caching policy;
- local token counting;
- provider cost estimation;
- second provider selection;
- local provider process architecture;
- user-facing provider privacy disclosures.

## 98. Final Decision

Chronicle will use OpenAI through the Responses API as its first official remote Narrative Intelligence provider.

The adapter will use structured output, explicit capability profiles, secure credential aliases, bounded retries, safe observability, and Chronicle-managed context.

OpenAI is the first voice Chronicle will learn to use.

It is not the owner of the story, the memory, the rules, or the truth.
