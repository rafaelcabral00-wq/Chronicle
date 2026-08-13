using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfActionRollInterpretationTests
{
    [Fact]
    public void InterpretReturnsSuccessWhenSuccessesExceedDifficulty()
    {
        var request = new WerewolfActionRollInterpretationRequest("req-1", [8, 9, 10], 6, 3);
        var result = WerewolfActionRollInterpretationService.Interpret(request);

        Assert.True(result.Succeeded);
        Assert.Equal("req-1", result.RequestId);
        Assert.Equal(3, result.SuccessCount);
        Assert.Equal(WerewolfActionRollInterpretationService.SuccessStatus, result.InterpretationStatus);
        Assert.Null(result.FailureClassification);
        Assert.Null(result.BotchClassification);
    }

    [Fact]
    public void InterpretReturnsFailureWhenNoSuccessesAndNoOnes()
    {
        var request = new WerewolfActionRollInterpretationRequest("req-1", [2, 3, 4], 6, 3);
        var result = WerewolfActionRollInterpretationService.Interpret(request);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(WerewolfActionRollInterpretationService.FailureStatus, result.InterpretationStatus);
        Assert.Equal("NoSuccesses", result.FailureClassification);
        Assert.Null(result.BotchClassification);
    }

    [Fact]
    public void InterpretReturnsBotchWhenNoSuccessesAndOnesPresent()
    {
        var request = new WerewolfActionRollInterpretationRequest("req-1", [1, 2, 3], 6, 3);
        var result = WerewolfActionRollInterpretationService.Interpret(request);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(WerewolfActionRollInterpretationService.BotchStatus, result.InterpretationStatus);
        Assert.Null(result.FailureClassification);
        Assert.Equal("CriticalFailure", result.BotchClassification);
    }

    [Fact]
    public void InterpretDoesNotBotchWhenSuccessesExistEvenWithOnes()
    {
        var request = new WerewolfActionRollInterpretationRequest("req-1", [1, 6, 7], 6, 3);
        var result = WerewolfActionRollInterpretationService.Interpret(request);

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(WerewolfActionRollInterpretationService.SuccessStatus, result.InterpretationStatus);
        Assert.Null(result.BotchClassification);
    }

    [Fact]
    public void InterpretCancelsSuccessesWithOnes()
    {
        var request = new WerewolfActionRollInterpretationRequest("req-1", [1, 6, 7], 6, 3);
        var result = WerewolfActionRollInterpretationService.Interpret(request);

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(WerewolfActionRollInterpretationService.SuccessStatus, result.InterpretationStatus);
    }

    [Fact]
    public void InterpretReturnsBotchWhenOnesExceedSuccesses()
    {
        var request = new WerewolfActionRollInterpretationRequest("req-1", [1, 1, 6], 6, 3);
        var result = WerewolfActionRollInterpretationService.Interpret(request);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(WerewolfActionRollInterpretationService.BotchStatus, result.InterpretationStatus);
        Assert.Equal("CriticalFailure", result.BotchClassification);
    }

    [Fact]
    public void InterpretReturnsZeroPoolErrorForZeroDiceQuantity()
    {
        var request = new WerewolfActionRollInterpretationRequest("req-1", [], 6, 0);
        var result = WerewolfActionRollInterpretationService.Interpret(request);

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(WerewolfActionRollInterpretationService.ZeroPoolStatus, result.InterpretationStatus);
        Assert.Contains(result.Findings, f => f.Code == "ZeroPoolCannotAttempt");
    }

    [Fact]
    public void InterpretRejectsEmptyRequestId()
    {
        var request = new WerewolfActionRollInterpretationRequest(string.Empty, [1, 5], 6, 2);
        var result = WerewolfActionRollInterpretationService.Interpret(request);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == "InvalidRequestId");
    }

    [Fact]
    public void InterpretRejectsNegativeDiceQuantity()
    {
        var request = new WerewolfActionRollInterpretationRequest("req-1", [1, 5], 6, -1);
        var result = WerewolfActionRollInterpretationService.Interpret(request);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == "InvalidDiceQuantity");
    }

    [Fact]
    public void InterpretRejectsInvalidDifficulty()
    {
        var request = new WerewolfActionRollInterpretationRequest("req-1", [1, 5], 0, 2);
        var result = WerewolfActionRollInterpretationService.Interpret(request);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == "InvalidDifficulty");
    }

    [Fact]
    public void InterpretRejectsDiceCountMismatch()
    {
        var request = new WerewolfActionRollInterpretationRequest("req-1", [1, 5], 6, 3);
        var result = WerewolfActionRollInterpretationService.Interpret(request);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == "DiceCountMismatch");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    [InlineData(-1)]
    public void InterpretRejectsOutOfBoundsDieFace(int dieFace)
    {
        var request = new WerewolfActionRollInterpretationRequest("req-1", [dieFace, 5, 10], 6, 3);
        var result = WerewolfActionRollInterpretationService.Interpret(request);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == "InvalidDieFace");
    }

    [Fact]
    public void InterpretIsDeterministicForSameInput()
    {
        var request = new WerewolfActionRollInterpretationRequest("req-1", [2, 4, 6, 8], 7, 4);
        var result1 = WerewolfActionRollInterpretationService.Interpret(request);
        var result2 = WerewolfActionRollInterpretationService.Interpret(request);

        Assert.Equal(result1.SerializedInterpretation, result2.SerializedInterpretation);
        Assert.Equal(result1.InterpretationStatus, result2.InterpretationStatus);
    }

    [Fact]
    public void InterpretRetainsRawDiceValuesSeparatelyFromInterpretation()
    {
        var request = new WerewolfActionRollInterpretationRequest("req-1", [3, 7, 10], 6, 3);
        var result = WerewolfActionRollInterpretationService.Interpret(request);

        Assert.True(result.Succeeded);
        Assert.Equal([3, 7, 10], result.RawDiceValues);
        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(WerewolfActionRollInterpretationService.SuccessStatus, result.InterpretationStatus);
    }

    [Fact]
    public void InterpretCountsTensAsSuccesses()
    {
        var request = new WerewolfActionRollInterpretationRequest("req-1", [10, 10, 1], 6, 3);
        var result = WerewolfActionRollInterpretationService.Interpret(request);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.RawSuccesses);
        Assert.Equal(1, result.OnesCount);
        Assert.Equal(1, result.SuccessCount);
    }

    [Fact]
    public void InterpretTenAndOneWithoutSpecializationCancelsToOne()
    {
        var request = new WerewolfActionRollInterpretationRequest("req-1", [10, 1], 6, 2);
        var result = WerewolfActionRollInterpretationService.Interpret(request);

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.RawSuccesses);
        Assert.Equal(1, result.OnesCount);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(WerewolfActionRollInterpretationService.BotchStatus, result.InterpretationStatus);
    }

    [Fact]
    public void InterpretTwoTensAndOneWithoutSpecializationCancelsToOne()
    {
        var request = new WerewolfActionRollInterpretationRequest("req-1", [10, 10, 1], 6, 3);
        var result = WerewolfActionRollInterpretationService.Interpret(request);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.RawSuccesses);
        Assert.Equal(1, result.OnesCount);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(WerewolfActionRollInterpretationService.SuccessStatus, result.InterpretationStatus);
    }

    [Fact]
    public void InterpretRejectsDifficultyBelowTwo()
    {
        var request = new WerewolfActionRollInterpretationRequest("req-1", [6], 1, 1);
        var result = WerewolfActionRollInterpretationService.Interpret(request);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == "InvalidDifficulty");
    }

    [Fact]
    public void InterpretRejectsDifficultyAboveTen()
    {
        var request = new WerewolfActionRollInterpretationRequest("req-1", [10], 11, 1);
        var result = WerewolfActionRollInterpretationService.Interpret(request);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == "InvalidDifficulty");
    }
}