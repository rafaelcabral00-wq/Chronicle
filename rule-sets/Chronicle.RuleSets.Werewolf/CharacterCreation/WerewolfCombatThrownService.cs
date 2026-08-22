namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfCombatThrownResult(
    int Difficulty,
    int DamagePoolBonus,
    IReadOnlyList<string> Findings);

public static class WerewolfCombatThrownService
{
    public static WerewolfCombatThrownResult ResolveThrownAttack(
        string requestId,
        int strength,
        bool isHeavyObject)
    {
        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(requestId))
        {
            return new WerewolfCombatThrownResult(0, 0, ["RequestId is required"]);
        }

        var baseDifficulty = 6;
        var damageBonus = isHeavyObject ? Math.Max(0, strength) : 0;

        findings.Add($"Thrown attack: difficulty {baseDifficulty}, Strength bonus {damageBonus} for heavy object.");

        return new WerewolfCombatThrownResult(baseDifficulty, damageBonus, findings);
    }
}
