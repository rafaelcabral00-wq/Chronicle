namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfApplyDamageService
{
    public static WerewolfApplyDamageResult ApplyDamage(WerewolfApplyDamageRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return new WerewolfApplyDamageResult(false, null, WerewolfHealthLevelDefinitions.All, default!, ["RequestId is required"], "InvalidRequestId", string.Empty);
        }

        if (request.CurrentState is null)
        {
            return new WerewolfApplyDamageResult(false, null, WerewolfHealthLevelDefinitions.All, default!, ["CurrentState is required"], "InvalidState", string.Empty);
        }

        if (request.ExpectedRuntimeStateVersion < 1)
        {
            return new WerewolfApplyDamageResult(false, null, WerewolfHealthLevelDefinitions.All, default!, ["ExpectedRuntimeStateVersion must be >= 1"], "InvalidVersion", string.Empty);
        }

        if (request.Amount <= 0)
        {
            return new WerewolfApplyDamageResult(false, null, WerewolfHealthLevelDefinitions.All, default!, ["Amount must be positive"], "InvalidAmount", string.Empty);
        }

        if (request.CurrentState.RuntimeStateVersion != request.ExpectedRuntimeStateVersion)
        {
            return new WerewolfApplyDamageResult(false, null, WerewolfHealthLevelDefinitions.All, default!, [
                $"Version mismatch: expected {request.ExpectedRuntimeStateVersion}, actual {request.CurrentState.RuntimeStateVersion}"
            ], "StaleVersion", request.RequestId);
        }

        if (!Enum.IsDefined(request.DamageType))
        {
            return new WerewolfApplyDamageResult(false, null, WerewolfHealthLevelDefinitions.All, default!, [$"Invalid damage type: {request.DamageType}"], "InvalidDamageType", request.RequestId);
        }

        if (request.CurrentState.HealthTrack is null)
        {
            return new WerewolfApplyDamageResult(false, null, WerewolfHealthLevelDefinitions.All, default!, ["HealthTrack is not initialized"], "HealthNotInitialized", request.RequestId);
        }

        var currentTrack = request.CurrentState.HealthTrack;
        var newDamageMarks = new List<WerewolfDamageMark>(currentTrack.DamageMarks);

        for (var i = 0; i < request.Amount; i++)
        {
            newDamageMarks.Add(new WerewolfDamageMark(request.DamageType, 1));
        }

        var newBashing = currentTrack.BashingCount + (request.DamageType == WerewolfDamageCategory.Bashing ? request.Amount : 0);
        var newLethal = currentTrack.LethalCount + (request.DamageType == WerewolfDamageCategory.Lethal ? request.Amount : 0);
        var newAggravated = currentTrack.AggravatedCount + (request.DamageType == WerewolfDamageCategory.Aggravated ? request.Amount : 0);
        var newTotalDamage = currentTrack.TotalDamage + request.Amount;

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

        if (newTrack.HealthState == WerewolfHealthState.Unconscious)
        {
            findings.Add("Character has exceeded Incapacitado with bashing damage and is unconscious.");
        }
        else if (newTrack.HealthState == WerewolfHealthState.NearDeath)
        {
            findings.Add("Character has exceeded Incapacitado with lethal damage and is near death. Permanecer Ativo or regeneration may save.");
        }
        else if (newTrack.HealthState == WerewolfHealthState.Dead)
        {
            findings.Add("Character has exceeded Incapacitado with aggravated damage and is dead. Permanecer Ativo may keep them active.");
        }
        else if (newTrack.HealthState == WerewolfHealthState.Incapacitated)
        {
            findings.Add("Character is incapacitated. Additional lethal or aggravated damage results in death threshold exceeded.");
        }

        return new WerewolfApplyDamageResult(
            true,
            updatedState,
            WerewolfHealthLevelDefinitions.All,
            newTrack,
            findings,
            null,
            request.RequestId);
    }
}
