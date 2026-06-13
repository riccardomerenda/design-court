# Testing and Code Review Guide

This guide describes how to verify Design Court before committing or opening a pull request.

## Required Commands

Run these commands from the repository root:

```powershell
dotnet build DesignCourt.slnx
dotnet test DesignCourt.slnx
dotnet run --project src/DesignCourt.Cli -- --version
dotnet run --project src/DesignCourt.Cli -- review samples/rfcs/payment-rfc-missing-rollback.md
```

## Functional Review Scenario

The seeded RFC at `samples/rfcs/payment-rfc-missing-rollback.md` contains a destructive migration without an explicit rollback or recovery procedure.

Expected behavior:

- `design-court --version` reports the current SemVer preview version;
- the CLI parses 5 sections;
- the local workflow produces 1 judged finding;
- the finding title is `Missing rollback strategy for database migration`;
- the finding status is `Accepted`;
- the finding severity is `High`;
- the evidence section is `database-migration-plan`;
- the evidence quote is machine-verified.

After running the CLI, inspect:

```powershell
Get-Content design-court-output/report.md
Get-Content design-court-output/findings.json
```

The report should include the accepted finding and the verified evidence quote.

## Clean Control Scenario

The clean control at `samples/clean-controls/payment-rfc-clean.md` includes rollback, retry, validation, and observability details.

Expected behavior:

- the functional test `RunAsync_produces_no_findings_for_clean_control` passes;
- the deterministic Operations Engineer Agent does not emit a false positive.

## Code Review Checklist

Before committing, review the change against these points:

- `DesignCourt.Core` does not reference agent frameworks, model providers, GitHub APIs, or concrete infrastructure.
- Agent output starts as `CandidateFinding`; only the Judge can produce `JudgedFinding`.
- Accepted findings have verified section evidence and verified or fuzzy-verified quote evidence.
- Reportable findings include a recommendation, verification step, and Judge verdict.
- Functional tests cover both a seeded defect and a clean control.
- Generated artifacts under `design-court-output/` are not committed.
- Public documentation matches the actual behavior of the CLI and tests.

## Current Test Coverage

- `DesignCourt.Core.Tests`: finding invariant validation.
- `DesignCourt.Parsing.Tests`: Markdown section parsing and quote verification.
- `DesignCourt.Agents.Tests`: end-to-end local review workflow on seeded and clean samples.
