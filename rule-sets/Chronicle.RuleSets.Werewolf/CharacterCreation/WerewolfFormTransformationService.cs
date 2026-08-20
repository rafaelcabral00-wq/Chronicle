namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfFormTransformationService
{
    private static readonly Dictionary<string, int> FormDistances = new(StringComparer.Ordinal)
    {
        [WerewolfFormIdentifiers.Homid] = 0,
        [WerewolfFormIdentifiers.Glabro] = 1,
        [WerewolfFormIdentifiers.Crinos] = 2,
        [WerewolfFormIdentifiers.Hispo] = 3,
        [WerewolfFormIdentifiers.Lupus] = 4
    };

    private static readonly Dictionary<string, int> TransformationBaseDifficulties = new(StringComparer.Ordinal)
    {
        [WerewolfFormIdentifiers.Homid] = 6,
        [WerewolfFormIdentifiers.Glabro] = 7,
        [WerewolfFormIdentifiers.Crinos] = 6,
        [WerewolfFormIdentifiers.Hispo] = 7,
        [WerewolfFormIdentifiers.Lupus] = 6
    };

    public static WerewolfFormTransformationResult Transform(WerewolfFormTransformationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<WerewolfFormTransformationFinding>();

        if (request.CurrentState is null)
        {
            return Invalid(WerewolfFormTransformationErrorCode.MissingState, "Transformation requires a current runtime state.", findings, null, null);
        }

        if (string.IsNullOrWhiteSpace(request.TargetFormId))
        {
            return Invalid(WerewolfFormTransformationErrorCode.InvalidTargetForm, "Target form is required.", findings, null, null);
        }

        var targetForm = request.TargetFormId.Trim();
        if (!StringComparer.Ordinal.Equals(targetForm, request.TargetFormId) || targetForm.Any(char.IsWhiteSpace))
        {
            return Invalid(WerewolfFormTransformationErrorCode.InvalidTargetForm, "Target form identifier must be canonical and whitespace-free.", findings, null, null);
        }

        if (!WerewolfFormIdentifiers.Supported.Contains(targetForm, StringComparer.Ordinal))
        {
            return Invalid(WerewolfFormTransformationErrorCode.InvalidTargetForm, $"Target form '{targetForm}' is not declared by the current slice.", findings, null, null);
        }

        var currentForm = request.CurrentState.CurrentForm;
        if (string.IsNullOrWhiteSpace(currentForm))
        {
            return Invalid(WerewolfFormTransformationErrorCode.MissingState, "Current runtime state does not have an active form.", findings, null, null);
        }

        if (StringComparer.Ordinal.Equals(currentForm, targetForm))
        {
            findings.Add(new WerewolfFormTransformationFinding(WerewolfFormTransformationFindingSeverity.Information, WerewolfFormTransformationErrorCode.SameForm, $"Already in target form '{targetForm}'. No transformation needed."));
            return new WerewolfFormTransformationResult(true, request.CurrentState, findings, request.CurrentState.RageCurrent, request.CurrentState.RuntimeStateVersion);
        }

        var currentDistance = FormDistances[currentForm];
        var targetDistance = FormDistances[targetForm];
        var distance = Math.Abs(targetDistance - currentDistance);

        var isNativeForm = IsNativeForm(request.CurrentState, targetForm);
        var rageSpent = 0;

        if (request.SpendRage && !isNativeForm)
        {
            if (request.CurrentState.RageCurrent < 1)
            {
                return Invalid(WerewolfFormTransformationErrorCode.InsufficientRage, "Transformation requires 1 Rage point but none are available.", findings, null, null);
            }

            rageSpent = 1;
        }

        if (!isNativeForm && rageSpent == 0)
        {
            var difficulty = TransformationBaseDifficulties[currentForm];
            var requiredSuccesses = distance;

            findings.Add(new WerewolfFormTransformationFinding(
                WerewolfFormTransformationFindingSeverity.Information,
                WerewolfFormTransformationErrorCode.TransformationSucceeded,
                $"Transformation from {currentForm} to {targetForm} requires test Vigor + Primal Instinct at difficulty {difficulty} with {requiredSuccesses} successes (distance {distance}). Chronicle owns randomness; Rule Set interprets result."));
        }
        else if (isNativeForm)
        {
            findings.Add(new WerewolfFormTransformationFinding(
                WerewolfFormTransformationFindingSeverity.Information,
                WerewolfFormTransformationErrorCode.TransformationSucceeded,
                $"Transformation to native form '{targetForm}' is automatic and instant."));
        }
        else if (rageSpent > 0)
        {
            findings.Add(new WerewolfFormTransformationFinding(
                WerewolfFormTransformationFindingSeverity.Information,
                WerewolfFormTransformationErrorCode.TransformationSucceeded,
                $"Transformation from {currentForm} to {targetForm} is automatic and instant via 1 Rage expenditure."));
        }

        var updatedRage = request.CurrentState.RageCurrent - rageSpent;
        var newState = request.CurrentState with
        {
            CurrentForm = targetForm,
            RageCurrent = updatedRage,
            RuntimeStateVersion = request.CurrentState.RuntimeStateVersion + 1
        };

        return new WerewolfFormTransformationResult(
            true,
            newState,
            findings,
            updatedRage,
            newState.RuntimeStateVersion);
    }

    private static bool IsNativeForm(WerewolfRuntimeCharacterState state, string targetForm)
    {
        if (string.IsNullOrWhiteSpace(state.BirthRace))
        {
            return false;
        }

        return state.BirthRace switch
        {
            WerewolfRaceIdentifiers.Homid => StringComparer.Ordinal.Equals(targetForm, WerewolfFormIdentifiers.Homid),
            WerewolfRaceIdentifiers.Metis => StringComparer.Ordinal.Equals(targetForm, WerewolfFormIdentifiers.Crinos),
            WerewolfRaceIdentifiers.Lupus => StringComparer.Ordinal.Equals(targetForm, WerewolfFormIdentifiers.Lupus),
            _ => false
        };
    }

    private static WerewolfFormTransformationResult Invalid(WerewolfFormTransformationErrorCode code, string message, List<WerewolfFormTransformationFinding> findings, int? remainingRage, int? newVersion)
    {
        findings.Add(new WerewolfFormTransformationFinding(WerewolfFormTransformationFindingSeverity.Error, code, message));
        return new WerewolfFormTransformationResult(false, null, findings, remainingRage, newVersion);
    }
}
