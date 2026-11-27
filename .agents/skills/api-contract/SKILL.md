---
name: api-contract
description: Verify compatibility of command-q package APIs, option and DI contracts, transport semantics or persisted formats with existing consumers on both target frameworks.
---

# API Contract

Use the [package contract standard](../../../docs/standards/api-conventions.md). Identify the baseline package and candidate, the changed API or behavior, and the actual consuming projects. Verify source compatibility, binary compatibility where relevant, and documented behavior.

- Compare public signatures, overloads, DI registration behavior, configuration defaults and dependency changes. Inspect package validation output for both target frameworks. A separate released baseline comparison may be needed.
- Exercise relevant consumer semantics, such as CQRS pipeline ordering, localization fallback, cancellation, RabbitMQ acknowledgements, or UTC precision when reading older data.
- For an authorized consumer check, use the candidate package from a temporary feed with a distinct test version or isolated NuGet cache as appropriate. Verify the resolved package and target before trusting the test. Keep deliberate pin changes and downstream edits in the requested scope.
- Update the library's published README when the external contract changes and document the package version and upgrade decision. Use [$package-release](../package-release/SKILL.md) for distribution readiness.
