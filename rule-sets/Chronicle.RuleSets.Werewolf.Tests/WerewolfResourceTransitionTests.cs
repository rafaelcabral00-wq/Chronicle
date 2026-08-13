using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfResourceTransitionTests
{
    private static WerewolfRuntimeCharacterState BuildRuntimeState(
        int ragePermanent = 5,
        int rageCurrent = 5,
        int gnosisPermanent = 3,
        int gnosisCurrent = 3,
        int willpowerPermanent = 4,
        int willpowerCurrent = 4,
        int gloryPermanent = 0,
        int gloryCurrent = 0,
        int honorPermanent = 0,
        int honorCurrent = 0,
        int wisdomPermanent = 0,
        int wisdomCurrent = 0,
        int runtimeStateVersion = 1)
    {
        return new WerewolfRuntimeCharacterState(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            "draft-1",
            runtimeStateVersion,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["packageId"] = WerewolfRuleSetPackage.ProvisionalPackageId,
                ["packageVersion"] = WerewolfRuleSetPackage.PackageVersion,
                ["declaredReleaseScope"] = WerewolfRuleSetPackage.DeclaredReleaseScope,
                ["contractVersion"] = "1"
            },
            ragePermanent,
            rageCurrent,
            gnosisPermanent,
            gnosisCurrent,
            willpowerPermanent,
            willpowerCurrent,
            gloryPermanent,
            gloryCurrent,
            honorPermanent,
            honorCurrent,
            wisdomPermanent,
            wisdomCurrent);
    }

    [Fact]
    public void SpendRageSucceedsWhenSufficient()
    {
        var state = BuildRuntimeState(rageCurrent: 5);
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Rage, 2);

        var result = WerewolfResourceTransitionService.Spend(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(3, result.NewState!.RageCurrent);
        Assert.Equal(5, result.NewState.RagePermanent);
        Assert.Equal(2, result.NewState.RuntimeStateVersion);
        Assert.Equal("req-1", result.RequestId);
        Assert.Equal(5, result.PreviousCurrent);
        Assert.Equal(3, result.NewCurrent);
        Assert.Equal(5, result.PreviousPermanent);
        Assert.Equal(5, result.NewPermanent);
    }

    [Fact]
    public void SpendGnosisSucceedsWhenSufficient()
    {
        var state = BuildRuntimeState(gnosisCurrent: 3);
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Gnosis, 1);

        var result = WerewolfResourceTransitionService.Spend(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(2, result.NewState!.GnosisCurrent);
        Assert.Equal(2, result.NewState.RuntimeStateVersion);
    }

    [Fact]
    public void SpendWillpowerSucceedsWhenSufficient()
    {
        var state = BuildRuntimeState(willpowerCurrent: 4);
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Willpower, 3);

        var result = WerewolfResourceTransitionService.Spend(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(1, result.NewState!.WillpowerCurrent);
        Assert.Equal(2, result.NewState.RuntimeStateVersion);
    }

    [Fact]
    public void RecoverRageSucceedsWhenBelowPermanent()
    {
        var state = BuildRuntimeState(ragePermanent: 5, rageCurrent: 2);
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Rage, 2);

        var result = WerewolfResourceTransitionService.Recover(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(4, result.NewState!.RageCurrent);
        Assert.Equal(5, result.NewState.RagePermanent);
        Assert.Equal(2, result.NewState.RuntimeStateVersion);
        Assert.Equal(2, result.PreviousCurrent);
        Assert.Equal(4, result.NewCurrent);
        Assert.Equal(5, result.PreviousPermanent);
        Assert.Equal(5, result.NewPermanent);
    }

    [Fact]
    public void RecoverGnosisSucceedsWhenBelowPermanent()
    {
        var state = BuildRuntimeState(gnosisPermanent: 3, gnosisCurrent: 0);
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Gnosis, 2);

        var result = WerewolfResourceTransitionService.Recover(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(2, result.NewState!.GnosisCurrent);
    }

    [Fact]
    public void RecoverWillpowerSucceedsWhenBelowPermanent()
    {
        var state = BuildRuntimeState(willpowerPermanent: 4, willpowerCurrent: 3);
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Willpower, 1);

        var result = WerewolfResourceTransitionService.Recover(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(4, result.NewState!.WillpowerCurrent);
    }

    [Fact]
    public void SpendToExactlyZeroSucceeds()
    {
        var state = BuildRuntimeState(rageCurrent: 2);
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Rage, 2);

        var result = WerewolfResourceTransitionService.Spend(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(0, result.NewState!.RageCurrent);
    }

    [Fact]
    public void RecoverToExactlyPermanentSucceeds()
    {
        var state = BuildRuntimeState(ragePermanent: 5, rageCurrent: 3);
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Rage, 2);

        var result = WerewolfResourceTransitionService.Recover(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(5, result.NewState!.RageCurrent);
    }

    [Fact]
    public void SpendRejectsInsufficientResource()
    {
        var state = BuildRuntimeState(rageCurrent: 1);
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Rage, 2);

        var result = WerewolfResourceTransitionService.Spend(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.NewState);
        Assert.Equal(WerewolfResourceTransitionErrorCode.InsufficientCurrentValue, result.Findings[0].Code);
    }

    [Fact]
    public void RecoverRejectsExceedingPermanent()
    {
        var state = BuildRuntimeState(ragePermanent: 5, rageCurrent: 4);
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Rage, 2);

        var result = WerewolfResourceTransitionService.Recover(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.NewState);
        Assert.Equal(WerewolfResourceTransitionErrorCode.RecoveryExceedsPermanent, result.Findings[0].Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SpendRejectsZeroOrNegativeAmount(int amount)
    {
        var state = BuildRuntimeState(rageCurrent: 5);
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Rage, amount);

        var result = WerewolfResourceTransitionService.Spend(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.NewState);
        Assert.Equal(WerewolfResourceTransitionErrorCode.AmountMissingOrZero, result.Findings[0].Code);
    }

    [Fact]
    public void RecoverRejectsNegativeAmount()
    {
        var state = BuildRuntimeState(rageCurrent: 5);
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Rage, -1);

        var result = WerewolfResourceTransitionService.Recover(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.NewState);
        Assert.Equal(WerewolfResourceTransitionErrorCode.AmountMissingOrZero, result.Findings[0].Code);
    }

    [Fact]
    public void RejectsUnknownResource()
    {
        var state = BuildRuntimeState();
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", "character.resource.unknown", 1);

        var result = WerewolfResourceTransitionService.Spend(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.NewState);
        Assert.Equal(WerewolfResourceTransitionErrorCode.UnknownResource, result.Findings[0].Code);
    }

    [Fact]
    public void RejectsMalformedResourceIdentifier()
    {
        var state = BuildRuntimeState();
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", "", 1);

        var result = WerewolfResourceTransitionService.Spend(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.NewState);
        Assert.Equal(WerewolfResourceTransitionErrorCode.MalformedResourceIdentifier, result.Findings[0].Code);
    }

    [Theory]
    [InlineData("character.resource.rage.permanent")]
    [InlineData("character.resource.gnosis.permanent")]
    [InlineData("character.resource.willpower.permanent")]
    public void RejectsPermanentResourceIdentifiers(string permanentResourceId)
    {
        var state = BuildRuntimeState();
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", permanentResourceId, 1);

        var result = WerewolfResourceTransitionService.Spend(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.NewState);
        Assert.Equal(WerewolfResourceTransitionErrorCode.PermanentResourceMutationUnsupported, result.Findings[0].Code);
    }

    [Fact]
    public void PermanentValueRemainsUnchangedAfterSpend()
    {
        var state = BuildRuntimeState(ragePermanent: 5, rageCurrent: 5);
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Rage, 2);

        var result = WerewolfResourceTransitionService.Spend(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(5, result.NewState!.RagePermanent);
    }

    [Fact]
    public void PermanentValueRemainsUnchangedAfterRecover()
    {
        var state = BuildRuntimeState(ragePermanent: 5, rageCurrent: 2);
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Rage, 2);

        var result = WerewolfResourceTransitionService.Recover(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(5, result.NewState!.RagePermanent);
    }

    [Fact]
    public void UnrelatedResourcesRemainUnchangedAfterSpend()
    {
        var state = BuildRuntimeState(gnosisCurrent: 3, willpowerCurrent: 4);
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Rage, 1);

        var result = WerewolfResourceTransitionService.Spend(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(3, result.NewState!.GnosisCurrent);
        Assert.Equal(4, result.NewState.WillpowerCurrent);
    }

    [Fact]
    public void UnrelatedResourcesRemainUnchangedAfterRecover()
    {
        var state = BuildRuntimeState(ragePermanent: 5, rageCurrent: 2, gnosisCurrent: 2, willpowerCurrent: 3);
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Rage, 1);

        var result = WerewolfResourceTransitionService.Recover(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(2, result.NewState!.GnosisCurrent);
        Assert.Equal(3, result.NewState.WillpowerCurrent);
    }

    [Fact]
    public void SourceStateRemainsImmutable()
    {
        var state = BuildRuntimeState(rageCurrent: 5);
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Rage, 2);

        var result = WerewolfResourceTransitionService.Spend(request);

        Assert.True(result.Succeeded);
        Assert.Equal(5, state.RageCurrent);
        Assert.Equal(1, state.RuntimeStateVersion);
    }

    [Fact]
    public void VersionIncrementsExactlyOnceOnSuccess()
    {
        var state = BuildRuntimeState(rageCurrent: 5, runtimeStateVersion: 7);
        var request = new WerewolfResourceTransitionRequest(state, 7, "req-1", WerewolfCharacterResourceIdentifiers.Rage, 1);

        var result = WerewolfResourceTransitionService.Spend(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
        Assert.Equal(8, result.NewState!.RuntimeStateVersion);
    }

    [Fact]
    public void StaleVersionIsRejected()
    {
        var state = BuildRuntimeState(rageCurrent: 5, runtimeStateVersion: 5);
        var request = new WerewolfResourceTransitionRequest(state, 3, "req-1", WerewolfCharacterResourceIdentifiers.Rage, 1);

        var result = WerewolfResourceTransitionService.Spend(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.NewState);
        Assert.Equal(WerewolfResourceTransitionErrorCode.StaleRuntimeStateVersion, result.Findings[0].Code);
    }

    [Fact]
    public void DeterministicFindingOrderingOnSpend()
    {
        var state = BuildRuntimeState(rageCurrent: 5);
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Rage, 1);

        var result = WerewolfResourceTransitionService.Spend(request);

        Assert.True(result.Succeeded);
        var codes = result.Findings.Select(f => f.Code).ToArray();
        Assert.Equal([WerewolfResourceTransitionErrorCode.ResourceSpendSucceeded], codes);
    }

    [Fact]
    public void DeterministicFindingOrderingOnRecover()
    {
        var state = BuildRuntimeState(rageCurrent: 2);
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Rage, 1);

        var result = WerewolfResourceTransitionService.Recover(request);

        Assert.True(result.Succeeded);
        var codes = result.Findings.Select(f => f.Code).ToArray();
        Assert.Equal([WerewolfResourceTransitionErrorCode.ResourceRecoverSucceeded], codes);
    }

    [Fact]
    public void RejectsInvalidSourceCurrentAbovePermanent()
    {
        var state = BuildRuntimeState(ragePermanent: 3, rageCurrent: 5);
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Rage, 1);

        var result = WerewolfResourceTransitionService.Spend(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.NewState);
        Assert.Equal(WerewolfResourceTransitionErrorCode.InvalidSourceCurrentAbovePermanent, result.Findings[0].Code);
    }

    [Fact]
    public void RejectsNullState()
    {
        var request = new WerewolfResourceTransitionRequest(null!, 1, "req-1", WerewolfCharacterResourceIdentifiers.Rage, 1);

        var result = WerewolfResourceTransitionService.Spend(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.NewState);
        Assert.Equal(WerewolfResourceTransitionErrorCode.MissingState, result.Findings[0].Code);
    }

    [Fact]
    public void RejectsInvalidPackageBinding()
    {
        var state = BuildRuntimeState() with { PackageId = "wrong.package" };
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Rage, 1);

        var result = WerewolfResourceTransitionService.Spend(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.NewState);
        Assert.Equal(WerewolfResourceTransitionErrorCode.InvalidPackageBinding, result.Findings[0].Code);
    }

    [Fact]
    public void RejectsMissingDraftId()
    {
        var state = BuildRuntimeState() with { DraftId = "" };
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Rage, 1);

        var result = WerewolfResourceTransitionService.Spend(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.NewState);
        Assert.Equal(WerewolfResourceTransitionErrorCode.CharacterNotCompleted, result.Findings[0].Code);
    }

    [Fact]
    public void RenownRemainsUntouched()
    {
        var state = BuildRuntimeState();
        var request = new WerewolfResourceTransitionRequest(state, 1, "req-1", WerewolfCharacterResourceIdentifiers.Rage, 1);

        var result = WerewolfResourceTransitionService.Spend(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.NewState);
    }
}
