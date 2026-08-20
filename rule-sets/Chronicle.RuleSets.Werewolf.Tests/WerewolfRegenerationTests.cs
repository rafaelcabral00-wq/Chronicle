using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfRegenerationTests
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
            BirthRace: WerewolfRaceIdentifiers.Homid,
            track);
    }

    [Fact]
    public void RegenerateBashingSucceedsAutomatically()
    {
        var damagedTrack = WerewolfHealthTrackComputer.Compute([
            new(WerewolfDamageCategory.Bashing, 3)
        ]);
        var state = BuildState(damagedTrack);
        var request = new WerewolfRegenerationRequest("req-1", state, 1, WerewolfDamageCategory.Bashing, 1, 1);

        var result = WerewolfRegenerationService.Regenerate(request);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.HealthTrack.TotalDamage);
        Assert.Equal(2, result.HealthTrack.BashingCount);
    }

    [Fact]
    public void RegenerateLethalOutsideStressSucceedsAutomatically()
    {
        var damagedTrack = WerewolfHealthTrackComputer.Compute([
            new(WerewolfDamageCategory.Lethal, 3)
        ]);
        var state = BuildState(damagedTrack);
        var request = new WerewolfRegenerationRequest("req-1", state, 1, WerewolfDamageCategory.Lethal, 1, 1, false, false);

        var result = WerewolfRegenerationService.Regenerate(request);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.HealthTrack.TotalDamage);
        Assert.Equal(2, result.HealthTrack.LethalCount);
    }

    [Fact]
    public void RegenerateLethalUnderStressRequiresVigorTest()
    {
        var damagedTrack = WerewolfHealthTrackComputer.Compute([
            new(WerewolfDamageCategory.Lethal, 3)
        ]);
        var state = BuildState(damagedTrack);
        var request = new WerewolfRegenerationRequest("req-1", state, 1, WerewolfDamageCategory.Lethal, 1, 1, true, false, 5, 2, 0);

        var result = WerewolfRegenerationService.Regenerate(request);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.HealthTrack.TotalDamage);
        Assert.Equal(2, result.Successes);
    }

    [Fact]
    public void RegenerateLethalUnderStressFailsWithZeroSuccesses()
    {
        var damagedTrack = WerewolfHealthTrackComputer.Compute([
            new(WerewolfDamageCategory.Lethal, 3)
        ]);
        var state = BuildState(damagedTrack);
        var request = new WerewolfRegenerationRequest("req-1", state, 1, WerewolfDamageCategory.Lethal, 1, 1, true, false, 5, 0, 3);

        var result = WerewolfRegenerationService.Regenerate(request);

        Assert.False(result.Succeeded);
        Assert.Equal("TestFailed", result.ErrorCode);
        Assert.Equal(3, result.HealthTrack.TotalDamage);
    }

    [Fact]
    public void RegenerateAggravatedWithoutRestReturnsFailure()
    {
        var damagedTrack = WerewolfHealthTrackComputer.Compute([
            new(WerewolfDamageCategory.Aggravated, 1)
        ]);
        var state = BuildState(damagedTrack);
        var request = new WerewolfRegenerationRequest("req-1", state, 1, WerewolfDamageCategory.Aggravated, 1, 1, false, false);

        var result = WerewolfRegenerationService.Regenerate(request);

        Assert.False(result.Succeeded);
        Assert.Equal("AggravatedRestRequired", result.ErrorCode);
    }

    [Fact]
    public void RegenerateAggravatedWithRestSucceeds()
    {
        var damagedTrack = WerewolfHealthTrackComputer.Compute([
            new(WerewolfDamageCategory.Aggravated, 2)
        ]);
        var state = BuildState(damagedTrack);
        var request = new WerewolfRegenerationRequest("req-1", state, 1, WerewolfDamageCategory.Aggravated, 1, 1, false, true);

        var result = WerewolfRegenerationService.Regenerate(request);

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.HealthTrack.TotalDamage);
        Assert.Equal(1, result.HealthTrack.AggravatedCount);
    }

    [Fact]
    public void RegenerateNoDamageOfTypeReturnsFailure()
    {
        var damagedTrack = WerewolfHealthTrackComputer.Compute([
            new(WerewolfDamageCategory.Bashing, 2)
        ]);
        var state = BuildState(damagedTrack);
        var request = new WerewolfRegenerationRequest("req-1", state, 1, WerewolfDamageCategory.Lethal, 1, 1);

        var result = WerewolfRegenerationService.Regenerate(request);

        Assert.False(result.Succeeded);
        Assert.Equal("NoDamageOfType", result.ErrorCode);
    }
}
