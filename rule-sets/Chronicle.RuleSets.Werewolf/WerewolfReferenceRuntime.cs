using Chronicle.RuleSets.Abstractions.Runtime;

namespace Chronicle.RuleSets.Werewolf;

public sealed class WerewolfReferenceRuntime : IRuleSetRuntime
{
    public const string CreateCharacterOperation = "character-creation.create-character";
    public const string PurchaseAdditionalGiftOperation = "character-creation.purchase-additional-gift";
    public const string ExecuteGiftEffectOperation = "gift-runtime.execute-gift-effect";

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

        return new RuleSetOperationResult(
            false,
            RuleSetOperationFailureCode.OperationNotImplemented,
            [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Information, "OperationNotImplemented", "Werewolf character creation runtime behavior is not implemented yet.")],
            new Dictionary<string, string>(StringComparer.Ordinal));
    }
}
