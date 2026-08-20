using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfFormCatalogTests
{
    [Fact]
    public void FormCatalogContainsExactlyFiveSourceDefinedForms()
    {
        Assert.Equal(5, WerewolfFormCatalog.Entries.Count);
    }

    [Fact]
    public void FormKeysAreLanguageNeutralAndWhitespaceFree()
    {
        Assert.All(WerewolfFormIdentifiers.Supported, key =>
        {
            Assert.DoesNotContain(key, c => char.IsWhiteSpace(c));
            Assert.StartsWith("character.form.", key);
        });
    }

    [Fact]
    public void FormKeysAreUnique()
    {
        Assert.Equal(WerewolfFormIdentifiers.Supported.Count, WerewolfFormIdentifiers.Supported.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void AllSourceDefinedFormsArePresent()
    {
        Assert.Contains(WerewolfFormIdentifiers.Homid, WerewolfFormIdentifiers.Supported);
        Assert.Contains(WerewolfFormIdentifiers.Glabro, WerewolfFormIdentifiers.Supported);
        Assert.Contains(WerewolfFormIdentifiers.Crinos, WerewolfFormIdentifiers.Supported);
        Assert.Contains(WerewolfFormIdentifiers.Hispo, WerewolfFormIdentifiers.Supported);
        Assert.Contains(WerewolfFormIdentifiers.Lupus, WerewolfFormIdentifiers.Supported);
    }

    [Fact]
    public void EachFormHasLocalizationKey()
    {
        Assert.All(WerewolfFormCatalog.Entries, entry =>
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.LocalizationKey));
            Assert.StartsWith("character.form.", entry.LocalizationKey);
        });
    }

    [Fact]
    public void EachFormHasSourceLocator()
    {
        Assert.All(WerewolfFormCatalog.Entries, entry =>
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.SourceLocator));
            Assert.StartsWith("Lines ", entry.SourceLocator);
        });
    }

    [Fact]
    public void HomidIsBirthFormForHomidRace()
    {
        var homid = WerewolfFormCatalog.Entries.First(f => f.FormId == WerewolfFormIdentifiers.Homid);
        Assert.Equal(WerewolfRaceIdentifiers.Homid, homid.BirthForm);
    }

    [Fact]
    public void CrinosIsBirthFormForMetisRace()
    {
        var crinos = WerewolfFormCatalog.Entries.First(f => f.FormId == WerewolfFormIdentifiers.Crinos);
        Assert.Equal(WerewolfRaceIdentifiers.Metis, crinos.BirthForm);
    }

    [Fact]
    public void LupusIsBirthFormForLupusRace()
    {
        var lupus = WerewolfFormCatalog.Entries.First(f => f.FormId == WerewolfFormIdentifiers.Lupus);
        Assert.Equal(WerewolfRaceIdentifiers.Lupus, lupus.BirthForm);
    }

    [Fact]
    public void GlabroAndHispoHaveNoBirthForm()
    {
        var glabro = WerewolfFormCatalog.Entries.First(f => f.FormId == WerewolfFormIdentifiers.Glabro);
        var hispo = WerewolfFormCatalog.Entries.First(f => f.FormId == WerewolfFormIdentifiers.Hispo);
        Assert.Null(glabro.BirthForm);
        Assert.Null(hispo.BirthForm);
    }

    [Fact]
    public void CrinosHasDeliriumTriggerEffect()
    {
        var crinos = WerewolfFormCatalog.Entries.First(f => f.FormId == WerewolfFormIdentifiers.Crinos);
        Assert.Contains(crinos.Effects, e => e.Kind == WerewolfFormEffectKind.DeliriumTrigger);
    }

    [Fact]
    public void HispoBiteHasExtraDamageNote()
    {
        var hispo = WerewolfFormCatalog.Entries.First(f => f.FormId == WerewolfFormIdentifiers.Hispo);
        var biteEffect = hispo.Effects.First(e => e.Target == "bite");
        Assert.Equal("Massive jaws; bite deals +1 extra damage die", biteEffect.Notes);
    }

    [Fact]
    public void LupusHasSpeedMovementModifier()
    {
        var lupus = WerewolfFormCatalog.Entries.First(f => f.FormId == WerewolfFormIdentifiers.Lupus);
        Assert.Equal(2, lupus.MovementModifiers["speed"]);
    }

    [Fact]
    public void LupusHasPerceptionSensoryModifier()
    {
        var lupus = WerewolfFormCatalog.Entries.First(f => f.FormId == WerewolfFormIdentifiers.Lupus);
        Assert.Equal(-2, lupus.SensoryModifiers[WerewolfAttributeIdentifiers.Perception]);
    }

    [Fact]
    public void HispoHasPerceptionSensoryModifier()
    {
        var hispo = WerewolfFormCatalog.Entries.First(f => f.FormId == WerewolfFormIdentifiers.Hispo);
        Assert.Equal(-1, hispo.SensoryModifiers[WerewolfAttributeIdentifiers.Perception]);
    }

    [Fact]
    public void CrinosHasZeroManipulationAndAppearance()
    {
        var crinos = WerewolfFormCatalog.Entries.First(f => f.FormId == WerewolfFormIdentifiers.Crinos);
        var manipulation = crinos.AttributeModifiers.First(m => m.AttributeId == WerewolfAttributeIdentifiers.Manipulation);
        var appearance = crinos.AttributeModifiers.First(m => m.AttributeId == WerewolfAttributeIdentifiers.Appearance);
        Assert.True(manipulation.IsAbsolute);
        Assert.Equal(0, manipulation.Value);
        Assert.True(appearance.IsAbsolute);
        Assert.Equal(0, appearance.Value);
    }
}
