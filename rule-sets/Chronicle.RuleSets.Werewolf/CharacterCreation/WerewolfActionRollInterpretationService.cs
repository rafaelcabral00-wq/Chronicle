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
    string? FailureClassification,
    string? BotchClassification,
    string InterpretationStatus,
    string SerializedInterpretation);

public static class WerewolfActionRollInterpretationService
{
    public const string PendingExtractionStatus = "pending-extraction";

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
            return new WerewolfActionRollInterpretationResult(false, findings, string.Empty, [], 0, 0, null, null, null, "error", string.Empty);
        }

        if (request.DiceQuantity < 0)
        {
            findings.Add(new WerewolfActionRollInterpretationFinding(
                WerewolfActionRollInterpretationFindingSeverity.Error,
                "InvalidDiceQuantity",
                "Dice quantity must be non-negative."));
            return new WerewolfActionRollInterpretationResult(false, findings, request.RequestId, request.DiceValues, request.DiceQuantity, request.Difficulty, null, null, null, "error", string.Empty);
        }

        if (request.Difficulty < 1)
        {
            findings.Add(new WerewolfActionRollInterpretationFinding(
                WerewolfActionRollInterpretationFindingSeverity.Error,
                "InvalidDifficulty",
                "Difficulty must be a positive integer."));
            return new WerewolfActionRollInterpretationResult(false, findings, request.RequestId, request.DiceValues, request.DiceQuantity, request.Difficulty, null, null, null, "error", string.Empty);
        }

        if (request.DiceValues.Count != request.DiceQuantity)
        {
            findings.Add(new WerewolfActionRollInterpretationFinding(
                WerewolfActionRollInterpretationFindingSeverity.Error,
                "DiceCountMismatch",
                $"Expected {request.DiceQuantity} dice values but received {request.DiceValues.Count}."));
            return new WerewolfActionRollInterpretationResult(false, findings, request.RequestId, request.DiceValues, request.DiceQuantity, request.Difficulty, null, null, null, "error", string.Empty);
        }

        foreach (var die in request.DiceValues)
        {
            if (die < 1 || die > 10)
            {
                findings.Add(new WerewolfActionRollInterpretationFinding(
                    WerewolfActionRollInterpretationFindingSeverity.Error,
                    "InvalidDieFace",
                    $"Die face {die} is out of bounds for d10."));
                return new WerewolfActionRollInterpretationResult(false, findings, request.RequestId, request.DiceValues, request.DiceQuantity, request.Difficulty, null, null, null, "error", string.Empty);
            }
        }

        findings.Add(new WerewolfActionRollInterpretationFinding(
            WerewolfActionRollInterpretationFindingSeverity.Information,
            "InterpretationPendingExtraction",
            "Werewolf 3E success-counting semantics (threshold, botch, cancellation, specialization, 10-again) are pending extraction per EXTRACTION-0004 ambiguity A-001. Raw dice values are retained for future resolution."));

        var serialized = System.Text.Json.JsonSerializer.Serialize(
            new
            {
                request.RequestId,
                RawDiceValues = request.DiceValues.ToArray(),
                request.Difficulty,
                request.DiceQuantity,
                InterpretationStatus = PendingExtractionStatus,
                SuccessCount = (int?)null,
                FailureClassification = (string?)null,
                BotchClassification = (string?)null,
                Findings = findings.Select(f => new { f.Code, f.Message }).ToArray()
            });

        return new WerewolfActionRollInterpretationResult(
            true,
            findings,
            request.RequestId,
            request.DiceValues,
            request.DiceQuantity,
            request.Difficulty,
            null,
            null,
            null,
            PendingExtractionStatus,
            serialized);
    }
}
