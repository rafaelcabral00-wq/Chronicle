namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfCreateCharacterRequest(string RequestId);

public sealed record WerewolfCharacterDraftIdentity(string Value);

public enum WerewolfCharacterDraftStatus
{
    Initialized,
    Completed
}

public sealed record WerewolfInitializedCharacterState(
    WerewolfCharacterDraftIdentity DraftIdentity,
    WerewolfCharacterDraftStatus Status,
    int DraftVersion,
    string? Race,
    string? Auspice,
    string? Tribe,
    string? MetisDeformity,
    string? RaceGift,
    string? AuspiceGift,
    string? TribeGift,
    IReadOnlyList<string> AttributePriorityOrder,
    IReadOnlyDictionary<string, int> AttributeBudgets,
    IReadOnlyList<string> AbilityPriorityOrder,
    IReadOnlyDictionary<string, int> AbilityBudgets,
    IReadOnlyDictionary<string, int?> Attributes,
    IReadOnlyDictionary<string, int?> Abilities,
    IReadOnlyDictionary<string, int?> Backgrounds,
    IReadOnlyList<string> Gifts,
    IReadOnlyDictionary<string, int?> Resources,
    IReadOnlyDictionary<string, int?> Renown,
    string? Rank,
    int? RankValue,
    string? IdentityName,
    IReadOnlyDictionary<string, string?> NarrativeFields,
    IReadOnlyList<string> RequiredNextSteps,
    IReadOnlyDictionary<string, string> DisabledCapabilities,
    IReadOnlyList<WerewolfFreebieLedgerEntry> FreebieLedger,
    int FreebieBudgetTotal,
    int FreebieBudgetSpent);

public sealed record WerewolfCreateCharacterResultPayload(
    bool Succeeded,
    WerewolfInitializedCharacterState? Draft,
    IReadOnlyList<WerewolfCharacterInitializationFinding> Findings);

public sealed record WerewolfCharacterInitializationFinding(
    WerewolfCharacterInitializationFindingSeverity Severity,
    string Code,
    string Message);

public enum WerewolfCharacterInitializationFindingSeverity
{
    Information,
    Error
}

public interface IWerewolfCharacterDraftIdentitySource
{
    WerewolfCharacterDraftIdentity CreateDraftIdentity(WerewolfCreateCharacterRequest request);
}

public sealed record WerewolfFreebieLedgerEntry(
    string ItemId,
    string Category,
    int Cost,
    int ResultingRating,
    string RequestId);
