using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfCombatNaturalWeaponTests
{
    [Fact]
    public void CrinosFormCatalogContainsClawAndBiteNaturalWeapons()
    {
        var crinos = WerewolfFormCatalog.Entries.First(f => f.FormId == WerewolfFormIdentifiers.Crinos);
        var naturalWeapons = crinos.Effects.Where(e => e.Kind == WerewolfFormEffectKind.NaturalWeapon).ToList();

        Assert.Equal(2, naturalWeapons.Count);
        Assert.Equal("claw", naturalWeapons[0].Target);
        Assert.Equal("bite", naturalWeapons[1].Target);
    }

    [Fact]
    public void CrinosAttackCatalogClawAndBiteAreNaturalWeapons()
    {
        var claw = WerewolfCombatAttackCatalog.Entries.First(a => a.AttackId == WerewolfCombatIdentifiers.Claw);
        var bite = WerewolfCombatAttackCatalog.Entries.First(a => a.AttackId == WerewolfCombatIdentifiers.Bite);

        Assert.True(claw.IsNaturalWeapon);
        Assert.True(bite.IsNaturalWeapon);
        Assert.Equal("claw", claw.NaturalWeaponTarget);
        Assert.Equal("bite", bite.NaturalWeaponTarget);
    }

    [Fact]
    public void CrinosAttackCatalogBiteAndClawAllowedInCrinos()
    {
        var claw = WerewolfCombatAttackCatalog.Entries.First(a => a.AttackId == WerewolfCombatIdentifiers.Claw);
        var bite = WerewolfCombatAttackCatalog.Entries.First(a => a.AttackId == WerewolfCombatIdentifiers.Bite);

        Assert.Contains(WerewolfFormIdentifiers.Crinos, claw.AllowedForms);
        Assert.Contains(WerewolfFormIdentifiers.Crinos, bite.AllowedForms);
    }

    [Fact]
    public void HispoFormCatalogContainsClawNaturalWeapon()
    {
        var hispo = WerewolfFormCatalog.Entries.First(f => f.FormId == WerewolfFormIdentifiers.Hispo);
        var naturalWeapons = hispo.Effects.Where(e => e.Kind == WerewolfFormEffectKind.NaturalWeapon).ToList();

        Assert.Single(naturalWeapons);
        Assert.Equal("claw", naturalWeapons[0].Target);
    }

    [Fact]
    public void HispoAttackCatalogClawAllowedInHispo()
    {
        var claw = WerewolfCombatAttackCatalog.Entries.First(a => a.AttackId == WerewolfCombatIdentifiers.Claw);

        Assert.Contains(WerewolfFormIdentifiers.Hispo, claw.AllowedForms);
    }

    [Fact]
    public void HispoAttackCatalogBiteNotAllowedInHispo()
    {
        var bite = WerewolfCombatAttackCatalog.Entries.First(a => a.AttackId == WerewolfCombatIdentifiers.Bite);

        Assert.DoesNotContain(WerewolfFormIdentifiers.Hispo, bite.AllowedForms);
    }

    [Fact]
    public void LupusFormCatalogContainsBiteNaturalWeapon()
    {
        var lupus = WerewolfFormCatalog.Entries.First(f => f.FormId == WerewolfFormIdentifiers.Lupus);
        var naturalWeapons = lupus.Effects.Where(e => e.Kind == WerewolfFormEffectKind.NaturalWeapon).ToList();

        Assert.Single(naturalWeapons);
        Assert.Equal("bite", naturalWeapons[0].Target);
    }

    [Fact]
    public void LupusAttackCatalogBiteAllowedInLupus()
    {
        var bite = WerewolfCombatAttackCatalog.Entries.First(a => a.AttackId == WerewolfCombatIdentifiers.Bite);

        Assert.Contains(WerewolfFormIdentifiers.Lupus, bite.AllowedForms);
    }

    [Fact]
    public void LupusAttackCatalogClawNotAllowedInLupus()
    {
        var claw = WerewolfCombatAttackCatalog.Entries.First(a => a.AttackId == WerewolfCombatIdentifiers.Claw);

        Assert.DoesNotContain(WerewolfFormIdentifiers.Lupus, claw.AllowedForms);
    }

    [Fact]
    public void HomidFormCatalogContainsNoNaturalWeapons()
    {
        var homid = WerewolfFormCatalog.Entries.First(f => f.FormId == WerewolfFormIdentifiers.Homid);
        var naturalWeapons = homid.Effects.Where(e => e.Kind == WerewolfFormEffectKind.NaturalWeapon).ToList();

        Assert.Empty(naturalWeapons);
    }

    [Fact]
    public void GlabroFormCatalogContainsNoNaturalWeapons()
    {
        var glabro = WerewolfFormCatalog.Entries.First(f => f.FormId == WerewolfFormIdentifiers.Glabro);
        var naturalWeapons = glabro.Effects.Where(e => e.Kind == WerewolfFormEffectKind.NaturalWeapon).ToList();

        Assert.Empty(naturalWeapons);
    }

    [Fact]
    public void AttackCatalogInvalidFormCombinationRejected()
    {
        var bite = WerewolfCombatAttackCatalog.Entries.First(a => a.AttackId == WerewolfCombatIdentifiers.Bite);
        var claw = WerewolfCombatAttackCatalog.Entries.First(a => a.AttackId == WerewolfCombatIdentifiers.Claw);

        Assert.DoesNotContain(WerewolfFormIdentifiers.Homid, bite.AllowedForms);
        Assert.DoesNotContain(WerewolfFormIdentifiers.Lupus, claw.AllowedForms);
    }
}

