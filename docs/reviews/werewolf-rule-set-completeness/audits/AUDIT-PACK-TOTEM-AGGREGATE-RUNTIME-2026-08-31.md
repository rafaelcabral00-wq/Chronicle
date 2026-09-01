# AUDIT — PACK / TOTEM AGGREGATE RUNTIME (2026-08-31)

## 0. Purpose

This is an **audit-only** pass to determine the smallest correct runtime required
to move Werewolf from `PLAYABLE_CORE_COMPLETE` toward `MECHANICALLY_COMPLETE`
without inventing mechanics or violating Chronicle ownership boundaries.

**This document does NOT modify production code, tests, or the completeness
matrix. It is a specification precursor.**

---

## 1. Existing Implementation Inventory

### 1.1 Werewolf Pack/Totem Artifacts

| File | Type | Responsibility | Current Owner | Current State | Reusable? | Requires Change? |
|---|---|---|---|---|---|---|
| `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfTotemIdentifiers.cs` | Static class (constants) | 19 totem key constants + `Supported` list | Werewolf | 19 entries | Yes | No |
| `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfTotemCatalog.cs` | Static class | `ByKey`/`AllDefinitions`/`Get` of `WerewolfTotemCatalogEntry` | Werewolf | 19 totem entries with effects | Yes | No |
| `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfTotemCatalogEntry.cs` | Sealed record | `(TotemKey, CanonicalName, NameEn, NamePtBr, SourceLocator, BackgroundCost, PatronTribeKey, Effects)` | Werewolf | 19 instances | Yes | No |
| `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfTotemEffect.cs` | Enum + record | `WerewolfTotemEffectKind` (13 kinds incl. `PackWideBenefit`) + effect record | Werewolf | 95 total effects across 19 totems | Yes | No |
| `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfTotemDefinitions.cs` | Static class | Constants, `ImprovementTable` (9 entries), `A012Conflict` (preserved contradiction), `RitualOfTotem`, `RitualOfContrition`, `BanirTotemGift`, `CalculateBeneficiaryCount` | Werewolf | Catalog + helper | Yes | No |
| `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfPackDefinitions.cs` | Static class | Pack size, Litany rules (13), tactics (5), alpha challenge methods, augury roles, `CalculateMaxTactics(minGnosis)` | Werewolf | Static lore + 1 deterministic helper | Yes (helpers); lore is descriptive | No |
| `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRiteBoundaryContracts.cs` | Sealed records | `WerewolfTotemBindingBoundaryPayload` (S4) and `WerewolfContritionBoundaryPayload` (S4) | Werewolf | Transient Rite-success contracts | Yes (boundary only) | No (already nominal) |
| `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfSpiritUmbraBoundaryContracts.cs` | Sealed records | `WerewolfPackTotemLinkBoundaryPayload`, `WerewolfSharedTotemEffectsBoundaryPayload` (S5) | Werewolf | Persistent-linkage boundary contracts | Yes | No |
| `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfCharacterCreationDraftContracts.cs` | Sealed record | `WerewolfInitializedCharacterState` — NO Pack/Totem fields | Werewolf | Character draft state | Yes (will need PackId field added) | Yes (add `PackId?` reference) |
| `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRuntimeCharacterState.cs` | Sealed record | Runtime character state — NO Pack/Totem fields | Werewolf | Runtime snapshot | Yes (will need PackId) | Yes (add `PackId?` reference) |
| `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfProgressionContracts.cs` | Sealed records | `WerewolfProgressionErrorCode.TotemExperienceCostUnresolved` | Werewolf | A-012 surfaced | Yes | No |
| `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfAdvancementService.cs` | Static class | Totem XP cost check (returns `TotemExperienceCostUnresolved`) | Werewolf | Validates A-012 boundary | Yes | No |
| `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRiteExecutionService.cs` | Static class | Emits `WerewolfTotemBindingBoundaryPayload` for `rite.mystic.totem` | Werewolf | Rite runtime | Yes | No |
| `rule-sets/Chronicle.RuleSets.Werewolf/WerewolfReferenceRuntime.cs` | Sealed class | Registers `spirit-umbra.pack-totem-link` and `spirit-umbra.shared-totem-effects` operations | Werewolf | Runtime dispatch | Yes | No (operations correct) |
| `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfPackMaterializationTests.cs` | Test class | 11 tests against `WerewolfPackDefinitions` constants only | Werewolf.Tests | Catalog tests | Yes (will need new aggregate tests) | Yes (extend) |
| `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfTotemMaterializationTests.cs` | Test class | 13 tests against catalog data (95 effects, improvement table, A-012, Banir, Rites) | Werewolf.Tests | Catalog tests | Yes | No (catalog) |
| `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfSpiritUmbraS5BoundaryTests.cs` | Test class | `S5NoPackAggregateIntroduced` enforces no state mutation | Werewolf.Tests | Boundary tests | Yes | No (existing) |
| `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfRiteExecutionTests.cs` | Test class | `S4TotemReturnsTypedBoundaryOnSuccess`, `WaveDContritionReusesExistingTotemArtifact` | Werewolf.Tests | Rite tests | Yes | No (existing) |
| `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfProgressionTests.cs` | Test class | `AdvanceTotemReturnsFailureWithTotemUnresolved` | Werewolf.Tests | Progression tests | Yes | No (existing) |

### 1.2 Chronicle Core Project Inventory

| Project | Files | Aggregates | Repositories | IDocument | ICommand/Query/Event | Status |
|---|---|---|---|---|---|---|
| `src/Chronicle.Domain` | `AssemblyMarker.cs` (5 lines) | NONE | NONE | NONE | NONE | Skeleton |
| `src/Chronicle.Contracts` | `AssemblyMarker.cs`, `Dice/IDiceValueGenerator.cs`, `Dice/DiceSize.cs`, `Dice/DiceRollRequest.cs`, `Dice/DiceRollResult.cs`, `Dice/DiceRollFailureCode.cs` | NONE | NONE | NONE | NONE | Skeleton (Dice only) |
| `src/Chronicle.Application` | `AssemblyMarker.cs`, `Resources/ResourceTransitionOrchestrator.cs`, `Dice/DiceRollService.cs`, `Dice/ActionResolutionOrchestrator.cs` | NONE | NONE | NONE | NONE | Skeleton (orchestrators only) |
| `src/Chronicle.Infrastructure` | `AssemblyMarker.cs`, `Dice/SystemDiceValueGenerator.cs`, `Dice/ScriptedDiceValueGenerator.cs` | NONE | NONE | NONE | NONE | Skeleton (Dice only) |
| `src/Chronicle.Persistence.Sqlite` | `AssemblyMarker.cs` | NONE | NONE | NONE | NONE | Skeleton |
| `src/Chronicle.RuleSets.Abstractions` | `AssemblyMarker.cs`, `Manifests/*`, `PackageSources/*`, `Runtime/RuleSetRuntimeContracts.cs` | N/A (Rule Set registry) | N/A | N/A | N/A | Real (registry only) |

**Key finding: there is NO Chronicle aggregate root infrastructure. There is no DDD building block. The `Chronicle.Domain` project is a 5-line placeholder.** This is a fundamental architectural gap that Pack/Totem must NOT silently paper over.

### 1.3 Cross-Cutting Patterns Found

- **Application orchestration pattern:** `ResourceTransitionOrchestrator` and `ActionResolutionOrchestrator` are plain classes that talk directly to `RuleSetRuntimeRegistry`. They take a request with `Dictionary<string,string>` inputs and return results with `Dictionary<string,string>` outputs.
- **Rule Set runtime pattern:** `IRuleSetRuntime` (interface) → `WerewolfReferenceRuntime` (implementation). `RuleSetOperationRequest` carries operation key + `Dictionary<string,string>` inputs. `RuleSetOperationResult` carries bool + `Dictionary<string,string>` outputs.
- **No aggregate/repository/document pattern exists.** This is the **biggest audit risk** for Pack/Totem — there is no established DDD pattern to follow.

### 1.4 Pack/Totem Service Operations in Runtime (current)

- `spirit-umbra.pack-totem-link` → `ExecutePackTotemLink` → emits `WerewolfPackTotemLinkBoundaryPayload` (validates PackId, TotemId).
- `spirit-umbra.shared-totem-effects` → `ExecuteSharedTotemEffects` → emits `WerewolfSharedTotemEffectsBoundaryPayload` (validates non-empty effectKeys).
- `rite-runtime.execute-rite` (with `rite.mystic.totem` key) → `WerewolfTotemBindingBoundaryPayload` (S4 Rite boundary, transient).
- `rite-runtime.execute-rite` (with `rite.pact.contrition` key) → `WerewolfContritionBoundaryPayload` (S4 Rite boundary, transient).
- `WerewolfProgressionErrorCode.TotemExperienceCostUnresolved` returned for Totem XP advancement (A-012 preserved).

**These operations are typed-boundary-only. No state mutation occurs inside the Werewolf rule set.**

---

## 2. Canonical Pack State (from `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt`)

For every field below, the canonical source statement, source locator, and
classification are provided. "Source unspec." = canonical source does not
state this. "Implied" = canonical source implies but does not explicitly
state.

### 2.1 Pack identity (REQUIRED)

- **Source:** Line 435: "Matilha: Célula operacional básica de lobisomens unidos por um propósito comum."
- **Source:** Line 191: "A matilha é a unidade fundamental da sociedade Garou (geralmente formada por 2 a 10 lobisomens). Nela, preconceitos tribais são colocados de lado em prol do trabalho em equipe."
- **Deterministic:** Yes (size range 2-10).
- **Owner:** **CHRONICLE_OWNED** (persistent entity).
- **Required for playable core:** Yes (every chronicle has at least one Pack).
- **Required for mechanically complete:** Yes.
- **Already represented:** No.

### 2.2 Pack name (REQUIRED)

- **Source:** Line 870: "Matilha e Totem: O nome do grupo e o espírito patrono escolhido coletivamente pelos jogadores."
- **Deterministic:** No (player-chosen).
- **Owner:** **CHRONICLE_OWNED**.
- **Required:** Yes.
- **Already represented:** No.

### 2.3 Pack purpose (REQUIRED)

- **Source:** Line 194: "Objetivo Comum: Toda matilha é unida por um propósito (proteger um Caern, erradicar a Wyrm em uma região, prestar auxílio político, etc.)."
- **Source:** Line 1002: "Missão Comum: Qual é a causa ou inimigo comum que os une (destruir um Maldito, defender um Caern, vingança)?"
- **Deterministic:** No (player-defined narrative).
- **Owner:** **NARRATIVE_BOUNDARY** (descriptive).
- **Required:** Yes for narrative; not for mechanics.

### 2.4 Pack members (REQUIRED)

- **Source:** Line 191: "geralmente formada por 2 a 10 lobisomens".
- **Source:** Line 1000-1004: defines 5 creation pillars; member identity implicit.
- **Source:** Line 1632: "Os pontos investidos por cada jogador são somados para definir o poder do espírito patrono da matilha." — implies a roster exists.
- **Deterministic:** Size bounds only.
- **Owner:** **CHRONICLE_OWNED** (set of character IDs).
- **Required:** Yes.
- **Already represented:** No.

### 2.5 Pack leadership (REQUIRED)

- **Source:** Lines 184-189 (alpha challenge mechanics).
- **Source:** Line 1003: "Estrutura de Liderança (Alfa): Como as decisões são tomadas (desafio físico, liderança rotativa por augúrio ou um líder fixo)?"
- **Source:** Lines 151-152 (Litany: leader obedience in war, challenge in peace).
- **Deterministic:** Challenge mechanics (lines 184-187: physical/jogo/duelo).
- **Owner:** **CHRONICLE_OWNED** (current leader character ID) + **NARRATIVE_BOUNDARY** (leadership style).
- **Required:** Yes for state; challenge mechanics are part of combat/test infrastructure (already partially in `WerewolfCombatManeuverCatalog.cs`).
- **Already represented:** Partial (challenge methods in `WerewolfPackDefinitions`).

### 2.6 Pack dissolution (REQUIRED)

- **Source:** Line 199: "Caso a matilha alcance seu objetivo e decida se dissolver, o totem é libertado mediante cerimônia formal."
- **Deterministic:** No (narrative event).
- **Owner:** **CHRONICLE_OWNED** (state transition); ceremony = **NARRATIVE_BOUNDARY**.
- **Required:** Yes for state machine.

### 2.7 Pack Renome (collective)

- **Source:** Line 2853: "A matilha é tratada como uma entidade única. Se vários membros participarem de um feito, cada um recebe a proporção completa de Renome correspondente ao nível total da ameaça, independentemente de quem desferiu o golpe fatal."
- **Source:** Line 1648+: individual character Renome.
- **Deterministic:** No (RP-gated; not numeric).
- **Owner:** **NARRATIVE_BOUNDARY** (collective renome is per-character).
- **Required:** No (not deterministic).

### 2.8 Pack XP

- **Source-wide search:** No canonical statement of "Pack XP" pool. Each member tracks their own XP (lines 2798-2803). Pack has no collective XP pool.
- **Deterministic:** N/A.
- **Owner:** N/A.
- **Required:** No.

### 2.9 Pack gifts (collective)

- **Source:** Line 3733: "Concedem características temporárias ou permanentes (atributos, habilidades, bônus de renome ou pontos de Força de Vontade) que podem ser alternados entre os membros da matilha a cada turno."
- **Source-wide:** No concept of "Pack gifts" as a separate list. Gifts are character-level (lines 1724-1728).
- **Deterministic:** No.
- **Owner:** N/A.
- **Required:** No.

### 2.10 Pack tactics

- **Source:** Lines 3182-3191.
- **Source:** Line 3183: "A matilha conhece um número máximo de táticas igual à menor pontuação de Gnose entre seus membros. Requer ligação espiritual (Totem)."
- **Deterministic:** Yes — `min(memberGnosis)`.
- **Owner:** **CHRONICLE_OWNED** (list of acquired tactics, owned per-pack) + deterministic helper in Werewolf (`WerewolfPackDefinitions.CalculateMaxTactics`).
- **Required:** Yes for mechanically complete (MECHANICALLY_COMPLETE gate).
- **Already represented:** Helper exists in `WerewolfPackDefinitions.CalculateMaxTactics`. No aggregate list.

### 2.11 Pack/Totem linkage

- **Source:** Line 1632: shared by all members; aggregate sum of points.
- **Source:** Line 197-199: every Pack binds to one Totem; dissolution releases Totem.
- **Deterministic:** Yes — exactly one active Totem per Pack.
- **Owner:** **CHRONICLE_OWNED** (persistent).
- **Required:** Yes.

### 2.12 Pack territory / base

- **Source:** Line 1001: "Território e Base: Onde fica o refúgio do grupo? É urbano, rural ou selvagem?"
- **Deterministic:** No.
- **Owner:** **NARRATIVE_BOUNDARY** + Chronicle world.
- **Required:** No.

### 2.13 Pack allies / enemies

- **Source:** Line 1005: "Seita e Aliados/Inimigos: Qual a relação do grupo com a comunidade local de lobisomens e com a Wyrm?"
- **Deterministic:** No.
- **Owner:** **NARRATIVE_BOUNDARY**.

### 2.14 Pack succession (post-dissolution)

- **Source-wide search:** No canonical statement of Pack succession mechanic. Renúncia (line 2856) is per-character.
- **Deterministic:** No.
- **Owner:** **SOURCE_UNSPECIFIED** / NARRATIVE_BOUNDARY.

### 2.15 Pack-level combat

- **Source:** Lines 3182-3191 (5 deterministic tactics).
- **Deterministic:** Yes (tactic definitions).
- **Owner:** Werewolf owns static catalog; Chronicle applies per-pack.
- **Required:** Yes for MECHANICALLY_COMPLETE (M2).

### 2.16 Pack summary

| Field | Required for PC? | Required for MC? | Owner | Already represented? |
|---|---|---|---|---|
| Pack identity | Yes | Yes | Chronicle | No |
| Pack name | Yes | Yes | Chronicle | No |
| Pack purpose | Narrative | Narrative | Narrative | No |
| Pack members | Yes | Yes | Chronicle | No |
| Pack leadership | Yes | Yes | Chronicle + Narrative | Partial |
| Pack dissolution | Yes | Yes | Chronicle + Narrative | No |
| Pack Renome | No | No | N/A (per-character) | N/A |
| Pack XP | No | No | N/A | N/A |
| Pack gifts | No | No | N/A | N/A |
| Pack tactics list | No | Yes | Chronicle + Werewolf | No (helper exists) |
| Pack/Totem linkage | Yes | Yes | Chronicle | No |
| Pack territory | No | No | Narrative | No |
| Pack allies/enemies | No | No | Narrative | No |
| Pack succession | No | No | Source unspec. | No |
| Pack-level combat | No | Yes | Chronicle + Werewolf | Static catalog only |

---

## 3. Canonical Totem State

Per the audit brief: separate into 9 distinct areas.

### 3.1 Totem identity / static definition (Werewolf)

- **Source:** Lines 3730-3820+ (totem list with Costs, Patronato, Características, Dogma).
- **Source:** Lines 3842-3865 (spirit stats for various totem avatars).
- **Deterministic:** Yes — static catalog.
- **Owner:** **Werewolf** (`WerewolfTotemCatalog`, `WerewolfTotemIdentifiers`).
- **Already represented:** Yes (complete).

### 3.2 Pack-owned Totem relationship state (Chronicle)

- **Source:** Line 1632: shared Antecedente; sum of member points.
- **Source:** Lines 1637-1647: improvement table (1/2/3/4/5 point benefits).
- **Source:** Line 1635: initial 8 points distributed among Rage/Willpower/Gnosis.
- **Source:** Line 199: dissolution releases Totem.
- **Deterministic:** Yes — sum of points, single current Totem per Pack, dissolution state.
- **Owner:** **CHRONICLE_OWNED** (persistent).
- **Required:** Yes.

### 3.3 Character-owned Totem relationship state

- **Source-wide search:** No canonical statement of a character having an individual relationship with a Pack Totem separate from Pack membership. Pack Totem is a **collective** Antecedente (line 1632). All members are bound by Pack membership.
- **Owner:** **N/A** (the relationship is purely a function of Pack membership).
- **Required:** No (it's a function of Pack membership).

### 3.4 Totem effects / benefits (static)

- **Source:** Lines 3736-3832 (per-totem Características); 3733 (general).
- **Deterministic:** Yes — static catalog.
- **Owner:** **Werewolf** (`WerewolfTotemCatalog.AllDefinitions` + `WerewolfTotemDefinitions.ImprovementTable`).
- **Required:** Yes.
- **Already represented:** Yes (95 effects across 19 totems).

### 3.5 Totem obligations / taboos

- **Source:** Line 3734: "Dogmas: Restrições comportamentais obrigatórias. O descumprimento resulta no isolamento do totem, perda de bônus e necessidade de realizar um Ritual de Contrição."
- **Source:** Lines 3800-3832 (per-totem Dogma).
- **Deterministic:** No — dogma compliance is a Storyteller judgment.
- **Owner:** **NARRATIVE_BOUNDARY** + Chronicle (records violations).
- **Required:** For narrative; not for mechanics. Already represented as static Dogma strings in catalog.

### 3.6 Totem communication / interaction

- **Source:** Line 1640: "O totem fala diretamente com a matilha sem necessidade do Dom Comunicação com Espíritos." (improvement benefit)
- **Source:** Line 3416: "Sentido de Orientação: Facilita a localização de direções e o uso de trilhas espirituais na Umbra."
- **Source:** Line 3415: "Reformar: Capacidade de dissolver a forma material para retornar aos Domínios natais na Umbra em um turno."
- **Deterministic:** Partial — base Sentido de Orientação + Reformar are static; "direct communication" is an upgrade.
- **Owner:** **Werewolf** (catalog) + **CHRONICLE_OWNED** (improvement state).
- **Already represented:** Initial charms in `WerewolfTotemDefinitions.InitialCharms`.

### 3.7 Totem approval / rejection

- **Source:** Line 575: "Ao concluir o Ritual de Passagem, o Totem da Tribo aceita ou rejeita o filhote." (tribal totem, not pack totem — but implies the same mechanic exists for Pack Totem)
- **Source:** Line 3734 (totem isolation consequence).
- **Deterministic:** No — Storyteller decision.
- **Owner:** **NARRATIVE_BOUNDARY**.

### 3.8 Totem departure / loss

- **Source:** Line 3734: "isolamento do totem" (consequence of violating dogma).
- **Source:** Line 199: dissolution releases Totem.
- **Source:** Line 2507: Banir o Totem gift: "os alvos perdem Características do totem, táticas de matilha e cooperação pelo resto da cena." (temporary, scene-scoped).
- **Deterministic:** No (story event).
- **Owner:** **CHRONICLE_OWNED** (state transition); **NARRATIVE_BOUNDARY** (trigger).

### 3.9 Totem advancement (XP)

- **Source A (Line 1633):** "2 Pontos de XP por ponto de Antecedente" — narrative rule.
- **Source B (Line 2820):** "3 pontos de XP por ponto de Totem" — XP table rule.
- **Deterministic:** Contradictory.
- **Disposition:** **EXPLICIT_CONTRADICTION** preserved in `WerewolfTotemDefinitions.A012Conflict`.
- **Owner of execution:** **Werewolf** runtime (returns `TotemExperienceCostUnresolved`).
- **Owner of resolution:** **CHRONICLE** (Human Decision at table or campaign-level resolution).

### 3.10 Totem state summary

| Area | Owner | Required for MC? | Already represented? |
|---|---|---|---|
| 3.1 Identity | Werewolf | Yes | Yes |
| 3.2 Pack↔Totem relationship | Chronicle | Yes | No |
| 3.3 Character↔Totem | N/A | N/A | N/A |
| 3.4 Effects/benefits (static) | Werewolf | Yes | Yes (95 effects) |
| 3.5 Obligations/taboos | Narrative | No | Yes (Dogma strings) |
| 3.6 Communication | Werewolf + Chronicle | Yes | Yes (base charms) |
| 3.7 Approval/rejection | Narrative | No | No (Tribal only) |
| 3.8 Departure/loss | Chronicle | Yes | No (event) |
| 3.9 XP advancement | Werewolf (boundary) + Chronicle (decision) | Yes | Yes (A-012 preserved) |

---

## 4. A-012 Interpretation (Totem XP contradiction)

- **Line 1633:** "Este é o único Antecedente que pode ser evoluído com Pontos de Experiência (Custo: 2 Pontos de XP por ponto de Antecedente)." — This is in the **Totem Antecedente** description. It governs "Xp por ponto de Antecedente" — character XP raised to **Totem (Antecedente) rating**.
- **Line 2820:** "| **Totem (Antecedente)** | 3 pontos de XP por ponto de Totem |" — This is in the **Cost of Elevation** master table. It governs the same purchase.

**Both statements describe the same operation:** spending character XP to raise the Pack's Totem Antecedente rating by 1.

- **Reconciliation possible:** No. Both are canonical.
- **Runtime representation:** `WerewolfTotemDefinitions.A012Conflict` already preserves both values (Line 1633=2, Line 2820=3, Status="Unresolved").
- **Progression runtime:** `WerewolfProgressionErrorCode.TotemExperienceCostUnresolved` is returned when a player attempts to raise Totem with XP.

**Design for the boundary (proposed, NOT implemented):**

The runtime should accept a `XpCostResolution` parameter from Chronicle:
```
WerewolfTotemAdvancementRequest(
    PackId, TotemId, NewRating, XpCostResolution = HumanDecision | TableValue,
    ProposedXpCostA = 2, ProposedXpCostB = 3
)
```

- If Chronicle provides a resolved cost → runtime applies.
- If Chronicle does not provide a cost → runtime returns `TotemExperienceCostUnresolved` (existing behavior).

The runtime must **not** pick 2 or 3. The contradiction is preserved.

---

## 5. Aggregate Boundary / Ownership Model

### 5.1 Verification of the expected ownership split

| Layer | Owns | Justification |
|---|---|---|
| Werewolf Rule Set | Canonical rules, identifiers, deterministic validation/calculation, typed boundaries | Source-faithful, deterministic, narrative-free |
| Chronicle Domain | Persistent Pack/Totem aggregate, member roster, Totem state, Pack dissolution, Banir-Totem effects, applied shared effects | Persistence and world ownership |
| Chronicle Application | Orchestration (commands, queries, event handlers) | Coordination |
| Narrative Intelligence | Dogma violations, dissolution narrative, Totem disposition/social | Story logic |

### 5.2 Verdict

**The expected ownership split is correct for this codebase.** Pack/Totem must:
- Be persisted in Chronicle (no state in Werewolf).
- Use Werewolf as the deterministic source of effects, costs, and identifiers.
- Never mutate Werewolf's existing boundary payloads to carry persistence state.

### 5.3 Architectural gap

**Critical finding:** `Chronicle.Domain` has no aggregate root pattern. Implementing the Pack/Totem aggregate requires first establishing the **minimum aggregate pattern** in `Chronicle.Domain` (or in `Chronicle.Application` if we follow the existing pattern of plain orchestrators). See §7.

---

## 6. Minimum Pack/Totem Aggregate

### 6.1 Aggregate root

**PackTotemAggregateRoot** (or similar) with the following fields:

| Field | Type | Source | Classification | Already implemented? |
|---|---|---|---|---|
| `PackId` | string (GUID) | Required identifier | REQUIRED | No |
| `PackName` | string | Line 870 | REQUIRED | No |
| `Members` | `IReadOnlyList<string>` (character IDs) | Line 191, 1632 | REQUIRED | No |
| `LeaderId` | `string?` | Lines 184-189 | OPTIONAL (Narrative fallback) | No |
| `TotemId` | `string?` | Lines 197, 1632 | REQUIRED (one Totem or none) | No |
| `TotemRating` | `int` (1-10, sum of member investments) | Line 1632 | REQUIRED | No |
| `TotemImprovementPurchases` | `IReadOnlyList<string>` (improvement keys) | Lines 1639-1647 | REQUIRED | No |
| `LinkState` | enum: `Unbound | Bound | Dissolving` | Line 199 | REQUIRED | No |
| `ActiveTactics` | `IReadOnlyList<string>` (tactic keys) | Lines 3182-3191 | REQUIRED for MC | No |
| `LastTotemXpResolution` | enum: `Unresolved | TwoXp | ThreeXp` (or `null` if no XP raised) | A-012 | REQUIRED for A-012 preservation | No (currently per-advancement only) |
| `EstablishedAt` | narrative timestamp | Line 1000 | OPTIONAL (narrative) | No |
| `DissolvedAt` | narrative timestamp | Line 199 | OPTIONAL (narrative) | No |
| `DogmaViolations` | `IReadOnlyList<DogmaViolationEvent>` | Line 3734 | NARRATIVE_ONLY | No |
| `BanirTotemEffects` | `IReadOnlyList<BanirTotemEffect>` | Line 2507 | FUTURE (post-MC) | No |
| `PackRenome` | collective renome | Line 2853 | NARRATIVE_ONLY (per-character mechanic) | N/A |

### 6.2 Operations

1. **Create Pack** — establishes identity, roster, name.
2. **Add/remove member** — updates roster.
3. **Bind Totem** — sets `TotemId`, `TotemRating`, purchases improvements.
4. **Dissolve Pack** — transitions to dissolved state, releases Totem.
5. **Advance Totem (XP)** — applies A-012 boundary; requires Chronicle `XpCostResolution`.
6. **Apply shared effects** — Werewolf computes via `WerewolfPackTotemLinkBoundaryPayload` + `WerewolfSharedTotemEffectsBoundaryPayload`; Chronicle applies to members.
7. **Record Banir-Totem Gift effect** — temporary, scene-scoped (Line 2507).
8. **Persist/load state** — round-trip via repository.

### 6.3 Banir-Totem (Gift 2507) — special case

The source rule is: "os alvos perdem Características do totem, táticas de matilha e cooperação pelo resto da cena." This is a **temporary, scene-scoped** state. It is NOT a permanent aggregate mutation. It belongs in scene/temporary state, not the persistent aggregate.

---

## 7. Runtime Contract

### 7.1 Does Chronicle already have an aggregate pattern?

**No.** The `Chronicle.Domain` project contains only `AssemblyMarker.cs`. There are no `IAggregateRoot`, `IRepository<T>`, `IDocument`, `IDocumentStore`, `ICommand`, `IQuery`, or `IDomainEvent` interfaces.

### 7.2 Does Chronicle already have repository/persistence interfaces?

**No.** There is no repository pattern anywhere. The only persistence-adjacent code is `Chronicle.Persistence.Sqlite/AssemblyMarker.cs`.

### 7.3 Does Chronicle already have IDocument-style persistence?

**No.**

### 7.4 Where should Pack/Totem state live?

The state must live in **Chronicle** (not Werewolf). Specifically:

- A new **Chronicle.Domain** aggregate root (`PackTotemAggregate` or similar) — but this requires first introducing the `IAggregateRoot` pattern.
- A new **Chronicle.Persistence.Sqlite** repository (`IPackTotemRepository`) — but this requires first introducing `IRepository<T>` and `IDocumentStore`.

**This is the largest pre-implementation blocker.** A "Pack/Totem aggregate runtime" wave cannot land without first creating the minimum Chronicle Domain pattern.

### 7.5 How does Werewolf request a mutation?

Existing pattern: `RuleSetOperationRequest` (in `Chronicle.RuleSets.Abstractions.Runtime`) with operation key + `Dictionary<string,string>` inputs → `RuleSetOperationResult` with `Dictionary<string,string>` outputs.

For Pack/Totem:
1. Werewolf `Execute*` method receives inputs.
2. Validates canonical constraints (TotemId is in `WerewolfTotemIdentifiers.Supported`; PackId format).
3. Returns a `*BoundaryPayload` record serialized to JSON in the `boundary` output key.
4. **Does NOT mutate any state.**

Chronicle application orchestrator then:
1. Reads the `boundary` JSON.
2. Invokes a `ICommand` (e.g., `BindTotemCommand`) on the aggregate.
3. The aggregate emits `IDomainEvent`s.
4. A repository persists the aggregate.

### 7.6 How does Chronicle persist the result?

The existing `ResourceTransitionOrchestrator` pattern (plain class taking a `RuleSetRuntimeRegistry`) is the closest analog. For Pack/Totem we need a new `PackTotemOrchestrator` that:
- Takes a `RuleSetRuntimeRegistry`.
- Receives a request (e.g., `BindTotemRequest`).
- Calls `WerewolfReferenceRuntime.Execute(...)` with `rite-runtime.execute-rite` + `rite.mystic.totem` inputs.
- Receives the `WerewolfTotemBindingBoundaryPayload`.
- Invokes `IPackTotemRepository.SaveAsync(...)` after invoking the command.

### 7.7 How does the Rule Set receive current state?

Currently, **it does not.** The Werewolf runtime is **stateless** — it validates and returns. The rule set does not need to read Pack/Totem state to execute Pack/Totem operations; it only validates identifiers.

If the rule set needs to read Pack/Totem state (e.g., to validate a Totem-binding request against current Pack state), Chronicle must inject state into the request `Inputs` dictionary before calling `Execute`. The pattern is: Chronicle owns state, passes relevant state into Werewolf via `Inputs`, Werewolf validates and returns a boundary.

### 7.8 Which S5 boundaries can be reused?

| Existing boundary | Reuse for |
|---|---|
| `WerewolfTotemBindingBoundaryPayload` (S4) | Transient Rite success → Chronicle receives the request to bind |
| `WerewolfPackTotemLinkBoundaryPayload` (S5) | Persistent linkage request |
| `WerewolfSharedTotemEffectsBoundaryPayload` (S5) | Shared effects application request |
| `WerewolfContritionBoundaryPayload` (S4) | Dogma violation response |

**No new boundaries are required for the minimum slice.** The existing 4 payloads are sufficient.

---

## 8. Totem Binding Bridge

### 8.1 Current S4 contract

`WerewolfTotemBindingBoundaryPayload`:
- Fields: `RiteKey`, `TotemId`, `PackId`, `MemberRoster`, `TotemAggregation`, `SourceLocator`, `Note`.

### 8.2 What the source requires

- **Source:** Line 2693-2695: "Ritual de Totem: Vincula um espírito totêmico a um grupo de Garou para formar uma matilha através de caça espiritual na Umbra. Exige o Antecedente Totem e o teste-padrão."
- **Source:** Line 1632: shared Antecedente; sum of investments.

### 8.3 Current behavior

`WerewolfRiteExecutionService` emits `WerewolfTotemBindingBoundaryPayload` for the `rite.mystic.totem` Rite, with placeholder strings "PackId from Chronicle", "TotemId from Chronicle".

### 8.4 Proposed bridge (NOT IMPLEMENTED)

1. Player triggers `rite.mystic.totem` via `rite-runtime.execute-rite`.
2. Werewolf validates Rite key, runs standard test (Line 2662: Raciocínio + Rituais difficulty 7).
3. On success, Werewolf returns `WerewolfTotemBindingBoundaryPayload` with:
   - `RiteKey` = `rite.mystic.totem`
   - `TotemId` = from request input
   - `PackId` = from request input
   - `MemberRoster` = from request input
   - `TotemAggregation` = sum of member investments
4. Chronicle application orchestrator receives the boundary, creates a `BindTotemCommand`, invokes the aggregate.
5. Aggregate transitions `LinkState` to `Bound` and persists.

### 8.5 No additional source data is required

The existing S4 boundary already carries the data Chronicle needs.

---

## 9. Shared Totem Effects Model

### 9.1 Current S5 contract

`WerewolfSharedTotemEffectsBoundaryPayload`:
- Fields: `TotemId`, `EffectKeys`, `IntendedRecipients`, `ApplicationScope`, `ChronicleOrchestrationRequired`, `SourceLocator`, `Note`.

### 9.2 What the source requires

- **Source:** Line 3733: "Concedem características temporárias ou permanentes... que podem ser alternados entre os membros da matilha a cada turno."
- **Source:** Line 1636: "Por padrão, os benefícios do totem estão disponíveis para apenas um membro da matilha por turno (passado ao final do turno)."
- **Source:** Line 1646: "+1 membro adicional por 4 pontos de Totem" (improvement).

### 9.3 Three layers

1. **Source-defined static benefit** → `WerewolfTotemCatalog.AllDefinitions[totemId].Effects` (Werewolf).
2. **Current relationship state** → Chronicle aggregate (`TotemId`, `TotemRating`, `TotemImprovementPurchases`).
3. **Calculated shared effect** → Werewolf computes via `WerewolfSharedTotemEffectsBoundaryPayload`; Chronicle applies to member(s).
4. **Narrative interpretation** → Narrative Intelligence.

### 9.4 Deterministic calculation

`WerewolfTotemDefinitions.CalculateBeneficiaryCount(totalTotemPoints)`:
- `DefaultBeneficiaryCount = 1`
- `AdditionalBeneficiaryCost = 4`
- Returns `1 + (totalTotemPoints - 1) / 4`

This helper already exists and is **reusable** in Chronicle to determine how many members can share effects simultaneously.

### 9.5 No duplicate Totem catalog

The current Werewolf catalog is the **single source of truth** for Totem effects. Chronicle does not duplicate; it reads identifiers + delegates to Werewolf.

---

## 10. All Pack/Totem Source Gaps

| Gap | Source | Disposition | Affects MC? |
|---|---|---|---|
| Pack Renome (collective) | Line 2853 only (per-character) | NARRATIVE_BOUNDARY | No |
| Pack XP pool | Source silent | SOURCE_UNSPECIFIED | No |
| Pack gifts (collective) | Line 3733 only (per-character) | NARRATIVE_BOUNDARY | No |
| Pack succession (post-dissolution) | Source silent | SOURCE_UNSPECIFIED | No |
| Pack territory/base state | Line 1001 only | NARRATIVE_BOUNDARY | No |
| Pack allies/enemies state | Line 1005 only | NARRATIVE_BOUNDARY | No |
| Totem approval/rejection (Pack level) | Line 575 only (tribal) | NARRATIVE_BOUNDARY | No |
| Totem XP cost (2 vs 3) | Line 1633 vs 2820 | EXPLICIT_CONTRADICTION (preserved) | No (boundary) |
| Banir-Totem Gift duration | Line 2507: "pelo resto da cena" | RESOLVED_BY_SOURCE | No (scene-scoped) |
| Totem effects distribution priority | Line 1636: "passado ao final do turno" | RESOLVED_BY_SOURCE | No (Chronicle chooses active recipient) |
| Pack dissolution ceremony | Line 199: "cerimônia formal" | NARRATIVE_BOUNDARY | No |
| Dogma violation → isolation mechanics | Line 3734: "isolamento" | SOURCE_UNSPECIFIED | No |
| Pack leadership style | Line 1003 only | NARRATIVE_BOUNDARY | No |

**No Pack/Totem gap blocks MECHANICALLY_COMPLETE.**

---

## 11. True MECHANICALLY_COMPLETE Gates (after Pack/Totem)

A truthful `MECHANICALLY_COMPLETE` requires:

1. **G1: Pack/Totem aggregate runtime exists** (this wave).
2. **G2: Renown/Rank state machine** (XP accumulation, challenge, rank transitions — Lines 2848-2850, 2798-2803).
3. **G3: Fetish/Talen domain** (creation, persistence, inventory — Lines 3466-3471, 3881-3930).
4. **G4: All 32 Rites catalogued + executable/typed-boundary** (currently 12/32).
5. **G5: All Gifts implemented by dependency order** (~125 remaining, 40-50 blocked by G1-G3).

Pack/Totem is **G1** and unblocks:
- RITE-WAVE-E (7 Pack/Sept/Totem Rites: Hunting Stone is already done; Totem Rite is S4 boundary; Caern Opening/Creation are S4 boundaries — remaining 7 include Banish, Gathering for the Rite, Questing Stone, Rites of Accord, Satire Rite, and the Pack-specific minor rites).
- ~40-50 Pack/Totem-linked Gifts (per Outlook).

After G1, the **remaining** gates are G2 (Renown/Rank) and G3 (Fetish/Talen). These are NOT in this audit.

**Equating "Pack/Totem aggregate exists" with `MECHANICALLY_COMPLETE` is incorrect.** It is one of three gates.

---

## 12. Test Strategy (no tests written in this audit)

### 12.1 Minimum test categories

1. **Pack aggregate invariants**
   - Empty Pack cannot have a bound Totem.
   - Pack must have at least 2 members (per source).
   - Pack cannot exceed 10 members (per source).
   - Pack can have at most 1 active Totem.
   - Dissolved Pack has no active Totem.

2. **Member lifecycle**
   - Add member increases roster.
   - Remove member updates roster.
   - Removing last member invalidates Pack.
   - Removing a member with active Totem relationship triggers re-aggregation.

3. **Totem binding**
   - Bind Totem requires PackId + TotemId.
   - Bind Totem requires valid TotemId (in `WerewolfTotemIdentifiers.Supported`).
   - Re-binding to a different Totem requires prior dissolution.
   - Re-binding to the same Totem is idempotent.

4. **Duplicate binding prevention**
   - Two Packs cannot share the same Totem at the same time? **(Source silent — source line 199 says "toda matilha vincula-se a um Espírito Totêmico" but doesn't explicitly forbid a Totem serving multiple Packs. Mark as NARRATIVE_BOUNDARY for now.)**

5. **Totem relationship state**
   - `Unbound` → `Bound` requires valid binding request.
   - `Bound` → `Dissolving` requires dissolution event.
   - `Dissolving` → terminal.

6. **Deterministic shared effects**
   - `CalculateBeneficiaryCount(8)` returns `1 + (8-1)/4 = 2` (matches `WerewolfTotemDefinitions`).
   - For TotemRating < 4 → only 1 beneficiary.
   - For TotemRating = 8 → 2 beneficiaries.

7. **Persistence round trip**
   - Save → Load → equal state.
   - Update → Save → Load → equal updated state.

8. **A-012 contradiction preservation**
   - Runtime returns `TotemExperienceCostUnresolved` for XP advancement when no `XpCostResolution` provided.
   - Both 2 and 3 values remain in `A012Conflict`.

9. **Chronicle ownership**
   - Werewolf runtime does not mutate state.
   - Chronicle application orchestrator does mutate state.

10. **No world mutation inside Werewolf**
    - `S5NoPackAggregateIntroduced` continues to pass.

11. **No second Totem catalog**
    - Only `WerewolfTotemCatalog` exists.

12. **No second Rite execution runtime**
    - Only `WerewolfRiteExecutionService` + `WerewolfReferenceRuntime.Execute*`.

13. **No new RNG**
    - All Pack/Totem deterministic calculations are pure functions.

14. **Runtime registration**
    - `spirit-umbra.pack-totem-link` and `spirit-umbra.shared-totem-effects` remain registered.

15. **Capability ownership**
    - `spirit-umbra` capability owns the operations; no new capability.

---

## 13. Implementation Plan (WAVE E0-E4)

### 13.1 WAVE E0 — Domain/Contract Foundation

**Goal:** Introduce the minimum Chronicle aggregate pattern that Pack/Totem can use. Without this, the wave cannot land.

**Files expected to change:**
- `src/Chronicle.Domain/AssemblyMarker.cs` (extend with `IPackTotemAggregate` interface)
- `src/Chronicle.Domain/PackTotem/PackTotemAggregate.cs` (NEW — concrete aggregate)
- `src/Chronicle.Domain/PackTotem/PackTotemState.cs` (NEW — state record)
- `src/Chronicle.Domain/PackTotem/PackTotemCommands.cs` (NEW — `CreatePackCommand`, `AddMemberCommand`, `RemoveMemberCommand`, `BindTotemCommand`, `DissolvePackCommand`, `AdvanceTotemCommand`)
- `src/Chronicle.Domain/PackTotem/PackTotemEvents.cs` (NEW — `PackCreated`, `MemberAdded`, `TotemBound`, `PackDissolved`, `TotemAdvanced`)
- `src/Chronicle.Domain/IAggregateRoot.cs` (NEW — base interface)
- `src/Chronicle.Domain/IDomainEvent.cs` (NEW — base interface)

**Responsibility:** Define the aggregate contract.
**Dependencies:** None (first wave).
**Source authority:** Source-faithful field list (per §6.1).
**Expected tests:** Domain unit tests for invariants and command handlers.
**Risks:** Setting an aggregate pattern that other future aggregates (Character, Sept, Caern) must also use. Risk of premature abstraction.

### 13.2 WAVE E1 — Persistence/Application Orchestration

**Goal:** Add the persistence layer for Pack/Totem and the application orchestrators.

**Files expected to change:**
- `src/Chronicle.Application/PackTotem/PackTotemOrchestrator.cs` (NEW)
- `src/Chronicle.Application/PackTotem/PackTotemRequestTypes.cs` (NEW)
- `src/Chronicle.Persistence.Sqlite/AssemblyMarker.cs` (extend)
- `src/Chronicle.Persistence.Sqlite/PackTotem/SqlitePackTotemRepository.cs` (NEW)
- `src/Chronicle.Persistence.Sqlite/PackTotem/PackTotemDocument.cs` (NEW)
- `src/Chronicle.Persistence.Sqlite/IDocumentStore.cs` (NEW)
- `src/Chronicle.Persistence.Sqlite/IDocumentRepository.cs` (NEW)
- `src/Chronicle.Persistence.Sqlite/IDocument.cs` (NEW)
- `src/Chronicle.Persistence.Sqlite/IRepository.cs` (NEW)

**Responsibility:** Persist and orchestrate Pack/Totem.
**Dependencies:** WAVE E0 (aggregate pattern).
**Source authority:** Pack/Totem field list.
**Expected tests:** Repository round-trip tests; orchestrator behavior tests.
**Risks:** Introducing a persistence pattern that doesn't generalize to other aggregates (e.g., Character, Sept).

### 13.3 WAVE E2 — Werewolf Integration

**Goal:** Add A-012 boundary resolution parameter to Werewolf progression runtime; ensure existing S4/S5 boundaries remain unchanged.

**Files expected to change:**
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfProgressionContracts.cs` (extend `WerewolfProgressionRequest` with optional `XpCostResolution`)
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfAdvancementService.cs` (read `XpCostResolution`)
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfCharacterCreationDraftContracts.cs` (add `PackId?` to `WerewolfInitializedCharacterState`)
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfCharacterCreationDraftInitializer.cs` (pass through `PackId?`)
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRuntimeCharacterState.cs` (add `PackId?`)

**Responsibility:** Allow Chronicle to supply a Totem XP cost resolution.
**Dependencies:** WAVE E0, WAVE E1.
**Source authority:** A-012 lines 1633/2820 (preserved contradiction).
**Expected tests:** `AdvanceTotemHonorsExplicitXpResolution`, `AdvanceTotemReturnsFailureWithoutResolution`, `CharacterDraftIncludesPackIdWhenProvided`.
**Risks:** None significant — purely additive.

### 13.4 WAVE E3 — Tests/Evidence

**Goal:** Add comprehensive test coverage and evidence document.

**Files expected to change:**
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfPackMaterializationTests.cs` (extend with new aggregate behavior)
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfProgressionTests.cs` (add A-012 resolution tests)
- `tests/Chronicle.Domain.Tests/PackTotem/PackTotemAggregateTests.cs` (NEW)
- `tests/Chronicle.Application.Tests/PackTotem/PackTotemOrchestratorTests.cs` (NEW)
- `tests/Chronicle.Persistence.Sqlite.Tests/PackTotem/SqlitePackTotemRepositoryTests.cs` (NEW)
- `docs/reviews/werewolf-rule-set-completeness/completion-evidence/RULESET-COMPLETION-PACK-TOTEM-AGGREGATE.md` (NEW)

**Responsibility:** Test all 15 categories in §12.
**Dependencies:** WAVE E0, E1, E2.
**Source authority:** All Pack/Totem source locators cited in §2-3.
**Expected tests:** 30-50 new tests across the 4 test projects.
**Risks:** Test flakiness if persistence test infrastructure is unstable.

### 13.5 WAVE E4 — First Dependent Rite/Gift Integration

**Goal:** Demonstrate that the new aggregate unblocks a dependent Rite or Gift.

**Files expected to change:**
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRiteCatalog.cs` (add Hunting Stone as `ExecutableMechanic`; the Pack-binding Rites are already typed-boundary)
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfGiftCatalog.cs` (add Banir o Totem as `ExecutableMechanic` — Line 2505-2507)
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRiteExecutionService.cs` (use Pack aggregate for Hunting Stone)
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfGiftRuntimeService.cs` (use Pack aggregate for Banir Totem)
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfRiteExecutionTests.cs` (extend)
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfGiftRuntimeTests.cs` (extend)

**Responsibility:** Show end-to-end Pack/Totem integration.
**Dependencies:** WAVE E0, E1, E2, E3.
**Source authority:** Hunting Stone (Line 2691), Banir o Totem (Line 2505-2507).
**Expected tests:** `HuntingStoneReadsPackTactics`, `BanirTotemIsSceneScoped`.
**Risks:** Confirming that no new "reasonable defaults" sneak in.

---

## 14. Files Expected to Change (cumulative)

### New files (proposed, NOT implemented)

```
src/Chronicle.Domain/
  IAggregateRoot.cs
  IDomainEvent.cs
  PackTotem/PackTotemAggregate.cs
  PackTotem/PackTotemState.cs
  PackTotem/PackTotemCommands.cs
  PackTotem/PackTotemEvents.cs

src/Chronicle.Application/
  PackTotem/PackTotemOrchestrator.cs
  PackTotem/PackTotemRequestTypes.cs

src/Chronicle.Persistence.Sqlite/
  IDocument.cs
  IRepository.cs
  IDocumentStore.cs
  IDocumentRepository.cs
  PackTotem/PackTotemDocument.cs
  PackTotem/SqlitePackTotemRepository.cs

rule-sets/Chronicle.RuleSets.Werewolf/
  (no new files; modifications only)

rule-sets/Chronicle.RuleSets.Werewolf.Tests/
  (no new files; modifications only)

tests/Chronicle.Domain.Tests/
  PackTotem/PackTotemAggregateTests.cs

tests/Chronicle.Application.Tests/
  PackTotem/PackTotemOrchestratorTests.cs

tests/Chronicle.Persistence.Sqlite.Tests/
  PackTotem/SqlitePackTotemRepositoryTests.cs

docs/reviews/werewolf-rule-set-completeness/completion-evidence/
  RULESET-COMPLETION-PACK-TOTEM-AGGREGATE.md
```

### Modified files (proposed, NOT implemented)

```
src/Chronicle.Domain/AssemblyMarker.cs
src/Chronicle.Persistence.Sqlite/AssemblyMarker.cs
src/Chronicle.RuleSets.Abstractions/PackageSources/RuleSetPackageSourceValidation.cs (allow-list — no Werewolf changes; this is for new Chronicle files)

rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfProgressionContracts.cs
rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfAdvancementService.cs
rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfCharacterCreationDraftContracts.cs
rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfCharacterCreationDraftInitializer.cs
rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfRuntimeCharacterState.cs

rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfPackMaterializationTests.cs
rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfProgressionTests.cs
rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfRiteExecutionTests.cs
rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfGiftRuntimeTests.cs
```

---

## 15. Tests to Add (proposed, NOT implemented)

| Test | Project | Category |
|---|---|---|
| `EmptyPackCannotBindTotem` | Chronicle.Domain.Tests | Invariant |
| `PackRequiresAtLeast2Members` | Chronicle.Domain.Tests | Invariant |
| `PackCannotExceed10Members` | Chronicle.Domain.Tests | Invariant |
| `PackCanHaveAtMostOneActiveTotem` | Chronicle.Domain.Tests | Invariant |
| `AddMemberUpdatesRoster` | Chronicle.Domain.Tests | Member lifecycle |
| `RemoveMemberUpdatesRoster` | Chronicle.Domain.Tests | Member lifecycle |
| `RemoveLastMemberInvalidatesPack` | Chronicle.Domain.Tests | Member lifecycle |
| `BindTotemRequiresValidTotemId` | Chronicle.Domain.Tests | Totem binding |
| `BindTotemRequiresPackAndTotem` | Chronicle.Domain.Tests | Totem binding |
| `RebindTotemRequiresDissolution` | Chronicle.Domain.Tests | Totem binding |
| `DissolvedPackHasNoActiveTotem` | Chronicle.Domain.Tests | Totem state |
| `UnboundToBoundRequiresBindingRequest` | Chronicle.Domain.Tests | Totem state |
| `BoundToDissolvingRequiresDissolutionEvent` | Chronicle.Domain.Tests | Totem state |
| `CalculateBeneficiaryCountForRating8` | Chronicle.Domain.Tests | Deterministic |
| `CalculateBeneficiaryCountForRating3` | Chronicle.Domain.Tests | Deterministic |
| `SaveLoadRoundTrip` | Chronicle.Persistence.Sqlite.Tests | Persistence |
| `UpdateSaveLoadRoundTrip` | Chronicle.Persistence.Sqlite.Tests | Persistence |
| `OrchestratorBindsTotemViaRuleSet` | Chronicle.Application.Tests | Orchestration |
| `OrchestratorRejectsInvalidTotemId` | Chronicle.Application.Tests | Orchestration |
| `AdvanceTotemHonorsExplicitXpResolution` | Werewolf.Tests | A-012 |
| `AdvanceTotemReturnsFailureWithoutResolution` | Werewolf.Tests | A-012 |
| `A012ConflictPreservesBothValues` | Werewolf.Tests | A-012 |
| `CharacterDraftIncludesPackIdWhenProvided` | Werewolf.Tests | State |
| `S5NoPackAggregateIntroduced` (existing, must pass) | Werewolf.Tests | Ownership |
| `HuntingStoneReadsPackTactics` (WAVE E4) | Werewolf.Tests | Integration |
| `BanirTotemIsSceneScoped` (WAVE E4) | Werewolf.Tests | Integration |

---

## 16. Risks

1. **Architectural lock-in (HIGH):** Pack/Totem is the first Chronicle aggregate. Whatever pattern is chosen becomes the template for Character, Sept, Caern, and possibly 5-10 other aggregates. A wrong choice cascades.

2. **A-012 handling (MEDIUM):** Choosing an interface that doesn't honor the contradiction (e.g., defaulting to 2 or 3) would violate the audit's "do not pick" rule.

3. **Werewolf/Choreography boundary (MEDIUM):** The current runtime is stateless. Adding read access (so Werewolf can validate against current Pack state) requires either (a) Chronicle injecting state into `Inputs` or (b) Werewolf exposing a state-query operation. Both add complexity.

4. **Source line 199 — "toda matilha vincula-se a um Espírito Totêmico":** This could be read as "every pack binds to exactly one Totem" or "every pack that has a Totem binds to exactly one." If the former, duplicate-binding prevention is deterministic; if the latter, it's narrative. Source is ambiguous; default to the stricter reading but document.

5. **The ~3-week effort to introduce the aggregate pattern is not "small."** WAVE E0 is the largest pre-implementation work and is largely out of scope for "Pack/Totem aggregate runtime" — it is a general Chronicle infrastructure prerequisite.

6. **Werewolf Totem test is currently blocked by AppLocker in this environment** (0x800711C7). The 1619 baseline + 13 new S5 + 1 new A-012 tests must be re-run on a system without AppLocker restriction.

---

## 17. Exact Proposed Outlook Changes

Do NOT modify Outlook in this audit pass. Proposed exact text for the next
implementation wave:

### 17.1 New section in Outlook: Phase 4 (Pack/Totem Aggregate)

```
**Phase 4: Pack/Totem Aggregate (WAVE E0-E4)**

Sub-phases:
- E0 — Domain/Contract foundation: introduce IAggregateRoot, IDomainEvent,
  IPackTotemAggregate in Chronicle.Domain.
- E1 — Persistence/Application: SQLite repository, orchestrator.
- E2 — Werewolf integration: A-012 explicit-resolution parameter, PackId on
  character draft.
- E3 — Tests/evidence: 25-30 new tests across 4 test projects; new evidence
  document.
- E4 — First integration: Hunting Stone (Rite) and Banir o Totem (Gift) use
  the new aggregate.

This unblocks 7 Rites (RITE-WAVE-E) and an estimated 40-50 Gifts.

Source authority: Lines 191-199 (Pack), 1631-1647 (Totem Antecedente),
3730-3832 (Totem list), 3182-3191 (Pack tactics), 2505-2507 (Banir o Totem),
1000-1005 (Pack creation pillars), 2853 (collective renome narrative).

Remaining MECHANICALLY_COMPLETE gates after Pack/Totem:
- Renown/Rank state machine (G2)
- Fetish/Talen domain (G3)
- All 32 Rites catalogued (G4)
- ~80 remaining Gifts (G5)
```

### 17.2 Update Section "1. PLAYABLE_CORE_COMPLETE" — `What remains`

Change from:
> "RITE-WAVE-E (Pack/Sept/Totem): 7 — blocked by S5 Pack/Totem aggregate"

To:
> "RITE-WAVE-E (Pack/Sept/Totem): 7 — blocked by WAVE E0-E4 (Chronicle aggregate pattern prerequisite)"

### 17.3 Update "MECHANICALLY_COMPLETE" section

Add:
> "**Phase 4 (WAVE E0-E4) is the next gate** after this audit. It introduces the minimum Chronicle aggregate pattern that Pack/Totem — and all future aggregates — require."

---

## 18. Audit Confirmation

- **Production code modified:** No. This is an audit-only pass.
- **Tests modified:** No.
- **Completeness matrix modified:** No.
- **Outlook modified:** No (proposed changes documented in §17).
- **No commit, no push, no branch created.**

## 19. Git Hygiene

Run after audit document write:
- `git diff --check` (expect clean — no source/test diff)
- `git status --short` (expect 1 untracked: this audit file)
- `git diff --stat` (expect no entries)

## 20. Readiness

This audit is ready to become the implementation specification for WAVE E0.
The first implementation slice (WAVE E0) is small and well-bounded, but
requires the broader Chronicle infrastructure prerequisite (aggregate
pattern). This prerequisite should be acknowledged and scheduled.

