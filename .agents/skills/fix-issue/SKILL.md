---
name: fix-issue
description: Reproduce and diagnose a command-q library defect reported by a consuming application or failing test. Use when the cause may involve package version, target framework, provider, registration or public behavior.
---

# Fix Issue

Record the affected package revision and resolved version, consuming framework, expected result and smallest synthetic reproduction. If the failure occurs only in a consuming application, identify the package boundary and compare its actual contract with the consumer's configuration, dependency graph and use case.

Find the first incorrect state in package registration, dispatch, provider selection, data conversion or transport as applicable. Reproduce the package-owned issue in its matching test project when practical. For a packaging or restore discrepancy, inspect selected assets and the version in the consumer's assets or lock file.

Carry the reproduction into the requested repair using [$ub-mode](../ub-mode/SKILL.md) and run [$verify-change](../verify-change/SKILL.md) for final evidence. Keep consumer edits in the requested scope.
