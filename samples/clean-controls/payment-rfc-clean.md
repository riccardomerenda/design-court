# Payment Processing Migration RFC

## Context

The payment service stores legacy payment attempts in a table that is no longer aligned with the current event model.

## Database Migration Plan

The migration will copy data into the new schema, verify row counts, and keep the old columns until the post-deployment validation passes.

If validation fails, the deployment will roll back to the previous service version and keep reading from the old columns. The migration job is idempotent and can be safely retried after the failed deployment is investigated.

## Observability

The deployment dashboard tracks service health, request latency, failed payment attempts, migration duration, row-count mismatches, and rollback status.

## Success Criteria

The migration is complete when the new service version can create and read payment attempts, validation metrics match expected row counts, and rollback has not been triggered.
