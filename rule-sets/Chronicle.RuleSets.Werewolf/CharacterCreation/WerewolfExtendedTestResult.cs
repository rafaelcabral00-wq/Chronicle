namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfExtendedTestResult(
    bool Succeeded,
    IReadOnlyList<WerewolfExtendedTestFinding> Findings,
    string RequestId,
    WerewolfExtendedTestProgress UpdatedProgress,
    WerewolfExtendedTestStatus Status,
    string SerializedResult);
