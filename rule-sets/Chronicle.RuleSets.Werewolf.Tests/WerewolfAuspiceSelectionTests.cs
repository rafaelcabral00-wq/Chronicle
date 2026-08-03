using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfAuspiceSelectionTests
{
    [Theory]
    [InlineData(WerewolfAuspiceIdentifiers.Ragabash)]
    [InlineData(WerewolfAuspiceIdentifiers.Theurge)]
    [InlineData(WerewolfAuspiceIdentifiers.Philodox)]
    [InlineData(WerewolfAuspiceIdentifiers.Galliard)]
    [InlineData(WerewolfAuspiceIdentifiers.Ahroun)]
    public void SelectsSupportedAuspice(string auspiceId)
    {
        var result = Select(Draft(), auspiceId);

        Assert.True(result.Succeeded);
        Assert.Equal(auspiceId, result.Draft?.Auspice);
        Assert.Equal(2, result.Draft?.DraftVersion);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfAuspiceSelectionErrorCode.AuspiceSelected);
    }

    [Fact]
    public void AuspiceIdentifiersAreCanonicalAndLocalizationIndependent()
    {
        Assert.Equal(["ahroun", "galliard", "philodox", "ragabash", "theurge"], WerewolfAuspiceIdentifiers.Supported);

        var result = Select(Draft(), "Theurge");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfAuspiceSelectionErrorCode.UnknownAuspice);
    }

    [Fact]
    public void ReplacesExistingAuspice()
    {
        var ragabash = Select(Draft(), WerewolfAuspiceIdentifiers.Ragabash).Draft!;

        var ahroun = Select(ragabash, WerewolfAuspiceIdentifiers.Ahroun);

        Assert.Equal(WerewolfAuspiceIdentifiers.Ahroun, ahroun.Draft?.Auspice);
        Assert.Equal(3, ahroun.Draft?.DraftVersion);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("the urge")]
    [InlineData(" theurge")]
    [InlineData("unknown")]
    public void RejectsInvalidAndMalformedAuspice(string auspiceId)
    {
        var result = Select(Draft(), auspiceId);

        Assert.False(result.Succeeded);
        Assert.Null(result.Draft);
    }

    [Fact]
    public void RejectsStaleDraftVersion()
    {
        var draft = Draft();

        var result = WerewolfAuspiceSelectionService.SelectAuspice(new WerewolfAuspiceSelectionRequest(draft, 0, WerewolfAuspiceIdentifiers.Theurge));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfAuspiceSelectionErrorCode.StaleDraftVersion);
    }

    [Fact]
    public void UpdatesDraftImmutablyAndIncrementsExactlyOnce()
    {
        var draft = Draft() with
        {
            Race = WerewolfRaceIdentifiers.Metis,
            RequiredNextSteps = Array.AsReadOnly(["allocate-abilities", "select-metis-deformity"])
        };

        var result = Select(draft, WerewolfAuspiceIdentifiers.Philodox);

        Assert.Null(draft.Auspice);
        Assert.Equal(draft.DraftVersion + 1, result.Draft?.DraftVersion);
        Assert.NotSame(draft.Attributes, result.Draft?.Attributes);
        Assert.Equal(WerewolfRaceIdentifiers.Metis, result.Draft?.Race);
        Assert.Contains("select-metis-deformity", result.Draft?.RequiredNextSteps ?? []);
    }

    [Fact]
    public void PreservesRaceMetisRequirementAndDisabledCapabilities()
    {
        var draft = Draft() with
        {
            Race = WerewolfRaceIdentifiers.Metis,
            RequiredNextSteps = Array.AsReadOnly(["select-metis-deformity", "select-tribe"])
        };

        var result = Select(draft, WerewolfAuspiceIdentifiers.Galliard);

        Assert.Equal(WerewolfRaceIdentifiers.Metis, result.Draft?.Race);
        Assert.Contains("select-metis-deformity", result.Draft?.RequiredNextSteps ?? []);
        Assert.Equal("disabled", result.Draft?.DisabledCapabilities["additional-gift-purchase"]);
        Assert.Equal("disabled", result.Draft?.DisabledCapabilities["runtime-gift-execution"]);
        Assert.Empty(result.Draft?.Gifts ?? []);
    }

    [Fact]
    public void NextStepsAreDeterministic()
    {
        var first = Select(Draft(), WerewolfAuspiceIdentifiers.Theurge);
        var second = Select(Draft(), WerewolfAuspiceIdentifiers.Theurge);

        Assert.Equal(first.Draft?.RequiredNextSteps, second.Draft?.RequiredNextSteps);
        Assert.DoesNotContain("select-auspice", first.Draft?.RequiredNextSteps ?? []);
    }

    [Fact]
    public void RuntimeFlowCreatesDraftSelectsRaceAndSelectsAuspice()
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

        Assert.True(auspice.Succeeded);
        Assert.Equal(WerewolfAuspiceIdentifiers.Theurge, auspice.Outputs["auspiceId"]);
        Assert.Equal(WerewolfRaceIdentifiers.Metis, auspice.Outputs["raceId"]);
        Assert.Equal("3", auspice.Outputs["draftVersion"]);
        Assert.Contains("select-metis-deformity", auspice.Outputs["nextSteps"], StringComparison.Ordinal);
    }

    [Fact]
    public void AuspiceSelectionHasNoForbiddenDependencies()
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "rule-sets", "Chronicle.RuleSets.Werewolf", "CharacterCreation", "WerewolfAuspiceSelection.cs"));
        var forbidden = new[] { "Chronicle.Persistence", "Chronicle.Presentation", "OpenAI", "HttpClient", "DbContext", "File.", "Directory.", "Random", "Campaign" };

        Assert.DoesNotContain(forbidden, token => source.Contains(token, StringComparison.Ordinal));
    }

    private static WerewolfAuspiceSelectionResult Select(WerewolfInitializedCharacterState draft, string auspiceId)
    {
        return WerewolfAuspiceSelectionService.SelectAuspice(new WerewolfAuspiceSelectionRequest(draft, draft.DraftVersion, auspiceId));
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
