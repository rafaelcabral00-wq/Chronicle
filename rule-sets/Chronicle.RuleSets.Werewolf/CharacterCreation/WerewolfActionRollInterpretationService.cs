namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfActionRollInterpretationFindingSeverity
{
    Information,
    Error
}

public sealed record WerewolfActionRollInterpretationFinding(
    WerewolfActionRollInterpretationFindingSeverity Severity,
    string Code,
    string Message);

public sealed record WerewolfActionRollInterpretationRequest(
    string RequestId,
    IReadOnlyList<int> DiceValues,
    int Difficulty,
    int DiceQuantity);

public sealed record WerewolfActionRollInterpretationResult(
    bool Succeeded,
    IReadOnlyList<WerewolfActionRollInterpretationFinding> Findings,
    string RequestId,
    IReadOnlyList<int> RawDiceValues,
    int DiceQuantity,
    int Difficulty,
    int? SuccessCount,
    int? RawSuccesses,
    int? OnesCount,
    string? FailureClassification,
    string? BotchClassification,
    string InterpretationStatus,
    string SerializedInterpretation);

public static class WerewolfActionRollInterpretationService
{
    public const string SuccessStatus = "success";
    public const string FailureStatus = "failure";
    public const string BotchStatus = "botch";
    public const string ZeroPoolStatus = "zero-pool";

    public static WerewolfActionRollInterpretationResult Interpret(WerewolfActionRollInterpretationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.DiceValues);

        var findings = new List<WerewolfActionRollInterpretationFinding>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            findings.Add(new WerewolfActionRollInterpretationFinding(
                WerewolfActionRollInterpretationFindingSeverity.Error,
                "InvalidRequestId",
                "Request ID is required for interpretation."));
            return new WerewolfActionRollInterpretationResult(false, findings, string.Empty, [], 0, 0, null, null, null, null, null, "error", string.Empty);
        }

        if (request.DiceQuantity < 0)
        {
            findings.Add(new WerewolfActionRollInterpretationFinding(
                WerewolfActionRollInterpretationFindingSeverity.Error,
                "InvalidDiceQuantity",
                "Dice quantity must be non-negative."));
            return new WerewolfActionRollInterpretationResult(false, findings, request.RequestId, request.DiceValues, request.DiceQuantity, request.Difficulty, null, null, null, null, null, "error", string.Empty);
        }

        if (request.Difficulty < 2 || request.Difficulty > 10)
        {
            findings.Add(new WerewolfActionRollInterpretationFinding(
                WerewolfActionRollInterpretationFindingSeverity.Error,
                "InvalidDifficulty",
                "Difficulty must be between 2 and 10 (source line 2781)."));
            return new WerewolfActionRollInterpretationResult(false, findings, request.RequestId, request.DiceValues, request.DiceQuantity, request.Difficulty, null, null, null, null, null, "error", string.Empty);
        }

        if (request.DiceQuantity == 0)
        {
            findings.Add(new WerewolfActionRollInterpretationFinding(
                WerewolfActionRollInterpretationFindingSeverity.Error,
                "ZeroPoolCannotAttempt",
                "A character cannot attempt an action with a dice pool of zero or less (source line 2720)."));
            return new WerewolfActionRollInterpretationResult(false, findings, request.RequestId, request.DiceValues, request.DiceQuantity, request.Difficulty, 0, 0, 0, null, null, ZeroPoolStatus, string.Empty);
        }

        if (request.DiceValues.Count != request.DiceQuantity)
        {
            findings.Add(new WerewolfActionRollInterpretationFinding(
                WerewolfActionRollInterpretationFindingSeverity.Error,
                "DiceCountMismatch",
                $"Expected {request.DiceQuantity} dice values but received {request.DiceValues.Count}."));
            return new WerewolfActionRollInterpretationResult(false, findings, request.RequestId, request.DiceValues, request.DiceQuantity, request.Difficulty, null, null, null, null, null, "error", string.Empty);
        }

        foreach (var die in request.DiceValues)
        {
            if (die < 1 || die > 10)
            {
                findings.Add(new WerewolfActionRollInterpretationFinding(
                    WerewolfActionRollInterpretationFindingSeverity.Error,
                    "InvalidDieFace",
                    $"Die face {die} is out of bounds for d10."));
                return new WerewolfActionRollInterpretationResult(false, findings, request.RequestId, request.DiceValues, request.DiceQuantity, request.Difficulty, null, null, null, null, null, "error", string.Empty);
            }
        }

        var rawSuccesses = 0;
        var onesCount = 0;

        foreach (var die in request.DiceValues)
        {
            if (die >= request.Difficulty)
            {
                rawSuccesses++;
            }
            if (die == 1)
            {
                onesCount++;
            }
        }

        var finalSuccesses = Math.Max(0, rawSuccesses - onesCount);

        string status;
        string? failureClassification = null;
        string? botchClassification = null;

        if (finalSuccesses > 0)
        {
            status = SuccessStatus;
        }
        else if (onesCount > 0)
        {
            status = BotchStatus;
            botchClassification = "CriticalFailure";
        }
        else
        {
            status = FailureStatus;
            failureClassification = "NoSuccesses";
        }

        var serialized = System.Text.Json.JsonSerializer.Serialize(
            new
            {
                request.RequestId,
                RawDiceValues = request.DiceValues.ToArray(),
                request.Difficulty,
                request.DiceQuantity,
                RawSuccesses = rawSuccesses,
                OnesCount = onesCount,
                FinalSuccesses = finalSuccesses,
                InterpretationStatus = status,
                FailureClassification = failureClassification,
                BotchClassification = botchClassification,
                Findings = findings.Select(f => new { f.Code, f.Message }).ToArray()
            });

        return new WerewolfActionRollInterpretationResult(
            true,
            findings,
            request.RequestId,
            request.DiceValues,
            request.DiceQuantity,
            request.Difficulty,
            finalSuccesses,
            rawSuccesses,
            onesCount,
            failureClassification,
            botchClassification,
            status,
            serialized);
    }
}
