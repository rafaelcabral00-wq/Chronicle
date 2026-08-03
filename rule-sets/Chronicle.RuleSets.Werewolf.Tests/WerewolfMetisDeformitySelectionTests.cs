using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfMetisDeformitySelectionTests
{
    [Theory]
    [InlineData(WerewolfMetisDeformityIdentifiers.Horns)]
    public void SelectsEveryCurrentSliceDeformity(string deformityId)
    {
        var result = Select(MetisDraft(), deformityId);

        Assert.True(result.Succeeded);
        Assert.Equal(deformityId, result.Draft?.MetisDeformity);
        Assert.Equal(2, result.Draft?.DraftVersion);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfMetisDeformitySelectionErrorCode.DeformitySelected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(WerewolfRaceIdentifiers.Homid)]
    [InlineData(WerewolfRaceIdentifiers.Lupus)]
    public void RejectsUnsetHomidAndLupusRace(string? race)
    {
        var result = Select(Draft() with { Race = race }, WerewolfMetisDeformityIdentifiers.Horns);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfMetisDeformitySelectionErrorCode.RaceNotMetis);
    }

    [Fact]
    public void DeformityIdentifiersAreCanonicalAndLocalizationIndependent()
    {
        Assert.Equal(["horns"], WerewolfMetisDeformityIdentifiers.Supported);

        var result = Select(MetisDraft(), "Horns");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfMetisDeformitySelectionErrorCode.UnknownDeformity);
    }

    [Fact]
    public void ReplacesExistingDeformityWhileRaceRemainsMetis()
    {
        var first = Select(MetisDraft(), WerewolfMetisDeformityIdentifiers.Horns).Draft!;

        var second = Select(first, WerewolfMetisDeformityIdentifiers.Horns);

        Assert.Equal(WerewolfMetisDeformityIdentifiers.Horns, second.Draft?.MetisDeformity);
        Assert.Equal(3, second.Draft?.DraftVersion);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("horn s")]
    [InlineData(" horns")]
    [InlineData("unknown")]
    public void RejectsInvalidMalformedAndUnknownDeformity(string deformityId)
    {
        var result = Select(MetisDraft(), deformityId);

        Assert.False(result.Succeeded);
        Assert.Null(result.Draft);
    }

    [Theory]
    [InlineData("albinism")]
    [InlineData("blind")]
    [InlineData("debilitating-disease")]
    [InlineData("fits-of-madness")]
    [InlineData("hairless")]
    [InlineData("hunchback")]
    [InlineData("no-sense-of-smell")]
    [InlineData("seizures")]
    [InlineData("tailless")]
    [InlineData("tough-hide")]
    [InlineData("weak-immune-system")]
    [InlineData("withered-limb")]
    public void RejectsCatalogedButOutOfScopeDeformities(string deformityId)
    {
        var result = Select(MetisDraft(), deformityId);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfMetisDeformitySelectionErrorCode.DeformityOutOfScope);
    }

    [Fact]
    public void RejectsStaleDraftVersion()
    {
        var draft = MetisDraft();

        var result = WerewolfMetisDeformitySelectionService.SelectDeformity(new WerewolfMetisDeformitySelectionRequest(draft, 0, WerewolfMetisDeformityIdentifiers.Horns));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfMetisDeformitySelectionErrorCode.StaleDraftVersion);
    }

    [Fact]
    public void UpdatesDraftImmutablyAndIncrementsExactlyOnce()
    {
        var draft = MetisDraft() with
        {
            Auspice = WerewolfAuspiceIdentifiers.Theurge,
            Tribe = WerewolfTribeIdentifiers.GlassWalkers
        };

        var result = Select(draft, WerewolfMetisDeformityIdentifiers.Horns);

        Assert.Null(draft.MetisDeformity);
        Assert.Equal(draft.DraftVersion + 1, result.Draft?.DraftVersion);
        Assert.NotSame(draft.Attributes, result.Draft?.Attributes);
        Assert.Equal(WerewolfAuspiceIdentifiers.Theurge, result.Draft?.Auspice);
        Assert.Equal(WerewolfTribeIdentifiers.GlassWalkers, result.Draft?.Tribe);
    }

    [Fact]
    public void ResolvesMetisDeformityNextStepWithoutApplyingEffects()
    {
        var result = Select(MetisDraft(), WerewolfMetisDeformityIdentifiers.Horns);

        Assert.DoesNotContain("select-metis-deformity", result.Draft?.RequiredNextSteps ?? []);
        Assert.Empty(result.Draft?.Gifts ?? []);
        Assert.All(result.Draft?.Resources ?? new Dictionary<string, int?>(), entry => Assert.Null(entry.Value));
        Assert.Equal("disabled", result.Draft?.DisabledCapabilities["additional-gift-purchase"]);
        Assert.Equal("disabled", result.Draft?.DisabledCapabilities["runtime-gift-execution"]);
    }

    [Fact]
    public void RaceChangesAwayFromAndBackToMetisRemainCoherent()
    {
        var selected = Select(MetisDraft(), WerewolfMetisDeformityIdentifiers.Horns).Draft!;

        var homid = WerewolfRaceSelectionService.SelectRace(new WerewolfRaceSelectionRequest(selected, selected.DraftVersion, WerewolfRaceIdentifiers.Homid)).Draft!;
        var metisAgain = WerewolfRaceSelectionService.SelectRace(new WerewolfRaceSelectionRequest(homid, homid.DraftVersion, WerewolfRaceIdentifiers.Metis)).Draft!;

        Assert.Equal(WerewolfRaceIdentifiers.Homid, homid.Race);
        Assert.Null(homid.MetisDeformity);
        Assert.DoesNotContain("select-metis-deformity", homid.RequiredNextSteps);
        Assert.Equal(WerewolfRaceIdentifiers.Metis, metisAgain.Race);
        Assert.Null(metisAgain.MetisDeformity);
        Assert.Contains("select-metis-deformity", metisAgain.RequiredNextSteps);
        Assert.Equal(selected.Auspice, homid.Auspice);
        Assert.Equal(selected.Tribe, metisAgain.Tribe);
    }

    [Fact]
    public void PreservesAuspiceTribeAndUnrelatedState()
    {
        var draft = MetisDraft() with
        {
            Auspice = WerewolfAuspiceIdentifiers.Philodox,
            Tribe = WerewolfTribeIdentifiers.GlassWalkers,
            RequiredNextSteps = Array.AsReadOnly(["allocate-abilities", "select-metis-deformity"])
        };

        var result = Select(draft, WerewolfMetisDeformityIdentifiers.Horns);

        Assert.Equal(WerewolfAuspiceIdentifiers.Philodox, result.Draft?.Auspice);
        Assert.Equal(WerewolfTribeIdentifiers.GlassWalkers, result.Draft?.Tribe);
        Assert.Contains("allocate-abilities", result.Draft?.RequiredNextSteps ?? []);
        Assert.Equal(draft.NarrativeFields, result.Draft?.NarrativeFields);
    }

    [Fact]
    public void RuntimeFlowCreatesDraftSelectsRaceAuspiceTribeAndMetisDeformity()
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
                ["currentAuspice"] = auspice.Outputs["auspiceId"],
                ["currentRace"] = auspice.Outputs["raceId"],
                ["draftId"] = auspice.Outputs["draftId"],
                ["draftVersion"] = auspice.Outputs["draftVersion"],
                ["expectedDraftVersion"] = auspice.Outputs["draftVersion"],
                ["requiresMetisDeformity"] = "true",
                ["tribeId"] = WerewolfTribeIdentifiers.GlassWalkers
            }));

        var deformity = registry.Execute(new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            WerewolfReferenceRuntime.SelectMetisDeformityOperation,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["currentAuspice"] = tribe.Outputs["auspiceId"],
                ["currentRace"] = tribe.Outputs["raceId"],
                ["currentTribe"] = tribe.Outputs["tribeId"],
                ["deformityId"] = WerewolfMetisDeformityIdentifiers.Horns,
                ["draftId"] = tribe.Outputs["draftId"],
                ["draftVersion"] = tribe.Outputs["draftVersion"],
                ["expectedDraftVersion"] = tribe.Outputs["draftVersion"]
            }));

        Assert.True(deformity.Succeeded);
        Assert.Equal(WerewolfMetisDeformityIdentifiers.Horns, deformity.Outputs["metisDeformityId"]);
        Assert.Equal(WerewolfRaceIdentifiers.Metis, deformity.Outputs["raceId"]);
        Assert.Equal(WerewolfAuspiceIdentifiers.Theurge, deformity.Outputs["auspiceId"]);
        Assert.Equal(WerewolfTribeIdentifiers.GlassWalkers, deformity.Outputs["tribeId"]);
        Assert.Equal("5", deformity.Outputs["draftVersion"]);
        Assert.DoesNotContain("select-metis-deformity", deformity.Outputs["nextSteps"], StringComparison.Ordinal);
    }

    [Fact]
    public void MetisDeformitySelectionHasNoForbiddenDependencies()
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "rule-sets", "Chronicle.RuleSets.Werewolf", "CharacterCreation", "WerewolfMetisDeformitySelection.cs"));
        var forbidden = new[] { "Chronicle.Persistence", "Chronicle.Presentation", "OpenAI", "HttpClient", "DbContext", "File.", "Directory.", "Random", "Campaign" };

        Assert.DoesNotContain(forbidden, token => source.Contains(token, StringComparison.Ordinal));
    }

    private static WerewolfMetisDeformitySelectionResult Select(WerewolfInitializedCharacterState draft, string deformityId)
    {
        return WerewolfMetisDeformitySelectionService.SelectDeformity(new WerewolfMetisDeformitySelectionRequest(draft, draft.DraftVersion, deformityId));
    }

    private static WerewolfInitializedCharacterState Draft()
    {
        return WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity("draft-001"), 1);
    }

    private static WerewolfInitializedCharacterState MetisDraft()
    {
        return Draft() with
        {
            Race = WerewolfRaceIdentifiers.Metis,
            RequiredNextSteps = Array.AsReadOnly(["select-metis-deformity", "select-tribe"])
        };
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
