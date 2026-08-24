namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfGiftAdvancementEligibilityService
{
    public static WerewolfGiftAdvancementResult Evaluate(WerewolfGiftAdvancementRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.CurrentState is null)
        {
            return new WerewolfGiftAdvancementResult(false, null, null, null, [new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Error, WerewolfProgressionErrorCode.MissingState, "Runtime state is required for gift advancement evaluation.")]);
        }

        if (string.IsNullOrWhiteSpace(request.CurrentState.PackageId) ||
            string.IsNullOrWhiteSpace(request.CurrentState.PackageVersion))
        {
            return new WerewolfGiftAdvancementResult(false, null, null, null, [new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Error, WerewolfProgressionErrorCode.InvalidPackageBinding, "Package binding is incomplete.")]);
        }

        if (!string.Equals(request.CurrentState.PackageId, WerewolfRuleSetPackage.ProvisionalPackageId, StringComparison.Ordinal) ||
            !string.Equals(request.CurrentState.PackageVersion, WerewolfRuleSetPackage.PackageVersion, StringComparison.Ordinal))
        {
            return new WerewolfGiftAdvancementResult(false, null, null, null, [new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Error, WerewolfProgressionErrorCode.InvalidPackageBinding, "Runtime state is bound to an unexpected package.")]);
        }

        if (string.IsNullOrWhiteSpace(request.GiftKey))
        {
            return new WerewolfGiftAdvancementResult(false, null, null, null, [new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Error, WerewolfProgressionErrorCode.InvalidTraitIdentifier, "Gift key is required.")]);
        }

        var definition = WerewolfGiftCatalog.Get(request.GiftKey);
        if (definition is null)
        {
            return new WerewolfGiftAdvancementResult(false, null, null, "UnknownGift", [new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Error, WerewolfProgressionErrorCode.UnknownTrait, $"Gift '{request.GiftKey}' is not in the catalog.")]);
        }

        if (request.CurrentState.KnownGiftKeys is not null && request.CurrentState.KnownGiftKeys.Contains(request.GiftKey, StringComparer.Ordinal))
        {
            return new WerewolfGiftAdvancementResult(false, null, false, "GiftAlreadyKnown", [new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Error, WerewolfProgressionErrorCode.GiftAlreadyKnown, $"Gift '{request.GiftKey}' is already known.")]);
        }

        var rankValue = request.CurrentState.PackageBinding.TryGetValue("rankValue", out var rankText) && int.TryParse(rankText, out var parsedRank) ? parsedRank : 0;
        if (rankValue < definition.Level)
        {
            return new WerewolfGiftAdvancementResult(false, definition.Level * 3, false, "RankRequirementNotMet", [new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Error, WerewolfProgressionErrorCode.GiftRankRequirementNotMet, $"Gift '{request.GiftKey}' requires Rank {definition.Level}, but character Rank is {rankValue}.")]);
        }

        var isOwnCategory = definition.Category == WerewolfGiftCategory.Breed &&
                            string.Equals(definition.OwnerKey, request.CurrentState.BirthRace, StringComparison.OrdinalIgnoreCase);
        var cost = isOwnCategory ? definition.Level * 3 : definition.Level * 5;

        return new WerewolfGiftAdvancementResult(
            true,
            cost,
            true,
            null,
            [new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Information, WerewolfProgressionErrorCode.GiftEligible, $"Gift '{request.GiftKey}' is eligible. Cost: {cost} XP. Own category: {isOwnCategory}.")]);
    }
}
