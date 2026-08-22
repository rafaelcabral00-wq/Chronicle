namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfConditionKind
{
    TemporaryPsychoticEpisode,
    Incapacitated,
    UnderTension,
    CriticalFailure
}

public sealed record WerewolfCondition(
    string ConditionKey,
    WerewolfConditionKind Kind,
    string SourceLocator,
    string SourceDeformity,
    int AppliedAtVersion,
    bool IsActive,
    int? DurationTurns = null);

public static class WerewolfConditionIdentifiers
{
    public const string TemporaryPsychoticEpisode = "condition.temporary-psychotic-episode";
    public const string Incapacitated = "condition.incapacitated";
    public const string UnderTension = "condition.under-tension";
    public const string CriticalFailure = "condition.critical-failure";
}
