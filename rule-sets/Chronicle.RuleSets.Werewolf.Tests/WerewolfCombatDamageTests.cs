using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfCombatDamageTests
{
    [Fact]
    public void DefineDamageRollOneSuccessReturnsPoolSizeOne()
    {
        var request = new WerewolfCombatDamageRequest(
            "req", State(), 1, 1, "Strength + 1", WerewolfDamageCategory.Bashing.ToString(), null);

        var definition = WerewolfCombatDamageService.DefineDamageRoll(request);

        Assert.True(definition.Findings.Count > 0);
        Assert.Equal(1, definition.DamagePoolSize);
        Assert.Equal(6, definition.Difficulty);
        Assert.Equal(WerewolfDamageCategory.Bashing.ToString(), definition.DamageCategory);
    }

    [Fact]
    public void DefineDamageRollThreeSuccessesReturnsPoolSizeThree()
    {
        var request = new WerewolfCombatDamageRequest(
            "req", State(), 1, 3, "Strength + 1", WerewolfDamageCategory.Lethal.ToString(), null);

        var definition = WerewolfCombatDamageService.DefineDamageRoll(request);

        Assert.Equal(3, definition.DamagePoolSize);
    }

    [Fact]
    public void DefineDamageRollZeroSuccessesReturnsPoolSizeOne()
    {
        var request = new WerewolfCombatDamageRequest(
            "req", State(), 1, 0, "Strength + 1", WerewolfDamageCategory.Bashing.ToString(), null);

        var definition = WerewolfCombatDamageService.DefineDamageRoll(request);

        Assert.Equal(1, definition.DamagePoolSize);
    }

    [Fact]
    public void DefineDamageRollStrengthBonusAddsToPool()
    {
        var request = new WerewolfCombatDamageRequest(
            "req", State(), 1, 2, "Strength", WerewolfDamageCategory.Aggravated.ToString(), 2);

        var definition = WerewolfCombatDamageService.DefineDamageRoll(request);

        Assert.Equal(3, definition.DamagePoolSize);
    }

    [Fact]
    public void DefineDamageRollNegativeSuccessesReturnsFailure()
    {
        var request = new WerewolfCombatDamageRequest(
            "req", State(), 1, -1, "Strength", WerewolfDamageCategory.Bashing.ToString(), null);

        var definition = WerewolfCombatDamageService.DefineDamageRoll(request);

        Assert.Equal(0, definition.DamagePoolSize);
        Assert.Contains(definition.Findings, f => f.Contains("Attack successes cannot be negative"));
    }

    [Fact]
    public void DefineDamageRollInvalidCategoryReturnsFailure()
    {
        var request = new WerewolfCombatDamageRequest(
            "req", State(), 1, 1, "Strength", "invalid", null);

        var definition = WerewolfCombatDamageService.DefineDamageRoll(request);

        Assert.Equal(0, definition.DamagePoolSize);
        Assert.Contains(definition.Findings, f => f.Contains("Invalid damage category"));
    }

    [Fact]
    public void DefineDamageRollVersionMismatchReturnsFailure()
    {
        var request = new WerewolfCombatDamageRequest(
            "req", State(), 2, 1, "Strength", WerewolfDamageCategory.Bashing.ToString(), null);

        var definition = WerewolfCombatDamageService.DefineDamageRoll(request);

        Assert.Equal(0, definition.DamagePoolSize);
        Assert.Contains(definition.Findings, f => f.Contains("Version mismatch"));
    }

    [Fact]
    public void InterpretDamageRollAllSuccessesReturnsCorrectDamage()
    {
        var definition = new WerewolfCombatDamageRollDefinition(
            "req", 3, 6, WerewolfDamageCategory.Bashing.ToString(), []);

        var result = WerewolfCombatDamageService.InterpretDamageRoll(definition, [8, 9, 10]);

        Assert.Equal(3, result.DamageSuccesses);
        Assert.Equal(3, result.TotalDamage);
    }

    [Fact]
    public void InterpretDamageRollZeroSuccessesReturnsZeroDamage()
    {
        var definition = new WerewolfCombatDamageRollDefinition(
            "req", 3, 6, WerewolfDamageCategory.Bashing.ToString(), []);

        var result = WerewolfCombatDamageService.InterpretDamageRoll(definition, [2, 3, 5]);

        Assert.Equal(0, result.DamageSuccesses);
        Assert.Equal(0, result.TotalDamage);
    }

    [Fact]
    public void InterpretDamageRollDiceMismatchReturnsZeroDamage()
    {
        var definition = new WerewolfCombatDamageRollDefinition(
            "req", 3, 6, WerewolfDamageCategory.Bashing.ToString(), []);

        var result = WerewolfCombatDamageService.InterpretDamageRoll(definition, [8, 9]);

        Assert.Equal(0, result.DamageSuccesses);
        Assert.Contains(result.Findings, f => f.Contains("Dice count mismatch"));
    }

    [Fact]
    public void DefaultDifficultyReturnsSix()
    {
        Assert.Equal(6, WerewolfCombatDamageService.DefaultDifficulty);
    }

    [Fact]
    public void DefineDamageRollAggravatedCategoryPreservesCategory()
    {
        var request = new WerewolfCombatDamageRequest(
            "req", State(), 1, 2, "Strength", WerewolfDamageCategory.Aggravated.ToString(), null);

        var definition = WerewolfCombatDamageService.DefineDamageRoll(request);

        Assert.Equal(WerewolfDamageCategory.Aggravated.ToString(), definition.DamageCategory);
    }

    private static WerewolfRuntimeCharacterState State(int version = 1, string currentForm = "character.form.homid", string birthRace = "homid")
    {
        return new WerewolfRuntimeCharacterState(
            "test", "1.0", "draft", version, new Dictionary<string, string>(),
            5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0,
            birthRace, null, currentForm);
    }
}
