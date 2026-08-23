namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfActionResolutionModifierResult(
    int DicePoolModifier,
    int DifficultyModifier,
    bool IsActionUnavailable,
    bool IsAutomaticFailure,
    IReadOnlyList<string> Findings,
    IReadOnlyList<WerewolfConditionalTest> ConditionalTests);

public sealed record WerewolfConditionalTest(
    string Condition,
    string Target,
    int TestDifficulty,
    int MinimumSuccesses,
    string Consequence);

public static class WerewolfActionResolutionModifierService
{
    public static WerewolfActionResolutionModifierResult ComputeModifiers(WerewolfActionResolutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var findings = new List<string>();
        var dicePoolModifier = 0;
        var difficultyModifier = 0;
        var isActionUnavailable = false;
        var isAutomaticFailure = false;
        var conditionalTests = new List<WerewolfConditionalTest>();

        if (string.IsNullOrWhiteSpace(context.MetisDeformity))
        {
            return new WerewolfActionResolutionModifierResult(0, 0, false, false, findings, []);
        }

        if (!WerewolfMetisDeformityIdentifiers.Effects.TryGetValue(context.MetisDeformity, out var effects) || effects is null)
        {
            return new WerewolfActionResolutionModifierResult(0, 0, false, false, findings, []);
        }

        foreach (var effect in effects)
        {
            switch (effect.Kind)
            {
                case WerewolfMetisDeformityEffectKind.DifficultyModifier:
                    ApplyDifficultyModifier(effect, context, findings, ref difficultyModifier);
                    break;
                case WerewolfMetisDeformityEffectKind.DiceBonus:
                    ApplyDiceBonus(effect, context, findings, ref dicePoolModifier);
                    break;
                case WerewolfMetisDeformityEffectKind.AutomaticFailure:
                    ApplyAutomaticFailure(effect, context, findings, ref isAutomaticFailure);
                    break;
                case WerewolfMetisDeformityEffectKind.ConditionalTest:
                    ApplyConditionalTest(effect, context, findings, conditionalTests);
                    break;
                case WerewolfMetisDeformityEffectKind.FormRestricted:
                    ApplyFormRestricted(effect, context, findings, ref difficultyModifier);
                    break;
                case WerewolfMetisDeformityEffectKind.SensoryFailure:
                    ApplySensoryFailure(effect, context, findings, ref isAutomaticFailure);
                    break;
                case WerewolfMetisDeformityEffectKind.TrackingPenalty:
                    ApplyTrackingPenalty(effect, context, findings, ref difficultyModifier);
                    break;
            }
        }

        if (context.IsInFrenzy && context.RagePermanent.HasValue && context.WillpowerPermanent.HasValue)
        {
            var socialPenalty = WerewolfBestaInteriorService.ComputeSocialDicePenalty(
                context.RagePermanent.Value,
                context.WillpowerPermanent.Value);

            if (socialPenalty > 0 && WerewolfBestaInteriorService.IsSocialTest(context.AttributeId, context.AbilityId))
            {
                dicePoolModifier -= socialPenalty;
                findings.Add($"Besta Interior: -{socialPenalty} dice penalty on Social test (Rage {context.RagePermanent} > Willpower {context.WillpowerPermanent}).");
            }
        }

        return new WerewolfActionResolutionModifierResult(
            dicePoolModifier,
            difficultyModifier,
            isActionUnavailable,
            isAutomaticFailure,
            findings,
            conditionalTests);
    }

    private static void ApplyDifficultyModifier(
        WerewolfMetisDeformityEffect effect,
        WerewolfActionResolutionContext context,
        List<string> findings,
        ref int difficultyModifier)
    {
        if (effect.Target is null || effect.Value is null)
        {
            return;
        }

        if (!TargetsAttribute(effect.Target, context.AttributeId))
        {
            return;
        }

        if (effect.Condition == "daylight-without-protection" && !context.IsDaylightWithoutProtection)
        {
            return;
        }

        if (effect.Condition == "using-withered-limb" && !context.IsUsingWitheredLimb)
        {
            return;
        }

        difficultyModifier += effect.Value.Value;
        findings.Add($"Difficulty +{effect.Value.Value} from {effect.Target} modifier (source: {context.MetisDeformity}).");
    }

    private static void ApplyDiceBonus(
        WerewolfMetisDeformityEffect effect,
        WerewolfActionResolutionContext context,
        List<string> findings,
        ref int dicePoolModifier)
    {
        if (effect.Value is null)
        {
            return;
        }

        dicePoolModifier += effect.Value.Value;
        findings.Add($"Dice pool +{effect.Value.Value} from {context.MetisDeformity}.");
    }

    private static void ApplyAutomaticFailure(
        WerewolfMetisDeformityEffect effect,
        WerewolfActionResolutionContext context,
        List<string> findings,
        ref bool isAutomaticFailure)
    {
        if (effect.Target == "vision-based-tests" && context.IsVisionBased)
        {
            isAutomaticFailure = true;
            findings.Add($"Automatic failure on vision-based tests from {context.MetisDeformity}.");
        }
    }

    private static void ApplyConditionalTest(
        WerewolfMetisDeformityEffect effect,
        WerewolfActionResolutionContext context,
        List<string> findings,
        List<WerewolfConditionalTest> conditionalTests)
    {
        if (effect.Condition == "under-tension" && !context.IsUnderTension)
        {
            return;
        }

        if (effect.Condition == "on-critical-failure")
        {
            return;
        }

        if (effect.Target is null || effect.TestDifficulty is null || effect.MinimumSuccesses is null || effect.Consequence is null)
        {
            return;
        }

        conditionalTests.Add(new WerewolfConditionalTest(
            effect.Condition ?? string.Empty,
            effect.Target,
            effect.TestDifficulty.Value,
            effect.MinimumSuccesses.Value,
            effect.Consequence));
        findings.Add($"Conditional test required: {effect.Target} vs difficulty {effect.TestDifficulty.Value} (minimum {effect.MinimumSuccesses.Value} successes) under {effect.Condition}.");
    }

    private static void ApplyFormRestricted(
        WerewolfMetisDeformityEffect effect,
        WerewolfActionResolutionContext context,
        List<string> findings,
        ref int difficultyModifier)
    {
        if (effect.Form is null || effect.Value is null)
        {
            return;
        }

        var forms = effect.Form.Split(',', StringSplitOptions.TrimEntries);
        var currentForm = NormalizeForm(context.CurrentForm);
        if (!forms.Contains(currentForm, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        if (effect.Condition == "balance" && !context.IsBalanceTest)
        {
            return;
        }

        difficultyModifier += effect.Value.Value;
        findings.Add($"Difficulty +{effect.Value.Value} from {context.MetisDeformity} in form {context.CurrentForm} ({effect.Condition}).");
    }

    private static string NormalizeForm(string formId)
    {
        if (formId.Contains('.', StringComparison.Ordinal))
        {
            var lastSegment = formId.Split('.').Last();
            return lastSegment.ToLowerInvariant();
        }
        return formId.ToLowerInvariant();
    }

    private static void ApplySensoryFailure(
        WerewolfMetisDeformityEffect effect,
        WerewolfActionResolutionContext context,
        List<string> findings,
        ref bool isAutomaticFailure)
    {
        if (effect.Sense is null)
        {
            return;
        }

        if (context.SenseBeingTested is not null &&
            StringComparer.OrdinalIgnoreCase.Equals(context.SenseBeingTested, effect.Sense))
        {
            isAutomaticFailure = true;
            findings.Add($"Automatic failure on {effect.Sense}-based perception from {context.MetisDeformity}.");
        }
    }

    private static void ApplyTrackingPenalty(
        WerewolfMetisDeformityEffect effect,
        WerewolfActionResolutionContext context,
        List<string> findings,
        ref int difficultyModifier)
    {
        if (effect.Value is null)
        {
            return;
        }

        if (effect.Condition == "tracking" && !context.IsTracking)
        {
            return;
        }

        difficultyModifier += effect.Value.Value;
        findings.Add($"Tracking difficulty +{effect.Value.Value} from {context.MetisDeformity}.");
    }

    private static bool TakesAttribute(string target, string attributeId)
    {
        return StringComparer.OrdinalIgnoreCase.Equals(target, attributeId) ||
               StringComparer.OrdinalIgnoreCase.Equals(target, "Absorption") ||
               StringComparer.OrdinalIgnoreCase.Equals(target, "PrimalInstinct");
    }

    private static bool TargetsAttribute(string target, string attributeId)
    {
        if (StringComparer.OrdinalIgnoreCase.Equals(target, attributeId))
        {
            return true;
        }

        if (StringComparer.OrdinalIgnoreCase.Equals(target, "social"))
        {
            return IsSocialAttribute(attributeId);
        }

        if (StringComparer.OrdinalIgnoreCase.Equals(target, "perception"))
        {
            return StringComparer.OrdinalIgnoreCase.Equals(attributeId, WerewolfAttributeIdentifiers.Perception);
        }
        if (StringComparer.OrdinalIgnoreCase.Equals(target, "stamina"))
        {
            return StringComparer.OrdinalIgnoreCase.Equals(attributeId, WerewolfAttributeIdentifiers.Stamina);
        }
        if (StringComparer.OrdinalIgnoreCase.Equals(target, "dexterity"))
        {
            return StringComparer.OrdinalIgnoreCase.Equals(attributeId, WerewolfAttributeIdentifiers.Dexterity);
        }
        if (StringComparer.OrdinalIgnoreCase.Equals(target, "strength"))
        {
            return StringComparer.OrdinalIgnoreCase.Equals(attributeId, WerewolfAttributeIdentifiers.Strength);
        }
        if (StringComparer.OrdinalIgnoreCase.Equals(target, "charisma"))
        {
            return StringComparer.OrdinalIgnoreCase.Equals(attributeId, WerewolfAttributeIdentifiers.Charisma);
        }
        if (StringComparer.OrdinalIgnoreCase.Equals(target, "manipulation"))
        {
            return StringComparer.OrdinalIgnoreCase.Equals(attributeId, WerewolfAttributeIdentifiers.Manipulation);
        }
        if (StringComparer.OrdinalIgnoreCase.Equals(target, "appearance"))
        {
            return StringComparer.OrdinalIgnoreCase.Equals(attributeId, WerewolfAttributeIdentifiers.Appearance);
        }
        if (StringComparer.OrdinalIgnoreCase.Equals(target, "intelligence"))
        {
            return StringComparer.OrdinalIgnoreCase.Equals(attributeId, WerewolfAttributeIdentifiers.Intelligence);
        }
        if (StringComparer.OrdinalIgnoreCase.Equals(target, "wits"))
        {
            return StringComparer.OrdinalIgnoreCase.Equals(attributeId, WerewolfAttributeIdentifiers.Wits);
        }
        return false;
    }

    private static bool IsSocialAttribute(string attributeId)
    {
        return StringComparer.OrdinalIgnoreCase.Equals(attributeId, WerewolfAttributeIdentifiers.Charisma) ||
               StringComparer.OrdinalIgnoreCase.Equals(attributeId, WerewolfAttributeIdentifiers.Manipulation) ||
               StringComparer.OrdinalIgnoreCase.Equals(attributeId, WerewolfAttributeIdentifiers.Appearance);
    }
}
