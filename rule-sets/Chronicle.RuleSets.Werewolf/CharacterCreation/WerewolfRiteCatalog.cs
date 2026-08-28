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
            "Line 2669-2671"),
        new WerewolfRiteDefinition(
            WerewolfRiteIdentifiers.Fetish,
            "Ritual de Fetiche",
            "Místicos",
            3,
            WerewolfAttributeIdentifiers.Wits,
            WerewolfAbilityIdentifiers.Rituals,
            10,
            null,
            null,
            null,
            null,
            "spirit",
            "Create fetish bound to a spirit. Each permanent Gnose point invested reduces difficulty by 2.",
            "Lines 2690, 3466-3469"),
        new WerewolfRiteDefinition(
            WerewolfRiteIdentifiers.Totem,
            "Ritual de Totem",
            "Místicos",
            3,
            WerewolfAttributeIdentifiers.Wits,
            WerewolfAbilityIdentifiers.Rituals,
            7,
            null,
            null,
            null,
            null,
            "spirit",
            "Binds a totemic spirit to a group of Garou to form a pack.",
            "Line 2693"),
        new WerewolfRiteDefinition(
            WerewolfRiteIdentifiers.Summoning,
            "Ritual de Conjuração",
            "Místicos",
            2,
            string.Empty,
            string.Empty,
            6,
            null,
            null,
            null,
            null,
            "spirit",
            "Summon spirit with Gnose cost. Test vs spirit Willpower.",
            "Line 2681"),
        new WerewolfRiteDefinition(
            WerewolfRiteIdentifiers.Commitment,
            "Ritual de Compromisso",
            "Místicos",
            1,
            string.Empty,
            string.Empty,
            6,
            null,
            null,
            null,
            null,
            "spirit",
            "Bind spirit to object via resisted Willpower vs spirit Gnose test. Creates amulet.",
            "Line 2666"),
        new WerewolfRiteDefinition(
            WerewolfRiteIdentifiers.AwakenSpirits,
            "Ritual para Despertar Espíritos",
            "Místicos",
            2,
            string.Empty,
            string.Empty,
            6,
            null,
            null,
            null,
            null,
            "spirit",
            "Awaken spirits. Requires Fury spend/test and Extended test.",
            "Line 2678"),
        new WerewolfRiteDefinition(
            WerewolfRiteIdentifiers.CaernOpening,
            "Ritual de Abertura de Caern",
            "Caern",
            1,
            WerewolfAttributeIdentifiers.Wits,
            WerewolfAbilityIdentifiers.Rituals,
            7,
            null,
            null,
            null,
            null,
            "place",
            "Open Caern. Extended resisted Raciocínio + Rituais against Caern spirit. Requires Sept and collective participants.",
            "Line 2586"),
        new WerewolfRiteDefinition(
            WerewolfRiteIdentifiers.CaernCreation,
            "Ritual de Criação de Caern",
            "Caern",
            5,
            WerewolfAttributeIdentifiers.Wits,
            WerewolfAbilityIdentifiers.Rituals,
            8,
            40,
            null,
            null,
            null,
            "place",
            "Create new Caern. Extended Raciocínio + Rituais accumulating 40 successes over hourly tests. Requires 13+ Garou, permanent Gnose cost.",
            "Line 2600")
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
