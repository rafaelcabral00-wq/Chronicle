namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfExtendedTestDefinition(
    string RequestId,
    int DicePool,
    int Difficulty,
    int RequiredSuccesses);
