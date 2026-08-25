namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfTotemEffectKind
{
    TraitBonus,
    AbilityBonus,
    GiftGrant,
    ResourceGrant,
    DiceBonus,
    DifficultyModifier,
    AdditionalBeneficiary,
    CommunicationCapability,
    TrackingCapability,
    SpiritCapability,
    BanOrRestriction,
    ConditionalBenefit,
    PackWideBenefit,
    IndividualBenefit
}

public sealed record WerewolfTotemEffect(
    WerewolfTotemEffectKind Kind,
    string Payload,
    string SourceLocator,
    IReadOnlyList<string>? Dependencies = null);
