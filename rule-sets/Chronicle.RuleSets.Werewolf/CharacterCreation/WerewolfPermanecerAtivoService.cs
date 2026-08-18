namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfPermanecerAtivoRequest(
    string RequestId,
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    int FurySuccesses,
    int FuryOnes);

public sealed record WerewolfPermanecerAtivoResult(
    bool Succeeded,
    WerewolfRuntimeCharacterState? UpdatedState,
    WerewolfHealthTrack HealthTrack,
    IReadOnlyList<string> Findings,
    string? ErrorCode,
    string RequestId,
    int? Successes = null);

public static class WerewolfPermanecerAtivoService
{
    public static WerewolfPermanecerAtivoResult PermanecerAtivo(WerewolfPermanecerAtivoRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return new WerewolfPermanecerAtivoResult(false, null, default!, ["RequestId is required"], "InvalidRequestId", string.Empty);
        }

        if (request.CurrentState is null)
        {
            return new WerewolfPermanecerAtivoResult(false, null, default!, ["CurrentState is required"], "InvalidState", string.Empty);
        }

        if (request.ExpectedRuntimeStateVersion < 1)
        {
            return new WerewolfPermanecerAtivoResult(false, null, default!, ["ExpectedRuntimeStateVersion must be >= 1"], "InvalidVersion", string.Empty);
        }

        if (request.CurrentState.RuntimeStateVersion != request.ExpectedRuntimeStateVersion)
        {
            return new WerewolfPermanecerAtivoResult(false, null, default!, [
                $"Version mismatch: expected {request.ExpectedRuntimeStateVersion}, actual {request.CurrentState.RuntimeStateVersion}"
            ], "StaleVersion", request.RequestId);
        }

        if (request.CurrentState.HealthTrack is null)
        {
            return new WerewolfPermanecerAtivoResult(false, null, default!, ["HealthTrack is not initialized"], "HealthNotInitialized", request.RequestId);
        }

        var currentTrack = request.CurrentState.HealthTrack;
        var furyDicePool = request.CurrentState.RagePermanent;

        if (furyDicePool < 1)
        {
            return new WerewolfPermanecerAtivoResult(false, null, currentTrack, [
                "Permanecer Ativo requires a permanent Fury value of at least 1."
            ], "InsufficientFury", request.RequestId);
        }

        if (currentTrack.HealthState != WerewolfHealthState.NearDeath && currentTrack.HealthState != WerewolfHealthState.Dead)
        {
            return new WerewolfPermanecerAtivoResult(false, null, currentTrack, [
                "Permanecer Ativo is only available when health state is NearDeath or Dead."
            ], "NotEligible", request.RequestId);
        }

        if (currentTrack.PermanecerAtivoAttempted)
        {
            return new WerewolfPermanecerAtivoResult(false, null, currentTrack, [
                "Permanecer Ativo has already been attempted this scene. Limited to one attempt per scene per source line 2870."
            ], "AlreadyAttempted", request.RequestId);
        }

        var finalSuccesses = Math.Max(0, request.FurySuccesses - request.FuryOnes);
        var recoveredLevels = finalSuccesses;

        if (finalSuccesses == 0)
        {
            findings.Add("Permanecer Ativo failed. Character succumbs to injuries.");
            var failedTrack = currentTrack with
            {
                PermanecerAtivoAttempted = true
            };

            var updatedState = request.CurrentState with
            {
                RuntimeStateVersion = request.CurrentState.RuntimeStateVersion + 1,
                HealthTrack = failedTrack
            };

            return new WerewolfPermanecerAtivoResult(
                false,
                updatedState,
                failedTrack,
                findings,
                "Failed",
                request.RequestId,
                0);
        }

        var primaryCategory = ResolvePrimaryDamageCategory(currentTrack);
        var newDamageMarks = new List<WerewolfDamageMark>(currentTrack.DamageMarks);
        var remaining = recoveredLevels;

        for (var i = newDamageMarks.Count - 1; i >= 0 && remaining > 0; i--)
        {
            if (newDamageMarks[i].Category != primaryCategory)
            {
                continue;
            }

            if (newDamageMarks[i].Amount > remaining)
            {
                newDamageMarks[i] = new WerewolfDamageMark(newDamageMarks[i].Category, newDamageMarks[i].Amount - remaining);
                remaining = 0;
            }
            else
            {
                remaining -= newDamageMarks[i].Amount;
                newDamageMarks.RemoveAt(i);
            }
        }

        var survivedTrack = WerewolfHealthTrackComputer.Compute(
            newDamageMarks,
            currentTrack.HasWeakenedImmuneSystem,
            true,
            currentTrack.LastRegenerationTurn);

        var survivedState = request.CurrentState with
        {
            RuntimeStateVersion = request.CurrentState.RuntimeStateVersion + 1,
            HealthTrack = survivedTrack
        };

        findings.Add($"Permanecer Ativo succeeded with {finalSuccesses} successes. Recovered {recoveredLevels - remaining} vitality levels. Character will start next turn in wild frenzy.");

        return new WerewolfPermanecerAtivoResult(
            true,
            survivedState,
            survivedTrack,
            findings,
            null,
            request.RequestId,
            finalSuccesses);
    }

    private static WerewolfDamageCategory ResolvePrimaryDamageCategory(WerewolfHealthTrack track)
    {
        if (track.HealthState == WerewolfHealthState.NearDeath)
        {
            return WerewolfDamageCategory.Lethal;
        }

        if (track.AggravatedCount > 0)
        {
            return WerewolfDamageCategory.Aggravated;
        }

        if (track.LethalCount > 0)
        {
            return WerewolfDamageCategory.Lethal;
        }

        return WerewolfDamageCategory.Bashing;
    }
}
