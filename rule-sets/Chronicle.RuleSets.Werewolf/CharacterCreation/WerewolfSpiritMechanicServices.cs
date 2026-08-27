namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfSpiritMechanicServices
{
    public const string InitializeSpiritOperation = "spirit-umbra.initialize-spirit";
    public const string EvaluateCrossingOperation = "spirit-umbra.evaluate-crossing";
    public const string ComputeMovementSpeedOperation = "spirit-umbra.compute-movement-speed";
    public const string EvaluateDetectionOperation = "spirit-umbra.evaluate-detection";
    public const string EvaluateMaterializationOperation = "spirit-umbra.evaluate-materialization";
    public const string SpendEssenceOperation = "spirit-umbra.spend-essence";
    public const string ExecuteCharmOperation = "spirit-umbra.execute-charm";
    public const string EvaluateCommandOperation = "spirit-umbra.evaluate-command";
    public const string EvaluatePossessionOperation = "spirit-umbra.evaluate-possession";
    public const string ApplySpiritDamageOperation = "spirit-umbra.apply-spirit-damage";

    private const int BaseUmbraMovementMeters = 20;
    private const int MinGauntlet = 2;
    private const int MaxGauntlet = 9;
    private const int MinTraitValue = 1;
    private const int MaxTraitValue = 10;

    public static SpiritMechanicResult Initialize(
        SpiritMechanicRequest request,
        string spiritId,
        string categoryKey,
        int willpowerPermanent,
        int ragePermanent,
        int gnosisPermanent,
        IReadOnlyList<string> knownCharmKeys)
    {
        var findings = new List<SpiritMechanicFinding>();

        if (string.IsNullOrWhiteSpace(spiritId))
        {
            return Invalid(SpiritMechanicErrorCode.InvalidSpiritId, "Spirit ID is required.", findings);
        }

        if (string.IsNullOrWhiteSpace(categoryKey))
        {
            return Invalid(SpiritMechanicErrorCode.InvalidCategory, "Spirit category is required.", findings);
        }

        if (willpowerPermanent < MinTraitValue || willpowerPermanent > MaxTraitValue)
        {
            return Invalid(SpiritMechanicErrorCode.InvalidTraitValue, $"Willpower permanent must be between {MinTraitValue} and {MaxTraitValue}.", findings);
        }

        if (ragePermanent < MinTraitValue || ragePermanent > MaxTraitValue)
        {
            return Invalid(SpiritMechanicErrorCode.InvalidTraitValue, $"Rage permanent must be between {MinTraitValue} and {MaxTraitValue}.", findings);
        }

        if (gnosisPermanent < MinTraitValue || gnosisPermanent > MaxTraitValue)
        {
            return Invalid(SpiritMechanicErrorCode.InvalidTraitValue, $"Gnosis permanent must be between {MinTraitValue} and {MaxTraitValue}.", findings);
        }

        var state = WerewolfSpiritRuntimeState.Create(
            spiritId,
            categoryKey,
            willpowerPermanent,
            ragePermanent,
            gnosisPermanent,
            knownCharmKeys);

        findings.Add(new SpiritMechanicFinding(
            SpiritMechanicFindingSeverity.Information,
            SpiritMechanicErrorCode.Succeeded,
            $"Spirit '{spiritId}' initialized with Essence {state.EssencePermanent}."));

        return new SpiritMechanicResult(
            true,
            state,
            findings.OrderBy(f => f.Code.ToString(), StringComparer.Ordinal).ToArray(),
            request.RequestId,
            state.StateVersion);
    }

    public static CrossingResult EvaluateCrossing(CrossingRequest request)
    {
        var findings = new List<SpiritMechanicFinding>();

        if (request.CurrentState is null)
        {
            return InvalidCrossing(SpiritMechanicErrorCode.MissingState, "Spirit state is required.", findings, request.ExpectedStateVersion);
        }

        if (request.GauntletValue < MinGauntlet || request.GauntletValue > MaxGauntlet)
        {
            return InvalidCrossing(SpiritMechanicErrorCode.InvalidGauntletValue, $"Gauntlet value must be between {MinGauntlet} and {MaxGauntlet}.", findings, request.CurrentState.StateVersion);
        }

        if (request.DiceValues is null || request.DiceValues.Count == 0)
        {
            return InvalidCrossing(SpiritMechanicErrorCode.InvalidDiceInput, "Dice values are required for crossing test.", findings, request.CurrentState.StateVersion);
        }

        if (request.ExpectedStateVersion != request.CurrentState.StateVersion)
        {
            return InvalidCrossing(SpiritMechanicErrorCode.StaleStateVersion, $"Expected state version {request.ExpectedStateVersion} does not match current version {request.CurrentState.StateVersion}.", findings, request.CurrentState.StateVersion);
        }

        if (request.IsFuryGrantedAction)
        {
            findings.Add(new SpiritMechanicFinding(
                SpiritMechanicFindingSeverity.Error,
                SpiritMechanicErrorCode.CrossingFuryRestricted,
                "Cannot step sideways using Fury-granted actions."));

            return new CrossingResult(
                false,
                request.CurrentState,
                findings.OrderBy(f => f.Code.ToString(), StringComparer.Ordinal).ToArray(),
                request.RequestId,
                request.CurrentState.StateVersion,
                0,
                false,
                false,
                true,
                CrossingTime.CannotRetry,
                request.GnosisPool,
                request.Difficulty,
                false,
                0);
        }

        var effectiveGnosis = Math.Max(0, request.GnosisPool - request.SilverItemCount);
        var effectiveDifficulty = request.Difficulty;
        if (request.HasReflectiveSurface)
        {
            effectiveDifficulty = Math.Max(1, effectiveDifficulty - 1);
        }

        var successes = CountSuccesses(request.DiceValues, effectiveDifficulty);
        var isBotch = IsBotch(request.DiceValues, effectiveDifficulty);

        if (successes == 0 && isBotch)
        {
            findings.Add(new SpiritMechanicFinding(
                SpiritMechanicFindingSeverity.Error,
                SpiritMechanicErrorCode.CrossingBotch,
                "Botch: spirit may get stuck in Pattern Web or disappear for hours."));

            return new CrossingResult(
                false,
                request.CurrentState,
                findings.OrderBy(f => f.Code.ToString(), StringComparer.Ordinal).ToArray(),
                request.RequestId,
                request.CurrentState.StateVersion,
                0,
                true,
                false,
                false,
                CrossingTime.CannotRetry,
                effectiveGnosis,
                effectiveDifficulty,
                false,
                0);
        }

        if (successes == 0)
        {
            findings.Add(new SpiritMechanicFinding(
                SpiritMechanicFindingSeverity.Information,
                SpiritMechanicErrorCode.CrossingZeroSuccessWait,
                "Zero successes: cannot retry same location for 1 hour."));

            return new CrossingResult(
                false,
                request.CurrentState,
                findings.OrderBy(f => f.Code.ToString(), StringComparer.Ordinal).ToArray(),
                request.RequestId,
                request.CurrentState.StateVersion,
                0,
                false,
                true,
                false,
                CrossingTime.CannotRetry,
                effectiveGnosis,
                effectiveDifficulty,
                false,
                0);
        }

        var crossingTime = successes switch
        {
            1 => CrossingTime.FiveMinutes,
            2 => CrossingTime.ThirtySeconds,
            _ => CrossingTime.Instant
        };

        var timeCode = successes switch
        {
            1 => SpiritMechanicErrorCode.CrossingFiveMinutes,
            2 => SpiritMechanicErrorCode.CrossingThirtySeconds,
            _ => SpiritMechanicErrorCode.CrossingInstant
        };

        findings.Add(new SpiritMechanicFinding(
            SpiritMechanicFindingSeverity.Information,
            timeCode,
            $"Crossing successful with {successes} success(es). Time: {crossingTime}."));

        return new CrossingResult(
            true,
            request.CurrentState,
            findings.OrderBy(f => f.Code.ToString(), StringComparer.Ordinal).ToArray(),
            request.RequestId,
            request.CurrentState.StateVersion,
            successes,
            false,
            false,
            false,
            crossingTime,
            effectiveGnosis,
            effectiveDifficulty,
            true,
            (request.PreviousFailedAttempts + 1) * 2);
    }

    public static MovementResult ComputeMovementSpeed(MovementRequest request)
    {
        var findings = new List<SpiritMechanicFinding>();

        if (request.CurrentState is null)
        {
            return InvalidMovement(SpiritMechanicErrorCode.MissingState, "Spirit state is required.", findings, request.ExpectedStateVersion);
        }

        var maxMeters = BaseUmbraMovementMeters + request.CurrentState.WillpowerCurrent;

        findings.Add(new SpiritMechanicFinding(
            SpiritMechanicFindingSeverity.Information,
            SpiritMechanicErrorCode.Succeeded,
            $"Maximum Umbra movement: {maxMeters} meters per turn."));

        return new MovementResult(
            true,
            request.CurrentState,
            findings.OrderBy(f => f.Code.ToString(), StringComparer.Ordinal).ToArray(),
            request.RequestId,
            request.CurrentState.StateVersion,
            maxMeters);
    }

    public static DetectionResult EvaluateDetection(DetectionRequest request)
    {
        var findings = new List<SpiritMechanicFinding>();

        if (request.CurrentState is null)
        {
            return InvalidDetection(SpiritMechanicErrorCode.MissingState, "Spirit state is required.", findings, request.ExpectedStateVersion);
        }

        if (request.DiceValues is null || request.DiceValues.Count == 0)
        {
            return InvalidDetection(SpiritMechanicErrorCode.InvalidDiceInput, "Dice values are required for detection test.", findings, request.CurrentState.StateVersion);
        }

        if (request.GnosisPool >= request.GauntletValue)
        {
            findings.Add(new SpiritMechanicFinding(
                SpiritMechanicFindingSeverity.Information,
                SpiritMechanicErrorCode.DetectionAutomatic,
                "Automatic detection: Gnosis pool >= Gauntlet value."));

            return new DetectionResult(
                true,
                request.CurrentState,
                findings.OrderBy(f => f.Code.ToString(), StringComparer.Ordinal).ToArray(),
                request.RequestId,
                request.CurrentState.StateVersion,
                true,
                true,
                0);
        }

        var successes = CountSuccesses(request.DiceValues, request.Difficulty);
        var isDetected = successes > 0;

        findings.Add(new SpiritMechanicFinding(
            isDetected ? SpiritMechanicFindingSeverity.Information : SpiritMechanicFindingSeverity.Error,
            isDetected ? SpiritMechanicErrorCode.DetectionSuccess : SpiritMechanicErrorCode.DetectionFailure,
            isDetected ? $"Detection successful with {successes} success(es)." : "Detection failed."));

        return new DetectionResult(
            isDetected,
            request.CurrentState,
            findings.OrderBy(f => f.Code.ToString(), StringComparer.Ordinal).ToArray(),
            request.RequestId,
            request.CurrentState.StateVersion,
            false,
            isDetected,
            successes);
    }

    public static MaterializationResult EvaluateMaterialization(MaterializationRequest request)
    {
        var findings = new List<SpiritMechanicFinding>();

        if (request.CurrentState is null)
        {
            return InvalidMaterialization(SpiritMechanicErrorCode.MissingState, "Spirit state is required.", findings, request.ExpectedStateVersion);
        }

        if (request.CurrentState.IsMaterialized)
        {
            findings.Add(new SpiritMechanicFinding(
                SpiritMechanicFindingSeverity.Information,
                SpiritMechanicErrorCode.Succeeded,
                "Spirit is already materialized."));

            return new MaterializationResult(
                true,
                request.CurrentState,
                findings.OrderBy(f => f.Code.ToString(), StringComparer.Ordinal).ToArray(),
                request.RequestId,
                request.CurrentState.StateVersion,
                true,
                true);
        }

        if (request.CurrentState.GnosisPermanent < request.GauntletValue)
        {
            findings.Add(new SpiritMechanicFinding(
                SpiritMechanicFindingSeverity.Error,
                SpiritMechanicErrorCode.MaterializationInsufficientGnosis,
                $"Materialization requires Gnosis >= Gauntlet ({request.GauntletValue}). Current Gnosis: {request.CurrentState.GnosisPermanent}."));

            return new MaterializationResult(
                false,
                request.CurrentState,
                findings.OrderBy(f => f.Code.ToString(), StringComparer.Ordinal).ToArray(),
                request.RequestId,
                request.CurrentState.StateVersion,
                false,
                false);
        }

        var newState = request.CurrentState with { IsMaterialized = true, StateVersion = request.CurrentState.StateVersion + 1 };

        findings.Add(new SpiritMechanicFinding(
            SpiritMechanicFindingSeverity.Information,
            SpiritMechanicErrorCode.MaterializationSuccess,
            "Spirit materialized successfully. Adopts physical health levels (usually 7)."));

        return new MaterializationResult(
            true,
            newState,
            findings.OrderBy(f => f.Code.ToString(), StringComparer.Ordinal).ToArray(),
            request.RequestId,
            newState.StateVersion,
            true,
            true);
    }

    public static EssenceSpendResult SpendEssence(EssenceSpendRequest request)
    {
        var findings = new List<SpiritMechanicFinding>();

        if (request.CurrentState is null)
        {
            return InvalidEssenceSpend(SpiritMechanicErrorCode.MissingState, "Spirit state is required.", findings, request.ExpectedStateVersion);
        }

        if (request.ExpectedStateVersion != request.CurrentState.StateVersion)
        {
            return InvalidEssenceSpend(SpiritMechanicErrorCode.StaleStateVersion, $"Expected state version {request.ExpectedStateVersion} does not match current version {request.CurrentState.StateVersion}.", findings, request.CurrentState.StateVersion);
        }

        if (request.Amount <= 0)
        {
            return InvalidEssenceSpend(SpiritMechanicErrorCode.InvalidRequest, "Essence spend amount must be positive.", findings, request.CurrentState.StateVersion);
        }

        if (request.CurrentState.EssenceCurrent < request.Amount)
        {
            findings.Add(new SpiritMechanicFinding(
                SpiritMechanicFindingSeverity.Error,
                SpiritMechanicErrorCode.EssenceInsufficient,
                $"Cannot spend {request.Amount} Essence: current is {request.CurrentState.EssenceCurrent}."));

            return new EssenceSpendResult(
                false,
                request.CurrentState,
                findings.OrderBy(f => f.Code.ToString(), StringComparer.Ordinal).ToArray(),
                request.RequestId,
                request.CurrentState.StateVersion,
                request.CurrentState.EssenceCurrent,
                request.CurrentState.EssenceCurrent);
        }

        var previousEssence = request.CurrentState.EssenceCurrent;
        var newEssence = previousEssence - request.Amount;

        var newState = request.CurrentState with { EssenceCurrent = newEssence, StateVersion = request.CurrentState.StateVersion + 1 };

        findings.Add(new SpiritMechanicFinding(
            SpiritMechanicFindingSeverity.Information,
            SpiritMechanicErrorCode.Succeeded,
            $"Spent {request.Amount} Essence: {previousEssence} -> {newEssence}."));

        return new EssenceSpendResult(
            true,
            newState,
            findings.OrderBy(f => f.Code.ToString(), StringComparer.Ordinal).ToArray(),
            request.RequestId,
            newState.StateVersion,
            previousEssence,
            newEssence);
    }

    public static CharmExecutionResult ExecuteCharm(CharmExecutionRequest request)
    {
        var findings = new List<SpiritMechanicFinding>();

        if (request.CurrentState is null)
        {
            return InvalidCharmExecution(SpiritMechanicErrorCode.MissingState, "Spirit state is required.", findings, request.ExpectedStateVersion);
        }

        if (string.IsNullOrWhiteSpace(request.CharmKey))
        {
            return InvalidCharmExecution(SpiritMechanicErrorCode.InvalidCharmKey, "Charm key is required.", findings, request.CurrentState.StateVersion);
        }

        var charm = WerewolfSpiritCharmCatalog.Get(request.CharmKey);
        if (charm is null)
        {
            return InvalidCharmExecution(SpiritMechanicErrorCode.InvalidCharmKey, $"Unknown charm: '{request.CharmKey}'.", findings, request.CurrentState.StateVersion);
        }

        if (!request.CurrentState.KnownCharmKeys.Contains(request.CharmKey, StringComparer.Ordinal))
        {
            findings.Add(new SpiritMechanicFinding(
                SpiritMechanicFindingSeverity.Error,
                SpiritMechanicErrorCode.CharmNotKnown,
                $"Spirit does not know charm '{request.CharmKey}'."));

            return new CharmExecutionResult(
                false,
                request.CurrentState,
                findings.OrderBy(f => f.Code.ToString(), StringComparer.Ordinal).ToArray(),
                request.RequestId,
                request.CurrentState.StateVersion,
                null,
                null);
        }

        var stateAfterCosts = request.CurrentState;
        if (request.GnosisCost.HasValue && request.GnosisCost.Value > 0)
        {
            if (stateAfterCosts.GnosisCurrent < request.GnosisCost.Value)
            {
                return InvalidCharmExecution(SpiritMechanicErrorCode.CharmExecutionFailure, $"Insufficient Gnosis for charm '{request.CharmKey}'.", findings, stateAfterCosts.StateVersion);
            }
            stateAfterCosts = stateAfterCosts with { GnosisCurrent = stateAfterCosts.GnosisCurrent - request.GnosisCost.Value };
        }

        if (request.EssenceCost.HasValue && request.EssenceCost.Value > 0)
        {
            if (stateAfterCosts.EssenceCurrent < request.EssenceCost.Value)
            {
                return InvalidCharmExecution(SpiritMechanicErrorCode.CharmExecutionFailure, $"Insufficient Essence for charm '{request.CharmKey}'.", findings, stateAfterCosts.StateVersion);
            }
            stateAfterCosts = stateAfterCosts with { EssenceCurrent = stateAfterCosts.EssenceCurrent - request.EssenceCost.Value };
        }

        var newState = stateAfterCosts with { StateVersion = stateAfterCosts.StateVersion + 1 };

        findings.Add(new SpiritMechanicFinding(
            SpiritMechanicFindingSeverity.Information,
            SpiritMechanicErrorCode.CharmExecutionSuccess,
            $"Charm '{charm.CanonicalName}' executed successfully."));

        return new CharmExecutionResult(
            true,
            newState,
            findings.OrderBy(f => f.Code.ToString(), StringComparer.Ordinal).ToArray(),
            request.RequestId,
            newState.StateVersion,
            request.CharmKey,
            charm.EffectSummary);
    }

    public static CommandResult EvaluateCommand(CommandRequest request)
    {
        var findings = new List<SpiritMechanicFinding>();

        if (request.CurrentState is null)
        {
            return InvalidCommand(SpiritMechanicErrorCode.MissingState, "Spirit state is required.", findings, request.ExpectedStateVersion);
        }

        if (request.DiceValues is null || request.DiceValues.Count == 0)
        {
            return InvalidCommand(SpiritMechanicErrorCode.InvalidDiceInput, "Dice values are required for command test.", findings, request.CurrentState.StateVersion);
        }

        var pool = request.Charisma + request.Leadership;
        var successes = CountSuccesses(request.DiceValues, request.TargetWillpower);
        var isCommanded = successes > 0;

        findings.Add(new SpiritMechanicFinding(
            isCommanded ? SpiritMechanicFindingSeverity.Information : SpiritMechanicFindingSeverity.Error,
            isCommanded ? SpiritMechanicErrorCode.CommandSuccess : SpiritMechanicErrorCode.CommandFailure,
            isCommanded ? $"Command successful with {successes} success(es)." : "Command failed."));

        return new CommandResult(
            isCommanded,
            request.CurrentState,
            findings.OrderBy(f => f.Code.ToString(), StringComparer.Ordinal).ToArray(),
            request.RequestId,
            request.CurrentState.StateVersion,
            successes,
            isCommanded);
    }

    public static PossessionResult EvaluatePossession(PossessionRequest request)
    {
        var findings = new List<SpiritMechanicFinding>();

        if (request.CurrentState is null)
        {
            return InvalidPossession(SpiritMechanicErrorCode.MissingState, "Spirit state is required.", findings, request.ExpectedStateVersion);
        }

        if (request.DiceValues is null || request.DiceValues.Count == 0)
        {
            return InvalidPossession(SpiritMechanicErrorCode.InvalidDiceInput, "Dice values are required for possession test.", findings, request.CurrentState.StateVersion);
        }

        var successes = CountSuccesses(request.DiceValues, request.TargetWillpower);
        var isPossessing = successes > 0;

        var duration = successes switch
        {
            0 => PossessionDuration.None,
            1 => PossessionDuration.SixHours,
            2 => PossessionDuration.ThreeHours,
            3 => PossessionDuration.OneHour,
            4 => PossessionDuration.FifteenMinutes,
            5 => PossessionDuration.FiveMinutes,
            _ => PossessionDuration.Instant
        };

        findings.Add(new SpiritMechanicFinding(
            isPossessing ? SpiritMechanicFindingSeverity.Information : SpiritMechanicFindingSeverity.Error,
            isPossessing ? SpiritMechanicErrorCode.PossessionSuccess : SpiritMechanicErrorCode.PossessionFailure,
            isPossessing ? $"Possession successful. Duration: {duration}." : "Possession failed."));

        return new PossessionResult(
            isPossessing,
            request.CurrentState,
            findings.OrderBy(f => f.Code.ToString(), StringComparer.Ordinal).ToArray(),
            request.RequestId,
            request.CurrentState.StateVersion,
            successes,
            isPossessing,
            duration);
    }

    public static SpiritDamageResult ApplyDamage(SpiritDamageRequest request)
    {
        var findings = new List<SpiritMechanicFinding>();

        if (request.CurrentState is null)
        {
            return InvalidDamage(SpiritMechanicErrorCode.MissingState, "Spirit state is required.", findings, request.ExpectedStateVersion);
        }

        if (request.DamageAmount <= 0)
        {
            return InvalidDamage(SpiritMechanicErrorCode.InvalidRequest, "Damage amount must be positive.", findings, request.CurrentState.StateVersion);
        }

        var absorptionRolls = Math.Max(0, request.CurrentState.WillpowerCurrent);
        var damageApplied = Math.Max(0, request.DamageAmount - absorptionRolls);
        var essenceLost = damageApplied;

        var newEssence = Math.Max(0, request.CurrentState.EssenceCurrent - essenceLost);
        var isAtDeathBoundary = newEssence == 0;

        var newState = request.CurrentState with
        {
            EssenceCurrent = newEssence,
            StateVersion = request.CurrentState.StateVersion + 1
        };

        findings.Add(new SpiritMechanicFinding(
            SpiritMechanicFindingSeverity.Information,
            SpiritMechanicErrorCode.DamageApplied,
            $"Damage applied: {damageApplied}. Essence lost: {essenceLost}. Current Essence: {newEssence}."));

        if (isAtDeathBoundary)
        {
            findings.Add(new SpiritMechanicFinding(
                SpiritMechanicFindingSeverity.Information,
                SpiritMechanicErrorCode.EssenceDepleted,
                "Essence depleted. Spirit reaches death/Modorra/destruction boundary (S5 threshold unresolved)."));
        }

        return new SpiritDamageResult(
            true,
            newState,
            findings.OrderBy(f => f.Code.ToString(), StringComparer.Ordinal).ToArray(),
            request.RequestId,
            newState.StateVersion,
            damageApplied,
            essenceLost,
            isAtDeathBoundary);
    }

    private static int CountSuccesses(IReadOnlyList<int> diceValues, int difficulty)
    {
        var successes = 0;
        foreach (var die in diceValues)
        {
            if (die >= difficulty)
            {
                successes++;
            }
        }
        return successes;
    }

    private static bool IsBotch(IReadOnlyList<int> diceValues, int difficulty)
    {
        foreach (var die in diceValues)
        {
            if (die == 1)
            {
                return true;
            }
        }
        return false;
    }

    private static SpiritMechanicResult Invalid(SpiritMechanicErrorCode code, string message, List<SpiritMechanicFinding> findings)
    {
        findings.Add(new SpiritMechanicFinding(SpiritMechanicFindingSeverity.Error, code, message));
        return new SpiritMechanicResult(false, null, findings.ToArray(), null, null);
    }

    private static CrossingResult InvalidCrossing(SpiritMechanicErrorCode code, string message, List<SpiritMechanicFinding> findings, int stateVersion)
    {
        findings.Add(new SpiritMechanicFinding(SpiritMechanicFindingSeverity.Error, code, message));
        return new CrossingResult(false, null, findings.ToArray(), null, stateVersion, 0, false, false, false, CrossingTime.CannotRetry, 0, 0, false, 0);
    }

    private static MovementResult InvalidMovement(SpiritMechanicErrorCode code, string message, List<SpiritMechanicFinding> findings, int stateVersion)
    {
        findings.Add(new SpiritMechanicFinding(SpiritMechanicFindingSeverity.Error, code, message));
        return new MovementResult(false, null, findings.ToArray(), null, stateVersion, 0);
    }

    private static DetectionResult InvalidDetection(SpiritMechanicErrorCode code, string message, List<SpiritMechanicFinding> findings, int stateVersion)
    {
        findings.Add(new SpiritMechanicFinding(SpiritMechanicFindingSeverity.Error, code, message));
        return new DetectionResult(false, null, findings.ToArray(), null, stateVersion, false, false, 0);
    }

    private static MaterializationResult InvalidMaterialization(SpiritMechanicErrorCode code, string message, List<SpiritMechanicFinding> findings, int stateVersion)
    {
        findings.Add(new SpiritMechanicFinding(SpiritMechanicFindingSeverity.Error, code, message));
        return new MaterializationResult(false, null, findings.ToArray(), null, stateVersion, false, false);
    }

    private static EssenceSpendResult InvalidEssenceSpend(SpiritMechanicErrorCode code, string message, List<SpiritMechanicFinding> findings, int stateVersion)
    {
        findings.Add(new SpiritMechanicFinding(SpiritMechanicFindingSeverity.Error, code, message));
        return new EssenceSpendResult(false, null, findings.ToArray(), null, stateVersion, 0, 0);
    }

    private static CharmExecutionResult InvalidCharmExecution(SpiritMechanicErrorCode code, string message, List<SpiritMechanicFinding> findings, int stateVersion)
    {
        findings.Add(new SpiritMechanicFinding(SpiritMechanicFindingSeverity.Error, code, message));
        return new CharmExecutionResult(false, null, findings.ToArray(), null, stateVersion, null, null);
    }

    private static CommandResult InvalidCommand(SpiritMechanicErrorCode code, string message, List<SpiritMechanicFinding> findings, int stateVersion)
    {
        findings.Add(new SpiritMechanicFinding(SpiritMechanicFindingSeverity.Error, code, message));
        return new CommandResult(false, null, findings.ToArray(), null, stateVersion, 0, false);
    }

    private static PossessionResult InvalidPossession(SpiritMechanicErrorCode code, string message, List<SpiritMechanicFinding> findings, int stateVersion)
    {
        findings.Add(new SpiritMechanicFinding(SpiritMechanicFindingSeverity.Error, code, message));
        return new PossessionResult(false, null, findings.ToArray(), null, stateVersion, 0, false, PossessionDuration.None);
    }

    private static SpiritDamageResult InvalidDamage(SpiritMechanicErrorCode code, string message, List<SpiritMechanicFinding> findings, int stateVersion)
    {
        findings.Add(new SpiritMechanicFinding(SpiritMechanicFindingSeverity.Error, code, message));
        return new SpiritDamageResult(false, null, findings.ToArray(), null, stateVersion, 0, 0, false);
    }
}
