using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfCombatRageTests
{
    [Fact]
    public void SpendRageExtraActionDecrementsRageAndGrantsActions()
    {
        var state = State(ragePermanent: 6, rageCurrent: 3, dexterity: 4, wits: 3);
        var result = WerewolfCombatRageService.SpendRage(new WerewolfCombatRageRequest(
            "req", state, 1, 1, "extra-action", 4, 3));

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.RageSpent);
        Assert.Equal(2, result.NewRageCurrent);
        Assert.Equal(1, result.ExtraActionsGranted);
        Assert.False(result.TransformationAllowed);
        Assert.False(result.StunNegated);
        Assert.Equal(2, result.UpdatedState!.RuntimeStateVersion);
    }

    [Fact]
    public void SpendRageTransformationAllowsTransformation()
    {
        var state = State(ragePermanent: 5, rageCurrent: 2);
        var result = WerewolfCombatRageService.SpendRage(new WerewolfCombatRageRequest(
            "req", state, 1, 1, "transformation", null, null));

        Assert.True(result.Succeeded);
        Assert.True(result.TransformationAllowed);
        Assert.Equal(0, result.ExtraActionsGranted);
        Assert.Equal(1, result.NewRageCurrent);
    }

    [Fact]
    public void SpendRageIgnoreStunNegatesStun()
    {
        var state = State(ragePermanent: 5, rageCurrent: 1);
        var result = WerewolfCombatRageService.SpendRage(new WerewolfCombatRageRequest(
            "req", state, 1, 1, "ignore-stun", null, null));

        Assert.True(result.Succeeded);
        Assert.True(result.StunNegated);
        Assert.Equal(0, result.NewRageCurrent);
    }

    [Fact]
    public void SpendRageInsufficientRageReturnsFailure()
    {
        var state = State(ragePermanent: 5, rageCurrent: 0);
        var result = WerewolfCombatRageService.SpendRage(new WerewolfCombatRageRequest(
            "req", state, 1, 1, "extra-action", 3, 3));

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.RageSpent);
        Assert.Equal(0, result.NewRageCurrent);
    }

    [Fact]
    public void SpendRageZeroRageReturnsFailure()
    {
        var state = State();
        var result = WerewolfCombatRageService.SpendRage(new WerewolfCombatRageRequest(
            "req", state, 1, 0, "extra-action", 3, 3));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void SpendRageUnknownPurposeReturnsFailure()
    {
        var state = State(rageCurrent: 2);
        var result = WerewolfCombatRageService.SpendRage(new WerewolfCombatRageRequest(
            "req", state, 1, 1, "unknown", 3, 3));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void SpendRageEmptyPurposeReturnsFailure()
    {
        var state = State(rageCurrent: 2);
        var result = WerewolfCombatRageService.SpendRage(new WerewolfCombatRageRequest(
            "req", state, 1, 1, "", 3, 3));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void SpendRageExtraActionExceedsMaxGrantsOnlyMax()
    {
        var state = State(ragePermanent: 4, rageCurrent: 3, dexterity: 2, wits: 2);
        var result = WerewolfCombatRageService.SpendRage(new WerewolfCombatRageRequest(
            "req", state, 1, 3, "extra-action", 2, 2));

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.ExtraActionsGranted);
        Assert.Equal(0, result.NewRageCurrent);
        Assert.Single(result.Findings);
        Assert.Contains("max 2", result.Findings[0]);
    }

    [Fact]
    public void CalculateExtraActionsValidInputReturnsCorrectCount()
    {
        var state = State(ragePermanent: 8);
        var result = WerewolfCombatRageService.CalculateExtraActions(state, 3);

        Assert.NotNull(result);
        Assert.Equal(3, result.ExtraActions);
        Assert.Equal(3, result.RageInvested);
    }

    [Fact]
    public void CalculateExtraActionsExceedsMaxClampsToMax()
    {
        var state = State(ragePermanent: 4);
        var result = WerewolfCombatRageService.CalculateExtraActions(state, 10);

        Assert.NotNull(result);
        Assert.Equal(2, result.ExtraActions);
    }

    [Fact]
    public void CalculateExtraActionsZeroOrNegativeReturnsZero()
    {
        var state = State();
        Assert.Equal(0, WerewolfCombatRageService.CalculateExtraActions(state, 0).ExtraActions);
        Assert.Equal(0, WerewolfCombatRageService.CalculateExtraActions(state, -1).ExtraActions);
    }

    [Fact]
    public void SpendRageVersionMismatchReturnsFailure()
    {
        var state = State(rageCurrent: 2);
        var result = WerewolfCombatRageService.SpendRage(new WerewolfCombatRageRequest(
            "req", state, 2, 1, "extra-action", 3, 3));

        Assert.False(result.Succeeded);
    }

    private static WerewolfRuntimeCharacterState State(int ragePermanent = 5, int rageCurrent = 5, int dexterity = 3, int wits = 2, string form = "character.form.homid", string birthRace = "homid")
    {
        return new WerewolfRuntimeCharacterState(
            "test", "1.0", "draft", 1, new Dictionary<string, string>(),
            ragePermanent, rageCurrent, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0,
            birthRace, null, form);
    }
}

