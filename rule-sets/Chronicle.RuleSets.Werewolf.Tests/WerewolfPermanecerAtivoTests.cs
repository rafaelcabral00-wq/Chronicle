using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfPermanecerAtivoTests
{
    private static WerewolfRuntimeCharacterState BuildNearDeathState(WerewolfHealthTrack? healthTrack = null, int ragePermanent = 5)
    {
        var track = healthTrack ?? WerewolfHealthTrackComputer.Compute(
            Enumerable.Repeat(new WerewolfDamageMark(WerewolfDamageCategory.Lethal, 1), 7).ToList());
        return new WerewolfRuntimeCharacterState(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            "draft-1",
            1,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["packageId"] = WerewolfRuleSetPackage.ProvisionalPackageId,
                ["packageVersion"] = WerewolfRuleSetPackage.PackageVersion,
                ["declaredReleaseScope"] = WerewolfRuleSetPackage.DeclaredReleaseScope,
                ["contractVersion"] = "1"
            },
            ragePermanent, 5, 3, 3, 4, 4, 0, 0, 0, 0, 0, 0,
            BirthRace: WerewolfRaceIdentifiers.Homid,
            track);
    }

    [Fact]
    public void PermanecerAtivoSucceedsAndSurvives()
    {
        var state = BuildNearDeathState();
        var request = new WerewolfPermanecerAtivoRequest("req-1", state, 1, 3, 0);

        var result = WerewolfPermanecerAtivoService.PermanecerAtivo(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.UpdatedState);
        Assert.Equal(2, result.UpdatedState!.RuntimeStateVersion);
        Assert.Equal(WerewolfHealthState.Wounded, result.HealthTrack.HealthState);
        Assert.True(result.HealthTrack.PermanecerAtivoAttempted);
        Assert.Equal(3, result.Successes);
        Assert.Equal(4, result.HealthTrack.LethalCount);
    }

    [Fact]
    public void PermanecerAtivoFailsRemainsNearDeath()
    {
        var state = BuildNearDeathState();
        var request = new WerewolfPermanecerAtivoRequest("req-1", state, 1, 0, 5);

        var result = WerewolfPermanecerAtivoService.PermanecerAtivo(request);

        Assert.False(result.Succeeded);
        Assert.NotNull(result.UpdatedState);
        Assert.Equal(2, result.UpdatedState!.RuntimeStateVersion);
        Assert.Equal(WerewolfHealthState.NearDeath, result.HealthTrack.HealthState);
        Assert.True(result.HealthTrack.PermanecerAtivoAttempted);
        Assert.Equal(0, result.Successes);
    }

    [Fact]
    public void PermanecerAtivoRejectsAlreadyAttempted()
    {
        var track = WerewolfHealthTrackComputer.Compute(
            Enumerable.Repeat(new WerewolfDamageMark(WerewolfDamageCategory.Lethal, 1), 7).ToList(),
            permanecerAtivoAttempted: true);
        var state = BuildNearDeathState(track);
        var request = new WerewolfPermanecerAtivoRequest("req-1", state, 1, 3, 0);

        var result = WerewolfPermanecerAtivoService.PermanecerAtivo(request);

        Assert.False(result.Succeeded);
        Assert.Equal("AlreadyAttempted", result.ErrorCode);
    }

    [Fact]
    public void PermanecerAtivoRejectsWhenNotEligible()
    {
        var healthyTrack = WerewolfHealthTrackComputer.Compute([]);
        var state = BuildNearDeathState(healthyTrack);
        var request = new WerewolfPermanecerAtivoRequest("req-1", state, 1, 3, 0);

        var result = WerewolfPermanecerAtivoService.PermanecerAtivo(request);

        Assert.False(result.Succeeded);
        Assert.Equal("NotEligible", result.ErrorCode);
    }

    [Fact]
    public void PermanecerAtivoRecoversLevelsFromDeadState()
    {
        var deadTrack = WerewolfHealthTrackComputer.Compute(
            Enumerable.Repeat(new WerewolfDamageMark(WerewolfDamageCategory.Aggravated, 1), 7).ToList());
        var state = BuildNearDeathState(deadTrack);
        var request = new WerewolfPermanecerAtivoRequest("req-1", state, 1, 3, 0);

        var result = WerewolfPermanecerAtivoService.PermanecerAtivo(request);

        Assert.True(result.Succeeded);
        Assert.Equal(WerewolfHealthState.Wounded, result.HealthTrack.HealthState);
        Assert.Equal(4, result.HealthTrack.AggravatedCount);
    }

    [Fact]
    public void PermanecerAtivoRejectsWhenUnconsciousFromBashing()
    {
        var unconsciousTrack = WerewolfHealthTrackComputer.Compute(
            Enumerable.Repeat(new WerewolfDamageMark(WerewolfDamageCategory.Bashing, 1), 8).ToList());
        var state = BuildNearDeathState(unconsciousTrack);
        var request = new WerewolfPermanecerAtivoRequest("req-1", state, 1, 3, 0);

        var result = WerewolfPermanecerAtivoService.PermanecerAtivo(request);

        Assert.False(result.Succeeded);
        Assert.Equal("NotEligible", result.ErrorCode);
    }

    [Fact]
    public void PermanecerAtivoSucceedsFromNearDeathLethal()
    {
        var nearDeathTrack = WerewolfHealthTrackComputer.Compute(
            Enumerable.Repeat(new WerewolfDamageMark(WerewolfDamageCategory.Lethal, 1), 7).ToList());
        var state = BuildNearDeathState(nearDeathTrack);
        var request = new WerewolfPermanecerAtivoRequest("req-1", state, 1, 3, 0);

        var result = WerewolfPermanecerAtivoService.PermanecerAtivo(request);

        Assert.True(result.Succeeded);
        Assert.Equal(WerewolfHealthState.Wounded, result.HealthTrack.HealthState);
        Assert.Equal(4, result.HealthTrack.LethalCount);
    }

    [Fact]
    public void PermanecerAtivoHealsOnlyPrimaryCategoryFromMixedDamage()
    {
        var mixedTrack = WerewolfHealthTrackComputer.Compute([
            new(WerewolfDamageCategory.Bashing, 3),
            new(WerewolfDamageCategory.Lethal, 4)
        ]);
        var state = BuildNearDeathState(mixedTrack);
        var request = new WerewolfPermanecerAtivoRequest("req-1", state, 1, 2, 0);

        var result = WerewolfPermanecerAtivoService.PermanecerAtivo(request);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.HealthTrack.LethalCount);
        Assert.Equal(3, result.HealthTrack.BashingCount);
        Assert.Equal(WerewolfHealthState.Wounded, result.HealthTrack.HealthState);
    }

    [Fact]
    public void PermanecerAtivoSucceedsFromDeadLethal()
    {
        var deadTrack = WerewolfHealthTrackComputer.Compute(
            Enumerable.Repeat(new WerewolfDamageMark(WerewolfDamageCategory.Lethal, 1), 8).ToList());
        var state = BuildNearDeathState(deadTrack);
        var request = new WerewolfPermanecerAtivoRequest("req-1", state, 1, 3, 0);

        var result = WerewolfPermanecerAtivoService.PermanecerAtivo(request);

        Assert.True(result.Succeeded);
        Assert.Equal(WerewolfHealthState.Wounded, result.HealthTrack.HealthState);
        Assert.Equal(5, result.HealthTrack.LethalCount);
    }

    [Fact]
    public void PermanecerAtivoRejectsWhenRagePermanentIsZero()
    {
        var state = BuildNearDeathState(ragePermanent: 0);
        var request = new WerewolfPermanecerAtivoRequest("req-1", state, 1, 3, 0);

        var result = WerewolfPermanecerAtivoService.PermanecerAtivo(request);

        Assert.False(result.Succeeded);
        Assert.Equal("InsufficientFury", result.ErrorCode);
    }

    [Fact]
    public void PermanecerAtivoPoolDerivedFromRagePermanentNotCallerSupplied()
    {
        var state = BuildNearDeathState(ragePermanent: 2);
        var request = new WerewolfPermanecerAtivoRequest("req-1", state, 1, 2, 0);

        var result = WerewolfPermanecerAtivoService.PermanecerAtivo(request);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Successes);
        Assert.Equal(WerewolfHealthState.Wounded, result.HealthTrack.HealthState);
    }
}
