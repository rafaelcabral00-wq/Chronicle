namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfCombatSilverRequest(
    string RequestId,
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    bool IsInContact);

public sealed record WerewolfCombatSilverResult(
    bool Succeeded,
    WerewolfRuntimeCharacterState? UpdatedState,
    IReadOnlyList<string> Findings,
    string? RequestId,
    bool IsVulnerable,
    bool IsRacialForm,
    int SilverDamagePerTurn,
    int GnosePenalty);

public sealed record WerewolfCombatSilverContactResult(
    IReadOnlyList<string> Findings,
    int TotalAggravatedDamage,
    string DamageType,
    bool RequiresGiftsOrFetishes);

public static class WerewolfCombatSilverService
{
    public static WerewolfCombatSilverResult CalculateSilver(WerewolfCombatSilverRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return new WerewolfCombatSilverResult(false, null, ["RequestId is required"], null, false, false, 0, 0);
        }

        if (request.CurrentState is null)
        {
            return new WerewolfCombatSilverResult(false, null, ["CurrentState is required"], null, false, false, 0, 0);
        }

        if (request.ExpectedRuntimeStateVersion < 1)
        {
            return new WerewolfCombatSilverResult(false, null, ["ExpectedRuntimeStateVersion must be >= 1"], null, false, false, 0, 0);
        }

        if (request.CurrentState.RuntimeStateVersion != request.ExpectedRuntimeStateVersion)
        {
            return new WerewolfCombatSilverResult(false, request.CurrentState, ["Version mismatch"], request.RequestId, false, false, 0, 0);
        }

        var currentForm = request.CurrentState.CurrentForm;
        var birthRace = request.CurrentState.BirthRace;
        var isRacialForm = string.Equals(currentForm, birthRace switch
        {
            WerewolfRaceIdentifiers.Homid => WerewolfFormIdentifiers.Homid,
            WerewolfRaceIdentifiers.Metis => WerewolfFormIdentifiers.Crinos,
            WerewolfRaceIdentifiers.Lupus => WerewolfFormIdentifiers.Lupus,
            _ => string.Empty
        }, StringComparison.Ordinal);

        var isVulnerable = request.IsInContact;
        var silverDamagePerTurn = 0;
        var gnosePenalty = 0;

        if (isVulnerable)
        {
            if (isRacialForm)
            {
                silverDamagePerTurn = 0;
                findings.Add("Racial form: silver contact causes normal weapon damage only.");
            }
            else
            {
                silverDamagePerTurn = 1;
                findings.Add("Non-racial form: silver contact causes 1 aggravated damage per turn.");
            }
        }

        var updatedState = request.CurrentState with
        {
            RuntimeStateVersion = request.CurrentState.RuntimeStateVersion + 1
        };

        return new WerewolfCombatSilverResult(
            true,
            updatedState,
            findings,
            request.RequestId,
            isVulnerable,
            isRacialForm,
            silverDamagePerTurn,
            gnosePenalty);
    }

    public static int ApplySilverContact(WerewolfRuntimeCharacterState currentState, int turnsOfContact, bool hasGiftsOrFetishes)
    {
        ArgumentNullException.ThrowIfNull(currentState);

        if (turnsOfContact <= 0)
        {
            return 0;
        }

        var currentForm = currentState.CurrentForm;
        var birthRace = currentState.BirthRace;
        var isRacialForm = string.Equals(currentForm, birthRace switch
        {
            WerewolfRaceIdentifiers.Homid => WerewolfFormIdentifiers.Homid,
            WerewolfRaceIdentifiers.Metis => WerewolfFormIdentifiers.Crinos,
            WerewolfRaceIdentifiers.Lupus => WerewolfFormIdentifiers.Lupus,
            _ => string.Empty
        }, StringComparison.Ordinal);

        if (isRacialForm)
        {
            return 0;
        }

        return turnsOfContact;
    }
}
