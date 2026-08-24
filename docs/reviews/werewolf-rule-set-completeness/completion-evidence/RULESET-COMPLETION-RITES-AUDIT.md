# RULESET-COMPLETION-RITES-AUDIT: Werewolf Rites Source-Authority Audit

**Status:** Complete
**Date:** 2026-08-24
**Auditor:** Kilo (automated source-authority audit)
**Scope:** Complete canonical source traversal for all Rite mechanics in Werewolf the Apocalypse 3e
**Canonical Source:** `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt`
**Baseline:** d3cf174
**Branch:** audit/werewolf-rites

---

## 1. Full Source Traversal Scope

**Source file:** `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt`
**File size:** 380,686 bytes
**Line count:** 3,948 lines
**SHA-256:** a4302e2938a137fb42d154c45decd43e02a19a2ba0eb3030b6eb1de942dab64a
**Language:** pt-BR

**Traversal method:** Complete linear read with targeted regex searches for:
- Rite section headers (`## RITUAIS`, `## Ritos`, `### Ritual`)
- Individual Rite entries (`* **Ritual de`, `* **Ritual do`, `* **Ritual da`, `* **Batismo`, `* **Cerimônia`)
- Level markers (`(Nível [0-9])`)
- Category tables (`Tabela de Categorias e Testes de Rituais`)
- Learning/prerequisite rules (`Aprendizado`, `Rituais` knowledge)

**Sections traversed:**
- Lines 45-47: Ritos de Passagem (conceptual)
- Lines 188-189: Ritos de Submissão (conceptual)
- Lines 567-569: Ritual de Renúncia (conceptual)
- Lines 2562-2702: Complete Rite catalog (sections 39-46)
- Lines 1619-1630: Rites Background definition
- Lines 1509-1511: Rituais Ability definition
- Lines 2575-2578: General Rite rules (duration, participation, learning)

**Excluded from Rite inventory:**
- Gifts (Dons)
- Fetishes/Talens
- Totems
- Spirit Charms (Encantos)
- Narrative ceremonies
- "Rituais Bizarros e Sobrevivência" (tribal flavor)

---

## 2. Exact Rite Count

**Total canonical Rites: 32**

---

## 3. Rite Category Counts

| Category | Count |
|----------|-------|
| Caern | 6 |
| Death | 2 |
| Pact | 3 |
| Punishment | 7 |
| Renown | 3 |
| Mystic | 9 |
| Periodic | 2 |
| **Total** | **32** |

---

## 4. Rite Level Counts

| Level | Count |
|-------|-------|
| 1 | 9 |
| 2 | 11 |
| 3 | 6 |
| 4 | 4 |
| 5 | 2 |
| **Total** | **32** |

---

## 5. Complete 32-Rite Inventory

| # | Stable Key | Canonical Name | Category | Level | Source Locator |
|---|-----------|----------------|----------|-------|----------------|
| 1 | `rite.caern.assembly` | Ritual de Assembleia | Caern | 1 | Line 2583 |
| 2 | `rite.caern.opening` | Ritual de Abertura de Caern | Caern | 1 | Line 2586 |
| 3 | `rite.caern.badger-set` | A Toca do Texugo | Caern | 4 | Line 2590 |
| 4 | `rite.caern.moon-bridge` | Ritual de Abertura de Ponte | Caern | 4 | Line 2593 |
| 5 | `rite.caern.hidden-ravine` | Ritual da Ravina Encoberta | Caern | 4 | Line 2596 |
| 6 | `rite.caern.creation` | Ritual de Criação de Caern | Caern | 5 | Line 2600 |
| 7 | `rite.death.commemoration` | Cerimônia pelo Falecido | Death | 1 | Line 2606 |
| 8 | `rite.death.winter-wolf` | Ritual do Lobo do Inverno | Death | 3 | Line 2608 |
| 9 | `rite.pact.purification` | Ritual de Purificação | Pact | 1 | Line 2614 |
| 10 | `rite.pact.contrition` | Ritual de Contrição | Pact | 1 | Line 2617 |
| 11 | `rite.pact.luna-mutable` | Ritual de Renome / Lua Cambiante | Pact | 2 | Line 2620 |
| 12 | `rite.punishment.ostracism` | Ritual de Ostracismo | Punishment | 2 | Line 2626 |
| 13 | `rite.punishment.stone-of-scorn` | Pedra de Escárnio | Punishment | 2 | Line 2629 |
| 14 | `rite.punishment.jackal-voice` | Voz do Chacal | Punishment | 2 | Line 2632 |
| 15 | `rite.punishment.the-hunt` | A Caçada | Punishment | 3 | Line 2635 |
| 16 | `rite.punishment.satirical-ritual` | Ritual Satírico | Punishment | 3 | Line 2638 |
| 17 | `rite.punishment.veil-laceration` | A Laceração do Véu | Punishment | 4 | Line 2641 |
| 18 | `rite.punishment.ciala-avenging-teeth` | Os Dentes Vingativos de Ciala | Punishment | 5 | Line 2645 |
| 19 | `rite.renown.conquest` | Ritual de Conquista | Renown | 2 | Line 2651 |
| 20 | `rite.renown.passage` | Ritual de Passagem | Renown | 2 | Line 2654 |
| 21 | `rite.renown.wounding` | Ritual de Ferimento | Renown | 1 | Line 2657 |
| 22 | `rite.mystic.fire-baptism` | Batismo de Fogo | Mystic | 1 | Line 2663 |
| 23 | `rite.mystic.commitment` | Ritual de Compromisso | Mystic | 1 | Line 2666 |
| 24 | `rite.mystic.hunting-stone` | Ritual da Pedra Caçadora | Mystic | 1 | Line 2669 |
| 25 | `rite.mystic.talisman-dedication` | Ritual de Dedicação do Talismã | Mystic | 1 | Line 2672 |
| 26 | `rite.mystic.initiation` | Ritual de Iniciação | Mystic | 2 | Line 2675 |
| 27 | `rite.mystic.awaken-spirits` | Ritual para Despertar Espíritos | Mystic | 2 | Line 2678 |
| 28 | `rite.mystic.summoning` | Ritual de Conjuração | Mystic | 2 | Line 2681 |
| 29 | `rite.mystic.fetish` | Ritual de Fetiche | Mystic | 3 | Line 2690 |
| 30 | `rite.mystic.totem` | Ritual de Totem | Mystic | 3 | Line 2693 |
| 31 | `rite.periodic.cold-winds` | Ritual dos Ventos Frios | Periodic | 2 | Line 2699 |
| 32 | `rite.periodic.new-awakening` | Ritual do Novo Despertar | Periodic | 2 | Line 2701 |

---

## 6. Activation/Test Taxonomy

| Test Pattern | Count | Rite Keys |
|-------------|-------|-----------|
| Carisma + Rituais | 13 | `rite.pact.purification`, `rite.pact.contrition`, `rite.punishment.ostracism`, `rite.punishment.stone-of-scorn`, `rite.punishment.jackal-voice`, `rite.punishment.the-hunt`, `rite.punishment.satirical-ritual`, `rite.punishment.veil-laceration`, `rite.punishment.ciala-avenging-teeth`, `rite.renown.conquest`, `rite.renown.passage`, `rite.renown.wounding`, `rite.mystic.fire-baptism` |
| Raciocínio + Rituais | 6 | `rite.caern.opening`, `rite.caern.creation`, `rite.mystic.initiation`, `rite.mystic.fetish`, `rite.mystic.totem`, `rite.periodic.cold-winds` (group) |
| Raciocínio + Enigmas | 1 | `rite.caern.moon-bridge` |
| Percepção + Rituais | 1 | `rite.caern.badger-set` |
| Vigor + Rituais | 1 | `rite.periodic.cold-winds` (individual) |
| Força de Vontade (fixed) | 1 | `rite.mystic.commitment` |
| Gnose (fixed) | 2 | `rite.mystic.awaken-spirits`, `rite.mystic.summoning` |
| Nenhum | 2 | `rite.caern.assembly` (Gnose donation only), `rite.death.winter-wolf` (narrative) |

---

## 7. Resource-Cost Taxonomy

| Cost Type | Count | Rite Keys |
|-----------|-------|-----------|
| Gnose | 8 | `rite.caern.assembly`, `rite.caern.creation`, `rite.caern.hidden-ravine`, `rite.pact.purification`, `rite.mystic.talisman-dedication`, `rite.mystic.fetish`, `rite.mystic.initiation`, `rite.periodic.cold-winds` (implied) |
| Renome (loss/gain) | 4 | `rite.punishment.ostracism`, `rite.punishment.stone-of-scorn`, `rite.punishment.jackal-voice`, `rite.renown.conquest` |
| Posto (loss) | 1 | `rite.punishment.satirical-ritual` |
| Posto (reset) | 1 | `rite.pact.luna-mutable`, `rite.renown.passage` |
| Fúria (spend/test) | 1 | `rite.mystic.awaken-spirits` |
| Material | 3 | `rite.pact.contrition` (gift), `rite.caern.moon-bridge` (lunar gem), `rite.punishment.veil-laceration` (manure/herbs) |
| None | 14 | Various |

---

## 8. Duration Taxonomy

| Duration | Count | Rite Keys |
|----------|-------|-----------|
| Instant/Scene | 12 | Most Level 1-2 Rites |
| Extended (accumulated) | 4 | `rite.caern.opening`, `rite.caern.creation`, `rite.mystic.fetish`, `rite.mystic.summoning` |
| Permanent | 3 | `rite.caern.creation`, `rite.punishment.ciala-avenging-teeth`, `rite.pact.luna-mutable` |
| Seasonal/Calendar | 2 | `rite.periodic.cold-winds`, `rite.periodic.new-awakening` |
| Variable | 11 | Dependent on successes |

---

## 9. Participant/Target Taxonomy

| Participant Pattern | Count | Rite Keys |
|---------------------|-------|-----------|
| Minimum 3 Garou | 8 | Most social Rites |
| Minimum 5 Garou | 4 | `rite.caern.hidden-ravine`, `rite.caern.creation`, `rite.periodic.*` |
| 13+ Garou | 1 | `rite.caern.creation` |
| Master required | 6 | All learning/teaching contexts |
| Target = caster | 3 | `rite.mystic.talisman-dedication`, `rite.mystic.initiation`, `rite.death.winter-wolf` |
| Target = spirit | 3 | `rite.mystic.commitment`, `rite.mystic.fetish`, `rite.mystic.totem` |
| Target = entity/place | 8 | Purification, Contrition, etc. |

---

## 10. State Effects

| Effect Type | Count | Rite Keys |
|-------------|-------|-----------|
| Renown gain | 3 | `rite.renown.*` |
| Renown loss | 3 | `rite.punishment.*` (ostracism, stone, jackal) |
| Rank change | 2 | `rite.renown.conquest`, `rite.punishment.satirical-ritual` |
| Posto reset | 2 | `rite.pact.luna-mutable`, `rite.renown.passage` |
| Permanent Gnose cost | 2 | `rite.caern.creation`, `rite.mystic.fetish` |
| Spirit binding | 3 | `rite.mystic.commitment`, `rite.mystic.fetish`, `rite.mystic.totem` |
| Caern state | 4 | `rite.caern.*` |
| Physical death | 1 | `rite.death.winter-wolf` |
| Umbra access | 1 | `rite.mystic.initiation` |

---

## 11. Spirit/Umbra Dependencies

| Dependency | Hard/Soft | Rite Keys |
|------------|-----------|-----------|
| Spirit identity/communication | HARD | `rite.mystic.commitment`, `rite.mystic.fetish`, `rite.mystic.totem`, `rite.mystic.summoning`, `rite.mystic.awaken-spirits` |
| Spirit summoning/conjuration | HARD | `rite.mystic.summoning` |
| Umbra traversal/materialization | HARD | `rite.mystic.initiation` |
| Caern spirit negotiation | HARD | `rite.caern.opening` |
| Spirit attendance/party | SOFT | `rite.periodic.*`, `rite.mystic.fire-baptism` |
| Ancestor/spirit guide | SOFT | `rite.death.commemoration`, `rite.mystic.fire-baptism` |

---

## 12. Pack/Sept/Totem Dependencies

| Dependency | Rite Keys |
|------------|-----------|
| Pack reference | `rite.mystic.totem` |
| Sept reference | `rite.caern.*` (all) |
| Totem reference | `rite.mystic.totem` |
| Collective participants | `rite.caern.assembly`, `rite.caern.creation`, `rite.periodic.*` |
| Caern ownership | `rite.caern.*` (all) |
| Pack state transition | `rite.mystic.totem` |

---

## 13. Progression/Learning Dependencies

| Learning Element | Owner | Source Locator |
|------------------|-------|----------------|
| Rite catalog | **Rites** | Lines 2562-2702 |
| Rite level | **Rites** | Lines 2562-2702 |
| Rite category | **Rites** | Lines 2564-2574 |
| Rite prerequisites (Rituais knowledge) | **Rites** | Line 2578 |
| Teacher/master requirement | **Rites** | Line 2578 ("mestre ou ancião") |
| Intelligence + Rituais extended test | **Action Resolution** (primitive) | Line 2578 |
| Accumulated successes = level | **Rites** (semantics) | Line 2578 |
| Study duration (weeks) | **Chronicle downtime** | Line 2578 ("semanas de estudo") |
| Payment/favors/amulets | **NPC/World** | Line 2578 ("pagamento de favores ou amuletos") |
| XP cost | **Progression** (NOT source-defined) | No explicit XP cost in source |

**Ownership split confirmed:**
- **Rites** owns: catalog, level, category, execution definition, knowledge prerequisite, learned-Rite identity
- **Progression** owns: XP transaction only if source defines it (source does NOT define XP cost for Rite learning)
- **Chronicle** owns: downtime/weeks elapsed, persistence/lifecycle
- **NPC/World** owns: master/elder availability, favors/material acquisition
- **Action Resolution** owns: Extended/Resisted primitive execution

---

## 14. Extended/Opposed-Action Requirements

### Extended Test Primitive Required

**Source requirement (Line 2578):**
> "acumulando sucessos equivalentes ao nível do ritual ao longo de semanas de estudo dedicadas"

**Source requirement (Line 2588):**
> "Teste prolongado e resistido de Raciocínio + Rituais (dificuldade 7) contra o espírito do caern"

**Source requirement (Line 2602):**
> "Teste prolongado de Raciocínio + Rituais... acúmulo de 40 sucessos (testes horários durante a noite)"

**Exact primitive required:**
- `ExtendedTestDefinition`: pool, difficulty, requiredSuccesses, interval (per turn/hour/session), failureSemantics, botchSemantics
- `ExtendedTestProgress`: currentSuccesses, attempts, abort state

**Rite keys requiring ExtendedTestDefinition:**
- `rite.caern.opening` (prolongado, resisted)
- `rite.caern.creation` (prolongado, 40 successes, hourly)
- `rite.mystic.fetish` (difficulty 10, implied extended for safety)
- `rite.mystic.summoning` (hourly reduction, extended conjuration)
- Learning ALL Rites (Intelligence + Rituals, weeks of study)

### Resisted/Opposed Primitive Required

**Source requirement (Line 2588):**
> "Teste prolongado e resistido de Raciocínio + Rituais (dificuldade 7) contra o espírito do caern (parada igual ao nível do caern, dificuldade igual à Gnose do personagem, sucessos necessários iguais à Força de Vontade do personagem)"

**Exact primitive required:**
- `ResistedTestDefinition`: attackerPool, defenderPool, attackerDifficulty, defenderDifficulty, requiredSuccesses, oppositionSemantics
- `OpposedTestResult`: netSuccesses, winner, failureMode

**Rite keys requiring ResistedTestDefinition:**
- `rite.caern.opening` (explicitly resisted)
- `rite.mystic.commitment` (Willpower vs spirit Gnose)

---

## 15. Randomness Boundary

**Source-defined randomness:**
- Dice pools and difficulties are source-defined
- Success thresholds are source-defined
- Failure/botch semantics follow generic rules (lines 2704-2707)

**Chronicle-owned randomness:**
- Dice generation
- Success counting
- Botch determination (1s cancel successes)

**Rite-owned determinism:**
- Pool composition
- Difficulty calculation
- Required successes
- Effect magnitude scaling
- State transitions on success/failure/botch

---

## 16. Ambiguities/Human Decisions

| ID | Rite Keys Affected | Source Locator | Ambiguity | Why Deterministic Implementation Cannot Proceed |
|-----|---------------------|----------------|-----------|-----------------------------------------------|
| A-010 | ALL | Lines 1619-1630, 2578 | Rites as Background vs Knowledge vs ritual itself | Stable keys must not collide; purchasing vs learning semantics unclear |
| A-010b | `rite.caern.opening` | Line 2588 | "sucessos necessários iguais à Força de Vontade do personagem" vs "sucessos necessários iguais ao nível do caern-alvo" (line 2595) | Conflicting success thresholds for opposed Caern tests |
| A-010c | `rite.caern.creation` | Line 2602 | "dificuldade inicial 8, reduzida por grupos de 5 participantes extras" | Exact reduction formula per participant group unclear |
| A-010d | `rite.mystic.fetish` | Line 2692 | "Cada ponto de Gnose permanente investido reduz a dificuldade em 2 pontos" | Whether permanent Gnose is spent or merely committed is unclear |
| A-010e | `rite.punishment.satirical-ritual` | Line 2640 | "Falha crítica faz o mestre do ritual perder 5 pontos de Sabedoria e virar o alvo do rito" | Botch consequence creates target swap; state transition unclear |
| A-010f | `rite.pact.luna-mutable` | Line 2622 | "Visto com grande desconfiança por tribos tradicionais" | Social penalty magnitude undefined |

**Note:** A-010 is the parent ambiguity for all Rite key/semantic issues.

---

## 17. Proposed Implementation Package Boundary

**Package:** `Chronicle.RuleSets.Werewolf.Rites`

**Owned domains:**
- Rite catalog (32 entries)
- Rite category/level taxonomy
- Rite execution definitions
- Rite learning prerequisites
- Learned-Rite identity and state

**Excluded domains:**
- Extended/Resisted test primitives (owned by Action Resolution)
- Spirit/Umbra mechanics (owned by Spirits domain)
- Pack/Sept/Totem aggregation (owned by Pack domain)
- Renown/Rank state transitions (owned by Progression)
- Chronicle downtime (owned by Chronicle lifecycle)

---

## 18. Proposed Implementation Order

### RITE-WAVE-A: Dependency-free executable Rites (0 Rites)

**Finding: No Rites are dependency-free.**

All 32 Rites require at least one of:
- Extended test primitive (learning)
- Resisted/Opposed primitive (Caern opening)
- Spirit/Umbra dependency (Mystic Rites)
- Pack/Sept/Totem dependency (Caern Rites)
- Renown/Rank dependency (Renown Rites)
- Human Decision (A-010 unresolved)

**First executable Rite wave cannot start independently without shared primitives.**

### RITE-WAVE-B: Small Rite-only typed extension (0 Rites)

No Rites qualify. All require shared primitives or external domains.

### RITE-WAVE-C: Extended/Resisted dependent (6 Rites)

Prerequisite: ExtendedTestDefinition and ResistedTestDefinition primitives implemented.

- `rite.caern.opening` (extended + resisted)
- `rite.caern.creation` (extended)
- `rite.mystic.fetish` (extended implied)
- `rite.mystic.summoning` (extended implied)
- Learning transaction for ALL Rites

### RITE-WAVE-D: Spirits/Umbra dependent (5 Rites)

Prerequisite: Spirit/Umbra domain implemented.

- `rite.mystic.commitment`
- `rite.mystic.fetish`
- `rite.mystic.totem`
- `rite.mystic.summoning`
- `rite.mystic.awaken-spirits`

### RITE-WAVE-E: Pack/Sept/Totem dependent (7 Rites)

Prerequisite: Pack/Sept/Totem domain implemented.

- `rite.caern.assembly`
- `rite.caern.badger-set`
- `rite.caern.moon-bridge`
- `rite.caern.hidden-ravine`
- `rite.caern.creation`
- `rite.periodic.cold-winds`
- `rite.periodic.new-awakening`

### RITE-WAVE-F: Renown/Rank dependent (7 Rites)

Prerequisite: Renown/Rank state machine implemented.

- `rite.renown.conquest`
- `rite.renown.passage`
- `rite.renown.wounding`
- `rite.punishment.ostracism`
- `rite.punishment.stone-of-scorn`
- `rite.punishment.jackal-voice`
- `rite.punishment.satirical-ritual`

### RITE-WAVE-G: Human Decision / narrative boundary (7 Rites)

Prerequisite: A-010 resolved; narrative mechanics defined.

- `rite.pact.luna-mutable` (social penalty undefined)
- `rite.punishment.veil-laceration` (Delirium exposure)
- `rite.punishment.ciala-avenging-teeth` (death sentence)
- `rite.death.winter-wolf` (narrative death)
- `rite.caern.creation` (permanent Gnose sacrifice)
- `rite.mystic.initiation` (permanent damage risk)
- `rite.mystic.fire-baptism` (spirit mark disappearance)

---

## 19. Exact Matrix Rows Affected

Read from `docs/reviews/werewolf-rule-set-completeness/completeness-matrix.json` (not modified):

| Row Key | mechanicalCompleteness | currentSliceExecutable | packageExposure |
|---------|------------------------|------------------------|-----------------|
| `Rite definitions` | false | false | declared-out-of-scope |
| `Rite knowledge requirements` | false | false | declared-out-of-scope |
| `Rite execution` | false | false | declared-out-of-scope |
| `Rite costs` | false | false | declared-out-of-scope |

**Current values confirmed. No modifications made.**

---

## 20. Ownerless Blockers

| Blocker | Current Owner | Assigned Owner | Status |
|---------|---------------|----------------|--------|
| ExtendedTestDefinition primitive | Unassigned | **Action Resolution** | Assigned |
| ResistedTestDefinition primitive | Unassigned | **Action Resolution** | Assigned |
| Spirit/Umbra domain | Unassigned | **Spirits/Umbra workstream** | Assigned |
| Pack/Sept/Totem aggregation | Unassigned | **Pack/Sept workstream** | Assigned |
| Renown/Rank state machine | Unassigned | **Progression workstream** | Assigned |
| A-010 ambiguity resolution | Unassigned | **Rites + Documentation** | Assigned |
| Chronicle downtime lifecycle | Unassigned | **Chronicle Application** | Assigned |

**Target: ownerless blockers = 0. All assigned.**

---

## 21. Exact Files Changed

**Only this evidence document was created/modified:**

- `docs/reviews/werewolf-rule-set-completeness/completion-evidence/RULESET-COMPLETION-RITES-AUDIT.md` (created)

**No other files modified.**

---

## 22. Git Status

```text
A  docs/reviews/werewolf-rule-set-completeness/completion-evidence/RULESET-COMPLETION-RITES-AUDIT.md
```

**No commits made. No pushes made.**

---

## 24. Corrected Primary Classification (A-J, sum = 32)

| Primary Class | Count | Rite Keys |
|---------------|-------|-----------|
| **A** — executable with current primitives | 1 | `rite.mystic.hunting-stone` |
| **B** — small Rite-only typed extension | 0 | — |
| **C** — requires Extended test primitive | 2 | `rite.caern.opening`, `rite.caern.creation` |
| **D** — requires Resisted/Opposed primitive | 0 | — |
| **E** — requires Spirits/Umbra | 7 | `rite.pact.purification`, `rite.pact.contrition`, `rite.mystic.fire-baptism`, `rite.mystic.commitment`, `rite.mystic.initiation`, `rite.mystic.awaken-spirits`, `rite.mystic.summoning` |
| **F** — requires Pack/Sept/Totem | 8 | `rite.caern.assembly`, `rite.caern.badger-set`, `rite.caern.moon-bridge`, `rite.caern.hidden-ravine`, `rite.punishment.the-hunt`, `rite.mystic.totem`, `rite.periodic.cold-winds`, `rite.periodic.new-awakening` |
| **G** — requires Renown/Rank | 8 | `rite.death.commemoration`, `rite.punishment.ostracism`, `rite.punishment.stone-of-scorn`, `rite.punishment.jackal-voice`, `rite.punishment.satirical-ritual`, `rite.renown.conquest`, `rite.renown.passage`, `rite.renown.wounding` |
| **H** — requires Fetish/Talen | 2 | `rite.mystic.talisman-dedication`, `rite.mystic.fetish` |
| **I** — narrative/adjudication boundary | 3 | `rite.death.winter-wolf`, `rite.punishment.veil-laceration`, `rite.punishment.ciala-avenging-teeth` |
| **J** — Human Decision / source ambiguity | 1 | `rite.pact.luna-mutable` |
| **Total** | **32** | |

**Note:** `rite.mystic.commitment` is primary class **E** (Spirits/Umbra) with secondary **D** (Resisted).
`rite.caern.opening` is primary class **C** (Extended) with secondary **D** (Resisted).

---

## 25. Secondary Dependency Sets

These are cross-cutting dependencies that do NOT affect primary-class arithmetic.

### Secondary Extended-test dependency
- `rite.caern.opening`
- `rite.caern.creation`

### Secondary Resisted-test dependency
- `rite.caern.opening`
- `rite.mystic.commitment`

### Secondary Spirit/Umbra dependency
- `rite.mystic.commitment`
- `rite.mystic.fetish`
- `rite.mystic.totem`
- `rite.mystic.fire-baptism` (soft)
- `rite.death.commemoration` (soft)
- `rite.periodic.cold-winds` (soft)
- `rite.periodic.new-awakening` (soft)

### Secondary Pack/Sept/Totem dependency
- `rite.mystic.totem`
- `rite.death.commemoration`

### Secondary Renown/Rank dependency
- `rite.death.commemoration`

### Secondary Fetish/Talen dependency
- `rite.mystic.fetish`

---

## 26. Dependency-Free First Rite Wave

**Dependency-free executable Rite set: 1 Rite**

- `rite.mystic.hunting-stone` — simple Carisma + Rituais test, no extended/resisted, no Spirits/Umbra hard dependency, no Pack/Sept hard dependency, no Renown/Rank dependency, no Fetish/Talen dependency, no Human Decision.

All other 31 Rites require at least one shared primitive or external domain.

---

## 27. Human Decisions (Genuine Source Ambiguities Only)

| ID | Affected Rite Keys | Source Locator | Ambiguity | Why Deterministic Implementation Cannot Proceed |
|-----|---------------------|----------------|-----------|-----------------------------------------------|
| A-010 | ALL 32 Rites | Lines 1619-1630, 2578 | Rites as Background vs Knowledge vs ritual itself | Stable keys must not collide; purchasing vs learning semantics unclear |
| A-010c | `rite.caern.creation` | Line 2602 | "dificuldade inicial 8, reduzida por grupos de 5 participantes extras" | Exact reduction formula per participant group unclear |
| A-010d | `rite.mystic.fetish` | Line 2692 | "Cada ponto de Gnose permanente investido reduz a dificuldade em 2 pontos" | Whether permanent Gnose is spent or merely committed is unclear |
| A-010e | `rite.punishment.satirical-ritual` | Line 2640 | "Falha crítica faz o mestre do ritual perder 5 pontos de Sabedoria e virar o alvo do rito" | Botch consequence creates target swap; state transition unclear |
| A-010f | `rite.pact.luna-mutable` | Line 2622 | "Visto com grande desconfiança por tribos tradicionais" | Social penalty magnitude undefined |

**Note:** A-010b was removed. Lines 2588 and 2595 describe two different Rites (`rite.caern.opening` vs `rite.caern.moon-bridge`) with legitimately different success thresholds. This is not a source contradiction.

---

## 28. Corrected Proposed Implementation Waves

### RITE-WAVE-A: Dependency-free executable Rites (1 Rite)

- `rite.mystic.hunting-stone`

### RITE-WAVE-B: Small Rite-only typed extension (0 Rites)

No Rites qualify.

### RITE-WAVE-C: Extended/Resisted dependent (2 Rites)

Prerequisite: ExtendedTestDefinition and ResistedTestDefinition primitives implemented.

- `rite.caern.opening` (extended + resisted)
- `rite.caern.creation` (extended)

### RITE-WAVE-D: Spirits/Umbra dependent (7 Rites)

Prerequisite: Spirit/Umbra domain implemented.

- `rite.pact.purification`
- `rite.pact.contrition`
- `rite.mystic.fire-baptism`
- `rite.mystic.commitment`
- `rite.mystic.initiation`
- `rite.mystic.awaken-spirits`
- `rite.mystic.summoning`

### RITE-WAVE-E: Pack/Sept/Totem dependent (8 Rites)

Prerequisite: Pack/Sept/Totem domain implemented.

- `rite.caern.assembly`
- `rite.caern.badger-set`
- `rite.caern.moon-bridge`
- `rite.caern.hidden-ravine`
- `rite.punishment.the-hunt`
- `rite.mystic.totem`
- `rite.periodic.cold-winds`
- `rite.periodic.new-awakening`

### RITE-WAVE-F: Renown/Rank dependent (8 Rites)

Prerequisite: Renown/Rank state machine implemented.

- `rite.death.commemoration`
- `rite.punishment.ostracism`
- `rite.punishment.stone-of-scorn`
- `rite.punishment.jackal-voice`
- `rite.punishment.satirical-ritual`
- `rite.renown.conquest`
- `rite.renown.passage`
- `rite.renown.wounding`

### RITE-WAVE-G: Human Decision / narrative boundary (3 Rites)

Prerequisite: A-010 resolved; narrative mechanics defined.

- `rite.death.winter-wolf`
- `rite.punishment.veil-laceration`
- `rite.punishment.ciala-avenging-teeth`

### RITE-WAVE-H: Fetish/Talen dependent (2 Rites)

Prerequisite: Fetish/Talen domain implemented.

- `rite.mystic.talisman-dedication`
- `rite.mystic.fetish`

---

## 29. Ownerless Blockers

| Blocker | Assigned Owner |
|---------|----------------|
| ExtendedTestDefinition primitive | Action Resolution |
| ResistedTestDefinition primitive | Action Resolution |
| Spirit/Umbra domain | Spirits/Umbra workstream |
| Pack/Sept/Totem aggregation | Pack/Sept workstream |
| Renown/Rank state machine | Progression workstream |
| Fetish/Talen domain | Items/Fetishes workstream |
| A-010 ambiguity resolution | Rites + Documentation |
| Chronicle downtime lifecycle | Chronicle Application |

**Target: ownerless blockers = 0. All assigned.**

---

## 30. Final Reconciliation Summary

| Question | Answer |
|----------|--------|
| Exact Rite count | **32** |
| Dependency-free Rite wave | **1 Rite** (`rite.mystic.hunting-stone`) |
| First independent wave | **RITE-WAVE-A** (1 Rite, no external dependencies) |
| Can RITE-WAVE-A start independently? | **Yes** |
| Primary A-J counts | A=1, B=0, C=2, D=0, E=7, F=8, G=8, H=2, I=3, J=1 (sum=32) |
| Human Decisions | **5** (A-010, A-010c, A-010d, A-010e, A-010f) |
| Ownerless blockers | **0** (all assigned) |
| Exact files changed | **1** (evidence document only) |
| Matrix rows modified | **0** |

</environment_details>
