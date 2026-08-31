# RULESET COMPLETION — Source Gap Reconciliation (Playable-Core Blockers)

## 1. Purpose

Re-read canonical source for every gap currently listed as a "playable-core
blocker" in `WEREWOLF-COMPLETION-OUTLOOK-2026-08-28.md` and the previous
S-reconciliation waves. Re-classify each gap and reassess whether it actually
blocks `PLAYABLE_CORE_COMPLETE`.

## 2. Gaps Reviewed (Source Pass)

For every gap below, the canonical source (`.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt`) was re-read in full context. The exact passages consulted are listed by line number.

### 2.1 A-010 — Rite learning semantics (umbrella)

#### 2.1.1 A-010a — Rite advancement cost

- **Source locator (Ritos Antecedente table):** Lines 1621–1630
- **Canonical statement:** "O nível do conhecimento Rituais limita o nível máximo dos ritos aprendidos. Dois Ritos Menores equivalem a um Rito de Nível 1."
- **Source locator (XP table):** Lines 2810–2821
- **Canonical statement:** The XP advancement table does **not** list Ritos as a separate line. Ritos is implicitly an Antecedente ("Antecedentes (exceto Totem) não podem ser comprados com XP, dependendo estritamente de eventos na narrativa").
- **Deterministic rule:** Yes — Ritos cannot be purchased with XP; they require narrative events.
- **Disposition:** **RESOLVED_BY_SOURCE**
- **Blocks playable core:** No.

#### 2.1.2 A-010b — Caern opening (extended + resisted) specifics

- **Source locator:** Line 2588
- **Canonical statement:** "Teste prolongado e resistido de Raciocínio + Rituais (dificuldade 7) contra o espírito do caern (parada igual ao nível do caern, dificuldade igual à Gnose do personagem, sucessos necessários iguais à Força de Vontade do personagem). O vencedor obtém bônus equivalentes ao nível do caern; o perdedor sofre ferimentos (agravados em caso de falha crítica)."
- **Deterministic rule:** Yes — fully specified.
- **Disposition:** **RESOLVED_BY_SOURCE** (already in S4 boundary payload).
- **Blocks playable core:** No.

#### 2.1.3 A-010c — Caern creation difficulty reduction formula

- **Source locator:** Line 2602
- **Canonical statement:** "dificuldade inicial 8, reduzida por grupos de 5 participantes extras" — **the source only states that groups of 5 reduce the difficulty, without giving an explicit numeric formula**.
- **Source locator (related):** Line 2577 (Rite general bonus): "Cada grupo de 5 Garou adicionais (além do mínimo de 3) que auxiliem ativamente reduz a dificuldade do ritual em 1 ponto (até o mínimo de 3)."
- **Cross-reference:** The general rite rule (Line 2577) explicitly states "reduz a dificuldade do ritual em 1 ponto" per group of 5 additional Garou. Line 2602's "reduzida por grupos de 5 participantes extras" omits the explicit -1 numeric but the context is the same ritual family.
- **Deterministic rule:** Inherited from the general Rite bonus rule (Line 2577): **-1 difficulty per group of 5 additional Garou beyond the minimum 3 (minimum difficulty 3)**.
- **Disposition:** **RESOLVED_BY_SOURCE** (cross-reference).
- **Blocks playable core:** No.

#### 2.1.4 A-010d — Fetish Rite Gnose investment: spend vs committed

- **Source locator:** Line 2692
- **Canonical statement:** "Cada ponto de Gnose **permanente** investido reduz a dificuldade em 2 pontos."
- **Deterministic rule:** Yes — "permanente" (permanent). The Gnose invested is permanent (consumed), not merely committed.
- **Disposition:** **RESOLVED_BY_SOURCE**
- **Blocks playable core:** No.

#### 2.1.5 A-010e — Satirical Ritual botch target swap

- **Source locator:** Line 2640
- **Canonical statement:** "Falha crítica faz o mestre do ritual perder 5 pontos de Sabedoria e virar o alvo do rito."
- **Deterministic rule:** Yes — botch effect is fully specified (5 Sabedoria loss + role swap).
- **Disposition:** **RESOLVED_BY_SOURCE**
- **Blocks playable core:** No.

#### 2.1.6 A-010f — Luna Mutable social penalty magnitude

- **Source locator:** Line 2622
- **Canonical statement:** "Mantém Dons antigos, mas perde acesso a futuros Dons do augúrio anterior, assumindo os do novo caminho. Visto com grande desconfiança por tribos tradicionais."
- **Search result:** Canonical source **does not** specify a numeric social penalty for the Luna Cambiante ritual. The source describes the consequence qualitatively ("grande desconfiança") only.
- **Deterministic rule:** No.
- **Disposition:** **SOURCE_UNSPECIFIED** (Narrative-boundary; not a mechanical blocker).
- **Blocks playable core:** No.

### 2.2 A-012 — Totem XP cost contradiction

- **Source locator A:** Line 1633 (Totem section narrative rule)
  - **Canonical statement:** "Este é o único Antecedente que pode ser evoluído com Pontos de Experiência (Custo: **2 Pontos de XP por ponto de Antecedente**)."
- **Source locator B:** Line 2820 (XP advancement table)
  - **Canonical statement:** "| **Totem (Antecedente)** | **3 pontos de XP por ponto de Totem** |"
- **Governs same purchase:** Yes. Both statements describe the XP cost to raise the Totem Antecedente.
- **Context distinction:** None. Both are canonical statements of the same rule.
- **Type:** Statement A is a section rule; Statement B is the master table. They differ.
- **Reconciliation possible:** No. They are explicit contradictory values for the same purchase.
- **Disposition:** **EXPLICIT_CONTRADICTION**
- **Existing code:** `WerewolfTotemDefinitions.A012Conflict` preserves both values with status "Unresolved". The runtime intentionally does not pick either value.
- **Blocks playable core:** No. Totem purchase with XP is a post-creation activity. The contradiction does not affect creation, ordinary play, or the S4 typed boundary for the S4 Rite.

### 2.3 Spirit death vs Modorra at Essence 0

- **Source locator:** Line 3410
- **Canonical statement:** "Essência: Representa os pontos de vitalidade e sobrevivência do espírito (soma de Força de Vontade, Fúria e Gnose). Ao esgotar-se, o espírito **morre, entra em Modorra ou é destruído**."
- **Deterministic rule:** No. Source presents a three-way OR (death / Modorra / destruction) without condition.
- **Modorra definition (Line 3460):** "Estado de total inatividade em um local retirado na Umbra onde espíritos com baixa Essência repousam para se reabastecer. Espíritos aprisionados em fetiches entram automaticamente em Modorra até serem liberados."
- **Deterministic rule:** Source only specifies: spirits imprisoned in fetishes auto-enter Modorra; otherwise the choice between death / Modorra / destruction is **GM/Storyteller decision**.
- **Disposition:** **SOURCE_UNSPECIFIED** for the general case; **RESOLVED_BY_SOURCE** for the fetish-imprisonment subcase.
- **Existing S2 behavior:** `WerewolfSpiritMechanicServices.ApplyDamage` flags "death/Modorra boundary unresolved" — this is the correct canonical behavior. S2 deliberately stops at the boundary.
- **Blocks playable core:** No. Spirit death is a Storyteller decision and is correctly deferred.

### 2.4 Spirit materialization duration

- **Source locator:** Line 3414
- **Canonical statement:** "Materializar: Permite assumir forma física na Terra se a Gnose for igual ou superior à Película local. Adota níveis de vitalidade físicos (geralmente sete)."
- **Other related:** Line 1774 (Gift Casulo): "Dura uma cena (ou mais, com gastos adicionais de Gnose)." — this is a Gift effect, not a base materialization rule.
- **Source-wide search result:** Canonical source does **not** specify a duration for the base "Materializar" Charm. The "geralmente sete" refers to health levels, not duration.
- **Deterministic rule:** No.
- **Disposition:** **SOURCE_UNSPECIFIED**
- **Blocks playable core:** No. Materialization duration is a Storyteller decision; S2 already validates Gnose ≥ Película and flips `IsMaterialized`.

### 2.5 Spirit possession control / permanence

- **Source locator (initial test + duration):** Lines 3442–3450
  - **Canonical statement:** "Possessão: O espírito possui um ser vivo ou objeto inativo. Requer sucesso em teste de Gnose (dificuldade igual à Força de Vontade da vítima). Tabela de velocidade [6h/3h/1h/15min/5min/instantâneo]... O espírito deve se isolar em uma parte escura da Umbra durante o processo, sem realizar outras ações; combate rompe o elo. O hospedeiro humano passa a ser chamado de fomori, numa relação permanente."
- **Initial test + duration:** Already implemented in S2.
- **Ongoing control / release / permanence:** Source specifies only:
  - Combat breaks the link (during process).
  - Host becomes fomori ("relação permanente").
- **Source-wide search:** Canonical source does **not** specify ongoing control mechanics, voluntary release rules, or permanence rules beyond the fomori consequence.
- **Deterministic rule:** No.
- **Disposition:** **SOURCE_UNSPECIFIED** (control mechanics) + **RESOLVED_BY_SOURCE** (combat breaks link; fomori is permanent).
- **Blocks playable core:** No. Possession is primarily a Storyteller scene mechanic; the only durable consequence (fomori) is canonical.

### 2.6 Non-Garou Gauntlet crossing

- **Source-wide search result:** Canonical source only describes Gauntlet/Película crossing rules for **Garou** (Lines 3276–3290: Gnose test vs local Gauntlet, time table, reflective surface modifier, retry restriction, botch, Fury restriction). No mechanics exist for humans, Kinfolk, vampires, mages, or fomori crossing.
- **Deterministic rule:** No.
- **Disposition:** **SOURCE_UNSPECIFIED**
- **Blocks playable core:** No. Kinfolk and humans are not required to cross the Gauntlet for ordinary canonical play. Garou crossing (which IS required) is fully specified.

## 3. Reassessed PLAYABLE_CORE_COMPLETE Blocker List

Per the rule: a blocker must satisfy **BOTH**:
1. affects ordinary canonical play rather than rare/full-materialization edge cases;
2. has no existing executable mechanic or truthful typed boundary that allows Chronicle to continue safely.

| Gap | Affects ordinary play? | Has safe boundary? | Truly blocks? |
|---|---|---|---|
| A-010a Rite advancement cost | No (post-creation) | Yes (table-based creation) | **No** |
| A-010b Caern opening | No (Rite execution) | Yes (S4 boundary) | **No** |
| A-010c Caern creation reduction | No (Caern foundation event) | Yes (inherits Line 2577) | **No** |
| A-010d Fetish Gnose investment | No (Rite execution) | Yes (S4 boundary) | **No** |
| A-010e Satirical botch | No (penalty) | Yes (line 2640) | **No** |
| A-010f Luna Mutable social penalty | Yes, but Storyteller territory | Yes (line 2622 narrative) | **No** |
| A-012 Totem XP | No (post-creation) | Yes (conflict preserved) | **No** |
| Spirit death/Modorra | Yes, but Storyteller territory | Yes (S2 boundary) | **No** |
| Materialization duration | Yes, but Storyteller territory | Yes (S2 boundary) | **No** |
| Possession control | Yes, but Storyteller territory | Yes (S2 + fomori) | **No** |
| Non-Garou crossing | No (only Garou cross) | N/A | **No** |

**Truly remaining PLAYABLE_CORE_COMPLETE blockers: 0**

All current source gaps either:
- Have a deterministic source rule (A-010a/b/c/d/e, partial possession);
- Are explicit contradictions preserved in code (A-012);
- Are Storyteller/GM-decision territory (Spirit death, materialization duration, possession control, Luna Mutable);
- Do not affect ordinary canonical play (non-Garou crossing, post-creation activities).

## 4. Updated PLAYABLE_CORE_COMPLETE Status

**Conclusion:** Per the repository's own definition in the Outlook, PLAYABLE_CORE_COMPLETE is **now materially achieved** by the source-faithful mechanics already implemented across S1–S5 waves. The remaining 7 `spirit.*` source-gap keys are correctly preserved as non-blocking source gaps. The remaining Rite catalog (~20) and Gift catalog (~125) are catalog backlog, not source-gap blockers.

**Caveat:** The 7 source-gap keys remain documented as `SOURCE_GAP` in the Outlook and are **not** mechanically resolved. The repository's definition of `MECHANICALLY_COMPLETE` (which includes Pack/Totem aggregate runtime, Renown/Rank state machine, Fetish/Talen domain) is **not** yet reached.

## 5. Code Changes

No production or test code changed in this reconciliation. This is a **documentation-only** pass.

## 6. Tests

No new tests added. Existing source-gap preservation tests in WerewolfS2Tests (and the `A012Conflict` constructor in `WerewolfTotemDefinitions`) remain valid.

## 7. Unresolved Decisions

- **A-012 (Totem XP):** Preserved as explicit contradiction. The 2 vs 3 XP choice must be a Human Decision at the table. Both values are exposed in `WerewolfTotemDefinitions.A012Conflict`.
- **Spirit death/Modorra:** Preserved as Storyteller decision. The S2 service does not auto-resolve.

## 8. Ownerless Blockers

0. All remaining gaps are either resolved, preserved contradictions, or Storyteller/GM territory — none are ownerless.

## 9. Files Changed

- NEW: `docs/reviews/werewolf-rule-set-completeness/completion-evidence/RULESET-COMPLETION-SOURCE-GAP-RECONCILIATION.md` (this file)
- MODIFIED: `docs/reviews/werewolf-rule-set-completeness/WEREWOLF-COMPLETION-OUTLOOK-2026-08-28.md` (Outlook updates)
