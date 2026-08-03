using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfAttributeAllocationRequest(
    WerewolfInitializedCharacterState Draft,
    int ExpectedDraftVersion,
    IReadOnlyList<WerewolfAttributeDotAllocation> Allocations);

public sealed record WerewolfAttributeDotAllocation(string AttributeId, int Rating);

public sealed record WerewolfAttributeAllocationResult(
    bool Succeeded,
    WerewolfInitializedCharacterState? Draft,
    IReadOnlyList<WerewolfAttributeAllocationCategoryTotal> CategoryTotals,
    IReadOnlyList<WerewolfAttributeAllocationFinding> Findings);

public sealed record WerewolfAttributeAllocationCategoryTotal(
    string CategoryId,
    int Budget,
    int Spent,
    int Remaining);

public sealed record WerewolfAttributeAllocationFinding(
    WerewolfAttributeAllocationFindingSeverity Severity,
    WerewolfAttributeAllocationErrorCode Code,
    string Message);

public enum WerewolfAttributeAllocationFindingSeverity
{
    Information,
    Error
}

public enum WerewolfAttributeAllocationErrorCode
{
    AttributesAllocated,
    MissingDraft,
    DraftNotInitialized,
    StaleDraftVersion,
    MissingPriorities,
    MissingAllocation,
    MissingAttribute,
    DuplicateAttribute,
    UnknownAttribute,
    MalformedAttribute,
    ValueBelowMinimum,
    ValueAboveMaximum,
    IncorrectCategoryTotal
}

public static class WerewolfAttributeIdentifiers
{
    public const string Strength = "character.attribute.strength";
    public const string Dexterity = "character.attribute.dexterity";
    public const string Stamina = "character.attribute.stamina";
    public const string Charisma = "character.attribute.charisma";
    public const string Manipulation = "character.attribute.manipulation";
    public const string Appearance = "character.attribute.appearance";
    public const string Perception = "character.attribute.perception";
    public const string Intelligence = "character.attribute.intelligence";
    public const string Wits = "character.attribute.wits";

    public static IReadOnlyList<string> Supported { get; } =
    [
        Appearance,
        Charisma,
        Dexterity,
        Intelligence,
        Manipulation,
        Perception,
        Stamina,
        Strength,
        Wits
    ];
}

public static class WerewolfAttributeAllocationService
{
    public const string AllocateAttributesStep = "allocate-attributes";
    private const int CreationBaseRating = 1;
    private const int CreationMaximumRating = 5;

    private static readonly ReadOnlyDictionary<string, string> AttributeCategories = new(
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [WerewolfAttributeIdentifiers.Strength] = WerewolfAttributeCategoryIdentifiers.Physical,
            [WerewolfAttributeIdentifiers.Dexterity] = WerewolfAttributeCategoryIdentifiers.Physical,
            [WerewolfAttributeIdentifiers.Stamina] = WerewolfAttributeCategoryIdentifiers.Physical,
            [WerewolfAttributeIdentifiers.Charisma] = WerewolfAttributeCategoryIdentifiers.Social,
            [WerewolfAttributeIdentifiers.Manipulation] = WerewolfAttributeCategoryIdentifiers.Social,
            [WerewolfAttributeIdentifiers.Appearance] = WerewolfAttributeCategoryIdentifiers.Social,
            [WerewolfAttributeIdentifiers.Perception] = WerewolfAttributeCategoryIdentifiers.Mental,
            [WerewolfAttributeIdentifiers.Intelligence] = WerewolfAttributeCategoryIdentifiers.Mental,
            [WerewolfAttributeIdentifiers.Wits] = WerewolfAttributeCategoryIdentifiers.Mental
        });

    public static WerewolfAttributeAllocationResult AllocateAttributes(WerewolfAttributeAllocationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Draft is null)
        {
            return Invalid(WerewolfAttributeAllocationErrorCode.MissingDraft, "Attribute allocation requires an initialized draft.");
        }

        if (request.Draft.Status != WerewolfCharacterDraftStatus.Initialized)
        {
            return Invalid(WerewolfAttributeAllocationErrorCode.DraftNotInitialized, "Attribute allocation requires an initialized draft.");
        }

        if (request.ExpectedDraftVersion != request.Draft.DraftVersion)
        {
            return Invalid(WerewolfAttributeAllocationErrorCode.StaleDraftVersion, "Attribute allocation expected draft version does not match current draft version.");
        }

        if (request.Draft.AttributePriorityOrder.Count != 3 || request.Draft.AttributeBudgets.Count != 3)
        {
            return Invalid(WerewolfAttributeAllocationErrorCode.MissingPriorities, "Attribute allocation requires Attribute priorities to be selected first.");
        }

        if (request.Allocations.Count == 0)
        {
            return Invalid(WerewolfAttributeAllocationErrorCode.MissingAllocation, "Attribute allocation requires a complete Attribute payload.");
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var values = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var allocation in request.Allocations)
        {
            if (string.IsNullOrWhiteSpace(allocation.AttributeId))
            {
                return Invalid(WerewolfAttributeAllocationErrorCode.MissingAttribute, "Attribute allocation includes a missing Attribute identifier.");
            }

            var attributeId = allocation.AttributeId.Trim();
            if (!StringComparer.Ordinal.Equals(attributeId, allocation.AttributeId) || attributeId.Any(char.IsWhiteSpace))
            {
                return Invalid(WerewolfAttributeAllocationErrorCode.MalformedAttribute, "Attribute identifiers must be canonical and whitespace-free.");
            }

            if (!AttributeCategories.ContainsKey(attributeId))
            {
                return Invalid(WerewolfAttributeAllocationErrorCode.UnknownAttribute, "Attribute identifier is not declared by the current slice.");
            }

            if (!seen.Add(attributeId))
            {
                return Invalid(WerewolfAttributeAllocationErrorCode.DuplicateAttribute, "Attribute allocation payload includes a duplicate Attribute identifier.");
            }

            if (allocation.Rating < CreationBaseRating)
            {
                return Invalid(WerewolfAttributeAllocationErrorCode.ValueBelowMinimum, "Attribute ratings must include the authoritative base dot and cannot be below 1.");
            }

            if (allocation.Rating > CreationMaximumRating)
            {
                return Invalid(WerewolfAttributeAllocationErrorCode.ValueAboveMaximum, "Attribute ratings cannot exceed 5 during Character creation.");
            }

            values[attributeId] = allocation.Rating;
        }

        foreach (var attributeId in WerewolfAttributeIdentifiers.Supported)
        {
            if (!values.ContainsKey(attributeId))
            {
                return Invalid(WerewolfAttributeAllocationErrorCode.MissingAttribute, "Attribute allocation payload must include every current-slice Attribute exactly once.");
            }
        }

        var totals = CalculateTotals(values, request.Draft.AttributeBudgets);
        var invalidTotal = totals.FirstOrDefault(total => total.Remaining != 0);
        if (invalidTotal is not null)
        {
            return new WerewolfAttributeAllocationResult(
                false,
                null,
                totals,
                [new WerewolfAttributeAllocationFinding(WerewolfAttributeAllocationFindingSeverity.Error, WerewolfAttributeAllocationErrorCode.IncorrectCategoryTotal, "Attribute allocation must spend exactly the selected category budgets.")]);
        }

        var nextSteps = request.Draft.RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, AllocateAttributesStep))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        var updated = request.Draft with
        {
            Attributes = new ReadOnlyDictionary<string, int?>(values.ToDictionary(entry => entry.Key, entry => (int?)entry.Value, StringComparer.Ordinal)),
            DraftVersion = request.Draft.DraftVersion + 1,
            RequiredNextSteps = Array.AsReadOnly(nextSteps),
            Abilities = CopyNumeric(request.Draft.Abilities),
            Backgrounds = CopyNumeric(request.Draft.Backgrounds),
            Gifts = Array.AsReadOnly(request.Draft.Gifts.ToArray()),
            Resources = CopyNumeric(request.Draft.Resources),
            NarrativeFields = CopyOptionalText(request.Draft.NarrativeFields),
            DisabledCapabilities = CopyRequiredText(request.Draft.DisabledCapabilities)
        };

        return new WerewolfAttributeAllocationResult(
            true,
            updated,
            totals,
            [new WerewolfAttributeAllocationFinding(WerewolfAttributeAllocationFindingSeverity.Information, WerewolfAttributeAllocationErrorCode.AttributesAllocated, "Attributes allocated.")]);
    }

    private static WerewolfAttributeAllocationCategoryTotal[] CalculateTotals(
        IReadOnlyDictionary<string, int> values,
        IReadOnlyDictionary<string, int> budgets)
    {
        return WerewolfAttributeCategoryIdentifiers.Supported
            .Select(category =>
            {
                var budget = budgets.TryGetValue(category, out var configuredBudget) ? configuredBudget : 0;
                var spent = values
                    .Where(entry => StringComparer.Ordinal.Equals(AttributeCategories[entry.Key], category))
                    .Sum(entry => entry.Value - CreationBaseRating);

                return new WerewolfAttributeAllocationCategoryTotal(category, budget, spent, budget - spent);
            })
            .OrderBy(total => total.CategoryId, StringComparer.Ordinal)
            .ToArray();
    }

    private static WerewolfAttributeAllocationResult Invalid(WerewolfAttributeAllocationErrorCode code, string message)
    {
        return new WerewolfAttributeAllocationResult(
            false,
            null,
            [],
            [new WerewolfAttributeAllocationFinding(WerewolfAttributeAllocationFindingSeverity.Error, code, message)]);
    }

    private static ReadOnlyDictionary<string, int?> CopyNumeric(IReadOnlyDictionary<string, int?> values)
    {
        return new ReadOnlyDictionary<string, int?>(values.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal));
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
