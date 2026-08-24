namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfApplyDamageRequest(
    string RequestId,
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    WerewolfDamageCategory DamageType,
    int Amount,
    bool IsPoison = false);
