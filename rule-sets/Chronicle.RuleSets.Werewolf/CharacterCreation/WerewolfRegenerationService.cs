namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfRegenerationRequest(
    string RequestId,
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    WerewolfDamageCategory DamageType,
    int Amount,
    int CurrentTurn,
    bool IsStressful = false,
    bool RequiresAlternateFormRest = false,
    int VigorDicePool = 0,
    int VigorSuccesses = 0,
    int VigorOnes = 0);

public sealed record WerewolfRegenerationResult(
    bool Succeeded,
    WerewolfRuntimeCharacterState? UpdatedState,
    WerewolfHealthTrack HealthTrack,
    IReadOnlyList<string> Findings,
    string? ErrorCode,
    string RequestId,
    int? Successes = null);

public static class WerewolfRegenerationService
{
    public static WerewolfRegenerationResult Regenerate(WerewolfRegenerationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return new WerewolfRegenerationResult(false, null, default!, ["RequestId is required"], "InvalidRequestId", string.Empty);
        }

        if (request.CurrentState is null)
        {
            return new WerewolfRegenerationResult(false, null, default!, ["CurrentState is required"], "InvalidState", string.Empty);
        }

        if (request.ExpectedRuntimeStateVersion < 1)
        {
            return new WerewolfRegenerationResult(false, null, default!, ["ExpectedRuntimeStateVersion must be >= 1"], "InvalidVersion", string.Empty);
        }

        if (request.CurrentState.RuntimeStateVersion != request.ExpectedRuntimeStateVersion)
        {
            return new WerewolfRegenerationResult(false, null, default!, [
                $"Version mismatch: expected {request.ExpectedRuntimeStateVersion}, actual {request.CurrentState.RuntimeStateVersion}"
            ], "StaleVersion", request.RequestId);
        }

        if (!Enum.IsDefined(request.DamageType))
        {
            return new WerewolfRegenerationResult(false, null, default!, [$"Invalid damage type: {request.DamageType}"], "InvalidDamageType", request.RequestId);
        }

        if (request.Amount <= 0)
        {
            return new WerewolfRegenerationResult(false, null, default!, ["Amount must be positive"], "InvalidAmount", request.RequestId);
        }

        if (request.CurrentState.HealthTrack is null)
        {
            return new WerewolfRegenerationResult(false, null, default!, ["HealthTrack is not initialized"], "HealthNotInitialized", request.RequestId);
        }

        var currentTrack = request.CurrentState.HealthTrack;

        if (request.CurrentTurn <= currentTrack.LastRegenerationTurn)
        {
            return new WerewolfRegenerationResult(false, null, currentTrack, [
                $"Regeneration already occurred this turn. Last regeneration turn: {currentTrack.LastRegenerationTurn}, current turn: {request.CurrentTurn}."
            ], "AlreadyRegeneratedThisTurn", request.RequestId);
        }

        if (request.DamageType == WerewolfDamageCategory.Aggravated && !request.RequiresAlternateFormRest)
        {
            return new WerewolfRegenerationResult(false, null, currentTrack, [
                "Aggravated damage regeneration requires alternate form rest per source line 2872."
            ], "AggravatedRestRequired", request.RequestId);
        }

        var recoverableByType = new Dictionary<WerewolfDamageCategory, int>
        {
            [WerewolfDamageCategory.Bashing] = currentTrack.BashingCount,
            [WerewolfDamageCategory.Lethal] = currentTrack.LethalCount,
            [WerewolfDamageCategory.Aggravated] = currentTrack.AggravatedCount
        };

        if (!recoverableByType.TryGetValue(request.DamageType, out var recoverable) || recoverable == 0)
        {
            return new WerewolfRegenerationResult(false, null, currentTrack, [$"No {request.DamageType} damage to regenerate"], "NoDamageOfType", request.RequestId);
        }

        var maxRecoveryThisTurn = 1;
        var actualRecovery = Math.Min(Math.Min(request.Amount, recoverable), maxRecoveryThisTurn);
        int? successes = null;

        if (request.DamageType == WerewolfDamageCategory.Lethal && request.IsStressful)
        {
            if (request.VigorDicePool < 1)
            {
                return new WerewolfRegenerationResult(false, null, currentTrack, [
                    "Lethal regeneration in stressful situations requires Vigor dice pool of at least 1."
                ], "InsufficientVigor", request.RequestId);
            }

            var finalSuccesses = Math.Max(0, request.VigorSuccesses - request.VigorOnes);
            successes = finalSuccesses;

            if (finalSuccesses == 0)
            {
                findings.Add($"Lethal regeneration Vigor test failed (0 successes). No damage healed.");
                return new WerewolfRegenerationResult(
                    false,
                    request.CurrentState,
                    currentTrack,
                    findings,
                    "TestFailed",
                    request.RequestId,
                    0);
            }

            actualRecovery = Math.Min(actualRecovery, finalSuccesses);
            findings.Add($"Lethal regeneration Vigor test succeeded with {finalSuccesses} successes. Healing {actualRecovery} level(s).");
        }
        else if (request.DamageType == WerewolfDamageCategory.Bashing)
        {
            findings.Add($"Bashing regeneration proceeds automatically at 1 level per turn. Healing {actualRecovery} level(s).");
        }
        else if (request.DamageType == WerewolfDamageCategory.Aggravated)
        {
            findings.Add($"Aggravated regeneration requires alternate form rest. Healing {actualRecovery} level(s).");
        }
        else
        {
            findings.Add($"Lethal regeneration proceeds automatically outside stressful situations. Healing {actualRecovery} level(s).");
        }

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

        if (remaining > 0)
        {
            for (var i = newDamageMarks.Count - 1; i >= 0 && remaining > 0; i--)
            {
                if (newDamageMarks[i].Category == request.DamageType)
                {
                    newDamageMarks.RemoveAt(i);
                    remaining--;
                }
            }
        }

        var newTrack = WerewolfHealthTrackComputer.Compute(
            newDamageMarks,
            currentTrack.HasWeakenedImmuneSystem,
            currentTrack.PermanecerAtivoAttempted,
            request.CurrentTurn);

        var updatedState = request.CurrentState with
        {
            RuntimeStateVersion = request.CurrentState.RuntimeStateVersion + 1,
            HealthTrack = newTrack
        };

        return new WerewolfRegenerationResult(
            true,
            updatedState,
            newTrack,
            findings,
            null,
            request.RequestId,
            successes);
    }
}
