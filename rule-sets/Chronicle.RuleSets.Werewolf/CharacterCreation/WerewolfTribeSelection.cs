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
    TribeOutOfScope
}

public static class WerewolfTribeIdentifiers
{
    public const string GlassWalkers = "glass-walkers";

    public static IReadOnlyList<string> Supported { get; } = [GlassWalkers];
}

public static class WerewolfTribeSelectionService
{
    private static readonly string[] KnownOutOfScopeTribes =
    [
        "black-furies",
        "bone-gnawers",
        "children-of-gaia",
        "fianna",
        "get-of-fenris",
        "red-talons",
        "shadow-lords",
        "silent-striders",
        "silver-fangs",
        "uktena",
        "wendigo"
    ];

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

        if (KnownOutOfScopeTribes.Contains(tribe, StringComparer.Ordinal))
        {
            return Invalid(WerewolfTribeSelectionErrorCode.TribeOutOfScope, "Tribe identifier is cataloged but not declared by the current slice.");
        }

        if (!WerewolfTribeIdentifiers.Supported.Contains(tribe, StringComparer.Ordinal))
        {
            return Invalid(WerewolfTribeSelectionErrorCode.UnknownTribe, "Tribe identifier is not declared by the current slice.");
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
            Backgrounds = CopyNumeric(request.Draft.Backgrounds),
            Gifts = Array.AsReadOnly(request.Draft.Gifts.ToArray()),
            Resources = CopyNumeric(request.Draft.Resources),
            NarrativeFields = CopyOptionalText(request.Draft.NarrativeFields),
            DisabledCapabilities = CopyRequiredText(request.Draft.DisabledCapabilities)
        };

        updated = updated with
        {
            RequiredNextSteps = WerewolfInitialGiftSelectionService.ReconcileInitialGiftNextSteps(updated)
        };

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

    private static ReadOnlyDictionary<string, string?> CopyOptionalText(IReadOnlyDictionary<string, string?> values)
    {
        return new ReadOnlyDictionary<string, string?>(values.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal));
    }

    private static ReadOnlyDictionary<string, string> CopyRequiredText(IReadOnlyDictionary<string, string> values)
    {
        return new ReadOnlyDictionary<string, string>(values.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal));
    }
}
