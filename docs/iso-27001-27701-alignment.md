# ISO/IEC 27001:2022 and ISO/IEC 27701:2025 alignment

This document records repository-level evidence for the reusable CommandQ NuGet
packages. It supports an information security management system (ISMS) and
privacy information management system (PIMS); it does not make the repository,
packages or organization ISO certified. Certification requires organization-
defined scope, risk assessment, statement of applicability, operating evidence,
internal audit and independent certification audit.

Normative standards must be obtained from ISO. Public overviews are available for
[ISO/IEC 27001:2022](https://www.iso.org/standard/27001) and
[ISO/IEC 27701:2025](https://www.iso.org/standard/27701).

## Repository control evidence

| Control objective | Implementation and evidence |
| --- | --- |
| Secure development lifecycle | Architecture tests protect dependency direction, AOT behavior, logging ranges, cancellation and security/package conventions. Pull-request CI builds, tests and packages the complete solution. |
| Dependency and vulnerability management | Versions are centrally managed and committed lock files make restore reproducible. NuGet audit covers transitive dependencies and advisory warnings fail CI. Dependabot monitors NuGet and workflow dependencies. |
| Build and package integrity | `global.json` selects the SDK; builds are deterministic and include repository URL/commit provenance. Package validation, content inspection, symbol-package checks and SHA-256 manifests run before distribution. See `docs/package-release.md`. |
| Source and debugging transparency | Packages intentionally remain unobfuscated and ship separate portable symbols with Source Link metadata. No credential or security control relies on source secrecy. |
| Least functionality and boundaries | Only the five product-neutral libraries are packable. Tests and deployable/application projects are excluded; repository boundaries are documented in `AGENTS.md` and package READMEs. |
| Data minimization | Cache keys, SQL text, database parameter names/values and file-upload exception messages are excluded from library operational output. The libraries add no user tracking or autonomous data store. |
| Privacy by design | `docs/privacy-boundaries.md` identifies potential data paths and assigns purposes, retention, rights handling and controller/processor decisions to consuming systems. |
| Vulnerability response | `SECURITY.md` defines private reporting, affected-version identification and package incident handling. |
| Availability and correctness | Cancellation propagation, bounded task execution, provider behavior and deterministic time serialization are covered by unit and architecture tests. |

## Organizational controls still required

Repository controls must be combined with:

- ISMS/PIMS scope, risk method, treatment plan and statement of applicability;
- protected branches, reviewed changes, least-privilege repository/feed access and
  periodic access recertification;
- release approval, immutable feed/version policy, artifact retention and signing
  policy appropriate to the distribution boundary;
- supplier and dependency review, patch SLAs, vulnerability triage and coordinated
  consumer notification;
- controller/processor roles and records of processing in every consuming system;
- secure CI runners, secret/HSM management, backup/recovery, incident response,
  internal audit, management review and corrective actions;
- approved licensing and intellectual-property terms before external distribution.

Review this mapping at least annually and after material changes to package
distribution, build infrastructure, dependencies, logging or data-handling APIs.
