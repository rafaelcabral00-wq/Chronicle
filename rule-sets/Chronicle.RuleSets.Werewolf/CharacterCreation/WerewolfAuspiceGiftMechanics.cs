using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

/// <summary>
/// One resolved Auspice Gift activation: the typed modifier it produces plus
/// the source-derived numbers behind it.
/// </summary>
public sealed record WerewolfAuspiceGiftMechanic(
    string GiftKey,
    WerewolfActiveGiftEffectKind Kind,
    int Magnitude,
    int DurationTurns,
    object? Payload,
    string SourceLocator);

// ---------------------------------------------------------------------------
// Typed payloads. Every field traces to the cited source line.
// ---------------------------------------------------------------------------
public sealed record WerewolfStealthPoolDeductionPayload(int PoolDifficultyIncrease, int PerSuccessPenalty);

public sealed record WerewolfInvisibilityPayload(int TestDifficulty, bool RequiresMotionless);

public sealed record WerewolfScentTrackingDifficultyPayload(int DifficultyIncrease, bool Inherent);

public sealed record WerewolfPreyTrackingPayload(string OpposingTrait, string SpiritOpposingTrait);

public sealed record WerewolfMoonBridgePayload(int MaxDistanceKilometers, int GnosisCost);

public sealed record WerewolfGremlinDevicePayload(IReadOnlyList<int> DeviceDifficulties, IReadOnlyList<string> DeviceNames, int PermanentDisablingSuccesses);

public sealed record WerewolfSilverMoonBlessingPayload(int AttackerExtraDice, bool CountOnlyForCrits, bool RequiresVisibleMoon);

public sealed record WerewolfPermanentAttributeReductionPayload(int AttributesRemovedPerSuccess, int UsesPerLifetime, bool PhysicalOnly);

public sealed record WerewolfMetamorphosisDifficultyPayload(int MinDifficulty, int MaxDifficulty, IReadOnlyList<string> TierDifficulties);

public sealed record WerewolfHealthRepairPayload(int HealthLevelsPerSuccess, int SecondGnosisForBattleScars);

public sealed record WerewolfVisionDecodingPayload(int TestDifficulty);

public sealed record WerewolfPenumbraPerceptionPayload(int AutomaticGnosisThreshold, int RequiredSuccesses);

public sealed record WerewolfEssenceDrainPayload(int EssencePerSuccessPerTurn, int EssencePerWillpower);

public sealed record WerewolfAnimalLobotomyPayload(int WillpowerDifficultyBonus, int MaxDifficulty, int GnosisPerIntelligencePoint);

public sealed record WerewolfSpiritSummonPayload(int RadiusKilometers, int AdditionalGnosisForRegion);

public sealed record WerewolfWillpowerRecoveryPayload(int TestDifficulty, int WillpowerPerSuccesses, int SuccessesPerPoint, int UsesPerScene);

public sealed record WerewolfExtraDiePayload(int TestDifficulty, int DicePerSuccess, int DurationTurns);

public sealed record WerepoolAncestralWisdomPayload(int TestDifficulty, int ReductionPerAncestorsPoint);

public sealed record WerewolfDistantScentPayload(int TestDifficulty, bool UmbraUsesPeliculaLevel);

public sealed record WerewolfSubmissionPayload(int NetSuccessesRequired);

public sealed record WerewolfMesmerizePayload(int GnosisCost, string OpposingTrait);

public sealed record WerewolfGraniteWallPayload(int LengthMeters, int HeightMeters, int DepthMeters, int AbsorptionDice, int DamageLevelsToBreak);

public sealed record WerewolfTelepathicCommunionPayload(int WillpowerPerMind, int PhysicalPoolReduction, string OpposingTrait);

public sealed record WerewolfDreamCommunionPayload(int TestDifficulty, int GnosisLostIfWoken);

public sealed record WerepoolFrenzyInducedPayload(string OpposingTrait, int TurnsPerSuccess);

public sealed record WerepoolGazePullPayload(string OpposingTrait, int SuccessesRequired);

public sealed record WerewolfShadowTheaterPayload(int SuccessesRequired, int TurnsPerGnosis, string OpposingTrait);

public sealed record WerepoolEmotionManipulationPayload(string OpposingTrait);

public sealed record WerepoolFrenzyWardPayload(int SuccessesPerDifficultyPoint, int EndingWillpowerCost);

public sealed record WerepoolSilverClawsPayload(int TestDifficulty, bool AggravatedIncurable, int AutomaticRagePerTurn, int NonCombatDifficultyIncrease);

public sealed record WerepoolRageRegenerationPayload(int RageRegainedOnDamage, bool NoFrenzyTest);

public sealed record WerepoolIronBitePayload(int MaintenanceDifficulty, int EscapeDamageLevels, double WillpowerAddedFraction);

public sealed record WerepoolSolarKissPayload(int GnosisCost, int BurningAttackBonusDice, bool Aggravated, int ArtificialFlameNumerator);

public sealed record WerepoolUnwaveringWillPayload(int TestDifficulty, int RadiusMeters, int WillpowerPerSuccess, int UsesPerScene, int AbsoluteCap);

public sealed record WerewolfAuspiceBlockedPayload(string Reason, string MissingSubsystem, string SourceLocator);

/// <summary>
/// Source-derived Auspice Gift mechanics for source lines 1871-2103.
/// Gifts whose effect requires an entity-mutating or entity-instantiating
/// subsystem are reported as blocked rather than faked.
/// </summary>
public static class WerewolfAuspiceGiftMechanics
{
    private const int TurnTurns = 1;
    private const int SceneTurns = -1;
    private const int PersistentTurns = -1;

    private static readonly ReadOnlyCollection<string> MetamorphosisTiers =
        Array.AsReadOnly(new[] { "5-equivalent-mammals", "7-larger-reptiles", "9-smaller-amphibians", "10-mythological" });

    public static IReadOnlyList<string> AuspiceGiftKeys { get; } = new ReadOnlyCollection<string>(new[]
    {
        // Ragabash (lines 1873-1917)
        WerewolfGiftIdentifiers.RagabashOpenSeal,
        WerewolfGiftIdentifiers.RagabashEmbacamentoDaPropriaForma,
        WerewolfGiftIdentifiers.RagabashSimularOCheiroDeAguaCorrente,
        WerewolfGiftIdentifiers.RagabashGerarIgnorancia,
        WerewolfGiftIdentifiers.RagabashInduzirEsquecimento,
        WerewolfGiftIdentifiers.RagabashSentirAPresa,
        WerewolfGiftIdentifiers.RagabashAbrirPonteDaLua,
        WerewolfGiftIdentifiers.RagabashGremlins,
        WerewolfGiftIdentifiers.RagabashBencaoDeLuna,
        WerewolfGiftIdentifiers.RagabashFragilizarCorpos,
        WerewolfGiftIdentifiers.RagabashAsMilFormas,
        WerewolfGiftIdentifiers.TheurgeRoubarPoderes,
        // Theurge (lines 1924-1964)
        WerewolfGiftIdentifiers.TheurgeSpiritSpeech,
        WerewolfGiftIdentifiers.TheurgeSentirAWyrm,
        WerewolfGiftIdentifiers.TheurgeToqueDaMae,
        WerewolfGiftIdentifiers.TheurgeComandarEspiritos,
        WerewolfGiftIdentifiers.TheurgeNomeDoEspirito,
        WerewolfGiftIdentifiers.TheurgeVisoes,
        WerewolfGiftIdentifiers.TheurgeExorcismo,
        WerewolfGiftIdentifiers.TheurgePercepcaoDoInvisivel,
        WerewolfGiftIdentifiers.TheurgeCapturaADistancia,
        WerewolfGiftIdentifiers.TheurgeDrenagemEspiritual,
        WerewolfGiftIdentifiers.TheurgeLobotomiaAnimal,
        WerewolfGiftIdentifiers.TheurgeModelagemDeEspirito,
        // Philodox (lines 1969-2013)
        WerewolfGiftIdentifiers.PhilodoxFaroParaAFormaVerdadeira,
        WerewolfGiftIdentifiers.PhilodoxResistPain,
        WerewolfGiftIdentifiers.PhilodoxVerdadeDeGaia,
        WerewolfGiftIdentifiers.PhilodoxChamadoDoDever,
        WerewolfGiftIdentifiers.PhilodoxDeterminacao,
        WerewolfGiftIdentifiers.PhilodoxReiDosAnimais,
        WerewolfGiftIdentifiers.PhilodoxDescobrirCalcanharDeAquiles,
        WerewolfGiftIdentifiers.PhilodoxSabedoriaDasAntigasTradicoes,
        WerewolfGiftIdentifiers.PhilodoxFaroParaGrandesDistancias,
        WerewolfGiftIdentifiers.PhilodoxImposicao,
        WerewolfGiftIdentifiers.PhilodoxMesmerizar,
        WerewolfGiftIdentifiers.PhilodoxParedeDeGranito,
        // Galliard (lines 2018-2056)
        WerewolfGiftIdentifiers.GalliardChamadoDaWyld,
        WerewolfGiftIdentifiers.GalliardComunicacaoComAnimais,
        WerewolfGiftIdentifiers.GalliardComunicacaoTelepatica,
        WerewolfGiftIdentifiers.GalliardChamadoDaWyrm,
        WerewolfGiftIdentifiers.GalliardComunicacaoOnirica,
        WerewolfGiftIdentifiers.GalliardDistracoes,
        WerewolfGiftIdentifiers.GalliardCancaoDaFuria,
        WerewolfGiftIdentifiers.GalliardOlhoDeCobra,
        WerewolfGiftIdentifiers.GalliardAndarilhoDaPonte,
        WerewolfGiftIdentifiers.GalliardTeatroDeSombrio,
        WerewolfGiftIdentifiers.GalliardJogosDaMente,
        WerewolfGiftIdentifiers.GalliardMaterializacaoDeSonhos,
        // Ahroun (lines 2063-2101)
        WerewolfGiftIdentifiers.AhrounGarrasAfiadas,
        WerewolfGiftIdentifiers.AhrounInspiracao,
        WerewolfGiftIdentifiers.AhrounFallingTouch,
        WerewolfGiftIdentifiers.AhrounEspiritoDaBatalha,
        WerewolfGiftIdentifiers.AhrounMedoVerdadeiro,
        WerewolfGiftIdentifiers.AhrounSentirAPrata,
        WerewolfGiftIdentifiers.AhrounCoracaoDaFuria,
        WerewolfGiftIdentifiers.AhrounGarrasDePrata,
        WerewolfGiftIdentifiers.AhrounAticandoAFornalhaDaFuria,
        WerewolfGiftIdentifiers.AhrounMordidaDeFerro,
        WerewolfGiftIdentifiers.AhrounBeijoDeHelios,
        WerewolfGiftIdentifiers.AhrounVontadeInabalavel
    });

    /// <summary>
    /// Auspice Gifts whose complete effect needs a subsystem that does not
    /// exist. Reported explicitly, never faked.
    /// </summary>
    public static IReadOnlyList<string> BlockedGiftKeys { get; } = new ReadOnlyCollection<string>(new[]
    {
        // Line 1964: alters a spirit's form, disposition or type. No spirit
        // mutation capability exists.
        WerewolfGiftIdentifiers.TheurgeModelagemDeEspirito,
        // Line 2056: gives physical form to imagined creatures. No entity
        // instantiation from imagination exists.
        WerewolfGiftIdentifiers.GalliardMaterializacaoDeSonhos
    });

    public static bool IsAuspiceGift(string giftKey) =>
        AuspiceGiftKeys.Contains(giftKey, StringComparer.Ordinal);

    public static bool IsBlocked(string giftKey) =>
        BlockedGiftKeys.Contains(giftKey, StringComparer.Ordinal);

    public static (WerewolfBreedGiftStatus Status, string? MissingDependency) StatusOf(string giftKey)
    {
        if (IsBlocked(giftKey))
        {
            return (WerewolfBreedGiftStatus.Blocked, BlockedSubsystem(giftKey));
        }

        return Resolve(giftKey, 0, WerewolfFormIdentifiers.Homid, EmptySheet()) is null
            ? (WerewolfBreedGiftStatus.Blocked, "No mechanic is registered for this Auspice Gift.")
            : (WerewolfBreedGiftStatus.Executable, null);
    }

    public static IReadOnlyList<WerewolfBreedGiftAudit> Audit()
    {
        var audit = new List<WerewolfBreedGiftAudit>(AuspiceGiftKeys.Count);

        foreach (var giftKey in AuspiceGiftKeys)
        {
            var definition = WerewolfGiftCatalog.Get(giftKey);
            var (status, dependency) = StatusOf(giftKey);

            audit.Add(new WerewolfBreedGiftAudit(
                giftKey, definition?.NameEn ?? string.Empty, definition?.NamePtBr ?? string.Empty,
                definition?.Level ?? 0, definition?.OwnerKey ?? string.Empty,
                definition?.SourceLocator ?? string.Empty, status, dependency));
        }

        return audit.AsReadOnly();
    }

    public static WerewolfAuspiceGiftMechanic? Resolve(
        string giftKey,
        int successes,
        string currentForm,
        Dictionary<string, int> sheet)
    {
        var source = WerewolfGiftCatalog.Get(giftKey)?.SourceLocator ?? "Line 0";
        var s = Math.Max(0, successes);

        if (IsBlocked(giftKey))
        {
            return new(giftKey, WerewolfActiveGiftEffectKind.Custom, s, 0,
                new WerewolfAuspiceBlockedPayload(BlockedReason(giftKey), BlockedSubsystem(giftKey), source), source);
        }

        return giftKey switch
        {
            // Line 1876: Gnosis test vs the local Película level.
            WerewolfGiftIdentifiers.RagabashOpenSeal =>
                new(giftKey, WerewolfActiveGiftEffectKind.LockOpening, 1, SceneTurns, null, source),

            // Line 1880: Manipulation + Stealth @8; each success +1 Perception difficulty.
            WerewolfGiftIdentifiers.RagabashEmbacamentoDaPropriaForma =>
                new(giftKey, WerewolfActiveGiftEffectKind.StealthBonus, s, SceneTurns,
                    new WerewolfStealthPoolDeductionPayload(s, 1), source),

            // Line 1883: inherent; +2 difficulty on any test to track the character.
            WerewolfGiftIdentifiers.RagabashSimularOCheiroDeAguaCorrente =>
                new(giftKey, WerewolfActiveGiftEffectKind.StealthBonus, 2, PersistentTurns,
                    new WerewolfScentTrackingDifficultyPayload(2, true), source),

            // Line 1887: Dexterity + Stealth @7; each success subtracts successes.
            WerewolfGiftIdentifiers.RagabashGerarIgnorancia =>
                new(giftKey, WerewolfActiveGiftEffectKind.StealthBonus, s, SceneTurns,
                    new WerewolfInvisibilityPayload(7, true), source),

            // Line 1890: automatic unless actively concealed or a spirit.
            WerewolfGiftIdentifiers.RagabashInduzirEsquecimento =>
                new(giftKey, WerewolfActiveGiftEffectKind.ObjectTransformation, 3, InstantTurns,
                    new WerewolfPreyTrackingPayload("Wits+Stealth", "Gnosis"), source),

            // Line 1893: automatic; conditional Perception + Enigmas.
            WerewolfGiftIdentifiers.RagabashSentirAPresa =>
                new(giftKey, WerewolfActiveGiftEffectKind.PreySensing, 1, SceneTurns,
                    new WerewolfPreyTrackingPayload("Wits+Stealth", "Gnosis"), source),

            // Line 1896: 1 Gnosis; max distance 1,500 km.
            WerewolfGiftIdentifiers.RagabashAbrirPonteDaLua =>
                new(giftKey, WerewolfActiveGiftEffectKind.UmbraCrossing, 1500, TurnTurns,
                    new WerewolfMoonBridgePayload(1500, 1), source),

            // Line 1899-1905: Manipulation + Intimidation; device table; 5 successes.
            WerewolfGiftIdentifiers.RagabashGremlins =>
                new(giftKey, WerewolfActiveGiftEffectKind.MachineControl, s, TurnTurns,
                    new WerewolfGremlinDevicePayload([4, 6, 8, 10], ["Computador", "Telefone", "Automóvel", "Faca"], 5), source),

            // Line 1909: 3 extra dice counting only for criticals.
            WerewolfGiftIdentifiers.RagabashBencaoDeLuna =>
                new(giftKey, WerewolfActiveGiftEffectKind.FireImmunity, 3, SceneTurns,
                    new WerewolfSilverMoonBlessingPayload(3, true, true), source),

            // Line 1912: 1 Gnosis; permanently removes 1 physical attribute per success.
            WerewolfGiftIdentifiers.RagabashFragilizarCorpos =>
                new(giftKey, WerewolfActiveGiftEffectKind.AttributeReduction, s, InstantTurns,
                    new WerewolfPermanentAttributeReductionPayload(1, 1, true), source),

            // Line 1916: 1 Gnosis; difficulty 5-10 by creature tier.
            WerewolfGiftIdentifiers.RagabashAsMilFormas =>
                new(giftKey, WerewolfActiveGiftEffectKind.FormTransformation, 1, SceneTurns,
                    new WerewolfMetamorphosisDifficultyPayload(5, 10, MetamorphosisTiers), source),

            // Line 1919: 3 successes; 1 Gnosis per turn of use.
            WerewolfGiftIdentifiers.TheurgeRoubarPoderes =>
                new(giftKey, WerewolfActiveGiftEffectKind.PowerStealing, 3, TurnTurns,
                    new WerewolfSpiritRepulsionPayload(0, 0, 0), source),

            // Line 1926: inherent once learned.
            WerewolfGiftIdentifiers.TheurgeSpiritSpeech =>
                new(giftKey, WerewolfActiveGiftEffectKind.SpiritCommunication, 1, PersistentTurns, null, source),

            // Line 1929: Perception + Occult, variable difficulty.
            WerewolfGiftIdentifiers.TheurgeSentirAWyrm =>
                new(giftKey, WerewolfActiveGiftEffectKind.WyrmSense, 1, SceneTurns, null, source),

            // Line 1933: 1 Gnosis; 1 health level repaired per success.
            WerewolfGiftIdentifiers.TheurgeToqueDaMae =>
                new(giftKey, WerewolfActiveGiftEffectKind.HealthLevelRepair, s, InstantTurns,
                    new WerewolfHealthRepairPayload(1, 1), source),

            // Line 1936: 1 Willpower per command; Charisma + Leadership.
            WerewolfGiftIdentifiers.TheurgeComandarEspiritos =>
                new(giftKey, WerewolfActiveGiftEffectKind.SpiritCommand, 1, TurnTurns,
                    new WerewolfMesmerizePayload(1, "Gnosis"), source),

            // Line 1939: 1 Willpower; Perception + Occult @8.
            WerewolfGiftIdentifiers.TheurgeNomeDoEspirito =>
                new(giftKey, WerewolfActiveGiftEffectKind.SpiritDetection, s, SceneTurns,
                    new WerewolfResistedTestPayload(8, "Gnosis", s, "SpiritAttributesRead"), source),

            // Line 1943: Narrator may require Wits + Occult @7 to decode.
            WerewolfGiftIdentifiers.TheurgeVisoes =>
                new(giftKey, WerewolfActiveGiftEffectKind.MentalTestBonus, 1, SceneTurns,
                    new WerewolfVisionDecodingPayload(7), source),

            // Line 1946: 3 turns of concentration; resisted tests.
            WerewolfGiftIdentifiers.TheurgeExorcismo =>
                new(giftKey, WerewolfActiveGiftEffectKind.SpiritPossession, 3, TurnTurns,
                    new WerewolfMesmerizePayload(0, "Willpower"), source),

            // Line 1952: automatic at Gnosis >= Película, else 1 Gnosis success.
            WerewolfGiftIdentifiers.TheurgePercepcaoDoInvisivel =>
                new(giftKey, WerewolfActiveGiftEffectKind.PerceptionBonus, 1, SceneTurns,
                    new WerewolfPenumbraPerceptionPayload(0, 1), source),

            // Line 1955: 1-3 Willpower by size; Gnosis test; 3 successes if forced.
            WerewolfGiftIdentifiers.TheurgeCapturaADistancia =>
                new(giftKey, WerewolfActiveGiftEffectKind.UmbraCrossing, 3, InstantTurns,
                    new WerewolfResistedTestPayload(0, "Willpower", 3, "ForcedAbductionSuccesses"), source),

            // Line 1959: 1 Essence per success per turn; 2 Essence = 1 Willpower.
            WerewolfGiftIdentifiers.TheurgeDrenagemEspiritual =>
                new(giftKey, WerewolfActiveGiftEffectKind.EssenceDrain, s, SceneTurns,
                    new WerewolfEssenceDrainPayload(1, 2), source),

            // Line 1963: Willpower + 3, max 10; 2 Gnosis per Intelligence point.
            WerewolfGiftIdentifiers.TheurgeLobotomiaAnimal =>
                new(giftKey, WerewolfActiveGiftEffectKind.AttributeReduction, s, InstantTurns,
                    new WerewolfAnimalLobotomyPayload(3, 10, 2), source),

            // Line 1971: automatic for werewolves; 2 or 4 successes otherwise.
            WerewolfGiftIdentifiers.PhilodoxFaroParaAFormaVerdadeira =>
                new(giftKey, WerewolfActiveGiftEffectKind.FormDetection, 1, SceneTurns, null, source),

            // Line 1974: 1 Willpower; ignores all wound penalties for the scene.
            WerewolfGiftIdentifiers.PhilodoxResistPain =>
                new(giftKey, WerewolfActiveGiftEffectKind.WoundPenaltyRemoval, 1, SceneTurns, null, source),

            // Line 1977: Intelligence + Empathy against Manipulation + Subterfuge.
            WerewolfGiftIdentifiers.PhilodoxVerdadeDeGaia =>
                new(giftKey, WerewolfActiveGiftEffectKind.LieDetection, 1, SceneTurns, null, source),

            // Line 1981: Charisma + Leadership vs spirit Willpower; 1.5 km; +2 Gnosis.
            WerewolfGiftIdentifiers.PhilodoxChamadoDoDever =>
                new(giftKey, WerewolfActiveGiftEffectKind.SpiritCommand, 1, SceneTurns,
                    new WerewolfSpiritSummonPayload(1, 2), source),

            // Line 1985: Stamina + Rituals @7; 1 Willpower per 2 successes.
            WerewolfGiftIdentifiers.PhilodoxDeterminacao =>
                new(giftKey, WerewolfActiveGiftEffectKind.WillpowerRecovery, s, InstantTurns,
                    new WerewolfWillpowerRecoveryPayload(7, 1, 2, 1), source),

            // Line 1988: Charisma + Animal Empathy against a relationship-based pool.
            WerewolfGiftIdentifiers.PhilodoxReiDosAnimais =>
                new(giftKey, WerewolfActiveGiftEffectKind.AnimalCommand, 1, SceneTurns, null, source),

            // Line 1997: Perception + Brawl @8; 1 extra die per success.
            WerewolfGiftIdentifiers.PhilodoxDescobrirCalcanharDeAquiles =>
                new(giftKey, WerewolfActiveGiftEffectKind.CombatModifier, s, SceneTurns,
                    new WerewolfExtraDiePayload(8, 1, SceneTurns), source),

            // Line 2000: Gnosis @9, -1 per Ancestors point.
            WerewolfGiftIdentifiers.PhilodoxSabedoriaDasAntigasTradicoes =>
                new(giftKey, WerewolfActiveGiftEffectKind.KnowledgeRetrieval, 1, InstantTurns,
                    new WerepoolAncestralWisdomPayload(9, 1), source),

            // Line 2004: Perception + Enigmas @8, or local Película in the Umbra.
            WerewolfGiftIdentifiers.PhilodoxFaroParaGrandesDistancias =>
                new(giftKey, WerewolfActiveGiftEffectKind.SensoryEnhancement, 1, SceneTurns,
                    new WerewolfDistantScentPayload(8, true), source),

            // Line 2007: resisted Willpower; 3 net successes.
            WerewolfGiftIdentifiers.PhilodoxImposicao =>
                new(giftKey, WerewolfActiveGiftEffectKind.Submissao, 3, TurnTurns,
                    new WerewolfSubmissionPayload(3), source),

            // Line 2011: 1 Gnosis; Manipulation + Leadership vs Willpower.
            WerewolfGiftIdentifiers.PhilodoxMesmerizar =>
                new(giftKey, WerewolfActiveGiftEffectKind.MentalDominance, 1, SceneTurns,
                    new WerewolfMesmerizePayload(1, "Willpower"), source),

            // Line 2015: 1 Gnosis; 3x2x1 m; 10 absorption dice; 15 levels to break.
            WerewolfGiftIdentifiers.PhilodoxParedeDeGranito =>
                new(giftKey, WerewolfActiveGiftEffectKind.DamageReduction, 10, SceneTurns,
                    new WerewolfGraniteWallPayload(3, 2, 1, 10, 15), source),

            // Line 2020: Stamina + Empathy; Narrator defines bonuses.
            WerewolfGiftIdentifiers.GalliardChamadoDaWyld =>
                new(giftKey, WerewolfActiveGiftEffectKind.InitiativeBonus, 1, SceneTurns, null, source),

            // Line 2023: Charisma + Animal Empathy, one test per species.
            WerewolfGiftIdentifiers.GalliardComunicacaoComAnimais =>
                new(giftKey, WerewolfActiveGiftEffectKind.AnimalCommunication, 1, SceneTurns, null, source),

            // Line 2026: 1 Willpower per sane mind; -2 physical dice.
            WerewolfGiftIdentifiers.GalliardComunicacaoTelepatica =>
                new(giftKey, WerewolfActiveGiftEffectKind.MentalCommunication, 2, SceneTurns,
                    new WerewolfTelepathicCommunionPayload(1, 2, "Willpower"), source),

            // Line 2030: resisted Manipulation + Performance, both @7.
            WerewolfGiftIdentifiers.GalliardChamadoDaWyrm =>
                new(giftKey, WerewolfActiveGiftEffectKind.TrapCalling, 1, SceneTurns,
                    new WerewolfResistedTestPayload(7, "Willpower", s, "WyrmAmbush"), source),

            // Line 2033: Wits + Empathy @8; -1 Gnosis if the target wakes.
            WerewolfGiftIdentifiers.GalliardComunicacaoOnirica =>
                new(giftKey, WerewolfActiveGiftEffectKind.DreamCommunion, 1, SceneTurns,
                    new WerewolfDreamCommunionPayload(8, 1), source),

            // Line 2035: Wits + Performance; -1 die per success.
            WerewolfGiftIdentifiers.GalliardDistracoes =>
                new(giftKey, WerewolfActiveGiftEffectKind.TestPenalty, s, TurnTurns,
                    new WerewolfExtraDiePayload(0, -1, TurnTurns), source),

            // Line 2040: Manipulation + Leadership; 1 frenzy turn per success.
            WerewolfGiftIdentifiers.GalliardCancaoDaFuria =>
                new(giftKey, WerewolfActiveGiftEffectKind.FrenzyInduced, s, TurnTurns,
                    new WerepoolFrenzyInducedPayload("Willpower", 1), source),

            // Line 2043: Appearance + Enigmas; 3 successes to pull the target.
            WerewolfGiftIdentifiers.GalliardOlhoDeCobra =>
                new(giftKey, WerewolfActiveGiftEffectKind.Suggestion, 3, InstantTurns,
                    new WerepoolGazePullPayload("Willpower", 3), source),

            // Line 2047: 1 Gnosis; distance equals Gnosis in km.
            WerewolfGiftIdentifiers.GalliardAndarilhoDaPonte =>
                new(giftKey, WerewolfActiveGiftEffectKind.UmbraCrossing, 1, TurnTurns,
                    new WerewolfMoonBridgePayload(0, 1), source),

            // Line 2050: 3 successes; 1 turn per Gnosis invested.
            WerewolfGiftIdentifiers.GalliardTeatroDeSombrio =>
                new(giftKey, WerewolfActiveGiftEffectKind.Compulsion, 3, TurnTurns,
                    new WerewolfShadowTheaterPayload(3, 1, "Willpower"), source),

            // Line 2054: Manipulation + Empathy vs Willpower.
            WerewolfGiftIdentifiers.GalliardJogosDaMente =>
                new(giftKey, WerewolfActiveGiftEffectKind.EmotionManipulation, s, SceneTurns,
                    new WerepoolEmotionManipulationPayload("Willpower"), source),

            // Line 2065: 1 Rage; +1 damage die.
            WerewolfGiftIdentifiers.AhrounGarrasAfiadas =>
                new(giftKey, WerewolfActiveGiftEffectKind.CombatDamageBonus, 1, SceneTurns, null, source),

            // Line 2068: 1 Gnosis; allies get 1 automatic Willpower success.
            WerewolfGiftIdentifiers.AhrounInspiracao =>
                new(giftKey, WerewolfActiveGiftEffectKind.TestBonus, 1, SceneTurns, null, source),

            // Line 2071: Dexterity + Medicine vs Stamina + Athletics; knockdown.
            WerewolfGiftIdentifiers.AhrounFallingTouch =>
                new(giftKey, WerewolfActiveGiftEffectKind.ProneCondition, s, TurnTurns,
                    new WerewolfResistedTestPayload(0, "Stamina+Athletics", s, "Knockdown"), source),

            // Line 2075: inherent; +10 initiative.
            WerewolfGiftIdentifiers.AhrounEspiritoDaBatalha =>
                new(giftKey, WerewolfActiveGiftEffectKind.InitiativeBonus, 10, PersistentTurns, null, source),

            // Line 2078: Strength + Intimidation vs Willpower; 1 turn per success.
            WerewolfGiftIdentifiers.AhrounMedoVerdadeiro =>
                new(giftKey, WerewolfActiveGiftEffectKind.FearAura, s, TurnTurns,
                    new WerewolfResistedTestPayload(0, "Willpower", s, "TurnsIntimidated"), source),

            // Line 2081: Perception + Primal Instinct @7; 3 successes to locate.
            WerewolfGiftIdentifiers.AhrounSentirAPrata =>
                new(giftKey, WerewolfActiveGiftEffectKind.SilverSense, 1, SceneTurns,
                    new WerewolfResistedTestPayload(7, "None", 3, "SuccessesToLocate"), source),

            // Line 2085: Willpower vs permanent Rage; +1 frenzy difficulty per 2.
            WerewolfGiftIdentifiers.AhrounCoracaoDaFuria =>
                new(giftKey, WerewolfActiveGiftEffectKind.FrenzyWard, s, SceneTurns,
                    new WerepoolFrenzyWardPayload(2, 1), source),

            // Line 2088: Gnosis @7; aggravated incurable; +1 Rage per turn.
            WerewolfGiftIdentifiers.AhrounGarrasDePrata =>
                new(giftKey, WerewolfActiveGiftEffectKind.CombatDamageBonus, 1, SceneTurns,
                    new WerepoolSilverClawsPayload(7, true, 1, 1), source),

            // Line 2092: regain 1 Rage on any turn taking damage, no frenzy test.
            WerewolfGiftIdentifiers.AhrounAticandoAFornalhaDaFuria =>
                new(giftKey, WerewolfActiveGiftEffectKind.RageRegeneration, 1, PersistentTurns,
                    new WerepoolRageRegenerationPayload(1, true), source),

            // Line 2095: 1 Rage; maintenance @3; +1 damage on escape.
            WerewolfGiftIdentifiers.AhrounMordidaDeFerro =>
                new(giftKey, WerewolfActiveGiftEffectKind.RestrainedCondition, 1, TurnTurns,
                    new WerepoolIronBitePayload(3, 1, 0.5), source),

            // Line 2099: 1 Gnosis; fire immunity; +2 aggravated dice.
            WerewolfGiftIdentifiers.AhrounBeijoDeHelios =>
                new(giftKey, WerewolfActiveGiftEffectKind.FireImmunity, 2, SceneTurns,
                    new WerepoolSolarKissPayload(1, 2, true, 4), source),

            // Line 2102: 1 Willpower; Charisma + Leadership @8; +1 Willpower each.
            WerewolfGiftIdentifiers.AhrounVontadeInabalavel =>
                new(giftKey, WerewolfActiveGiftEffectKind.WillpowerGrant, s, SceneTurns,
                    new WerepoolUnwaveringWillPayload(8, 30, 1, 1, 10), source),

            _ => null
        };
    }

    private const int InstantTurns = 0;

    private static Dictionary<string, int> EmptySheet() => new(StringComparer.Ordinal);

    private static string BlockedReason(string giftKey) => giftKey switch
    {
        WerewolfGiftIdentifiers.TheurgeModelagemDeEspirito =>
            "Source line 1966 alters a spirit's form, disposition or type. The spirit model " +
            "exposes traits and charms but offers no way to mutate a spirit's characteristics.",
        WerewolfGiftIdentifiers.GalliardMaterializacaoDeSonhos =>
            "Source line 2058 gives physical form to creatures generated by the imagination. " +
            "No capability exists to instantiate an entity from imagination.",
        _ => "Blocked: prerequisite subsystem is not implemented."
    };

    private static string BlockedSubsystem(string giftKey) => giftKey switch
    {
        WerewolfGiftIdentifiers.TheurgeModelagemDeEspirito =>
            "Spirit mutation: a typed operation to change a spirit's Characteristics, " +
            "Disposition or Type on WerewolfSpiritRuntimeState.",
        WerewolfGiftIdentifiers.GalliardMaterializacaoDeSonhos =>
            "Entity instantiation from imagination, plus a Trait-granting extended test " +
            "that can materialise the result.",
        _ => "unknown"
    };
}
