# Chronicle Documentation Reconciliation Decision Requests

Status: complete

This index covers the architecture decision-resolution work package created from `docs/reviews/documentation-reconciliation`.

## Decision Requests

| ID | Title | Covers | Recommended option | Depends on |
| --- | --- | --- | --- | --- |
| [DR-0001](DR-0001-topology-authority-and-repository-paths.md) | Topology Authority and Repository Paths | CONTRADICTION-001; MAT-001 | Accepted Option B: ADR-0043 v0.2.0 is concrete topology authority; ADR-0002 remains architecture authority | None |
| [DR-0002](DR-0002-mvp-rule-set-completeness.md) | MVP Rule Set Completeness | CONTRADICTION-002; complete Rule Set decision; MAT-005 | Accepted Option B: completeness means complete declared release scope with explicit enforceable exclusions | DR-0001 |
| [DR-0003](DR-0003-rule-set-lifecycle-promotion-and-publication.md) | Rule Set Lifecycle, Promotion, and Publication | Remaining CONTRADICTION-002 lifecycle decision; promotion/publication gap; MAT-003 | Accepted Option B: one shared normative Rule Set lifecycle across RFC-0027 and SPEC-0001 | DR-0002 accepted |
| [DR-0004](DR-0004-prototype-materialization-and-validation-contracts.md) | Prototype Materialization and Validation Contracts | CONTRADICTION-003; SPEC artifact-family decision; materialization mapping; MAT-002; MAT-004; MAT-006 | Accepted Option B: define normative materialization-role mapping and validation evidence contract | DR-0001, DR-0002, DR-0003 accepted |
| [DR-0012](DR-0012-spirit-umbra-capability-authority.md) | Spirit-Umbra Capability Authority | Spirit/Umbra S2 runtime operation capability; new capability key decision | Accepted Option A: new partial-executable capability `spirit-umbra` | None |

## Coverage

Contradictions covered:

- CONTRADICTION-001: covered by DR-0001.
- CONTRADICTION-002: completeness semantics resolved by DR-0002; lifecycle and promotion/publication semantics resolved by DR-0003.
- CONTRADICTION-003: resolved by DR-0004.

Architecture decisions covered:

- Whether MVP requires a complete game-system Rule Set or complete advertised current slice: resolved by DR-0002.
- Whether SPEC-0001 should add normative artifact families for evidence, blockers, reconciliation, localization validation, security enforcement, and executable test reports: resolved by DR-0004.
- How to map docs prototype, package source, packaged artifact, and installed package paths: resolved by DR-0004.
- Whether Rule Set lifecycle states should become a shared normative contract across RFC-0027 and SPEC-0001: resolved by DR-0003.

## Suggested Decision Order

1. DR-0001: settle topology authority before materialization language.
2. DR-0002: settled MVP completeness semantics before lifecycle gates.
3. DR-0003: settled lifecycle and promotion/publication states.
4. DR-0004: settled prototype-to-package materialization and validation contracts.
