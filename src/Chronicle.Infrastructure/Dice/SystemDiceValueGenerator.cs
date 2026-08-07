using Chronicle.Contracts;

namespace Chronicle.Infrastructure;

public sealed class SystemDiceValueGenerator : IDiceValueGenerator
{
    public IReadOnlyList<int> Generate(int quantity, DiceSize diceSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 0);

        if (!SupportsSize(diceSize))
        {
            throw new ArgumentOutOfRangeException(nameof(diceSize), $"Dice size {diceSize} is not supported.");
        }

        var faces = (int)diceSize;
        var values = new int[quantity];
        var random = new Random();

        for (var i = 0; i < quantity; i++)
        {
            values[i] = random.Next(1, faces + 1);
        }

        return Array.AsReadOnly(values);
    }

    public bool SupportsSize(DiceSize diceSize)
    {
        return Enum.IsDefined(diceSize);
    }
}
