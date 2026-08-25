namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfRiteDefinition(
    string Key,
    string DisplayName,
    string Category,
    int Level,
    string AttributeId,
    string AbilityId,
    int BaseDifficulty,
    int? RequiredSuccesses,
    int? GnosisCost,
    int? RageCost,
    int? WillpowerCost,
    string TargetType,
    string EffectDescription,
    string SourceLocator);
