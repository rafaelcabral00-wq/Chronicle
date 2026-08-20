namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfBackgroundEffectIdentifiers
{
    public const string PureBreedSocialBonus = "background.effect.pure-breed.social-bonus";
    public const string AncestorsGuidance = "background.effect.ancestors.guidance";
}

public static class WerewolfBackgroundEffectCatalog
{
    public static IReadOnlyList<WerewolfBackgroundEffect> Entries { get; } =
    [
        new WerewolfBackgroundEffect(
            WerewolfBackgroundEffectKind.SocialTestBonus,
            Target: "social",
            Value: 1,
            Applicability: "Tests involving other Garou",
            Notes: "Source line 1601: each Pure Breed dot grants +1 die to all Social tests or Challenges involving other Garou."),
        new WerewolfBackgroundEffect(
            WerewolfBackgroundEffectKind.DiceBonus,
            Target: "any-ability",
            Value: 1,
            Condition: "Once per session",
            TestDifficulty: 8,
            Applicability: "Scene",
            Notes: "Source line 1551-1560: test Ancestors (Diff 8, or Diff 10 for specific ancestor). Each success adds +1 die to any Ability pool for the scene. Critical failure may cause catatonia or ancestor refusal to leave body.")
    ];
}
