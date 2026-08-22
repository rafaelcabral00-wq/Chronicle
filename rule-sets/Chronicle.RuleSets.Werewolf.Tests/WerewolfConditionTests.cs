using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfConditionTests
{
    private static WerewolfRuntimeCharacterState CreateState(int version = 1)
    {
        return new WerewolfRuntimeCharacterState(
            "test-package",
            "0.1.0",
            "draft-001",
            version,
            new Dictionary<string, string>(StringComparer.Ordinal),
            5,
            5,
            3,
            3,
            7,
            7,
            0,
            0,
            0,
            0,
            0,
            0,
            WerewolfRaceIdentifiers.Metis,
            null,
            WerewolfFormIdentifiers.Crinos,
            []);
    }

    [Fact]
    public void ApplyConditionIncrementsVersion()
    {
        var state = CreateState();
        var request = new WerewolfApplyConditionRequest(
            "req-1",
            state,
            1,
            WerewolfConditionIdentifiers.TemporaryPsychoticEpisode,
            WerewolfConditionKind.TemporaryPsychoticEpisode,
            "Line 527",
            WerewolfMetisDeformityIdentifiers.FitsOfMadness);

        var result = WerewolfConditionService.ApplyCondition(request);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.NewRuntimeStateVersion);
        Assert.Single(result.NewState!.Conditions!);
        Assert.True(result.NewState!.Conditions![0].IsActive);
    }

    [Fact]
    public void ApplyConditionPreservesUnrelatedFields()
    {
        var state = CreateState();
        var request = new WerewolfApplyConditionRequest(
            "req-1",
            state,
            1,
            WerewolfConditionIdentifiers.TemporaryPsychoticEpisode,
            WerewolfConditionKind.TemporaryPsychoticEpisode,
            "Line 527",
            WerewolfMetisDeformityIdentifiers.FitsOfMadness);

        var result = WerewolfConditionService.ApplyCondition(request);

        Assert.Equal(state.RagePermanent, result.NewState!.RagePermanent);
        Assert.Equal(state.RageCurrent, result.NewState!.RageCurrent);
        Assert.Equal(state.BirthRace, result.NewState!.BirthRace);
        Assert.Equal(state.CurrentForm, result.NewState!.CurrentForm);
        Assert.Equal(state.PackageBinding, result.NewState!.PackageBinding);
    }

    [Fact]
    public void ApplyConditionRejectsVersionMismatch()
    {
        var state = CreateState(version: 2);
        var request = new WerewolfApplyConditionRequest(
            "req-1",
            state,
            1,
            WerewolfConditionIdentifiers.TemporaryPsychoticEpisode,
            WerewolfConditionKind.TemporaryPsychoticEpisode,
            "Line 527",
            WerewolfMetisDeformityIdentifiers.FitsOfMadness);

        var result = WerewolfConditionService.ApplyCondition(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.NewState);
    }

    [Fact]
    public void ApplyConditionDoesNotDuplicateActiveCondition()
    {
        var state = CreateState();
        var request = new WerewolfApplyConditionRequest(
            "req-1",
            state,
            1,
            WerewolfConditionIdentifiers.TemporaryPsychoticEpisode,
            WerewolfConditionKind.TemporaryPsychoticEpisode,
            "Line 527",
            WerewolfMetisDeformityIdentifiers.FitsOfMadness);

        var first = WerewolfConditionService.ApplyCondition(request);

        var secondRequest = request with
        {
            RequestId = "req-2",
            CurrentState = first.NewState!,
            ExpectedRuntimeStateVersion = first.NewRuntimeStateVersion
        };
        var second = WerewolfConditionService.ApplyCondition(secondRequest);

        Assert.True(second.Succeeded);
        Assert.Single(second.NewState!.Conditions!);
    }

    [Fact]
    public void ClearConditionDeactivatesAndIncrementsVersion()
    {
        var state = CreateState();
        var applyRequest = new WerewolfApplyConditionRequest(
            "req-1",
            state,
            1,
            WerewolfConditionIdentifiers.TemporaryPsychoticEpisode,
            WerewolfConditionKind.TemporaryPsychoticEpisode,
            "Line 527",
            WerewolfMetisDeformityIdentifiers.FitsOfMadness);

        var applied = WerewolfConditionService.ApplyCondition(applyRequest);

        var clearRequest = new WerewolfClearConditionRequest(
            "req-2",
            applied.NewState!,
            applied.NewRuntimeStateVersion,
            WerewolfConditionIdentifiers.TemporaryPsychoticEpisode);

        var cleared = WerewolfConditionService.ClearCondition(clearRequest);

        Assert.True(cleared.Succeeded);
        Assert.Equal(3, cleared.NewRuntimeStateVersion);
        Assert.Single(cleared.NewState!.Conditions!);
        Assert.False(cleared.NewState!.Conditions![0].IsActive);
    }

    [Fact]
    public void ClearConditionHandlesNonExistentCondition()
    {
        var state = CreateState();
        var clearRequest = new WerewolfClearConditionRequest(
            "req-1",
            state,
            1,
            "nonexistent-condition");

        var result = WerewolfConditionService.ClearCondition(clearRequest);

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.NewRuntimeStateVersion);
    }

    [Fact]
    public void ClearConditionRejectsVersionMismatch()
    {
        var state = CreateState(version: 3);
        var clearRequest = new WerewolfClearConditionRequest(
            "req-1",
            state,
            1,
            WerewolfConditionIdentifiers.TemporaryPsychoticEpisode);

        var result = WerewolfConditionService.ClearCondition(clearRequest);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void EvaluateActionAvailabilityReturnsAvailableForHealthyState()
    {
        var state = CreateState();
        var request = new WerewolfEvaluateActionAvailabilityRequest(
            "req-1",
            state,
            1,
            "any-action");

        var result = WerewolfConditionService.EvaluateActionAvailability(request);

        Assert.True(result.Succeeded);
        Assert.True(result.IsAvailable);
        Assert.Null(result.UnavailableReason);
    }

    [Fact]
    public void EvaluateActionAvailabilityBlocksWhenIncapacitated()
    {
        var state = CreateState();
        var applyRequest = new WerewolfApplyConditionRequest(
            "req-1",
            state,
            1,
            WerewolfConditionIdentifiers.Incapacitated,
            WerewolfConditionKind.Incapacitated,
            "Line 531",
            WerewolfMetisDeformityIdentifiers.Seizures);

        var applied = WerewolfConditionService.ApplyCondition(applyRequest);

        var availabilityRequest = new WerewolfEvaluateActionAvailabilityRequest(
            "req-2",
            applied.NewState!,
            applied.NewRuntimeStateVersion,
            "any-action");

        var result = WerewolfConditionService.EvaluateActionAvailability(availabilityRequest);

        Assert.True(result.Succeeded);
        Assert.False(result.IsAvailable);
        Assert.Equal("Incapacitated", result.UnavailableReason);
    }

    [Fact]
    public void EvaluateActionAvailabilityBlocksWhenPsychoticEpisode()
    {
        var state = CreateState();
        var applyRequest = new WerewolfApplyConditionRequest(
            "req-1",
            state,
            1,
            WerewolfConditionIdentifiers.TemporaryPsychoticEpisode,
            WerewolfConditionKind.TemporaryPsychoticEpisode,
            "Line 527",
            WerewolfMetisDeformityIdentifiers.FitsOfMadness);

        var applied = WerewolfConditionService.ApplyCondition(applyRequest);

        var availabilityRequest = new WerewolfEvaluateActionAvailabilityRequest(
            "req-2",
            applied.NewState!,
            applied.NewRuntimeStateVersion,
            "any-action");

        var result = WerewolfConditionService.EvaluateActionAvailability(availabilityRequest);

        Assert.True(result.Succeeded);
        Assert.False(result.IsAvailable);
        Assert.Equal("Temporary psychotic episode", result.UnavailableReason);
    }

    [Fact]
    public void EvaluateActionAvailabilityRejectsVersionMismatch()
    {
        var state = CreateState(version: 5);
        var request = new WerewolfEvaluateActionAvailabilityRequest(
            "req-1",
            state,
            1,
            "any-action");

        var result = WerewolfConditionService.EvaluateActionAvailability(request);

        Assert.False(result.Succeeded);
        Assert.False(result.IsAvailable);
    }

    [Fact]
    public void StateImmutabilityOriginalStateUnchangedAfterApply()
    {
        var state = CreateState();
        var originalConditions = state.Conditions;

        var request = new WerewolfApplyConditionRequest(
            "req-1",
            state,
            1,
            WerewolfConditionIdentifiers.TemporaryPsychoticEpisode,
            WerewolfConditionKind.TemporaryPsychoticEpisode,
            "Line 527",
            WerewolfMetisDeformityIdentifiers.FitsOfMadness);

        var result = WerewolfConditionService.ApplyCondition(request);

        Assert.Equal(originalConditions, state.Conditions);
        Assert.NotSame(state, result.NewState);
    }

    [Fact]
    public void ConditionRecordsSourceOrigin()
    {
        var state = CreateState();
        var request = new WerewolfApplyConditionRequest(
            "req-1",
            state,
            1,
            WerewolfConditionIdentifiers.TemporaryPsychoticEpisode,
            WerewolfConditionKind.TemporaryPsychoticEpisode,
            "Line 527",
            WerewolfMetisDeformityIdentifiers.FitsOfMadness);

        var result = WerewolfConditionService.ApplyCondition(request);

        var condition = result.NewState!.Conditions![0];
        Assert.Equal("Line 527", condition.SourceLocator);
        Assert.Equal(WerewolfMetisDeformityIdentifiers.FitsOfMadness, condition.SourceDeformity);
        Assert.Equal(WerewolfConditionKind.TemporaryPsychoticEpisode, condition.Kind);
    }
}
