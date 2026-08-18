namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfRecoverDamageResult(
    bool Succeeded,
    WerewolfRuntimeCharacterState? UpdatedState,
    WerewolfHealthTrack HealthTrack,
    IReadOnlyList<string> Findings,
    string? ErrorCode,
    string RequestId);
