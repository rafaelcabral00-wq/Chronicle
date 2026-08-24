namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfCombatDamageRequest(
    string RequestId,
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    int AttackSuccesses,
    string DamageExpression,
    string DamageCategory,
    int? StrengthBonus);

public sealed record WerewolfCombatDamageRollDefinition(
    string RequestId,
    int DamagePoolSize,
    int Difficulty,
    string DamageCategory,
    IReadOnlyList<string> Findings);

public sealed record WerewolfCombatDamageRollResult(
    string RequestId,
    IReadOnlyList<int> DiceValues,
    int DamageSuccesses,
    int TotalDamage,
    IReadOnlyList<string> Findings);

public static class WerewolfCombatDamageService
{
    public static WerewolfCombatDamageRollDefinition DefineDamageRoll(WerewolfCombatDamageRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return new WerewolfCombatDamageRollDefinition(string.Empty, 0, 0, string.Empty, ["RequestId is required"]);
        }

        if (request.CurrentState is null)
        {
            return new WerewolfCombatDamageRollDefinition(request.RequestId, 0, 0, string.Empty, ["CurrentState is required"]);
        }

        if (request.ExpectedRuntimeStateVersion < 1)
        {
            return new WerewolfCombatDamageRollDefinition(request.RequestId, 0, 0, string.Empty, ["ExpectedRuntimeStateVersion must be >= 1"]);
        }

        if (request.CurrentState.RuntimeStateVersion != request.ExpectedRuntimeStateVersion)
        {
            return new WerewolfCombatDamageRollDefinition(request.RequestId, 0, 0, string.Empty, ["Version mismatch"]);
        }

        if (request.AttackSuccesses < 0)
        {
            return new WerewolfCombatDamageRollDefinition(request.RequestId, 0, 0, string.Empty, ["Attack successes cannot be negative"]);
        }

        if (string.IsNullOrWhiteSpace(request.DamageCategory) || !Enum.TryParse(request.DamageCategory, true, out WerewolfDamageCategory _))
        {
            return new WerewolfCombatDamageRollDefinition(request.RequestId, 0, 0, string.Empty, ["Invalid damage category"]);
        }

        var baseDamageDice = EvaluateDamageExpression(request.DamageExpression, request.StrengthBonus ?? 0);
        var extraSuccessDice = request.AttackSuccesses > 1 ? request.AttackSuccesses - 1 : 0;
        var giftDamageBonus = ComputeGiftDamageBonus(request.CurrentState);
        var giftDamageReduction = ComputeGiftDamageReduction(request.CurrentState, request.DamageCategory);
        var damagePoolSize = Math.Max(0, baseDamageDice + extraSuccessDice + giftDamageBonus);

        if (giftDamageBonus != 0)
        {
            findings.Add($"Gift damage bonus: {giftDamageBonus} dice from active Gift effects.");
        }
        if (giftDamageReduction != 0)
        {
            findings.Add($"Gift damage reduction: {giftDamageReduction} dice reduced from active Gift effects.");
        }

        findings.Add($"Damage roll defined: {baseDamageDice} base + {extraSuccessDice} extra success dice + {giftDamageBonus} gift bonus - {giftDamageReduction} gift reduction = {damagePoolSize} dice vs difficulty {DefaultDifficulty}.");

        return new WerewolfCombatDamageRollDefinition(
            request.RequestId,
            damagePoolSize,
            DefaultDifficulty,
            request.DamageCategory,
            findings);
    }

    private static int ComputeGiftDamageBonus(WerewolfRuntimeCharacterState state)
    {
        var activeEffects = WerewolfGiftEffectService.GetSceneValidEffects(state);
        if (activeEffects.Count == 0)
        {
            return 0;
        }

        return activeEffects
            .Where(e => e.EffectKind == WerewolfActiveGiftEffectKind.CombatDamageBonus && e.Magnitude > 0)
            .Sum(e => e.Magnitude);
    }

    private static int ComputeGiftDamageReduction(WerewolfRuntimeCharacterState state, string damageCategory)
    {
        var activeEffects = WerewolfGiftEffectService.GetSceneValidEffects(state);
        if (activeEffects.Count == 0)
        {
            return 0;
        }

        var hasDamageReduction = activeEffects.Any(e => e.EffectKind == WerewolfActiveGiftEffectKind.DamageReduction);
        if (!hasDamageReduction)
        {
            return 0;
        }

        if (!Enum.TryParse(damageCategory, true, out WerewolfDamageCategory category))
        {
            return 0;
        }

        if (category == WerewolfDamageCategory.Aggravated)
        {
            return 0;
        }

        return 1;
    }

    public static WerewolfCombatDamageRollResult InterpretDamageRoll(WerewolfCombatDamageRollDefinition definition, IReadOnlyList<int> diceValues)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(diceValues);

        var findings = new List<string>(definition.Findings);

        if (diceValues.Count != definition.DamagePoolSize)
        {
            findings.Add($"Dice count mismatch: expected {definition.DamagePoolSize}, got {diceValues.Count}.");
            return new WerewolfCombatDamageRollResult(definition.RequestId, diceValues, 0, 0, findings);
        }

        var successes = 0;
        foreach (var die in diceValues)
        {
            if (die >= definition.Difficulty)
            {
                successes++;
            }
        }

        var totalDamage = successes;
        findings.Add($"Damage roll interpreted: {successes} successes from {diceValues.Count} dice = {totalDamage} {definition.DamageCategory} damage.");

        return new WerewolfCombatDamageRollResult(
            definition.RequestId,
            diceValues,
            successes,
            totalDamage,
            findings);
    }

    private static int EvaluateDamageExpression(string? expression, int strengthBonus)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return Math.Max(0, strengthBonus);
        }

        var normalized = expression.Trim().ToLowerInvariant();
        if (normalized == "variable")
        {
            return Math.Max(0, strengthBonus);
        }

        var strengthIndex = normalized.IndexOf("strength", StringComparison.Ordinal);
        if (strengthIndex >= 0)
        {
            var before = normalized.Substring(0, strengthIndex).Trim();
            var after = normalized.Substring(strengthIndex + "strength".Length).Trim();

            int baseValue = 0;
            if (!string.IsNullOrEmpty(before) && int.TryParse(before, out var b))
            {
                baseValue = b;
            }

            int modifier = 0;
            if (!string.IsNullOrEmpty(after))
            {
                if (after.StartsWith('-'))
                {
                    var numStr = after.Substring(1).Trim();
                    if (int.TryParse(numStr, out var m))
                    {
                        modifier = -m;
                    }
                }
                else if (after.StartsWith('+'))
                {
                    var numStr = after.Substring(1).Trim();
                    if (int.TryParse(numStr, out var m))
                    {
                        modifier = m;
                    }
                }
                else if (int.TryParse(after, out var m2))
                {
                    modifier = m2;
                }
            }

            return Math.Max(0, baseValue + modifier + strengthBonus);
        }

        return Math.Max(0, strengthBonus);
    }

    public static int ComputeDamagePool(int successes, int baseDamage)
    {
        var bonusDamage = successes > 1 ? successes - 1 : 0;
        return Math.Max(0, baseDamage + bonusDamage);
    }

    public static int DefaultDifficulty => 6;
}
