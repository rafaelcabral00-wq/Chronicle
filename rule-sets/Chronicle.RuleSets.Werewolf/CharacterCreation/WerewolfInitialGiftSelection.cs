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
    public const string HomidMasterOfFire = "gift.race.homid.master-of-fire";
    public const string MetisCreateElement = "gift.race.metis.create-element";
    public const string LupusHareLeap = "gift.race.lupus.hare-leap";
    public const string RagabashOpenSeal = "gift.auspice.ragabash.open-seal";
    public const string TheurgeSpiritSpeech = "gift.auspice.theurge.spirit-speech";
    public const string PhilodoxResistPain = "gift.auspice.philodox.resist-pain";
    public const string GalliardBeastSpeech = "gift.auspice.galliard.beast-speech";
    public const string AhrounFallingTouch = "gift.auspice.ahroun.falling-touch";
    public const string GlassWalkersControlSimpleMachine = "gift.tribe.glass-walkers.control-simple-machine";
    public const string GlassWalkersDiagnostics = "gift.tribe.glass-walkers.diagnostics";
    public const string GlassWalkersTrickShot = "gift.tribe.glass-walkers.trick-shot";
    public const string GetOfFenrisRazorClaws = "gift.tribe.get-of-fenris.razor-claws";
    public const string GetOfFenrisResistPain = "gift.tribe.get-of-fenris.resist-pain";
    public const string GetOfFenrisVisageOfFenris = "gift.tribe.get-of-fenris.visage-of-fenris";
    public const string FiannaFaerieLight = "gift.tribe.fianna.faerie-light";
    public const string FiannaPersuasion = "gift.tribe.fianna.persuasion";
    public const string FiannaResistToxin = "gift.tribe.fianna.resist-toxin";
    public const string ChildrenOfGaiaMercy = "gift.tribe.children-of-gaia.mercy";
    public const string ChildrenOfGaiaMothersTouch = "gift.tribe.children-of-gaia.mothers-touch";
    public const string BlackFuriesBreathOfTheWyrm = "gift.tribe.black-furies.breath-of-the-wyrm";
    public const string BlackFuriesHeightenedSenses = "gift.tribe.black-furies.heightened-senses";
    public const string BlackFuriesSenseWyrm = "gift.tribe.black-furies.sense-wyrm";
    public const string RedTalonsBeastSpeech = "gift.tribe.red-talons.beast-speech";
    public const string RedTalonsWolfAtTheDoor = "gift.tribe.red-talons.wolf-at-the-door";
    public const string RedTalonsScentOfRunningWater = "gift.tribe.red-talons.scent-of-running-water";
    public const string SilentStridersSilence = "gift.tribe.silent-striders.silence";
    public const string SilentStridersSpeedOfThought = "gift.tribe.silent-striders.speed-of-thought";
    public const string SilverFangsLambentFlame = "gift.tribe.silver-fangs.lambent-flame";
    public const string SilverFangsFalconsGrasp = "gift.tribe.silver-fangs.falcons-grasp";
    public const string BoneGnawersCooking = "gift.tribe.bone-gnawers.cooking";
    public const string BoneGnawersStickyFingers = "gift.tribe.bone-gnawers.sticky-fingers";
    public const string ShadowLordsSeizingTheEdge = "gift.tribe.shadow-lords.seizing-the-edge";
    public const string ShadowLordsAuraOfConfidence = "gift.tribe.shadow-lords.aura-of-confidence";
    public const string ShadowLordsFatalFlaw = "gift.tribe.shadow-lords.fatal-flaw";
    public const string UktenaSpiritSpeech = "gift.tribe.uktena.spirit-speech";
    public const string UktenaShroud = "gift.tribe.uktena.shroud";
    public const string UktenaSenseMagic = "gift.tribe.uktena.sense-magic";
    public const string WendigoCamouflage = "gift.tribe.wendigo.camouflage";
    public const string WendigoCallTheBreeze = "gift.tribe.wendigo.call-the-breeze";

    public static IReadOnlyList<string> Supported { get; } =
    [
        RagabashOpenSeal,
        TheurgeSpiritSpeech,
        PhilodoxResistPain,
        GalliardBeastSpeech,
        AhrounFallingTouch,
        HomidMasterOfFire,
        LupusHareLeap,
        MetisCreateElement,
        GlassWalkersControlSimpleMachine,
        GlassWalkersDiagnostics,
        GlassWalkersTrickShot,
        GetOfFenrisRazorClaws,
        GetOfFenrisResistPain,
        GetOfFenrisVisageOfFenris,
        FiannaFaerieLight,
        FiannaPersuasion,
        FiannaResistToxin,
        ChildrenOfGaiaMercy,
        ChildrenOfGaiaMothersTouch,
        BlackFuriesBreathOfTheWyrm,
        BlackFuriesHeightenedSenses,
        BlackFuriesSenseWyrm,
        RedTalonsBeastSpeech,
        RedTalonsWolfAtTheDoor,
        RedTalonsScentOfRunningWater,
        SilentStridersSilence,
        SilentStridersSpeedOfThought,
        SilverFangsLambentFlame,
        SilverFangsFalconsGrasp,
        BoneGnawersCooking,
        BoneGnawersStickyFingers,
        ShadowLordsSeizingTheEdge,
        ShadowLordsAuraOfConfidence,
        ShadowLordsFatalFlaw,
        UktenaSpiritSpeech,
        UktenaShroud,
        UktenaSenseMagic,
        WendigoCamouflage,
        WendigoCallTheBreeze
    ];
}

public static class WerewolfInitialGiftSelectionService
{
    public const string SelectInitialGiftsStep = "select-initial-gifts";
    public const string SelectRaceGiftStep = "select-race-gift";
    public const string SelectAuspiceGiftStep = "select-auspice-gift";
    public const string SelectTribeGiftStep = "select-tribe-gift";

    private static readonly InitialGiftDefinition[] CurrentSliceGifts =
    [
        new(WerewolfInitialGiftIdentifiers.HomidMasterOfFire, WerewolfInitialGiftSource.Race, WerewolfRaceIdentifiers.Homid, 1),
        new(WerewolfInitialGiftIdentifiers.MetisCreateElement, WerewolfInitialGiftSource.Race, WerewolfRaceIdentifiers.Metis, 1),
        new(WerewolfInitialGiftIdentifiers.LupusHareLeap, WerewolfInitialGiftSource.Race, WerewolfRaceIdentifiers.Lupus, 1),
        new(WerewolfInitialGiftIdentifiers.RagabashOpenSeal, WerewolfInitialGiftSource.Auspice, WerewolfAuspiceIdentifiers.Ragabash, 1),
        new(WerewolfInitialGiftIdentifiers.TheurgeSpiritSpeech, WerewolfInitialGiftSource.Auspice, WerewolfAuspiceIdentifiers.Theurge, 1),
        new(WerewolfInitialGiftIdentifiers.PhilodoxResistPain, WerewolfInitialGiftSource.Auspice, WerewolfAuspiceIdentifiers.Philodox, 1),
        new(WerewolfInitialGiftIdentifiers.GalliardBeastSpeech, WerewolfInitialGiftSource.Auspice, WerewolfAuspiceIdentifiers.Galliard, 1),
        new(WerewolfInitialGiftIdentifiers.AhrounFallingTouch, WerewolfInitialGiftSource.Auspice, WerewolfAuspiceIdentifiers.Ahroun, 1),
        new(WerewolfInitialGiftIdentifiers.GlassWalkersControlSimpleMachine, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.GlassWalkers, 1),
        new(WerewolfInitialGiftIdentifiers.GlassWalkersDiagnostics, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.GlassWalkers, 1),
        new(WerewolfInitialGiftIdentifiers.GlassWalkersTrickShot, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.GlassWalkers, 1),
        new(WerewolfInitialGiftIdentifiers.GetOfFenrisRazorClaws, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.GetOfFenris, 1),
        new(WerewolfInitialGiftIdentifiers.GetOfFenrisResistPain, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.GetOfFenris, 1),
        new(WerewolfInitialGiftIdentifiers.GetOfFenrisVisageOfFenris, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.GetOfFenris, 1),
        new(WerewolfInitialGiftIdentifiers.FiannaFaerieLight, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.Fianna, 1),
        new(WerewolfInitialGiftIdentifiers.FiannaPersuasion, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.Fianna, 1),
        new(WerewolfInitialGiftIdentifiers.FiannaResistToxin, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.Fianna, 1),
        new(WerewolfInitialGiftIdentifiers.ChildrenOfGaiaMercy, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.ChildrenOfGaia, 1),
        new(WerewolfInitialGiftIdentifiers.ChildrenOfGaiaMothersTouch, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.ChildrenOfGaia, 1),
        new(WerewolfInitialGiftIdentifiers.BlackFuriesBreathOfTheWyrm, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.BlackFuries, 1),
        new(WerewolfInitialGiftIdentifiers.BlackFuriesHeightenedSenses, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.BlackFuries, 1),
        new(WerewolfInitialGiftIdentifiers.BlackFuriesSenseWyrm, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.BlackFuries, 1),
        new(WerewolfInitialGiftIdentifiers.RedTalonsBeastSpeech, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.RedTalons, 1),
        new(WerewolfInitialGiftIdentifiers.RedTalonsWolfAtTheDoor, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.RedTalons, 1),
        new(WerewolfInitialGiftIdentifiers.RedTalonsScentOfRunningWater, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.RedTalons, 1),
        new(WerewolfInitialGiftIdentifiers.SilentStridersSilence, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.SilentStriders, 1),
        new(WerewolfInitialGiftIdentifiers.SilentStridersSpeedOfThought, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.SilentStriders, 1),
        new(WerewolfInitialGiftIdentifiers.SilverFangsLambentFlame, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.SilverFangs, 1),
        new(WerewolfInitialGiftIdentifiers.SilverFangsFalconsGrasp, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.SilverFangs, 1),
        new(WerewolfInitialGiftIdentifiers.BoneGnawersCooking, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.BoneGnawers, 1),
        new(WerewolfInitialGiftIdentifiers.BoneGnawersStickyFingers, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.BoneGnawers, 1),
        new(WerewolfInitialGiftIdentifiers.ShadowLordsSeizingTheEdge, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.ShadowLords, 1),
        new(WerewolfInitialGiftIdentifiers.ShadowLordsAuraOfConfidence, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.ShadowLords, 1),
        new(WerewolfInitialGiftIdentifiers.ShadowLordsFatalFlaw, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.ShadowLords, 1),
        new(WerewolfInitialGiftIdentifiers.UktenaSpiritSpeech, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.Uktena, 1),
        new(WerewolfInitialGiftIdentifiers.UktenaShroud, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.Uktena, 1),
        new(WerewolfInitialGiftIdentifiers.UktenaSenseMagic, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.Uktena, 1),
        new(WerewolfInitialGiftIdentifiers.WendigoCamouflage, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.Wendigo, 1),
        new(WerewolfInitialGiftIdentifiers.WendigoCallTheBreeze, WerewolfInitialGiftSource.Tribe, WerewolfTribeIdentifiers.Wendigo, 1)
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
