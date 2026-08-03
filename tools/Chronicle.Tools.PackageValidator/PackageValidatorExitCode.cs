namespace Chronicle.Tools.PackageValidator;

public enum PackageValidatorExitCode
{
    ValidPackage = 0,
    ValidationFailure = 1,
    InvalidInvocation = 2,
    UnexpectedInternalFailure = 3
}
