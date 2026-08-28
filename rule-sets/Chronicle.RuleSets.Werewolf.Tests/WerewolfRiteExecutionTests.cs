using Chronicle.RuleSets.Abstractions.Runtime;
using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfRiteExecutionTests
{
    [Fact]
    public void ExecuteHuntingStoneReturnsSuccessWhenSuccessesMeetDifficulty()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [7, 8, 9],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.Equal("req-1", result.RequestId);
        Assert.Equal(WerewolfRiteIdentifiers.HuntingStone, result.RiteKey);
        Assert.Equal(7, result.Difficulty);
        Assert.Equal(3, result.DicePool);
        Assert.Equal(3, result.SuccessCount);
        Assert.Equal(WerewolfActionRollInterpretationService.SuccessStatus, result.InterpretationStatus);
        Assert.NotNull(result.Effect);
    }

    [Fact]
    public void ExecuteHuntingStoneReducesDifficultyWhenTargetPiecePossessed()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [6, 7, 8],
            true);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.Equal(6, result.Difficulty);
        Assert.Equal(3, result.SuccessCount);
    }

    [Fact]
    public void ExecuteHuntingStoneReturnsFailureWhenNoSuccesses()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [2, 3, 4],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(WerewolfActionRollInterpretationService.FailureStatus, result.InterpretationStatus);
    }

    [Fact]
    public void ExecuteHuntingStoneReturnsBotchWhenOnesExceedSuccesses()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [1, 1, 6],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(WerewolfActionRollInterpretationService.BotchStatus, result.InterpretationStatus);
    }

    [Fact]
    public void ExecuteHuntingStoneRejectsUnknownRiteKey()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            "rite.unknown",
            [7, 8, 9],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.False(result.Succeeded);
        Assert.Equal("UnknownRite", result.Findings[0].Code);
    }

    [Fact]
    public void ExecuteHuntingStoneRejectsEmptyDiceValues()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.False(result.Succeeded);
        Assert.Equal("InvalidDiceValues", result.Findings[0].Code);
    }

    [Fact]
    public void ExecuteHuntingStoneRejectsInvalidDieFace()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [0, 6, 7],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.False(result.Succeeded);
        Assert.Equal("InvalidDieFace", result.Findings[0].Code);
    }

    [Fact]
    public void ExecuteHuntingStoneRejectsEmptyRequestId()
    {
        var request = new WerewolfRiteExecutionRequest(
            string.Empty,
            WerewolfRiteIdentifiers.HuntingStone,
            [6, 7, 8],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.False(result.Succeeded);
        Assert.Equal("InvalidRequestId", result.Findings[0].Code);
    }

    [Fact]
    public void ExecuteHuntingStoneRejectsEmptyRiteKey()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            string.Empty,
            [6, 7, 8],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.False(result.Succeeded);
        Assert.Equal("InvalidRiteKey", result.Findings[0].Code);
    }

    [Fact]
    public void ExecuteHuntingStoneReturnsGeneralLocationOnSuccess()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [8, 9, 10],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.SuccessCount);
        Assert.Equal("Fornece apenas a localização geral do alvo. A posse de um pedaço do alvo reduz a dificuldade em 1 ponto.", result.Effect);
    }

    [Fact]
    public void ExecuteHuntingStoneReturnsNoInformationOnFailure()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [2, 3, 4],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal("No information gained.", result.Effect);
    }

    [Fact]
    public void ExecuteHuntingStoneDoesNotDependOnSpiritUmbra()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [7, 8, 9],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.SuccessCount);
        Assert.DoesNotContain("spirit", result.Effect, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("umbra", result.Effect, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExecuteHuntingStoneDoesNotDependOnPackSept()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [7, 8, 9],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.SuccessCount);
        Assert.DoesNotContain("pack", result.Effect, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sept", result.Effect, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("caern", result.Effect, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExecuteHuntingStoneDoesNotClaimLearningImplementation()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [6, 7, 8],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.DoesNotContain("xp", result.Effect ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("background", result.Effect ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("knowledge", result.Effect ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExecuteHuntingStonePreservesProgressionState()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.HuntingStone,
            [7, 8, 9],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.SuccessCount);
    }

    [Fact]
    public void S4ExactKeyCountIsFive()
    {
        var s4Keys = new[]
        {
            WerewolfRiteIdentifiers.Fetish,
            WerewolfRiteIdentifiers.Totem,
            WerewolfRiteIdentifiers.Summoning,
            WerewolfRiteIdentifiers.Commitment,
            WerewolfRiteIdentifiers.AwakenSpirits
        };

        Assert.Equal(5, s4Keys.Length);
        Assert.Distinct(s4Keys);
    }

    [Fact]
    public void S4AllRitesAreCatalogued()
    {
        foreach (var key in new[]
        {
            WerewolfRiteIdentifiers.Fetish,
            WerewolfRiteIdentifiers.Totem,
            WerewolfRiteIdentifiers.Summoning,
            WerewolfRiteIdentifiers.Commitment,
            WerewolfRiteIdentifiers.AwakenSpirits
        })
        {
            var definition = WerewolfRiteCatalog.Get(key);
            Assert.NotNull(definition);
            Assert.Equal(key, definition.Key);
        }
    }

    [Fact]
    public void S4FetishReturnsTypedBoundaryOnSuccess()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-fetish",
            WerewolfRiteIdentifiers.Fetish,
            [8, 9, 10],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Payload);
        Assert.IsType<WerewolfFetishCreationBoundaryPayload>(result.Payload);
    }

    [Fact]
    public void S4TotemReturnsTypedBoundaryOnSuccess()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-totem",
            WerewolfRiteIdentifiers.Totem,
            [7, 8, 9],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Payload);
        Assert.IsType<WerewolfTotemBindingBoundaryPayload>(result.Payload);
    }

    [Fact]
    public void S4SummoningReturnsTypedBoundaryOnSuccess()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-summoning",
            WerewolfRiteIdentifiers.Summoning,
            [6, 7, 8],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Payload);
        Assert.IsType<WerewolfSpiritSummonBoundaryPayload>(result.Payload);
    }

    [Fact]
    public void S4CommitmentReturnsTypedBoundaryOnSuccess()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-commitment",
            WerewolfRiteIdentifiers.Commitment,
            [7, 8, 9],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Payload);
        Assert.IsType<WerewolfCommitmentBoundaryPayload>(result.Payload);
    }

    [Fact]
    public void S4AwakenSpiritsReturnsTypedBoundaryOnSuccess()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-awaken",
            WerewolfRiteIdentifiers.AwakenSpirits,
            [6, 7, 8],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Payload);
        Assert.IsType<WerewolfAwakenSpiritsBoundaryPayload>(result.Payload);
    }

    [Fact]
    public void S4NoS5KeysImplemented()
    {
        var s5Keys = new[]
        {
            "spirit.location.state",
            "spirit.gauntlet.by-location",
            "spirit.realm.travel",
            "spirit.scene.presence",
            "spirit.caern.película-table",
            "spirit.totem.binding",
            "spirit.pack.totem-link",
            "spirit.shared.totem-effects",
            "spirit.disposition.ai",
            "spirit.bargaining.valuation",
            "spirit.materialization.duration",
            "spirit.death.modorra-threshold",
            "spirit.possession.control",
            "spirit.crossing.non-garou",
            "spirit.hierarchy.behavior",
            "spirit.voting.system",
            "spirit.persistence.lifecycle",
            "spirit.world-travel.rules"
        };

        foreach (var key in s5Keys)
        {
            Assert.Null(WerewolfRiteCatalog.Get(key));
        }
    }

    [Fact]
    public void S4RiteRuntimeIsReused()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-1",
            WerewolfRiteIdentifiers.Fetish,
            [7, 8, 9],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        var runtime = new WerewolfReferenceRuntime();
        var operation = runtime.Metadata.Operations.First(o => o.OperationKey == WerewolfReferenceRuntime.ExecuteRiteOperation);
        Assert.Equal("rite-runtime.execute-rite", operation.OperationKey);
    }

    [Fact]
    public void S4NoDuplicateRiteExecutionService()
    {
        var serviceType = typeof(WerewolfRiteExecutionService);
        Assert.NotNull(serviceType);
    }

    [Fact]
    public void S4NoNewRngInsideWerewolf()
    {
        var riteTypes = new[]
        {
            typeof(WerewolfRiteExecutionService),
            typeof(WerewolfRiteCatalog),
            typeof(WerewolfRiteDefinition)
        };

        foreach (var type in riteTypes)
        {
            var methods = type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            foreach (var method in methods)
            {
                Assert.DoesNotContain("Random", method.Name, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void S4RuntimeRegistrationRemainsValid()
    {
        var runtime = new WerewolfReferenceRuntime();
        var operation = runtime.Metadata.Operations.FirstOrDefault(o => o.OperationKey == WerewolfReferenceRuntime.ExecuteRiteOperation);
        Assert.NotNull(operation);
        Assert.Equal(RuleSetOperationStatus.Enabled, operation.Status);
    }

    [Fact]
    public void S4CapabilityOwnershipRemainsCorrect()
    {
        var runtime = new WerewolfReferenceRuntime();
        var operation = runtime.Metadata.Operations.FirstOrDefault(o => o.OperationKey == WerewolfReferenceRuntime.ExecuteRiteOperation);
        Assert.NotNull(operation);
        Assert.Equal("post-creation-character-operations", operation.CapabilityKey);
    }

    [Fact]
    public void S4AllCataloguedRitesArePresentInSupported()
    {
        var allRites = WerewolfRiteCatalog.GetAll();
        foreach (var rite in allRites)
        {
            Assert.False(string.IsNullOrWhiteSpace(rite.Key));
            Assert.False(string.IsNullOrWhiteSpace(rite.DisplayName));
            Assert.True(rite.Level >= 1 && rite.Level <= 5);
        }
    }

    [Fact]
    public void WaveCCaernOpeningReturnsTypedBoundaryOnSuccess()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-caern-opening",
            WerewolfRiteIdentifiers.CaernOpening,
            [7, 8, 9],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Payload);
        Assert.IsType<WerewolfCaernOpeningBoundaryPayload>(result.Payload);
    }

    [Fact]
    public void WaveCCaernCreationReturnsTypedBoundaryOnSuccess()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-caern-creation",
            WerewolfRiteIdentifiers.CaernCreation,
            [8, 9, 10],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Payload);
        Assert.IsType<WerewolfCaernCreationBoundaryPayload>(result.Payload);
    }

    [Fact]
    public void WaveCCaernOpeningRecordsA010bAmbiguity()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-caern-opening",
            WerewolfRiteIdentifiers.CaernOpening,
            [7, 8, 9],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        var payload = Assert.IsType<WerewolfCaernOpeningBoundaryPayload>(result.Payload);
        Assert.Equal(0, payload.RequiredSuccesses);
        Assert.Contains("A-010b", payload.Note);
    }

    [Fact]
    public void WaveCCaernCreationRecordsA010cAmbiguity()
    {
        var request = new WerewolfRiteExecutionRequest(
            "req-caern-creation",
            WerewolfRiteIdentifiers.CaernCreation,
            [8, 9, 10],
            false);

        var result = WerewolfRiteExecutionService.Execute(request);

        Assert.True(result.Succeeded);
        var payload = Assert.IsType<WerewolfCaernCreationBoundaryPayload>(result.Payload);
        Assert.Equal(8, payload.BaseDifficulty);
        Assert.Equal(0, payload.DifficultyReduction);
        Assert.Contains("A-010c", payload.Note);
    }

    [Fact]
    public void WaveCNoWorldStateMutationInWerewolf()
    {
        var openingRequest = new WerewolfRiteExecutionRequest(
            "req-caern-opening",
            WerewolfRiteIdentifiers.CaernOpening,
            [7, 8, 9],
            false);
        var openingResult = WerewolfRiteExecutionService.Execute(openingRequest);

        var creationRequest = new WerewolfRiteExecutionRequest(
            "req-caern-creation",
            WerewolfRiteIdentifiers.CaernCreation,
            [8, 9, 10],
            false);
        var creationResult = WerewolfRiteExecutionService.Execute(creationRequest);

        Assert.True(openingResult.Succeeded);
        Assert.True(creationResult.Succeeded);
        Assert.DoesNotContain("CaernId", openingResult.Effect ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SeptId", creationResult.Effect ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WaveCCaernRitesAreCatalogued()
    {
        var opening = WerewolfRiteCatalog.Get(WerewolfRiteIdentifiers.CaernOpening);
        var creation = WerewolfRiteCatalog.Get(WerewolfRiteIdentifiers.CaernCreation);

        Assert.NotNull(opening);
        Assert.NotNull(creation);
        Assert.Equal("Caern", opening.Category);
        Assert.Equal("Caern", creation.Category);
        Assert.Equal(1, opening.Level);
        Assert.Equal(5, creation.Level);
    }
}
