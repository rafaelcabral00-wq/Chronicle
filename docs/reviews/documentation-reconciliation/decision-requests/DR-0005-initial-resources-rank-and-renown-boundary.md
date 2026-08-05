---
id: DR-0005
title: Initial Resources, Rank, and Renown Boundary
status: accepted
accepted_option: Option A
accepted_date: 2026-08-05
---

# DR-0005: Initial Resources, Rank, and Renown Boundary

## Decision Record

Status: accepted.

Accepted option: Option A.

Effective date: 2026-08-05.

Decision:

This document records a new explicit Chronicle project decision. It is not a historical approval from the prototype review process. The prototype creation-mechanics-review remains `overallDecision: not-approved` with all mechanics `candidate-confirmed`. The rank-review and auspice-review carry internal inconsistencies (`review` sub-block: `decisionStatus: draft`, `taskCompletionStatus: not-eligible`, `evidenceStatus: missing`). No pre-existing approved artifact authorizes the executable behavior defined below.

Chronicle hereby approves the following deterministic initial values and operation boundary for the current executable slice:

- Race → Gnosis: homid 1, metis 3, lupus 5.
- Auspice → Rage: ragabash 1, theurge 2, philodox 3, galliard 4, ahroun 5.
- Tribe → Willpower: glass-walkers 3 (only executable Tribe in current slice).
- Rank: `character.rank.cliath`, numeric rank 1, system-assigned.
- Operation boundary: one atomic operation after required classifications are selected.
- Operation key: `character-creation.initialize-resources-and-rank`.

This decision applies only to IMPLEMENT-019A. IMPLEMENT-019B remains blocked.

## Scope (IMPLEMENT-019A)

Approved for implementation:

- Deterministic derivation of Gnosis from Race.
- Deterministic derivation of Rage from Auspice.
- Deterministic derivation of Willpower from Tribe (Glass Walkers only).
- System-assigned initialization of Rank to `character.rank.cliath` / numeric rank 1.
- One atomic operation `character-creation.initialize-resources-and-rank` executed after Race, Auspice, and Tribe classifications are selected (and Metis deformity is selected when applicable).
- Operation preconditions: draft initialized, draft version matches, Race/Auspice/Tribe present, Metis deformity present when Race is Metis.
- Operation postconditions: Resources dictionary populated with current and permanent values; Rank and RankValue populated; draft version incremented exactly once; RequiredNextSteps updated.

## Exclusions (IMPLEMENT-019A)

The following are explicitly excluded from this decision:

- All Renown initialization, interpretation, validation, serialization as behavior, findings, or next steps.
- Ragabash free-combination Renown interaction.
- Initial temporary Renown values for any Auspice.
- Fixed permanent Renown allocations for Philodox or any other Auspice.
- The `select-ragabash-renown` operation or any equivalent Renown-selection operation.
- Completion requirements involving Renown (deferred to the future completion-validation work package).
- Per-classification recalculation alternative (this decision adopts the atomic-operation boundary; the alternative remains available for future review but is not approved here).
- Freebie-points interaction with Resources or Rank.

## Renown Structural Presence

Renown may remain structurally present in the draft contract with null/unset values. IMPLEMENT-019A must not initialize, interpret, validate, serialize as behavior, emit findings, or emit next steps for Renown. Temporary Renown semantics are unresolved. Fixed Renown allocations are unresolved. select-ragabash-renown is not authorized.

## IMPLEMENT-019B Blocked Boundary

IMPLEMENT-019B is blocked. No Renown allocation is approved. No temporary Renown semantics are approved. No select-ragabash-renown operation is approved. No fixed Philodox or other Auspice Renown is approved.

## Completion Requirements Involving Renown

This decision does not define whether character completion requires Renown. That belongs to the future completion-validation work package.

## Authoritative Sources

- `docs/reviews/werewolf-executable-completeness/discrepancy-register.json` WEC-006.
- `docs/reviews/werewolf-executable-completeness/executable-completeness-matrix.json` "initial resources" and "Rank" rows.
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/reviews/catalog-identity-decisions.json` (approved-current-slice-reference-baseline).
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/reviews/rank-review-record.json` (character.rank.cliath identity and creation-binding findings).
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/reviews/auspice-review-record.json` (initial-rage findings for supported Auspices).
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/reviews/tribe-review-record.json` (glass-walkers.initial-willpower finding).
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/reviews/creation-mechanics-review.json` (candidate-confirmed mechanics; not approved).
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/character-model/creation/profile.json` (initializationPlan order 10).
- `docs/specs/SPEC-0001-rule-set-package-artifact-model-and-extraction-contract.md` (materialization role authority per DR-0004).
- `docs/reviews/documentation-reconciliation/decision-requests/DR-0004-prototype-materialization-and-validation-contracts.md` (prototype is evidence-only).

## Governance

- New decision request: required (this document).
- Existing decision set reopened: no.
- Decision set artifact: this document.
- Review record artifact: to be attached after creation-mechanics review advances.
- Decision boundary: approves only IMPLEMENT-019A deterministic Resources and Rank initialization; does not approve Renown, completion validation, identity fields, ledger, persistence, publication, installation, activation, or Campaign binding.
