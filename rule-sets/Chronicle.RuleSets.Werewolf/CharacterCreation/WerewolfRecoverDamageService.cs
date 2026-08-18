namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfRecoverDamageService
{
    public static WerewolfRecoverDamageResult RecoverDamage(WerewolfRecoverDamageRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return new WerewolfRecoverDamageResult(false, null, default!, ["RequestId is required"], "InvalidRequestId", string.Empty);
        }

        if (request.CurrentState is null)
        {
            return new WerewolfRecoverDamageResult(false, null, default!, ["CurrentState is required"], "InvalidState", string.Empty);
        }

        if (request.ExpectedRuntimeStateVersion < 1)
        {
            return new WerewolfRecoverDamageResult(false, null, default!, ["ExpectedRuntimeStateVersion must be >= 1"], "InvalidVersion", string.Empty);
        }

        if (request.Amount <= 0)
        {
            return new WerewolfRecoverDamageResult(false, null, default!, ["Amount must be positive"], "InvalidAmount", string.Empty);
        }

        if (request.CurrentState.RuntimeStateVersion != request.ExpectedRuntimeStateVersion)
        {
            return new WerewolfRecoverDamageResult(false, null, default!, [
                $"Version mismatch: expected {request.ExpectedRuntimeStateVersion}, actual {request.CurrentState.RuntimeStateVersion}"
            ], "StaleVersion", request.RequestId);
        }

        if (!Enum.IsDefined(request.DamageType))
        {
            return new WerewolfRecoverDamageResult(false, null, default!, [$"Invalid damage type: {request.DamageType}"], "InvalidDamageType", request.RequestId);
        }

        if (request.CurrentState.HealthTrack is null)
        {
            return new WerewolfRecoverDamageResult(false, null, default!, ["HealthTrack is not initialized"], "HealthNotInitialized", request.RequestId);
        }

        var currentTrack = request.CurrentState.HealthTrack;

        if (request.DamageType == WerewolfDamageCategory.Aggravated && !request.RequiresAlternateFormRest)
        {
            return new WerewolfRecoverDamageResult(false, null, currentTrack, [
                "Aggravated damage recovery requires alternate form rest per source line 2872."
            ], "AggravatedRestRequired", request.RequestId);
        }

        if (currentTrack.TotalDamage == 0)
        {
            return new WerewolfRecoverDamageResult(false, null, currentTrack, ["No damage to recover"], "NoDamage", request.RequestId);
        }

        var recoverableByType = new Dictionary<WerewolfDamageCategory, int>
        {
            [WerewolfDamageCategory.Bashing] = currentTrack.BashingCount,
            [WerewolfDamageCategory.Lethal] = currentTrack.LethalCount,
            [WerewolfDamageCategory.Aggravated] = currentTrack.AggravatedCount
        };

        if (!recoverableByType.TryGetValue(request.DamageType, out var recoverable) || recoverable == 0)
        {
            return new WerewolfRecoverDamageResult(false, null, currentTrack, [$"No {request.DamageType} damage to recover"], "NoDamageOfType", request.RequestId);
        }

        var actualRecovery = Math.Min(request.Amount, recoverable);
        var newDamageMarks = new List<WerewolfDamageMark>(currentTrack.DamageMarks);

        var remaining = actualRecovery;
        for (var i = newDamageMarks.Count - 1; i >= 0 && remaining > 0; i--)
        {
            if (newDamageMarks[i].Category == request.DamageType)
            {
                if (newDamageMarks[i].Amount > remaining)
                {
                    newDamageMarks[i] = new WerewolfDamageMark(request.DamageType, newDamageMarks[i].Amount - remaining);
                    remaining = 0;
                }
                else
                {
                    remaining -= newDamageMarks[i].Amount;
                    newDamageMarks.RemoveAt(i);
                }
            }
        }

        var newBashing = currentTrack.BashingCount - (request.DamageType == WerewolfDamageCategory.Bashing ? actualRecovery : 0);
        var newLethal = currentTrack.LethalCount - (request.DamageType == WerewolfDamageCategory.Lethal ? actualRecovery : 0);
        var newAggravated = currentTrack.AggravatedCount - (request.DamageType == WerewolfDamageCategory.Aggravated ? actualRecovery : 0);
        var newTotalDamage = currentTrack.TotalDamage - actualRecovery;

        var newTrack = WerewolfHealthTrackComputer.Compute(
            newDamageMarks,
            currentTrack.HasWeakenedImmuneSystem,
            currentTrack.PermanecerAtivoAttempted,
            currentTrack.LastRegenerationTurn);

        var updatedState = request.CurrentState with
        {
            RuntimeStateVersion = request.CurrentState.RuntimeStateVersion + 1,
            HealthTrack = newTrack
        };

        findings.Add($"Recovered {actualRecovery} {request.DamageType} damage.");

        return new WerewolfRecoverDamageResult(
            true,
            updatedState,
            newTrack,
            findings,
            null,
            request.RequestId);
    }
}
