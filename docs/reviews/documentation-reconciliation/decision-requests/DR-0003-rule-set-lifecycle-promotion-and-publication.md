---
id: DR-0003
title: Rule Set Lifecycle, Promotion, and Publication
status: accepted
accepted_option: Option B
accepted_date: 2026-08-03
---

# DR-0003: Rule Set Lifecycle, Promotion, and Publication

## Decision Record

Status: accepted

Accepted option: Option B.

Effective date: 2026-08-03.

Decision:

Chronicle defines one shared normative Rule Set lifecycle across RFC-0027 and SPEC-0001.

Normative lifecycle states:

```text
proposed
source-registered
slice-defined
extracted
modeled
structurally-valid
substantively-reviewed
decision-set-finalized
evidence-complete
promotion-eligible
promoted
published
installed
enabled
active
deprecated
withdrawn
```

Promotion and publication are separate:

- promotion means the package satisfies the declared quality gate for a release scope;
- publication means an artifact was distributed through an approved channel;
- a package may be promotion-eligible or promoted without being published;
- publication never implies installation, enablement, or activation.

The lifecycle distinguishes:

- package maturity: proposed through promoted;
- publication status: unpublished, published, deprecated, or withdrawn;
- installation status: not-installed or installed;
- runtime activation status: disabled, enabled, or active.

Werewolf baseline evidence:

- approved-current-slice-reference-baseline;
- eligible-not-published;
- work package complete;
- no active issues or blockers;
- package not published.

## Context

The reconciliation found that several docs use publication language, while the Werewolf baseline demonstrates more lifecycle states: extraction candidate, prototype candidate, blocked, approved for promotion, promoted, stale evidence, and published. Promotion eligibility is not the same as runtime publication.

This decision determines whether those states become a shared normative contract across RFC-0027 and SPEC-0001.

## Authoritative Sources

- `docs/rfcs/RFC-0027-Rule-Set-Package-Architecture.md`: defines package status as Development, Experimental, Supported, Deprecated, and Unavailable, but does not define extraction/prototype/promotion lifecycle.
- `docs/specs/SPEC-0001-rule-set-package-artifact-model-and-extraction-contract.md`: defines sourcebook evidence, candidate extraction, reviewed interpretation, validation, and publication requirements.
- `docs/extraction/werewolf-3e/EXTRACTION-0001-source-inventory.md`: says missing metadata does not block first prototype extraction but does block final publication approval.
- `docs/extraction/werewolf-3e/EXTRACTION-0004-ambiguities-and-conflicts.md`: says prototype policy must not masquerade as source truth.
- Werewolf `prototype-readiness.json`: defines readiness statuses including not-started, in-progress, candidate-partial, prototype-candidate, blocked, approved-for-promotion, and published.
- Werewolf `tests/promotion-gate.json`: defines promotion gates and result statuses including passed, failed, blocked-pending-review, and stale-evidence.

## Contradiction or Gap

This is missing detail, with a downstream contradiction if left unresolved.

RFC-0027 package status describes user/package presentation. Werewolf readiness describes evidence and promotion lifecycle. These can coexist, but docs need to separate them.

## Options

### Option A: Keep lifecycle states implementation-specific

Leave Werewolf readiness and promotion states as prototype-specific implementation detail.

Consequences:

- No immediate RFC/SPEC changes.
- Other Rule Sets may invent incompatible lifecycle states.
- Promotion and publication remain ambiguous.
- Evidence freshness and stale gates have no shared meaning.

### Option B: Add shared lifecycle states to RFC-0027 and SPEC-0001

Define a generic lifecycle model covering extraction candidate, review-working-record, prototype-candidate, candidate-partial, blocked, implementation-ready, approved-for-promotion, promoted package artifact, runtime-publication-ready, published, deprecated, unavailable, and stale-evidence.

Consequences:

- Smallest shared contract that matches Werewolf evidence.
- Separates package presentation status from evidence/promotion lifecycle.
- Provides a consistent gate model for official and community packages.
- Requires amendments but not a new broad architecture decision.

### Option C: Create a new lifecycle ADR

Define all Rule Set lifecycle state, promotion, publication, and status semantics in a new ADR.

Consequences:

- Strong authority boundary.
- More process overhead.
- May duplicate RFC-0027 and SPEC-0001 unless carefully scoped.
- Useful only if lifecycle semantics affect Core architecture beyond package contracts.

## Recommendation

Recommend Option B.

It keeps the decision inside the existing Rule Set package architecture and artifact contract, adds the missing normative detail, and does not change Core architecture.

## Affected Documents

- `docs/rfcs/RFC-0027-Rule-Set-Package-Architecture.md`
- `docs/specs/SPEC-0001-rule-set-package-artifact-model-and-extraction-contract.md`
- `docs/extraction/werewolf-3e/EXTRACTION-0001-source-inventory.md`
- `docs/extraction/werewolf-3e/EXTRACTION-0003-character-creation-slice.md`
- `docs/extraction/werewolf-3e/EXTRACTION-0004-ambiguities-and-conflicts.md`
- `docs/extraction/werewolf-3e/EXTRACTION-0005-contract-findings.md`
- `docs/rfcs/RFC-0041-Build-Packaging-Release-and-Update-Architecture.md`
- `docs/rfcs/RFC-0042-Official-MVP-Scope-and-Acceptance-Criteria.md`

## Werewolf Evidence

- `prototype-readiness.json`: exposes readiness and publication readiness separately.
- `prototype-work-status.json`: separates declared plan state from verified completion evidence.
- `tests/promotion-gate.json`: defines promotion gates, promotion blockers, stale evidence behavior, and no manual bypass.
- `reviews/catalog-review-evidence-status.json`: defines evidence statuses and staleness policy.

## Implementation Impact

The application and tooling would need to track lifecycle separately from package display status. Build/release tooling would evaluate promotion eligibility before publication. Runtime selection would only expose packages that meet the chosen runtime-publication criteria.

## Community Rule Set Impact

Community Rule Set authors get a clear path from extraction to prototype to promotion to publication. They also inherit stricter evidence, stale-input, and blocker expectations, which improves trust but increases authoring burden.

## Required Document Mechanism

RFC-0027 amendment plus SPEC-0001 amendment. A new ADR is not recommended unless reviewers decide lifecycle state affects Core authority beyond package contracts.

## Acceptance Questions

- Accepted: Rule Set lifecycle states are a shared normative contract across RFC-0027 and SPEC-0001.
- Accepted: promotion eligibility is explicitly separate from publication.
- Accepted: stale evidence blocks promotion gates across all Rule Sets when gate inputs change.
- Accepted: package display status remains separate from evidence/promotion lifecycle, publication status, installation status, and runtime activation status.
