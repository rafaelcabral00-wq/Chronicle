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
        "combat",
        "fixture-driven-tests",
        "generic-dice",
        "post-creation-character-operations"
    ];

    public static IReadOnlyList<string> DisabledCapabilities { get; } =
    [
        "additional-gift-purchase",
        "runtime-gift-execution",
        "progression",
        "rites",
        "umbra"
    ];

    public const string DefineActionTestOperation = "character-runtime.define-action-test";
    public const string InterpretActionRollOperation = "character-runtime.interpret-action-roll";
    public const string DefineExtendedTestOperation = "character-runtime.define-extended-test";
    public const string AdvanceExtendedTestOperation = "character-runtime.advance-extended-test";
    public const string DefineResistedTestOperation = "character-runtime.define-resisted-test";
    public const string InterpretResistedTestOperation = "character-runtime.interpret-resisted-test";
}
