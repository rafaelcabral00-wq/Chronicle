namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfActionResolutionContext(
    string AttributeId,
    string AbilityId,
    string CurrentForm,
    string? MetisDeformity,
    bool IsDaylightWithoutProtection,
    bool IsUnderTension,
    bool IsUsingWitheredLimb,
    string? SenseBeingTested,
    bool IsTracking,
    bool IsVisionBased,
    bool IsBalanceTest,
    IReadOnlyList<string> ActiveConditions,
    bool IsInFrenzy = false,
    int? RagePermanent = null,
    int? WillpowerPermanent = null,
    IReadOnlyList<WerewolfActiveGiftEffect>? ActiveGiftEffects = null,
    string CurrentSceneToken = "");
