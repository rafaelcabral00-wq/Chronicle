# Rule Set Mechanical Completeness Criteria

**Status:** Accepted  
**Applies to:** All Chronicle Rule Set packages  
**Depends on:** SPEC-0001, DR-0002

## 1. Purpose

This document defines the reusable criteria for a Rule Set package to claim:

mechanically-complete

These criteria apply to all Rule Sets, not only Werewolf.

## 2. Definitions

| Term | Meaning |
|------|---------|
| mechanical domain | A source-derived category of deterministic or randomness-involving mechanic that requires runtime representation, execution, or validation |
| declared release scope | The exact set of capabilities, catalogs, operations, and artifacts a package advertises as supported |
| executable | Implemented in compiled deterministic module and dispatcheable through runtime |
| complete | All required artifacts for the declared scope exist, are validated, and match |

## 3. Formal Criteria

A Rule Set package is mechanically-complete for a declared release scope only when all of the following are true.

### 3.1 Source Traversal

The package has performed and recorded a complete traversal of its registered source material.

Required evidence:

- source inventory document with file fingerprint, line count, heading count, and SHA-256
- mechanical-domain classification covering every extracted domain
- no unregistered source segment that contains deterministic mechanics for the declared scope

### 3.2 Extraction and Evidence Coverage

Every mechanical domain within the declared scope has extraction artifacts.

Required evidence:

- extraction documents or structured artifacts for each domain
- normalized rule statements linked to source segments
- prototype decisions stored separately from source truth
- provenance records for every candidate rule

### 3.3 Ambiguity and Contradiction Disposition

Every ambiguity and contradiction affecting the declared scope has an explicit disposition.

Required evidence:

- ambiguity register with severity, status, and resolution evidence
- contradiction register with selected interpretation or explicit deferral
- no ambiguity marked resolved without evidence
- no contradiction left open for declared-scope mechanics

### 3.4 Complete Catalogs

All catalogs required by the declared scope are complete and materialized.

Required evidence:

- catalog files for every required entity type
- no placeholder entries for declared-scope content
- canonical keys stable and reviewed
- localization covers all user-facing catalog strings

### 3.5 Executable Deterministic Mechanics

Every deterministic mechanic in the declared scope is implemented and tested.

Required evidence:

- compiled deterministic module with no build errors
- runtime operation for every declared capability
- atomic state transitions validated
- versioned immutable state updates where required
- package metadata accurately reflects supported and disabled operations

### 3.6 Runtime State Transitions

The runtime state model covers all required resource, track, and condition state for the declared scope.

Required evidence:

- state records or equivalent carry all required fields
- spend/recover/initialize operations are deterministic
- derived state is computed from source-derived rules, not invented
- no hidden state mutations

### 3.7 Randomness Boundary Compliance

Every mechanic involving randomness is explicitly bounded and source-derived.

Required evidence:

- dice pools, difficulty, success thresholds, and failure/botch rules are extracted
- specialization and extended/resisted test semantics are extracted
- no randomness implemented from general system knowledge without source authority
- randomness boundary is documented: what is random, what is deterministic, what is narrative

### 3.8 Tests

Required evidence:

- fixture-driven tests for every declared operation
- acceptance tests for every supported creation path or workflow
- regression tests for every remediated discrepancy
- focused test suite passes for the package

### 3.9 Package Metadata Accuracy

Required evidence:

- manifest declares exact supported capabilities
- manifest declares exact disabled capabilities and reasons
- manifest declares exact excluded mechanics
- localization does not advertise unsupported features
- capability status matches implementation and extraction state

### 3.10 Source-to-Runtime Traceability

Required evidence:

- every runtime operation can be traced to extraction evidence
- every catalog entry can be traced to source segment
- every validation rule can be traced to source statement or documented prototype decision
- no executable behavior lacks provenance

## 4. Completeness Levels

| Level | Meaning |
|-------|---------|
| source-system complete | Entire source RPG system is extracted, reviewed, and implemented |
| declared-scope complete | All advertised capabilities within one release scope are implemented and verified |
| executable complete | Declared-scope complete plus all tests pass and metadata is accurate |
| promotion eligible | Executable complete plus all review, security, localization, and publication gates pass |

## 5. Governance

A package may claim:

mechanically-complete

only for a specific declared release scope.

Claiming completeness for a broader scope than the declared release scope is a metadata overclaim and violates this criterion.

## 6. Review Requirements

Before claiming mechanically-complete, the package must have:

- extraction review
- semantic review
- implementation review
- fixture review
- provenance review
- terminology review
- security review
- localization validation
- reconciliation review

## 7. Minimal Artifacts

The minimum artifact set for a completeness claim:

```text
source inventory
mechanical-domain taxonomy
extraction artifacts per domain
ambiguity register
contradiction register
catalog files
runtime implementation
test suite
package manifest
localization files
completeness matrix or equivalent evidence
reconciliation review
```

## 8. Next Document

For application to a specific Rule Set, see the Rule Set's completeness matrix and report under `docs/reviews/<rule-set>-rule-set-completeness/`.
