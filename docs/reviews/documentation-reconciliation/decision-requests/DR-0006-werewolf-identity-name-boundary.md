---
id: DR-0006
title: Werewolf Identity Name Boundary
status: accepted
accepted_option: Option A
accepted_date: 2026-08-06
---

# DR-0006: Werewolf Identity Name Boundary

## Decision Record

Status: accepted.

Accepted option: Option A.

Effective date: 2026-08-06.

Decision:

This document records a new explicit Chronicle project decision. It is not a historical approval from the prototype review process. The prototype `creation-mechanics-review.json` remains `overallDecision: not-approved` with all mechanics `candidate-confirmed`. The prototype `identity.json` remains `publicationStatus: candidate` with `semanticReviewStatus: pending` and `implementationReviewStatus: pending`. No pre-existing approved artifact authorizes the executable behavior defined below.

Chronicle hereby approves the following identity-name boundary for the current executable slice:

- `character.identity.name` is the only identity field required by the current executable slice.
- `character.identity.name` is user-authored.
- `character.identity.name` is stored as a dedicated nullable `IdentityName` property on `WerewolfInitializedCharacterState`.
- `character.identity.name` is not stored in `NarrativeFields`.
- Accepted value is trimmed text with length 1 through 120 characters.
- Empty, whitespace-only, and values longer than 120 characters are rejected.
- Operation key is `character-creation.set-identity-name`.
- The operation uses `ExpectedDraftVersion` and increments `DraftVersion` exactly once on success.
- It preserves all unrelated draft state.
- Successful execution removes `set-identity-name` from `RequiredNextSteps`.
- Changing the name after other creation steps is allowed.
- `player-name`, `chronicle-name`, `pack-name`, `concept`, `nature`, and `demeanor` are not required by IMPLEMENT-020.
- `nature` and `demeanor` remain blocked.
- Optional identity and narrative fields remain deferred.
- IMPLEMENT-020 exposes a reusable identity-name-required validation rule, but does not implement `complete-character`, snapshot, freeze, persistence, or activation.
- Renown is outside scope and remains behaviorally inactive.

## Scope (IMPLEMENT-020)

Approved for implementation:

- Add `string? IdentityName` to `WerewolfInitializedCharacterState`.
- Initialize `IdentityName` to `null` in draft creation.
- Add `set-identity-name` to `RequiredNextSteps` during draft initialization.
- Implement `character-creation.set-identity-name` operation with typed request/result/finding contracts.
- Validate missing draft, uninitialized draft, stale version, missing input, whitespace-only input, and length above 120.
- Trim valid input before storing.
- Increment `DraftVersion` exactly once on success.
- Remove `set-identity-name` from `RequiredNextSteps` deterministically.
- Preserve all unrelated draft state and structural Renown state.
- Register and dispatch `character-creation.set-identity-name` through capability `character-creation`.
- Expose canonical runtime input/output keys.
- Add the new production file to both package-source required and declared resource collections.
- Implement a reusable pure validation rule reporting canonical blocking code `character.completion.identity.name-required`.

## Exclusions (IMPLEMENT-020)

- Optional identity fields (`player-name`, `chronicle-name`, `pack-name`, `concept`, `nature`, `demeanor`).
- `nature` and `demeanor` archetype catalogs and mechanics.
- Final character completion, snapshot, freeze, persistence, or activation.
- IMPLEMENT-021 (completion validation) dependencies.
- Renown behavior of any kind.

## Governing Authority

- `docs/reviews/werewolf-executable-completeness/discrepancy-register.json` WEC-007.
- `docs/reviews/werewolf-executable-completeness/executable-completeness-matrix.json` "narrative and identity fields required for completion" row.
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/character-model/fields/identity.json` (prototype evidence only).
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/character-model/validation/character-completion.json` (prototype evidence only).
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/character-creation/operation-index.json` (prototype evidence only; operation boundary not yet finalized).
- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype/reviews/creation-mechanics-review.json` (prototype evidence only; overall not-approved).
- SPEC-0001 and DR-0004 (materialization and validation contracts).

## Governance

- New decision request: required (this document).
- Existing decision set reopened: no.
- Decision set artifact: this document.
- Review record artifact: to be attached after creation-mechanics review advances.
- Decision boundary: approves only IMPLEMENT-020 identity-name state, operation, and required-name validation rule; does not approve optional identity fields, completion validation, completion operation, persistence, snapshot, freeze, activation, or Campaign binding.
