# Architecture

Design Court is built around a strict domain boundary.

The core project owns the review contracts:

- findings;
- candidate findings;
- evidence;
- verification steps;
- Judge verdicts;
- reviewed documents;
- validation rules;
- workflow interfaces.

The core project must not depend on:

- OpenAI SDKs;
- Azure SDKs;
- Microsoft Agent Framework;
- GitHub SDKs;
- concrete file-system implementations.

## Dependency Direction

```text
DesignCourt.Core
  <- DesignCourt.Agents
  <- DesignCourt.Parsing
  <- DesignCourt.Reporting
  <- DesignCourt.Providers
  <- DesignCourt.Infrastructure.MicrosoftAgentFramework
  <- DesignCourt.GitHub
  <- DesignCourt.Cli
```

This keeps the product testable, portable, and measurable.

## Current Skeleton

The repository currently includes:

- `DesignCourt.Core` for contracts and invariants.
- `DesignCourt.Agents` for deterministic review agents, Judge logic, and local workflow orchestration.
- `DesignCourt.Parsing` for Markdown section addressing and evidence verification.
- `DesignCourt.Reporting` for Markdown report rendering.
- `DesignCourt.Cli` for the local command entrypoint.

The next implementation slice should add the evaluation command and metric reporting before introducing real LLM provider infrastructure.
