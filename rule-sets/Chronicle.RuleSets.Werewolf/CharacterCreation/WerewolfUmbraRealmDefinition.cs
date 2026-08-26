namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfUmbraRealmDefinition(
    string RealmKey,
    string CanonicalName,
    string NameEn,
    string SourceLocator,
    string? RealmType = null,
    string? Relationship = null);
