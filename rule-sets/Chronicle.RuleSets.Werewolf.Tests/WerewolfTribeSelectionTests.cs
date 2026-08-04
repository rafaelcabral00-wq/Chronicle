using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfTribeSelectionTests
{
    [Theory]
    [InlineData(WerewolfTribeIdentifiers.GlassWalkers)]
    public void SelectsEveryCurrentSliceTribe(string tribeId)
    {
        var result = Select(Draft(), tribeId);

        Assert.True(result.Succeeded);
        Assert.Equal(tribeId, result.Draft?.Tribe);
        Assert.Equal(2, result.Draft?.DraftVersion);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfTribeSelectionErrorCode.TribeSelected);
    }

    [Fact]
    public void TribeIdentifiersAreCanonicalAndLocalizationIndependent()
    {
        Assert.Equal(["glass-walkers"], WerewolfTribeIdentifiers.Supported);

        var result = Select(Draft(), "Glass Walkers");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfTribeSelectionErrorCode.MalformedTribe);
    }

    [Fact]
    public void ReplacesExistingTribe()
    {
        var first = Select(Draft(), WerewolfTribeIdentifiers.GlassWalkers).Draft!;

        var second = Select(first, WerewolfTribeIdentifiers.GlassWalkers);

        Assert.Equal(WerewolfTribeIdentifiers.GlassWalkers, second.Draft?.Tribe);
        Assert.Equal(3, second.Draft?.DraftVersion);
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

    [Theory]
    [InlineData("black-furies")]
    [InlineData("bone-gnawers")]
    [InlineData("children-of-gaia")]
    [InlineData("fianna")]
    [InlineData("get-of-fenris")]
    [InlineData("red-talons")]
    [InlineData("shadow-lords")]
    [InlineData("silent-striders")]
    [InlineData("silver-fangs")]
    [InlineData("uktena")]
    [InlineData("wendigo")]
    public void RejectsCatalogedButOutOfScopeTribes(string tribeId)
    {
        var result = Select(Draft(), tribeId);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfTribeSelectionErrorCode.TribeOutOfScope);
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
        Assert.Contains("review-resources", first.Draft?.RequiredNextSteps ?? []);
        Assert.All(first.Draft?.NarrativeFields ?? new Dictionary<string, string?>(), entry => Assert.Null(entry.Value));
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
