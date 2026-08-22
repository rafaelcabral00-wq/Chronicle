namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfCombatBowResult(
    bool IsHeartShotAttempt,
    int Difficulty,
    int RequiredSuccessesForHeart,
    int MinDamageAfterSoak,
    IReadOnlyList<string> Findings);

public static class WerewolfCombatBowService
{
    public const int HeartShotRequiredSuccesses = 5;
    public const int HeartShotMinDamageAfterSoak = 3;

    public static WerewolfCombatBowResult ResolveBowAttack(
        string requestId,
        bool isHeartShotAttempt,
        bool usesArcherySkill)
    {
        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(requestId))
        {
            return new WerewolfCombatBowResult(false, 0, 0, 0, ["RequestId is required"]);
        }

        var baseDifficulty = usesArcherySkill ? 6 : 7;
        var difficulty = baseDifficulty;

        if (isHeartShotAttempt)
        {
            difficulty += 2;
            findings.Add($"Heart shot attempt: difficulty {difficulty} (base {baseDifficulty} + 2). Requires {HeartShotRequiredSuccesses} successes and {HeartShotMinDamageAfterSoak} damage after soak.");
        }
        else
        {
            findings.Add($"Normal bow shot: difficulty {difficulty}.");
        }

        return new WerewolfCombatBowResult(
            isHeartShotAttempt,
            difficulty,
            HeartShotRequiredSuccesses,
            HeartShotMinDamageAfterSoak,
            findings);
    }
}
