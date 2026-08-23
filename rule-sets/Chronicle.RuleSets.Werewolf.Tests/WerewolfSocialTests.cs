using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfSocialTests
{
    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null
    };

    private static WerewolfRuntimeCharacterState BuildRuntimeState(
        int ragePermanent = 5,
        int rageCurrent = 5,
        int willpowerPermanent = 5,
        int willpowerCurrent = 5,
        string currentForm = WerewolfFormIdentifiers.Homid,
        int charisma = 3,
        int manipulation = 3,
        int appearance = 3,
        int wits = 3,
        int intelligence = 3,
        int primalInstinct = 2,
        int subterfuge = 2,
        int leadership = 2,
        int performance = 2,
        int empathy = 2,
        int streetwise = 2,
        int intimidation = 2)
    {
        var attributes = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [WerewolfAttributeIdentifiers.Strength] = 3,
            [WerewolfAttributeIdentifiers.Dexterity] = 3,
            [WerewolfAttributeIdentifiers.Stamina] = 3,
            [WerewolfAttributeIdentifiers.Charisma] = charisma,
            [WerewolfAttributeIdentifiers.Manipulation] = manipulation,
            [WerewolfAttributeIdentifiers.Appearance] = appearance,
            [WerewolfAttributeIdentifiers.Perception] = 3,
            [WerewolfAttributeIdentifiers.Intelligence] = intelligence,
            [WerewolfAttributeIdentifiers.Wits] = wits
        };

        var abilities = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [WerewolfAbilityIdentifiers.PrimalInstinct] = primalInstinct,
            [WerewolfAbilityIdentifiers.Subterfuge] = subterfuge,
            [WerewolfAbilityIdentifiers.Leadership] = leadership,
            [WerewolfAbilityIdentifiers.Performance] = performance,
            [WerewolfAbilityIdentifiers.Empathy] = empathy,
            [WerewolfAbilityIdentifiers.Streetwise] = streetwise,
            [WerewolfAbilityIdentifiers.Intimidation] = intimidation
        };

        var packageBinding = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["packageId"] = WerewolfRuleSetPackage.ProvisionalPackageId,
            ["packageVersion"] = WerewolfRuleSetPackage.PackageVersion,
            ["declaredReleaseScope"] = WerewolfRuleSetPackage.DeclaredReleaseScope,
            ["contractVersion"] = "1",
            ["attributes"] = System.Text.Json.JsonSerializer.Serialize(attributes, JsonOptions),
            ["abilities"] = System.Text.Json.JsonSerializer.Serialize(abilities, JsonOptions)
        };

        return new WerewolfRuntimeCharacterState(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            "draft-1",
            1,
            packageBinding,
            ragePermanent,
            rageCurrent,
            3,
            3,
            willpowerPermanent,
            willpowerCurrent,
            0,
            0,
            0,
            0,
            0,
            0,
            WerewolfRaceIdentifiers.Homid,
            null,
            currentForm,
            Array.AsReadOnly(new List<WerewolfCondition>().ToArray()),
            null);
    }

    [Fact]
    public void SocialChallengeCatalogContainsAllSourceDefinedChallenges()
    {
        Assert.Contains(WerewolfSocialChallengeIdentifiers.AtracaoAnimal, WerewolfSocialChallengeCatalog.Entries.Keys);
        Assert.Contains(WerewolfSocialChallengeIdentifiers.Credibilidade, WerewolfSocialChallengeCatalog.Entries.Keys);
        Assert.Contains(WerewolfSocialChallengeIdentifiers.Defrontacao, WerewolfSocialChallengeCatalog.Entries.Keys);
        Assert.Contains(WerewolfSocialChallengeIdentifiers.Engabelacao, WerewolfSocialChallengeCatalog.Entries.Keys);
        Assert.Contains(WerewolfSocialChallengeIdentifiers.Interrogatorio, WerewolfSocialChallengeCatalog.Entries.Keys);
        Assert.Contains(WerewolfSocialChallengeIdentifiers.Intimidacao, WerewolfSocialChallengeCatalog.Entries.Keys);
        Assert.Contains(WerewolfSocialChallengeIdentifiers.OratoriaPerformance, WerewolfSocialChallengeCatalog.Entries.Keys);
        Assert.Contains(WerewolfSocialChallengeIdentifiers.Seducao, WerewolfSocialChallengeCatalog.Entries.Keys);
    }

    [Fact]
    public void DefrontacaoUsesFuryPool()
    {
        var challenge = WerewolfSocialChallengeCatalog.Entries[WerewolfSocialChallengeIdentifiers.Defrontacao];
        Assert.True(challenge.UsesFuryPool);
        Assert.Equal(WerewolfAttributeIdentifiers.Charisma, challenge.AttributeId);
        Assert.Equal(WerewolfAbilityIdentifiers.Intimidation, challenge.AbilityId);
    }

    [Fact]
    public void AtracaoAnimalUsesCharismaAndPrimalInstinct()
    {
        var challenge = WerewolfSocialChallengeCatalog.Entries[WerewolfSocialChallengeIdentifiers.AtracaoAnimal];
        Assert.Equal(WerewolfAttributeIdentifiers.Charisma, challenge.AttributeId);
        Assert.Equal(WerewolfAbilityIdentifiers.PrimalInstinct, challenge.AbilityId);
        Assert.False(challenge.UsesFuryPool);
    }

    [Fact]
    public void SocialTestDefinitionSucceedsForValidChallenge()
    {
        var state = BuildRuntimeState();
        var request = new WerewolfSocialTestDefinitionRequest(
            state,
            1,
            "req",
            WerewolfSocialChallengeIdentifiers.AtracaoAnimal,
            new WerewolfSocialTargetContext(TargetWillpower: 6));

        var result = WerewolfSocialTestDefinitionService.DefineTest(request);

        Assert.True(result.Succeeded);
        Assert.Equal(WerewolfSocialChallengeIdentifiers.AtracaoAnimal, result.ChallengeId);
        Assert.NotNull(result.FinalPool);
        Assert.NotNull(result.FinalDifficulty);
    }

    [Fact]
    public void SocialTestDefinitionRejectsInvalidChallengeId()
    {
        var state = BuildRuntimeState();
        var request = new WerewolfSocialTestDefinitionRequest(
            state,
            1,
            "req",
            "invalid-challenge",
            new WerewolfSocialTargetContext());

        var result = WerewolfSocialTestDefinitionService.DefineTest(request);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == "InvalidChallengeId");
    }

    [Fact]
    public void SocialTestDefinitionRejectsVersionMismatch()
    {
        var state = BuildRuntimeState();
        var request = new WerewolfSocialTestDefinitionRequest(
            state,
            2,
            "req",
            WerewolfSocialChallengeIdentifiers.AtracaoAnimal,
            new WerewolfSocialTargetContext());

        var result = WerewolfSocialTestDefinitionService.DefineTest(request);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void DefrontacaoUsesHigherOfCharismaIntimidationOrFury()
    {
        var state = BuildRuntimeState(charisma: 2, intimidation: 2, ragePermanent: 5);
        var request = new WerewolfSocialTestDefinitionRequest(
            state,
            1,
            "req",
            WerewolfSocialChallengeIdentifiers.Defrontacao,
            new WerewolfSocialTargetContext(TargetWillpower: 6));

        var result = WerewolfSocialTestDefinitionService.DefineTest(request);

        Assert.True(result.Succeeded);
        Assert.Equal(5, result.BasePool);
    }

    [Fact]
    public void SeducaoStageOneUsesAppearanceAndSubterfuge()
    {
        var challenge = WerewolfSocialChallengeCatalog.Entries[WerewolfSocialChallengeIdentifiers.Seducao];
        Assert.Equal(WerewolfAttributeIdentifiers.Appearance, challenge.AttributeId);
        Assert.Equal(WerewolfAbilityIdentifiers.Subterfuge, challenge.AbilityId);
    }

    [Fact]
    public void SocialTestDefinitionComputesPureBreedBonusForGarouTarget()
    {
        var state = BuildRuntimeState();
        state = state with { PackageBinding = new Dictionary<string, string>(state.PackageBinding) { ["backgrounds"] = "{\"pure-breed\": 3}" } };
        var request = new WerewolfSocialTestDefinitionRequest(
            state,
            1,
            "req",
            WerewolfSocialChallengeIdentifiers.Credibilidade,
            new WerewolfSocialTargetContext(IsGarouTarget: true));

        var result = WerewolfSocialTestDefinitionService.DefineTest(request);

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.FinalPool - result.BasePool);
    }

    [Fact]
    public void SocialTestDefinitionNoPureBreedBonusForNonGarouTarget()
    {
        var state = BuildRuntimeState();
        state = state with { PackageBinding = new Dictionary<string, string>(state.PackageBinding) { ["backgrounds"] = "{\"pure-breed\": 3}" } };
        var request = new WerewolfSocialTestDefinitionRequest(
            state,
            1,
            "req",
            WerewolfSocialChallengeIdentifiers.Credibilidade,
            new WerewolfSocialTargetContext(IsGarouTarget: false));

        var result = WerewolfSocialTestDefinitionService.DefineTest(request);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.FinalPool - result.BasePool);
    }

    [Fact]
    public void SocialTestDefinitionComputesBestaInteriorPenalty()
    {
        var state = BuildRuntimeState(ragePermanent: 5, willpowerPermanent: 3);
        var request = new WerewolfSocialTestDefinitionRequest(
            state,
            1,
            "req",
            WerewolfSocialChallengeIdentifiers.Credibilidade,
            new WerewolfSocialTargetContext());

        var result = WerewolfSocialTestDefinitionService.DefineTest(request);

        Assert.True(result.Succeeded);
        Assert.Equal(-2, result.FinalPool - result.BasePool - result.ExplicitModifier);
    }

    [Fact]
    public void IntimidacaoInCrinosCausesAutomaticDeliriumOnHuman()
    {
        var state = BuildRuntimeState(currentForm: WerewolfFormIdentifiers.Crinos);
        var request = new WerewolfSocialTestDefinitionRequest(
            state,
            1,
            "req",
            WerewolfSocialChallengeIdentifiers.Intimidacao,
            new WerewolfSocialTargetContext(IsHumanTarget: true));

        var result = WerewolfSocialTestDefinitionService.DefineTest(request);

        Assert.True(result.Succeeded);
        Assert.True(result.IsAutomaticFailure);
    }

    [Fact]
    public void SocialTestDefinitionRejectsMissingAttribute()
    {
        var state = BuildRuntimeState();
        var request = new WerewolfSocialTestDefinitionRequest(
            state,
            1,
            "req",
            WerewolfSocialChallengeIdentifiers.AtracaoAnimal,
            new WerewolfSocialTargetContext());

        var result = WerewolfSocialTestDefinitionService.DefineTest(request);

        Assert.True(result.Succeeded);
    }
}
