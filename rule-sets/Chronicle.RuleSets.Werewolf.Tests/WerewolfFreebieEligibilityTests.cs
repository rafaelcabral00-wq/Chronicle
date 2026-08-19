using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfFreebieEligibilityTests
{
    [Theory]
    [InlineData(WerewolfFreebieCategory.Attribute, "character.attribute.strength", 1, 1, true)]
    [InlineData(WerewolfFreebieCategory.Attribute, "character.attribute.strength", 5, 1, false)]
    [InlineData(WerewolfFreebieCategory.Ability, "character.ability.crafts", 0, 1, true)]
    [InlineData(WerewolfFreebieCategory.Ability, "character.ability.crafts", 3, 1, true)]
    [InlineData(WerewolfFreebieCategory.Ability, "character.ability.crafts", 3, 2, true)]
    [InlineData(WerewolfFreebieCategory.Ability, "character.ability.crafts", 4, 1, true)]
    [InlineData(WerewolfFreebieCategory.Background, "character.background.allies", 0, 1, true)]
    [InlineData(WerewolfFreebieCategory.Background, "character.background.allies", 5, 1, false)]
    [InlineData(WerewolfFreebieCategory.Gift, "gift.race.homid.master-of-fire", 0, 1, true)]
    [InlineData(WerewolfFreebieCategory.Gift, "gift.race.homid.master-of-fire", 1, 1, false)]
    [InlineData(WerewolfFreebieCategory.Rage, "character.resource.rage", 1, 1, true)]
    [InlineData(WerewolfFreebieCategory.Gnosis, "character.resource.gnosis", 1, 1, true)]
    [InlineData(WerewolfFreebieCategory.Willpower, "character.resource.willpower", 3, 1, true)]
    public void EligiblePurchaseSucceeds(WerewolfFreebieCategory category, string itemId, int current, int increase, bool expectedEligible)
    {
        var request = new WerewolfFreebieEligibilityRequest(
            "req-1",
            category,
            itemId,
            current,
            increase,
            15);

        var result = WerewolfFreebieEligibilityService.CheckEligibility(request);

        Assert.Equal(expectedEligible, result.IsEligible);
        if (expectedEligible)
        {
            Assert.Equal(15 - (GetCost(category) * increase), result.RemainingBudgetAfterPurchase);
        }
    }

    [Theory]
    [InlineData(WerewolfFreebieCategory.Attribute, "character.attribute.strength", 1, 2, 10)]
    [InlineData(WerewolfFreebieCategory.Ability, "character.ability.crafts", 0, 3, 6)]
    [InlineData(WerewolfFreebieCategory.Background, "character.background.allies", 0, 5, 5)]
    [InlineData(WerewolfFreebieCategory.Gift, "gift.race.homid.master-of-fire", 0, 1, 7)]
    [InlineData(WerewolfFreebieCategory.Rage, "character.resource.rage", 1, 2, 2)]
    [InlineData(WerewolfFreebieCategory.Gnosis, "character.resource.gnosis", 1, 1, 2)]
    [InlineData(WerewolfFreebieCategory.Willpower, "character.resource.willpower", 3, 1, 1)]
    public void CostCalculationIsCorrect(WerewolfFreebieCategory category, string itemId, int current, int increase, int expectedCost)
    {
        var request = new WerewolfFreebieEligibilityRequest(
            "req-1",
            category,
            itemId,
            current,
            increase,
            15);

        var result = WerewolfFreebieEligibilityService.CheckEligibility(request);

        Assert.Equal(expectedCost, result.CalculatedCost);
    }

    [Fact]
    public void RejectsInsufficientBudget()
    {
        var request = new WerewolfFreebieEligibilityRequest(
            "req-1",
            WerewolfFreebieCategory.Attribute,
            "character.attribute.strength",
            1,
            1,
            4);

        var result = WerewolfFreebieEligibilityService.CheckEligibility(request);

        Assert.False(result.IsEligible);
        Assert.Contains(result.Findings, f => f.Code == "InsufficientBudget");
    }

    [Fact]
    public void RejectsNegativeBudget()
    {
        var request = new WerewolfFreebieEligibilityRequest(
            "req-1",
            WerewolfFreebieCategory.Ability,
            "character.ability.crafts",
            0,
            1,
            -1);

        var result = WerewolfFreebieEligibilityService.CheckEligibility(request);

        Assert.False(result.IsEligible);
        Assert.Contains(result.Findings, f => f.Code == "InvalidBudget");
    }

    [Fact]
    public void RejectsZeroIncrease()
    {
        var request = new WerewolfFreebieEligibilityRequest(
            "req-1",
            WerewolfFreebieCategory.Rage,
            "character.resource.rage",
            1,
            0,
            15);

        var result = WerewolfFreebieEligibilityService.CheckEligibility(request);

        Assert.False(result.IsEligible);
        Assert.Contains(result.Findings, f => f.Code == "InvalidIncrease");
    }

    [Fact]
    public void RejectsGiftAtMaximum()
    {
        var request = new WerewolfFreebieEligibilityRequest(
            "req-1",
            WerewolfFreebieCategory.Gift,
            "gift.race.homid.master-of-fire",
            1,
            1,
            15);

        var result = WerewolfFreebieEligibilityService.CheckEligibility(request);

        Assert.False(result.IsEligible);
        Assert.Contains(result.Findings, f => f.Code == "MaximumExceeded");
    }

    [Fact]
    public void RejectsAttributeAboveMaximum()
    {
        var request = new WerewolfFreebieEligibilityRequest(
            "req-1",
            WerewolfFreebieCategory.Attribute,
            "character.attribute.strength",
            5,
            1,
            15);

        var result = WerewolfFreebieEligibilityService.CheckEligibility(request);

        Assert.False(result.IsEligible);
        Assert.Contains(result.Findings, f => f.Code == "MaximumExceeded");
    }

    [Fact]
    public void RecordsInformationWhenAbilityExceedsBaseLimit()
    {
        var request = new WerewolfFreebieEligibilityRequest(
            "req-1",
            WerewolfFreebieCategory.Ability,
            "character.ability.crafts",
            3,
            1,
            15);

        var result = WerewolfFreebieEligibilityService.CheckEligibility(request);

        Assert.True(result.IsEligible);
        Assert.Contains(result.Findings, f => f.Code == "BonusPointAboveBaseLimit");
    }

    [Theory]
    [InlineData(WerewolfFreebieCategory.Attribute, 5)]
    [InlineData(WerewolfFreebieCategory.Ability, 2)]
    [InlineData(WerewolfFreebieCategory.Background, 1)]
    [InlineData(WerewolfFreebieCategory.Gift, 7)]
    [InlineData(WerewolfFreebieCategory.Rage, 1)]
    [InlineData(WerewolfFreebieCategory.Gnosis, 2)]
    [InlineData(WerewolfFreebieCategory.Willpower, 1)]
    public void CostCatalogMatchesSourceTable(WerewolfFreebieCategory category, int expectedCost)
    {
        var entry = WerewolfFreebieCostCatalog.GetEntry(category.ToString().ToLowerInvariant());
        Assert.NotNull(entry);
        Assert.Equal(expectedCost, entry.CostPerUnit);
    }

    private static int GetCost(WerewolfFreebieCategory category)
    {
        return category switch
        {
            WerewolfFreebieCategory.Attribute => 5,
            WerewolfFreebieCategory.Ability => 2,
            WerewolfFreebieCategory.Background => 1,
            WerewolfFreebieCategory.Gift => 7,
            WerewolfFreebieCategory.Rage => 1,
            WerewolfFreebieCategory.Gnosis => 2,
            WerewolfFreebieCategory.Willpower => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        };
    }
}
