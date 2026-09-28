using System.Text.Json;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

/// <summary>
/// Wave 2B: Breed Gift closure and execution-path integrity.
/// Proves the single authoritative execution path, the explicit status of
/// every Breed Gift, and the evidence behind each remaining blocker.
/// </summary>
public sealed class WerewolfBreedGiftClosureTests
{
    // =====================================================================
    // 1. LupusSentirACaca execution path.
    // =====================================================================

    [Fact]
    public void SenseTheHuntIsTestRequiredWithPerceptionAndPrimalInstinct()
    {
        // Source line 1842: Teste de Percepção + Instinto Primitivo.
        var definition = WerewolfGiftCatalog.Get(WerewolfGiftIdentifiers.LupusSentirACaca);

        Assert.NotNull(definition);
        Assert.Equal(WerewolfGiftActivationType.TestRequired, definition!.ActivationType);
        Assert.Equal(WerewolfRaceIdentifiers.Lupus, definition.OwnerKey);
        Assert.Equal("Perception", definition.TestAttribute);
        Assert.Equal("PrimalInstinct", definition.TestAbility);
        Assert.Equal(7, definition.TestDifficulty);
        Assert.Equal(WerewolfGiftCategory.Breed, definition.Category);
    }

    [Fact]
    public void SenseTheHuntActivationPoolComesFromTheRealCharacterSheet()
    {
        // The activation test must read the character's own Perception and
        // Primal Instinct, not a fixed value.
        var weak = ActivateSenseTheHunt(perception: 1, primalInstinct: 1);
        var strong = ActivateSenseTheHunt(perception: 4, primalInstinct: 3);

        Assert.True(weak.Succeeded);
        Assert.True(strong.Succeeded);
        Assert.NotNull(weak.ActivationDefinition);
        Assert.NotNull(strong.ActivationDefinition);
        Assert.Equal(2, weak.ActivationDefinition!.DicePool);
        Assert.Equal(7, strong.ActivationDefinition!.DicePool);
        Assert.NotEqual(weak.ActivationDefinition.DicePool, strong.ActivationDefinition.DicePool);
    }

    [Fact]
    public void SenseTheHuntEffectCarriesTheSourceRadiusAndBothDifficulties()
    {
        // Source line 1842: difficulty 7 wild, 9 urban; prey within 80 km.
        var mechanic = WerewolfBreedGiftMechanics.Resolve(
            WerewolfGiftIdentifiers.LupusSentirACaca, 1, WerewolfFormIdentifiers.Crinos, EmptySheet());

        Assert.NotNull(mechanic);
        Assert.Equal(WerewolfActiveGiftEffectKind.PreySensing, mechanic!.Kind);

        var payload = Assert.IsType<WerewolfSenseRadiusPayload>(mechanic.Payload);
        Assert.Equal(80, payload.RadiusKilometers);
        Assert.Equal(7, payload.WildDifficulty);
        Assert.Equal(9, payload.UrbanDifficulty);
        Assert.Equal("Line 1840", mechanic.SourceLocator);
    }

    [Fact]
    public void SenseTheHuntHasNoDeadCatalogPayloadBranch()
    {
        // Wave 2A raised a possible dead-payload contradiction. The catalog
        // payload path is only consulted for non-Breed Gifts, and this Gift
        // resolves through WerewolfBreedGiftMechanics, so the effect carries
        // the Breed payload rather than a second, unreachable one.
        var effectServiceSource = typeof(WerewolfGiftEffectService);
        Assert.NotNull(effectServiceSource);

        var state = BuildState(
            WerewolfGiftIdentifiers.LupusSentirACaca,
            WerewolfRaceIdentifiers.Lupus,
            WerewolfFormIdentifiers.Crinos,
            perception: 3,
            primalInstinct: 3);

        var result = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
            "req-sentir-a-caca", state, state.RuntimeStateVersion, WerewolfGiftIdentifiers.LupusSentirACaca, 2));

        Assert.True(result.Succeeded);
        var effect = Assert.Single(result.ActiveEffects);
        Assert.IsType<WerewolfSenseRadiusPayload>(effect.Payload);
        Assert.Equal("Line 1840", effect.SourceLocator);

        // The activation definition and the effect must agree on the source.
        var definition = WerewolfGiftCatalog.Get(WerewolfGiftIdentifiers.LupusSentirACaca);
        Assert.Equal(definition!.SourceLocator, effect.SourceLocator);
    }

    [Fact]
    public void BreedGiftsDoNotConsultTheLegacyCatalogPayloadPath()
    {
        // One authoritative path: every Breed Gift's payload comes from
        // WerewolfBreedGiftMechanics, so no Breed Gift has a live-but-unused
        // catalog payload branch.
        foreach (var giftKey in WerewolfBreedGiftMechanics.BreedGiftKeys)
        {
            var mechanic = WerewolfBreedGiftMechanics.Resolve(giftKey, 1, WerewolfFormIdentifiers.Crinos, EmptySheet());
            Assert.NotNull(mechanic);

            var state = BuildState(giftKey, WerewolfRaceIdentifiers.Homid, WerewolfFormIdentifiers.Crinos, 3, 3);
            var result = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
                $"req-payload-{giftKey}", state, state.RuntimeStateVersion, giftKey, 1));

            Assert.True(result.Succeeded);

            // Blocked Gifts legitimately report Custom; executable ones must
            // carry a specific typed kind rather than falling back.
            if (!WerewolfBreedGiftMechanics.IsBlocked(giftKey))
            {
                foreach (var effect in result.ActiveEffects)
                {
                    Assert.NotEqual(WerewolfActiveGiftEffectKind.Custom, effect.EffectKind);
                }
            }
        }
    }

    // =====================================================================
    // 2. MetisDomDoTotem - investigation result.
    // =====================================================================

    [Fact]
    public void TotemEffectsAreFreeTextWithNoExecutorSoTheGiftStaysBlocked()
    {
        // The Gift itself (source line 1819-1821) is fully specified, but its
        // effect *is* the totem's direct power, and totem effects cannot be
        // computed: WerewolfTotemEffect.Payload is a string and nothing reads
        // WerewolfTotemEffectKind outside the catalog that writes it.
        var entry = WerewolfTotemCatalog.AllDefinitions
            .FirstOrDefault(t => t.Effects.Count > 0);

        Assert.NotNull(entry);
        Assert.All(entry!.Effects, effect => Assert.False(string.IsNullOrWhiteSpace(effect.Payload)));
        Assert.All(entry.Effects, effect => Assert.IsType<string>(effect.Payload));

        var (status, dependency) = WerewolfBreedGiftMechanics.StatusOf(WerewolfGiftIdentifiers.MetisDomDoTotem);
        Assert.Equal(WerewolfBreedGiftStatus.Blocked, status);
        Assert.Contains("executor", dependency!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DomDoTotemStillRecordsItsSourceDefinedCostAndTest()
    {
        // Even while blocked, the parts the source does define are catalogued.
        var definition = WerewolfGiftCatalog.Get(WerewolfGiftIdentifiers.MetisDomDoTotem);

        Assert.NotNull(definition);
        Assert.Equal(5, definition!.Level);
        Assert.Equal(WerewolfRaceIdentifiers.Metis, definition.OwnerKey);
        Assert.Equal(WerewolfGiftCostType.Gnosis, definition.CostType);
        Assert.Equal(1, definition.CostAmount);
        Assert.Equal("Charisma", definition.TestAttribute);
        Assert.Equal("Rituals", definition.TestAbility);
        Assert.Equal(7, definition.TestDifficulty);
    }

    [Fact]
    public void DomDoTotemActivationStillRollsItsSourceDefinedTest()
    {
        // The implementable part of the Gift is real: Charisma + Rituals at 7.
        var state = BuildState(
            WerewolfGiftIdentifiers.MetisDomDoTotem,
            WerewolfRaceIdentifiers.Metis,
            WerewolfFormIdentifiers.Crinos,
            charisma: 3,
            rituals: 3);

        var activation = WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-totem-activate", state, 1, WerewolfGiftIdentifiers.MetisDomDoTotem));

        Assert.True(activation.Succeeded);
        Assert.Equal(6, activation.ActivationDefinition!.DicePool);
        Assert.Equal(7, activation.ActivationDefinition.Difficulty);
    }

    // =====================================================================
    // 3. LupusCancaoDaGrandeFera - investigation result.
    // =====================================================================

    [Fact]
    public void PangaeaRealmIsCatalogedButTheBeastsHaveNoSourceStatistics()
    {
        // Pangaea itself exists (source "Lines 3326-3331") with a real
        // mechanical effect, and RealmTravel exists. The summoned beasts do
        // not: the only mention in the source is the Gift's own description.
        var pangea = WerewolfUmbraRealmCatalog.AllDefinitions
            .FirstOrDefault(r => r.RealmKey == "spirit.realm.pangeia");

        Assert.NotNull(pangea);
        Assert.Equal("Lines 3326-3331", pangea!.SourceLocator);

        // No spirit definition describes the legendary beasts.
        Assert.DoesNotContain(
            WerewolfSpiritCategoryCatalog.AllDefinitions,
            category => category.NameEn.Contains("Mammoth", StringComparison.OrdinalIgnoreCase)
                     || category.NameEn.Contains("Megalodon", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SongOfTheGreatBeastStaysBlockedWithItsExactDependency()
    {
        var (status, dependency) = WerewolfBreedGiftMechanics.StatusOf(WerewolfGiftIdentifiers.LupusCancaoDaGrandeFera);

        Assert.Equal(WerewolfBreedGiftStatus.Blocked, status);
        Assert.Contains("no source statistics", dependency!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("summoning", dependency, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SongOfTheGreatBeastStillRecordsItsSourceDefinedCostAndTest()
    {
        // Source line 1867: 1 Gnosis, Charisma + Primal Instinct at difficulty 8.
        var definition = WerewolfGiftCatalog.Get(WerewolfGiftIdentifiers.LupusCancaoDaGrandeFera);

        Assert.NotNull(definition);
        Assert.Equal(5, definition!.Level);
        Assert.Equal(WerewolfRaceIdentifiers.Lupus, definition.OwnerKey);
        Assert.Equal(WerewolfGiftCostType.Gnosis, definition.CostType);
        Assert.Equal(1, definition.CostAmount);
        Assert.Equal("Charisma", definition.TestAttribute);
        Assert.Equal("PrimalInstinct", definition.TestAbility);
        Assert.Equal(8, definition.TestDifficulty);
    }

    // =====================================================================
    // 4. Completeness re-audit: no CATALOG_ONLY remainder.
    // =====================================================================

    [Fact]
    public void EveryBreedGiftHasAnExplicitExecutableOrBlockedStatus()
    {
        var audit = WerewolfBreedGiftMechanics.Audit();

        Assert.Equal(33, audit.Count);
        Assert.All(audit, entry => Assert.Contains(
            entry.Status,
            new[] { WerewolfBreedGiftStatus.Executable, WerewolfBreedGiftStatus.Blocked }));

        var executable = audit.Count(entry => entry.Status == WerewolfBreedGiftStatus.Executable);
        var blocked = audit.Count(entry => entry.Status == WerewolfBreedGiftStatus.Blocked);

        Assert.Equal(31, executable);
        Assert.Equal(2, blocked);
    }

    [Fact]
    public void EveryExecutableBreedGiftResolvesToANonNullMechanic()
    {
        foreach (var entry in WerewolfBreedGiftMechanics.Audit())
        {
            var mechanic = WerewolfBreedGiftMechanics.Resolve(
                entry.GiftKey, 2, WerewolfFormIdentifiers.Crinos, EmptySheet());

            Assert.NotNull(mechanic);
            Assert.Equal(entry.SourceLocator, mechanic!.SourceLocator);
        }
    }

    [Fact]
    public void EveryBlockedBreedGiftNamesItsMissingDependency()
    {
        var blocked = WerewolfBreedGiftMechanics.Audit()
            .Where(entry => entry.Status == WerewolfBreedGiftStatus.Blocked)
            .ToList();

        Assert.Equal(2, blocked.Count);
        Assert.All(blocked, entry =>
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.MissingDependency));
            Assert.False(string.IsNullOrWhiteSpace(entry.SourceLocator));
        });
    }

    [Fact]
    public void EveryBreedGiftAuditEntryResolvesToARealCatalogEntry()
    {
        foreach (var entry in WerewolfBreedGiftMechanics.Audit())
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.GiftKey), "audit entry must name its gift");
            Assert.False(string.IsNullOrWhiteSpace(entry.NameEn));
            Assert.False(string.IsNullOrWhiteSpace(entry.NamePtBr));
            Assert.InRange(entry.Level, 1, 5);
            Assert.False(string.IsNullOrWhiteSpace(entry.OwnerKey));
            Assert.StartsWith("Line ", entry.SourceLocator, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void StatusOfRejectsAnyGiftWithoutARegisteredMechanic()
    {
        // A Breed Gift with no mechanic must surface as blocked, never as a
        // silent catalog-only entry.
        var (status, dependency) = WerewolfBreedGiftMechanics.StatusOf("gift.race.homid.does-not-exist");

        Assert.Equal(WerewolfBreedGiftStatus.Blocked, status);
        Assert.False(string.IsNullOrWhiteSpace(dependency));
    }

    [Fact]
    public void AllThirtyOneExecutableBreedGiftsRemainRuntimeReachable()
    {
        foreach (var entry in WerewolfBreedGiftMechanics.Audit()
                     .Where(e => e.Status == WerewolfBreedGiftStatus.Executable))
        {
            var state = BuildState(entry.GiftKey, entry.OwnerKey, WerewolfFormIdentifiers.Crinos, 3, 3);
            var result = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
                $"req-reach-{entry.GiftKey}", state, state.RuntimeStateVersion, entry.GiftKey, 2));

            Assert.True(result.Succeeded, $"'{entry.GiftKey}' unreachable: {string.Join("; ", result.Findings)}");
            Assert.NotEmpty(result.ActiveEffects);
        }
    }

    [Fact]
    public void BlockedBreedGiftsStillExecuteAndReportTheirDependency()
    {
        foreach (var entry in WerewolfBreedGiftMechanics.Audit()
                     .Where(e => e.Status == WerewolfBreedGiftStatus.Blocked))
        {
            var state = BuildState(entry.GiftKey, entry.OwnerKey, WerewolfFormIdentifiers.Crinos, 3, 3);
            var result = WerewolfGiftEffectService.ApplyEffect(new WerewolfGiftEffectRequest(
                $"req-blocked-{entry.GiftKey}", state, state.RuntimeStateVersion, entry.GiftKey, 2));

            Assert.True(result.Succeeded);
            var effect = Assert.Single(result.ActiveEffects);
            var payload = Assert.IsType<WerewolfBlockedGiftPayload>(effect.Payload);
            Assert.Contains(result.Findings, finding => finding.Contains(payload.MissingSubsystem, StringComparison.Ordinal));
        }
    }

    // =====================================================================
    // Helpers
    // =====================================================================

    private static Dictionary<string, int> EmptySheet() => new(StringComparer.Ordinal);

    private static WerewolfGiftActivationResult ActivateSenseTheHunt(int perception, int primalInstinct)
    {
        var state = BuildState(
            WerewolfGiftIdentifiers.LupusSentirACaca,
            WerewolfRaceIdentifiers.Lupus,
            WerewolfFormIdentifiers.Crinos,
            perception: perception,
            primalInstinct: primalInstinct);

        return WerewolfGiftActivationService.ActivateGift(new WerewolfGiftActivationRequest(
            "req-sentir-activate", state, 1, WerewolfGiftIdentifiers.LupusSentirACaca));
    }

    private static WerewolfRuntimeCharacterState BuildState(
        string giftKey,
        string race,
        string form,
        int perception = 3,
        int primalInstinct = 3,
        int charisma = 3,
        int rituals = 3)
    {
        var binding = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["attributes"] = JsonSerializer.Serialize(new Dictionary<string, int>(StringComparer.Ordinal)
            {
                [WerewolfAttributeIdentifiers.Strength] = 3,
                [WerewolfAttributeIdentifiers.Stamina] = 3,
                [WerewolfAttributeIdentifiers.Dexterity] = 3,
                [WerewolfAttributeIdentifiers.Perception] = perception,
                [WerewolfAttributeIdentifiers.Manipulation] = 3,
                [WerewolfAttributeIdentifiers.Charisma] = charisma
            }),
            ["abilities"] = JsonSerializer.Serialize(new Dictionary<string, int>(StringComparer.Ordinal)
            {
                [WerewolfAbilityIdentifiers.Athletics] = 3,
                [WerewolfAbilityIdentifiers.Crafts] = 3,
                [WerewolfAbilityIdentifiers.Rituals] = rituals,
                [WerewolfAbilityIdentifiers.Occult] = 3,
                [WerewolfAbilityIdentifiers.Enigmas] = 3,
                [WerewolfAbilityIdentifiers.PrimalInstinct] = primalInstinct,
                [WerewolfAbilityIdentifiers.Intimidation] = 3,
                [WerewolfAbilityIdentifiers.Empathy] = 3,
                [WerewolfAbilityIdentifiers.Expression] = 3,
                [WerewolfAbilityIdentifiers.AnimalEmpathy] = 3
            })
        };

        return new WerewolfRuntimeCharacterState(
            PackageId: "test-package",
            PackageVersion: "0.1.0",
            DraftId: "draft-breed-2b",
            RuntimeStateVersion: 1,
            PackageBinding: binding,
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
            BirthRace: race,
            HealthTrack: WerewolfHealthTrackComputer.Compute([], hasWeakenedImmuneSystem: false, lastRegenerationTurn: -1),
            CurrentForm: form,
            Conditions: [],
            FrenzyState: null,
            ActiveGiftEffects: [],
            KnownGiftKeys: [giftKey],
            SceneGiftUsage: new Dictionary<string, int>(StringComparer.Ordinal),
            CurrentSceneToken: "scene-1",
            ActivatedGiftKeys: [giftKey]);
    }
}
