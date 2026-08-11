namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfResourceTransitionService
{
    public const string SpendResourceOperation = "character-runtime.spend-resource";
    public const string RecoverResourceOperation = "character-runtime.recover-resource";

    public static WerewolfResourceTransitionResult Spend(WerewolfResourceTransitionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Transition(request, isSpend: true);
    }

    public static WerewolfResourceTransitionResult Recover(WerewolfResourceTransitionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Transition(request, isSpend: false);
    }

    private static WerewolfResourceTransitionResult Transition(WerewolfResourceTransitionRequest request, bool isSpend)
    {
        var findings = new List<WerewolfResourceTransitionFinding>();

        if (request.CurrentState is null)
        {
            return Invalid(WerewolfResourceTransitionErrorCode.MissingState, "Runtime state is required for resource transition.", findings);
        }

        if (string.IsNullOrWhiteSpace(request.CurrentState.PackageId) ||
            string.IsNullOrWhiteSpace(request.CurrentState.PackageVersion))
        {
            return Invalid(WerewolfResourceTransitionErrorCode.InvalidPackageBinding, "Package binding is incomplete.", findings);
        }

        if (!string.Equals(request.CurrentState.PackageId, WerewolfRuleSetPackage.ProvisionalPackageId, StringComparison.Ordinal) ||
            !string.Equals(request.CurrentState.PackageVersion, WerewolfRuleSetPackage.PackageVersion, StringComparison.Ordinal))
        {
            return Invalid(WerewolfResourceTransitionErrorCode.InvalidPackageBinding, "Runtime state is bound to an unexpected package.", findings);
        }

        if (string.IsNullOrWhiteSpace(request.CurrentState.DraftId))
        {
            return Invalid(WerewolfResourceTransitionErrorCode.CharacterNotCompleted, "Runtime state must be bound to a completed character draft.", findings);
        }

        if (request.ExpectedRuntimeStateVersion != request.CurrentState.RuntimeStateVersion)
        {
            return Invalid(WerewolfResourceTransitionErrorCode.StaleRuntimeStateVersion, $"Expected runtime state version {request.ExpectedRuntimeStateVersion} does not match current version {request.CurrentState.RuntimeStateVersion}.", findings);
        }

        if (string.IsNullOrWhiteSpace(request.ResourceId))
        {
            return Invalid(WerewolfResourceTransitionErrorCode.MalformedResourceIdentifier, "Resource identifier is required.", findings);
        }

        if (request.ResourceId.EndsWith(".permanent", StringComparison.Ordinal))
        {
            return Invalid(WerewolfResourceTransitionErrorCode.PermanentResourceMutationUnsupported, $"Permanent resource mutation is not supported: '{request.ResourceId}'.", findings);
        }

        var resourceKey = request.ResourceId;
        if (!TryGetResourcePermanent(request.CurrentState, resourceKey, out var permanent) ||
            !TryGetResourceCurrent(request.CurrentState, resourceKey, out var currentRef))
        {
            return Invalid(WerewolfResourceTransitionErrorCode.UnknownResource, $"Resource identifier '{request.ResourceId}' is not recognized.", findings);
        }

        var current = currentRef;

        if (current > permanent)
        {
            return Invalid(WerewolfResourceTransitionErrorCode.InvalidSourceCurrentAbovePermanent, $"Source state has current {current} exceeding permanent {permanent} for {request.ResourceId}.", findings);
        }

        if (request.Amount <= 0)
        {
            return Invalid(WerewolfResourceTransitionErrorCode.AmountMissingOrZero, "Transition amount must be a positive integer.", findings);
        }

        if (isSpend)
        {
            if (current < request.Amount)
            {
                return Invalid(WerewolfResourceTransitionErrorCode.InsufficientCurrentValue, $"Cannot spend {request.Amount} {request.ResourceId}: current is {current}.", findings);
            }

            var newCurrent = current - request.Amount;
            var newState = request.CurrentState with { RuntimeStateVersion = request.CurrentState.RuntimeStateVersion + 1 };

            newState = resourceKey switch
            {
                WerewolfCharacterResourceIdentifiers.Rage => newState with { RageCurrent = newCurrent },
                WerewolfCharacterResourceIdentifiers.Gnosis => newState with { GnosisCurrent = newCurrent },
                WerewolfCharacterResourceIdentifiers.Willpower => newState with { WillpowerCurrent = newCurrent },
                _ => newState
            };

            findings.Add(new WerewolfResourceTransitionFinding(
                WerewolfResourceTransitionFindingSeverity.Information,
                WerewolfResourceTransitionErrorCode.ResourceSpendSucceeded,
                $"Spent {request.Amount} {request.ResourceId}: {current} -> {newCurrent}."));

            return new WerewolfResourceTransitionResult(
                true,
                newState,
                findings.OrderBy(f => f.Code.ToString(), StringComparer.Ordinal).ToArray(),
                request.RequestId,
                newState.RuntimeStateVersion,
                current,
                newCurrent,
                permanent,
                permanent);
        }

        var recoverNewCurrent = current + request.Amount;
        if (recoverNewCurrent > permanent)
        {
            return Invalid(WerewolfResourceTransitionErrorCode.RecoveryExceedsPermanent, $"Cannot recover {request.Amount} {request.ResourceId}: current {current} + amount {request.Amount} would exceed permanent {permanent}.", findings);
        }

        var recoveredState = request.CurrentState with { RuntimeStateVersion = request.CurrentState.RuntimeStateVersion + 1 };

        recoveredState = resourceKey switch
        {
            WerewolfCharacterResourceIdentifiers.Rage => recoveredState with { RageCurrent = recoverNewCurrent },
            WerewolfCharacterResourceIdentifiers.Gnosis => recoveredState with { GnosisCurrent = recoverNewCurrent },
            WerewolfCharacterResourceIdentifiers.Willpower => recoveredState with { WillpowerCurrent = recoverNewCurrent },
            _ => recoveredState
        };

        findings.Add(new WerewolfResourceTransitionFinding(
            WerewolfResourceTransitionFindingSeverity.Information,
            WerewolfResourceTransitionErrorCode.ResourceRecoverSucceeded,
            $"Recovered {request.Amount} {request.ResourceId}: {current} -> {recoverNewCurrent}."));

        return new WerewolfResourceTransitionResult(
            true,
            recoveredState,
            findings.OrderBy(f => f.Code.ToString(), StringComparer.Ordinal).ToArray(),
            request.RequestId,
            recoveredState.RuntimeStateVersion,
            current,
            recoverNewCurrent,
            permanent,
            permanent);
    }

    private static bool TryGetResourcePermanent(WerewolfRuntimeCharacterState state, string resourceId, out int permanent)
    {
        var normalized = NormalizeResourceId(resourceId);
        permanent = normalized switch
        {
            WerewolfCharacterResourceIdentifiers.Rage => state.RagePermanent,
            WerewolfCharacterResourceIdentifiers.Gnosis => state.GnosisPermanent,
            WerewolfCharacterResourceIdentifiers.Willpower => state.WillpowerPermanent,
            _ => -1
        };
        return permanent >= 0;
    }

    private static bool TryGetResourceCurrent(WerewolfRuntimeCharacterState state, string resourceId, out int current)
    {
        var normalized = NormalizeResourceId(resourceId);
        current = normalized switch
        {
            WerewolfCharacterResourceIdentifiers.Rage => state.RageCurrent,
            WerewolfCharacterResourceIdentifiers.Gnosis => state.GnosisCurrent,
            WerewolfCharacterResourceIdentifiers.Willpower => state.WillpowerCurrent,
            _ => -1
        };
        return current >= 0;
    }

    private static string NormalizeResourceId(string resourceId)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
        {
            return resourceId;
        }

        var trimmed = resourceId.Trim();
        if (trimmed.EndsWith(".current", StringComparison.Ordinal))
        {
            return trimmed.Substring(0, trimmed.Length - ".current".Length);
        }

        if (trimmed.EndsWith(".permanent", StringComparison.Ordinal))
        {
            return trimmed.Substring(0, trimmed.Length - ".permanent".Length);
        }

        return trimmed;
    }

    private static WerewolfResourceTransitionResult Invalid(WerewolfResourceTransitionErrorCode code, string message, List<WerewolfResourceTransitionFinding> findings)
    {
        findings.Add(new WerewolfResourceTransitionFinding(WerewolfResourceTransitionFindingSeverity.Error, code, message));
        return new WerewolfResourceTransitionResult(false, null, findings.ToArray(), null, null, null, null, null, null);
    }
}
