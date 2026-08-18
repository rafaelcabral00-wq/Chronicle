namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfRecoverDamageRequest(
    string RequestId,
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    WerewolfDamageCategory DamageType,
    int Amount,
    bool RequiresAlternateFormRest = false);
