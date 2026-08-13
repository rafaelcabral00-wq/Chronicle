using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfRenownTransitionService
{
    public const int TemporaryToPermanentThreshold = 10;
    public const int PermanentToTemporaryRatio = 10;

    public static WerewolfRenownTransitionResult AwardTemporaryRenown(WerewolfRenownTransitionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.CurrentState is null)
        {
            return Invalid(WerewolfRenownTransitionErrorCode.MissingState, "Renown transition requires a runtime state.");
        }

        if (request.ExpectedRuntimeStateVersion != request.CurrentState.RuntimeStateVersion)
        {
            return Invalid(WerewolfRenownTransitionErrorCode.StaleRuntimeStateVersion, "Renown transition expected runtime state version does not match current state.");
        }

        if (string.IsNullOrWhiteSpace(request.RenownId))
        {
            return Invalid(WerewolfRenownTransitionErrorCode.MalformedRenownIdentifier, "Renown transition requires a renown identifier.");
        }

        if (!IsSupportedRenown(request.RenownId))
        {
            return Invalid(WerewolfRenownTransitionErrorCode.UnknownRenown, "Renown identifier is not declared by the current slice.");
        }

        if (request.Amount <= 0)
        {
            return Invalid(WerewolfRenownTransitionErrorCode.AmountMissingOrZero, "Renown transition amount must be greater than zero.");
        }

        var (current, permanent) = GetCurrentAndPermanent(request.CurrentState, request.RenownId);
        var newCurrent = current + request.Amount;

        var updated = request.CurrentState with
        {
            RuntimeStateVersion = request.CurrentState.RuntimeStateVersion + 1
        };

        updated = SetRenownValue(updated, request.RenownId, permanent, newCurrent);

        return new WerewolfRenownTransitionResult(
            true,
            updated,
            [new WerewolfRenownTransitionFinding(WerewolfRenownTransitionFindingSeverity.Information, WerewolfRenownTransitionErrorCode.RenownAwarded, "Temporary Renown awarded.")],
            request.RequestId,
            updated.RuntimeStateVersion,
            current,
            newCurrent,
            permanent,
            permanent);
    }

    public static WerewolfRenownTransitionResult LoseTemporaryRenown(WerewolfRenownTransitionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.CurrentState is null)
        {
            return Invalid(WerewolfRenownTransitionErrorCode.MissingState, "Renown transition requires a runtime state.");
        }

        if (request.ExpectedRuntimeStateVersion != request.CurrentState.RuntimeStateVersion)
        {
            return Invalid(WerewolfRenownTransitionErrorCode.StaleRuntimeStateVersion, "Renown transition expected runtime state version does not match current state.");
        }

        if (string.IsNullOrWhiteSpace(request.RenownId))
        {
            return Invalid(WerewolfRenownTransitionErrorCode.MalformedRenownIdentifier, "Renown transition requires a renown identifier.");
        }

        if (!IsSupportedRenown(request.RenownId))
        {
            return Invalid(WerewolfRenownTransitionErrorCode.UnknownRenown, "Renown identifier is not declared by the current slice.");
        }

        if (request.Amount <= 0)
        {
            return Invalid(WerewolfRenownTransitionErrorCode.AmountMissingOrZero, "Renown transition amount must be greater than zero.");
        }

        var (current, permanent) = GetCurrentAndPermanent(request.CurrentState, request.RenownId);

        if (current < request.Amount)
        {
            return Invalid(WerewolfRenownTransitionErrorCode.InsufficientCurrentValue, "Renown transition amount exceeds current temporary Renown.");
        }

        var newCurrent = current - request.Amount;

        var updated = request.CurrentState with
        {
            RuntimeStateVersion = request.CurrentState.RuntimeStateVersion + 1
        };

        updated = SetRenownValue(updated, request.RenownId, permanent, newCurrent);

        return new WerewolfRenownTransitionResult(
            true,
            updated,
            [new WerewolfRenownTransitionFinding(WerewolfRenownTransitionFindingSeverity.Information, WerewolfRenownTransitionErrorCode.RenownLost, "Temporary Renown lost.")],
            request.RequestId,
            updated.RuntimeStateVersion,
            current,
            newCurrent,
            permanent,
            permanent);
    }

    public static WerewolfRenownTransitionResult ConvertTemporaryToPermanent(WerewolfRenownTransitionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.CurrentState is null)
        {
            return Invalid(WerewolfRenownTransitionErrorCode.MissingState, "Renown transition requires a runtime state.");
        }

        if (request.ExpectedRuntimeStateVersion != request.CurrentState.RuntimeStateVersion)
        {
            return Invalid(WerewolfRenownTransitionErrorCode.StaleRuntimeStateVersion, "Renown transition expected runtime state version does not match current state.");
        }

        if (string.IsNullOrWhiteSpace(request.RenownId))
        {
            return Invalid(WerewolfRenownTransitionErrorCode.MalformedRenownIdentifier, "Renown transition requires a renown identifier.");
        }

        if (!IsSupportedRenown(request.RenownId))
        {
            return Invalid(WerewolfRenownTransitionErrorCode.UnknownRenown, "Renown identifier is not declared by the current slice.");
        }

        var (current, permanent) = GetCurrentAndPermanent(request.CurrentState, request.RenownId);

        if (current < TemporaryToPermanentThreshold)
        {
            return Invalid(WerewolfRenownTransitionErrorCode.ConversionBelowThreshold, $"Renown conversion requires at least {TemporaryToPermanentThreshold} temporary Renown.");
        }

        var newPermanent = permanent + 1;
        var newCurrent = 0;

        var updated = request.CurrentState with
        {
            RuntimeStateVersion = request.CurrentState.RuntimeStateVersion + 1
        };

        updated = SetRenownValue(updated, request.RenownId, newPermanent, newCurrent);

        return new WerewolfRenownTransitionResult(
            true,
            updated,
            [new WerewolfRenownTransitionFinding(WerewolfRenownTransitionFindingSeverity.Information, WerewolfRenownTransitionErrorCode.RenownConverted, "Temporary Renown converted to permanent.")],
            request.RequestId,
            updated.RuntimeStateVersion,
            current,
            newCurrent,
            permanent,
            newPermanent);
    }

    private static (int current, int permanent) GetCurrentAndPermanent(WerewolfRuntimeCharacterState state, string renownId)
    {
        return renownId switch
        {
            WerewolfRenownIdentifiers.Glory => (state.GloryCurrent, state.GloryPermanent),
            WerewolfRenownIdentifiers.Honor => (state.HonorCurrent, state.HonorPermanent),
            WerewolfRenownIdentifiers.Wisdom => (state.WisdomCurrent, state.WisdomPermanent),
            _ => (0, 0)
        };
    }

    private static WerewolfRuntimeCharacterState SetRenownValue(WerewolfRuntimeCharacterState state, string renownId, int permanent, int current)
    {
        return renownId switch
        {
            WerewolfRenownIdentifiers.Glory => state with { GloryPermanent = permanent, GloryCurrent = current },
            WerewolfRenownIdentifiers.Honor => state with { HonorPermanent = permanent, HonorCurrent = current },
            WerewolfRenownIdentifiers.Wisdom => state with { WisdomPermanent = permanent, WisdomCurrent = current },
            _ => state
        };
    }

    private static bool IsSupportedRenown(string renownId)
    {
        return WerewolfRenownIdentifiers.Supported.Contains(renownId, StringComparer.Ordinal);
    }

    private static WerewolfRenownTransitionResult Invalid(WerewolfRenownTransitionErrorCode code, string message)
    {
        return new WerewolfRenownTransitionResult(
            false,
            null,
            [new WerewolfRenownTransitionFinding(WerewolfRenownTransitionFindingSeverity.Error, code, message)],
            null,
            null,
            null,
            null,
            null,
            null);
    }
}
