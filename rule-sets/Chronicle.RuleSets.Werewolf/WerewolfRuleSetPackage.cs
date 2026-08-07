namespace Chronicle.RuleSets.Werewolf;

public static class WerewolfRuleSetPackage
{
    public const string ProvisionalPackageId = "chronicle.rulesets.werewolf";
    public const string PackageIdStatus = "provisional-governance-pending";
    public const string PackageVersion = "0.1.0-source-skeleton";
    public const string DeclaredReleaseScope = "werewolf3e.character-creation.current-slice";
    public const string PrototypeReferencePath = "docs/rule-sets/Chronicle.RuleSets.Werewolf/prototype";

    public static IReadOnlyList<string> SupportedCapabilities { get; } =
    [
        "character-completion",
        "character-creation",
        "character-model",
        "character-sheet",
        "character-validation",
        "fixture-driven-tests",
        "generic-dice"
    ];

    public static IReadOnlyList<string> DisabledCapabilities { get; } =
    [
        "additional-gift-purchase",
        "runtime-gift-execution",
        "post-creation-character-operations",
        "combat",
        "progression",
        "rites",
        "umbra"
    ];

    public const string DefineActionTestOperation = "character-runtime.define-action-test";
    public const string InterpretActionRollOperation = "character-runtime.interpret-action-roll";
}
