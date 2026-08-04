using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfResourceRankInitializationRequest(
    WerewolfInitializedCharacterState Draft,
    int ExpectedDraftVersion);

public sealed record WerewolfInitialResourceValue(string ResourceId, int Permanent, int Current);

public sealed record WerewolfInitialRenownValue(string RenownId, int? Permanent, int Temporary, bool RequiresSelection);

public sealed record WerewolfInitialRankValue(string RankId, int NumericRank);

public sealed record WerewolfResourceRankInitializationResult(
    bool Succeeded,
    WerewolfInitializedCharacterState? Draft,
    IReadOnlyList<WerewolfInitialResourceValue> Resources,
    IReadOnlyList<WerewolfInitialRenownValue> Renown,
    WerewolfInitialRankValue? Rank,
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
    RagabashRenownRequiresSelection,
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

public static class WerewolfRenownIdentifiers
{
    public const string Glory = "character.renown.glory";
    public const string Honor = "character.renown.honor";
    public const string Wisdom = "character.renown.wisdom";
    public const string GloryPermanent = "character.renown.glory.permanent";
    public const string GloryTemporary = "character.renown.glory.temporary";
    public const string HonorPermanent = "character.renown.honor.permanent";
    public const string HonorTemporary = "character.renown.honor.temporary";
    public const string WisdomPermanent = "character.renown.wisdom.permanent";
    public const string WisdomTemporary = "character.renown.wisdom.temporary";
}

public static class WerewolfRankIdentifiers
{
    public const string Cliath = "character.rank.cliath";
}

public static class WerewolfResourceRankInitializationService
{
    public const string InitializeResourcesAndRankStep = "initialize-resources-and-rank";
    public const string SelectRagabashRenownStep = "select-ragabash-renown";

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

        var (renown, ragabashRequiresSelection) = BuildRenown(request.Draft.Auspice);
        var findings = new List<WerewolfResourceRankInitializationFinding>
        {
            new(WerewolfResourceRankInitializationFindingSeverity.Information, WerewolfResourceRankInitializationErrorCode.ResourcesRenownAndRankInitialized, "Resources, Renown, and Rank initialized from current-slice evidence.")
        };

        if (ragabashRequiresSelection)
        {
            findings.Add(new WerewolfResourceRankInitializationFinding(WerewolfResourceRankInitializationFindingSeverity.Warning, WerewolfResourceRankInitializationErrorCode.RagabashRenownRequiresSelection, "Ragabash initial Renown is an explicit three-point free-combination choice and remains unresolved."));
        }

        var nextSteps = request.Draft.RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, InitializeResourcesAndRankStep))
            .Where(step => !StringComparer.Ordinal.Equals(step, SelectRagabashRenownStep))
            .ToList();

        if (ragabashRequiresSelection)
        {
            nextSteps.Add(SelectRagabashRenownStep);
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
            renown,
            new WerewolfInitialRankValue(WerewolfRankIdentifiers.Cliath, 1),
            findings.OrderBy(finding => finding.Code.ToString(), StringComparer.Ordinal).ToArray());
    }

    public static WerewolfInitializedCharacterState ClearInitializedValues(WerewolfInitializedCharacterState draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        var nextSteps = draft.RequiredNextSteps
            .Append(InitializeResourcesAndRankStep)
            .Where(step => !StringComparer.Ordinal.Equals(step, SelectRagabashRenownStep))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

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
        value = StringComparer.Ordinal.Equals(tribe, WerewolfTribeIdentifiers.GlassWalkers) ? 3 : -1;
        return value >= 0;
    }

    private static (WerewolfInitialRenownValue[] Renown, bool RequiresSelection) BuildRenown(string auspice)
    {
        if (StringComparer.Ordinal.Equals(auspice, WerewolfAuspiceIdentifiers.Ragabash))
        {
            return ([
                new WerewolfInitialRenownValue(WerewolfRenownIdentifiers.Glory, null, 0, true),
                new WerewolfInitialRenownValue(WerewolfRenownIdentifiers.Honor, null, 0, true),
                new WerewolfInitialRenownValue(WerewolfRenownIdentifiers.Wisdom, null, 0, true)
            ], true);
        }

        var values = auspice switch
        {
            WerewolfAuspiceIdentifiers.Theurge => new Dictionary<string, int>(StringComparer.Ordinal)
            {
                [WerewolfRenownIdentifiers.Glory] = 0,
                [WerewolfRenownIdentifiers.Honor] = 0,
                [WerewolfRenownIdentifiers.Wisdom] = 3
            },
            WerewolfAuspiceIdentifiers.Philodox => new Dictionary<string, int>(StringComparer.Ordinal)
            {
                [WerewolfRenownIdentifiers.Glory] = 0,
                [WerewolfRenownIdentifiers.Honor] = 3,
                [WerewolfRenownIdentifiers.Wisdom] = 0
            },
            WerewolfAuspiceIdentifiers.Galliard => new Dictionary<string, int>(StringComparer.Ordinal)
            {
                [WerewolfRenownIdentifiers.Glory] = 2,
                [WerewolfRenownIdentifiers.Honor] = 0,
                [WerewolfRenownIdentifiers.Wisdom] = 1
            },
            WerewolfAuspiceIdentifiers.Ahroun => new Dictionary<string, int>(StringComparer.Ordinal)
            {
                [WerewolfRenownIdentifiers.Glory] = 2,
                [WerewolfRenownIdentifiers.Honor] = 1,
                [WerewolfRenownIdentifiers.Wisdom] = 0
            },
            _ => new Dictionary<string, int>(StringComparer.Ordinal)
        };

        return (values
            .OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .Select(entry => new WerewolfInitialRenownValue(entry.Key, entry.Value, 0, false))
            .ToArray(), false);
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

    private static ReadOnlyDictionary<string, int?> ToRenownDictionary(IEnumerable<WerewolfInitialRenownValue> renown)
    {
        var values = new Dictionary<string, int?>(StringComparer.Ordinal);
        foreach (var value in renown)
        {
            values[$"{value.RenownId}.permanent"] = value.Permanent;
            values[$"{value.RenownId}.temporary"] = value.Temporary;
        }

        return new ReadOnlyDictionary<string, int?>(values.OrderBy(entry => entry.Key, StringComparer.Ordinal).ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal));
    }

    private static WerewolfResourceRankInitializationResult Invalid(WerewolfResourceRankInitializationErrorCode code, string message)
    {
        return new WerewolfResourceRankInitializationResult(
            false,
            null,
            [],
            [],
            null,
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
