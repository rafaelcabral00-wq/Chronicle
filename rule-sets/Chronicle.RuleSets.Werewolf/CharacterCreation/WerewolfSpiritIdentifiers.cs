namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfSpiritIdentifiers
{
    public const string Totem = "spirit.category.totem";
    public const string Bane = "spirit.category.bane";
    public const string Naturae = "spirit.category.naturae";
    public const string Incarna = "spirit.category.incarna";
    public const string Celestine = "spirit.category.celestine";
    public const string Jaggling = "spirit.category.jaggling";
    public const string Gaffling = "spirit.category.gaffling";
    public const string Ancestor = "spirit.category.ancestor";

    public static IReadOnlyList<string> Supported { get; } =
    [
        Totem,
        Bane,
        Naturae,
        Incarna,
        Celestine,
        Jaggling,
        Gaffling,
        Ancestor
    ];
}
