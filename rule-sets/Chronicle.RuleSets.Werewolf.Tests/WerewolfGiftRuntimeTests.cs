using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfGiftRuntimeTests
{
    [Theory]
    [InlineData(WerewolfGiftIdentifiers.HomidMasterOfFire)]
    [InlineData(WerewolfGiftIdentifiers.MetisCreateElement)]
    [InlineData(WerewolfGiftIdentifiers.LupusHareLeap)]
    [InlineData(WerewolfGiftIdentifiers.RagabashOpenSeal)]
    [InlineData(WerewolfGiftIdentifiers.TheurgeSpiritSpeech)]
    [InlineData(WerewolfGiftIdentifiers.PhilodoxResistPain)]
    [InlineData(WerewolfGiftIdentifiers.GalliardBeastSpeech)]
    [InlineData(WerewolfGiftIdentifiers.AhrounFallingTouch)]
    [InlineData(WerewolfGiftIdentifiers.GlassWalkersControlSimpleMachine)]
    [InlineData(WerewolfGiftIdentifiers.GlassWalkersDiagnostics)]
    [InlineData(WerewolfGiftIdentifiers.GlassWalkersTrickShot)]
    [InlineData(WerewolfGiftIdentifiers.GetOfFenrisRazorClaws)]
    [InlineData(WerewolfGiftIdentifiers.GetOfFenrisResistPain)]
    [InlineData(WerewolfGiftIdentifiers.GetOfFenrisVisageOfFenris)]
    [InlineData(WerewolfGiftIdentifiers.FiannaFaerieLight)]
    [InlineData(WerewolfGiftIdentifiers.FiannaPersuasion)]
    [InlineData(WerewolfGiftIdentifiers.FiannaResistToxin)]
    [InlineData(WerewolfGiftIdentifiers.ChildrenOfGaiaMercy)]
    [InlineData(WerewolfGiftIdentifiers.ChildrenOfGaiaMothersTouch)]
    [InlineData(WerewolfGiftIdentifiers.BlackFuriesBreathOfTheWyrm)]
    [InlineData(WerewolfGiftIdentifiers.BlackFuriesHeightenedSenses)]
    [InlineData(WerewolfGiftIdentifiers.BlackFuriesSenseWyrm)]
    [InlineData(WerewolfGiftIdentifiers.RedTalonsBeastSpeech)]
    [InlineData(WerewolfGiftIdentifiers.RedTalonsWolfAtTheDoor)]
    [InlineData(WerewolfGiftIdentifiers.RedTalonsScentOfRunningWater)]
    [InlineData(WerewolfGiftIdentifiers.SilentStridersSilence)]
    [InlineData(WerewolfGiftIdentifiers.SilentStridersSpeedOfThought)]
    [InlineData(WerewolfGiftIdentifiers.SilverFangsLambentFlame)]
    [InlineData(WerewolfGiftIdentifiers.SilverFangsFalconsGrasp)]
    [InlineData(WerewolfGiftIdentifiers.BoneGnawersCooking)]
    [InlineData(WerewolfGiftIdentifiers.BoneGnawersStickyFingers)]
    [InlineData(WerewolfGiftIdentifiers.ShadowLordsSeizingTheEdge)]
    [InlineData(WerewolfGiftIdentifiers.ShadowLordsAuraOfConfidence)]
    [InlineData(WerewolfGiftIdentifiers.ShadowLordsFatalFlaw)]
    [InlineData(WerewolfGiftIdentifiers.UktenaSpiritSpeech)]
    [InlineData(WerewolfGiftIdentifiers.UktenaShroud)]
    [InlineData(WerewolfGiftIdentifiers.UktenaSenseMagic)]
    [InlineData(WerewolfGiftIdentifiers.WendigoCamouflage)]
    [InlineData(WerewolfGiftIdentifiers.WendigoCallTheBreeze)]
    public void CatalogReturnsDefinitionForEveryInitialGift(string giftKey)
    {
        var definition = WerewolfGiftCatalog.Get(giftKey);

        Assert.NotNull(definition);
        Assert.Equal(giftKey, definition.GiftKey);
        Assert.Equal(1, definition.Level);
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
    public void GiftCatalogAllInitialGiftsHaveLevelOne()
    {
        foreach (var definition in WerewolfGiftCatalog.AllDefinitions)
        {
            Assert.Equal(1, definition.Level);
        }
    }

    [Fact]
    public void GiftCatalogHasExpectedCount()
    {
        Assert.Equal(39, WerewolfGiftCatalog.AllDefinitions.Count);
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
    public void AllInitialGiftsAreCatalogued()
    {
        Assert.Equal(39, WerewolfGiftCatalog.AllDefinitions.Count);
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
}






