using Chronicle.Contracts;
using Xunit;

namespace Chronicle.Application.Tests;

public sealed class DiceRollServiceTests
{
    [Fact]
    public void ExecuteReturnsSuccessWithGeneratedDice()
    {
        var generator = new FakeDiceValueGenerator([3, 7, 10], DiceSize.D10);
        var request = new DiceRollRequest("req-1", 3, DiceSize.D10, null);
        var result = DiceRollService.Execute(request, generator);

        Assert.True(result.Succeeded);
        Assert.Equal("req-1", result.RequestId);
        Assert.Equal([3, 7, 10], result.DiceValues);
        Assert.Null(result.FailureCode);
    }

    [Fact]
    public void ExecuteReturnsFailureForInvalidQuantity()
    {
        var generator = new FakeDiceValueGenerator([1], DiceSize.D10);
        var request = new DiceRollRequest("req-1", -1, DiceSize.D10, null);
        var result = DiceRollService.Execute(request, generator);

        Assert.False(result.Succeeded);
        Assert.Equal(DiceRollFailureCode.InvalidQuantity, result.FailureCode);
    }

    [Fact]
    public void ExecuteReturnsFailureForUnsupportedDiceSize()
    {
        var generator = new FakeDiceValueGenerator([1], DiceSize.D10);
        var request = new DiceRollRequest("req-1", 1, (DiceSize)999, null);
        var result = DiceRollService.Execute(request, generator);

        Assert.False(result.Succeeded);
        Assert.Equal(DiceRollFailureCode.InvalidFaces, result.FailureCode);
    }

    [Fact]
    public void ExecuteReturnsFailureForEmptyRequestId()
    {
        var generator = new FakeDiceValueGenerator([1], DiceSize.D10);
        var request = new DiceRollRequest(string.Empty, 1, DiceSize.D10, null);
        var result = DiceRollService.Execute(request, generator);

        Assert.False(result.Succeeded);
        Assert.Equal(DiceRollFailureCode.InvalidQuantity, result.FailureCode);
    }

    [Fact]
    public void ExecutePreservesMetadata()
    {
        var generator = new FakeDiceValueGenerator([5], DiceSize.D10);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["source"] = "test"
        };
        var request = new DiceRollRequest("req-1", 1, DiceSize.D10, metadata);
        var result = DiceRollService.Execute(request, generator);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Metadata);
        Assert.Equal("test", result.Metadata!["source"]);
    }
}
