using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfUmbraRealmCatalog
{
    private static readonly WerewolfUmbraRealmDefinition[] All = new[]
    {
        new WerewolfUmbraRealmDefinition("spirit.realm.penumbra", "Penumbra", "Penumbra", "Lines 3198, 3265-3275", "Layer", "Immediate spiritual reflection of the physical world"),
        new WerewolfUmbraRealmDefinition("spirit.realm.umbra-rasa", "Umbra Rasa", "Umbra Rasa", "Lines 3199, 3291-3293", "Layer", "Near-earth spiritual plane"),
        new WerewolfUmbraRealmDefinition("spirit.realm.deep-umbra", "Umbra Profunda", "Deep Umbra", "Lines 3370-3371", "Layer", "Abstract realms beyond the Membrane"),
        new WerewolfUmbraRealmDefinition("spirit.realm.umbra-negra", "Umbra Negra", "Umbra Negra", "Lines 3374-3375", "Layer", "Underworld realm of the dead"),
        new WerewolfUmbraRealmDefinition("spirit.realm.abismo", "Abismo", "Abyss", "Lines 3295-3299", "Realm", "Realm of extreme destruction and void"),
        new WerewolfUmbraRealmDefinition("spirit.realm.campo-de-batalha", "Campo de Batalha", "Battlefield", "Lines 3300-3304", "Realm", "Realm formed by human glorification of war"),
        new WerewolfUmbraRealmDefinition("spirit.realm.erebo", "Erebo", "Erebus", "Lines 3305-3309", "Realm", "Purgatory realm for Garou"),
        new WerewolfUmbraRealmDefinition("spirit.realm.fluxo", "Fluxo", "Flux", "Lines 3310-3314", "Realm", "Realm of pure Wyld energy"),
        new WerewolfUmbraRealmDefinition("spirit.realm.cicatriz", "Cicatriz", "Scar", "Lines 3315-3319", "Realm", "Industrial corrupted city realm; Gauntlet level 9"),
        new WerewolfUmbraRealmDefinition("spirit.realm.malfeas", "Malfeas", "Malfeas", "Lines 3320-3325", "Realm", "Realm of the Wyrm itself"),
        new WerewolfUmbraRealmDefinition("spirit.realm.pangeia", "Pangeia", "Pangaea", "Lines 3326-3331", "Realm", "Primordial Umbra realm; Garou gain 1 Gnose at dawn"),
        new WerewolfUmbraRealmDefinition("spirit.realm.pais-do-verao", "Pais do Verao", "Summer Country", "Lines 3332-3336", "Realm", "Legendary realm of peace, grace, pleasure and healing"),
        new WerewolfUmbraRealmDefinition("spirit.realm.reino-da-atrocidade", "Reino da Atrocidade", "Realm of Atrocity", "Lines 3337-3341", "Realm", "Realm mirroring torture, profanation and cruelty"),
        new WerewolfUmbraRealmDefinition("spirit.realm.reino-cibernetico", "Reino Cibernetico", "CyberRealm", "Lines 3342-3347", "Realm", "Weaver-generated realm of technology and technoshock"),
        new WerewolfUmbraRealmDefinition("spirit.realm.reino-etereo", "Reino Etereo", "Ethereal Realm", "Lines 3348-3352", "Realm", "Upper Umbra region associated with sky, air, stars and planets"),
        new WerewolfUmbraRealmDefinition("spirit.realm.reino-lendario", "Reino Lendario", "Legendary Realm", "Lines 3353-3356", "Realm", "Realm composed of Garou myths and legends"),
        new WerewolfUmbraRealmDefinition("spirit.realm.toca-dos-lobos", "Toca dos Lobos", "Wolf Den", "Lines 3357-3361", "Realm", "Umbral realm hostile to wolves; Garou trapped in Lupus form"),
        new WerewolfUmbraRealmDefinition("spirit.realm.zona-onirica", "Zona Onirica", "Dream Zone", "Lines 3362-3363", "Zone", "Umbral zone transcending normal Umbra limits; borders Umbra Rasa and Deep Umbra"),
        new WerewolfUmbraRealmDefinition("spirit.realm.periferia", "Periferia", "Periphery", "Lines 3372-3373", "Zone", "Exudation of spiritual energy from the Gauntlet; altered consciousness state")
    };

    public static IReadOnlyDictionary<string, WerewolfUmbraRealmDefinition> ByKey { get; } =
        new ReadOnlyDictionary<string, WerewolfUmbraRealmDefinition>(All.ToDictionary(r => r.RealmKey, StringComparer.Ordinal));

    public static IReadOnlyList<WerewolfUmbraRealmDefinition> AllDefinitions { get; } = Array.AsReadOnly(All);

    public static WerewolfUmbraRealmDefinition? Get(string realmKey)
    {
        if (string.IsNullOrWhiteSpace(realmKey))
        {
            return null;
        }

        return ByKey.TryGetValue(realmKey, out var definition) ? definition : null;
    }
}
