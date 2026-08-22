namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfCombatAutomaticFireResult(
    bool CanUseAutomaticFire,
    int AttackDiceBonus,
    int DifficultyPenalty,
    int AmmunitionRequired,
    IReadOnlyList<string> Findings);

public static class WerewolfCombatAutomaticFireService
{
    public const int AutomaticFireDiceBonus = 10;
    public const int AutomaticFireDifficultyPenalty = 2;
    public const int MinimumAmmunitionFraction = 2;

    public static WerewolfCombatAutomaticFireResult ResolveAutomaticFire(
        string requestId,
        int currentAmmunition,
        int totalAmmunitionCapacity)
    {
        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(requestId))
        {
            return new WerewolfCombatAutomaticFireResult(false, 0, 0, 0, ["RequestId is required"]);
        }

        if (totalAmmunitionCapacity < 1)
        {
            return new WerewolfCombatAutomaticFireResult(false, 0, 0, 0, ["Total ammunition capacity must be at least 1"]);
        }

        var minimumRequired = (int)Math.Ceiling(totalAmmunitionCapacity / (double)MinimumAmmunitionFraction);
        var canUse = currentAmmunition >= minimumRequired;

        if (canUse)
        {
            findings.Add($"Automatic fire available: {currentAmmunition}/{totalAmmunitionCapacity} ammunition (requires {minimumRequired}).");
        }
        else
        {
            findings.Add($"Automatic fire unavailable: {currentAmmunition}/{totalAmmunitionCapacity} ammunition (requires {minimumRequired}).");
        }

        return new WerewolfCombatAutomaticFireResult(
            canUse,
            AutomaticFireDiceBonus,
            AutomaticFireDifficultyPenalty,
            totalAmmunitionCapacity,
            findings);
    }
}
