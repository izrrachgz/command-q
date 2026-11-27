---
name: package-release
description: Assess or prepare a CommandQ NuGet package release and verify artifact contents, provenance, version compatibility and an explicitly authorized distribution.
---

# Package Release

Use the [secure package procedure](../../../docs/package-release.md) and [quality workflow](../../../.github/workflows/quality.yml). Establish the intended commit, version and destination before producing release evidence.

- Verify the locked, audited Release build and Release tests. Use the pack verification script to confirm all seven packages and their symbol packages, both target assets, repository commit metadata, README files and the SHA-256 manifest. Use isolated package output for local experiments.
- Verify that the package version and its consumer impact match the reviewed compatibility decision. Record source, SDK, package versions, manifest, expected consumer pins and any release gaps.
- Distribution requires authorization for the actual destination. Follow the approved feed and signing policy. Keep published versions immutable. If a shipped version is defective, follow the release procedure for consumer notice, a corrected version and any approved restriction of the old one.
