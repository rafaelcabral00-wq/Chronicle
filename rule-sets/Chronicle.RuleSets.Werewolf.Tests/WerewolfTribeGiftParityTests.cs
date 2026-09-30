using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

/// <summary>
/// Wave 2D-1: Tribe Gift catalog parity and alias resolution.
/// Source sections 27-38 (lines 2104-2561) define 11 Gifts for each of the
/// 12 tribes, 132 in total.
/// </summary>
public sealed class WerewolfTribeGiftParityTests
{
    private const string ReplacementCharacter = "\uFFFD";

    private static readonly string[] Tribes =
    {
        WerewolfTribeIdentifiers.GlassWalkers,
        WerewolfTribeIdentifiers.GetOfFenris,
        WerewolfTribeIdentifiers.Fianna,
        WerewolfTribeIdentifiers.ChildrenOfGaia,
        WerewolfTribeIdentifiers.BlackFuries,
        WerewolfTribeIdentifiers.RedTalons,
        WerewolfTribeIdentifiers.SilentStriders,
        WerewolfTribeIdentifiers.SilverFangs,
        WerewolfTribeIdentifiers.BoneGnawers,
        WerewolfTribeIdentifiers.ShadowLords,
        WerewolfTribeIdentifiers.Uktena,
        WerewolfTribeIdentifiers.Wendigo
    };

    private static List<WerewolfGiftDefinition> TribeGifts() =>
        WerewolfGiftCatalog.AllDefinitions
            .Where(g => g.Category == WerewolfGiftCategory.Tribe)
            .ToList();

    [Fact]
    public void CatalogContainsExactlyOneHundredAndThirtyTwoTribeGifts()
    {
        Assert.Equal(132, TribeGifts().Count);
    }

    [Fact]
    public void AllTwelveTribesAreRepresented()
    {
        var tribes = TribeGifts().Select(g => g.OwnerKey).Distinct(StringComparer.Ordinal).ToList();

        Assert.Equal(12, tribes.Count);
        Assert.All(Tribes, tribe => Assert.Contains(tribe, tribes, StringComparer.Ordinal));
    }

    [Theory]
    [InlineData(WerewolfTribeIdentifiers.GlassWalkers)]
    [InlineData(WerewolfTribeIdentifiers.GetOfFenris)]
    [InlineData(WerewolfTribeIdentifiers.Fianna)]
    [InlineData(WerewolfTribeIdentifiers.ChildrenOfGaia)]
    [InlineData(WerewolfTribeIdentifiers.BlackFuries)]
    [InlineData(WerewolfTribeIdentifiers.RedTalons)]
    [InlineData(WerewolfTribeIdentifiers.SilentStriders)]
    [InlineData(WerewolfTribeIdentifiers.SilverFangs)]
    [InlineData(WerewolfTribeIdentifiers.BoneGnawers)]
    [InlineData(WerewolfTribeIdentifiers.ShadowLords)]
    [InlineData(WerewolfTribeIdentifiers.Uktena)]
    [InlineData(WerewolfTribeIdentifiers.Wendigo)]
    public void EachTribeHasExactlyElevenGifts(string tribe)
    {
        Assert.Equal(11, TribeGifts().Count(g => g.OwnerKey == tribe));
    }

    [Fact]
    public void EveryTribeGiftHasAValidSourceLocator()
    {
        foreach (var gift in TribeGifts())
        {
            Assert.StartsWith("Line ", gift.SourceLocator, StringComparison.Ordinal);
            Assert.True(int.TryParse(gift.SourceLocator.AsSpan("Line ".Length), out var line) && line > 0,
                $"Gift '{gift.GiftKey}' has an invalid SourceLocator '{gift.SourceLocator}'.");
        }
    }

    [Fact]
    public void ExactlyTwentyFiveTribeGiftsDeclareASourceAlias()
    {
        var aliased = TribeGifts().Where(g => g.AliasedGiftKey is not null).ToList();

        Assert.Equal(25, aliased.Count);
    }

    [Fact]
    public void EveryAliasResolvesToAnExistingCanonicalGift()
    {
        foreach (var gift in TribeGifts().Where(g => g.AliasedGiftKey is not null))
        {
            Assert.NotNull(WerewolfGiftCatalog.Get(gift.AliasedGiftKey!));
        }
    }

    [Fact]
    public void NoGiftAliasesItself()
    {
        foreach (var gift in TribeGifts().Where(g => g.AliasedGiftKey is not null))
        {
            Assert.NotEqual(gift.GiftKey, gift.AliasedGiftKey);
        }
    }

    [Fact]
    public void AliasChainsTerminateWithoutCycles()
    {
        foreach (var gift in TribeGifts())
        {
            var seen = new HashSet<string>(StringComparer.Ordinal) { gift.GiftKey };
            var current = gift;

            while (current.AliasedGiftKey is not null)
            {
                Assert.True(
                    seen.Add(current.AliasedGiftKey),
                    $"Alias cycle detected while resolving '{gift.GiftKey}'.");
                current = WerewolfGiftCatalog.Get(current.AliasedGiftKey)
                    ?? throw new InvalidOperationException(
                        $"Alias target '{current.AliasedGiftKey}' for '{gift.GiftKey}' does not exist.");
            }
        }
    }

    [Fact]
    public void SourceVariantsAreDistinctNonAliasedGifts()
    {
        // Source line 2313: Red Talons "Favor do Elemental" derives from the
        // Glass Walkers Gift but manipulates all four classic elements.
        var favor = TribeGifts().Single(g => g.SourceLocator == "Line 2313");
        Assert.Equal(WerewolfTribeIdentifiers.RedTalons, favor.OwnerKey);
        Assert.Null(favor.AliasedGiftKey);

        // Source line 2359: Silent Striders "Harmonia" derives from the Glass
        // Walkers Gift but preselects a city or nature operation.
        var harmonia = TribeGifts().Single(g => g.SourceLocator == "Line 2359");
        Assert.Equal(WerewolfTribeIdentifiers.SilentStriders, harmonia.OwnerKey);
        Assert.Null(harmonia.AliasedGiftKey);
    }

    [Fact]
    public void EveryTribeGiftHasNonEmptyLocalizedTextWithoutEncodingArtifacts()
    {
        foreach (var gift in TribeGifts())
        {
            Assert.False(string.IsNullOrWhiteSpace(gift.NamePtBr), $"'{gift.GiftKey}' NamePtBr is empty.");
            Assert.False(string.IsNullOrWhiteSpace(gift.EffectDescriptionPtBr), $"'{gift.GiftKey}' EffectDescriptionPtBr is empty.");
            Assert.DoesNotContain(ReplacementCharacter, gift.NamePtBr, StringComparison.Ordinal);
            Assert.DoesNotContain(ReplacementCharacter, gift.EffectDescriptionPtBr, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void BreedAndAuspiceCatalogsAreUnaffected()
    {
        Assert.Equal(33, WerewolfGiftCatalog.AllDefinitions.Count(g => g.Category == WerewolfGiftCategory.Breed));
        Assert.Equal(60, WerewolfGiftCatalog.AllDefinitions.Count(g => g.Category == WerewolfGiftCategory.Auspice));
    }
}
