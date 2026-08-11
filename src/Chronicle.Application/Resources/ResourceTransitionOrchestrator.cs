using Chronicle.Contracts;
using Chronicle.RuleSets.Abstractions.Runtime;

namespace Chronicle.Application;

public sealed class ResourceTransitionOrchestrator
{
    private readonly RuleSetRuntimeRegistry _registry;

    public ResourceTransitionOrchestrator(RuleSetRuntimeRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
    }

    public ResourceTransitionResult Transition(ResourceTransitionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var operationResult = _registry.Execute(new RuleSetOperationRequest(
            request.PackageId,
            request.PackageVersion,
            request.OperationKey,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = request.RequestId,
                ["currentState"] = request.CurrentStateJson,
                ["expectedRuntimeStateVersion"] = request.ExpectedRuntimeStateVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["resourceId"] = request.ResourceId,
                ["amount"] = request.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture)
            }));

        if (!operationResult.Succeeded)
        {
            return new ResourceTransitionResult(
                false,
                null,
                null,
                null,
                operationResult.FailureCode?.ToString(),
                "orchestration-failed",
                operationResult.Findings.Select(f => new ResourceTransitionFinding(f.Severity.ToString(), f.Code, f.Message)).ToArray());
        }

        var outputs = operationResult.Outputs;
        if (!outputs.TryGetValue("newState", out var newStateJson) ||
            !outputs.TryGetValue("newRuntimeStateVersion", out var newVersionText) ||
            !int.TryParse(newVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var newVersion))
        {
            return new ResourceTransitionResult(
                false,
                null,
                null,
                null,
                RuleSetOperationFailureCode.InvalidRequest.ToString(),
                "invalid-operation-output",
                [new ResourceTransitionFinding(RuleSetRuntimeFindingSeverity.Error.ToString(), "InvalidOperationOutput", "Operation did not return required outputs.")]);
        }

        return new ResourceTransitionResult(
            true,
            request.RequestId,
            newStateJson,
            newVersion,
            null,
            "resolved",
            operationResult.Findings.Select(f => new ResourceTransitionFinding(f.Severity.ToString(), f.Code, f.Message)).ToArray());
    }
}

public sealed record ResourceTransitionRequest(
    string PackageId,
    string PackageVersion,
    string OperationKey,
    string RequestId,
    string CurrentStateJson,
    int ExpectedRuntimeStateVersion,
    string ResourceId,
    int Amount);

public sealed record ResourceTransitionResult(
    bool Succeeded,
    string? RequestId,
    string? NewStateJson,
    int? NewRuntimeStateVersion,
    string? FailureCode,
    string ResolutionStage,
    IReadOnlyList<ResourceTransitionFinding> Findings);

public sealed record ResourceTransitionFinding(
    string Severity,
    string Code,
    string Message);
