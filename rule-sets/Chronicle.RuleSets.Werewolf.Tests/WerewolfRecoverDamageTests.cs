using Chronicle.RuleSets.Werewolf.CharacterCreation;
using System.Linq;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfRecoverDamageTests
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
    public void RecoverBashingDamageSucceedsAndDecrementsTotal()
    {
        var damagedTrack = WerewolfHealthTrackComputer.Compute([
            new(WerewolfDamageCategory.Bashing, 2),
            new(WerewolfDamageCategory.Bashing, 1)
        ]);
        var state = BuildState(damagedTrack);
        var request = new WerewolfRecoverDamageRequest("req-1", state, 1, WerewolfDamageCategory.Bashing, 1);

        var result = WerewolfRecoverDamageService.RecoverDamage(request);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.HealthTrack.TotalDamage);
        Assert.Equal(2, result.HealthTrack.BashingCount);
    }

    [Fact]
    public void RecoverLethalDamageSucceeds()
    {
        var damagedTrack = WerewolfHealthTrackComputer.Compute([
            new(WerewolfDamageCategory.Lethal, 2)
        ]);
        var state = BuildState(damagedTrack);
        var request = new WerewolfRecoverDamageRequest("req-1", state, 1, WerewolfDamageCategory.Lethal, 1);

        var result = WerewolfRecoverDamageService.RecoverDamage(request);

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.HealthTrack.TotalDamage);
        Assert.Equal(1, result.HealthTrack.LethalCount);
    }

    [Fact]
    public void RecoverAggravatedDamageWithoutAlternateFormRestReturnsFailure()
    {
        var damagedTrack = WerewolfHealthTrackComputer.Compute([
            new(WerewolfDamageCategory.Aggravated, 1)
        ]);
        var state = BuildState(damagedTrack);
        var request = new WerewolfRecoverDamageRequest("req-1", state, 1, WerewolfDamageCategory.Aggravated, 1, false);

        var result = WerewolfRecoverDamageService.RecoverDamage(request);

        Assert.False(result.Succeeded);
        Assert.Equal("AggravatedRestRequired", result.ErrorCode);
    }

    [Fact]
    public void RecoverAggravatedDamageWithAlternateFormRestSucceeds()
    {
        var damagedTrack = WerewolfHealthTrackComputer.Compute([
            new(WerewolfDamageCategory.Aggravated, 1)
        ]);
        var state = BuildState(damagedTrack);
        var request = new WerewolfRecoverDamageRequest("req-1", state, 1, WerewolfDamageCategory.Aggravated, 1, true);

        var result = WerewolfRecoverDamageService.RecoverDamage(request);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.HealthTrack.TotalDamage);
        Assert.Equal(0, result.HealthTrack.AggravatedCount);
    }

    [Fact]
    public void RecoverDamageNoDamageReturnsFailure()
    {
        var state = BuildState();
        var request = new WerewolfRecoverDamageRequest("req-1", state, 1, WerewolfDamageCategory.Lethal, 1, true);

        var result = WerewolfRecoverDamageService.RecoverDamage(request);

        Assert.False(result.Succeeded);
        Assert.Equal("NoDamage", result.ErrorCode);
    }

    [Fact]
    public void RecoverDamageStaleVersionReturnsFailure()
    {
        var damagedTrack = WerewolfHealthTrackComputer.Compute([new(WerewolfDamageCategory.Bashing, 1)]);
        var state = BuildState(damagedTrack);
        var request = new WerewolfRecoverDamageRequest("req-1", state, 2, WerewolfDamageCategory.Bashing, 1);

        var result = WerewolfRecoverDamageService.RecoverDamage(request);

        Assert.False(result.Succeeded);
        Assert.Equal("StaleVersion", result.ErrorCode);
    }

    [Fact]
    public void RecoverDamageOverRecoverClampsToZero()
    {
        var damagedTrack = WerewolfHealthTrackComputer.Compute([new(WerewolfDamageCategory.Bashing, 1)]);
        var state = BuildState(damagedTrack);
        var request = new WerewolfRecoverDamageRequest("req-1", state, 1, WerewolfDamageCategory.Bashing, 5);

        var result = WerewolfRecoverDamageService.RecoverDamage(request);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.HealthTrack.TotalDamage);
        Assert.Equal(0, result.HealthTrack.BashingCount);
    }
}
