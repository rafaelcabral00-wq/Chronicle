namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfFormCatalog
{
    public static IReadOnlyList<WerewolfFormDefinition> Entries { get; } =
    [
        new WerewolfFormDefinition(
            WerewolfFormIdentifiers.Homid,
            "character.form.homid.display-name",
            "Homid",
            "Hominídea",
            "Lines 3054-3057",
            BirthForm: WerewolfRaceIdentifiers.Homid,
            AttributeModifiers: [],
            MovementModifiers: new Dictionary<string, int>(StringComparer.Ordinal),
            SensoryModifiers: new Dictionary<string, int>(StringComparer.Ordinal),
            Effects: Array.AsReadOnly([
                new WerewolfFormEffect(WerewolfFormEffectKind.SpeechLimitation, Notes: "Full human speech"),
                new WerewolfFormEffect(WerewolfFormEffectKind.ManipulationLimitation, Notes: "Full manual dexterity"),
                new WerewolfFormEffect(WerewolfFormEffectKind.SocialLimitation, Notes: "Triggers Delirium in humans"),
                new WerewolfFormEffect(WerewolfFormEffectKind.RegenerationModifier, Notes: "Standard regeneration; can absorb lethal/aggravated vs difficulty 6"),
                new WerewolfFormEffect(WerewolfFormEffectKind.SoakModifier, Notes: "Silver damage only absorbable for homid birth identity")
            ])),
        new WerewolfFormDefinition(
            WerewolfFormIdentifiers.Glabro,
            "character.form.glabro.display-name",
            "Glabro",
            "Glabro",
            "Lines 3058-3061",
            BirthForm: null,
            AttributeModifiers: Array.AsReadOnly([
                new WerewolfFormAttributeModifier(WerewolfAttributeIdentifiers.Strength, 2, IsAbsolute: false),
                new WerewolfFormAttributeModifier(WerewolfAttributeIdentifiers.Stamina, 2, IsAbsolute: false),
                new WerewolfFormAttributeModifier(WerewolfAttributeIdentifiers.Manipulation, -2, IsAbsolute: false),
                new WerewolfFormAttributeModifier(WerewolfAttributeIdentifiers.Appearance, -1, IsAbsolute: false)
            ]),
            MovementModifiers: new Dictionary<string, int>(StringComparer.Ordinal),
            SensoryModifiers: new Dictionary<string, int>(StringComparer.Ordinal),
            Effects: Array.AsReadOnly([
                new WerewolfFormEffect(WerewolfFormEffectKind.SpeechLimitation, Notes: "Rough human speech; Garou tongue possible"),
                new WerewolfFormEffect(WerewolfFormEffectKind.ManipulationLimitation, Notes: "Reduced manual dexterity"),
                new WerewolfFormEffect(WerewolfFormEffectKind.SocialLimitation, Notes: "Triggers Delirium in humans")
            ])),
        new WerewolfFormDefinition(
            WerewolfFormIdentifiers.Crinos,
            "character.form.crinos.display-name",
            "Crinos",
            "Crinos",
            "Lines 3062-3065",
            BirthForm: WerewolfRaceIdentifiers.Metis,
            AttributeModifiers: Array.AsReadOnly([
                new WerewolfFormAttributeModifier(WerewolfAttributeIdentifiers.Strength, 4, IsAbsolute: false),
                new WerewolfFormAttributeModifier(WerewolfAttributeIdentifiers.Dexterity, 1, IsAbsolute: false),
                new WerewolfFormAttributeModifier(WerewolfAttributeIdentifiers.Stamina, 3, IsAbsolute: false),
                new WerewolfFormAttributeModifier(WerewolfAttributeIdentifiers.Manipulation, 0, IsAbsolute: true),
                new WerewolfFormAttributeModifier(WerewolfAttributeIdentifiers.Appearance, 0, IsAbsolute: true)
            ]),
            MovementModifiers: new Dictionary<string, int>(StringComparer.Ordinal),
            SensoryModifiers: new Dictionary<string, int>(StringComparer.Ordinal),
            Effects: Array.AsReadOnly([
                new WerewolfFormEffect(WerewolfFormEffectKind.SpeechLimitation, Notes: "One or two word phrases only; Willpower expenditure for more"),
                new WerewolfFormEffect(WerewolfFormEffectKind.ManipulationLimitation, Notes: "No manual dexterity"),
                new WerewolfFormEffect(WerewolfFormEffectKind.NaturalWeapon, Target: "claw", Notes: "Fully developed claws"),
                new WerewolfFormEffect(WerewolfFormEffectKind.NaturalWeapon, Target: "bite", Notes: "Fully developed fangs"),
                new WerewolfFormEffect(WerewolfFormEffectKind.DeliriumTrigger, Notes: "Induces Delirium in humans"),
                new WerewolfFormEffect(WerewolfFormEffectKind.RegenerationModifier, Notes: "Standard regeneration; can absorb lethal/aggravated vs difficulty 6"),
                new WerewolfFormEffect(WerewolfFormEffectKind.SoakModifier, Notes: "Silver damage requires Gifts/fetishes to absorb")
            ])),
        new WerewolfFormDefinition(
            WerewolfFormIdentifiers.Hispo,
            "character.form.hispo.display-name",
            "Hispo",
            "Hispo",
            "Lines 3066-3069",
            BirthForm: null,
            AttributeModifiers: Array.AsReadOnly([
                new WerewolfFormAttributeModifier(WerewolfAttributeIdentifiers.Strength, 3, IsAbsolute: false),
                new WerewolfFormAttributeModifier(WerewolfAttributeIdentifiers.Dexterity, 2, IsAbsolute: false),
                new WerewolfFormAttributeModifier(WerewolfAttributeIdentifiers.Stamina, 3, IsAbsolute: false),
                new WerewolfFormAttributeModifier(WerewolfAttributeIdentifiers.Manipulation, 0, IsAbsolute: true)
            ]),
            MovementModifiers: new Dictionary<string, int>(StringComparer.Ordinal),
            SensoryModifiers: new Dictionary<string, int>(StringComparer.Ordinal),
            Effects: Array.AsReadOnly([
                new WerewolfFormEffect(WerewolfFormEffectKind.ManipulationLimitation, Notes: "No manual dexterity (quadrupedal)"),
                new WerewolfFormEffect(WerewolfFormEffectKind.NaturalWeapon, Target: "bite", Notes: "Massive jaws; bite deals +1 extra damage die"),
                new WerewolfFormEffect(WerewolfFormEffectKind.SpeechLimitation, Notes: "No human speech"),
                new WerewolfFormEffect(WerewolfFormEffectKind.RegenerationModifier, Notes: "Standard regeneration; can absorb lethal/aggravated vs difficulty 6"),
                new WerewolfFormEffect(WerewolfFormEffectKind.SoakModifier, Notes: "Silver damage requires Gifts/fetishes to absorb"),
                new WerewolfFormEffect(WerewolfFormEffectKind.DifficultyModifier, Target: "Perception", Value: -1, Notes: "heightened senses (-1 to Perception difficulty)")
            ])),
        new WerewolfFormDefinition(
            WerewolfFormIdentifiers.Lupus,
            "character.form.lupus.display-name",
            "Lupus",
            "Lupina",
            "Lines 3070-3073",
            BirthForm: WerewolfRaceIdentifiers.Lupus,
            AttributeModifiers: Array.AsReadOnly([
                new WerewolfFormAttributeModifier(WerewolfAttributeIdentifiers.Strength, 1, IsAbsolute: false),
                new WerewolfFormAttributeModifier(WerewolfAttributeIdentifiers.Dexterity, 2, IsAbsolute: false),
                new WerewolfFormAttributeModifier(WerewolfAttributeIdentifiers.Stamina, 2, IsAbsolute: false),
                new WerewolfFormAttributeModifier(WerewolfAttributeIdentifiers.Manipulation, 0, IsAbsolute: true)
            ]),
            MovementModifiers: new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["speed"] = 2
            },
            SensoryModifiers: new Dictionary<string, int>(StringComparer.Ordinal),
            Effects: Array.AsReadOnly([
                new WerewolfFormEffect(WerewolfFormEffectKind.ManipulationLimitation, Notes: "No manual dexterity (quadrupedal)"),
                new WerewolfFormEffect(WerewolfFormEffectKind.NaturalWeapon, Target: "bite", Notes: "Natural wolf bite"),
                new WerewolfFormEffect(WerewolfFormEffectKind.NaturalWeapon, Target: "claw", Notes: "Natural wolf claws"),
                new WerewolfFormEffect(WerewolfFormEffectKind.SpeechLimitation, Notes: "No human speech"),
                new WerewolfFormEffect(WerewolfFormEffectKind.RegenerationModifier, Notes: "Standard regeneration; can absorb lethal/aggravated vs difficulty 6"),
                new WerewolfFormEffect(WerewolfFormEffectKind.SoakModifier, Notes: "Silver damage only absorbable for lupus birth identity"),
                new WerewolfFormEffect(WerewolfFormEffectKind.DifficultyModifier, Target: "Perception", Value: -2, Notes: "reduces all Perception difficulties by 2 points")
            ]))
    ];
}

public sealed record WerewolfFormDefinition(
    string FormId,
    string LocalizationKey,
    string SourceLabelEn,
    string SourceLabelPtBr,
    string SourceLocator,
    string? BirthForm,
    IReadOnlyList<WerewolfFormAttributeModifier> AttributeModifiers,
    IReadOnlyDictionary<string, int> MovementModifiers,
    IReadOnlyDictionary<string, int> SensoryModifiers,
    IReadOnlyList<WerewolfFormEffect> Effects);
