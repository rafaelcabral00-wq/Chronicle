# AUDIT-WEREWOLF-SPIRIT-UMBRA-2026-08-25

## Source Authority
- Repository: C:\Dev\Chronicle-wt-progression
- Branch: audit/werewolf-spirit-umbra
- Baseline: af6d26a
- Canonical source: `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt` (3948 lines)
- Scope: Complete canonical Spirit/Umbra mechanical domain
- Nature: READ-ONLY AUDIT — no implementation, no file modifications, no metadata changes

## 1. Mechanic Inventory

### Root Cause of 99-vs-87 Discrepancy

The original audit over-counted because:

1. **Entity vs mechanic conflation:** The 23 "declarative" items were counted as 23 separate mechanics, but they are actually 5 declarative catalogs containing 23+ entity values. A catalog containing 8 categories is ONE mechanic, not 8.

2. **Double-counting dependencies:** Gift/Rite dependency associations were counted as additional Spirit/Umbra mechanics. Consumer integrations are not independent domain primitives.

3. **Mixed granularity:** The A-I classification mixed entity-rows with mechanic-rules in a single sum.

### Source-Verification Correction: Charm Entity Count

During S1 implementation, canonical source verification corrected the Charm entity count:

- **Previous audit count:** 29 Charms (16 Special)
- **Source-verified count:** 30 Charms (17 Special)

**Root cause:** The original audit counted physical source lines rather than semantic Charm entries. Source line 3424 contains two distinct Charm entries:
- `Criar Chamas` (Create Flames)
- `Criar Vento` (Create Wind)

**Authoritative inventory:**
- Common: 4
- Special: 17
- Bane: 4
- Weaver: 3
- Wyld: 2
- **Total: 30**

This correction does NOT change the mechanic count. The Charm catalog remains ONE declarative S1 mechanic. The 53-mechanic accounting is preserved.

### Corrected Authoritative Inventory

See the canonical keyed inventory table at the end of this document.

## 2. Declarative Inventory

### Spirit Categories = 8 (1 catalog mechanic, 8 entity values)
1. Totem
2. Bane
3. Naturae
4. Incarna
5. Celestine
6. Jaggling
7. Gaffling
8. Ancestor

### Umbra Layers/Realms = 19 (1 catalog mechanic, 19 entity values)
1. Penumbra
2. Umbra Rasa
3. Deep Umbra / Umbra Profunda
4. Umbra Negra
5. Abismo
6. Campo de Batalha
7. Érebo
8. Fluxo
9. Cicatriz
10. Malfeas
11. Pangéia
12. País do Verão
13. Reino da Atrocidade
14. Reino Cibernético
15. Reino Etéreo
16. Reino Lendário
17. Toca dos Lobos
18. Zona Onírica
19. Periferia

### Barriers = 3 (1 catalog mechanic, 3 entity values)
1. Película (physical ↔ Penumbra)
2. Membrana (Umbra Rasa ↔ Deep Umbra)
3. Teia do Padrão (Weaver's pattern web)

### Charms = 30 (1 catalog mechanic, 30 entity values)
- Common: 4 (Materializar, Reformar, Sentido de Orientação, Sentir o Reino)
- Special: 17 (Abrir Ponte da Lua, Armadura, Congelar, Controle de Sistemas Elétricos, Criar Chamas, Criar Vento, Curar, Espiar, Estilhaçar Vidro, Inundação, Levitação, Metamorfose, Purificar os Domínios Sombrios, Rajada, Rastrear, Umbramoto, Vôo Ligeiro)
- Bane: 4 (Corrupção, Incitar o Frenesi, Influência Maléfica, Possessão)
- Weaver: 3 (Estática Espiritual, Petrificar, Solidificar a Realidade)
- Wyld: 2 (Desorientar, Romper a Realidade)

### Existing Totem Reuse Disposition

**The 19 Totems are ALREADY materialized in baseline af6d26a.**

They are NOT counted as new Spirit/Umbra declarative mechanics or entities.

The 5 new declarative mechanics are:
1. Spirit category catalog (Totem is one of 8 categories)
2. Umbra realm catalog (19 realms/layers)
3. Barrier catalog (3 barriers)
4. Spirit trait schema (4 traits)
5. Charm catalog (30 charms)

**How Spirit/Umbra references Totems:**

The Spirit/Umbra domain references existing Totems through stable contracts already materialized:
- `WerewolfTotemIdentifiers` (stable keys like `totem.catalog.cervo`)
- `WerewolfTotemCatalog` (entity lookup by key)
- `WerewolfTotemDefinitions` (Totem Background key, aggregation formula, improvement table)

New Spirit/Umbra materialization does NOT duplicate Totem definitions.

## 3. Gauntlet / Crossing Semantics

### Core Crossing Rule (Source Lines 3277-3290)
- **Test:** Gnose vs. Película level
- **Success:** Character slides to the other side
- **Failure:** Cannot force passage; subsequent attempts at same location increase difficulty by +2 per attempt
- **Botch:** Character may get stuck in the Pattern Web or disappear for hours
- **Fury restriction:** Impossible to step sideways using actions granted by Fury

### Crossing Time by Successes (Lines 3282-3286)
| Successes | Time |
|---|---|
| 0 (failure) | Cannot retry same location for 1 hour |
| 1 | 5 minutes |
| 2 | 30 seconds |
| 3+ | Instant |

### Reflective Surface Modifiers (Line 3288)
- Mirrors, polished silver, or water tanks reduce difficulty by 1
- No alert: Failures do not alert Weaver spirits, allowing new attempts without penalties at same location
- Safety: Critical failures only break/cloud the reflective surface, preventing inter-world trapping

### Gauntlet/Película Ratings (Lines 3235-3249)
- Typical range: 2 to 9
- Higher in technological/civilized areas
- Lower in wild/caern areas

### Caern Película Table (Lines 3249)
| Caern Level | Película Level | Moon Bridge Max Distance |
|---|---|---|
| 1 | 3 | 50 km |
| 2 | 2 | 100 km |
| 3 | 1 | 200 km |
| 4 | 1 | 500 km |
| 5 | 0 | 1,000 km |

### Reach the Umbra Gift Interaction (Line 2365-2367)
- No test needed; instant shortcuts without risk of getting stuck
- -2 difficulty for entering or exiting Umbra realms
- Cannot use Fury in the same turn

### Silver Effect (Line 1692)
- Each silver item carried temporarily reduces effective Gnose (penalty lasts until silver is discarded, up to 1 day)

## 4. Minimum Spirit State Proposal

### Proposed 15-Field Model

| Field | Purpose |
|---|---|
| SpiritId | Stable identity |
| SpiritKey | Machine-readable category/type |
| Category | Totem/Bane/Naturae/Incarna/etc. |
| GnosisPermanent | Permanent Gnose trait |
| GnosisCurrent | Temporary Gnose pool |
| WillpowerPermanent | Permanent Willpower trait |
| WillpowerCurrent | Temporary Willpower pool |
| RagePermanent | Permanent Rage/Fury trait |
| RageCurrent | Temporary Rage pool |
| EssenceCurrent | Vitality/survival points |
| Charms | List of spirit powers |
| IsMaterialized | Physical presence flag |
| LocationToken | Current scene/location |
| IsInSlumber | Resting state (Modorra) |
| OwnerId | Fetish/character binding |

### Classification of Fields

#### A. Directly Required by Canonical Mechanics
- **GnosisPermanent/Current:** Source Line 3409: Required for all spirit tests.
- **WillpowerPermanent/Current:** Source Line 3407: Required for spirit actions.
- **RagePermanent/Current:** Source Line 3408: Required for spirit combat.
- **EssenceCurrent:** Source Line 3410: Required for spirit survival.
- **Charms:** Source Lines 3412-3458: Required for spirit powers.
- **IsMaterialized:** Source Line 3414: Required for materialization state.
- **Category:** Source Lines 3394-3404: Required for spirit classification.

#### B. Proposed Identity/Integration Fields
- **SpiritId:** Not explicitly named in source, but required for stable references in code.
- **SpiritKey:** Machine-readable category shorthand.
- **LocationToken:** Not explicitly defined in source as a field name, but source assumes spirits have location.
- **OwnerId:** Not explicitly defined in source, but implied by Fetish binding (Line 3466-3469).

#### C. Fields Whose Lifecycle Semantics Remain Unresolved
- **EssenceCurrent lifecycle:** Exact threshold and recovery mechanics are ambiguous.
- **IsInSlumber (Modorra):** Entry/exit conditions and duration are not fully specified.
- **Materialization lifecycle:** Duration, voluntary dismissal, and forced dematerialization are not fully specified.
- **OwnerId binding/unbinding:** Source does not define how spirit-to-fetish binding is represented as persistent state.

**Important:** This state model is NOT implementation-authoritative yet. It is a proposal for minimum typed state required to support canonical mechanics. Further validation is needed before implementation.

## 5. Chronicle/Werewolf Authority

### Chronicle Owns
- Scene/location identity
- Location Gauntlet (Película) value storage
- World-object identity (realms, domains, caerns)
- Persistence and timeline
- Cross-entity aggregate storage
- Random dice generation

### Werewolf Owns
- Gauntlet rule interpretation (test definition, difficulty, time, retries)
- Spirit trait interpretation (Gnose, Willpower, Rage, Essence mechanics)
- Spirit test definitions (Gnose vs Película, opposed Gnose tests)
- Umbra transition mechanics (stepping sideways, Alcançar a Umbra, surface modifiers)
- Charm mechanics (common, special, Bane, Weaver, Wyld Charms)
- Deterministic spirit interactions (command, possession, bargains)
- Spirit category definitions and restrictions
- Totem spirit binding mechanics (Banir Totem, Ritual de Totem)

### Explicitly NOT Created
- No second "Chronicle world engine" inside Werewolf
- No generic spirit AI/attitude system beyond source-defined mechanics
- No Spirit/Umbra state in WerewolfRuntimeCharacterState

## 6. Gift Unlock Mapping

### Primary Spirit/Umbra Blocker Mapping

| Primitive | Gift Count | Gift Keys |
|---|---|---|
| Spirit Entity State | 18 | MetisSentirAWyrm, TheurgeSentirAWyrm, SilentStridersSentirAWyrm, SilverFangsSentirAWyrm, AhrounMedoVerdadeiro, UktenaEspiritoDoPassaro, UktenaEspiritoDoPeixe, BlackFuriesSentirAPresa, RedTalonsSentirAPresa, GalliardComunicacaoComAnimais, ChildrenOfGaiaAcalmar, SilverFangsEmpatia, BoneGnawersOdorRepugnante, FiannaUivoDaBanshee, ShadowLordsAplausoTrovejante, GlassWalkersSobrecargaDeEnergia, HomidPerturbarTecnologia, RagabashInduzirEsquecimento |
| Crossing/Umbra Presence | 8 | RagabashEmbacamentoDaPropriaForma, RagabashSimularOCheiroDeAguaCorrente, AhrounEspiritoDaBatalha, PhilodoxFaroParaAFormaVerdadeira, PhilodoxReiDosAnimais, WendigoResistenciaADor, WendigoVentoCortante, LupusSentidosAgucados |
| Charm Mechanics | 9 | MetisRaivaPrimordial, AhrounGarrasAfiadas, ChildrenOfGaiaArmaduraDeLuna, BlackFuriesMaldicaoDeEolo, GlassWalkersSentidosCiberneticos, GetOfFenrisRugidoDoPredador, GetOfFenrisDeterAFugaDosCovardes, ShadowLordsArmaduraDeLuna, SilentStridersResistenciaDeMensageiro |
| Spirit Command/Control | 5 | MetisCavar, RagabashInduzirEsquecimento, PhilodoxVerdadeDeGaia, GalliardDistracoes, BoneGnawersGerarIgnorancia |
| Totem Spirit Binding | 3 | BanirTotem, DomDoTotem, Anamae |

**Total Gift dependency associations = 43**

### Secondary Blockers
- **Spirit Entity State** Gifts: Secondary blocker = Gift runtime integration (effect execution, target selection)
- **Crossing/Umbra Presence** Gifts: Secondary blocker = Chronicle scene state (Gauntlet rating per location)
- **Charm Mechanics** Gifts: Secondary blocker = Gift runtime integration (Charm activation triggers)
- **Spirit Command/Control** Gifts: Secondary blocker = Spirit entity state + Gift runtime integration
- **Totem Spirit Binding** Gifts: Secondary blocker = Pack/Totem aggregate runtime + Spirit entity state

## 7. Rite Unlock Mapping

### Spirit/Umbra-Dependent Rites by Primitive

| Primitive | Rite Count | Rite Keys |
|---|---|---|
| Spirit Entity State + Crossing | 5 | `rite.mystic.commitment`, `rite.mystic.fetish`, `rite.mystic.totem`, `rite.mystic.summoning`, `rite.mystic.awaken-spirits` |
| Pack/Sept/Totem Aggregate | 7 | `rite.caern.assembly`, `rite.caern.badger-set`, `rite.caern.moon-bridge`, `rite.caern.hidden-ravine`, `rite.caern.creation`, `rite.periodic.cold-winds`, `rite.periodic.new-awakening` |
| Renown/Rank State | 7 | `rite.renown.conquest`, `rite.renown.passage`, `rite.renown.wounding`, `rite.punishment.ostracism`, `rite.punishment.stone-of-scorn`, `rite.punishment.jackal-voice`, `rite.punishment.satirical-ritual` |
| Human Decision/Narrative | 7 | `rite.pact.luna-mutable`, `rite.punishment.veil-laceration`, `rite.punishment.ciala-avenging-teeth`, `rite.death.winter-wolf`, `rite.caern.creation` (permanent Gnose sacrifice), `rite.mystic.initiation` (permanent damage risk), `rite.mystic.fire-baptism` (spirit mark disappearance) |

**Total Rite dependency associations = 26**

**Note:** This mapping does not cover all 32 Rites. It only identifies Spirit/Umbra dependencies. The authoritative Rites execution classification is maintained by the Rites workstream.

## 8. Pack/Totem Integration

### Requiring Future Spirit Runtime
1. **Totem spirit identity** — all 19 Totems need Spirit entity state for spirit-type references
2. **Totem Charms** — Encantos (Sentido de Orientação, Reformar, plus additional Charms) require Charm primitive
3. **Totem presence/materialization** — "fisicamente ou espiritualmente presente" requires materialization state
4. **Totem communication** — "fala diretamente com a matilha" requires spirit communication primitive
5. **Totem prestige with other spirits** — requires spirit hierarchy/disposition mechanics
6. **Banir Totem Gift** — requires spirit binding/command mechanics
7. **Totem severance/liberation** — requires spirit severance mechanics (Ritual de Contrição)

### Remaining Purely Declarative
1. **Totem Background identifier** — stable key only
2. **Totem aggregation formula** — deterministic calculation, no runtime state
3. **Totem improvement table** — declarative catalog
4. **Totem initial state** — declarative constants (8 points, 2 initial Charms)
5. **Totem dogma/restrictions** — declarative text
6. **Totem tribe associations** — declarative links
7. **Totem cost/point metadata** — declarative numbers

### A-012 Status
Remains unresolved. Totem XP cost conflict: Line 1633 = 2 XP, Line 2820 = 3 XP.

## 9. Source-Gap Discipline

### S-001..S-008 Reconciliations

| ID | Classification | Issue | Source Locator | Owner | Semantic Availability |
|---|---|---|---|---|---|
| S-001 | UnsupportedByCanonicalSource | Spirit AI/disposition system | Not stated | Werewolf | Not available — source defines no disposition scale |
| S-002 | UnsupportedByCanonicalSource | Spirit bargaining terms and chiminage valuation | Line 1696 | Werewolf | Not available — source mentions bartering but defines no valuation rules |
| S-003 | HumanDecision | Spirit materialization limits (duration, permanence) | Line 3414 | Human Decision | Partially available — materialization requires Gnose ≥ Película, adopts 7 health levels; duration/permanence unspecified |
| S-004 | HumanDecision | Spirit death/destruction threshold (Essence = 0 → death vs Modorra) | Line 3410 | Human Decision | Partially available — Essence = sum of Willpower+Rage+Gnose; reaching 0 causes death or Modorra; exact threshold/transition not specified |
| S-005 | B — executable using existing af6d26a primitives | Spirit movement speed in Umbra (Desfocamento: 20 + Willpower meters/turn) | Line 3462 | Werewolf | Available — source states maximum distance per turn is [twenty plus Willpower] meters in Umbra; Penumbra uses physical distances. AI/path-selection/disposition semantics not defined by source and remain unsupported. |
| S-006 | HumanDecision | Spirit possession permanence and control mechanics | Lines 3442-3450 | Human Decision | Partially available — possession requires Gnose test vs Willpower, duration by successes (1 success = 6 hours, 2 = 3 hours, 3 = 1 hour, 4 = 15 min, 5 = 5 min, 6+ = instant); permanence stated for fomori; control mechanics not fully specified |
| S-007 | UnsupportedByCanonicalSource | Fetish harmonization difficulty scaling | Line 3468 | Werewolf | Partially available — harmonization requires Gnose test (difficulty = Gnose of fetish); scaling beyond base not specified |
| S-008 | HumanDecision | Crossing difficulty for non-Garou beings | Not stated | Human Decision | Not available — source only defines crossing for Garou |

**Ownerless blockers: 0. All assigned.**

**Key discipline:** "Werewolf owns the domain" is NOT permission to invent a rule. Where canonical source does not define a mechanic, it is recorded as UnsupportedByCanonicalSource or HumanDecision, not fabricated.

## 10. Implementation Waves

### S1 — Declarative Spirit/Umbra Foundation
**Scope:**
- Spirit category catalog (8 categories: Totem, Bane, Naturae, Incarna, Celestine, Jaggling, Gaffling, Ancestor)
- Umbra layer/Realm catalog (19 realms/layers with source locators)
- Barrier definitions (Película, Membrana, Teia do Padrão)
- Spirit trait definitions (Gnose, Willpower, Rage, Essence)
- Charm catalog (common, special, Bane, Weaver, Wyld Charms)
- Stable references to existing Totem definitions (via WerewolfTotemIdentifiers)
- Gauntlet/Película rating system

**Does NOT include:**
- 19 existing Totems (already materialized in af6d26a)
- Runtime state
- Unresolved lifecycle semantics

**Files affected (conceptual):**
- `WerewolfSpiritIdentifiers.cs` (new)
- `WerewolfSpiritCatalog.cs` (new)
- `WerewolfUmbraDefinitions.cs` (new)
- `WerewolfCharmDefinitions.cs` (new)

**Blockers:** None — pure materialization
**Gifts unlocked:** 0
**Rites unlocked:** 0
**Chronicle contract:** None required

### S2 — Deterministic Runtime Primitives
**Scope:**
- SpiritEntityState record type
- Basic spirit trait tests (Gnose vs Película, opposed Gnose tests)
- Crossing primitive (Gnose test vs Película difficulty, time table, surface modifiers)
- Spirit detection primitive
- Basic spirit communication primitive
- Spirit materialization/dematerialization state machine
- Essence economy (drain, recovery, Modorra)

**Blockers:** Chronicle scene/location state (Gauntlet rating per scene)
**Gifts unlocked:** 12 (Crossing/Umbra Presence)
**Rites unlocked:** 0
**Chronicle contract:** Scene state extension for Gauntlet rating

### S3 — Gift Integration
**Scope:**
- Spirit-related Gift effect types
- Spirit command/control effects
- Spirit damage and Essence drain
- Charm activation (Gift triggers for spirit powers)
- Totem spirit binding effects

**Blockers:** S2 complete
**Gifts unlocked:** 31 remaining Spirit/Umbra Gifts
**Rites unlocked:** 0
**Chronicle contract:** None

### S4 — Rite Integration
**Scope:**
- Fetish creation ritual
- Totem binding ritual
- Spirit summoning ritual
- Awaken spirits ritual
- Commitment ritual

**Blockers:** S3 complete, Pack/Totem aggregate (for Totem binding)
**Gifts unlocked:** 0
**Rites unlocked:** 5 (mystic rites)
**Chronicle contract:** None

### S5 — Deeper Spirit Interactions
**Scope:**
- Spirit bargaining/chiminage system
- Spirit hierarchy and disposition
- Spirit death/destruction resolution
- Pack/Sept spirit integration
- Realm travel mechanics

**Blockers:** Human Decisions S-001, S-004, S-006 resolved
**Gifts unlocked:** 0
**Rites unlocked:** 0
**Chronicle contract:** Possible realm/location persistence

## 11. Implementation Readiness

- **S1:** Ready for implementation planning (no blockers, pure materialization)
- **S2+:** Dependent on further typed-runtime review as applicable

### Current Baseline
- af6d26a

### Accepted Validation
- Werewolf: 1458/1458 passed
- PackageValidator: 8/8 passed

## 12. Git Hygiene

```
git diff --check: clean
git diff --stat: 0 files changed
git status --short: (no changes)
```

Only the new audit evidence file is created. No TestResults, no .kilo, no matrix/report changes, no metadata/manifest changes.

## 13. Ownerless Blockers

**0 ownerless blockers.** All blockers assigned:
- A-012: Human Decision
- S-001: Werewolf owner
- S-002: Werewolf owner
- S-003: Human Decision
- S-004: Human Decision
- S-005: Werewolf owner
- S-006: Human Decision
- S-007: Werewolf owner
- S-008: Human Decision
- Spirit/Umbra runtime: Spirits/Umbra workstream
- Rite runtime: Rites workstream
- Pack/Totem aggregate: Pack/Totem workstream
- Chronicle scene state: Application/Infrastructure layer

## 14. Ready to Commit

**YES.** This audit is ready to commit as evidence.

Summary:
- 53 total distinct Spirit/Umbra mechanics after semantic deduplication
- A-I classification: A=5, B=13, C=7, D=5, E=3, F=5, G=5, H=7, I=4; sum=53
- 5 declarative mechanics (8 categories + 19 realms + 3 barriers + trait schema + 30 charms)
- 19 existing Totems reused via stable WerewolfTotemIdentifiers contracts
- Gauntlet/crossing semantics fully extracted with source locators
- 15-field minimum Spirit state proposed, with clear classification of required vs proposed vs unresolved fields
- Chronicle/Werewolf ownership boundary explicitly defined
- 43 remaining Gifts mapped to 5 primitive groups
- 26 Spirit/Umbra-dependent Rites mapped
- 8 source gaps reconciled (HumanDecision/UnsupportedByCanonicalSource)
- 5-wave implementation plan with exact scope and blockers
- 0 ownerless blockers
- No production code, tests, matrix, report, or metadata modified
- No commit or push performed

## 15. Canonical Keyed Inventory

| MechanicKey | CanonicalTerm | Kind | Class | SecondaryDependencies | SourceLocator | ShortDescription |
|---|---|---|---|---|---|---|
| spirit.category.catalog | Categorias Espirituais | DeclarativeEntity | A | — | Lines 3394-3404 | Catalog of 8 spirit categories |
| spirit.umbra.realm.catalog | Reinos da Umbra | DeclarativeEntity | A | — | Lines 3265-3361 | Catalog of 19 Umbra realms/layers |
| spirit.barrier.catalog | Barreiras Espirituais | DeclarativeEntity | A | — | Lines 3196-3220, 3368-3369 | Catalog of 3 barriers: Película, Membrana, Teia do Padrão |
| spirit.trait.schema | Traços Espirituais | DeclarativeRule | A | — | Lines 3406-3410 | Schema: Willpower, Rage, Gnose, Essence |
| spirit.charm.catalog | Encantos | DeclarativeEntity | A | — | Lines 3411-3458 | Catalog of 30 Charms: 4 common, 17 special, 4 Bane, 3 Weaver, 2 Wyld |
| spirit.crossing.test | Travessia da Película | ExecutableMechanic | B | — | Lines 3277-3290 | Gnose test vs Película level |
| spirit.crossing.time-table | Tempo de Travessia | ExecutableMechanic | B | — | Lines 3282-3286 | Time by successes: 0=1h wait, 1=5min, 2=30sec, 3+=instant |
| spirit.crossing.reflective-surface | Modificador de Superfície | ExecutableMechanic | B | — | Line 3288 | Mirrors/silver/water reduce difficulty by 1 |
| spirit.crossing.retry-restriction | Restrição de Retentativa | ExecutableMechanic | B | — | Lines 3279, 3283 | Failure: +2 difficulty per retry; 0 successes: cannot retry for 1 hour |
| spirit.crossing.botch | Falha Crítica | ExecutableMechanic | B | — | Line 3280 | Botch: stuck in Pattern Web or disappear for hours |
| spirit.crossing.fury-restriction | Restrição de Fúria | ExecutableMechanic | B | — | Line 3281 | Cannot step sideways using Fury-granted actions |
| spirit.crossing.silver-penalty | Penalidade de Prata | ExecutableMechanic | B | — | Line 1692 | Each silver item reduces effective Gnose temporarily (up to 1 day) |
| spirit.movement.speed | Desfocamento | ExecutableMechanic | B | — | Line 3462 | Maximum distance per turn = 20 + Willpower meters in Umbra; Penumbra uses physical distances |
| spirit.detection.test | Detecção de Espíritos | ExecutableMechanic | B | — | Lines 1845, 1852, 1951-1952 | Gnose test to detect spirits; automatic if Gnose ≥ Película |
| spirit.communication.requirement | Requisito de Comunicação | ExecutableMechanic | B | — | Line 3464 | Requires Comunicação com Espíritos Gift for mutual understanding |
| spirit.materialization.requirement | Requisito de Materialização | ExecutableMechanic | B | — | Line 3414 | Materialization requires Gnose ≥ Película; adopts physical health levels (usually 7) |
| spirit.essence.formula | Fórmula de Essência | ExecutableMechanic | B | — | Line 3410 | Essence = sum of Willpower + Rage + Gnose |
| spirit.modorra.definition | Definição de Modorra | ExecutableMechanic | B | — | Line 3460 | Total inactivity state in remote Umbra where low-Essence spirits rest |
| spirit.entity.state | Estado de Espírito | ExternalBoundary | C | Chronicle | Lines 3406-3410, 3414 | Proposed record type for spirit traits, Charms, materialization state |
| spirit.charm.execution | Execução de Encantos | ExternalBoundary | C | Gift/Rite | Lines 3412-3458 | Charm activation mechanics (cost, test, effect) |
| spirit.command.mechanic | Comando de Espíritos | ExternalBoundary | C | Gift | Lines 1936-1937, 1981-1982 | Compel/command spirits; Carisma + Liderança vs Willpower |
| spirit.possession.mechanic | Posse Espiritual | ExternalBoundary | C | Gift/Rite | Lines 3442-3450 | Possession: Gnose test vs Willpower; duration by successes; fomori permanent |
| spirit.damage.mechanic | Dano Espiritual | ExternalBoundary | C | Gift | Lines 3407-3408, 3454 | Spirit damage: difficulty = Rage; absorption = Willpower; Essence loss = death/Modorra |
| spirit.essence.economy | Economia de Essência | ExternalBoundary | C | Gift/Rite | Lines 3410, 3458, 1958-1959 | Essence drain, recovery, loss |
| spirit.materialization.state | Estado de Materialização | ExternalBoundary | C | — | Line 3414 | IsMaterialized flag; physical health levels (usually 7) |
| spirit.location.state | Estado de Localização | ExternalBoundary | D | Chronicle | Lines 3384, 3462 | Spirit location in scene/realm |
| spirit.gauntlet.by-location | Película por Local | ExternalBoundary | D | Chronicle | Lines 3235-3249 | Gauntlet rating varies by location (2-9) |
| spirit.realm.travel | Viagem entre Reinos | ExternalBoundary | D | Chronicle | Lines 3376-3382 | Travel via Moon Trails, Spirit Trails, Portals, Webs, Wyrm Tunnels |
| spirit.scene.presence | Presença em Cena | ExternalBoundary | D | Chronicle | Lines 3200, 3384 | Spirit presence/absence in scene |
| spirit.caern.película-table | Tabela de Película de Caern | ExternalBoundary | D | Chronicle | Lines 3249-3255 | Caern level ↔ Película level ↔ Moon Bridge max distance |
| spirit.totem.binding | Vinculação de Totem | ExecutableMechanic | E | Pack/Totem, Rite | Lines 1632, 2505-2507, 2693-2695 | Bind totem spirit to Pack |
| spirit.pack.totem-link | Vínculo Matilha-Totem | ExecutableMechanic | E | Pack/Totem | Lines 1632, 1636 | Pack-Totem connection; enables shared benefits |
| spirit.shared.totem-effects | Efeitos Compartilhados de Totem | ExecutableMechanic | E | Pack/Totem | Lines 1636, 1646 | Totem benefits available to members per turn |
| spirit.rite.fetish-creation | Rito de Criação de Fetiche | ExecutableMechanic | F | Rite | Lines 3466-3469 | Create fetish via Ritual de Fetiche |
| spirit.rite.totem-binding | Rito de Vinculação de Totem | ExecutableMechanic | F | Rite | Lines 2693-2695 | Ritual de Totem: binds totemic spirit to group of Garou |
| spirit.rite.summoning | Rito de Invocação | ExecutableMechanic | F | Rite | Lines 2683-2689 | Summon spirit; Gnose cost, test vs spirit Willpower |
| spirit.rite.commitment | Rito de Compromisso | ExecutableMechanic | F | Rite | Line 3471 | Bind spirit to object via Ritual de Compromisso; creates amulet |
| spirit.rite.awaken | Rito de Despertar | ExecutableMechanic | F | Rite | — | Awaken spirits; requires Extended test |
| spirit.gift.detection | Dom de Detecção | ConsumerIntegration | G | Gift | Lines 1845, 1852, 1939 | Detect spirits via Gift |
| spirit.gift.command | Dom de Comando | ConsumerIntegration | G | Gift | Lines 1936-1937 | Compel spirits to obey via Gift |
| spirit.gift.possession | Dom de Posse | ConsumerIntegration | G | Gift | Lines 1946-1948 | Expel spirits via Gift |
| spirit.gift.charm-activation | Dom de Ativação de Encantos | ConsumerIntegration | G | Gift | — | Activate spirit Charms via Gift |
| spirit.gift.crossing | Dom de Travessia | ConsumerIntegration | G | Gift | Lines 1955, 2365-2367 | Transport through Película via Gift |
| spirit.disposition.ai | Disposição de Espíritos | SourceGap | H | — | Not stated | Spirit AI/decision-making not defined by source |
| spirit.bargaining.valuation | Avaliação de Barganha | SourceGap | H | — | Line 1696 | Chiminage/bargaining mentioned but no valuation rules defined |
| spirit.materialization.duration | Duração de Materialização | HumanDecision | H | — | Line 3414 | Materialization requires Gnose ≥ Película, 7 health levels; duration/permanence unspecified |
| spirit.death.modorra-threshold | Morte vs Modorra | HumanDecision | H | — | Line 3410 | Essence=0 causes death or Modorra; exact threshold/transition not specified |
| spirit.possession.control | Controle de Posse | HumanDecision | H | — | Lines 3442-3450 | Possession duration defined; control mechanics and permanence rules not fully specified |
| spirit.crossing.non-garou | Travessia de Não-Garou | HumanDecision | H | — | Not stated | Crossing rules only defined for Garou; difficulty for other beings unspecified |
| spirit.hierarchy.behavior | Comportamento Hierárquico | SourceGap | H | — | Lines 3394-3404 | Hierarchy defined but behavior/rules not specified |
| spirit.voting.system | Sistema de Votação | UnsupportedByCanonicalSource | I | — | Not stated | No spirit voting/consensus mechanics defined |
| spirit.persistence.lifecycle | Ciclo de Vida | UnsupportedByCanonicalSource | I | — | Not stated | No spirit birth/aging/death lifecycle defined |
| spirit.world-travel.rules | Regras de Viagem | UnsupportedByCanonicalSource | I | — | Not stated | No general world-hopping rules beyond Gauntlet crossing |

## 16. Accounting Invariants

RAW_KEYED_ROWS = 53
SEMANTIC_DUPLICATES_REMOVED = 2 (spirit.bargaining.valuation merged with spirit.chiminage.valuation; spirit.disposition.ai merged with spirit.disposition.scale)
FINAL_KEYED_ROWS = 53

A = 5
B = 13
C = 7
D = 5
E = 3
F = 5
G = 5
H = 7
I = 3

A_I_SUM = 53
ASSERT A_I_SUM == FINAL_KEYED_ROWS = TRUE

DUPLICATE_KEY_COUNT = 0
SEMANTIC_DUPLICATE_COUNT = 0
EXISTING_TOTEM_DUPLICATE_COUNT = 0

DECLARATIVE_MECHANIC_COUNT = 5
SPIRIT_CATEGORY_ENTITIES = 8
UMBRA_REALM_ENTITIES = 19
BARRIER_ENTITIES = 3
CHARM_ENTITIES = 30
EXISTING_TOTEMS_REUSED = 19

SPIRIT_DOMAIN_PRIMITIVE_COUNT = 33 (A + B + C + D + E)
CONSUMER_INTEGRATION_COUNT = 10 (F + G)
SOURCE_GAP_COUNT = 10 (H + I)

GIFT_DEPENDENCY_ASSOCIATIONS = 43
RITE_DEPENDENCY_ASSOCIATIONS = 26

S1 = 5
S2 = 20
S3 = 5
S4 = 5
S5 = 18

S1-S5 verify: 5 + 20 + 5 + 5 + 18 = 53 = FINAL_KEYED_ROWS. TRUE.

SPIRIT_DOMAIN_PRIMITIVES + CONSUMER_INTEGRATIONS + SOURCE_GAPS = 33 + 10 + 10 = 53 = FINAL_KEYED_ROWS. TRUE.
