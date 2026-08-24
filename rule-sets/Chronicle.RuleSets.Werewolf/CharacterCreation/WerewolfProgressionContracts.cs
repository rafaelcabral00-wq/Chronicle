namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfAdvancementCostRequest(
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    string TraitType,
    string? TraitIdentifier,
    int CurrentRating);

public sealed record WerewolfAdvancementCostResult(
    bool Succeeded,
    int? Cost,
    IReadOnlyList<WerewolfProgressionFinding> Findings);

public sealed record WerewolfAdvanceTraitRequest(
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    string RequestId,
    string TraitType,
    string? TraitIdentifier);

public sealed record WerewolfAdvanceTraitResult(
    bool Succeeded,
    WerewolfRuntimeCharacterState? NewState,
    IReadOnlyList<WerewolfProgressionFinding> Findings,
    string? RequestId,
    int? NewRuntimeStateVersion,
    int? XpSpent,
    int? RemainingXp);

public sealed record WerewolfSpecialtyEligibilityRequest(
    string TraitType,
    string TraitIdentifier,
    int CurrentRating);

public sealed record WerewolfSpecialtyEligibilityResult(
    bool Succeeded,
    bool IsEligible,
    IReadOnlyList<WerewolfProgressionFinding> Findings);

public sealed record WerewolfGiftAdvancementRequest(
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    string GiftKey);

public sealed record WerewolfGiftAdvancementResult(
    bool Succeeded,
    int? Cost,
    bool? IsEligible,
    string? IneligibilityReason,
    IReadOnlyList<WerewolfProgressionFinding> Findings);

public sealed record WerewolfProgressionFinding(
    WerewolfProgressionFindingSeverity Severity,
    WerewolfProgressionErrorCode Code,
    string Message);

public enum WerewolfProgressionFindingSeverity
{
    Information,
    Error
}

public enum WerewolfProgressionErrorCode
{
    CostCalculated,
    UnknownTraitType,
    InvalidTraitIdentifier,
    InvalidCurrentRating,
    AdvancementSucceeded,
    InsufficientXp,
    UnknownTrait,
    InvalidTarget,
    BackgroundNotPurchasableWithExperience,
    GiftRankRequirementNotMet,
    GiftAlreadyKnown,
    TotemExperienceCostUnresolved,
    InvalidOrNonPositiveProgressionState,
    StaleRuntimeStateVersion,
    MissingState,
    InvalidPackageBinding,
    CharacterNotCompleted,
    SpecialtyEligible,
    SpecialtyNotEligible,
    GiftEligible,
    GiftIneligible
}
