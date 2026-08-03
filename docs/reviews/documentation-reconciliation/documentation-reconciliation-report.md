# Documentation Reconciliation Report

Date: 2026-08-03

## Scope

Inspected every Markdown document under `docs/adrs`, `docs/rfcs`, `docs/specs`, and `docs/extraction`: 94 Markdown documents total.

Inspected the finalized Werewolf reference baseline under `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype`: 135 files. The baseline was treated as implementation evidence for testing the architecture, not as a source of generic mechanics.

## Files Created

- `docs/reviews/documentation-reconciliation/documentation-inventory.json`
- `docs/reviews/documentation-reconciliation/terminology-conflicts.json`
- `docs/reviews/documentation-reconciliation/contradiction-register.json`
- `docs/reviews/documentation-reconciliation/supersession-register.json`
- `docs/reviews/documentation-reconciliation/werewolf-lessons-impact.json`
- `docs/reviews/documentation-reconciliation/required-document-changes.json`
- `docs/reviews/documentation-reconciliation/repository-materialization-requirements.json`
- `docs/reviews/documentation-reconciliation/documentation-reconciliation-report.md`

## Files Corrected

- `docs/rfcs/RFC-0027-Rule-Set-Package-Architecture.md`: added missing `RFC-0031` dependency metadata.
- `docs/specs/SPEC-0001-rule-set-package-artifact-model-and-extraction-contract.md`: added missing `RFC-0031` dependency metadata.
- `docs/adrs/ADR-0002-Repository-and-Solution-Structure.md`: added DR-0001 authority-boundary note deferring concrete materialization details to ADR-0043 v0.2.0.
- `docs/adrs/ADR-0043-Repository-Topology-Project-Boundaries-and-Solution-Structure-v0.2.0.md`: recorded concrete topology authority, partial-supersession boundary, and conflict-resolution rule accepted by DR-0001.
- `docs/rfcs/RFC-0002-Product-Vision.md`: clarified that MVP Rule Set completeness means complete declared release scope, not complete source-system implementation.
- `docs/rfcs/RFC-0027-Rule-Set-Package-Architecture.md`: added the DR-0002 completeness definition, release declaration requirements, exclusion requirements, and public-claim constraints.
- `docs/rfcs/RFC-0027-Rule-Set-Package-Architecture.md`: added the DR-0003 shared lifecycle, transition rules, blocking conditions, and promotion/publication separation.
- `docs/specs/SPEC-0001-rule-set-package-artifact-model-and-extraction-contract.md`: added the DR-0003 shared lifecycle, transition contract, and publication distinction.
- `docs/rfcs/RFC-0042-Official-MVP-Scope-and-Acceptance-Criteria.md`: added the DR-0002 MVP acceptance definition, required Rule Set release declarations, and blocking criteria for unsupported public claims.
- `docs/specs/SPEC-0001-rule-set-package-artifact-model-and-extraction-contract.md`: added the DR-0004 materialization role mapping, progression requirements, evidence families, and blocking rule.
- `docs/rfcs/RFC-0027-Rule-Set-Package-Architecture.md`: added the DR-0004 materialization role boundary and cross-reference to SPEC-0001.

No unrelated architectural semantics were changed.

## Decision Updates

DR-0001 was accepted on 2026-08-03.

Accepted authority rule:

- ADR-0002 remains authoritative for repository and solution architecture semantics.
- ADR-0043 v0.2.0 is authoritative for concrete current repository topology, project boundaries, paths, project names, and dependency layout.
- Where concrete topology statements conflict, ADR-0043 prevails.
- ADR-0002 concrete materialization examples are partially superseded only where they conflict with ADR-0043.

DR-0002 was accepted on 2026-08-03.

Accepted completeness rule:

- A Rule Set is complete for a declared release scope when every advertised capability, mechanic, workflow, artifact, validation, test, localization requirement, security requirement, and compatibility promise within that scope is implemented and verified.
- Completeness does not require implementing the entire source RPG system.
- Every Rule Set release declares supported scope, supported capabilities, excluded mechanics, disabled operations, known limitations, compatibility boundaries, and evidence and validation status.
- A current slice or MVP may be promoted only when advertised scope is internally complete, exclusions are explicit and enforceable, unsupported features are not presented as supported, required tests and evidence pass, and package metadata plus public documentation match actual behavior.

DR-0003 was accepted on 2026-08-03.

Accepted lifecycle rule:

- Chronicle uses one shared normative Rule Set lifecycle across RFC-0027 and SPEC-0001: proposed, source-registered, slice-defined, extracted, modeled, structurally-valid, substantively-reviewed, decision-set-finalized, evidence-complete, promotion-eligible, promoted, published, installed, enabled, active, deprecated, withdrawn.
- Promotion means the package satisfies the declared quality gate for a release scope.
- Publication means an artifact was distributed through an approved channel.
- A package may be promotion-eligible or promoted without being published.
- Publication never implies installation, enablement, or activation.

DR-0004 was accepted on 2026-08-03.

Accepted materialization rule:

- SPEC-0001 defines the normative mapping between documentation prototype, package source, packaged artifact, and installed artifact.
- A documentation prototype is review and authoring evidence. It is never executable, packaging, installation, activation, Campaign-binding, promotion, or publication authority merely because it exists in the repository.
- Progression between materialization roles requires explicit transformation, identity preservation, fingerprints, validation, reconciliation, and accepted evidence.
- Required evidence families cover structural validation, substantive review, finalized decisions, source provenance, localization, fixtures, executable tests, security, reconciliation, compatibility, migration when applicable, and promotion readiness.

## Principal Findings

Found 3 material contradictions or authority gaps:

1. Resolved by DR-0001: ADR-0002 concrete topology examples defer to ADR-0043 v0.2.0 concrete repository topology where they conflict. ADR-0002 remains architecture-semantics authority.
2. Resolved by DR-0002 and DR-0003: RFC-0027 and MVP-scope language now define Rule Set completeness as complete declared release scope, not complete source-system implementation; RFC-0027 and SPEC-0001 now define shared lifecycle, promotion, and publication semantics.
3. Resolved by DR-0004: SPEC-0001 now maps documentation prototype, package source, packaged artifact, and installed artifact roles so future materialization does not confuse prototype evidence with executable package source, installed package artifacts, Campaign bindings, or publication authority.

## Werewolf Lessons Evaluated

Werewolf demonstrates contracts that should be promoted into the generic documentation surface:

- source registration and source/artifact fingerprints;
- strict separation between extraction facts and substantive prototype/package decisions;
- current-slice boundaries and future-slice exclusions;
- subset catalog states rather than pretending all catalogs are complete;
- unresolved decisions, issues, blockers, and promotion impact tracking;
- executable tests with fixtures, reports, environment metadata, and fingerprints;
- runtime security enforcement for package isolation and no-network requirements;
- localization validation as a readiness gate;
- evidence freshness and stale-evidence handling;
- reconciliation across catalogs, fixtures, tests, localization, security, and evidence;
- promotion eligibility separated from runtime publication;
- declarative prototype artifacts separated from the executable Rule Set package implementation.

## Architecture Decisions Requiring Confirmation

No decision requests remain open from this reconciliation package.

## Missing Specifications

No missing specifications from the four reconciliation decision requests remain unresolved. Runtime implementation details, serialization choices, installation layouts, and Campaign-binding mechanics remain outside the scope of this reconciliation unless separately authorized.

## Repository Materialization Assessment

Repository materialization is safe to plan against ADR-0043, RFC-0027, and SPEC-0001.

The topology-authority blocker MAT-001 is resolved by DR-0001. The MVP completeness blocker MAT-005 is resolved by DR-0002. The lifecycle blocker MAT-003 is resolved by DR-0003. The prototype-to-package materialization, evidence-family, and prototype-authority blockers MAT-002, MAT-004, and MAT-006 are resolved by DR-0004.

This reconciliation did not create application source projects, executable Rule Set package projects, package artifacts, installed artifacts, Campaign bindings, or publication records.
