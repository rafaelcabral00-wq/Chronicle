using Chronicle.RuleSets.Werewolf.CharacterCreation;
using System.Collections.ObjectModel;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfFreebiePurchaseServiceTests
{
    [Fact]
    public void InitialBudgetIsFifteen()
    {
        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(
            new WerewolfCharacterDraftIdentity("draft-1"), 1);

        Assert.Equal(15, draft.FreebieBudgetTotal);
        Assert.Equal(0, draft.FreebieBudgetSpent);
    }

    [Theory]
    [InlineData(WerewolfFreebieCategory.Attribute, "character.attribute.strength", 1, 1, 5, 10)]
    [InlineData(WerewolfFreebieCategory.Ability, "character.ability.crafts", 0, 1, 2, 13)]
    [InlineData(WerewolfFreebieCategory.Background, "character.background.allies", 0, 1, 1, 14)]
    [InlineData(WerewolfFreebieCategory.Gift, "gift.race.homid.master-of-fire", 0, 1, 7, 8)]
    [InlineData(WerewolfFreebieCategory.Rage, "character.resource.rage", 1, 1, 1, 14)]
    [InlineData(WerewolfFreebieCategory.Gnosis, "character.resource.gnosis", 1, 1, 2, 13)]
    [InlineData(WerewolfFreebieCategory.Willpower, "character.resource.willpower", 3, 1, 1, 14)]
    public void SinglePurchaseDeductsCorrectCost(WerewolfFreebieCategory category, string itemId, int current, int increase, int expectedCost, int expectedRemaining)
    {
        var draft = BuildDraftWithRating(category, itemId, current);

        var request = new WerewolfFreebiePurchaseRequest(
            "req-1",
            draft,
            draft.DraftVersion,
            category,
            itemId,
            increase);

        var result = WerewolfFreebiePurchaseService.Purchase(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Draft);
        Assert.Equal(expectedCost, result.LedgerEntry!.Cost);
        Assert.Equal(expectedRemaining, result.RemainingBudget);
        Assert.Equal(draft.FreebieBudgetTotal, result.Draft!.FreebieBudgetTotal);
        Assert.Equal(expectedCost, result.Draft.FreebieBudgetSpent);
    }

    [Fact]
    public void MultiPurchaseAccumulatesDeductions()
    {
        var draft = BuildDraftWithRating(WerewolfFreebieCategory.Attribute, "character.attribute.strength", 1);

        var request1 = new WerewolfFreebiePurchaseRequest("req-1", draft, draft.DraftVersion, WerewolfFreebieCategory.Attribute, "character.attribute.strength", 1);
        var result1 = WerewolfFreebiePurchaseService.Purchase(request1);
        Assert.True(result1.Succeeded);
        Assert.NotNull(result1.Draft);

        var request2 = new WerewolfFreebiePurchaseRequest("req-2", result1.Draft!, result1.Draft!.DraftVersion, WerewolfFreebieCategory.Ability, "character.ability.crafts", 1);
        var result2 = WerewolfFreebiePurchaseService.Purchase(request2);
        Assert.True(result2.Succeeded);
        Assert.NotNull(result2.Draft);

        Assert.Equal(5 + 2, result2.Draft!.FreebieBudgetSpent);
        Assert.Equal(15 - (5 + 2), result2.RemainingBudget);
        Assert.Equal(2, result2.Draft!.FreebieLedger.Count);
    }

    [Fact]
    public void ZeroSpendLeadsToZeroSpentBudget()
    {
        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(
            new WerewolfCharacterDraftIdentity("draft-1"), 1);

        Assert.Equal(0, draft.FreebieBudgetSpent);
        Assert.Equal(15, draft.FreebieBudgetTotal - draft.FreebieBudgetSpent);
    }

    [Fact]
    public void InsufficientBudgetRejectsPurchase()
    {
        var draft = BuildDraftWithRating(WerewolfFreebieCategory.Attribute, "character.attribute.strength", 1);
        draft = draft with { FreebieBudgetSpent = 14 };

        var request = new WerewolfFreebiePurchaseRequest(
            "req-1",
            draft,
            draft.DraftVersion,
            WerewolfFreebieCategory.Attribute,
            "character.attribute.strength",
            1);

        var result = WerewolfFreebiePurchaseService.Purchase(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.Draft);
        Assert.Equal(1, result.RemainingBudget);
    }

    [Fact]
    public void RejectedPurchaseDoesNotMutateState()
    {
        var draft = BuildDraftWithRating(WerewolfFreebieCategory.Attribute, "character.attribute.strength", 1);
        draft = draft with { FreebieBudgetSpent = 14 };

        var request = new WerewolfFreebiePurchaseRequest(
            "req-1",
            draft,
            draft.DraftVersion,
            WerewolfFreebieCategory.Attribute,
            "character.attribute.strength",
            1);

        var result = WerewolfFreebiePurchaseService.Purchase(request);

        Assert.False(result.Succeeded);
        Assert.Equal(14, draft.FreebieBudgetSpent);
        Assert.Equal(1, draft.Attributes["character.attribute.strength"]);
    }

    [Fact]
    public void AttributeMaximumIsEnforced()
    {
        var draft = BuildDraftWithRating(WerewolfFreebieCategory.Attribute, "character.attribute.strength", 5);

        var request = new WerewolfFreebiePurchaseRequest(
            "req-1",
            draft,
            draft.DraftVersion,
            WerewolfFreebieCategory.Attribute,
            "character.attribute.strength",
            1);

        var result = WerewolfFreebiePurchaseService.Purchase(request);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == "MaximumExceeded");
    }

    [Theory]
    [InlineData(3, 1)]
    [InlineData(3, 2)]
    [InlineData(4, 1)]
    public void AbilityRatingsFourAndFiveAreAllowed(int current, int increase)
    {
        var draft = BuildDraftWithRating(WerewolfFreebieCategory.Ability, "character.ability.crafts", current);

        var request = new WerewolfFreebiePurchaseRequest(
            "req-1",
            draft,
            draft.DraftVersion,
            WerewolfFreebieCategory.Ability,
            "character.ability.crafts",
            increase);

        var result = WerewolfFreebiePurchaseService.Purchase(request);

        Assert.True(result.Succeeded);
        Assert.Equal(current + increase, result.Draft!.Abilities["character.ability.crafts"]);
    }

    [Fact]
    public void BackgroundPurchaseSucceeds()
    {
        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(
            new WerewolfCharacterDraftIdentity("draft-1"), 1);

        var request = new WerewolfFreebiePurchaseRequest(
            "req-1",
            draft,
            draft.DraftVersion,
            WerewolfFreebieCategory.Background,
            "character.background.allies",
            1);

        var result = WerewolfFreebiePurchaseService.Purchase(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Draft);
        Assert.Equal(1, result.Draft!.Backgrounds["character.background.allies"]);
        Assert.Equal(1, result.Draft!.FreebieBudgetSpent);
    }

    [Fact]
    public void LevelOneGiftPurchaseSucceeds()
    {
        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(
            new WerewolfCharacterDraftIdentity("draft-1"), 1);

        var request = new WerewolfFreebiePurchaseRequest(
            "req-1",
            draft,
            draft.DraftVersion,
            WerewolfFreebieCategory.Gift,
            "gift.race.homid.master-of-fire",
            1);

        var result = WerewolfFreebiePurchaseService.Purchase(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Draft);
        Assert.Contains("gift.race.homid.master-of-fire", result.Draft!.Gifts, StringComparer.Ordinal);
        Assert.Equal(7, result.Draft!.FreebieBudgetSpent);
    }

    [Fact]
    public void RagePermanentAndCurrentIncreaseTogether()
    {
        var draft = BuildDraftWithRating(WerewolfFreebieCategory.Rage, "character.resource.rage", 1);

        var request = new WerewolfFreebiePurchaseRequest(
            "req-1",
            draft,
            draft.DraftVersion,
            WerewolfFreebieCategory.Rage,
            "character.resource.rage",
            2);

        var result = WerewolfFreebiePurchaseService.Purchase(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Draft);
        Assert.Equal(3, result.Draft!.Resources[WerewolfCharacterResourceIdentifiers.RagePermanent]);
        Assert.Equal(3, result.Draft!.Resources[WerewolfCharacterResourceIdentifiers.RageCurrent]);
    }

    [Fact]
    public void GnosisPermanentAndCurrentIncreaseTogether()
    {
        var draft = BuildDraftWithRating(WerewolfFreebieCategory.Gnosis, "character.resource.gnosis", 1);

        var request = new WerewolfFreebiePurchaseRequest(
            "req-1",
            draft,
            draft.DraftVersion,
            WerewolfFreebieCategory.Gnosis,
            "character.resource.gnosis",
            1);

        var result = WerewolfFreebiePurchaseService.Purchase(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Draft);
        Assert.Equal(2, result.Draft!.Resources[WerewolfCharacterResourceIdentifiers.GnosisPermanent]);
        Assert.Equal(2, result.Draft!.Resources[WerewolfCharacterResourceIdentifiers.GnosisCurrent]);
    }

    [Fact]
    public void WillpowerPermanentAndCurrentIncreaseTogether()
    {
        var draft = BuildDraftWithRating(WerewolfFreebieCategory.Willpower, "character.resource.willpower", 3);

        var request = new WerewolfFreebiePurchaseRequest(
            "req-1",
            draft,
            draft.DraftVersion,
            WerewolfFreebieCategory.Willpower,
            "character.resource.willpower",
            1);

        var result = WerewolfFreebiePurchaseService.Purchase(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Draft);
        Assert.Equal(4, result.Draft!.Resources[WerewolfCharacterResourceIdentifiers.WillpowerPermanent]);
        Assert.Equal(4, result.Draft!.Resources[WerewolfCharacterResourceIdentifiers.WillpowerCurrent]);
    }

    [Fact]
    public void LupusRestrictedAbilityViaBonusPointsSucceeds()
    {
        var draft = BuildLupusDraft();

        var request = new WerewolfFreebiePurchaseRequest(
            "req-1",
            draft,
            draft.DraftVersion,
            WerewolfFreebieCategory.Ability,
            WerewolfAbilityIdentifiers.Crafts,
            1);

        var result = WerewolfFreebiePurchaseService.Purchase(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Draft);
        Assert.Equal(1, result.Draft!.Abilities[WerewolfAbilityIdentifiers.Crafts]);
    }

    [Fact]
    public void CompletionWithValidLedgerSucceeds()
    {
        var draft = BuildCompletionReadyDraft();
        draft = BuildDraftWithRating(WerewolfFreebieCategory.Ability, "character.ability.crafts", 0, draft);

        var purchaseRequest = new WerewolfFreebiePurchaseRequest("req-1", draft, draft.DraftVersion, WerewolfFreebieCategory.Ability, "character.ability.crafts", 1);
        var purchaseResult = WerewolfFreebiePurchaseService.Purchase(purchaseRequest);
        Assert.True(purchaseResult.Succeeded);

        var completedDraft = purchaseResult.Draft! with { Status = WerewolfCharacterDraftStatus.Initialized };
        var completionRequest = new WerewolfCharacterCompletionRequest(completedDraft, completedDraft.DraftVersion);
        var completionResult = WerewolfCharacterCompletionOperation.Complete(completionRequest);

        if (!completionResult.Succeeded)
        {
            var messages = string.Join(", ", completionResult.Findings.Select(f => $"{f.Code}: {f.Message}"));
            Assert.True(completionResult.Succeeded, $"Completion failed: {messages}");
        }
    }

    private static WerewolfInitializedCharacterState BuildCompletionReadyDraft()
    {
        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(
            new WerewolfCharacterDraftIdentity("draft-1"), 1);

        var attributes = new Dictionary<string, int?>(StringComparer.Ordinal);
        foreach (var key in WerewolfAttributeIdentifiers.Supported)
        {
            attributes[key] = 1;
        }

        var abilities = new Dictionary<string, int?>(StringComparer.Ordinal);
        foreach (var key in WerewolfAbilityIdentifiers.Supported)
        {
            abilities[key] = 0;
        }

        var backgrounds = new Dictionary<string, int?>(StringComparer.Ordinal);
        foreach (var key in WerewolfBackgroundIdentifiers.Supported)
        {
            backgrounds[key] = 1;
        }

        var resources = new Dictionary<string, int?>(StringComparer.Ordinal);
        foreach (var key in WerewolfCharacterCreationDraftFactory.GetResourceKeys())
        {
            resources[key] = 1;
        }

        var renown = new Dictionary<string, int?>(StringComparer.Ordinal);
        foreach (var key in WerewolfCharacterCreationDraftFactory.GetRenownKeys())
        {
            renown[key] = 1;
        }

        return draft with
        {
            Status = WerewolfCharacterDraftStatus.Initialized,
            Race = WerewolfRaceIdentifiers.Homid,
            Auspice = WerewolfAuspiceIdentifiers.Ragabash,
            Tribe = WerewolfTribeIdentifiers.BoneGnawers,
            MetisDeformity = null,
            RaceGift = WerewolfInitialGiftIdentifiers.HomidMasterOfFire,
            AuspiceGift = WerewolfInitialGiftIdentifiers.RagabashOpenSeal,
            TribeGift = WerewolfInitialGiftIdentifiers.BoneGnawersCooking,
            AttributePriorityOrder = [WerewolfAttributeCategoryIdentifiers.Physical, WerewolfAttributeCategoryIdentifiers.Social, WerewolfAttributeCategoryIdentifiers.Mental],
            AttributeBudgets = new Dictionary<string, int>(StringComparer.Ordinal),
            AbilityPriorityOrder = [WerewolfAbilityCategoryIdentifiers.Talents, WerewolfAbilityCategoryIdentifiers.Skills, WerewolfAbilityCategoryIdentifiers.Knowledges],
            AbilityBudgets = new Dictionary<string, int>(StringComparer.Ordinal),
            Attributes = attributes,
            Abilities = abilities,
            Backgrounds = backgrounds,
            Gifts = [],
            Resources = resources,
            Renown = renown,
            Rank = "cliath",
            RankValue = 1,
            IdentityName = "Test Character",
            RequiredNextSteps = []
        };
    }

    [Fact]
    public void CompletionRejectsForgedBudgetSpent()
    {
        var draft = BuildCompletionReadyDraft();
        draft = draft with { FreebieBudgetSpent = 5 };

        var request = new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion);
        var result = WerewolfCharacterCompletionOperation.Complete(request);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == WerewolfCharacterCompletionErrorCode.FreebieBudgetLedgerMismatch);
    }

    [Fact]
    public void CompletionRejectsOverspentFreebieBudget()
    {
        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(
            new WerewolfCharacterDraftIdentity("draft-1"), 1);
        draft = draft with { FreebieBudgetSpent = 16 };

        var request = new WerewolfCharacterCompletionRequest(draft, draft.DraftVersion);
        var result = WerewolfCharacterCompletionOperation.Complete(request);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == WerewolfCharacterCompletionErrorCode.FreebieBudgetOverspent);
    }

    [Fact]
    public void PurchaseProducesVersionedImmutableTransition()
    {
        var draft = BuildDraftWithRating(WerewolfFreebieCategory.Attribute, "character.attribute.strength", 1);
        var originalVersion = draft.DraftVersion;
        var originalDraft = draft;

        var request = new WerewolfFreebiePurchaseRequest(
            "req-1",
            draft,
            draft.DraftVersion,
            WerewolfFreebieCategory.Attribute,
            "character.attribute.strength",
            1);

        var result = WerewolfFreebiePurchaseService.Purchase(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Draft);
        Assert.Equal(originalVersion + 1, result.Draft!.DraftVersion);
        Assert.Equal(2, result.Draft!.Attributes["character.attribute.strength"]);
        Assert.Equal(1, originalDraft.Attributes["character.attribute.strength"]);
        Assert.Equal(0, originalDraft.FreebieBudgetSpent);
    }

    [Fact]
    public void RepeatedCallsCannotReuseOriginalBudget()
    {
        var draft = BuildDraftWithRating(WerewolfFreebieCategory.Attribute, "character.attribute.strength", 1);

        var request1 = new WerewolfFreebiePurchaseRequest("req-1", draft, draft.DraftVersion, WerewolfFreebieCategory.Attribute, "character.attribute.strength", 1);
        var result1 = WerewolfFreebiePurchaseService.Purchase(request1);
        Assert.True(result1.Succeeded);
        Assert.NotNull(result1.Draft);
        Assert.Equal(10, result1.RemainingBudget);

        var request2 = new WerewolfFreebiePurchaseRequest("req-2", result1.Draft!, result1.Draft!.DraftVersion, WerewolfFreebieCategory.Attribute, "character.attribute.strength", 1);
        var result2 = WerewolfFreebiePurchaseService.Purchase(request2);
        Assert.True(result2.Succeeded);
        Assert.NotNull(result2.Draft);
        Assert.Equal(5, result2.RemainingBudget);

        var request3 = new WerewolfFreebiePurchaseRequest("req-3", result2.Draft!, result2.Draft!.DraftVersion, WerewolfFreebieCategory.Attribute, "character.attribute.strength", 1);
        var result3 = WerewolfFreebiePurchaseService.Purchase(request3);
        Assert.True(result3.Succeeded);
        Assert.NotNull(result3.Draft);
        Assert.Equal(0, result3.RemainingBudget);

        var request4 = new WerewolfFreebiePurchaseRequest("req-4", result3.Draft!, result3.Draft!.DraftVersion, WerewolfFreebieCategory.Ability, "character.ability.crafts", 1);
        var result4 = WerewolfFreebiePurchaseService.Purchase(request4);
        Assert.False(result4.Succeeded);
        Assert.Equal(0, result4.RemainingBudget);
    }

    private static WerewolfInitializedCharacterState BuildDraftWithRating(WerewolfFreebieCategory category, string itemId, int rating, WerewolfInitializedCharacterState? baseDraft = null)
    {
        var draft = baseDraft ?? WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(
            new WerewolfCharacterDraftIdentity("draft-1"), 1);

        switch (category)
        {
            case WerewolfFreebieCategory.Attribute:
                var attrs = draft.Attributes.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
                attrs[itemId] = rating;
                draft = draft with { Attributes = new ReadOnlyDictionary<string, int?>(attrs) };
                break;
            case WerewolfFreebieCategory.Ability:
                var abilities = draft.Abilities.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
                abilities[itemId] = rating;
                draft = draft with { Abilities = new ReadOnlyDictionary<string, int?>(abilities) };
                break;
            case WerewolfFreebieCategory.Background:
                var bgs = draft.Backgrounds.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
                bgs[itemId] = rating;
                draft = draft with { Backgrounds = new ReadOnlyDictionary<string, int?>(bgs) };
                break;
            case WerewolfFreebieCategory.Gift:
                var gifts = draft.Gifts.ToList();
                if (rating > 0) gifts.Add(itemId);
                draft = draft with { Gifts = Array.AsReadOnly(gifts.ToArray()) };
                break;
            case WerewolfFreebieCategory.Rage:
                var rageResources = draft.Resources.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
                rageResources[WerewolfCharacterResourceIdentifiers.RagePermanent] = rating;
                rageResources[WerewolfCharacterResourceIdentifiers.RageCurrent] = rating;
                draft = draft with { Resources = new ReadOnlyDictionary<string, int?>(rageResources) };
                break;
            case WerewolfFreebieCategory.Gnosis:
                var gnoResources = draft.Resources.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
                gnoResources[WerewolfCharacterResourceIdentifiers.GnosisPermanent] = rating;
                gnoResources[WerewolfCharacterResourceIdentifiers.GnosisCurrent] = rating;
                draft = draft with { Resources = new ReadOnlyDictionary<string, int?>(gnoResources) };
                break;
            case WerewolfFreebieCategory.Willpower:
                var wpResources = draft.Resources.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
                wpResources[WerewolfCharacterResourceIdentifiers.WillpowerPermanent] = rating;
                wpResources[WerewolfCharacterResourceIdentifiers.WillpowerCurrent] = rating;
                draft = draft with { Resources = new ReadOnlyDictionary<string, int?>(wpResources) };
                break;
        }

        return draft;
    }

    [Fact]
    public void EndToEndFifteenPointLifecycleSucceeds()
    {
        var draft = BuildCompletionReadyDraft();

        var request1 = new WerewolfFreebiePurchaseRequest("req-1", draft, draft.DraftVersion, WerewolfFreebieCategory.Ability, "character.ability.crafts", 1);
        var result1 = WerewolfFreebiePurchaseService.Purchase(request1);
        Assert.True(result1.Succeeded);
        Assert.NotNull(result1.Draft);

        var request2 = new WerewolfFreebiePurchaseRequest("req-2", result1.Draft!, result1.Draft!.DraftVersion, WerewolfFreebieCategory.Attribute, "character.attribute.strength", 1);
        var result2 = WerewolfFreebiePurchaseService.Purchase(request2);
        Assert.True(result2.Succeeded);
        Assert.NotNull(result2.Draft);

        var request3 = new WerewolfFreebiePurchaseRequest("req-3", result2.Draft!, result2.Draft!.DraftVersion, WerewolfFreebieCategory.Rage, "character.resource.rage", 2);
        var result3 = WerewolfFreebiePurchaseService.Purchase(request3);
        Assert.True(result3.Succeeded);
        Assert.NotNull(result3.Draft);

        var request4 = new WerewolfFreebiePurchaseRequest("req-4", result3.Draft!, result3.Draft!.DraftVersion, WerewolfFreebieCategory.Background, "character.background.allies", 1);
        var result4 = WerewolfFreebiePurchaseService.Purchase(request4);
        Assert.True(result4.Succeeded);
        Assert.NotNull(result4.Draft);

        Assert.Equal(10, result4.Draft!.FreebieBudgetSpent);
        Assert.Equal(5, result4.RemainingBudget);
        Assert.Equal(4, result4.Draft!.FreebieLedger.Count);
        Assert.Equal(draft.DraftVersion + 4, result4.Draft!.DraftVersion);

        var request5 = new WerewolfFreebiePurchaseRequest("req-5", result4.Draft!, result4.Draft!.DraftVersion, WerewolfFreebieCategory.Attribute, "character.attribute.strength", 1);
        var result5 = WerewolfFreebiePurchaseService.Purchase(request5);
        Assert.True(result5.Succeeded);
        Assert.NotNull(result5.Draft);
        Assert.Equal(0, result5.RemainingBudget);

        var request6 = new WerewolfFreebiePurchaseRequest("req-6", result5.Draft!, result5.Draft!.DraftVersion, WerewolfFreebieCategory.Ability, "character.ability.crafts", 1);
        var result6 = WerewolfFreebiePurchaseService.Purchase(request6);
        Assert.False(result6.Succeeded);
        Assert.Equal(0, result6.RemainingBudget);
    }

    private static WerewolfInitializedCharacterState BuildLupusDraft()
    {
        var draft = WerewolfCharacterCreationDraftFactory.CreateInitializedDraft(
            new WerewolfCharacterDraftIdentity("draft-lupus-1"), 1);

        var abilities = draft.Abilities.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
        abilities[WerewolfAbilityIdentifiers.Crafts] = 0;
        draft = draft with
        {
            Race = WerewolfRaceIdentifiers.Lupus,
            Abilities = new ReadOnlyDictionary<string, int?>(abilities)
        };

        return draft;
    }
}
