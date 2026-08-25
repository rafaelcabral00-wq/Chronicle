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

        if (!string.IsNullOrWhiteSpace(context.MetisDeformity) && WerewolfMetisDeformityIdentifiers.Effects.TryGetValue(context.MetisDeformity, out var effects) && effects is not null)
        {
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
        }

        var giftBonus = ComputeGiftActionModifiers(context, ref dicePoolModifier, ref difficultyModifier, findings);
        if (giftBonus != 0)
        {
            findings.Add($"Gift action modifier: {giftBonus} applied to action resolution.");
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

    private static int ComputeGiftActionModifiers(WerewolfActionResolutionContext context, ref int dicePoolModifier, ref int difficultyModifier, List<string> findings)
    {
        var activeEffects = WerewolfGiftEffectService.GetSceneValidEffects(context);
        if (activeEffects.Count == 0)
        {
            return 0;
        }

        var total = 0;

        foreach (var effect in activeEffects)
        {
            switch (effect.EffectKind)
            {
                case WerewolfActiveGiftEffectKind.PerceptionBonus:
                    if (string.Equals(context.AttributeId, WerewolfAttributeIdentifiers.Perception, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(context.SenseBeingTested, "perception", StringComparison.OrdinalIgnoreCase))
                    {
                        var bonus = Math.Max(0, effect.Magnitude);
                        dicePoolModifier += bonus;
                        total += bonus;
                        findings.Add($"Gift Perception Bonus: +{bonus} dice to Perception test.");
                    }
                    break;

                case WerewolfActiveGiftEffectKind.MovementBonus:
                    if (context.IsBalanceTest || string.Equals(context.AbilityId, "athletics", StringComparison.OrdinalIgnoreCase))
                    {
                        var bonus = Math.Max(0, effect.Magnitude);
                        dicePoolModifier += bonus;
                        total += bonus;
                        findings.Add($"Gift Movement Bonus: +{bonus} dice to movement/athletics test.");
                    }
                    break;

                case WerewolfActiveGiftEffectKind.StealthBonus:
                    if (string.Equals(context.AbilityId, "stealth", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(context.SenseBeingTested, "perception", StringComparison.OrdinalIgnoreCase))
                    {
                        var bonus = Math.Max(0, effect.Magnitude);
                        if (string.Equals(context.SenseBeingTested, "perception", StringComparison.OrdinalIgnoreCase))
                        {
                            difficultyModifier += bonus;
                            findings.Add($"Gift Stealth Bonus: +{bonus} difficulty to Perception tests to detect character.");
                        }
                        else
                        {
                            dicePoolModifier += bonus;
                            findings.Add($"Gift Stealth Bonus: +{bonus} dice to Stealth test.");
                        }
                        total += bonus;
                    }
                    break;

                case WerewolfActiveGiftEffectKind.WyrmSense:
                    if (string.Equals(context.AttributeId, WerewolfAttributeIdentifiers.Perception, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(context.AbilityId, "occult", StringComparison.OrdinalIgnoreCase))
                    {
                        var bonus = Math.Max(0, effect.Magnitude);
                        dicePoolModifier += bonus;
                        total += bonus;
                        findings.Add($"Gift Wyrm Sense: +{bonus} dice to detect Wyrm manifestations.");
                    }
                    break;

                case WerewolfActiveGiftEffectKind.AnimalCommunication:
                    if (string.Equals(context.AbilityId, "animal-empathy", StringComparison.OrdinalIgnoreCase))
                    {
                        var bonus = Math.Max(0, effect.Magnitude);
                        dicePoolModifier += bonus;
                        total += bonus;
                        findings.Add($"Gift Animal Communication: +{bonus} dice to Animal Empathy test.");
                    }
                    break;

                case WerewolfActiveGiftEffectKind.FearAura:
                    if (IsSocialAttribute(context.AttributeId))
                    {
                        var penalty = Math.Max(0, effect.Magnitude);
                        dicePoolModifier -= penalty;
                        total -= penalty;
                        findings.Add($"Gift Fear Aura: -{penalty} dice to Social test.");
                    }
                    break;

                case WerewolfActiveGiftEffectKind.MentalTestBonus:
                    if (string.Equals(context.AttributeId, WerewolfAttributeIdentifiers.Intelligence, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(context.AttributeId, WerewolfAttributeIdentifiers.Wits, StringComparison.OrdinalIgnoreCase))
                    {
                        var bonus = Math.Max(0, effect.Magnitude);
                        dicePoolModifier += bonus;
                        total += bonus;
                        findings.Add($"Gift Mental Test Bonus: +{bonus} dice to Mental test.");
                    }
                    break;

                case WerewolfActiveGiftEffectKind.WindEffect:
                    if (string.Equals(context.AbilityId, "survival", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(context.AttributeId, WerewolfAttributeIdentifiers.Perception, StringComparison.OrdinalIgnoreCase))
                    {
                        var bonus = Math.Max(0, effect.Magnitude);
                        dicePoolModifier += bonus;
                        total += bonus;
                        findings.Add($"Gift Wind Effect: +{bonus} dice to Survival/Perception test in natural environment.");
                    }
                    break;

                case WerewolfActiveGiftEffectKind.MagicDetection:
                    if (string.Equals(context.AbilityId, "occult", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(context.AttributeId, WerewolfAttributeIdentifiers.Perception, StringComparison.OrdinalIgnoreCase))
                    {
                        var bonus = Math.Max(0, effect.Magnitude);
                        dicePoolModifier += bonus;
                        total += bonus;
                        findings.Add($"Gift Magic Detection: +{bonus} dice to detect magical auras.");
                    }
                    break;

                case WerewolfActiveGiftEffectKind.AuraBlocking:
                    if (IsSocialAttribute(context.AttributeId) && string.Equals(context.AbilityId, "subterfuge", StringComparison.OrdinalIgnoreCase))
                    {
                        var bonus = Math.Max(0, effect.Magnitude);
                        dicePoolModifier += bonus;
                        total += bonus;
                        findings.Add($"Gift Aura Blocking: +{bonus} dice to block aura analysis and flaw detection.");
                    }
                    break;

                case WerewolfActiveGiftEffectKind.LightEffect:
                    if (string.Equals(context.SenseBeingTested, "vision", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(context.AttributeId, WerewolfAttributeIdentifiers.Perception, StringComparison.OrdinalIgnoreCase))
                    {
                        var bonus = Math.Max(0, effect.Magnitude);
                        if (string.Equals(context.SenseBeingTested, "vision", StringComparison.OrdinalIgnoreCase))
                        {
                            difficultyModifier -= bonus;
                            findings.Add($"Gift Light Effect: -{bonus} difficulty to Perception tests to detect character by vision.");
                        }
                        else
                        {
                            dicePoolModifier += bonus;
                            findings.Add($"Gift Light Effect: +{bonus} dice to Perception test.");
                        }
                        total += bonus;
                    }
                    break;

                case WerewolfActiveGiftEffectKind.TestBonus:
                    var testBonus = Math.Max(0, effect.Magnitude);
                    dicePoolModifier += testBonus;
                    total += testBonus;
                    findings.Add($"Gift Test Bonus: +{testBonus} dice to test.");
                    break;
            }
        }

        return total;
    }
}
