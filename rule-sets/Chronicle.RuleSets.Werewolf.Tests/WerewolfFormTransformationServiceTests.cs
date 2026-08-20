using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfFormTransformationServiceTests
{
    private static WerewolfRuntimeCharacterState CreateState(string currentForm, int rageCurrent = 1, string birthRace = "homid")
    {
        return new WerewolfRuntimeCharacterState(
            "pkg", "1.0.0", "draft-1", 1,
            new Dictionary<string, string>(StringComparer.Ordinal),
            1, rageCurrent,
            1, 1,
            3, 3,
            0, 0,
            0, 0,
            0, 0,
            BirthRace: birthRace,
            HealthTrack: WerewolfHealthTrackComputer.Compute([], hasWeakenedImmuneSystem: false, lastRegenerationTurn: -1),
            CurrentForm: currentForm);
    }

    [Fact]
    public void TransformToInvalidFormReturnsError()
    {
        var state = CreateState(WerewolfFormIdentifiers.Homid);
        var result = WerewolfFormTransformationService.Transform(new WerewolfFormTransformationRequest(
            "req-1", state, 1, "invalid-form", SpendRage: false));

        Assert.False(result.Succeeded);
        Assert.Null(result.UpdatedState);
        Assert.Contains(result.Findings, f => f.Code == WerewolfFormTransformationErrorCode.InvalidTargetForm);
    }

    [Fact]
    public void TransformToSameFormReturnsSuccessWithoutChange()
    {
        var state = CreateState(WerewolfFormIdentifiers.Homid);
        var result = WerewolfFormTransformationService.Transform(new WerewolfFormTransformationRequest(
            "req-1", state, 1, WerewolfFormIdentifiers.Homid, SpendRage: false));

        Assert.True(result.Succeeded);
        Assert.Equal(state, result.UpdatedState);
        Assert.Contains(result.Findings, f => f.Code == WerewolfFormTransformationErrorCode.SameForm);
    }

    [Fact]
    public void TransformHomidToHomidIsNativeAndAutomatic()
    {
        var state = CreateState(WerewolfFormIdentifiers.Glabro, birthRace: WerewolfRaceIdentifiers.Homid);
        var result = WerewolfFormTransformationService.Transform(new WerewolfFormTransformationRequest(
            "req-1", state, 1, WerewolfFormIdentifiers.Homid, SpendRage: false));

        Assert.True(result.Succeeded);
        Assert.Equal(WerewolfFormIdentifiers.Homid, result.UpdatedState?.CurrentForm);
        Assert.Contains(result.Findings, f => f.Code == WerewolfFormTransformationErrorCode.TransformationSucceeded && f.Message.Contains("automatic"));
    }

    [Fact]
    public void TransformWithRageSpendDeductsRage()
    {
        var state = CreateState(WerewolfFormIdentifiers.Homid, rageCurrent: 2, birthRace: WerewolfRaceIdentifiers.Homid);
        var result = WerewolfFormTransformationService.Transform(new WerewolfFormTransformationRequest(
            "req-1", state, 1, WerewolfFormIdentifiers.Glabro, SpendRage: true));

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.RemainingRage);
        Assert.Equal(WerewolfFormIdentifiers.Glabro, result.UpdatedState?.CurrentForm);
        Assert.Equal(2, result.NewStateVersion);
    }

    [Fact]
    public void TransformWithInsufficientRageReturnsError()
    {
        var state = CreateState(WerewolfFormIdentifiers.Homid, rageCurrent: 0, birthRace: WerewolfRaceIdentifiers.Homid);
        var result = WerewolfFormTransformationService.Transform(new WerewolfFormTransformationRequest(
            "req-1", state, 1, WerewolfFormIdentifiers.Glabro, SpendRage: true));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == WerewolfFormTransformationErrorCode.InsufficientRage);
    }

    [Fact]
    public void TransformWithoutRageReportsRequiredDifficultyAndSuccesses()
    {
        var state = CreateState(WerewolfFormIdentifiers.Homid, birthRace: WerewolfRaceIdentifiers.Homid);
        var result = WerewolfFormTransformationService.Transform(new WerewolfFormTransformationRequest(
            "req-1", state, 1, WerewolfFormIdentifiers.Crinos, SpendRage: false));

        Assert.True(result.Succeeded);
        Assert.Equal(WerewolfFormIdentifiers.Crinos, result.UpdatedState?.CurrentForm);
        Assert.Contains(result.Findings, f => f.Message.Contains("difficulty") && f.Message.Contains("successes"));
    }

    [Fact]
    public void TransformIncrementsRuntimeStateVersion()
    {
        var state = CreateState(WerewolfFormIdentifiers.Homid);
        var result = WerewolfFormTransformationService.Transform(new WerewolfFormTransformationRequest(
            "req-1", state, 1, WerewolfFormIdentifiers.Glabro, SpendRage: false));

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.NewStateVersion);
    }

    [Fact]
    public void TransformWithStaleStateVersionStillSucceeds()
    {
        var state = CreateState(WerewolfFormIdentifiers.Homid);
        var result = WerewolfFormTransformationService.Transform(new WerewolfFormTransformationRequest(
            "req-1", state, 999, WerewolfFormIdentifiers.Glabro, SpendRage: false));

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void TransformMetisToCrinosIsNativeAndAutomatic()
    {
        var state = CreateState(WerewolfFormIdentifiers.Homid, birthRace: WerewolfRaceIdentifiers.Metis);
        var result = WerewolfFormTransformationService.Transform(new WerewolfFormTransformationRequest(
            "req-1", state, 1, WerewolfFormIdentifiers.Crinos, SpendRage: false));

        Assert.True(result.Succeeded);
        Assert.Equal(WerewolfFormIdentifiers.Crinos, result.UpdatedState?.CurrentForm);
        Assert.Contains(result.Findings, f => f.Message.Contains("automatic"));
    }

    [Fact]
    public void TransformLupusToLupusIsNativeAndAutomatic()
    {
        var state = CreateState(WerewolfFormIdentifiers.Homid, birthRace: WerewolfRaceIdentifiers.Lupus);
        var result = WerewolfFormTransformationService.Transform(new WerewolfFormTransformationRequest(
            "req-1", state, 1, WerewolfFormIdentifiers.Lupus, SpendRage: false));

        Assert.True(result.Succeeded);
        Assert.Equal(WerewolfFormIdentifiers.Lupus, result.UpdatedState?.CurrentForm);
        Assert.Contains(result.Findings, f => f.Message.Contains("automatic"));
    }

    [Fact]
    public void TransformFromNullStateReturnsError()
    {
        var result = WerewolfFormTransformationService.Transform(new WerewolfFormTransformationRequest(
            "req-1", null, 1, WerewolfFormIdentifiers.Homid, SpendRage: false));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == WerewolfFormTransformationErrorCode.MissingState);
    }

    [Fact]
    public void TransformWithEmptyTargetFormReturnsError()
    {
        var state = CreateState(WerewolfFormIdentifiers.Homid);
        var result = WerewolfFormTransformationService.Transform(new WerewolfFormTransformationRequest(
            "req-1", state, 1, "", SpendRage: false));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Findings, f => f.Code == WerewolfFormTransformationErrorCode.InvalidTargetForm);
    }
}
