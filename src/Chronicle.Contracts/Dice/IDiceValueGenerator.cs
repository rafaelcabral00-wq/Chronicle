namespace Chronicle.Contracts;

public interface IDiceValueGenerator
{
    IReadOnlyList<int> Generate(int quantity, DiceSize diceSize);

    bool SupportsSize(DiceSize diceSize);
}
