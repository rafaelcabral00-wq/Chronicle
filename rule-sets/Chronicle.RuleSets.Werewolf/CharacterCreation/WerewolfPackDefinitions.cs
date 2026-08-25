namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfPackDefinitions
{
    public const string PackTypicalSizeMin = "2";
    public const string PackTypicalSizeMax = "10";
    public const string PackTypicalSizeDescription = "generally formed by 2 to 10 Garou";

    public static IReadOnlyList<string> AlphaChallengeMethods { get; } = ["Confrontação", "O Jogo", "Duelo"];

    public static IReadOnlyDictionary<string, string> AlphaChallengeAvailabilityByContext { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["peace"] = "challenge permitted",
            ["war"] = "challenge forbidden"
        };

    public static IReadOnlyDictionary<string, string> AuguryRoles { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Ahroun"] = "leads in battles and physical combat tactics",
            ["Theurge"] = "leads in Umbra, mystical affairs, and spirit negotiation",
            ["Philodox"] = "mediates internal disputes, conducts judgments, and diplomatic negotiations",
            ["Galliard"] = "takes charge for speeches, inspiring the sept, and reporting deeds in moots",
            ["Ragabash"] = "assumes command when situation requires cunning, subterfuge, espionage, or disinformation"
        };

    public static IReadOnlyDictionary<string, string> LitanyPackRules { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["1"] = "Não Te Acasalarás Com Outro Garou",
            ["2"] = "Combate a Wyrm Onde Ela Estiver",
            ["3"] = "Respeita o Território do Próximo",
            ["4"] = "Aceita Uma Rendição Honrosa",
            ["5"] = "Submete-te aos Garou de Posto Mais Elevado",
            ["6"] = "Oferece o Primeiro Quinhão aos Superiores",
            ["7"] = "Não Provarás da Carne Humana",
            ["8"] = "Respeita Aqueles Inferiores a Ti",
            ["9"] = "Não Erguerás o Véu",
            ["10"] = "Não Serás Um Fardo Para Teu Povo",
            ["11"] = "Não Desafiarás o Líder em Tempos de Guerra",
            ["12"] = "Pode-se Desafiar o Líder em Tempos de Paz",
            ["13"] = "Não Provocarás a Violação de um Caern"
        };

    public static IReadOnlyDictionary<string, string> PackTactics { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Arrancar Pêlos"] = "First member attacks with claws (difficulty 7) to tear armor. Second member attacks same area with +2 difficulty. Minimum 2 members.",
            ["Cerco"] = "Continuous relay pursuit to exhaust prey. Reduces human Willpower per hunter pass. Requires prolonged Athletics tests. Minimum 4 members.",
            ["Ataque Feroz"] = "One member knocks opponent down and pack mass-attacks in Lupine/Hispo form. Survivors must test Força + Esportes (difficulty 4 + 1 per Garou) to stand. Minimum 3 members.",
            ["Osso da Sorte"] = "Multiple Garou grab opponent extremities and pull simultaneously. Initial difficulty 6 (reduced by 1 per excess Garou). Destreza + Briga to grab, Força for lethal damage. Minimum 2 members.",
            ["Escárnio"] = "Verbal and visual provocation to destabilize target. Every 2 successes removes 1 die from target's next action. Cumulative pack effects can zero actions. Minimum 1 member."
        };

    public static IReadOnlyDictionary<string, string> PackTacticsMinimumMembers { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Arrancar Pêlos"] = "2",
            ["Cerco"] = "4",
            ["Ataque Feroz"] = "3",
            ["Osso da Sorte"] = "2",
            ["Escárnio"] = "1"
        };

    public static int CalculateMaxTactics(IReadOnlyList<int> memberGnosisValues)
    {
        if (memberGnosisValues is null || memberGnosisValues.Count == 0)
        {
            return 0;
        }

        return memberGnosisValues.Min();
    }
}
