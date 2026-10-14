# Cancellation token handling

Cancellation is a control-flow signal, not an ordinary failure result. Public asynchronous APIs that accept a
`CancellationToken` must propagate caller-requested cancellation by throwing `OperationCanceledException` (normally
`TaskCanceledException` for HTTP operations).

## Implementation rules

- Accept a token at the public async boundary and pass it through every cancellable operation in the call chain.
- Do not replace a received token with `CancellationToken.None`, a new unrelated token, or an omitted optional argument.
- When an operation has both an outer token and an item-specific token, create a linked token source and keep it alive
  until that item finishes.
- Catch `OperationCanceledException` before broad exception handlers. Log it if useful, then rethrow with `throw;` so
  the original exception and stack are preserved.
- Do not translate cancellation into an empty collection, `false`, an HTTP conflict response, or another ordinary
  result. Callers must be able to distinguish cancellation from a completed operation.
- Keep expected non-cancellation fallback behavior separate from cancellation handling.
- Add a deterministic regression test using an already-cancelled token or a controlled in-flight operation.

## Cleanup after cancellation

Cleanup may need to run after the caller token has already been cancelled. Use a short, independently bounded cleanup
token for operations such as rolling back a transaction. Preserve and rethrow the original cancellation even if cleanup
also fails. Never use an unbounded `CancellationToken.None` for I/O cleanup.

Caller-owned resources are different: a method receiving an `IDbTransaction` must not commit, roll back, or dispose it.
Propagate cancellation and leave the caller responsible for transaction cleanup.

## CommandQ contracts

- CQRS dispatch passes the token through every pipeline behavior and request handler.
- `TaskRunner` passes the caller token to cancellable task factories and propagates caller cancellation after active work
  observes it. Ordinary per-task failures remain logged and isolated.
- `HttpCall` and `FileUploader` propagate caller cancellation rather than returning fallback HTTP responses.
- Persistence repositories, session-context operations, command runners, and provider APIs pass tokens to database calls.
- `ICommandRunner.ExecuteCommands` links the batch token with each `QueryCommand.CancellationToken`; either token can
  cancel the command and the batch reports cancellation.
- Runner-owned bulk-insert transactions attempt rollback with a bounded cleanup token. Caller-owned transaction overloads
  never manipulate the supplied transaction.

## Review checklist

When adding or reviewing async code, search for token parameters, broad `catch` blocks, omitted token arguments,
`CancellationToken.None`, fire-and-forget tasks, and fallback return values. Run a fresh analyzer-enabled build with
`CA2016` treated as an error, then run the repository's release build and tests.
