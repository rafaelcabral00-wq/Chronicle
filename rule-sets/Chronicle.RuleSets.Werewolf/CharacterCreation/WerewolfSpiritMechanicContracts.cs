namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum SpiritMechanicFindingSeverity
{
    Information,
    Error
}

public sealed record SpiritMechanicFinding(
    SpiritMechanicFindingSeverity Severity,
    SpiritMechanicErrorCode Code,
    string Message);

public enum SpiritMechanicErrorCode
{
    Succeeded,
    MissingState,
    InvalidSpiritId,
    InvalidCategory,
    InvalidTraitValue,
    InvalidGauntletValue,
    InvalidDifficulty,
    InvalidDiceInput,
    InvalidCharmKey,
    InvalidSilverCount,
    InvalidWillpowerValue,
    CrossingSuccess,
    CrossingFailure,
    CrossingBotch,
    CrossingZeroSuccessWait,
    CrossingInstant,
    CrossingThirtySeconds,
    CrossingFiveMinutes,
    CrossingRetryBlocked,
    CrossingFuryRestricted,
    DetectionAutomatic,
    DetectionSuccess,
    DetectionFailure,
    MaterializationSuccess,
    MaterializationFailure,
    MaterializationInsufficientGnosis,
    EssenceInsufficient,
    EssenceDepleted,
    CharmNotKnown,
    CharmExecutionSuccess,
    CharmExecutionFailure,
    CommandSuccess,
    CommandFailure,
    PossessionSuccess,
    PossessionFailure,
    DamageApplied,
    DamageAbsorbed,
    InvalidRequest,
    StaleStateVersion
}

public sealed record SpiritMechanicRequest(
    WerewolfSpiritRuntimeState CurrentState,
    int ExpectedStateVersion,
    string RequestId);

public sealed record SpiritMechanicResult(
    bool Succeeded,
    WerewolfSpiritRuntimeState? NewState,
    IReadOnlyList<SpiritMechanicFinding> Findings,
    string? RequestId,
    int? NewStateVersion);

public sealed record CrossingRequest(
    WerewolfSpiritRuntimeState CurrentState,
    int ExpectedStateVersion,
    string RequestId,
    int GauntletValue,
    int GnosisPool,
    int Difficulty,
    bool HasReflectiveSurface,
    int SilverItemCount,
    bool IsFuryGrantedAction,
    int PreviousFailedAttempts,
    IReadOnlyList<int> DiceValues);

public sealed record CrossingResult(
    bool Succeeded,
    WerewolfSpiritRuntimeState? NewState,
    IReadOnlyList<SpiritMechanicFinding> Findings,
    string? RequestId,
    int? NewStateVersion,
    int Successes,
    bool IsBotch,
    bool IsZeroSuccessWait,
    bool IsFuryRestricted,
    CrossingTime CrossingTime,
    int EffectiveGnosis,
    int EffectiveDifficulty,
    bool CanRetry,
    int NextRetryDifficultyModifier);

public enum CrossingTime
{
    Instant,
    ThirtySeconds,
    FiveMinutes,
    CannotRetry
}

public sealed record MovementRequest(
    WerewolfSpiritRuntimeState CurrentState,
    int ExpectedStateVersion,
    string RequestId);

public sealed record MovementResult(
    bool Succeeded,
    WerewolfSpiritRuntimeState? NewState,
    IReadOnlyList<SpiritMechanicFinding> Findings,
    string? RequestId,
    int? NewStateVersion,
    int MaxMetersPerTurn);

public sealed record DetectionRequest(
    WerewolfSpiritRuntimeState CurrentState,
    int ExpectedStateVersion,
    string RequestId,
    int GauntletValue,
    int GnosisPool,
    int Difficulty,
    IReadOnlyList<int> DiceValues);

public sealed record DetectionResult(
    bool Succeeded,
    WerewolfSpiritRuntimeState? NewState,
    IReadOnlyList<SpiritMechanicFinding> Findings,
    string? RequestId,
    int? NewStateVersion,
    bool IsAutomatic,
    bool IsDetected,
    int Successes);

public sealed record MaterializationRequest(
    WerewolfSpiritRuntimeState CurrentState,
    int ExpectedStateVersion,
    string RequestId,
    int GauntletValue);

public sealed record MaterializationResult(
    bool Succeeded,
    WerewolfSpiritRuntimeState? NewState,
    IReadOnlyList<SpiritMechanicFinding> Findings,
    string? RequestId,
    int? NewStateVersion,
    bool CanMaterialize,
    bool IsNowMaterialized);

public sealed record EssenceSpendRequest(
    WerewolfSpiritRuntimeState CurrentState,
    int ExpectedStateVersion,
    string RequestId,
    int Amount);

public sealed record EssenceSpendResult(
    bool Succeeded,
    WerewolfSpiritRuntimeState? NewState,
    IReadOnlyList<SpiritMechanicFinding> Findings,
    string? RequestId,
    int? NewStateVersion,
    int PreviousEssence,
    int NewEssence);

public sealed record CharmExecutionRequest(
    WerewolfSpiritRuntimeState CurrentState,
    int ExpectedStateVersion,
    string RequestId,
    string CharmKey,
    int? GnosisCost,
    int? EssenceCost);

public sealed record CharmExecutionResult(
    bool Succeeded,
    WerewolfSpiritRuntimeState? NewState,
    IReadOnlyList<SpiritMechanicFinding> Findings,
    string? RequestId,
    int? NewStateVersion,
    string? ExecutedCharmKey,
    string? EffectDescription);

public sealed record CommandRequest(
    WerewolfSpiritRuntimeState CurrentState,
    int ExpectedStateVersion,
    string RequestId,
    int Charisma,
    int Leadership,
    int TargetWillpower,
    IReadOnlyList<int> DiceValues);

public sealed record CommandResult(
    bool Succeeded,
    WerewolfSpiritRuntimeState? NewState,
    IReadOnlyList<SpiritMechanicFinding> Findings,
    string? RequestId,
    int? NewStateVersion,
    int Successes,
    bool IsCommanded);

public sealed record PossessionRequest(
    WerewolfSpiritRuntimeState CurrentState,
    int ExpectedStateVersion,
    string RequestId,
    int TargetWillpower,
    IReadOnlyList<int> DiceValues);

public sealed record PossessionResult(
    bool Succeeded,
    WerewolfSpiritRuntimeState? NewState,
    IReadOnlyList<SpiritMechanicFinding> Findings,
    string? RequestId,
    int? NewStateVersion,
    int Successes,
    bool IsPossessing,
    PossessionDuration Duration);

public enum PossessionDuration
{
    None,
    Instant,
    FiveMinutes,
    FifteenMinutes,
    OneHour,
    ThreeHours,
    SixHours
}

public sealed record SpiritDamageRequest(
    WerewolfSpiritRuntimeState CurrentState,
    int ExpectedStateVersion,
    string RequestId,
    int DamageAmount,
    int Difficulty,
    bool IsAggravated);

public sealed record SpiritDamageResult(
    bool Succeeded,
    WerewolfSpiritRuntimeState? NewState,
    IReadOnlyList<SpiritMechanicFinding> Findings,
    string? RequestId,
    int? NewStateVersion,
    int DamageApplied,
    int EssenceLost,
    bool IsAtDeathBoundary);
