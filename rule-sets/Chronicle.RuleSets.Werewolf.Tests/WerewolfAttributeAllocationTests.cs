using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfAttributeAllocationTests
{
    public static TheoryData<string> EveryAttribute => new()
    {
        WerewolfAttributeIdentifiers.Strength,
        WerewolfAttributeIdentifiers.Dexterity,
        WerewolfAttributeIdentifiers.Stamina,
        WerewolfAttributeIdentifiers.Charisma,
        WerewolfAttributeIdentifiers.Manipulation,
        WerewolfAttributeIdentifiers.Appearance,
        WerewolfAttributeIdentifiers.Perception,
        WerewolfAttributeIdentifiers.Intelligence,
        WerewolfAttributeIdentifiers.Wits
    };

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
    [MemberData(nameof(EveryAttribute))]
    public void SupportsEveryCanonicalAttributeIdentifier(string attributeId)
    {
        Assert.Contains(attributeId, WerewolfAttributeIdentifiers.Supported);
    }

    [Theory]
    [MemberData(nameof(ValidPriorityOrders))]
    public void AllocatesValidCompletePayloadForEveryPriorityOrdering(string primary, string secondary, string tertiary)
    {
        var draft = PrioritizedDraft(primary, secondary, tertiary);

        var result = Allocate(draft, ValidAllocation(primary, secondary, tertiary));

        Assert.True(result.Succeeded);
        Assert.Equal(draft.DraftVersion + 1, result.Draft?.DraftVersion);
        Assert.All(result.CategoryTotals, total => Assert.Equal(0, total.Remaining));
        Assert.DoesNotContain(WerewolfAttributeAllocationService.AllocateAttributesStep, result.Draft?.RequiredNextSteps ?? []);
        Assert.Equal(5, result.Draft?.Attributes[FirstAttribute(primary)]);
        Assert.Equal(4, result.Draft?.Attributes[FirstAttribute(secondary)]);
        Assert.Equal(2, result.Draft?.Attributes[FirstAttribute(tertiary)]);
    }

    [Fact]
    public void AccountsForAuthoritativeBaseDotConvention()
    {
        var draft = PrioritizedDraft(
            WerewolfAttributeCategoryIdentifiers.Physical,
            WerewolfAttributeCategoryIdentifiers.Social,
            WerewolfAttributeCategoryIdentifiers.Mental);

        var result = Allocate(draft, ValidAllocation(
            WerewolfAttributeCategoryIdentifiers.Physical,
            WerewolfAttributeCategoryIdentifiers.Social,
            WerewolfAttributeCategoryIdentifiers.Mental));

        Assert.True(result.Succeeded);
        Assert.Contains(result.CategoryTotals, total => total.CategoryId == WerewolfAttributeCategoryIdentifiers.Physical && total.Spent == 7 && total.Budget == 7);
        Assert.Contains(result.CategoryTotals, total => total.CategoryId == WerewolfAttributeCategoryIdentifiers.Social && total.Spent == 5 && total.Budget == 5);
        Assert.Contains(result.CategoryTotals, total => total.CategoryId == WerewolfAttributeCategoryIdentifiers.Mental && total.Spent == 3 && total.Budget == 3);
    }

    [Fact]
    public void RequiresPrioritiesBeforeAllocation()
    {
        var result = Allocate(Draft(), ValidAllocation(
            WerewolfAttributeCategoryIdentifiers.Physical,
            WerewolfAttributeCategoryIdentifiers.Social,
            WerewolfAttributeCategoryIdentifiers.Mental));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfAttributeAllocationErrorCode.MissingPriorities);
    }

    [Fact]
    public void RejectsIncorrectCategoryTotals()
    {
        var draft = PrioritizedDraft(
            WerewolfAttributeCategoryIdentifiers.Physical,
            WerewolfAttributeCategoryIdentifiers.Social,
            WerewolfAttributeCategoryIdentifiers.Mental);
        var allocations = ValidAllocation(
            WerewolfAttributeCategoryIdentifiers.Physical,
            WerewolfAttributeCategoryIdentifiers.Social,
            WerewolfAttributeCategoryIdentifiers.Mental)
            .Select(allocation => allocation.AttributeId == WerewolfAttributeIdentifiers.Strength ? allocation with { Rating = 4 } : allocation)
            .ToArray();

        var result = Allocate(draft, allocations);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfAttributeAllocationErrorCode.IncorrectCategoryTotal);
        Assert.Contains(result.CategoryTotals, total => total.CategoryId == WerewolfAttributeCategoryIdentifiers.Physical && total.Remaining == 1);
    }

    [Fact]
    public void RejectsMissingDuplicateUnknownMalformedNegativeAndExcessiveValues()
    {
        var draft = PrioritizedDraft(
            WerewolfAttributeCategoryIdentifiers.Physical,
            WerewolfAttributeCategoryIdentifiers.Social,
            WerewolfAttributeCategoryIdentifiers.Mental);
        var valid = ValidAllocation(
            WerewolfAttributeCategoryIdentifiers.Physical,
            WerewolfAttributeCategoryIdentifiers.Social,
            WerewolfAttributeCategoryIdentifiers.Mental);

        AssertCode(draft, valid.Where(item => item.AttributeId != WerewolfAttributeIdentifiers.Wits).ToArray(), WerewolfAttributeAllocationErrorCode.MissingAttribute);
        AssertCode(draft, valid.Concat([new WerewolfAttributeDotAllocation(WerewolfAttributeIdentifiers.Wits, 1)]).ToArray(), WerewolfAttributeAllocationErrorCode.DuplicateAttribute);
        AssertCode(draft, valid.Select(item => item.AttributeId == WerewolfAttributeIdentifiers.Wits ? item with { AttributeId = "character.attribute.memory" } : item).ToArray(), WerewolfAttributeAllocationErrorCode.UnknownAttribute);
        AssertCode(draft, valid.Select(item => item.AttributeId == WerewolfAttributeIdentifiers.Wits ? item with { AttributeId = " character.attribute.wits" } : item).ToArray(), WerewolfAttributeAllocationErrorCode.MalformedAttribute);
        AssertCode(draft, valid.Select(item => item.AttributeId == WerewolfAttributeIdentifiers.Wits ? item with { Rating = -1 } : item).ToArray(), WerewolfAttributeAllocationErrorCode.ValueBelowMinimum);
        AssertCode(draft, valid.Select(item => item.AttributeId == WerewolfAttributeIdentifiers.Strength ? item with { Rating = 6 } : item).ToArray(), WerewolfAttributeAllocationErrorCode.ValueAboveMaximum);
    }

    [Fact]
    public void RejectsStaleVersion()
    {
        var draft = PrioritizedDraft(
            WerewolfAttributeCategoryIdentifiers.Physical,
            WerewolfAttributeCategoryIdentifiers.Social,
            WerewolfAttributeCategoryIdentifiers.Mental);

        var result = WerewolfAttributeAllocationService.AllocateAttributes(new WerewolfAttributeAllocationRequest(
            draft,
            draft.DraftVersion - 1,
            ValidAllocation(WerewolfAttributeCategoryIdentifiers.Physical, WerewolfAttributeCategoryIdentifiers.Social, WerewolfAttributeCategoryIdentifiers.Mental)));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfAttributeAllocationErrorCode.StaleDraftVersion);
    }

    [Fact]
    public void ReplacesPreviousAllocationAtomically()
    {
        var draft = PrioritizedDraft(
            WerewolfAttributeCategoryIdentifiers.Physical,
            WerewolfAttributeCategoryIdentifiers.Social,
            WerewolfAttributeCategoryIdentifiers.Mental);
        var first = Allocate(draft, ValidAllocation(
            WerewolfAttributeCategoryIdentifiers.Physical,
            WerewolfAttributeCategoryIdentifiers.Social,
            WerewolfAttributeCategoryIdentifiers.Mental)).Draft!;

        var replacementPayload = ValidAllocation(
            WerewolfAttributeCategoryIdentifiers.Physical,
            WerewolfAttributeCategoryIdentifiers.Social,
            WerewolfAttributeCategoryIdentifiers.Mental)
            .Select(allocation => allocation.AttributeId switch
            {
                WerewolfAttributeIdentifiers.Strength => allocation with { Rating = 3 },
                WerewolfAttributeIdentifiers.Dexterity => allocation with { Rating = 5 },
                _ => allocation
            })
            .ToArray();
        var replacement = Allocate(first, replacementPayload);

        Assert.True(replacement.Succeeded);
        Assert.Equal(3, replacement.Draft?.Attributes[WerewolfAttributeIdentifiers.Strength]);
        Assert.Equal(5, replacement.Draft?.Attributes[WerewolfAttributeIdentifiers.Dexterity]);
        Assert.Equal(first.DraftVersion + 1, replacement.Draft?.DraftVersion);
    }

    [Fact]
    public void UpdatesImmutablyAndPreservesPriorDraftState()
    {
        var draft = PrioritizedDraft(
            WerewolfAttributeCategoryIdentifiers.Physical,
            WerewolfAttributeCategoryIdentifiers.Social,
            WerewolfAttributeCategoryIdentifiers.Mental) with
        {
            Race = WerewolfRaceIdentifiers.Metis,
            Auspice = WerewolfAuspiceIdentifiers.Theurge,
            Tribe = WerewolfTribeIdentifiers.GlassWalkers,
            MetisDeformity = WerewolfMetisDeformityIdentifiers.Horns,
            RaceGift = WerewolfGiftIdentifiers.MetisCreateElement,
            AuspiceGift = WerewolfGiftIdentifiers.TheurgeSpiritSpeech,
            TribeGift = WerewolfGiftIdentifiers.GlassWalkersControlSimpleMachine
        };

        var result = Allocate(draft, ValidAllocation(
            WerewolfAttributeCategoryIdentifiers.Physical,
            WerewolfAttributeCategoryIdentifiers.Social,
            WerewolfAttributeCategoryIdentifiers.Mental));

        Assert.True(result.Succeeded);
        Assert.NotSame(draft.Attributes, result.Draft?.Attributes);
        Assert.All(draft.Attributes, attribute => Assert.Null(attribute.Value));
        Assert.Equal(draft.Race, result.Draft?.Race);
        Assert.Equal(draft.Auspice, result.Draft?.Auspice);
        Assert.Equal(draft.Tribe, result.Draft?.Tribe);
        Assert.Equal(draft.MetisDeformity, result.Draft?.MetisDeformity);
        Assert.Equal(draft.RaceGift, result.Draft?.RaceGift);
        Assert.Equal(draft.AuspiceGift, result.Draft?.AuspiceGift);
        Assert.Equal(draft.TribeGift, result.Draft?.TribeGift);
        Assert.Equal(draft.AttributePriorityOrder, result.Draft?.AttributePriorityOrder);
        Assert.Equal(draft.AttributeBudgets, result.Draft?.AttributeBudgets);
        Assert.Equal(draft.DisabledCapabilities, result.Draft?.DisabledCapabilities);
    }

    [Fact]
    public void PriorityChangeClearsPriorAttributeAllocation()
    {
        var draft = PrioritizedDraft(
            WerewolfAttributeCategoryIdentifiers.Physical,
            WerewolfAttributeCategoryIdentifiers.Social,
            WerewolfAttributeCategoryIdentifiers.Mental);
        var allocated = Allocate(draft, ValidAllocation(
            WerewolfAttributeCategoryIdentifiers.Physical,
            WerewolfAttributeCategoryIdentifiers.Social,
            WerewolfAttributeCategoryIdentifiers.Mental)).Draft!;

        var changed = WerewolfAttributePrioritySelectionService.SelectPriorities(new WerewolfAttributePrioritySelectionRequest(
            allocated,
            allocated.DraftVersion,
            WerewolfAttributeCategoryIdentifiers.Mental,
            WerewolfAttributeCategoryIdentifiers.Physical,
            WerewolfAttributeCategoryIdentifiers.Social));

        Assert.True(changed.Succeeded);
        Assert.All(changed.Draft?.Attributes ?? new Dictionary<string, int?>(), attribute => Assert.Null(attribute.Value));
        Assert.Contains(WerewolfAttributeAllocationService.AllocateAttributesStep, changed.Draft?.RequiredNextSteps ?? []);
    }

    [Fact]
    public void RuntimeRegistryInvokesAttributeAllocation()
    {
        var registry = RuntimeRegistry();
        var created = registry.Execute(Request(WerewolfReferenceRuntime.CreateCharacterOperation, new Dictionary<string, string>(StringComparer.Ordinal) { ["requestId"] = "request-001" }));
        var priorities = registry.Execute(Request(WerewolfReferenceRuntime.SelectAttributePrioritiesOperation, new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["draftId"] = created.Outputs["draftId"],
            ["draftVersion"] = created.Outputs["draftVersion"],
            ["expectedDraftVersion"] = created.Outputs["draftVersion"],
            ["primaryCategoryId"] = WerewolfAttributeCategoryIdentifiers.Physical,
            ["secondaryCategoryId"] = WerewolfAttributeCategoryIdentifiers.Social,
            ["tertiaryCategoryId"] = WerewolfAttributeCategoryIdentifiers.Mental
        }));

        var allocated = registry.Execute(Request(WerewolfReferenceRuntime.AllocateAttributesOperation, new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["draftId"] = priorities.Outputs["draftId"],
            ["draftVersion"] = priorities.Outputs["draftVersion"],
            ["expectedDraftVersion"] = priorities.Outputs["draftVersion"],
            ["attributePriorityOrder"] = priorities.Outputs["attributePriorityOrder"],
            ["attributeBudgets"] = priorities.Outputs["attributeBudgets"],
            ["attributes"] = Format(ValidAllocation(WerewolfAttributeCategoryIdentifiers.Physical, WerewolfAttributeCategoryIdentifiers.Social, WerewolfAttributeCategoryIdentifiers.Mental))
        }));

        Assert.True(allocated.Succeeded);
        Assert.Equal("3", allocated.Outputs["draftVersion"]);
        Assert.Contains("physical:7/7/0", allocated.Outputs["attributeCategoryTotals"], StringComparison.Ordinal);
        Assert.Contains("character.attribute.strength:5", allocated.Outputs["attributes"], StringComparison.Ordinal);
        Assert.DoesNotContain(WerewolfAttributeAllocationService.AllocateAttributesStep, allocated.Outputs["nextSteps"], StringComparison.Ordinal);
    }

    [Fact]
    public void AttributeAllocationHasNoForbiddenDependencies()
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "rule-sets", "Chronicle.RuleSets.Werewolf", "CharacterCreation", "WerewolfAttributeAllocation.cs"));
        var forbidden = new[] { "Chronicle.Persistence", "Chronicle.Presentation", "OpenAI", "HttpClient", "DbContext", "File.", "Directory.", "Random", "Campaign" };

        Assert.DoesNotContain(forbidden, token => source.Contains(token, StringComparison.Ordinal));
    }

    private static void AssertCode(WerewolfInitializedCharacterState draft, IReadOnlyList<WerewolfAttributeDotAllocation> allocations, WerewolfAttributeAllocationErrorCode code)
    {
        var result = Allocate(draft, allocations);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == code);
    }

    private static WerewolfAttributeAllocationResult Allocate(WerewolfInitializedCharacterState draft, IReadOnlyList<WerewolfAttributeDotAllocation> allocations)
    {
        return WerewolfAttributeAllocationService.AllocateAttributes(new WerewolfAttributeAllocationRequest(draft, draft.DraftVersion, allocations));
    }

    private static WerewolfInitializedCharacterState PrioritizedDraft(string primary, string secondary, string tertiary)
    {
        return WerewolfAttributePrioritySelectionService.SelectPriorities(new WerewolfAttributePrioritySelectionRequest(Draft(), 1, primary, secondary, tertiary)).Draft!;
    }

    private static WerewolfInitializedCharacterState Draft()
    {
        return WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity("draft-001"), 1);
    }

    private static WerewolfAttributeDotAllocation[] ValidAllocation(string primary, string secondary, string tertiary)
    {
        var values = new Dictionary<string, int>(StringComparer.Ordinal);
        AddCategory(values, primary, [5, 3, 2]);
        AddCategory(values, secondary, [4, 3, 1]);
        AddCategory(values, tertiary, [2, 2, 2]);

        return values
            .OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .Select(entry => new WerewolfAttributeDotAllocation(entry.Key, entry.Value))
            .ToArray();
    }

    private static void AddCategory(Dictionary<string, int> values, string category, IReadOnlyList<int> ratings)
    {
        var attributes = AttributesInCategory(category);
        for (var index = 0; index < attributes.Length; index += 1)
        {
            values[attributes[index]] = ratings[index];
        }
    }

    private static string FirstAttribute(string category)
    {
        return AttributesInCategory(category)[0];
    }

    private static string[] AttributesInCategory(string category)
    {
        return category switch
        {
            WerewolfAttributeCategoryIdentifiers.Physical => [WerewolfAttributeIdentifiers.Strength, WerewolfAttributeIdentifiers.Dexterity, WerewolfAttributeIdentifiers.Stamina],
            WerewolfAttributeCategoryIdentifiers.Social => [WerewolfAttributeIdentifiers.Charisma, WerewolfAttributeIdentifiers.Manipulation, WerewolfAttributeIdentifiers.Appearance],
            WerewolfAttributeCategoryIdentifiers.Mental => [WerewolfAttributeIdentifiers.Perception, WerewolfAttributeIdentifiers.Intelligence, WerewolfAttributeIdentifiers.Wits],
            _ => []
        };
    }

    private static string Format(IReadOnlyList<WerewolfAttributeDotAllocation> allocations)
    {
        return string.Join(",", allocations.Select(allocation => $"{allocation.AttributeId}:{allocation.Rating.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));
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
