namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfSocialChallengeDefinition(
    string ChallengeId,
    string DisplayName,
    string AttributeId,
    string AbilityId,
    int BaseDifficulty,
    bool UsesFuryPool,
    int SuccessThreshold,
    string SourceLocator);
