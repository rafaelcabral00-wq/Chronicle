using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfRiteCatalog
{
    private static readonly WerewolfRiteDefinition[] All = new[]
    {
        new WerewolfRiteDefinition(
            WerewolfRiteIdentifiers.HuntingStone,
            "Ritual da Pedra Caçadora",
            "Místicos",
            1,
            WerewolfAttributeIdentifiers.Wits,
            WerewolfAbilityIdentifiers.Rituals,
            7,
            null,
            null,
            null,
            null,
            "person-or-object",
            "Fornece apenas a localização geral do alvo. A posse de um pedaço do alvo reduz a dificuldade em 1 ponto.",
            "Line 2669-2671")
    };

    public static WerewolfRiteDefinition? Get(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        return All.FirstOrDefault(definition => string.Equals(definition.Key, key, StringComparison.Ordinal));
    }

    public static IReadOnlyList<WerewolfRiteDefinition> GetAll()
    {
        return new ReadOnlyCollection<WerewolfRiteDefinition>(All);
    }
}
