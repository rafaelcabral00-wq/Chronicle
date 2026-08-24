namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfAdvancementCostService
{
    public static WerewolfAdvancementCostResult CalculateCost(WerewolfAdvancementCostRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.CurrentState is null)
        {
            return Invalid(WerewolfProgressionErrorCode.MissingState, "Runtime state is required for advancement cost calculation.");
        }

        if (string.IsNullOrWhiteSpace(request.CurrentState.PackageId) ||
            string.IsNullOrWhiteSpace(request.CurrentState.PackageVersion))
        {
            return Invalid(WerewolfProgressionErrorCode.InvalidPackageBinding, "Package binding is incomplete.");
        }

        if (!string.Equals(request.CurrentState.PackageId, WerewolfRuleSetPackage.ProvisionalPackageId, StringComparison.Ordinal) ||
            !string.Equals(request.CurrentState.PackageVersion, WerewolfRuleSetPackage.PackageVersion, StringComparison.Ordinal))
        {
            return Invalid(WerewolfProgressionErrorCode.InvalidPackageBinding, "Runtime state is bound to an unexpected package.");
        }

        if (string.IsNullOrWhiteSpace(request.TraitType))
        {
            return Invalid(WerewolfProgressionErrorCode.UnknownTraitType, "Trait type is required.");
        }

        if (request.CurrentRating < 0)
        {
            return Invalid(WerewolfProgressionErrorCode.InvalidCurrentRating, "Current rating must be non-negative.");
        }

        var traitType = request.TraitType.Trim().ToLowerInvariant();
        var cost = traitType switch
        {
            "attribute" => request.CurrentRating * 4,
            "ability" => request.CurrentRating * 2,
            "new-ability" => 3,
            "rage" => request.CurrentRating,
            "gnosis" => request.CurrentRating * 2,
            "willpower" => request.CurrentRating,
            "gift" => request.CurrentRating * 3,
            "other-gift" => request.CurrentRating * 5,
            "totem" => 3,
            _ => -1
        };

        if (cost < 0)
        {
            return Invalid(WerewolfProgressionErrorCode.UnknownTraitType, $"Trait type '{request.TraitType}' is not recognized.");
        }

        return new WerewolfAdvancementCostResult(
            true,
            cost,
            [new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Information, WerewolfProgressionErrorCode.CostCalculated, $"Cost for {request.TraitType} at rating {request.CurrentRating} is {cost} XP.")]);
    }

    private static WerewolfAdvancementCostResult Invalid(WerewolfProgressionErrorCode code, string message)
    {
        return new WerewolfAdvancementCostResult(
            false,
            null,
            [new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Error, code, message)]);
    }
}
