namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfTotemCatalogEntry(
    string TotemKey,
    string CanonicalName,
    string NameEn,
    string NamePtBr,
    string SourceLocator,
    int? BackgroundCost,
    string? PatronTribeKey,
    IReadOnlyList<WerewolfTotemEffect> Effects);
