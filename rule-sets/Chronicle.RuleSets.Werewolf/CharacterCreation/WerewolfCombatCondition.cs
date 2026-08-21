namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfCombatConditionKind
{
    Healthy,
    Wounded,
    Incapacitated,
    Unconscious,
    NearDeath,
    Dead,
    Prone,
    Grappled,
    Immobilized,
    Stunned,
    Blinded,
    SilverContact,
    Fall,
    ChangeAction
}

public sealed record WerewolfCombatCondition(
    WerewolfCombatConditionKind Kind,
    string SourceLocator,
    string Notes,
    int? DifficultyModifier = null,
    bool? CanDodge = null,
    bool? CanParry = null,
    bool? CanBlock = null,
    bool? CanAct = null);

public static class WerewolfCombatConditionCatalog
{
    public static IReadOnlyList<WerewolfCombatCondition> Entries { get; } =
    [
        new WerewolfCombatCondition(WerewolfCombatConditionKind.Healthy, "Line 2860", "No damage"),
        new WerewolfCombatCondition(WerewolfCombatConditionKind.Wounded, "Lines 2866-2869", "1-5 damage levels"),
        new WerewolfCombatCondition(WerewolfCombatConditionKind.Incapacitated, "Line 2866", "6 damage levels"),
        new WerewolfCombatCondition(WerewolfCombatConditionKind.Unconscious, "Line 2866", "Exceeded Incapacitado with bashing"),
        new WerewolfCombatCondition(WerewolfCombatConditionKind.NearDeath, "Line 2867", "Exceeded Incapacitado with lethal"),
        new WerewolfCombatCondition(WerewolfCombatConditionKind.Dead, "Line 2868", "Exceeded Incapacitado with aggravated or lethal >= 8"),
        new WerewolfCombatCondition(WerewolfCombatConditionKind.Prone, "Lines 3110-3111", "Knocked down; requires actions to stand"),
        new WerewolfCombatCondition(WerewolfCombatConditionKind.Grappled, "Lines 3144-3147", "Grappled or keyed"),
        new WerewolfCombatCondition(WerewolfCombatConditionKind.Immobilized, "Line 3109", "Totally immobile; automatic failure on actions"),
        new WerewolfCombatCondition(WerewolfCombatConditionKind.Stunned, "Line 3111", "More damage than Stamina in one turn; no actions except stagger, +2 difficulty to received attacks next turn"),
        new WerewolfCombatCondition(WerewolfCombatConditionKind.Blinded, "Line 3107", "Cannot dodge/parry/block, +2 difficulty to all actions"),
        new WerewolfCombatCondition(WerewolfCombatConditionKind.SilverContact, "Lines 2895-2897", "Silver contact causes aggravated damage per turn")
    ];
}
