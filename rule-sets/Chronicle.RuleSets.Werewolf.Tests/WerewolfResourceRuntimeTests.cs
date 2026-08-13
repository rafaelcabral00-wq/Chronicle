using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfResourceRuntimeTests
{
    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null
    };

    private static RuleSetRuntimeRegistry CreateRegistry()
    {
        return WerewolfTestRuntimeHelpers.RegisteredRuntimeRegistry();
    }

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
    public void SpendResourceThroughRegistrySucceeds()
    {
        var registry = CreateRegistry();
        var state = BuildRuntimeState(rageCurrent: 5);
        var stateJson = System.Text.Json.JsonSerializer.Serialize(state, JsonOptions);

        var result = registry.Execute(new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            WerewolfReferenceRuntime.SpendResourceOperation,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = "req-1",
                ["currentState"] = stateJson,
                ["expectedRuntimeStateVersion"] = "1",
                ["resourceId"] = WerewolfCharacterResourceIdentifiers.Rage,
                ["amount"] = "2"
            }));

        Assert.True(result.Succeeded);
        Assert.True(result.Outputs.TryGetValue("newState", out var newStateJson));
        Assert.False(string.IsNullOrEmpty(newStateJson));
        Assert.Equal("2", result.Outputs["newRuntimeStateVersion"]);
    }

    [Fact]
    public void RecoverResourceThroughRegistrySucceeds()
    {
        var registry = CreateRegistry();
        var state = BuildRuntimeState(ragePermanent: 5, rageCurrent: 2);
        var stateJson = System.Text.Json.JsonSerializer.Serialize(state, JsonOptions);

        var result = registry.Execute(new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            WerewolfReferenceRuntime.RecoverResourceOperation,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = "req-1",
                ["currentState"] = stateJson,
                ["expectedRuntimeStateVersion"] = "1",
                ["resourceId"] = WerewolfCharacterResourceIdentifiers.Rage,
                ["amount"] = "2"
            }));

        Assert.True(result.Succeeded);
        Assert.True(result.Outputs.TryGetValue("newState", out var newStateJson));
        Assert.False(string.IsNullOrEmpty(newStateJson));
        Assert.Equal("2", result.Outputs["newRuntimeStateVersion"]);
    }

    [Fact]
    public void StaleTransitionFailsThroughRegistry()
    {
        var registry = CreateRegistry();
        var state = BuildRuntimeState(rageCurrent: 5, runtimeStateVersion: 5);
        var stateJson = System.Text.Json.JsonSerializer.Serialize(state, JsonOptions);

        var result = registry.Execute(new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            WerewolfReferenceRuntime.SpendResourceOperation,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = "req-1",
                ["currentState"] = stateJson,
                ["expectedRuntimeStateVersion"] = "3",
                ["resourceId"] = WerewolfCharacterResourceIdentifiers.Rage,
                ["amount"] = "1"
            }));

        Assert.False(result.Succeeded);
        Assert.Equal(RuleSetOperationFailureCode.InvalidRequest, result.FailureCode);
    }

    [Fact]
    public void PackageBindingRemainsStableAcrossTransitions()
    {
        var registry = CreateRegistry();
        var state = BuildRuntimeState(rageCurrent: 5);
        var stateJson = System.Text.Json.JsonSerializer.Serialize(state, JsonOptions);

        var result = registry.Execute(new RuleSetOperationRequest(
            WerewolfRuleSetPackage.ProvisionalPackageId,
            WerewolfRuleSetPackage.PackageVersion,
            WerewolfReferenceRuntime.SpendResourceOperation,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["requestId"] = "req-1",
                ["currentState"] = stateJson,
                ["expectedRuntimeStateVersion"] = "1",
                ["resourceId"] = WerewolfCharacterResourceIdentifiers.Rage,
                ["amount"] = "1"
            }));

        Assert.True(result.Succeeded);
        Assert.True(result.Outputs.TryGetValue("newState", out var newStateJson));
        Assert.False(string.IsNullOrEmpty(newStateJson));
        var newState = System.Text.Json.JsonSerializer.Deserialize<WerewolfRuntimeCharacterState>(newStateJson, JsonOptions);
        Assert.NotNull(newState);
        Assert.Equal(WerewolfRuleSetPackage.ProvisionalPackageId, newState!.PackageId);
        Assert.Equal(WerewolfRuleSetPackage.PackageVersion, newState.PackageVersion);
        Assert.Equal("draft-1", newState.DraftId);
    }
}
