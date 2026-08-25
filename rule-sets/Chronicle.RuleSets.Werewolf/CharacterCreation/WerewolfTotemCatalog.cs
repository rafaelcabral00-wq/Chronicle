using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfTotemCatalog
{
    private static readonly WerewolfTotemCatalogEntry[] All = BuildAll();

    public static IReadOnlyDictionary<string, WerewolfTotemCatalogEntry> ByKey { get; } =
        new ReadOnlyDictionary<string, WerewolfTotemCatalogEntry>(All.ToDictionary(t => t.TotemKey, StringComparer.Ordinal));

    public static IReadOnlyList<WerewolfTotemCatalogEntry> AllDefinitions { get; } = Array.AsReadOnly(All);

    public static WerewolfTotemCatalogEntry? Get(string totemKey)
    {
        if (string.IsNullOrWhiteSpace(totemKey))
        {
            return null;
        }

        return ByKey.TryGetValue(totemKey, out var definition) ? definition : null;
    }

    private static WerewolfTotemCatalogEntry[] BuildAll()
    {
        return
        [
            BuildAvoTrovao(),
            BuildCervo(),
            BuildFalcao(),
            BuildPegaso(),
            BuildFenris(),
            BuildGrifo(),
            BuildJavali(),
            BuildRato(),
            BuildUrso(),
            BuildWendigo(),
            BuildBarata(),
            BuildCoruja(),
            BuildCorvo(),
            BuildQuimera(),
            BuildUktena(),
            BuildUnicornio(),
            BuildCoiote(),
            BuildCuco(),
            BuildRaposa()
        ];

        static WerewolfTotemCatalogEntry BuildAvoTrovao()
        {
            return new WerewolfTotemCatalogEntry(
                WerewolfTotemIdentifiers.AvoTrovao,
                "Avô Trovão",
                "Thunderbird",
                "Thunderbird",
                "Lines 3736-3740",
                7,
                "tribe.shadow-lords",
                [
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.ResourceGrant, "+5 Willpower per story", "Line 3739"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.AbilityBonus, "+3 Etiqueta", "Line 3739"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.DiceBonus, "+2 Intimidação when invoked", "Line 3739"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.ResourceGrant, "+1 Honra per story", "Line 3739"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.BanOrRestriction, "must demand respect from equals and rivals", "Line 3740")
                ]);
        }

        static WerewolfTotemCatalogEntry BuildCervo()
        {
            return new WerewolfTotemCatalogEntry(
                WerewolfTotemIdentifiers.Cervo,
                "Cervo",
                "Stag",
                "Stag",
                "Lines 3741-3745",
                6,
                "tribe.fianna",
                [
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.ResourceGrant, "+3 Willpower per story", "Line 3744"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.AbilityBonus, "+3 Sobrevivência", "Line 3744"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.DiceBonus, "+1 Vigor for long runs", "Line 3744"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.ResourceGrant, "+3 Honra per story", "Line 3744"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.PackWideBenefit, "faires and changelings respect pack", "Line 3744"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.BanOrRestriction, "demonstrate respect for the hunt", "Line 3745"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.BanOrRestriction, "execute Oração pela Presa", "Line 3745"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.BanOrRestriction, "always aid faires", "Line 3745")
                ]);
        }

        static WerewolfTotemCatalogEntry BuildFalcao()
        {
            return new WerewolfTotemCatalogEntry(
                WerewolfTotemIdentifiers.Falcao,
                "Falcão",
                "Falcon",
                "Falcão",
                "Lines 3746-3750",
                5,
                "tribe.silver-fangs",
                [
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.ResourceGrant, "+4 Willpower per story", "Line 3749"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.AbilityBonus, "+3 Liderança", "Line 3749"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.ResourceGrant, "+2 Honra per story", "Line 3749"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.BanOrRestriction, "dishonor requires immediate repair or suicidal expiation against Wyrm servants", "Line 3750")
                ]);
        }

        static WerewolfTotemCatalogEntry BuildPegaso()
        {
            return new WerewolfTotemCatalogEntry(
                WerewolfTotemIdentifiers.Pegaso,
                "Pégaso",
                "Pegasus",
                "Pégaso",
                "Lines 3751-3755",
                4,
                "tribe.black-furies",
                [
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.ResourceGrant, "+3 Willpower per story", "Line 3754"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.AbilityBonus, "+3 Empatia com Animais", "Line 3754"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.ResourceGrant, "+2 Honra per story", "Line 3754"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.BanOrRestriction, "aid all females, especially young", "Line 3755")
                ]);
        }

        static WerewolfTotemCatalogEntry BuildFenris()
        {
            return new WerewolfTotemCatalogEntry(
                WerewolfTotemIdentifiers.Fenris,
                "Fenris",
                "Fenris",
                "Fenris",
                "Lines 3757-3761",
                5,
                "tribe.fenris",
                [
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.TraitBonus, "+1 Physical Attribute (can exceed 5)", "Line 3760"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.ResourceGrant, "+2 Glória per story", "Line 3760"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.BanOrRestriction, "never miss opportunity for worthy fight", "Line 3761")
                ]);
        }

        static WerewolfTotemCatalogEntry BuildGrifo()
        {
            return new WerewolfTotemCatalogEntry(
                WerewolfTotemIdentifiers.Grifo,
                "Grifo",
                "Griffin",
                "Grifo",
                "Lines 3762-3766",
                4,
                "tribe.red-talons",
                [
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.DiceBonus, "+3 Prontidão", "Line 3765"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.SpiritCapability, "communicate with birds of prey", "Line 3765"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.ResourceGrant, "+2 Glória per story", "Line 3765"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.BanOrRestriction, "prohibited association with humans", "Line 3766")
                ]);
        }

        static WerewolfTotemCatalogEntry BuildJavali()
        {
            return new WerewolfTotemCatalogEntry(
                WerewolfTotemIdentifiers.Javali,
                "Javali",
                "Boar",
                "Javali",
                "Lines 3767-3771",
                5,
                null,
                [
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.AbilityBonus, "+2 Briga", "Line 3770"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.TraitBonus, "+1 permanent Vigor", "Line 3770"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.BanOrRestriction, "prohibited hunting or consuming boar meat", "Line 3771")
                ]);
        }

        static WerewolfTotemCatalogEntry BuildRato()
        {
            return new WerewolfTotemCatalogEntry(
                WerewolfTotemIdentifiers.Rato,
                "Rato",
                "Rat",
                "Rato",
                "Lines 3772-3776",
                5,
                "tribe.bone-gnawers",
                [
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.ResourceGrant, "+5 Willpower per story", "Line 3775"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.DifficultyModifier, "-1 difficulty on bite attacks", "Line 3775"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.DifficultyModifier, "-1 difficulty on stealth and silence", "Line 3775"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.BanOrRestriction, "prohibited killing pests", "Line 3776")
                ]);
        }

        static WerewolfTotemCatalogEntry BuildUrso()
        {
            return new WerewolfTotemCatalogEntry(
                WerewolfTotemIdentifiers.Urso,
                "Urso",
                "Bear",
                "Urso",
                "Lines 3777-3781",
                5,
                null,
                [
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.TraitBonus, "+1 permanent Força", "Line 3780"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.AbilityBonus, "+3 Medicina", "Line 3780"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.GiftGrant, "daily use of Toque da Mãe", "Line 3780"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.SpiritCapability, "hibernate up to 3 months", "Line 3780"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.DifficultyModifier, "-5 temporary Honra", "Line 3780"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.BanOrRestriction, "no formal restriction, but costs respect of other Garou", "Line 3781")
                ]);
        }

        static WerewolfTotemCatalogEntry BuildWendigo()
        {
            return new WerewolfTotemCatalogEntry(
                WerewolfTotemIdentifiers.Wendigo,
                "Wendigo",
                "Wendigo",
                "Wendigo",
                "Lines 3782-3786",
                7,
                "tribe.wendigo",
                [
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.ResourceGrant, "+5 Fúria per story (regardless of actual value)", "Line 3785"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.ResourceGrant, "+2 Glória per story", "Line 3785"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.BanOrRestriction, "aid animist peoples in need", "Line 3786")
                ]);
        }

        static WerewolfTotemCatalogEntry BuildBarata()
        {
            return new WerewolfTotemCatalogEntry(
                WerewolfTotemIdentifiers.Barata,
                "Barata",
                "Cockroach",
                "Barata",
                "Lines 3788-3792",
                6,
                "tribe.glass-walkers",
                [
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.DifficultyModifier, "-2 difficulty on computer/electricity/science tests", "Line 3791"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.DiceBonus, "+3 for technological Gift activation", "Line 3791"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.SpiritCapability, "enter Umbra to view data in media/cables (1 Gnose success)", "Line 3791"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.BanOrRestriction, "strive not to kill cockroaches", "Line 3792")
                ]);
        }

        static WerewolfTotemCatalogEntry BuildCoruja()
        {
            return new WerewolfTotemCatalogEntry(
                WerewolfTotemIdentifiers.Coruja,
                "Coruja",
                "Owl",
                "Coruja",
                "Lines 3793-3797",
                6,
                "tribe.silent-striders",
                [
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.SpiritCapability, "premonition to detect dangers and mystical locations", "Line 3796"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.SpiritCapability, "umbral wings for flight", "Line 3796"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.DifficultyModifier, "-2 difficulty on stealth and silence", "Line 3796"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.DiceBonus, "+3 for air/travel/movement/darkness Gifts", "Line 3796"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.ResourceGrant, "+2 Sabedoria per story", "Line 3796"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.BanOrRestriction, "leave small rodents bound or helpless in the woods", "Line 3797")
                ]);
        }

        static WerewolfTotemCatalogEntry BuildCorvo()
        {
            return new WerewolfTotemCatalogEntry(
                WerewolfTotemIdentifiers.Corvo,
                "Corvo",
                "Crow",
                "Corvo",
                "Lines 3799-3803",
                5,
                null,
                [
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.ResourceGrant, "+1 Sabedoria per member", "Line 3802"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.AbilityBonus, "+3 Sobrevivência", "Line 3802"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.AbilityBonus, "+1 Lábia", "Line 3802"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.AbilityBonus, "+1 Enigmas", "Line 3802"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.BanOrRestriction, "children must not carry money, trust Totem providence", "Line 3803")
                ]);
        }

        static WerewolfTotemCatalogEntry BuildQuimera()
        {
            return new WerewolfTotemCatalogEntry(
                WerewolfTotemIdentifiers.Quimera,
                "Quimera",
                "Chimera",
                "Quimera",
                "Lines 3804-3808",
                7,
                null,
                [
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.AbilityBonus, "+3 Enigmas", "Line 3807"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.TraitBonus, "+1 Percepção", "Line 3807"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.DifficultyModifier, "-2 difficulty on charades/riddles/dreams", "Line 3807"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.SpiritCapability, "Umbra disguises (Gnose test difficulty 7)", "Line 3807"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.ResourceGrant, "+2 Sabedoria per member", "Line 3807"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.BanOrRestriction, "pack must seek enlightenment", "Line 3808")
                ]);
        }

        static WerewolfTotemCatalogEntry BuildUktena()
        {
            return new WerewolfTotemCatalogEntry(
                WerewolfTotemIdentifiers.Uktena,
                "Uktena",
                "Uktena",
                "Uktena",
                "Lines 3809-3813",
                7,
                "tribe.uktena",
                [
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.DiceBonus, "+3 absorption in Umbra", "Line 3812"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.ResourceGrant, "+2 XP per story for mystic knowledges", "Line 3812"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.ResourceGrant, "+2 Sabedoria per member", "Line 3812"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.DifficultyModifier, "+1 difficulty in social tests with other tribe Garou (except Wendigo)", "Line 3812"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.BanOrRestriction, "recover knowledge/objects/places/animals taken by Wyrm servants", "Line 3813")
                ]);
        }

        static WerewolfTotemCatalogEntry BuildUnicornio()
        {
            return new WerewolfTotemCatalogEntry(
                WerewolfTotemIdentifiers.Unicornio,
                "Unicórnio",
                "Unicorn",
                "Unicórnio",
                "Lines 3814-3818",
                7,
                "tribe.children-of-gaia",
                [
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.SpiritCapability, "double Umbra speed", "Line 3817"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.DifficultyModifier, "-2 difficulty on cure and empathy tests", "Line 3817"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.DifficultyModifier, "+2 difficulty to harm non-Wyrm Garou", "Line 3817"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.DiceBonus, "+3 for healing/force/protection Gifts", "Line 3817"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.ResourceGrant, "+3 Sabedoria per member", "Line 3817"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.BanOrRestriction, "aid and protect weak and oppressed (not Wyrm)", "Line 3818")
                ]);
        }

        static WerewolfTotemCatalogEntry BuildCoiote()
        {
            return new WerewolfTotemCatalogEntry(
                WerewolfTotemIdentifiers.Coiote,
                "Coiote",
                "Coyote",
                "Coiote",
                "Lines 3820-3824",
                7,
                null,
                [
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.AbilityBonus, "+3 Furtividade", "Line 3823"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.AbilityBonus, "+3 Manha", "Line 3823"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.AbilityBonus, "+1 Lábia", "Line 3823"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.AbilityBonus, "+1 Sobrevivência", "Line 3823"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.SpiritCapability, "locate children permanently", "Line 3823"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.DifficultyModifier, "-1 all temporary Sabedoria received", "Line 3823"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.BanOrRestriction, "no formal restrictions", "Line 3824")
                ]);
        }

        static WerewolfTotemCatalogEntry BuildCuco()
        {
            return new WerewolfTotemCatalogEntry(
                WerewolfTotemIdentifiers.Cuco,
                "Cuco",
                "Cuckoo",
                "Cuco",
                "Lines 3825-3829",
                6,
                null,
                [
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.TraitBonus, "+1 Manipulação", "Line 3828"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.AbilityBonus, "+2 Lábia", "Line 3828"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.SpiritCapability, "pass unnoticed", "Line 3828"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.SpiritCapability, "Manipulation + Lábia resisted by Perception + Prontidão", "Line 3828"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.DifficultyModifier, "-2 temporary Honra", "Line 3828"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.BanOrRestriction, "opportunism benefiting pack at others' expense", "Line 3829")
                ]);
        }

        static WerewolfTotemCatalogEntry BuildRaposa()
        {
            return new WerewolfTotemCatalogEntry(
                WerewolfTotemIdentifiers.Raposa,
                "Raposa",
                "Fox",
                "Raposa",
                "Lines 3830-3834",
                7,
                null,
                [
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.AbilityBonus, "Furtividade 2", "Line 3833"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.AbilityBonus, "Lábia 2", "Line 3833"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.AbilityBonus, "Manha 2", "Line 3833"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.TraitBonus, "+1 Manipulação", "Line 3833"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.DifficultyModifier, "-1 Honra reduction", "Line 3833"),
                    new WerewolfTotemEffect(WerewolfTotemEffectKind.BanOrRestriction, "prohibited from fox hunts, must aid persecuted foxes", "Line 3834")
                ]);
        }
    }
}
