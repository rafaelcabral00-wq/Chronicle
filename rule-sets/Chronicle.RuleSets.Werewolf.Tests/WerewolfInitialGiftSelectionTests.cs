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
    [InlineData(WerewolfInitialGiftSource.Auspice, null, WerewolfAuspiceIdentifiers.Philodox, null, WerewolfInitialGiftIdentifiers.PhilodoxResistPain)]
    [InlineData(WerewolfInitialGiftSource.Tribe, null, null, WerewolfTribeIdentifiers.GlassWalkers, WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine)]
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
    [InlineData(WerewolfInitialGiftSource.Tribe, "gift.tribe.black-furies.sense-wyrm", WerewolfInitialGiftSelectionErrorCode.OutOfScopeGift)]
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
