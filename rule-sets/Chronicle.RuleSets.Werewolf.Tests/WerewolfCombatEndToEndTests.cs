using Chronicle.RuleSets.Werewolf.CharacterCreation;
using System.Linq;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfCombatEndToEndTests
{
    [Fact]
    public void FullCombatScenarioProducesCorrectFinalState()
    {
        var attributes = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["character.attribute.strength"] = "4",
            ["character.attribute.dexterity"] = "3",
            ["character.attribute.stamina"] = "3"
        };

        var state = new WerewolfRuntimeCharacterState(
            "test", "1.0", "draft", 1, attributes,
            5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0,
            WerewolfRaceIdentifiers.Metis,
            WerewolfHealthTrackComputer.Compute([]),
            WerewolfFormIdentifiers.Crinos);

        var initiativeResult = WerewolfCombatInitiativeService.CalculateInitiative(new WerewolfCombatInitiativeRequest(
            "req-1", state, 1, 3, 2, 7, null));
        Assert.True(initiativeResult.Succeeded);
        Assert.Equal(5, initiativeResult.InitiativeModifier);
        Assert.Equal(7, initiativeResult.SuppliedDieRoll);
        Assert.Equal(12, initiativeResult.FinalInitiative);
        Assert.Equal(2, initiativeResult.UpdatedState!.RuntimeStateVersion);

        var attackResult = WerewolfCombatAttackDefinitionService.ResolveAttack(WerewolfCombatIdentifiers.Claw);
        Assert.Equal(WerewolfCombatIdentifiers.Claw, attackResult.AttackId);

        var defenseResult = WerewolfCombatDefenseService.CalculateDefense(new WerewolfCombatDefenseRequest(
            "req-2", initiativeResult.UpdatedState, 2, "dodge", 2, null, null, 3, null));
        Assert.True(defenseResult.Succeeded);
        Assert.Equal(5, defenseResult.DefensePool);
        Assert.Equal(3, defenseResult.UpdatedState!.RuntimeStateVersion);

        var damageDefinition = WerewolfCombatDamageService.DefineDamageRoll(new WerewolfCombatDamageRequest(
            "req-3", defenseResult.UpdatedState, 3, 3, "Strength", WerewolfDamageCategory.Aggravated.ToString(), 1));
        Assert.True(damageDefinition.Findings.Count > 0);
        Assert.Equal(3, damageDefinition.DamagePoolSize);

        var damageInterpretation = WerewolfCombatDamageService.InterpretDamageRoll(damageDefinition, [8, 9, 10]);
        Assert.Equal(3, damageInterpretation.DamageSuccesses);
        Assert.Equal(3, damageInterpretation.TotalDamage);

        var soakDefinition = WerewolfCombatSoakService.DefineSoakRoll(new WerewolfCombatSoakRequest(
            "req-4", defenseResult.UpdatedState, 3, WerewolfDamageCategory.Aggravated, 3));
        Assert.True(soakDefinition.IsRacialForm);
        Assert.True(soakDefinition.SoakBlocked);

        var applyResult = WerewolfApplyDamageService.ApplyDamage(new WerewolfApplyDamageRequest(
            "req-5", defenseResult.UpdatedState, 3, WerewolfDamageCategory.Aggravated, 3));
        Assert.True(applyResult.Succeeded);
        Assert.Equal(3, applyResult.HealthTrack.TotalDamage);
        Assert.Equal(WerewolfHealthState.Wounded, applyResult.HealthTrack.HealthState);
        Assert.Equal(4, applyResult.UpdatedState!.RuntimeStateVersion);
        Assert.Equal(WerewolfRaceIdentifiers.Metis, applyResult.UpdatedState.BirthRace);
        Assert.Equal(WerewolfFormIdentifiers.Crinos, applyResult.UpdatedState.CurrentForm);
    }

    [Fact]
    public void FullCombatScenarioBashingResultProducesCorrectHealthState()
    {
        var state = new WerewolfRuntimeCharacterState(
            "test", "1.0", "draft", 1, new Dictionary<string, string>(),
            5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0,
            WerewolfRaceIdentifiers.Homid,
            WerewolfHealthTrackComputer.Compute([]),
            WerewolfFormIdentifiers.Homid);

        var damageDefinition = WerewolfCombatDamageService.DefineDamageRoll(new WerewolfCombatDamageRequest(
            "req-1", state, 1, 2, "Strength + 1", WerewolfDamageCategory.Bashing.ToString(), null));
        var damageInterpretation = WerewolfCombatDamageService.InterpretDamageRoll(damageDefinition, [6, 7, 8]);

        var soakDefinition = WerewolfCombatSoakService.DefineSoakRoll(new WerewolfCombatSoakRequest(
            "req-2", state, 1, WerewolfDamageCategory.Bashing, damageInterpretation.TotalDamage));
        var soakInterpretation = WerewolfCombatSoakService.InterpretSoakRoll(soakDefinition, [6, 7, 8]);

        var netBashing = Math.Max(0, damageInterpretation.TotalDamage - soakInterpretation.SoakSuccesses);
        var applyResult = netBashing > 0
            ? WerewolfApplyDamageService.ApplyDamage(new WerewolfApplyDamageRequest(
                "req-3", state, 1, WerewolfDamageCategory.Bashing, netBashing))
            : new WerewolfApplyDamageResult(true, state, [], state.HealthTrack!, [], "none", "req-3");

        Assert.True(applyResult.Succeeded);
        Assert.Equal(0, applyResult.HealthTrack.TotalDamage);
        Assert.Equal(WerewolfHealthState.Healthy, applyResult.HealthTrack.HealthState);
    }

    [Fact]
    public void FullCombatScenarioLethalResultProducesCorrectHealthState()
    {
        var state = new WerewolfRuntimeCharacterState(
            "test", "1.0", "draft", 1, new Dictionary<string, string>(),
            5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0,
            WerewolfRaceIdentifiers.Homid,
            WerewolfHealthTrackComputer.Compute([]),
            WerewolfFormIdentifiers.Glabro);

        var damageDefinition = WerewolfCombatDamageService.DefineDamageRoll(new WerewolfCombatDamageRequest(
            "req-1", state, 1, 4, "Variable", WerewolfDamageCategory.Lethal.ToString(), 1));
        var damageInterpretation = WerewolfCombatDamageService.InterpretDamageRoll(damageDefinition, [8, 9, 10, 10]);

        var soakDefinition = WerewolfCombatSoakService.DefineSoakRoll(new WerewolfCombatSoakRequest(
            "req-2", state, 1, WerewolfDamageCategory.Lethal, damageInterpretation.TotalDamage));
        var soakInterpretation = WerewolfCombatSoakService.InterpretSoakRoll(soakDefinition, [6, 7, 8]);

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
    public void FullCombatScenarioAggravatedResultProducesCorrectHealthState()
    {
        var state = new WerewolfRuntimeCharacterState(
            "test", "1.0", "draft", 1, new Dictionary<string, string>(),
            5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0,
            WerewolfRaceIdentifiers.Lupus,
            WerewolfHealthTrackComputer.Compute([]),
            WerewolfFormIdentifiers.Lupus);

        var damageDefinition = WerewolfCombatDamageService.DefineDamageRoll(new WerewolfCombatDamageRequest(
            "req-1", state, 1, 3, "Strength", WerewolfDamageCategory.Aggravated.ToString(), 1));
        var damageInterpretation = WerewolfCombatDamageService.InterpretDamageRoll(damageDefinition, [8, 9, 10]);

        var soakDefinition = WerewolfCombatSoakService.DefineSoakRoll(new WerewolfCombatSoakRequest(
            "req-2", state, 1, WerewolfDamageCategory.Aggravated, damageInterpretation.TotalDamage));
        var soakInterpretation = WerewolfCombatSoakService.InterpretSoakRoll(soakDefinition, [6, 7, 8]);

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
    public void FullCombatScenarioIncapacitatedBoundaryProducesIncapacitatedState()
    {
        var nearIncapacitationTrack = WerewolfHealthTrackComputer.Compute(
            Enumerable.Repeat(new WerewolfDamageMark(WerewolfDamageCategory.Bashing, 1), 5).ToList());
        var state = new WerewolfRuntimeCharacterState(
            "test", "1.0", "draft", 1, new Dictionary<string, string>(),
            5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0,
            WerewolfRaceIdentifiers.Homid,
            nearIncapacitationTrack,
            WerewolfFormIdentifiers.Homid);

        var applyResult = WerewolfApplyDamageService.ApplyDamage(new WerewolfApplyDamageRequest(
            "req-1", state, 1, WerewolfDamageCategory.Bashing, 1));
        Assert.True(applyResult.Succeeded);
        Assert.Equal(WerewolfHealthState.Incapacitated, applyResult.HealthTrack.HealthState);
    }

    [Fact]
    public void FullCombatScenarioRuntimeStateVersionIncrementsExactlyOncePerStep()
    {
        var state = new WerewolfRuntimeCharacterState(
            "test", "1.0", "draft", 1, new Dictionary<string, string>(),
            5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0,
            WerewolfRaceIdentifiers.Homid,
            WerewolfHealthTrackComputer.Compute([]),
            WerewolfFormIdentifiers.Homid);

        var initiativeResult = WerewolfCombatInitiativeService.CalculateInitiative(new WerewolfCombatInitiativeRequest(
            "req-1", state, 1, 3, 2, 7, null));
        Assert.Equal(2, initiativeResult.UpdatedState!.RuntimeStateVersion);

        var defenseResult = WerewolfCombatDefenseService.CalculateDefense(new WerewolfCombatDefenseRequest(
            "req-2", initiativeResult.UpdatedState, 2, "dodge", 2, null, null, 3, null));
        Assert.Equal(3, defenseResult.UpdatedState!.RuntimeStateVersion);

        var damageDefinition = WerewolfCombatDamageService.DefineDamageRoll(new WerewolfCombatDamageRequest(
            "req-3", defenseResult.UpdatedState, 3, 2, "Strength", WerewolfDamageCategory.Bashing.ToString(), null));
        Assert.True(damageDefinition.Findings.Count > 0);
    }

    [Fact]
    public void FullCombatScenarioNoInternalRandomNumberGeneration()
    {
        var state = new WerewolfRuntimeCharacterState(
            "test", "1.0", "draft", 1, new Dictionary<string, string>(),
            5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0,
            WerewolfRaceIdentifiers.Homid,
            WerewolfHealthTrackComputer.Compute([]),
            WerewolfFormIdentifiers.Homid);

        var initiativeResult = WerewolfCombatInitiativeService.CalculateInitiative(new WerewolfCombatInitiativeRequest(
            "req-1", state, 1, 3, 2, 7, null));
        var defenseResult = WerewolfCombatDefenseService.CalculateDefense(new WerewolfCombatDefenseRequest(
            "req-2", initiativeResult.UpdatedState!, 2, "dodge", 2, null, null, 3, null));
        var damageDefinition = WerewolfCombatDamageService.DefineDamageRoll(new WerewolfCombatDamageRequest(
            "req-3", defenseResult.UpdatedState!, 3, 2, "Strength", WerewolfDamageCategory.Bashing.ToString(), null));
        var damageInterpretation = WerewolfCombatDamageService.InterpretDamageRoll(damageDefinition, [6, 7]);
        var soakDefinition = WerewolfCombatSoakService.DefineSoakRoll(new WerewolfCombatSoakRequest(
            "req-4", defenseResult.UpdatedState!, 4, WerewolfDamageCategory.Bashing, 2));

        Assert.True(initiativeResult.Succeeded);
        Assert.True(defenseResult.Succeeded);
        Assert.True(damageDefinition.Findings.Count > 0);
        Assert.Equal(2, damageInterpretation.DiceValues.Count);
        Assert.True(soakDefinition.Findings.Count > 0);
    }
}
