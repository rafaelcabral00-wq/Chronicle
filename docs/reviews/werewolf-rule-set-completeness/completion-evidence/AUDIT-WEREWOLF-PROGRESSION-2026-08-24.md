# Werewolf Progression / Experience / Advancement / Gift Learning Source-Authority Audit

**Audit ID:** AUDIT-WEREWOLF-PROGRESSION-2026-08-24
**Date:** 2026-08-24
**Branch:** audit/werewolf-progression
**Baseline:** d3cf174
**Auditor:** Kilo (automated source-authority audit)
**Status:** Evidence-only — no mechanics implemented

---

## 1. Full Source Traversal Scope

**Canonical source file:** `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt`
**Local path:** `C:\Dev\Chronicle-wt-progression\.rule-set-sources\werewolf\Werewolf the Apocalypse 3e-pt_br.txt`
**SHA-256:** `a4302e2938a137fb42d154c45decd43e02a19a2ba0eb3030b6eb1de942dab64a`
**Line count:** 3,948
**Byte size:** 380,686
**Language:** pt-BR (Brazilian Portuguese, cleaned working source)

**Traversal method:** Full line-by-line read (lines 1–3,948) supplemented by targeted regex searches for progression-related keywords. All heading paths and line ranges cited below were verified against the exact source file.

**Source sections traversed for this audit:**
- Lines 1–154: Setting, mythology, Litany, hierarchy, Sept/Pack
- Lines 155–290: Society, Ranks, leadership, geography
- Lines 291–393: Antagonists, Weaver, corporate threats
- Lines 394–493: Glossary
- Lines 494–549: Races (Homid, Metis, Lupus)
- Lines 550–636: Auspices, comparative tables, initial Gifts
- Lines 637–977: Tribes (all 12), Background definitions, Mentor
- Lines 978–1077: Character creation summary, example character
- Lines 1078–1087: Specialties mechanic
- Lines 1088–1532: Attributes, Abilities, Knowledges, Backgrounds
- Lines 1533–1647: Backgrounds evolution rules, Totem, Totem improvements
- Lines 1648–1711: Renown, Rank, Rage, Gnosis, Willpower
- Lines 1712–1723: Health levels
- Lines 1724–1920: Gifts (racial, Auspice, tribal, learning rules)
- Lines 1921–2574: Gifts by Auspice/tribe, Rituals
- Lines 2575–2648: Ritual learning, Caern rituals, Renown rituals
- Lines 2649–2858: Renome system, Rank advancement, Renunciation, Experience and Advancement
- Lines 2859–2909: Health, aging, battle scars
- Lines 2910–3109: Frenzy, Delirium, combat, damage
- Lines 3110–3948: Dramatic systems, forms, storytelling guidance, Totems, Fetishes

---

## 2. Renown Current-State Truth

### 2.1 Implemented Runtime State

The following Renown mechanics are **implemented** in the current codebase:

| Component | Status | Evidence |
|:---|:---|:---|
| Structural/runtime state | **Implemented** | `WerewolfRuntimeCharacterState` carries `GloryPermanent`, `GloryCurrent`, `HonorPermanent`, `HonorCurrent`, `WisdomPermanent`, `WisdomCurrent` |
| Initialization by Auspice | **Implemented** | `WerewolfResourceRankInitializationService.GetInitialRenown()` at `WerewolfResourceRankInitialization.cs:281-311` |
| Temporary award | **Implemented** | `WerewolfRenownTransitionService.AwardTemporaryRenown()`; operation `character-runtime.award-temporary-renown` is Enabled |
| Temporary loss | **Implemented** | `WerewolfRenownTransitionService.LoseTemporaryRenown()`; operation `character-runtime.lose-temporary-renown` is Enabled |
| Temporary-to-permanent conversion | **Implemented** | `WerewolfRenownTransitionService.ConvertTemporaryToPermanent()`; threshold = 10; operation `character-runtime.convert-temporary-to-permanent-renown` is Enabled |
| Completion validation | **Implemented** | `WerewolfCharacterCompletion.cs:259-285` validates all 6 Renown keys |

### 2.2 Incomplete / Governance-Blocked

| Component | Status | Evidence |
|:---|:---|:---|
| Source/governance approval | **Unresolved** | DR-0005 explicitly blocks IMPLEMENT-019B; no superseding decision exists |
| Advancement integration with Rank | **Incomplete** | No Renown-threshold-to-Rank-advancement workflow exists |
| Ritual of Conquest / contested conversion | **Incomplete** | Conversion primitive exists (10:1), but Ritual of Conquest test (difficulty 4/6, contested Rage test) is not implemented |
| Ragabash free-combination selection | **Implemented structurally** | `SelectRagabashRenownStep` is defined and operation is Enabled, but DR-0005 does not approve it |

### 2.3 Governance Conflict

`GOV-RENOWN-001 — DR-0005/runtime divergence`

**Problem:** The runtime currently implements Renown behaviors that DR-0005 explicitly did not approve.

**Required future action (do not decide which is correct in this audit):**
- supersede DR-0005; or
- reconcile/remove the implemented behavior; or
- issue a new decision approving the current semantics.

**Owner:** Renown/Rank governance reconciliation.

This blocker is NOT ownerless.

---

## 3. Rank Current-State Truth

### 3.1 Implemented Runtime State

| Component | Status | Evidence |
|:---|:---|:---|
| Rank identity/value | **Implemented** | `WerewolfRankIdentifiers.Cliath` at `WerewolfResourceRankInitialization.cs:65-68`; `WerewolfInitialRankValue` record at line 11 |
| Cliath initialization | **Implemented** | `WerewolfResourceRankInitializationService.Initialize()` sets `Rank = "character.rank.cliath"`, `RankValue = 1` at lines 180-181 |
| Rank consumed by Frenzy | **Implemented** | `WerewolfFrenzyTestDefinitionService` uses `rank` parameter for difficulty and success thresholds at lines 47-85 |
| Rank consumed by Social | **Implemented** | `WerewolfSocialTestDefinitionService.ComputeOratoriaDifficulty()` subtracts `CharacterRankValue` at lines 295-298 |
| Completion validation | **Implemented** | `WerewolfCharacterCompletion.cs:287-290` requires non-null Rank and RankValue |

### 3.2 Not Implemented

| Component | Status | Evidence |
|:---|:---|:---|
| Rank advancement | **Does not exist** | No promotion, challenge, or advancement code found |
| Rank fall | **Does not exist** | No fall-of-Rank mechanics implemented |
| Rank challenge workflow | **Does not exist** | No challenge operation or elder NPC interaction |
| Gift-level eligibility enforcement | **Does not exist** | No code checks `Rank >= GiftLevel` |
| Numeric Renown threshold table | **Absent from source** | Canonical source line 2849 references thresholds but does not enumerate them |

### 3.3 True Blocker

The true blocker is **Rank advancement semantics and thresholds**, not "Rank runtime state missing."

---

## 4. Trait-Maxima Truth (Corrected)

The earlier audit conclusion that "Attributes hard maximum 5; Abilities post-creation maximum 5" is **superseded** by the following source-verified truth:

| Dimension | Maximum | Source Locator |
|:---|:---|:---|
| Creation Attribute max | 5 | Line 909: "Nenhum Atributo pode ultrapassar o nível 5 na criação." |
| Creation Ability base-allocation max | 3 | Line 921: "Nenhuma Habilidade pode ter mais de 3 pontos nesta etapa" |
| Creation Ability max with Bonus Points | 5 | Lines 992-993: Ability costs 2/point in Bonus Points budget |
| Post-creation general ceiling | 10 | Line 2788: "geralmente variando de 1 a 5, podendo ir até 10"; Line 857: "de 1 a 5, com exceções até 10" |
| Form-modified effective Physical Attributes | May exceed 5 | Line 1081: "Atributos Físicos sofrem alterações que podem ultrapassar os limites humanos" |
| Rank-dependent trait maximum | **None found** | No source linkage between Rank and trait maxima |

**Key distinction:** Base permanent rating ≠ effective transformed rating. Form changes and certain effects can produce effective ratings above 5, but the base permanent maximum is 5 at creation and may advance up to 10 post-creation via XP.

---

## 5. A/B/C/D/E Classification Semantics

### 5.1 Definitions

| Category | Meaning |
|:---|:---|
| **A** | Mechanic logic itself is directly implementable with current Rule Set architecture, though it may consume an external token/state already represented as a secondary dependency. |
| **B** | Requires a small typed extension. |
| **C** | Cannot execute until another subsystem exists or is reconciled. |
| **D** | Source-defined category/amount may exist, but triggering judgment is narrative/adjudicative. |
| **E** | Requires Human Decision because source is contradictory or insufficient. |

### 5.2 Exact Counts (Total = 37)

| Category | Count |
|:---|:---:|
| A | 14 |
| B | 2 |
| C | 10 |
| D | 10 |
| E | 1 |
| **Total** | **37** |

### 5.3 Per-Mechanic Classification

| # | Mechanic | Primary | Secondary Dependencies |
|:---|:---|:---|:---|
| 1 | XP — Per-Session Automatic | D | C (Chronicle persistence) |
| 2 | XP — Per-Session Learning Curve | D | C (Chronicle persistence) |
| 3 | XP — Per-Session Roleplay | D | C (Chronicle persistence) |
| 4 | XP — Per-Session Concept | D | C (Chronicle persistence) |
| 5 | XP — Per-Session Heroism | D | C (Chronicle persistence) |
| 6 | XP — Per-Story Success | D | C (Chronicle persistence) |
| 7 | XP — Per-Story Danger | D | C (Chronicle persistence) |
| 8 | XP — Per-Story Wisdom | D | C (Chronicle persistence) |
| 9 | XP Cost — Attribute | A | |
| 10 | XP Cost — Ability | A | |
| 11 | XP Cost — New Ability | A | |
| 12 | XP Cost — Own-Race/Auspice/Tribe Gift | A | C (Rank prerequisite check) |
| 13 | XP Cost — Other-Race/Auspice/Tribe Gift | A | C (Rank prerequisite check) |
| 14 | XP Cost — Rage | A | |
| 15 | XP Cost — Gnosis | A | |
| 16 | XP Cost — Willpower | A | |
| 17 | XP Cost — Totem | E | A (pending A-012 resolution) |
| 18 | Background XP restriction | A | |
| 19 | Per-story limit | A | C (Chronicle StoryToken) |
| 20 | Willpower permanent loss | A | B (requires critical-failure event hook) |
| 21 | Renown permanent vs temporary | C | GOV-RENOWN-001 |
| 22 | Renown initial values | C | GOV-RENOWN-001 |
| 23 | Renown conversion (10:1) | C | GOV-RENOWN-001 + Ritual of Conquest |
| 24 | Rank advancement requirements | C | Threshold table absent from source |
| 25 | Rank advancement challenge | C | Threshold table + elder NPCs |
| 26 | Rank = Gift level prerequisite | A | C (Rank runtime state — already exists) |
| 27 | Rank fall | C | Threshold table absent from source |
| 28 | Rank optional tribal reputation | D | |
| 29 | Rank renunciation | C | Rituals domain |
| 30 | Gift learning — spirit instructor | C | Spirits / Caerns / Theurge |
| 31 | Gift learning — mutual teaching | C | Downtime + Tribe restrictions + Spirits |
| 32 | Ritual learning | C | Rituals domain + elder NPCs |
| 33 | Specialty acquisition threshold | A | B (character state must track specialties) |
| 34 | Specialty — exploding 10s | B | A (dice algorithm exists; needs specialty binding) |
| 35 | Specialty — 1 protection | B | A (dice algorithm exists; needs specialty binding) |
| 36 | Specialty — GM approval | D | |
| 37 | Totem advancement — XP exception | A | C (Totem aggregation) |

---

## 6. First Implementation Package: Werewolf Progression Core

### 6.1 CORE-A — Immediately Implementable

No external subsystem prerequisites. Can be implemented against current Rule Set architecture.

| Mechanic | Classification |
|:---|:---|
| XP balance/state | A |
| XP cost calculation (full table) | A |
| Attribute advancement | A |
| Ability advancement / new Ability | A |
| Rage advancement | A |
| Gnosis advancement | A |
| Willpower advancement | A |
| Background XP prohibition | A |
| Gift XP cost calculation (own ×3, other ×5) | A |
| Rank-vs-Gift-level eligibility using existing RankValue | A |
| Specialty unlock eligibility (4+ threshold) | A |
| Willpower permanent loss on critical failure | A |

### 6.2 CORE-B — Implementable Once Chronicle Provides StoryToken Contract

| Mechanic | Classification |
|:---|:---|
| Per-story advancement cap (max +1 per trait per story) | A |

**Chronicle prerequisite:** Provide opaque `StoryToken` to Progression. Progression consumes it; does not define story boundaries.

### 6.3 CORE-C — Blocked by Human Decision

| Mechanic | Classification | Blocker |
|:---|:---|:---|
| Totem XP cost | E | A-012: 2 XP vs 3 XP conflict (lines 1633 vs 2820) |

### 6.4 Explicitly Excluded from Progression Core

These belong to a future **Renown / Rank Advancement and Governance Reconciliation** work package:

- Renown initialization
- Renown temporary transitions
- Renown conversion workflow (Ritual of Conquest)
- Rank advancement
- Rank challenge
- Rank fall
- Rank renunciation

Also excluded (owned by other domains):

- Gift learning — spirit instructor (Spirits)
- Gift learning — mutual teaching downtime (Chronicle + Spirits)
- Ritual learning (Rites)
- Totem aggregation (Pack/Totem)
- Session-based XP earning (Chronicle)

---

## 7. Specialty Ownership (Final)

| Aspect | Owner |
|:---|:---|
| Unlock eligibility | **Progression** |
| Specialty identity storage | **Progression** |
| Player selection state | **Progression** |
| Applicability | **Action Resolution** |
| Exploding-10 execution | **Action Resolution** |
| 1-protection behavior | **Action Resolution** |

Do not implement either side here.

---

## 8. Gift-Learning Ownership (Final)

| Aspect | Owner |
|:---|:---|
| XP cost | **Progression** |
| Rank prerequisite | **Progression** |
| Acquisition transaction | **Progression** |
| Spirit teacher identity/availability | **Spirits** |
| Spirit summoning/interaction | **Spirits** |
| Downtime/story lifecycle | **Chronicle** |
| Ritual learning | **Rites** (not Gift learning) |

---

## 9. Human Decisions (Final)

| ID | Severity | Description | Source Evidence | Required Action |
|:---|:---|:---|:---|:---|
| A-012 | Medium | Totem XP cost: 2 XP/point (line 1633) vs 3 XP/point (line 2820) | `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` lines 1633, 2820 | Select authoritative cost |
| Rank threshold source gap | High | Numeric Renown thresholds per Rank per Auspice absent from canonical source | Line 2849: "pontuação de Renome permanente exigida para o posto correspondente ao seu augúrio" (values not enumerated) | Extract from supplementary material or define via Human Decision |
| GOV-RENOWN-001 | High | DR-0005/runtime divergence: runtime implements Renown behaviors that DR-0005 did not approve | DR-0005 vs current codebase (`WerewolfRenownTransitionService`, `WerewolfResourceRankInitializationService`) | Supersede DR-0005; or reconcile/remove implemented behavior; or issue new decision approving current semantics |

**Not a Werewolf Human Decision:** StoryToken/story delimiter is a Chronicle architecture contract, not a Werewolf mechanic decision.

---

## 10. Blocker Ownership (Final)

| Blocker | Owner |
|:---|:---|
| Session/downtime persistence | Chronicle platform/runtime |
| Renown runtime governance conflict (GOV-RENOWN-001) | Renown/Rank governance reconciliation |
| Rank advancement semantics and thresholds | Renown/Rank future work package |
| Spirit instructor availability | Spirits/Umbra future work package |
| Ritual learning | Rites |
| Totem aggregation | Pack/Totem |
| Story delimiter / StoryToken contract | Chronicle platform/runtime |

**Ownerless blockers = 0.**

---

## 11. Exact Files Changed

**This audit is evidence-only.**

Files read:
- `C:\Dev\Chronicle-wt-progression\.rule-set-sources\werewolf\Werewolf the Apocalypse 3e-pt_br.txt`
- `C:\Dev\Chronicle-wt-progression\docs\reviews\werewolf-rule-set-completeness\completeness-matrix.json`
- `C:\Dev\Chronicle-wt-progression\docs\reviews\werewolf-rule-set-completeness\completeness-report.md`
- `C:\Dev\Chronicle-wt-progression\docs\extraction\werewolf-3e\EXTRACTION-0001-source-inventory.md`
- `C:\Dev\Chronicle-wt-progression\docs\reviews\documentation-reconciliation\decision-requests\DR-0005-initial-resources-rank-and-renown-boundary.md`
- `C:\Dev\Chronicle-wt-progression\rule-sets\Chronicle.RuleSets.Werewolf\CharacterCreation\WerewolfResourceRankInitialization.cs`
- `C:\Dev\Chronicle-wt-progression\rule-sets\Chronicle.RuleSets.Werewolf\CharacterCreation\WerewolfRuntimeCharacterState.cs`
- `C:\Dev\Chronicle-wt-progression\rule-sets\Chronicle.RuleSets.Werewolf\CharacterCreation\WerewolfRenownTransitionService.cs`
- `C:\Dev\Chronicle-wt-progression\rule-sets\Chronicle.RuleSets.Werewolf\CharacterCreation\WerewolfCharacterCompletion.cs`
- `C:\Dev\Chronicle-wt-progression\rule-sets\Chronicle.RuleSets.Werewolf\CharacterCreation\WerewolfCharacterCreationDraftInitializer.cs`
- `C:\Dev\Chronicle-wt-progression\rule-sets\Chronicle.RuleSets.Werewolf\CharacterCreation\WerewolfFrenzyTestDefinitionService.cs`
- `C:\Dev\Chronicle-wt-progression\rule-sets\Chronicle.RuleSets.Werewolf\CharacterCreation\WerewolfSocialTestDefinitionService.cs`
- `C:\Dev\Chronicle-wt-progression\rule-sets\Chronicle.RuleSets.Werewolf\WerewolfReferenceRuntime.cs`

Files created:
- `docs/reviews/werewolf-rule-set-completeness/completion-evidence/AUDIT-WEREWOLF-PROGRESSION-2026-08-24.md`

Files modified:
- **None.** No production code, matrix, report, manifest, or metadata was touched.

---

## 12. Git Status

```
nothing to commit, working tree clean
```

---

## 13. Acceptance Statement

AUDIT-WEREWOLF-PROGRESSION-2026-08-24 is accepted as implementation-planning evidence.
