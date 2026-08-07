using Chronicle.Contracts;

namespace Chronicle.Infrastructure;

public sealed class ScriptedDiceValueGenerator : IDiceValueGenerator
{
    private readonly Queue<int> _scriptedValues;
    private readonly DiceSize _fixedSize;

    public ScriptedDiceValueGenerator(IEnumerable<int> scriptedValues, DiceSize fixedSize)
    {
        ArgumentNullException.ThrowIfNull(scriptedValues);

        _scriptedValues = new Queue<int>(scriptedValues);
        _fixedSize = fixedSize;

        if (!SupportsSize(_fixedSize))
        {
            throw new ArgumentOutOfRangeException(nameof(fixedSize), $"Dice size {fixedSize} is not supported.");
        }
    }

    public IReadOnlyList<int> Generate(int quantity, DiceSize diceSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 0);

        if (!SupportsSize(diceSize))
        {
            throw new ArgumentOutOfRangeException(nameof(diceSize), $"Dice size {diceSize} is not supported.");
        }

        if (!Enum.Equals(diceSize, _fixedSize))
        {
            throw new ArgumentException($"Scripted generator is fixed to {_fixedSize}. Requested {diceSize}.", nameof(diceSize));
        }

        if (_scriptedValues.Count < quantity)
        {
            throw new InvalidOperationException($"Scripted dice generator has {_scriptedValues.Count} values remaining but {quantity} were requested.");
        }

        var values = new int[quantity];
        for (var i = 0; i < quantity; i++)
        {
            values[i] = _scriptedValues.Dequeue();
        }

        return Array.AsReadOnly(values);
    }

    public bool SupportsSize(DiceSize diceSize)
    {
        return Enum.IsDefined(diceSize);
    }
}
