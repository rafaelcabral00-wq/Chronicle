using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfSpiritUmbraS1Tests
{
    [Fact]
    public void SpiritCategoryCatalogContainsExactly8Categories()
    {
        Assert.Equal(8, WerewolfSpiritIdentifiers.Supported.Count);
    }

    [Fact]
    public void SpiritCategoryKeysAreUnique()
    {
        var keys = WerewolfSpiritIdentifiers.Supported;
        Assert.Equal(keys.Count, keys.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void SpiritCategoryAllEntriesResolvable()
    {
        foreach (var key in WerewolfSpiritIdentifiers.Supported)
        {
            Assert.NotNull(WerewolfSpiritCategoryCatalog.Get(key));
        }
    }

    [Fact]
    public void SpiritCategoryEntriesHaveSourceLocators()
    {
        foreach (var entry in WerewolfSpiritCategoryCatalog.AllDefinitions)
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.SourceLocator));
        }
    }

    [Fact]
    public void UmbraRealmCatalogContainsExactly19Realms()
    {
        Assert.Equal(19, WerewolfUmbraRealmCatalog.AllDefinitions.Count);
    }

    [Fact]
    public void UmbraRealmKeysAreUnique()
    {
        var keys = WerewolfUmbraRealmCatalog.AllDefinitions.Select(r => r.RealmKey).ToList();
        Assert.Equal(keys.Count, keys.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void UmbraRealmEntriesHaveSourceLocators()
    {
        foreach (var entry in WerewolfUmbraRealmCatalog.AllDefinitions)
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.SourceLocator));
        }
    }

    [Fact]
    public void BarrierCatalogContainsExactly3Barriers()
    {
        Assert.Equal(3, WerewolfSpiritBarrierCatalog.AllDefinitions.Count);
    }

    [Fact]
    public void BarrierKeysAreUnique()
    {
        var keys = WerewolfSpiritBarrierCatalog.AllDefinitions.Select(b => b.BarrierKey).ToList();
        Assert.Equal(keys.Count, keys.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void BarrierEntriesHaveSourceLocators()
    {
        foreach (var entry in WerewolfSpiritBarrierCatalog.AllDefinitions)
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.SourceLocator));
        }
    }

    [Fact]
    public void SpiritTraitSchemaContainsExact4Traits()
    {
        Assert.Equal(4, WerewolfSpiritTraitSchema.AllTraits.Count);
    }

    [Fact]
    public void SpiritTraitKeysAreUnique()
    {
        var keys = WerewolfSpiritTraitSchema.AllTraits.Select(t => t.TraitKey).ToList();
        Assert.Equal(keys.Count, keys.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void SpiritTraitEntriesHaveSourceLocators()
    {
        foreach (var trait in WerewolfSpiritTraitSchema.AllTraits)
        {
            Assert.False(string.IsNullOrWhiteSpace(trait.SourceLocator));
        }
    }

    [Fact]
    public void EssenceFormulaMatchesSource()
    {
        var essence = WerewolfSpiritTraitSchema.Get("spirit.trait.essence");
        Assert.NotNull(essence);
        Assert.Equal("Willpower + Rage + Gnosis", essence.Formula);
    }

    [Fact]
    public void CharmCatalogContainsExactly30Charms()
    {
        Assert.Equal(30, WerewolfSpiritCharmCatalog.AllDefinitions.Count);
    }

    [Fact]
    public void CharmKeysAreUnique()
    {
        var keys = WerewolfSpiritCharmCatalog.AllDefinitions.Select(c => c.CharmKey).ToList();
        Assert.Equal(keys.Count, keys.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void CharmCategoryDistributionMatchesSource()
    {
        var common = WerewolfSpiritCharmCatalog.AllDefinitions.Count(c => c.CharmType == WerewolfSpiritCharmType.Common);
        var special = WerewolfSpiritCharmCatalog.AllDefinitions.Count(c => c.CharmType == WerewolfSpiritCharmType.Special);
        var bane = WerewolfSpiritCharmCatalog.AllDefinitions.Count(c => c.CharmType == WerewolfSpiritCharmType.Bane);
        var weaver = WerewolfSpiritCharmCatalog.AllDefinitions.Count(c => c.CharmType == WerewolfSpiritCharmType.Weaver);
        var wyld = WerewolfSpiritCharmCatalog.AllDefinitions.Count(c => c.CharmType == WerewolfSpiritCharmType.Wyld);

        Assert.Equal(4, common);
        Assert.Equal(17, special);
        Assert.Equal(4, bane);
        Assert.Equal(3, weaver);
        Assert.Equal(2, wyld);
        Assert.Equal(30, common + special + bane + weaver + wyld);
    }

    [Fact]
    public void CharmEntriesHaveSourceLocators()
    {
        foreach (var charm in WerewolfSpiritCharmCatalog.AllDefinitions)
        {
            Assert.False(string.IsNullOrWhiteSpace(charm.SourceLocator));
        }
    }

    [Fact]
    public void CriarChamasAndCriarVentoAreDistinctCharms()
    {
        var criarChamas = WerewolfSpiritCharmCatalog.Get("spirit.charm.special.criar-chamas");
        var criarVento = WerewolfSpiritCharmCatalog.Get("spirit.charm.special.criar-vento");

        Assert.NotNull(criarChamas);
        Assert.NotNull(criarVento);
        Assert.NotEqual(criarChamas.CharmKey, criarVento.CharmKey);
        Assert.Equal("Criar Chamas", criarChamas.CanonicalName);
        Assert.Equal("Criar Vento", criarVento.CanonicalName);
        Assert.False(string.IsNullOrWhiteSpace(criarChamas.SourceLocator));
        Assert.False(string.IsNullOrWhiteSpace(criarVento.SourceLocator));
    }

    [Fact]
    public void TotalUniqueCharmKeysEquals30()
    {
        var keys = WerewolfSpiritCharmCatalog.AllDefinitions.Select(c => c.CharmKey).ToList();
        Assert.Equal(30, keys.Count);
        Assert.Equal(30, keys.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void ExistingTotemsAreNotDuplicated()
    {
        Assert.Equal(19, WerewolfTotemIdentifiers.Supported.Count);
        Assert.Equal(19, WerewolfTotemCatalog.AllDefinitions.Count);
    }

    [Fact]
    public void NoRuntimeOperationsAdded()
    {
        var werewolfAssembly = typeof(WerewolfSpiritCategoryCatalog).Assembly;
        var serviceTypes = werewolfAssembly.GetTypes().Where(t => t.Name.EndsWith("Service", StringComparison.Ordinal)).ToList();
        Assert.DoesNotContain(serviceTypes, t => t.Namespace?.Contains("Werewolf") == true && t.Name.Contains("Spirit"));
    }
}
