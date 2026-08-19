namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfFreebieCostCatalogEntry(
    string CategoryId,
    string DisplayName,
    int CostPerUnit,
    int? MaximumRating);

public static class WerewolfFreebieCostCatalog
{
    public static readonly IReadOnlyList<WerewolfFreebieCostCatalogEntry> Entries =
    [
        new WerewolfFreebieCostCatalogEntry("attribute", "Attribute", 5, 5),
        new WerewolfFreebieCostCatalogEntry("ability", "Ability", 2, 5),
        new WerewolfFreebieCostCatalogEntry("background", "Background", 1, 5),
        new WerewolfFreebieCostCatalogEntry("gift", "Gift", 7, 1),
        new WerewolfFreebieCostCatalogEntry("rage", "Rage", 1, null),
        new WerewolfFreebieCostCatalogEntry("gnosis", "Gnosis", 2, null),
        new WerewolfFreebieCostCatalogEntry("willpower", "Willpower", 1, null)
    ];

    public static WerewolfFreebieCostCatalogEntry? GetEntry(string categoryId)
    {
        return Entries.FirstOrDefault(entry => StringComparer.Ordinal.Equals(entry.CategoryId, categoryId));
    }

    public static int GetCost(string categoryId, int unitCount)
    {
        var entry = GetEntry(categoryId);
        if (entry is null)
        {
            throw new KeyNotFoundException($"Freebie cost catalog does not contain category '{categoryId}'.");
        }

        return entry.CostPerUnit * unitCount;
    }
}

public enum WerewolfFreebieCategory
{
    Attribute,
    Ability,
    Background,
    Gift,
    Rage,
    Gnosis,
    Willpower
}

public sealed record WerewolfFreebieEligibilityRequest(
    string RequestId,
    WerewolfFreebieCategory Category,
    string ItemId,
    int CurrentRating,
    int RequestedIncrease,
    int RemainingBudget);

public sealed record WerewolfFreebieEligibilityResult(
    bool IsEligible,
    IReadOnlyList<WerewolfFreebieEligibilityFinding> Findings,
    int CalculatedCost,
    int? RemainingBudgetAfterPurchase,
    string? RequestId);

public sealed record WerewolfFreebieEligibilityFinding(
    WerewolfFreebieEligibilityFindingSeverity Severity,
    string Code,
    string Message);

public enum WerewolfFreebieEligibilityFindingSeverity
{
    Information,
    Error
}

public static class WerewolfFreebieEligibilityService
{
    public static WerewolfFreebieEligibilityResult CheckEligibility(WerewolfFreebieEligibilityRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<WerewolfFreebieEligibilityFinding>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            findings.Add(new WerewolfFreebieEligibilityFinding(WerewolfFreebieEligibilityFindingSeverity.Error, "MissingRequestId", "Request identifier is required."));
            return new WerewolfFreebieEligibilityResult(false, findings, 0, request.RemainingBudget, null);
        }

        if (request.RequestedIncrease <= 0)
        {
            findings.Add(new WerewolfFreebieEligibilityFinding(WerewolfFreebieEligibilityFindingSeverity.Error, "InvalidIncrease", "Requested increase must be greater than zero."));
            return new WerewolfFreebieEligibilityResult(false, findings, 0, request.RemainingBudget, request.RequestId);
        }

        if (request.RemainingBudget < 0)
        {
            findings.Add(new WerewolfFreebieEligibilityFinding(WerewolfFreebieEligibilityFindingSeverity.Error, "InvalidBudget", "Remaining budget cannot be negative."));
            return new WerewolfFreebieEligibilityResult(false, findings, 0, request.RemainingBudget, request.RequestId);
        }

        var categoryId = request.Category.ToString().ToLowerInvariant();
        var entry = WerewolfFreebieCostCatalog.GetEntry(categoryId);
        if (entry is null)
        {
            findings.Add(new WerewolfFreebieEligibilityFinding(WerewolfFreebieEligibilityFindingSeverity.Error, "UnknownCategory", $"Freebie category '{request.Category}' is not recognized."));
            return new WerewolfFreebieEligibilityResult(false, findings, 0, request.RemainingBudget, request.RequestId);
        }

        var cost = entry.CostPerUnit * request.RequestedIncrease;
        if (cost > request.RemainingBudget)
        {
            findings.Add(new WerewolfFreebieEligibilityFinding(WerewolfFreebieEligibilityFindingSeverity.Error, "InsufficientBudget", $"Cost {cost} exceeds remaining budget {request.RemainingBudget}."));
            return new WerewolfFreebieEligibilityResult(false, findings, cost, request.RemainingBudget, request.RequestId);
        }

        var projectedRating = request.CurrentRating + request.RequestedIncrease;
        if (entry.MaximumRating.HasValue && projectedRating > entry.MaximumRating.Value)
        {
            findings.Add(new WerewolfFreebieEligibilityFinding(WerewolfFreebieEligibilityFindingSeverity.Error, "MaximumExceeded", $"Projected rating {projectedRating} exceeds creation maximum {entry.MaximumRating.Value} for {entry.DisplayName}."));
            return new WerewolfFreebieEligibilityResult(false, findings, cost, request.RemainingBudget, request.RequestId);
        }

        if (request.Category == WerewolfFreebieCategory.Ability && projectedRating > 3 && request.CurrentRating <= 3)
        {
            findings.Add(new WerewolfFreebieEligibilityFinding(WerewolfFreebieEligibilityFindingSeverity.Information, "BonusPointAboveBaseLimit", $"Ability rating {projectedRating} exceeds base-allocation limit of 3; requires bonus points (source line 920)."));
        }

        if (request.Category == WerewolfFreebieCategory.Gift && request.CurrentRating > 0)
        {
            findings.Add(new WerewolfFreebieEligibilityFinding(WerewolfFreebieEligibilityFindingSeverity.Error, "GiftAlreadyOwned", $"Gift '{request.ItemId}' is already owned; freebies cannot increase Gift rating."));
            return new WerewolfFreebieEligibilityResult(false, findings, cost, request.RemainingBudget, request.RequestId);
        }

        var remainingAfter = request.RemainingBudget - cost;
        findings.Add(new WerewolfFreebieEligibilityFinding(WerewolfFreebieEligibilityFindingSeverity.Information, "Eligible", $"{entry.DisplayName} '{request.ItemId}' is eligible for freebie purchase at cost {cost}."));

        return new WerewolfFreebieEligibilityResult(true, findings, cost, remainingAfter, request.RequestId);
    }
}
