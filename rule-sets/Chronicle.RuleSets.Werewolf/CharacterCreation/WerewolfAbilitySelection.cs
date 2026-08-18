using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfAbilityPrioritySelectionRequest(
    WerewolfInitializedCharacterState Draft,
    int ExpectedDraftVersion,
    string PrimaryCategoryId,
    string SecondaryCategoryId,
    string TertiaryCategoryId);

public sealed record WerewolfAbilityPrioritySelectionResult(
    bool Succeeded,
    WerewolfInitializedCharacterState? Draft,
    IReadOnlyList<WerewolfAbilityPrioritySelectionFinding> Findings);

public sealed record WerewolfAbilityPrioritySelectionFinding(
    WerewolfAbilityPrioritySelectionFindingSeverity Severity,
    WerewolfAbilityPrioritySelectionErrorCode Code,
    string Message);

public enum WerewolfAbilityPrioritySelectionFindingSeverity
{
    Information,
    Error
}

public enum WerewolfAbilityPrioritySelectionErrorCode
{
    AbilityPrioritiesSelected,
    MissingDraft,
    DraftNotInitialized,
    StaleDraftVersion,
    MissingCategory,
    MalformedCategory,
    UnknownCategory,
    DuplicateCategory,
    IncompleteCategorySet
}

public sealed record WerewolfAbilityAllocationRequest(
    WerewolfInitializedCharacterState Draft,
    int ExpectedDraftVersion,
    IReadOnlyList<WerewolfAbilityDotAllocation> Allocations);

public sealed record WerewolfAbilityDotAllocation(string AbilityId, int Rating);

public sealed record WerewolfAbilityAllocationResult(
    bool Succeeded,
    WerewolfInitializedCharacterState? Draft,
    IReadOnlyList<WerewolfAbilityAllocationCategoryTotal> CategoryTotals,
    IReadOnlyList<WerewolfAbilityAllocationFinding> Findings);

public sealed record WerewolfAbilityAllocationCategoryTotal(
    string CategoryId,
    int Budget,
    int Spent,
    int Remaining);

public sealed record WerewolfAbilityAllocationFinding(
    WerewolfAbilityAllocationFindingSeverity Severity,
    WerewolfAbilityAllocationErrorCode Code,
    string Message);

public enum WerewolfAbilityAllocationFindingSeverity
{
    Information,
    Error
}

public enum WerewolfAbilityAllocationErrorCode
{
    AbilitiesAllocated,
    MissingDraft,
    DraftNotInitialized,
    StaleDraftVersion,
    MissingPriorities,
    MissingAllocation,
    MissingAbility,
    DuplicateAbility,
    UnknownAbility,
    MalformedAbility,
    ValueBelowMinimum,
    ValueAboveMaximum,
    RestrictedAbility,
    IncorrectCategoryTotal
}

public static class WerewolfAbilityCategoryIdentifiers
{
    public const string Talents = "talents";
    public const string Skills = "skills";
    public const string Knowledges = "knowledges";

    public static IReadOnlyList<string> Supported { get; } = [Knowledges, Skills, Talents];
}

public static class WerewolfAbilityIdentifiers
{
    public const string Alertness = "character.ability.alertness";
    public const string Athletics = "character.ability.athletics";
    public const string Brawl = "character.ability.brawl";
    public const string Dodge = "character.ability.dodge";
    public const string Empathy = "character.ability.empathy";
    public const string Expression = "character.ability.expression";
    public const string Intimidation = "character.ability.intimidation";
    public const string PrimalInstinct = "character.ability.primal-instinct";
    public const string Streetwise = "character.ability.streetwise";
    public const string Subterfuge = "character.ability.subterfuge";
    public const string AnimalEmpathy = "character.ability.animal-empathy";
    public const string Crafts = "character.ability.crafts";
    public const string Drive = "character.ability.drive";
    public const string Etiquette = "character.ability.etiquette";
    public const string Firearms = "character.ability.firearms";
    public const string Leadership = "character.ability.leadership";
    public const string Melee = "character.ability.melee";
    public const string Performance = "character.ability.performance";
    public const string Stealth = "character.ability.stealth";
    public const string Survival = "character.ability.survival";
    public const string Computer = "character.ability.computer";
    public const string Enigmas = "character.ability.enigmas";
    public const string Investigation = "character.ability.investigation";
    public const string Law = "character.ability.law";
    public const string Linguistics = "character.ability.linguistics";
    public const string Medicine = "character.ability.medicine";
    public const string Occult = "character.ability.occult";
    public const string Politics = "character.ability.politics";
    public const string Rituals = "character.ability.rituals";
    public const string Science = "character.ability.science";

    public static IReadOnlyList<string> Supported { get; } =
    [
        Alertness,
        Athletics,
        Brawl,
        Dodge,
        Empathy,
        Expression,
        Intimidation,
        PrimalInstinct,
        Streetwise,
        Subterfuge,
        AnimalEmpathy,
        Crafts,
        Drive,
        Etiquette,
        Firearms,
        Leadership,
        Melee,
        Performance,
        Stealth,
        Survival,
        Computer,
        Enigmas,
        Investigation,
        Law,
        Linguistics,
        Medicine,
        Occult,
        Politics,
        Rituals,
        Science
    ];
}

public static class WerewolfAbilitySelectionService
{
    public const string SelectAbilityPrioritiesStep = "select-ability-priorities";
    public const string AllocateAbilitiesStep = "allocate-abilities";

    private const int CreationBaseRating = 0;
    private const int CreationMaximumRating = 3;

    private static readonly ReadOnlyDictionary<string, string> AbilityCategories = new(
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [WerewolfAbilityIdentifiers.Alertness] = WerewolfAbilityCategoryIdentifiers.Talents,
            [WerewolfAbilityIdentifiers.Athletics] = WerewolfAbilityCategoryIdentifiers.Talents,
            [WerewolfAbilityIdentifiers.Brawl] = WerewolfAbilityCategoryIdentifiers.Talents,
            [WerewolfAbilityIdentifiers.Dodge] = WerewolfAbilityCategoryIdentifiers.Talents,
            [WerewolfAbilityIdentifiers.Empathy] = WerewolfAbilityCategoryIdentifiers.Talents,
            [WerewolfAbilityIdentifiers.Expression] = WerewolfAbilityCategoryIdentifiers.Talents,
            [WerewolfAbilityIdentifiers.Intimidation] = WerewolfAbilityCategoryIdentifiers.Talents,
            [WerewolfAbilityIdentifiers.PrimalInstinct] = WerewolfAbilityCategoryIdentifiers.Talents,
            [WerewolfAbilityIdentifiers.Streetwise] = WerewolfAbilityCategoryIdentifiers.Talents,
            [WerewolfAbilityIdentifiers.Subterfuge] = WerewolfAbilityCategoryIdentifiers.Talents,
            [WerewolfAbilityIdentifiers.AnimalEmpathy] = WerewolfAbilityCategoryIdentifiers.Skills,
            [WerewolfAbilityIdentifiers.Crafts] = WerewolfAbilityCategoryIdentifiers.Skills,
            [WerewolfAbilityIdentifiers.Drive] = WerewolfAbilityCategoryIdentifiers.Skills,
            [WerewolfAbilityIdentifiers.Etiquette] = WerewolfAbilityCategoryIdentifiers.Skills,
            [WerewolfAbilityIdentifiers.Firearms] = WerewolfAbilityCategoryIdentifiers.Skills,
            [WerewolfAbilityIdentifiers.Leadership] = WerewolfAbilityCategoryIdentifiers.Skills,
            [WerewolfAbilityIdentifiers.Melee] = WerewolfAbilityCategoryIdentifiers.Skills,
            [WerewolfAbilityIdentifiers.Performance] = WerewolfAbilityCategoryIdentifiers.Skills,
            [WerewolfAbilityIdentifiers.Stealth] = WerewolfAbilityCategoryIdentifiers.Skills,
            [WerewolfAbilityIdentifiers.Survival] = WerewolfAbilityCategoryIdentifiers.Skills,
            [WerewolfAbilityIdentifiers.Computer] = WerewolfAbilityCategoryIdentifiers.Knowledges,
            [WerewolfAbilityIdentifiers.Enigmas] = WerewolfAbilityCategoryIdentifiers.Knowledges,
            [WerewolfAbilityIdentifiers.Investigation] = WerewolfAbilityCategoryIdentifiers.Knowledges,
            [WerewolfAbilityIdentifiers.Law] = WerewolfAbilityCategoryIdentifiers.Knowledges,
            [WerewolfAbilityIdentifiers.Linguistics] = WerewolfAbilityCategoryIdentifiers.Knowledges,
            [WerewolfAbilityIdentifiers.Medicine] = WerewolfAbilityCategoryIdentifiers.Knowledges,
            [WerewolfAbilityIdentifiers.Occult] = WerewolfAbilityCategoryIdentifiers.Knowledges,
            [WerewolfAbilityIdentifiers.Politics] = WerewolfAbilityCategoryIdentifiers.Knowledges,
            [WerewolfAbilityIdentifiers.Rituals] = WerewolfAbilityCategoryIdentifiers.Knowledges,
            [WerewolfAbilityIdentifiers.Science] = WerewolfAbilityCategoryIdentifiers.Knowledges
        });

    private static readonly string[] LupusBaseRestrictedAbilities =
    [
        WerewolfAbilityIdentifiers.Computer,
        WerewolfAbilityIdentifiers.Crafts,
        WerewolfAbilityIdentifiers.Drive,
        WerewolfAbilityIdentifiers.Etiquette,
        WerewolfAbilityIdentifiers.Firearms,
        WerewolfAbilityIdentifiers.Law,
        WerewolfAbilityIdentifiers.Linguistics,
        WerewolfAbilityIdentifiers.Politics,
        WerewolfAbilityIdentifiers.Science
    ];

    public static WerewolfAbilityPrioritySelectionResult SelectPriorities(WerewolfAbilityPrioritySelectionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Draft is null)
        {
            return InvalidPriority(WerewolfAbilityPrioritySelectionErrorCode.MissingDraft, "Ability priority selection requires an initialized draft.");
        }

        if (request.Draft.Status != WerewolfCharacterDraftStatus.Initialized)
        {
            return InvalidPriority(WerewolfAbilityPrioritySelectionErrorCode.DraftNotInitialized, "Ability priority selection requires an initialized draft.");
        }

        if (request.ExpectedDraftVersion != request.Draft.DraftVersion)
        {
            return InvalidPriority(WerewolfAbilityPrioritySelectionErrorCode.StaleDraftVersion, "Ability priority selection expected draft version does not match current draft version.");
        }

        var categories = new[] { request.PrimaryCategoryId, request.SecondaryCategoryId, request.TertiaryCategoryId };
        foreach (var category in categories)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return InvalidPriority(WerewolfAbilityPrioritySelectionErrorCode.MissingCategory, "Ability priority selection requires all three category identifiers.");
            }

            var normalized = category.Trim();
            if (!StringComparer.Ordinal.Equals(normalized, category) || normalized.Any(char.IsWhiteSpace))
            {
                return InvalidPriority(WerewolfAbilityPrioritySelectionErrorCode.MalformedCategory, "Ability category identifiers must be canonical and whitespace-free.");
            }

            if (!WerewolfAbilityCategoryIdentifiers.Supported.Contains(normalized, StringComparer.Ordinal))
            {
                return InvalidPriority(WerewolfAbilityPrioritySelectionErrorCode.UnknownCategory, "Ability category identifier is not declared by the current slice.");
            }
        }

        if (categories.Distinct(StringComparer.Ordinal).Count() != categories.Length)
        {
            return InvalidPriority(WerewolfAbilityPrioritySelectionErrorCode.DuplicateCategory, "Each Ability category must be assigned exactly once.");
        }

        if (!categories.Order(StringComparer.Ordinal).SequenceEqual(WerewolfAbilityCategoryIdentifiers.Supported, StringComparer.Ordinal))
        {
            return InvalidPriority(WerewolfAbilityPrioritySelectionErrorCode.IncompleteCategorySet, "Ability priorities must assign talents, skills, and knowledges exactly once.");
        }

        var budgets = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [request.PrimaryCategoryId] = 13,
            [request.SecondaryCategoryId] = 9,
            [request.TertiaryCategoryId] = 5
        };

        var nextSteps = request.Draft.RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, SelectAbilityPrioritiesStep))
            .Where(step => !StringComparer.Ordinal.Equals(step, AllocateAbilitiesStep))
            .ToList();
        nextSteps.Add(AllocateAbilitiesStep);

        var updated = request.Draft with
        {
            AbilityPriorityOrder = Array.AsReadOnly(categories.ToArray()),
            AbilityBudgets = new ReadOnlyDictionary<string, int>(budgets),
            Abilities = ResetNumeric(request.Draft.Abilities),
            DraftVersion = request.Draft.DraftVersion + 1,
            RequiredNextSteps = Array.AsReadOnly(nextSteps.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray()),
            Attributes = CopyNumeric(request.Draft.Attributes),
            Backgrounds = CopyNumeric(request.Draft.Backgrounds),
            Gifts = Array.AsReadOnly(request.Draft.Gifts.ToArray()),
            Resources = CopyNumeric(request.Draft.Resources),
            NarrativeFields = CopyOptionalText(request.Draft.NarrativeFields),
            DisabledCapabilities = CopyRequiredText(request.Draft.DisabledCapabilities)
        };

        return new WerewolfAbilityPrioritySelectionResult(
            true,
            updated,
            [new WerewolfAbilityPrioritySelectionFinding(WerewolfAbilityPrioritySelectionFindingSeverity.Information, WerewolfAbilityPrioritySelectionErrorCode.AbilityPrioritiesSelected, "Ability priorities selected.")]);
    }

    public static WerewolfAbilityAllocationResult AllocateAbilities(WerewolfAbilityAllocationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Draft is null)
        {
            return InvalidAllocation(WerewolfAbilityAllocationErrorCode.MissingDraft, "Ability allocation requires an initialized draft.");
        }

        if (request.Draft.Status != WerewolfCharacterDraftStatus.Initialized)
        {
            return InvalidAllocation(WerewolfAbilityAllocationErrorCode.DraftNotInitialized, "Ability allocation requires an initialized draft.");
        }

        if (request.ExpectedDraftVersion != request.Draft.DraftVersion)
        {
            return InvalidAllocation(WerewolfAbilityAllocationErrorCode.StaleDraftVersion, "Ability allocation expected draft version does not match current draft version.");
        }

        if (request.Draft.AbilityPriorityOrder.Count != 3 || request.Draft.AbilityBudgets.Count != 3)
        {
            return InvalidAllocation(WerewolfAbilityAllocationErrorCode.MissingPriorities, "Ability allocation requires Ability priorities to be selected first.");
        }

        if (request.Allocations.Count == 0)
        {
            return InvalidAllocation(WerewolfAbilityAllocationErrorCode.MissingAllocation, "Ability allocation requires a complete Ability payload.");
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var values = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var allocation in request.Allocations)
        {
            if (string.IsNullOrWhiteSpace(allocation.AbilityId))
            {
                return InvalidAllocation(WerewolfAbilityAllocationErrorCode.MissingAbility, "Ability allocation includes a missing Ability identifier.");
            }

            var abilityId = allocation.AbilityId.Trim();
            if (!StringComparer.Ordinal.Equals(abilityId, allocation.AbilityId) || abilityId.Any(char.IsWhiteSpace))
            {
                return InvalidAllocation(WerewolfAbilityAllocationErrorCode.MalformedAbility, "Ability identifiers must be canonical and whitespace-free.");
            }

            if (!AbilityCategories.ContainsKey(abilityId))
            {
                return InvalidAllocation(WerewolfAbilityAllocationErrorCode.UnknownAbility, "Ability identifier is not declared by the current slice.");
            }

            if (!seen.Add(abilityId))
            {
                return InvalidAllocation(WerewolfAbilityAllocationErrorCode.DuplicateAbility, "Ability allocation payload includes a duplicate Ability identifier.");
            }

            if (allocation.Rating < CreationBaseRating)
            {
                return InvalidAllocation(WerewolfAbilityAllocationErrorCode.ValueBelowMinimum, "Ability ratings cannot be below 0.");
            }

            if (allocation.Rating > CreationMaximumRating)
            {
                return InvalidAllocation(WerewolfAbilityAllocationErrorCode.ValueAboveMaximum, "Ability ratings cannot exceed 3 during base allocation.");
            }

            if (StringComparer.Ordinal.Equals(request.Draft.Race, WerewolfRaceIdentifiers.Lupus) &&
                allocation.Rating > 0 &&
                LupusBaseRestrictedAbilities.Contains(abilityId, StringComparer.Ordinal))
            {
                return InvalidAllocation(WerewolfAbilityAllocationErrorCode.RestrictedAbility, "Lupus base allocation cannot assign dots to the restricted Ability.");
            }

            values[abilityId] = allocation.Rating;
        }

        foreach (var abilityId in WerewolfAbilityIdentifiers.Supported)
        {
            if (!values.ContainsKey(abilityId))
            {
                return InvalidAllocation(WerewolfAbilityAllocationErrorCode.MissingAbility, "Ability allocation payload must include every current-slice Ability exactly once.");
            }
        }

        var totals = CalculateTotals(values, request.Draft.AbilityBudgets);
        if (totals.Any(total => total.Remaining != 0))
        {
            return new WerewolfAbilityAllocationResult(
                false,
                null,
                totals,
                [new WerewolfAbilityAllocationFinding(WerewolfAbilityAllocationFindingSeverity.Error, WerewolfAbilityAllocationErrorCode.IncorrectCategoryTotal, "Ability allocation must spend exactly the selected category budgets.")]);
        }

        var nextSteps = request.Draft.RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, AllocateAbilitiesStep))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        var updated = request.Draft with
        {
            Abilities = new ReadOnlyDictionary<string, int?>(values.ToDictionary(entry => entry.Key, entry => (int?)entry.Value, StringComparer.Ordinal)),
            DraftVersion = request.Draft.DraftVersion + 1,
            RequiredNextSteps = Array.AsReadOnly(nextSteps),
            Attributes = CopyNumeric(request.Draft.Attributes),
            Backgrounds = CopyNumeric(request.Draft.Backgrounds),
            Gifts = Array.AsReadOnly(request.Draft.Gifts.ToArray()),
            Resources = CopyNumeric(request.Draft.Resources),
            NarrativeFields = CopyOptionalText(request.Draft.NarrativeFields),
            DisabledCapabilities = CopyRequiredText(request.Draft.DisabledCapabilities)
        };

        return new WerewolfAbilityAllocationResult(
            true,
            updated,
            totals,
            [new WerewolfAbilityAllocationFinding(WerewolfAbilityAllocationFindingSeverity.Information, WerewolfAbilityAllocationErrorCode.AbilitiesAllocated, "Abilities allocated.")]);
    }

    private static WerewolfAbilityAllocationCategoryTotal[] CalculateTotals(
        IReadOnlyDictionary<string, int> values,
        IReadOnlyDictionary<string, int> budgets)
    {
        return WerewolfAbilityCategoryIdentifiers.Supported
            .Select(category =>
            {
                var budget = budgets.TryGetValue(category, out var configuredBudget) ? configuredBudget : 0;
                var spent = values
                    .Where(entry => StringComparer.Ordinal.Equals(AbilityCategories[entry.Key], category))
                    .Sum(entry => entry.Value);

                return new WerewolfAbilityAllocationCategoryTotal(category, budget, spent, budget - spent);
            })
            .OrderBy(total => total.CategoryId, StringComparer.Ordinal)
            .ToArray();
    }

    private static WerewolfAbilityPrioritySelectionResult InvalidPriority(WerewolfAbilityPrioritySelectionErrorCode code, string message)
    {
        return new WerewolfAbilityPrioritySelectionResult(
            false,
            null,
            [new WerewolfAbilityPrioritySelectionFinding(WerewolfAbilityPrioritySelectionFindingSeverity.Error, code, message)]);
    }

    private static WerewolfAbilityAllocationResult InvalidAllocation(WerewolfAbilityAllocationErrorCode code, string message)
    {
        return new WerewolfAbilityAllocationResult(
            false,
            null,
            [],
            [new WerewolfAbilityAllocationFinding(WerewolfAbilityAllocationFindingSeverity.Error, code, message)]);
    }

    private static ReadOnlyDictionary<string, int?> ResetNumeric(IReadOnlyDictionary<string, int?> values)
    {
        return new ReadOnlyDictionary<string, int?>(values.ToDictionary(entry => entry.Key, _ => (int?)null, StringComparer.Ordinal));
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
