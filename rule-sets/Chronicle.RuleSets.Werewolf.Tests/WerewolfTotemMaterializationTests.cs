using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfTotemMaterializationTests
{
    [Fact]
    public void TotemCatalogContainsExactly19Identities()
    {
        Assert.Equal(19, WerewolfTotemIdentifiers.Supported.Count);
    }

    [Fact]
    public void TotemCatalogKeysAreUnique()
    {
        var keys = WerewolfTotemIdentifiers.Supported;
        Assert.Equal(keys.Count, keys.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void TotemCatalogAllEntriesResolvable()
    {
        foreach (var key in WerewolfTotemIdentifiers.Supported)
        {
            Assert.NotNull(WerewolfTotemCatalog.Get(key));
        }
    }

    [Fact]
    public void TotemCatalogEntriesHaveSourceLocators()
    {
        foreach (var entry in WerewolfTotemCatalog.AllDefinitions)
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.SourceLocator));
        }
    }

    [Fact]
    public void TotemCatalogEffectCountMatchesSource()
    {
        var totalEffects = 0;
        foreach (var entry in WerewolfTotemCatalog.AllDefinitions)
        {
            totalEffects += entry.Effects.Count;
        }

        Assert.Equal(95, totalEffects);
    }

    [Fact]
    public void TotemEffectKeysAreUniqueWithinTotem()
    {
        foreach (var entry in WerewolfTotemCatalog.AllDefinitions)
        {
            var payloads = entry.Effects.Select(e => e.Payload).ToList();
            Assert.Equal(payloads.Count, payloads.Distinct(StringComparer.Ordinal).Count());
        }
    }

    [Fact]
    public void TotemInitialStateMatchesSource()
    {
        Assert.Equal(8, WerewolfTotemDefinitions.InitialTotemPoints);
        Assert.Equal(1, WerewolfTotemDefinitions.DefaultBeneficiaryCount);
        Assert.Equal(4, WerewolfTotemDefinitions.AdditionalBeneficiaryCost);
        Assert.Equal(2, WerewolfTotemDefinitions.InitialCharms.Count);
        Assert.Contains("Sentido de Orientação", WerewolfTotemDefinitions.InitialCharms);
        Assert.Contains("Reformar", WerewolfTotemDefinitions.InitialCharms);
    }

    [Fact]
    public void TotemAdditionalBeneficiaryFormulaMatchesSource()
    {
        Assert.Equal(0, WerewolfTotemDefinitions.CalculateAdditionalBeneficiaries(1));
        Assert.Equal(0, WerewolfTotemDefinitions.CalculateAdditionalBeneficiaries(3));
        Assert.Equal(0, WerewolfTotemDefinitions.CalculateAdditionalBeneficiaries(4));
        Assert.Equal(1, WerewolfTotemDefinitions.CalculateAdditionalBeneficiaries(5));
        Assert.Equal(1, WerewolfTotemDefinitions.CalculateAdditionalBeneficiaries(8));
        Assert.Equal(2, WerewolfTotemDefinitions.CalculateAdditionalBeneficiaries(9));
        Assert.Equal(2, WerewolfTotemDefinitions.CalculateAdditionalBeneficiaries(11));
    }

    [Fact]
    public void TotemBeneficiaryCountFormulaMatchesSource()
    {
        Assert.Equal(1, WerewolfTotemDefinitions.CalculateBeneficiaryCount(1));
        Assert.Equal(1, WerewolfTotemDefinitions.CalculateBeneficiaryCount(3));
        Assert.Equal(1, WerewolfTotemDefinitions.CalculateBeneficiaryCount(4));
        Assert.Equal(2, WerewolfTotemDefinitions.CalculateBeneficiaryCount(5));
        Assert.Equal(2, WerewolfTotemDefinitions.CalculateBeneficiaryCount(8));
        Assert.Equal(3, WerewolfTotemDefinitions.CalculateBeneficiaryCount(9));
    }

    [Fact]
    public void TotemBackgroundIdentifierExists()
    {
        Assert.Equal("character.background.totem", WerewolfTotemDefinitions.TotemBackgroundKey);
    }

    [Fact]
    public void TotemImprovementTableContains9Entries()
    {
        Assert.Equal(9, WerewolfTotemDefinitions.ImprovementTable.Count);
    }

    [Fact]
    public void TotemImprovementTableHasCorrectCostDistribution()
    {
        var costs = WerewolfTotemDefinitions.ImprovementTable.Select(entry => entry.Cost).OrderBy(c => c).ToList();
        Assert.Equal(3, costs.Count(c => c == 1));
        Assert.Equal(3, costs.Count(c => c == 2));
        Assert.Equal(1, costs.Count(c => c == 3));
        Assert.Equal(1, costs.Count(c => c == 4));
        Assert.Equal(1, costs.Count(c => c == 5));
    }

    [Fact]
    public void TotemXpConflictA012IsPreserved()
    {
        Assert.Equal("A-012", WerewolfTotemDefinitions.A012Conflict.ConflictId);
        Assert.Equal(2, WerewolfTotemDefinitions.A012Conflict.XpCostA);
        Assert.Equal(3, WerewolfTotemDefinitions.A012Conflict.XpCostB);
        Assert.Equal("Unresolved", WerewolfTotemDefinitions.A012Conflict.Status);
    }

    [Fact]
    public void TotemCatalogBanirGiftEntryExists()
    {
        Assert.NotNull(WerewolfTotemDefinitions.BanirTotemGift);
        Assert.Equal("gift.totem.banir-totem", WerewolfTotemDefinitions.BanirTotemGift.GiftKey);
        Assert.Equal(3, WerewolfTotemDefinitions.BanirTotemGift.Level);
    }

    [Fact]
    public void TotemCatalogRitualOfTotemEntryExists()
    {
        Assert.NotNull(WerewolfTotemDefinitions.RitualOfTotem);
        Assert.Equal("rite.totem.ritual-of-totem", WerewolfTotemDefinitions.RitualOfTotem.RiteKey);
        Assert.Equal(3, WerewolfTotemDefinitions.RitualOfTotem.Level);
    }

    [Fact]
    public void TotemCatalogRitualOfContritionEntryExists()
    {
        Assert.NotNull(WerewolfTotemDefinitions.RitualOfContrition);
        Assert.Equal("rite.totem.ritual-of-contrition", WerewolfTotemDefinitions.RitualOfContrition.RiteKey);
        Assert.Equal(1, WerewolfTotemDefinitions.RitualOfContrition.Level);
    }
}
