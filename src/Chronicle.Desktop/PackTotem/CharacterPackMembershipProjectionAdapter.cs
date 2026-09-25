using Chronicle.Application.PackTotem;
using Chronicle.RuleSets.Abstractions.Runtime;

namespace Chronicle.Desktop.PackTotem;

public sealed class CharacterPackMembershipProjectionAdapter
{
    private readonly PackTotemOrchestrator orchestrator;

    public CharacterPackMembershipProjectionAdapter(PackTotemOrchestrator orchestrator)
    {
        ArgumentNullException.ThrowIfNull(orchestrator);
        this.orchestrator = orchestrator;
    }

    public async Task<CharacterPackMembershipProjectionResult> QueryAsync(
        RuleSetRuntimeRegistry registry,
        QueryCharacterPackMembershipRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(request);

        if (!IsValidCharacterIdentity(request.CharacterId))
        {
            return new CharacterPackMembershipProjectionResult(
                Success: false,
                Outcome: CharacterPackMembershipProjectionOutcome.InvalidCharacterIdentity,
                PackId: null,
                PackName: null,
                LeaderId: null,
                IsMember: false,
                FailureReason: $"Character identity '{request.CharacterId}' is invalid.");
        }

        var membershipResult = await orchestrator
            .FindPackByCharacterIdAsync(request.CharacterId, cancellationToken)
            .ConfigureAwait(false);

        if (!membershipResult.Found)
        {
            return new CharacterPackMembershipProjectionResult(
                Success: true,
                Outcome: CharacterPackMembershipProjectionOutcome.NotAMember,
                PackId: null,
                PackName: null,
                LeaderId: null,
                IsMember: false,
                FailureReason: null);
        }

        return new CharacterPackMembershipProjectionResult(
            Success: true,
            Outcome: CharacterPackMembershipProjectionOutcome.MemberFound,
            PackId: membershipResult.PackId,
            PackName: membershipResult.PackName,
            LeaderId: membershipResult.LeaderId,
            IsMember: true,
            FailureReason: null);
    }

    private static bool IsValidCharacterIdentity(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return false;
        }

        if (!characterId.StartsWith("werewolf-draft-", StringComparison.Ordinal))
        {
            return false;
        }

        var suffix = characterId["werewolf-draft-".Length..];
        return !string.IsNullOrWhiteSpace(suffix);
    }
}