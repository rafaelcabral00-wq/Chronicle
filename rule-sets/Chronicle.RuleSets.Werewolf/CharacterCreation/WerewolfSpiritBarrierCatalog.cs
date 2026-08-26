using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfSpiritBarrierCatalog
{
    private static readonly WerewolfSpiritBarrierDefinition[] All = new[]
    {
        new WerewolfSpiritBarrierDefinition(
            "spirit.barrier.pelicula",
            "Película",
            "Gauntlet",
            "Lines 439, 3196-3197, 3220, 3235-3249",
            "Barrier separating physical world from Penumbra; difficulty 2-9 varying by location"),
        new WerewolfSpiritBarrierDefinition(
            "spirit.barrier.membrana",
            "Membrana",
            "Membrane",
            "Lines 436, 3368-3369",
            "Barrier separating Umbra Rasa from Deep Umbra; requires Ádito to cross"),
        new WerewolfSpiritBarrierDefinition(
            "spirit.barrier.teia-do-padrao",
            "Teia do Padrão",
            "Pattern Web",
            "Lines 3225, 3453-3455",
            "Weaver's pattern web; reinforces reality laws in Umbra")
    };

    public static IReadOnlyDictionary<string, WerewolfSpiritBarrierDefinition> ByKey { get; } =
        new ReadOnlyDictionary<string, WerewolfSpiritBarrierDefinition>(All.ToDictionary(b => b.BarrierKey, StringComparer.Ordinal));

    public static IReadOnlyList<WerewolfSpiritBarrierDefinition> AllDefinitions { get; } = Array.AsReadOnly(All);

    public static WerewolfSpiritBarrierDefinition? Get(string barrierKey)
    {
        if (string.IsNullOrWhiteSpace(barrierKey))
        {
            return null;
        }

        return ByKey.TryGetValue(barrierKey, out var definition) ? definition : null;
    }
}
