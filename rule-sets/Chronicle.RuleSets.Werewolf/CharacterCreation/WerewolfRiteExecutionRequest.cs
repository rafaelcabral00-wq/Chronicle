namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfRiteExecutionRequest(
    string RequestId,
    string RiteKey,
    IReadOnlyList<int> DiceValues,
    bool HasTargetPiece);
