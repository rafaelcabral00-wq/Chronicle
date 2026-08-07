using Chronicle.Contracts;
using Xunit;

namespace Chronicle.Infrastructure.Tests;

public sealed class ScriptedDiceValueGeneratorTests
{
    [Fact]
    public void GenerateReturnsScriptedValuesInOrder()
    {
        var generator = new ScriptedDiceValueGenerator([1, 5, 10], DiceSize.D10);
        var result = generator.Generate(3, DiceSize.D10);
        Assert.Equal([1, 5, 10], result);
    }

    [Fact]
    public void GenerateThrowsWhenInsufficientScriptedValues()
    {
        var generator = new ScriptedDiceValueGenerator([1, 2], DiceSize.D10);
        Assert.Throws<InvalidOperationException>(() => generator.Generate(3, DiceSize.D10));
    }

    [Fact]
    public void GenerateThrowsWhenRequestedSizeDiffersFromFixedSize()
    {
        var generator = new ScriptedDiceValueGenerator([1, 2, 3], DiceSize.D10);
        Assert.Throws<ArgumentException>(() => generator.Generate(2, DiceSize.D20));
    }

    [Fact]
    public void SupportsSizeReturnsTrueForDefinedDice()
    {
        var generator = new ScriptedDiceValueGenerator([1], DiceSize.D10);
        Assert.True(generator.SupportsSize(DiceSize.D10));
        Assert.True(generator.SupportsSize(DiceSize.D4));
    }

    [Fact]
    public void GenerateThrowsForNegativeQuantity()
    {
        var generator = new ScriptedDiceValueGenerator([1], DiceSize.D10);
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Generate(-1, DiceSize.D10));
    }

    [Fact]
    public void ConstructorThrowsForUnsupportedFixedSize()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ScriptedDiceValueGenerator([1], (DiceSize)999));
    }
}
