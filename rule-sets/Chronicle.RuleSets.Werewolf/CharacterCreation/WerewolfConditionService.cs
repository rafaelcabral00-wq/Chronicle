namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfApplyConditionRequest(
    string RequestId,
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    string ConditionKey,
    WerewolfConditionKind Kind,
    string SourceLocator,
    string SourceDeformity,
    int? DurationTurns = null);

public sealed record WerewolfApplyConditionResult(
    bool Succeeded,
    WerewolfRuntimeCharacterState? NewState,
    IReadOnlyList<string> Findings,
    string RequestId,
    int NewRuntimeStateVersion);

public sealed record WerewolfClearConditionRequest(
    string RequestId,
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    string ConditionKey);

public sealed record WerewolfClearConditionResult(
    bool Succeeded,
    WerewolfRuntimeCharacterState? NewState,
    IReadOnlyList<string> Findings,
    string RequestId,
    int NewRuntimeStateVersion);

public sealed record WerewolfEvaluateActionAvailabilityRequest(
    string RequestId,
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    string ActionType);

public sealed record WerewolfEvaluateActionAvailabilityResult(
    bool Succeeded,
    bool IsAvailable,
    IReadOnlyList<string> Findings,
    string RequestId,
    string? UnavailableReason = null);

public static class WerewolfConditionService
{
    public static WerewolfApplyConditionResult ApplyCondition(WerewolfApplyConditionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return new WerewolfApplyConditionResult(false, null, ["RequestId is required."], string.Empty, 0);
        }

        if (request.CurrentState is null)
        {
            return new WerewolfApplyConditionResult(false, null, ["CurrentState is required."], request.RequestId, 0);
        }

        if (request.CurrentState.RuntimeStateVersion != request.ExpectedRuntimeStateVersion)
        {
            return new WerewolfApplyConditionResult(false, null, ["Version mismatch."], request.RequestId, request.CurrentState.RuntimeStateVersion);
        }

        var existingConditions = request.CurrentState.Conditions?.ToList() ?? new List<WerewolfCondition>();

        if (existingConditions.Any(c => string.Equals(c.ConditionKey, request.ConditionKey, StringComparison.Ordinal) && c.IsActive))
        {
            findings.Add($"Condition '{request.ConditionKey}' is already active.");
            return new WerewolfApplyConditionResult(true, request.CurrentState, findings, request.RequestId, request.CurrentState.RuntimeStateVersion);
        }

        var condition = new WerewolfCondition(
            request.ConditionKey,
            request.Kind,
            request.SourceLocator,
            request.SourceDeformity,
            request.CurrentState.RuntimeStateVersion,
            true,
            request.DurationTurns);

        existingConditions.Add(condition);

        var newState = request.CurrentState with
        {
            Conditions = Array.AsReadOnly(existingConditions.ToArray()),
            RuntimeStateVersion = request.CurrentState.RuntimeStateVersion + 1
        };

        findings.Add($"Condition '{request.ConditionKey}' applied from {request.SourceDeformity}.");

        return new WerewolfApplyConditionResult(true, newState, findings, request.RequestId, newState.RuntimeStateVersion);
    }

    public static WerewolfClearConditionResult ClearCondition(WerewolfClearConditionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return new WerewolfClearConditionResult(false, null, ["RequestId is required."], string.Empty, 0);
        }

        if (request.CurrentState is null)
        {
            return new WerewolfClearConditionResult(false, null, ["CurrentState is required."], request.RequestId, 0);
        }

        if (request.CurrentState.RuntimeStateVersion != request.ExpectedRuntimeStateVersion)
        {
            return new WerewolfClearConditionResult(false, null, ["Version mismatch."], request.RequestId, request.CurrentState.RuntimeStateVersion);
        }

        var existingConditions = request.CurrentState.Conditions?.ToList() ?? new List<WerewolfCondition>();
        var targetCondition = existingConditions.FirstOrDefault(c => string.Equals(c.ConditionKey, request.ConditionKey, StringComparison.Ordinal) && c.IsActive);

        if (targetCondition is null)
        {
            findings.Add($"Condition '{request.ConditionKey}' is not active.");
            return new WerewolfClearConditionResult(true, request.CurrentState, findings, request.RequestId, request.CurrentState.RuntimeStateVersion);
        }

        var updatedConditions = new List<WerewolfCondition>();
        foreach (var c in existingConditions)
        {
            if (string.Equals(c.ConditionKey, request.ConditionKey, StringComparison.Ordinal) && c.IsActive)
            {
                updatedConditions.Add(c with { IsActive = false });
            }
            else
            {
                updatedConditions.Add(c);
            }
        }

        var newState = request.CurrentState with
        {
            Conditions = Array.AsReadOnly(updatedConditions.ToArray()),
            RuntimeStateVersion = request.CurrentState.RuntimeStateVersion + 1
        };

        findings.Add($"Condition '{request.ConditionKey}' cleared.");

        return new WerewolfClearConditionResult(true, newState, findings, request.RequestId, newState.RuntimeStateVersion);
    }

    public static WerewolfEvaluateActionAvailabilityResult EvaluateActionAvailability(WerewolfEvaluateActionAvailabilityRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return new WerewolfEvaluateActionAvailabilityResult(false, false, ["RequestId is required."], string.Empty, "Invalid request");
        }

        if (request.CurrentState is null)
        {
            return new WerewolfEvaluateActionAvailabilityResult(false, false, ["CurrentState is required."], request.RequestId, "Invalid state");
        }

        if (request.CurrentState.RuntimeStateVersion != request.ExpectedRuntimeStateVersion)
        {
            return new WerewolfEvaluateActionAvailabilityResult(false, false, ["Version mismatch."], request.RequestId, "Version mismatch");
        }

        var activeConditions = request.CurrentState.Conditions?
            .Where(c => c.IsActive)
            .Select(c => c.ConditionKey)
            .ToList() ?? new List<string>();

        if (activeConditions.Contains(WerewolfConditionIdentifiers.Incapacitated, StringComparer.Ordinal))
        {
            findings.Add("Action unavailable: character is incapacitated.");
            return new WerewolfEvaluateActionAvailabilityResult(true, false, findings, request.RequestId, "Incapacitated");
        }

        if (activeConditions.Contains(WerewolfConditionIdentifiers.TemporaryPsychoticEpisode, StringComparer.Ordinal))
        {
            findings.Add("Action unavailable: character is experiencing a temporary psychotic episode.");
            return new WerewolfEvaluateActionAvailabilityResult(true, false, findings, request.RequestId, "Temporary psychotic episode");
        }

        if (activeConditions.Contains(WerewolfConditionIdentifiers.Prone, StringComparer.Ordinal))
        {
            findings.Add("Action unavailable: character is prone and must stand before acting.");
            return new WerewolfEvaluateActionAvailabilityResult(true, false, findings, request.RequestId, "Prone");
        }

        if (activeConditions.Contains(WerewolfConditionIdentifiers.Restrained, StringComparer.Ordinal))
        {
            findings.Add("Action unavailable: character is restrained and cannot act freely.");
            return new WerewolfEvaluateActionAvailabilityResult(true, false, findings, request.RequestId, "Restrained");
        }

        findings.Add("Action is available.");
        return new WerewolfEvaluateActionAvailabilityResult(true, true, findings, request.RequestId, null);
    }

    public static WerewolfRuntimeCharacterState ApplyGiftConditions(WerewolfRuntimeCharacterState state)
    {
        if (state.ActiveGiftEffects is null || state.ActiveGiftEffects.Count == 0)
        {
            return state;
        }

        var activeEffects = WerewolfGiftEffectService.GetSceneValidEffects(state);
        if (activeEffects.Count == 0)
        {
            return state;
        }

        var existingConditions = state.Conditions?.ToList() ?? new List<WerewolfCondition>();
        var updated = false;

        foreach (var effect in activeEffects)
        {
            var conditionKey = effect.EffectKind switch
            {
                WerewolfActiveGiftEffectKind.ProneCondition => WerewolfConditionIdentifiers.Prone,
                WerewolfActiveGiftEffectKind.RestrainedCondition => WerewolfConditionIdentifiers.Restrained,
                _ => null
            };

            if (string.IsNullOrWhiteSpace(conditionKey))
            {
                continue;
            }

            if (effect.Magnitude <= 0)
            {
                continue;
            }

            if (existingConditions.Any(c => string.Equals(c.ConditionKey, conditionKey, StringComparison.Ordinal) && c.IsActive))
            {
                continue;
            }

            var condition = new WerewolfCondition(
                conditionKey,
                effect.EffectKind switch
                {
                    WerewolfActiveGiftEffectKind.ProneCondition => WerewolfConditionKind.Prone,
                    WerewolfActiveGiftEffectKind.RestrainedCondition => WerewolfConditionKind.Restrained,
                    _ => WerewolfConditionKind.CriticalFailure
                },
                effect.SourceLocator,
                string.Empty,
                state.RuntimeStateVersion,
                true,
                effect.DurationType == WerewolfGiftDurationType.Turn ? 1 : null);

            existingConditions.Add(condition);
            updated = true;
        }

        if (!updated)
        {
            return state;
        }

        return state with
        {
            Conditions = Array.AsReadOnly(existingConditions.ToArray())
        };
    }
}
