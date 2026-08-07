using Chronicle.Contracts;
using Xunit;

namespace Chronicle.Contracts.Tests;

public sealed class DiceContractTests
{
    [Fact]
    public void DiceSizeEnumContainsExpectedValues()
    {
        Assert.Equal(4, (int)DiceSize.D4);
        Assert.Equal(10, (int)DiceSize.D10);
        Assert.Equal(20, (int)DiceSize.D20);
    }

    [Fact]
    public void DiceRollRequestRequiresValidFields()
    {
        var request = new DiceRollRequest("req-1", 5, DiceSize.D10, null);
        Assert.Equal("req-1", request.RequestId);
        Assert.Equal(5, request.Quantity);
        Assert.Equal(DiceSize.D10, request.DiceSize);
        Assert.Null(request.Metadata);
    }

    [Fact]
    public void DiceRollResultCapturesSuccessAndValues()
    {
        var result = new DiceRollResult("req-1", true, [1, 5, 10], null, null);
        Assert.True(result.Succeeded);
        Assert.Equal(3, result.DiceValues.Count);
        Assert.Equal(1, result.DiceValues[0]);
        Assert.Equal(10, result.DiceValues[2]);
        Assert.Null(result.FailureCode);
    }

    [Fact]
    public void DiceRollResultCapturesFailure()
    {
        var result = new DiceRollResult("req-1", false, [], DiceRollFailureCode.InvalidQuantity, null);
        Assert.False(result.Succeeded);
        Assert.Empty(result.DiceValues);
        Assert.Equal(DiceRollFailureCode.InvalidQuantity, result.FailureCode);
    }

    [Theory]
    [InlineData(DiceRollFailureCode.InvalidQuantity)]
    [InlineData(DiceRollFailureCode.InvalidFaces)]
    [InlineData(DiceRollFailureCode.GeneratorUnavailable)]
    public void DiceRollFailureCodeHasExpectedMembers(DiceRollFailureCode code)
    {
        Assert.True(Enum.IsDefined((DiceRollFailureCode)code));
    }
}
