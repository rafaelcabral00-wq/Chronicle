namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfAbilityCreationStage
{
    BaseAbilityAllocation,
    FreebieSpending,
    PostCreation
}

public sealed record WerewolfAbilityFreebieEligibilityRequest(
    string RequestId,
    string Race,
    string AbilityId,
    WerewolfAbilityCreationStage CreationStage,
    int CurrentRating,
    int RequestedRatingIncrease);

public sealed record WerewolfAbilityFreebieEligibilityResult(
    bool IsEligible,
    IReadOnlyList<WerewolfAbilityFreebieEligibilityFinding> Findings,
    string? RequestId);

public sealed record WerewolfAbilityFreebieEligibilityFinding(
    WerewolfAbilityFreebieEligibilityFindingSeverity Severity,
    string Code,
    string Message);

public enum WerewolfAbilityFreebieEligibilityFindingSeverity
{
    Information,
    Error
}

public static class WerewolfAbilityFreebieEligibilityService
{
    public static WerewolfAbilityFreebieEligibilityResult CheckEligibility(WerewolfAbilityFreebieEligibilityRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<WerewolfAbilityFreebieEligibilityFinding>();

        if (string.IsNullOrWhiteSpace(request.AbilityId))
        {
            findings.Add(new WerewolfAbilityFreebieEligibilityFinding(
                WerewolfAbilityFreebieEligibilityFindingSeverity.Error,
                "MissingAbilityId",
                "Ability identifier is required."));
            return new WerewolfAbilityFreebieEligibilityResult(false, findings, request.RequestId);
        }

        if (!WerewolfAbilityIdentifiers.Supported.Contains(request.AbilityId, StringComparer.Ordinal))
        {
            findings.Add(new WerewolfAbilityFreebieEligibilityFinding(
                WerewolfAbilityFreebieEligibilityFindingSeverity.Error,
                "UnknownAbility",
                $"Ability identifier '{request.AbilityId}' is not recognized."));
            return new WerewolfAbilityFreebieEligibilityResult(false, findings, request.RequestId);
        }

        if (request.RequestedRatingIncrease <= 0)
        {
            findings.Add(new WerewolfAbilityFreebieEligibilityFinding(
                WerewolfAbilityFreebieEligibilityFindingSeverity.Error,
                "InvalidRatingIncrease",
                "Requested rating increase must be greater than zero."));
            return new WerewolfAbilityFreebieEligibilityResult(false, findings, request.RequestId);
        }

        if (request.CurrentRating < 0)
        {
            findings.Add(new WerewolfAbilityFreebieEligibilityFinding(
                WerewolfAbilityFreebieEligibilityFindingSeverity.Error,
                "InvalidCurrentRating",
                "Current rating cannot be negative."));
            return new WerewolfAbilityFreebieEligibilityResult(false, findings, request.RequestId);
        }

        if (request.CreationStage == WerewolfAbilityCreationStage.BaseAbilityAllocation &&
            StringComparer.Ordinal.Equals(request.Race, WerewolfRaceIdentifiers.Lupus) &&
            WerewolfAbilitySelectionService.LupusBaseRestrictedAbilities.Contains(request.AbilityId, StringComparer.Ordinal))
        {
            findings.Add(new WerewolfAbilityFreebieEligibilityFinding(
                WerewolfAbilityFreebieEligibilityFindingSeverity.Error,
                "RestrictedAbility",
                $"Lupus characters cannot allocate base points to '{request.AbilityId}' during base ability allocation (source line 547)."));
            return new WerewolfAbilityFreebieEligibilityResult(false, findings, request.RequestId);
        }

        findings.Add(new WerewolfAbilityFreebieEligibilityFinding(
            WerewolfAbilityFreebieEligibilityFindingSeverity.Information,
            "Eligible",
            $"Ability '{request.AbilityId}' is eligible for {request.CreationStage} purchase."));

        return new WerewolfAbilityFreebieEligibilityResult(true, findings, request.RequestId);
    }
}
