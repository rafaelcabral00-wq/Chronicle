---
id: EXTRACTION-0004
title: Werewolf 3e Character Creation Ambiguities and Conflicts
status: Draft
version: 0.1.0
owner: Chronicle Team
last_updated: 2026-08-02
category: Rule Set Extraction
source_document_id: source.werewolf3e.cleaned-ptbr
source_document_version: 1
source_fingerprint: a4302e2938a137fb42d154c45decd43e02a19a2ba0eb3030b6eb1de942dab64a
slice_id: werewolf3e.character-creation.slice-001
normalization_policy: FidelityWithNormalization
publication_status: candidate-only
depends_on:
  - SPEC-0001
  - EXTRACTION-0001
  - EXTRACTION-0002
  - EXTRACTION-0003
related_to:
  - EXTRACTION-0005
---

> **"An unresolved rule is a visible question, not an invisible implementation choice."**

# Werewolf 3e Character Creation Ambiguities and Conflicts

## 1. Purpose

This document records uncertainties, incomplete definitions, repeated statements, and possible conflicts affecting the first Werewolf Character creation vertical slice.

It prevents the prototype from:

- inventing missing mechanics;
- choosing silently between competing interpretations;
- turning narrative descriptions into mechanical restrictions;
- applying general system knowledge not present in the registered source;
- hiding source limitations inside implementation code.

This report is not a rules correction document.

It describes what the submitted cleaned source does and does not establish clearly enough for package publication.

## 2. Status Vocabulary

Each issue uses one of these states:

```text
Open
SourceSearchRequired
SemanticReviewRequired
ImplementationDecisionRequired
ExternalVerificationRequired
ResolvedForPrototype
ResolvedForPublication
Deferred
Rejected
```

Severity:

```text
Critical
High
Medium
Low
```

A prototype resolution does not automatically become an official package resolution.

## 3. Resolution Principles

For every ambiguity:

1. preserve the source statements;
2. identify the exact uncertain point;
3. list candidate interpretations;
4. describe implementation impact;
5. select no interpretation silently;
6. distinguish prototype policy from source truth;
7. retain provenance for the final decision.

## 4. Critical Issues

## A-001 — Exact General Dice Resolution Algorithm

```yaml
ambiguityId: ambiguity.werewolf3e.dice.basic-resolution
severity: Critical
status: Resolved
classification:
  - MechanicalResolutionRule
```

### Supported by the source inventory

The source contains sections about:

- Dice pools;
- difficulty;
- successes;
- results of 1;
- failure;
- botches;
- tests using Attribute + Ability.

### Resolved algorithm (source lines 2703–2760)

This is the **ordinary non-specialized** test resolution algorithm. Specialization
behavior is deferred to the Specialties completion domain (A-002).

```text
1. If DiceQuantity <= 0:
   - The character cannot attempt the action (source line 2720).
   - Return ZeroPoolCannotAttempt error.

2. Validate Difficulty is between 2 and 10 (source line 2781: "Valor de 2 a 10").

3. For each die in DiceValues:
   a. If die < 1 or die > 10: return InvalidDieFace error.
   b. Count rawSuccesses: die >= Difficulty (source line 2781: "igualar ou superar este número em pelo menos um dado").
   c. Count ones: die == 1.

4. Apply base cancellation rule (source line 2705: "Cada dado que mostra o valor 1 anula um sucesso obtido na jogada"):
   - finalSuccesses = max(0, rawSuccesses - ones)

5. Determine result classification:
   a. If finalSuccesses > 0: Success.
   b. If finalSuccesses == 0 and onesCount > 0: Botch (source line 2706: "Ocorre quando um teste não resulta em nenhum sucesso e apresenta um ou mais 1s").
   c. If finalSuccesses == 0 and onesCount == 0: Failure.

6. Return result with:
   - SuccessCount = finalSuccesses
   - RawSuccesses = rawSuccesses
   - OnesCount = onesCount
   - FailureClassification = "NoSuccesses" (for failures)
   - BotchClassification = "CriticalFailure" (for botches)
   - InterpretationStatus = "success" | "failure" | "botch" | "zero-pool"
```

### Source-derived answers to implementation questions

```text
valid difficulty range: 2 to 10 inclusive (source line 2781)
success threshold: die >= Difficulty (source line 2781)
effect of each result of 1: cancels one raw success (source line 2705)
whether 1 cancels a success: yes, in the base non-specialized algorithm
definition of failure: finalSuccesses == 0 and onesCount == 0
definition of botch: finalSuccesses == 0 and onesCount > 0 (source line 2706)
behavior when the initial pool is zero: cannot attempt action (source line 2720)
behavior when modifiers reduce a pool below one: clamped to 0 at test definition; interpretation returns ZeroPoolCannotAttempt
automatic successes: not applied by interpretation service (pre-roll decision per source line 2709)
automatic failure: not explicitly defined in source; zero successes with no ones is a normal failure
result of 10 without specialization: counts as one success (10 >= Difficulty for any valid Difficulty)
specialization behavior: NOT implemented in RULESET-COMPLETION-004; deferred to Specialties domain (A-002)
```

### Worked examples (base non-specialized algorithm)

| Dice | Diff | Raw successes | Ones | Final successes | Outcome |
|------|------|---------------|------|-----------------|---------|
| [6] | 6 | 1 | 0 | 1 | Success |
| [6,1] | 6 | 1 | 1 | 0 | Botch |
| [6,6,1] | 6 | 2 | 1 | 1 | Success |
| [1] | 6 | 0 | 1 | 0 | Botch |
| [2,3] | 6 | 0 | 0 | 0 | Failure |
| [10,1] | 6 | 1 | 1 | 0 | Botch |
| [10,10,1] | 6 | 2 | 1 | 1 | Success |
| [] | 6 | 0 | 0 | 0 | ZeroPoolCannotAttempt |

Note: [10,1] yields 1 raw success (10 >= 6, 1 < 6), not 2. The 10 does not grant an additional die in the base algorithm; that is specialization behavior.

### Implementation notes

- The interpretation service receives already-rolled dice values from the Chronicle (DR-0008: Chronicle owns RNG).
- The base algorithm contains NO specialization logic.
- Specialization eligibility, selection, applicability, and dice provenance are deferred to the Specialties completion domain (A-002).

### Impact

Resolved blocks:

```text
dice/resolution/basic-success-test.json
WerewolfActionRollInterpretationService (base algorithm)
WerewolfActionTestDefinitionService (base test definition)
WerewolfActionRollInterpretationTests (base algorithm cases)
WerewolfActionTestDefinitionTests (base test definition cases)
```

---

## A-002 — Specialization Resolution

```yaml
ambiguityId: ambiguity.werewolf3e.dice.specialization
severity: Critical
status: PartiallyResolved
classification:
  - MechanicalResolutionRule
```

### Source-derived semantics (NOT executable in RULESET-COMPLETION-004)

The cleaned source associates specialization with a rating of 4 or higher and describes additional Dice behavior for results of 10 (source line 1083, 1085, 1086).

### What is known from source

```text
Specialization eligibility: Ability rating >= 4 grants the right to choose a specialization (source line 1083: "adquire o direito de escolher").
Specialization selection requirement: Rating >= 4 does NOT automatically grant specialization. The character must actively select a specific specialization.
Specialization applicability: Benefits apply only when the test is within the chosen specialization area (source line 1085: "dentro da sua área de especialização").
10-again/additional-die behavior: Each rolled 10 grants +1 additional die (source line 1085: "cada número 10 obtido nos dados permite rolar +1 dado adicional").
Special handling of 1s on additional dice: 1s on added specialization dice do not cancel already-obtained successes (source line 1086).
```

### Unresolved (deferred to Specialties domain)

```text
How is a specific specialization selected and recorded? Deferred to Specialties completion domain.
How is applicability to a specific test determined? Deferred to Specialties completion domain.
What is the continuation roll protocol for 10-again? Deferred to Specialties completion domain.
How are original and added dice distinguished (provenance)? Deferred to Specialties completion domain.
Does chaining have any limit? Source does not explicitly limit, but exact protocol is deferred.
```

### Implementation status

- **RULESET-COMPLETION-004:** Does NOT implement executable Specialties behavior.
- The base dice algorithm (A-001) is implemented without any specialization approximation.
- Any `HasSpecialization` field previously present in contracts has been removed from the executable path to prevent unsourced mechanical benefits.
- The Specialties completion domain owns: chosen specialty character state, applicability to current test, continuation roll protocol, dice provenance, and exact chaining behavior.

### Impact

Partially resolved blocks:

```text
A-002 source semantics extracted
A-002 executable implementation remains blocked on Specialties domain
```

### Required resolution

Capture and review the complete specialization section.

---

## A-003 — Lupus Restricted Abilities and Freebie Spending

```yaml
ambiguityId: ambiguity.werewolf3e.creation.lupus-restricted-abilities
severity: High
status: ResolvedFromSourceCrossReference
resolution: OptionA
decisionAuthority: human
classification:
  - CharacterCreationRule
  - MechanicalConstraint
```

### Source-derived statements

Source line 547 states Lupus cannot apply "pontos iniciais" (initial points) to 9 restricted Abilities during creation. The same line states these Abilities "podem ser adquiridos posteriormente com pontos de bônus/experiência via treino" (can be acquired later with bonus points/experience via training).

### Resolution

**Resolved as Option A — Creation-time freebies permitted.**

The source explicitly distinguishes between:
- "pontos iniciais" (base allocation, Step 3) — restricted for Lupus
- "pontos de bônus" (creation-time bonus points, Step 5) — explicitly named as a later acquisition mechanism
- "experiência" (post-creation experience) — post-creation acquisition

The phrase "posteriormente com pontos de bônus/experiência via treino" explicitly permits Lupus to acquire restricted Abilities using creation-time bonus points (Step 5) after base allocation is complete.

"Via treino" is interpreted as narrative flavor explaining the acquisition path, not a mechanically testable prerequisite during character creation.

### Source locators

- Base restriction: source line 547
- Bonus Points definition: source line 938-939
- Bonus Point spending example: source line 1063
- Human confirmation: DR-0011-equivalent decision for A-003, Option A selected

### Implementation

- Base allocation: Lupus restricted Abilities rejected (RULESET-COMPLETION-006)
- Freebie stage: Lupus restricted Abilities allowed (RULESET-COMPLETION-007)
- Post-creation: no further restriction
- Completion validator: permits nonzero restricted Ability ratings for Lupus

### Impact

Resolved. Affects:
- Ability allocation validator;
- freebie operation (when materialized);
- Lupus fixtures;
- Character completion validation.

---

## A-004 — Complete Tribe Mechanical Records

```yaml
ambiguityId: ambiguity.werewolf3e.creation.tribe-record-completeness
severity: Critical
status: SourceSearchRequired
classification:
  - CharacterCreationRule
  - CatalogDefinition
```

### Required data per Tribe

```text
initial Willpower
initial Tribe Gift candidates
Background restrictions
required Background minimums
prohibited Backgrounds
eligibility restrictions
```

### Unresolved question

Does the cleaned source provide a complete and internally consistent mechanical record for every playable Tribe?

Narrative descriptions alone are insufficient.

### Impact

No Tribe may be enabled unless all mandatory mechanical fields are present or explicitly marked not applicable.

### Required resolution

Build a Tribe completeness matrix from the source.

---

## A-005 — Exact Initial Renown by Auspice

```yaml
ambiguityId: ambiguity.werewolf3e.creation.initial-renown
severity: High
status: SourceSearchRequired
classification:
  - ResourceRule
  - CharacterCreationRule
```

### Current candidates

```text
Ragabash:
    three points in a permitted combination

Theurge:
    Wisdom 3

Philodox:
    Honor 3

Galliard:
    Glory 2, Wisdom 1

Ahroun:
    Glory 2, Honor 1
```

### Unresolved questions

```text
Is Ragabash distribution completely free?
Are zero values implicit for unspecified dimensions?
Are these permanent Renown points?
Does the source distinguish temporary and permanent initial Renown?
Are there Tribe or Race modifiers during creation?
```

### Impact

Blocks approved `initialize-renown` operation.

---

## A-006 — Permanent and Current Resource Initialization

```yaml
ambiguityId: ambiguity.werewolf3e.creation.resource-current-values
severity: High
status: ResolvedFromSource
classification:
  - ResourceRule
```

### Source-derived facts

Race, Auspice, and Tribe initialize Gnosis, Rage, and Willpower respectively.

Source line 934-937 places resource initialization in Step 5 (Final Touches), alongside Bonus Points.

Source line 1063-1068 provides an example: Gnosis 1→2 and Rage 1→3 via bonus points, with the note "1 base de Homínideo + 1 comprado com PB" (1 base from Homid + 1 purchased with PB).

### Resolution

**Resolved from source:**

- Listed values ARE permanent ratings.
- Current pool begins equal to permanent rating.
- Freebie points increase the permanent rating; current pool follows permanent.
- No explicit maximum is stated in the source for resources during creation.
- Resources are initialized in Step 5; the example shows base values first, then bonus point increases.

### Impact

Resolved. Affects resource initialization, freebie spending, sheet display, and later spend/restore operations.

---

## A-007 — Freebie Points and Creation Limits

```yaml
ambiguityId: ambiguity.werewolf3e.creation.freebie-limits
severity: High
status: ResolvedFromSource
classification:
  - CharacterCreationRule
  - CostRule
```

### Source-derived facts

Source line 938-939: 15 Pontos de Bônus budget.
Source line 988-997: Cost table.
Source line 920: Levels 4 and 5 of Abilities can only be acquired using Bonus Points.
Source line 908: No Attribute can exceed level 5 during creation.
Source line 1063-1068: Example shows Abilities, Backgrounds, Rage, Gnosis, and Willpower purchased with bonus points.

### Resolution

**Resolved from source:**

- Budget: 15 points (source explicit)
- Attribute cost: 5 per point; maximum 5 during creation (source explicit)
- Ability cost: 2 per point; levels 4-5 require bonus points (source explicit)
- Background cost: 1 per point; new Backgrounds can be purchased (example shows Ritos and Totem)
- Gift cost: 7 per Gift (Level 1 only); additional Gifts beyond the initial 3 can be purchased
- Rage cost: 1 per point
- Gnosis cost: 2 per point
- Willpower cost: 1 per point
- Renown/Rank: cannot be changed with freebies (not mentioned in source)
- Resource interaction: freebies increase permanent rating; current follows permanent (A-006 resolution)

### Impact

Resolved. Affects freebie validator and fixtures.

---

## 5. High-Severity Character Model Issues

## A-008 — Metis Deformity Representation

```yaml
ambiguityId: ambiguity.werewolf3e.creation.metis-deformity-model
severity: High
status: ImplementationDecisionRequired
classification:
  - CharacterFieldDefinition
  - CharacterCreationRule
  - MechanicalEffect
```

### Source-derived facts

A Metis Character must have at least one deformity.

The source provides deformities with varied effects, including:

- fixed penalties;
- conditional tests;
- automatic failure for a sensory domain;
- field maxima;
- additional Dice;
- missing Health level;
- attacks;
- social consequences.

### Unresolved modeling question

Should a deformity be represented as:

```text
A. a simple catalog selection;
B. a catalog selection plus declarative effects;
C. a structured effect graph;
D. a compiled handler per deformity;
E. a hybrid.
```

### Recommended direction

Hybrid:

```text
catalog identity
    +
declarative static modifiers
    +
compiled handler only for conditional or event-driven behavior
```

### Impact

This is a generic contract pressure point because future systems may also define flaws, disadvantages, conditions, or templates with mechanical effects.

---

## A-009 — Ability Catalog Canonicalization

```yaml
ambiguityId: ambiguity.werewolf3e.terminology.ability-catalog
severity: High
status: SemanticReviewRequired
classification:
  - Terminology
  - CatalogDefinition
```

### Problem

The source uses Portuguese terminology, occasional English terminology, and names that may vary by translation.

Examples include concepts corresponding to:

```text
Alertness
Athletics
Brawl
Primal-Urge
Streetwise
Subterfuge
Crafts
Drive
Etiquette
Firearms
Melee
Stealth
Enigmas
Linguistics
Occult
Rituals
```

### Unresolved question

Which English canonical key maps to each Portuguese source term?

### Rule

Canonical keys must not be chosen solely by literal machine translation.

### Impact

Affects:

- Character schema;
- Dice operation inputs;
- localization;
- Tribe and Race restrictions;
- Gifts and dramatic systems;
- migration stability.

---

## A-010 — “Rites” as Background and Knowledge

```yaml
ambiguityId: ambiguity.werewolf3e.character.rites-dual-concept
severity: Medium
status: SemanticReviewRequired
classification:
  - Terminology
  - CharacterFieldDefinition
```

### Problem

The source appears to use Rites both as:

- a Background or advantage-related value;
- a Knowledge or learned capability;
- the rituals themselves.

### Unresolved questions

```text
Are these distinct mechanical concepts?
What canonical keys distinguish them?
Does one grant access to the other?
Which one is purchased during Character creation?
```

### Impact

Requires separate stable keys and prevents accidental field collision.

---

## A-011 — Totem Background Ownership

```yaml
ambiguityId: ambiguity.werewolf3e.creation.totem-ownership
severity: High
status: Deferred
classification:
  - GroupResourceRule
  - CharacterCreationRule
```

### Source-derived concern

Totem is associated with a pack, while Character creation may allocate individual Background points toward it.

### Unresolved questions

```text
Is the value stored on each contributing Character?
Is there one pack aggregate total?
Can Character creation complete without an existing pack?
How are contributions reconciled when the pack changes?
Can points be refunded or reassigned?
```

### Prototype policy

Store a candidate individual contribution and defer active Totem aggregation.

### Impact

Does not block basic sheet rendering.

Blocks full Totem mechanics.

---

## A-012 — Background Advancement

```yaml
ambiguityId: ambiguity.werewolf3e.progression.background-advancement
severity: Medium
status: Deferred
classification:
  - ProgressionRule
  - NarrativeProgression
```

### Source-derived concern

The source states that Backgrounds generally do not increase through ordinary experience and instead change through narrative events, with Totem treated differently.

### Unresolved questions

```text
Which Backgrounds can never use experience?
Is Totem the only exception?
Does loss use the same operation path?
Can Background ratings exceed creation limits through story events?
```

### Impact

Deferred from Slice 001 but confirms the need for Chronicle-authorized narrative progression operations.

---

## 6. Medium-Severity Workflow Issues

## A-013 — Revising Race, Auspice, or Tribe During Creation

```yaml
ambiguityId: ambiguity.werewolf3e.creation.classification-revision
severity: Medium
status: ImplementationDecisionRequired
```

### Question

If the user changes Race, Auspice, or Tribe after dependent allocations, should Chronicle:

```text
A. clear all dependent selections;
B. preserve valid values and remove invalid ones;
C. block the change until dependencies are manually reset;
D. create a recalculation plan requiring confirmation.
```

### Recommended prototype behavior

Create a previewed recalculation plan:

```text
values preserved
values reset
values invalidated
freebie refund
new restrictions
```

Require user confirmation before application.

This is workflow architecture, not a source rule.

---

## A-014 — Character Creation Draft Persistence

```yaml
ambiguityId: ambiguity.werewolf3e.creation.draft-validity
severity: Medium
status: ImplementationDecisionRequired
```

### Question

May an incomplete Character draft contain temporarily invalid allocations?

### Recommended prototype behavior

Distinguish:

```text
DraftValidForStep
CompleteCharacterValid
```

A draft may be incomplete but must remain structurally valid and recoverable.

---

## A-015 — Initial Gift Duplicate Selection

```yaml
ambiguityId: ambiguity.werewolf3e.creation.duplicate-gifts
severity: Medium
status: SemanticReviewRequired
```

### Question

If the same Level One Gift appears in more than one eligibility catalog, may it satisfy two categories?

Candidate interpretations:

```text
A. no; three distinct Gifts are required;
B. yes; one Gift may satisfy multiple eligibility sources;
C. duplicate identity is forbidden but the user may select another source category;
D. source-specific exception exists.
```

### Recommended prototype policy

Require three distinct Gift keys until source evidence says otherwise.

Mark as candidate.

---

## A-016 — Tribe Eligibility Restrictions

```yaml
ambiguityId: ambiguity.werewolf3e.creation.tribe-eligibility
severity: High
status: SourceSearchRequired
```

### Problem

Some Tribe descriptions may include identity or social restrictions.

### Unresolved question

Which are:

```text
hard mechanical eligibility rules;
setting-era social expectations;
common membership patterns;
narrative guidance;
historical source assumptions.
```

### Rule

Do not convert a narrative description into an enforced Character creation prohibition without an explicit mechanical source segment.

---

## A-017 — Metis Terminology and Localization

```yaml
ambiguityId: ambiguity.werewolf3e.terminology.metis
severity: Medium
status: TerminologyReviewRequired
```

### Problem

The cleaned source uses more than one Portuguese label and includes socially derogatory in-world terms.

### Requirements

The package must distinguish:

```text
canonical technical key
neutral display label
in-world historical or derogatory synonym
narrative context
localization policy
```

A derogatory source term must not become an unqualified UI default automatically.

---

## A-018 — Race Versus Breed Naming

```yaml
ambiguityId: ambiguity.werewolf3e.terminology.race-breed
severity: Medium
status: TerminologyReviewRequired
```

### Problem

The Portuguese source uses “Raça,” while English source terminology may use another term.

### Question

What canonical technical key should represent this classification?

Candidate keys:

```text
character.classification.race
character.classification.breed
character.origin.birth-form
```

### Rule

The key must remain stable and package-local.

Chronicle Core must not encode any of these concepts.

---

## 7. Source Conflict Categories to Search

The detailed pass must search for conflicts in these pairs:

```text
creation summary
    versus
detailed Character creation chapter

Race summary
    versus
detailed Race section

Auspice summary
    versus
detailed Auspice section

Tribe overview
    versus
individual Tribe profile

freebie summary table
    versus
progression or creation prose

specialization summary
    versus
generic Dice section

Background creation rules
    versus
Background detailed descriptions

initial Gifts summary
    versus
Gift catalogs

resource initialization
    versus
resource-system chapters
```

When two statements differ, retain both source segments and create a conflict record.

## 8. Conflict Record Template

```yaml
conflictId: conflict.<stable-key>
severity: high
status: open

sourceStatements:
  - sourceSegmentRef: <segment-a>
    normalizedStatement: <statement-a>
  - sourceSegmentRef: <segment-b>
    normalizedStatement: <statement-b>

conflictType:
  - numeric
  - timing
  - scope
  - terminology
  - exception
  - optionality
  - procedure

affectedArtifacts:
  - <artifact>

resolution:
  selectedInterpretation: null
  rationale: null
  reviewer: null
```

## 9. Prototype Decision Registry

Temporary prototype decisions must be stored separately from source-derived rules.

Recommended file:

```text
rule-sets/Chronicle.RuleSets.Werewolf/prototype/
    provenance/prototype-decisions.json
```

Each decision includes:

```text
decisionKey
ambiguityId
selectedPrototypeBehavior
sourceTruthStatus
reason
reversibility
affectedArtifacts
reviewRequired
```

## 10. Current Prototype Recommendations

These are recommendations, not approved Werewolf rules.

### PR-001 — Draft Creation State

Allow incomplete drafts but block final completion until all required validations pass.

### PR-002 — Classification Recalculation

Changing Race, Auspice, or Tribe creates a previewed dependency-recalculation plan.

### PR-003 — Metis Deformities

Use catalog entries with declarative modifiers plus compiled handlers where necessary.

### PR-004 — Totem

Store only an individual candidate contribution in Slice 001.

### PR-005 — Initial Gifts

Require three distinct Level One Gift identities, one from each eligibility source.

### PR-006 — Resource Schema

Model permanent and current values separately even before exact initialization semantics are resolved.

### PR-007 — Lupus Restrictions

Do not approve freebie behavior until the dedicated source statements are reviewed.

### PR-008 — Dice Resolver

Do not implement from model knowledge or remembered World of Darkness rules.

## 11. Issues Blocking Concrete Prototype Artifacts

### Blocks Manifest

None, except final package identity remains provisional.

### Blocks Character Model Skeleton

None.

### Blocks Complete Race Catalog

```text
Metis deformity artifact contract
Lupus freebie interpretation
```

### Blocks Complete Tribe Catalog

```text
Tribe completeness matrix
eligibility classification
Willpower values
Background restrictions
Gift options
```

### Blocks Creation Workflow Skeleton

None.

### Blocks Final Creation Validator

```text
freebie limits
Lupus restriction timing
resource current/permanent semantics
initial Renown
Gift duplicate handling
```

### Blocks Generic Dice End-to-End Test

```text
basic resolution algorithm
specialization resolution
```

## 12. Review Order

Resolve in this order:

```text
1. generic Dice algorithm
2. Tribe completeness
3. Ability catalog and creation limits
4. freebie interactions
5. Lupus restrictions
6. initial Renown
7. resource initialization
8. Gift eligibility and duplicates
9. Metis deformity model
10. terminology
11. Totem ownership
```

## 13. Evidence Requirements

An ambiguity may move to `ResolvedForPublication` only with:

```text
source segment references
normalized rule statement
reviewer approval
implementation mapping
fixtures
regression tests
provenance record
```

External knowledge may be used only when explicitly requested and must be labeled separately from the submitted source.

## 14. No-Silent-Reconciliation Tests

Architecture or extraction tests must fail when:

- two source statements map to the same rule key with different values;
- a candidate rule lacks review status;
- an ambiguity is marked resolved without evidence;
- a prototype decision is serialized as source truth;
- a narrative description creates a hard restriction with no mechanical source;
- translated labels are used as technical identity;
- a Dice resolver exists without linked source segments;
- a missing Tribe value is replaced by a default silently.

## 15. Next Document

Create:

```text
docs/extraction/werewolf-3e/
    EXTRACTION-0005-contract-findings.md
```

That document will identify which generic package contracts have been validated or challenged by this source, including:

- classification dependencies;
- staged Character creation;
- catalog-driven initialization;
- conditional restrictions;
- shared resources such as Totem;
- flaws and deformities with heterogeneous effects;
- permanent/current resources;
- linked specialization Dice;
- narrative progression;
- terminology aliases;
- source and prototype decision separation.

## 16. Final Rule

The presence of an ambiguity is not a failure of the extraction.

Hiding it would be.

For Slice 001:

```text
known uncertainty
    is acceptable

invisible invention
    is not
```
