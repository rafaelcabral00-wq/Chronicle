using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;

namespace Chronicle.RuleSets.Werewolf;

public sealed class WerewolfReferenceRuntime : IRuleSetRuntime
{
    public const string CreateCharacterOperation = "character-creation.create-character";
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
            new RuleSetOperationDescriptor(PurchaseAdditionalGiftOperation, "additional-gift-purchase", RuleSetOperationStatus.Disabled),
            new RuleSetOperationDescriptor(ExecuteGiftEffectOperation, "runtime-gift-execution", RuleSetOperationStatus.Disabled)
        ]);

    public RuleSetOperationResult Execute(RuleSetOperationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

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
}
