# Contributing to Chronicle

Thank you for helping build Chronicle.

Chronicle is still in repository bootstrap. Contributions should preserve the accepted architecture and avoid materializing future phases before their task is opened.

## Contributor Expectations

- Follow the [Code of Conduct](CODE_OF_CONDUCT.md).
- Do not include secrets, private Campaign data, proprietary sourcebook text, or personal contact data.
- Keep technical identifiers in English.
- Keep Rule Set source material, package source, packaged artifacts, and installed artifacts distinct.
- Use DCO sign-off for commits when contributing to a protected branch workflow.

## Required Tools

- Git.
- .NET 10 SDK, not only the runtime.
- VS Code or another compatible editor.
- C# tooling for the editor.
- Python 3.12 or newer for prototype validation harnesses.

Recommended:

- PowerShell 7.
- VS Code C# Dev Kit.
- VS Code Python extension.
- GitHub integration through VS Code or another editor.

Optional:

- GitHub CLI.

Not currently required:

- Visual Studio.
- Docker.
- SQL Server.
- A manually installed SQLite server.
- Node.js for the Chronicle runtime.
- Cloud infrastructure.
- OpenAI credentials merely to build the repository.

## Verify Your Environment

Run these commands from the repository root:

```powershell
git --version
dotnet --version
python --version
```

For the current bootstrap, `dotnet --version` must resolve to:

```text
10.0.302
```

The SDK is pinned by [global.json](global.json). That file controls local and CI SDK selection. If your machine has another .NET SDK installed, install .NET SDK `10.0.302` or adjust your environment so the repository root resolves to the pinned SDK.

## Python Selection

Chronicle uses Python for documentation prototype validation harnesses.

Do not assume `python` and `py` resolve to the same interpreter. Check both when debugging local tool behavior:

```powershell
python --version
py --version
py -0p
```

Use Python 3.12 or newer for prototype validation work. If a harness later requires an exact version, that requirement should be documented in the harness or task package.

## Solution Verification

BOOT-001 creates an empty solution only. Verify it with:

```powershell
dotnet sln Chronicle.sln list
```

At this phase, there are no source projects, test projects, Rule Set source projects, tools projects, or GitHub workflows.

## GitHub Contribution Paths

You may contribute through either:

- VS Code Git and GitHub integration; or
- GitHub CLI, if you choose to install it.

GitHub CLI is not required for bootstrap.

Typical flow:

```powershell
git status
git checkout -b docs/your-change
git add .
git commit -s -m "Describe the change"
```

The `-s` flag adds a DCO sign-off line.

## Bootstrap Versus Future Runtime Requirements

Repository bootstrap requirements are intentionally small:

- root governance files;
- documentation indexes;
- pinned SDK configuration;
- empty solution;
- central build settings.

Future runtime/provider work may require additional tools, package validation commands, provider credentials, installer tools, or CI configuration. Those requirements should be introduced only by the task that needs them.

## Documentation Changes

When editing documentation:

- preserve ADR/RFC authority boundaries;
- add supersession or dependency metadata when required;
- do not silently change architecture decisions;
- record unresolved contradictions as issues instead of guessing;
- keep links relative and portable.

## Rule Set Contributions

Rule Set documentation prototypes are evidence. They are not executable package source by repository presence.

Before Rule Set package source is materialized, a contribution must preserve:

- source provenance;
- identity and fingerprints;
- explicit exclusions;
- validation evidence;
- localization status;
- security boundaries;
- reconciliation records.

## Security

Do not report vulnerabilities through public issues. See [SECURITY.md](SECURITY.md).
