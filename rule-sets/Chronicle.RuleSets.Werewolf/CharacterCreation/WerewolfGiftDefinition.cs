namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfGiftCategory
{
    Breed,
    Auspice,
    Tribe
}

public enum WerewolfGiftActivationType
{
    Passive,
    Active,
    TestRequired
}

public enum WerewolfGiftCostType
{
    None,
    Rage,
    Gnosis,
    Willpower,
    Health
}

public enum WerewolfGiftDurationType
{
    Instant,
    Scene,
    Permanent,
    Turn
}

public sealed record WerewolfGiftDefinition(
    string GiftKey,
    string NameEn,
    string NamePtBr,
    int Level,
    WerewolfGiftCategory Category,
    string OwnerKey,
    WerewolfGiftActivationType ActivationType,
    WerewolfGiftCostType CostType,
    int CostAmount,
    string? TestAttribute,
    string? TestAbility,
    int? TestDifficulty,
    WerewolfGiftDurationType DurationType,
    int MaxUsesPerScene,
    string EffectDescriptionEn,
    string EffectDescriptionPtBr,
    string SourceLocator);
