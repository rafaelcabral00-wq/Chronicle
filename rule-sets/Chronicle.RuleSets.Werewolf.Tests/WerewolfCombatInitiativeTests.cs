using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfCombatInitiativeTests
{
    private static WerewolfRuntimeCharacterState State(int ragePermanent = 5, int rageCurrent = 5, int dexterity = 3, int wits = 2, int version = 1, string currentForm = "character.form.homid", string birthRace = "homid")
    {
        return new WerewolfRuntimeCharacterState(
            "test", "1.0", "draft", version, new Dictionary<string, string>(),
            ragePermanent, rageCurrent, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0,
            birthRace, null, currentForm);
    }

    [Fact]
    public void CalculateInitiativeReturnsFixedModifierPlusSuppliedDie()
    {
        var result = WerewolfCombatInitiativeService.CalculateInitiative(new WerewolfCombatInitiativeRequest(
            "req", State(), 1, 3, 2, 7, null));

        Assert.True(result.Succeeded);
        Assert.Equal(5, result.InitiativeModifier);
        Assert.Equal(7, result.SuppliedDieRoll);
        Assert.Equal(12, result.FinalInitiative);
        Assert.Equal(3, result.Dexterity);
        Assert.Equal(2, result.Wits);
        Assert.Equal(2, result.UpdatedState!.RuntimeStateVersion);
    }

    [Fact]
    public void CalculateInitiativeSuppliedDieRollOfOne()
    {
        var result = WerewolfCombatInitiativeService.CalculateInitiative(new WerewolfCombatInitiativeRequest(
            "req", State(), 1, 3, 2, 1, null));

        Assert.True(result.Succeeded);
        Assert.Equal(6, result.FinalInitiative);
    }

    [Fact]
    public void CalculateInitiativeSuppliedDieRollOfTen()
    {
        var result = WerewolfCombatInitiativeService.CalculateInitiative(new WerewolfCombatInitiativeRequest(
            "req", State(), 1, 3, 2, 10, null));

        Assert.True(result.Succeeded);
        Assert.Equal(15, result.FinalInitiative);
    }

    [Fact]
    public void CalculateInitiativeMaxExtraActionsCalculatesCorrectly()
    {
        var result = WerewolfCombatInitiativeService.CalculateInitiative(new WerewolfCombatInitiativeRequest(
            "req", State(ragePermanent: 10), 1, 4, 3, 2, 2));

        Assert.True(result.Succeeded);
        Assert.Equal(7, result.InitiativeModifier);
        Assert.Equal(9, result.FinalInitiative);
        Assert.Equal(3, result.MaxExtraActions);
    }

    [Fact]
    public void CalculateInitiativeMissingRequestIdReturnsFailure()
    {
        var result = WerewolfCombatInitiativeService.CalculateInitiative(new WerewolfCombatInitiativeRequest(
            "", State(), 1, 3, 2, 7, null));

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.FinalInitiative);
    }

    [Fact]
    public void CalculateInitiativeNullCurrentStateReturnsFailure()
    {
        WerewolfRuntimeCharacterState? nullState = null;
        var result = WerewolfCombatInitiativeService.CalculateInitiative(new WerewolfCombatInitiativeRequest(
            "req", nullState!, 1, 3, 2, 7, null));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void CalculateInitiativeVersionMismatchReturnsFailure()
    {
        var result = WerewolfCombatInitiativeService.CalculateInitiative(new WerewolfCombatInitiativeRequest(
            "req", State(), 2, 3, 2, 7, null));

        Assert.False(result.Succeeded);
        Assert.Equal(1, result.UpdatedState!.RuntimeStateVersion);
    }

    [Fact]
    public void CalculateInitiativeNegativeDexterityReturnsFailure()
    {
        var result = WerewolfCombatInitiativeService.CalculateInitiative(new WerewolfCombatInitiativeRequest(
            "req", State(), 1, -1, 2, 7, null));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void CalculateInitiativeNegativeWitsReturnsFailure()
    {
        var result = WerewolfCombatInitiativeService.CalculateInitiative(new WerewolfCombatInitiativeRequest(
            "req", State(), 1, 3, -1, 7, null));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void CalculateInitiativeInvalidSuppliedDieRollReturnsFailure()
    {
        var result = WerewolfCombatInitiativeService.CalculateInitiative(new WerewolfCombatInitiativeRequest(
            "req", State(), 1, 3, 2, 11, null));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void CalculateInitiativeZeroDexterityAndWitsReturnsZeroModifier()
    {
        var result = WerewolfCombatInitiativeService.CalculateInitiative(new WerewolfCombatInitiativeRequest(
            "req", State(), 1, 0, 0, 5, null));

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.InitiativeModifier);
        Assert.Equal(5, result.FinalInitiative);
    }

    [Fact]
    public void CalculateInitiativeRageExtraActionsExceedsMaxRecordsFinding()
    {
        var result = WerewolfCombatInitiativeService.CalculateInitiative(new WerewolfCombatInitiativeRequest(
            "req", State(ragePermanent: 4), 1, 3, 2, 5, 5));

        Assert.True(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Contains("exceeds maximum"));
    }

    [Fact]
    public void ComputeInitiativeModifierReturnsDexterityPlusWits()
    {
        var attributes = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [WerewolfAttributeIdentifiers.Dexterity] = 4,
            [WerewolfAttributeIdentifiers.Wits] = 3
        };

        var modifier = WerewolfCombatInitiativeService.ComputeInitiativeModifier(attributes);

        Assert.Equal(7, modifier);
    }

    [Fact]
    public void GetTurnStructureReturnsExpectedPhases()
    {
        var structure = WerewolfCombatInitiativeService.GetTurnStructure();

        Assert.Equal(["Initiative", "Declaration", "Attack", "Damage"], structure);
    }

    [Fact]
    public void GetTurnDurationReturnsThreeSeconds()
    {
        Assert.Equal("3 seconds", WerewolfCombatInitiativeService.GetTurnDuration());
    }
}
