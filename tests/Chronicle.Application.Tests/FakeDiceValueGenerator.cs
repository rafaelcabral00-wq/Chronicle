using Chronicle.Contracts;

namespace Chronicle.Application.Tests;

public sealed class FakeDiceValueGenerator : IDiceValueGenerator
{
    private readonly Queue<int> _values;
    private readonly DiceSize _fixedSize;

    public FakeDiceValueGenerator(IEnumerable<int> values, DiceSize fixedSize)
    {
        ArgumentNullException.ThrowIfNull(values);
        _values = new Queue<int>(values);
        _fixedSize = fixedSize;
    }

    public IReadOnlyList<int> Generate(int quantity, DiceSize diceSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 0);

        if (!Enum.IsDefined(diceSize))
        {
            throw new ArgumentOutOfRangeException(nameof(diceSize));
        }

        if (!Enum.Equals(diceSize, _fixedSize))
        {
            throw new ArgumentException($"Fake generator is fixed to {_fixedSize}. Requested {diceSize}.", nameof(diceSize));
        }

        if (_values.Count < quantity)
        {
            throw new InvalidOperationException($"Fake dice generator has {_values.Count} values remaining but {quantity} were requested.");
        }

        var result = new int[quantity];
        for (var i = 0; i < quantity; i++)
        {
            result[i] = _values.Dequeue();
        }

        return Array.AsReadOnly(result);
    }

    public bool SupportsSize(DiceSize diceSize)
    {
        return Enum.IsDefined(diceSize);
    }
}
