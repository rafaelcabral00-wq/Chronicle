# AUDIT-WEREWOLF-RITES-EXECUTION-2026-08-25: Werewolf Rites Execution Reconciliation

**Status:** Complete
**Date:** 2026-08-25
**Auditor:** Kilo (automated source-authority audit)
**Scope:** Authoritative execution reconciliation for all 32 canonical Rites
**Canonical Source:** `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt`
**Baseline:** af6d26a

---

## 1. Canonical Rite Inventory Authority

**Total canonical Rites: 32**
**Duplicate count: 0**

Source: Lines 2562-2702 of canonical source.

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

## 2. Exact A-H Primary Counts (sum = 32)

| Primary Class | Count | Description |
|---------------|-------|-------------|
| **A** | 1 | Executable now with current af6d26a primitives |
| **B** | 8 | Spirit/Umbra dependency |
| **C** | 9 | Pack/Sept/Totem aggregate/runtime dependency |
| **D** | 8 | Renown/Rank dependency |
| **E** | 2 | Fetish/Talen dependency |
| **F** | 3 | Chronicle/world/narrative interaction boundary |
| **G** | 1 | Human Decision / source ambiguity |
| **H** | 0 | Small isolated Rite-only extension |
| **Total** | **32** | |

---

## 3. Exact Class-A Rite Key

- `rite.mystic.hunting-stone`

---

## 4. Primary Disposition of Important Rites

### `rite.mystic.hunting-stone`
- **Primary class: A**
- Entire deterministic Werewolf-side execution can be implemented now.
- No Spirit/Umbra state required.
- No Pack/Sept aggregate state required.
- No Chronicle/world interaction boundary required.
- No Human Decision required.
- Simple Carisma + Rituais test, standard difficulty, provides general location only.

### `rite.caern.opening`
- **Primary class: B** (Spirit/Umbra dependency)
- Deterministic Werewolf-side execution CANNOT be implemented now.
- Requires Spirit/Umbra state: caern spirit identity, communication, and opposition.
- Extended/Resisted primitives are available, but the caern spirit entity itself is the blocker.
- Also requires Caern location (Pack/Sept aggregate), but Spirit/Umbra is the primary blocker.

### `rite.caern.creation`
- **Primary class: C** (Pack/Sept/Totem aggregate/runtime dependency)
- Deterministic Werewolf-side execution CANNOT be implemented now.
- Requires 13 Garou participants (aggregate), leader with Gnose, Caern location.
- Also requires Spirit/Umbra for the new Caern's spirit, but Pack/Sept aggregate is the primary blocker.
- Baseline af6d26a contains declarative Pack/Totem definitions only — not runtime aggregate state.

---

## 5. Separation: Definition / Learning / Execution

### A. Rite Definition
- **Owner:** Rites
- **Scope:** Catalog of 32 Rites with stable keys, categories, levels, test definitions, costs, durations, effects.
- **Status:** Defined in canonical source; catalog foundation can be materialized.

### B. Rite Learning/Acquisition
- **Owner:** Shared boundary (Rites + Progression + Chronicle + NPC/World)
- **Source requirements:** teacher/master, Rituais knowledge compatible with level, Intelligence + Rituais extended test, accumulate successes = level, weeks of study, payment/favors/amulets.
- **XP:** NOT explicitly required by source.
- **Status:** NOT implemented in this package. A-010 preserves Background/Knowledge/stable-key semantics as unresolved.

### C. Rite Execution
- **Owner:** Rites
- **Scope:** Deterministic interpretation of a resolved roll against a known Rite definition.
- **Status:** Hunting Stone execution implemented in this package.

---

## 6. A-010 Impact

- **Scope:** ALL 32 Rites, primarily affecting LEARNING/ACQUISITION semantics.
- **Source locators:** Lines 1619-1630 (Rites as Background vs Knowledge), Line 2578 (learning prerequisites).
- **Impact on execution:** Minimal for Hunting Stone — the Rituais ability identifier is unambiguous in the current model (`character.ability.rituals`).
- **Impact on learning:** Unresolved. Stable key semantics must not collide; purchasing vs learning semantics unclear.
- **Status:** Preserved as Human Decision. Does not block Hunting Stone execution.

---

## 7. Secondary Dependency Counts

| Dependency | Count | Notes |
|------------|-------|-------|
| Extended | 3 | caern.opening, caern.creation, learning |
| Resisted | 2 | caern.opening, mystic.commitment |
| Spirit/Umbra | 15 | Multiple Mystic and Caern Rites |
| Pack/Sept/Totem | 11 | Caern, Periodic, and select Mystic Rites |
| Renown/Rank | 10 | Death, Punishment, Renown Rites |
| Fetish/Talen | 2 | talisman-dedication, fetish |
| Chronicle/world | 4 | winter-wolf, veil-laceration, ciala-avenging-teeth, the-hunt |
| A-010/Human Decision | 32 | ALL Rites (primarily affects learning) |

*Note: Secondary counts overlap and do NOT sum to 32.*

---

## 8. Ownerless Blockers

| Blocker | Assigned Owner | Status |
|---------|----------------|--------|
| ExtendedTestDefinition primitive | Action Resolution | Resolved |
| ResistedTestDefinition primitive | Action Resolution | Resolved |
| Spirit/Umbra domain | Spirits/Umbra workstream | Pending |
| Pack/Sept/Totem aggregation | Pack/Sept workstream | Pending |
| Renown/Rank state machine | Progression workstream | Pending |
| Fetish/Talen domain | Items/Fetishes workstream | Pending |
| A-010 ambiguity resolution | Rites + Documentation | Pending (learning only) |
| Chronicle downtime lifecycle | Chronicle Application | Pending |

**Ownerless blockers = 0. All assigned.**

---

## 9. Recommended First Implementation Package

**RITE-WAVE-A: `rite.mystic.hunting-stone`**

Rationale:
- Source-complete and deterministic.
- No external domain dependencies.
- Useful as reference architecture for later Mystic L1 Rites.
- Low conflict with Gift Wave B and Spirit/Umbra audit.
- Requires only existing ordinary action resolution + minimal Rite catalog metadata.
