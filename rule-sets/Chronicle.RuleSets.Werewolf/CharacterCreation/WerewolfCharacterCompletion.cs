using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfCharacterCompletionRequest(
    WerewolfInitializedCharacterState Draft,
    int ExpectedDraftVersion);

public sealed record WerewolfCharacterSnapshot(
    string DraftId,
    int DraftVersion,
    WerewolfCharacterDraftStatus Status,
    string Race,
    string Auspice,
    string Tribe,
    string? MetisDeformity,
    string? RaceGift,
    string? AuspiceGift,
    string? TribeGift,
    IReadOnlyList<string> AttributePriorityOrder,
    IReadOnlyDictionary<string, int> AttributeBudgets,
    IReadOnlyList<string> AbilityPriorityOrder,
    IReadOnlyDictionary<string, int> AbilityBudgets,
    IReadOnlyDictionary<string, int?> Attributes,
    IReadOnlyDictionary<string, int?> Abilities,
    IReadOnlyDictionary<string, int?> Backgrounds,
    IReadOnlyList<string> Gifts,
    IReadOnlyDictionary<string, int?> Resources,
    IReadOnlyDictionary<string, int?> Renown,
    string? Rank,
    int? RankValue,
    string? IdentityName,
    IReadOnlyDictionary<string, string?> NarrativeFields,
    IReadOnlyDictionary<string, string> PackageBinding,
    IReadOnlyList<WerewolfCharacterCompletionFinding> ValidationResult,
    IReadOnlyList<string> CompletedStepKeys,
    string ValidationFingerprint);

public sealed record WerewolfCharacterCompletionResult(
    bool Succeeded,
    WerewolfInitializedCharacterState? Draft,
    WerewolfCharacterSnapshot? Snapshot,
    IReadOnlyList<WerewolfCharacterCompletionFinding> Findings);

public sealed record WerewolfCharacterCompletionFinding(
    WerewolfCharacterCompletionFindingSeverity Severity,
    WerewolfCharacterCompletionErrorCode Code,
    string Message);

public enum WerewolfCharacterCompletionFindingSeverity
{
    Information,
    Error
}

public enum WerewolfCharacterCompletionErrorCode
{
    CompletionAllowed,
    MissingDraft,
    DraftNotInitialized,
    DraftAlreadyCompleted,
    StaleDraftVersion,
    IdentityNameMissing,
    IdentityNameTooLong,
    RaceMissing,
    AuspiceMissing,
    TribeMissing,
    MetisDeformityMissing,
    RaceGiftMissing,
    AuspiceGiftMissing,
    TribeGiftMissing,
    AttributePrioritiesMissing,
    AttributeAllocationIncomplete,
    AbilityPrioritiesMissing,
    AbilityAllocationIncomplete,
    BackgroundAllocationIncomplete,
    ResourcesNotInitialized,
    RenownNotInitialized,
    RagabashRenownNotSelected,
    RankNotInitialized,
    MandatoryNextStepsPending
}

public static class WerewolfCharacterCompletionOperation
{
    private static readonly string[] MandatoryStepKeys =
    [
        "select-race",
        "select-auspice",
        "select-tribe",
        "allocate-attributes",
        "allocate-abilities",
        "allocate-backgrounds",
        "select-initial-gifts",
        "initialize-resources-and-rank",
        "select-ragabash-renown",
        "set-identity-name"
    ];

    public static WerewolfCharacterCompletionResult Complete(WerewolfCharacterCompletionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Draft is null)
        {
            return Invalid(WerewolfCharacterCompletionErrorCode.MissingDraft, "Completion requires an initialized draft.");
        }

        if (request.Draft.Status != WerewolfCharacterDraftStatus.Initialized && request.Draft.Status != WerewolfCharacterDraftStatus.Completed)
        {
            return Invalid(WerewolfCharacterCompletionErrorCode.DraftNotInitialized, "Completion requires an initialized or completed draft.");
        }

        if (request.Draft.Status == WerewolfCharacterDraftStatus.Completed)
        {
            return Invalid(WerewolfCharacterCompletionErrorCode.DraftAlreadyCompleted, "Draft is already completed.");
        }

        if (request.ExpectedDraftVersion != request.Draft.DraftVersion)
        {
            return Invalid(WerewolfCharacterCompletionErrorCode.StaleDraftVersion, "Completion expected draft version does not match current draft version.");
        }

        var findings = new List<WerewolfCharacterCompletionFinding>();

        if (string.IsNullOrWhiteSpace(request.Draft.IdentityName))
        {
            findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.IdentityNameMissing, "Identity name is required."));
        }
        else if (request.Draft.IdentityName.Trim().Length > 120)
        {
            findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.IdentityNameTooLong, "Identity name must not exceed 120 characters."));
        }

        if (string.IsNullOrWhiteSpace(request.Draft.Race))
        {
            findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.RaceMissing, "Race is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Draft.Auspice))
        {
            findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.AuspiceMissing, "Auspice is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Draft.Tribe))
        {
            findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.TribeMissing, "Tribe is required."));
        }

        if (StringComparer.Ordinal.Equals(request.Draft.Race, WerewolfRaceIdentifiers.Metis) &&
            string.IsNullOrWhiteSpace(request.Draft.MetisDeformity))
        {
            findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.MetisDeformityMissing, "Metis deformity is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Draft.RaceGift))
        {
            findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.RaceGiftMissing, "Race Gift is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Draft.AuspiceGift))
        {
            findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.AuspiceGiftMissing, "Auspice Gift is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Draft.TribeGift))
        {
            findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.TribeGiftMissing, "Tribe Gift is required."));
        }

        if (request.Draft.AttributePriorityOrder is null || request.Draft.AttributePriorityOrder.Count == 0)
        {
            findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.AttributePrioritiesMissing, "Attribute priorities are required."));
        }

        var attributeKeys = WerewolfAttributeIdentifiers.Supported;
        foreach (var key in attributeKeys)
        {
            if (!request.Draft.Attributes.TryGetValue(key, out var value) || value is null)
            {
                findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.AttributeAllocationIncomplete, $"Attribute {key} is not allocated."));
                break;
            }
        }

        if (request.Draft.AbilityPriorityOrder is null || request.Draft.AbilityPriorityOrder.Count == 0)
        {
            findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.AbilityPrioritiesMissing, "Ability priorities are required."));
        }

        var abilityKeys = WerewolfAbilityIdentifiers.Supported;
        foreach (var key in abilityKeys)
        {
            if (!request.Draft.Abilities.TryGetValue(key, out var value) || value is null)
            {
                findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.AbilityAllocationIncomplete, $"Ability {key} is not allocated."));
                break;
            }
        }

        var backgroundKeys = WerewolfBackgroundIdentifiers.Supported;
        foreach (var key in backgroundKeys)
        {
            if (!request.Draft.Backgrounds.TryGetValue(key, out var value) || value is null)
            {
                findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.BackgroundAllocationIncomplete, $"Background {key} is not allocated."));
                break;
            }
        }

        var resourceKeys = new[]
        {
            WerewolfCharacterResourceIdentifiers.RagePermanent,
            WerewolfCharacterResourceIdentifiers.RageCurrent,
            WerewolfCharacterResourceIdentifiers.GnosisPermanent,
            WerewolfCharacterResourceIdentifiers.GnosisCurrent,
            WerewolfCharacterResourceIdentifiers.WillpowerPermanent,
            WerewolfCharacterResourceIdentifiers.WillpowerCurrent
        };
        foreach (var key in resourceKeys)
        {
            if (!request.Draft.Resources.TryGetValue(key, out var value) || value is null)
            {
                findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.ResourcesNotInitialized, $"Resource {key} is not initialized."));
                break;
            }
        }

        var renownKeys = new[]
        {
            WerewolfRenownIdentifiers.GloryPermanent,
            WerewolfRenownIdentifiers.GloryCurrent,
            WerewolfRenownIdentifiers.HonorPermanent,
            WerewolfRenownIdentifiers.HonorCurrent,
            WerewolfRenownIdentifiers.WisdomPermanent,
            WerewolfRenownIdentifiers.WisdomCurrent
        };
        foreach (var key in renownKeys)
        {
            if (!request.Draft.Renown.TryGetValue(key, out var value) || value is null)
            {
                findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.RenownNotInitialized, $"Renown {key} is not initialized."));
                break;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Draft.Auspice) &&
            StringComparer.Ordinal.Equals(request.Draft.Auspice, WerewolfAuspiceIdentifiers.Ragabash))
        {
            var hasAllRenownKeys = renownKeys.All(key => request.Draft.Renown.TryGetValue(key, out var value) && value is not null);
            if (!hasAllRenownKeys)
            {
                findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.RagabashRenownNotSelected, "Ragabash initial Renown allocation is required before completion."));
            }
        }

        if (string.IsNullOrWhiteSpace(request.Draft.Rank) || request.Draft.RankValue is null)
        {
            findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.RankNotInitialized, "Rank is required."));
        }

        var pendingMandatorySteps = request.Draft.RequiredNextSteps
            .Where(step => MandatoryStepKeys.Contains(step, StringComparer.Ordinal))
            .ToArray();

        if (pendingMandatorySteps.Length > 0)
        {
            findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.MandatoryNextStepsPending, $"Mandatory creation steps are still pending: {string.Join(", ", pendingMandatorySteps)}."));
        }

        if (findings.Count > 0)
        {
            return new WerewolfCharacterCompletionResult(
                false,
                request.Draft,
                null,
                findings.OrderBy(finding => finding.Code.ToString(), StringComparer.Ordinal).ToArray());
        }

        var snapshot = new WerewolfCharacterSnapshot(
            request.Draft.DraftIdentity.Value,
            request.Draft.DraftVersion + 1,
            WerewolfCharacterDraftStatus.Completed,
            request.Draft.Race ?? string.Empty,
            request.Draft.Auspice ?? string.Empty,
            request.Draft.Tribe ?? string.Empty,
            request.Draft.MetisDeformity,
            request.Draft.RaceGift,
            request.Draft.AuspiceGift,
            request.Draft.TribeGift,
            Array.AsReadOnly<string>((request.Draft.AttributePriorityOrder ?? []).ToArray()),
            CopyRequiredNumber(request.Draft.AttributeBudgets),
            Array.AsReadOnly<string>((request.Draft.AbilityPriorityOrder ?? []).ToArray()),
            CopyRequiredNumber(request.Draft.AbilityBudgets),
            CopyNumeric(request.Draft.Attributes),
            CopyNumeric(request.Draft.Abilities),
            CopyNumeric(request.Draft.Backgrounds),
            Array.AsReadOnly<string>(request.Draft.Gifts.ToArray()),
            CopyNumeric(request.Draft.Resources),
            CopyNumeric(request.Draft.Renown),
            request.Draft.Rank,
            request.Draft.RankValue,
            request.Draft.IdentityName,
            CopyOptionalText(request.Draft.NarrativeFields),
            new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["packageId"] = WerewolfRuleSetPackage.ProvisionalPackageId,
                ["packageVersion"] = WerewolfRuleSetPackage.PackageVersion,
                ["declaredReleaseScope"] = WerewolfRuleSetPackage.DeclaredReleaseScope,
                ["contractVersion"] = "1"
            }),
            Array.AsReadOnly<WerewolfCharacterCompletionFinding>([]),
            Array.AsReadOnly<string>(MandatoryStepKeys.Order(StringComparer.Ordinal).ToArray()),
            ComputeValidationFingerprint(request.Draft));

        var updatedDraft = request.Draft with
        {
            Status = WerewolfCharacterDraftStatus.Completed,
            DraftVersion = request.Draft.DraftVersion + 1,
            RequiredNextSteps = Array.AsReadOnly<string>([])
        };

        return new WerewolfCharacterCompletionResult(
            true,
            updatedDraft,
            snapshot,
            [new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Information, WerewolfCharacterCompletionErrorCode.CompletionAllowed, "Character creation completed.")]);
    }

    public static WerewolfCharacterCompletionValidationResult ValidateCompletionRequired(string? identityName, IReadOnlyList<string> requiredNextSteps)
    {
        var findings = new List<WerewolfCharacterCompletionFinding>();

        if (string.IsNullOrWhiteSpace(identityName))
        {
            findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.IdentityNameMissing, "Identity name is required."));
        }
        else if (identityName.Trim().Length > 120)
        {
            findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.IdentityNameTooLong, "Identity name must not exceed 120 characters."));
        }

        var pendingMandatorySteps = requiredNextSteps
            .Where(step => MandatoryStepKeys.Contains(step, StringComparer.Ordinal))
            .ToArray();

        if (pendingMandatorySteps.Length > 0)
        {
            findings.Add(new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, WerewolfCharacterCompletionErrorCode.MandatoryNextStepsPending, $"Mandatory creation steps are still pending: {string.Join(", ", pendingMandatorySteps)}."));
        }

        return new WerewolfCharacterCompletionValidationResult(
            findings.Count == 0,
            findings.OrderBy(finding => finding.Code.ToString(), StringComparer.Ordinal).ToArray());
    }

    private static WerewolfCharacterCompletionResult Invalid(WerewolfCharacterCompletionErrorCode code, string message)
    {
        return new WerewolfCharacterCompletionResult(
            false,
            null,
            null,
            [new WerewolfCharacterCompletionFinding(WerewolfCharacterCompletionFindingSeverity.Error, code, message)]);
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

    private static string ComputeValidationFingerprint(WerewolfInitializedCharacterState draft)
    {
        var parts = new List<string>
        {
            draft.DraftVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
            draft.Status.ToString()
        };

        foreach (var key in WerewolfCharacterCreationDraftFactory.GetAttributeKeys().Order(StringComparer.Ordinal))
        {
            if (draft.Attributes.TryGetValue(key, out var value))
            {
                parts.Add($"{key}={value}");
            }
        }

        foreach (var key in WerewolfCharacterCreationDraftFactory.GetAbilityKeys().Order(StringComparer.Ordinal))
        {
            if (draft.Abilities.TryGetValue(key, out var value))
            {
                parts.Add($"{key}={value}");
            }
        }

        foreach (var key in WerewolfCharacterCreationDraftFactory.GetBackgroundKeys().Order(StringComparer.Ordinal))
        {
            if (draft.Backgrounds.TryGetValue(key, out var value))
            {
                parts.Add($"{key}={value}");
            }
        }

        foreach (var key in WerewolfCharacterCreationDraftFactory.GetResourceKeys().Order(StringComparer.Ordinal))
        {
            if (draft.Resources.TryGetValue(key, out var value))
            {
                parts.Add($"{key}={value}");
            }
        }

        return string.Join("|", parts);
    }
}

public sealed record WerewolfCharacterCompletionValidationResult(
    bool IsValid,
    IReadOnlyList<WerewolfCharacterCompletionFinding> Findings);
