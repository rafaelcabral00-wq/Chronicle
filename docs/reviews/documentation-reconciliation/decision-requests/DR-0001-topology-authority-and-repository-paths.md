---
id: DR-0001
title: Topology Authority and Repository Paths
status: accepted
accepted_option: Option B
accepted_date: 2026-08-03
---

# DR-0001: Topology Authority and Repository Paths

## Decision Record

Status: accepted

Accepted option: Option B.

Effective date: 2026-08-03.

Decision:

- ADR-0002 remains authoritative for repository and solution architecture semantics.
- ADR-0043 v0.2.0 is authoritative for the concrete current repository topology, project boundaries, paths, project names, and dependency layout.
- Where concrete topology statements conflict, ADR-0043 prevails.
- ADR-0002 explicitly defers concrete materialization details to ADR-0043.

This is a partial supersession of ADR-0002 concrete topology examples only. It does not supersede ADR-0002 architectural semantics, dependency-direction rules, or boundary decisions.

Repository materialization consequence:

- Future repository materialization must use ADR-0043 v0.2.0 for concrete paths and project names.
- Older ADR-0002 path examples remain historical architecture context where they do not conflict with ADR-0043.
- No source directories, projects, solution files, or repository root files are created by this decision.

## Context

The reconciliation found that ADR-0002 still contains concrete project and repository examples that place Rule Set implementation projects under the general source tree, while ADR-0043 v0.2.0 defines a later concrete topology with `rule-sets/` as the repository area for package-facing contracts and implementations.

This is mostly an authority clarity issue. ADR-0043 already says it translates the architecture into concrete folder and project names, while ADR-0002 remains the architectural and dependency-direction authority.

## Authoritative Sources

- `docs/adrs/ADR-0002-Repository-and-Solution-Structure.md`: defines the initial repository and solution structure, dependency direction, project boundaries, and contribution constraints.
- `docs/adrs/ADR-0043-Repository-Topology-Project-Boundaries-and-Solution-Structure-v0.2.0.md`: states that ADR-0002 defines architectural layers and dependency direction, while ADR-0043 defines folders, project names, solution organization, test placement, and repository conventions.
- `docs/adrs/ADR-0043-Repository-Topology-Project-Boundaries-and-Solution-Structure-v0.2.0.md`: records the DR-0001 boundary that ADR-0002 wins for architecture semantics and ADR-0043 wins for concrete topology statements.
- `docs/specs/SPEC-0001-rule-set-package-artifact-model-and-extraction-contract.md`: uses `rule-sets/Chronicle.RuleSets.<SystemName>/` as recommended repository layout.
- Werewolf baseline: `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype` exists as documentation-hosted prototype evidence, not as an application source project.

## Contradiction or Gap

Actual contradiction is limited to concrete path examples. The architecture authority is not actually contradictory if ADR-0002 is treated as dependency-direction authority and ADR-0043 as topology authority.

The missing detail is an explicit cross-reference from ADR-0002 to ADR-0043 explaining that older concrete topology examples defer to ADR-0043 v0.2.0.

## Options

### Option A: ADR-0002 remains sole topology authority

Use ADR-0002 examples as the materialization guide and treat ADR-0043 as secondary.

Consequences:

- Preserves early document simplicity.
- Reintroduces the conflict ADR-0043 was created to resolve.
- Risks creating Rule Set source projects in the wrong place.
- Undercuts SPEC-0001 repository layout and Werewolf package autonomy lessons.

### Option B: ADR-0043 controls concrete topology; ADR-0002 controls architecture semantics

Use ADR-0043 v0.2.0 for folders, project names, solution organization, and test placement. Use ADR-0002 for architecture boundaries and dependency direction.

Consequences:

- Smallest change because it follows ADR-0043's own authority statement.
- Avoids changing accepted architecture.
- Requires minor metadata/navigation amendments to ADR-0002 and possibly doc indexes.
- Gives repository materialization a clear path without creating source projects now.

### Option C: Create a new ADR that supersedes both ADR-0002 and ADR-0043

Replace the authority relationship with a single consolidated repository/architecture ADR.

Consequences:

- Removes ambiguity long-term.
- High review cost.
- Risks changing architecture under the guise of cleanup.
- Too large for the current reconciliation scope.

## Recommendation

Recommend Option B.

It is the smallest option consistent with Chronicle's principles: preserve architecture decisions, make boundaries visible, keep Rule Set packages autonomous, and avoid premature restructuring.

## Affected Documents

- `docs/adrs/ADR-0002-Repository-and-Solution-Structure.md`
- `docs/adrs/ADR-0043-Repository-Topology-Project-Boundaries-and-Solution-Structure-v0.2.0.md`
- `docs/specs/SPEC-0001-rule-set-package-artifact-model-and-extraction-contract.md`
- Documentation indexes or reading-order documents, if present later.

## Werewolf Evidence

- `docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype` proves there is a documentation-hosted prototype baseline distinct from application source.
- `prototype-work-status.json` shows package-loading, runtime, tests, security, and packaging phases are not source materialization yet.

## Implementation Impact

Option B means future source projects should follow `rule-sets/Chronicle.RuleSets.Werewolf/` and not older `src/Chronicle.RuleSets.Werewolf` examples. It does not create source projects.

## Community Rule Set Impact

Clarifies that community Rule Sets should follow the same package-facing topology and not depend on Chronicle Core internals. This improves portability and keeps package autonomy visible.

## Required Document Mechanism

RFC amendment or ADR amendment only. No new ADR is required if Option B is accepted.

## Acceptance Questions

- Should ADR-0043 v0.2.0 be the concrete topology authority while ADR-0002 remains architecture authority?
- Should ADR-0002 receive an explicit note that conflicting concrete path examples defer to ADR-0043 v0.2.0?
- Should future Rule Set source materialization use `rule-sets/Chronicle.RuleSets.<SystemName>/`?
