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
    SpiritDetection,
    SpiritCommand,
    SpiritPossession,
    CharmActivation,
    UmbraCrossing,
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

public sealed record WerewolfExorcismBoundaryPayload(
    string GiftKey,
    string Mechanic,
    string TargetType,
    int RequiredConcentrationTurns,
    string ReluctantSpiritTest,
    string TrappedSpiritTest,
    string SourceLocator,
    string Note);

public sealed record WerewolfCharmStealBoundaryPayload(
    string GiftKey,
    string StolenCharmKey,
    int GnosisCostPerTurn,
    string SourceLocator,
    string Note);

public sealed record WerewolfCrossingModifierPayload(
    string GiftKey,
    int DifficultyModifier,
    bool AutomaticCrossing,
    bool NoFuryAllowed,
    string SourceLocator,
    string Note);

public sealed record WerewolfRemoteTransportBoundaryPayload(
    string GiftKey,
    string SourceSpiritReference,
    string TargetEntityReference,
    string CrossingResult,
    string TransportIntent,
    string DestinationSemantics,
    string ChronicleOrchestrationRequired,
    string SourceLocator,
    string Note);

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
