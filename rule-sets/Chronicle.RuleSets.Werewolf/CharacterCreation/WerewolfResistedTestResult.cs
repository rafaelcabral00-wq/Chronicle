namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfResistedTestWinner
{
    None,
    SideA,
    SideB,
    Tie
}

public sealed record WerewolfResistedTestResult(
    bool Succeeded,
    IReadOnlyList<WerewolfResistedTestFinding> Findings,
    string RequestId,
    int SideASuccesses,
    int SideBSuccesses,
    int NetSuccesses,
    WerewolfResistedTestWinner Winner,
    string Status,
    string SerializedResult);
