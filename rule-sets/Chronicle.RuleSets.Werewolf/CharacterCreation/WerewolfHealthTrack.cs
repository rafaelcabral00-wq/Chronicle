namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfHealthState
{
    Healthy,
    Wounded,
    Incapacitated,
    Unconscious,
    NearDeath,
    Survived,
    Dead
}

public sealed record WerewolfHealthTrack(
    IReadOnlyList<WerewolfDamageMark> DamageMarks,
    int BashingCount,
    int LethalCount,
    int AggravatedCount,
    int TotalDamage,
    WerewolfHealthState HealthState,
    WerewolfDamageCategory? FatalDamageType,
    WerewolfHealthLevelName CurrentLevel,
    int WoundPenalty,
    string? AmbiguityNote,
    bool HasWeakenedImmuneSystem = false,
    bool PermanecerAtivoAttempted = false,
    int LastRegenerationTurn = -1);
