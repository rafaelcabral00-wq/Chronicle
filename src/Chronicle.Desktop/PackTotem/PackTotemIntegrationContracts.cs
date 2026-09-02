using Chronicle.Application.PackTotem;

namespace Chronicle.Desktop.PackTotem;

public enum PackTotemIntegrationOutcome
{
    Bound,
    AggregateNotFound,
    AggregateInvariantViolated,
    BoundarySignalNotReceived,
    ChronicleMutationFailed
}

public sealed record IntegratePackTotemBindingRequest(
    Guid PackIdAggregateId,
    string PackId,
    string TotemId,
    int TotemRating,
    int TotemAggregation,
    IReadOnlyList<string> InitialImprovementPurchases,
    string PackageId,
    string PackageVersion,
    string OperationKey,
    string ExpectedRiteKey,
    IReadOnlyList<int> DiceValues,
    bool HasTargetPiece,
    string RequestId);

public sealed record PackTotemIntegrationResult(
    bool WerewolfSucceeded,
    bool ChronicleMutationSucceeded,
    PackTotemIntegrationOutcome Outcome,
    PackTotemBoundaryValidationResult BoundaryValidation,
    PackTotemOperationResult? AggregateResult,
    string? FailureReason);
