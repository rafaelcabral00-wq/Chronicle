using Chronicle.Contracts;
using Chronicle.RuleSets.Abstractions.Runtime;

namespace Chronicle.Application;

public sealed class ActionResolutionOrchestrator
{
    private readonly RuleSetRuntimeRegistry _registry;
    private readonly IDiceValueGenerator _diceGenerator;

    public ActionResolutionOrchestrator(RuleSetRuntimeRegistry registry, IDiceValueGenerator diceGenerator)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(diceGenerator);

        _registry = registry;
        _diceGenerator = diceGenerator;
    }

    public ActionResolutionResult Resolve(ActionResolutionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var defineResult = _registry.Execute(new RuleSetOperationRequest(
            request.PackageId,
            request.PackageVersion,
            "character-runtime.define-action-test",
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["draftId"] = request.DraftId,
                ["draftVersion"] = request.DraftVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["expectedDraftVersion"] = request.ExpectedDraftVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["requestId"] = request.RequestId,
                ["attributeId"] = request.AttributeId,
                ["abilityId"] = request.AbilityId,
                ["difficulty"] = request.Difficulty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["modifier"] = request.Modifier?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
            }));

        if (!defineResult.Succeeded)
        {
            return new ActionResolutionResult(false, null, [], defineResult.FailureCode?.ToString(), "definition-failed", null);
        }

        var diceQuantity = int.Parse(defineResult.Outputs["diceQuantity"], System.Globalization.CultureInfo.InvariantCulture);
        var diceFaces = int.Parse(defineResult.Outputs["diceFaces"], System.Globalization.CultureInfo.InvariantCulture);
        var diceRequest = new DiceRollRequest(
            request.RequestId,
            diceQuantity,
            (DiceSize)diceFaces,
            request.Metadata);

        var diceResult = DiceRollService.Execute(diceRequest, _diceGenerator);
        if (!diceResult.Succeeded)
        {
            return new ActionResolutionResult(false, null, [], diceResult.FailureCode?.ToString(), "dice-execution-failed", null);
        }

        var diceValuesText = string.Join(",", diceResult.DiceValues.Select(d => d.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        var interpretResult = _registry.Execute(new RuleSetOperationRequest(
            request.PackageId,
            request.PackageVersion,
            "character-runtime.interpret-action-roll",
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = request.RequestId,
                ["diceValues"] = diceValuesText,
                ["difficulty"] = request.Difficulty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["diceQuantity"] = diceQuantity.ToString(System.Globalization.CultureInfo.InvariantCulture)
            }));

        if (!interpretResult.Succeeded)
        {
            return new ActionResolutionResult(false, null, [], interpretResult.FailureCode?.ToString(), "interpretation-failed", null);
        }

        return new ActionResolutionResult(
            true,
            request.RequestId,
            diceResult.DiceValues,
            null,
            "resolved",
            interpretResult.Outputs);
    }
}

public sealed record ActionResolutionRequest(
    string PackageId,
    string PackageVersion,
    string DraftId,
    int DraftVersion,
    int ExpectedDraftVersion,
    string RequestId,
    string AttributeId,
    string AbilityId,
    int Difficulty,
    int? Modifier,
    IReadOnlyDictionary<string, string>? Metadata);

public sealed record ActionResolutionResult(
    bool Succeeded,
    string? RequestId,
    IReadOnlyList<int> RawDiceValues,
    string? FailureCode,
    string ResolutionStage,
    IReadOnlyDictionary<string, string>? InterpretationOutputs);
