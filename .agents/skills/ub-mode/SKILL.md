---
name: ub-mode
description: Implement command-q library behavior changes with test-first development and small verified steps. Use for features, behavior fixes and refactors of control flow, responsibilities or state handling.
---

# UB Mode

Adapted from Robert C. Martin's [UB Mode](https://github.com/unclebob/ub-mode/blob/master/SKILL.md), published under the [MIT license](https://github.com/unclebob/ub-mode/blob/master/LICENSE). Follow the [library code style](../../../docs/standards/code-style.md).

## Establish the behavior

- State what a consuming application should observe and which package owns the decision. Identify the public API, active framework target, and any relevant DI scope, caller-owned resource, or persisted format. For a defect, start from [$fix-issue](../fix-issue/SKILL.md).
- For changed behavior, run a focused failing test through the package's public entry point and confirm it fails for the intended reason. For a refactor, start with a passing characterization test. Reuse existing coverage when it proves the affected guarantee and exercise both target frameworks.
- When a database provider, broker, or downstream revision is unavailable, exercise the nearest runnable seam and record the evidence gap. If successive fixes fail the same way, verify the assumption they share, such as which assembly, package version, or DI registration the caller actually resolved.

## Implement and simplify

- Make one observable change at a time and rerun the focused test before and after simplifying. Keep product rules in consuming applications. Preserve explicit CQRS registration and scoped resolution, transport-neutral Messaging, and provider-specific behavior inside its adapter. Share implementation when callers depend on the same rule.
- For asynchronous or retryable work, trace who owns the token, transaction, and cleanup. Exercise a relevant interruption or duplicate operation and verify the caller's outcome. Use the [cancellation contract](../../../docs/cancellation.md) for exception propagation and bounded cleanup.
- Use [$api-contract](../api-contract/SKILL.md) when existing callers, wire values or package assets may observe a difference. Use [$database-change](../database-change/SKILL.md) for provider-specific execution, and [$package-release](../package-release/SKILL.md) for changes to the published package contract and its version.

## Check the affected guarantee

- **CQRS dispatch.** Verify pipeline order, scoped resolution and token propagation through the registered handler. When registration changes, check relevant missing or duplicate handler behavior through the public composition path. [Dispatcher tests](../../../Cqrs.Test/Dispatching/RequestDispatcherTest.cs)
- **Persistence cancellation.** Exercise the batch token and an individual command token independently. Where transaction ownership changes, verify cancellation reaches the provider and caller-owned transactions remain under the caller's control. [Command runner tests](../../../Persistence.Test/CommandRunnerCancellationTest.cs) and [transaction ownership](../../../Persistence/README.md#command-runners)
- **Time serialization.** Exercise supported UTC values and representative invalid input. Preserve the documented offset and precision rules and verify existing stored data with the affected consumer when changing the format. [UTC serialization tests](../../../Time.Test/Serialization/UtcJsonSerializerOptionsExtensionsTest.cs)

## Verify and report

Use [$verify-change](../verify-change/SKILL.md) and the [test standard](../../../docs/standards/testing.md). Report the failing reproduction or passing baseline, the behavior proved, target frameworks, package or consumer evidence, and any environment gaps.
