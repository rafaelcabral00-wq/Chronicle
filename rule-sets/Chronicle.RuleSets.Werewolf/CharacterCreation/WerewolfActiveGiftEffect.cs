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
    RageRecoveryPenalty,
    ExtendedTestDifficultyModifier,
    ObjectTransformation,
    Custom
}

public sealed record WerewolfRageRecoveryPenaltyPayload(
    int PenaltyAmount,
    int? DurationTurns = null);

public sealed record WerewolfExtendedTestDifficultyPayload(
    int DifficultyIncrease,
    string Scope,
    int? DurationTurns = null);

public sealed record WerewolfObjectTransformationPayload(
    string TargetMaterial,
    IReadOnlyList<string> AllowedResultCategories,
    bool SupportsPermanentAlteration,
    bool SupportsAggravatedDamage,
    int? VariableDurationTurns = null);

public sealed record WerewolfActiveGiftEffect(
    string GiftKey,
    int StartedAtTurn,
    WerewolfGiftDurationType DurationType,
    int RemainingDuration,
    WerewolfActiveGiftEffectKind EffectKind,
    int Magnitude,
    string SourceLocator,
    string? SceneToken = null,
    object? Payload = null);
