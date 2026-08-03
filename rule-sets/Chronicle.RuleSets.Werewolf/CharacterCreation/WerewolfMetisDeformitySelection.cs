using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfMetisDeformitySelectionRequest(
    WerewolfInitializedCharacterState Draft,
    int ExpectedDraftVersion,
    string DeformityId);

public sealed record WerewolfMetisDeformitySelectionResult(
    bool Succeeded,
    WerewolfInitializedCharacterState? Draft,
    IReadOnlyList<WerewolfMetisDeformitySelectionFinding> Findings);

public sealed record WerewolfMetisDeformitySelectionFinding(
    WerewolfMetisDeformitySelectionFindingSeverity Severity,
    WerewolfMetisDeformitySelectionErrorCode Code,
    string Message);

public enum WerewolfMetisDeformitySelectionFindingSeverity
{
    Information,
    Error
}

public enum WerewolfMetisDeformitySelectionErrorCode
{
    DeformitySelected,
    MissingDraft,
    DraftNotInitialized,
    StaleDraftVersion,
    RaceNotMetis,
    MissingDeformity,
    MalformedDeformity,
    UnknownDeformity,
    DeformityOutOfScope
}

public static class WerewolfMetisDeformityIdentifiers
{
    public const string Horns = "horns";

    public static IReadOnlyList<string> Supported { get; } = [Horns];
}

public static class WerewolfMetisDeformitySelectionService
{
    private const string MetisDeformityStep = "select-metis-deformity";

    private static readonly string[] KnownOutOfScopeDeformities =
    [
        "albinism",
        "blind",
        "debilitating-disease",
        "fits-of-madness",
        "hairless",
        "hunchback",
        "no-sense-of-smell",
        "seizures",
        "tailless",
        "tough-hide",
        "weak-immune-system",
        "withered-limb"
    ];

    public static WerewolfMetisDeformitySelectionResult SelectDeformity(WerewolfMetisDeformitySelectionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Draft is null)
        {
            return Invalid(WerewolfMetisDeformitySelectionErrorCode.MissingDraft, "Metis deformity selection requires an initialized draft.");
        }

        if (request.Draft.Status != WerewolfCharacterDraftStatus.Initialized)
        {
            return Invalid(WerewolfMetisDeformitySelectionErrorCode.DraftNotInitialized, "Metis deformity selection requires an initialized draft.");
        }

        if (request.ExpectedDraftVersion != request.Draft.DraftVersion)
        {
            return Invalid(WerewolfMetisDeformitySelectionErrorCode.StaleDraftVersion, "Metis deformity selection expected draft version does not match current draft version.");
        }

        if (!StringComparer.Ordinal.Equals(request.Draft.Race, WerewolfRaceIdentifiers.Metis))
        {
            return Invalid(WerewolfMetisDeformitySelectionErrorCode.RaceNotMetis, "Metis deformity selection is available only while Race is metis.");
        }

        if (string.IsNullOrWhiteSpace(request.DeformityId))
        {
            return Invalid(WerewolfMetisDeformitySelectionErrorCode.MissingDeformity, "Metis deformity selection requires a deformity identifier.");
        }

        var deformity = request.DeformityId.Trim();
        if (!StringComparer.Ordinal.Equals(deformity, request.DeformityId) || deformity.Any(char.IsWhiteSpace))
        {
            return Invalid(WerewolfMetisDeformitySelectionErrorCode.MalformedDeformity, "Metis deformity identifier must be canonical and whitespace-free.");
        }

        if (KnownOutOfScopeDeformities.Contains(deformity, StringComparer.Ordinal))
        {
            return Invalid(WerewolfMetisDeformitySelectionErrorCode.DeformityOutOfScope, "Metis deformity identifier is cataloged but not declared by the current slice.");
        }

        if (!WerewolfMetisDeformityIdentifiers.Supported.Contains(deformity, StringComparer.Ordinal))
        {
            return Invalid(WerewolfMetisDeformitySelectionErrorCode.UnknownDeformity, "Metis deformity identifier is not declared by the current slice.");
        }

        var nextSteps = request.Draft.RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, MetisDeformityStep))
            .Order(StringComparer.Ordinal)
            .ToArray();

        var updated = request.Draft with
        {
            MetisDeformity = deformity,
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

        return new WerewolfMetisDeformitySelectionResult(
            true,
            updated,
            [new WerewolfMetisDeformitySelectionFinding(WerewolfMetisDeformitySelectionFindingSeverity.Information, WerewolfMetisDeformitySelectionErrorCode.DeformitySelected, "Metis deformity selected.")]);
    }

    private static WerewolfMetisDeformitySelectionResult Invalid(WerewolfMetisDeformitySelectionErrorCode code, string message)
    {
        return new WerewolfMetisDeformitySelectionResult(
            false,
            null,
            [new WerewolfMetisDeformitySelectionFinding(WerewolfMetisDeformitySelectionFindingSeverity.Error, code, message)]);
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
