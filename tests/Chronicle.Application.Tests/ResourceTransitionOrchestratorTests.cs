using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Xunit;

namespace Chronicle.Application.Tests;

public sealed class ResourceTransitionOrchestratorTests
{
    private sealed class FakeRuntime(string operationKey, RuleSetOperationResult result) : IRuleSetRuntime
    {
        public RuleSetRuntimeMetadata Metadata => new(
            new RuleSetRuntimeIdentity("fake.package", "1.0.0", "Fake Runtime", 1),
            "fake.scope",
            [new RuleSetOperationDescriptor(operationKey, "fake-capability", RuleSetOperationStatus.Enabled)]);

        public RuleSetOperationResult Execute(RuleSetOperationRequest request)
        {
            return result;
        }
    }

    private static RuleSetRuntimeRegistry CreateRegistry(IRuleSetRuntime runtime)
    {
        var catalog = new RuleSetPackageCatalog(
        [
            new RegisteredRuleSetPackageDescriptor(
                "fake.package",
                "provisional",
                "1.0.0",
                "fake.scope",
                "0.1.0",
                "same-major-contract",
                1,
                ["en"],
                ["fake-capability"],
                [],
                "fake-path")
        ]);

        var registration = RuleSetRuntimeRegistrationService.Register(new RuleSetRuntimeRegistrationRequest(catalog, [runtime]));
        return registration.Registry;
    }

    [Fact]
    public void TransitionReturnsResultFromRegistry()
    {
        var expectedResult = new RuleSetOperationResult(
            true,
            null,
            [],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["newState"] = "{\"PackageId\":\"fake.package\",\"PackageVersion\":\"1.0.0\",\"DraftId\":\"draft-1\",\"RuntimeStateVersion\":2,\"PackageBinding\":{},\"RagePermanent\":5,\"RageCurrent\":3,\"GnosisPermanent\":3,\"GnosisCurrent\":3,\"WillpowerPermanent\":4,\"WillpowerCurrent\":4}",
                ["newRuntimeStateVersion"] = "2"
            });

        var registry = CreateRegistry(new FakeRuntime("character-runtime.spend-resource", expectedResult));
        var orchestrator = new ResourceTransitionOrchestrator(registry);

        var request = new ResourceTransitionRequest(
            "fake.package",
            "1.0.0",
            "character-runtime.spend-resource",
            "req-1",
            "{\"dummy\":true}",
            1,
            "character.resource.rage",
            2);

        var result = orchestrator.Transition(request);

        Assert.True(result.Succeeded);
        Assert.Equal("req-1", result.RequestId);
        Assert.NotNull(result.NewStateJson);
        Assert.Equal(2, result.NewRuntimeStateVersion);
        Assert.Equal("resolved", result.ResolutionStage);
    }

    [Fact]
    public void TransitionReturnsFailureWhenRegistryFails()
    {
        var failureResult = new RuleSetOperationResult(
            false,
            RuleSetOperationFailureCode.InvalidRequest,
            [new RuleSetRuntimeFinding(RuleSetRuntimeFindingSeverity.Error, "TestError", "test")],
            new Dictionary<string, string>(StringComparer.Ordinal));

        var registry = CreateRegistry(new FakeRuntime("character-runtime.spend-resource", failureResult));
        var orchestrator = new ResourceTransitionOrchestrator(registry);

        var request = new ResourceTransitionRequest(
            "fake.package",
            "1.0.0",
            "character-runtime.spend-resource",
            "req-1",
            "{\"dummy\":true}",
            1,
            "character.resource.rage",
            2);

        var result = orchestrator.Transition(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.NewStateJson);
        Assert.Equal("orchestration-failed", result.ResolutionStage);
    }

    [Fact]
    public void TransitionReturnsFailureWhenOutputsMissing()
    {
        var registry = CreateRegistry(new FakeRuntime("character-runtime.spend-resource", new RuleSetOperationResult(true, null, [], new Dictionary<string, string>(StringComparer.Ordinal))));
        var orchestrator = new ResourceTransitionOrchestrator(registry);

        var request = new ResourceTransitionRequest(
            "fake.package",
            "1.0.0",
            "character-runtime.spend-resource",
            "req-1",
            "{\"dummy\":true}",
            1,
            "character.resource.rage",
            2);

        var result = orchestrator.Transition(request);

        Assert.False(result.Succeeded);
        Assert.Equal("invalid-operation-output", result.ResolutionStage);
    }
}
