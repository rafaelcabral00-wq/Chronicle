using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfActionResolutionIntegrationTests
{
    private static WerewolfRuntimeCharacterState CreateState(string? metisDeformity = null, int version = 1)
    {
        var packageBinding = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["attributes"] = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, int>
            {
                [WerewolfAttributeIdentifiers.Strength] = 3,
                [WerewolfAttributeIdentifiers.Dexterity] = 3,
                [WerewolfAttributeIdentifiers.Stamina] = 3,
                [WerewolfAttributeIdentifiers.Charisma] = 2,
                [WerewolfAttributeIdentifiers.Manipulation] = 2,
                [WerewolfAttributeIdentifiers.Appearance] = 2,
                [WerewolfAttributeIdentifiers.Perception] = 3,
                [WerewolfAttributeIdentifiers.Intelligence] = 2,
                [WerewolfAttributeIdentifiers.Wits] = 3
            }),
            ["abilities"] = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, int>
            {
                [WerewolfAbilityIdentifiers.Alertness] = 2,
                [WerewolfAbilityIdentifiers.Athletics] = 2,
                [WerewolfAbilityIdentifiers.Brawl] = 2,
                [WerewolfAbilityIdentifiers.Expression] = 1,
                [WerewolfAbilityIdentifiers.Survival] = 1,
                [WerewolfAbilityIdentifiers.PrimalInstinct] = 1,
                [WerewolfAbilityIdentifiers.Intimidation] = 1,
                [WerewolfAbilityIdentifiers.Leadership] = 1
            })
        };

        if (metisDeformity is not null)
        {
            packageBinding["metis-deformity"] = metisDeformity;
        }

        return new WerewolfRuntimeCharacterState(
            "test-package",
            "0.1.0",
            "draft-001",
            version,
            packageBinding,
            5,
            5,
            3,
            3,
            7,
            7,
            0,
            0,
            0,
            0,
            0,
            0,
            WerewolfRaceIdentifiers.Metis,
            null,
            WerewolfFormIdentifiers.Crinos,
            []);
    }

    [Fact]
    public void OrdinaryActionWithNoDeformityReturnsBasePool()
    {
        var state = CreateState();
        var request = new WerewolfActionResolutionRequest(
            "req-1",
            state,
            1,
            WerewolfAttributeIdentifiers.Dexterity,
            WerewolfAbilityIdentifiers.Brawl,
            6);

        var result = WerewolfActionResolutionService.ResolveActionTest(request);

        Assert.Equal(5, result.BasePool);
        Assert.Equal(0, result.DicePoolModifier);
        Assert.Equal(5, result.FinalPool);
        Assert.Equal(6, result.BaseDifficulty);
        Assert.Equal(0, result.DifficultyModifier);
        Assert.Equal(6, result.FinalDifficulty);
        Assert.False(result.IsAutomaticFailure);
    }

    [Fact]
    public void ActionWithDicePoolModifier()
    {
        var state = CreateState(WerewolfMetisDeformityIdentifiers.ToughHide);
        var request = new WerewolfActionResolutionRequest(
            "req-1",
            state,
            1,
            WerewolfAttributeIdentifiers.Stamina,
            WerewolfAbilityIdentifiers.Survival,
            6);

        var result = WerewolfActionResolutionService.ResolveActionTest(request);

        Assert.Equal(4, result.BasePool);
        Assert.Equal(1, result.DicePoolModifier);
        Assert.Equal(5, result.FinalPool);
    }

    [Fact]
    public void ActionWithDifficultyModifier()
    {
        var state = CreateState(WerewolfMetisDeformityIdentifiers.DebilitatingDisease);
        var request = new WerewolfActionResolutionRequest(
            "req-1",
            state,
            1,
            WerewolfAttributeIdentifiers.Stamina,
            WerewolfAbilityIdentifiers.Survival,
            6);

        var result = WerewolfActionResolutionService.ResolveActionTest(request);

        Assert.Equal(6, result.BaseDifficulty);
        Assert.Equal(2, result.DifficultyModifier);
        Assert.Equal(8, result.FinalDifficulty);
    }

    [Fact]
    public void ActionMadeUnavailableByCondition()
    {
        var state = CreateState();
        var applyRequest = new WerewolfApplyConditionRequest(
            "req-0",
            state,
            1,
            WerewolfConditionIdentifiers.Incapacitated,
            WerewolfConditionKind.Incapacitated,
            "Line 531",
            WerewolfMetisDeformityIdentifiers.Seizures);

        var applied = WerewolfConditionService.ApplyCondition(applyRequest);

        var availabilityRequest = new WerewolfEvaluateActionAvailabilityRequest(
            "req-1",
            applied.NewState!,
            applied.NewRuntimeStateVersion,
            "any-action");

        var result = WerewolfConditionService.EvaluateActionAvailability(availabilityRequest);

        Assert.False(result.IsAvailable);
    }

    [Fact]
    public void SensoryActionUnderApplicableDeformity()
    {
        var state = CreateState(WerewolfMetisDeformityIdentifiers.NoSenseOfSmell);
        var request = new WerewolfActionResolutionRequest(
            "req-1",
            state,
            1,
            WerewolfAttributeIdentifiers.Perception,
            WerewolfAbilityIdentifiers.Alertness,
            6,
            SenseBeingTested: "olfactory");

        var result = WerewolfActionResolutionService.ResolveActionTest(request);

        Assert.True(result.IsAutomaticFailure);
    }

    [Fact]
    public void TaillessInCrinosAddsBalanceDifficulty()
    {
        var state = CreateState(WerewolfMetisDeformityIdentifiers.Tailless);
        var request = new WerewolfActionResolutionRequest(
            "req-1",
            state,
            1,
            WerewolfAttributeIdentifiers.Dexterity,
            WerewolfAbilityIdentifiers.Athletics,
            6,
            IsBalanceTest: true);

        var result = WerewolfActionResolutionService.ResolveActionTest(request);

        Assert.Equal(1, result.DifficultyModifier);
        Assert.Equal(7, result.FinalDifficulty);
    }

    [Fact]
    public void TaillessInHomidDoesNotAddBalanceDifficulty()
    {
        var state = CreateState(WerewolfMetisDeformityIdentifiers.Tailless);
        state = state with { CurrentForm = WerewolfFormIdentifiers.Homid };
        var request = new WerewolfActionResolutionRequest(
            "req-1",
            state,
            1,
            WerewolfAttributeIdentifiers.Dexterity,
            WerewolfAbilityIdentifiers.Athletics,
            6,
            IsBalanceTest: true);

        var result = WerewolfActionResolutionService.ResolveActionTest(request);

        Assert.Equal(0, result.DifficultyModifier);
    }

    [Fact]
    public void FitsOfMadnessTriggerUnderTension()
    {
        var state = CreateState(WerewolfMetisDeformityIdentifiers.FitsOfMadness);
        var request = new WerewolfActionResolutionRequest(
            "req-1",
            state,
            1,
            WerewolfAttributeIdentifiers.Wits,
            WerewolfAbilityIdentifiers.Survival,
            6,
            IsUnderTension: true);

        var result = WerewolfActionResolutionService.ResolveActionTest(request);

        Assert.Single(result.ConditionalTests);
        Assert.Equal("temporary-psychotic-episode", result.ConditionalTests[0].Consequence);
    }

    [Fact]
    public void FitsOfMadnessRecoveryClearsCondition()
    {
        var state = CreateState();
        var applyRequest = new WerewolfApplyConditionRequest(
            "req-1",
            state,
            1,
            WerewolfConditionIdentifiers.TemporaryPsychoticEpisode,
            WerewolfConditionKind.TemporaryPsychoticEpisode,
            "Line 527",
            WerewolfMetisDeformityIdentifiers.FitsOfMadness);

        var applied = WerewolfConditionService.ApplyCondition(applyRequest);

        var clearRequest = new WerewolfClearConditionRequest(
            "req-2",
            applied.NewState!,
            applied.NewRuntimeStateVersion,
            WerewolfConditionIdentifiers.TemporaryPsychoticEpisode);

        var cleared = WerewolfConditionService.ClearCondition(clearRequest);

        var availabilityRequest = new WerewolfEvaluateActionAvailabilityRequest(
            "req-3",
            cleared.NewState!,
            cleared.NewRuntimeStateVersion,
            "any-action");

        var result = WerewolfConditionService.EvaluateActionAvailability(availabilityRequest);

        Assert.True(result.IsAvailable);
    }

    [Fact]
    public void SeizuresOnCriticalFailure()
    {
        var state = CreateState(WerewolfMetisDeformityIdentifiers.Seizures);

        var applyRequest = new WerewolfApplyConditionRequest(
            "req-1",
            state,
            1,
            WerewolfConditionIdentifiers.CriticalFailure,
            WerewolfConditionKind.CriticalFailure,
            "Line 531",
            WerewolfMetisDeformityIdentifiers.Seizures);

        var applied = WerewolfConditionService.ApplyCondition(applyRequest);

        Assert.True(applied.Succeeded);
        Assert.Single(applied.NewState!.Conditions!);
    }

    [Fact]
    public void AlbinismInDaylightPerception()
    {
        var state = CreateState(WerewolfMetisDeformityIdentifiers.Albinism);
        var request = new WerewolfActionResolutionRequest(
            "req-1",
            state,
            1,
            WerewolfAttributeIdentifiers.Perception,
            WerewolfAbilityIdentifiers.Alertness,
            6,
            IsDaylightWithoutProtection: true);

        var result = WerewolfActionResolutionService.ResolveActionTest(request);

        Assert.Equal(2, result.DifficultyModifier);
        Assert.Equal(8, result.FinalDifficulty);
    }

    [Fact]
    public void AlbinismAtNightNoModifier()
    {
        var state = CreateState(WerewolfMetisDeformityIdentifiers.Albinism);
        var request = new WerewolfActionResolutionRequest(
            "req-1",
            state,
            1,
            WerewolfAttributeIdentifiers.Perception,
            WerewolfAbilityIdentifiers.Alertness,
            6,
            IsDaylightWithoutProtection: false);

        var result = WerewolfActionResolutionService.ResolveActionTest(request);

        Assert.Equal(0, result.DifficultyModifier);
    }

    [Fact]
    public void WitheredLimbWhenUsingLimb()
    {
        var state = CreateState(WerewolfMetisDeformityIdentifiers.WitheredLimb);
        var request = new WerewolfActionResolutionRequest(
            "req-1",
            state,
            1,
            WerewolfAttributeIdentifiers.Dexterity,
            WerewolfAbilityIdentifiers.Brawl,
            6,
            IsUsingWitheredLimb: true);

        var result = WerewolfActionResolutionService.ResolveActionTest(request);

        Assert.Equal(2, result.DifficultyModifier);
    }

    [Fact]
    public void WitheredLimbWhenNotUsingLimb()
    {
        var state = CreateState(WerewolfMetisDeformityIdentifiers.WitheredLimb);
        var request = new WerewolfActionResolutionRequest(
            "req-1",
            state,
            1,
            WerewolfAttributeIdentifiers.Dexterity,
            WerewolfAbilityIdentifiers.Brawl,
            6,
            IsUsingWitheredLimb: false);

        var result = WerewolfActionResolutionService.ResolveActionTest(request);

        Assert.Equal(0, result.DifficultyModifier);
    }

    [Fact]
    public void BlindVisionTestAutomaticFailure()
    {
        var state = CreateState(WerewolfMetisDeformityIdentifiers.Blind);
        var request = new WerewolfActionResolutionRequest(
            "req-1",
            state,
            1,
            WerewolfAttributeIdentifiers.Perception,
            WerewolfAbilityIdentifiers.Alertness,
            6,
            IsVisionBased: true);

        var result = WerewolfActionResolutionService.ResolveActionTest(request);

        Assert.True(result.IsAutomaticFailure);
    }

    [Fact]
    public void BlindNonVisionTestNoFailure()
    {
        var state = CreateState(WerewolfMetisDeformityIdentifiers.Blind);
        var request = new WerewolfActionResolutionRequest(
            "req-1",
            state,
            1,
            WerewolfAttributeIdentifiers.Perception,
            WerewolfAbilityIdentifiers.Alertness,
            6,
            SenseBeingTested: "auditory");

        var result = WerewolfActionResolutionService.ResolveActionTest(request);

        Assert.False(result.IsAutomaticFailure);
    }

    [Fact]
    public void NoSenseOfSmellTrackingPenalty()
    {
        var state = CreateState(WerewolfMetisDeformityIdentifiers.NoSenseOfSmell);
        var request = new WerewolfActionResolutionRequest(
            "req-1",
            state,
            1,
            WerewolfAttributeIdentifiers.Perception,
            WerewolfAbilityIdentifiers.PrimalInstinct,
            6,
            IsTracking: true);

        var result = WerewolfActionResolutionService.ResolveActionTest(request);

        Assert.Equal(2, result.DifficultyModifier);
    }

    [Fact]
    public void FinalDifficultyClampedToMaximum10()
    {
        var state = CreateState(WerewolfMetisDeformityIdentifiers.DebilitatingDisease);
        var request = new WerewolfActionResolutionRequest(
            "req-1",
            state,
            1,
            WerewolfAttributeIdentifiers.Stamina,
            WerewolfAbilityIdentifiers.Survival,
            9);

        var result = WerewolfActionResolutionService.ResolveActionTest(request);

        Assert.Equal(10, result.FinalDifficulty);
    }

    [Fact]
    public void FinalPoolNeverNegative()
    {
        var state = CreateState();
        state = state with
        {
            PackageBinding = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["attributes"] = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, int>
                {
                    [WerewolfAttributeIdentifiers.Strength] = 1
                }),
                ["abilities"] = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, int>
                {
                    [WerewolfAbilityIdentifiers.Brawl] = 0
                })
            }
        };

        var request = new WerewolfActionResolutionRequest(
            "req-1",
            state,
            1,
            WerewolfAttributeIdentifiers.Strength,
            WerewolfAbilityIdentifiers.Brawl,
            6);

        var result = WerewolfActionResolutionService.ResolveActionTest(request);

        Assert.True(result.FinalPool >= 0);
    }

    [Fact]
    public void VersionMismatchReturnsBlocked()
    {
        var state = CreateState(version: 5);
        var request = new WerewolfActionResolutionRequest(
            "req-1",
            state,
            1,
            WerewolfAttributeIdentifiers.Dexterity,
            WerewolfAbilityIdentifiers.Brawl,
            6);

        var result = WerewolfActionResolutionService.ResolveActionTest(request);

        Assert.True(result.IsActionUnavailable);
    }
}
