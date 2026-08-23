using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfFrenzyTests
{
    [Fact]
    public void FrenzyTestDefinitionNewMoonDifficultyIs8()
    {
        var result = WerewolfFrenzyTestDefinitionService.ComputeTestDefinition(
            "req", 5, 5, 1, null, null, "new", null);

        Assert.True(result.IsValid);
        Assert.Equal(8, result.BaseDifficulty);
    }

    [Fact]
    public void FrenzyTestDefinitionWaxingCrescentDifficultyIs7()
    {
        var result = WerewolfFrenzyTestDefinitionService.ComputeTestDefinition(
            "req", 5, 5, 1, null, null, "waxing-crescent", null);

        Assert.True(result.IsValid);
        Assert.Equal(7, result.BaseDifficulty);
    }

    [Fact]
    public void FrenzyTestDefinitionHalfMoonDifficultyIs6()
    {
        var result = WerewolfFrenzyTestDefinitionService.ComputeTestDefinition(
            "req", 5, 5, 1, null, null, "half", null);

        Assert.True(result.IsValid);
        Assert.Equal(6, result.BaseDifficulty);
    }

    [Fact]
    public void FrenzyTestDefinitionWaxingGibbousDifficultyIs5()
    {
        var result = WerewolfFrenzyTestDefinitionService.ComputeTestDefinition(
            "req", 5, 5, 1, null, null, "waxing-gibbous", null);

        Assert.True(result.IsValid);
        Assert.Equal(5, result.BaseDifficulty);
    }

    [Fact]
    public void FrenzyTestDefinitionFullMoonDifficultyIs4()
    {
        var result = WerewolfFrenzyTestDefinitionService.ComputeTestDefinition(
            "req", 5, 5, 1, null, null, "full", null);

        Assert.True(result.IsValid);
        Assert.Equal(4, result.BaseDifficulty);
    }

    [Fact]
    public void FrenzyTestDefinitionRank1Adds1Difficulty()
    {
        var result = WerewolfFrenzyTestDefinitionService.ComputeTestDefinition(
            "req", 5, 5, 1, null, null, "half", null);

        Assert.True(result.IsValid);
        Assert.Equal(1, result.DifficultyModifier);
    }

    [Fact]
    public void FrenzyTestDefinitionRank2Adds2Difficulty()
    {
        var result = WerewolfFrenzyTestDefinitionService.ComputeTestDefinition(
            "req", 5, 5, 2, null, null, "half", null);

        Assert.True(result.IsValid);
        Assert.Equal(2, result.DifficultyModifier);
    }

    [Fact]
    public void FrenzyTestDefinitionRank3SuccessThresholdIs5()
    {
        var result = WerewolfFrenzyTestDefinitionService.ComputeTestDefinition(
            "req", 5, 5, 3, null, null, "half", null);

        Assert.True(result.IsValid);
        Assert.Equal(5, result.SuccessThreshold);
    }

    [Fact]
    public void FrenzyTestDefinitionRank4SuccessThresholdIs6()
    {
        var result = WerewolfFrenzyTestDefinitionService.ComputeTestDefinition(
            "req", 5, 5, 4, null, null, "half", null);

        Assert.True(result.IsValid);
        Assert.Equal(6, result.SuccessThreshold);
    }

    [Fact]
    public void FrenzyTestDefinitionAuspiceMoonMatchReducesDifficultyBy1()
    {
        var withoutMatch = WerewolfFrenzyTestDefinitionService.ComputeTestDefinition(
            "req", 5, 5, 1, null, null, "full", null);
        var withMatch = WerewolfFrenzyTestDefinitionService.ComputeTestDefinition(
            "req", 5, 5, 1, null, "full", "full", null);

        Assert.True(withoutMatch.IsValid);
        Assert.True(withMatch.IsValid);
        Assert.Equal(withoutMatch.FinalDifficulty - 1, withMatch.FinalDifficulty);
    }

    [Fact]
    public void FrenzyTestDefinitionCrinosFormReducesDifficultyBy1()
    {
        var withoutCrinos = WerewolfFrenzyTestDefinitionService.ComputeTestDefinition(
            "req", 5, 5, 1, null, null, "half", null);
        var withCrinos = WerewolfFrenzyTestDefinitionService.ComputeTestDefinition(
            "req", 5, 5, 1, WerewolfFormIdentifiers.Crinos, null, "half", null);

        Assert.True(withoutCrinos.IsValid);
        Assert.True(withCrinos.IsValid);
        Assert.Equal(withoutCrinos.FinalDifficulty - 1, withCrinos.FinalDifficulty);
    }

    [Fact]
    public void FrenzyTestDefinitionDifficultyClampedToMinimum2()
    {
        var result = WerewolfFrenzyTestDefinitionService.ComputeTestDefinition(
            "req", 5, 5, 1, WerewolfFormIdentifiers.Crinos, "full", "full", "-5");

        Assert.True(result.IsValid);
        Assert.Equal(2, result.FinalDifficulty);
    }

    [Fact]
    public void FrenzyTestDefinitionDifficultyClampedToMaximum10()
    {
        var result = WerewolfFrenzyTestDefinitionService.ComputeTestDefinition(
            "req", 5, 5, 1, null, null, "new", "5");

        Assert.True(result.IsValid);
        Assert.Equal(10, result.FinalDifficulty);
    }

    [Fact]
    public void EnterFrenzyWildSucceedsAndUpdatesState()
    {
        var state = State(frenzyState: null);
        var result = WerewolfFrenzyResolutionService.EnterFrenzy(new WerewolfEnterFrenzyRequest(
            "req", state, 1, WerewolfFrenzyType.Wild, "trigger", 4));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.UpdatedState);
        Assert.Equal(2, result.UpdatedState!.RuntimeStateVersion);
        Assert.NotNull(result.UpdatedState.FrenzyState);
        Assert.True(result.UpdatedState.FrenzyState.IsInFrenzy);
        Assert.Equal(WerewolfFrenzyType.Wild, result.UpdatedState.FrenzyState.FrenzyType);
    }

    [Fact]
    public void EnterFrenzyFoxChangesToLupus()
    {
        var state = State(currentForm: WerewolfFormIdentifiers.Homid);
        var result = WerewolfFrenzyResolutionService.EnterFrenzy(new WerewolfEnterFrenzyRequest(
            "req", state, 1, WerewolfFrenzyType.Fox, "trigger", 4));

        Assert.True(result.Succeeded);
        Assert.Equal(WerewolfFrenzyType.Fox, result.UpdatedState!.FrenzyState!.FrenzyType);
    }

    [Fact]
    public void EnterFrenzyExtremeCannotBeSuppressed()
    {
        var state = State();
        var result = WerewolfFrenzyResolutionService.EnterFrenzy(new WerewolfEnterFrenzyRequest(
            "req", state, 1, WerewolfFrenzyType.Extreme, "trigger", 6));

        Assert.True(result.Succeeded);
        Assert.Equal(WerewolfFrenzyType.Extreme, result.UpdatedState!.FrenzyState!.FrenzyType);

        var suppressResult = WerewolfFrenzyResolutionService.SuppressFrenzy(result.UpdatedState, 2);
        Assert.False(suppressResult.Succeeded);
        Assert.Equal("ExtremeFrenzyUncontrollable", suppressResult.ErrorCode);
    }

    [Fact]
    public void EnterFrenzyRejectsWhenAlreadyInFrenzy()
    {
        var existingFrenzy = new WerewolfFrenzyState(true, WerewolfFrenzyType.Wild, "old", 4, 0, null, false, "Line 2916");
        var state = State(frenzyState: existingFrenzy);
        var result = WerewolfFrenzyResolutionService.EnterFrenzy(new WerewolfEnterFrenzyRequest(
            "req", state, 1, WerewolfFrenzyType.Wild, "trigger", 4));

        Assert.False(result.Succeeded);
        Assert.Equal("AlreadyInFrenzy", result.ErrorCode);
    }

    [Fact]
    public void SuppressFrenzySpendsWillpowerAndEndsFrenzy()
    {
        var frenzyState = new WerewolfFrenzyState(true, WerewolfFrenzyType.Wild, "trigger", 4, 0, null, false, "Line 2916");
        var state = State(frenzyState: frenzyState, willpowerCurrent: 5);
        var result = WerewolfFrenzyResolutionService.SuppressFrenzy(state, 1);

        Assert.True(result.Succeeded);
        Assert.Equal(4, result.UpdatedState!.WillpowerCurrent);
        Assert.False(result.UpdatedState.FrenzyState!.IsInFrenzy);
        Assert.True(result.UpdatedState.FrenzyState.IsSuppressed);
    }

    [Fact]
    public void SuppressFrenzyFailsWithoutWillpower()
    {
        var frenzyState = new WerewolfFrenzyState(true, WerewolfFrenzyType.Wild, "trigger", 4, 0, null, false, "Line 2916");
        var state = State(frenzyState: frenzyState, willpowerCurrent: 0);
        var result = WerewolfFrenzyResolutionService.SuppressFrenzy(state, 1);

        Assert.False(result.Succeeded);
        Assert.Equal("InsufficientWillpower", result.ErrorCode);
    }

    [Fact]
    public void EndFrenzySucceeds()
    {
        var frenzyState = new WerewolfFrenzyState(true, WerewolfFrenzyType.Wild, "trigger", 4, 0, null, false, "Line 2916");
        var state = State(frenzyState: frenzyState);
        var result = WerewolfFrenzyResolutionService.EndFrenzy(state, 1);

        Assert.True(result.Succeeded);
        Assert.False(result.UpdatedState!.FrenzyState!.IsInFrenzy);
        Assert.Equal(WerewolfFrenzyType.None, result.UpdatedState.FrenzyState!.FrenzyType);
    }

    [Fact]
    public void EndFrenzyFailsWhenNotInFrenzy()
    {
        var state = State(frenzyState: null);
        var result = WerewolfFrenzyResolutionService.EndFrenzy(state, 1);

        Assert.False(result.Succeeded);
        Assert.Equal("NotInFrenzy", result.ErrorCode);
    }

    [Fact]
    public void EvaluateFrenzyActionWildAllowsAttack()
    {
        var frenzyState = new WerewolfFrenzyState(true, WerewolfFrenzyType.Wild, "trigger", 4, 0, null, false, "Line 2916");
        var state = State(frenzyState: frenzyState);
        var availability = WerewolfFrenzyResolutionService.EvaluateFrenzyAction(state, "attack");

        Assert.Equal("available", availability);
    }

    [Fact]
    public void EvaluateFrenzyActionFoxAllowsOnlyFlee()
    {
        var frenzyState = new WerewolfFrenzyState(true, WerewolfFrenzyType.Fox, "trigger", 4, 0, null, false, "Line 2916");
        var state = State(frenzyState: frenzyState);
        var availability = WerewolfFrenzyResolutionService.EvaluateFrenzyAction(state, "flee");

        Assert.Equal("available", availability);
    }

    [Fact]
    public void EvaluateFrenzyActionFoxBlocksAttack()
    {
        var frenzyState = new WerewolfFrenzyState(true, WerewolfFrenzyType.Fox, "trigger", 4, 0, null, false, "Line 2916");
        var state = State(frenzyState: frenzyState);
        var availability = WerewolfFrenzyResolutionService.EvaluateFrenzyAction(state, "attack");

        Assert.Equal("unavailable-fox-frenzy", availability);
    }

    [Fact]
    public void EvaluateFrenzyActionExtremeAllowsActions()
    {
        var frenzyState = new WerewolfFrenzyState(true, WerewolfFrenzyType.Extreme, "trigger", 6, 0, null, false, "Line 2916");
        var state = State(frenzyState: frenzyState);
        var availability = WerewolfFrenzyResolutionService.EvaluateFrenzyAction(state, "attack");

        Assert.Equal("available", availability);
    }

    [Fact]
    public void EvaluateFrenzyActionExtremeAllowsFlee()
    {
        var frenzyState = new WerewolfFrenzyState(true, WerewolfFrenzyType.Extreme, "trigger", 6, 0, null, false, "Line 2916");
        var state = State(frenzyState: frenzyState);
        var availability = WerewolfFrenzyResolutionService.EvaluateFrenzyAction(state, "flee");

        Assert.Equal("available", availability);
    }

    [Fact]
    public void EvaluateFrenzyActionExtremeAllowsNonCombatAction()
    {
        var frenzyState = new WerewolfFrenzyState(true, WerewolfFrenzyType.Extreme, "trigger", 6, 0, null, false, "Line 2916");
        var state = State(frenzyState: frenzyState);
        var availability = WerewolfFrenzyResolutionService.EvaluateFrenzyAction(state, "social-interaction");

        Assert.Equal("available", availability);
    }

    [Fact]
    public void SuppressExtremeFrenzyRejectsAndDoesNotSpendWillpower()
    {
        var frenzyState = new WerewolfFrenzyState(true, WerewolfFrenzyType.Extreme, "trigger", 6, 0, null, false, "Line 2916");
        var state = State(frenzyState: frenzyState, willpowerCurrent: 5);
        var result = WerewolfFrenzyResolutionService.SuppressFrenzy(state, 1);

        Assert.False(result.Succeeded);
        Assert.Equal("ExtremeFrenzyUncontrollable", result.ErrorCode);
        Assert.Equal(5, state.WillpowerCurrent);
    }

    [Fact]
    public void SuppressExtremeFrenzyRejectsAndDoesNotIncrementVersion()
    {
        var frenzyState = new WerewolfFrenzyState(true, WerewolfFrenzyType.Extreme, "trigger", 6, 0, null, false, "Line 2916");
        var state = State(frenzyState: frenzyState);
        var result = WerewolfFrenzyResolutionService.SuppressFrenzy(state, 1);

        Assert.False(result.Succeeded);
        Assert.Equal(1, state.RuntimeStateVersion);
    }

    [Fact]
    public void BestaInteriorPenaltyAppliedWhenRageExceedsWillpower()
    {
        var penalty = WerewolfBestaInteriorService.ComputeSocialDicePenalty(5, 3);
        Assert.Equal(2, penalty);
    }

    [Fact]
    public void BestaInteriorNoPenaltyWhenRageEqualsWillpower()
    {
        var penalty = WerewolfBestaInteriorService.ComputeSocialDicePenalty(5, 5);
        Assert.Equal(0, penalty);
    }

    [Fact]
    public void BestaInteriorNoPenaltyWhenRageBelowWillpower()
    {
        var penalty = WerewolfBestaInteriorService.ComputeSocialDicePenalty(3, 5);
        Assert.Equal(0, penalty);
    }

    [Fact]
    public void BestaInteriorIsSocialTestReturnsTrueForCharisma()
    {
        Assert.True(WerewolfBestaInteriorService.IsSocialTest("character.attribute.charisma", "character.ability.empathy"));
    }

    [Fact]
    public void BestaInteriorIsSocialTestReturnsFalseForStrength()
    {
        Assert.False(WerewolfBestaInteriorService.IsSocialTest("character.attribute.strength", "character.ability.brawl"));
    }

    [Fact]
    public void PermanecerAtivoSuccessEntersWildFrenzy()
    {
        var track = WerewolfHealthTrackComputer.Compute(
            Enumerable.Repeat(new WerewolfDamageMark(WerewolfDamageCategory.Lethal, 1), 7).ToList());
        var state = new WerewolfRuntimeCharacterState(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            "draft-1",
            1,
            new Dictionary<string, string>(StringComparer.Ordinal),
            5, 5, 3, 3, 4, 4, 0, 0, 0, 0, 0, 0,
            WerewolfRaceIdentifiers.Homid,
            track);

        var result = WerewolfPermanecerAtivoService.PermanecerAtivo(new WerewolfPermanecerAtivoRequest(
            "req", state, 1, 3, 0));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.UpdatedState);
        Assert.NotNull(result.UpdatedState!.FrenzyState);
        Assert.True(result.UpdatedState.FrenzyState.IsInFrenzy);
        Assert.Equal(WerewolfFrenzyType.Wild, result.UpdatedState.FrenzyState.FrenzyType);
    }

    [Fact]
    public void FrenzyStateImmutabilityPreservedOnEnter()
    {
        var original = State(frenzyState: null);
        var result = WerewolfFrenzyResolutionService.EnterFrenzy(new WerewolfEnterFrenzyRequest(
            "req", original, 1, WerewolfFrenzyType.Wild, "trigger", 4));

        Assert.True(result.Succeeded);
        Assert.Null(original.FrenzyState);
        Assert.NotNull(result.UpdatedState!.FrenzyState);
    }

    [Fact]
    public void FrenzyTestDefinitionInvalidRankReturnsInvalid()
    {
        var result = WerewolfFrenzyTestDefinitionService.ComputeTestDefinition(
            "req", 5, 5, 0, null, null, "half", null);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void FrenzyTestDefinitionZeroRageReturnsInvalid()
    {
        var result = WerewolfFrenzyTestDefinitionService.ComputeTestDefinition(
            "req", 0, 5, 1, null, null, "half", null);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void FrenzyTestDefinitionEmptyRequestIdReturnsInvalid()
    {
        var result = WerewolfFrenzyTestDefinitionService.ComputeTestDefinition(
            "", 5, 5, 1, null, null, "half", null);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void EnterFrenzyRejectsNoneFrenzyType()
    {
        var state = State();
        var result = WerewolfFrenzyResolutionService.EnterFrenzy(new WerewolfEnterFrenzyRequest(
            "req", state, 1, WerewolfFrenzyType.None, "trigger", 4));

        Assert.False(result.Succeeded);
        Assert.Equal("InvalidFrenzyType", result.ErrorCode);
    }

    [Fact]
    public void EnterFrenzyRejectsVersionMismatch()
    {
        var state = State();
        var result = WerewolfFrenzyResolutionService.EnterFrenzy(new WerewolfEnterFrenzyRequest(
            "req", state, 2, WerewolfFrenzyType.Wild, "trigger", 4));

        Assert.False(result.Succeeded);
        Assert.Equal("StaleVersion", result.ErrorCode);
    }

    [Fact]
    public void EnterFrenzyRejectsEmptyRequestId()
    {
        var state = State();
        var result = WerewolfFrenzyResolutionService.EnterFrenzy(new WerewolfEnterFrenzyRequest(
            "", state, 1, WerewolfFrenzyType.Wild, "trigger", 4));

        Assert.False(result.Succeeded);
        Assert.Equal("InvalidRequestId", result.ErrorCode);
    }

    [Fact]
    public void SuppressFrenzyRejectsWhenNotInFrenzy()
    {
        var result = WerewolfFrenzyResolutionService.SuppressFrenzy(State(), 1);

        Assert.False(result.Succeeded);
        Assert.Equal("NotInFrenzy", result.ErrorCode);
    }

    [Fact]
    public void SuppressFrenzyRejectsVersionMismatch()
    {
        var frenzyState = new WerewolfFrenzyState(true, WerewolfFrenzyType.Wild, "trigger", 4, 0, null, false, "Line 2916");
        var state = State(frenzyState: frenzyState);
        var result = WerewolfFrenzyResolutionService.SuppressFrenzy(state, 2);

        Assert.False(result.Succeeded);
        Assert.Equal("StaleVersion", result.ErrorCode);
    }

    [Fact]
    public void EndFrenzyRejectsWhenNotInFrenzy()
    {
        var result = WerewolfFrenzyResolutionService.EndFrenzy(State(), 1);

        Assert.False(result.Succeeded);
        Assert.Equal("NotInFrenzy", result.ErrorCode);
    }

    [Fact]
    public void EndFrenzyRejectsVersionMismatch()
    {
        var frenzyState = new WerewolfFrenzyState(true, WerewolfFrenzyType.Wild, "trigger", 4, 0, null, false, "Line 2916");
        var state = State(frenzyState: frenzyState);
        var result = WerewolfFrenzyResolutionService.EndFrenzy(state, 2);

        Assert.False(result.Succeeded);
        Assert.Equal("StaleVersion", result.ErrorCode);
    }

    [Fact]
    public void EvaluateFrenzyActionSuppressedFrenzyAllowsAllActions()
    {
        var frenzyState = new WerewolfFrenzyState(true, WerewolfFrenzyType.Wild, "trigger", 4, 0, null, true, "Line 2916");
        var state = State(frenzyState: frenzyState);
        var availability = WerewolfFrenzyResolutionService.EvaluateFrenzyAction(state, "attack");

        Assert.Equal("available", availability);
    }

    [Fact]
    public void EvaluateFrenzyActionNoFrenzyAllowsAllActions()
    {
        var state = State(frenzyState: null);
        var availability = WerewolfFrenzyResolutionService.EvaluateFrenzyAction(state, "attack");

        Assert.Equal("available", availability);
    }

    private static WerewolfRuntimeCharacterState State(
        int ragePermanent = 5,
        int rageCurrent = 5,
        int willpowerPermanent = 5,
        int willpowerCurrent = 5,
        string currentForm = "character.form.homid",
        string birthRace = "homid",
        WerewolfFrenzyState? frenzyState = null)
    {
        return new WerewolfRuntimeCharacterState(
            "test", "1.0", "draft", 1, new Dictionary<string, string>(),
            ragePermanent, rageCurrent, 5, 5, willpowerPermanent, willpowerCurrent,
            0, 0, 0, 0, 0, 0,
            birthRace, null, currentForm, Array.Empty<WerewolfCondition>(), frenzyState);
    }
}
