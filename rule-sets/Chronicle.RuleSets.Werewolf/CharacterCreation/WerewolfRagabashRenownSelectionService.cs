using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfRagabashRenownSelectionRequest(
    WerewolfInitializedCharacterState Draft,
    int ExpectedDraftVersion,
    int Glory,
    int Honor,
    int Wisdom);

public sealed record WerewolfRagabashRenownSelectionResult(
    bool Succeeded,
    WerewolfInitializedCharacterState? Draft,
    IReadOnlyList<WerewolfRagabashRenownSelectionFinding> Findings);

public sealed record WerewolfRagabashRenownSelectionFinding(
    WerewolfRagabashRenownSelectionFindingSeverity Severity,
    WerewolfRagabashRenownSelectionErrorCode Code,
    string Message);

public enum WerewolfRagabashRenownSelectionFindingSeverity
{
    Information,
    Error
}

public enum WerewolfRagabashRenownSelectionErrorCode
{
    RagabashRenownSelected,
    MissingDraft,
    DraftNotInitialized,
    StaleDraftVersion,
    NotRagabash,
    InvalidAllocation,
    TotalMustBeThree,
    NegativeAllocation
}

public static class WerewolfRagabashRenownSelectionService
{
    public const string SelectRagabashRenownStep = "select-ragabash-renown";

    public static WerewolfRagabashRenownSelectionResult SelectRenown(WerewolfRagabashRenownSelectionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Draft is null)
        {
            return Invalid(WerewolfRagabashRenownSelectionErrorCode.MissingDraft, "Ragabash Renown selection requires an initialized draft.");
        }

        if (request.Draft.Status != WerewolfCharacterDraftStatus.Initialized)
        {
            return Invalid(WerewolfRagabashRenownSelectionErrorCode.DraftNotInitialized, "Ragabash Renown selection requires an initialized draft.");
        }

        if (request.ExpectedDraftVersion != request.Draft.DraftVersion)
        {
            return Invalid(WerewolfRagabashRenownSelectionErrorCode.StaleDraftVersion, "Ragabash Renown selection expected draft version does not match current draft version.");
        }

        if (!StringComparer.Ordinal.Equals(request.Draft.Auspice, WerewolfAuspiceIdentifiers.Ragabash))
        {
            return Invalid(WerewolfRagabashRenownSelectionErrorCode.NotRagabash, "Ragabash Renown selection is only valid for Ragabash characters.");
        }

        var glory = request.Glory;
        var honor = request.Honor;
        var wisdom = request.Wisdom;

        if (glory < 0 || honor < 0 || wisdom < 0)
        {
            return Invalid(WerewolfRagabashRenownSelectionErrorCode.NegativeAllocation, "Renown allocation values must be non-negative integers.");
        }

        if (glory + honor + wisdom != 3)
        {
            return Invalid(WerewolfRagabashRenownSelectionErrorCode.TotalMustBeThree, "Ragabash initial Renown must total exactly 3 points.");
        }

        var renown = new Dictionary<string, int?>(StringComparer.Ordinal)
        {
            [WerewolfRenownIdentifiers.GloryPermanent] = glory,
            [WerewolfRenownIdentifiers.GloryCurrent] = glory,
            [WerewolfRenownIdentifiers.HonorPermanent] = honor,
            [WerewolfRenownIdentifiers.HonorCurrent] = honor,
            [WerewolfRenownIdentifiers.WisdomPermanent] = wisdom,
            [WerewolfRenownIdentifiers.WisdomCurrent] = wisdom
        };

        var nextSteps = request.Draft.RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, SelectRagabashRenownStep))
            .Order(StringComparer.Ordinal)
            .ToArray();

        var updated = request.Draft with
        {
            Renown = new ReadOnlyDictionary<string, int?>(renown),
            DraftVersion = request.Draft.DraftVersion + 1,
            RequiredNextSteps = Array.AsReadOnly(nextSteps.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray()),
            Resources = CopyNumeric(request.Draft.Resources),
            Attributes = CopyNumeric(request.Draft.Attributes),
            Abilities = CopyNumeric(request.Draft.Abilities),
            Backgrounds = CopyNumeric(request.Draft.Backgrounds),
            Gifts = Array.AsReadOnly(request.Draft.Gifts.ToArray()),
            NarrativeFields = CopyOptionalText(request.Draft.NarrativeFields),
            DisabledCapabilities = CopyRequiredText(request.Draft.DisabledCapabilities)
        };

        return new WerewolfRagabashRenownSelectionResult(
            true,
            updated,
            [new WerewolfRagabashRenownSelectionFinding(WerewolfRagabashRenownSelectionFindingSeverity.Information, WerewolfRagabashRenownSelectionErrorCode.RagabashRenownSelected, "Ragabash Renown selected.")]);
    }

    private static WerewolfRagabashRenownSelectionResult Invalid(WerewolfRagabashRenownSelectionErrorCode code, string message)
    {
        return new WerewolfRagabashRenownSelectionResult(
            false,
            null,
            [new WerewolfRagabashRenownSelectionFinding(WerewolfRagabashRenownSelectionFindingSeverity.Error, code, message)]);
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
