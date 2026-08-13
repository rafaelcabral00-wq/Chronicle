namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfRenownTransitionFindingSeverity
{
    Information,
    Error
}

public sealed record WerewolfRenownTransitionFinding(
    WerewolfRenownTransitionFindingSeverity Severity,
    WerewolfRenownTransitionErrorCode Code,
    string Message);

public sealed record WerewolfRenownTransitionRequest(
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    string RequestId,
    string RenownId,
    int Amount,
    bool IsPermanent);

public sealed record WerewolfRenownTransitionResult(
    bool Succeeded,
    WerewolfRuntimeCharacterState? NewState,
    IReadOnlyList<WerewolfRenownTransitionFinding> Findings,
    string? RequestId,
    int? NewRuntimeStateVersion,
    int? PreviousCurrent,
    int? NewCurrent,
    int? PreviousPermanent,
    int? NewPermanent);

public enum WerewolfRenownTransitionErrorCode
{
    RenownAwarded,
    RenownLost,
    RenownConverted,
    MissingState,
    InvalidPackageBinding,
    CharacterNotCompleted,
    StaleRuntimeStateVersion,
    UnknownRenown,
    MalformedRenownIdentifier,
    AmountMissingOrZero,
    AmountNegative,
    InsufficientCurrentValue,
    ConversionBelowThreshold,
    PermanentLossUnsupported
}
