using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfAttributePrioritySelectionTests
{
    public static TheoryData<string, string, string> ValidPriorityOrders => new()
    {
        { WerewolfAttributeCategoryIdentifiers.Physical, WerewolfAttributeCategoryIdentifiers.Social, WerewolfAttributeCategoryIdentifiers.Mental },
        { WerewolfAttributeCategoryIdentifiers.Physical, WerewolfAttributeCategoryIdentifiers.Mental, WerewolfAttributeCategoryIdentifiers.Social },
        { WerewolfAttributeCategoryIdentifiers.Social, WerewolfAttributeCategoryIdentifiers.Physical, WerewolfAttributeCategoryIdentifiers.Mental },
        { WerewolfAttributeCategoryIdentifiers.Social, WerewolfAttributeCategoryIdentifiers.Mental, WerewolfAttributeCategoryIdentifiers.Physical },
        { WerewolfAttributeCategoryIdentifiers.Mental, WerewolfAttributeCategoryIdentifiers.Physical, WerewolfAttributeCategoryIdentifiers.Social },
        { WerewolfAttributeCategoryIdentifiers.Mental, WerewolfAttributeCategoryIdentifiers.Social, WerewolfAttributeCategoryIdentifiers.Physical }
    };

    [Theory]
    [MemberData(nameof(ValidPriorityOrders))]
    public void SelectsAllValidCategoryOrderings(string primary, string secondary, string tertiary)
    {
        var result = Select(Draft(), primary, secondary, tertiary);

        Assert.True(result.Succeeded);
        Assert.Equal([primary, secondary, tertiary], result.Draft?.AttributePriorityOrder);
        Assert.Equal(7, result.Draft?.AttributeBudgets[primary]);
        Assert.Equal(5, result.Draft?.AttributeBudgets[secondary]);
        Assert.Equal(3, result.Draft?.AttributeBudgets[tertiary]);
    }

    [Fact]
    public void CategoryIdentifiersAreCanonicalAndLocalizationIndependent()
    {
        Assert.Equal(["mental", "physical", "social"], WerewolfAttributeCategoryIdentifiers.Supported);

        var result = Select(Draft(), "Physical", WerewolfAttributeCategoryIdentifiers.Social, WerewolfAttributeCategoryIdentifiers.Mental);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfAttributePrioritySelectionErrorCode.UnknownCategory);
    }

    [Theory]
    [InlineData("", WerewolfAttributeCategoryIdentifiers.Social, WerewolfAttributeCategoryIdentifiers.Mental, WerewolfAttributePrioritySelectionErrorCode.MissingCategory)]
    [InlineData(" physical", WerewolfAttributeCategoryIdentifiers.Social, WerewolfAttributeCategoryIdentifiers.Mental, WerewolfAttributePrioritySelectionErrorCode.MalformedCategory)]
    [InlineData("phys ical", WerewolfAttributeCategoryIdentifiers.Social, WerewolfAttributeCategoryIdentifiers.Mental, WerewolfAttributePrioritySelectionErrorCode.MalformedCategory)]
    [InlineData("spiritual", WerewolfAttributeCategoryIdentifiers.Social, WerewolfAttributeCategoryIdentifiers.Mental, WerewolfAttributePrioritySelectionErrorCode.UnknownCategory)]
    [InlineData(WerewolfAttributeCategoryIdentifiers.Physical, WerewolfAttributeCategoryIdentifiers.Physical, WerewolfAttributeCategoryIdentifiers.Mental, WerewolfAttributePrioritySelectionErrorCode.DuplicateCategory)]
    public void RejectsMissingDuplicateMalformedAndUnknownCategories(
        string primary,
        string secondary,
        string tertiary,
        WerewolfAttributePrioritySelectionErrorCode expectedCode)
    {
        var result = Select(Draft(), primary, secondary, tertiary);

        Assert.False(result.Succeeded);
        Assert.Null(result.Draft);
        Assert.Contains(result.Findings, finding => finding.Code == expectedCode);
    }

    [Fact]
    public void RejectsStaleVersion()
    {
        var draft = Draft();

        var result = WerewolfAttributePrioritySelectionService.SelectPriorities(new WerewolfAttributePrioritySelectionRequest(
            draft,
            0,
            WerewolfAttributeCategoryIdentifiers.Physical,
            WerewolfAttributeCategoryIdentifiers.Social,
            WerewolfAttributeCategoryIdentifiers.Mental));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfAttributePrioritySelectionErrorCode.StaleDraftVersion);
    }

    [Fact]
    public void ReplacesPrioritiesAndIncrementsVersionExactlyOnce()
    {
        var first = Select(Draft(), WerewolfAttributeCategoryIdentifiers.Physical, WerewolfAttributeCategoryIdentifiers.Social, WerewolfAttributeCategoryIdentifiers.Mental).Draft!;

        var replacement = Select(first, WerewolfAttributeCategoryIdentifiers.Mental, WerewolfAttributeCategoryIdentifiers.Physical, WerewolfAttributeCategoryIdentifiers.Social);

        Assert.True(replacement.Succeeded);
        Assert.Equal(first.DraftVersion + 1, replacement.Draft?.DraftVersion);
        Assert.Equal([WerewolfAttributeCategoryIdentifiers.Mental, WerewolfAttributeCategoryIdentifiers.Physical, WerewolfAttributeCategoryIdentifiers.Social], replacement.Draft?.AttributePriorityOrder);
        Assert.Equal(7, replacement.Draft?.AttributeBudgets[WerewolfAttributeCategoryIdentifiers.Mental]);
    }

    [Fact]
    public void UpdatesImmutablyAndPreservesPreviousDraftState()
    {
        var draft = Draft() with
        {
            Race = WerewolfRaceIdentifiers.Metis,
            Auspice = WerewolfAuspiceIdentifiers.Philodox,
            Tribe = WerewolfTribeIdentifiers.GlassWalkers,
            MetisDeformity = WerewolfMetisDeformityIdentifiers.Horns,
            RaceGift = WerewolfInitialGiftIdentifiers.MetisCreateElement,
            AuspiceGift = WerewolfInitialGiftIdentifiers.PhilodoxResistPain,
            TribeGift = WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine
        };

        var result = Select(draft, WerewolfAttributeCategoryIdentifiers.Physical, WerewolfAttributeCategoryIdentifiers.Social, WerewolfAttributeCategoryIdentifiers.Mental);

        Assert.Empty(draft.AttributePriorityOrder);
        Assert.NotSame(draft.Attributes, result.Draft?.Attributes);
        Assert.Equal(draft.Race, result.Draft?.Race);
        Assert.Equal(draft.Auspice, result.Draft?.Auspice);
        Assert.Equal(draft.Tribe, result.Draft?.Tribe);
        Assert.Equal(draft.MetisDeformity, result.Draft?.MetisDeformity);
        Assert.Equal(draft.RaceGift, result.Draft?.RaceGift);
        Assert.Equal(draft.AuspiceGift, result.Draft?.AuspiceGift);
        Assert.Equal(draft.TribeGift, result.Draft?.TribeGift);
        Assert.Equal(draft.DisabledCapabilities, result.Draft?.DisabledCapabilities);
        Assert.Empty(result.Draft?.Gifts ?? []);
    }

    [Fact]
    public void NextStepsAreDeterministicAndAllocationRemainsExplicit()
    {
        var first = Select(Draft(), WerewolfAttributeCategoryIdentifiers.Physical, WerewolfAttributeCategoryIdentifiers.Social, WerewolfAttributeCategoryIdentifiers.Mental);
        var second = Select(Draft(), WerewolfAttributeCategoryIdentifiers.Physical, WerewolfAttributeCategoryIdentifiers.Social, WerewolfAttributeCategoryIdentifiers.Mental);

        Assert.Equal(first.Draft?.RequiredNextSteps, second.Draft?.RequiredNextSteps);
        Assert.Contains("allocate-attributes", first.Draft?.RequiredNextSteps ?? []);
        Assert.DoesNotContain("select-attribute-priorities", first.Draft?.RequiredNextSteps ?? []);
        Assert.All(first.Draft?.Attributes ?? new Dictionary<string, int?>(), attribute => Assert.Null(attribute.Value));
    }

    [Fact]
    public void RuntimeRegistryInvokesAttributePrioritySelection()
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
            WerewolfReferenceRuntime.SelectAttributePrioritiesOperation,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["draftId"] = created.Outputs["draftId"],
                ["draftVersion"] = created.Outputs["draftVersion"],
                ["expectedDraftVersion"] = created.Outputs["draftVersion"],
                ["primaryCategoryId"] = WerewolfAttributeCategoryIdentifiers.Physical,
                ["secondaryCategoryId"] = WerewolfAttributeCategoryIdentifiers.Social,
                ["tertiaryCategoryId"] = WerewolfAttributeCategoryIdentifiers.Mental
            }));

        Assert.True(selected.Succeeded);
        Assert.Equal("physical,social,mental", selected.Outputs["attributePriorityOrder"]);
        Assert.Equal("mental:3,physical:7,social:5", selected.Outputs["attributeBudgets"]);
        Assert.Equal("2", selected.Outputs["draftVersion"]);
    }

    [Fact]
    public void AttributePrioritySelectionHasNoForbiddenDependencies()
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "rule-sets", "Chronicle.RuleSets.Werewolf", "CharacterCreation", "WerewolfAttributePrioritySelection.cs"));
        var forbidden = new[] { "Chronicle.Persistence", "Chronicle.Presentation", "OpenAI", "HttpClient", "DbContext", "File.", "Directory.", "Random", "Campaign" };

        Assert.DoesNotContain(forbidden, token => source.Contains(token, StringComparison.Ordinal));
    }

    private static WerewolfAttributePrioritySelectionResult Select(WerewolfInitializedCharacterState draft, string primary, string secondary, string tertiary)
    {
        return WerewolfAttributePrioritySelectionService.SelectPriorities(new WerewolfAttributePrioritySelectionRequest(draft, draft.DraftVersion, primary, secondary, tertiary));
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
