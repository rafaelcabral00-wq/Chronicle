using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

/// <summary>
/// Glass Walkers Tribe Gift mechanics: source section 27, lines 2104-2143.
/// Every assertion below is a number or an outcome the source states, never
/// merely that an activation succeeded or that a payload was produced.
/// </summary>
public sealed class WerewolfTribeGiftTests
{
    private static readonly string[] ManipulationCrafts = ["Manipulation", "Crafts"];
    private static readonly string[] CharismaPerformance = ["Charisma", "Performance"];
    private static readonly string[] WitsScience = ["Wits", "Science"];
    private static readonly string[] SimpleMachineKinds = ["lever", "door", "pulley"];
    private static readonly string[] PerceptionStreetwise = ["Perception", "Streetwise"];
    private static readonly string[] DoppelgangerClasses = ["human", "wolf", "Garou"];
    // =====================================================================
    // 1. Controle de Maquinas Simples - source lines 2107-2109.
    //    1 Willpower, Manipulation + Crafts at difficulty 7, until the end of
    //    the scene, with a binary "commanded" outcome.
    // =====================================================================

    [Fact]
    public void ControlSimpleMachineCostsOneWillpowerAndRollsManipulationPlusCraftsAtSeven()
    {
        // Source line 2109: "Custa 1 ponto de Forca de Vontade e exige teste
        // de Manipulacao + Oficios (dificuldade 7)."
        var state = WithSheet(
            BuildState(),
            new Dictionary<string, int>(StringComparer.Ordinal) { [WerewolfAttributeIdentifiers.Manipulation] = 3 },
            new Dictionary<string, int>(StringComparer.Ordinal) { [WerewolfAbilityIdentifiers.Crafts] = 2 });

        var activation = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-control", state, 1, WerewolfGiftIdentifiers.GlassWalkersControlSimpleMachine));

        Assert.True(activation.Succeeded);
        Assert.NotNull(activation.UpdatedState);
        Assert.Equal(4, activation.UpdatedState!.WillpowerCurrent);
        Assert.Equal(5, activation.UpdatedState.WillpowerPermanent);
        Assert.NotNull(activation.ActivationDefinition);
        Assert.Equal(5, activation.ActivationDefinition.DicePool);
        Assert.Equal(7, activation.ActivationDefinition.Difficulty);
        Assert.Equal(ManipulationCrafts, activation.ActivationDefinition.TestComponents);
        Assert.Empty(activation.ActivationDefinition.UndeterminedTestValues);
        Assert.Equal(WerewolfGiftDurationType.Scene, activation.ActivationDefinition.DurationType);
    }

    [Fact]
    public void ControlSimpleMachineCommandsTheMachinesOnlyOnASuccess()
    {
        var commanded = WerewolfTribeGiftMechanics.Resolve(
            WerewolfGiftIdentifiers.GlassWalkersControlSimpleMachine, 1, WerewolfFormIdentifiers.Homid, EmptySheet());
        var notCommanded = WerewolfTribeGiftMechanics.Resolve(
            WerewolfGiftIdentifiers.GlassWalkersControlSimpleMachine, 0, WerewolfFormIdentifiers.Homid, EmptySheet());

        Assert.NotNull(commanded);
        Assert.NotNull(notCommanded);
        Assert.Equal(WerewolfActiveGiftEffectKind.MachineControl, commanded!.Kind);
        Assert.Equal(WerewolfActiveGiftEffectKind.MachineControl, notCommanded!.Kind);

        var won = Assert.IsType<WerewolfSimpleMachineCommandPayload>(commanded.Payload);
        var lost = Assert.IsType<WerewolfSimpleMachineCommandPayload>(notCommanded.Payload);

        Assert.True(won.IsCommanded);
        Assert.False(lost.IsCommanded);
        Assert.Equal(1, commanded.Magnitude);
        Assert.Equal(0, notCommanded.Magnitude);
        Assert.Equal(7, won.TestDifficulty);
        Assert.Equal("Manipulation", won.TestAttribute);
        Assert.Equal("Crafts", won.TestAbility);
        Assert.Equal(1, won.WillpowerCost);
        Assert.Equal(SimpleMachineKinds, won.MachineKinds);

        // Source line 2109: "Dura ate o final da cena."
        Assert.Equal(-1, commanded.DurationTurns);
        Assert.Equal("Line 2107", commanded.SourceLocator);
    }

    [Fact]
    public void ControlSimpleMachineIsReachableThroughTheRuntimeEffectPath()
    {
        var state = BuildState(WerewolfGiftIdentifiers.GlassWalkersControlSimpleMachine);

        var result = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-control-effect", state, state.RuntimeStateVersion,
            WerewolfGiftIdentifiers.GlassWalkersControlSimpleMachine, 2));

        Assert.True(result.Succeeded);
        var effect = Assert.Single(result.ActiveEffects);
        Assert.Equal(WerewolfActiveGiftEffectKind.MachineControl, effect.EffectKind);
        Assert.Equal(1, effect.Magnitude);
        Assert.Equal(-1, effect.RemainingDuration);
        Assert.Equal(WerewolfGiftDurationType.Scene, effect.DurationType);
        Assert.Equal("Line 2107", effect.SourceLocator);

        var payload = Assert.IsType<WerewolfSimpleMachineCommandPayload>(effect.Payload);
        Assert.True(payload.IsCommanded);
        Assert.Equal(7, payload.TestDifficulty);
    }

    [Fact]
    public void ControlSimpleMachineRegistersNoCommandWithoutASuccess()
    {
        var state = BuildState(WerewolfGiftIdentifiers.GlassWalkersControlSimpleMachine);

        var result = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-control-fail", state, state.RuntimeStateVersion,
            WerewolfGiftIdentifiers.GlassWalkersControlSimpleMachine, 0));

        Assert.True(result.Succeeded);
        var effect = Assert.Single(result.ActiveEffects);
        Assert.Equal(0, effect.Magnitude);
        Assert.False(Assert.IsType<WerewolfSimpleMachineCommandPayload>(effect.Payload).IsCommanded);
    }

    // =====================================================================
    // 2. Numero de Tiro - source lines 2113-2115. The permanent Glory rating
    //    joins the dice pool in indirect shooting maneuvers only, deals no
    //    direct damage, and is permanent.
    // =====================================================================

    [Fact]
    public void TrickShotAddsThePermanentGloryRatingAndNothingElse()
    {
        var withoutGlory = WerewolfTribeGiftMechanics.Resolve(
            WerewolfGiftIdentifiers.GlassWalkersTrickShot, 0, WerewolfFormIdentifiers.Homid, EmptySheet(), permanentGlory: 0);
        var withGlory = WerewolfTribeGiftMechanics.Resolve(
            WerewolfGiftIdentifiers.GlassWalkersTrickShot, 0, WerewolfFormIdentifiers.Homid, EmptySheet(), permanentGlory: 4);

        Assert.NotNull(withoutGlory);
        Assert.NotNull(withGlory);
        Assert.Equal(0, withoutGlory!.Magnitude);
        Assert.Equal(4, withGlory!.Magnitude);

        var payload = Assert.IsType<WerewolfTrickShotPayload>(withGlory.Payload);
        Assert.Equal(4, payload.PermanentGloryDiceBonus);
        Assert.True(payload.IndirectManeuversOnly);

        // Source line 2115: "manobras de tiro indiretas (nao causa dano
        // direto)". The parentheses deny direct damage, so the Gift causes no
        // direct damage; encoding the opposite would contradict the source.
        Assert.True(payload.CausesNoDirectDamage);

        // "Efeito permanente": the effect outlives any scene.
        Assert.Equal(-1, withGlory.DurationTurns);
        Assert.Null(withGlory.DaysPerSuccess);
        Assert.Equal("Line 2113", withGlory.SourceLocator);
    }

    [Fact]
    public void TrickShotTakesNoPermanentMagnitudeFromSuccesses()
    {
        // Source line 2115 states the magnitude as the permanent Glory rating,
        // not as a per-roll success count, so successes must not move it.
        var oneSuccess = WerewolfTribeGiftMechanics.Resolve(
            WerewolfGiftIdentifiers.GlassWalkersTrickShot, 1, WerewolfFormIdentifiers.Homid, EmptySheet(), permanentGlory: 2);
        var threeSuccesses = WerewolfTribeGiftMechanics.Resolve(
            WerewolfGiftIdentifiers.GlassWalkersTrickShot, 3, WerewolfFormIdentifiers.Homid, EmptySheet(), permanentGlory: 2);

        Assert.Equal(2, oneSuccess!.Magnitude);
        Assert.Equal(2, threeSuccesses!.Magnitude);
    }

    [Fact]
    public void TrickShotReadsTheCharactersPermanentGloryThroughTheRuntime()
    {
        var state = BuildState(WerewolfGiftIdentifiers.GlassWalkersTrickShot) with { GloryPermanent = 3, GloryCurrent = 3 };

        var result = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-trick-shot", state, state.RuntimeStateVersion,
            WerewolfGiftIdentifiers.GlassWalkersTrickShot, 1));

        Assert.True(result.Succeeded);
        var effect = Assert.Single(result.ActiveEffects);
        Assert.Equal(3, effect.Magnitude);
        Assert.Equal(-1, effect.RemainingDuration);
        var payload = Assert.IsType<WerewolfTrickShotPayload>(effect.Payload);
        Assert.Equal(3, payload.PermanentGloryDiceBonus);
        Assert.True(payload.IndirectManeuversOnly);

        // Source line 2115 denies direct damage; the runtime payload must say so.
        Assert.True(payload.CausesNoDirectDamage);

        // Negative: a damage-bearing kind would contradict the source outright,
        // so the Gift must not register as a CombatDamageBonus.
        Assert.Equal(WerewolfActiveGiftEffectKind.Custom, effect.EffectKind);
        Assert.NotEqual(WerewolfActiveGiftEffectKind.CombatDamageBonus, effect.EffectKind);
    }

    [Fact]
    public void TrickShotIsPassiveSoActivationRollsNothingAndCostsNothing()
    {
        var state = BuildState(WerewolfGiftIdentifiers.GlassWalkersTrickShot) with { RageCurrent = 3, GnosisCurrent = 4, WillpowerCurrent = 4 };

        var activation = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-trick-activate", state, 1, WerewolfGiftIdentifiers.GlassWalkersTrickShot));

        Assert.True(activation.Succeeded);
        Assert.NotNull(activation.UpdatedState);
        Assert.Equal(3, activation.UpdatedState!.RageCurrent);
        Assert.Equal(4, activation.UpdatedState.GnosisCurrent);
        Assert.Equal(4, activation.UpdatedState.WillpowerCurrent);
        Assert.NotNull(activation.ActivationDefinition);
        Assert.Equal(WerewolfGiftCostType.None, activation.ActivationDefinition.CostType);
        Assert.Equal(0, activation.ActivationDefinition.CostAmount);
        Assert.Equal(WerewolfGiftDurationType.Permanent, activation.ActivationDefinition.DurationType);
    }

    // =====================================================================
    // 3. Doppelganger - source lines 2131-2133. 1 Gnosis, Charisma +
    //    Performance at difficulty 8, one day per success, no Attributes
    //    duplicated.
    // =====================================================================

    [Fact]
    public void DoppelgangerCostsOneGnosisAndRollsCharismaPlusPerformanceAtEight()
    {
        var state = WithSheet(
            BuildState(),
            new Dictionary<string, int>(StringComparer.Ordinal) { [WerewolfAttributeIdentifiers.Charisma] = 2 },
            new Dictionary<string, int>(StringComparer.Ordinal) { [WerewolfAbilityIdentifiers.Performance] = 3 });

        var activation = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-doppel", state, 1, WerewolfGiftIdentifiers.GlassWalkersDoppelganger));

        Assert.True(activation.Succeeded);
        Assert.NotNull(activation.UpdatedState);
        Assert.Equal(4, activation.UpdatedState!.GnosisCurrent);
        Assert.NotNull(activation.ActivationDefinition);
        Assert.Equal(5, activation.ActivationDefinition.DicePool);
        Assert.Equal(8, activation.ActivationDefinition.Difficulty);
        Assert.Equal(CharismaPerformance, activation.ActivationDefinition.TestComponents);
        Assert.Empty(activation.ActivationDefinition.UndeterminedTestValues);
    }

    [Fact]
    public void DoppelgangerLastsOneDayPerSuccessWithoutDuplicatingAttributes()
    {
        var twoSuccesses = WerewolfTribeGiftMechanics.Resolve(
            WerewolfGiftIdentifiers.GlassWalkersDoppelganger, 2, WerewolfFormIdentifiers.Homid, EmptySheet());

        Assert.NotNull(twoSuccesses);
        Assert.Equal(2, twoSuccesses!.Magnitude);
        Assert.Equal(1, twoSuccesses.DaysPerSuccess);
        Assert.Equal(-1, twoSuccesses.DurationTurns);

        var payload = Assert.IsType<WerewolfDoppelgangerPayload>(twoSuccesses.Payload);
        Assert.Equal(8, payload.TestDifficulty);
        Assert.Equal("Charisma", payload.TestAttribute);
        Assert.Equal("Performance", payload.TestAbility);
        Assert.Equal(1, payload.GnosisCost);
        Assert.Equal(1, payload.DaysPerSuccess);
        Assert.False(payload.DuplicatesAttributes);
        Assert.Equal(DoppelgangerClasses, payload.MimickedCreatureClasses);
        Assert.Equal("Line 2131", twoSuccesses.SourceLocator);
    }

    [Fact]
    public void DoppelgangerWithoutASuccessHasNoDurationAtAll()
    {
        var failed = WerewolfTribeGiftMechanics.Resolve(
            WerewolfGiftIdentifiers.GlassWalkersDoppelganger, 0, WerewolfFormIdentifiers.Homid, EmptySheet());

        Assert.NotNull(failed);
        Assert.Equal(0, failed!.Magnitude);
        Assert.Equal(1, failed.DaysPerSuccess);
    }

    [Fact]
    public void DoppelgangerDayDurationIsNotExpressedInTurns()
    {
        // The source says "Dura um dia por sucesso" and defines no turn-to-day
        // conversion, so the turn counter must not claim a per-turn duration.
        var mechanic = WerewolfTribeGiftMechanics.Resolve(
            WerewolfGiftIdentifiers.GlassWalkersDoppelganger, 5, WerewolfFormIdentifiers.Homid, EmptySheet());

        Assert.Equal(-1, mechanic!.DurationTurns);
        Assert.NotEqual(5 * 10, mechanic.DurationTurns);
        Assert.NotEqual(5 * 60, mechanic.DurationTurns);
    }

    // =====================================================================
    // 4. Mecanica do Caos - source lines 2141-2143. Permanent; Rage and Gnosis
    //    on the same turn without penalty, the named exception to the general
    //    prohibition at lines 1677 and 1691.
    // =====================================================================

    [Fact]
    public void ChaosMechanicsIsPermanentAndAllowsRageAndGnosisOnTheSameTurn()
    {
        var mechanic = WerewolfTribeGiftMechanics.Resolve(
            WerewolfGiftIdentifiers.GlassWalkersMecanicaDoCaos, 0, WerewolfFormIdentifiers.Homid, EmptySheet());

        Assert.NotNull(mechanic);
        Assert.Equal(1, mechanic!.Magnitude);
        Assert.Equal(-1, mechanic.DurationTurns);
        Assert.Equal("Line 2141", mechanic.SourceLocator);

        var payload = Assert.IsType<WerewolfChaosMechanicsPayload>(mechanic.Payload);
        Assert.True(payload.AllowsRageAndGnosisSameTurn);
        Assert.True(payload.AllowsInstantFetishGiftAndUmbralActivation);
        Assert.Contains("1677", payload.OverridesRule, StringComparison.Ordinal);
        Assert.Contains("1691", payload.OverridesRule, StringComparison.Ordinal);

        // The unresolved source conflict must travel with the mechanic: line
        // 2143 waives a penalty ("sem penalidade") while the rules it overrides
        // state a flat prohibition, not a penalty.
        Assert.False(string.IsNullOrWhiteSpace(payload.NoPenaltyWordingAmbiguity));
        Assert.Contains("sem penalidade", payload.NoPenaltyWordingAmbiguity, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("2143", payload.NoPenaltyWordingAmbiguity, StringComparison.Ordinal);
        Assert.Contains("prohibition", payload.NoPenaltyWordingAmbiguity, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ChaosMechanicsIsCataloguedAsPermanentWithNoCostAndNoTest()
    {
        // Source line 2143: "Efeito permanente" and no cost, no test.
        var definition = WerewolfGiftCatalog.Get(WerewolfGiftIdentifiers.GlassWalkersMecanicaDoCaos);

        Assert.NotNull(definition);
        Assert.Equal(WerewolfGiftDurationType.Permanent, definition!.DurationType);
        Assert.Equal(WerewolfGiftCostType.None, definition.CostType);
        Assert.Equal(0, definition.CostAmount);
        Assert.Equal(WerewolfGiftActivationType.Active, definition.ActivationType);
        Assert.Null(definition.TestAttribute);
        Assert.Null(definition.TestAbility);
        Assert.Null(definition.TestDifficulty);
    }

    [Fact]
    public void ChaosMechanicsIsReachableThroughTheRuntimeEffectPath()
    {
        var state = BuildState(WerewolfGiftIdentifiers.GlassWalkersMecanicaDoCaos);

        var result = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-chaos", state, state.RuntimeStateVersion,
            WerewolfGiftIdentifiers.GlassWalkersMecanicaDoCaos, 0));

        Assert.True(result.Succeeded);
        var effect = Assert.Single(result.ActiveEffects);
        Assert.Equal(1, effect.Magnitude);
        Assert.Equal(-1, effect.RemainingDuration);
        Assert.Equal(WerewolfGiftDurationType.Permanent, effect.DurationType);
        var payload = Assert.IsType<WerewolfChaosMechanicsPayload>(effect.Payload);
        Assert.True(payload.AllowsRageAndGnosisSameTurn);

        // The recorded source conflict must survive the runtime path too, so no
        // consumer of the effect can read it as a settled rule.
        Assert.False(string.IsNullOrWhiteSpace(payload.NoPenaltyWordingAmbiguity));
        Assert.Contains("sem penalidade", payload.NoPenaltyWordingAmbiguity, StringComparison.OrdinalIgnoreCase);
    }

    // =====================================================================
    // 5. The seven blocked Glass Walkers Gifts each name their own missing
    //    dependency, and the runtime says which one is missing.
    // =====================================================================

    [Theory]
    [InlineData(WerewolfGiftIdentifiers.GlassWalkersDiagnostics)]
    [InlineData(WerewolfGiftIdentifiers.GlassWalkersSentidosCiberneticos)]
    [InlineData(WerewolfGiftIdentifiers.GlassWalkersSobrecargaDeEnergia)]
    [InlineData(WerewolfGiftIdentifiers.GlassWalkersControleDeMaquinasComplexas)]
    [InlineData(WerewolfGiftIdentifiers.GlassWalkersFavorDoElemental)]
    [InlineData(WerewolfGiftIdentifiers.GlassWalkersHarmonia)]
    [InlineData(WerewolfGiftIdentifiers.GlassWalkersInvocarAranhaDeRede)]
    public void EveryBlockedGlassWalkersGiftNamesItsOwnMissingDependencyAtRuntime(string giftKey)
    {
        var state = BuildState(giftKey);

        var result = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            $"req-{giftKey}", state, state.RuntimeStateVersion, giftKey, 1));

        Assert.True(result.Succeeded);
        var blocked = Assert.IsType<WerewolfTribeGiftBlockedPayload>(Assert.Single(result.ActiveEffects).Payload);

        Assert.Equal(WerewolfActiveGiftEffectKind.Custom, result.ActiveEffects[0].EffectKind);
        Assert.False(string.IsNullOrWhiteSpace(blocked.Reason));
        Assert.False(string.IsNullOrWhiteSpace(blocked.MissingSubsystem));
        Assert.DoesNotContain(result.Findings, finding => finding.Contains("unspecified", StringComparison.OrdinalIgnoreCase));

        // The reported dependency is the blocked Gift's own, not a fallback.
        Assert.Contains(
            result.Findings,
            finding => finding.Contains(blocked.MissingSubsystem, StringComparison.Ordinal));
        Assert.Equal(
            WerewolfGiftCatalog.Get(giftKey)!.SourceLocator,
            blocked.SourceLocator);
    }

    [Fact]
    public void BlockedGlassWalkersGiftsAreDistinctlyBlockedWithSevenDifferentDependencies()
    {
        var dependencies = WerewolfTribeGiftMechanics.BlockedGiftKeys
            .Select(key => WerewolfTribeGiftMechanics.StatusOf(key).Reason)
            .ToList();

        Assert.Equal(7, dependencies.Count);
        Assert.Equal(7, dependencies.Distinct(StringComparer.Ordinal).Count());
        Assert.All(dependencies, dependency => Assert.NotEqual("unknown", dependency));

        foreach (var giftKey in WerewolfTribeGiftMechanics.BlockedGiftKeys)
        {
            var mechanic = WerewolfTribeGiftMechanics.Resolve(giftKey, 3, WerewolfFormIdentifiers.Homid, EmptySheet());
            var payload = Assert.IsType<WerewolfTribeGiftBlockedPayload>(mechanic!.Payload);
            Assert.Contains("Source line", payload.Reason, StringComparison.Ordinal);
            Assert.Equal(
                WerewolfTribeGiftMechanics.StatusOf(giftKey).Reason,
                payload.MissingSubsystem);
        }
    }

    // =====================================================================
    // 6. Catalog fidelity: the source's own numbers, and no invented ones.
    // =====================================================================

    [Fact]
    public void EnergyOverloadRollsWitsPlusScienceAtSevenAndTakesOneGnosis()
    {
        // Source line 2122: 1 Gnosis; Wits + Science at difficulty 7. The
        // catalog previously named neither trait, so activation rolled a
        // Gnosis pool instead of the source's test.
        var state = WithSheet(
            BuildState(),
            new Dictionary<string, int>(StringComparer.Ordinal) { [WerewolfAttributeIdentifiers.Wits] = 2 },
            new Dictionary<string, int>(StringComparer.Ordinal) { [WerewolfAbilityIdentifiers.Science] = 3 });

        var activation = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-overload", state, 1, WerewolfGiftIdentifiers.GlassWalkersSobrecargaDeEnergia));

        Assert.True(activation.Succeeded);
        Assert.Equal(4, activation.UpdatedState!.GnosisCurrent);
        Assert.NotNull(activation.ActivationDefinition);
        Assert.Equal(5, activation.ActivationDefinition.DicePool);
        Assert.Equal(7, activation.ActivationDefinition.Difficulty);
        Assert.Equal(WitsScience, activation.ActivationDefinition.TestComponents);
        Assert.DoesNotContain("Gnosis", activation.ActivationDefinition.TestComponents);
        Assert.Empty(activation.ActivationDefinition.UndeterminedTestValues);
    }

    [Fact]
    public void EnergyOverloadPoolFollowsTheCharactersOwnRatings()
    {
        var weak = OverloadPool(wits: 1, science: 1);
        var strong = OverloadPool(wits: 4, science: 3);

        Assert.Equal(2, weak);
        Assert.Equal(7, strong);
    }

    [Theory]
    [InlineData(WerewolfGiftIdentifiers.GlassWalkersDiagnostics, "Perception", "Crafts", 1)]
    [InlineData(WerewolfGiftIdentifiers.GlassWalkersSentidosCiberneticos, "Perception", "Science", 1)]
    public void GiftsTheSourceLeavesWithoutADifficultyReportItAsUndetermined(
        string giftKey,
        string expectedAttribute,
        string expectedAbility,
        int expectedCost)
    {
        // The source states the test but no difficulty. Chronicle must not
        // substitute a difficulty of its own.
        var state = WithSheet(
            BuildState(),
            new Dictionary<string, int>(StringComparer.Ordinal)
            {
                [WerewolfAttributeIdentifiers.Perception] = 3
            },
            new Dictionary<string, int>(StringComparer.Ordinal)
            {
                [WerewolfAbilityIdentifiers.Crafts] = 2,
                [WerewolfAbilityIdentifiers.Science] = 2
            });

        var activation = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            $"req-{giftKey}", state, 1, giftKey));

        Assert.True(activation.Succeeded);
        Assert.NotNull(activation.ActivationDefinition);
        Assert.Equal(5, activation.ActivationDefinition.DicePool);
        Assert.Equal(new[] { expectedAttribute, expectedAbility }, activation.ActivationDefinition.TestComponents);
        Assert.Null(activation.ActivationDefinition.Difficulty);
        Assert.Equal(expectedCost, activation.ActivationDefinition.CostAmount);
        Assert.Contains(
            activation.ActivationDefinition.UndeterminedTestValues,
            reason => reason.Contains("difficulty", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            activation.Findings,
            finding => finding.Contains("undetermined", StringComparison.OrdinalIgnoreCase));

        // The finding must not claim a difficulty the source never gave.
        Assert.DoesNotContain(
            activation.Findings,
            finding => finding.Contains("vs difficulty 6", StringComparison.Ordinal));
    }

    [Fact]
    public void HarmonyRequiresPerceptionPlusStreetwiseBecauseManhaIsTheSourcesNameForStreetwise()
    {
        // Source line 2136: "exige teste de Percepcao + Manha". The source's
        // Manha is Chronicle's Streetwise talent, so both traits of the
        // source's pool are recorded and nothing is left unrepresented.
        var definition = WerewolfGiftCatalog.Get(WerewolfGiftIdentifiers.GlassWalkersHarmonia);

        Assert.NotNull(definition);
        Assert.Equal("Perception", definition!.TestAttribute);
        Assert.Equal("Streetwise", definition.TestAbility);
        Assert.Null(definition.TestDifficulty);

        // Runtime reachability: the ability is not merely a string on the
        // record. Streetwise resolves through "character.ability.streetwise"
        // on the character's own sheet, so a Perception 3 / Streetwise 2
        // character rolls a pool of 5 and names both source traits.
        var state = WithSheet(
            BuildState(),
            new Dictionary<string, int>(StringComparer.Ordinal)
            {
                [WerewolfAttributeIdentifiers.Perception] = 3
            },
            new Dictionary<string, int>(StringComparer.Ordinal)
            {
                [WerewolfAbilityIdentifiers.Streetwise] = 2
            });

        var activation = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-harmony", state, 1, WerewolfGiftIdentifiers.GlassWalkersHarmonia));

        Assert.True(activation.Succeeded);
        Assert.NotNull(activation.ActivationDefinition);
        Assert.Equal(PerceptionStreetwise, activation.ActivationDefinition.TestComponents);
        Assert.Equal(5, activation.ActivationDefinition.DicePool);

        // The source states no difficulty, so the test still cannot be rolled
        // against a source-defined number - and that, not a missing ability, is
        // why the Gift stays blocked.
        Assert.Null(activation.ActivationDefinition.Difficulty);
        Assert.Contains(
            activation.ActivationDefinition.UndeterminedTestValues,
            reason => reason.Contains("difficulty", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            activation.ActivationDefinition.UndeterminedTestValues,
            reason => reason.Contains("names no attribute and no ability", StringComparison.OrdinalIgnoreCase));

        var (status, reason) = WerewolfTribeGiftMechanics.StatusOf(
            WerewolfGiftIdentifiers.GlassWalkersHarmonia);
        Assert.Equal(WerewolfTribeGiftStatus.Blocked, status);

        var mechanic = WerewolfTribeGiftMechanics.Resolve(
            WerewolfGiftIdentifiers.GlassWalkersHarmonia, 2, WerewolfFormIdentifiers.Homid, EmptySheet());
        var payload = Assert.IsType<WerewolfTribeGiftBlockedPayload>(mechanic!.Payload);

        // The block cites only what the source omits: the difficulty, the
        // duration, and a mechanical value for the botch consequence.
        Assert.Contains("no stated difficulty", payload.Reason, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no duration", payload.Reason, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no mechanical value", payload.Reason, StringComparison.OrdinalIgnoreCase);

        // It must never rest on the ability being unrepresentable, because the
        // ability exists and is recorded.
        Assert.DoesNotContain("no ability", payload.Reason, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ability catalogue", payload.Reason, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("no identifier", payload.Reason, StringComparison.OrdinalIgnoreCase);

        // StatusOf reports the missing subsystem and the mechanic carries it as
        // MissingSubsystem, so the two views of the block cannot drift apart.
        Assert.Equal(payload.MissingSubsystem, reason);
        Assert.Contains("stated difficulty", reason, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("no identifier", reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ElementalFavorIsCataloguedWithNoCostAndNoDuration()
    {
        // Source line 2129 states only the resisted test: no cost, no duration.
        var definition = WerewolfGiftCatalog.Get(WerewolfGiftIdentifiers.GlassWalkersFavorDoElemental);

        Assert.NotNull(definition);
        Assert.Equal("Charisma", definition!.TestAttribute);
        Assert.Equal("Subterfuge", definition.TestAbility);
        Assert.Null(definition.TestDifficulty);
        Assert.Equal(WerewolfGiftCostType.None, definition.CostType);
        Assert.Equal(0, definition.CostAmount);
        Assert.NotEqual(WerewolfGiftDurationType.Scene, definition.DurationType);
    }

    [Fact]
    public void ComplexMachineControlAndWebSpiderRecordTheSourceTestAndCost()
    {
        // Source line 2126: 1 Willpower, Manipulation + Science/Computer,
        // "difficulty based on complexity, generally 8".
        var complex = WerewolfGiftCatalog.Get(WerewolfGiftIdentifiers.GlassWalkersControleDeMaquinasComplexas);
        Assert.NotNull(complex);
        Assert.Equal(WerewolfGiftCostType.Willpower, complex!.CostType);
        Assert.Equal(1, complex.CostAmount);
        Assert.Equal("Manipulation", complex.TestAttribute);
        Assert.Equal("Computer", complex.TestAbility);
        Assert.Equal(8, complex.TestDifficulty);
        Assert.Equal(WerewolfGiftDurationType.Scene, complex.DurationType);

        // Source line 2140: 1 Gnosis, Charisma + Computer at difficulty 8.
        var spider = WerewolfGiftCatalog.Get(WerewolfGiftIdentifiers.GlassWalkersInvocarAranhaDeRede);
        Assert.NotNull(spider);
        Assert.Equal(WerewolfGiftCostType.Gnosis, spider!.CostType);
        Assert.Equal(1, spider.CostAmount);
        Assert.Equal("Charisma", spider.TestAttribute);
        Assert.Equal("Computer", spider.TestAbility);
        Assert.Equal(8, spider.TestDifficulty);
    }

    [Fact]
    public void GlassWalkersGiftsCarryFaithfulEnglishInsteadOfPlaceholders()
    {
        foreach (var giftKey in GlassWalkersKeys())
        {
            var definition = WerewolfGiftCatalog.Get(giftKey)!;

            Assert.DoesNotContain("(see the Portuguese description)", definition.EffectDescriptionEn, StringComparison.Ordinal);
            Assert.DoesNotContain("see the Portuguese description", definition.EffectDescriptionEn, StringComparison.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(definition.NameEn));

            // A real assertion: NameEn must be a real English display name, not
            // the raw dotted Gift identifier it used to be. Comparing NameEn to
            // giftKey passed trivially even for the identifier, because the
            // two are different strings; the placeholder itself is what has to
            // be absent.
            Assert.DoesNotContain(giftKey, definition.NameEn, StringComparison.Ordinal);
            Assert.DoesNotContain("GlassWalkers", definition.NameEn, StringComparison.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(definition.NameEn.Trim()));
            Assert.NotEqual(
                definition.NamePtBr,
                definition.NameEn.Equals(definition.NamePtBr, StringComparison.Ordinal)
                    ? $"Untranslated Portuguese name '{definition.NamePtBr}'."
                    : definition.NameEn);
        }
    }

    /// <summary>
    /// The corruption guard must actually reach the records it protects. The
    /// previous version swept only the Glass Walkers Gifts while every string
    /// in its data set lived in another tribe's record, so it was vacuous; and
    /// it stood in a proxy string ("regiao") that occurred nowhere in the
    /// catalog instead of the two strings that were still broken.
    /// </summary>
    [Theory]
    [InlineData(WerewolfGiftIdentifiers.HomidPerturbarTecnologia, "tecnolágicos")]
    [InlineData(WerewolfGiftIdentifiers.GetOfFenrisDeterAFugaDosCovardes, "concentraááo")]
    [InlineData(WerewolfGiftIdentifiers.GetOfFenrisDeterAFugaDosCovardes, "Intimidaááo")]
    [InlineData(WerewolfGiftIdentifiers.GetOfFenrisRugidoDoPredador, "confianáa")]
    [InlineData(WerewolfGiftIdentifiers.ChildrenOfGaiaArmaduraDeLuna, "proteááo")]
    [InlineData(WerewolfGiftIdentifiers.ChildrenOfGaiaArmaduraDeLuna, "resiliáncia")]
    [InlineData(WerewolfGiftIdentifiers.ChildrenOfGaiaArmaduraDeLuna, "Concentraááo")]
    public void CorrectedCatalogRecordsHaveNoCorruptedSpellings(string giftKey, string corrupted)
    {
        var definition = WerewolfGiftCatalog.Get(giftKey);

        Assert.NotNull(definition);
        Assert.DoesNotContain(corrupted, definition!.EffectDescriptionEn, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void HarmonyKeepsTheSourceRenderingManhaAndNeverRewritesItToMania()
    {
        // "Manha" is the source's own rendering of its Streetwise talent. The
        // catalogue must preserve it verbatim and must never "correct" it to
        // "Mania"; it must also stop asserting that Chronicle has no identifier
        // for it, because Streetwise is that identifier.
        var definition = WerewolfGiftCatalog.Get(WerewolfGiftIdentifiers.GlassWalkersHarmonia);

        Assert.NotNull(definition);
        Assert.Equal("Perception", definition!.TestAttribute);
        Assert.Equal("Streetwise", definition.TestAbility);
        Assert.Contains("Manha", definition.EffectDescriptionEn, StringComparison.Ordinal);
        Assert.DoesNotContain("Mania", definition.EffectDescriptionEn, StringComparison.Ordinal);

        // The false claims that the previous remediation baked into shipped
        // data must not return.
        Assert.DoesNotContain("no identifier for it", definition.EffectDescriptionEn, StringComparison.Ordinal);
        Assert.DoesNotContain("no ability is recorded", definition.EffectDescriptionEn, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("rather than substituting Crafts", definition.EffectDescriptionEn, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Treachery", definition.EffectDescriptionEn, StringComparison.Ordinal);

        // The source states no difficulty; cost and level are unchanged.
        Assert.Null(definition.TestDifficulty);
        Assert.Equal(WerewolfGiftCostType.Gnosis, definition.CostType);
        Assert.Equal(1, definition.CostAmount);
        Assert.Equal(4, definition.Level);
        Assert.Equal("Line 2134", definition.SourceLocator);
    }

    // =====================================================================
    // 7. The deferred wave keeps the behaviour it already had. Its reason
    //    states deferral only and makes no claim about source semantics,
    //    because only 11 of the 132 Tribe Gifts were source-audited.
    // =====================================================================

    [Fact]
    public void DeferredTribeGiftsStateDeferralAndClaimNothingAboutTheirSourceSemantics()
    {
        var deferred = WerewolfTribeGiftMechanics.DeferredGiftKeys;

        Assert.Equal(121, deferred.Count);

        // A deferred Gift still keeps whatever behaviour it already had; this
        // one is the subject of the test below.
        Assert.Contains(
            deferred,
            giftKey => giftKey == WerewolfGiftIdentifiers.GetOfFenrisRazorClaws);

        foreach (var giftKey in deferred)
        {
            var (status, reason) = WerewolfTribeGiftMechanics.StatusOf(giftKey);

            Assert.Equal(WerewolfTribeGiftStatus.Deferred, status);
            Assert.False(string.IsNullOrWhiteSpace(reason));
            Assert.Contains("Not yet source-audited", reason, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("pending a later Tribe Gift wave", reason, StringComparison.OrdinalIgnoreCase);

            // The falsifying claim this table used to make for all 121 Gifts:
            // only the 11 Glass Walkers Gifts were source-audited, so nothing
            // supports it, and it must never return.
            Assert.DoesNotContain("semantics are not in question", reason, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("semantics are settled", reason, StringComparison.OrdinalIgnoreCase);

            var mechanic = WerewolfTribeGiftMechanics.Resolve(giftKey, 2, WerewolfFormIdentifiers.Homid, EmptySheet());
            var payload = Assert.IsType<WerewolfTribeGiftBlockedPayload>(mechanic!.Payload);
            Assert.Equal(reason, payload.MissingSubsystem);
            Assert.Contains("Not yet source-audited", payload.Reason, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("semantics are not in question", payload.Reason, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ADedferredTribeGiftsReasonCoversKnownUnauditedSourceConditionsWithoutClaimingSettlement()
    {
        // Counter-examples that make the old blanket claim falsifiable. These
        // deferred Gifts have source conditions that are plainly NOT settled:
        // variable, undefined difficulties, a missing entity, or a
        // "semelhante ao" derivation from a Gift this wave blocked. The reason
        // must therefore stay silent about semantics instead of asserting them.
        var unaudited = new[]
        {
            // Source line 2313-2314: "Semelhante ao dos Andarilhos do Asfalto",
            // i.e. the same urban-elemental Gift that Glass Walkers Favor do
            // Elemental is blocked for at line 2129.
            WerewolfGiftIdentifiers.RedTalonsFavorDoElemental,
            // Source line 2295: difficulty "igual a Pelicula local", undefined
            // as a number.
            WerewolfGiftIdentifiers.BlackFuriesInvocacaoDaWyld,
            // Source line 2317: a Gnosis test with no difficulty at all.
            WerewolfGiftIdentifiers.RedTalonsTerrenoIrrastreavel,
        };

        foreach (var giftKey in unaudited)
        {
            Assert.True(WerewolfTribeGiftMechanics.IsDeferred(giftKey), $"{giftKey} should be deferred.");

            var (_, reason) = WerewolfTribeGiftMechanics.StatusOf(giftKey);

            Assert.NotNull(reason);
            Assert.DoesNotContain("semantics are not in question", reason, StringComparison.OrdinalIgnoreCase);
        }

        // And the counterpart: the Gift that IS source-audited and executable
        // must not borrow the deferral reason.
        var (_, executableReason) = WerewolfTribeGiftMechanics.StatusOf(
            WerewolfGiftIdentifiers.GlassWalkersControlSimpleMachine);

        Assert.Null(executableReason);
    }

    [Fact]
    public void ADedicatedTribeGiftKeepsItsExistingRuntimeEffectBecauseItIsNeverRoutedThroughTheTable()
    {
        // Get of Fenris Razor Claws is deferred, so this block must not change
        // what its activation already produced. WerewolfGiftEffectService only
        // calls WerewolfTribeGiftMechanics.Resolve for executable or blocked
        // Gifts, so a deferred Gift deliberately keeps the effect switch's own
        // behaviour rather than a Kind = Custom placeholder from the table.
        var giftKey = WerewolfGiftIdentifiers.GetOfFenrisRazorClaws;
        Assert.True(WerewolfTribeGiftMechanics.IsDeferred(giftKey));
        Assert.False(WerewolfTribeGiftMechanics.IsExecutable(giftKey));
        Assert.False(WerewolfTribeGiftMechanics.IsBlocked(giftKey));

        var state = BuildState(giftKey);

        var result = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-razor", state, state.RuntimeStateVersion, giftKey, 1));

        Assert.True(result.Succeeded);
        var effect = Assert.Single(result.ActiveEffects);
        Assert.Equal(WerewolfActiveGiftEffectKind.CombatDamageBonus, effect.EffectKind);
        Assert.Equal("Line 2147", effect.SourceLocator);

        // The runtime never carries the table's deferral reason, and the effect
        // is not the table's Custom placeholder. The deferral is reported only
        // by the table's own reporting projections.
        Assert.DoesNotContain(result.Findings, finding => finding.Contains("source-audited", StringComparison.OrdinalIgnoreCase));
        Assert.NotEqual(WerewolfActiveGiftEffectKind.Custom, effect.EffectKind);

        var (deferredStatus, deferredReason) = WerewolfTribeGiftMechanics.StatusOf(giftKey);
        Assert.Equal(WerewolfTribeGiftStatus.Deferred, deferredStatus);
        Assert.Equal(
            "Not yet source-audited or implemented; pending a later Tribe Gift wave.",
            deferredReason);

        // The audit projection is the other place the deferral is visible, and
        // it carries no missing dependency, because no audit established one.
        var audit = WerewolfTribeGiftMechanics.Audit().Single(entry => entry.GiftKey == giftKey);
        Assert.Equal(WerewolfTribeGiftStatus.Deferred, audit.Status);
        Assert.Null(audit.MissingDependency);
    }

    [Fact]
    public void ATribeGiftThatDuplicatesABreedGiftReportsDeferralAndNamesTheAliasOnly()
    {
        // Source line 2207 defines Fianna Remodelar Objeto as identical to the
        // homid Gift. The catalog's alias is a fact about the catalog; it is
        // NOT evidence that this wave audited the Gift's source semantics, so
        // the reason must name the alias and stop there.
        var giftKey = WerewolfGiftIdentifiers.FiannaRemodelarObjeto;
        var definition = WerewolfGiftCatalog.Get(giftKey)!;

        Assert.NotNull(definition.AliasedGiftKey);
        var (status, reason) = WerewolfTribeGiftMechanics.StatusOf(giftKey);

        Assert.Equal(WerewolfTribeGiftStatus.Deferred, status);
        Assert.NotNull(reason);
        Assert.Contains(definition.AliasedGiftKey!, reason, StringComparison.Ordinal);
        Assert.Contains("Not yet source-audited", reason, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("semantics are not in question", reason, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("whose mechanic exists", reason, StringComparison.OrdinalIgnoreCase);
    }

    // =====================================================================
    // Helpers
    // =====================================================================

    private static IEnumerable<string> GlassWalkersKeys() =>
        WerewolfGiftCatalog.AllDefinitions
            .Where(definition => definition.Category == WerewolfGiftCategory.Tribe
                              && definition.OwnerKey == WerewolfTribeIdentifiers.GlassWalkers)
            .Select(definition => definition.GiftKey);

    private static int OverloadPool(int wits, int science)
    {
        var state = WithSheet(
            BuildState(),
            new Dictionary<string, int>(StringComparer.Ordinal) { [WerewolfAttributeIdentifiers.Wits] = wits },
            new Dictionary<string, int>(StringComparer.Ordinal) { [WerewolfAbilityIdentifiers.Science] = science });

        var activation = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-overload-pool", state, 1, WerewolfGiftIdentifiers.GlassWalkersSobrecargaDeEnergia));

        Assert.True(activation.Succeeded);
        return activation.ActivationDefinition!.DicePool!.Value;
    }

    private static Dictionary<string, int> EmptySheet() => new(StringComparer.Ordinal);

    private static WerewolfRuntimeCharacterState WithSheet(
        WerewolfRuntimeCharacterState state,
        IReadOnlyDictionary<string, int> attributes,
        IReadOnlyDictionary<string, int> abilities)
    {
        var binding = new Dictionary<string, string>(state.PackageBinding, StringComparer.Ordinal)
        {
            ["attributes"] = System.Text.Json.JsonSerializer.Serialize(attributes),
            ["abilities"] = System.Text.Json.JsonSerializer.Serialize(abilities)
        };

        return state with { PackageBinding = binding };
    }

    private static WerewolfRuntimeCharacterState BuildState(params string[] activatedGiftKeys) =>
        new(
            PackageId: "test-package",
            PackageVersion: "0.1.0",
            DraftId: "draft-001",
            RuntimeStateVersion: 1,
            PackageBinding: new Dictionary<string, string>(StringComparer.Ordinal),
            RagePermanent: 5,
            RageCurrent: 5,
            GnosisPermanent: 5,
            GnosisCurrent: 5,
            WillpowerPermanent: 5,
            WillpowerCurrent: 5,
            GloryPermanent: 0,
            GloryCurrent: 0,
            HonorPermanent: 0,
            HonorCurrent: 0,
            WisdomPermanent: 0,
            WisdomCurrent: 0,
            BirthRace: WerewolfRaceIdentifiers.Homid,
            HealthTrack: WerewolfHealthTrackComputer.Compute([], hasWeakenedImmuneSystem: false, lastRegenerationTurn: -1),
            CurrentForm: WerewolfFormIdentifiers.Homid,
            Conditions: [],
            FrenzyState: null,
            ActiveGiftEffects: [],
            KnownGiftKeys: WerewolfGiftIdentifiers.Supported.ToList(),
            SceneGiftUsage: new Dictionary<string, int>(StringComparer.Ordinal),
            CurrentSceneToken: string.Empty,
            ActivatedGiftKeys: activatedGiftKeys);
}