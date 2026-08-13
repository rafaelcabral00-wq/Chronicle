using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfActionTestDefinitionTests
{
    private static WerewolfInitializedCharacterState BuildCompletedDraft(string race, string auspice, string tribe)
    {
        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(
            new WerewolfCharacterDraftIdentity("draft-1"), 1);

        return draft with
        {
            Status = WerewolfCharacterDraftStatus.Completed,
            Race = race,
            Auspice = auspice,
            Tribe = tribe,
            MetisDeformity = null,
            RaceGift = WerewolfInitialGiftIdentifiers.HomidMasterOfFire,
            AuspiceGift = WerewolfInitialGiftIdentifiers.RagabashOpenSeal,
            TribeGift = WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine,
            Attributes = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfAttributeIdentifiers.Strength] = 3,
                [WerewolfAttributeIdentifiers.Dexterity] = 2,
                [WerewolfAttributeIdentifiers.Stamina] = 3,
                [WerewolfAttributeIdentifiers.Charisma] = 2,
                [WerewolfAttributeIdentifiers.Manipulation] = 2,
                [WerewolfAttributeIdentifiers.Appearance] = 2,
                [WerewolfAttributeIdentifiers.Perception] = 3,
                [WerewolfAttributeIdentifiers.Intelligence] = 2,
                [WerewolfAttributeIdentifiers.Wits] = 3
            },
            Abilities = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfAbilityIdentifiers.Athletics] = 3,
                [WerewolfAbilityIdentifiers.Brawl] = 2
            },
            Backgrounds = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfBackgroundIdentifiers.Allies] = 1,
                [WerewolfBackgroundIdentifiers.Contacts] = 1,
                [WerewolfBackgroundIdentifiers.Resources] = 1,
                [WerewolfBackgroundIdentifiers.Rites] = 1,
                [WerewolfBackgroundIdentifiers.Mentor] = 1
            },
            Resources = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                ["gnosis"] = 1,
                ["rage"] = 1,
                ["willpower"] = 3
            },
            Rank = "cliath",
            RankValue = 1,
            IdentityName = "Test Character",
            RequiredNextSteps = []
        };
    }

    [Fact]
    public void DefineTestSucceedsForValidAttributeAndAbility()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash, WerewolfTribeIdentifiers.GlassWalkers);
        var request = new WerewolfActionTestDefinitionRequest(draft, draft.DraftVersion, "req-1", WerewolfAttributeIdentifiers.Strength, WerewolfAbilityIdentifiers.Athletics, 6, 0);

        var result = WerewolfActionTestDefinitionService.DefineTest(request);

        Assert.True(result.Succeeded);
        Assert.Equal(6, result.DiceQuantity); // 3 + 3 = 6
        Assert.Equal(10, result.DiceFaces);
        Assert.Equal("req-1", result.RequestId);
        Assert.Equal(WerewolfAttributeIdentifiers.Strength, result.AttributeId);
        Assert.Equal(WerewolfAbilityIdentifiers.Athletics, result.AbilityId);
        Assert.Equal(6, result.Difficulty);
        Assert.Equal(0, result.Modifier);
    }

    [Fact]
    public void DefineTestRejectsNonCompletedCharacter()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash, WerewolfTribeIdentifiers.GlassWalkers) with { Status = WerewolfCharacterDraftStatus.Initialized };
        var request = new WerewolfActionTestDefinitionRequest(draft, draft.DraftVersion, "req-1", WerewolfAttributeIdentifiers.Strength, WerewolfAbilityIdentifiers.Athletics, 6, 0);

        var result = WerewolfActionTestDefinitionService.DefineTest(request);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == "CharacterNotCompleted");
    }

    [Fact]
    public void DefineTestRejectsInvalidAttribute()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash, WerewolfTribeIdentifiers.GlassWalkers);
        var request = new WerewolfActionTestDefinitionRequest(draft, draft.DraftVersion, "req-1", "invalid-attribute", WerewolfAbilityIdentifiers.Athletics, 6, 0);

        var result = WerewolfActionTestDefinitionService.DefineTest(request);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == "InvalidAttribute");
    }

    [Fact]
    public void DefineTestRejectsInvalidAbility()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash, WerewolfTribeIdentifiers.GlassWalkers);
        var request = new WerewolfActionTestDefinitionRequest(draft, draft.DraftVersion, "req-1", WerewolfAttributeIdentifiers.Strength, "invalid-ability", 6, 0);

        var result = WerewolfActionTestDefinitionService.DefineTest(request);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == "InvalidAbility");
    }

    [Fact]
    public void DefineTestRejectsUnallocatedAttribute()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash, WerewolfTribeIdentifiers.GlassWalkers) with
        {
            Attributes = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfAttributeIdentifiers.Strength] = null
            }
        };
        var request = new WerewolfActionTestDefinitionRequest(draft, draft.DraftVersion, "req-1", WerewolfAttributeIdentifiers.Strength, WerewolfAbilityIdentifiers.Athletics, 6, 0);

        var result = WerewolfActionTestDefinitionService.DefineTest(request);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == "AttributeNotAllocated");
    }

    [Fact]
    public void DefineTestRejectsUnallocatedAbility()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash, WerewolfTribeIdentifiers.GlassWalkers) with
        {
            Abilities = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfAbilityIdentifiers.Athletics] = null
            }
        };
        var request = new WerewolfActionTestDefinitionRequest(draft, draft.DraftVersion, "req-1", WerewolfAttributeIdentifiers.Strength, WerewolfAbilityIdentifiers.Athletics, 6, 0);

        var result = WerewolfActionTestDefinitionService.DefineTest(request);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == "AbilityNotAllocated");
    }

    [Fact]
    public void DefineTestRejectsInvalidDifficulty()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash, WerewolfTribeIdentifiers.GlassWalkers);
        var request = new WerewolfActionTestDefinitionRequest(draft, draft.DraftVersion, "req-1", WerewolfAttributeIdentifiers.Strength, WerewolfAbilityIdentifiers.Athletics, 0, 0);

        var result = WerewolfActionTestDefinitionService.DefineTest(request);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == "InvalidDifficulty");
    }

    [Fact]
    public void DefineTestRejectsStaleDraftVersion()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash, WerewolfTribeIdentifiers.GlassWalkers);
        var request = new WerewolfActionTestDefinitionRequest(draft, draft.DraftVersion + 1, "req-1", WerewolfAttributeIdentifiers.Strength, WerewolfAbilityIdentifiers.Athletics, 6, 0);

        var result = WerewolfActionTestDefinitionService.DefineTest(request);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == "StaleDraftVersion");
    }

    [Fact]
    public void DefineTestAppliesModifier()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash, WerewolfTribeIdentifiers.GlassWalkers);
        var request = new WerewolfActionTestDefinitionRequest(draft, draft.DraftVersion, "req-1", WerewolfAttributeIdentifiers.Strength, WerewolfAbilityIdentifiers.Athletics, 6, 2);

        var result = WerewolfActionTestDefinitionService.DefineTest(request);

        Assert.True(result.Succeeded);
        Assert.Equal(8, result.DiceQuantity); // 3 + 3 + 2 = 8
    }

    [Fact]
    public void DefineTestDoesNotAllowNegativePool()
    {
        var draft = BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash, WerewolfTribeIdentifiers.GlassWalkers) with
        {
            Attributes = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfAttributeIdentifiers.Strength] = 0
            },
            Abilities = new Dictionary<string, int?>(StringComparer.Ordinal)
            {
                [WerewolfAbilityIdentifiers.Athletics] = 0
            }
        };
        var request = new WerewolfActionTestDefinitionRequest(draft, draft.DraftVersion, "req-1", WerewolfAttributeIdentifiers.Strength, WerewolfAbilityIdentifiers.Athletics, 6, -10);

        var result = WerewolfActionTestDefinitionService.DefineTest(request);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.DiceQuantity);
    }

    [Theory]
    [InlineData(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash, WerewolfTribeIdentifiers.GlassWalkers)]
    [InlineData(WerewolfRaceIdentifiers.Metis, WerewolfAuspiceIdentifiers.Philodox, WerewolfTribeIdentifiers.GlassWalkers)]
    [InlineData(WerewolfRaceIdentifiers.Lupus, WerewolfAuspiceIdentifiers.Ahroun, WerewolfTribeIdentifiers.GlassWalkers)]
    public void AllSupportedRacesCanDefineActionTest(string race, string auspice, string tribe)
    {
        var draft = BuildCompletedDraft(race, auspice, tribe);
        var request = new WerewolfActionTestDefinitionRequest(draft, draft.DraftVersion, "req-1", WerewolfAttributeIdentifiers.Strength, WerewolfAbilityIdentifiers.Athletics, 6, 0);

        var result = WerewolfActionTestDefinitionService.DefineTest(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.DiceQuantity);
        Assert.True(result.DiceQuantity >= 0);
    }
}