namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfBackgroundEffectKind
{
    DiceBonus,
    DifficultyModifier,
    AttributeMaximum,
    RenownPenalty,
    ConditionalTest,
    SocialTestBonus
}

public sealed record WerewolfBackgroundEffect(
    WerewolfBackgroundEffectKind Kind,
    string? Target = null,
    int? Value = null,
    string? Condition = null,
    int? TestDifficulty = null,
    int? MinimumSuccesses = null,
    string? Applicability = null,
    string? Notes = null);
