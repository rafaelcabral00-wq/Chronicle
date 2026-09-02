namespace Chronicle.Desktop.PackTotem;

public sealed record ValidatePackTotemBoundaryRequest(
    string PackageId,
    string PackageVersion,
    string OperationKey,
    string ExpectedRiteKey,
    IReadOnlyList<int> DiceValues,
    bool HasTargetPiece,
    string RequestId);

public sealed record PackTotemBoundaryValidationResult(
    bool Succeeded,
    string? FailureReason,
    PackTotemBoundaryValidationKind Kind,
    PackTotemBoundaryS4Observation? S4Observation);

public enum PackTotemBoundaryValidationKind
{
    BoundarySignalReceived,
    RiteTestFailed,
    WrongPayloadType,
    InvalidRiteKey,
    UnregisteredRuntime,
    UndeclaredOperation
}

public sealed record PackTotemBoundaryS4Observation(
    string ObservedRiteKey,
    string SourceLocator,
    string Note,
    string PayloadTotemId,
    string PayloadPackId,
    IReadOnlyList<string> PayloadMemberRoster,
    int PayloadTotemAggregation,
    int SuccessCount,
    int? Difficulty,
    string InterpretationStatus,
    string? Effect);
