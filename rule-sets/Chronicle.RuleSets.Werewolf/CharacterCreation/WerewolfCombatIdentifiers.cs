namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfCombatIdentifiers
{
    public static IReadOnlyList<string> Supported { get; } =
    [
        "brawl",
        "bite",
        "bite-glabro",
        "claw",
        "horns",
        "kick",
        "punch",
        "tackle",
        "disarm",
        "grapple",
        "sweep",
        "melee-weapon",
        "firearm",
        "thrown",
        "bow",
        "evasive-action",
        "incapacitate",
        "iron-mandible",
        "savage-leap",
        "taunt",
        "tear-armor",
        "dogpile",
        "lucky-bone"
    ];

    public const string Brawl = "brawl";
    public const string Bite = "bite";
    public const string BiteGlabro = "bite-glabro";
    public const string Claw = "claw";
    public const string Kick = "kick";
    public const string Punch = "punch";
    public const string Tackle = "tackle";
    public const string Disarm = "disarm";
    public const string Grapple = "grapple";
    public const string Sweep = "sweep";
    public const string MeleeWeapon = "melee-weapon";
    public const string Firearm = "firearm";
    public const string Thrown = "thrown";
    public const string Bow = "bow";
    public const string EvasiveAction = "evasive-action";
    public const string Incapacitate = "incapacitate";
    public const string IronMandible = "iron-mandible";
    public const string SavageLeap = "savage-leap";
    public const string Taunt = "taunt";
    public const string TearArmor = "tear-armor";
    public const string Dogpile = "dogpile";
    public const string LuckyBone = "lucky-bone";
}
