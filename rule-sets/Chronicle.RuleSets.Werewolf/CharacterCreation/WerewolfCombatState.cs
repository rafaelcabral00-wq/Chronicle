namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfCombatState(
    int CombatStateVersion,
    int InitiativeDicePool,
    int InitiativeScore,
    int ExtraActionsAvailable,
    int RageInvestedInInitiative,
    bool RageInvestedInTransformation,
    bool RageInvestedInPainNegation,
    IReadOnlyList<WerewolfCombatCondition> ActiveConditions,
    int CurrentActionCount,
    int TurnNumber,
    string? CurrentManeuverId = null,
    string? CurrentAttackId = null,
    string? CurrentDefenseId = null)
{
    public static WerewolfCombatState Initial(int initiativeDicePool) => new(
        1,
        initiativeDicePool,
        0,
        0,
        0,
        false,
        false,
        [],
        0,
        0);
}
