namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfSpiritCategoryDefinition(
    string SpiritCategoryKey,
    string CanonicalName,
    string NameEn,
    string SourceLocator,
    string? HierarchyRelationship = null);
