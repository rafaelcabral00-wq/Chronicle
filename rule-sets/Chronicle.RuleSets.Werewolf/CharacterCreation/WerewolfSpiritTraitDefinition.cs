namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfSpiritTraitDefinition(
    string TraitKey,
    string CanonicalName,
    string NameEn,
    string SourceLocator,
    string? Formula = null,
    string? Notes = null);
