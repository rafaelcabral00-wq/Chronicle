namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfCombatRangeService
{
    public static int GetRangeDifficultyModifier(WerewolfCombatRangeBand rangeBand)
    {
        return rangeBand switch
        {
            WerewolfCombatRangeBand.PointBlank => -2,
            WerewolfCombatRangeBand.Medium => 0,
            WerewolfCombatRangeBand.LongRange => 2,
            _ => 0
        };
    }

    public static string GetRangeDifficultyDescription(WerewolfCombatRangeBand rangeBand)
    {
        return rangeBand switch
        {
            WerewolfCombatRangeBand.PointBlank => "Point-blank (<1.80m): difficulty 4",
            WerewolfCombatRangeBand.Medium => "Medium range: difficulty 6",
            WerewolfCombatRangeBand.LongRange => "Long/double range: difficulty 8",
            _ => "Unknown range"
        };
    }
}
