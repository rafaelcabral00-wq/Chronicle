namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfEnterFrenzyRequest(
    string RequestId,
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    WerewolfFrenzyType FrenzyType,
    string Trigger,
    int AccumulatedSuccesses,
    string? TargetRestriction = null);

public sealed record WerewolfEnterFrenzyResult(
    bool Succeeded,
    WerewolfRuntimeCharacterState? UpdatedState,
    IReadOnlyList<string> Findings,
    string RequestId,
    int NewRuntimeStateVersion,
    string? ErrorCode = null);

public static class WerewolfFrenzyResolutionService
{
    public static WerewolfEnterFrenzyResult EnterFrenzy(WerewolfEnterFrenzyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return new WerewolfEnterFrenzyResult(false, null, ["RequestId is required."], string.Empty, 0, "InvalidRequestId");
        }

        if (request.CurrentState is null)
        {
            return new WerewolfEnterFrenzyResult(false, null, ["CurrentState is required."], request.RequestId, 0, "InvalidState");
        }

        if (request.ExpectedRuntimeStateVersion < 1)
        {
            return new WerewolfEnterFrenzyResult(false, null, ["ExpectedRuntimeStateVersion must be >= 1."], request.RequestId, 0, "InvalidVersion");
        }

        if (request.CurrentState.RuntimeStateVersion != request.ExpectedRuntimeStateVersion)
        {
            return new WerewolfEnterFrenzyResult(false, request.CurrentState, ["Version mismatch."], request.RequestId, request.CurrentState.RuntimeStateVersion, "StaleVersion");
        }

        if (request.FrenzyType == WerewolfFrenzyType.None)
        {
            return new WerewolfEnterFrenzyResult(false, request.CurrentState, ["FrenzyType cannot be None for EnterFrenzy."], request.RequestId, request.CurrentState.RuntimeStateVersion, "InvalidFrenzyType");
        }

        if (request.CurrentState.FrenzyState is { IsInFrenzy: true })
        {
            return new WerewolfEnterFrenzyResult(false, request.CurrentState, ["Character is already in frenzy."], request.RequestId, request.CurrentState.RuntimeStateVersion, "AlreadyInFrenzy");
        }

        var newFrenzyState = new WerewolfFrenzyState(
            IsInFrenzy: true,
            FrenzyType: request.FrenzyType,
            Trigger: request.Trigger,
            AccumulatedSuccesses: request.AccumulatedSuccesses,
            StartedAtTurn: 0,
            TargetRestriction: request.TargetRestriction,
            IsSuppressed: false,
            SourceLocator: "Line 2916");

        var newState = request.CurrentState with
        {
            RuntimeStateVersion = request.CurrentState.RuntimeStateVersion + 1,
            FrenzyState = newFrenzyState
        };

        var frenzyTypeName = request.FrenzyType switch
        {
            WerewolfFrenzyType.Wild => "Wild Frenzy",
            WerewolfFrenzyType.Fox => "Fox Frenzy",
            WerewolfFrenzyType.Extreme => "Extreme Frenzy",
            _ => "Unknown Frenzy"
        };

        findings.Add($"Entered {frenzyTypeName} triggered by {request.Trigger} with {request.AccumulatedSuccesses} successes.");

        if (request.FrenzyType == WerewolfFrenzyType.Fox)
        {
            findings.Add("Fox Frenzy: character changes to Lupus form and flees.");
        }

        if (request.FrenzyType == WerewolfFrenzyType.Extreme)
        {
            findings.Add("Extreme Frenzy: 6+ successes; uncontrollable by Willpower.");
        }

        return new WerewolfEnterFrenzyResult(true, newState, findings, request.RequestId, newState.RuntimeStateVersion);
    }

    public static WerewolfEnterFrenzyResult SuppressFrenzy(WerewolfRuntimeCharacterState currentState, int expectedRuntimeStateVersion)
    {
        ArgumentNullException.ThrowIfNull(currentState);

        var findings = new List<string>();

        if (expectedRuntimeStateVersion < 1)
        {
            return new WerewolfEnterFrenzyResult(false, null, ["ExpectedRuntimeStateVersion must be >= 1."], string.Empty, 0, "InvalidVersion");
        }

        if (currentState.RuntimeStateVersion != expectedRuntimeStateVersion)
        {
            return new WerewolfEnterFrenzyResult(false, currentState, ["Version mismatch."], "suppress", currentState.RuntimeStateVersion, "StaleVersion");
        }

        if (currentState.FrenzyState is null || !currentState.FrenzyState.IsInFrenzy)
        {
            return new WerewolfEnterFrenzyResult(false, currentState, ["Character is not in frenzy."], "suppress", currentState.RuntimeStateVersion, "NotInFrenzy");
        }

        if (currentState.FrenzyState.FrenzyType == WerewolfFrenzyType.Extreme)
        {
            return new WerewolfEnterFrenzyResult(false, currentState, ["Extreme Frenzy cannot be suppressed by Willpower."], "suppress", currentState.RuntimeStateVersion, "ExtremeFrenzyUncontrollable");
        }

        if (currentState.WillpowerCurrent < 1)
        {
            return new WerewolfEnterFrenzyResult(false, currentState, ["Insufficient Willpower to suppress frenzy."], "suppress", currentState.RuntimeStateVersion, "InsufficientWillpower");
        }

        var newWillpowerCurrent = currentState.WillpowerCurrent - 1;

        var newFrenzyState = currentState.FrenzyState with
        {
            IsInFrenzy = false,
            IsSuppressed = true
        };

        var newState = currentState with
        {
            RuntimeStateVersion = currentState.RuntimeStateVersion + 1,
            FrenzyState = newFrenzyState,
            WillpowerCurrent = newWillpowerCurrent
        };

        findings.Add("Frenzy suppressed by spending 1 Willpower point.");

        return new WerewolfEnterFrenzyResult(true, newState, findings, "suppress", newState.RuntimeStateVersion);
    }

    public static WerewolfEnterFrenzyResult EndFrenzy(WerewolfRuntimeCharacterState currentState, int expectedRuntimeStateVersion)
    {
        ArgumentNullException.ThrowIfNull(currentState);

        var findings = new List<string>();

        if (expectedRuntimeStateVersion < 1)
        {
            return new WerewolfEnterFrenzyResult(false, null, ["ExpectedRuntimeStateVersion must be >= 1."], string.Empty, 0, "InvalidVersion");
        }

        if (currentState.RuntimeStateVersion != expectedRuntimeStateVersion)
        {
            return new WerewolfEnterFrenzyResult(false, currentState, ["Version mismatch."], "end", currentState.RuntimeStateVersion, "StaleVersion");
        }

        if (currentState.FrenzyState is null || !currentState.FrenzyState.IsInFrenzy)
        {
            return new WerewolfEnterFrenzyResult(false, currentState, ["Character is not in frenzy."], "end", currentState.RuntimeStateVersion, "NotInFrenzy");
        }

        var newState = currentState with
        {
            RuntimeStateVersion = currentState.RuntimeStateVersion + 1,
            FrenzyState = new WerewolfFrenzyState(
                IsInFrenzy: false,
                FrenzyType: WerewolfFrenzyType.None,
                Trigger: string.Empty,
                AccumulatedSuccesses: 0,
                StartedAtTurn: 0,
                TargetRestriction: null,
                IsSuppressed: false,
                SourceLocator: "Line 2916")
        };

        findings.Add("Frenzy ended.");

        return new WerewolfEnterFrenzyResult(true, newState, findings, "end", newState.RuntimeStateVersion);
    }

    public static string EvaluateFrenzyAction(WerewolfRuntimeCharacterState currentState, string actionType)
    {
        ArgumentNullException.ThrowIfNull(currentState);

        if (currentState.FrenzyState is null || !currentState.FrenzyState.IsInFrenzy)
        {
            return "available";
        }

        var frenzy = currentState.FrenzyState;

        if (frenzy.IsSuppressed)
        {
            return "available";
        }

        if (frenzy.FrenzyType == WerewolfFrenzyType.Extreme)
        {
            return "available";
        }

        if (frenzy.FrenzyType == WerewolfFrenzyType.Fox)
        {
            if (string.Equals(actionType, "flee", StringComparison.OrdinalIgnoreCase))
            {
                return "available";
            }
            return "unavailable-fox-frenzy";
        }

        if (frenzy.FrenzyType == WerewolfFrenzyType.Wild)
        {
            if (string.Equals(actionType, "attack", StringComparison.OrdinalIgnoreCase))
            {
                return "available";
            }

            if (!string.IsNullOrWhiteSpace(frenzy.TargetRestriction))
            {
                if (actionType.Contains(frenzy.TargetRestriction, StringComparison.OrdinalIgnoreCase))
                {
                    return "available";
                }
            }

            return "available";
        }

        return "available";
    }
}
