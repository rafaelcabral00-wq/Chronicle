namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfFormIdentifiers
{
    public const string Homid = "character.form.homid";
    public const string Glabro = "character.form.glabro";
    public const string Crinos = "character.form.crinos";
    public const string Hispo = "character.form.hispo";
    public const string Lupus = "character.form.lupus";

    public static IReadOnlyList<string> Supported { get; } =
    [
        Homid,
        Glabro,
        Crinos,
        Hispo,
        Lupus
    ];
}
