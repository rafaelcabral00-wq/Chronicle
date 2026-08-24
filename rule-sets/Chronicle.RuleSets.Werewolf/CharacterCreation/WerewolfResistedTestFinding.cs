namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfResistedTestFindingSeverity
{
    Information,
    Error
}

public sealed record WerewolfResistedTestFinding(
    WerewolfResistedTestFindingSeverity Severity,
    string Code,
    string Message);
