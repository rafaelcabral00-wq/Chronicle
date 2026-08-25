using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfRiteExecutionTests
{
    [Fact]
    public void ExecuteHuntingStoneReturnsSuccessWhenSuccessesMeetDifficulty()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [7, 8, 9],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.Equal("req-1", result.RequestId);
        Assert.Equal(WerewolfRiteIdentifiers.HuntingStone, result.RiteKey);
        Assert.Equal(7, result.Difficulty);
        Assert.Equal(3, result.DicePool);
        Assert.Equal(3, result.SuccessCount);
        Assert.Equal(WerewolfActionRollInterpretationService.SuccessStatus, result.InterpretationStatus);
        Assert.NotNull(result.Effect);
    }

    [Fact]
    public void ExecuteHuntingStoneReducesDifficultyWhenTargetPiecePossessed()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [6, 7, 8],
            true);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.Equal(6, result.Difficulty);
        Assert.Equal(3, result.SuccessCount);
    }

    [Fact]
    public void ExecuteHuntingStoneReturnsFailureWhenNoSuccesses()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [2, 3, 4],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(WerewolfActionRollInterpretationService.FailureStatus, result.InterpretationStatus);
    }

    [Fact]
    public void ExecuteHuntingStoneReturnsBotchWhenOnesExceedSuccesses()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [1, 1, 6],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(WerewolfActionRollInterpretationService.BotchStatus, result.InterpretationStatus);
    }

    [Fact]
    public void ExecuteHuntingStoneRejectsUnknownRiteKey()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            "rite.unknown",
            [7, 8, 9],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.False(result.Succeeded);
        Assert.Equal("UnknownRite", result.Findings[0].Code);
    }

    [Fact]
    public void ExecuteHuntingStoneRejectsEmptyDiceValues()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.False(result.Succeeded);
        Assert.Equal("InvalidDiceValues", result.Findings[0].Code);
    }

    [Fact]
    public void ExecuteHuntingStoneRejectsInvalidDieFace()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [0, 6, 7],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.False(result.Succeeded);
        Assert.Equal("InvalidDieFace", result.Findings[0].Code);
    }

    [Fact]
    public void ExecuteHuntingStoneRejectsEmptyRequestId()
    {
        var request = new WerewolfRiteExecutionRequest(
            string.Empty,
            WerewolfRiteIdentifiers.HuntingStone,
            [6, 7, 8],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.False(result.Succeeded);
        Assert.Equal("InvalidRequestId", result.Findings[0].Code);
    }

    [Fact]
    public void ExecuteHuntingStoneRejectsEmptyRiteKey()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            string.Empty,
            [6, 7, 8],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.False(result.Succeeded);
        Assert.Equal("InvalidRiteKey", result.Findings[0].Code);
    }

    [Fact]
    public void ExecuteHuntingStoneReturnsGeneralLocationOnSuccess()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [8, 9, 10],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.SuccessCount);
        Assert.Equal("Fornece apenas a localização geral do alvo. A posse de um pedaço do alvo reduz a dificuldade em 1 ponto.", result.Effect);
    }

    [Fact]
    public void ExecuteHuntingStoneReturnsNoInformationOnFailure()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [2, 3, 4],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal("No information gained.", result.Effect);
    }

    [Fact]
    public void ExecuteHuntingStoneDoesNotDependOnSpiritUmbra()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [7, 8, 9],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.SuccessCount);
        Assert.DoesNotContain("spirit", result.Effect, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("umbra", result.Effect, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExecuteHuntingStoneDoesNotDependOnPackSept()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [7, 8, 9],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.SuccessCount);
        Assert.DoesNotContain("pack", result.Effect, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sept", result.Effect, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("caern", result.Effect, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExecuteHuntingStoneDoesNotClaimLearningImplementation()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [6, 7, 8],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.DoesNotContain("xp", result.Effect ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("background", result.Effect ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("knowledge", result.Effect ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExecuteHuntingStonePreservesProgressionState()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [7, 8, 9],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.SuccessCount);
    }
}
