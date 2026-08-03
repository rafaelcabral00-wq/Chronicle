using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfAttributePrioritySelectionRequest(
    WerewolfInitializedCharacterState Draft,
    int ExpectedDraftVersion,
    string PrimaryCategoryId,
    string SecondaryCategoryId,
    string TertiaryCategoryId);

public sealed record WerewolfAttributePrioritySelectionResult(
    bool Succeeded,
    WerewolfInitializedCharacterState? Draft,
    IReadOnlyList<WerewolfAttributePrioritySelectionFinding> Findings);

public sealed record WerewolfAttributePrioritySelectionFinding(
    WerewolfAttributePrioritySelectionFindingSeverity Severity,
    WerewolfAttributePrioritySelectionErrorCode Code,
    string Message);

public enum WerewolfAttributePrioritySelectionFindingSeverity
{
    Information,
    Error
}

public enum WerewolfAttributePrioritySelectionErrorCode
{
    AttributePrioritiesSelected,
    MissingDraft,
    DraftNotInitialized,
    StaleDraftVersion,
    MissingCategory,
    MalformedCategory,
    UnknownCategory,
    DuplicateCategory,
    IncompleteCategorySet
}

public static class WerewolfAttributeCategoryIdentifiers
{
    public const string Physical = "physical";
    public const string Social = "social";
    public const string Mental = "mental";

    public static IReadOnlyList<string> Supported { get; } = [Mental, Physical, Social];
}

public static class WerewolfAttributePriorityIdentifiers
{
    public const string Primary = "primary";
    public const string Secondary = "secondary";
    public const string Tertiary = "tertiary";
}

public static class WerewolfAttributePrioritySelectionService
{
    private const string SelectAttributePrioritiesStep = "select-attribute-priorities";
    private const string AllocateAttributesStep = "allocate-attributes";

    public static WerewolfAttributePrioritySelectionResult SelectPriorities(WerewolfAttributePrioritySelectionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Draft is null)
        {
            return Invalid(WerewolfAttributePrioritySelectionErrorCode.MissingDraft, "Attribute priority selection requires an initialized draft.");
        }

        if (request.Draft.Status != WerewolfCharacterDraftStatus.Initialized)
        {
            return Invalid(WerewolfAttributePrioritySelectionErrorCode.DraftNotInitialized, "Attribute priority selection requires an initialized draft.");
        }

        if (request.ExpectedDraftVersion != request.Draft.DraftVersion)
        {
            return Invalid(WerewolfAttributePrioritySelectionErrorCode.StaleDraftVersion, "Attribute priority selection expected draft version does not match current draft version.");
        }

        var categories = new[] { request.PrimaryCategoryId, request.SecondaryCategoryId, request.TertiaryCategoryId };
        foreach (var category in categories)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return Invalid(WerewolfAttributePrioritySelectionErrorCode.MissingCategory, "Attribute priority selection requires all three category identifiers.");
            }

            var normalized = category.Trim();
            if (!StringComparer.Ordinal.Equals(normalized, category) || normalized.Any(char.IsWhiteSpace))
            {
                return Invalid(WerewolfAttributePrioritySelectionErrorCode.MalformedCategory, "Attribute category identifiers must be canonical and whitespace-free.");
            }

            if (!WerewolfAttributeCategoryIdentifiers.Supported.Contains(normalized, StringComparer.Ordinal))
            {
                return Invalid(WerewolfAttributePrioritySelectionErrorCode.UnknownCategory, "Attribute category identifier is not declared by the current slice.");
            }
        }

        if (categories.Distinct(StringComparer.Ordinal).Count() != categories.Length)
        {
            return Invalid(WerewolfAttributePrioritySelectionErrorCode.DuplicateCategory, "Each Attribute category must be assigned exactly once.");
        }

        if (!categories.Order(StringComparer.Ordinal).SequenceEqual(WerewolfAttributeCategoryIdentifiers.Supported, StringComparer.Ordinal))
        {
            return Invalid(WerewolfAttributePrioritySelectionErrorCode.IncompleteCategorySet, "Attribute priorities must assign physical, social, and mental exactly once.");
        }

        var budgets = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [request.PrimaryCategoryId] = 7,
            [request.SecondaryCategoryId] = 5,
            [request.TertiaryCategoryId] = 3
        };

        var nextSteps = request.Draft.RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, SelectAttributePrioritiesStep))
            .Where(step => !StringComparer.Ordinal.Equals(step, AllocateAttributesStep))
            .ToList();

        nextSteps.Add(AllocateAttributesStep);

        var updated = request.Draft with
        {
            AttributePriorityOrder = Array.AsReadOnly(categories.ToArray()),
            AttributeBudgets = new ReadOnlyDictionary<string, int>(budgets),
            DraftVersion = request.Draft.DraftVersion + 1,
            RequiredNextSteps = Array.AsReadOnly(nextSteps.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray()),
            Attributes = ResetAttributes(request.Draft.Attributes),
            Abilities = CopyNumeric(request.Draft.Abilities),
            Backgrounds = CopyNumeric(request.Draft.Backgrounds),
            Gifts = Array.AsReadOnly(request.Draft.Gifts.ToArray()),
            Resources = CopyNumeric(request.Draft.Resources),
            NarrativeFields = CopyOptionalText(request.Draft.NarrativeFields),
            DisabledCapabilities = CopyRequiredText(request.Draft.DisabledCapabilities)
        };

        return new WerewolfAttributePrioritySelectionResult(
            true,
            updated,
            [new WerewolfAttributePrioritySelectionFinding(WerewolfAttributePrioritySelectionFindingSeverity.Information, WerewolfAttributePrioritySelectionErrorCode.AttributePrioritiesSelected, "Attribute priorities selected.")]);
    }

    private static WerewolfAttributePrioritySelectionResult Invalid(WerewolfAttributePrioritySelectionErrorCode code, string message)
    {
        return new WerewolfAttributePrioritySelectionResult(
            false,
            null,
            [new WerewolfAttributePrioritySelectionFinding(WerewolfAttributePrioritySelectionFindingSeverity.Error, code, message)]);
    }

    private static ReadOnlyDictionary<string, int?> CopyNumeric(IReadOnlyDictionary<string, int?> values)
    {
        return new ReadOnlyDictionary<string, int?>(values.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal));
    }

    private static ReadOnlyDictionary<string, int?> ResetAttributes(IReadOnlyDictionary<string, int?> values)
    {
        return new ReadOnlyDictionary<string, int?>(values.ToDictionary(entry => entry.Key, _ => (int?)null, StringComparer.Ordinal));
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
