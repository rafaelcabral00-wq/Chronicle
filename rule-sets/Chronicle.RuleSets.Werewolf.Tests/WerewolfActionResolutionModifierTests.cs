using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfActionResolutionModifierTests
{
    [Fact]
    public void NoDeformityReturnsZeroModifiers()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Perception,
            WerewolfAbilityIdentifiers.Alertness,
            WerewolfFormIdentifiers.Homid,
            null,
            false,
            false,
            false,
            null,
            false,
            false,
            false,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Equal(0, result.DicePoolModifier);
        Assert.Equal(0, result.DifficultyModifier);
        Assert.False(result.IsAutomaticFailure);
        Assert.Empty(result.ConditionalTests);
    }

    [Fact]
    public void AlbinismAddsDaylightPerceptionDifficulty()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Perception,
            WerewolfAbilityIdentifiers.Alertness,
            WerewolfFormIdentifiers.Homid,
            WerewolfMetisDeformityIdentifiers.Albinism,
            IsDaylightWithoutProtection: true,
            false,
            false,
            null,
            false,
            false,
            false,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Equal(2, result.DifficultyModifier);
        Assert.Equal(0, result.DicePoolModifier);
    }

    [Fact]
    public void AlbinismDoesNotApplyWithoutDaylight()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Perception,
            WerewolfAbilityIdentifiers.Alertness,
            WerewolfFormIdentifiers.Homid,
            WerewolfMetisDeformityIdentifiers.Albinism,
            IsDaylightWithoutProtection: false,
            false,
            false,
            null,
            false,
            false,
            false,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Equal(0, result.DifficultyModifier);
    }

    [Fact]
    public void AlbinismDoesNotAffectNonPerceptionTests()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Strength,
            WerewolfAbilityIdentifiers.Brawl,
            WerewolfFormIdentifiers.Homid,
            WerewolfMetisDeformityIdentifiers.Albinism,
            IsDaylightWithoutProtection: true,
            false,
            false,
            null,
            false,
            false,
            false,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Equal(0, result.DifficultyModifier);
    }

    [Fact]
    public void BlindCausesAutomaticFailureOnVisionTests()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Perception,
            WerewolfAbilityIdentifiers.Alertness,
            WerewolfFormIdentifiers.Homid,
            WerewolfMetisDeformityIdentifiers.Blind,
            false,
            false,
            false,
            null,
            false,
            IsVisionBased: true,
            false,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.True(result.IsAutomaticFailure);
    }

    [Fact]
    public void BlindDoesNotAffectNonVisionTests()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Perception,
            WerewolfAbilityIdentifiers.Alertness,
            WerewolfFormIdentifiers.Homid,
            WerewolfMetisDeformityIdentifiers.Blind,
            false,
            false,
            false,
            "olfactory",
            false,
            IsVisionBased: false,
            false,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.False(result.IsAutomaticFailure);
    }

    [Fact]
    public void DebilitatingDiseaseAddsStaminaDifficulty()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Stamina,
            WerewolfAbilityIdentifiers.Survival,
            WerewolfFormIdentifiers.Homid,
            WerewolfMetisDeformityIdentifiers.DebilitatingDisease,
            false,
            false,
            false,
            null,
            false,
            false,
            false,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Equal(2, result.DifficultyModifier);
    }

    [Fact]
    public void DebilitatingDiseaseDoesNotAffectNonStaminaTests()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Strength,
            WerewolfAbilityIdentifiers.Brawl,
            WerewolfFormIdentifiers.Homid,
            WerewolfMetisDeformityIdentifiers.DebilitatingDisease,
            false,
            false,
            false,
            null,
            false,
            false,
            false,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Equal(0, result.DifficultyModifier);
    }

    [Fact]
    public void WitheredLimbAddsDexterityDifficultyWhenUsingLimb()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Dexterity,
            WerewolfAbilityIdentifiers.Brawl,
            WerewolfFormIdentifiers.Homid,
            WerewolfMetisDeformityIdentifiers.WitheredLimb,
            false,
            false,
            IsUsingWitheredLimb: true,
            null,
            false,
            false,
            false,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Equal(2, result.DifficultyModifier);
    }

    [Fact]
    public void WitheredLimbDoesNotApplyWhenNotUsingLimb()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Dexterity,
            WerewolfAbilityIdentifiers.Brawl,
            WerewolfFormIdentifiers.Homid,
            WerewolfMetisDeformityIdentifiers.WitheredLimb,
            false,
            false,
            IsUsingWitheredLimb: false,
            null,
            false,
            false,
            false,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Equal(0, result.DifficultyModifier);
    }

    [Fact]
    public void TaillessAddsSocialDifficultyInAllForms()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Charisma,
            WerewolfAbilityIdentifiers.Expression,
            WerewolfFormIdentifiers.Homid,
            WerewolfMetisDeformityIdentifiers.Tailless,
            false,
            false,
            false,
            null,
            false,
            false,
            false,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Equal(1, result.DifficultyModifier);
    }

    [Fact]
    public void TaillessAddsBalanceDifficultyInLupus()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Dexterity,
            WerewolfAbilityIdentifiers.Athletics,
            WerewolfFormIdentifiers.Lupus,
            WerewolfMetisDeformityIdentifiers.Tailless,
            false,
            false,
            false,
            null,
            false,
            false,
            IsBalanceTest: true,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Equal(1, result.DifficultyModifier);
    }

    [Fact]
    public void TaillessAddsBalanceDifficultyInHispo()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Dexterity,
            WerewolfAbilityIdentifiers.Athletics,
            WerewolfFormIdentifiers.Hispo,
            WerewolfMetisDeformityIdentifiers.Tailless,
            false,
            false,
            false,
            null,
            false,
            false,
            IsBalanceTest: true,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Equal(1, result.DifficultyModifier);
    }

    [Fact]
    public void TaillessAddsBalanceDifficultyInCrinos()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Dexterity,
            WerewolfAbilityIdentifiers.Athletics,
            WerewolfFormIdentifiers.Crinos,
            WerewolfMetisDeformityIdentifiers.Tailless,
            false,
            false,
            false,
            null,
            false,
            false,
            IsBalanceTest: true,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Equal(1, result.DifficultyModifier);
    }

    [Fact]
    public void TaillessDoesNotAddBalanceDifficultyInGlabro()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Dexterity,
            WerewolfAbilityIdentifiers.Athletics,
            WerewolfFormIdentifiers.Glabro,
            WerewolfMetisDeformityIdentifiers.Tailless,
            false,
            false,
            false,
            null,
            false,
            false,
            IsBalanceTest: true,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Equal(0, result.DifficultyModifier);
    }

    [Fact]
    public void NoSenseOfSmellCausesAutomaticFailureOnSmellPerception()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Perception,
            WerewolfAbilityIdentifiers.Alertness,
            WerewolfFormIdentifiers.Lupus,
            WerewolfMetisDeformityIdentifiers.NoSenseOfSmell,
            false,
            false,
            false,
            "olfactory",
            false,
            false,
            false,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.True(result.IsAutomaticFailure);
    }

    [Fact]
    public void NoSenseOfSmellAddsTrackingPenalty()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Perception,
            WerewolfAbilityIdentifiers.PrimalInstinct,
            WerewolfFormIdentifiers.Lupus,
            WerewolfMetisDeformityIdentifiers.NoSenseOfSmell,
            false,
            false,
            false,
            null,
            IsTracking: true,
            false,
            false,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Equal(2, result.DifficultyModifier);
    }

    [Fact]
    public void NoSenseOfSmellDoesNotAffectNonTrackingTests()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Perception,
            WerewolfAbilityIdentifiers.Alertness,
            WerewolfFormIdentifiers.Lupus,
            WerewolfMetisDeformityIdentifiers.NoSenseOfSmell,
            false,
            false,
            false,
            "visual",
            IsTracking: false,
            IsVisionBased: true,
            false,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Equal(0, result.DifficultyModifier);
        Assert.False(result.IsAutomaticFailure);
    }

    [Fact]
    public void FitsOfMadnessRequiresConditionalTestUnderTension()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Wits,
            WerewolfAbilityIdentifiers.Survival,
            WerewolfFormIdentifiers.Homid,
            WerewolfMetisDeformityIdentifiers.FitsOfMadness,
            false,
            IsUnderTension: true,
            false,
            null,
            false,
            false,
            false,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Single(result.ConditionalTests);
        var test = result.ConditionalTests[0];
        Assert.Equal("under-tension", test.Condition);
        Assert.Equal(8, test.TestDifficulty);
        Assert.Equal(3, test.MinimumSuccesses);
        Assert.Equal("temporary-psychotic-episode", test.Consequence);
    }

    [Fact]
    public void FitsOfMadnessDoesNotRequireTestWithoutTension()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Wits,
            WerewolfAbilityIdentifiers.Survival,
            WerewolfFormIdentifiers.Homid,
            WerewolfMetisDeformityIdentifiers.FitsOfMadness,
            false,
            IsUnderTension: false,
            false,
            null,
            false,
            false,
            false,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Empty(result.ConditionalTests);
    }

    [Fact]
    public void SeizuresConditionalTestNotAddedWithoutCriticalFailure()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Wits,
            WerewolfAbilityIdentifiers.Survival,
            WerewolfFormIdentifiers.Homid,
            WerewolfMetisDeformityIdentifiers.Seizures,
            false,
            false,
            false,
            null,
            false,
            false,
            false,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Empty(result.ConditionalTests);
    }

    [Fact]
    public void DiceModifiersDoNotAffectDifficulty()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Stamina,
            WerewolfAbilityIdentifiers.Survival,
            WerewolfFormIdentifiers.Homid,
            WerewolfMetisDeformityIdentifiers.ToughHide,
            false,
            false,
            false,
            null,
            false,
            false,
            false,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Equal(1, result.DicePoolModifier);
        Assert.Equal(0, result.DifficultyModifier);
    }

    [Fact]
    public void DifficultyModifiersDoNotAffectDicePool()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Stamina,
            WerewolfAbilityIdentifiers.Survival,
            WerewolfFormIdentifiers.Homid,
            WerewolfMetisDeformityIdentifiers.DebilitatingDisease,
            false,
            false,
            false,
            null,
            false,
            false,
            false,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Equal(0, result.DicePoolModifier);
        Assert.Equal(2, result.DifficultyModifier);
    }

    [Fact]
    public void UnknownDeformityReturnsZeroModifiers()
    {
        var context = new WerewolfActionResolutionContext(
            WerewolfAttributeIdentifiers.Perception,
            WerewolfAbilityIdentifiers.Alertness,
            WerewolfFormIdentifiers.Homid,
            "unknown-deformity",
            false,
            false,
            false,
            null,
            false,
            false,
            false,
            []);

        var result = WerewolfActionResolutionModifierService.ComputeModifiers(context);

        Assert.Equal(0, result.DicePoolModifier);
        Assert.Equal(0, result.DifficultyModifier);
        Assert.False(result.IsAutomaticFailure);
    }
}
