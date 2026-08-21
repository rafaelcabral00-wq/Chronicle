using Chronicle.RuleSets.Werewolf.CharacterCreation;
using System.Linq;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfCombatHealthIntegrationTests
{
    [Fact]
    public void BashingResultNetDamageZeroHealthRemainsHealthy()
    {
        var state = BuildState();
        var damageDefinition = WerewolfCombatDamageService.DefineDamageRoll(new WerewolfCombatDamageRequest(
            "req-1", state, 1, 1, "Strength", WerewolfDamageCategory.Bashing.ToString(), null));
        var damageInterpretation = WerewolfCombatDamageService.InterpretDamageRoll(damageDefinition, [6]);

        var soakDefinition = WerewolfCombatSoakService.DefineSoakRoll(new WerewolfCombatSoakRequest(
            "req-2", state, 2, WerewolfDamageCategory.Bashing, damageInterpretation.TotalDamage));
        var soakInterpretation = WerewolfCombatSoakService.InterpretSoakRoll(soakDefinition, [6, 7, 8]);

        var netBashing = Math.Max(0, damageInterpretation.TotalDamage - soakInterpretation.SoakSuccesses);
        var applyResult = netBashing > 0
            ? WerewolfApplyDamageService.ApplyDamage(new WerewolfApplyDamageRequest(
                "req-3", state, 3, WerewolfDamageCategory.Bashing, netBashing))
            : new WerewolfApplyDamageResult(true, state, [], state.HealthTrack!, [], "none", "req-3");

        Assert.True(applyResult.Succeeded);
        Assert.Equal(0, applyResult.HealthTrack.TotalDamage);
        Assert.Equal(WerewolfHealthState.Healthy, applyResult.HealthTrack.HealthState);
    }

    [Fact]
    public void LethalResultNetDamageOneProducesWounded()
    {
        var state = BuildState();
        var damageDefinition = WerewolfCombatDamageService.DefineDamageRoll(new WerewolfCombatDamageRequest(
            "req-1", state, 1, 1, "Strength + 1", WerewolfDamageCategory.Lethal.ToString(), null));
        var damageInterpretation = WerewolfCombatDamageService.InterpretDamageRoll(damageDefinition, [6]);

        var soakDefinition = WerewolfCombatSoakService.DefineSoakRoll(new WerewolfCombatSoakRequest(
            "req-2", state, 1, WerewolfDamageCategory.Lethal, damageInterpretation.TotalDamage));
        var soakInterpretation = WerewolfCombatSoakService.InterpretSoakRoll(soakDefinition, []);

        var netLethal = Math.Max(0, damageInterpretation.TotalDamage - soakInterpretation.SoakSuccesses);
        var applyResult = netLethal > 0
            ? WerewolfApplyDamageService.ApplyDamage(new WerewolfApplyDamageRequest(
                "req-3", state, 1, WerewolfDamageCategory.Lethal, netLethal))
            : new WerewolfApplyDamageResult(true, state, [], state.HealthTrack!, [], "none", "req-3");

        Assert.True(applyResult.Succeeded);
        Assert.Equal(netLethal, applyResult.HealthTrack.TotalDamage);
        Assert.Equal(netLethal, applyResult.HealthTrack.LethalCount);
        Assert.Equal(WerewolfHealthState.Wounded, applyResult.HealthTrack.HealthState);
    }

    [Fact]
    public void AggravatedResultNetDamageTwoProducesWounded()
    {
        var state = BuildState(form: WerewolfFormIdentifiers.Lupus, birthRace: WerewolfRaceIdentifiers.Lupus);
        var damageDefinition = WerewolfCombatDamageService.DefineDamageRoll(new WerewolfCombatDamageRequest(
            "req-1", state, 1, 3, "Strength + 1", WerewolfDamageCategory.Aggravated.ToString(), 1));
        var damageInterpretation = WerewolfCombatDamageService.InterpretDamageRoll(damageDefinition, [8, 9, 10, 10]);

        var soakDefinition = WerewolfCombatSoakService.DefineSoakRoll(new WerewolfCombatSoakRequest(
            "req-2", state, 2, WerewolfDamageCategory.Aggravated, damageInterpretation.TotalDamage));
        var soakInterpretation = WerewolfCombatSoakService.InterpretSoakRoll(soakDefinition, [6]);

        var netAggravated = Math.Max(0, damageInterpretation.TotalDamage - soakInterpretation.SoakSuccesses);
        var applyResult = netAggravated > 0
            ? WerewolfApplyDamageService.ApplyDamage(new WerewolfApplyDamageRequest(
                "req-3", state, 1, WerewolfDamageCategory.Aggravated, netAggravated))
            : new WerewolfApplyDamageResult(true, state, [], state.HealthTrack!, [], "none", "req-3");

        Assert.True(applyResult.Succeeded);
        Assert.Equal(netAggravated, applyResult.HealthTrack.TotalDamage);
        Assert.Equal(netAggravated, applyResult.HealthTrack.AggravatedCount);
        Assert.Equal(WerewolfHealthState.Wounded, applyResult.HealthTrack.HealthState);
    }

    [Fact]
    public void IncapacitatedBoundaryNetDamageSixProducesIncapacitated()
    {
        var incapacitatedTrack = WerewolfHealthTrackComputer.Compute(
            Enumerable.Repeat(new WerewolfDamageMark(WerewolfDamageCategory.Bashing, 1), 5).ToList());
        var state = BuildState(healthTrack: incapacitatedTrack);
        var damageDefinition = WerewolfCombatDamageService.DefineDamageRoll(new WerewolfCombatDamageRequest(
            "req-1", state, 1, 1, "Strength + 1", WerewolfDamageCategory.Bashing.ToString(), null));
        var damageInterpretation = WerewolfCombatDamageService.InterpretDamageRoll(damageDefinition, [6]);

        var soakDefinition = WerewolfCombatSoakService.DefineSoakRoll(new WerewolfCombatSoakRequest(
            "req-2", state, 1, WerewolfDamageCategory.Bashing, damageInterpretation.TotalDamage));
        var soakInterpretation = WerewolfCombatSoakService.InterpretSoakRoll(soakDefinition, []);

        var netBashing = Math.Max(0, damageInterpretation.TotalDamage - soakInterpretation.SoakSuccesses);
        var applyResult = netBashing > 0
            ? WerewolfApplyDamageService.ApplyDamage(new WerewolfApplyDamageRequest(
                "req-3", state, 1, WerewolfDamageCategory.Bashing, netBashing))
            : new WerewolfApplyDamageResult(true, state, [], state.HealthTrack!, [], "none", "req-3");

        Assert.True(applyResult.Succeeded);
        Assert.Equal(6, applyResult.HealthTrack.TotalDamage);
        Assert.Equal(WerewolfHealthState.Incapacitated, applyResult.HealthTrack.HealthState);
    }

    [Fact]
    public void HealthIntegrationPreservesBirthRaceAndCurrentForm()
    {
        var state = BuildState(form: WerewolfFormIdentifiers.Crinos, birthRace: WerewolfRaceIdentifiers.Metis);
        var damageDefinition = WerewolfCombatDamageService.DefineDamageRoll(new WerewolfCombatDamageRequest(
            "req-1", state, 1, 2, "Strength + 1", WerewolfDamageCategory.Lethal.ToString(), null));
        var damageInterpretation = WerewolfCombatDamageService.InterpretDamageRoll(damageDefinition, [6, 7]);

        var soakDefinition = WerewolfCombatSoakService.DefineSoakRoll(new WerewolfCombatSoakRequest(
            "req-2", state, 2, WerewolfDamageCategory.Lethal, Math.Max(1, damageInterpretation.TotalDamage)));
        var soakInterpretation = WerewolfCombatSoakService.InterpretSoakRoll(soakDefinition, [6]);

        var netLethal = Math.Max(0, damageInterpretation.TotalDamage - soakInterpretation.SoakSuccesses);
        var applyResult = netLethal > 0
            ? WerewolfApplyDamageService.ApplyDamage(new WerewolfApplyDamageRequest(
                "req-3", state, 1, WerewolfDamageCategory.Lethal, netLethal))
            : new WerewolfApplyDamageResult(true, state, [], state.HealthTrack!, [], "none", "req-3");

        Assert.True(applyResult.Succeeded);
        Assert.Equal(WerewolfRaceIdentifiers.Metis, applyResult.UpdatedState!.BirthRace);
        Assert.Equal(WerewolfFormIdentifiers.Crinos, applyResult.UpdatedState.CurrentForm);
    }

    [Fact]
    public void HealthIntegrationVersionIncrementsThroughPipeline()
    {
        var state = BuildState();
        var initiativeResult = WerewolfCombatInitiativeService.CalculateInitiative(new WerewolfCombatInitiativeRequest(
            "req-init", state, 1, 3, 2, 7, null));
        Assert.Equal(2, initiativeResult.UpdatedState!.RuntimeStateVersion);

        var defenseResult = WerewolfCombatDefenseService.CalculateDefense(new WerewolfCombatDefenseRequest(
            "req-def", initiativeResult.UpdatedState, 2, "dodge", 2, null, null, 3, null));
        Assert.Equal(3, defenseResult.UpdatedState!.RuntimeStateVersion);

        var damageDefinition = WerewolfCombatDamageService.DefineDamageRoll(new WerewolfCombatDamageRequest(
            "req-dmg", defenseResult.UpdatedState, 3, 2, "Strength + 1", WerewolfDamageCategory.Bashing.ToString(), 1));
        var damageInterpretation = WerewolfCombatDamageService.InterpretDamageRoll(damageDefinition, [6, 7, 8]);

        var soakDefinition = WerewolfCombatSoakService.DefineSoakRoll(new WerewolfCombatSoakRequest(
            "req-soak", defenseResult.UpdatedState, 4, WerewolfDamageCategory.Bashing, damageInterpretation.TotalDamage));
        var soakInterpretation = WerewolfCombatSoakService.InterpretSoakRoll(soakDefinition, [6]);

        var netBashing = Math.Max(0, damageInterpretation.TotalDamage - soakInterpretation.SoakSuccesses);
        var applyResult = WerewolfApplyDamageService.ApplyDamage(new WerewolfApplyDamageRequest(
            "req-apply", defenseResult.UpdatedState, defenseResult.UpdatedState.RuntimeStateVersion, WerewolfDamageCategory.Bashing, netBashing));

        Assert.True(applyResult.Succeeded);
        Assert.Equal(4, applyResult.UpdatedState!.RuntimeStateVersion);
    }

    private static WerewolfRuntimeCharacterState BuildState(WerewolfHealthTrack? healthTrack = null, string form = WerewolfFormIdentifiers.Homid, string birthRace = WerewolfRaceIdentifiers.Homid)
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
            birthRace, track, form);
    }
}
