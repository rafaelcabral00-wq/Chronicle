using Chronicle.Application.PackTotem;
using Chronicle.RuleSets.Abstractions.Runtime;

namespace Chronicle.Desktop.PackTotem;

public sealed class WerewolfPackTotemIntegrationAdapter
{
    private readonly WerewolfPackTotemBoundaryAdapter boundaryAdapter;
    private readonly PackTotemOrchestrator orchestrator;

    public WerewolfPackTotemIntegrationAdapter(
        WerewolfPackTotemBoundaryAdapter boundaryAdapter,
        PackTotemOrchestrator orchestrator)
    {
        ArgumentNullException.ThrowIfNull(boundaryAdapter);
        ArgumentNullException.ThrowIfNull(orchestrator);
        this.boundaryAdapter = boundaryAdapter;
        this.orchestrator = orchestrator;
    }

    public async Task<PackTotemIntegrationResult> IntegrateAsync(
        RuleSetRuntimeRegistry registry,
        IntegratePackTotemBindingRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(request);

        var boundary = boundaryAdapter.Validate(
            registry,
            new ValidatePackTotemBoundaryRequest(
                PackageId: request.PackageId,
                PackageVersion: request.PackageVersion,
                OperationKey: request.OperationKey,
                ExpectedRiteKey: request.ExpectedRiteKey,
                DiceValues: request.DiceValues,
                HasTargetPiece: request.HasTargetPiece,
                RequestId: request.RequestId));

        if (boundary.Kind != PackTotemBoundaryValidationKind.BoundarySignalReceived)
        {
            return new PackTotemIntegrationResult(
                WerewolfSucceeded: boundary.Succeeded,
                ChronicleMutationSucceeded: false,
                Outcome: PackTotemIntegrationOutcome.BoundarySignalNotReceived,
                BoundaryValidation: boundary,
                AggregateResult: null,
                FailureReason: boundary.FailureReason);
        }

        var bindRequest = new BindTotemRequest(
            PackIdAggregateId: request.PackIdAggregateId,
            PackId: request.PackId,
            TotemId: request.TotemId,
            TotemRating: request.TotemRating,
            TotemAggregation: request.TotemAggregation,
            InitialImprovementPurchases: request.InitialImprovementPurchases);

        var aggregateResult = await orchestrator
            .BindTotemAsync(bindRequest, cancellationToken)
            .ConfigureAwait(false);

        var outcome = aggregateResult.Succeeded
            ? PackTotemIntegrationOutcome.Bound
            : ClassifyAggregateFailure(aggregateResult.FailureReason);

        return new PackTotemIntegrationResult(
            WerewolfSucceeded: boundary.Succeeded,
            ChronicleMutationSucceeded: aggregateResult.Succeeded,
            Outcome: outcome,
            BoundaryValidation: boundary,
            AggregateResult: aggregateResult,
            FailureReason: aggregateResult.Succeeded ? null : aggregateResult.FailureReason);
    }

    private static PackTotemIntegrationOutcome ClassifyAggregateFailure(string? failureReason)
    {
        if (string.IsNullOrEmpty(failureReason))
        {
            return PackTotemIntegrationOutcome.ChronicleMutationFailed;
        }

        if (failureReason.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return PackTotemIntegrationOutcome.AggregateNotFound;
        }

        if (failureReason.Contains("already bound", StringComparison.OrdinalIgnoreCase) ||
            failureReason.Contains("dissolved", StringComparison.OrdinalIgnoreCase))
        {
            return PackTotemIntegrationOutcome.AggregateInvariantViolated;
        }

        return PackTotemIntegrationOutcome.ChronicleMutationFailed;
    }
}
