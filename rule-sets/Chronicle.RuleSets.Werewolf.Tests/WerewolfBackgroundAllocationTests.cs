using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfBackgroundAllocationTests
{
    public static TheoryData<string> EveryBackground => new()
    {
        WerewolfBackgroundIdentifiers.Allies,
        WerewolfBackgroundIdentifiers.Ancestors,
        WerewolfBackgroundIdentifiers.Contacts,
        WerewolfBackgroundIdentifiers.Fetish,
        WerewolfBackgroundIdentifiers.Kinfolk,
        WerewolfBackgroundIdentifiers.Mentor,
        WerewolfBackgroundIdentifiers.PureBreed,
        WerewolfBackgroundIdentifiers.Resources,
        WerewolfBackgroundIdentifiers.Rites
    };

    [Theory]
    [MemberData(nameof(EveryBackground))]
    public void SupportsEveryCanonicalCurrentSliceBackgroundIdentifier(string backgroundId)
    {
        Assert.Contains(backgroundId, WerewolfBackgroundIdentifiers.Supported);
    }

    [Fact]
    public void AllocatesValidCompleteBackgroundPayload()
    {
        var draft = Draft();

        var result = Allocate(draft, ValidAllocation());

        Assert.True(result.Succeeded, Format(result.Findings));
        Assert.Equal(draft.DraftVersion + 1, result.Draft?.DraftVersion);
        Assert.Equal(5, result.Spent);
        Assert.Equal(5, result.Budget);
        Assert.Equal(0, result.Remaining);
        Assert.Equal(2, result.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.Allies]);
        Assert.Equal(0, result.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.Ancestors]);
        Assert.Equal(1, result.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.Contacts]);
        Assert.Equal(0, result.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.Fetish]);
        Assert.Equal(0, result.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.Kinfolk]);
        Assert.Equal(0, result.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.Mentor]);
        Assert.Equal(0, result.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.PureBreed]);
        Assert.Equal(1, result.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.Resources]);
        Assert.Equal(1, result.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.Rites]);
        Assert.DoesNotContain(WerewolfBackgroundAllocationService.AllocateBackgroundsStep, result.Draft?.RequiredNextSteps ?? []);
    }

    [Fact]
    public void AllowsZeroValuedEntriesAndRequiresCompletePayload()
    {
        var draft = Draft();

        var result = Allocate(draft, ValidAllocation().Where(allocation => allocation.BackgroundId != WerewolfBackgroundIdentifiers.Mentor).ToArray());

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfBackgroundAllocationErrorCode.MissingBackground);
    }

    [Fact]
    public void EnforcesMinimumMaximumAndExactBudget()
    {
        var draft = Draft();

        AssertCode(draft, ValidAllocation().Select(item => item.BackgroundId == WerewolfBackgroundIdentifiers.Allies ? item with { Rating = -1 } : item).ToArray(), WerewolfBackgroundAllocationErrorCode.ValueBelowMinimum);
        AssertCode(draft, ValidAllocation().Select(item => item.BackgroundId == WerewolfBackgroundIdentifiers.Allies ? item with { Rating = 6 } : item).ToArray(), WerewolfBackgroundAllocationErrorCode.ValueAboveMaximum);
        AssertCode(draft, ValidAllocation().Select(item => item.BackgroundId == WerewolfBackgroundIdentifiers.Contacts ? item with { Rating = 2 } : item).ToArray(), WerewolfBackgroundAllocationErrorCode.IncorrectTotal);

        var maxSingleBackground = Allocate(draft, [
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Allies, 5),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Ancestors, 0),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Contacts, 0),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Fetish, 0),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Kinfolk, 0),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Mentor, 0),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.PureBreed, 0),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Resources, 0),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Rites, 0)
        ]);

        Assert.True(maxSingleBackground.Succeeded);
    }

    [Fact]
    public void RejectsMissingTribeDuplicateUnknownMalformedRestrictedAndStalePayloads()
    {
        var draft = Draft();
        var valid = ValidAllocation();

        var missingTribe = Allocate(draft with { Tribe = null }, valid);
        Assert.False(missingTribe.Succeeded);
        Assert.Contains(missingTribe.Findings, finding => finding.Code == WerewolfBackgroundAllocationErrorCode.MissingTribe);

        AssertCode(draft, valid.Concat([new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Allies, 0)]).ToArray(), WerewolfBackgroundAllocationErrorCode.DuplicateBackground);
        AssertCode(draft, valid.Select(item => item.BackgroundId == WerewolfBackgroundIdentifiers.Rites ? item with { BackgroundId = "character.background.totem" } : item).ToArray(), WerewolfBackgroundAllocationErrorCode.UnknownBackground);
        AssertCode(draft, valid.Select(item => item.BackgroundId == WerewolfBackgroundIdentifiers.Rites ? item with { BackgroundId = " character.background.rites" } : item).ToArray(), WerewolfBackgroundAllocationErrorCode.MalformedBackground);
        AssertCode(draft, valid.Select(item => item.BackgroundId == WerewolfBackgroundIdentifiers.Mentor ? item with { Rating = 1 } : item).ToArray(), WerewolfBackgroundAllocationErrorCode.RestrictedBackground);

        var stale = WerewolfBackgroundAllocationService.AllocateBackgrounds(new WerewolfBackgroundAllocationRequest(draft, draft.DraftVersion - 1, valid));
        Assert.False(stale.Succeeded);
        Assert.Contains(stale.Findings, finding => finding.Code == WerewolfBackgroundAllocationErrorCode.StaleDraftVersion);
    }

    [Fact]
    public void ReplacesPreviousAllocationAtomicallyAndImmutably()
    {
        var draft = Draft();
        var first = Allocate(draft, ValidAllocation()).Draft!;
        var replacement = Allocate(first, [
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Allies, 1),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Ancestors, 0),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Contacts, 1),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Fetish, 0),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Kinfolk, 0),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Mentor, 0),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.PureBreed, 0),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Resources, 2),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Rites, 1)
        ]);

        Assert.True(replacement.Succeeded, Format(replacement.Findings));
        Assert.NotSame(first.Backgrounds, replacement.Draft?.Backgrounds);
        Assert.Equal(1, replacement.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.Allies]);
        Assert.Equal(0, replacement.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.Ancestors]);
        Assert.Equal(1, replacement.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.Contacts]);
        Assert.Equal(0, replacement.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.Fetish]);
        Assert.Equal(0, replacement.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.Kinfolk]);
        Assert.Equal(0, replacement.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.Mentor]);
        Assert.Equal(0, replacement.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.PureBreed]);
        Assert.Equal(2, replacement.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.Resources]);
        Assert.Equal(1, replacement.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.Rites]);
        Assert.Equal(first.DraftVersion + 1, replacement.Draft?.DraftVersion);
    }

    [Fact]
    public void PreservesExistingDraftState()
    {
        var draft = Draft() with
        {
            Race = WerewolfRaceIdentifiers.Metis,
            Auspice = WerewolfAuspiceIdentifiers.Theurge,
            MetisDeformity = WerewolfMetisDeformityIdentifiers.Horns,
            RaceGift = WerewolfInitialGiftIdentifiers.MetisCreateElement,
            AuspiceGift = WerewolfInitialGiftIdentifiers.TheurgeSpiritSpeech,
            TribeGift = WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine,
            AttributePriorityOrder = Array.AsReadOnly([WerewolfAttributeCategoryIdentifiers.Physical, WerewolfAttributeCategoryIdentifiers.Social, WerewolfAttributeCategoryIdentifiers.Mental]),
            AbilityPriorityOrder = Array.AsReadOnly([WerewolfAbilityCategoryIdentifiers.Talents, WerewolfAbilityCategoryIdentifiers.Skills, WerewolfAbilityCategoryIdentifiers.Knowledges]),
            Attributes = new Dictionary<string, int?>(StringComparer.Ordinal) { [WerewolfAttributeIdentifiers.Strength] = 5 },
            Abilities = new Dictionary<string, int?>(StringComparer.Ordinal) { [WerewolfAbilityIdentifiers.Alertness] = 3 }
        };

        var result = Allocate(draft, ValidAllocation());

        Assert.True(result.Succeeded);
        Assert.Equal(draft.Race, result.Draft?.Race);
        Assert.Equal(draft.Auspice, result.Draft?.Auspice);
        Assert.Equal(draft.Tribe, result.Draft?.Tribe);
        Assert.Equal(draft.MetisDeformity, result.Draft?.MetisDeformity);
        Assert.Equal(draft.RaceGift, result.Draft?.RaceGift);
        Assert.Equal(draft.AuspiceGift, result.Draft?.AuspiceGift);
        Assert.Equal(draft.TribeGift, result.Draft?.TribeGift);
        Assert.Equal(draft.AttributePriorityOrder, result.Draft?.AttributePriorityOrder);
        Assert.Equal(draft.AbilityPriorityOrder, result.Draft?.AbilityPriorityOrder);
        Assert.Equal(draft.Attributes, result.Draft?.Attributes);
        Assert.Equal(draft.Abilities, result.Draft?.Abilities);
        Assert.Equal(draft.DisabledCapabilities, result.Draft?.DisabledCapabilities);
    }

    [Fact]
    public void TribeChangeClearsOnlyNewlyInvalidBackgroundAllocation()
    {
        var draft = Draft() with
        {
            Tribe = "legacy-tribe",
            Backgrounds = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfBackgroundIdentifiers.Allies] = 2,
                [WerewolfBackgroundIdentifiers.Contacts] = 1,
                [WerewolfBackgroundIdentifiers.Mentor] = 1,
                [WerewolfBackgroundIdentifiers.Resources] = 1,
                [WerewolfBackgroundIdentifiers.Rites] = 0
            }
        };

        var result = WerewolfTribeSelectionService.SelectTribe(new WerewolfTribeSelectionRequest(draft, draft.DraftVersion, WerewolfTribeIdentifiers.GlassWalkers));

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.Allies]);
        Assert.Null(result.Draft?.Backgrounds[WerewolfBackgroundIdentifiers.Mentor]);
    }

    [Fact]
    public void RuntimeRegistryInvokesBackgroundAllocation()
    {
        var registry = RuntimeRegistry();
        var created = registry.Execute(Request(WerewolfReferenceRuntime.CreateCharacterOperation, new Dictionary<string, string>(StringComparer.Ordinal) { ["requestId"] = "request-001" }));
        var tribe = registry.Execute(Request(WerewolfReferenceRuntime.SelectTribeOperation, new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["draftId"] = created.Outputs["draftId"],
            ["draftVersion"] = created.Outputs["draftVersion"],
            ["expectedDraftVersion"] = created.Outputs["draftVersion"],
            ["tribeId"] = WerewolfTribeIdentifiers.GlassWalkers
        }));

        var allocated = registry.Execute(Request(WerewolfReferenceRuntime.AllocateBackgroundsOperation, new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["draftId"] = tribe.Outputs["draftId"],
            ["draftVersion"] = tribe.Outputs["draftVersion"],
            ["expectedDraftVersion"] = tribe.Outputs["draftVersion"],
            ["currentTribe"] = tribe.Outputs["tribeId"],
            ["backgrounds"] = Format(ValidAllocation())
        }));

        Assert.True(allocated.Succeeded, Format(allocated.Findings));
        Assert.Equal("3", allocated.Outputs["draftVersion"]);
        Assert.Equal("5/5/0", allocated.Outputs["backgroundTotal"]);
        Assert.Contains("character.background.allies:2", allocated.Outputs["backgrounds"], StringComparison.Ordinal);
        Assert.DoesNotContain(WerewolfBackgroundAllocationService.AllocateBackgroundsStep, allocated.Outputs["nextSteps"], StringComparison.Ordinal);
    }

    [Fact]
    public void BackgroundAllocationHasNoForbiddenDependencies()
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "rule-sets", "Chronicle.RuleSets.Werewolf", "CharacterCreation", "WerewolfBackgroundAllocation.cs"));
        var forbidden = new[] { "Chronicle.Persistence", "Chronicle.Presentation", "OpenAI", "HttpClient", "DbContext", "File.", "Directory.", "Random", "Campaign" };

        Assert.DoesNotContain(forbidden, token => source.Contains(token, StringComparison.Ordinal));
    }

    private static void AssertCode(WerewolfInitializedCharacterState draft, IReadOnlyList<WerewolfBackgroundRatingAllocation> allocations, WerewolfBackgroundAllocationErrorCode code)
    {
        var result = Allocate(draft, allocations);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == code);
    }

    private static WerewolfBackgroundAllocationResult Allocate(WerewolfInitializedCharacterState draft, IReadOnlyList<WerewolfBackgroundRatingAllocation> allocations)
    {
        return WerewolfBackgroundAllocationService.AllocateBackgrounds(new WerewolfBackgroundAllocationRequest(draft, draft.DraftVersion, allocations));
    }

    private static WerewolfInitializedCharacterState Draft()
    {
        return WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity("draft-001"), 1) with
        {
            Tribe = WerewolfTribeIdentifiers.GlassWalkers
        };
    }

    private static WerewolfBackgroundRatingAllocation[] ValidAllocation()
    {
        return
        [
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Allies, 2),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Ancestors, 0),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Contacts, 1),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Fetish, 0),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Kinfolk, 0),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Mentor, 0),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.PureBreed, 0),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Resources, 1),
            new WerewolfBackgroundRatingAllocation(WerewolfBackgroundIdentifiers.Rites, 1)
        ];
    }

    private static string Format(IReadOnlyList<WerewolfBackgroundRatingAllocation> allocations)
    {
        return string.Join(",", allocations.Select(allocation => $"{allocation.BackgroundId}:{allocation.Rating.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));
    }

    private static string Format(IEnumerable<RuleSetRuntimeFinding> findings)
    {
        return string.Join(Environment.NewLine, findings.Select(finding => $"{finding.Severity}|{finding.Code}|{finding.Message}"));
    }

    private static string Format(IEnumerable<WerewolfBackgroundAllocationFinding> findings)
    {
        return string.Join(Environment.NewLine, findings.Select(finding => $"{finding.Severity}|{finding.Code}|{finding.Message}"));
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
