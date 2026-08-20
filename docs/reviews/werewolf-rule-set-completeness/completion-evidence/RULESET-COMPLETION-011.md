# RULESET-COMPLETION-011: Resolve Metis and Race/Breed Terminology

## 1. Exact Package Scope

**Title:** Resolve Metis and Race/Breed terminology

**Owned domain keys:**
- `Race selection` (completeness-matrix.json domain)
- `Metis deformity` (terminology aspect)
- Terminology/localization stability

**Exact ambiguity IDs:**
- A-017 — Metis Terminology and Localization
- A-018 — Race Versus Breed Naming

**Exact completion condition:**
A-017 and A-018 resolved; canonical technical keys stabilized; birth-identity vs current-form distinction documented; localization coverage complete.

**Exact expected executable artifacts:**
- Stable `WerewolfRaceIdentifiers` keys (`homid`, `metis`, `lupus`)
- Localization keys for birth identity in `en` and `pt-BR`
- Terminology-semantic tests
- Evidence document

**Exact terminology concepts owned by 011:**
- Birth identity machine keys (`homid`, `metis`, `lupus`)
- Birth identity display labels (localization)
- Metis terminology resolution (canonical key vs in-world synonym)
- Race vs Breed naming decision
- Birth identity vs shapeshift form distinction

**Exact concepts explicitly owned by 012 instead:**
- Shapeshift forms system (Hominídea/Glabro/Crinos/Hispo/Lupina)
- Form-specific mechanics
- Form transformation rules
- Full Race/Breed Gift catalog expansion

## 2. A-017 Disposition

**Original ambiguity:** The cleaned source uses more than one Portuguese label for Metis and includes socially derogatory in-world terms. The package must distinguish canonical technical key, neutral display label, in-world historical/derogatory synonym, narrative context, and localization policy.

**Source evidence:**
- Line 865: Table entry "Impura (Metis)" — both terms used together
- Line 874: "Impuro (Metis): Fruto da união proibida..."
- Narrative sections use "Impuros" as in-world classification
- Mechanical sections use "Metis" or "Impura (Metis)" in tables

**Final disposition:** ResolvedFromSource
- Canonical technical key: `metis`
- Neutral display label: "Metis" (both en and pt-BR)
- In-world derogatory synonym: "Impuro" — documented as narrative context only, not used as default UI label
- Machine key `metis` is already in use and stable
- No rename required

**Source fully resolves:** Yes. The source consistently pairs "Impura" with "Metis" in mechanical contexts, indicating "Metis" is the intended technical term.

**Human decision remaining:** No.

## 3. A-018 Disposition

**Original ambiguity:** The Portuguese source uses "Raça," while English source terminology may use another term. What canonical technical key should represent this classification? Candidates: `character.classification.race`, `character.classification.breed`, `character.origin.birth-form`.

**Source evidence:**
- Line 865: "Raça — A origem de nascimento do personagem (Hominídea, Lupina ou Impuro)"
- Line 871: "1. Raça (Origem de Nascimento)"
- Line 494: "A raça é determinada unicamente pela forma natural da mãe no momento do parto"
- Table at line 495-499 maps Raça to mother type and primary natural form

**Final disposition:** ResolvedFromSource
- Canonical concept: `Race` in code represents source "Raça" (birth identity/origin)
- Machine keys: `homid`, `metis`, `lupus` (stable, already in use)
- No rename required
- The source consistently uses "Raça" for birth identity classification

**Source fully resolves:** Yes. The source uses "Raça" consistently for birth identity, with clear mechanical meaning tied to mother's form at birth.

**Human decision remaining:** No.

## 4. Source Terminology Matrix

| Source Term | Source Meaning | Mechanical Meaning | Stable Machine Concept | Locator | Ambiguous? |
|-------------|---------------|-------------------|----------------------|---------|------------|
| Raça | Birth identity classification (Hominídea/Metis/Lupina) | Immutable creation identity | `WerewolfRaceIdentifiers` / `Race` field | Lines 865, 871, 494-499 | No |
| Hominídea | Birth identity (born of human mother) | Homid birth identity | `WerewolfRaceIdentifiers.Homid` | Lines 500-510 | No |
| Impuro / Metis | Birth identity (born of two Garou) | Metis birth identity | `WerewolfRaceIdentifiers.Metis` | Lines 512-538, 865 | Yes — "Impuro" is derogatory in-world synonym; "Metis" is canonical technical key |
| Lupina | Birth identity (born of wolf mother) | Lupus birth identity | `WerewolfRaceIdentifiers.Lupus` | Lines 539-548 | No |
| Forma Racial | Primary natural form associated with birth identity | Birth form (not mutable in current slice) | Not implemented; deferred to 012 | Lines 72-77 | No |
| Hominídea (form) | Human pure form | Shapeshift form | Not implemented; deferred to 012 | Line 73 | Potentially ambiguous with birth identity Homid |
| Lupina (form) | Wolf pure form | Shapeshift form | Not implemented; deferred to 012 | Line 74 | Potentially ambiguous with birth identity Lupus |
| Crinos | Battle form / Metis birth form | Shapeshift form | Not implemented; deferred to 012 | Lines 75-78 | No current conflation |
| Glabro | Intermediate form | Shapeshift form | Not implemented; deferred to 012 | Line 79 | No current conflation |
| Hispo | Intermediate form | Shapeshift form | Not implemented; deferred to 012 | Line 79 | No current conflation |

## 5. Canonical Technical Vocabulary

**Birth Identity (immutable):**
- Concept: The immutable biological/spiritual origin category of the character
- Code field: `Race` in `WerewolfInitializedCharacterState`, `WerewolfCharacterSnapshot`, `WerewolfRuntimeCharacterState`
- Machine keys: `homid`, `metis`, `lupus`
- Identifier class: `WerewolfRaceIdentifiers`
- Source term: "Raça" / "Origem de Nascimento"

**Current Form (mutable, deferred to 012):**
- Concept: The current shapeshift form of the character
- Code: Not implemented
- Machine keys: Not defined
- Source terms: "Forma Racial", "Hominídea", "Glabro", "Crinos", "Hispo", "Lupina"

**Key distinction:**
- `Race` in the current codebase ALWAYS refers to birth identity
- No current-form implementation exists
- No conflation is possible because forms are not implemented
- Future form implementation must use distinct keys and contracts

## 6. Birth Identity vs Current Form

**Audit result:** No conflation found.

Current code paths checked:
- `WerewolfInitializedCharacterState.Race` — birth identity only
- `WerewolfCharacterSnapshot.Race` — birth identity only
- `WerewolfRuntimeCharacterState` — no form field; birth identity preserved via snapshot
- `WerewolfTribeEligibilityService` — uses `race` parameter as birth identity
- `WerewolfMetisDeformitySelectionService` — uses `request.Draft.Race` as birth identity
- `WerewolfResourceRankInitializationService` — uses `race` as birth identity
- `WerewolfInitialGiftSelectionService` — uses `WerewolfRaceIdentifiers` as birth identity
- Localization — no form-related keys exist yet

**Red Talons eligibility:** Source says "Tribo composta exclusivamente por lobisomens de raça Lupina" (line 733). Implementation checks `WerewolfRaceIdentifiers.Lupus` — correct birth-identity enforcement.

**Metis deformity eligibility:** Source says Metis are born of two Garou and have mandatory deformities. Implementation checks `WerewolfRaceIdentifiers.Metis` — correct birth-identity enforcement.

## 7. Machine Key Decision

**Decision:** Preserve existing keys. No rename.

| Current Key | Status | Reason |
|-------------|--------|--------|
| `WerewolfRaceIdentifiers.Homid` | Stable | Semantically correct; maps to source Hominídea birth identity |
| `WerewolfRaceIdentifiers.Metis` | Stable | Canonical technical key per A-017; maps to source Impura (Metis) birth identity |
| `WerewolfRaceIdentifiers.Lupus` | Stable | Semantically correct; maps to source Lupina birth identity |
| `Race` field name | Preserved | Maps to source "Raça" (birth identity); no mechanical ambiguity in current code |

**Compatibility:** No breaking changes. All public identifiers remain unchanged.

## 8. Localization

**Added keys:**

English (`en/current-slice.json`):
```json
"character.race.homid.display-name": "Homid"
"character.race.metis.display-name": "Metis"
"character.race.lupus.display-name": "Lupus"
```

Portuguese (`pt-BR/current-slice.json`):
```json
"character.race.homid.display-name": "Hominídeo"
"character.race.metis.display-name": "Metis"
"character.race.lupus.display-name": "Lupino"
```

**Localization policy:**
- Machine keys remain language-neutral (`homid`, `metis`, `lupus`)
- `metis` is used in both locales as the neutral display label
- The derogatory in-world term "Impuro" is not used as a default UI label in either locale
- If narrative context requires "Impuro", it should be handled through separate narrative/localization fields, not the birth-identity display name

## 9. Tests by Project

**Project:** `Chronicle.RuleSets.Werewolf.Tests`

**New file:** `WerewolfBirthIdentitySemanticsTests.cs` (7 tests)

1. `HomidBirthIdentityIsStableAndDistinctFromMetisAndLupus` — verifies stable keys and uniqueness
2. `BirthIdentityKeysAreLanguageNeutralAndWhitespaceFree` — verifies canonical format
3. `MetisIsTheCanonicalTechnicalKeyNotImpuro` — verifies A-017 resolution
4. `BirthIdentityIsNotCurrentForm` — verifies no form conflation in draft state
5. `RedTalonsEligibilityUsesBirthIdentityNotCurrentForm` — verifies Red Talons rule uses birth identity
6. `HomidCannotAccessRedTalonsViaBirthIdentity` — verifies Red Talons restriction
7. `MetisDeformityEligibilityUsesBirthIdentity` — verifies Metis deformity uses birth identity
8. `HomidCannotAccessMetisDeformityViaBirthIdentity` — verifies Metis-only restriction
9. `BirthIdentityPreservesThroughSnapshotToRuntimeState` — verifies snapshot-to-runtime preservation

**Existing tests verified:**
- `WerewolfRaceSelectionTests` — all pass; birth identity semantics confirmed
- `WerewolfTribeEligibilityTests` — Red Talons Lupus-only rule correct
- `WerewolfMetisDeformitySelectionTests` — Metis-only rule correct
- `WerewolfCharacterCompletionTests` — completion validation correct

## 10. Affected Mechanics

| Mechanic | Birth Identity Usage | Status |
|----------|---------------------|--------|
| Race selection | `WerewolfRaceIdentifiers` | Correct |
| Tribe eligibility (Red Talons) | `WerewolfTribeEligibilityService.CheckEligibility` | Correct |
| Metis deformity eligibility | `WerewolfMetisDeformitySelectionService` | Correct |
| Resource initialization | `WerewolfResourceRankInitializationService` | Correct |
| Initial Race Gifts | `WerewolfInitialGiftSelectionService` | Correct |
| Character Completion | `WerewolfCharacterCompletionOperation` | Correct |
| Runtime state | `WerewolfRuntimeCharacterState.FromSnapshot` | Correct |

## 11. Completeness Impact

**Mechanically complete:** 24/68 (unchanged)
**Current-slice executable:** 35/68 (unchanged)

**Reason:** This package resolves terminology ambiguities (A-017, A-018) and adds localization. No mechanical rules were blocked by these ambiguities. The affected domains (Race selection, Metis deformity, Tribe selection) were already mechanically complete or executable before terminology stabilization.

## 12. Files Changed

### Source files (modified)
- `rule-sets/Chronicle.RuleSets.Werewolf/Localization/en/current-slice.json`
- `rule-sets/Chronicle.RuleSets.Werewolf/Localization/pt-BR/current-slice.json`

### Test files (new)
- `rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfBirthIdentitySemanticsTests.cs`

### Documentation files (modified)
- `docs/reviews/werewolf-rule-set-completeness/completeness-report.md`

### Evidence file (new)
- `docs/reviews/werewolf-rule-set-completeness/completion-evidence/RULESET-COMPLETION-011.md`

## 13. Full-Solution Total

**Baseline before 010:** 861

**After 010:** 883

**After 011 (including this audit):** 892

| Project | After 010 | After 011 |
|---------|-----------|-----------|
| Domain | 1 | 1 |
| Contracts | 8 | 8 |
| Application | 9 | 9 |
| PackageValidator | 8 | 8 |
| Persistence | 1 | 1 |
| Werewolf | 833 | 842 |
| Infrastructure | 12 | 12 |
| Architecture | 11 | 11 |
| **Total** | **883** | **892** |

**Werewolf delta:** +9 tests (new `WerewolfBirthIdentitySemanticsTests` with 9 terminology-semantic tests)

## 14. Package Validator Result

Valid. 46 files inventoried, 0 findings.

## 15. Matrix Integrity

Valid JSON. Verified with Python `json.load()`.

## 16. Localization Integrity

Both `en/current-slice.json` and `pt-BR/current-slice.json` are valid JSON. Added 3 birth-identity keys per locale (96 keys total per locale).

## 17. git diff --check

Clean.

## 18. git status --short

```
 M docs/reviews/werewolf-rule-set-completeness/completeness-report.md
 M rule-sets/Chronicle.RuleSets.Werewolf/Localization/en/current-slice.json
 M rule-sets/Chronicle.RuleSets.Werewolf/Localization/pt-BR/current-slice.json
?? .kilo/
?? docs/reviews/werewolf-rule-set-completeness/completion-evidence/RULESET-COMPLETION-011.md
?? rule-sets/Chronicle.RuleSets.Werewolf.Tests/WerewolfBirthIdentitySemanticsTests.cs
```

`.kilo/` remains untracked.

## 19. Remaining Blockers

**0 ownerless blockers.**

**Deferred dependencies (all have exact owners):**
- Forms system implementation → RULESET-COMPLETION-012
- Full Race Gift catalog expansion → RULESET-COMPLETION-012
- Runtime effect enforcement → RULESET-COMPLETION-012
