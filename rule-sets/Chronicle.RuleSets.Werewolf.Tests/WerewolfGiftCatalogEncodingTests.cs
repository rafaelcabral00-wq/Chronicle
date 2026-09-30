using System.Text;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

/// <summary>
/// Wave 2D-0: catalog text and encoding integrity.
/// Proves the Gift catalog contains no encoding artifacts and that every
/// localized field is valid, non-empty, and traceable to the source.
/// </summary>
public sealed class WerewolfGiftCatalogEncodingTests
{
    private const string ReplacementCharacter = "\uFFFD";

    private static string CatalogSourcePath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Chronicle.sln")))
            {
                return Path.Combine(
                    directory.FullName,
                    "rule-sets",
                    "Chronicle.RuleSets.Werewolf",
                    "CharacterCreation",
                    "WerewolfGiftCatalog.cs");
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not find repository root from test base directory.");
    }

    [Fact]
    public void CatalogSourceFileContainsNoReplacementCharacters()
    {
        var bytes = File.ReadAllBytes(CatalogSourcePath());

        // Strict UTF-8 decode: throws on invalid byte sequences, so a broken
        // encoding can never pass silently.
        var content = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true)
            .GetString(bytes);

        Assert.DoesNotContain(ReplacementCharacter, content, StringComparison.Ordinal);
    }

    [Fact]
    public void CatalogSourceFileHasNoByteOrderMark()
    {
        var bytes = File.ReadAllBytes(CatalogSourcePath());

        Assert.False(
            bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF,
            "Catalog source must be UTF-8 without a byte order mark.");
    }

    [Fact]
    public void EveryCatalogEntryHasValidNonEmptyPortugueseText()
    {
        var entries = WerewolfGiftCatalog.AllDefinitions;

        Assert.Equal(225, entries.Count);

        foreach (var entry in entries)
        {
            Assert.False(
                string.IsNullOrWhiteSpace(entry.NamePtBr),
                $"Gift '{entry.GiftKey}' has an empty NamePtBr.");
            Assert.False(
                string.IsNullOrWhiteSpace(entry.EffectDescriptionPtBr),
                $"Gift '{entry.GiftKey}' has an empty EffectDescriptionPtBr.");
            Assert.False(
                entry.NamePtBr.Contains(ReplacementCharacter, StringComparison.Ordinal),
                $"Gift '{entry.GiftKey}' NamePtBr contains an encoding artifact.");
            Assert.False(
                entry.EffectDescriptionPtBr.Contains(ReplacementCharacter, StringComparison.Ordinal),
                $"Gift '{entry.GiftKey}' EffectDescriptionPtBr contains an encoding artifact.");
            Assert.False(
                entry.EffectDescriptionEn.Contains(ReplacementCharacter, StringComparison.Ordinal),
                $"Gift '{entry.GiftKey}' EffectDescriptionEn contains an encoding artifact.");
        }
    }

    [Fact]
    public void EveryCatalogEntryCarriesASourceLocator()
    {
        foreach (var entry in WerewolfGiftCatalog.AllDefinitions)
        {
            Assert.StartsWith("Line ", entry.SourceLocator, StringComparison.Ordinal);
            Assert.DoesNotContain(ReplacementCharacter, entry.SourceLocator, StringComparison.Ordinal);
        }
    }

    [Theory]
    [InlineData("gift.tribe.uktena.shroud", "Line 2491")]
    [InlineData("gift.tribe.uktena.sense-magic", "Line 2494")]
    [InlineData("gift.tribe.wendigo.camouflage", "Line 2527")]
    [InlineData("gift.tribe.wendigo.call-the-breeze", "Line 2530")]
    public void CorrectedUktenaAndWendigoEntriesCiteTheirTrueGiftSourceLines(
        string giftKey, string expectedLocator)
    {
        // These four entries previously pointed at non-Gift source lines, which
        // made their localized text unrecoverable and left encoding artifacts.
        var entry = WerewolfGiftCatalog.Get(giftKey);

        Assert.NotNull(entry);
        Assert.Equal(expectedLocator, entry!.SourceLocator);
        Assert.DoesNotContain(ReplacementCharacter, entry.NamePtBr, StringComparison.Ordinal);
        Assert.DoesNotContain(ReplacementCharacter, entry.EffectDescriptionPtBr, StringComparison.Ordinal);
    }

    [Fact]
    public void PortugueseNamesRetainTheirAccentedOrthography()
    {
        // Guards the specific orthography that the corruption destroyed.
        Assert.Equal("Gerar Ignorância", WerewolfGiftCatalog.Get("gift.auspice.ragabash.gerar-ignorancia")!.NamePtBr);
        Assert.Equal("Materialização de Sonhos", WerewolfGiftCatalog.Get("gift.auspice.galliard.materializacao-de-sonhos")!.NamePtBr);
        Assert.Equal("Resistência à Dor", WerewolfGiftCatalog.Get("gift.auspice.philodox.resist-pain")!.NamePtBr);
        Assert.Equal("Espírito da Batalha", WerewolfGiftCatalog.Get("gift.ahroun.espirito-da-batalha")!.NamePtBr);
    }
}
