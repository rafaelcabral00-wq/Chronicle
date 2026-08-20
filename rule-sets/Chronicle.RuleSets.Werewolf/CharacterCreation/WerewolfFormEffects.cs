namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfFormEffectKind
{
    AttributeModifier,
    MovementModifier,
    SensoryModifier,
    SpeechLimitation,
    ManipulationLimitation,
    SocialLimitation,
    NaturalWeapon,
    RegenerationModifier,
    SoakModifier,
    DeliriumTrigger,
    DifficultyModifier
}

public sealed record WerewolfFormAttributeModifier(string AttributeId, int Value, bool IsAbsolute);

public sealed record WerewolfFormEffect(
    WerewolfFormEffectKind Kind,
    string? Target = null,
    int? Value = null,
    string? Form = null,
    string? Condition = null,
    string? Consequence = null,
    string? Sense = null,
    int? TestDifficulty = null,
    int? MinimumSuccesses = null,
    string? Notes = null);
