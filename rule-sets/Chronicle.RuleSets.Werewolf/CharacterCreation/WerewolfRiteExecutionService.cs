using System.Text.Json;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfRiteExecutionService
{
    public static WerewolfRiteExecutionResult Execute(WerewolfRiteExecutionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<WerewolfRiteFinding>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            findings.Add(new WerewolfRiteFinding("InvalidRequestId", "RequestId is required.", WerewolfRiteFindingSeverity.Error));
            return new WerewolfRiteExecutionResult(false, findings, string.Empty, string.Empty, 0, 0, 0, string.Empty, null);
        }

        if (string.IsNullOrWhiteSpace(request.RiteKey))
        {
            findings.Add(new WerewolfRiteFinding("InvalidRiteKey", "RiteKey is required.", WerewolfRiteFindingSeverity.Error));
            return new WerewolfRiteExecutionResult(false, findings, request.RequestId, string.Empty, 0, 0, 0, string.Empty, null);
        }

        var definition = WerewolfRiteCatalog.Get(request.RiteKey);
        if (definition is null)
        {
            findings.Add(new WerewolfRiteFinding("UnknownRite", $"Unknown rite: {request.RiteKey}", WerewolfRiteFindingSeverity.Error));
            return new WerewolfRiteExecutionResult(false, findings, request.RequestId, request.RiteKey, 0, 0, 0, string.Empty, null);
        }

        if (request.DiceValues is null || request.DiceValues.Count == 0)
        {
            findings.Add(new WerewolfRiteFinding("InvalidDiceValues", "DiceValues is required and must not be empty.", WerewolfRiteFindingSeverity.Error));
            return new WerewolfRiteExecutionResult(false, findings, request.RequestId, request.RiteKey, 0, definition.BaseDifficulty, 0, string.Empty, null);
        }

        var difficulty = definition.BaseDifficulty;
        if (request.HasTargetPiece)
        {
            difficulty = Math.Max(2, difficulty - 1);
            findings.Add(new WerewolfRiteFinding("TargetPieceModifier", "Target piece possessed: difficulty reduced by 1.", WerewolfRiteFindingSeverity.Information));
        }

        if (difficulty < 2 || difficulty > 10)
        {
            findings.Add(new WerewolfRiteFinding("InvalidDifficulty", $"Difficulty {difficulty} is out of bounds (2-10).", WerewolfRiteFindingSeverity.Error));
            return new WerewolfRiteExecutionResult(false, findings, request.RequestId, request.RiteKey, request.DiceValues.Count, definition.BaseDifficulty, 0, string.Empty, null);
        }

        foreach (var die in request.DiceValues)
        {
            if (die < 1 || die > 10)
            {
                findings.Add(new WerewolfRiteFinding("InvalidDieFace", $"Die face {die} is out of bounds for d10.", WerewolfRiteFindingSeverity.Error));
                return new WerewolfRiteExecutionResult(false, findings, request.RequestId, request.RiteKey, request.DiceValues.Count, definition.BaseDifficulty, 0, string.Empty, null);
            }
        }

        var interpretation = WerewolfActionRollInterpretationService.Interpret(
            new WerewolfActionRollInterpretationRequest(
                request.RequestId,
                request.DiceValues,
                difficulty,
                request.DiceValues.Count));

        findings.AddRange(interpretation.Findings.Select(f => new WerewolfRiteFinding(
            f.Code,
            f.Message,
            f.Severity == WerewolfActionRollInterpretationFindingSeverity.Error
                ? WerewolfRiteFindingSeverity.Error
                : WerewolfRiteFindingSeverity.Information)));

        if (!interpretation.Succeeded)
        {
            return new WerewolfRiteExecutionResult(
                false,
                findings,
                request.RequestId,
                request.RiteKey,
                request.DiceValues.Count,
                difficulty,
                0,
                interpretation.InterpretationStatus,
                null);
        }

        var successCount = interpretation.SuccessCount ?? 0;
        var effect = successCount > 0
            ? definition.EffectDescription
            : "No information gained.";

        var serialized = JsonSerializer.Serialize(
            new
            {
                request.RequestId,
                definition.Key,
                definition.DisplayName,
                definition.Category,
                definition.Level,
                definition.AttributeId,
                definition.AbilityId,
                BaseDifficulty = definition.BaseDifficulty,
                EffectiveDifficulty = difficulty,
                DicePool = request.DiceValues.Count,
                SuccessCount = successCount,
                InterpretationStatus = interpretation.InterpretationStatus,
                Effect = effect,
                HasTargetPiece = request.HasTargetPiece,
                Findings = findings.Select(f => new { f.Code, f.Message, f.Severity }).ToArray()
            });

        return new WerewolfRiteExecutionResult(
            true,
            findings,
            request.RequestId,
            request.RiteKey,
            request.DiceValues.Count,
            difficulty,
            successCount,
            interpretation.InterpretationStatus,
            effect);
    }
}
