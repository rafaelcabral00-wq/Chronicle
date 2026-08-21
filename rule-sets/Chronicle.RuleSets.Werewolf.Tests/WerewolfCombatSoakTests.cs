using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfCombatSoakTests
{
    [Fact]
    public void DefineSoakRollBashingRacialFormReturnsVigorPool()
    {
        var state = State(form: WerewolfFormIdentifiers.Homid, birthRace: WerewolfRaceIdentifiers.Homid, stamina: 3);
        var request = new WerewolfCombatSoakRequest("req", state, 1, WerewolfDamageCategory.Bashing, 2);

        var definition = WerewolfCombatSoakService.DefineSoakRoll(request);

        Assert.False(definition.IsSilver);
        Assert.True(definition.IsRacialForm);
        Assert.False(definition.SoakBlocked);
        Assert.Equal(3, definition.SoakPoolSize);
        Assert.Equal(6, definition.Difficulty);
    }

    [Fact]
    public void DefineSoakRollLethalRacialFormMetisBlocksSoak()
    {
        var state = State(form: WerewolfFormIdentifiers.Crinos, birthRace: WerewolfRaceIdentifiers.Metis, stamina: 4);
        var request = new WerewolfCombatSoakRequest("req", state, 1, WerewolfDamageCategory.Lethal, 3);

        var definition = WerewolfCombatSoakService.DefineSoakRoll(request);

        Assert.False(definition.IsSilver);
        Assert.True(definition.IsRacialForm);
        Assert.True(definition.SoakBlocked);
        Assert.Equal(0, definition.SoakPoolSize);
    }

    [Fact]
    public void DefineSoakRollLethalNonRacialFormAllowsSoak()
    {
        var state = State(form: WerewolfFormIdentifiers.Glabro, birthRace: WerewolfRaceIdentifiers.Homid, stamina: 4);
        var request = new WerewolfCombatSoakRequest("req", state, 1, WerewolfDamageCategory.Lethal, 3);

        var definition = WerewolfCombatSoakService.DefineSoakRoll(request);

        Assert.False(definition.IsSilver);
        Assert.False(definition.IsRacialForm);
        Assert.False(definition.SoakBlocked);
        Assert.Equal(6, definition.SoakPoolSize);
    }

    [Fact]
    public void DefineSoakRollAggravatedRacialFormHomidReturnsVigorPool()
    {
        var state = State(form: WerewolfFormIdentifiers.Homid, birthRace: WerewolfRaceIdentifiers.Homid, stamina: 5);
        var request = new WerewolfCombatSoakRequest("req", state, 1, WerewolfDamageCategory.Aggravated, 4);

        var definition = WerewolfCombatSoakService.DefineSoakRoll(request);

        Assert.True(definition.IsSilver);
        Assert.True(definition.IsRacialForm);
        Assert.False(definition.SoakBlocked);
        Assert.Equal(5, definition.SoakPoolSize);
    }

    [Fact]
    public void DefineSoakRollAggravatedNonRacialFormBlocksSoak()
    {
        var state = State(form: WerewolfFormIdentifiers.Crinos, birthRace: WerewolfRaceIdentifiers.Homid, stamina: 3);
        var request = new WerewolfCombatSoakRequest("req", state, 1, WerewolfDamageCategory.Aggravated, 2);

        var definition = WerewolfCombatSoakService.DefineSoakRoll(request);

        Assert.True(definition.IsSilver);
        Assert.False(definition.IsRacialForm);
        Assert.True(definition.SoakBlocked);
        Assert.Equal(0, definition.SoakPoolSize);
    }

    [Fact]
    public void DefineSoakRollSilverRacialFormMetisBlocksSoak()
    {
        var state = State(form: WerewolfFormIdentifiers.Crinos, birthRace: WerewolfRaceIdentifiers.Metis, stamina: 4);
        var request = new WerewolfCombatSoakRequest("req", state, 1, WerewolfDamageCategory.Aggravated, 3);

        var definition = WerewolfCombatSoakService.DefineSoakRoll(request);

        Assert.True(definition.IsSilver);
        Assert.True(definition.IsRacialForm);
        Assert.True(definition.SoakBlocked);
        Assert.Equal(0, definition.SoakPoolSize);
    }

    [Fact]
    public void DefineSoakRollIncomingDamageZeroReturnsFailure()
    {
        var state = State();
        var request = new WerewolfCombatSoakRequest("req", state, 1, WerewolfDamageCategory.Bashing, 0);

        var definition = WerewolfCombatSoakService.DefineSoakRoll(request);

        Assert.Equal(0, definition.SoakPoolSize);
        Assert.Contains(definition.Findings, f => f.Contains("Incoming damage must be positive"));
    }

    [Fact]
    public void DefineSoakRollInvalidDamageTypeReturnsFailure()
    {
        var state = State();
        var request = new WerewolfCombatSoakRequest("req", state, 1, (WerewolfDamageCategory)999, 1);

        var definition = WerewolfCombatSoakService.DefineSoakRoll(request);

        Assert.Equal(0, definition.SoakPoolSize);
        Assert.Contains(definition.Findings, f => f.Contains("Invalid damage type"));
    }

    [Fact]
    public void DefineSoakRollVersionMismatchReturnsFailure()
    {
        var state = State();
        var request = new WerewolfCombatSoakRequest("req", state, 2, WerewolfDamageCategory.Bashing, 1);

        var definition = WerewolfCombatSoakService.DefineSoakRoll(request);

        Assert.Equal(0, definition.SoakPoolSize);
        Assert.Contains(definition.Findings, f => f.Contains("Version mismatch"));
    }

    [Fact]
    public void InterpretSoakRollAllSuccessesAbsorbsAllDamage()
    {
        var definition = new WerewolfCombatSoakRollDefinition(
            "req", 3, 6, true, false, false, []);

        var result = WerewolfCombatSoakService.InterpretSoakRoll(definition, [6, 7, 8]);

        Assert.Equal(3, result.SoakSuccesses);
        Assert.False(result.SoakBlocked);
    }

    [Fact]
    public void InterpretSoakRollZeroSuccessesAbsorbsNoDamage()
    {
        var definition = new WerewolfCombatSoakRollDefinition(
            "req", 3, 6, true, false, false, []);

        var result = WerewolfCombatSoakService.InterpretSoakRoll(definition, [2, 3, 5]);

        Assert.Equal(0, result.SoakSuccesses);
    }

    [Fact]
    public void InterpretSoakRollDiceMismatchReturnsZeroSuccesses()
    {
        var definition = new WerewolfCombatSoakRollDefinition(
            "req", 3, 6, true, false, false, []);

        var result = WerewolfCombatSoakService.InterpretSoakRoll(definition, [6, 7]);

        Assert.Equal(0, result.SoakSuccesses);
        Assert.Contains(result.Findings, f => f.Contains("Dice count mismatch"));
    }

    [Fact]
    public void InterpretSoakRollSilverNonRacialBlocksSoak()
    {
        var definition = new WerewolfCombatSoakRollDefinition(
            "req", 3, 6, false, true, true, []);

        var result = WerewolfCombatSoakService.InterpretSoakRoll(definition, [6, 7, 8]);

        Assert.True(result.SoakBlocked);
        Assert.Equal(0, result.SoakSuccesses);
        Assert.Contains(result.Findings, f => f.Contains("Silver soak blocked"));
    }

    [Fact]
    public void DefineSoakRollToughHideAddsOneDieInNonRacialLethal()
    {
        var state = State(form: WerewolfFormIdentifiers.Glabro, birthRace: WerewolfRaceIdentifiers.Metis, stamina: 3, hasToughHide: true);
        var request = new WerewolfCombatSoakRequest("req", state, 1, WerewolfDamageCategory.Lethal, 2);

        var definition = WerewolfCombatSoakService.DefineSoakRoll(request);

        Assert.Equal(6, definition.SoakPoolSize);
        Assert.Contains(definition.Findings, f => f.Contains("ToughHide 1"));
    }

    [Fact]
    public void DefineSoakRollNoToughHideReturnsNormalPoolInNonRacialLethal()
    {
        var state = State(form: WerewolfFormIdentifiers.Glabro, birthRace: WerewolfRaceIdentifiers.Metis, stamina: 3, hasToughHide: false);
        var request = new WerewolfCombatSoakRequest("req", state, 1, WerewolfDamageCategory.Lethal, 2);

        var definition = WerewolfCombatSoakService.DefineSoakRoll(request);

        Assert.Equal(5, definition.SoakPoolSize);
        Assert.Contains(definition.Findings, f => f.Contains("ToughHide 0"));
    }

    [Fact]
    public void DefineSoakRollToughHideDoesNotAddDieWhenSoakBlocked()
    {
        var state = State(form: WerewolfFormIdentifiers.Crinos, birthRace: WerewolfRaceIdentifiers.Metis, stamina: 3, hasToughHide: true);
        var request = new WerewolfCombatSoakRequest("req", state, 1, WerewolfDamageCategory.Lethal, 2);

        var definition = WerewolfCombatSoakService.DefineSoakRoll(request);

        Assert.Equal(0, definition.SoakPoolSize);
        Assert.True(definition.SoakBlocked);
    }

    private static WerewolfRuntimeCharacterState State(string form = "character.form.homid", string birthRace = "homid", int stamina = 3, bool hasToughHide = false)
    {
        var attributes = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["attributes"] = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, int>
            {
                ["character.attribute.stamina"] = stamina
            })
        };

        if (hasToughHide)
        {
            var deformity = new List<WerewolfMetisDeformityEffect>
            {
                new WerewolfMetisDeformityEffect(WerewolfMetisDeformityEffectKind.ToughHide)
            };
            attributes["metis-deformities"] = System.Text.Json.JsonSerializer.Serialize(deformity);
        }

        return new WerewolfRuntimeCharacterState(
            "test", "1.0", "draft", 1, attributes,
            5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0,
            birthRace, null, form);
    }
}
