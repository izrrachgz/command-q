# CommandQ release readiness

Use the [secure package release procedure](../package-release.md) and [quality workflow](../../.github/workflows/quality.yml). The release unit is a set of versioned NuGet packages and symbols. Applications incorporate them into separately built and released hosts.

- Identify the source revision, SDK, tested targets, shared version and affected consumers. Connect the package report and SHA-256 manifest to that revision.
- Verify both target assets, matching portable symbols, project-specific README, license, repository reference and absence of unwanted package entries.
- Different bytes under the same version can produce inconsistent downstream builds. Keep experiments in separate output directories. For an authorized distribution, confirm the destination, publisher approval and applicable signing policy.
- For a defective published package, follow the release policy for a new corrected version, affected consumer pins and any approved restriction of the old version.
