using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfCombatMetisDeformityTests
{
    [Fact]
    public void HornsCombatDamageEffectIsDeclared()
    {
        var effects = WerewolfMetisDeformityIdentifiers.Effects[WerewolfMetisDeformityIdentifiers.Horns];

        Assert.Contains(effects, e => e.Kind == WerewolfMetisDeformityEffectKind.CombatDamage);
    }

    [Fact]
    public void ToughHideDiceBonusEffectIsDeclared()
    {
        var effects = WerewolfMetisDeformityIdentifiers.Effects[WerewolfMetisDeformityIdentifiers.ToughHide];

        Assert.Contains(effects, e => e.Kind == WerewolfMetisDeformityEffectKind.DiceBonus);
    }

    [Fact]
    public void HornsCombatDamageEffectHasCorrectTargetAndValue()
    {
        var effects = WerewolfMetisDeformityIdentifiers.Effects[WerewolfMetisDeformityIdentifiers.Horns];
        var combatDamage = effects.First(e => e.Kind == WerewolfMetisDeformityEffectKind.CombatDamage);

        Assert.Equal("Strength", combatDamage.Target);
        Assert.Equal(1, combatDamage.Value);
        Assert.Equal("bashing", combatDamage.Notes);
    }

    [Fact]
    public void ToughHideDiceBonusEffectHasCorrectTargetAndValue()
    {
        var effects = WerewolfMetisDeformityIdentifiers.Effects[WerewolfMetisDeformityIdentifiers.ToughHide];
        var diceBonus = effects.First(e => e.Kind == WerewolfMetisDeformityEffectKind.DiceBonus);

        Assert.Equal("Absorption", diceBonus.Target);
        Assert.Equal(1, diceBonus.Value);
    }

    [Fact]
    public void HornsDoesNotHaveToughHideEffect()
    {
        var effects = WerewolfMetisDeformityIdentifiers.Effects[WerewolfMetisDeformityIdentifiers.Horns];

        Assert.DoesNotContain(effects, e => e.Kind == WerewolfMetisDeformityEffectKind.ToughHide);
    }

    [Fact]
    public void ToughHideDoesNotHaveCombatDamageEffect()
    {
        var effects = WerewolfMetisDeformityIdentifiers.Effects[WerewolfMetisDeformityIdentifiers.ToughHide];

        Assert.DoesNotContain(effects, e => e.Kind == WerewolfMetisDeformityEffectKind.CombatDamage);
    }

    [Fact]
    public void AllDeformitiesEffectsAreNonNull()
    {
        foreach (var deformityId in WerewolfMetisDeformityIdentifiers.Supported)
        {
            var effects = WerewolfMetisDeformityIdentifiers.Effects[deformityId];
            Assert.NotNull(effects);
            Assert.NotEmpty(effects);
        }
    }
}

