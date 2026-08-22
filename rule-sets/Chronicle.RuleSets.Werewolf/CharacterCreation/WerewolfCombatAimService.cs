namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfCombatAimDefinition(
    string RequestId,
    int AimTurns,
    int PerceptionCap,
    bool HasScope,
    int EffectiveAimDice,
    IReadOnlyList<string> Findings);

public static class WerewolfCombatAimService
{
    public static WerewolfCombatAimDefinition DefineAim(
        string requestId,
        int aimTurns,
        int perception,
        bool hasScope)
    {
        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(requestId))
        {
            return new WerewolfCombatAimDefinition(string.Empty, 0, 0, false, 0, ["RequestId is required"]);
        }

        if (aimTurns < 0)
        {
            return new WerewolfCombatAimDefinition(requestId, 0, 0, false, 0, ["AimTurns must be non-negative"]);
        }

        if (perception < 1)
        {
            return new WerewolfCombatAimDefinition(requestId, 0, 0, false, 0, ["Perception must be at least 1"]);
        }

        var cap = perception;
        var effectiveAimTurns = Math.Min(aimTurns, cap);
        var scopeBonus = hasScope ? 2 : 0;
        var effectiveAimDice = effectiveAimTurns + scopeBonus;

        findings.Add($"Aim defined: {effectiveAimTurns} turns (capped at {cap}) + scope {scopeBonus} = {effectiveAimDice} dice.");

        return new WerewolfCombatAimDefinition(
            requestId,
            effectiveAimTurns,
            cap,
            hasScope,
            effectiveAimDice,
            findings);
    }
}
