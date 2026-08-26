using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfGiftRuntimeTests
{
    [Theory]
    [InlineData(WerewolfGiftIdentifiers.HomidMasterOfFire, 1)]
    [InlineData(WerewolfGiftIdentifiers.MetisCreateElement, 1)]
    [InlineData(WerewolfGiftIdentifiers.LupusHareLeap, 1)]
    [InlineData(WerewolfGiftIdentifiers.RagabashOpenSeal, 1)]
    [InlineData(WerewolfGiftIdentifiers.TheurgeSpiritSpeech, 1)]
    [InlineData(WerewolfGiftIdentifiers.PhilodoxResistPain, 1)]
    [InlineData(WerewolfGiftIdentifiers.GalliardBeastSpeech, 1)]
    [InlineData(WerewolfGiftIdentifiers.AhrounFallingTouch, 1)]
    [InlineData(WerewolfGiftIdentifiers.GlassWalkersControlSimpleMachine, 1)]
    [InlineData(WerewolfGiftIdentifiers.GlassWalkersDiagnostics, 1)]
    [InlineData(WerewolfGiftIdentifiers.GlassWalkersTrickShot, 1)]
    [InlineData(WerewolfGiftIdentifiers.GetOfFenrisRazorClaws, 1)]
    [InlineData(WerewolfGiftIdentifiers.GetOfFenrisResistPain, 1)]
    [InlineData(WerewolfGiftIdentifiers.GetOfFenrisVisageOfFenris, 1)]
    [InlineData(WerewolfGiftIdentifiers.FiannaFaerieLight, 1)]
    [InlineData(WerewolfGiftIdentifiers.FiannaPersuasion, 1)]
    [InlineData(WerewolfGiftIdentifiers.FiannaResistToxin, 1)]
    [InlineData(WerewolfGiftIdentifiers.ChildrenOfGaiaMercy, 1)]
    [InlineData(WerewolfGiftIdentifiers.ChildrenOfGaiaMothersTouch, 1)]
    [InlineData(WerewolfGiftIdentifiers.BlackFuriesBreathOfTheWyrm, 1)]
    [InlineData(WerewolfGiftIdentifiers.BlackFuriesHeightenedSenses, 1)]
    [InlineData(WerewolfGiftIdentifiers.BlackFuriesSenseWyrm, 1)]
    [InlineData(WerewolfGiftIdentifiers.RedTalonsBeastSpeech, 1)]
    [InlineData(WerewolfGiftIdentifiers.RedTalonsWolfAtTheDoor, 1)]
    [InlineData(WerewolfGiftIdentifiers.RedTalonsScentOfRunningWater, 1)]
    [InlineData(WerewolfGiftIdentifiers.SilentStridersSilence, 1)]
    [InlineData(WerewolfGiftIdentifiers.SilentStridersSpeedOfThought, 1)]
    [InlineData(WerewolfGiftIdentifiers.SilverFangsLambentFlame, 1)]
    [InlineData(WerewolfGiftIdentifiers.SilverFangsFalconsGrasp, 1)]
    [InlineData(WerewolfGiftIdentifiers.BoneGnawersCooking, 1)]
    [InlineData(WerewolfGiftIdentifiers.BoneGnawersStickyFingers, 1)]
    [InlineData(WerewolfGiftIdentifiers.ShadowLordsSeizingTheEdge, 1)]
    [InlineData(WerewolfGiftIdentifiers.ShadowLordsAuraOfConfidence, 1)]
    [InlineData(WerewolfGiftIdentifiers.ShadowLordsFatalFlaw, 1)]
    [InlineData(WerewolfGiftIdentifiers.UktenaSpiritSpeech, 1)]
    [InlineData(WerewolfGiftIdentifiers.UktenaShroud, 1)]
    [InlineData(WerewolfGiftIdentifiers.UktenaSenseMagic, 1)]
    [InlineData(WerewolfGiftIdentifiers.WendigoCamouflage, 1)]
    [InlineData(WerewolfGiftIdentifiers.WendigoCallTheBreeze, 1)]
    [InlineData(WerewolfGiftIdentifiers.HomidSimularOdorDeHomem, 1)]
    [InlineData(WerewolfGiftIdentifiers.HomidPerturbarTecnologia, 2)]
    [InlineData(WerewolfGiftIdentifiers.MetisRaivaPrimordial, 1)]
    [InlineData(WerewolfGiftIdentifiers.MetisSentirAWyrm, 1)]
    [InlineData(WerewolfGiftIdentifiers.MetisCavar, 2)]
    [InlineData(WerewolfGiftIdentifiers.MetisOlhosDeGato, 3)]
    [InlineData(WerewolfGiftIdentifiers.LupusSentidosAgucados, 1)]
    [InlineData(WerewolfGiftIdentifiers.RagabashEmbacamentoDaPropriaForma, 1)]
    [InlineData(WerewolfGiftIdentifiers.RagabashSimularOCheiroDeAguaCorrente, 1)]
    [InlineData(WerewolfGiftIdentifiers.RagabashInduzirEsquecimento, 2)]
    [InlineData(WerewolfGiftIdentifiers.TheurgeSentirAWyrm, 1)]
    [InlineData(WerewolfGiftIdentifiers.PhilodoxFaroParaAFormaVerdadeira, 1)]
    [InlineData(WerewolfGiftIdentifiers.PhilodoxVerdadeDeGaia, 1)]
    [InlineData(WerewolfGiftIdentifiers.PhilodoxReiDosAnimais, 2)]
    [InlineData(WerewolfGiftIdentifiers.GalliardComunicacaoComAnimais, 1)]
    [InlineData(WerewolfGiftIdentifiers.GalliardDistracoes, 2)]
    [InlineData(WerewolfGiftIdentifiers.AhrounGarrasAfiadas, 1)]
    [InlineData(WerewolfGiftIdentifiers.AhrounInspiracao, 1)]
    [InlineData(WerewolfGiftIdentifiers.AhrounEspiritoDaBatalha, 2)]
    [InlineData(WerewolfGiftIdentifiers.AhrounMedoVerdadeiro, 2)]
    [InlineData(WerewolfGiftIdentifiers.AhrounSentirAPrata, 2)]
    [InlineData(WerewolfGiftIdentifiers.GlassWalkersSentidosCiberneticos, 2)]
    [InlineData(WerewolfGiftIdentifiers.GlassWalkersSobrecargaDeEnergia, 2)]
    [InlineData(WerewolfGiftIdentifiers.GetOfFenrisDeterAFugaDosCovardes, 2)]
    [InlineData(WerewolfGiftIdentifiers.GetOfFenrisRugidoDoPredador, 2)]
    [InlineData(WerewolfGiftIdentifiers.FiannaUivoDaBanshee, 2)]
    [InlineData(WerewolfGiftIdentifiers.ChildrenOfGaiaResistenciaADor, 1)]
    [InlineData(WerewolfGiftIdentifiers.ChildrenOfGaiaAcalmar, 2)]
    [InlineData(WerewolfGiftIdentifiers.ChildrenOfGaiaArmaduraDeLuna, 2)]
    [InlineData(WerewolfGiftIdentifiers.BlackFuriesMaldicaoDeEolo, 2)]
    [InlineData(WerewolfGiftIdentifiers.BlackFuriesSentirAPresa, 2)]
    [InlineData(WerewolfGiftIdentifiers.RedTalonsMenteAnimal, 2)]
    [InlineData(WerewolfGiftIdentifiers.RedTalonsSentirAPresa, 2)]
    [InlineData(WerewolfGiftIdentifiers.SilentStridersSentirAWyrm, 1)]
    [InlineData(WerewolfGiftIdentifiers.SilentStridersGerarIgnorancia, 2)]
    [InlineData(WerewolfGiftIdentifiers.SilentStridersResistenciaDeMensageiro, 2)]
    [InlineData(WerewolfGiftIdentifiers.SilverFangsSentirAWyrm, 1)]
    [InlineData(WerewolfGiftIdentifiers.SilverFangsArmaduraDeLuna, 2)]
    [InlineData(WerewolfGiftIdentifiers.SilverFangsEmpatia, 2)]
    [InlineData(WerewolfGiftIdentifiers.BoneGnawersGerarIgnorancia, 2)]
    [InlineData(WerewolfGiftIdentifiers.BoneGnawersOdorRepugnante, 2)]
    [InlineData(WerewolfGiftIdentifiers.ShadowLordsAplausoTrovejante, 2)]
    [InlineData(WerewolfGiftIdentifiers.ShadowLordsArmaduraDeLuna, 2)]
    [InlineData(WerewolfGiftIdentifiers.UktenaEspiritoDoPassaro, 2)]
    [InlineData(WerewolfGiftIdentifiers.UktenaEspiritoDoPeixe, 2)]
    [InlineData(WerewolfGiftIdentifiers.WendigoResistenciaADor, 1)]
    [InlineData(WerewolfGiftIdentifiers.WendigoVentoCortante, 2)]
    [InlineData(WerewolfGiftIdentifiers.HomidPersuasao, 1)]
    [InlineData(WerewolfGiftIdentifiers.HomidFitar, 2)]
    [InlineData(WerewolfGiftIdentifiers.HomidInquietacao, 3)]
    [InlineData(WerewolfGiftIdentifiers.HomidRemodelarObjeto, 3)]
    [InlineData(WerewolfGiftIdentifiers.HomidCasulo, 4)]
    [InlineData(WerewolfGiftIdentifiers.FiannaRemodelarObjeto, 3)]
    [InlineData(WerewolfGiftIdentifiers.BoneGnawersRemodelarObjeto, 3)]
    public void CatalogReturnsDefinitionForEveryGift(string giftKey, int expectedLevel)
    {
        var definition = WerewolfGiftCatalog.Get(giftKey);

        Assert.NotNull(definition);
        Assert.Equal(giftKey, definition.GiftKey);
        Assert.Equal(expectedLevel, definition.Level);
    }

    [Fact]
    public void CatalogReturnsNullForUnknownGift()
    {
        var definition = WerewolfGiftCatalog.Get("gift.unknown");

        Assert.Null(definition);
    }

    [Fact]
    public void CatalogIsKnownReturnsFalseForNullOrWhiteSpace()
    {
        Assert.False(WerewolfGiftCatalog.IsKnown(null));
        Assert.False(WerewolfGiftCatalog.IsKnown(string.Empty));
        Assert.False(WerewolfGiftCatalog.IsKnown("  "));
    }

    [Fact]
    public void ActivationFailsForUnknownGiftKey()
    {
        var state = BuildRuntimeState();
        var result = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, "gift.unknown"));

        Assert.False(result.Succeeded);
        Assert.Equal("UnknownGift", result.ErrorCode);
    }

    [Fact]
    public void ActivationFailsForNullOrEmptyGiftKey()
    {
        var state = BuildRuntimeState();

        var nullResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest("req-001", state, 1, ""));
        Assert.False(nullResult.Succeeded);
        Assert.Equal("MissingGiftKey", nullResult.ErrorCode);

        var emptyResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, ""));
        Assert.False(emptyResult.Succeeded);
        Assert.Equal("MissingGiftKey", emptyResult.ErrorCode);
    }

    [Fact]
    public void ActivationFailsForStaleVersion()
    {
        var state = BuildRuntimeState();
        var result = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 2, WerewolfGiftIdentifiers.HomidMasterOfFire));

        Assert.False(result.Succeeded);
        Assert.Equal("StaleVersion", result.ErrorCode);
    }

    [Fact]
    public void ActivationSucceedsForPassiveGift()
    {
        var state = BuildRuntimeState();
        var result = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.TheurgeSpiritSpeech));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.ActivationDefinition);
        Assert.Equal(0, result.ActivationDefinition.DicePool);
        Assert.Equal(0, result.ActivationDefinition.Difficulty);
        Assert.Empty(result.ActivationDefinition.TestComponents);
    }

    [Fact]
    public void ActivationPaysRageCostForRageGift()
    {
        var state = BuildRuntimeState();
        var result = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.GetOfFenrisRazorClaws));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.UpdatedState);
        Assert.Equal(4, result.UpdatedState.RageCurrent);
    }

    [Fact]
    public void ActivationPaysGnosisCostForGnosisGift()
    {
        var state = BuildRuntimeState();
        var result = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.HomidMasterOfFire));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.UpdatedState);
        Assert.Equal(4, result.UpdatedState.GnosisCurrent);
    }

    [Fact]
    public void ActivationPaysWillpowerCostForWillpowerGift()
    {
        var state = BuildRuntimeState();
        var result = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.PhilodoxResistPain));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.UpdatedState);
        Assert.Equal(4, result.UpdatedState.WillpowerCurrent);
    }

    [Fact]
    public void ActivationFailsWhenInsufficientRage()
    {
        var state = BuildRuntimeState() with { RageCurrent = 0 };
        var result = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.GetOfFenrisRazorClaws));

        Assert.False(result.Succeeded);
        Assert.Equal("InsufficientResources", result.ErrorCode);
    }

    [Fact]
    public void ActivationFailsWhenInsufficientGnosis()
    {
        var state = BuildRuntimeState() with { GnosisCurrent = 0 };
        var result = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.HomidMasterOfFire));

        Assert.False(result.Succeeded);
        Assert.Equal("InsufficientResources", result.ErrorCode);
    }

    [Fact]
    public void ActivationFailsWhenInsufficientWillpower()
    {
        var state = BuildRuntimeState() with { WillpowerCurrent = 0 };
        var result = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.PhilodoxResistPain));

        Assert.False(result.Succeeded);
        Assert.Equal("InsufficientResources", result.ErrorCode);
    }

    [Fact]
    public void ActivationComputesTestPoolForTestRequiredGift()
    {
        var state = BuildRuntimeState();
        var result = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.RagabashOpenSeal));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.ActivationDefinition);
        Assert.Equal(5, result.ActivationDefinition.DicePool);
        Assert.Equal(6, result.ActivationDefinition.Difficulty);
        Assert.Contains("Gnosis", result.ActivationDefinition.TestComponents);
    }

    [Fact]
    public void EffectRegistersActiveEffectForSceneDurationGift()
    {
        var state = BuildRuntimeState();
        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.HomidMasterOfFire));

        Assert.True(activationResult.Succeeded);
        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
            WerewolfGiftIdentifiers.HomidMasterOfFire, 1));

        Assert.True(effectResult.Succeeded);
        Assert.Single(effectResult.ActiveEffects);
        Assert.Equal(WerewolfGiftIdentifiers.HomidMasterOfFire, effectResult.ActiveEffects[0].GiftKey);
        Assert.Equal(WerewolfGiftDurationType.Scene, effectResult.ActiveEffects[0].DurationType);
    }

    [Fact]
    public void EffectDoesNotRegisterActiveEffectForInstantGift()
    {
        var state = BuildRuntimeState() with { BirthRace = WerewolfRaceIdentifiers.Lupus };
        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.LupusHareLeap));

        Assert.True(activationResult.Succeeded);
        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
            WerewolfGiftIdentifiers.LupusHareLeap, 2));

        Assert.True(effectResult.Succeeded);
        Assert.Empty(effectResult.ActiveEffects);
    }

    [Fact]
    public void EffectFailsForStaleVersion()
    {
        var state = BuildRuntimeState();
        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-001", state, 2, WerewolfGiftIdentifiers.HomidMasterOfFire, 1));

        Assert.False(effectResult.Succeeded);
        Assert.Equal("StaleVersion", effectResult.ErrorCode);
    }

    [Fact]
    public void EffectFailsForUnknownGiftKey()
    {
        var state = BuildRuntimeState();
        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-001", state, 1, "gift.unknown", 1));

        Assert.False(effectResult.Succeeded);
        Assert.Equal("UnknownGift", effectResult.ErrorCode);
    }

    [Fact]
    public void RuntimeActivateGiftThroughReferenceRuntime()
    {
        var registry = RegisteredRuntimeRegistry();
        var created = registry.Execute(Request(WerewolfReferenceRuntime.CreateCharacterOperation, new Dictionary<string, string>(StringComparer.Ordinal) { ["requestId"] = "req-001" }));
        var completed = CompleteCharacter(registry, created);
        Assert.True(completed.Succeeded, "CompleteCharacter failed: " + string.Join("; ", completed.Findings.Select(f => f.Code + ":" + f.Message)));
        Assert.Equal("Completed", completed.Outputs["status"]);

        var activateResult = registry.Execute(Request(WerewolfReferenceRuntime.ActivateGiftOperation, new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["requestId"] = "activate-gift-001",
            ["draftId"] = completed.Outputs["draftId"],
            ["newState"] = completed.Outputs["newState"],
            ["runtimeStateVersion"] = "1",
            ["expectedRuntimeStateVersion"] = "1",
            ["giftKey"] = WerewolfGiftIdentifiers.RagabashOpenSeal
        }));

        Assert.True(activateResult.Succeeded, "ActivateGift failed: " + string.Join("; ", activateResult.Findings.Select(f => f.Code + ":" + f.Message)));
        Assert.Equal(WerewolfGiftIdentifiers.RagabashOpenSeal, activateResult.Outputs["giftKey"]);
        Assert.Equal("None", activateResult.Outputs["costType"]);
    }

    [Fact]
    public void RuntimeExecuteGiftEffectThroughReferenceRuntime()
    {
        var registry = RegisteredRuntimeRegistry();
        var created = registry.Execute(Request(WerewolfReferenceRuntime.CreateCharacterOperation, new Dictionary<string, string>(StringComparer.Ordinal) { ["requestId"] = "req-001" }));
        var completed = CompleteCharacter(registry, created);

        var activateResult = registry.Execute(Request(WerewolfReferenceRuntime.ActivateGiftOperation, new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["requestId"] = "activate-gift-002",
            ["draftId"] = completed.Outputs["draftId"],
            ["newState"] = completed.Outputs["newState"],
            ["runtimeStateVersion"] = "1",
            ["expectedRuntimeStateVersion"] = "1",
            ["giftKey"] = WerewolfGiftIdentifiers.HomidMasterOfFire
        }));

        Assert.True(activateResult.Succeeded, "ActivateGift failed: " + string.Join("; ", activateResult.Findings.Select(f => f.Code + ":" + f.Message)));
        var newVersion = activateResult.Outputs["newRuntimeStateVersion"];
        var effectResult = registry.Execute(Request(WerewolfReferenceRuntime.ExecuteGiftEffectOperation, new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["requestId"] = "execute-effect-002",
            ["draftId"] = completed.Outputs["draftId"],
            ["newState"] = activateResult.Outputs["newState"],
            ["runtimeStateVersion"] = newVersion,
            ["expectedRuntimeStateVersion"] = newVersion,
            ["giftKey"] = WerewolfGiftIdentifiers.HomidMasterOfFire,
            ["activationSuccesses"] = "1"
        }));

        Assert.True(effectResult.Succeeded, "ExecuteGiftEffect failed: " + string.Join("; ", effectResult.Findings.Select(f => f.Code + ":" + f.Message)));
        Assert.Equal("1", effectResult.Outputs["activeEffectsCount"]);
    }

    [Fact]
    public void GiftCatalogAllDefinitionsHaveSourceLocators()
    {
        foreach (var definition in WerewolfGiftCatalog.AllDefinitions)
        {
            Assert.False(string.IsNullOrWhiteSpace(definition.SourceLocator));
            Assert.StartsWith("Line ", definition.SourceLocator);
        }
    }

    [Fact]
    public void GiftCatalogAllDefinitionsHaveValidCategory()
    {
        foreach (var definition in WerewolfGiftCatalog.AllDefinitions)
        {
            Assert.True(Enum.IsDefined<WerewolfGiftCategory>(definition.Category));
        }
    }

    [Fact]
    public void GiftCatalogBreedGiftsMapToValidRaces()
    {
        var breedGifts = WerewolfGiftCatalog.AllDefinitions.Where(g => g.Category == WerewolfGiftCategory.Breed).ToList();
        Assert.NotEmpty(breedGifts);

        foreach (var gift in breedGifts)
        {
            Assert.Contains(gift.OwnerKey, WerewolfRaceIdentifiers.Supported);
        }
    }

    [Fact]
    public void GiftCatalogAuspiceGiftsMapToValidAuspices()
    {
        var auspiceGifts = WerewolfGiftCatalog.AllDefinitions.Where(g => g.Category == WerewolfGiftCategory.Auspice).ToList();
        Assert.NotEmpty(auspiceGifts);

        foreach (var gift in auspiceGifts)
        {
            Assert.Contains(gift.OwnerKey, WerewolfAuspiceIdentifiers.Supported);
        }
    }

    [Fact]
    public void GiftCatalogTribeGiftsMapToValidTribes()
    {
        var tribeGifts = WerewolfGiftCatalog.AllDefinitions.Where(g => g.Category == WerewolfGiftCategory.Tribe).ToList();
        Assert.NotEmpty(tribeGifts);

        foreach (var gift in tribeGifts)
        {
            Assert.Contains(gift.OwnerKey, WerewolfTribeIdentifiers.Supported);
        }
    }

    [Fact]
    public void ActivationDefinitionIncludesSourceLocator()
    {
        var state = BuildRuntimeState();
        var result = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.HomidMasterOfFire));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.ActivationDefinition);
        Assert.StartsWith("Line ", result.ActivationDefinition.SourceLocator);
    }

    [Fact]
    public void EffectResultIncludesFindings()
    {
        var state = BuildRuntimeState();
        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.HomidMasterOfFire));

        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
            WerewolfGiftIdentifiers.HomidMasterOfFire, 1));

        Assert.True(effectResult.Succeeded);
        Assert.NotEmpty(effectResult.Findings);
    }

    [Fact]
    public void ActivationRuntimeStateIncrementsVersion()
    {
        var state = BuildRuntimeState();
        var result = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.PhilodoxResistPain));

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.NewRuntimeStateVersion);
    }

    [Fact]
    public void EffectRuntimeStateIncrementsVersion()
    {
        var state = BuildRuntimeState();
        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.HomidMasterOfFire));

        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
            WerewolfGiftIdentifiers.HomidMasterOfFire, 1));

        Assert.True(effectResult.Succeeded);
        Assert.Equal(activationResult.NewRuntimeStateVersion + 1, effectResult.NewRuntimeStateVersion);
    }

    [Fact]
    public void ActivationFailsForGiftNotKnownByCharacter()
    {
        var state = BuildRuntimeState() with { KnownGiftKeys = WerewolfGiftIdentifiers.Supported.Where(k => k == WerewolfGiftIdentifiers.LupusHareLeap).ToList() };
        var result = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.HomidMasterOfFire));

        Assert.False(result.Succeeded);
        Assert.Equal("GiftNotKnown", result.ErrorCode);
    }

    [Fact]
    public void EffectMapsEffectKindCorrectlyForCombatGifts()
    {
        var state = BuildRuntimeState();
        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.GetOfFenrisRazorClaws));

        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
            WerewolfGiftIdentifiers.GetOfFenrisRazorClaws, 1));

        Assert.True(effectResult.Succeeded);
        Assert.Single(effectResult.ActiveEffects);
        Assert.Equal(WerewolfActiveGiftEffectKind.CombatDamageBonus, effectResult.ActiveEffects[0].EffectKind);
    }

    [Fact]
    public void EffectMapsEffectKindCorrectlyForSocialGifts()
    {
        var state = BuildRuntimeState();
        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.FiannaPersuasion));

        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
            WerewolfGiftIdentifiers.FiannaPersuasion, 1));

        Assert.True(effectResult.Succeeded);
        Assert.Single(effectResult.ActiveEffects);
        Assert.Equal(WerewolfActiveGiftEffectKind.SocialTestBonus, effectResult.ActiveEffects[0].EffectKind);
    }

    [Fact]
    public void PassiveSpiritGiftDoesNotCreateActiveEffect()
    {
        var state = BuildRuntimeState();
        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.TheurgeSpiritSpeech));

        Assert.True(activationResult.Succeeded);
        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
            WerewolfGiftIdentifiers.TheurgeSpiritSpeech, 1));

        Assert.True(effectResult.Succeeded);
        Assert.Empty(effectResult.ActiveEffects);
    }

    [Fact]
    public void ActivationReturnsCorrectCostForEachCostType()
    {
        var state = BuildRuntimeState();

        var rageResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.GetOfFenrisRazorClaws));
        Assert.True(rageResult.Succeeded);
        Assert.Equal(WerewolfGiftCostType.Rage, rageResult.ActivationDefinition!.CostType);
        Assert.Equal(1, rageResult.ActivationDefinition.CostAmount);

        var gnosisResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-002", state, 1, WerewolfGiftIdentifiers.HomidMasterOfFire));
        Assert.True(gnosisResult.Succeeded);
        Assert.Equal(WerewolfGiftCostType.Gnosis, gnosisResult.ActivationDefinition!.CostType);
        Assert.Equal(1, gnosisResult.ActivationDefinition.CostAmount);

        var willpowerResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-003", state, 1, WerewolfGiftIdentifiers.PhilodoxResistPain));
        Assert.True(willpowerResult.Succeeded);
        Assert.Equal(WerewolfGiftCostType.Willpower, willpowerResult.ActivationDefinition!.CostType);
        Assert.Equal(1, willpowerResult.ActivationDefinition.CostAmount);
    }

    [Fact]
    public void GiftCatalogAllGiftsHaveValidLevel()
    {
        foreach (var definition in WerewolfGiftCatalog.AllDefinitions)
        {
            Assert.True(definition.Level >= 1 && definition.Level <= 5);
        }
    }

    [Fact]
    public void GiftCatalogHasExpectedCount()
    {
        Assert.Equal(93, WerewolfGiftCatalog.AllDefinitions.Count);
    }

    [Fact]
    public void GiftIdentifiersSupportedMatchesCatalog()
    {
        var catalogKeys = WerewolfGiftCatalog.AllDefinitions.Select(g => g.GiftKey).ToHashSet(StringComparer.Ordinal);
        var identifierKeys = WerewolfGiftIdentifiers.Supported.ToHashSet(StringComparer.Ordinal);

        Assert.Equal(catalogKeys, identifierKeys);
    }

    [Fact]
    public void RuntimeCharacterStateIncludesActiveGiftEffects()
    {
        var state = BuildRuntimeState();
        var activeEffects = new List<WerewolfActiveGiftEffect>
        {
            new WerewolfActiveGiftEffect("test-gift", 0, WerewolfGiftDurationType.Scene, 10, WerewolfActiveGiftEffectKind.SocialTestBonus, 1, "Line 1")
        };

        var updated = state with { ActiveGiftEffects = activeEffects };

        Assert.Single(updated.ActiveGiftEffects);
        Assert.Equal("test-gift", updated.ActiveGiftEffects[0].GiftKey);
    }

    [Fact]
    public void EffectActiveEffectsPersistAcrossStateVersions()
    {
        var state = BuildRuntimeState();
        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.HomidMasterOfFire));

        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
            WerewolfGiftIdentifiers.HomidMasterOfFire, 1));

        Assert.True(effectResult.Succeeded);
        Assert.NotNull(effectResult.UpdatedState);
        Assert.Single(effectResult.UpdatedState.ActiveGiftEffects);
    }

    private static WerewolfRuntimeCharacterState BuildRuntimeState()
    {
        return new WerewolfRuntimeCharacterState(
            PackageId: "test-package",
            PackageVersion: "0.1.0",
            DraftId: "draft-001",
            RuntimeStateVersion: 1,
            PackageBinding: new Dictionary<string, string>(StringComparer.Ordinal),
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
            BirthRace: WerewolfRaceIdentifiers.Homid,
            HealthTrack: WerewolfHealthTrackComputer.Compute([], hasWeakenedImmuneSystem: false, lastRegenerationTurn: -1),
            CurrentForm: WerewolfFormIdentifiers.Homid,
            Conditions: [],
            FrenzyState: null,
            ActiveGiftEffects: [],
            KnownGiftKeys: WerewolfGiftIdentifiers.Supported.ToList(),
            SceneGiftUsage: new Dictionary<string, int>(StringComparer.Ordinal),
            CurrentSceneToken: string.Empty,
            ActivatedGiftKeys: []);
    }

    private static RuleSetOperationResult CompleteCharacter(RuleSetRuntimeRegistry registry, RuleSetOperationResult created)
    {
        var race = registry.Execute(Request(WerewolfReferenceRuntime.SelectRaceOperation, Inputs(created.Outputs,
            ("raceId", WerewolfRaceIdentifiers.Homid))));
        EnsureSucceeded(race, "SelectRace");

        var auspice = registry.Execute(Request(WerewolfReferenceRuntime.SelectAuspiceOperation, Inputs(race.Outputs,
            ("auspiceId", WerewolfAuspiceIdentifiers.Ragabash))));
        EnsureSucceeded(auspice, "SelectAuspice");

        var tribe = registry.Execute(Request(WerewolfReferenceRuntime.SelectTribeOperation, Inputs(auspice.Outputs,
            ("tribeId", WerewolfTribeIdentifiers.GlassWalkers))));
        EnsureSucceeded(tribe, "SelectTribe");

        var raceGift = registry.Execute(Request(WerewolfReferenceRuntime.SelectRaceGiftOperation, GiftInputs(tribe, WerewolfGiftIdentifiers.HomidMasterOfFire)));
        EnsureSucceeded(raceGift, "SelectRaceGift");

        var auspiceGift = registry.Execute(Request(WerewolfReferenceRuntime.SelectAuspiceGiftOperation, GiftInputs(raceGift, WerewolfGiftIdentifiers.RagabashOpenSeal)));
        EnsureSucceeded(auspiceGift, "SelectAuspiceGift");

        var tribeGift = registry.Execute(Request(WerewolfReferenceRuntime.SelectTribeGiftOperation, GiftInputs(auspiceGift, WerewolfGiftIdentifiers.GlassWalkersControlSimpleMachine)));
        EnsureSucceeded(tribeGift, "SelectTribeGift");

        var attributePriorities = registry.Execute(Request(WerewolfReferenceRuntime.SelectAttributePrioritiesOperation, Inputs(tribeGift.Outputs,
            ("primaryCategoryId", WerewolfAttributeCategoryIdentifiers.Physical),
            ("secondaryCategoryId", WerewolfAttributeCategoryIdentifiers.Social),
            ("tertiaryCategoryId", WerewolfAttributeCategoryIdentifiers.Mental))));
        EnsureSucceeded(attributePriorities, "SelectAttributePriorities");

        var attributeAllocation = registry.Execute(Request(WerewolfReferenceRuntime.AllocateAttributesOperation, Inputs(attributePriorities.Outputs,
            ("attributePriorityOrder", attributePriorities.Outputs["attributePriorityOrder"]),
            ("attributeBudgets", attributePriorities.Outputs["attributeBudgets"]),
            ("attributes", "character.attribute.strength:4,character.attribute.dexterity:3,character.attribute.stamina:3,character.attribute.charisma:3,character.attribute.manipulation:3,character.attribute.appearance:2,character.attribute.perception:2,character.attribute.intelligence:2,character.attribute.wits:2"))));
        EnsureSucceeded(attributeAllocation, "AllocateAttributes");

        var abilityPriorities = registry.Execute(Request(WerewolfReferenceRuntime.SelectAbilityPrioritiesOperation, Inputs(attributeAllocation.Outputs,
            ("primaryCategoryId", WerewolfAbilityCategoryIdentifiers.Talents),
            ("secondaryCategoryId", WerewolfAbilityCategoryIdentifiers.Skills),
            ("tertiaryCategoryId", WerewolfAbilityCategoryIdentifiers.Knowledges))));
        EnsureSucceeded(abilityPriorities, "SelectAbilityPriorities");

        var abilityAllocation = registry.Execute(Request(WerewolfReferenceRuntime.AllocateAbilitiesOperation, Inputs(abilityPriorities.Outputs,
            ("abilityPriorityOrder", abilityPriorities.Outputs["abilityPriorityOrder"]),
            ("abilityBudgets", abilityPriorities.Outputs["abilityBudgets"]),
            ("abilities", "character.ability.alertness:2,character.ability.athletics:2,character.ability.brawl:2,character.ability.dodge:0,character.ability.empathy:2,character.ability.expression:2,character.ability.intimidation:2,character.ability.primal-instinct:0,character.ability.streetwise:0,character.ability.subterfuge:1,character.ability.animal-empathy:0,character.ability.crafts:0,character.ability.drive:2,character.ability.etiquette:2,character.ability.firearms:0,character.ability.leadership:2,character.ability.melee:0,character.ability.performance:2,character.ability.stealth:1,character.ability.survival:0,character.ability.computer:1,character.ability.enigmas:0,character.ability.investigation:1,character.ability.law:1,character.ability.linguistics:0,character.ability.medicine:0,character.ability.occult:1,character.ability.politics:1,character.ability.rituals:0,character.ability.science:0"))));
        EnsureSucceeded(abilityAllocation, "AllocateAbilities");

        var backgrounds = registry.Execute(Request(WerewolfReferenceRuntime.AllocateBackgroundsOperation, Inputs(abilityAllocation.Outputs,
            ("backgrounds", "character.background.allies:2,character.background.ancestors:0,character.background.contacts:1,character.background.fetish:0,character.background.kinfolk:0,character.background.mentor:0,character.background.pure-breed:0,character.background.resources:1,character.background.rites:1"))));
        EnsureSucceeded(backgrounds, "AllocateBackgrounds");

        var resources = registry.Execute(Request(WerewolfReferenceRuntime.InitializeResourcesAndRankOperation, Inputs(backgrounds.Outputs)));
        EnsureSucceeded(resources, "InitializeResourcesAndRank");

        var ragbashRenown = registry.Execute(Request(WerewolfReferenceRuntime.SelectRagabashRenownOperation, Inputs(resources.Outputs,
            ("glory", "0"),
            ("honor", "0"),
            ("wisdom", "3"))));
        EnsureSucceeded(ragbashRenown, "SelectRagabashRenown");

        var identity = registry.Execute(Request(WerewolfReferenceRuntime.SetIdentityNameOperation, Inputs(ragbashRenown.Outputs, ("identityName", "Test Character"))));
        EnsureSucceeded(identity, "SetIdentityName");

        return registry.Execute(Request(WerewolfReferenceRuntime.CompleteCharacterOperation, Inputs(identity.Outputs)));
    }

    private static void EnsureSucceeded(RuleSetOperationResult result, string step)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"{step} failed: " + string.Join("; ", result.Findings.Select(f => f.Code + ":" + f.Message)));
        }
    }

    private static Dictionary<string, string> Inputs(IReadOnlyDictionary<string, string> outputs, params (string Key, string Value)[] additional)
    {
        var inputs = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["draftId"] = outputs["draftId"],
            ["draftVersion"] = outputs["draftVersion"],
            ["expectedDraftVersion"] = outputs["draftVersion"]
        };

        foreach (var pair in additional)
        {
            inputs[pair.Key] = pair.Value;
        }

        foreach (var key in new[] { "raceId", "auspiceId", "tribeId", "metisDeformityId", "raceGiftId", "auspiceGiftId", "tribeGiftId", "attributePriorityOrder", "attributeBudgets", "abilityPriorityOrder", "abilityBudgets", "attributes", "abilities", "backgrounds", "resources", "renown", "rankId", "rankValue", "identityName", "nextSteps", "status" })
        {
            if (outputs.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value) && !inputs.ContainsKey($"current{char.ToUpperInvariant(key[0])}{key[1..]}"))
            {
                inputs[key switch
                {
                    "raceId" => "currentRace",
                    "auspiceId" => "currentAuspice",
                    "tribeId" => "currentTribe",
                    "metisDeformityId" => "currentMetisDeformity",
                    "raceGiftId" => "currentRaceGift",
                    "auspiceGiftId" => "currentAuspiceGift",
                    "tribeGiftId" => "currentTribeGift",
                    "status" => "draftStatus",
                    _ => key
                 }] = value;
            }
        }

        return inputs;
    }

    private static RuleSetOperationRequest Request(string operationKey, IReadOnlyDictionary<string, string> inputs)
    {
        return new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            operationKey,
            inputs);
    }

    private static Dictionary<string, string> GiftInputs(RuleSetOperationResult state, string giftId)
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["draftId"] = state.Outputs["draftId"],
            ["draftVersion"] = state.Outputs["draftVersion"],
            ["expectedDraftVersion"] = state.Outputs["draftVersion"],
            ["currentRace"] = state.Outputs["raceId"],
            ["currentAuspice"] = state.Outputs["auspiceId"],
            ["currentTribe"] = state.Outputs["tribeId"],
            ["currentRaceGift"] = state.Outputs["raceGiftId"],
            ["currentAuspiceGift"] = state.Outputs["auspiceGiftId"],
            ["currentTribeGift"] = state.Outputs["tribeGiftId"],
            ["giftId"] = giftId
        };
    }

    private static RuleSetRuntimeRegistry RegisteredRuntimeRegistry()
    {
        return RuleSetRuntimeRegistrationService.Register(new RuleSetRuntimeRegistrationRequest(RegisteredCatalog(), [new WerewolfReferenceRuntime()])).Registry;
    }

    private static RuleSetPackageCatalog RegisteredCatalog()
    {
        var discovery = RuleSetPackageSourceDiscoveryService.Discover(new RuleSetPackageSourceDiscoveryRequest([RuleSetsRoot()]));
        var registration = RuleSetPackageRegistrationService.Register(new RuleSetPackageRegistrationRequest(discovery.ValidatedPackages, 1));
        return registration.Catalog;
    }

    private static string RuleSetsRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Chronicle.sln")))
            {
                return Path.Combine(directory.FullName, "rule-sets");
            }
            directory = directory.Parent;
        }
        throw new InvalidOperationException("Could not find repository root.");
    }

    [Fact]
    public void KnownGiftIsNotAutomaticallyActive()
    {
        var state = BuildRuntimeState();
        var knownGift = WerewolfGiftIdentifiers.HomidMasterOfFire;
        Assert.Contains(knownGift, state.KnownGiftKeys);

        Assert.Empty(state.ActiveGiftEffects);
    }

    [Fact]
    public void UnknownGiftActivationRejects()
    {
        var state = BuildRuntimeState() with { KnownGiftKeys = new List<string> { WerewolfGiftIdentifiers.HomidMasterOfFire } };
        var result = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, "gift.unknown"));

        Assert.False(result.Succeeded);
        Assert.Equal("UnknownGift", result.ErrorCode);
    }

    [Fact]
    public void KnownPassiveGiftDoesNotCreateActiveEffect()
    {
        var state = BuildRuntimeState();
        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.TheurgeSpiritSpeech));

        Assert.True(activationResult.Succeeded);
        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
            WerewolfGiftIdentifiers.TheurgeSpiritSpeech, 0));

        Assert.True(effectResult.Succeeded);
        Assert.Empty(effectResult.ActiveEffects);
    }

    [Fact]
    public void KnownActiveGiftCreatesActiveEffectOnlyWhenRequired()
    {
        var state = BuildRuntimeState();
        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.HomidMasterOfFire));

        Assert.True(activationResult.Succeeded);
        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
            WerewolfGiftIdentifiers.HomidMasterOfFire, 1));

        Assert.True(effectResult.Succeeded);
        Assert.Single(effectResult.ActiveEffects);
        Assert.Equal(WerewolfGiftDurationType.Scene, effectResult.ActiveEffects[0].DurationType);
    }

    [Fact]
    public void SceneDurationUsesSceneTokenNotTurns()
    {
        var state = BuildRuntimeState() with { CurrentSceneToken = "scene-1" };
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.HomidMasterOfFire);
        state = state with { KnownGiftKeys = knownGifts };

        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-000", state, 1, WerewolfGiftIdentifiers.HomidMasterOfFire));
        Assert.True(activationResult.Succeeded);

        var activationResult2 = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", activationResult.UpdatedState!, 2, WerewolfGiftIdentifiers.HomidMasterOfFire));
        Assert.True(activationResult2.Succeeded);
        Assert.NotNull(activationResult2.ActivationDefinition);
        Assert.Equal(-1, activationResult2.ActivationDefinition.DurationTurns);
    }

    [Fact]
    public void OnePerSceneIsEncodedAsUsageLimitNotDuration()
    {
        var definition = WerewolfGiftCatalog.Get(WerewolfGiftIdentifiers.BoneGnawersStickyFingers);
        Assert.NotNull(definition);
        Assert.Equal(1, definition.MaxUsesPerScene);
        Assert.Equal(WerewolfGiftDurationType.Scene, definition.DurationType);
    }

    [Fact]
    public void CostIsPaidBeforeRollOnActivation()
    {
        var state = BuildRuntimeState();
        var initialRage = state.RageCurrent;
        var result = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.GetOfFenrisRazorClaws));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.UpdatedState);
        Assert.Equal(initialRage - 1, result.UpdatedState.RageCurrent);
    }

    [Fact]
    public void FailedRollDoesNotRefundCost()
    {
        var state = BuildRuntimeState();
        var initialGnosis = state.GnosisCurrent;
        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.HomidMasterOfFire));

        Assert.True(activationResult.Succeeded);
        Assert.Equal(initialGnosis - 1, activationResult.UpdatedState!.GnosisCurrent);
    }

    [Fact]
    public void AllCataloguedGiftsArePresent()
    {
        Assert.Equal(93, WerewolfGiftCatalog.AllDefinitions.Count);
    }

    [Fact]
    public void EachCataloguedGiftHasSourceLocator()
    {
        foreach (var definition in WerewolfGiftCatalog.AllDefinitions)
        {
            Assert.False(string.IsNullOrWhiteSpace(definition.SourceLocator));
            Assert.StartsWith("Line ", definition.SourceLocator);
        }
    }

    [Fact]
    public void SceneEffectsExpireWhenSceneTokenChanges()
    {
        var state = BuildRuntimeState() with { CurrentSceneToken = "scene-1" };
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.HomidMasterOfFire);
        state = state with { KnownGiftKeys = knownGifts };

        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-000", state, 1, WerewolfGiftIdentifiers.HomidMasterOfFire));
        Assert.True(activationResult.Succeeded);

        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-001", activationResult.UpdatedState!, 2, WerewolfGiftIdentifiers.HomidMasterOfFire, 3));
        Assert.True(effectResult.Succeeded);
        Assert.Single(effectResult.ActiveEffects);

        var updatedState = effectResult.UpdatedState! with { CurrentSceneToken = "scene-2" };
        var validEffects = WerewolfGiftEffectService.GetSceneValidEffects(updatedState);
        Assert.Empty(validEffects);
    }

    [Fact]
    public void PermanentEffectsDoNotCarrySceneToken()
    {
        var state = BuildRuntimeState() with { CurrentSceneToken = "scene-1" };
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.TheurgeSpiritSpeech);
        state = state with { KnownGiftKeys = knownGifts };

        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-000", state, 1, WerewolfGiftIdentifiers.TheurgeSpiritSpeech));
        Assert.True(activationResult.Succeeded);

        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-001", activationResult.UpdatedState!, 2, WerewolfGiftIdentifiers.TheurgeSpiritSpeech, 0));
        Assert.True(effectResult.Succeeded);
        Assert.Empty(effectResult.ActiveEffects);
    }

    [Fact]
    public void KnownGiftDoesNotCreateActiveEffect()
    {
        var state = BuildRuntimeState();
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.HomidMasterOfFire);
        var updatedState = state with { KnownGiftKeys = knownGifts };

        Assert.Empty(updatedState.ActiveGiftEffects);
    }

    [Fact]
    public void ActivatingActiveGiftCreatesExactlyOneEffect()
    {
        var state = BuildRuntimeState();
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.HomidMasterOfFire);
        state = state with { KnownGiftKeys = knownGifts };

        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.HomidMasterOfFire));
        Assert.True(activationResult.Succeeded);

        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, 2, WerewolfGiftIdentifiers.HomidMasterOfFire, 3));
        Assert.True(effectResult.Succeeded);
        Assert.Single(effectResult.ActiveEffects);
        Assert.Equal(WerewolfActiveGiftEffectKind.DamageReduction, effectResult.ActiveEffects[0].EffectKind);
    }

    [Fact]
    public void ExecuteGiftEffectRejectsUnknownGift()
    {
        var state = BuildRuntimeState();
        var result = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-001", state, 1, "unknown-gift", 0));

        Assert.False(result.Succeeded);
        Assert.Equal("UnknownGift", result.ErrorCode);
    }

    [Fact]
    public void ExecuteGiftEffectRejectsKnownButNotActivatedGift()
    {
        var state = BuildRuntimeState();
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.HomidMasterOfFire);
        state = state with { KnownGiftKeys = knownGifts };

        var result = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.HomidMasterOfFire, 0));

        Assert.False(result.Succeeded);
        Assert.Equal("GiftNotActivated", result.ErrorCode);
    }

    [Fact]
    public void HealthIntegrationMotherTouchHealsDamage()
    {
        var state = BuildRuntimeState();
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.ChildrenOfGaiaMothersTouch);
        state = state with { KnownGiftKeys = knownGifts };

        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.ChildrenOfGaiaMothersTouch));
        Assert.True(activationResult.Succeeded);

        var effectState = activationResult.UpdatedState!;
        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", effectState, 2, WerewolfGiftIdentifiers.ChildrenOfGaiaMothersTouch, 3));
        Assert.True(effectResult.Succeeded);

        var damageResult = WerewolfApplyDamageService.ApplyDamage(new WerewolfApplyDamageRequest(
            "req-003", effectResult.UpdatedState!, 3, WerewolfDamageCategory.Lethal, 2));
        Assert.True(damageResult.Succeeded);

        var recoverResult = WerewolfRecoverDamageService.RecoverDamage(new WerewolfRecoverDamageRequest(
            "req-004", damageResult.UpdatedState!, 4, WerewolfDamageCategory.Lethal, 1));
        Assert.True(recoverResult.Succeeded);
        Assert.True(recoverResult.Findings.Any(f => f.Contains("Gift healing")), $"Expected 'Gift healing' in findings, got: {string.Join(", ", recoverResult.Findings)}");
    }

    [Fact]
    public void HealthIntegrationResistPainRemovesWoundPenalty()
    {
        var state = BuildRuntimeState();
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.PhilodoxResistPain);
        state = state with { KnownGiftKeys = knownGifts };

        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.PhilodoxResistPain));
        Assert.True(activationResult.Succeeded);

        var effectState = activationResult.UpdatedState!;
        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", effectState, 2, WerewolfGiftIdentifiers.PhilodoxResistPain, 0));
        Assert.True(effectResult.Succeeded);

        var damageResult = WerewolfApplyDamageService.ApplyDamage(new WerewolfApplyDamageRequest(
            "req-003", effectResult.UpdatedState!, 3, WerewolfDamageCategory.Lethal, 3));
        Assert.True(damageResult.Succeeded);
        Assert.Equal(0, damageResult.UpdatedState!.HealthTrack!.WoundPenalty);
        Assert.Contains("Gift effect ignores wound penalties.", damageResult.Findings);
    }

    [Fact]
    public void HealthIntegrationResistToxinBlocksPoison()
    {
        var state = BuildRuntimeState();
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.FiannaResistToxin);
        state = state with { KnownGiftKeys = knownGifts };

        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.FiannaResistToxin));
        Assert.True(activationResult.Succeeded);

        var effectState = activationResult.UpdatedState!;
        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", effectState, 2, WerewolfGiftIdentifiers.FiannaResistToxin, 0));
        Assert.True(effectResult.Succeeded);

        var damageResult = WerewolfApplyDamageService.ApplyDamage(new WerewolfApplyDamageRequest(
            "req-003", effectResult.UpdatedState!, 3, WerewolfDamageCategory.Bashing, 2, true));
        Assert.True(damageResult.Succeeded);
        Assert.Equal(0, damageResult.UpdatedState!.HealthTrack!.TotalDamage);
        Assert.Contains("Gift effect grants immunity to poison damage.", damageResult.Findings);
    }

    [Fact]
    public void ConditionIntegrationFallingTouchAppliesProne()
    {
        var state = BuildRuntimeState();
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.AhrounFallingTouch);
        state = state with { KnownGiftKeys = knownGifts };

        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.AhrounFallingTouch));
        Assert.True(activationResult.Succeeded);

        var effectState = activationResult.UpdatedState!;
        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", effectState, 2, WerewolfGiftIdentifiers.AhrounFallingTouch, 3));
        Assert.True(effectResult.Succeeded);

        Assert.Contains(effectResult.UpdatedState!.Conditions!, c => c.ConditionKey == WerewolfConditionIdentifiers.Prone && c.IsActive);
    }

    [Fact]
    public void ConditionIntegrationFalconsGraspAppliesRestrained()
    {
        var state = BuildRuntimeState();
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.SilverFangsFalconsGrasp);
        state = state with { KnownGiftKeys = knownGifts };

        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.SilverFangsFalconsGrasp));
        Assert.True(activationResult.Succeeded);

        var effectState = activationResult.UpdatedState!;
        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", effectState, 2, WerewolfGiftIdentifiers.SilverFangsFalconsGrasp, 0));
        Assert.True(effectResult.Succeeded);

        Assert.Contains(effectResult.UpdatedState!.Conditions!, c => c.ConditionKey == WerewolfConditionIdentifiers.Restrained && c.IsActive);
    }

    [Fact]
    public void ProneConditionBlocksActions()
    {
        var state = BuildRuntimeState();
        state = state with { Conditions = Array.AsReadOnly(new[]
        {
            new WerewolfCondition(WerewolfConditionIdentifiers.Prone, WerewolfConditionKind.Prone, "test", "", 1, true)
        })};

        var result = WerewolfConditionService.EvaluateActionAvailability(new WerewolfEvaluateActionAvailabilityRequest(
            "req-001", state, 1, "any-action"));
        Assert.True(result.Succeeded);
        Assert.False(result.IsAvailable);
        Assert.Equal("Prone", result.UnavailableReason);
    }

    [Fact]
    public void RestrainedConditionBlocksActions()
    {
        var state = BuildRuntimeState();
        state = state with { Conditions = Array.AsReadOnly(new[]
        {
            new WerewolfCondition(WerewolfConditionIdentifiers.Restrained, WerewolfConditionKind.Restrained, "test", "", 1, true)
        })};

        var result = WerewolfConditionService.EvaluateActionAvailability(new WerewolfEvaluateActionAvailabilityRequest(
            "req-001", state, 1, "any-action"));
        Assert.True(result.Succeeded);
        Assert.False(result.IsAvailable);
        Assert.Equal("Restrained", result.UnavailableReason);
    }

    [Fact]
    public void WaveAGiftsAreAllCatalogued()
    {
        var waveAGiftKeys = new[]
        {
            WerewolfGiftIdentifiers.HomidSimularOdorDeHomem,
            WerewolfGiftIdentifiers.HomidPerturbarTecnologia,
            WerewolfGiftIdentifiers.MetisRaivaPrimordial,
            WerewolfGiftIdentifiers.MetisSentirAWyrm,
            WerewolfGiftIdentifiers.MetisCavar,
            WerewolfGiftIdentifiers.MetisOlhosDeGato,
            WerewolfGiftIdentifiers.LupusSentidosAgucados,
            WerewolfGiftIdentifiers.RagabashEmbacamentoDaPropriaForma,
            WerewolfGiftIdentifiers.RagabashSimularOCheiroDeAguaCorrente,
            WerewolfGiftIdentifiers.RagabashInduzirEsquecimento,
            WerewolfGiftIdentifiers.TheurgeSentirAWyrm,
            WerewolfGiftIdentifiers.PhilodoxFaroParaAFormaVerdadeira,
            WerewolfGiftIdentifiers.PhilodoxVerdadeDeGaia,
            WerewolfGiftIdentifiers.PhilodoxReiDosAnimais,
            WerewolfGiftIdentifiers.GalliardComunicacaoComAnimais,
            WerewolfGiftIdentifiers.GalliardDistracoes,
            WerewolfGiftIdentifiers.AhrounGarrasAfiadas,
            WerewolfGiftIdentifiers.AhrounInspiracao,
            WerewolfGiftIdentifiers.AhrounEspiritoDaBatalha,
            WerewolfGiftIdentifiers.AhrounMedoVerdadeiro,
            WerewolfGiftIdentifiers.AhrounSentirAPrata,
            WerewolfGiftIdentifiers.GlassWalkersSentidosCiberneticos,
            WerewolfGiftIdentifiers.GlassWalkersSobrecargaDeEnergia,
            WerewolfGiftIdentifiers.GetOfFenrisDeterAFugaDosCovardes,
            WerewolfGiftIdentifiers.GetOfFenrisRugidoDoPredador,
            WerewolfGiftIdentifiers.FiannaUivoDaBanshee,
            WerewolfGiftIdentifiers.ChildrenOfGaiaResistenciaADor,
            WerewolfGiftIdentifiers.ChildrenOfGaiaAcalmar,
            WerewolfGiftIdentifiers.ChildrenOfGaiaArmaduraDeLuna,
            WerewolfGiftIdentifiers.BlackFuriesMaldicaoDeEolo,
            WerewolfGiftIdentifiers.BlackFuriesSentirAPresa,
            WerewolfGiftIdentifiers.RedTalonsMenteAnimal,
            WerewolfGiftIdentifiers.RedTalonsSentirAPresa,
            WerewolfGiftIdentifiers.SilentStridersSentirAWyrm,
            WerewolfGiftIdentifiers.SilentStridersGerarIgnorancia,
            WerewolfGiftIdentifiers.SilentStridersResistenciaDeMensageiro,
            WerewolfGiftIdentifiers.SilverFangsSentirAWyrm,
            WerewolfGiftIdentifiers.SilverFangsArmaduraDeLuna,
            WerewolfGiftIdentifiers.SilverFangsEmpatia,
            WerewolfGiftIdentifiers.BoneGnawersGerarIgnorancia,
            WerewolfGiftIdentifiers.BoneGnawersOdorRepugnante,
            WerewolfGiftIdentifiers.ShadowLordsAplausoTrovejante,
            WerewolfGiftIdentifiers.ShadowLordsArmaduraDeLuna,
            WerewolfGiftIdentifiers.UktenaEspiritoDoPassaro,
            WerewolfGiftIdentifiers.UktenaEspiritoDoPeixe,
            WerewolfGiftIdentifiers.WendigoResistenciaADor,
            WerewolfGiftIdentifiers.WendigoVentoCortante
        };

        foreach (var giftKey in waveAGiftKeys)
        {
            var definition = WerewolfGiftCatalog.Get(giftKey);
            Assert.NotNull(definition);
            Assert.Equal(giftKey, definition.GiftKey);
            Assert.False(string.IsNullOrWhiteSpace(definition.SourceLocator));
            Assert.StartsWith("Line ", definition.SourceLocator);
            Assert.True(Enum.IsDefined<WerewolfGiftCategory>(definition.Category));
        }
    }

    [Fact]
    public void WaveAGnosisGiftPaysGnosisCost()
    {
        var state = BuildRuntimeState();
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.HomidPerturbarTecnologia);
        state = state with { KnownGiftKeys = knownGifts };

        var result = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.HomidPerturbarTecnologia));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.UpdatedState);
        Assert.Equal(4, result.UpdatedState.GnosisCurrent);
    }

    [Fact]
    public void WaveARageGiftPaysRageCost()
    {
        var state = BuildRuntimeState();
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.AhrounGarrasAfiadas);
        state = state with { KnownGiftKeys = knownGifts };

        var result = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.AhrounGarrasAfiadas));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.UpdatedState);
        Assert.Equal(4, result.UpdatedState.RageCurrent);
    }

    [Fact]
    public void WaveATestRequiredGiftComputesTestPool()
    {
        var state = BuildRuntimeState();
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.MetisCavar);
        state = state with { KnownGiftKeys = knownGifts };

        var result = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.MetisCavar));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.ActivationDefinition);
        Assert.True(result.ActivationDefinition.DicePool > 0);
        Assert.Contains("Athletics", result.ActivationDefinition.TestComponents);
    }

    [Fact]
    public void WaveASceneDurationGiftRegistersActiveEffect()
    {
        var state = BuildRuntimeState();
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.HomidSimularOdorDeHomem);
        state = state with { KnownGiftKeys = knownGifts };

        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.HomidSimularOdorDeHomem));

        Assert.True(activationResult.Succeeded);
        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
            WerewolfGiftIdentifiers.HomidSimularOdorDeHomem, 1));

        Assert.True(effectResult.Succeeded);
        Assert.Single(effectResult.ActiveEffects);
        Assert.Equal(WerewolfGiftIdentifiers.HomidSimularOdorDeHomem, effectResult.ActiveEffects[0].GiftKey);
        Assert.Equal(WerewolfGiftDurationType.Scene, effectResult.ActiveEffects[0].DurationType);
    }

    [Fact]
    public void WaveATurnDurationGiftRegistersActiveEffect()
    {
        var state = BuildRuntimeState();
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.MetisCavar);
        state = state with { KnownGiftKeys = knownGifts };

        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.MetisCavar));

        Assert.True(activationResult.Succeeded);
        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
            WerewolfGiftIdentifiers.MetisCavar, 1));

        Assert.True(effectResult.Succeeded);
        Assert.Single(effectResult.ActiveEffects);
        Assert.Equal(WerewolfGiftDurationType.Turn, effectResult.ActiveEffects[0].DurationType);
    }

    [Fact]
    public void WaveAPermanentGiftDoesNotCreateActiveEffect()
    {
        var state = BuildRuntimeState();
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.MetisRaivaPrimordial);
        state = state with { KnownGiftKeys = knownGifts };

        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.MetisRaivaPrimordial));

        Assert.True(activationResult.Succeeded);
        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
            WerewolfGiftIdentifiers.MetisRaivaPrimordial, 0));

        Assert.True(effectResult.Succeeded);
        Assert.Empty(effectResult.ActiveEffects);
    }

    [Fact]
    public void WaveAFiannaBansheeRegistersFearAuraEffect()
    {
        var state = BuildRuntimeState();
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.FiannaUivoDaBanshee);
        state = state with { KnownGiftKeys = knownGifts };

        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.FiannaUivoDaBanshee));

        Assert.True(activationResult.Succeeded);
        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
            WerewolfGiftIdentifiers.FiannaUivoDaBanshee, 1));

        Assert.True(effectResult.Succeeded);
        Assert.Single(effectResult.ActiveEffects);
        Assert.Equal(WerewolfActiveGiftEffectKind.FearAura, effectResult.ActiveEffects[0].EffectKind);
    }

    [Fact]
    public void WaveAShadowLordsThunderousApplauseRegistersProneCondition()
    {
        var state = BuildRuntimeState();
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.ShadowLordsAplausoTrovejante);
        state = state with { KnownGiftKeys = knownGifts };

        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.ShadowLordsAplausoTrovejante));

        Assert.True(activationResult.Succeeded);
        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
            WerewolfGiftIdentifiers.ShadowLordsAplausoTrovejante, 1));

        Assert.True(effectResult.Succeeded);
        Assert.Contains(effectResult.UpdatedState!.Conditions!, c => c.ConditionKey == WerewolfConditionIdentifiers.Prone && c.IsActive);
    }

    [Fact]
    public void FearAuraReducesDiceForSocialTests()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Charisma,
            WerewolfAbilityIdentifiers.Subterfuge,
            WerewolfFormIdentifiers.Homid,
            null,
            false,
            false,
            false,
            null,
            false,
            false,
            false,
            [],
            false,
            null,
            null,
            [
                new WerewolfActiveGiftEffect("test-gift", 0, WerewolfGiftDurationType.Scene, 10, WerewolfActiveGiftEffectKind.FearAura, 2, "Line 1")
            ]);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Equal(-2, result.DicePoolModifier);
    }

    [Fact]
    public void ProneConditionFromGiftBlocksActions()
    {
        var state = BuildRuntimeState();
        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.ShadowLordsAplausoTrovejante));

        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
            WerewolfGiftIdentifiers.ShadowLordsAplausoTrovejante, 1));

        Assert.True(effectResult.Succeeded);
        var conditionResult = WerewolfConditionService.EvaluateActionAvailability(new WerewolfEvaluateActionAvailabilityRequest(
            "req-003", effectResult.UpdatedState!, effectResult.NewRuntimeStateVersion, "any-action"));

        Assert.False(conditionResult.IsAvailable);
        Assert.Equal("Prone", conditionResult.UnavailableReason);
    }

    [Fact]
    public void WaveBPersuasionActivatesAndCreatesSocialTestBonus()
    {
        var state = BuildRuntimeState();
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.HomidPersuasao);
        state = state with { KnownGiftKeys = knownGifts };

        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.HomidPersuasao));
        Assert.True(activationResult.Succeeded);
        Assert.NotNull(activationResult.ActivationDefinition);
        Assert.Equal(3, activationResult.ActivationDefinition.DicePool);
        Assert.Equal(6, activationResult.ActivationDefinition.Difficulty);
        Assert.Equal(WerewolfGiftCostType.None, activationResult.ActivationDefinition.CostType);

        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
            WerewolfGiftIdentifiers.HomidPersuasao, 1));
        Assert.True(effectResult.Succeeded);
        Assert.Single(effectResult.ActiveEffects);
        Assert.Equal(WerewolfActiveGiftEffectKind.SocialTestBonus, effectResult.ActiveEffects[0].EffectKind);
        Assert.Equal(1, effectResult.ActiveEffects[0].Magnitude);
    }

    [Fact]
    public void WaveBFitarAppliesProneConditionOnSuccess()
    {
        var state = BuildRuntimeState();
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.HomidFitar);
        state = state with { KnownGiftKeys = knownGifts };

        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.HomidFitar));
        Assert.True(activationResult.Succeeded);

        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
            WerewolfGiftIdentifiers.HomidFitar, 3));
        Assert.True(effectResult.Succeeded);
        Assert.Contains(effectResult.UpdatedState!.Conditions!, c => c.ConditionKey == WerewolfConditionIdentifiers.Prone && c.IsActive);
    }

    [Fact]
    public void WaveBFitarDoesNotApplyProneOnZeroSuccesses()
    {
        var state = BuildRuntimeState();
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.HomidFitar);
        state = state with { KnownGiftKeys = knownGifts };

        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.HomidFitar));
        Assert.True(activationResult.Succeeded);

        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
            WerewolfGiftIdentifiers.HomidFitar, 0));
        Assert.True(effectResult.Succeeded);
        Assert.DoesNotContain(effectResult.UpdatedState!.Conditions!, c => c.ConditionKey == WerewolfConditionIdentifiers.Prone && c.IsActive);
    }

    [Fact]
    public void WaveBCasuloPaysGnosisAndRegistersDamageReduction()
    {
        var state = BuildRuntimeState();
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.HomidCasulo);
        state = state with { KnownGiftKeys = knownGifts };

        var initialGnosis = state.GnosisCurrent;
        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.HomidCasulo));
        Assert.True(activationResult.Succeeded);
        Assert.Equal(initialGnosis - 1, activationResult.UpdatedState!.GnosisCurrent);

        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
            WerewolfGiftIdentifiers.HomidCasulo, 1));
        Assert.True(effectResult.Succeeded);
        Assert.Single(effectResult.ActiveEffects);
        Assert.Equal(WerewolfActiveGiftEffectKind.DamageReduction, effectResult.ActiveEffects[0].EffectKind);
        Assert.Equal(WerewolfGiftDurationType.Scene, effectResult.ActiveEffects[0].DurationType);
    }

    [Fact]
    public void WaveBRemodelarObjetoSharesHandlerAcrossOwners()
    {
        var owners = new[]
        {
            (WerewolfGiftIdentifiers.FiannaRemodelarObjeto, "Line 2207"),
            (WerewolfGiftIdentifiers.BoneGnawersRemodelarObjeto, "Line 2429")
        };

        foreach (var (giftKey, expectedSourceLocator) in owners)
        {
            var state = BuildRuntimeState();
            var knownGifts = state.KnownGiftKeys.ToList();
            knownGifts.Add(giftKey);
            state = state with { KnownGiftKeys = knownGifts };

            var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
                "req-001", state, 1, giftKey));
            Assert.True(activationResult.Succeeded);

            var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
                "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
                giftKey, 2));
            Assert.True(effectResult.Succeeded);
            Assert.Single(effectResult.ActiveEffects);
            Assert.Equal(WerewolfActiveGiftEffectKind.ObjectTransformation, effectResult.ActiveEffects[0].EffectKind);
            Assert.Equal(expectedSourceLocator, effectResult.ActiveEffects[0].SourceLocator);
            var rawPayload = effectResult.ActiveEffects[0].Payload;
            Assert.NotNull(rawPayload);
            var payload = (WerewolfObjectTransformationPayload)rawPayload;
            Assert.Equal("living-material", payload.TargetMaterial);
            Assert.Equal(3, payload.AllowedResultCategories.Count);
            Assert.Contains("tools", payload.AllowedResultCategories);
            Assert.Contains("weapons", payload.AllowedResultCategories);
            Assert.Contains("shelter", payload.AllowedResultCategories);
            Assert.True(payload.SupportsPermanentAlteration);
            Assert.True(payload.SupportsAggravatedDamage);
            Assert.Equal(2, payload.VariableDurationTurns);
        }
    }

    [Fact]
    public void WaveBInquietacaoProducesTwoTypedEffects()
    {
        var state = BuildRuntimeState();
        var knownGifts = state.KnownGiftKeys.ToList();
        knownGifts.Add(WerewolfGiftIdentifiers.HomidInquietacao);
        state = state with { KnownGiftKeys = knownGifts };

        var activationResult = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-001", state, 1, WerewolfGiftIdentifiers.HomidInquietacao));
        Assert.True(activationResult.Succeeded);

        var effectResult = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-002", activationResult.UpdatedState!, activationResult.NewRuntimeStateVersion,
            WerewolfGiftIdentifiers.HomidInquietacao, 1));
        Assert.True(effectResult.Succeeded);
        Assert.Equal(2, effectResult.ActiveEffects.Count);

        var rageEffect = effectResult.ActiveEffects.First(e => e.EffectKind == WerewolfActiveGiftEffectKind.RageRecoveryPenalty);
        Assert.Equal(WerewolfGiftIdentifiers.HomidInquietacao, rageEffect.GiftKey);
        Assert.Equal(1, rageEffect.Magnitude);
        Assert.Equal("Line 1758", rageEffect.SourceLocator);
        var rageRawPayload = rageEffect.Payload;
        Assert.NotNull(rageRawPayload);
        var ragePayload = (WerewolfRageRecoveryPenaltyPayload)rageRawPayload;
        Assert.Equal(1, ragePayload.PenaltyAmount);

        var extendedEffect = effectResult.ActiveEffects.First(e => e.EffectKind == WerewolfActiveGiftEffectKind.ExtendedTestDifficultyModifier);
        Assert.Equal(WerewolfGiftIdentifiers.HomidInquietacao, extendedEffect.GiftKey);
        Assert.Equal(1, extendedEffect.Magnitude);
        Assert.Equal("Line 1758", extendedEffect.SourceLocator);
        var extendedRawPayload = extendedEffect.Payload;
        Assert.NotNull(extendedRawPayload);
        var extendedPayload = (WerewolfExtendedTestDifficultyPayload)extendedRawPayload;
        Assert.Equal(1, extendedPayload.DifficultyIncrease);
        Assert.Equal("prolonged-actions", extendedPayload.Scope);
    }

    [Fact]
    public void WaveBCatalogCountReflectsWaveBImplementation()
    {
        var allKeys = WerewolfGiftCatalog.AllDefinitions.Select(g => g.GiftKey).ToList();
        Assert.Equal(93, allKeys.Count);
    }

    [Fact]
    public void WaveBExistingGiftsRemainUnchanged()
    {
        var waveAGiftKeys = new[]
        {
            WerewolfGiftIdentifiers.HomidSimularOdorDeHomem,
            WerewolfGiftIdentifiers.HomidPerturbarTecnologia,
            WerewolfGiftIdentifiers.MetisRaivaPrimordial,
            WerewolfGiftIdentifiers.MetisSentirAWyrm,
            WerewolfGiftIdentifiers.MetisCavar,
            WerewolfGiftIdentifiers.MetisOlhosDeGato,
            WerewolfGiftIdentifiers.LupusSentidosAgucados,
            WerewolfGiftIdentifiers.RagabashEmbacamentoDaPropriaForma,
            WerewolfGiftIdentifiers.RagabashSimularOCheiroDeAguaCorrente,
            WerewolfGiftIdentifiers.RagabashInduzirEsquecimento,
            WerewolfGiftIdentifiers.TheurgeSentirAWyrm,
            WerewolfGiftIdentifiers.PhilodoxFaroParaAFormaVerdadeira,
            WerewolfGiftIdentifiers.PhilodoxVerdadeDeGaia,
            WerewolfGiftIdentifiers.PhilodoxReiDosAnimais,
            WerewolfGiftIdentifiers.GalliardComunicacaoComAnimais,
            WerewolfGiftIdentifiers.GalliardDistracoes,
            WerewolfGiftIdentifiers.AhrounGarrasAfiadas,
            WerewolfGiftIdentifiers.AhrounInspiracao,
            WerewolfGiftIdentifiers.AhrounEspiritoDaBatalha,
            WerewolfGiftIdentifiers.AhrounMedoVerdadeiro,
            WerewolfGiftIdentifiers.AhrounSentirAPrata,
            WerewolfGiftIdentifiers.GlassWalkersSentidosCiberneticos,
            WerewolfGiftIdentifiers.GlassWalkersSobrecargaDeEnergia,
            WerewolfGiftIdentifiers.GetOfFenrisDeterAFugaDosCovardes,
            WerewolfGiftIdentifiers.GetOfFenrisRugidoDoPredador,
            WerewolfGiftIdentifiers.FiannaUivoDaBanshee,
            WerewolfGiftIdentifiers.ChildrenOfGaiaResistenciaADor,
            WerewolfGiftIdentifiers.ChildrenOfGaiaAcalmar,
            WerewolfGiftIdentifiers.ChildrenOfGaiaArmaduraDeLuna,
            WerewolfGiftIdentifiers.BlackFuriesMaldicaoDeEolo,
            WerewolfGiftIdentifiers.BlackFuriesSentirAPresa,
            WerewolfGiftIdentifiers.RedTalonsMenteAnimal,
            WerewolfGiftIdentifiers.RedTalonsSentirAPresa,
            WerewolfGiftIdentifiers.SilentStridersSentirAWyrm,
            WerewolfGiftIdentifiers.SilentStridersGerarIgnorancia,
            WerewolfGiftIdentifiers.SilentStridersResistenciaDeMensageiro,
            WerewolfGiftIdentifiers.SilverFangsSentirAWyrm,
            WerewolfGiftIdentifiers.SilverFangsArmaduraDeLuna,
            WerewolfGiftIdentifiers.SilverFangsEmpatia,
            WerewolfGiftIdentifiers.BoneGnawersGerarIgnorancia,
            WerewolfGiftIdentifiers.BoneGnawersOdorRepugnante,
            WerewolfGiftIdentifiers.ShadowLordsAplausoTrovejante,
            WerewolfGiftIdentifiers.ShadowLordsArmaduraDeLuna,
            WerewolfGiftIdentifiers.UktenaEspiritoDoPassaro,
            WerewolfGiftIdentifiers.UktenaEspiritoDoPeixe,
            WerewolfGiftIdentifiers.WendigoResistenciaADor,
            WerewolfGiftIdentifiers.WendigoVentoCortante,
            WerewolfGiftIdentifiers.HomidMasterOfFire,
            WerewolfGiftIdentifiers.MetisCreateElement,
            WerewolfGiftIdentifiers.LupusHareLeap,
            WerewolfGiftIdentifiers.RagabashOpenSeal,
            WerewolfGiftIdentifiers.TheurgeSpiritSpeech,
            WerewolfGiftIdentifiers.PhilodoxResistPain,
            WerewolfGiftIdentifiers.GalliardBeastSpeech,
            WerewolfGiftIdentifiers.AhrounFallingTouch,
            WerewolfGiftIdentifiers.GlassWalkersControlSimpleMachine,
            WerewolfGiftIdentifiers.GlassWalkersDiagnostics,
            WerewolfGiftIdentifiers.GlassWalkersTrickShot,
            WerewolfGiftIdentifiers.GetOfFenrisRazorClaws,
            WerewolfGiftIdentifiers.GetOfFenrisResistPain,
            WerewolfGiftIdentifiers.GetOfFenrisVisageOfFenris,
            WerewolfGiftIdentifiers.FiannaFaerieLight,
            WerewolfGiftIdentifiers.FiannaPersuasion,
            WerewolfGiftIdentifiers.FiannaResistToxin,
            WerewolfGiftIdentifiers.ChildrenOfGaiaMercy,
            WerewolfGiftIdentifiers.ChildrenOfGaiaMothersTouch,
            WerewolfGiftIdentifiers.BlackFuriesBreathOfTheWyrm,
            WerewolfGiftIdentifiers.BlackFuriesHeightenedSenses,
            WerewolfGiftIdentifiers.BlackFuriesSenseWyrm,
            WerewolfGiftIdentifiers.RedTalonsBeastSpeech,
            WerewolfGiftIdentifiers.RedTalonsWolfAtTheDoor,
            WerewolfGiftIdentifiers.RedTalonsScentOfRunningWater,
            WerewolfGiftIdentifiers.SilentStridersSilence,
            WerewolfGiftIdentifiers.SilentStridersSpeedOfThought,
            WerewolfGiftIdentifiers.SilverFangsLambentFlame,
            WerewolfGiftIdentifiers.SilverFangsFalconsGrasp,
            WerewolfGiftIdentifiers.BoneGnawersCooking,
            WerewolfGiftIdentifiers.BoneGnawersStickyFingers,
            WerewolfGiftIdentifiers.ShadowLordsSeizingTheEdge,
            WerewolfGiftIdentifiers.ShadowLordsAuraOfConfidence,
            WerewolfGiftIdentifiers.ShadowLordsFatalFlaw,
            WerewolfGiftIdentifiers.UktenaSpiritSpeech,
            WerewolfGiftIdentifiers.UktenaShroud,
            WerewolfGiftIdentifiers.UktenaSenseMagic,
            WerewolfGiftIdentifiers.WendigoCamouflage,
            WerewolfGiftIdentifiers.WendigoCallTheBreeze
        };

        foreach (var giftKey in waveAGiftKeys)
        {
            var definition = WerewolfGiftCatalog.Get(giftKey);
            Assert.NotNull(definition);
            Assert.Equal(giftKey, definition.GiftKey);
            Assert.False(string.IsNullOrWhiteSpace(definition.SourceLocator));
            Assert.StartsWith("Line ", definition.SourceLocator);
        }
    }
}






