using System.Text.Json;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

/// <summary>
/// Wave 2A: Breed Gift parity and executable semantics.
/// Source lines 1730-1870 (Homid, Metis, Lupus racial Gifts).
/// Every assertion below checks a concrete, source-derived outcome.
/// </summary>
public sealed class WerewolfBreedGiftTests
{
    private static readonly int[] NoDice = [];
    private static readonly int[] SourceDeviceDifficulties = [4, 6, 8, 9, 10];

    // =====================================================================
    // Catalog parity: the source has exactly 33 Breed Gifts.
    // =====================================================================
    [Fact]
    public void AllThirtyThreeSourceBreedGiftsAreRepresentedInTheCatalog()
    {
        // Homid 11 (lines 1733-1784), Metis 11 (1788-1824), Lupus 11 (1827-1870).
        var breedGifts = WerewolfGiftCatalog.AllDefinitions
            .Where(g => g.Category == WerewolfGiftCategory.Breed)
            .ToList();

        Assert.Equal(33, breedGifts.Count);
        Assert.Equal(33, WerewolfBreedGiftMechanics.BreedGiftKeys.Count);
        Assert.Equal(33, breedGifts.Select(g => g.GiftKey).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void BreedGiftsCoverEveryCataloguedBreedGiftAndNoOthers()
    {
        var catalogBreedKeys = WerewolfGiftCatalog.AllDefinitions
            .Where(g => g.Category == WerewolfGiftCategory.Breed)
            .Select(g => g.GiftKey)
            .ToHashSet(StringComparer.Ordinal);

        var mechanicsKeys = WerewolfBreedGiftMechanics.BreedGiftKeys.ToHashSet(StringComparer.Ordinal);

        Assert.Equal(catalogBreedKeys, mechanicsKeys);
    }

    [Fact]
    public void BreedGiftsAreOwnedByTheCorrectBreed()
    {
        // Source line 1730 Homid, 1785 Metis, 1825/1832 Lupus.
        var expectedOwners = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [WerewolfGiftIdentifiers.HomidMasterOfFire] = WerewolfRaceIdentifiers.Homid,
            [WerewolfGiftIdentifiers.HomidDefesaContraEspiritos] = WerewolfRaceIdentifiers.Homid,
            [WerewolfGiftIdentifiers.HomidAssimilacao] = WerewolfRaceIdentifiers.Homid,
            [WerewolfGiftIdentifiers.HomidRomperOVeu] = WerewolfRaceIdentifiers.Homid,
            [WerewolfGiftIdentifiers.MetisCreateElement] = WerewolfRaceIdentifiers.Metis,
            [WerewolfGiftIdentifiers.MetisMaldicaoDoOdio] = WerewolfRaceIdentifiers.Metis,
            [WerewolfGiftIdentifiers.MetisComunicacaoMental] = WerewolfRaceIdentifiers.Metis,
            [WerewolfGiftIdentifiers.MetisDefinharMembro] = WerewolfRaceIdentifiers.Metis,
            [WerewolfGiftIdentifiers.MetisDomDoPorcoEspinho] = WerewolfRaceIdentifiers.Metis,
            [WerewolfGiftIdentifiers.MetisDomDoTotem] = WerewolfRaceIdentifiers.Metis,
            [WerewolfGiftIdentifiers.MetisLoucura] = WerewolfRaceIdentifiers.Metis,
            [WerewolfGiftIdentifiers.LupusHareLeap] = WerewolfRaceIdentifiers.Lupus,
            [WerewolfGiftIdentifiers.LupusSentidosAgucados] = WerewolfRaceIdentifiers.Lupus,
            [WerewolfGiftIdentifiers.LupusSentirACaca] = WerewolfRaceIdentifiers.Lupus,
            [WerewolfGiftIdentifiers.LupusSensoDoSobrenatural] = WerewolfRaceIdentifiers.Lupus,
            [WerewolfGiftIdentifiers.LupusVisaoOlfativa] = WerewolfRaceIdentifiers.Lupus,
            [WerewolfGiftIdentifiers.LupusNomeDoEspirito] = WerewolfRaceIdentifiers.Lupus,
            [WerewolfGiftIdentifiers.LupusPesDeGato] = WerewolfRaceIdentifiers.Lupus,
            [WerewolfGiftIdentifiers.LupusRoer] = WerewolfRaceIdentifiers.Lupus,
            [WerewolfGiftIdentifiers.LupusVidaAnimal] = WerewolfRaceIdentifiers.Lupus,
            [WerewolfGiftIdentifiers.LupusCancaoDaGrandeFera] = WerewolfRaceIdentifiers.Lupus,
            [WerewolfGiftIdentifiers.LupusDomDosElementos] = WerewolfRaceIdentifiers.Lupus
        };

        foreach (var (giftKey, owner) in expectedOwners)
        {
            var definition = WerewolfGiftCatalog.Get(giftKey);
            Assert.NotNull(definition);
            Assert.Equal(owner, definition!.OwnerKey);
            Assert.Equal(WerewolfGiftCategory.Breed, definition.Category);
        }
    }

    [Fact]
    public void BreedGiftKeysAreStableAndUnique()
    {
        // Existing keys keep their historical prefixes; new keys follow the
        // gift.race.* convention. Nothing is churned.
        var keys = WerewolfBreedGiftMechanics.BreedGiftKeys;

        Assert.Equal(keys.Count, keys.Distinct(StringComparer.Ordinal).Count());
        Assert.All(keys, key =>
        {
            Assert.False(string.IsNullOrWhiteSpace(key));
            Assert.StartsWith("gift.", key, StringComparison.Ordinal);
            Assert.NotNull(WerewolfGiftCatalog.Get(key));
            Assert.False(string.IsNullOrWhiteSpace(WerewolfGiftCatalog.Get(key)!.SourceLocator));
        });
    }

    [Theory]
    [InlineData(WerewolfGiftIdentifiers.HomidDefesaContraEspiritos, 4, WerewolfGiftCostType.Gnosis, 1, 7)]
    [InlineData(WerewolfGiftIdentifiers.HomidAssimilacao, 5, WerewolfGiftCostType.None, 0, 5)]
    [InlineData(WerewolfGiftIdentifiers.HomidRomperOVeu, 5, WerewolfGiftCostType.Gnosis, 1, null)]
    [InlineData(WerewolfGiftIdentifiers.MetisMaldicaoDoOdio, 2, WerewolfGiftCostType.Gnosis, 1, null)]
    [InlineData(WerewolfGiftIdentifiers.MetisComunicacaoMental, 3, WerewolfGiftCostType.Willpower, 1, 8)]
    [InlineData(WerewolfGiftIdentifiers.MetisDefinharMembro, 4, WerewolfGiftCostType.Gnosis, 1, null)]
    [InlineData(WerewolfGiftIdentifiers.MetisDomDoPorcoEspinho, 4, WerewolfGiftCostType.Gnosis, 1, null)]
    [InlineData(WerewolfGiftIdentifiers.MetisDomDoTotem, 5, WerewolfGiftCostType.Gnosis, 1, 7)]
    [InlineData(WerewolfGiftIdentifiers.MetisLoucura, 5, WerewolfGiftCostType.Gnosis, 1, null)]
    [InlineData(WerewolfGiftIdentifiers.LupusSentirACaca, 1, WerewolfGiftCostType.None, 0, 7)]
    [InlineData(WerewolfGiftIdentifiers.LupusSensoDoSobrenatural, 2, WerewolfGiftCostType.None, 0, null)]
    [InlineData(WerewolfGiftIdentifiers.LupusVisaoOlfativa, 2, WerewolfGiftCostType.None, 0, null)]
    [InlineData(WerewolfGiftIdentifiers.LupusPesDeGato, 3, WerewolfGiftCostType.None, 0, null)]
    [InlineData(WerewolfGiftIdentifiers.LupusRoer, 4, WerewolfGiftCostType.Willpower, 1, 4)]
    [InlineData(WerewolfGiftIdentifiers.LupusVidaAnimal, 4, WerewolfGiftCostType.Gnosis, 1, 7)]
    [InlineData(WerewolfGiftIdentifiers.LupusCancaoDaGrandeFera, 5, WerewolfGiftCostType.Gnosis, 1, 8)]
    [InlineData(WerewolfGiftIdentifiers.LupusDomDosElementos, 5, WerewolfGiftCostType.Gnosis, 1, 7)]
    public void NewlyCataloguedBreedGiftsCarrySourceRankCostAndDifficulty(
        string giftKey, int level, WerewolfGiftCostType costType, int costAmount, int? difficulty)
    {
        var definition = WerewolfGiftCatalog.Get(giftKey);

        Assert.NotNull(definition);
        Assert.Equal(level, definition!.Level);
        Assert.Equal(costType, definition.CostType);
        Assert.Equal(costAmount, definition.CostAmount);
        Assert.Equal(difficulty, definition.TestDifficulty);
    }

    [Fact]
    public void ThornsOfTheBoarIsAPassiveGatedOnPredatorForm()
    {
        // Source line 1817 requires Crinos, Hispo or Lupus form.
        var definition = WerewolfGiftCatalog.Get(WerewolfGiftIdentifiers.MetisDomDoPorcoEspinho);
        Assert.Equal(WerewolfGiftActivationType.Active, definition!.ActivationType);

        var homid = Resolve(WerewolfGiftIdentifiers.MetisDomDoPorcoEspinho, successes: 2, form: WerewolfFormIdentifiers.Homid, sheet: EmptySheet());
        var crinos = Resolve(WerewolfGiftIdentifiers.MetisDomDoPorcoEspinho, successes: 2, form: WerewolfFormIdentifiers.Crinos, sheet: EmptySheet());

        Assert.Equal(0, homid!.Magnitude);
        Assert.Equal(1, crinos!.Magnitude);
        Assert.False(((WerewolfThornsPayload)homid.Payload!).FormSatisfied);
        Assert.True(((WerewolfThornsPayload)crinos.Payload!).FormSatisfied);
    }

    [Fact]
    public void CatsFeetIsAnInherentPermanentGift()
    {
        // Source line 1856: inherent ability, permanent once learned.
        var definition = WerewolfGiftCatalog.Get(WerewolfGiftIdentifiers.LupusPesDeGato);
        Assert.Equal(WerewolfGiftActivationType.Passive, definition!.ActivationType);
        Assert.Equal(WerewolfGiftDurationType.Permanent, definition.DurationType);
        Assert.Equal(WerewolfGiftCostType.None, definition.CostType);
    }

    [Fact]
    public void HeightenedSensesDifficultyReductionDependsOnForm()
    {
        // Source lines 1838-1839: -2 for Homid/Glabro, -3 for Crinos/Hispo/Lupus.
        var homid = Resolve(WerewolfGiftIdentifiers.LupusSentidosAgucados, 1, WerewolfFormIdentifiers.Homid, EmptySheet());
        var glabro = Resolve(WerewolfGiftIdentifiers.LupusSentidosAgucados, 1, WerewolfFormIdentifiers.Glabro, EmptySheet());
        var crinos = Resolve(WerewolfGiftIdentifiers.LupusSentidosAgucados, 1, WerewolfFormIdentifiers.Crinos, EmptySheet());
        var hispo = Resolve(WerewolfGiftIdentifiers.LupusSentidosAgucados, 1, WerewolfFormIdentifiers.Hispo, EmptySheet());

        Assert.Equal(2, homid!.Magnitude);
        Assert.Equal(2, glabro!.Magnitude);
        Assert.Equal(3, crinos!.Magnitude);
        Assert.Equal(3, hispo!.Magnitude);

        var crinosPayload = (WerewolfHeightenedSensesPayload)crinos.Payload!;
        Assert.Equal(1, crinosPayload.PrimalInstinctDiceBonus);

        var homidPayload = (WerewolfHeightenedSensesPayload)homid.Payload!;
        Assert.Equal(0, homidPayload.PrimalInstinctDiceBonus);
    }

    [Fact]
    public void CocoonIgnoresAttacksBelowStaminaPlusRituals()
    {
        // Source line 1774: ignores attacks that do not reach Stamina + Rituals.
        var sheet = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [WerewolfAbilityIdentifiers.Rituals] = 3
        };

        var mechanic = Resolve(WerewolfGiftIdentifiers.HomidCasulo, 1, WerewolfFormIdentifiers.Homid, sheet);

        Assert.Equal(3, mechanic!.Magnitude);
        Assert.Equal(3, ((WerewolfCocoonPayload)mechanic.Payload!).IgnoreAttacksBelow);
    }

    [Fact]
    public void CocoonIgnoresLessWithoutTheRitualsRating()
    {
        var mechanic = Resolve(WerewolfGiftIdentifiers.HomidCasulo, 1, WerewolfFormIdentifiers.Homid, EmptySheet());

        Assert.Equal(0, mechanic!.Magnitude);
        Assert.Equal(0, ((WerewolfCocoonPayload)mechanic.Payload!).IgnoreAttacksBelow);
    }

    // =====================================================================
    // Numeric outcomes driven by successes.
    // =====================================================================

    [Fact]
    public void ReshapeObjectDurationScalesWithSuccesses()
    {
        // Source lines 1764-1770.
        Assert.Equal(1, Resolve(WerewolfGiftIdentifiers.HomidRemodelarObjeto, 1, WerewolfFormIdentifiers.Homid, EmptySheet())!.DurationTurns);
        Assert.Equal(2, Resolve(WerewolfGiftIdentifiers.HomidRemodelarObjeto, 2, WerewolfFormIdentifiers.Homid, EmptySheet())!.DurationTurns);
        Assert.Equal(-1, Resolve(WerewolfGiftIdentifiers.HomidRemodelarObjeto, 3, WerewolfFormIdentifiers.Homid, EmptySheet())!.DurationTurns);
        Assert.Equal(-2, Resolve(WerewolfGiftIdentifiers.HomidRemodelarObjeto, 4, WerewolfFormIdentifiers.Homid, EmptySheet())!.DurationTurns);
        Assert.Equal(-1, Resolve(WerewolfGiftIdentifiers.HomidRemodelarObjeto, 5, WerewolfFormIdentifiers.Homid, EmptySheet())!.DurationTurns);
    }

    [Fact]
    public void MentalCommunicationReachIsFifteenKilometersPerSuccess()
    {
        // Source line 1807.
        var two = Resolve(WerewolfGiftIdentifiers.MetisComunicacaoMental, 2, WerewolfFormIdentifiers.Crinos, EmptySheet());
        var four = Resolve(WerewolfGiftIdentifiers.MetisComunicacaoMental, 4, WerewolfFormIdentifiers.Crinos, EmptySheet());

        Assert.Equal(30, two!.Magnitude);
        Assert.Equal(60, four!.Magnitude);
        Assert.Equal(15, ((WerewolfMentalCommunicationPayload)two.Payload!).KilometersPerSuccess);
        Assert.Equal(8, ((WerewolfMentalCommunicationPayload)two.Payload!).TestDifficulty);
    }

    [Fact]
    public void AnimalLifeReachIsFifteenKilometersPerSuccess()
    {
        // Source line 1863.
        var three = Resolve(WerewolfGiftIdentifiers.LupusVidaAnimal, 3, WerewolfFormIdentifiers.Crinos, EmptySheet());

        Assert.Equal(45, three!.Magnitude);
        var payload = (WerewolfAnimalCommandPayload)three.Payload!;
        Assert.Equal(15, payload.RadiusKilometersPerSuccess);
        Assert.Equal(7, payload.TestDifficulty);
    }

    [Fact]
    public void ElementalDominionAreaGrowsOneSuccessAtATime()
    {
        // Source line 1870: a 5m x 5m area per success.
        var three = Resolve(WerewolfGiftIdentifiers.LupusDomDosElementos, 3, WerewolfFormIdentifiers.Crinos, EmptySheet());

        Assert.Equal(3, three!.Magnitude);
        var payload = (WerewolfElementalDominionPayload)three.Payload!;
        Assert.Equal(5, payload.AreaMetersPerSuccess);
        Assert.Equal(7, payload.TestDifficulty);
        Assert.Equal(4, payload.Elements.Count);
    }

    [Fact]
    public void CreateElementFlameDamageIsOneLevelPerSuccessCappedAtThree()
    {
        // Source line 1790: 1 damage level per success, maximum 3 levels.
        var five = Resolve(WerewolfGiftIdentifiers.MetisCreateElement, 5, WerewolfFormIdentifiers.Crinos, EmptySheet());
        var payload = (WerewolfElementCreationPayload)five!.Payload!;

        Assert.Equal(3, five.Magnitude);
        Assert.Equal(3, payload.FlameDamageLevels);
        Assert.Equal(20, payload.RadiusMeters);
        Assert.Equal(50, payload.MaxMassKilograms);
    }

    [Fact]
    public void FascinateFleeTurnsEqualSuccesses()
    {
        // Source line 1745: 5 + the victim's Posto; the victim flees per success.
        var mechanic = Resolve(WerewolfGiftIdentifiers.HomidFitar, 3, WerewolfFormIdentifiers.Homid, EmptySheet());
        var payload = (WerewolfResistedTestPayload)mechanic!.Payload!;

        Assert.Equal(3, mechanic.Magnitude);
        Assert.Equal(5, payload.BaseDifficulty);
        Assert.Equal("Posto", payload.OpposingTrait);
        Assert.Equal(3, payload.Successes);
    }

    [Fact]
    public void PerturbTechnologyCarriesTheSourceDeviceDifficultyTable()
    {
        // Source lines 1750-1756: Computer 4, Telefone 6, Automóvel 8,
        // Arma de fogo 9, Faca 10.
        var mechanic = Resolve(WerewolfGiftIdentifiers.HomidPerturbarTecnologia, 2, WerewolfFormIdentifiers.Homid, EmptySheet());
        var payload = (WerewolfDeviceDisruptionPayload)mechanic!.Payload!;

        Assert.Equal(SourceDeviceDifficulties, payload.DeviceDifficulties);
        Assert.Equal(5, payload.DeviceNames.Count);
        Assert.Equal(15, payload.RadiusMeters);
        Assert.Equal(2, payload.DisabledTiers);
    }

    [Fact]
    public void DefenseAgainstSpiritsRemovesOneDiePerSuccessWithinThirtyMeters()
    {
        // Source line 1777.
        var mechanic = Resolve(WerewolfGiftIdentifiers.HomidDefesaContraEspiritos, 3, WerewolfFormIdentifiers.Homid, EmptySheet());
        var payload = (WerewolfSpiritRepulsionPayload)mechanic!.Payload!;

        Assert.Equal(3, mechanic.Magnitude);
        Assert.Equal(30, payload.RadiusMeters);
        Assert.Equal(1, payload.DiceRemovedPerSuccess);
        Assert.Equal(7, payload.TestDifficulty);
    }

    [Fact]
    public void CurseOfHatredDrainsTwoWillpowerAndTwoRageOncePerScene()
    {
        // Source line 1803.
        var mechanic = Resolve(WerewolfGiftIdentifiers.MetisMaldicaoDoOdio, 1, WerewolfFormIdentifiers.Crinos, EmptySheet());
        var payload = (WerewolfTargetResourceDrainPayload)mechanic!.Payload!;

        Assert.Equal(2, mechanic.Magnitude);
        Assert.Equal(2, payload.WillpowerLoss);
        Assert.Equal(2, payload.RageLoss);
        Assert.Equal(1, payload.UsesPerScene);
    }

    [Fact]
    public void WitherLimbHalvesSpeedAndAddsTwoDexterityDifficulty()
    {
        // Source line 1814.
        var mechanic = Resolve(WerewolfGiftIdentifiers.MetisDefinharMembro, 1, WerewolfFormIdentifiers.Crinos, EmptySheet());
        var payload = (WerewolfLimbAtrophyPayload)mechanic!.Payload!;

        Assert.Equal(4, payload.VictimStaminaBonus);
        Assert.Equal(2, payload.DexterityDifficultyIncrease);
        Assert.Equal(0.5, payload.MovementSpeedMultiplier);
        Assert.True(payload.PermanentAgainstNonRegenerators);
    }

    [Fact]
    public void MadnessLastsOneDayPerSuccess()
    {
        // Source line 1824.
        var mechanic = Resolve(WerewolfGiftIdentifiers.MetisLoucura, 4, WerewolfFormIdentifiers.Crinos, EmptySheet());
        var payload = (WerewolfMadnessPayload)mechanic!.Payload!;

        Assert.Equal(4, mechanic.Magnitude);
        Assert.Equal(4, payload.DaysPerSuccess);
    }

    [Fact]
    public void GnawAddsTwoBiteDamageDice()
    {
        // Source line 1860.
        var mechanic = Resolve(WerewolfGiftIdentifiers.LupusRoer, 1, WerewolfFormIdentifiers.Crinos, EmptySheet());
        var payload = (WerewolfBiteDamagePayload)mechanic!.Payload!;

        Assert.Equal(2, mechanic.Magnitude);
        Assert.Equal(2, payload.BonusDamageDice);
        Assert.Equal(4, payload.TestDifficulty);
    }

    [Fact]
    public void SenseTheHuntUsesSevenInTheWildAndNineInUrbanAreas()
    {
        // Source line 1842.
        var mechanic = Resolve(WerewolfGiftIdentifiers.LupusSentirACaca, 1, WerewolfFormIdentifiers.Crinos, EmptySheet());
        var payload = (WerewolfSenseRadiusPayload)mechanic!.Payload!;

        Assert.Equal(80, payload.RadiusKilometers);
        Assert.Equal(7, payload.WildDifficulty);
        Assert.Equal(9, payload.UrbanDifficulty);
    }

    [Fact]
    public void SenseTheWyrmUsesSixForFomoriAndEightForWyrmTrail()
    {
        // Source line 1796.
        var mechanic = Resolve(WerewolfGiftIdentifiers.MetisSentirAWyrm, 1, WerewolfFormIdentifiers.Crinos, EmptySheet());
        var payload = (WerewolfSenseRadiusPayload)mechanic!.Payload!;

        Assert.Equal(6, payload.WildDifficulty);
        Assert.Equal(8, payload.UrbanDifficulty);
    }

    [Fact]
    public void CatFeetGrantsThirtyMeterFallImmunityAndTwoPointCombatReduction()
    {
        // Source line 1855.
        var mechanic = Resolve(WerewolfGiftIdentifiers.LupusPesDeGato, 0, WerewolfFormIdentifiers.Crinos, EmptySheet());
        var payload = (WerewolfCatFeetPayload)mechanic!.Payload!;

        Assert.Equal(30, mechanic.Magnitude);
        Assert.Equal(30, payload.FallImmunityMeters);
        Assert.Equal(2, payload.CombatDifficultyReduction);
        Assert.True(payload.AlwaysLandsOnFeet);
    }

    [Fact]
    public void SimulateHumanScentPenalisesWildAnimalsWithinSixMeters()
    {
        // Source line 1741.
        var mechanic = Resolve(WerewolfGiftIdentifiers.HomidSimularOdorDeHomem, 0, WerewolfFormIdentifiers.Homid, EmptySheet());
        var payload = (WerewolfHumanScentPayload)mechanic!.Payload!;

        Assert.Equal(6, payload.RadiusMeters);
        Assert.Equal(1, payload.WildAnimalDicePenalty);
    }

    [Fact]
    public void PerceptionGiftDifferencesAreRepresentedWithDistinctKinds()
    {
        var metisWyrm = Resolve(WerewolfGiftIdentifiers.MetisSentirAWyrm, 1, WerewolfFormIdentifiers.Crinos, EmptySheet());
        var supernatural = Resolve(WerewolfGiftIdentifiers.LupusSensoDoSobrenatural, 1, WerewolfFormIdentifiers.Crinos, EmptySheet());

        Assert.Equal(WerewolfActiveGiftEffectKind.WyrmSense, metisWyrm!.Kind);
        Assert.Equal(WerewolfActiveGiftEffectKind.SupernaturalSense, supernatural!.Kind);
        Assert.NotEqual(metisWyrm.Kind, supernatural.Kind);

        var classes = (WerewolfSupernaturalSensePayload)supernatural.Payload!;
        Assert.Equal(5, classes.DetectableClasses.Count);
        Assert.Contains("wyrm", classes.DetectableClasses);
    }

    [Fact]
    public void EveryBreedGiftProducesADistinctTypedModifier()
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var giftKey in WerewolfBreedGiftMechanics.BreedGiftKeys)
        {
            var mechanic = Resolve(giftKey, successes: 2, form: WerewolfFormIdentifiers.Crinos, sheet: EmptySheet());

            Assert.NotNull(mechanic);
            Assert.False(string.IsNullOrWhiteSpace(mechanic!.SourceLocator));
            Assert.True(
                mechanic.SourceLocator.StartsWith("Line 17", StringComparison.Ordinal) ||
                mechanic.SourceLocator.StartsWith("Line 18", StringComparison.Ordinal),
                $"Breed Gift '{giftKey}' cites '{mechanic.SourceLocator}', outside the Breed Gift source range.");

            var signature = WerewolfBreedGiftMechanics.IsBlocked(giftKey)
                ? $"blocked:{giftKey}"
                : $"{mechanic.Kind}:{mechanic.Magnitude}:{mechanic.Payload?.GetType().Name}";
            Assert.True(seen.Add(signature), $"Breed Gift '{giftKey}' produced a duplicate mechanic signature '{signature}'.");
        }
    }

    [Fact]
    public void EveryBreedGiftCarriesASourceLocatorMatchingItsCatalogEntry()
    {
        foreach (var giftKey in WerewolfBreedGiftMechanics.BreedGiftKeys)
        {
            var definition = WerewolfGiftCatalog.Get(giftKey);
            Assert.NotNull(definition);
            Assert.Equal(definition!.SourceLocator, Resolve(giftKey, 1, WerewolfFormIdentifiers.Crinos, EmptySheet())!.SourceLocator);
        }
    }

    // =====================================================================
    // Blocked gifts are reported, never faked.
    // =====================================================================
    [Fact]
    public void ExactlyTwoBreedGiftsAreBlockedByMissingSubsystems()
    {
        // Dom do Totem (line 1819) needs totem effect semantics.
        // Canção da Grande Fera (line 1865) needs Pangaea and summoning.
        Assert.Equal(2, WerewolfBreedGiftMechanics.BlockedGiftKeys.Count);
        Assert.Contains(WerewolfGiftIdentifiers.MetisDomDoTotem, WerewolfBreedGiftMechanics.BlockedGiftKeys);
        Assert.Contains(WerewolfGiftIdentifiers.LupusCancaoDaGrandeFera, WerewolfBreedGiftMechanics.BlockedGiftKeys);
    }

    [Theory]
    [InlineData(WerewolfGiftIdentifiers.MetisDomDoTotem)]
    [InlineData(WerewolfGiftIdentifiers.LupusCancaoDaGrandeFera)]
    public void BlockedBreedGiftsReportTheirMissingSubsystemInsteadOfFakingIt(string giftKey)
    {
        var mechanic = Resolve(giftKey, 3, WerewolfFormIdentifiers.Crinos, EmptySheet());
        var payload = Assert.IsType<WerewolfBlockedGiftPayload>(mechanic!.Payload);

        Assert.False(string.IsNullOrWhiteSpace(payload.Reason));
        Assert.False(string.IsNullOrWhiteSpace(payload.MissingSubsystem));
        Assert.Equal(giftKey, mechanic.GiftKey);
    }

    [Fact]
    public void ThirtyOneBreedGiftsAreExecutableAndTwoAreBlocked()
    {
        var executable = WerewolfBreedGiftMechanics.BreedGiftKeys
            .Where(key => !WerewolfBreedGiftMechanics.IsBlocked(key))
            .ToList();

        Assert.Equal(31, executable.Count);
    }

    // =====================================================================
    // Runtime reachability: every Breed Gift resolves through the registered
    // runtime operation and produces its typed effect.
    // =====================================================================
    [Fact]
    public void EveryBreedGiftIsReachableThroughTheEffectService()
    {
        foreach (var giftKey in WerewolfBreedGiftMechanics.BreedGiftKeys)
        {
            var state = BuildState(giftKey, WerewolfRaceIdentifiers.Homid, WerewolfFormIdentifiers.Crinos);
            var result = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
                $"req-{giftKey}", state, state.RuntimeStateVersion, giftKey, ActivationSuccesses: 2));

            Assert.True(result.Succeeded, $"Breed Gift '{giftKey}' did not execute: {string.Join("; ", result.Findings)}");
            Assert.NotEmpty(result.ActiveEffects);
            Assert.NotNull(result.UpdatedState);
            Assert.True(result.UpdatedState!.RuntimeStateVersion > state.RuntimeStateVersion);
        }
    }

    [Fact]
    public void BreedGiftStateTransitionIsRecordedOnTheUpdatedState()
    {
        var state = BuildState(WerewolfGiftIdentifiers.MetisRaivaPrimordial, WerewolfRaceIdentifiers.Metis, WerewolfFormIdentifiers.Crinos);
        var rageBefore = state.RageCurrent;

        var result = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-rage", state, state.RuntimeStateVersion, WerewolfGiftIdentifiers.MetisRaivaPrimordial, ActivationSuccesses: 0));

        Assert.True(result.Succeeded);
        // Source line 1793: +2 Rage, allowed to exceed the permanent limit.
        Assert.Equal(rageBefore + 2, result.UpdatedState!.RageCurrent);
    }

    [Fact]
    public void UnrelatedGiftsKeepTheirExistingEffectPath()
    {
        // A non-Breed Gift must not be routed through the Breed mechanics table.
        Assert.False(WerewolfBreedGiftMechanics.IsBreedGift(WerewolfGiftIdentifiers.RagabashOpenSeal));
        Assert.Null(WerewolfBreedGiftMechanics.Resolve(WerewolfGiftIdentifiers.RagabashOpenSeal, 1, WerewolfFormIdentifiers.Homid, EmptySheet()));
    }

    // =====================================================================
    // Helpers
    // =====================================================================

    private static WerewolfBreedGiftMechanic? Resolve(string giftKey, int successes, string form, Dictionary<string, int> sheet) =>
        WerewolfBreedGiftMechanics.Resolve(giftKey, successes, form, sheet);

    private static Dictionary<string, int> EmptySheet() => new(StringComparer.Ordinal);

    private static WerewolfRuntimeCharacterState BuildState(string giftKey, string race, string form)
    {
        var binding = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["attributes"] = JsonSerializer.Serialize(new Dictionary<string, int>(StringComparer.Ordinal)
            {
                [WerewolfAttributeIdentifiers.Strength] = 3,
                [WerewolfAttributeIdentifiers.Stamina] = 3,
                [WerewolfAttributeIdentifiers.Dexterity] = 3,
                [WerewolfAttributeIdentifiers.Perception] = 3,
                [WerewolfAttributeIdentifiers.Manipulation] = 3,
                [WerewolfAttributeIdentifiers.Charisma] = 3
            }),
            ["abilities"] = JsonSerializer.Serialize(new Dictionary<string, int>(StringComparer.Ordinal)
            {
                [WerewolfAbilityIdentifiers.Athletics] = 3,
                [WerewolfAbilityIdentifiers.Crafts] = 3,
                [WerewolfAbilityIdentifiers.Rituals] = 3,
                [WerewolfAbilityIdentifiers.Occult] = 3,
                [WerewolfAbilityIdentifiers.Enigmas] = 3,
                [WerewolfAbilityIdentifiers.PrimalInstinct] = 3,
                [WerewolfAbilityIdentifiers.Intimidation] = 3,
                [WerewolfAbilityIdentifiers.Empathy] = 3,
                [WerewolfAbilityIdentifiers.Expression] = 3,
                [WerewolfAbilityIdentifiers.AnimalEmpathy] = 3
            })
        };

        return new WerewolfRuntimeCharacterState(
            PackageId: "test-package",
            PackageVersion: "0.1.0",
            DraftId: "draft-breed",
            RuntimeStateVersion: 1,
            PackageBinding: binding,
            RagePermanent: 5,
            RageCurrent: 5,
            GnosisPermanent: 5,
            GnosisCurrent: 5,
            WillpowerPermanent: 5,
            WillpowerCurrent: 5,
            GloryPermanent: 0,
            GloryCurrent: 0,
            HonorPermanent: 0,
            HonorCurrent: 0,
            WisdomPermanent: 0,
            WisdomCurrent: 0,
            BirthRace: race,
            HealthTrack: WerewolfHealthTrackComputer.Compute([], hasWeakenedImmuneSystem: false, lastRegenerationTurn: -1),
            CurrentForm: form,
            Conditions: [],
            FrenzyState: null,
            ActiveGiftEffects: [],
            KnownGiftKeys: [giftKey],
            SceneGiftUsage: new Dictionary<string, int>(StringComparer.Ordinal),
            CurrentSceneToken: "scene-1",
            ActivatedGiftKeys: [giftKey]);
    }
}
