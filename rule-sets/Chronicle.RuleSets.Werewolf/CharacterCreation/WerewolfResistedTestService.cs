using System.Text.Json;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfResistedTestService
{
    public static WerewolfResistedTestResult Interpret(
        WerewolfResistedTestDefinition definition,
        IReadOnlyList<int> sideADice,
        IReadOnlyList<int> sideBDice)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(sideADice);
        ArgumentNullException.ThrowIfNull(sideBDice);

        var findings = new List<WerewolfResistedTestFinding>();

        if (!ValidateDefinition(definition, findings))
        {
            return new WerewolfResistedTestResult(
                false,
                findings,
                definition.RequestId,
                0,
                0,
                0,
                WerewolfResistedTestWinner.None,
                string.Empty,
                string.Empty);
        }

        if (sideADice.Count != definition.SideADicePool)
        {
            findings.Add(new WerewolfResistedTestFinding(WerewolfResistedTestFindingSeverity.Error, "SideADiceCountMismatch", $"Expected {definition.SideADicePool} dice values for side A but received {sideADice.Count}."));
            return new WerewolfResistedTestResult(
                false,
                findings,
                definition.RequestId,
                0,
                0,
                0,
                WerewolfResistedTestWinner.None,
                string.Empty,
                string.Empty);
        }

        if (sideBDice.Count != definition.SideBDicePool)
        {
            findings.Add(new WerewolfResistedTestFinding(WerewolfResistedTestFindingSeverity.Error, "SideBDiceCountMismatch", $"Expected {definition.SideBDicePool} dice values for side B but received {sideBDice.Count}."));
            return new WerewolfResistedTestResult(
                false,
                findings,
                definition.RequestId,
                0,
                0,
                0,
                WerewolfResistedTestWinner.None,
                string.Empty,
                string.Empty);
        }

        foreach (var die in sideADice)
        {
            if (die < 1 || die > 10)
            {
                findings.Add(new WerewolfResistedTestFinding(WerewolfResistedTestFindingSeverity.Error, "InvalidSideADieFace", $"Side A die face {die} is out of bounds for d10."));
                return new WerewolfResistedTestResult(
                    false,
                    findings,
                    definition.RequestId,
                    0,
                    0,
                    0,
                    WerewolfResistedTestWinner.None,
                    string.Empty,
                    string.Empty);
            }
        }

        foreach (var die in sideBDice)
        {
            if (die < 1 || die > 10)
            {
                findings.Add(new WerewolfResistedTestFinding(WerewolfResistedTestFindingSeverity.Error, "InvalidSideBDieFace", $"Side B die face {die} is out of bounds for d10."));
                return new WerewolfResistedTestResult(
                    false,
                    findings,
                    definition.RequestId,
                    0,
                    0,
                    0,
                    WerewolfResistedTestWinner.None,
                    string.Empty,
                    string.Empty);
            }
        }

        var interpretationA = WerewolfActionRollInterpretationService.Interpret(
            new WerewolfActionRollInterpretationRequest(
                definition.RequestId + "-a",
                sideADice,
                definition.SideADifficulty,
                definition.SideADicePool));

        var interpretationB = WerewolfActionRollInterpretationService.Interpret(
            new WerewolfActionRollInterpretationRequest(
                definition.RequestId + "-b",
                sideBDice,
                definition.SideBDifficulty,
                definition.SideBDicePool));

        findings.AddRange(interpretationA.Findings.Select(f => new WerewolfResistedTestFinding(
            WerewolfResistedTestFindingSeverity.Information,
            f.Code + "-side-a",
            $"[Side A] {f.Message}")));

        findings.AddRange(interpretationB.Findings.Select(f => new WerewolfResistedTestFinding(
            WerewolfResistedTestFindingSeverity.Information,
            f.Code + "-side-b",
            $"[Side B] {f.Message}")));

        if (!interpretationA.Succeeded || !interpretationB.Succeeded)
        {
            return new WerewolfResistedTestResult(
                false,
                findings,
                definition.RequestId,
                0,
                0,
                0,
                WerewolfResistedTestWinner.None,
                string.Empty,
                string.Empty);
        }

        var sideASuccesses = interpretationA.SuccessCount ?? 0;
        var sideBSuccesses = interpretationB.SuccessCount ?? 0;
        var sideAOnes = interpretationA.OnesCount ?? 0;
        var sideBOnes = interpretationB.OnesCount ?? 0;
        var netSuccesses = sideASuccesses - sideBSuccesses;

        var sideAStatus = interpretationA.InterpretationStatus;
        var sideBStatus = interpretationB.InterpretationStatus;

        var sideABotched = sideAStatus == WerewolfActionRollInterpretationService.BotchStatus;
        var sideBBotched = sideBStatus == WerewolfActionRollInterpretationService.BotchStatus;
        var sideAFailed = sideAStatus == WerewolfActionRollInterpretationService.FailureStatus;
        var sideBFailed = sideBStatus == WerewolfActionRollInterpretationService.FailureStatus;

        string status;
        if (sideABotched && sideBBotched)
        {
            status = "both-botch";
        }
        else if (sideABotched)
        {
            status = "side-a-botch";
        }
        else if (sideBBotched)
        {
            status = "side-b-botch";
        }
        else if (sideAFailed && sideBFailed)
        {
            status = "both-fail";
        }
        else if (sideASuccesses > 0 && sideBSuccesses > 0)
        {
            if (netSuccesses > 0)
            {
                status = "side-a-wins";
            }
            else if (netSuccesses < 0)
            {
                status = "side-b-wins";
            }
            else
            {
                status = "tie";
            }
        }
        else if (sideASuccesses > 0)
        {
            status = "side-a-wins";
        }
        else if (sideBSuccesses > 0)
        {
            status = "side-b-wins";
        }
        else
        {
            status = "both-fail";
        }

        var winner = WerewolfResistedTestWinner.None;
        if (netSuccesses > 0)
        {
            winner = WerewolfResistedTestWinner.SideA;
        }
        else if (netSuccesses < 0)
        {
            winner = WerewolfResistedTestWinner.SideB;
        }
        else if (netSuccesses == 0 && sideASuccesses > 0 && sideBSuccesses > 0)
        {
            winner = WerewolfResistedTestWinner.Tie;
        }

        var serialized = JsonSerializer.Serialize(
            new
            {
                definition.RequestId,
                SideARawDiceValues = sideADice.ToArray(),
                SideBRawDiceValues = sideBDice.ToArray(),
                SideADifficulty = definition.SideADifficulty,
                SideBDifficulty = definition.SideBDifficulty,
                SideASuccesses = sideASuccesses,
                SideBSuccesses = sideBSuccesses,
                NetSuccesses = netSuccesses,
                SideARawSuccesses = interpretationA.RawSuccesses,
                SideBRawSuccesses = interpretationB.RawSuccesses,
                SideAOnesCount = sideAOnes,
                SideBOnesCount = sideBOnes,
                Winner = winner.ToString(),
                Status = status,
                Findings = findings.Select(f => new { f.Code, f.Message }).ToArray()
            });

        return new WerewolfResistedTestResult(
            true,
            findings,
            definition.RequestId,
            sideASuccesses,
            sideBSuccesses,
            netSuccesses,
            winner,
            status,
            serialized);
    }

    private static bool ValidateDefinition(WerewolfResistedTestDefinition definition, List<WerewolfResistedTestFinding> findings)
    {
        if (string.IsNullOrWhiteSpace(definition.RequestId))
        {
            findings.Add(new WerewolfResistedTestFinding(WerewolfResistedTestFindingSeverity.Error, "InvalidRequestId", "Request ID is required."));
            return false;
        }

        if (definition.SideADifficulty < 2 || definition.SideADifficulty > 10)
        {
            findings.Add(new WerewolfResistedTestFinding(WerewolfResistedTestFindingSeverity.Error, "InvalidSideADifficulty", "Side A difficulty must be between 2 and 10."));
            return false;
        }

        if (definition.SideBDifficulty < 2 || definition.SideBDifficulty > 10)
        {
            findings.Add(new WerewolfResistedTestFinding(WerewolfResistedTestFindingSeverity.Error, "InvalidSideBDifficulty", "Side B difficulty must be between 2 and 10."));
            return false;
        }

        if (definition.SideADicePool < 0)
        {
            findings.Add(new WerewolfResistedTestFinding(WerewolfResistedTestFindingSeverity.Error, "InvalidSideADicePool", "Side A dice pool must be non-negative."));
            return false;
        }

        if (definition.SideBDicePool < 0)
        {
            findings.Add(new WerewolfResistedTestFinding(WerewolfResistedTestFindingSeverity.Error, "InvalidSideBDicePool", "Side B dice pool must be non-negative."));
            return false;
        }

        return true;
    }
}
