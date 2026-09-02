using Chronicle.Desktop.PackTotem;
using Chronicle.RuleSets.Abstractions.PackageSources;
using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.Desktop.Tests.PackTotem;

public sealed class WerewolfPackTotemBoundaryAdapterTests
{
    private static readonly IReadOnlyList<int> SuccessDice = [7, 8, 9];
    private static readonly IReadOnlyList<int> InvalidDice = [0, 0, 0];

    [Fact]
    public void SuccessfulTotemRiteProducesBoundarySignalReceived()
    {
        var registry = BuildRegistry();
        var adapter = new WerewolfPackTotemBoundaryAdapter();

        var result = adapter.Validate(
            registry,
            new ValidatePackTotemBoundaryRequest(
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                OperationKey: WerewolfReferenceRuntime.ExecuteRiteOperation,
                ExpectedRiteKey: WerewolfRiteIdentifiers.Totem,
                DiceValues: SuccessDice,
                HasTargetPiece: false,
                RequestId: "req-success"));

        Assert.True(result.Succeeded);
        Assert.Equal(PackTotemBoundaryValidationKind.BoundarySignalReceived, result.Kind);
        Assert.Null(result.FailureReason);
        Assert.NotNull(result.S4Observation);

        var observation = result.S4Observation!;
        Assert.Equal(WerewolfRiteIdentifiers.Totem, observation.ObservedRiteKey);
        Assert.Equal("Line 2693", observation.SourceLocator);
        Assert.Contains("S4 represents this as a typed boundary", observation.Note);
    }

    [Fact]
    public void SuccessfulTotemRiteReportsTypedBoundaryPayloadVerbatim()
    {
        var registry = BuildRegistry();
        var adapter = new WerewolfPackTotemBoundaryAdapter();

        var result = adapter.Validate(
            registry,
            new ValidatePackTotemBoundaryRequest(
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                OperationKey: WerewolfReferenceRuntime.ExecuteRiteOperation,
                ExpectedRiteKey: WerewolfRiteIdentifiers.Totem,
                DiceValues: SuccessDice,
                HasTargetPiece: false,
                RequestId: "req-payload"));

        Assert.NotNull(result.S4Observation);
        var observation = result.S4Observation!;
        Assert.Equal("TotemId from Chronicle", observation.PayloadTotemId);
        Assert.Equal("PackId from Chronicle", observation.PayloadPackId);
        Assert.Empty(observation.PayloadMemberRoster);
        Assert.Equal(0, observation.PayloadTotemAggregation);
    }

    [Fact]
    public void InvalidDiceProduceRiteTestFailed()
    {
        var registry = BuildRegistry();
        var adapter = new WerewolfPackTotemBoundaryAdapter();

        var result = adapter.Validate(
            registry,
            new ValidatePackTotemBoundaryRequest(
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                OperationKey: WerewolfReferenceRuntime.ExecuteRiteOperation,
                ExpectedRiteKey: WerewolfRiteIdentifiers.Totem,
                DiceValues: InvalidDice,
                HasTargetPiece: false,
                RequestId: "req-invalid-dice"));

        Assert.False(result.Succeeded);
        Assert.Equal(PackTotemBoundaryValidationKind.RiteTestFailed, result.Kind);
        Assert.NotNull(result.FailureReason);
        Assert.Null(result.S4Observation);
    }

    [Fact]
    public void WrongExpectedRiteKeyReturnsInvalidRiteKeyWithoutServiceExecution()
    {
        var registry = BuildRegistry();
        var adapter = new WerewolfPackTotemBoundaryAdapter();

        var result = adapter.Validate(
            registry,
            new ValidatePackTotemBoundaryRequest(
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                OperationKey: WerewolfReferenceRuntime.ExecuteRiteOperation,
                ExpectedRiteKey: "rite.mystic.fetish",
                DiceValues: SuccessDice,
                HasTargetPiece: false,
                RequestId: "req-wrong-key"));

        Assert.False(result.Succeeded);
        Assert.Equal(PackTotemBoundaryValidationKind.InvalidRiteKey, result.Kind);
        Assert.Null(result.S4Observation);
        Assert.Contains(WerewolfRiteIdentifiers.Totem, result.FailureReason, StringComparison.Ordinal);
    }

    [Fact]
    public void UnregisteredRuntimeReturnsUnregisteredRuntime()
    {
        var registry = BuildRegistry();
        var adapter = new WerewolfPackTotemBoundaryAdapter();

        var result = adapter.Validate(
            registry,
            new ValidatePackTotemBoundaryRequest(
                PackageId: "chronicle.rulesets.unknown",
                PackageVersion: "9.9.9",
                OperationKey: WerewolfReferenceRuntime.ExecuteRiteOperation,
                ExpectedRiteKey: WerewolfRiteIdentifiers.Totem,
                DiceValues: SuccessDice,
                HasTargetPiece: false,
                RequestId: "req-missing-runtime"));

        Assert.False(result.Succeeded);
        Assert.Equal(PackTotemBoundaryValidationKind.UnregisteredRuntime, result.Kind);
        Assert.Null(result.S4Observation);
    }

    [Fact]
    public void UndeclaredOperationReturnsUndeclaredOperation()
    {
        var registry = BuildRegistry();
        var adapter = new WerewolfPackTotemBoundaryAdapter();

        var result = adapter.Validate(
            registry,
            new ValidatePackTotemBoundaryRequest(
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                OperationKey: "rite-runtime.unregistered-op",
                ExpectedRiteKey: WerewolfRiteIdentifiers.Totem,
                DiceValues: SuccessDice,
                HasTargetPiece: false,
                RequestId: "req-undeclared"));

        Assert.False(result.Succeeded);
        Assert.Equal(PackTotemBoundaryValidationKind.UndeclaredOperation, result.Kind);
        Assert.Null(result.S4Observation);
    }

    [Fact]
    public void TotemPayloadTypeIsDiscriminatedAsWerewolfTotemBindingBoundaryPayload()
    {
        var registry = BuildRegistry();
        var adapter = new WerewolfPackTotemBoundaryAdapter();

        var result = adapter.Validate(
            registry,
            new ValidatePackTotemBoundaryRequest(
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                OperationKey: WerewolfReferenceRuntime.ExecuteRiteOperation,
                ExpectedRiteKey: WerewolfRiteIdentifiers.Totem,
                DiceValues: SuccessDice,
                HasTargetPiece: false,
                RequestId: "req-discriminator"));

        Assert.Equal(PackTotemBoundaryValidationKind.BoundarySignalReceived, result.Kind);
        Assert.NotNull(result.S4Observation);
        var observation = result.S4Observation!;
        Assert.Equal(WerewolfRiteIdentifiers.Totem, observation.ObservedRiteKey);
        Assert.Equal("TotemId from Chronicle", observation.PayloadTotemId);
        Assert.Equal("PackId from Chronicle", observation.PayloadPackId);
        Assert.Empty(observation.PayloadMemberRoster);
        Assert.Equal(0, observation.PayloadTotemAggregation);
        Assert.Equal("Line 2693", observation.SourceLocator);
    }

    [Fact]
    public void PlaceholderTotemIdAndPackIdAreReportedVerbatim()
    {
        var registry = BuildRegistry();
        var adapter = new WerewolfPackTotemBoundaryAdapter();

        var result = adapter.Validate(
            registry,
            new ValidatePackTotemBoundaryRequest(
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                OperationKey: WerewolfReferenceRuntime.ExecuteRiteOperation,
                ExpectedRiteKey: WerewolfRiteIdentifiers.Totem,
                DiceValues: SuccessDice,
                HasTargetPiece: false,
                RequestId: "req-placeholders"));

        Assert.NotNull(result.S4Observation);
        var observation = result.S4Observation!;
        Assert.Equal("TotemId from Chronicle", observation.PayloadTotemId);
        Assert.Equal("PackId from Chronicle", observation.PayloadPackId);
        Assert.Empty(observation.PayloadMemberRoster);
        Assert.Equal(0, observation.PayloadTotemAggregation);
    }

    [Fact]
    public void TwoConsecutiveCallsProduceEquivalentResults()
    {
        var registry = BuildRegistry();
        var adapter = new WerewolfPackTotemBoundaryAdapter();

        var request = new ValidatePackTotemBoundaryRequest(
            PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
            PackageVersion: WerewolfRuleSetPackage.PackageVersion,
            OperationKey: WerewolfReferenceRuntime.ExecuteRiteOperation,
            ExpectedRiteKey: WerewolfRiteIdentifiers.Totem,
            DiceValues: SuccessDice,
            HasTargetPiece: false,
            RequestId: "req-determinism");

        var first = adapter.Validate(registry, request);
        var second = adapter.Validate(registry, request);

        Assert.Equal(first.Succeeded, second.Succeeded);
        Assert.Equal(first.Kind, second.Kind);
        Assert.Equal(first.FailureReason, second.FailureReason);
        Assert.NotNull(first.S4Observation);
        Assert.NotNull(second.S4Observation);
        Assert.Equal(first.S4Observation!.ObservedRiteKey, second.S4Observation!.ObservedRiteKey);
        Assert.Equal(first.S4Observation.PayloadTotemId, second.S4Observation.PayloadTotemId);
        Assert.Equal(first.S4Observation.PayloadPackId, second.S4Observation.PayloadPackId);
        Assert.Equal(first.S4Observation.PayloadTotemAggregation, second.S4Observation.PayloadTotemAggregation);
    }

    [Fact]
    public void SuccessfulRiteObservationReportsSuccessCountAndDifficulty()
    {
        var registry = BuildRegistry();
        var adapter = new WerewolfPackTotemBoundaryAdapter();

        var result = adapter.Validate(
            registry,
            new ValidatePackTotemBoundaryRequest(
                PackageId: WerewolfRuleSetPackage.ProvisionalPackageId,
                PackageVersion: WerewolfRuleSetPackage.PackageVersion,
                OperationKey: WerewolfReferenceRuntime.ExecuteRiteOperation,
                ExpectedRiteKey: WerewolfRiteIdentifiers.Totem,
                DiceValues: SuccessDice,
                HasTargetPiece: false,
                RequestId: "req-success-count"));

        Assert.NotNull(result.S4Observation);
        var observation = result.S4Observation!;
        Assert.True(observation.SuccessCount > 0);
        Assert.NotNull(observation.Difficulty);
        Assert.True(observation.Difficulty.Value > 0);
        Assert.False(string.IsNullOrEmpty(observation.InterpretationStatus));
        Assert.False(string.IsNullOrEmpty(observation.Effect));
    }

    private static RuleSetRuntimeRegistry BuildRegistry()
    {
        var discovery = RuleSetPackageSourceDiscoveryService.Discover(new RuleSetPackageSourceDiscoveryRequest([RuleSetsRoot()]));
        var registration = RuleSetPackageRegistrationService.Register(new RuleSetPackageRegistrationRequest(discovery.ValidatedPackages, 1));
        return RuleSetRuntimeRegistrationService.Register(new RuleSetRuntimeRegistrationRequest(registration.Catalog, [new WerewolfReferenceRuntime()])).Registry;
    }

    private static string RuleSetsRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Chronicle.sln")))
            {
                return Path.Combine(directory.FullName, "rule-sets");
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not find repository root from test base directory.");
    }
}
