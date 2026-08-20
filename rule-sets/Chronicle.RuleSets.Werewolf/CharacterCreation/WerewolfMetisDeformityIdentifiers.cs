using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfMetisDeformityIdentifiers
{
    public const string FitsOfMadness = "fits-of-madness";
    public const string Albinism = "albinism";
    public const string Hairless = "hairless";
    public const string Blind = "blind";
    public const string Seizures = "seizures";
    public const string Hunchback = "hunchback";
    public const string Horns = "horns";
    public const string ToughHide = "tough-hide";
    public const string DebilitatingDisease = "debilitating-disease";
    public const string WitheredLimb = "withered-limb";
    public const string Tailless = "tailless";
    public const string NoSenseOfSmell = "no-sense-of-smell";
    public const string WeakImmuneSystem = "weak-immune-system";

    public static IReadOnlyList<string> Supported { get; } =
    [
        FitsOfMadness,
        Albinism,
        Hairless,
        Blind,
        Seizures,
        Hunchback,
        Horns,
        ToughHide,
        DebilitatingDisease,
        WitheredLimb,
        Tailless,
        NoSenseOfSmell,
        WeakImmuneSystem
    ];

    public static IReadOnlyDictionary<string, IReadOnlyList<WerewolfMetisDeformityEffect>> Effects { get; } =
        new Dictionary<string, IReadOnlyList<WerewolfMetisDeformityEffect>>(StringComparer.Ordinal)
        {
            [FitsOfMadness] = Array.AsReadOnly([
                new WerewolfMetisDeformityEffect(
                    WerewolfMetisDeformityEffectKind.ConditionalTest,
                    Target: "Willpower",
                    TestDifficulty: 8,
                    MinimumSuccesses: 3,
                    Condition: "under-tension",
                    Consequence: "temporary-psychotic-episode")
            ]),
            [Albinism] = Array.AsReadOnly([
                new WerewolfMetisDeformityEffect(
                    WerewolfMetisDeformityEffectKind.DifficultyModifier,
                    Target: "Perception",
                    Value: 2,
                    Condition: "daylight-without-protection")
            ]),
            [Hairless] = Array.AsReadOnly([
                new WerewolfMetisDeformityEffect(
                    WerewolfMetisDeformityEffectKind.DifficultyModifier,
                    Target: "Social",
                    Value: 1)
            ]),
            [Blind] = Array.AsReadOnly([
                new WerewolfMetisDeformityEffect(
                    WerewolfMetisDeformityEffectKind.AutomaticFailure,
                    Target: "vision-based-tests")
            ]),
            [Seizures] = Array.AsReadOnly([
                new WerewolfMetisDeformityEffect(
                    WerewolfMetisDeformityEffectKind.ConditionalTest,
                    Target: "Willpower",
                    TestDifficulty: 8,
                    MinimumSuccesses: 3,
                    Condition: "on-critical-failure",
                    Consequence: "incapacitated")
            ]),
            [Hunchback] = Array.AsReadOnly([
                new WerewolfMetisDeformityEffect(
                    WerewolfMetisDeformityEffectKind.DifficultyModifier,
                    Target: "Social",
                    Value: 1),
                new WerewolfMetisDeformityEffect(
                    WerewolfMetisDeformityEffectKind.DifficultyModifier,
                    Target: "Dexterity",
                    Value: 1)
            ]),
            [Horns] = Array.AsReadOnly([
                new WerewolfMetisDeformityEffect(
                    WerewolfMetisDeformityEffectKind.DifficultyModifier,
                    Target: "Social",
                    Value: 1),
                new WerewolfMetisDeformityEffect(
                    WerewolfMetisDeformityEffectKind.CombatDamage,
                    Target: "Strength",
                    Value: 1,
                    Notes: "bashing"),
                new WerewolfMetisDeformityEffect(
                    WerewolfMetisDeformityEffectKind.RenownPenalty,
                    Target: "Glory",
                    Value: -1)
            ]),
            [ToughHide] = Array.AsReadOnly([
                new WerewolfMetisDeformityEffect(
                    WerewolfMetisDeformityEffectKind.AttributeMaximum,
                    Target: "Appearance",
                    Value: 1),
                new WerewolfMetisDeformityEffect(
                    WerewolfMetisDeformityEffectKind.DiceBonus,
                    Target: "Absorption",
                    Value: 1)
            ]),
            [DebilitatingDisease] = Array.AsReadOnly([
                new WerewolfMetisDeformityEffect(
                    WerewolfMetisDeformityEffectKind.DifficultyModifier,
                    Target: "Stamina",
                    Value: 2,
                    Notes: "includes absorption")
            ]),
            [WitheredLimb] = Array.AsReadOnly([
                new WerewolfMetisDeformityEffect(
                    WerewolfMetisDeformityEffectKind.DifficultyModifier,
                    Target: "Dexterity",
                    Value: 2,
                    Condition: "using-withered-limb")
            ]),
            [Tailless] = Array.AsReadOnly([
                new WerewolfMetisDeformityEffect(
                    WerewolfMetisDeformityEffectKind.DifficultyModifier,
                    Target: "Social",
                    Value: 1),
                new WerewolfMetisDeformityEffect(
                    WerewolfMetisDeformityEffectKind.FormRestricted,
                    Target: "Dexterity",
                    Value: 1,
                    Form: "Lupus,Hispo,Crinos",
                    Condition: "balance")
            ]),
            [NoSenseOfSmell] = Array.AsReadOnly([
                new WerewolfMetisDeformityEffect(
                    WerewolfMetisDeformityEffectKind.SensoryFailure,
                    Target: "smell",
                    Sense: "olfactory"),
                new WerewolfMetisDeformityEffect(
                    WerewolfMetisDeformityEffectKind.TrackingPenalty,
                    Target: "PrimalInstinct",
                    Value: 2,
                    Condition: "tracking")
            ]),
            [WeakImmuneSystem] = Array.AsReadOnly([
                new WerewolfMetisDeformityEffect(
                    WerewolfMetisDeformityEffectKind.HealthLevelRemoved,
                    Target: "Escoriado")
            ])
        };
}
