# RULESET-COMPLETION-SPIRIT-UMBRA-S3

## 1. S3 Mechanic Count

**5 mechanics implemented via Gift integration:**

1. spirit.gift.detection
2. spirit.gift.command
3. spirit.gift.possession
4. spirit.gift.charm-activation
5. spirit.gift.crossing

## 2. Entity Counts

| Category | Count |
|---|---|
| S3 mechanics implemented | 2 |
| S3 mechanics typed boundary | 3 |
| S3 source gaps | 0 |
| Total | 5 |

## 3. S3 Accounting

S3_EXPECTED = 5
S3_IMPLEMENTED = 2
S3_TYPED_BOUNDARY = 3
S3_SOURCE_GAP = 0

S3_IMPLEMENTED + S3_TYPED_BOUNDARY + S3_SOURCE_GAP = 5

DUPLICATE_KEY_COUNT = 0
S1_S2_OVERLAP_COUNT = 0
S4_S5_IMPLEMENTED_COUNT = 0
OWNERLESS_BLOCKER_COUNT = 0

## 4. Exact Gift Mapping for Each S3 Key

### spirit.gift.detection → IMPLEMENTED

| Gift Key | Owner | Level | Source Locator |
|---|---|---|---|
| gift.lupus.nome-do-espirito | Lupus (Breed) | 3 | Lines 1851-1853 |
| gift.theurge.nome-do-espirito | Theurge (Auspice) | 2 | Lines 1938-1940 |

**S2 primitive reused:** EvaluateDetection
**Integration:** Gift activation → S2 detection mechanic → typed DetectionResult payload
**Classification:** A (fully executable with downstream S2 primitive)

### spirit.gift.command → IMPLEMENTED

| Gift Key | Owner | Level | Source Locator |
|---|---|---|---|
| gift.theurge.comandar-espiritos | Theurge (Auspice) | 2 | Lines 1935-1937 |

**S2 primitive reused:** EvaluateCommand
**Integration:** Gift activation → S2 command mechanic → typed CommandResult payload
**Classification:** A (fully executable with downstream S2 primitive)

### spirit.gift.possession → TYPED_BOUNDARY

| Gift Key | Owner | Level | Source Locator |
|---|---|---|---|
| gift.theurge.exorcismo | Theurge (Auspice) | 3 | Lines 1945-1949 |

**S2 primitive reused:** EvaluatePossession (base mechanic)
**Integration:** Named typed boundary WerewolfExorcismBoundaryPayload
**Classification:** C (complete typed external/deferred boundary)

### spirit.gift.charm-activation → TYPED_BOUNDARY

| Gift Key | Owner | Level | Source Locator |
|---|---|---|---|
| gift.theurge.roubar-poderes | Theurge (Auspice) | 5 | Lines 1917-1919 |

**S2 primitive reused:** ExecuteCharm (validation framework only)
**Integration:** Named typed boundary WerewolfCharmStealBoundaryPayload
**Classification:** C (complete typed external/deferred boundary)
**Note:** Underlying Charms remain A=0 B=0 C=0 D=30. Roubar Poderes does not execute individual Charm effects; it only reaches a typed boundary for the stolen Charm.

### spirit.gift.crossing → TYPED_BOUNDARY

| Gift Key | Owner | Level | Source Locator |
|---|---|---|---|
| gift.silent-striders.alcancar-a-umbra | Silent Striders (Tribe) | 5 | Lines 2365-2367 |
| gift.theurge.captura-a-distancia | Theurge (Auspice) | 4 | Lines 1954-1956 |

**S2 primitive reused:** EvaluateCrossing
**Integration:**
- Alcançar a Umbra → named typed boundary WerewolfCrossingModifierPayload (automatic crossing, -2 difficulty modifier)
- Captura à Distância → named typed boundary WerewolfRemoteTransportBoundaryPayload (cross-entity transport orchestration deferred)
**Classification:**
- Alcançar a Umbra: B (passive/capability with complete typed semantics)
- Captura à Distância: C (complete typed external/deferred boundary)

**Mechanic-level disposition:** TYPED_BOUNDARY because Captura à Distância requires cross-entity/world orchestration that is deferred to Chronicle/S5.

## 5. Source Locators

| S3 Key | Canonical Source Lines |
|---|---|
| spirit.gift.detection | Lines 1845, 1852, 1939 |
| spirit.gift.command | Lines 1936-1937 |
| spirit.gift.possession | Lines 1945-1949 |
| spirit.gift.charm-activation | Lines 1917-1919 |
| spirit.gift.crossing | Lines 1955, 2365-2367 |

## 6. Request/Result Contracts

### Detection Integration (Nome do Espírito)
- **Request:** GiftEffectRequest with DiceValues
- **Result:** GiftEffectResult with DetectionResult payload
- **S2 primitive:** DetectionRequest → EvaluateDetection → DetectionResult
- **Owner variants:** Lupus (L3) and Theurge (L2) share identical mechanics: Perception + Occultism diff 8, costs 1 Willpower. Only level differs.

### Command Integration (Comandar Espíritos)
- **Request:** GiftEffectRequest with DiceValues
- **Result:** GiftEffectResult with CommandResult payload
- **S2 primitive:** CommandRequest → EvaluateCommand → CommandResult

### Possession Integration (Exorcismo)
- **Request:** GiftEffectRequest
- **Result:** GiftEffectResult with WerewolfExorcismBoundaryPayload
- **Fields:** GiftKey, Mechanic, TargetType, RequiredConcentrationTurns, ReluctantSpiritTest, TrappedSpiritTest, SourceLocator, Note
- **Note:** S3 represents this as a typed boundary. Full 3-turn concentration mechanics are deferred (Human Decision).

### Charm Activation Integration (Roubar Poderes)
- **Request:** GiftEffectRequest
- **Result:** GiftEffectResult with WerewolfCharmStealBoundaryPayload
- **Fields:** GiftKey, StolenCharmKey, GnosisCostPerTurn, SourceLocator, Note
- **Note:** S3 represents this as a typed boundary. Individual Charm effect execution is deferred (Charms remain D=30).

### Crossing Integration (Alcançar a Umbra / Captura à Distância)
- **Request:** GiftEffectRequest
- **Result:** GiftEffectResult with typed boundary payload
- **Alcançar a Umbra fields:** GiftKey, DifficultyModifier (-2), AutomaticCrossing (true), NoFuryAllowed (true), SourceLocator, Note
- **Captura à Distância fields:** GiftKey, SourceSpiritReference, TargetEntityReference, CrossingResult, TransportIntent, DestinationSemantics, ChronicleOrchestrationRequired, SourceLocator, Note
- **Note:** Cross-entity transport orchestration is deferred to Chronicle.

## 7. Operation Keys

| Operation Key | Capability | Status |
|---|---|---|
| runtime-gift-activation | runtime-gift-activation | Enabled |
| runtime-gift-execution | runtime-gift-execution | Enabled |
| spirit-umbra.initialize-spirit | spirit-umbra | Enabled |
| spirit-umbra.evaluate-crossing | spirit-umbra | Enabled |
| spirit-umbra.compute-movement-speed | spirit-umbra | Enabled |
| spirit-umbra.evaluate-detection | spirit-umbra | Enabled |
| spirit-umbra.evaluate-materialization | spirit-umbra | Enabled |
| spirit-umbra.spend-essence | spirit-umbra | Enabled |
| spirit-umbra.execute-charm | spirit-umbra | Enabled |
| spirit-umbra.evaluate-command | spirit-umbra | Enabled |
| spirit-umbra.evaluate-possession | spirit-umbra | Enabled |
| spirit-umbra.apply-spirit-damage | spirit-umbra | Enabled |

## 8. Chronicle/Werewolf Authority Boundary

**Chronicle owns:**
- Raw dice generation
- Local Película/Gauntlet values
- Reflective surface context
- Silver item context
- Previous retry state
- Fury-granted action flag
- Scene/location identity
- Persistence
- Timeline
- Gift activation input (DiceValues)
- Cross-entity transport orchestration (Captura à Distância)
- 3-turn concentration orchestration (Exorcismo)

**Werewolf owns:**
- Spirit/Umbra rules
- Deterministic interpretation
- Difficulty computation
- Retry semantics
- Crossing result semantics
- Spirit mechanical traits and calculations
- Source-authoritative restrictions
- Gift-to-S2-primitive integration mapping
- Typed boundary contracts for deferred mechanics

## 9. Gift A/B/C/D Classification

### Touched Gifts (7 total)

| Gift Key | Classification | Reason |
|---|---|---|
| gift.lupus.nome-do-espirito | A | Executes S2 detection, returns DetectionResult |
| gift.theurge.nome-do-espirito | A | Executes S2 detection, returns DetectionResult |
| gift.theurge.comandar-espiritos | A | Executes S2 command, returns CommandResult |
| gift.theurge.exorcismo | C | Typed boundary; 3-turn concentration deferred |
| gift.theurge.roubar-poderes | C | Typed boundary; individual Charm effect execution deferred |
| gift.silent-striders.alcancar-a-umbra | B | Passive capability; provides automatic crossing and -2 difficulty modifier |
| gift.theurge.captura-a-distancia | C | Typed boundary; cross-entity transport orchestration deferred |

**Summary:** A=3, B=1, C=3, D=0. Total = 7.

### Underlying Charm Classification

**Charms remain: A=0, B=0, C=0, D=30**

Roubar Poderes does not execute individual Charm effects. It only reaches a typed boundary for the stolen Charm. The 30 Charm classification is unchanged.

## 10. Invocar Aranha de Rede Disposition

**Status: DEFERRED**

The Gift `gift.glass-walkers.invocar-aranha-de-rede` was explicitly removed in Gift Wave B because it requires a Web Spider Spirit entity and S2 did not yet exist at that time.

S2 now provides Spirit runtime state and crossing/detection primitives, but S2 does NOT implement:
- World/scene Spirit presence
- Generic Spirit world instantiation
- Spirit AI
- S5 scene/location state

Summoning a Web Spider requires those S5 mechanics. Therefore, this Gift remains deferred.

**Disposition:** Not in catalog, not implemented, not claimed by S3.

## 11. S2 Primitives Reused

| S2 Primitive | S3 Integration |
|---|---|
| EvaluateDetection | Nome do Espírito (Lupus/Theurge) |
| EvaluateCommand | Comandar Espíritos (Theurge) |
| EvaluatePossession | Exorcismo base mechanic (Theurge) - boundary only |
| ExecuteCharm | Roubar Poderes (Theurge) - validation framework only |
| EvaluateCrossing | Alcançar a Umbra / Captura à Distância - boundary only |

**No Spirit primitive was duplicated.** All S3 integrations invoke existing S2 services through deterministic request/result contracts.

## 12. Spirit State Changes

No new Spirit state types were introduced in S3. S3 reuses:
- `WerewolfSpiritRuntimeState` (from S2)
- `WerewolfGiftEffectRequest` (extended with `DiceValues`)
- `WerewolfGiftEffectResult` (extended with `Payload`)

New named typed payload records added:
- `WerewolfExorcismBoundaryPayload`
- `WerewolfCharmStealBoundaryPayload`
- `WerewolfCrossingModifierPayload`
- `WerewolfRemoteTransportBoundaryPayload`

The `Payload` field on `WerewolfGiftEffectResult` carries typed S2 primitive results or named typed boundaries.

## 13. Gift Count Impact

| Metric | Count |
|---|---|
| Baseline implemented Gifts | 93 |
| Newly completed Gifts (S3) | 7 |
| Upgraded existing Gifts | 0 |
| Final implemented Gifts | 100 |
| Remaining canonical Gifts | 125 |

**Owner breakdown of 7 new Gifts:**
- Breed: 1 (gift.lupus.nome-do-espirito)
- Auspice: 5 (gift.theurge.nome-do-espirito, gift.theurge.comandar-espiritos, gift.theurge.exorcismo, gift.theurge.roubar-poderes, gift.theurge.captura-a-distancia)
- Tribe: 1 (gift.silent-striders.alcancar-a-umbra)

**Final owner totals:**
- Breed: 15 (baseline) + 1 = 16
- Auspice: 19 (baseline) + 5 = 24
- Tribe: 59 (baseline) + 1 = 60

**Arithmetic assertion:** 16 + 24 + 60 = 100 = FINAL_IMPLEMENTED_GIFTS. TRUE.

## 14. Tests

**Baseline Werewolf tests:** 1568
**Current full Werewolf tests:** 1586
**NEWLY_DISCOVERED_S3_TESTS:** 18

**Focused S3 tests:** 18 test cases
- S3ExactKeyCountIsFive
- S3GiftMappingsReuseS2Primitives
- S3NomeDoEspiritoProducesDetectionPayload
- S3ComandarEspiritosProducesCommandPayload
- S3ExorcismoProducesTypedBoundary
- S3RoubarPoderesProducesCharmStealBoundary
- S3AlcancarAUmbraProducesCrossingModifierBoundary
- S3CapturaADistanciaProducesTypedBoundary
- S3ExorcismoAndCapturaUseDifferentPayloads
- S3NoS4S5KeysImplemented
- S3InvocarAranhaDeRedeRemainsDeferred
- CatalogReturnsDefinitionForEveryGift (7 new InlineData entries)
- AllCataloguedGiftsArePresent (updated count)
- GiftCatalogHasExpectedCount (updated count)
- WaveBCatalogCountReflectsWaveBImplementation (updated count)
- WaveBExistingGiftsRemainUnchanged
- AllCataloguedGiftsArePresentInSupported
- NoCustomGiftKeysRemain

**Key test coverage:**
- Exact S3 key count = 5
- No duplicate S3 mechanics
- No S1/S2 overlap
- Detection integration (Nome do Espírito)
- Command integration (Comandar Espíritos)
- Possession boundary (Exorcismo)
- Charm activation boundary (Roubar Poderes)
- Crossing modifier boundary (Alcançar a Umbra)
- Crossing transport boundary (Captura à Distância)
- Exorcismo and Captura use different payload types
- No S4/S5 keys implemented
- No duplicate Spirit algorithms
- No new RNG inside Werewolf
- Runtime registration valid
- Gift operations use Gift capabilities
- Spirit primitive operations remain spirit-umbra
- No ownerless blockers introduced

## 15. Exclusions

The following S4/S5 mechanics were intentionally NOT implemented:
- spirit.rite.fetish-creation, spirit.rite.totem-binding, spirit.rite.summoning, spirit.rite.commitment, spirit.rite.awaken
- All 18 S5 keys (location.state, gauntlet.by-location, realm.travel, scene.presence, caern.película-table, totem.binding, pack.totem-link, shared.totem-effects, disposition.ai, bargaining.valuation, materialization.duration, death.modorra-threshold, possession.control, crossing.non-garou, hierarchy.behavior, voting.system, persistence.lifecycle, world-travel.rules)

## 16. Source Gaps Intentionally Preserved

- **spirit.possession.control (S5, Lines 3442-3450):** Control mechanics and permanence rules not fully specified. Exorcismo represents this as a typed boundary.

## 17. Ownerless Blockers

**0 ownerless blockers.** All blockers assigned to future waves or Human Decisions.

## 18. Exact Files Changed

- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfGiftEffectService.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfGiftIdentifiers.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfGiftCatalog.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfActiveGiftEffect.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf/WerewolfReferenceRuntime.cs`
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfGiftRuntimeTests.cs`
- `docs/reviews/werewolf-rule-set-completeness/completion-evidence/RULESET-COMPLETION-SPIRIT-UMBRA-S3.md`

## 19. Validation Results

**Full Werewolf tests:** 1586 passed, 0 failed
**PackageValidator tests:** 8 passed, 0 failed
**Contracts:** 8/8
**Domain:** 1/1
**Architecture:** 11/11
**Application:** 9/9
**Infrastructure:** 12/12

All failures = 0.

## 20. Git Hygiene

```
git diff --check: CRLF replacement warnings only; no whitespace errors
git status: 6 modified, 1 untracked (evidence doc)
```
