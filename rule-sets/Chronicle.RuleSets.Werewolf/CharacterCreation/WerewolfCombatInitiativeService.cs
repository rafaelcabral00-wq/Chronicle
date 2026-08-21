namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfCombatInitiativeRequest(
    string RequestId,
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    int? Dexterity,
    int? Wits,
    int? SuppliedDieRoll,
    int? RageExtraActions);

public sealed record WerewolfCombatInitiativeResult(
    bool Succeeded,
    WerewolfRuntimeCharacterState? UpdatedState,
    IReadOnlyList<string> Findings,
    string? RequestId,
    int InitiativeModifier,
    int SuppliedDieRoll,
    int FinalInitiative,
    int Dexterity,
    int Wits,
    int MaxExtraActions);

public static class WerewolfCombatInitiativeService
{
    public static int ComputeInitiativeModifier(IReadOnlyDictionary<string, int> effectiveAttributes)
    {
        ArgumentNullException.ThrowIfNull(effectiveAttributes);

        var dexterity = effectiveAttributes.GetValueOrDefault(WerewolfAttributeIdentifiers.Dexterity, 0);
        var wits = effectiveAttributes.GetValueOrDefault(WerewolfAttributeIdentifiers.Wits, 0);

        return dexterity + wits;
    }

    public static IReadOnlyList<string> GetTurnStructure() => ["Initiative", "Declaration", "Attack", "Damage"];

    public static string GetTurnDuration() => "3 seconds";

    public static WerewolfCombatInitiativeResult CalculateInitiative(WerewolfCombatInitiativeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return new WerewolfCombatInitiativeResult(false, null, ["RequestId is required"], null, 0, 0, 0, 0, 0, 0);
        }

        if (request.CurrentState is null)
        {
            return new WerewolfCombatInitiativeResult(false, null, ["CurrentState is required"], null, 0, 0, 0, 0, 0, 0);
        }

        if (request.ExpectedRuntimeStateVersion < 1)
        {
            return new WerewolfCombatInitiativeResult(false, null, ["ExpectedRuntimeStateVersion must be >= 1"], null, 0, 0, 0, 0, 0, 0);
        }

        if (request.CurrentState.RuntimeStateVersion != request.ExpectedRuntimeStateVersion)
        {
            return new WerewolfCombatInitiativeResult(false, request.CurrentState, ["Version mismatch"], request.RequestId, 0, 0, 0, 0, 0, 0);
        }

        if (request.Dexterity is null || request.Dexterity < 0)
        {
            return new WerewolfCombatInitiativeResult(false, request.CurrentState, ["Dexterity must be non-negative"], request.RequestId, 0, 0, 0, 0, 0, 0);
        }

        if (request.Wits is null || request.Wits < 0)
        {
            return new WerewolfCombatInitiativeResult(false, request.CurrentState, ["Wits must be non-negative"], request.RequestId, 0, 0, 0, 0, 0, 0);
        }

        if (request.SuppliedDieRoll is null || request.SuppliedDieRoll < 1 || request.SuppliedDieRoll > 10)
        {
            return new WerewolfCombatInitiativeResult(false, request.CurrentState, ["Supplied die roll must be between 1 and 10"], request.RequestId, 0, 0, 0, 0, 0, 0);
        }

        var dexterity = request.Dexterity.Value;
        var wits = request.Wits.Value;
        var suppliedDieRoll = request.SuppliedDieRoll.Value;
        var initiativeModifier = dexterity + wits;
        var finalInitiative = initiativeModifier + suppliedDieRoll;

        var maxExtraActions = Math.Min(request.CurrentState.RagePermanent / 2, Math.Min(dexterity, wits));

        if (request.RageExtraActions is not null && request.RageExtraActions > 0)
        {
            if (request.RageExtraActions > maxExtraActions)
            {
                findings.Add($"Requested {request.RageExtraActions} extra actions exceeds maximum {maxExtraActions}.");
            }
        }

        findings.Add($"Initiative: {dexterity} Dexterity + {wits} Wits = {initiativeModifier} modifier + {suppliedDieRoll} die = {finalInitiative}. Max extra actions from Rage: {maxExtraActions}.");

        var updatedState = request.CurrentState with
        {
            RuntimeStateVersion = request.CurrentState.RuntimeStateVersion + 1
        };

        return new WerewolfCombatInitiativeResult(
            true,
            updatedState,
            findings,
            request.RequestId,
            initiativeModifier,
            suppliedDieRoll,
            finalInitiative,
            dexterity,
            wits,
            maxExtraActions);
    }
}
