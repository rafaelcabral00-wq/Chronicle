namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfFrenzyType
{
    None,
    Wild,
    Fox,
    Extreme
}

public sealed record WerewolfFrenzyState(
    bool IsInFrenzy,
    WerewolfFrenzyType FrenzyType,
    string Trigger,
    int AccumulatedSuccesses,
    int StartedAtTurn,
    string? TargetRestriction,
    bool IsSuppressed,
    string SourceLocator);
