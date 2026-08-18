using Chronicle.RuleSets.Werewolf.CharacterCreation;
using System.Linq;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfApplyDamageTests
{
    private static WerewolfRuntimeCharacterState BuildState(WerewolfHealthTrack? healthTrack = null)
    {
        var track = healthTrack ?? WerewolfHealthTrackComputer.Compute([]);
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
            5, 5, 3, 3, 4, 4, 0, 0, 0, 0, 0, 0,
            track);
    }

    [Fact]
    public void ApplyBashingDamageSucceedsAndIncrementsVersion()
    {
        var state = BuildState();
        var request = new WerewolfApplyDamageRequest("req-1", state, 1, WerewolfDamageCategory.Bashing, 2);

        var result = WerewolfApplyDamageService.ApplyDamage(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.UpdatedState);
        Assert.Equal(2, result.UpdatedState!.RuntimeStateVersion);
        Assert.Equal(2, result.HealthTrack.TotalDamage);
        Assert.Equal(-1, result.HealthTrack.WoundPenalty);
        Assert.Equal(WerewolfHealthLevelName.Ferido, result.HealthTrack.CurrentLevel);
    }

    [Fact]
    public void ApplyLethalDamageSucceeds()
    {
        var state = BuildState();
        var request = new WerewolfApplyDamageRequest("req-1", state, 1, WerewolfDamageCategory.Lethal, 1);

        var result = WerewolfApplyDamageService.ApplyDamage(request);

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.HealthTrack.TotalDamage);
        Assert.Equal(1, result.HealthTrack.LethalCount);
    }

    [Fact]
    public void ApplyAggravatedDamageSucceeds()
    {
        var state = BuildState();
        var request = new WerewolfApplyDamageRequest("req-1", state, 1, WerewolfDamageCategory.Aggravated, 1);

        var result = WerewolfApplyDamageService.ApplyDamage(request);

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.HealthTrack.TotalDamage);
        Assert.Equal(1, result.HealthTrack.AggravatedCount);
    }

    [Fact]
    public void ApplyBashingBeyondIncapacitadoSetsUnconscious()
    {
        var nearDeathTrack = WerewolfHealthTrackComputer.Compute(
            Enumerable.Repeat(new WerewolfDamageMark(WerewolfDamageCategory.Bashing, 1), 6).ToList());
        var state = BuildState(nearDeathTrack);
        var request = new WerewolfApplyDamageRequest("req-1", state, 1, WerewolfDamageCategory.Bashing, 2);

        var result = WerewolfApplyDamageService.ApplyDamage(request);

        Assert.True(result.Succeeded);
        Assert.Equal(WerewolfHealthState.Unconscious, result.HealthTrack.HealthState);
        Assert.Equal(WerewolfDamageCategory.Bashing, result.HealthTrack.FatalDamageType);
    }

    [Fact]
    public void ApplyLethalBeyondIncapacitadoSetsNearDeath()
    {
        var nearDeathTrack = WerewolfHealthTrackComputer.Compute(
            Enumerable.Repeat(new WerewolfDamageMark(WerewolfDamageCategory.Bashing, 1), 6).ToList());
        var state = BuildState(nearDeathTrack);
        var request = new WerewolfApplyDamageRequest("req-1", state, 1, WerewolfDamageCategory.Lethal, 1);

        var result = WerewolfApplyDamageService.ApplyDamage(request);

        Assert.True(result.Succeeded);
        Assert.Equal(WerewolfHealthState.NearDeath, result.HealthTrack.HealthState);
        Assert.Equal(WerewolfDamageCategory.Lethal, result.HealthTrack.FatalDamageType);
    }

    [Fact]
    public void ApplyAggravatedBeyondIncapacitadoSetsDead()
    {
        var nearDeathTrack = WerewolfHealthTrackComputer.Compute(
            Enumerable.Repeat(new WerewolfDamageMark(WerewolfDamageCategory.Bashing, 1), 6).ToList());
        var state = BuildState(nearDeathTrack);
        var request = new WerewolfApplyDamageRequest("req-1", state, 1, WerewolfDamageCategory.Aggravated, 1);

        var result = WerewolfApplyDamageService.ApplyDamage(request);

        Assert.True(result.Succeeded);
        Assert.Equal(WerewolfHealthState.Dead, result.HealthTrack.HealthState);
        Assert.Equal(WerewolfDamageCategory.Aggravated, result.HealthTrack.FatalDamageType);
    }

    [Fact]
    public void ApplySevenBashingSetsUnconscious()
    {
        var state = BuildState();
        var request = new WerewolfApplyDamageRequest("req-1", state, 1, WerewolfDamageCategory.Bashing, 7);

        var result = WerewolfApplyDamageService.ApplyDamage(request);

        Assert.True(result.Succeeded);
        Assert.Equal(WerewolfHealthState.Unconscious, result.HealthTrack.HealthState);
        Assert.Equal(WerewolfDamageCategory.Bashing, result.HealthTrack.FatalDamageType);
    }

    [Fact]
    public void ApplySevenLethalSetsNearDeath()
    {
        var state = BuildState();
        var request = new WerewolfApplyDamageRequest("req-1", state, 1, WerewolfDamageCategory.Lethal, 7);

        var result = WerewolfApplyDamageService.ApplyDamage(request);

        Assert.True(result.Succeeded);
        Assert.Equal(WerewolfHealthState.NearDeath, result.HealthTrack.HealthState);
        Assert.Equal(WerewolfDamageCategory.Lethal, result.HealthTrack.FatalDamageType);
    }

    [Fact]
    public void ApplySevenAggravatedSetsDead()
    {
        var state = BuildState();
        var request = new WerewolfApplyDamageRequest("req-1", state, 1, WerewolfDamageCategory.Aggravated, 7);

        var result = WerewolfApplyDamageService.ApplyDamage(request);

        Assert.True(result.Succeeded);
        Assert.Equal(WerewolfHealthState.Dead, result.HealthTrack.HealthState);
        Assert.Equal(WerewolfDamageCategory.Aggravated, result.HealthTrack.FatalDamageType);
    }

    [Fact]
    public void ApplyEightBashingRemainsUnconscious()
    {
        var state = BuildState();
        var request = new WerewolfApplyDamageRequest("req-1", state, 1, WerewolfDamageCategory.Bashing, 8);

        var result = WerewolfApplyDamageService.ApplyDamage(request);

        Assert.True(result.Succeeded);
        Assert.Equal(WerewolfHealthState.Unconscious, result.HealthTrack.HealthState);
        Assert.Equal(WerewolfDamageCategory.Bashing, result.HealthTrack.FatalDamageType);
    }

    [Fact]
    public void ApplyEightLethalSetsDead()
    {
        var state = BuildState();
        var request = new WerewolfApplyDamageRequest("req-1", state, 1, WerewolfDamageCategory.Lethal, 8);

        var result = WerewolfApplyDamageService.ApplyDamage(request);

        Assert.True(result.Succeeded);
        Assert.Equal(WerewolfHealthState.Dead, result.HealthTrack.HealthState);
        Assert.Equal(WerewolfDamageCategory.Lethal, result.HealthTrack.FatalDamageType);
    }

    [Fact]
    public void ApplyEightAggravatedSetsDead()
    {
        var state = BuildState();
        var request = new WerewolfApplyDamageRequest("req-1", state, 1, WerewolfDamageCategory.Aggravated, 8);

        var result = WerewolfApplyDamageService.ApplyDamage(request);

        Assert.True(result.Succeeded);
        Assert.Equal(WerewolfHealthState.Dead, result.HealthTrack.HealthState);
        Assert.Equal(WerewolfDamageCategory.Aggravated, result.HealthTrack.FatalDamageType);
    }

    [Fact]
    public void ApplyDamageStaleVersionReturnsFailure()
    {
        var state = BuildState();
        var request = new WerewolfApplyDamageRequest("req-1", state, 2, WerewolfDamageCategory.Bashing, 1);

        var result = WerewolfApplyDamageService.ApplyDamage(request);

        Assert.False(result.Succeeded);
        Assert.Equal("StaleVersion", result.ErrorCode);
    }

    [Fact]
    public void ApplyDamageNullStateReturnsFailure()
    {
        var request = new WerewolfApplyDamageRequest("req-1", null!, 1, WerewolfDamageCategory.Bashing, 1);

        var result = WerewolfApplyDamageService.ApplyDamage(request);

        Assert.False(result.Succeeded);
        Assert.Equal("InvalidState", result.ErrorCode);
    }

    [Fact]
    public void ApplyDamageZeroAmountReturnsFailure()
    {
        var state = BuildState();
        var request = new WerewolfApplyDamageRequest("req-1", state, 1, WerewolfDamageCategory.Bashing, 0);

        var result = WerewolfApplyDamageService.ApplyDamage(request);

        Assert.False(result.Succeeded);
        Assert.Equal("InvalidAmount", result.ErrorCode);
    }

    [Fact]
    public void ApplyDamageInvalidDamageTypeReturnsFailure()
    {
        var state = BuildState();
        var request = new WerewolfApplyDamageRequest("req-1", state, 1, (WerewolfDamageCategory)999, 1);

        var result = WerewolfApplyDamageService.ApplyDamage(request);

        Assert.False(result.Succeeded);
        Assert.Equal("InvalidDamageType", result.ErrorCode);
    }
}
