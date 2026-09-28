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

        // Aim is capped by the shooter's Perception, which must come from the
        // character's own sheet rather than a fixed constant. Aim is the one
        // place a scope counts, so DefineAim already folds the scope bonus into
        // EffectiveAimDice; the reported ScopeDiceBonus is informational and is
        // deliberately not added a second time below.
        var perception = ReadPerception(request.CurrentState, request.ExpectedRuntimeStateVersion);
        var aimResult = WerewolfCombatAimService.DefineAim(request.RequestId, request.AimTurns, perception, request.HasScope);
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

        // aimDiceBonus already includes the scope bonus, so scopeDiceBonus is
        // not added again here.
        var finalDiceBonus = aimDiceBonus + autoFireDiceBonus - reloadPenalty;

        findings.Add($"Ranged combat resolved: base {baseDifficulty} + range {rangeModifier} + cover {coverModifier} + moving {movingModifier} + autoFire {autoFireDifficultyModifier} = {finalDifficulty} difficulty.");
        findings.Add($"Aim capped at Perception {perception}: {aimResult.AimTurns} turn(s) + scope {scopeDiceBonus} = {aimDiceBonus} dice (scope counted once).");
        findings.Add($"Dice bonus: aim {aimDiceBonus} + autoFire {autoFireDiceBonus} - reload {reloadPenalty} = {finalDiceBonus}.");

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

    /// <summary>
    /// Reads the shooter's Perception from the character sheet carried on the
    /// runtime state. Falls back to 1 only when the sheet genuinely does not
    /// carry the rating, which is the minimum value the aim cap accepts.
    /// </summary>
    private static int ReadPerception(WerewolfRuntimeCharacterState state, int expectedRuntimeStateVersion)
    {
        if (!state.PackageBinding.TryGetValue("attributes", out var text) || string.IsNullOrWhiteSpace(text))
        {
            return 1;
        }

        var attributes = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(text);
        if (attributes is null)
        {
            return 1;
        }

        return Math.Max(1, attributes.GetValueOrDefault(WerewolfAttributeIdentifiers.Perception, 1));
    }
}
