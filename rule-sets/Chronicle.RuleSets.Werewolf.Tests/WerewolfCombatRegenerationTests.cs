using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfCombatRegenerationTests
{
    [Fact]
    public void RegenerationServiceExistsAndIsCallable()
    {
        var state = BuildState(healthTrack: WerewolfHealthTrackComputer.Compute([
            new WerewolfDamageMark(WerewolfDamageCategory.Bashing, 1)]));
        var request = new WerewolfRegenerationRequest("req", state, 1, WerewolfDamageCategory.Bashing, 1, 1);

        var result = WerewolfRegenerationService.Regenerate(request);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void RegenerationServiceIncrementsVersion()
    {
        var state = BuildState(healthTrack: WerewolfHealthTrackComputer.Compute([
            new WerewolfDamageMark(WerewolfDamageCategory.Bashing, 1)]));
        var request = new WerewolfRegenerationRequest("req", state, 1, WerewolfDamageCategory.Bashing, 1, 1);

        var result = WerewolfRegenerationService.Regenerate(request);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.UpdatedState!.RuntimeStateVersion);
    }

    [Fact]
    public void RegenerationServiceVersionMismatchReturnsFailure()
    {
        var state = BuildState(healthTrack: WerewolfHealthTrackComputer.Compute([
            new WerewolfDamageMark(WerewolfDamageCategory.Bashing, 1)]));
        var request = new WerewolfRegenerationRequest("req", state, 2, WerewolfDamageCategory.Bashing, 1, 1);

        var result = WerewolfRegenerationService.Regenerate(request);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void RegenerationServiceDoesNotDuplicateCombatLogic()
    {
        var state = BuildState(healthTrack: WerewolfHealthTrackComputer.Compute([
            new WerewolfDamageMark(WerewolfDamageCategory.Bashing, 1)]));
        var request = new WerewolfRegenerationRequest("req", state, 1, WerewolfDamageCategory.Bashing, 1, 1);

        var result = WerewolfRegenerationService.Regenerate(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.UpdatedState);
        Assert.Equal(WerewolfRaceIdentifiers.Homid, result.UpdatedState.BirthRace);
    }

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
            track,
            CurrentForm: WerewolfFormIdentifiers.Homid);
    }
}
