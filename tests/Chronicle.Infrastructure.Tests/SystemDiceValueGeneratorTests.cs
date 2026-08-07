using Chronicle.Contracts;
using Xunit;

namespace Chronicle.Infrastructure.Tests;

public sealed class SystemDiceValueGeneratorTests
{
    [Fact]
    public void GenerateProducesRequestedQuantity()
    {
        var generator = new SystemDiceValueGenerator();
        var result = generator.Generate(5, DiceSize.D10);
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void GenerateProducesFacesWithinBounds()
    {
        var generator = new SystemDiceValueGenerator();
        var result = generator.Generate(100, DiceSize.D10);
        Assert.All(result, die => Assert.InRange(die, 1, 10));
    }

    [Fact]
    public void SupportsSizeReturnsTrueForDefinedDice()
    {
        var generator = new SystemDiceValueGenerator();
        Assert.True(generator.SupportsSize(DiceSize.D4));
        Assert.True(generator.SupportsSize(DiceSize.D10));
        Assert.True(generator.SupportsSize(DiceSize.D20));
    }

    [Fact]
    public void GenerateThrowsForNegativeQuantity()
    {
        var generator = new SystemDiceValueGenerator();
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Generate(-1, DiceSize.D10));
    }

    [Fact]
    public void GenerateThrowsForUnsupportedSize()
    {
        var generator = new SystemDiceValueGenerator();
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Generate(1, (DiceSize)999));
    }
}
