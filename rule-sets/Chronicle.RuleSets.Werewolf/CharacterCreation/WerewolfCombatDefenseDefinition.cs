namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfCombatDefenseDefinition(
    string DefenseId,
    string SourceLocator,
    string AttributeId,
    string AbilityId,
    int BaseDifficulty,
    int? DifficultyModifier,
    bool IsEffectiveAgainstFirearms,
    string Notes);
