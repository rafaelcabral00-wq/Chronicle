# Security Policy

Chronicle has no supported production release at this time.

The repository is in bootstrap and active development. Please do not report suspected vulnerabilities through public issues, discussions, pull requests, or social channels.

## Reporting a Vulnerability

Private vulnerability reporting will be enabled after the public GitHub repository is created.

Until then, use this repository-owner-controlled placeholder:

```text
PROJECT_SECURITY_CONTACT_PLACEHOLDER
```

Do not include personal email addresses in repository files. The placeholder should be replaced with GitHub private vulnerability reporting or a project-owned security alias before public release.

## Supported Versions

| Version | Supported |
| --- | --- |
| No production release | No |

## What to Include

When private reporting is available, include:

- affected component or document;
- steps to reproduce;
- expected impact;
- whether credentials, private Campaign data, or protected Rule Set content may be involved;
- any safe proof-of-concept details.

Do not include real secrets, private Campaign content, proprietary sourcebook text, or personal data unless maintainers explicitly request a secure transfer path.

## Scope

Security-sensitive areas include:

- credential handling;
- encrypted local persistence;
- package loading and Rule Set boundaries;
- provider adapters;
- logging and diagnostics;
- build and release workflows;
- provenance and licensing checks;
- private Campaign data handling.

## Public Disclosure

Please allow maintainers time to investigate and coordinate before public disclosure. Public advisories and supported-version statements will be introduced after Chronicle has a release process.
