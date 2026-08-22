using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfCombatRangedTests
{
    [Fact]
    public void RangeBandPointBlankReturnsDifficulty4()
    {
        var result = WerewolfCombatRangedService.ResolveRangedCombat(new WerewolfCombatRangedRequest(
            "req", State(), 1, WerewolfCombatIdentifiers.Firearm,
            WerewolfCombatRangeBand.PointBlank, 0, false, WerewolfCombatCoverType.None, false,
            WerewolfCombatFiringMode.SingleShot, 1, 1, 10, 10, false, false, false));

        Assert.Equal(4, result.FinalDifficulty);
    }

    [Fact]
    public void RangeBandMediumReturnsDifficulty6()
    {
        var result = WerewolfCombatRangedService.ResolveRangedCombat(new WerewolfCombatRangedRequest(
            "req", State(), 1, WerewolfCombatIdentifiers.Firearm,
            WerewolfCombatRangeBand.Medium, 0, false, WerewolfCombatCoverType.None, false,
            WerewolfCombatFiringMode.SingleShot, 1, 1, 10, 10, false, false, false));

        Assert.Equal(6, result.FinalDifficulty);
    }

    [Fact]
    public void RangeBandLongReturnsDifficulty8()
    {
        var result = WerewolfCombatRangedService.ResolveRangedCombat(new WerewolfCombatRangedRequest(
            "req", State(), 1, WerewolfCombatIdentifiers.Firearm,
            WerewolfCombatRangeBand.LongRange, 0, false, WerewolfCombatCoverType.None, false,
            WerewolfCombatFiringMode.SingleShot, 1, 1, 10, 10, false, false, false));

        Assert.Equal(8, result.FinalDifficulty);
    }

    [Fact]
    public void AimWithScopeAddsDice()
    {
        var result = WerewolfCombatRangedService.ResolveRangedCombat(new WerewolfCombatRangedRequest(
            "req", State(), 1, WerewolfCombatIdentifiers.Firearm,
            WerewolfCombatRangeBand.Medium, 3, true, WerewolfCombatCoverType.None, false,
            WerewolfCombatFiringMode.SingleShot, 1, 1, 10, 10, false, false, false));

        Assert.True(result.AimDiceBonus > 0);
        Assert.True(result.ScopeDiceBonus > 0);
    }

    [Fact]
    public void CoverProneAddsDifficulty()
    {
        var result = WerewolfCombatRangedService.ResolveRangedCombat(new WerewolfCombatRangedRequest(
            "req", State(), 1, WerewolfCombatIdentifiers.Firearm,
            WerewolfCombatRangeBand.Medium, 0, false, WerewolfCombatCoverType.Prone, false,
            WerewolfCombatFiringMode.SingleShot, 1, 1, 10, 10, false, false, false));

        Assert.Equal(1, result.CoverDifficultyModifier);
        Assert.Equal(7, result.FinalDifficulty);
    }

    [Fact]
    public void MovingTargetAddsDifficulty()
    {
        var result = WerewolfCombatRangedService.ResolveRangedCombat(new WerewolfCombatRangedRequest(
            "req", State(), 1, WerewolfCombatIdentifiers.Firearm,
            WerewolfCombatRangeBand.Medium, 0, false, WerewolfCombatCoverType.None, true,
            WerewolfCombatFiringMode.SingleShot, 1, 1, 10, 10, false, false, false));

        Assert.Equal(1, result.MovingTargetModifier);
        Assert.Equal(7, result.FinalDifficulty);
    }

    [Fact]
    public void AutomaticFireAddsDiceAndDifficulty()
    {
        var result = WerewolfCombatRangedService.ResolveRangedCombat(new WerewolfCombatRangedRequest(
            "req", State(), 1, WerewolfCombatIdentifiers.Firearm,
            WerewolfCombatRangeBand.Medium, 0, false, WerewolfCombatCoverType.None, false,
            WerewolfCombatFiringMode.AutomaticFire, 1, 1, 10, 10, false, false, false));

        Assert.Equal(10, result.AutomaticFireDiceBonus);
        Assert.Equal(2, result.AutomaticFireDifficultyModifier);
        Assert.Equal(8, result.FinalDifficulty);
    }

    [Fact]
    public void BowHeartShotIncreasesDifficulty()
    {
        var bowResult = WerewolfCombatBowService.ResolveBowAttack("req", true, true);
        Assert.Equal(8, bowResult.Difficulty);
        Assert.True(bowResult.IsHeartShotAttempt);
    }

    [Fact]
    public void ThrownAttackReturnsCorrectDifficulty()
    {
        var thrownResult = WerewolfCombatThrownService.ResolveThrownAttack("req", 4, true);
        Assert.Equal(6, thrownResult.Difficulty);
        Assert.Equal(4, thrownResult.DamagePoolBonus);
    }

    [Fact]
    public void ReloadWithSpareClipsAllowsSameTurn()
    {
        var reloadResult = WerewolfCombatReloadService.ResolveReload("req", true, false);
        Assert.True(reloadResult.CanReloadAndFireSameTurn);
        Assert.Equal(2, reloadResult.AttackDicePenalty);
    }

    [Fact]
    public void ManualRevolverRequiresFullTurn()
    {
        var reloadResult = WerewolfCombatReloadService.ResolveReload("req", false, true);
        Assert.False(reloadResult.CanReloadAndFireSameTurn);
    }

    [Fact]
    public void MultipleShotsLimitedByRateOfFire()
    {
        var multipleResult = WerewolfCombatMultipleShotService.ResolveMultipleShots("req", 3, 2, 0, 5);
        Assert.Equal(2, multipleResult.ShotsFired);
        Assert.True(multipleResult.RequiresMultipleActions);
    }

    [Fact]
    public void AreaFireDistributesSuccesses()
    {
        var areaResult = WerewolfCombatAreaFireService.DistributeAreaFireSuccesses("req", 6, 3, 6);
        Assert.True(areaResult.CanUseAreaFire);
        Assert.Equal(3, areaResult.DistributedSuccesses.Count);
        Assert.Equal(2, areaResult.DistributedSuccesses[0]);
    }

    private static WerewolfRuntimeCharacterState State()
    {
        return new WerewolfRuntimeCharacterState(
            "test", "1.0", "draft", 1, new Dictionary<string, string>(),
            5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0,
            WerewolfRaceIdentifiers.Homid, null, WerewolfFormIdentifiers.Homid);
    }
}
