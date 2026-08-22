namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfCombatMultipleShotResult(
    int MaxShotsAllowed,
    int ShotsFired,
    bool RequiresMultipleActions,
    int FuryCost,
    IReadOnlyList<string> Findings);

public static class WerewolfCombatMultipleShotService
{
    public static WerewolfCombatMultipleShotResult ResolveMultipleShots(
        string requestId,
        int rateOfFire,
        int availableActions,
        int availableFury,
        int requestedShots)
    {
        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(requestId))
        {
            return new WerewolfCombatMultipleShotResult(0, 0, false, 0, ["RequestId is required"]);
        }

        if (rateOfFire < 1)
        {
            return new WerewolfCombatMultipleShotResult(0, 0, false, 0, ["Rate of Fire must be at least 1"]);
        }

        if (requestedShots < 1)
        {
            return new WerewolfCombatMultipleShotResult(0, 0, false, 0, ["Requested shots must be at least 1"]);
        }

        var maxByRoF = rateOfFire;
        var maxByActions = Math.Max(0, availableActions);
        var maxAllowed = Math.Min(maxByRoF, maxByActions);

        if (requestedShots > maxAllowed && availableFury > 0)
        {
            var furyNeeded = requestedShots - maxAllowed;
            var furyCost = Math.Min(furyNeeded, availableFury);
            maxAllowed += furyCost;
            findings.Add($"Fury spent: {furyCost} to enable additional shots.");
        }

        var shotsFired = Math.Min(requestedShots, maxAllowed);
        var requiresMultipleActions = shotsFired > 1;

        findings.Add($"Multiple shots resolved: {shotsFired} of {requestedShots} requested (RoF {rateOfFire}, actions {availableActions}).");

        return new WerewolfCombatMultipleShotResult(
            maxAllowed,
            shotsFired,
            requiresMultipleActions,
            0,
            findings);
    }
}
