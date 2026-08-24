using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfGiftEffectRequest(
    string RequestId,
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    string GiftKey,
    int ActivationSuccesses,
    string? TargetId = null);

public sealed record WerewolfGiftEffectResult(
    bool Succeeded,
    WerewolfRuntimeCharacterState? UpdatedState,
    IReadOnlyList<WerewolfActiveGiftEffect> ActiveEffects,
    IReadOnlyList<string> Findings,
    string RequestId,
    int NewRuntimeStateVersion,
    string? ErrorCode = null);

public static class WerewolfGiftEffectService
{
    public static WerewolfGiftEffectResult ApplyEffect(WerewolfGiftEffectRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<string>();
        var activeEffects = new List<WerewolfActiveGiftEffect>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return new WerewolfGiftEffectResult(false, null, [], ["RequestId is required."], string.Empty, 0, "InvalidRequestId");
        }

        if (request.CurrentState is null)
        {
            return new WerewolfGiftEffectResult(false, null, [], ["CurrentState is required."], request.RequestId, 0, "InvalidState");
        }

        if (request.ExpectedRuntimeStateVersion < 1 || request.ExpectedRuntimeStateVersion != request.CurrentState.RuntimeStateVersion)
        {
            return new WerewolfGiftEffectResult(false, request.CurrentState, [], ["Version mismatch."], request.RequestId, request.CurrentState.RuntimeStateVersion, "StaleVersion");
        }

        if (string.IsNullOrWhiteSpace(request.GiftKey))
        {
            return new WerewolfGiftEffectResult(false, request.CurrentState, [], ["GiftKey is required."], request.RequestId, request.CurrentState.RuntimeStateVersion, "MissingGiftKey");
        }

        var definition = WerewolfGiftCatalog.Get(request.GiftKey);
        if (definition is null)
        {
            return new WerewolfGiftEffectResult(false, request.CurrentState, [], [$"Unknown gift: {request.GiftKey}"], request.RequestId, request.CurrentState.RuntimeStateVersion, "UnknownGift");
        }

        if (request.CurrentState.ActivatedGiftKeys is null || !request.CurrentState.ActivatedGiftKeys.Contains(request.GiftKey))
        {
            return new WerewolfGiftEffectResult(false, request.CurrentState, [], [$"Gift {request.GiftKey} has not been activated."], request.RequestId, request.CurrentState.RuntimeStateVersion, "GiftNotActivated");
        }

        var currentState = request.CurrentState;
        var successes = Math.Max(0, request.ActivationSuccesses);

        var effectResult = definition.GiftKey switch
        {
            WerewolfGiftIdentifiers.HomidMasterOfFire => ApplyMasterOfFire(currentState, successes),
            WerewolfGiftIdentifiers.MetisCreateElement => ApplyCreateElement(currentState, successes),
            WerewolfGiftIdentifiers.LupusHareLeap => ApplyHareLeap(currentState, successes),
            WerewolfGiftIdentifiers.RagabashOpenSeal => ApplyOpenSeal(currentState, successes),
            WerewolfGiftIdentifiers.TheurgeSpiritSpeech => ApplySpiritSpeech(currentState, successes),
            WerewolfGiftIdentifiers.PhilodoxResistPain => ApplyResistPain(currentState, successes),
            WerewolfGiftIdentifiers.GalliardBeastSpeech => ApplyBeastSpeech(currentState, successes),
            WerewolfGiftIdentifiers.AhrounFallingTouch => ApplyFallingTouch(currentState, successes),
            WerewolfGiftIdentifiers.GlassWalkersControlSimpleMachine => ApplyControlSimpleMachine(currentState, successes),
            WerewolfGiftIdentifiers.GlassWalkersDiagnostics => ApplyDiagnostics(currentState, successes),
            WerewolfGiftIdentifiers.GlassWalkersTrickShot => currentState,
            WerewolfGiftIdentifiers.GetOfFenrisRazorClaws => ApplyRazorClaws(currentState, successes),
            WerewolfGiftIdentifiers.GetOfFenrisResistPain => ApplyResistPain(currentState, successes),
            WerewolfGiftIdentifiers.GetOfFenrisVisageOfFenris => ApplyVisageOfFenris(currentState, successes),
            WerewolfGiftIdentifiers.FiannaFaerieLight => ApplyFaerieLight(currentState, successes),
            WerewolfGiftIdentifiers.FiannaPersuasion => ApplyFiannaPersuasion(currentState, successes),
            WerewolfGiftIdentifiers.FiannaResistToxin => ApplyResistToxin(currentState, successes),
            WerewolfGiftIdentifiers.ChildrenOfGaiaMercy => ApplyMercy(currentState, successes),
            WerewolfGiftIdentifiers.ChildrenOfGaiaMothersTouch => ApplyMothersTouch(currentState, successes),
            WerewolfGiftIdentifiers.BlackFuriesBreathOfTheWyrm => ApplyBreathOfTheWyrm(currentState, successes),
            WerewolfGiftIdentifiers.BlackFuriesHeightenedSenses => ApplyHeightenedSenses(currentState, successes),
            WerewolfGiftIdentifiers.BlackFuriesSenseWyrm => currentState,
            WerewolfGiftIdentifiers.RedTalonsBeastSpeech => ApplyBeastSpeech(currentState, successes),
            WerewolfGiftIdentifiers.RedTalonsWolfAtTheDoor => ApplyWolfAtTheDoor(currentState, successes),
            WerewolfGiftIdentifiers.RedTalonsScentOfRunningWater => currentState,
            WerewolfGiftIdentifiers.SilentStridersSilence => ApplySilence(currentState, successes),
            WerewolfGiftIdentifiers.SilentStridersSpeedOfThought => ApplySpeedOfThought(currentState, successes),
            WerewolfGiftIdentifiers.SilverFangsLambentFlame => ApplyLambentFlame(currentState, successes),
            WerewolfGiftIdentifiers.SilverFangsFalconsGrasp => ApplyFalconsGrasp(currentState, successes),
            WerewolfGiftIdentifiers.BoneGnawersCooking => currentState,
            WerewolfGiftIdentifiers.BoneGnawersStickyFingers => currentState,
            WerewolfGiftIdentifiers.ShadowLordsSeizingTheEdge => ApplySeizingTheEdge(currentState, successes),
            WerewolfGiftIdentifiers.ShadowLordsAuraOfConfidence => ApplyAuraOfConfidence(currentState, successes),
            WerewolfGiftIdentifiers.ShadowLordsFatalFlaw => ApplyFatalFlaw(currentState, successes),
            WerewolfGiftIdentifiers.UktenaSpiritSpeech => ApplySpiritSpeech(currentState, successes),
            WerewolfGiftIdentifiers.UktenaShroud => currentState,
            WerewolfGiftIdentifiers.UktenaSenseMagic => currentState,
            WerewolfGiftIdentifiers.WendigoCamouflage => ApplyCamouflage(currentState, successes),
            WerewolfGiftIdentifiers.WendigoCallTheBreeze => currentState,
            _ => currentState
        };

        if (definition.DurationType != WerewolfGiftDurationType.Instant && definition.DurationType != WerewolfGiftDurationType.Permanent)
        {
            var effectKind = MapEffectKind(definition.GiftKey);
            var magnitude = ComputeMagnitude(definition.GiftKey, successes);
            var activeEffect = new WerewolfActiveGiftEffect(
                definition.GiftKey,
                0,
                definition.DurationType,
                ComputeDurationTurns(definition),
                effectKind,
                magnitude,
                definition.SourceLocator,
                currentState.CurrentSceneToken);

            activeEffects.Add(activeEffect);
            findings.Add($"Active effect registered: {definition.NameEn} (magnitude={magnitude}, duration={activeEffect.DurationType}, scene={currentState.CurrentSceneToken}).");
        }
        else
        {
            findings.Add($"Instant effect applied: {definition.NameEn}.");
        }

        if (activeEffects.Count > 0)
        {
            var currentEffects = (effectResult.ActiveGiftEffects ?? []).ToList();
            currentEffects.AddRange(activeEffects);
            effectResult = effectResult with { ActiveGiftEffects = currentEffects.ToArray() };
        }

        effectResult = WerewolfConditionService.ApplyGiftConditions(effectResult);

        effectResult = effectResult with { RuntimeStateVersion = effectResult.RuntimeStateVersion + 1 };

        return new WerewolfGiftEffectResult(
            true,
            effectResult,
            new ReadOnlyCollection<WerewolfActiveGiftEffect>(activeEffects),
            new ReadOnlyCollection<string>(findings),
            request.RequestId,
            effectResult.RuntimeStateVersion,
            null);
    }

    private static WerewolfRuntimeCharacterState ApplyMasterOfFire(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyCreateElement(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyHareLeap(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyOpenSeal(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplySpiritSpeech(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyResistPain(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyBeastSpeech(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyFallingTouch(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyControlSimpleMachine(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyDiagnostics(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyRazorClaws(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyVisageOfFenris(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyFaerieLight(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyFiannaPersuasion(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyResistToxin(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyMercy(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyMothersTouch(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyBreathOfTheWyrm(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyHeightenedSenses(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyWolfAtTheDoor(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplySilence(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplySpeedOfThought(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyLambentFlame(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyFalconsGrasp(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplySeizingTheEdge(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyAuraOfConfidence(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyFatalFlaw(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyCamouflage(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfActiveGiftEffectKind MapEffectKind(string giftKey)
    {
        return giftKey switch
        {
            WerewolfGiftIdentifiers.HomidMasterOfFire => WerewolfActiveGiftEffectKind.DamageReduction,
            WerewolfGiftIdentifiers.MetisCreateElement => WerewolfActiveGiftEffectKind.ElementalCreation,
            WerewolfGiftIdentifiers.LupusHareLeap => WerewolfActiveGiftEffectKind.MovementBonus,
            WerewolfGiftIdentifiers.RagabashOpenSeal => WerewolfActiveGiftEffectKind.LockOpening,
            WerewolfGiftIdentifiers.TheurgeSpiritSpeech => WerewolfActiveGiftEffectKind.SpiritCommunication,
            WerewolfGiftIdentifiers.PhilodoxResistPain => WerewolfActiveGiftEffectKind.WoundPenaltyRemoval,
            WerewolfGiftIdentifiers.GalliardBeastSpeech => WerewolfActiveGiftEffectKind.Custom,
            WerewolfGiftIdentifiers.AhrounFallingTouch => WerewolfActiveGiftEffectKind.ProneCondition,
            WerewolfGiftIdentifiers.GlassWalkersControlSimpleMachine => WerewolfActiveGiftEffectKind.MachineControl,
            WerewolfGiftIdentifiers.GlassWalkersDiagnostics => WerewolfActiveGiftEffectKind.Custom,
            WerewolfGiftIdentifiers.GlassWalkersTrickShot => WerewolfActiveGiftEffectKind.Custom,
            WerewolfGiftIdentifiers.GetOfFenrisRazorClaws => WerewolfActiveGiftEffectKind.CombatDamageBonus,
            WerewolfGiftIdentifiers.GetOfFenrisResistPain => WerewolfActiveGiftEffectKind.WoundPenaltyRemoval,
            WerewolfGiftIdentifiers.GetOfFenrisVisageOfFenris => WerewolfActiveGiftEffectKind.SocialIntimidationBonus,
            WerewolfGiftIdentifiers.FiannaFaerieLight => WerewolfActiveGiftEffectKind.LightEffect,
            WerewolfGiftIdentifiers.FiannaPersuasion => WerewolfActiveGiftEffectKind.SocialTestBonus,
            WerewolfGiftIdentifiers.FiannaResistToxin => WerewolfActiveGiftEffectKind.PoisonImmunity,
            WerewolfGiftIdentifiers.ChildrenOfGaiaMercy => WerewolfActiveGiftEffectKind.DamageReduction,
            WerewolfGiftIdentifiers.ChildrenOfGaiaMothersTouch => WerewolfActiveGiftEffectKind.HealthLevelRepair,
            WerewolfGiftIdentifiers.BlackFuriesBreathOfTheWyrm => WerewolfActiveGiftEffectKind.MentalTestBonus,
            WerewolfGiftIdentifiers.BlackFuriesHeightenedSenses => WerewolfActiveGiftEffectKind.PerceptionBonus,
            WerewolfGiftIdentifiers.BlackFuriesSenseWyrm => WerewolfActiveGiftEffectKind.WyrmSense,
            WerewolfGiftIdentifiers.RedTalonsBeastSpeech => WerewolfActiveGiftEffectKind.AnimalCommunication,
            WerewolfGiftIdentifiers.RedTalonsWolfAtTheDoor => WerewolfActiveGiftEffectKind.FearAura,
            WerewolfGiftIdentifiers.RedTalonsScentOfRunningWater => WerewolfActiveGiftEffectKind.Custom,
            WerewolfGiftIdentifiers.SilentStridersSilence => WerewolfActiveGiftEffectKind.StealthBonus,
            WerewolfGiftIdentifiers.SilentStridersSpeedOfThought => WerewolfActiveGiftEffectKind.MovementBonus,
            WerewolfGiftIdentifiers.SilverFangsLambentFlame => WerewolfActiveGiftEffectKind.DefenseBonus,
            WerewolfGiftIdentifiers.SilverFangsFalconsGrasp => WerewolfActiveGiftEffectKind.RestrainedCondition,
            WerewolfGiftIdentifiers.BoneGnawersCooking => WerewolfActiveGiftEffectKind.Custom,
            WerewolfGiftIdentifiers.BoneGnawersStickyFingers => WerewolfActiveGiftEffectKind.Custom,
            WerewolfGiftIdentifiers.ShadowLordsSeizingTheEdge => WerewolfActiveGiftEffectKind.TestBonus,
            WerewolfGiftIdentifiers.ShadowLordsAuraOfConfidence => WerewolfActiveGiftEffectKind.AuraBlocking,
            WerewolfGiftIdentifiers.ShadowLordsFatalFlaw => WerewolfActiveGiftEffectKind.CombatDamageBonus,
            WerewolfGiftIdentifiers.UktenaSpiritSpeech => WerewolfActiveGiftEffectKind.SpiritCommunication,
            WerewolfGiftIdentifiers.UktenaShroud => WerewolfActiveGiftEffectKind.Custom,
            WerewolfGiftIdentifiers.UktenaSenseMagic => WerewolfActiveGiftEffectKind.MagicDetection,
            WerewolfGiftIdentifiers.WendigoCamouflage => WerewolfActiveGiftEffectKind.StealthBonus,
            WerewolfGiftIdentifiers.WendigoCallTheBreeze => WerewolfActiveGiftEffectKind.WindEffect,
            _ => WerewolfActiveGiftEffectKind.Custom
        };
    }

    private static int ComputeMagnitude(string giftKey, int successes)
    {
        return giftKey switch
        {
            WerewolfGiftIdentifiers.HomidMasterOfFire => 1,
            WerewolfGiftIdentifiers.GetOfFenrisRazorClaws => 1,
            WerewolfGiftIdentifiers.FiannaPersuasion => 1,
            WerewolfGiftIdentifiers.ShadowLordsFatalFlaw => successes,
            WerewolfGiftIdentifiers.BlackFuriesHeightenedSenses => successes,
            WerewolfGiftIdentifiers.SilentStridersSilence => successes,
            WerewolfGiftIdentifiers.WendigoCamouflage => successes,
            _ => successes
        };
    }

    private static int ComputeDurationTurns(WerewolfGiftDefinition definition)
    {
        return definition.DurationType switch
        {
            WerewolfGiftDurationType.Instant => 0,
            WerewolfGiftDurationType.Turn => 1,
            WerewolfGiftDurationType.Scene => -1,
            WerewolfGiftDurationType.Permanent => -1,
            _ => 0
        };
    }

    public static IReadOnlyList<WerewolfActiveGiftEffect> GetSceneValidEffects(WerewolfRuntimeCharacterState state)
    {
        if (state.ActiveGiftEffects is null || state.ActiveGiftEffects.Count == 0)
        {
            return Array.Empty<WerewolfActiveGiftEffect>();
        }

        if (string.IsNullOrWhiteSpace(state.CurrentSceneToken))
        {
            return state.ActiveGiftEffects;
        }

        return state.ActiveGiftEffects
            .Where(e => string.IsNullOrWhiteSpace(e.SceneToken) || StringComparer.Ordinal.Equals(e.SceneToken, state.CurrentSceneToken))
            .ToList();
    }

    public static IReadOnlyList<WerewolfActiveGiftEffect> GetSceneValidEffects(WerewolfActionResolutionContext context)
    {
        if (context.ActiveGiftEffects is null || context.ActiveGiftEffects.Count == 0)
        {
            return Array.Empty<WerewolfActiveGiftEffect>();
        }

        if (string.IsNullOrWhiteSpace(context.CurrentSceneToken))
        {
            return context.ActiveGiftEffects;
        }

        return context.ActiveGiftEffects
            .Where(e => string.IsNullOrWhiteSpace(e.SceneToken) || StringComparer.Ordinal.Equals(e.SceneToken, context.CurrentSceneToken))
            .ToList();
    }
}
