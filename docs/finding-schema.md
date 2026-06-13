# Finding Schema

Every final finding must be evidence-bound, judge-ruled, actionable, and measurable.

## Required Fields

- `id`
- `fingerprint`
- `title`
- `description`
- `raisedBy`
- `category`
- `severity`
- `status`
- `confidenceBand`
- `evidence`
- `impact`
- `recommendation`
- `verification`
- `counterarguments`
- `judgeVerdict`

## Severity

Allowed values:

- `critical`
- `high`
- `medium`
- `low`
- `info`

## Status

Allowed values:

- `accepted`
- `partially_accepted`
- `rejected`
- `needs_human_review`

Only these statuses appear in the main report:

- `accepted`
- `partially_accepted`
- `needs_human_review`

Rejected findings belong in verbose output or JSON appendices.

## Mandatory Invariants

An accepted or partially accepted finding must have:

- at least one evidence item;
- verified section evidence;
- verified or fuzzy-verified quote evidence;
- a recommendation;
- a verification step;
- a Judge verdict;
- a stable fingerprint.

A finding without verified evidence must not be accepted.
