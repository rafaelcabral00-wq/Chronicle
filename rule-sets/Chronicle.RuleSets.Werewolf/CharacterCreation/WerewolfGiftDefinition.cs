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
    string SourceLocator,
    /// <summary>
    /// Set when the source defines this Gift as identical to another Gift
    /// (for example "Identico ao Dom dos hominideos"). The value is the
    /// canonical Gift key whose mechanic applies; it is never the Gift's own
    /// key. Null for Gifts the source defines in full, and for the two source
    /// variants that derive from another Gift but differ from it.
    /// </summary>
    string? AliasedGiftKey = null);
