namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfSpiritCharmType
{
    Common,
    Special,
    Bane,
    Weaver,
    Wyld
}

public sealed record WerewolfSpiritCharmDefinition(
    string CharmKey,
    string CanonicalName,
    string NameEn,
    WerewolfSpiritCharmType CharmType,
    string SourceLocator,
    string? EffectSummary = null);
