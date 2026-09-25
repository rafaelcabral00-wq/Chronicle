using Chronicle.Application.PackTotem;
using Chronicle.RuleSets.Abstractions.Runtime;

namespace Chronicle.Desktop.PackTotem;

public sealed class CharacterPackMembershipIntegrationAdapter
{
    private readonly PackTotemOrchestrator orchestrator;

    public CharacterPackMembershipIntegrationAdapter(PackTotemOrchestrator orchestrator)
    {
        ArgumentNullException.ThrowIfNull(orchestrator);
        this.orchestrator = orchestrator;
    }

    public async Task<CharacterPackMembershipIntegrationResult> AddMemberAsync(
        RuleSetRuntimeRegistry registry,
        IntegrateCharacterPackMembershipRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(request);

        if (!IsValidCharacterIdentity(request.CharacterId))
        {
            return new CharacterPackMembershipIntegrationResult(
                ChronicleMutationSucceeded: false,
                Outcome: CharacterPackMembershipIntegrationOutcome.InvalidCharacterIdentity,
                AggregateResult: new PackTotemOperationResult(
                    Succeeded: false,
                    PackId: request.PackId,
                    LinkState: null,
                    FailureReason: $"Character identity '{request.CharacterId}' is invalid."),
                FailureReason: $"Character identity '{request.CharacterId}' is invalid.");
        }

        var aggregateId = await orchestrator
            .FindByPackIdAsync(request.PackId, cancellationToken)
            .ConfigureAwait(false);

        if (aggregateId is null)
        {
            return new CharacterPackMembershipIntegrationResult(
                ChronicleMutationSucceeded: false,
                Outcome: CharacterPackMembershipIntegrationOutcome.PackNotFound,
                AggregateResult: new PackTotemOperationResult(
                    Succeeded: false,
                    PackId: request.PackId,
                    LinkState: null,
                    FailureReason: $"Pack '{request.PackId}' was not found."),
                FailureReason: $"Pack '{request.PackId}' was not found.");
        }

        var addRequest = new AddMemberRequest(request.PackId, request.CharacterId);
        var aggregateResult = await orchestrator
            .AddMemberAsync(addRequest, cancellationToken)
            .ConfigureAwait(false);

        var outcome = aggregateResult.Succeeded
            ? CharacterPackMembershipIntegrationOutcome.MemberAdded
            : ClassifyAggregateFailure(aggregateResult.FailureReason);

        return new CharacterPackMembershipIntegrationResult(
            ChronicleMutationSucceeded: aggregateResult.Succeeded,
            Outcome: outcome,
            AggregateResult: aggregateResult,
            FailureReason: aggregateResult.Succeeded ? null : aggregateResult.FailureReason);
    }

    public async Task<CharacterPackMembershipIntegrationResult> RemoveMemberAsync(
        RuleSetRuntimeRegistry registry,
        IntegrateCharacterPackMembershipRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(request);

        if (!IsValidCharacterIdentity(request.CharacterId))
        {
            return new CharacterPackMembershipIntegrationResult(
                ChronicleMutationSucceeded: false,
                Outcome: CharacterPackMembershipIntegrationOutcome.InvalidCharacterIdentity,
                AggregateResult: new PackTotemOperationResult(
                    Succeeded: false,
                    PackId: request.PackId,
                    LinkState: null,
                    FailureReason: $"Character identity '{request.CharacterId}' is invalid."),
                FailureReason: $"Character identity '{request.CharacterId}' is invalid.");
        }

        var aggregateId = await orchestrator
            .FindByPackIdAsync(request.PackId, cancellationToken)
            .ConfigureAwait(false);

        if (aggregateId is null)
        {
            return new CharacterPackMembershipIntegrationResult(
                ChronicleMutationSucceeded: false,
                Outcome: CharacterPackMembershipIntegrationOutcome.PackNotFound,
                AggregateResult: new PackTotemOperationResult(
                    Succeeded: false,
                    PackId: request.PackId,
                    LinkState: null,
                    FailureReason: $"Pack '{request.PackId}' was not found."),
                FailureReason: $"Pack '{request.PackId}' was not found.");
        }

        var removeRequest = new RemoveMemberRequest(request.PackId, request.CharacterId);
        var aggregateResult = await orchestrator
            .RemoveMemberAsync(removeRequest, cancellationToken)
            .ConfigureAwait(false);

        var outcome = aggregateResult.Succeeded
            ? CharacterPackMembershipIntegrationOutcome.MemberRemoved
            : ClassifyAggregateFailure(aggregateResult.FailureReason);

        return new CharacterPackMembershipIntegrationResult(
            ChronicleMutationSucceeded: aggregateResult.Succeeded,
            Outcome: outcome,
            AggregateResult: aggregateResult,
            FailureReason: aggregateResult.Succeeded ? null : aggregateResult.FailureReason);
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

    private static CharacterPackMembershipIntegrationOutcome ClassifyAggregateFailure(string? failureReason)
    {
        if (string.IsNullOrEmpty(failureReason))
        {
            return CharacterPackMembershipIntegrationOutcome.ChronicleMutationFailed;
        }

        if (failureReason.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return CharacterPackMembershipIntegrationOutcome.PackNotFound;
        }

        if (failureReason.Contains("already a member", StringComparison.OrdinalIgnoreCase) ||
            failureReason.Contains("not a member", StringComparison.OrdinalIgnoreCase) ||
            failureReason.Contains("dissolved", StringComparison.OrdinalIgnoreCase))
        {
            return CharacterPackMembershipIntegrationOutcome.AggregateInvariantViolated;
        }

        return CharacterPackMembershipIntegrationOutcome.ChronicleMutationFailed;
    }
}