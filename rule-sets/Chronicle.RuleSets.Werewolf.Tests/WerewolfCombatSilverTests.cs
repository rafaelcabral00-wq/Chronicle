using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfCombatSilverTests
{
    [Fact]
    public void CalculateSilverInContactNonRacialFormReturnsVulnerable()
    {
        var result = WerewolfCombatSilverService.CalculateSilver(new WerewolfCombatSilverRequest(
            "req", State(form: WerewolfFormIdentifiers.Crinos, birthRace: WerewolfRaceIdentifiers.Homid), 1, true));

        Assert.True(result.Succeeded);
        Assert.True(result.IsVulnerable);
        Assert.False(result.IsRacialForm);
        Assert.Equal(1, result.SilverDamagePerTurn);
        Assert.Equal(2, result.UpdatedState!.RuntimeStateVersion);
    }

    [Fact]
    public void CalculateSilverInContactRacialFormReturnsNotVulnerable()
    {
        var result = WerewolfCombatSilverService.CalculateSilver(new WerewolfCombatSilverRequest(
            "req", State(form: WerewolfFormIdentifiers.Homid, birthRace: WerewolfRaceIdentifiers.Homid), 1, true));

        Assert.True(result.Succeeded);
        Assert.True(result.IsVulnerable);
        Assert.True(result.IsRacialForm);
        Assert.Equal(0, result.SilverDamagePerTurn);
    }

    [Fact]
    public void CalculateSilverNotInContactReturnsZeroDamage()
    {
        var result = WerewolfCombatSilverService.CalculateSilver(new WerewolfCombatSilverRequest(
            "req", State(form: WerewolfFormIdentifiers.Crinos, birthRace: WerewolfRaceIdentifiers.Homid), 1, false));

        Assert.True(result.Succeeded);
        Assert.False(result.IsVulnerable);
        Assert.Equal(0, result.SilverDamagePerTurn);
    }

    [Fact]
    public void CalculateSilverVersionMismatchReturnsFailure()
    {
        var result = WerewolfCombatSilverService.CalculateSilver(new WerewolfCombatSilverRequest(
            "req", State(), 2, true));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void ApplySilverContactNonRacialFormReturnsTurnsAsDamage()
    {
        var state = State(form: WerewolfFormIdentifiers.Crinos, birthRace: WerewolfRaceIdentifiers.Homid);
        var damage = WerewolfCombatSilverService.ApplySilverContact(state, 3, false);

        Assert.Equal(3, damage);
    }

    [Fact]
    public void ApplySilverContactRacialFormReturnsZero()
    {
        var state = State(form: WerewolfFormIdentifiers.Lupus, birthRace: WerewolfRaceIdentifiers.Lupus);
        var damage = WerewolfCombatSilverService.ApplySilverContact(state, 5, false);

        Assert.Equal(0, damage);
    }

    [Fact]
    public void ApplySilverContactZeroOrNegativeTurnsReturnsZero()
    {
        var state = State(form: WerewolfFormIdentifiers.Crinos, birthRace: WerewolfRaceIdentifiers.Homid);
        Assert.Equal(0, WerewolfCombatSilverService.ApplySilverContact(state, 0, false));
        Assert.Equal(0, WerewolfCombatSilverService.ApplySilverContact(state, -1, false));
    }

    [Theory]
    [InlineData(WerewolfRaceIdentifiers.Homid, WerewolfFormIdentifiers.Homid, true)]
    [InlineData(WerewolfRaceIdentifiers.Metis, WerewolfFormIdentifiers.Crinos, true)]
    [InlineData(WerewolfRaceIdentifiers.Lupus, WerewolfFormIdentifiers.Lupus, true)]
    [InlineData(WerewolfRaceIdentifiers.Homid, WerewolfFormIdentifiers.Crinos, false)]
    [InlineData(WerewolfRaceIdentifiers.Metis, WerewolfFormIdentifiers.Homid, false)]
    [InlineData(WerewolfRaceIdentifiers.Lupus, WerewolfFormIdentifiers.Hispo, false)]
    public void ApplySilverContactBirthRaceAndCurrentFormDetermineImmunity(string birthRace, string currentForm, bool expectImmune)
    {
        var state = State(form: currentForm, birthRace: birthRace);
        var damage = WerewolfCombatSilverService.ApplySilverContact(state, 2, false);

        Assert.Equal(expectImmune ? 0 : 2, damage);
    }

    private static WerewolfRuntimeCharacterState State(string form = "character.form.homid", string birthRace = "homid")
    {
        return new WerewolfRuntimeCharacterState(
            "test", "1.0", "draft", 1, new Dictionary<string, string>(),
            5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0,
            birthRace, null, form);
    }
}

