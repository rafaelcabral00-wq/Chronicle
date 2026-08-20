namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfMetisDeformityEffectKind
{
    DifficultyModifier,
    AutomaticFailure,
    AttributeMaximum,
    DiceBonus,
    HealthLevelRemoved,
    RenownPenalty,
    ConditionalTest,
    CombatDamage,
    FormRestricted,
    SensoryFailure,
    TrackingPenalty
}

public sealed record WerewolfMetisDeformityEffect(
    WerewolfMetisDeformityEffectKind Kind,
    string? Target = null,
    int? Value = null,
    string? Form = null,
    string? Condition = null,
    string? Consequence = null,
    string? Sense = null,
    int? TestDifficulty = null,
    int? MinimumSuccesses = null,
    string? Notes = null);
