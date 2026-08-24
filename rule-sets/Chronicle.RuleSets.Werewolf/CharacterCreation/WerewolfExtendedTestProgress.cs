namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfExtendedTestStatus
{
    InProgress,
    Completed,
    Failed,
    Botched
}

public sealed record WerewolfExtendedTestProgress(
    string RequestId,
    int AccumulatedSuccesses,
    int AttemptCount,
    bool IsBotched,
    WerewolfExtendedTestStatus Status);
