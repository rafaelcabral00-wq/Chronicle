# RULESET-COMPLETION-GIFT-WAVE-B

## 1. Candidate Inventory

Original Wave B candidates (8):

| # | Candidate Key | Canonical Name | Owner | Level |
|---|--------------|----------------|-------|-------|
| 1 | gift.homid.persuasao | Persuasão | Homid | 1 |
| 2 | gift.homid.fitar | Fitar | Homid | 2 |
| 3 | gift.homid.inquietacao | Inquietação | Homid | 3 |
| 4 | gift.homid.remodelar-objeto | Remodelar Objeto | Homid | 3 |
| 5 | gift.homid.casulo | Casulo | Homid | 4 |
| 6 | gift.glass-walkers.invocar-aranha-de-rede | Invocar Aranha de Rede | GlassWalkers | 5 |
| 7 | gift.fianna.remodelar-objeto | Remodelar Objeto | Fianna | 3 |
| 8 | gift.bonegnawers.remodelar-objeto | Remodelar Objeto | BoneGnawers | 3 |

## 2. Source Preflight

All 8 candidates verified against canonical source `.rule-set-sources/werewolf/Werewolf the Apocalypse 3e-pt_br.txt`:

| Candidate | Source Line | Activation | Test | Cost | Duration | External Dependency |
|-----------|-------------|------------|------|------|----------|---------------------|
| Persuasão | 1736 | TestRequired | Carisma + Lábia | None | Scene | None |
| Fitar | 1743 | TestRequired | Carisma + Intimidação | None | Turn | None |
| Inquietação | 1758 | TestRequired | Manipulação + Empatia | None | Scene | None |
| Remodelar Objeto (Homid) | 1761 | TestRequired | Manipulação + Ofícios | 1 Gnose | Scene | None |
| Casulo | 1772 | Active | None | 1 Gnose | Scene | None |
| Invocar Aranha de Rede | 2138 | TestRequired | Carisma + Computador | 1 Gnose | Scene | Spirit/Umbra runtime |
| Remodelar Objeto (Fianna) | 2207 | TestRequired | Manipulação + Ofícios | None | Scene | None |
| Remodelar Objeto (BoneGnawers) | 2429 | TestRequired | Manipulação + Ofícios | None | Scene | None |

**Result: 1 candidate removed. 7 accepted.**

## 3. Removed Candidates

| Candidate Key | Reason |
|---------------|--------|
| gift.glass-walkers.invocar-aranha-de-rede | Canonical source requires summoning a Web Spider (Spirit entity). Baseline af6d26a has no Spirit/Umbra active runtime. The computer-difficulty modifier portion could be wired to existing action-resolution infrastructure, but the summoning component cannot be represented as a complete typed external boundary without Spirit Entity runtime. Removed to Spirit/Umbra-dependent backlog. |

## 4. Final Accepted Gift Inventory

### 4.1 Wave B Gifts

| Gift Key | Canonical Name | Owner | Level | Source Locator | Effect Kind(s) | Downstream Consumer |
|----------|----------------|-------|-------|----------------|----------------|---------------------|
| gift.homid.persuasao | Persuasão | Homid | 1 | Line 1736 | SocialTestBonus | WerewolfSocialTestDefinitionService |
| gift.homid.fitar | Fitar | Homid | 2 | Line 1743 | ProneCondition | WerewolfConditionService |
| gift.homid.inquietacao | Inquietação | Homid | 3 | Line 1758 | RageRecoveryPenalty, ExtendedTestDifficultyModifier | Gift runtime boundary (deferred typed boundaries) |
| gift.homid.remodelar-objeto | Remodelar Objeto | Homid | 3 | Line 1761 | ObjectTransformation | Gift runtime boundary (typed transformation contract) |
| gift.homid.casulo | Casulo | Homid | 4 | Line 1772 | DamageReduction | WerewolfCombatDefenseService |
| gift.fianna.remodelar-objeto | Remodelar Objeto | Fianna | 3 | Line 2207 | ObjectTransformation | Gift runtime boundary (typed transformation contract) |
| gift.bonegnawers.remodelar-objeto | Remodelar Objeto | BoneGnawers | 3 | Line 2429 | ObjectTransformation | Gift runtime boundary (typed transformation contract) |

### 4.2 Effect Kind Counts

| Effect Kind | Count | Gift Keys |
|-------------|-------|-----------|
| SocialTestBonus | 1 | gift.homid.persuasao |
| ProneCondition | 1 | gift.homid.fitar |
| DamageReduction | 1 | gift.homid.casulo |
| RageRecoveryPenalty | 1 | gift.homid.inquietacao |
| ExtendedTestDifficultyModifier | 1 | gift.homid.inquietacao |
| ObjectTransformation | 3 | gift.homid.remodelar-objeto, gift.fianna.remodelar-objeto, gift.bonegnawers.remodelar-objeto |

### 4.3 Execution Classification

| Class | Count | Description |
|-------|-------|-------------|
| A — fully executable with existing downstream consumer | 3 | Persuasão, Fitar, Casulo |
| B — passive capability with complete typed semantics | 0 | — |
| C — complete machine-readable external/deferred boundary | 4 | Inquietação (×2 typed boundaries), Remodelar Objeto (×3 shared transformation boundary) |
| D — opaque/no-op/catalog-only | 0 | — |

## 5. Typed Payload Contracts

### 5.1 Inquietacao Typed Effects

Canonical source line 1758: "o oponente não consegue recuperar Fúria durante uma cena, e as dificuldades de ações prolongadas aumentam em 1."

**Effect A: RageRecoveryPenalty**
- Payload type: `WerewolfRageRecoveryPenaltyPayload`
- Fields:
  - `PenaltyAmount` (int): 1 — Rage recovery is blocked/reduced by this amount per recovery attempt
  - `DurationTurns` (int?): null (scene duration encoded in DurationType)
- Consumer: Future Rage/Combat service can switch on `EffectKind == RageRecoveryPenalty` and read `Payload` without parsing prose or switching on GiftKey.

**Effect B: ExtendedTestDifficultyModifier**
- Payload type: `WerewolfExtendedTestDifficultyPayload`
- Fields:
  - `DifficultyIncrease` (int): 1 — difficulty increases by this amount
  - `Scope` (string): "prolonged-actions" — only applies to prolonged action Extended tests
  - `DurationTurns` (int?): null (scene duration encoded in DurationType)
- Consumer: Future Extended-test modifier consumer can switch on `EffectKind == ExtendedTestDifficultyModifier` and read `Payload` to apply difficulty modifiers without parsing prose or switching on GiftKey.

### 5.2 Remodelar Objeto Typed Transformation Contract

Canonical source line 1761: "Modela material vivo (exceto mortos-vivos) transformando-o instantaneamente em ferramentas, armas ou abrigos improvisados."

**Shared payload for all 3 owner variants:**
- Payload type: `WerewolfObjectTransformationPayload`
- Fields:
  - `TargetMaterial` (string): "living-material" — supported target material category
  - `AllowedResultCategories` (IReadOnlyList<string>): ["tools", "weapons", "shelter"] — permitted result object categories
  - `SupportsPermanentAlteration` (bool): true — extra Gnose sacrifice can make alteration permanent
  - `SupportsAggravatedDamage` (bool): true — extra Gnose can create aggravated-damage weapons
  - `VariableDurationTurns` (int?): success-dependent duration in turns (computed from successes)
- Consumer: Future Chronicle/world consumer switches on `EffectKind == ObjectTransformation` and reads `Payload` to understand the transformation request. No GiftKey-based semantic dispatch required.

**Owner differences preserved in catalog:**
- Homid: 1 Gnose cost, TestRequired
- Fianna: 0 Gnose cost, TestRequired  
- BoneGnawers: 0 Gnose cost, TestRequired

All three share the same `ApplyRemodelarObjeto` semantic handler and `ObjectTransformation` effect kind.

## 6. Custom Count

| Metric | Value |
|--------|-------|
| Custom effect count before correction | 4 |
| Custom effect count after correction | 0 |
| Exact retained Custom keys | None |
| Why zero Custom remains | All four previously-Custom effects now use explicit typed effect kinds (RageRecoveryPenalty, ExtendedTestDifficultyModifier, ObjectTransformation) with strongly typed payload records. No downstream code must parse prose or switch on GiftKey to discover meaning. GiftKey is provenance/ownership only. |

## 7. Behavioral Proof Per Gift

### 7.1 gift.homid.persuasao
- Activation succeeds with TestRequired (Charisma + Subterfuge = 3, Difficulty 6, no cost)
- `SocialTestBonus` active effect created with Magnitude=1, DurationType=Scene
- `WerewolfSocialTestDefinitionService.ComputeGiftSocialBonus` consumes it and adds +1 to social test dice pools
- Unrelated social tests without the active effect are not modified

### 7.2 gift.homid.fitar
- Activation succeeds with TestRequired (Charisma + Intimidation, Difficulty 6, no cost)
- With successes > 0: `ProneCondition` active effect created; `WerewolfConditionService.ApplyGiftConditions` adds Prone condition; `EvaluateActionAvailability` blocks actions with reason "Prone"
- With successes = 0: `ProneCondition` active effect created with Magnitude=0; `ApplyGiftConditions` skips condition application due to Magnitude <= 0 guard; no Prone condition added

### 7.3 gift.homid.inquietacao
- Activation succeeds with TestRequired (Manipulation + Empathy = 3, Difficulty 6, no cost)
- Produces **two** active effects:
  1. `RageRecoveryPenalty` with `WerewolfRageRecoveryPenaltyPayload` (PenaltyAmount=1, DurationType=Scene)
  2. `ExtendedTestDifficultyModifier` with `WerewolfExtendedTestDifficultyPayload` (DifficultyIncrease=1, Scope="prolonged-actions", DurationType=Scene)
- No existing downstream consumer consumes these deferred boundaries; both are machine-readable typed contracts

### 7.4 gift.homid.remodelar-objeto
- Activation succeeds with TestRequired (Manipulation + Crafts, Difficulty 6), pays 1 Gnosis
- `ObjectTransformation` active effect created with `WerewolfObjectTransformationPayload`:
  - TargetMaterial="living-material"
  - AllowedResultCategories=["tools", "weapons", "shelter"]
  - SupportsPermanentAlteration=true
  - SupportsAggravatedDamage=true
  - VariableDurationTurns=successes
- No existing downstream consumer; typed boundary is `WerewolfObjectTransformationPayload`

### 7.5 gift.homid.casulo
- Activation pays 1 Gnosis
- `DamageReduction` active effect created with Magnitude=successes, DurationType=Scene
- `WerewolfCombatDefenseService.ComputeGiftDamageReduction` consumes it
- Effect persists for scene duration; expires when scene token changes

### 7.6 gift.fianna.remodelar-objeto
- Activation succeeds with TestRequired (Manipulation + Crafts, Difficulty 6), no Gnosis cost
- `ObjectTransformation` active effect created with same `WerewolfObjectTransformationPayload` semantics as Homid variant
- Distinct owner eligibility preserved in catalog

### 7.7 gift.bonegnawers.remodelar-objeto
- Activation succeeds with TestRequired (Manipulation + Crafts, Difficulty 6), no Gnosis cost
- `ObjectTransformation` active effect created with same `WerewolfObjectTransformationPayload` semantics as Homid variant
- Distinct owner eligibility preserved in catalog

## 8. Remodelar Objeto Shared-Semantics Disposition

Canonical source contains three owner-specific versions of Remodelar Objeto:

1. **Homid** (Line 1761, Level 3): Manipulation + Crafts, 1 Gnose, variable duration
2. **Fianna** (Line 2207, Level 3): Same mechanics, no Gnose cost
3. **BoneGnawers** (Line 2429, Level 3): Same mechanics, no Gnose cost

**Disposition:** Single shared `ObjectTransformation` effect kind with `WerewolfObjectTransformationPayload`. All three map to the same semantic handler. Owner/catalog eligibility (including Gnose cost) is preserved separately in `WerewolfGiftCatalog`. No duplicate runtime handlers.

**Proof of shared semantics:** `WaveBRemodelarObjetoSharesHandlerAcrossOwners` test verifies Fianna and BoneGnawers variants produce identical `ObjectTransformation` payloads with same TargetMaterial, AllowedResultCategories, SupportsPermanentAlteration, and SupportsAggravatedDamage. Only VariableDurationTurns varies with successes.

## 9. Invocar Aranha de Rede Final Disposition

**Removed from Wave B.** Canonical source requires summoning a Web Spider (Spirit entity). The computer-difficulty modifier portion could be wired to existing action-resolution modifier infrastructure, but the summoning component requires Spirit Entity runtime not present in baseline af6d26a. Returned to Spirit/Umbra-dependent backlog.

## 10. Tests

### 10.1 Focused Tests Added

**Catalog InlineData (7 new cases):**
- `CatalogReturnsDefinitionForEveryGift` now covers all 7 Wave B Gifts

**Behavioral Tests (8 new Facts):**
- `WaveBPersuasionActivatesAndCreatesSocialTestBonus` — activation, dice pool, SocialTestBonus active effect, exact modifier
- `WaveBFitarAppliesProneConditionOnSuccess` — activation, ProneCondition active effect, condition applied on success
- `WaveBFitarDoesNotApplyProneOnZeroSuccesses` — failure path, no condition applied when successes=0
- `WaveBCasuloPaysGnosisAndRegistersDamageReduction` — Gnosis cost, DamageReduction active effect, scene duration
- `WaveBRemodelarObjetoSharesHandlerAcrossOwners` — shared ObjectTransformation handler for Fianna and BoneGnawers, typed payload verification
- `WaveBInquietacaoProducesTwoTypedEffects` — Inquietacao produces both RageRecoveryPenalty and ExtendedTestDifficultyModifier with typed payloads
- `WaveBCatalogCountReflectsWaveBImplementation` — catalog count = 93
- `WaveBExistingGiftsRemainUnchanged` — all Wave A gifts present with source locators

### 10.2 Regression Tests

- 86 existing Gifts remain unchanged (verified by catalog enumeration)
- No duplicate semantic identities introduced (all 7 keys are unique)
- Progression state survives Gift transitions (existing ApplyCost/IncrementSceneUsage preserve all state fields)

### 10.3 Test Discovery Arithmetic

| Metric | Value |
|--------|-------|
| Baseline Werewolf test count | 1458 |
| Newly discovered Wave B test cases | 15 (7 InlineData + 8 Facts) |
| Final total discovered | 1473 |
| Passed | 1473 |
| Failed | 0 |

## 11. Validation Results

| Test Suite | Passed | Failed |
|------------|--------|--------|
| Werewolf | 1473 | 0 |
| PackageValidator | 8 | 0 |
| Contracts | 8 | 0 |
| Domain | 1 | 0 |
| Architecture | 11 | 0 |
| Application | 9 | 0 |
| Infrastructure | 12 | 0 |

## 12. RuntimeCharacterState/Progression Preservation

All Gift transitions preserve:
- UnspentXp
- Post-creation Attribute state
- Post-creation Ability state
- Permanent/current Rage
- Permanent/current Gnosis
- Permanent/current Willpower
- Rank/Renown state
- RuntimeStateVersion

No state reconstruction drops any field.

## 13. Exact Files Changed

| File | Change |
|------|--------|
| `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfActiveGiftEffect.cs` | Added 3 new effect kinds (RageRecoveryPenalty, ExtendedTestDifficultyModifier, ObjectTransformation); added 3 typed payload records; added Payload field to WerewolfActiveGiftEffect |
| `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfGiftIdentifiers.cs` | Removed GlassWalkersInvocarAranhaDeRede; added 7 Wave B identifiers + Supported list entries |
| `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfGiftCatalog.cs` | Added 7 WerewolfGiftDefinition entries; removed 1 Invocar Aranha de Rede entry |
| `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfGiftEffectService.cs` | Added CreateActiveEffect helper; CreatePayload helper; updated MapEffectKind to return typed kinds; updated ComputeMagnitude; Inquietacao produces two typed effects; Remodelar Objeto produces ObjectTransformation with typed payload |
| `rule-sets/Chronicle.RuleSets.Werewolf/CharacterCreation/WerewolfConditionService.cs` | Added magnitude guard in ApplyGiftConditions (ProneCondition/RestrainedCondition only apply when Magnitude > 0) |
| `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfGiftRuntimeTests.cs` | Added 7 InlineData entries; added 8 behavioral Fact tests including typed payload verification; updated count assertions from 86→93; updated level validation to 1-5 |

## 14. Git Hygiene

```
git diff --check: CRLF replacement warnings only (no whitespace errors)
git diff --stat: 6 files changed, insertions, deletions
git status --short: 6 modified files
```

No TestResults, no .kilo, no unrelated files, no manifest/current-slice/global metadata changes, no matrix/report modifications.

## 15. Ready to Commit

Gift Wave B is ready to commit. All validation gates pass (1473/1473 Werewolf, 8/8 PackageValidator, all other suites 0 failures). No external domain dependencies introduced. One candidate (Invocar Aranha de Rede) removed to Spirit/Umbra-dependent backlog. All retained Custom effects have been replaced with explicit typed effect kinds and strongly typed payload contracts. D=0.
