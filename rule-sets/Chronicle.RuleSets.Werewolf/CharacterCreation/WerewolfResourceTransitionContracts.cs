namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfResourceTransitionFindingSeverity
{
    Information,
    Error
}

public sealed record WerewolfResourceTransitionFinding(
    WerewolfResourceTransitionFindingSeverity Severity,
    WerewolfResourceTransitionErrorCode Code,
    string Message);

public sealed record WerewolfResourceTransitionRequest(
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    string RequestId,
    string ResourceId,
    int Amount);

public sealed record WerewolfResourceTransitionResult(
    bool Succeeded,
    WerewolfRuntimeCharacterState? NewState,
    IReadOnlyList<WerewolfResourceTransitionFinding> Findings,
    string? RequestId,
    int? NewRuntimeStateVersion,
    int? PreviousCurrent,
    int? NewCurrent,
    int? PreviousPermanent,
    int? NewPermanent);

public enum WerewolfResourceTransitionErrorCode
{
    ResourceSpendSucceeded,
    ResourceRecoverSucceeded,
    MissingState,
    InvalidPackageBinding,
    CharacterNotCompleted,
    StaleRuntimeStateVersion,
    UnknownResource,
    MalformedResourceIdentifier,
    PermanentResourceMutationUnsupported,
    AmountMissingOrZero,
    AmountNegative,
    InsufficientCurrentValue,
    RecoveryExceedsPermanent,
    InvalidSourceCurrentAbovePermanent
}
