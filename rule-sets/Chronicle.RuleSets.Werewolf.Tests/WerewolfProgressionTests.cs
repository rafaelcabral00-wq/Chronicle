using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Chronicle.RuleSets.Abstractions.Runtime;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfProgressionTests
{
    private static WerewolfRuntimeCharacterState BuildRuntimeState(
        int unspentXp = 10,
        int ragePermanent = 5,
        int rageCurrent = 5,
        int gnosisPermanent = 3,
        int gnosisCurrent = 3,
        int willpowerPermanent = 4,
        int willpowerCurrent = 4,
        int gloryPermanent = 0,
        int gloryCurrent = 0,
        int honorPermanent = 0,
        int honorCurrent = 0,
        int wisdomPermanent = 0,
        int wisdomCurrent = 0,
        int runtimeStateVersion = 1,
        IReadOnlyList<string>? knownGiftKeys = null,
        string? birthRace = null,
        IReadOnlyDictionary<string, string>? packageBinding = null)
    {
        var bindings = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["packageId"] = WerewolfRuleSetPackage.ProvisionalPackageId,
            ["packageVersion"] = WerewolfRuleSetPackage.PackageVersion,
            ["declaredReleaseScope"] = WerewolfRuleSetPackage.DeclaredReleaseScope,
            ["contractVersion"] = "1",
            ["attributes"] = "character.attribute.strength:3,character.attribute.dexterity:3,character.attribute.stamina:3,character.attribute.charisma:3,character.attribute.manipulation:3,character.attribute.appearance:3,character.attribute.perception:3,character.attribute.intelligence:3,character.attribute.wits:3",
            ["abilities"] = "character.ability.alertness:2,character.ability.athletics:1,character.ability.brawl:2,character.ability.dodge:2,character.ability.empathy:1,character.ability.expression:1,character.ability.intimidation:1,character.ability.primal-instinct:1,character.ability.streetwise:1,character.ability.subterfuge:1,character.ability.animal-empathy:1,character.ability.crafts:1,character.ability.drive:1,character.ability.etiquette:1,character.ability.firearms:1,character.ability.leadership:1,character.ability.melee:1,character.ability.performance:1,character.ability.stealth:1,character.ability.survival:1,character.ability.computer:0,character.ability.enigmas:0,character.ability.investigation:0,character.ability.law:0,character.ability.linguistics:0,character.ability.medicine:0,character.ability.occult:0,character.ability.politics:0,character.ability.rituals:0,character.ability.science:0",
            ["rankValue"] = "1"
        };

        if (packageBinding is not null)
        {
            foreach (var kvp in packageBinding)
            {
                bindings[kvp.Key] = kvp.Value;
            }
        }

        return new WerewolfRuntimeCharacterState(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            "draft-1",
            runtimeStateVersion,
            bindings,
            ragePermanent,
            rageCurrent,
            gnosisPermanent,
            gnosisCurrent,
            willpowerPermanent,
            willpowerCurrent,
            gloryPermanent,
            gloryCurrent,
            honorPermanent,
            honorCurrent,
            wisdomPermanent,
            wisdomCurrent,
            birthRace ?? WerewolfRaceIdentifiers.Homid,
            null,
            UnspentXp: unspentXp,
            KnownGiftKeys: knownGiftKeys ?? []);
    }

    [Theory]
    [InlineData("attribute", 3, 12)]
    [InlineData("ability", 3, 6)]
    [InlineData("new-ability", 0, 3)]
    [InlineData("rage", 5, 5)]
    [InlineData("gnosis", 3, 6)]
    [InlineData("willpower", 4, 4)]
    [InlineData("gift", 2, 6)]
    [InlineData("other-gift", 2, 10)]
    [InlineData("totem", 1, 3)]
    public void CalculateCostReturnsExpectedCost(string traitType, int currentRating, int expectedCost)
    {
        var state = BuildRuntimeState();
        var request = new WerewolfAdvancementCostRequest(state, 1, traitType, "test-trait", currentRating);
        var result = WerewolfAdvancementCostService.CalculateCost(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Cost);
        Assert.Equal(expectedCost, result.Cost.Value);
    }

    [Fact]
    public void CalculateCostRejectsUnknownTraitType()
    {
        var state = BuildRuntimeState();
        var request = new WerewolfAdvancementCostRequest(state, 1, "unknown", "test", 1);
        var result = WerewolfAdvancementCostService.CalculateCost(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.Cost);
    }

    [Fact]
    public void CalculateCostRejectsNegativeCurrentRating()
    {
        var state = BuildRuntimeState();
        var request = new WerewolfAdvancementCostRequest(state, 1, "attribute", "test", -1);
        var result = WerewolfAdvancementCostService.CalculateCost(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.Cost);
    }

    [Fact]
    public void AdvanceAttributeSucceedsAndDeductsXp()
    {
        var state = BuildRuntimeState(unspentXp: 12);
        var request = new WerewolfAdvanceTraitRequest(state, 1, "req-1", "attribute", "character.attribute.strength");
        var result = WerewolfAdvancementService.Advance(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(0, result.NewState!.UnspentXp);
        Assert.Equal(2, result.NewState.RuntimeStateVersion);
        Assert.Equal(12, result.XpSpent);
    }

    [Fact]
    public void AdvanceAbilitySucceedsAndDeductsXp()
    {
        var state = BuildRuntimeState(unspentXp: 6);
        var request = new WerewolfAdvanceTraitRequest(state, 1, "req-1", "ability", "character.ability.brawl");
        var result = WerewolfAdvancementService.Advance(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(2, result.NewState!.UnspentXp);
        Assert.Equal(2, result.NewState.RuntimeStateVersion);
    }

    [Fact]
    public void AdvanceNewAbilitySucceedsWithCost3()
    {
        var state = BuildRuntimeState(unspentXp: 3);
        var request = new WerewolfAdvanceTraitRequest(state, 1, "req-1", "new-ability", "character.ability.archery");
        var result = WerewolfAdvancementService.Advance(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(0, result.NewState!.UnspentXp);
        Assert.Equal(2, result.NewState.RuntimeStateVersion);
    }

    [Fact]
    public void AdvanceRageSucceedsAndUpdatesPermanent()
    {
        var state = BuildRuntimeState(unspentXp: 5, ragePermanent: 5);
        var request = new WerewolfAdvanceTraitRequest(state, 1, "req-1", "rage", null);
        var result = WerewolfAdvancementService.Advance(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(6, result.NewState!.RagePermanent);
        Assert.Equal(0, result.NewState.UnspentXp);
        Assert.Equal(2, result.NewState.RuntimeStateVersion);
    }

    [Fact]
    public void AdvanceGnosisSucceedsAndUpdatesPermanent()
    {
        var state = BuildRuntimeState(unspentXp: 6, gnosisPermanent: 3);
        var request = new WerewolfAdvanceTraitRequest(state, 1, "req-1", "gnosis", null);
        var result = WerewolfAdvancementService.Advance(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(4, result.NewState!.GnosisPermanent);
        Assert.Equal(0, result.NewState.UnspentXp);
        Assert.Equal(2, result.NewState.RuntimeStateVersion);
    }

    [Fact]
    public void AdvanceWillpowerSucceedsAndUpdatesPermanent()
    {
        var state = BuildRuntimeState(unspentXp: 4, willpowerPermanent: 4);
        var request = new WerewolfAdvanceTraitRequest(state, 1, "req-1", "willpower", null);
        var result = WerewolfAdvancementService.Advance(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(5, result.NewState!.WillpowerPermanent);
        Assert.Equal(0, result.NewState.UnspentXp);
        Assert.Equal(2, result.NewState.RuntimeStateVersion);
    }

    [Fact]
    public void AdvanceGiftSucceedsAndAddsToKnownGifts()
    {
        var state = BuildRuntimeState(unspentXp: 6, knownGiftKeys: [], birthRace: WerewolfRaceIdentifiers.Homid);
        var request = new WerewolfAdvanceTraitRequest(state, 1, "req-1", "gift", WerewolfGiftIdentifiers.HomidMasterOfFire);
        var result = WerewolfAdvancementService.Advance(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(3, result.NewState!.UnspentXp);
        Assert.Equal(2, result.NewState.RuntimeStateVersion);
        Assert.Contains(WerewolfGiftIdentifiers.HomidMasterOfFire, result.NewState.KnownGiftKeys!);
    }

    [Fact]
    public void AdvanceInsufficientXpReturnsFailureAndPreservesState()
    {
        var state = BuildRuntimeState(unspentXp: 5);
        var request = new WerewolfAdvanceTraitRequest(state, 1, "req-1", "attribute", "character.attribute.strength");
        var result = WerewolfAdvancementService.Advance(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.NewState);
        Assert.Equal(5, result.RemainingXp);
    }

    [Fact]
    public void AdvanceBackgroundReturnsFailureWithBackgroundNotPurchasable()
    {
        var state = BuildRuntimeState(unspentXp: 10);
        var request = new WerewolfAdvanceTraitRequest(state, 1, "req-1", "background", "character.background.allies");
        var result = WerewolfAdvancementService.Advance(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.NewState);
        Assert.Contains(result.Findings, f => f.Code == WerewolfProgressionErrorCode.BackgroundNotPurchasableWithExperience);
    }

    [Fact]
    public void AdvanceTotemReturnsFailureWithTotemUnresolved()
    {
        var state = BuildRuntimeState(unspentXp: 10);
        var request = new WerewolfAdvanceTraitRequest(state, 1, "req-1", "totem", "totem");
        var result = WerewolfAdvancementService.Advance(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.NewState);
        Assert.Contains(result.Findings, f => f.Code == WerewolfProgressionErrorCode.TotemExperienceCostUnresolved);
    }

    [Fact]
    public void AdvanceGiftAlreadyKnownReturnsFailure()
    {
        var state = BuildRuntimeState(unspentXp: 6, knownGiftKeys: [WerewolfGiftIdentifiers.HomidMasterOfFire]);
        var request = new WerewolfAdvanceTraitRequest(state, 1, "req-1", "gift", WerewolfGiftIdentifiers.HomidMasterOfFire);
        var result = WerewolfAdvancementService.Advance(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.NewState);
        Assert.Contains(result.Findings, f => f.Code == WerewolfProgressionErrorCode.GiftAlreadyKnown);
    }

    [Fact]
    public void AdvanceAttributeUnknownTraitReturnsFailure()
    {
        var state = BuildRuntimeState(unspentXp: 12);
        var request = new WerewolfAdvanceTraitRequest(state, 1, "req-1", "attribute", "character.attribute.nonexistent");
        var result = WerewolfAdvancementService.Advance(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.NewState);
        Assert.Contains(result.Findings, f => f.Code == WerewolfProgressionErrorCode.UnknownTrait);
    }

    [Fact]
    public void AdvanceStaleVersionReturnsFailure()
    {
        var state = BuildRuntimeState(unspentXp: 12, runtimeStateVersion: 2);
        var request = new WerewolfAdvanceTraitRequest(state, 1, "req-1", "attribute", "character.attribute.strength");
        var result = WerewolfAdvancementService.Advance(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.NewState);
    }

    [Fact]
    public void EvaluateSpecialtyRating4IsEligible()
    {
        var result = WerewolfSpecialtyEligibilityService.Evaluate(new WerewolfSpecialtyEligibilityRequest("attribute", "character.attribute.strength", 4));

        Assert.True(result.Succeeded);
        Assert.True(result.IsEligible);
    }

    [Fact]
    public void EvaluateSpecialtyRating3IsNotEligible()
    {
        var result = WerewolfSpecialtyEligibilityService.Evaluate(new WerewolfSpecialtyEligibilityRequest("ability", "character.ability.brawl", 3));

        Assert.True(result.Succeeded);
        Assert.False(result.IsEligible);
    }

    [Fact]
    public void EvaluateSpecialtyInvalidTraitTypeReturnsFailure()
    {
        var result = WerewolfSpecialtyEligibilityService.Evaluate(new WerewolfSpecialtyEligibilityRequest("invalid", "test", 4));

        Assert.False(result.Succeeded);
        Assert.False(result.IsEligible);
    }

    [Fact]
    public void EvaluateSpecialtyEmptyIdentifierReturnsFailure()
    {
        var result = WerewolfSpecialtyEligibilityService.Evaluate(new WerewolfSpecialtyEligibilityRequest("attribute", "", 4));

        Assert.False(result.Succeeded);
        Assert.False(result.IsEligible);
    }

    [Fact]
    public void EvaluateGiftOwnCategoryEligibleWithCorrectCost()
    {
        var state = BuildRuntimeState(birthRace: WerewolfRaceIdentifiers.Homid, packageBinding: new Dictionary<string, string>(StringComparer.Ordinal) { ["rankValue"] = "1" });
        var request = new WerewolfGiftAdvancementRequest(state, 1, WerewolfGiftIdentifiers.HomidMasterOfFire);
        var result = WerewolfGiftAdvancementEligibilityService.Evaluate(request);

        Assert.True(result.Succeeded);
        Assert.True(result.IsEligible);
        Assert.Equal(3, result.Cost);
    }

    [Fact]
    public void EvaluateGiftOtherCategoryEligibleWithHigherCost()
    {
        var state = BuildRuntimeState(birthRace: WerewolfRaceIdentifiers.Homid, packageBinding: new Dictionary<string, string>(StringComparer.Ordinal) { ["rankValue"] = "1" });
        var request = new WerewolfGiftAdvancementRequest(state, 1, WerewolfGiftIdentifiers.TheurgeSpiritSpeech);
        var result = WerewolfGiftAdvancementEligibilityService.Evaluate(request);

        Assert.True(result.Succeeded);
        Assert.True(result.IsEligible);
        Assert.Equal(5, result.Cost);
    }

    [Fact]
    public void EvaluateGiftAlreadyKnownReturnsIneligible()
    {
        var state = BuildRuntimeState(knownGiftKeys: [WerewolfGiftIdentifiers.HomidMasterOfFire], birthRace: WerewolfRaceIdentifiers.Homid);
        var request = new WerewolfGiftAdvancementRequest(state, 1, WerewolfGiftIdentifiers.HomidMasterOfFire);
        var result = WerewolfGiftAdvancementEligibilityService.Evaluate(request);

        Assert.False(result.Succeeded);
        Assert.False(result.IsEligible);
        Assert.Equal("GiftAlreadyKnown", result.IneligibilityReason);
    }

    [Fact]
    public void EvaluateGiftUnknownGiftReturnsFailure()
    {
        var state = BuildRuntimeState();
        var request = new WerewolfGiftAdvancementRequest(state, 1, "gift.unknown.unknown");
        var result = WerewolfGiftAdvancementEligibilityService.Evaluate(request);

        Assert.False(result.Succeeded);
        Assert.Equal("UnknownGift", result.IneligibilityReason);
    }

    [Fact]
    public void ReferenceRuntimeRegistersProgressionOperations()
    {
        var runtime = new WerewolfReferenceRuntime();
        var metadata = runtime.Metadata;

        var operationKeys = metadata.Operations.Select(o => o.OperationKey).ToList();
        Assert.Contains(WerewolfReferenceRuntime.CalculateAdvancementCostOperation, operationKeys);
        Assert.Contains(WerewolfReferenceRuntime.AdvanceTraitOperation, operationKeys);
        Assert.Contains(WerewolfReferenceRuntime.EvaluateSpecialtyEligibilityOperation, operationKeys);
        Assert.Contains(WerewolfReferenceRuntime.EvaluateGiftAdvancementOperation, operationKeys);
    }

    [Fact]
    public void AdvanceAttributeStoresRatingInPostCreationDictionaryAndPreservesPackageBinding()
    {
        var state = BuildRuntimeState(unspentXp: 12);
        var request = new WerewolfAdvanceTraitRequest(state, 1, "req-1", "attribute", "character.attribute.strength");
        var result = WerewolfAdvancementService.Advance(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(4, result.NewState!.PostCreationAttributeRatings!["character.attribute.strength"]);
        var attrSnapshot = result.NewState.PackageBinding!["attributes"].Split(',').First(e => e.StartsWith("character.attribute.strength:", StringComparison.Ordinal)).Split(':')[1];
        Assert.Equal("3", attrSnapshot);
    }

    [Fact]
    public void AdvanceAbilityStoresRatingInPostCreationDictionaryAndPreservesPackageBinding()
    {
        var state = BuildRuntimeState(unspentXp: 6);
        var request = new WerewolfAdvanceTraitRequest(state, 1, "req-1", "ability", "character.ability.brawl");
        var result = WerewolfAdvancementService.Advance(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(3, result.NewState!.PostCreationAbilityRatings!["character.ability.brawl"]);
        var abilSnapshot = result.NewState.PackageBinding!["abilities"].Split(',').First(e => e.StartsWith("character.ability.brawl:", StringComparison.Ordinal)).Split(':')[1];
        Assert.Equal("2", abilSnapshot);
    }

    [Fact]
    public void ResourceTransitionPreservesUnspentXp()
    {
        var registry = WerewolfTestRuntimeHelpers.RegisteredRuntimeRegistry();
        var state = BuildRuntimeState(rageCurrent: 5, unspentXp: 10);
        var jsonOptions = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = null };
        var stateJson = System.Text.Json.JsonSerializer.Serialize(state, jsonOptions);

        var result = registry.Execute(new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            WerewolfReferenceRuntime.SpendResourceOperation,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = "req-1",
                ["currentState"] = stateJson,
                ["expectedRuntimeStateVersion"] = "1",
                ["resourceId"] = WerewolfCharacterResourceIdentifiers.Rage,
                ["amount"] = "1"
            }));

        Assert.True(result.Succeeded);
        Assert.True(result.Outputs.TryGetValue("newState", out var newStateJson));
        var newState = System.Text.Json.JsonSerializer.Deserialize<WerewolfRuntimeCharacterState>(newStateJson, jsonOptions);
        Assert.NotNull(newState);
        Assert.Equal(10, newState!.UnspentXp);
    }

    [Fact]
    public void AdvancementPreservesRuntimeStateVersionOnRejection()
    {
        var state = BuildRuntimeState(unspentXp: 1);
        var request = new WerewolfAdvanceTraitRequest(state, 1, "req-1", "attribute", "character.attribute.strength");
        var result = WerewolfAdvancementService.Advance(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.NewState);
        Assert.Equal(1, state.RuntimeStateVersion);
    }

    [Fact]
    public void TotemIdentifiersAreVisible()
    {
        Assert.Equal(19, WerewolfTotemIdentifiers.Supported.Count);
    }

    [Fact]
    public void PackDefinitionsAreVisible()
    {
        Assert.Equal("2", WerewolfPackDefinitions.PackTypicalSizeMin);
    }
}
