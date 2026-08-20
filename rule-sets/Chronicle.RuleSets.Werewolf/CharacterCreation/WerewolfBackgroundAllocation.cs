using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfBackgroundAllocationRequest(
    WerewolfInitializedCharacterState Draft,
    int ExpectedDraftVersion,
    IReadOnlyList<WerewolfBackgroundRatingAllocation> Backgrounds);

public sealed record WerewolfBackgroundRatingAllocation(string BackgroundId, int Rating);

public sealed record WerewolfBackgroundAllocationResult(
    bool Succeeded,
    WerewolfInitializedCharacterState? Draft,
    int Spent,
    int Budget,
    int Remaining,
    IReadOnlyList<WerewolfBackgroundAllocationFinding> Findings);

public sealed record WerewolfBackgroundAllocationFinding(
    WerewolfBackgroundAllocationFindingSeverity Severity,
    WerewolfBackgroundAllocationErrorCode Code,
    string Message);

public enum WerewolfBackgroundAllocationFindingSeverity
{
    Information,
    Error
}

public enum WerewolfBackgroundAllocationErrorCode
{
    BackgroundsAllocated,
    MissingDraft,
    DraftNotInitialized,
    StaleDraftVersion,
    MissingTribe,
    MissingAllocation,
    MissingBackground,
    DuplicateBackground,
    UnknownBackground,
    MalformedBackground,
    ValueBelowMinimum,
    ValueAboveMaximum,
    RestrictedBackground,
    IncorrectTotal
}

public static class WerewolfBackgroundIdentifiers
{
    public const string Allies = "character.background.allies";
    public const string Ancestors = "character.background.ancestors";
    public const string Contacts = "character.background.contacts";
    public const string Fetish = "character.background.fetish";
    public const string Kinfolk = "character.background.kinfolk";
    public const string Mentor = "character.background.mentor";
    public const string PureBreed = "character.background.pure-breed";
    public const string Resources = "character.background.resources";
    public const string Rites = "character.background.rites";

    public static IReadOnlyList<string> Supported { get; } =
    [
        Allies,
        Ancestors,
        Contacts,
        Fetish,
        Kinfolk,
        Mentor,
        PureBreed,
        Resources,
        Rites
    ];
}

public static class WerewolfBackgroundAllocationService
{
    public const string AllocateBackgroundsStep = "allocate-backgrounds";
    public const int CreationBudget = 5;
    public const int CreationMinimumRating = 0;
    public const int CreationMaximumRating = 5;

    public static WerewolfBackgroundAllocationResult AllocateBackgrounds(WerewolfBackgroundAllocationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Draft is null)
        {
            return Invalid(WerewolfBackgroundAllocationErrorCode.MissingDraft, "Background allocation requires an initialized draft.");
        }

        if (request.Draft.Status != WerewolfCharacterDraftStatus.Initialized)
        {
            return Invalid(WerewolfBackgroundAllocationErrorCode.DraftNotInitialized, "Background allocation requires an initialized draft.");
        }

        if (request.ExpectedDraftVersion != request.Draft.DraftVersion)
        {
            return Invalid(WerewolfBackgroundAllocationErrorCode.StaleDraftVersion, "Background allocation expected draft version does not match current draft version.");
        }

        if (string.IsNullOrWhiteSpace(request.Draft.Tribe))
        {
            return Invalid(WerewolfBackgroundAllocationErrorCode.MissingTribe, "Background allocation requires Tribe selection so restrictions can be evaluated.");
        }

        if (request.Backgrounds is null || request.Backgrounds.Count == 0)
        {
            return Invalid(WerewolfBackgroundAllocationErrorCode.MissingAllocation, "Background allocation requires a complete Background payload.");
        }

        var findings = new List<WerewolfBackgroundAllocationFinding>();
        var values = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var allocation in request.Backgrounds)
        {
            if (string.IsNullOrWhiteSpace(allocation.BackgroundId))
            {
                findings.Add(Error(WerewolfBackgroundAllocationErrorCode.MissingBackground, "Background allocation includes a missing Background identifier."));
                continue;
            }

            var backgroundId = allocation.BackgroundId.Trim();
            if (!StringComparer.Ordinal.Equals(backgroundId, allocation.BackgroundId) || backgroundId.Any(char.IsWhiteSpace))
            {
                findings.Add(Error(WerewolfBackgroundAllocationErrorCode.MalformedBackground, "Background identifier must be canonical and whitespace-free."));
                continue;
            }

            if (!WerewolfBackgroundIdentifiers.Supported.Contains(backgroundId, StringComparer.Ordinal))
            {
                findings.Add(Error(WerewolfBackgroundAllocationErrorCode.UnknownBackground, "Background identifier is not declared by the current slice."));
                continue;
            }

            if (!values.TryAdd(backgroundId, allocation.Rating))
            {
                findings.Add(Error(WerewolfBackgroundAllocationErrorCode.DuplicateBackground, "Background allocation includes a duplicate Background identifier."));
            }

            if (allocation.Rating < CreationMinimumRating)
            {
                findings.Add(Error(WerewolfBackgroundAllocationErrorCode.ValueBelowMinimum, "Background rating is below the creation minimum."));
            }

            if (allocation.Rating > CreationMaximumRating)
            {
                findings.Add(Error(WerewolfBackgroundAllocationErrorCode.ValueAboveMaximum, "Background rating exceeds the creation maximum."));
            }

            if (IsRestricted(request.Draft.Tribe, backgroundId) && allocation.Rating > 0)
            {
                findings.Add(Error(WerewolfBackgroundAllocationErrorCode.RestrictedBackground, "Selected Tribe prohibits this Background during base creation allocation."));
            }
        }

        foreach (var backgroundId in WerewolfBackgroundIdentifiers.Supported)
        {
            if (!values.ContainsKey(backgroundId))
            {
                findings.Add(Error(WerewolfBackgroundAllocationErrorCode.MissingBackground, "Background allocation must include every current-slice Background exactly once."));
            }
        }

        var spent = values
            .Where(entry => WerewolfBackgroundIdentifiers.Supported.Contains(entry.Key, StringComparer.Ordinal))
            .Sum(entry => entry.Value);
        var remaining = CreationBudget - spent;
        if (spent != CreationBudget)
        {
            findings.Add(Error(WerewolfBackgroundAllocationErrorCode.IncorrectTotal, "Background allocation must spend exactly five base Background points."));
        }

        if (findings.Any(finding => finding.Severity == WerewolfBackgroundAllocationFindingSeverity.Error))
        {
            return new WerewolfBackgroundAllocationResult(false, null, spent, CreationBudget, remaining, findings.OrderBy(finding => finding.Code.ToString(), StringComparer.Ordinal).ThenBy(finding => finding.Message, StringComparer.Ordinal).ToArray());
        }

        var nextSteps = request.Draft.RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, AllocateBackgroundsStep))
            .Order(StringComparer.Ordinal)
            .ToArray();

        var updated = request.Draft with
        {
            Backgrounds = new ReadOnlyDictionary<string, int?>(values.OrderBy(entry => entry.Key, StringComparer.Ordinal).ToDictionary(entry => entry.Key, entry => (int?)entry.Value, StringComparer.Ordinal)),
            DraftVersion = request.Draft.DraftVersion + 1,
            RequiredNextSteps = Array.AsReadOnly(nextSteps),
            AttributePriorityOrder = Array.AsReadOnly(request.Draft.AttributePriorityOrder.ToArray()),
            AttributeBudgets = CopyRequiredNumber(request.Draft.AttributeBudgets),
            AbilityPriorityOrder = Array.AsReadOnly(request.Draft.AbilityPriorityOrder.ToArray()),
            AbilityBudgets = CopyRequiredNumber(request.Draft.AbilityBudgets),
            Attributes = CopyNumeric(request.Draft.Attributes),
            Abilities = CopyNumeric(request.Draft.Abilities),
            Gifts = Array.AsReadOnly(request.Draft.Gifts.ToArray()),
            Resources = CopyNumeric(request.Draft.Resources),
            NarrativeFields = CopyOptionalText(request.Draft.NarrativeFields),
            DisabledCapabilities = CopyRequiredText(request.Draft.DisabledCapabilities)
        };

        return new WerewolfBackgroundAllocationResult(
            true,
            updated,
            spent,
            CreationBudget,
            remaining,
            [new WerewolfBackgroundAllocationFinding(WerewolfBackgroundAllocationFindingSeverity.Information, WerewolfBackgroundAllocationErrorCode.BackgroundsAllocated, "Backgrounds allocated.")]);
    }

    public static bool IsBackgroundAllocationValidForTribe(IReadOnlyDictionary<string, int?> backgrounds, string? tribe)
    {
        if (string.IsNullOrWhiteSpace(tribe) || !IsKnownTribe(tribe))
        {
            return true;
        }

        foreach (var backgroundId in GetRestrictedBackgroundsForTribe(tribe))
        {
            if (backgrounds.TryGetValue(backgroundId, out var rating) && rating.HasValue && rating.Value > 0)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsRestricted(string tribe, string backgroundId)
    {
        return IsKnownTribe(tribe) && GetRestrictedBackgroundsForTribe(tribe).Contains(backgroundId, StringComparer.Ordinal);
    }

    private static string[] GetRestrictedBackgroundsForTribe(string tribe)
    {
        switch (tribe)
        {
            case WerewolfTribeIdentifiers.GlassWalkers:
                return [WerewolfBackgroundIdentifiers.Ancestors, WerewolfBackgroundIdentifiers.Mentor, WerewolfBackgroundIdentifiers.PureBreed];
            case WerewolfTribeIdentifiers.GetOfFenris:
                return [WerewolfBackgroundIdentifiers.Contacts];
            case WerewolfTribeIdentifiers.RedTalons:
                return [WerewolfBackgroundIdentifiers.Allies, WerewolfBackgroundIdentifiers.Contacts, WerewolfBackgroundIdentifiers.Resources];
            case WerewolfTribeIdentifiers.SilentStriders:
                return [WerewolfBackgroundIdentifiers.Ancestors, WerewolfBackgroundIdentifiers.Resources];
            case WerewolfTribeIdentifiers.BoneGnawers:
                return [WerewolfBackgroundIdentifiers.Ancestors, WerewolfBackgroundIdentifiers.PureBreed, WerewolfBackgroundIdentifiers.Resources];
            case WerewolfTribeIdentifiers.ShadowLords:
                return [WerewolfBackgroundIdentifiers.Allies, WerewolfBackgroundIdentifiers.Mentor];
            case WerewolfTribeIdentifiers.Wendigo:
                return [WerewolfBackgroundIdentifiers.Contacts, WerewolfBackgroundIdentifiers.Resources];
            default:
                return [];
        }
    }

    private static bool IsKnownTribe(string tribe)
    {
        return WerewolfTribeIdentifiers.Supported.Contains(tribe, StringComparer.Ordinal);
    }

    private static WerewolfBackgroundAllocationResult Invalid(WerewolfBackgroundAllocationErrorCode code, string message)
    {
        return new WerewolfBackgroundAllocationResult(
            false,
            null,
            0,
            CreationBudget,
            CreationBudget,
            [Error(code, message)]);
    }

    private static WerewolfBackgroundAllocationFinding Error(WerewolfBackgroundAllocationErrorCode code, string message)
    {
        return new WerewolfBackgroundAllocationFinding(WerewolfBackgroundAllocationFindingSeverity.Error, code, message);
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
