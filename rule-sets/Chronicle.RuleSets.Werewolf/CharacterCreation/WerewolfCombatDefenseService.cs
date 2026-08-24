namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfCombatDefenseRequest(
    string RequestId,
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    string DefenseType,
    int? DodgeAbility,
    int? BrawlAbility,
    int? MeleeAbility,
    int? Dexterity,
    int? Stamina);

public sealed record WerewolfCombatDefenseResult(
    bool Succeeded,
    WerewolfRuntimeCharacterState? UpdatedState,
    IReadOnlyList<string> Findings,
    string? RequestId,
    string DefenseType,
    int DefensePool,
    int DefenseDifficulty);

public static class WerewolfCombatDefenseService
{
    public static WerewolfCombatDefenseResult CalculateDefense(WerewolfCombatDefenseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return new WerewolfCombatDefenseResult(false, null, ["RequestId is required"], null, string.Empty, 0, 6);
        }

        if (request.CurrentState is null)
        {
            return new WerewolfCombatDefenseResult(false, null, ["CurrentState is required"], null, string.Empty, 0, 6);
        }

        if (request.ExpectedRuntimeStateVersion < 1)
        {
            return new WerewolfCombatDefenseResult(false, null, ["ExpectedRuntimeStateVersion must be >= 1"], null, string.Empty, 0, 6);
        }

        if (request.CurrentState.RuntimeStateVersion != request.ExpectedRuntimeStateVersion)
        {
            return new WerewolfCombatDefenseResult(false, request.CurrentState, ["Version mismatch"], request.RequestId, string.Empty, 0, 6);
        }

        if (string.IsNullOrWhiteSpace(request.DefenseType))
        {
            return new WerewolfCombatDefenseResult(false, request.CurrentState, ["Defense type is required"], request.RequestId, string.Empty, 0, 6);
        }

        var defenseType = request.DefenseType.ToLowerInvariant();
        var pool = 0;
        var difficulty = 6;

        switch (defenseType)
        {
            case "dodge":
                if (request.Dexterity is null || request.DodgeAbility is null)
                {
                    return new WerewolfCombatDefenseResult(false, request.CurrentState, ["Dodge requires Dexterity and Dodge ability"], request.RequestId, defenseType, 0, difficulty);
                }
                pool = request.Dexterity.Value + request.DodgeAbility.Value;
                findings.Add($"Dodge defense: {request.Dexterity} Dexterity + {request.DodgeAbility} Dodge = {pool}.");
                break;

            case "block":
                if (request.Dexterity is null || request.BrawlAbility is null)
                {
                    return new WerewolfCombatDefenseResult(false, request.CurrentState, ["Block requires Dexterity and Brawl ability"], request.RequestId, defenseType, 0, difficulty);
                }
                pool = request.Dexterity.Value + request.BrawlAbility.Value;
                findings.Add($"Block defense: {request.Dexterity} Dexterity + {request.BrawlAbility} Brawl = {pool}. Ineffective against firearms.");
                break;

            case "parry":
                if (request.Dexterity is null || request.MeleeAbility is null)
                {
                    return new WerewolfCombatDefenseResult(false, request.CurrentState, ["Parry requires Dexterity and Melee ability"], request.RequestId, defenseType, 0, difficulty);
                }
                pool = request.Dexterity.Value + request.MeleeAbility.Value;
                findings.Add($"Parry defense: {request.Dexterity} Dexterity + {request.MeleeAbility} Melee = {pool}.");
                break;

            default:
                return new WerewolfCombatDefenseResult(false, request.CurrentState, ["Unknown defense type"], request.RequestId, defenseType, 0, difficulty);
        }

        if (pool < 0)
        {
            pool = 0;
        }

        var giftDefenseBonus = ComputeGiftDefenseBonus(request.CurrentState);
        if (giftDefenseBonus > 0)
        {
            pool += giftDefenseBonus;
            findings.Add($"Gift defense bonus: +{giftDefenseBonus} dice from active Gift effects.");
        }

        var updatedState = request.CurrentState with
        {
            RuntimeStateVersion = request.CurrentState.RuntimeStateVersion + 1
        };

        return new WerewolfCombatDefenseResult(
            true,
            updatedState,
            findings,
            request.RequestId,
            defenseType,
            pool,
            difficulty);
    }

    private static int ComputeGiftDefenseBonus(WerewolfRuntimeCharacterState state)
    {
        var activeEffects = WerewolfGiftEffectService.GetSceneValidEffects(state);
        if (activeEffects.Count == 0)
        {
            return 0;
        }

        return activeEffects
            .Where(e => e.EffectKind == WerewolfActiveGiftEffectKind.DefenseBonus && e.Magnitude > 0)
            .Sum(e => e.Magnitude);
    }

    public static int ComputeDefensePool(IReadOnlyDictionary<string, int> effectiveAttributes, string attackId, string defenseType)
    {
        ArgumentNullException.ThrowIfNull(effectiveAttributes);
        ArgumentNullException.ThrowIfNull(attackId);
        ArgumentNullException.ThrowIfNull(defenseType);

        var dexterity = effectiveAttributes.GetValueOrDefault(WerewolfAttributeIdentifiers.Dexterity, 0);
        var normalizedDefense = defenseType.ToLowerInvariant();

        if (string.Equals(attackId, WerewolfCombatIdentifiers.Firearm, StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(normalizedDefense, "block", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            if (string.Equals(normalizedDefense, "dodge", StringComparison.OrdinalIgnoreCase))
            {
                return dexterity + effectiveAttributes.GetValueOrDefault(WerewolfAbilityIdentifiers.Dodge, 0);
            }

            if (string.Equals(normalizedDefense, "parry", StringComparison.OrdinalIgnoreCase))
            {
                return dexterity + effectiveAttributes.GetValueOrDefault(WerewolfAbilityIdentifiers.Melee, 0);
            }
        }

        return normalizedDefense switch
        {
            "dodge" => dexterity + effectiveAttributes.GetValueOrDefault(WerewolfAbilityIdentifiers.Dodge, 0),
            "block" => dexterity + effectiveAttributes.GetValueOrDefault(WerewolfAbilityIdentifiers.Brawl, 0),
            "parry" => dexterity + effectiveAttributes.GetValueOrDefault(WerewolfAbilityIdentifiers.Melee, 0),
            _ => 0
        };
    }

    public static int ComputeDefensePool(IReadOnlyDictionary<string, int> effectiveAttributes, string attackId)
    {
        return ComputeDefensePool(effectiveAttributes, attackId, "dodge");
    }

    public static WerewolfCombatDefenseDefinition ResolveDefense(string defenseId)
    {
        if (string.IsNullOrWhiteSpace(defenseId))
        {
            throw new ArgumentException("DefenseId is required.", nameof(defenseId));
        }

        return defenseId.ToLowerInvariant() switch
        {
            "dodge" => new WerewolfCombatDefenseDefinition("dodge", "Line 3086", WerewolfAttributeIdentifiers.Dexterity, WerewolfAbilityIdentifiers.Dodge, 6, null, false, "Successes directly cancel attacker successes"),
            "block" => new WerewolfCombatDefenseDefinition("block", "Line 3087", WerewolfAttributeIdentifiers.Dexterity, WerewolfAbilityIdentifiers.Brawl, 6, null, false, "Ineffective against firearms"),
            "parry" => new WerewolfCombatDefenseDefinition("parry", "Line 3088", WerewolfAttributeIdentifiers.Dexterity, WerewolfAbilityIdentifiers.Melee, 6, null, false, "Requires weapon"),
            _ => throw new ArgumentException($"Unknown defense type: {defenseId}", nameof(defenseId))
        };
    }
}
