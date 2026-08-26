using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfSpiritTraitSchema
{
    public static IReadOnlyList<WerewolfSpiritTraitDefinition> AllTraits { get; } =
    [
        new WerewolfSpiritTraitDefinition(
            "spirit.trait.willpower",
            "Força de Vontade",
            "Willpower",
            "Line 3407",
            Notes: "Concedes coordination and self-control; used for physical actions (attack, fly) and damage absorption"),
        new WerewolfSpiritTraitDefinition(
            "spirit.trait.rage",
            "Fúria",
            "Rage",
            "Line 3408",
            Notes: "Functions as survival desire and equivalent to Strength in attacks; determines difficulty to wound the spirit"),
        new WerewolfSpiritTraitDefinition(
            "spirit.trait.gnosis",
            "Gnose",
            "Gnosis",
            "Line 3409",
            Notes: "Measures cosmic consciousness; employed in all Social and Mental tests"),
        new WerewolfSpiritTraitDefinition(
            "spirit.trait.essence",
            "Essência",
            "Essence",
            "Line 3410",
            Formula: "Willpower + Rage + Gnosis",
            Notes: "Represents vitality and survival points; when exhausted, spirit dies, enters Modorra, or is destroyed")
    ];

    public static IReadOnlyDictionary<string, WerewolfSpiritTraitDefinition> ByKey { get; } =
        new ReadOnlyDictionary<string, WerewolfSpiritTraitDefinition>(AllTraits.ToDictionary(t => t.TraitKey, StringComparer.Ordinal));

    public static WerewolfSpiritTraitDefinition? Get(string traitKey)
    {
        if (string.IsNullOrWhiteSpace(traitKey))
        {
            return null;
        }

        return ByKey.TryGetValue(traitKey, out var definition) ? definition : null;
    }
}
