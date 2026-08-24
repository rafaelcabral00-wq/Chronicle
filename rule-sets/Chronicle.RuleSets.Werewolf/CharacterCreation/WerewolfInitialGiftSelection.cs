using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfInitialGiftSelectionRequest(
    WerewolfInitializedCharacterState Draft,
    int ExpectedDraftVersion,
    WerewolfInitialGiftSource GiftSource,
    string GiftId);

public sealed record WerewolfInitialGiftSelectionResult(
    bool Succeeded,
    WerewolfInitializedCharacterState? Draft,
    IReadOnlyList<WerewolfInitialGiftSelectionFinding> Findings);

public sealed record WerewolfInitialGiftSelectionFinding(
    WerewolfInitialGiftSelectionFindingSeverity Severity,
    WerewolfInitialGiftSelectionErrorCode Code,
    string Message);

public enum WerewolfInitialGiftSelectionFindingSeverity
{
    Information,
    Error
}

public enum WerewolfInitialGiftSelectionErrorCode
{
    GiftSelected,
    MissingDraft,
    DraftNotInitialized,
    StaleDraftVersion,
    MissingGift,
    MalformedGift,
    UnknownGift,
    WrongSource,
    WrongLevel,
    OutOfScopeGift,
    MissingRequiredClassification
}

public enum WerewolfInitialGiftSource
{
    Race,
    Auspice,
    Tribe
}

public static class WerewolfInitialGiftIdentifiers
{
}

public static class WerewolfInitialGiftSelectionService
{
    public const string SelectInitialGiftsStep = "select-initial-gifts";
    public const string SelectRaceGiftStep = "select-race-gift";
    public const string SelectAuspiceGiftStep = "select-auspice-gift";
    public const string SelectTribeGiftStep = "select-tribe-gift";

    private static readonly InitialGiftDefinition[] CurrentSliceGifts =
    [
        new(WerewolfGiftIdentifiers.HomidMasterOfFire, WerewolfInitialGiftSource.Race, WerewolfRaceIdentifiers.Homid, 1),
        new(WerewolfGiftIdentifiers.MetisCreateElement, WerewolfInitialGiftSource.Race, WerewolfRaceIdentifiers.Metis, 1),
        new(WerewolfGiftIdentifiers.LupusHareLeap, WerewolfInitialGiftSource.Race, WerewolfRaceIdentifiers.Lupus, 1),
        new(WerewolfGiftIdentifiers.RagabashOpenSeal, WerewolfInitialGiftSource.Auspice, WerewolfAuspiceIdentifiers.Ragabash, 1),
        new(WerewolfGiftIdentifiers.TheurgeSpiritSpeech, WerewolfInitialGiftSource.Auspice, WerewolfAuspiceIdentifiers.Theurge, 1),
        new(WerewolfGiftIdentifiers.PhilodoxResistPain, WerewolfInitialGiftSource.Auspice, WerewolfAuspiceIdentifiers.Philodox, 1),
        new(WerewolfGiftIdentifiers.GalliardBeastSpeech, WerewolfInitialGiftSource.Auspice, WerewolfAuspiceIdentifiers.Galliard, 1),
        new(WerewolfGiftIdentifiers.AhrounFallingTouch, WerewolfInitialGiftSource.Auspice, WerewolfAuspiceIdentifiers.Ahroun, 1),
        new(WerewolfGiftIdentifiers.GlassWalkersControlSimpleMachine, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.GlassWalkers, 1),
        new(WerewolfGiftIdentifiers.GlassWalkersDiagnostics, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.GlassWalkers, 1),
        new(WerewolfGiftIdentifiers.GlassWalkersTrickShot, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.GlassWalkers, 1),
        new(WerewolfGiftIdentifiers.GetOfFenrisRazorClaws, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.GetOfFenris, 1),
        new(WerewolfGiftIdentifiers.GetOfFenrisResistPain, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.GetOfFenris, 1),
        new(WerewolfGiftIdentifiers.GetOfFenrisVisageOfFenris, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.GetOfFenris, 1),
        new(WerewolfGiftIdentifiers.FiannaFaerieLight, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.Fianna, 1),
        new(WerewolfGiftIdentifiers.FiannaPersuasion, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.Fianna, 1),
        new(WerewolfGiftIdentifiers.FiannaResistToxin, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.Fianna, 1),
        new(WerewolfGiftIdentifiers.ChildrenOfGaiaMercy, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.ChildrenOfGaia, 1),
        new(WerewolfGiftIdentifiers.ChildrenOfGaiaMothersTouch, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.ChildrenOfGaia, 1),
        new(WerewolfGiftIdentifiers.BlackFuriesBreathOfTheWyrm, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.BlackFuries, 1),
        new(WerewolfGiftIdentifiers.BlackFuriesHeightenedSenses, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.BlackFuries, 1),
        new(WerewolfGiftIdentifiers.BlackFuriesSenseWyrm, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.BlackFuries, 1),
        new(WerewolfGiftIdentifiers.RedTalonsBeastSpeech, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.RedTalons, 1),
        new(WerewolfGiftIdentifiers.RedTalonsWolfAtTheDoor, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.RedTalons, 1),
        new(WerewolfGiftIdentifiers.RedTalonsScentOfRunningWater, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.RedTalons, 1),
        new(WerewolfGiftIdentifiers.SilentStridersSilence, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.SilentStriders, 1),
        new(WerewolfGiftIdentifiers.SilentStridersSpeedOfThought, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.SilentStriders, 1),
        new(WerewolfGiftIdentifiers.SilverFangsLambentFlame, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.SilverFangs, 1),
        new(WerewolfGiftIdentifiers.SilverFangsFalconsGrasp, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.SilverFangs, 1),
        new(WerewolfGiftIdentifiers.BoneGnawersCooking, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.BoneGnawers, 1),
        new(WerewolfGiftIdentifiers.BoneGnawersStickyFingers, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.BoneGnawers, 1),
        new(WerewolfGiftIdentifiers.ShadowLordsSeizingTheEdge, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.ShadowLords, 1),
        new(WerewolfGiftIdentifiers.ShadowLordsAuraOfConfidence, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.ShadowLords, 1),
        new(WerewolfGiftIdentifiers.ShadowLordsFatalFlaw, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.ShadowLords, 1),
        new(WerewolfGiftIdentifiers.UktenaSpiritSpeech, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.Uktena, 1),
        new(WerewolfGiftIdentifiers.UktenaShroud, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.Uktena, 1),
        new(WerewolfGiftIdentifiers.UktenaSenseMagic, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.Uktena, 1),
        new(WerewolfGiftIdentifiers.WendigoCamouflage, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.Wendigo, 1),
        new(WerewolfGiftIdentifiers.WendigoCallTheBreeze, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.Wendigo, 1)
    ];

    private static readonly string[] KnownOutOfScopeGiftPrefixes =
    [
        "gift.race.",
        "gift.auspice.",
        "gift.tribe."
    ];

    public static WerewolfInitialGiftSelectionResult SelectGift(WerewolfInitialGiftSelectionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Draft is null)
        {
            return Invalid(WerewolfInitialGiftSelectionErrorCode.MissingDraft, "Initial Gift selection requires an initialized draft.");
        }

        if (request.Draft.Status != WerewolfCharacterDraftStatus.Initialized)
        {
            return Invalid(WerewolfInitialGiftSelectionErrorCode.DraftNotInitialized, "Initial Gift selection requires an initialized draft.");
        }

        if (request.ExpectedDraftVersion != request.Draft.DraftVersion)
        {
            return Invalid(WerewolfInitialGiftSelectionErrorCode.StaleDraftVersion, "Initial Gift selection expected draft version does not match current draft version.");
        }

        if (string.IsNullOrWhiteSpace(request.GiftId))
        {
            return Invalid(WerewolfInitialGiftSelectionErrorCode.MissingGift, "Initial Gift selection requires a Gift identifier.");
        }

        var giftId = request.GiftId.Trim();
        if (!StringComparer.Ordinal.Equals(giftId, request.GiftId) || giftId.Any(char.IsWhiteSpace))
        {
            return Invalid(WerewolfInitialGiftSelectionErrorCode.MalformedGift, "Initial Gift identifier must be canonical and whitespace-free.");
        }

        if (giftId.Contains(".level-2", StringComparison.Ordinal) || giftId.Contains(".level-two", StringComparison.Ordinal))
        {
            return Invalid(WerewolfInitialGiftSelectionErrorCode.WrongLevel, "Initial Gift selection accepts only level-one Gifts.");
        }

        var gift = CurrentSliceGifts.FirstOrDefault(candidate => StringComparer.Ordinal.Equals(candidate.GiftId, giftId));
        if (gift is null)
        {
            return Invalid(
                KnownOutOfScopeGiftPrefixes.Any(prefix => giftId.StartsWith(prefix, StringComparison.Ordinal))
                    ? WerewolfInitialGiftSelectionErrorCode.OutOfScopeGift
                    : WerewolfInitialGiftSelectionErrorCode.UnknownGift,
                "Gift identifier is not approved for the declared current slice.");
        }

        if (gift.Source != request.GiftSource)
        {
            return Invalid(WerewolfInitialGiftSelectionErrorCode.WrongSource, "Gift identifier does not belong to the requested initial Gift source.");
        }

        var selectedClassification = SelectedClassification(request.Draft, request.GiftSource);
        if (string.IsNullOrWhiteSpace(selectedClassification))
        {
            return Invalid(WerewolfInitialGiftSelectionErrorCode.MissingRequiredClassification, "Initial Gift selection requires the corresponding classification first.");
        }

        if (!StringComparer.Ordinal.Equals(selectedClassification, gift.OwnerId))
        {
            return Invalid(WerewolfInitialGiftSelectionErrorCode.WrongSource, "Gift identifier is not eligible for the selected classification.");
        }

        var nextSteps = CompleteGiftStep(request.Draft, request.GiftSource);
        var updated = request.Draft with
        {
            RaceGift = request.GiftSource == WerewolfInitialGiftSource.Race ? gift.GiftId : request.Draft.RaceGift,
            AuspiceGift = request.GiftSource == WerewolfInitialGiftSource.Auspice ? gift.GiftId : request.Draft.AuspiceGift,
            TribeGift = request.GiftSource == WerewolfInitialGiftSource.Tribe ? gift.GiftId : request.Draft.TribeGift,
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

        return new WerewolfInitialGiftSelectionResult(
            true,
            updated,
            [new WerewolfInitialGiftSelectionFinding(WerewolfInitialGiftSelectionFindingSeverity.Information, WerewolfInitialGiftSelectionErrorCode.GiftSelected, "Initial Gift selected.")]);
    }

    public static IReadOnlyList<string> ReconcileInitialGiftNextSteps(WerewolfInitializedCharacterState draft)
    {
        var steps = draft.RequiredNextSteps.ToList();
        ReconcileSourceStep(steps, draft.Race, draft.RaceGift, SelectRaceGiftStep);
        ReconcileSourceStep(steps, draft.Auspice, draft.AuspiceGift, SelectAuspiceGiftStep);
        ReconcileSourceStep(steps, draft.Tribe, draft.TribeGift, SelectTribeGiftStep);

        if (draft.RaceGift is not null && draft.AuspiceGift is not null && draft.TribeGift is not null)
        {
            steps.RemoveAll(step => StringComparer.Ordinal.Equals(step, SelectInitialGiftsStep));
        }
        else if (!steps.Contains(SelectInitialGiftsStep, StringComparer.Ordinal))
        {
            steps.Add(SelectInitialGiftsStep);
        }

        return Array.AsReadOnly(steps.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray());
    }

    private static string? SelectedClassification(WerewolfInitializedCharacterState draft, WerewolfInitialGiftSource source)
    {
        return source switch
        {
            WerewolfInitialGiftSource.Race => draft.Race,
            WerewolfInitialGiftSource.Auspice => draft.Auspice,
            WerewolfInitialGiftSource.Tribe => draft.Tribe,
            _ => null
        };
    }

    private static string[] CompleteGiftStep(WerewolfInitializedCharacterState draft, WerewolfInitialGiftSource source)
    {
        var stepToRemove = source switch
        {
            WerewolfInitialGiftSource.Race => SelectRaceGiftStep,
            WerewolfInitialGiftSource.Auspice => SelectAuspiceGiftStep,
            WerewolfInitialGiftSource.Tribe => SelectTribeGiftStep,
            _ => SelectInitialGiftsStep
        };

        var draftWithSelectedGift = source switch
        {
            WerewolfInitialGiftSource.Race => draft with { RaceGift = "selected" },
            WerewolfInitialGiftSource.Auspice => draft with { AuspiceGift = "selected" },
            WerewolfInitialGiftSource.Tribe => draft with { TribeGift = "selected" },
            _ => draft
        };

        return ReconcileInitialGiftNextSteps(draftWithSelectedGift)
            .Where(step => !StringComparer.Ordinal.Equals(step, stepToRemove))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static void ReconcileSourceStep(List<string> steps, string? classification, string? giftId, string step)
    {
        if (string.IsNullOrWhiteSpace(classification) || !string.IsNullOrWhiteSpace(giftId))
        {
            steps.RemoveAll(candidate => StringComparer.Ordinal.Equals(candidate, step));
            return;
        }

        if (!steps.Contains(step, StringComparer.Ordinal))
        {
            steps.Add(step);
        }
    }

    private static WerewolfInitialGiftSelectionResult Invalid(WerewolfInitialGiftSelectionErrorCode code, string message)
    {
        return new WerewolfInitialGiftSelectionResult(
            false,
            null,
            [new WerewolfInitialGiftSelectionFinding(WerewolfInitialGiftSelectionFindingSeverity.Error, code, message)]);
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

    private sealed record InitialGiftDefinition(string GiftId, WerewolfInitialGiftSource Source, string OwnerId, int Level);
}
