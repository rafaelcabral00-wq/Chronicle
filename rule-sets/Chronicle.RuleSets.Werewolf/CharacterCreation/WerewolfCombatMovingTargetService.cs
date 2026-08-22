namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfCombatMovingTargetService
{
    public static int GetMovingTargetDifficultyModifier(bool isMovingAboveWalkSpeed)
    {
        return isMovingAboveWalkSpeed ? 1 : 0;
    }

    public static string GetMovingTargetDescription(bool isMovingAboveWalkSpeed)
    {
        return isMovingAboveWalkSpeed
            ? "Moving target above walking speed: +1 difficulty"
            : "Stationary or walking-speed target: no modifier";
    }
}
