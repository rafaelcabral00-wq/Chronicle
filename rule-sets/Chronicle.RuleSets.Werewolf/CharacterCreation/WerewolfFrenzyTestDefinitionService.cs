namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfFrenzyTestDefinitionRequest(
    string RequestId,
    int RagePermanent,
    int WillpowerPermanent,
    int Rank,
    string? CurrentForm,
    string? AuspiceMoon,
    string? MoonPhase,
    string? EnvironmentalModifier);

public sealed record WerewolfFrenzyTestDefinitionResult(
    string RequestId,
    int DicePool,
    int BaseDifficulty,
    int FinalDifficulty,
    int DifficultyModifier,
    int SuccessThreshold,
    IReadOnlyList<string> Findings,
    bool IsValid);

public static class WerewolfFrenzyTestDefinitionService
{
    public static WerewolfFrenzyTestDefinitionResult ComputeTestDefinition(
        string requestId,
        int ragePermanent,
        int willpowerPermanent,
        int rank,
        string? currentForm,
        string? auspiceMoon,
        string? moonPhase,
        string? environmentalModifier)
    {
        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(requestId))
        {
            return new WerewolfFrenzyTestDefinitionResult(string.Empty, 0, 0, 0, 0, 0, ["RequestId is required."], false);
        }

        if (ragePermanent < 1)
        {
            return new WerewolfFrenzyTestDefinitionResult(requestId, 0, 0, 0, 0, 0, ["RagePermanent must be at least 1."], false);
        }

        if (rank < 1 || rank > 5)
        {
            return new WerewolfFrenzyTestDefinitionResult(requestId, 0, 0, 0, 0, 0, ["Rank must be between 1 and 5."], false);
        }

        var dicePool = ragePermanent;
        var difficultyModifier = 0;

        var baseDifficulty = moonPhase switch
        {
            "new" => 8,
            "waxing-crescent" => 7,
            "half" => 6,
            "waxing-gibbous" => 5,
            "full" => 4,
            _ => 6
        };

        findings.Add($"Base difficulty from moon phase '{moonPhase ?? "half"}': {baseDifficulty}.");

        if (rank == 1)
        {
            difficultyModifier += 1;
            findings.Add("Rank 1: difficulty +1.");
        }
        else if (rank >= 2)
        {
            difficultyModifier += 2;
            findings.Add($"Rank {rank}: difficulty +2.");

            if (rank >= 3)
            {
                findings.Add($"Rank {rank}: 5+ successes required to trigger frenzy.");
            }

            if (rank >= 4)
            {
                findings.Add($"Rank {rank}: 6+ successes required to trigger frenzy.");
            }
        }

        var auspiceMatches = !string.IsNullOrWhiteSpace(auspiceMoon) &&
            StringComparer.OrdinalIgnoreCase.Equals(auspiceMoon, moonPhase);
        if (auspiceMatches)
        {
            difficultyModifier -= 1;
            findings.Add("Auspice moon matches current moon phase: difficulty -1.");
        }

        var isCrinos = StringComparer.OrdinalIgnoreCase.Equals(currentForm, WerewolfFormIdentifiers.Crinos);
        if (isCrinos)
        {
            difficultyModifier -= 1;
            findings.Add("Character is in Crinos form: difficulty -1.");
        }

        if (!string.IsNullOrWhiteSpace(environmentalModifier) &&
            int.TryParse(environmentalModifier, out var envMod))
        {
            difficultyModifier += envMod;
            findings.Add($"Environmental modifier: difficulty {envMod:+0;-0;0}.");
        }

        var finalDifficulty = Math.Max(2, Math.Min(10, baseDifficulty + difficultyModifier));
        findings.Add($"Final difficulty: {finalDifficulty} (base {baseDifficulty} + modifier {difficultyModifier}).");

        var successThreshold = rank >= 4 ? 6 : rank >= 3 ? 5 : 4;
        findings.Add($"Success threshold: {successThreshold} successes required to trigger frenzy.");

        return new WerewolfFrenzyTestDefinitionResult(
            string.Empty,
            dicePool,
            baseDifficulty,
            finalDifficulty,
            difficultyModifier,
            successThreshold,
            findings,
            true);
    }
}
