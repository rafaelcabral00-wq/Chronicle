namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

using System.Collections.ObjectModel;

public static class WerewolfAdvancementService
{
    public static WerewolfAdvanceTraitResult Advance(WerewolfAdvanceTraitRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.CurrentState is null)
        {
            return Invalid(WerewolfProgressionErrorCode.MissingState, "Runtime state is required for advancement.");
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

        if (request.CurrentState.UnspentXp < 0)
        {
            return Invalid(WerewolfProgressionErrorCode.InvalidOrNonPositiveProgressionState, "Unspent XP cannot be negative.");
        }

        if (request.ExpectedRuntimeStateVersion != request.CurrentState.RuntimeStateVersion)
        {
            return Invalid(WerewolfProgressionErrorCode.StaleRuntimeStateVersion, $"Expected runtime state version {request.ExpectedRuntimeStateVersion} does not match current version {request.CurrentState.RuntimeStateVersion}.");
        }

        var traitType = request.TraitType.Trim().ToLowerInvariant();
        var validTraitType = traitType switch
        {
            "attribute" => true,
            "ability" => true,
            "new-ability" => true,
            "rage" => true,
            "gnosis" => true,
            "willpower" => true,
            "gift" => true,
            "other-gift" => true,
            "totem" => true,
            "background" => true,
            _ => false
        };

        if (!validTraitType)
        {
            return Invalid(WerewolfProgressionErrorCode.UnknownTraitType, $"Trait type '{request.TraitType}' is not recognized.");
        }

        if (traitType == "background")
        {
            if (string.IsNullOrWhiteSpace(request.TraitIdentifier))
            {
                return Invalid(WerewolfProgressionErrorCode.BackgroundNotPurchasableWithExperience, "Backgrounds cannot be increased with XP.");
            }

            if (request.TraitIdentifier.Contains("totem", StringComparison.OrdinalIgnoreCase))
            {
                return Invalid(WerewolfProgressionErrorCode.TotemExperienceCostUnresolved, "Totem XP cost is unresolved (A-012: 2 XP vs 3 XP conflict).");
            }

            return Invalid(WerewolfProgressionErrorCode.BackgroundNotPurchasableWithExperience, $"Background '{request.TraitIdentifier}' cannot be increased with XP.");
        }

        if (traitType == "totem")
        {
            return Invalid(WerewolfProgressionErrorCode.TotemExperienceCostUnresolved, "Totem XP cost is unresolved (A-012: 2 XP vs 3 XP conflict).");
        }

        if (traitType is "attribute" or "ability" or "new-ability")
        {
            if (string.IsNullOrWhiteSpace(request.TraitIdentifier))
            {
                return Invalid(WerewolfProgressionErrorCode.InvalidTraitIdentifier, "Trait identifier is required.");
            }
        }

        if (traitType is "gift" or "other-gift")
        {
            if (string.IsNullOrWhiteSpace(request.TraitIdentifier))
            {
                return Invalid(WerewolfProgressionErrorCode.InvalidTraitIdentifier, "Gift identifier is required.");
            }

            if (request.CurrentState.KnownGiftKeys is not null && request.CurrentState.KnownGiftKeys.Contains(request.TraitIdentifier!, StringComparer.Ordinal))
            {
                return Invalid(WerewolfProgressionErrorCode.GiftAlreadyKnown, $"Gift '{request.TraitIdentifier}' is already known.");
            }
        }

        var cost = traitType switch
        {
            "attribute" => CalculateAttributeCost(request.CurrentState, request.TraitIdentifier),
            "ability" => CalculateAbilityCost(request.CurrentState, request.TraitIdentifier, isNew: false),
            "new-ability" => CalculateAbilityCost(request.CurrentState, request.TraitIdentifier, isNew: true),
            "rage" => request.CurrentState.RagePermanent,
            "gnosis" => request.CurrentState.GnosisPermanent * 2,
            "willpower" => request.CurrentState.WillpowerPermanent,
            "gift" => CalculateGiftCost(request.CurrentState, request.TraitIdentifier, isOtherCategory: false),
            "other-gift" => CalculateGiftCost(request.CurrentState, request.TraitIdentifier, isOtherCategory: true),
            _ => -1
        };

        if (cost < 0)
        {
            return Invalid(WerewolfProgressionErrorCode.UnknownTrait, $"Trait '{request.TraitIdentifier}' not found for type '{request.TraitType}'.");
        }

        if (request.CurrentState.UnspentXp < cost)
        {
            return new WerewolfAdvanceTraitResult(
                false,
                null,
                [new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Error, WerewolfProgressionErrorCode.InsufficientXp, $"Insufficient XP: need {cost}, have {request.CurrentState.UnspentXp}.")],
                request.RequestId,
                null,
                null,
                request.CurrentState.UnspentXp);
        }

        var newState = request.CurrentState with
        {
            RuntimeStateVersion = request.CurrentState.RuntimeStateVersion + 1,
            UnspentXp = request.CurrentState.UnspentXp - cost
        };

        var findings = new List<WerewolfProgressionFinding>();

        switch (traitType)
        {
            case "attribute":
                if (!TryGetCurrentRating(request.CurrentState, "attribute", request.TraitIdentifier!, out var attrValue))
                {
                    return Invalid(WerewolfProgressionErrorCode.UnknownTrait, $"Attribute '{request.TraitIdentifier}' not found.");
                }

                var newAttrRatings = new Dictionary<string, int>(request.CurrentState.PostCreationAttributeRatings ?? new Dictionary<string, int>(StringComparer.Ordinal), StringComparer.Ordinal)
                {
                    [request.TraitIdentifier!] = attrValue + 1
                };
                newState = newState with { PostCreationAttributeRatings = new ReadOnlyDictionary<string, int>(newAttrRatings) };
                findings.Add(new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Information, WerewolfProgressionErrorCode.AdvancementSucceeded, $"Advanced attribute '{request.TraitIdentifier}' to {newAttrRatings[request.TraitIdentifier!]}."));
                break;

            case "ability":
                if (!TryGetCurrentRating(request.CurrentState, "ability", request.TraitIdentifier!, out var abilValue))
                {
                    return Invalid(WerewolfProgressionErrorCode.UnknownTrait, $"Ability '{request.TraitIdentifier}' not found.");
                }

                var newAbilRatings = new Dictionary<string, int>(request.CurrentState.PostCreationAbilityRatings ?? new Dictionary<string, int>(StringComparer.Ordinal), StringComparer.Ordinal)
                {
                    [request.TraitIdentifier!] = abilValue + 1
                };
                newState = newState with { PostCreationAbilityRatings = new ReadOnlyDictionary<string, int>(newAbilRatings) };
                findings.Add(new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Information, WerewolfProgressionErrorCode.AdvancementSucceeded, $"Advanced ability '{request.TraitIdentifier}' to {newAbilRatings[request.TraitIdentifier!]}."));
                break;

            case "new-ability":
                if (TraitExists(request.CurrentState, "ability", request.TraitIdentifier!))
                {
                    return Invalid(WerewolfProgressionErrorCode.InvalidTarget, $"Ability '{request.TraitIdentifier}' already exists. Use 'ability' advancement instead.");
                }

                var newAbilityRatings = new Dictionary<string, int>(request.CurrentState.PostCreationAbilityRatings ?? new Dictionary<string, int>(StringComparer.Ordinal), StringComparer.Ordinal)
                {
                    [request.TraitIdentifier!] = 1
                };
                newState = newState with { PostCreationAbilityRatings = new ReadOnlyDictionary<string, int>(newAbilityRatings) };
                findings.Add(new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Information, WerewolfProgressionErrorCode.AdvancementSucceeded, $"Created new ability '{request.TraitIdentifier}' at rating 1."));
                break;

            case "rage":
                newState = newState with { RagePermanent = request.CurrentState.RagePermanent + 1 };
                findings.Add(new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Information, WerewolfProgressionErrorCode.AdvancementSucceeded, $"Advanced Rage permanent to {newState.RagePermanent}."));
                break;

            case "gnosis":
                newState = newState with { GnosisPermanent = request.CurrentState.GnosisPermanent + 1 };
                findings.Add(new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Information, WerewolfProgressionErrorCode.AdvancementSucceeded, $"Advanced Gnosis permanent to {newState.GnosisPermanent}."));
                break;

            case "willpower":
                newState = newState with { WillpowerPermanent = request.CurrentState.WillpowerPermanent + 1 };
                findings.Add(new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Information, WerewolfProgressionErrorCode.AdvancementSucceeded, $"Advanced Willpower permanent to {newState.WillpowerPermanent}."));
                break;

            case "gift":
                var updatedGifts = new List<string>(request.CurrentState.KnownGiftKeys ?? []);
                updatedGifts.Add(request.TraitIdentifier!);
                newState = newState with { KnownGiftKeys = Array.AsReadOnly(updatedGifts.Distinct(StringComparer.Ordinal).ToArray()) };
                findings.Add(new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Information, WerewolfProgressionErrorCode.AdvancementSucceeded, $"Added gift '{request.TraitIdentifier}' to known Gifts."));
                break;

            case "other-gift":
                var updatedOtherGifts = new List<string>(request.CurrentState.KnownGiftKeys ?? []);
                updatedOtherGifts.Add(request.TraitIdentifier!);
                newState = newState with { KnownGiftKeys = Array.AsReadOnly(updatedOtherGifts.Distinct(StringComparer.Ordinal).ToArray()) };
                findings.Add(new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Information, WerewolfProgressionErrorCode.AdvancementSucceeded, $"Added other-category gift '{request.TraitIdentifier}' to known Gifts."));
                break;
        }

        return new WerewolfAdvanceTraitResult(
            true,
            newState,
            findings,
            request.RequestId,
            newState.RuntimeStateVersion,
            cost,
            newState.UnspentXp);
    }

    private static bool TryGetCurrentRating(WerewolfRuntimeCharacterState state, string traitType, string traitIdentifier, out int currentRating)
    {
        currentRating = 0;
        var ratings = traitType == "attribute" ? state.PostCreationAttributeRatings : state.PostCreationAbilityRatings;

        if (ratings is not null && ratings.TryGetValue(traitIdentifier, out var postCreationRating))
        {
            currentRating = postCreationRating;
            return true;
        }

        var bindingKey = traitType == "attribute" ? "attributes" : "abilities";
        if (!TryGetPackageBindingValues(state, bindingKey, out var snapshotRatings))
        {
            return false;
        }

        if (snapshotRatings.TryGetValue(traitIdentifier, out var snapshotRating))
        {
            currentRating = snapshotRating;
            return true;
        }

        return false;
    }

    private static bool TraitExists(WerewolfRuntimeCharacterState state, string traitType, string traitIdentifier)
    {
        var ratings = traitType == "attribute" ? state.PostCreationAttributeRatings : state.PostCreationAbilityRatings;

        if (ratings is not null && ratings.ContainsKey(traitIdentifier))
        {
            return true;
        }

        var bindingKey = traitType == "attribute" ? "attributes" : "abilities";
        if (!TryGetPackageBindingValues(state, bindingKey, out var snapshotRatings))
        {
            return false;
        }

        return snapshotRatings.ContainsKey(traitIdentifier);
    }

    private static int CalculateAttributeCost(WerewolfRuntimeCharacterState state, string? traitIdentifier)
    {
        if (string.IsNullOrWhiteSpace(traitIdentifier) || !TryGetCurrentRating(state, "attribute", traitIdentifier, out var value))
        {
            return -1;
        }

        return value * 4;
    }

    private static int CalculateAbilityCost(WerewolfRuntimeCharacterState state, string? traitIdentifier, bool isNew)
    {
        if (isNew)
        {
            return 3;
        }

        if (string.IsNullOrWhiteSpace(traitIdentifier) || !TryGetCurrentRating(state, "ability", traitIdentifier, out var value))
        {
            return -1;
        }

        return value * 2;
    }

    private static int CalculateGiftCost(WerewolfRuntimeCharacterState state, string? giftKey, bool isOtherCategory)
    {
        if (string.IsNullOrWhiteSpace(giftKey))
        {
            return -1;
        }

        var definition = WerewolfGiftCatalog.Get(giftKey);
        if (definition is null)
        {
            return -1;
        }

        return isOtherCategory ? definition.Level * 5 : definition.Level * 3;
    }

    private static bool TryGetPackageBindingValues(WerewolfRuntimeCharacterState state, string key, out Dictionary<string, int> values)
    {
        values = new Dictionary<string, int>(StringComparer.Ordinal);
        if (!state.PackageBinding.TryGetValue(key, out var raw) || string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        foreach (var entry in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = entry.Split(':', StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || !int.TryParse(parts[1], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var value))
            {
                continue;
            }

            values[parts[0]] = value;
        }

        return true;
    }

    private static WerewolfAdvanceTraitResult Invalid(WerewolfProgressionErrorCode code, string message)
    {
        return new WerewolfAdvanceTraitResult(
            false,
            null,
            [new WerewolfProgressionFinding(WerewolfProgressionFindingSeverity.Error, code, message)],
            null,
            null,
            null,
            null);
    }
}
