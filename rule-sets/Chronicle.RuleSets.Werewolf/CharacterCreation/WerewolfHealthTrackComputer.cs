namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfHealthTrackComputer
{
    public static WerewolfHealthTrack Compute(
        IReadOnlyList<WerewolfDamageMark> damageMarks,
        bool hasWeakenedImmuneSystem = false,
        bool permanecerAtivoAttempted = false,
        int lastRegenerationTurn = -1)
    {
        ArgumentNullException.ThrowIfNull(damageMarks);

        var bashingCount = 0;
        var lethalCount = 0;
        var aggravatedCount = 0;

        foreach (var mark in damageMarks)
        {
            switch (mark.Category)
            {
                case WerewolfDamageCategory.Bashing:
                    bashingCount += mark.Amount;
                    break;
                case WerewolfDamageCategory.Lethal:
                    lethalCount += mark.Amount;
                    break;
                case WerewolfDamageCategory.Aggravated:
                    aggravatedCount += mark.Amount;
                    break;
            }
        }

        var totalDamage = bashingCount + lethalCount + aggravatedCount;
        var trackCapacity = WerewolfHealthLevelDefinitions.Count;

        var effectiveTotal = totalDamage;
        if (hasWeakenedImmuneSystem && effectiveTotal < 1)
        {
            effectiveTotal = 1;
        }

        var levelIndex = Math.Min(effectiveTotal, trackCapacity - 1);
        var currentLevel = WerewolfHealthLevelDefinitions.All[levelIndex].Name;

        var woundPenalty = levelIndex switch
        {
            0 => 0,
            1 => -1,
            2 => -1,
            3 => -2,
            4 => -2,
            5 => -5,
            _ => 0
        };

        var (healthState, fatalDamageType) = ComputeHealthState(effectiveTotal, bashingCount, lethalCount, aggravatedCount, permanecerAtivoAttempted);

        return new WerewolfHealthTrack(
            damageMarks,
            bashingCount,
            lethalCount,
            aggravatedCount,
            totalDamage,
            healthState,
            fatalDamageType,
            currentLevel,
            woundPenalty,
            null,
            hasWeakenedImmuneSystem,
            permanecerAtivoAttempted,
            lastRegenerationTurn);
    }

    private static (WerewolfHealthState State, WerewolfDamageCategory? FatalType) ComputeHealthState(
        int effectiveTotal,
        int bashingCount,
        int lethalCount,
        int aggravatedCount,
        bool permanecerAtivoAttempted)
    {
        if (effectiveTotal == 0)
        {
            return (WerewolfHealthState.Healthy, null);
        }

        if (effectiveTotal <= 5)
        {
            return (WerewolfHealthState.Wounded, null);
        }

        if (effectiveTotal == 6)
        {
            return (WerewolfHealthState.Incapacitated, null);
        }

        if (effectiveTotal > 6)
        {
            if (aggravatedCount > 0)
            {
                return (WerewolfHealthState.Dead, WerewolfDamageCategory.Aggravated);
            }

            if (lethalCount > 0)
            {
                if (effectiveTotal >= 8)
                {
                    return (WerewolfHealthState.Dead, WerewolfDamageCategory.Lethal);
                }

                return (WerewolfHealthState.NearDeath, WerewolfDamageCategory.Lethal);
            }

            if (bashingCount > 0)
            {
                return (WerewolfHealthState.Unconscious, WerewolfDamageCategory.Bashing);
            }
        }

        return (WerewolfHealthState.Wounded, null);
    }
}
