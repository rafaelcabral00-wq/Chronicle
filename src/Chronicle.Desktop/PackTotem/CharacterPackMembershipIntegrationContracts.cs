using Chronicle.Application.PackTotem;

namespace Chronicle.Desktop.PackTotem;

public enum CharacterPackMembershipIntegrationOutcome
{
    MemberAdded,
    MemberRemoved,
    PackNotFound,
    AggregateInvariantViolated,
    InvalidCharacterIdentity,
    ChronicleMutationFailed
}

public sealed record IntegrateCharacterPackMembershipRequest(
    string PackId,
    string CharacterId,
    string OperationKey,
    string PackageId,
    string PackageVersion,
    string RequestId);

public sealed record CharacterPackMembershipIntegrationResult(
    bool ChronicleMutationSucceeded,
    CharacterPackMembershipIntegrationOutcome Outcome,
    PackTotemOperationResult? AggregateResult,
    string? FailureReason);