# Chronicle Documentation

This directory contains Chronicle's architectural record.

## Reading Order

1. [Project charter](rfcs/RFC-0000-Project-Charter.md)
2. [Constitution](rfcs/RFC-0003-Constitution.md)
3. [Architecture overview](rfcs/RFC-0005-Architecture-Overview.md)
4. [Repository topology ADR](adrs/ADR-0043-Repository-Topology-Project-Boundaries-and-Solution-Structure-v0.2.0.md)
5. [Rule Set package architecture](rfcs/RFC-0027-Rule-Set-Package-Architecture.md)
6. [Rule Set package artifact model and extraction contract](specs/SPEC-0001-rule-set-package-artifact-model-and-extraction-contract.md)
7. [Repository materialization plan](reviews/repository-materialization/repository-materialization-plan.md)

## Authority Rules

- ADRs record accepted or proposed architecture decisions.
- RFCs define product, domain, application, architecture, and contract intent.
- SPEC documents define normative detailed contracts.
- Reconciliation and review packages explain how documentation conflicts were audited and resolved.

When concrete repository topology conflicts with older examples, ADR-0043 v0.2.0 controls current paths and project names.

## Status and Supersession

Check document front matter for `status`, `version`, `supersedes`, `superseded_by`, and `depends_on`.

Do not silently apply a superseded decision. If authority is unclear, record an issue or decision request.

## Navigation

- [ADRs](adrs/README.md)
- [RFCs](rfcs/README.md)
- [Specifications](specs/README.md)
- [Rule Set documentation](rule-sets/README.md)
