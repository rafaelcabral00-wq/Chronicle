using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfResistedTestTests
{
    [Fact]
    public void SideAWinsWhenNetSuccessesGreaterThanZero()
    {
        var definition = new WerewolfResistedTestDefinition("req-1", 3, 6, 3, 6);
        var sideADice = new[] { 8, 9, 10 };
        var sideBDice = new[] { 2, 3, 4 };
        var result = WerewolfResistedTestService.Interpret(definition, sideADice, sideBDice);

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.SideASuccesses);
        Assert.Equal(0, result.SideBSuccesses);
        Assert.Equal(3, result.NetSuccesses);
        Assert.Equal(WerewolfResistedTestWinner.SideA, result.Winner);
        Assert.Equal("side-a-wins", result.Status);
    }

    [Fact]
    public void SideBWinsWhenNetSuccessesLessThanZero()
    {
        var definition = new WerewolfResistedTestDefinition("req-1", 3, 6, 3, 6);
        var sideADice = new[] { 2, 3, 4 };
        var sideBDice = new[] { 8, 9, 10 };
        var result = WerewolfResistedTestService.Interpret(definition, sideADice, sideBDice);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.SideASuccesses);
        Assert.Equal(3, result.SideBSuccesses);
        Assert.Equal(-3, result.NetSuccesses);
        Assert.Equal(WerewolfResistedTestWinner.SideB, result.Winner);
        Assert.Equal("side-b-wins", result.Status);
    }

    [Fact]
    public void TieWhenNetSuccessesZeroAndBothSucceeded()
    {
        var definition = new WerewolfResistedTestDefinition("req-1", 3, 6, 3, 6);
        var sideADice = new[] { 8, 9, 10 };
        var sideBDice = new[] { 8, 9, 10 };
        var result = WerewolfResistedTestService.Interpret(definition, sideADice, sideBDice);

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.SideASuccesses);
        Assert.Equal(3, result.SideBSuccesses);
        Assert.Equal(0, result.NetSuccesses);
        Assert.Equal(WerewolfResistedTestWinner.Tie, result.Winner);
        Assert.Equal("tie", result.Status);
    }

    [Fact]
    public void BotchOnSideA()
    {
        var definition = new WerewolfResistedTestDefinition("req-1", 3, 6, 3, 6);
        var sideADice = new[] { 1, 2, 3 };
        var sideBDice = new[] { 8, 9, 10 };
        var result = WerewolfResistedTestService.Interpret(definition, sideADice, sideBDice);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.SideASuccesses);
        Assert.Equal(3, result.SideBSuccesses);
        Assert.Equal(-3, result.NetSuccesses);
        Assert.Equal(WerewolfResistedTestWinner.SideB, result.Winner);
        Assert.Equal("side-a-botch", result.Status);
    }

    [Fact]
    public void BotchOnSideB()
    {
        var definition = new WerewolfResistedTestDefinition("req-1", 3, 6, 3, 6);
        var sideADice = new[] { 8, 9, 10 };
        var sideBDice = new[] { 1, 2, 3 };
        var result = WerewolfResistedTestService.Interpret(definition, sideADice, sideBDice);

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.SideASuccesses);
        Assert.Equal(0, result.SideBSuccesses);
        Assert.Equal(3, result.NetSuccesses);
        Assert.Equal(WerewolfResistedTestWinner.SideA, result.Winner);
        Assert.Equal("side-b-botch", result.Status);
    }

    [Fact]
    public void BothBotch()
    {
        var definition = new WerewolfResistedTestDefinition("req-1", 3, 6, 3, 6);
        var sideADice = new[] { 1, 2, 3 };
        var sideBDice = new[] { 1, 2, 3 };
        var result = WerewolfResistedTestService.Interpret(definition, sideADice, sideBDice);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.SideASuccesses);
        Assert.Equal(0, result.SideBSuccesses);
        Assert.Equal(0, result.NetSuccesses);
        Assert.Equal(WerewolfResistedTestWinner.None, result.Winner);
        Assert.Equal("both-botch", result.Status);
    }

    [Fact]
    public void BothFail()
    {
        var definition = new WerewolfResistedTestDefinition("req-1", 3, 6, 3, 6);
        var sideADice = new[] { 2, 3, 4 };
        var sideBDice = new[] { 2, 3, 4 };
        var result = WerewolfResistedTestService.Interpret(definition, sideADice, sideBDice);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.SideASuccesses);
        Assert.Equal(0, result.SideBSuccesses);
        Assert.Equal(0, result.NetSuccesses);
        Assert.Equal(WerewolfResistedTestWinner.None, result.Winner);
        Assert.Equal("both-fail", result.Status);
    }

    [Fact]
    public void DifferentPoolsAndDifficultiesWork()
    {
        var definition = new WerewolfResistedTestDefinition("req-1", 5, 7, 2, 5);
        var sideADice = new[] { 7, 8, 9, 10, 6 };
        var sideBDice = new[] { 6, 6 };
        var result = WerewolfResistedTestService.Interpret(definition, sideADice, sideBDice);

        Assert.True(result.Succeeded);
        Assert.Equal(4, result.SideASuccesses);
        Assert.Equal(2, result.SideBSuccesses);
        Assert.Equal(2, result.NetSuccesses);
        Assert.Equal(WerewolfResistedTestWinner.SideA, result.Winner);
        Assert.Equal("side-a-wins", result.Status);
    }

    [Fact]
    public void NetSuccessesComputedCorrectly()
    {
        var definition = new WerewolfResistedTestDefinition("req-1", 3, 6, 2, 6);
        var sideADice = new[] { 8, 9, 10 };
        var sideBDice = new[] { 8, 9 };
        var result = WerewolfResistedTestService.Interpret(definition, sideADice, sideBDice);

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.SideASuccesses);
        Assert.Equal(2, result.SideBSuccesses);
        Assert.Equal(1, result.NetSuccesses);
        Assert.Equal(WerewolfResistedTestWinner.SideA, result.Winner);
        Assert.Equal("side-a-wins", result.Status);
    }

    [Theory]
    [InlineData(new int[] { 8, 9, 10 }, new int[] { 8, 9, 10 })]
    [InlineData(new int[] { 6, 7, 8 }, new int[] { 6, 7, 8 })]
    [InlineData(new int[] { 1, 1, 1 }, new int[] { 1, 1, 1 })]
    public void NoInternalRandomnessForSameInputs(int[] sideA, int[] sideB)
    {
        var definition = new WerewolfResistedTestDefinition("req-1", 3, 6, 3, 6);
        var result1 = WerewolfResistedTestService.Interpret(definition, sideA, sideB);
        var result2 = WerewolfResistedTestService.Interpret(definition, sideA, sideB);

        Assert.Equal(result1.SerializedResult, result2.SerializedResult);
        Assert.Equal(result1.Status, result2.Status);
        Assert.Equal(result1.Winner, result2.Winner);
    }

    [Fact]
    public void TieWhenBothSucceedWithEqualSuccesses()
    {
        var definition = new WerewolfResistedTestDefinition("req-1", 4, 6, 4, 6);
        var sideADice = new[] { 6, 7, 8, 9 };
        var sideBDice = new[] { 6, 7, 8, 9 };
        var result = WerewolfResistedTestService.Interpret(definition, sideADice, sideBDice);

        Assert.True(result.Succeeded);
        Assert.Equal(4, result.SideASuccesses);
        Assert.Equal(4, result.SideBSuccesses);
        Assert.Equal(0, result.NetSuccesses);
        Assert.Equal(WerewolfResistedTestWinner.Tie, result.Winner);
        Assert.Equal("tie", result.Status);
    }
}
