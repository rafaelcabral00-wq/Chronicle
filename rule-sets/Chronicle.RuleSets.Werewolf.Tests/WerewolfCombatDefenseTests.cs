using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfCombatDefenseTests
{
    [Fact]
    public void CalculateDefenseDodgeReturnsCorrectPool()
    {
        var result = WerewolfCombatDefenseService.CalculateDefense(new WerewolfCombatDefenseRequest(
            "req", State(), 1, "dodge", 3, null, null, 4, null));

        Assert.True(result.Succeeded);
        Assert.Equal("dodge", result.DefenseType);
        Assert.Equal(7, result.DefensePool);
        Assert.Equal(6, result.DefenseDifficulty);
        Assert.Equal(2, result.UpdatedState!.RuntimeStateVersion);
    }

    [Fact]
    public void CalculateDefenseBlockReturnsCorrectPool()
    {
        var result = WerewolfCombatDefenseService.CalculateDefense(new WerewolfCombatDefenseRequest(
            "req", State(), 1, "block", null, 2, null, 3, null));

        Assert.True(result.Succeeded);
        Assert.Equal("block", result.DefenseType);
        Assert.Equal(5, result.DefensePool);
        Assert.Single(result.Findings);
        Assert.Contains("Ineffective against firearms", result.Findings[0]);
    }

    [Fact]
    public void CalculateDefenseParryReturnsCorrectPool()
    {
        var result = WerewolfCombatDefenseService.CalculateDefense(new WerewolfCombatDefenseRequest(
            "req", State(), 1, "parry", null, null, 2, 3, null));

        Assert.True(result.Succeeded);
        Assert.Equal("parry", result.DefenseType);
        Assert.Equal(5, result.DefensePool);
    }

    [Fact]
    public void CalculateDefenseUnknownTypeReturnsFailure()
    {
        var result = WerewolfCombatDefenseService.CalculateDefense(new WerewolfCombatDefenseRequest(
            "req", State(), 1, "unknown", 3, null, null, 4, null));

        Assert.False(result.Succeeded);
        Assert.Equal("unknown", result.DefenseType);
    }

    [Fact]
    public void CalculateDefenseMissingDodgeAbilitiesReturnsFailure()
    {
        var result = WerewolfCombatDefenseService.CalculateDefense(new WerewolfCombatDefenseRequest(
            "req", State(), 1, "dodge", null, null, null, 4, null));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void CalculateDefenseNegativePoolClampsToZero()
    {
        var result = WerewolfCombatDefenseService.CalculateDefense(new WerewolfCombatDefenseRequest(
            "req", State(), 1, "dodge", -1, null, null, -1, null));

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.DefensePool);
    }

    [Fact]
    public void CalculateDefenseVersionMismatchReturnsFailure()
    {
        var result = WerewolfCombatDefenseService.CalculateDefense(new WerewolfCombatDefenseRequest(
            "req", State(), 2, "dodge", 3, null, null, 4, null));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void ComputeDefensePoolBlockAgainstFirearmReturnsZero()
    {
        var attributes = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [WerewolfAttributeIdentifiers.Dexterity] = 3,
            [WerewolfAbilityIdentifiers.Brawl] = 2
        };

        var pool = WerewolfCombatDefenseService.ComputeDefensePool(attributes, WerewolfCombatIdentifiers.Firearm, "block");

        Assert.Equal(0, pool);
    }

    [Fact]
    public void ComputeDefensePoolDodgeAgainstFirearmReturnsDexterityPlusDodge()
    {
        var attributes = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [WerewolfAttributeIdentifiers.Dexterity] = 3,
            [WerewolfAbilityIdentifiers.Dodge] = 2
        };

        var pool = WerewolfCombatDefenseService.ComputeDefensePool(attributes, WerewolfCombatIdentifiers.Firearm, "dodge");

        Assert.Equal(5, pool);
    }

    [Fact]
    public void ComputeDefensePoolParryAgainstFirearmReturnsDexterityPlusMelee()
    {
        var attributes = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [WerewolfAttributeIdentifiers.Dexterity] = 3,
            [WerewolfAbilityIdentifiers.Melee] = 2
        };

        var pool = WerewolfCombatDefenseService.ComputeDefensePool(attributes, WerewolfCombatIdentifiers.Firearm, "parry");

        Assert.Equal(5, pool);
    }

    [Fact]
    public void ComputeDefensePoolDodgeAgainstMeleeReturnsDexterityPlusDodge()
    {
        var attributes = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [WerewolfAttributeIdentifiers.Dexterity] = 3,
            [WerewolfAbilityIdentifiers.Dodge] = 2
        };

        var pool = WerewolfCombatDefenseService.ComputeDefensePool(attributes, WerewolfCombatIdentifiers.Brawl, "dodge");

        Assert.Equal(5, pool);
    }

    [Fact]
    public void ComputeDefensePoolBlockAgainstNaturalAttackReturnsDexterityPlusBrawl()
    {
        var attributes = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [WerewolfAttributeIdentifiers.Dexterity] = 3,
            [WerewolfAbilityIdentifiers.Brawl] = 2
        };

        var pool = WerewolfCombatDefenseService.ComputeDefensePool(attributes, WerewolfCombatIdentifiers.Bite, "block");

        Assert.Equal(5, pool);
    }

    [Fact]
    public void ComputeDefensePoolParryAgainstNaturalAttackReturnsDexterityPlusMelee()
    {
        var attributes = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [WerewolfAttributeIdentifiers.Dexterity] = 3,
            [WerewolfAbilityIdentifiers.Melee] = 2
        };

        var pool = WerewolfCombatDefenseService.ComputeDefensePool(attributes, WerewolfCombatIdentifiers.Claw, "parry");

        Assert.Equal(5, pool);
    }

    [Fact]
    public void ResolveDefenseDodgeReturnsCorrectDefinition()
    {
        var defense = WerewolfCombatDefenseService.ResolveDefense("dodge");

        Assert.Equal("dodge", defense.DefenseId);
        Assert.Equal(WerewolfAttributeIdentifiers.Dexterity, defense.AttributeId);
        Assert.Equal(WerewolfAbilityIdentifiers.Dodge, defense.AbilityId);
        Assert.Equal(6, defense.BaseDifficulty);
    }

    [Fact]
    public void ResolveDefenseBlockReturnsCorrectDefinition()
    {
        var defense = WerewolfCombatDefenseService.ResolveDefense("block");

        Assert.Equal("block", defense.DefenseId);
        Assert.Equal(WerewolfAbilityIdentifiers.Brawl, defense.AbilityId);
    }

    [Fact]
    public void ResolveDefenseParryReturnsCorrectDefinition()
    {
        var defense = WerewolfCombatDefenseService.ResolveDefense("parry");

        Assert.Equal("parry", defense.DefenseId);
        Assert.Equal(WerewolfAbilityIdentifiers.Melee, defense.AbilityId);
    }

    [Fact]
    public void ResolveDefenseUnknownThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => WerewolfCombatDefenseService.ResolveDefense("unknown"));
    }

    private static WerewolfRuntimeCharacterState State(int version = 1, string currentForm = "character.form.homid", string birthRace = "homid")
    {
        return new WerewolfRuntimeCharacterState(
            "test", "1.0", "draft", version, new Dictionary<string, string>(),
            5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0,
            birthRace, null, currentForm);
    }
}
