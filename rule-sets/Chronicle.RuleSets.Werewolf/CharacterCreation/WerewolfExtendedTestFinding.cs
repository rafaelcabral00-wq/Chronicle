namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfExtendedTestFindingSeverity
{
    Information,
    Error
}

public sealed record WerewolfExtendedTestFinding(
    WerewolfExtendedTestFindingSeverity Severity,
    string Code,
    string Message);
