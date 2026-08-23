namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfSocialChallengeIdentifiers
{
    public const string AtracaoAnimal = "atracao-animal";
    public const string Credibilidade = "credibilidade";
    public const string Defrontacao = "defrontacao";
    public const string Engabelacao = "engabelacao";
    public const string Interrogatorio = "interrogatorio";
    public const string Intimidacao = "intimidacao";
    public const string OratoriaPerformance = "oratoria-performance";
    public const string Seducao = "seducao";

    public static IReadOnlyList<string> Supported { get; } =
    [
        AtracaoAnimal,
        Credibilidade,
        Defrontacao,
        Engabelacao,
        Interrogatorio,
        Intimidacao,
        OratoriaPerformance,
        Seducao
    ];
}
