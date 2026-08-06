using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfIdentityNameRequest(
    WerewolfInitializedCharacterState Draft,
    int ExpectedDraftVersion,
    string? IdentityName);

public sealed record WerewolfIdentityNameResult(
    bool Succeeded,
    WerewolfInitializedCharacterState? Draft,
    IReadOnlyList<WerewolfIdentityNameFinding> Findings);

public sealed record WerewolfIdentityNameFinding(
    WerewolfIdentityNameFindingSeverity Severity,
    WerewolfIdentityNameErrorCode Code,
    string Message);

public enum WerewolfIdentityNameFindingSeverity
{
    Information,
    Error
}

public enum WerewolfIdentityNameErrorCode
{
    IdentityNameSet,
    MissingDraft,
    DraftNotInitialized,
    StaleDraftVersion,
    MissingIdentityName,
    IdentityNameWhitespace,
    IdentityNameTooLong
}

public static class WerewolfIdentityNameOperation
{
    public const string SetIdentityNameStep = "set-identity-name";

    public static WerewolfIdentityNameResult SetIdentityName(WerewolfIdentityNameRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Draft is null)
        {
            return Invalid(WerewolfIdentityNameErrorCode.MissingDraft, "Identity name operation requires an initialized draft.");
        }

        if (request.Draft.Status != WerewolfCharacterDraftStatus.Initialized)
        {
            return Invalid(WerewolfIdentityNameErrorCode.DraftNotInitialized, "Identity name operation requires an initialized draft.");
        }

        if (request.ExpectedDraftVersion != request.Draft.DraftVersion)
        {
            return Invalid(WerewolfIdentityNameErrorCode.StaleDraftVersion, "Identity name operation expected draft version does not match current draft version.");
        }

        if (request.IdentityName is null)
        {
            return Invalid(WerewolfIdentityNameErrorCode.MissingIdentityName, "Identity name is required.");
        }

        if (request.IdentityName.Length == 0)
        {
            return Invalid(WerewolfIdentityNameErrorCode.MissingIdentityName, "Identity name is required.");
        }

        var trimmed = request.IdentityName.Trim();
        if (trimmed.Length == 0)
        {
            return Invalid(WerewolfIdentityNameErrorCode.IdentityNameWhitespace, "Identity name cannot be whitespace-only.");
        }

        if (trimmed.Length > 120)
        {
            return Invalid(WerewolfIdentityNameErrorCode.IdentityNameTooLong, "Identity name must not exceed 120 characters.");
        }

        var nextSteps = request.Draft.RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, SetIdentityNameStep))
            .Order(StringComparer.Ordinal)
            .ToArray();

        var updated = request.Draft with
        {
            IdentityName = trimmed,
            DraftVersion = request.Draft.DraftVersion + 1,
            RequiredNextSteps = Array.AsReadOnly(nextSteps),
            Race = request.Draft.Race,
            Auspice = request.Draft.Auspice,
            Tribe = request.Draft.Tribe,
            MetisDeformity = request.Draft.MetisDeformity,
            RaceGift = request.Draft.RaceGift,
            AuspiceGift = request.Draft.AuspiceGift,
            TribeGift = request.Draft.TribeGift,
            AttributePriorityOrder = Array.AsReadOnly(request.Draft.AttributePriorityOrder.ToArray()),
            AttributeBudgets = CopyRequiredNumber(request.Draft.AttributeBudgets),
            AbilityPriorityOrder = Array.AsReadOnly(request.Draft.AbilityPriorityOrder.ToArray()),
            AbilityBudgets = CopyRequiredNumber(request.Draft.AbilityBudgets),
            Attributes = CopyNumeric(request.Draft.Attributes),
            Abilities = CopyNumeric(request.Draft.Abilities),
            Backgrounds = CopyNumeric(request.Draft.Backgrounds),
            Gifts = Array.AsReadOnly(request.Draft.Gifts.ToArray()),
            Resources = CopyNumeric(request.Draft.Resources),
            Renown = CopyNumeric(request.Draft.Renown),
            Rank = request.Draft.Rank,
            RankValue = request.Draft.RankValue,
            NarrativeFields = CopyOptionalText(request.Draft.NarrativeFields),
            DisabledCapabilities = CopyRequiredText(request.Draft.DisabledCapabilities)
        };

        return new WerewolfIdentityNameResult(
            true,
            updated,
            [new WerewolfIdentityNameFinding(WerewolfIdentityNameFindingSeverity.Information, WerewolfIdentityNameErrorCode.IdentityNameSet, "Identity name set.")]);
    }

    public static WerewolfIdentityNameValidationResult ValidateIdentityNameRequired(string? identityName)
    {
        if (string.IsNullOrWhiteSpace(identityName))
        {
            return new WerewolfIdentityNameValidationResult(false, "character.completion.identity.name-required", "Identity name is required.");
        }

        var trimmed = identityName.Trim();
        if (trimmed.Length > 120)
        {
            return new WerewolfIdentityNameValidationResult(false, "character.completion.identity.name-required", "Identity name must not exceed 120 characters.");
        }

        return new WerewolfIdentityNameValidationResult(true, "character.completion.identity.name-valid", "Identity name is valid.");
    }
    private static WerewolfIdentityNameResult Invalid(WerewolfIdentityNameErrorCode code, string message)
    {
        return new WerewolfIdentityNameResult(
            false,
            null,
            [new WerewolfIdentityNameFinding(WerewolfIdentityNameFindingSeverity.Error, code, message)]);
    }

    private static ReadOnlyDictionary<string, int?> CopyNumeric(IReadOnlyDictionary<string, int?> values)
    {
        return new ReadOnlyDictionary<string, int?>(values.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal));
    }

    private static ReadOnlyDictionary<string, int> CopyRequiredNumber(IReadOnlyDictionary<string, int> values)
    {
        return new ReadOnlyDictionary<string, int>(values.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal));
    }

    private static ReadOnlyDictionary<string, string?> CopyOptionalText(IReadOnlyDictionary<string, string?> values)
    {
        return new ReadOnlyDictionary<string, string?>(values.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal));
    }

    private static ReadOnlyDictionary<string, string> CopyRequiredText(IReadOnlyDictionary<string, string> values)
    {
        return new ReadOnlyDictionary<string, string>(values.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal));
    }
}

public sealed record WerewolfIdentityNameValidationResult(
    bool IsValid,
    string RuleKey,
    string Message);
