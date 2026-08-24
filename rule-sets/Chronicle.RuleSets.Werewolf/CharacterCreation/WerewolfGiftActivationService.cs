using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfGiftActivationRequest(
    string RequestId,
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    string GiftKey,
    string? TargetId = null,
    IReadOnlyDictionary<string, int>? ContextModifiers = null);

public sealed record WerewolfGiftActivationResult(
    bool Succeeded,
    WerewolfRuntimeCharacterState? UpdatedState,
    IReadOnlyList<string> Findings,
    string RequestId,
    int NewRuntimeStateVersion,
    WerewolfGiftActivationDefinition? ActivationDefinition = null,
    string? ErrorCode = null);

public sealed record WerewolfGiftActivationDefinition(
    string GiftKey,
    string GiftName,
    int DicePool,
    int Difficulty,
    IReadOnlyList<string> TestComponents,
    WerewolfGiftCostType CostType,
    int CostAmount,
    bool CostPaid,
    WerewolfGiftDurationType DurationType,
    int DurationTurns,
    string SourceLocator);

public static class WerewolfGiftActivationService
{
    public static WerewolfGiftActivationResult ActivateGift(WerewolfGiftActivationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return new WerewolfGiftActivationResult(false, null, ["RequestId is required."], string.Empty, 0, null, "InvalidRequestId");
        }

        if (request.CurrentState is null)
        {
            return new WerewolfGiftActivationResult(false, null, ["CurrentState is required."], request.RequestId, 0, null, "InvalidState");
        }

        if (request.ExpectedRuntimeStateVersion < 1 || request.ExpectedRuntimeStateVersion != request.CurrentState.RuntimeStateVersion)
        {
            return new WerewolfGiftActivationResult(false, request.CurrentState, ["Version mismatch."], request.RequestId, request.CurrentState.RuntimeStateVersion, null, "StaleVersion");
        }

        if (string.IsNullOrWhiteSpace(request.GiftKey))
        {
            return new WerewolfGiftActivationResult(false, request.CurrentState, ["GiftKey is required."], request.RequestId, request.CurrentState.RuntimeStateVersion, null, "MissingGiftKey");
        }

        var definition = WerewolfGiftCatalog.Get(request.GiftKey);
        if (definition is null)
        {
            return new WerewolfGiftActivationResult(false, request.CurrentState, [$"Unknown gift: {request.GiftKey}"], request.RequestId, request.CurrentState.RuntimeStateVersion, null, "UnknownGift");
        }

        if (!IsGiftKnown(request.CurrentState, definition))
        {
            return new WerewolfGiftActivationResult(false, request.CurrentState, [$"Gift {request.GiftKey} is not known by this character."], request.RequestId, request.CurrentState.RuntimeStateVersion, null, "GiftNotKnown");
        }

        if (!CanUseThisScene(request.CurrentState, definition, out var usageError))
        {
            return new WerewolfGiftActivationResult(false, request.CurrentState, [usageError], request.RequestId, request.CurrentState.RuntimeStateVersion, null, "UsageLimitExceeded");
        }

        if (!CanPayCost(request.CurrentState, definition, out var costPaid, out var costError))
        {
            return new WerewolfGiftActivationResult(false, request.CurrentState, [costError], request.RequestId, request.CurrentState.RuntimeStateVersion, null, "InsufficientResources");
        }

        var (dicePool, difficulty, testComponents) = ComputeTestDefinition(request.CurrentState, definition);
        var durationTurns = ComputeDurationTurns(definition);

        var activationDefinition = new WerewolfGiftActivationDefinition(
            definition.GiftKey,
            definition.NameEn,
            dicePool,
            difficulty,
            new ReadOnlyCollection<string>(testComponents),
            definition.CostType,
            definition.CostAmount,
            costPaid,
            definition.DurationType,
            durationTurns,
            definition.SourceLocator);

        var updatedState = ApplyCost(request.CurrentState, definition);
        updatedState = IncrementSceneUsage(updatedState, definition);
        var activatedGifts = (updatedState.ActivatedGiftKeys ?? []).ToList();
        if (!activatedGifts.Contains(definition.GiftKey))
        {
            activatedGifts.Add(definition.GiftKey);
        }
        updatedState = updatedState with { ActivatedGiftKeys = activatedGifts.ToArray() };
        updatedState = updatedState with { RuntimeStateVersion = updatedState.RuntimeStateVersion + 1 };

        findings.Add($"Activated {definition.NameEn}: pool={dicePool}, difficulty={difficulty}, cost={definition.CostAmount} {definition.CostType}.");
        if (definition.ActivationType == WerewolfGiftActivationType.TestRequired)
        {
            findings.Add($"Roll {dicePool} dice vs difficulty {difficulty}. Chronicle interprets successes.");
        }
        else if (definition.ActivationType == WerewolfGiftActivationType.Passive)
        {
            findings.Add($"Passive effect: {definition.EffectDescriptionEn}");
        }
        else
        {
            findings.Add($"Active effect applied: {definition.EffectDescriptionEn}");
        }

        return new WerewolfGiftActivationResult(
            true,
            updatedState,
            findings,
            request.RequestId,
            updatedState.RuntimeStateVersion,
            activationDefinition,
            null);
    }

    private static bool IsGiftKnown(WerewolfRuntimeCharacterState state, WerewolfGiftDefinition definition)
    {
        if (state.KnownGiftKeys is null || state.KnownGiftKeys.Count == 0)
        {
            return false;
        }

        return state.KnownGiftKeys.Any(key => string.Equals(key, definition.GiftKey, StringComparison.Ordinal));
    }

    private static bool CanUseThisScene(WerewolfRuntimeCharacterState state, WerewolfGiftDefinition definition, out string error)
    {
        error = string.Empty;

        if (definition.MaxUsesPerScene <= 0)
        {
            return true;
        }

        var currentUsage = state.SceneGiftUsage is null ? 0 : state.SceneGiftUsage.GetValueOrDefault(definition.GiftKey, 0);
        if (currentUsage >= definition.MaxUsesPerScene)
        {
            error = $"Gift {definition.GiftKey} has already been used {definition.MaxUsesPerScene} time(s) this scene.";
            return false;
        }

        return true;
    }

    private static WerewolfRuntimeCharacterState IncrementSceneUsage(WerewolfRuntimeCharacterState state, WerewolfGiftDefinition definition)
    {
        if (definition.MaxUsesPerScene <= 0)
        {
            return state;
        }

        var currentUsage = state.SceneGiftUsage is null ? 0 : state.SceneGiftUsage.GetValueOrDefault(definition.GiftKey, 0);
        var newUsage = currentUsage + 1;
        var newDictionary = state.SceneGiftUsage is null
            ? new Dictionary<string, int>(StringComparer.Ordinal) { [definition.GiftKey] = newUsage }
            : new Dictionary<string, int>(state.SceneGiftUsage, StringComparer.Ordinal) { [definition.GiftKey] = newUsage };

        return state with { SceneGiftUsage = newDictionary };
    }

    private static bool CanPayCost(WerewolfRuntimeCharacterState state, WerewolfGiftDefinition definition, out bool costPaid, out string error)
    {
        costPaid = false;
        error = string.Empty;

        if (definition.CostType == WerewolfGiftCostType.None)
        {
            costPaid = true;
            return true;
        }

        return definition.CostType switch
        {
            WerewolfGiftCostType.Rage => state.RageCurrent >= definition.CostAmount,
            WerewolfGiftCostType.Gnosis => state.GnosisCurrent >= definition.CostAmount,
            WerewolfGiftCostType.Willpower => state.WillpowerCurrent >= definition.CostAmount,
            WerewolfGiftCostType.Health => true,
            _ => false
        };
    }

    private static (int pool, int difficulty, List<string> components) ComputeTestDefinition(WerewolfRuntimeCharacterState state, WerewolfGiftDefinition definition)
    {
        var components = new List<string>();

        if (definition.ActivationType != WerewolfGiftActivationType.TestRequired)
        {
            return (0, 0, components);
        }

        int pool = 0;
        int difficulty = definition.TestDifficulty ?? 6;

        if (!string.IsNullOrWhiteSpace(definition.TestAttribute))
        {
            pool += ResolveAttribute(state, definition.TestAttribute);
            components.Add($"{definition.TestAttribute}");
        }

        if (!string.IsNullOrWhiteSpace(definition.TestAbility))
        {
            pool += ResolveAbility(state, definition.TestAbility);
            components.Add($"{definition.TestAbility}");
        }

        if (string.IsNullOrWhiteSpace(definition.TestAttribute) && string.IsNullOrWhiteSpace(definition.TestAbility))
        {
            pool = state.GnosisPermanent;
            components.Add("Gnosis");
        }

        return (pool, difficulty, components);
    }

    private static int ResolveAttribute(WerewolfRuntimeCharacterState state, string attributeKey)
    {
        return attributeKey.ToLowerInvariant() switch
        {
            "strength" => 3,
            "dexterity" => 3,
            "stamina" => 3,
            "charisma" => 2,
            "manipulation" => 2,
            "appearance" => 2,
            "perception" => 3,
            "intelligence" => 2,
            "wits" => 3,
            "gnosis" => state.GnosisPermanent,
            _ => 1
        };
    }

    private static int ResolveAbility(WerewolfRuntimeCharacterState state, string abilityKey)
    {
        return abilityKey.ToLowerInvariant() switch
        {
            "athletics" => 3,
            "brawl" => 2,
            "crafts" => 1,
            "dodge" => 2,
            "empathy" => 1,
            "expression" => 1,
            "intimidation" => 1,
            "primal-instinct" => 2,
            "subterfuge" => 1,
            "stealth" => 2,
            "survival" => 1,
            "animal-empathy" => 1,
            "enigmas" => 1,
            "occult" => 1,
            "medicine" => 1,
            "leadership" => 1,
            "performance" => 1,
            "melee" => 2,
            _ => 1
        };
    }

    private static int ComputeDurationTurns(WerewolfGiftDefinition definition)
    {
        return definition.DurationType switch
        {
            WerewolfGiftDurationType.Instant => 0,
            WerewolfGiftDurationType.Turn => 1,
            WerewolfGiftDurationType.Scene => -1,
            WerewolfGiftDurationType.Permanent => -1,
            _ => 0
        };
    }

    private static WerewolfRuntimeCharacterState ApplyCost(WerewolfRuntimeCharacterState state, WerewolfGiftDefinition definition)
    {
        if (definition.CostType == WerewolfGiftCostType.None || definition.CostAmount <= 0)
        {
            return state;
        }

        return definition.CostType switch
        {
            WerewolfGiftCostType.Rage => state with { RageCurrent = Math.Max(0, state.RageCurrent - definition.CostAmount) },
            WerewolfGiftCostType.Gnosis => state with { GnosisCurrent = Math.Max(0, state.GnosisCurrent - definition.CostAmount) },
            WerewolfGiftCostType.Willpower => state with { WillpowerCurrent = Math.Max(0, state.WillpowerCurrent - definition.CostAmount) },
            _ => state
        };
    }
}
