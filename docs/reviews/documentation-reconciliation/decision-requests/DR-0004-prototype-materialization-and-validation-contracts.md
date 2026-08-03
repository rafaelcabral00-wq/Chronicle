---
id: DR-0004
title: Prototype Materialization and Validation Contracts
status: accepted
accepted_option: Option B
accepted_date: 2026-08-03
---

# DR-0004: Prototype Materialization and Validation Contracts

## Decision Record

Status: accepted

Accepted option: Option B.

Effective date: 2026-08-03.

Decision:

Chronicle defines the normative mapping and validation contract between:

- documentation prototype;
- package source;
- packaged artifact;
- installed artifact.

The documentation prototype is review and authoring evidence. It is never executable, packaging, installation, activation, Campaign-binding, or publication authority merely because it exists in the repository.

Progression between materialization roles requires explicit transformation, identity preservation, fingerprints, validation, reconciliation, and accepted evidence.

Required evidence families:

- structural validation;
- substantive review;
- finalized decisions;
- source provenance;
- localization;
- fixtures;
- executable tests;
- security;
- reconciliation;
- compatibility;
- migration when applicable;
- promotion readiness.

Repository materialization was blocked until the normative SPEC amendment existed. This application adds that amendment to SPEC-0001; materialization may now be planned against SPEC-0001, RFC-0027, and ADR-0043, but this decision does not publish or materialize a package, create Campaign bindings, define runtime implementation details, or treat the Werewolf prototype path as the generic package source layout.

## Context

SPEC-0001 defines an installed package layout and a recommended repository layout. ADR-0043 defines `rule-sets/` for official package implementations. The Werewolf baseline currently lives under `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype` as declarative prototype evidence.

The gap is the mapping among documentation prototype, future package source, packaged artifact, installed package layout, and the validation evidence required before promotion or publication.

## Authoritative Sources

- `docs/specs/SPEC-0001-rule-set-package-artifact-model-and-extraction-contract.md`: defines installed package layout, repository layout, artifact families, extraction workflow, provenance, review, validation, and publication requirements.
- `docs/adrs/ADR-0043-Repository-Topology-Project-Boundaries-and-Solution-Structure-v0.2.0.md`: defines `rule-sets/Chronicle.RuleSets.Werewolf/` for the first official package implementation and allows package-adjacent tests.
- `docs/adrs/ADR-0041-Security-Architecture-Secret-Management-Encryption-Privacy-and-Trust-Boundaries-v0.3.0.md`: establishes Rule Set package trust boundaries and runtime security constraints.
- `docs/adrs/ADR-0040-English-First-Technical-Language-Localization-Boundaries-and-Content-Language-Policy-v0.2.0.md`: establishes localization boundaries and stable technical identifiers.
- `docs/rfcs/RFC-0040-Testing-Strategy-and-Quality-Gates.md`: owns general testing strategy and quality gates.
- `docs/extraction/werewolf-3e/EXTRACTION-0004-ambiguities-and-conflicts.md`: requires prototype decisions to stay separate from source truth.
- `docs/extraction/werewolf-3e/EXTRACTION-0005-contract-findings.md`: recommends SPEC-0001 refinements after the first source pass.

## Contradiction or Gap

This is primarily missing detail, not a direct contradiction.

The installed layout, repository layout, and docs prototype path can all be valid if their authority and transitions are explicit. Without a mapping, future materialization may mistakenly copy prototype evidence into package source, treat declarative artifacts as executable implementation, or publish artifacts without complete validation evidence.

## Options

### Option A: Treat the docs prototype as the package source

Move or copy `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype` directly into package source.

Consequences:

- Fastest path to visible files.
- Blurs documentation evidence and executable package implementation.
- Risks treating prototype decisions as source truth.
- Conflicts with Werewolf work status showing runtime/package phases are not complete.

### Option B: Define a mapping and validation contract before source materialization

Keep the docs prototype as reference evidence. Define how approved artifacts flow into `rule-sets/Chronicle.RuleSets.Werewolf/` source, package output, and installed `rule-sets/<package-id>/<version>/` layout. Add validation contracts for evidence freshness, issue/blocker ledgers, reconciliation, localization validation, runtime security enforcement, and executable tests.

Consequences:

- Smallest safe path consistent with reconciliation.
- Avoids creating source projects before authority gaps are settled.
- Requires SPEC-0001 amendment and likely RFC cross-references.
- Gives future materialization an explicit checklist.

### Option C: Draft eight standalone missing specifications immediately

Create separate specs for lifecycle, materialization mapping, evidence freshness, issue/blockers, security enforcement, localization validation, promotion/publication, and prototype/package separation.

Consequences:

- Maximum detail.
- Violates the current instruction not to draft the eight missing specifications yet.
- May over-fragment the documentation.
- Should wait until decision requests are accepted.

## Recommendation

Recommend Option B.

It preserves the Werewolf baseline as evidence, prevents premature source project creation, and adds only the contract surface needed before materialization can safely begin.

## Affected Documents

- `docs/specs/SPEC-0001-rule-set-package-artifact-model-and-extraction-contract.md`
- `docs/adrs/ADR-0043-Repository-Topology-Project-Boundaries-and-Solution-Structure-v0.2.0.md`
- `docs/rfcs/RFC-0027-Rule-Set-Package-Architecture.md`
- `docs/rfcs/RFC-0040-Testing-Strategy-and-Quality-Gates.md`
- `docs/rfcs/RFC-0041-Build-Packaging-Release-and-Update-Architecture.md`
- `docs/rfcs/RFC-0042-Official-MVP-Scope-and-Acceptance-Criteria.md`
- `docs/adrs/ADR-0040-English-First-Technical-Language-Localization-Boundaries-and-Content-Language-Policy-v0.2.0.md`
- `docs/adrs/ADR-0041-Security-Architecture-Secret-Management-Encryption-Privacy-and-Trust-Boundaries-v0.3.0.md`
- `docs/extraction/werewolf-3e/EXTRACTION-0001-source-inventory.md`
- `docs/extraction/werewolf-3e/EXTRACTION-0002-content-classification.md`
- `docs/extraction/werewolf-3e/EXTRACTION-0003-character-creation-slice.md`
- `docs/extraction/werewolf-3e/EXTRACTION-0004-ambiguities-and-conflicts.md`
- `docs/extraction/werewolf-3e/EXTRACTION-0005-contract-findings.md`

## Werewolf Evidence

- `manifest.json`: contains source document ID, source version, source fingerprint, artifact families, localization, provenance, and review-required status.
- `reviews/catalog-review-issues.json`: provides issue status model, missing localization, stale evidence, reconciliation defect, security defect, and contract validation failure types.
- `reviews/catalog-review-evidence-status.json`: tracks evidence work items, validation state, fingerprints, staleness, dependencies, and completion eligibility.
- `reviews/catalog-review-reconciliation-report.json`: reconciles required identities, catalogs, review records, fixtures, tests, localization, security, and promotion readiness.
- `reviews/catalog-review-localization-validation-report.json`: proves localization validation is a first-class readiness concern.
- `security/runtime-enforcement.json`: distinguishes declaration checks from enforceable runtime security.
- `tests/harness/current-slice-runner.py`, `tests/test-suite-index.json`, and `tests/test-run-report-schema.json`: show that executable test evidence is part of promotion readiness.
- `prototype-work-status.json`: shows implementation, test/security hardening, packaging, and promotion phases are not complete.

## Implementation Impact

No source projects should be created until this decision is accepted and the resulting SPEC amendment exists. After acceptance, implementation can materialize repository structure with clear artifact flow, validation gates, and evidence requirements.

## Community Rule Set Impact

Community authors would receive a clear authoring path: extraction evidence, prototype artifacts, reviewed decisions, validation evidence, package source, packaged artifact, installed package. The burden is higher, but the trust model is clearer and reusable.

## Required Document Mechanism

SPEC-0001 amendment is required. RFC-0027 may need an amendment or cross-reference. A new ADR is not recommended unless the mapping changes repository topology beyond ADR-0043.

## Acceptance Questions

- Accepted: `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype` remains reference evidence rather than package source.
- Accepted: SPEC-0001 defines explicit transitions from documentation prototype to package source, packaged artifact, and installed artifact.
- Accepted: evidence freshness, issue/blocker ledgers, reconciliation, localization validation, runtime security enforcement, and executable test reports are represented through required evidence families for promotion readiness.
- Accepted: repository materialization remained blocked until the SPEC-0001 amendment existed; this DR-0004 blocker is resolved as of 2026-08-03.
