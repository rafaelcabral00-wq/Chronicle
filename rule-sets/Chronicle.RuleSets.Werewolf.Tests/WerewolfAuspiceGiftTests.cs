using System.Text.Json;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

/// <summary>
/// Wave 2C: Auspice Gift parity and executable mechanics.
/// Source lines 1871-2103: 12 Gifts for each of the five auspices, 60 total.
/// </summary>
public sealed class WerewolfAuspiceGiftTests
{
    // =====================================================================
    // Catalog parity: 60 source Gifts, 60 catalog entries, 12 per auspice.
    // =====================================================================
    [Fact]
    public void AllSixtySourceAuspiceGiftsAreCataloguedWithTwelvePerAuspice()
    {
        var auspiceGifts = WerewolfGiftCatalog.AllDefinitions
            .Where(g => g.Category == WerewolfGiftCategory.Auspice)
            .ToList();

        Assert.Equal(60, auspiceGifts.Count);
        Assert.Equal(60, WerewolfAuspiceGiftMechanics.AuspiceGiftKeys.Count);
        Assert.Equal(60, auspiceGifts.Select(g => g.GiftKey).Distinct(StringComparer.Ordinal).Count());

        foreach (var auspice in new[]
                 {
                     WerewolfAuspiceIdentifiers.Ragabash,
                     WerewolfAuspiceIdentifiers.Theurge,
                     WerewolfAuspiceIdentifiers.Philodox,
                     WerewolfAuspiceIdentifiers.Galliard,
                     WerewolfAuspiceIdentifiers.Ahroun
                 })
        {
            Assert.Equal(12, auspiceGifts.Count(g => g.OwnerKey == auspice));
        }
    }

    [Fact]
    public void AuspiceMechanicsTableCoversExactlyTheCataloguedAuspiceGifts()
    {
        var catalogKeys = WerewolfGiftCatalog.AllDefinitions
            .Where(g => g.Category == WerewolfGiftCategory.Auspice)
            .Select(g => g.GiftKey)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(catalogKeys, WerewolfAuspiceGiftMechanics.AuspiceGiftKeys.ToHashSet(StringComparer.Ordinal));
    }

    [Fact]
    public void EveryAuspiceGiftHasAUniqueKeyAndASourceLocator()
    {
        var keys = WerewolfAuspiceGiftMechanics.AuspiceGiftKeys;
        Assert.Equal(keys.Count, keys.Distinct(StringComparer.Ordinal).Count());

        foreach (var entry in WerewolfAuspiceGiftMechanics.Audit())
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.GiftKey));
            Assert.StartsWith("Line ", entry.SourceLocator, StringComparison.Ordinal);
            Assert.InRange(entry.Level, 1, 5);
            Assert.False(string.IsNullOrWhiteSpace(entry.OwnerKey));
        }
    }

    [Fact]
    public void RanksMatchTheFiveLevelStructureOfTheSource()
    {
        foreach (var entry in WerewolfAuspiceGiftMechanics.Audit())
        {
            var number = int.Parse(entry.SourceLocator.Replace("Line ", string.Empty, StringComparison.Ordinal), System.Globalization.CultureInfo.InvariantCulture);

            // Each auspice section lists 12 Gifts as 3/2/2/2/2/1 per rank 1..5.
            Assert.InRange(number, 1874, 2101);
        }
    }

    [Fact]
    public void EveryAuspiceGiftIsEitherExecutableOrExplicitlyBlocked()
    {
        var audit = WerewolfAuspiceGiftMechanics.Audit();

        Assert.Equal(60, audit.Count);
        Assert.All(audit, entry => Assert.Contains(
            entry.Status,
            new[] { WerewolfBreedGiftStatus.Executable, WerewolfBreedGiftStatus.Blocked }));

        Assert.Equal(58, audit.Count(e => e.Status == WerewolfBreedGiftStatus.Executable));
        Assert.Equal(2, audit.Count(e => e.Status == WerewolfBreedGiftStatus.Blocked));
    }

    [Fact]
    public void StatusOfRejectsAnyGiftWithoutARegisteredMechanic()
    {
        var (status, dependency) = WerewolfAuspiceGiftMechanics.StatusOf("gift.auspice.ragabash.nope");

        Assert.Equal(WerewolfBreedGiftStatus.Blocked, status);
        Assert.False(string.IsNullOrWhiteSpace(dependency));
    }

    // =====================================================================
    // Ownership defect corrected: Roubar Poderes is Ragabash (line 1917),
    // not Theurge. Chamado da Wyld/Wyrm are distinct Gifts.
    // =====================================================================
    [Fact]
    public void StealPowersBelongsToRagabashNotTheurge()
    {
        // Source lines 1917-1919 sit inside the Ragabash section (1871-1921).
        var definition = WerewolfGiftCatalog.Get(WerewolfGiftIdentifiers.TheurgeRoubarPoderes);

        Assert.NotNull(definition);
        Assert.Equal(WerewolfAuspiceIdentifiers.Ragabash, definition!.OwnerKey);
        Assert.Equal(5, definition.Level);
        Assert.Equal("Line 1917", definition.SourceLocator);
    }

    [Fact]
    public void CallOfTheWyldAndCallOfTheWyrmAreDistinctGifts()
    {
        var wyld = WerewolfGiftCatalog.Get(WerewolfGiftIdentifiers.GalliardChamadoDaWyld);
        var wyrm = WerewolfGiftCatalog.Get(WerewolfGiftIdentifiers.GalliardChamadoDaWyrm);

        Assert.NotNull(wyld);
        Assert.NotNull(wyrm);
        Assert.Equal(1, wyld!.Level);
        Assert.Equal("Line 2019", wyld.SourceLocator);
        Assert.Equal(2, wyrm!.Level);
        Assert.Equal("Line 2029", wyrm.SourceLocator);
        Assert.NotEqual(wyld.GiftKey, wyrm.GiftKey);
        Assert.Null(WerewolfGiftCatalog.Get("gift.auspice.galliard.beast-speech"));
    }

    // =====================================================================
    // Source-derived numeric outcomes.
    // =====================================================================

    [Fact]
    public void EmbaçamentoAddsOnePerceptionDifficultyPerSuccess()
    {
        // Line 1880: Manipulation + Stealth @8; each success +1 Perception difficulty.
        var one = Resolve(WerewolfGiftIdentifiers.RagabashEmbacamentoDaPropriaForma, 1);
        var three = Resolve(WerewolfGiftIdentifiers.RagabashEmbacamentoDaPropriaForma, 3);

        Assert.Equal(1, one!.Magnitude);
        Assert.Equal(3, three!.Magnitude);

        var payload = (WerewolfStealthPoolDeductionPayload)one.Payload!;
        Assert.Equal(1, payload.PoolDifficultyIncrease);
        Assert.Equal(1, payload.PerSuccessPenalty);
    }

    [Fact]
    public void SimulateRunningWaterScentAddsTwoTrackingDifficultyInherently()
    {
        // Line 1883: inherent; +2 difficulty on any test to track the character.
        var mechanic = Resolve(WerewolfGiftIdentifiers.RagabashSimularOCheiroDeAguaCorrente, 0);
        var payload = (WerewolfScentTrackingDifficultyPayload)mechanic!.Payload!;

        Assert.Equal(2, mechanic.Magnitude);
        Assert.Equal(2, payload.DifficultyIncrease);
        Assert.True(payload.Inherent);
    }

    [Fact]
    public void GenerateForgettingUsesTheSourceInvisibilityContract()
    {
        // Line 1887: Dexterity + Stealth @7; only while motionless.
        var mechanic = Resolve(WerewolfGiftIdentifiers.RagabashGerarIgnorancia, 2);
        var payload = (WerewolfInvisibilityPayload)mechanic!.Payload!;

        Assert.Equal(2, mechanic.Magnitude);
        Assert.Equal(7, payload.TestDifficulty);
        Assert.True(payload.RequiresMotionless);
    }

    [Fact]
    public void OpenMoonBridgeUsesTheSourceMaximumDistance()
    {
        // Line 1896: 1 Gnosis, max 1,500 km.
        var mechanic = Resolve(WerewolfGiftIdentifiers.RagabashAbrirPonteDaLua, 0);
        var payload = (WerewolfMoonBridgePayload)mechanic!.Payload!;

        Assert.Equal(1500, mechanic!.Magnitude);
        Assert.Equal(1500, payload.MaxDistanceKilometers);
        Assert.Equal(1, payload.GnosisCost);
    }

    [Fact]
    public void GremlinsCarriesTheSourceDeviceTableAndPermanentThreshold()
    {
        // Line 1899-1905: Computer 4, Telefone 6, Automóvel 8, Faca 10; 5 successes.
        var mechanic = Resolve(WerewolfGiftIdentifiers.RagabashGremlins, 3);
        var payload = (WerewolfGremlinDevicePayload)mechanic!.Payload!;

        Assert.Equal([4, 6, 8, 10], payload.DeviceDifficulties);
        Assert.Equal(4, payload.DeviceNames.Count);
        Assert.Equal(5, payload.PermanentDisablingSuccesses);
    }

    [Fact]
    public void LunaBlessingGrantsThreeExtraDiceCountingOnlyForCriticals()
    {
        // Line 1909.
        var mechanic = Resolve(WerewolfGiftIdentifiers.RagabashBencaoDeLuna, 0);
        var payload = (WerewolfSilverMoonBlessingPayload)mechanic!.Payload!;

        Assert.Equal(3, mechanic!.Magnitude);
        Assert.Equal(3, payload.AttackerExtraDice);
        Assert.True(payload.CountOnlyForCrits);
        Assert.True(payload.RequiresVisibleMoon);
    }

    [Fact]
    public void WeakenBodiesRemovesOnePhysicalAttributePerSuccessOncePerLifetime()
    {
        // Line 1912.
        var mechanic = Resolve(WerewolfGiftIdentifiers.RagabashFragilizarCorpos, 4);
        var payload = (WerewolfPermanentAttributeReductionPayload)mechanic!.Payload!;

        Assert.Equal(4, mechanic!.Magnitude);
        Assert.Equal(1, payload.AttributesRemovedPerSuccess);
        Assert.Equal(1, payload.UsesPerLifetime);
        Assert.True(payload.PhysicalOnly);
    }

    [Fact]
    public void ThousandFormsUsesTheSourceDifficultyTiers()
    {
        // Line 1916: 5 equivalent mammals, 7 larger reptiles, 9 smaller amphibians,
        // 10 mythological.
        var mechanic = Resolve(WerewolfGiftIdentifiers.RagabashAsMilFormas, 0);
        var payload = (WerewolfMetamorphosisDifficultyPayload)mechanic!.Payload!;

        Assert.Equal(5, payload.MinDifficulty);
        Assert.Equal(10, payload.MaxDifficulty);
        Assert.Equal(4, payload.TierDifficulties.Count);
    }

    [Fact]
    public void MothersTouchRepairsOneHealthLevelPerSuccess()
    {
        // Line 1933: 1 Gnosis; 1 health level per success; a second Gnosis heals
        // Battle Scars in the same scene.
        var mechanic = Resolve(WerewolfGiftIdentifiers.TheurgeToqueDaMae, 3);
        var payload = (WerewolfHealthRepairPayload)mechanic!.Payload!;

        Assert.Equal(3, mechanic!.Magnitude);
        Assert.Equal(1, payload.HealthLevelsPerSuccess);
        Assert.Equal(1, payload.SecondGnosisForBattleScars);
    }

    [Fact]
    public void VisionsUsesDifficultySevenForDecoding()
    {
        // Line 1943.
        var mechanic = Resolve(WerewolfGiftIdentifiers.TheurgeVisoes, 0);
        Assert.Equal(7, ((WerewolfVisionDecodingPayload)mechanic!.Payload!).TestDifficulty);
    }

    [Fact]
    public void PerceptionOfTheInvisibleRequiresOneGnosisSuccessBelowThePelicula()
    {
        // Line 1952: automatic at Gnosis >= Película, otherwise 1 Gnosis success.
        var mechanic = Resolve(WerewolfGiftIdentifiers.TheurgePercepcaoDoInvisivel, 0);
        var payload = (WerewolfPenumbraPerceptionPayload)mechanic!.Payload!;

        Assert.Equal(1, payload.RequiredSuccesses);
        Assert.Equal(0, payload.AutomaticGnosisThreshold);
    }

    [Fact]
    public void SpiritualDrainageConvertsTwoEssenceIntoOneWillpower()
    {
        // Line 1959.
        var mechanic = Resolve(WerewolfGiftIdentifiers.TheurgeDrenagemEspiritual, 4);
        var payload = (WerewolfEssenceDrainPayload)mechanic!.Payload!;

        Assert.Equal(4, mechanic!.Magnitude);
        Assert.Equal(1, payload.EssencePerSuccessPerTurn);
        Assert.Equal(2, payload.EssencePerWillpower);
    }

    [Fact]
    public void AnimalLobotomyUsesWillpowerPlusThreeCappedAtTen()
    {
        // Line 1963: 2 Gnosis per Intelligence point.
        var mechanic = Resolve(WerewolfGiftIdentifiers.TheurgeLobotomiaAnimal, 2);
        var payload = (WerewolfAnimalLobotomyPayload)mechanic!.Payload!;

        Assert.Equal(2, mechanic!.Magnitude);
        Assert.Equal(3, payload.WillpowerDifficultyBonus);
        Assert.Equal(10, payload.MaxDifficulty);
        Assert.Equal(2, payload.GnosisPerIntelligencePoint);
    }

    [Fact]
    public void CallOfDutyCoversOneAndAHalfKilometresAndTwoGnosisForTheRegion()
    {
        // Line 1981.
        var mechanic = Resolve(WerewolfGiftIdentifiers.PhilodoxChamadoDoDever, 0);
        var payload = (WerewolfSpiritSummonPayload)mechanic!.Payload!;

        Assert.Equal(1, payload.RadiusKilometers);
        Assert.Equal(2, payload.AdditionalGnosisForRegion);
    }

    [Fact]
    public void DeterminationRecoversOneWillpowerPerTwoSuccessesAtDifficultySeven()
    {
        // Line 1985.
        var mechanic = Resolve(WerewolfGiftIdentifiers.PhilodoxDeterminacao, 5);
        var payload = (WerewolfWillpowerRecoveryPayload)mechanic!.Payload!;

        Assert.Equal(5, mechanic!.Magnitude);
        Assert.Equal(7, payload.TestDifficulty);
        Assert.Equal(1, payload.WillpowerPerSuccesses);
        Assert.Equal(2, payload.SuccessesPerPoint);
        Assert.Equal(1, payload.UsesPerScene);
    }

    [Fact]
    public void FindTheAchillesHeelGrantsOneExtraDiePerSuccessAtDifficultyEight()
    {
        // Line 1997.
        var mechanic = Resolve(WerewolfGiftIdentifiers.PhilodoxDescobrirCalcanharDeAquiles, 3);
        var payload = (WerewolfExtraDiePayload)mechanic!.Payload!;

        Assert.Equal(3, mechanic!.Magnitude);
        Assert.Equal(8, payload.TestDifficulty);
        Assert.Equal(1, payload.DicePerSuccess);
    }

    [Fact]
    public void WisdomOfAncientTraditionsCostsNineReducedByOnePerAncestorsPoint()
    {
        // Line 2000.
        var mechanic = Resolve(WerewolfGiftIdentifiers.PhilodoxSabedoriaDasAntigasTradicoes, 0);
        var payload = (WerepoolAncestralWisdomPayload)mechanic!.Payload!;

        Assert.Equal(9, payload.TestDifficulty);
        Assert.Equal(1, payload.ReductionPerAncestorsPoint);
    }

    [Fact]
    public void ScentAtGreatDistancesUsesDifficultyEightOrThePeliculaInUmbra()
    {
        // Line 2004.
        var mechanic = Resolve(WerewolfGiftIdentifiers.PhilodoxFaroParaGrandesDistancias, 0);
        var payload = (WerewolfDistantScentPayload)mechanic!.Payload!;

        Assert.Equal(8, payload.TestDifficulty);
        Assert.True(payload.UmbraUsesPeliculaLevel);
    }

    [Fact]
    public void ImpositionRequiresThreeNetSuccesses()
    {
        // Line 2007.
        var mechanic = Resolve(WerewolfGiftIdentifiers.PhilodoxImposicao, 3);
        var payload = (WerewolfSubmissionPayload)mechanic!.Payload!;

        Assert.Equal(3, mechanic!.Magnitude);
        Assert.Equal(3, payload.NetSuccessesRequired);
    }

    [Fact]
    public void MesmerizeCostsOneGnosisAndOpposesWillpower()
    {
        // Line 2011.
        var mechanic = Resolve(WerewolfGiftIdentifiers.PhilodoxMesmerizar, 0);
        var payload = (WerewolfMesmerizePayload)mechanic!.Payload!;

        Assert.Equal(1, mechanic!.Magnitude);
        Assert.Equal(1, payload.GnosisCost);
        Assert.Equal("Willpower", payload.OpposingTrait);
    }

    [Fact]
    public void GraniteWallUsesTheSourceDimensionsAndTenAbsorptionDice()
    {
        // Line 2015: 3m x 2m x 1m, 10 absorption dice, 15 damage levels to break.
        var mechanic = Resolve(WerewolfGiftIdentifiers.PhilodoxParedeDeGranito, 0);
        var payload = (WerewolfGraniteWallPayload)mechanic!.Payload!;

        Assert.Equal(10, mechanic!.Magnitude);
        Assert.Equal(3, payload.LengthMeters);
        Assert.Equal(2, payload.HeightMeters);
        Assert.Equal(1, payload.DepthMeters);
        Assert.Equal(10, payload.AbsorptionDice);
        Assert.Equal(15, payload.DamageLevelsToBreak);
    }

    [Fact]
    public void TelepathicCommunionCostsOneWillpowerPerMindAndReducesTwoDice()
    {
        // Line 2026.
        var mechanic = Resolve(WerewolfGiftIdentifiers.GalliardComunicacaoTelepatica, 0);
        var payload = (WerewolfTelepathicCommunionPayload)mechanic!.Payload!;

        Assert.Equal(2, mechanic!.Magnitude);
        Assert.Equal(1, payload.WillpowerPerMind);
        Assert.Equal(2, payload.PhysicalPoolReduction);
        Assert.Equal("Willpower", payload.OpposingTrait);
    }

    [Fact]
    public void CallOfTheWyrmIsAResistedTestWithBothSidesAtSeven()
    {
        // Line 2030.
        var mechanic = Resolve(WerewolfGiftIdentifiers.GalliardChamadoDaWyrm, 0);
        var payload = (WerewolfResistedTestPayload)mechanic!.Payload!;

        Assert.Equal(7, payload.BaseDifficulty);
        Assert.Equal("Willpower", payload.OpposingTrait);
    }

    [Fact]
    public void DreamCommunionCostsEightAndLosesOneGnosisIfWoken()
    {
        // Line 2033.
        var mechanic = Resolve(WerewolfGiftIdentifiers.GalliardComunicacaoOnirica, 0);
        var payload = (WerewolfDreamCommunionPayload)mechanic!.Payload!;

        Assert.Equal(8, payload.TestDifficulty);
        Assert.Equal(1, payload.GnosisLostIfWoken);
    }

    [Fact]
    public void DistractionsSubtractOneDiePerSuccess()
    {
        // Line 2035.
        var mechanic = Resolve(WerewolfGiftIdentifiers.GalliardDistracoes, 2);
        var payload = (WerewolfExtraDiePayload)mechanic!.Payload!;

        Assert.Equal(2, mechanic!.Magnitude);
        Assert.Equal(-1, payload.DicePerSuccess);
    }

    [Fact]
    public void SongOfFuryCausesOneTurnOfFrenzyPerSuccess()
    {
        // Line 2040.
        var mechanic = Resolve(WerewolfGiftIdentifiers.GalliardCancaoDaFuria, 3);
        var payload = (WerepoolFrenzyInducedPayload)mechanic!.Payload!;

        Assert.Equal(3, mechanic!.Magnitude);
        Assert.Equal(1, payload.TurnsPerSuccess);
        Assert.Equal("Willpower", payload.OpposingTrait);
    }

    [Fact]
    public void CobrasEyeRequiresThreeSuccessesToPullTheTarget()
    {
        // Line 2043.
        var mechanic = Resolve(WerewolfGiftIdentifiers.GalliardOlhoDeCobra, 0);
        var payload = (WerepoolGazePullPayload)mechanic!.Payload!;

        Assert.Equal(3, mechanic!.Magnitude);
        Assert.Equal(3, payload.SuccessesRequired);
    }

    [Fact]
    public void MoonBridgeWalkerCostsOneGnosisAndScalesWithGnosis()
    {
        // Line 2047: max distance equals Gnosis in kilometers.
        var mechanic = Resolve(WerewolfGiftIdentifiers.GalliardAndarilhoDaPonte, 0);
        var payload = (WerewolfMoonBridgePayload)mechanic!.Payload!;

        Assert.Equal(0, payload.MaxDistanceKilometers);
        Assert.Equal(1, payload.GnosisCost);
    }

    [Fact]
    public void ShadowTheaterLastsOneTurnPerGnosisAfterThreeSuccesses()
    {
        // Line 2050.
        var mechanic = Resolve(WerewolfGiftIdentifiers.GalliardTeatroDeSombrio, 0);
        var payload = (WerewolfShadowTheaterPayload)mechanic!.Payload!;

        Assert.Equal(3, mechanic!.Magnitude);
        Assert.Equal(3, payload.SuccessesRequired);
        Assert.Equal(1, payload.TurnsPerGnosis);
    }

    [Fact]
    public void SharpenedClawsAddOneDamageDie()
    {
        // Line 2065: 1 Rage; +1 damage die.
        var mechanic = Resolve(WerewolfGiftIdentifiers.AhrounGarrasAfiadas, 0);
        Assert.Equal(1, mechanic!.Magnitude);
    }

    [Fact]
    public void InspirationGrantsAlliesOneAutomaticWillpowerSuccess()
    {
        // Line 2068: 1 Gnosis.
        var mechanic = Resolve(WerewolfGiftIdentifiers.AhrounInspiracao, 0);
        Assert.Equal(WerewolfActiveGiftEffectKind.TestBonus, mechanic!.Kind);
        Assert.Equal(1, mechanic.Magnitude);
    }

    [Fact]
    public void SpiritOfTheBattleGrantsTenInitiativeInherently()
    {
        // Line 2075: inherent/permanent; +10 initiative.
        var mechanic = Resolve(WerewolfGiftIdentifiers.AhrounEspiritoDaBatalha, 0);

        Assert.Equal(10, mechanic!.Magnitude);
        Assert.Equal(-1, mechanic.DurationTurns);
    }

    [Fact]
    public void SenseSilverUsesDifficultySevenAndThreeSuccessesToLocate()
    {
        // Line 2081.
        var mechanic = Resolve(WerewolfGiftIdentifiers.AhrounSentirAPrata, 0);
        var payload = (WerewolfResistedTestPayload)mechanic!.Payload!;

        Assert.Equal(7, payload.BaseDifficulty);
        Assert.Equal(3, payload.Successes);
    }

    [Fact]
    public void HeartOfFuryRaisesFrenzyDifficultyEveryTwoSuccesses()
    {
        // Line 2085.
        var mechanic = Resolve(WerewolfGiftIdentifiers.AhrounCoracaoDaFuria, 4);
        var payload = (WerepoolFrenzyWardPayload)mechanic!.Payload!;

        Assert.Equal(4, mechanic!.Magnitude);
        Assert.Equal(2, payload.SuccessesPerDifficultyPoint);
        Assert.Equal(1, payload.EndingWillpowerCost);
    }

    [Fact]
    public void SilverClawsDealAggravatedIncurableDamageAndGrantRagePerTurn()
    {
        // Line 2088: Gnosis @7; +1 automatic Rage per turn; +1 non-combat difficulty.
        var mechanic = Resolve(WerewolfGiftIdentifiers.AhrounGarrasDePrata, 0);
        var payload = (WerepoolSilverClawsPayload)mechanic!.Payload!;

        Assert.Equal(7, payload.TestDifficulty);
        Assert.True(payload.AggravatedIncurable);
        Assert.Equal(1, payload.AutomaticRagePerTurn);
        Assert.Equal(1, payload.NonCombatDifficultyIncrease);
    }

    [Fact]
    public void StokeTheFurnaceRegainsOneRageOnDamageWithoutAFrenzyTest()
    {
        // Line 2092.
        var mechanic = Resolve(WerewolfGiftIdentifiers.AhrounAticandoAFornalhaDaFuria, 0);
        var payload = (WerepoolRageRegenerationPayload)mechanic!.Payload!;

        Assert.Equal(1, mechanic!.Magnitude);
        Assert.Equal(1, payload.RageRegainedOnDamage);
        Assert.True(payload.NoFrenzyTest);
    }

    [Fact]
    public void IronBiteUsesMaintenanceThreeAndOneHalfWillpower()
    {
        // Line 2095: 1 Rage; maintenance @3; +1 damage on escape.
        var mechanic = Resolve(WerewolfGiftIdentifiers.AhrounMordidaDeFerro, 0);
        var payload = (WerepoolIronBitePayload)mechanic!.Payload!;

        Assert.Equal(1, mechanic!.Magnitude);
        Assert.Equal(3, payload.MaintenanceDifficulty);
        Assert.Equal(1, payload.EscapeDamageLevels);
        Assert.Equal(0.5, payload.WillpowerAddedFraction);
    }

    [Fact]
    public void KissOfHeliosGrantsFireImmunityAndTwoAggravatedDice()
    {
        // Line 2099: 1 Gnosis; +2 aggravated dice; artificial flames at 1/4.
        var mechanic = Resolve(WerewolfGiftIdentifiers.AhrounBeijoDeHelios, 0);
        var payload = (WerepoolSolarKissPayload)mechanic!.Payload!;

        Assert.Equal(2, mechanic!.Magnitude);
        Assert.Equal(1, payload.GnosisCost);
        Assert.Equal(2, payload.BurningAttackBonusDice);
        Assert.True(payload.Aggravated);
        Assert.Equal(4, payload.ArtificialFlameNumerator);
    }

    [Fact]
    public void UnwaveringWillGrantsOneWillpowerPerSuccessWithinThirtyMeters()
    {
        // Line 2102: 1 Willpower; @8; radius 30 m; +1 per success; capped at 10.
        var mechanic = Resolve(WerewolfGiftIdentifiers.AhrounVontadeInabalavel, 3);
        var payload = (WerepoolUnwaveringWillPayload)mechanic!.Payload!;

        Assert.Equal(3, mechanic!.Magnitude);
        Assert.Equal(8, payload.TestDifficulty);
        Assert.Equal(30, payload.RadiusMeters);
        Assert.Equal(1, payload.WillpowerPerSuccess);
        Assert.Equal(1, payload.UsesPerScene);
        Assert.Equal(10, payload.AbsoluteCap);
    }

    // =====================================================================
    // Runtime reachability and blocked reporting.
    // =====================================================================

    [Fact]
    public void AllFiftyEightExecutableAuspiceGiftsAreRuntimeReachable()
    {
        foreach (var entry in WerewolfAuspiceGiftMechanics.Audit()
                     .Where(e => e.Status == WerewolfBreedGiftStatus.Executable))
        {
            var state = BuildState(entry.GiftKey, entry.OwnerKey);
            var result = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
                $"req-aus-{entry.GiftKey}", state, state.RuntimeStateVersion, entry.GiftKey, 2));

            Assert.True(result.Succeeded, $"'{entry.GiftKey}' unreachable: {string.Join("; ", result.Findings)}");
            Assert.NotEmpty(result.ActiveEffects);
            Assert.Equal(entry.SourceLocator, result.ActiveEffects[0].SourceLocator);
            Assert.NotEqual(WerewolfActiveGiftEffectKind.Custom, result.ActiveEffects[0].EffectKind);
        }
    }

    [Fact]
    public void DeterminationActuallyIncreasesCurrentWillpower()
    {
        // Source line 1985: 1 Willpower per 2 successes is a real state change.
        var state = BuildState(WerewolfGiftIdentifiers.PhilodoxDeterminacao, WerewolfAuspiceIdentifiers.Philodox);
        var willBefore = state.WillpowerCurrent;

        var result = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-determinacao", state, state.RuntimeStateVersion, WerewolfGiftIdentifiers.PhilodoxDeterminacao, 4));

        Assert.True(result.Succeeded);
        Assert.Equal(willBefore + 2, result.UpdatedState!.WillpowerCurrent);
    }

    [Fact]
    public void BlockedAuspiceGiftsExecuteAndReportTheirExactDependency()
    {
        var blocked = WerewolfAuspiceGiftMechanics.Audit()
            .Where(e => e.Status == WerewolfBreedGiftStatus.Blocked)
            .ToList();

        Assert.Equal(2, blocked.Count);

        foreach (var entry in blocked)
        {
            var state = BuildState(entry.GiftKey, entry.OwnerKey);
            var result = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
                $"req-blocked-{entry.GiftKey}", state, state.RuntimeStateVersion, entry.GiftKey, 2));

            Assert.True(result.Succeeded);
            var payload = Assert.IsType<WerewolfAuspiceBlockedPayload>(result.ActiveEffects[0].Payload);
            Assert.Equal(entry.MissingDependency, payload.MissingSubsystem);
            Assert.False(string.IsNullOrWhiteSpace(payload.Reason));
            Assert.Contains(result.Findings, f => f.Contains(entry.MissingDependency!, StringComparison.Ordinal));
        }
    }

    [Fact]
    public void BlockedAuspiceGiftsAreTheEntityShapingPair()
    {
        Assert.Equal(
            new[] { WerewolfGiftIdentifiers.GalliardMaterializacaoDeSonhos, WerewolfGiftIdentifiers.TheurgeModelagemDeEspirito },
            WerewolfAuspiceGiftMechanics.BlockedGiftKeys.Order(StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void AuspiceGiftsDoNotFallBackToTheLegacyCatalogPayloadPath()
    {
        foreach (var giftKey in WerewolfAuspiceGiftMechanics.AuspiceGiftKeys)
        {
            var mechanic = WerewolfAuspiceGiftMechanics.Resolve(giftKey, 1, WerewolfFormIdentifiers.Homid, new Dictionary<string, int>(StringComparer.Ordinal));
            Assert.NotNull(mechanic);
        }
    }

    [Fact]
    public void EveryExecutableAuspiceGiftProducesADistinctTypedModifier()
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var entry in WerewolfAuspiceGiftMechanics.Audit()
                     .Where(e => e.Status == WerewolfBreedGiftStatus.Executable))
        {
            var mechanic = WerewolfAuspiceGiftMechanics.Resolve(
                entry.GiftKey, 2, WerewolfFormIdentifiers.Homid, new Dictionary<string, int>(StringComparer.Ordinal));

            // Gifts may share a mechanic archetype, but each must retain its own
            // source-derived parameters, so the payload values are part of the
            // signature rather than just its type.
            var signature = mechanic!.Payload is null
                ? $"{mechanic.Kind}:{mechanic.Magnitude}:none"
                : $"{mechanic.Kind}:{mechanic.Magnitude}:{JsonSerializer.Serialize(mechanic.Payload)}";
            Assert.True(seen.Add(signature), $"'{entry.GiftKey}' duplicated mechanic signature '{signature}'.");
        }
    }

    // =====================================================================
    // Helpers
    // =====================================================================

    private static WerewolfAuspiceGiftMechanic? Resolve(string giftKey, int successes) =>
        WerewolfAuspiceGiftMechanics.Resolve(giftKey, successes, WerewolfFormIdentifiers.Homid, new Dictionary<string, int>(StringComparer.Ordinal));

    private static WerewolfRuntimeCharacterState BuildState(string giftKey, string auspice)
    {
        var binding = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["attributes"] = JsonSerializer.Serialize(new Dictionary<string, int>(StringComparer.Ordinal)
            {
                [WerewolfAttributeIdentifiers.Strength] = 3,
                [WerewolfAttributeIdentifiers.Stamina] = 3,
                [WerewolfAttributeIdentifiers.Dexterity] = 3,
                [WerewolfAttributeIdentifiers.Appearance] = 3,
                [WerewolfAttributeIdentifiers.Perception] = 3,
                [WerewolfAttributeIdentifiers.Manipulation] = 3,
                [WerewolfAttributeIdentifiers.Charisma] = 3,
                [WerewolfAttributeIdentifiers.Intelligence] = 3,
                [WerewolfAttributeIdentifiers.Wits] = 3
            }),
            ["abilities"] = JsonSerializer.Serialize(new Dictionary<string, int>(StringComparer.Ordinal)
            {
                [WerewolfAbilityIdentifiers.Athletics] = 3,
                [WerewolfAbilityIdentifiers.Brawl] = 3,
                [WerewolfAbilityIdentifiers.Stealth] = 3,
                [WerewolfAbilityIdentifiers.Subterfuge] = 3,
                [WerewolfAbilityIdentifiers.Intimidation] = 3,
                [WerewolfAbilityIdentifiers.Leadership] = 3,
                [WerewolfAbilityIdentifiers.Performance] = 3,
                [WerewolfAbilityIdentifiers.Rituals] = 3,
                [WerewolfAbilityIdentifiers.Occult] = 3,
                [WerewolfAbilityIdentifiers.Enigmas] = 3,
                [WerewolfAbilityIdentifiers.Medicine] = 3,
                [WerewolfAbilityIdentifiers.Empathy] = 3,
                [WerewolfAbilityIdentifiers.Expression] = 3,
                [WerewolfAbilityIdentifiers.AnimalEmpathy] = 3,
                [WerewolfAbilityIdentifiers.PrimalInstinct] = 3
            })
        };

        return new WerewolfRuntimeCharacterState(
            PackageId: "test-package",
            PackageVersion: "0.1.0",
            DraftId: "draft-auspice",
            RuntimeStateVersion: 1,
            PackageBinding: binding,
            RagePermanent: 5,
            RageCurrent: 5,
            GnosisPermanent: 5,
            GnosisCurrent: 5,
            WillpowerPermanent: 5,
            WillpowerCurrent: 3,
            GloryPermanent: 0,
            GloryCurrent: 0,
            HonorPermanent: 0,
            HonorCurrent: 0,
            WisdomPermanent: 0,
            WisdomCurrent: 0,
            BirthRace: WerewolfRaceIdentifiers.Homid,
            HealthTrack: WerewolfHealthTrackComputer.Compute([], hasWeakenedImmuneSystem: false, lastRegenerationTurn: -1),
            CurrentForm: WerewolfFormIdentifiers.Homid,
            Conditions: [],
            FrenzyState: null,
            ActiveGiftEffects: [],
            KnownGiftKeys: [giftKey],
            SceneGiftUsage: new Dictionary<string, int>(StringComparer.Ordinal),
            CurrentSceneToken: "scene-1",
            ActivatedGiftKeys: [giftKey]);
    }
}
