using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfActionRuntimeTests
{
    private static Dictionary<string, string> BuildInputsFromDraft(WerewolfInitializedCharacterState draft, string requestId, string attributeId, string abilityId, int difficulty, int? modifier)
    {
        var inputs = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["draftId"] = draft.DraftIdentity.Value,
            ["draftVersion"] = draft.DraftVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["expectedDraftVersion"] = draft.DraftVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["requestId"] = requestId,
            ["attributeId"] = attributeId,
            ["abilityId"] = abilityId,
            ["difficulty"] = difficulty.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["modifier"] = modifier?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            ["draftStatus"] = draft.Status.ToString(),
            ["currentRace"] = draft.Race ?? string.Empty,
            ["currentAuspice"] = draft.Auspice ?? string.Empty,
            ["currentTribe"] = draft.Tribe ?? string.Empty,
            ["currentMetisDeformity"] = draft.MetisDeformity ?? string.Empty,
            ["currentRaceGift"] = draft.RaceGift ?? string.Empty,
            ["currentAuspiceGift"] = draft.AuspiceGift ?? string.Empty,
            ["currentTribeGift"] = draft.TribeGift ?? string.Empty,
            ["attributePriorityOrder"] = string.Join(",", draft.AttributePriorityOrder),
            ["attributeBudgets"] = FormatBudgets(draft.AttributeBudgets),
            ["abilityPriorityOrder"] = string.Join(",", draft.AbilityPriorityOrder),
            ["abilityBudgets"] = FormatBudgets(draft.AbilityBudgets),
            ["attributes"] = FormatNullableRatings(draft.Attributes),
            ["abilities"] = FormatNullableRatings(draft.Abilities),
            ["backgrounds"] = FormatNullableRatings(draft.Backgrounds),
            ["resources"] = FormatNullableRatings(draft.Resources),
            ["rankId"] = draft.Rank ?? string.Empty,
            ["rankValue"] = draft.RankValue?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            ["identityName"] = draft.IdentityName ?? string.Empty,
            ["nextSteps"] = string.Join(",", draft.RequiredNextSteps)
        };

        return inputs;
    }

    private static string FormatNullableRatings(IReadOnlyDictionary<string, int?> ratings)
    {
        return string.Join(
            ",",
            ratings
                .Where(entry => entry.Value.HasValue)
                .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .Select(entry => $"{entry.Key}:{entry.Value!.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));
    }

    private static string FormatBudgets(IReadOnlyDictionary<string, int> budgets)
    {
        return string.Join(
            ",",
            budgets
                .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .Select(entry => $"{entry.Key}:{entry.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));
    }

    [Fact]
    public void DefineActionTestSucceedsForCompletedCharacter()
    {
        var registry = WerewolfTestRuntimeHelpers.RegisteredRuntimeRegistry();
        var draft = WerewolfTestRuntimeHelpers.BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash, WerewolfTribeIdentifiers.GlassWalkers);
        var inputs = BuildInputsFromDraft(draft, "action-req-1", WerewolfAttributeIdentifiers.Strength, WerewolfAbilityIdentifiers.Athletics, 6, 0);

        var result = registry.Execute(new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            WerewolfReferenceRuntime.DefineActionTestOperation,
            inputs));

        if (!result.Succeeded)
        {
            var details = string.Join(", ", result.Findings.Select(f => $"{f.Code}: {f.Message}"));
            Assert.Fail($"DefineActionTest failed: {result.FailureCode} - {details}");
        }

        Assert.Equal("action-req-1", result.Outputs["requestId"]);
        Assert.Equal("6", result.Outputs["diceQuantity"]);
        Assert.Equal("10", result.Outputs["diceFaces"]);
    }

    [Fact]
    public void DefineActionTestRejectsNonCompletedCharacter()
    {
        var registry = WerewolfTestRuntimeHelpers.RegisteredRuntimeRegistry();
        var draft = WerewolfTestRuntimeHelpers.BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash, WerewolfTribeIdentifiers.GlassWalkers) with { Status = WerewolfCharacterDraftStatus.Initialized };
        var inputs = BuildInputsFromDraft(draft, "action-req-1", WerewolfAttributeIdentifiers.Strength, WerewolfAbilityIdentifiers.Athletics, 6, 0);

        var result = registry.Execute(new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            WerewolfReferenceRuntime.DefineActionTestOperation,
            inputs));

        Assert.False(result.Succeeded);
        Assert.Equal(RuleSetOperationFailureCode.InvalidRequest, result.FailureCode);
    }

    [Fact]
    public void InterpretActionRollSucceedsForValidRawDice()
    {
        var registry = WerewolfTestRuntimeHelpers.RegisteredRuntimeRegistry();

        var result = registry.Execute(new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            WerewolfReferenceRuntime.InterpretActionRollOperation,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = "action-req-1",
                ["diceValues"] = "3,7,10",
                ["difficulty"] = "6",
                ["diceQuantity"] = "3"
            }));

        Assert.True(result.Succeeded);
        Assert.Equal("action-req-1", result.Outputs["requestId"]);
        Assert.Equal("3,7,10", result.Outputs["rawDiceValues"]);
        Assert.Equal(WerewolfActionRollInterpretationService.PendingExtractionStatus, result.Outputs["interpretationStatus"]);
    }

    [Fact]
    public void InterpretActionRollRejectsInvalidDieFace()
    {
        var registry = WerewolfTestRuntimeHelpers.RegisteredRuntimeRegistry();

        var result = registry.Execute(new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            WerewolfReferenceRuntime.InterpretActionRollOperation,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = "action-req-1",
                ["diceValues"] = "11,5",
                ["difficulty"] = "6",
                ["diceQuantity"] = "2"
            }));

        Assert.False(result.Succeeded);
        Assert.Equal(RuleSetOperationFailureCode.InvalidRequest, result.FailureCode);
    }

    [Fact]
    public void ActionOperationsDoNotGenerateRandomValues()
    {
        var registry = WerewolfTestRuntimeHelpers.RegisteredRuntimeRegistry();
        var draft = WerewolfTestRuntimeHelpers.BuildCompletedDraft(WerewolfRaceIdentifiers.Homid, WerewolfAuspiceIdentifiers.Ragabash, WerewolfTribeIdentifiers.GlassWalkers);
        var inputs = BuildInputsFromDraft(draft, "action-req-1", WerewolfAttributeIdentifiers.Strength, WerewolfAbilityIdentifiers.Athletics, 6, 0);

        var defineResult = registry.Execute(new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            WerewolfReferenceRuntime.DefineActionTestOperation,
            inputs));

        Assert.True(defineResult.Succeeded);
        Assert.DoesNotContain(defineResult.Outputs, kvp => kvp.Key.Contains("random", StringComparison.OrdinalIgnoreCase));
    }
}
