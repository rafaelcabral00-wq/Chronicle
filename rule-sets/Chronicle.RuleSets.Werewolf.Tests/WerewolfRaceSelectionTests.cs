using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfRaceSelectionTests
{
    [Theory]
    [InlineData(WerewolfRaceIdentifiers.Homid)]
    [InlineData(WerewolfRaceIdentifiers.Metis)]
    [InlineData(WerewolfRaceIdentifiers.Lupus)]
    public void SelectsSupportedRace(string raceId)
    {
        var draft = Draft();

        var result = Select(draft, raceId);

        Assert.True(result.Succeeded);
        Assert.Equal(raceId, result.Draft?.Race);
        Assert.Equal(2, result.Draft?.DraftVersion);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfRaceSelectionErrorCode.RaceSelected);
    }

    [Fact]
    public void RaceIdentifiersAreCanonicalAndLocalizationIndependent()
    {
        Assert.Equal(["homid", "lupus", "metis"], WerewolfRaceIdentifiers.Supported);

        var result = Select(Draft(), "Homid");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfRaceSelectionErrorCode.UnknownRace);
    }

    [Fact]
    public void MetisSelectionAddsDeformityRequirement()
    {
        var result = Select(Draft(), WerewolfRaceIdentifiers.Metis);

        Assert.Contains("select-metis-deformity", result.Draft?.RequiredNextSteps ?? []);
        Assert.DoesNotContain("select-race", result.Draft?.RequiredNextSteps ?? []);
    }

    [Fact]
    public void ChangingAwayFromMetisRemovesOnlyMetisDeformityRequirement()
    {
        var metis = Select(Draft(), WerewolfRaceIdentifiers.Metis).Draft! with
        {
            MetisDeformity = WerewolfMetisDeformityIdentifiers.Horns
        };

        var homid = Select(metis, WerewolfRaceIdentifiers.Homid);

        Assert.Equal(WerewolfRaceIdentifiers.Homid, homid.Draft?.Race);
        Assert.Null(homid.Draft?.MetisDeformity);
        Assert.DoesNotContain("select-metis-deformity", homid.Draft?.RequiredNextSteps ?? []);
        Assert.Contains("select-auspice", homid.Draft?.RequiredNextSteps ?? []);
    }

    [Fact]
    public void ReplacesExistingRace()
    {
        var homid = Select(Draft(), WerewolfRaceIdentifiers.Homid).Draft!;

        var lupus = Select(homid, WerewolfRaceIdentifiers.Lupus);

        Assert.Equal(WerewolfRaceIdentifiers.Lupus, lupus.Draft?.Race);
        Assert.Equal(3, lupus.Draft?.DraftVersion);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ho mid")]
    [InlineData(" homid")]
    [InlineData("unknown")]
    public void RejectsUnknownMissingOrMalformedRace(string raceId)
    {
        var result = Select(Draft(), raceId);

        Assert.False(result.Succeeded);
        Assert.Null(result.Draft);
    }

    [Fact]
    public void RejectsStaleDraftVersion()
    {
        var draft = Draft();

        var result = WerewolfRaceSelectionService.SelectRace(new WerewolfRaceSelectionRequest(draft, 0, WerewolfRaceIdentifiers.Homid));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfRaceSelectionErrorCode.StaleDraftVersion);
    }

    [Fact]
    public void UpdatesDraftImmutablyAndPreservesUnrelatedState()
    {
        var draft = Draft() with
        {
            Auspice = "kept-auspice-test-value",
            Tribe = "kept-tribe-test-value"
        };

        var result = Select(draft, WerewolfRaceIdentifiers.Homid);

        Assert.Null(draft.Race);
        Assert.Equal(1, draft.DraftVersion);
        Assert.NotSame(draft.Attributes, result.Draft?.Attributes);
        Assert.Equal(draft.Auspice, result.Draft?.Auspice);
        Assert.Equal(draft.Tribe, result.Draft?.Tribe);
        Assert.Equal(draft.MetisDeformity, result.Draft?.MetisDeformity);
        Assert.Equal(draft.DisabledCapabilities, result.Draft?.DisabledCapabilities);
    }

    [Fact]
    public void IncrementsVersionExactlyOnce()
    {
        var draft = Draft();

        var result = Select(draft, WerewolfRaceIdentifiers.Lupus);

        Assert.Equal(draft.DraftVersion + 1, result.Draft?.DraftVersion);
    }

    [Fact]
    public void NextStepsAreDeterministic()
    {
        var first = Select(Draft(), WerewolfRaceIdentifiers.Metis);
        var second = Select(Draft(), WerewolfRaceIdentifiers.Metis);

        Assert.Equal(first.Draft?.RequiredNextSteps, second.Draft?.RequiredNextSteps);
    }

    [Fact]
    public void RuntimeFlowCreatesDraftAndSelectsRace()
    {
        var registry = RuntimeRegistry();
        var created = registry.Execute(new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            WerewolfReferenceRuntime.CreateCharacterOperation,
            new Dictionary<string, string>(StringComparer.Ordinal) { ["requestId"] = "request-001" }));

        var selected = registry.Execute(new RuleSetOperationRequest(
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

        Assert.True(selected.Succeeded);
        Assert.Equal(WerewolfRaceIdentifiers.Metis, selected.Outputs["raceId"]);
        Assert.Equal("2", selected.Outputs["draftVersion"]);
        Assert.Contains("select-metis-deformity", selected.Outputs["nextSteps"], StringComparison.Ordinal);
    }

    [Fact]
    public void RaceSelectionHasNoForbiddenDependencies()
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "rule-sets", "Chronicle.RuleSets.Werewolf", "CharacterCreation", "WerewolfRaceSelection.cs"));
        var forbidden = new[] { "Chronicle.Persistence", "Chronicle.Presentation", "OpenAI", "HttpClient", "DbContext", "File.", "Directory.", "Random", "Campaign" };

        Assert.DoesNotContain(forbidden, token => source.Contains(token, StringComparison.Ordinal));
    }

    private static WerewolfRaceSelectionResult Select(WerewolfInitializedCharacterState draft, string raceId)
    {
        return WerewolfRaceSelectionService.SelectRace(new WerewolfRaceSelectionRequest(draft, draft.DraftVersion, raceId));
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
