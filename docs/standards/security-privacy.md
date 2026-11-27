# CommandQ security and privacy standard

Use the [data-flow inventory](../privacy-boundaries.md), [ISO alignment evidence](../iso-27001-27701-alignment.md), and [private reporting policy](../../SECURITY.md).

- Trace caller-supplied data through libraries when changes affect caches, HTTP, messaging, database parameters, localization or diagnostics. Keep credentials, tokens, payloads and direct identifiers out of templates and fixtures. Provider exception details need appropriate consumer redaction and access controls.
- Keep query parameters bound and preserve transaction and session-context ownership. Consumers control connection access, row-level security policy and the classification and retention of data sent to a broker or other recipient.
- Keep test inputs synthetic. When findings contain private data or credentials, redact them before recording or sharing evidence.
- For dependency and release changes use the locked audited build and [package-release controls](../package-release.md). Organizational governance and service-level privacy obligations belong to the relevant consumer and distribution owner.
