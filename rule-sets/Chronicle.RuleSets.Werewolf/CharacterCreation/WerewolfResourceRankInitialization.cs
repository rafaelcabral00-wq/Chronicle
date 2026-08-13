using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfResourceRankInitializationRequest(
    WerewolfInitializedCharacterState Draft,
    int ExpectedDraftVersion);

public sealed record WerewolfInitialResourceValue(string ResourceId, int Permanent, int Current);

public sealed record WerewolfInitialRankValue(string RankId, int NumericRank);

public sealed record WerewolfInitialRenownValue(string RenownId, int Permanent, int Current);

public sealed record WerewolfResourceRankInitializationResult(
    bool Succeeded,
    WerewolfInitializedCharacterState? Draft,
    IReadOnlyList<WerewolfInitialResourceValue> Resources,
    WerewolfInitialRankValue? Rank,
    IReadOnlyList<WerewolfInitialRenownValue> Renown,
    IReadOnlyList<WerewolfResourceRankInitializationFinding> Findings);

public sealed record WerewolfResourceRankInitializationFinding(
    WerewolfResourceRankInitializationFindingSeverity Severity,
    WerewolfResourceRankInitializationErrorCode Code,
    string Message);

public enum WerewolfResourceRankInitializationFindingSeverity
{
    Information,
    Warning,
    Error
}

public enum WerewolfResourceRankInitializationErrorCode
{
    ResourcesRenownAndRankInitialized,

    MissingDraft,
    DraftNotInitialized,
    StaleDraftVersion,
    MissingRace,
    MissingAuspice,
    MissingTribe,
    MissingMetisDeformity,
    UnsupportedRace,
    UnsupportedAuspice,
    UnsupportedTribe,
    ContradictoryDraftState
}

public static class WerewolfCharacterResourceIdentifiers
{
    public const string Rage = "character.resource.rage";
    public const string Gnosis = "character.resource.gnosis";
    public const string Willpower = "character.resource.willpower";
    public const string RagePermanent = "character.resource.rage.permanent";
    public const string RageCurrent = "character.resource.rage.current";
    public const string GnosisPermanent = "character.resource.gnosis.permanent";
    public const string GnosisCurrent = "character.resource.gnosis.current";
    public const string WillpowerPermanent = "character.resource.willpower.permanent";
    public const string WillpowerCurrent = "character.resource.willpower.current";
}

public static class WerewolfRankIdentifiers
{
    public const string Cliath = "character.rank.cliath";
}

public static class WerewolfRenownIdentifiers
{
    public const string Glory = "character.renown.glory";
    public const string Honor = "character.renown.honor";
    public const string Wisdom = "character.renown.wisdom";
    public const string GloryPermanent = "character.renown.glory.permanent";
    public const string GloryCurrent = "character.renown.glory.current";
    public const string HonorPermanent = "character.renown.honor.permanent";
    public const string HonorCurrent = "character.renown.honor.current";
    public const string WisdomPermanent = "character.renown.wisdom.permanent";
    public const string WisdomCurrent = "character.renown.wisdom.current";

    public static IReadOnlyList<string> Supported { get; } =
    [
        Glory,
        Honor,
        Wisdom
    ];
}


public static class WerewolfResourceRankInitializationService
{
    public const string InitializeResourcesAndRankStep = "initialize-resources-and-rank";

    public static WerewolfResourceRankInitializationResult Initialize(WerewolfResourceRankInitializationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Draft is null)
        {
            return Invalid(WerewolfResourceRankInitializationErrorCode.MissingDraft, "Resource, Renown, and Rank initialization requires an initialized draft.");
        }

        if (request.Draft.Status != WerewolfCharacterDraftStatus.Initialized)
        {
            return Invalid(WerewolfResourceRankInitializationErrorCode.DraftNotInitialized, "Resource, Renown, and Rank initialization requires an initialized draft.");
        }

        if (request.ExpectedDraftVersion != request.Draft.DraftVersion)
        {
            return Invalid(WerewolfResourceRankInitializationErrorCode.StaleDraftVersion, "Resource initialization expected draft version does not match current draft version.");
        }

        if (string.IsNullOrWhiteSpace(request.Draft.Race))
        {
            return Invalid(WerewolfResourceRankInitializationErrorCode.MissingRace, "Resource initialization requires Race selection.");
        }

        if (string.IsNullOrWhiteSpace(request.Draft.Auspice))
        {
            return Invalid(WerewolfResourceRankInitializationErrorCode.MissingAuspice, "Resource initialization requires Auspice selection.");
        }

        if (string.IsNullOrWhiteSpace(request.Draft.Tribe))
        {
            return Invalid(WerewolfResourceRankInitializationErrorCode.MissingTribe, "Resource initialization requires Tribe selection.");
        }

        if (StringComparer.Ordinal.Equals(request.Draft.Race, WerewolfRaceIdentifiers.Metis) &&
            string.IsNullOrWhiteSpace(request.Draft.MetisDeformity))
        {
            return Invalid(WerewolfResourceRankInitializationErrorCode.MissingMetisDeformity, "Metis characters must select a deformity before resource and Rank initialization can support completion.");
        }

        if (!TryGetGnosis(request.Draft.Race, out var gnosis))
        {
            return Invalid(WerewolfResourceRankInitializationErrorCode.UnsupportedRace, "Selected Race is not supported by the current slice resource evidence.");
        }

        if (!TryGetRage(request.Draft.Auspice, out var rage))
        {
            return Invalid(WerewolfResourceRankInitializationErrorCode.UnsupportedAuspice, "Selected Auspice is not supported by the current slice resource evidence.");
        }

        if (!TryGetWillpower(request.Draft.Tribe, out var willpower))
        {
            return Invalid(WerewolfResourceRankInitializationErrorCode.UnsupportedTribe, "Selected Tribe is not supported by the executable current slice.");
        }

        var resources = new[]
        {
            new WerewolfInitialResourceValue(WerewolfCharacterResourceIdentifiers.Gnosis, gnosis, gnosis),
            new WerewolfInitialResourceValue(WerewolfCharacterResourceIdentifiers.Rage, rage, rage),
            new WerewolfInitialResourceValue(WerewolfCharacterResourceIdentifiers.Willpower, willpower, willpower)
        }.OrderBy(value => value.ResourceId, StringComparer.Ordinal).ToArray();

        var renown = GetInitialRenown(request.Draft.Auspice)
            .OrderBy(value => value.RenownId, StringComparer.Ordinal)
            .ToArray();

        var findings = new List<WerewolfResourceRankInitializationFinding>
        {
            new(WerewolfResourceRankInitializationFindingSeverity.Information, WerewolfResourceRankInitializationErrorCode.ResourcesRenownAndRankInitialized, "Resources, Renown, and Rank initialized from source-derived evidence.")
        };

        var nextSteps = request.Draft.RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, InitializeResourcesAndRankStep))
            .ToList();

        if (StringComparer.Ordinal.Equals(request.Draft.Auspice, WerewolfAuspiceIdentifiers.Ragabash))
        {
            nextSteps.Add(WerewolfRagabashRenownSelectionService.SelectRagabashRenownStep);
            renown = [];
        }

        var updated = request.Draft with
        {
            Resources = ToResourceDictionary(resources),
            Renown = ToRenownDictionary(renown),
            Rank = WerewolfRankIdentifiers.Cliath,
            RankValue = 1,
            DraftVersion = request.Draft.DraftVersion + 1,
            RequiredNextSteps = Array.AsReadOnly(nextSteps.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray()),
            AttributePriorityOrder = Array.AsReadOnly(request.Draft.AttributePriorityOrder.ToArray()),
            AttributeBudgets = CopyRequiredNumber(request.Draft.AttributeBudgets),
            AbilityPriorityOrder = Array.AsReadOnly(request.Draft.AbilityPriorityOrder.ToArray()),
            AbilityBudgets = CopyRequiredNumber(request.Draft.AbilityBudgets),
            Attributes = CopyNumeric(request.Draft.Attributes),
            Abilities = CopyNumeric(request.Draft.Abilities),
            Backgrounds = CopyNumeric(request.Draft.Backgrounds),
            Gifts = Array.AsReadOnly(request.Draft.Gifts.ToArray()),
            NarrativeFields = CopyOptionalText(request.Draft.NarrativeFields),
            DisabledCapabilities = CopyRequiredText(request.Draft.DisabledCapabilities)
        };

        return new WerewolfResourceRankInitializationResult(
            true,
            updated,
            resources,
            new WerewolfInitialRankValue(WerewolfRankIdentifiers.Cliath, 1),
            renown,
            findings.OrderBy(finding => finding.Code.ToString(), StringComparer.Ordinal).ToArray());
    }

    public static WerewolfInitializedCharacterState ClearInitializedValues(WerewolfInitializedCharacterState draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        var nextSteps = draft.RequiredNextSteps
            .Append(InitializeResourcesAndRankStep)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        if (StringComparer.Ordinal.Equals(draft.Auspice, WerewolfAuspiceIdentifiers.Ragabash))
        {
            nextSteps = nextSteps
                .Append(WerewolfRagabashRenownSelectionService.SelectRagabashRenownStep)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();
        }

        return draft with
        {
            Resources = ClearNumeric(draft.Resources),
            Renown = ClearNumeric(draft.Renown),
            Rank = null,
            RankValue = null,
            RequiredNextSteps = Array.AsReadOnly(nextSteps)
        };
    }

    private static bool TryGetGnosis(string race, out int value)
    {
        value = race switch
        {
            WerewolfRaceIdentifiers.Homid => 1,
            WerewolfRaceIdentifiers.Metis => 3,
            WerewolfRaceIdentifiers.Lupus => 5,
            _ => -1
        };
        return value >= 0;
    }

    private static bool TryGetRage(string auspice, out int value)
    {
        value = auspice switch
        {
            WerewolfAuspiceIdentifiers.Ragabash => 1,
            WerewolfAuspiceIdentifiers.Theurge => 2,
            WerewolfAuspiceIdentifiers.Philodox => 3,
            WerewolfAuspiceIdentifiers.Galliard => 4,
            WerewolfAuspiceIdentifiers.Ahroun => 5,
            _ => -1
        };
        return value >= 0;
    }

    private static bool TryGetWillpower(string tribe, out int value)
    {
        value = tribe switch
        {
            WerewolfTribeIdentifiers.GlassWalkers => 3,
            WerewolfTribeIdentifiers.GetOfFenris => 3,
            WerewolfTribeIdentifiers.Fianna => 3,
            WerewolfTribeIdentifiers.ChildrenOfGaia => 4,
            WerewolfTribeIdentifiers.BlackFuries => 3,
            WerewolfTribeIdentifiers.RedTalons => 3,
            WerewolfTribeIdentifiers.SilentStriders => 3,
            WerewolfTribeIdentifiers.SilverFangs => 3,
            WerewolfTribeIdentifiers.BoneGnawers => 4,
            WerewolfTribeIdentifiers.ShadowLords => 3,
            WerewolfTribeIdentifiers.Uktena => 3,
            WerewolfTribeIdentifiers.Wendigo => 4,
            _ => -1
        };
        return value >= 0;
    }

    private static List<WerewolfInitialRenownValue> GetInitialRenown(string auspice)
    {
        int glory = 0, honor = 0, wisdom = 0;

        switch (auspice)
        {
            case WerewolfAuspiceIdentifiers.Ragabash:
                break;
            case WerewolfAuspiceIdentifiers.Theurge:
                wisdom = 3;
                break;
            case WerewolfAuspiceIdentifiers.Philodox:
                honor = 3;
                break;
            case WerewolfAuspiceIdentifiers.Galliard:
                glory = 2;
                wisdom = 1;
                break;
            case WerewolfAuspiceIdentifiers.Ahroun:
                glory = 2;
                honor = 1;
                break;
        }

        return
        [
            new WerewolfInitialRenownValue(WerewolfRenownIdentifiers.Glory, glory, glory),
            new WerewolfInitialRenownValue(WerewolfRenownIdentifiers.Honor, honor, honor),
            new WerewolfInitialRenownValue(WerewolfRenownIdentifiers.Wisdom, wisdom, wisdom)
        ];
    }

    private static ReadOnlyDictionary<string, int?> ToRenownDictionary(IEnumerable<WerewolfInitialRenownValue> renown)
    {
        var values = new Dictionary<string, int?>(StringComparer.Ordinal);
        foreach (var entry in renown)
        {
            values[$"{entry.RenownId}.permanent"] = entry.Permanent;
            values[$"{entry.RenownId}.current"] = entry.Current;
        }

        return new ReadOnlyDictionary<string, int?>(values.OrderBy(entry => entry.Key, StringComparer.Ordinal).ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal));
    }


    private static ReadOnlyDictionary<string, int?> ToResourceDictionary(IEnumerable<WerewolfInitialResourceValue> resources)
    {
        var values = new Dictionary<string, int?>(StringComparer.Ordinal);
        foreach (var resource in resources)
        {
            values[$"{resource.ResourceId}.current"] = resource.Current;
            values[$"{resource.ResourceId}.permanent"] = resource.Permanent;
        }

        return new ReadOnlyDictionary<string, int?>(values.OrderBy(entry => entry.Key, StringComparer.Ordinal).ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal));
    }


    private static WerewolfResourceRankInitializationResult Invalid(WerewolfResourceRankInitializationErrorCode code, string message)
    {
        return new WerewolfResourceRankInitializationResult(
            false,
            null,
            [],
            null,
            [],
            [new WerewolfResourceRankInitializationFinding(WerewolfResourceRankInitializationFindingSeverity.Error, code, message)]);
    }

    private static ReadOnlyDictionary<string, int?> ClearNumeric(IReadOnlyDictionary<string, int?> values)
    {
        return new ReadOnlyDictionary<string, int?>(values.ToDictionary(entry => entry.Key, _ => (int?)null, StringComparer.Ordinal));
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
