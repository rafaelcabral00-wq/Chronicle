namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfApplyDamageResult(
    bool Succeeded,
    WerewolfRuntimeCharacterState? UpdatedState,
    IReadOnlyList<WerewolfHealthLevelDefinition> HealthLevelDefinitions,
    WerewolfHealthTrack HealthTrack,
    IReadOnlyList<string> Findings,
    string? ErrorCode,
    string RequestId);
