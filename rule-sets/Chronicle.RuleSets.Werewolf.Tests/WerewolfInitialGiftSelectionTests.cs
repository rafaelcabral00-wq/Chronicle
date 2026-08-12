using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfInitialGiftSelectionTests
{
    [Theory]
    [InlineData(WerewolfInitialGiftSource.Race, WerewolfRaceIdentifiers.Homid, null, null, WerewolfInitialGiftIdentifiers.HomidMasterOfFire)]
    [InlineData(WerewolfInitialGiftSource.Race, WerewolfRaceIdentifiers.Metis, null, null, WerewolfInitialGiftIdentifiers.MetisCreateElement)]
    [InlineData(WerewolfInitialGiftSource.Race, WerewolfRaceIdentifiers.Lupus, null, null, WerewolfInitialGiftIdentifiers.LupusHareLeap)]
    [InlineData(WerewolfInitialGiftSource.Auspice, null, WerewolfAuspiceIdentifiers.Ragabash, null, WerewolfInitialGiftIdentifiers.RagabashOpenSeal)]
    [InlineData(WerewolfInitialGiftSource.Auspice, null, WerewolfAuspiceIdentifiers.Theurge, null, WerewolfInitialGiftIdentifiers.TheurgeSpiritSpeech)]
    [InlineData(WerewolfInitialGiftSource.Auspice, null, WerewolfAuspiceIdentifiers.Philodox, null, WerewolfInitialGiftIdentifiers.PhilodoxResistPain)]
    [InlineData(WerewolfInitialGiftSource.Auspice, null, WerewolfAuspiceIdentifiers.Galliard, null, WerewolfInitialGiftIdentifiers.GalliardBeastSpeech)]
    [InlineData(WerewolfInitialGiftSource.Auspice, null, WerewolfAuspiceIdentifiers.Ahroun, null, WerewolfInitialGiftIdentifiers.AhrounFallingTouch)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.GlassWalkers, WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.GlassWalkers, WerewolfInitialGiftIdentifiers.GlassWalkersDiagnostics)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.GlassWalkers, WerewolfInitialGiftIdentifiers.GlassWalkersTrickShot)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.GetOfFenris, WerewolfInitialGiftIdentifiers.GetOfFenrisRazorClaws)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.GetOfFenris, WerewolfInitialGiftIdentifiers.GetOfFenrisResistPain)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.GetOfFenris, WerewolfInitialGiftIdentifiers.GetOfFenrisVisageOfFenris)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.Fianna, WerewolfInitialGiftIdentifiers.FiannaFaerieLight)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.Fianna, WerewolfInitialGiftIdentifiers.FiannaPersuasion)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.Fianna, WerewolfInitialGiftIdentifiers.FiannaResistToxin)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.ChildrenOfGaia, WerewolfInitialGiftIdentifiers.ChildrenOfGaiaMercy)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.ChildrenOfGaia, WerewolfInitialGiftIdentifiers.ChildrenOfGaiaMothersTouch)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.BlackFuries, WerewolfInitialGiftIdentifiers.BlackFuriesBreathOfTheWyrm)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.BlackFuries, WerewolfInitialGiftIdentifiers.BlackFuriesHeightenedSenses)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.BlackFuries, WerewolfInitialGiftIdentifiers.BlackFuriesSenseWyrm)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.RedTalons, WerewolfInitialGiftIdentifiers.RedTalonsBeastSpeech)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.RedTalons, WerewolfInitialGiftIdentifiers.RedTalonsWolfAtTheDoor)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.RedTalons, WerewolfInitialGiftIdentifiers.RedTalonsScentOfRunningWater)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.SilentStriders, WerewolfInitialGiftIdentifiers.SilentStridersSilence)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.SilentStriders, WerewolfInitialGiftIdentifiers.SilentStridersSpeedOfThought)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.SilverFangs, WerewolfInitialGiftIdentifiers.SilverFangsLambentFlame)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.SilverFangs, WerewolfInitialGiftIdentifiers.SilverFangsFalconsGrasp)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.BoneGnawers, WerewolfInitialGiftIdentifiers.BoneGnawersCooking)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.BoneGnawers, WerewolfInitialGiftIdentifiers.BoneGnawersStickyFingers)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.ShadowLords, WerewolfInitialGiftIdentifiers.ShadowLordsSeizingTheEdge)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.ShadowLords, WerewolfInitialGiftIdentifiers.ShadowLordsAuraOfConfidence)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.ShadowLords, WerewolfInitialGiftIdentifiers.ShadowLordsFatalFlaw)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.Uktena, WerewolfInitialGiftIdentifiers.UktenaSpiritSpeech)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.Uktena, WerewolfInitialGiftIdentifiers.UktenaShroud)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.Uktena, WerewolfInitialGiftIdentifiers.UktenaSenseMagic)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.Wendigo, WerewolfInitialGiftIdentifiers.WendigoCamouflage)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.Wendigo, WerewolfInitialGiftIdentifiers.WendigoCallTheBreeze)]
    public void SelectsEveryApprovedCurrentSliceInitialGift(
        WerewolfInitialGiftSource source,
        string? race,
        string? auspice,
        string? tribe,
        string giftId)
    {
        var draft = Draft() with
        {
            Race = race,
            Auspice = auspice,
            Tribe = tribe
        };

        var result = Select(draft, source, giftId);

        Assert.True(result.Succeeded);
        Assert.Equal(draft.DraftVersion + 1, result.Draft?.DraftVersion);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfInitialGiftSelectionErrorCode.GiftSelected);
        Assert.Equal(giftId, SelectedGift(result.Draft!, source));
        Assert.Empty(result.Draft?.Gifts ?? []);
    }

    [Theory]
    [InlineData(WerewolfAuspiceIdentifiers.Ragabash, WerewolfInitialGiftIdentifiers.RagabashOpenSeal)]
    [InlineData(WerewolfAuspiceIdentifiers.Theurge, WerewolfInitialGiftIdentifiers.TheurgeSpiritSpeech)]
    [InlineData(WerewolfAuspiceIdentifiers.Philodox, WerewolfInitialGiftIdentifiers.PhilodoxResistPain)]
    [InlineData(WerewolfAuspiceIdentifiers.Galliard, WerewolfInitialGiftIdentifiers.GalliardBeastSpeech)]
    [InlineData(WerewolfAuspiceIdentifiers.Ahroun, WerewolfInitialGiftIdentifiers.AhrounFallingTouch)]
    public void SelectsOneExecutableInitialGiftForEverySupportedAuspice(string auspiceId, string giftId)
    {
        var draft = Draft() with { Auspice = auspiceId };

        var result = Select(draft, WerewolfInitialGiftSource.Auspice, giftId);

        Assert.True(result.Succeeded);
        Assert.Equal(giftId, result.Draft?.AuspiceGift);
    }

    [Theory]
    [InlineData(WerewolfInitialGiftSource.Race, WerewolfInitialGiftIdentifiers.HomidMasterOfFire)]
    [InlineData(WerewolfInitialGiftSource.Auspice, WerewolfInitialGiftIdentifiers.RagabashOpenSeal)]
    [InlineData(WerewolfInitialGiftSource.Tribe, WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine)]
    public void RequiresCorrespondingClassificationBeforeGift(WerewolfInitialGiftSource source, string giftId)
    {
        var result = Select(Draft(), source, giftId);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfInitialGiftSelectionErrorCode.MissingRequiredClassification);
    }

    [Theory]
    [InlineData(WerewolfInitialGiftSource.Race, WerewolfInitialGiftIdentifiers.RagabashOpenSeal, WerewolfInitialGiftSelectionErrorCode.WrongSource)]
    [InlineData(WerewolfInitialGiftSource.Auspice, WerewolfInitialGiftIdentifiers.HomidMasterOfFire, WerewolfInitialGiftSelectionErrorCode.WrongSource)]
    [InlineData(WerewolfInitialGiftSource.Tribe, "gift.tribe.black-furies.sense-wyrm", WerewolfInitialGiftSelectionErrorCode.WrongSource)]
    [InlineData(WerewolfInitialGiftSource.Race, "gift.race.homid.level-2-test", WerewolfInitialGiftSelectionErrorCode.WrongLevel)]
    [InlineData(WerewolfInitialGiftSource.Race, "gift.unknown", WerewolfInitialGiftSelectionErrorCode.UnknownGift)]
    [InlineData(WerewolfInitialGiftSource.Race, " gift.race.homid.master-of-fire", WerewolfInitialGiftSelectionErrorCode.MalformedGift)]
    [InlineData(WerewolfInitialGiftSource.Race, "gift.race.homid.master of fire", WerewolfInitialGiftSelectionErrorCode.MalformedGift)]
    public void RejectsWrongInvalidMalformedAndOutOfScopeGifts(
        WerewolfInitialGiftSource source,
        string giftId,
        WerewolfInitialGiftSelectionErrorCode expected)
    {
        var draft = Draft() with
        {
            Race = WerewolfRaceIdentifiers.Homid,
            Auspice = WerewolfAuspiceIdentifiers.Ragabash,
            Tribe = WerewolfTribeIdentifiers.GlassWalkers
        };

        var result = Select(draft, source, giftId);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == expected);
    }

    [Fact]
    public void RejectsStaleVersion()
    {
        var draft = Draft() with { Race = WerewolfRaceIdentifiers.Homid };

        var result = WerewolfInitialGiftSelectionService.SelectGift(new WerewolfInitialGiftSelectionRequest(
            draft,
            0,
            WerewolfInitialGiftSource.Race,
            WerewolfInitialGiftIdentifiers.HomidMasterOfFire));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfInitialGiftSelectionErrorCode.StaleDraftVersion);
    }

    [Fact]
    public void ReplacesWithinSameSourceAndPreservesUnrelatedState()
    {
        var draft = Draft() with
        {
            Race = WerewolfRaceIdentifiers.Homid,
            Auspice = WerewolfAuspiceIdentifiers.Ragabash,
            Tribe = WerewolfTribeIdentifiers.GlassWalkers,
            RaceGift = "old-race-gift",
            AuspiceGift = WerewolfInitialGiftIdentifiers.RagabashOpenSeal,
            TribeGift = WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine
        };

        var result = Select(draft, WerewolfInitialGiftSource.Race, WerewolfInitialGiftIdentifiers.HomidMasterOfFire);

        Assert.Equal(WerewolfInitialGiftIdentifiers.HomidMasterOfFire, result.Draft?.RaceGift);
        Assert.Equal(WerewolfInitialGiftIdentifiers.RagabashOpenSeal, result.Draft?.AuspiceGift);
        Assert.Equal(WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine, result.Draft?.TribeGift);
        Assert.Equal(draft.Auspice, result.Draft?.Auspice);
        Assert.Equal(draft.Tribe, result.Draft?.Tribe);
        Assert.NotSame(draft.RequiredNextSteps, result.Draft?.RequiredNextSteps);
    }

    [Fact]
    public void ClassificationChangesClearOnlyIncompatibleGift()
    {
        var draft = Draft() with
        {
            Race = WerewolfRaceIdentifiers.Homid,
            Auspice = WerewolfAuspiceIdentifiers.Ragabash,
            Tribe = WerewolfTribeIdentifiers.GlassWalkers,
            RaceGift = WerewolfInitialGiftIdentifiers.HomidMasterOfFire,
            AuspiceGift = WerewolfInitialGiftIdentifiers.RagabashOpenSeal,
            TribeGift = WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine,
            RequiredNextSteps = Array.AsReadOnly(["allocate-abilities"])
        };

        var result = WerewolfRaceSelectionService.SelectRace(new WerewolfRaceSelectionRequest(draft, draft.DraftVersion, WerewolfRaceIdentifiers.Metis));

        Assert.True(result.Succeeded);
        Assert.Null(result.Draft?.RaceGift);
        Assert.Equal(WerewolfInitialGiftIdentifiers.RagabashOpenSeal, result.Draft?.AuspiceGift);
        Assert.Equal(WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine, result.Draft?.TribeGift);
        Assert.Contains(WerewolfInitialGiftSelectionService.SelectRaceGiftStep, result.Draft?.RequiredNextSteps ?? []);
        Assert.Contains(WerewolfInitialGiftSelectionService.SelectInitialGiftsStep, result.Draft?.RequiredNextSteps ?? []);
    }

    [Fact]
    public void CompleteInitialGiftSelectionClearsAggregateNextStepDeterministically()
    {
        var draft = Draft() with
        {
            Race = WerewolfRaceIdentifiers.Homid,
            Auspice = WerewolfAuspiceIdentifiers.Ragabash,
            Tribe = WerewolfTribeIdentifiers.GlassWalkers
        };

        var raceGift = Select(draft, WerewolfInitialGiftSource.Race, WerewolfInitialGiftIdentifiers.HomidMasterOfFire).Draft!;
        var auspiceGift = Select(raceGift, WerewolfInitialGiftSource.Auspice, WerewolfInitialGiftIdentifiers.RagabashOpenSeal).Draft!;
        var tribeGift = Select(auspiceGift, WerewolfInitialGiftSource.Tribe, WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine).Draft!;

        Assert.DoesNotContain(WerewolfInitialGiftSelectionService.SelectRaceGiftStep, tribeGift.RequiredNextSteps);
        Assert.DoesNotContain(WerewolfInitialGiftSelectionService.SelectAuspiceGiftStep, tribeGift.RequiredNextSteps);
        Assert.DoesNotContain(WerewolfInitialGiftSelectionService.SelectTribeGiftStep, tribeGift.RequiredNextSteps);
        Assert.DoesNotContain(WerewolfInitialGiftSelectionService.SelectInitialGiftsStep, tribeGift.RequiredNextSteps);
        Assert.Equal(tribeGift.RequiredNextSteps.Order(StringComparer.Ordinal), tribeGift.RequiredNextSteps);
    }

    [Fact]
    public void RuntimeFlowSelectsAllInitialGiftSources()
    {
        var registry = RuntimeRegistry();
        var created = registry.Execute(Request(WerewolfReferenceRuntime.CreateCharacterOperation, new Dictionary<string, string>(StringComparer.Ordinal) { ["requestId"] = "request-001" }));
        var race = registry.Execute(Request(WerewolfReferenceRuntime.SelectRaceOperation, new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["draftId"] = created.Outputs["draftId"],
            ["draftVersion"] = created.Outputs["draftVersion"],
            ["expectedDraftVersion"] = created.Outputs["draftVersion"],
            ["raceId"] = WerewolfRaceIdentifiers.Homid
        }));
        var auspice = registry.Execute(Request(WerewolfReferenceRuntime.SelectAuspiceOperation, new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["draftId"] = race.Outputs["draftId"],
            ["draftVersion"] = race.Outputs["draftVersion"],
            ["expectedDraftVersion"] = race.Outputs["draftVersion"],
            ["currentRace"] = race.Outputs["raceId"],
            ["auspiceId"] = WerewolfAuspiceIdentifiers.Ragabash
        }));
        var tribe = registry.Execute(Request(WerewolfReferenceRuntime.SelectTribeOperation, new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["draftId"] = auspice.Outputs["draftId"],
            ["draftVersion"] = auspice.Outputs["draftVersion"],
            ["expectedDraftVersion"] = auspice.Outputs["draftVersion"],
            ["currentRace"] = auspice.Outputs["raceId"],
            ["currentAuspice"] = auspice.Outputs["auspiceId"],
            ["tribeId"] = WerewolfTribeIdentifiers.GlassWalkers
        }));

        var raceGift = registry.Execute(Request(WerewolfReferenceRuntime.SelectRaceGiftOperation, GiftInputs(tribe, WerewolfInitialGiftIdentifiers.HomidMasterOfFire)));
        var auspiceGift = registry.Execute(Request(WerewolfReferenceRuntime.SelectAuspiceGiftOperation, GiftInputs(raceGift, WerewolfInitialGiftIdentifiers.RagabashOpenSeal)));
        var tribeGift = registry.Execute(Request(WerewolfReferenceRuntime.SelectTribeGiftOperation, GiftInputs(auspiceGift, WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine)));

        Assert.True(tribeGift.Succeeded);
        Assert.Equal(WerewolfInitialGiftIdentifiers.HomidMasterOfFire, tribeGift.Outputs["raceGiftId"]);
        Assert.Equal(WerewolfInitialGiftIdentifiers.RagabashOpenSeal, tribeGift.Outputs["auspiceGiftId"]);
        Assert.Equal(WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine, tribeGift.Outputs["tribeGiftId"]);
        Assert.DoesNotContain(WerewolfInitialGiftSelectionService.SelectInitialGiftsStep, tribeGift.Outputs["nextSteps"], StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(AllClassificationPaths))]
    public void EveryRaceAuspiceTribePathPassesClassificationAndInitialGiftPhase(string raceId, string raceGiftId, string auspiceId, string auspiceGiftId)
    {
        var registry = RuntimeRegistry();
        var created = registry.Execute(Request(WerewolfReferenceRuntime.CreateCharacterOperation, new Dictionary<string, string>(StringComparer.Ordinal) { ["requestId"] = $"request-{raceId}-{auspiceId}" }));
        var race = registry.Execute(Request(WerewolfReferenceRuntime.SelectRaceOperation, new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["draftId"] = created.Outputs["draftId"],
            ["draftVersion"] = created.Outputs["draftVersion"],
            ["expectedDraftVersion"] = created.Outputs["draftVersion"],
            ["raceId"] = raceId
        }));

        var afterDeformity = race;
        if (StringComparer.Ordinal.Equals(raceId, WerewolfRaceIdentifiers.Metis))
        {
            afterDeformity = registry.Execute(Request(WerewolfReferenceRuntime.SelectMetisDeformityOperation, new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["draftId"] = race.Outputs["draftId"],
                ["draftVersion"] = race.Outputs["draftVersion"],
                ["expectedDraftVersion"] = race.Outputs["draftVersion"],
                ["currentRace"] = race.Outputs["raceId"],
                ["deformityId"] = WerewolfMetisDeformityIdentifiers.Horns
            }));
        }

        var auspice = registry.Execute(Request(WerewolfReferenceRuntime.SelectAuspiceOperation, new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["draftId"] = afterDeformity.Outputs["draftId"],
            ["draftVersion"] = afterDeformity.Outputs["draftVersion"],
            ["expectedDraftVersion"] = afterDeformity.Outputs["draftVersion"],
            ["currentRace"] = afterDeformity.Outputs["raceId"],
            ["currentMetisDeformity"] = afterDeformity.Outputs["metisDeformityId"],
            ["auspiceId"] = auspiceId
        }));
        var tribe = registry.Execute(Request(WerewolfReferenceRuntime.SelectTribeOperation, new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["draftId"] = auspice.Outputs["draftId"],
            ["draftVersion"] = auspice.Outputs["draftVersion"],
            ["expectedDraftVersion"] = auspice.Outputs["draftVersion"],
            ["currentRace"] = auspice.Outputs["raceId"],
            ["currentMetisDeformity"] = auspice.Outputs["metisDeformityId"],
            ["currentAuspice"] = auspice.Outputs["auspiceId"],
            ["tribeId"] = WerewolfTribeIdentifiers.GlassWalkers
        }));

        var raceGift = registry.Execute(Request(WerewolfReferenceRuntime.SelectRaceGiftOperation, GiftInputs(tribe, raceGiftId)));
        var auspiceGift = registry.Execute(Request(WerewolfReferenceRuntime.SelectAuspiceGiftOperation, GiftInputs(raceGift, auspiceGiftId)));
        var tribeGift = registry.Execute(Request(WerewolfReferenceRuntime.SelectTribeGiftOperation, GiftInputs(auspiceGift, WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine)));

        Assert.True(tribeGift.Succeeded);
        Assert.Equal(raceId, tribeGift.Outputs["raceId"]);
        Assert.Equal(auspiceId, tribeGift.Outputs["auspiceId"]);
        Assert.Equal(WerewolfTribeIdentifiers.GlassWalkers, tribeGift.Outputs["tribeId"]);
        Assert.Equal(raceGiftId, tribeGift.Outputs["raceGiftId"]);
        Assert.Equal(auspiceGiftId, tribeGift.Outputs["auspiceGiftId"]);
        Assert.Equal(WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine, tribeGift.Outputs["tribeGiftId"]);
        Assert.DoesNotContain(WerewolfInitialGiftSelectionService.SelectInitialGiftsStep, tribeGift.Outputs["nextSteps"], StringComparison.Ordinal);
    }

    [Fact]
    public void InitialGiftSelectionHasNoForbiddenDependencies()
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "rule-sets", "Chronicle.RuleSets.Werewolf", "CharacterCreation", "WerewolfInitialGiftSelection.cs"));
        var forbidden = new[] { "Chronicle.Persistence", "Chronicle.Presentation", "OpenAI", "HttpClient", "DbContext", "File.", "Directory.", "Random", "Campaign" };

        Assert.DoesNotContain(forbidden, token => source.Contains(token, StringComparison.Ordinal));
    }

    private static WerewolfInitialGiftSelectionResult Select(WerewolfInitializedCharacterState draft, WerewolfInitialGiftSource source, string giftId)
    {
        return WerewolfInitialGiftSelectionService.SelectGift(new WerewolfInitialGiftSelectionRequest(draft, draft.DraftVersion, source, giftId));
    }

    private static string? SelectedGift(WerewolfInitializedCharacterState draft, WerewolfInitialGiftSource source)
    {
        return source switch
        {
            WerewolfInitialGiftSource.Race => draft.RaceGift,
            WerewolfInitialGiftSource.Auspice => draft.AuspiceGift,
            WerewolfInitialGiftSource.Tribe => draft.TribeGift,
            _ => null
        };
    }

    private static WerewolfInitializedCharacterState Draft()
    {
        return WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity("draft-001"), 1);
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

    private static RuleSetOperationRequest Request(string operationKey, IReadOnlyDictionary<string, string> inputs)
    {
        return new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            operationKey,
            inputs);
    }

    public static IEnumerable<object[]> AllClassificationPaths()
    {
        var races = new[]
        {
            (WerewolfRaceIdentifiers.Homid, WerewolfInitialGiftIdentifiers.HomidMasterOfFire),
            (WerewolfRaceIdentifiers.Metis, WerewolfInitialGiftIdentifiers.MetisCreateElement),
            (WerewolfRaceIdentifiers.Lupus, WerewolfInitialGiftIdentifiers.LupusHareLeap)
        };
        var auspices = new[]
        {
            (WerewolfAuspiceIdentifiers.Ragabash, WerewolfInitialGiftIdentifiers.RagabashOpenSeal),
            (WerewolfAuspiceIdentifiers.Theurge, WerewolfInitialGiftIdentifiers.TheurgeSpiritSpeech),
            (WerewolfAuspiceIdentifiers.Philodox, WerewolfInitialGiftIdentifiers.PhilodoxResistPain),
            (WerewolfAuspiceIdentifiers.Galliard, WerewolfInitialGiftIdentifiers.GalliardBeastSpeech),
            (WerewolfAuspiceIdentifiers.Ahroun, WerewolfInitialGiftIdentifiers.AhrounFallingTouch)
        };

        foreach (var race in races)
        {
            foreach (var auspice in auspices)
            {
                yield return [race.Item1, race.Item2, auspice.Item1, auspice.Item2];
            }
        }
    }

    private static RuleSetRuntimeRegistry RuntimeRegistry()
    {
        var discovery = RuleSetPackageSourceDiscoveryService.Discover(new RuleSetPackageSourceDiscoveryRequest([Path.Combine(FindRepositoryRoot(), "rule-sets")]));
        var registration = RuleSetPackageRegistrationService.Register(new RuleSetPackageRegistrationRequest(discovery.ValidatedPackages, 1));
        return RuleSetRuntimeRegistrationService.Register(new RuleSetRuntimeRegistrationRequest(registration.Catalog, [new WerewolfReferenceRuntime(new TestIdentitySource())])).Registry;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Chronicle.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not find repository root.");
    }

    private sealed class TestIdentitySource : IWerewolfCharacterDraftIdentitySource
    {
        public WerewolfCharacterDraftIdentity CreateDraftIdentity(WerewolfCreateCharacterRequest request)
        {
            return new WerewolfCharacterDraftIdentity("runtime-draft-001");
        }
    }
}
