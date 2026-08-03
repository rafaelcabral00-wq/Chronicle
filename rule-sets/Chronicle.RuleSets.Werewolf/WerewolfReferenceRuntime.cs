using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;

namespace Chronicle.RuleSets.Werewolf;

public sealed class WerewolfReferenceRuntime : IRuleSetRuntime
{
    public const string CreateCharacterOperation = "character-creation.create-character";
    public const string SelectRaceOperation = "character-creation.select-race";
    public const string SelectAuspiceOperation = "character-creation.select-auspice";
    public const string SelectTribeOperation = "character-creation.select-tribe";
    public const string SelectMetisDeformityOperation = "character-creation.select-metis-deformity";
    public const string SelectRaceGiftOperation = "character-creation.select-race-gift";
    public const string SelectAuspiceGiftOperation = "character-creation.select-auspice-gift";
    public const string SelectTribeGiftOperation = "character-creation.select-tribe-gift";
    public const string SelectAttributePrioritiesOperation = "character-creation.select-attribute-priorities";
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
            new RuleSetOperationDescriptor(SelectAuspiceGiftOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectAttributePrioritiesOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectMetisDeformityOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectRaceOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectRaceGiftOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectTribeOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectTribeGiftOperation, "character-creation", RuleSetOperationStatus.Enabled),
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

        if (StringComparer.Ordinal.Equals(request.OperationKey, SelectMetisDeformityOperation))
        {
            return ExecuteSelectMetisDeformity(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SelectRaceGiftOperation))
        {
            return ExecuteSelectInitialGift(request, WerewolfInitialGiftSource.Race);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SelectAuspiceGiftOperation))
        {
            return ExecuteSelectInitialGift(request, WerewolfInitialGiftSource.Auspice);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SelectTribeGiftOperation))
        {
            return ExecuteSelectInitialGift(request, WerewolfInitialGiftSource.Tribe);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SelectAttributePrioritiesOperation))
        {
            return ExecuteSelectAttributePriorities(request);
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
                ["attributePriorityOrder"] = string.Join(",", payload.Draft.AttributePriorityOrder),
                ["attributeBudgets"] = FormatBudgets(payload.Draft.AttributeBudgets),
                ["raceGiftId"] = payload.Draft.RaceGift ?? string.Empty,
                ["auspiceGiftId"] = payload.Draft.AuspiceGift ?? string.Empty,
                ["tribeGiftId"] = payload.Draft.TribeGift ?? string.Empty,
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
        var currentRaceGift = request.Inputs.GetValueOrDefault("currentRaceGift");
        var currentAuspiceGift = request.Inputs.GetValueOrDefault("currentAuspiceGift");
        var currentTribeGift = request.Inputs.GetValueOrDefault("currentTribeGift");
        var attributePriorityOrder = ParseCsv(request.Inputs.GetValueOrDefault("attributePriorityOrder"));
        var attributeBudgets = ParseBudgets(request.Inputs.GetValueOrDefault("attributeBudgets"));
        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion) with
        {
            Race = string.IsNullOrWhiteSpace(currentRace) ? null : currentRace,
            RaceGift = string.IsNullOrWhiteSpace(currentRaceGift) ? null : currentRaceGift,
            AuspiceGift = string.IsNullOrWhiteSpace(currentAuspiceGift) ? null : currentAuspiceGift,
            TribeGift = string.IsNullOrWhiteSpace(currentTribeGift) ? null : currentTribeGift,
            AttributePriorityOrder = Array.AsReadOnly(attributePriorityOrder),
            AttributeBudgets = attributeBudgets
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
                ["attributePriorityOrder"] = string.Join(",", result.Draft.AttributePriorityOrder),
                ["attributeBudgets"] = FormatBudgets(result.Draft.AttributeBudgets),
                ["metisDeformityId"] = result.Draft.MetisDeformity ?? string.Empty,
                ["raceId"] = result.Draft.Race ?? string.Empty,
                ["raceGiftId"] = result.Draft.RaceGift ?? string.Empty,
                ["auspiceGiftId"] = result.Draft.AuspiceGift ?? string.Empty,
                ["tribeGiftId"] = result.Draft.TribeGift ?? string.Empty,
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
        var currentMetisDeformity = request.Inputs.GetValueOrDefault("currentMetisDeformity");
        var currentRaceGift = request.Inputs.GetValueOrDefault("currentRaceGift");
        var currentAuspiceGift = request.Inputs.GetValueOrDefault("currentAuspiceGift");
        var currentTribeGift = request.Inputs.GetValueOrDefault("currentTribeGift");
        var attributePriorityOrder = ParseCsv(request.Inputs.GetValueOrDefault("attributePriorityOrder"));
        var attributeBudgets = ParseBudgets(request.Inputs.GetValueOrDefault("attributeBudgets"));
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
            MetisDeformity = string.IsNullOrWhiteSpace(currentMetisDeformity) ? null : currentMetisDeformity,
            RaceGift = string.IsNullOrWhiteSpace(currentRaceGift) ? null : currentRaceGift,
            AuspiceGift = string.IsNullOrWhiteSpace(currentAuspiceGift) ? null : currentAuspiceGift,
            TribeGift = string.IsNullOrWhiteSpace(currentTribeGift) ? null : currentTribeGift,
            AttributePriorityOrder = Array.AsReadOnly(attributePriorityOrder),
            AttributeBudgets = attributeBudgets,
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
                ["attributePriorityOrder"] = string.Join(",", result.Draft.AttributePriorityOrder),
                ["attributeBudgets"] = FormatBudgets(result.Draft.AttributeBudgets),
                ["metisDeformityId"] = result.Draft.MetisDeformity ?? string.Empty,
                ["nextSteps"] = string.Join(",", result.Draft.RequiredNextSteps),
                ["raceId"] = result.Draft.Race ?? string.Empty,
                ["raceGiftId"] = result.Draft.RaceGift ?? string.Empty,
                ["auspiceGiftId"] = result.Draft.AuspiceGift ?? string.Empty,
                ["tribeGiftId"] = result.Draft.TribeGift ?? string.Empty
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
        var currentMetisDeformity = request.Inputs.GetValueOrDefault("currentMetisDeformity");
        var currentRaceGift = request.Inputs.GetValueOrDefault("currentRaceGift");
        var currentAuspiceGift = request.Inputs.GetValueOrDefault("currentAuspiceGift");
        var currentTribeGift = request.Inputs.GetValueOrDefault("currentTribeGift");
        var attributePriorityOrder = ParseCsv(request.Inputs.GetValueOrDefault("attributePriorityOrder"));
        var attributeBudgets = ParseBudgets(request.Inputs.GetValueOrDefault("attributeBudgets"));
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
            MetisDeformity = string.IsNullOrWhiteSpace(currentMetisDeformity) ? null : currentMetisDeformity,
            RaceGift = string.IsNullOrWhiteSpace(currentRaceGift) ? null : currentRaceGift,
            AuspiceGift = string.IsNullOrWhiteSpace(currentAuspiceGift) ? null : currentAuspiceGift,
            TribeGift = string.IsNullOrWhiteSpace(currentTribeGift) ? null : currentTribeGift,
            AttributePriorityOrder = Array.AsReadOnly(attributePriorityOrder),
            AttributeBudgets = attributeBudgets,
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
                ["attributePriorityOrder"] = string.Join(",", result.Draft.AttributePriorityOrder),
                ["attributeBudgets"] = FormatBudgets(result.Draft.AttributeBudgets),
                ["metisDeformityId"] = result.Draft.MetisDeformity ?? string.Empty,
                ["nextSteps"] = string.Join(",", result.Draft.RequiredNextSteps),
                ["raceId"] = result.Draft.Race ?? string.Empty,
                ["tribeId"] = result.Draft.Tribe ?? string.Empty,
                ["raceGiftId"] = result.Draft.RaceGift ?? string.Empty,
                ["auspiceGiftId"] = result.Draft.AuspiceGift ?? string.Empty,
                ["tribeGiftId"] = result.Draft.TribeGift ?? string.Empty
            });
    }

    private static RuleSetOperationResult ExecuteSelectMetisDeformity(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("deformityId", out var deformityId) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidMetisDeformitySelectionRequest", "Metis deformity selection requires draftId, draftVersion, expectedDraftVersion, and deformityId.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentRace = request.Inputs.GetValueOrDefault("currentRace");
        var currentAuspice = request.Inputs.GetValueOrDefault("currentAuspice");
        var currentTribe = request.Inputs.GetValueOrDefault("currentTribe");
        var currentMetisDeformity = request.Inputs.GetValueOrDefault("currentMetisDeformity");
        var currentRaceGift = request.Inputs.GetValueOrDefault("currentRaceGift");
        var currentAuspiceGift = request.Inputs.GetValueOrDefault("currentAuspiceGift");
        var currentTribeGift = request.Inputs.GetValueOrDefault("currentTribeGift");
        var attributePriorityOrder = ParseCsv(request.Inputs.GetValueOrDefault("attributePriorityOrder"));
        var attributeBudgets = ParseBudgets(request.Inputs.GetValueOrDefault("attributeBudgets"));
        var nextSteps = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion).RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-race"))
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-auspice"))
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-tribe"))
            .ToList();

        if (!nextSteps.Contains("select-metis-deformity", StringComparer.Ordinal))
        {
            nextSteps.Add("select-metis-deformity");
        }

        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion) with
        {
            Race = string.IsNullOrWhiteSpace(currentRace) ? null : currentRace,
            Auspice = string.IsNullOrWhiteSpace(currentAuspice) ? null : currentAuspice,
            Tribe = string.IsNullOrWhiteSpace(currentTribe) ? null : currentTribe,
            MetisDeformity = string.IsNullOrWhiteSpace(currentMetisDeformity) ? null : currentMetisDeformity,
            RaceGift = string.IsNullOrWhiteSpace(currentRaceGift) ? null : currentRaceGift,
            AuspiceGift = string.IsNullOrWhiteSpace(currentAuspiceGift) ? null : currentAuspiceGift,
            TribeGift = string.IsNullOrWhiteSpace(currentTribeGift) ? null : currentTribeGift,
            AttributePriorityOrder = Array.AsReadOnly(attributePriorityOrder),
            AttributeBudgets = attributeBudgets,
            RequiredNextSteps = Array.AsReadOnly(nextSteps.Order(StringComparer.Ordinal).ToArray())
        };

        var result = WerewolfMetisDeformitySelectionService.SelectDeformity(new WerewolfMetisDeformitySelectionRequest(draft, expectedVersion, deformityId));
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
                ["attributePriorityOrder"] = string.Join(",", result.Draft.AttributePriorityOrder),
                ["attributeBudgets"] = FormatBudgets(result.Draft.AttributeBudgets),
                ["metisDeformityId"] = result.Draft.MetisDeformity ?? string.Empty,
                ["nextSteps"] = string.Join(",", result.Draft.RequiredNextSteps),
                ["raceId"] = result.Draft.Race ?? string.Empty,
                ["tribeId"] = result.Draft.Tribe ?? string.Empty,
                ["raceGiftId"] = result.Draft.RaceGift ?? string.Empty,
                ["auspiceGiftId"] = result.Draft.AuspiceGift ?? string.Empty,
                ["tribeGiftId"] = result.Draft.TribeGift ?? string.Empty
            });
    }

    private static RuleSetOperationResult ExecuteSelectInitialGift(RuleSetOperationRequest request, WerewolfInitialGiftSource source)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("giftId", out var giftId) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidInitialGiftSelectionRequest", "Initial Gift selection requires draftId, draftVersion, expectedDraftVersion, and giftId.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var result = WerewolfInitialGiftSelectionService.SelectGift(new WerewolfInitialGiftSelectionRequest(draft, expectedVersion, source, giftId));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return ToDraftOperationResult(
            result.Draft,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, finding.Code.ToString(), finding.Message)).ToArray());
    }

    private static RuleSetOperationResult ExecuteSelectAttributePriorities(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("primaryCategoryId", out var primaryCategoryId) ||
            !request.Inputs.TryGetValue("secondaryCategoryId", out var secondaryCategoryId) ||
            !request.Inputs.TryGetValue("tertiaryCategoryId", out var tertiaryCategoryId) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidAttributePrioritySelectionRequest", "Attribute priority selection requires draftId, draftVersion, expectedDraftVersion, primaryCategoryId, secondaryCategoryId, and tertiaryCategoryId.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var result = WerewolfAttributePrioritySelectionService.SelectPriorities(new WerewolfAttributePrioritySelectionRequest(
            draft,
            expectedVersion,
            primaryCategoryId,
            secondaryCategoryId,
            tertiaryCategoryId));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return ToDraftOperationResult(
            result.Draft,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, finding.Code.ToString(), finding.Message)).ToArray());
    }

    private static WerewolfInitializedCharacterState BuildDraftFromInputs(RuleSetOperationRequest request, string draftId, int draftVersion)
    {
        var currentRace = request.Inputs.GetValueOrDefault("currentRace");
        var currentAuspice = request.Inputs.GetValueOrDefault("currentAuspice");
        var currentTribe = request.Inputs.GetValueOrDefault("currentTribe");
        var currentMetisDeformity = request.Inputs.GetValueOrDefault("currentMetisDeformity");
        var currentRaceGift = request.Inputs.GetValueOrDefault("currentRaceGift");
        var currentAuspiceGift = request.Inputs.GetValueOrDefault("currentAuspiceGift");
        var currentTribeGift = request.Inputs.GetValueOrDefault("currentTribeGift");
        var attributePriorityOrder = ParseCsv(request.Inputs.GetValueOrDefault("attributePriorityOrder"));
        var attributeBudgets = ParseBudgets(request.Inputs.GetValueOrDefault("attributeBudgets"));

        var nextSteps = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion).RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-race"))
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-auspice"))
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-tribe"))
            .ToList();

        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion) with
        {
            Race = string.IsNullOrWhiteSpace(currentRace) ? null : currentRace,
            Auspice = string.IsNullOrWhiteSpace(currentAuspice) ? null : currentAuspice,
            Tribe = string.IsNullOrWhiteSpace(currentTribe) ? null : currentTribe,
            MetisDeformity = string.IsNullOrWhiteSpace(currentMetisDeformity) ? null : currentMetisDeformity,
            RaceGift = string.IsNullOrWhiteSpace(currentRaceGift) ? null : currentRaceGift,
            AuspiceGift = string.IsNullOrWhiteSpace(currentAuspiceGift) ? null : currentAuspiceGift,
            TribeGift = string.IsNullOrWhiteSpace(currentTribeGift) ? null : currentTribeGift,
            AttributePriorityOrder = Array.AsReadOnly(attributePriorityOrder),
            AttributeBudgets = attributeBudgets,
            RequiredNextSteps = Array.AsReadOnly(nextSteps.Order(StringComparer.Ordinal).ToArray())
        };

        return draft with
        {
            RequiredNextSteps = WerewolfInitialGiftSelectionService.ReconcileInitialGiftNextSteps(draft)
        };
    }

    private static RuleSetOperationResult ToDraftOperationResult(
        WerewolfInitializedCharacterState draft,
        IReadOnlyList<RuleSetRuntimeFinding> findings)
    {
        return new RuleSetOperationResult(
            true,
            null,
            findings,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["attributeBudgets"] = FormatBudgets(draft.AttributeBudgets),
                ["attributePriorityOrder"] = string.Join(",", draft.AttributePriorityOrder),
                ["auspiceGiftId"] = draft.AuspiceGift ?? string.Empty,
                ["auspiceId"] = draft.Auspice ?? string.Empty,
                ["draftId"] = draft.DraftIdentity.Value,
                ["draftVersion"] = draft.DraftVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["metisDeformityId"] = draft.MetisDeformity ?? string.Empty,
                ["nextSteps"] = string.Join(",", draft.RequiredNextSteps),
                ["raceGiftId"] = draft.RaceGift ?? string.Empty,
                ["raceId"] = draft.Race ?? string.Empty,
                ["tribeGiftId"] = draft.TribeGift ?? string.Empty,
                ["tribeId"] = draft.Tribe ?? string.Empty
            });
    }

    private static string[] ParseCsv(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static Dictionary<string, int> ParseBudgets(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new Dictionary<string, int>(StringComparer.Ordinal);
        }

        var values = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var entry in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = entry.Split(':', StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && int.TryParse(parts[1], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var budget))
            {
                values[parts[0]] = budget;
            }
        }

        return values;
    }

    private static string FormatBudgets(IReadOnlyDictionary<string, int> budgets)
    {
        return string.Join(
            ",",
            budgets
                .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .Select(entry => $"{entry.Key}:{entry.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));
    }
}
