using Chronicle.RuleSets.Abstractions.PackageSources;

namespace Chronicle.Tools.PackageValidator;

public static class PackageValidatorCommand
{
    public static int Run(string[] args, TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        if (args.Length != 1 || string.IsNullOrWhiteSpace(args[0]))
        {
            WriteUsage(error);
            return (int)PackageValidatorExitCode.InvalidInvocation;
        }

        try
        {
            var packageSourcePath = Path.GetFullPath(args[0]);
            var result = RuleSetPackageSourceValidator.Validate(packageSourcePath);

            WriteResult(output, packageSourcePath, result);

            return result.IsValid
                ? (int)PackageValidatorExitCode.ValidPackage
                : (int)PackageValidatorExitCode.ValidationFailure;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            error.WriteLine("Unexpected internal failure.");
            error.WriteLine(exception.GetType().Name);
            return (int)PackageValidatorExitCode.UnexpectedInternalFailure;
        }
    }

    private static void WriteUsage(TextWriter error)
    {
        error.WriteLine("Usage:");
        error.WriteLine("  Chronicle.Tools.PackageValidator <package-source-path>");
        error.WriteLine();
        error.WriteLine("Exit codes:");
        error.WriteLine("  0 valid package source");
        error.WriteLine("  1 validation findings");
        error.WriteLine("  2 invalid invocation");
        error.WriteLine("  3 unexpected internal failure");
    }

    private static void WriteResult(
        TextWriter output,
        string packageSourcePath,
        RuleSetPackageSourceValidationResult result)
    {
        output.WriteLine("Chronicle Rule Set Package Source Validation");
        output.WriteLine($"PackageSource: {packageSourcePath}");
        output.WriteLine($"Status: {(result.IsValid ? "valid" : "invalid")}");
        output.WriteLine($"Files: {result.FileInventory.Count}");
        output.WriteLine($"Findings: {result.Findings.Count}");

        foreach (var finding in result.Findings)
        {
            output.WriteLine($"{finding.Severity} {finding.Code} {finding.Path}");
            output.WriteLine($"  {finding.Message}");
        }

        output.WriteLine("Inventory:");
        foreach (var file in result.FileInventory)
        {
            output.WriteLine($"  {file}");
        }
    }
}
