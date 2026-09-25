using Chronicle.Application.PackTotem;

namespace Chronicle.Desktop.PackTotem;

public enum CharacterPackMembershipProjectionOutcome
{
    MemberFound,
    NotAMember,
    InvalidCharacterIdentity,
    ChronicleReadFailed
}

public sealed record QueryCharacterPackMembershipRequest(
    string CharacterId,
    string RequestId);

public sealed record CharacterPackMembershipProjectionResult(
    bool Success,
    CharacterPackMembershipProjectionOutcome Outcome,
    string? PackId,
    string? PackName,
    string? LeaderId,
    bool IsMember,
    string? FailureReason);