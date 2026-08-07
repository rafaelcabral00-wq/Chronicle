---
id: DR-0007
title: Werewolf Character Completion Boundary
status: accepted
accepted_option: Option A
accepted_date: 2026-08-06
---

# DR-0007: Werewolf Character Completion Boundary

## Decision Record

Status: accepted.

Accepted option: Option A.

Effective date: 2026-08-06.

Decision:

This document records a new explicit Chronicle project decision. It is not a historical approval from the prototype review process. The prototype `character-completion.json` remains `publicationStatus: prototype-candidate` with `semanticReviewStatus: pending` and `implementationReviewStatus: pending`. No pre-existing approved artifact authorizes the executable behavior defined below.

Chronicle hereby approves the following character-completion boundary for the current executable slice:

- Completion validation is deterministic and Rule Set-owned.
- Completion evaluates the current immutable draft without mutating it.
- Only fields and stages approved in the executable current slice may block completion.
- `character.identity.name` (`IdentityName`) is required.
- Race, Auspice, Tribe, Metis deformity when applicable, initial Gifts, Attribute priorities/allocation, Ability priorities/allocation, Background allocation, Resources, and Rank must be complete.
- Renown does not block completion under IMPLEMENT-021 because it remains unresolved and behaviorally inactive under IMPLEMENT-019B.
- Optional identity/narrative fields do not block completion.
- Nature and demeanor do not block completion.
- Disabled additional Gift purchase does not block completion.
- Completion operation key is `character-creation.complete-character`.
- Completion uses `ExpectedDraftVersion`.
- Validation failure leaves the draft unchanged.
- Successful completion produces an immutable completed-character snapshot.
- Successful completion marks/finalizes the creation draft boundary.
- Completion must not persist through Chronicle infrastructure directly in this work package.
- The Rule Set returns deterministic completion output/evidence to Chronicle.
- Chronicle Core/Application owns persistence and aggregate activation.
- No database, filesystem, network, random, clock, or provider dependency is allowed in the Rule Set completion operation.
- Runtime output must contain enough deterministic state for Chronicle to persist/activate later.
- Package binding and Rule Set identity/version must be retained in the completed snapshot.
- Completion is idempotent with respect to the same already-completed state by explicit rejection with a deterministic canonical finding.

## Ownership Boundary

- **Rule Set owns**: deterministic mechanical validation, completion eligibility, completion snapshot data structure, validation findings, and canonical error codes.
- **Chronicle Core/Application owns**: persistence, aggregate activation, transaction coordination, ledger persistence, snapshot persistence, timestamps, and event emission.
- The Rule Set must not call database, filesystem, network, random, clock, provider, or persistence APIs.
- The Rule Set must return all deterministic state needed for Chronicle to persist/activate later.

## Scope (IMPLEMENT-021)

Approved for implementation:

- Pure completion validator that evaluates an immutable draft and returns deterministic findings.
- Typed completion request/result/finding contracts.
- Immutable completed-character snapshot contract containing approved character state, package binding, validation result, and deterministic metadata.
- `character-creation.complete-character` service.
- Runtime registration and dispatch for `character-creation.complete-character`.
- Package-source allow-list update for the new completion file.
- Deterministic idempotency: if the draft is already completed, return a deterministic `AlreadyCompleted` finding.

## Blocking Validation Families

The completion validator must report deterministic canonical findings for every missing or invalid mandatory current-slice element:

- Draft missing/uninitialized/stale
- Identity name missing/invalid
- Race missing
- Auspice missing
- Tribe missing
- Metis deformity missing when Race is Metis
- Initial Race Gift missing
- Initial Auspice Gift missing
- Initial Tribe Gift missing
- Attribute priorities missing
- Attribute allocation incomplete or invalid
- Ability priorities missing
- Ability allocation incomplete or invalid
- Background allocation incomplete or invalid
- Resources not initialized
- Rank not initialized
- Unresolved `RequiredNextSteps` that belong to approved mandatory current-slice work

## Explicitly Non-Blocking

- Renown (unresolved under IMPLEMENT-019B)
- Optional narrative fields (`character-concept`, `character-goals`, `character-relationships`)
- Nature and demeanor (archetype catalogs blocked)
- Additional Gift purchase (disabled)
- Runtime Gift effects (disabled)
- Generic dice
- Post-creation runtime

## Exclusions

- Database persistence
- Chronicle aggregate activation
- Creation ledger persistence
- Snapshot persistence
- Timestamp generation
- Event emission
- Combat, dice, runtime Gifts, or advancement
- Optional identity fields
- Renown behavior

## Governing Authority

- `docs/reviews/werewolf-executable-completeness/discrepancy-register.json` WEC-008
- `docs/reviews/werewolf-executable-completeness/executable-completeness-matrix.json` "completion validation prerequisites" row
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/character-model/validation/character-completion.json` (prototype evidence only)
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/character-creation/complete-character.json` (prototype evidence only)
- `docs/rule-sets/Chronicle.RuleSets/Werewolf/prototype/character-model/creation/creation-ledger.json` (prototype evidence only)
- `docs/rule-sets/Chronicle.RuleSets/Werewolf/prototype/character-model/creation/invalidations.json` (prototype evidence only)
- DR-0005 (resources, Rank boundary)
- DR-0006 (identity name boundary)
- SPEC-0001 and DR-0004 (materialization and validation contracts)

## Governance

- New decision request: required (this document).
- Existing decision set reopened: no.
- Decision set artifact: this document.
- Review record artifact: to be attached after creation-mechanics review advances.
- Decision boundary: approves only IMPLEMENT-021 completion validation, snapshot contract, and complete-character operation boundary; does not approve persistence, activation, ledger persistence, events, timestamps, optional identity fields, Renown, or post-creation runtime.
