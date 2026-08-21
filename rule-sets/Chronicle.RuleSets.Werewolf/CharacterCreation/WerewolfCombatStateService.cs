namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfCombatStateRequest(
    string RequestId,
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    string CombatAction,
    IReadOnlyDictionary<string, string> Parameters);

public sealed record WerewolfCombatStateResult(
    bool Succeeded,
    WerewolfRuntimeCharacterState? UpdatedState,
    IReadOnlyList<string> Findings,
    string? RequestId,
    string CombatAction,
    bool StateChanged);

public static class WerewolfCombatStateService
{
    public static WerewolfCombatStateResult ApplyCombatAction(WerewolfCombatStateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return new WerewolfCombatStateResult(false, null, ["RequestId is required"], null, string.Empty, false);
        }

        if (request.CurrentState is null)
        {
            return new WerewolfCombatStateResult(false, null, ["CurrentState is required"], null, string.Empty, false);
        }

        if (request.ExpectedRuntimeStateVersion < 1)
        {
            return new WerewolfCombatStateResult(false, null, ["ExpectedRuntimeStateVersion must be >= 1"], null, string.Empty, false);
        }

        if (request.CurrentState.RuntimeStateVersion != request.ExpectedRuntimeStateVersion)
        {
            return new WerewolfCombatStateResult(false, request.CurrentState, ["Version mismatch"], request.RequestId, request.CombatAction, false);
        }

        if (string.IsNullOrWhiteSpace(request.CombatAction))
        {
            return new WerewolfCombatStateResult(false, request.CurrentState, ["Combat action is required"], request.RequestId, string.Empty, false);
        }

        var updatedState = request.CurrentState with
        {
            RuntimeStateVersion = request.CurrentState.RuntimeStateVersion + 1
        };

        findings.Add($"Combat action '{request.CombatAction}' applied. State version incremented to {updatedState.RuntimeStateVersion}.");

        return new WerewolfCombatStateResult(
            true,
            updatedState,
            findings,
            request.RequestId,
            request.CombatAction,
            true);
    }

    public static WerewolfCombatState AddCondition(WerewolfCombatState combatState, WerewolfCombatCondition condition)
    {
        ArgumentNullException.ThrowIfNull(combatState);
        ArgumentNullException.ThrowIfNull(condition);

        var newConditions = new List<WerewolfCombatCondition>(combatState.ActiveConditions)
        {
            condition
        };

        return combatState with
        {
            CombatStateVersion = combatState.CombatStateVersion + 1,
            ActiveConditions = Array.AsReadOnly(newConditions.ToArray())
        };
    }
}
