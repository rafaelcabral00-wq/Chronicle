using System.Text.Json;

namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public static class WerewolfRiteExecutionService
{
    public static WerewolfRiteExecutionResult Execute(WerewolfRiteExecutionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var findings = new List<WerewolfRiteFinding>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            findings.Add(new WerewolfRiteFinding("InvalidRequestId", "RequestId is required.", WerewolfRiteFindingSeverity.Error));
            return new WerewolfRiteExecutionResult(false, findings, string.Empty, string.Empty, 0, 0, 0, string.Empty, null, null);
        }

        if (string.IsNullOrWhiteSpace(request.RiteKey))
        {
            findings.Add(new WerewolfRiteFinding("InvalidRiteKey", "RiteKey is required.", WerewolfRiteFindingSeverity.Error));
            return new WerewolfRiteExecutionResult(false, findings, request.RequestId, string.Empty, 0, 0, 0, string.Empty, null, null);
        }

        var definition = WerewolfRiteCatalog.Get(request.RiteKey);
        if (definition is null)
        {
            findings.Add(new WerewolfRiteFinding("UnknownRite", $"Unknown rite: {request.RiteKey}", WerewolfRiteFindingSeverity.Error));
            return new WerewolfRiteExecutionResult(false, findings, request.RequestId, request.RiteKey, 0, 0, 0, string.Empty, null, null);
        }

        if (request.DiceValues is null || request.DiceValues.Count == 0)
        {
            findings.Add(new WerewolfRiteFinding("InvalidDiceValues", "DiceValues is required and must not be empty.", WerewolfRiteFindingSeverity.Error));
            return new WerewolfRiteExecutionResult(false, findings, request.RequestId, request.RiteKey, 0, definition.BaseDifficulty, 0, string.Empty, null, null);
        }

        if (definition.BaseDifficulty is null)
        {
            findings.Add(new WerewolfRiteFinding("UnspecifiedDifficulty", "BaseDifficulty is not defined for this Rite.", WerewolfRiteFindingSeverity.Error));
            var unresolvedPayload = CreateBoundaryPayload(definition.Key, 0);
            return new WerewolfRiteExecutionResult(false, findings, request.RequestId, request.RiteKey, request.DiceValues.Count, null, 0, "UnspecifiedDifficulty", null, unresolvedPayload);
        }

        var difficulty = definition.BaseDifficulty.Value;
        if (request.HasTargetPiece)
        {
            difficulty = Math.Max(2, difficulty - 1);
            findings.Add(new WerewolfRiteFinding("TargetPieceModifier", "Target piece possessed: difficulty reduced by 1.", WerewolfRiteFindingSeverity.Information));
        }

        if (difficulty < 2 || difficulty > 10)
        {
            findings.Add(new WerewolfRiteFinding("InvalidDifficulty", $"Difficulty {difficulty} is out of bounds (2-10).", WerewolfRiteFindingSeverity.Error));
            return new WerewolfRiteExecutionResult(false, findings, request.RequestId, request.RiteKey, request.DiceValues.Count, definition.BaseDifficulty, 0, string.Empty, null, null);
        }

        foreach (var die in request.DiceValues)
        {
            if (die < 1 || die > 10)
            {
                findings.Add(new WerewolfRiteFinding("InvalidDieFace", $"Die face {die} is out of bounds for d10.", WerewolfRiteFindingSeverity.Error));
                return new WerewolfRiteExecutionResult(false, findings, request.RequestId, request.RiteKey, request.DiceValues.Count, definition.BaseDifficulty, 0, string.Empty, null, null);
            }
        }

        var interpretation = WerewolfActionRollInterpretationService.Interpret(
            new WerewolfActionRollInterpretationRequest(
                request.RequestId,
                request.DiceValues,
                difficulty,
                request.DiceValues.Count));

        findings.AddRange(interpretation.Findings.Select(f => new WerewolfRiteFinding(
            f.Code,
            f.Message,
            f.Severity == WerewolfActionRollInterpretationFindingSeverity.Error
                ? WerewolfRiteFindingSeverity.Error
                : WerewolfRiteFindingSeverity.Information)));

        if (!interpretation.Succeeded)
        {
            return new WerewolfRiteExecutionResult(
                false,
                findings,
                request.RequestId,
                request.RiteKey,
                request.DiceValues.Count,
                difficulty,
                0,
                interpretation.InterpretationStatus,
                null,
                null);
        }

        var successCount = interpretation.SuccessCount ?? 0;
        var effect = successCount > 0
            ? definition.EffectDescription
            : "No information gained.";

        var payload = CreateBoundaryPayload(definition.Key, successCount);

        var serialized = JsonSerializer.Serialize(
            new
            {
                request.RequestId,
                definition.Key,
                definition.DisplayName,
                definition.Category,
                definition.Level,
                definition.AttributeId,
                definition.AbilityId,
                BaseDifficulty = definition.BaseDifficulty,
                EffectiveDifficulty = difficulty,
                DicePool = request.DiceValues.Count,
                SuccessCount = successCount,
                InterpretationStatus = interpretation.InterpretationStatus,
                Effect = effect,
                HasTargetPiece = request.HasTargetPiece,
                Findings = findings.Select(f => new { f.Code, f.Message, f.Severity }).ToArray()
            });

        return new WerewolfRiteExecutionResult(
            true,
            findings,
            request.RequestId,
            request.RiteKey,
            request.DiceValues.Count,
            difficulty,
            successCount,
            interpretation.InterpretationStatus,
            effect,
            payload);
    }

    private static object? CreateBoundaryPayload(string riteKey, int successCount)
    {
        return riteKey switch
        {
            WerewolfRiteIdentifiers.Fetish => new WerewolfFetishCreationBoundaryPayload(
                RiteKey: riteKey,
                SpiritReference: "SpiritId from Chronicle",
                FetishMaterialReference: "FetishMaterialId from Chronicle",
                PermanentGnoseInvestment: 0,
                DifficultyModifier: 0,
                SourceLocator: "Lines 2690, 3466-3469",
                Note: "S4 represents this as a typed boundary. Fetish world-item creation/persistence is deferred to Chronicle."),

            WerewolfRiteIdentifiers.Totem => new WerewolfTotemBindingBoundaryPayload(
                RiteKey: riteKey,
                TotemId: "TotemId from Chronicle",
                PackId: "PackId from Chronicle",
                MemberRoster: [],
                TotemAggregation: 0,
                SourceLocator: "Line 2693",
                Note: "S4 represents this as a typed boundary. Pack/Totem binding lifecycle is deferred to S5."),

            WerewolfRiteIdentifiers.Summoning => new WerewolfSpiritSummonBoundaryPayload(
                RiteKey: riteKey,
                SpiritKey: "SpiritKey from Chronicle",
                GnosisCost: 1,
                WillpowerTestResult: "Net successes from opposed test",
                SourceLocator: "Line 2681",
                Note: "S4 represents this as a typed boundary. Spirit appearance in Chronicle scene/world is deferred to S5."),

            WerewolfRiteIdentifiers.Commitment => new WerewolfCommitmentBoundaryPayload(
                RiteKey: riteKey,
                SpiritReference: "SpiritId from Chronicle",
                TargetObjectReference: "ObjectId from Chronicle",
                WillpowerTestResult: "Net successes from resisted Willpower vs spirit Gnose test",
                SourceLocator: "Line 2666",
                Note: "S4 represents this as a typed boundary. Amulet/fetish world-item creation/persistence is deferred to Chronicle."),

            WerewolfRiteIdentifiers.AwakenSpirits => new WerewolfAwakenSpiritsBoundaryPayload(
                RiteKey: riteKey,
                TargetSpiritReferences: [],
                FuryCost: 1,
                ExtendedTestRequirement: "Extended Gnose test",
                SourceLocator: "Line 2678",
                Note: "S4 represents this as a typed boundary. Spirit property awakening/state mutation is deferred to Chronicle/S5."),

            WerewolfRiteIdentifiers.CaernOpening => new WerewolfCaernOpeningBoundaryPayload(
                RiteKey: riteKey,
                CaernReference: "CaernId from Chronicle",
                SeptReference: "SeptId from Chronicle",
                ParticipantRoster: [],
                TestType: "Extended resisted Raciocínio + Rituais",
                OpposedSpirit: "Caern spirit",
                RequiredSuccesses: 0,
                SourceLocator: "Line 2586",
                Note: "Wave C represents this as a typed boundary. A-010b: source contains conflicting success thresholds (Willpower vs Caern level). Exact required successes are a Human Decision. Caern world-state creation/opening is deferred to Chronicle."),

            WerewolfRiteIdentifiers.CaernCreation => new WerewolfCaernCreationBoundaryPayload(
                RiteKey: riteKey,
                CaernReference: "CaernId from Chronicle",
                SeptReference: "SeptId from Chronicle",
                ParticipantRoster: [],
                BaseDifficulty: 8,
                ParticipantGroupCount: 0,
                DifficultyReduction: 0,
                RequiredSuccesses: 40,
                HourlyTestInterval: 1,
                PermanentGnoseCost: true,
                SourceLocator: "Line 2600",
                Note: "Wave C represents this as a typed boundary. A-010c: exact difficulty reduction formula per group of 5 extra participants is unresolved. Chronicle must compute difficulty from participant count. Permanent Gnose cost and Caern world-state creation are deferred to Chronicle."),

            WerewolfRiteIdentifiers.Purification => new WerewolfPurificationBoundaryPayload(
                RiteKey: riteKey,
                TargetReference: "TargetId from Chronicle",
                TargetType: "entity-or-place",
                CorruptionType: "Unknown from source",
                CleansingResult: "Net successes from Carisma + Rituais test",
                SourceLocator: "Line 2614",
                Note: "Wave D represents this as a typed boundary. Exact corruption/cleansing mechanics are not explicitly defined in source. Chronicle owns entity/place state mutation."),

            WerewolfRiteIdentifiers.Contrition => new WerewolfContritionBoundaryPayload(
                RiteKey: riteKey,
                TotemId: "TotemId from Chronicle",
                PackId: "PackId from Chronicle",
                DogmaViolationState: "Unknown from source",
                RelationshipResult: "Net successes from Carisma + Rituais test",
                SourceLocator: "Line 2617",
                Note: "Wave D represents this as a typed boundary. Existing WerewolfTotemDefinitions contains RitualOfContrition (rite.totem.ritual-of-contrition) — known key collision with audit stable key rite.pact.contrition. Totem relationship repair is deferred to Chronicle/S5."),

            WerewolfRiteIdentifiers.FireBaptism => new WerewolfFireBaptismBoundaryPayload(
                RiteKey: riteKey,
                TargetReference: "TargetId from Chronicle",
                TargetType: "entity",
                SpiritAttendanceResult: "Unknown from source",
                AncestorGuideResult: "Unknown from source",
                SourceLocator: "Line 2663",
                Note: "Wave D represents this as a typed boundary. Exact target and effect mechanics are not explicitly defined in source. Spirit attendance/party and ancestor/spirit guide are soft dependencies. Chronicle owns persistent spiritual relationship/status."),

            WerewolfRiteIdentifiers.Initiation => new WerewolfInitiationBoundaryPayload(
                RiteKey: riteKey,
                InitiateReference: "CharacterId from Chronicle",
                UmbraAccessResult: "Net successes from Raciocínio + Rituais test",
                SourceLocator: "Line 2675",
                Note: "Wave D represents this as a typed boundary. Grants Umbra access. Umbra traversal/materialization is a hard Spirit/Umbra dependency. Chronicle/S2 owns Umbra access state mutation."),

            _ => null,
        };
    }
}
