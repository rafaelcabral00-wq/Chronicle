namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfRiteFinding(
    string Code,
    string Message,
    WerewolfRiteFindingSeverity Severity);

public enum WerewolfRiteFindingSeverity
{
    Information,
    Error
}
