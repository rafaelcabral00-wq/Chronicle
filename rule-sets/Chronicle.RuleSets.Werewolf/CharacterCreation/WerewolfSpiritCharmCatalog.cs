using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfSpiritCharmCatalog
{
    private static readonly WerewolfSpiritCharmDefinition[] All = BuildAll();

    public static IReadOnlyDictionary<string, WerewolfSpiritCharmDefinition> ByKey { get; } =
        new ReadOnlyDictionary<string, WerewolfSpiritCharmDefinition>(All.ToDictionary(c => c.CharmKey, StringComparer.Ordinal));

    public static IReadOnlyList<WerewolfSpiritCharmDefinition> AllDefinitions { get; } = Array.AsReadOnly(All);

    public static WerewolfSpiritCharmDefinition? Get(string charmKey)
    {
        if (string.IsNullOrWhiteSpace(charmKey))
        {
            return null;
        }

        return ByKey.TryGetValue(charmKey, out var definition) ? definition : null;
    }

    private static WerewolfSpiritCharmDefinition[] BuildAll()
    {
        return BuildCommonCharms()
            .Concat(BuildSpecialCharms())
            .Concat(BuildBaneCharms())
            .Concat(BuildWeaverCharms())
            .Concat(BuildWyldCharms())
            .ToArray();

        static WerewolfSpiritCharmDefinition[] BuildCommonCharms()
        {
            return
            [
                new WerewolfSpiritCharmDefinition("spirit.charm.common.materializar", "Materializar", "Materialize", WerewolfSpiritCharmType.Common, "Line 3414", "Assume physical form on Earth if Gnose >= local Gauntlet; adopts physical health levels (usually 7)"),
                new WerewolfSpiritCharmDefinition("spirit.charm.common.reformar", "Reformar", "Reform", WerewolfSpiritCharmType.Common, "Line 3415", "Dissolve material form to return to native Umbra domains in one turn"),
                new WerewolfSpiritCharmDefinition("spirit.charm.common.sentido-de-orientacao", "Sentido de OrientaÃ§Ã£o", "Sense of Direction", WerewolfSpiritCharmType.Common, "Line 3416", "Facilitates direction finding and use of spiritual trails in Umbra"),
                new WerewolfSpiritCharmDefinition("spirit.charm.common.sentir-o-reino", "Sentir o Reino", "Sense the Realm", WerewolfSpiritCharmType.Common, "Line 3417", "Sense events in specific domain; common among Naturae and area-bound spirits")
            ];
        }

        static WerewolfSpiritCharmDefinition[] BuildSpecialCharms()
        {
            return
            [
                new WerewolfSpiritCharmDefinition("spirit.charm.special.abrir-ponte-da-lua", "Abrir Ponte da Lua", "Open Moon Bridge", WerewolfSpiritCharmType.Special, "Line 3420", "Create lunar bridge up to 1,500 km without requiring a caern"),
                new WerewolfSpiritCharmDefinition("spirit.charm.special.armadura", "Armadura", "Armor", WerewolfSpiritCharmType.Special, "Line 3421", "Grant absorption equal to Gnose for rest of scene (costs 2 Essence)"),
                new WerewolfSpiritCharmDefinition("spirit.charm.special.congelar", "Congelar", "Freeze", WerewolfSpiritCharmType.Special, "Line 3422", "Reduce temperature and cause aggravated damage to targets in area based on reduced Rage"),
                new WerewolfSpiritCharmDefinition("spirit.charm.special.controle-de-sistemas-eletricos", "Controle de Sistemas ElÃ©tricos", "Control Electrical Systems", WerewolfSpiritCharmType.Special, "Line 3423", "Manipulate or overload electrical systems through Gnose tests"),
                new WerewolfSpiritCharmDefinition("spirit.charm.special.criar-chamas", "Criar Chamas", "Create Flames", WerewolfSpiritCharmType.Special, "Line 3424", "Generate small fires based on Gnose"),
                new WerewolfSpiritCharmDefinition("spirit.charm.special.criar-vento", "Criar Vento", "Create Wind", WerewolfSpiritCharmType.Special, "Line 3424", "Generate gusts and wind effects based on Gnose"),
                new WerewolfSpiritCharmDefinition("spirit.charm.special.curar", "Curar", "Heal", WerewolfSpiritCharmType.Special, "Line 3425", "Restore vitality levels in physical beings (maximum equal to Gnose)"),
                new WerewolfSpiritCharmDefinition("spirit.charm.special.espiar", "Espiar", "Scry", WerewolfSpiritCharmType.Special, "Line 3426", "Observe at distance in Penumbra or physical world"),
                new WerewolfSpiritCharmDefinition("spirit.charm.special.estilhacar-vidro", "EstilhaÃ§ar Vidro", "Shatter Glass", WerewolfSpiritCharmType.Special, "Line 3427", "Break nearby glass objects"),
                new WerewolfSpiritCharmDefinition("spirit.charm.special.inundacao", "InundaÃ§Ã£o", "Inundate", WerewolfSpiritCharmType.Special, "Line 3428", "Rapidly raise water levels in an area (costs 1 Essence)"),
                new WerewolfSpiritCharmDefinition("spirit.charm.special.levitacao", "LevitaÃ§Ã£o", "Levitation", WerewolfSpiritCharmType.Special, "Line 3429", "Lift a human-sized creature using Willpower"),
                new WerewolfSpiritCharmDefinition("spirit.charm.special.metamorfose", "Metamorfose", "Metamorphosis", WerewolfSpiritCharmType.Special, "Line 3430", "Assume appearance and form of any object or being without copying powers"),
                new WerewolfSpiritCharmDefinition("spirit.charm.special.purificar-dominios-sombrios", "Purificar os DomÃ­nios Sombrios", "Purify Shadow Domains", WerewolfSpiritCharmType.Special, "Line 3431", "Purge spiritual corruption in specific areas"),
                new WerewolfSpiritCharmDefinition("spirit.charm.special.rajada", "Rajada", "Burst", WerewolfSpiritCharmType.Special, "Line 3432", "Fire distant attacks causing aggravated damage equal to Rage (costs 1 Essence)"),
                new WerewolfSpiritCharmDefinition("spirit.charm.special.rastrear", "Rastrear", "Track", WerewolfSpiritCharmType.Special, "Line 3433", "Track prey infallibly (costs 1 Essence)"),
                new WerewolfSpiritCharmDefinition("spirit.charm.special.umbramoto", "Umbramoto", "Umbraquake", WerewolfSpiritCharmType.Special, "Line 3434", "Shake Umbra, knocking down targets and causing bashing damage"),
                new WerewolfSpiritCharmDefinition("spirit.charm.special.voo-ligeiro", "VÃ´o Ligeiro", "Swift Flight", WerewolfSpiritCharmType.Special, "Line 3435", "Triple flying movement speed in Umbra")
            ];
        }

        static WerewolfSpiritCharmDefinition[] BuildBaneCharms()
        {
            return
            [
                new WerewolfSpiritCharmDefinition("spirit.charm.bane.corrupcao", "CorrupÃ§Ã£o", "Corruption", WerewolfSpiritCharmType.Bane, "Lines 3439", "Whisper malignant suggestion to target; Gnose vs Willpower; works through Gauntlet"),
                new WerewolfSpiritCharmDefinition("spirit.charm.bane.incitar-o-frenesi", "Incitar o Frenesi", "Incite Frenzy", WerewolfSpiritCharmType.Bane, "Line 3440", "Make a Garou enter frenzy; Rage vs Willpower; normal frenzy rules apply"),
                new WerewolfSpiritCharmDefinition("spirit.charm.bane.influencia-malefica", "InfluÃªncia MalÃ©fica", "Malefic Influence", WerewolfSpiritCharmType.Bane, "Line 3441", "Negatively influence target; on success target tests Willpower; failure dominates personality for hours; critical failure makes influence permanent"),
                new WerewolfSpiritCharmDefinition("spirit.charm.bane.possessao", "PossessÃ£o", "Possession", WerewolfSpiritCharmType.Bane, "Lines 3442-3450", "Possess living being or inactive object; Gnose test vs Willpower; duration by successes (1=6h, 2=3h, 3=1h, 4=15min, 5=5min, 6+=instant); human host becomes fomori permanently")
            ];
        }

        static WerewolfSpiritCharmDefinition[] BuildWeaverCharms()
        {
            return
            [
                new WerewolfSpiritCharmDefinition("spirit.charm.weaver.estatica-espiritual", "EstÃ¡tica Espiritual", "Spiritual Static", WerewolfSpiritCharmType.Weaver, "Lines 3453", "Increase Gauntlet of an area by 1 (up to 3 with cooperation); spirit must remain in location; distracted spirits suffer -2 dice pools"),
                new WerewolfSpiritCharmDefinition("spirit.charm.weaver.petrificar", "Petrificar", "Petrify", WerewolfSpiritCharmType.Weaver, "Line 3454", "Trap target in Pattern Web; spirit Willpower vs target Rage; each success subtracts 1 from Physical Attributes or Essence; at zero target is bound to Web"),
                new WerewolfSpiritCharmDefinition("spirit.charm.weaver.solidificar-a-realidade", "Solidificar a Realidade", "Solidify Reality", WerewolfSpiritCharmType.Weaver, "Line 3455", "Reinforce Weaver laws in Umbra; Willpower test; each success increases target Essence or vitality levels by 1 for one day; once per target")
            ];
        }

        static WerewolfSpiritCharmDefinition[] BuildWyldCharms()
        {
            return
            [
                new WerewolfSpiritCharmDefinition("spirit.charm.wyld.desorientar", "Desorientar", "Disorient", WerewolfSpiritCharmType.Wyld, "Line 3457", "Alter reference points and directions; Gnose test (difficulty 6 or Gauntlet level)"),
                new WerewolfSpiritCharmDefinition("spirit.charm.wyld.romper-a-realidade", "Romper a Realidade", "Break Reality", WerewolfSpiritCharmType.Wyld, "Line 3458", "Disintegrate Umbral substance; Gnose test to modify form (e.g., create door); failures cause Essence and Gnose loss")
            ];
        }
    }
}

