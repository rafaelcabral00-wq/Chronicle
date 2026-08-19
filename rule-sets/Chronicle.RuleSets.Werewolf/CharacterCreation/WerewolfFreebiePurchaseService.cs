namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

using System.Collections.ObjectModel;

public sealed record WerewolfFreebiePurchaseRequest(
    string RequestId,
    WerewolfInitializedCharacterState Draft,
    int ExpectedDraftVersion,
    WerewolfFreebieCategory Category,
    string ItemId,
    int RequestedIncrease);

public sealed record WerewolfFreebiePurchaseResult(
    bool Succeeded,
    WerewolfInitializedCharacterState? Draft,
    IReadOnlyList<WerewolfFreebiePurchaseFinding> Findings,
    WerewolfFreebieLedgerEntry? LedgerEntry,
    int? RemainingBudget);

public sealed record WerewolfFreebiePurchaseFinding(
    WerewolfFreebiePurchaseFindingSeverity Severity,
    string Code,
    string Message);

public enum WerewolfFreebiePurchaseFindingSeverity
{
    Information,
    Error
}

public enum WerewolfFreebiePurchaseErrorCode
{
    FreebiePurchaseSucceeded,
    MissingDraft,
    StaleDraftVersion,
    InvalidIncrease,
    UnknownItem,
    InsufficientBudget
}

public static class WerewolfFreebiePurchaseService
{
    public static WerewolfFreebiePurchaseResult Purchase(WerewolfFreebiePurchaseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<WerewolfFreebiePurchaseFinding>();

        if (request.Draft is null)
        {
            return Invalid(WerewolfFreebiePurchaseErrorCode.MissingDraft, "Freebie purchase requires an initialized draft.", findings, null, null);
        }

        if (request.ExpectedDraftVersion != request.Draft.DraftVersion)
        {
            return Invalid(WerewolfFreebiePurchaseErrorCode.StaleDraftVersion, "Freebie purchase expected draft version does not match current draft version.", findings, null, null);
        }

        if (request.RequestedIncrease <= 0)
        {
            return Invalid(WerewolfFreebiePurchaseErrorCode.InvalidIncrease, "Requested increase must be greater than zero.", findings, null, null);
        }

        var remainingBudget = request.Draft.FreebieBudgetTotal - request.Draft.FreebieBudgetSpent;
        if (remainingBudget <= 0)
        {
            return Invalid(WerewolfFreebiePurchaseErrorCode.InsufficientBudget, $"No freebie budget remaining. Total: {request.Draft.FreebieBudgetTotal}, Spent: {request.Draft.FreebieBudgetSpent}.", findings, null, request.Draft.FreebieBudgetTotal - request.Draft.FreebieBudgetSpent);
        }

        var currentRating = GetCurrentRating(request.Draft, request.Category, request.ItemId);
        if (currentRating is null)
        {
            return Invalid(WerewolfFreebiePurchaseErrorCode.UnknownItem, $"Item '{request.ItemId}' is not recognized in category {request.Category}.", findings, null, remainingBudget);
        }

        var eligibilityRequest = new WerewolfFreebieEligibilityRequest(
            request.RequestId,
            request.Category,
            request.ItemId,
            currentRating.Value,
            request.RequestedIncrease,
            remainingBudget);

        var eligibilityResult = WerewolfFreebieEligibilityService.CheckEligibility(eligibilityRequest);
        if (!eligibilityResult.IsEligible)
        {
            foreach (var finding in eligibilityResult.Findings)
            {
                findings.Add(new WerewolfFreebiePurchaseFinding(
                    finding.Severity == WerewolfFreebieEligibilityFindingSeverity.Error
                        ? WerewolfFreebiePurchaseFindingSeverity.Error
                        : WerewolfFreebiePurchaseFindingSeverity.Information,
                    finding.Code,
                    finding.Message));
            }

            return new WerewolfFreebiePurchaseResult(false, null, findings, null, remainingBudget);
        }

        var cost = eligibilityResult.CalculatedCost;
        var newRating = currentRating.Value + request.RequestedIncrease;
        var newSpent = request.Draft.FreebieBudgetSpent + cost;
        var newRemaining = request.Draft.FreebieBudgetTotal - newSpent;

        var ledgerEntry = new WerewolfFreebieLedgerEntry(
            request.ItemId,
            request.Category.ToString().ToLowerInvariant(),
            cost,
            newRating,
            request.RequestId);

        var updatedDraft = ApplyPurchase(request.Draft, request.Category, request.ItemId, newRating);

        updatedDraft = updatedDraft with
        {
            DraftVersion = updatedDraft.DraftVersion + 1,
            FreebieLedger = Array.AsReadOnly(updatedDraft.FreebieLedger.Concat([ledgerEntry]).ToArray()),
            FreebieBudgetSpent = newSpent
        };

        findings.Add(new WerewolfFreebiePurchaseFinding(
            WerewolfFreebiePurchaseFindingSeverity.Information,
            "FreebiePurchaseSucceeded",
            $"Purchased {request.RequestedIncrease} {request.Category} '{request.ItemId}' for {cost} freebie points. New rating: {newRating}."));

        return new WerewolfFreebiePurchaseResult(
            true,
            updatedDraft,
            findings,
            ledgerEntry,
            newRemaining);
    }

    private static int? GetCurrentRating(WerewolfInitializedCharacterState draft, WerewolfFreebieCategory category, string itemId)
    {
        switch (category)
        {
            case WerewolfFreebieCategory.Attribute:
                if (draft.Attributes.TryGetValue(itemId, out var attr)) return attr ?? 0;
                return null;
            case WerewolfFreebieCategory.Ability:
                if (draft.Abilities.TryGetValue(itemId, out var ability)) return ability ?? 0;
                return null;
            case WerewolfFreebieCategory.Background:
                if (draft.Backgrounds.TryGetValue(itemId, out var bg)) return bg ?? 0;
                return null;
            case WerewolfFreebieCategory.Gift:
                return draft.Gifts.Contains(itemId, StringComparer.Ordinal) ? 1 : 0;
            case WerewolfFreebieCategory.Rage:
                if (draft.Resources.TryGetValue(WerewolfCharacterResourceIdentifiers.RagePermanent, out var ragePerm)) return ragePerm ?? 0;
                return null;
            case WerewolfFreebieCategory.Gnosis:
                if (draft.Resources.TryGetValue(WerewolfCharacterResourceIdentifiers.GnosisPermanent, out var gnoPerm)) return gnoPerm ?? 0;
                return null;
            case WerewolfFreebieCategory.Willpower:
                if (draft.Resources.TryGetValue(WerewolfCharacterResourceIdentifiers.WillpowerPermanent, out var wpPerm)) return wpPerm ?? 0;
                return null;
            default:
                return null;
        }
    }

    private static WerewolfInitializedCharacterState ApplyPurchase(WerewolfInitializedCharacterState draft, WerewolfFreebieCategory category, string itemId, int newRating)
    {
        switch (category)
        {
            case WerewolfFreebieCategory.Attribute:
                var newAttrs = draft.Attributes.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
                newAttrs[itemId] = newRating;
                return draft with { Attributes = new ReadOnlyDictionary<string, int?>(newAttrs) };
            case WerewolfFreebieCategory.Ability:
                var newAbilities = draft.Abilities.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
                newAbilities[itemId] = newRating;
                return draft with { Abilities = new ReadOnlyDictionary<string, int?>(newAbilities) };
            case WerewolfFreebieCategory.Background:
                var newBgs = draft.Backgrounds.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
                newBgs[itemId] = newRating;
                return draft with { Backgrounds = new ReadOnlyDictionary<string, int?>(newBgs) };
            case WerewolfFreebieCategory.Gift:
                var newGifts = draft.Gifts.ToList();
                if (!newGifts.Contains(itemId, StringComparer.Ordinal))
                {
                    newGifts.Add(itemId);
                }
                return draft with { Gifts = Array.AsReadOnly(newGifts.ToArray()) };
            case WerewolfFreebieCategory.Rage:
                var newRagePerm = newRating;
                var newRageCurr = newRating;
                var newRageResources = draft.Resources.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
                newRageResources[WerewolfCharacterResourceIdentifiers.RagePermanent] = newRagePerm;
                newRageResources[WerewolfCharacterResourceIdentifiers.RageCurrent] = newRageCurr;
                return draft with { Resources = new ReadOnlyDictionary<string, int?>(newRageResources) };
            case WerewolfFreebieCategory.Gnosis:
                var newGnoPerm = newRating;
                var newGnoCurr = newRating;
                var newGnoResources = draft.Resources.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
                newGnoResources[WerewolfCharacterResourceIdentifiers.GnosisPermanent] = newGnoPerm;
                newGnoResources[WerewolfCharacterResourceIdentifiers.GnosisCurrent] = newGnoCurr;
                return draft with { Resources = new ReadOnlyDictionary<string, int?>(newGnoResources) };
            case WerewolfFreebieCategory.Willpower:
                var newWpPerm = newRating;
                var newWpCurr = newRating;
                var newWpResources = draft.Resources.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
                newWpResources[WerewolfCharacterResourceIdentifiers.WillpowerPermanent] = newWpPerm;
                newWpResources[WerewolfCharacterResourceIdentifiers.WillpowerCurrent] = newWpCurr;
                return draft with { Resources = new ReadOnlyDictionary<string, int?>(newWpResources) };
            default:
                return draft;
        }
    }

    private static WerewolfFreebiePurchaseResult Invalid(WerewolfFreebiePurchaseErrorCode code, string message, List<WerewolfFreebiePurchaseFinding> findings, WerewolfFreebieLedgerEntry? ledger, int? remaining)
    {
        findings.Add(new WerewolfFreebiePurchaseFinding(WerewolfFreebiePurchaseFindingSeverity.Error, code.ToString(), message));
        return new WerewolfFreebiePurchaseResult(false, null, findings, ledger, remaining);
    }
}
