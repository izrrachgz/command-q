---
name: review-change
description: Review command-q changes for correctness, compatibility, dependency boundaries and missing tests. Use for read-only review of a working tree, commit, branch or pull request.
---

# Review Change

Establish baseline and candidate, including staged, unstaged, relevant untracked files and commits awaiting publication when applicable. Follow changed behavior from the called public API through registration and the affected operation.

Review both target frameworks, downstream compatibility, cancellation and resource ownership, AOT and adapter boundaries, and provider differences as applicable. For packaging, inspect identity, dependencies, lock files, project README, symbols and source provenance. Use the [package contracts](../../../docs/standards/api-conventions.md) and [testing standard](../../../docs/standards/testing.md).

Keep the review read-only. Lead with concrete findings by impact, including the location, affected caller, precondition, consequence and focused verification. Report material gaps in consumer or runtime evidence.
