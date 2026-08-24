using System.Text.Json;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfExtendedTestService
{
    public static WerewolfExtendedTestProgress CreateInitialProgress(WerewolfExtendedTestDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var findings = new List<WerewolfExtendedTestFinding>();
        if (!ValidateDefinition(definition, findings))
        {
            return new WerewolfExtendedTestProgress(
                definition.RequestId,
                0,
                0,
                false,
                WerewolfExtendedTestStatus.Failed);
        }

        return new WerewolfExtendedTestProgress(
            definition.RequestId,
            0,
            0,
            false,
            WerewolfExtendedTestStatus.InProgress);
    }

    public static WerewolfExtendedTestResult Advance(
        WerewolfExtendedTestDefinition definition,
        WerewolfExtendedTestProgress progress,
        IReadOnlyList<int> diceValues)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(diceValues);

        var findings = new List<WerewolfExtendedTestFinding>();

        if (!ValidateDefinition(definition, findings))
        {
            return new WerewolfExtendedTestResult(
                false,
                findings,
                progress.RequestId,
                progress,
                progress.Status,
                string.Empty);
        }

        if (progress.Status == WerewolfExtendedTestStatus.Completed || progress.Status == WerewolfExtendedTestStatus.Botched)
        {
            findings.Add(new WerewolfExtendedTestFinding(WerewolfExtendedTestFindingSeverity.Error, "TestAlreadyTerminal", "Extended test is already in a terminal state."));
            return new WerewolfExtendedTestResult(
                false,
                findings,
                progress.RequestId,
                progress,
                progress.Status,
                string.Empty);
        }

        var interpretation = WerewolfActionRollInterpretationService.Interpret(
            new WerewolfActionRollInterpretationRequest(
                progress.RequestId,
                diceValues,
                definition.Difficulty,
                definition.DicePool));

        findings.AddRange(interpretation.Findings.Select(f => new WerewolfExtendedTestFinding(
            f.Severity == WerewolfActionRollInterpretationFindingSeverity.Error
                ? WerewolfExtendedTestFindingSeverity.Error
                : WerewolfExtendedTestFindingSeverity.Information,
            f.Code,
            f.Message)));

        if (!interpretation.Succeeded)
        {
            return new WerewolfExtendedTestResult(
                false,
                findings,
                progress.RequestId,
                progress,
                progress.Status,
                string.Empty);
        }

        var updatedProgress = progress;
        var finalSuccesses = interpretation.SuccessCount ?? 0;
        var onesCount = interpretation.OnesCount ?? 0;

        if (finalSuccesses == 0 && onesCount > 0)
        {
            updatedProgress = progress with
            {
                IsBotched = true,
                Status = WerewolfExtendedTestStatus.Botched,
                AccumulatedSuccesses = 0
            };
        }
        else if (finalSuccesses > 0)
        {
            var newAccumulated = progress.AccumulatedSuccesses + finalSuccesses;
            var newStatus = newAccumulated >= definition.RequiredSuccesses
                ? WerewolfExtendedTestStatus.Completed
                : WerewolfExtendedTestStatus.InProgress;

            updatedProgress = progress with
            {
                AccumulatedSuccesses = newAccumulated,
                Status = newStatus
            };
        }
        else
        {
            updatedProgress = progress with
            {
                AttemptCount = progress.AttemptCount + 1
            };
        }

        var serialized = JsonSerializer.Serialize(
            new
            {
                progress.RequestId,
                PreviousAccumulatedSuccesses = progress.AccumulatedSuccesses,
                NewAccumulatedSuccesses = updatedProgress.AccumulatedSuccesses,
                PreviousAttemptCount = progress.AttemptCount,
                NewAttemptCount = updatedProgress.AttemptCount,
                FinalSuccesses = finalSuccesses,
                OnesCount = onesCount,
                UpdatedStatus = updatedProgress.Status.ToString(),
                IsBotched = updatedProgress.IsBotched,
                Findings = findings.Select(f => new { f.Code, f.Message }).ToArray()
            });

        return new WerewolfExtendedTestResult(
            true,
            findings,
            progress.RequestId,
            updatedProgress,
            updatedProgress.Status,
            serialized);
    }

    private static bool ValidateDefinition(WerewolfExtendedTestDefinition definition, List<WerewolfExtendedTestFinding> findings)
    {
        if (string.IsNullOrWhiteSpace(definition.RequestId))
        {
            findings.Add(new WerewolfExtendedTestFinding(WerewolfExtendedTestFindingSeverity.Error, "InvalidRequestId", "Request ID is required."));
            return false;
        }

        if (definition.RequiredSuccesses <= 0)
        {
            findings.Add(new WerewolfExtendedTestFinding(WerewolfExtendedTestFindingSeverity.Error, "InvalidRequiredSuccesses", "Required successes must be greater than zero."));
            return false;
        }

        if (definition.Difficulty < 2 || definition.Difficulty > 10)
        {
            findings.Add(new WerewolfExtendedTestFinding(WerewolfExtendedTestFindingSeverity.Error, "InvalidDifficulty", "Difficulty must be between 2 and 10."));
            return false;
        }

        if (definition.DicePool < 0)
        {
            findings.Add(new WerewolfExtendedTestFinding(WerewolfExtendedTestFindingSeverity.Error, "InvalidDicePool", "Dice pool must be non-negative."));
            return false;
        }

        return true;
    }
}
