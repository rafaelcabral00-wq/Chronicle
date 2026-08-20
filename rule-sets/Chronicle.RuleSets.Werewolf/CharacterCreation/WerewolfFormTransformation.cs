namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfFormTransformationRequest(
    string RequestId,
    WerewolfRuntimeCharacterState? CurrentState,
    int ExpectedStateVersion,
    string TargetFormId,
    bool SpendRage);

public sealed record WerewolfFormTransformationResult(
    bool Succeeded,
    WerewolfRuntimeCharacterState? UpdatedState,
    IReadOnlyList<WerewolfFormTransformationFinding> Findings,
    int? RemainingRage,
    int? NewStateVersion);

public sealed record WerewolfFormTransformationFinding(
    WerewolfFormTransformationFindingSeverity Severity,
    WerewolfFormTransformationErrorCode Code,
    string Message);

public enum WerewolfFormTransformationFindingSeverity
{
    Information,
    Error
}

public enum WerewolfFormTransformationErrorCode
{
    TransformationSucceeded,
    InvalidTargetForm,
    SameForm,
    InsufficientRage,
    StaleStateVersion,
    MissingState,
    InvalidDistance,
    BotchConsequence
}
