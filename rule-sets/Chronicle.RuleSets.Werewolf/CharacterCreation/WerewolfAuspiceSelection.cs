using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfAuspiceSelectionRequest(
    WerewolfInitializedCharacterState Draft,
    int ExpectedDraftVersion,
    string AuspiceId);

public sealed record WerewolfAuspiceSelectionResult(
    bool Succeeded,
    WerewolfInitializedCharacterState? Draft,
    IReadOnlyList<WerewolfAuspiceSelectionFinding> Findings);

public sealed record WerewolfAuspiceSelectionFinding(
    WerewolfAuspiceSelectionFindingSeverity Severity,
    WerewolfAuspiceSelectionErrorCode Code,
    string Message);

public enum WerewolfAuspiceSelectionFindingSeverity
{
    Information,
    Error
}

public enum WerewolfAuspiceSelectionErrorCode
{
    AuspiceSelected,
    MissingDraft,
    DraftNotInitialized,
    StaleDraftVersion,
    MissingAuspice,
    MalformedAuspice,
    UnknownAuspice
}

public static class WerewolfAuspiceIdentifiers
{
    public const string Ragabash = "ragabash";
    public const string Theurge = "theurge";
    public const string Philodox = "philodox";
    public const string Galliard = "galliard";
    public const string Ahroun = "ahroun";

    public static IReadOnlyList<string> Supported { get; } = [Ahroun, Galliard, Philodox, Ragabash, Theurge];
}

public static class WerewolfAuspiceSelectionService
{
    public static WerewolfAuspiceSelectionResult SelectAuspice(WerewolfAuspiceSelectionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Draft is null)
        {
            return Invalid(WerewolfAuspiceSelectionErrorCode.MissingDraft, "Auspice selection requires an initialized draft.");
        }

        if (request.Draft.Status != WerewolfCharacterDraftStatus.Initialized)
        {
            return Invalid(WerewolfAuspiceSelectionErrorCode.DraftNotInitialized, "Auspice selection requires an initialized draft.");
        }

        if (request.ExpectedDraftVersion != request.Draft.DraftVersion)
        {
            return Invalid(WerewolfAuspiceSelectionErrorCode.StaleDraftVersion, "Auspice selection expected draft version does not match current draft version.");
        }

        if (string.IsNullOrWhiteSpace(request.AuspiceId))
        {
            return Invalid(WerewolfAuspiceSelectionErrorCode.MissingAuspice, "Auspice selection requires an auspice identifier.");
        }

        var auspice = request.AuspiceId.Trim();
        if (!StringComparer.Ordinal.Equals(auspice, request.AuspiceId) || auspice.Any(char.IsWhiteSpace))
        {
            return Invalid(WerewolfAuspiceSelectionErrorCode.MalformedAuspice, "Auspice identifier must be canonical and whitespace-free.");
        }

        if (!WerewolfAuspiceIdentifiers.Supported.Contains(auspice, StringComparer.Ordinal))
        {
            return Invalid(WerewolfAuspiceSelectionErrorCode.UnknownAuspice, "Auspice identifier is not declared by the current slice.");
        }

        var nextSteps = request.Draft.RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-auspice"))
            .Order(StringComparer.Ordinal)
            .ToArray();

        var updated = request.Draft with
        {
            Auspice = auspice,
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

        return new WerewolfAuspiceSelectionResult(
            true,
            updated,
            [new WerewolfAuspiceSelectionFinding(WerewolfAuspiceSelectionFindingSeverity.Information, WerewolfAuspiceSelectionErrorCode.AuspiceSelected, "Auspice selected.")]);
    }

    private static WerewolfAuspiceSelectionResult Invalid(WerewolfAuspiceSelectionErrorCode code, string message)
    {
        return new WerewolfAuspiceSelectionResult(
            false,
            null,
            [new WerewolfAuspiceSelectionFinding(WerewolfAuspiceSelectionFindingSeverity.Error, code, message)]);
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
