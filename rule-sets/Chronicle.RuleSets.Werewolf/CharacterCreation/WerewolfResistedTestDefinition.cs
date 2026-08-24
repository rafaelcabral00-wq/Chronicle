namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfResistedTestDefinition(
    string RequestId,
    int SideADicePool,
    int SideADifficulty,
    int SideBDicePool,
    int SideBDifficulty);
