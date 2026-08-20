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
    public const string AllocateAttributesOperation = "character-creation.allocate-attributes";
    public const string SelectAbilityPrioritiesOperation = "character-creation.select-ability-priorities";
    public const string AllocateAbilitiesOperation = "character-creation.allocate-abilities";
    public const string AllocateBackgroundsOperation = "character-creation.allocate-backgrounds";
    public const string InitializeResourcesAndRankOperation = "character-creation.initialize-resources-and-rank";
    public const string SelectRagabashRenownOperation = "character-creation.select-ragabash-renown";
    public const string SetIdentityNameOperation = "character-creation.set-identity-name";
    public const string CompleteCharacterOperation = "character-creation.complete-character";
    public const string DefineActionTestOperation = "character-runtime.define-action-test";
    public const string InterpretActionRollOperation = "character-runtime.interpret-action-roll";
    public const string SpendResourceOperation = "character-runtime.spend-resource";
    public const string RecoverResourceOperation = "character-runtime.recover-resource";
    public const string AwardTemporaryRenownOperation = "character-runtime.award-temporary-renown";
    public const string LoseTemporaryRenownOperation = "character-runtime.lose-temporary-renown";
    public const string ConvertTemporaryToPermanentRenownOperation = "character-runtime.convert-temporary-to-permanent-renown";
    public const string ApplyDamageOperation = "character-runtime.apply-damage";
    public const string RecoverDamageOperation = "character-runtime.recover-damage";
    public const string PermanecerAtivoOperation = "character-runtime.permanecer-ativo";
    public const string RegenerateOperation = "character-runtime.regenerate";
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

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null
    };

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
            new RuleSetOperationDescriptor(AllocateAbilitiesOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(AllocateAttributesOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(AllocateBackgroundsOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(InitializeResourcesAndRankOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SetIdentityNameOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(CompleteCharacterOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectAbilityPrioritiesOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectAttributePrioritiesOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectMetisDeformityOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectRaceOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectRaceGiftOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectRagabashRenownOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectTribeOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SelectTribeGiftOperation, "character-creation", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(DefineActionTestOperation, "generic-dice", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(InterpretActionRollOperation, "generic-dice", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(SpendResourceOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(RecoverResourceOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(AwardTemporaryRenownOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(LoseTemporaryRenownOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(ConvertTemporaryToPermanentRenownOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(RecoverDamageOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(ApplyDamageOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(PermanecerAtivoOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
            new RuleSetOperationDescriptor(RegenerateOperation, "post-creation-character-operations", RuleSetOperationStatus.Enabled),
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

        if (StringComparer.Ordinal.Equals(request.OperationKey, AllocateAttributesOperation))
        {
            return ExecuteAllocateAttributes(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SelectAbilityPrioritiesOperation))
        {
            return ExecuteSelectAbilityPriorities(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, AllocateAbilitiesOperation))
        {
            return ExecuteAllocateAbilities(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, AllocateBackgroundsOperation))
        {
            return ExecuteAllocateBackgrounds(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, InitializeResourcesAndRankOperation))
        {
            return ExecuteInitializeResourcesAndRank(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SelectRagabashRenownOperation))
        {
            return ExecuteSelectRagabashRenown(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SetIdentityNameOperation))
        {
            return ExecuteSetIdentityName(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, CompleteCharacterOperation))
        {
            return ExecuteCompleteCharacter(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, DefineActionTestOperation))
        {
            return ExecuteDefineActionTest(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, InterpretActionRollOperation))
        {
            return ExecuteInterpretActionRoll(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, SpendResourceOperation))
        {
            return ExecuteSpendResource(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, RecoverResourceOperation))
        {
            return ExecuteRecoverResource(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, AwardTemporaryRenownOperation))
        {
            return ExecuteAwardTemporaryRenown(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, LoseTemporaryRenownOperation))
        {
            return ExecuteLoseTemporaryRenown(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, ConvertTemporaryToPermanentRenownOperation))
        {
            return ExecuteConvertTemporaryToPermanentRenown(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, ApplyDamageOperation))
        {
            return ExecuteApplyDamage(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, RecoverDamageOperation))
        {
            return ExecuteRecoverDamage(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, PermanecerAtivoOperation))
        {
            return ExecutePermanecerAtivo(request);
        }

        if (StringComparer.Ordinal.Equals(request.OperationKey, RegenerateOperation))
        {
            return ExecuteRegenerate(request);
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
                ["abilityPriorityOrder"] = string.Join(",", payload.Draft.AbilityPriorityOrder),
                ["abilityBudgets"] = FormatBudgets(payload.Draft.AbilityBudgets),
                ["backgrounds"] = FormatNullableRatings(payload.Draft.Backgrounds),
                ["resources"] = FormatNullableRatings(payload.Draft.Resources),
                ["renown"] = FormatNullableRatings(payload.Draft.Renown),
                ["rankId"] = payload.Draft.Rank ?? string.Empty,
                ["rankValue"] = payload.Draft.RankValue?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
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
        var defaultNextSteps = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion).RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-race"))
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-auspice"))
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-tribe"))
            .ToList();

        var nextSteps = request.Inputs.TryGetValue("nextSteps", out var nextStepsText) && !string.IsNullOrWhiteSpace(nextStepsText)
            ? nextStepsText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            : defaultNextSteps;

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
        var defaultNextSteps = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion).RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-race"))
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-auspice"))
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-tribe"))
            .ToList();

        var nextSteps = request.Inputs.TryGetValue("nextSteps", out var nextStepsText) && !string.IsNullOrWhiteSpace(nextStepsText)
            ? nextStepsText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            : defaultNextSteps;

        if (attributePriorityOrder.Length > 0 || attributeBudgets.Count > 0)
        {
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "select-attribute-priorities"));
        }

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

    private static RuleSetOperationResult ExecuteAllocateAttributes(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("attributes", out var attributesText) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidAttributeAllocationRequest", "Attribute allocation requires draftId, draftVersion, expectedDraftVersion, and attributes.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var allocations = ParseAttributeAllocations(attributesText);
        var result = WerewolfAttributeAllocationService.AllocateAttributes(new WerewolfAttributeAllocationRequest(draft, expectedVersion, allocations));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["attributeCategoryTotals"] = FormatAttributeCategoryTotals(result.CategoryTotals)
                });
        }

        return ToDraftOperationResult(
            result.Draft,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, finding.Code.ToString(), finding.Message)).ToArray(),
            result.CategoryTotals);
    }

    private static RuleSetOperationResult ExecuteSelectAbilityPriorities(RuleSetOperationRequest request)
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
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidAbilityPrioritySelectionRequest", "Ability priority selection requires draftId, draftVersion, expectedDraftVersion, primaryCategoryId, secondaryCategoryId, and tertiaryCategoryId.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var result = WerewolfAbilitySelectionService.SelectPriorities(new WerewolfAbilityPrioritySelectionRequest(
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

    private static RuleSetOperationResult ExecuteAllocateAbilities(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("abilities", out var abilitiesText) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidAbilityAllocationRequest", "Ability allocation requires draftId, draftVersion, expectedDraftVersion, and abilities.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var result = WerewolfAbilitySelectionService.AllocateAbilities(new WerewolfAbilityAllocationRequest(draft, expectedVersion, ParseAbilityAllocations(abilitiesText)));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["abilityCategoryTotals"] = FormatAbilityCategoryTotals(result.CategoryTotals)
                });
        }

        return ToDraftOperationResult(
            result.Draft,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, finding.Code.ToString(), finding.Message)).ToArray(),
            null,
            result.CategoryTotals);
    }

    private static RuleSetOperationResult ExecuteAllocateBackgrounds(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("backgrounds", out var backgroundsText) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidBackgroundAllocationRequest", "Background allocation requires draftId, draftVersion, expectedDraftVersion, and backgrounds.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var result = WerewolfBackgroundAllocationService.AllocateBackgrounds(new WerewolfBackgroundAllocationRequest(draft, expectedVersion, ParseBackgroundAllocations(backgroundsText)));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["backgroundTotal"] = FormatBackgroundTotal(result.Spent, result.Budget, result.Remaining)
                });
        }

        return ToDraftOperationResult(
            result.Draft,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, finding.Code.ToString(), finding.Message)).ToArray(),
            null,
            null,
            result);
    }

    private static RuleSetOperationResult ExecuteInitializeResourcesAndRank(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidResourceRankInitializationRequest", "Resource and Rank initialization requires draftId, draftVersion, and expectedDraftVersion.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var result = WerewolfResourceRankInitializationService.Initialize(new WerewolfResourceRankInitializationRequest(draft, expectedVersion));
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
            result.Findings.Select(finding => new RuleSetRuntimeFinding(
                    finding.Severity == WerewolfResourceRankInitializationFindingSeverity.Error ? RuleSetRuntimeFindingSeverity.Error : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code.ToString(),
                    finding.Message))
                .ToArray());
    }

    private static RuleSetOperationResult ExecuteSelectRagabashRenown(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("glory", out var gloryText) ||
            !request.Inputs.TryGetValue("honor", out var honorText) ||
            !request.Inputs.TryGetValue("wisdom", out var wisdomText) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
            !int.TryParse(gloryText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var glory) ||
            !int.TryParse(honorText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var honor) ||
            !int.TryParse(wisdomText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var wisdom))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidRagabashRenownRequest", "Ragabash Renown selection requires draftId, draftVersion, expectedDraftVersion, glory, honor, and wisdom.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var result = WerewolfRagabashRenownSelectionService.SelectRenown(new WerewolfRagabashRenownSelectionRequest(draft, expectedVersion, glory, honor, wisdom));
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
            result.Findings.Select(finding => new RuleSetRuntimeFinding(
                    finding.Severity == WerewolfRagabashRenownSelectionFindingSeverity.Error ? RuleSetRuntimeFindingSeverity.Error : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code.ToString(),
                    finding.Message))
                .ToArray());
    }

    private static RuleSetOperationResult ExecuteSetIdentityName(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("identityName", out var identityName) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidIdentityNameRequest", "Identity name operation requires draftId, draftVersion, expectedDraftVersion, and identityName.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var result = WerewolfIdentityNameOperation.SetIdentityName(new WerewolfIdentityNameRequest(draft, expectedVersion, identityName));
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
            result.Findings.Select(finding => new RuleSetRuntimeFinding(
                    finding.Severity == WerewolfIdentityNameFindingSeverity.Error ? RuleSetRuntimeFindingSeverity.Error : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code.ToString(),
                    finding.Message))
                .ToArray());
    }

    private static RuleSetOperationResult ExecuteCompleteCharacter(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidCompletionRequest", "Completion operation requires draftId, draftVersion, and expectedDraftVersion.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var result = WerewolfCharacterCompletionOperation.Complete(new WerewolfCharacterCompletionRequest(draft, expectedVersion));
        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, finding.Code.ToString(), finding.Message)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var snapshotJson = System.Text.Json.JsonSerializer.Serialize(result.Snapshot, JsonOptions);

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(
                    finding.Severity == WerewolfCharacterCompletionFindingSeverity.Error ? RuleSetRuntimeFindingSeverity.Error : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code.ToString(),
                    finding.Message))
                .ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["draftId"] = result.Draft.DraftIdentity.Value,
                ["draftVersion"] = result.Draft.DraftVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["status"] = result.Draft.Status.ToString(),
                ["identityName"] = result.Draft.IdentityName ?? string.Empty,
                ["nextSteps"] = string.Join(",", result.Draft.RequiredNextSteps),
                ["validationFingerprint"] = result.Snapshot!.ValidationFingerprint,
                ["completedStepKeys"] = string.Join(",", result.Snapshot.CompletedStepKeys),
                ["snapshot"] = snapshotJson
            });
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
        var attributes = ParseNullableAttributes(request.Inputs.GetValueOrDefault("attributes"));
        var abilityPriorityOrder = ParseCsv(request.Inputs.GetValueOrDefault("abilityPriorityOrder"));
        var abilityBudgets = ParseBudgets(request.Inputs.GetValueOrDefault("abilityBudgets"));
        var abilities = ParseNullableRatings(request.Inputs.GetValueOrDefault("abilities"));
        var backgrounds = ParseNullableRatings(request.Inputs.GetValueOrDefault("backgrounds"));
        var resources = ParseNullableRatings(request.Inputs.GetValueOrDefault("resources"));
        var renown = ParseNullableRatings(request.Inputs.GetValueOrDefault("renown"));
        var rank = request.Inputs.GetValueOrDefault("rankId");
        var rankValue = request.Inputs.TryGetValue("rankValue", out var rankValueText) &&
            int.TryParse(rankValueText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var parsedRankValue)
            ? parsedRankValue
            : (int?)null;
        var identityName = request.Inputs.GetValueOrDefault("identityName");
        var draftStatus = request.Inputs.TryGetValue("draftStatus", out var draftStatusText) &&
            Enum.TryParse<WerewolfCharacterDraftStatus>(draftStatusText, true, out var parsedDraftStatus)
            ? parsedDraftStatus
            : WerewolfCharacterDraftStatus.Initialized;

        var defaultNextSteps = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion).RequiredNextSteps
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-race"))
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-auspice"))
            .Where(step => !StringComparer.Ordinal.Equals(step, "select-tribe"))
            .ToList();

        var nextSteps = request.Inputs.TryGetValue("nextSteps", out var nextStepsText) && !string.IsNullOrWhiteSpace(nextStepsText)
            ? nextStepsText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            : defaultNextSteps;

        if (attributePriorityOrder.Length > 0 || attributeBudgets.Count > 0)
        {
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "select-attribute-priorities"));
        }

        if (abilityPriorityOrder.Length > 0 || abilityBudgets.Count > 0)
        {
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "select-ability-priorities"));
        }

        if (attributes.Count > 0)
        {
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "allocate-attributes"));
        }

        if (abilities.Count > 0)
        {
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "allocate-abilities"));
        }

        if (backgrounds.Count > 0)
        {
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "allocate-backgrounds"));
        }

        if (!string.IsNullOrWhiteSpace(currentRaceGift) && !string.IsNullOrWhiteSpace(currentAuspiceGift) && !string.IsNullOrWhiteSpace(currentTribeGift))
        {
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "select-initial-gifts"));
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "select-race-gift"));
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "select-auspice-gift"));
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "select-tribe-gift"));
        }

        if (resources.Count > 0 || !string.IsNullOrWhiteSpace(rank))
        {
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "initialize-resources-and-rank"));
        }

        if (!string.IsNullOrWhiteSpace(request.Inputs.GetValueOrDefault("identityName")))
        {
            nextSteps.RemoveAll(step => StringComparer.Ordinal.Equals(step, "set-identity-name"));
        }

        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion) with
        {
            Status = draftStatus,
            Race = string.IsNullOrWhiteSpace(currentRace) ? null : currentRace,
            Auspice = string.IsNullOrWhiteSpace(currentAuspice) ? null : currentAuspice,
            Tribe = string.IsNullOrWhiteSpace(currentTribe) ? null : currentTribe,
            MetisDeformity = string.IsNullOrWhiteSpace(currentMetisDeformity) ? null : currentMetisDeformity,
            RaceGift = string.IsNullOrWhiteSpace(currentRaceGift) ? null : currentRaceGift,
            AuspiceGift = string.IsNullOrWhiteSpace(currentAuspiceGift) ? null : currentAuspiceGift,
            TribeGift = string.IsNullOrWhiteSpace(currentTribeGift) ? null : currentTribeGift,
            AttributePriorityOrder = Array.AsReadOnly(attributePriorityOrder),
            AttributeBudgets = attributeBudgets,
            AbilityPriorityOrder = Array.AsReadOnly(abilityPriorityOrder),
            AbilityBudgets = abilityBudgets,
            Attributes = attributes.Count == 0
                ? WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion).Attributes
                : attributes,
            Abilities = abilities.Count == 0
                ? WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion).Abilities
                : abilities,
            Backgrounds = backgrounds.Count == 0
                ? WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion).Backgrounds
                : backgrounds,
            Resources = resources.Count == 0
                ? WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion).Resources
                : resources,
            Renown = renown.Count == 0
                ? WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(new WerewolfCharacterDraftIdentity(draftId), draftVersion).Renown
                : renown,
            Rank = string.IsNullOrWhiteSpace(rank) ? null : rank,
            RankValue = rankValue,
            IdentityName = string.IsNullOrWhiteSpace(identityName) ? null : identityName,
            RequiredNextSteps = Array.AsReadOnly(nextSteps.Order(StringComparer.Ordinal).ToArray())
        };

        return draft with
        {
            RequiredNextSteps = WerewolfInitialGiftSelectionService.ReconcileInitialGiftNextSteps(draft)
        };
    }

    private static RuleSetOperationResult ToDraftOperationResult(
        WerewolfInitializedCharacterState draft,
        IReadOnlyList<RuleSetRuntimeFinding> findings,
        IReadOnlyList<WerewolfAttributeAllocationCategoryTotal>? attributeCategoryTotals = null,
        IReadOnlyList<WerewolfAbilityAllocationCategoryTotal>? abilityCategoryTotals = null,
        WerewolfBackgroundAllocationResult? backgroundAllocation = null)
    {
        return new RuleSetOperationResult(
            true,
            null,
            findings,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["attributeBudgets"] = FormatBudgets(draft.AttributeBudgets),
                ["attributeCategoryTotals"] = FormatAttributeCategoryTotals(attributeCategoryTotals ?? []),
                ["attributes"] = FormatNullableAttributes(draft.Attributes),
                ["attributePriorityOrder"] = string.Join(",", draft.AttributePriorityOrder),
                ["abilityBudgets"] = FormatBudgets(draft.AbilityBudgets),
                ["abilityCategoryTotals"] = FormatAbilityCategoryTotals(abilityCategoryTotals ?? []),
                ["abilities"] = FormatNullableRatings(draft.Abilities),
                ["abilityPriorityOrder"] = string.Join(",", draft.AbilityPriorityOrder),
                ["auspiceGiftId"] = draft.AuspiceGift ?? string.Empty,
                ["auspiceId"] = draft.Auspice ?? string.Empty,
                ["backgroundTotal"] = backgroundAllocation is null
                    ? string.Empty
                    : FormatBackgroundTotal(backgroundAllocation.Spent, backgroundAllocation.Budget, backgroundAllocation.Remaining),
                ["backgrounds"] = FormatNullableRatings(draft.Backgrounds),
                ["draftId"] = draft.DraftIdentity.Value,
                ["draftVersion"] = draft.DraftVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["identityName"] = draft.IdentityName ?? string.Empty,
                ["metisDeformityId"] = draft.MetisDeformity ?? string.Empty,
                ["nextSteps"] = string.Join(",", draft.RequiredNextSteps),
                ["raceGiftId"] = draft.RaceGift ?? string.Empty,
                ["raceId"] = draft.Race ?? string.Empty,
                ["rankId"] = draft.Rank ?? string.Empty,
                ["rankValue"] = draft.RankValue?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["renown"] = FormatNullableRatings(draft.Renown),
                ["resources"] = FormatNullableRatings(draft.Resources),
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

    private static List<WerewolfAttributeDotAllocation> ParseAttributeAllocations(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        var allocations = new List<WerewolfAttributeDotAllocation>();
        foreach (var entry in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = entry.Split(':', StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && int.TryParse(parts[1], System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out var rating))
            {
                allocations.Add(new WerewolfAttributeDotAllocation(parts[0], rating));
            }
            else
            {
                allocations.Add(new WerewolfAttributeDotAllocation(entry, 0));
            }
        }

        return allocations;
    }

    private static List<WerewolfAbilityDotAllocation> ParseAbilityAllocations(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        var allocations = new List<WerewolfAbilityDotAllocation>();
        foreach (var entry in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = entry.Split(':', StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && int.TryParse(parts[1], System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out var rating))
            {
                allocations.Add(new WerewolfAbilityDotAllocation(parts[0], rating));
            }
            else
            {
                allocations.Add(new WerewolfAbilityDotAllocation(entry, -1));
            }
        }

        return allocations;
    }

    private static List<WerewolfBackgroundRatingAllocation> ParseBackgroundAllocations(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        var allocations = new List<WerewolfBackgroundRatingAllocation>();
        foreach (var entry in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = entry.Split(':', StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && int.TryParse(parts[1], System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out var rating))
            {
                allocations.Add(new WerewolfBackgroundRatingAllocation(parts[0], rating));
            }
            else
            {
                allocations.Add(new WerewolfBackgroundRatingAllocation(entry, -1));
            }
        }

        return allocations;
    }

    private static System.Collections.ObjectModel.ReadOnlyDictionary<string, int?> ParseNullableAttributes(string? value)
    {
        return ParseNullableRatings(value);
    }

    private static System.Collections.ObjectModel.ReadOnlyDictionary<string, int?> ParseNullableRatings(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new System.Collections.ObjectModel.ReadOnlyDictionary<string, int?>(new Dictionary<string, int?>(StringComparer.Ordinal));
        }

        var values = new Dictionary<string, int?>(StringComparer.Ordinal);
        foreach (var entry in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = entry.Split(':', StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && int.TryParse(parts[1], System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out var rating))
            {
                values[parts[0]] = rating;
            }
        }

        return new System.Collections.ObjectModel.ReadOnlyDictionary<string, int?>(values);
    }

    private static string FormatNullableAttributes(IReadOnlyDictionary<string, int?> attributes)
    {
        return FormatNullableRatings(attributes);
    }

    private static string FormatNullableRatings(IReadOnlyDictionary<string, int?> attributes)
    {
        return string.Join(
            ",",
            attributes
                .Where(entry => entry.Value.HasValue)
                .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .Select(entry => $"{entry.Key}:{entry.Value!.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));
    }

    private static string FormatBackgroundTotal(int spent, int budget, int remaining)
    {
        return $"{spent.ToString(System.Globalization.CultureInfo.InvariantCulture)}/{budget.ToString(System.Globalization.CultureInfo.InvariantCulture)}/{remaining.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
    }

    private static string FormatAttributeCategoryTotals(IReadOnlyList<WerewolfAttributeAllocationCategoryTotal> totals)
    {
        return string.Join(
            ",",
            totals
                .OrderBy(total => total.CategoryId, StringComparer.Ordinal)
                .Select(total => $"{total.CategoryId}:{total.Spent.ToString(System.Globalization.CultureInfo.InvariantCulture)}/{total.Budget.ToString(System.Globalization.CultureInfo.InvariantCulture)}/{total.Remaining.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));
    }

    private static string FormatAbilityCategoryTotals(IReadOnlyList<WerewolfAbilityAllocationCategoryTotal> totals)
    {
        return string.Join(
            ",",
            totals
                .OrderBy(total => total.CategoryId, StringComparer.Ordinal)
                .Select(total => $"{total.CategoryId}:{total.Spent.ToString(System.Globalization.CultureInfo.InvariantCulture)}/{total.Budget.ToString(System.Globalization.CultureInfo.InvariantCulture)}/{total.Remaining.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));
    }

    private static string FormatBudgets(IReadOnlyDictionary<string, int> budgets)
    {
        return string.Join(
            ",",
            budgets
                .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .Select(entry => $"{entry.Key}:{entry.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));
    }

    private static RuleSetOperationResult ExecuteDefineActionTest(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("draftId", out var draftId) ||
            !request.Inputs.TryGetValue("draftVersion", out var draftVersionText) ||
            !request.Inputs.TryGetValue("expectedDraftVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("requestId", out var actionRequestId) ||
            !request.Inputs.TryGetValue("attributeId", out var attributeId) ||
            !request.Inputs.TryGetValue("abilityId", out var abilityId) ||
            !request.Inputs.TryGetValue("difficulty", out var difficultyText) ||
            !int.TryParse(draftVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var draftVersion) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
            !int.TryParse(difficultyText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var difficulty))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidActionTestDefinitionRequest", "Action test definition requires draftId, draftVersion, expectedDraftVersion, requestId, attributeId, abilityId, and difficulty.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var modifierText = request.Inputs.GetValueOrDefault("modifier");
        var modifier = int.TryParse(modifierText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var parsedModifier)
            ? parsedModifier
            : (int?)null;

        var currentFormText = request.Inputs.GetValueOrDefault("currentForm");

        var draft = BuildDraftFromInputs(request, draftId, draftVersion);
        var result = WerewolfActionTestDefinitionService.DefineTest(new WerewolfActionTestDefinitionRequest(
            draft,
            expectedVersion,
            actionRequestId,
            attributeId,
            abilityId,
            difficulty,
            modifier,
            currentFormText));

        if (!result.Succeeded || result.Draft is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(
                        finding.Severity == WerewolfActionTestDefinitionFindingSeverity.Error
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
            result.Findings.Select(finding => new RuleSetRuntimeFinding(
                    finding.Severity == WerewolfActionTestDefinitionFindingSeverity.Error
                        ? RuleSetRuntimeFindingSeverity.Error
                        : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code,
                    finding.Message))
                .ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = result.RequestId ?? string.Empty,
                ["diceQuantity"] = result.DiceQuantity?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["diceFaces"] = result.DiceFaces?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["attributeId"] = result.AttributeId ?? string.Empty,
                ["abilityId"] = result.AbilityId ?? string.Empty,
                ["difficulty"] = result.Difficulty?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["modifier"] = result.Modifier?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
            });
    }

    private static RuleSetOperationResult ExecuteInterpretActionRoll(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("requestId", out var actionRequestId) ||
            !request.Inputs.TryGetValue("diceValues", out var diceValuesText) ||
            !request.Inputs.TryGetValue("difficulty", out var difficultyText) ||
            !request.Inputs.TryGetValue("diceQuantity", out var diceQuantityText) ||
            !int.TryParse(difficultyText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var difficulty) ||
            !int.TryParse(diceQuantityText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var diceQuantity))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidActionRollInterpretationRequest", "Interpretation requires requestId, diceValues, difficulty, and diceQuantity.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var diceValues = ParseCsv(diceValuesText)
            .Select(value => int.TryParse(value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var die) ? die : 0)
            .ToArray();

        var result = WerewolfActionRollInterpretationService.Interpret(new WerewolfActionRollInterpretationRequest(
            actionRequestId,
            Array.AsReadOnly(diceValues),
            difficulty,
            diceQuantity));

        if (!result.Succeeded)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(
                        finding.Severity == WerewolfActionRollInterpretationFindingSeverity.Error
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
            result.Findings.Select(finding => new RuleSetRuntimeFinding(
                    finding.Severity == WerewolfActionRollInterpretationFindingSeverity.Error
                        ? RuleSetRuntimeFindingSeverity.Error
                        : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code,
                    finding.Message))
                .ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = result.RequestId,
                ["rawDiceValues"] = string.Join(",", result.RawDiceValues.Select(d => d.ToString(System.Globalization.CultureInfo.InvariantCulture))),
                ["diceQuantity"] = result.DiceQuantity.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["difficulty"] = result.Difficulty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["successCount"] = result.SuccessCount?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["failureClassification"] = result.FailureClassification ?? string.Empty,
                ["botchClassification"] = result.BotchClassification ?? string.Empty,
                ["interpretationStatus"] = result.InterpretationStatus,
                ["serializedInterpretation"] = result.SerializedInterpretation
            });
    }

    private static RuleSetOperationResult ExecuteSpendResource(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
            !request.Inputs.TryGetValue("currentState", out var currentStateText) ||
            !request.Inputs.TryGetValue("expectedRuntimeStateVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("resourceId", out var resourceId) ||
            !request.Inputs.TryGetValue("amount", out var amountText) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
            !int.TryParse(amountText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var amount))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidSpendResourceRequest", "Spend-resource requires requestId, currentState, expectedRuntimeStateVersion, resourceId, and amount.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentState = System.Text.Json.JsonSerializer.Deserialize<WerewolfRuntimeCharacterState>(currentStateText, JsonOptions);
        if (currentState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidCurrentState", "Current state is not a valid WerewolfRuntimeCharacterState.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var result = WerewolfResourceTransitionService.Spend(new WerewolfResourceTransitionRequest(currentState, expectedVersion, requestId, resourceId, amount));

        if (!result.Succeeded || result.NewState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(
                        finding.Severity == WerewolfResourceTransitionFindingSeverity.Error
                            ? RuleSetRuntimeFindingSeverity.Error
                            : RuleSetRuntimeFindingSeverity.Information,
                        finding.Code.ToString(),
                        finding.Message))
                    .ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return ToOperationResult(result);
    }

    private static RuleSetOperationResult ExecuteRecoverResource(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("requestId", out var requestId) ||
            !request.Inputs.TryGetValue("currentState", out var currentStateText) ||
            !request.Inputs.TryGetValue("expectedRuntimeStateVersion", out var expectedVersionText) ||
            !request.Inputs.TryGetValue("resourceId", out var resourceId) ||
            !request.Inputs.TryGetValue("amount", out var amountText) ||
            !int.TryParse(expectedVersionText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var expectedVersion) ||
            !int.TryParse(amountText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var amount))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidRecoverResourceRequest", "Recover-resource requires requestId, currentState, expectedRuntimeStateVersion, resourceId, and amount.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var currentState = System.Text.Json.JsonSerializer.Deserialize<WerewolfRuntimeCharacterState>(currentStateText, JsonOptions);
        if (currentState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidCurrentState", "Current state is not a valid WerewolfRuntimeCharacterState.")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var result = WerewolfResourceTransitionService.Recover(new WerewolfResourceTransitionRequest(currentState, expectedVersion, requestId, resourceId, amount));

        if (!result.Succeeded || result.NewState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(
                        finding.Severity == WerewolfResourceTransitionFindingSeverity.Error
                            ? RuleSetRuntimeFindingSeverity.Error
                            : RuleSetRuntimeFindingSeverity.Information,
                        finding.Code.ToString(),
                        finding.Message))
                    .ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return ToOperationResult(result);
    }

    private static RuleSetOperationResult ToOperationResult(WerewolfResourceTransitionResult result)
    {
        var newState = result.NewState!;
        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(
                    finding.Severity == WerewolfResourceTransitionFindingSeverity.Error
                        ? RuleSetRuntimeFindingSeverity.Error
                        : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code.ToString(),
                    finding.Message))
                .ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = result.RequestId ?? string.Empty,
                ["newState"] = System.Text.Json.JsonSerializer.Serialize(newState, JsonOptions),
                ["newRuntimeStateVersion"] = newState.RuntimeStateVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["previousCurrent"] = result.PreviousCurrent?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["newCurrent"] = result.NewCurrent?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["previousPermanent"] = result.PreviousPermanent?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["newPermanent"] = result.NewPermanent?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
            });
    }

    private static RuleSetOperationResult ExecuteAwardTemporaryRenown(RuleSetOperationRequest request)
    {
        var currentState = GetCurrentRuntimeState(request);
        var expectedVersion = GetExpectedVersion(request);
        var requestId = request.Inputs.GetValueOrDefault("requestId", string.Empty);
        var renownId = request.Inputs.GetValueOrDefault("renownId", string.Empty);
        var amount = ParseInt(request.Inputs.GetValueOrDefault("amount", "0"));

        var result = WerewolfRenownTransitionService.AwardTemporaryRenown(new WerewolfRenownTransitionRequest(currentState, expectedVersion, requestId, renownId, amount, false));

        if (!result.Succeeded || result.NewState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(
                        finding.Severity == WerewolfRenownTransitionFindingSeverity.Error
                            ? RuleSetRuntimeFindingSeverity.Error
                            : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code.ToString(),
                    finding.Message))
                    .ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return ToRenownOperationResult(result);
    }

    private static RuleSetOperationResult ExecuteLoseTemporaryRenown(RuleSetOperationRequest request)
    {
        var currentState = GetCurrentRuntimeState(request);
        var expectedVersion = GetExpectedVersion(request);
        var requestId = request.Inputs.GetValueOrDefault("requestId", string.Empty);
        var renownId = request.Inputs.GetValueOrDefault("renownId", string.Empty);
        var amount = ParseInt(request.Inputs.GetValueOrDefault("amount", "0"));

        var result = WerewolfRenownTransitionService.LoseTemporaryRenown(new WerewolfRenownTransitionRequest(currentState, expectedVersion, requestId, renownId, amount, false));

        if (!result.Succeeded || result.NewState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(
                        finding.Severity == WerewolfRenownTransitionFindingSeverity.Error
                            ? RuleSetRuntimeFindingSeverity.Error
                            : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code.ToString(),
                    finding.Message))
                    .ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return ToRenownOperationResult(result);
    }

    private static RuleSetOperationResult ExecuteConvertTemporaryToPermanentRenown(RuleSetOperationRequest request)
    {
        var currentState = GetCurrentRuntimeState(request);
        var expectedVersion = GetExpectedVersion(request);
        var requestId = request.Inputs.GetValueOrDefault("requestId", string.Empty);
        var renownId = request.Inputs.GetValueOrDefault("renownId", string.Empty);

        var result = WerewolfRenownTransitionService.ConvertTemporaryToPermanent(new WerewolfRenownTransitionRequest(currentState, expectedVersion, requestId, renownId, WerewolfRenownTransitionService.TemporaryToPermanentThreshold, false));

        if (!result.Succeeded || result.NewState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(finding => new RuleSetRuntimeFinding(
                        finding.Severity == WerewolfRenownTransitionFindingSeverity.Error
                            ? RuleSetRuntimeFindingSeverity.Error
                            : RuleSetRuntimeFindingSeverity.Information,
                    finding.Code.ToString(),
                    finding.Message))
                    .ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return ToRenownOperationResult(result);
    }

    private static RuleSetOperationResult ToRenownOperationResult(WerewolfRenownTransitionResult result)
    {
        var newState = result.NewState!;
        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(finding => new RuleSetRuntimeFinding(
                    finding.Severity == WerewolfRenownTransitionFindingSeverity.Error
                        ? RuleSetRuntimeFindingSeverity.Error
                        : RuleSetRuntimeFindingSeverity.Information,
                finding.Code.ToString(),
                finding.Message))
            .ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = result.RequestId ?? string.Empty,
                ["newState"] = System.Text.Json.JsonSerializer.Serialize(newState, JsonOptions),
                ["newRuntimeStateVersion"] = newState.RuntimeStateVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["previousCurrent"] = result.PreviousCurrent?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["newCurrent"] = result.NewCurrent?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["previousPermanent"] = result.PreviousPermanent?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                ["newPermanent"] = result.NewPermanent?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
            });
    }

    private static RuleSetOperationResult ExecuteApplyDamage(RuleSetOperationRequest request)
    {
        var currentState = GetCurrentRuntimeState(request);
        var expectedVersion = GetExpectedVersion(request);
        var requestId = request.Inputs.GetValueOrDefault("requestId", string.Empty);
        var damageTypeText = request.Inputs.GetValueOrDefault("damageType", string.Empty);
        var amountText = request.Inputs.GetValueOrDefault("amount", string.Empty);

        if (!Enum.TryParse<WerewolfDamageCategory>(damageTypeText, true, out var damageType))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidDamageType", $"Invalid damage type: {damageTypeText}")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        if (!int.TryParse(amountText, out var amount) || amount <= 0)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidAmount", "Amount must be a positive integer")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        var result = WerewolfApplyDamageService.ApplyDamage(new WerewolfApplyDamageRequest(
            requestId,
            currentState,
            expectedVersion,
            damageType,
            amount));

        if (!result.Succeeded || result.UpdatedState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, result.ErrorCode ?? "ApplyDamageFailed", f)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "ApplyDamageSucceeded", f)).ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = result.RequestId,
                ["newState"] = System.Text.Json.JsonSerializer.Serialize(result.UpdatedState, JsonOptions),
                ["newRuntimeStateVersion"] = result.UpdatedState.RuntimeStateVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["healthTrack"] = System.Text.Json.JsonSerializer.Serialize(result.HealthTrack, JsonOptions),
                ["currentLevel"] = result.HealthTrack.CurrentLevel.ToString(),
                ["woundPenalty"] = result.HealthTrack.WoundPenalty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["healthState"] = result.HealthTrack.HealthState.ToString(),
                ["fatalDamageType"] = result.HealthTrack.FatalDamageType?.ToString() ?? string.Empty,
                ["isIncapacitated"] = (result.HealthTrack.HealthState == WerewolfHealthState.Incapacitated).ToString(),
                ["isUnconscious"] = (result.HealthTrack.HealthState == WerewolfHealthState.Unconscious).ToString(),
                ["isNearDeath"] = (result.HealthTrack.HealthState == WerewolfHealthState.NearDeath).ToString(),
                ["isDead"] = (result.HealthTrack.HealthState == WerewolfHealthState.Dead).ToString(),
                ["totalDamage"] = result.HealthTrack.TotalDamage.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
    }

    private static RuleSetOperationResult ExecuteRecoverDamage(RuleSetOperationRequest request)
    {
        var currentState = GetCurrentRuntimeState(request);
        var expectedVersion = GetExpectedVersion(request);
        var requestId = request.Inputs.GetValueOrDefault("requestId", string.Empty);
        var damageTypeText = request.Inputs.GetValueOrDefault("damageType", string.Empty);
        var amountText = request.Inputs.GetValueOrDefault("amount", string.Empty);
        var requiresAlternateFormRestText = request.Inputs.GetValueOrDefault("requiresAlternateFormRest", "false");

        if (!Enum.TryParse<WerewolfDamageCategory>(damageTypeText, true, out var damageType))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidDamageType", $"Invalid damage type: {damageTypeText}")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        if (!int.TryParse(amountText, out var amount) || amount <= 0)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidAmount", "Amount must be a positive integer")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        if (!bool.TryParse(requiresAlternateFormRestText, out var requiresAlternateFormRest))
        {
            requiresAlternateFormRest = false;
        }

        var result = WerewolfRecoverDamageService.RecoverDamage(new WerewolfRecoverDamageRequest(
            requestId,
            currentState,
            expectedVersion,
            damageType,
            amount,
            requiresAlternateFormRest));

        if (!result.Succeeded || result.UpdatedState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, result.ErrorCode ?? "RecoverDamageFailed", f)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "RecoverDamageSucceeded", f)).ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = result.RequestId,
                ["newState"] = System.Text.Json.JsonSerializer.Serialize(result.UpdatedState, JsonOptions),
                ["newRuntimeStateVersion"] = result.UpdatedState.RuntimeStateVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["healthTrack"] = System.Text.Json.JsonSerializer.Serialize(result.HealthTrack, JsonOptions),
                ["currentLevel"] = result.HealthTrack.CurrentLevel.ToString(),
                ["woundPenalty"] = result.HealthTrack.WoundPenalty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["healthState"] = result.HealthTrack.HealthState.ToString(),
                ["fatalDamageType"] = result.HealthTrack.FatalDamageType?.ToString() ?? string.Empty,
                ["isIncapacitated"] = (result.HealthTrack.HealthState == WerewolfHealthState.Incapacitated).ToString(),
                ["isUnconscious"] = (result.HealthTrack.HealthState == WerewolfHealthState.Unconscious).ToString(),
                ["isNearDeath"] = (result.HealthTrack.HealthState == WerewolfHealthState.NearDeath).ToString(),
                ["isDead"] = (result.HealthTrack.HealthState == WerewolfHealthState.Dead).ToString(),
                ["totalDamage"] = result.HealthTrack.TotalDamage.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
    }

    private static RuleSetOperationResult ExecuteRegenerate(RuleSetOperationRequest request)
    {
        var currentState = GetCurrentRuntimeState(request);
        var expectedVersion = GetExpectedVersion(request);
        var requestId = request.Inputs.GetValueOrDefault("requestId", string.Empty);
        var damageTypeText = request.Inputs.GetValueOrDefault("damageType", string.Empty);
         var amountText = request.Inputs.GetValueOrDefault("amount", string.Empty);
         var currentTurnText = request.Inputs.GetValueOrDefault("currentTurn", "0");
         var isStressfulText = request.Inputs.GetValueOrDefault("isStressful", "false");
        var requiresAlternateFormRestText = request.Inputs.GetValueOrDefault("requiresAlternateFormRest", "false");
        var vigorDicePoolText = request.Inputs.GetValueOrDefault("vigorDicePool", "0");
        var vigorSuccessesText = request.Inputs.GetValueOrDefault("vigorSuccesses", "0");
        var vigorOnesText = request.Inputs.GetValueOrDefault("vigorOnes", "0");

        if (!Enum.TryParse<WerewolfDamageCategory>(damageTypeText, true, out var damageType))
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidDamageType", $"Invalid damage type: {damageTypeText}")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        if (!int.TryParse(amountText, out var amount) || amount <= 0)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "InvalidAmount", "Amount must be a positive integer")],
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        if (!int.TryParse(currentTurnText, out var currentTurn))
        {
            currentTurn = 0;
        }

        if (!bool.TryParse(isStressfulText, out var isStressful))
        {
            isStressful = false;
        }

        if (!bool.TryParse(requiresAlternateFormRestText, out var requiresAlternateFormRest))
        {
            requiresAlternateFormRest = false;
        }

        if (!int.TryParse(vigorDicePoolText, out var vigorDicePool))
        {
            vigorDicePool = 0;
        }

        if (!int.TryParse(vigorSuccessesText, out var vigorSuccesses))
        {
            vigorSuccesses = 0;
        }

        if (!int.TryParse(vigorOnesText, out var vigorOnes))
        {
            vigorOnes = 0;
        }

        var result = WerewolfRegenerationService.Regenerate(new WerewolfRegenerationRequest(
            requestId,
            currentState,
            expectedVersion,
            damageType,
            amount,
            currentTurn,
            isStressful,
            requiresAlternateFormRest,
            vigorDicePool,
            vigorSuccesses,
            vigorOnes));

        if (!result.Succeeded || result.UpdatedState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, result.ErrorCode ?? "RegenerateFailed", f)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "RegenerateSucceeded", f)).ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = result.RequestId,
                ["newState"] = System.Text.Json.JsonSerializer.Serialize(result.UpdatedState, JsonOptions),
                ["newRuntimeStateVersion"] = result.UpdatedState.RuntimeStateVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["healthTrack"] = System.Text.Json.JsonSerializer.Serialize(result.HealthTrack, JsonOptions),
                ["currentLevel"] = result.HealthTrack.CurrentLevel.ToString(),
                ["woundPenalty"] = result.HealthTrack.WoundPenalty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["healthState"] = result.HealthTrack.HealthState.ToString(),
                ["fatalDamageType"] = result.HealthTrack.FatalDamageType?.ToString() ?? string.Empty,
                ["isIncapacitated"] = (result.HealthTrack.HealthState == WerewolfHealthState.Incapacitated).ToString(),
                ["isUnconscious"] = (result.HealthTrack.HealthState == WerewolfHealthState.Unconscious).ToString(),
                ["isNearDeath"] = (result.HealthTrack.HealthState == WerewolfHealthState.NearDeath).ToString(),
                ["isDead"] = (result.HealthTrack.HealthState == WerewolfHealthState.Dead).ToString(),
                ["totalDamage"] = result.HealthTrack.TotalDamage.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["successes"] = result.Successes?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
            });
    }

    private static RuleSetOperationResult ExecutePermanecerAtivo(RuleSetOperationRequest request)
    {
        var currentState = GetCurrentRuntimeState(request);
        var expectedVersion = GetExpectedVersion(request);
        var requestId = request.Inputs.GetValueOrDefault("requestId", string.Empty);
        var furySuccessesText = request.Inputs.GetValueOrDefault("furySuccesses", "0");
        var furyOnesText = request.Inputs.GetValueOrDefault("furyOnes", "0");

        if (!int.TryParse(furySuccessesText, out var furySuccesses))
        {
            furySuccesses = 0;
        }

        if (!int.TryParse(furyOnesText, out var furyOnes))
        {
            furyOnes = 0;
        }

        var result = WerewolfPermanecerAtivoService.PermanecerAtivo(new WerewolfPermanecerAtivoRequest(
            requestId,
            currentState,
            expectedVersion,
            furySuccesses,
            furyOnes));

        if (!result.Succeeded || result.UpdatedState is null)
        {
            return new RuleSetOperationResult(
                false,
                RuleSetOperationFailureCode.InvalidRequest,
                result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, result.ErrorCode ?? "PermanecerAtivoFailed", f)).ToArray(),
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        return new RuleSetOperationResult(
            true,
            null,
            result.Findings.Select(f => new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "PermanecerAtivoSucceeded", f)).ToArray(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = result.RequestId,
                ["newState"] = System.Text.Json.JsonSerializer.Serialize(result.UpdatedState, JsonOptions),
                ["newRuntimeStateVersion"] = result.UpdatedState.RuntimeStateVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["healthTrack"] = System.Text.Json.JsonSerializer.Serialize(result.HealthTrack, JsonOptions),
                ["currentLevel"] = result.HealthTrack.CurrentLevel.ToString(),
                ["woundPenalty"] = result.HealthTrack.WoundPenalty.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["healthState"] = result.HealthTrack.HealthState.ToString(),
                ["fatalDamageType"] = result.HealthTrack.FatalDamageType?.ToString() ?? string.Empty,
                ["isIncapacitated"] = (result.HealthTrack.HealthState == WerewolfHealthState.Incapacitated).ToString(),
                ["isUnconscious"] = (result.HealthTrack.HealthState == WerewolfHealthState.Unconscious).ToString(),
                ["isNearDeath"] = (result.HealthTrack.HealthState == WerewolfHealthState.NearDeath).ToString(),
                ["isDead"] = (result.HealthTrack.HealthState == WerewolfHealthState.Dead).ToString(),
                ["isSurvived"] = (result.HealthTrack.HealthState == WerewolfHealthState.Survived).ToString(),
                ["totalDamage"] = result.HealthTrack.TotalDamage.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["successes"] = result.Successes?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
            });
    }

    private static WerewolfRuntimeCharacterState GetCurrentRuntimeState(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("newState", out var newStateJson) || string.IsNullOrWhiteSpace(newStateJson))
        {
            throw new ArgumentException("Runtime Renown transition requires a current state payload.", nameof(request));
        }

        var state = System.Text.Json.JsonSerializer.Deserialize<WerewolfRuntimeCharacterState>(newStateJson, JsonOptions);
        if (state is null)
        {
            throw new ArgumentException("Runtime Renown transition state payload is invalid.", nameof(request));
        }

        return state;
    }

    private static int GetExpectedVersion(RuleSetOperationRequest request)
    {
        if (!request.Inputs.TryGetValue("expectedRuntimeStateVersion", out var versionString) || !int.TryParse(versionString, out var version))
        {
            throw new ArgumentException("Runtime Renown transition requires expected runtime state version.", nameof(request));
        }

        return version;
    }

    private static int ParseInt(string value)
    {
        if (int.TryParse(value, out var result))
        {
            return result;
        }

        return 0;
    }
}
