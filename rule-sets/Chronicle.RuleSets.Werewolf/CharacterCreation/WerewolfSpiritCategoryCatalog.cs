using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfSpiritCategoryCatalog
{
    private static readonly WerewolfSpiritCategoryDefinition[] All = new[]
    {
        new WerewolfSpiritCategoryDefinition(
            WerewolfSpiritIdentifiers.Totem,
            "Totem",
            "Totem",
            "Lines 449, 3400"),
        new WerewolfSpiritCategoryDefinition(
            WerewolfSpiritIdentifiers.Bane,
            "Bane",
            "Bane",
            "Lines 434, 3437-3441"),
        new WerewolfSpiritCategoryDefinition(
            WerewolfSpiritIdentifiers.Naturae,
            "Naturae",
            "Naturae",
            "Lines 3417"),
        new WerewolfSpiritCategoryDefinition(
            WerewolfSpiritIdentifiers.Incarna,
            "Incarna",
            "Incarna",
            "Lines 427, 3396, 3399-3400"),
        new WerewolfSpiritCategoryDefinition(
            WerewolfSpiritIdentifiers.Celestine,
            "Celestine",
            "Celestine",
            "Lines 407, 3395"),
        new WerewolfSpiritCategoryDefinition(
            WerewolfSpiritIdentifiers.Jaggling,
            "Jaggling",
            "Jaggling",
            "Lines 428, 3397, 3401-3402"),
        new WerewolfSpiritCategoryDefinition(
            WerewolfSpiritIdentifiers.Gaffling,
            "Gaffling",
            "Gaffling",
            "Lines 418, 3397, 3403-3404"),
        new WerewolfSpiritCategoryDefinition(
            WerewolfSpiritIdentifiers.Ancestor,
            "Ancestor",
            "Ancestor",
            "Line 2005")
    };

    public static IReadOnlyDictionary<string, WerewolfSpiritCategoryDefinition> ByKey { get; } =
        new ReadOnlyDictionary<string, WerewolfSpiritCategoryDefinition>(All.ToDictionary(c => c.SpiritCategoryKey, StringComparer.Ordinal));

    public static IReadOnlyList<WerewolfSpiritCategoryDefinition> AllDefinitions { get; } = Array.AsReadOnly(All);

    public static WerewolfSpiritCategoryDefinition? Get(string spiritCategoryKey)
    {
        if (string.IsNullOrWhiteSpace(spiritCategoryKey))
        {
            return null;
        }

        return ByKey.TryGetValue(spiritCategoryKey, out var definition) ? definition : null;
    }
}
