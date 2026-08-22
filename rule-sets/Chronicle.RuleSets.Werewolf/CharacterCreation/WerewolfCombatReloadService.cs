namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfCombatReloadResult(
    bool RequiresReloadAction,
    int AttackDicePenalty,
    bool CanReloadAndFireSameTurn,
    int SpareClips,
    IReadOnlyList<string> Findings);

public static class WerewolfCombatReloadService
{
    public const int ReloadDicePenalty = 2;

    public static WerewolfCombatReloadResult ResolveReload(
        string requestId,
        bool hasSpareClips,
        bool isManualRevolver)
    {
        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(requestId))
        {
            return new WerewolfCombatReloadResult(false, 0, false, 0, ["RequestId is required"]);
        }

        if (isManualRevolver)
        {
            findings.Add("Manual revolver requires full turn of concentration for reload.");
            return new WerewolfCombatReloadResult(true, ReloadDicePenalty, false, hasSpareClips ? 1 : 0, findings);
        }

        if (hasSpareClips)
        {
            findings.Add($"Spare clips available: reload and fire same turn with -{ReloadDicePenalty} dice penalty.");
            return new WerewolfCombatReloadResult(true, ReloadDicePenalty, true, 1, findings);
        }

        findings.Add("No spare clips: reload requires full turn.");
        return new WerewolfCombatReloadResult(true, ReloadDicePenalty, false, 0, findings);
    }
}
