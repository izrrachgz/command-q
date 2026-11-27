---
name: verify-change
description: Select and run validation for command-q changes across .NET 10 and .NET 11. Use for targeted tests, implementation handoff, package inspection or reproducing relevant CI checks.
---

# Verify Change

Choose the nearest matching project and xUnit method filter using the [testing standard](../../../docs/standards/testing.md). Confirm the selection found tests on both target frameworks.

~~~powershell
.\scripts\test.ps1 -Project Cqrs.Test\Cqrs.Test.csproj -Filter "*RequestDispatcherTest*"
.\scripts\test.ps1 -Project Persistence.Test\Persistence.Test.csproj -Filter "*CommandRunnerCancellationTest*"
~~~

For a library or packaging change, follow the [quality workflow](../../../.github/workflows/quality.yml) for the locked audited Release build and run the matching Release tests with `-NoBuild`. Use the isolated output procedure in the testing standard if the sibling feed should be kept intact. Verify local metadata, links, shell guards, solution items and package README content for scaffold changes.

Use [$api-contract](../api-contract/SKILL.md) for changed consumer behavior, [$database-change](../database-change/SKILL.md) for provider-specific evidence, and [$package-release](../package-release/SKILL.md) for release preparation. Report target, filter, counts, test environment, package evidence and any unverified consumer or provider combination.
