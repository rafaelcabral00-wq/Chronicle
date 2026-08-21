using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfCombatPermanecerAtivoTests
{
    [Fact]
    public void PermanecerAtivoServiceExistsAndIsCallable()
    {
        var nearDeathTrack = WerewolfHealthTrackComputer.Compute(
            Enumerable.Repeat(new WerewolfDamageMark(WerewolfDamageCategory.Lethal, 1), 7).ToList());
        var state = BuildState(healthTrack: nearDeathTrack);
        var request = new WerewolfPermanecerAtivoRequest("req", state, 1, 3, 0);

        var result = WerewolfPermanecerAtivoService.PermanecerAtivo(request);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void PermanecerAtivoServiceIncrementsVersion()
    {
        var nearDeathTrack = WerewolfHealthTrackComputer.Compute(
            Enumerable.Repeat(new WerewolfDamageMark(WerewolfDamageCategory.Lethal, 1), 7).ToList());
        var state = BuildState(healthTrack: nearDeathTrack);
        var request = new WerewolfPermanecerAtivoRequest("req", state, 1, 3, 0);

        var result = WerewolfPermanecerAtivoService.PermanecerAtivo(request);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.UpdatedState!.RuntimeStateVersion);
    }

    [Fact]
    public void PermanecerAtivoServiceVersionMismatchReturnsFailure()
    {
        var nearDeathTrack = WerewolfHealthTrackComputer.Compute(
            Enumerable.Repeat(new WerewolfDamageMark(WerewolfDamageCategory.Lethal, 1), 7).ToList());
        var state = BuildState(healthTrack: nearDeathTrack);
        var request = new WerewolfPermanecerAtivoRequest("req", state, 2, 3, 0);

        var result = WerewolfPermanecerAtivoService.PermanecerAtivo(request);

        Assert.False(result.Succeeded);
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
