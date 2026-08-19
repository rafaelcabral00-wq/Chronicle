using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfTribeSelectionRequest(
    WerewolfInitializedCharacterState Draft,
    int ExpectedDraftVersion,
    string TribeId);

public sealed record WerewolfTribeSelectionResult(
    bool Succeeded,
    WerewolfInitializedCharacterState? Draft,
    IReadOnlyList<WerewolfTribeSelectionFinding> Findings);

public sealed record WerewolfTribeSelectionFinding(
    WerewolfTribeSelectionFindingSeverity Severity,
    WerewolfTribeSelectionErrorCode Code,
    string Message);

public enum WerewolfTribeSelectionFindingSeverity
{
    Information,
    Error
}

public enum WerewolfTribeSelectionErrorCode
{
    TribeSelected,
    MissingDraft,
    DraftNotInitialized,
    StaleDraftVersion,
    MissingTribe,
    MalformedTribe,
    UnknownTribe,
    TribeOutOfScope,
    RaceBreedIneligible,
    BackgroundMinimumNotMet,
    DependencyUnavailable
}

public static class WerewolfTribeIdentifiers
{
    public const string GlassWalkers = "glass-walkers";
    public const string GetOfFenris = "get-of-fenris";
    public const string Fianna = "fianna";
    public const string ChildrenOfGaia = "children-of-gaia";
    public const string BlackFuries = "black-furies";
    public const string RedTalons = "red-talons";
    public const string SilentStriders = "silent-striders";
    public const string SilverFangs = "silver-fangs";
    public const string BoneGnawers = "bone-gnawers";
    public const string ShadowLords = "shadow-lords";
    public const string Uktena = "uktena";
    public const string Wendigo = "wendigo";

    public static IReadOnlyList<string> Supported { get; } =
    [
        GlassWalkers,
        GetOfFenris,
        Fianna,
        ChildrenOfGaia,
        BlackFuries,
        RedTalons,
        SilentStriders,
        SilverFangs,
        BoneGnawers,
        ShadowLords,
        Uktena,
        Wendigo
    ];
}

public static class WerewolfTribeSelectionService
{
    // Silver Fangs require Pure Breed >= 3 per source line 779.
    // Pure Breed is not in the current executable Background catalog.
    // This requirement is preserved as a known blocker pending a later
    // Background-catalog completion package.
    public static WerewolfTribeSelectionResult SelectTribe(WerewolfTribeSelectionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Draft is null)
        {
            return Invalid(WerewolfTribeSelectionErrorCode.MissingDraft, "Tribe selection requires an initialized draft.");
        }

        if (request.Draft.Status != WerewolfCharacterDraftStatus.Initialized)
        {
            return Invalid(WerewolfTribeSelectionErrorCode.DraftNotInitialized, "Tribe selection requires an initialized draft.");
        }

        if (request.ExpectedDraftVersion != request.Draft.DraftVersion)
        {
            return Invalid(WerewolfTribeSelectionErrorCode.StaleDraftVersion, "Tribe selection expected draft version does not match current draft version.");
        }

        if (string.IsNullOrWhiteSpace(request.TribeId))
        {
            return Invalid(WerewolfTribeSelectionErrorCode.MissingTribe, "Tribe selection requires a tribe identifier.");
        }

        var tribe = request.TribeId.Trim();
        if (!StringComparer.Ordinal.Equals(tribe, request.TribeId) || tribe.Any(char.IsWhiteSpace))
        {
            return Invalid(WerewolfTribeSelectionErrorCode.MalformedTribe, "Tribe identifier must be canonical and whitespace-free.");
        }

        if (!WerewolfTribeIdentifiers.Supported.Contains(tribe, StringComparer.Ordinal))
        {
            return Invalid(WerewolfTribeSelectionErrorCode.UnknownTribe, "Tribe identifier is not declared by the current slice.");
        }

        var eligibility = WerewolfTribeEligibilityService.CheckEligibility(new WerewolfTribeEligibilityRequest(tribe, request.Draft.Race, request.Draft.Backgrounds));
        if (!eligibility.IsEligible)
        {
            var finding = eligibility.Findings.First(finding => finding.Severity == WerewolfTribeEligibilitySeverity.Error);
            var code = finding.Code switch
            {
                WerewolfTribeEligibilityErrorCode.RaceBreedIneligible => WerewolfTribeSelectionErrorCode.RaceBreedIneligible,
                WerewolfTribeEligibilityErrorCode.BackgroundMinimumNotMet => WerewolfTribeSelectionErrorCode.BackgroundMinimumNotMet,
                WerewolfTribeEligibilityErrorCode.DependencyUnavailable => WerewolfTribeSelectionErrorCode.DependencyUnavailable,
                _ => WerewolfTribeSelectionErrorCode.TribeOutOfScope
            };
            return new WerewolfTribeSelectionResult(false, null, [new WerewolfTribeSelectionFinding(WerewolfTribeSelectionFindingSeverity.Error, code, finding.Message)]);
        }

        var tribeChanged = !StringComparer.Ordinal.Equals(request.Draft.Tribe, tribe);
        var nextSteps = request.Draft.RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-tribe"))
            .Order(StringComparer.Ordinal)
            .ToArray();

        var updated = request.Draft with
        {
            Tribe = tribe,
            TribeGift = tribeChanged ? null : request.Draft.TribeGift,
            DraftVersion = request.Draft.DraftVersion + 1,
            RequiredNextSteps = Array.AsReadOnly(nextSteps),
            Attributes = CopyNumeric(request.Draft.Attributes),
            Abilities = CopyNumeric(request.Draft.Abilities),
            Backgrounds = tribeChanged ? ClearInvalidBackgroundsForTribe(request.Draft.Backgrounds, tribe) : CopyNumeric(request.Draft.Backgrounds),
            Gifts = Array.AsReadOnly(request.Draft.Gifts.ToArray()),
            Resources = CopyNumeric(request.Draft.Resources),
            NarrativeFields = CopyOptionalText(request.Draft.NarrativeFields),
            DisabledCapabilities = CopyRequiredText(request.Draft.DisabledCapabilities)
        };

        updated = updated with
        {
            RequiredNextSteps = WerewolfInitialGiftSelectionService.ReconcileInitialGiftNextSteps(updated)
        };

        if (tribeChanged)
        {
            updated = WerewolfResourceRankInitializationService.ClearInitializedValues(updated);
        }

        return new WerewolfTribeSelectionResult(
            true,
            updated,
            [new WerewolfTribeSelectionFinding(WerewolfTribeSelectionFindingSeverity.Information, WerewolfTribeSelectionErrorCode.TribeSelected, "Tribe selected.")]);
    }

    private static WerewolfTribeSelectionResult Invalid(WerewolfTribeSelectionErrorCode code, string message)
    {
        return new WerewolfTribeSelectionResult(
            false,
            null,
            [new WerewolfTribeSelectionFinding(WerewolfTribeSelectionFindingSeverity.Error, code, message)]);
    }

    private static ReadOnlyDictionary<string, int?> CopyNumeric(IReadOnlyDictionary<string, int?> values)
    {
        return new ReadOnlyDictionary<string, int?>(values.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal));
    }

    private static ReadOnlyDictionary<string, int?> ClearInvalidBackgroundsForTribe(IReadOnlyDictionary<string, int?> values, string tribe)
    {
        var copy = values.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
        var restricted = GetRestrictedBackgroundsForTribe(tribe);
        foreach (var backgroundId in restricted)
        {
            if (copy.ContainsKey(backgroundId))
            {
                copy[backgroundId] = null;
            }
        }

        return new ReadOnlyDictionary<string, int?>(copy);
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

    private static IReadOnlyDictionary<string, int?> GetRequiredMinimumBackgroundsForTribe(string tribe)
    {
        switch (tribe)
        {
            case WerewolfTribeIdentifiers.SilverFangs:
                return new Dictionary<string, int?>(StringComparer.Ordinal)
                {
                    [WerewolfBackgroundIdentifiers.PureBreed] = 3
                };
            default:
                return new ReadOnlyDictionary<string, int?>(new Dictionary<string, int?>(StringComparer.Ordinal));
        }
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
