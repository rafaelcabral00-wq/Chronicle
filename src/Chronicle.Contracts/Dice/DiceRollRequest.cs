namespace Chronicle.Contracts;

public sealed record DiceRollRequest(
    string RequestId,
    int Quantity,
    DiceSize DiceSize,
    IReadOnlyDictionary<string, string>? Metadata);
