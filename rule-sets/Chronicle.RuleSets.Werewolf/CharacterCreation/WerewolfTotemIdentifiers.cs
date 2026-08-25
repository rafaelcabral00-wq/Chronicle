namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfTotemIdentifiers
{
    public const string AvoTrovao = "totem.avo-trovao";
    public const string Cervo = "totem.cervo";
    public const string Falcao = "totem.falcao";
    public const string Pegaso = "totem.pegaso";
    public const string Fenris = "totem.fenris";
    public const string Grifo = "totem.grifo";
    public const string Javali = "totem.javali";
    public const string Rato = "totem.rato";
    public const string Urso = "totem.urso";
    public const string Wendigo = "totem.wendigo";
    public const string Barata = "totem.barata";
    public const string Coruja = "totem.coruja";
    public const string Corvo = "totem.corvo";
    public const string Quimera = "totem.quimera";
    public const string Uktena = "totem.uktena";
    public const string Unicornio = "totem.unicornio";
    public const string Coiote = "totem.coiote";
    public const string Cuco = "totem.cuco";
    public const string Raposa = "totem.raposa";

    public static IReadOnlyList<string> Supported { get; } =
    [
        AvoTrovao,
        Cervo,
        Falcao,
        Pegaso,
        Fenris,
        Grifo,
        Javali,
        Rato,
        Urso,
        Wendigo,
        Barata,
        Coruja,
        Corvo,
        Quimera,
        Uktena,
        Unicornio,
        Coiote,
        Cuco,
        Raposa
    ];
}
