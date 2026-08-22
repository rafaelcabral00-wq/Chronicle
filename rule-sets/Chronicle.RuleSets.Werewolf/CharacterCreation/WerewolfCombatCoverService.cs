namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfCombatCoverType
{
    None,
    Prone,
    BehindWall,
    HeadOnly
}

public static class WerewolfCombatCoverService
{
    public static int GetCoverDifficultyModifier(WerewolfCombatCoverType coverType)
    {
        return coverType switch
        {
            WerewolfCombatCoverType.None => 0,
            WerewolfCombatCoverType.Prone => 1,
            WerewolfCombatCoverType.BehindWall => 2,
            WerewolfCombatCoverType.HeadOnly => 3,
            _ => 0
        };
    }

    public static int GetReturnFirePenalty(WerewolfCombatCoverType coverType)
    {
        return coverType switch
        {
            WerewolfCombatCoverType.None => 0,
            WerewolfCombatCoverType.Prone => 1,
            WerewolfCombatCoverType.BehindWall => 1,
            WerewolfCombatCoverType.HeadOnly => 1,
            _ => 0
        };
    }

    public static string GetCoverDescription(WerewolfCombatCoverType coverType)
    {
        return coverType switch
        {
            WerewolfCombatCoverType.None => "No cover",
            WerewolfCombatCoverType.Prone => "Prone: +1 difficulty",
            WerewolfCombatCoverType.BehindWall => "Behind wall: +2 difficulty",
            WerewolfCombatCoverType.HeadOnly => "Head exposed: +3 difficulty",
            _ => "Unknown cover"
        };
    }
}
