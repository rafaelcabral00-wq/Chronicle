namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfCombatRangedRequest(
    string RequestId,
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    string AttackId,
    WerewolfCombatRangeBand RangeBand,
    int AimTurns,
    bool HasScope,
    WerewolfCombatCoverType CoverType,
    bool TargetIsMoving,
    WerewolfCombatFiringMode FiringMode,
    int RequestedShots,
    int? RateOfFire,
    int CurrentAmmunition,
    int TotalAmmunitionCapacity,
    bool HasSpareClips,
    bool IsManualRevolver,
    bool IsBowHeartShot);

public sealed record WerewolfCombatRangedResult(
    string RequestId,
    int BaseDifficulty,
    int RangeModifier,
    int AimDiceBonus,
    int ScopeDiceBonus,
    int CoverDifficultyModifier,
    int MovingTargetModifier,
    int AutomaticFireDiceBonus,
    int AutomaticFireDifficultyModifier,
    int ReloadDicePenalty,
    int FinalDifficulty,
    int FinalDiceBonus,
    bool IsBlocked,
    IReadOnlyList<string> Findings);

public static class WerewolfCombatRangedService
{
    public static WerewolfCombatRangedResult ResolveRangedCombat(WerewolfCombatRangedRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return new WerewolfCombatRangedResult(string.Empty, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, true, ["RequestId is required"]);
        }

        if (request.CurrentState is null)
        {
            return new WerewolfCombatRangedResult(request.RequestId, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, true, ["CurrentState is required"]);
        }

        if (request.ExpectedRuntimeStateVersion < 1)
        {
            return new WerewolfCombatRangedResult(request.RequestId, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, true, ["ExpectedRuntimeStateVersion must be >= 1"]);
        }

        if (request.CurrentState.RuntimeStateVersion != request.ExpectedRuntimeStateVersion)
        {
            return new WerewolfCombatRangedResult(request.RequestId, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, true, ["Version mismatch"]);
        }

        var baseDifficulty = 6;
        var rangeModifier = WerewolfCombatRangeService.GetRangeDifficultyModifier(request.RangeBand);
        var coverModifier = WerewolfCombatCoverService.GetCoverDifficultyModifier(request.CoverType);
        var movingModifier = WerewolfCombatMovingTargetService.GetMovingTargetDifficultyModifier(request.TargetIsMoving);

        var aimResult = WerewolfCombatAimService.DefineAim(request.RequestId, request.AimTurns, 3, request.HasScope);
        var aimDiceBonus = aimResult.EffectiveAimDice;
        var scopeDiceBonus = aimResult.HasScope ? 2 : 0;

        var automaticFireResult = WerewolfCombatAutomaticFireService.ResolveAutomaticFire(
            request.RequestId + "-auto",
            request.CurrentAmmunition,
            request.TotalAmmunitionCapacity);
        var autoFireDiceBonus = automaticFireResult.CanUseAutomaticFire && request.FiringMode == WerewolfCombatFiringMode.AutomaticFire
            ? WerewolfCombatAutomaticFireService.AutomaticFireDiceBonus
            : 0;
        var autoFireDifficultyModifier = automaticFireResult.CanUseAutomaticFire && request.FiringMode == WerewolfCombatFiringMode.AutomaticFire
            ? WerewolfCombatAutomaticFireService.AutomaticFireDifficultyPenalty
            : 0;

        var reloadResult = WerewolfCombatReloadService.ResolveReload(request.RequestId + "-reload", request.HasSpareClips, request.IsManualRevolver);
        var reloadPenalty = reloadResult.CanReloadAndFireSameTurn ? reloadResult.AttackDicePenalty : 0;

        var finalDifficulty = baseDifficulty + rangeModifier + coverModifier + movingModifier + autoFireDifficultyModifier;
        var finalDiceBonus = aimDiceBonus + scopeDiceBonus + autoFireDiceBonus - reloadPenalty;

        findings.Add($"Ranged combat resolved: base {baseDifficulty} + range {rangeModifier} + cover {coverModifier} + moving {movingModifier} + autoFire {autoFireDifficultyModifier} = {finalDifficulty} difficulty.");
        findings.Add($"Dice bonus: aim {aimDiceBonus} + scope {scopeDiceBonus} + autoFire {autoFireDiceBonus} - reload {reloadPenalty} = {finalDiceBonus}.");

        return new WerewolfCombatRangedResult(
            request.RequestId,
            baseDifficulty,
            rangeModifier,
            aimDiceBonus,
            scopeDiceBonus,
            coverModifier,
            movingModifier,
            autoFireDiceBonus,
            autoFireDifficultyModifier,
            reloadPenalty,
            finalDifficulty,
            finalDiceBonus,
            false,
            findings);
    }
}
