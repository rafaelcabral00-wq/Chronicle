namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

using System.Text.Json;

public sealed record WerewolfSocialTestDefinitionRequest(
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    string RequestId,
    string SocialChallengeId,
    WerewolfSocialTargetContext TargetContext,
    string? CurrentForm = null,
    bool UsesPhysicalPosture = false,
    int? Modifier = null);

public sealed record WerewolfSocialTestDefinitionResult(
    bool Succeeded,
    WerewolfRuntimeCharacterState CurrentState,
    IReadOnlyList<WerewolfSocialTestDefinitionFinding> Findings,
    string RequestId,
    string ChallengeId,
    int? BasePool,
    int? BaseDifficulty,
    string? AttributeId,
    string? AbilityId,
    int? DifficultyModifier,
    int? ExplicitModifier,
    int? FinalPool,
    int? FinalDifficulty,
    int? SuccessThreshold,
    string? SpecialRules,
    bool IsAutomaticFailure,
    bool IsActionUnavailable);

public sealed record WerewolfSocialTestDefinitionFinding(
    WerewolfSocialTestDefinitionFindingSeverity Severity,
    string Code,
    string Message);

public enum WerewolfSocialTestDefinitionFindingSeverity
{
    Information,
    Error
}

public static class WerewolfSocialTestDefinitionService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null
    };

    public static WerewolfSocialTestDefinitionResult DefineTest(WerewolfSocialTestDefinitionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.CurrentState);
        ArgumentNullException.ThrowIfNull(request.RequestId);
        ArgumentNullException.ThrowIfNull(request.SocialChallengeId);

        var findings = new List<WerewolfSocialTestDefinitionFinding>();

        if (request.CurrentState.RuntimeStateVersion != request.ExpectedRuntimeStateVersion)
        {
            return new WerewolfSocialTestDefinitionResult(false, request.CurrentState, findings, request.RequestId, request.SocialChallengeId, null, null, null, null, null, null, null, null, null, null, false, false);
        }

        if (!WerewolfSocialChallengeCatalog.Entries.TryGetValue(request.SocialChallengeId, out var challenge))
        {
            return new WerewolfSocialTestDefinitionResult(false, request.CurrentState,
                [new WerewolfSocialTestDefinitionFinding(WerewolfSocialTestDefinitionFindingSeverity.Error, "InvalidChallengeId", $"Social challenge identifier '{request.SocialChallengeId}' is not recognized.")],
                request.RequestId, request.SocialChallengeId, null, null, null, null, null, null, null, null, null, null, false, false);
        }

        if (!ValidateChallengeAttributeAbility(request.CurrentState, challenge, findings))
        {
            return new WerewolfSocialTestDefinitionResult(false, request.CurrentState, findings, request.RequestId, request.SocialChallengeId, null, null, null, null, null, null, null, null, null, null, false, false);
        }

        var baseAttributes = GetBaseAttributes(request.CurrentState);
        var baseAbilities = GetBaseAbilities(request.CurrentState);
        var currentForm = string.IsNullOrWhiteSpace(request.CurrentForm)
            ? request.CurrentState.CurrentForm
            : request.CurrentForm;

        var effectiveAttributes = WerewolfEffectiveAttributeService.ComputeEffectiveAttributes(
            baseAttributes.ToDictionary(kvp => kvp.Key, kvp => (int?)kvp.Value, StringComparer.Ordinal),
            currentForm);

        var baseAttributeRating = effectiveAttributes.TryGetValue(challenge.AttributeId, out var effectiveAttr)
            ? effectiveAttr
            : 0;

        var baseAbilityRating = baseAbilities.TryGetValue(challenge.AbilityId, out var abilityVal)
            ? abilityVal
            : 0;

        int basePool;
        if (challenge.UsesFuryPool)
        {
            var charismaPlusIntimidation = baseAttributeRating + baseAbilityRating;
            var rageValue = request.CurrentState.RagePermanent;
            basePool = Math.Max(charismaPlusIntimidation, rageValue);
            findings.Add(new WerewolfSocialTestDefinitionFinding(WerewolfSocialTestDefinitionFindingSeverity.Information, "FuryPoolUsed", $"Defrontação uses higher of Charisma+Intimidation ({charismaPlusIntimidation}) or Fury ({rageValue}). Base pool: {basePool}."));
        }
        else
        {
            basePool = baseAttributeRating + baseAbilityRating;
        }

        if (basePool < 0)
        {
            basePool = 0;
        }

        var modifierResult = WerewolfActionResolutionModifierService.ComputeModifiers(
            new WerewolfActionResolutionContext(
                challenge.AttributeId,
                challenge.AbilityId,
                currentForm,
                GetMetisDeformity(request.CurrentState),
                false,
                request.CurrentState.Conditions?.Any(c => c.IsActive && c.Kind == WerewolfConditionKind.UnderTension) ?? false,
                false,
                null,
                false,
                false,
                false,
                request.CurrentState.Conditions?.Where(c => c.IsActive).Select(c => c.ConditionKey).ToList() ?? [],
                true,
                request.CurrentState.RagePermanent,
                request.CurrentState.WillpowerCurrent,
                request.CurrentState.ActiveGiftEffects,
                request.CurrentState.CurrentSceneToken));

        findings.AddRange(modifierResult.Findings.Select(f => new WerewolfSocialTestDefinitionFinding(WerewolfSocialTestDefinitionFindingSeverity.Information, "ModifierApplied", f)));

        var socialPenalty = WerewolfBestaInteriorService.ComputeSocialDicePenalty(
            request.CurrentState.RagePermanent,
            request.CurrentState.WillpowerPermanent);

        if (socialPenalty > 0 && WerewolfBestaInteriorService.IsSocialTest(challenge.AttributeId, challenge.AbilityId))
        {
            modifierResult = modifierResult with { DicePoolModifier = modifierResult.DicePoolModifier - socialPenalty };
            findings.Add(new WerewolfSocialTestDefinitionFinding(WerewolfSocialTestDefinitionFindingSeverity.Information, "BestaInteriorApplied", $"Besta Interior: -{socialPenalty} dice penalty on Social test (Rage {request.CurrentState.RagePermanent} > Willpower {request.CurrentState.WillpowerPermanent})."));
        }

        if (modifierResult.IsAutomaticFailure)
        {
            return new WerewolfSocialTestDefinitionResult(true, request.CurrentState, findings, request.RequestId, challenge.ChallengeId, basePool, 10, challenge.AttributeId, challenge.AbilityId, challenge.BaseDifficulty, request.Modifier ?? 0, 0, challenge.BaseDifficulty, ComputeSuccessThreshold(challenge, request.TargetContext), challenge.SourceLocator, true, false);
        }

        if (modifierResult.IsActionUnavailable)
        {
            return new WerewolfSocialTestDefinitionResult(true, request.CurrentState, findings, request.RequestId, challenge.ChallengeId, basePool, 10, challenge.AttributeId, challenge.AbilityId, challenge.BaseDifficulty, request.Modifier ?? 0, 0, challenge.BaseDifficulty, ComputeSuccessThreshold(challenge, request.TargetContext), challenge.SourceLocator, false, true);
        }

        if (challenge.ChallengeId == WerewolfSocialChallengeIdentifiers.Intimidacao &&
            StringComparer.Ordinal.Equals(currentForm, WerewolfFormIdentifiers.Crinos) &&
            request.TargetContext.IsHumanTarget)
        {
            findings.Add(new WerewolfSocialTestDefinitionFinding(WerewolfSocialTestDefinitionFindingSeverity.Information, "CrinosDelirium", "Crinos form causes automatic Delirium in humans for Intimidation."));
            return new WerewolfSocialTestDefinitionResult(true, request.CurrentState, findings, request.RequestId, challenge.ChallengeId, basePool, 10, challenge.AttributeId, challenge.AbilityId, challenge.BaseDifficulty, request.Modifier ?? 0, 0, challenge.BaseDifficulty, ComputeSuccessThreshold(challenge, request.TargetContext), challenge.SourceLocator, true, false);
        }

        var pureBreedBonus = ComputePureBreedBonus(request.CurrentState, request.TargetContext);
        var giftSocialBonus = ComputeGiftSocialBonus(request.CurrentState);
        var finalPool = Math.Max(0, basePool + modifierResult.DicePoolModifier + pureBreedBonus + giftSocialBonus + (request.Modifier ?? 0));
        if (pureBreedBonus > 0)
        {
            findings.Add(new WerewolfSocialTestDefinitionFinding(WerewolfSocialTestDefinitionFindingSeverity.Information, "PureBreedBonus", $"Pure Breed grants +{pureBreedBonus} dice to Social test involving other Garou."));
        }
        if (giftSocialBonus > 0)
        {
            findings.Add(new WerewolfSocialTestDefinitionFinding(WerewolfSocialTestDefinitionFindingSeverity.Information, "GiftSocialBonus", $"Gift effect grants +{giftSocialBonus} dice to Social test."));
        }

        var baseDifficulty = ComputeBaseDifficulty(challenge, request.TargetContext);
        var finalDifficulty = Math.Max(2, Math.Min(10, baseDifficulty + modifierResult.DifficultyModifier));

        var successThreshold = ComputeSuccessThreshold(challenge, request.TargetContext);

        findings.Add(new WerewolfSocialTestDefinitionFinding(WerewolfSocialTestDefinitionFindingSeverity.Information, "SocialTestDefined", $"Social test defined: {challenge.ChallengeId} pool {basePool} (effective attribute {baseAttributeRating} + ability {baseAbilityRating}) + dice modifier {modifierResult.DicePoolModifier} + pure breed {pureBreedBonus} + explicit modifier {request.Modifier ?? 0} = final pool {finalPool} at difficulty {finalDifficulty}."));

        return new WerewolfSocialTestDefinitionResult(true, request.CurrentState, findings, request.RequestId, challenge.ChallengeId, basePool, 10, challenge.AttributeId, challenge.AbilityId, baseDifficulty, request.Modifier ?? 0, finalPool, finalDifficulty, successThreshold, challenge.SourceLocator, false, false);
    }

    private static int ComputeSuccessThreshold(WerewolfSocialChallengeDefinition challenge, WerewolfSocialTargetContext context)
    {
        if (challenge.SuccessThreshold > 0)
        {
            return challenge.SuccessThreshold;
        }

        return challenge.ChallengeId switch
        {
            WerewolfSocialChallengeIdentifiers.AtracaoAnimal => context.TargetWillpower ?? 6,
            WerewolfSocialChallengeIdentifiers.Defrontacao => (context.TargetRaciocinio ?? 3) + 5,
            _ => 1
        };
    }

    private static int ComputePureBreedBonus(WerewolfRuntimeCharacterState state, WerewolfSocialTargetContext context)
    {
        if (!context.IsGarouTarget)
        {
            return 0;
        }

        if (state.PackageBinding.TryGetValue("backgrounds", out var bgText) && !string.IsNullOrWhiteSpace(bgText))
        {
            try
            {
                var backgrounds = JsonSerializer.Deserialize<Dictionary<string, int?>>(bgText);
                if (backgrounds is not null && backgrounds.TryGetValue("pure-breed", out var pureBreed) && pureBreed.HasValue)
                {
                    return pureBreed.Value;
                }
            }
            catch
            {
            }
        }

        return 0;
    }

    private static int ComputeGiftSocialBonus(WerewolfRuntimeCharacterState state)
    {
        var activeEffects = WerewolfGiftEffectService.GetSceneValidEffects(state);
        if (activeEffects.Count == 0)
        {
            return 0;
        }

        var socialBonus = activeEffects
            .Where(e => e.EffectKind == WerewolfActiveGiftEffectKind.SocialTestBonus && e.Magnitude > 0)
            .Sum(e => e.Magnitude);

        var intimidationBonus = activeEffects
            .Where(e => e.EffectKind == WerewolfActiveGiftEffectKind.SocialIntimidationBonus && e.Magnitude > 0)
            .Sum(e => e.Magnitude);

        return socialBonus + intimidationBonus;
    }

    private static bool ValidateChallengeAttributeAbility(WerewolfRuntimeCharacterState state, WerewolfSocialChallengeDefinition challenge, List<WerewolfSocialTestDefinitionFinding> findings)
    {
        var baseAttributes = GetBaseAttributes(state);
        var baseAbilities = GetBaseAbilities(state);

        if (!baseAttributes.ContainsKey(challenge.AttributeId))
        {
            findings.Add(new WerewolfSocialTestDefinitionFinding(WerewolfSocialTestDefinitionFindingSeverity.Error, "AttributeNotAllocated", $"Attribute '{challenge.AttributeId}' is not present in character state."));
            return false;
        }

        if (!baseAbilities.ContainsKey(challenge.AbilityId))
        {
            findings.Add(new WerewolfSocialTestDefinitionFinding(WerewolfSocialTestDefinitionFindingSeverity.Error, "AbilityNotAllocated", $"Ability '{challenge.AbilityId}' is not present in character state."));
            return false;
        }

        return true;
    }

    private static int ComputeBaseDifficulty(WerewolfSocialChallengeDefinition challenge, WerewolfSocialTargetContext context)
    {
        if (challenge.BaseDifficulty > 0)
        {
            return challenge.BaseDifficulty;
        }

        return challenge.ChallengeId switch
        {
            WerewolfSocialChallengeIdentifiers.AtracaoAnimal => context.TargetWillpower ?? 6,
            WerewolfSocialChallengeIdentifiers.Credibilidade => Math.Max(2, (context.TargetInteligencia ?? 3) + (context.TargetRaciocinio ?? 0) - context.TruthLevel),
            WerewolfSocialChallengeIdentifiers.Defrontacao => context.TargetWillpower ?? 6,
            WerewolfSocialChallengeIdentifiers.Engabelacao => Math.Max(2, (context.TargetRaciocinio ?? 3) + (context.TargetRage ?? 0)),
            WerewolfSocialChallengeIdentifiers.Interrogatorio => context.TargetWillpower ?? 6,
            WerewolfSocialChallengeIdentifiers.Intimidacao => context.TargetWillpower ?? 6,
            WerewolfSocialChallengeIdentifiers.OratoriaPerformance => ComputeOratoriaDifficulty(context),
            WerewolfSocialChallengeIdentifiers.Seducao => Math.Max(2, (context.TargetRaciocinio ?? 3) + 3),
            _ => 6
        };
    }

    private static int ComputeOratoriaDifficulty(WerewolfSocialTargetContext context)
    {
        var baseDifficulty = 6;

        if (context.CrowdDispositionBonus.HasValue)
        {
            baseDifficulty += context.CrowdDispositionBonus.Value;
        }

        if (context.CharacterRankValue.HasValue && context.CharacterRankValue.Value > 0)
        {
            baseDifficulty -= context.CharacterRankValue.Value;
        }

        return Math.Max(2, Math.Min(10, baseDifficulty));
    }

    private static string? GetMetisDeformity(WerewolfRuntimeCharacterState state)
    {
        if (state.PackageBinding.TryGetValue("metis-deformity", out var deformity) && !string.IsNullOrWhiteSpace(deformity))
        {
            return deformity;
        }
        return null;
    }

    private static Dictionary<string, int> GetBaseAttributes(WerewolfRuntimeCharacterState state)
    {
        var result = new Dictionary<string, int>(StringComparer.Ordinal);
        if (state.PackageBinding.TryGetValue("attributes", out var attrText) && !string.IsNullOrWhiteSpace(attrText))
        {
            try
            {
                var attributes = JsonSerializer.Deserialize<Dictionary<string, int>>(attrText);
                if (attributes is not null)
                {
                    foreach (var kvp in attributes)
                    {
                        result[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch
            {
            }
        }
        return result;
    }

    private static Dictionary<string, int> GetBaseAbilities(WerewolfRuntimeCharacterState state)
    {
        var result = new Dictionary<string, int>(StringComparer.Ordinal);
        if (state.PackageBinding.TryGetValue("abilities", out var abilText) && !string.IsNullOrWhiteSpace(abilText))
        {
            try
            {
                var abilities = JsonSerializer.Deserialize<Dictionary<string, int>>(abilText);
                if (abilities is not null)
                {
                    foreach (var kvp in abilities)
                    {
                        result[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch
            {
            }
        }
        return result;
    }
}
