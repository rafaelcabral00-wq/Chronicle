namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfCombatManeuver(
    string ManeuverId,
    string SourceLocator,
    string AttackAbility,
    int BaseDifficulty,
    int? DifficultyModifier,
    IReadOnlyList<string> AllowedForms,
    bool RequiresWeapon,
    string? WeaponAbility,
    string DamageExpression,
    string DamageCategory,
    int ActionCost,
    bool IsSpecial,
    bool RequiresPrerequisiteSuccess,
    string? PrerequisiteManeuverId,
    string Notes);
