using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfRaceSelectionRequest(
    WerewolfInitializedCharacterState Draft,
    int ExpectedDraftVersion,
    string RaceId);

public sealed record WerewolfRaceSelectionResult(
    bool Succeeded,
    WerewolfInitializedCharacterState? Draft,
    IReadOnlyList<WerewolfRaceSelectionFinding> Findings);

public sealed record WerewolfRaceSelectionFinding(
    WerewolfRaceSelectionFindingSeverity Severity,
    WerewolfRaceSelectionErrorCode Code,
    string Message);

public enum WerewolfRaceSelectionFindingSeverity
{
    Information,
    Error
}

public enum WerewolfRaceSelectionErrorCode
{
    RaceSelected,
    MissingDraft,
    DraftNotInitialized,
    StaleDraftVersion,
    MissingRace,
    MalformedRace,
    UnknownRace
}

public static class WerewolfRaceIdentifiers
{
    public const string Homid = "homid";
    public const string Metis = "metis";
    public const string Lupus = "lupus";

    public static IReadOnlyList<string> Supported { get; } = [Homid, Lupus, Metis];
}

public static class WerewolfRaceSelectionService
{
    private const string MetisDeformityStep = "select-metis-deformity";

    public static WerewolfRaceSelectionResult SelectRace(WerewolfRaceSelectionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Draft is null)
        {
            return Invalid(WerewolfRaceSelectionErrorCode.MissingDraft, "Race selection requires an initialized draft.");
        }

        if (request.Draft.Status != WerewolfCharacterDraftStatus.Initialized)
        {
            return Invalid(WerewolfRaceSelectionErrorCode.DraftNotInitialized, "Race selection requires an initialized draft.");
        }

        if (request.ExpectedDraftVersion != request.Draft.DraftVersion)
        {
            return Invalid(WerewolfRaceSelectionErrorCode.StaleDraftVersion, "Race selection expected draft version does not match current draft version.");
        }

        if (string.IsNullOrWhiteSpace(request.RaceId))
        {
            return Invalid(WerewolfRaceSelectionErrorCode.MissingRace, "Race selection requires a race identifier.");
        }

        var race = request.RaceId.Trim();
        if (!StringComparer.Ordinal.Equals(race, request.RaceId) || race.Any(char.IsWhiteSpace))
        {
            return Invalid(WerewolfRaceSelectionErrorCode.MalformedRace, "Race identifier must be canonical and whitespace-free.");
        }

        if (!WerewolfRaceIdentifiers.Supported.Contains(race, StringComparer.Ordinal))
        {
            return Invalid(WerewolfRaceSelectionErrorCode.UnknownRace, "Race identifier is not declared by the current slice.");
        }

        var raceChanged = !StringComparer.Ordinal.Equals(request.Draft.Race, race);
        var nextSteps = request.Draft.RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, MetisDeformityStep))
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-race"))
            .ToList();

        if (StringComparer.Ordinal.Equals(race, WerewolfRaceIdentifiers.Metis))
        {
            nextSteps.Add(MetisDeformityStep);
        }

        var updated = request.Draft with
        {
            Race = race,
            MetisDeformity = StringComparer.Ordinal.Equals(race, WerewolfRaceIdentifiers.Metis)
                ? request.Draft.MetisDeformity
                : null,
            RaceGift = raceChanged ? null : request.Draft.RaceGift,
            DraftVersion = request.Draft.DraftVersion + 1,
            RequiredNextSteps = Array.AsReadOnly(nextSteps.Order(StringComparer.Ordinal).ToArray()),
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

        return new WerewolfRaceSelectionResult(
            true,
            updated,
            [new WerewolfRaceSelectionFinding(WerewolfRaceSelectionFindingSeverity.Information, WerewolfRaceSelectionErrorCode.RaceSelected, "Race selected.")]);
    }

    private static WerewolfRaceSelectionResult Invalid(WerewolfRaceSelectionErrorCode code, string message)
    {
        return new WerewolfRaceSelectionResult(
            false,
            null,
            [new WerewolfRaceSelectionFinding(WerewolfRaceSelectionFindingSeverity.Error, code, message)]);
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
