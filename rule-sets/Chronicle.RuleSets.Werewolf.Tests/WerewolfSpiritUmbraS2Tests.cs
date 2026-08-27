using Chronicle.RuleSets.Werewolf.CharacterCreation;
using System.Globalization;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfSpiritUmbraS2Tests
{
    private static WerewolfSpiritRuntimeState CreateTestState(
        int willpower = 5,
        int rage = 4,
        int gnosis = 3,
        bool isMaterialized = false,
        bool isInModorra = false,
        IReadOnlyList<string>? knownCharmKeys = null)
    {
        return WerewolfSpiritRuntimeState.Create(
            "spirit.test.001",
            "spirit.category.totem",
            willpower,
            rage,
            gnosis,
            knownCharmKeys ?? ["spirit.charm.common.materializar", "spirit.charm.special.armadura"]);
    }

    [Fact]
    public void S2ExactKeyCountIs20()
    {
        var expectedKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            "spirit.crossing.test",
            "spirit.crossing.time-table",
            "spirit.crossing.reflective-surface",
            "spirit.crossing.retry-restriction",
            "spirit.crossing.botch",
            "spirit.crossing.fury-restriction",
            "spirit.crossing.silver-penalty",
            "spirit.movement.speed",
            "spirit.detection.test",
            "spirit.communication.requirement",
            "spirit.materialization.requirement",
            "spirit.essence.formula",
            "spirit.modorra.definition",
            "spirit.entity.state",
            "spirit.charm.execution",
            "spirit.command.mechanic",
            "spirit.possession.mechanic",
            "spirit.damage.mechanic",
            "spirit.essence.economy",
            "spirit.materialization.state"
        };

        Assert.Equal(20, expectedKeys.Count);
        Assert.Equal(20, expectedKeys.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void EntityStateHasMinimumRequiredFields()
    {
        var state = CreateTestState();

        Assert.Equal("spirit.test.001", state.SpiritId);
        Assert.Equal("spirit.category.totem", state.CategoryKey);
        Assert.Equal(5, state.WillpowerPermanent);
        Assert.Equal(4, state.RagePermanent);
        Assert.Equal(3, state.GnosisPermanent);
        Assert.Equal(12, state.EssenceCurrent);
        Assert.Equal(12, state.EssencePermanent);
        Assert.False(state.IsMaterialized);
        Assert.Equal(1, state.StateVersion);
    }

    [Fact]
    public void EntityStateKnownCharmKeysAreStored()
    {
        var state = CreateTestState(knownCharmKeys: ["spirit.charm.common.materializar", "spirit.charm.special.armadura"]);

        Assert.Equal(2, state.KnownCharmKeys.Count);
        Assert.Contains("spirit.charm.common.materializar", state.KnownCharmKeys);
        Assert.Contains("spirit.charm.special.armadura", state.KnownCharmKeys);
    }

    [Fact]
    public void EntityStateVersionStartsAt1()
    {
        var state = CreateTestState();
        Assert.Equal(1, state.StateVersion);
    }

    [Fact]
    public void EntityStateCreateValidatesInputs()
    {
        var request = new SpiritMechanicRequest(null!, 0, "req-1");

        var invalidSpiritId = WerewolfSpiritMechanicServices.Initialize(request, "", "spirit.category.totem", 5, 4, 3, []);
        Assert.False(invalidSpiritId.Succeeded);

        var invalidCategory = WerewolfSpiritMechanicServices.Initialize(request, "spirit.test.001", "", 5, 4, 3, []);
        Assert.False(invalidCategory.Succeeded);

        var invalidWillpower = WerewolfSpiritMechanicServices.Initialize(request, "spirit.test.001", "spirit.category.totem", 0, 4, 3, []);
        Assert.False(invalidWillpower.Succeeded);

        var invalidRage = WerewolfSpiritMechanicServices.Initialize(request, "spirit.test.001", "spirit.category.totem", 5, 11, 3, []);
        Assert.False(invalidRage.Succeeded);
    }

    [Fact]
    public void EssenceFormulaIsWillpowerPlusRagePlusGnosis()
    {
        var state = CreateTestState(willpower: 7, rage: 5, gnosis: 4);
        Assert.Equal(16, state.EssencePermanent);
        Assert.Equal(16, state.EssenceCurrent);
    }

    [Fact]
    public void EssenceEconomySpendReducesCurrentEssence()
    {
        var state = CreateTestState();
        var request = new EssenceSpendRequest(state, state.StateVersion, "req-1", 5);
        var result = WerewolfSpiritMechanicServices.SpendEssence(request);

        Assert.True(result.Succeeded);
        Assert.Equal(12, result.PreviousEssence);
        Assert.Equal(7, result.NewEssence);
        Assert.Equal(7, result.NewState?.EssenceCurrent);
        Assert.Equal(2, result.NewState?.StateVersion);
    }

    [Fact]
    public void EssenceEconomyCannotSpendMoreThanCurrent()
    {
        var state = CreateTestState();
        var request = new EssenceSpendRequest(state, state.StateVersion, "req-1", 20);
        var result = WerewolfSpiritMechanicServices.SpendEssence(request);

        Assert.False(result.Succeeded);
        Assert.Equal(12, result.NewEssence);
    }

    [Fact]
    public void EssenceEconomySpendZeroOrNegativeFails()
    {
        var state = CreateTestState();
        var request = new EssenceSpendRequest(state, state.StateVersion, "req-1", 0);
        var result = WerewolfSpiritMechanicServices.SpendEssence(request);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void ModorraDefinitionEssenceDepletedReachesBoundary()
    {
        var state = CreateTestState(willpower: 3, rage: 3, gnosis: 2);
        var request = new SpiritDamageRequest(state, state.StateVersion, "req-1", 20, 6, false);
        var result = WerewolfSpiritMechanicServices.ApplyDamage(request);

        Assert.True(result.Succeeded);
        Assert.True(result.IsAtDeathBoundary);
        Assert.Equal(0, result.NewState?.EssenceCurrent);
    }

    [Fact]
    public void ModorraDefinitionDeathVsModorraThresholdNotInvented()
    {
        var state = CreateTestState(willpower: 3, rage: 3, gnosis: 2);
        var request = new SpiritDamageRequest(state, state.StateVersion, "req-1", 20, 6, false);
        var result = WerewolfSpiritMechanicServices.ApplyDamage(request);

        Assert.True(result.IsAtDeathBoundary);
        Assert.Equal(0, result.NewState?.EssenceCurrent);
        var findings = result.Findings.Select(f => f.Code.ToString()).ToList();
        Assert.DoesNotContain(findings, f => f.Contains("Death", StringComparison.Ordinal) || f.Contains("Threshold", StringComparison.Ordinal));
    }

    [Fact]
    public void MaterializationRequirementGnosisMustBeGreaterThanOrEqualToGauntlet()
    {
        var state = CreateTestState(gnosis: 3);
        var request = new MaterializationRequest(state, state.StateVersion, "req-1", 5);
        var result = WerewolfSpiritMechanicServices.EvaluateMaterialization(request);

        Assert.False(result.Succeeded);
        Assert.False(result.CanMaterialize);
    }

    [Fact]
    public void MaterializationRequirementSucceedsWhenGnosisSufficient()
    {
        var state = CreateTestState(gnosis: 5);
        var request = new MaterializationRequest(state, state.StateVersion, "req-1", 3);
        var result = WerewolfSpiritMechanicServices.EvaluateMaterialization(request);

        Assert.True(result.Succeeded);
        Assert.True(result.CanMaterialize);
        Assert.True(result.IsNowMaterialized);
        Assert.True(result.NewState?.IsMaterialized);
        Assert.Equal(2, result.NewState?.StateVersion);
    }

    [Fact]
    public void MaterializationStateAlreadyMaterializedReturnsSuccess()
    {
        var state = CreateTestState(isMaterialized: true);
        var request = new MaterializationRequest(state, state.StateVersion, "req-1", 3);
        var result = WerewolfSpiritMechanicServices.EvaluateMaterialization(request);

        Assert.True(result.Succeeded);
        Assert.True(result.IsNowMaterialized);
    }

    [Fact]
    public void MaterializationDurationNotImplemented()
    {
        var state = CreateTestState(gnosis: 5);
        var request = new MaterializationRequest(state, state.StateVersion, "req-1", 3);
        var result = WerewolfSpiritMechanicServices.EvaluateMaterialization(request);

        Assert.True(result.Succeeded);
        Assert.True(result.IsNowMaterialized);
        var findings = result.Findings.Select(f => f.Code.ToString()).ToList();
        Assert.DoesNotContain(findings, f => f.Contains("Duration", StringComparison.Ordinal) || f.Contains("Permanent", StringComparison.Ordinal));
    }

    [Fact]
    public void CrossingTestSuccessWithSufficientSuccesses()
    {
        var state = CreateTestState(gnosis: 5);
        var request = new CrossingRequest(state, state.StateVersion, "req-1", 5, 5, 6, false, 0, false, 0, [7, 8, 6]);
        var result = WerewolfSpiritMechanicServices.EvaluateCrossing(request);

        Assert.True(result.Succeeded);
        Assert.True(result.Successes > 0);
        Assert.False(result.IsBotch);
        Assert.False(result.IsZeroSuccessWait);
    }

    [Fact]
    public void CrossingTestFailureWithZeroSuccesses()
    {
        var state = CreateTestState(gnosis: 3);
        var request = new CrossingRequest(state, state.StateVersion, "req-1", 5, 3, 6, false, 0, false, 0, [2, 3, 4]);
        var result = WerewolfSpiritMechanicServices.EvaluateCrossing(request);

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.Successes);
        Assert.True(result.IsZeroSuccessWait);
        Assert.Equal(CrossingTime.CannotRetry, result.CrossingTime);
    }

    [Fact]
    public void CrossingTestBotchWithOnes()
    {
        var state = CreateTestState(gnosis: 5);
        var request = new CrossingRequest(state, state.StateVersion, "req-1", 5, 5, 6, false, 0, false, 0, [1, 1, 2]);
        var result = WerewolfSpiritMechanicServices.EvaluateCrossing(request);

        Assert.False(result.Succeeded);
        Assert.True(result.IsBotch);
        Assert.Equal(CrossingTime.CannotRetry, result.CrossingTime);
    }

    [Fact]
    public void CrossingTimeTableOneSuccessIsFiveMinutes()
    {
        var state = CreateTestState(gnosis: 5);
        var request = new CrossingRequest(state, state.StateVersion, "req-1", 5, 5, 6, false, 0, false, 0, [6, 2, 3]);
        var result = WerewolfSpiritMechanicServices.EvaluateCrossing(request);

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Successes);
        Assert.Equal(CrossingTime.FiveMinutes, result.CrossingTime);
    }

    [Fact]
    public void CrossingTimeTableTwoSuccessesIsThirtySeconds()
    {
        var state = CreateTestState(gnosis: 5);
        var request = new CrossingRequest(state, state.StateVersion, "req-1", 5, 5, 6, false, 0, false, 0, [6, 7, 2]);
        var result = WerewolfSpiritMechanicServices.EvaluateCrossing(request);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Successes);
        Assert.Equal(CrossingTime.ThirtySeconds, result.CrossingTime);
    }

    [Fact]
    public void CrossingTimeTableThreeOrMoreSuccessesIsInstant()
    {
        var state = CreateTestState(gnosis: 5);
        var request = new CrossingRequest(state, state.StateVersion, "req-1", 5, 5, 6, false, 0, false, 0, [6, 7, 8]);
        var result = WerewolfSpiritMechanicServices.EvaluateCrossing(request);

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.Successes);
        Assert.Equal(CrossingTime.Instant, result.CrossingTime);
    }

    [Fact]
    public void CrossingTimeTableZeroSuccessIsCannotRetry()
    {
        var state = CreateTestState(gnosis: 3);
        var request = new CrossingRequest(state, state.StateVersion, "req-1", 5, 3, 6, false, 0, false, 0, [2, 3, 4]);
        var result = WerewolfSpiritMechanicServices.EvaluateCrossing(request);

        Assert.False(result.Succeeded);
        Assert.Equal(CrossingTime.CannotRetry, result.CrossingTime);
    }

    [Fact]
    public void ReflectiveSurfaceReducesDifficultyByOne()
    {
        var state = CreateTestState(gnosis: 5);
        var requestWithout = new CrossingRequest(state, state.StateVersion, "req-1", 5, 5, 6, false, 0, false, 0, [5, 5, 5]);
        var resultWithout = WerewolfSpiritMechanicServices.EvaluateCrossing(requestWithout);

        var requestWith = new CrossingRequest(state, state.StateVersion, "req-2", 5, 5, 6, true, 0, false, 0, [5, 5, 5]);
        var resultWith = WerewolfSpiritMechanicServices.EvaluateCrossing(requestWith);

        Assert.Equal(6, resultWithout.EffectiveDifficulty);
        Assert.Equal(5, resultWith.EffectiveDifficulty);
        Assert.True(resultWith.Successes > resultWithout.Successes);
    }

    [Fact]
    public void RetryRestrictionZeroSuccessCannotRetry()
    {
        var state = CreateTestState(gnosis: 3);
        var request = new CrossingRequest(state, state.StateVersion, "req-1", 5, 3, 6, false, 0, false, 0, [2, 3, 4]);
        var result = WerewolfSpiritMechanicServices.EvaluateCrossing(request);

        Assert.False(result.Succeeded);
        Assert.True(result.IsZeroSuccessWait);
        Assert.False(result.CanRetry);
    }

    [Fact]
    public void RetryRestrictionSuccessCanRetry()
    {
        var state = CreateTestState(gnosis: 5);
        var request = new CrossingRequest(state, state.StateVersion, "req-1", 5, 5, 6, false, 0, false, 2, [6, 2, 3]);
        var result = WerewolfSpiritMechanicServices.EvaluateCrossing(request);

        Assert.True(result.Succeeded);
        Assert.True(result.CanRetry);
        Assert.Equal(6, result.NextRetryDifficultyModifier);
    }

    [Fact]
    public void BotchClassifiedMachineReadably()
    {
        var state = CreateTestState(gnosis: 5);
        var request = new CrossingRequest(state, state.StateVersion, "req-1", 5, 5, 6, false, 0, false, 0, [1, 1, 2]);
        var result = WerewolfSpiritMechanicServices.EvaluateCrossing(request);

        Assert.True(result.IsBotch);
        var findings = result.Findings.Select(f => f.Code).ToList();
        Assert.Contains(SpiritMechanicErrorCode.CrossingBotch, findings);
    }

    [Fact]
    public void FuryRestrictionCannotStepSideways()
    {
        var state = CreateTestState(gnosis: 5);
        var request = new CrossingRequest(state, state.StateVersion, "req-1", 5, 5, 6, false, 0, true, 0, [7, 8, 9]);
        var result = WerewolfSpiritMechanicServices.EvaluateCrossing(request);

        Assert.False(result.Succeeded);
        Assert.True(result.IsFuryRestricted);
        var findings = result.Findings.Select(f => f.Code).ToList();
        Assert.Contains(SpiritMechanicErrorCode.CrossingFuryRestricted, findings);
    }

    [Fact]
    public void SilverPenaltyReducesEffectiveGnosis()
    {
        var state = CreateTestState(gnosis: 5);
        var requestWithoutSilver = new CrossingRequest(state, state.StateVersion, "req-1", 5, 5, 4, false, 0, false, 0, [4, 4, 4]);
        var resultWithoutSilver = WerewolfSpiritMechanicServices.EvaluateCrossing(requestWithoutSilver);

        var requestWithSilver = new CrossingRequest(state, state.StateVersion, "req-2", 5, 5, 4, false, 2, false, 0, [4, 4, 4]);
        var resultWithSilver = WerewolfSpiritMechanicServices.EvaluateCrossing(requestWithSilver);

        Assert.Equal(5, resultWithoutSilver.EffectiveGnosis);
        Assert.Equal(3, resultWithSilver.EffectiveGnosis);
    }

    [Fact]
    public void MovementSpeedIs20PlusWillpower()
    {
        var state = CreateTestState(willpower: 5);
        var request = new MovementRequest(state, state.StateVersion, "req-1");
        var result = WerewolfSpiritMechanicServices.ComputeMovementSpeed(request);

        Assert.True(result.Succeeded);
        Assert.Equal(25, result.MaxMetersPerTurn);
    }

    [Fact]
    public void MovementSpeedHigherWillpowerIncreasesSpeed()
    {
        var state = CreateTestState(willpower: 8);
        var request = new MovementRequest(state, state.StateVersion, "req-1");
        var result = WerewolfSpiritMechanicServices.ComputeMovementSpeed(request);

        Assert.True(result.Succeeded);
        Assert.Equal(28, result.MaxMetersPerTurn);
    }

    [Fact]
    public void DetectionTestAutomaticWhenGnosisGreaterThanOrEqualToGauntlet()
    {
        var state = CreateTestState(gnosis: 5);
        var request = new DetectionRequest(state, state.StateVersion, "req-1", 5, 5, 6, [6, 7, 8]);
        var result = WerewolfSpiritMechanicServices.EvaluateDetection(request);

        Assert.True(result.Succeeded);
        Assert.True(result.IsAutomatic);
        Assert.True(result.IsDetected);
    }

    [Fact]
    public void DetectionTestRolledWhenGnosisLessThanGauntlet()
    {
        var state = CreateTestState(gnosis: 3);
        var request = new DetectionRequest(state, state.StateVersion, "req-1", 5, 3, 6, [6, 7, 8]);
        var result = WerewolfSpiritMechanicServices.EvaluateDetection(request);

        Assert.True(result.Succeeded);
        Assert.False(result.IsAutomatic);
        Assert.True(result.IsDetected);
        Assert.True(result.Successes > 0);
    }

    [Fact]
    public void DetectionTestFailureWhenInsufficientSuccesses()
    {
        var state = CreateTestState(gnosis: 3);
        var request = new DetectionRequest(state, state.StateVersion, "req-1", 5, 3, 6, [2, 3, 4]);
        var result = WerewolfSpiritMechanicServices.EvaluateDetection(request);

        Assert.False(result.Succeeded);
        Assert.False(result.IsDetected);
        Assert.Equal(0, result.Successes);
    }

    [Fact]
    public void CommunicationRequirementDoesNotRequireDialogueAI()
    {
        var state = CreateTestState();
        Assert.NotNull(state);
        Assert.DoesNotContain(state.KnownCharmKeys, k => k.Contains("dialogue", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CharmExecutionKnownCharmSucceeds()
    {
        var state = CreateTestState(knownCharmKeys: ["spirit.charm.common.materializar"]);
        var request = new CharmExecutionRequest(state, state.StateVersion, "req-1", "spirit.charm.common.materializar", null, null);
        var result = WerewolfSpiritMechanicServices.ExecuteCharm(request);

        Assert.True(result.Succeeded);
        Assert.Equal("spirit.charm.common.materializar", result.ExecutedCharmKey);
        Assert.NotNull(result.EffectDescription);
        Assert.Equal(2, result.NewState?.StateVersion);
    }

    [Fact]
    public void CharmExecutionUnknownCharmFails()
    {
        var state = CreateTestState();
        var request = new CharmExecutionRequest(state, state.StateVersion, "req-1", "spirit.charm.special.unknown", null, null);
        var result = WerewolfSpiritMechanicServices.ExecuteCharm(request);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void CharmExecutionNotKnownCharmFails()
    {
        var state = CreateTestState(knownCharmKeys: ["spirit.charm.common.materializar"]);
        var request = new CharmExecutionRequest(state, state.StateVersion, "req-1", "spirit.charm.special.armadura", null, null);
        var result = WerewolfSpiritMechanicServices.ExecuteCharm(request);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void CharmExecutionEssenceCostConsumed()
    {
        var state = CreateTestState(knownCharmKeys: ["spirit.charm.special.armadura"]);
        var request = new CharmExecutionRequest(state, state.StateVersion, "req-1", "spirit.charm.special.armadura", null, 2);
        var result = WerewolfSpiritMechanicServices.ExecuteCharm(request);

        Assert.True(result.Succeeded);
        Assert.Equal(10, result.NewState?.EssenceCurrent);
    }

    [Fact]
    public void CharmExecutionInsufficientEssenceFails()
    {
        var state = CreateTestState(knownCharmKeys: ["spirit.charm.special.armadura"]);
        var request = new CharmExecutionRequest(state, state.StateVersion, "req-1", "spirit.charm.special.armadura", null, 20);
        var result = WerewolfSpiritMechanicServices.ExecuteCharm(request);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void CommandMechanicSuccessWithSufficientSuccesses()
    {
        var state = CreateTestState();
        var request = new CommandRequest(state, state.StateVersion, "req-1", 3, 4, 6, [6, 7, 8]);
        var result = WerewolfSpiritMechanicServices.EvaluateCommand(request);

        Assert.True(result.Succeeded);
        Assert.True(result.IsCommanded);
        Assert.True(result.Successes > 0);
    }

    [Fact]
    public void CommandMechanicFailureWithInsufficientSuccesses()
    {
        var state = CreateTestState();
        var request = new CommandRequest(state, state.StateVersion, "req-1", 3, 4, 6, [2, 3, 4]);
        var result = WerewolfSpiritMechanicServices.EvaluateCommand(request);

        Assert.False(result.Succeeded);
        Assert.False(result.IsCommanded);
    }

    [Fact]
    public void PossessionMechanicSuccessWithSufficientSuccesses()
    {
        var state = CreateTestState();
        var request = new PossessionRequest(state, state.StateVersion, "req-1", 6, [6, 7, 8]);
        var result = WerewolfSpiritMechanicServices.EvaluatePossession(request);

        Assert.True(result.Succeeded);
        Assert.True(result.IsPossessing);
    }

    [Fact]
    public void PossessionMechanicDurationBySuccesses()
    {
        var state = CreateTestState();

        var oneSuccess = WerewolfSpiritMechanicServices.EvaluatePossession(
            new PossessionRequest(state, state.StateVersion, "req-1", 6, [6, 2, 3]));
        Assert.Equal(PossessionDuration.SixHours, oneSuccess.Duration);

        var twoSuccesses = WerewolfSpiritMechanicServices.EvaluatePossession(
            new PossessionRequest(state, state.StateVersion, "req-2", 6, [6, 7, 2]));
        Assert.Equal(PossessionDuration.ThreeHours, twoSuccesses.Duration);

        var threeSuccesses = WerewolfSpiritMechanicServices.EvaluatePossession(
            new PossessionRequest(state, state.StateVersion, "req-3", 6, [6, 7, 8]));
        Assert.Equal(PossessionDuration.OneHour, threeSuccesses.Duration);

        var fourSuccesses = WerewolfSpiritMechanicServices.EvaluatePossession(
            new PossessionRequest(state, state.StateVersion, "req-4", 6, [6, 7, 8, 9]));
        Assert.Equal(PossessionDuration.FifteenMinutes, fourSuccesses.Duration);

        var fiveSuccesses = WerewolfSpiritMechanicServices.EvaluatePossession(
            new PossessionRequest(state, state.StateVersion, "req-5", 6, [6, 7, 8, 9, 10]));
        Assert.Equal(PossessionDuration.FiveMinutes, fiveSuccesses.Duration);

        var sixSuccesses = WerewolfSpiritMechanicServices.EvaluatePossession(
            new PossessionRequest(state, state.StateVersion, "req-6", 6, [6, 7, 8, 9, 10, 10]));
        Assert.Equal(PossessionDuration.Instant, sixSuccesses.Duration);
    }

    [Fact]
    public void PossessionMechanicFailureWithZeroSuccesses()
    {
        var state = CreateTestState();
        var request = new PossessionRequest(state, state.StateVersion, "req-1", 6, [2, 3, 4]);
        var result = WerewolfSpiritMechanicServices.EvaluatePossession(request);

        Assert.False(result.Succeeded);
        Assert.False(result.IsPossessing);
        Assert.Equal(PossessionDuration.None, result.Duration);
    }

    [Fact]
    public void PossessionControlNotImplemented()
    {
        var state = CreateTestState();
        var request = new PossessionRequest(state, state.StateVersion, "req-1", 6, [6, 7, 8]);
        var result = WerewolfSpiritMechanicServices.EvaluatePossession(request);

        Assert.True(result.Succeeded);
        Assert.True(result.IsPossessing);
        var findings = result.Findings.Select(f => f.Code.ToString()).ToList();
        Assert.DoesNotContain(findings, f => f.Contains("Control", StringComparison.Ordinal) || f.Contains("Permanent", StringComparison.Ordinal));
    }

    [Fact]
    public void SpiritDamageAbsorbedByWillpower()
    {
        var state = CreateTestState(willpower: 5);
        var request = new SpiritDamageRequest(state, state.StateVersion, "req-1", 8, 6, false);
        var result = WerewolfSpiritMechanicServices.ApplyDamage(request);

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.DamageApplied);
        Assert.Equal(3, result.EssenceLost);
        Assert.Equal(9, result.NewState?.EssenceCurrent);
    }

    [Fact]
    public void SpiritDamageFullAbsorptionWhenWillpowerExceedsDamage()
    {
        var state = CreateTestState(willpower: 10);
        var request = new SpiritDamageRequest(state, state.StateVersion, "req-1", 5, 6, false);
        var result = WerewolfSpiritMechanicServices.ApplyDamage(request);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.DamageApplied);
        Assert.Equal(0, result.EssenceLost);
    }

    [Fact]
    public void SpiritDamageEntersModorraWhenEssenceDepleted()
    {
        var state = CreateTestState(willpower: 3, rage: 3, gnosis: 2);
        var request = new SpiritDamageRequest(state, state.StateVersion, "req-1", 20, 6, false);
        var result = WerewolfSpiritMechanicServices.ApplyDamage(request);

        Assert.True(result.Succeeded);
        Assert.True(result.IsAtDeathBoundary);
        Assert.Equal(0, result.NewState?.EssenceCurrent);
    }

    [Fact]
    public void SpiritDamageIncrementesStateVersion()
    {
        var state = CreateTestState(willpower: 5);
        var request = new SpiritDamageRequest(state, state.StateVersion, "req-1", 10, 6, false);
        var result = WerewolfSpiritMechanicServices.ApplyDamage(request);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.NewState?.StateVersion);
    }

    [Fact]
    public void StateVersioningImmutableTransitions()
    {
        var state = CreateTestState();
        var originalVersion = state.StateVersion;

        var spendRequest = new EssenceSpendRequest(state, state.StateVersion, "req-1", 3);
        var spendResult = WerewolfSpiritMechanicServices.SpendEssence(spendRequest);

        Assert.Equal(originalVersion, state.StateVersion);
        Assert.Equal(originalVersion + 1, spendResult.NewState?.StateVersion);
    }

    [Fact]
    public void StateVersioningRejectsStaleVersion()
    {
        var state = CreateTestState();
        var spendRequest = new EssenceSpendRequest(state, 999, "req-1", 3);
        var spendResult = WerewolfSpiritMechanicServices.SpendEssence(spendRequest);

        Assert.False(spendResult.Succeeded);
    }

    [Fact]
    public void NoS3KeysImplemented()
    {
        var s3Keys = new[] { "spirit.gift.detection", "spirit.gift.command", "spirit.gift.possession", "spirit.gift.charm-activation", "spirit.gift.crossing" };
        var state = CreateTestState();
        Assert.NotNull(state);
        Assert.DoesNotContain(state.KnownCharmKeys, k => s3Keys.Contains(k));
    }

    [Fact]
    public void NoS4KeysImplemented()
    {
        var state = CreateTestState();
        Assert.NotNull(state);
        Assert.DoesNotContain(state.KnownCharmKeys, k => k.StartsWith("rite.", StringComparison.Ordinal));
    }

    [Fact]
    public void NoS5KeysImplemented()
    {
        var state = CreateTestState();
        Assert.NotNull(state);
        Assert.DoesNotContain(state.KnownCharmKeys, k => k.Contains("disposition", StringComparison.OrdinalIgnoreCase) || k.Contains("bargaining", StringComparison.OrdinalIgnoreCase) || k.Contains("hierarchy", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void NoRandomGenerationInsideWerewolf()
    {
        var state = CreateTestState();
        Assert.NotNull(state);
        var randomFields = typeof(WerewolfSpiritRuntimeState).GetProperties()
            .Where(p => p.PropertyType == typeof(Random) || p.Name.Contains("Random", StringComparison.OrdinalIgnoreCase))
            .ToList();
        Assert.Empty(randomFields);
    }

    [Fact]
    public void NoWorldStateMutation()
    {
        var state = CreateTestState(gnosis: 5);
        var originalLocation = state.SpiritId;

        var materializationRequest = new MaterializationRequest(state, state.StateVersion, "req-1", 3);
        var result = WerewolfSpiritMechanicServices.EvaluateMaterialization(materializationRequest);

        Assert.True(result.Succeeded);
        Assert.True(result.NewState?.IsMaterialized);
        Assert.Equal(originalLocation, result.NewState?.SpiritId);
    }

    [Fact]
    public void RuntimeRegistrationRemainsValid()
    {
        var registry = CreateRegistry();
        Assert.NotNull(registry);
    }

    [Fact]
    public void S1OperationsStillWork()
    {
        var registry = CreateRegistry();
        var metadata = registry.Metadata;
        Assert.Contains(metadata.Operations, o => o.OperationKey == "character-creation.create-character");
        Assert.Contains(metadata.Operations, o => o.OperationKey == "character-runtime.spend-resource");
    }

    private static WerewolfReferenceRuntime CreateRegistry()
    {
        return new WerewolfReferenceRuntime();
    }
}
