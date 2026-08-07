namespace Chronicle.Contracts;

public sealed record DiceRollResult(
    string RequestId,
    bool Succeeded,
    IReadOnlyList<int> DiceValues,
    DiceRollFailureCode? FailureCode,
    IReadOnlyDictionary<string, string>? Metadata);
