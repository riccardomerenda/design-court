# Payment Processing Migration RFC

## Context

The payment service stores legacy payment attempts in a table that is no longer aligned with the current event model.

## Database Migration Plan

The migration will remove deprecated columns after data transformation.

The migration will run during the regular deployment window. The deployment pipeline will apply the schema change and then start the new payment service version.

## Observability

The deployment dashboard already tracks service health, request latency, and failed payment attempts.

## Success Criteria

The migration is complete when the new service version can create and read payment attempts without using deprecated columns.
