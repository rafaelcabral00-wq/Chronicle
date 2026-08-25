using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfTotemDefinitions
{
    public const string TotemBackgroundKey = "character.background.totem";
    public const int InitialTotemPoints = 8;
    public const int DefaultBeneficiaryCount = 1;
    public const int AdditionalBeneficiaryCost = 4;

    public static IReadOnlyList<string> InitialCharms { get; } = ["Sentido de Orientação", "Reformar"];

    public sealed record TotemImprovementEntry(int Cost, string BenefitKey, string DescriptionEn, string DescriptionPtBr, string SourceLocator);

    public sealed record TotemXpConflict(string ConflictId, int LineA, int XpCostA, int LineB, int XpCostB, string Status);

    public static TotemXpConflict A012Conflict { get; } =
        new TotemXpConflict("A-012", 1633, 2, 2820, 3, "Unresolved");

    public static IReadOnlyList<TotemImprovementEntry> ImprovementTable { get; } =
    [
        new TotemImprovementEntry(1, "totem.improvement.3-points-distribution", "+3 points to distribute among Willpower/Rage/Gnosis", "+3 pontos para distribuir em Força de Vontade, Fúria e/ou Gnose", "Line 1639"),
        new TotemImprovementEntry(1, "totem.improvement.direct-communication", "Totem speaks directly without Spirit Communication Gift", "O totem fala diretamente com a matilha sem necessidade do Dom Comunicação com Espíritos", "Line 1640"),
        new TotemImprovementEntry(1, "totem.improvement.location-tracking", "Totem always knows member locations", "O totem sempre sabe onde os membros da matilha estão localizados", "Line 1641"),
        new TotemImprovementEntry(2, "totem.improvement.presence", "Totem physically/spiritually present almost always", "O totem está fisicamente ou espiritualmente presente com a matilha quase o tempo todo", "Line 1642"),
        new TotemImprovementEntry(2, "totem.improvement.prestige", "Totem has great prestige, respected by other spirits", "O totem possui grande prestígio e é respeitado por outros espíritos", "Line 1643"),
        new TotemImprovementEntry(2, "totem.improvement.extra-charm", "Grants 1 additional spiritual charm", "Concede um Encanto espiritual adicional ao Totem", "Line 1644"),
        new TotemImprovementEntry(3, "totem.improvement.telepathic-communication", "Telepathic communication at distance between all members", "Elo místico de comunicação telepática à distância entre todos os membros da matilha", "Line 1645"),
        new TotemImprovementEntry(4, "totem.improvement.additional-member", "+1 additional member can use Totem powers simultaneously per turn", "Permite que +1 membro adicional da matilha use os poderes do totem simultaneamente no mesmo turno", "Line 1646"),
        new TotemImprovementEntry(5, "totem.improvement.wyrm-fear", "Wyrm agents fear Totem (flee or prioritize total attack)", "O totem é temido pelos agentes da Wyrm (causa fuga ou atrai prioridade total de ataque)", "Line 1647")
    ];

    public static int CalculateAdditionalBeneficiaries(int totalTotemPoints)
    {
        if (totalTotemPoints < AdditionalBeneficiaryCost)
        {
            return 0;
        }

        return (totalTotemPoints - 1) / AdditionalBeneficiaryCost;
    }

    public static int CalculateBeneficiaryCount(int totalTotemPoints)
    {
        return DefaultBeneficiaryCount + CalculateAdditionalBeneficiaries(totalTotemPoints);
    }

    public sealed record TotemRiteDefinition(string RiteKey, string Name, int Level, string Description, string SourceLocator);

    public static TotemRiteDefinition? RitualOfTotem { get; } =
        new TotemRiteDefinition("rite.totem.ritual-of-totem", "Ritual de Totem", 3, "Binds a totemic spirit to a group of Garou to form a pack through spiritual hunt in the Umbra.", "Lines 2693-2695");

    public static TotemRiteDefinition? RitualOfContrition { get; } =
        new TotemRiteDefinition("rite.totem.ritual-of-contrition", "Ritual de Contrição", 1, "Formal apology to appease offended spirits or Garou and avoid conflict between septs.", "Lines 2617-2619");

    public sealed record BanirTotemGiftDefinition(string GiftKey, string Name, int Level, string Description, string SourceLocator);

    public static BanirTotemGiftDefinition? BanirTotemGift { get; } =
        new BanirTotemGiftDefinition("gift.totem.banir-totem", "Banir o Totem", 3, "Cuts temporarily the link between a pack and its totem and between pack members.", "Lines 2505-2507");
}
