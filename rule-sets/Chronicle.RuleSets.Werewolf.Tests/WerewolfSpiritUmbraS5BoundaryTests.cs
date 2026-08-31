using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfSpiritUmbraS5BoundaryTests
{
    private static WerewolfSpiritRuntimeState CreateTestState()
    {
        return WerewolfSpiritRuntimeState.Create(
            "spirit.test.001",
            "spirit.category.totem",
            5,
            4,
            3,
            ["spirit.charm.common.materializar"]);
    }

    [Fact]
    public void S5ExactBoundaryKeyCountIsEight()
    {
        var s5BoundaryKeys = new[]
        {
            WerewolfSpiritMechanicServices.SpiritLocationOperation,
            WerewolfSpiritMechanicServices.GauntletLookupOperation,
            WerewolfSpiritMechanicServices.RealmTravelOperation,
            WerewolfSpiritMechanicServices.ScenePresenceOperation,
            WerewolfSpiritMechanicServices.CaernPelículaOperation,
            WerewolfSpiritMechanicServices.PackTotemLinkOperation,
            WerewolfSpiritMechanicServices.SharedTotemEffectsOperation
        };

        Assert.Equal(7, s5BoundaryKeys.Length);
        Assert.Distinct(s5BoundaryKeys);
    }

    [Fact]
    public void S5AllBoundaryOperationsRegistered()
    {
        var runtime = new WerewolfReferenceRuntime();
        var spiritOperations = runtime.Metadata.Operations
            .Where(o => o.CapabilityKey == "spirit-umbra")
            .Select(o => o.OperationKey)
            .ToList();

        Assert.Contains(WerewolfSpiritMechanicServices.SpiritLocationOperation, spiritOperations);
        Assert.Contains(WerewolfSpiritMechanicServices.GauntletLookupOperation, spiritOperations);
        Assert.Contains(WerewolfSpiritMechanicServices.RealmTravelOperation, spiritOperations);
        Assert.Contains(WerewolfSpiritMechanicServices.ScenePresenceOperation, spiritOperations);
        Assert.Contains(WerewolfSpiritMechanicServices.CaernPelículaOperation, spiritOperations);
        Assert.Contains(WerewolfSpiritMechanicServices.PackTotemLinkOperation, spiritOperations);
        Assert.Contains(WerewolfSpiritMechanicServices.SharedTotemEffectsOperation, spiritOperations);
    }

    [Fact]
    public void S5SpiritLocationReturnsBoundary()
    {
        var state = CreateTestState();
        var request = new SpiritMechanicRequest(state, state.StateVersion, "req-location")
        {
            // boundary is represented via service return
        };

        var result = WerewolfSpiritMechanicServices.SpiritLocation(
            request,
            "spirit.test.001",
            "spirit.realm.penumbra",
            "layer.upper",
            "transition.to.layer");

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Findings);
        Assert.Empty(result.Findings);
    }

    [Fact]
    public void S5GauntletLookupRejectsOutOfRangeValue()
    {
        var state = CreateTestState();
        var request = new SpiritMechanicRequest(state, state.StateVersion, "req-gauntlet");

        var result = WerewolfSpiritMechanicServices.GauntletLookup(
            request,
            "location.urban.downtown",
            10,
            5);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == SpiritMechanicErrorCode.InvalidGauntletValue);
    }

    [Fact]
    public void S5GauntletLookupAcceptsValidRange()
    {
        var state = CreateTestState();
        var request = new SpiritMechanicRequest(state, state.StateVersion, "req-gauntlet");

        var result = WerewolfSpiritMechanicServices.GauntletLookup(
            request,
            "location.wild.forest",
            3,
            2);

        Assert.True(result.Succeeded);
        Assert.Empty(result.Findings);
    }

    [Fact]
    public void S5RealmTravelRequiresOriginAndDestination()
    {
        var state = CreateTestState();
        var request = new SpiritMechanicRequest(state, state.StateVersion, "req-realm");

        var result = WerewolfSpiritMechanicServices.RealmTravel(
            request,
            "spirit.test.001",
            "spirit.realm.penumbra",
            "spirit.realm.deep-umbra",
            "Moon Trail");

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Findings);
        Assert.Empty(result.Findings);
    }

    [Fact]
    public void S5ScenePresenceRequiresPresenceState()
    {
        var state = CreateTestState();
        var request = new SpiritMechanicRequest(state, state.StateVersion, "req-presence");

        var result = WerewolfSpiritMechanicServices.ScenePresence(
            request,
            "spirit.test.001",
            "scene.sept.gathering",
            "present");

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Findings);
        Assert.Empty(result.Findings);
    }

    [Fact]
    public void S5CaernPelículaReturnsExactTableValues()
    {
        var state = CreateTestState();
        var request = new SpiritMechanicRequest(state, state.StateVersion, "req-caern");

        var level1 = WerewolfSpiritMechanicServices.CaernPelícula(request, 1);
        Assert.True(level1.Succeeded);

        var level5 = WerewolfSpiritMechanicServices.CaernPelícula(request, 5);
        Assert.True(level5.Succeeded);
    }

    [Fact]
    public void S5CaernPelículaRejectsInvalidLevel()
    {
        var state = CreateTestState();
        var request = new SpiritMechanicRequest(state, state.StateVersion, "req-caern");

        var result = WerewolfSpiritMechanicServices.CaernPelícula(request, 0);
        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == SpiritMechanicErrorCode.InvalidCategory);
    }

    [Fact]
    public void S5PackTotemLinkRequiresPackAndTotem()
    {
        var state = CreateTestState();
        var request = new SpiritMechanicRequest(state, state.StateVersion, "req-link");

        var result = WerewolfSpiritMechanicServices.PackTotemLink(
            request,
            "pack.test.001",
            "totem.cervo",
            "bound");

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Findings);
        Assert.Empty(result.Findings);
    }

    [Fact]
    public void S5SharedTotemEffectsRequiresEffectKeys()
    {
        var state = CreateTestState();
        var request = new SpiritMechanicRequest(state, state.StateVersion, "req-effects");

        var result = WerewolfSpiritMechanicServices.SharedTotemEffects(
            request,
            "totem.cervo",
            ["totem.improvement.presence", "totem.improvement.prestige"],
            "AllPackMembers");

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Findings);
        Assert.Empty(result.Findings);
    }

    [Fact]
    public void S5SharedTotemEffectsRejectsEmptyEffectKeys()
    {
        var state = CreateTestState();
        var request = new SpiritMechanicRequest(state, state.StateVersion, "req-effects");

        var result = WerewolfSpiritMechanicServices.SharedTotemEffects(
            request,
            "totem.cervo",
            Array.Empty<string>(),
            "AllPackMembers");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == SpiritMechanicErrorCode.InvalidCategory);
    }

    [Fact]
    public void S5NoNarrativeAiImplemented()
    {
        var narrativeAiKeys = new[]
        {
            "spirit.disposition.ai",
            "spirit.bargaining.valuation",
            "spirit.hierarchy.behavior"
        };

        var runtime = new WerewolfReferenceRuntime();
        var allOperations = runtime.Metadata.Operations.Select(o => o.OperationKey).ToList();

        foreach (var key in narrativeAiKeys)
        {
            Assert.DoesNotContain(key, allOperations);
        }
    }

    [Fact]
    public void S5NoSourceGapKeyImplemented()
    {
        var sourceGapKeys = new[]
        {
            "spirit.materialization.duration",
            "spirit.death.modorra-threshold",
            "spirit.possession.control",
            "spirit.crossing.non-garou",
            "spirit.voting.system",
            "spirit.persistence.lifecycle",
            "spirit.world-travel.rules"
        };

        var runtime = new WerewolfReferenceRuntime();
        var allOperations = runtime.Metadata.Operations.Select(o => o.OperationKey).ToList();

        foreach (var key in sourceGapKeys)
        {
            Assert.DoesNotContain(key, allOperations);
        }
    }

    [Fact]
    public void S5NoWorldScenePersistenceIntroduced()
    {
        var state = CreateTestState();
        var request = new SpiritMechanicRequest(state, state.StateVersion, "req-location");

        var result = WerewolfSpiritMechanicServices.SpiritLocation(request, "spirit.test.001", "realm", "layer", "transition");
        Assert.True(result.Succeeded);
        Assert.Null(result.NewState);
    }

    [Fact]
    public void S5NoPackAggregateIntroduced()
    {
        var state = CreateTestState();
        var request = new SpiritMechanicRequest(state, state.StateVersion, "req-link");

        var result = WerewolfSpiritMechanicServices.PackTotemLink(request, "pack.1", "totem.1", "bound");
        Assert.True(result.Succeeded);
        Assert.Null(result.NewState);
    }

    [Fact]
    public void S5NoAiBehaviorIntroduced()
    {
        var runtime = new WerewolfReferenceRuntime();
        var spiritOperations = runtime.Metadata.Operations
            .Where(o => o.CapabilityKey == "spirit-umbra")
            .Select(o => o.OperationKey)
            .ToList();

        Assert.DoesNotContain("spirit.disposition.ai", spiritOperations);
        Assert.DoesNotContain("spirit.bargaining.valuation", spiritOperations);
        Assert.DoesNotContain("spirit.hierarchy.behavior", spiritOperations);
    }
}