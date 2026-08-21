namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfCombatRageRequest(
    string RequestId,
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    int RageToSpend,
    string RagePurpose,
    int? Dexterity,
    int? Wits);

public sealed record WerewolfCombatRageResult(
    bool Succeeded,
    WerewolfRuntimeCharacterState? UpdatedState,
    IReadOnlyList<string> Findings,
    string? RequestId,
    int RageSpent,
    int NewRageCurrent,
    int ExtraActionsGranted,
    bool TransformationAllowed,
    bool StunNegated);

public sealed record WerewolfCombatRageExtraActionsResult(
    IReadOnlyList<string> Findings,
    int ExtraActions,
    int RageInvested);

public static class WerewolfCombatRageService
{
    public static WerewolfCombatRageResult SpendRage(WerewolfCombatRageRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return new WerewolfCombatRageResult(false, null, ["RequestId is required"], null, 0, 0, 0, false, false);
        }

        if (request.CurrentState is null)
        {
            return new WerewolfCombatRageResult(false, null, ["CurrentState is required"], null, 0, 0, 0, false, false);
        }

        if (request.ExpectedRuntimeStateVersion < 1)
        {
            return new WerewolfCombatRageResult(false, null, ["ExpectedRuntimeStateVersion must be >= 1"], null, 0, 0, 0, false, false);
        }

        if (request.CurrentState.RuntimeStateVersion != request.ExpectedRuntimeStateVersion)
        {
            return new WerewolfCombatRageResult(false, request.CurrentState, ["Version mismatch"], request.RequestId, 0, 0, 0, false, false);
        }

        if (request.RageToSpend <= 0)
        {
            return new WerewolfCombatRageResult(false, request.CurrentState, ["Rage to spend must be positive"], request.RequestId, 0, 0, 0, false, false);
        }

        if (request.RageToSpend > request.CurrentState.RageCurrent)
        {
            return new WerewolfCombatRageResult(false, request.CurrentState, ["Insufficient Rage"], request.RequestId, 0, 0, 0, false, false);
        }

        if (string.IsNullOrWhiteSpace(request.RagePurpose))
        {
            return new WerewolfCombatRageResult(false, request.CurrentState, ["Rage purpose is required"], request.RequestId, 0, 0, 0, false, false);
        }

        var rageSpent = request.RageToSpend;
        var newRageCurrent = request.CurrentState.RageCurrent - rageSpent;
        var extraActionsGranted = 0;
        var transformationAllowed = false;
        var stunNegated = false;

        switch (request.RagePurpose.ToLowerInvariant())
        {
            case "extra-action":
                var dexterity = request.Dexterity ?? 0;
                var wits = request.Wits ?? 0;
                var maxExtra = Math.Min(request.CurrentState.RagePermanent / 2, Math.Min(dexterity, wits));
                extraActionsGranted = Math.Min(rageSpent, maxExtra);
                findings.Add($"Spent {rageSpent} Rage for {extraActionsGranted} extra actions (max {maxExtra}).");
                break;

            case "transformation":
                transformationAllowed = true;
                findings.Add($"Spent {rageSpent} Rage for instant transformation.");
                break;

            case "ignore-stun":
                stunNegated = true;
                findings.Add($"Spent {rageSpent} Rage to ignore stun/pain for one turn.");
                break;

            default:
                return new WerewolfCombatRageResult(false, request.CurrentState, ["Unknown Rage purpose"], request.RequestId, 0, 0, 0, false, false);
        }

        var updatedState = request.CurrentState with
        {
            RuntimeStateVersion = request.CurrentState.RuntimeStateVersion + 1,
            RageCurrent = newRageCurrent
        };

        return new WerewolfCombatRageResult(
            true,
            updatedState,
            findings,
            request.RequestId,
            rageSpent,
            newRageCurrent,
            extraActionsGranted,
            transformationAllowed,
            stunNegated);
    }

    public static WerewolfCombatRageExtraActionsResult CalculateExtraActions(WerewolfRuntimeCharacterState currentState, int rageInvested)
    {
        ArgumentNullException.ThrowIfNull(currentState);

        var findings = new List<string>();

        if (rageInvested <= 0)
        {
            return new WerewolfCombatRageExtraActionsResult([], 0, 0);
        }

        var maxExtra = Math.Min(currentState.RagePermanent / 2, 5);
        var extraActions = Math.Min(rageInvested, maxExtra);
        findings.Add($"Calculated {extraActions} extra actions from {rageInvested} Rage invested (max {maxExtra}).");

        return new WerewolfCombatRageExtraActionsResult(findings, extraActions, rageInvested);
    }
}
