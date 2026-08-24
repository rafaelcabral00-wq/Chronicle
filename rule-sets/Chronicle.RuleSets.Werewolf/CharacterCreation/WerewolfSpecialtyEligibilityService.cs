namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfSpecialtyEligibilityService
{
    public static WerewolfSpecialtyEligibilityResult Evaluate(WerewolfSpecialtyEligibilityRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.TraitType))
        {
            return new WerewolfSpecialtyEligibilityResult(false, false, [new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Error, WerewolfProgressionErrorCode.UnknownTraitType, "Trait type is required.")]);
        }

        if (string.IsNullOrWhiteSpace(request.TraitIdentifier))
        {
            return new WerewolfSpecialtyEligibilityResult(false, false, [new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Error, WerewolfProgressionErrorCode.InvalidTraitIdentifier, "Trait identifier is required.")]);
        }

        if (request.CurrentRating < 0)
        {
            return new WerewolfSpecialtyEligibilityResult(false, false, [new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Error, WerewolfProgressionErrorCode.InvalidCurrentRating, "Current rating must be non-negative.")]);
        }

        var traitType = request.TraitType.Trim().ToLowerInvariant();
        if (traitType != "attribute" && traitType != "ability")
        {
            return new WerewolfSpecialtyEligibilityResult(false, false, [new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Error, WerewolfProgressionErrorCode.UnknownTraitType, $"Trait type '{request.TraitType}' is not valid for specialty eligibility. Only 'attribute' and 'ability' are supported.")]);
        }

        if (request.CurrentRating >= 4)
        {
            return new WerewolfSpecialtyEligibilityResult(true, true, [new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Information, WerewolfProgressionErrorCode.SpecialtyEligible, $"Trait '{request.TraitIdentifier}' at rating {request.CurrentRating} is eligible for specialty selection.")]);
        }

        return new WerewolfSpecialtyEligibilityResult(true, false, [new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Information, WerewolfProgressionErrorCode.SpecialtyNotEligible, $"Trait '{request.TraitIdentifier}' at rating {request.CurrentRating} is not eligible for specialty selection. Minimum rating is 4.")]);
    }
}
