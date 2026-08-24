namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfActiveGiftEffectKind
{
    SocialTestBonus,
    SocialIntimidationBonus,
    CombatDamageBonus,
    DefenseBonus,
    InitiativeBonus,
    HealthLevelRepair,
    DamageReduction,
    PerceptionBonus,
    StealthBonus,
    MovementBonus,
    SpiritCommunication,
    WyrmSense,
    FormDetection,
    AnimalCommunication,
    MachineControl,
    LockOpening,
    ElementalCreation,
    FearAura,
    PoisonImmunity,
    FireImmunity,
    SensoryEnhancement,
    WoundPenaltyRemoval,
    ProneCondition,
    RestrainedCondition,
    MentalTestBonus,
    WindEffect,
    MagicDetection,
    AuraBlocking,
    LightEffect,
    TestBonus,
    Custom
}

public sealed record WerewolfActiveGiftEffect(
    string GiftKey,
    int StartedAtTurn,
    WerewolfGiftDurationType DurationType,
    int RemainingDuration,
    WerewolfActiveGiftEffectKind EffectKind,
    int Magnitude,
    string SourceLocator,
    string? SceneToken = null);
