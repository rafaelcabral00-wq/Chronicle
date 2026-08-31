namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public sealed record WerewolfSpiritLocationBoundaryPayload(
    string SpiritId,
    string RealmKey,
    string LayerKey,
    string GauntletReference,
    string LocationStateTransition,
    string ChronicleOrchestrationRequired,
    string SourceLocator,
    string Note);

public sealed record WerewolfGauntletLookupBoundaryPayload(
    string LocationCategoryKey,
    string LocationReference,
    int GauntletValue,
    int PelículaValue,
    string SourceLocator,
    string Note);

public sealed record WerewolfRealmTravelBoundaryPayload(
    string SpiritId,
    string OriginRealmKey,
    string DestinationRealmKey,
    string TravelPath,
    string EligibilityResult,
    string ChronicleOrchestrationRequired,
    string SourceLocator,
    string Note);

public sealed record WerewolfScenePresenceBoundaryPayload(
    string SpiritId,
    string SceneReference,
    string PresenceState,
    string ObservableState,
    string ChronicleOrchestrationRequired,
    string SourceLocator,
    string Note);

public sealed record WerewolfCaernPelículaBoundaryPayload(
    string CaernReference,
    int CaernLevel,
    int PelículaLevel,
    int MoonBridgeMaxDistanceKm,
    string SourceLocator,
    string Note);

public sealed record WerewolfPackTotemLinkBoundaryPayload(
    string PackId,
    string TotemId,
    string LinkState,
    string BenefitScope,
    string ChronicleOrchestrationRequired,
    string SourceLocator,
    string Note);

public sealed record WerewolfSharedTotemEffectsBoundaryPayload(
    string TotemId,
    IReadOnlyList<string> EffectKeys,
    string IntendedRecipients,
    string ApplicationScope,
    string ChronicleOrchestrationRequired,
    string SourceLocator,
    string Note);
