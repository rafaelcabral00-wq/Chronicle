using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfExtendedTestTests
{
    [Fact]
    public void InitialProgressStartsWithZeroAccumulatedAndInProgress()
    {
        var definition = new WerewolfExtendedTestDefinition("req-1", 5, 6, 3);
        var progress = WerewolfExtendedTestService.CreateInitialProgress(definition);

        Assert.Equal("req-1", progress.RequestId);
        Assert.Equal(0, progress.AccumulatedSuccesses);
        Assert.Equal(0, progress.AttemptCount);
        Assert.False(progress.IsBotched);
        Assert.Equal(WerewolfExtendedTestStatus.InProgress, progress.Status);
    }

    [Fact]
    public void SuccessfulAccumulationAddsFinalSuccesses()
    {
        var definition = new WerewolfExtendedTestDefinition("req-1", 3, 6, 5);
        var progress = WerewolfExtendedTestService.CreateInitialProgress(definition);
        var diceValues = new[] { 8, 9, 10 };
        var result = WerewolfExtendedTestService.Advance(definition, progress, diceValues);

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.UpdatedProgress.AccumulatedSuccesses);
        Assert.Equal(0, result.UpdatedProgress.AttemptCount);
        Assert.False(result.UpdatedProgress.IsBotched);
        Assert.Equal(WerewolfExtendedTestStatus.InProgress, result.UpdatedProgress.Status);
    }

    [Fact]
    public void MultipleRollsAccumulateCorrectly()
    {
        var definition = new WerewolfExtendedTestDefinition("req-1", 3, 6, 5);
        var progress = WerewolfExtendedTestService.CreateInitialProgress(definition);

        var dice1 = new[] { 8, 9, 10 };
        var result1 = WerewolfExtendedTestService.Advance(definition, progress, dice1);
        var dice2 = new[] { 6, 7, 5 };
        var result2 = WerewolfExtendedTestService.Advance(definition, result1.UpdatedProgress, dice2);

        Assert.True(result1.Succeeded);
        Assert.True(result2.Succeeded);
        Assert.Equal(5, result2.UpdatedProgress.AccumulatedSuccesses);
        Assert.Equal(0, result2.UpdatedProgress.AttemptCount);
    }

    [Fact]
    public void ExactThresholdCompletionTransitionsToCompleted()
    {
        var definition = new WerewolfExtendedTestDefinition("req-1", 3, 6, 3);
        var progress = WerewolfExtendedTestService.CreateInitialProgress(definition);
        var diceValues = new[] { 8, 9, 10 };
        var result = WerewolfExtendedTestService.Advance(definition, progress, diceValues);

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.UpdatedProgress.AccumulatedSuccesses);
        Assert.Equal(WerewolfExtendedTestStatus.Completed, result.UpdatedProgress.Status);
    }

    [Fact]
    public void ExcessSuccessesBeyondThresholdKeepCompleted()
    {
        var definition = new WerewolfExtendedTestDefinition("req-1", 5, 6, 2);
        var progress = WerewolfExtendedTestService.CreateInitialProgress(definition);
        var diceValues = new[] { 8, 9, 10, 7, 6 };
        var result = WerewolfExtendedTestService.Advance(definition, progress, diceValues);

        Assert.True(result.Succeeded);
        Assert.Equal(5, result.UpdatedProgress.AccumulatedSuccesses);
        Assert.Equal(WerewolfExtendedTestStatus.Completed, result.UpdatedProgress.Status);
    }

    [Fact]
    public void OrdinaryFailureDoesNotAddSuccessesAndIncrementsAttempts()
    {
        var definition = new WerewolfExtendedTestDefinition("req-1", 3, 6, 5);
        var progress = WerewolfExtendedTestService.CreateInitialProgress(definition);
        var diceValues = new[] { 2, 3, 4 };
        var result = WerewolfExtendedTestService.Advance(definition, progress, diceValues);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.UpdatedProgress.AccumulatedSuccesses);
        Assert.Equal(1, result.UpdatedProgress.AttemptCount);
        Assert.False(result.UpdatedProgress.IsBotched);
        Assert.Equal(WerewolfExtendedTestStatus.InProgress, result.UpdatedProgress.Status);
    }

    [Fact]
    public void BotchSetsIsBotchedAndZerosAccumulatedSuccesses()
    {
        var definition = new WerewolfExtendedTestDefinition("req-1", 3, 6, 5);
        var progress = WerewolfExtendedTestService.CreateInitialProgress(definition);
        var dice1 = new[] { 8, 9, 10 };
        var partial = WerewolfExtendedTestService.Advance(definition, progress, dice1);
        var dice2 = new[] { 1, 2, 3 };
        var result = WerewolfExtendedTestService.Advance(definition, partial.UpdatedProgress, dice2);

        Assert.True(result.Succeeded);
        Assert.True(result.UpdatedProgress.IsBotched);
        Assert.Equal(WerewolfExtendedTestStatus.Botched, result.UpdatedProgress.Status);
        Assert.Equal(0, result.UpdatedProgress.AccumulatedSuccesses);
    }

    [Fact]
    public void InvalidRequiredSuccessesReturnsError()
    {
        var definition = new WerewolfExtendedTestDefinition("req-1", 3, 6, 0);
        var result = WerewolfExtendedTestService.CreateInitialProgress(definition);

        Assert.Equal(WerewolfExtendedTestStatus.Failed, result.Status);
    }

    [Fact]
    public void InvalidDifficultyReturnsError()
    {
        var definition = new WerewolfExtendedTestDefinition("req-1", 3, 1, 5);
        var progress = WerewolfExtendedTestService.CreateInitialProgress(definition);
        var diceValues = new[] { 8, 9, 10 };
        var result = WerewolfExtendedTestService.Advance(definition, progress, diceValues);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == "InvalidDifficulty");
    }

    [Fact]
    public void DifficultyAboveTenReturnsError()
    {
        var definition = new WerewolfExtendedTestDefinition("req-1", 3, 11, 5);
        var progress = WerewolfExtendedTestService.CreateInitialProgress(definition);
        var diceValues = new[] { 8, 9, 10 };
        var result = WerewolfExtendedTestService.Advance(definition, progress, diceValues);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == "InvalidDifficulty");
    }

    [Fact]
    public void NegativePoolReturnsError()
    {
        var definition = new WerewolfExtendedTestDefinition("req-1", -1, 6, 5);
        var progress = WerewolfExtendedTestService.CreateInitialProgress(definition);
        var diceValues = new[] { 8, 9, 10 };
        var result = WerewolfExtendedTestService.Advance(definition, progress, diceValues);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == "InvalidDicePool");
    }

    [Fact]
    public void AdvanceDoesNotMutatePriorProgress()
    {
        var definition = new WerewolfExtendedTestDefinition("req-1", 3, 6, 5);
        var progress = WerewolfExtendedTestService.CreateInitialProgress(definition);
        var originalProgress = progress;
        var diceValues = new[] { 8, 9, 10 };

        WerewolfExtendedTestService.Advance(definition, progress, diceValues);

        Assert.Equal(0, originalProgress.AccumulatedSuccesses);
        Assert.Equal(0, originalProgress.AttemptCount);
        Assert.Equal(WerewolfExtendedTestStatus.InProgress, originalProgress.Status);
    }
}
