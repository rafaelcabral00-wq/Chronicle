using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfAbilitySelectionTests
{
    public static TheoryData<string> EveryAbility => new()
    {
        WerewolfAbilityIdentifiers.Alertness,
        WerewolfAbilityIdentifiers.Athletics,
        WerewolfAbilityIdentifiers.Brawl,
        WerewolfAbilityIdentifiers.Computer,
        WerewolfAbilityIdentifiers.Drive,
        WerewolfAbilityIdentifiers.Empathy,
        WerewolfAbilityIdentifiers.Etiquette,
        WerewolfAbilityIdentifiers.Expression,
        WerewolfAbilityIdentifiers.Intimidation,
        WerewolfAbilityIdentifiers.Investigation,
        WerewolfAbilityIdentifiers.Law,
        WerewolfAbilityIdentifiers.Leadership,
        WerewolfAbilityIdentifiers.Occult,
        WerewolfAbilityIdentifiers.Performance,
        WerewolfAbilityIdentifiers.Politics,
        WerewolfAbilityIdentifiers.Stealth,
        WerewolfAbilityIdentifiers.Subterfuge,
        WerewolfAbilityIdentifiers.Survival
    };

    public static TheoryData<string, string, string> ValidPriorityOrders => new()
    {
        { WerewolfAbilityCategoryIdentifiers.Talents, WerewolfAbilityCategoryIdentifiers.Skills, WerewolfAbilityCategoryIdentifiers.Knowledges },
        { WerewolfAbilityCategoryIdentifiers.Talents, WerewolfAbilityCategoryIdentifiers.Knowledges, WerewolfAbilityCategoryIdentifiers.Skills },
        { WerewolfAbilityCategoryIdentifiers.Skills, WerewolfAbilityCategoryIdentifiers.Talents, WerewolfAbilityCategoryIdentifiers.Knowledges },
        { WerewolfAbilityCategoryIdentifiers.Skills, WerewolfAbilityCategoryIdentifiers.Knowledges, WerewolfAbilityCategoryIdentifiers.Talents },
        { WerewolfAbilityCategoryIdentifiers.Knowledges, WerewolfAbilityCategoryIdentifiers.Talents, WerewolfAbilityCategoryIdentifiers.Skills },
        { WerewolfAbilityCategoryIdentifiers.Knowledges, WerewolfAbilityCategoryIdentifiers.Skills, WerewolfAbilityCategoryIdentifiers.Talents }
    };

    [Theory]
    [MemberData(nameof(EveryAbility))]
    public void SupportsEveryCanonicalAbilityIdentifier(string abilityId)
    {
        Assert.Contains(abilityId, WerewolfAbilityIdentifiers.Supported);
    }

    [Theory]
    [MemberData(nameof(ValidPriorityOrders))]
    public void SelectsAbilityPrioritiesForEveryOrdering(string primary, string secondary, string tertiary)
    {
        var draft = Draft();

        var result = WerewolfAbilitySelectionService.SelectPriorities(new WerewolfAbilityPrioritySelectionRequest(draft, draft.DraftVersion, primary, secondary, tertiary));

        Assert.True(result.Succeeded);
        Assert.Equal(draft.DraftVersion + 1, result.Draft?.DraftVersion);
        Assert.Equal([primary, secondary, tertiary], result.Draft?.AbilityPriorityOrder);
        Assert.Equal(13, result.Draft?.AbilityBudgets[primary]);
        Assert.Equal(9, result.Draft?.AbilityBudgets[secondary]);
        Assert.Equal(5, result.Draft?.AbilityBudgets[tertiary]);
        Assert.Contains(WerewolfAbilitySelectionService.AllocateAbilitiesStep, result.Draft?.RequiredNextSteps ?? []);
    }

    [Fact]
    public void RejectsInvalidAbilityPriorityPayloads()
    {
        var draft = Draft();

        AssertPriorityCode(draft, "", WerewolfAbilityCategoryIdentifiers.Skills, WerewolfAbilityCategoryIdentifiers.Knowledges, WerewolfAbilityPrioritySelectionErrorCode.MissingCategory);
        AssertPriorityCode(draft, " talents", WerewolfAbilityCategoryIdentifiers.Skills, WerewolfAbilityCategoryIdentifiers.Knowledges, WerewolfAbilityPrioritySelectionErrorCode.MalformedCategory);
        AssertPriorityCode(draft, "crafts", WerewolfAbilityCategoryIdentifiers.Skills, WerewolfAbilityCategoryIdentifiers.Knowledges, WerewolfAbilityPrioritySelectionErrorCode.UnknownCategory);
        AssertPriorityCode(draft, WerewolfAbilityCategoryIdentifiers.Talents, WerewolfAbilityCategoryIdentifiers.Talents, WerewolfAbilityCategoryIdentifiers.Knowledges, WerewolfAbilityPrioritySelectionErrorCode.DuplicateCategory);
        AssertPriorityCode(draft, WerewolfAbilityCategoryIdentifiers.Talents, WerewolfAbilityCategoryIdentifiers.Skills, "", WerewolfAbilityPrioritySelectionErrorCode.MissingCategory);
    }

    [Fact]
    public void AbilityPrioritySelectionRejectsStaleVersion()
    {
        var draft = Draft();

        var result = WerewolfAbilitySelectionService.SelectPriorities(new WerewolfAbilityPrioritySelectionRequest(
            draft,
            draft.DraftVersion - 1,
            WerewolfAbilityCategoryIdentifiers.Talents,
            WerewolfAbilityCategoryIdentifiers.Skills,
            WerewolfAbilityCategoryIdentifiers.Knowledges));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfAbilityPrioritySelectionErrorCode.StaleDraftVersion);
    }

    [Theory]
    [MemberData(nameof(ValidPriorityOrders))]
    public void AllocatesValidCompletePayloadForEveryPriorityOrdering(string primary, string secondary, string tertiary)
    {
        var draft = PrioritizedDraft(primary, secondary, tertiary, WerewolfRaceIdentifiers.Homid);

        var result = Allocate(draft, ValidAllocation(primary, secondary, tertiary));

        Assert.True(result.Succeeded);
        Assert.Equal(draft.DraftVersion + 1, result.Draft?.DraftVersion);
        Assert.All(result.CategoryTotals, total => Assert.Equal(0, total.Remaining));
        Assert.DoesNotContain(WerewolfAbilitySelectionService.AllocateAbilitiesStep, result.Draft?.RequiredNextSteps ?? []);
    }

    [Fact]
    public void AccountsForAuthoritativeZeroBaseRatingConvention()
    {
        var draft = PrioritizedDraft(
            WerewolfAbilityCategoryIdentifiers.Talents,
            WerewolfAbilityCategoryIdentifiers.Skills,
            WerewolfAbilityCategoryIdentifiers.Knowledges,
            WerewolfRaceIdentifiers.Homid);

        var result = Allocate(draft, ValidAllocation(
            WerewolfAbilityCategoryIdentifiers.Talents,
            WerewolfAbilityCategoryIdentifiers.Skills,
            WerewolfAbilityCategoryIdentifiers.Knowledges));

        Assert.True(result.Succeeded);
        Assert.Contains(result.CategoryTotals, total => total.CategoryId == WerewolfAbilityCategoryIdentifiers.Talents && total.Spent == 13 && total.Budget == 13);
        Assert.Contains(result.CategoryTotals, total => total.CategoryId == WerewolfAbilityCategoryIdentifiers.Skills && total.Spent == 9 && total.Budget == 9);
        Assert.Contains(result.CategoryTotals, total => total.CategoryId == WerewolfAbilityCategoryIdentifiers.Knowledges && total.Spent == 5 && total.Budget == 5);
    }

    [Fact]
    public void RequiresPrioritiesBeforeAllocation()
    {
        var result = Allocate(Draft() with { Race = WerewolfRaceIdentifiers.Homid }, ValidAllocation(
            WerewolfAbilityCategoryIdentifiers.Talents,
            WerewolfAbilityCategoryIdentifiers.Skills,
            WerewolfAbilityCategoryIdentifiers.Knowledges));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfAbilityAllocationErrorCode.MissingPriorities);
    }

    [Fact]
    public void EnforcesApprovedLupusBaseRestrictions()
    {
        var draft = PrioritizedDraft(
            WerewolfAbilityCategoryIdentifiers.Talents,
            WerewolfAbilityCategoryIdentifiers.Skills,
            WerewolfAbilityCategoryIdentifiers.Knowledges,
            WerewolfRaceIdentifiers.Lupus);
        var allocations = ValidAllocation(
            WerewolfAbilityCategoryIdentifiers.Talents,
            WerewolfAbilityCategoryIdentifiers.Skills,
            WerewolfAbilityCategoryIdentifiers.Knowledges);

        AssertCode(draft, allocations.Select(allocation => allocation.AbilityId == WerewolfAbilityIdentifiers.Drive ? allocation with { Rating = 1 } : allocation).ToArray(), WerewolfAbilityAllocationErrorCode.RestrictedAbility);
        AssertCode(draft, allocations.Select(allocation => allocation.AbilityId == WerewolfAbilityIdentifiers.Etiquette ? allocation with { Rating = 1 } : allocation).ToArray(), WerewolfAbilityAllocationErrorCode.RestrictedAbility);
        AssertCode(draft, allocations.Select(allocation => allocation.AbilityId == WerewolfAbilityIdentifiers.Computer ? allocation with { Rating = 1 } : allocation).ToArray(), WerewolfAbilityAllocationErrorCode.RestrictedAbility);
        AssertCode(draft, allocations.Select(allocation => allocation.AbilityId == WerewolfAbilityIdentifiers.Law ? allocation with { Rating = 1 } : allocation).ToArray(), WerewolfAbilityAllocationErrorCode.RestrictedAbility);
        AssertCode(draft, allocations.Select(allocation => allocation.AbilityId == WerewolfAbilityIdentifiers.Politics ? allocation with { Rating = 1 } : allocation).ToArray(), WerewolfAbilityAllocationErrorCode.RestrictedAbility);
    }

    [Fact]
    public void AllowsLupusAllocationWhenRestrictedAbilitiesRemainAtZero()
    {
        var draft = PrioritizedDraft(
            WerewolfAbilityCategoryIdentifiers.Talents,
            WerewolfAbilityCategoryIdentifiers.Skills,
            WerewolfAbilityCategoryIdentifiers.Knowledges,
            WerewolfRaceIdentifiers.Lupus);

        var result = Allocate(draft, ValidLupusAllocation());

        Assert.True(result.Succeeded, Format(result.Findings));
        Assert.Equal(0, result.Draft?.Abilities[WerewolfAbilityIdentifiers.Drive]);
        Assert.Equal(0, result.Draft?.Abilities[WerewolfAbilityIdentifiers.Etiquette]);
        Assert.Equal(0, result.Draft?.Abilities[WerewolfAbilityIdentifiers.Computer]);
        Assert.Equal(0, result.Draft?.Abilities[WerewolfAbilityIdentifiers.Law]);
        Assert.Equal(0, result.Draft?.Abilities[WerewolfAbilityIdentifiers.Politics]);
    }

    [Fact]
    public void RejectsMissingDuplicateUnknownMalformedRestrictedBelowMinimumAboveMaximumAndIncorrectTotals()
    {
        var draft = PrioritizedDraft(
            WerewolfAbilityCategoryIdentifiers.Talents,
            WerewolfAbilityCategoryIdentifiers.Skills,
            WerewolfAbilityCategoryIdentifiers.Knowledges,
            WerewolfRaceIdentifiers.Homid);
        var valid = ValidAllocation(
            WerewolfAbilityCategoryIdentifiers.Talents,
            WerewolfAbilityCategoryIdentifiers.Skills,
            WerewolfAbilityCategoryIdentifiers.Knowledges);

        AssertCode(draft, valid.Where(item => item.AbilityId != WerewolfAbilityIdentifiers.Occult).ToArray(), WerewolfAbilityAllocationErrorCode.MissingAbility);
        AssertCode(draft, valid.Concat([new WerewolfAbilityDotAllocation(WerewolfAbilityIdentifiers.Occult, 0)]).ToArray(), WerewolfAbilityAllocationErrorCode.DuplicateAbility);
        AssertCode(draft, valid.Select(item => item.AbilityId == WerewolfAbilityIdentifiers.Occult ? item with { AbilityId = "character.ability.enigmas" } : item).ToArray(), WerewolfAbilityAllocationErrorCode.UnknownAbility);
        AssertCode(draft, valid.Select(item => item.AbilityId == WerewolfAbilityIdentifiers.Occult ? item with { AbilityId = " character.ability.occult" } : item).ToArray(), WerewolfAbilityAllocationErrorCode.MalformedAbility);
        AssertCode(draft, valid.Select(item => item.AbilityId == WerewolfAbilityIdentifiers.Occult ? item with { Rating = -1 } : item).ToArray(), WerewolfAbilityAllocationErrorCode.ValueBelowMinimum);
        AssertCode(draft, valid.Select(item => item.AbilityId == WerewolfAbilityIdentifiers.Alertness ? item with { Rating = 4 } : item).ToArray(), WerewolfAbilityAllocationErrorCode.ValueAboveMaximum);
        AssertCode(draft, valid.Select(item => item.AbilityId == WerewolfAbilityIdentifiers.Alertness ? item with { Rating = 2 } : item).ToArray(), WerewolfAbilityAllocationErrorCode.IncorrectCategoryTotal);
        AssertCode(draft with { Race = WerewolfRaceIdentifiers.Lupus }, valid.Select(item => item.AbilityId == WerewolfAbilityIdentifiers.Computer ? item with { Rating = 1 } : item).ToArray(), WerewolfAbilityAllocationErrorCode.RestrictedAbility);
    }

    [Fact]
    public void RejectsStaleVersion()
    {
        var draft = PrioritizedDraft(
            WerewolfAbilityCategoryIdentifiers.Talents,
            WerewolfAbilityCategoryIdentifiers.Skills,
            WerewolfAbilityCategoryIdentifiers.Knowledges,
            WerewolfRaceIdentifiers.Homid);

        var result = WerewolfAbilitySelectionService.AllocateAbilities(new WerewolfAbilityAllocationRequest(
            draft,
            draft.DraftVersion - 1,
            ValidAllocation(WerewolfAbilityCategoryIdentifiers.Talents, WerewolfAbilityCategoryIdentifiers.Skills, WerewolfAbilityCategoryIdentifiers.Knowledges)));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == WerewolfAbilityAllocationErrorCode.StaleDraftVersion);
    }

    [Fact]
    public void ReplacesPreviousAllocationAtomically()
    {
        var draft = PrioritizedDraft(
            WerewolfAbilityCategoryIdentifiers.Talents,
            WerewolfAbilityCategoryIdentifiers.Skills,
            WerewolfAbilityCategoryIdentifiers.Knowledges,
            WerewolfRaceIdentifiers.Homid);
        var first = Allocate(draft, ValidAllocation(
            WerewolfAbilityCategoryIdentifiers.Talents,
            WerewolfAbilityCategoryIdentifiers.Skills,
            WerewolfAbilityCategoryIdentifiers.Knowledges)).Draft!;
        var replacementPayload = ValidAllocation(
            WerewolfAbilityCategoryIdentifiers.Talents,
            WerewolfAbilityCategoryIdentifiers.Skills,
            WerewolfAbilityCategoryIdentifiers.Knowledges)
            .Select(allocation => allocation.AbilityId switch
            {
                WerewolfAbilityIdentifiers.Alertness => allocation with { Rating = 2 },
                WerewolfAbilityIdentifiers.Athletics => allocation with { Rating = 3 },
                _ => allocation
            })
            .ToArray();

        var replacement = Allocate(first, replacementPayload);

        Assert.True(replacement.Succeeded);
        Assert.Equal(2, replacement.Draft?.Abilities[WerewolfAbilityIdentifiers.Alertness]);
        Assert.Equal(3, replacement.Draft?.Abilities[WerewolfAbilityIdentifiers.Athletics]);
        Assert.Equal(first.DraftVersion + 1, replacement.Draft?.DraftVersion);
    }

    [Fact]
    public void UpdatesImmutablyAndPreservesPriorDraftState()
    {
        var draft = PrioritizedDraft(
            WerewolfAbilityCategoryIdentifiers.Talents,
            WerewolfAbilityCategoryIdentifiers.Skills,
            WerewolfAbilityCategoryIdentifiers.Knowledges,
            WerewolfRaceIdentifiers.Metis) with
        {
            Auspice = WerewolfAuspiceIdentifiers.Theurge,
            Tribe = WerewolfTribeIdentifiers.GlassWalkers,
            MetisDeformity = WerewolfMetisDeformityIdentifiers.Horns,
            RaceGift = WerewolfInitialGiftIdentifiers.MetisCreateElement,
            AuspiceGift = WerewolfInitialGiftIdentifiers.TheurgeSpiritSpeech,
            TribeGift = WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine,
            AttributePriorityOrder = Array.AsReadOnly([WerewolfAttributeCategoryIdentifiers.Physical, WerewolfAttributeCategoryIdentifiers.Social, WerewolfAttributeCategoryIdentifiers.Mental]),
            AttributeBudgets = new Dictionary<string, int>(StringComparer.Ordinal)
            {
                [WerewolfAttributeCategoryIdentifiers.Physical] = 7,
                [WerewolfAttributeCategoryIdentifiers.Social] = 5,
                [WerewolfAttributeCategoryIdentifiers.Mental] = 3
            },
            Attributes = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfAttributeIdentifiers.Strength] = 5,
                [WerewolfAttributeIdentifiers.Dexterity] = 3,
                [WerewolfAttributeIdentifiers.Stamina] = 2,
                [WerewolfAttributeIdentifiers.Charisma] = 4,
                [WerewolfAttributeIdentifiers.Manipulation] = 3,
                [WerewolfAttributeIdentifiers.Appearance] = 1,
                [WerewolfAttributeIdentifiers.Perception] = 2,
                [WerewolfAttributeIdentifiers.Intelligence] = 2,
                [WerewolfAttributeIdentifiers.Wits] = 2
            }
        };

        var result = Allocate(draft, ValidAllocation(
            WerewolfAbilityCategoryIdentifiers.Talents,
            WerewolfAbilityCategoryIdentifiers.Skills,
            WerewolfAbilityCategoryIdentifiers.Knowledges));

        Assert.True(result.Succeeded);
        Assert.NotSame(draft.Abilities, result.Draft?.Abilities);
        Assert.All(draft.Abilities, ability => Assert.Null(ability.Value));
        Assert.Equal(draft.Race, result.Draft?.Race);
        Assert.Equal(draft.Auspice, result.Draft?.Auspice);
        Assert.Equal(draft.Tribe, result.Draft?.Tribe);
        Assert.Equal(draft.MetisDeformity, result.Draft?.MetisDeformity);
        Assert.Equal(draft.RaceGift, result.Draft?.RaceGift);
        Assert.Equal(draft.AuspiceGift, result.Draft?.AuspiceGift);
        Assert.Equal(draft.TribeGift, result.Draft?.TribeGift);
        Assert.Equal(draft.AttributePriorityOrder, result.Draft?.AttributePriorityOrder);
        Assert.Equal(draft.AttributeBudgets, result.Draft?.AttributeBudgets);
        Assert.Equal(draft.Attributes, result.Draft?.Attributes);
        Assert.Equal(draft.DisabledCapabilities, result.Draft?.DisabledCapabilities);
    }

    [Fact]
    public void PriorityChangeClearsPriorAbilityAllocation()
    {
        var draft = PrioritizedDraft(
            WerewolfAbilityCategoryIdentifiers.Talents,
            WerewolfAbilityCategoryIdentifiers.Skills,
            WerewolfAbilityCategoryIdentifiers.Knowledges,
            WerewolfRaceIdentifiers.Homid);
        var allocated = Allocate(draft, ValidAllocation(
            WerewolfAbilityCategoryIdentifiers.Talents,
            WerewolfAbilityCategoryIdentifiers.Skills,
            WerewolfAbilityCategoryIdentifiers.Knowledges)).Draft!;

        var changed = WerewolfAbilitySelectionService.SelectPriorities(new WerewolfAbilityPrioritySelectionRequest(
            allocated,
            allocated.DraftVersion,
            WerewolfAbilityCategoryIdentifiers.Knowledges,
            WerewolfAbilityCategoryIdentifiers.Talents,
            WerewolfAbilityCategoryIdentifiers.Skills));

        Assert.True(changed.Succeeded);
        Assert.All(changed.Draft?.Abilities ?? new Dictionary<string, int?>(), ability => Assert.Null(ability.Value));
        Assert.Contains(WerewolfAbilitySelectionService.AllocateAbilitiesStep, changed.Draft?.RequiredNextSteps ?? []);
    }

    [Fact]
    public void RuntimeRegistryInvokesAbilityPriorityAndAllocation()
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
        var priorities = registry.Execute(Request(WerewolfReferenceRuntime.SelectAbilityPrioritiesOperation, new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["draftId"] = race.Outputs["draftId"],
            ["draftVersion"] = race.Outputs["draftVersion"],
            ["expectedDraftVersion"] = race.Outputs["draftVersion"],
            ["currentRace"] = race.Outputs["raceId"],
            ["primaryCategoryId"] = WerewolfAbilityCategoryIdentifiers.Talents,
            ["secondaryCategoryId"] = WerewolfAbilityCategoryIdentifiers.Skills,
            ["tertiaryCategoryId"] = WerewolfAbilityCategoryIdentifiers.Knowledges
        }));

        Assert.Equal(WerewolfRaceIdentifiers.Homid, priorities.Outputs["raceId"]);
        Assert.False(priorities.Outputs.ContainsKey("race"));

        var allocated = registry.Execute(Request(WerewolfReferenceRuntime.AllocateAbilitiesOperation, new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["draftId"] = priorities.Outputs["draftId"],
            ["draftVersion"] = priorities.Outputs["draftVersion"],
            ["expectedDraftVersion"] = priorities.Outputs["draftVersion"],
            ["currentRace"] = priorities.Outputs["raceId"],
            ["abilityPriorityOrder"] = priorities.Outputs["abilityPriorityOrder"],
            ["abilityBudgets"] = priorities.Outputs["abilityBudgets"],
            ["abilities"] = Format(ValidAllocation(WerewolfAbilityCategoryIdentifiers.Talents, WerewolfAbilityCategoryIdentifiers.Skills, WerewolfAbilityCategoryIdentifiers.Knowledges))
        }));

        Assert.True(allocated.Succeeded, Format(allocated.Findings));
        Assert.Equal("4", allocated.Outputs["draftVersion"]);
        Assert.Contains("talents:13/13/0", allocated.Outputs["abilityCategoryTotals"], StringComparison.Ordinal);
        Assert.Contains("character.ability.alertness:3", allocated.Outputs["abilities"], StringComparison.Ordinal);
        Assert.DoesNotContain(WerewolfAbilitySelectionService.AllocateAbilitiesStep, allocated.Outputs["nextSteps"], StringComparison.Ordinal);
    }

    [Fact]
    public void AbilitySelectionHasNoForbiddenDependencies()
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "rule-sets", "Chronicle.RuleSets.Werewolf", "CharacterCreation", "WerewolfAbilitySelection.cs"));
        var forbidden = new[] { "Chronicle.Persistence", "Chronicle.Presentation", "OpenAI", "HttpClient", "DbContext", "File.", "Directory.", "Random", "Campaign" };

        Assert.DoesNotContain(forbidden, token => source.Contains(token, StringComparison.Ordinal));
    }

    private static void AssertPriorityCode(WerewolfInitializedCharacterState draft, string primary, string secondary, string tertiary, WerewolfAbilityPrioritySelectionErrorCode code)
    {
        var result = WerewolfAbilitySelectionService.SelectPriorities(new WerewolfAbilityPrioritySelectionRequest(draft, draft.DraftVersion, primary, secondary, tertiary));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == code);
    }

    private static void AssertCode(WerewolfInitializedCharacterState draft, IReadOnlyList<WerewolfAbilityDotAllocation> allocations, WerewolfAbilityAllocationErrorCode code)
    {
        var result = Allocate(draft, allocations);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, finding => finding.Code == code);
    }

    private static WerewolfAbilityAllocationResult Allocate(WerewolfInitializedCharacterState draft, IReadOnlyList<WerewolfAbilityDotAllocation> allocations)
    {
        return WerewolfAbilitySelectionService.AllocateAbilities(new WerewolfAbilityAllocationRequest(draft, draft.DraftVersion, allocations));
    }

    private static WerewolfInitializedCharacterState PrioritizedDraft(string primary, string secondary, string tertiary, string race)
    {
        return WerewolfAbilitySelectionService.SelectPriorities(new WerewolfAbilityPrioritySelectionRequest(Draft() with { Race = race }, 1, primary, secondary, tertiary)).Draft!;
    }

    private static WerewolfInitializedCharacterState Draft()
    {
        return WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity("draft-001"), 1);
    }

    private static WerewolfAbilityDotAllocation[] ValidAllocation(string primary, string secondary, string tertiary)
    {
        var values = new Dictionary<string, int>(StringComparer.Ordinal);
        AddCategory(values, primary, BudgetRatings(primary, 13));
        AddCategory(values, secondary, BudgetRatings(secondary, 9));
        AddCategory(values, tertiary, BudgetRatings(tertiary, 5));

        return values
            .OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .Select(entry => new WerewolfAbilityDotAllocation(entry.Key, entry.Value))
            .ToArray();
    }

    private static WerewolfAbilityDotAllocation[] ValidLupusAllocation()
    {
        return
        [
            new WerewolfAbilityDotAllocation(WerewolfAbilityIdentifiers.Alertness, 3),
            new WerewolfAbilityDotAllocation(WerewolfAbilityIdentifiers.Athletics, 2),
            new WerewolfAbilityDotAllocation(WerewolfAbilityIdentifiers.Brawl, 2),
            new WerewolfAbilityDotAllocation(WerewolfAbilityIdentifiers.Empathy, 2),
            new WerewolfAbilityDotAllocation(WerewolfAbilityIdentifiers.Expression, 2),
            new WerewolfAbilityDotAllocation(WerewolfAbilityIdentifiers.Intimidation, 1),
            new WerewolfAbilityDotAllocation(WerewolfAbilityIdentifiers.Subterfuge, 1),
            new WerewolfAbilityDotAllocation(WerewolfAbilityIdentifiers.Drive, 0),
            new WerewolfAbilityDotAllocation(WerewolfAbilityIdentifiers.Etiquette, 0),
            new WerewolfAbilityDotAllocation(WerewolfAbilityIdentifiers.Leadership, 3),
            new WerewolfAbilityDotAllocation(WerewolfAbilityIdentifiers.Performance, 3),
            new WerewolfAbilityDotAllocation(WerewolfAbilityIdentifiers.Stealth, 3),
            new WerewolfAbilityDotAllocation(WerewolfAbilityIdentifiers.Survival, 0),
            new WerewolfAbilityDotAllocation(WerewolfAbilityIdentifiers.Computer, 0),
            new WerewolfAbilityDotAllocation(WerewolfAbilityIdentifiers.Investigation, 3),
            new WerewolfAbilityDotAllocation(WerewolfAbilityIdentifiers.Law, 0),
            new WerewolfAbilityDotAllocation(WerewolfAbilityIdentifiers.Occult, 2),
            new WerewolfAbilityDotAllocation(WerewolfAbilityIdentifiers.Politics, 0)
        ];
    }

    private static int[] BudgetRatings(string category, int budget)
    {
        return (category, budget) switch
        {
            (WerewolfAbilityCategoryIdentifiers.Talents, 13) => [3, 2, 2, 2, 2, 1, 1],
            (WerewolfAbilityCategoryIdentifiers.Talents, 9) => [2, 2, 1, 1, 1, 1, 1],
            (WerewolfAbilityCategoryIdentifiers.Talents, 5) => [1, 1, 1, 1, 1, 0, 0],
            (WerewolfAbilityCategoryIdentifiers.Skills, 13) => [3, 3, 2, 2, 2, 1],
            (WerewolfAbilityCategoryIdentifiers.Skills, 9) => [2, 2, 2, 1, 1, 1],
            (WerewolfAbilityCategoryIdentifiers.Skills, 5) => [1, 1, 1, 1, 1, 0],
            (WerewolfAbilityCategoryIdentifiers.Knowledges, 13) => [3, 3, 3, 2, 2],
            (WerewolfAbilityCategoryIdentifiers.Knowledges, 9) => [2, 2, 2, 2, 1],
            (WerewolfAbilityCategoryIdentifiers.Knowledges, 5) => [1, 1, 1, 1, 1],
            _ => []
        };
    }

    private static void AddCategory(Dictionary<string, int> values, string category, int[] ratings)
    {
        var abilities = AbilitiesInCategory(category);
        for (var index = 0; index < abilities.Length; index += 1)
        {
            values[abilities[index]] = ratings[index];
        }
    }

    private static string[] AbilitiesInCategory(string category)
    {
        return category switch
        {
            WerewolfAbilityCategoryIdentifiers.Talents =>
            [
                WerewolfAbilityIdentifiers.Alertness,
                WerewolfAbilityIdentifiers.Athletics,
                WerewolfAbilityIdentifiers.Brawl,
                WerewolfAbilityIdentifiers.Empathy,
                WerewolfAbilityIdentifiers.Expression,
                WerewolfAbilityIdentifiers.Intimidation,
                WerewolfAbilityIdentifiers.Subterfuge
            ],
            WerewolfAbilityCategoryIdentifiers.Skills =>
            [
                WerewolfAbilityIdentifiers.Drive,
                WerewolfAbilityIdentifiers.Etiquette,
                WerewolfAbilityIdentifiers.Leadership,
                WerewolfAbilityIdentifiers.Performance,
                WerewolfAbilityIdentifiers.Stealth,
                WerewolfAbilityIdentifiers.Survival
            ],
            WerewolfAbilityCategoryIdentifiers.Knowledges =>
            [
                WerewolfAbilityIdentifiers.Computer,
                WerewolfAbilityIdentifiers.Investigation,
                WerewolfAbilityIdentifiers.Law,
                WerewolfAbilityIdentifiers.Occult,
                WerewolfAbilityIdentifiers.Politics
            ],
            _ => []
        };
    }

    private static string Format(IReadOnlyList<WerewolfAbilityDotAllocation> allocations)
    {
        return string.Join(",", allocations.Select(allocation => $"{allocation.AbilityId}:{allocation.Rating.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));
    }

    private static string Format(IEnumerable<RuleSetRuntimeFinding> findings)
    {
        return string.Join(Environment.NewLine, findings.Select(finding => $"{finding.Severity}|{finding.Code}|{finding.Message}"));
    }

    private static string Format(IEnumerable<WerewolfAbilityAllocationFinding> findings)
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
