using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfCombatManeuverTests
{
    [Fact]
    public void ManeuverCatalogContainsExactlyFourteenManeuvers()
    {
        Assert.Equal(15, WerewolfCombatManeuverCatalog.Entries.Count);
    }

    [Fact]
    public void ManeuverCatalogAllEntriesHaveUniqueStableKeys()
    {
        var keys = WerewolfCombatManeuverCatalog.Entries.Select(m => m.ManeuverId).ToList();
        Assert.Equal(15, keys.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void ManeuverCatalogEveryEntryHasSourceLocator()
    {
        foreach (var maneuver in WerewolfCombatManeuverCatalog.Entries)
        {
            Assert.False(string.IsNullOrWhiteSpace(maneuver.SourceLocator));
            Assert.StartsWith("Lines", maneuver.SourceLocator);
        }
    }

    [Fact]
    public void ManeuverCatalogEveryEntryHasNonNegativeBaseDifficulty()
    {
        foreach (var maneuver in WerewolfCombatManeuverCatalog.Entries)
        {
            Assert.True(maneuver.BaseDifficulty >= 0);
        }
    }

    [Fact]
    public void ManeuverCatalogEveryEntryHasActionCost()
    {
        foreach (var maneuver in WerewolfCombatManeuverCatalog.Entries)
        {
            Assert.True(maneuver.ActionCost >= 1);
        }
    }

    [Fact]
    public void ManeuverCatalogBiteHasCorrectProperties()
    {
        var bite = WerewolfCombatManeuverCatalog.Entries.First(m => m.ManeuverId == WerewolfCombatIdentifiers.Bite);

        Assert.Equal(WerewolfAbilityIdentifiers.Brawl, bite.AttackAbility);
        Assert.Equal(5, bite.BaseDifficulty);
        Assert.Equal("Strength + 1", bite.DamageExpression);
        Assert.Equal(WerewolfDamageCategory.Aggravated.ToString(), bite.DamageCategory);
        Assert.Equal(1, bite.ActionCost);
        Assert.Contains(WerewolfFormIdentifiers.Crinos, bite.AllowedForms);
        Assert.Contains(WerewolfFormIdentifiers.Lupus, bite.AllowedForms);
    }

    [Fact]
    public void ManeuverCatalogClawHasCorrectProperties()
    {
        var claw = WerewolfCombatManeuverCatalog.Entries.First(m => m.ManeuverId == WerewolfCombatIdentifiers.Claw);

        Assert.Equal(WerewolfAbilityIdentifiers.Brawl, claw.AttackAbility);
        Assert.Equal(6, claw.BaseDifficulty);
        Assert.Equal("Strength + 1", claw.DamageExpression);
        Assert.Equal(WerewolfDamageCategory.Aggravated.ToString(), claw.DamageCategory);
        Assert.Contains(WerewolfFormIdentifiers.Crinos, claw.AllowedForms);
        Assert.Contains(WerewolfFormIdentifiers.Hispo, claw.AllowedForms);
    }

    [Fact]
    public void ManeuverCatalogTackleHasCorrectProperties()
    {
        var tackle = WerewolfCombatManeuverCatalog.Entries.First(m => m.ManeuverId == WerewolfCombatIdentifiers.Tackle);

        Assert.Equal(WerewolfAbilityIdentifiers.Brawl, tackle.AttackAbility);
        Assert.Equal(7, tackle.BaseDifficulty);
        Assert.Equal("Strength", tackle.DamageExpression);
        Assert.Equal(WerewolfDamageCategory.Bashing.ToString(), tackle.DamageCategory);
    }

    [Fact]
    public void ManeuverCatalogDisarmHasCorrectProperties()
    {
        var disarm = WerewolfCombatManeuverCatalog.Entries.First(m => m.ManeuverId == WerewolfCombatIdentifiers.Disarm);

        Assert.Equal(WerewolfAbilityIdentifiers.Melee, disarm.AttackAbility);
        Assert.Equal(0, disarm.BaseDifficulty);
        Assert.Equal(1, disarm.DifficultyModifier);
        Assert.True(disarm.RequiresWeapon);
    }

    [Fact]
    public void ManeuverCatalogGrappleHasCorrectProperties()
    {
        var grapple = WerewolfCombatManeuverCatalog.Entries.First(m => m.ManeuverId == WerewolfCombatIdentifiers.Grapple);

        Assert.Equal(WerewolfAbilityIdentifiers.Brawl, grapple.AttackAbility);
        Assert.Equal(6, grapple.BaseDifficulty);
        Assert.Equal("Strength", grapple.DamageExpression);
    }

    [Fact]
    public void ManeuverCatalogKickHasCorrectProperties()
    {
        var kick = WerewolfCombatManeuverCatalog.Entries.First(m => m.ManeuverId == WerewolfCombatIdentifiers.Kick);

        Assert.Equal(WerewolfAbilityIdentifiers.Brawl, kick.AttackAbility);
        Assert.Equal(7, kick.BaseDifficulty);
        Assert.Equal("Strength + 1", kick.DamageExpression);
    }

    [Fact]
    public void ManeuverCatalogPunchHasCorrectProperties()
    {
        var punch = WerewolfCombatManeuverCatalog.Entries.First(m => m.ManeuverId == WerewolfCombatIdentifiers.Punch);

        Assert.Equal(WerewolfAbilityIdentifiers.Brawl, punch.AttackAbility);
        Assert.Equal(6, punch.BaseDifficulty);
        Assert.Equal("Strength", punch.DamageExpression);
    }

    [Fact]
    public void ManeuverCatalogSweepHasCorrectProperties()
    {
        var sweep = WerewolfCombatManeuverCatalog.Entries.First(m => m.ManeuverId == WerewolfCombatIdentifiers.Sweep);

        Assert.Equal(WerewolfAbilityIdentifiers.Brawl, sweep.AttackAbility);
        Assert.Equal(8, sweep.BaseDifficulty);
        Assert.Equal(1, sweep.DifficultyModifier);
        Assert.Equal("None", sweep.DamageExpression);
    }

    [Fact]
    public void ManeuverCatalogMeleeWeaponHasCorrectProperties()
    {
        var melee = WerewolfCombatManeuverCatalog.Entries.First(m => m.ManeuverId == WerewolfCombatIdentifiers.MeleeWeapon);

        Assert.Equal(WerewolfAbilityIdentifiers.Melee, melee.AttackAbility);
        Assert.Equal(0, melee.BaseDifficulty);
        Assert.True(melee.RequiresWeapon);
        Assert.Equal("Variable", melee.DamageExpression);
    }

    [Fact]
    public void ManeuverCatalogEvasiveActionHasCorrectProperties()
    {
        var evasive = WerewolfCombatManeuverCatalog.Entries.First(m => m.ManeuverId == WerewolfCombatIdentifiers.EvasiveAction);

        Assert.Equal(WerewolfAbilityIdentifiers.Dodge, evasive.AttackAbility);
        Assert.Equal(6, evasive.BaseDifficulty);
        Assert.True(evasive.IsSpecial);
        Assert.False(evasive.RequiresPrerequisiteSuccess);
    }

    [Fact]
    public void ManeuverCatalogIncapacitateHasCorrectProperties()
    {
        var incapacitate = WerewolfCombatManeuverCatalog.Entries.First(m => m.ManeuverId == WerewolfCombatIdentifiers.Incapacitate);

        Assert.Equal(WerewolfAbilityIdentifiers.Brawl, incapacitate.AttackAbility);
        Assert.Equal(8, incapacitate.BaseDifficulty);
        Assert.Equal("Strength + Cripple", incapacitate.DamageExpression);
        Assert.Equal(WerewolfDamageCategory.Aggravated.ToString(), incapacitate.DamageCategory);
    }

    [Fact]
    public void ManeuverCatalogIronMandibleHasCorrectProperties()
    {
        var ironMandible = WerewolfCombatManeuverCatalog.Entries.First(m => m.ManeuverId == WerewolfCombatIdentifiers.IronMandible);

        Assert.Equal(WerewolfAbilityIdentifiers.Brawl, ironMandible.AttackAbility);
        Assert.Equal(6, ironMandible.BaseDifficulty);
        Assert.Equal(1, ironMandible.DifficultyModifier);
        Assert.True(ironMandible.RequiresPrerequisiteSuccess);
        Assert.True(ironMandible.IsSpecial);
    }

    [Fact]
    public void ManeuverCatalogSavageLeapHasCorrectProperties()
    {
        var savageLeap = WerewolfCombatManeuverCatalog.Entries.First(m => m.ManeuverId == WerewolfCombatIdentifiers.SavageLeap);

        Assert.Equal(WerewolfAbilityIdentifiers.Athletics, savageLeap.AttackAbility);
        Assert.Equal(8, savageLeap.BaseDifficulty);
        Assert.Equal(2, savageLeap.ActionCost);
        Assert.True(savageLeap.RequiresPrerequisiteSuccess);
        Assert.True(savageLeap.IsSpecial);
        Assert.Equal(WerewolfCombatIdentifiers.Tackle, savageLeap.PrerequisiteManeuverId);
    }

    [Fact]
    public void ManeuverCatalogTauntHasCorrectProperties()
    {
        var taunt = WerewolfCombatManeuverCatalog.Entries.First(m => m.ManeuverId == WerewolfCombatIdentifiers.Taunt);

        Assert.Equal(WerewolfAbilityIdentifiers.Expression, taunt.AttackAbility);
        Assert.Equal(0, taunt.BaseDifficulty);
        Assert.True(taunt.IsSpecial);
        Assert.False(taunt.RequiresPrerequisiteSuccess);
    }
}
