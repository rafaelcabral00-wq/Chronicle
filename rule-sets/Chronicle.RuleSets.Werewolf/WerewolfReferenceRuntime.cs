using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;

namespace Chronicle.RuleSets.Werewolf;

public sealed class WerewolfReferenceRuntime : IRuleSetRuntime
{
    public const string CreateCharacterOperation = "character-creation.create-character";
    public const string SelectRaceOperation = "character-creation.select-race";
    public const string SelectAuspiceOperation = "character-creation.select-auspice";
    public const string SelectTribeOperation = "character-creation.select-tribe";
    public const string PurchaseAdditionalGiftOperation = "character-creation.purchase-additional-gift";
    public const string ExecuteGiftEffectOperation = "gift-runtime.execute-gift-effect";

    private readonly WerewolfCharacterCreationDraftInitializer characterCreation;
    public WerewolfReferenceRuntime()
        : this(new InMemoryWerewolfCharacterDraftIdentitySource())
    {
    }

    public WerewolfReferenceRuntime(IWerewolfCharacterDraftIdentitySource identitySource)
    {
        characterCreation = new WerewolfCharacterCreationDraftInitializer(identitySource);
    }

    public RuleSetRuntimeMetadata Metadata { get; } = new(
        new RuleSetRuntimeIdentity(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            "Chronicle Werewolf Reference Runtime",
            1),
        WerewolfRuleSetPackage.DeclaredReleaseScope,
        [
            new RuleSetOperationDescriptor(CreateCharacterOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectAuspiceOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectRaceOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectTribeOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(PurchaseAdditionalGiftOperation, "additional-gift-purchase", RuleSetOperationStatus.Disabled),
            new RuleSetOperationDescriptor(ExecuteGiftEffectOperation, "runtime-gift-execution", RuleSetOperationStatus.Disabled)
        ]);

    public RuleSetOperationResult Execute(RuleSetOperationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (StringComparer.Ordinal.Equals(request.OperationKey, SelectRaceOperation))
        {
            return ExecuteSelectRace(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SelectAuspiceOperation))
        {
            return ExecuteSelectAuspice(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SelectTribeOperation))
        {
            return ExecuteSelectTribe(request);
        }

        if (!StringComparer.Ordinal.Equals(request.OperationKey, CreateCharacterOperation))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.OperationUndeclared,
            [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "OperationUndeclared", "Werewolf reference runtime does not implement the requested operation.")],
            new Dictionary<string, string>(StringComparer.Ordinal));
        }

        if (!request.Inputs.TryGetValue("requestId", out var requestId) || string.IsNullOrWhiteSpace(requestId))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "MissingRequestId", "Create-character request requires a deterministic request id.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var payload = characterCreation.Initialize(new WerewolfCreateCharacterRequest(requestId));
        if (!payload.Succeeded || payload.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                payload.Findings.Select(finding => new RuleSetRuntimeFinding(
                        finding.Severity == WerewolfCharacterInitializationFindingSeverity.Error
                            ? RuleSetRuntimeFindingSeverity.Error
                            : RuleSetRuntimeFindingSeverity.Information,
                        finding.Code,
                        finding.Message))
                    .ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            payload.Findings.Select(finding => new RuleSetRuntimeFinding(
                    RuleSetRuntimeFindingSeverity.Information,
                    finding.Code,
                    finding.Message))
                .ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["draftId"] = payload.Draft.DraftIdentity.Value,
                ["draftStatus"] = payload.Draft.Status.ToString(),
                ["draftVersion"] = payload.Draft.DraftVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["nextSteps"] = string.Join(",", payload.Draft.RequiredNextSteps)
            });
    }

    private static RuleSetOperationResult ExecuteSelectRace(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("raceId", out var raceId) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidRaceSelectionRequest", "Race selection requires draftId, draftVersion, expectedDraftVersion, and raceId.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentRace = request.Inputs.GetValueOrDefault("currentRace");
        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion) with
        {
            Race = string.IsNullOrWhiteSpace(currentRace) ? null : currentRace
        };

        var result = WerewolfRaceSelectionService.SelectRace(new WerewolfRaceSelectionRequest(draft, expectedVersion, raceId));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, finding.Code.ToString(), finding.Message)).ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["draftId"] = result.Draft.DraftIdentity.Value,
                ["draftVersion"] = result.Draft.DraftVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["raceId"] = result.Draft.Race ?? string.Empty,
                ["nextSteps"] = string.Join(",", result.Draft.RequiredNextSteps)
            });
    }

    private static RuleSetOperationResult ExecuteSelectAuspice(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("auspiceId", out var auspiceId) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidAuspiceSelectionRequest", "Auspice selection requires draftId, draftVersion, expectedDraftVersion, and auspiceId.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentRace = request.Inputs.GetValueOrDefault("currentRace");
        var currentAuspice = request.Inputs.GetValueOrDefault("currentAuspice");
        var metisRequirement = request.Inputs.TryGetValue("requiresMetisDeformity", out var requiresMetisDeformity) &&
            StringComparer.Ordinal.Equals(requiresMetisDeformity, "true");
        var nextSteps = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion).RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-race"))
            .ToList();

        if (metisRequirement && !nextSteps.Contains("select-metis-deformity", StringComparer.Ordinal))
        {
            nextSteps.Add("select-metis-deformity");
        }

        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion) with
        {
            Race = string.IsNullOrWhiteSpace(currentRace) ? null : currentRace,
            Auspice = string.IsNullOrWhiteSpace(currentAuspice) ? null : currentAuspice,
            RequiredNextSteps = Array.AsReadOnly(nextSteps.Order(StringComparer.Ordinal).ToArray())
        };

        var result = WerewolfAuspiceSelectionService.SelectAuspice(new WerewolfAuspiceSelectionRequest(draft, expectedVersion, auspiceId));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, finding.Code.ToString(), finding.Message)).ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["auspiceId"] = result.Draft.Auspice ?? string.Empty,
                ["draftId"] = result.Draft.DraftIdentity.Value,
                ["draftVersion"] = result.Draft.DraftVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["nextSteps"] = string.Join(",", result.Draft.RequiredNextSteps),
                ["raceId"] = result.Draft.Race ?? string.Empty
            });
    }

    private static RuleSetOperationResult ExecuteSelectTribe(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("tribeId", out var tribeId) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidTribeSelectionRequest", "Tribe selection requires draftId, draftVersion, expectedDraftVersion, and tribeId.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentRace = request.Inputs.GetValueOrDefault("currentRace");
        var currentAuspice = request.Inputs.GetValueOrDefault("currentAuspice");
        var currentTribe = request.Inputs.GetValueOrDefault("currentTribe");
        var metisRequirement = request.Inputs.TryGetValue("requiresMetisDeformity", out var requiresMetisDeformity) &&
            StringComparer.Ordinal.Equals(requiresMetisDeformity, "true");
        var nextSteps = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion).RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-race"))
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-auspice"))
            .ToList();

        if (metisRequirement && !nextSteps.Contains("select-metis-deformity", StringComparer.Ordinal))
        {
            nextSteps.Add("select-metis-deformity");
        }

        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion) with
        {
            Race = string.IsNullOrWhiteSpace(currentRace) ? null : currentRace,
            Auspice = string.IsNullOrWhiteSpace(currentAuspice) ? null : currentAuspice,
            Tribe = string.IsNullOrWhiteSpace(currentTribe) ? null : currentTribe,
            RequiredNextSteps = Array.AsReadOnly(nextSteps.Order(StringComparer.Ordinal).ToArray())
        };

        var result = WerewolfTribeSelectionService.SelectTribe(new WerewolfTribeSelectionRequest(draft, expectedVersion, tribeId));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, finding.Code.ToString(), finding.Message)).ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["auspiceId"] = result.Draft.Auspice ?? string.Empty,
                ["draftId"] = result.Draft.DraftIdentity.Value,
                ["draftVersion"] = result.Draft.DraftVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["nextSteps"] = string.Join(",", result.Draft.RequiredNextSteps),
                ["raceId"] = result.Draft.Race ?? string.Empty,
                ["tribeId"] = result.Draft.Tribe ?? string.Empty
            });
    }
}
