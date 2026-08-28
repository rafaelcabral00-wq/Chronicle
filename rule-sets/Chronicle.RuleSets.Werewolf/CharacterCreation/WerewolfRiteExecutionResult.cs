namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfRiteExecutionResult(
    bool Succeeded,
    IReadOnlyList<WerewolfRiteFinding> Findings,
    string RequestId,
    string RiteKey,
    int DicePool,
    int Difficulty,
    int SuccessCount,
    string InterpretationStatus,
    string? Effect,
    object? Payload = null);
