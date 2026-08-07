using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfActionRollInterpretationTests
{
    [Fact]
    public void InterpretReturnsPendingStatusWithRawDice()
    {
        var request = new WerewolfActionRollInterpretationRequest("req-1", [1, 5, 10], 6, 3);
        var result = WerewolfActionRollInterpretationService.Interpret(request);

        Assert.True(result.Succeeded);
        Assert.Equal("req-1", result.RequestId);
        Assert.Equal(3, result.RawDiceValues.Count);
        Assert.Equal(6, result.Difficulty);
        Assert.Equal(3, result.DiceQuantity);
        Assert.Equal(WerewolfActionRollInterpretationService.PendingExtractionStatus, result.InterpretationStatus);
        Assert.Null(result.SuccessCount);
        Assert.Null(result.FailureClassification);
        Assert.Null(result.BotchClassification);
        Assert.NotEmpty(result.SerializedInterpretation);
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
        Assert.Null(result.SuccessCount);
        Assert.Null(result.FailureClassification);
    }
}
