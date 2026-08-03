---
id: DR-0002
title: MVP Rule Set Completeness
status: accepted
accepted_option: Option B
accepted_date: 2026-08-03
---

# DR-0002: MVP Rule Set Completeness

## Decision Record

Status: accepted

Accepted option: Option B.

Effective date: 2026-08-03.

Decision:

A Rule Set is complete for a declared release scope when every advertised capability, mechanic, workflow, artifact, validation, test, localization requirement, security requirement, and compatibility promise within that scope is implemented and verified.

Completeness does not require implementing the entire source RPG system.

Every Rule Set release must explicitly declare:

- supported scope;
- supported capabilities;
- excluded mechanics;
- disabled operations;
- known limitations;
- compatibility boundaries;
- evidence and validation status.

A current slice or MVP may be promoted only when:

- its advertised scope is internally complete;
- exclusions are explicit and enforceable;
- no unavailable feature is presented as supported;
- required tests and evidence for that scope pass;
- package metadata and user-facing documentation match actual behavior.

Completeness distinctions:

- source-system completeness means the entire source RPG system is implemented;
- declared-scope completeness means all advertised support within one release scope is implemented and verified;
- implementation completeness means the declared scope has working package behavior, not only declarative artifacts;
- validation completeness means required evidence, tests, localization checks, security checks, and compatibility checks for the declared scope pass;
- publication status remains separate from completeness and is not fully defined by this decision.

Werewolf current-slice evidence:

- 46/46 required identities finalized;
- required evidence accepted;
- additional Gift purchase disabled;
- runtime Gift effects disabled;
- eligible-not-published;
- complete within declared scope, not complete for the entire Werewolf system.

## Context

Several product and architecture documents say the MVP should deliver one complete Rule Set rather than multiple partial Rule Sets. The Werewolf baseline demonstrates a practical current-slice prototype: character-model and character-creation capabilities are candidate-ready, while dice, post-creation operations, runtime Gift execution, implementation, security proof, and publication remain blocked or not started.

The decision is not whether Chronicle should ship a poor partial experience. The decision is what "complete Rule Set" means for MVP acceptance.

## Authoritative Sources

- `docs/rfcs/RFC-0002-Product-Vision.md`: says the MVP only needs one complete Rule Set and that a second incomplete Rule Set is less valuable than one reliable first experience.
- `docs/rfcs/RFC-0042-Official-MVP-Scope-and-Acceptance-Criteria.md`: lists one complete Rule Set package and excludes multiple incomplete Rule Sets.
- `docs/rfcs/RFC-0027-Rule-Set-Package-Architecture.md`: says the MVP may statically register one complete Rule Set package.
- `docs/specs/SPEC-0001-rule-set-package-artifact-model-and-extraction-contract.md`: says extracted content is candidate until reviewed and the sourcebook does not become executable authority automatically.
- `docs/extraction/werewolf-3e/EXTRACTION-0001-source-inventory.md`: labels the first slice as a prototype and blocks final publication approval on missing metadata and legal/review work.
- `docs/extraction/werewolf-3e/EXTRACTION-0003-character-creation-slice.md`: separates prototype completion blockers from official release blockers.
- `docs/extraction/werewolf-3e/EXTRACTION-0004-ambiguities-and-conflicts.md`: says prototype resolutions do not automatically become official package resolutions.
- `docs/extraction/werewolf-3e/EXTRACTION-0005-contract-findings.md`: says a prototype may contain valid Character creation artifacts but incomplete combat.

## Contradiction or Gap

This is a real semantic gap and possible contradiction.

If "complete Rule Set" means complete source-system catalog and full game mechanics, the Werewolf current-slice baseline disproves readiness for MVP materialization. If it means complete advertised MVP-supported slice with explicit exclusions, the docs need to say that directly.

## Options

### Option A: Complete means complete game-system package

MVP requires full Werewolf package coverage across all relevant game mechanics before being considered Rule Set complete.

Consequences:

- Strongest user expectation alignment if the MVP claims full Werewolf support.
- Greatly expands MVP scope.
- Delays materialization until many future-slice mechanics are extracted, reviewed, implemented, tested, localized, and legally cleared.
- Conflicts with Werewolf evidence that current-slice boundaries are necessary.

### Option B: Complete means complete advertised MVP/current slice

MVP requires one Rule Set package that completely supports the capabilities it advertises for MVP, while future-slice mechanics remain explicitly excluded, blocked, or not advertised.

Consequences:

- Preserves the principle of one reliable first experience.
- Aligns with Werewolf's current-slice baseline.
- Requires lifecycle, capability declaration, exclusion, and publication language so partial coverage is never hidden.
- Keeps MVP scope feasible.

### Option C: Drop the one complete Rule Set principle

Allow multiple partial Rule Sets as long as each has clear warnings.

Consequences:

- Maximizes experimentation.
- Conflicts with product vision and MVP scope.
- Increases support, testing, and community confusion.
- Weakens architecture validation because no package proves a coherent flow.

## Recommendation

Recommend Option B.

It is the smallest option consistent with Chronicle's principles and MVP scope: one reliable first experience, no hidden partial support, no Werewolf-specific Core behavior, and clear current-slice boundaries.

## Affected Documents

- `docs/rfcs/RFC-0002-Product-Vision.md`
- `docs/rfcs/RFC-0042-Official-MVP-Scope-and-Acceptance-Criteria.md`
- `docs/rfcs/RFC-0027-Rule-Set-Package-Architecture.md`
- `docs/specs/SPEC-0001-rule-set-package-artifact-model-and-extraction-contract.md`
- `docs/extraction/werewolf-3e/EXTRACTION-0003-character-creation-slice.md`
- `docs/extraction/werewolf-3e/EXTRACTION-0005-contract-findings.md`

## Werewolf Evidence

- `prototype-readiness.json` reports `prototypeIntegrationReady: true`, `implementationReady: false`, `promotionReady: false`, and `runtimePublicationReady: false`.
- `prototype-readiness.json` lists character-model, character-creation, character-validation, character-sheet, and fixture-driven-tests as prototype-candidate capabilities, while additional Gift purchase, generic dice, post-creation operations, and runtime Gift execution are blocked or not started.
- `reviews/current-slice-boundary-review.json` demonstrates explicit current-slice boundaries.

## Implementation Impact

Option B requires capability flags, current-slice support declarations, blocked capability handling, and UI/runtime prevention of unsupported mechanics being advertised or invoked. It does not require source project creation now.

## Community Rule Set Impact

Community packages would be allowed to publish complete advertised slices only if their unsupported capabilities are explicit and non-selectable. This avoids all-or-nothing game-system completeness while still protecting users from vague partial packages.

## Required Document Mechanism

RFC amendment is required for RFC-0002, RFC-0042, and RFC-0027. SPEC-0001 may need a follow-on amendment after DR-0003 and DR-0004.

## Acceptance Questions

- Should MVP completeness mean complete advertised MVP/current slice rather than complete source-system catalog?
- Must unsupported future-slice capabilities be blocked and excluded from supported capability declarations?
- Should MVP acceptance criteria explicitly require no advertised capability without review, implementation, tests, localization, security, and evidence gates?
