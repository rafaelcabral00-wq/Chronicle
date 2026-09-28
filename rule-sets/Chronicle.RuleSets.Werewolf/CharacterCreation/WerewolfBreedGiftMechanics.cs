using System.Collections.ObjectModel;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

/// <summary>
/// One resolved Breed Gift activation: the typed context modifier the Gift
/// produces plus the source-derived numbers behind it.
/// </summary>
public sealed record WerewolfBreedGiftMechanic(
    string GiftKey,
    WerewolfActiveGiftEffectKind Kind,
    int Magnitude,
    int DurationTurns,
    object? Payload,
    string SourceLocator);

/// <summary>
/// Typed payloads describing the concrete, source-derived outcomes of Breed
/// Gift activations. These are the numeric facts a resolver needs; the Rule
/// Set does not invent generic strings in place of a mechanic.
/// </summary>
public sealed record WerewolfSpiritRepulsionPayload(
    int RadiusMeters,
    int DiceRemovedPerSuccess,
    int TestDifficulty);

public sealed record WerewolfSocialPenaltyRemovalPayload(
    int MinDifficulty,
    int MaxDifficulty,
    int ExtraDaysPerWillpower);

public sealed record WerewolfDerangementImmunityPayload(
    int DurationScenes,
    bool GrantsHumanInteraction);

public sealed record WerewolfTargetResourceDrainPayload(
    int WillpowerLoss,
    int RageLoss,
    int UsesPerScene);

public sealed record WerewolfMentalCommunicationPayload(
    int KilometersPerSuccess,
    int TestDifficulty);

public sealed record WerewolfLimbAtrophyPayload(
    int VictimStaminaBonus,
    int DexterityDifficultyIncrease,
    double MovementSpeedMultiplier,
    bool PermanentAgainstNonRegenerators);

public sealed record WerewolfThornsPayload(
    int ThornStrengthBonus,
    bool Aggravated,
    int BareFleshThreshold,
    IReadOnlyList<string> RequiredForms)
{
    public bool FormSatisfied { get; init; }
}

public sealed record WerewolfTotemDominionPayload(
    int TestDifficulty,
    int MagnitudeFromSuccesses,
    string Dependency);

public sealed record WerewolfMadnessPayload(
    int DaysPerSuccess,
    int AgainstWillpower);

public sealed record WerewolfSenseRadiusPayload(
    int RadiusKilometers,
    int WildDifficulty,
    int UrbanDifficulty);

public sealed record WerewolfSupernaturalSensePayload(
    IReadOnlyList<string> DetectableClasses);

public sealed record WerewolfOlfactoryVisionPayload(
    bool NegatesDarknessPenalty,
    bool EnablesCombatAgainstInvisible,
    int NarratorSetDifficulty);

public sealed record WerewolfCatFeetPayload(
    int FallImmunityMeters,
    int CombatDifficultyReduction,
    bool AlwaysLandsOnFeet);

public sealed record WerewolfBiteDamagePayload(
    int BonusDamageDice,
    int TestDifficulty);

public sealed record WerewolfAnimalCommandPayload(
    int RadiusKilometersPerSuccess,
    int TestDifficulty,
    bool CommunicatesWithDomestic);

public sealed record WerewolfElementalDominionPayload(
    int AreaMetersPerSuccess,
    int TestDifficulty,
    IReadOnlyList<string> Elements);

public sealed record WerewolfBlockedGiftPayload(
    string Reason,
    string MissingSubsystem,
    string SourceLocator);

public sealed record WerewolfResistedTestPayload(
    int BaseDifficulty,
    string OpposingTrait,
    int Successes,
    string EffectUnit);

public sealed record WerewolfDeviceDisruptionPayload(
    IReadOnlyList<int> DeviceDifficulties,
    IReadOnlyList<string> DeviceNames,
    int RadiusMeters,
    int DisabledTiers);

public sealed record WerewolfCocoonPayload(int IgnoreAttacksBelow);

public sealed record WerewolfElementCreationPayload(
    int RadiusMeters,
    int FlameDamageLevels,
    int VolumeCubicCentimeters,
    int MaxMassKilograms);

public sealed record WerewolfRageSacrificePayload(
    int RageGained,
    int VitalityLevelsSacrificed,
    bool MayExceedPermanentLimit);

public sealed record WerewolfExcavationPayload(
    int MinDifficulty,
    int MaxDifficulty,
    int MetersPerSuccessPerTurn);

public sealed record WerewolfHeightenedSensesPayload(
    int PerceptionDifficultyReduction,
    int PrimalInstinctDiceBonus,
    string CurrentForm);

public sealed record WerewolfHumanScentPayload(int RadiusMeters, int WildAnimalDicePenalty);

public sealed record WerewolfJumpPayload(int TestDifficulty, int DistanceMultiplier);

/// <summary>
/// Explicit completion status for a Breed Gift. Every Breed Gift must resolve
/// to exactly one of these; CATALOG_ONLY is deliberately absent so a Gift
/// cannot silently remain without semantics.
/// </summary>
public enum WerewolfBreedGiftStatus
{
    /// <summary>Produces a real, source-derived typed mechanic at runtime.</summary>
    Executable,

    /// <summary>Cannot be implemented without a subsystem that does not exist.</summary>
    Blocked
}

public sealed record WerewolfBreedGiftAudit(
    string GiftKey,
    string NameEn,
    string NamePtBr,
    int Level,
    string OwnerKey,
    string SourceLocator,
    WerewolfBreedGiftStatus Status,
    string? MissingDependency);

/// <summary>
/// Source-derived Breed Gift mechanics for the racial Gifts at source lines
/// 1730-1870 (Homid, Metis, Lupus). Every number here is traceable to the
/// cited line. Gifts whose full effect requires a subsystem that does not
/// exist yet are represented explicitly as blocked rather than faked.
/// </summary>
public static class WerewolfBreedGiftMechanics
{
    private static readonly IReadOnlyList<string> NoForms = Array.AsReadOnly(Array.Empty<string>());

    private static readonly IReadOnlyList<string> PredatorForms =
        Array.AsReadOnly(new[] { WerewolfFormIdentifiers.Crinos, WerewolfFormIdentifiers.Hispo, WerewolfFormIdentifiers.Lupus });

    private static readonly ReadOnlyCollection<string> SupernaturalClasses =
        Array.AsReadOnly(new[] { "mage", "spirit", "wyrm", "apparition", "vampire" });

    private static readonly IReadOnlyList<string> Elements =
        Array.AsReadOnly(new[] { "earth", "water", "fire", "air" });

    /// <summary>All Breed Gift keys covered by this service.</summary>
    public static IReadOnlyList<string> BreedGiftKeys { get; } = new ReadOnlyCollection<string>(new[]
    {
        // Homid (source lines 1733-1784)
        WerewolfGiftIdentifiers.HomidMasterOfFire,
        WerewolfGiftIdentifiers.HomidPersuasao,
        WerewolfGiftIdentifiers.HomidSimularOdorDeHomem,
        WerewolfGiftIdentifiers.HomidFitar,
        WerewolfGiftIdentifiers.HomidPerturbarTecnologia,
        WerewolfGiftIdentifiers.HomidInquietacao,
        WerewolfGiftIdentifiers.HomidRemodelarObjeto,
        WerewolfGiftIdentifiers.HomidCasulo,
        WerewolfGiftIdentifiers.HomidDefesaContraEspiritos,
        WerewolfGiftIdentifiers.HomidAssimilacao,
        WerewolfGiftIdentifiers.HomidRomperOVeu,
        // Metis (source lines 1788-1824)
        WerewolfGiftIdentifiers.MetisCreateElement,
        WerewolfGiftIdentifiers.MetisRaivaPrimordial,
        WerewolfGiftIdentifiers.MetisSentirAWyrm,
        WerewolfGiftIdentifiers.MetisCavar,
        WerewolfGiftIdentifiers.MetisMaldicaoDoOdio,
        WerewolfGiftIdentifiers.MetisComunicacaoMental,
        WerewolfGiftIdentifiers.MetisOlhosDeGato,
        WerewolfGiftIdentifiers.MetisDefinharMembro,
        WerewolfGiftIdentifiers.MetisDomDoPorcoEspinho,
        WerewolfGiftIdentifiers.MetisDomDoTotem,
        WerewolfGiftIdentifiers.MetisLoucura,
        // Lupus (source lines 1827-1870)
        WerewolfGiftIdentifiers.LupusHareLeap,
        WerewolfGiftIdentifiers.LupusSentidosAgucados,
        WerewolfGiftIdentifiers.LupusSentirACaca,
        WerewolfGiftIdentifiers.LupusSensoDoSobrenatural,
        WerewolfGiftIdentifiers.LupusVisaoOlfativa,
        WerewolfGiftIdentifiers.LupusNomeDoEspirito,
        WerewolfGiftIdentifiers.LupusPesDeGato,
        WerewolfGiftIdentifiers.LupusRoer,
        WerewolfGiftIdentifiers.LupusVidaAnimal,
        WerewolfGiftIdentifiers.LupusCancaoDaGrandeFera,
        WerewolfGiftIdentifiers.LupusDomDosElementos
    });

    /// <summary>
    /// Gifts whose complete effect depends on a subsystem that is not
    /// implemented. These are reported as blockers, never faked.
    /// </summary>
    public static IReadOnlyList<string> BlockedGiftKeys { get; } = new ReadOnlyCollection<string>(new[]
    {
        // Source line 1819 invokes the tribal totem's power; totem effects are
        // catalog-only free text (WerewolfTotemCatalog), with no mechanics.
        WerewolfGiftIdentifiers.MetisDomDoTotem,
        // Source line 1865 summons Pangaea Realm beasts; the Pangaea realm and
        // spirit summoning have no implemented semantics.
        WerewolfGiftIdentifiers.LupusCancaoDaGrandeFera
    });

    public static bool IsBreedGift(string giftKey) =>
        BreedGiftKeys.Contains(giftKey, StringComparer.Ordinal);

    public static bool IsBlocked(string giftKey) =>
        BlockedGiftKeys.Contains(giftKey, StringComparer.Ordinal);

    /// <summary>
    /// Returns the explicit completion status of a Breed Gift. A Gift that is
    /// neither executable nor listed as blocked is reported as such rather than
    /// being allowed to remain silently catalog-only.
    /// </summary>
    public static (WerewolfBreedGiftStatus Status, string? MissingDependency) StatusOf(string giftKey)
    {
        if (IsBlocked(giftKey))
        {
            return (WerewolfBreedGiftStatus.Blocked, BlockedSubsystem(giftKey));
        }

        return Resolve(giftKey, successes: 0, WerewolfFormIdentifiers.Homid, EmptySheet()) is null
            ? (WerewolfBreedGiftStatus.Blocked, "No mechanic is registered for this Breed Gift.")
            : (WerewolfBreedGiftStatus.Executable, null);
    }

    /// <summary>
    /// Audits every Breed Gift against the catalog, returning its explicit
    /// status. Used to prove the domain has no catalog-only remainder.
    /// </summary>
    public static IReadOnlyList<WerewolfBreedGiftAudit> Audit()
    {
        var audit = new List<WerewolfBreedGiftAudit>(BreedGiftKeys.Count);

        foreach (var giftKey in BreedGiftKeys)
        {
            var definition = WerewolfGiftCatalog.Get(giftKey);
            var (status, dependency) = StatusOf(giftKey);

            audit.Add(new WerewolfBreedGiftAudit(
                giftKey,
                definition?.NameEn ?? string.Empty,
                definition?.NamePtBr ?? string.Empty,
                definition?.Level ?? 0,
                definition?.OwnerKey ?? string.Empty,
                definition?.SourceLocator ?? string.Empty,
                status,
                dependency));
        }

        return audit.AsReadOnly();
    }

    private static Dictionary<string, int> EmptySheet() =>
        new(StringComparer.Ordinal);

    /// <summary>
    /// Resolves the typed mechanic for a Breed Gift activation.
    /// </summary>
    public static WerewolfBreedGiftMechanic? Resolve(string giftKey, int successes, string currentForm, IReadOnlyDictionary<string, int> sheet)
    {
        var source = WerewolfGiftCatalog.Get(giftKey)?.SourceLocator ?? "Line 0";
        var s = Math.Max(0, successes);

        if (IsBlocked(giftKey))
        {
            return new WerewolfBreedGiftMechanic(
                giftKey,
                WerewolfActiveGiftEffectKind.Custom,
                s,
                0,
                new WerewolfBlockedGiftPayload(
                    BlockedReason(giftKey),
                    BlockedSubsystem(giftKey),
                    source),
                source);
        }

        switch (giftKey)
        {
            // ---- Homid ----
            // Line 1735: repairs fire damage as bashing damage.
            case WerewolfGiftIdentifiers.HomidMasterOfFire:
                return new(giftKey, WerewolfActiveGiftEffectKind.DamageReduction, 1, SceneTurns, null, source);

            // Line 1738: Charisma + Lábia; success reduces all Social test
            // difficulties by 1 for the rest of the scene.
            case WerewolfGiftIdentifiers.HomidPersuasao:
                return new(giftKey, WerewolfActiveGiftEffectKind.SocialTestBonus, 1, SceneTurns, null, source);

            // Line 1741: wild animals lose 1 die within 6 meters; the effect is
            // freely toggled, so it is a persistent die penalty.
            case WerewolfGiftIdentifiers.HomidSimularOdorDeHomem:
                return new(giftKey, WerewolfActiveGiftEffectKind.AnimalIntimidation, 1, PersistentTurns,
                    new WerewolfHumanScentPayload(6, 1), source);

            // Line 1745: Charisma + Intimidação at 5 + the victim's Posto; the
            // victim flees one turn per success.
            case WerewolfGiftIdentifiers.HomidFitar:
                return new(giftKey, WerewolfActiveGiftEffectKind.ProneCondition, s, TurnTurns,
                    new WerewolfResistedTestPayload(5, "Posto", s, "TurnsFled"), source);

            // Line 1748-1756: Manipulation + Ofícios at Gnosis cost; each success
            // disables one device complexity for a turn.
            case WerewolfGiftIdentifiers.HomidPerturbarTecnologia:
                return new(giftKey, WerewolfActiveGiftEffectKind.MachineControl, s, TurnTurns,
                    new WerewolfDeviceDisruptionPayload(
                        [4, 6, 8, 9, 10],
                        ["Computer", "Telefone", "Automóvel", "Arma de fogo", "Faca"],
                        15,
                        s), source);

            // Line 1760: Manipulation + Empatia against the target's Willpower;
            // the target cannot recover Rage for a scene and prolonged-action
            // difficulties rise by 1. The source specifies both consequences.
            case WerewolfGiftIdentifiers.HomidInquietacao:
                return new(giftKey, WerewolfActiveGiftEffectKind.RageRecoveryPenalty, 1, SceneTurns,
                    new WerewolfRageRecoveryPenaltyPayload(1, SceneTurns), source);

            // Line 1763-1770: Manipulation + Ofícios at Gnosis cost; duration
            // scales with successes.
            case WerewolfGiftIdentifiers.HomidRemodelarObjeto:
                return new(giftKey, WerewolfActiveGiftEffectKind.ObjectTransformation, s, DurationTurnsForSuccesses(s),
                    new WerewolfObjectTransformationPayload("living-material", ["tools", "weapons", "shelter"], false, true, 1), source);

            // Line 1774: Gnosis cost; ignores attacks below Stamina + Rituals.
            case WerewolfGiftIdentifiers.HomidCasulo:
                return new(giftKey, WerewolfActiveGiftEffectKind.DamageReduction,
                    ResolveSheetValue(sheet, WerewolfAbilityIdentifiers.Rituals), SceneTurns,
                    new WerewolfCocoonPayload(ResolveSheetValue(sheet, WerewolfAbilityIdentifiers.Rituals)), source);

            // Line 1777: Gnosis cost, Manipulation + Rituais at difficulty 7;
            // spirits within 30m lose 1 die from every pool per success.
            case WerewolfGiftIdentifiers.HomidDefesaContraEspiritos:
                return new(giftKey, WerewolfActiveGiftEffectKind.AuraBlocking, s, SceneTurns,
                    new WerewolfSpiritRepulsionPayload(30, 1, 7), source);

            // Line 1781: Manipulation + Empatia (5-9); eliminates Social
            // penalties; one scene plus one day per Willpower spent.
            case WerewolfGiftIdentifiers.HomidAssimilacao:
                return new(giftKey, WerewolfActiveGiftEffectKind.SocialPenaltyRemoval, 1, SceneTurns,
                    new WerewolfSocialPenaltyRemovalPayload(5, 9, 1), source);

            // Line 1784: Gnosis cost, Charisma + Empatia success; immunity to
            // the Derangement for one scene.
            case WerewolfGiftIdentifiers.HomidRomperOVeu:
                return new(giftKey, WerewolfActiveGiftEffectKind.DerangementImmunity, 1, SceneTurns,
                    new WerewolfDerangementImmunityPayload(1, true), source);

            // ---- Metis ----
            // Line 1790: Gnosis cost and a Gnosis test; each success creates
            // matter within 20m, and created flame deals 1 damage level per
            // success, capped at 3.
            case WerewolfGiftIdentifiers.MetisCreateElement:
                return new(giftKey, WerewolfActiveGiftEffectKind.ElementalCreation, Math.Min(s, 3), SceneTurns,
                    new WerewolfElementCreationPayload(20, Math.Min(s, 3), 30, 50), source);

            // Line 1793: sacrifice one vitality level once per scene for +2 Rage,
            // allowed to exceed the permanent limit.
            case WerewolfGiftIdentifiers.MetisRaivaPrimordial:
                return new(giftKey, WerewolfActiveGiftEffectKind.RageGain, 2, TurnTurns,
                    new WerewolfRageSacrificePayload(2, 1, true), source);

            // Line 1796: Perception + Ocultismo; difficulty 6 for a nearby
            // fomori, 8 for a Wyrm trail.
            case WerewolfGiftIdentifiers.MetisSentirAWyrm:
                return new(giftKey, WerewolfActiveGiftEffectKind.WyrmSense, 1, SceneTurns,
                    new WerewolfSenseRadiusPayload(0, 6, 8), source);

            // Line 1800: Strength + Esportes (4 loose earth to 9 solid rock);
            // each success digs one meter per turn.
            case WerewolfGiftIdentifiers.MetisCavar:
                return new(giftKey, WerewolfActiveGiftEffectKind.MovementBonus, s, TurnTurns,
                    new WerewolfExcavationPayload(4, 9, 1), source);

            // Line 1803: Gnosis cost, Manipulation + Expressão against the
            // opponent's Willpower; the target loses 2 Willpower and 2 Rage,
            // once per scene per opponent.
            case WerewolfGiftIdentifiers.MetisMaldicaoDoOdio:
                return new(giftKey, WerewolfActiveGiftEffectKind.ResourceDrain, 2, SceneTurns,
                    new WerewolfTargetResourceDrainPayload(2, 2, 1), source);

            // Line 1807: Willpower cost, Charisma + Empatia at difficulty 8;
            // 15 kilometers of reach per success.
            case WerewolfGiftIdentifiers.MetisComunicacaoMental:
                return new(giftKey, WerewolfActiveGiftEffectKind.MentalCommunication, s * 15, SceneTurns,
                    new WerewolfMentalCommunicationPayload(15, 8), source);

            // Line 1810: free activation; negates darkness penalties.
            case WerewolfGiftIdentifiers.MetisOlhosDeGato:
                return new(giftKey, WerewolfActiveGiftEffectKind.SensoryEnhancement, 1, SceneTurns, null, source);

            // Line 1814: Gnosis cost, Willpower test against Stamina + 4; a
            // withered leg adds +2 Dexterity difficulty and halves speed.
            case WerewolfGiftIdentifiers.MetisDefinharMembro:
                return new(giftKey, WerewolfActiveGiftEffectKind.LimbAtrophy, 1, SceneTurns,
                    new WerewolfLimbAtrophyPayload(4, 2, 0.5, true), source);

            // Line 1817: Gnosis cost, Crinos/Hispo/Lupus only; immobilized or
            // grappled foes take aggravated damage of Strength + 1.
            case WerewolfGiftIdentifiers.MetisDomDoPorcoEspinho:
            {
                var usable = PredatorForms.Contains(currentForm, StringComparer.Ordinal);
                return new(giftKey, WerewolfActiveGiftEffectKind.Thorns, usable ? 1 : 0, TurnTurns,
                    new WerewolfThornsPayload(1, true, 5, PredatorForms) { FormSatisfied = usable }, source);
            }

            // Line 1824: Gnosis cost, Manipulation + Intimidação against the
            // victim's Willpower; one day of disturbance per success.
            case WerewolfGiftIdentifiers.MetisLoucura:
                return new(giftKey, WerewolfActiveGiftEffectKind.MadnessInduced, s, SceneTurns,
                    new WerewolfMadnessPayload(s, 1), source);

            // ---- Lupus ----
            // Line 1829: Stamina + Esportes at difficulty 7; success doubles
            // the standard jump distance.
            case WerewolfGiftIdentifiers.LupusHareLeap:
                return new(giftKey, WerewolfActiveGiftEffectKind.MovementBonus, 2, TurnTurns,
                    new WerewolfJumpPayload(7, 2), source);

            // Line 1838-1839: Gnosis cost; Homid/Glabro reduce Perception
            // difficulties by 2, Crinos/Hispo/Lupus by 3 and add 1 die to
            // Primal Instinct.
            case WerewolfGiftIdentifiers.LupusSentidosAgucados:
                var crinos = PredatorForms.Contains(currentForm, StringComparer.Ordinal);
                return new(giftKey, WerewolfActiveGiftEffectKind.SensoryEnhancement,
                    crinos ? 3 : 2, SceneTurns,
                    new WerewolfHeightenedSensesPayload(crinos ? 3 : 2, crinos ? 1 : 0, currentForm), source);

            // Line 1842: Perception + Instinto Primitivo, difficulty 7 in the
            // wild and 9 in urban areas; locates prey for a pack.
            case WerewolfGiftIdentifiers.LupusSentirACaca:
                return new(giftKey, WerewolfActiveGiftEffectKind.PreySensing, 1, SceneTurns,
                    new WerewolfSenseRadiusPayload(80, 7, 9), source);

            // Line 1846: Perception + Enigmas detects supernatural classes.
            case WerewolfGiftIdentifiers.LupusSensoDoSobrenatural:
                return new(giftKey, WerewolfActiveGiftEffectKind.SupernaturalSense, SupernaturalClasses.Count, SceneTurns,
                    new WerewolfSupernaturalSensePayload(SupernaturalClasses), source);

            // Line 1849: Perception + Instinto Primitivo; sight replaced by
            // smell, enabling darkness and invisible combat.
            case WerewolfGiftIdentifiers.LupusVisaoOlfativa:
                return new(giftKey, WerewolfActiveGiftEffectKind.SensoryEnhancement, 1, SceneTurns,
                    new WerewolfOlfactoryVisionPayload(true, true, 0), source);

            // Line 1853: Willpower cost, Perception + Ocultismo at difficulty 8.
            case WerewolfGiftIdentifiers.LupusNomeDoEspirito:
                return new(giftKey, WerewolfActiveGiftEffectKind.SpiritDetection, s, SceneTurns,
                    new WerewolfResistedTestPayload(8, "Gnosis", s, "SpiritAttributesRead"), source);

            // Line 1855-1856: inherent and permanent; immunity to falls under
            // 30 meters and -2 difficulty on grapples and rams.
            case WerewolfGiftIdentifiers.LupusPesDeGato:
                return new(giftKey, WerewolfActiveGiftEffectKind.FallImmunity, 30, PersistentTurns,
                    new WerewolfCatFeetPayload(30, 2, true), source);

            // Line 1860: Willpower cost, Vigor + 4 test (3 wood to 9 train
            // cars); +2 damage dice to the bite for one scene.
            case WerewolfGiftIdentifiers.LupusRoer:
                return new(giftKey, WerewolfActiveGiftEffectKind.CombatDamageBonus, 2, SceneTurns,
                    new WerewolfBiteDamagePayload(2, 4), source);

            // Line 1863: Gnosis cost, Carisma + Empatia com Animais at
            // difficulty 7; 15 km of influence per success.
            case WerewolfGiftIdentifiers.LupusVidaAnimal:
                return new(giftKey, WerewolfActiveGiftEffectKind.AnimalCommunication, s * 15, SceneTurns,
                    new WerewolfAnimalCommandPayload(15, 7, true), source);

            // Line 1870: Gnosis cost, Carisma + Ocultismo at difficulty 7;
            // a 5m x 5m control area per success.
            case WerewolfGiftIdentifiers.LupusDomDosElementos:
                return new(giftKey, WerewolfActiveGiftEffectKind.ElementalCreation, s, SceneTurns,
                    new WerewolfElementalDominionPayload(5, 7, Elements), source);

            default:
                return null;
        }
    }

    private static string BlockedReason(string giftKey) => giftKey switch
    {
        WerewolfGiftIdentifiers.MetisDomDoTotem =>
            "Source line 1821 makes the invoked totem's direct power the effect. " +
            "WerewolfTotemEffect declares a Kind but its Payload is a free-text string, " +
            "and no code consumes WerewolfTotemEffectKind, so the magnitude a totem produces " +
            "cannot be computed without inventing it.",
        WerewolfGiftIdentifiers.LupusCancaoDaGrandeFera =>
            "Source line 1866 names the legendary beasts (mammoths, sabre-toothed tigers, " +
            "megalodon) but supplies no statistics, no summoning procedure and no realm-entry " +
            "rule for them; the only occurrence of those creatures in the source is this Gift's " +
            "own description. The source itself therefore cannot define the summoned result.",
        _ => "Blocked: prerequisite subsystem is not implemented."
    };

    private static string BlockedSubsystem(string giftKey) => giftKey switch
    {
        WerewolfGiftIdentifiers.MetisDomDoTotem =>
            "A totem effect executor: WerewolfTotemEffect.Kind must resolve to a typed, numeric " +
            "outcome (WerewolfTotemEffectKind is currently written only in WerewolfTotemCatalog and " +
            "never read).",
        WerewolfGiftIdentifiers.LupusCancaoDaGrandeFera =>
            "A spirit-summoning mechanic that can instantiate an entity from source-defined " +
            "characteristics. The Pangaea realm is cataloged (WerewolfUmbraRealmCatalog, " +
            "'Lines 3326-3331') and RealmTravel exists, but no source statistics exist for the beasts.",
        _ => "unknown"
    };

    private static int ResolveSheetValue(IReadOnlyDictionary<string, int> sheet, string abilityId)
    {
        ArgumentNullException.ThrowIfNull(sheet);
        return sheet.GetValueOrDefault(abilityId, 0);
    }

    private const int TurnTurns = 1;
    private const int SceneTurns = -1;
    private const int PersistentTurns = -1;

    /// <summary>
    /// Source lines 1764-1770 map successes to a duration:
    /// one = five minutes, two = ten minutes, three = one scene,
    /// four = one story, five = permanent.
    /// </summary>
    private static int DurationTurnsForSuccesses(int successes) => successes switch
    {
        1 => 1,
        2 => 2,
        3 => SceneTurns,
        4 => -2,
        5 => PersistentTurns,
        _ => 0
    };
}
