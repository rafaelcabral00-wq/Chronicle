using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfGiftEffectRequest(
    string RequestId,
    WerewolfRuntimeCharacterState CurrentState,
    int ExpectedRuntimeStateVersion,
    string GiftKey,
    int ActivationSuccesses,
    string? TargetId = null,
    IReadOnlyList<int>? DiceValues = null);

public sealed record WerewolfGiftEffectResult(
    bool Succeeded,
    WerewolfRuntimeCharacterState? UpdatedState,
    IReadOnlyList<WerewolfActiveGiftEffect> ActiveEffects,
    IReadOnlyList<string> Findings,
    string RequestId,
    int NewRuntimeStateVersion,
    string? ErrorCode = null,
    object? Payload = null);

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
            WerewolfGiftIdentifiers.HomidPersuasao => ApplyHomidPersuasao(currentState, successes),
            WerewolfGiftIdentifiers.HomidFitar => ApplyHomidFitar(currentState, successes),
            WerewolfGiftIdentifiers.HomidInquietacao => ApplyHomidInquietacao(currentState, successes),
            WerewolfGiftIdentifiers.HomidRemodelarObjeto => ApplyHomidRemodelarObjeto(currentState, successes),
            WerewolfGiftIdentifiers.HomidCasulo => ApplyHomidCasulo(currentState, successes),
            WerewolfGiftIdentifiers.FiannaRemodelarObjeto => ApplyFiannaRemodelarObjeto(currentState, successes),
            WerewolfGiftIdentifiers.BoneGnawersRemodelarObjeto => ApplyBoneGnawersRemodelarObjeto(currentState, successes),
            WerewolfGiftIdentifiers.LupusNomeDoEspirito => ApplyLupusNomeDoEspirito(currentState, successes),
            WerewolfGiftIdentifiers.TheurgeNomeDoEspirito => ApplyTheurgeNomeDoEspirito(currentState, successes),
            WerewolfGiftIdentifiers.TheurgeComandarEspiritos => ApplyTheurgeComandarEspiritos(currentState, successes),
            WerewolfGiftIdentifiers.TheurgeExorcismo => ApplyTheurgeExorcismo(currentState, successes),
            WerewolfGiftIdentifiers.TheurgeRoubarPoderes => ApplyTheurgeRoubarPoderes(currentState, successes),
            WerewolfGiftIdentifiers.SilentStridersAlcancarAUmbra => ApplySilentStridersAlcancarAUmbra(currentState, successes),
            WerewolfGiftIdentifiers.TheurgeCapturaADistancia => ApplyTheurgeCapturaADistancia(currentState, successes),
                    _ => currentState
        };

        if (definition.GiftKey == WerewolfGiftIdentifiers.HomidInquietacao)
        {
            var rageEffect = CreateActiveEffect(definition, WerewolfActiveGiftEffectKind.RageRecoveryPenalty, 1, currentState);
            var extendedEffect = CreateActiveEffect(definition, WerewolfActiveGiftEffectKind.ExtendedTestDifficultyModifier, 1, currentState);
            activeEffects.Add(rageEffect);
            activeEffects.Add(extendedEffect);
            findings.Add($"Active effects registered: {definition.NameEn} (RageRecoveryPenalty + ExtendedTestDifficultyModifier).");
        }
        else if (definition.DurationType != WerewolfGiftDurationType.Instant && definition.DurationType != WerewolfGiftDurationType.Permanent)
        {
            var effectKind = MapEffectKind(definition.GiftKey);
            var magnitude = ComputeMagnitude(definition.GiftKey, successes);
            var payload = CreatePayload(definition.GiftKey, effectKind, successes);
            var activeEffect = new WerewolfActiveGiftEffect(
                definition.GiftKey,
                0,
                definition.DurationType,
                ComputeDurationTurns(definition),
                effectKind,
                magnitude,
                definition.SourceLocator,
                currentState.CurrentSceneToken,
                payload);

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

        object? s3Payload = null;
        if (definition.GiftKey is WerewolfGiftIdentifiers.LupusNomeDoEspirito or WerewolfGiftIdentifiers.TheurgeNomeDoEspirito)
        {
            s3Payload = ApplyNomeDoEspirito(effectResult, request.DiceValues, successes);
        }
        else if (definition.GiftKey == WerewolfGiftIdentifiers.TheurgeComandarEspiritos)
        {
            s3Payload = ApplyComandarEspiritos(effectResult, request.DiceValues, successes);
        }
        else if (definition.GiftKey == WerewolfGiftIdentifiers.TheurgeExorcismo)
        {
            s3Payload = ApplyExorcismo(effectResult, successes);
        }
        else if (definition.GiftKey == WerewolfGiftIdentifiers.TheurgeRoubarPoderes)
        {
            s3Payload = ApplyRoubarPoderes(effectResult, successes);
        }
        else if (definition.GiftKey == WerewolfGiftIdentifiers.SilentStridersAlcancarAUmbra)
        {
            s3Payload = ApplyAlcancarAUmbra(effectResult, request.DiceValues, successes);
        }
        else if (definition.GiftKey == WerewolfGiftIdentifiers.TheurgeCapturaADistancia)
        {
            s3Payload = ApplyCapturaADistancia(effectResult, request.DiceValues, successes);
        }

        return new WerewolfGiftEffectResult(
            true,
            effectResult,
            new ReadOnlyCollection<WerewolfActiveGiftEffect>(activeEffects),
            new ReadOnlyCollection<string>(findings),
            request.RequestId,
            effectResult.RuntimeStateVersion,
            null,
            s3Payload);
    }

    private static WerewolfRuntimeCharacterState ApplyHomidPersuasao(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyHomidFitar(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyHomidInquietacao(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyHomidRemodelarObjeto(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyHomidCasulo(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyFiannaRemodelarObjeto(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyBoneGnawersRemodelarObjeto(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }

    private static WerewolfRuntimeCharacterState ApplyLupusNomeDoEspirito(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyTheurgeNomeDoEspirito(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyTheurgeComandarEspiritos(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyTheurgeExorcismo(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyTheurgeRoubarPoderes(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplySilentStridersAlcancarAUmbra(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyTheurgeCapturaADistancia(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
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


    private static WerewolfRuntimeCharacterState ApplyHomidSimularOdorDeHomem(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyHomidPerturbarTecnologia(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyMetisRaivaPrimordial(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyMetisSentirAWyrm(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyMetisCavar(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyMetisOlhosDeGato(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyLupusSentidosAgucados(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyRagabashEmbacamentoDaPropriaForma(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyRagabashSimularOCheiroDeAguaCorrente(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyRagabashInduzirEsquecimento(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyTheurgeSentirAWyrm(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyPhilodoxFaroParaAFormaVerdadeira(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyPhilodoxVerdadeDeGaia(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyPhilodoxReiDosAnimais(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyGalliardComunicacaoComAnimais(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyGalliardDistracoes(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyAhrounGarrasAfiadas(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyAhrounInspiracao(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyAhrounEspiritoDaBatalha(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyAhrounMedoVerdadeiro(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyAhrounSentirAPrata(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyGlassWalkersSentidosCiberneticos(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyGlassWalkersSobrecargaDeEnergia(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyGetOfFenrisDeterAFugaDosCovardes(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyGetOfFenrisRugidoDoPredador(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyFiannaUivoDaBanshee(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyChildrenOfGaiaResistenciaADor(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyChildrenOfGaiaAcalmar(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyChildrenOfGaiaArmaduraDeLuna(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyBlackFuriesMaldicaoDeEolo(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyBlackFuriesSentirAPresa(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyRedTalonsMenteAnimal(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyRedTalonsSentirAPresa(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplySilentStridersSentirAWyrm(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplySilentStridersGerarIgnorancia(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplySilentStridersResistenciaDeMensageiro(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplySilverFangsSentirAWyrm(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplySilverFangsArmaduraDeLuna(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplySilverFangsEmpatia(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyBoneGnawersGerarIgnorancia(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyBoneGnawersOdorRepugnante(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyShadowLordsAplausoTrovejante(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyShadowLordsArmaduraDeLuna(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyUktenaEspiritoDoPassaro(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyUktenaEspiritoDoPeixe(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyWendigoResistenciaADor(WerewolfRuntimeCharacterState state, int successes)
    {
        return state;
    }
    private static WerewolfRuntimeCharacterState ApplyWendigoVentoCortante(WerewolfRuntimeCharacterState state, int successes)
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
            WerewolfGiftIdentifiers.HomidPersuasao => WerewolfActiveGiftEffectKind.SocialTestBonus,
            WerewolfGiftIdentifiers.HomidFitar => WerewolfActiveGiftEffectKind.ProneCondition,
            WerewolfGiftIdentifiers.HomidInquietacao => WerewolfActiveGiftEffectKind.RageRecoveryPenalty,
            WerewolfGiftIdentifiers.HomidRemodelarObjeto => WerewolfActiveGiftEffectKind.ObjectTransformation,
            WerewolfGiftIdentifiers.HomidCasulo => WerewolfActiveGiftEffectKind.DamageReduction,
            WerewolfGiftIdentifiers.FiannaRemodelarObjeto => WerewolfActiveGiftEffectKind.ObjectTransformation,
            WerewolfGiftIdentifiers.BoneGnawersRemodelarObjeto => WerewolfActiveGiftEffectKind.ObjectTransformation,
            WerewolfGiftIdentifiers.LupusNomeDoEspirito => WerewolfActiveGiftEffectKind.SpiritDetection,
            WerewolfGiftIdentifiers.TheurgeNomeDoEspirito => WerewolfActiveGiftEffectKind.SpiritDetection,
            WerewolfGiftIdentifiers.TheurgeComandarEspiritos => WerewolfActiveGiftEffectKind.SpiritCommand,
            WerewolfGiftIdentifiers.TheurgeExorcismo => WerewolfActiveGiftEffectKind.SpiritPossession,
            WerewolfGiftIdentifiers.TheurgeRoubarPoderes => WerewolfActiveGiftEffectKind.CharmActivation,
            WerewolfGiftIdentifiers.SilentStridersAlcancarAUmbra => WerewolfActiveGiftEffectKind.UmbraCrossing,
            WerewolfGiftIdentifiers.TheurgeCapturaADistancia => WerewolfActiveGiftEffectKind.UmbraCrossing,
            WerewolfGiftIdentifiers.HomidSimularOdorDeHomem => WerewolfActiveGiftEffectKind.Custom,            WerewolfGiftIdentifiers.HomidPerturbarTecnologia => WerewolfActiveGiftEffectKind.Custom,            WerewolfGiftIdentifiers.MetisRaivaPrimordial => WerewolfActiveGiftEffectKind.Custom,            WerewolfGiftIdentifiers.MetisSentirAWyrm => WerewolfActiveGiftEffectKind.WyrmSense,            WerewolfGiftIdentifiers.MetisCavar => WerewolfActiveGiftEffectKind.Custom,            WerewolfGiftIdentifiers.MetisOlhosDeGato => WerewolfActiveGiftEffectKind.SensoryEnhancement,            WerewolfGiftIdentifiers.LupusSentidosAgucados => WerewolfActiveGiftEffectKind.SensoryEnhancement,            WerewolfGiftIdentifiers.RagabashEmbacamentoDaPropriaForma => WerewolfActiveGiftEffectKind.StealthBonus,            WerewolfGiftIdentifiers.RagabashSimularOCheiroDeAguaCorrente => WerewolfActiveGiftEffectKind.StealthBonus,            WerewolfGiftIdentifiers.RagabashInduzirEsquecimento => WerewolfActiveGiftEffectKind.Custom,            WerewolfGiftIdentifiers.TheurgeSentirAWyrm => WerewolfActiveGiftEffectKind.WyrmSense,            WerewolfGiftIdentifiers.PhilodoxFaroParaAFormaVerdadeira => WerewolfActiveGiftEffectKind.FormDetection,            WerewolfGiftIdentifiers.PhilodoxVerdadeDeGaia => WerewolfActiveGiftEffectKind.Custom,            WerewolfGiftIdentifiers.PhilodoxReiDosAnimais => WerewolfActiveGiftEffectKind.AnimalCommunication,            WerewolfGiftIdentifiers.GalliardComunicacaoComAnimais => WerewolfActiveGiftEffectKind.AnimalCommunication,            WerewolfGiftIdentifiers.GalliardDistracoes => WerewolfActiveGiftEffectKind.Custom,            WerewolfGiftIdentifiers.AhrounGarrasAfiadas => WerewolfActiveGiftEffectKind.CombatDamageBonus,            WerewolfGiftIdentifiers.AhrounInspiracao => WerewolfActiveGiftEffectKind.TestBonus,            WerewolfGiftIdentifiers.AhrounEspiritoDaBatalha => WerewolfActiveGiftEffectKind.InitiativeBonus,            WerewolfGiftIdentifiers.AhrounMedoVerdadeiro => WerewolfActiveGiftEffectKind.FearAura,            WerewolfGiftIdentifiers.AhrounSentirAPrata => WerewolfActiveGiftEffectKind.PerceptionBonus,            WerewolfGiftIdentifiers.GlassWalkersSentidosCiberneticos => WerewolfActiveGiftEffectKind.SensoryEnhancement,            WerewolfGiftIdentifiers.GlassWalkersSobrecargaDeEnergia => WerewolfActiveGiftEffectKind.Custom,            WerewolfGiftIdentifiers.GetOfFenrisDeterAFugaDosCovardes => WerewolfActiveGiftEffectKind.SocialIntimidationBonus,            WerewolfGiftIdentifiers.GetOfFenrisRugidoDoPredador => WerewolfActiveGiftEffectKind.FearAura,            WerewolfGiftIdentifiers.FiannaUivoDaBanshee => WerewolfActiveGiftEffectKind.FearAura,            WerewolfGiftIdentifiers.ChildrenOfGaiaResistenciaADor => WerewolfActiveGiftEffectKind.WoundPenaltyRemoval,            WerewolfGiftIdentifiers.ChildrenOfGaiaAcalmar => WerewolfActiveGiftEffectKind.Custom,            WerewolfGiftIdentifiers.ChildrenOfGaiaArmaduraDeLuna => WerewolfActiveGiftEffectKind.DefenseBonus,            WerewolfGiftIdentifiers.BlackFuriesMaldicaoDeEolo => WerewolfActiveGiftEffectKind.WindEffect,            WerewolfGiftIdentifiers.BlackFuriesSentirAPresa => WerewolfActiveGiftEffectKind.Custom,            WerewolfGiftIdentifiers.RedTalonsMenteAnimal => WerewolfActiveGiftEffectKind.AnimalCommunication,            WerewolfGiftIdentifiers.RedTalonsSentirAPresa => WerewolfActiveGiftEffectKind.Custom,            WerewolfGiftIdentifiers.SilentStridersSentirAWyrm => WerewolfActiveGiftEffectKind.WyrmSense,            WerewolfGiftIdentifiers.SilentStridersGerarIgnorancia => WerewolfActiveGiftEffectKind.StealthBonus,            WerewolfGiftIdentifiers.SilentStridersResistenciaDeMensageiro => WerewolfActiveGiftEffectKind.Custom,            WerewolfGiftIdentifiers.SilverFangsSentirAWyrm => WerewolfActiveGiftEffectKind.WyrmSense,            WerewolfGiftIdentifiers.SilverFangsArmaduraDeLuna => WerewolfActiveGiftEffectKind.DefenseBonus,            WerewolfGiftIdentifiers.SilverFangsEmpatia => WerewolfActiveGiftEffectKind.Custom,            WerewolfGiftIdentifiers.BoneGnawersGerarIgnorancia => WerewolfActiveGiftEffectKind.StealthBonus,            WerewolfGiftIdentifiers.BoneGnawersOdorRepugnante => WerewolfActiveGiftEffectKind.Custom,            WerewolfGiftIdentifiers.ShadowLordsAplausoTrovejante => WerewolfActiveGiftEffectKind.ProneCondition,            WerewolfGiftIdentifiers.ShadowLordsArmaduraDeLuna => WerewolfActiveGiftEffectKind.DefenseBonus,            WerewolfGiftIdentifiers.UktenaEspiritoDoPassaro => WerewolfActiveGiftEffectKind.MovementBonus,            WerewolfGiftIdentifiers.UktenaEspiritoDoPeixe => WerewolfActiveGiftEffectKind.MovementBonus,            WerewolfGiftIdentifiers.WendigoResistenciaADor => WerewolfActiveGiftEffectKind.WoundPenaltyRemoval,            WerewolfGiftIdentifiers.WendigoVentoCortante => WerewolfActiveGiftEffectKind.WindEffect,
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
            WerewolfGiftIdentifiers.SilverFangsFalconsGrasp => 1,
            WerewolfGiftIdentifiers.HomidInquietacao => 1,
            WerewolfGiftIdentifiers.HomidRemodelarObjeto => successes,
            WerewolfGiftIdentifiers.FiannaRemodelarObjeto => successes,
            WerewolfGiftIdentifiers.BoneGnawersRemodelarObjeto => successes,
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

    private static WerewolfActiveGiftEffect CreateActiveEffect(
        WerewolfGiftDefinition definition,
        WerewolfActiveGiftEffectKind effectKind,
        int magnitude,
        WerewolfRuntimeCharacterState currentState)
    {
        var payload = CreatePayload(definition.GiftKey, effectKind, magnitude);
        return new WerewolfActiveGiftEffect(
            definition.GiftKey,
            0,
            definition.DurationType,
            ComputeDurationTurns(definition),
            effectKind,
            magnitude,
            definition.SourceLocator,
            currentState.CurrentSceneToken,
            payload);
    }

    private static readonly IReadOnlyList<string> RemodelarObjetoResultCategories = Array.AsReadOnly(new[] { "tools", "weapons", "shelter" });

    private static object? CreatePayload(string giftKey, WerewolfActiveGiftEffectKind effectKind, int successes)
    {
        return giftKey switch
        {
            WerewolfGiftIdentifiers.HomidInquietacao when effectKind == WerewolfActiveGiftEffectKind.RageRecoveryPenalty => new WerewolfRageRecoveryPenaltyPayload(
                PenaltyAmount: 1,
                DurationTurns: ComputeDurationTurns(WerewolfGiftCatalog.Get(giftKey)!)),
            WerewolfGiftIdentifiers.HomidInquietacao when effectKind == WerewolfActiveGiftEffectKind.ExtendedTestDifficultyModifier => new WerewolfExtendedTestDifficultyPayload(
                DifficultyIncrease: 1,
                Scope: "prolonged-actions",
                DurationTurns: ComputeDurationTurns(WerewolfGiftCatalog.Get(giftKey)!)),
            WerewolfGiftIdentifiers.HomidRemodelarObjeto => new WerewolfObjectTransformationPayload(
                TargetMaterial: "living-material",
                AllowedResultCategories: RemodelarObjetoResultCategories,
                SupportsPermanentAlteration: true,
                SupportsAggravatedDamage: true,
                VariableDurationTurns: successes),
            WerewolfGiftIdentifiers.FiannaRemodelarObjeto => new WerewolfObjectTransformationPayload(
                TargetMaterial: "living-material",
                AllowedResultCategories: RemodelarObjetoResultCategories,
                SupportsPermanentAlteration: true,
                SupportsAggravatedDamage: true,
                VariableDurationTurns: successes),
            WerewolfGiftIdentifiers.BoneGnawersRemodelarObjeto => new WerewolfObjectTransformationPayload(
                TargetMaterial: "living-material",
                AllowedResultCategories: RemodelarObjetoResultCategories,
                SupportsPermanentAlteration: true,
                SupportsAggravatedDamage: true,
                VariableDurationTurns: successes),
            _ => null
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

    private static WerewolfSpiritRuntimeState ToSpiritState(WerewolfRuntimeCharacterState characterState)
    {
        return new WerewolfSpiritRuntimeState(
            SpiritId: $"{characterState.DraftId}-spirit",
            CategoryKey: "garou",
            WillpowerPermanent: characterState.WillpowerPermanent,
            WillpowerCurrent: characterState.WillpowerCurrent,
            RagePermanent: characterState.RagePermanent,
            RageCurrent: characterState.RageCurrent,
            GnosisPermanent: characterState.GnosisPermanent,
            GnosisCurrent: characterState.GnosisCurrent,
            EssenceCurrent: characterState.WillpowerPermanent + characterState.RagePermanent + characterState.GnosisPermanent,
            IsMaterialized: false,
            KnownCharmKeys: [],
            StateVersion: characterState.RuntimeStateVersion);
    }

    private static DetectionResult ApplyNomeDoEspirito(WerewolfRuntimeCharacterState characterState, IReadOnlyList<int>? diceValues, int activationSuccesses)
    {
        var spiritState = ToSpiritState(characterState);
        var difficulty = 8;
        var request = new DetectionRequest(
            spiritState,
            spiritState.StateVersion,
            Guid.NewGuid().ToString(),
            characterState.GnosisPermanent,
            characterState.GnosisPermanent,
            difficulty,
            diceValues ?? Array.Empty<int>());
        var result = WerewolfSpiritMechanicServices.EvaluateDetection(request);
        return result;
    }

    private static CommandResult ApplyComandarEspiritos(WerewolfRuntimeCharacterState characterState, IReadOnlyList<int>? diceValues, int activationSuccesses)
    {
        var spiritState = ToSpiritState(characterState);
        var targetWillpower = spiritState.WillpowerPermanent;
        var request = new CommandRequest(
            spiritState,
            spiritState.StateVersion,
            Guid.NewGuid().ToString(),
            3,
            3,
            targetWillpower,
            diceValues ?? Array.Empty<int>());
        var result = WerewolfSpiritMechanicServices.EvaluateCommand(request);
        return result;
    }

    private static WerewolfExorcismBoundaryPayload ApplyExorcismo(WerewolfRuntimeCharacterState characterState, int activationSuccesses)
    {
        return new WerewolfExorcismBoundaryPayload(
            GiftKey: WerewolfGiftIdentifiers.TheurgeExorcismo,
            Mechanic: "spirit.possession",
            TargetType: "LocationObjectFetishOrHost",
            RequiredConcentrationTurns: 3,
            ReluctantSpiritTest: "Manipulation + Intimidation vs Spirit Willpower",
            TrappedSpiritTest: "Wits + Subterfuge diff 8 vs creator successes",
            SourceLocator: "Line 1945",
            Note: "S3 represents this as a typed boundary. Full 3-turn concentration mechanics are deferred (Human Decision).");
    }

    private static WerewolfCharmStealBoundaryPayload ApplyRoubarPoderes(WerewolfRuntimeCharacterState characterState, int activationSuccesses)
    {
        return new WerewolfCharmStealBoundaryPayload(
            GiftKey: WerewolfGiftIdentifiers.TheurgeRoubarPoderes,
            StolenCharmKey: "materializar",
            GnosisCostPerTurn: 1,
            SourceLocator: "Line 1917",
            Note: "S3 represents this as a typed boundary. Individual Charm effect execution is deferred (Charms remain D=30).");
    }

    private static WerewolfCrossingModifierPayload ApplyAlcancarAUmbra(WerewolfRuntimeCharacterState characterState, IReadOnlyList<int>? diceValues, int activationSuccesses)
    {
        return new WerewolfCrossingModifierPayload(
            GiftKey: WerewolfGiftIdentifiers.SilentStridersAlcancarAUmbra,
            DifficultyModifier: -2,
            AutomaticCrossing: true,
            NoFuryAllowed: true,
            SourceLocator: "Line 2365",
            Note: "Grants automatic Umbra crossing without reflective surfaces. -2 difficulty for realm entry/exit tests. Cannot use Fury in the same turn.");
    }

    private static WerewolfRemoteTransportBoundaryPayload ApplyCapturaADistancia(WerewolfRuntimeCharacterState characterState, IReadOnlyList<int>? diceValues, int activationSuccesses)
    {
        return new WerewolfRemoteTransportBoundaryPayload(
            GiftKey: WerewolfGiftIdentifiers.TheurgeCapturaADistancia,
            SourceSpiritReference: "Garou caster",
            TargetEntityReference: "TargetId from request",
            CrossingResult: "Gnosis test vs Película required",
            TransportIntent: "Transport target through Película to Umbra",
            DestinationSemantics: "Umbra (Penumbra)",
            ChronicleOrchestrationRequired: "Cross-entity world movement and target state transition",
            SourceLocator: "Line 1954",
            Note: "S3 represents this as a typed boundary. Cross-entity transport orchestration is deferred to Chronicle.");
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
