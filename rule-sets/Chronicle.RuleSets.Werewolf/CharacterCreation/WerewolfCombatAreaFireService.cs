namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfCombatAreaFireResult(
    bool CanUseAreaFire,
    int TargetCount,
    int BaseDifficulty,
    IReadOnlyList<int> DistributedSuccesses,
    IReadOnlyList<string> Findings);

public static class WerewolfCombatAreaFireService
{
    public static WerewolfCombatAreaFireResult DistributeAreaFireSuccesses(
        string requestId,
        int totalSuccesses,
        int targetCount,
        int baseDifficulty)
    {
        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(requestId))
        {
            return new WerewolfCombatAreaFireResult(false, 0, 0, [], ["RequestId is required"]);
        }

        if (targetCount < 2)
        {
            return new WerewolfCombatAreaFireResult(false, 0, baseDifficulty, [], ["Area fire requires at least 2 targets"]);
        }

        if (totalSuccesses < 1)
        {
            return new WerewolfCombatAreaFireResult(true, targetCount, baseDifficulty, Enumerable.Repeat(0, targetCount).ToList(), findings);
        }

        var baseDistribution = totalSuccesses / targetCount;
        var remainder = totalSuccesses % targetCount;
        var distributed = new List<int>();

        for (var i = 0; i < targetCount; i++)
        {
            distributed.Add(baseDistribution + (i < remainder ? 1 : 0));
        }

        findings.Add($"Area fire distributed {totalSuccesses} successes across {targetCount} targets: {string.Join(", ", distributed)}.");

        return new WerewolfCombatAreaFireResult(true, targetCount, baseDifficulty, distributed, findings);
    }
}
