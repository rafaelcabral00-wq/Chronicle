namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfActionResolutionRequest(
    string RequestId,
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    string AttributeId,
    string AbilityId,
    int BaseDifficulty,
    bool IsDaylightWithoutProtection = false,
    bool IsUnderTension = false,
    bool IsUsingWitheredLimb = false,
    string? SenseBeingTested = null,
    bool IsTracking = false,
    bool IsVisionBased = false,
    bool IsBalanceTest = false);

public sealed record WerewolfActionResolutionDefinition(
    string RequestId,
    int BasePool,
    int DicePoolModifier,
    int FinalPool,
    int BaseDifficulty,
    int DifficultyModifier,
    int FinalDifficulty,
    bool IsActionUnavailable,
    bool IsAutomaticFailure,
    IReadOnlyList<WerewolfConditionalTest> ConditionalTests,
    IReadOnlyList<string> Findings);

public static class WerewolfActionResolutionService
{
    public static WerewolfActionResolutionDefinition ResolveActionTest(WerewolfActionResolutionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<string>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return Blocked(request.RequestId, ["RequestId is required."]);
        }

        if (request.CurrentState is null)
        {
            return Blocked(request.RequestId, ["CurrentState is required."]);
        }

        if (request.CurrentState.RuntimeStateVersion != request.ExpectedRuntimeStateVersion)
        {
            return Blocked(request.RequestId, ["Version mismatch."]);
        }

        if (request.BaseDifficulty < 2 || request.BaseDifficulty > 10)
        {
            return Blocked(request.RequestId, ["Base difficulty must be between 2 and 10."]);
        }

        var metisDeformity = GetMetisDeformity(request.CurrentState);
        var activeConditions = request.CurrentState.Conditions?
            .Where(c => c.IsActive)
            .Select(c => c.ConditionKey)
            .ToList() ?? new List<string>();

        var context = new WerewolfActionResolutionContext(
            request.AttributeId,
            request.AbilityId,
            request.CurrentState.CurrentForm,
            metisDeformity,
            request.IsDaylightWithoutProtection,
            request.IsUnderTension,
            request.IsUsingWitheredLimb,
            request.SenseBeingTested,
            request.IsTracking,
            request.IsVisionBased,
            request.IsBalanceTest,
            activeConditions);

        var modifierResult = WerewolfActionResolutionModifierService.ComputeModifiers(context);
        findings.AddRange(modifierResult.Findings);

        if (modifierResult.IsAutomaticFailure)
        {
            findings.Add("Action results in automatic failure due to deformity.");
            return new WerewolfActionResolutionDefinition(
                request.RequestId,
                0,
                0,
                0,
                request.BaseDifficulty,
                0,
                request.BaseDifficulty,
                false,
                true,
                modifierResult.ConditionalTests,
                findings);
        }

        if (modifierResult.IsActionUnavailable)
        {
            findings.Add("Action is unavailable due to active conditions.");
            return new WerewolfActionResolutionDefinition(
                request.RequestId,
                0,
                0,
                0,
                request.BaseDifficulty,
                0,
                request.BaseDifficulty,
                true,
                false,
                modifierResult.ConditionalTests,
                findings);
        }

        var baseAttribute = GetBaseAttribute(request.CurrentState, request.AttributeId);
        var baseAbility = GetBaseAbility(request.CurrentState, request.AbilityId);
        var basePool = baseAttribute + baseAbility;
        var finalPool = Math.Max(0, basePool + modifierResult.DicePoolModifier);
        var finalDifficulty = Math.Max(2, Math.Min(10, request.BaseDifficulty + modifierResult.DifficultyModifier));

        findings.Add($"Action test resolved: base pool {basePool} (attribute {baseAttribute} + ability {baseAbility}) + dice modifier {modifierResult.DicePoolModifier} = final pool {finalPool}.");
        findings.Add($"Difficulty: base {request.BaseDifficulty} + modifier {modifierResult.DifficultyModifier} = final {finalDifficulty}.");

        return new WerewolfActionResolutionDefinition(
            request.RequestId,
            basePool,
            modifierResult.DicePoolModifier,
            finalPool,
            request.BaseDifficulty,
            modifierResult.DifficultyModifier,
            finalDifficulty,
            false,
            false,
            modifierResult.ConditionalTests,
            findings);
    }

    private static string? GetMetisDeformity(WerewolfRuntimeCharacterState state)
    {
        if (state.PackageBinding.TryGetValue("metis-deformity", out var deformity) && !string.IsNullOrWhiteSpace(deformity))
        {
            return deformity;
        }
        return null;
    }

    private static int GetBaseAttribute(WerewolfRuntimeCharacterState state, string attributeId)
    {
        if (state.PackageBinding.TryGetValue("attributes", out var attrText) && !string.IsNullOrWhiteSpace(attrText))
        {
            try
            {
                var attributes = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(attrText);
                if (attributes is not null && attributes.TryGetValue(attributeId, out var value))
                {
                    return value;
                }
            }
            catch
            {
            }
        }
        return 0;
    }

    private static int GetBaseAbility(WerewolfRuntimeCharacterState state, string abilityId)
    {
        if (state.PackageBinding.TryGetValue("abilities", out var abilText) && !string.IsNullOrWhiteSpace(abilText))
        {
            try
            {
                var abilities = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(abilText);
                if (abilities is not null && abilities.TryGetValue(abilityId, out var value))
                {
                    return value;
                }
            }
            catch
            {
            }
        }
        return 0;
    }

    private static WerewolfActionResolutionDefinition Blocked(string requestId, IReadOnlyList<string> findings)
    {
        return new WerewolfActionResolutionDefinition(
            requestId,
            0,
            0,
            0,
            0,
            0,
            0,
            true,
            false,
            [],
            findings);
    }
}
