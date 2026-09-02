using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;

namespace Chronicle.Desktop.PackTotem;

public sealed class WerewolfPackTotemBoundaryAdapter
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "Adapter is intentionally stateless. Kept as an instance method so composition-root injection and future per-call options remain available without breaking callers.")]
    public PackTotemBoundaryValidationResult Validate(RuleSetRuntimeRegistry registry, ValidatePackTotemBoundaryRequest request)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(request);

        var runtime = registry.FindRuntime(request.PackageId, request.PackageVersion);
        if (runtime is null)
        {
            return new PackTotemBoundaryValidationResult(
                Succeeded: false,
                FailureReason: $"Runtime not registered for package '{request.PackageId}' version '{request.PackageVersion}'.",
                Kind: PackTotemBoundaryValidationKind.UnregisteredRuntime,
                S4Observation: null);
        }

        var operation = registry.FindOperation(request.PackageId, request.PackageVersion, request.OperationKey);
        if (operation is null || operation.Status != RuleSetOperationStatus.Enabled)
        {
            return new PackTotemBoundaryValidationResult(
                Succeeded: false,
                FailureReason: $"Operation '{request.OperationKey}' is not declared or not enabled.",
                Kind: PackTotemBoundaryValidationKind.UndeclaredOperation,
                S4Observation: null);
        }

        if (!string.Equals(request.ExpectedRiteKey, WerewolfRiteIdentifiers.Totem, StringComparison.Ordinal))
        {
            return new PackTotemBoundaryValidationResult(
                Succeeded: false,
                FailureReason: $"Expected rite key '{WerewolfRiteIdentifiers.Totem}' but received '{request.ExpectedRiteKey}'.",
                Kind: PackTotemBoundaryValidationKind.InvalidRiteKey,
                S4Observation: null);
        }

        var executionRequest = new WerewolfRiteExecutionRequest(
            RequestId: request.RequestId,
            RiteKey: request.ExpectedRiteKey,
            DiceValues: request.DiceValues,
            HasTargetPiece: request.HasTargetPiece);

        var result = WerewolfRiteExecutionService.Execute(executionRequest);

        if (result.Payload is not WerewolfTotemBindingBoundaryPayload payload)
        {
            if (!result.Succeeded)
            {
                return new PackTotemBoundaryValidationResult(
                    Succeeded: false,
                    FailureReason: $"Rite test failed and no typed boundary payload was produced: {result.InterpretationStatus}.",
                    Kind: PackTotemBoundaryValidationKind.RiteTestFailed,
                    S4Observation: null);
            }

            return new PackTotemBoundaryValidationResult(
                Succeeded: false,
                FailureReason: "Execution result payload was not of the expected WerewolfTotemBindingBoundaryPayload type.",
                Kind: PackTotemBoundaryValidationKind.WrongPayloadType,
                S4Observation: null);
        }

        var observation = new PackTotemBoundaryS4Observation(
            ObservedRiteKey: payload.RiteKey,
            SourceLocator: payload.SourceLocator,
            Note: payload.Note,
            PayloadTotemId: payload.TotemId,
            PayloadPackId: payload.PackId,
            PayloadMemberRoster: payload.MemberRoster,
            PayloadTotemAggregation: payload.TotemAggregation,
            SuccessCount: result.SuccessCount,
            Difficulty: result.Difficulty,
            InterpretationStatus: result.InterpretationStatus,
            Effect: result.Effect);

        if (!result.Succeeded)
        {
            return new PackTotemBoundaryValidationResult(
                Succeeded: false,
                FailureReason: $"Rite test failed: {result.InterpretationStatus}.",
                Kind: PackTotemBoundaryValidationKind.RiteTestFailed,
                S4Observation: observation);
        }

        return new PackTotemBoundaryValidationResult(
            Succeeded: true,
            FailureReason: null,
            Kind: PackTotemBoundaryValidationKind.BoundarySignalReceived,
            S4Observation: observation);
    }
}
