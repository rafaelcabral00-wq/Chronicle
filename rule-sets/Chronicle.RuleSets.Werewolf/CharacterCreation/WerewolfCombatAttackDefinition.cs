namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfCombatAttackDefinition(
    string AttackId,
    string SourceLocator,
    string AttributeId,
    string AbilityId,
    int BaseDifficulty,
    int? DifficultyModifier,
    string DamageExpression,
    string DamageCategory,
    bool RequiresForm,
    IReadOnlyList<string> AllowedForms,
    bool IsNaturalWeapon,
    string? NaturalWeaponTarget,
    int ActionCost,
    string? DamageType = null,
    string? Notes = null);
