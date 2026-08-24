using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfTribeSelectionTests
{
    [Theory]
    [InlineData(WerewolfTribeIdentifiers.GlassWalkers)]
    [InlineData(WerewolfTribeIdentifiers.GetOfFenris)]
    [InlineData(WerewolfTribeIdentifiers.Fianna)]
    [InlineData(WerewolfTribeIdentifiers.ChildrenOfGaia)]
    [InlineData(WerewolfTribeIdentifiers.SilentStriders)]
    [InlineData(WerewolfTribeIdentifiers.BoneGnawers)]
    [InlineData(WerewolfTribeIdentifiers.ShadowLords)]
    [InlineData(WerewolfTribeIdentifiers.Uktena)]
    [InlineData(WerewolfTribeIdentifiers.Wendigo)]
    public void SelectsEveryCurrentSliceTribe(string tribeId)
    {
        var result = Select(Draft() with { Race = WerewolfRaceIdentifiers.Homid }, tribeId);

        Assert.True(result.Succeeded);
        Assert.Equal(tribeId, result.Draft?.Tribe);
        Assert.Equal(2, result.Draft?.DraftVersion);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfTribeSelectionErrorCode.TribeSelected);
    }

    [Fact]
    public void TribeIdentifiersAreCanonicalAndLocalizationIndependent()
    {
        Assert.Equal(
            [
                WerewolfTribeIdentifiers.GlassWalkers,
                WerewolfTribeIdentifiers.GetOfFenris,
                WerewolfTribeIdentifiers.Fianna,
                WerewolfTribeIdentifiers.ChildrenOfGaia,
                WerewolfTribeIdentifiers.BlackFuries,
                WerewolfTribeIdentifiers.RedTalons,
                WerewolfTribeIdentifiers.SilentStriders,
                WerewolfTribeIdentifiers.SilverFangs,
                WerewolfTribeIdentifiers.BoneGnawers,
                WerewolfTribeIdentifiers.ShadowLords,
                WerewolfTribeIdentifiers.Uktena,
                WerewolfTribeIdentifiers.Wendigo
            ],
            WerewolfTribeIdentifiers.Supported);

        var result = Select(Draft(), "Glass Walkers");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfTribeSelectionErrorCode.MalformedTribe);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("glass walkers")]
    [InlineData(" glass-walkers")]
    [InlineData("unknown")]
    public void RejectsInvalidAndMalformedTribe(string tribeId)
    {
        var result = Select(Draft(), tribeId);

        Assert.False(result.Succeeded);
        Assert.Null(result.Draft);
    }

    [Fact]
    public void RejectsStaleDraftVersion()
    {
        var draft = Draft();

        var result = WerewolfTribeSelectionService.SelectTribe(new WerewolfTribeSelectionRequest(draft, 0, WerewolfTribeIdentifiers.GlassWalkers));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfTribeSelectionErrorCode.StaleDraftVersion);
    }

    [Fact]
    public void UpdatesDraftImmutablyAndIncrementsExactlyOnce()
    {
        var draft = Draft() with
        {
            Race = WerewolfRaceIdentifiers.Metis,
            Auspice = WerewolfAuspiceIdentifiers.Theurge,
            RequiredNextSteps = Array.AsReadOnly(["allocate-abilities", "select-metis-deformity", "select-tribe"])
        };

        var result = Select(draft, WerewolfTribeIdentifiers.GlassWalkers);

        Assert.Null(draft.Tribe);
        Assert.Equal(draft.DraftVersion + 1, result.Draft?.DraftVersion);
        Assert.NotSame(draft.Attributes, result.Draft?.Attributes);
        Assert.Equal(WerewolfRaceIdentifiers.Metis, result.Draft?.Race);
        Assert.Equal(WerewolfAuspiceIdentifiers.Theurge, result.Draft?.Auspice);
        Assert.Contains("select-metis-deformity", result.Draft?.RequiredNextSteps ?? []);
    }

    [Fact]
    public void PreservesRaceAuspiceMetisRequirementAndDisabledCapabilities()
    {
        var draft = Draft() with
        {
            Race = WerewolfRaceIdentifiers.Metis,
            Auspice = WerewolfAuspiceIdentifiers.Galliard,
            RequiredNextSteps = Array.AsReadOnly(["select-metis-deformity", "select-tribe"])
        };

        var result = Select(draft, WerewolfTribeIdentifiers.GlassWalkers);

        Assert.Equal(WerewolfRaceIdentifiers.Metis, result.Draft?.Race);
        Assert.Equal(WerewolfAuspiceIdentifiers.Galliard, result.Draft?.Auspice);
        Assert.Contains("select-metis-deformity", result.Draft?.RequiredNextSteps ?? []);
        Assert.Equal("disabled", result.Draft?.DisabledCapabilities["additional-gift-purchase"]);
        Assert.Equal("disabled", result.Draft?.DisabledCapabilities["runtime-gift-execution"]);
        Assert.Empty(result.Draft?.Gifts ?? []);
        Assert.All(result.Draft?.Resources ?? new Dictionary<string, int?>(), entry => Assert.Null(entry.Value));
    }

    [Fact]
    public void NextStepsAreDeterministicAndDoNotAutoChooseDownstreamFields()
    {
        var first = Select(Draft(), WerewolfTribeIdentifiers.GlassWalkers);
        var second = Select(Draft(), WerewolfTribeIdentifiers.GlassWalkers);

        Assert.Equal(first.Draft?.RequiredNextSteps, second.Draft?.RequiredNextSteps);
        Assert.DoesNotContain("select-tribe", first.Draft?.RequiredNextSteps ?? []);
        Assert.Contains("select-initial-gifts", first.Draft?.RequiredNextSteps ?? []);
        Assert.Contains(WerewolfBackgroundAllocationService.AllocateBackgroundsStep, first.Draft?.RequiredNextSteps ?? []);
        Assert.Contains("initialize-resources-and-rank", first.Draft?.RequiredNextSteps ?? []);
        Assert.All(first.Draft?.NarrativeFields ?? new Dictionary<string, string?>(), entry => Assert.Null(entry.Value));
    }

    [Theory]
    [InlineData(WerewolfTribeIdentifiers.GlassWalkers, WerewolfCharacterResourceIdentifiers.Willpower, 3)]
    [InlineData(WerewolfTribeIdentifiers.GetOfFenris, WerewolfCharacterResourceIdentifiers.Willpower, 3)]
    [InlineData(WerewolfTribeIdentifiers.Fianna, WerewolfCharacterResourceIdentifiers.Willpower, 3)]
    [InlineData(WerewolfTribeIdentifiers.ChildrenOfGaia, WerewolfCharacterResourceIdentifiers.Willpower, 4)]
    [InlineData(WerewolfTribeIdentifiers.SilentStriders, WerewolfCharacterResourceIdentifiers.Willpower, 3)]
    [InlineData(WerewolfTribeIdentifiers.BoneGnawers, WerewolfCharacterResourceIdentifiers.Willpower, 4)]
    [InlineData(WerewolfTribeIdentifiers.ShadowLords, WerewolfCharacterResourceIdentifiers.Willpower, 3)]
    [InlineData(WerewolfTribeIdentifiers.Uktena, WerewolfCharacterResourceIdentifiers.Willpower, 3)]
    [InlineData(WerewolfTribeIdentifiers.Wendigo, WerewolfCharacterResourceIdentifiers.Willpower, 4)]
    public void InitializesCorrectWillpowerForTribe(string tribeId, string resourceId, int expected)
    {
        var draft = Select(Draft() with { Race = WerewolfRaceIdentifiers.Homid, Auspice = WerewolfAuspiceIdentifiers.Ragabash }, tribeId).Draft!;
        var initialized = WerewolfResourceRankInitializationService.Initialize(new WerewolfResourceRankInitializationRequest(draft, draft.DraftVersion));

        Assert.True(initialized.Succeeded);
        var willpower = initialized.Resources.First(value => StringComparer.Ordinal.Equals(value.ResourceId, resourceId));
        Assert.Equal(expected, willpower.Permanent);
        Assert.Equal(expected, willpower.Current);
    }

    [Theory]
    [InlineData(WerewolfTribeIdentifiers.GlassWalkers, WerewolfBackgroundIdentifiers.Mentor, true)]
    [InlineData(WerewolfTribeIdentifiers.GetOfFenris, WerewolfBackgroundIdentifiers.Contacts, true)]
    [InlineData(WerewolfTribeIdentifiers.RedTalons, WerewolfBackgroundIdentifiers.Allies, true)]
    [InlineData(WerewolfTribeIdentifiers.RedTalons, WerewolfBackgroundIdentifiers.Contacts, true)]
    [InlineData(WerewolfTribeIdentifiers.RedTalons, WerewolfBackgroundIdentifiers.Resources, true)]
    [InlineData(WerewolfTribeIdentifiers.SilentStriders, WerewolfBackgroundIdentifiers.Resources, true)]
    [InlineData(WerewolfTribeIdentifiers.BoneGnawers, WerewolfBackgroundIdentifiers.Resources, true)]
    [InlineData(WerewolfTribeIdentifiers.ShadowLords, WerewolfBackgroundIdentifiers.Allies, true)]
    [InlineData(WerewolfTribeIdentifiers.ShadowLords, WerewolfBackgroundIdentifiers.Mentor, true)]
    [InlineData(WerewolfTribeIdentifiers.Wendigo, WerewolfBackgroundIdentifiers.Contacts, true)]
    [InlineData(WerewolfTribeIdentifiers.Wendigo, WerewolfBackgroundIdentifiers.Resources, true)]
    [InlineData(WerewolfTribeIdentifiers.Fianna, WerewolfBackgroundIdentifiers.Allies, false)]
    [InlineData(WerewolfTribeIdentifiers.ChildrenOfGaia, WerewolfBackgroundIdentifiers.Mentor, false)]
    [InlineData(WerewolfTribeIdentifiers.BlackFuries, WerewolfBackgroundIdentifiers.Rites, false)]
    [InlineData(WerewolfTribeIdentifiers.SilverFangs, WerewolfBackgroundIdentifiers.Contacts, false)]
    [InlineData(WerewolfTribeIdentifiers.Uktena, WerewolfBackgroundIdentifiers.Allies, false)]
    public void EnforcesTribeBackgroundRestrictions(string tribeId, string backgroundId, bool restricted)
    {
        var draft = Select(Draft(), tribeId).Draft!;
        var backgrounds = new Dictionary<string, int?>(StringComparer.Ordinal);
        foreach (var supported in WerewolfBackgroundIdentifiers.Supported)
        {
            backgrounds[supported] = StringComparer.Ordinal.Equals(supported, backgroundId) ? 1 : 0;
        }
        backgrounds[WerewolfBackgroundIdentifiers.Rites] = 4;

        var valid = WerewolfBackgroundAllocationService.IsBackgroundAllocationValidForTribe(backgrounds, tribeId);

        Assert.Equal(!restricted, valid);
    }

    [Theory]
    [InlineData(WerewolfTribeIdentifiers.GlassWalkers, WerewolfGiftIdentifiers.GlassWalkersControlSimpleMachine)]
    [InlineData(WerewolfTribeIdentifiers.GetOfFenris, WerewolfGiftIdentifiers.GetOfFenrisRazorClaws)]
    [InlineData(WerewolfTribeIdentifiers.Fianna, WerewolfGiftIdentifiers.FiannaFaerieLight)]
    [InlineData(WerewolfTribeIdentifiers.ChildrenOfGaia, WerewolfGiftIdentifiers.ChildrenOfGaiaMercy)]
    [InlineData(WerewolfTribeIdentifiers.SilentStriders, WerewolfGiftIdentifiers.SilentStridersSilence)]
    [InlineData(WerewolfTribeIdentifiers.BoneGnawers, WerewolfGiftIdentifiers.BoneGnawersCooking)]
    [InlineData(WerewolfTribeIdentifiers.ShadowLords, WerewolfGiftIdentifiers.ShadowLordsSeizingTheEdge)]
    [InlineData(WerewolfTribeIdentifiers.Uktena, WerewolfGiftIdentifiers.UktenaSpiritSpeech)]
    [InlineData(WerewolfTribeIdentifiers.Wendigo, WerewolfGiftIdentifiers.WendigoCamouflage)]
    public void TribeGiftIsEligibleForTribe(string tribeId, string giftId)
    {
        var draft = Select(Draft() with { Race = WerewolfRaceIdentifiers.Homid, Auspice = WerewolfAuspiceIdentifiers.Ragabash }, tribeId).Draft!;
        var result = WerewolfInitialGiftSelectionService.SelectGift(new WerewolfInitialGiftSelectionRequest(draft, draft.DraftVersion, WerewolfInitialGiftSource.Tribe, giftId));

        Assert.True(result.Succeeded);
        Assert.Equal(giftId, result.Draft?.TribeGift);
    }

    [Fact]
    public void RuntimeFlowCreatesDraftSelectsRaceSelectsAuspiceAndSelectsTribe()
    {
        var registry = RuntimeRegistry();
        var created = registry.Execute(new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            WerewolfReferenceRuntime.CreateCharacterOperation,
            new Dictionary<string, string>(StringComparer.Ordinal) { ["requestId"] = "request-001" }));
        var race = registry.Execute(new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            WerewolfReferenceRuntime.SelectRaceOperation,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["draftId"] = created.Outputs["draftId"],
                ["draftVersion"] = created.Outputs["draftVersion"],
                ["expectedDraftVersion"] = created.Outputs["draftVersion"],
                ["raceId"] = WerewolfRaceIdentifiers.Metis
            }));
        var auspice = registry.Execute(new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            WerewolfReferenceRuntime.SelectAuspiceOperation,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["auspiceId"] = WerewolfAuspiceIdentifiers.Theurge,
                ["currentRace"] = race.Outputs["raceId"],
                ["draftId"] = race.Outputs["draftId"],
                ["draftVersion"] = race.Outputs["draftVersion"],
                ["expectedDraftVersion"] = race.Outputs["draftVersion"],
                ["requiresMetisDeformity"] = "true"
            }));

        var tribe = registry.Execute(new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            WerewolfReferenceRuntime.SelectTribeOperation,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["auspiceId"] = auspice.Outputs["auspiceId"],
                ["currentAuspice"] = auspice.Outputs["auspiceId"],
                ["currentRace"] = auspice.Outputs["raceId"],
                ["draftId"] = auspice.Outputs["draftId"],
                ["draftVersion"] = auspice.Outputs["draftVersion"],
                ["expectedDraftVersion"] = auspice.Outputs["draftVersion"],
                ["requiresMetisDeformity"] = "true",
                ["tribeId"] = WerewolfTribeIdentifiers.GlassWalkers
            }));

        Assert.True(tribe.Succeeded);
        Assert.Equal(WerewolfTribeIdentifiers.GlassWalkers, tribe.Outputs["tribeId"]);
        Assert.Equal(WerewolfAuspiceIdentifiers.Theurge, tribe.Outputs["auspiceId"]);
        Assert.Equal(WerewolfRaceIdentifiers.Metis, tribe.Outputs["raceId"]);
        Assert.Equal("4", tribe.Outputs["draftVersion"]);
        Assert.Contains("select-metis-deformity", tribe.Outputs["nextSteps"], StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(WerewolfTribeIdentifiers.RedTalons, WerewolfRaceIdentifiers.Homid)]
    [InlineData(WerewolfTribeIdentifiers.RedTalons, WerewolfRaceIdentifiers.Metis)]
    [InlineData(WerewolfTribeIdentifiers.SilverFangs, WerewolfRaceIdentifiers.Homid)]
    [InlineData(WerewolfTribeIdentifiers.SilverFangs, WerewolfRaceIdentifiers.Lupus)]
    [InlineData(WerewolfTribeIdentifiers.SilverFangs, WerewolfRaceIdentifiers.Metis)]
    [InlineData(WerewolfTribeIdentifiers.BlackFuries, WerewolfRaceIdentifiers.Homid)]
    [InlineData(WerewolfTribeIdentifiers.BlackFuries, WerewolfRaceIdentifiers.Lupus)]
    [InlineData(WerewolfTribeIdentifiers.BlackFuries, WerewolfRaceIdentifiers.Metis)]
    public void RejectsTribeSelectionForIneligibleRaceOrDependency(string tribeId, string raceId)
    {
        var draft = Draft() with { Race = raceId };

        var result = Select(draft, tribeId);

        Assert.False(result.Succeeded);
        Assert.Null(result.Draft);
        Assert.Contains(result.Findings, finding => finding.Severity == WerewolfTribeSelectionFindingSeverity.Error);
    }

    [Fact]
    public void RedTalonsLupusSelectionSucceeds()
    {
        var draft = Draft() with { Race = WerewolfRaceIdentifiers.Lupus };

        var result = Select(draft, WerewolfTribeIdentifiers.RedTalons);

        Assert.True(result.Succeeded);
        Assert.Equal(WerewolfTribeIdentifiers.RedTalons, result.Draft?.Tribe);
    }

    [Theory]
    [InlineData(WerewolfTribeIdentifiers.GlassWalkers, WerewolfRaceIdentifiers.Homid)]
    [InlineData(WerewolfTribeIdentifiers.GetOfFenris, WerewolfRaceIdentifiers.Lupus)]
    [InlineData(WerewolfTribeIdentifiers.Fianna, WerewolfRaceIdentifiers.Metis)]
    [InlineData(WerewolfTribeIdentifiers.ChildrenOfGaia, WerewolfRaceIdentifiers.Homid)]
    [InlineData(WerewolfTribeIdentifiers.SilentStriders, WerewolfRaceIdentifiers.Lupus)]
    [InlineData(WerewolfTribeIdentifiers.BoneGnawers, WerewolfRaceIdentifiers.Metis)]
    [InlineData(WerewolfTribeIdentifiers.ShadowLords, WerewolfRaceIdentifiers.Homid)]
    [InlineData(WerewolfTribeIdentifiers.Uktena, WerewolfRaceIdentifiers.Lupus)]
    [InlineData(WerewolfTribeIdentifiers.Wendigo, WerewolfRaceIdentifiers.Metis)]
    public void UnrestrictedTribesRemainSelectableForAllRaces(string tribeId, string raceId)
    {
        var draft = Draft() with { Race = raceId };

        var result = Select(draft, tribeId);

        Assert.True(result.Succeeded);
        Assert.Equal(tribeId, result.Draft?.Tribe);
    }

    [Fact]
    public void TribeSelectionHasNoForbiddenDependencies()
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "rule-sets", "Chronicle.RuleSets.Werewolf", "CharacterCreation", "WerewolfTribeSelection.cs"));
        var forbidden = new[] { "Chronicle.Persistence", "Chronicle.Presentation", "OpenAI", "HttpClient", "DbContext", "File.", "Directory.", "Random", "Campaign" };

        Assert.DoesNotContain(forbidden, token => source.Contains(token, StringComparison.Ordinal));
    }

    private static WerewolfTribeSelectionResult Select(WerewolfInitializedCharacterState draft, string tribeId)
    {
        return WerewolfTribeSelectionService.SelectTribe(new WerewolfTribeSelectionRequest(draft, draft.DraftVersion, tribeId));
    }

    private static WerewolfInitializedCharacterState Draft()
    {
        return WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity("draft-001"), 1);
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
