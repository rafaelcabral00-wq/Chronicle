using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfCombatAttackTests
{
    [Fact]
    public void ResolveAttackBiteReturnsCorrectDefinition()
    {
        var definition = WerewolfCombatAttackDefinitionService.ResolveAttack(WerewolfCombatIdentifiers.Bite);

        Assert.Equal(WerewolfCombatIdentifiers.Bite, definition.AttackId);
        Assert.Equal(WerewolfAttributeIdentifiers.Dexterity, definition.AttributeId);
        Assert.Equal(WerewolfAbilityIdentifiers.Brawl, definition.AbilityId);
        Assert.Equal(5, definition.BaseDifficulty);
        Assert.Equal("Strength + 1", definition.DamageExpression);
        Assert.Equal(WerewolfDamageCategory.Aggravated.ToString(), definition.DamageCategory);
        Assert.True(definition.IsNaturalWeapon);
        Assert.Equal("bite", definition.NaturalWeaponTarget);
    }

    [Fact]
    public void ResolveAttackClawReturnsCorrectDefinition()
    {
        var definition = WerewolfCombatAttackDefinitionService.ResolveAttack(WerewolfCombatIdentifiers.Claw);

        Assert.Equal(WerewolfCombatIdentifiers.Claw, definition.AttackId);
        Assert.Equal("Strength + 1", definition.DamageExpression);
        Assert.Equal(WerewolfDamageCategory.Aggravated.ToString(), definition.DamageCategory);
        Assert.True(definition.IsNaturalWeapon);
        Assert.Equal("claw", definition.NaturalWeaponTarget);
    }

    [Fact]
    public void ResolveAttackFirearmReturnsCorrectDefinition()
    {
        var definition = WerewolfCombatAttackDefinitionService.ResolveAttack(WerewolfCombatIdentifiers.Firearm);

        Assert.Equal(WerewolfCombatIdentifiers.Firearm, definition.AttackId);
        Assert.Equal(WerewolfAttributeIdentifiers.Dexterity, definition.AttributeId);
        Assert.Equal(WerewolfAbilityIdentifiers.Firearms, definition.AbilityId);
        Assert.Equal(6, definition.BaseDifficulty);
        Assert.Equal("Variable", definition.DamageExpression);
        Assert.Equal(WerewolfDamageCategory.Lethal.ToString(), definition.DamageCategory);
        Assert.False(definition.IsNaturalWeapon);
    }

    [Fact]
    public void ResolveAttackThrownReturnsCorrectDefinition()
    {
        var definition = WerewolfCombatAttackDefinitionService.ResolveAttack(WerewolfCombatIdentifiers.Thrown);

        Assert.Equal(WerewolfCombatIdentifiers.Thrown, definition.AttackId);
        Assert.Equal(WerewolfAttributeIdentifiers.Dexterity, definition.AttributeId);
        Assert.Equal(WerewolfAbilityIdentifiers.Athletics, definition.AbilityId);
        Assert.Equal("Variable", definition.DamageExpression);
    }

    [Fact]
    public void ResolveAttackMeleeWeaponReturnsCorrectDefinition()
    {
        var definition = WerewolfCombatAttackDefinitionService.ResolveAttack(WerewolfCombatIdentifiers.MeleeWeapon);

        Assert.Equal(WerewolfCombatIdentifiers.MeleeWeapon, definition.AttackId);
        Assert.Equal(WerewolfAttributeIdentifiers.Dexterity, definition.AttributeId);
        Assert.Equal(WerewolfAbilityIdentifiers.Melee, definition.AbilityId);
        Assert.Equal("Variable", definition.DamageExpression);
        Assert.True(definition.RequiresForm);
    }

    [Fact]
    public void ResolveAttackBrawlReturnsCorrectDefinition()
    {
        var definition = WerewolfCombatAttackDefinitionService.ResolveAttack(WerewolfCombatIdentifiers.Brawl);

        Assert.Equal(WerewolfCombatIdentifiers.Brawl, definition.AttackId);
        Assert.Equal("Strength", definition.DamageExpression);
        Assert.Equal(WerewolfDamageCategory.Bashing.ToString(), definition.DamageCategory);
        Assert.False(definition.IsNaturalWeapon);
    }

    [Fact]
    public void ResolveAttackBowReturnsCorrectDefinition()
    {
        var definition = WerewolfCombatAttackCatalog.Entries.First(a => a.AttackId == WerewolfCombatIdentifiers.Bow);

        Assert.Equal(WerewolfCombatIdentifiers.Bow, definition.AttackId);
        Assert.Equal(WerewolfAttributeIdentifiers.Dexterity, definition.AttributeId);
        Assert.Equal(WerewolfAbilityIdentifiers.Athletics, definition.AbilityId);
        Assert.Equal("Variable", definition.DamageExpression);
        Assert.Equal(WerewolfDamageCategory.Lethal.ToString(), definition.DamageCategory);
    }

    [Fact]
    public void ResolveAttackUnknownThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => WerewolfCombatAttackDefinitionService.ResolveAttack("unknown"));
    }

    [Fact]
    public void ResolveAttackEmptyThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => WerewolfCombatAttackDefinitionService.ResolveAttack(""));
    }

    [Fact]
    public void AttackCatalogContainsExactlySevenEntries()
    {
        Assert.Equal(7, WerewolfCombatAttackCatalog.Entries.Count);
    }

    [Fact]
    public void AttackCatalogAllEntriesHaveUniqueKeys()
    {
        var keys = WerewolfCombatAttackCatalog.Entries.Select(e => e.AttackId).ToList();
        Assert.Equal(7, keys.Distinct(StringComparer.Ordinal).Count());
    }

    [Theory]
    [InlineData(WerewolfCombatIdentifiers.Bite, WerewolfFormIdentifiers.Crinos, true)]
    [InlineData(WerewolfCombatIdentifiers.Bite, WerewolfFormIdentifiers.Lupus, true)]
    [InlineData(WerewolfCombatIdentifiers.Bite, WerewolfFormIdentifiers.Homid, false)]
    [InlineData(WerewolfCombatIdentifiers.Claw, WerewolfFormIdentifiers.Crinos, true)]
    [InlineData(WerewolfCombatIdentifiers.Claw, WerewolfFormIdentifiers.Hispo, true)]
    [InlineData(WerewolfCombatIdentifiers.Claw, WerewolfFormIdentifiers.Lupus, false)]
    [InlineData(WerewolfCombatIdentifiers.Firearm, WerewolfFormIdentifiers.Homid, true)]
    [InlineData(WerewolfCombatIdentifiers.Firearm, WerewolfFormIdentifiers.Lupus, false)]
    public void AttackCatalogFormRestrictionsAreEnforced(string attackId, string formId, bool expectedAllowed)
    {
        var attack = WerewolfCombatAttackCatalog.Entries.First(a => a.AttackId == attackId);
        var allowed = attack.AllowedForms.Contains(formId, StringComparer.Ordinal);

        Assert.Equal(expectedAllowed, allowed);
    }
}
