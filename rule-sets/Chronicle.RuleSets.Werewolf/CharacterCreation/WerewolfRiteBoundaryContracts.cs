namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfFetishCreationBoundaryPayload(
    string RiteKey,
    string SpiritReference,
    string FetishMaterialReference,
    int PermanentGnoseInvestment,
    int DifficultyModifier,
    string SourceLocator,
    string Note);

public sealed record WerewolfTotemBindingBoundaryPayload(
    string RiteKey,
    string TotemId,
    string PackId,
    IReadOnlyList<string> MemberRoster,
    int TotemAggregation,
    string SourceLocator,
    string Note);

public sealed record WerewolfSpiritSummonBoundaryPayload(
    string RiteKey,
    string SpiritKey,
    int GnosisCost,
    string WillpowerTestResult,
    string SourceLocator,
    string Note);

public sealed record WerewolfCommitmentBoundaryPayload(
    string RiteKey,
    string SpiritReference,
    string TargetObjectReference,
    string WillpowerTestResult,
    string SourceLocator,
    string Note);

public sealed record WerewolfAwakenSpiritsBoundaryPayload(
    string RiteKey,
    IReadOnlyList<string> TargetSpiritReferences,
    int FuryCost,
    string ExtendedTestRequirement,
    string SourceLocator,
    string Note);
