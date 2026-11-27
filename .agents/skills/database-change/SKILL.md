---
name: database-change
description: Verify changes to CommandQ.Persistence query construction, command runners, provider adapters, connection or transaction behavior, and session-context handling.
---

# Database Change

Read [Persistence's public contract](../../../Persistence/README.md) and the [cancellation guide](../../../docs/cancellation.md). Identify the affected providers, public caller and owner of the transaction or session context.

- Compare SQL, parameters, ordering, UTC precision and provider-specific limits. Preserve the same observable result for shared behavior and use provider-specific code where appropriate.
- For caller-owned transactions, retain the provider fast path and leave commit, rollback and disposal with the caller. For runner-owned transactions, verify cancellation propagation and bounded cleanup. For session context, check connection usage and restoration semantics where changed. Consumer applications own schema, migration and authorization policy decisions.
- Run affected [Persistence tests](../../../Persistence.Test/README.md). Provider-neutral construction tests establish compilation and API behavior. Use a disposable native database fixture for provider-specific SQL execution, bulk writes or session state. Report which engines were exercised.

Use [$verify-change](../verify-change/SKILL.md) for the wider checks and [$api-contract](../api-contract/SKILL.md) if callers depend on the changed behavior.
