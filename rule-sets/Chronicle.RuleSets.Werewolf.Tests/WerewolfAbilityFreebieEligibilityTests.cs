using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfAbilityFreebieEligibilityTests
{
    [Theory]
    [MemberData(nameof(AllRestrictedAbilities))]
    public void LupusBaseAllocationRejectsRestrictedAbilities(string abilityId)
    {
        var request = new WerewolfAbilityFreebieEligibilityRequest(
            "req-1",
            WerewolfRaceIdentifiers.Lupus,
            abilityId,
            WerewolfAbilityCreationStage.BaseAbilityAllocation,
            0,
            1);

        var result = WerewolfAbilityFreebieEligibilityService.CheckEligibility(request);

        Assert.False(result.IsEligible);
        Assert.Contains(result.Findings, f => f.Code == "RestrictedAbility");
    }

    [Theory]
    [MemberData(nameof(AllRestrictedAbilities))]
    public void LupusFreebieSpendingAllowsRestrictedAbilities(string abilityId)
    {
        var request = new WerewolfAbilityFreebieEligibilityRequest(
            "req-1",
            WerewolfRaceIdentifiers.Lupus,
            abilityId,
            WerewolfAbilityCreationStage.FreebieSpending,
            0,
            1);

        var result = WerewolfAbilityFreebieEligibilityService.CheckEligibility(request);

        Assert.True(result.IsEligible);
        Assert.DoesNotContain(result.Findings, f => f.Code == "RestrictedAbility");
    }

    [Theory]
    [MemberData(nameof(AllRestrictedAbilities))]
    public void LupusPostCreationAllowsRestrictedAbilities(string abilityId)
    {
        var request = new WerewolfAbilityFreebieEligibilityRequest(
            "req-1",
            WerewolfRaceIdentifiers.Lupus,
            abilityId,
            WerewolfAbilityCreationStage.PostCreation,
            0,
            1);

        var result = WerewolfAbilityFreebieEligibilityService.CheckEligibility(request);

        Assert.True(result.IsEligible);
        Assert.DoesNotContain(result.Findings, f => f.Code == "RestrictedAbility");
    }

    [Theory]
    [MemberData(nameof(AllNonRestrictedAbilities))]
    public void LupusBaseAllocationAllowsNonRestrictedAbilities(string abilityId)
    {
        var request = new WerewolfAbilityFreebieEligibilityRequest(
            "req-1",
            WerewolfRaceIdentifiers.Lupus,
            abilityId,
            WerewolfAbilityCreationStage.BaseAbilityAllocation,
            0,
            1);

        var result = WerewolfAbilityFreebieEligibilityService.CheckEligibility(request);

        Assert.True(result.IsEligible);
    }

    [Theory]
    [MemberData(nameof(AllNonRestrictedAbilities))]
    public void HomidBaseAllocationAllowsRestrictedAbilities(string abilityId)
    {
        var request = new WerewolfAbilityFreebieEligibilityRequest(
            "req-1",
            WerewolfRaceIdentifiers.Homid,
            abilityId,
            WerewolfAbilityCreationStage.BaseAbilityAllocation,
            0,
            1);

        var result = WerewolfAbilityFreebieEligibilityService.CheckEligibility(request);

        Assert.True(result.IsEligible);
    }

    [Fact]
    public void RejectsUnknownAbility()
    {
        var request = new WerewolfAbilityFreebieEligibilityRequest(
            "req-1",
            WerewolfRaceIdentifiers.Lupus,
            "character.ability.unknown",
            WerewolfAbilityCreationStage.FreebieSpending,
            0,
            1);

        var result = WerewolfAbilityFreebieEligibilityService.CheckEligibility(request);

        Assert.False(result.IsEligible);
        Assert.Contains(result.Findings, f => f.Code == "UnknownAbility");
    }

    [Fact]
    public void RejectsInvalidRatingIncrease()
    {
        var request = new WerewolfAbilityFreebieEligibilityRequest(
            "req-1",
            WerewolfRaceIdentifiers.Lupus,
            WerewolfAbilityIdentifiers.Computer,
            WerewolfAbilityCreationStage.FreebieSpending,
            0,
            0);

        var result = WerewolfAbilityFreebieEligibilityService.CheckEligibility(request);

        Assert.False(result.IsEligible);
        Assert.Contains(result.Findings, f => f.Code == "InvalidRatingIncrease");
    }

    [Fact]
    public void RejectsNegativeCurrentRating()
    {
        var request = new WerewolfAbilityFreebieEligibilityRequest(
            "req-1",
            WerewolfRaceIdentifiers.Lupus,
            WerewolfAbilityIdentifiers.Computer,
            WerewolfAbilityCreationStage.FreebieSpending,
            -1,
            1);

        var result = WerewolfAbilityFreebieEligibilityService.CheckEligibility(request);

        Assert.False(result.IsEligible);
        Assert.Contains(result.Findings, f => f.Code == "InvalidCurrentRating");
    }

    [Fact]
    public void BaseAndFreebieStagesCannotBeConfused()
    {
        var baseRequest = new WerewolfAbilityFreebieEligibilityRequest(
            "req-base",
            WerewolfRaceIdentifiers.Lupus,
            WerewolfAbilityIdentifiers.Crafts,
            WerewolfAbilityCreationStage.BaseAbilityAllocation,
            0,
            1);

        var freebieRequest = new WerewolfAbilityFreebieEligibilityRequest(
            "req-freebie",
            WerewolfRaceIdentifiers.Lupus,
            WerewolfAbilityIdentifiers.Crafts,
            WerewolfAbilityCreationStage.FreebieSpending,
            0,
            1);

        var baseResult = WerewolfAbilityFreebieEligibilityService.CheckEligibility(baseRequest);
        var freebieResult = WerewolfAbilityFreebieEligibilityService.CheckEligibility(freebieRequest);

        Assert.False(baseResult.IsEligible);
        Assert.True(freebieResult.IsEligible);
        Assert.NotEqual(baseResult.Findings[0].Code, freebieResult.Findings[0].Code);
    }

    [Fact]
    public void LupusCompletionPermitsNonzeroRestrictedAbilityFromFreebies()
    {
        var draft = BuildLupusDraftWithRestrictedAbility();
        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion));

        if (!result.Succeeded)
        {
            var messages = string.Join(", ", result.Findings.Select(f => $"{f.Code}: {f.Message}"));
            Assert.True(result.Succeeded, $"Completion failed: {messages}");
        }

        Assert.NotNull(result.Snapshot);
        Assert.Equal(1, result.Snapshot!.Abilities[WerewolfAbilityIdentifiers.Crafts]);
    }

    private static WerewolfInitializedCharacterState BuildLupusDraftWithRestrictedAbility()
    {
        var attributeKeys = WerewolfAttributeIdentifiers.Supported;
        var attributes = new Dictionary<string, int?>(StringComparer.Ordinal);
        foreach (var key in attributeKeys)
        {
            attributes[key] = 1;
        }

        var abilityKeys = WerewolfCharacterCreationDraftFactory.GetAbilityKeys();
        var abilities = new Dictionary<string, int?>(StringComparer.Ordinal);
        foreach (var key in abilityKeys)
        {
            abilities[key] = 0;
        }
        abilities[WerewolfAbilityIdentifiers.Crafts] = 1;

        var backgroundKeys = WerewolfCharacterCreationDraftFactory.GetBackgroundKeys();
        var backgrounds = new Dictionary<string, int?>(StringComparer.Ordinal);
        foreach (var key in backgroundKeys)
        {
            backgrounds[key] = 1;
        }

        var resourceKeys = WerewolfCharacterCreationDraftFactory.GetResourceKeys();
        var resources = new Dictionary<string, int?>(StringComparer.Ordinal);
        foreach (var key in resourceKeys)
        {
            resources[key] = 1;
        }

        var renownKeys = WerewolfCharacterCreationDraftFactory.GetRenownKeys();
        var renown = new Dictionary<string, int?>(StringComparer.Ordinal);
        foreach (var key in renownKeys)
        {
            renown[key] = 1;
        }

        return new WerewolfInitializedCharacterState(
            new WerewolfCharacterDraftIdentity("draft-lupus-001"),
            WerewolfCharacterDraftStatus.Initialized,
            1,
            WerewolfRaceIdentifiers.Lupus,
            WerewolfAuspiceIdentifiers.Ragabash,
            WerewolfTribeIdentifiers.BoneGnawers,
            null,
            WerewolfInitialGiftIdentifiers.LupusHareLeap,
            WerewolfInitialGiftIdentifiers.RagabashOpenSeal,
            WerewolfInitialGiftIdentifiers.BoneGnawersCooking,
            [WerewolfAttributeCategoryIdentifiers.Physical, WerewolfAttributeCategoryIdentifiers.Social, WerewolfAttributeCategoryIdentifiers.Mental],
            new Dictionary<string, int>(StringComparer.Ordinal),
            [WerewolfAbilityCategoryIdentifiers.Talents, WerewolfAbilityCategoryIdentifiers.Skills, WerewolfAbilityCategoryIdentifiers.Knowledges],
            new Dictionary<string, int>(StringComparer.Ordinal),
            attributes,
            abilities,
            backgrounds,
            [],
            resources,
            renown,
            "cliath",
            1,
            "Test Lupus",
            new Dictionary<string, string?>(StringComparer.Ordinal),
            [],
            new Dictionary<string, string>(StringComparer.Ordinal),
            Array.AsReadOnly<WerewolfFreebieLedgerEntry>([]),
            15,
            0);
    }

    public static TheoryData<string> AllRestrictedAbilities => new()
    {
        WerewolfAbilityIdentifiers.Computer,
        WerewolfAbilityIdentifiers.Crafts,
        WerewolfAbilityIdentifiers.Drive,
        WerewolfAbilityIdentifiers.Etiquette,
        WerewolfAbilityIdentifiers.Firearms,
        WerewolfAbilityIdentifiers.Law,
        WerewolfAbilityIdentifiers.Linguistics,
        WerewolfAbilityIdentifiers.Politics,
        WerewolfAbilityIdentifiers.Science
    };

    public static TheoryData<string> AllNonRestrictedAbilities => new()
    {
        WerewolfAbilityIdentifiers.Alertness,
        WerewolfAbilityIdentifiers.Athletics,
        WerewolfAbilityIdentifiers.Brawl,
        WerewolfAbilityIdentifiers.Dodge,
        WerewolfAbilityIdentifiers.Empathy,
        WerewolfAbilityIdentifiers.Expression,
        WerewolfAbilityIdentifiers.Intimidation,
        WerewolfAbilityIdentifiers.PrimalInstinct,
        WerewolfAbilityIdentifiers.Streetwise,
        WerewolfAbilityIdentifiers.Subterfuge,
        WerewolfAbilityIdentifiers.AnimalEmpathy,
        WerewolfAbilityIdentifiers.Leadership,
        WerewolfAbilityIdentifiers.Melee,
        WerewolfAbilityIdentifiers.Performance,
        WerewolfAbilityIdentifiers.Stealth,
        WerewolfAbilityIdentifiers.Survival,
        WerewolfAbilityIdentifiers.Enigmas,
        WerewolfAbilityIdentifiers.Investigation,
        WerewolfAbilityIdentifiers.Medicine,
        WerewolfAbilityIdentifiers.Occult,
        WerewolfAbilityIdentifiers.Rituals
    };
}
